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
using Tangosol.Util.Filter;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// Conditional entry processor represents a processor that is invoked
    /// conditionally based on the result of an entry evaluation.
    /// </summary>
    /// <remarks>
    /// If the underlying filter expects to evaluate existent entries only
    /// (i.e. entries for which
    /// <see cref="IInvocableCacheEntry.IsPresent"/> is <b>true</b>, it
    /// should be combined with a <see cref="PresentFilter"/> as follows:
    /// <pre>
    /// IFilter filterPresent = new AndFilter(new PresentFilter(), filter);
    /// </pre>
    /// </remarks>
    /// <author>Gene Gleyzer  2005.10.31</author>
    /// <author>Jason Howes  2005.10.31</author>
    /// <author>Ivan Cikic  2006.10.23</author>
    /// <seealso cref="PresentFilter"/>
    public class ConditionalProcessor : AbstractProcessor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConditionalProcessor()
        {}

        /// <summary>
        /// Construct a <b>ConditionalProcessor</b> for the specified filter
        /// and the processor.
        /// </summary>
        /// <remarks>
        /// The specified entry processor gets invoked if and only if the
        /// filter applied to the <see cref="IInvocableCacheEntry"/>
        /// evaluates to <b>true</b>; otherwize the result of the
        /// <see cref="Process"/> invocation will return <c>null</c>.
        /// </remarks>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <param name="processor">
        /// The entry processor.
        /// </param>
        public ConditionalProcessor(IFilter filter, IEntryProcessor processor)
        {
            Debug.Assert(filter != null && processor != null,
                         "Both filter and processor must be specified");
            m_filter    = filter;
            m_processor = processor;
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
                return m_processor.Process(entry);
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
                if (InvocableCacheHelper.EvaluateEntry(filter, entry))
                {
                    results[entry.Key] = Process(entry);
                }
            }
            return results;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>ConditionalProcessor</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>ConditionalProcessor</b> and the passed
        /// object are equivalent <b>ConditionalProcessors</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is ConditionalProcessor)
            {
                ConditionalProcessor that = (ConditionalProcessor) o;
                return Equals(m_filter, that.m_filter) && Equals(m_processor, that.m_processor);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>ConditionalProcessor</b> object
        /// according to the general <see cref="object.GetHashCode()"/>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>ConditionalProcessor</b>
        /// object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_filter.GetHashCode() + m_processor.GetHashCode();
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>ConditionalProcessor</b>.
        /// </summary>
        /// <returns>
        /// A <b>String</b> description of the <b>ConditionalProcessor</b>.
        /// </returns>
        public override string ToString()
        {
            return "ConditionalProcessor(" + m_filter + ", " + m_processor + ')';
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
            m_filter    = (IFilter) reader.ReadObject(0);
            m_processor = (IEntryProcessor) reader.ReadObject(1);
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
            writer.WriteObject(1, m_processor);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying filter.
        /// </summary>
        protected IFilter m_filter;

        /// <summary>
        /// The underlying entry processor.
        /// </summary>
        protected IEntryProcessor m_processor;

        #endregion
    }
}