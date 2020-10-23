/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Util;

namespace Tangosol.Web
{
    /// <summary>
    /// A holder object that stores model and metadata for a single session.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.09.21</author>
    public class SessionHolder : IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Deserialization constructor (for internal use only).
        /// </summary>
        public SessionHolder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SessionHolder.
        /// </summary>
        /// <param name="model">
        /// The session model.
        /// </param>
        /// <param name="lockId">
        /// The lock ID.
        /// </param>
        /// <param name="initialized">
        /// A flag to indicated whether the session is initialized.
        /// </param>
        /// <param name="timeout">
        /// The session timeout value.
        /// </param>
        public SessionHolder(ISessionModel model, long lockId, 
                             bool initialized, TimeSpan timeout)
        {
            m_model       = model;
            m_lockId      = lockId;
            m_initialized = initialized;
            m_timeout     = timeout;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Object version (used for optimistic locking).
        /// </summary>
        public long Version
        {
            get { return m_version; }
            set { m_version = value; }
        }

        /// <summary>
        /// Lock identifier.
        /// </summary>
        public long LockId
        {
            get { return m_lockId; }
            set { m_lockId = value; }
        }

        /// <summary>
        /// Lock time.
        /// </summary>
        public DateTime LockTime
        {
            get { return m_lockTime; }
            set { m_lockTime = value; }
        }

        /// <summary>
        /// True if the session is locked, false otherwise.
        /// </summary>
        public bool IsLocked
        {
            get { return m_lockId != 0; }
        }

        /// <summary>
        /// Lock age.
        /// </summary>
        public TimeSpan LockAge
        {
            get
            {
                return LockTime == DateTime.MinValue
                           ? TimeSpan.Zero
                           : DateTime.Now - LockTime;
            }
        }

        /// <summary>
        /// Flag specifying whether this session is initailized.
        /// </summary>
        public bool Initialized
        {
            get { return m_initialized; }
            set { m_initialized = value; }
        }

        /// <summary>
        /// Session timeout.
        /// </summary>
        public TimeSpan Timeout
        {
            get { return m_timeout; }
            set { m_timeout = value; }
        }

        /// <summary>
        /// Serialized session model.
        /// </summary>
        public Binary SerializedModel
        {
            get { return m_binModel; }
            set { m_binModel = value; }
        }

        /// <summary>
        /// Session model.
        /// </summary>
        public ISessionModel Model
        {
            get { return m_model; }
            set { m_model = value; }
        }

        #endregion

        #region Implementation of IPortableObject

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
        public void ReadExternal(IPofReader reader)
        {
            m_version     = reader.ReadInt64(VERSION);
            m_lockId      = reader.ReadInt64(LOCK_ID);
            m_lockTime    = reader.ReadDateTime(LOCK_TIME);
            m_initialized = reader.ReadBoolean(INITIALIZED);
            m_timeout     = TimeSpan.FromMilliseconds(reader.ReadInt64(TIMEOUT));
            m_binModel    = reader.ReadBinary(ITEMS);
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
        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt64(VERSION, m_version);
            writer.WriteInt64(LOCK_ID, m_lockId);
            writer.WriteDateTime(LOCK_TIME, m_lockTime);
            writer.WriteBoolean(INITIALIZED, m_initialized);
            writer.WriteInt64(TIMEOUT, (long) m_timeout.TotalMilliseconds);
            writer.WriteBinary(ITEMS, m_binModel);
        }

        #endregion

        #region POF constants

        private const int VERSION     = 0;
        private const int LOCK_ID     = 1;
        private const int LOCK_TIME   = 2;
        private const int INITIALIZED = 3;
        private const int TIMEOUT     = 4;
        private const int ITEMS       = 5;

        #endregion

        #region Data members

        /// <summary>
        /// Object version.
        /// </summary>
        private long m_version;

        /// <summary>
        /// Lock identifier.
        /// </summary>
        private long m_lockId;

        /// <summary>
        /// Lock time.
        /// </summary>
        private DateTime m_lockTime = DateTime.MinValue;

        /// <summary>
        /// Flag specifying whether this session is initailized.
        /// </summary>
        private bool m_initialized;

        /// <summary>
        /// Session timeout.
        /// </summary>
        private TimeSpan m_timeout;

        /// <summary>
        /// Serialized session model.
        /// </summary>
        private Binary m_binModel;

        /// <summary>
        /// Session state items.
        /// </summary>
        [NonSerialized]
        private ISessionModel m_model;

        #endregion
    }
}