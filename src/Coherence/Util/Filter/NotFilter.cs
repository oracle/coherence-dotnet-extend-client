/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Util.Collections;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which negates the results of another filter.
    /// </summary>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.10.26</author>
    /// <author>Goran Milosavljevic  2006.10.24</author>
    /// <author>Tom Beerbower  2009.03.09</author>
    public class NotFilter : IIndexAwareFilter, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Obtain the <b>IFilter</b> whose results are negated by this
        /// filter.
        /// </summary>
        /// <value>
        /// The filter whose results are negated by this filter.
        /// </value>
        public virtual IFilter Filter
        {
            get { return m_filter; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NotFilter()
        {}

        /// <summary>
        /// Construct a negation filter.
        /// </summary>
        /// <param name="filter">
        /// The filter whose results this <b>IFilter</b> negates.
        /// </param>
        public NotFilter(IFilter filter)
        {
            m_filter = filter;
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
        public virtual bool Evaluate(object o)
        {
            return !m_filter.Evaluate(o);
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
        public virtual bool EvaluateEntry(ICacheEntry entry)
        {
            return !InvocableCacheHelper.EvaluateEntry(m_filter, entry);
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
            IFilter filter = m_filter;
            return filter is IIndexAwareFilter
                ? ((IIndexAwareFilter)filter).CalculateEffectiveness(indexes, keys)
                : keys.Count * ExtractorFilter.EVAL_COST;
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
            // NOTE: jh 2010.10.06
            // this method differs a bit from the Java version due to a lack
            // of a proper ISet interfacea and SubSet implementation
            IFilter filter = m_filter;
            if (filter is IIndexAwareFilter)
            {
                // create delta set
                var setDelta = new HashSet(keys);

                // delegate to the not-ed filter, but only use the non-partial
                // indexes, since for a partial index the fact that it contains 
                // keys for entries that fit the underlying filter does not 
                // mean that it contains them all. As a result, the "negating" 
                // operation may produce invalid result
                IFilter filterNew = ((IIndexAwareFilter) filter).ApplyIndex(
                    indexes, setDelta);

                // see if any keys were filtered out
                //azzert(setDelta.getAdded().isEmpty());
                var setRemoved = new HashSet();
                foreach (object key in keys)
                {
                    if (!CollectionUtils.Contains(setDelta, key))
                    {
                        setRemoved.Add(key);
                    }
                }

                if (filterNew == null || setDelta.Count == 0)
                {
                    // invert the key selection by the delegated-to filter
                    if (setRemoved.Count == 0)
                    {
                        // no keys were removed; therefore the result of the
                        // "not" is to remove all keys (clear)
                        CollectionUtils.Clear(keys);
                    }
                    else if (setDelta.Count == 0)
                    {
                        // all keys were removed; therefore the result of the
                        // "not" is to retain all keys (remove none)
                    }
                    else
                    {
                        // some keys were removed; therefore the result of the
                        // "not" is to retain only those removed keys
                        CollectionUtils.RetainAll(keys, setRemoved);
                    }

                    // nothing left to do; the index fully resolved the filter
                    return null;
                }
                if (setRemoved.Count == 0)
                {
                    // no obvious effect from the index application
                    return filterNew == filter ? this : new NotFilter(filterNew);
                }
                // some keys have been removed; those are definitely "in";
                // the remaining keys each need to be evaluated later
                var filterKey = new KeyFilter(setRemoved);
                var filterNot = filterNew == filter ? this : new NotFilter(filterNew);
                return new AndFilter(filterKey, filterNot);
            }
            return this;
        }

        #endregion

        #region Helper methods
        
        /// <summary>
        /// Get an IDictionary of the available non-partial indexes from the
        /// given IDictionary of all available indexes.
        /// </summary>
        /// <param name="indexes">The available <see cref="ICacheIndex"/>
        /// objects keyed by the related IValueExtractor; read-only.</param>
        /// <returns>An IDictionary of the available non-partial
        /// <see cref="ICacheIndex"/> objects.</returns>
        protected virtual IDictionary GetNonPartialIndexes(IDictionary indexes)
        {
            IDictionary nonPartialIndexes = new HashDictionary();
            foreach (DictionaryEntry entry in indexes)
            {
                if (!((ICacheIndex) entry.Value).IsPartial)
                {
                    nonPartialIndexes[entry.Key] = entry.Value;
                }
            }
            return nonPartialIndexes;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>NotFilter</b> with another object to determine
        /// equality.
        /// </summary>
        /// <param name="o">
        /// The <b>NotFilter</b> to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if this <b>NotFilter</b> and the passed object are
        /// equivalent <b>NotFilter</b> objects.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is NotFilter)
            {
                var that = o as NotFilter;
                return Equals(m_filter, that.m_filter);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>NotFilter</b> object according
        /// to the general <b>object.GetHashCode()</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>NotFilter</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_filter.GetHashCode();
        }

        /// <summary>
        /// Return a human-readable description for this <b>NotFilter</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>NotFilter</b>.
        /// </returns>
        public override string ToString()
        {
            return "NotFilter: !(" + m_filter + ')';
        }

        #endregion

        #region IPortableObject implementations

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
            m_filter = (IFilter)reader.ReadObject(0);
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
            writer.WriteObject(0, m_filter);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The IFilter whose results are negated by this filter.
        /// </summary>
        private IFilter m_filter;

        #endregion
    }
}