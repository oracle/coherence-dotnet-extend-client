/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which returns the logical exclusive or ("xor")
    /// of two other filters.
    /// </summary>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.10.27</author>
    /// <author>Goran Milosavljevic  2006.10.24</author>
    /// <author>Tom Beerbower 2009.03.08</author>
    public class XorFilter : ArrayFilter
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public XorFilter()
        {}

        /// <summary>
        /// Construct a "xor" filter.
        /// </summary>
        /// <remarks>
        /// The result is defined as:
        /// <code>
        /// filterLeft ^ filterRight
        /// </code>
        /// </remarks>
        /// <param name="filterLeft">
        /// The "left" filter.
        /// </param>
        /// <param name="filterRight">
        /// The "right" filter.
        /// </param>
        public XorFilter(IFilter filterLeft, IFilter filterRight)
            : base(new IFilter[]{filterLeft, filterRight})
        {}

        #endregion

        #region IFilter implementation

        /// <summary>
        /// Apply the test to the object.
        /// </summary>
        /// <param name="o">
        /// An object to which the test is applied.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        public override bool Evaluate(object o)
        {
            IFilter[] filters = m_filters;
            return filters[0].Evaluate(o) ^ filters[1].Evaluate(o);
        }

        #endregion

        #region IEntryFilter implementation

        /// <summary>
        /// Apply the test to an <see cref="ICacheEntry"/>.
        /// </summary>
        /// <param name="entry">
        /// The <b>ICacheEntry</b> to evaluate; never <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        public override bool EvaluateEntry(ICacheEntry entry)
        {
            IFilter[] filters = m_filters;
            return InvocableCacheHelper.EvaluateEntry(filters[0], entry) 
                 ^ InvocableCacheHelper.EvaluateEntry(filters[1], entry);
        }

        #endregion

        #region IIndexAwareFilter implementation

        /// <summary>
        /// Given an IDictionary of available indexes, determine if this 
        /// IIndexAwareFilter can use any of the indexes to assist in its 
        /// processing, and if so, determine how effective the use of that 
        /// index would be.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The returned value is an effectiveness estimate of how well this 
        /// filter can use the specified indexes to filter the specified 
        /// keys. An operation that requires no more than a single access to 
        /// the index content (i.e. Equals, NotEquals) has an effectiveness of 
        /// <b>one</b>. Evaluation of a single entry is assumed to have an 
        /// effectiveness that depends on the index implementation and is 
        /// usually measured as a constant number of the single operations.  
        /// This number is referred to as <i>evaluation cost</i>.
        /// </p>
        /// <p>
        /// If the effectiveness of a filter evaluates to a number larger 
        /// than the keys.size() then a user could avoid using the index and 
        /// iterate through the keys calling <tt>Evaluate</tt> rather than 
        /// <tt>ApplyIndex</tt>.
        /// </p>
        /// </remarks>
        /// <param name="indexes">
        /// The available <see cref="ICacheIndex"/> objects keyed by the 
        /// related IValueExtractor; read-only.
        /// </param>
        /// <param name="keys">
        /// The set of keys that will be filtered; read-only.
        /// </param>
        /// <returns>
        /// An effectiveness estimate of how well this filter can use the 
        /// specified indexes to filter the specified keys.
        /// </returns>
        public override int CalculateEffectiveness(IDictionary indexes, ICollection keys)
        {
            return System.Int32.MaxValue;
        }

        /// <summary>
        /// Filter remaining keys using an IDictionary of available indexes.
        /// </summary>
        /// <remarks>
        /// The filter is responsible for removing all keys from the passed 
        /// set of keys that the applicable indexes can prove should be 
        /// filtered. If the filter does not fully evaluate the remaining 
        /// keys using just the index information, it must return a filter
        /// (which may be an <see cref="IEntryFilter"/>) that can complete the 
        /// task using an iterating implementation. If, on the other hand, the
        /// filter does fully evaluate the remaining keys using just the index
        /// information, then it should return <c>null</c> to indicate that no 
        /// further filtering is necessary.
        /// </remarks>
        /// <param name="indexes">
        /// The available <see cref="ICacheIndex"/> objects keyed by the 
        /// related IValueExtractor; read-only.
        /// </param>
        /// <param name="keys">
        /// The mutable set of keys that remain to be filtered.
        /// </param>
        /// <returns>
        /// An <see cref="IFilter"/> object that can be used to process the 
        /// remaining keys, or <c>null</c> if no additional filter processing 
        /// is necessary.
        /// </returns>
        public override IFilter ApplyIndex(IDictionary indexes, ICollection keys)
        {
            // no use for indexes
            return this;
        }

        #endregion
    }
}