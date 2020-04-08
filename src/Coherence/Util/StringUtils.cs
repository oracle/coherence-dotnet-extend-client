/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Globalization;
using System.Text;

namespace Tangosol.Util
{
    /// <summary>
    /// Miscellaneuos utility methods for string manipulation.
    /// </summary>
    /// <author>Aleksandar Seovic  2006.08.09</author>
    public abstract class StringUtils
    {
        /// <summary>
        /// Returns <b>true</b> if specified string is <c>null</c> or empty,
        /// <b>false</b> otherwise.
        /// </summary>
        /// <param name="stringValue">
        /// Value to check.
        /// </param>
        /// <returns>
        /// <b>true</b> if specified string is <c>null</c> or empty,
        /// <b>false</b> otherwise.
        /// </returns>
        public static bool IsNullOrEmpty(string stringValue)
        {
            return string.IsNullOrEmpty(stringValue);
        }

        /// <summary>
        /// Create a String of the specified length containing the specified
        /// character.
        /// </summary>
        /// <param name="ch">
        /// The character to fill the String with.
        /// </param>
        /// <param name="length">
        /// The length of the String.
        /// </param>
        /// <returns>
        /// A String containing the character <paramref name="ch"/> repeated
        /// <paramref name="length"/> times.
        /// </returns>
        public static string Dup(char ch, int length)
        {
            var ach = new char[length];
            for (int of = 0; of < length; ++of)
            {
                ach[of] = ch;
            }
            return new string(ach);
        }

        /// <summary>
        /// Create a String which is a duplicate of the specified number of
        /// the passed String.
        /// </summary>
        /// <param name="text">
        /// The String to fill the new String with.
        /// </param>
        /// <param name="count">
        /// The number of duplicates to put into the new String.
        /// </param>
        /// <returns>
        /// A String containing the String <paramref name="text"/> repeated
        /// <paramref name="count"/> times.
        /// </returns>
        public static string Dup(string text, int count)
        {
            if (count < 1)
            {
                return String.Empty;
            }
            if (count == 1)
            {
                return text;
            }

            char[] achPat = text.ToCharArray();
            int    cchPat = achPat.Length;
            int    cchBuf = cchPat * count;
            var    achBuf = new char[cchBuf];
            for (int i = 0, of = 0; i < count; ++i, of += cchPat)
            {
                Array.Copy(achPat, 0, achBuf, of, cchPat);
            }
            return new string(achBuf);
        }

        /// <summary>
        /// Format the passed memory size (in bytes) as a String.
        /// </summary>
        /// <remarks>
        /// This method will possibly round the memory size for purposes of
        /// producing a more-easily read String value unless the
        /// <paramref name="isExact"/> parameter is passed as <b>true</b>.
        /// </remarks>
        /// <param name="memorySize">
        /// The number of bytes of memory.
        /// </param>
        /// <param name="isExact">
        /// <b>true</b> if the String representation must be exact, or
        /// <b>false</b> if it can be an approximation.
        /// </param>
        /// <returns>
        /// A String representation of the given memory size.
        /// </returns>
        public static string ToMemorySizeString(long memorySize, bool isExact)
        {
            if (memorySize < 0)
            {
                throw new ArgumentException("negative quantity: " + memorySize);
            }

            if (memorySize < 1024)
            {
                return memorySize.ToString();
            }

            int divs    = 0;
            int maxDivs = MEM_SUFFIX.Length - 1;

            if (isExact)
            {
                // kilobytes? megabytes? gigabytes? terabytes?
                while (((((int) memorySize) & KB_MASK) == 0) && divs < maxDivs)
                {
                    memorySize = NumberUtils.URShift(memorySize, 10);
                    ++divs;
                }
                return memorySize + MEM_SUFFIX[divs];
            }

            // need roughly the 3 most significant decimal digits
            int rem = 0;
            while (memorySize >= KB && divs < maxDivs)
            {
                rem = ((int) memorySize) & KB_MASK;
                memorySize = NumberUtils.URShift(memorySize, 10);
                ++divs;
            }

            var sb = new StringBuilder();
            sb.Append(memorySize.ToString());
            int len = sb.Length;
            if (len < 3 && rem != 0)
            {
                // need the first digit or two of string value of rem / 1024;
                // format the most significant two digits ".xx" as a string "1xx"
                string dec = ((int) (rem / 10.24 + 100)).ToString();
                sb.Append('.')
                  .Append(dec.Substring(1, 3 - len));
            }
            sb.Append(MEM_SUFFIX[divs]);

            return sb.ToString();
        }

        /// <summary>
        /// Format the passed bandwidth (in bytes per second) as a String.
        /// </summary>
        /// <remarks>
        /// This method will possibly round the memory size for purposes of
        /// producing a more-easily read String value unless the
        /// <paramref name="isExact"/> parameter is passed as <b>true</b>.
        /// </remarks>
        /// <param name="bps">
        /// The number of bytes per second.
        /// </param>
        /// <param name="isExact">
        /// <b>true</b> if the String representation must be exact, or
        /// <b>false</b> if it can be an approximation.
        /// </param>
        /// <returns>
        /// A String representation of the given bandwidth.
        /// </returns>
        public static string ToBandwidthString(long bps, bool isExact)
        {
            bool bits = (bps & 0xF00000000000000L) == 0L;

            if (bits)
            {
                bps <<= 3;
            }

            var sb = new StringBuilder(ToMemorySizeString(bps, isExact));
            int ofLast = sb.Length - 1;
            if (sb[ofLast] == 'B')
            {
                if (bits)
                {
                    sb[ofLast] = 'b';
                }
            }
            else
            {
                sb.Append(bits ? 'b' : 'B');
            }
            sb.Append("ps");

            return sb.ToString();
        }

        /// <summary>
        /// Format the passed integer as a fixed-length decimal string.
        /// </summary>
        /// <param name="n">
        /// The integer value.
        /// </param>
        /// <param name="digits">
        /// The length of the resulting decimal string.
        /// </param>
        /// <returns>
        /// The decimal value formated to the specified length string.
        /// </returns>
        public static string ToDecString(int n, int digits)
        {
            var ach = new char[digits];
            while (digits > 0)
            {
                ach[--digits] = (char) ('0' + n % 10);
                n /= 10;
            }

            return new string(ach);
        }

        /// <summary>
        /// Breaks the specified string into a multi-line string.
        /// </summary>
        /// <param name="text">
        /// The string to break.
        /// </param>
        /// <param name="width">
        /// The max width of resulting lines (including the indent).
        /// </param>
        /// <param name="indent">
        /// A string used to indent each line.
        /// </param>
        /// <returns>
        /// The string, broken and indented.
        /// </returns>
        public static string BreakLines(string text, int width, string indent)
        {
            return BreakLines(text, width, indent, true);
        }

        /// <summary>
        /// Breaks the specified string into a multi-line string.
        /// </summary>
        /// <param name="text">
        /// The string to break.
        /// </param>
        /// <param name="width">
        /// The max width of resulting lines (including the indent).
        /// </param>
        /// <param name="indent">
        /// A string used to indent each line.
        /// </param>
        /// <param name="isFirstLine">
        /// If <b>true</b> indents all lines; otherwise indents all but the
        /// first.
        /// </param>
        /// <returns>
        /// The string, broken and indented.
        /// </returns>
        public static string BreakLines(string text, int width, string indent, bool isFirstLine)
        {
            if (indent == null)
            {
                indent = "";
            }

            width -= indent.Length;
            if (width <= 0)
            {
                throw new ArgumentException("The width and indent are incompatible");
            }

            char[] ach = text.ToCharArray();
            int    cch = ach.Length;

            var sb = new StringBuilder(cch);

            int ofPrev = 0;
            int of     = 0;

            while (of < cch)
            {
                char c = ach[of++];

                bool isBreak = false;
                int  ofBreak = of;
                int  ofNext  = of;

                if (c == '\n')
                {
                    isBreak = true;
                    ofBreak--;
                }
                else if (of == cch)
                {
                    isBreak = true;
                }
                else if (of == ofPrev + width)
                {
                    isBreak = true;

                    while (!char.IsWhiteSpace(ach[--ofBreak]) && ofBreak > ofPrev)
                    {}
                    if (ofBreak == ofPrev)
                    {
                        ofBreak = of; // no spaces -- force the break
                    }
                    else
                    {
                        ofNext = ofBreak + 1;
                    }
                }

                if (isBreak)
                {
                    if (ofPrev > 0)
                    {
                        sb.Append('\n').Append(indent);
                    }
                    else if (isFirstLine)
                    {
                        sb.Append(indent);
                    }

                    sb.Append(text.Substring(ofPrev, ofBreak - ofPrev));

                    ofPrev = ofNext;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Format a char to a printable escape if necessary.
        /// </summary>
        /// <param name="ch">
        /// The char.
        /// </param>
        /// <returns>
        /// A printable string representing the passed char.
        /// </returns>
        public static string ToCharEscape(char ch)
        {
            var ach = new char[6];
            int cch = Escape(ch, ach, 0);
            return new string(ach, 0, cch);
        }

        /// <summary>
        /// Format a char to a printable escape if necessary, putting the
        /// result into the passed array.
        /// </summary>
        /// <remarks>
        /// The array must be large enough to accept six characters.
        /// </remarks>
        /// <param name="ch">
        /// The character to format.
        /// </param>
        /// <param name="ach">
        /// The array of characters to format into.
        /// </param>
        /// <param name="of">
        /// The offset in the array to format at.
        /// </param>
        /// <returns>
        /// The number of characters used to format the char.
        /// </returns>
        public static int Escape(char ch, char[] ach, int of)
        {
            switch (ch)
            {
                case '\b':
                    ach[of++] = '\\';
                    ach[of]   = 'b';
                    return 2;
                case '\t':
                    ach[of++] = '\\';
                    ach[of]   = 't';
                    return 2;
                case '\n':
                    ach[of++] = '\\';
                    ach[of]   = 'n';
                    return 2;
                case '\f':
                    ach[of++] = '\\';
                    ach[of]   = 'f';
                    return 2;
                case '\r':
                    ach[of++] = '\\';
                    ach[of]   = 'r';
                    return 2;
                case '\"':
                    ach[of++] = '\\';
                    ach[of]   = '\"';
                    return 2;
                case '\'':
                    ach[of++] = '\\';
                    ach[of]   = '\'';
                    return 2;
                case '\\':
                    ach[of++] = '\\';
                    ach[of]   = '\\';
                    return 2;

                default:
                    switch (char.GetUnicodeCategory(ch))
                    {
                        case UnicodeCategory.Control:
                            if (ch <= 0xFF)
                            {
                                ach[of++] = '\\';
                                ach[of++] = '0';
                                ach[of++] = (char)(ch / 8 + '0');
                                ach[of]   = (char)(ch % 8 + '0');
                                return 4;
                            }
                            goto case UnicodeCategory.PrivateUse;

                        case UnicodeCategory.PrivateUse:
                        case UnicodeCategory.OtherNotAssigned:
                            int n = ch;
                            ach[of++] = '\\';
                            ach[of++] = 'u';
                            ach[of++] = HEX_DIGIT[n >> 12];
                            ach[of++] = HEX_DIGIT[n >> 8 & 0x0F];
                            ach[of++] = HEX_DIGIT[n >> 4 & 0x0F];
                            ach[of]   = HEX_DIGIT[n & 0x0F];
                            return 6;
                    }
                    break;
            }

            // character does not need to be escaped
            ach[of] = ch;
            return 1;
        }

        /// <summary>
        /// Convert a byte array to a hex string of 2 hex digits per byte.
        /// </summary>
        /// <param name="array">
        /// The byte array to convert.
        /// </param>
        /// <returns>
        /// The hex string.
        /// </returns>
        public static string ByteArrayToHexString(byte[] array)
        {
            if (array == null)
            {
                return null;
            }
            int cb     = array.Length;
            var result = new char[cb * 2];
            for (int ofb = 0, ofch = 0; ofb < cb; ++ofb)
            {
                int n = array[ofb] & 0xFF;
                result[ofch++] = HEX_DIGIT[n >> 4];
                result[ofch++] = HEX_DIGIT[n & 0x0F];
            }
            return new string(result);
        }

        /// <summary>
        /// Convert a hex string to a byte array.
        /// </summary>
        /// <param name="hexString">
        /// The hex string to convert.
        /// </param>
        /// <returns>
        /// The byte array.
        /// </returns>
        public static byte[] HexStringToByteArray(string hexString)
        {
            if (hexString == null)
            {
                return null;
            }

            char[] array  = hexString.ToCharArray();
            int    cch    = array.Length;
            int    cb     = cch / 2;
            var    result = new byte[cb];

            for (int ofb = 0, ofch = 0; ofch < cch; ++ofb)
            {
                char ch1 = Char.ToUpper(array[ofch++]);
                char ch2 = Char.ToUpper(array[ofch++]);
                int  n1  = ch1 - '0';
                int  n2  = ch2 - '0';

                result[ofb] = (byte)((HEX_VALUE[n1] << 4) | HEX_VALUE[n2]);
            }
            return result;
        }

        /// <summary>
        /// Convert a .NET Version object to an Oracle version string.
        /// Oracle version number can have 5 numbers (N.N.N.N.N) while .NET
        /// version number can only have up to 4 numbers (N.N.N.N).  So to
        /// represent Oracle version in .NET version format, the 4th .NET
        /// version number is a combination of the 4th and 5th Oracle version
        /// numbers as follows:
        /// 
        /// 4th .NET number = 4th Oracle number * 1000 + 5th Oracle number;
        /// 
        /// 4th Oracle number = int (4th .NET number / 1000);
        /// 5th Oracle number = 4th .NET number - 4th Oracle number * 1000;
        /// 
        /// e.g.
        /// 12.1.2.1    (.NET) ==> 12.1.2.0.1 (Oracle)
        /// 12.1.2.1001 (.NET) ==> 12.1.2.1.1 (Oracle)
        /// 12.1.2      (.NET) ==> 12.1.2.0.0 (Oracle)
        /// </summary>
        /// <param name="dotnetVersion">
        /// The .NET Version object.
        /// </param>
        /// <returns>
        /// The Oracle version string.
        /// </returns>
        public static string ToOracleVersion(Version dotnetVersion)
        {
            if (dotnetVersion == null)
            {
                return null;
            }

            int    dotnetNumber4 = dotnetVersion.Revision < 0 ? 0 : dotnetVersion.Revision;
            int    oracleNumber4 = dotnetNumber4 / 1000;
            int    oracleNumber5 = dotnetNumber4 - oracleNumber4 * 1000;
            return dotnetVersion.Major + "." + dotnetVersion.Minor + "." + dotnetVersion.Build + "." + oracleNumber4 + "." + oracleNumber5;
        }


        #region Memory size constants

        private const int                KB         = 1 << 10;
        private const int                KB_MASK    = KB - 1;
        private static readonly string[] MEM_SUFFIX = new[] {"", "KB", "MB", "GB", "TB"};

        #endregion

        #region Hex digits and values

        private static readonly char[] HEX_DIGIT = "0123456789ABCDEF".ToCharArray();
        private static readonly byte[] HEX_VALUE = new byte[] { 
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x0B, 0x0C, 
                0x0D, 0x0E, 0x0F };

        #endregion
    }
}