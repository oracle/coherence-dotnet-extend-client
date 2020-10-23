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
    /// The PutRequest is a <see cref="KeyRequest"/> sent to map a key to a
    /// value in a remote NamedCache.
    /// </summary>
    /// <author>Ivan Cikic  2006.08.30</author>
    /// <seealso cref="KeyRequest"/>
    /// <seealso cref="NamedCacheProtocol"/>
    public class PutRequest : KeyRequest
    {
        #region Properties

        /// <summary>
        /// The entry expiration value.
        /// </summary>
        /// <value>
        /// The entry expiration value.
        /// </value>
        public virtual long ExpiryDelay
        {
            get { return m_expiryDelay; }
            set { m_expiryDelay = value; }
        }

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
        /// The new entry value.
        /// </summary>
        /// <value>
        /// The new entry value.
        /// </value>
        public virtual object Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        /// <summary>
        /// If <b>true</b>, this PutRequest should return the old value back
        /// to the caller; otherwise the return value will be ignored.
        /// </summary>
        /// <value>
        /// <b>true</b> if this PutRequest should return the old value back
        /// to the caller; <b>false</b> otherwise.
        /// </value>
        public virtual bool IsReturnRequired
        {
            get { return m_isReturnRequired; }
            set { m_isReturnRequired = value; }
        }

        #endregion

        #region IPortableObject implementation

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

            writer.WriteObject(2, Value);
            writer.WriteInt64(3, ExpiryDelay);
            writer.WriteBoolean(4, IsReturnRequired);

            // release state
            Value = null;
        }

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

            Value            = reader.ReadObject(2);
            ExpiryDelay      = reader.ReadInt64(3);
            IsReturnRequired = reader.ReadBoolean(4);

        }

        #endregion
        
        #region Extend Overrides
        
        /// <inheritdoc />
        protected override string GetDescription()
        {
            object oValue = Value;
            
            return base.GetDescription()
                   + ", Value="  + (oValue == null ? "null" : oValue.GetType().Name + "(HashCode=" + oValue.GetHashCode() + ')')
                   + ", Expiry=" + ExpiryDelay;
        }
        
        #endregion

        #region Data members

        /// <summary>
        /// The entry expiration value.
        /// </summary>
        private long m_expiryDelay;

        /// <summary>
        /// If true this PutRequest should return the old value back
        /// to the caller; otherwise the return value will be ignored.
        /// </summary>
        private bool m_isReturnRequired;

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 5;

        /// <summary>
        /// The new entry value.
        /// </summary>
        private object m_Value;

        #endregion

    }
}