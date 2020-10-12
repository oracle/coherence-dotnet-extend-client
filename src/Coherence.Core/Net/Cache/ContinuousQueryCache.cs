/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

using Tangosol.Net.Cache.Support;
using Tangosol.Net.Internal;
using Tangosol.Net.Messaging;
using Tangosol.Util;
using Tangosol.Util.Collections;
using Tangosol.Util.Daemon;
using Tangosol.Util.Daemon.QueueProcessor;
using Tangosol.Util.Filter;
using Tangosol.Util.Processor;
using Tangosol.Util.Transformer;
using Queue = Tangosol.Util.Daemon.QueueProcessor.Queue;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// Create a materialized view of an <see cref="INamedCache"/> using the
    /// Coherence <i>Continuous Query</i> capability.
    /// </summary>
    /// <author>Cameron Purdy  2006.01.19</author>
    /// <author>Ana Cikic  2006.11.27</author>
    /// <author>Goran Milosavljevic  2006.11.28</author>
    /// <author>Ivan Cikic  2006.11.28</author>
    /// <author>Aleksandar Seovic  2012.01.13</author>
    /// <since>Coherence 3.1</since>
    public class ContinuousQueryCache : AbstractKeySetBasedCache, INamedCache
    {
        #region Properties

        /// <summary>
        /// Obtain the <see cref="INamedCache"/> that this
        /// <b>ContinuousQueryCache</b> is based on.
        /// </summary>
        /// <value>
        /// The underlying <b>INamedCache</b>.
        /// </value>
        public virtual INamedCache Cache
        {
            get
            {
                INamedCache cache = m_cache;
                if (cache == null)
                {
                    cache = m_cache = m_supplierCache();
                    if (cache == null)
                    {
                        throw new InvalidOperationException("INamedCache is not active");
                    }
                }

                 return cache;
            }
        }

        /// <summary>
        /// The index IDictionary used by this <b>ContinuousQueryCache</b>.
        /// </summary>
        /// <value>
        /// The <see cref="IDictionary"/> used by this <b>ContinuousQueryCache</b>,
        /// or <c>null</c> if none.
        /// </value>
        protected virtual IDictionary IndexMap
        {
            get
            {
                return m_indexMap;
            }
        }
        
        /// <summary>
        /// Obtain the <see cref="IFilter"/> that this
        /// <b>ContinuousQueryCache</b> is using to query the underlying
        /// <see cref="INamedCache"/>.
        /// </summary>
        /// <value>
        /// The <b>IFilter</b> that this cache uses to select its contents
        /// from the underlying <b>INamedCache</b>.
        /// </value>
        public virtual IFilter Filter
        {
            get { return m_filter; }
        }

        /// <summary>
        /// Obtain or modify the local-caching option for this
        /// <b>ContinuousQueryCache</b>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// By changing this value from <b>false</b> to <b>true</b>, the
        /// <b>ContinuousQueryCache</b> will fully realize its contents
        /// locally and maintain them coherently in a manner analogous to the
        /// Coherence Near Cache. By changing this value from <b>true</b> to
        /// <b>false</b>, the <b>ContinuousQueryCache</b> will discard its
        /// locally cached data and rely on the underlying
        /// <see cref="INamedCache"/>.</p>
        /// </remarks>
        /// <value>
        /// <b>true</b> if this object caches values locally, and false if it
        /// relies on the underlying <b>INamedCache</b>.
        /// </value>
        [Obsolete("As of Coherence 3.4 this property is replaced with CacheValues")]
        public virtual bool IsCacheValues
        {
            get { return CacheValues; }
            set { CacheValues = value; }
        }

        /// <summary>
        /// Obtain or modify the local-caching option for this
        /// <b>ContinuousQueryCache</b>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// By changing this value from <b>false</b> to <b>true</b>, the
        /// <b>ContinuousQueryCache</b> will fully realize its contents
        /// locally and maintain them coherently in a manner analogous to the
        /// Coherence Near Cache. By changing this value from <b>true</b> to
        /// <b>false</b>, the <b>ContinuousQueryCache</b> will discard its
        /// locally cached data and rely on the underlying
        /// <see cref="INamedCache"/>.</p>
        /// </remarks>
        /// <value>
        /// <b>true</b> if this object caches values locally, and false if it
        /// relies on the underlying <b>INamedCache</b>.
        /// </value>
        public virtual bool CacheValues
        {
            get
            {
                return m_cacheValues || IsObserved;
            }
            set
            {
                lock (SyncRoot)
                {
                    if (value != m_cacheValues)
                    {
                        bool didCacheValues = CacheValues;
                        // If we are no longer caching the values then we don't
                        // need the local indexes.
                        if (didCacheValues)
                        {
                            ReleaseIndexMap();
                        }

                        m_cacheValues = value;

                        if (CacheValues != didCacheValues)
                        {
                            ConfigureSynchronization(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Obtain or modify the read-only option for the
        /// <b>ContinuousQueryCache</b>.
        /// </summary>
        /// <remarks>
        /// Note that the cache can be made read-only, but the opposite
        /// (making it mutable) is explicitly disallowed.
        /// </remarks>
        /// <value>
        /// <b>true</b> if this <b>ContinuousQueryCache</b> has been
        /// configured as read-only.
        /// </value>
        public virtual new bool IsReadOnly
        {
            get
            {
                return m_isReadOnly;
            }
            set
            {
                lock (SyncRoot)
                {
                    if (value != m_isReadOnly)
                    {
                        // once the cache is read-only, changing its read-only 
                        // setting is a mutating operation and thus is dis-allowed
                        CheckReadOnly();
                        m_isReadOnly = value;
                    }
                }
            }
        }

        /// <summary>
        /// Obtain a reference to the internal cache.
        /// </summary>
        /// <remarks>
        /// The internal cache maintains all of the keys in the
        /// <b>ContinuousQueryCache</b>, and if <see cref="CacheValues"/> is
        /// <b>true</b>, it also maintains the up-to-date values
        /// corresponding to those keys.
        /// </remarks>
        /// <value>
        /// The internal cache that represents the materialized view of the
        /// <b>ContinuousQueryCache</b>.
        /// </value>
        protected virtual IObservableCache InternalCache
        {
            get
            {
                EnsureSynchronized(true);
                return m_cacheLocal;
            }
        }

        /// <summary>
        /// Determine or modify if the <b>ContinuousQueryCache</b> has any
        /// listeners that cannot be served by this cache listening to lite
        /// events.
        /// </summary>
        /// <value>
        /// <b>true</b> iff there is at least one listener.
        /// </value>
        public virtual bool IsObserved
        {
            get
            {
                return m_hasListeners;
            }
            set
            {
                lock (SyncRoot)
                {
                    if (value != m_hasListeners)
                    {
                        bool didCacheValues = CacheValues;
                        m_hasListeners = value;

                        if (CacheValues != didCacheValues)
                        {
                            ConfigureSynchronization(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The state of the ContinuousQueryCache.
        /// </summary>
        /// <value>
        /// One of the <see cref="CacheState"/> values.
        /// </value>
        public virtual CacheState State
        {
            get
            {
                return m_state;
            }
            set
            {
                switch (value)
                {
                    case CacheState.Disconnected:
                        m_cache = null;
                        m_state = CacheState.Disconnected;
                        break;

                    case CacheState.Configuring:
                        lock (SyncRoot)
                        {
                            CacheState statePrev = m_state;
                            Debug.Assert(statePrev == CacheState.Disconnected || statePrev == CacheState.Synchronized);

                            m_syncReq = new SynchronizedDictionary();
                            m_state   = CacheState.Configuring;
                        }
                        break;

                    case CacheState.Configured:
                        lock (SyncRoot)
                        {
                            if (m_state == CacheState.Configuring)
                            {
                                m_state = CacheState.Configured;
                            }
                            else
                            {
                                throw new InvalidOperationException(CacheName + " has been invalidated");
                            }
                        }
                        break;

                    case CacheState.Synchronized:
                        lock (SyncRoot)
                        {
                            if (m_state == CacheState.Configured)
                            {
                                m_syncReq = null;
                                m_state   = CacheState.Synchronized;
                            }
                            else
                            {
                                throw new InvalidOperationException(CacheName + " has been invalidated");
                            }
                        }
                    break;

                    default:
                        throw new InvalidOperationException("unknown state: " + value);
                }
            }
        }

        /// <summary>
        /// Return a reconnection interval (in milliseconds).
        /// </summary>
        /// <remarks>
        /// This value indicates the period
        /// in which re-synchronization with the underlying cache will be delayed in the case the
        /// connection is severed.  During this time period, local content can be accessed without
        /// triggering re-synchronization of the local content.
        /// </remarks>
        /// <value>
        /// A reconnection interval (in milliseconds). The value of zero
        /// means that the ContinuousQueryCache cannot be used when not
        /// connected.
        /// </value>
        /// <since>Coherence 3.4</since>
        public virtual long ReconnectInterval
        {
            get { return m_reconnectMillis; }
            set { m_reconnectMillis = value; }
        }

        /// <summary>
        /// Obtain the <b>ValueExtractor</b> that this <b>ContinuousQueryCache</b> is
        /// using to transform the results from the underlying cache prior to storing them locally.
        /// </summary>
        /// <value>
        /// The <b>ValueExtractor</b> that this <b>ContinuousQueryCache</b> is
        /// using to transform the results from the underlying cache prior to storing them locally.
        /// </value>
        /// <since>12.2.1.4</since>
        public virtual IValueExtractor Transformer
        {
            get { return m_transformer; }
        }

        /// <summary>
        /// Obtain the configured <b>CacheListener</b> for this <b>ContinuousQueryCache</b>
        /// </summary>
        /// <value>
        /// The <b>CacheListener</b> for this <b>ContinuousQueryCache</b>
        /// </value>
        /// <since>12.2.1.4</since>
        public virtual ICacheListener CacheListener
        {
            get { return m_cacheListener; }
        }

        /// <summary>
        /// Return the function providing the name of this <b>ContinuousQueryCache</b>.
        /// </summary>
        /// <value>
        /// A function used to provide the name of this <b>ContinuousQueryCache</b>'s.
        /// </value>
        /// <since>12.2.1.4</since>
        public virtual Func<String> CacheNameSupplier
        {
            get { return m_cacheNameSupplier; }
            set { m_cacheNameSupplier = value; }
        }

        #endregion
  
        #region Constructors

        /// <summary>
        /// Create a locally materialized view of an <b>INamedCache</b> using
        /// an <see cref="IFilter"/>.
        /// </summary>
        /// <remarks>
        /// A materialized view is an implementation of <i>Continuous Query</i>
        /// exposed through the standard INamedCache API. This constructor will
        /// result in a ContinuousQueryCache that caches both its keys and
        /// values locally.
        /// </remarks>
        /// <param name="cache">
        /// The <b>INamedCache</b> to create a view of.
        /// </param>
        /// <since>12.2.1.4</since>
        public ContinuousQueryCache(INamedCache cache)
            : this(() => cache, AlwaysFilter.Instance, true, null, null)
        {
        }

        /// <summary>
        /// Create a locally materialized view of an <b>INamedCache</b> using
        /// an <see cref="IFilter"/>.
        /// </summary>
        /// <remarks>
        /// A materialized view is an implementation of <i>Continuous Query</i>
        /// exposed through the standard INamedCache API. This constructor will
        /// result in a ContinuousQueryCache that caches both its keys and
        /// values locally.
        /// </remarks>
        /// <param name="supplierCache">
        /// The <b>Func</b> returning an <b>INamedCache</b> with which this ContinuousQueryCache
        /// will be created.
        /// 
        /// The <b>Func</b> must return a new instance each time it is invoked.
        /// </param>
        /// <since>12.2.1.4</since>
        public ContinuousQueryCache(Func<INamedCache> supplierCache)
            : this(supplierCache, AlwaysFilter.Instance, true, null, null)
        {
        }

        /// <summary>
        /// Create a locally materialized view of an <b>INamedCache</b> using
        /// an <see cref="IFilter"/>.
        /// </summary>
        /// <remarks>
        /// A materialized view is an implementation of <i>Continuous Query</i>
        /// exposed through the standard INamedCache API. This constructor will
        /// result in a ContinuousQueryCache that caches both its keys and
        /// values locally.
        /// </remarks>
        /// <param name="cache">
        /// The <b>INamedCache</b> to create a view of.
        /// </param>
        /// <param name="filter">
        /// The filter that defines the view.
        /// </param>
        public ContinuousQueryCache(INamedCache cache, IFilter filter)
                : this(() => cache, filter, true, null, null)
        {
        }

        /// <summary>
        /// Create a locally materialized view of an <b>INamedCache</b> using
        /// an <see cref="IFilter"/>.
        /// </summary>
        /// <remarks>
        /// A materialized view is an implementation of <i>Continuous Query</i>
        /// exposed through the standard INamedCache API. This constructor will
        /// result in a ContinuousQueryCache that caches both its keys and
        /// values locally.
        /// </remarks>
        /// <param name="supplierCache">
        /// The <b>Func</b> returning an <b>INamedCache</b> with which this ContinuousQueryCache
        /// will be created.
        /// 
        /// The <b>Func</b> must return a new instance each time it is invoked.
        /// </param>
        /// <param name="filter">
        /// The filter that defines the view.
        /// </param>
        /// <since>12.2.1.4</since>
        public ContinuousQueryCache(Func<INamedCache> supplierCache, IFilter filter)
                : this(supplierCache, filter, true, null, null)
        {
        }

        /// <summary>
        /// Create a locally materialized view of an <b>INamedCache</b> using
        /// an <see cref="IFilter"/> and a transformer.
        /// </summary>
        /// <remarks>
        /// A materialized view is an implementation of <i>Continuous Query</i>
        /// exposed through the standard INamedCache API. This constructor will
        /// result in a ContinuousQueryCache that caches both its keys and
        /// values locally.
        /// </remarks>
        /// <param name="cache">
        /// The <b>INamedCache</b> to create a view of.
        /// </param>
        /// <param name="filter">
        /// The filter that defines the view.
        /// </param>
        /// <param name="transformer">
        /// The transformer that should be used to convert values from the 
        /// underlying cache before storing them locally
        /// </param>
        public ContinuousQueryCache(INamedCache cache, IFilter filter, IValueExtractor transformer)
            : this(() => cache, filter, true, null, transformer)
        {
        }

        /// <summary>
        /// Create a materialized view of an <b>INamedCache</b> using an
        /// <b>IFilter</b>.
        /// </summary>
        /// <remarks>
        /// A materialized view is an implementation of <i>Continuous Query</i>
        /// exposed through the standard INamedCache API.
        /// </remarks>
        /// <param name="cache">
        /// The <b>INamedCache</b> to create a view of.
        /// </param>
        /// <param name="filter">
        /// The filter that defines the view.
        /// </param>
        /// <param name="isCacheValues">
        /// Pass <b>true</b> to cache both the keys and values of the
        /// materialized view locally, or <b>false</b> to only cache the keys.
        /// </param>
        public ContinuousQueryCache(INamedCache cache, IFilter filter, bool isCacheValues)
                : this(() => cache, filter, isCacheValues, null, null)
        {}

        /// <summary>
        /// Create a materialized view of an <b>INamedCache</b> using an
        /// <b>IFilter</b>.
        /// </summary>
        /// <remarks>
        /// A materialized view is an implementation of <i>Continuous Query</i>
        /// exposed through the standard INamedCache API. This constructor
        /// allows a client to receive all events, including those that result
        /// from the initial population of the ContinuousQueryCache. In other
        /// words, all contents of the ContinuousQueryCache will be delivered
        /// to the listener as a sequence of events, including those items that
        /// already exist in the underlying (unfiltered) cache. Note that this
        /// constructor will always result in both the keys and values being
        /// cached locally if a listener is passed.
        /// </remarks>
        /// <param name="cache">
        /// The <b>INamedCache</b> to create a view of.
        /// </param>
        /// <param name="filter">
        /// The <b>IFilter</b> that defines the view.
        /// </param>
        /// <param name="listener">
        /// An initial <see cref="ICacheListener"/> that will receive all the
        /// events from the ContinuousQueryCache, including those corresponding
        /// to its initial population.
        /// </param>
        public ContinuousQueryCache(INamedCache cache, IFilter filter, ICacheListener listener)
                : this(() => cache, filter, false, listener, null)
        {}

        /// <summary>
        /// Initialize the ContinuousQueryCache.
        /// </summary>
        /// <param name="supplierCache">
        /// The <b>Func</b> returning an <b>INamedCache</b> with which this ContinuousQueryCache
        /// will be created.
        /// 
        /// The <b>Func</b> must return a new instance each time it is invoked.
        /// </param>
        /// <param name="filter">
        /// The filter that defines the view.
        /// </param>
        /// <param name="cacheValues">
        /// Pass <b>true</b> to cache both the keys and values of the
        /// materialized view locally, or <b>false</b> to only cache the keys.
        /// </param>
        /// <param name="cacheListener">
        /// The optional <b>ICacheListener</b> that will receive all events
        /// starting from the initialization of the ContinuousQueryCache.
        /// </param>
        /// <param name="transformer">
        /// The transformer that should be used to convert values from the 
        /// underlying cache before storing them locally
        /// </param>
        public ContinuousQueryCache(Func<INamedCache> supplierCache, IFilter filter, bool cacheValues, 
            ICacheListener cacheListener, IValueExtractor transformer)
        {
            INamedCache cache = supplierCache();
            if (cache == null)
            {
                throw new ArgumentNullException("cache", "INamedCache must be specified");
            }

            if (filter == null)
            {
                throw new ArgumentNullException("filter", "Filter must be specified");
            }

            if (filter is LimitFilter)
            {
                // TODO: it would be nice to eventually be able to have a
                // cache of the "top ten" items, etc.
                throw new NotSupportedException("LimitFilter may not be used");
            }

            m_supplierCache = supplierCache;
            m_cache         = cache;
            m_filter        = filter;
            m_cacheValues   = cacheValues;
            m_transformer   = transformer;
            m_isReadOnly    = transformer != null;
            m_state         = CacheState.Disconnected;
            m_cacheListener = cacheListener;

            // by including information about the underlying cache, filter and 
            // transformer, the resulting cache name is convoluted but extremely
            // helpful for tasks such as debugging
            m_name = String.Format("ContinuousQueryCache{{Cache={0}, Filter={1}, Transformer={2}}}",
                                   cache.CacheName, 
                                   filter,
                                   transformer);

            EnsureInternalCache();
            EnsureSynchronized(false);
        }

        #endregion

        #region AbstractKeySetBasedCache override methods

        /// <summary>
        /// Obtain a collection of keys that are represented by this cache.
        /// </summary>
        /// <remarks>
        /// The AbstractKeySetBasedCache only utilizes the internal keys
        /// collection as a read-only resource.
        /// </remarks>
        /// <returns>
        /// An internal collection of keys that are contained by this cache.
        /// </returns>
        protected override ICollection GetInternalKeysCollection()
        {
            return InternalCache.Keys;
        }

        /// <summary>
        /// Returns the value for the specified key.
        /// </summary>
        /// <param name="key">
        /// Key whose value is returned.
        /// </param>
        /// <returns>
        /// Value from the cache for the specified key.
        /// </returns>
        protected override object Get(object key)
        {
			return CacheValues
				? InternalCache[key]
				: Contains(key)
					? Cache[key]
					: null;
        }

        /// <summary>
        /// Returns <b>true</b> if this cache contains a mapping for the
        /// specified key.
        /// </summary>
        /// <param name="key">
        /// Key whose mapping is searched for.
        /// </param>
        /// <returns>
        /// <b>true</b> if this cache contains a mapping for the specified
        /// key, <b>false</b> otherwise.
        /// </returns>
        public override bool Contains(object key)
        {
            return InternalCache.Contains(key);
        }

        /// <summary>
        /// Removes the mapping for this key from this cache if present.
        /// </summary>
        /// <remarks>
        /// This method exists to allow sub-types to optimize remove
        /// functionalitly for situations in which the original value is not
        /// required.
        /// </remarks>
        /// <param name="key">
        /// Key whose mapping is to be removed from the cache.
        /// </param>
        /// <returns>
        /// <b>true</b> if the cache changed as the result of this
        /// operation.
        /// </returns>
        protected override bool RemoveBlind(object key)
        {
            CheckReadOnly();
            if (Contains(key))
            {
                Cache.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a value indicating whether access to the <b>ICollection</b>
        /// is synchronized (thread safe).
        /// </summary>
        /// <value>
        /// Always <b>true</b> for this cache.
        /// </value>
        public override bool IsSynchronized
        {
            get { return true; }
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
            INamedCache cache = Cache;
            if (Contains(key) || !cache.Contains(key))
            {
                return cache.Invoke(key, agent);
            }
            else
            {
                throw new InvalidOperationException(CacheName + ": key=" + key +
                    " is outside the ContinuousQueryCache");
            }
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
            if (keys.Count == 0)
            {
                return new HashDictionary(0);
            }

            // verify that the non-existent keys are NOT present in the
            // underlying cache (assumption is most keys in the collection are
            // already in the ContinuousQueryCache)
            INamedCache cache   = Cache;
            HashSet     colView = new HashSet(GetInternalKeysCollection());
            foreach (object key in keys)
            {
                if (!colView.Contains(key) && cache.Contains(key))
                {
                    throw new InvalidOperationException(CacheName
                             + ": key=" + key + " is outside the ContinuousQueryCache");
                }
            }
            return cache.InvokeAll(keys, agent);
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
            return Cache.InvokeAll(MergeFilter(filter), agent);
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
            if (keys.Count == 0)
            {
                return new HashDictionary(0);
            }

            // verify that the non-existent keys are NOT present in the
            // underlying cache (assumption is most keys in the collection are
            // already in the ContinuousQueryCache)
            INamedCache cache   = Cache;
            HashSet     colView = new HashSet(GetInternalKeysCollection());
            foreach (object key in keys)
            {
                if (!colView.Contains(key) && cache.Contains(key))
                {
                    throw new InvalidOperationException(CacheName
                                                        + ": key=" + key
                                                        + " is outside the ContinuousQueryCache");
                }
            }
            return cache.Aggregate(keys, agent);
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
            return Cache.Aggregate(MergeFilter(filter), agent);
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
            return CacheValues
                    ? InvocableCacheHelper.Query(InternalCache, IndexMap, filter,
                            InvocableCacheHelper.QueryType.Keys, false, null)
                    : Cache.GetKeys(MergeFilter(filter));
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
            return CacheValues
                    ? InvocableCacheHelper.Query(InternalCache, IndexMap, filter,
                            InvocableCacheHelper.QueryType.Values, false, null)
                    : Cache.GetValues(MergeFilter(filter));
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
            return CacheValues
                    ? InvocableCacheHelper.Query(InternalCache, IndexMap, filter,
                            InvocableCacheHelper.QueryType.Values, true, comparer)
                    : Cache.GetValues(MergeFilter(filter), comparer);
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
            return CacheValues
                    ? (ICacheEntry[]) InvocableCacheHelper.Query(InternalCache, IndexMap, filter, 
                                            InvocableCacheHelper.QueryType.Entries, false, 
                                            null)
                    : Cache.GetEntries(MergeFilter(filter));
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
            return CacheValues
                    ? (ICacheEntry[]) InvocableCacheHelper.Query(InternalCache, IndexMap, filter,
                                            InvocableCacheHelper.QueryType.Entries, true,
                                            comparer)
                    : Cache.GetEntries(MergeFilter(filter), comparer);
        }

        /// <summary>
        /// Add an index to this IQueryCache.
        /// </summary>
        /// <remarks>
        /// This allows to correlate values stored in thisg
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
            lock (SyncRoot)
            {
                if (CacheValues)
                {
                    InvocableCacheHelper.AddIndex(extractor, isOrdered, comparer,
                            InternalCache, EnsureIndexMap());
                }
            }

            // addIndex is a no-op if many clients are trying to add the same one
            Cache.AddIndex(extractor, isOrdered, comparer);

            // TODO : since we never remove an index from the underlying cache 
            // we might get an exception here if we are adding for a second
            // time, even if the user had previously attempted to remove it.
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
            // remove the index locally if we are caching values but do not
            // attempt to remove it from the underlying cache ...
            // removeIndex would kill all the other clients' performance if every
            // client balanced their add and remove index calls, so this cache
            // ignores the suggestion (since it cannot know if it was the cache
            // that originally added the index)
            lock (SyncRoot)
            {
                if (CacheValues)
                {
                    InvocableCacheHelper.RemoveIndex(extractor, InternalCache, IndexMap);
                }
            }
        }

        /// <summary>
        /// Obtain the IDictionary of indexes maintained by this cache. 
        /// </summary>
        /// <returns>
        /// The IDictionary of indexes maintained by this cache.
        /// </returns>
        protected virtual IDictionary EnsureIndexMap()
        {
            lock (SyncRoot)
            {
                IDictionary indexMap = m_indexMap;
                if (indexMap == null)
                {
                    m_indexMap = indexMap = new SynchronizedDictionary();
                }
                return indexMap;
            }
        }
        
        /// <summary>
        /// Release the the entire index map.
        /// </summary>
        protected void ReleaseIndexMap()
        {
            IDictionary mapIndex = m_indexMap;
            if (mapIndex != null)
            {
                ICollection setExtractors = new ArrayList(mapIndex.Keys);
                foreach (IValueExtractor extractor in setExtractors)
                {
                    RemoveIndex(extractor);
                }
                m_indexMap = null;
            }
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
            // locking is counted as a mutating operation
            CheckReadOnly();
            return Cache.Lock(key, waitTimeMillis);
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
            // we intentially don't do the ReadOnly check as you must
            // hold the lock in order to release it
            return Cache.Unlock(key);
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

            if (listener is CacheTriggerListener)
            {
                throw new ArgumentException("ContinuousQueryCache does not support CacheListenerTriggers");
            }

            lock (SyncRoot)
            {
                // need to cache values locally to provide standard (not lite) 
                // events
                if (!isLite)
                {
                    IsObserved = true;
                }

                EnsureEventDispatcher();

                InternalCache.AddCacheListener(InstantiateEventRouter(listener, isLite), key, isLite);                
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

            lock (SyncRoot)
            {
                InternalCache.RemoveCacheListener(
                        InstantiateEventRouter(listener, false), key);
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

            if (listener is CacheTriggerListener)
            {
                throw new ArgumentException("ContinuousQueryCache does not support CacheListenerTriggers");
            }

            lock (SyncRoot)
            {
                // need to cache values locally to provide event filtering and
                // to provide standard (not lite) events
                if (filter != null || !isLite)
                {
                    IsObserved = true;
                }

                EnsureEventDispatcher();

                InternalCache.AddCacheListener(InstantiateEventRouter(listener, isLite), filter, isLite);
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

            lock (SyncRoot)
            {
                InternalCache.RemoveCacheListener(
                        InstantiateEventRouter(listener, false), filter);
            }
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
        /// A collection of keys that may be in the named cache.
        /// </param>
        /// <returns>
        /// A dictionary of keys to values for the specified keys passed in
        /// <paramref name="keys"/>.
        /// </returns>
        public override IDictionary GetAll(ICollection keys)
        {
            IDictionary result;
            ICache      cacheLocal = InternalCache;

            if (CacheValues)
            {
                result = new HashDictionary();
                foreach (object key in keys)
                {
                    object value = cacheLocal[key];
                    if (value != null || Contains(key))
                    {
                        result[key] = value;
                    }
                }
            }
            else if (keys.Count <= 1)
            {
                // optimization: the requested set is either empty or the caller
                // is doing a combined "containsKey() and get()"
                result = new HashDictionary();
                foreach (object key in keys)
                {
                    if (cacheLocal.Contains(key))
                    {
                        object value = Cache[key];
                        if ((value != null || cacheLocal.Contains(key)) && InvocableCacheHelper.EvaluateEntry(Filter, key, value))
                        {
                            result[key] = value;
                        }
                    }
                }
            }
            else
            {
                // since the values are not cached, delegate the processing to
                // the underlying NamedCache
                ICollection collView       = new HashSet(keys);
                ICollection cacheLocalKeys = new HashSet(cacheLocal.Keys); 
                CollectionUtils.RetainAll(collView, cacheLocalKeys);
                result = Cache.GetAll(collView);

                // verify that the returned contents should all be in this
                // cache
                IFilter filter = Filter;
                if (result.Count > 0 )
                {
                    IDictionary result2 = new HashDictionary();
                    foreach (DictionaryEntry entry in result)
                    {
                        if (InvocableCacheHelper.EvaluateEntry(filter, new CacheEntry(entry.Key, entry.Value)))
                        {
                            result2[entry.Key] = entry.Value;
                        }
                    }
                    result = result2;
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
        public override object Insert(object key, object value)
        {
            object orig;

            CheckReadOnly();
            CheckEntry(key, value);

            // see if the putAll() optimization will work; this requires the
            // return value to be locally cached, or knowledge that the orig
            // value is null (because it is not present in the
            // ContinuousQueryCache)
            INamedCache cache        = Cache;
            bool        isLocalCache = CacheValues;
            bool        isPresent    = Contains(key);
            if (isLocalCache || !isPresent)
            {
                orig = isPresent ? InternalCache[key] : null;
                cache.Insert(key, value);
            }
            else
            {
                orig = cache.Insert(key, value);
                if (!InvocableCacheHelper.EvaluateEntry(Filter, key, orig))
                {
                    orig = null;
                }
            }

            return orig;
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
        public override object Insert(object key, object value, long millis)
        {
            if (millis == CacheExpiration.DEFAULT)
            {
                return Insert(key, value);
            }
            else
            {
                CheckReadOnly();
                CheckEntry(key, value);

                object orig = Cache.Insert(key, value, millis);
                return InvocableCacheHelper.EvaluateEntry(Filter, key, orig)
                              ? orig
                              : null;
            }
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
        public override void InsertAll(IDictionary dictionary)
        {
            CheckReadOnly();
            foreach (DictionaryEntry entry in dictionary)
            {
                CheckEntry(new CacheEntry(entry.Key, entry.Value));
            }
            Cache.InsertAll(dictionary);
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
            get
            {
                Func<String> nameSupplier = m_cacheNameSupplier;
                return nameSupplier == null ? m_name : nameSupplier();
            }
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
            get { return Cache.CacheService; }
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
                INamedCache cache = m_cache;
                return cache != null && cache.IsActive;
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
        public virtual void Release()
        {
            // shut down the event queue
            ShutdownEventQueue();

            lock (SyncRoot)
            {
                ReleaseListeners();
             
                m_cacheLocal   = null;
                m_cache        = null;
                m_state        = CacheState.Disconnected;
               
            }
        }

        /// <summary>
        /// Release and destroy this instance of <see cref="INamedCache"/>.
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
            // destroys the view but not the underlying cache
            Release();
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
            CheckReadOnly();
            Cache.Truncate();
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

        #region IDictionary implementation

        /// <summary>
        /// Removes all mappings from this cache.
        /// </summary>
        /// <remarks>
        /// Some implementations will attempt to lock the entire cache
        /// (if necessary) before preceeding with the clear operation. For
        /// such implementations, the entire cache has to be either
        /// already locked or able to be locked for this operation to
        /// succeed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// If the lock could not be succesfully obtained for some key.
        /// </exception>
        public override void Clear()
        {
            CheckReadOnly();

            ArrayList keysToRemove = new ArrayList(GetInternalKeysCollection());

            foreach (object key in keysToRemove)
            {
                Cache.Remove(key);
            }
        }

        /// <summary>
        /// Removes the element with the specified key from the
        /// <b>IDictionary</b> object.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        public override void Remove(object key)
        {
            CheckReadOnly();
            if (Contains(key))
            {
                INamedCache cache = Cache;
                if (CacheValues)
                {
                    RemoveBlind(key);
                }
                else
                {
                    cache.Remove(key);
                }
            }
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

        #region Internal methods

        /// <summary>
        /// Return a filter which merges the <b>ContinousQueueCache</b>'s
        /// filter with the supplied filter.
        /// </summary>
        /// <param name="filter">
        /// The <see cref="IFilter"/> to merge with this cache's filter.
        /// </param>
        /// <returns>
        /// The merged filter.
        /// </returns>
        protected virtual IFilter MergeFilter(IFilter filter)
        {
            if (filter == null)
            {
                return m_filter;
            }

            IFilter filterMerged;

            // strip off key association
            IFilter filterCQC  = Filter;
            bool    isKeyAssoc = false;
            object  keyAssoc   = null;

            if (filterCQC is KeyAssociatedFilter)
            {
                KeyAssociatedFilter filterAssoc = (KeyAssociatedFilter) filterCQC;
                keyAssoc   = filterAssoc.HostKey;
                filterCQC  = filterAssoc.Filter;
                isKeyAssoc = true;

                // if the passed filter is also key-associated, strip it off too
                if (filter is KeyAssociatedFilter)
                {
                    filter = ((KeyAssociatedFilter) filter).Filter;
                }
            }
            else if (filter is KeyAssociatedFilter)
            {
                KeyAssociatedFilter filterAssoc = (KeyAssociatedFilter) filter;
                keyAssoc   = filterAssoc.HostKey;
                filter     = filterAssoc.Filter;
                isKeyAssoc = true;
            }

            if (filter is LimitFilter)
            {
                // To merge a LimitFilter with the CQC Filter we cannot
                // simply And the two, we must And the CQC Filter with the
                // LimitFilter's internal Filter, and then apply the limit
                // on top of that
                LimitFilter filterNew;
                LimitFilter filterOrig = (LimitFilter) filter;
                int         pageSize   = filterOrig.PageSize;
                object      cookie     = filterOrig.Cookie;

                if (cookie is LimitFilter)
                {
                    // apply the page size as it could have changed since the
                    // wrapper was created
                    filterNew = (LimitFilter) cookie;
                    filterNew.PageSize = pageSize;
                }
                else
                {
                    // cookie either didn't exist, or was not our cookie
                    // construct the wrapper and stick it in the cookie for
                    // future re-use
                    filterNew = new LimitFilter(
                            new AndFilter(filterCQC, filterOrig.Filter),
                            pageSize);
                    filterOrig.Cookie = filterNew;
                }

                // apply current page number;
                // all other properites are for use by the query processor
                // and only need to be maintained within the wrapper
                filterNew.Page = filterOrig.Page;
                filterMerged   = filterNew;
            }
            else
            {
                filterMerged = new AndFilter(filterCQC, filter);
            }

            // apply key association
            if (isKeyAssoc)
            {
                filterMerged = new KeyAssociatedFilter(filterMerged, keyAssoc);
            }

            return filterMerged;
        }


        /// <summary>
        /// Check the read-only setting to verify that the cache is NOT read-only.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// if the <b>ContinuousQueryCache</b> is read-only.
        /// </exception>
        protected void CheckReadOnly()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(CacheName + " is read-only");
            }
        }

        /// <summary>
        /// Check the passed value to verify that it does belong in this
        /// <b>ContinuousQueryCache</b>.
        /// </summary>
        /// <param name="entry">
        /// a key/value pair to check.
        /// </param>
        /// <exception cref="ArgumentException">
        /// if the entry does not belong in this <b>ContinuousQueryCache</b>
        /// (based on the cache's filter).
        /// </exception>
        protected void CheckEntry(ICacheEntry entry)
        {
            if (!InvocableCacheHelper.EvaluateEntry(Filter, entry))
            {
                throw new ArgumentException(CacheName
                           + ": Attempted modification violates filter; key=\""
                           + entry.Key + "\", value=\""
                           + entry.Value + "\"");
            }
        }

        /// <summary>
        /// Check the passed value to verify that it does belong in this
        /// <b>ContinuousQueryCache</b>.
        /// </summary>
        /// <param name="key">
        /// The key for the entry.
        /// </param>
        /// <param name="value">
        /// The value for the entry.
        /// </param>
        /// <exception cref="ArgumentException">
        /// if the entry does not belong in this <b>ContinuousQueryCache</b>
        /// (based on the cache's filter).
        /// </exception>
        protected void CheckEntry(object key, object value)
        {
            if (!InvocableCacheHelper.EvaluateEntry(Filter, key, value))
            {
                throw new ArgumentException(CacheName
                              + ": Attempted modification violates filter; key=\""
                              + key + "\", value=\"" + value + "\"");
            }
        }

        /// <summary>
        /// Instantiate the internal cache used by the ContinuousQueryCache.
        /// </summary>
        /// <returns>
        /// A new <b>IObservableMap</b> that will represent the materialized
        /// view of the <b>ContinuousQueryCache</b>.
        /// </returns>
        protected virtual IObservableCache InstantiateInternalCache()
        {
            return new LocalCache();
        }

        /// <summary>
        /// Instantiate an <see cref="ICacheListener"/> for evicting items
        /// from the query.
        /// </summary>
        /// <returns>
        /// A new <b>ICacheListener</b> that will listen to all events that
        /// will remove items from the <b>ContinuousQueryCache</b>.
        /// </returns>
        protected virtual ICacheListener InstantiateRemoveListener()
        {
            return new RemoveListener(this);
        }

        /// <summary>
        /// Instantiate an <see cref="ICacheListener"/> for adding items to
        /// the query, and (if there are listeners on the
        /// <b>ContinuousQueryCache</b>) for dispatching inserts and updates.
        /// </summary>
        /// <returns>
        /// A new <b>ICacheListener</b> that will add items to and update
        /// items in the <b>ContinuousQueryCache</b>.
        /// </returns>
        protected virtual ICacheListener InstantiateAddListener()
        {
            return new AddListener(this);
        }

        /// <summary>
        /// Instantiate a listener on the internal cache that will direct
        /// events to the passed listener, either synchronously or
        /// asynchronously as appropriate.
        /// </summary>
        /// <param name="listener">
        /// The listener to route to.
        /// </param>
        /// <param name="fLite">
        /// true to indicate the the event objects passed to the listener does
        /// not have to include old or new values in order to allow optimizations.
        /// </param>
        /// <returns>
        /// A new <b>EventRouter</b> specific to the passed listener.
        /// </returns>
        protected virtual EventRouter InstantiateEventRouter(ICacheListener listener, bool fLite)
        {
            return new EventRouter(this, listener, fLite);
        }

        /// <summary>
        /// Invoked when an <see cref="IMember"/> has joined the service.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Note: this event could be called during the service restart
        /// on the local node in which case the listener's code should
        /// not attempt to use any clustered cache or service
        /// functionality.
        /// </p>
        /// <p>
        /// The most critical situation arises when a number of threads
        /// are waiting for a local service restart, being blocked by a
        /// <b>IService</b> object synchronization monitor. Since the
        /// Joined event should be fired only once, it is called on a
        /// client thread <b>while holding a synchronization monitor</b>.
        /// An attempt to use other clustered service functionality
        /// during this local event notification may result in a
        /// deadlock.
        /// </p>
        /// </remarks>
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// An event which indicates that membership has changed.
        /// </param>
        public void OnMemberJoined(object sender, MemberEventArgs evt)
        {}

        /// <summary>
        /// Invoked when an <see cref="IMember"/> is leaving the service.
        /// </summary>
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// An event which indicates that membership has changed.
        /// </param>
        public void OnMemberLeaving(object sender, MemberEventArgs evt)
        {}

        /// <summary>
        /// Invoked when an <see cref="IMember"/> has left the service.
        /// </summary>
        /// <remarks>
        /// Note: this event could be called during the service restart
        /// on the local node in which case the listener's code should
        /// not attempt to use any clustered cache or service
        /// functionality.
        /// </remarks>
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// An event which indicates that membership has changed.
        /// </param>
        public void OnMemberLeft(object sender, MemberEventArgs evt)
        {
            State = CacheState.Disconnected;
        }

        /// <summary>
        /// Register a member event handler with the underlying caches's
        /// service.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// The primary goal of that event handler is invalidation of the
        /// front cache in case of the service [automatic] restart.
        /// </remarks>
        protected void RegisterServiceMemberEventHandler()
        {
            // automatic front map clean up (upon service restart)
            // requires a MemberListener implementation
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
        /// Unregister underlying caches's service member event handler.
        /// </summary>
        protected void UnregisterServiceMemberEventHandler()
        {
            ICacheService service = CacheService;
            try
            {
                service.MemberJoined  -= new MemberEventHandler(OnMemberJoined);
                service.MemberLeaving -= new MemberEventHandler(OnMemberLeaving);
                service.MemberLeft    -= new MemberEventHandler(OnMemberLeft);
            }
            catch (Exception)
            {}
        }

        /// <summary>
        /// Release all locally registered listeners.
        /// </summary>
        /// <since>12.2.1.4</since>
        protected void ReleaseListeners()
        {
            INamedCache cache = m_cache;
            if (cache != null)
            {
                UnregisterServiceMemberEventHandler();
                UnregisterDeactivationListener();

                ICacheListener listenerAdd = m_listenerAdd;
                if (listenerAdd != null)
                {
                    try
                    {
                        cache.RemoveCacheListener(listenerAdd, CreateTransformerFilter(m_filterAdd));
                    }
                    catch (Exception) { }
                    m_listenerAdd = null;
                }
                m_filterAdd = null;

                ICacheListener listenerRemove = m_listenerRemove;
                if (listenerRemove != null)
                {
                    try
                    {
                        cache.RemoveCacheListener(listenerRemove, m_filterRemove);
                    }
                    catch (Exception) { }
                    m_listenerRemove = null;
                }

                m_filterRemove = null;
            }
        }

        /// <summary>
        /// Instantiate and register a <b>INamedCacheDeactivationListener</b> to allow this
        /// cache to listen for cache deactivation or truncation events.
        /// </summary>
        /// <since>12.2.1.4</since>
        protected void RegisterDeactivationListener()
        {
            // automatic named cache clean up (upon cache destruction)
            // requires a DeactivationListener implementation
            ICacheService service = CacheService;
            if (service != null)
            {
                try
                {
                    INamedCacheDeactivationListener listener = m_listenerDeactivation = new DeactivationListener(this);
                    Cache.AddCacheListener(listener);
                } 
                catch (Exception)
                {
                }
            }
            
        }

        /// <summary>
        /// Unregister the previously registered <b>INamedCacheDeactivationListener</b>.
        /// </summary>
        /// <since>12.2.1.4</since>
        protected void UnregisterDeactivationListener()
        {
            INamedCacheDeactivationListener listenerDeactivation = m_listenerDeactivation;
            if (listenerDeactivation != null)
            {
                try
                {
                    INamedCache cache = Cache;
                    if (cache != null)
                    {
                        cache.RemoveCacheListener(listenerDeactivation);
                    }
                }
                catch (Exception)
                {
                }
            }
           
        }

        /// <summary>
        /// Set up the listeners that keep the <b>ContinuousQueryCache</b>
        /// up-to-date.
        /// </summary>
        /// <param name="reload">
        /// Pass <b>true</b> to force a data reload.
        /// </param>
        protected void ConfigureSynchronization(bool reload)
        {
            lock (SyncRoot)
            {
                IObservableCache cacheLocal = null;
                try
                {
                    State = CacheState.Configuring;
                    
                    Interlocked.Exchange(ref m_connectionTimestamp,
                            DateTimeUtils.GetSafeTimeMillis());

                    INamedCache cache   = Cache;
                    IFilter filter      = Filter;
                    bool    cacheValues = CacheValues;

                    // get the old filters and listeners
                    CacheEventFilter filterAddPrev   = m_filterAdd;
                    ICacheListener   listenerAddPrev = m_listenerAdd;

                    // determine if this is initial configuration
                    bool isFirstTime = filterAddPrev == null;
                    CacheEventFilter.CacheEventMask mask;

                    if (isFirstTime)
                    {
                        // register for service restart notification
                        RegisterServiceMemberEventHandler();
                        RegisterDeactivationListener();

                        // create the "remove listener"
                        mask = CacheEventFilter.CacheEventMask.UpdatedLeft | CacheEventFilter.CacheEventMask.Deleted;
                        CacheEventFilter filterRemove   = new CacheEventFilter(mask, filter);
                        ICacheListener   listenerRemove = InstantiateRemoveListener();
                        cache.AddCacheListener(listenerRemove, filterRemove, true);

                        m_filterRemove   = filterRemove;
                        m_listenerRemove = listenerRemove;
                    }
                    else
                    {
                        cache.AddCacheListener(m_listenerRemove, m_filterRemove, true);
                    }

                    // configure the "add listener"
                    mask = CacheEventFilter.CacheEventMask.Inserted | CacheEventFilter.CacheEventMask.UpdatedEntered;
                    if (cacheValues)
                    {
                        mask |= CacheEventFilter.CacheEventMask.UpdatedWithin;
                    }

                    if (isFirstTime || mask != filterAddPrev.EventMask)
                    {
                        CacheEventFilter filterAdd   = new CacheEventFilter(mask, filter);
                        ICacheListener   listenerAdd = InstantiateAddListener();
                        cache.AddCacheListener(listenerAdd, CreateTransformerFilter(filterAdd), !cacheValues);

                        m_filterAdd   = filterAdd;
                        m_listenerAdd = listenerAdd;

                        if (listenerAddPrev != null)
                        {
                            //Debug.Assert(filterAddPrev != null);
                            cache.RemoveCacheListener(listenerAddPrev, CreateTransformerFilter(filterAddPrev));
                        }
                    }
                    else
                    {
                        cache.AddCacheListener(listenerAddPrev, CreateTransformerFilter(filterAddPrev), !cacheValues);
                    }

                    // update the local query image
                    cacheLocal = EnsureInternalCache();
                    if (isFirstTime || reload)
                    {
                        // populate the internal cache
                        if (cacheValues)
                        {
                            IList entries;
                            if (m_transformer == null)
                            {
                                entries = cache.GetEntries(filter);
                            }
                            else
                            {
                                IDictionary results = cache.InvokeAll(filter, new ExtractorProcessor(m_transformer));
                                entries             = new ArrayList(results.Count);
                                foreach (DictionaryEntry entry in results)
                                {
                                    entries.Add(new CacheEntry(entry.Key, entry.Value));
                                }
                            }

                            // first remove anything that is not in the
                            // resulting query
                            if (cacheLocal.Count != 0)
                            {
                                HashSet queryKeys = new HashSet();
                                foreach (ICacheEntry entry in entries)
                                {
                                    queryKeys.Add(entry.Key);
                                }

                                CollectionUtils.RetainAll(cacheLocal.Keys, queryKeys);
                            }

                            // next, populate the local cache
                            foreach (ICacheEntry entry in entries)
                            {
                                cacheLocal.Insert(entry.Key, entry.Value);
                            }
                        }
                        else
                        {
                            // first remove the keys that are not in the
                            // resulting query
                            ICollection queryKeys = cache.GetKeys(filter);
                            if (cacheLocal.Count != 0)
                            {
                                CollectionUtils.RetainAll(cacheLocal.Keys,
                                        new HashSet(queryKeys));
                            }

                            // next, populate the local cache with the keys from
                            // the query
                            foreach (object o in queryKeys)
                            {
                                cacheLocal.Insert(o, null);
                            }
                        }
                    }
                    else
                    {
                        // not the first time; internal cache is already populated
                        if (cacheValues)
                        {
                            // used to cache only keys, now caching values too
                            object[] keys;
                            lock (cacheLocal.SyncRoot) // COHNET-160
                            {
                                keys = CollectionUtils.ToArray(cacheLocal.Keys);
                            }
                            IDictionary values = cache.GetAll(keys);
                            cacheLocal.InsertAll(values);
                        }
                        else
                        {
                            // used to cache values, now caching only keys
                            foreach (ICacheEntry entry in cacheLocal.Entries)
                            {
                                entry.Value = null;
                            }
                        }
                    }
                    CacheState currentState = State;
                    if (currentState != CacheState.Configuring)
                    {
                        // This is possible if the service thread has set the state
                        // to STATE_DISCONNECTED. In this case, throw and let the caller
                        // handle retry logic
                        throw createUnexpectedStateException(currentState, CacheState.Configuring);
                    }
                    State = CacheState.Configured;

                    // resolve all changes that occurred while configuration was going on
                    IDictionary cacheSyncReq = m_syncReq;
                    if (cacheSyncReq.Count != 0)
                    {
                        object[] keys;
                        lock (cacheSyncReq.SyncRoot) // COHNET-160
                        {
                            keys = CollectionUtils.ToArray(cacheSyncReq.Keys);
                        }
                        IDictionary cacheSyncValues = cache.GetAll(keys);
                        lock (cacheSyncReq.SyncRoot)
                        {
                            foreach (object key in cacheSyncReq.Keys)
                            {
                                object value     = cacheSyncValues[key];
                                bool   isPresent = value != null ||
                                         cacheSyncValues.Contains(key);

                                // COH-3847 - an update event was received and 
                                // deferred while configuring the CQC, but we need to
                                // double-check that the new value satisfies the 
                                // filter
                                if (isPresent && InvocableCacheHelper.EvaluateEntry(
                                            filter, key, value))
                                {
                                    cacheLocal.Insert(key, value);
                                }
                                else
                                {
                                    cacheLocal.Remove(key);
                                }
                            }
                            // notify other threads that there is nothing to resolve
                            cacheSyncReq.Clear();
                        }
                    }

                    currentState = State;
                    if (currentState != CacheState.Configured)
                    {
                        // This is possible if the service thread has set the state
                        // to STATE_DISCONNECTED. In this case, throw and let the caller
                        // handle retry logic
                        throw createUnexpectedStateException(currentState, CacheState.Configured);
                    }
                    State = CacheState.Synchronized;
                }
                catch (Exception)
                {
                    if (cacheLocal != null)
                    {
                        // exception during initial load (COH-2625) or reconciliation;
                        // in either case we need to unregister listeners and
                        // start from scratch
                        ReleaseListeners();
                    }
                    
                    // mark as disconnected
                    State = CacheState.Disconnected;
                    throw;
                }
            }
        }

        /// <summary>
        /// Simple helper to create an exception for communicating invalid state transitions.
        /// </summary>
        /// <param name="expectedState">
        /// expected state
        /// </param>
        /// <param name="actualState">
        /// actual state
        /// </param>
        /// <returns>
        /// a new <b>SystemException</b> with a description of the invalid state transition
        /// </returns>
        /// <since>
        /// </since>
        protected SystemException createUnexpectedStateException(CacheState expectedState, CacheState actualState)
        {
            String sMsg = "Unexpected synchronization state.  Expected: " + expectedState + ", actual: " + actualState;
            return new SystemException(sMsg);
        }

        /// <summary>
        /// Wrap specified CacheEventFilter with a CacheEventTransformerFilter that
        /// will either transform cache value using transformer defined for this
        /// ContinuousQueryCache, or remove the old value from the event using
        /// SemiLiteEventTransformer, if no transformer is defined for this CQC.
        /// </summary>
        /// <param name="filterAdd">
        /// Add filter to wrap.
        /// </param>
        /// <returns>
        /// CacheEventTransformerFilter that wraps specified add filter
        /// </returns>
        protected IFilter CreateTransformerFilter(CacheEventFilter filterAdd)
        {
            return new CacheEventTransformerFilter(filterAdd,
                    m_transformer == null
                        ? (ICacheEventTransformer) SemiLiteEventTransformer.Instance
                        : new ExtractorEventTransformer(null, m_transformer));
        }

        /// <summary>
        /// Ensure that the ContinousQueryCache listeners have been
        /// registered and its content synchronized with the underlying
        /// INamedCache.
        /// </summary>
        /// <param name="reload">
        /// The value to pass to the <see cref="ConfigureSynchronization"/>
        /// method if the ContinousQueryCache needs to be configured and
        /// synchronized.
        /// </param>
        protected void EnsureSynchronized(bool reload)
        {
            // configure and synchronize the ContinousQueryCache, if necessary
            if (State != CacheState.Synchronized)
            {
                long reconnectMillis     = ReconnectInterval;
                bool isDisconnectAllowed = reconnectMillis > 0;

                if (isDisconnectAllowed
                        && DateTimeUtils.GetSafeTimeMillis() < 
                        Interlocked.Read(ref m_connectionTimestamp) + reconnectMillis)
                {
                    // don't try to re-connect just yet
                    return;
                }

                Exception eConfig  = null;
                int       attempts = isDisconnectAllowed ? 1 : 3;
                for (int i = 0; i < attempts; ++i)
                {
                    lock (SyncRoot)
                    {
                        CacheState state = State;
                        if (state == CacheState.Disconnected)
                        {
                            try
                            {
                                ConfigureSynchronization(reload);
                                return;
                            }
                            catch (Exception e)
                            {
                                eConfig = e;
                            }
                        }
                        else
                        {
                            //Debug.Assert(state == CacheState.Synchronized);
                            return;
                        }
                    }
                }

                if (!isDisconnectAllowed)
                {
                    String sMsg = "This ContinuousQueryCache is disconnected. Retry the operation again.";
                    if (CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Max))
                    {
                        throw new InvalidOperationException(sMsg, eConfig);
                    }
                    throw new InvalidOperationException(sMsg);
                }
            }
        }

        /// <summary>
        /// Called when an event has occurred.
        /// </summary>
        /// <remarks>
        /// Allows the key to be logged as requiring deferred
        /// synchronization if the event occurs during the configuration or
        /// population of the <b>ContinuousQueryCache</b>.
        /// </remarks>
        /// <param name="key">
        /// The key that the event is related to.
        /// </param>
        /// <returns>
        /// <b>true</b> if the event processing has been deferred.
        /// </returns>
        protected bool IsEventDeferred(object key)
        {
            bool isDeferred = false;

            IDictionary cacheSyncReq = m_syncReq;
            if (cacheSyncReq != null)
            {
                if (State <= CacheState.Configuring)
                {
                    // since the listeners are being configured and the local
                    // cache is being populated, assume that the event is
                    // being processed out-of-order and requires a subsequent
                    // synchronization of the corresponding value
                    cacheSyncReq[key] = null;
                    isDeferred = true;
                }
                else
                {
                    // since an event has arrived after the configuration
                    // completed, the event automatically resolves the sync
                    // requirement
                    CollectionUtils.Remove(cacheSyncReq.Keys, key);
                }
            }

            return isDeferred;
        }

        /// <summary>
        /// Create and initialize this <code>ContinuousQueryCache</code>'s (if not already present) internal cache.
        /// This method is called by <code>ConfigureSynchronization(boolean)</code>, as such, it shouldn't be called
        /// directly.  Use <code>GetInternalCache</code>.
        /// </summary>
        /// <returns>
        /// The <b>IObservableCache</b> functioning as this <code>ContinuousQueryCache</code>'s internal cache.
        /// </returns>
        protected IObservableCache EnsureInternalCache()
        {
            if (m_cacheLocal == null)
            {
                IObservableCache cacheLocal    = m_cacheLocal = InstantiateInternalCache();
                ICacheListener   cacheListener = m_cacheListener; 
                if (cacheListener != null)
                {
                    // the initial listener has to hear the initial events
                    EnsureEventDispatcher();
                    cacheLocal.AddCacheListener(InstantiateEventRouter(cacheListener, false));
                    m_hasListeners = true;
                }
            }
            return m_cacheLocal;
        }

        #endregion

        #region Inner class: AddListener

        /// <summary>
        /// An <see cref="ICacheListener"/> for evicting items from the query.
        /// </summary>
        internal class AddListener : MultiplexingCacheListener, CacheListenerSupport.ISynchronousListener
        {
            #region Constructors

            /// <summary>
            /// Create AddListener object.
            /// </summary>
            /// <param name="parentQueryCache">
            /// Parent cache.
            /// </param>
            public AddListener(ContinuousQueryCache parentQueryCache)
            {
                m_parentQueryCache = parentQueryCache;
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Produce a human-readable description of this object.
            /// </summary>
            /// <returns>
            /// a String describing this object
            /// </returns>
            public override string ToString()
            {
                return "AddListener[" + m_parentQueryCache + "]";
            }

            #endregion

            #region MultiplexingCacheListener override methods

            /// <summary>
            /// Invoked when a cache entry has been inserted, updated or
            /// deleted.
            /// </summary>
            /// <remarks>
            ///  To determine what action has occurred, use
            /// <see cref="CacheEventArgs.EventType"/> property.
            /// </remarks>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/> carrying the insert, update or
            /// delete information.
            /// </param>
            protected override void OnCacheEvent(CacheEventArgs evt)
            {
                ContinuousQueryCache cqc = m_parentQueryCache;
                object               key = evt.Key;
                if (!cqc.IsEventDeferred(key))
                {
                    // guard against possible NRE; one could theoretically occur
                    // during construction or after release; one occured during
                    // testing of a deadlock issue (COHNET-161)
                    ICache cache = cqc.m_cacheLocal;
                    if (cache != null)
                    {
                        cache.Insert(key, cqc.CacheValues ?
                                evt.NewValue : null);
                    }
                }
            }

            #endregion

            #region Data members

            private ContinuousQueryCache m_parentQueryCache;

            #endregion
        }

        #endregion

        #region Inner class: EventRouter

        /// <summary>
        /// An <b>EventRouter</b> routes events from the internal cache of
        /// the <see cref="ContinuousQueryCache"/> to the client listeners,
        /// and it can do so asynchronously when appropriate.
        /// </summary>
        protected class EventRouter : MultiplexingCacheListener
        {
            #region Constructors

            /// <summary>
            /// Construct an <b>EventRouter</b> to route events from the
            /// internal cache of the <see cref="ContinuousQueryCache"/> to
            /// the client listeners.
            /// </summary>
            /// <param name="parentQueryCache">
            /// Parent cache instance.
            /// </param>
            /// <param name="listener">
            /// A client listener.
            /// </param>
            /// <param name="fLite">
            /// true to indicate the the event objects passed to the listener does
            /// not have to include old or new values in order to allow optimizations.
            /// </param>
            public EventRouter(ContinuousQueryCache parentQueryCache, ICacheListener listener, bool fLite)
            {
                m_listener         = listener;
                m_parentQueryCache = parentQueryCache;
                m_fLite            = fLite;
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Determine a hash value for the EventRouter object according to the
            /// general <b>Object.GetHashCode()</b> contract.
            /// </summary>
            /// <returns>
            /// an integer hash value for this EventRouter
            /// </returns>
            public override int GetHashCode()
            {
                return m_listener.GetHashCode();
            }

            /// <summary>
            /// Compare the <b>EventRouter</b> with another object to
            /// determine equality.
            /// </summary>
            /// <param name="o">
            /// The object to compare to.
            /// </param>
            /// <returns>
            /// <b>true</b> if this <b>EventRouter</b> and the passed object are
            /// equivalent listeners.
            /// </returns>
            public override bool Equals(object o)
            {
                return o is EventRouter
                       && m_listener.Equals(((EventRouter)o).m_listener);
            }

            /// <summary>
            /// Produce a human-readable description of this
            /// <b>EventRouter</b>.
            /// </summary>
            /// <returns>
            /// A String describing this <b>EventRouter</b>.
            /// </returns>
            public override string ToString()
            {
                return "EventRouter[" + m_listener + "]";
            }

            #endregion

            #region Data members

            private ICacheListener m_listener;
            private ContinuousQueryCache m_parentQueryCache;
            
            /// <summary>
            /// Flag indicating <b>MapEvent</b> objects do not have to include the OldValue and NewValue
            /// property values in order to allow optimizations.
            /// </summary>
            /// <since>12.2.1.4</since>
            private bool m_fLite;

            #endregion

            #region MultiplexingCacheListner override methods

            /// <summary>
            /// Invoked when a cache entry has been inserted, updated or
            /// deleted.
            /// </summary>
            /// <remarks>
            ///  To determine what action has occurred, use
            /// <see cref="CacheEventArgs.EventType"/> property.
            /// </remarks>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/> carrying the insert, update or
            /// delete information.
            /// </param>
            protected override void OnCacheEvent(CacheEventArgs evt)
            {
                ICacheListener listener = m_listener;
                CacheEventArgs evtRoute = CreateLocalCacheEvent(evt);

                if (evt is FilterEventArgs)
                {
                    evtRoute = new FilterEventArgs(evtRoute, ((FilterEventArgs)evt).Filters);
                }

                if (listener is CacheListenerSupport.ISynchronousListener)
                {
                    try
                    {
                        CacheListenerSupport.Dispatch(evtRoute, listener);
                    }
                    catch (Exception e)
                    {
                        CacheFactory.Log(e, CacheFactory.LogLevel.Error);
                    }
                }
                else
                {
                    EventDispatcher dispatcher = m_parentQueryCache.Dispatcher;

                    if (dispatcher != null)
                    {
                        DispatcherCacheEvent dispatcherCacheEvent = new DispatcherCacheEvent(evtRoute, listener);
                        dispatcher.Queue.Add(dispatcherCacheEvent);
                    }
                }
            }

            #endregion

            #region helper methods

            /// <summary>
            /// Construct a new local <b>CacheEventArgs</b> to dispatch to local listeners.
            /// </summary>
            /// <param name="source">
            /// the source event from the back cache
            /// </param>
            /// <returns>
            /// the new local <b>CacheEventArgs</b>
            /// </returns>
            protected CacheEventArgs CreateLocalCacheEvent(CacheEventArgs source)
            {
                bool lite = m_fLite;
                return new CacheEventArgs(m_parentQueryCache,
                                          source.EventType,
                                          source.Key,
                                          lite ? null : source.OldValue,
                                          lite ? null : source.NewValue,
                                          source.IsSynthetic);
            }

            #endregion

            #region Inner class: DispatcherCacheEvent

            /// <summary>
            /// Wraps <see cref="CacheEventArgs"/> and
            /// <see cref="ICacheListener"/>, so the
            /// <see cref="QueueProcessor"/> can <b>Run</b> event
            /// dispatching to the listener.
            /// </summary>
            internal class DispatcherCacheEvent : IRunnable
            {
                #region Constructors

                /// <summary>
                /// Constructs <b>DispatcherCacheEvent</b> with the specific
                /// cache event and listener.
                /// </summary>
                /// <param name="cacheEvent">
                /// The <b>CacheEventArgs</b> to dispatch.
                /// </param>
                /// <param name="listener">
                /// The listener.
                /// </param>
                public DispatcherCacheEvent(CacheEventArgs cacheEvent, ICacheListener listener)
                {
                    m_event    = cacheEvent;
                    m_listener = listener;
                }

                #endregion

                #region IRunnable implementation

                /// <summary>
                /// The method that will be called by the queue processor.
                /// </summary>
                public void Run()
                {
                    Dispatch(m_event, m_listener);
                }

                #endregion

                #region Internal methods

                /// <summary>
                /// Dispatch the <see cref="CacheEventArgs"/> to the specified
                /// <see cref="ICacheListener"/>.
                /// </summary>
                /// <param name="evt">
                /// The <b>CacheEventArgs</b>.
                /// </param>
                /// <param name="listener">
                /// The listener.
                /// </param>
                public virtual void Dispatch(CacheEventArgs evt, ICacheListener listener)
                {
                    CacheListenerSupport.Dispatch(evt, listener);
                }

                #endregion

                #region Data members

                private CacheEventArgs m_event;
                private ICacheListener m_listener;

                #endregion
            }

            #endregion
        }

        #endregion

        #region Inner class: RemoveListener

        /// <summary>
        /// An <see cref="ICacheListener"/> for evicting items from the query.
        /// </summary>
        internal class RemoveListener : MultiplexingCacheListener, CacheListenerSupport.ISynchronousListener
        {
            #region Constructors

            /// <summary>
            /// Construct an <b>RemoveListener</b> .
            /// </summary>
            /// <param name="parentQueryCache">
            /// Parent cache instance.
            /// </param>
            public RemoveListener(ContinuousQueryCache parentQueryCache)
            {
                m_parentQueryCache = parentQueryCache;
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Produce a human-readable description of this object.
            /// </summary>
            /// <returns>
            /// A String describing this object.
            /// </returns>
            public override string ToString()
            {
                return "RemoveListener[" + m_parentQueryCache + "]";
            }

            #endregion

            #region MultiplexingCacheListener override methods

            /// <summary>
            /// Invoked when a cache entry has been inserted, updated or
            /// deleted.
            /// </summary>
            /// <remarks>
            ///  To determine what action has occurred, use
            /// <see cref="CacheEventArgs.EventType"/> property.
            /// </remarks>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/> carrying the insert, update or
            /// delete information.
            /// </param>
            protected override void OnCacheEvent(CacheEventArgs evt)
            {
                ContinuousQueryCache cqc = m_parentQueryCache;
                object               key = evt.Key;
                if (!cqc.IsEventDeferred(key))
                {
                    // guard against possible NRE; one could theoretically occur
                    // during construction or after release; one occured during
                    // testing of a deadlock issue (COHNET-161)
                    ICache cache = cqc.m_cacheLocal;
                    if (cache != null)
                    {
                        cache.Remove(key);
                    }
                }
            }

            #endregion

            #region Data members

            private ContinuousQueryCache m_parentQueryCache;

            #endregion
        }

        #endregion

        #region Inner class: DeactivationListener

        /// <summary>
        /// DeactivationListener for the underlying NamedCache.
        /// </summary>
        /// <remarks>
        /// The primary goal of that listener is invalidation of the named cache when
        /// the named cache is destroyed or to truncate the local cache if the back cache has been truncated.
        /// </remarks>
        /// <since>12.2.1.4</since>
        protected class DeactivationListener : AbstractCacheListener, INamedCacheDeactivationListener
        {
            #region Constructors

            /// <summary>
            /// Constructs the <b>DeactivationListener</b> with a reference to this 
            /// <b>ContinuousQueryCache</b>
            /// </summary>
            /// <param name="parentContinuousQueryCache">
            /// The <b>ContinuousQueryCache</b> associated with this listener.
            /// </param>
            public DeactivationListener(ContinuousQueryCache parentContinuousQueryCache)
            {
                m_parentQueryCache = parentContinuousQueryCache;
            }
            #endregion

            #region AbstractCacheListener override methods

            /// <summary>
            /// Notify this cache of the underlying cache's deactivation state.
            /// </summary>
            /// <param name="evt">
            /// The deactivation event.
            /// </param>
            public override void EntryDeleted(CacheEventArgs evt)
            {
                // destroy/disconnect event
                ContinuousQueryCache queryCache = m_parentQueryCache;
                queryCache.State = CacheState.Disconnected;
            }

            /// <summary>
            /// Notify this cache of the underlying cache's truncation state.
            /// </summary>
            /// <param name="evt">
            /// The truncation event.
            /// </param>
            public override void EntryUpdated(CacheEventArgs evt)
            {
                // "truncate" event
                ContinuousQueryCache queryCache    = m_parentQueryCache;
                IObservableCache     internalCache = queryCache.InternalCache;
                if (internalCache is LocalCache)
                {
                    ((LocalCache) internalCache).Truncate();
                }
                else
                {
                    internalCache.Clear();
                }
            }

            #endregion

            #region Data members

            private ContinuousQueryCache m_parentQueryCache;

            #endregion

        }

        #endregion

        #region Data members

        /// <summary>
        /// The provided function resolves the named cache used to back this ContinuousQueryCache. 
        /// This function must return a new instance every time the function is invoked.
        /// </summary>
        /// <since>12.2.1.4</since>
        private Func<INamedCache> m_supplierCache;

        /// <summary>
        /// The underlying <see cref="INamedCache"/> object.
        /// </summary>
        private volatile INamedCache m_cache;

        /// <summary>
        /// The IDictionary of indexes maintaned by this cache. The keys are
        /// IValueExtractor objects, and for each key, the corresponding value
        /// stored in the IDictionary is a MapIndex object.
        /// </summary>
        private IDictionary m_indexMap;
        
        /// <summary>
        /// The name of the underlying <see cref="INamedCache"/>.
        /// </summary>
        /// <remarks>
        /// A copy is kept here because the reference to the underlying
        /// <b>INamedCache</b> is discarded when this cache is released.
        /// </remarks>
        private string m_name;

        /// <summary>
        /// The filter that represents the subset of information from the
        /// underlying <see cref="INamedCache"/> that this
        /// <b>ContinuousQueryCache</b> represents.
        /// </summary>
        private IFilter m_filter;

        /// <summary>
        /// The option of whether or not to locally cache values.
        /// </summary>
        private bool m_cacheValues;

        /// <summary>
        /// The option to disallow modifications through this
        /// <b>ContinuousQueryCache</b> interface.
        /// </summary>
        private bool m_isReadOnly;

        /// <summary>
        /// The transformer that should be used to convert values from the
        /// underlying cache.
        /// </summary>
        private IValueExtractor m_transformer;

        /// <summary>
        /// The interval (in millisceonds) that indicates how often the
        /// ContinuousQueryCache should attempt to synchronize its content
        /// with the underlying cache in case the connection is severed.
        /// </summary>
        private long m_reconnectMillis;

        /// <summary>
        /// The timestamp when the synchronization was last attempted.
        /// </summary>
        protected long m_connectionTimestamp;

        /// <summary>
        ///  The keys that are in this <b>ContinuousQueryCache</b>, and (if
        /// <see cref="m_cacheValues"/> is true) the corresponding values as
        /// well.
        /// </summary>
        private IObservableCache m_cacheLocal;

        /// <summary>
        /// While the <b>ContinuousQueryCache</b> is configuring or
        /// re-configuring its listeners and content, any events that are
        /// received must be logged to ensure that the corresponding content
        /// is in sync.
        /// </summary>
        private volatile IDictionary m_syncReq;

        /// <summary>
        /// The event queue for this <b>ContinuousQueryCache</b>.
        /// </summary>
        private volatile EventDispatcher m_eventDispatcher;

        /// <summary>
        /// Keeps track of whether the <b>ContinuousQueryCache</b> has
        /// listeners that require this cache to cache values.
        /// </summary>
        private bool m_hasListeners;

        /// <summary>
        /// The <see cref="CacheEventFilter"/> that uses the
        /// <b>ContinuousQueryCache</b>'s filter to select events that would
        /// add elements to this cache's contents.
        /// </summary>
        private CacheEventFilter m_filterAdd;

        /// <summary>
        /// The <see cref="CacheEventFilter"/> that uses the
        /// <b>ContinuousQueryCache</b>'s filter to select events that would
        /// remove elements from this cache's contents.
        /// </summary>
        private CacheEventFilter m_filterRemove;

        /// <summary>
        /// The listener that gets information about what should be in this
        /// cache.
        /// </summary>
        private ICacheListener m_listenerAdd;

        /// <summary>
        /// The listener that gets information about what should be thrown
        /// out of this cache.
        /// </summary>
        private ICacheListener m_listenerRemove;

        /// <summary>
        /// State of the ContinousQueryCache.
        /// </summary>
        private volatile CacheState m_state;

        /// <summary>
        /// The NamedCache deactivation listener; used to deal with destroy/release/truncate events from
        /// the underlying cache.
        /// </summary>
        /// <since>12.2.1.4</since>
        private volatile INamedCacheDeactivationListener m_listenerDeactivation;

        /// <summary>
        /// The optional function to resolve the name of this cache.
        /// </summary>
        /// <since>12.2.1.4</since>
        private Func<String> m_cacheNameSupplier;

        /// <summary>
        /// The optional <b>CacheListener</b> that may be provided during <b>ContinuousQueryCache</b>
        /// construction.
        /// </summary>
        /// <since>12.2.1.4</since>
        private ICacheListener m_cacheListener;

        #endregion

        #region EventDispatcher helper methods

        /// <summary>
        /// Create a self-processing event queue dispatcher.
        /// </summary>
        /// <returns>
        /// A EventDispatcher onto which events can be placed in order to be
        /// dispatched asynchronously.
        /// </returns>
        protected virtual EventDispatcher InstantiateEventDispatcher()
        {
            return new EventDispatcher();
        }

        /// <summary>
        /// Obtain this ContinuousQueryCache's event dispatcher.
        /// </summary>
        /// <value>
        /// The event dispatcher that this ContinuousQueryCache uses to
        /// dispatch its events to its non-synchronous listeners.
        /// </value>
        protected virtual EventDispatcher Dispatcher
        {
            get
            {
                return m_eventDispatcher;
            }
        }

        /// <summary>
        /// Obtain the existing event queue or create one if none exists.
        /// </summary>
        /// <returns>
        /// The event dispatcher that this ContinuousQueryCache uses to
        /// dispatch its events to its non-synchronous listeners.
        /// </returns>
        protected virtual EventDispatcher EnsureEventDispatcher()
        {
            lock (SyncRoot)
            {
                EventDispatcher dispatcher = m_eventDispatcher;
                if (dispatcher == null)
                {
                    m_eventDispatcher = dispatcher = InstantiateEventDispatcher();
                    dispatcher.ContinuousQueryCache = this;
                }
                if (!dispatcher.IsStarted)
                {
                    dispatcher.Start();
                }
                return dispatcher;
            }
        }

        /// <summary>
        /// Shut down running event queue.
        /// </summary>
        protected virtual void ShutdownEventQueue()
        {
            EventDispatcher eventDispatcher = m_eventDispatcher;
            if (eventDispatcher != null)
            {
                m_eventDispatcher = null;
                eventDispatcher.Stop();
            }
        }

        #endregion

        #region Inner class: EventDispatcher

        /// <summary>
        /// <see cref="Daemon"/> used to dispatch asynchronous
        /// <see cref="CacheEventArgs"/>s.
        /// </summary>
        public class EventDispatcher : QueueProcessor
        {
            #region Properties

            /// <summary>
            /// Parent ContinuousQueryCache.
            /// </summary>
            /// <value>
            /// Parent ContinuousQueryCache.
            /// </value>
            public virtual ContinuousQueryCache ContinuousQueryCache
            {
                get { return m_querycache; }
                set { m_querycache = value; }
            }

            /// <summary>
            /// This is the <b>Queue</b> to which items that need to be
            /// processed are added, and from which the daemon pulls items to
            /// process.
            /// </summary>
            public override Queue Queue
            {
                get
                {
                    if (m_queue == null)
                    {
                        m_queue = new Queue();
                    }
                    return m_queue;
                }
                set { m_queue = value; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Default constructor.
            /// </summary>
            public EventDispatcher()
            {
                DaemonState = DaemonState.Initial;
            }

            #endregion

            #region Daemon override methods

            /// <summary>
            /// This event occurs when an exception is thrown from
            /// <b>OnEnter</b>, <b>OnWait</b>, <b>OnNotify</b> and <b>OnExit</b>.
            /// </summary>
            /// <param name="e">
            /// Exception that has occured.
            /// </param>
            protected override void OnException(Exception e)
            {
                if (!IsExiting)
                {}
            }

            /// <summary>
            /// Event notification called right before the daemon thread
            /// terminates.
            /// </summary>
            /// <remarks>
            /// This method is guaranteed to be called only once and on the
            /// daemon's thread.
            /// </remarks>
            protected override void OnExit()
            {
                // drain the queue
                OnNotify();
                base.OnExit();
            }

            /// <summary>
            /// Event notification to perform a regular daemon activity.
            /// </summary>
            /// <remarks>
            /// To get it called, another thread has to set IsNotification to
            /// <b>true</b>:
            /// <c>daemon.IsNotification = true;</c>
            /// </remarks>
            protected override void OnNotify()
            {
                Queue     queue = Queue;
                IRunnable task;
                while ((task = (IRunnable) queue.RemoveNoWait()) != null)
                {
                    try
                    {
                        task.Run();
                    }
                    catch (Exception e)
                    {
                        OnException(e);
                    }
                }
            }

            #endregion

            #region Data members

            /// <summary>
            /// Parent ContinousQueryCache.
            /// </summary>
            private ContinuousQueryCache m_querycache;

            /// <summary>
            /// This is the Queue to which items that need to be processed are
            /// added, and from which the daemon pulls items to process.
            /// </summary>
            private Queue m_queue;

            #endregion
        }

        #endregion

        #region Enum: CacheState

        /// <summary>
        /// The <b>ContinuousQueryCache</b> state values.
        /// </summary>
        public enum CacheState
        {
            /// <summary>
            /// The ContinousQueryCache must be configured or re-configured
            /// before it can be used.
            /// </summary>
            [Obsolete("As of Coherence 3.4 this value is replaced with Disconnected")]
            Init = 0,

            /// <summary>
            /// The content of the ContinousQueryCache is not fully
            /// synchronized with the underlying [clustered] cache.
            /// </summary>
            /// <remarks>
            /// If the value of the
            /// <see cref="ContinuousQueryCache.ReconnectInterval"/> is zero,
            /// it must be configured (synchronized) before it can be used.
            /// </remarks>
            /// <since>Coherence 3.4</since>
            Disconnected = 0,

            /// <summary>
            /// The ContinuousQueryCache is configuring or re-configuring its
            /// listeners and content.
            /// </summary>
            Configuring = 1,

            /// <summary>
            /// The ContinousQueryCache has been configured.
            /// </summary>
            Configured = 2,

            /// <summary>
            /// The ContinousQueryCache has been configured and fully
            /// synchronized.
            /// </summary>
            Synchronized = 3
        }

        #endregion
    }
}
