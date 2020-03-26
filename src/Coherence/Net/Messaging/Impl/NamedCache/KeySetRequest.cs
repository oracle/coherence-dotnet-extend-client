/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// Base class for all NamedCache Protocol
    /// <see cref="NamedCacheRequest"/> messages that include a collection of
    /// keys.
    /// </summary>
    /// <seealso cref="NamedCacheProtocol"/>
    /// <seealso cref="NamedCacheRequest"/>
    /// <author>Ivan Cikic  2006.08.30</author>
    public abstract class KeySetRequest : NamedCacheRequest
    {
        #region Properties

        /// <summary>
        /// The collection of keys associated with this KeySetRequest.
        /// </summary>
        /// <value>
        /// The collection of keys associated with this KeySetRequest.
        /// </value>
        public virtual ICollection Keys
        {
            get { return m_keys; }
            set { m_keys = value; }
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

            Keys = reader.ReadCollection(1, new ArrayList());
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

            writer.WriteCollection(1, Keys);

            // release state
            Keys = null;
        }

        #endregion
        
        #region Extend Overrides

        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription() + ", KeySet=" + Keys;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The collection of keys associated with this KeySetRequest.
        /// </summary>
        private ICollection m_keys;

        #endregion
    }
}