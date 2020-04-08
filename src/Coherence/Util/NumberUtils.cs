/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Net;
using System.Threading;

using Tangosol.IO.Pof;

namespace Tangosol.Util
{
    /// <summary>
    /// Miscellaneuos utility methods for numbers manipulation.
    /// </summary>
    /// <author>Aleksandar Seovic  2006.08.09</author>
    /// <author>Ivan Cikic  2006.09.13</author>
    public abstract class NumberUtils
    {
        static NumberUtils()
        {
            for (int i = 0, c = CRC32_TABLE.Length; i < c; ++i)
            {
                uint crc = Convert.ToUInt32(i);
                for (int n = 0; n < 8; ++n)
                {
                    if ((crc & 1) == 1)
                    {
                        crc = (crc >> 1) ^ CRC32_BASE;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
                CRC32_TABLE[i] = crc;
            }
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number.
        /// </summary>
        /// <param name="number">
        /// Number to operate on.
        /// </param>
        /// <param name="bits">
        /// Ammount of bits to shift.
        /// </param>
        /// <returns>
        /// The resulting number from the shift operation.
        /// </returns>
        public static int URShift(int number, int bits)
        {
            if (number >= 0)
            {
                return number >> bits;
            }
            else
            {
                return (number >> bits) + (2 << ~bits);
            }
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number.
        /// </summary>
        /// <param name="number">
        /// Number to operate on.
        /// </param>
        /// <param name="bits">
        /// Ammount of bits to shift.
        /// </param>
        /// <returns>
        /// The resulting number from the shift operation.
        /// </returns>
        public static long URShift(long number, int bits)
        {
            if (number >= 0)
            {
                return number >> bits;
            }
            else
            {
                return (number >> bits) + (2L << ~bits);
            }
        }

        /// <summary>
        /// Gets the unscaled value of <b>Decimal</b> value.
        /// </summary>
        /// <param name="value">
        /// The <b>Decimal</b> value to get scale from.
        /// </param>
        /// <returns>
        /// Decimal which is unscaled value of a <b>Decimal</b>.
        /// </returns>
        public static Decimal GetUnscaledValue(Decimal value)
        {
            if (NumberUtils.GetScale(value) == 0)
            {
                // if scale is 0, value is already unscaled.
                return value;
            }
            else if (value == 0)
            {
                return 0M;
            }
            else
            {
                String sValue = value.ToString();
                int    index  = sValue.IndexOf(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                if (index < 0)
                {
                    // no decimal place, return value as is
                    return value;
                }

                String sUnscaledValue = null;
                if (index == 0)
                {
                    sUnscaledValue = sValue.Substring(1).TrimStart('0');
                }
                else
                {
                    sUnscaledValue = String.Concat(sValue.Substring(0, index), sValue.Substring(index + 1)).TrimStart('0');
                }

                return Decimal.Parse(sUnscaledValue);
            }
        }

        /// <summary>
        /// Gets a scale of <b>Decimal</b> value.
        /// </summary>
        /// <param name="value">
        /// The <b>Decimal</b> value to get scale from.
        /// </param>
        /// <returns>
        /// Scale of a <b>Decimal</b>.
        /// </returns>
        public static int GetScale(Decimal value)
        {
            int[] bits = Decimal.GetBits(value);

            return ((bits[3] >> 16) & 0xFF);
        }

        /// <summary>
        /// Converts <b>Decimal</b> to unscaled <b>RawInt128</b>
        /// representation - compatible with Java BigInteger type.
        /// </summary>
        /// <param name="value">
        /// Decimal value to convert.
        /// </param>
        /// <returns>
        /// Signed-byte array representing unscaled <b>RawInt128</b> value.
        /// </returns>
        public static RawInt128 DecimalToRawInt128(Decimal value)
        {
            int[] bits  = Decimal.GetBits(value);
            int   scale = ((bits[3] >> 16) & 0xFF);

            // add 1 as decimal
            bool isNeg = false;
            Decimal d1 = new Decimal(1, 0, 0, false, (byte)scale);
            if (value < 0)
            {
                value += d1;
                isNeg = true;
            }

            // convert to Little-Endian
            bits    = Decimal.GetBits(value);
            bits[0] = IPAddress.NetworkToHostOrder(bits[0]);
            bits[1] = IPAddress.NetworkToHostOrder(bits[1]);
            bits[2] = IPAddress.NetworkToHostOrder(bits[2]);
            bits[3] = IPAddress.NetworkToHostOrder(bits[3]);

            // join all bits that represent unscaled value
            byte[] ab   = new byte[12];
            byte[] out1 = BitConverter.GetBytes(bits[2]);
            byte[] out2 = BitConverter.GetBytes(bits[1]);
            byte[] out3 = BitConverter.GetBytes(bits[0]);

            Buffer.BlockCopy(out1, 0, ab, 0, out1.Length);
            Buffer.BlockCopy(out2, 0, ab, 4, out2.Length);
            Buffer.BlockCopy(out3, 0, ab, 8, out3.Length);

            sbyte[] sab = CollectionUtils.ToSByteArrayUnchecked(ab);

            // trim zeroes from the beggining
            int index = -1;
            for (int of = 0; of < sab.Length; of++)
            {
                if (sab[of] != 0)
                {
                    if (index == -1) { index = of; break; }
                }
            }

            if (index == -1)
            {
                index = 0;
            }

            sbyte[] sabf = new sbyte[sab.Length - index];
            Buffer.BlockCopy(sab, index, sabf, 0, sabf.Length);

            // add 0 byte if it is a positive number
            sbyte[] resultBigInt;
            sbyte   firstByte = sabf[0];
            if ((firstByte & 0x80) != 0)
            {
                resultBigInt = new sbyte[sabf.Length + 1];
                Buffer.BlockCopy(sabf, 0, resultBigInt, 1, sabf.Length);
                resultBigInt[0] = 0;
            }
            else
            {
                resultBigInt = sabf;
            }

            // do difference if it is a negative number
            if (isNeg)
            {
                for (int of = 0; of < resultBigInt.Length; of++)
                {
                    resultBigInt[of] = (sbyte) ~resultBigInt[of];
                }
            }

            return new RawInt128(resultBigInt, isNeg);
        }

        /// <summary>
        /// Encode <b>RawInt128</b> value provided to array of int values
        /// representing <b>Decimal</b> unscaled value bits.
        /// </summary>
        /// <param name="int128">
        /// <b>RawInt128</b> value.
        /// </param>
        /// <returns>
        /// Decimal unscaled value bits as array of int.
        /// </returns>
        public static int[] EncodeDecimalBits(RawInt128 int128)
        {
            int[]  result = new int[4];
            byte[] rawInt = CollectionUtils.ToByteArrayUnchecked(int128.Value);
            byte[] bits   = new byte[4];
            int    fourth = 0;
            int    bcount = 0;

            for (int i = rawInt.Length - 1; i > -1; i--)
            {
                fourth++;
                bits[fourth - 1] = rawInt[i];

                if (fourth == 4)
                {
                    int intval       = BitConverter.ToInt32(bits, 0);
                    result[bcount++] = intval;
                    bits             = new byte[4];
                    fourth           = 0;
                }
            }

            if (fourth > 0 && bcount < result.Length)
            {
                result[bcount] = BitConverter.ToInt32(bits, 0);
            }

            return result;
        }

        /// <summary>
        /// Converts <b>Single</b> to its bits, which are stored in a
        /// <b>Int32</b> instance.
        /// </summary>
        /// <param name="value">
        /// Value to convert to bits.
        /// </param>
        /// <returns>
        /// Bits packed within an <b>Int32</b> instance.
        /// </returns>
        public static unsafe int SingleToInt32Bits(Single value)
        {
            return *((int*) &value);
        }

        /// <summary>
        /// Converts bits which are stored in an <b>Int32</b> instance into
        /// the <b>Single</b> object.
        /// </summary>
        /// <param name="value">
        /// Bits packed within an <b>Int32</b> instance.
        /// </param>
        /// <returns>
        /// <b>Single</b> value represented by the bits in
        /// <paramref name="value"/>.
        /// </returns>
        public static unsafe float Int32BitsToSingle(Int32 value)
        {
            return *((float*) &value);
        }
        /// <summary>
        /// Converts <b>Double</b> to its bits, which are stored in a
        /// <b>Int64</b> instance.
        /// </summary>
        /// <param name="value">
        /// Value to convert to bits.
        /// </param>
        /// <returns>
        /// Bits packed within an <b>Int64</b> instance.
        /// </returns>
        public static unsafe long DoubleToInt64Bits(Double value)
        {
            return *((long*) &value);
        }

        /// <summary>
        /// Converts bits which are stored in an <b>Int64</b> instance into
        /// the <b>Double</b> object.
        /// </summary>
        /// <param name="value">
        /// Bits packed within an <b>Int64</b> instance.
        /// </param>
        /// <returns>
        /// <b>Double</b> value represented by the bits in
        /// <paramref name="value"/>.
        /// </returns>
        public static unsafe double Int64BitsToDouble(Int64 value)
        {
            return *((double*) &value);
        }

        /// <summary>
        /// Converts <b>Int32</b> to its byte array.
        /// </summary>
        /// <param name="number">
        /// Value to convert to byte array.
        /// </param>
        /// <returns>
        /// Number packed within a <b>Byte</b> array.
        /// </returns>
        public static unsafe byte[] IntToByteArray(int number)
        {
            byte[] intBytes = new byte[4];
            intBytes[0] = *((byte*) &number);
            intBytes[1] = *((byte*) (&number + 1));
            intBytes[2] = *((byte*) (&number + 2));
            intBytes[3] = *((byte*) (&number + 3));

            return intBytes;
        }

        /// <summary>
        /// Changes the endian of <b>Int16</b> value.
        /// </summary>
        /// <param name="value">
        /// Value for which endian is being changed.
        /// </param>
        /// <returns>
        /// Value with changed endian.
        /// </returns>
        public static Int16 ChangeEndian(Int16 value)
        {
            byte b0 = (byte)((value & 0x00FF) >> 0);
            byte b1 = (byte)((value & 0xFF00) >> 8);
            Int16 retVal = (Int16) ((b0 << 8) | (b1 << 0));

            return retVal;
        }

        /// <summary>
        /// Changes the endian of <b>UInt16</b> value.
        /// </summary>
        /// <param name="value">
        /// Value for which endian is being changed.
        /// </param>
        /// <returns>
        /// Value with changed endian.
        /// </returns>
        public static UInt16 ChangeEndian(UInt16 value)
        {
            byte b0 = (byte)((value & 0x00FF) >> 0);
            byte b1 = (byte)((value & 0xFF00) >> 8);
            UInt16 retVal = (UInt16)((b0 << 8) | (b1 << 0));

            return retVal;
        }

        /// <summary>
        /// Changes the endian of <b>Int32</b> value.
        /// </summary>
        /// <param name="value">
        /// Value for which endian is being changed.
        /// </param>
        /// <returns>
        /// Value with changed endian.
        /// </returns>
        public static Int32 ChangeEndian(Int32 value)
        {
            byte b0 = (byte) ((value & 0x000000FF) >> 0);
            byte b1 = (byte) ((value & 0x0000FF00) >> 8);
            byte b2 = (byte) ((value & 0x00FF0000) >> 16);
            byte b3 = (byte) ((value & 0xFF000000) >> 24);
            Int32 retVal = (b0 << 24) | (b1 << 16) | (b2 << 8) | (b3 << 0);

            return retVal;
        }

        /// <summary>
        /// Changes the endian of <b>UInt32</b> value.
        /// </summary>
        /// <param name="value">
        /// Value for which endian is being changed.
        /// </param>
        /// <returns>
        /// Value with changed endian.
        /// </returns>
        public static UInt32 ChangeEndian(UInt32 value)
        {
            byte b0 = (byte)((value & 0x000000FF) >> 0);
            byte b1 = (byte)((value & 0x0000FF00) >> 8);
            byte b2 = (byte)((value & 0x00FF0000) >> 16);
            byte b3 = (byte)((value & 0xFF000000) >> 24);
            UInt32 retVal = ((UInt32) b0 << 24) | ((UInt32) b1 << 16) | ((UInt32) b2 << 8) | ((UInt32) b3 << 0);

            return retVal;
        }

        /// <summary>
        /// Changes the endian of <b>Int64</b> value.
        /// </summary>
        /// <param name="value">
        /// Value for which endian is being changed.
        /// </param>
        /// <returns>
        /// Value with changed endian.
        /// </returns>
        public static Int64 ChangeEndian(Int64 value)
        {
            byte b0 = (byte)((value & 0x00000000000000FF) >> 0);
            byte b1 = (byte)((value & 0x000000000000FF00) >> 8);
            byte b2 = (byte)((value & 0x0000000000FF0000) >> 16);
            byte b3 = (byte)((value & 0x00000000FF000000) >> 24);
            byte b4 = (byte)((value & 0x000000FF00000000) >> 32);
            byte b5 = (byte)((value & 0x0000FF0000000000) >> 40);
            byte b6 = (byte)((value & 0x00FF000000000000) >> 48);
            byte b7 = (byte)((value & unchecked((Int64) 0xFF00000000000000)) >> 56);
            Int64 retVal = ((Int64)b0 << 56) | ((Int64)b1 << 48) | ((Int64)b2 << 40) | ((Int64)b3 << 32)
                        | ((Int64)b4 << 24) | ((Int64)b5 << 16) | ((Int64)b6 << 8) | ((Int64)b7 << 0);

            return retVal;
        }

        /// <summary>
        /// Changes the endian of <b>UInt64</b> value.
        /// </summary>
        /// <param name="value">
        /// Value for which endian is being changed.
        /// </param>
        /// <returns>
        /// Value with changed endian.
        /// </returns>
        public static UInt64 ChangeEndian(UInt64 value)
        {
            byte b0 = (byte)((value & 0x00000000000000FF) >> 0);
            byte b1 = (byte)((value & 0x000000000000FF00) >> 8);
            byte b2 = (byte)((value & 0x0000000000FF0000) >> 16);
            byte b3 = (byte)((value & 0x00000000FF000000) >> 24);
            byte b4 = (byte)((value & 0x000000FF00000000) >> 32);
            byte b5 = (byte)((value & 0x0000FF0000000000) >> 40);
            byte b6 = (byte)((value & 0x00FF000000000000) >> 48);
            byte b7 = (byte)((value & 0xFF00000000000000) >> 56);
            UInt64 retVal = ((UInt64) b0 << 56) | ((UInt64) b1 << 48) | ((UInt64) b2 << 40) | ((UInt64) b3 << 32)
                        | ((UInt64) b4 << 24) | ((UInt64) b5 << 16) | ((UInt64) b6 << 8) | ((UInt64) b7 << 0);

            return retVal;
        }

        /// <summary>
        /// Parse the passed string of hexidecimal characters into a binary
        /// value.
        /// </summary>
        /// <remarks>
        /// This implementation allows the passed string to be prefixed with
        /// "0x".
        /// </remarks>
        /// <param name="s">
        /// The hex string to evaluate.
        /// </param>
        /// <returns>
        /// The byte array value of the passed hex string.
        /// </returns>
        public static byte[] ParseHex(string s)
        {
            if (s == null)
            {
                return new byte[0];
            }
            char[] charArray = s.ToCharArray();
            int    length    = charArray.Length;
            if (length == 0)
            {
                return new byte[0];
            }

            if ((length & 0x1) != 0)
            {
                throw new ArgumentException("invalid length hex string");
            }

            int offset = 0;
            if (charArray[1] == 'x' || charArray[1] == 'X')
            {
                offset = 2;
            }

            int    lenWithoutLeadingChars = (length - offset) / 2;
            byte[] result                 = new byte[lenWithoutLeadingChars];
            for (int i = 0; i < lenWithoutLeadingChars; ++i)
            {
                result[i] = (byte) (ParseHex(charArray[offset++]) << 4 | ParseHex(charArray[offset++]));
            }

            return result;
        }

        /// <summary>
        /// Return the integer value of a hexidecimal digit.
        /// </summary>
        /// <param name="ch">
        /// The hex character to evaluate.
        /// </param>
        /// <returns>
        /// The integer value of the passed hex character.
        /// </returns>
        public static int ParseHex(char ch)
        {
            switch (ch)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return ch - '0';

                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                    return ch - 'A' + 0x0A;

                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                    return ch - 'a' + 0x0A;

                default:
                    throw new ArgumentException("illegal hex char: " + ch);
            }
        }

        /// <summary>
        /// Convert a byte to the hex sequence of 2 hex digits.
        /// </summary>
        /// <param name="b">
        /// The byte.
        /// </param>
        /// <returns>
        /// The hex sequence.
        /// </returns>
        public static string ToHex(int b)
        {
            int    n   = b & 0xFF;
            char[] ach = new char[2];

            ach[0] = HEX[n >> 4];
            ach[1] = HEX[n & 0x0F];

            return new string(ach);
        }

        /// <summary>
        /// Convert a byte to a hex sequence of '0' + 'x' + 2 hex digits.
        /// </summary>
        /// <param name="b">
        /// The byte.
        /// </param>
        /// <returns>
        /// The hex sequence.
        /// </returns>
        public static string ToHexEscape(byte b)
        {
            int n = b & 0xFF;
            char[] chars = new char[4];

            chars[0] = '0';
            chars[1] = 'x';
            chars[2] = HEX[n >> 4];
            chars[3] = HEX[n & 0x0F];

            return new string(chars);
        }

        /// <summary>
        /// Convert a byte array to a hex sequence of '0' + 'x' + 2 hex
        /// digits per byte.
        /// </summary>
        /// <param name="bytes">
        /// The byte array.
        /// </param>
        /// <returns>
        /// The hex sequence.
        /// </returns>
        public static string ToHexEscape(byte[] bytes)
        {
            return ToHexEscape(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Convert a byte array to a hex sequence of '0' + 'x' + 2 hex
        /// digits per byte.
        /// </summary>
        /// <param name="bytes">
        /// The byte array.
        /// </param>
        /// <param name="offset">
        /// The offset into array.
        /// </param>
        /// <param name="bytesCount">
        /// The number of bytes to convert.
        /// </param>
        /// <returns>
        /// The hex sequence.
        /// </returns>
        public static string ToHexEscape(byte[] bytes, int offset, int bytesCount)
        {
            char[] charArray = new char[2 + bytesCount * 2];

            charArray[0] = '0';
            charArray[1] = 'x';

            for (int i = offset, ofch = 2, ofStop = offset + bytesCount; i < ofStop; ++i)
            {
                int n = bytes[i] & 0xFF;
                charArray[ofch++] = HEX[n >> 4];
                charArray[ofch++] = HEX[n & 0x0F];
            }

            return new string(charArray);
        }

        /// <summary>
        /// Obtain a <b>Random</b> object that can be used to get random
        /// values.
        /// </summary>
        /// <returns>
        /// A random number generator.
        /// </returns>
        public static Random GetRandom()
        {
            Random rnd = m_rnd;

            if (rnd == null)
            {
            // double-check locking is not required to work; the worst that
            // can happen is that we create a couple extra Random objects
            //lock (Random)

                rnd = m_rnd;
                if (rnd == null)
                {
                    rnd = new Random();

                    // spin the seed a bit
                    long stop = DateTimeUtils.GetSafeTimeMillis() + 31 + rnd.Next(31);
                    long min  = 1021 + rnd.Next(Math.Max(1, (int) (stop % 1021)));

                    while (DateTimeUtils.GetSafeTimeMillis() < stop || --min > 0)
                    {
                        min += rnd.Next(0, 2) == 1 ? 1 : -1;  //rnd.nextBoolean() ? 1 : -1;
                        rnd = new Random(rnd.Next());
                    }

                    // spin the random until the clock ticks again
                    long start = DateTimeUtils.GetSafeTimeMillis();
                    do
                    {
                        if (Convert.ToBoolean(rnd.Next(0, 2)))
                        {
                            if ((rnd.Next() & 0x01) == (DateTimeUtils.GetSafeTimeMillis() & 0x01))
                            {
                                rnd.Next(0, 2);
                            }
                        }
                    }
                    while (DateTimeUtils.GetSafeTimeMillis() == start);

                    m_rnd = rnd;
                }
            }

            return rnd;
        }

        /// <summary>
        /// Returns <b>true</b> if specified object is one of .NET supported
        /// numeric types:
        /// <list type="bullet">
        /// <item>byte</item>
        /// <item>short</item>
        /// <item>int</item>
        /// <item>long</item>
        /// <item>double</item>
        /// <item>float</item>
        /// <item>decimal</item>
        /// </list>
        /// </summary>
        /// <param name="num">
        /// An object being tested.
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="num"/> is numeric, <b>false</b>
        /// otherwise.
        /// </returns>
        public static bool IsNumber(object num)
        {
            return num is byte ||
                   num is short ||
                   num is int ||
                   num is long ||
                   num is double ||
                   num is float ||
                   num is decimal;
        }

        #region CRC

        /// <summary>
        /// Calculate a CRC32 value from a byte array.
        /// </summary>
        /// <param name="ab">
        /// An array of bytes.
        /// </param>
        /// <returns>
        /// The 32-bit CRC value.
        /// </returns>
        public static uint ToCrc(byte[] ab)
        {
            return ToCrc(ab, 0, ab.Length);
        }

        /// <summary>
        /// Calculate a CRC32 value from a portion of a byte array.
        /// </summary>
        /// <param name="ab">
        /// An array of bytes.
        /// </param>
        /// <param name="of">
        /// The offset into the array.
        /// </param>
        /// <param name="cb">
        /// The number of bytes to evaluate.
        /// </param>
        /// <returns>
        /// The 32-bit CRC value.
        /// </returns>
        public static uint ToCrc(byte[] ab, int of, int cb)
        {
            return ToCrc(ab, of, cb, 0xFFFFFFFF);
        }

        /// <summary>
        /// Continue to calculate a CRC32 value from a portion of a byte
        /// array.
        /// </summary>
        /// <param name="ab">
        /// An array of bytes.
        /// </param>
        /// <param name="of">
        /// The offset into the array.
        /// </param>
        /// <param name="cb">
        /// The number of bytes to evaluate.
        /// </param>
        /// <param name="crc">
        /// The previous CRC value.
        /// </param>
        /// <returns>
        /// The 32-bit CRC value.
        /// </returns>
        public static uint ToCrc(byte[] ab, int of, int cb, uint crc)
        {
            while (cb > 0)
            {
                crc = (crc >> 8) ^ CRC32_TABLE[(crc ^ ab[of++]) & 0xFF];
                --cb;
            }
            return crc;
        }

        private static readonly uint CRC32_BASE  = 0xEDB88320;
        private static readonly uint[] CRC32_TABLE = new uint[256];

        #endregion

        // Hex digits.
        private static readonly char[] HEX = "0123456789ABCDEF".ToCharArray();

        //A lazily-instantiated shared Random object.
        private static Random m_rnd;
    }
}