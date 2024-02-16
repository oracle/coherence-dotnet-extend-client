/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Diagnostics;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// The <b>CompositeProcessor</b> represents a collection of entry
    /// processors that are invoked sequentially against the same entry.
    /// </summary>
    /// <author>Gene Gleyzer  2005.10.31</author>
    /// <author>Jason Howes  2005.10.31</author>
    /// <author>Ivan Cikic  2005.10.25</author>
    public class CompositeProcessor : AbstractProcessor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CompositeProcessor()
        {}

        /// <summary>
        /// Construct a <b>CompositeProcessor</b> for the specified array of
        /// individual entry processors.
        /// </summary>
        /// <remarks>
        /// The result of the <b>CompositeProcessor</b> execution is an array
        /// of results returned by the individual
        /// <see cref="IEntryProcessor"/> invocations.
        /// </remarks>
        /// <param name="processors">
        /// An array of <b>IEntryProcessor</b> objects.
        /// </param>
        public CompositeProcessor(IEntryProcessor[] processors)
        {
            Debug.Assert(processors != null, "Processor array is null");
            m_processors = processors;
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
            IEntryProcessor[] processors = m_processors;
            int               count      = processors.Length;
            object[]          result     = new object[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = processors[i].Process(entry);
            }
            return result;
        }

        #endregion

        #region Object methods override

        /// <summary>
        /// Compare the <b>CompositeProcessor</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>CompositeProcessor</b> and the passed
        /// object are equivalent <b>CompositeProcessor</b>s.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is CompositeProcessor)
            {
                CompositeProcessor that = (CompositeProcessor) o;
                return CollectionUtils.EqualsDeep(m_processors, that.m_processors);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>CompositeProcessor</b> object
        /// according to the general <b>object.GetHashCode()</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>ConditionalProcessor</b>
        /// object.
        /// </returns>
        public override int GetHashCode()
        {
            IEntryProcessor[] processors = m_processors;
            int               count      = processors.Length;
            int               hash       = 0;
            for (int i = 0; i < count; i++)
            {
                hash += processors[i].GetHashCode();
            }
            return hash;
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
            return "CompositeProcessor(" + CollectionUtils.ToDelimitedString(m_processors, ", ") + ')';
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
            m_processors = (IEntryProcessor[]) reader.ReadArray(0, EMPTY_PROCESSOR_ARRAY);
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
            writer.WriteArray(0, m_processors);
        }

        #endregion

        #region Constants

        /// <summary>
        /// An empty array of <see cref="IEntryProcessor"/> objects.
        /// </summary>
        private static readonly IEntryProcessor[] EMPTY_PROCESSOR_ARRAY = new IEntryProcessor[0];

        #endregion

        #region Data members

        /// <summary>
        ///  The underlying entry processor array.
        /// </summary>
        protected IEntryProcessor[] m_processors;

        #endregion
    }
}