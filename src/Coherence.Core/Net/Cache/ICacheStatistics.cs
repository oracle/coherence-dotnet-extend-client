/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Cache
{
    /// <summary>
    /// An interface for exposing <see cref="ICache"/> statistics.
    /// </summary>
    /// <since>Coherence 2.2</since>
    /// <author>Cameron Purdy  2003.05.26</author>
    /// <author>Goran Milosavljevic  2006.11.09</author>
    public interface ICacheStatistics
    {
        /// <summary>
        /// Determine the total number of "get" operations since the cache
        /// statistics were last reset.
        /// </summary>
        /// <value>
        /// The total number of "get" operations.
        /// </value>
        long TotalGets { get; }

        /// <summary>
        /// Determine the total number of milliseconds spent on "get"
        /// operations since the cache statistics were last reset.
        /// </summary>
        /// <value>
        /// The total number of milliseconds processing "get" operations.
        /// </value>
        long TotalGetsMillis { get; }

        /// <summary>
        /// Determine the average number of milliseconds per "get"
        /// invocation since the cache statistics were last reset.
        /// </summary>
        /// <value>
        /// The average number of milliseconds per "get" operation.
        /// </value>
        double AverageGetMillis { get; }

        /// <summary>
        /// Determine the total number of "put" operations since the cache
        /// statistics were last reset.
        /// </summary>
        /// <value>
        /// The total number of "put" operations.
        /// </value>
        long TotalPuts { get; }

        /// <summary>
        /// Determine the total number of milliseconds spent on "put"
        /// operations since the cache statistics were last reset.
        /// </summary>
        /// <value>
        /// The total number of milliseconds processing "put" operations.
        /// </value>
        long TotalPutsMillis { get; }

        /// <summary>
        /// Determine the average number of milliseconds per "put"
        /// invocation since the cache statistics were last reset.
        /// </summary>
        /// <value>
        /// The average number of milliseconds per "put" operation.
        /// </value>
        double AveragePutMillis { get; }

        /// <summary>
        /// Determine the rough number of cache hits since the cache
        /// statistics were last reset.
        /// </summary>
        /// <remarks>
        /// A cache hit is a read operation invocation (i.e. "get") for which
        /// an entry exists in this cache.
        /// </remarks>
        /// <value>
        /// The number of "get" calls that have been served by
        /// existing cache entries.
        /// </value>
        long CacheHits { get; }

        /// <summary>
        /// Determine the total number of milliseconds (since that last
        /// statistics reset) for the "get" operations for which an entry
        /// existed in this cache.
        /// </summary>
        /// <value>
        /// The total number of milliseconds for the "get" operations that
        /// were hits.
        /// </value>
        long CacheHitsMillis { get; }

        /// <summary>
        /// Determine the average number of milliseconds per "get"
        /// invocation that is a hit.
        /// </summary>
        /// <value>
        /// The average number of milliseconds per cache hit.
        /// </value>
        double AverageHitMillis { get; }

        /// <summary>
        /// Determine the rough number of cache misses since the cache
        /// statistics were last reset.
        /// </summary>
        /// <remarks>
        /// A cache miss is a "get" invocation that does not have an entry
        /// in this cache.
        /// </remarks>
        /// <value>
        /// The number of "get" calls that failed to find an existing
        /// cache entry because the requested key was not in the cache.
        /// </value>
        long CacheMisses { get; }

        /// <summary>
        /// Determine the total number of milliseconds (since that last
        /// statistics reset) for the "get" operations for which no entry
        /// existed in this cache.
        /// </summary>
        /// <value>
        /// The total number of milliseconds (since that last statistics
        /// reset) for the "get" operations that were misses.
        /// </value>
        long CacheMissesMillis { get; }

        /// <summary>
        /// Determine the average number of milliseconds per "get" invocation
        /// that is a miss.
        /// </summary>
        /// <value>
        /// The average number of milliseconds per cache miss.
        /// </value>
        double AverageMissMillis { get; }

        /// <summary>
        /// Determine the rough probability (0 &lt;= p &lt;= 1) that the next
        /// invocation will be a hit, based on the statistics collected since
        /// the last reset of the cache statistics.
        /// </summary>
        /// <value>
        /// The cache hit probability (0 &lt;= p &lt;= 1).
        /// </value>
        double HitProbability { get; }

        /// <summary>
        /// Determine the rough number of cache pruning cycles since the
        /// cache statistics were last reset.
        /// </summary>
        /// <remarks>
        /// For the LocalCache implementation, this refers to the number of
        /// times that the <tt>prune()</tt> method is executed.
        /// </remarks>
        /// <value>
        /// The total number of cache pruning cycles (since that last
        /// statistics reset).
        /// </value>
        long CachePrunes { get; }

        /// <summary>
        /// Determine the total number of milliseconds (since that last
        /// statistics reset) spent on cache pruning.
        /// </summary>
        /// <remarks>
        /// For the LocalCache implementation, this refers to the time spent in
        /// the <tt>prune()</tt> method.
        /// </remarks>
        /// <value>
        /// The total number of milliseconds (since that last statistics
        /// reset) for cache pruning operations.
        /// </value>
        long CachePrunesMillis { get; }

        /// <summary>
        /// Determine the average number of milliseconds per cache pruning.
        /// </summary>
        /// <value>
        /// The average number of milliseconds per cache pruning.
        /// </value>
        double AveragePruneMillis { get; }

        /// <summary>
        /// Reset the cache statistics.
        /// </summary>
        void ResetHitStatistics();
    }
}