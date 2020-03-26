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
    /// A cache store.
    /// </summary>
    /// <author>Cameron Purdy  2003.05.29</author>
    /// <author>Goran Milosavljevic  2006.09.07</author>
    /// <since>Coherence 2.2</since>
    /// <seealso cref="ICacheLoader"/>
    public interface ICacheStore : ICacheLoader
    {
        /// <summary>
        /// Store the specified value under the specified key in the
        /// underlying store.
        /// </summary>
        /// <remarks>
        /// This method is intended to support both key/value creation
        /// and value update for a specific key.
        /// </remarks>
        /// <param name="key">
        /// Key to store the value under.
        /// </param>
        /// <param name="value">
        /// Value to be stored.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If this implementation or the underlying store is read-only.
        /// </exception>
        void Store(object key, object value);

        /// <summary>
        /// Store the specified values under the specified keys in the
        /// underlying store.
        /// </summary>
        /// <remarks>
        /// This method is intended to support both key/value creation
        /// and value update for the specified keys.
        /// </remarks>
        /// <param name="dictionary">
        /// An <see cref="IDictionary"/> of any number of keys and values
        /// to store.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If this implementation or the underlying store is read-only.
        /// </exception>
        void StoreAll(IDictionary dictionary);

        /// <summary>
        /// Remove the specified key from the underlying store if present.
        /// </summary>
        /// <param name="key">
        /// Key whose mapping is being removed from the cache.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If this implementation or the underlying store is read-only.
        /// </exception>
        void Erase(object key);

        /// <summary>
        /// Remove the specified keys from the underlying store if present.
        /// </summary>
        /// <param name="keys">
        /// Keys whose mappings are being removed from the cache.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If this implementation or the underlying store is read-only.
        /// </exception>
        void EraseAll(ICollection keys);
    }
}