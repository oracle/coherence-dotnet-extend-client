/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Util.Collections;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which returns the logical "or" of a filter
    /// array.
    /// </summary>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.11.01</author>
    /// <author>Goran Milosavljevic  2006.10.20</author>
    /// <author>Tom Beerbower  2009.03.06</author>
    public class AnyFilter : ArrayFilter
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AnyFilter()
        {}

        /// <summary>
        /// Construct an "any" filter.
        /// </summary>
        /// <remarks>
        /// The result is defined as:
        /// <code>
        /// filters[0] || filters[1] ... || filters[n]
        /// </code>
        /// </remarks>
        /// <param name="filters">
        /// An array of filters.
        /// </param>
        public AnyFilter(IFilter[] filters) : base(filters)
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
            for (int i = 0, c = filters.Length; i < c; i++)
            {
                if (filters[i].Evaluate(o))
                {
                    return true;
                }
            }
            return false;
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
            for (int i = 0, c = filters.Length; i < c; i++)
            {
                if (InvocableCacheHelper.EvaluateEntry(filters[i], entry))
                {
                    return true;
                }
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
        public override int CalculateEffectiveness(IDictionary indexes, ICollection keys)
        {
            OptimizeFilterOrder(indexes, keys);

            IFilter[] filters     = m_filters;
            int       filterCount = filters.Length;

            if (filterCount > 0)
            {
                IFilter filterN = filters[filters.Length - 1];

                return filterN is IIndexAwareFilter
                    ? ((IIndexAwareFilter) filterN)
                        .CalculateEffectiveness(indexes, keys)
                    : keys.Count * ExtractorFilter.EVAL_COST;
            }

            return 1;
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
            OptimizeFilterOrder(indexes, keys);

            IFilter[] filters     = m_filters;
            int       filterCount = filters.Length;
            var       filterList  = new ArrayList(filterCount);
            var       matches     = new HashSet(keys.Count);

            // filterList is an array of filters that will have to be re-applied
            // matches    is an accumulating set of already matching keys

            for (int i = 0; i < filterCount; i++)
            {
                IFilter filter = filters[i];
                if (filter is IIndexAwareFilter)
                {
                    ICollection remain = new ArrayList(keys);
                    if (matches.Count > 0)
                    {
                        CollectionUtils.RemoveAll(remain, matches);
                    }

                    IFilter filterDefer = ApplyFilter(
                        (IIndexAwareFilter) filter, indexes, remain);

                    if (filterDefer == null)
                    {
                        // these are definitely "in"
                        CollectionUtils.AddAll(matches, remain);
                    }
                    else
                    {
                        int keyCount  = keys.Count;
                        int remaining = remain.Count;
                        if (remaining < keyCount)
                        {
                            // some keys are definitely "out" for this filter;
                            // we need to incorporate this knowledge into a deferred
                            // filter
                            if (remaining > 0)
                            {
                                var filterKey = new KeyFilter(remain);
                                filterList.Add(new AndFilter(filterDefer, filterKey));
                            }
                            //else
                            {
                                // though a filter was returned, the key set was
                                // fully reduced; this should have the same effect
                                // as a fully resolved filter without any matches
                            }
                        }
                        else
                        {
                            filterList.Add(filterDefer);
                        }
                    }
                }
                else
                {
                    filterList.Add(filter);
                }
            }
            int matchCount = matches.Count;

            filterCount = filterList.Count;
            if (filterCount == 0)
            {
                if (matchCount > 0)
                {
                    CollectionUtils.RetainAll(keys, matches);
                }
                else
                {
                    CollectionUtils.Clear(keys);
                }
                return null;
            }
            if (filterCount == 1 && matchCount == 0)
            {
                return (IFilter) filterList[0];
            }
            if (matchCount > 0)
            {
                // the keys that have been matched are definitely "in";
                // the remaining keys each need to be evaluated later
                var filterKey = new KeyFilter(matches);
                filterList.Insert(0, filterKey);
            }
            return new AnyFilter((IFilter[]) filterList.ToArray(typeof(IFilter)));
        }

        #endregion
    }
}