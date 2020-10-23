/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// The RemoveRequest is a <see cref="KeyRequest"/> sent to remove a
    /// mapping in a remote NamedCache.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.09.04</author>
    /// <seealso cref="KeyRequest"/>
    /// <seealso cref="NamedCacheProtocol"/>
    public class RemoveRequest : KeyRequest
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
        /// If <b>true</b>, this RemoveRequest should return the old value
        /// back to the caller; otherwise the return value will be ignored.
        /// </summary>
        /// <value>
        /// <b>true</b> if this RemoveRequest should return the old value
        /// back to the caller; <b>false</b> otherwise.
        /// </value>
        public virtual bool IsReturnRequired
        {
            get { return m_isReturnRequired; }
            set { m_isReturnRequired = value; }
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

            IsReturnRequired = reader.ReadBoolean(2);
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

            writer.WriteBoolean(2, IsReturnRequired);
        }

        #endregion

        #region Data members

        /// <summary>
        /// If <b>true</b>, this RemoveRequest should return the old value
        /// back to the caller; otherwise the return value will be ignored.
        /// </summary>
        [NonSerialized]
        private bool m_isReturnRequired;

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 6;

        #endregion
    }
}