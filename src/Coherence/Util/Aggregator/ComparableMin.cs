/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Net.Cache;
using Tangosol.Util.Comparator;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    /// Calculates a minimum among values extracted from a set of entries 
    /// in a cache.
    /// </summary>
    /// <remarks>
    /// This aggregator is most commonly used with objects that implement
    /// <b>IComparable</b> such as <b>String</b> or <b>DateTime</b>.
    /// An <b>IComparer</b> can also be supplied to perform the comparisons.
    /// </remarks>
    /// <author>Gene Gleyzer  2006.02.13</author>
    /// <author>Ivan Cikic  2006.10.24</author>
    /// <author>Patrick Peralta  2009.01.22</author>
    /// <since>Coherence 3.2</since>
    public class ComparableMin : AbstractComparableAggregator
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ComparableMin()
        {}

        /// <summary>
        /// Construct a <b>ComparableMin</b> aggregator.
        /// </summary>
        /// <param name="extractor">
        /// The extractor that provides a value in the form of any object
        /// that implements <see cref="IComparable"/> interface.
        /// </param>
        public ComparableMin(IValueExtractor extractor) : base(extractor)
        {}

        /// <summary>
        /// Construct a <b>ComparableMin</b> aggregator.
        /// </summary>
        /// <param name="extractor">
        /// The extractor that provides an object to be compared.
        /// </param>
        /// <param name="comparer">
        /// The comparer used to compare the extracted object.
        /// </param>
        public ComparableMin(IValueExtractor extractor, IComparer comparer)
            : base(extractor, comparer)
        {}

        /// <summary>
        /// Construct a <b>ComparableMin</b> aggregator.
        /// </summary>
        /// <param name="member">
        /// The name of the member that returns a value in the form of any
        /// object that implements <see cref="IComparable"/> interface.
        /// </param>
        public ComparableMin(string member) : base(member)
        {}

        #endregion

        #region AbstractAgregator override methods

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
                Object result = m_result;
                if (result == null ||
                    SafeComparer.CompareSafe(m_comparer, result, o) > 0)
                {
                    m_result = o;
                }
                m_count++;
            }
        }

        #endregion
    }
}