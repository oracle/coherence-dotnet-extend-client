/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Threading;

using Tangosol.Net.Internal;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// "Safe" wrapper for <see cref="RemoteCacheService"/>.
    /// </summary>
    public class SafeCacheService : SafeService, ICacheService
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
            get { return RunningCacheService.CacheNames; }
        }

        /// <summary>
        /// Calculated property that returns the running wrapped
        /// <b>ICacheService</b>.
        /// </summary>
        /// <value>
        /// The wrapped <b>ICacheService</b>.
        /// </value>
        public virtual ICacheService RunningCacheService
        {
            get { return EnsureRunningCacheService(true); }
        }

        /// <summary>
        /// The actual (wrapped) <see cref="ICacheService"/>.
        /// </summary>
        /// <value>
        /// <b>ICacheService</b> object.
        /// </value>
        public virtual ICacheService CacheService
        {
            get { return (ICacheService) Service; }
        }

        /// <summary>
        /// Store that holds cache references by name and optionally,
        /// if configured, Principal.
        /// </summary>
        /// <value>
        /// Store that holds cache references by name and optionally,
        /// if configured, Principal.
        /// </value>
        public virtual ScopedReferenceStore StoreSafeNamedCache
        {
            get
            {
                var storeSafeNamedCache = m_storeSafeNamedCache;
                if (storeSafeNamedCache == null)
                {
                    m_storeSafeNamedCache
                            = storeSafeNamedCache
                            = new ScopedReferenceStore(OperationalContext);
                }
                return storeSafeNamedCache;
            }
            set
            {
                if (m_storeSafeNamedCache != null)
                {
                    throw new InvalidOperationException(
                        "store safe named cache already set");
                }
                m_storeSafeNamedCache = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SafeCacheService()
        {
            SafeServiceState = ServiceState.Initial;
        }

        #endregion

        #region ICacheService implementation

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
        public INamedCache EnsureCache(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "Default";
            }

            ScopedReferenceStore storeCache = StoreSafeNamedCache;
            SafeNamedCache       cacheSafe  = (SafeNamedCache)storeCache.GetCache(name);
            if (cacheSafe == null)
            {
                lock(storeCache)
                {
                    INamedCache cache = RunningCacheService.EnsureCache(name);

                    cacheSafe = new SafeNamedCache
                                    {
                                        SafeCacheService = this,
                                        CacheName        = name,
                                        NamedCache       = cache,
                                        Principal        = Thread.CurrentPrincipal
                                    };

                    storeCache.PutCache(cacheSafe);
                }
            }

            return cacheSafe;
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
        /// Releasing a map reference to a cache makes the map reference no
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
            SafeNamedCache cacheSafe = (SafeNamedCache) cache;

            RemoveCacheReference(cacheSafe);

            ICacheService service = CacheService;
            try
            {
                INamedCache cacheWrapped = cacheSafe.NamedCache;
                if (cacheWrapped != null)
                {
                    service.ReleaseCache(cacheWrapped);
                }
            }
            catch (Exception)
            {
                if (service != null && service.IsRunning)
                {
                    throw;
                }
            }
        }

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
            SafeNamedCache cacheSafe = (SafeNamedCache) cache;

            RemoveCacheReference(cacheSafe);

            ICacheService service = CacheService;
            try
            {
                INamedCache cacheWrapped = cacheSafe.NamedCache;
                if (cacheWrapped == null)
                {
                    throw new InvalidOperationException("Cache is already released");
                }
                else
                {
                    service.DestroyCache(cacheWrapped);
                }
            }
            catch (Exception)
            {
                if (service != null && service.IsRunning)
                {
                    throw;
                }
            }
        }

        #endregion

        #region SafeService override methods

        /// <summary>
        /// Cleanup used resources.
        /// </summary>
        protected override void Cleanup()
        {
            base.Cleanup();

            ScopedReferenceStore storeCache = StoreSafeNamedCache;

            lock (storeCache)
            {
                storeCache.Clear();
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Return the wrapped <b>ICacheService</b>.
        /// </summary>
        /// <remarks>
        /// This method ensures that the returned <b>ICacheService</b> is
        /// running before returning it. If the <b>ICacheService</b> is not
        /// running and has not been explicitly stopped, the
        /// <b>ICacheService</b> is restarted.
        /// </remarks>
        /// <param name="drain">
        /// If true and the wrapped <b>ICacheService</b> is restarted, the
        /// calling thread will be blocked until the wrapped
        /// <b>ICacheService</b> event dispatcher queue is empty and all
        /// outstanding tasks have been executed.
        /// </param>
        /// <returns>
        /// The running wrapped <b>ICacheService</b>.
        /// </returns>
        public virtual ICacheService EnsureRunningCacheService(bool drain)
        {
            return (ICacheService) EnsureRunningService(drain);
        }

        /// <summary>
        /// Removes <see cref="SafeNamedCache"/> from the
        /// <see cref="ScopedReferenceStore"/>.
        /// </summary>
        /// <param name="cacheSafe">
        /// <b>SafeNamedCache</b> to be removed.
        /// </param>
        protected virtual void RemoveCacheReference(SafeNamedCache cacheSafe)
        {
            cacheSafe.IsReleased = true;

            ScopedReferenceStore storeCache = StoreSafeNamedCache;

            lock(storeCache)
            {
                storeCache.ReleaseCache(cacheSafe);
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// Store that holds cache references by name and optionally,
        /// if configured, Subject.
        /// </summary>
        [NonSerialized]
        private ScopedReferenceStore m_storeSafeNamedCache;

        #endregion
    }
}