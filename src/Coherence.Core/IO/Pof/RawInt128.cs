/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// An immutable POF <b>RawInt128</b> value.
    /// </summary>
    /// <author>Goran Milosavljevic  2008.01.30</author>
    public struct RawInt128
    {
        #region Constructors

        /// <summary>
        /// Constructs an <b>RawInt128</b> value.
        /// </summary>
        /// <param name="bytes">
        /// The array of signed bytes representing <b>RawInt128</b> value.
        /// </param>
        public RawInt128(byte[] bytes) : this(bytes, false)
        { }

        /// <summary>
        /// Constructs an <b>RawInt128</b> value.
        /// </summary>
        /// <param name="bytes">
        /// The array of signed bytes representing <b>RawInt128</b> value.
        /// </param>
        public RawInt128(sbyte[] bytes) : this(bytes, false)
        { }

        /// <summary>
        /// Constructs an <b>RawInt128</b> value.
        /// </summary>
        /// <param name="bytes">
        /// The array of signed bytes representing <b>RawInt128</b> value.
        /// </param>
        /// <param name="isNegative">
        /// Flag representing whether this <b>RawInt128</b> value is a
        /// negative number.
        /// </param>
        public RawInt128(sbyte[] bytes, bool isNegative)
        {
            m_bytes      = bytes;
            m_isNegative = isNegative;
        }

        /// <summary>
        /// Constructs an <b>RawInt128</b> value.
        /// </summary>
        /// <param name="bytes">
        /// The array of bytes representing <b>RawInt128</b> value.
        /// </param>
        /// <param name="isNegative">
        /// Flag representing whether this <b>RawInt128</b> value is a
        /// negative number.
        /// </param>
        public RawInt128(byte[] bytes, bool isNegative) : this(CollectionUtils.ToSByteArrayUnchecked(bytes), isNegative)
        { }                

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare this object with another for equality.
        /// </summary>
        /// <param name="o">
        /// Another object to compare to for equality.
        /// </param>
        /// <returns>
        /// <b>true</b> if this object is equal to the other object.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is RawInt128)
            {
                RawInt128 that = (RawInt128) o;

                if (this.m_bytes == null && that.m_bytes == null)
                {
                    return true;
                }
                else
                {
                    if (this.m_bytes == null || that.m_bytes == null)
                    {
                        return false;
                    }
                }

                int c = this.m_bytes.Length;
                if (c == that.m_bytes.Length)
                {
                    for (int i = 0; i < c; i++)
                    {
                        if (this.m_bytes[i] != that.m_bytes[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Obtain the hashcode for this object.
        /// </summary>
        /// <returns>
        /// An integer hashcode.
        /// </returns>
        public override int GetHashCode()
        {
            sbyte[] bytes = m_bytes;
            int count     = bytes.Length;
            int hash      = 0;
            for (int i = 0; i < count; i++)
            {
                hash += bytes[i].GetHashCode();
            }

            return hash;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Int128 value as array of byte values.
        /// </summary>
        /// <value>
        /// Array of signed bytes representing Int128.
        /// </value>
        public sbyte[] Value
        {
            get { return m_bytes; }
        }

        /// <summary>
        /// Returns the size of <b>RawInt128</b> value.
        /// </summary>
        /// <value>
        /// The size of <b>RawInt128</b>.
        /// </value>
        public int Length
        {
            get { return m_bytes == null ? 0 : m_bytes.Length; }
        }

        /// <summary>
        /// Returns if this <b>RawInt128</b> is a negative number.
        /// </summary>
        public bool IsNegative
        {
            get { return m_isNegative; }
        }

        /// <summary>
        /// Gets if this is a zero value.
        /// </summary>
        public bool IsZero
        {
            get
            {
                sbyte[] bytes = Value;
                if (bytes == null)
                {
                    return true;
                }

                foreach (sbyte b in bytes)
                {
                    if (b != 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        #endregion

        #region RawInt128 methods

        /// <summary>
        /// Returns <b>Decimal</b> value of this object.
        /// </summary>
        /// <returns>
        /// <b>Decimal</b> value of this object.
        /// </returns>
        public Decimal ToDecimal()
        {
            return ToDecimal(0);
        }

        /// <summary>
        /// Returns <b>Decimal</b> value of this object with given scale.
        /// </summary>
        /// <param name="scale">
        /// Scale value used for constructing a <b>Decimal</b> result.
        /// </param>
        /// <returns>
        /// <b>Decimal</b> value of this object.
        /// </returns>
        public Decimal ToDecimal(byte scale)
        {
            RawInt128 rawInt128 = this;            

            if (rawInt128.Length > 12)
            {
                throw new OverflowException("the value is out of range of a Decimal");
            }

            bool isNeg = rawInt128.IsNegative;

            // if it's negative number, do negation
            if (isNeg)
            {
                for (int i = 0; i < rawInt128.Length; ++i)
                {
                    rawInt128.Value[i] = (sbyte) ~rawInt128.Value[i];
                }
            }

            int[]   decimals      = NumberUtils.EncodeDecimalBits(this);
            Decimal decimalResult = new Decimal(decimals[0], decimals[1], decimals[2], isNeg, scale);
            if (isNeg)
            {
                Decimal d1 = new Decimal(1, 0, 0, false, scale);
                decimalResult -= d1;
            }

            return decimalResult;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The array of signed bytes representing Int128 value.
        /// </summary>
        private readonly sbyte[] m_bytes;

        /// <summary>
        /// Flag representing whether this byte representation is a negative
        /// number.
        /// </summary>
        private readonly bool m_isNegative;

        #endregion
    }
}