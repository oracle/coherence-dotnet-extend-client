/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Net.Cache;
using Tangosol.Util;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// <see cref="INamedCache"/> wrapper for <see cref="LocalCache"/>.
    /// </summary>
    /// <author>Ivan Cikic  2006.11.17</author>
    public class LocalNamedCache : INamedCache
    {
        #region Properties

        /// <summary>
        /// Actual (wrapped) <see cref="LocalCache"/>.
        /// </summary>
        /// <value>
        /// Wrapped <b>LocalCache</b>.
        /// </value>
        public virtual LocalCache LocalCache
        {
            get
            {
                lock (this)
                {
                    if (m_isReleased)
                    {
                        throw new InvalidOperationException("cache has been released");
                    }

                    LocalCache cache = m_localCache;
                    if (cache == null)
                    {
                        throw new InvalidOperationException("cache is not active");
                    }
                    return cache;
                }
            }
            set
            {
                lock (this)
                {
                    m_localCache = value;
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
                lock (this)
                {
                    return m_cacheName;
                }
            }
            set
            {
                lock (this)
                {
                    m_cacheName = value;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ICacheService"/> that this LocalCache is a
        /// part of.
        /// </summary>
        /// <value>
        /// The cache service this LocalCache is a part of.
        /// </value>
        public virtual ICacheService CacheService
        {
            get
            {
                lock (this)
                {
                    return m_cacheService;
                }
            }
            set
            {
                lock (this)
                {
                    m_cacheService = value;
                }
            }
        }

        /// <summary>
        /// Specifies whether or not the <see cref="LocalCache"/> is active.
        /// </summary>
        /// <value>
        /// <b>true</b> if the <b>LocalCache</b> is active; <b>false</b>
        /// otherwise.
        /// </value>
        public virtual bool IsActive
        {
            get
            {
                lock (this)
                {
                    return m_localCache != null;
                }
            }
        }

        /// <summary>
        /// Specifies whether or not the underlying <see cref="LocalCache"/>
        /// has been explicitly released.
        /// </summary>
        /// <value>
        /// Specifies whether or not the underlying <b>LocalCache</b> has
        /// been explicitly released.
        /// </value>
        public virtual bool IsReleased
        {
            get
            {
                lock (this)
                {
                    return m_isReleased;
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct the LocalNamedCache.
        /// </summary>
        public LocalNamedCache()
        {
            m_localCache = new LocalCache();
        }

        /// <summary>
        /// Construct the LocalNamedCache.
        /// </summary>
        /// <param name="units">
        /// The number of units that the underlying <b>LocalCache</b> will
        /// cache before pruning the cache.
        /// </param>
        public LocalNamedCache(int units)
        {
            m_localCache = new LocalCache(units);
        }

        /// <summary>
        /// Construct the LocalNamedCache.
        /// </summary>
        /// <param name="units">
        /// The number of units that the underlying <b>LocalCache</b> will
        /// cache before pruning the cache.
        /// </param>
        /// <param name="expiryMillis">
        /// The number of milliseconds that each cache entry lives before
        /// being automatically expired.
        /// </param>
        public LocalNamedCache(int units, int expiryMillis)
        {
            m_localCache = new LocalCache(units, expiryMillis);
        }

        /// <summary>
        /// Construct the LocalNamedCache.
        /// </summary>
        /// <param name="units">
        /// The number of units that the underlying <b>LocalCache</b> will
        /// cache before pruning the cache.
        /// </param>
        /// <param name="expiryMillis">
        /// The number of milliseconds that each cache entry lives before
        /// being automatically expired.
        /// </param>
        /// <param name="pruneLevel">
        /// The percentage of the total number of units that will remain
        /// after the underlying <b>LocalCache</b> prunes the cache (i.e.
        /// this is the "low water mark" value); this value is in the range
        /// 0.0 to 1.0.
        /// </param>
        public LocalNamedCache(int units, int expiryMillis, double pruneLevel)
        {
            m_localCache = new LocalCache(units, expiryMillis, pruneLevel);
        }

        /// <summary>
        /// Construct the LocalNamedCache.
        /// </summary>
        /// <param name="units">
        /// The number of units that the underlying <b>LocalCache</b> will
        /// cache before pruning the cache.
        /// </param>
        /// <param name="expiryMillis">
        /// The number of milliseconds that each cache entry lives before
        /// being automatically expired.
        /// </param>
        /// <param name="loader">
        /// The <see cref="ICacheLoader"/> or <see cref="ICacheStore"/> to
        /// use.
        /// </param>
        public LocalNamedCache(int units, int expiryMillis, ICacheLoader loader)
        {
            m_localCache = new LocalCache(units, expiryMillis, loader);
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
            lock (this)
            {
                if (!m_isReleased)
                {
                    m_isReleased = true;
                    m_localCache = null;
                }
            }
        }

        /// <summary>
        /// Release and destroy this instance of INamedCache.
        /// </summary>
        public virtual void Destroy()
        {
            Release();
        }

        /// <summary>
        /// Truncate is not support for LocalNamedCache.
        /// </summary>
        public virtual void Truncate()
        {
            throw new NotSupportedException("Truncate is not supported");
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
            LocalCache.AddCacheListener(listener);
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
            LocalCache.RemoveCacheListener(listener);
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
            LocalCache.AddCacheListener(listener, key, isLite);
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
            LocalCache.RemoveCacheListener(listener, key);
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
            LocalCache.AddCacheListener(listener, filter, isLite);
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
            LocalCache.RemoveCacheListener(listener, filter);
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
            return LocalCache.GetAll(keys);
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
            return LocalCache.Insert(key, value);
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
            return LocalCache.Insert(key, value, millis);
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
            LocalCache.InsertAll(dictionary);
        }

        /// <summary>
        /// Gets a collection of <see cref="ICacheEntry"/> instances
        /// within the cache.
        /// </summary>
        public virtual ICollection Entries
        {
            get { return LocalCache.Entries; }
        }

        /// <summary>
        /// Returns an <see cref="ICacheEnumerator"/> object for the
        /// <b>ICache</b> instance.
        /// </summary>
        /// <returns>An <b>ICacheEnumerator</b> object for the
        /// <b>ICache</b> instance.</returns>
        ICacheEnumerator ICache.GetEnumerator()
        {
            return LocalCache.GetEnumerator();
        }

        #endregion

        #region IDictionary and ICollection implementation

        /// <summary>
        /// Determines whether the <see cref="IDictionary"/> object contains
        /// an element with the specified key.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the <b>IDictionary</b> contains an element with
        /// the key; otherwise, <b>false</b>.
        /// </returns>
        public virtual bool Contains(object key)
        {
            return LocalCache.Contains(key);
        }

        /// <summary>
        /// Adds an element with the provided key and value to the
        /// <b>IDictionary</b> object.
        /// </summary>
        public virtual void Add(object key, object value)
        {
            LocalCache.Add(key, value);
        }

        /// <summary>
        /// Removes all elements from the <b>IDictionary</b> object.
        /// </summary>
        public virtual void Clear()
        {
            LocalCache.Clear();
        }

        /// <summary>
        /// Returns an <b>IDictionaryEnumerator</b> object for the
        /// <b>IDictionary</b> object.
        /// </summary>
        /// <returns>
        /// An <b>IDictionaryEnumerator</b> object for the <b>IDictionary</b>
        /// object.
        /// </returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return LocalCache.GetEnumerator();
        }

        /// <summary>
        /// Removes the element with the specified key from the
        /// <b>IDictionary</b>.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        public virtual void Remove(object key)
        {
            LocalCache.Remove(key);
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key of the element to get or set.
        /// </param>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        public virtual object this[object key]
        {
            get { return LocalCache[key]; }
            set { LocalCache[key] = value; }
        }

        /// <summary>
        /// Gets an <b>ICollection</b> object containing the keys of the
        /// <b>IDcitionary</b> object.
        /// </summary>
        /// <returns>
        /// An <b>ICollection</b> object containing the keys of the
        /// <b>IDictionary</b> object.
        /// </returns>
        public virtual ICollection Keys
        {
            get { return LocalCache.Keys; }
        }

        /// <summary>
        /// Gets an <b>ICollection</b> object containing the values in the
        /// <b>IDictionary</b> object.
        /// </summary>
        /// <returns>
        /// An <b>ICollection</b> object containing the values in the
        /// <b>IDictionary</b> object.
        /// </returns>
        public virtual ICollection Values
        {
            get { return LocalCache.Values; }
        }

        /// <summary>
        /// Gets a value indicating whether the <b>IDictionary</b> object is
        /// read-only.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the <b>IDictionary</b> object is read-only;
        /// otherwise, false.
        /// </returns>
        public virtual bool IsReadOnly
        {
            get { return LocalCache.IsReadOnly; }
        }

        /// <summary>
        /// Gets a value indicating whether the <b>IDictionary</b> object has
        /// a fixed size.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the <b>IDictionary</b> object has a fixed size;
        /// otherwise, <b>false</b>.
        /// </returns>
        public virtual bool IsFixedSize
        {
            get { return LocalCache.IsFixedSize; }
        }

        /// <summary>
        /// Copies the elements of the cache to an array, starting at a
        /// particular index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the
        /// destination of the elements copied from the cache.
        /// </param>
        /// <param name="index">
        /// The zero-based index in array at which copying begins.
        /// </param>
        public virtual void CopyTo(Array array, int index)
        {
            LocalCache.CopyTo(array, index);
        }

        /// <summary>
        /// Gets the number of elements contained in the <b>ICollection</b>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <b>ICollection</b>.
        /// </returns>
        public virtual int Count
        {
            get { return LocalCache.Count; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the
        /// <b>ICollection</b>.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the
        /// <b>ICollection</b>.
        /// </returns>
        public virtual object SyncRoot
        {
            get { return LocalCache.SyncRoot; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <b>ICollection</b>
        /// is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// <b>true</b> if access to the <b>ICollection</b> is synchronized
        /// (thread safe); otherwise, <b>false</b>.
        /// </returns>
        public virtual bool IsSynchronized
        {
            get { return LocalCache.IsSynchronized; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <b>IEnumerator</b> object that can be used to iterate through
        /// the collection.
        /// </returns>
        public virtual IEnumerator GetEnumerator()
        {
            return LocalCache.GetEnumerator();
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
            return LocalCache.Lock(key, waitTimeMillis);
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
            return LocalCache.Lock(key);
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
            return LocalCache.Unlock(key);
        }

        #endregion

        #region IInvocableCache implementation

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
            return LocalCache.GetKeys(filter);
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
            return LocalCache.GetValues(filter);
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
            return LocalCache.GetValues(filter, comparer);
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
            return LocalCache.GetEntries(filter);
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
            return LocalCache.GetEntries(filter, comparer);
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
            LocalCache.AddIndex(extractor, isOrdered, comparer);
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
            LocalCache.RemoveIndex(extractor);
        }

        #endregion

        #region IQueryCache implementation

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
            return LocalCache.Invoke(key, agent);
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
            return LocalCache.InvokeAll(keys, agent);
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
            return LocalCache.InvokeAll(filter, agent);
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
            return LocalCache.Aggregate(keys, agent);
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
            return LocalCache.Aggregate(filter, agent);
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

        #region Data members

        /// <summary>
        /// Actual (wrapped) <see cref="LocalCache"/>.
        /// </summary>
        private LocalCache m_localCache;

        /// <summary>
        /// The cache name.
        /// </summary>
        [NonSerialized]
        private string m_cacheName;

        /// <summary>
        /// <see cref="ICacheService"/> this <see cref="INamedCache"/> is
        /// part of.
        /// </summary>
        private ICacheService m_cacheService;

        /// <summary>
        /// Specifies whether or not the underlying <see cref="LocalCache"/>
        /// has been explicitly released.
        /// </summary>
        private bool m_isReleased;

        #endregion
    }
}