/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    /// Abstract aggregator that processes numeric values extracted from a
    /// collection of entries in a cache.
    ///  </summary>
    /// <remarks>
    /// All the extracted objects (Byte, Int16, Int32, Int64, Single, Double)
    /// will be treated as <b>Int64</b> values. If the collection of
    /// entries is empty, a <c>null</c> result is returned.
    /// </remarks>
    /// <author>Cameron Purdy, Gene Gleyzer, Jason Howes  2005.07.19</author>
    /// <author>Ivan Cikic  2005.10.24</author>
    /// <since>Coherence 3.1</since>
    public abstract class AbstractLongAggregator : AbstractAggregator
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AbstractLongAggregator()
        {}

        /// <summary>
        /// Construct a <b>AbstractLongAggregator</b> aggregator.
        /// </summary>
        /// <param name="extractor">
        /// The extractor that provides a value in the form of any .NET
        /// object out of Byte, Int16, Int32, Int64, Single, Double.
        /// </param>
        public AbstractLongAggregator(IValueExtractor extractor)
                : base(extractor)
        {}

        /// <summary>
        /// Construct a <b>AbstractLongAggregator</b> aggregator.
        /// </summary>
        /// <param name="member">
        /// The name of the member that returns a value in the form of any
        /// .NET object out of Byte, Int16, Int32, Int64, Single, Double.
        /// </param>
        public AbstractLongAggregator(string member)
                : base(member)
        {}

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
            m_count = 0;
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
            return m_count == 0 ? (object) null : m_result;
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
        protected long m_result;

        #endregion
    }
}