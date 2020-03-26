/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util
{
    /// <summary>
    /// A collection of classes that do nothing.
    /// </summary>
    /// <remarks>
    /// For each class implemented, a factory method will exist following the
    /// naming convention "Get" plus the class or interface name.
    /// </remarks>
    /// <author>Cameron Purdy  2000.08.02</author>
    /// <authro>Ana Cikic  2007.09.12</authro>
    public class NullImplementation
    {
        #region Factory methods

        /// <summary>
        /// Returns an instance of the <see cref="NullEnumerator"/>.
        /// </summary>
        /// <returns>
        /// An <b>IEnumerator</b> instance with no values to enumerate.
        /// </returns>
        public static IEnumerator GetEnumerator()
        {
            return NullEnumerator.Instance;
        }

        /// <summary>
        /// Returns an instance of the <see cref="NullCollection"/>.
        /// </summary>
        /// <returns>
        /// An empty immutable collection.
        /// </returns>
        public static ICollection GetCollection()
        {
            return NullCollection.Instance;
        }

        /// <summary>
        /// Returns an instance of the <see cref="NullDictionary"/>.
        /// </summary>
        /// <returns>
        /// An empty immutable dictionary.
        /// </returns>
        public static IDictionary GetDictionary()
        {
            return NullDictionary.Instance;
        }

        /// <summary>
        /// Returns an instance of the <see cref="NullCache"/>.
        /// </summary>
        /// <returns>
        /// An empty <see cref="ICache"/> that does nothing.
        /// </returns>
        public static ICache GetCache()
        {
            return NullCache.Instance;
        }

        /// <summary>
        /// Returns an instance of the <see cref="NullObservableCache"/>.
        /// </summary>
        /// <returns>
        /// An empty <see cref="IObservableCache"/> that does nothing.
        /// </returns>
        public static IObservableCache GetObservableCache()
        {
            return NullObservableCache.Instance;
        }

        /// <summary>
        /// Returns an instance of the <see cref="NullValueExtractor"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="IValueExtractor"/> that does not actually extract
        /// anything from the passed value.
        /// </returns>
        public static IValueExtractor GetValueExtractor()
        {
            return NullValueExtractor.Instance;
        }

        /// <summary>
        /// Obtain a null implementation of a <see cref="IConverter"/>.
        /// </summary>
        /// <returns>
        /// A conforming implementation of <b>IConverter</b> that does as
        /// little as possible.
        /// </returns>
        public static IConverter GetConverter()
        {
            return NullConverter.Instance;
        }

        /// <summary>
        /// Obtain a null implementation of a <see cref="IPofContext"/>.
        /// </summary>
        /// <returns>
        /// A conforming implementation of <b>IPofContext</b> that does as
        /// little as possible.
        /// </returns>
        public static IPofContext GetPofContext()
        {
            return NullPofContext.Instance;
        }

        #endregion

        #region Inner class: NullEnumerator

        /// <summary>
        /// An empty enumerator.
        /// </summary>
        public class NullEnumerator : ICacheEnumerator
        {
            #region Constructors

            /// <summary>
            /// No public constructor.
            /// </summary>
            /// <remarks>
            /// The whole point of this class is to minimize allocations in
            /// cases where there is nothing to enumerate.
            /// </remarks>
            private NullEnumerator()
            {}

            #endregion

            #region IEnumerator implementation

            /// <summary>
            /// Advances the enumerator to the next element of the
            /// collection.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the enumerator was successfully advanced to
            /// the next element; <b>false</b> if the enumerator has passed
            /// the end of the collection.
            /// </returns>
            public bool MoveNext()
            {
                return false;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before
            /// the first element in the collection.
            /// </summary>
            public void Reset()
            {}

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
            public object Current
            {
                get { throw new InvalidOperationException(); }
            }

            #endregion

            #region ICacheEnumerator implementation

            /// <summary>
            /// Gets both the key and the value of the current cache entry.
            /// </summary>
            /// <value>
            /// An <see cref="ICacheEntry"/> containing both the key and
            /// the value of the current cache entry.
            /// </value>
            ICacheEntry ICacheEnumerator.Entry
            {
                get { throw new InvalidOperationException(); }
            }

            #endregion

            #region IDictionaryEnumerator implementation

            /// <summary>
            /// Gets the key of the current dictionary entry.
            /// </summary>
            /// <returns>
            /// The key of the current element of the enumeration.
            /// </returns>
            public object Key
            {
                get { throw new InvalidOperationException(); }
            }

            /// <summary>
            /// Gets the value of the current dictionary entry.
            /// </summary>
            /// <returns>
            /// The value of the current element of the enumeration.
            /// </returns>
            public object Value
            {
                get { throw new InvalidOperationException(); }
            }

            /// <summary>
            /// Gets both the key and the value of the current dictionary
            /// entry.
            /// </summary>
            /// <returns>
            /// A <b>DictionaryEntry</b> containing both the key and the
            /// value of the current dictionary entry.
            /// </returns>
            public DictionaryEntry Entry
            {
                get { throw new InvalidOperationException(); }
            }

            #endregion

            /// <summary>
            /// Since the enumerator contains no information, only one ever
            /// has to exist.
            /// </summary>
            public static readonly NullEnumerator Instance = new NullEnumerator();
        }

        #endregion

        #region Inner class: NullCollection

        /// <summary>
        /// An immutable collection which contains nothing.
        /// </summary>
        public class NullCollection : ICollection, IPortableObject
        {
            #region ICollection implementation

            /// <summary>
            /// Copies the elements of the collection to an <b>Array</b>,
            /// starting at a particular array index.
            /// </summary>
            /// <param name="array">
            /// The one-dimensional array that is the destination of the
            /// elements copied from the collection. The array must have
            /// zero-based indexing.
            /// </param>
            /// <param name="index">
            /// The zero-based index in array at which copying begins.
            /// </param>
            public void CopyTo(Array array, int index)
            {}

            /// <summary>
            /// Gets the number of elements contained in the collection.
            /// </summary>
            /// <value>
            /// The number of elements contained in the collection.
            /// </value>
            public int Count
            {
                get { return 0; }
            }

            /// <summary>
            /// Gets an object that can be used to synchronize access to the
            /// collection.
            /// </summary>
            /// <value>
            /// An object that can be used to synchronize access to the
            /// collection.
            /// </value>
            public object SyncRoot
            {
                get { throw new InvalidOperationException(); }
            }

            /// <summary>
            /// Gets a value indicating whether access to the collection is
            /// synchronized (thread safe).
            /// </summary>
            /// <value>
            /// <b>true</b> if access to the collection is synchronized
            /// (thread safe); otherwise, <b>false</b>.
            /// </value>
            public bool IsSynchronized
            {
                get { return false; }
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <b>IEnumerator</b> object that can be used to iterate
            /// through the collection.
            /// </returns>
            public IEnumerator GetEnumerator()
            {
                return EMPTY_ENUMERATOR;
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Compares this object with another object for equality.
            /// </summary>
            /// <param name="o">
            /// An object reference or <c>null</c>.
            /// </param>
            /// <returns>
            /// <b>true</b> if the passed object reference is of the same
            /// class and has the same state as this object.
            /// </returns>
            public override bool Equals(object o)
            {
                return o is ICollection && ((ICollection) o).Count == 0;
            }

            /// <summary>
            /// Returns a hash code value for this object.
            /// </summary>
            /// <returns>
            /// A hash code value for this object.
            /// </returns>
            public override int GetHashCode()
            {
                return 0;
            }

            #endregion

            #region IPortableObject implementation

            /// <summary>
            /// Restore the contents of a user type instance by reading its
            /// state using the specified <see cref="IPofReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>IPofReader</b> from which to read the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public void ReadExternal(IPofReader reader)
            {}

            /// <summary>
            /// Save the contents of a POF user type instance by writing its
            /// state using the specified <see cref="IPofWriter"/> object.
            /// </summary>
            /// <param name="writer">
            /// The <b>IPofWriter</b> to which to write the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public void WriteExternal(IPofWriter writer)
            {}

            #endregion

            /// <summary>
            /// Since the collection contains no information, only one ever
            /// has to exist.
            /// </summary>
            public static readonly ICollection Instance = new NullCollection();

            /// <summary>
            /// Since the collection contains no information, only one
            /// enumerator has to exist.
            /// </summary>
            protected static readonly IEnumerator EMPTY_ENUMERATOR = NullImplementation.GetEnumerator();
        }

        #endregion

        #region Inner class: NullDictionary

        /// <summary>
        /// A dictionary that contains nothing and does nothing.
        /// </summary>
        public class NullDictionary : NullCollection, IDictionary
        {
            #region IDictionary implementation

            /// <summary>
            /// Determines whether the dictionary object contains an element
            /// with the specified key.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the dictionary contains an element with the
            /// key; otherwise, <b>false</b>.
            /// </returns>
            /// <param name="key">
            /// The key to locate in the dictionary.
            /// </param>
            public bool Contains(object key)
            {
                return false;
            }

            /// <summary>
            /// Adds an element with the provided key and value to the
            /// dictionary.
            /// </summary>
            /// <param name="value">
            /// The object to use as the value of the element to add.
            /// </param>
            /// <param name="key">
            /// The object to use as the key of the element to add.
            /// </param>
            public void Add(object key, object value)
            {}

            /// <summary>
            /// Removes all elements from the dictionary.
            /// </summary>
            public void Clear()
            {}

            /// <summary>
            /// Returns an <b>IDictionaryEnumerator</b> object for the
            /// dictionary.
            /// </summary>
            /// <returns>
            /// An <b>IDictionaryEnumerator</b> object for the dictionary.
            /// </returns>
            IDictionaryEnumerator IDictionary.GetEnumerator()
            {
                return EMPTY_ENUMERATOR as IDictionaryEnumerator;
            }

            /// <summary>
            /// Removes the element with the specified key from the
            /// dictionary.
            /// </summary>
            /// <param name="key">
            /// The key of the element to remove.
            /// </param>
            public void Remove(object key)
            {}

            /// <summary>
            /// Gets or sets the element with the specified key.
            /// </summary>
            /// <returns>
            /// The element with the specified key.
            /// </returns>
            /// <param name="key">
            /// The key of the element to get or set.
            /// </param>
            public object this[object key]
            {
                get { return null; }
                set {}
            }

            /// <summary>
            /// Gets an <b>ICollection</b> object containing the keys of the
            /// dictionary.
            /// </summary>
            /// <returns>
            /// An <b>ICollection</b> object containing the keys of the
            /// dictionary.
            /// </returns>
            public ICollection Keys
            {
                get { return GetCollection(); }
            }

            /// <summary>
            /// Gets an <b>ICollection</b> object containing the values in the
            /// dictionary.
            /// </summary>
            /// <returns>
            /// An <b>ICollection</b> object containing the values in the
            /// dictionary.
            /// </returns>
            public ICollection Values
            {
                get { return GetCollection(); }
            }

            /// <summary>
            /// Gets a value indicating whether the dictionary object is
            /// read-only.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the dictionary object is read-only; otherwise,
            /// <b>false</b>.
            /// </returns>
            public bool IsReadOnly
            {
                get { return true; }
            }

            /// <summary>
            /// Gets a value indicating whether the dictionary object has a
            /// fixed size.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the dictionary object has a fixed size;
            /// otherwise, <b>false</b>.
            /// </returns>
            public bool IsFixedSize
            {
                get { return true; }
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Compares this object with another object for equality.
            /// </summary>
            /// <param name="o">
            /// An object reference or <c>null</c>.
            /// </param>
            /// <returns>
            /// <b>true</b> if the passed object reference is of the same
            /// class and has the same state as this object.
            /// </returns>
            public override bool Equals(object o)
            {
                return o is IDictionary && ((IDictionary) o).Count == 0;
            }

            /// <summary>
            /// Returns a hash code value for this object.
            /// </summary>
            /// <returns>
            /// A hash code value for this object.
            /// </returns>
            public override int GetHashCode()
            {
                return 0;
            }

            #endregion

            /// <summary>
            /// Since the dictionary contains no information, only one ever
            /// has to exist.
            /// </summary>
            public new static readonly IDictionary Instance = new NullDictionary();
        }

        #endregion

        #region Inner class: NullCache

        /// <summary>
        /// A <b>ICache</b> that contains nothing and does nothing.
        /// </summary>
        public class NullCache : NullCollection, ICache
        {
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
            public IDictionary GetAll(ICollection keys)
            {
                return GetDictionary();
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
            public object Insert(object key, object value)
            {
                return null;
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
            public object Insert(object key, object value, long millis)
            {
                return null;
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
            public void InsertAll(IDictionary dictionary)
            {}

            /// <summary>
            /// Gets a collection of <see cref="ICacheEntry"/> instances
            /// within the cache.
            /// </summary>
            public ICollection Entries
            {
                get { return GetCollection(); }
            }

            /// <summary>
            /// Returns an <see cref="ICacheEnumerator"/> object for the
            /// <b>ICache</b> instance.
            /// </summary>
            /// <returns>An <b>ICacheEnumerator</b> object for the
            /// <b>ICache</b> instance.</returns>
            ICacheEnumerator ICache.GetEnumerator()
            {
                return EMPTY_ENUMERATOR as ICacheEnumerator;
            }

            #endregion

            #region IDictionary implementation

            /// <summary>
            /// Determines whether the dictionary object contains an element
            /// with the specified key.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the dictionary contains an element with the
            /// key; otherwise, <b>false</b>.
            /// </returns>
            /// <param name="key">
            /// The key to locate in the dictionary.
            /// </param>
            public bool Contains(object key) {
                return false;
            }

            /// <summary>
            /// Adds an element with the provided key and value to the
            /// dictionary.
            /// </summary>
            /// <param name="value">
            /// The object to use as the value of the element to add.
            /// </param>
            /// <param name="key">
            /// The object to use as the key of the element to add.
            /// </param>
            public void Add(object key, object value) { }

            /// <summary>
            /// Removes all elements from the dictionary.
            /// </summary>
            public void Clear() { }

            /// <summary>
            /// Returns an <b>IDictionaryEnumerator</b> object for the
            /// dictionary.
            /// </summary>
            /// <returns>
            /// An <b>IDictionaryEnumerator</b> object for the dictionary.
            /// </returns>
            IDictionaryEnumerator IDictionary.GetEnumerator() {
                return EMPTY_ENUMERATOR as IDictionaryEnumerator;
            }

            /// <summary>
            /// Removes the element with the specified key from the
            /// dictionary.
            /// </summary>
            /// <param name="key">
            /// The key of the element to remove.
            /// </param>
            public void Remove(object key) { }

            /// <summary>
            /// Gets or sets the element with the specified key.
            /// </summary>
            /// <returns>
            /// The element with the specified key.
            /// </returns>
            /// <param name="key">
            /// The key of the element to get or set.
            /// </param>
            public object this[object key] {
                get { return null; }
                set { }
            }

            /// <summary>
            /// Gets an <b>ICollection</b> object containing the keys of the
            /// dictionary.
            /// </summary>
            /// <returns>
            /// An <b>ICollection</b> object containing the keys of the
            /// dictionary.
            /// </returns>
            public ICollection Keys {
                get { return GetCollection(); }
            }

            /// <summary>
            /// Gets an <b>ICollection</b> object containing the values in the
            /// dictionary.
            /// </summary>
            /// <returns>
            /// An <b>ICollection</b> object containing the values in the
            /// dictionary.
            /// </returns>
            public ICollection Values {
                get { return GetCollection(); }
            }

            /// <summary>
            /// Gets a value indicating whether the dictionary object is
            /// read-only.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the dictionary object is read-only; otherwise,
            /// <b>false</b>.
            /// </returns>
            public bool IsReadOnly {
                get { return true; }
            }

            /// <summary>
            /// Gets a value indicating whether the dictionary object has a
            /// fixed size.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the dictionary object has a fixed size;
            /// otherwise, <b>false</b>.
            /// </returns>
            public bool IsFixedSize {
                get { return true; }
            }

            #endregion

            /// <summary>
            /// Since the cache contains no information, only one ever has to
            /// exist.
            /// </summary>
            public new static readonly ICache Instance = new NullCache();
        }

        #endregion

        #region Inner class: NullObservableCache

        /// <summary>
        /// An immutable <b>IObservableCache</b> which contains nothing.
        /// </summary>
        public class NullObservableCache : NullCache, IObservableCache
        {
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
            public void AddCacheListener(ICacheListener listener)
            {}

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
            public void RemoveCacheListener(ICacheListener listener)
            {}

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
            public void AddCacheListener(ICacheListener listener, object key, bool isLite)
            {}

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
            public void RemoveCacheListener(ICacheListener listener, object key)
            {}

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
            public void AddCacheListener(ICacheListener listener, IFilter filter, bool isLite)
            {}

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
            public void RemoveCacheListener(ICacheListener listener, IFilter filter)
            {}

            #endregion

            /// <summary>
            /// Since the cache contains no information, only one ever has to
            /// exist.
            /// </summary>
            public new static readonly IObservableCache Instance = new NullObservableCache();
        }

        #endregion

        #region Inner class: NullValueExtractor

        /// <summary>
        /// An <b>IValueExtractor</b> that always results in the passed-in
        /// value.
        /// </summary>
        public class NullValueExtractor : IValueExtractor, IPortableObject
        {
            #region IValueExtractor implementation

            /// <summary>
            /// Extract the value from the passed object.
            /// </summary>
            /// <remarks>
            /// The returned value may be <c>null</c>.
            /// </remarks>
            /// <param name="target">
            /// An object to retrieve the value from.
            /// </param>
            /// <returns>
            /// The extracted value as an object; <c>null</c> is an
            /// acceptable value.
            /// </returns>
            /// <exception cref="InvalidCastException">
            /// If this IValueExtractor is incompatible with the passed
            /// object to extract a value from and the implementation
            /// <b>requires</b> the passed object to be of a certain type.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// If this IValueExtractor cannot handle the passed object for
            /// any other reason; an implementor should include a descriptive
            /// message.
            /// </exception>
            public object Extract(object target)
            {
                return target;
            }

            #endregion

            #region IPortableObject implementation

            /// <summary>
            /// Restore the contents of a user type instance by reading its
            /// state using the specified <see cref="IPofReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>IPofReader</b> from which to read the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public void ReadExternal(IPofReader reader)
            {}

            /// <summary>
            /// Save the contents of a POF user type instance by writing its
            /// state using the specified <see cref="IPofWriter"/> object.
            /// </summary>
            /// <param name="writer">
            /// The <b>IPofWriter</b> to which to write the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public void WriteExternal(IPofWriter writer)
            {}

            #endregion

            #region Object override methods

            /// <summary>
            /// Compares this object with another object for equality.
            /// </summary>
            /// <param name="o">
            /// An object reference or <c>null</c>.
            /// </param>
            /// <returns>
            /// <b>true</b> if the passed object reference is of the same
            /// class and has the same state as this object.
            /// </returns>
            public override bool Equals(object o)
            {
                return o is NullValueExtractor;
            }

            /// <summary>
            /// Returns a hash code value for this object.
            /// </summary>
            /// <returns>
            /// A hash code value for this object.
            /// </returns>
            public override int GetHashCode()
            {
                return 42;
            }

            /// <summary>
            /// Provide a human-readable representation of this object.
            /// </summary>
            /// <returns>
            /// A string whose contents represent the value of this object.
            /// </returns>
            public override string ToString()
            {
                return "NullValueExtractor";
            }

            #endregion

            /// <summary>
            /// Since the <b>IValueExtractor</b> contains no information,
            /// only one ever has to exist.
            /// </summary>
            public static readonly NullValueExtractor Instance = new NullValueExtractor();
        }

        #endregion

        #region Inner class: NullConverter

        /// <summary>
        /// An <see cref="IConverter"/> that does nothing.
        /// </summary>
        /// <author>Cameron Purdy  2002.02.08</author>
        public class NullConverter : IConverter
        {
            #region Constructors

            /// <summary>
            /// Off-limits constructor.
            /// </summary>
            private NullConverter()
            {}

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
                return o;
            }

            #endregion

            /// <summary>
            /// Since the <see cref="IConverter"/> contains no information,
            /// only one ever has to exist.
            /// </summary>
            public static readonly NullConverter Instance = new NullConverter();
        }

        #endregion

        #region Inner class: NullPofContext

        /// <summary>
        /// An implementation of IPofContext that does nothing.
        /// </summary>
        public class NullPofContext : IPofContext
        {
            #region Constructors

            /// <summary>
            /// Default constructor.
            /// </summary>
            private NullPofContext()
            {
            }

            #endregion

            #region IPofContext implementation

            /// <summary>
            /// Return an <see cref="IPofSerializer"/> that can be used to
            /// serialize and deserialize an object of the specified user type to
            /// and from a POF stream.
            /// </summary>
            /// <param name="typeId">
            /// The type identifier of the user type that can be serialized and
            /// deserialized using the returned <b>IPofSerializer</b>; must be
            /// non-negative.
            /// </param>
            /// <returns>
            /// An <b>IPofSerializer</b> for the specified user type.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// If the specified user type is negative or unknown to this
            /// <b>IPofContext</b>.
            /// </exception>
            public IPofSerializer GetPofSerializer(int typeId)
            {
                throw new ArgumentException();
            }

            /// <summary>
            /// Determine the user type identifier associated with the given
            /// object.
            /// </summary>
            /// <param name="o">
            /// An instance of a user type; must not be <c>null</c>.
            /// </param>
            /// <returns>
            /// The type identifier of the user type associated with the given
            /// object.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// If the user type associated with the given object is unknown to
            /// this <b>IPofContext</b>.
            /// </exception>
            public int GetUserTypeIdentifier(Object o)
            {
                throw new ArgumentException();
            }

            /// <summary>
            /// Determine the user type identifier associated with the given
            /// type.
            /// </summary>
            /// <param name="type">
            /// A user type; must not be <c>null</c>.
            /// </param>
            /// <returns>
            /// The type identifier of the user type associated with the given
            /// type.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// If the user type associated with the given type is unknown to
            /// this <b>IPofContext</b>.
            /// </exception>
            public int GetUserTypeIdentifier(Type type)
            {
                throw new ArgumentException();
            }

            /// <summary>
            /// Determine the user type identifier associated with the given type
            /// name.
            /// </summary>
            /// <param name="typeName">
            /// The name of a user type; must not be <c>null</c>.
            /// </param>
            /// <returns>
            /// The type identifier of the user type associated with the given
            /// type name.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// If the user type associated with the given type name is unknown
            /// to this <b>IPofContext</b>.
            /// </exception>
            public int GetUserTypeIdentifier(String typeName)
            {
                throw new ArgumentException();
            }

            /// <summary>
            /// Determine the name of the type associated with a user type
            /// identifier.
            /// </summary>
            /// <param name="typeId">
            /// The user type identifier; must be non-negative.
            /// </param>
            /// <returns>
            /// The name of the type associated with the specified user type
            /// identifier.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// If the specified user type is negative or unknown to this
            /// <b>IPofContext</b>.
            /// </exception>
            public String GetTypeName(int typeId)
            {
                throw new ArgumentException();
            }

            /// <summary>
            /// Determine the type associated with the given user type
            /// identifier.
            /// </summary>
            /// <param name="typeId">
            /// The user type identifier; must be non-negative.
            /// </param>
            /// <returns>
            /// The type associated with the specified user type identifier.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// If the specified user type is negative or unknown to this
            /// <b>IPofContext</b>.
            /// </exception>
            public Type GetType(int typeId)
            {
                throw new ArgumentException();
            }

            /// <summary>
            /// Determine if the given object is of a user type known to this
            /// <b>IPofContext</b>.
            /// </summary>
            /// <param name="o">
            /// The object to test; must not be <c>null</c>.
            /// </param>
            /// <returns>
            /// <b>true</b> iff the specified object is of a valid user type.
            /// </returns>
            public bool IsUserType(Object o)
            {
                return false;
            }

            /// <summary>
            /// Determine if the given type is a user type known to this
            /// <b>IPofContext</b>.
            /// </summary>
            /// <param name="type">
            /// The type to test; must not be <c>null</c>.
            /// </param>
            /// <returns>
            /// <b>true</b> iff the specified type is a valid user type.
            /// </returns>
            public bool IsUserType(Type type)
            {
                return false;
            }

            /// <summary>
            /// Determine if the type with the given name is a user type known to
            /// this <b>IPofContext</b>.
            /// </summary>
            /// <param name="typeName">
            /// The name of the type to test; must not be <c>null</c>.
            /// </param>
            /// <returns>
            /// <b>true</b> iff the type with the specified name is a valid user
            /// type.
            /// </returns>
            public bool IsUserType(String typeName)
            {
                return false;
            }

            #endregion

            #region ISerializer implementation

            /// <summary>
            /// Serialize an object to a stream by writing its state using the
            /// specified <see cref="DataWriter"/> object.
            /// </summary>
            /// <param name="writer">
            /// The <b>DataWriter</b> with which to write the object's state.
            /// </param>
            /// <param name="o">
            /// The object to serialize.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public void Serialize(DataWriter writer, Object o)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Deserialize an object from a stream by reading its state using
            /// the specified <see cref="DataReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>DataReader</b> with which to read the object's state.
            /// </param>
            /// <returns>
            /// The deserialized user type instance.
            /// </returns>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public object Deserialize(DataReader reader)
            {
                throw new NotSupportedException();
            }

            #endregion

            /// <summary>
            /// Singleton instance.
            /// </summary>
            public static readonly NullPofContext Instance = new NullPofContext();
        }

        #endregion
    }
}