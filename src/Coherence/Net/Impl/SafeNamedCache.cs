/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Security.Principal;
using System.Threading;
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Util;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// "Safe" wrapper for <see cref="RemoteNamedCache"/>.
    /// </summary>
    public class SafeNamedCache : INamedCache, ICacheListener
    {
        #region Properties

        /// <summary>
        /// Actual (wrapped) <see cref="INamedCache"/>.
        /// </summary>
        /// <value>
        /// Wrapped <b>INamedCache</b>.
        /// </value>
        public virtual INamedCache NamedCache
        {
            get
            {
                using (BlockingLock l = BlockingLock.Lock(SyncRoot))
                {
                    return m_namedCache;
                }
            }
            set
            {
                using (BlockingLock l = BlockingLock.Lock(SyncRoot))
                {
                    m_namedCache = value;
                }
            }
        }

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
                using (BlockingLock l = BlockingLock.Lock(SyncRoot))
                {
                    return m_cacheName;
                }
            }
            set
            {
                using (BlockingLock l = BlockingLock.Lock(SyncRoot))
                {
                    m_cacheName = value;
                }
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
            get { return SafeCacheService; }
        }

        /// <summary>
        /// Gets the <see cref="RemoteCacheService"/> that this INamedCache
        /// is a part of.
        /// </summary>
        /// <value>
        /// The cache service this INamedCache is a part of.
        /// </value>
        public virtual SafeCacheService SafeCacheService
        {
            get
            {
                using (BlockingLock l = BlockingLock.Lock(SyncRoot))
                {
                    return m_cacheService;
                }
            }
            set
            {
                using (BlockingLock l = BlockingLock.Lock(SyncRoot))
                {
                    m_cacheService = value;
                }
            }
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
                    return NamedCache.IsActive;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Specifies whether or not the underlying <see cref="INamedCache"/>
        /// has been explicitly released.
        /// </summary>
        /// <value>
        /// Specifies whether or not the underlying <b>INamedCache</b> has
        /// been explicitly released.
        /// </value>
        public virtual bool IsReleased
        {
            get
            {
                using (BlockingLock l = BlockingLock.Lock(SyncRoot))
                {
                    return m_isReleased;
                }
            }
            set
            {
                using (BlockingLock l = BlockingLock.Lock(SyncRoot))
                {
                    m_isReleased = value;
                }
            }
        }

        /// <summary>
        /// Calculated property that returns the running wrapped
        /// <b>INamedCache</b>.
        /// </summary>
        /// <value>
        /// The wrapped <b>INamedCache</b>.
        /// </value>
        public virtual INamedCache RunningNamedCache
        {
            get
            {
                INamedCache cache   = NamedCache;
                IService    service = cache == null ? null : cache.CacheService;

                if (service == null || !service.IsRunning || !cache.IsActive)
                {
                    SafeService safeservice = SafeCacheService;
                    using (BlockingLock l = BlockingLock.Lock(safeservice))
                    {
                        using (BlockingLock l2 = BlockingLock.Lock(SyncRoot))
                        {
                            cache   = NamedCache;
                            service = cache == null ? null : cache.CacheService;
                            if (service == null || !service.IsRunning || !cache.IsActive)
                            {
                                if (IsReleased)
                                {
                                    throw new InvalidOperationException("SafeNamedCache was explicitly released");
                                }
                                else
                                {
                                    // restart the actual named cache
                                    if (cache != null)
                                    {
                                        NamedCache = null;
                                        CacheFactory.Log("Restarting NamedCache: " + CacheName,
                                                          CacheFactory.LogLevel.Info);
                                    }
                                    NamedCache = cache = RestartNamedCache();
                                }
                            }
                        }
                    }

                    safeservice.DrainEvents();
                }
                return cache;
            }
        }

        /// <summary>
        /// Returns the number of key-value mappings in this cache.
        /// </summary>
        /// <remarks>
        /// Note that this number does not include the items that were
        /// <i>locked</i> but didn't have corresponding cache entries.
        /// </remarks>
        /// <value>
        /// The number of key-value mappings in this cache.
        /// </value>
        public virtual int Count
        {
            get { return RunningNamedCache.Count; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the
        /// <b>ICollection</b>.
        /// </summary>
        /// <value>
        /// An object that can be used to synchronize access to the
        /// <b>ICollection</b>.
        /// </value>
        public virtual object SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <b>ICollection</b>
        /// is synchronized (thread safe).
        /// </summary>
        /// <value>
        /// <b>true</b> if access to the <b>ICollection</b> is synchronized
        /// (thread safe); otherwise, <b>false</b>.
        /// </value>
        public virtual bool IsSynchronized
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the value to which this cache maps the specified key.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Returns <c>null</c> if the cache contains no mapping for
        /// this key. A return value of <c>null</c> does not
        /// <i>necessarily</i> indicate that the cache contains no mapping
        /// for the key; it's also possible that the cache explicitly maps
        /// the key to <c>null</c>.</p>
        /// <p>
        /// The <see cref="IDictionary.Contains"/> operation may be used to
        /// distinguish these two cases.</p>
        /// </remarks>
        /// <param name="key">
        /// Key whose associated value is to be returned.
        /// </param>
        /// <returns>
        /// The value to which this cache maps the specified key, or
        /// <c>null</c> if the cache contains no mapping for this key.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// If the key is of an inappropriate type for this cache.
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// If the key is <c>null</c> and this cache does not permit
        /// <c>null</c> keys.
        /// </exception>
        /// <seealso cref="IDictionary.Contains(object)"/>
        public virtual object this[object key]
        {
            get { return RunningNamedCache[key]; }
            set { RunningNamedCache.Add(key, value); }
        }

        /// <summary>
        /// Gets an <b>ICollection</b> containing the keys of the
        /// <b>IDictionary</b>.
        /// </summary>
        /// <returns>
        /// An <b>ICollection</b> object containing the keys of the
        /// <b>IDictionary</b> object.
        /// </returns>
        public virtual ICollection Keys
        {
            get { return RunningNamedCache.Keys; }
        }

        /// <summary>
        /// Gets an <b>ICollection</b> containing the values of the
        /// <b>IDictionary</b>.
        /// </summary>
        /// <returns>
        /// An <b>ICollection</b> object containing the values of the
        /// <b>IDictionary</b> object.
        /// </returns>
        public virtual ICollection Values
        {
            get { return RunningNamedCache.Values; }
        }

        /// <summary>
        /// Gets a value indicating whether the <b>IDictionary</b> object is
        /// read-only.
        /// </summary>
        /// <value>
        /// Always <b>true</b> for this <b>INamedCache</b>.
        /// </value>
        public virtual bool IsReadOnly
        {
            get { return RunningNamedCache.IsReadOnly; }
        }

        /// <summary>
        /// Gets a value indicating whether the <b>IDictionary</b> object has
        /// a fixed size.
        /// </summary>
        /// <value>
        /// Always <b>false</b> for this <b>INamedCache</b>.
        /// </value>
        public virtual bool IsFixedSize
        {
            get { return RunningNamedCache.IsFixedSize; }
        }

        /// <summary>
        /// <see cref="CacheListenerSupport"/> used by this
        /// <see cref="INamedCache"/> to dispatch
        /// <see cref="CacheEventArgs"/>s to registered
        /// <see cref="ICacheListener"/>s.
        /// </summary>
        public virtual CacheListenerSupport CacheListenerSupport
        {
            get { return m_cacheListenerSupport; }
        }
        
        /// <summary>
        /// The optional <b>IPrincipal</b> object associated with this
        /// cache.
        /// </summary>
        /// <remarks>
        /// If an <b>IPrincipal</b> is associated with this cache, 
        /// RestartNamedCache will be done on behalf of this <b>IPrincipal</b>.
        /// </remarks>
        /// <value>
        /// The <b>IPrincipal</b> associated with this cache.
        /// </value>
        public virtual IPrincipal Principal
        {
            get { return m_principal; }
            set { m_principal = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SafeNamedCache()
        {
            m_cacheListenerSupport = new CacheListenerSupport();
        }

        #endregion

        #region IDictionary implementation

        /// <summary>
        /// Determines whether the <b>IDictionary</b> contains an element
        /// with the specified key.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the <b>IDictionary</b> contains an element with
        /// the key; otherwise, <b>false</b>.
        /// </returns>
        /// <param name="key">
        /// The key to locate in the <b>IDictionary</b>.
        /// </param>
        public virtual bool Contains(object key)
        {
            return RunningNamedCache.Contains(key);
        }

        /// <summary>
        /// Removes all mappings from this dictionary.
        /// </summary>
        /// <remarks>
        /// Some implementations will attempt to lock the entire dictionary
        /// (if necessary) before preceeding with the clear operation. For
        /// such implementations, the entire dictionary has to be either
        /// already locked or able to be locked for this operation to
        /// succeed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// If the lock could not be succesfully obtained for some key.
        /// </exception>
        public virtual void Clear()
        {
            RunningNamedCache.Clear();
        }

        /// <summary>
        /// Adds an element with the provided key and value to the
        /// <b>IDictionary</b> object.
        /// </summary>
        /// <param name="value">
        /// The object to use as the value of the element to add.
        /// </param>
        /// <param name="key">
        /// The object to use as the key of the element to add.
        /// </param>
        public virtual void Add(object key, object value)
        {
            RunningNamedCache.Add(key, value);
        }

        /// <summary>
        /// Removes the element with the specified key from the
        /// <b>IDictionary</b> object.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        public virtual void Remove(object key)
        {
            RunningNamedCache.Remove(key);
        }

        /// <summary>
        /// Copies the elements of the <b>ICollection</b> to an array,
        /// starting at a particular array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional array that is the destination of the elements
        /// copied from <b>ICollection</b>.
        /// </param>
        /// <param name="index">
        /// The zero-based index in array at which copying begins.
        /// </param>
        public virtual void CopyTo(Array array, int index)
        {
            RunningNamedCache.CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <b>IEnumerator</b> object that can be used to iterate through
        /// the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) RunningNamedCache).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <b>IEnumerator</b> object that can be used to iterate through
        /// the collection.
        /// </returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary) RunningNamedCache).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <b>IEnumerator</b> object that can be used to iterate through
        /// the collection.
        /// </returns>
        ICacheEnumerator ICache.GetEnumerator()
        {
            return RunningNamedCache.GetEnumerator();
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
        public virtual IDictionary GetAll(ICollection keys)
        {
            return RunningNamedCache.GetAll(keys);
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
        /// indicate that the cache previously associated <c>null</c>
        /// with the specified key, if the implementation supports
        /// <c>null</c> values.
        /// </returns>
        public virtual object Insert(object key, object value)
        {
            return RunningNamedCache.Insert(key, value);
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
            return RunningNamedCache.Insert(key, value, millis);
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
            RunningNamedCache.InsertAll(dictionary);
        }

        /// <summary>
        /// Gets a collection of <see cref="ICacheEntry"/> instances
        /// within the cache.
        /// </summary>
        /// <value>
        /// A collection of <b>ICacheEntry</b> objects.
        /// </value>
        public virtual ICollection Entries
        {
            get { return RunningNamedCache.Entries; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through cache items.
        /// </summary>
        /// <returns>
        /// An <b>IEnumerator</b> object that can be used to iterate through
        /// cache items.
        /// </returns>
        public virtual ICacheEnumerator GetEnumerator()
        {
            return RunningNamedCache.GetEnumerator();
        }

        #endregion

        #region INamedCache implementation

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
            SafeService safeservice = SafeCacheService;
            using (BlockingLock l = BlockingLock.Lock(safeservice))
            {
                using (BlockingLock l2 = BlockingLock.Lock(SyncRoot))
                {
                    IsReleased = true;
                    ReleaseListeners();
                    SafeCacheService.ReleaseCache(this);
                    NamedCache = null;
                }
            }
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
            SafeService safeservice = SafeCacheService;
            using (BlockingLock l = BlockingLock.Lock(safeservice))
            {
                using (BlockingLock l2 = BlockingLock.Lock(SyncRoot))
                {
                    IsReleased = true;
                    ReleaseListeners();
                    SafeCacheService.DestroyCache(this);
                    NamedCache = null;
                }
            }
        }

        /// <summary>
        /// Removes all mappings from this cache.
        /// </summary>
        /// <remarks>
        /// Note: the removal of entries caused by this truncate operation will
        /// not be observable.
        /// </remarks>
        public virtual void Truncate()
        {
            RunningNamedCache.Truncate();
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
        /// <see cref="RemoveCacheListener(ICacheListener, object)"/>
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
            if (listener == this)
            {
                INamedCache cache = NamedCache;
                try
                {
                    cache.AddCacheListener(listener, key, isLite);
                }
                catch (Exception)
                {
                    if (cache != null && cache.IsActive && cache.CacheService.IsRunning)
                    {
                        throw;
                    }
                    // NamedCache has been invalidated
                }
            }
            else if (listener is CacheListenerSupport.ISynchronousListener || listener is CacheTriggerListener)
            {
                RunningNamedCache.AddCacheListener(listener, key, isLite);
            }
            else if (listener != null)
            {
                bool wasEmpty;
                bool wasLite;

                CacheListenerSupport support = CacheListenerSupport;
                using (BlockingLock l = BlockingLock.Lock(support))
                {
                    wasEmpty = support.IsEmpty(key);
                    wasLite  = wasEmpty || !support.ContainsStandardListeners(key);

                    support.AddListener(listener, key, isLite);
                }

                if (wasEmpty || (wasLite && !isLite))
                {
                    try
                    {
                        AddCacheListener(this, key, isLite);
                    }
                    catch (Exception)
                    {
                        support.RemoveListener(listener, key);
                        throw;
                    }
                }
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
            if (listener == this || listener is CacheListenerSupport.ISynchronousListener)
            {
                INamedCache cache = NamedCache;
                try
                {
                    cache.RemoveCacheListener(listener, key);
                }
                catch (Exception)
                {
                    if (cache != null && cache.IsActive && cache.CacheService.IsRunning)
                    {
                        throw;
                    }
                    // NamedCache has been invalidated
                }
            }
            else if (listener != null)
            {
                bool isEmpty;

                CacheListenerSupport support = CacheListenerSupport;
                using (BlockingLock l = BlockingLock.Lock(support))
                {
                    support.RemoveListener(listener, key);
                    isEmpty = support.IsEmpty(key);
                }

                if (isEmpty)
                {
                    RemoveCacheListener(this, key);
                }
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
        /// <see cref="RemoveCacheListener(ICacheListener, IFilter)"/>
        /// method.</p>
        /// </remarks>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to add.</param>
        /// <param name="filter">
        /// A filter that will be passed <b>CacheEvent</b> objects to
        /// select from; a <b>CacheEvent</b> will be delivered to the
        /// listener only if the filter evaluates to <b>true</b> for that
        /// <b>CacheEvent</b>; <c>null</c> is equivalent to a filter
        /// that alway returns <b>true</b>.
        /// </param>
        /// <param name="isLite">
        /// <b>true</b> to indicate that the <see cref="CacheEventArgs"/>
        /// objects do not have to include the <b>OldValue</b> and
        /// <b>NewValue</b> property values in order to allow optimizations.
        /// </param>
        public virtual void AddCacheListener(ICacheListener listener, IFilter filter, bool isLite)
        {
            if (listener == this)
            {
                INamedCache cache = NamedCache;
                try
                {
                    cache.AddCacheListener(listener, filter, isLite);
                }
                catch (Exception)
                {
                    if (cache != null && cache.IsActive && cache.CacheService.IsRunning)
                    {
                        throw;
                    }
                    // NamedCache has been invalidated
                }
            }
            else if (listener is CacheListenerSupport.ISynchronousListener || listener is CacheTriggerListener)
            {
                RunningNamedCache.AddCacheListener(listener, filter, isLite);
            }
            else if (listener != null)
            {
                bool wasEmpty;
                bool wasLite;

                CacheListenerSupport support = CacheListenerSupport;
                using (BlockingLock l = BlockingLock.Lock(support))
                {
                    wasEmpty = support.IsEmpty(filter);
                    wasLite  = wasEmpty || !support.ContainsStandardListeners(filter);

                    support.AddListener(listener, filter, isLite);
                }

                if (wasEmpty || (wasLite && !isLite))
                {
                    try
                    {
                        AddCacheListener(this, filter, isLite);
                    }
                    catch (Exception)
                    {
                        support.RemoveListener(listener, filter);
                        throw;
                    }
                }
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
            if (listener == this || listener is CacheListenerSupport.ISynchronousListener || listener is CacheTriggerListener)
            {
                INamedCache cache = NamedCache;
                try
                {
                    cache.RemoveCacheListener(listener, filter);
                }
                catch (Exception)
                {
                    if (cache != null && cache.IsActive && cache.CacheService.IsRunning)
                    {
                        throw;
                    }
                    // NamedCache has been invalidated
                }
            }
            else if (listener != null)
            {
                bool isEmpty;

                CacheListenerSupport support = CacheListenerSupport;
                using (BlockingLock l = BlockingLock.Lock(support))
                {
                    support.RemoveListener(listener, filter);
                    isEmpty = support.IsEmpty(filter);
                }

                if (isEmpty)
                {
                    RemoveCacheListener(this, filter);
                }
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
        /// Some implementations may allow the entire map to be locked. If
        /// the map is locked in such a way, then only a lock holder is
        /// allowed to perform any of the "put" or "remove" operations.</p>
        /// <p>
        /// Passing the special constant
        /// <see cref="LockScope.LOCK_ALL"/> as the <i>key</i>
        /// parameter to indicate the cache lock is not allowed for
        /// SafeNamedCache and will cause an exception to be thrown.</p>
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
        [Obsolete("Obsolete as of Coherence 12.1")]
        public virtual bool Lock(object key, long waitTimeMillis)
        {
            return RunningNamedCache.Lock(key, waitTimeMillis);
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
        [Obsolete("Obsolete as of Coherence 12.1")]
        public virtual bool Lock(object key)
        {
            return RunningNamedCache.Lock(key);
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
        [Obsolete("Obsolete as of Coherence 12.1")]
        public virtual bool Unlock(object key)
        {
            return RunningNamedCache.Unlock(key);
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
            return RunningNamedCache.GetKeys(filter);
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
            return RunningNamedCache.GetValues(filter);
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
            return RunningNamedCache.GetValues(filter, comparer);
        }

        /// <summary>
        /// Return a collectioin of the entries contained in this cache
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
            return RunningNamedCache.GetEntries(filter);
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
            return RunningNamedCache.GetEntries(filter, comparer);
        }

        /// <summary>
        /// Add an index to this IQueryCache.
        /// </summary>
        /// <remarks>
        /// This allows to correlate values stored in this
        /// <i>indexed cache</i> (or attributes of those values) to the
        /// corresponding keys in the indexed cache and increase the
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
            RunningNamedCache.AddIndex(extractor, isOrdered, comparer);
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
            RunningNamedCache.RemoveIndex(extractor);
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
            return RunningNamedCache.Invoke(key, agent);
        }

        /// <summary>
        /// Invoke the passed <see cref="IEntryProcessor"/> against the
        /// entries specified by the passed keys, returning the result of the
        /// invocation for each.
        /// </summary>
        /// <param name="keys">
        /// The keys to process; these keys are not required to exist within
        /// the cache.
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
            return RunningNamedCache.InvokeAll(keys, agent);
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
            return RunningNamedCache.InvokeAll(filter, agent);
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
        /// across the specified entries of this cache.
        /// </param>
        /// <returns>
        /// The result of the aggregation.
        /// </returns>
        public virtual object Aggregate(ICollection keys, IEntryAggregator agent)
        {
            return RunningNamedCache.Aggregate(keys, agent);
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
        /// across the selected entries of this cache.
        /// </param>
        /// <returns>
        /// The result of the aggregation.
        /// </returns>
        public virtual object Aggregate(IFilter filter, IEntryAggregator agent)
        {
            return RunningNamedCache.Aggregate(filter, agent);
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
            TranslateCacheEvent(evt);
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
            TranslateCacheEvent(evt);
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
            TranslateCacheEvent(evt);
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
        /// Convert <see cref="CacheEventArgs"/> into new one with all
        /// properties copied except <see cref="CacheEventArgs.Cache"/> which
        /// is set to this cache.
        /// </summary>
        /// <param name="evt">
        /// <b>CacheEventArgs</b> object.
        /// </param>
        protected virtual void TranslateCacheEvent(CacheEventArgs evt)
        {
            if (evt.Cache == NamedCache)
            {
                // ensure lazy event data access
                evt = CacheListenerSupport.ConvertEvent(evt, this, null, null);
                CacheListenerSupport.FireEvent(evt, true);
            }
        }

        /// <summary>
        /// Release all <see cref="ICacheListener"/> instances registered
        /// with this cache.
        /// </summary>
        protected virtual void ReleaseListeners()
        {
            CacheListenerSupport support = CacheListenerSupport;
            if (!support.IsEmpty())
            {
                ICollection listFilter = new ArrayList();
                ICollection listKeys   = new ArrayList();
                using (BlockingLock l = BlockingLock.Lock(support))
                {
                    if (!support.IsEmpty())
                    {
                        CollectionUtils.AddAll(listFilter, support.Filters);
                        CollectionUtils.AddAll(listKeys, support.Keys);
                        support.Clear();
                    }
                }

                foreach (IFilter filter in listFilter)
                {
                    RemoveCacheListener(this, filter);
                }
                foreach (object key in listKeys)
                {
                    RemoveCacheListener(this, key);
                }
            }
        }

        /// <summary>
        /// Restarts underlying <b>RemoteNamedCache</b>.
        /// </summary>
        /// <returns>
        /// Active instance of SafeNamedCache.
        /// </returns>
        protected virtual INamedCache RestartNamedCache()
        {
            IPrincipal currentPrincipal = Thread.CurrentPrincipal;
            // In case the underlying cache is scoped by Principal, use the 
            // original Principal
            Thread.CurrentPrincipal = Principal;
            INamedCache cache;
            try
            {
                cache = SafeCacheService.EnsureRunningCacheService(false)
                        .EnsureCache(CacheName);
            }
            finally
            {
                // restore the Principal
                Thread.CurrentPrincipal = currentPrincipal;
            }

            CacheListenerSupport support = CacheListenerSupport;
            using (BlockingLock l = BlockingLock.Lock(support))
            {
                if (!support.IsEmpty())
                {
                    foreach (IFilter filter in support.Filters)
                    {
                        cache.AddCacheListener(this, filter, !support.ContainsStandardListeners(filter));
                    }
                    foreach (object key in support.Keys)
                    {
                        cache.AddCacheListener(this, key, !support.ContainsStandardListeners(key));
                    }
                }
            }

            return cache;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Provide a human-readable representation of this <b>SafeNamedCache</b>.
        /// </summary>
        /// <returns>
        /// A human-readable representation of this <b>SafeNamedCache</b>.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + ": " + m_namedCache;
        }

        #endregion

        #region Data members

        /// <summary>
        /// Actual (wrapped) INamedCache.
        /// </summary>
        private INamedCache m_namedCache;

        /// <summary>
        /// The cache name.
        /// </summary>
        private string m_cacheName;

        /// <summary>
        /// SafeCacheService this INamedCache is part of.
        /// </summary>
        private SafeCacheService m_cacheService;

        /// <summary>
        /// Specifies whether or not the underlying INamedCache has been
        /// explicitly released.
        /// </summary>
        private bool m_isReleased;

        /// <summary>
        /// CacheListenerSupport used by this INamedCache to dispatch
        /// CacheEventArgs to registered IDictionaryListeners.
        /// </summary>
        private readonly CacheListenerSupport m_cacheListenerSupport;

        /// <summary>
        /// The <b>IPrincipal</b> associated with the cache.
        /// </summary>
        private IPrincipal m_principal;

        #endregion
    }
}
