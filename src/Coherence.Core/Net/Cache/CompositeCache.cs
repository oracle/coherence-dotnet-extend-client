/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

using Tangosol.Net;
using Tangosol.Net.Cache.Support;
using Tangosol.Net.Internal;
using Tangosol.Util;
using Tangosol.Util.Collections;
using Tangosol.Util.Filter;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// <see cref="ICache"/> implementation that wraps two caches - a
    /// front cache (assumed to be "inexpensive" and probably
    /// "incomplete") and a back cache (assumed to be "complete" and
    /// "correct", but more "expensive") - using a read-through/write-through
    /// approach.
    /// </summary>
    /// <remarks>
    /// <p>
    /// If the back cache implements <see cref="IObservableCache"/> interface,
    /// the <b>CompositeCache</b> provides four different strategies of
    /// invalidating the front cache entries that have changed by other
    /// processes in the back cache:
    /// <list type="bullet">
    /// <item>
    /// <see cref="CompositeCacheStrategyType.ListenNone"/> strategy
    /// instructs the cache not to listen for invalidation events at all.
    /// This is the best choice for raw performance and scalability when
    /// business requirements permit the use of data which might not be
    /// absolutely current. Freshness of data can be guaranteed by use of a
    /// sufficiently brief eviction policy for the front cache.
    /// </item>
    /// <item>
    /// <see cref="CompositeCacheStrategyType.ListenPresent"/> strategy
    /// instructs the <b>CompositeCache</b> to listen to the back cache
    /// events related <b>only</b> to the items currently present in the
    /// front cache. This strategy works best when each instance of a
    /// front cache contains distinct subset of data relative to the
    /// other front cache instances (e.g. sticky data access patterns).
    /// </item>
    /// <item>
    /// <see cref="CompositeCacheStrategyType.ListenAll"/> strategy instructs
    /// the <b>CompositeCache</b> to listen to <b>all</b> back cache events.
    /// This strategy is optimal for read-heavy tiered access patterns where
    /// there is significant overlap between the different instances of front
    /// caches.
    /// </item>
    /// <item>
    /// <see cref="CompositeCacheStrategyType.ListenAuto"/> strategy
    /// instructs the <b>CompositeCache</b> implementation to switch
    /// automatically between <b>ListenPresent</b> and <b>ListenAll</b>
    /// strategies based on the cache statistics.
    /// </item>
    /// <item>
    /// <see cref="CompositeCacheStrategyType.ListenLogical"/> strategy
    /// instructs the <b>CompositeCache</b> to listen to <b>all</b> back map
    /// events that are <b>not synthetic</b>.  A synthetic event could be
    /// emitted as a result of eviction or expiration.  With this invalidation
    /// stategy, it is possible for the front map to contain cache entries that
    /// have been synthetically removed from the back (though any subsequent
    /// re-insertion will cause the corresponding entries in the front map to
    /// be invalidated).
    /// </item>
    /// </list></p>
    /// <p>
    /// The front cache implementation is assumed to be thread safe;
    /// additionally any modifications to the front cache are allowed
    /// only after the corresponding lock is acquired against the
    /// <see cref="CacheControl"/> property.</p>
    /// <p>
    /// <b>Note:</b> <c>null</c> values are not cached in the front
    /// cache and therefore this implementation is not optimized for
    /// caches that allow <c>null</c> values to be stored.</p>
    /// </remarks>
    /// <author>Alex Gleyzer, Gene Gleyzer  2002.09.10</author>
    /// <author>Gene Gleyzer  2003.10.16</author>
    /// <author>Ivan Cikic  2006.09.11</author>
    public class CompositeCache : ICache, ICacheStatistics, IDisposable
    {
        #region Properties

        /// <summary>
        /// Obtain the front cache reference.
        /// </summary>
        /// <remarks>
        /// <b>Note:</b> direct modifications of the returned cache may
        /// cause an unpredictable behavior of the <b>CompositeCache</b>.
        /// </remarks>
        /// <value>
        /// The front <see cref="ICache"/>.
        /// </value>
        public virtual ICache FrontCache
        {
            get
            {
                ICache cache = m_front;
                if (cache == null)
                {
                    throw new InvalidOperationException("Cache is not active");
                }
                return cache;
            }
        }

        /// <summary>
        /// Obtain the back cache reference.
        /// </summary>
        /// <remarks>
        /// <b>Note:</b> direct modifications of the returned cache may
        /// cause an unpredictable behavior of the <b>CompositeCache</b>.
        /// </remarks>
        /// <value>
        /// The back <see cref="ICache"/>.
        /// </value>
        public virtual ICache BackCache
        {
            get
            {
                ICache cache = m_back;
                if (cache == null)
                {
                    throw new InvalidOperationException("Cache is not active");
                }
                return cache;
            }
        }

        /// <summary>
        /// Obtain the invalidation strategy used by this
        /// <b>CompositeCache</b>.
        /// </summary>
        /// <value>
        /// One of <see cref="CompositeCacheStrategyType"/> values.
        /// </value>
        public virtual CompositeCacheStrategyType InvalidationStrategy
        {
            get { return m_strategyTarget;}
        }

        /// <summary>
        /// Obtain the <see cref="IConcurrentCache"/> that should be used to
        /// synchronize the front cache modification access.
        /// </summary>
        /// <value>
        /// An <b>IConcurrentCache</b> instance controlling the front cache
        /// modifications.
        /// </value>
        public virtual IConcurrentCache CacheControl
        {
            get { return m_cacheControl; }
        }

        /// <summary>
        /// Determine if changes to the back cache affect the front cache so
        /// that data in the front cache stays in sync.
        /// </summary>
        /// <value>
        /// <c>true</c> if the front cache has a means to stay in sync
        /// with the back cache so that it does not contain stale data.
        /// </value>
        protected virtual bool IsCoherent
        {
            get { return m_listener != null; }
        }

        /// <summary>
        /// Obtain the <see cref="CacheStatistics"/> for this cache.
        /// </summary>
        /// <value>
        /// A <b>CacheStatistics</b> object.
        /// </value>
        public virtual ICacheStatistics CacheStatistics
        {
            get { return m_stats; }
        }

        /// <summary>
        /// Determine the rough number of front cache invalidation hits
        /// since the cache statistics were last reset.
        /// </summary>
        /// <remarks>
        /// An invalidation hit is an externally induced cache event
        /// for an entry that exists in the front cache.
        /// </remarks>
        /// <value>
        /// The number of cache invalidation hits.
        /// </value>
        public virtual long InvalidationHits
        {
            get { return m_countInvalidationHits; }
        }

        /// <summary>
        /// Determine the rough number of front cache invalidation
        /// misses since the cache statistics were last reset.
        /// </summary>
        /// <remarks>
        /// An invalidation hit is an externally induced cache event
        /// for an entry that exists in the front cache.
        /// </remarks>
        /// <value>
        /// The number of cache invalidation misses.
        /// </value>
        public virtual long InvalidationMisses
        {
            get { return m_countInvalidationMisses; }
        }

        /// <summary>
        /// Determine the total number of
        /// <see cref="RegisterListener(object)"/> operations since the cache
        /// statistics were last reset.
        /// </summary>
        /// <value>
        /// The total number of listener registrations.
        /// </value>
        public virtual long TotalRegisterListener
        {
            get { return m_countRegisterListener; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a <b>CompositeCache</b> using two specified
        /// caches:
        /// <list type="bullet">
        /// <item>
        /// <i>FrontCache</i> (aka "cache", "near" or "shallow")
        /// </item>
        /// <item>
        /// <i>BackCache</i>  (aka "actual", "real" or "deep")
        /// </item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// If the BackCache implements the <see cref="IObservableCache"/>
        /// interface a listener will be added to the BackCache to invalidate
        /// FrontCache items updated [externally] in the back cache using the
        /// <see cref="CompositeCacheStrategyType.ListenAuto"/> strategy.
        /// </remarks>
        /// <param name="front">
        /// The front cache.
        /// </param>
        /// <param name="back">
        /// The back cache.
        /// </param>
        public CompositeCache(ICache front, ICache back)
            : this(front, back, CompositeCacheStrategyType.ListenAuto)
        {}

        /// <summary>
        /// Construct a <b>CompositeCache</b> using two specified
        /// caches:
        /// <list type="bullet">
        /// <item>
        /// <i>FrontCache</i> (aka "cache", "near" or "shallow")
        /// </item>
        /// <item>
        /// <i>BackCache</i>  (aka "actual", "real" or "deep")
        /// </item>
        /// </list>
        /// and using the specified front cache invalidation strategy.
        /// </summary>
        /// <param name="front">
        /// The front cache.
        /// </param>
        /// <param name="back">
        /// The back cache.
        /// </param>
        /// <param name="strategy">
        /// Specifies the strategy used for the front caches invalidation;
        /// valid values are <see cref="CompositeCacheStrategyType"/>
        /// values.
        /// </param>
        /// <since>Coherence 2.3</since>
        public CompositeCache(ICache front, ICache back, CompositeCacheStrategyType strategy)
        {
            Debug.Assert(front != null && back != null, "Null cache");
            Debug.Assert(CompositeCacheStrategyType.ListenNone <= strategy
                         && strategy <= CompositeCacheStrategyType.ListenLogical,
                         "Invalid strategy value");

            m_front = front;
            m_back  = back;

            if (strategy != CompositeCacheStrategyType.ListenNone)
            {
                if (back is IObservableCache)
                {
                    m_listener = InstantiateBackCacheListener(strategy);
                    if (front is IObservableCache)
                    {
                        m_listenerFront = InstantiateFrontCacheListener();
                    }
                    m_listenerDeactivation = new DeactivationListener(this);
                }
                else
                {
                    strategy = CompositeCacheStrategyType.ListenNone;
                }
            }

            m_strategyTarget  = strategy;
            m_strategyCurrent = CompositeCacheStrategyType.ListenNone;
        }

        #endregion

        #region Life-cycle

        /// <summary>
        /// Release the <b>CompositeCache</b>.
        /// </summary>
        /// <remarks>
        /// If the <see cref="ICache"/> implements an
        /// <see cref="IObservableCache"/> calling this method is necessary
        /// to remove the back cache listener. Any access to the
        /// <b>CompositeCache</b> which has been released will cause
        /// <b>InvalidOperationException</b>.
        /// </remarks>
        public virtual void Release()
        {
            IConcurrentCache cacheControl = CacheControl;
            if (!cacheControl.Lock(LockScope.LOCK_ALL, 0))
            {
                // Note: we cannot do a blocking LOCK_ALL as any event which came
                // in while the Gate is in the closing state would cause the
                // service thread to spin.  Unlike Clear() there is no benefit in
                // sleeping/retrying here as we know that there are other active
                // threads, thus if we succeede they would get the InvalidOperationException
                throw new InvalidOperationException("Cache is in active use by other threads.");
            }

            try
            {
                cacheControl.Insert(GLOBAL_KEY, IGNORE_LIST);
                switch (m_strategyCurrent)
                {
                    case CompositeCacheStrategyType.ListenPresent:
                        UnregisterFrontListener();
                        UnregisterListeners(FrontCache.Keys);
                        break;

                    case CompositeCacheStrategyType.ListenLogical:
                    case CompositeCacheStrategyType.ListenAll:
                        UnregisterListener();
                        break;
                }

                UnregisterDeactivationListener();

                m_listener             = null;
                m_front                = null;
                m_back                 = null;
                m_filterListener       = null;
                m_listenerDeactivation = null;
            }
            catch (Exception)
            {
                // one of the following should be ignored:
                // InvalidOperationException("Cache is not active");
                // Exception("Storage is not configured");
                // Exception("Service has been terminated");
            }
            finally
            {
                cacheControl.Remove(GLOBAL_KEY);
                cacheControl.Unlock(LockScope.LOCK_ALL);
            }
        }

        #endregion

        #region IDictionary implementation

        /// <summary>
        /// Clears both the front and back caches.
        /// </summary>
        public virtual void Clear()
        {
            IConcurrentCache cacheControl = CacheControl;

            // Note: we cannot do a blocking LOCK_ALL as any event which came
            // in while the Gate is in the closing state would cause the
            // service thread to spin.  Try for up ~1s before giving up and
            // issue the operation against the back, allowing events to perform
            // the cleanup. We don't even risk a timed LOCK_ALL as whatever
            // time value we choose would risk a useless spin for that duration
            for (int i = 0; !cacheControl.Lock(LockScope.LOCK_ALL, 0); ++i)
            {
                if (i == 100)
                {
                    BackCache.Clear();
                    if (m_strategyTarget == CompositeCacheStrategyType.ListenNone)
                    {
                        FrontCache.Clear();
                    }
                    return;
                }
                Thread.Sleep(10);
            }

            try
            {
                cacheControl.Insert(GLOBAL_KEY, IGNORE_LIST);

                ICache front = FrontCache;
                ICache back  = BackCache;

                switch (m_strategyCurrent)
                {
                    case CompositeCacheStrategyType.ListenPresent:
                        UnregisterFrontListener();
                        try
                        {
                            ArrayList removeEntries = new ArrayList();
                            foreach (ICacheEntry entry in front)
                            {
                                UnregisterListener(entry.Key);
                                removeEntries.Add(entry);
                            }

                            foreach (ICacheEntry entry in removeEntries)
                            {
                                front.Remove(entry.Key);
                            }
                        }
                        catch (Exception)
                        {
                            // we're not going to reset the invalidation strategy
                            // so we must keep the front listener around
                            RegisterFrontListener();
                            throw;
                        }
                        break;

                    case CompositeCacheStrategyType.ListenLogical:
                    case CompositeCacheStrategyType.ListenAll:
                        UnregisterListener();
                        try
                        {
                            front.Clear();
                        }
                        catch (Exception)
                        {
                            // since we don't know what's left there
                            // leave the cache in a coherent state
                            RegisterListener();
                            throw;
                        }
                        break;

                    default:
                        front.Clear();
                        break;
                }
                ResetInvalidationStrategy();
                back.Clear();
            }
            finally
            {
                cacheControl.Remove(GLOBAL_KEY);
                cacheControl.Unlock(LockScope.LOCK_ALL);
            }
        }

        /// <summary>
        /// Check whether or not this cache contains a mapping for the
        /// specified key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// <b>true</b> if this cache contains a mapping for the
        /// specified key, <b>false</b> otherwise.
        /// </returns>
        public virtual bool Contains(object key)
        {
            ICache front = FrontCache;
            if (front.Contains(key))
            {
                m_stats.RegisterHit();
                return true;
            }

            IConcurrentCache cacheControl = CacheControl;
            cacheControl.Lock(key, -1);
            try
            {
                if (front.Contains(key))
                {
                    m_stats.RegisterHit();
                    return true;
                }
                cacheControl.Insert(key, IGNORE_LIST);
                m_stats.RegisterMiss();
                return BackCache.Contains(key);
            }
            finally
            {
                cacheControl.Remove(key);
                cacheControl.Unlock(key);
            }
        }

        /// <summary>
        /// Associates the specified value with the specified key in this
        /// cache.
        /// </summary>
        /// <param name="key">
        /// Key with which the specified value is to be associated.
        /// </param>
        /// <param name="value">
        /// Value to be associated with the specified key.
        /// </param>
        public virtual void Add(object key, object value)
        {
            Insert(key, value, CacheExpiration.DEFAULT);
        }

        /// <summary>
        /// Gets the <b>IDictionaryEnumerator</b> object for this object.
        /// </summary>
        /// <returns>
        /// An <b>IDictionaryEnumerator</b>.
        /// </returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return BackCache.GetEnumerator();
        }

        /// <summary>
        /// Remove the mapping for this key from this cache if present.
        /// </summary>
        /// <remarks>
        /// Expensive: updates both the underlying cache and the local cache.
        /// </remarks>
        /// <param name="key">
        /// Key whose mapping is to be removed from the cache.
        /// </param>
        public virtual void Remove(object key)
        {
            ICache                     front    = FrontCache;
            ICache                     back     = BackCache;
            CompositeCacheStrategyType strategy = m_strategyTarget;

            IConcurrentCache cacheControl = CacheControl;
            cacheControl.Lock(key, -1);
            try
            {
                if (strategy != CompositeCacheStrategyType.ListenNone)
                {
                    cacheControl.Insert(key, IGNORE_LIST);
                }
                if (front.Contains(key))
                {
                    front.Remove(key);
                    UnregisterListener(key);
                }
                back.Remove(key);
            }
            finally
            {
                if (strategy != CompositeCacheStrategyType.ListenNone)
                {
                    cacheControl.Remove(key);
                }
                cacheControl.Unlock(key);
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key of the element to get or set.
        /// </param>
        /// <value>
        /// The element with the specified key.
        /// </value>
        public virtual object this[object key]
        {
            get
            {
                ICache front = FrontCache;
                object value = front[key];
                if (value != null)
                {
                    m_stats.RegisterHit();
                    return value;
                }

                long             start        = DateTimeUtils.GetSafeTimeMillis();
                IConcurrentCache cacheControl = CacheControl;
                cacheControl.Lock(key, -1);
                try
                {
                    value = front[key];
                    if (value != null)
                    {
                        m_stats.RegisterHit(start);
                        return value;
                    }

                    ICache back = BackCache;
                    if (m_strategyTarget == CompositeCacheStrategyType.ListenNone)
                    {
                        value = back[key];
                        if (value != null)
                        {
                            front.Insert(key, value);
                        }
                    }
                    else
                    {
                        IList listEvents = new ArrayList();
                        cacheControl.Insert(key, listEvents);

                        RegisterListener(key);

                        bool isPrimed;
                        lock (listEvents.SyncRoot)
                        {
                            int c;
                            switch (c = listEvents.Count)
                            {
                                case 0:
                                    isPrimed = false;
                                    break;
                                default:
                                    // check if the last event is a "priming" one
                                    CacheEventArgs evt = (CacheEventArgs) listEvents[c - 1];
                                    if (isPrimed = IsPriming(evt))
                                    {
                                        value = evt.NewValue;
                                        listEvents.RemoveAt(c -1);
                                    }
                                    break;
                            }
                        }

                        if (!isPrimed)
                        {
                            // this call could be a network call
                            // generating events on a service thread
                            try
                            {
                                value = back[key];
                            }
                            catch (Exception)
                            {
                                UnregisterListener(key);
                                cacheControl.Remove(key);
                                throw;
                            }
                        }

                        lock (listEvents.SyncRoot)
                        {
                            if (value == null)
                            {
                                // we don't cache null values
                                UnregisterListener(key);
                            }
                            else
                            {
                                // get operation itself can generate only
                                // a synthetic INSERT; anything else should be
                                // considered as an invalidating event
                                bool isValid = true;
                                switch (listEvents.Count)
                                {
                                    case 0:
                                        break;

                                    case 1:
                                        // it's theoretically possible (though very
                                        // unlikely) that another thread caused the
                                        // entry expiration, reload and the synthetic
                                        // insert all while this request had already
                                        // been supplied with a value;
                                        // we'll take our chance here to provide greater
                                        // effectiveness for the more probable situation
                                        CacheEventArgs evt = (CacheEventArgs) listEvents[0];
                                        isValid = evt.EventType == CacheEventType.Inserted &&
                                                  evt.IsSynthetic;
                                        break;

                                    default:
                                        isValid = false;
                                        break;
                                }

                                if (isValid)
                                {
                                    // Adding to the front cache could cause a large number
                                    // of evictions. Instead of unregistering the listeners
                                    // individually, try to collect them for a bulk unregistration.
                                    HashSet setHolder = SetKeyHolder();
                                    try
                                    {
                                        front.Insert(key, value);
                                    }
                                    finally
                                    {
                                        if (setHolder != null)
                                        {
                                            UnregisterListeners(setHolder);
                                            RemoveKeyHolder();
                                        }
                                    }
                                }
                                else
                                {
                                    UnregisterListener(key);
                                    m_countInvalidationHits++;
                                }
                            }
                            // remove must occur under sync (if we're caching) otherwise we risk losing events
                            cacheControl.Remove(key);
                        }
                    }

                    // update miss statistics
                    m_stats.RegisterMiss(start);
                    return value;
                }
                finally
                {
                    cacheControl.Unlock(key);
                }
            }

            set { Add(key, value); }
        }

        /// <summary>
        /// Obtain an <b>ICollection</b> of the keys contained in this cache.
        /// </summary>
        /// <remarks>
        /// If there is a listener for the back cache, then the collection
        /// will be mutable; otherwise the returned collection will be
        /// immutable. The returned collection reflects the full contents of
        /// the back cache.
        /// </remarks>
        /// <value>
        /// An <b>ICollection</b> of the keys contained in this cache.
        /// </value>
        public virtual ICollection Keys
        {
            get
            {
                ICollection keys = BackCache.Keys;
                if (!IsCoherent)
                {
                    keys = new ArrayList(keys);
                }
                return keys;
            }
        }

        /// <summary>
        /// Obtain an <b>ICollection</b> of the values contained in this
        /// cache.
        /// </summary>
        /// <remarks>
        /// If there is a listener for the back cache, then the collection
        /// will be mutable; otherwise the returned collection will be
        /// immutable. The returned collection reflects the full contents of
        /// the back cache.
        /// </remarks>
        /// <value>
        /// An <b>ICollection</b> of the values contained in this cache.
        /// </value>
        public virtual ICollection Values
        {
            get
            {
                ICollection values = BackCache.Values;
                if (!IsCoherent)
                {
                    values = new ArrayList(values);
                }
                return values;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="BackCache"/>
        /// object is read-only.
        /// </summary>
        /// <value>
        /// <b>true</b> if the <see cref="BackCache"/> object is a read-only;
        /// otherwise, <b>false</b>.
        /// </value>
        public virtual bool IsReadOnly
        {
            get { return BackCache.IsReadOnly; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="BackCache"/>
        /// object has a fixed size.
        /// </summary>
        /// <value>
        /// <b>true</b> if the <see cref="BackCache"/> object has a fixed
        /// size; otherwise, <b>false</b>.
        /// </value>
        public virtual bool IsFixedSize
        {
            get { return BackCache.IsFixedSize; }
        }

        /// <summary>
        /// Copies the elements of the <see cref="BackCache"/> to an
        /// <b>Array</b>, starting at a particular <b>Array</b> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <b>Array</b> that is the destination of the
        /// elements copied from <b>BackCache</b>. The <b>Array</b> must have
        /// zero-based indexing.
        /// </param>
        /// <param name="index">
        /// The zero-based index in array at which copying begins.
        /// </param>
        public virtual void CopyTo(Array array, int index)
        {
            BackCache.CopyTo(array, index);
        }

        /// <summary>
        /// Gets the number of elements contained in the
        /// <see cref="BackCache"/>.
        /// </summary>
        /// <value>
        /// The number of elements contained in the back cache.
        /// </value>
        public virtual int Count
        {
            get { return BackCache.Count; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the back
        /// cache.
        /// </summary>
        /// <value>
        /// An object that can be used to synchronize access to the back
        /// cache.
        /// </value>
        public virtual object SyncRoot
        {
            get { return BackCache.SyncRoot; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the back cache is
        /// synchronized (thread safe).
        /// </summary>
        /// <value>
        /// <b>true</b> if access to the back cache is synchronized (thread
        /// safe); otherwise, <b>false</b>.
        /// </value>
        public virtual bool IsSynchronized
        {
            get { return BackCache.IsSynchronized; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a cache.
        /// </summary>
        /// <returns>
        /// An <b>IEnumerator</b> object that can be used to iterate through
        /// the back cache.
        /// </returns>
        public virtual IEnumerator GetEnumerator()
        {
            return BackCache.GetEnumerator();
        }

        #endregion

        #region ICache implementation

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
        /// <b>Note:</b> this implementation does not differentiate between
        ///  missing keys or <c>null</c> values stored in the back
        /// dictionary; in both cases the returned dictionary will not
        /// contain the corresponding entry.</p>
        /// </remarks>
        /// <param name="keys">
        /// A collection of keys that may be in the named cache.
        /// </param>
        /// <returns>
        /// A dictionary of keys to values for the specified keys passed in
        /// <paramref name="keys"/>.
        /// </returns>
        /// <since>Coherence 2.5</since>
        public virtual IDictionary GetAll(ICollection keys)
        {
            long start = DateTimeUtils.GetSafeTimeMillis();

            // Step 1: retrieve all we can from the front map first
            IDictionary result = FrontCache.GetAll(keys);
            int countHits = result.Count;
            if (countHits > 0)
            {
                m_stats.RegisterHits(countHits, start);
            }

            if (keys.Count == countHits)
            {
                // all keys found in front
                return result;
            }

            HashSet frontMiss = new HashSet(keys);
            CollectionUtils.RemoveAll(frontMiss, new HashSet(result.Keys));

            ICache back = BackCache;

            // Step 2: Lock the missing keys without blocking
            ICache                     front        = FrontCache;
            IConcurrentCache           cacheControl = CacheControl;
            CompositeCacheStrategyType strategy     = EnsureInvalidationStrategy();
            HashSet                    setLocked    = TryLock(frontMiss);
            int                        cLocked      = setLocked.Count;
            int                        cMisses      = frontMiss.Count;

            try
            {
                IList listEvents = new ArrayList();
                if (strategy != CompositeCacheStrategyType.ListenNone)
                {
                    foreach (object key in setLocked)
                    {
                        cacheControl.Insert(key, listEvents);
                    }

                    if (strategy == CompositeCacheStrategyType.ListenPresent)
                    {
                        // Step 3: Register listeners and try to get the values
                        // through priming events
                        RegisterListeners(setLocked);

                        lock (listEvents.SyncRoot)
                        {
                            for (int i = listEvents.Count - 1; i >= 0; --i)
                            {
                                CacheEventArgs evt = (CacheEventArgs)listEvents[i];

                                if (IsPriming(evt))
                                {
                                    object key = evt.Key;
                                    result.Add(key, evt.NewValue);
                                    frontMiss.Remove(key);
                                    listEvents.RemoveAt(i);
                                }
                            }
                        }
                    }
                }

                // Step 4: do a bulk getAll() for all the front misses
                //         that were not "primed"
                if (frontMiss.Count > 0)
                {
                    try
                    {
                        // COH-4447: materialize the converted results to avoid
                        //           unnecessary repeated deserialization
                        CollectionUtils.AddAll(result,  new HashDictionary(back.GetAll(frontMiss)));
                    }
                    catch (Exception)
                    {
                        if (strategy != CompositeCacheStrategyType.ListenNone)
                        {
                            foreach (object key in setLocked)
                            {
                                if (strategy == CompositeCacheStrategyType.ListenPresent)
                                {
                                     UnregisterListener(key);
                                }
                                cacheControl.Remove(key);
                            }
                        }
                        throw;
                    }  
                }

                // Step 5: for the locked keys move the retrieved values to the front
                if (strategy == CompositeCacheStrategyType.ListenNone)
                {
                    foreach (object key in setLocked)
                    {
                        object value = result[key];

                        if (value != null)
                        {
                            front.Insert(key, value);
                        }
                    }
                }
                else
                {
                    HashSet collInValid = new HashSet();
                    HashSet setAdd      = new HashSet(setLocked);

                    // remove entries invalidated during the getAll() call
                    lock (listEvents.SyncRoot)
                    {
                        // GetAll() operation itself can generate not more
                        // than one synthetic INSERT per key; anything else
                        // should be considered as an invalidating event
                        // (see additional comment at "get" processing)
                        foreach (CacheEventArgs evt in listEvents)
                        {
                            object key = evt.Key;
                            // always start with removing the key from the
                            // result set, so a second event is always
                            // treated as an invalidatation
                            bool isValid = setAdd.Remove(key)
                                            && evt.EventType == CacheEventType.Inserted
                                            && evt.IsSynthetic;

                            if (!isValid)
                            {
                                collInValid.Add(key);
                                m_countInvalidationHits++;
                            }
                        }

                        // Adding to the front cache could cause a large number
                        // of evictions. Instead of unregistering the listeners
                        // individually, try to collect them for a bulk unregistration.
                        HashSet setUnregister = SetKeyHolder();
                        try
                        {
                            foreach (object key in setLocked)
                            {
                                object value = result[key];
                                if (value != null && !collInValid.Contains(key))
                                {
                                    try
                                    {
                                        front.Add(key, value);
                                    }
                                    catch (ArgumentException)
                                    { }
                                }
                                else // null or invalid
                                {
                                    if (value == null)
                                    {
                                        result.Remove(key);
                                    }
                                    front.Remove(key);
                                    UnregisterListener(key);
                                }
                                // remove must occur under sync (if we're caching) otherwise we risk losing events
                                cacheControl.Remove(key);
                            }
                        }
                        finally
                        {
                            if (setUnregister != null)
                            {
                                UnregisterListeners(setUnregister);
                                RemoveKeyHolder();
                            }
                        }
                    }
                }
                
                m_stats.RegisterMisses(cMisses, start);
            }
            finally
            {
                foreach (object key in setLocked)
                {
                    cacheControl.Unlock(key);
                }
            }
            return result;
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
        /// This variation of the <see cref="Insert(object, object)"/>
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
            long                       start           = DateTimeUtils.GetSafeTimeMillis();
            ICache                     front           = FrontCache;
            ICache                     back            = BackCache;
            CompositeCacheStrategyType strategyTarget  = m_strategyTarget; // Use of target is intentional
            CompositeCacheStrategyType strategyCurrent = m_strategyCurrent;

            IConcurrentCache cacheControl = CacheControl;
            cacheControl.Lock(key, -1);
            try
            {
                IList listEvents = null;

                // obtain current front value; if the new value is null then
                // remove from the front map since we will ignore any changes
                object frontObj = front[key];
                if (value == null)
                {
                    front.Remove(key);
                }

                if (strategyTarget != CompositeCacheStrategyType.ListenNone)
                {
                    // NOTE: Insert() will not register any new key-based listeners;
                    // per-key registering for new entries would double the number
                    // of synchronous network operations; instead we defer the
                    // registration until the first get; we are assuming that
                    // "get(a), put(a)", or "put(a), put(b)" are more likely
                    // sequences then "put(a), get(a)"

                    if (value == null)
                    {
                        // we won't cache null values, so no need to listen
                        cacheControl.Insert(key, listEvents = IGNORE_LIST);
                        if (frontObj != null)
                        {
                            // the value was previously in the front, cleanup
                            UnregisterListener(key);
                        }
                    }
                    else if (frontObj != null || 
                             strategyCurrent == CompositeCacheStrategyType.ListenLogical ||
                             strategyCurrent == CompositeCacheStrategyType.ListenAll)
                    {
                        // we are already registered for events covering this key

                        // when back cache operations returns we may choose to cache
                        // the new [non-null] value into the front cache. This is
                        // cheap since we already have a listener (global or key)
                        // registered for this entry
                        cacheControl.Insert(key, listEvents = new ArrayList());
                    }
                    else
                    {
                        // we are not registered for events covering this key

                        // we will ignore any changes; this allows us to avoid the
                        // cost of registering a listener and/or generating a
                        // questionably usefull LinkedList allocation which could
                        // become tenured
                        cacheControl.Insert(key, listEvents = IGNORE_LIST);
                    }
                }

                object origObj;
                try
                {
                    // the back map calls could be network calls
                    // generating events on a service thread
                    if (millis > 0)
                    {
                        // normal put with return value
                        origObj = Insert(back, key, value, millis);
                    }
                    else
                    {
                        // optimize out the return value
                        Insert(back, key, value, millis);
                        origObj = null;
                    }
                }
                catch (Exception)
                {
                    // we don't know the state of the back; cleanup and invalidate
                    // this key on the front
                    cacheControl.Remove(key);
                    try
                    {
                        InvalidateFront(key);
                    }
                    catch
                    {
                        // any exception thrown while invalidating front cache
                        // should be ignored
                    }

                    throw;
                }

                // cleanup, and update the front if possible
                FinalizeInsert(key, value, listEvents, millis);

                m_stats.RegisterPut(start);
                return origObj;
            }
            finally
            {
                cacheControl.Unlock(key);
            }
        }

        /// <summary>
        /// Extended put implementation that respects the expiration contract.
        /// </summary>
        /// <param name="dictionary">
        /// The <b>IDictionary</b> object to add pair key/value to.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
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
        private static object Insert(IDictionary dictionary, object key, object value, long millis)
        {
            if (dictionary is ICache)
            {
                return ((ICache) dictionary).Insert(key, value, millis);
            }
            if (millis <= 0)
            {
                object previous = dictionary[key];
                dictionary.Add(key, value);
                return previous;
            }
            throw new InvalidOperationException(
                    "Type \"" + dictionary.GetType().Name +
                    "\" does not implement ICache interface");
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
        /// <exception cref="InvalidOperationException">
        /// If the lock could not be succesfully obtained for some key.
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// This cache does not permit <c>null</c> keys or values, and the
        /// specified key or value is <c>null</c>.
        /// </exception>
        public virtual void InsertAll(IDictionary dictionary)
        {
            // optimize for caller doing a single blind put
            if (dictionary.Count == 1)
            {
                IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    DictionaryEntry entry = (DictionaryEntry) enumerator.Current;
                    Insert(entry.Key, entry.Value, CacheExpiration.DEFAULT);
                }
                return;
            }

            CompositeCacheStrategyType strategyTarget  = m_strategyTarget;
            CompositeCacheStrategyType strategyCurrent = m_strategyCurrent;
            bool                       isAllRegistered = strategyCurrent == CompositeCacheStrategyType.ListenLogical ||
                                                         strategyCurrent == CompositeCacheStrategyType.ListenAll;
            long                       start           = DateTimeUtils.GetSafeTimeMillis();
            IConcurrentCache           cacheControl    = CacheControl;
            ICache                     front           = FrontCache;
            ICache                     back            = BackCache;
            IDictionary                mapLocked       = new HashDictionary();
            IList                      listUnlockable  = null;

            try
            {
                // lock keys where possible
                foreach (DictionaryEntry entry in dictionary)
                {
                    object key   = entry.Key;
                    object value = entry.Value;

                    if (value != null && cacheControl.Lock(key, 0))
                    {
                        mapLocked.Add(key, value);

                        if (strategyTarget != CompositeCacheStrategyType.ListenNone)
                        {
                            // we only track keys which have registered listeners
                            // thus avoiding the synchronous network call for event
                            // registration
                            cacheControl.Insert(key, isAllRegistered || front.Contains(key) ? new ArrayList() : IGNORE_LIST);
                        }
                    }
                    else
                    {
                        // for null values or unlockable keys we will just push
                        // the entry to the back, any required cleanup will occur
                        // automatically during event validation or manually for
                        // ListenNone
                        if (listUnlockable == null)
                        {
                            listUnlockable = new ArrayList();
                        }
                        listUnlockable.Add(key);
                    }
                }

                // update the back with all entries
                back.InsertAll(dictionary);

                // update front with locked keys where possible
                if (strategyTarget == CompositeCacheStrategyType.ListenNone)
                {
                    // no event based cleanup to do, simply update the front
                    front.InsertAll(mapLocked);
                    foreach (object key in mapLocked.Keys)
                    {
                        cacheControl.Unlock(key);
                    }

                    // NOTE:
                    // this is techically incorrect, as we should remove the
                    // key from the key set as soon as it is unlocked; however
                    // there does not appear to be a way in .NET to remove
                    // while iterating, therefore, we'll have to live with it.
                    // The worst case scenario is that we'll invalidate one or
                    // more keys that were already unlocked, so it's more a
                    // question of efficiency than correctness.
                    mapLocked.Clear();
                    // unlockable key cleanup in finally
                }
                else
                {
                    // conditionally update locked keys based on event results
                    foreach (DictionaryEntry entry in mapLocked)
                    {
                        object key = entry.Key;

                        FinalizeInsert(key, entry.Value, (IList) cacheControl[key], 0L);
                        cacheControl.Unlock(key);
                    }

                    // NOTE:
                    // this is techically incorrect, as we should remove the
                    // key from the key set as soon as it is unlocked; however
                    // there does not appear to be a way in .NET to remove
                    // while iterating, therefore, we'll have to live with it.
                    // The worst case scenario is that we'll invalidate one or
                    // more keys that were already unlocked, so it's more a
                    // question of efficiency than correctness.
                    mapLocked.Clear();
                }

                m_stats.RegisterPuts(dictionary.Count, start);
            }
            finally
            {
                // invalidate and unlock anything which remains locked
                foreach (object key in mapLocked.Keys)
                {
                    try
                    {
                        InvalidateFront(key);
                    }
                    catch (Exception)
                    {
                        // any exception thrown while invalidating front cache
                        // should be ignored
                    }

                    cacheControl.Remove(key);
                    cacheControl.Unlock(key);
                }

                // invalidate unlockable keys as needed
                if (listUnlockable != null &&
                    strategyTarget == CompositeCacheStrategyType.ListenNone)
                {
                    // not using events, do it manually
                    CollectionUtils.RemoveAll(front.Keys, listUnlockable);
                }
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="ICacheEntry"/> instances
        /// within the cache.
        /// </summary>
        public virtual ICollection Entries
        {
            get
            {
                ICollection entries = BackCache.Entries;
                if (!IsCoherent)
                {
                    entries = new ArrayList(entries);
                }
                return entries;
            }
        }

        /// <summary>
        /// Returns an <see cref="ICacheEnumerator"/> object for the
        /// <b>ICache</b> instance.
        /// </summary>
        /// <returns>An <b>ICacheEnumerator</b> object for the
        /// <b>ICache</b> instance.</returns>
        ICacheEnumerator ICache.GetEnumerator()
        {
            return BackCache.GetEnumerator();
        }

        #endregion

        #region IDisposable interface implementation

        /// <summary>
        /// Calls <see cref="Release"/> to release the resources associated with this cache.
        /// </summary>
        public void Dispose()
        {
            Release();
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Invalidate the key from the front.
        /// </summary>
        /// <remarks>
        /// The caller must have the key locked.
        /// </remarks>
        /// <param name="key">
        /// The key to invalidate.
        /// </param>
        protected virtual void InvalidateFront(object key)
        {
            if (FrontCache.Contains(key))
            {
                FrontCache.Remove(key);
                UnregisterListener(key);
                m_countInvalidationHits++;
            }
            else
            {
                m_countInvalidationMisses++;
            }
        }

        /// <summary>
        /// Helper method used by <see cref="Insert(object,object)"/> and
        /// <see cref="InsertAll"/> to perform common maintanence tasks after
        /// completing an operation against the back.
        /// </summary>
        /// <reamarks>
        /// This includes removing the keys from the control cache, and
        /// evaluating if it is safe to update the front with the "new"
        /// value.  The implementation makes use of the following assumption:
        /// if listEvents == IGNORE_LIST then key does not exist in the
        /// front, and there is no key based listener for it. Any key passed
        /// to this method must be locked in the control cache by the caller.
        /// </reamarks>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The new value.
        /// </param>
        /// <param name="listEvents">
        /// The event list associated with the key, or <c>null</c> if
        /// it must be looked up from the control cache.
        /// </param>
        /// <param name="millis">
        /// The number of milliseconds until the cache entry will expire.
        /// </param>
        private void FinalizeInsert(object key, object value, IList listEvents, long millis)
        {
            ICache                     front           = FrontCache;
            IConcurrentCache           cacheControl    = CacheControl;
            CompositeCacheStrategyType strategyTarget  = m_strategyTarget;
            CompositeCacheStrategyType strategyCurrent = m_strategyCurrent;

            if (strategyTarget == CompositeCacheStrategyType.ListenNone)
            {
                // we're not validating; simply update the front
                if (value != null)
                {
                    front.Insert(key, value, millis);
                }
            }
            else if (listEvents == IGNORE_LIST)
            {
                // IGNORE_LIST indicates that the entry is not already in the
                // front; we're not going to add it
                cacheControl.Remove(key);
            }
            else if (listEvents == null)
            {
                throw new InvalidOperationException("Encountered unexpected key "
                        + key + "; this may be caused by concurrent "
                        + "modification of the supplied key(s), or by an "
                        + "inconsistent hashCode() or equals() implementation.");
            }
            else
            {
                // validate events and update the front if possible
                lock (listEvents.SyncRoot)
                {
                    // put operation itself should generate one "natural"
                    // INSERT or UPDATE; anything else should be considered
                    // as an invalidating event
                    bool isValid;
                    if (value == null)
                    {
                        isValid = false;
                    }
                    else
                    {
                        switch (listEvents.Count)
                        {
                            case 0:
                                if (STRICT_SYNCHRO_LISTENER && 
                                    (strategyCurrent == CompositeCacheStrategyType.ListenLogical ||
                                     strategyCurrent == CompositeCacheStrategyType.ListenAll ||
                                     front.Contains(key)))
                                {
                                    CacheFactory.Log("Expected an insert/update for "
                                                     + key + ", but none have been received",
                                                     CacheFactory.LogLevel.Info);
                                    isValid = false;
                                }
                                else
                                {
                                    isValid = true;
                                }
                                break;
                            case 1:
                                CacheEventType type = ((CacheEventArgs) listEvents[0]).EventType;
                                isValid = type == CacheEventType.Inserted ||
                                          type == CacheEventType.Updated;
                                break;
                            default:
                                isValid = false;
                                break;
                        }
                    }

                    if (isValid)
                    {
                        if (front.Insert(key, value, millis) == null && 
                            strategyTarget == CompositeCacheStrategyType.ListenPresent)
                        {
                            // this entry was evicted from behind us, and thus
                            // we haven't been listening to its events for
                            // some time, so we may not have the current value
                            front.Remove(key);
                        }
                    }
                    else
                    {
                        InvalidateFront(key);
                    }

                    // remove event list from the control map; in this case
                    // it must be done while still under sycnhronization
                    cacheControl.Remove(key);
                }
            }
        }

        /// <summary>
        /// Validate the front cache entry for the specified back cache
        /// event.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> from the back cache.
        /// </param>
        protected virtual void Validate(CacheEventArgs evt)
        {
            IConcurrentCache cacheControl = CacheControl;
            object           key          = evt.Key;
            long             start        = 0;

            for (int i = 0;; ++i)
            {
                if (cacheControl.Lock(key, 0))
                {
                    try
                    {
                        IList listEvents = (IList) cacheControl[key];
                        if (listEvents == null)
                        {
                            if (!IsPriming(evt))
                            {
                                // not in use; invalidate front entry
                                InvalidateFront(key);
                            }
                        }
                        else
                        {
                            // this can only happen if the back map fires event on
                            // the caller's thread (e.g. LocalCache)
                            listEvents.Add(evt);
                        }
                        return;
                    }
                    finally
                    {
                        cacheControl.Unlock(key);
                    }
                }
                else
                {
                    // check for a key based action
                    IList listEvents = (IList) cacheControl[key];

                    if (listEvents == null)
                    {
                        // check for a global action
                        listEvents = (IList) cacheControl[GLOBAL_KEY];
                        if (listEvents == null)
                        {
                            // has not been assigned yet, or has been just
                            // removed or switched; try again
                            long now = DateTimeUtils.GetSafeTimeMillis();
                            if (start == 0)
                            {
                                start = now;
                            }
                            else if (i > 5000 && now - start > 5000)
                            {
                                // we've been spinning and have given the other
                                // thread ample time to register the event list;
                                // the control map is corrupt
                                CacheFactory.Log("Detected a state corruption on the key \""
                                                + key + "\", of type "
                                                + key.GetType().Name
                                                + " which is missing from the active key set "
                                                + cacheControl.Keys
                                                + ". This could be caused by a mutating or "
                                                + "inconsistent key implementation, or a "
                                                + "concurrent modification to the cache passed to "
                                                + GetType().Name + ".InsertAll()",
                                                 CacheFactory.LogLevel.Error);

                                InvalidateFront(key);
                                return;
                            }

                            continue;
                        }
                    }

                    lock (listEvents.SyncRoot)
                    {
                        IList listKey = (IList) cacheControl[key];
                        if (listEvents == listKey ||
                            (listKey == null && listEvents == cacheControl[GLOBAL_KEY]))
                        {
                            listEvents.Add(evt);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Lock the keys in the given set without blocking.
        /// </summary>
        /// <param name="setKeys">
        /// keys to lock in the control map.
        /// </param>
        /// <returns>
        ///  Set of keys that were successfully locked.
        /// </returns>
        protected virtual HashSet TryLock(HashSet setKeys)
        {
            IConcurrentCache cacheControl = CacheControl;
            HashSet          setLocked    = new HashSet();
            foreach (object key in setKeys)
            {
                if (cacheControl.Lock(key, 0L)) // don't block on lock
                {
                    setLocked.Add(key);
                }
            }
            return setLocked;
        }

        /// <summary>
        /// Check if the specified event is a "priming" one.
        /// </summary>
        /// <param name="evt">
        /// CacheEvent to check.
        /// </param>
        /// <returns>
        /// true iff the event is a Priming Event.
        /// </returns>
        protected static bool IsPriming(CacheEventArgs evt)
        {
            return evt.EventType == CacheEventType.Updated
                && evt.IsSynthetic || evt.IsPriming;
        }

        /// <summary>
        /// Set up a thread local Set to hold all the keys that might be evicted
        /// from the front cache.
        /// </summary>
        /// <returns>
        /// a Set to hold all the keys in the ThreadLocal object or null
        /// if the bulk unregistering is not needed.
        /// </returns>
        /// <since>12.2.1</since>
        protected HashSet SetKeyHolder()
        {
            if (EnsureInvalidationStrategy() == CompositeCacheStrategyType.ListenPresent
                && m_listener is CompositeCache.PrimingListener)
            {
                HashSet setKeys = new HashSet();
                s_tloKeys.Value = setKeys;
                return setKeys;
            }

        return null;
        }

        /// <summary>
        /// Remove the key holder from the ThreadLocal object.
        /// </summary>
        /// <since>12.2.1</since>
        protected void RemoveKeyHolder()
        {
            s_tloKeys.Value = null;
        }

        #endregion

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
            get { return m_stats.TotalGets; }
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
            get { return m_stats.TotalGetsMillis; }
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
            get { return m_stats.AverageGetMillis; }
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
            get { return m_stats.TotalPuts; }
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
            get { return m_stats.TotalPutsMillis; }
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
            get { return m_stats.AveragePutMillis; }
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
            get { return m_stats.CacheHits; }
        }

        /// <summary>
        /// Determine the total number of milliseconds (since that last
        /// statistics reset) for the "get" operations for which an entry
        /// existed in this cache.
        /// </summary>
        /// <value>
        /// The total number of milliseconds for the "get" operations that
        /// were hits.
        /// </value>
        public virtual long CacheHitsMillis
        {
            get { return m_stats.CacheHitsMillis; }
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
            get { return m_stats.AverageHitMillis; }
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
            get { return m_stats.CacheMisses; }
        }

        /// <summary>
        /// Determine the total number of milliseconds (since that last
        /// statistics reset) for the "get" operations for which no entry
        /// existed in this map.
        /// </summary>
        /// <value>
        /// The total number of milliseconds (since that last statistics
        /// reset) for the "get" operations that were misses.
        /// </value>
        public virtual long CacheMissesMillis
        {
            get { return m_stats.CacheMissesMillis; }
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
            get { return m_stats.AverageMissMillis; }
        }

        /// <summary>
        /// Determine the rough probability (0 &lt;= p &lt;= 1) that the next
        /// invocation will be a hit, based on the statistics collected since
        /// the last reset of the cache statistics.
        /// </summary>
        /// <value>
        /// The cache hit probability (0 &lt;= p &lt;= 1).
        /// </value>
        public virtual double HitProbability
        {
            get { return m_stats.HitProbability; }
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
        public long CachePrunes
        {
            get { return m_stats.CachePrunes; }
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
        public long CachePrunesMillis
        {
            get { return m_stats.CachePrunesMillis; }
        }

        /// <summary>
        /// Determine the average number of milliseconds per cache pruning.
        /// </summary>
        /// <value>
        /// The average number of milliseconds per cache pruning.
        /// </value>
        public double AveragePruneMillis
        {
            get { return m_stats.AveragePruneMillis; }
        }

        /// <summary>
        /// Reset the cache statistics.
        /// </summary>
        public virtual void ResetHitStatistics()
        {
            m_stats.ResetHitStatistics();
            m_countInvalidationHits   = 0;
            m_countInvalidationMisses = 0;
            m_countRegisterListener   = 0;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compares the specified object with this dictionary for equality.
        /// </summary>
        /// <param name="o">
        /// <b>Object</b> to be compared for equality with this dictionary.
        /// </param>
        /// <returns>
        /// <b>true</b> if the specified object is equal to this dictionary.
        /// </returns>
        public override bool Equals(object o)
        {
            return BackCache.Equals(o);
        }

        /// <summary>
        /// Return the hash code value for this dictionary.
        /// </summary>
        /// <returns>
        /// The hash code value for this dictionary.
        /// </returns>
        public override int GetHashCode()
        {
            return BackCache.GetHashCode();
        }

        /// <summary>
        /// For debugging purposes, format the contents of the
        /// <b>CompositeCache</b> in a human readable format.
        /// </summary>
        /// <returns>
        /// A <b>String</b> representation of the <b>CompositeCache</b>
        /// object.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("CompositeCache");
            try
            {
                ICache front = FrontCache;
                ICache back  = BackCache;

                string[] strategy = {"NONE", "PRESENT", "ALL", "AUTO", "LOGICAL"};
                sb.Append("{FrontCache{class=")
                        .Append(front.GetType().Name)
                        .Append(", size=")
                        .Append(front.Count)
                        .Append("}, BackCache{class=")
                        .Append(back.GetType().Name)
                        .Append(", size=")
                        .Append(back.Count)
                        .Append("}, strategy=")
                        .Append(strategy[(int) InvalidationStrategy])
                        .Append(", CacheStatistics=")
                        .Append(CacheStatistics)
                        .Append(", invalidation hits=")
                        .Append(InvalidationHits)
                        .Append(", invalidation misses=")
                        .Append(InvalidationMisses)
                        .Append(", listener registrations=")
                        .Append(TotalRegisterListener)
                        .Append('}');
            }
            catch (InvalidOperationException)
            {
                sb.Append(" not active");
            }
            return sb.ToString();
        }

        #endregion

        #region  Back Cache listener support

        /// <summary>
        /// Register the global back cache listener.
        /// </summary>
        protected virtual void RegisterListener()
        {
            ((IObservableCache) BackCache).AddCacheListener(m_listener, m_filterListener, true);
        }

        /// <summary>
        /// Unregister the global back cache listener.
        /// </summary>
        protected virtual void UnregisterListener()
        {
            ((IObservableCache) BackCache).RemoveCacheListener(m_listener, m_filterListener);
        }

        /// <summary>
        /// Register the back cache listener for the specified key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        protected virtual void RegisterListener(object key)
        {
            if (EnsureInvalidationStrategy() == CompositeCacheStrategyType.ListenPresent)
            {
                try
                {
                    ((IObservableCache) BackCache).AddCacheListener(m_listener, key, true);
                }
                catch (NotSupportedException)
                {
                    // the back is of an older version; need to reset the
                    // "old" non-priming listener
                    m_listener = InstantiateBackCacheListener(CompositeCacheStrategyType.ListenAll);
                    ((IObservableCache) BackCache).AddCacheListener(m_listener, key, true);
                }
                m_countRegisterListener++;
            }
        }

        /// <summary>
        /// Register the back map listeners for the specified set of keys.
        /// </summary>
        /// <param name="setKeys">
        /// The key set.
        /// </param>
        protected virtual void RegisterListeners(HashSet setKeys)
        {
            if (EnsureInvalidationStrategy() == CompositeCacheStrategyType.ListenPresent)
            {
                if (m_listener is CompositeCache.PrimingListener)
                {
                    try
                    {
                        ((IObservableCache)BackCache).AddCacheListener(m_listener, new InKeySetFilter(null, setKeys), true);
                        m_countRegisterListener += setKeys.Count;
                        return;
                    }
                    catch (NotSupportedException)
                    {
                        // the back is of an older version; need to reset the
                        // "old" non-priming listener
                        m_listener = InstantiateBackCacheListener(CompositeCacheStrategyType.ListenAll);
                    }
                }
                // use non-optimized legacy algorithm
                foreach (object key in setKeys)
                {
                    RegisterListener(key);
                }
            }
        }

        /// <summary>
        /// Unregister the back cache listener for the specified key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        protected virtual void UnregisterListener(object key)
        {
            if (m_strategyCurrent == CompositeCacheStrategyType.ListenPresent)
            {
                IConcurrentCache cacheControl = CacheControl;
                if (cacheControl.Lock(key, 0))
                {
                    if (m_listener is CompositeCache.PrimingListener)
                    {
                        HashSet setKeys = s_tloKeys.Value;
                        if (setKeys != null)
                        {
                            setKeys.Add(key);

                            // the key is still locked; it will be unlocked
                            // along with other keys after bulk un-registration
                            // in the unregisterListeners(setKeys) method
                            return;
                        }
                    }

                    try
                    {
                        ((IObservableCache) BackCache).RemoveCacheListener(m_listener, key);
                    }
                    finally
                    {
                        cacheControl.Unlock(key);
                    }
                }

            }
        }

        /// <summary>
        /// Unregister the back cache listener for the specified keys.
        /// <p> 
        /// Note: all the keys in the passed-in set must be locked and will be unlocked.
        /// </p>
        /// </summary>
        /// <param name="setKeys">
        /// Set of keys to unregister (and unlock).
        /// </param>
        protected virtual void UnregisterListeners(ICollection setKeys)
        {
            if (m_strategyCurrent == CompositeCacheStrategyType.ListenPresent
                && m_listener is CompositeCache.PrimingListener)
            {
                if (setKeys.Count > 0)
                {
                    try
                    {
                        ((IObservableCache)BackCache).RemoveCacheListener(m_listener, new InKeySetFilter(null, setKeys));
                    }
                    finally
                    {
                        IConcurrentCache cacheControl = CacheControl;
                        foreach (object key in setKeys)
                        {
                            cacheControl.Unlock(key);
                        }
                    }
                }
            }
            else
            {
                throw new NotSupportedException("unregisterListeners can only be called with PRESENT strategy");
            }
        }

        #endregion

        #region Front Cache listener support

        /// <summary>
        /// Register the global front cache listener.
        /// </summary>
        protected virtual void RegisterFrontListener()
        {
            FrontCacheListener listener = m_listenerFront;
            if (listener != null)
            {
                listener.Register();
            }
        }

        /// <summary>
        /// Unregister the global front cache listener.
        /// </summary>
        protected virtual void UnregisterFrontListener()
        {
            FrontCacheListener listener = m_listenerFront;
            if (listener != null)
            {
                listener.Unregister();
            }
        }

        #endregion

        #region Invalidation Strategy methods

        /// <summary>
        /// Ensure that a strategy has been choosen and that any appropriate
        /// global listeners have been registered.
        /// </summary>
        /// <returns>
        /// The current strategy.
        /// </returns>
        protected virtual CompositeCacheStrategyType EnsureInvalidationStrategy()
        {
            // the situation in which
            // (m_strategyCurrent != m_strategyTarget)
            // can happen either at the first cache access following the
            // instantiation or after ResetInvalidationStrategy() is called
            CompositeCacheStrategyType strategyTarget = m_strategyTarget;
            switch (strategyTarget)
            {
                case CompositeCacheStrategyType.ListenAuto:
                    // as of Coherence 12.1.2, default LISTEN_AUTO to LISTEN_PRESENT
                case CompositeCacheStrategyType.ListenPresent:
                    if (m_strategyCurrent != CompositeCacheStrategyType.ListenPresent)
                    {
                        lock (GLOBAL_KEY)
                        {
                            if (m_strategyCurrent != CompositeCacheStrategyType.ListenPresent)
                            {
                                RegisterFrontListener();
                                RegisterDeactivationListener();

                                m_strategyCurrent = CompositeCacheStrategyType.ListenPresent;
                            }
                        }
                    }
                    return CompositeCacheStrategyType.ListenPresent;

                case CompositeCacheStrategyType.ListenLogical:
                case CompositeCacheStrategyType.ListenAll:
                    if (m_strategyCurrent != strategyTarget)
                    {
                        lock (GLOBAL_KEY)
                        {
                            if (m_strategyCurrent != strategyTarget)
                            {
                                if (strategyTarget == CompositeCacheStrategyType.ListenLogical)
                                {
                                    // LOGICAL behaves like ALL, but with synthetic deletes filtered out
                                    m_filterListener = new NotFilter(
                                                     new CacheEventFilter(CacheEventFilter.CacheEventMask.Deleted,
                                                                          CacheEventFilter.CacheEventSyntheticMask.Synthetic));
                                }
                                RegisterListener();
                                RegisterDeactivationListener();

                                m_strategyCurrent = strategyTarget;
                            }
                        }
                    }
                    return strategyTarget;
            }
            return CompositeCacheStrategyType.ListenNone;
        }

        /// <summary>
        /// Reset the "current invalidation strategy" flag.
        /// </summary>
        /// <remarks>
        /// This method should be called <b>only</b> while the access to the
        /// front cache is fully synchronzied and the front cache
        /// is empty to prevent stalled data.
        /// </remarks>
        protected virtual void ResetInvalidationStrategy()
        {
            m_strategyCurrent = CompositeCacheStrategyType.ListenNone;
            m_filterListener  = null;
        }

        #endregion

        #region BackCacheListener factory method

        /// <summary>
        /// Factory pattern: instantiate back cache listener.
        /// </summary>
        /// <param name="strategy">
        /// CompositeCacheStrategyType.
        /// </param>
        /// <returns>
        /// An instance of back cache listener responsible for keeping the
        /// front cache coherent with the back cache.
        /// </returns>
        protected virtual ICacheListener InstantiateBackCacheListener(CompositeCacheStrategyType strategy)
        {
            return strategy == CompositeCacheStrategyType.ListenAuto || strategy == CompositeCacheStrategyType.ListenPresent
                                ? (ICacheListener) new PrimingListener(this)
                                : (ICacheListener) new SimpleListener(this);
        }

        #endregion

        #region Deactivation Listener support

        /// <summary>
        /// Instantiate and register a DeactivationListener with the back cache.
        /// </summary>
        protected virtual void RegisterDeactivationListener()
            {
            try
                {
                INamedCacheDeactivationListener listener = m_listenerDeactivation;
                ICache                          back     = BackCache;
                if (listener != null && back is INamedCache)
                    {
                    ((INamedCache) BackCache).AddCacheListener(listener);
                    }
                }
            catch (Exception) {};
            }

        /// <summary>
        /// Unregister back cache deactivation listener.
        /// </summary>
        protected void UnregisterDeactivationListener()
            {
            try
                {
                INamedCacheDeactivationListener listener = m_listenerDeactivation;
                ICache                          back     = BackCache;
                if (listener != null && back is INamedCache)
                    {
                    ((INamedCache) back).RemoveCacheListener(listener);
                    }
                }
            catch (Exception) {}
            }

        /// <summary>
        /// Reset the front map.
        /// </summary>
        public void ResetFrontMap()
            {
            try
                {
                UnregisterFrontListener();
                FrontCache.Clear();
                }
            catch (Exception) {}

            ResetInvalidationStrategy();
            }

        #endregion

         #region Inner class: DeactivationListener

        /// <summary>
        /// DeactivationListener for the back NamedCache.
        /// </summary>
        /// <remarks>
        /// The primary goal of that listener is invalidation of the front map
        /// when the back cache is destroyed or all storage nodes are stopped.
        /// </remarks>
        /// <since>12.1.3</since>
        protected class DeactivationListener : AbstractCacheListener, INamedCacheDeactivationListener
            {
            #region Constructors

            /// <summary>
            /// Constructor that passes the reference of the parent object.
            /// </summary>
            /// <param name="parent">
            /// The reference to the instatnce of the parent class.
            /// </param>
            public DeactivationListener(CompositeCache parent)
            {
                m_parent = parent;
            }

            #endregion

            #region AbstractCacheListener override methods

            /// <summary>
            /// Invoked when a back cache is destroyed or all storage nodes are stopped.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/>
            /// </param>
            public override void EntryDeleted(CacheEventArgs evt)
            {
                m_parent.ResetFrontMap();
            }

            /// <summary>
            /// Invoked when a back cache is truncated.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/>
            /// </param>
            public override void EntryUpdated(CacheEventArgs evt)
            {
                m_parent.ResetFrontMap();
            }
            
            #endregion

            #region Data members

            /// <summary>
            /// The parent class reference.
            /// </summary>
            private readonly CompositeCache m_parent;

            #endregion
            }

        #endregion

        #region Inner class: PrimingListener

        /// <summary>
        /// <see cref="ICacheListener"/> for back cache responsible for
        /// keeping the front cache coherent with the back cache.
        /// </summary>
        /// <remarks>
        /// This listener is registered as a synchronous listener for lite
        /// events (carrying only a key) and generates a "priming" event when registered.
        /// </remarks>
        /// <since>12.2.1</since>
        protected class PrimingListener : MultiplexingCacheListener, CacheListenerSupport.IPrimingListener
        {
            #region Constructors

            /// <summary>
            /// Constructor that passes the reference of the parent object.
            /// </summary>
            /// <param name="parent">
            /// The reference to the instatnce of the parent class.
            /// </param>
            public PrimingListener(CompositeCache parent)
            {
                m_parent = parent;
            }

            #endregion

            #region MultiplexingCacheListener override methods

            /// <summary>
            /// Invoked for any event.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/>.
            /// </param>
            protected override void OnCacheEvent(CacheEventArgs evt)
            {
                m_parent.Validate(evt);
            }

            #endregion

            #region Data members

            /// <summary>
            /// The parent class reference.
            /// </summary>
            private readonly CompositeCache m_parent;

            #endregion
        }

        #endregion

        #region Inner class: SimpleListener

        /// <summary>
        /// <see cref="ICacheListener"/> for back cache responsible for
        /// keeping the front cache coherent with the back cache.
        /// </summary>
        /// <remarks>
        /// This listener is registered as a synchronous listener for lite
        /// events (carrying only a key).
        /// </remarks>
        protected class SimpleListener : MultiplexingCacheListener, CacheListenerSupport.ISynchronousListener
        {
            #region Constructors

            /// <summary>
            /// Constructor that passes the reference of the parent object.
            /// </summary>
            /// <param name="parent">
            /// The reference to the instatnce of the parent class.
            /// </param>
            public SimpleListener(CompositeCache parent)
            {
                m_parent = parent;
            }

            #endregion

            #region MultiplexingCacheListener override methods

            /// <summary>
            /// Invoked for any event.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/>.
            /// </param>
            protected override void OnCacheEvent(CacheEventArgs evt)
            {
                m_parent.Validate(evt);
            }

            #endregion

            #region Data members

            /// <summary>
            /// The parent class reference.
            /// </summary>
            private readonly CompositeCache m_parent;

            #endregion
        }

        #endregion

        #region FrontCacheListener factory method

        /// <summary>
        /// Factory pattern: instantiate front cache listener.
        /// </summary>
        /// <returns>
        /// An instance of front cache listener.
        /// </returns>
        protected virtual FrontCacheListener InstantiateFrontCacheListener()
        {
            return new FrontCacheListener(this);
        }

        #endregion

        #region Inner class: FrontCacheListener

        /// <summary>
        /// <see cref="ICacheListener"/> for front cache responsible for
        /// deregistering back cache listeners upon front cache eviction.
        /// </summary>
        protected class FrontCacheListener : AbstractCacheListener, CacheListenerSupport.ISynchronousListener
        {
            #region Constructors

            /// <summary>
            /// Constructor that passes the reference of the parent object.
            /// </summary>
            /// <param name="parent">
            /// The reference to the instatnce of the parent class.
            /// </param>
            public FrontCacheListener(CompositeCache parent)
            {
                m_parent = parent;
            }

            #endregion

            #region AbstractCacheListener override methods

            /// <summary>
            /// Invoked when a cache entry has been deleted.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/> carrying the remove
            /// information.
            /// </param>
            public override void EntryDeleted(CacheEventArgs evt)
            {
                if (evt.IsSynthetic)
                {
                    m_parent.UnregisterListener(evt.Key);
                }
            }

            #endregion

            #region Helper registation methods

            /// <summary>
            /// Register this listener with the "front" cache.
            /// </summary>
            public virtual void Register()
            {
                ((IObservableCache) m_parent.FrontCache).AddCacheListener(this, m_filter, true);
            }

            /// <summary>
            /// Unregister this listener with the "front" cache.
            /// </summary>
            public virtual void Unregister()
            {
                ((IObservableCache) m_parent.FrontCache).RemoveCacheListener(this, m_filter);
            }

            #endregion

            #region Data members

            /// <summary>
            /// The filter associated with this listener.
            /// </summary>
            protected IFilter m_filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.Deleted);

            /// <summary>
            /// The parent class reference.
            /// </summary>
            private readonly CompositeCache m_parent;

            #endregion
        }

        #endregion

        #region Internal collections

        /// <summary>
        /// List that ignores any Add operations.
        /// </summary>
        internal class IgnoreList : ArrayList
        {
            /// <summary>
            /// Adds an object to the end of the list.
            /// </summary>
            /// <remarks>
            /// Add operation is ignored in this list.
            /// </remarks>
            /// <param name="value">
            /// Value to add.
            /// </param>
            /// <returns>
            /// Always -1.
            /// </returns>
            public override int Add(object value)
            {
                return -1;
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// Specifies whether the back cache listener strictly adheres
        /// to the <see cref="CacheListenerSupport.ISynchronousListener"/>
        /// contract.
        /// </summary>
        //TODO: property reading
        private const bool STRICT_SYNCHRO_LISTENER = true;
        //private static readonly bool STRICT_SYNCHRO_LISTENER = "true".Equals(
        //        System.getProperty("tangosol.coherence.near.strictlistener", "true"));

        /// <summary>
        /// The "back" cache, considered to be "complete" yet "expensive" to
        /// access.
        /// </summary>
        private ICache m_back;

        /// <summary>
        /// The "front" cache, considered to be "incomplete" yet
        /// "inexpensive" to access.
        /// </summary>
        private ICache m_front;

        /// <summary>
        /// The invalidation strategy that this cache is to use.
        /// </summary>
        protected CompositeCacheStrategyType m_strategyTarget;

        /// <summary>
        /// The current invalidation strategy, which at times could be
        /// different from the target strategy.
        /// </summary>
        protected CompositeCacheStrategyType m_strategyCurrent;

        /// <summary>
        /// An optional listener for the "back" cache.
        /// </summary>
        private ICacheListener m_listener;

        /// <summary>
        /// An optional listener for the "front" cache.
        /// </summary>
        private readonly FrontCacheListener m_listenerFront;

        /// <summary>
        /// A filter that selects events for the back map listener.
        /// </summary>
        private IFilter m_filterListener;
        
        /// <summary>
        /// A filter that selects events for the back map listener.
        /// </summary>
        private INamedCacheDeactivationListener m_listenerDeactivation;

        /// <summary>
        /// The <see cref="IConcurrentCache"/> to keep track of front cache
        /// updates.
        /// </summary>
        /// <remarks>
        /// Values are list of events received by the listener while the
        /// corresponding key was locked.
        /// </remarks>
        private readonly IConcurrentCache m_cacheControl = new LocalCache();

        /// <summary>
        /// The <see cref="ICacheStatistics"/> object maintained by this
        /// cache.
        /// </summary>
        private readonly SimpleCacheStatistics m_stats = new SimpleCacheStatistics();

        /// <summary>
        /// The rough (ie unsynchronized) number of times the front cache
        /// entries that were present in the front cache were invalidated by
        /// the listener.
        /// </summary>
        private long m_countInvalidationHits;

        /// <summary>
        /// The rough (ie unsynchronized) number of times the front cache
        /// entries that were absent in the front cache received invalidation
        /// event.
        /// </summary>
        private long m_countInvalidationMisses;

        /// <summary>
        /// The total number of RegisterListener(key) operations.
        /// </summary>
        private long m_countRegisterListener;

         /// <summary>
        /// The ThreadLocal to hold all the keys that are evicted while the front cache
        /// is updated during get or getAll operation.
        /// </summary>
        /// <since>12.2.1</since>
        protected static ThreadLocal<HashSet> s_tloKeys = new ThreadLocal<HashSet>();

        /// <summary>
        /// A unique <b>Object</b> that serves as a control key for global
        /// operations such as clear and release and synchronization point
        /// for the current strategy change.
        /// </summary>
        private readonly object GLOBAL_KEY = new object();

        /// <summary>
        /// Empty list that ignores any add operations.
        /// </summary>
        private static readonly IList IGNORE_LIST = new IgnoreList();

        #endregion

    }

    #region Enum: CompositeCacheStrategyType

    /// <summary>
    /// Type of <b>CompositeCache</b> invalidation strategy.
    /// </summary>
    public enum CompositeCacheStrategyType
    {
        /// <summary>
        /// No invalidation strategy.
        /// </summary>
        ListenNone    = 0,

        /// <summary>
        /// Invalidation strategy that instructs the <b>CompositeCache</b>
        /// to listen to the back dictionary events related <b>only</b> to
        /// the items currently present in the front dictionary.
        /// </summary>
        /// <remarks>
        /// This strategy serves best when the changes to the back dictionary
        /// come mostly from the CompositeCache itself.
        /// </remarks>
        ListenPresent = 1,

        /// <summary>
        /// Invalidation strategy that instructs the <b>CompositeCache</b>
        /// to listen to <b>all</b> back dictionary events.
        /// </summary>
        /// <remarks>
        /// This strategy is preferred when updates to the back dictionary
        /// are frequent and with high probability come from the outside of
        /// this <b>CompositeCache</b>; for example multiple
        /// <b>CompositeCache</b> instances using the same back dictionary
        /// with a large degree of key set overlap between front dictionaries.
        /// </remarks>
        ListenAll     = 2,

        /// <summary>
        /// Invalidation strategy that instructs the <b>CompositeCache</b>
        /// implementation to switch automatically between ListenPresent and
        /// ListenAll strategies based on the cache statistics.
        /// </summary>
        ListenAuto    = 3,

        /// <summary>
        /// Invalidation strategy that instructs the <b>CompositeCache</b>
        /// to listen to <b>all</b> back map events that are <b>not synthetic
        /// </b>.  A synthetic event could be emitted as a result of eviction
        /// or expiration.  With this invalidation stategy, it is possible for
        /// the front map to contain cache entries that have been synthetically
        /// removed from the back (though any subsequent re-insertion will
        /// cause the corresponding entries in the front map to be invalidated).
        /// </summary>
        ListenLogical = 4
    }

    #endregion
}
