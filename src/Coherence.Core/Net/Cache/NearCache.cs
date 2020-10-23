/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Util;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// A "near cache" is a <see cref="CompositeCache"/> whose front
    /// cache is a size-limited and/or auto-expiring local cache,
    /// and whose back cache is a distributed cache.
    /// </summary>
    /// <remarks>
    /// A <b>CompositeCache</b> is a cache that has a "front" cache and a
    /// "back" cache; the front cache is assumed to be low latency but
    /// incomplete, and the back cache is assumed to be complete but high
    /// latency.
    /// </remarks>
    /// <seealso cref="CompositeCache"/>
    /// <author>Alex Gleyzer, Cameron Purdy  2002.10.20</author>
    /// <author>Gene Gleyzer  2003.10.16</author>
    /// <author>Ivan Cikic  2006.11.13</author>
    public class NearCache : CompositeCache, INamedCache
    {
        #region Properties

        /// <summary>
        /// Obtain the <see cref="INamedCache"/> object that sits behind this
        /// <b>NearCache</b>.
        /// </summary>
        /// <value>
        /// The <b>INamedCache</b> object, which is the back cache of this
        /// <b>NearCache</b>.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If this <b>INamedCache</b> has been released.
        /// </exception>
        new public virtual INamedCache BackCache
        {
            get { return (INamedCache) base.BackCache; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a <b>NearCache</b>, using a <i>back</i>
        /// <see cref="INamedCache"/> as the complete (back) storage and
        /// <i>front</i> <see cref="ICache"/> as a near (front) storage using
        /// the <see cref="CompositeCacheStrategyType"/> invalidation
        /// strategy.
        /// </summary>
        /// <param name="front">
        /// <b>ICache</b> to put in front of the back cache.
        /// </param>
        /// <param name="back">
        /// <b>INamedCache</b> to put behind the front cache.
        /// </param>
        public NearCache(ICache front, INamedCache back)
                : this(front, back, CompositeCacheStrategyType.ListenAuto)
        {}

        /// <summary>
        /// Construct a <b>NearCache</b>, using a <i>back</i>
        /// <see cref="INamedCache"/> as the complete (back) storage and
        /// <i>front</i> <see cref="ICache"/> as a near (front) storage using
        /// the <see cref="CompositeCacheStrategyType"/> invalidation
        /// strategy.
        /// </summary>
        /// <param name="front">
        /// <b>ICache</b> to put in front of the back cache.
        /// </param>
        /// <param name="back">
        /// <b>INamedCache</b> to put behind the front cache.
        /// </param>
        /// <param name="strategy">
        /// Specifies the strategy used for the front cache
        /// invalidation; valid values are:
        /// <see cref="CompositeCacheStrategyType.ListenNone"/>
        /// <see cref="CompositeCacheStrategyType.ListenPresent"/>
        /// <see cref="CompositeCacheStrategyType.ListenAll"/>
        /// <see cref="CompositeCacheStrategyType.ListenAuto"/>
        /// </param>
        /// <since>Coherence 2.3</since>
        public NearCache(ICache front, INamedCache back, CompositeCacheStrategyType strategy)
                : base(front, back, strategy)
        {
            m_name    = back.CacheName;
            m_service = back.CacheService;
            RegisterBackServiceMemberEventHandler();
        }

        #endregion

        #region INamedCache implementation

        /// <summary>
        /// Gets the cache name.
        /// </summary>
        /// <value>
        /// The cache name.
        /// </value>
        public virtual string CacheName
        {
            get { return m_name; }
        }

        /// <summary>
        /// Gets the <see cref="ICacheService"/> that this INamedCache is a
        /// part of.
        /// </summary>
        /// <value>
        /// The cache service this INamedCache is a part of.
        /// </value>
        public virtual ICacheService CacheService
        {
            get  { return m_service; }
        }

        /// <summary>
        /// Specifies whether or not the INamedCache is active.
        /// </summary>
        /// <value>
        /// <b>true</b> if the INamedCache is active; <b>false</b> otherwise.
        /// </value>
        public virtual bool IsActive
        {
            get
            {
                try
                {
                    return FrontCache != null && BackCache.IsActive;
                }
                catch(InvalidOperationException)
                {
                    return false;
                }
            }
        }

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
        /// </remarks>
        public override void Release()
        {
            Release(false);
        }

        /// <summary>
        /// Release and destroy this instance of INamedCache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// <b>Warning:</b> This method is used to completely destroy the
        /// specified cache across the cluster. All references in the entire
        /// cluster to this cache will be invalidated, the cached data will
        /// be cleared, and all resources will be released.</p>
        /// </remarks>
        public virtual void Destroy()
        {
            Release(true);
        }

        /// <summary>
        /// Remove all mappings of this instance of INamedCache.
        /// </summary>
        public virtual void Truncate()
        {
            BackCache.Truncate();
        }

        /// <summary>
        /// Construct a view of this INamedCache.
        /// </summary>
        /// <returns>A local view for this INamedCache</returns>
        /// <see cref="ViewBuilder"/>
        /// <since>12.2.1.4</since>
        public virtual ViewBuilder View()
        {
            return new ViewBuilder(this);
        }

        #endregion

        #region IObservableCache implementation

        /// <summary>
        /// Add a standard cache listener that will receive all events
        /// (inserts, updates, deletes) that occur against the cache, with
        /// the key, old-value and new-value included.
        /// </summary>
        /// <remarks>
        /// Expensive: Listening always occurs on the back cache.
        /// </remarks>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to add.
        /// </param>
        public virtual void AddCacheListener(ICacheListener listener)
        {
            BackCache.AddCacheListener(listener);
        }

        /// <summary>
        /// Remove a standard cache listener that previously signed up for
        /// all events.
        /// </summary>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to remove.
        /// </param>
        public virtual void RemoveCacheListener(ICacheListener listener)
        {
            BackCache.RemoveCacheListener(listener);
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
        /// <see cref="RemoveCacheListener(ICacheListener, object)"/>
        /// method.</p>
        /// <p>
        /// Expensive: Listening always occurs on the back cache.</p>
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
            BackCache.AddCacheListener(listener, key, isLite);
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
            BackCache.RemoveCacheListener(listener, key);
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
        /// <see cref="RemoveCacheListener(ICacheListener, IFilter)"/>
        /// method.</p>
        /// <p>
        /// Expensive: Listening always occurs on the back cache.</p>
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
            BackCache.AddCacheListener(listener, filter, isLite);
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
            BackCache.RemoveCacheListener(listener, filter);
        }

        #endregion

        #region IConcurrentCache implementation

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
        /// <p>
        /// Expensive: Locking always occurs on the back cache.</p>
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
            if (BackCache.Lock(key, waitTimeMillis))
            {
                // back cache listeners are always synchronous, so if there
                // is one the front cache invalidation is not necessary
                if (InvalidationStrategy == CompositeCacheStrategyType.ListenNone)
                {
                    FrontCache.Remove(key);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Attempt to lock the specified item and return immediately.
        /// </summary>
        /// <remarks>
        /// This method behaves exactly as if it simply performs the call
        /// <b>Lock(key, 0)</b>.
        /// <p>
        /// Expensive: Locking always occurs on the back cache.</p>
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
            return BackCache.Unlock(key);
        }

        #endregion

        #region IQueryCache implementation

        /// <summary>
        /// Return a collection of the keys contained in this cache for
        /// entries that satisfy the criteria expressed by the filter.
        /// </summary>
        /// <remarks>
        /// The operation always executes against the back cache.
        /// </remarks>
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
            return BackCache.GetKeys(filter);
        }

        /// <summary>
        /// Return a collection of the values contained in this cache for
        /// entries that satisfy the criteria expressed by the filter.
        /// </summary>
        /// <remarks>
        /// The operation always executes against the back cache.
        /// </remarks>
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
            return BackCache.GetValues(filter);
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
        /// <p>
        /// The operation always executes against the back cache.</p>
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
            return BackCache.GetValues(filter, comparer);
        }

        /// <summary>
        /// Return a collection of the entries contained in this cache
        /// that satisfy the criteria expressed by the filter.
        /// </summary>
        /// <remarks>
        /// The operation always executes against the back cache.
        /// </remarks>
        /// <param name="filter">
        /// The <see cref="IFilter"/> object representing the criteria that
        /// the entries of this cache should satisfy.
        /// </param>
        /// <returns>
        /// A collection of entries that satisfy the specified criteria.
        /// </returns>
        public virtual ICacheEntry[] GetEntries(IFilter filter)
        {
            return BackCache.GetEntries(filter);
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
        /// <p>
        /// The operation always executes against the back cache.</p>
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
            return BackCache.GetEntries(filter, comparer);
        }

        /// <summary>
        /// Add an index to this IQueryCache.
        /// </summary>
        /// <remarks>
        /// This allows to correlate values stored in this
        /// <i>indexed cache</i> (or attributes of those values) to the
        /// corresponding keys in the indexed cache and increase the
        /// performance of <b>GetKeys</b> and <b>GetEntries</b> methods.
        /// <p>
        /// The operation always executes against the back cache.</p>
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
            BackCache.AddIndex(extractor, isOrdered, comparer);
        }

        /// <summary>
        /// Remove an index from this IQueryCache.
        /// </summary>
        /// <remarks>
        /// The operation always executes against the back cache.
        /// </remarks>
        /// <param name="extractor">
        /// The <see cref="IValueExtractor"/> object that is used to extract
        /// an indexable object from a value stored in the cache.
        /// </param>
        public virtual void RemoveIndex(IValueExtractor extractor)
        {
            BackCache.RemoveIndex(extractor);
        }

        #endregion

        #region IInvocableCache implementation

        /// <summary>
        /// Invoke the passed <see cref="IEntryProcessor"/> against the entry
        /// specified by the passed key, returning the result of the
        /// invocation.
        /// </summary>
        /// <remarks>
        /// The operation always executes against the back cache.
        /// </remarks>
        /// <param name="key">
        /// The key to process; it is not required to exist within the
        /// cache.
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
            return BackCache.Invoke(key, agent);
        }

        /// <summary>
        /// Invoke the passed <see cref="IEntryProcessor"/> against the
        /// entries specified by the passed keys, returning the result of the
        /// invocation for each.
        /// </summary>
        /// <remarks>
        /// The operation always executes against the back cache.
        /// </remarks>
        /// <param name="keys">
        /// The keys to process; these keys are not required to exist within
        /// the cache.
        /// </param>
        /// <param name="agent">
        /// The <b>IEntryProcessor</b> to use to process the specified keys.
        /// </param>
        /// <returns>
        /// A cache containing the results of invoking the
        /// <b>IEntryProcessor</b> against each of the specified keys.
        /// </returns>
        public virtual IDictionary InvokeAll(ICollection keys, IEntryProcessor agent)
        {
            return BackCache.InvokeAll(keys, agent);
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
        /// <p>
        /// The operation always executes against the back cache.</p>
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
        /// A cache containing the results of invoking the
        /// <b>IEntryProcessor</b> against the keys that are selected by the
        /// given <b>IFilter</b>.
        /// </returns>
        public virtual IDictionary InvokeAll(IFilter filter, IEntryProcessor agent)
        {
            return BackCache.InvokeAll(filter, agent);
        }

        /// <summary>
        /// Perform an aggregating operation against the entries specified by
        /// the passed keys.
        /// </summary>
        /// <remarks>
        /// The operation always executes against the back cache.
        /// </remarks>
        /// <param name="keys">
        /// The collection of keys that specify the entries within this cache
        /// to aggregate across.
        /// </param>
        /// <param name="agent">
        /// The <see cref="IEntryAggregator"/> that is used to aggregate
        /// across the specified entries of this cache.
        /// </param>
        /// <returns>
        /// The result of the aggregation.
        /// </returns>
        public virtual object Aggregate(ICollection keys, IEntryAggregator agent)
        {
            return BackCache.Aggregate(keys, agent);
        }

        /// <summary>
        /// Perform an aggregating operation against the collection of
        /// entries that are selected by the given <b>IFilter</b>.
        /// </summary>
        /// <remarks>
        /// The operation always executes against the back cache.
        /// </remarks>
        /// <param name="filter">
        /// an <see cref="IFilter"/> that is used to select entries within
        /// this cache to aggregate across.
        /// </param>
        /// <param name="agent">
        /// The <see cref="IEntryAggregator"/> that is used to aggregate
        /// across the selected entries of this cache.
        /// </param>
        /// <returns>
        /// The result of the aggregation.
        /// </returns>
        public virtual object Aggregate(IFilter filter, IEntryAggregator agent)
        {
            return BackCache.Aggregate(filter, agent);
        }

        #endregion

        #region Member event handlers

        /// <summary>
        /// Invoked when an <see cref="IMember"/> has joined the service.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The most critical situation arises when a number of threads are
        /// waiting for a local service restart, being blocked by a
        /// <b>IService</b> object synchronization monitor. Since the Joined
        /// event should be fired only once, it is called on a client thread
        /// <b>while holding a synchronization monitor</b>. An attempt to use
        /// other clustered service functionality during this local event
        /// notification may result in a deadlock.</p>
        /// </remarks>
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// An event which indicates that membership has changed.
        /// </param>
        public virtual void OnMemberJoined(object sender, MemberEventArgs evt)
        {
            ResetFrontMap();
        }

        /// <summary>
        /// Invoked when an <see cref="IMember"/> is leaving the service.
        /// </summary>
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// An event which indicates that membership has changed.
        /// </param>
        public virtual void OnMemberLeaving(object sender, MemberEventArgs evt)
        {}

        /// <summary>
        /// Invoked when an <see cref="IMember"/> has left the service.
        /// </summary>
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// An event which indicates that membership has changed.
        /// </param>
        public virtual void OnMemberLeft(object sender, MemberEventArgs evt)
        {
            if (InvalidationStrategy != CompositeCacheStrategyType.ListenNone)
            {
                ResetFrontMap();
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Release this cache, optionally destroying it.
        /// </summary>
        /// <param name="fDestroy">
        /// If true, destroy the cache as well.
        /// </param> 
        protected virtual void Release(Boolean fDestroy)
        {
            try
            {
                INamedCache cache = BackCache;
                UnregisterBackServiceMemberEventHandler();
                base.Release();
                if (fDestroy)
                {
                    cache.Destroy();
                }
                else
                {
                    cache.Release();
                }
            }
            catch (Exception)
            {
                // one of the following should be ignored:
                // IllegalOperationException("Cache is not active");
                // Exception("Storage is not configured");
                // Exception("Service has been terminated");
            }
        }

        /// <summary>
        /// Register an event handler for member events on back caches's
        /// service.
        /// </summary>
        /// <remarks>
        /// The primary goal of that event handler is invalidation of the
        /// front cache in case of the service [automatic] restart.
        /// </remarks>
        protected virtual void RegisterBackServiceMemberEventHandler()
        {
            // automatic front cache clean up (upon service restart)
            ICacheService service = CacheService;
            if (service != null)
            {
                try
                {
                    service.MemberJoined  += new MemberEventHandler(OnMemberJoined);
                    service.MemberLeaving += new MemberEventHandler(OnMemberLeaving);
                    service.MemberLeft    += new MemberEventHandler(OnMemberLeft);
                }
                catch (NotSupportedException)
                {}
            }
        }

        /// <summary>
        /// Unregister back caches's service member event handler.
        /// </summary>
        protected virtual void UnregisterBackServiceMemberEventHandler()
        {
            try
            {
                CacheService.MemberJoined  -= new MemberEventHandler(OnMemberJoined);
                CacheService.MemberLeaving -= new MemberEventHandler(OnMemberLeaving);
                CacheService.MemberLeft    -= new MemberEventHandler(OnMemberLeft);
            }
            catch (Exception)
            {}
        }

        #endregion

        #region Data members

        /// <summary>
        /// The cache name.
        /// </summary>
        private readonly string m_name;

        /// <summary>
        /// The back cache service.
        /// </summary>
        private readonly ICacheService m_service;

        #endregion
    }
}