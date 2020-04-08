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
    /// The ClearRequest is a <see cref="NamedCacheRequest"/> sent to remove
    /// all mappings in a remote NamedCache.
    /// </summary>
    /// <author>Ivan Cikic  2006.08.30</author>
    /// <seealso cref="NamedCacheRequest"/>
    /// <seealso cref="NamedCacheProtocol"/>
    public class ClearRequest : NamedCacheRequest
    {
        #region Properties

        /// <summary>
        /// Return the identifier for <b>Message</b> object's class.
        /// </summary>
        /// <value>
        /// An identifier that uniquely identifies <b>Message</b> object's
        /// class.
        /// </value>
        /// <seealso cref="Message.TypeId"/>
        public override int TypeId
        {
            get { return TYPE_ID; }
        }

        /// <summary>
        /// <b>true</b> if this is a cache truncate request.
        /// <b>false</b> if this is a cache clear request.
        /// </summary>
        /// <value>
        /// <b>true</b> if is is a truncate request.
        /// <b>false</b> if it is clear request.
        /// </value>
        /// <since>12.2.1</since>
        public virtual bool IsTruncate
        {
            get { return m_isTruncate; }
            set { m_isTruncate = value; }
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

            IsTruncate = reader.ReadBoolean(1);
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

            writer.WriteBoolean(1, IsTruncate);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 8;

        /// <summary>
        /// The value of true indicates that this is a cache truncate request.
        /// </summary>
        /// <since>12.2.1</since>
        private bool m_isTruncate;

        #endregion
    }
}