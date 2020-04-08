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
    /// Abstract aggregator that processes <see cref="IComparable"/> values
    /// extracted from a set of entries in a <b>IDictionary</b> and returns
    /// a result in a form of a <see cref="Decimal"/> value.
    /// </summary>
    /// <remarks>
    /// All the extracted objects will be treated as <b>Decimal</b> values.
    /// If the set of entries is empty, a <c>null</c> result is returned.
    /// </remarks>
    /// <author>Gene Gleyzer  2006.02.13</author>
    /// <author>Goran Milosavljevic  2008.01.30</author>
    public abstract class AbstractDecimalAggregator : AbstractAggregator
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AbstractDecimalAggregator()
        {}

        /// <summary>
        /// Construct a <b>AbstractDecimalAggregator</b> aggregator.
        /// </summary>
        /// <param name="extractor">
        /// The extractor that provides a value in the form of any .NET
        /// object that is <b>Decimal</b>.
        /// </param>
        public AbstractDecimalAggregator(IValueExtractor extractor) : base(extractor)
        {}

        /// <summary>
        /// Construct a <b>AbstractDecimalAggregator</b> aggregator.
        /// </summary>
        /// <param name="member">
        /// The name of the member that returns a value in the form of any
        /// .NET object that is <b>Decimal</b>.
        /// </param>
        public AbstractDecimalAggregator(string member) : base(member)
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

        #region Helper methods

        /// <summary>
        /// Ensure the specified object is a <b>Decimal</b> value or convert
        /// it into a new Decimal object.
        /// </summary>
        /// <param name="value">
        /// Object that should be ensured of Decimal.
        /// </param>
        /// <returns>
        /// Decimal value.
        /// </returns>
        public static Decimal EnsureDecimal(object value)
        {
            if (value == null)
            {
                throw new NotSupportedException("decimal value cannot be null");
            }

            return value is Decimal ? (Decimal) value : new Decimal(Convert.ToDouble(value));
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
        protected Decimal m_result;

        #endregion
    }
}