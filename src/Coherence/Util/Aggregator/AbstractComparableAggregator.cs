/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    /// Abstract aggregator that processes values extracted from a set of 
    /// entries in a cache, with knowledge of how to compare those values. 
    /// </summary>
    /// <remarks>
    /// There are two way to use the AbstractComparableAggregator:
    /// <ul>
    /// <li>All the extracted objects must implement <b>IComparable</b>, or</li>
    /// <li>The AbstractComparableAggregator has to be provided with an 
    /// <b>IComparer</b> object.</li>
    /// </ul>
    /// If the set of entries passed to <b>Aggregate</b> is empty, a
    /// <tt>null</tt> result is returned.
    /// </remarks>
    /// <author>Gene Gleyzer  2006.02.13</author>
    /// <author>Ana Cikic  2006.10.23</author>
    /// <author>Patrick Peralta  2009.01.22</author>
    /// <since>Coherence 3.2</since>
    public abstract class AbstractComparableAggregator : AbstractAggregator
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AbstractComparableAggregator()
        {}

        /// <summary>
        /// Construct an AbstractComparableAggregator object.
        /// </summary>
        /// <param name="extractor">
        /// The <see cref="IValueExtractor"/> that provides a value in the
        /// form of any object that implements the <b>IComparable</b>
        /// interface.
        /// </param>
        public AbstractComparableAggregator(IValueExtractor extractor) : base(extractor)
        {}

        /// <summary>
        /// Construct an AbstractComparableAggregator object.
        /// </summary>
        /// <param name="extractor">
        /// The extractor that provides an object to be compared.
        /// </param>
        /// <param name="comparer">
        /// The comparer used to compare the extracted object.
        /// </param>
        public AbstractComparableAggregator(IValueExtractor extractor, IComparer comparer)
            : base(extractor)
        {
            m_comparer = comparer;
        }

        /// <summary>
        /// Construct an AbstractComparableAggregator object.
        /// </summary>
        /// <param name="member">
        /// The name of the member that returns a value in the form of any
        /// object that implements the <b>IComparable</b> interface.
        /// </param>
        public AbstractComparableAggregator(string member)
            : base(member)
        { }

        #endregion

        #region AbstractAggregator override methods

        /// <summary>
        /// Initialize the aggregation result.
        /// </summary>
        /// <param name="isFinal">
        /// <b>true</b> is passed if the aggregation process that is being
        /// initialized must produce a final aggregation result; this will
        /// only be <b>false</b> if a parallel approach is being used and the
        /// initial (partial) aggregation process is being initialized.
        /// </param>
        protected override void Init(bool isFinal)
        {
            m_count  = 0;
            m_result = null;
        }

        /// <summary>
        /// Obtain the result of the aggregation.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="isFinal"/> parameter is <b>true</b>, the
        /// returned object must be the final result of the aggregation;
        /// otherwise, the returned object will be treated as a partial
        /// result that should be incorporated into the final result.
        /// </remarks>
        /// <param name="isFinal">
        /// <b>true</b> to indicate that the final result of the aggregation
        /// process should be returned; this will only be <b>false</b> if a
        /// parallel approach is being used.
        /// </param>
        /// <returns>
        /// The result of the aggregation process.
        /// </returns>
        protected override object FinalizeResult(bool isFinal)
        {
            return m_count == 0 ? null : m_result;
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
            m_comparer = (IComparer) reader.ReadObject(2);
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
            writer.WriteObject(2, m_comparer);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The count of processed entries.
        /// </summary>
        [NonSerialized]
        protected int m_count;

        /// <summary>
        /// The running result value.
        /// </summary>
        [NonSerialized]
        protected object m_result;

        /// <summary>
        /// The comparer to use for comparing extracted values.
        /// </summary>
        protected IComparer m_comparer;

        #endregion
    }
}