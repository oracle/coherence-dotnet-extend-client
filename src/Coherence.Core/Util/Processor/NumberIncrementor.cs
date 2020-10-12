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
    /// The <b>NumberIncrementor</b> entry processor is used to increment a
    /// property value of a Byte, Int16, Int32, Int64, Single, Double and
    /// Decimal type.
    /// </summary>
    /// <author>Gene Gleyzer  2005.10.31</author>
    /// <author>Ivan Cikic  2006.10.24</author>
    /// <since>Coherence 3.1</since>
    public class NumberIncrementor : PropertyProcessor
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NumberIncrementor()
        {}

        /// <summary>
        /// Construct an <b>NumberIncrementor</b> processor that will
        /// increment a property value by a specified amount, returning
        /// either the old or the new value as specified.
        /// </summary>
        /// <remarks>
        /// The .NET type of the <paramref name="numInc"/> parameter will
        /// dictate the .NET type of the original and the new value.
        /// </remarks>
        /// <param name="name">
        /// The property name.
        /// </param>
        /// <param name="numInc">
        /// The object representing the magnitude and sign of the increment.
        /// </param>
        /// <param name="postIncrement">
        /// Pass <b>true</b> to return the value as it was before it was
        /// incremented, or pass <b>false</b> to return the value as it is
        /// after it is incremented.
        /// </param>
        public NumberIncrementor(string name, object numInc, bool postIncrement) : base(name)
        {
            if (numInc == null)
            {
                throw new ArgumentNullException("numInc", "Argument 'numInc' cannot be null.");
            }
            if (numInc != null && !NumberUtils.IsNumber(numInc))
            {
                throw new ArgumentException("Bad parameter format! " +
                                            numInc.GetType().Name + " is not supported type.");
            }
            m_numInc  = numInc;
            m_postInc = postIncrement;
        }

        /// <summary>
        /// Construct an <b>NumberIncrementor</b> processor that will
        /// increment a property value by a specified amount, returning
        /// either the old or the new value as specified.
        /// </summary>
        /// <remarks>
        /// The .NET type of the numInc parameter will dictate the .NET type
        /// of the original and the new value.
        /// </remarks>
        /// <param name="manipulator">
        /// The <see cref="PropertyManipulator"/>; could be <c>null</c>.
        /// </param>
        /// <param name="numInc">
        /// The object representing the magnitude and sign of the increment.
        /// </param>
        /// <param name="postIncrement">
        /// Pass <b>true</b> to return the value as it was before it was
        /// incremented, or pass <b>false</b> to return the value as it is
        /// after it is incremented.
        /// </param>
        public NumberIncrementor(PropertyManipulator manipulator,
                                 object numInc,
                                 bool postIncrement) : base(manipulator)
        {
            Debug.Assert(numInc != null);
            if (numInc != null && !NumberUtils.IsNumber(numInc))
            {
                throw new ArgumentException("Bad parameter format! " +
                                            numInc.GetType().Name + " is not supported type.");
            }
            m_numInc  = numInc;
            m_postInc = postIncrement;
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
            object numInc = m_numInc;
            if (numInc == null)
            {
                throw new ArgumentNullException("Incorrectly constructed NumberIncrementor.");
            }
            object numOld = Get(entry);
            if (numOld == null)
            {
                if (NumberUtils.IsNumber(numInc))
                {
                    numOld = 0;    
                }
            }

            object numNew;
            if (numOld is Int32)
            {
                numNew = (Int32) numOld + Convert.ToInt32(numInc);
            }
            else if (numOld is Int64)
            {
                numNew = (Int64) numOld + Convert.ToInt64(numInc);
            }
            else if (numOld is Double)
            {
                numNew = (Double) numOld + Convert.ToDouble(numInc);
            }
            else if (numOld is Single)
            {
                numNew = (Single) numOld + Convert.ToSingle(numInc);
            }
            else if (numOld is Decimal)
            {
                numNew = Decimal.Add((Decimal) numOld, Convert.ToDecimal(numInc));
            }
            else if (numOld is Int16)
            {
                numNew = Convert.ToInt16((Int16) numOld + Convert.ToInt16(numInc));
            }
            else if (numOld is Byte)
            {
                numNew = Convert.ToByte((Byte) numOld + Convert.ToByte(numInc));
            }
            else if (numOld is RawInt128)
            {
                Decimal newDec = Decimal.Add(((RawInt128) numOld).ToDecimal(), Convert.ToDecimal(numInc));
                numNew         = NumberUtils.DecimalToRawInt128(newDec);
            }
            else
            {
                throw new Exception("Unsupported type:" +
                                    (numOld == null ?
                                     numInc.GetType().Name :
                                     numOld.GetType().Name));
            }
            Set(entry, numNew);
            return m_postInc ? numOld : numNew;
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
                return (m_postInc ? ", post" : ", pre") + "-increment=" + m_numInc;
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
            m_numInc  = reader.ReadObject(1);
            m_postInc = reader.ReadBoolean(2);
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
            writer.WriteObject(1, m_numInc);
            writer.WriteObject(2, m_postInc);
        }

        #endregion

        #region Data member

        /// <summary>
        /// The number to increment by.
        /// </summary>
        private object m_numInc;

        /// <summary>
        /// Whether to return the value before it was incremented
        /// ("post-increment") or after it is incremented ("pre-increment").
        /// </summary>
        private bool m_postInc;

        #endregion
    }
}