/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
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
    /// <b>BinaryWriter</b> extension that adds methods for writing 32 and
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
    /// <seealso cref="DataReader"/>
    /// <author>Goran Milosavljevic  2006.08.09</author>
    public class DataWriter : BinaryWriter
    {
        #region Constructors

        /// <summary>
        /// Construct a new <b>DataWriter</b> that will write data to the
        /// specified <b>Stream</b> object.
        /// </summary>
        /// <param name="output">
        /// The <b>Stream</b> object to write to; must not be <c>null</c>.
        /// </param>
        public DataWriter(Stream output) : base(output)
        {}

        #endregion

        #region Packed format writing

        /// <summary>
        /// Write an <b>Int32</b> value using a variable-length storage
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
        /// <param name="n">
        /// An <b>Int32</b> value to write.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WritePackedInt32(int n)
        {
            byte[] buffer = new byte[5];
            int    cb     = 0;

            // first byte contains sign bit (bit 7 set if neg)
            int b = 0;
            if (n < 0)
            {
                b = 0x40;
                n = ~n;
            }

            // first byte contains only 6 data bits
            b |= (byte) (n & 0x3F);
            n >>= 6;

            while (n != 0)
            {
                b |= 0x80;          // bit 8 is a continuation bit
                buffer[cb++] = (byte) b;

                b = (n & 0x7F);
                n >>= 7;
            }

            if (cb == 0)
            {
                // one-byte format
                Write((byte) b);
            }
            else
            {
                buffer[cb++] = (byte) b;
                Write(buffer, 0, cb);
            }
        }

        /// <summary>
        /// Write an <b>Int64</b> value using a variable-length storage
        /// format.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The "packed" format includes a sign bit (0x40) and a continuation
        /// bit (0x80) in the first byte, followed by the least 6 significant
        /// bits of the long value. Subsequent bytes (each appearing only if
        /// the previous byte had its continuation bit set) include a
        /// continuation bit (0x80) and the next least 7 significant bits of
        /// the long value. In this way, a 64-bit value is encoded into 1-10
        /// bytes, depending on the magnitude of the value being encoded.</p>
        /// </remarks>
        /// <param name="l">
        /// An <b>Int64</b> value to write.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WritePackedInt64(long l)
        {
            byte[] buffer = new byte[10];
            int    cb     = 0;

            // first byte contains sign bit (bit 7 set if neg)
            int b = 0;
            if (l < 0)
            {
                b = 0x40;
                l = ~l;
            }

            // first byte contains only 6 data bits
            b |= (byte) (((int) l) & 0x3F);
            l >>= 6;

            while (l != 0)
            {
                b |= 0x80; // bit 8 is a continuation bit
                buffer[cb++] = (byte) b;

                b = (((int) l) & 0x7F);
                l >>= 7;
            }

            if (cb == 0)
            {
                // one-byte format
                Write((byte) b);
            }
            else
            {
                buffer[cb++] = (byte) b;
                Write(buffer, 0, cb);
            }
        }

        /// <summary>
        /// Write a <b>RawInt128</b> value to <b>DataWriter</b>.
        /// </summary>
        /// <remarks>
        /// <b>RawInt128</b> value, which is represented as array of
        /// signed bytes.
        /// </remarks>
        /// <param name="writer">
        /// The DataWriter to write to.
        /// </param>
        /// <param name="rawInt128">
        /// <b>RawInt128</b> value.
        /// </param>
        public virtual void WritePackedRawInt128(DataWriter writer, RawInt128 rawInt128)
        {
            sbyte[] bigInteger = new sbyte[rawInt128.Length];
            Buffer.BlockCopy(rawInt128.Value, 0, bigInteger, 0, rawInt128.Length);

            int b = 0;
            int cBits = bigInteger.Length * 8;
            int cb = bigInteger.Length;

            // check for negative
            if (rawInt128.IsNegative)
            {
                b = 0x40;
                for (int of = 0; of < cb; ++of)
                {
                    bigInteger[of] = (sbyte)~bigInteger[of];
                }
            }

            int ofMSB = 0;
            while (ofMSB < cb && bigInteger[ofMSB] == 0)
            {
                ++ofMSB;
            }

            if (ofMSB < cb)
            {
                int of = cb - 1;
                int nBits = bigInteger[of] & 0xFF;

                b |= (sbyte)(nBits & 0x3F);
                nBits = NumberUtils.URShift(nBits, 6);

                cBits = 2;  // only 2 data bits left in nBits

                while (nBits != 0 || of > ofMSB)
                {
                    b |= 0x80;
                    writer.Write((sbyte)b);

                    // load more data bits if necessary
                    if (cBits < 7)
                    {
                        nBits |= (--of < 0 ? 0 : bigInteger[of] & 0xFF) << cBits;
                        cBits += 8;
                    }

                    b = (nBits & 0x7F);
                    nBits = NumberUtils.URShift(nBits, 7);
                    cBits -= 7;
                }
            }

            writer.Write((sbyte)b);
        }

        #endregion

        #region BinaryWriter override methods

        /// <summary>
        /// Writes a two-byte signed integer to the current stream and
        /// advances the stream position by two bytes.
        /// </summary>
        /// <remarks>
        /// Overrides BinaryWriter.Write(Int16) by changing endian of the
        /// value written to the stream.
        /// </remarks>
        /// <param name="value">
        /// The two-byte signed integer to write.
        /// </param>
        public override void Write(Int16 value)
        {
            base.Write(NumberUtils.ChangeEndian(value));
        }

        /// <summary>
        /// Writes a two-byte unsigned integer to the current stream and
        /// advances the stream position by two bytes.
        /// </summary>
        /// <remarks>
        /// Overrides BinaryWriter.Write(UInt16) by changing endian of the
        /// value written to the stream.
        /// </remarks>
        /// <param name="value">
        /// The two-byte unsigned integer to write.
        /// </param>
        public override void Write(UInt16 value)
        {
            base.Write(NumberUtils.ChangeEndian(value));
        }

        /// <summary>
        /// Writes a four-byte signed integer to the current stream and
        /// advances the stream position by four bytes.
        /// </summary>
        /// <remarks>
        /// Overrides BinaryWriter.Write(Int32) by changing endian of the
        /// value written to the stream.
        /// </remarks>
        /// <param name="value">
        /// The four-byte signed integer to write.
        /// </param>
        public override void Write(Int32 value)
        {
            base.Write(NumberUtils.ChangeEndian(value));
        }

        /// <summary>
        /// Writes a four-byte unsigned integer to the current stream and
        /// advances the stream position by four bytes.
        /// </summary>
        /// <remarks>
        /// Overrides BinaryWriter.Write(UInt32) by changing endian of the
        /// value written to the stream.
        /// </remarks>
        /// <param name="value">
        /// The four-byte unsigned integer to write.
        /// </param>
        public override void Write(UInt32 value)
        {
            base.Write(NumberUtils.ChangeEndian(value));
        }

        /// <summary>
        /// Writes a eight-byte signed integer to the current stream and
        /// advances the stream position by eight bytes.
        /// </summary>
        /// <remarks>
        /// Overrides BinaryWriter.Write(Int64) by changing endian of the
        /// value written to the stream.
        /// </remarks>
        /// <param name="value">
        /// The eight-byte signed integer to write.
        /// </param>
        public override void Write(Int64 value)
        {
            base.Write(NumberUtils.ChangeEndian(value));
        }

        /// <summary>
        /// Writes a eight-byte unsigned integer to the current stream and
        /// advances the stream position by eight bytes.
        /// </summary>
        /// <remarks>
        /// Overrides BinaryWriter.Write(UInt64) by changing endian of the
        /// value written to the stream.
        /// </remarks>
        /// <param name="value">
        /// The eight-byte unsigned integer to write.
        /// </param>
        public override void Write(UInt64 value)
        {
            base.Write(NumberUtils.ChangeEndian(value));
        }

        /// <summary>
        /// Converts a Single value to its bits and writes an Int32 instance
        /// which stores the bits.
        /// </summary>
        /// <param name="value">
        /// A Single value to write.
        /// </param>
        public override void Write(Single value)
        {
            Write(NumberUtils.SingleToInt32Bits(value));
        }

        /// <summary>
        /// Converts a Double value to its bits and writes an Int64 instance
        /// which stores the bits.
        /// </summary>
        /// <param name="value">
        /// A Double value to write.
        /// </param>
        public override void Write(Double value)
        {
            Write(NumberUtils.DoubleToInt64Bits(value));
        }

        /// <summary>
        /// Writes string to the stream prefixed by its length in "packed"
        /// format.
        /// </summary>
        /// <param name="text">
        /// A string to write.
        /// </param>
        public override void Write(string text)
        {
            if (text == null)
            {
                WritePackedInt32(-1);
            }
            else if (text.Length == 0)
            {
                WritePackedInt32(0);
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                WritePackedInt32(bytes.Length);
                Write(bytes);
            }
        }

        #endregion
    }
}