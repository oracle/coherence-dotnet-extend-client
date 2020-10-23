/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

namespace Tangosol.Net.Cache
{
    #region IConfigurableCache interface

    /// <summary>
    /// An extension to the <see cref="ICache"/> interface that supports 
    /// runtime configuration and monitoring of various caching properties.
    /// </summary>
    /// <author>Cameron Purdy  2009.01.13</author>
    /// <author>Aleksandar Seovic  2009.07.27</author>
    /// <since>Coherence 3.5.1</since>
    public interface IConfigurableCache : ICache
    {
        #region Size limiting support

        /// <summary>
        /// Determine the number of units that the cache currently stores.
        /// </summary>
        /// <value>
        /// The current size of the cache in units.
        /// </value>
        long Units { get; }

        /// <summary>
        /// Get or set the limit of the cache size in units. The cache will prune
        /// itself automatically once it reaches its maximum unit level. This is
        /// often referred to as the "high water mark" of the cache.
        /// </summary>
        /// <value>
        /// The limit of the cache size in units.
        /// </value>
        long HighUnits { get; set; }

        /// <summary>
        /// Get or set the point to which the cache will shrink when it prunes.
        /// This is often referred to as a "low water mark" of the cache. If the
        /// cache incrementally prunes, then this setting will have no effect.
        /// </summary>
        /// <value>
        /// The number of units that the cache prunes to.
        /// </value>
        long LowUnits { get; set; }

        #endregion

        #region Eviction support
        
        /// <summary>
        /// Evict a specified key from the cache, as if it had expired from the
        /// cache. If the key is not in the cache, then the method has no effect.
        /// </summary>
        /// <param name="oKey">
        /// The key to evict from the cache.
        /// </param>
        void Evict(Object oKey);

        /// <summary>
        /// Evict the specified keys from the cache, as if they had each expired
        /// from the cache.
        /// </summary>
        /// <remarks>
        /// The result of this method is defined to be semantically the same as
        /// the following implementation:
        ///
        /// <code>
        /// foreach (Object oKey in colKeys)
        /// {
        ///     Evict(oKey);
        /// }
        /// </code>
        /// </remarks>
        /// <param name="colKeys">
        /// A collection of keys to evict from the cache.
        /// </param>
        void EvictAll(ICollection colKeys);

        /// <summary>
        /// Evict all entries from the cache that are no longer valid, and
        /// potentially prune the cache size if the cache is size-limited
        /// and its size is above the caching low water mark.
        /// </summary>
        void Evict();

        #endregion
        
        #region Expiry support 

        /// <summary>
        /// Get or set the default "time to live" for each individual cache entry.
        /// </summary>
        /// <remarks>
        /// Change of this property does not affect the already-scheduled expiry 
        /// of existing entries.
        /// </remarks>
        /// <value>
        /// The number of milliseconds that a cache entry value will live,
        /// or zero if cache entries are never automatically expired.
        /// </value>
        int ExpiryDelay { get; set; }

        /// <summary>
        /// Get or set the delay between cache flushes. A cache flush evicts 
        /// entries that have expired.
        /// </summary>
        /// <remarks>
        /// This value is used by cache implementations that periodically evict
        /// entries that have expired; this value has no meaning for cache
        /// implementations that aggressively evict entries as they expire.
        /// </remarks>
        /// <value>
        /// The number of milliseconds between cache flushes, or zero which
        /// signifies that the cache never flushes.
        /// </value>
        int FlushDelay { get; set; }

        #endregion

        /// <summary>
        /// Locate a cache entry in the cache based on its key.
        /// </summary>
        /// <param name="key">
        /// The key object to search for.
        /// </param>
        /// <returns>
        /// The entry or null.
        /// </returns>
        IConfigurableCacheEntry GetCacheEntry(Object key);

        /// <summary>
        /// Get or set the eviction policy used by the cache.
        /// </summary>
        /// <value>
        /// The eviction policy used by the cache.
        /// </value>
        IEvictionPolicy EvictionPolicy { get; }

        /// <summary>
        /// Get or set the unit calculator used by the cache.
        /// </summary>
        /// <value>
        /// The unit calculator used by the cache.
        /// </value>
        IUnitCalculator UnitCalculator { get; set; }
    }

    #endregion

    #region IConfigurableCacheEntry interface

    /// <summary>
    /// A cache Entry carries information additional to the base Map Entry in
    /// order to support eviction and expiry.
    /// </summary>
    /// <author>Cameron Purdy  2009.01.13</author>
    /// <author>Aleksandar Seovic  2009.07.27</author>
    /// <since>Coherence 3.5.1</since>
    public interface IConfigurableCacheEntry : ICacheEntry
    {
        /// <summary>
        /// Indicate to the entry that it has been touched, such as when it is
        /// accessed or modified.
        /// </summary>
        void Touch();

        /// <summary>
        /// Determine the number of times that the cache entry has been
        /// touched (since the touch count was last reset).
        /// </summary>
        /// <value>
        /// The number of times that the cache entry has been touched.
        /// </value>
        int TouchCount { get; }

        /// <summary>
        /// Determine when the cache entry was last touched.
        /// </summary>
        /// <value>
        /// The date/time value, in millis, when the entry was most
        /// recently touched.
        /// </value>
        long LastTouchMillis { get; }

        /// <summary>
        /// Get or set when the cache entry will expire, if ever.
        /// </summary>
        /// <remarks>
        /// If the cache is configured for automatic expiry, each subsequent 
        /// update to this cache entry will reschedule the expiry time.
        /// </remarks>
        /// <value>
        /// The date/time value, in millis, when the entry will (or did) 
        /// expire; zero indicates no expiry.
        /// </value>
        long ExpiryMillis { get; set; }

        /// <summary>
        /// Determine if this entry has expired.
        /// </summary>
        /// <value>
        /// <c>true</c> if this entry has already expired, 
        /// <c>false</c> otherwise.
        /// </value>
        bool IsExpired { get; }

        /// <summary>
        /// Get or set the number of cache units used by this entry.
        /// </summary>
        /// <value>
        /// An integer value 0 or greater, with a larger value
        /// signifying a higher cost; -1 implies that the entry
        /// has been discarded
        /// </value>
        int Units { get; set; }
    }

    #endregion
}
