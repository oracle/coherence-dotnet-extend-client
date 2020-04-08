/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Net.Cache;
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    /// Return the <b>ICollection</b> of unique values extracted from a
    /// collection of entries in a cache.
    /// </summary>
    /// <remarks>
    /// <p>
    /// If the <b>ICollection</b> of entries is empty, an empty collection
    /// is returned.</p>
    /// <p>
    /// This aggregator could be used in combination with
    /// <see cref="MultiExtractor"/> allowing to collect all unique
    /// combinations (tuples) of a given set of attributes.</p>
    /// </remarks>
    /// <p>The <b>DistinctValues</b> aggregator covers a simple case of a
    /// more generic aggregation pattern implemented by the
    /// <see cref="GroupAggregator"/>, which in addition to collecting all
    /// distinct values or tuples, runs an aggregation against each distinct
    /// entry set (group).</p>
    /// <author>Jason Howes  2005.12.20</author>
    /// <author>Ivan Cikic  2006.10.24</author>
    public class DistinctValues : AbstractAggregator
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DistinctValues()
        {}

        /// <summary>
        /// Construct a <b>DistinctValues</b> aggregator.
        /// </summary>
        /// <param name="extractor">
        /// The extractor that provides a value in the form of any .NET
        /// object.
        /// </param>
        public DistinctValues(IValueExtractor extractor) : base(extractor)
        {}

        /// <summary>
        /// Construct a <b>DistinctValues</b> aggregator.
        /// </summary>
        /// <param name="member">
        /// The name of the member that returns a value in the form of any
        /// .NET object.
        /// </param>
        public DistinctValues(string member) : base(member)
        {}

        #endregion

        #region Helper methods

        /// <summary>
        /// Return a collection that can be used to store distinct values,
        /// creating it if one has not already been created.
        /// </summary>
        /// <returns>
        /// A collection that can be used to store distinct values.
        /// </returns>
        protected virtual ICollection EnsureCollection()
        {
            ICollection coll = m_coll;
            if (coll == null)
            {
                coll = m_coll = new ArrayList();
            }
            return coll;
        }

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
            ICollection coll = m_coll;
            if (coll != null)
            {
                CollectionUtils.Clear(coll);
            }
        }

        /// <summary>
        /// Incorporate one aggregatable value into the result.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="isFinal"/> parameter is <b>true</b>, the
        /// given object is a partial result (returned by an individual
        /// parallel aggregator) that should be incorporated into the final
        /// result; otherwise, the object is a value extracted from an
        /// <see cref="IInvocableCacheEntry"/>.
        /// </remarks>
        /// <param name="o">
        /// The value to incorporate into the aggregated result.
        /// </param>
        /// <param name="isFinal">
        /// <b>true</b> to indicate that the given object is a partial
        /// result returned by a parallel aggregator.
        /// </param>
        protected override void Process(object o, bool isFinal)
        {
            if (o != null)
            {
                if (isFinal)
                {
                    // aggregate partial results
                    ICollection coll = (ICollection) o;
                    if (coll.Count != 0)
                    {
                        CollectionUtils.AddAll(EnsureCollection(), coll);
                    }
                }
                else
                {
                    // collect partial results
                    CollectionUtils.Add(EnsureCollection(), o, true);
                }
            }
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
            ICollection coll = m_coll;
            m_coll = null; // COHNET-181

            if (isFinal)
            {
                // return the final aggregated result
                return coll == null ? NullImplementation.GetCollection() : coll;
            }
            else
            {
                // return partial aggregation data
                return coll;
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The resulting collection of distinct values.
        /// </summary>
        [NonSerialized]
        protected ICollection m_coll;

        #endregion
    }
}