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

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// PriorityProcessor is used to explicitly control the scheduling
    /// priority and timeouts for execution of <see cref="IEntryProcessor"/>
    /// -based methods.
    /// </summary>
    /// <remarks>
    /// For example, let's assume that there is a cache that belongs to a
    /// partitioned cache service configured with a <i>task-timeout</i> of 5
    /// seconds. Also assume that there is a particular
    /// <see cref="PreloadRequest"/> processor that could take much longer to
    /// complete due to a large amount of database related processing. Then
    /// we could override the default task timeout value by using the
    /// PriorityProcessor as follows:
    /// <code>
    /// PreloadRequest     procStandard = PreloadRequest.Instance;
    /// PriorityProcessor  procPriority = new PriorityProcessor(procStandard);
    /// procPriority.ExecutionTimeoutMillis = PriorityTaskTimeout.None;
    /// cache.ProcessAll(keys, procPriority);
    /// </code>
    /// This is an advanced feature which should be used judiciously.
    /// </remarks>
    /// <author>Gene Gleyzer  2007.03.20</author>
    /// <since>Coherence 3.3</since>
    public class PriorityProcessor : AbstractPriorityTask, IEntryProcessor, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Obtain the underlying processor.
        /// </summary>
        /// <value>
        /// The processor wrapped by this PriorityProcessor.
        /// </value>
        public IEntryProcessor Processor
        {
            get { return m_processor; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PriorityProcessor()
        {}

        /// <summary>
        /// Construct a PriorityProcessor.
        /// </summary>
        /// <param name="processor">
        /// The processor wrapped by this PriorityProcessor.
        /// </param>
        public PriorityProcessor(IEntryProcessor processor)
        {
            m_processor = processor;
        }

        #endregion

        #region IEntryProcessor implementation

        /// <summary>
        /// Process an <see cref="IInvocableCacheEntry"/>.
        /// </summary>
        /// <param name="entry">
        /// The <b>IInvocableCacheEntry</b> to process.
        /// </param>
        /// <returns>
        /// The result of the processing, if any.
        /// </returns>
        public object Process(IInvocableCacheEntry entry)
        {
            return m_processor.Process(entry);
        }

        /// <summary>
        /// Process a collection of <see cref="IInvocableCacheEntry"/>
        /// objects.
        /// </summary>
        /// <remarks>
        /// This method is semantically equivalent to:
        /// <pre>
        /// IDictionary results = new Hashtable();
        /// foreach (IInvocableCacheEntry entry in entries)
        /// {
        ///     results[entry.Key] = Process(entry);
        /// }
        /// return results;
        /// </pre>
        /// </remarks>
        /// <param name="entries">
        /// A read-only collection of <b>IInvocableCacheEntry</b>
        /// objects to process.
        /// </param>
        /// <returns>
        /// A dictionary containing the results of the processing, up to one
        /// entry for each <b>IInvocableCacheEntry</b> that was processed,
        /// keyed by the keys of the dictionary that were processed, with a
        /// corresponding value being the result of the processing for each
        /// key.
        /// </returns>
        public IDictionary ProcessAll(ICollection entries)
        {
            return m_processor.ProcessAll(entries);
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

            m_processor = (IEntryProcessor) reader.ReadObject(10);
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

            writer.WriteObject(10, m_processor);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Return a human-readable description for this
        /// <b>PriorityProcessor</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>PriorityProcessor</b>.
        /// </returns>
        public override string ToString()
        {
            return "PriorityProcessor (" + m_processor + ")";
        }

        #endregion

        #region Data members

        /// <summary>
        /// The wrapped IEntryProcessor.
        /// </summary>
        private IEntryProcessor m_processor;

        #endregion
    }
}