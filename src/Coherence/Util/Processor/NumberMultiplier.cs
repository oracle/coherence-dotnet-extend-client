/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// The <b>NumberMultiplier</b> entry processor is used to multiply a
    /// property value of a Byte, Int16, Int32, Int64, Single, Double,
    /// BigInteger and BigDecimal type.
    /// </summary>
    /// <author>Gene Gleyzer  2005.10.31</author>
    /// <author>Ivan Cikic  2006.10.24</author>
    /// <since>Coherence 3.1</since>
    public class NumberMultiplier : PropertyProcessor
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NumberMultiplier()
        {}

        /// <summary>
        /// Construct an <b>NumberMultiplier</b> processor that will
        /// multiply a property value by a specified factor, returning
        /// either the old or the new value as specified.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The .NET type of the original property value will dictate the way
        /// the specified factor is interpreted. For example, applying a
        /// factor of Double(0.5) to a property value of Int32(4) will
        /// result in a new property value of Int32(2).</p>
        /// <p>
        /// If the original property value is <c>null</c>, the .NET type of
        /// the numFactor parameter will dictate the .NET type of the new
        /// value.</p>
        /// </remarks>
        /// <param name="name">
        /// The property name.
        /// </param>
        /// <param name="numInc">
        /// The object representing the magnitude and sign of the multiplier.
        /// </param>
        /// <param name="postIncrement">
        /// Pass <b>true</b> to return the value as it was before it was
        /// multiplied, or pass <b>false</b> to return the value as it is
        /// after it is multiplied.
        /// </param>
        public NumberMultiplier(string name, object numInc, bool postIncrement) : base(name)
        {
            Debug.Assert(numInc != null);
            if (!NumberUtils.IsNumber(numInc))
            {
                throw new ArgumentException("Bad parameter format! " +
                                            numInc.GetType().Name + " is not supported type.");
            }
            m_numFactor  = numInc;
            m_postFactor = postIncrement;
        }

        /// <summary>
        /// Construct an <b>NumberMultiplier</b> processor that will
        /// increment a property value by a specified amount, returning
        /// either the old or the new value as specified.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The .NET type of the original property value will dictate the way
        /// the specified factor is interpreted. For example, applying a
        /// factor of Double(0.5) to a property value of Int32(4) will
        /// result in a new property value of Int32(2).</p>
        /// <p>
        /// If the original property value is <c>null</c>, the .NET type of
        /// the numFactor parameter will dictate the .NET type of the new
        /// value.</p>
        /// </remarks>
        /// <param name="manipulator">
        /// The <see cref="PropertyManipulator"/>; could be <c>null</c>.
        /// </param>
        /// <param name="numInc">
        /// The object representing the magnitude and sign of the multiplier.
        /// </param>
        /// <param name="postIncrement">
        /// Pass <b>true</b> to return the value as it was before it was
        /// multiplied, or pass <b>false</b> to return the value as it is
        /// after it is multiplied.
        /// </param>
        public NumberMultiplier(PropertyManipulator manipulator,
                                 object numInc,
                                 bool postIncrement) : base(manipulator)
        {
            Debug.Assert(numInc != null);
            if (!NumberUtils.IsNumber(numInc))
            {
                throw new ArgumentException("Bad parameter format! " +
                                            numInc.GetType().Name + " is not supported type.");
            }
            m_numFactor  = numInc;
            m_postFactor = postIncrement;
        }

        #endregion

        #region IEntryProcess implementation

        /// <summary>
        /// Process an <see cref="IInvocableCacheEntry"/>.
        /// </summary>
        /// <param name="entry">
        /// The <b>IInvocableCacheEntry</b> to process.
        /// </param>
        /// <returns>
        /// The result of the processing, if any.
        /// </returns>
        public override object Process(IInvocableCacheEntry entry)
        {
            if (!entry.IsPresent)
            {
                return null;
            }

            object numFactor = m_numFactor;

            object numOld = Get(entry);
            if (numOld == null)
            {
                if (NumberUtils.IsNumber(numFactor))
                {
                    numOld = 0;
                }
            }

            object numNew;
            if (numOld is Int32)
            {
                Int32 i_32 = (Int32) numOld;
                if (numFactor is Double || numFactor is Single)
                {
                    i_32 = (Int32)(i_32 * (Double) numFactor);
                }
                else
                {
                    i_32 *= Convert.ToInt32(numFactor);
                }
                numNew = i_32;
            }
            else if (numOld is Int64)
            {
                Int64 i_64 = (Int64) numOld;
                if (numFactor is Double || numFactor is Single)
                {
                    i_64 = (Int64) (i_64 * (Double) numFactor);
                }
                else
                {
                    i_64 *= Convert.ToInt64(numFactor);
                }
                numNew = i_64;
            }
            else if (numOld is Double)
            {
                numNew = Convert.ToDouble(numOld) * Convert.ToDouble(numFactor);
            }
            else if (numOld is Single)
            {
                numNew = Convert.ToSingle(numOld) * Convert.ToSingle(numFactor);
            }
            else if (numOld is Decimal)
            {
                numNew = Decimal.Multiply((Decimal) numOld, Convert.ToDecimal(numFactor));
            }
            else if (numOld is Int16)
            {
                Int16 i_16 = (Int16) numOld;
                if (numFactor is Double || numFactor is Single)
                {
                    i_16 = (Int16)(i_16 * (Double) numFactor);
                }
                else
                {
                    i_16 *= Convert.ToInt16(numFactor);
                }
                numNew = i_16;
            }
            else if (numOld is Byte)
            {
                byte b = (Byte) numOld;
                if (numFactor is Double || numFactor is Single)
                {
                    b = (Byte)(b * (Double) numFactor);
                }
                else
                {
                    b *= Convert.ToByte(numFactor);
                }
                numNew = b;
            }
            else if (numOld is RawInt128)
            {
                Decimal newDec = Decimal.Multiply(((RawInt128)numOld).ToDecimal(), Convert.ToDecimal(numFactor));
                numNew         = NumberUtils.DecimalToRawInt128(newDec);
            }
            else
            {
                throw new Exception("Unsupported type:" +
                                    (numOld == null ?
                                     numFactor.GetType().Name :
                                     numOld.GetType().Name));
            }

            if (!numNew.Equals(numOld))
            {
                Set(entry, numNew);
            }
            return m_postFactor ? numOld : numNew;
        }

        #endregion

        #region Helper methods

        /// <summary>
        ///  Returns this <b>PropertyProcessor</b>'s description.
        /// </summary>
        /// <returns>
        /// This <b>PropertyProcessor</b>'s description.
        /// </returns>
        protected override string Description
        {
            get
            {
                return (m_postFactor ? ", post" : ", pre") + "-increment=" + m_numFactor;
            }
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);
            m_numFactor  = reader.ReadObject(1);
            m_postFactor = reader.ReadBoolean(2);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);
            writer.WriteObject(1, m_numFactor);
            writer.WriteObject(2, m_postFactor);
        }

        #endregion

        #region Data member

        /// <summary>
        /// The number to multiply by.
        /// </summary>
        private object m_numFactor;

        /// <summary>
        /// Whether to return the value before it was multiplied
        /// ("post-increment") or after it is multiplied ("pre-increment").
        /// </summary>
        private bool m_postFactor;

        #endregion
    }
}