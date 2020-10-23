/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// Base class for all NamedCache Protocol
    /// <see cref="NamedCacheRequest"/> messages that include a key.
    /// </summary>
    /// <seealso cref="NamedCacheProtocol"/>
    /// <seealso cref="NamedCacheRequest"/>
    /// <author>Ivan Cikic  2006.08.28</author>
    public abstract class KeyRequest : NamedCacheRequest
    {
        #region Properties
        /// <summary>
        /// The key associated with the KeyRequest.
        /// </summary>
        /// <value>
        /// The key associated with the KeyRequest.
        /// </value>
        public virtual object Key
        {
            get { return m_key; }
            set { m_key = value; }
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
        /// <seealso cref="Request.ReadExternal"/>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            Key = reader.ReadObject(1);
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
        /// <seealso cref="Request.WriteExternal"/>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteObject(1, Key);

            // release state
            Key = null;
        }

        #endregion
        
        #region Extend Overrides

        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription() + ", Key=" + Key;
        }

        #endregion

        #region Data members
        /// <summary>
        /// The key associated with the KeyRequest.
        /// </summary>
        private object m_key;

        #endregion
    }
}