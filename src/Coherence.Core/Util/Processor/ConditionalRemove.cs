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

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// <b>ConditionalRemove</b> is an <see cref="IEntryProcessor"/> that
    /// performs an <see cref="IInvocableCacheEntry.Remove"/>
    /// operation if the specified condition is satisfied.
    /// </summary>
    /// <remarks>
    /// <p>
    /// While the <b>ConditionalRemove</b> processing could be implemented via
    /// direct key-based <see cref="IQueryCache"/> operations, it is
    /// more efficient and enforces concurrency control without explicit
    /// locking.</p>
    /// <p>
    /// For example, the following operations are functionally similar, but
    /// the <b>IInvocableDictionary</b> versions (a) perform significantly
    /// better for partitioned caches; (b) provide all necessary concurrency
    /// control (which is ommited from the <b>IQueryCache</b> examples):
    /// </p>
    /// <pre>
    /// <table>
    /// <tr>
    /// <th>IInvocableCache</th>
    /// <th>IQueryCache</th>
    /// </tr>
    /// <tr>
    /// <td>cache.invoke(key, new ConditionalRemove(filter));</td>
    /// <td>if (filter.Evaluate(cache[key]) cache.Remove(key);</td>
    /// </tr>
    /// <tr>
    /// <td>cache.InvokeAll(colKeys, new ConditionalRemove(filter));</td>
    /// <td>foreach (object key in colKeys)
    ///     if (filter.Evaluate(cache.[key])
    ///         cache.Remove(key);</td>
    /// </tr>
    /// <tr>
    /// <td>cache.InvokeAll(filter1, new ConditionalRemove(filter2);</td>
    /// <td>foreach (object key in cache.GetKeys(filter1))
    ///     if (filter2.Evaluate(cache[key])
    ///         cache.Remove(key);</td>
    /// </tr>
    /// <tr>
    /// <td>cache.InvokeAll(filter, new
    ///                 ConditionalRemove(AlwaysFilter.INSTANCE));</td>
    /// <td>ICollection colKeys = cache.GetKeys(filter);
    ///     cache.Keys.RemoveAll(colKeys);</td>
    /// </tr>
    /// </table>
    /// </pre>
    /// </remarks>
    /// <author>Gene Gleyzer  2006.03.15</author>
    /// <author>Ivan Cikic  2006.10.23</author>
    /// <since>Coherence 3.2</since>
    public class ConditionalRemove : AbstractProcessor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConditionalRemove()
        {}

        /// <summary>
        /// Construct a <b>ConditionalRemove</b> processor that removes an
        /// <see cref="IInvocableCacheEntry"/> if and only if the filter
        /// applied to the entry evaluates to <b>true</b>.
        /// </summary>
        /// <remarks>
        /// The result of the <see cref="Process"/> invocation
        /// does not return any result.
        /// </remarks>
        /// <param name="filter">
        /// The filter to evaluate an entry.
        /// </param>
        public ConditionalRemove(IFilter filter) : this(filter, false)
        {}

        /// <summary>
        /// Construct a <b>ConditionalRemove</b> processor that removes an
        /// <see cref="IInvocableCacheEntry"/> if and only if the filter
        /// applied to the entry evaluates to <b>true</b>.
        /// </summary>
        /// <remarks>
        /// This processor may optionally return the current value as a
        /// result of the invocation if it has not been removed (the filter
        /// evaluated to <b>false</b>).
        /// </remarks>
        /// <param name="filter">
        /// The filter to evaluate an entry.
        /// </param>
        /// <param name="ret">
        /// Specifies whether or not the processor should return the
        /// current value if it has not been removed.
        /// </param>
        public ConditionalRemove(IFilter filter, bool ret)
        {
            Debug.Assert(filter != null, "Filter is null");
            m_filter = filter;
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
            if (entry.IsPresent && InvocableCacheHelper.EvaluateEntry(m_filter, entry))
            {
                entry.Remove(false);
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

            foreach (IInvocableCacheEntry entry in entries)
            {
                if (entry.IsPresent && InvocableCacheHelper.EvaluateEntry(filter, entry))
                {
                    entry.Remove(false);
                }
                else if (m_return)
                {
                    results[entry.Key] = entry.Value;
                }
            }
            return results;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>ConditionalRemove</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>ConditionalRemove</b> and the passed object
        /// are equivalent <b>ConditionalRemove</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is ConditionalRemove)
            {
                ConditionalRemove that = (ConditionalRemove) o;
                return Equals(m_filter, that.m_filter)
                       && m_return == that.m_return;
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>ConditionalRemove</b> object
        /// according to the general <see cref="object.GetHashCode()"/>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>ConditionalRemove</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_filter.GetHashCode() + (m_return ? -1 : 1);
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>ConditionalRemove</b>.
        /// </summary>
        /// <returns>
        /// A <b>String</b> description of the <b>ConditionalRemove</b>.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + "{Filter = " + m_filter + ", ReturnRequired= " + m_return + '}';
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
            m_return = reader.ReadBoolean(1);
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
            writer.WriteObject(1, m_return);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying filter.
        /// </summary>
        protected IFilter m_filter;

        /// <summary>
        /// Specifies whether or not a return value is required.
        /// </summary>
        protected bool m_return;

        #endregion
    }
}