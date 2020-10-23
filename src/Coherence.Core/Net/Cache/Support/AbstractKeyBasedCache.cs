/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Text;

using Tangosol.Util.Collections;

namespace Tangosol.Net.Cache.Support
{
    /// <summary>
    /// <b>AbstractKeyBasedCache</b> is a base class for <see cref="ICache"/>
    /// implementations.
    /// </summary>
    /// <remarks>
    /// <p>
    /// AbstractKeyBasedCache requires a read-only sub-type to implement only
    /// <see cref="Get"/> and <see cref="GetKeysEnumerator"/> methods, and a
    /// read-write sub-type to additionally implement only
    /// <see cref="Insert(object, object, long)"/> and
    /// <see cref="Remove"/>.</p>
    /// <p>
    /// A number of the methods have implementations provided, but are
    /// extremely inefficient for caches that contain large amounts of data,
    /// including <see cref="Clear"/>, <see cref="Contains"/> and
    /// <see cref="Count"/>. Furthermore, if any of a number of method
    /// implementations has any cost of returning an "old value", then the
    /// <see cref="InsertAll"/> and <see cref="RemoveBlind"/> methods should
    /// also be implemented.</p>
    /// </remarks>
    /// <author>Cameron Purdy  2005.07.13</author>
    /// <author>Ana Cikic  2006.11.27</author>
    public abstract class AbstractKeyBasedCache : ICache
    {
        #region Properties

        /// <summary>
        /// Returns the number of key-value mappings in this cache.
        /// </summary>
        /// <value>
        /// The number of key-value mappings in this cache.
        /// </value>
        public virtual int Count
        {
            get
            {
                // this begs for sub-class optimization
                int c = 0;
                for (IEnumerator keyEnumerator = GetKeysEnumerator(); keyEnumerator.MoveNext(); )
                {
                    ++c;
                }
                return c;
            }
        }

        /// <summary>
        /// Returns a collection of the mappings contained in this cache.
        /// </summary>
        /// <remarks>
        /// Each element in the returned collectioin is an
        /// <see cref="Entry"/>.
        /// </remarks>
        /// <value>
        /// A collection of the mappings contained in this cache.
        /// </value>
        public virtual ICollection Entries
        {
            get
            {
                // no need to synchronize; it is acceptable that two threads would
                // instantiate an entries collection
                ICollection col = m_entries;
                if (col == null)
                {
                    m_entries = col = InstantiateEntriesCollection();
                }
                return col;
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <b>ICollection</b>
        /// is synchronized (thread safe).
        /// </summary>
        /// <value>
        /// Always <b>false</b> for this cache.
        /// </value>
        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the <b>IDictionary</b> object is
        /// read-only.
        /// </summary>
        /// <value>
        /// Always <b>false</b> for this cache.
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
        /// Always <b>false</b> for this cache.
        /// </value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Returns a collection view of the keys contained in this cache.
        /// </summary>
        /// <value>
        /// A colleciton of the keys contained in this cache.
        /// </value>
        public virtual ICollection Keys
        {
            get
            {
                // no need to synchronize; it is acceptable that two threads would
                // instantiate a key set
                ICollection col = m_keys;
                if (col == null)
                {
                    m_keys = col = InstantiateKeysCollection();
                }
                return col;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the
        /// <b>ICollection</b>.
        /// </summary>
        /// <value>
        /// An object that can be used to synchronize access to the
        /// <b>ICollection"></b>.
        /// </value>
        public virtual object SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// Returns a collection of the values contained in this cache.
        /// </summary>
        /// <value>
        /// A collection of the values contained in this cache.
        /// </value>
        public virtual ICollection Values
        {
            get
            {
                // no need to synchronize; it is acceptable that two threads would
                // instantiate a key set
                ICollection col = m_values;
                if (col == null)
                {
                    m_values = col = InstantiateValuesCollection();
                }
                return col;
            }
        }

        /// <summary>
        /// Returns the value to which this cache maps the specified key.
        /// </summary>
        /// <param name="key">
        /// The key object.
        /// </param>
        /// <value>
        /// The value to which this cache maps the specified key, or
        /// <c>null</c> if the cache contains no mapping for this key.
        /// </value>
        public virtual object this[object key]
        {
            get { return Get(key); }
            set { Insert(key, value); }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Returns the value for the specified key.
        /// </summary>
        /// <param name="key">
        /// Key whose value is returned.
        /// </param>
        /// <returns>
        /// Value from the cache for the specified key.
        /// </returns>
        protected abstract object Get(object key);

        /// <summary>
        /// Create an <b>IEnumerator</b> over the keys in this cache.
        /// </summary>
        /// <returns>
        /// A new instance of an <b>IEnumerator</b> over the keys in this
        /// cache.
        /// </returns>
        protected abstract IEnumerator GetKeysEnumerator();

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
        /// <b>true</b> iff the cache changed as the result of this
        /// operation.
        /// </returns>
        protected virtual bool RemoveBlind(object key)
        {
            if (Contains(key))
            {
                Remove(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region ICollection, IDictionary and ICache implementation

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
            Insert(key, value);
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
        public virtual bool Contains(object key)
        {
            // this begs for sub-class optimization
            for (IEnumerator keyEnumerator = GetKeysEnumerator(); keyEnumerator.MoveNext(); )
            {
                if (Equals(key, keyEnumerator.Current))
                {
                    return true;
                }
            }
            return false;
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
            Entries.CopyTo(array, index);
        }

        /// <summary>
        /// Clear all key/value mappings.
        /// </summary>
        public virtual void Clear()
        {
            object[] array = new object[Count];
            int      i     = 0;
            for (IEnumerator keyEnumerator = GetKeysEnumerator(); keyEnumerator.MoveNext(); )
            {
                array[i++] = keyEnumerator.Current;
            }

            // this begs for sub-class optimization
            foreach (object key in array)
            {
                Remove(key);
            }
        }

        /// <summary>
        /// Get all the specified keys, if they are in the cache.
        /// </summary>
        /// <remarks>
        /// For each key that is in the cache, that key and its corresponding
        /// value will be placed in the dictionary that is returned by this
        /// method. The absence of a key in the returned dictionary indicates
        /// that it was not in the cache, which may imply (for caches that
        /// can load behind the scenes) that the requested data could not be
        /// loaded.
        /// </remarks>
        /// <param name="keys">
        /// A collection of keys that may be in the named cache.
        /// </param>
        /// <returns>
        /// An <b>IDictionary</b> of keys to values for the specified keys
        /// passed in <paramref name="keys"/>.
        /// </returns>
        public virtual IDictionary GetAll(ICollection keys)
        {
            IDictionary dictionary = new HashDictionary();
            foreach (object key in keys)
            {
                object value = this[key];
                if (value != null || Contains(key))
                {
                    dictionary[key] = value;
                }
            }
            return dictionary;
        }

        /// <summary>
        /// Returns an <see cref="ICacheEnumerator"/> object for the
        /// <b>ICache</b> instance.
        /// </summary>
        /// <returns>An <b>ICacheEnumerator</b> object for the
        /// <b>ICache</b> instance.</returns>
        public virtual ICacheEnumerator GetEnumerator()
        {
            return new Enumerator(this);
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
        /// Associates the specified value with the specified key in this
        /// cache.
        /// </summary>
        /// <param name="key">
        /// Key with which the specified value is to be associated.
        /// </param>
        /// <param name="value">
        /// Value to be associated with the specified key.
        /// </param>
        /// <returns>
        /// Previous value associated with specified key, or <c>null</c> if
        /// there was no mapping for key.
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
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies all of the mappings from the specified <b>IDictionary</b>
        /// to this cache.
        /// </summary>
        /// <remarks>
        /// The effect of this call is equivalent to that of calling
        /// <see cref="Insert(object, object)"/> on this cache once for each
        /// mapping in the passed dictionary. The behavior of this operation
        /// is unspecified if the passed dictionary is modified while the
        /// operation is in progress.
        /// </remarks>
        /// <param name="dictionary">
        /// The <b>IDictionary</b> containing the key/value pairings to
        /// insert into this cache.
        /// </param>
        public virtual void InsertAll(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                Insert(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Removes the mapping for this key from this cache if present.
        /// </summary>
        /// <remarks>
        /// Expensive: updates both the underlying cache and the local cache.
        /// </remarks>
        /// <param name="key">
        /// Key whose mapping is to be removed from the cache.
        /// </param>
        public virtual void Remove(object key)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compares the specified object with this cache for equality.
        /// </summary>
        /// <remarks>
        /// Returns <b>true</b> if the given object is also a cache and the
        /// two caches represent the same mappings. More formally, two caches
        /// <pre>t1</pre> and <pre>t2</pre> represent the same mappings if
        /// <pre>t1.Keys.Equals(t2.Keys)</pre> and for every key <pre>k</pre>
        /// in <pre>t1.Keys</pre>, <pre> (t1[k]==null ? t2[k]==null :
        /// t1[k].Equals(t2[k]) </pre>. This ensures that the <b>Equals</b>
        /// method works properly across different implementations of the
        /// cache interface.
        /// </remarks>
        /// <param name="o">
        /// Object to be compared for equality with this cache.
        /// </param>
        /// <returns>
        /// <b>true</b> if the specified object is equal to this cache.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is ICache)
            {
                ICache that = (ICache) o;
                if (this == that)
                {
                    return true;
                }

                if (Count == that.Count)
                {
                    foreach (ICacheEntry entry in Entries)
                    {
                        object key       = entry.Key;
                        object thisValue = entry.Value;
                        object thatValue;
                        try
                        {
                            thatValue = that[key];
                        }
                        catch (InvalidCastException)
                        {
                            return false;
                        }
                        catch (NullReferenceException)
                        {
                            return false;
                        }

                        if (thisValue == this || thisValue == that)
                        {
                            // this could be infinite recursion
                            if (thatValue != this && thatValue != that)
                            {
                                // it is not safe to call equals(); it would
                                // likely lead to infinite recursion
                                return false;
                            }
                        }
                        else if (!Equals(thisValue, thatValue) || thatValue == null && !that.Contains(key))
                        {
                            return false;
                        }
                    }

                    // size is identical and all entries match
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code value for this cache.
        /// </summary>
        /// <remarks>
        /// The hash code of a cache is defined to be the sum of the hash
        /// codes of each entry in the cache's <b>Entries</b>. This ensures
        /// that <pre>t1.Equals(t2)</pre> implies that
        /// <pre>t1.GetHashCode()==t2.GetHashCode()</pre> for any two caches
        /// <pre>t1</pre> and <pre>t2</pre>, as required by the general
        /// contract of <b>object.GetHashCode</b>.
        /// </remarks>
        /// <returns>
        /// The hash code value for this cache.
        /// </returns>
        public override int GetHashCode()
        {
            int hash = 0;
            foreach (ICacheEntry entry in Entries)
            {
                hash += entry.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Returns a string representation of this cache.
        /// </summary>
        /// <remarks>
        /// The string representation consists of a list of key-value
        /// mappings in the order returned by the cache's <b>Entries</b>
        /// enumerator, enclosed in braces (<pre>"{}"</pre>).  Adjacent
        /// mappings are separated by the characters <pre>", "</pre> (comma
        /// and space). Each key-value mapping is rendered as the key
        /// followed by an equals sign (<pre>"="</pre>) followed by the
        /// associated value.  Keys and values are converted to strings as by
        /// <pre>object.ToString()</pre>.
        /// </remarks>
        /// <returns>
        /// A string representation of this cache.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100 + (Count << 3));
            sb.Append('{');

            bool isFirst = true;
            foreach (ICacheEntry entry in Entries)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append(", ");
                }

                object key   = entry.Key;
                object value = entry.Value;

                // detect the condition in which case this
                // cache is used as a key and/or a value inside itself, which
                // would result in infinite recursion
                sb.Append(key == this ? "(this cache)" : key.ToString())
                    .Append('=')
                    .Append(value == this ? "(this cache)" : value.ToString());
            }

            sb.Append('}');
            return sb.ToString();
        }

        #endregion

        #region Factory methods

        /// <summary>
        /// Factory pattern: Create a collection that represents the keys in
        /// the cache.
        /// </summary>
        /// <returns>
        /// A new instance of <b>ICollection</b> that represents the keys in
        /// the cache.
        /// </returns>
        protected virtual ICollection InstantiateKeysCollection()
        {
            return new KeyBasedCollection(this, KeyBasedCollectionType.Keys);
        }

        /// <summary>
        /// Factory pattern: Create a collection that represents the entries
        /// in the cache.
        /// </summary>
        /// <returns>
        /// A new instance of <b>ICollection</b> that represents the entries
        /// in the cache.
        /// </returns>
        protected virtual ICollection InstantiateEntriesCollection()
        {
            return new KeyBasedCollection(this, KeyBasedCollectionType.Entries);
        }

        /// <summary>
        /// Factory pattern: Instantiate the values collection.
        /// </summary>
        /// <returns>
        /// A new instance of <b>ICollection</b> that represents this cache's
        /// values.
        /// </returns>
        protected virtual ICollection InstantiateValuesCollection()
        {
            return new KeyBasedCollection(this, KeyBasedCollectionType.Values);
        }

        #endregion

        #region Inner class: Entry

        /// <summary>
        /// <see cref="CacheEntry"/> implementation for this cache.
        /// </summary>
        internal struct Entry : ICacheEntry
        {
            /// <summary>
            /// Creates an instance of a <b>Entry</b>.
            /// </summary>
            /// <param name="key">
            /// Cache entry <b>key</b>.
            /// </param>
            /// <param name="value">
            /// Cache entry <b>value</b>.
            /// </param>
            public Entry(object key, object value)
            {
                m_key   = key; 
                m_value = value;
                m_cache = null;
            }

            /// <summary>
            /// Creates an instance of a <b>Entry</b>.
            /// </summary>
            /// <param name="cache">
            /// Parent <b>AbstractKeyBasedCache</b>.
            /// </param>
            /// <param name="key">
            /// Cache entry <b>key</b>.
            /// </param>
            /// <param name="value">
            /// Cache entry <b>value</b>.
            /// </param>
            public Entry(AbstractKeyBasedCache cache, object key, object value) 
                    : this(key, value)
            {
                m_cache = cache;
            }

            public object Key
            {
                get { return m_key; }
            }

            /// <summary>
            /// Gets or sets the value for this cache entry.
            /// </summary>
            /// <value>
            /// The <b>value</b> for this cache entry.
            /// </value>
            public object Value
            {
                get
                {
                    object value = m_value;
                    if (value == null)
                    {
                        value = m_cache[Key];
                        m_value = value;
                    }
                    return value;
                }
                set
                {
                    m_cache[Key] = value;
                    m_value = value;
                }
            }

            #region Object override methods

            /// <summary>
            /// Generates hash code for this <b>CacheEntry.</b>
            /// </summary>
            /// <returns>
            /// A hash code for this <b>CacheEntry.</b>
            /// </returns>
            public override int GetHashCode()
            {
                return m_key.GetHashCode();
            }

            /// <summary>
            /// Checks two cache entries for equality.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                if (!(obj is Entry))
                {
                    return false;
                }

                Entry cacheEntry = (Entry)obj;
                return m_key == cacheEntry.m_key
                    && m_value == cacheEntry.m_value;
            }

            /// <summary>
            /// Returns a <see cref="T:System.String"/> that represents
            /// the current <b>CacheEntry</b>.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.String"/> that represents the current
            /// <b>CacheEntry</b>.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override string ToString()
            {
                return "Entry("
                       + "Key = " + Key
                       + ", Value = " + Value
                       + ')';
            }

            #endregion

            #region Conversion Operators

            /// <summary>
            /// Converts CacheEntry to <b>DictionaryEntry</b>.
            /// </summary>
            /// <param name="entry">
            /// CacheEntry.
            /// </param>
            /// <returns>
            /// <b>DictionaryEntry</b> with key and value extracted from
            /// specified CacheEntry.
            /// </returns>
            public static implicit operator DictionaryEntry(Entry entry)
            {
                return new DictionaryEntry(entry.Key, entry.Value);
            }

            /// <summary>
            /// Converts <b>DictionaryEntry</b> to CacheEntry.
            /// </summary>
            /// <param name="entry">
            /// <b>DictionaryEntry</b>.
            /// </param>
            /// <returns>
            /// CacheEntry with key and value extracted from specified
            /// <b>DictionaryEntry</b>.
            /// </returns>
            public static implicit operator Entry(DictionaryEntry entry)
            {
                return new Entry(entry.Key, entry.Value);
            }
            #endregion

            #region Data Members

            /// <summary>
            /// Parent AbstractKeyBasedCache.
            /// </summary>
            internal AbstractKeyBasedCache m_cache;
            
            /// <summary>
            /// The key for this entry.
            /// </summary>
            private readonly object m_key;
            
            /// <summary>
            /// The value for this entry. 
            /// </summary>
            private object m_value;

            #endregion
        }

        #endregion

        #region Enum: KeyBasedCollectionType

        /// <summary>
        /// AbstractKeyBasedCache collection type enumeration.
        /// </summary>
        internal enum KeyBasedCollectionType
        {
            Entries,
            Keys,
            Values
        }

        #endregion

        #region Inner class: KeyBasedCollection

        /// <summary>
        /// Represents collection of keys, values or entries in the
        /// <b>AbstractKeyBasedCache</b>.
        /// </summary>
        internal class KeyBasedCollection : ICollection
        {
            #region Constructors

            /// <summary>
            /// Create new instance of KeyBasedCollection.
            /// </summary>
            /// <param name="parent">
            /// Parent AbstractKeyBasedCache.
            /// </param>
            /// <param name="type">
            /// Collection type, one of the
            /// <see cref="KeyBasedCollectionType"/> values.
            /// </param>
            public KeyBasedCollection(AbstractKeyBasedCache parent, KeyBasedCollectionType type)
            {
                m_cache = parent;
                m_type  = type;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets the number of elements contained in the collection.
            /// </summary>
            /// <value>
            /// The number of elements contained in the collection.
            /// </value>
            public virtual int Count
            {
                get { return m_cache.Count; }
            }

            /// <summary>
            /// Gets an object that can be used to synchronize access to the
            /// collection.
            ///</summary>
            /// <value>
            /// An object that can be used to synchronize access to the
            /// collection.
            /// </value>
            public virtual object SyncRoot
            {
                get { return m_cache.SyncRoot; }
            }

            /// <summary>
            /// Gets a value indicating whether access to the collection is
            /// synchronized (thread safe).
            /// </summary>
            /// <value>
            /// <b>true</b> if access to the collection is synchronized
            /// (thread safe); otherwise, <b>false</b>.
            /// </value>
            public virtual bool IsSynchronized
            {
                get { return m_cache.IsSynchronized; }
            }

            #endregion

            #region ICollection implementation

            /// <summary>
            /// Copies the elements of the collection to an array, starting
            /// at a particular index.
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
            /// Array is multidimensional -or- the number of elements in the
            /// source collection is greater than the available space from
            /// index to the end of the destination array.
            /// </exception>
            public virtual void CopyTo(Array array, int index)
            {
                foreach (object o in this)
                {
                    array.SetValue(o, index++);
                }
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="IEnumerator"/> object that can be used to
            /// iterate through the collection.
            /// </returns>
            public virtual IEnumerator GetEnumerator()
            {
                return new KeyBasedEnumerator(m_cache, m_type);
            }

            #endregion

            #region Data members

            /// <summary>
            /// Parent AbstractKeyBasedCache.
            /// </summary>
            protected AbstractKeyBasedCache m_cache;

            /// <summary>
            /// The KeyBasedCollection type.
            /// </summary>
            protected KeyBasedCollectionType m_type;

            #endregion

            #region Inner class: KeyBasedEnumerator

            /// <summary>
            /// <b>IEnumerator</b> implementation for KeyBasedCollection.
            /// </summary>
            internal class KeyBasedEnumerator : IEnumerator
            {
                #region Constructors

                /// <summary>
                /// Sets <see cref="AbstractKeyBasedCache"/> that created the
                /// parent KeyBasedCollection.
                /// </summary>
                /// <param name="cache">
                /// Sets <b>AbstractKeyBasedCache</b> that created the parent
                /// KeyBasedCollection.
                /// </param>
                /// <param name="type">
                /// Type of the collection, one of the
                /// <see cref="KeyBasedCollectionType"/> values.
                /// </param>
                public KeyBasedEnumerator(AbstractKeyBasedCache cache, KeyBasedCollectionType type)
                {
                    m_cache         = cache;
                    m_type          = type;
                    m_keyEnumerator = cache.GetKeysEnumerator();
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
                /// The enumerator is positioned before the first element of
                /// the collection or after the last element.
                /// </exception>
                public virtual object Current
                {
                    get
                    {
                        object key = m_keyEnumerator.Current;
                        switch (m_type)
                        {
                            case KeyBasedCollectionType.Keys:
                                return key;
                            case KeyBasedCollectionType.Values:
                                return m_cache[key];
                            default:
                                return new Entry(m_cache, key, m_cache[key]);
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
                /// <b>true</b> if the enumerator was successfully advanced
                /// to the next element; <b>false</b> if the enumerator has
                /// passed the end of the collection.
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

                #region Data members

                /// <summary>
                /// An iterator over the keys returned by
                /// AbstractKeyBasedCache.GetKeysEnumerator().
                /// </summary>
                [NonSerialized]
                protected IEnumerator m_keyEnumerator;

                /// <summary>
                /// The AbstractKeyBasedCache that created the parent
                /// collection.
                /// </summary>
                protected AbstractKeyBasedCache m_cache;

                /// <summary>
                /// The KeyBasedCollection type.
                /// </summary>
                protected KeyBasedCollectionType m_type;

                #endregion
            }

            #endregion
        }

        #endregion

        #region Inner class: Enumerator

        /// <summary>
        /// <b>IEnumerator</b> implementation for AbstractKeysBasedCache
        /// entries collection.
        /// </summary>
        internal class Enumerator : ICacheEnumerator
        {
            #region Constructors

            /// <summary>
            /// Sets parent <see cref="AbstractKeyBasedCache"/>.
            /// </summary>
            /// <param name="cache">
            /// Sets parent <b>AbstractKeyBasedCache</b>.
            /// </param>
            public Enumerator(AbstractKeyBasedCache cache)
            {
                m_cache         = cache;
                m_keyEnumerator = cache.Keys.GetEnumerator();
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
                    object key = Key = m_keyEnumerator.Current;
                    return new CacheEntry(key, m_cache[key]);
                }
            }

            #endregion

            #region ICacheEnumerator implementation

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
                return m_keyEnumerator.MoveNext();
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
                m_keyEnumerator.Reset();
            }

            #endregion

            #region Data members

            /// <summary>
            /// Last key that was iterated.
            /// </summary>
            protected object m_key;

            /// <summary>
            /// An iterator over the keys returned by
            /// AbstractKeyBasedCache.Keys.
            /// </summary>
            [NonSerialized]
            protected IEnumerator m_keyEnumerator;

            /// <summary>
            /// The parent AbstractKeyBasedCache.
            /// </summary>
            protected AbstractKeyBasedCache m_cache;

            #endregion
        }

        #endregion

        #region Data members

        /// <summary>
        /// The keys collection for this cache; lazily instantiated.
        /// </summary>
        [NonSerialized]
        private ICollection m_keys;

        /// <summary>
        /// The entries collection for this cache; lazily instantiated.
        /// </summary>
        [NonSerialized]
        private ICollection m_entries;

        /// <summary>
        /// The values collection for this cache; lazily instantiated.
        /// </summary>
        [NonSerialized]
        private ICollection m_values;

        #endregion
    }
}