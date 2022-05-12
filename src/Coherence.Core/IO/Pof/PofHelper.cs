/*
 * Copyright (c) 2000, 2022, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;

using Tangosol.Util;
using Tangosol.Util.Collections;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// Collection of helper methods for POF streams.
    /// </summary>
    /// <author>Cameron Purdy/Jason Howes  2006.07.17</author>
    /// <author>Ivan Cikic 2006.08.09</author>
    /// <since>Coherence 3.2</since>
    public abstract class PofHelper
    {
        #region Type conversion

        /// <summary>
        /// Returns an identifier that represents the .NET type of the
        /// specified object.
        /// </summary>
        /// <param name="obj">
        /// An object to determine the type of.
        /// </param>
        /// <param name="ctx">
        /// The <see cref="IPofContext"/> used to determine if the object is
        /// an instance of a valid user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// One of the <see cref="PofConstants"/> struct <b>N_*</b>
        /// constants.
        /// </returns>
        public static int GetDotNetTypeId(object obj, IPofContext ctx)
        {
            Debug.Assert(ctx != null);

            return obj == null                                      ? PofConstants.N_NULL
                    : obj is IPortableObject                        ? PofConstants.N_USER_TYPE
                    : obj is string                                 ? PofConstants.N_STRING
                    : obj is Int32                                  ? PofConstants.N_INT32
                    : obj is Int64                                  ? PofConstants.N_INT64
                    : obj is Double                                 ? PofConstants.N_DOUBLE
                    : obj is Decimal                                ? PofConstants.N_DECIMAL
                    : obj is Int16                                  ? PofConstants.N_INT16
                    : obj is Single                                 ? PofConstants.N_SINGLE
                    : obj is Byte                                   ? PofConstants.N_BYTE
                    : obj is Boolean                                ? PofConstants.N_BOOLEAN
                    : obj is Char                                   ? PofConstants.N_CHARACTER
                    : obj is Binary                                 ? PofConstants.N_BINARY
                    : obj.GetType().IsArray ?
                          obj is Byte[]                             ? PofConstants.N_BYTE_ARRAY
                        : obj is Int32[]                            ? PofConstants.N_INT32_ARRAY
                        : obj is Int64[]                            ? PofConstants.N_INT64_ARRAY
                        : obj is Double[]                           ? PofConstants.N_DOUBLE_ARRAY
                        : obj is Char[]                             ? PofConstants.N_CHAR_ARRAY
                        : obj is Boolean[]                          ? PofConstants.N_BOOLEAN_ARRAY
                        : obj is Int16[]                            ? PofConstants.N_INT16_ARRAY
                        : obj is Single[]                           ? PofConstants.N_SINGLE_ARRAY
                        : ctx.IsUserType(obj)                       ? PofConstants.N_USER_TYPE
                        :                                             PofConstants.N_OBJECT_ARRAY
                    // final POF-primitive types MUST be checked before customizable
                    // serialization (IsUserType); additionally, IsUserType is a
                    // potentially expensive call, and it is thus desirable to avoid
                    // it where possible
                    : ctx.IsUserType(obj)                           ? PofConstants.N_USER_TYPE
                    : obj is IDictionary                            ? PofConstants.N_DICTIONARY
                    : obj is ICollection                            ? PofConstants.N_COLLECTION
                    : obj is DateTime                               ? PofConstants.N_DATETIME
                    : obj is DateTime &&
                      ((DateTime) obj).TimeOfDay == TimeSpan.Zero   ? PofConstants.N_DATE
                    : obj is TimeSpan && ((TimeSpan) obj).Days == 0 ? PofConstants.N_TIME_INTERVAL
                    : obj is TimeSpan                               ? PofConstants.N_DAY_TIME_INTERVAL
                    : obj is ILongArray                             ? PofConstants.N_SPARSE_ARRAY
                    : obj is RawInt128                              ? PofConstants.N_INT128
                    : obj is RawTime                                ? PofConstants.N_TIME
                    : obj is RawDateTime                            ? PofConstants.N_DATETIME
                    : obj is RawYearMonthInterval                   ? PofConstants.N_YEAR_MONTH_INTERVAL
                    :                                                 PofConstants.N_USER_TYPE;
        }

        /// <summary>
        /// Return an identifier that represents the POF type of the
        /// specified type.
        /// </summary>
        /// <param name="type">
        /// The type; must not be <c>null</c>.
        /// </param>
        /// <param name="ctx">
        /// The <see cref="IPofContext"/> used to determine the type
        /// identifier of a user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// One of the <see cref="PofConstants"/> struct <b>T_*</b>
        /// constants.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the user type associated with the given object is unknown to
        /// the specified <see cref="IPofContext"/>.
        /// </exception>
        public static int GetPofTypeId(Type type, IPofContext ctx)
        {
            Debug.Assert(type != null);
            Debug.Assert(ctx  != null);

            object pofType = DOTNET_TO_POF_TYPE[type];

            return pofType != null                                    ? (int) pofType
                : typeof(IPortableObject)     .IsAssignableFrom(type) ? ctx.GetUserTypeIdentifier(type)
                : ctx.IsUserType(type)                                ? ctx.GetUserTypeIdentifier(type)
                : type.IsArray                                        ? PofConstants.T_ARRAY
                : typeof(IDictionary)         .IsAssignableFrom(type) ? PofConstants.T_MAP
                : typeof(ICollection)         .IsAssignableFrom(type) ? PofConstants.T_COLLECTION
                : typeof(RawTime)             .IsAssignableFrom(type) ? PofConstants.T_TIME
                : typeof(RawDateTime)         .IsAssignableFrom(type) ? PofConstants.T_DATETIME
                : typeof(RawYearMonthInterval).IsAssignableFrom(type) ? PofConstants.T_YEAR_MONTH_INTERVAL
                :                                                       ctx.GetUserTypeIdentifier(type);
        }

        /// <summary>
        /// Returns a .NET Type based on the POF type identifer.
        /// </summary>
        /// <param name="pofTypeId">
        /// POF type identifier.
        /// </param>
        /// <returns>
        /// A .NET Type for the specified POF type identifier.
        /// </returns>
        public static Type GetDotNetType(int pofTypeId)
        {
            Type dotNetType = (Type) POF_TO_DOTNET_TYPE[pofTypeId];
            if (dotNetType != null)
            {
                return dotNetType;
            }

            return typeof(object);
        }

        /// <summary>
        /// Determine if the given type can be represented as an intrinsic
        /// POF type.
        /// </summary>
        /// <param name="type">
        /// The object type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> if the given type can be represented as an intrinsic POF 
        /// type; <b>false</b>, otherwise.
        /// </returns>
        public static bool IsIntrinsicPofType(Type type)
        {
            Debug.Assert(type != null);

            object pofType = DOTNET_TO_POF_TYPE[type];
            return pofType != null
                   || typeof (IDictionary).IsAssignableFrom(type)
                   || typeof (ICollection).IsAssignableFrom(type)
                   || typeof (RawTime).IsAssignableFrom(type)
                   || typeof (RawDateTime).IsAssignableFrom(type)
                   || typeof (RawYearMonthInterval).IsAssignableFrom(type);    
        }

        /// <summary>
        /// Convert the passed number to the specified type.
        /// </summary>
        /// <param name="number">
        /// The number to convert.
        /// </param>
        /// <param name="dotNetTypeId">
        /// The .NET type ID to convert to, one of the
        /// <see cref="PofConstants"/> struct <b>T_*</b> constants.
        /// </param>
        /// <returns>
        /// The number converted to the specified type.
        /// </returns>
        public static object ConvertNumber(object number, int dotNetTypeId)
        {
            if (number == null)
            {
                return null;
            }

            switch (dotNetTypeId)
            {
                case PofConstants.N_BYTE:
                    return number is Byte ? number : Convert.ToByte(number);

                case PofConstants.N_INT16:
                    return number is Int16 ? number : Convert.ToInt16(number);

                case PofConstants.N_INT32:
                    return number is Int32 ? number : Convert.ToInt32(number);

                case PofConstants.N_INT64:
                    return number is Int64 ? number : Convert.ToInt64(number);

                case PofConstants.N_DECIMAL:
                    return number is Decimal ? number : Convert.ToDecimal(number);

                case PofConstants.N_SINGLE:
                    return number is Single ? number : Convert.ToSingle(number);

                case PofConstants.N_DOUBLE:
                    return number is Double ? number : Convert.ToDouble(number);

                default:
                    throw new ArgumentException(".NET type ID " + dotNetTypeId + " is not a number type");
            }
        }

        /// <summary>
        /// Expand the passed array to contain the specified number of
        /// elements.
        /// </summary>
        /// <param name="array">
        /// The "template" array or <c>null</c>.
        /// </param>
        /// <param name="newSize">
        /// The number of desired elements in the new array.
        /// </param>
        /// <returns>
        /// The old array, if it was big enough, or a new array of the same
        /// type.
        /// </returns>
        public static Array ResizeArray(Array array, int newSize)
        {
            return ResizeArray(array, newSize, array == null 
                                                ? typeof(object) 
                                                : array.GetType().GetElementType());
        }

        /// <summary>
        /// Expand the passed array to contain the specified number of
        /// elements.
        /// </summary>
        /// <param name="array">
        /// The "template" array or <c>null</c>.
        /// </param>
        /// <param name="newSize">
        /// The number of desired elements in the new array.
        /// </param>
        /// <param name="elementType">
        /// Type of the array elements.
        /// </param>
        /// <returns>
        /// The old array, if it was big enough, or a new array of the same
        /// type.
        /// </returns>
        public static Array ResizeArray(Array array, int newSize, Type elementType)
        {
            Array newArray;

            if (array == null)
            {
                newArray = Array.CreateInstance(elementType, newSize);
            }
            else
            {
                if (newSize > array.Length)
                {
                    newArray = Array.CreateInstance(array.GetType().GetElementType(), newSize);
                }
                else
                {
                    newArray = array;
                }
            }

            return newArray;
        }

        #endregion

        #region Parsing

        // TODO: add support for RawQuad

        /// <summary>
        /// Decode an integer value from one of the reserved single-byte
        /// combined type and value indicators.
        /// </summary>
        /// <param name="n">
        /// The integer value that the integer is encoded as.
        /// </param>
        /// <returns>
        /// An integer between -1 and 22, inclusive.
        /// </returns>
        public static int DecodeTinyInt(int n)
        {
            Debug.Assert(n <= PofConstants.V_INT_NEG_1 && n >= PofConstants.V_INT_22);
            return PofConstants.V_INT_0 - n;
        }

        /// <summary>
        /// Read a <b>Char</b> value from the passed
        /// <see cref="DataReader"/>.
        /// </summary>
        /// <param name="reader">
        /// The <b>DataReader</b> object to read from.
        /// </param>
        /// <returns>
        /// A char value.
        /// </returns>
        public static char ReadChar(DataReader reader)
        {
            // int ch = reader.PeekChar();
            // if (ch == 65533)    // Unicode replacement character
            // {
            //     ch = reader.ReadByte();
            //     int ch1 = ch & 0xFF;
            //     switch ((ch1 & 0xF0) >> 4)
            //     {
            //         case 0xC:
            //         case 0xD:
            //             {
            //                 // 2-byte format:  110x xxxx, 10xx xxxx
            //                 int ch2 = reader.ReadByte() & 0xFF;
            //                 if ((ch2 & 0xC0) != 0x80)
            //                 {
            //                     throw new ArgumentException(
            //                             "illegal leading UTF byte: " + ch2);
            //                 }
            //                 ch = (char)(((ch1 & 0x1F) << 6) | ch2 & 0x3F);
            //                 break;
            //             }
            //
            //         case 0xE:
            //             {
            //                 // 3-byte format:  1110 xxxx, 10xx xxxx, 10xx xxxx
            //                 int ch2 = reader.ReadByte() & 0xFF;
            //                 int ch3 = reader.ReadByte() & 0xFF;
            //                 if ((ch2 & 0xC0) != 0x80 || (ch3 & 0xC0) != 0x80)
            //                 {
            //                     throw new ArgumentException(
            //                             "illegal leading UTF bytes: " + ch2 + ", " + ch3);
            //                 }
            //                 ch = (char)(((ch & 0x0F) << 12) |
            //                             ((ch2 & 0x3F) << 6) |
            //                             ((ch3 & 0x3F)));
            //                 break;
            //             }
            //
            //         default:
            //             throw new ArgumentException(
            //                     "illegal leading UTF byte: " + ch);
            //     }
            //
            //     return (char) ch;
            // }

            return reader.ReadChar();
        }

        /// <summary>
        /// Read a literal <b>DateTime</b> value from a POF stream.
        /// </summary>
        /// <param name="reader">
        /// The stream containing the POF date value.
        /// </param>
        /// <returns>A literal date value.</returns>
        public static DateTime ReadDate(DataReader reader)
        {
            int year  = reader.ReadPackedInt32();
            int month = reader.ReadPackedInt32();
            int day   = reader.ReadPackedInt32();
            return new DateTime(year, month, day);
        }

        /// <summary>
        /// Read <b>Decimal</b> value from <b>DataReader</b>.
        /// </summary>
        /// <param name="reader">
        /// DataReader stream to read value from.
        /// </param>
        /// <returns>
        /// Decimal value read from stream.
        /// </returns>
        public static Decimal ReadDecimal(DataReader reader)
        {
            RawInt128 rawInt128 = reader.ReadPackedRawInt128(reader);
            int       scale     = reader.ReadPackedInt32();
         
            return rawInt128.ToDecimal((byte) scale);
        }

        /// <summary>
        /// Read <b>Decimal</b> value from <b>DataReader</b>.
        /// </summary>
        /// <param name="reader">
        /// DataReader stream to read value from.
        /// </param>
        /// <param name="size">
        /// Number of bytes to read from the stream.
        /// </param>
        /// <returns>
        /// Decimal value read from stream.
        /// </returns>
        public static Decimal ReadDecimal(DataReader reader, int size)
        {
            if (size == 4)
            {
                Int32 i32   = reader.ReadPackedInt32();
                Int32 scale = reader.ReadPackedInt32();
                return new Decimal(Math.Abs(i32), 0, 0, (i32 < 0), (byte)scale);
            }
            else if (size == 8)
            {
                Int64 i64   = reader.ReadPackedInt64();
                Int32 scale = reader.ReadPackedInt32();
                int[] bits    = Decimal.GetBits(new Decimal(i64));
                bool negative = (bits[3] & 0x80000000) != 0 ? true : false;
                return new Decimal(bits[0], bits[1], bits[2], negative, (byte)scale);
            }
            else 
            {
                RawInt128 rawInt128 = reader.ReadPackedRawInt128(reader);
                int       scale     = reader.ReadPackedInt32();
                return rawInt128.ToDecimal((byte) scale);
            }
        }

        /// <summary>
        /// Read a <see cref="RawTime"/> value from a POF stream.
        /// </summary>
        /// <param name="reader">
        /// The stream containing the POF time value.
        /// </param>
        /// <returns>
        /// A literal <b>Time</b> value.
        /// </returns>
        public static RawTime ReadRawTime(DataReader reader)
        {
            RawTime time;
            int hour     = reader.ReadPackedInt32();
            int minute   = reader.ReadPackedInt32();
            int second   = reader.ReadPackedInt32();
            int fraction = reader.ReadPackedInt32();
            int nanos    = fraction <= 0 ? -fraction : fraction * 1000000;

            int zoneType = reader.ReadPackedInt32();
            if (zoneType == 2)
            {
                int hourOffset   = reader.ReadPackedInt32();
                int minuteOffset = reader.ReadPackedInt32();
                time = new RawTime(hour, minute, second, nanos, hourOffset, minuteOffset);
            }
            else
            {
                Debug.Assert(zoneType == 0 || zoneType == 1);
                bool isUTC = zoneType == 1;
                time = new RawTime(hour, minute, second, nanos, isUTC);
            }
            return time;
        }

        /// <summary>
        /// Read a literal <b>DateTime</b> value from a POF stream.
        /// </summary>
        /// <remarks>
        /// This method will ignore any time zone information (if present),
        /// and return literal date-time value, as encoded in the stream.
        /// </remarks>
        /// <param name="reader">
        /// The stream containing the POF date-time value.
        /// </param>
        /// <returns>
        /// A literal <b>DateTime</b> value.
        /// </returns>
        public static DateTime ReadDateTime(DataReader reader)
        {
            int year     = reader.ReadPackedInt32();
            int month    = reader.ReadPackedInt32();
            int day      = reader.ReadPackedInt32();
            int hour     = reader.ReadPackedInt32();
            int minute   = reader.ReadPackedInt32();
            int second   = reader.ReadPackedInt32();
            int fraction = reader.ReadPackedInt32();
            int nanos = fraction <= 0 ? -fraction : fraction * 1000000;
            DateTime dateTime = new DateTime(year, month, day, hour, minute, second, nanos / 1000000);

            int zoneType = reader.ReadPackedInt32();
            if (zoneType == 2)
            {
                SkipPackedInts(reader, 2);
            }

            return dateTime;
        }

        /// <summary>
        /// Read a UTC <b>DateTime</b> value from a POF stream.
        /// </summary>
        /// <remarks>
        /// This method will use time zone information (if present) to
        /// determine a UTC value of the encoded POF date-time value.
        /// </remarks>
        /// <param name="reader">
        /// The stream containing the POF date-time value.
        /// </param>
        /// <returns>
        /// A UTC <b>DateTime</b> value.
        /// </returns>
        public static DateTime ReadUniversalDateTime(DataReader reader)
        {
            int year     = reader.ReadPackedInt32();
            int month    = reader.ReadPackedInt32();
            int day      = reader.ReadPackedInt32();
            int hour     = reader.ReadPackedInt32();
            int minute   = reader.ReadPackedInt32();
            int second   = reader.ReadPackedInt32();
            int fraction = reader.ReadPackedInt32();
            int nanos    = fraction <= 0 ? -fraction : fraction * 1000000;
            DateTime dateTime = new DateTime(year, month, day, hour, minute, second, nanos / 1000000, DateTimeKind.Utc);
            int zoneType = reader.ReadPackedInt32();
            if (zoneType == 2)
            {
                TimeSpan zoneOffest = new TimeSpan(reader.ReadPackedInt32(), reader.ReadPackedInt32(), 0);
                dateTime = DateTime.SpecifyKind(dateTime.Subtract(zoneOffest), DateTimeKind.Utc);
            }

            return dateTime;
        }

        /// <summary>
        /// Read a value of the specified encoding from the POF stream and
        /// convert it to a char.
        /// </summary>
        /// <param name="reader">
        /// The POF stream containing the value.
        /// </param>
        /// <param name="typeId">
        /// The POF type of the value.
        /// </param>
        /// <returns>
        /// The POF value as a char.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs reading the POF stream, or the POF value
        /// cannot be coerced to a char value.
        /// </exception>
        public static char ReadAsChar(DataReader reader, int typeId)
        {
            switch (typeId)
            {
                case PofConstants.T_OCTET:
                    return (char) reader.ReadByte();


                case PofConstants.T_CHAR:
                    return ReadChar(reader);


                default:
                    return (char) ReadAsInt32(reader, typeId);
            }
        }

        /// <summary>
        /// Read a value of the specified encoding from the POF stream and
        /// convert it to an <b>Int32</b>.
        /// </summary>
        /// <param name="reader">
        /// The POF stream containing the value.
        /// </param>
        /// <param name="typeId">
        /// The POF type of the value.
        /// </param>
        /// <returns>
        /// The POF value as an <b>Int32</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs reading the POF stream, or the POF value
        /// cannot be coerced to an <b>Int32</b> value.
        /// </exception>
        public static Int32 ReadAsInt32(DataReader reader, int typeId)
        {
            switch (typeId)
            {
                case PofConstants.T_BOOLEAN:
                case PofConstants.T_INT16:
                case PofConstants.T_INT32:
                case PofConstants.T_INT64:
                case PofConstants.T_INT128:
                    return reader.ReadPackedInt32();

                case PofConstants.T_FLOAT32:
                    return (int) reader.ReadSingle();

                case PofConstants.T_FLOAT64:
                    return (int) reader.ReadDouble();

                case PofConstants.T_FLOAT128:
                    throw new NotSupportedException("T_FLOAT128 type is not supported.");

                case PofConstants.T_DECIMAL32:
                    throw new NotSupportedException("T_DECIMAL32 type is not supported.");

                case PofConstants.T_DECIMAL64:
                    throw new NotSupportedException("T_DECIMAL64 type is not supported.");

                case PofConstants.T_DECIMAL128:
                    throw new NotSupportedException("T_DECIMAL128 type is not supported.");

                case PofConstants.T_OCTET:
                    return reader.ReadByte();

                case PofConstants.T_CHAR:
                    return ReadChar(reader);

                case PofConstants.V_REFERENCE_NULL:
                case PofConstants.V_BOOLEAN_FALSE:
                case PofConstants.V_INT_0:
                    return 0;

                case PofConstants.V_BOOLEAN_TRUE:
                case PofConstants.V_INT_1:
                    return 1;

                case PofConstants.V_INT_NEG_1:
                case PofConstants.V_INT_2:
                case PofConstants.V_INT_3:
                case PofConstants.V_INT_4:
                case PofConstants.V_INT_5:
                case PofConstants.V_INT_6:
                case PofConstants.V_INT_7:
                case PofConstants.V_INT_8:
                case PofConstants.V_INT_9:
                case PofConstants.V_INT_10:
                case PofConstants.V_INT_11:
                case PofConstants.V_INT_12:
                case PofConstants.V_INT_13:
                case PofConstants.V_INT_14:
                case PofConstants.V_INT_15:
                case PofConstants.V_INT_16:
                case PofConstants.V_INT_17:
                case PofConstants.V_INT_18:
                case PofConstants.V_INT_19:
                case PofConstants.V_INT_20:
                case PofConstants.V_INT_21:
                case PofConstants.V_INT_22:
                    return DecodeTinyInt(typeId);

                default:
                    throw new IOException("Unable to convert type " + typeId + " to a numeric type.");
            }
        }

        /// <summary>
        /// Read a value of the specified encoding from the POF stream and
        /// convert it to an <b>Int64</b>.
        /// </summary>
        /// <param name="reader">
        /// The POF stream containing the value.
        /// </param>
        /// <param name="typeId">
        /// The POF type of the value.
        /// </param>
        /// <returns>
        /// The POF value as an <b>Int64</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs reading the POF stream, or the POF value
        /// cannot be coerced to an <b>Int64</b> value.
        /// </exception>
        public static long ReadAsInt64(DataReader reader, int typeId)
        {
            switch (typeId)
            {
                case PofConstants.T_INT64:
                case PofConstants.T_INT128:
                    return reader.ReadPackedInt64();

                case PofConstants.T_FLOAT32:
                    return (long) reader.ReadSingle();

                case PofConstants.T_FLOAT64:
                    return (long) reader.ReadDouble();

                case PofConstants.T_FLOAT128:
                    throw new NotSupportedException("T_FLOAT128 type is not supported.");

                case PofConstants.T_DECIMAL32:
                    throw new NotSupportedException("T_DECIMAL32 type is not supported.");

                case PofConstants.T_DECIMAL64:
                    throw new NotSupportedException("T_DECIMAL64 type is not supported.");

                case PofConstants.T_DECIMAL128:
                    throw new NotSupportedException("T_DECIMAL128 type is not supported.");

                default:
                    return ReadAsInt32(reader, typeId);
            }
        }

        /// <summary>
        /// Read a value of the specified encoding from the POF stream and
        /// convert it to an <b>RawInt128</b>.
        /// </summary>
        /// <param name="reader">
        /// The POF stream containing the value.
        /// </param>
        /// <param name="typeId">
        /// The POF type of the value.
        /// </param>
        /// <returns>
        /// The POF value as an <b>RawInt128</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs reading the POF stream, or the POF value
        /// cannot be coerced to an <b>Int64</b> value.
        /// </exception>
        public static RawInt128 ReadAsRawInt128(DataReader reader, int typeId)
        {
            switch (typeId)
            {
                case PofConstants.T_INT128:
                    return reader.ReadPackedRawInt128(reader);

                case PofConstants.T_FLOAT32:
                case PofConstants.T_FLOAT64:
                case PofConstants.T_FLOAT128:
                case PofConstants.T_DECIMAL32:
                case PofConstants.T_DECIMAL64:
                case PofConstants.T_DECIMAL128:
                    return NumberUtils.DecimalToRawInt128(ReadDecimal(reader));

                default:
                    return new RawInt128(BitConverter.GetBytes(ReadAsInt64(reader, typeId)));
            }
        }        

        /// <summary>
        /// Read a value of the specified encoding from the POF stream and
        /// convert it to a <b>Decimal</b>.
        /// </summary>
        /// <param name="reader">
        /// The POF stream containing the value.
        /// </param>
        /// <param name="typeId">
        /// The POF type of the value.
        /// </param>
        /// <returns>
        /// The POF value as a Decimal.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs reading the POF stream, or the POF value
        /// cannot be coerced to a Decimal value.
        /// </exception>
        public static Decimal ReadAsDecimal(DataReader reader, int typeId)
        {
            switch (typeId)
            {
                case PofConstants.T_INT128:
                    return new Decimal(reader.ReadPackedInt64());

                case PofConstants.T_FLOAT32:
                    return new Decimal((double) reader.ReadSingle());

                case PofConstants.T_FLOAT64:
                    return new Decimal(reader.ReadDouble());

                case PofConstants.T_FLOAT128:
                    throw new NotSupportedException("T_FLOAT128 type is not supported.");

                case PofConstants.T_DECIMAL32:
                    return ReadDecimal(reader, 4);
 
                case PofConstants.T_DECIMAL64:
                    return ReadDecimal(reader, 8);

                case PofConstants.T_DECIMAL128:
                    return ReadDecimal(reader);

                default:
                    return new Decimal(ReadAsInt64(reader, typeId));
            }
        }

        /// <summary>
        /// Read a value of the specified encoding from the POF stream and
        /// convert it to a <b>Single</b>.
        /// </summary>
        /// <param name="reader">
        /// The POF stream containing the value.
        /// </param>
        /// <param name="typeId">
        /// The POF type of the value.
        /// </param>
        /// <returns>
        /// The POF value as a <b>Single</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs reading the POF stream, or the POF value
        /// cannot be coerced to a <b>Single</b> value.
        /// </exception>
        public static Single ReadAsSingle(DataReader reader, int typeId)
        {
            switch (typeId)
            {
                case PofConstants.T_INT64:
                    return reader.ReadPackedInt64();

                case PofConstants.T_FLOAT32:
                    return reader.ReadSingle();

                case PofConstants.V_FP_NEG_INFINITY:
                    return Single.NegativeInfinity;

                case PofConstants.V_FP_POS_INFINITY:
                    return Single.PositiveInfinity;

                case PofConstants.V_FP_NAN:
                    return Single.NaN;

                case PofConstants.T_FLOAT64:
                    return (float) reader.ReadDouble();

                case PofConstants.T_FLOAT128:
                    throw new NotSupportedException("T_FLOAT128 type is not supported.");

                case PofConstants.T_INT128:
                    throw new NotSupportedException("T_INT128 type is not supported.");

                case PofConstants.T_DECIMAL32:
                    throw new NotSupportedException("T_DECIMAL32 type is not supported.");

                case PofConstants.T_DECIMAL64:
                    throw new NotSupportedException("T_DECIMAL64 type is not supported.");

                case PofConstants.T_DECIMAL128:
                    throw new NotSupportedException("T_DECIMAL128 type is not supported.");

                default:
                    return ReadAsInt32(reader, typeId);
            }
        }

        /// <summary>
        /// Read a value of the specified encoding from the POF stream and
        /// convert it to a <b>Double</b>.
        /// </summary>
        /// <param name="reader">
        /// The POF stream containing the value.
        /// </param>
        /// <param name="typeId">
        /// The POF type of the value.
        /// </param>
        /// <returns>
        /// The POF value as a <b>Double</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs reading the POF stream, or the POF value
        /// cannot be coerced to a <b>Double</b> value.
        /// </exception>
        public static double ReadAsDouble(DataReader reader, int typeId)
        {
            switch (typeId)
            {
                case PofConstants.T_INT64:
                    return reader.ReadPackedInt64();

                case PofConstants.T_FLOAT32:
                    return reader.ReadSingle();

                case PofConstants.T_FLOAT64:
                    return reader.ReadDouble();

                case PofConstants.V_FP_NEG_INFINITY:
                    return Double.NegativeInfinity;

                case PofConstants.V_FP_POS_INFINITY:
                    return Double.PositiveInfinity;

                case PofConstants.V_FP_NAN:
                    return Double.NaN;

                case PofConstants.T_FLOAT128:
                    throw new NotSupportedException("T_FLOAT128 type is not supported.");

                case PofConstants.T_INT128:
                    throw new NotSupportedException("T_INT128 type is not supported.");

                case PofConstants.T_DECIMAL32:
                case PofConstants.T_DECIMAL64:
                case PofConstants.T_DECIMAL128:
                    throw new NotSupportedException("T_DECIMALxx type is not supported.");

                default:
                    return ReadAsInt32(reader, typeId);
            }
        }        

        /// <summary>
        /// Within the POF stream, skip the next POF value.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="DataReader"/> containing the POF stream.
        /// </param>
        public static void SkipValue(DataReader reader)
        {
            int typeId = reader.ReadPackedInt32();
            if (typeId == PofConstants.T_IDENTITY)
            {
                SkipPackedInts(reader, 1);
                typeId = reader.ReadPackedInt32();
            }

            SkipUniformValue(reader, typeId);
        }

        /// <summary>
        /// Within the POF stream, skip the next POF value of the specified
        /// type.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="DataReader"/> containing the POF stream.
        /// </param>
        /// <param name="typeId">
        /// The type of the value to skip.
        /// </param>
        public static void SkipUniformValue(DataReader reader, int typeId)
        {
            BinaryReader tempReader = null;
            int          length     = 0;

            switch (typeId)
            {
                case PofConstants.T_INT16:
                case PofConstants.T_INT32:
                case PofConstants.T_INT64:
                case PofConstants.T_INT128:
                case PofConstants.T_REFERENCE:
                case PofConstants.T_BOOLEAN:
                    SkipPackedInts(reader, 1);
                    break;

                case PofConstants.T_YEAR_MONTH_INTERVAL:
                    SkipPackedInts(reader, 2);
                    break;

                case PofConstants.T_DATE:
                    SkipPackedInts(reader, 3);
                    break;

                case PofConstants.T_TIME_INTERVAL:
                    SkipPackedInts(reader, 4);
                    break;

                case PofConstants.T_DAY_TIME_INTERVAL:
                    SkipPackedInts(reader, 5);
                    break;

                case PofConstants.T_FLOAT32:
                    tempReader = reader;
                    tempReader.BaseStream.Seek(4, SeekOrigin.Current);
                    break;

                case PofConstants.T_FLOAT64:
                    tempReader = reader;
                    tempReader.BaseStream.Seek(8, SeekOrigin.Current);
                    break;

                case PofConstants.T_FLOAT128:
                    tempReader = reader;
                    tempReader.BaseStream.Seek(16, SeekOrigin.Current);
                    break;

                case PofConstants.T_DECIMAL32:
                case PofConstants.T_DECIMAL64:
                case PofConstants.T_DECIMAL128:
                    SkipPackedInts(reader, 2);
                    break;

                case PofConstants.T_OCTET:
                    tempReader = reader;
                    tempReader.BaseStream.Seek(1, SeekOrigin.Current);
                    break;

                case PofConstants.T_CHAR:
                    switch (reader.ReadByte() & 0xF0)
                    {
                        case 0xC0:
                        case 0xD0:
                            tempReader = reader;
                            tempReader.BaseStream.Seek(1, SeekOrigin.Current);
                            break;

                        case 0xE0:
                            tempReader = reader;
                            tempReader.BaseStream.Seek(2, SeekOrigin.Current);
                            break;
                    }
                    break;

                case PofConstants.T_OCTET_STRING:  // octet-string
                case PofConstants.T_CHAR_STRING:   // char-string
                    length = reader.ReadPackedInt32();

                    if (length == PofConstants.V_REFERENCE_NULL)
                    {
                        break;
                    }

                    tempReader = reader;
                    tempReader.BaseStream.Seek(length, SeekOrigin.Current);
                    break;

                case PofConstants.T_DATETIME:
                    SkipPackedInts(reader, 3);
                    // fall through (datetime ends with a time)
                    goto case PofConstants.T_TIME;

                case PofConstants.T_TIME:
                    {
                        SkipPackedInts(reader, 4);
                        int zoneType = reader.ReadPackedInt32();
                        if (zoneType == 2)
                        {
                            SkipPackedInts(reader, 2);
                        }
                    }
                    break;

                case PofConstants.T_COLLECTION:
                case PofConstants.T_ARRAY:
                    for (int i = 0, c = reader.ReadPackedInt32(); i < c; ++i)
                    {
                        SkipValue(reader);
                    }
                    break;

                case PofConstants.T_UNIFORM_COLLECTION:
                case PofConstants.T_UNIFORM_ARRAY:
                    for (int i = 0, elementTypeId = reader.ReadPackedInt32(), c = reader.ReadPackedInt32(); i < c; ++i)
                    {
                        SkipUniformValue(reader, elementTypeId);
                    }
                    break;

                case PofConstants.T_SPARSE_ARRAY:
                    for (int i = 0, c = reader.ReadPackedInt32(); i < c; ++i)
                    {
                        int pos = reader.ReadPackedInt32();
                        if (pos < 0)
                        {
                            break;
                        }
                        SkipValue(reader);
                    }
                    break;

                case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                    for (int i = 0, elementTypeId = reader.ReadPackedInt32(), c = reader.ReadPackedInt32(); i < c; ++i)
                    {
                        int pos = reader.ReadPackedInt32();
                        if (pos < 0)
                        {
                            break;
                        }
                        SkipUniformValue(reader, elementTypeId);
                    }
                    break;

                case PofConstants.T_MAP:
                    for (int i = 0, c = reader.ReadPackedInt32(); i < c; ++i)
                    {
                        SkipValue(reader); // key
                        SkipValue(reader); // value
                    }
                    break;

                case PofConstants.T_UNIFORM_KEYS_MAP:
                    for (int i = 0, keyTypeId = reader.ReadPackedInt32(), c = reader.ReadPackedInt32(); i < c; ++i)
                    {
                        SkipUniformValue(reader, keyTypeId);
                        SkipValue(reader);
                    }
                    break;

                case PofConstants.T_UNIFORM_MAP:
                    for (int i = 0, keyTypeId = reader.ReadPackedInt32(), valueTypeId = reader.ReadPackedInt32(),
                        c = reader.ReadPackedInt32();
                        i < c;
                        ++i)
                    {
                        SkipUniformValue(reader, keyTypeId);
                        SkipUniformValue(reader, valueTypeId);
                    }
                    break;

                case PofConstants.V_BOOLEAN_FALSE:
                case PofConstants.V_BOOLEAN_TRUE:
                case PofConstants.V_STRING_ZERO_LENGTH:
                case PofConstants.V_COLLECTION_EMPTY:
                case PofConstants.V_REFERENCE_NULL:
                case PofConstants.V_FP_POS_INFINITY:
                case PofConstants.V_FP_NEG_INFINITY:
                case PofConstants.V_FP_NAN:
                case PofConstants.V_INT_NEG_1:
                case PofConstants.V_INT_0:
                case PofConstants.V_INT_1:
                case PofConstants.V_INT_2:
                case PofConstants.V_INT_3:
                case PofConstants.V_INT_4:
                case PofConstants.V_INT_5:
                case PofConstants.V_INT_6:
                case PofConstants.V_INT_7:
                case PofConstants.V_INT_8:
                case PofConstants.V_INT_9:
                case PofConstants.V_INT_10:
                case PofConstants.V_INT_11:
                case PofConstants.V_INT_12:
                case PofConstants.V_INT_13:
                case PofConstants.V_INT_14:
                case PofConstants.V_INT_15:
                case PofConstants.V_INT_16:
                case PofConstants.V_INT_17:
                case PofConstants.V_INT_18:
                case PofConstants.V_INT_19:
                case PofConstants.V_INT_20:
                case PofConstants.V_INT_21:
                case PofConstants.V_INT_22:
                    break;

                default:
                    if (typeId >= 0)
                    {
                        // user type
                        // version id, reference id, or T_IDENTITY
                        if (reader.ReadPackedInt32() == PofConstants.T_IDENTITY)
                        {
                            // TODO: see COH-11347
                            throw new NotSupportedException("Detected object identity/reference"
                                    + " in uniform collection, which is not currently supported");
                        }
                        int pos = reader.ReadPackedInt32();
                        while (pos >= 0)
                        {
                            SkipValue(reader);
                            pos = reader.ReadPackedInt32();
                        }
                    }
                    else
                    {
                        throw new IOException("type=" + typeId);
                    }
                    break;
            }
        }

        /// <summary>
        /// Skip the specified number of packed integers in the passed POF
        /// stream.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="DataReader"/> containing the POF stream.
        /// </param>
        /// <param name="count">
        /// The number of packed integers to skip over.
        /// </param>
        public static void SkipPackedInts(DataReader reader, int count)
        {
            while (count-- > 0)
            {
                reader.ReadPackedInt32();
            }
        }

        /// <summary>
        /// Determine if the specified byte array contains a packed
        /// 32-bit integer.
        /// </summary>
        /// <param name="buffer">
        /// The byte array that contains the packed 32-bit integer.
        /// </param>
        /// <param name="count">
        /// The total number of bytes that have been written into the
        /// byte array.
        /// </param>
        /// <returns>
        /// <b>true</b> if the specified byte array contains a packed
        /// 32-bit integer; <b>false</b>, otherwise.
        /// </returns>
        public static bool ContainsPackedInt32(byte[] buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if ((buffer[i] & 0x80) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determine the number of bytes required to encode the given integer
        /// using a packed 32-bit format.
        /// </summary>
        /// <param name="n">
        /// The integer to be encoded.
        /// </param>
        /// <returns>
        /// The total number of bytes required to encode the given integer
        /// (0 - 5).
        /// </returns>
        public static byte LengthPackedInt32(int n)
        {
            // NOTE: see DataWriter.WritePackedInt32()
            // these two algorithms must be kept in sync

            byte cb = 1;

            // first byte contains sign bit (bit 7 set if neg)
            if (n < 0)
            {
                n = ~n;
            }

            // first byte contains only 6 data bits
            n = NumberUtils.URShift(n, 6);

            // bytes 2-5 contain 7 data bits
            while (n != 0)
            {
                cb++;
                n = NumberUtils.URShift(n, 7);
            }

            return cb;
        }

        #endregion

        #region Encoding

        /// <summary>
        /// Write a <b>Decimal</b> to the passed <b>DataWriter</b> stream as
        /// a decimal value.
        /// </summary>
        /// <param name="writer">
        /// The DataWriter to write to.
        /// </param>
        /// <param name="value">
        /// The Decimal value.
        /// </param>
        public static void WriteDecimal(DataWriter writer, Decimal value)
        {
            RawInt128 int128 = NumberUtils.DecimalToRawInt128(value);
            int scale        = NumberUtils.GetScale(value);

            writer.WritePackedRawInt128(writer, int128);
            writer.WritePackedInt32(scale);
        }

        /// <summary>
        /// Write a <b>Decimal</b> to the passed <b>DataWriter</b> stream as
        /// a decimal value.
        /// </summary>
        /// <param name="writer">
        /// The DataWriter to write to.
        /// </param>
        /// <param name="value">
        /// The Decimal value.
        /// </param>
        /// <param name="cBytes">
        /// Number of bytes to write.
        /// </param>
        public static void WriteDecimal(DataWriter writer, Decimal value, int cBytes)
        {
            CheckDecimalRange(value, cBytes);

            int scale = NumberUtils.GetScale(value);

            switch (cBytes)
            {
                case 4:
                    Int32 i32 = Decimal.ToInt32(NumberUtils.GetUnscaledValue(value));
                    writer.WritePackedInt32(i32);
                    break;

                case 8:
                    Int64 i64 = Decimal.ToInt64(NumberUtils.GetUnscaledValue(value));
                    writer.WritePackedInt64(i64);
                    break;

                case 16:
                default:
                    RawInt128 nInt128 = NumberUtils.DecimalToRawInt128(value);
                    writer.WritePackedRawInt128(writer, nInt128);
                    break;
            }
            writer.WritePackedInt32(scale);
        }

        /// <summary>
        /// Encode an integer value into one of the reserved single-byte
        /// combined type and value indicators.
        /// </summary>
        /// <param name="n">
        /// An integer between -1 and 22, inclusive.
        /// </param>
        /// <returns>
        /// The integer value that the integer is encoded as.
        /// </returns>
        public static int EncodeTinyInt(int n)
        {
            Debug.Assert(n >= -1 && n <= 22);
            return PofConstants.V_INT_0 - n;
        }

        /// <summary>
        /// Write a date value to a <see cref="DataWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="DataWriter"/> to write to.
        /// </param>
        /// <param name="year">
        /// The year number as defined by ISO8601.
        /// </param>
        /// <param name="month">
        /// The month number between 1 and 12 inclusive as defined by
        /// ISO8601.
        /// </param>
        /// <param name="day">
        /// The day number between 1 and 31 inclusive as defined by ISO8601.
        /// </param>
        /// <exception cref="IOException">
        /// If the passed <b>DataWriter</b> object throws an exception while
        /// the value is being written to it.
        /// </exception>
        public static void WriteDate(DataWriter writer, int year, int month, int day)
        {
            writer.WritePackedInt32(year);
            writer.WritePackedInt32(month);
            writer.WritePackedInt32(day);
        }

        /// <summary>
        /// Write a time value to a <see cref="DataWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="DataWriter"/> to write to.
        /// </param>
        /// <param name="hour">
        /// The hour between 0 and 23, inclusive.
        /// </param>
        /// <param name="minute">
        /// The minute value between 0 and 59, inclusive.
        /// </param>
        /// <param name="second">
        /// The second value between 0 and 59, inclusive (and theoretically
        /// 60 for a leap-second).
        /// </param>
        /// <param name="nano">
        /// The nanosecond value between 0 and 999999999, inclusive.
        /// </param>
        /// <param name="timeZoneType">
        /// 0 if the time value does not have an explicit time zone, 1 if the
        /// time value is UTC and 2 if the time zone has an explicit hour and
        /// minute offset.
        /// </param>
        /// <param name="zoneOffset">
        /// The timezone offset from UTC, for example 0 for BST, -5 for EST
        /// and +1 for CET.
        /// </param>
        /// <exception cref="IOException">
        /// If the passed <b>DataWriter</b> object throws an exception while
        /// the value is being written to it.
        /// </exception>
        public static void WriteTime(DataWriter writer, int hour, int minute, int second,
                                     int nano, int timeZoneType, TimeSpan zoneOffset)
        {
            int fraction = 0;
            if (nano != 0)
            {
                fraction = nano % 1000000 == 0 ? nano/1000000 : - nano;
            }

            writer.WritePackedInt32(hour);
            writer.WritePackedInt32(minute);
            writer.WritePackedInt32(second);
            writer.WritePackedInt32(fraction);
            writer.WritePackedInt32(timeZoneType);

            if (timeZoneType == 2)
            {
                writer.WritePackedInt32(zoneOffset.Hours);
                writer.WritePackedInt32(zoneOffset.Minutes);
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validate a type identifier.
        /// </summary>
        /// <param name="typeId">
        /// The type identifier.
        /// </param>
        public static void CheckType(int typeId)
        {
            // user types are all values >= 0
            if (typeId < 0)
            {
                // all types < 0 are pre-defined
                switch (typeId)
                {
                    case PofConstants.T_INT16:
                    case PofConstants.T_INT32:
                    case PofConstants.T_INT64:
                    case PofConstants.T_INT128:
                    case PofConstants.T_FLOAT32:
                    case PofConstants.T_FLOAT64:
                    case PofConstants.T_FLOAT128:
                    case PofConstants.T_DECIMAL32:
                    case PofConstants.T_DECIMAL64:
                    case PofConstants.T_DECIMAL128:
                    case PofConstants.T_BOOLEAN:
                    case PofConstants.T_OCTET:
                    case PofConstants.T_OCTET_STRING:
                    case PofConstants.T_CHAR:
                    case PofConstants.T_CHAR_STRING:
                    case PofConstants.T_DATE:
                    case PofConstants.T_YEAR_MONTH_INTERVAL:
                    case PofConstants.T_TIME:
                    case PofConstants.T_TIME_INTERVAL:
                    case PofConstants.T_DATETIME:
                    case PofConstants.T_DAY_TIME_INTERVAL:
                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_ARRAY:
                    case PofConstants.T_UNIFORM_ARRAY:
                    case PofConstants.T_SPARSE_ARRAY:
                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                    case PofConstants.T_MAP:
                    case PofConstants.T_UNIFORM_KEYS_MAP:
                    case PofConstants.T_UNIFORM_MAP:
                    case PofConstants.T_IDENTITY:
                    case PofConstants.T_REFERENCE:
                        break;

                    default:
                        throw new ArgumentException("unknown type: " + typeId);
                }
            }
        }

        /// <summary>
        /// Verify that the number of elements is valid.
        /// </summary>
        /// <param name="elementCount">
        /// The number of elements in a complex data structure.
        /// </param>
        public static void CheckElementCount(int elementCount)
        {
            if (elementCount < 0)
            {
                throw new ArgumentException("illegal element count: " + elementCount);
            }
        }

        /// <summary>
        /// Validate a reference identifier to make sure it is in a valid
        /// range.
        /// </summary>
        /// <param name="id">
        /// The reference identity.
        /// </param>
        public static void CheckReferenceRange(int id)
        {
            if (id < 0)
            {
                throw new ArgumentException("illegal reference identity: " + id);
            }
        }

        /// <summary>
        /// Verify that the specified Decimal value will fit in the specified
        /// number of bytes.
        /// </summary>
        /// <param name="dec">
        /// The Decimal value.
        /// </param>
        /// <param name="cBytes">
        /// The number of bytes (4, 8 or 16).
        /// </param>
        ///
        public static void CheckDecimalRange(Decimal dec, int cBytes)
        {
            Decimal absValue = Math.Abs(NumberUtils.GetUnscaledValue(dec));
            int     nScale   = NumberUtils.GetScale(dec);

            switch (cBytes)
            {
                case 4:
                if (absValue.CompareTo(PofConstants.MAX_DECIMAL32_UNSCALED) > 0
                        || nScale < PofConstants.MIN_DECIMAL32_SCALE
                        || nScale > PofConstants.MAX_DECIMAL32_SCALE)
                    {
                    throw new InvalidOperationException(
                        "decimal value exceeds IEEE754r 32-bit range: " + dec);
                    }
                break;

            case 8:
                if (absValue.CompareTo(PofConstants.MAX_DECIMAL64_UNSCALED) > 0
                        || nScale < PofConstants.MIN_DECIMAL64_SCALE
                        || nScale > PofConstants.MAX_DECIMAL64_SCALE)
                    {
                    throw new InvalidOperationException(
                        "decimal value exceeds IEEE754r 64-bit range: " + dec);
                    }
                break;

            case 16:
                if (absValue.CompareTo(PofConstants.MAX_DECIMAL_UNSCALED) > 0
                        || nScale < PofConstants.MIN_DECIMAL_SCALE
                        || nScale > PofConstants.MAX_DECIMAL_SCALE)
                    {
                    throw new InvalidOperationException(
                        "decimal value exceeds .NET 96-bit range: " + dec);
                    }
                break;

            default:
                throw new InvalidOperationException("byte count (" + cBytes
                        + ") must be 4, 8 or 16");
            }
        }

        /// <summary>
        /// Determine the minimum size (in bytes) of the IEEE754 decimal type
        /// that would be capable of holding the passed value.
        /// </summary>
        /// <param name="value">
        /// The decimal value.
        /// </param>
        /// <returns>
        /// The number of bytes (4, 8 or 16).
        /// </returns>
        public static int CalcDecimalSize(Decimal value)
        {

            int nScale       = NumberUtils.GetScale(value);
            Decimal absValue = Math.Abs(NumberUtils.GetUnscaledValue(value));
            if (absValue.CompareTo(PofConstants.MAX_DECIMAL32_UNSCALED) <= 0
                    && nScale >= PofConstants.MIN_DECIMAL32_SCALE
                    && nScale <= PofConstants.MAX_DECIMAL32_SCALE)
            {
                return 4;
            }
            else if (absValue.CompareTo(PofConstants.MAX_DECIMAL64_UNSCALED) <= 0
                    && nScale >= PofConstants.MIN_DECIMAL64_SCALE
                    && nScale <= PofConstants.MAX_DECIMAL64_SCALE)
            {
                return 8;
            }
            else
            {
                // we should compare against the MAX_DECIMAL128_UNSCALED value,
                // but there is no datatype on NET that can hold that value.
                return 16;
            }

            throw new InvalidOperationException(
                    "decimal value exceeds IEEE754r 128-bit range: " + value);
        }


        /// <summary>
        /// Validate date information.
        /// </summary>
        /// <param name="year">
        /// The year number.
        /// </param>
        /// <param name="month">
        /// The month number.
        /// </param>
        /// <param name="day">
        /// The day number.
        /// </param>
        public static void CheckDate(int year, int month, int day)
        {
            if (month < 1 || month > 12)
            {
                throw new ArgumentException("month is out of range: " + month);
            }

            if (day < 1 || day > MAX_DAYS_PER_MONTH[month - 1])
            {
                throw new ArgumentException("day is out of range: " + day);
            }

            if (month == 2 && day == 29 && (year%4 != 0 || (year%100 == 0 && year%400 != 0)))
            {
                throw new ArgumentException("not a leap year: " + year);
            }
        }

        /// <summary>
        /// Validate time information.
        /// </summary>
        /// <param name="hour">
        /// The hour number.
        /// </param>
        /// <param name="minute">
        /// The minute number.
        /// </param>
        /// <param name="second">
        /// The second number.
        /// </param>
        /// <param name="nano">
        /// The nanosecond number.
        /// </param>
        public static void CheckTime(int hour, int minute, int second, int nano)
        {
            if (hour < 0 || hour > 23)
            {
                if (hour == 24 && minute == 0 && second == 0 && nano == 0)
                {
                    throw new ArgumentException("end-of-day midnight (24:00:00.0) is supported by ISO8601" +
                                              ", but use 00:00:00.0 instead");
                }
                else
                {
                    throw new ArgumentException("hour is out of range: " + hour);
                }
            }

            if (minute < 0 || minute > 59)
            {
                throw new ArgumentException("minute is out of range: " + minute);
            }

            // 60 is allowed for a leap-second
            if (second < 0 || (second == 60 && nano > 0) || second > 60)
            {
                throw new ArgumentException("second is out of range: " + second);
            }

            if (nano < 0 || nano > 999999999)
            {
                throw new ArgumentException("nanosecond is out of range: " + nano);
            }
        }

        /// <summary>
        /// Check the specified timezone offset.
        /// </summary>
        /// <param name="hourOffset">
        /// The hour offset.
        /// </param>
        /// <param name="minuteOffset">
        /// The minute offset.
        /// </param>
        public static void CheckTimeZone(int hourOffset, int minuteOffset)
        {
            // technically this is reasonable, but in reality it should never be
            // over 14; unfortunately, countries keep changing theirs for silly
            // reasons; see http://www.worldtimezone.com/faq.html
            if (hourOffset < -23 || hourOffset > 23)
            {
                throw new ArgumentException("invalid hour offset: " + hourOffset);
            }

            // The minute offset should be 0, 15, 30 or 45 for standard timezones, but for
            // non-standard timezones, the minute offset could be any number between 0 and 59.
            // For example, Hong Kong switched from local mean time to standard time in 1904,
            // so prior to 1904, the minute offset was 36.See http://en.wikipedia.org/wiki/Standard_time
            if (minuteOffset < 0 || minuteOffset > 59)
            {
                throw new ArgumentException("invalid minute offset: " + minuteOffset);
            }
        }

        /// <summary>
        /// Validate a year-month interval.
        ///  </summary>
        /// <param name="years">
        /// The number of years.
        /// </param>
        /// <param name="months">
        /// The number of months.
        /// </param>
        public static void CheckYearMonthInterval(int years, int months)
        {
            if (years == 0)
            {
                if (months < -11 || months > 11)
                {
                    throw new ArgumentException("month interval is out of range: " + months);
                }
            }
        }

        /// <summary>
        /// Validate a time interval.
        /// </summary>
        /// <param name="hours">
        /// The number of hours.
        /// </param>
        /// <param name="minutes">
        /// The number of minutes.
        /// </param>
        /// <param name="seconds">
        /// The number of seconds.
        /// </param>
        /// <param name="nanos">
        /// The number of nanoseconds.
        /// </param>
        public static void CheckTimeInterval(int hours, int minutes, int seconds, int nanos)
        {
            // duration is allowed to be negative
            if (hours == 0)
            {
                if (minutes == 0)
                {
                    if (seconds == 0)
                    {
                        nanos = Math.Abs(nanos);
                    }
                    else
                    {
                        seconds = Math.Abs(seconds);
                    }
                }
                else
                {
                    minutes = Math.Abs(minutes);
                }
            }
            else
            {
                hours = Math.Abs(hours);
            }

            // apply the same rules as limit the time values themselves
            CheckTime(hours, minutes, seconds, nanos);
        }

        /// <summary>
        /// Validate a day-time interval.
        /// </summary>
        /// <param name="days">
        /// The number of days.
        /// </param>
        /// <param name="hours">
        /// The number of hours.
        /// </param>
        /// <param name="minutes">
        /// The number of minutes.
        /// </param>
        /// <param name="seconds">
        /// The number of seconds.
        /// </param>
        /// <param name="nanos">
        /// The number of nanoseconds.
        /// </param>
        public static void CheckDayTimeInterval(int days, int hours, int minutes, int seconds, int nanos)
        {
            if (days == 0)
            {
                CheckTimeInterval(hours, minutes, seconds, nanos);
            }
            else
            {
                // number of days is permitted to be any value

                // apply the same rules as limit the time values themselves
                CheckTime(hours, minutes, seconds, nanos);
            }
        }

        #endregion

        #region String formatting

        /// <summary>
        /// Format a date in the form YYYY-MM-DD.
        /// </summary>
        /// <param name="year">
        /// The year number.
        /// </param>
        /// <param name="month">
        /// The month number.
        /// </param>
        /// <param name="day">
        /// The day number.
        /// </param>
        /// <returns>
        /// Return the date in the form YYYY-MM-DD.
        /// </returns>
        public static string FormatDate(int year, int month, int day)
        {
            return new DateTime(year, month, day).ToString("yyyy-MM-dd");
        }



        /// <summary>
        /// Format a time.
        /// </summary>
        /// <remarks>
        /// Time format is the simplest applicable of the following formats:
        /// <list type="bullet">
        /// <item><tt>HH:MM</tt></item>
        /// <item><tt>HH:MM:SS</tt></item>
        /// <item><tt>HH:MM:SS.MMM</tt></item>
        /// <item><tt>HH:MM:SS.NNNNNNNNN</tt></item>
        /// </list>
        /// </remarks>
        /// <param name="hour">
        /// The hour number.
        /// </param>
        /// <param name="minute">
        /// The minute number.
        /// </param>
        /// <param name="second">
        /// The second number.
        /// </param>
        /// <param name="nano">
        /// The nanosecond number.
        /// </param>
        /// <param name="isUTC">
        /// <b>true</b> for UTC, <b>false</b> for no time zone.
        /// </param>
        /// <returns>
        /// A time string.
        /// </returns>
        public static string FormatTime(int hour, int minute, int second, int nano, bool isUTC)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(hour.ToString("D2")).Append(":").Append(minute.ToString("D2"));
            if (second != 0 || nano != 0)
            {
                sb.Append(":").Append(second.ToString("D2"));

                if (nano != 0)
                {
                    sb.Append('.');
                    if (nano % 1000000 == 0)
                    {
                        sb.Append((nano/1000000).ToString("D3"));
                    }
                    else
                    {
                        sb.Append(nano.ToString("D9"));
                    }
                }
            }

            if (isUTC)
            {
                sb.Append('Z');
            }

            return sb.ToString();
        }

        /// <summary>
        /// Format a time.
        /// </summary>
        /// <remarks>
        /// Time format is the simplest applicable of the following formats:
        /// <list type="bullet">
        /// <item><tt>HH:MM(+|-)HH:MM</tt></item>
        /// <item><tt>HH:MM:SS(+|-)HH:MM</tt></item>
        /// <item><tt>HH:MM:SS.MMM(+|-)HH:MM</tt></item>
        /// <item><tt>HH:MM:SS.NNNNNNNNN(+|-)HH:MM</tt></item>
        /// </list>
        /// </remarks>
        /// <param name="hour">
        /// The hour number.
        /// </param>
        /// <param name="minute">
        /// The minute number.
        /// </param>
        /// <param name="second">
        /// The second number.
        /// </param>
        /// <param name="nano">
        /// The nanosecond number.
        /// </param>
        /// <param name="hourOffset">
        /// The timezone offset in hours.
        /// </param>
        /// <param name="minuteOffset">
        /// The timezone offset in minutes.
        /// </param>
        /// <returns>
        /// A time string.
        /// </returns>
        public static string FormatTime(int hour, int minute, int second, int nano, int hourOffset, int minuteOffset)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(FormatTime(hour, minute, second, nano, false));
            if (hourOffset < 0)
            {
                sb.Append('-');
                hourOffset = -hourOffset;
            }
            else
            {
                sb.Append('+');
            }

            sb.Append(hourOffset.ToString("D2")).Append(":").Append(minuteOffset.ToString("D2"));
            return sb.ToString();
        }


        #endregion

        #region Constants

        private static IDictionary DOTNET_TO_POF_TYPE;
        private static IDictionary POF_TO_DOTNET_TYPE;

        /// <summary>
        /// The maximum number of days in each month.
        /// </summary>
        /// <remarks>
        /// Note February.
        /// </remarks>
        private static readonly int[] MAX_DAYS_PER_MONTH = new int[] {31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};

        /// <summary>
        /// The default BigDecimal value.
        /// </summary>
        public static readonly Decimal BIGDECIMAL_ZERO = new Decimal(0L);

        /// <summary>
        /// An empty array of bytes.
        /// </summary>
        public static readonly bool[] BOOLEAN_ARRAY_EMPTY = new bool[0];

        /// <summary>
        /// An empty array of bytes.
        /// </summary>
        public static readonly byte[] BYTE_ARRAY_EMPTY = new byte[0];

        /// <summary>
        /// An empty array of chars.
        /// </summary>
        public static readonly char[] CHAR_ARRAY_EMPTY = new char[0];

        /// <summary>
        /// An empty array of shorts.
        /// </summary>
        public static readonly short[] INT16_ARRAY_EMPTY = new short[0];

        /// <summary>
        /// An empty array of ints.
        /// </summary>
        public static readonly int[] INT32_ARRAY_EMPTY = new int[0];

        /// <summary>
        /// An empty array of longs.
        /// </summary>
        public static readonly long[] INT64_ARRAY_EMPTY = new long[0];

        /// <summary>
        /// An empty array of floats.
        /// </summary>
        public static readonly float[] SINGLE_ARRAY_EMPTY = new float[0];

        /// <summary>
        /// An empty array of doubles.
        /// </summary>
        public static readonly double[] DOUBLE_ARRAY_EMPTY = new double[0];

        /// <summary>
        /// An empty array of objects.
        /// </summary>
        public static readonly object[] OBJECT_ARRAY_EMPTY = new object[0];

        /// <summary>
        /// An empty (and immutable) collection.
        /// </summary>
        public static readonly ICollection COLLECTION_EMPTY = new ArrayList();

        /// <summary>
        /// An empty Binary value.
        /// </summary>
        public static readonly Binary BINARY_EMPTY = new Binary(BYTE_ARRAY_EMPTY);

        static PofHelper()
        {
            {
                IDictionary map                         = new HashDictionary();
                map[typeof (Int16)]                     = PofConstants.T_INT16;
                map[typeof (Int32)]                     = PofConstants.T_INT32;
                map[typeof (Int64)]                     = PofConstants.T_INT64;
                map[typeof (Single)]                    = PofConstants.T_FLOAT32;
                map[typeof (Double)]                    = PofConstants.T_FLOAT64;
                map[typeof (Decimal)]                   = PofConstants.T_DECIMAL64;
                map[typeof (Boolean)]                   = PofConstants.T_BOOLEAN;
                map[typeof (Byte)]                      = PofConstants.T_OCTET;
                map[typeof (Binary)]                    = PofConstants.T_OCTET_STRING;
                map[typeof (Char)]                      = PofConstants.T_CHAR;
                map[typeof (string)]                    = PofConstants.T_CHAR_STRING;
                map[typeof (DateTime)]                  = PofConstants.T_DATETIME;
                map[typeof (RawTime)]                   = PofConstants.T_TIME;
                map[typeof (RawDateTime)]               = PofConstants.T_DATETIME;
                map[typeof (RawYearMonthInterval)]      = PofConstants.T_YEAR_MONTH_INTERVAL;
                map[typeof (TimeSpan)]                  = PofConstants.T_DAY_TIME_INTERVAL;
                map[typeof (bool[])]                    = PofConstants.T_UNIFORM_ARRAY;
                map[typeof (byte[])]                    = PofConstants.T_OCTET_STRING;
                map[typeof (char[])]                    = PofConstants.T_CHAR_STRING;
                map[typeof (short[])]                   = PofConstants.T_UNIFORM_ARRAY;
                map[typeof (int[])]                     = PofConstants.T_UNIFORM_ARRAY;
                map[typeof (long[])]                    = PofConstants.T_UNIFORM_ARRAY;
                map[typeof (float[])]                   = PofConstants.T_UNIFORM_ARRAY;
                map[typeof (double[])]                  = PofConstants.T_UNIFORM_ARRAY;
                map[typeof (ArrayList)]                 = PofConstants.T_COLLECTION;
                map[typeof (Queue)]                     = PofConstants.T_COLLECTION;
                map[typeof (Stack)]                     = PofConstants.T_COLLECTION;
                map[typeof (Hashtable)]                 = PofConstants.T_MAP;
                DOTNET_TO_POF_TYPE                      = map;

                map                                     = new HashDictionary();
                map[PofConstants.T_INT16]               = typeof(Int16);
                map[PofConstants.T_INT32]               = typeof(Int32);
                map[PofConstants.T_INT64]               = typeof(Int64);
                map[PofConstants.T_FLOAT32]             = typeof(Single);
                map[PofConstants.T_FLOAT64]             = typeof(Double);
                map[PofConstants.T_DECIMAL32]           = typeof(Decimal);
                map[PofConstants.T_DECIMAL64]           = typeof(Decimal);
                map[PofConstants.T_BOOLEAN]             = typeof(Boolean);
                map[PofConstants.T_OCTET]               = typeof(Byte);
                map[PofConstants.T_OCTET_STRING]        = typeof(Byte[]);
                map[PofConstants.T_CHAR]                = typeof(Char);
                map[PofConstants.T_CHAR_STRING]         = typeof(string);
                map[PofConstants.T_DATE]                = typeof(DateTime);
                map[PofConstants.T_TIME]                = typeof(RawTime);
                map[PofConstants.T_DATETIME]            = typeof(RawDateTime);
                map[PofConstants.T_YEAR_MONTH_INTERVAL] = typeof(RawYearMonthInterval);
                map[PofConstants.T_DAY_TIME_INTERVAL]   = typeof(TimeSpan);
                map[PofConstants.T_TIME_INTERVAL]       = typeof(TimeSpan);
                map[PofConstants.T_ARRAY]               = typeof(object[]);
                map[PofConstants.T_COLLECTION]          = typeof(ArrayList);
                map[PofConstants.T_MAP]                 = typeof(Hashtable);
                POF_TO_DOTNET_TYPE                      = map;
            }
        }

        #endregion
    }
}
