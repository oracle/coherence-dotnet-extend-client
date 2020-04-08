/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// PriorityFilter is used to explicitly control the scheduling priority
    /// and timeouts for execution of filter-based methods.
    /// </summary>
    /// <remarks>
    /// For example, let's assume that there is a cache that belongs to a
    /// partitioned cache service configured with a <i>request-timeout</i>
    /// and <i>task-timeout</i> of 5 seconds. Also assume that we are willing
    /// to wait longer for a particular rarely executed parallel query that
    /// does not employ any indexes. Then we could override the default
    /// timeout values by using the PriorityFilter as follows:
    /// <code>
    /// LikeFilter     filterStandard = new LikeFilter("GetComments", "%fail%");
    /// PriorityFilter filterPriority = new PriorityFilter(filterStandard);
    /// filterPriority.ExecutionTimeoutMillis = PriorityTaskTimeout.None;
    /// filterPriority.RequestTimeoutMillis   = PriorityTaskTimeout.None;
    /// ICollection entries = cache.GetEntries(filterPriority);
    /// </code>
    /// This is an advanced feature which should be used judiciously.
    /// </remarks>
    /// <author>Gene Gleyzer  2007.03.20</author>
    /// <author>Tom Beerbower  2009.03.10</author>
    /// <since>Coherence 3.3</since>
    public class PriorityFilter : AbstractPriorityTask, IIndexAwareFilter
    {
        #region Properties

        /// <summary>
        /// Obtain the underlying filter.
        /// </summary>
        /// <value>
        /// The filter wrapped by this PriorityFilter.
        /// </value>
        public IIndexAwareFilter Filter
        {
            get { return m_filter; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PriorityFilter()
        {}

        /// <summary>
        /// Construct a PriorityFilter.
        /// </summary>
        /// <param name="filter">
        /// The <see cref="IIndexAwareFilter"/> wrapped by this PriorityFilter.
        /// </param>
        public PriorityFilter(IIndexAwareFilter filter)
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
        public bool Evaluate(object o)
        {
            return m_filter.Evaluate(o);
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
            return m_filter.EvaluateEntry(entry);
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
            return m_filter.CalculateEffectiveness(indexes, keys);
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
            return m_filter.ApplyIndex(indexes, keys);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Return a human-readable description for this
        /// <b>PriorityFilter</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>PriorityFilter</b>.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + '(' + m_filter + ')';
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <remarks>
        /// This implementation reserves property index 10.
        /// </remarks>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            m_filter = (IIndexAwareFilter) reader.ReadObject(10);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <remarks>
        /// This implementation reserves property index 10.
        /// </remarks>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteObject(10, m_filter);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The wrapped IIndexAwareFilter.
        /// </summary>
        private IIndexAwareFilter m_filter;

        #endregion
    }
}