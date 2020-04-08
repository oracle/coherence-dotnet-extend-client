/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which is a logical operator of a filter array.
    /// </summary>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.11.01</author>
    /// <author>Goran Milosavljevic  2006.10.20</author>
    /// <author>Tom Beerbower  2009.03.06</author>
    public abstract class ArrayFilter : IIndexAwareFilter, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Obtain the <see cref="IFilter"/> array.
        /// </summary>
        /// <value>
        /// The <b>IFilter</b> array.
        /// </value>
        public virtual IFilter[] Filters
        {
            get { return m_filters; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected ArrayFilter()
        {}

        /// <summary>
        /// Construct a logical filter that applies a binary operator to a
        /// filter array.
        /// </summary>
        /// <remarks>
        /// The result is defined as:
        /// <code>
        /// filters[0] &lt;op&gt; filters[1] ... &lt;op&gt; filters[n]
        /// </code>
        /// </remarks>
        /// <param name="filters">
        /// The filter array.
        /// </param>
        protected ArrayFilter(IFilter[] filters)
        {
            Debug.Assert(filters != null);
            for (int i = 0, c = filters.Length; i < c; i++)
            {
                Debug.Assert(filters[i] != null, "Null element");
            }
            m_filters = filters;
        }

        #endregion

        #region IIndexAwareFilter

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
        public abstract int CalculateEffectiveness(IDictionary indexes, ICollection keys);

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
        public abstract IFilter ApplyIndex(IDictionary indexes, ICollection keys);

        #endregion

        #region Abstract methods

        /// <summary>
        /// Apply the test to the object.
        /// </summary>
        /// <param name="o">
        /// An object to which the test is applied.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        public abstract bool Evaluate(object o);

        /// <summary>
        /// Apply the test to an <see cref="ICacheEntry"/>.
        /// </summary>
        /// <param name="entry">
        /// The <b>ICacheEntry</b> to evaluate; never <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        public abstract bool EvaluateEntry(ICacheEntry entry);

        #endregion

        #region ArrayFilter methods

        /// <summary>
        /// Ensure that the order of underlying filters is preserved by the
        /// <see cref="ApplyIndex(IDictionary, ICollection)"/> and
        /// <see cref="EvaluateEntry(ICacheEntry)"/> implementations.
        /// </summary>
        /// <since>Coherence 12.2.1</since>
        public void HonorOrder()
        {
            m_preserveOrder = true;
        }

        #endregion

        #region Internal helpers

        /// <summary>
        /// Apply the specified IndexAwareFilter to the specified keySet.
        /// </summary>
        /// <param name="filter">
        /// The IndexAwareFilter to apply an index to.
        /// </param>
        /// <param name="indexes">
        /// The available MapIndex objects keyed by the related 
        /// IValueExtractor; read-only.
        /// </param>
        /// <param name="keys">
        /// The mutable set of keys that remain to be filtered.
        /// </param>
        /// <returns>
        /// A Filter object that can be used to process the remaining
        /// keys, or null if no additional filter processing is necessary
        ///</returns>
        protected virtual IFilter ApplyFilter(IIndexAwareFilter filter, 
            IDictionary indexes, ICollection keys)
        {
            return filter.ApplyIndex(indexes, keys);
        }

        /// <summary>
        /// Sort all the participating filters according to their effectiveness.
        /// </summary>
        /// <param name="indexes">
        /// The available ICacheIndex objects keyed by the related 
        /// <b>IValueExtractor</b>; read-only.
        /// </param>
        /// <param name="keys">
        /// The set of keys that will be filtered; read-only
        /// </param>
        /// <since>Coherence 12.2.1</since>
        protected void OptimizeFilterOrder(IDictionary indexes,
            ICollection keys)
        {
            if (m_preserveOrder)
            {
                return;
            }

            IFilter[]        filters     = m_filters;
            int              filterCount = filters.Length;
            WeightedFilter[] wf          = new WeightedFilter[filterCount];
            int              max         = keys.Count * ExtractorFilter.EVAL_COST;
            bool             sort        = false;
            int              effect0     = -1;

            for (int i = 0; i < filterCount; i++)
            {
                IFilter filter = filters[i];
                int     effect = filter is IIndexAwareFilter
                    ? ((IIndexAwareFilter) filter)
                        .CalculateEffectiveness(indexes, keys)
                    : max;

                wf[i] = new WeightedFilter(filter, effect);

                if (i == 0)
                {
                    effect0 = effect;
                }
                else
                {
                    // only need to sort if the weights are different
                    sort |= (effect != effect0);
                }
            }

            if (sort)
            {
                Array.Sort(wf);
                for (int i = 0; i < filterCount; i++)
                {
                    filters[i] = wf[i].Filter;
                }
            }
            m_preserveOrder = true;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>ArrayFilter</b> with another object to determine
        /// equality.
        /// </summary>
        /// <remarks>
        /// Two <b>ArrayFilter</b> objects are considered equal if they are
        /// of same type and their filter arrays are equal.
        /// </remarks>
        /// <param name="o">
        /// The <b>ArrayFilter</b> to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if this <b>ArrayFilter</b> and the passed object are
        /// equivalent <b>ArrayFilter</b> objects.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is ArrayFilter)
            {
                var that = (ArrayFilter) o;
                return GetType() == that.GetType()
                       && CollectionUtils.EqualsDeep(m_filters, that.m_filters);
            }

            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>ArrayFilter</b> object
        /// according to the general <b>object.GetHashCode()</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>ArrayFilter</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            int       hash    = 0;
            IFilter[] filters = m_filters;
            for (int i = 0, c = filters.Length; i < c; i++)
            {
                IFilter filter = filters[i];
                hash += filter == null ? 0 : filter.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Return a human-readable description for this <b>ArrayFilter</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>ArrayFilter</b>.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder(GetType().Name);
            sb.Append('(');

            IFilter[] filters = m_filters;
            for (int i = 0, c = filters.Length; i < c; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(filters[i]);
            }

            sb.Append(')');

            return sb.ToString();
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void ReadExternal(IPofReader reader)
        {
            m_filters = (IFilter[]) reader.ReadArray(0, EMPTY_FILTER_ARRAY);

            // if we read an old version of the filter that didn't have this field,
            // it would result in maintaining the old behavior
            m_preserveOrder = reader.ReadBoolean(1);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteExternal(IPofWriter writer)
        {
            writer.WriteArray(0, m_filters);
            writer.WriteBoolean(1, m_preserveOrder);
        }

        #endregion

        #region Inner class: WeightedFilter

        /// <summary>
        /// A thin wrapper around a Filter allowing for sorting the filters
        /// according to their effectiveness.        
        /// </summary>
        public class WeightedFilter : IComparable
        {
            #region Constructors

            ///<summary>
            /// Construct the WeightedFilter.
            ///</summary>
            ///<param name="filter">
            ///The wrapped filter.
            ///</param>
            ///<param name="effect">
            ///The filter's effectiveness.
            ///</param>
            public WeightedFilter(IFilter filter, int effect)
            {
                m_filter = filter;
                m_effect = effect;
            }

            #endregion

            #region Comparable interface

            /// <summary>
            /// Compares this WeightedFilter with the specified WeightedFilter
            /// for order.  Returns a negative integer, zero, or a positive 
            /// integer as this WeightedFilter's effectiveness is less than, 
            /// equal to, or greater than the effectiveness of the specified 
            /// WeightedFilter object.
            /// </summary>
            /// <param name="obj">
            /// The Object to be compared
            /// </param>
            /// <returns>
            /// A 32-bit signed integer that indicates the relative order of 
            /// the objects being compared. The return value has these meanings: 
            /// Value Meaning Less than zero This instance is less than obj. 
            /// Zero This instance is equal to obj. Greater than zero This 
            /// instance is greater than obj.
            /// </returns>
            /// <exception>
            /// System.ArgumentException: obj is not the same type as this 
            /// instance.
            /// </exception>
            public virtual int CompareTo(object obj)
            {
                int thisEffect = m_effect;
                int thatEffect = ((WeightedFilter)obj).m_effect;

                return (thisEffect < thatEffect 
                    ? -1 : (thisEffect > thatEffect ? +1 : 0));
            }
            #endregion

            #region Properties

            ///<summary>
            /// Get the wrapped filter.
            ///</summary>
            ///<returns>
            ///The wrapped filter
            ///</returns>
            public virtual IFilter Filter
            {
                get { return m_filter; }
            }

            #endregion

            #region Data members

            /// <summary>
            /// The wrapped filter.
            /// </summary>
            protected internal IFilter m_filter;

            /// <summary>
            /// The effectiveness of the wrapped filter.
            /// </summary>
            protected internal int m_effect;

            #endregion
        }

        #endregion

        #region Constants

        /// <summary>
        /// A zero-length array of IFilter objects.
        /// </summary>
        private static readonly IFilter[] EMPTY_FILTER_ARRAY = new IFilter[0];

        #endregion

        #region Data members

        /// <summary>
        /// The IFilter array.
        /// </summary>
        protected internal IFilter[] m_filters;

        /// <summary>
        /// Flag indicating whether or not the filter order should be preserved.
        /// </summary>
        /// <since>Coherence 12.2.1</since>
        protected internal bool m_preserveOrder;

        #endregion
    }
}