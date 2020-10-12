/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Text;

using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Util.Collections;
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which compares the result of a member
    /// invocation with a value for "Between" condition.
    /// </summary>
    /// <remarks>
    /// We use the standard ISO/IEC 9075:1992 semantic,
    /// according to which "X between Y and Z" is equivalent to
    /// "X &gt;= Y &amp;&amp; X &lt;= Z".
    /// In a case when either result of a member invocation or a value to
    /// compare are equal to <c>null</c>, the <b>Evaluate</b> test yields
    /// <b>false</b>.
    /// This approach is equivalent to the way the <c>null</c> values are
    /// handled by SQL.
    /// </remarks>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.10.29</author>
    /// <author>Goran Milosavljevic  2006.10.20</author>
    /// <author>Jonathan Knight  2014.04.28</author>
    public class BetweenFilter : AndFilter
    {
        #region Properties

        /// <summary>
        /// Obtain the <see cref="IValueExtractor"/> used by this filter.
        /// </summary>
        /// <value>
        /// The <b>IValueExtractor</b> used by this filter.
        /// </value>
        public virtual IValueExtractor ValueExtractor
        {
            get { return ((ComparisonFilter)Filters[0]).ValueExtractor; }
        }

        /// <summary>
        /// Gets the object to representing the lower bound of the range
        /// of values that evaluate to true
        /// </summary>
        /// <value>
        /// The lower bound of the range of values to compare the extraction result with.
        /// </value>
        public virtual IComparable LowerBound
        {
            get { return (IComparable) ((ComparisonFilter)Filters[0]).Value; }
        }

        /// <summary>
        /// Gets the object to representing the upper bound of the range
        /// of values that evaluate to true
        /// </summary>
        /// <value>
        /// The upper bound of the range of values to compare the extraction result with.
        /// </value>
        public virtual IComparable UpperBound
        {
            get { return (IComparable) ((ComparisonFilter)Filters[1]).Value; }
        }

        /// <summary>
        /// Returns true if extracted values equal to the lower bound of the range evaluate to true.
        /// </summary>
        /// <value>
        /// True if extracted values equal to the lower bound of the range evaluate to true.
        /// </value>
        public virtual bool IsLowerBoundInclusive
        {
            get { return Filters[0] is GreaterEqualsFilter; }
        }

        /// <summary>
        /// Returns true if extracted values equal to the upper bound of the range evaluate to true.
        /// </summary>
        /// <value>
        /// True if extracted values equal to the upper bound of the range evaluate to true.
        /// </value>
        public virtual bool IsUpperBoundInclusive
        {
            get { return Filters[1] is LessEqualsFilter; }
        }

        #endregion
        
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BetweenFilter()
        {}

        /// <summary>
        /// Construct a BetweenFilter for testing "Between" condition.
        /// </summary>
        /// <param name="method">
        /// The name of the method to invoke via reflection.
        /// </param>
        /// <param name="from">
        /// The object to compare the "Greater or Equals" boundary
        /// with.
        /// </param>
        /// <param name="to">
        /// The object to compare the "Less or Equals" boundary
        /// with.
        /// </param>
        public BetweenFilter(string method, IComparable from, IComparable to)
            : this(method, from, to, true, true)
        {}

        /// <summary>
        /// Construct a BetweenFilter for testing "Between" condition.
        /// </summary>
        /// <param name="extractor">
        /// The <see cref="IValueExtractor"/> to use by this filter.
        /// </param>
        /// <param name="from">
        /// The object to compare the "Greater or Equals"
        /// boundary with.
        /// </param>
        /// <param name="to">
        /// The object to compare the "Less or Equals" boundary
        /// with.
        /// </param>
        public BetweenFilter(IValueExtractor extractor, IComparable from, IComparable to)
            : this(extractor, from, to, true, true)
        {}

        /// <summary>
        /// Construct a BetweenFilter for testing "Between" condition.
        /// </summary>
        /// <param name="method">
        /// The name of the method to invoke via reflection.
        /// </param>
        /// <param name="from">
        /// The integer value to compare the "Greater or Equals"
        /// boundary with.
        /// </param>
        /// <param name="to">
        /// The integer value to compare the "Less or Equals"
        /// boundary with.
        /// </param>
        public BetweenFilter(string method, int from, int to)
            : this(method, from, to, true, true)
        {}

        /// <summary>
        /// Construct a BetweenFilter for testing "Between" condition.
        /// </summary>
        /// <param name="method">
        /// The name of the method to invoke via reflection.
        /// </param>
        /// <param name="from">
        /// The long value to compare the "Greater or Equals"
        /// boundary with.
        /// </param>
        /// <param name="to">
        /// The long value to compare the "Less or Equals" boundary
        /// with.
        /// </param>
        public BetweenFilter(string method, long from, long to)
            : this(method, from, to, true, true)
        {}

        /// <summary>
        /// Construct a BetweenFilter for testing "Between" condition.
        /// </summary>
        /// <param name="method">
        /// The name of the method to invoke via reflection.
        /// </param>
        /// <param name="from">
        /// The float value to compare the "Greater or Equals"
        /// boundary with.
        /// </param>
        /// <param name="to">
        /// The float value to compare the "Less or Equals"
        /// boundary with.
        /// </param>
        public BetweenFilter(string method, float from, float to)
            : this(method, from, to, true, true)
        {}

        /// <summary>
        /// Construct a BetweenFilter for testing "Between" condition.
        /// </summary>
        /// <param name="method">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="from">
        /// The double value to compare the "Greater or Equals"
        /// boundary with.
        /// </param>
        /// <param name="to">
        /// The double value to compare the "Less or Equals"
        /// boundary with.
        /// </param>
        public BetweenFilter(string method, double from, double to)
            : this(method, from, to, true, true)
        {}

        /// <summary>
        /// Construct a BetweenFilter for testing "Between" condition.
        /// </summary>
        /// <param name="method">The name of the member to invoke via reflection.</param>
        /// <param name="lower">The lower bound of the range of values that evaluate to true</param>
        /// <param name="upper">The upper bound of the range of values that evaluate to true</param>
        /// <param name="includeLower">A flag indicating whether values matching the lower bound 
        /// of the range evaluate to true</param>
        /// <param name="includeUpper">A flag indicating whether values matching the upper bound
        /// of the range evaluate to true</param>
        public BetweenFilter(string method, IComparable lower, IComparable upper, 
                           bool includeLower, bool includeUpper)
            : this(new ReflectionExtractor(method), lower, upper, includeLower, includeUpper)
        {}

        /// <summary>
        /// Construct a BetweenFilter for testing "Between" condition.
        /// </summary>
        /// <param name="extractor">The IValueExtractor to use to extract values from the entry to evaluate</param>
        /// <param name="lower">The lower bound of the range of values that evaluate to true</param>
        /// <param name="upper">The upper bound of the range of values that evaluate to true</param>
        /// <param name="includeLower">A flag indicating whether values matching the lower bound 
        /// of the range evaluate to true</param>
        /// <param name="includeUpper">A flag indicating whether values matching the upper bound 
        /// of the range evaluate to true</param>
        public BetweenFilter(IValueExtractor extractor, IComparable lower, IComparable upper, 
                           bool includeLower, bool includeUpper)
            : base()
        {
            m_filters = new IFilter[2];
            m_filters[0] = (lower != null && includeLower)
                                   ? (IFilter) new GreaterEqualsFilter(extractor, lower)
                                   : new GreaterFilter(extractor, lower);
            m_filters[1] = (upper != null && includeUpper)
                                   ? (IFilter)new LessEqualsFilter(extractor, upper)
                                   : new LessFilter(extractor, upper);
        }

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
        public new virtual bool Evaluate(object o)
        {
            return EvaluateExtracted(Extract(o));
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
        public new virtual bool EvaluateEntry(ICacheEntry entry)
        {
            IValueExtractor extractor = ValueExtractor;
            return EvaluateExtracted(entry is IQueryCacheEntry
                    ? ((IQueryCacheEntry)entry).Extract(extractor)
                    : InvocableCacheHelper.ExtractFromEntry(extractor, entry));
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
        public new int CalculateEffectiveness(IDictionary indexes, ICollection keys)
        {
            var invertedIndex = GetInvertedIndex(indexes);
            if (invertedIndex == null)
            {
                return ExtractorFilter.EVAL_COST * keys.Count;
            }
            
            if (invertedIndex is SortedDictionary)
            {
                var lower  = CollectionUtils.HeadList(invertedIndex, LowerBound).Count;
                var higher = CollectionUtils.TailList(invertedIndex, UpperBound).Count;
                return  higher - lower;
            }

            return invertedIndex.Count;
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
        public new IFilter ApplyIndex(IDictionary indexes, ICollection keys)
        {
            if (LowerBound == null || UpperBound == null)
            {
                CollectionUtils.Clear(keys);
                return null;
            }

            var invertedIndex = GetInvertedIndex(indexes);
            if (invertedIndex == null)
            {
                return this;
            }
            
            if (invertedIndex is SortedDictionary)
            {
                return ApplySortedIndex(keys, invertedIndex);
            }

            ArrayList toRetain = new ArrayList();
            foreach (var indexValue in invertedIndex.Keys)
            {
                if (EvaluateExtracted(indexValue))
                {
                    toRetain.AddRange((ICollection) invertedIndex[indexValue]);
                }
            }

            CollectionUtils.RetainAll(keys, toRetain);

            return null;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Return a human-readable description for this <b>ArrayFilter</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>ArrayFilter</b>.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder(GetType().Name)
                          .Append('(')
                          .Append(ValueExtractor)
                          .Append(IsLowerBoundInclusive ? " >= " : " > ")
                          .Append(LowerBound)
                          .Append(" and ")
                          .Append(ValueExtractor)
                          .Append(IsUpperBoundInclusive ? " <= " : " < ")
                          .Append(UpperBound)
                          .Append(')');

            return sb.ToString();
        }

        #endregion

        #region Helper methods

        bool EvaluateExtracted(object extracted)
        {
            if (extracted == null || LowerBound == null || UpperBound == null)
            {
                return false;
            }

            var lower = LowerBound.CompareTo(extracted);
            if (lower > 0 || (!IsLowerBoundInclusive && lower == 0))
            {
                return false;
            }

            var upper = UpperBound.CompareTo(extracted);
            return upper > 0 || (IsUpperBoundInclusive && upper == 0);
        }

        internal virtual object Extract(object o)
        {
            return ValueExtractor.Extract(o);
        }

        IFilter ApplySortedIndex(ICollection keys, IDictionary dict)
        {
            var syncDict = dict as SynchronizedDictionary;
            if (syncDict != null)
            {
                dict = syncDict.Delegate;
            }

            var toRetain = new ArrayList();
            SortedList list;
            IComparer comparer;

            var sortedDictionary = dict as SortedDictionary;
            if (sortedDictionary != null)
            {
                list = sortedDictionary;
                comparer = sortedDictionary.Comparer;
            }
            else
            {
                throw new NotSupportedException("Dictionary is not sorted: " + dict);
            }

            if (comparer == null)
            {
                comparer = Comparer.Default;
            }

            if (syncDict != null)
            {
                syncDict.AcquireReadLock();
            }

            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var key = list.GetKey(i);

                    if ((comparer.Compare(key, LowerBound) < 0) || (comparer.Compare(key, LowerBound) == 0 && !IsLowerBoundInclusive))
                    {
                        continue;
                    }

                    if ((comparer.Compare(key, UpperBound) == 0 && !IsUpperBoundInclusive) || (comparer.Compare(key, UpperBound) > 0))
                    {
                        break;
                    }

                    CollectionUtils.AddAll(toRetain, (ICollection)list[key]);
                }

                if (toRetain.Count == 0)
                {
                    CollectionUtils.Clear(keys);
                }
                else
                {
                    CollectionUtils.RetainAll(keys, toRetain);
                }
            }
            finally
            {
                if (syncDict != null)
                {
                    syncDict.ReleaseReadLock();
                }
            }

            return null;
        }

        IDictionary GetInvertedIndex(IDictionary mapIndexes)
        {
            ICacheIndex index = (ICacheIndex)mapIndexes[ValueExtractor];
            return index != null ? index.IndexContents : null;
        }

        #endregion
    }
}