/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Net.Internal;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;
using Tangosol.Net.Messaging.Impl.NamedCache;
using Tangosol.Util;
using Tangosol.Util.Comparator;
using Tangosol.Util.Daemon.QueueProcessor;
using Tangosol.Util.Filter;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// <see cref="INamedCache"/> implementation that delegates to a remote
    /// <b>INamedCache</b> using an <see cref="IChannel"/>.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.09.07</author>
    public class RemoteNamedCache : Extend, INamedCache, IReceiver
    {
        #region Properties

        /// <summary>
        /// Gets the entries collection.
        /// </summary>
        /// <value>
        /// The collection of <see cref="ICacheEntry"/> objects.
        /// </value>
        public virtual ICollection Entries
        {
            get { return ConverterCache.Entries; }
        }

        /// <summary>
        /// Gets the keys collection.
        /// </summary>
        /// <value>
        /// The keys collection.
        /// </value>
        public virtual ICollection Keys
        {
            get { return ConverterCache.Keys; }
        }

        /// <summary>
        /// Gets the values collection.
        /// </summary>
        /// <value>
        /// The values collection.
        /// </value>
        public virtual ICollection Values
        {
            get { return ConverterCache.Values; }
        }

        /// <summary>
        /// The <see cref="IProtocol"/> understood by the IReceiver.
        /// </summary>
        /// <remarks>
        /// Only <b>IChannel</b> objects with the specified <b>IProtocol</b>
        /// can be registered with this IReceiver.
        /// </remarks>
        /// <value>
        /// The <b>IProtocol</b> used by this IReceiver.
        /// </value>
        public virtual IProtocol Protocol
        {
            get { return NamedCacheProtocol.Instance; }
        }

        /// <summary>
        /// The <see cref="RemoteCacheService"/> that created this
        /// RemoteNamedCache.
        /// </summary>
        /// <value>
        /// The <b>RemoteCacheService</b> that created this
        /// <b>RemoteNamedCache</b>.
        /// </value>
        public virtual ICacheService CacheService { get; internal set; }

        /// <summary>
        /// The name of this <see cref="IReceiver"/>.
        /// </summary>
        /// <remarks>
        /// If the <b>IReceiver</b> is registered with a
        /// <see cref="IConnectionManager"/>, the registration and any
        /// subsequent accesses are by the IReceiver's name, meaning that
        /// the name must be unique within the domain of the
        /// <b>IConnectionManager</b>.
        /// </remarks>
        /// <value>
        /// The <b>IReceiver</b> name.
        /// </value>
        public virtual string Name
        {
            get
            {
                return "RemoteNamedCache(Cache=" + CacheName + ")";
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IChannel"/> used to exchange
        /// NamedCache Protocol Messages with a remote ProxyService.
        /// </summary>
        /// <value>
        /// The <b>IChannel</b> used for NamedCache protocol messages.
        /// </value>
        public virtual IChannel Channel
        {
            get { return m_channel; }
            set { m_channel = value; }
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
            get { return BinaryCache.Count; }
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
        /// The <see cref="Contains"/> operation may be used to distinguish
        /// these two cases.</p>
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
        public virtual object this[object key]
        {
            get { return ConverterCache[key]; }
            set { Add(key, value); }
        }

        /// <summary>
        /// The <see cref="QueueProcessor"/> used to dispatch
        /// <see cref="CacheEventArgs"/>.
        /// </summary>
        /// <value>
        /// The <b>QueueProcessor</b> object.
        /// </value>
        public virtual QueueProcessor EventDispatcher
        {
            get { return m_eventDispatcher; }
            set
            {
                m_eventDispatcher = value;

                BinaryCache.EventDispatcher = value;
            }
        }

        /// <summary>
        /// Child <see cref="BinaryNamedCache"/> instance.
        /// </summary>
        /// <value>
        /// Child <b>BinaryNamedCache</b> instance.
        /// </value>
        public virtual BinaryNamedCache BinaryCache { get; set; }

        /// <summary>
        /// The client view of the <see cref="BinaryCache"/>.
        /// </summary>
        /// <value>
        /// <b>INamedCache</b> instance.
        /// </value>
        public virtual INamedCache ConverterCache { get; set; }

        /// <summary>
        /// Child <see cref="ConverterFromBinary"/> instance.
        /// </summary>
        /// <value>
        /// Child <b>ConverterFromBinary</b> instance.
        /// </value>
        public virtual ConverterFromBinary FromBinaryConverter { get; set; }

        /// <summary>
        /// Child <see cref="ConverterKeyToBinary"/> instance.
        /// </summary>
        /// <value>
        /// Child <b>ConverterKeyToBinary</b> instance.
        /// </value>
        public virtual ConverterKeyToBinary KeyToBinaryConverter { get; set; }

        /// <summary>
        /// Child <see cref="ConverterValueToBinary"/> instance.
        /// </summary>
        /// <value>
        /// Child <b>ConverterValueToBinary</b> instance.
        /// </value>
        public virtual ConverterValueToBinary ValueToBinaryConverter { get; set; }

        /// <summary>
        /// Holder for listeners such as NamedCacheDeactivationListeners.
        /// Utilized for implementing cache destroy/release calls across multiple nodes.
        /// </summary>
        /// <value>
        /// Holder for listeners such as NamedCacheDeactivationListeners.
        /// Utilized for implementing cache destroy/release calls across multiple nodes.
        /// </value>
        public virtual Listeners DeactivationListeners
        {
            get { return m_deactivationListeners; }
            set { m_deactivationListeners = value; }
        }

        /// <summary>
        /// Whether a key should be checked for <see cref="IKeyAssociation"/>
        /// by the extend client (false) or deferred until the key is
        /// received by the PartionedService (true).
        /// </summary>
        /// <value>
        /// Whether a key should be checked for <b>IKeyAssociation</b> by the
        /// extend client (false) or deferred until the key is received by
        /// the PartionedService (true).
        /// </value>
        public virtual Boolean DeferKeyAssociationCheck { get; set; }

        /// <summary>
        /// Whether we have warned user about the deprecated lock API.
        /// </summary>
        /// <value>
        /// A value of true indicates we have warned user about the deprecated
        /// lock API; false, otherwise.
        /// </value>
        public static bool LockDeprecateWarned { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RemoteNamedCache()
        {
            DeactivationListeners = new Listeners();
            OnInit();
        }

        #endregion

        #region ICollection and IDictionary implementation

        /// <summary>
        /// Gets a value indicating whether the <b>IDictionary</b> object is
        /// read-only.
        /// </summary>
        /// <value>
        /// Always <b>false</b> for RemoteNamedCache.
        /// </value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the <b>IDictionary</b> object has
        /// a fixed size.
        /// </summary>
        /// <value>
        /// Always <b>false</b> for RemoteNamedCache.
        /// </value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Copies the elements of the <b>ICollection</b> to an <b>Array</b>,
        /// starting at a particular index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <b>Array</b> that is the destination of the
        /// elements copied from <b>ICollection</b>.
        /// </param>
        /// <param name="index">
        /// The zero-based index in array at which copying begins.
        /// </param>
        public virtual void CopyTo(Array array, int index)
        {
            ConverterCache.CopyTo(array, index);
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
        /// Always <b>true</b> for RemoteNamedCache.
        /// </value>
        public virtual bool IsSynchronized
        {
            get { return true; }
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
            return GetEnumerator();
        }

        /// <summary>
        /// Adds an element with the provided key and value to the
        /// <see cref="IDictionary"/> object.
        /// </summary>
        /// <param name="value">
        /// The <see cref="Object"/> to use as the value of the element to
        /// add.
        /// </param>
        /// <param name="key">
        /// The <see cref="Object"/> to use as the key of the element to add.
        /// </param>
        public virtual void Add(object key, object value)
        {
            ConverterCache.Add(key, value);
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
            return GetEnumerator();
        }

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
        public virtual void Clear()
        {
            BinaryCache.Clear();
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
            ConverterCache.Remove(key);
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
        public virtual bool Contains(object key)
        {
            return ConverterCache.Contains(key);
        }

        #endregion

        #region ICache implementation

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
            return ConverterCache.Insert(key, value, CacheExpiration.DEFAULT);
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
            return ConverterCache.Insert(key, value, millis);
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
            ConverterCache.InsertAll(dictionary);
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
        /// A collection of keys that may be in the named cache.
        /// </param>
        /// <returns>
        /// A dictionary of keys to values for the specified keys passed in
        /// <paramref name="keys"/>.
        /// </returns>
        public virtual IDictionary GetAll(ICollection keys)
        {
            return ConverterCache.GetAll(keys);
        }

        /// <summary>
        /// Returns an <see cref="ICacheEnumerator"/> object for this
        /// <b>ICache</b> object.
        /// </summary>
        /// <returns>
        /// An <b>ICacheEnumerator</b> object for this <b>ICache</b>
        /// object.
        /// </returns>
        public virtual ICacheEnumerator GetEnumerator()
        {
            return ConverterCache.GetEnumerator();
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
            CacheService.ReleaseCache(this);
        }

        /// <summary>
        /// Specifies whether or not the <see cref="INamedCache"/> is active.
        /// </summary>
        /// <value>
        /// <b>true</b> if the INamedCache is active; <b>false</b> otherwise.
        /// </value>
        public virtual bool IsActive
        {
            get { return BinaryCache.IsActive ; }
        }

        /// <summary>
        /// The name of the <see cref="INamedCache"/> represented by
        /// this RemoteNamedCache.
        /// </summary>
        /// <value>
        /// The name of the <see cref="INamedCache"/>.
        /// </value>
        public virtual string CacheName { get; set; }

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
            CacheService.DestroyCache(this);
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
            BinaryCache.Truncate();
        }

        /// <summary>
        /// Construct a view of this INamedCache.
        /// </summary>
        /// <returns>A local view for this INamedCache</returns>
        /// <see cref="ViewBuilder"/>
        /// <since>12.2.1.4</since>
        public virtual ViewBuilder View()
        {
            throw new InvalidOperationException();
        }

        #endregion

        #region IQueryCache implementation

        /// <summary>
        /// Remove an index from this IQueryCache.
        /// </summary>
        /// <param name="extractor">
        /// The <see cref="IValueExtractor"/> object that is used to extract
        /// an indexable object from a value stored in the cache.
        /// </param>
        public virtual void RemoveIndex(IValueExtractor extractor)
        {
            BinaryCache.RemoveIndex(extractor);
        }

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
            return ConverterCache.GetKeys(filter);
        }

        /// <summary>
        /// Return a collection of the values contained in this cache
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
        /// The <b>IComparer</b> object which imposes an ordering on values
        /// in the resulting collection; or <c>null</c> if the entries'
        /// values natural ordering should be used.
        /// </param>
        /// <returns>
        /// A collection of values that satisfy the specified criteria.
        /// </returns>
        public virtual object[] GetValues(IFilter filter, IComparer comparer)
        {
            object[] values = ConverterCache.GetValues(filter);

            if (values.Length > 0)
            {
                Array.Sort(values, new SafeComparer(comparer));
            }
            return values;
        }

        /// <summary>
        /// Return a collection of the values contained in this cache
        /// that satisfy the criteria expressed by the filter.
        /// </summary>
        /// <param name="filter">
        /// The <see cref="IFilter"/> object representing the criteria that
        /// the entries of this cache should satisfy.
        /// </param>
        /// <returns>
        /// A collection of values that satisfy the specified criteria.
        /// </returns>
        public virtual object[] GetValues(IFilter filter)
        {
            return ConverterCache.GetValues(filter);
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
        /// The <b>IComparer</b> object which imposes an ordering on entries
        /// in the resulting collection; or <c>null</c> if the entries'
        /// values natural ordering should be used.
        /// </param>
        /// <returns>
        /// An array of entries that satisfy the specified criteria.
        /// </returns>
        public virtual ICacheEntry[] GetEntries(IFilter filter, IComparer comparer)
        {
            if (comparer == null)
            {
                comparer = SafeComparer.Instance;
            }

            // COH-2717
            LimitFilter filterLimit = null;
            if (filter is LimitFilter)
            {
                filterLimit          = (LimitFilter) filter;
                filterLimit.Comparer = comparer;
            }
    
            ICacheEntry[] entries   = ConverterCache.GetEntries(filter);
            EntryComparer compEntry = new EntryComparer(comparer);

            if (entries.Length > 0)
            {
                Array.Sort(entries, compEntry);
            }

            if (filterLimit == null)
            {
                return entries;
            }
            else
            {
                filterLimit.Comparer = compEntry;
                object[] retSet      = filterLimit.ExtractPage(entries);
                filterLimit.Comparer = comparer;

                ICacheEntry[] entrySet = new ICacheEntry[retSet.Length];
                for (int i = 0; i < retSet.Length; i++)
                {
                    entrySet[i] = (ICacheEntry) retSet[i];
                }  
                return entrySet;
            }
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
            if (filter is LimitFilter)
            {
                ((LimitFilter) filter).Comparer = null;
            }

            return ConverterCache.GetEntries(filter);
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
            BinaryCache.AddIndex(extractor, isOrdered, comparer);
        }

        #endregion

        #region IObservableCache implementation

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
            if (listener is INamedCacheDeactivationListener)
            {
                DeactivationListeners.Remove(listener);
            }
            else
            {
                if (filter is InKeySetFilter)
                {
                    ((InKeySetFilter) filter).EnsureConverted(KeyToBinaryConverter);
                }
                BinaryCache.RemoveCacheListener(InstantiateConverterListener(listener), filter);
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
            BinaryCache.RemoveCacheListener(InstantiateConverterListener(listener),
                KeyToBinaryConverter.Convert(key));
        }

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
            if (listener is INamedCacheDeactivationListener)
            {
                DeactivationListeners.Add(listener);
            }
            else
            {
                if (filter is InKeySetFilter)
                {
                    ((InKeySetFilter) filter).EnsureConverted(KeyToBinaryConverter);
                }
                BinaryCache.AddCacheListener(InstantiateConverterListener(listener), filter, isLite);
            }
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
            BinaryCache.AddCacheListener(InstantiateConverterListener(listener),
                KeyToBinaryConverter.Convert(key), isLite);
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
            return ConverterCache.Invoke(key, agent);
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
            return ConverterCache.InvokeAll(filter, agent);
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
            return ConverterCache.InvokeAll(keys, agent);
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
            return ConverterCache.Aggregate(filter, agent);
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
            return ConverterCache.Aggregate(keys, agent);
        }

        #endregion

        #region IConcurrentCache implementation

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
        /// <exception cref="NotSupportedException">
        /// If <see cref="LockScope.LOCK_ALL"/> is passed
        /// as <i>key</i> parameter.
        /// </exception>
        [Obsolete("Obsolete as of Coherence 12.1")]
        public virtual bool Unlock(object key)
        {
            PrintLockDeprecatedMessage();
            if (key == LockScope.LOCK_ALL)
            {
                throw new NotSupportedException("RemoteNamedCache does not support LOCK_ALL");
            }

            return ConverterCache.Unlock(key);
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
        /// <exception cref="NotSupportedException">
        /// If <see cref="LockScope.LOCK_ALL"/> is passed
        /// as <i>key</i> parameter.
        /// </exception>
        [Obsolete("Obsolete as of Coherence 12.1")]
        public virtual bool Lock(object key)
        {
            return Lock(key, 0);
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
        /// <exception cref="NotSupportedException">
        /// If <see cref="LockScope.LOCK_ALL"/> is passed
        /// as <i>key</i> parameter.
        /// </exception>
        [Obsolete("Obsolete as of Coherence 12.1")]
        public virtual bool Lock(object key, long waitTimeMillis)
        {
            PrintLockDeprecatedMessage();
            if (key == LockScope.LOCK_ALL)
            {
                throw new NotSupportedException("RemoteNamedCache does not support LOCK_ALL");
            }

            return ConverterCache.Lock(key, waitTimeMillis);
        }

        #endregion

        #region IReceiver implementation

        //CLOVER:OFF
        //server side methods

        /// <summary>
        /// Unregister the given <b>IChannel</b> with this IReceiver.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method is invoked by the <b>IChannel</b> when an IReceiver is
        /// disassociated with the <b>IChannel</b>.</p>
        /// <p>
        /// Once unregistered, the IReceiver will no longer receive
        /// unsolicited <b>IMessage</b> objects sent through the
        /// <b>IChannel</b>.</p>
        /// </remarks>
        /// <param name="channel">
        /// An <b>IChannel</b> that was disassociated with this IReceiver.
        /// </param>
        public virtual void UnregisterChannel(IChannel channel)
        {
            m_channel = null;
        }

        /// <summary>
        /// Notify this IReceiver that it has been associated with a
        /// <b>IChannel</b>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method is invoked by the <b>IChannel</b> when an IReceiver is
        /// associated with the <b>IChannel</b>.</p>
        /// <p>
        /// Once registered, the IReceiver will receive all unsolicited
        /// <b>IMessage</b> objects sent through the <b>IChannel</b> until
        /// the <b>IChannel</b> is unregistered or closed. Without a
        /// IReceiver, the unsolicited <b>IMessage</b> objects are executed
        /// with only an <b>IChannel</b> as context; with an IReceiver, the
        /// IReceiver is given the <b>IMessage</b> to process, and may
        /// execute the <b>IMessage</b> in turn.</p>
        /// </remarks>
        /// <param name="channel">
        /// An <b>IChannel</b> that has been associated with this IReceiver.
        /// </param>
        public virtual void RegisterChannel(IChannel channel)
        {
            m_channel           = channel;
            BinaryCache.Channel = channel;

            ISerializer            serializer = channel.Serializer;
            FromBinaryConverter.Serializer    = serializer;
            ValueToBinaryConverter.Serializer = serializer;
            if (serializer is ConfigurablePofContext)
            {
                var cpc = (ConfigurablePofContext)serializer;
                if (cpc.IsReferenceEnabled)
                {
                    cpc = new ConfigurablePofContext(cpc);
                    cpc.IsReferenceEnabled = false;
                    serializer = cpc;
                }
            }
            KeyToBinaryConverter.Serializer = serializer;
        }

        /// <summary>
        /// Called when an unsolicited (non-Response) <b>IMessage</b> is
        /// received by an <b>IChannel</b> that had been previously
        /// registered with this IReceiver.
        /// </summary>
        /// <param name="message">
        /// An unsolicited <b>IMessage</b> received by a registered
        /// <b>IChannel</b>.
        /// </param>
        public virtual void OnMessage(IMessage message)
        {
            message.Run();
        }

        /// <summary>
        /// Notify this IReceiver that the <b>IChannel</b> it was associated with has
        /// been closed.
        /// </summary>
        /// <param name="channel">
        /// An <b>IChannel</b> that was associated with this IReceiver.
        /// </param>
        /// <since>12.2.1.2.0</since>
        public virtual void OnChannelClosed(IChannel channel)
        {
            Listeners listeners = DeactivationListeners;
            if (!listeners.IsEmpty)
            {
                CacheEventArgs evtRoute = new CacheEventArgs(this, CacheEventType.Deleted, null, null, null, true);
                // dispatch the event to the listeners, which are all synchronous (hence the null Queue)
                RunnableCacheEvent.DispatchSafe(evtRoute, listeners, null /*Queue*/);
            }
        }

        //CLOVER:ON
        #endregion

        #region Extend override methods

        /// <summary>
        /// Return a human-readable description of this class.
        /// </summary>
        /// <returns>
        /// A string representation of this class.
        /// </returns>
        /// <since>12.2.1.3</since>
        protected override string GetDescription()
        {
            return "NamedCache=" + CacheName
                + ", Service=" + CacheService.Info.ServiceName;
        }

        #endregion

        #region RemoteNamedCache methods

        /// <summary>
        /// Initialization method.
        /// </summary>
        protected virtual void OnInit()
        {
            BinaryCache            = new BinaryNamedCache(this);
            FromBinaryConverter    = new ConverterFromBinary();
            KeyToBinaryConverter   = new ConverterKeyToBinary(this);
            ValueToBinaryConverter = new ConverterValueToBinary();

            ConverterCache = ConverterCollections.GetNamedCache(BinaryCache,
                    FromBinaryConverter,
                    KeyToBinaryConverter,
                    FromBinaryConverter,
                    ValueToBinaryConverter);
        }

        /// <summary>
        /// Instantiate and configure a new <see cref="ConverterListener"/>
        /// for the given <see cref="ICacheListener"/>.
        /// </summary>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to wrap.
        /// </param>
        /// <returns>
        /// A new <b>ConverterListener</b> that wraps the given
        /// <b>ICacheListener</b>.
        /// </returns>
        protected virtual ICacheListener InstantiateConverterListener(ICacheListener listener)
        {
            if (listener is CacheTriggerListener)
            {
                return listener;
            }

            ConverterListener listenerConv = new ConverterListener();
            listenerConv.Converter  = FromBinaryConverter;
            listenerConv.Listener   = listener;
            listenerConv.NamedCache = this;

            if (listener is CacheListenerSupport.IPrimingListener)
            {
                return new CacheListenerSupport.WrapperPrimingListener(listenerConv);
            }
            else
            {
                return listener is CacheListenerSupport.ISynchronousListener
                    ? new CacheListenerSupport.WrapperSynchronousListener(listenerConv)
                    : (ICacheListener)listenerConv;
            }
        }

        /// <summary>
        /// Print only once a warning message for deprecated lock API.
        /// </summary>
        public void PrintLockDeprecatedMessage()
        {
            if (!LockDeprecateWarned)
            {
                CacheFactory.Log("Using the Lock API from a Coherence*Extend client is deprecated and will be removed in a future release", CacheFactory.LogLevel.Warn);
                LockDeprecateWarned = true;
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

        #region Data members

        /// <summary>
        /// The Channel used to exchange NamedCache Protocol Messages with
        /// a remote ProxyService.
        /// </summary>
        private IChannel m_channel;

        /// <summary>
        /// The holder for listeners such as INamedCacheDeactivationListener
        /// </summary>
        private Listeners m_deactivationListeners;

        /// <summary>
        /// The QueueProcessor used to dispatch cache events.
        /// </summary>
        [NonSerialized]
        private QueueProcessor m_eventDispatcher;

        #endregion

        #region Inner class: ConverterBinaryToDecoratedBinary

        /// <summary>
        /// <see cref="IConverter"/> implementation that deserializes a
        /// <see cref="Binary"/> object using the
        /// <see cref="RemoteNamedCache"/> <see cref="IChannel"/>'s
        /// serializer and decorates the <see cref="Binary"/> using the
        /// associated key.
        /// </summary>
        public class ConverterBinaryToDecoratedBinary : IConverter
        {
            #region Properties

            /// <summary>
            /// Gets the <see cref="RemoteNamedCache"/> that created this
            /// ConverterBinaryToDecoratedBinary.
            /// </summary>
            public virtual RemoteNamedCache RemoteNamedCache
            {
                get { return m_cache; }
            }

            /// <summary>
            /// <see cref="ISerializer"/> instance used to convert objects
            /// from <see cref="Binary"/>.
            /// </summary>
            /// <value>
            /// <b>ISerializer</b> instance used to convert objects from
            /// <b>Binary</b>.
            /// </value>
            public virtual ISerializer Serializer { get; set; }

            #endregion

            #region Constructors

            /// <summary>
            /// Default constructor.
            /// </summary>
            public ConverterBinaryToDecoratedBinary(RemoteNamedCache cache)
            {
                m_cache = cache;
            }

            #endregion

            #region IConverter implementation

            /// <summary>
            /// Convert the passed object to another object.
            /// </summary>
            /// <param name="o">
            /// Object to be converted.
            /// </param>
            /// <returns>
            /// The new, converted object.
            /// </returns>
            public object Convert(object o)
            {
                if (o == null || RemoteNamedCache.DeferKeyAssociationCheck)
                {
                    return o;
                }

                Binary bin = (Binary) o;

                if (SerializationHelper.IsIntDecorated(bin))
                {
                    return bin;
                }

                Binary binDeco = bin;

                o = SerializationHelper.FromBinary(bin, Serializer);
                if (o is IKeyAssociation)
                {
                    o = ((IKeyAssociation) o).AssociatedKey;
                    if (o != null)
                    {
                        binDeco = SerializationHelper.ToBinary(o, Serializer);
                    }
                }

                return SerializationHelper.DecorateBinary(bin, binDeco.CalculateNaturalPartition(0));
            }

            #endregion

            #region Data members

            /// <summary>
            /// The <see cref="RemoteNamedCache"/> that created this
            /// ConverterBinaryToDecoratedBinary.
            /// </summary>
            private readonly RemoteNamedCache m_cache;

            #endregion
        }

        #endregion

        #region Inner class: ConverterBinaryToUndecoratedBinary

        /// <summary>
        /// <see cref="IConverter"/> implementation that removes
        /// an int decoration from a <see cref="Binary"/> if present.
        /// </summary>
        public class ConverterBinaryToUndecoratedBinary : IConverter
        {
            #region Properties

            /// <summary>
            /// Gets the <see cref="RemoteNamedCache"/> that created this
            /// ConverterBinaryToUndecoratedBinary.
            /// </summary>
            public virtual RemoteNamedCache RemoteNamedCache
            {
                get { return m_cache; }
            }

            /// <summary>
            /// <see cref="ISerializer"/> instance used to convert objects
            /// from <see cref="Binary"/>.
            /// </summary>
            /// <value>
            /// <b>ISerializer</b> instance used to convert objects from
            /// <b>Binary</b>.
            /// </value>
            public virtual ISerializer Serializer { get; set; }

            #endregion

            #region Constructors

            /// <summary>
            /// Default constructor.
            /// </summary>
            public ConverterBinaryToUndecoratedBinary(RemoteNamedCache cache)
            {
                m_cache = cache;
            }

            #endregion

            #region IConverter implementation

            /// <summary>
            /// Convert the passed object to another object.
            /// </summary>
            /// <param name="o">
            /// Object to be converted.
            /// </param>
            /// <returns>
            /// The new, converted object.
            /// </returns>
            public object Convert(object o)
            {
                if (o == null || RemoteNamedCache.DeferKeyAssociationCheck)
                {
                    return o;
                }

                Binary bin = (Binary) o;

                return SerializationHelper.IsIntDecorated(bin)
                    ? SerializationHelper.RemoveIntDecoration(bin)
                    : bin;
            }

            #endregion

            #region Data members

            /// <summary>
            /// The <see cref="RemoteNamedCache"/> that created this
            /// ConverterBinaryToUndecoratedBinary.
            /// </summary>
            private readonly RemoteNamedCache m_cache;

            #endregion
        }

        #endregion

        #region Inner class: ConverterFromBinary

        /// <summary>
        /// <see cref="IConverter"/> implementation that converts objects
        /// from a <see cref="Binary"/> representation via the
        /// <see cref="RemoteNamedCache.Channel"/>'s serializer.
        /// </summary>
        public class ConverterFromBinary : IConverter
        {
            #region Properties

            /// <summary>
            /// <see cref="ISerializer"/> instance used to convert objects
            /// from <see cref="Binary"/>.
            /// </summary>
            /// <value>
            /// <b>ISerializer</b> instance used to convert objects from
            /// <b>Binary</b>.
            /// </value>
            public virtual ISerializer Serializer { get; set; }

            #endregion

            #region IConverter implementation

            /// <summary>
            /// Convert the passed object to another object.
            /// </summary>
            /// <param name="o">
            /// Object to be converted.
            /// </param>
            /// <returns>
            /// The new, converted object.
            /// </returns>
            public object Convert(object o)
            {
                Binary bin = (Binary) o;
                return bin == null
                        ? null
                        : SerializationHelper.FromBinary(bin, Serializer);
            }

            #endregion
        }

        #endregion

        #region Inner class: ConverterKeyToBinary

        /// <summary>
        /// <see cref="IConverter"/> implementation that converts keys into
        /// their <see cref="Binary"/> representation via the
        /// <see cref="RemoteNamedCache"/> <see cref="IChannel"/>'s
        /// serializer.
        /// </summary>
        public class ConverterKeyToBinary : IConverter
        {
            #region Properties

            /// <summary>
            /// Gets the <see cref="RemoteNamedCache"/> that created this
            /// ConverterKeyToBinary.
            /// </summary>
            public virtual RemoteNamedCache RemoteNamedCache
            {
                get { return m_cache; }
            }

            /// <summary>
            /// <see cref="ISerializer"/> instance used to convert objects
            /// from <see cref="Binary"/>.
            /// </summary>
            /// <value>
            /// <b>ISerializer</b> instance used to convert objects from
            /// <b>Binary</b>.
            /// </value>
            public virtual ISerializer Serializer { get; set; }

            #endregion

            #region Constructors

            /// <summary>
            /// Default constructor.
            /// </summary>
            public ConverterKeyToBinary(RemoteNamedCache cache)
            {
                m_cache = cache;
            }

            #endregion

            #region IConverter implementation

            /// <summary>
            /// Convert the passed object to another object.
            /// </summary>
            /// <param name="o">
            /// Object to be converted.
            /// </param>
            /// <returns>
            /// The new, converted object.
            /// </returns>
            public object Convert(object o)
            {
                Binary bin = SerializationHelper.ToBinary(o, Serializer);
                if (RemoteNamedCache.DeferKeyAssociationCheck)
                {
                    return bin;
                }

                Binary binDeco = bin;
                if (o is IKeyAssociation)
                {
                    o = ((IKeyAssociation) o).AssociatedKey;
                    if (o != null)
                    {
                        binDeco = SerializationHelper.ToBinary(o, Serializer);
                    }
                }

                return SerializationHelper.DecorateBinary(bin, binDeco.CalculateNaturalPartition(0));
            }

            #endregion

            #region Data members

            /// <summary>
            /// The <see cref="RemoteNamedCache"/> that created this
            /// ConverterKeyToBinary.
            /// </summary>
            private readonly RemoteNamedCache m_cache;

            #endregion
        }

        #endregion

        #region Inner class: ConverterValueToBinary

        /// <summary>
        /// <see cref="IConverter"/> implementation that converts objects to
        /// a <see cref="Binary"/> representation via the
        /// <see cref="RemoteNamedCache.Channel"/>'s serializer.
        /// </summary>
        public class ConverterValueToBinary : IConverter
        {
            #region Properties

            /// <summary>
            /// <see cref="ISerializer"/> instance used to convert objects
            /// from <see cref="Binary"/>.
            /// </summary>
            /// <value>
            /// <b>ISerializer</b> instance used to convert objects from
            /// <b>Binary</b>.
            /// </value>
            public virtual ISerializer Serializer { get; set; }

            #endregion

            #region IConverter implementation

            /// <summary>
            /// Convert the passed object to another object.
            /// </summary>
            /// <param name="o">
            /// Object to be converted.
            /// </param>
            /// <returns>
            /// The new, converted object.
            /// </returns>
            public object Convert(object o)
            {
                return SerializationHelper.ToBinary(o, Serializer);
            }

            #endregion
        }

        #endregion

        #region Inner class: BinaryNamedCache

        /// <summary>
        /// The internal view of the <see cref="RemoteNamedCache"/>.
        /// </summary>
        public class BinaryNamedCache : INamedCache
        {
            #region Properties

            /// <summary>
            /// Gets the <see cref="RemoteNamedCache"/> that created this
            /// BinaryNamedCache.
            /// </summary>
            public virtual RemoteNamedCache RemoteNamedCache
            {
                get { return m_cache; }
            }

            /// <summary>
            /// Gets or sets the <see cref="IChannel"/> used to exchange
            /// NamedCache Protocol messages with a remote ProxyService.
            /// </summary>
            /// <value>
            /// The <b>IChannel</b> used for NamedCache protocol messages.
            /// </value>
            public virtual IChannel Channel
            {
                get { return m_channel; }
                set
                {
                    m_channel = value;

                    BinaryToDecoratedBinaryConverter.Serializer = value.Serializer;
                }
            }

            /// <summary>
            /// <see cref="ConverterBinaryToDecoratedBinary"/> instance.
            /// </summary>
            /// <value>
            /// <b>ConverterBinaryToDecoratedBinary</b> instance.
            /// </value>
            public virtual ConverterBinaryToDecoratedBinary
                BinaryToDecoratedBinaryConverter { get; set; }

            /// <summary>
            /// <see cref="ConverterBinaryToUndecoratedBinary"/> instance.
            /// </summary>
            /// <value>
            /// <b>ConverterBinaryToUndecoratedBinary</b> instance.
            /// </value>
            public virtual ConverterBinaryToUndecoratedBinary
                BinaryToUndecoratedBinaryConverter { get; set; }

            /// <summary>
            /// The <see cref="QueueProcessor"/> used to dispatch
            /// <see cref="CacheEventArgs"/>.
            /// </summary>
            /// <value>
            /// The <b>QueueProcessor</b> object.
            /// </value>
            public virtual QueueProcessor EventDispatcher
            {
                get { return m_eventDispatcher; }
                set { m_eventDispatcher = value; }
            }

            /// <summary>
            /// Gets or sets an <see cref="ILongArray"/> of
            /// <see cref="IFilter"/> objects indexed by the unique filter
            /// id.
            /// </summary>
            /// <remarks>
            /// These filter id values are used by the
            /// <see cref="CacheEventArgs"/> message to specify what filters
            /// caused a cache event.
            /// Note: all access (for update) to this array should be
            /// synchronized on the CacheListenerSupport object.
            /// </remarks>
            /// <value>
            /// <b>ILongArray</b> of filters.
            /// </value>
            protected virtual ILongArray FilterArray
            {
                get { return m_filterArray; }
                set { m_filterArray = value; }
            }

            /// <summary>
            /// <see cref="CacheListenerSupport"/> used by this
            /// <see cref="INamedCache"/> to dispatch
            /// <see cref="CacheEventArgs"/>s to registered
            /// <see cref="ICacheListener"/>s.
            /// </summary>
            /// <value>
            /// <b>CacheListenerSupport</b> instance.
            /// </value>
            protected virtual CacheListenerSupport CacheListenerSupport
            {
                get { return m_cacheListenerSupport; }
                set { m_cacheListenerSupport = value; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Default constructor.
            /// </summary>
            public BinaryNamedCache(RemoteNamedCache cache)
            {
                m_cache = cache;
                OnInit();
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Initialization method.
            /// </summary>
            protected virtual void OnInit()
            {
                BinaryToDecoratedBinaryConverter   =
                        new ConverterBinaryToDecoratedBinary(RemoteNamedCache);
                BinaryToUndecoratedBinaryConverter =
                        new ConverterBinaryToUndecoratedBinary(RemoteNamedCache);

                m_filterArray          = new LongSortedList();
                m_cacheListenerSupport = new CacheListenerSupport();
                m_entries              = new RemoteCollection(this, RemoteCollectionType.Entries);
                m_keys                 = new RemoteCollection(this, RemoteCollectionType.Keys);
                m_values               = new RemoteCollection(this, RemoteCollectionType.Values);
            }

            /// <summary>
            /// Return the result associated with the given
            /// <see cref="IResponse"/>.
            /// </summary>
            /// <param name="response">
            /// The <b>IResponse</b> to process.
            /// </param>
            /// <returns>
            /// The result associated with the given <b>IResponse</b>.
            /// </returns>
            /// <exception cref="Exception">
            /// If the response was a failure.
            /// </exception>
            protected virtual object ProcessResponse(IResponse response)
            {
                if (response.IsFailure)
                {
                    object result = response.Result;
                    if (result is Exception)
                    {
                        throw (Exception) result;
                    }
                    throw new Exception("received error: " + result);
                }
                return response.Result;
            }

            /// <summary>
            /// Perform a remote query.
            /// </summary>
            /// <param name="filter">
            /// The <see cref="IFilter"/> used in the query.
            /// </param>
            /// <param name="keysOnly">
            /// If <b>true</b>, only the keys from the result will be
            /// returned; otherwise, the entries will be returned.
            /// </param>
            /// <returns>
            /// The result of the query.
            /// </returns>
            protected virtual ICollection Query(IFilter filter, bool keysOnly)
            {
                IChannel        channel   = EnsureChannel();
                IMessageFactory factory   = channel.MessageFactory;
                Binary          binCookie = null;
                ICollection     colResult = null;
                IList           listPart  = null;

                do
                {
                    QueryRequest request = (QueryRequest) factory.CreateMessage(QueryRequest.TYPE_ID);

                    request.Cookie   = binCookie;
                    request.Filter   = filter;
                    request.KeysOnly = keysOnly;
                    if (filter is LimitFilter)
                    {
                        request.FilterCookie = ((LimitFilter) filter).Cookie;
                    }

                    IStatus         status   = channel.Send(request);
                    PartialResponse response = (PartialResponse) status.WaitForResponse();
                    ICollection     col      = (ICollection) ProcessResponse(response);

                    if (colResult == null || colResult.Count == 0)
                    {
                        // first non-empty result set
                        colResult = col;
                    }
                    else if (col == null || col.Count == 0)
                    {
                        // empty result set; nothing to do
                    }
                    else
                    {
                        // additional non-empty result set
                        if (listPart == null)
                        {
                            // start recording each result set
                            listPart = new ArrayList
                                           {
                                                   colResult is Object[]
                                                           ? colResult
                                                           : CollectionUtils.
                                                                     ToArray(
                                                                     colResult)
                                           };
                        }
                        listPart.Add(col is Object[]
                            ? col
                            : CollectionUtils.ToArray(col));
                    }

                    if (filter is LimitFilter)
                    {
                        LimitFilter filterLimit     = (LimitFilter) filter;
                        NamedCachePartialResponse 
                                    partialResponse = (NamedCachePartialResponse) response;
                        LimitFilter filterReturned  = (LimitFilter) partialResponse.Filter;

                        // update LimitFilter with the state of the returned
                        // LimitFilter/cookie
                        filterLimit.BottomAnchor = filterReturned.BottomAnchor;
                        filterLimit.TopAnchor    = filterReturned.TopAnchor;
                        filterLimit.Cookie       = partialResponse.FilterCookie;
                    }
                    else
                    {
                        binCookie = response.Cookie;
                    }
                }
                while (binCookie != null);

                ICollection colReturn;
                if (listPart == null)
                {
                    colReturn = colResult;
                }
                else
                {
                    Object[][] aao = new object[listPart.Count][];
                    listPart.CopyTo(aao, 0);
                    colReturn = new ImmutableMultiList(aao);
                }

                return colReturn == null
                    ? colReturn
                    : ConverterCollections.GetCollection(colReturn,
                                NullImplementation.GetConverter(),
                                BinaryToUndecoratedBinaryConverter);
            }

            /// <summary>
            /// Send a request to the remote NamedCacheProxy to register a
            /// <see cref="ICacheListener"/> on behalf of this
            /// <b>INamedCache</b>.
            /// </summary>
            /// <param name="filter">
            /// The <see cref="IFilter"/> used to register the remote
            /// <b>ICacheListener</b>
            /// </param>
            /// <param name="filterId">
            /// The unique positive identifier for the specified
            /// <see cref="IFilter"/>.
            /// </param>
            /// <param name="isLite">
            /// If the remote <b>ICacheListener</b> should be "lite".
            /// </param>
            /// <param name="trigger">
            /// The optional <see cref="ICacheTrigger"/> to associate with
            /// the request 
            /// </param>
            /// <param name="isPriming">
            /// If the remote <b>ICacheListener</b> is "priming".
            /// </param>
            protected virtual void AddRemoteCacheListener(IFilter filter, long filterId, bool isLite, ICacheTrigger trigger,
                bool isPriming)
            {
                IChannel              channel = EnsureChannel();
                IMessageFactory       factory = channel.MessageFactory;
                ListenerFilterRequest request = (ListenerFilterRequest) factory.CreateMessage(ListenerFilterRequest.TYPE_ID);

                // COH-4615
                if (request.ImplVersion <= 5 && filter is InKeySetFilter && isLite)
                {
                    throw new NotSupportedException("Priming events are not supported");
                }

                request.Add       = true;
                request.Filter    = filter;
                request.FilterId  = filterId;
                request.IsLite    = isLite;
                request.Trigger   = trigger;
                request.IsPriming = isPriming;

                channel.Request(request);
            }

            /// <summary>
            /// Send a request to the remote NamedCacheProxy to register a
            /// <see cref="ICacheListener"/> on behalf of this
            /// <b>INamedCache</b>.
            /// </summary>
            /// <param name="key">
            /// The key used to register the remote <b>ICacheListener</b>.
            /// </param>
            /// <param name="isLite">
            /// If the remote <b>ICacheListener</b> should be "lite".
            /// </param>
            /// <param name="trigger">
            /// The optional <see cref="ICacheTrigger"/> to associate with
            /// the request 
            /// </param>
            /// <param name="isPriming">
            /// If the remote <b>ICacheListener</b> is "priming".
            /// </param>
            protected virtual void AddRemoteCacheListener(object key, bool isLite, ICacheTrigger trigger, bool isPriming)
            {
                IChannel           channel = EnsureChannel();
                IMessageFactory    factory = channel.MessageFactory;
                ListenerKeyRequest request = (ListenerKeyRequest) factory.CreateMessage(ListenerKeyRequest.TYPE_ID);

                request.Add       = true;
                request.Key       = key;
                request.IsLite    = isLite;
                request.Trigger   = trigger;
                request.IsPriming = isPriming;

                channel.Request(request);
            }

            /// <summary>
            /// Determine if the remote <see cref="INamedCache"/> contains
            /// the specified keys.
            /// </summary>
            /// <param name="keys">
            /// The keys.
            /// </param>
            /// <returns>
            /// <b>true</b> if the <b>INamedCache</b> contains the specified
            /// keys.
            /// </returns>
            public virtual bool ContainsAll(ICollection keys)
            {
                IChannel           channel = EnsureChannel();
                IMessageFactory    factory = channel.MessageFactory;
                ContainsAllRequest request = (ContainsAllRequest) factory.CreateMessage(ContainsAllRequest.TYPE_ID);

                request.Keys = keys;

                return (bool) channel.Request(request);
            }

            /// <summary>
            /// Returns <b>true</b> if this cache maps one or more keys to
            /// the specified value.
            /// </summary>
            /// <remarks>
            /// More formally, returns <b>true</b> if and only if this cache
            /// contains at least one mapping to a value <b>v</b> such that
            /// <b>(value == null ? v == null : value.Equals(v))</b>. This
            /// operation will probably require time linear in the cache size
            /// for most implementations of the IConcurrentCache interface.
            /// </remarks>
            /// <param name="value">
            /// Value whose presence in this map is to be tested.
            /// </param>
            /// <returns>
            /// <b>true</b> if this map maps one or more keys to the
            /// specified value.
            /// </returns>
            public virtual bool ContainsValue(object value)
            {
                IChannel             channel = EnsureChannel();
                IMessageFactory      factory = channel.MessageFactory;
                ContainsValueRequest request = (ContainsValueRequest) factory.CreateMessage(ContainsValueRequest.TYPE_ID);

                request.Value = value;

                return (bool) channel.Request(request);
            }

            /// <summary>
            /// Dispatch a <see cref="CacheEventArgs"/> created using the
            /// supplied information to the cache listeners registered with
            /// this <b>INamedCache</b>.
            /// </summary>
            /// <param name="type">
            /// The type of the <b>CacheEvent</b>, one of
            /// <see cref="CacheEventType"/> values.
            /// </param>
            /// <param name="alFilterIds">
            /// The positive unique identifier(s) of the <b>IFilter</b> that
            /// caused this <b>CacheEvent</b> to be dispatched.
            /// </param>
            /// <param name="key">
            /// The key associated with the <b>CacheEvent</b>.
            /// </param>
            /// <param name="valueOld">
            /// The old value associated with the <b>CacheEvent</b>.
            /// </param>
            /// <param name="valueNew">
            /// The new value associated with the <b>CacheEvent</b>.
            /// </param>
            /// <param name="isSynthetic">
            /// <b>true</b> if the <b>CacheEvent</b> occured because of
            /// internal cache processing.
            /// </param>
            /// <param name="intTransformState">
            /// The transformation state of the event.
            /// </param>
            /// <param name="isPriming">
            /// <b>true</b> if the <b>CacheEvent</b> is a priming event.
            /// </param>
            public virtual void Dispatch(CacheEventType type, long[] alFilterIds, object key, object valueOld, object valueNew, bool isSynthetic, int intTransformState, bool isPriming)
            {
                CacheListenerSupport support  = CacheListenerSupport;
                int                  cFilters = alFilterIds == null ? 0 : alFilterIds.Length;
                CacheEventArgs       evt      = null;

                CacheEventArgs.TransformationState transformState = (CacheEventArgs.TransformationState) intTransformState;

                Listeners listeners = transformState == CacheEventArgs.TransformationState.TRANSFORMED
                        ? null : support.GetListeners(key);

                if (cFilters > 0)
                {
                    ILongArray laFilters   = FilterArray;
                    ArrayList  listFilters = null;
                    lock (support)
                    {
                        for (int i = 0; i < cFilters; i++)
                        {
                            long lFilterId = alFilterIds[i];
                            if (laFilters.Exists(lFilterId))
                            {
                                IFilter filter = (IFilter)laFilters[lFilterId];
                                if (listFilters == null)
                                    {
                                    listFilters = new ArrayList();

                                    // close the key listeners before merging filter listeners
                                    Listeners listenersTemp = new Listeners();
                                    listenersTemp.AddAll(listeners);
                                    listeners = listenersTemp;
                                    }
                                listFilters.Add(filter);
                                listeners.AddAll(support.GetListeners(filter));
                            }
                        }
                    }
                    if (listFilters != null)
                    {
                        IFilter[] aFilters = new IFilter[listFilters.Count];
                        listFilters.CopyTo(aFilters, 0);

                        evt = new FilterEventArgs(this, type, key, valueOld,
                                                        valueNew, isSynthetic,
                                                        transformState, isPriming, aFilters);
                    }
                }

                if (listeners == null || listeners.IsEmpty)
                {
                    // we cannot safely remove the orphaned listener because of the following
                    // race condition: if another thread registers a listener for the same key
                    // or filter associated with the event between the time that this thread
                    // detected the orphaned listener, but before either sends a message to the
                    // server, it is possible for this thread to inadvertantly remove the new
                    // listener
                    //
                    // since it is only possible for synchronous listeners to be leaked (due to
                    // the extra synchronization in the SafeNamedCache), let's err on the side
                    // of leaking a listener than possibily incorrectly removing a listener
                    if (CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Quiet))
                    {
                        if (cFilters > 0)
                        {
                            string msg = new string(("Received an orphaned event; "
                                             + CacheEventArgs.GetDescription(type)
                                             + "filter=[").ToCharArray());
                            for (int i = 0; i < cFilters; i++)
                            {
                                if (i > 0)
                                {
                                    msg = string.Concat(msg, ", ");
                                }
                                IFilter filter = (IFilter) FilterArray[i];
                                msg = string.Concat(msg, filter);
                            }
                            msg = string.Concat(msg, "]");
                            CacheFactory.Log(msg, CacheFactory.LogLevel.Quiet);               
                        }
                        else
                        {
                            CacheFactory.Log("Received an orphaned event; "
                                             + CacheEventArgs.GetDescription(type)
                                             + " key=" + key, CacheFactory.LogLevel.Quiet);
                        }
                    }
                }
                else
                {
                    if (evt == null)
                    {
                        // CacheEvent was sent by a key-based ICacheListener
                        evt = new CacheEventArgs(this, type, key, valueOld, valueNew, isSynthetic, transformState, isPriming);
                    }
                    RunnableCacheEvent.DispatchSafe(evt, listeners, EventDispatcher.Queue);
                }
            }

            /// <summary>
            /// Returns the <see cref="IChannel"/> used by this
            /// <b>INamedCache</b>.
            /// </summary>
            /// <remarks>
            /// If the <b>IChannel</b> is <c>null</c> or
            /// is not open, this method throws an <see cref="Exception"/>.
            /// </remarks>
            /// <returns>
            /// An <b>IChannel</b> that can be used to exchange
            /// <see cref="INamedCache"/> Protocol Messages with a remote
            /// ProxyService.
            /// </returns>
            protected virtual IChannel EnsureChannel()
            {
                IChannel channel = Channel;
                if (channel == null || !channel.IsOpen)
                {
                    string      cause      = "released";
                    IConnection connection = null;

                    if (channel != null)
                    {
                        connection = channel.Connection;
                        if (connection == null || !connection.IsOpen)
                        {
                            cause = "closed";
                        }
                    }

                    throw new ConnectionException(string.Format(
                            "NamedCache \"{0}\" has been {1}", CacheName, cause),
                            connection);
                }

                return channel;
            }

            /// <summary>
            /// Return the unique positivie identifier that the specified
            /// <see cref="IFilter"/> was registered with or 0 if the
            /// specified <b>IFilter</b> has not been registered.
            /// </summary>
            /// <remarks>
            /// Note: all calls to this method should be synchronized using
            /// the <see cref="CacheListenerSupport"/> object.
            /// </remarks>
            /// <param name="filter">
            /// The <b>IFilter</b>.
            /// </param>
            /// <returns>
            /// The unique identifier that the specified <b>IFilter</b> was
            /// registered with.
            /// </returns>
            /// <seealso cref="RegisterFilter"/>
            protected virtual long GetFilterId(IFilter filter)
            {
                foreach (DictionaryEntry entryFilter in FilterArray)
                {
                    if (Equals(filter, entryFilter.Value))
                    {
                        return (long) entryFilter.Key;
                    }
                }

                return 0L;
            }

            /// <summary>
            /// Return the next page of keys.
            /// </summary>
            /// <param name="binCookie">
            /// The optional opaque cookie returned from the last call to
            /// this method.
            /// </param>
            /// <returns>
            /// A <see cref="PartialResponse"/> containing the next set of
            /// keys.
            /// </returns>
            public virtual PartialResponse GetKeysPage(Binary binCookie)
            {
                IChannel        channel = EnsureChannel();
                IMessageFactory factory = channel.MessageFactory;
                QueryRequest    request = (QueryRequest) factory.CreateMessage(QueryRequest.TYPE_ID);

                request.Cookie   = binCookie;
                request.KeysOnly = true;

                IStatus         status   = channel.Send(request);
                PartialResponse response = (PartialResponse) status.WaitForResponse();

                ProcessResponse(response);

                return response;
            }

            /// <summary>
            /// Associate the specified value with the specified key and
            /// expiry delay in the remote <see cref="INamedCache"/>.
            /// </summary>
            /// <param name="key">
            /// The entry key.
            /// </param>
            /// <param name="value">
            /// The entry value.
            /// </param>
            /// <param name="millis">
            /// The entry expiry delay.
            /// </param>
            /// <param name="isReturnRequired">
            /// If <b>true</b>, the old value will be returned.
            /// </param>
            /// <returns>
            /// The old value associated with the given key; only applicable
            /// if <paramref name="isReturnRequired"/> is <b>true</b>.
            /// </returns>
            public virtual object Insert(object key, object value, long millis, bool isReturnRequired)
            {
                IChannel        channel = EnsureChannel();
                IMessageFactory factory = channel.MessageFactory;
                PutRequest      request = (PutRequest) factory.CreateMessage(PutRequest.TYPE_ID);

                request.Key              = key;
                request.Value            = value;
                request.ExpiryDelay      = millis;
                request.IsReturnRequired = isReturnRequired;

                return channel.Request(request);
            }

            /// <summary>
            /// Create a unqiue positive identifier for the specified
            /// <see cref="IFilter"/>.
            /// </summary>
            /// <remarks>
            /// Note: all calls to this method should be synchronized using
            /// the <see cref="CacheListenerSupport"/> object.
            /// </remarks>
            /// <param name="filter">
            /// The <b>IFilter</b>.
            /// </param>
            /// <returns>
            /// The unique identifier that the specified <b>IFilter</b> was
            /// registered with.
            /// </returns>
            /// <seealso cref="GetFilterId"/>
            protected virtual long RegisterFilter(IFilter filter)
            {
                ILongArray laFilter = FilterArray;
                if (laFilter.IsEmpty)
                {
                    laFilter[1] = filter;
                    return 1L;
                }
                return laFilter.Add(filter);
            }

            /// <summary>
            /// Remove the entry with the given key from the remote
            /// <see cref="INamedCache"/>.
            /// </summary>
            /// <param name="key">
            /// The key to remove.
            /// </param>
            /// <param name="isReturnRequired">
            /// If <b>true</b>, the removed value will be returned.
            /// </param>
            /// <returns>
            /// If <paramref name="isReturnRequired"/> is <b>true</b>,
            /// returns the removed value.
            /// </returns>
            public virtual object Remove(object key, bool isReturnRequired)
            {
                IChannel        channel = EnsureChannel();
                IMessageFactory factory = channel.MessageFactory;
                RemoveRequest   request = (RemoveRequest) factory.CreateMessage(RemoveRequest.TYPE_ID);

                request.Key              = key;
                request.IsReturnRequired = isReturnRequired;

                return channel.Request(request);
            }

            /// <summary>
            /// Remove the entries with the specified keys from the remote
            /// <see cref="INamedCache"/>.
            /// </summary>
            /// <param name="keys">
            /// The keys to remove.
            /// </param>
            /// <returns>
            /// <b>true</b> if the INamedCache was modified as a result of
            /// this call.
            /// </returns>
            public virtual bool RemoveAll(ICollection keys)
            {
                IChannel         channel = EnsureChannel();
                IMessageFactory  factory = channel.MessageFactory;
                RemoveAllRequest request = (RemoveAllRequest) factory.CreateMessage(RemoveAllRequest.TYPE_ID);

                request.Keys = keys;

                return (bool) channel.Request(request);
            }

            /// <summary>
            /// Send a request to the remote NamedCacheProxy to unregister a
            /// <see cref="ICacheListener"/> on behalf of this
            /// <b>INamedCache</b>.
            /// </summary>
            /// <param name="filter">
            /// The <see cref="IFilter"/> used to unregister the remote
            /// <b>ICacheListener</b>.
            /// </param>
            /// <param name="filterId">
            /// The unqiue positive identifier for the specified
            /// <b>IFilter</b>.
            /// </param>
            /// <param name="isSync">
            /// If the remote <b>ICacheListener</b> is a
            /// <see cref="Cache.Support.CacheListenerSupport.ISynchronousListener"/>.
            /// </param>
            /// <param name="trigger">
            /// The optional <see cref="ICacheTrigger"/> to associate with
            /// the request.
            /// </param>
            /// <param name="isPriming">
            /// If the remote <b>ICacheListener</b> is "priming".
            /// </param>
            protected virtual void RemoveRemoteCacheListener(IFilter filter, long filterId, bool isSync, ICacheTrigger trigger,
                bool isPriming)
            {
                IChannel              channel = EnsureChannel();
                IMessageFactory       factory = channel.MessageFactory;
                ListenerFilterRequest request = (ListenerFilterRequest) factory.CreateMessage(ListenerFilterRequest.TYPE_ID);

                request.Filter    = filter;
                request.FilterId  = filterId;
                request.Trigger   = trigger;
                request.IsPriming = isPriming;

                if (isSync)
                {
                    // this is necessary to support the removal of an ISynchronousListener from
                    // within another ISynchronousListener (as is the case in NearCache)
                    channel.Send(request);
                }
                else
                {
                    channel.Request(request);
                }
            }

            /// <summary>
            /// Send a request to the remote NamedCacheProxy to unregister a
            /// <see cref="ICacheListener"/> on behalf of this
            /// <b>INamedCache</b>.
            /// </summary>
            /// <param name="key">
            /// The key used to unregister the remote <b>ICacheListener</b>.
            /// </param>
            /// <param name="isSync">
            /// If the remote <b>ICacheListener</b> is a
            /// <see cref="Cache.Support.CacheListenerSupport.ISynchronousListener"/>.
            /// </param>
            /// <param name="trigger">
            /// The optional <see cref="ICacheTrigger"/> to associate with
            /// the request.
            /// </param>
            /// <param name="isPriming">
            /// If the remote <b>ICacheListener</b> is "priming".
            /// </param>
            protected virtual void RemoveRemoteCacheListener(object key, bool isSync, ICacheTrigger trigger, bool isPriming)
            {
                IChannel           channel = EnsureChannel();
                IMessageFactory    factory = channel.MessageFactory;
                ListenerKeyRequest request = (ListenerKeyRequest) factory.CreateMessage(ListenerKeyRequest.TYPE_ID);

                request.Key       = key;
                request.Trigger   = trigger;
                request.IsPriming = isPriming;

                if (isSync)
                {
                    // this is necessary to support the removal of an ISynchronousListener from
                    // within another ISynchronousListener (as is the case in NearCache)
                    channel.Send(request);
                }
                else
                {
                    channel.Request(request);
                }
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
                get { return RemoteNamedCache.CacheName; }
            }

            /// <summary>
            /// Gets the <see cref="ICacheService"/> that this INamedCache is
            /// a part of.
            /// </summary>
            /// <value>
            /// The cache service this INamedCache is a part of.
            /// </value>
            public virtual ICacheService CacheService
            {
                get { return null; }
            }

            /// <summary>
            /// Specifies whether or not the <see cref="INamedCache"/> is
            /// active.
            /// </summary>
            /// <value>
            /// <b>true</b> if the INamedCache is active; <b>false</b>
            /// otherwise.
            /// </value>
            public virtual bool IsActive
            {
                get
                {
                    IChannel channel = Channel;
                    return channel == null ? false : channel.IsOpen;
                }
            }

            /// <summary>
            /// Release local resources associated with this instance of
            /// INamedCache.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Releasing a cache makes it no longer usable, but does not
            /// affect the cache itself. In other words, all other references
            /// to the cache will still be valid, and the cache data is not
            /// affected by releasing the reference.
            /// Any attempt to use this reference afterword will result in an
            /// exception.</p>
            /// </remarks>
            public virtual void Release()
            {}

            /// <summary>
            /// Release and destroy this instance of
            /// <see cref="INamedCache"/>.
            /// </summary>
            /// <remarks>
            /// <p>
            /// <b>Warning:</b> This method is used to completely destroy the
            /// specified cache across the cluster. All references in the
            /// entire cluster to this cache will be invalidated, the cached
            /// data will be cleared, and all resources will be released.</p>
            /// </remarks>
            public virtual void Destroy()
            {}

            /// <summary>
            /// Construct a view of this INamedCache.
            /// </summary>
            /// <returns>A local view for this INamedCache</returns>
            /// <see cref="ViewBuilder"/>
            /// <since>12.2.1.4</since>
            public virtual ViewBuilder View()
            {
                throw new InvalidOperationException();
            }

            #endregion

            #region IObservableCache implementation

            /// <summary>
            /// Add a standard cache listener that will receive all events
            /// (inserts, updates, deletes) that occur against the cache,
            /// with the key, old-value and new-value included.
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
            /// Remove a standard cache listener that previously signed up
            /// for all events.
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
            /// The listeners will receive <see cref="CacheEventArgs"/>
            /// objects, but if <paramref name="isLite"/> is passed as
            /// <b>true</b>, they <i>might</i> not contain the
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
            /// <b>NewValue</b> property values in order to allow
            /// optimizations.
            /// </param>
            public virtual void AddCacheListener(ICacheListener listener, object key, bool isLite)
            {
                if (listener == null)
                {
                    throw new ArgumentNullException("listener");
                }
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                if (listener is CacheTriggerListener)
                {
                    AddRemoteCacheListener(key, isLite, ((CacheTriggerListener) listener).Trigger, false /*isPriming*/);
                }
                else
                {
                    bool wasEmpty;
                    bool wasLite;

                    // returned keys will not be int decorated
                    Binary binKey = (Binary) BinaryToUndecoratedBinaryConverter.Convert(key);

                    CacheListenerSupport support = CacheListenerSupport;
                    lock (support)
                    {
                        wasEmpty = support.IsEmpty(binKey);
                        wasLite  = !wasEmpty && !support.ContainsStandardListeners(binKey);
                        support.AddListener(listener, binKey, isLite);
                    }

                    bool isPriming = CacheListenerSupport.IsPrimingListener(listener);

                    if (wasEmpty || (wasLite && !isLite) || isPriming)
                    {
                        try
                        {
                            AddRemoteCacheListener(key, isLite, null, isPriming);
                        }
                        catch (Exception)
                        {
                            support.RemoveListener(listener, binKey);
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
            /// A filter used to evaluate events; <c>null</c> is equivalent
            /// to a filter that alway returns <b>true</b>.
            /// </param>
            public virtual void RemoveCacheListener(ICacheListener listener, IFilter filter)
            {
                if (listener == null)
                {
                    throw new ArgumentNullException("listener");
                }

                CacheListenerSupport support = CacheListenerSupport;
                bool isSync = listener is CacheListenerSupport.ISynchronousListener;
                
                if (listener is CacheTriggerListener)
                {
                    RemoveRemoteCacheListener(filter, 0L, isSync, ((CacheTriggerListener) listener).Trigger, /*isPriming*/ false);
                }
                else if (CacheListenerSupport.IsPrimingListener(listener)
                         && filter is InKeySetFilter)
                {
                    ICollection keys = ((InKeySetFilter) filter).Keys;

                    IConverter conv = BinaryToUndecoratedBinaryConverter;

                    lock (support)
                        {
                            foreach (object key in keys)
                            {
                                Binary binKey = (Binary) conv.Convert(key);
                                support.RemoveListener(listener, binKey);
                            }
                        }
                    RemoveRemoteCacheListener(filter, 1 /*dummy*/, isSync, null, true /*isPriming*/);
                }
                else
                {
                    bool isEmpty;
                    long filterId = 0L;

                    lock (support)
                    {
                        support.RemoveListener(listener, filter);
                        isEmpty = support.IsEmpty(filter);
                        if (isEmpty)
                        {
                            filterId = GetFilterId(filter);
                            FilterArray.Remove(filterId);
                        }
                    }

                    if (isEmpty)
                    {
                        RemoveRemoteCacheListener(filter, filterId, isSync, null, false /*isPriming*/);
                    }
                }
            }

            /// <summary>
            /// Add a cache listener that receives events based on a filter
            /// evaluation.
            /// </summary>
            /// <remarks>
            /// <p>
            /// The listeners will receive <see cref="CacheEventArgs"/>
            /// objects, but if <paramref name="isLite"/> is passed as
            /// <b>true</b>, they <i>might</i> not contain the
            /// <b>OldValue</b> and <b>NewValue</b> properties.</p>
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
            /// listener only if the filter evaluates to <b>true</b> for
            /// that <b>CacheEvent</b>; <c>null</c> is equivalent to a filter
            /// that alway returns <b>true</b>.
            /// </param>
            /// <param name="isLite">
            /// <b>true</b> to indicate that the <see cref="CacheEventArgs"/>
            /// objects do not have to include the <b>OldValue</b> and
            /// <b>NewValue</b> property values in order to allow
            /// optimizations.
            /// </param>
            public virtual void AddCacheListener(ICacheListener listener, IFilter filter, bool isLite)
            {
                if (listener == null)
                {
                    throw new ArgumentNullException("listener");
                }

                CacheListenerSupport support = CacheListenerSupport;

                if (listener is CacheTriggerListener)
                {
                    AddRemoteCacheListener(filter, 0L, isLite, ((CacheTriggerListener) listener).Trigger, /*isPriming*/ false);
                }
                else if (CacheListenerSupport.IsPrimingListener(listener)
                            && filter is InKeySetFilter)
                {
                    ICollection keys = ((InKeySetFilter) filter).Keys;

                    IConverter conv = BinaryToUndecoratedBinaryConverter;

                    lock (support)
                        {
                            foreach (object key in keys)
                            {
                                Binary binKey = (Binary) conv.Convert(key);
                                support.AddListener(listener, binKey, isLite); 
                            }
                        }

                    try
                    {
                        AddRemoteCacheListener(filter, 1 /*dummy*/, isLite, null, /*fPriming*/ true);
                    }
                    catch (Exception)
                    {
                        lock (support)
                        {
                            foreach (object key in Keys)
                            {
                                Binary binKey = (Binary) conv.Convert(key);
                                support.RemoveListener(listener, binKey);
                            }
                        }
                        throw;
                    }
                }
                else
                {
                    bool wasEmpty;
                    bool wasLite;
                    long filterId;

                    lock (support)
                    {
                        wasEmpty = support.IsEmpty(filter);
                        wasLite  = !wasEmpty && !support.ContainsStandardListeners(filter);
                        filterId = wasEmpty ? RegisterFilter(filter) : GetFilterId(filter);
                        support.AddListener(listener, filter, isLite);
                    }

                    if (wasEmpty || (wasLite && !isLite))
                    {
                        try
                        {
                            AddRemoteCacheListener(filter, filterId, isLite, null, /*isPriming*/ false);
                        }
                        catch (Exception)
                        {
                            lock (support)
                            {
                                if (wasEmpty)
                                {
                                    FilterArray.Remove(filterId);
                                }
                                support.RemoveListener(listener, filter);
                            }
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
                if (listener == null)
                {
                    throw new ArgumentNullException("listener");
                }
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                bool isSync = listener is CacheListenerSupport.ISynchronousListener;
                if (listener is CacheTriggerListener)
                {
                    RemoveRemoteCacheListener(key, isSync, ((CacheTriggerListener) listener).Trigger, /*isPriming*/ false);
                }
                else
                {
                    bool isEmpty;

                    // returned keys will not be int decorated
                    Binary binKey = (Binary) BinaryToUndecoratedBinaryConverter.Convert(key);

                    CacheListenerSupport support = CacheListenerSupport;
                    lock (support)
                    {
                        support.RemoveListener(listener, binKey);
                        isEmpty = support.IsEmpty(binKey);
                    }

                    bool isPriming = CacheListenerSupport.IsPrimingListener(listener);
                    if (isEmpty || isPriming)
                    {
                        RemoveRemoteCacheListener(key, isSync, null, isPriming);
                    }
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
            /// For each key that is in the cache, that key and its
            /// corresponding value will be placed in the dictionary that is
            /// returned by this method. The absence of a key in the returned
            /// dictionary indicates that it was not in the cache, which may
            /// imply (for caches that can load behind the scenes) that the
            /// requested data could not be loaded.</p>
            /// <p>
            /// The result of this method is defined to be semantically the
            /// same as the following implementation, without regards to
            /// threading issues:</p>
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
            /// A dictionary of keys to values for the specified keys passed
            /// in <paramref name="keys"/>.
            /// </returns>
            public virtual IDictionary GetAll(ICollection keys)
            {
                IChannel        channel = EnsureChannel();
                IMessageFactory factory = channel.MessageFactory;
                GetAllRequest   request = (GetAllRequest) factory.CreateMessage(GetAllRequest.TYPE_ID);

                request.Keys = keys;

                IDictionary response = (IDictionary) channel.Request(request);

                return response == null
                    ? response
                    : ConverterCollections.GetDictionary(response,
                            NullImplementation.GetConverter(),
                            BinaryToUndecoratedBinaryConverter,
                            NullImplementation.GetConverter(),
                            NullImplementation.GetConverter());
            }

            /// <summary>
            /// Associates the specified value with the specified key in this
            /// cache.
            /// </summary>
            /// <remarks>
            /// <p>
            /// If the cache previously contained a mapping for this key, the
            /// old value is replaced.</p>
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
            /// Previous value associated with specified key, or <c>null</c>
            /// if there was no mapping for key. A <c>null</c> return can
            /// also indicate that the cache previously associated
            /// <c>null</c> with the specified key, if the implementation
            /// supports <c>null</c> values.
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
            /// If the cache previously contained a mapping for this key, the
            /// old value is replaced.</p>
            /// This variation of the <see cref="Insert(object, object)"/>
            /// method allows the caller to specify an expiry (or "time to
            /// live") for the cache entry.
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
            /// <b>not</b> a date/time value, but the amount of time object
            /// will be kept in the cache.
            /// </param>
            /// <returns>
            /// Previous value associated with specified key, or <c>null</c>
            /// if there was no mapping for key. A <c>null</c> return can
            /// also indicate that the cache previously associated
            /// <c>null</c> with the specified key, if the implementation
            /// supports <c>null</c> values.
            /// </returns>
            /// <exception cref="NotSupportedException">
            /// If the requested expiry is a positive value and the
            /// implementation does not support expiry of cache entries.
            /// </exception>
            public virtual object Insert(object key, object value, long millis)
            {
                return Insert(key, value, millis, true);
            }

            /// <summary>
            /// Copies all of the mappings from the specified dictionary to
            /// this cache (optional operation).
            /// </summary>
            /// <remarks>
            /// These mappings will replace any mappings that this cache had
            /// for any of the keys currently in the specified dictionary.
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
            /// This cache does not permit <c>null</c> keys or values, and
            /// the specified key or value is <c>null</c>.
            /// </exception>
            public virtual void InsertAll(IDictionary dictionary)
            {
                IChannel        channel = EnsureChannel();
                IMessageFactory factory = channel.MessageFactory;
                PutAllRequest   request = (PutAllRequest) factory.CreateMessage(PutAllRequest.TYPE_ID);

                request.Map = dictionary;

                channel.Request(request);
            }

            /// <summary>
            /// Gets the entries collection.
            /// </summary>
            /// <value>
            /// The collection of <see cref="ICacheEntry"/> objects.
            /// </value>
            public virtual ICollection Entries
            {
                get { return m_entries; }
            }

            /// <summary>
            /// Returns an <see cref="ICacheEnumerator"/> object for this
            /// <b>ICache</b> object.
            /// </summary>
            /// <returns>
            /// An <b>ICacheEnumerator</b> object for this <b>ICache</b>
            /// object.
            /// </returns>
            public virtual ICacheEnumerator GetEnumerator()
            {
                return new Enumerator(this);
            }

            #endregion

            #region ICollection and IDictionary implementation

            /// <summary>
            /// Determines whether the <b>IDictionary</b> object contains an
            /// element with the specified key.
            /// </summary>
            /// <param name="key">
            /// The key to locate in the <b>IDictionary</b> object.
            /// </param>
            /// <returns>
            /// <b>true</b> if the <b>IDictionary</b> contains an element
            /// with the key; otherwise, <b>false</b>.
            /// </returns>
            public virtual bool Contains(object key)
            {
                IChannel           channel = EnsureChannel();
                IMessageFactory    factory = channel.MessageFactory;
                ContainsKeyRequest request = (ContainsKeyRequest) factory.CreateMessage(ContainsKeyRequest.TYPE_ID);

                request.Key = key;

                return (bool) channel.Request(request);
            }

            /// <summary>
            /// Adds an element with the provided key and value to the
            /// <see cref="IDictionary"/> object.
            /// </summary>
            /// <param name="value">
            /// The <see cref="Object"/> to use as the value of the element
            /// to add.
            /// </param>
            /// <param name="key">
            /// The <see cref="Object"/> to use as the key of the element to
            /// add.
            /// </param>
            /// <seealso cref="Insert(object,object)"/>
            public virtual void Add(object key, object value)
            {
                Insert(key, value, CacheExpiration.DEFAULT, false);
            }

            /// <summary>
            /// Removes all mappings from this cache.
            /// </summary>
            /// <remarks>
            /// Some implementations will attempt to lock the entire cache
            /// (if necessary) before preceeding with the clear operation.
            /// For such implementations, the entire cache has to be either
            /// already locked or able to be locked for this operation to
            /// succeed.
            /// </remarks>
            /// <exception cref="InvalidOperationException">
            /// If the lock could not be succesfully obtained for some key.
            /// </exception>
            public virtual void Clear()
            {
                IChannel        channel = EnsureChannel();
                IMessageFactory factory = channel.MessageFactory;
                ClearRequest    request = (ClearRequest) factory.CreateMessage(ClearRequest.TYPE_ID);

                channel.Request(request);
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
                IChannel channel = EnsureChannel();
                IMessageFactory factory = channel.MessageFactory;
                ClearRequest request = (ClearRequest)factory.CreateMessage(ClearRequest.TYPE_ID);

                // COH-13916
                if (request.ImplVersion <= 5)
                {
                    throw new NotSupportedException("NamedCache.truncate is not supported by the current proxy. "
                          + "Either upgrade the version of Coherence on the proxy or connect to a proxy "
                          + "that supports the truncate operation.");
                }

                request.IsTruncate = true;

                channel.Request(request);
            }

            /// <summary>
            /// Returns an <b>IDictionaryEnumerator</b> object for the
            /// <b>IDictionary</b> object.
            /// </summary>
            /// <returns>
            /// An <b>IDictionaryEnumerator</b> object for the
            /// <b>IDictionary</b> object.
            /// </returns>
            IDictionaryEnumerator IDictionary.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Removes the element with the specified key from the
            /// <b>IDictionary</b> object.
            /// </summary>
            /// <param name="key">
            /// The key of the element to remove.
            /// </param>
            /// <seealso cref="Remove(Object, bool)"/>
            public virtual void Remove(object key)
            {
                Remove(key, false);
            }

            /// <summary>
            /// Returns the value to which this cache maps the specified key.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Returns <c>null</c> if the cache contains no mapping for
            /// this key. A return value of <c>null</c> does not
            /// <i>necessarily</i> indicate that the cache contains no
            /// mapping for the key; it's also possible that the cache
            /// explicitly maps the key to <c>null</c>.</p>
            /// <p>
            /// The <see cref="Contains"/> operation may be used to
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
            public virtual object this[object key]
            {
                get
                {
                    IChannel        channel = EnsureChannel();
                    IMessageFactory factory = channel.MessageFactory;
                    GetRequest      request = (GetRequest) factory.CreateMessage(GetRequest.TYPE_ID);

                    request.Key = key;

                    return channel.Request(request);
                }
                set { Add(key, value); }
            }

            /// <summary>
            /// Gets the keys collection.
            /// </summary>
            /// <value>
            /// The keys collection.
            /// </value>
            public virtual ICollection Keys
            {
                get { return m_keys; }
            }

            /// <summary>
            /// Gets the values collection.
            /// </summary>
            /// <value>
            /// The values collection.
            /// </value>
            public virtual ICollection Values
            {
                get { return m_values; }
            }

            /// <summary>
            /// Gets a value indicating whether the <b>IDictionary</b> object
            /// is read-only.
            /// </summary>
            /// <value>
            /// Always <b>false</b> for BinaryCache.
            /// </value>
            public virtual bool IsReadOnly
            {
                get { return false; }
            }

            /// <summary>
            /// Gets a value indicating whether the <b>IDictionary</b> object
            /// has a fixed size.
            /// </summary>
            /// <value>
            /// Always <b>false</b> for BinaryCache.
            /// </value>
            public virtual bool IsFixedSize
            {
                get { return false; }
            }

            /// <summary>
            /// Copies the elements of the <b>ICollection</b> to an
            /// <b>Array</b>, starting at a particular index.
            /// </summary>
            /// <param name="array">
            /// The one-dimensional <b>Array</b> that is the destination of
            /// the elements copied from <b>ICollection</b>.
            /// </param>
            /// <param name="index">
            /// The zero-based index in array at which copying begins.
            /// </param>
            public virtual void CopyTo(Array array, int index)
            {
                object[] list = new object[Count];
                int      i    = 0;
                foreach (object o in this)
                {
                    list[i++] = o;
                }

                Array.Copy(list, 0, array, index, Count);
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
                get
                {
                    IChannel        channel = EnsureChannel();
                    IMessageFactory factory = channel.MessageFactory;
                    SizeRequest     request = (SizeRequest) factory.CreateMessage(SizeRequest.TYPE_ID);

                    return (int) channel.Request(request);
                }
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
            /// Gets a value indicating whether access to the
            /// <b>ICollection</b> is synchronized (thread safe).
            /// </summary>
            /// <value>
            /// Always <b>false</b> for BinaryCache.
            /// </value>
            public virtual bool IsSynchronized
            {
                get { return true; }
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <b>IEnumerator</b> object that can be used to iterate
            /// through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            #region IConcurrentCache implementation

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
                return Lock(key, 0);
            }

            /// <summary>
            /// Attempt to lock the specified item within the specified
            /// period of time.
            /// </summary>
            /// <remarks>
            /// <p>
            /// The item doesn't have to exist to be <i>locked</i>. While the
            /// item is locked there is known to be a <i>lock holder</i>
            /// which has an exclusive right to modify (calling put and
            /// remove methods) that item.</p>
            /// <p>
            /// Lock holder is an abstract concept that depends on the
            /// IConcurrentCache implementation. For example, holder could
            /// be a cluster member or a thread (or both).</p>
            /// <p>
            /// Locking strategy may vary for concrete implementations as
            /// well. Lock could have an expiration time (this lock is
            /// sometimes called a "lease") or be held indefinitely (until
            /// the lock holder terminates).</p>
            /// <p>
            /// Some implementations may allow the entire map to be locked.
            /// If the map is locked in such a way, then only a lock holder
            /// is allowed to perform any of the "put" or "remove"
            /// operations.</p>
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
            /// The number of milliseconds to continue trying to obtain a
            /// lock; pass zero to return immediately; pass -1 to block the
            /// calling thread until the lock could be obtained.
            /// </param>
            /// <returns>
            /// <b>true</b> if the item was successfully locked within the
            /// specified time; <b>false</b> otherwise.
            /// </returns>
            [Obsolete("Obsolete as of Coherence 12.1")]
            public virtual bool Lock(object key, long waitTimeMillis)
            {
                IChannel        channel = EnsureChannel();
                IMessageFactory factory = channel.MessageFactory;
                LockRequest     request = (LockRequest) factory.CreateMessage(LockRequest.TYPE_ID);

                request.Key           = key;
                request.TimeoutMillis = waitTimeMillis;

                return (bool) channel.Request(request);
            }

            /// <summary>
            /// Unlock the specified item.
            /// </summary>
            /// <remarks>
            /// The item doesn't have to exist to be <i>unlocked</i>.
            /// If the item is currently locked, only the <i>holder</i> of
            /// the lock could successfully unlock it.
            /// </remarks>
            /// <param name="key">
            /// Key being unlocked.
            /// </param>
            /// <returns>
            /// <b>true</b> if the item was successfully unlocked;
            /// <b>false</b> otherwise.
            /// </returns>
            [Obsolete("Obsolete as of Coherence 12.1")]
            public virtual bool Unlock(object key)
            {
                IChannel        channel = EnsureChannel();
                IMessageFactory factory = channel.MessageFactory;
                UnlockRequest   request = (UnlockRequest) factory.CreateMessage(UnlockRequest.TYPE_ID);

                request.Key = key;

                return (bool) channel.Request(request);
            }

            #endregion

            #region IQueryCache implementation
            
            /// <summary>
            /// Return a collection of the keys contained in this cache for
            /// entries that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <returns>
            /// A collection of keys for entries that satisfy the specified
            /// criteria.
            /// </returns>
            public virtual object[] GetKeys(IFilter filter)
            {
                return CollectionUtils.ToArray(Query(filter, true));
            }

            /// <summary>
            /// Return a collection of the values contained in this cache
            /// that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <remarks>
            /// <p>
            /// It is guaranteed that enumerator will traverse the array in
            /// such a way that the entry values come up in ascending order,
            /// sorted by the specified comparer or according to the
            /// <i>natural ordering</i>.</p>
            /// </remarks>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <param name="comparer">
            /// The <b>IComparer</b> object which imposes an ordering on
            /// values in the resulting collection; or <c>null</c> if the
            /// entries' values natural ordering should be used.
            /// </param>
            /// <returns>
            /// A collection of values that satisfy the specified criteria.
            /// </returns>
            public virtual object[] GetValues(IFilter filter, IComparer comparer)
            {
                object[] values = GetValues(filter);

                if (values.Length > 0)
                {
                    Array.Sort(values, new SafeComparer(comparer));
                }
                return values;
            }

            /// <summary>
            /// Return a collection of the values contained in this cache
            /// that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <returns>
            /// A collection of values that satisfy the specified criteria.
            /// </returns>
            public virtual object[] GetValues(IFilter filter)
            {
                ICollection result = Query(filter, false);

                if (result.Count == 0)
                {
                    return new object[0];
                }

                object[] values = new object[result.Count];
                int      i      = 0;
                foreach (DictionaryEntry entry in result)
                {
                    values[i++] = entry.Value;
                }
                return values;
            }

            /// <summary>
            /// Return a collection of the entries contained in this cache
            /// that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <remarks>
            /// <p>
            /// It is guaranteed that enumerator will traverse the array in
            /// such a way that the entry values come up in ascending order,
            /// sorted by the specified comparer or according to the
            /// <i>natural ordering</i>.</p>
            /// </remarks>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <param name="comparer">
            /// The <b>IComparer</b> object which imposes an ordering on
            /// entries in the resulting collection; or <c>null</c> if the
            /// entries' values natural ordering should be used.
            /// </param>
            /// <returns>
            /// An array of entries that satisfy the specified criteria.
            /// </returns>
            public virtual ICacheEntry[] GetEntries(IFilter filter, IComparer comparer)
            {
                // COH-2717
                throw new NotSupportedException();
            }

            /// <summary>
            /// Return a collection of the entries contained in this cache
            /// that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <returns>
            /// A collection of entries that satisfy the specified criteria.
            /// </returns>
            public virtual ICacheEntry[] GetEntries(IFilter filter)
            {
                List<ICacheEntry> entries = new List<ICacheEntry>();
                if (filter == null)
                {
                    foreach (CacheEntry entry in Entries)
                    {
                        entries.Add(entry);
                    }
                }
                else
                {
                    foreach (DictionaryEntry entry in Query(filter, false))
                    {
                        entries.Add((CacheEntry) entry);
                    }  
                }

                return entries.ToArray();
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
            /// The <see cref="IValueExtractor"/> object that is used to
            /// extract an indexable object from a value stored in the
            /// indexed cache. Must not be <c>null</c>.
            /// </param>
            /// <param name="isOrdered">
            /// <b>true</b> if the contents of the indexed information should
            /// be ordered; <b>false</b> otherwise.
            /// </param>
            /// <param name="comparer">
            /// The <b>IComparer</b> object which imposes an ordering on
            /// entries in the indexed cache; or <c>null</c> if the entries'
            /// values natural ordering should be used.
            /// </param>
            public virtual void AddIndex(IValueExtractor extractor, bool isOrdered, IComparer comparer)
            {
                IChannel        channel = EnsureChannel();
                IMessageFactory factory = channel.MessageFactory;
                IndexRequest    request = (IndexRequest) factory.CreateMessage(IndexRequest.TYPE_ID);

                request.Add       = true;
                request.Comparer  = comparer;
                request.Extractor = extractor;
                request.IsOrdered = isOrdered;

                channel.Request(request);
            }

            /// <summary>
            /// Remove an index from this IQueryCache.
            /// </summary>
            /// <param name="extractor">
            /// The <see cref="IValueExtractor"/> object that is used to
            /// extract an indexable object from a value stored in the cache.
            /// </param>
            public virtual void RemoveIndex(IValueExtractor extractor)
            {
                IChannel        channel = EnsureChannel();
                IMessageFactory factory = channel.MessageFactory;
                IndexRequest    request = (IndexRequest) factory.CreateMessage(IndexRequest.TYPE_ID);

                request.Extractor = extractor;
                channel.Request(request);
            }

            #endregion

            #region IInvocableCache implementation

            /// <summary>
            /// Invoke the passed <see cref="IEntryProcessor"/> against the
            /// entry specified by the passed key, returning the result of
            /// the invocation.
            /// </summary>
            /// <param name="key">
            /// The key to process; it is not required to exist within the
            /// cache.
            /// </param>
            /// <param name="agent">
            /// The <b>IEntryProcessor</b> to use to process the specified
            /// key.
            /// </param>
            /// <returns>
            /// The result of the invocation as returned from the
            /// <b>IEntryProcessor</b>.
            /// </returns>
            public virtual object Invoke(object key, IEntryProcessor agent)
            {
                IChannel        channel = EnsureChannel();
                IMessageFactory factory = channel.MessageFactory;
                InvokeRequest   request = (InvokeRequest) factory.CreateMessage(InvokeRequest.TYPE_ID);

                request.Key       = key;
                request.Processor = agent;

                return channel.Request(request);
            }

            /// <summary>
            /// Invoke the passed <see cref="IEntryProcessor"/> against the
            /// set of entries that are selected by the given
            /// <see cref="IFilter"/>, returning the result of the invocation
            /// for each.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Unless specified otherwise, IInvocableCache implementations
            /// will perform this operation in two steps: (1) use the filter
            /// to retrieve a matching entry collection; (2) apply the agent
            /// to every filtered entry. This algorithm assumes that the
            /// agent's processing does not affect the result of the
            /// specified filter evaluation, since the filtering and
            /// processing could be performed in parallel on different
            /// threads.</p>
            /// <p>
            /// If this assumption does not hold, the processor logic has to
            /// be idempotent, or at least re-evaluate the filter. This could
            /// be easily accomplished by wrapping the processor with the
            /// <see cref="ConditionalProcessor"/>.</p>
            /// </remarks>
            /// <param name="filter">
            /// An <see cref="IFilter"/> that results in the collection of
            /// keys to be processed.
            /// </param>
            /// <param name="agent">
            /// The <see cref="IEntryProcessor"/> to use to process the
            /// specified keys.
            /// </param>
            /// <returns>
            /// A dictionary containing the results of invoking the
            /// <b>IEntryProcessor</b> against the keys that are selected by
            /// the given <b>IFilter</b>.
            /// </returns>
            public virtual IDictionary InvokeAll(IFilter filter, IEntryProcessor agent)
            {
                IChannel        channel    = EnsureChannel();
                IMessageFactory factory    = channel.MessageFactory;
                Binary          binCookie  = null;
                IDictionary     dictResult = null;

                do
                {
                    InvokeFilterRequest request = (InvokeFilterRequest) factory.CreateMessage(InvokeFilterRequest.TYPE_ID);

                    request.Cookie    = binCookie;
                    request.Filter    = filter;
                    request.Processor = agent;

                    IStatus         status   = channel.Send(request);
                    PartialResponse response = (PartialResponse) status.WaitForResponse();
                    IDictionary     dict     = (IDictionary) ProcessResponse(response);

                    if (dictResult == null || dictResult.Count == 0)
                    {
                        dictResult = dict;
                    }
                    else if (dict == null || dict.Count == 0)
                    {
                        // nothing to do
                    }
                    else
                    {
                        CollectionUtils.AddAll(dictResult, dict);
                    }

                    binCookie = response.Cookie;
                }
                while (binCookie != null);

                return dictResult == null
                    ? dictResult
                    : ConverterCollections.GetDictionary(dictResult,
                            NullImplementation.GetConverter(),
                            BinaryToUndecoratedBinaryConverter,
                            NullImplementation.GetConverter(),
                            NullImplementation.GetConverter());
            }

            /// <summary>
            /// Invoke the passed <see cref="IEntryProcessor"/> against the
            /// entries specified by the passed keys, returning the result of
            /// the invocation for each.
            /// </summary>
            /// <param name="keys">
            /// The keys to process; these keys are not required to exist
            /// within the cache.
            /// </param>
            /// <param name="agent">
            /// The <b>IEntryProcessor</b> to use to process the specified
            /// keys.
            /// </param>
            /// <returns>
            /// A dictionary containing the results of invoking the
            /// <b>IEntryProcessor</b> against each of the specified keys.
            /// </returns>
            public virtual IDictionary InvokeAll(ICollection keys, IEntryProcessor agent)
            {
                IChannel         channel = EnsureChannel();
                IMessageFactory  factory = channel.MessageFactory;
                InvokeAllRequest request = (InvokeAllRequest) factory.CreateMessage(InvokeAllRequest.TYPE_ID);

                request.Keys      = keys;
                request.Processor = agent;

                IDictionary response = (IDictionary) channel.Request(request);

                return response == null
                    ? response
                    : ConverterCollections.GetDictionary(response,
                            NullImplementation.GetConverter(),
                            BinaryToUndecoratedBinaryConverter,
                            NullImplementation.GetConverter(),
                            NullImplementation.GetConverter());
            }

            /// <summary>
            /// Perform an aggregating operation against the entries
            /// specified by the passed keys.
            /// </summary>
            /// <param name="keys">
            /// The collection of keys that specify the entries within this
            /// cache to aggregate across.
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
                IChannel            channel = EnsureChannel();
                IMessageFactory     factory = channel.MessageFactory;
                AggregateAllRequest request = (AggregateAllRequest) factory.CreateMessage(AggregateAllRequest.TYPE_ID);

                request.Aggregator = agent;
                request.Keys       = keys;

                return channel.Request(request);
            }

            /// <summary>
            /// Perform an aggregating operation against the collection of
            /// entries that are selected by the given <b>IFilter</b>.
            /// </summary>
            /// <param name="filter">
            /// an <see cref="IFilter"/> that is used to select entries
            /// within this cache to aggregate across.
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
                IChannel               channel = EnsureChannel();
                IMessageFactory        factory = channel.MessageFactory;
                AggregateFilterRequest request = (AggregateFilterRequest) factory.CreateMessage(AggregateFilterRequest.TYPE_ID);

                request.Aggregator = agent;
                request.Filter     = filter;

                return channel.Request(request);
            }

            #endregion

            #region IDisposable implementation

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
            /// The RemoteNamedCache that created this BinaryNamedCache.
            /// </summary>
            private readonly RemoteNamedCache m_cache;

            /// <summary>
            /// The Channel used to exchange NamedCache Protocol Messages with
            /// a remote ProxyService.
            /// </summary>
            private IChannel m_channel;

            /// <summary>
            /// The entries collection.
            /// </summary>
            private ICollection m_entries;

            /// <summary>
            /// The keys collection.
            /// </summary>
            private ICollection m_keys;

            /// <summary>
            /// The values collection.
            /// </summary>
            private ICollection m_values;

            /// <summary>
            /// An ILongArray of Filter objects indexed by the unique filter
            /// id.
            /// </summary>
            private ILongArray m_filterArray;

            /// <summary>
            /// CacheListenerSupport used by this BinaryCache to dispatch
            /// cache events to registered ICacheListeners.
            /// </summary>
            private CacheListenerSupport m_cacheListenerSupport;

            /// <summary>
            /// The QueueProcessor used to dispatch cache events.
            /// </summary>
            [NonSerialized]
            private QueueProcessor m_eventDispatcher;

            #endregion

            #region Enum: RemoteCollectionType

            /// <summary>
            /// Remote collection type enumeration.
            /// </summary>
            internal enum RemoteCollectionType
            {
                Entries,
                Keys,
                Values
            }

            #endregion

            #region Inner class: Enumerator

            /// <summary>
            /// <b>IEnumerator</b> implementation for BinaryNamedCache
            /// entries.
            /// </summary>
            internal class Enumerator : ICacheEnumerator
            {
                #region Data members

                /// <summary>
                /// Last key that was iterated.
                /// </summary>
                private object m_key;

                /// <summary>
                /// An iterator over the keys returned by
                /// BinaryNamedCache.Keys.
                /// </summary>
                [NonSerialized]
                private IEnumerator m_keyEnumerator;

                #endregion

                #region Constructors

                /// <summary>
                /// Sets parent <see cref="BinaryNamedCache"/>.
                /// </summary>
                /// <param name="cache">
                /// Sets parent <b>BinaryNamedCache</b>.
                /// </param>
                public Enumerator(BinaryNamedCache cache)
                {
                    BinaryCache   = cache;
                    KeyEnumerator = cache.Keys.GetEnumerator();
                }

                #endregion

                #region Properties

                /// <summary>
                /// Last key that was iterated.
                /// </summary>
                /// <value>
                /// Last key that was iterated.
                /// </value>
                public virtual object Key
                {
                    get { return m_key; }
                    set { m_key = value; }
                }

                /// <summary>
                /// The value of the current cache entry.
                /// </summary>
                /// <value>
                /// The value of the current cache entry.
                /// </value>
                public virtual object Value
                {
                    get { return ((ICacheEntry) Current).Value; }
                }

                /// <summary>
                /// The key and the value of the current dictionary entry.
                /// </summary>
                /// <value>
                /// The key and the value of the current dictionary entry.
                /// </value>
                DictionaryEntry IDictionaryEnumerator.Entry
                {
                    get { return (DictionaryEntry) Current; }
                }

                /// <summary>
                /// The key and the value of the current cache entry.
                /// </summary>
                /// <value>
                /// The key and the value of the current cache entry.
                /// </value>
                public virtual ICacheEntry Entry
                {
                    get { return (ICacheEntry) Current; }
                }

                /// <summary>
                /// An iterator over the keys returned by
                /// <see cref="BinaryNamedCache.Keys"/>.
                /// </summary>
                /// <value>
                /// An iterator over the keys returned by
                /// <b>BinaryNamedCache.Keys</b>.
                /// </value>
                protected virtual IEnumerator KeyEnumerator
                {
                    get { return m_keyEnumerator; }
                    set { m_keyEnumerator = value; }
                }

                /// <summary>
                /// The parent <see cref="BinaryNamedCache"/>.
                /// </summary>
                /// <value>
                /// The parent <b>BinaryNamedCache</b>.
                /// </value>
                protected virtual BinaryNamedCache BinaryCache { get; set; }

                /// <summary>
                /// Gets the current element in the collection.
                /// </summary>
                /// <value>
                /// The current element in the collection.
                /// </value>
                /// <exception cref="InvalidOperationException">
                /// The enumerator is positioned before the first element of
                /// the collection or after the last element.
                /// </exception>
                public virtual object Current
                {
                    get
                    {
                        object key = Key = KeyEnumerator.Current;
                        return new CacheEntry(key,
                                BinaryCache[BinaryCache.BinaryToDecoratedBinaryConverter.Convert(key)]);
                    }
                }

                #endregion

                #region Enumerator methods

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
                    return KeyEnumerator.MoveNext();
                }

                /// <summary>
                /// Sets the enumerator to its initial position, which is before
                /// the first element in the collection.
                /// </summary>
                /// <exception cref="InvalidOperationException">
                /// The collection was modified after the enumerator was created.
                /// </exception>
                public virtual void Reset()
                {
                    KeyEnumerator.Reset();
                }

                #endregion
            }

            #endregion

            #region Inner class: RemoteCollection

            /// <summary>
            /// Represents remote collection of cache keys, values or entries.
            /// </summary>
            /// <author>Ana Cikic  2006.09.11</author>
            /// <author>Aleksandar Seovic  2006.11.11</author>
            /// <seealso cref="BinaryNamedCache"/>
            internal class RemoteCollection : ICollection
            {
                #region Data members

                /// <summary>
                /// Parent BinaryNamedCache.
                /// </summary>
                private BinaryNamedCache m_binaryCache;

                /// <summary>
                /// The remote collection type.
                /// </summary>
                private RemoteCollectionType m_type;

                #endregion

                #region Constructors

                /// <summary>
                /// Initializes <b>RemoteCollection</b>.
                /// </summary>
                /// <param name="parent">
                /// Parent <b>BinaryNamedCache</b>.
                /// </param>
                /// <param name="type">
                /// Type of the remote collection, one of
                /// <see cref="RemoteCollectionType"/> values.
                /// </param>
                public RemoteCollection(BinaryNamedCache parent, RemoteCollectionType type)
                {
                    m_binaryCache = parent;
                    m_type        = type;
                }

                #endregion

                #region Properties

                /// <summary>
                /// Gets the number of elements contained in the collection.
                /// </summary>
                /// <value>
                /// The number of elements contained in the collection.
                /// </value>
                /// <seealso cref="ICollection.Count"/>
                public virtual int Count
                {
                    get { return m_binaryCache.Count; }
                }

                /// <summary>
                /// Gets an object that can be used to synchronize access to
                /// the collection.
                ///</summary>
                /// <value>
                /// An object that can be used to synchronize access to the
                /// collection.
                /// </value>
                public virtual object SyncRoot
                {
                    get { return m_binaryCache.SyncRoot; }
                }

                /// <summary>
                /// Gets a value indicating whether access to the collection
                /// is synchronized (thread safe).
                /// </summary>
                /// <value>
                /// <b>true</b> if access to the collection is synchronized
                /// (thread safe); otherwise, <b>false</b>.
                /// </value>
                public virtual bool IsSynchronized
                {
                    get { return m_binaryCache.IsSynchronized; }
                }

                #endregion

                #region ICollection implementation

                /// <summary>
                /// Returns an enumerator that iterates through a collection.
                /// </summary>
                /// <returns>
                /// An <see cref="IEnumerator"/> object that can be used to
                /// iterate through the collection.
                /// </returns>
                public virtual IEnumerator GetEnumerator()
                {
                    if (m_type == RemoteCollectionType.Keys)
                    {
                        return new PagedEnumerator(new Advancer(m_binaryCache));
                    }
                    return new RemoteEnumerator(m_binaryCache, m_type);
                }

                /// <summary>
                /// Copies the elements of the collection to an array,
                /// starting at a particular index.
                /// </summary>
                /// <param name="array">
                /// The one-dimensional array that is the destination of the
                /// elements copied from the collection. The array must have
                /// zero-based indexing.
                /// </param>
                /// <param name="index">
                /// The zero-based index in array at which copying begins.
                /// </param>
                /// <exception cref="ArgumentNullException">
                /// Array is null.
                /// </exception>
                /// <exception cref="ArgumentOutOfRangeException">
                /// Index is less than zero -or- index is equal to or greater
                /// than the length of array.
                /// </exception>
                /// <exception cref="ArgumentException">
                /// Array is multidimensional -or- the number of elements in
                /// the source collection is greater than the available space
                /// from index to the end of the destination array.
                /// </exception>
                public virtual void CopyTo(Array array, int index)
                {
                    if (array == null)
                    {
                        throw new ArgumentNullException("array", "Array cannot be null.");
                    }
                    if (index < 0 || index >= array.Length)
                    {
                        throw new ArgumentOutOfRangeException("index",
                                                              "Index has to be within array boundaries.");
                    }
                    if (array.Rank > 1)
                    {
                        throw new ArgumentException("One-dimensional array expected.", "array");
                    }

                    ICollection keys = m_binaryCache.GetKeys(null);

                    if (keys.Count > array.Length - index)
                    {
                        throw new ArgumentException(
                                "Array is not big enough to accomodate all collection elements", "array");
                    }

                    if (m_type == RemoteCollectionType.Keys)
                    {
                        keys.CopyTo(array, index);
                    }
                    else
                    {
                        foreach (object key in keys)
                        {
                            object value = (m_type == RemoteCollectionType.Values
                                ? m_binaryCache[m_binaryCache
                                        .BinaryToDecoratedBinaryConverter.Convert(key)]
                                : new CacheEntry(key,
                                        m_binaryCache[m_binaryCache
                                        .BinaryToDecoratedBinaryConverter.Convert(key)]));
                            array.SetValue(value, index++);
                        }
                    }
                }

                #endregion

                #region Inner class: RemoteEnumerator

                /// <summary>
                /// <b>IEnumerator</b> implementation for
                /// <b>RemoteCollection</b>.
                /// </summary>
                internal class RemoteEnumerator : IEnumerator
                {
                    #region Data members

                    /// <summary>
                    /// An iterator over the keys returned by
                    /// BinaryNamedCache.Keys.
                    /// </summary>
                    [NonSerialized]
                    private IEnumerator m_keyEnumerator;

                    /// <summary>
                    /// The BinaryNamedCache that created the parent virtual
                    /// collection.
                    /// </summary>
                    private readonly BinaryNamedCache m_binaryCache;

                    /// <summary>
                    /// The remote collection type. Determines whether key,
                    /// value or an entry should be returned by the Current
                    /// property.
                    /// </summary>
                    private readonly RemoteCollectionType m_type;

                    #endregion

                    #region Constructors

                    /// <summary>
                    /// Sets <see cref="BinaryNamedCache"/> that created the
                    /// parent RemoteCollection.
                    /// </summary>
                    /// <param name="cache">
                    /// The <b>BinaryNamedCache</b> that created parent
                    /// collection.
                    /// </param>
                    /// <param name="type">
                    /// Type of the remote collection, one of the
                    /// <see cref="RemoteCollectionType"/> values.
                    /// </param>
                    public RemoteEnumerator(BinaryNamedCache cache, RemoteCollectionType type)
                    {
                        m_binaryCache   = cache;
                        m_type          = type;
                        m_keyEnumerator = cache.GetKeys(null).GetEnumerator();
                    }

                    #endregion

                    #region Properties

                    /// <summary>
                    /// Gets the current element in the collection.
                    /// </summary>
                    /// <value>
                    /// The current element in the collection.
                    /// </value>
                    /// <exception cref="InvalidOperationException">
                    /// The enumerator is positioned before the first element
                    /// of the collection or after the last element.
                    /// </exception>
                    public virtual object Current
                    {
                        get
                        {
                            object key = m_keyEnumerator.Current;
                            switch (m_type)
                            {
                                case RemoteCollectionType.Keys:
                                    return key;
                                case RemoteCollectionType.Values:
                                    return m_binaryCache[m_binaryCache
                                            .BinaryToDecoratedBinaryConverter.Convert(key)];
                                default:
                                    return new CacheEntry(key, m_binaryCache[m_binaryCache
                                            .BinaryToDecoratedBinaryConverter.Convert(key)]);
                            }
                        }
                    }

                    #endregion

                    #region IEnumerator implementation

                    /// <summary>
                    /// Advances the enumerator to the next element of the
                    /// collection.
                    /// </summary>
                    /// <returns>
                    /// <b>true</b> if the enumerator was successfully
                    /// advanced to the next element; <b>false</b> if the
                    /// enumerator has passed the end of the collection.
                    /// </returns>
                    /// <exception cref="InvalidOperationException">
                    /// The collection was modified after the enumerator was
                    /// created.
                    /// </exception>
                    public virtual bool MoveNext()
                    {
                        return m_keyEnumerator.MoveNext();
                    }

                    /// <summary>
                    /// Sets the enumerator to its initial position, which is
                    /// before the first element in the collection.
                    /// </summary>
                    /// <exception cref="InvalidOperationException">
                    /// The collection was modified after the enumerator was
                    /// created.
                    /// </exception>
                    public virtual void Reset()
                    {
                        m_keyEnumerator.Reset();
                    }

                    #endregion
                }

                #endregion
            }

            #endregion

            #region Inner class: Advancer

            /// <summary>
            /// <see cref="PagedEnumerator.IAdvancer"/> implementation used
            /// to page remote collection keys.
            /// </summary>
            public class Advancer : PagedEnumerator.IAdvancer
            {
                #region Properties

                /// <summary>
                /// The <see cref="BinaryNamedCache"/> that is being
                /// iterated.
                /// </summary>
                public virtual BinaryNamedCache BinaryCache { get; set; }

                /// <summary>
                /// Opaque cookie used for streaming.
                /// </summary>
                public virtual Binary Cookie { get; set; }

                /// <summary>
                /// <b>true</b> iff the Advancer has been exhausted.
                /// </summary>
                public virtual bool IsExhausted { get; set; }

                #endregion

                #region Constructors

                /// <summary>
                /// Construct an Advancer with specified parent
                /// <see cref="BinaryNamedCache"/>.
                /// </summary>
                /// <param name="binaryCache">
                /// Parent <b>BinaryNamedCache</b>.
                /// </param>
                public Advancer(BinaryNamedCache binaryCache)
                {
                    BinaryCache = binaryCache;
                }

                #endregion

                #region Data members

                #endregion

                #region IAdvancer implementation

                /// <summary>
                /// Obtain a new page of objects to be used by the enclosing
                /// <see cref="PagedEnumerator"/>.
                /// </summary>
                /// <returns>
                /// A collection of objects or <c>null</c> if the advancer is
                /// exhausted.
                /// </returns>
                public virtual ICollection NextPage()
                {
                    if (IsExhausted)
                    {
                        return null;
                    }

                    PartialResponse response  = BinaryCache.GetKeysPage(Cookie);
                    Binary          binCookie = response.Cookie;

                    Cookie = binCookie;
                    if (binCookie == null)
                    {
                        IsExhausted = true;
                    }

                    return (ICollection) response.Result;
                }

                /// <summary>
                /// Sets the advancer to its initial position, which is
                /// before the first page.
                /// </summary>
                public virtual void Reset()
                {
                    throw new NotSupportedException();
                }

                /// <summary>
                /// Remove the specified object from the underlying collection.
                /// </summary>
                /// <remarks>
                /// Naturally, only an object from the very last non-empty page
                /// could be removed.
                /// </remarks>
                /// <param name="curr">
                /// Currently "active" item to be removed from an underlying
                /// collection.
                /// </param>
                public virtual void Remove(object curr)
                {
                    BinaryCache.Remove(
                            BinaryCache.BinaryToDecoratedBinaryConverter.Convert(curr), false);
                }

                #endregion
            }

            #endregion
        }

        #endregion

        #region Inner class: ConverterListener

        /// <summary>
        /// <see cref="ICacheListener"/> implementation that wraps another
        /// <b>ICacheListener</b> and converts dispatched cache events using
        /// the <see cref="RemoteNamedCache"/>'s converters.
        /// </summary>
        public class ConverterListener : ICacheListener
        {
            #region Properties

            /// <summary>
            /// The <see cref="IConverter"/> used to convert
            /// <see cref="CacheEventArgs"/>.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> used to convert <b>CacheEventArgs</b>.
            /// </value>
            public virtual IConverter Converter { get; set; }

            /// <summary>
            /// The delegate <see cref="ICacheListener"/>.
            /// </summary>
            /// <value>
            /// The delegate <b>ICacheListener</b>.
            /// </value>
            public virtual ICacheListener Listener
            {
                get { return m_listener; }
                set { m_listener = value; }
            }

            /// <summary>
            /// The <see cref="INamedCache"/> that is the source of converted
            /// <see cref="CacheEventArgs"/>.
            /// </summary>
            /// <value>
            /// The <b>INamedCache</b> that is the source of converted
            /// <b>CacheEventArgs</b>.
            /// </value>
            public virtual INamedCache NamedCache { get; set; }

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
                Dispatch(evt);
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
                Dispatch(evt);
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
                Dispatch(evt);
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Dispatch the given <see cref="CacheEventArgs"/> to the
            /// delegate <see cref="ICacheListener"/>.
            /// </summary>
            /// <param name="evt">
            /// The <b>CacheEventArgs</b> to dispatch.
            /// </param>
            protected virtual void Dispatch(CacheEventArgs evt)
            {
                IConverter     conv    = Converter;
                CacheEventArgs convEvt = CacheListenerSupport.ConvertEvent(evt, NamedCache, conv, conv);
                CacheListenerSupport.Dispatch(convEvt, Listener);
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Determines whether the specified object is equal to the
            /// current object.
            /// </summary>
            /// <param name="o">
            /// The object to compare to this object.
            /// </param>
            /// <returns>
            /// <b>true</b> if specified object is equal to this object.
            /// </returns>
            public override bool Equals(object o)
            {
                return o is ConverterListener && Listener.Equals(((ConverterListener) o).Listener);
            }

            /// <summary>
            /// Returns a hash code for this object.
            /// </summary>
            /// <returns>
            /// A hash code for this object.
            /// </returns>
            public override int GetHashCode()
            {
                return Listener.GetHashCode();
            }

            /// <summary>
            /// Returns string representation of this object.
            /// </summary>
            /// <returns>
            /// String representation of this object.
            /// </returns>
            public override string ToString()
            {
                return GetType().FullName + ": " + Listener;
            }

            #endregion

            #region Data members

            /// <summary>
            /// The delegate ICacheListener.
            /// </summary>
            [NonSerialized]
            private ICacheListener m_listener;

            #endregion
        }

        #endregion
    }
}
