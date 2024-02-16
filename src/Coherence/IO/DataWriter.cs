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

        #region Properties

        /// <summary>
        /// Obtain a temp buffer used to avoid allocations from
        /// repeated calls to String APIs.
        /// </summary>
        /// <return>
        /// a char buffer of CHAR_BUF_SIZE characters long
        /// </return>
        protected char[] CharBuf
        {
            get
            {
                // "partial" (i.e. windowed) char buffer just for formatUTF
                char[] ach = m_achBuf;
                if (ach == null)
                {
                    m_achBuf = ach = new char[CHAR_BUF_SIZE];
                }
                return ach;
            }
        }

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
                byte[] bytes = FormatUTF(text);
                WritePackedInt32(bytes.Length);
                Write(bytes);
            }
        }

        #endregion

        #region UTF encoding functions

        /// <summary>
        /// Figure out how many bytes it will take to hold the passed String.
        /// </summary>
        /// <remarks>
        /// This method is tightly bound to formatUTF.
        /// </remarks>
        /// <param  name="s">
        /// the String
        /// </param>
        /// <return>
        /// the binary UTF length
        /// </return>
        protected int CalcUTF(String s)
        {
            int    cch    = s.Length;
            int    cb     = cch;
            char[] ach    = CharBuf;
            bool   fSmall = (cch <= CHAR_BUF_SIZE);
            if (fSmall)
            {
                var src = new StringBuilder(s);
                src.CopyTo(0, ach, 0, cch);
            }

            for (int ofch = 0; ofch < cch; ++ofch)
            {
                int ch;
                if (fSmall)
                {
                    ch = ach[ofch];
                }
                else
                {
                    int ofBuf = ofch & CHAR_BUF_MASK;
                    if (ofBuf == 0)
                    {
                        var src = new StringBuilder(s);
                        int len = Math.Min(ofch + CHAR_BUF_SIZE, cch) - ofch;
                        src.CopyTo(ofch, ach, 0, len);
                    }
                    ch = ach[ofBuf];
                }

                if (ch <= 0x007F)
                {
                    // all bytes in this range use the 1-byte format
                    // except for 0
                    if (ch == 0)
                    {
                        ++cb;
                    }
                }
                else
                {
                    // either a 2-byte format or a 3-byte format (if over
                    // 0x07FF)
                    cb += (ch <= 0x07FF ? 1 : 2);
                }
            }

            return cb;
       }

        /// <summary>
        /// Format the passed String as UTF into the passed byte array.
        /// </summary>
        /// <remarks>
        /// This method is tightly bound to calcUTF.
        /// </remarks>
        /// <param name="s">
        /// the string.
        /// </param>
        /// <returns>
        /// The formated UTF byte array.
        /// </returns>
        public byte[] FormatUTF(String s)
        {
            int    cch = s.Length;
            int    cb  = CalcUTF(s);
            int    ofb = 0;
            byte[] ab  = new byte[cb];

            if (cb == cch)
            {
                // ask the string to convert itself to ascii bytes
                // straight into the WriteBuffer                
                Encoding.ASCII.GetBytes(s, 0, cch, ab, ofb);
            }
            else
            {
                char[]  ach = CharBuf;
                if (cch <= CHAR_BUF_SIZE)
                {
                    // The following is unnecessary, because it would already
                    // have been performed by calcUTF:
                    //
                    //   if (fSmall)
                    //       {
                    //       s.getChars(0, cch, ach, 0);
                    //       }
                    FormatUTF(ab, ofb, ach, cch);
                }
                else
                {
                    for (int ofch = 0; ofch < cch; ofch += CHAR_BUF_SIZE)
                    {
                        int cchChunk = Math.Min(CHAR_BUF_SIZE, cch - ofch);
                        StringBuilder src = new StringBuilder(s);
                        src.CopyTo(ofch, ach, 0, cchChunk);
                        ofb += FormatUTF(ab, ofb, ach, cchChunk);
                    }
                }
            }

            return ab;
        }

        /// <summary>
        /// Format the passed characters as UTF into the passed byte array.
        /// </summary>
        /// <param name="ab">
        /// The byte array to format into.
        /// </param>
        /// <param name="ofb">
        /// The offset into the byte array to write the first byte.
        /// </param>
        /// <param name="ach">
        /// The array of characters to format.
        /// </param>
        /// <param name="cch">
        /// The number of characters to format.
        /// </param>
        /// <return>
        /// The number of bytes written to the array.
        /// </return>
        protected int FormatUTF(byte[] ab, int ofb, char[] ach, int cch)
        {
            int ofbOrig = ofb;
            for (int ofch = 0; ofch < cch; ++ofch)
            {
                char ch = ach[ofch];
                if (ch >= 0x0001 && ch <= 0x007F)
                {
                    // 1-byte format:  0xxx xxxx
                    ab[ofb++] = (byte) ch;
                }
                else if (ch <= 0x07FF)
                {
                    // 2-byte format:  110x xxxx, 10xx xxxx
                    ab[ofb++] = (byte) (0xC0 | ((ch >> 6) & 0x1F));
                    ab[ofb++] = (byte) (0x80 | ((ch     ) & 0x3F));
                }
                else
                {
                    // 3-byte format:  1110 xxxx, 10xx xxxx, 10xx xxxx
                    ab[ofb++] = (byte) (0xE0 | ((ch >> 12) & 0x0F));
                    ab[ofb++] = (byte) (0x80 | ((ch >>  6) & 0x3F));
                    ab[ofb++] = (byte) (0x80 | ((ch      ) & 0x3F));
               }
            }
            return ofb - ofbOrig;
        }

        ///<summary>
        /// Get a buffer for formating data to bytes. Note that the resulting buffer
        /// may be shorter than the requested size.
        /// </summary>
        /// <param  name="cb">
        /// the requested size for the buffer
        /// </param>
        /// <return>
        /// A byte array that is at least <tt>cb</tt> bytes long, but not
        /// shorter than <see cref="MIN_BUF"/> and (regardless of the value of
        /// <tt>cb</tt>) not longer than <see cref="MAX_BUF"/>.
        /// </return>
        protected byte[] Tmpbuf(int cb)
        {
            byte[] ab = m_abBuf;
            if (ab == null || ab.Length < cb)
            {
                int cbOld = ab == null ? 0 : ab.Length;
                int cbNew = Math.Max(MIN_BUF, Math.Min(MAX_BUF , cb));
                if (cbNew > cbOld)
                {
                    m_abBuf = ab = new byte[cbNew > ((uint) MAX_BUF >> 1) ? MAX_BUF : cbNew];
                }
            }
            return ab;
        }

        #endregion

        #region Data Members

        /// <summary>
        /// The minimum size of the temp buffer.
        /// </summary>
        private const int MIN_BUF = 0x40;

        /// <summary>
        /// The maximum size of the temp buffer. The maximum size must be at least
        /// <tt>(3 * CHAR_BUF_SIZE)</tt> to accomodate the worst-case UTF
        /// formatting length.
        /// </summary>
        private const int MAX_BUF = 0x400;

        /// <summary>
        /// Size of the temporary character buffer. Must be a power of 2.
        /// Size is: 256 characters (.25 KB).        
        /// </summary> 
        protected const int CHAR_BUF_SIZE = 0x100;

        /// <summary>
        /// Bitmask used against a raw offset to determine the offset within
        /// the temporary character buffer.
        /// </summary>
        protected const int CHAR_BUF_MASK = (CHAR_BUF_SIZE - 1);

        /// <summary>
        /// A temp buffer to use for building the data to write.
        /// </summary>
        [NonSerialized]
        private byte[] m_abBuf;

        /// <summary>
        /// A lazily instantiated temp buffer used to avoid allocations from
        /// and repeated calls to String functions.
        /// </summary>
        [NonSerialized]
        protected char[] m_achBuf;

        #endregion
    }
}