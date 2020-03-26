/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Text;

using Tangosol.Util;
using Tangosol.IO.Pof;

namespace Tangosol.IO
{
    /// <summary>
    /// <b>BinaryReader</b> extension that adds methods for reading 32 and
    /// 64-bit integer values in a packed format.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The "packed" format includes a sign bit (0x40) and a continuation
    /// bit (0x80) in the first byte, followed by the least 6 significant
    /// bits of the int value. Subsequent bytes (each appearing only if
    /// the previous byte had its continuation bit set) include a
    /// continuation bit (0x80) and the next least 7 significant bits of
    /// the int value.</p>
    /// <p>
    /// In this way, a 32-bit value is encoded into 1-5 bytes, and 64-bit
    /// value is encoded into 1-10 bytes, depending on the magnitude of the
    /// value being encoded.</p>
    /// </remarks>
    /// <seealso cref="DataWriter"/>
    /// <author>Aleksandar Seovic  2006.08.09</author>
    /// <author>Ivan Cikic  2006.08.09</author>
    public class DataReader : BinaryReader
    {
        #region Constructors

        /// <summary>
        /// Construct a new <b>DataReader</b> that will read from a passed
        /// <b>Stream</b> object.
        /// </summary>
        /// <param name="input">
        /// The <b>Stream</b> object to write from; must not be <c>null</c>.
        /// </param>
        public DataReader(Stream input) : base(input)
        {}

        #endregion

        #region Packed format reading

        /// <summary>
        /// Reads an <b>Int32</b> value using a variable-length storage
        /// format.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The "packed" format includes a sign bit (0x40) and a continuation
        /// bit (0x80) in the first byte, followed by the least 6 significant
        /// bits of the int value. Subsequent bytes (each appearing only if
        /// the previous byte had its continuation bit set) include a
        /// continuation bit (0x80) and the next least 7 significant bits of
        /// the int value. In this way, a 32-bit value is encoded into 1-5
        /// bytes, depending on the magnitude of the value being encoded.</p>
        /// </remarks>
        ///<returns>
        /// An <b>Int32</b> value.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual Int32 ReadPackedInt32()
        {
            int  b     = ReadByte();
            int  n     = b & 0x3F;          // only 6 bits of data in first byte
            int  bits  = 6;
            bool isNeg = (b & 0x40) != 0;   // seventh bit is a sign bit

            while ((b & 0x80) != 0)         // eighth bit is the continuation bit
            {
                b = ReadByte();
                n |= ((b & 0x7F) << bits);
                bits += 7;
            }

            if (isNeg)
            {
                n = ~n;
            }

            return n;
        }

        /// <summary>
        /// Reads an <b>Int64</b> value using a variable-length storage
        /// format.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The "packed" format includes a sign bit (0x40) and a continuation
        /// bit (0x80) in the first byte, followed by the least 6 significant
        /// bits of the int value. Subsequent bytes (each appearing only if
        /// the previous byte had its continuation bit set) include a
        /// continuation bit (0x80) and the next least 7 significant bits of
        /// the int value. In this way, a 64-bit value is encoded into 1-10
        /// bytes, depending on the magnitude of the value being encoded.</p>
        /// </remarks>
        /// <returns>
        /// An <b>Int64</b> value.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual Int64 ReadPackedInt64()
        {
            int  b     = ReadByte();
            long l     = b & 0x3F;          // only 6 bits of data in first byte
            int  bits  = 6;
            bool isNeg = (b & 0x40) != 0;   // seventh bit is a sign bit

            while ((b & 0x80) != 0)         // eighth bit is the continuation bit
            {
                b = ReadByte();
                l |= (((long) (b & 0x7F)) << bits);
                bits += 7;
            }

            if (isNeg)
            {
                l = ~l;
            }

            return l;
        }

        /// <summary>
        /// Reads a <b>RawInt128</b> value from <b>DataReader</b>.
        /// </summary>
        /// <param name="reader">
        /// The <b>DataReader</b> to read from.
        /// </param>
        /// <returns>
        /// <b>RawInt128</b> value.
        /// </returns>
        public virtual RawInt128 ReadPackedRawInt128(DataReader reader)
        {
            const int cb = 16;
            var ab = new byte[cb + 1];
            int b = reader.ReadByte();
            bool isNeg = (b & 0x40) != 0;
            int of = cb;
            int cBits = 6;

            ab[of] = (byte)(b & 0x3F);

            while ((b & 0x80) != 0)
            {
                b = reader.ReadByte();
                ab[of] = (byte)((ab[of] & 0xFF) | ((b & 0x7F) << cBits));
                cBits += 7;
                if (cBits >= 8)
                {
                    cBits -= 8;
                    --of;

                    if (cBits > 0 && of >= 0)
                    {
                        ab[of] = (byte)(NumberUtils.URShift((b & 0x7F), (7 - cBits)));
                    }
                }
            }

            if (ab[of] == 0 && of < 16)
            {
                of++;
            }

            var result = new byte[ab.Length - of];
            Buffer.BlockCopy(ab, of, result, 0, result.Length);

            if (isNeg)
            {
                for (of = 0; of < result.Length; ++of)
                {
                    result[of] = (byte)~result[of];
                }
            }

            return new RawInt128(result, isNeg);
        }

        #endregion

        #region BinaryReader override methods

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream and
        /// advances the current position of the stream by two bytes.
        /// </summary>
        /// <remarks>
        /// Overrides BinaryReader.ReadInt16 by changing endian of the value
        /// read from the stream.
        /// </remarks>
        /// <returns>
        /// A 2-byte signed integer read from the current stream.
        /// </returns>
        public override Int16 ReadInt16()
        {
            return NumberUtils.ChangeEndian(base.ReadInt16());
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using
        /// little endian encoding and advances the position of the stream by
        /// two bytes.
        /// </summary>
        /// <remarks>
        /// Overrides BinaryReader.ReadUInt16 by changing endian of the value
        /// read from the stream.
        /// </remarks>
        /// <returns>
        /// A 2-byte unsigned integer read from this stream.
        /// </returns>
        public override UInt16 ReadUInt16()
        {
            return NumberUtils.ChangeEndian(base.ReadUInt16());
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream and
        /// advances the current position of the stream by four bytes.
        /// </summary>
        /// <remarks>
        /// Overrides BinaryReader.ReadInt32 by changing endian of the value
        /// read from the stream.
        /// </remarks>
        /// <returns>
        /// A 4-byte signed integer read from the current stream.
        /// </returns>
        public override Int32 ReadInt32()
        {
            return NumberUtils.ChangeEndian(base.ReadInt32());
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream and
        /// advances the position of the stream by four bytes.
        /// </summary>
        /// <remarks>
        /// Overrides BinaryReader.ReadUInt32 by changing endian of the value
        /// read from the stream.
        /// </remarks>
        /// <returns>
        /// A 4-byte unsigned integer read from this stream.
        /// </returns>
        public override UInt32 ReadUInt32()
        {
            return NumberUtils.ChangeEndian(base.ReadUInt32());
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream and
        /// advances the current position of the stream by eight bytes.
        /// </summary>
        /// <remarks>
        /// Overrides BinaryReader.ReadInt64 by changing endian of the value
        /// read from the stream.
        /// </remarks>
        /// <returns>
        /// An 8-byte signed integer read from the current stream.
        /// </returns>
        public override Int64 ReadInt64()
        {
            return NumberUtils.ChangeEndian(base.ReadInt64());
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream and
        /// advances the position of the stream by eight bytes.
        /// </summary>
        /// <remarks>
        /// Overrides BinaryReader.ReadUInt64 by changing endian of the value
        /// read from the stream.
        /// </remarks>
        /// <returns>
        /// An 8-byte unsigned integer read from this stream.
        /// </returns>
        public override UInt64 ReadUInt64()
        {
            return NumberUtils.ChangeEndian(base.ReadUInt64());
        }

        /// <summary>
        /// Reads bits which are stored in an <b>Int32</b> instance and
        /// converts them into the <b>Single</b> object.
        /// </summary>
        /// <returns>
        /// A Single value read from the stream.
        /// </returns>
        public override float ReadSingle()
        {
            return NumberUtils.Int32BitsToSingle(ReadInt32());
        }

        /// <summary>
        /// Reads bits which are stored in an <b>Int64</b> instance and
        /// converts them into the <b>Double</b> object.
        /// </summary>
        /// <returns>
        /// A Double value read from the stream.
        /// </returns>
        public override double ReadDouble()
        {
            return NumberUtils.Int64BitsToDouble(ReadInt64());
        }

        /// <summary>
        /// Reads string from the stream.
        /// </summary>
        /// <remarks>
        /// String is prefixed with the string length encoded as "packed"
        /// Int32.
        /// </remarks>
        /// <returns>
        /// A String value read from the stream.
        /// </returns>
        public override string ReadString()
        {
            int length = ReadPackedInt32();
            switch (length)
            {
                case -1:
                case PofConstants.V_REFERENCE_NULL:
                    return null;
                case 0:
                    return String.Empty;
                default:
                    byte[] bytes = ReadBytes(length);
                    return SerializationHelper.ConvertUTF(bytes, 0, bytes.Length);
            }
        }

        #endregion
    }
}