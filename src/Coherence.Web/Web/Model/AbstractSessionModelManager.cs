/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Web.SessionState;
using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Util;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;
using Tangosol.Util.Processor;

namespace Tangosol.Web.Model
{
    /// <summary>
    /// Abstract base implementation of <see cref="ISessionModelManager"/>. 
    /// </summary>
    /// <author>Aleksandar Seovic  2009.09.22</author>
    public abstract class AbstractSessionModelManager : ISessionModelManager
    {
        #region Constructors

        /// <summary>
        /// Construct new AbstractSessionModelManager.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        protected AbstractSessionModelManager(ISerializer serializer)
            : this(serializer, null)
        {}

        /// <summary>
        /// Construct new AbstractSessionModelManager.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        /// <param name="cacheName">The cache name.</param>
        protected AbstractSessionModelManager(ISerializer serializer, string cacheName)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            m_sessionCache = String.IsNullOrEmpty(cacheName)
                ? CacheFactory.GetCache(SESSION_CACHE_NAME)
                : CacheFactory.GetCache(cacheName);
            m_serializer   = serializer;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The <b>SessionStateItemExpireCallback</b> delegate for the
        /// <b>Session_OnEnd</b> event as defined in the Global.asax file.
        /// </summary>
        public SessionStateItemExpireCallback SessionEndCallback
        {
            get { return m_sessionEndCallback; }
            set { m_sessionEndCallback = value; }
        }

        /// <summary>
        /// Session cache.
        /// </summary>
        public INamedCache SessionCache
        {
            get { return m_sessionCache; }
        }

        /// <summary>
        /// Serializer to use.
        /// </summary>
        public ISerializer Serializer
        {
            get { return m_serializer; }
        }

        #endregion

        #region Implementation of ISessionModelManager

        /// <summary>
        /// Create a new instance of an appropriate 
        /// <see cref="ISessionModel"/> for this model manager.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="ISessionModel"/>.
        /// </returns>
        public abstract ISessionModel CreateSessionModel();

        /// <summary>
        /// Load session from the cache.
        /// </summary>
        /// <param name="sessionId">Session key.</param>
        /// <param name="exclusive">
        /// Flag specifying whether to obtain exclusive access to the 
        /// specified session.
        /// </param>
        /// <returns>
        /// The <b>SessionHolder</b> object.
        /// </returns>
        public virtual SessionHolder LoadSession(SessionKey sessionId, bool exclusive)
        {
            SessionHolder holder = GetSessionHolder(sessionId, exclusive);
            if (holder != null && holder.SerializedModel != null)
            {
                holder.Model = Deserialize(holder.SerializedModel);
                holder.Model.SessionId = sessionId;
            }
            return holder;
        }

        /// <summary>
        /// Update session items in the cache and release the exclusive lock.
        /// </summary>
        /// <param name="sessionId">Session key.</param>
        /// <param name="holder">Session holder.</param>
        /// <param name="newSession">
        /// Flag specifying whether this is a new session.
        /// </param>
        public virtual void SaveSession(SessionKey sessionId, SessionHolder holder, bool newSession)
        {
            long          lockId      = holder.LockId;
            bool          initialized = holder.Initialized;
            long          timeout     = (long) holder.Timeout.TotalMilliseconds;
            ISessionModel model       = holder.Model;
            Binary        binModel    = newSession || model.Dirty ? Serialize(model) : null;
            IDictionary   extAttr     = model.Dirty ? GetExternalAttributes(model) : null;
            IList         obsExtAttr  = model.Dirty ? GetObsoleteExternalAttributes(model) : null;

            SessionCache.Invoke(sessionId,
                new SaveSessionProcessor(lockId, newSession, initialized, timeout, binModel, extAttr, obsExtAttr));

            if (newSession && m_sessionEndCallback != null)
            {
                SessionEndListener listener = new SessionEndListener(this, sessionId);
                SessionCache.AddCacheListener(listener, listener.EventFilter, false);
            }
        }

        /// <summary>
        /// Releases a lock on the session in a data store.
        /// </summary>
        /// <param name="sessionId">
        /// Session key for the current request.
        /// </param>
        /// <param name="lockId">
        /// The lock identifier for the current request.
        /// </param>
        public virtual void ReleaseSession(SessionKey sessionId, long lockId)
        {
            SessionCache.Invoke(sessionId, new ReleaseSessionProcessor(lockId));
        }

        /// <summary>
        /// Delete session from the cache.
        /// </summary>
        /// <param name="sessionId">Session key.</param>
        public virtual void RemoveSession(SessionKey sessionId)
        {
            SessionCache.Remove(sessionId);
        }

        /// <summary>
        /// Reset session timeout.
        /// </summary>
        /// <param name="sessionId">Session key.</param>
        public virtual void ResetSessionTimeout(SessionKey sessionId)
        {
            SessionCache.Invoke(sessionId, new ResetSessionTimeoutProcessor());
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Get session holder from the cache.
        /// </summary>
        /// <param name="key">
        /// Session key.
        /// </param>
        /// <param name="exclusive">
        /// Flag specifying whether to obtain exclusive access to the specified
        /// session.
        /// </param>
        /// <returns>
        /// The <b>SessionHolder</b> object.
        /// </returns>
        protected virtual SessionHolder GetSessionHolder(SessionKey key, bool exclusive)
        {
            return (SessionHolder) 
                (exclusive
                    ? SessionCache.Invoke(key, new AcquireSessionProcessor())
                    : SessionCache[key]);
        }

        /// <summary>
        /// Get external attributes.
        /// </summary>
        /// <param name="model">
        /// Model to get the external attributes from.
        /// </param>
        /// <returns>External attributes dictionary.</returns>
        protected virtual IDictionary GetExternalAttributes(ISessionModel model)
        {
            return null;
        }

        /// <summary>
        /// Get obsolete external attributes.
        /// </summary>
        /// <param name="model">
        /// Model to get the obsolete external attributes from.
        /// </param>
        /// <returns>External attributes dictionary.</returns>
        protected virtual IList GetObsoleteExternalAttributes(ISessionModel model)
        {
            return null;
        }

        /// <summary>
        /// Serialize specified <b>ISessionModel</b>.
        /// </summary>
        /// <param name="model">
        /// The <b>ISessionModel</b> to serialize.
        /// </param>
        /// <returns>
        /// The serialized form of the given <b>ISessionModel</b>.
        /// </returns>
        protected virtual Binary Serialize(ISessionModel model)
        {
            BinaryMemoryStream stream = new BinaryMemoryStream(4 * 1024);
            using (DataWriter writer = new DataWriter(stream))
            {
                model.WriteExternal(writer);
                return stream.ToBinary();
            }
        }

        /// <summary>
        /// Deserialize specified Binary into a <b>ISessionModel</b>.
        /// </summary>
        /// <param name="binModel">
        /// A Binary containing the serialized form of a <b>ISessionModel</b>.
        /// </param>
        /// <returns>
        /// The deserialized <b>ISessionModel</b>.
        /// </returns>
        public virtual ISessionModel Deserialize(Binary binModel)
        {
            using (DataReader reader = binModel.GetReader())
            {
                ISessionModel model = CreateSessionModel();
                model.ReadExternal(reader);
                return model;
            }
        }

        #endregion

        #region Entry processors

        /// <summary>
        /// Entry processor that acquires session for exclusive access.
        /// </summary>
        public class AcquireSessionProcessor : AbstractClusterProcessor
        {}

        /// <summary>
        /// Entry processor that releases exclusive access on a session.
        /// </summary>
        public class ReleaseSessionProcessor : AbstractClusterProcessor
        {
            /// <summary>
            /// Construct a new instance of SessionReleaseExclusive.
            /// </summary>
            /// <param name="lockId">Lock identifier.</param>
            public ReleaseSessionProcessor(long lockId)
            {
                m_lockId = lockId;
            }

            /// <summary>
            /// Serializes this processor into a POF stream.
            /// </summary>
            /// <param name="writer">
            /// The POF writer.
            /// </param>
            public override void WriteExternal(IPofWriter writer)
            {
                writer.WriteInt64(0, m_lockId);
            }

            /// <summary>
            /// Lock identifier.
            /// </summary>
            private readonly long m_lockId;
        }

        /// <summary>
        /// Entry processor that updates session items and releases the lock.
        /// </summary>
        public class SaveSessionProcessor : AbstractClusterProcessor
        {
            /// <summary>
            /// Construct a new instance of SaveSessionProcessor.
            /// </summary>
            /// <param name="lockId">Lock identifier.</param>
            /// <param name="newSession">
            /// Flag specifying whether this is a new session.
            /// </param>
            /// <param name="initialized">
            /// Flag specifying whether this session is initialized.
            /// </param>
            /// <param name="timeout">Session timeout.</param>
            /// <param name="binModel">Serialized session model.</param>
            /// <param name="externalAttributes">External attributes.</param>
            /// <param name="obsoleteExternalAttributes">Obsolete external
            /// attributes.</param>
            public SaveSessionProcessor(long lockId, bool newSession, bool initialized,
                    long timeout, Binary binModel, IDictionary externalAttributes,
                    IList obsoleteExternalAttributes)
            {
                m_lockId                     = lockId;
                m_newSession                 = newSession;
                m_initialized                = initialized;
                m_timeout                    = timeout;
                m_binModel                   = binModel;
                m_externalAttributes         = externalAttributes;
                m_obsoleteExternalAttributes = obsoleteExternalAttributes;
            }

            /// <summary>
            /// Serializes this processor into a POF stream.
            /// </summary>
            /// <param name="writer">
            /// The POF writer.
            /// </param>
            public override void WriteExternal(IPofWriter writer)
            {
                writer.WriteInt64(0, m_lockId);
                writer.WriteBoolean(1, m_newSession);
                writer.WriteBoolean(2, m_initialized);
                writer.WriteInt64(3, m_timeout);
                writer.WriteBinary(4, m_binModel);
                writer.WriteDictionary(5, m_externalAttributes, typeof(String), typeof(Binary));
                writer.WriteCollection(6, m_obsoleteExternalAttributes, typeof(String));
            }

            /// <summary>
            /// Lock identifier.
            /// </summary>
            private readonly long m_lockId;

            /// <summary>
            /// Flag specifying whether this is a new session.
            /// </summary>
            private readonly bool m_newSession;

            /// <summary>
            /// Flag specifying whether this session is initialized.
            /// </summary>
            private readonly bool m_initialized;

            /// <summary>
            /// Session timeout.
            /// </summary>
            private readonly long m_timeout;

            /// <summary>
            /// Serialized session model.
            /// </summary>
            private readonly Binary m_binModel;

            /// <summary>
            /// External attributes (used by Split model only).
            /// </summary>
            private readonly IDictionary m_externalAttributes;

            /// <summary>
            /// Obsolete external attributes (used by Split model only).
            /// </summary>
            private readonly IList m_obsoleteExternalAttributes;
        }

        /// <summary>
        /// Entry processor that resets session timeout.
        /// </summary>
        public class ResetSessionTimeoutProcessor : AbstractClusterProcessor
        {}

        #endregion

        #region Inner class: SessionEndListener

        /// <summary>
        /// <b>ICacheListener</b> implementation that listens for HTTP session
        /// deleted events and dispatches <b>Session_OnEnd</b> events to a
        /// <b>SessionStateItemExpireCallback</b> delegate.
        /// </summary>
        private class SessionEndListener : AbstractCacheListener
        {
            #region Constructors

            /// <summary>
            /// Create a new SessionEndListener that will dispatch
            /// <b>Session_OnEnd</b> events when an HTTP session 
            /// expires from the <b>INamedCache</b>.
            /// </summary>
            /// <param name="modelManager">
            /// The model manager to use.
            /// </param>
            /// <param name="sessionId">
            /// Session identifier.
            /// </param>
            public SessionEndListener(ISessionModelManager modelManager, SessionKey sessionId)
            {
                m_modelManager = modelManager;
                m_sessionId    = sessionId;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Return an event filter that should be used when registering
            /// or unregistering this listener.
            /// </summary>
            public CacheEventFilter EventFilter
            {
                get
                {
                    return new CacheEventFilter(CacheEventFilter.CacheEventMask.Deleted,
                                                new EqualsFilter(new KeyExtractor(IdentityExtractor.Instance), m_sessionId));
                }
            }

            #endregion

            #region ICacheListener implementation

            /// <summary>
            /// Invoked when a cache entry has been deleted.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/> carrying the remove
            /// information.
            /// </param>
            public override void EntryDeleted(CacheEventArgs evt)
            {
                AbstractSessionModelManager modelManager = (AbstractSessionModelManager) m_modelManager;
                modelManager.SessionCache.RemoveCacheListener(this, EventFilter);

                SessionKey    key    = (SessionKey) evt.Key;
                SessionHolder holder = (SessionHolder) evt.OldValue;
                if (holder != null)
                {
                    holder.Model           = modelManager.Deserialize(holder.SerializedModel);
                    holder.Model.SessionId = key;
                    modelManager.SessionEndCallback.Invoke(key.SessionId,
                                      new SessionStateStoreData(holder.Model, null, (int) holder.Timeout.TotalMinutes));
                }
                else
                {
                    modelManager.SessionEndCallback.Invoke(key.SessionId, new SessionStateStoreData(null, null, 0));
                }
            }

            #endregion

            #region Data members

            /// <summary>
            /// The model manager to use.
            /// </summary>
            private readonly ISessionModelManager m_modelManager;

            /// <summary>
            /// ID of the session this listener is registered for.
            /// </summary>
            private readonly SessionKey m_sessionId;

            #endregion
        }

        #endregion

        #region Data members

        /// <summary>
        /// Default session cache name.
        /// </summary>
        public const string SESSION_CACHE_NAME = "aspnet-session-storage";

        /// <summary>
        /// Session cache.
        /// </summary>
        private readonly INamedCache m_sessionCache;

        /// <summary>
        /// Serializer to use.
        /// </summary>
        private readonly ISerializer m_serializer;

        /// <summary>
        /// The <b>SessionStateItemExpireCallback</b> delegate for the
        /// <b>Session_OnEnd</b> event as defined in the Global.asax file.
        /// </summary>
        private SessionStateItemExpireCallback m_sessionEndCallback;

        #endregion
    }
}