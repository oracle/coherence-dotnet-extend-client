/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using Tangosol.Util;

namespace Tangosol.IO
{
    /// <summary>
    /// An IDeltaCompressor implementation that works with opaque (binary) values.
    /// </summary>
    /// <remarks>
    /// The delta format is composed of a leading byte that indicates the format;
    /// the format indicator byte is one of the FMT_* field values. If the delta
    /// value does not begin with one of the FMT_* indicators, then the delta value
    /// is itself the new value. If the delta is null, then it indicates no change.
    /// The grammar follows:
    /// <pre>
    /// BinaryDelta:
    ///   FMT_EMPTY
    ///   FMT_BINDIFF BinaryChangeList-opt OP_TERM
    ///   FMT_REPLACE-opt Bytes
    ///   null
    ///
    /// BinaryChangeList:
    ///   OP_EXTRACT Offset Length BinaryChangeList-opt
    ///   OP_APPEND Length Bytes BinaryChangeList-opt
    ///
    /// Offset:
    /// Length:
    ///   packed-integer
    ///
    /// Bytes:
    ///   byte Bytes-opt
    /// </pre>
    /// </remarks>
    /// <author>Cameron Purdy  2009.01.06</author>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public class BinaryDeltaCompressor : IDeltaCompressor
    {
        #region Implementation of IDeltaCompressor

        /// <summary>
        /// Compare an old value to a new value and generate a delta that
        /// represents the changes that must be made to the old value in order to
        /// transform it into the new value.
        /// </summary>
        /// <param name="binOld">
        /// The old value.
        /// </param>
        /// <param name="binNew">
        /// The new value.
        /// </param>
        /// <returns>
        /// The changes that must be made to the old value in order to
        /// transform it into the new value, or null to indicate no change.
        /// </returns>
        public Binary ExtractDelta(Binary binOld, Binary binNew)
        {
            // check for no delta
            int cbOld = binOld == null ? 0 : binOld.Length;
            int cbNew = binNew.Length;
            if (cbOld == cbNew && binNew.Equals(binOld))
            {
                return null;
            }

            // check for truncation
            if (cbNew == 0)
            {
                return DELTA_TRUNCATE;
            }

            // for relatively small binaries (or deltas from nothing), just
            // encode the entire thing
            if (cbOld == 0 || cbNew < 64)
            {
                return EncodeReplace(binNew);
            }

            return CreateDelta(binOld, binNew);
        }

        /// <summary>
        /// Apply a delta to an old value in order to create a new value.
        /// </summary>
        /// <param name="binOld">
        /// The old value.
        /// </param>
        /// <param name="binDelta">
        /// The delta information returned from <see cref="IDeltaCompressor.ExtractDelta"/>
        /// to apply to the old value.
        /// </param>
        /// <returns>
        /// The new value.
        /// </returns>
        public Binary ApplyDelta(Binary binOld, Binary binDelta)
        {
            switch (binDelta.ByteAt(0))
            {
                case FMT_EMPTY:
                    // the new value is empty
                    return NO_BINARY;

                case FMT_BINDIFF:
                    // apply a binary dif
                    DataReader inDelta = new DataReader(binDelta.GetStream());
                    inDelta.ReadByte(); // FMT_BINDIFF
                    BinaryMemoryStream bufNew = new BinaryMemoryStream(
                            Math.Max(binOld.Length, binDelta.Length));
                    //DataWriter outNew = new DataWriter(bufNew);
                    while (true)
                    {
                        int nOp = inDelta.ReadByte();
                        switch (nOp)
                        {
                            case OP_EXTRACT:
                                binOld.WriteTo(bufNew, 
                                               inDelta.ReadPackedInt32(),
                                               inDelta.ReadPackedInt32());
                                break;

                            case OP_APPEND:
                                int cb = inDelta.ReadPackedInt32();
                                int of = (int) inDelta.BaseStream.Position;
                                binDelta.WriteTo(bufNew, of, cb);
                                inDelta.BaseStream.Position += cb;
                                break;

                            case OP_TERM:
                                return bufNew.ToBinary();

                            default:
                                throw new InvalidOperationException("Unknown delta operation ("
                                                                + NumberUtils.ToHexEscape((byte) nOp)
                                                                + ") encountered at offset "
                                                                + (inDelta.BaseStream.Position - 1));
                        }
                    }

                case FMT_REPLACE:
                    // the delta is the new value (except for the 1-byte format
                    // indicator)
                    return binDelta.GetBinary(1, binDelta.Length - 1);

                default:
                    // all other formats indicate that the delta _is_ the new
                    // value
                    return binDelta;
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Actually create a delta in the binary delta format. This method is
        /// designed to be overridden by subclasses that have more intimate
        /// knowledge of the contents of the buffers.
        /// </summary>
        /// <param name="binOld">
        /// The old value.
        /// </param>
        /// <param name="binNew">
        /// The new value.
        /// </param>
        /// <returns>
        /// A delta in the binary delta format.
        /// </returns>
        protected Binary CreateDelta(Binary binOld, Binary binNew)
        {
            byte[] abOld      = binOld.ToByteArray();;
            int    ofOldStart = 0;
            byte[] abNew      = binNew.ToByteArray();
            int    ofNewStart = 0;

            // measure the head portion of the binary that is identical
            int ofOld = ofOldStart;
            int ofNew = ofNewStart;
            int cbOld = binOld.Length;
            int cbNew = binNew.Length;
            int cbMax = Math.Min(cbOld, cbNew);
            int ofOldStop = ofOldStart + cbMax;
            while (ofOld < ofOldStop && abOld[ofOld] == abNew[ofNew])
                {
                ++ofOld;
                ++ofNew;
                }
            int cbHead = ofOld - ofOldStart;

            // check if we stripped off the maximum possible (all of it!), and if
            // not, then measure the tail portion that is identical
            int cbTail = 0;
            if (cbHead == cbMax)
                {
                if (cbOld == cbNew)
                    {
                    // the binaries are identical
                    return null;
                    }
                }
            else
                {
                // measure the identical tail
                ofOld = ofOldStart + cbOld - 1;
                ofNew = ofNewStart + cbNew - 1;
                int cbMaxTail = cbMax - cbHead;
                while (--cbMaxTail >= 0 && abOld[ofOld] == abNew[ofNew])
                    {
                    --ofOld;
                    --ofNew;
                    }
                cbTail = ofOldStart + cbOld - 1 - ofOld;
                }

            int cbDone = 0;
            DataWriter outDelta = null;

            if (cbHead > MIN_BLOCK)
                {
                outDelta = WriteExtract(outDelta, cbMax, 0, cbHead);
                cbDone   = cbHead;
                }

            if (cbOld == cbNew)
                {
                // look for identical sections inside the binaries
                ofOld     = ofOldStart + cbHead + 1;
                ofNew     = ofNewStart + cbHead + 1;
                ofOldStop = ofOldStop - cbTail;
                int cbRun = 0;
                while (ofOld < ofOldStop)
                    {
                    if (abOld[ofOld] == abNew[ofNew])
                        {
                        cbRun += 1;
                        }
                    else
                        {
                        if (cbRun > MIN_BLOCK)
                            {
                            // immediately previous to the current offset, there
                            // is a run of cbRun identical bytes, previous to
                            // which there is a series of differing bytes that
                            // will have to be copied verbatum starting at cbDone
                            // bytes into the buffer and proceeding up to the
                            // run of identical bytes
                            int cbDif = ofNew - ofNewStart - cbDone - cbRun;
                            outDelta  = WriteAppend(outDelta, cbMax, abNew, ofNewStart + cbDone, cbDif);
                            outDelta  = WriteExtract(outDelta, cbMax, cbDone + cbDif, cbRun);
                            cbDone   += cbDif + cbRun;
                            }

                        cbRun = 0;
                        }

                    ++ofOld;
                    ++ofNew;
                    }
                }

            if (cbTail > MIN_BLOCK)
                {
                int cbAppend = cbNew - cbDone - cbTail;
                if (cbAppend > 0)
                    {
                    outDelta = WriteAppend(outDelta, cbMax, abNew, ofNewStart + cbDone, cbAppend);
                    }

                // encode the tail
                outDelta = WriteExtract(outDelta, cbMax, cbOld - cbTail, cbTail);
                }
            else if (outDelta != null && cbDone < cbNew)
                {
                outDelta = WriteAppend(outDelta, cbMax, abNew, ofNewStart + cbDone, cbNew - cbDone);
                }

            return outDelta == null
                    ? EncodeReplace(binNew)
                    : FinalizeDelta(outDelta);
        }

        /// <summary>
        /// Encode the passed buffer into a delta value that will cause the old
        /// value to be replaced by the value in the passed buffer.
        /// </summary>
        /// <param name="bin">
        /// A non-null, non-zero-length Binary
        /// </param>
        /// <returns>
        /// a Binary that acts as a delta that replaces an old value
        /// with the contents of <paramref name="bin"/>
        /// </returns>
        private static Binary EncodeReplace(Binary bin)
        {
            switch (bin.ByteAt(0))
            {
                case FMT_EMPTY:
                case FMT_BINDIFF:
                case FMT_REPLACE:
                {
                    int cb = bin.Length;
                    BinaryMemoryStream bufDelta = new BinaryMemoryStream(1 + cb);
                    bufDelta.WriteByte(FMT_REPLACE);
                    bin.WriteTo(bufDelta);
                    return bufDelta.ToBinary();
                }

                default:
                    return bin;
            }
        }

        /// <summary>
        /// Make sure that a DataWriter exists if one doesn't already.
        /// </summary>
        /// <param name="writer">
        /// The existing DataWriter or null.
        /// </param>
        /// <param name="cbMax">
        /// The expected resulting size of the write buffer
        /// </param>
        /// <returns>
        /// A DataWriter, never null
        /// </returns>
        protected static DataWriter EnsureWriter(DataWriter writer, int cbMax)
        {
            if (writer == null)
            {
                writer = new DataWriter(new BinaryMemoryStream(cbMax));
                writer.Write(FMT_BINDIFF);
            }
            return writer;
        }

        /// <summary>
        /// Encode a binary diff "append" operator to indicate that bytes should
        /// be appended from the delta stream to the new value.
        /// </summary>
        /// <param name="writer">
        /// The existing DataWriter for the diff, or null
        /// </param>
        /// <param name="cbMax">
        /// The expected resulting size of the write buffer.
        /// </param>
        /// <param name="ab">
        /// The byte array from which to get the bytes to append.
        /// </param>
        /// <param name="of">
        /// The offset of the old buffer to append.
        /// </param>
        /// <param name="cb">
        /// The length of the old buffer to append.
        /// </param>
        /// <returns>
        /// A DataWriter, never null.
        /// </returns>
        protected static DataWriter WriteAppend(
                DataWriter writer, int cbMax, byte[] ab, int of, int cb)
        {
            writer = EnsureWriter(writer, cbMax);
            writer.Write(OP_APPEND);
            writer.WritePackedInt32(cb);
            writer.Write(ab, of, cb);
            return writer;
        }

        /// <summary>
        /// Encode a binary diff "extract" operator to indicate that bytes should
        /// be copied from the old value to the new value.
        /// </summary>
        /// <param name="writer">
        /// The existing DataWriter for the diff, or null
        /// </param>
        /// <param name="cbMax">
        /// The expected resulting size of the write buffer.
        /// </param>
        /// <param name="of">
        /// The offset of the old buffer to append.
        /// </param>
        /// <param name="cb">
        /// The length of the old buffer to append.
        /// </param>
        /// <returns>
        /// A DataWriter, never null.
        /// </returns>
        protected static DataWriter WriteExtract(
                DataWriter writer, int cbMax, int of, int cb)
        {
            writer = EnsureWriter(writer, cbMax);
            writer.Write(OP_EXTRACT);
            writer.WritePackedInt32(of);
            writer.WritePackedInt32(cb);
            return writer;
        }

        /// <summary>
        /// Convert an open delta output stream into a finalized Binary delta.
        /// </summary>
        /// <param name="writer">
        /// The delta writer.
        /// </param>
        /// <returns>
        /// Finalized Binary delta.
        /// </returns>
        protected static Binary FinalizeDelta(DataWriter writer)
        {
            writer.Write(OP_TERM);
            return ((BinaryMemoryStream) writer.BaseStream).ToBinary();
        }

        #endregion

        #region Constants

        /// <summary>
        /// A format indicator (the first byte of the binary delta) that indicates
        /// that the new value is a zero-length binary value.
        /// </summary>
        protected const byte FMT_EMPTY   = 0xF6;

        /// <summary>
        /// A format indicator (the first byte of the binary delta) that indicates
        /// that the new value is found in its entirety in the delta value. In
        /// other words, other than the first byte, the delta is itself the new
        /// value.
        /// </summary>
        protected const byte FMT_REPLACE = 0xF5;

        /// <summary>
        /// A format indicator (the first byte of the binary delta) that indicates
        /// that the new value is formed by applying a series of modifications to
        /// the old value. The possible modifications are defined by the OP_*
        /// constants.
        /// </summary>
        protected const byte FMT_BINDIFF = 0xF4;

        /// <summary>
        /// A binary delta operator that instructs the <see cref="ApplyDelta"/> method
        /// to extract bytes from the old value and append them to the new value.
        /// The format is the one-byte OP_EXTRACT indicator followed by a packed
        /// int offset and packed int length. The offset and length indicate the
        /// region of the old value to extract and append to the new value.
        /// </summary>
        protected const byte OP_EXTRACT  = 0x01;

        /// <summary>
        /// A binary delta operator that instructs the {@link #applyDelta} method
        /// to copy the following bytes from the delta value and append them to the
        /// new value. The format is the one-byte OP_APPEND indicator followed by a
        /// packed int length and then a series of bytes. The length indicates the
        /// length of the series of bytes to copy from the delta value and append
        /// to the new value.
        /// </summary>
        protected const byte OP_APPEND   = 0x02;

        /// <summary>
        /// A binary delta operator that instructs the {@link #applyDelta} method
        /// that the delta has been fully applied.
        /// </summary>
        protected const byte OP_TERM     = 0x03;

        /// <summary>
        /// Minimum length of an "extract" block to encode.
        /// </summary>
        protected const int  MIN_BLOCK   = 12;

        /// <summary>
        /// An empty Binary object.
        /// </summary>
        protected static readonly Binary NO_BINARY = Binary.NO_BINARY;

        /// <summary>
        /// A delta value that indicates an empty new value.
        /// </summary>
        protected static readonly Binary DELTA_TRUNCATE = new Binary(new byte[] {FMT_EMPTY});

        #endregion
    }
}