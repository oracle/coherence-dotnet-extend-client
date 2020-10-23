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
using Tangosol.Util;
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// <b>ConditionalPut</b> is an <see cref="IEntryProcessor"/> that
    /// sets <see cref="IInvocableCacheEntry.Value"/>if the specified
    /// condition is satisfied.
    /// </summary>
    /// <remarks>
    /// <p>
    /// While the <b>ConditionalPut</b> processing could be implemented via
    /// direct key-based <see cref="IQueryCache"/> operations, it is
    /// more efficient and enforces concurrency control without explicit
    /// locking.</p>
    /// <p>
    /// <pre>
    /// <table>
    /// <tr>
    /// <th>IInvocableCache</th>
    /// <th>IConcurrentCache</th>
    /// </tr>
    /// <tr>
    /// <td>filter = PresentFilter.Instance;
    /// cache.Invoke(key, new ConditionalPut(filter, value));</td>
    /// <td>cache.Replace(key, value);</td>
    /// </tr>
    /// <tr>
    /// <td>filter = new NotFilter(PresentFilter.Instance);
    /// cache.Invoke(key, new ConditionalPut(filter, value));</td>
    /// <td>cache.PutIfAbsent(key, value);</td>
    /// </tr>
    /// <tr>
    /// <td>filter = new EqualsFilter(IdentityExtractor.Instance, valueOld);
    /// cache.Invoke(key, new ConditionalPut(filter, valueNew));</td>
    /// <td>cache.Replace(key, valueOld, valueNew);</td>
    /// </tr>
    /// </table>
    /// </pre>
    /// </p>
    /// <p>
    /// Obviously, using more specific, fine-tuned filters (rather than ones
    /// based on the <see cref="IdentityExtractor"/> may provide additional
    /// flexibility and efficiency allowing the put operation to be performed
    /// conditionally on values of specific attributes (or even calculations)
    /// instead of the entire object.</p>
    /// </remarks>
    /// <author>Gene Gleyzer  2006.03.15</author>
    /// <author>Ivan Cikic  2006.10.23</author>
    /// <since>Coherence 3.2</since>
    public class ConditionalPut : AbstractProcessor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConditionalPut()
        {}

        /// <summary>
        /// Construct a <b>ConditionalPut</b> that updates an entry with a
        /// new value if and only if the filter applied to the entry
        /// evaluates to <b>true</b>.
        /// </summary>
        /// <remarks>
        /// The result of the <see cref="Process"/> invocation does not
        /// return any result.
        /// </remarks>
        /// <param name="filter">
        /// The filter to evaluate an entry.
        /// </param>
        /// <param name="value">
        /// A value to update an entry with.
        /// </param>
        public ConditionalPut(IFilter filter, object value) : this(filter, value, false)
        {}

        /// <summary>
        /// Construct a <b>ConditionalPut</b> that updates an entry with a
        /// new value if and only if the filter applied to the entry
        /// evaluates to <b>true</b>.
        /// </summary>
        /// <remarks>
        /// This processor optionally returns the current value as a result
        /// of the invocation if it has not been updated (the filter
        /// evaluated to <b>false</b>).
        /// </remarks>
        /// <param name="filter">
        /// The filter to evaluate an entry.
        /// </param>
        /// <param name="value">
        /// A value to update an entry with.
        /// </param>
        /// <param name="ret">
        /// Specifies whether or not the processor should return the current
        /// value in case it has not been updated.
        /// </param>
        public ConditionalPut(IFilter filter, object value, bool ret)
        {
            Debug.Assert(filter != null, "Filter is null");

            m_filter = filter;
            m_value  = value;
            m_return = ret;
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
            if (InvocableCacheHelper.EvaluateEntry(m_filter, entry))
            {
                entry.SetValue(m_value, false);
                return null;
            }
            return m_return ? entry.Value : null;
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
        /// A dictionary containing the results of the processing, up to one
        /// entry for each <b>IInvocableCacheEntry</b> that was
        /// processed, keyed by the keys of the dictionary that were
        /// processed, with a corresponding value being the result of the
        /// processing for each key.
        /// </returns>
        public override IDictionary ProcessAll(ICollection entries)
        {
            IDictionary results = new LiteDictionary();
            IFilter     filter  = m_filter;
            object      value   = m_value;
            bool        ret     = m_return;
            
            foreach (IInvocableCacheEntry entry in entries)
            {
                if (InvocableCacheHelper.EvaluateEntry(filter, entry))
                {
                    entry.SetValue(value, false);
                }
                else if (ret)
                {
                    results[entry.Key] = entry.Value;
                }
            }
            return results;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>ConditionalPut</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>ConditionalPut</b> and the passed object
        /// are equivalent <b>ConditionalPut</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is ConditionalPut)
            {
                ConditionalPut that = (ConditionalPut) o;
                return Equals(m_filter, that.m_filter)
                       && Equals(m_value, that.m_value)
                       && m_return == that.m_return;
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>ConditionalPut</b> object
        /// according to the general <see cref="object.GetHashCode()"/>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>ConditionalPut</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            object value = m_value;
            int    hash  = value == null ? 0 : value.GetHashCode();
            return hash + m_filter.GetHashCode() + (m_return ? -1 : 1);
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>ConditionalPut</b>.
        /// </summary>
        /// <returns>
        /// A <b>String</b> description of the <b>ConditionalPut</b>.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + "{Filter = " + m_filter + ", Value=" + m_value +
                    ", ReturnRequired= " + m_return + '}';
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
            m_value  = reader.ReadObject(1);
            m_return = reader.ReadBoolean(2);
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
            writer.WriteObject(1, m_value);
            writer.WriteObject(2, m_return);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying filter.
        /// </summary>
        protected IFilter m_filter;

        /// <summary>
        /// Specifies the new value to update an entry with.
        /// </summary>
        protected object m_value;

        /// <summary>
        /// Specifies whether or not a return value is required.
        /// </summary>
        protected bool m_return;

        #endregion
    }
}