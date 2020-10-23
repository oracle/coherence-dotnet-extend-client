/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;

using Tangosol.Net.Cache;
using Tangosol.Net.Internal;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl.CacheService;
using Tangosol.Net.Messaging.Impl.NamedCache;
using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// <see cref="ICacheService"/> implementation that allows a client to
    /// use a remote CacheService without having to join the Cluster.
    /// </summary>
    /// <author>Ana Cikic  2006.09.15</author>
    public class RemoteCacheService : RemoteService, ICacheService
    {
        #region Properties

        /// <summary>
        /// A collection of string objects, one for each cache name that has
        /// been previously registered with this ICacheService.
        /// </summary>
        /// <value>
        /// <b>ICollection</b> of cache names.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If the service is not running or has stopped.
        /// </exception>
        public virtual ICollection CacheNames
        {
            get
            {
                return new List<string>(StoreRemoteNamedCache.GetNames());
            }
        }

        /// <summary>
        /// Whether a key should be checked for <see cref="IKeyAssociation"/>
        /// by the extend client (false) or deferred until the key is
        /// received by the PartionedService (true).
        /// </summary>
        /// <value>
        /// Whether a key should be checked for <b>IKeyAssociation</b>
        /// by the extend client (false) or deferred until the key is
        /// received by the PartionedService (true).
        /// </value>
        public virtual Boolean DeferKeyAssociationCheck { get; set; }

        /// <summary>
        /// Store that holds cache references by name and optionally,
        /// if configured, Principal.
        /// </summary>
        /// <value>
        /// Store that holds cache references by name and optionally,
        /// if configured, Principal.
        /// </value>
        protected virtual ScopedReferenceStore StoreRemoteNamedCache
        {
            get
            {
                var storeRemoteNamedCache = m_storeRemoteNamedCache;
                if (storeRemoteNamedCache == null)
                {
                    m_storeRemoteNamedCache
                            = storeRemoteNamedCache
                            = new ScopedReferenceStore(OperationalContext);
                }
                return storeRemoteNamedCache;
            }
            set
            {
                if (m_storeRemoteNamedCache != null)
                {
                    throw new InvalidOperationException(
                        "store remote named cache already set");
                }
                m_storeRemoteNamedCache = value;
            }
        }

        /// <summary>
        /// Gets the type of the <see cref="IService"/>.
        /// </summary>
        /// <value>
        /// The type of the <b>IService</b>.
        /// </value>
        public override ServiceType ServiceType
        {
            get { return Net.ServiceType.RemoteCache; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RemoteCacheService()
        {
            ServiceVersion = "3.2";
        }

        #endregion

        #region ICacheService implementation

        /// <summary>
        /// Release and destroy the specified cache.
        /// </summary>
        /// <remarks>
        /// <b>Warning:</b> This method is used to completely destroy the
        /// specified cache across the cluster. All references in the entire
        /// cluster to this cache will be invalidated, the cached data will
        /// be cleared, and all resources will be released.
        /// </remarks>
        /// <param name="cache">
        /// The cache object to be released.
        /// </param>
        public virtual void DestroyCache(INamedCache cache)
        {
            if (!(cache is RemoteNamedCache))
            {
                throw new ArgumentException("illegal cache: " + cache);
            }

            ScopedReferenceStore storeCache  = StoreRemoteNamedCache;

            lock(storeCache)
            {
                storeCache.ReleaseCache(cache);
            }
            DestroyRemoteNamedCache(cache as RemoteNamedCache);
        }

        /// <summary>
        /// Obtain an <see cref="INamedCache"/> interface that provides a view
        /// of resources shared among members of a cluster.
        /// </summary>
        /// <remarks>
        /// The view is identified by name within this ICacheService.
        /// Typically, repeated calls to this method with the same view name
        /// will result in the same view reference being returned.
        /// </remarks>
        /// <param name="name">
        /// The name, within this ICacheService, that uniquely identifies a
        /// view; <c>null</c> is legal, and may imply a default name.
        /// </param>
        /// <returns>
        /// An <b>INamedCache</b> interface which can be used to access the
        /// resources of the specified view.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the service is not running.
        /// </exception>
        public virtual INamedCache EnsureCache(string name)
        {
            if (StringUtils.IsNullOrEmpty(name))
            {
                name = "Default";
            }

            ScopedReferenceStore storeCache = StoreRemoteNamedCache;
            RemoteNamedCache     cache      = storeCache.GetCache(name)
                                              as RemoteNamedCache;

            if (cache == null || !cache.IsActive)
            {
                lock(storeCache)
                {
                    cache = storeCache.GetCache(name) as RemoteNamedCache;
                    if (cache == null || !cache.IsActive)
                    {
                        cache = CreateRemoteNamedCache(name);
                        storeCache.PutCache(cache);
                    }
                }
            }

            return cache;
        }

        /// <summary>
        /// Release local resources associated with the specified instance of
        /// the cache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This invalidates a reference obtained by using the
        /// <see cref="ICacheService.EnsureCache"/> method.</p>
        /// <p>
        /// Releasing a reference to a cache makes the cache reference no
        /// longer usable, but does not affect the cache itself. In other
        /// words, all other references to the cache will still be valid, and
        /// the cache data is not affected by releasing the reference.</p>
        /// <p>
        /// The reference that is released using this method can no longer be
        /// used; any attempt to use the reference will result in an
        /// exception.</p>
        /// </remarks>
        /// <param name="cache">
        /// The cache object to be released.
        /// </param>
        public virtual void ReleaseCache(INamedCache cache)
        {
            if (!(cache is RemoteNamedCache))
            {
                throw new ArgumentException("illegal cache: " + cache);
            }

            ScopedReferenceStore storeCache  = StoreRemoteNamedCache;

            lock(storeCache)
            {
                storeCache.ReleaseCache(cache);
            }
            ReleaseRemoteNamedCache(cache as RemoteNamedCache);
        }

        /// <summary>
        /// Releases all the caches fetched from the store and then clears the store.
        /// </summary>
        public virtual void ReleaseCaches()
        {
            ScopedReferenceStore storeCache  = StoreRemoteNamedCache;

            lock (storeCache)
            {
                for (IEnumerator iter = storeCache.GetAllCaches().GetEnumerator(); iter.MoveNext(); )
                {
                    RemoteNamedCache cache = (RemoteNamedCache) iter.Current;
                    ReleaseRemoteNamedCache(cache as RemoteNamedCache);
                }
            storeCache.Clear();
            }
        }
        
        #endregion

        #region RemoteService override methods

        /// <summary>
        /// Called immediately after the RemoteService is shutdown.
        /// </summary>
        protected override void DoShutdown()
        {
            base.DoShutdown();

            var storeCache = StoreRemoteNamedCache;

            lock (storeCache)
            {
                storeCache.Clear();
            }
        }

        /// <summary>
        /// Open an <b>IChannel</b> to the remote Service proxy.
        /// </summary>
        /// <seealso cref="RemoteService.OpenChannel"/>
        protected override IChannel OpenChannel()
        {
            LookupProxyServiceAddress();

            IConnection connection = Initiator.EnsureConnection();
            return connection.OpenChannel(CacheServiceProtocol.Instance,
                                          "CacheServiceProxy",
                                          null,
                                          Thread.CurrentPrincipal);
        }

        /// <summary>
        /// Invoked after an <see cref="IConnection"/> is closed.
        /// </summary>
        /// <param name="sender">
        /// <see cref="IConnectionManager"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// The <see cref="ConnectionEventType.Closed"/> event.
        /// </param>
        public override void OnConnectionClosed(object sender, ConnectionEventArgs evt)
        {
            ReleaseCaches();

            base.OnConnectionClosed(sender, evt);
        }

        /// <summary>
        /// Invoked when the <see cref="IConnection"/> detects that the
        /// underlying communication channel has been severed or become
        /// unusable.
        /// </summary>
        /// <remarks>
        /// After this event is raised, any attempt to use the
        /// <b>IConnection</b> (or any <see cref="IChannel"/> created by the
        /// <b>IConnection</b>) may result in an exception.
        /// </remarks>
        /// <param name="sender">
        /// <see cref="IConnectionManager"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// The <see cref="ConnectionEventType.Error"/> event.
        /// </param>
        public override void OnConnectionError(object sender, ConnectionEventArgs evt)
        {
            ReleaseCaches();

            base.OnConnectionError(sender, evt);
        }

        /// <summary>
        /// The <see cref="RemoteService.Configure"/> implementation method.
        /// </summary>
        /// <remarks>
        /// This method must only be called by a thread that has synchronized
        /// on this RemoteService.
        /// </remarks>
        /// <param name="xml">
        /// The <see cref="IXmlElement"/> containing the new configuration
        /// for this RemoteService.
        /// </param>
        protected override void DoConfigure(IXmlElement xml)
        {
            base.DoConfigure(xml);

            DeferKeyAssociationCheck =
                    xml.GetSafeElement("defer-key-association-check")
                    .GetBoolean(DeferKeyAssociationCheck);

            // register all Protocols
            IConnectionInitiator initiator = Initiator;
            initiator.RegisterProtocol(CacheServiceProtocol.Instance);
            initiator.RegisterProtocol(NamedCacheProtocol.Instance);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Create a new <see cref="RemoteNamedCache"/> for the given
        /// <see cref="INamedCache"/> name.
        /// </summary>
        /// <param name="name">
        /// The name of the cache.
        /// </param>
        /// <returns>
        /// A new <b>RemoteNamedCache</b>.
        /// </returns>
        public virtual RemoteNamedCache CreateRemoteNamedCache(string name)
        {
            IChannel           channel    = EnsureChannel();
            IConnection        connection = channel.Connection;
            IMessageFactory    factory    = channel.MessageFactory;
            RemoteNamedCache   cache      = new RemoteNamedCache();
            IPrincipal         principal  = Thread.CurrentPrincipal;
            EnsureCacheRequest request    = (EnsureCacheRequest) factory.CreateMessage(EnsureCacheRequest.TYPE_ID);

            request.CacheName = name;
            Uri uri;

            try
            {
                uri = new Uri((string) channel.Request(request));
            }
            catch (UriFormatException e)
            {
                throw new Exception("error instantiating URI", e);
            }

            cache.CacheName                = name;
            cache.CacheService             = this;
            cache.DeferKeyAssociationCheck = DeferKeyAssociationCheck;
            cache.EventDispatcher          = EnsureEventDispatcher();

            connection.AcceptChannel(uri, cache, principal);

            return cache;
        }

        /// <summary>
        /// Destroy the given <see cref="RemoteNamedCache"/>.
        /// </summary>
        /// <param name="cache">
        /// The <b>RemoteNamedCache</b> to destroy.
        /// </param>
        protected virtual void DestroyRemoteNamedCache(RemoteNamedCache cache)
        {
            ReleaseRemoteNamedCache(cache);

            IChannel            channel = EnsureChannel();
            IMessageFactory     factory = channel.MessageFactory;
            DestroyCacheRequest request = (DestroyCacheRequest) factory.CreateMessage(DestroyCacheRequest.TYPE_ID);

            request.CacheName = cache.CacheName;
            channel.Request(request);
        }

        /// <summary>
        /// Release the given <see cref="RemoteNamedCache"/>.
        /// </summary>
        /// <param name="cache">
        /// The <b>RemoteNamedCache</b> to release.
        /// </param>
        protected virtual void ReleaseRemoteNamedCache(RemoteNamedCache cache)
        {
            try
            {
                // when this is called due to certain connection error, e.g. ping
                // timeout, the channel could be null and closed.
                IChannel channel = cache.Channel;
                if (channel != null)
                {
                    channel.Close();
                }
            }
            catch (Exception)
            {}
        }

        #endregion

        #region Data members

        /// <summary>
        /// Store that holds cache references by name and optionally,
        /// if configured, Principal.
        /// </summary>
        [NonSerialized]
        private ScopedReferenceStore m_storeRemoteNamedCache;

        #endregion
    }
}