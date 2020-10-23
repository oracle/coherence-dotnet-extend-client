/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// An <b>ICache</b> is a dictionary that supports caching.
    /// </summary>
    /// <author>Gene Gleyzer  2004.01.05</author>
    /// <author>Aleksandar Seovic  2006.07.11</author>
    public interface ICache : IDictionary
    {
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
        IDictionary GetAll(ICollection keys);

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
        object Insert(object key, object value);

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
        object Insert(object key, object value, long millis);

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
        void InsertAll(IDictionary dictionary);

        /// <summary>
        /// Gets a collection of <see cref="ICacheEntry"/> instances
        /// within the cache.
        /// </summary>
        ICollection Entries { get; }

        /// <summary>
        /// Returns an <see cref="ICacheEnumerator"/> object for the
        /// <b>ICache</b> instance.
        /// </summary>
        /// <returns>An <b>ICacheEnumerator</b> object for the
        /// <b>ICache</b> instance.</returns>
        new ICacheEnumerator GetEnumerator();
    }

    /// <summary>
    /// Enumerates cache elements.
    /// </summary>
    public interface ICacheEnumerator : IDictionaryEnumerator
    {
        /// <summary>
        /// Gets both the key and the value of the current cache entry.
        /// </summary>
        /// <value>
        /// An <see cref="ICacheEntry"/> containing both the key and
        /// the value of the current cache entry.
        /// </value>
        new ICacheEntry Entry { get; }
    }

    /// <summary>
    /// A cache entry (key-value pair).
    /// </summary>
    public interface ICacheEntry
    {
        /// <summary>
        /// Gets the key corresponding to this entry.
        /// </summary>
        /// <value>
        /// The key corresponding to this entry; may be <c>null</c> if the
        /// underlying dictionary supports <c>null</c> keys.
        /// </value>
        object Key { get; }

        /// <summary>
        /// Gets or sets the value corresponding to this entry.
        /// </summary>
        /// <value>
        /// The value corresponding to this entry; may be <c>null</c> if the
        /// value is <c>null</c> or if the entry does not exist in the
        /// cache.
        /// </value>
        object Value { get; set; }
    }
}