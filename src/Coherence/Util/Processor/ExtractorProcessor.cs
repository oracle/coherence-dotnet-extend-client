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
using Tangosol.Util;
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// <b>ExtractorProcessor</b> is an <see cref="IEntryProcessor"/>
    /// implementations that extracts a value from an object cached in an
    /// <see cref="IInvocableCache"/>.
    /// </summary>
    /// <remarks>
    /// A common usage pattern is:
    /// <pre>
    /// cache.Invoke(key, new ExtractorProcessor(extractor));
    /// </pre>
    /// which is functionally equivalent to the following operation:
    /// <pre>
    /// extractor.Extract(cache[key]);
    /// </pre>
    /// </remarks>
    /// <author>Gene Gleyzer  2005.11.30</author>
    /// <author>Ivan Cikic  2006.10.23</author>
    /// <since>Coherence 3.2</since>
    public class ExtractorProcessor : AbstractProcessor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ExtractorProcessor()
        {}

        /// <summary>
        /// Construct an <b>ExtractorProcessor</b> based on the specified
        /// <see cref="IValueExtractor"/>.
        /// </summary>
        /// <param name="extractor">
        /// An <see cref="IValueExtractor"/> object; passing <c>null</c> is
        /// equivalent to using the <see cref="IdentityExtractor"/>.
        /// </param>
        public ExtractorProcessor(IValueExtractor extractor)
        {
            m_extractor = extractor == null ? IdentityExtractor.Instance : extractor;
        }

        /// <summary>
        /// Construct an <b>ExtractorProcessor</b> for a given member name.
        /// </summary>
        /// <param name="member">
        /// A member name to make a  <see cref="ReflectionExtractor"/> for;
        /// this parameter can also be a dot-delimited sequence of member
        /// names which would result in an <b>ExtractorProcessor</b> based on
        /// the <see cref="ChainedExtractor"/> that is based on an array of
        /// corresponding <b>ReflectionExtractor</b> objects.
        /// </param>
        public ExtractorProcessor(string member)
        {
            m_extractor = member == null || member.Length == 0
                    ? IdentityExtractor.Instance
                    : member.IndexOf('.') < 0
                            ? new ReflectionExtractor(member)
                            : (IValueExtractor) new ChainedExtractor(member);
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
           return entry.Extract(m_extractor);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>PropertyProcessor</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>PropertyProcessor</b> and the passed object
        /// are equivalent <b>PropertyProcessor</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is ExtractorProcessor)
            {
                ExtractorProcessor that = (ExtractorProcessor)o;
                return Equals(m_extractor, that.m_extractor);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>PropertyProcessor</b> object
        /// according to the general <see cref="object.GetHashCode()"/>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>PropertyProcessor</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_extractor.GetHashCode();
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>PropertyProcessor</b>.
        /// </summary>
        /// <returns>
        /// A <b>String</b> description of the <b>PropertyProcessor</b>.
        /// </returns>
        public override string ToString()
        {
            return "ExtractorProcessor(" + m_extractor + ')';
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
            m_extractor = (IValueExtractor) reader.ReadObject(0);
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
            writer.WriteObject(0, m_extractor);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying value extractor.
        /// </summary>
        protected IValueExtractor m_extractor;

        #endregion
    }
}