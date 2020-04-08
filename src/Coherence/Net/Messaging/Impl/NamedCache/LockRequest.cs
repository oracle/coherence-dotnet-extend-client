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
    /// The LockRequest is a <see cref="KeyRequest"/> sent to lock a
    /// specified key in a remote NamedCache.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.08.31</author>
    /// <seealso cref="KeyRequest"/>
    /// <seealso cref="NamedCacheProtocol"/>
    public class LockRequest : KeyRequest
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
        /// The number of milliseconds to wait to obtain the lock;
        /// 0 to return immediately; -1 to wait indefinitely.
        /// </summary>
        /// <value>
        /// Number of milliseconds to wait for the lock; 0 to return
        /// immediately, -1 to wait indefinetely.
        /// </value>
        public virtual long TimeoutMillis
        {
            get { return m_timeoutMillis; }
            set { m_timeoutMillis = value; }
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

            writer.WriteInt64(2, TimeoutMillis);
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

            TimeoutMillis = reader.ReadInt64(2);
        }

        #endregion
        
         
        #region Extend Overrides
        
        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription() + ", TimeoutMillis=" + TimeoutMillis;
        }
        
        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 31;

        /// <summary>
        /// The number of milliseconds to wait to obtain the lock;
        /// 0 to return immediately; -1 to wait indefinitely.
        /// </summary>
        private long m_timeoutMillis;

        #endregion
    }
}