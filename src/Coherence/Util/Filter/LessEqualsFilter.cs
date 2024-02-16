/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Net.Cache;
using Tangosol.Util.Collections;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which compares the result of a member
    /// invocation with a value for "Less or Equals" condition.
    /// </summary>
    /// <remarks>
    /// In a case when either result of a member invocation or a value to
    /// compare are equal to <c>null</c>, the <b>Evaluate</b> test yields
    /// <b>false</b>. This approach is equivalent to the way the <c>null</c>
    /// values are handled by SQL.
    /// </remarks>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.10.29</author>
    /// <author>Goran Milosavljevic  2006.10.23</author>
    /// <author>Tom Beerbower  2009.03.09</author>
    public class LessEqualsFilter : ComparisonFilter, IIndexAwareFilter
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LessEqualsFilter()
        {}

        /// <summary>
        /// Construct a LessEqualsFilter for testing "Less or Equals"
        /// condition.
        /// </summary>
        /// <param name="extractor">
        /// The <see cref="IValueExtractor"/> to use by this filter.
        /// </param>
        /// <param name="value">
        /// The object to compare the result with.
        /// </param>
        public LessEqualsFilter(IValueExtractor extractor, IComparable value)
            : base(extractor, value)
        {}

        /// <summary>
        /// Construct a LessEqualsFilter for testing "Less or Equals"
        /// condition.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="value">
        /// The object to compare the result with.
        /// </param>
        public LessEqualsFilter(string member, IComparable value)
            : base(member, value)
        {}

        /// <summary>
        /// Construct a LessEqualsFilter for testing "Less or Equals"
        /// condition.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="value">
        /// The integer value to compare the result with.
        /// </param>
        public LessEqualsFilter(string member, int value)
            : this(member, (IComparable) value)
        {}

        /// <summary>
        /// Construct a LessEqualsFilter for testing "Less or Equals"
        /// condition.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="value">
        /// The long value to compare the result with.
        /// </param>
        public LessEqualsFilter(string member, long value)
            : this(member, (IComparable) value)
        {}

        /// <summary>
        /// Construct a LessEqualsFilter for testing "Less or Equals"
        /// condition.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="value">
        /// The float value to compare the result with.
        /// </param>
        public LessEqualsFilter(string member, float value)
            : this(member, (IComparable) value)
        {}

        /// <summary>
        /// Construct a LessEqualsFilter for testing "Less or Equals"
        /// condition.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="value">
        /// The double value to compare the result with.
        /// </param>
        public LessEqualsFilter(string member, double value)
            : this(member, (IComparable) value)
        {}

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
            try
            {
                var left  = (IComparable) extracted;
                var right = (IComparable) Value;

                return left != null && right != null && 
                    left.CompareTo(right) <= 0;
            }
            catch (InvalidCastException)
            {
                return false;
            }
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
            return CalculateRangeEffectiveness(indexes, keys);
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
            object value = Value;
            if (value == null)
            {
                // nothing could be compared to null
                CollectionUtils.Clear(keys);
                return null;
            }

            var index = (ICacheIndex)indexes[ValueExtractor];
            if (index == null)
            {
                // there is no relevant index
                return this;
            }

            if (index.IsOrdered)
            {
                var contents = (IDictionary) index.IndexContents;
                var colEQ    = (ICollection) contents[value];
                var colNULL  = (ICollection) contents[null];

                SortedList sortedLT = CollectionUtils.HeadList(contents, value);
                SortedList sortedGE = CollectionUtils.TailList(contents, value);

                bool headHeavy = sortedLT.Count > contents.Count / 2;

                if (headHeavy && !index.IsPartial)
                {
                    foreach (ICollection col in sortedGE.Values)
                    {
                        if (col != colEQ)
                        {
                            CollectionUtils.RemoveAll(keys, col);
                        }
                    }

                    if (colNULL != null)
                    {
                        CollectionUtils.RemoveAll(keys, colNULL);
                    }
                }
                else
                {
                    var setLE = new HashSet();
                    foreach (ICollection col in sortedLT.Values)
                    {
                        if (col != colNULL)
                        {
                            CollectionUtils.AddAll(setLE, col);
                        }
                    }

                    if (colEQ != null)
                    {
                        CollectionUtils.AddAll(setLE, colEQ);
                    }
                    CollectionUtils.RetainAll(keys, setLE);
                }
            }
            else
            {
                IDictionary contents = index.IndexContents;

                if (index.IsPartial)
                {
                    var setLE = new HashSet();
                    foreach (DictionaryEntry entry in contents)
                    {
                        var test = (IComparable) entry.Key;
                        if (test != null && test.CompareTo(value) <= 0)
                        {
                            CollectionUtils.AddAll(setLE, (ICollection) entry.Value);
                        }
                    }
                    CollectionUtils.RetainAll(keys, setLE);
                }
                else
                {
                    foreach (DictionaryEntry entry in contents)
                    {
                        var test = (IComparable)entry.Key;
                        if (test == null || test.CompareTo(value) > 0)
                        {
                            CollectionUtils.RemoveAll(keys, (ICollection) entry.Value);
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}