/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.SessionState;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Util;
using Tangosol.Web.Model;

namespace Tangosol.Web
{
    /// <summary>
    /// ASP.NET Session-State Store Provider implementation that uses a
    /// Coherence for .NET <see cref="INamedCache"/> to store HTTP session
    /// state.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This implementation allows you to have a truly clustered session-
    /// state store.
    /// </p>
    /// <p>
    /// The <b>CoherenceSessionStore</b> has the following conditions and
    /// features:
    /// <list type="bullet">
    /// <item>Objects stored within a <b>Session</b> must be serializable.</item>
    /// <item>The <b>Session_OnEnd</b> event <i>is</i> supported.</item>
    /// </list>
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic  2006.11.02</author>
    /// <author>Jason Howes  2007.03.29</author>
    public class CoherenceSessionStore : SessionStateStoreProviderBase
    {
        # region Properties

        /// <summary>
        /// The configured HTTP session timeout.
        /// </summary>
        public TimeSpan Timeout
        {
            get { return m_timeout; }
            set { m_timeout = value; }
        }

        /// <summary>
        /// The flag used to enable Session_OnEnd event and listener
        /// registration.
        /// </summary>
        public bool SessionEndEnabled
        {
            get { return m_sessionEndEnabled; }
            set { m_sessionEndEnabled = value; }
        }

        /// <summary>
        /// Allows users to specify application identifier explicitly, in order 
        /// to share session state across different ASP.NET applications.
        /// </summary>
        public string ApplicationId
        {
            get { return m_applicationId; }
            set { m_applicationId = value; }
        }

        /// <summary>
        /// The session model manager.
        /// </summary>
        public ISessionModelManager ModelManager
        {
            get { return m_modelManager; }
            set { m_modelManager = value; }
        }

        /// <summary>
        /// The name of the <b>INamedCache</b> used to store serialized HTTP 
        /// session data.
        /// </summary>
        public string CacheName
        {
            get { return m_cacheName; }
            set { m_cacheName = value; }
        }

        #endregion

        #region SessionStateStoreProviderBase implementation

        /// <summary>
        /// Initializes this session-state store provider by obtaining the
        /// <b>INamedCache</b> used to store serialized HTTP session state.
        /// </summary>
        /// <param name="name">
        /// The friendly name of the provider.
        /// </param>
        /// <param name="config">
        /// A collection of name/value pairs containing the provider-specific
        /// attributes specified in the configuration for this provider.
        /// </param>
        public override void Initialize(string name, NameValueCollection config)
        {
            //
            // initialize the abstract base class
            //

            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (name == null || name.Length == 0)
            {
                name = "CoherenceSessionStore";
            }

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Coherence for .NET Session-State Store Provider");
            }

            base.Initialize(name, config);

            //
            // initialize the CacheName property
            //
            CacheName = config["cacheName"];

            //
            // initialize the Timeout property
            //
            Configuration configWeb = WebConfigurationManager.OpenWebConfiguration(
                    HostingEnvironment.ApplicationVirtualPath);
            SessionStateSection section = (SessionStateSection)
                    configWeb.GetSection("system.web/sessionState");
            Timeout = section.Timeout;

            //
            // initialize the SessionEndEnabled property
            //
            SessionEndEnabled = Convert.ToBoolean(config["sessionEndEnabled"]);

            //
            // initialize the ApplicationId property
            //
            ApplicationId = String.IsNullOrEmpty(config["applicationId"]) 
                            ? HostingEnvironment.ApplicationID 
                            : config["applicationId"];

            //
            // determine serializer to use
            //
            ISerializer serializer;
            string      serializerType = config["serializer"];
            if (String.IsNullOrEmpty(serializerType))
            {
                serializerType = "binary";
            }
            switch (serializerType.ToLowerInvariant())
            {
                case "binary":
                    serializer = new OptimizedBinarySerializer();
                    break;
                case "pof":
                    serializer = new ConfigurablePofContext();
                    break;
                default:
                    serializer = (ISerializer) ObjectUtils.CreateInstance(TypeResolver.Resolve(serializerType));
                    break;
            }

            //
            // initialize the ModelManager property
            //
            string model = config["model"];
            if (String.IsNullOrEmpty(model))
            {
                model = "traditional";
            }
            switch (model.ToLowerInvariant())
            {
                case "traditional":
                    ModelManager = new TraditionalSessionModelManager(serializer, CacheName);
                    break;
                case "monolithic":
                    ModelManager = new MonolithicSessionModelManager(serializer, CacheName);
                    break;
                case "split":
                    int minExtAttrSize = Convert.ToInt32(config["externalAttributeSize"]);
                    ModelManager = new SplitSessionModelManager(serializer, CacheName, minExtAttrSize);
                    break;
                default:
                    ModelManager = (ISessionModelManager) ObjectUtils.CreateInstance(TypeResolver.Resolve(model), serializer, CacheName);
                    break;
            }
        }

        /// <summary>
        /// Releases all resources used by this session-state store provider.
        /// </summary>
        public override void Dispose()
        {
        }

        /// <summary>
        /// Configure the <b>Web.SessionState.SessionStateItemExpireCallback</b>
        /// delegate for the <b>Session_OnEnd</b> event defined in the
        /// Global.asax file.
        /// </summary>
        /// <param name="expireCallback">
        /// The <b>Web.SessionState.SessionStateItemExpireCallback</b>
        /// delegate for the <b>Session_OnEnd</b> event defined in the
        /// Global.asax file.
        /// </param>
        /// <returns>
        /// <b>true</b> if the session-state store provider supports calling
        /// the <b>Session_OnEnd</b> event; otherwise, <b>false</b>.
        /// </returns>
        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            if (SessionEndEnabled)
            {
                ModelManager.SessionEndCallback = expireCallback;
            }

            return SessionEndEnabled;
        }

        /// <summary>
        /// Returns read-only HTTP session data from the session data store.
        /// </summary>
        /// <param name="context">
        /// The <b>Web.HttpContext</b> for the current request.
        /// </param>
        /// <param name="id">
        /// The <b>Web.SessionState.HttpSessionState.SessionID</b> for the
        /// current request.
        /// </param>
        /// <param name="locked">
        /// When this method returns, contains a Boolean value that is set to
        /// <b>true</b> if the requested HTTP session item is locked at the
        /// session data store; otherwise, <b>false</b>.
        /// </param>
        /// <param name="lockAge">
        /// When this method returns, contains a <b>TimeSpan</b> object that
        /// is set to the amount of time that an item in the session data
        /// store has been locked.
        /// </param>
        /// <param name="lockId">
        /// When this method returns, contains an object that is set to the
        /// lock identifier for the current request. For details on the lock
        /// identifier, see "Locking Session-Store Data" in the
        /// <see cref="T:System.Web.SessionState.SessionStateStoreProviderBase"/>
        /// class summary.
        /// </param>
        /// <param name="actions">
        /// When this method returns, contains one of the
        /// <b>Web.SessionState.SessionStateActions</b> values, indicating
        /// whether the current HTTP session is an uninitialized, cookieless
        /// HTTP session.
        /// </param>
        /// <returns>
        /// A <b>Web.SessionState.SessionStateStoreData</b> populated with HTTP
        /// session values and information from the session data store.
        /// </returns>
        public override SessionStateStoreData GetItem(HttpContext context,
                string id, out bool locked, out TimeSpan lockAge,
                out object lockId, out SessionStateActions actions)
        {
            return GetSessionStateItem(false, context, id, out locked, 
                                       out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// Returns writeable HTTP session data from the session data store.
        /// </summary>
        /// <param name="context">
        /// The <b>Web.HttpContext</b> for the current request.
        /// </param>
        /// <param name="id">
        /// The <b>Web.SessionState.HttpSessionState.SessionID</b> for the
        /// current request.
        /// </param>
        /// <param name="locked">
        /// When this method returns, contains a Boolean value that is set to
        /// <b>true</b> if the requested HTTP session item is locked at the
        /// session data store; otherwise, <b>false</b>.
        /// </param>
        /// <param name="lockAge">
        /// When this method returns, contains a <b>TimeSpan</b> object that
        /// is set to the amount of time that an item in the session data
        /// store has been locked.
        /// </param>
        /// <param name="lockId">
        /// When this method returns, contains an object that is set to the
        /// lock identifier for the current request. For details on the lock
        /// identifier, see "Locking Session-Store Data" in the
        /// <see cref="T:System.Web.SessionState.SessionStateStoreProviderBase"/>
        /// class summary.
        /// </param>
        /// <param name="actions">
        /// When this method returns, contains one of the
        /// <b>Web.SessionState.SessionStateActions</b> values, indicating
        /// whether the current HTTP session is an uninitialized, cookieless
        /// HTTP session.
        /// </param>
        /// <returns>
        /// A <b>Web.SessionState.SessionStateStoreData</b> populated with HTTP
        /// session values and information from the session data store.
        /// </returns>
        public override SessionStateStoreData GetItemExclusive(HttpContext context,
                string id, out bool locked, out TimeSpan lockAge, out object lockId,
                out SessionStateActions actions)
        {
            return GetSessionStateItem(true, context, id, out locked, 
                                       out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// Releases a lock on an item in the session data store.
        /// </summary>
        /// <param name="context">
        /// The <b>Web.HttpContext</b> for the current request.
        /// </param>
        /// <param name="id">
        /// The HTTP session identifier for the current request.
        /// </param>
        /// <param name="lockId">
        /// The lock identifier for the current request.
        /// </param>
        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            ModelManager.ReleaseSession(GetSessionKey(id), GetLockId(lockId));
        }

        /// <summary>
        /// Updates the session item information in the session data store
        /// with values from the current request, and clears the lock
        /// on the data.
        /// </summary>
        /// <param name="context">
        /// The <b>Web.HttpContext</b> for the current request.
        /// </param>
        /// <param name="id">
        /// The HTTP session identifier for the current request.
        /// </param>
        /// <param name="item">
        /// The <b>Web.SessionState.SessionStateStoreData</b> object that
        /// contains the current HTTP session values to be stored.
        /// </param>
        /// <param name="lockId">
        /// The lock identifier for the current request.
        /// </param>
        /// <param name="newItem">
        /// <b>true</b> to identify the session item as a new item;
        /// <b>false</b> to identify the session item as an existing item.
        /// </param>
        public override void SetAndReleaseItemExclusive(HttpContext context,
                string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            // timeouts shorter than one minute are used for unit testing
            TimeSpan timeout = item.Timeout > 0 
                               ? TimeSpan.FromMinutes(item.Timeout)
                               : Timeout;

            SessionHolder holder = new SessionHolder((ISessionModel) item.Items, GetLockId(lockId), true, timeout);
            ModelManager.SaveSession(GetSessionKey(id), holder, newItem);
        }

        /// <summary>
        /// Deletes item data from the session data store.
        /// </summary>
        /// <param name="context">
        /// The <b>Web.HttpContext</b> for the current request.
        /// </param>
        /// <param name="id">
        /// The HTTP session identifier for the current request.
        /// </param>
        /// <param name="lockId">
        /// The lock identifier for the current request.
        /// </param>
        /// <param name="item">
        /// The <b>Web.SessionState.SessionStateStoreData</b> that represents
        /// the item to delete from the data store.
        /// </param>
        public override void RemoveItem(HttpContext context, string id,
                object lockId, SessionStateStoreData item)
        {
            ModelManager.RemoveSession(GetSessionKey(id));
        }

        /// <summary>
        /// Updates the expiration date and time of an item in the session
        /// data store.
        /// </summary>
        /// <param name="context">
        /// The <b>Web.HttpContext</b> for the current request.
        /// </param>
        /// <param name="id">
        /// The HTTP session identifier for the current request.
        /// </param>
        public override void ResetItemTimeout(HttpContext context, string id)
        {
            ModelManager.ResetSessionTimeout(GetSessionKey(id));
        }

        /// <summary>
        /// Creates a new <b>Web.SessionState.SessionStateStoreData</b>
        /// object to be used for the current request.
        /// </summary>
        /// <param name="context">
        /// The <b>Web.HttpContext</b> for the current request.
        /// </param>
        /// <param name="timeout">
        /// The session-state <b>Web.SessionState.HttpSessionState.Timeout</b>
        /// value for the new <b>Web.SessionState.SessionStateStoreData</b>.
        /// </param>
        /// <returns>
        /// A new <b>Web.SessionState.SessionStateStoreData</b> for the
        /// current request.
        /// </returns>
        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return CreateSessionStateStoreData(context, ModelManager.CreateSessionModel(), timeout);
        }

        /// <summary>
        /// Adds a new session-state item to the data store.
        /// </summary>
        /// <param name="context">
        /// The <b>Web.HttpContext</b> for the current request.
        /// </param>
        /// <param name="id">
        /// The <b>Web.SessionState.HttpSessionState.SessionID</b>
        /// for the current request.
        /// </param>
        /// <param name="timeout">
        /// The HTTP session <b>Web.SessionState.HttpSessionState.Timeout</b>
        /// for the current request.
        /// </param>
        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            SessionHolder holder = new SessionHolder(ModelManager.CreateSessionModel(), 0L, false, Timeout);
            ModelManager.SaveSession(GetSessionKey(id), holder, true);
        }

        /// <summary>
        /// Called by the <b>Web.SessionState.SessionStateModule</b>
        /// object for per-request initialization.
        /// </summary>
        /// <param name="context">
        /// The <b>Web.HttpContext</b> for the current request.
        /// </param>
        public override void InitializeRequest(HttpContext context)
        {}

        /// <summary>
        /// Called by the <b>Web.SessionState.SessionStateModule</b>
        /// object at the end of a request.
        /// </summary>
        /// <param name="context">
        /// The <b>Web.HttpContext</b> for the current request.
        /// </param>
        public override void EndRequest(HttpContext context)
        {}

        #endregion

        #region Helper methods

        /// <summary>
        /// Determine the key used to store the state of the HTTP session with
        /// the given identifier in the underlying <b>INamedCache</b>.
        /// </summary>
        /// <param name="sessionId">
        /// The HTTP session identifier. Cannot be null.
        /// </param>
        /// <returns>
        /// A unique key for the given HTTP session identifier, scoped by the
        /// web application.
        /// </returns>
        protected virtual SessionKey GetSessionKey(string sessionId)
        {
            return new SessionKey(ApplicationId, sessionId);
        }

        /// <summary>
        /// Converts lock identifier to long.
        /// </summary>
        /// <param name="lockId">Lock identifier to convert.</param>
        /// <returns>Lock identifier as long value.</returns>
        protected virtual long GetLockId(object lockId)
        {
            return lockId == null ? 0L : (long) lockId;
        }

        /// <summary>
        /// Acquire and return HTTP session data from the session data store.
        /// </summary>
        /// <param name="exclusive">
        /// Flag specifying whether to obtain exclusive access to the specified
        /// session.
        /// </param>
        /// <param name="context">
        /// The <b>Web.HttpContext</b> for the current request.
        /// </param>
        /// <param name="id">
        /// The <b>Web.SessionState.HttpSessionState.SessionID</b> for the
        /// current request.
        /// </param>
        /// <param name="locked">
        /// When this method returns, contains a Boolean value that is set to
        /// <b>true</b> if the requested HTTP session item is locked at the
        /// session data store; otherwise, <b>false</b>.
        /// </param>
        /// <param name="lockAge">
        /// When this method returns, contains a <b>TimeSpan</b> object that
        /// is set to the amount of time that an item in the session data
        /// store has been locked.
        /// </param>
        /// <param name="lockId">
        /// When this method returns, contains an object that is set to the
        /// lock identifier for the current request. For details on the lock
        /// identifier, see "Locking Session-Store Data" in the
        /// <see cref="T:System.Web.SessionState.SessionStateStoreProviderBase"/>
        /// class summary.
        /// </param>
        /// <param name="actions">
        /// When this method returns, contains one of the
        /// <b>Web.SessionState.SessionStateActions</b> values, indicating
        /// whether the current HTTP session is an uninitialized, cookieless
        /// HTTP session.
        /// </param>
        /// <returns>
        /// A <b>Web.SessionState.SessionStateStoreData</b> populated with HTTP
        /// session values and information from the session data store.
        /// </returns>
        protected virtual SessionStateStoreData GetSessionStateItem(bool exclusive, 
                HttpContext context, string id, out bool locked, out TimeSpan lockAge, 
                out object lockId, out SessionStateActions actions)
        {
            SessionKey    key    = GetSessionKey(id);
            SessionHolder holder = ModelManager.LoadSession(key, exclusive);
            if (holder == null)
            {
                locked  = false;
                lockAge = TimeSpan.Zero;
                lockId  = null;
                actions = SessionStateActions.None;

                return null;
            }
            else
            {
                locked  = holder.IsLocked;
                lockAge = holder.LockAge;
                lockId  = holder.LockId == 0 ? (object) null : holder.LockId;
                actions = holder.Initialized ? SessionStateActions.None : SessionStateActions.InitializeItem;

                return holder.Model == null 
                       ? null 
                       : CreateSessionStateStoreData(context, holder.Model, (int) holder.Timeout.TotalMinutes);
            }
        }

        /// <summary>
        /// Construct a <b>SessionStateStoreData</b> object from the serialized
        /// <b>ISessionStateItemCollection</b> stored in the underlying
        /// <b>INamedCache</b>.
        /// </summary>
        /// <param name="context">
        /// The <b>HttpContext</b> used to initialize the
        /// <b>SessionStateStoreData</b> object.
        /// </param>
        /// <param name="model">
        /// Session model.
        /// </param>
        /// <param name="timeout">
        /// Session timeout (in minutes).
        /// </param>
        /// <returns>
        /// A new <b>SessionStateStoreData</b> object.
        /// </returns>
        protected virtual SessionStateStoreData CreateSessionStateStoreData(
            HttpContext context, ISessionModel model, int timeout)
        {
            return new SessionStateStoreData(model, 
                                             context == null ? null : SessionStateUtility.GetSessionStaticObjects(context), 
                                             timeout);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The configured HTTP session timeout.
        /// </summary>
        private TimeSpan m_timeout;

        /// <summary>
        /// The flag used to enable Session_OnEnd event and listener
        /// registration.
        /// </summary>
        private bool m_sessionEndEnabled;

        /// <summary>
        /// Allows users to specify application identifier explicitly, 
        /// in order to share session state across different ASP.NET 
        /// applications.
        /// </summary>
        private string m_applicationId;

        /// <summary>
        /// The session model manager.
        /// </summary>
        private ISessionModelManager m_modelManager;

        /// <summary>
        /// The name of the <b>INamedCache</b> used to store serialized HTTP
        /// session data.
        /// </summary>
        private string m_cacheName;

        #endregion
    }
}