/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Net.Messaging.Impl.CacheService
{
    /// <summary>
    /// Base class for all CacheService Protocol <see cref="Request"/>
    /// messages.
    /// </summary>
    /// <author>Ana Cikic  2006.08.25</author>
    /// <seealso cref="Request"/>
    /// <seealso cref="CacheServiceProtocol"/>
    public abstract class CacheServiceRequest : Request
    {
        #region Properties

        /// <summary>
        /// The name of the target NamedCache.
        /// </summary>
        /// <value>
        /// The name of the target NamedCache.
        /// </value>
        public virtual string CacheName
        {
            get { return m_cacheName; }
            set { m_cacheName = value; }
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
        /// <seealso cref="Message.ReadExternal"/>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            CacheName = reader.ReadString(1);
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
        /// <seealso cref="Message.WriteExternal"/>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteString(1, CacheName);
        }

        #endregion
        
        #region Extend Overrides

        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription() + ", CacheName=" + CacheName;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The name of the target NamedCache.
        /// </summary>
        private string m_cacheName;

        #endregion
    }
}