/*
 * Copyright (c) 2000, 2022, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

using Tangosol.Net.Cache.Support;
using Tangosol.Util;
using Tangosol.Util.Collections;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// A local in-memory cache implementation.
    /// </summary>
    /// <remarks>
    /// <p/>
    /// The implementation is thread safe and uses a combination of
    /// Most Recently Used (MRU) and Most Frequently Used (MFU) caching
    /// strategies.
    /// <p/>
    /// The cache is size-limited, which means that once it reaches its
    /// maximum size ("high-water mark") it prunes itself (to its
    /// "low-water mark"). The cache high- and low-water-marks are measured
    /// in terms of "units", and each cached item by default uses one unit.
    /// All of the cache constructors, except for the default constructor,
    /// require the maximum number of units to be passed in. To change the
    /// number of units that each cache entry uses, either set the Units
    /// property of the cache entry, or extend the <see cref="ICache"/>
    /// implementation so that the inner <see cref="Entry"/> class calculates
    /// its own unit size. To determine the current, high-water and low-water
    /// sizes of the cache, use the cache object's <see cref="Units"/>,
    /// <see cref="HighUnits"/> and <see cref="LowUnits"/> properties.
    /// The <b>HighUnits</b> and <b>LowUnits</b> properties can be changed,
    /// even after the cache is in use. To specify the <b>LowUnits</b> value
    /// as a percentage when constructing the cache, use the extended
    /// constructor taking the percentage-prune-level.
    /// <p/>
    /// Each cached entry never expires by default. To alter this behavior,
    /// use a constructor that takes the expiry-millis; for example, an
    /// expiry-millis value of 10000 will expire entries after 10 seconds.
    /// The <see cref="ExpiryDelay"/> property can also be set once the cache
    /// is in use, but it will not affect the expiry of previously cached
    /// items.
    /// <p/>
    /// The cache can optionally be flushed on a periodic basis by setting
    /// the <see cref="FlushDelay"/> property or scheduling a specific flush
    /// time by setting the <see cref="FlushTime"/> property.
    /// <p/>
    /// Cache hit statistics can be obtained from the
    /// <see cref="CacheHits"/>, <see cref="CacheMisses"/> and
    /// <see cref="HitProbability"/> read-only properties. The statistics can
    /// be reset by invoking <see cref="ResetHitStatistics"/>. The statistics
    /// are automatically reset when the cache is cleared (the
    /// <see cref="Clear"/> method).
    /// <p/>
    /// The <b>LocalCache</b> implements the <see cref="IObservableCache"/>
    /// interface, meaning it provides event notifications to any interested
    /// listener for each insert, update and delete, including those that
    /// occur when the cache is pruned or entries are automatically expired.
    /// <p/>
    /// This implementation is designed to support extension through
    /// inheritence. To override the one-unit-per-entry default behavior,
    /// extend the inner <b>Entry</b> class and override the
    /// <see cref="Entry.CalculateUnits"/> method.
    /// </remarks>
    /// <author>Cameron Purdy  2001.04.19, 2005.05.18</author>
    /// <author>Goran Milosavljevic  2006.11.13</author>
    public class LocalCache : SynchronizedDictionary, IConfigurableCache, IObservableCache, 
                              IConcurrentCache, IQueryCache, IInvocableCache
    {
        #region Properties

        /// <summary>
        /// Gets or sets the current eviction type.
        /// </summary>
        /// <remarks>
        /// The type can only be set to an external policy if an
        /// <see cref="IEvictionPolicy"/> object has been provided.
        /// </remarks>
        /// <value>
        /// One of the <b>EvictionPolicyType</b> enum values.
        /// </value>
        public virtual EvictionPolicyType EvictionType
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_evictionType;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                AcquireWriteLock();
                try
                {
                    ConfigureEviction(value, null);
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// The percentage of the total number of units that will remain
        /// after the cache manager prunes the cache.
        /// </summary>
        /// <value>
        /// The value in the range 0.0 to 1.0.
        /// </value>
        public virtual double PruneLevel
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_pruneLevel;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                AcquireWriteLock();
                try
                {
                    m_pruneLevel = Math.Min(Math.Max(value, 0.0), 0.99);
                    m_pruneUnits = (int) (m_pruneLevel * m_maxUnits);
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ICacheStatistics"/> for this cache.
        /// </summary>
        /// <value>
        /// An <b>ICacheStatistics</b> object.
        /// </value>
        public virtual ICacheStatistics CacheStatistics
        {
            get
            {
                // the field is read-only, no need to acquire read lock
                return m_stats;
            }
        }

        /// <summary>
        /// Gets or sets the current unit calculator type for the cache.
        /// </summary>
        /// <remarks>
        /// The type can only be set to an external unit calculator if a
        /// <see cref="UnitCalculator"/> object has been provided.
        /// </remarks>
        /// <value>
        /// One of the <see cref="UnitCalculatorType"/> enum values.
        /// </value>
        public virtual UnitCalculatorType CalculatorType
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_calculatorType;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                AcquireWriteLock();
                try
                {
                    ConfigureUnitCalculator(value, null);
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets the date/time offset in milliseconds at which the
        /// next cache flush is scheduled.
        /// </summary>
        /// <remarks>
        /// Note that the date/time may be long.MaxValue, which implies that
        /// a flush will never occur. Also note that the cache may internally
        /// adjust the flush time to prevent a flush from occurring during
        /// certain processing as a means to raise concurrency.
        /// </remarks>
        /// <value>
        /// The date/time offset in milliseconds at which the next cache
        /// flush is scheduled.
        /// </value>
        public virtual long FlushTime
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_nextFlush;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                AcquireWriteLock();
                try
                {
                    m_nextFlush = value;
                    CheckFlush();
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets the rough number of cache hits since the cache statistics
        /// were last reset.
        /// </summary>
        /// <value>
        /// The number of <see cref="LocalCache.this"/> calls that have been
        /// served by existing cache entries.
        /// </value>
        public virtual long CacheHits
        {
            get
            {
                return m_stats.CacheHits;
            }
        }

        /// <summary>
        /// Gets the rough number of cache misses since the cache statistics
        /// were last reset.
        /// </summary>
        /// <value>
        /// The number of <see cref="LocalCache.this"/> calls that failed to
        /// find an existing cache entry because the requested key was not in
        /// the cache.
        /// </value>
        public virtual long CacheMisses
        {
            get
            {
                return m_stats.CacheMisses;
            }
        }

        /// <summary>
        /// Gets the rough probability (0 &lt;= p &lt;= 1) that any
        /// particular "get" invocation will be satisfied by an existing
        /// entry in the cache, based on the statistics collected since the
        /// last reset of the cache statistics.
        /// </summary>
        /// <value>
        /// The cache hit probability (0 &lt;= p &lt;= 1).
        /// </value>
        public virtual double HitProbability
        {
            get
            {
                return m_stats.HitProbability;
            }
        }

        /// <summary>
        /// Gets or sets the loader used by this <b>LocalCache</b>.
        /// </summary>
        /// <value>
        /// An <b>ICacheLoader</b> instance.
        /// </value>
        public virtual ICacheLoader CacheLoader
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_loader;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                AcquireWriteLock();
                try
                {
                    if (value != m_loader)
                    {
                        // unconfigure the old loader
                        m_loader = null;
                        m_store  = null;

                        ICacheListener listener = m_listener;
                        if (listener != null)
                        {
                            RemoveCacheListener(listener);
                            m_listener = null;
                        }

                        // configure with the new loader
                        m_loader = value;
                        if (value is ICacheStore)
                        {
                            m_store    = (ICacheStore) value;
                            m_listener = listener = InstantiateInternalListener();
                            AddCacheListener(listener);
                        }
                    }
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Get underlying cache storage.
        /// </summary>
        /// <remarks>
        /// This property should only be used while holding a read or write
        /// lock, depending on the operation that needs to be performed against
        /// the underlying storage.
        /// </remarks>
        protected virtual HashDictionary Storage
        {
            get { return (HashDictionary) m_dict; }
        }

        /// <summary>
        /// Determine the store used by this <b>LocalCache</b>, if any.
        /// </summary>
        /// <value>
        /// The <see cref="ICacheStore"/> used by this <b>LocalCache</b>,
        /// or <c>null</c> if none.
        /// </value>
        protected virtual ICacheStore CacheStore
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_store;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
        }

        /// <summary>
        /// The index IDictionary used by this <b>LocalCache</b>.
        /// </summary>
        /// <value>
        /// The <see cref="IDictionary"/> used by this <b>LocalCache</b>,
        /// or <c>null</c> if none.
        /// </value>
        protected virtual IDictionary IndexMap
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_indexMap;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current key mask for the current thread.
        /// </summary>
        /// <value>
        /// The current key mask.
        /// </value>
        protected virtual KeyMask CurrentKeyMask
        {
            get
            {
                KeyMask mask = Thread.GetData(m_ignore) as KeyMask;
                return mask ?? DEFAULT_KEY_MASK;
            }
            set { Thread.SetData(m_ignore, value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct the cache manager.
        /// </summary>
        public LocalCache()
            : this(DEFAULT_UNITS)
        {}

        /// <summary>
        /// Construct the cache manager.
        /// </summary>
        /// <param name="units">
        /// The number of units that the cache manager will cache before
        /// pruning the cache.
        /// </param>
        public LocalCache(int units)
            : this(units, DEFAULT_EXPIRE)
        {
        }

        /// <summary>
        /// Construct the cache manager.
        /// </summary>
        /// <param name="units">
        /// The number of units that the cache manager will cache before
        /// pruning the cache.
        /// </param>
        /// <param name="expiryMillis">
        /// The number of milliseconds that each cache entry lives before
        /// being automatically expired.
        /// </param>
        public LocalCache(int units, int expiryMillis)
            : this(units, expiryMillis, DEFAULT_PRUNE)
        {
        }

        /// <summary>
        /// Construct the cache manager.
        /// </summary>
        /// <param name="units">
        /// The number of units that the cache manager will cache before
        /// pruning the cache.
        /// </param>
        /// <param name="expiryMillis">
        /// The number of milliseconds that each cache entry lives before
        /// being automatically expired.
        /// </param>
        /// <param name="pruneLevel">
        /// The percentage of the total number of units that will remain
        /// after the cache manager prunes the cache (i.e. this is the
        /// "low water mark" value); this value is in the range 0.0 to 1.0.
        /// </param>
        public LocalCache(int units, int expiryMillis, double pruneLevel)
        {
            m_maxUnits    = units;
            m_expiryDelay = Math.Max(expiryMillis, 0);
            m_ignore      = Thread.AllocateDataSlot();
            PruneLevel    = pruneLevel;
            m_avgTouch    = 0;
            m_lastPrune   = DateTimeUtils.GetSafeTimeMillis();

            ScheduleFlush();
        }

        /// <summary>
        /// Construct the cache manager.
        /// </summary>
        /// <param name="units">
        /// The number of units that the cache manager will cache before
        /// pruning the cache.
        /// </param>
        /// <param name="expiryMillis">
        /// The number of milliseconds that each cache entry lives before
        /// being automatically expired.
        /// </param>
        /// <param name="loader">
        /// The <see cref="ICacheLoader"/> or <see cref="ICacheStore"/> to
        /// use.
        /// </param>
        public LocalCache(int units, int expiryMillis, ICacheLoader loader)
            : this(units, expiryMillis)
        {
            CacheLoader = loader;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Reset the cache statistics.
        /// </summary>
        public virtual void ResetHitStatistics()
        {
            m_stats.ResetHitStatistics();
        }

        /// <summary>
        /// Determines whether the <b>IDictionary</b> object contains an
        /// element with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to locate in the <b>IDictionary</b> object.
        /// </param>
        /// <returns>
        /// <b>true</b> if the <b>IDictionary</b> contains an element with
        /// the key; otherwise, <b>false</b>.
        /// </returns>
        public virtual bool ContainsKey(object key)
        {
            return Contains(key);
        }

        /// <summary>
        /// Determines whether the <b>IDictionary</b> object contains an
        /// element with the specified value.
        /// </summary>
        /// <param name="value">
        /// The value to locate in the <b>IDictionary</b> object.
        /// </param>
        /// <returns>
        /// <b>true</b> if the <b>IDictionary</b> contains an element with
        /// the value; otherwise, <b>false</b>.
        /// </returns>
        public virtual bool ContainsValue(object value)
        {
            // perform a quick check while holding a read lock
            if (!IsWriteLockHeld)
            {
                AcquireReadLock();
                try
                {
                    foreach (object key in Storage.Keys)
                    {
                        Entry entry = PeekEntryInternal(key);
                        if (entry != null)
                        {
                            if (entry.IsExpired)
                            {
                                break;
                            }
                            if (value == null)
                            {
                                if (entry.Value == null)
                                {
                                    return true;
                                }
                            }
                            else if (value.Equals(entry.Value))
                            {
                                return true;
                            }
                        }
                    }
                }
                finally
                {
                    ReleaseReadLock();
                }
            }

            // if this thread is holding a read lock, the write lock
            // attempt will fail.  So, defer the expiration check until
            // the next operation.
            if (IsReadLockHeld)
            {
                // if we got here, it's not in the cache
                return false;
            }           
            else
            {
                // perform a second check while holding the write lock
                AcquireWriteLock();
                try
                {
                    foreach (object key in Storage.Keys)
                    {
                        Entry entry = GetEntryInternal(key);
                        if (entry != null)
                        {
                            if (value == null)
                            {
                                if (entry.Value == null)
                                {
                                    return true;
                                }
                            }
                            else if (value.Equals(entry.Value))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Locate an <see cref="Entry"/> in the cache based on its key.
        /// </summary>
        /// <param name="key">
        /// The key object to search for.
        /// </param>
        /// <returns>
        /// The <b>Entry</b> or <c>null</c>.
        /// </returns>
        public virtual Entry GetEntry(object key)
        {
            // perform a quick check while holding a read lock
            if (!IsWriteLockHeld)
            {
                AcquireReadLock();
                try
                {
                    // check if the cache needs flushing
                    if (!IsFlushRequired())
                    {
                        Entry entry = PeekEntryInternal(key);
                        if (entry == null)
                        {
                            if (m_loader == null)
                            {
                                m_stats.RegisterMiss();
                                return null;
                            }
                        }
                        else if (!entry.IsExpired)
                        {
                            m_stats.RegisterHit();
                            entry.Touch();
                            return entry;
                        }
                    }
                }
                finally
                {
                    ReleaseReadLock();
                }
            }

            // perform a second check while holding the write lock
            AcquireWriteLock();
            try
            {
                // check if the cache needs flushing
                CheckFlush();

                Entry entry = GetEntryInternal(key);
                if (entry == null)
                {
                    if (m_loader == null)
                    {
                        m_stats.RegisterMiss();
                    }
                    else
                    {
                        long start = DateTimeUtils.GetSafeTimeMillis();

                        Load(key);

                        // use GetEntryInternal() instead of get to avoid screwing
                        // up stats
                        entry = GetEntryInternal(key);
                        m_stats.RegisterMisses(1, start);    
                    }
                }
                else
                {
                    m_stats.RegisterHit();
                    entry.Touch();
                }

                return entry;
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Indicates to the cache that the specified key should be loaded
        /// into the cache, if it is not already in the cache.
        /// </summary>
        /// <remarks>
        /// This provides a means to "pre-load" a single entry into the cache
        /// using the cache's loader.
        /// <p/>
        /// If a valid entry with the specified key already exists in the
        /// cache, or if the cache does not have a loader, then this method
        /// has no effect.
        /// <p/>
        /// An implementation may perform the load operation asynchronously.
        /// </remarks>
        /// <param name="key">
        /// The key to request to be loaded.
        /// </param>
        public virtual void Load(object key)
        {
            if (CacheLoader == null)
            {
                return;
            }

            AcquireWriteLock();
            try
            {
                ICacheLoader loader = m_loader;
                if (loader != null && GetEntryInternal(key) == null)
                {
                    object value = loader.Load(key);
                    if (value != null)
                    {
                        CurrentKeyMask = new LoadKeyMask(key);
                        try
                        {
                            Insert(key, value);
                        }
                        finally
                        {
                            CurrentKeyMask = null;
                        }
                    }
                }
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Indicates to the cache that it should load data from its loader
        /// to fill the cache; this is sometimes referred to as
        /// "pre-loading" or "warming" a cache.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// The specific set of data that will be loaded is unspecified.
        /// The implementation may choose to load all data, some specific
        /// subset of the data, or no data. An implementation may require
        /// that the loader implement the IIterableCacheLoader interface in
        /// order for this method to load any data.
        /// <p/>
        /// An implementation may perform the load operation asynchronously.
        /// </remarks>
        public virtual void LoadAll()
        {
            if (!(CacheLoader is IIterableCacheLoader))
            {
                return;
            }

            AcquireWriteLock();
            try
            {
                ICacheLoader loader = m_loader;
                if (loader is IIterableCacheLoader)
                {
                    IEnumerator iter = ((IIterableCacheLoader) loader).Keys;

                    long maxUnits = HighUnits;
                    if (maxUnits > 0 && maxUnits < Int32.MaxValue)
                    {
                        long target  = Math.Max(LowUnits, (int) (0.9 * maxUnits));
                        long current = Units;
                        while (iter.MoveNext() && current < target)
                        {
                            Load(iter.Current);

                            long units = Units;
                            if (units < current)
                            {
                                // cache is already starting to prune itself 
                                // for some reason; assume that eviction 
                                // occurred which is an indication that we've
                                // warmed the cache suitably
                                break;
                            }

                            current = units;
                        }
                    }
                    else
                    {
                        IList array = new ArrayList();
                        while (iter.MoveNext())
                        {
                            array.Add(iter.Current);
                        }

                        LoadAll(array);
                    }
                }
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Indicates to the cache that the specified keys should be loaded
        /// into the cache, if they are not already in the cache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This provides a means to "pre-load" entries into the cache using
        /// the cache's loader.</p>
        /// <p>
        /// The result of this method is defined to be semantically the same
        /// as the following implementation:</p>
        /// <pre>
        /// ICacheLoader loader = CacheLoader;
        /// if (loader != null &amp;&amp; keys.Count != 0)
        /// {
        ///     ArrayList requestList = new ArrayList(keys);
        ///     CollectionUtils.RemoveAll(requestList, PeekAll(keys).Keys);
        ///     if (requestList.Count != 0)
        ///     {
        ///         IDictionary dictionary = loader.LoadAll(requestList);
        ///         if (dictionary.Count != 0)
        ///         {
        ///             CollectionUtils.AddAll(dictionary);
        ///         }
        ///     }
        /// }
        /// </pre>
        /// </remarks>
        /// <param name="keys">
        /// A collection of keys to request to be loaded.
        /// </param>
        public virtual void LoadAll(ICollection keys)
        {
            if (keys == null || keys.Count == 0 || CacheLoader == null)
            {
                return;
            }

            AcquireWriteLock();
            try
            {
                ICacheLoader loader = m_loader;
                if (loader != null)
                {
                    ICollection colRequest = new HashSet(keys);
                    CollectionUtils.RemoveAll(colRequest, new HashSet(PeekAll(keys).Keys));

                    if (colRequest.Count != 0)
                    {
                        IDictionary dictionary = loader.LoadAll(colRequest);
                        if (dictionary.Count != 0)
                        {
                            CurrentKeyMask = new LoadAllKeyMask(dictionary.Keys);
                            try
                            {
                                InsertAll(dictionary);
                            }
                            finally
                            {
                                CurrentKeyMask = null;
                            }
                        }
                    }
                }
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Checks for a valid entry corresponding to the specified key in
        /// the cache, and returns the corresponding value if it is.
        /// </summary>
        /// <remarks>
        /// If it is not in the cache, returns <c>null</c>, and does not
        /// attempt to load the value using its cache loader.
        /// </remarks>
        /// <param name="key">
        /// The key to "peek" into the cache for.
        /// </param>
        /// <returns>
        /// The value corresponding to the specified key.
        /// </returns>
        public virtual object Peek(object key)
        {
            // perform a quick check while holding a read lock
            if (!IsWriteLockHeld)
            {
                AcquireReadLock();
                try
                {
                    // avoid base[] because it affects statistics
                    Entry entry = PeekEntryInternal(key);
                    if (entry == null)
                    {
                        return null;
                    }
                    if (!entry.IsExpired)
                    {
                        return entry.Value;
                    }
                }
                finally
                {
                    ReleaseReadLock();
                }
            }

            // if this thread is holding a read lock, the write lock
            // attempt will fail.  So, defer the expiration check until
            // the next operation.
            if (IsReadLockHeld)
            {
                return null;
            }
            else {
                // perform a second check while holding the write lock
                AcquireWriteLock();
                try
                {
                    // avoid base[] because it affects statistics
                    Entry entry = GetEntryInternal(key);
                    return entry == null ? null : entry.Value;
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Checks for a valid entry corresponding to each specified key in
        /// the cache, and places the corresponding value in the returned
        /// dictionary if it is.
        /// </summary>
        /// <remarks>
        /// For each key that is not in the cache, no entry is placed into
        /// the returned dictionary. The cache does not attempt to load any
        /// values using its cache loader.
        /// <p/>
        /// The result of this method is defined to be semantically the same
        /// as the following implementation, without regards to threading
        /// issues:
        /// <pre>
        /// IDictionary dict = new Hashtable();
        ///
        /// foreach (object key in keys)
        /// {
        ///    Object value = Peek(key);
        ///    if (value != null || Contains(key))
        ///    {
        ///        dict.Add(key, value);
        ///    }
        /// }
        /// return dict;
        /// </pre>
        /// </remarks>
        /// <param name="keys">
        /// A collection of keys to "peek" into the cache for.
        /// </param>
        /// <returns>
        /// An <b>IDictionary</b> of keys that were found in the cache and
        /// their values.
        /// </returns>
        public virtual IDictionary PeekAll(ICollection keys)
        {
            if (keys == null || keys.Count == 0)
            {
                return new HashDictionary();
            }

            // perform a quick check while holding the read lock
            if (!IsWriteLockHeld)
            {
                bool fReadLockHeld = IsReadLockHeld;
                AcquireReadLock();
                try
                {
                    IDictionary dictionary = new HashDictionary(keys.Count);
                    foreach (object key in keys)
                    {
                        Entry entry = PeekEntryInternal(key);
                        if (entry != null)
                        {
                            if (entry.IsExpired)
                            {
                                if (fReadLockHeld)
                                {
                                    continue; // defer expiration processing
                                }
                                else
                                {
                                    dictionary = null;
                                    break;
                                }
                            }
                            dictionary[key] = entry.Value;
                        }
                    }

                    // if this thread is holding a read lock, the write lock
                    // attempt will fail.  So, defer the expiration check until
                    // the next operation.
                    if (dictionary != null || fReadLockHeld)
                    {
                        return dictionary;
                    }
                }
                finally
                {
                    ReleaseReadLock();
                }
            }

            // perform a second check while holding the write lock
            AcquireWriteLock();
            try
            {
                IDictionary dictionary = new HashDictionary(keys.Count);
                foreach (object key in keys)
                {
                    Entry entry = GetEntryInternal(key);
                    if (entry != null)
                    {
                        dictionary[key] = entry.Value;
                    }
                }

                return dictionary;
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Removes all mappings from this map.
        /// </summary>
        /// <remarks>
        /// Note: the removal of entries caused by this truncate operation will
        /// not be observable.
        /// </remarks>
        public virtual void Truncate()
        {
            ClearInternal(false);
        }

        #endregion

        #region ICollection implementation

        /// <summary>
        /// Gets the number of elements contained in the
        /// <see cref="ICache"/>.
        /// </summary>
        /// <value>
        /// The number of elements contained in the <b>ICache</b>.
        /// </value>
        public override int Count
        {
            get
            {
                bool fReadLockHeld = IsReadLockHeld;

                // perform a quick check while holding a read lock
                AcquireReadLock();
                try
                {
                    // if this thread is holding a read lock, the write lock
                    // attempt will fail.  So, defer the expiration check until
                    // the next operation.
                    if (fReadLockHeld || !IsFlushRequired())
                    {
                        return Storage.Count;   
                    }
                }
                finally
                {
                    ReleaseReadLock();
                }

                // perform a second check while holding the write lock
                AcquireWriteLock();
                try
                {
                    // check if the cache needs flushing
                    CheckFlush();
                    return Storage.Count;
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Copies the elements of the <b>IDictionary</b> to an <b>Array</b>,
        /// starting at a particular index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <b>Array</b> that is the destination of the
        /// elements copied from <b>IDictionary</b>.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        public override void CopyTo(Array array, int arrayIndex)
        {
            AcquireReadLock();
            try
            {
                foreach (Entry entry in Entries)
                {
                    array.SetValue(entry.Value, arrayIndex++);
                }
            }
            finally
            {
                ReleaseReadLock();
            }
        }

        #endregion

        #region IDictionary implementation

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <value>
        /// The element with the specified key.
        /// </value>
        /// <param name="key">
        /// The key of the element to get or set.
        /// </param>
        public override object this[object key]
        {
            get
            {
                Entry entry = GetEntry(key);
                return entry == null ? null : entry.Value;
            }
            set
            {
                Insert(key, value);
            }
        }

        /// <summary>
        /// Determines whether the object contains an element with the
        /// specified key.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the <see cref="ICache"/> contains an element
        /// with the key; otherwise, <b>false</b>.
        /// </returns>
        /// <param name="key">
        /// The key to locate in the <b>ICache</b> object.
        /// </param>
        public override bool Contains(object key)
        {
            // perform a quick check while holding a read lock
            if (!IsWriteLockHeld)
            {
                AcquireReadLock();
                try
                {
                    Entry entry = PeekEntryInternal(key);
                    if (entry == null)
                    {
                        return false;
                    }
                    else if (!entry.IsExpired)
                    {
                        return true;
                    }
                }
                finally
                {
                    ReleaseReadLock();
                }
            }

            // if this thread is holding a read lock, the write lock
            // attempt will fail.  So, defer the expiration check until
            // the next operation.
            if (IsReadLockHeld)
            {
                return false;
            }
            else
            {
                // perform a second check while holding the write lock
                AcquireWriteLock();
                try
                {
                    return GetEntryInternal(key) != null;
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Removes all elements from the <see cref="ICache"/> object.
        /// </summary>
        public override void Clear()
        {
            ClearInternal(true);   
        }

        /// <summary>
        /// Removes the element with the specified key from the
        /// <see cref="ICache"/> object.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        public override void Remove(object key)
        {
            // this method is only called as a result of a call from the cache
            // consumer, not from any internal eviction etc.

            AcquireWriteLock();
            try
            {
                // check if the cache needs flushing
                CheckFlush();

                // check for the specified entry; GetEntryInternal() will only
                // return an entry if the entry exists and has not expired
                Entry entry = GetEntryInternal(key);
                if (entry != null)
                {
                    // if there is an ICacheStore, tell it that the entry is
                    // being erased
                    ICacheStore store = m_store;
                    if (store != null)
                    {
                        store.Erase(key);
                    }

                    RemoveInternal(entry, true);
                }
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Adds an element with the provided key and value to the cache.
        /// </summary>
        /// <param name="value">
        /// The object to use as the value of the element to add.
        /// </param>
        /// <param name="key">
        /// The object to use as the key of the element to add.
        /// </param>
        public override void Add(object key, object value)
        {
            AcquireWriteLock();
            try
            {
                // check if the cache needs flushing
                CheckFlush();

                Entry entry = GetEntryInternal(key);
                if (entry == null)
                {
                    AddInternal(key, value);
                }
                else
                {
                    throw new ArgumentException("An entry with the key '" + key + "' already exists");
                }

                // check the cache size (COH-467, COH-480)
                if (m_curUnits > m_maxUnits)
                {
                    Prune();
                }

                m_stats.RegisterPut(0L);
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Get the keys collection.
        /// </summary>
        /// <value>
        /// The keys collection.
        /// </value>
        public override ICollection Keys
        {
            get
            {
                ICollection col = m_colKeys;
                if (col == null)
                {
                    AcquireReadLock();
                    try
                    {
                        if (m_colKeys != null)
                        {
                            return m_colKeys;
                        }
                        m_colKeys = col = InstantiateKeysCollection(this);   
                    }
                    finally
                    {
                        ReleaseReadLock();
                    }
                }
                return col;
            }
        }

        /// <summary>
        /// Get the values collection.
        /// </summary>
        /// <value>
        /// The values collection.
        /// </value>
        public override ICollection Values
        {
            get
            {
                ICollection col = m_colValues;
                if (col == null)
                {
                    AcquireReadLock();
                    try
                    {
                        if (m_colValues != null)
                        {
                            return m_colValues;
                        }
                        m_colValues = col = InstantiateValuesCollection(this);
                    }
                    finally 
                    {
                        ReleaseReadLock();
                    }
                }
                return col;
            }
        }

        #endregion

        #region ICache implementation

        /// <summary>
        /// Get a collection of <see cref="ICacheEntry"/> instances
        /// within the cache.
        /// </summary>
        public virtual ICollection Entries
        {
            get
            {
                ICollection col = m_colEntries;
                if (col == null)
                {
                    AcquireReadLock();
                    try
                    {
                        if (m_colEntries != null)
                        {
                            return m_colEntries;
                        }
                        m_colEntries = col = InstantiateEntriesCollection(this);
                    }
                    finally
                    {
                        ReleaseReadLock();
                    }
                }
                return col;
            }
        }

        /// <summary>
        /// Get the values for all the specified keys, if they are in the
        /// cache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// For each key that is in the cache, that key and its corresponding
        /// value will be placed in the dictionary that is returned by this
        /// method. The absence of a key in the returned dictionary indicates
        /// that it was not in the cache, which may imply (for caches that
        /// can load behind the scenes) that the requested data could not be
        /// loaded.</p>
        /// <p>
        /// The result of this method is defined to be semantically the same
        /// as the following implementation, without regards to threading
        /// issues:</p>
        /// <pre>
        /// IDictionary dict = new AnyDictionary();
        /// // could be a Hashtable (but does not have to)
        /// foreach (object key in colKeys)
        /// {
        ///     object value = this[key];
        ///     if (value != null || Contains(key))
        ///     {
        ///         dict[key] = value;
        ///     }
        /// }
        /// return dict;
        /// </pre>
        /// </remarks>
        /// <param name="keys">
        /// A collection of keys that may be in the cache.
        /// </param>
        /// <returns>
        /// A dictionary of keys to values for the specified keys passed in
        /// <paramref name="keys"/>.
        /// </returns>
        public virtual IDictionary GetAll(ICollection keys)
        {
            // perform a quick check while holding a read lock
            if (!IsWriteLockHeld)
            {
                AcquireReadLock();
                try
                {
                    IDictionary dictionary = new HashDictionary(keys.Count);

                    // get all of the requested keys that are already loaded 
                    // into the dictionary
                    foreach (object key in keys)
                    {
                        Entry entry = PeekEntryInternal(key);
                        if (entry != null)
                        {
                            if (entry.IsExpired)
                            {
                                dictionary = null;
                                break;
                            }
                            dictionary[key] = entry.Value;
                        }
                    }

                    if (dictionary != null)
                    {
                        int total = keys.Count;
                        int hits  = dictionary.Count;

                        if (hits == total || m_loader == null)
                        {
                            // update stats
                            m_stats.RegisterHits(hits, 0);
                            m_stats.RegisterMisses(total - hits, 0);

                            return dictionary;
                        }
                    }
                }
                finally
                {
                    ReleaseReadLock();
                }
            }

            // perform a second check while holding the write lock
            AcquireWriteLock();
            try
            {
                long start = 0;

                // first, get all of the requested keys that are already loaded
                // into the dictionary
                IDictionary dictionary = PeekAll(keys);
                int         total      = keys.Count;
                int         hits       = dictionary.Count;

                if (hits < total)
                {
                    // load the remaining keys
                    ICacheLoader loader = m_loader;
                    if (loader != null)
                    {
                        start = DateTimeUtils.GetSafeTimeMillis();

                        // build a list of the missing keys to load
                        ICollection request = new HashSet(keys);

                        CollectionUtils.RemoveAll(request, new HashSet(dictionary.Keys));

                        // load the missing keys
                        LoadAll(request);

                        // whichever ones are now loaded, add their values to the result
                        CollectionUtils.AddAll(dictionary, PeekAll(request));
                    }
                }

                // update stats
                m_stats.RegisterHits(hits, start);
                m_stats.RegisterMisses(total - hits, start);

                return dictionary;
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Associates the specified value with the specified key in this
        /// cache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the cache previously contained a mapping for this key, the old
        /// value is replaced.</p>
        /// <p>
        /// Invoking this method is equivalent to the following call:
        /// <pre>
        /// Insert(key, value, CacheExpiration.Default);
        /// </pre></p>
        /// </remarks>
        /// <param name="key">
        /// Key with which the specified value is to be associated.
        /// </param>
        /// <param name="value">
        /// Value to be associated with the specified key.
        /// </param>
        /// <returns>
        /// Previous value associated with specified key, or <c>null</c> if
        /// there was no mapping for key. A <c>null</c> return can also
        /// indicate that the dictionary previously associated <c>null</c>
        /// with the specified key, if the implementation supports
        /// <c>null</c> values.
        /// </returns>
        public virtual object Insert(object key, object value)
        {
            return Insert(key, value, CacheExpiration.DEFAULT);
        }

        /// <summary>
        /// Associates the specified value with the specified key in this
        /// cache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the cache previously contained a mapping for this key, the old
        /// value is replaced.</p>
        /// This variation of the <see cref="ICache.Insert(object,object)"/>
        /// method allows the caller to specify an expiry (or "time to live")
        /// for the cache entry.
        /// </remarks>
        /// <param name="key">
        /// Key with which the specified value is to be associated.
        /// </param>
        /// <param name="value">
        /// Value to be associated with the specified key.
        /// </param>
        /// <param name="millis">
        /// The number of milliseconds until the cache entry will expire,
        /// also referred to as the entry's "time to live"; pass
        /// <see cref="CacheExpiration.DEFAULT"/> to use the cache's
        /// default time-to-live setting; pass
        /// <see cref="CacheExpiration.NEVER"/> to indicate that the
        /// cache entry should never expire; this milliseconds value is
        /// <b>not</b> a date/time value, but the amount of time object will
        /// be kept in the cache.
        /// </param>
        /// <returns>
        /// Previous value associated with specified key, or <c>null</c> if
        /// there was no mapping for key. A <c>null</c> return can also
        /// indicate that the cache previously associated <c>null</c> with
        /// the specified key, if the implementation supports <c>null</c>
        /// values.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// If the requested expiry is a positive value and the
        /// implementation does not support expiry of cache entries.
        /// </exception>
        public virtual object Insert(object key, object value, long millis)
        {
            object orig = null;

            AcquireWriteLock();
            try
            {
                // check if the cache needs flushing
                CheckFlush();

                Entry entry = GetEntryInternal(key);
                if (entry == null)
                {
                    entry = AddInternal(key, value);
                }
                else
                {
                    // cache entry already exists
                    entry.Touch();

                    orig        = entry.Value;
                    entry.Value = value;
                }

                if (millis != 0L)
                {
                    entry.ExpiryMillis = millis > 0L ? DateTimeUtils.GetSafeTimeMillis() + millis : 0L;

                    if (millis > 0 && FlushDelay == 0)
                    {
                        // the cache does not have a flush delay; we need to
                        // ensure that this entry would be eventually flushed
                        FlushDelay = DEFAULT_FLUSH;
                    }
                }

                // check the cache size (COH-467, COH-480)
                if (m_curUnits > m_maxUnits)
                {
                    Prune();

                    // could have evicted the item we just inserted/updated
                    if (GetEntryInternal(key) == null)
                    {
                        orig = null;
                    }
                }
            }
            finally
            {
                ReleaseWriteLock();
            }

            m_stats.RegisterPut(0L);
            return orig;
        }

        /// <summary>
        /// Copies all of the mappings from the specified dictionary to this
        /// cache (optional operation).
        /// </summary>
        /// <remarks>
        /// These mappings will replace any mappings that this cache had for
        /// any of the keys currently in the specified dictionary.
        /// </remarks>
        /// <param name="dictionary">
        /// Mappings to be stored in this cache.
        ///  </param>
        /// <exception cref="InvalidCastException">
        /// If the class of a key or value in the specified dictionary
        /// prevents it from being stored in this cache.
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// This cache does not permit <c>null</c> keys or values, and the
        /// specified key or value is <c>null</c>.
        /// </exception>
        public virtual void InsertAll(IDictionary dictionary)
        {
            AcquireWriteLock();
            try
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    Insert(entry.Key, entry.Value);
                }
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Returns an <see cref="ICacheEnumerator"/> object for the
        /// <b>ICache</b> instance.
        /// </summary>
        /// <returns>An <b>ICacheEnumerator</b> object for the
        /// <b>ICache</b> instance.</returns>
        public new virtual ICacheEnumerator GetEnumerator()
        {
            return InstantiateCacheEnumerator(this, EnumeratorMode.Entries);
        }

        #endregion

        #region IConfigurableCache implementation

        /// <summary>
        /// Gets the number of units that the cache currently stores.
        /// </summary>
        /// <value>
        /// The number of units that the cache currently stores.
        /// </value>
        public virtual long Units
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_curUnits;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets the point to which the cache will shrink when it
        /// prunes.
        /// </summary>
        /// <remarks>
        /// This is often referred to as a "low water mark" of the cache.
        /// </remarks>
        /// <value>
        /// The number of units that the cache prunes to.
        /// </value>
        public virtual long LowUnits
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_pruneUnits;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("low units out of bounds");
                }

                AcquireWriteLock();
                try
                {
                    if (value >= m_maxUnits)
                    {
                        value = (int) (m_pruneLevel * m_maxUnits);
                    }

                    m_pruneUnits = value;
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets the limit of the cache size in units.
        /// </summary>
        /// <remarks>
        /// The cache will prune itself automatically once it reaches
        /// its maximum unit level. This is often referred to as the
        /// "high water mark" of the cache.
        /// </remarks>
        /// <value>
        /// The limit of the cache size in units.
        /// </value>
        public virtual long HighUnits
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_maxUnits;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("High units out of bounds.");
                }

                AcquireWriteLock();
                try
                {
                    bool shrink = value < m_maxUnits;

                    m_maxUnits = value;

                    // ensure that low units are in range
                    LowUnits = LowUnits;

                    if (shrink)
                    {
                        CheckSize();
                    }
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets the "time to live" for each individual cache entry.
        /// </summary>
        /// <remarks>
        /// This does not affect the already-scheduled expiry of existing
        /// entries.
        /// </remarks>
        /// <value>
        /// The number of milliseconds that a cache entry value will live,
        /// or zero if cache entries are never automatically expired.
        /// </value>
        public virtual int ExpiryDelay
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_expiryDelay;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                AcquireWriteLock();
                try
                {
                    m_expiryDelay = Math.Max(value, 0);
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets the delay between cache flushes.
        /// </summary>
        /// <value>
        /// The number of milliseconds between cache flushes, or zero which
        /// signifies that the cache never flushes
        /// </value>
        public virtual int FlushDelay
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_flushDelay;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                AcquireWriteLock();
                try
                {
                    m_flushDelay = Math.Max(value, 0);
                    ScheduleFlush();
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current external unit calculator, if any.
        /// </summary>
        /// <remarks>
        /// If <c>null</c> is passed, clear the external unit calculator,
        /// and use the default unit calculator.
        /// </remarks>
        /// <value>
        /// The external unit calculator, if one has been provided.
        /// </value>
        public virtual IUnitCalculator UnitCalculator
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_calculator;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                AcquireWriteLock();
                try
                {
                    UnitCalculatorType type = value == null ? UnitCalculatorType.Fixed : UnitCalculatorType.External;
                    ConfigureUnitCalculator(type, value);
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Determine the current external eviction policy, if any.
        /// </summary>
        /// <remarks>
        /// If <c>null</c> is passed, clear the external eviction policy, and
        /// use the default internal policy.
        /// </remarks>
        /// <value>
        /// The external eviction policy, if one has been provided.
        /// </value>
        public virtual IEvictionPolicy EvictionPolicy
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_policy;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                AcquireWriteLock();
                try
                {
                    EvictionPolicyType type = value == null ? EvictionPolicyType.Hybrid : EvictionPolicyType.External;
                    ConfigureEviction(type, value);
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Locate a cache entry in the cache based on its key.
        /// </summary>
        /// <param name="key">
        /// The key object to search for.
        /// </param>
        /// <returns>
        /// The entry or null.
        /// </returns>
        public IConfigurableCacheEntry GetCacheEntry(object key)
        {
            return GetEntry(key);
        }

        /// <summary>
        /// Evict a specified key from the cache, as if it had expired from
        /// the cache.
        /// </summary>
        /// <remarks>
        /// If the key is not in the cache, then the method has no effect.
        /// </remarks>
        /// <param name="key">
        /// The key to evict from the cache.
        /// </param>
        public virtual void Evict(object key)
        {
            AcquireWriteLock();
            try
            {
                Entry entry = GetEntryInternal(key);
                if (entry != null)
                {
                    RemoveExpired(entry, true);
                }
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Evict the specified keys from the cache, as if they had each
        /// expired from the cache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The result of this method is defined to be semantically the same
        /// as the following implementation:</p>
        /// <pre>
        /// foreach (object key in keys)
        /// {
        ///     Evict(key);
        /// }
        /// </pre>
        /// </remarks>
        /// <param name="keys">
        /// A collection of keys to evict from the cache.
        /// </param>
        public virtual void EvictAll(ICollection keys)
        {
            AcquireWriteLock();
            try
            {
                foreach (object key in keys)
                {
                    Entry entry = GetEntryInternal(key);
                    if (entry != null)
                    {
                        RemoveExpired(entry, true);
                    }
                }
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Evict all entries from the cache that are no longer valid, and
        /// potentially prune the cache size if the cache is size-limited
        /// and its size is above the caching low water mark.
        /// </summary>
        public virtual void Evict()
        {
            AcquireWriteLock();
            try
            {
                IList expiredEntries = new ArrayList();
                foreach (Entry entry in Storage.Values)
                {
                    if (entry.IsExpired)
                    {
                        expiredEntries.Add(entry);
                    }
                }

                foreach (Entry entry in expiredEntries)
                {
                    RemoveExpired(entry, true);
                }

                ScheduleFlush();
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        #endregion

        #region IObservableCache implementation

        /// <summary>
        /// Add a standard cache listener that will receive all events
        /// (inserts, updates, deletes) that occur against the cache, with
        /// the key, old-value and new-value included.
        /// </summary>
        /// <remarks>
        /// This has the same result as the following call:
        /// <pre>
        /// AddCacheListener(listener, (IFilter) null, false);
        /// </pre>
        /// </remarks>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to add.
        /// </param>
        public virtual void AddCacheListener(ICacheListener listener)
        {
            AddCacheListener(listener, null, false);
        }

        /// <summary>
        /// Remove a standard cache listener that previously signed up for
        /// all events.
        /// </summary>
        /// <remarks>
        /// This has the same result as the following call:
        /// <pre>
        /// RemoveCacheListener(listener, (IFilter) null);
        /// </pre>
        /// </remarks>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to remove.
        /// </param>
        public virtual void RemoveCacheListener(ICacheListener listener)
        {
            RemoveCacheListener(listener, null);
        }

        /// <summary>
        /// Add a cache listener for a specific key.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The listeners will receive <see cref="CacheEventArgs"/> objects,
        /// but if <paramref name="isLite"/> is passed as <b>true</b>, they
        /// <i>might</i> not contain the
        /// <see cref="CacheEventArgs.OldValue"/> and
        /// <see cref="CacheEventArgs.NewValue"/> properties.</p>
        /// <p>
        /// To unregister the ICacheListener, use the
        /// <see cref="IObservableCache.RemoveCacheListener(ICacheListener,object)"/>
        /// method.</p>
        /// </remarks>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to add.
        /// </param>
        /// <param name="key">
        /// The key that identifies the entry for which to raise events.
        /// </param>
        /// <param name="isLite">
        /// <b>true</b> to indicate that the <see cref="CacheEventArgs"/>
        /// objects do not have to include the <b>OldValue</b> and
        /// <b>NewValue</b> property values in order to allow optimizations.
        /// </param>
        public virtual void AddCacheListener(ICacheListener listener, object key, bool isLite)
        {
            Debug.Assert(listener != null);

            AcquireWriteLock();
            try
            {
                CacheListenerSupport support =  m_listenerSupport 
                                             ?? (m_listenerSupport = new CacheListenerSupport());

                support.AddListener(listener, key, isLite);
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Remove a cache listener that previously signed up for events
        /// about a specific key.
        /// </summary>
        /// <param name="listener">
        /// The listener to remove.
        /// </param>
        /// <param name="key">
        /// The key that identifies the entry for which to raise events.
        /// </param>
        public virtual void RemoveCacheListener(ICacheListener listener, object key)
        {
            Debug.Assert(listener != null);

            AcquireWriteLock();
            try
            {
                CacheListenerSupport support = m_listenerSupport;
                if (support != null)
                {
                    support.RemoveListener(listener, key);
                    if (support.IsEmpty())
                    {
                        m_listenerSupport = null;
                    }
                }
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Add a cache listener that receives events based on a filter
        /// evaluation.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The listeners will receive <see cref="CacheEventArgs"/> objects,
        /// but if <paramref name="isLite"/> is passed as <b>true</b>, they
        /// <i>might</i> not contain the <b>OldValue</b> and <b>NewValue</b>
        /// properties.</p>
        /// <p>
        /// To unregister the <see cref="ICacheListener"/>, use the
        /// <see cref="IObservableCache.RemoveCacheListener(ICacheListener,IFilter)"/>
        /// method.</p>
        /// </remarks>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to add.</param>
        /// <param name="filter">
        /// A filter that will be passed <b>CacheEventArgs</b> objects to
        /// select from; a <b>CacheEventArgs</b> will be delivered to the
        /// listener only if the filter evaluates to <b>true</b> for that
        /// <b>CacheEventArgs</b>; <c>null</c> is equivalent to a filter
        /// that alway returns <b>true</b>.
        /// </param>
        /// <param name="isLite">
        /// <b>true</b> to indicate that the <see cref="CacheEventArgs"/>
        /// objects do not have to include the <b>OldValue</b> and
        /// <b>NewValue</b> property values in order to allow optimizations.
        /// </param>
        public virtual void AddCacheListener(ICacheListener listener, IFilter filter, bool isLite)
        {
            Debug.Assert(listener != null);

            AcquireWriteLock();
            try
            {
                CacheListenerSupport support =   m_listenerSupport 
                                             ?? (m_listenerSupport = new CacheListenerSupport());

                support.AddListener(listener, filter, isLite);
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Remove a cache listener that previously signed up for events
        /// based on a filter evaluation.
        /// </summary>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to remove.
        /// </param>
        /// <param name="filter">
        /// A filter used to evaluate events; <c>null</c> is equivalent to a
        /// filter that alway returns <b>true</b>.
        /// </param>
        public virtual void RemoveCacheListener(ICacheListener listener, IFilter filter)
        {
            Debug.Assert(listener != null);

            AcquireWriteLock();
            try
            {
                CacheListenerSupport support = m_listenerSupport;
                if (support != null)
                {
                    support.RemoveListener(listener, filter);
                    if (support.IsEmpty())
                    {
                        m_listenerSupport = null;
                    }
                }
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        #endregion

        #region IConcurrentCache implementation

        /// <summary>
        /// Factory pattern.
        /// </summary>
        /// <returns>
        /// A new instance of the Lock class (or a subclass thereof).
        /// </returns>
        internal virtual CacheLock InstantiateLock(object key)
        {
            return new CacheLock();
        }

        /// <summary>
        /// Attempt to lock the specified item within the specified period of
        /// time.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The item doesn't have to exist to be <i>locked</i>. While the
        /// item is locked there is known to be a <i>lock holder</i> which
        /// has an exclusive right to modify (calling put and remove methods)
        /// that item.</p>
        /// <p>
        /// Lock holder is an abstract concept that depends on the
        /// IConcurrentCache implementation. For example, holder could
        /// be a cluster member or a thread (or both).</p>
        /// <p>
        /// Locking strategy may vary for concrete implementations as well.
        /// Lock could have an expiration time (this lock is sometimes called
        /// a "lease") or be held indefinitely (until the lock holder
        /// terminates).</p>
        /// <p>
        /// Some implementations may allow the entire cache to be locked. If
        /// the cache is locked in such a way, then only a lock holder is
        /// allowed to perform any of the "put" or "remove" operations.</p>
        /// <p>
        /// Pass the special constant
        /// <see cref="LockScope.LOCK_ALL"/> as the <i>key</i>
        /// parameter to indicate the cache lock.</p>
        /// </remarks>
        /// <param name="key">
        /// Key being locked.
        /// </param>
        /// <param name="waitTimeMillis">
        /// The number of milliseconds to continue trying to obtain a lock;
        /// pass zero to return immediately; pass -1 to block the calling
        /// thread until the lock could be obtained.
        /// </param>
        /// <returns>
        /// <b>true</b> if the item was successfully locked within the
        /// specified time; <b>false</b> otherwise.
        /// </returns>
        public virtual bool Lock(object key, long waitTimeMillis)
        {
            IDictionary mapLock = m_mapLock;
            Gate        gateMap = m_gateMap;

            if (key == LockScope.LOCK_ALL)
            {
                return gateMap.Close(waitTimeMillis);
            }

            if (!gateMap.Enter(waitTimeMillis))
            {
                return false;
            }

            bool      isSuccess = false;
            CacheLock cacheLock = null;
            try
            {
                while (true)
                {
                    lock (mapLock.SyncRoot)
                    {
                        cacheLock = (CacheLock) mapLock[key];
                        if (cacheLock == null)
                        {
                            cacheLock = InstantiateLock(key);
                            cacheLock.Assign(0); // this will succeed without blocking
                            mapLock[key] = cacheLock;
                            return isSuccess = true;
                        }
                        else
                        {
                            // perform a quick, non-blocking check to see if the
                            // current thread already owns the lock
                            if (cacheLock.IsOwnedByCaller)
                            {
                                cacheLock.Assign(0); // this will succeed without blocking
                                return isSuccess = true;
                            }
                        }
                    }

                    lock (cacheLock)
                    {
                        // make sure the lock didn't just get removed
                        if (cacheLock == mapLock[key])
                        {
                            return isSuccess = cacheLock.Assign(waitTimeMillis);
                        }
                    }
                }
            }
            finally
            {
                if (!isSuccess)
                {
                    if (cacheLock != null && cacheLock.IsDiscardable)
                    {
                        mapLock.Remove(key);
                    }
                    gateMap.Exit();
                }
            }
        }

        /// <summary>
        /// Attempt to lock the specified item and return immediately.
        /// </summary>
        /// <remarks>
        /// This method behaves exactly as if it simply performs the call
        /// <b>Lock(key, 0)</b>.
        /// </remarks>
        /// <param name="key">
        /// Key being locked.
        /// </param>
        /// <returns>
        /// <b>true</b> if the item was successfully locked; <b>false</b>
        /// otherwise.
        /// </returns>
        public virtual bool Lock(object key)
        {
            return Lock(key, 0);
        }

        /// <summary>
        /// Unlock the specified item.
        /// </summary>
        /// <remarks>
        /// The item doesn't have to exist to be <i>unlocked</i>.
        /// If the item is currently locked, only the <i>holder</i> of the
        /// lock could successfully unlock it.
        /// </remarks>
        /// <param name="key">
        /// Key being unlocked.
        /// </param>
        /// <returns>
        /// <b>true</b> if the item was successfully unlocked; <b>false</b>
        /// otherwise.
        /// </returns>
        public virtual bool Unlock(object key)
        {
            IDictionary mapLock = m_mapLock;
            Gate        gateMap = m_gateMap;

            if (key == LockScope.LOCK_ALL)
            {
                try
                {
                    gateMap.Open();
                    return true;
                }
                catch (SystemException)
                {
                    return false;
                }
            }

            bool isReleased = true;
            while (true)
            {
                CacheLock cacheLock = mapLock[key] as CacheLock;
                if (cacheLock == null)
                {
                    break;
                }

                lock (cacheLock)
                {
                    if (mapLock[key] == cacheLock)
                    {
                        isReleased = cacheLock.Release();
                        if (cacheLock.IsDiscardable)
                        {
                            mapLock.Remove(key);
                        }
                        break;
                    }
                }
            }

            try
            {
                gateMap.Exit();
            }
            catch (SystemException)
            {
            }

            return isReleased;
        }

        #endregion

        #region IQueryCache implementation

        /// <summary>
        /// Return a collection of the keys contained in this cache for
        /// entries that satisfy the criteria expressed by the filter.
        /// </summary>
        /// <param name="filter">
        /// The <see cref="IFilter"/> object representing the criteria that
        /// the entries of this cache should satisfy.
        /// </param>
        /// <returns>
        /// A collection of keys for entries that satisfy the specified
        /// criteria.
        /// </returns>
        public virtual object[] GetKeys(IFilter filter)
        {
            return InvocableCacheHelper.Query(this, IndexMap, filter,
                    InvocableCacheHelper.QueryType.Keys, false, null);
        }

        /// <summary>
        /// Return a collection of the values contained in this cache for
        /// entries that satisfy the criteria expressed by the filter.
        /// </summary>
        /// <param name="filter">
        /// The <see cref="IFilter"/> object representing the criteria that
        /// the entries of this cache should satisfy.
        /// </param>
        /// <returns>
        /// A collection of the values for entries that satisfy the specified
        /// criteria.
        /// </returns>
        public virtual object[] GetValues(IFilter filter)
        {
            return InvocableCacheHelper.Query(this, IndexMap, filter,
                    InvocableCacheHelper.QueryType.Values, false, null);
        }

        /// <summary>
        /// Return a collection of the values contained in this cache for
        /// entries that satisfy the criteria expressed by the filter.
        /// </summary>
        /// <remarks>
        /// It is guaranteed that enumerator will traverse the array in such
        /// a way that the values come up in ascending order, sorted by
        /// the specified comparer or according to the
        /// <i>natural ordering</i>.
        /// </remarks>
        /// <param name="filter">
        /// The <see cref="IFilter"/> object representing the criteria that
        /// the entries of this cache should satisfy.
        /// </param>
        /// <param name="comparer">
        /// The <b>IComparable</b> object which imposes an ordering on
        /// entries in the resulting collection; or <c>null</c> if the
        /// entries' values natural ordering should be used.
        /// </param>
        /// <returns>
        /// A collection of entries that satisfy the specified criteria.
        /// </returns>
        public virtual object[] GetValues(IFilter filter, IComparer comparer)
        {
            return InvocableCacheHelper.Query(this, IndexMap, filter,
                    InvocableCacheHelper.QueryType.Values, true, comparer);
        }

        /// <summary>
        /// Return a collection of the entries contained in this cache
        /// that satisfy the criteria expressed by the filter.
        /// </summary>
        /// <param name="filter">
        /// The <see cref="IFilter"/> object representing the criteria that
        /// the entries of this cache should satisfy.
        /// </param>
        /// <returns>
        /// A collection of entries that satisfy the specified criteria.
        /// </returns>
        public virtual ICacheEntry[] GetEntries(IFilter filter)
        {
            return (ICacheEntry[]) InvocableCacheHelper.Query(this, IndexMap,
                    filter, InvocableCacheHelper.QueryType.Entries, false, null);
        }

        /// <summary>
        /// Return a collection of the entries contained in this cache
        /// that satisfy the criteria expressed by the filter.
        /// </summary>
        /// <remarks>
        /// <p>
        /// It is guaranteed that enumerator will traverse the array in such
        /// a way that the entry values come up in ascending order, sorted by
        /// the specified comparer or according to the
        /// <i>natural ordering</i>.</p>
        /// </remarks>
        /// <param name="filter">
        /// The <see cref="IFilter"/> object representing the criteria that
        /// the entries of this cache should satisfy.
        /// </param>
        /// <param name="comparer">
        /// The <b>IComparable</b> object which imposes an ordering on
        /// entries in the resulting collection; or <c>null</c> if the
        /// entries' values natural ordering should be used.
        /// </param>
        /// <returns>
        /// A collection of entries that satisfy the specified criteria.
        /// </returns>
        public virtual ICacheEntry[] GetEntries(IFilter filter, IComparer comparer)
        {
            return (ICacheEntry[]) InvocableCacheHelper.Query(this, IndexMap,
                    filter, InvocableCacheHelper.QueryType.Entries, true, comparer);
        }

        /// <summary>
        /// Add an index to this IQueryCache.
        /// </summary>
        /// <remarks>
        /// This allows to correlate values stored in this
        /// <i>indexed cache</i> (or attributes of those values) to the
        /// corresponding keys in the indexed dictionary and increase the
        /// performance of <b>GetKeys</b> and <b>GetEntries</b> methods.
        /// </remarks>
        /// <param name="extractor">
        /// The <see cref="IValueExtractor"/> object that is used to extract
        /// an indexable object from a value stored in the indexed
        /// cache. Must not be <c>null</c>.
        /// </param>
        /// <param name="isOrdered">
        /// <b>true</b> if the contents of the indexed information should be
        /// ordered; <b>false</b> otherwise.
        /// </param>
        /// <param name="comparer">
        /// The <b>IComparer</b> object which imposes an ordering on entries
        /// in the indexed cache; or <c>null</c> if the entries' values
        /// natural ordering should be used.
        /// </param>
        public virtual void AddIndex(IValueExtractor extractor, bool isOrdered, IComparer comparer)
        {
            InvocableCacheHelper.AddIndex(extractor, isOrdered, comparer, this, 
                    EnsureIndexMap());
        }

        /// <summary>
        /// Remove an index from this IQueryCache.
        /// </summary>
        /// <param name="extractor">
        /// The <see cref="IValueExtractor"/> object that is used to extract
        /// an indexable object from a value stored in the cache.
        /// </param>
        public virtual void RemoveIndex(IValueExtractor extractor)
        {
            InvocableCacheHelper.RemoveIndex(extractor, this, IndexMap);
        }

        /// <summary>
        /// Obtain the IDictionary of indexes maintained by this cache. 
        /// </summary>
        /// <returns>
        /// The IDictionary of indexes maintained by this cache.
        /// </returns>
        protected virtual IDictionary EnsureIndexMap()
        {
            AcquireWriteLock();
            try
            {
                IDictionary indexMap = m_indexMap;
                if (indexMap == null)
                {
                    m_indexMap = indexMap = new SynchronizedDictionary();
                }
                return indexMap;
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        #endregion

        #region IInvocableCache implementation

        /// <summary>
        /// Invoke the passed <see cref="IEntryProcessor"/> against the entry
        /// specified by the passed key, returning the result of the
        /// invocation.
        /// </summary>
        /// <param name="key">
        /// The key to process; it is not required to exist within the
        /// dictionary.
        /// </param>
        /// <param name="agent">
        /// The <b>IEntryProcessor</b> to use to process the specified key.
        /// </param>
        /// <returns>
        /// The result of the invocation as returned from the
        /// <b>IEntryProcessor</b>.
        /// </returns>
        public virtual object Invoke(object key, IEntryProcessor agent)
        {
            return InvocableCacheHelper.InvokeLocked(this, EnsureEntry(key), agent);
        }

        /// <summary>
        /// Invoke the passed <see cref="IEntryProcessor"/> against the
        /// entries specified by the passed keys, returning the result of the
        /// invocation for each.
        /// </summary>
        /// <param name="keys">
        /// The keys to process; these keys are not required to exist within
        /// the dictionary.
        /// </param>
        /// <param name="agent">
        /// The <b>IEntryProcessor</b> to use to process the specified keys.
        /// </param>
        /// <returns>
        /// A dictionary containing the results of invoking the
        /// <b>IEntryProcessor</b> against each of the specified keys.
        /// </returns>
        public virtual IDictionary InvokeAll(ICollection keys, IEntryProcessor agent)
        {
            return InvocableCacheHelper.InvokeAllLocked(this, EnsureEntryCollection(keys), agent);
        }

        /// <summary>
        /// Invoke the passed <see cref="IEntryProcessor"/> against the set
        /// of entries that are selected by the given <see cref="IFilter"/>,
        /// returning the result of the invocation for each.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Unless specified otherwise, IInvocableCache implementations
        /// will perform this operation in two steps: (1) use the filter to
        /// retrieve a matching entry collection; (2) apply the agent to
        /// every filtered entry. This algorithm assumes that the agent's
        /// processing does not affect the result of the specified filter
        /// evaluation, since the filtering and processing could be
        /// performed in parallel on different threads.</p>
        /// <p>
        /// If this assumption does not hold, the processor logic has to be
        /// idempotent, or at least re-evaluate the filter. This could be
        /// easily accomplished by wrapping the processor with the
        /// <see cref="ConditionalProcessor"/>.</p>
        /// </remarks>
        /// <param name="filter">
        /// An <see cref="IFilter"/> that results in the collection of keys to
        /// be processed.
        /// </param>
        /// <param name="agent">
        /// The <see cref="IEntryProcessor"/> to use to process the specified
        /// keys.
        /// </param>
        /// <returns>
        /// A dictionary containing the results of invoking the
        /// <b>IEntryProcessor</b> against the keys that are selected by the
        /// given <b>IFilter</b>.
        /// </returns>
        public virtual IDictionary InvokeAll(IFilter filter, IEntryProcessor agent)
        {
            return InvokeAll(GetKeys(filter), agent);
        }

        /// <summary>
        /// Perform an aggregating operation against the entries specified by
        /// the passed keys.
        /// </summary>
        /// <param name="keys">
        /// The collection of keys that specify the entries within this cache
        /// to aggregate across.
        /// </param>
        /// <param name="agent">
        /// The <see cref="IEntryAggregator"/> that is used to aggregate
        /// across the specified entries of this dictionary.
        /// </param>
        /// <returns>
        /// The result of the aggregation.
        /// </returns>
        public virtual object Aggregate(ICollection keys, IEntryAggregator agent)
        {
            return agent.Aggregate(EnsureEntryCollection(keys));
        }

        /// <summary>
        /// Perform an aggregating operation against the collection of
        /// entries that are selected by the given <b>IFilter</b>.
        /// </summary>
        /// <param name="filter">
        /// an <see cref="IFilter"/> that is used to select entries within
        /// this cache to aggregate across.
        /// </param>
        /// <param name="agent">
        /// The <see cref="IEntryAggregator"/> that is used to aggregate
        /// across the selected entries of this dictionary.
        /// </param>
        /// <returns>
        /// The result of the aggregation.
        /// </returns>
        public virtual object Aggregate(IFilter filter, IEntryAggregator agent)
        {
            return Aggregate(GetKeys(filter), agent);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Creates an <see cref="ICacheEntry"/>.
        /// </summary>
        /// <param name="key">
        /// Entry key.
        /// </param>
        /// <param name="value">
        /// Entry value.
        /// </param>
        /// <returns>
        /// <see cref="ICacheEntry"/> instance.
        /// </returns>
        protected virtual ICacheEntry CreateEntry(object key, object value)
        {
            return new Entry(this, key, value);
        }

        /// <summary>
        /// Add new cache entry and raise Inserted event if necessary.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding the write lock.
        /// </remarks>
        /// <param name="key">Key to add.</param>
        /// <param name="value">Value to add.</param>
        /// <returns>Added entry.</returns>
        protected virtual Entry AddInternal(object key, object value)
        {
            Entry entry = value is Entry
                          ? (Entry) value
                          : (Entry) CreateEntry(key, value);

            Storage.Add(key, entry);

            // issue add notification
            if (HasListeners())
            {
                DispatchEvent(InstantiateCacheEvent(CacheEventType.Inserted,
                                                    key, null, entry.Value));
            }
            return entry;
        }

        /// <summary>
        /// Locate an <see cref="Entry"/> in the cache based on its key.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the <b>Entry</b> has expired, it is removed from the cache.
        /// </p>
        /// <p>
        /// Unlike the <see cref="GetEntry"/> method, this method does not
        /// flush the cache (if necessary) or update cache statistics.
        /// </p>
        /// <p>
        /// This method should only be called while holding the write lock.
        /// </p>
        /// </remarks>
        /// <param name="key">
        /// The key object to search for.
        /// </param>
        /// <returns>
        /// The <b>Entry</b> or <c>null</c> if the entry is not found in the
        /// cache or has expired.
        /// </returns>
        protected internal virtual Entry GetEntryInternal(object key)
        {
            Entry entry = Storage[key] as Entry;
            if (entry != null && entry.IsExpired)
            {
                RemoveExpired(entry, true);
                entry = null;
            }
            return entry;
        }

        /// <summary>
        /// Locate an <see cref="Entry"/> in the cache based on its key.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Unlike the <see cref="GetEntryInternal"/> method, this method does
        /// not remove expired entries from the cache.</p>p
        /// <p>
        /// This method should only be called while holding a read or write
        /// lock.
        /// </p>
        /// </remarks>
        /// <param name="key">
        /// The key object to search for.
        /// </param>
        /// <returns>
        /// The <b>Entry</b> or <c>null</c> if the entry is not found in the
        /// cache.
        /// </returns>
        protected internal virtual Entry PeekEntryInternal(object key)
        {
            return Storage[key] as Entry;
        }

        /// <summary>
        /// Remove an entry.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding the write lock.
        /// </remarks>
        /// <param name="entry">
        /// The expired cache entry.
        /// </param>
        /// <param name="removeInternal">
        /// <b>true</b> if the cache entry still needs to be removed from the
        /// cache.
        /// </param>
        protected virtual void RemoveInternal(Entry entry, bool removeInternal)
        {
            entry.Discard();
            if (removeInternal)
            {
                Storage.Remove(entry.Key);
            }
        }

        /// <summary>
        /// Remove an entry because it has expired.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding the write lock.
        /// </remarks>
        /// <param name="entry">
        /// The expired cache entry.
        /// </param>
        /// <param name="removeInternal">
        /// <b>true</b> if the cache entry still needs to be removed from the
        /// cache.
        /// </param>
        protected virtual void RemoveExpired(Entry entry, bool removeInternal)
        {
            long    expiry       = entry.ExpiryMillis;
            bool    fExpired     = expiry != 0 && (expiry & ~0xFFL) < DateTimeUtils.GetSafeTimeMillis();
            KeyMask mask         = CurrentKeyMask;
            bool    fSynthetic   = mask.EnsureSynthetic();
            bool    fPrevExpired = fExpired ? mask.EnsureExpired() : false;
            try
            {
                RemoveInternal(entry, removeInternal);
            }
            finally
            {
                mask.IsSynthetic = fSynthetic;
                mask.IsExpired   = fPrevExpired;
            }
        }

        /// <summary>
        /// Check if the cache is too big, and if it is prune it by
        /// discarding the lowest priority cache entries.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding the write lock.
        /// </remarks>
        protected virtual void CheckSize()
        {
            // check if pruning is required
            if (m_curUnits > m_maxUnits)
            {
                Prune();
            }
        }

        /// <summary>
        /// Check if the cache needs to be flushed.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding a read or write 
        /// lock.
        /// </remarks>
        /// <returns>
        /// <b>true</b> if it is time to flush the cache.
        /// </returns>
        protected virtual bool IsFlushRequired()
        {
            return DateTimeUtils.GetSafeTimeMillis() > m_nextFlush;
        }

        /// <summary>
        /// Flush the cache if it needs to be flushed.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding the write lock.
        /// </remarks>
        protected virtual void CheckFlush()
        {
            if (IsFlushRequired())
            {
                Evict();
            }
        }

        /// <summary>
        /// Prune the cache by discarding the lowest priority cache entries.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding the write lock.
        /// </remarks>
        protected virtual void Prune()
        {
            if (m_curUnits < m_maxUnits)
            {
                return;
            }

            // COH-764: try just throwing away expired stuff
            Evict();
            if (m_curUnits < m_maxUnits)
            {
                return;
            }

            // start a new eviction cycle
            long start = DateTimeUtils.GetSafeTimeMillis();
            long curr  = m_curUnits;
            long min   = m_pruneUnits;

            EvictionPolicyType type = m_evictionType;
            switch (type)
            {
                case EvictionPolicyType.Hybrid:
                    {
                        // calculate a rough average number of touches that each
                        // entry should expect to have
                        ICacheStatistics stats = m_stats;
                        m_avgTouch =
                            (int) ((stats.TotalPuts + stats.TotalGets) 
                                / ((Storage.Count + 1) * (stats.CachePrunes + 1)));

                        // sum the entries' units per priority
                        long[] units = new long[11];
                        foreach (Entry entry in Entries)
                        {
                            long entryUnits = entry.Units;
                            try
                            {
                                units[entry.Priority] += entryUnits;
                            }
                            catch (IndexOutOfRangeException)
                            {
                                units[Math.Max(0, Math.Min(entry.Priority, 10))] += entryUnits;
                            }
                        }

                        long total         = 0;
                        int  prunePriority = 0;
                        while (prunePriority <= 10)
                        {
                            total += units[prunePriority];
                            if (total > min)
                            {
                                break;
                            }
                            ++prunePriority;
                        }

                        // determine the number at the cut-off priority that must be pruned
                        long additional = Math.Max(0, total - min);

                        // determine a list of entries that should be removed
                        IList entriesToRemove = new ArrayList();
                        for (ICacheEnumerator en = GetEnumerator(); curr > min && en.MoveNext(); )
                        {
                            Entry entry = en.Entry as Entry;
                            if (entry == null)
                            {
                                continue;
                            }
                            
                            int priority = entry.Priority;
                            if (priority >= prunePriority)
                            {
                                long unitsCount = entry.Units;
                                if (priority == prunePriority)
                                {
                                    if (additional <= 0)
                                    {
                                        continue;
                                    }
                                    additional -= unitsCount;
                                }
                                curr -= unitsCount;
                                entriesToRemove.Add(entry);
                            }
                        }

                        // remove entries from the cache
                        foreach (Entry entry in entriesToRemove)
                        {
                            Storage.Remove(entry.Key);
                            RemoveExpired(entry, false);
                        }
                    }
                    break;


                case EvictionPolicyType.LRU:
                case EvictionPolicyType.LFU:
                    {
                        bool           isLRU     = (type == EvictionPolicyType.LRU);
                        LongSortedList longArray = new LongSortedList();
                        
                        foreach (Entry entry in Entries)
                        {
                            long   order = isLRU ? entry.LastTouchMillis : entry.TouchCount;
                            object prev  = longArray[order];
                            longArray[order] = entry;

                            if (prev != null)
                            {
                                // oops, more than one entry with the same order;
                                // make a list of entries
                                IList list;
                                if (prev is IList)
                                {
                                    list = (IList) prev;
                                }
                                else
                                {
                                    list = new ArrayList {prev};
                                }
                                list.Add(entry);
                                longArray[order] = list;
                            }
                        }

                        foreach (object obj in longArray)
                        {
                            if (m_curUnits <= min)
                            {
                                break;
                            }
                            object o = ((DictionaryEntry) obj).Value;
                            if (o is Entry)
                            {
                                Entry entry = o as Entry;
                                RemoveExpired(entry, true);
                            }
                            else
                            {
                                IList list = (IList) o;
                                foreach (Entry entry in list)
                                {
                                    if (m_curUnits <= min)
                                    {
                                        break;
                                    }
                                    RemoveExpired(entry, true);
                                }
                            }
                        }
                    }
                    break;


                case EvictionPolicyType.External:
                    EvictionPolicy.RequestEviction(min);
                    break;
            }

            // reset touch counts
            foreach (Entry entry in Entries)
            {
                // walk all entries
                entry.ResetTouchCount();
            }

            m_stats.RegisterCachePrune(start);
            m_lastPrune = DateTimeUtils.GetSafeTimeMillis();
        }

        /// <summary>
        /// Schedule the next flush.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding the write lock.
        /// </remarks>
        protected virtual void ScheduleFlush()
        {
            int delayMillis = m_flushDelay;
            m_nextFlush = delayMillis == 0
                              ? Int64.MaxValue
                              : DateTimeUtils.GetSafeTimeMillis() + delayMillis;
        }

        /// <summary>
        /// Adjust current size.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding the write lock.
        /// </remarks>
        /// <param name="delta">
        /// Value that current size should be adjusted by.
        /// </param>
        protected virtual void AdjustUnits(int delta)
        {
            m_curUnits += delta;
        }

        /// <summary>
        /// Determine if the <b>LocalCache</b> has any listeners at all.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding the read or write
        /// lock.
        /// </remarks>
        /// <returns>
        /// <b>true</b> if this <b>LocalCache</b> has at least one
        /// <see cref="ICacheListener"/>.
        /// </returns>
        protected virtual bool HasListeners()
        {
            // m_listenerSupport defaults to null, and it is reset to null when
            // the last listener unregisters
            return m_listenerSupport != null;
        }

        /// <summary>
        /// Dispatch the passed event.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding the read lock.
        /// </remarks>
        /// <param name="evt">
        /// A <see cref="CacheEventArgs"/> object.
        /// </param>
        protected virtual void DispatchEvent(CacheEventArgs evt)
        {
            CacheListenerSupport listenerSupport = m_listenerSupport;
            if (listenerSupport != null)
            {
                listenerSupport.FireEvent(evt, false);
            }
        }

        /// <summary>
        /// Factory pattern: instantiate a new <see cref="CacheEventArgs"/>
        /// corresponding to the specified parameters.
        /// </summary>
        /// <param name="type">
        /// This event's type, one of <see cref="CacheEventType"/>
        /// values.
        /// </param>
        /// <param name="key">
        /// The key into the cache.
        /// </param>
        /// <param name="valueOld">
        /// The old value (for update and delete events).
        /// </param>
        /// <param name="valueNew">
        /// The new value (for insert and update events).
        /// </param>
        /// <returns>
        /// A new instance of the <b>CacheEventArgs</b> class (or a
        /// subclass thereof).
        /// </returns>
        protected virtual CacheEventArgs InstantiateCacheEvent(CacheEventType type, object key, object valueOld,
                                                               object valueNew)
        {
            return new CacheEventArgs(this, type, key, valueOld, valueNew, CurrentKeyMask.IsSynthetic, CacheEventArgs.TransformationState.TRANSFORMABLE, false, CurrentKeyMask.IsExpired);
        }

        /// <summary>
        /// Factory pattern: Instantiate an internal
        /// <see cref="ICacheListener"/> to listen to this cache and report
        /// changes to the <see cref="ICacheStore"/>.
        /// </summary>
        /// <returns>
        /// A new <b>ICacheListener</b> instance.
        /// </returns>
        protected virtual ICacheListener InstantiateInternalListener()
        {
            return new InternalListener(this);
        }

        /// <summary>
        /// Configure the eviction type and policy.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding the write lock.
        /// </remarks>
        /// <param name="type">
        /// One of the <see cref="EvictionPolicyType"/> enum values.
        /// </param>
        /// <param name="policy">
        /// An external eviction policy, or <c>null</c>.
        /// </param>
        protected virtual void ConfigureEviction(EvictionPolicyType type, IEvictionPolicy policy)
        {
            switch (type)
            {
                case EvictionPolicyType.Hybrid:
                case EvictionPolicyType.LFU:
                case EvictionPolicyType.LRU:
                    break;

                case EvictionPolicyType.External:
                    if (policy == null)
                    {
                        throw new InvalidOperationException(
                            "An attempt was made to set eviction type to " +
                            "EvictionPolicyType.External without providing " +
                            "an external IEvictionPolicy");
                    }
                    break;

                default:
                    throw new ArgumentException("Unknown eviction type: " + type);
            }

            IEvictionPolicy policyPrev = m_policy;
            if (policyPrev is ICacheListener)
            {
                RemoveCacheListener((ICacheListener) policyPrev);
            }

            m_evictionType = type;
            m_policy       = policy;

            if (policy is ICacheListener)
            {
                AddCacheListener((ICacheListener) policy);
            }
        }

        /// <summary>
        /// Configure the unit calculator type and implementation.
        /// </summary>
        /// <remarks>
        /// This method should only be called while holding the write lock.
        /// </remarks>
        /// <param name="type">
        /// One of the <see cref="UnitCalculatorType"/> enum values.
        /// </param>
        /// <param name="calculator">
        /// An external unit calculator, or <c>null</c>.
        /// </param>
        protected virtual void ConfigureUnitCalculator(UnitCalculatorType type, IUnitCalculator calculator)
        {
            switch (type)
            {
                case UnitCalculatorType.Fixed:
                    if (type == m_calculatorType)
                    {
                        // nothing to do
                        return;
                    }
                    break;

                case UnitCalculatorType.External:
                    if (calculator == null)
                    {
                        throw new InvalidOperationException(
                            "An attempt was made to set the unit calculator " + 
                            "type to UnitCalculatorType.External without " +
                            "providing an external IUnitCalculator");
                    }
                    if (UnitCalculatorType.External == m_calculatorType 
                        && Equals(calculator, m_calculator))
                    {
                        // nothing to do
                        return;
                    }
                    break;

                default:
                    throw new ArgumentException("unknown unit calculator type: " + type);
            }

            m_calculatorType = type;
            m_calculator     = calculator;

            // recalculate unit costs

            foreach (Entry entry in Entries)
            {
                int units = entry.CalculateUnits(entry.Value);

                // update both the entry unit count and total unit count
                entry.Units = units;
            }
        }

        /// <summary>
        /// Create a <see cref="Entry"/> object for the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to create an entry for; the key is not required to exist
        /// within the cache.
        /// </param>
        /// <returns>
        /// A <b>Entry</b> object.
        /// </returns>
        protected virtual IInvocableCacheEntry EnsureEntry(object key)
        {
            IInvocableCacheEntry entry =  GetEntry(key) 
                                       ?? CreateEntry(key, null) as Entry;
            return entry;
        }

        /// <summary>
        /// Create an array of <see cref="Entry"/> objects for the specified
        /// <see cref="ICache"/> and the keys collection.
        /// </summary>
        /// <param name="keys">
        /// Collection of keys to create entries for; these keys are not
        /// required to exist within the cache.
        /// </param>
        /// <returns>
        /// An array of <b>Entry</b> objects.
        /// </returns>
        protected virtual IInvocableCacheEntry[] EnsureEntryCollection(ICollection keys)
        {
            IInvocableCacheEntry[] entries = new IInvocableCacheEntry[keys.Count];
            int i = 0;
            foreach (object key in keys)
            {
                entries[i++] = EnsureEntry(key);
            }
            return entries;
        }

        /// <summary>
        /// Utility method to support clear and truncation operations.
        /// </summary>
        /// <param name="fNotifyObservers">
        /// <b>true</b> if observers should be notified, otherwise <b>false</b>.
        /// </param>
        protected virtual void ClearInternal(bool fNotifyObservers)
        {
            // this method is only called as a result of a call from the cache
            // consumer, not from any internal eviction etc.

            AcquireWriteLock();
            try
            {
                if (fNotifyObservers)
                {
                    // if there is an ICacheStore, tell it that all entries are
                    // being erased
                    ICacheStore store = m_store;
                    if (store != null)
                    {
                        store.EraseAll(Keys);
                    }

                    // notify cache entries of their impending removal
                    foreach (Entry entry in Entries)
                    {
                        entry.Discard();
                    }

                    // verify that the cache maintains its data correctly
                    if (m_curUnits != 0)
                    {
                        // soft assertion
                        CacheFactory.Log("Invalid unit count after Clear: " + m_curUnits,
                                         CacheFactory.LogLevel.Always);
                        m_curUnits = 0;
                    }
                }
               
                // reset the cache storage
                Storage.Clear();

                // reset hit/miss stats
                ResetHitStatistics();
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Returns a string representation of this LocalCache object.
        /// </summary>
        /// <returns>
        /// A string representation of this LocalCache object.
        /// </returns>
        public override string ToString()
        {
            AcquireReadLock();
            try
            {
                StringBuilder sb = new StringBuilder("LocalCache{");

                int i = 0;
                foreach (ICacheEntry entry in Entries)
                {
                    sb.Append('[')
                        .Append(i++)
                        .Append("]: ")
                        .Append(entry);
                }

                sb.Append('}');
                return sb.ToString();
            }
            finally
            {
                ReleaseReadLock();
            }
        }

        #endregion

        #region Inner class: Entry

        /// <summary>
        /// A holder for a cached value.
        /// </summary>
        public class Entry : IInvocableCacheEntry, IConfigurableCacheEntry
        {
            #region Properties

            /// <summary>
            /// Parent cache.
            /// </summary>
            /// <value>
            /// Parent cache.
            /// </value>
            public virtual LocalCache Cache
            {
                get { return m_cache; }
                set { m_cache = value; }
            }

            /// <summary>
            /// Gets the key corresponding to this entry.
            /// </summary>
            /// <value>
            /// The key corresponding to this entry; may be <c>null</c> if the
            /// underlying dictionary supports <c>null</c> keys.
            /// </value>
            public virtual object Key
            {
                get { return m_key; }
            }

            /// <summary>
            /// Gets or sets the value corresponding to this entry.
            /// </summary>
            /// <value>
            /// The value corresponding to this entry; may be <c>null</c> if the
            /// value is <c>null</c> or if the entry does not exist in the
            /// cache.
            /// </value>
            public virtual object Value
            {
                get { return m_value; }
                set
                {
                    // optimization - verify that the entry is still valid
                    if (m_units == -1)
                    {
                        // entry is discarded; avoid exception
                        m_value = value;
                    }
                    else
                    {
                        // perform the entry update
                        int newUnits = CalculateUnits(value);
                        LocalCache cache = Cache;

                        cache.AcquireWriteLock();
                        try
                        {
                            int oldUnits = m_units;
                            if (oldUnits == -1)
                            {
                                // entry is discarded; avoid repetitive events
                                m_value = value;
                                return;
                            }

                            if (newUnits != oldUnits)
                            {
                                cache.AdjustUnits(newUnits - oldUnits);
                                m_units = newUnits;
                            }
                            Object oldValue = m_value;
                            m_value = value;

                            ScheduleExpiry();

                            // if this entry's key is present in the cache and is
                            // not expired, raise an CacheEventType.Updated event
                            if (cache.GetEntryInternal(Key) != null)
                            {
                                // issue update notification
                                if (cache.HasListeners())
                                {
                                    cache.DispatchEvent(
                                        cache.InstantiateCacheEvent(CacheEventType.Updated, Key, oldValue, value));
                                }
                            }
                        }
                        finally
                        {
                            cache.ReleaseWriteLock();
                        }
                    }
                }
            }

            /// <summary>
            /// Store the value corresponding to this entry.
            /// </summary>
            /// <remarks>
            /// <p>
            /// If the entry does not exist, then the entry will be created by
            /// invoking this method, even with a <c>null</c> value (assuming the
            /// cache supports <c>null</c> values).</p>
            /// <p>
            /// Unlike the property <see cref="Value"/>, this method does not
            /// return the previous value, and as a result may be significantly
            /// less expensive (in terms of cost of execution) for certain
            /// cache implementations.</p>
            /// </remarks>
            /// <param name="value">
            /// The new value for this entry.
            /// </param>
            /// <param name="isSynthetic">
            /// Pass <b>true</b> only if the insertion into or modification of
            /// the cache should be treated as a synthetic event.
            /// </param>
            public virtual void SetValue(object value, bool isSynthetic)
            {
                m_cache.Insert(m_key, value);
                m_value = value;
            }

            /// <summary>
            /// Determine if this entry exists in the cache.
            /// </summary>
            /// <remarks>
            /// If the entry is not present, it can be created by setting the
            /// <see cref="Value"/> property. If the entry is present,
            /// it can be destroyed by calling <see cref="Remove"/>.
            /// </remarks>
            /// <value>
            /// <b>true</b> if this entry exists in the containing cache.
            /// </value>
            public virtual bool IsPresent
            {
                get
                {
                    // m_oValue could be null in two cases:
                    // (a) the actual value is null;
                    // (b) the value has been removed
                    // If the map is not specified, an entry is assumed to exist
                    object value = m_value;
                    return (value != UNKNOWN && value != null)
                           || m_cache == null || m_cache.Contains(m_key);
                }
            }

            /// <summary>
            /// Extract a value out of the entry's value.
            /// </summary>
            /// <remarks>
            /// Calling this method is semantically equivalent to
            /// <b>extractor.Extract(entry.Value)</b>, but this method may be
            /// significantly less expensive because the resultant value may be
            /// obtained from a forward index, for example.
            /// </remarks>
            /// <param name="extractor">
            /// An <see cref="IValueExtractor"/> to apply to the entry's value
            /// </param>
            /// <returns>
            /// The extracted value.
            /// </returns>
            public virtual object Extract(IValueExtractor extractor)
            {
                return InvocableCacheHelper.ExtractFromEntry(extractor, this);
            }

            /// <summary>
            /// Update the entry's value.
            /// </summary>
            /// <remarks>
            /// Calling this method is semantically equivalent to:
            /// <pre>
            /// object target = entry.Value;
            /// updater.Update(target, value);
            /// entry.Value = target;
            /// </pre>
            /// The benefit of using this method is that it may allow the entry
            /// implementation to significantly optimize the operation, such as
            /// for purposes of delta updates and backup maintenance.
            /// </remarks>
            /// <param name="updater">
            /// An <see cref="IValueUpdater"/> used to modify the entry's value.
            /// </param>
            /// <param name="value">
            /// Value to update target object to.
            /// </param>
            public virtual void Update(IValueUpdater updater, object value)
            {
                object target = Value;
                updater.Update(target, value);
                SetValue(target, false);
            }

            /// <summary>
            /// Remove this entry from the cache if it is present in the cache.
            /// </summary>
            /// <remarks>
            /// <p>
            /// This method supports both the operation corresponding to
            /// <b>IDictionary.Remove</b> as well as synthetic operations such as
            /// eviction. If the containing cache does not differentiate between
            /// the two, then this method will always be identical to
            /// <tt>IInvocableCache.Remove(Key)</tt>.</p>
            /// </remarks>
            /// <param name="isSynthetic">
            /// Pass <b>true</b> only if the removal from the dictionary should
            /// be treated as a synthetic event.
            /// </param>
            public virtual void Remove(bool isSynthetic)
            {
                ICache cache = m_cache;
                object key   = m_key;

                if (isSynthetic && cache is LocalCache)
                {
                    ((LocalCache) cache).Evict(key);
                }
                else
                {
                    cache.Remove(key);
                }
                m_value = null;
            }

            /// <summary>
            /// Determine when the cache entry was created.
            /// </summary>
            /// <value>
            /// The date/time value, in millis, when the entry was created.
            /// </value>
            public virtual long CreatedMillis
            {
                get { return m_created; }
            }

            /// <summary>
            /// Determine when the cache entry will expire, if ever.
            /// </summary>
            /// <remarks>
            /// Note that if the cache is configured for automatic expiry,
            /// each subsequent update to this cache entry will reschedule
            /// the expiry time.
            /// </remarks>
            /// <value>
            /// The date/time value, in millis, when the entry will (or did)
            /// expire; zero indicates no expiry.
            /// </value>
            public virtual long ExpiryMillis
            {
                get { return m_expiry; }
                set { m_expiry = value; }
            }

            /// <summary>
            /// Determine when the cache entry was last touched.
            /// </summary>
            /// <value>
            /// The date/time value, in millis, when the entry was most
            /// recently touched.
            /// </value>
            public virtual long LastTouchMillis
            {
                get { return m_lastUse; }
            }

            /// <summary>
            /// Calculate a cache priority.
            /// </summary>
            /// <value>
            /// A value between 0 and 10, 0 being the highest priority.
            /// </value>
            public virtual int Priority
            {
                get
                {
                    // calculate an LRU score - how recently was the entry used?
                    long prune = m_cache.m_lastPrune;
                    long touch = m_lastUse;
                    int scoreLRU = 0;
                    if (touch > prune)
                    {
                        // measure recentness against the window of time since the
                        // last prune
                        long current       = DateTimeUtils.GetSafeTimeMillis();
                        long millisDormant = current - touch;
                        long millisWindow  = current - prune;

                        double pct = (millisWindow - millisDormant) / (1.0 + millisWindow);
                        scoreLRU = 1 + IndexOfMSB((int) ((pct * pct * 64)));
                    }

                    // calculate "frequency" - how often has the entry been used?
                    int uses = m_uses;
                    int scoreLFU = 0;
                    if (uses > 0)
                    {
                        scoreLFU = 1;
                        int avg = m_cache.m_avgTouch;
                        if (uses > avg)
                        {
                            ++scoreLFU;
                        }

                        int adj = (uses << 1) - avg;
                        if (adj > 0)
                        {
                            scoreLFU += 1 + Math.Min(4, IndexOfMSB((int) ((adj << 3)/(1.0 + avg))));
                        }
                    }

                    return Math.Max(0, 10 - scoreLRU - scoreLFU);
                }
            }

            /// <summary>
            /// Reset the number of times that the cache entry has been
            /// touched.
            /// </summary>
            /// <remarks>
            /// The touch count does not get reset to zero, but rather to
            /// a fraction of its former self; this prevents long lived items
            /// from gaining an unasailable advantage in the eviction
            /// process.
            /// </remarks>
            /// <since>Coherence 3.5</since>
            public void ResetTouchCount()
            {
                int uses = m_uses;
                if (uses > 0)
                {
                    m_uses = Math.Max(1, uses >> 4);
                }
            }

            /// <summary>
            /// Determine the number of times that the cache entry has been
            /// touched.
            /// </summary>
            /// <value>
            /// The number of times that the cache entry has been touched.
            /// </value>
            public virtual int TouchCount
            {
                get { return m_uses; }
            }

            /// <summary>
            /// Determine the number of cache units used by this Entry.
            /// </summary>
            /// <value>
            /// An integer value 0 or greater, with a larger value
            /// signifying a higher cost; -1 implies that the Entry
            /// has been discarded.
            /// </value>
            public virtual int Units
            {
                get { return m_units; }
                set
                {
                    LocalCache cache = Cache;

                    cache.AcquireWriteLock();
                    try
                    {
                        int oldUnits = m_units;
                        if (oldUnits == -1)
                        {
                            // entry is discarded; avoid exception
                            return;
                        }

                        if (value != oldUnits)
                        {
                            cache.AdjustUnits(value - oldUnits);
                            m_units = value;
                        }
                    }
                    finally
                    {
                        cache.ReleaseWriteLock();
                    }
                }
            }

            /// <summary>
            /// Determine if this entry has already been discarded from the
            /// cache.
            /// </summary>
            /// <value>
            /// <b>true</b> if this entry has been discarded.
            /// </value>
            protected virtual bool IsDiscarded
            {
                get { return m_units == -1; }
            }

            /// <summary>
            /// Determine if the cache entry has expired.
            /// </summary>
            /// <value>
            /// <b>true</b> if the cache entry was subject to automatic
            /// expiry and the current time is greater than the entry's
            /// expiry time.
            /// </value>
            public virtual bool IsExpired
            {
                get
                {
                    long expiry = m_expiry;
                    return expiry != 0 && expiry < DateTimeUtils.GetSafeTimeMillis();
                }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Construct the cacheable entry that holds the cached value.
            /// </summary>
            /// <param name="localCache">
            /// The local cache for this entry.
            /// </param>
            /// <param name="key">
            /// The key of this entry.
            /// </param>
            /// <param name="value">
            /// The value of this entry.
            /// </param>
            public Entry(LocalCache localCache, object key, object value)
            {
                m_cache   = localCache;
                m_key     = key;
                Value     = value;
                m_lastUse = m_created = DateTimeUtils.GetSafeTimeMillis();
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Called each time the entry is accessed or modified.
            /// </summary>
            public virtual void Touch()
            {
                ++m_uses;
                m_lastUse = DateTimeUtils.GetSafeTimeMillis();

                IEvictionPolicy policy = Cache.EvictionPolicy;
                if (policy != null)
                {
                    policy.EntryTouched(this);
                }
            }

            /// <summary>
            /// Reschedule the cache entry expiration.
            /// </summary>
            protected virtual void ScheduleExpiry()
            {
                long expiry = 0L;
                int  delay  = Cache.ExpiryDelay;

                if (delay > 0)
                {
                    expiry = DateTimeUtils.GetSafeTimeMillis() + delay;
                }

                ExpiryMillis = expiry;
            }

            /// <summary>
            /// Called to inform the Entry that it is no longer used.
            /// </summary>
            /// <remarks>
            /// This method should only be called while holding the LocalCache
            /// write lock.
            /// </remarks>
            protected internal virtual void Discard()
            {
                if (!IsDiscarded)
                {
                    int units = m_units;
                    if (units == -1)
                    {
                        // entry is discarded; avoid repetitive events
                        return;
                    }

                    LocalCache cache = Cache;

                    if (units > 0)
                    {
                        cache.AdjustUnits(-units);
                    }

                    m_units = -1;

                    // issue remove notification
                    if (cache.HasListeners())
                    {
                        cache.DispatchEvent(cache.InstantiateCacheEvent(CacheEventType.Deleted, Key, Value, null));
                    }
                }
            }

            /// <summary>
            /// Calculate a cache cost for the specified object.
            /// </summary>
            /// <remarks>
            /// The default implementation uses the unit calculator type of
            /// the containing cache.
            /// </remarks>
            /// <param name="value">
            /// The cache value to evaluate for unit cost.
            /// </param>
            /// <returns>
            /// An integer value 0 or greater, with a larger value signifying
            /// a higher cost.
            /// </returns>
            public virtual int CalculateUnits(object value)
            {
                LocalCache cache = Cache;
                object     key   = Key;

                switch (cache.CalculatorType)
                {
                    case UnitCalculatorType.External:
                        return cache.UnitCalculator.CalculateUnits(key, value);

                    case UnitCalculatorType.Fixed:
                    default:
                        return 1;
                }
            }

            /// <summary>
            /// Determine the most significant bit of the passed integral
            ///  value.
            /// </summary>
            /// <param name="n">
            /// An int.
            /// </param>
            /// <returns>
            /// -1 if no bits are set; otherwise, the bit position
            /// <tt>p</tt> of the most significant bit such that
            /// <tt>1 &lt;&lt; p</tt> is the most significant bit
            /// of <tt>n</tt>
            /// </returns>
            public virtual int IndexOfMSB(int n)
            {
                // see http://aggregate.org/MAGIC/
                n |= (n >> 1);
                n |= (n >> 2);
                n |= (n >> 4);
                n |= (n >> 8);
                n |= (n >> 16);
                n -= ((n >> 1) & 0x55555555);
                n = (((n >> 2) & 0x33333333) + (n & 0x33333333));
                n = (((n >> 4) + n) & 0x0f0f0f0f);
                n += (n >> 8);
                n += (n >> 16);
                n = (n & 0x0000003f) - 1;
                return n;
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Render the cache entry as a string.
            /// </summary>
            /// <returns>
            /// The details about this Entry.
            /// </returns>
            public override string ToString()
            {
                long expiry = ExpiryMillis;

                return GetType().Name + ", priority=" + Priority + ", created="
                        + new DateTime(CreatedMillis * 10000L)
                        + ", last-use=" + new DateTime(LastTouchMillis * 10000L)
                        + ", expiry=" + (expiry == 0
                                             ? "none"
                                             : new DateTime(expiry * 10000L) +
                                               (IsExpired ? " (expired)" : ""))
                        + ", use-count=" + TouchCount + ", units=" + Units;
            }

            #endregion

            #region Conversion Operators

            /// <summary>
            /// Converts Entry to <b>DictionaryEntry</b>.
            /// </summary>
            /// <param name="entry">
            /// Entry instance.
            /// </param>
            /// <returns>
            /// <b>DictionaryEntry</b> with key and value extracted from
            /// the specified Entry.
            /// </returns>
            public static implicit operator DictionaryEntry(Entry entry)
            {
                return new DictionaryEntry(entry.Key, entry.Value);
            }

            /// <summary>
            /// Converts Entry to <b>CacheEntry</b>.
            /// </summary>
            /// <param name="entry">
            /// Entry instance.
            /// </param>
            /// <returns>
            /// <b>CacheEntry</b> with key and value extracted from
            /// the specified Entry.
            /// </returns>
            public static implicit operator CacheEntry(Entry entry)
            {
                return new CacheEntry(entry.Key, entry.Value);
            }

            #endregion

            #region Data members

            /// <summary>
            /// Parent cache.
            /// </summary>
            protected LocalCache m_cache;

            /// <summary>
            /// Entry's key.
            /// </summary>
            protected object m_key;

            /// <summary>
            /// Entry's value.
            /// </summary>
            protected object m_value;

            /// <summary>
            /// The time at which this Entry was created.
            /// </summary>
            private readonly long m_created;

            /// <summary>
            /// The time at which this Entry was last accessed.
            /// </summary>
            private long m_lastUse;

            /// <summary>
            /// The time at which this Entry will (or did) expire.
            /// </summary>
            private long m_expiry;

            /// <summary>
            /// The number of times that this Entry has been accessed.
            /// </summary>
            private int m_uses;

            /// <summary>
            /// The number of units for the Entry.
            /// </summary>
            private int m_units;

            #endregion
        }

        #endregion

        #region Inner class: LocalCacheEnumerator

        /// <summary>
        /// Factory method for cache enumerator.
        /// </summary>
        /// <param name="cache">Cache to enumerate.</param>
        /// <param name="mode">Enumerator mode.</param>
        /// <returns>Cache enumerator.</returns>
        protected virtual ICacheEnumerator InstantiateCacheEnumerator(
                LocalCache cache, EnumeratorMode mode)
        {
            return new LocalCacheEnumerator(cache, mode);
        }

        /// <summary>
        /// Enumerator mode.
        /// </summary>
        protected enum EnumeratorMode
        {
            /// <summary>
            /// Enumerate entries.
            /// </summary>
            Entries,

            /// <summary>
            /// Enumerate keys.
            /// </summary>
            Keys,

            /// <summary>
            /// Enumerate values.
            /// </summary>
            Values
        }

        /// <summary>
        /// <see cref="ICacheEnumerator"/> implementation.
        /// </summary>
        private class LocalCacheEnumerator : ICacheEnumerator
        {
            #region Constructors

            /// <summary>
            /// Creates an instance of cache enumerator.
            /// </summary>
            public LocalCacheEnumerator(LocalCache cache,
                                        EnumeratorMode mode)
            {
                m_enumerator = cache.Storage.Values.GetEnumerator();
                m_mode       = mode;
            }

            #endregion

            #region Implementation of ICacheEnumerator

            /// <summary>
            /// Gets both the key and the value of the current cache entry.
            /// </summary>
            /// <value>
            /// An <see cref="ICacheEntry"/> containing both the key and
            /// the value of the current cache entry.
            /// </value>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first entry
            /// of the cache or after the last entry.
            /// </exception>
            public virtual ICacheEntry Entry
            {
                get
                {
                    return (ICacheEntry) m_enumerator.Current;
                }
            }

            /// <summary>
            /// Gets the key of the current cache entry.
            /// </summary>
            /// <returns>
            /// The key of the current element of the enumeration.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first entry
            /// of the cache or after the last entry.
            /// </exception>
            public virtual object Key
            {
                get
                {
                    return Entry.Key;
                }
            }

            /// <summary>
            /// Gets the value of the current cache entry.
            /// </summary>
            /// <returns>
            /// The value of the current element of the enumeration.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first entry
            /// of the cache or after the last entry.
            /// </exception>
            public virtual object Value
            {
                get
                {
                    return Entry.Value;
                }
            }

            /// <summary>
            /// Gets both the key and the value of the current dictionary
            /// entry.
            /// </summary>
            /// <returns>
            /// A <b>DictionaryEntry</b> containing both the key and the
            /// value of the current dictionary entry.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first entry of the
            /// cache or after the last entry.
            /// </exception>
            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    ICacheEntry entry = Entry;
                    return new DictionaryEntry(entry.Key, entry.Value);
                }
            }

            /// <summary>
            /// Advances the enumerator to the next element of the
            /// collection.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the enumerator was successfully advanced to
            /// the next element; <b>false</b> if the enumerator has passed
            /// the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public virtual bool MoveNext()
            {
                bool hasNext = m_enumerator.MoveNext();
                while (hasNext && ((Entry) m_enumerator.Current).IsExpired)
                {
                    hasNext = m_enumerator.MoveNext();
                }
                return hasNext;
            }

            /// <summary>
            /// Sets the enumerator to its initial position,
            /// which is before the first element in the collection.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public virtual void Reset()
            {
                m_enumerator.Reset();
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <returns>
            /// The current element in the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first element of the
            /// collection or after the last element.
            /// </exception>
            public virtual object Current
            {
                get
                {
                    switch (m_mode)
                    {
                        case EnumeratorMode.Entries: return Entry;
                        case EnumeratorMode.Keys:    return Key;
                        case EnumeratorMode.Values:  return Value;
                        default:
                            throw new ArgumentException("Mode is invalid");
                    }
                }
            }

            #endregion

            #region Data members

            /// <summary>
            /// The key enumerator of the SynchronizedCache this enumerator 
            /// enumerates over.
            /// </summary>
            private readonly IEnumerator m_enumerator;

            /// <summary>
            /// Enumerator mode, determines what will be returned by the 
            /// Current property.
            /// </summary>
            private readonly EnumeratorMode m_mode;

            #endregion
        }

        #endregion

        #region Inner class: EntriesCollection

        /// <summary>
        /// Factory method that creates virtual collection of cache entries.
        /// </summary>
        /// <param name="cache">
        /// Cache to create entries collection for.
        /// </param>
        /// <returns>
        /// Virtual collection of cache entries.
        /// </returns>
        protected virtual ICollection InstantiateEntriesCollection(LocalCache cache)
        {
            return new EntriesCollection(cache);
        }

        /// <summary>
        /// Internal entries collection.
        /// </summary>
        private class EntriesCollection : ICollection
        {
            private readonly LocalCache m_cache;

            protected internal EntriesCollection(LocalCache cache)
            {
                m_cache = cache;
            }

            public IEnumerator GetEnumerator()
            {
                LocalCache cache = m_cache;
                return cache.InstantiateCacheEnumerator(cache, EnumeratorMode.Entries);
            }

            public void CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (array.Rank != 1)
                {
                    throw new ArgumentException(
                        "Multidimensional array is not supported for this operation");
                }
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "index", "Index cannot be a negative number");
                }
                if ((array.Length - index) < m_cache.Count)
                {
                    throw new ArgumentException("Destination array is too small");
                }

                foreach (Object val in this)
                {
                    array.SetValue(val, index++);
                }
            }

            public int Count
            {
                get { return m_cache.Count; }
            }

            public object SyncRoot
            {
                get { return m_cache.SyncRoot; }
            }

            public bool IsSynchronized
            {
                get { return m_cache.IsSynchronized; }
            }
        }

        #endregion

        #region Inner class: KeysCollection

        /// <summary>
        /// Factory method that creates virtual collection of cache keys.
        /// </summary>
        /// <param name="cache">
        /// Cache to create keys collection for.
        /// </param>
        /// <returns>
        /// Virtual collection of cache keys.
        /// </returns>
        protected virtual ICollection InstantiateKeysCollection(LocalCache cache)
        {
            return new KeysCollection(cache);
        }

        /// <summary>
        /// Internal keys collection.
        /// </summary>
        private class KeysCollection : ICollection
        {
            private readonly LocalCache m_cache;

            protected internal KeysCollection(LocalCache cache)
            {
                m_cache = cache;
            }

            public IEnumerator GetEnumerator()
            {
                LocalCache cache = m_cache;
                return cache.InstantiateCacheEnumerator(cache, EnumeratorMode.Keys);
            }

            public void CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (array.Rank != 1)
                {
                    throw new ArgumentException(
                        "Multidimensional array is not supported for this operation");
                }
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "index", "Index cannot be a negative number");
                }
                if ((array.Length - index) < m_cache.Count)
                {
                    throw new ArgumentException("Destination array is too small");
                }

                foreach (Object val in this)
                {
                    array.SetValue(val, index++);
                }
            }

            public int Count
            {
                get { return m_cache.Count; }
            }

            public object SyncRoot
            {
                get { return m_cache.SyncRoot; }
            }

            public bool IsSynchronized
            {
                get { return m_cache.IsSynchronized; }
            }
        }

        #endregion

        #region Inner class: ValuesCollection

        /// <summary>
        /// Factory method that creates virtual collection of cache values.
        /// </summary>
        /// <param name="cache">
        /// Cache to create values collection for.
        /// </param>
        /// <returns>
        /// Virtual collection of cache values.
        /// </returns>
        protected virtual ICollection InstantiateValuesCollection(LocalCache cache)
        {
            return new ValuesCollection(cache);
        }

        /// <summary>
        /// Internal values collection.
        /// </summary>
        private class ValuesCollection : ICollection
        {
            private readonly LocalCache m_cache;

            protected internal ValuesCollection(LocalCache cache)
            {
                m_cache = cache;
            }

            public IEnumerator GetEnumerator()
            {
                LocalCache cache = m_cache;
                return cache.InstantiateCacheEnumerator(cache, EnumeratorMode.Values);
            }

            public void CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (array.Rank != 1)
                {
                    throw new ArgumentException(
                        "Multidimensional array is not supported for this operation");
                }
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "index", "Index cannot be a negative number");
                }
                if ((array.Length - index) < m_cache.Count)
                {
                    throw new ArgumentException("Destination array is too small");
                }

                foreach (Object val in this)
                {
                    array.SetValue(val, index++);
                }
            }

            public int Count
            {
                get { return m_cache.Count; }
            }

            public object SyncRoot
            {
                get { return m_cache.SyncRoot; }
            }

            public bool IsSynchronized
            {
                get { return m_cache.IsSynchronized; }
            }
        }

        #endregion

        #region Inner class: KeyMask

        /// <summary>
        /// A class that masks certain changes so that they are not
        /// reported back to the <see cref="ICacheStore"/>.
        /// </summary>
        protected class KeyMask
        {
            /// <summary>
            /// Check whether or not the currently performed operation is
            /// internally initiated.
            /// </summary>
            /// <value>
            /// <b>true</b> if the current operation is internal.
            /// </value>
            public virtual bool IsSynthetic
            {
                get { return true; }
                set { }
            }

            /// <summary>
            /// Check if a key should be ignored.
            /// </summary>
            /// <param name="key">
            /// The key that a change event has occurred for.
            /// </param>
            /// <returns>
            /// <b>true</b> if change events for the key should be ignored.
            /// </returns>
            public virtual bool IsIgnored(object key)
            {
                return false;
            }

            /// <summary>
            /// Ensure that the synthetic operation flag is set.
            /// </summary>
            /// <returns>
            /// The previous value of the flag.
            /// </returns>
            public virtual bool EnsureSynthetic()
            {
                bool isSynthetic = IsSynthetic;
                if (!isSynthetic)
                {
                    IsSynthetic = true;
                }
                return isSynthetic;
            }

            /// <summary>
            /// Check whether or not the currently performed operation has been initiated
            /// because the entry expired.
            /// </summary>
            /// <value>
            /// <b>true</b> iff the entry has expired
            /// </value>
            /// <since>14.1.1.0.10</since>
            public virtual bool IsExpired
            {
                get { return true; }
                set { }
            }

            /// <summary>
            /// Ensure that the expired flag is set.
            /// </summary>
            /// <returns>
            /// The previous value of the flag.
            /// </returns>
            /// <since>14.1.1.0.10</since>
            public virtual bool EnsureExpired()
            {
                bool isExpired = IsExpired;
                if (!isExpired)
                {
                    IsExpired = true;
                }
                return isExpired;
            }
        }

        #endregion

        #region Inner classes: DefaultKeyMask, LoadKeyMask, LoadAllKeyMask

        /// <summary>
        /// KeyMask implementation that ignores nothing.
        /// </summary>
        protected class DefaultKeyMask : KeyMask
        {
            /// <summary>
            /// Check whether or not the currently performed operation is
            /// internally initiated.
            /// </summary>
            /// <value>
            /// <b>true</b> if the current operation is internal.
            /// </value>
            public override bool IsSynthetic { get; set; }

            /// <summary>
            /// Check whether or not the currently performed operation is
            /// due to entry expired.
            /// </summary>
            /// <value>
            /// <b>true</b> if the current operation is due to entry expired.
            /// </value>
            /// <since>14.1.1.0.10</since>
            public override bool IsExpired { get; set; }
        }

        /// <summary>
        /// KeyMask implementation used in LoadAll().
        /// </summary>
        protected class LoadAllKeyMask : KeyMask
        {
            /// <summary>
            /// Create a new LoadAllKeyMask that will mask events associated
            /// with any of the given keys.
            /// </summary>
            /// <param name="keys">
            /// The collection of keys for which events should be ignored.
            /// </param>
            public LoadAllKeyMask(ICollection keys)
            {
                m_keys = keys;
            }

            /// <summary>
            /// Check if a key should be ignored.
            /// </summary>
            /// <param name="key">
            /// The key that a change event has occurred for.
            /// </param>
            /// <returns>
            /// <b>true</b> if change events for the key should be ignored.
            /// </returns>
            public override bool IsIgnored(object key)
            {
                return CollectionUtils.Contains(m_keys, key);
            }

            /// <summary>
            /// The collection of keys for which events should be ignored.
            /// </summary>
            private readonly ICollection m_keys;
        }

        /// <summary>
        /// KeyMask implementation used in Load().
        /// </summary>
        protected class LoadKeyMask : KeyMask
        {
            /// <summary>
            /// Create a new LoadKeyMask that will mask events associated with
            /// the specified key.
            /// </summary>
            /// <param name="key">
            /// The key for which events should be ignored.
            /// </param>
            public LoadKeyMask(object key)
            {
                m_key = key;
            }

            /// <summary>
            /// Check if a key should be ignored.
            /// </summary>
            /// <param name="key">
            /// The key that a change event has occurred for.
            /// </param>
            /// <returns>
            /// <b>true</b> if change events for the key should be ignored.
            /// </returns>
            public override bool IsIgnored(object key)
            {
                return Equals(m_key, key);
            }

            /// <summary>
            /// The key for which events should be ignored.
            /// </summary>
            private readonly object m_key;
        }

        #endregion

        #region Inner class: InternalListener

        /// <summary>
        /// An internal <see cref="ICacheListener"/> that listens to this
        /// cache and reports changes to the <see cref="ICacheStore"/>.
        /// </summary>
        protected class InternalListener : ICacheListener
        {
            #region Constructors

            /// <summary>
            /// Parametrized constructor.
            /// </summary>
            /// <param name="parentCache">
            /// Parent LocalCache.
            /// </param>
            public InternalListener(LocalCache parentCache)
            {
                this.parentCache = parentCache;
            }

            #endregion

            #region ICacheListener implementation

            /// <summary>
            /// Invoked when a cache entry has been inserted.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/> carrying the insert
            /// information.
            /// </param>
            public virtual void EntryInserted(CacheEventArgs evt)
            {
                OnModify(evt);
            }

            /// <summary>
            /// Invoked when a cache entry has been updated.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/> carrying the update
            /// information.
            /// </param>
            public virtual void EntryUpdated(CacheEventArgs evt)
            {
                OnModify(evt);
            }

            /// <summary>
            /// Invoked when a cache entry has been deleted.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/> carrying the remove
            /// information.
            /// </param>
            public virtual void EntryDeleted(CacheEventArgs evt)
            {
                // deletions are handled by the Clear() and Remove(Object)
                // methods, and are ignored by the listener, because they
                // include evictions, which may be impossible to
                // differentiate from client-invoked removes and clears
            }

            #endregion

            #region Event handling methods

            /// <summary>
            /// A value modification event (insert or update) has occurred.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/> object.
            /// </param>
            private void OnModify(CacheEventArgs evt)
            {
                if (!parentCache.CurrentKeyMask.IsIgnored(evt.Key))
                {
                    ICacheStore store = parentCache.CacheStore;
                    if (store != null)
                    {
                        store.Store(evt.Key, evt.NewValue);
                    }
                }
            }

            #endregion

            #region Data members

            private readonly LocalCache parentCache;

            #endregion
        }

        #endregion

        #region Inner class: CacheLock

        /// <summary>
        /// A lock object.
        /// </summary>
        protected internal class CacheLock
        {
            #region Properties

            /// <summary>
            /// Checks whether or not this <b>Lock</b> object is held by
            /// another thread.
            /// <p>
            /// Note: caller of this method is expected to hold a
            /// synchronization monitor for the <b>Lock</b> object while
            /// making this call.</p>
            /// </summary>
            /// <returns>
            /// <b>true</b> if the <b>Lock</b> is held by another thread;
            /// <b>false</b> otherwise.
            /// </returns>
            internal virtual bool IsDirty
            {
                get
                {
                    Thread threadHolder  = m_thread;
                    Thread threadCurrent = Thread.CurrentThread;

                    if (threadHolder != null && threadHolder != threadCurrent)
                    {
                        // make sure that the holder thread is still alive
                        if (threadHolder.IsAlive)
                        {
                            return true;
                        }

                        // the holder is dead - release the lock
                        m_thread    = null;
                        m_lockCount = 0;

                        if (m_blockCount > 0)
                        {
                            Monitor.Pulse(this);
                        }
                    }
                    return false;
                }
            }

            /// <summary>
            /// Checks whether or not this <b>Lock</b> object is held by the
            /// calling thread.
            /// <p>
            /// Note: unlike other methods of this class, the caller of this
            /// method is <i>not</i> required to hold a synchronization
            /// monitor for the <b>Lock</b> object while making this call.</p>
            /// </summary>
            /// <returns>
            /// <b>true</b> if the <b>Lock</b> is held by the calling thread;
            /// <b>false</b> otherwise.
            /// </returns>
            internal virtual bool IsOwnedByCaller
            {
                get { return m_thread == Thread.CurrentThread; }
            }

            /// <summary>
            /// Checks whether or not this <b>Lock</b> object is discardable.
            /// <p>
            /// Note: caller of this method is expected to hold a
            /// synchronization monitor for the <b>Lock</b> object while
            /// making this call.</p>
            /// </summary>
            /// <returns>
            /// <b>true</b> if the <b>Lock</b> is discardable; <b>false</b>
            /// otherwise.
            /// </returns>
            internal virtual bool IsDiscardable
            {
                get { return m_lockCount == 0 && m_blockCount == 0; }
            }

            /// <summary>
            /// Gets the <b>Thread</b> object holding this <b>Lock</b>.
            /// </summary>
            /// <returns>
            /// The <b>Thread</b> object holding this <b>Lock</b>.
            /// </returns>
            protected virtual Thread LockThread
            {
                get { return m_thread; }
            }

            /// <summary>
            /// Gets the lock count.
            /// </summary>
            /// <returns>
            /// The lock count.
            /// </returns>
            protected virtual int LockCount
            {
                get { return m_lockCount; }
            }

            /// <summary>
            /// Gets the blocked threads count.
            /// </summary>
            /// <returns>
            /// The blocked threads count.
            /// </returns>
            protected virtual int BlockCount
            {
                get { return m_blockCount; }
            }

            /// <summary>
            /// Gets a human readable decription of the <b>Lock</b> type.
            /// </summary>
            /// <returns>
            /// A human readable decription of the <b>Lock</b> type.
            /// </returns>
            protected virtual string LockTypeDescription
            {
                get { return "Lock"; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Construct a new <b>Lock</b> object.
            /// </summary>
            protected internal CacheLock()
            {
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Assign the ownership of this <b>Lock</b> to the calling
            /// thread.
            /// </summary>
            /// <remarks>
            /// <p />
            /// Note: caller of this method is expected to hold a
            /// synchronization monitor for the <b>Lock</b> object while
            /// making this call.
            /// </remarks>
            /// <param name="waitMillis">
            /// The number of milliseconds to continue trying to obtain
            /// a lock; pass zero to return immediately; pass -1 to block
            /// the calling thread until the lock could be obtained.
            /// </param>
            /// <returns>
            /// <b>true</b> if lock was successful; <b>false</b> otherwise.
            /// </returns>
            public virtual bool Assign(long waitMillis)
            {
                while (IsDirty)
                {
                    if (waitMillis == 0)
                    {
                        return false;
                    }
                    waitMillis = WaitForNotify(waitMillis);
                }

                int lockCount = m_lockCount + 1;
                if (lockCount == 1)
                {
                    m_thread = Thread.CurrentThread;
                }
                else if (lockCount == Int16.MaxValue)
                {
                    throw new SystemException("Lock count overflow: " + this);
                }
                m_lockCount = (short) lockCount;

                return true;
            }

            /// <summary>
            /// Wait for a <b>Lock</b> release notification.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Note: caller of this method is expected to hold a synchronization
            /// monitor for the Lock object while making this call.</p>
            /// </remarks>
            /// <param name="waitMillis">
            /// The number of milliseconds to continue waiting;
            /// pass -1 to block the calling thread indefinitely.
            /// </param>
            /// <returns>
            /// Updated wait time.
            /// </returns>
            public virtual long WaitForNotify(long waitMillis)
            {
                long time = DateTimeUtils.GetSafeTimeMillis();
                try
                {
                    m_blockCount++;

                    // in case of thread death of the lock holder, do not wait forever,
                    // because thread death becomes an implicit unlock, and this thread
                    // needs to then wake up and take the lock
                    const int MAX_WAIT = 1000;
                    int millis = (waitMillis <= 0 || waitMillis > MAX_WAIT) ? MAX_WAIT : (int) waitMillis;

                    Monitor.Wait(this, millis);
                }
                finally
                {
                    m_blockCount--;
                }

                if (waitMillis > 0)
                {
                    // reduce the waiting time by the elapsed amount
                    waitMillis -= (DateTimeUtils.GetSafeTimeMillis() - time);
                    waitMillis = Math.Max(0, waitMillis);
                }

                return waitMillis;
            }

            /// <summary>
            /// Release this Lock.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Note: caller of this method is expected to hold a
            /// synchronization monitor for the <b>Lock</b> object while
            /// making this call.</p>
            /// </remarks>
            /// <returns>
            /// <b>true</b> if unlock is successful; <b>false</b> if the
            /// entry remained locked.
            /// </returns>
            public virtual bool Release()
            {
                if (IsDirty)
                {
                    return false;
                }

                int lockCount = m_lockCount - 1;

                if (lockCount == 0)
                {
                    m_thread = null;
                }
                else if (lockCount < 0)
                {
                    lockCount = 0;
                }
                m_lockCount = (short) lockCount;

                if (lockCount == 0)
                {
                    if (m_blockCount > 0)
                    {
                        Monitor.Pulse(this);
                    }
                    return true;
                }
                return false;
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Return a human readable decription of the <b>Lock</b>.
            /// </summary>
            /// <returns>
            /// A human readable decription of the <b>Lock</b>.
            /// </returns>
            public override string ToString()
            {
                return LockTypeDescription + "[" + m_thread + ", cnt=" + m_lockCount + ", block=" + m_blockCount + ']';
            }

            #endregion

            #region Data members

            /// <summary>
            /// The Thread object holding a lock for this entry.
            /// </summary>
            private Thread m_thread;

            /// <summary>
            /// The lock count (number of times the "assign" was called by
            /// the locking thread).
            /// </summary>
            private short m_lockCount;

            /// <summary>
            /// The number of threads waiting on this Lock to be released.
            /// </summary>
            private short m_blockCount;

            #endregion
        }

        #endregion

        #region Enum: UnitCalculatorType

        /// <summary>
        /// The type of unit calculator used by the cache.
        /// </summary>
        public enum UnitCalculatorType
        {
            /// <summary>
            /// Specifies the default unit calculator that weighs all entries
            /// equally as 1.
            /// </summary>
            Fixed = 0,

            /// <summary>
            /// Specifies a external (custom) unit calculator implementation.
            /// </summary>
            External = 1,

            /// <summary>
            /// Unspecified unit calculator.
            /// </summary>
            Unknown = 2
        }

        #endregion

        #region Enum: EvictionPolicyType

        /// <summary>
        /// The type of eviction policy employed by the cache.
        /// </summary>
        public enum EvictionPolicyType
        {
            /// <summary>
            /// By default, the cache prunes based on a hybrid LRU+LFU
            /// algorithm.
            /// </summary>
            Hybrid = 0,

            /// <summary>
            /// The cache can prune based on a pure Least Recently Used (LRU)
            /// algorithm.
            /// </summary>
            LRU = 1,

            /// <summary>
            /// The cache can prune based on a pure Least Frequently Used
            /// (LFU) algorithm.
            /// </summary>
            LFU = 2,

            /// <summary>
            /// The cache can prune using an external eviction policy.
            /// </summary>
            External = 3,

            /// <summary>
            /// Uncpecified eviction policy type.
            /// </summary>
            Unknown = 4
        }

        #endregion

        #region Constants

        /// <summary>
        /// The default key mask that ignores nothing.
        /// </summary>
        protected readonly KeyMask DEFAULT_KEY_MASK = new DefaultKeyMask();

        /// <summary>
        /// By default, the cache size (in units) is infinite.
        /// </summary>
        public const int DEFAULT_UNITS = Int32.MaxValue;

        /// <summary>
        /// By default, the cache entries never expire.
        /// </summary>
        public const int DEFAULT_EXPIRE = 0;

        /// <summary>
        /// By default, expired cache entries are flushed on a minute
        /// interval.
        /// </summary>
        public const int DEFAULT_FLUSH = 60000;

        /// <summary>
        /// By default, when the cache prunes, it reduces its entries by 25%,
        /// meaning it retains 75% (.75) of its entries.
        /// </summary>
        public const double DEFAULT_PRUNE = 0.75;

        #endregion

        #region Data members

        /// <summary>
        /// The cache listener used by this cache to listen to itself in
        /// order to pass events to the ICacheStore.
        /// </summary>
        private ICacheListener m_listener;

        /// <summary>
        /// The loader used by this cache for misses.
        /// </summary>
        private ICacheLoader m_loader;

        /// <summary>
        /// The store used by this cache for modifications.
        /// </summary>
        private ICacheStore m_store;

        /// <summary>
        /// The IDictionary of indexes maintaned by this cache. The keys are
        /// IValueExtractor objects, and for each key, the corresponding value
        /// stored in the IDictionary is a MapIndex object.
        /// </summary>
        private IDictionary m_indexMap;

        /// <summary>
        /// The current number of units in the cache.
        /// </summary>
        /// <remarks>
        /// A unit is an undefined means of measuring cached values, and
        /// must be 0 or positive. The particular <see cref="Entry"/>
        /// implementation being used defines the meaning of unit.
        /// </remarks>
        private long m_curUnits;

        /// <summary>
        /// The number of units to allow the cache to grow to before pruning.
        /// </summary>
        private long m_maxUnits;

        /// <summary>
        /// The number of units to prune the cache down to.
        /// </summary>
        private long m_pruneUnits;

        /// <summary>
        /// The percentage of the total number of units that will remain
        /// after the cache manager prunes the cache; this value is in the
        /// range 0.0 to 1.0.
        /// </summary>
        private double m_pruneLevel;

        /// <summary>
        /// The number of milliseconds that a value will live in the cache.
        /// </summary>
        /// <remarks>
        /// Zero indicates no timeout.
        /// </remarks>
        private int m_expiryDelay;

        /// <summary>
        /// The interval between full cache flushes, in milliseconds.
        /// </summary>
        private int m_flushDelay;

        /// <summary>
        /// The time at which the next full cache flush should occur.
        /// </summary>
        private long m_nextFlush;

        /// <summary>
        /// The <see cref="CacheStatistics"/> object maintained by this
        /// cache.
        /// </summary>
        private readonly SimpleCacheStatistics m_stats = new SimpleCacheStatistics();

        /// <summary>
        /// The <see cref="CacheListenerSupport"/> object.
        /// </summary>
        private CacheListenerSupport m_listenerSupport;

        /// <summary>
        /// The type of eviction policy employed by the cache; one of the
        /// EvictionPolicyType enum values.
        /// </summary>
        private EvictionPolicyType m_evictionType;

        /// <summary>
        /// The external eviction policy.
        /// </summary>
        private IEvictionPolicy m_policy;

        /// <summary>
        /// The type of unit calculator employed by the cache; one of the
        /// UnitCalculatorType enum values.
        /// </summary>
        private UnitCalculatorType m_calculatorType;

        /// <summary>
        /// The external unit calculator.
        /// </summary>
        private IUnitCalculator m_calculator;

        /// <summary>
        /// The last time that a prune was run. This value is used by the
        /// hybrid eviction policy.
        /// </summary>
        /// <since>Coherence 3.5</since>
        protected internal long m_lastPrune;

        /// <summary>
        /// For a prune cycle, this value is the average number of touches
        /// that an entry should have. This value is used by the hybrid
        /// eviction policy.
        /// </summary>
        /// <since>Coherence 3.5</since>
        protected internal int m_avgTouch;

        /// <summary>
        /// The thread-local object to check for keys that the current thread
        /// is supposed to ignore if those keys change.
        /// </summary>
        /// <remarks>
        /// Contains <b>KeyMask</b> objects.
        /// </remarks>
        private readonly LocalDataStoreSlot m_ignore;

        /// <summary>
        /// The map containing all the locks.
        /// </summary>
        private readonly IDictionary m_mapLock = new SynchronizedDictionary();

        /// <summary>
        /// The Gate object for the entire cache.
        /// </summary>
        private readonly Gate m_gateMap = GateFactory.NewGate;

        /// <summary>
        /// The collection of entries in this cache.
        /// </summary>
        private ICollection m_colEntries;

        /// <summary>
        /// The collection of keys in this cache.
        /// </summary>
        private ICollection m_colKeys;

        /// <summary>
        /// The collection of values in this cache.
        /// </summary>
        private ICollection m_colValues;

        private static readonly object UNKNOWN = new object();

        #endregion
    }
}