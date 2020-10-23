/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Util.Collections;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> that limits the underlying filter evaluation
    /// only to the specified collection of keys.
    /// </summary>
    /// <author>Gene Gleyzer  2006.06.12</author>
    /// <author>Goran Milosavljevic  2006.10.24</author>
    /// <author>Tom Beerbower  2009.03.09</author>
    public class InKeySetFilter : IIndexAwareFilter, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Obtain the underying <see cref="IFilter"/>.
        /// </summary>
        /// <value>
        /// The underlying filter.
        /// </value>
        public virtual IFilter Filter
        {
            get { return m_filter; }
        }

        /// <summary>
        /// Obtain the underlying collection of keys.
        /// </summary>
        /// <value>
        /// The underlying key set.
        /// </value>
        /// <since>12.2.1</since>
        public virtual ICollection Keys
        {
            get { return m_keys; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the
        /// <b>InKeySetFilter</b>.
        /// </summary>
        /// <value>
        /// An object that can be used to synchronize access to the
        /// <b>InKeySetFilter</b>.
        /// </value>
        /// <since>12.2.1</since>
        public virtual object SyncRoot
        {
            get { return this; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InKeySetFilter()
        {}

        /// <summary>
        /// Construct an InKeySetFilter.
        /// </summary>
        /// <param name="filter">
        /// The underlying filter.
        /// </param>
        /// <param name="keys">
        /// The collection of keys to limit the filter evaluation to.
        /// </param>
        public InKeySetFilter(IFilter filter, ICollection keys)
        {
            m_filter = filter;
            m_keys   = keys;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Ensure that the underlying keys are converted using the specified converter.
        /// </summary>
        /// <param name="converter">
        /// The Converter to use to convert the keys.
        /// </param>
        /// <since>12.2.1</since>
        public virtual void EnsureConverted(IConverter converter)
        {
            lock (SyncRoot)
            {
                if (!m_isConverted)
                {
                    HashSet convKeys = new HashSet();
                    foreach (object key in Keys)
                    {
                        convKeys.Add(converter.Convert(key));
                    }
                    m_keys        = convKeys;
                    m_isConverted = true;
                }
            }
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Return a human-readable description for this
        /// <b>InKeySetFilter</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>InKeySetFilter</b>.
        /// </returns>
        public override string ToString()
        {
            return "InKeySetFilter(" + m_filter + ", keys=" + 
                CollectionUtils.ToDelimitedString(m_keys, ",") + ')';
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
        public virtual bool EvaluateEntry(ICacheEntry entry)
        {
            return CollectionUtils.Contains(m_keys, entry.Key) && 
                InvocableCacheHelper.EvaluateEntry(m_filter, entry);
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
            m_filter = (IFilter) reader.ReadObject(0);
            m_keys   = reader.ReadCollection(1, new HashSet());
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
            writer.WriteCollection(1, m_keys);
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
            IFilter filter = Filter;
            if (m_keys.Count < keys.Count)
            {
                keys = m_keys;
            }
            return filter is IIndexAwareFilter
                    ? ((IIndexAwareFilter) filter).CalculateEffectiveness(indexes, keys)
                    : keys.Count*ExtractorFilter.EVAL_COST;
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

            IFilter filter = Filter;
            return filter is IIndexAwareFilter
                    ? ((IIndexAwareFilter) filter).ApplyIndex(indexes, keys)
                    : null;
        }

        #endregion
        
        #region Data members

        /// <summary>
        /// The underlying IFilter.
        /// </summary>
        private IFilter m_filter;

        /// <summary>
        /// The underlying collection of keys.
        /// </summary>
        private ICollection m_keys;

        /// <summary>
        /// A flag that indicates that the key set has been converted to internal form.
        /// </summary>
        /// <since>12.2.1</since>
        private bool m_isConverted;

        #endregion
    }
}