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
    /// EntryFilter which checks whether an entry key belongs to a set.
    /// </summary>
    /// <remarks>
    /// <b>Note: this filter is not serializable and intended to be used only
    /// internally by other composite filters to reduce the remaining key set.</b>
    /// As of Coherence 3.2, an equivalent functionality could be achieved using the
    /// InFilter as follows:
    /// <pre>
    ///   new InFilter(new KeyExtractor(IdentityExtractor.INSTANCE), setKeys);
    /// </pre>
    /// </remarks>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.11.01</author>
    /// <author>Tom Beerbower  2009.03.09</author>
    public class KeyFilter : IIndexAwareFilter
    {
        #region Constructors

        /// <summary>
        /// Construct a key  filter.
        /// </summary>
        /// <param name="keys">
        /// The keys that this filter will evaluate to true.
        /// </param>
        public KeyFilter(ICollection keys)
        {
            CollectionUtils.AddAll(m_keys, keys);
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
            throw new NotSupportedException();
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
        public bool EvaluateEntry(ICacheEntry entry)
        {
            return CollectionUtils.Contains(m_keys, entry.Key);
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
        public IFilter ApplyIndex(IDictionary indexes, ICollection keys)
        {
            CollectionUtils.RetainAll(keys, m_keys);
            return null;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Return a human-readable description for this
        /// <b>KeyAssociatedFilter</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>KeyFilter</b>.
        /// </returns>
        public override string ToString()
        {
            return "Key in " + m_keys;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The keys that are evaluated to true by this filter.
        /// </summary>
        private readonly ICollection m_keys = new HashSet();

        #endregion
    }
}