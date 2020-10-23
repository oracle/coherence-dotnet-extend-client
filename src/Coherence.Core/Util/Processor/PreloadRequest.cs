/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// <b>PreloadRequest</b> is a simple <see cref="IEntryProcessor"/> that
    /// gets an <see cref="IInvocableCacheEntry.Value"/> property.
    /// </summary>
    /// <remarks>
    /// <p>
    /// No results are reported back to the caller.</p>
    /// <p>
    /// The <b>PreloadRequest</b> process provides a means to "pre-load" an
    /// entry or a collection of entries into the cache using the cache's
    /// loader without incurring the cost of sending the value(s) over the
    /// network. If the corresponding entry (or entries) already exists in
    /// the cache, or if the cache does not have a loader, then invoking this
    /// <b>IEntryProcessor</b> has no effect.</p>
    /// </remarks>
    /// <author>Gene Gleyzer  2006.04.28</author>
    /// <author>Ivan Cikic  2006.10.24</author>
    /// <since>Coherence 3.2</since>
    public class PreloadRequest : AbstractProcessor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PreloadRequest()
        {}

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
        public override object Process(IInvocableCacheEntry entry)
        {
            if (!entry.IsPresent)
            {
                object o = entry.Value;
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
            base.ProcessAll(entries);
            return NullImplementation.GetDictionary();
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>PreloadRequest</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>PreloadRequest</b> and the passed object
        /// are equivalent <b>PreloadRequest</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            return o is PreloadRequest;
        }

        /// <summary>
        /// Determine a hash value for the <b>PreloadRequest</b> object
        /// according to the general <see cref="object.GetHashCode()"/>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>PreloadRequest</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return 3;
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>PreloadRequest</b>.
        /// </summary>
        /// <returns>
        /// A <b>String</b> description of the <b>PreloadRequest</b>.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name;
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
        {}

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
        {}

        #endregion

        #region Constants

        /// <summary>
        /// An instance of the PreloadRequest processor.
        /// </summary>
        public static readonly PreloadRequest Instance = new PreloadRequest();

        #endregion
    }
}