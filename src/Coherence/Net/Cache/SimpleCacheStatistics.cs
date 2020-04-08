/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Text;

using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// Implementation of the <see cref="ICacheStatistics"/> interface
    /// intended for use by a cache to maintain its statistics.
    /// </summary>
    /// <since>Coherence 2.2</since>
    /// <author>Cameron Purdy  2003.06.02</author>
    /// <author>Goran Milosavljevic  2006.11.09</author>
    public class SimpleCacheStatistics : ICacheStatistics
    {
        #region ICacheStatistics implementation

        /// <summary>
        /// Determine the total number of "get" operations since the cache
        /// statistics were last reset.
        /// </summary>
        /// <value>
        /// The total number of "get" operations.
        /// </value>
        public virtual long TotalGets
        {
            get { return m_cacheHits + m_cacheMisses; }
        }

        /// <summary>
        /// Determine the total number of milliseconds spent on "get"
        /// operations since the cache statistics were last reset.
        /// </summary>
        /// <value>
        /// The total number of milliseconds processing "get" operations.
        /// </value>
        public virtual long TotalGetsMillis
        {
            get { return m_hitsMillis + m_missesMillis; }
        }

        /// <summary>
        /// Determine the average number of milliseconds per "get"
        /// invocation since the cache statistics were last reset.
        /// </summary>
        /// <value>
        /// The average number of milliseconds per "get" operation.
        /// </value>
        public virtual double AverageGetMillis
        {
            get
            {
                double millis = m_hitsMillis + m_missesMillis;
                double gets   = m_cacheHits + m_cacheMisses;
                return gets == 0.0 ? 0.0 : millis / gets;
            }
        }

        /// <summary>
        /// Determine the total number of "put" operations since the cache
        /// statistics were last reset.
        /// </summary>
        /// <value>
        /// The total number of "put" operations.
        /// </value>
        public virtual long TotalPuts
        {
            get { return m_cachePuts; }
        }

        /// <summary>
        /// Determine the total number of milliseconds spent on "put"
        /// operations since the cache statistics were last reset.
        /// </summary>
        /// <value>
        /// The total number of milliseconds processing "put" operations.
        /// </value>
        public virtual long TotalPutsMillis
        {
            get { return m_putsMillis; }
        }

        /// <summary>
        /// Determine the average number of milliseconds per "put"
        /// invocation since the cache statistics were last reset.
        /// </summary>
        /// <value>
        /// The average number of milliseconds per "put" operation.
        /// </value>
        public virtual double AveragePutMillis
        {
            get
            {
                double millis = m_putsMillis;
                double puts   = m_cachePuts;
                return puts == 0.0 ? 0.0 : millis / puts;
            }
        }

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
        public virtual long CacheHits
        {
            get { return m_cacheHits; }
        }

        /// <summary>
        /// Determine the total number of milliseconds (since the last
        /// statistics reset) for the "get" operations for which an entry
        /// existed in this cache.
        /// </summary>
        /// <value>
        /// The total number of milliseconds for the "get" operations that
        /// were hits.
        /// </value>
        public virtual long CacheHitsMillis
        {
            get { return m_hitsMillis; }
        }

        /// <summary>
        /// Determine the average number of milliseconds per "get"
        /// invocation that is a hit.
        /// </summary>
        /// <value>
        /// The average number of milliseconds per cache hit.
        /// </value>
        public virtual double AverageHitMillis
        {
            get
            {
                double millis = m_hitsMillis;
                double gets   = m_cacheHits;
                return gets == 0.0 ? 0.0 : millis / gets;
            }
        }

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
        public virtual long CacheMisses
        {
            get { return m_cacheMisses; }
        }

        /// <summary>
        /// Determine the total number of milliseconds (since the last
        /// statistics reset) for the "get" operations for which no entry
        /// existed in this cache.
        /// </summary>
        /// <value>
        /// The total number of milliseconds (since the last statistics
        /// reset) for the "get" operations that were misses.
        /// </value>
        public virtual long CacheMissesMillis
        {
            get { return m_missesMillis; }
        }

        /// <summary>
        /// Determine the average number of milliseconds per "get" invocation
        /// that is a miss.
        /// </summary>
        /// <value>
        /// The average number of milliseconds per cache miss.
        /// </value>
        public virtual double AverageMissMillis
        {
            get
            {
                double millis = m_missesMillis;
                double gets   = m_cacheMisses;
                return gets == 0.0 ? 0.0 : millis / gets;
            }
        }

        /// <summary>
        /// Determine the rough probability (0 &lt;= p &lt;= 1) that the next
        /// invocation will be a hit, based on the statistics collected since
        /// the last reset of the cache statistics.
        /// </summary>
        /// <value>
        /// The cache hit probability (0 &lt;= p &lt;= 1).
        /// </value>
        virtual public double HitProbability
        {
            get
            {
                double hits  = m_cacheHits;
                double total = hits + m_cacheMisses;
                return total == 0.0 ? 0.0 : hits / total;
            }
        }

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
        public virtual long CachePrunes
        {
            get { return m_cachePrunes; }
        }

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
        public virtual long CachePrunesMillis
        {
            get { return m_prunesMillis; }
        }

        /// <summary>
        /// Determine the average number of milliseconds per cache pruning.
        /// </summary>
        /// <value>
        /// The average number of milliseconds per cache pruning.
        /// </value>
        public virtual double AveragePruneMillis
        {
            get
            {
                double millis = m_prunesMillis;
                double prunes = m_cachePrunes;
                return prunes == 0.0 ? 0.0 : millis / prunes;
            }
        }

        /// <summary>
        /// Reset the cache statistics.
        /// </summary>
        public virtual void ResetHitStatistics()
        {
            m_cacheHits    = 0L;
            m_cacheMisses  = 0L;
            m_hitsMillis   = 0L;
            m_missesMillis = 0L;
            m_putsMillis   = 0L;
            m_cachePuts    = 0L;
            m_cachePrunes  = 0L;
            m_prunesMillis = 0L;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// For debugging purposes, format the contents of the
        /// <b>SimpleCachingStatistics</b> in a human readable format.
        /// </summary>
        /// <returns>
        /// A String representation of this object.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("ICacheStatistics {TotalGets=")
                .Append(TotalGets)
                .Append(", TotalGetsMillis=")
                .Append(TotalGetsMillis)
                .Append(", AverageGetMillis=")
                .Append(AverageGetMillis)
                .Append(", TotalPuts=")
                .Append(TotalPuts)
                .Append(", TotalPutsMillis=")
                .Append(TotalPutsMillis)
                .Append(", AveragePutMillis=")
                .Append(AveragePutMillis)
                .Append(", CacheHits=")
                .Append(CacheHits)
                .Append(", CacheHitsMillis=")
                .Append(CacheHitsMillis)
                .Append(", AverageHitMillis=")
                .Append(AverageHitMillis)
                .Append(", CacheMisses=")
                .Append(CacheMisses)
                .Append(", CacheMissesMillis=")
                .Append(CacheMissesMillis)
                .Append(", AverageMissMillis=")
                .Append(AverageMissMillis)
                .Append(", HitProbability=")
                .Append(HitProbability)
                .Append(", CachePrunes=")
                .Append(CachePrunes)
                .Append(", CachePrunesMillis=")
                .Append(CachePrunesMillis)
                .Append(", AveragePruneMillis=")
                .Append(AveragePruneMillis)
                .Append('}');

            return sb.ToString();
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Register a cache hit (no timing information).
        /// </summary>
        public virtual void RegisterHit()
        {
            ++m_cacheHits;
        }

        /// <summary>
        /// Register a cache hit.
        /// </summary>
        /// <param name="startMillis">
        /// The time when the get operation started.
        /// </param>
        public virtual void RegisterHit(long startMillis)
        {
            m_cacheHits++;
            long stopMillis = DateTimeUtils.GetSafeTimeMillis();
            if (stopMillis > startMillis)
            {
                m_hitsMillis += (stopMillis - startMillis);
            }
        }

        /// <summary>
        /// Register a multiple cache hit.
        /// </summary>
        /// <param name="hits">
        /// The number of hits.
        /// </param>
        /// <param name="startMillis">
        /// The time when the get operation started.
        /// </param>
        public virtual void RegisterHits(int hits, long startMillis)
        {
            m_cacheHits += hits;
            if (startMillis > 0)
            {
                long stopMillis = DateTimeUtils.GetSafeTimeMillis();
                if (stopMillis > startMillis)
                {
                    m_hitsMillis += (stopMillis - startMillis);
                }
            }
        }

        /// <summary>
        /// Register a cache miss (no timing information).
        /// </summary>
        public virtual void RegisterMiss()
        {
            ++m_cacheMisses;
        }

        /// <summary>
        /// Register a cache miss.
        /// </summary>
        /// <param name="startMillis">
        /// The time when the get operation started.
        /// </param>
        public virtual void RegisterMiss(long startMillis)
        {
            m_cacheMisses++;
            long stopMillis = DateTimeUtils.GetSafeTimeMillis();
            if (stopMillis > startMillis)
            {
                m_missesMillis += (stopMillis - startMillis);
            }
        }

        /// <summary>
        /// Register a multiple cache miss.
        /// </summary>
        /// <param name="misses">
        /// The number of misses.
        /// </param>
        /// <param name="startMillis">
        /// The time when the get operation started.
        /// </param>
        public virtual void RegisterMisses(int misses, long startMillis)
        {
            m_cacheMisses += misses;
            if (startMillis > 0)
            {
                long stopMillis = DateTimeUtils.GetSafeTimeMillis();
                if (stopMillis > startMillis)
                {
                    m_missesMillis += (stopMillis - startMillis);
                }
            }
        }

        /// <summary>
        /// Register a cache put.
        /// </summary>
        /// <param name="startMillis">
        /// The time when the put operation started.
        /// </param>
        public virtual void RegisterPut(long startMillis)
        {
            m_cachePuts++;
            if (startMillis > 0)
            {
                long stopMillis = DateTimeUtils.GetSafeTimeMillis();
                if (stopMillis > startMillis)
                {
                    m_putsMillis += (stopMillis - startMillis);
                }
            }
        }

        /// <summary>
        /// Register a multiple cache put.
        /// </summary>
        /// <param name="puts">
        /// The number of puts.
        /// </param>
        /// <param name="startMillis">
        /// The time when the put operation started.
        /// </param>
        public virtual void RegisterPuts(int puts, long startMillis)
        {
            m_cachePuts += puts;
            if (startMillis > 0)
            {
                long stopMillis = DateTimeUtils.GetSafeTimeMillis();
                if (stopMillis > startMillis)
                {
                    m_putsMillis += (stopMillis - startMillis);
                }
            }
        }

        /// <summary>
        /// Register a cache prune.
        /// </summary>
        /// <param name="startMillis">
        /// The time when the prune operation started.
        /// </param>
        public virtual void RegisterCachePrune(long startMillis)
        {
            m_cachePrunes++;
            if (startMillis > 0)
            {
                long stopMillis = DateTimeUtils.GetSafeTimeMillis();
                if (stopMillis > startMillis)
                {
                    m_prunesMillis += (stopMillis - startMillis);
                }
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The rough (ie unsynchronized) number of calls that could be
        /// answered from the front or the back and were answered by data
        /// in the front cache.
        /// </summary>
        protected long m_cacheHits;

        /// <summary>
        /// The rough (ie unsynchronized) number of calls that could be
        /// answered from the front or the back and were answered by data in
        /// the back map.
        /// </summary>
        protected long m_cacheMisses;

        /// <summary>
        /// Total number of milliseconds used for get operations that were
        /// hits since the last statistics reset.
        /// </summary>
        protected long m_hitsMillis;

        /// <summary>
        /// Total number of milliseconds used for get operations that were
        /// misses since the last statistics reset.
        /// </summary>
        protected long m_missesMillis;

        /// <summary>
        /// Total number of put operations since the last statistics reset.
        /// </summary>
        protected long m_cachePuts;

        /// <summary>
        /// Total number of milliseconds used for put operations since the
        /// last statistics reset.
        /// </summary>
        protected long m_putsMillis;

        /// <summary>
        /// Total number of prune operations since the last statistics reset.
        /// </summary>
        protected long m_cachePrunes;

        /// <summary>
        /// Total number of milliseconds used for prune operations since the
        /// last statistics reset.
        /// </summary>
        protected long m_prunesMillis;

        #endregion
    }
}