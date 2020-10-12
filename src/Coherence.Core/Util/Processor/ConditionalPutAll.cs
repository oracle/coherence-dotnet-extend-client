/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.Diagnostics;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Util.Collections;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// <b>ConditionalPutAll</b> is an <see cref="IEntryProcessor"/> that
    /// sets <see cref="IInvocableCacheEntry.Value"/> for multiple
    /// entries that satisfy the specified condition.
    /// </summary>
    /// <remarks>
    /// Obviously, using more specific, fine-tuned filters may provide
    /// additional flexibility and efficiency allowing the multi-put
    /// operations to be performed conditionally on values of specific
    /// attributes (or even calculations) instead of a simple existence
    /// check.
    /// </remarks>
    /// <author>Gene Gleyzer  2006.04.28</author>
    /// <author>Ivan Cikic  2006.10.23</author>
    /// <since>Coherence 3.2</since>
    public class ConditionalPutAll : AbstractProcessor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConditionalPutAll()
        {}

        /// <summary>
        /// Construct a <b>ConditionalPutAll</b> processor that updates an
        /// entry with a new value if and only if the filter applied to the
        /// entry evaluates to <b>true</b>.
        /// </summary>
        /// <remarks>
        /// The new value is extracted from the specified map based on the
        /// entry's key.
        /// </remarks>
        /// <param name="filter">
        /// The filter to evaluate all supplied entries.
        /// </param>
        /// <param name="dictionary">
        /// A dictionary of values to update entries with.
        /// </param>
        public ConditionalPutAll(IFilter filter, IDictionary dictionary)
        {
            Debug.Assert(filter != null, "Filter is null");
            Debug.Assert(dictionary != null, "Map is null");

            m_filter     = filter;
            m_dictionary = new HashDictionary(dictionary);
        }

        #endregion

        #region IEntryProcess implementation

        /// <summary>
        /// Process an <see cref="IInvocableCacheEntry"/>.
        /// </summary>
        /// <param name="entry">
        /// The <b>IInvocableCacheEntry</b> to process.
        /// </param>
        /// <returns>
        /// The result of the processing, if any.
        /// </returns>
        public override object Process(IInvocableCacheEntry entry)
        {
            IDictionary dictionary = m_dictionary;
            object      key        = entry.Key;

            if (dictionary.Contains(key) && InvocableCacheHelper.EvaluateEntry(m_filter, entry))
            {
                entry.SetValue(dictionary[key], false);
            }
            return null;
        }

        /// <summary>
        /// Process a collection of <see cref="IInvocableCacheEntry"/>
        /// objects.
        /// </summary>
        /// <param name="entries">
        /// A read-only collection of <b>IInvocableCacheEntry</b>
        /// objects to process.
        /// </param>
        /// <returns>
        /// An empty, immutable dictionary.
        /// </returns>
        public override IDictionary ProcessAll(ICollection entries)
        {
            IDictionary dictionary = m_dictionary;
            IFilter     filter     = m_filter;

            foreach (IInvocableCacheEntry entry in entries)
            {
                object key = entry.Key;
                if (dictionary.Contains(key) && InvocableCacheHelper.EvaluateEntry(filter, entry))
                {
                    entry.SetValue(dictionary[key], false);
                }
            }
            return NullImplementation.GetDictionary();
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>ConditionalPutAll</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>ConditionalPutAll</b> and the passed
        /// object are equivalent <b>ConditionalPutAll</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is ConditionalPutAll)
            {
                ConditionalPutAll that = (ConditionalPutAll) o;
                return Equals(m_filter, that.m_filter)
                       && Equals(m_dictionary, that.m_dictionary);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>ConditionalPut</b> object
        /// according to the general <see cref="object.GetHashCode()"/>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>ConditionalPutAll</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_filter.GetHashCode() + m_dictionary.GetHashCode();
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>ConditionalPutAll</b>.
        /// </summary>
        /// <returns>
        /// A <b>String</b> description of the <b>ConditionalPutAll</b>.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + "{Filter = " + m_filter + ", Map=" + m_dictionary + '}';
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
            m_filter     = (IFilter) reader.ReadObject(0);
            m_dictionary = (IDictionary) reader.ReadObject(1);
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
            // note: not WriteDictionary(), just in case the map is a POF object
            writer.WriteObject(1, m_dictionary);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying filter.
        /// </summary>
        protected IFilter m_filter;

        /// <summary>
        /// Specifies the map of new values.
        /// </summary>
        protected IDictionary m_dictionary;

        #endregion
    }
}