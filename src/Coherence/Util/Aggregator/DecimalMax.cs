/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.Net.Cache;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    /// Calculates a maximum of numeric values extracted from a set of
    /// entries in a <b>IDictionary</b> in a form of a <see cref="Decimal"/>
    /// value.
    /// </summary>
    /// <remarks>
    /// All the extracted objects will be treated as <b>Decimal</b> values.
    /// If the set of entries is empty, a <c>null</c> result is returned.
    /// </remarks>
    /// <author>Gene Gleyzer  2006.07.18</author>
    /// <author>Goran Milosavljevic  2008.01.30</author>
    public class DecimalMax : AbstractDecimalAggregator
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DecimalMax()
        {}

        /// <summary>
        /// Construct a <b>DecimalMax</b> aggregator.
        /// </summary>
        /// <param name="extractor">
        /// The extractor that provides a value in the form of any .NET
        /// object that is <b>Decimal</b>.
        /// </param>
        public DecimalMax(IValueExtractor extractor) : base(extractor)
        {}

        /// <summary>
        /// Construct a <b>DecimalMax</b> aggregator.
        /// </summary>
        /// <param name="member">
        /// The name of the member that returns a value in the form of any
        /// .NET object that is <b>Decimal</b>.
        /// </param>
        public DecimalMax(string member) : base(member)
        {}

        #endregion

        #region AbstractAggregator override methods

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
                Decimal dec    = EnsureDecimal(o);
                Decimal result = m_result;

                m_result = Math.Max(result, dec);
                m_count++;
            }
        }

        #endregion
    }
}