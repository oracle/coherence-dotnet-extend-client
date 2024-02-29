/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;

using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// An implementation of <see cref="IPofHandler"/> that writes a POF
    /// stream to a <b>Stream</b> using a <see cref="DataWriter"/> object.
    /// </summary>
    /// <author>Cameron Purdy  2006.07.11</author>
    /// <author>Aleksandar Seovic  2006.08.08</author>
    /// <author>Ivan Cikic  2006.08.09</author>
    /// <since>Coherence 3.2</since>
    public class WritingPofHandler : IPofHandler
    {
        #region Constructors

        /// <summary>
        /// Construct a WritingPofHandler that will write a POF stream to
        /// the passed <see cref="DataWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="DataWriter"/> to write to.
        /// </param>
        public WritingPofHandler(DataWriter writer)
        {
            Debug.Assert(writer != null);
            m_writer = writer;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="DataWriter"/> object that this
        /// WritingPofHandler is writing to.
        /// </summary>
        /// <value>
        /// The <see cref="DataWriter"/> object that this POF handler is
        /// writing to.
        /// </value>
        public virtual DataWriter Writer
        {
            get { return m_writer; }
        }

        /// <summary>
        /// Determine if the value encoding can be skipped.
        /// </summary>
        /// <remarks>
        /// A value can be skipped if it is a default value and if it does
        /// not have an identity and if it is in a sparse data structure.
        /// </remarks>
        /// <returns>
        /// <b>true</b> if value encoding of default values can be skipped
        /// altogether.
        /// </returns>
        protected internal virtual bool IsSkippable
        {
            get
            {
                if (m_hasIdentity)
                {
                    return false;
                }

                Complex complex = m_complex;
                return complex != null && complex.IsSparse;
            }
        }

        /// <summary>
        /// Determine if the value encoding can be compressed by combining
        /// type and value information in such a way that type information
        /// could be lost.
        /// </summary>
        /// <returns>
        /// <b>true</b> if values can be encoded without type information.
        /// </returns>
        protected internal virtual bool IsCompressable
        {
            get { return !m_hasIdentity; }
        }

        #endregion

        #region IPofHandler implementation

        /// <summary>
        /// This method is invoked when an identity is encountered in the POF
        /// stream.
        /// </summary>
        /// <remarks>
        /// The identity is used to uniquely identify the next value in the
        /// POF stream, and can be later referenced by the
        /// <see cref="OnIdentityReference"/> method.
        /// </remarks>
        /// <param name="id">
        /// If <tt>(nId >= 0)</tt>, then this is the identity encountered in
        /// the POF stream, otherwise it is an indicator that the following
        /// value <i>could</i> have been assigned an identifier but was not
        /// (i.e. that the subsequent value is of a referenceable data type).
        /// </param>
        public virtual void RegisterIdentity(int id)
        {
            Debug.Assert(!m_hasIdentity || id < 0);

            if (id >= 0)
            {
                DataWriter writer = m_writer;
                writer.WritePackedInt32(PofConstants.T_IDENTITY);
                writer.WritePackedInt32(id);
            }

            m_hasIdentity = true;
        }

        /// <summary>
        /// Specifies that a <b>null</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        public virtual void OnNullReference(int position)
        {
            if (!IsSkippable)
            {
                EncodePosition(position);
                m_writer.WritePackedInt32(PofConstants.V_REFERENCE_NULL);
            }
        }

        /// <summary>
        /// Specifies that a reference to a previously-identified value has
        /// been encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a dictionary, -1
        /// otherwise.
        /// </param>
        /// <param name="id">
        /// The identity of the previously encountered value, as was
        /// specified in a previous call to <see cref="RegisterIdentity"/>.
        /// </param>
        public virtual void OnIdentityReference(int position, int id)
        {
            EncodePosition(position);

            DataWriter writer = m_writer;
            if (IsTypeIdEncoded(PofConstants.T_REFERENCE))
            {
                writer.WritePackedInt32(PofConstants.T_REFERENCE);
            }
            writer.WritePackedInt32(id);
        }

        /// <summary>
        /// Report that a short integer value has been encountered in the POF
        /// stream.
        /// </summary>
        ///<param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a dictionary, -1
        /// otherwise.
        /// </param>
        /// <param name="n">
        /// The integer value as a short.
        /// </param>
        public virtual void OnInt16(int position, short n)
        {
            if (n != 0 || !IsSkippable)
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_INT16))
                {
                    if (n >= -1 && n <= 22 && isCompressable)
                    {
                        writer.WritePackedInt32(PofHelper.EncodeTinyInt(n));
                        return;
                    }
                    writer.WritePackedInt32(PofConstants.T_INT16);
                }
                writer.WritePackedInt32(n);
            }
        }

        /// <summary>
        /// Report that an integer value has been encountered in the POF
        /// stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="n">
        /// The integer value as an int.
        /// </param>
        public virtual void OnInt32(int position, int n)
        {
            if (n != 0 || !IsSkippable)
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_INT32))
                {
                    if (n >= - 1 && n <= 22 && isCompressable)
                    {
                        writer.WritePackedInt32(PofHelper.EncodeTinyInt(n));
                        return;
                    }
                    writer.WritePackedInt32(PofConstants.T_INT32);
                }
                writer.WritePackedInt32(n);
            }
        }

        /// <summary>
        /// Report that a long integer value has been encountered in the POF
        /// stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="n">
        /// The integer value as a long.
        /// </param>
        public virtual void OnInt64(int position, long n)
        {
            if (n != 0L || !IsSkippable)
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_INT64))
                {
                    if (n >= - 1L && n <= 22L && isCompressable)
                    {
                        writer.WritePackedInt32(PofHelper.EncodeTinyInt((int) n));
                        return;
                    }
                    writer.WritePackedInt32(PofConstants.T_INT64);
                }
                writer.WritePackedInt64(n);
            }
        }

        /// <summary>
        /// Report that an <b>Int128</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="n">
        /// The integer value as an <b>Int128</b>.
        /// </param>
        public virtual void OnInt128(int position, RawInt128 n)
        {
            if (!n.IsZero || !IsSkippable)
            {
                EncodePosition(position);

                DataWriter writer = m_writer;
                writer.WritePackedInt32(PofConstants.T_INT128);
                writer.WritePackedRawInt128(writer, n);
            }
        }

        /// <summary>
        /// Report that a base-2 single-precision floating point value has
        /// been encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="fl">
        /// The floating point value as a float.
        /// </param>
        public virtual void OnFloat32(int position, float fl)
        {
            if (fl != 0.0F || !IsSkippable)
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_FLOAT32))
                {
                    // encode special values
                    int bits = NumberUtils.SingleToInt32Bits(fl);
                    if ((bits & 0x0000FFFF) == 0 && isCompressable)
                    {
                        switch (NumberUtils.URShift(bits, 16))
                        {
                            case 0xFF80:
                                writer.WritePackedInt32(PofConstants.V_FP_NEG_INFINITY);
                                break;
                            case 0x7F80:
                                writer.WritePackedInt32(PofConstants.V_FP_POS_INFINITY);
                                break;
                            case 0xFFC0:
                                writer.WritePackedInt32(PofConstants.V_FP_NAN);
                                break;
                            case 0xBF80:
                            // -1
                            case 0x0000:
                            // 0
                            case 0x3F80:
                            // 1
                            case 0x4000:
                            case 0x4040:
                            case 0x4080:
                            case 0x40A0:
                            case 0x40C0:
                            case 0x40E0:
                            case 0x4100:
                            case 0x4110:
                            case 0x4120:
                            case 0x4130:
                            case 0x4140:
                            case 0x4150:
                            case 0x4160:
                            case 0x4170:
                            case 0x4180:
                            case 0x4188:
                            case 0x4190:
                            case 0x4198:
                            case 0x41A0:
                            case 0x41A8:
                            case 0x41B0: // 22
                                writer.WritePackedInt32(PofHelper.EncodeTinyInt((int)fl));
                                break;
                            default:
                                writer.WritePackedInt32(PofConstants.T_FLOAT32);
                                writer.Write(bits);
                                break;
                        }
                    }
                    else
                    {
                        writer.WritePackedInt32(PofConstants.T_FLOAT32);
                        writer.Write(bits);
                    }
                }
                else
                {
                    writer.Write(fl);
                }
            }
        }

        /// <summary>
        /// Report that a base-2 double-precision floating point value has
        /// been encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="dfl">
        /// The floating point value as a double.
        /// </param>
        public virtual void OnFloat64(int position, double dfl)
        {
            if (dfl != 0.0 || !IsSkippable)
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_FLOAT64))
                {
                    // encode special values
                    long bits = NumberUtils.DoubleToInt64Bits(dfl);
                    if ((bits & 0x0000FFFFFFFFFFFFL) == 0L && isCompressable)
                    {
                        switch ((int)(NumberUtils.URShift(bits, 48)))
                        {
                            case 0xFFF0:
                                writer.WritePackedInt32(PofConstants.V_FP_NEG_INFINITY);
                                break;
                            case 0x7FF0:
                                writer.WritePackedInt32(PofConstants.V_FP_POS_INFINITY);
                                break;
                            case 0xFFF8:
                                writer.WritePackedInt32(PofConstants.V_FP_NAN);
                                break;
                            case 0xBFF0:
                            // -1
                            case 0x0000:
                            // 0
                            case 0x3FF0:
                            // 1
                            case 0x4000:
                            case 0x4008:
                            case 0x4010:
                            case 0x4014:
                            case 0x4018:
                            case 0x401C:
                            case 0x4020:
                            case 0x4022:
                            case 0x4024:
                            case 0x4026:
                            case 0x4028:
                            case 0x402A:
                            case 0x402C:
                            case 0x402E:
                            case 0x4030:
                            case 0x4031:
                            case 0x4032:
                            case 0x4033:
                            case 0x4034:
                            case 0x4035:
                            case 0x4036: // 22
                                writer.WritePackedInt32(PofConstants.V_INT_0 - (int)dfl);
                                break;

                            default:
                                writer.WritePackedInt32(PofConstants.T_FLOAT64);
                                writer.Write(bits);
                                break;
                        }
                    }
                    else
                    {
                        writer.WritePackedInt32(PofConstants.T_FLOAT64);
                        writer.Write(bits);
                    }
                }
                else
                {
                    writer.Write(dfl);
                }
            }
        }

        // CLOVER:OFF

        // TODO: add support for RawQuad
        // TODO: add support for Binary

        // CLOVER:ON

        /// <summary>
        /// Report that a <b>Decimal32</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="dec">
        /// The decimal value as a <b>Decimal</b>.
        /// </param>
        public virtual void OnDecimal32(int position, Decimal dec)
        {
            if (NumberUtils.GetScale(dec) != 0 || !dec.Equals(Decimal.Zero) || !IsSkippable)
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_DECIMAL32))
                {
                    /**************************
                    * The following block of code is not yet implemented due to the
                    * the lack of library methods to use.  Worth implementing this
                    * compression if a use case warrants it. (and this is in Java, not .NET)
                    
                    if (dec.scale() == 0 && fCompressable)
                        {
                        BigInteger n = dec.unscaledValue();
                        if (n.bitLength() <= 7)
                            {
                            int nTiny = n.intValue();
                            if (nTiny >= -1 && nTiny <= 22)
                                {
                                out.writePackedInt(encodeTinyInt(nTiny));
                                break output;
                                }
                            }
                        }
                    ************************/
                    writer.WritePackedInt32(PofConstants.T_DECIMAL32);
                }

                PofHelper.WriteDecimal(writer, dec, 4);
            }
        }

        /// <summary>
        /// Report that a <b>Decimal64</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="dec">
        /// The decimal value as a <b>Decimal</b>.
        /// </param>
        public virtual void OnDecimal64(int position, Decimal dec)
        {
            if (NumberUtils.GetScale(dec) != 0 || !dec.Equals(Decimal.Zero) || !IsSkippable)
            {
                bool       isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer         = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_DECIMAL64))
                {
                    /**************************
                    * The following block of code is not yet implemented due to the
                    * the lack of library methods to use.  Worth implementing this
                    * compression if a use case warrants it. (and this is in Java, not .NET)
                    
                    if (dec.scale() == 0 && fCompressable)
                        {
                        BigInteger n = dec.unscaledValue();
                        if (n.bitLength() <= 7)
                            {
                            int nTiny = n.intValue();
                            if (nTiny >= -1 && nTiny <= 22)
                                {
                                out.writePackedInt(encodeTinyInt(nTiny));
                                break output;
                                }
                            }
                        }
                    ************************/
                    writer.WritePackedInt64(PofConstants.T_DECIMAL64);
                }

                PofHelper.WriteDecimal(writer, dec, 8);
            }
        }

        /// <summary>
        /// Report that a <b>Decimal128</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="dec">
        /// The decimal value as a <b>Decimal</b>.
        /// </param>
        public virtual void OnDecimal128(int position, Decimal dec)
        {
            OnDecimal(position, dec);
        }

        /// <summary>
        /// Report that a <b>Decimal</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="dec">
        /// The decimal value as a <b>Decimal</b>.
        /// </param>
        protected virtual void OnDecimal(int position, Decimal dec)
        {
            int scale = NumberUtils.GetScale(dec);

            if (scale != 0 || !IsSkippable)
            {
                bool       isCompressable = IsCompressable;
                DataWriter writer         = m_writer;

                EncodePosition(position);

                if (IsTypeIdEncoded(PofConstants.T_DECIMAL128))
                {
                    writer.WritePackedInt32(PofConstants.T_DECIMAL128);
                }

                PofHelper.WriteDecimal(writer, dec);
            }
        }

        /// <summary>
        /// Report that a boolean value has been encountered in the POF
        /// stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="f">
        /// The boolean value.
        /// </param>
        public virtual void OnBoolean(int position, bool f)
        {
            if (f || !IsSkippable)
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_BOOLEAN))
                {
                    if (isCompressable)
                    {
                        writer.WritePackedInt32(f ? PofConstants.V_BOOLEAN_TRUE : PofConstants.V_BOOLEAN_FALSE);
                        return;
                    }
                    writer.WritePackedInt32(PofConstants.T_BOOLEAN);
                }

                writer.WritePackedInt32(f ? 1 : 0);
            }
        }

        /// <summary>
        /// Report that an octet value (a byte) has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="b">
        /// The octet value as an int whose value is in the range 0 to
        /// 255 (0x00-0xFF) inclusive.
        /// </param>
        public virtual void OnOctet(int position, int b)
        {
            if (b != 0x00 || !IsSkippable)
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_OCTET))
                {
                    if (isCompressable)
                    {
                        // get rid of any extra bits
                        b &= 0xFF;

                        if (b <= 22)
                        {
                            writer.WritePackedInt32(PofHelper.EncodeTinyInt(b));
                            return;
                        }
                        else if (b == 0xFF)
                        {
                            writer.WritePackedInt32(PofConstants.V_INT_NEG_1);
                            return;
                        }
                    }

                    writer.WritePackedInt32(PofConstants.T_OCTET);
                }
                writer.Write((byte) b);
            }
        }

        /// <summary>
        /// Report that a character value has been encountered in the POF
        /// stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="ch">
        /// The character value as a char.
        /// </param>
        public virtual void OnChar(int position, char ch)
        {
            if (ch != 0 || !IsSkippable)
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_CHAR))
                {
                    if (isCompressable)
                    {
                        if (ch <= 22)
                        {
                            writer.WritePackedInt32(PofHelper.EncodeTinyInt(ch));
                            return;
                        }
                        else if (ch == 0xFFFF)
                        {
                            writer.WritePackedInt32(PofConstants.V_INT_NEG_1);
                            return;
                        }
                    }

                    writer.WritePackedInt32(PofConstants.T_CHAR);
                }

                writer.Write(ch);
            }
        }

        /// <summary>
        /// Report that a character string value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="s">
        /// The character string value as a String object.
        /// </param>
        public virtual void OnCharString(int position, string s)
        {
            if (s.Length != 0 || !IsSkippable)
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_CHAR_STRING))
                {
                    if (s.Length == 0 && isCompressable)
                    {
                        writer.WritePackedInt32(PofConstants.V_STRING_ZERO_LENGTH);
                        return;
                    }

                    writer.WritePackedInt32(PofConstants.T_CHAR_STRING);
                }

                writer.Write(s);
            }
        }

        /// <summary>
        /// Report that a octet string value has been encountered in the POF
        /// stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="bin">
        /// The octect string value as a <b>Binary</b> object.
        /// </param>
        public virtual void OnOctetString(int position, Binary bin)
        {
            if (bin.Length != 0 || !IsSkippable)
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_OCTET_STRING))
                {
                    if (bin.Length == 0 && isCompressable)
                    {
                        writer.WritePackedInt32(PofConstants.V_STRING_ZERO_LENGTH);
                        return;
                    }

                    writer.WritePackedInt32(PofConstants.T_OCTET_STRING);
                }

                writer.WritePackedInt32(bin.Length);
                bin.WriteTo(writer.BaseStream);
            }
        }

        /// <summary>
        /// Report that a date value has been encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="year">
        /// The year number as defined by ISO8601; note the difference with
        /// the Java Date class, whose year is relative to 1900.
        /// </param>
        /// <param name="month">
        /// The month number between 1 and 12 inclusive as defined by
        /// ISO8601; note the difference from the Java Date class, whose
        /// month value is 0-based (0-11).
        /// </param>
        /// <param name="day">
        /// The day number between 1 and 31 inclusive as defined by ISO8601.
        /// </param>
        public virtual void OnDate(int position, int year, int month, int day)
        {
            EncodePosition(position);

            DataWriter writer = m_writer;
            if (IsTypeIdEncoded(PofConstants.T_DATE))
            {
                writer.WritePackedInt32(PofConstants.T_DATE);
            }

            PofHelper.WriteDate(writer, year, month, day);
        }

        /// <summary>
        /// Report that a year-month interval value has been encountered
        /// in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="years">
        /// The number of years in the year-month interval.
        /// </param>
        /// <param name="months">
        /// The number of months in the year-month interval.
        /// </param>
        public virtual void OnYearMonthInterval(int position, int years, int months)
        {
            EncodePosition(position);

            DataWriter writer = m_writer;
            if (IsTypeIdEncoded(PofConstants.T_YEAR_MONTH_INTERVAL))
            {
                writer.WritePackedInt32(PofConstants.T_YEAR_MONTH_INTERVAL);
            }

            writer.WritePackedInt32(years);
            writer.WritePackedInt32(months);
        }

        /// <summary>
        /// Report that a time value has been encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="hour">
        /// The hour between 0 and 23 inclusive.
        /// </param>
        /// <param name="minute">
        /// The minute value between 0 and 59 inclusive.
        /// </param>
        /// <param name="second">
        /// The second value between 0 and 59 inclusive (and theoretically 60
        /// for a leap-second).
        /// </param>
        /// <param name="nanosecond">
        /// The nanosecond value between 0 and 999999999 inclusive.
        /// </param>
        /// <param name="isUTC">
        /// <b>true</b> if the time value is UTC or <b>false</b> if the time
        /// value does not have an explicit time zone.
        /// </param>
        public virtual void OnTime(int position, int hour, int minute, int second, int nanosecond, bool isUTC)
        {
            EncodePosition(position);

            DataWriter writer = m_writer;
            if (IsTypeIdEncoded(PofConstants.T_TIME))
            {
                writer.WritePackedInt32(PofConstants.T_TIME);
            }

            PofHelper.WriteTime(writer, hour, minute, second, nanosecond,
                                isUTC ? 1 : 0, TimeSpan.Zero);
        }

        /// <summary>
        /// Report that a time value (with a timezone offset) has been
        /// encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="hour">
        /// The hour between 0 and 23 inclusive.
        /// </param>
        /// <param name="minute">
        /// The minute value between 0 and 59 inclusive.
        /// </param>
        /// <param name="second">
        /// The second value between 0 and 59 inclusive (and theoretically 60
        /// for a leap-second).
        /// </param>
        /// <param name="nano">
        /// The nanosecond value between 0 and 999999999 inclusive.
        /// </param>
        /// <param name="zoneOffset">
        /// The timezone offset from UTC, for example 0 for BST, -5 for EST
        /// and +1 for CET.
        /// </param>
        /// <seealso href="http://www.worldtimezone.com/faq.html">worldtimezone.com</seealso>
        public virtual void OnTime(int position, int hour, int minute, int second,
                                   int nano, TimeSpan zoneOffset)
        {
            EncodePosition(position);

            DataWriter writer = m_writer;
            if (IsTypeIdEncoded(PofConstants.T_TIME))
            {
                writer.WritePackedInt32(PofConstants.T_TIME);
            }

            PofHelper.WriteTime(writer, hour, minute, second, nano, 2, zoneOffset);
        }

        /// <summary>
        /// Report that a time interval value has been encountered in the POF
        /// stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="hours">
        /// The number of hours in the time interval.
        /// </param>
        /// <param name="minutes">
        /// The number of minutes in the time interval, from 0 to 59
        /// inclusive.
        /// </param>
        /// <param name="seconds">
        /// The number of seconds in the time interval, from 0 to 59
        /// inclusive.
        /// </param>
        /// <param name="nanos">
        /// The number of nanoseconds, from 0 to 999999999 inclusive.
        /// </param>
        public virtual void OnTimeInterval(int position, int hours, int minutes, int seconds, int nanos)
        {
            EncodePosition(position);

            DataWriter writer = m_writer;
            if (IsTypeIdEncoded(PofConstants.T_TIME_INTERVAL))
            {
                writer.WritePackedInt32(PofConstants.T_TIME_INTERVAL);
            }

            writer.WritePackedInt32(hours);
            writer.WritePackedInt32(minutes);
            writer.WritePackedInt32(seconds);
            writer.WritePackedInt32(nanos);
        }

        /// <summary>
        /// Report that a date-time value has been encountered in the POF
        /// stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="year">
        /// The year number as defined by ISO8601; note the difference with
        /// the Java Date class, whose year is relative to 1900.
        /// </param>
        /// <param name="month">
        /// The month number between 1 and 12 inclusive as defined by
        /// ISO8601; note the difference from the Java Date class, whose
        /// month value is 0-based (0-11).
        /// </param>
        /// <param name="day">
        /// The day number between 1 and 31 inclusive as defined by ISO8601.
        /// </param>
        /// <param name="hour">
        /// The hour between 0 and 23 inclusive.
        /// </param>
        /// <param name="minute">
        /// The minute value between 0 and 59 inclusive.
        /// </param>
        /// <param name="second">
        /// The second value between 0 and 59 inclusive (and theoretically 60
        /// for a leap-second).
        /// </param>
        /// <param name="nano">
        /// The nanosecond value between 0 and 999999999 inclusive.
        /// </param>
        /// <param name="isUTC">
        /// <b>true</b> if the time value is UTC or <b>false</b> if the time
        /// value does not have an explicit time zone.
        /// </param>
        public virtual void OnDateTime(int position, int year, int month, int day,
                                       int hour, int minute, int second, int nano, bool isUTC)
        {
            EncodePosition(position);

            DataWriter writer = m_writer;
            if (IsTypeIdEncoded(PofConstants.T_DATETIME))
            {
                writer.WritePackedInt32(PofConstants.T_DATETIME);
            }

            PofHelper.WriteDate(writer, year, month, day);
            PofHelper.WriteTime(writer, hour, minute, second, nano, isUTC ? 1 : 0, TimeSpan.Zero);
        }

        /// <summary>
        /// Report that a date-time value (with a timezone offset) has been
        /// encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="year">
        /// The year number as defined by ISO8601; note the difference with
        /// the Java Date class, whose year is relative to 1900.
        /// </param>
        /// <param name="month">
        /// The month number between 1 and 12 inclusive as defined by
        /// ISO8601; note the difference from the Java Date class, whose
        /// month value is 0-based (0-11).
        /// </param>
        /// <param name="day">
        /// The day number between 1 and 31 inclusive as defined by ISO8601.
        /// </param>
        /// <param name="hour">
        /// The hour between 0 and 23 inclusive.
        /// </param>
        /// <param name="minute">
        /// The minute value between 0 and 59 inclusive.
        /// </param>
        /// <param name="second">
        /// The second value between 0 and 59 inclusive (and theoretically 60
        /// for a leap-second).
        /// </param>
        /// <param name="nano">
        /// The nanosecond value between 0 and 999999999 inclusive.
        /// </param>
        /// <param name="zoneOffset">
        /// The timezone offset from UTC, for example 0 for BST, -5 for EST
        /// and +1 for CET.
        /// </param>
        public virtual void OnDateTime(int position, int year, int month, int day,
                                       int hour, int minute, int second,
                                       int nano, TimeSpan zoneOffset)
        {
            EncodePosition(position);

            DataWriter writer = m_writer;
            if (IsTypeIdEncoded(PofConstants.T_DATETIME))
            {
                writer.WritePackedInt32(PofConstants.T_DATETIME);
            }

            PofHelper.WriteDate(writer, year, month, day);
            PofHelper.WriteTime(writer, hour, minute, second,
                                nano, 2, zoneOffset);
        }

        /// <summary>
        /// Report that a day-time interval value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="days">
        /// The number of days in the day-time interval.
        /// </param>
        /// <param name="hours">
        /// The number of hours in the day-time interval, from 0 to 23
        /// inclusive.
        /// </param>
        /// <param name="minutes">
        /// The number of minutes in the day-time interval, from 0 to 59
        /// inclusive.
        /// </param>
        /// <param name="seconds">
        /// The number of seconds in the day-time interval, from 0 to 59
        /// inclusive.
        /// </param>
        /// <param name="nanos">
        /// The number of nanoseconds in the day-time interval, from 0 to
        /// 999999999 inclusive.
        /// </param>
        public virtual void OnDayTimeInterval(int position, int days, int hours,
                                                int minutes, int seconds, int nanos)
        {
            EncodePosition(position);

            DataWriter writer = m_writer;
            if (IsTypeIdEncoded(PofConstants.T_DAY_TIME_INTERVAL))
            {
                writer.WritePackedInt32(PofConstants.T_DAY_TIME_INTERVAL);
            }
            writer.WritePackedInt32(days);
            writer.WritePackedInt32(hours);
            writer.WritePackedInt32(minutes);
            writer.WritePackedInt32(seconds);
            writer.WritePackedInt32(nanos);
        }

        /// <summary>
        /// Report that a collection of values has been encountered in the
        /// POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>elements</tt> elements in
        /// the collection, and the collection extent will then be terminated
        /// by a call to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="elements">
        /// The exact number of values (elements) in the collection.
        /// </param>
        public virtual void BeginCollection(int position, int elements)
        {
            if (elements == 0 && IsSkippable)
            {
                // dummy complex type (no contents, no termination)
                m_complex = new Complex(m_complex, false);
            }
            else
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_COLLECTION))
                {
                    if (elements == 0 && isCompressable)
                    {
                        writer.WritePackedInt32(PofConstants.V_COLLECTION_EMPTY);
                        m_complex = new Complex(m_complex, false);
                        return;
                    }

                    writer.WritePackedInt32(PofConstants.T_COLLECTION);
                }

                writer.WritePackedInt32(elements);

                m_complex = new Complex(m_complex, false);
            }
        }

        /// <summary>
        /// Report that a uniform collection of values has been encountered
        /// in the POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>cElements</tt> elements in
        /// the collection, and the collection extent will then be terminated
        /// by a call to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of values (elements) in the collection.
        /// </param>
        /// <param name="typeId">
        /// The type identifier for all of the values in the uniform
        /// collection.
        /// </param>
        public virtual void BeginUniformCollection(int position, int elementCount, int typeId)
        {
            if (elementCount == 0 && IsSkippable)
            {
                // dummy complex type (no contents, no termination)
                m_complex = new Complex(m_complex, false);
            }
            else
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_UNIFORM_COLLECTION))
                {
                    if (elementCount == 0 && isCompressable)
                    {
                        writer.WritePackedInt32(PofConstants.V_COLLECTION_EMPTY);
                        m_complex = new Complex(m_complex, false, typeId);
                        return;
                    }

                    writer.WritePackedInt32(PofConstants.T_UNIFORM_COLLECTION);
                }

                writer.WritePackedInt32(typeId);
                writer.WritePackedInt32(elementCount);

                m_complex = new Complex(m_complex, false, typeId);
            }
        }

        /// <summary>
        /// Report that an array of values has been encountered in the POF
        /// stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>cElements</tt> elements in
        /// the array, and the array extent will then be terminated by a call
        /// to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of values (elements) in the array.
        /// </param>
        public virtual void BeginArray(int position, int elementCount)
        {
            if (elementCount == 0 && IsSkippable)
            {
                // dummy complex type (no contents, no termination)
                m_complex = new Complex(m_complex, false);
            }
            else
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_ARRAY))
                {
                    if (elementCount == 0 && isCompressable)
                    {
                        writer.WritePackedInt32(PofConstants.V_COLLECTION_EMPTY);
                        m_complex = new Complex(m_complex, false);
                        return;
                    }

                    writer.WritePackedInt32(PofConstants.T_ARRAY);
                }

                writer.WritePackedInt32(elementCount);

                m_complex = new Complex(m_complex, false);
            }
        }

        /// <summary>
        /// Report that a uniform array of values has been encountered in the
        /// POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>cElements</tt> elements in
        /// the array, and the array extent will then be terminated by a call
        /// to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of values (elements) in the array.
        /// </param>
        /// <param name="typeId">
        /// The type identifier for all of the values in the uniform array.
        /// </param>
        public virtual void BeginUniformArray(int position, int elementCount, int typeId)
        {
            if (elementCount == 0 && IsSkippable)
            {
                // dummy complex type (no contents, no termination)
                m_complex = new Complex(m_complex, false);
            }
            else
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_UNIFORM_ARRAY))
                {
                    if (elementCount == 0 && isCompressable)
                    {
                        writer.WritePackedInt32(PofConstants.V_COLLECTION_EMPTY);
                        m_complex = new Complex(m_complex, false, typeId);
                        return;
                    }

                    writer.WritePackedInt32(PofConstants.T_UNIFORM_ARRAY);
                }

                writer.WritePackedInt32(typeId);
                writer.WritePackedInt32(elementCount);

                m_complex = new Complex(m_complex, false, typeId);
            }
        }

        /// <summary>
        /// Report that a sparse array of values has been encountered in the
        /// POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for present element in the sparse array (up to
        /// <paramref name="elementCount"/> elements), and the array extent
        /// will then be terminated by a call to
        /// <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of elements in the array, which is greater than
        /// or equal to the number of values in the sparse POF stream; in
        /// other words, the number of values that will subsequently be
        /// reported will not exceed this number.
        /// </param>
        public virtual void BeginSparseArray(int position, int elementCount)
        {
            if (elementCount == 0 && IsSkippable)
            {
                // dummy complex type (no contents, no termination)
                m_complex = new Complex(m_complex, false);
            }
            else
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                bool       isTerminated = true;
                DataWriter writer       = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_SPARSE_ARRAY))
                {
                    if (elementCount == 0 && isCompressable)
                    {
                        writer.WritePackedInt32(PofConstants.V_COLLECTION_EMPTY);
                        isTerminated = false;
                        m_complex    = new Complex(m_complex, isTerminated);
                        return;
                    }

                    writer.WritePackedInt32(PofConstants.T_SPARSE_ARRAY);
                }

                writer.WritePackedInt32(elementCount);

                m_complex = new Complex(m_complex, isTerminated);
            }
        }

        /// <summary>
        /// Report that a uniform sparse array of values has been encountered
        /// in the POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for present element in the sparse array (up to
        /// <tt>elements</tt> elements), and the array extent will then be
        /// terminated by a call to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="elements">
        /// The exact number of elements in the array, which is greater than
        /// or equal to the number of values in the sparse POF stream; in
        /// other words, the number of values that will subsequently be
        /// reported will not exceed this number.
        /// </param>
        /// <param name="typeId">
        /// The type identifier for all of the values in the uniform sparse
        /// array.
        /// </param>
        public virtual void BeginUniformSparseArray(int position, int elements, int typeId)
        {
            if (elements == 0 && IsSkippable)
            {
                // dummy complex type (no contents, no termination)
                m_complex = new Complex(m_complex, false);
            }
            else
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                bool       isTerminated = true;
                DataWriter writer       = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_UNIFORM_SPARSE_ARRAY))
                {
                    if (elements == 0 && isCompressable)
                    {
                        writer.WritePackedInt32(PofConstants.V_COLLECTION_EMPTY);
                        isTerminated = false;
                        m_complex    = new Complex(m_complex, isTerminated, typeId);
                        return;
                    }

                    writer.WritePackedInt32(PofConstants.T_UNIFORM_SPARSE_ARRAY);
                }

                writer.WritePackedInt32(typeId);
                writer.WritePackedInt32(elements);

                m_complex = new Complex(m_complex, isTerminated, typeId);
            }
        }

        /// <summary>
        /// Report that a map of key/value pairs has been encountered in the
        /// POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>elements</tt> elements in
        /// the map, and the map extent will then be terminated by a call to
        /// <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="elements">
        /// The exact number of key/value pairs (entries) in the map.
        /// </param>
        public virtual void BeginMap(int position, int elements)
        {
            if (elements == 0 && IsSkippable)
            {
                // dummy complex type (no contents, no termination)
                m_complex = new Complex(m_complex, false);
            }
            else
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_MAP))
                {
                    if (elements == 0 && isCompressable)
                    {
                        writer.WritePackedInt32(PofConstants.V_COLLECTION_EMPTY);
                        m_complex = new Complex(m_complex, false);
                        return;
                    }

                    writer.WritePackedInt32(PofConstants.T_MAP);
                }

                writer.WritePackedInt32(elements);

                m_complex = new Complex(m_complex, false);
            }
        }

        /// <summary>
        /// Report that a map of key/value pairs (with the keys being of a
        /// uniform type) has been encountered in the POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>elements</tt> elements in
        /// the map, and the map extent will then be terminated by a call to
        /// <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="elements">
        /// The exact number of key/value pairs (entries) in the map.
        /// </param>
        /// <param name="keysTypeId">
        /// The type identifier for all of the keys in the uniform-keys map.
        /// </param>
        public virtual void BeginUniformKeysMap(int position, int elements, int keysTypeId)
        {
            if (elements == 0 && IsSkippable)
            {
                // dummy complex type (no contents, no termination)
                m_complex = new Complex(m_complex, false);
            }
            else
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_UNIFORM_KEYS_MAP))
                {
                    if (elements == 0 && isCompressable)
                    {
                        writer.WritePackedInt32(PofConstants.V_COLLECTION_EMPTY);
                        m_complex = new ComplexMap(m_complex, keysTypeId);
                        return;
                    }

                    writer.WritePackedInt32(PofConstants.T_UNIFORM_KEYS_MAP);
                }

                writer.WritePackedInt32(keysTypeId);
                writer.WritePackedInt32(elements);

                m_complex = new ComplexMap(m_complex, keysTypeId);
            }
        }

        /// <summary>
        /// Report that a map of key/value pairs (with the keys being of a
        /// uniform type and the values being of a uniform type) has been
        /// encountered in the POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>elements</tt> elements in
        /// the map, and the map extent will then be terminated by a call to
        /// <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="elements">
        /// The exact number of key/value pairs (entries) in the map.
        /// </param>
        /// <param name="keysTypeId">
        /// The type identifier for all of the keys in the uniform map.
        /// </param>
        /// <param name="valuesTypeId">
        /// The type identifier for all of the values in the uniform map.
        /// </param>
        public virtual void BeginUniformMap(int position, int elements, int keysTypeId, int valuesTypeId)
        {
            if (elements == 0 && IsSkippable)
            {
                // dummy complex type (no contents, no termination)
                m_complex = new Complex(m_complex, false);
            }
            else
            {
                bool isCompressable = IsCompressable;
                EncodePosition(position);

                DataWriter writer = m_writer;

                if (IsTypeIdEncoded(PofConstants.T_UNIFORM_MAP))
                {
                    if (elements == 0 && isCompressable)
                    {
                        writer.WritePackedInt32(PofConstants.V_COLLECTION_EMPTY);
                        m_complex = new ComplexMap(m_complex, keysTypeId, valuesTypeId);
                        return;
                    }

                    writer.WritePackedInt32(PofConstants.T_UNIFORM_MAP);
                }

                writer.WritePackedInt32(keysTypeId);
                writer.WritePackedInt32(valuesTypeId);
                writer.WritePackedInt32(elements);

                m_complex = new ComplexMap(m_complex, keysTypeId, valuesTypeId);
            }
        }

        /// <summary>
        /// Report that a value of a "user type" has been encountered in the
        /// POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// A user type is analogous to a "class", and a value of a user type
        /// is analogous to an "object".</p>
        /// <p>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the property values in the user
        /// type, and the user type will then be terminated by a call to
        /// <see cref="EndComplexValue"/>.</p>
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property position within
        /// a user type, array position within an array, element counter
        /// within a collection, entry counter within a map, -1 otherwise.
        /// </param>
        /// <param name="nId">
        /// Identity of the object to encode, or -1 if identity
        /// shouldn't be encoded in the POF stream.
        /// </param>
        /// <param name="userTypeId">
        /// The user type identifier, <tt>(userTypeId &gt;= 0)</tt>.
        /// </param>
        /// <param name="versionId">
        /// The version identifier for the user data type data in the POF
        /// stream, <tt>(versionId &gt;= 0)</tt>.
        /// </param>
        public virtual void BeginUserType(int position, int nId, 
            int userTypeId, int versionId)
        {
            EncodePosition(position);

            DataWriter writer = m_writer;
            RegisterIdentity(nId);
            if (IsTypeIdEncoded(userTypeId))
            {
                writer.WritePackedInt32(userTypeId);
            }

            writer.WritePackedInt32(versionId);

            m_complex = new Complex(m_complex, true);
        }

        /// <summary>
        /// Signifies the termination of the current complex value.
        /// </summary>
        /// <remarks>
        /// Complex values are any of the collection, array, map and user
        /// types. For each call to one of the "begin" methods, there will be
        /// a corresponding call to this method, even if there were no
        /// contents in the complex value.
        /// </remarks>
        public virtual void EndComplexValue()
        {
            Complex complex = m_complex;

            if (complex.IsSparse)
            {
                m_writer.WritePackedInt32(-1);
            }

            m_complex = complex.Pop();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Obtain the current Complex object that represents the complex
        /// type that is being written to the POF stream.
        /// </summary>
        /// <returns>
        /// The current Complex object.
        /// </returns>
        protected internal virtual Complex GetComplex()
        {
            return m_complex;
        }

        /// <summary>
        /// Called for each and every value going into the POF stream, in
        /// case the value needs its position to be encoded into the stream.
        /// </summary>
        /// <param name="position">
        /// The position (property position, array position, etc.)
        /// </param>
        protected internal virtual void EncodePosition(int position)
        {
            Complex complex = m_complex;
            if (complex != null)
            {
                complex.OnValue(position);

                if (position >= 0 && complex.IsSparse)
                {
                    m_writer.WritePackedInt32(position);
                }
            }

            // once the position is encoded, the "has identity" flag is reset
            m_hasIdentity = false;
        }

        /// <summary>
        /// Determine if the type should be encoded for the current value.
        /// </summary>
        /// <param name="typeId">
        /// The type of the current value.
        /// </param>
        /// <returns>
        /// <b>true</b> if the type ID should be placed into the POF stream,
        /// and <b>false</b> if only the value itself should be placed into
        /// the stream.
        /// </returns>
        protected internal virtual bool IsTypeIdEncoded(int typeId)
        {
            Complex complex = m_complex;

            // if the type is not being encoded, it must match the expected uniform type
            Debug.Assert(complex == null || !complex.IsUniform || typeId == complex.UniformType
                || typeId == PofConstants.T_REFERENCE);
            return complex == null || !complex.IsUniform;
        }

        #endregion

        #region Inner class: Complex

        /// <summary>
        /// A Complex object represents the current complex data structure in
        /// the POF stream.
        /// </summary>
        public class Complex
        {
            #region Properties

            /// <summary>
            /// Determine if the object encoding within the Complex type is
            /// uniform.
            /// </summary>
            /// <value>
            /// <b>true</b> if values within the Complex type are of a
            /// uniform type and are encoded uniformly.
            /// </value>
            public virtual bool IsUniform
            {
                get { return m_isUniform; }
            }

            /// <summary>
            /// If the object encoding is using uniform encoding, obtain the
            /// type id of the uniform type.
            /// </summary>
            /// <value>
            /// The type id used for the uniform encoding.
            /// </value>
            public virtual int UniformType
            {
                get { return m_typeId; }
            }

            /// <summary>
            /// Determine if the position information is encoded with the
            /// values of the complex type, and if the Complex type is
            /// terminated in the POF stream with an illegal position (-1).
            /// </summary>
            /// <value>
            /// <b>true</b> iff the complex value is a sparse type.
            /// </value>
            public virtual bool IsSparse
            {
                get { return m_isSparse; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Construct a Complex object for a data collection or user
            /// type.
            /// </summary>
            /// <param name="complexCurrent">
            /// The current Complex object or <c>null</c>.
            /// </param>
            /// <param name="encodePosition">
            /// <b>true</b> to encode the position information.
            /// </param>
            public Complex(Complex complexCurrent, bool encodePosition)
            {
                m_outerComplex = complexCurrent;
                m_isSparse     = encodePosition;
            }

            /// <summary>
            /// Construct a Complex object for a uniformly-typed data
            /// collection.
            /// </summary>
            /// <param name="complexCurrent">
            /// The current Complex object or <c>null</c>.
            /// </param>
            /// <param name="encodePosition">
            /// <b>true</b> to encode the position information.
            /// </param>
            /// <param name="uniformTypeId">
            /// The type identifier of the uniform type.
            /// </param>
            public Complex(Complex complexCurrent, bool encodePosition, int uniformTypeId)
                : this(complexCurrent, encodePosition)
            {
                m_isUniform = true;
                m_typeId    = uniformTypeId;
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Notify the Complex object that a value has been encountered.
            /// </summary>
            /// <param name="position">
            /// The position that accomponied the value.
            /// </param>
            public virtual void OnValue(int position)
            {}

            /// <summary>
            /// Pop this Complex object off the stack, returning the outer
            /// Complex object or <c>null</c> if there is none.
            /// </summary>
            /// <returns>
            /// The outer Complex object or <c>null</c> if there is none.
            /// </returns>
            public virtual Complex Pop()
            {
                return m_outerComplex;
            }

            #endregion

            #region Data members

            /// <summary>
            /// Whether or not the position information is encoded.
            /// </summary>
            private readonly bool m_isSparse;

            /// <summary>
            /// Whether or not values within the complex type are uniformly
            /// encoded.
            /// </summary>
            private readonly bool m_isUniform;

            /// <summary>
            /// The type ID, if uniform encoding is used.
            /// </summary>
            private readonly int m_typeId;

            /// <summary>
            /// The Complex within which this Complex exists, to support
            /// nesting.
            /// </summary>
            private readonly Complex m_outerComplex;

            #endregion
        }

        #endregion

        #region Inner class: ComplexMap

        /// <summary>
        /// A ComplexMap object represents a map data structure (with uniform
        /// keys or with uniform keys and values) in the POF stream.
        /// </summary>
        public class ComplexMap : Complex
        {
            #region Properties

            /// <summary>
            /// Determine if the object encoding within the Complex type is
            /// uniform.
            /// </summary>
            /// <value>
            /// <b>true</b> if values within the Complex type are of a
            /// uniform type and are encoded uniformly.
            /// </value>
            public override bool IsUniform
            {
                get { return m_isKey ? base.IsUniform : m_isUniformValue; }
            }

            /// <summary>
            /// If the object encoding is using uniform encoding, obtain the
            /// type id of the uniform type.
            /// </summary>
            /// <returns>
            /// The type id used for the uniform encoding.
            /// </returns>
            public override int UniformType
            {
                get { return m_isKey ? base.UniformType : m_valueTypeId; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Construct a ComplexMap object for maps with uniformly-typed
            /// keys.
            /// </summary>
            /// <param name="complexCurrent">
            /// The current Complex object or <c>null</c>.
            /// </param>
            /// <param name="uniformKeyTypeId">
            /// The type identifier of the uniform type.
            /// </param>
            public ComplexMap(Complex complexCurrent, int uniformKeyTypeId)
                : base(complexCurrent, false, uniformKeyTypeId)
            {}

            /// <summary>
            /// Construct a ComplexMap object for maps with uniformly-typed
            /// keys and values.
            /// </summary>
            /// <param name="complexCurrent">
            /// The current Complex object or <c>null</c>.
            /// </param>
            /// <param name="uniformKeyTypeId">
            /// The type identifier of the uniform type for keys in the map.
            /// </param>
            /// <param name="uniformValTypeId">
            /// The type identifier of the uniform type for values in the
            /// map.
            /// </param>
            public ComplexMap(Complex complexCurrent, int uniformKeyTypeId, int uniformValTypeId)
                : this(complexCurrent, uniformKeyTypeId)
            {
                m_isUniformValue = true;
                m_valueTypeId    = uniformValTypeId;
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Notify the Complex object that a value has been encountered.
            /// </summary>
            /// <param name="position">
            /// The position that accomponied the value.
            /// </param>
            public override void OnValue(int position)
            {
                m_isKey = !m_isKey;
            }

            #endregion

            #region Data members

            /// <summary>
            /// Toggles between key and value processing every time the
            /// caller invokes <see cref="OnValue"/>.
            /// </summary>
            private bool m_isKey;

            /// <summary>
            /// Whether or not values within the map are uniformly encoded.
            /// </summary>
            private readonly bool m_isUniformValue;

            /// <summary>
            /// The value type ID, if uniform encoding is used for values.
            /// </summary>
            private readonly int m_valueTypeId;

            #endregion
        }

        #endregion

        #region Data members

        /// <summary>
        /// The DataWriter to write to.
        /// </summary>
        private DataWriter m_writer;

        /// <summary>
        /// The current containing Complex value in the POF stream.
        /// </summary>
        private Complex m_complex;

        /// <summary>
        /// Set to true when the next value to write has been tagged with an identity.
        /// </summary>
        private bool m_hasIdentity;

        #endregion
    }
}