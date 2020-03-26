/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using Tangosol.Net.Cache;

namespace Tangosol.Net
{
    /// <summary>
    /// An INamedCache is an <see cref="ICache"/> that adds lifecycle management,
    /// event support, concurrency control, the ability to query cache content,
    /// and entry-targeted processing and aggregating operations.
    /// </summary>
    /// <remarks>
    /// Cached resources are expected to be managed in memory, and are
    /// typically composed of data that are stored persistently in a
    /// database, or data that have been assembled or calculated at some
    /// significant cost, thus these resources are referred to as
    /// <i>cached</i>.
    /// </remarks>
    /// <author>Gene Gleyzer  2002.03.27</author>
    /// <author>Aleksandar Seovic  2006.07.11</author>
    public interface INamedCache : IObservableCache, IConcurrentCache, IQueryCache, IInvocableCache, IDisposable
    {
        /// <summary>
        /// Gets the cache name.
        /// </summary>
        /// <value>
        /// The cache name.
        /// </value>
        string CacheName { get; }

        /// <summary>
        /// Gets the <see cref="ICacheService"/> that this INamedCache is a
        /// part of.
        /// </summary>
        /// <value>
        /// The cache service this INamedCache is a part of.
        /// </value>
        ICacheService CacheService { get; }

        /// <summary>
        /// Specifies whether or not the INamedCache is active.
        /// </summary>
        /// <value>
        /// <b>true</b> if the INamedCache is active; <b>false</b> otherwise.
        /// </value>
        bool IsActive { get; }

        /// <summary>
        /// Release local resources associated with this instance of
        /// INamedCache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Releasing a cache makes it no longer usable, but does not affect
        /// the cache itself. In other words, all other references to the
        /// cache will still be valid, and the cache data is not affected by
        /// releasing the reference.
        /// Any attempt to use this reference afterword will result in an
        /// exception.</p>
        /// <p>
        /// Caches should be released by the same mechansim in which they were
        /// obtained. For example:
        /// <ul>
        ///  <li> new Cache() - cache.Release()</li>
        ///  <li> CacheFactory.GetCache() - CacheFactory.ReleaseCache()</li>
        ///  <li> ConfigurableCacheFactory.EnsureCache() - ConfigurableCacheFactory.ReleaseCache()</li>
        /// </ul>
        /// Except for the case where the application code expicitly allocated the
        /// cache, this method should not be called by application code.</p>
        /// 
        /// </remarks>
        void Release();

        /// <summary>
        /// Release and destroy this instance of INamedCache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// <b>Warning:</b> This method is used to completely destroy the
        /// specified cache across the cluster. All references in the entire
        /// cluster to this cache will be invalidated, the cached data will
        /// be cleared, and all resources will be released.</p>
        /// <p>
        /// Caches should be destroyed by the same mechansim in which they were
        /// obtained. For example:
        /// <ul>
        ///  <li> new Cache() - cache.Destroy()</li>
        ///  <li> CacheFactory.GetCache() - CacheFactory.DestroyCache()</li>
        ///  <li> ConfigurableCacheFactory.EnsureCache() - ConfigurableCacheFactory.DestroyCache()</li>
        /// </ul>
        /// Except for the case where the application code expicitly allocated the
        /// cache, this method should not be called by application code.</p>
        /// </remarks>
        void Destroy();

        /// <summary>
        /// Removes all mappings from this map.
        /// </summary>
        /// <remarks>
        /// Note: the removal of entries caused by this truncate operation will
        /// not be observable.
        /// </remarks>
        void Truncate();

        /// <summary>
        /// Construct a view of this INamedCache.
        /// </summary>
        /// <returns>A local view for this INamedCache</returns>
        /// <see cref="ViewBuilder"/>
        /// <since>12.2.1.4</since>
        ViewBuilder View();
    }
}