/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.Diagnostics;

using Tangosol.Net.Cache;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which tests an <b>ICollection</b> or object
    /// array value returned from a member invocation for containment of all
    /// values in an collection.
    /// </summary>
    /// <author>Jason Howes  2005.06.08</author>
    /// <author>Goran Milosavljevic  2006.10.23</author>
    /// <author>Tom Beerbower  2009.03.09</author>
    public class ContainsAllFilter : ComparisonFilter, IIndexAwareFilter
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ContainsAllFilter()
        {}

        /// <summary>
        /// Construct an <b>ContainsAllFilter</b> for testing containment of
        /// the given collection of values.
        /// </summary>
        /// <param name="extractor">
        /// The <see cref="IValueExtractor"/> used by this filter.
        /// </param>
        /// <param name="values">
        /// The ICollection of values that a collection or object array is
        /// tested to contain.
        /// </param>
        public ContainsAllFilter(IValueExtractor extractor, ICollection values)
            : base(extractor, new ImmutableArrayList(CollectionUtils.ToArray(values)))
        {
            Debug.Assert(values != null);
        }

        /// <summary>
        /// Construct an <b>ContainsAllFilter</b> for testing containment of
        /// the given collection of values.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="values">
        /// The ICollection of values that a collection or object array is
        /// tested to contain.
        /// </param>
        public ContainsAllFilter(string member, ICollection values)
            : base(member, new ImmutableArrayList(CollectionUtils.ToArray(values)))
        {
            Debug.Assert(values != null);
        }

        #endregion

        #region ExtractorFilter override methods

        /// <summary>
        /// Evaluate the specified extracted value.
        /// </summary>
        /// <param name="extracted">
        /// An extracted value to evaluate.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        protected internal override bool EvaluateExtracted(object extracted)
        {
            if (extracted is ICollection)
            {
                return CollectionUtils.ContainsAll(((ICollection) extracted), 
                    (ICollection) Value);
            }

            return false;
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
        public int CalculateEffectiveness(IDictionary indexes, ICollection keys)
        {
            var index = (ICacheIndex) indexes[ValueExtractor];
            return index == null ? CalculateIteratorEffectiveness(keys.Count)
                                 : ((ICollection) Value).Count;
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
        public IFilter ApplyIndex(IDictionary indexes, ICollection keys)
        {
            var index = (ICacheIndex) indexes[ValueExtractor];
            if (index == null)
            {
                // there is no relevant index
                return this;
            }

            foreach (object oValue in (ICollection) Value)
            {
                var colEQ = (ICollection) index.IndexContents[oValue];
                if (colEQ == null)
                {
                    CollectionUtils.Clear(keys);
                    break;
                }
                CollectionUtils.RetainAll(keys, colEQ);
                if (keys.Count == 0)
                {
                    break;
                }
            }
            return null;
        }
        #endregion
    }
}