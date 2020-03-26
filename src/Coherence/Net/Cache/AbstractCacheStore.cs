/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    /// <summary>
    ///  An abstract base class for the <see cref="ICacheStore"/>.
    /// </summary>
    /// <author>Cameron Purdy  2003.05.29</author>
    /// <author>Jason Howes  2005.09.01</author>
    /// <author>Ivan Cikic  2007.05.21</author>
    public abstract class AbstractCacheStore : AbstractCacheLoader, ICacheStore
    {
        #region ICacheStore interface implementation

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
        /// <exception cref="NotSupportedException">
        /// If this implementation or the underlying store is read-only.
        /// </exception>
        public void Store(object key, object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Store the specified values under the specified keys in the
        /// underlying store.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method is intended to support both key/value creation
        /// and value update for the specified keys.</p>
        /// <p>
        /// The implementation of this method calls <see cref="Store"/> for
        /// each entry in the supplied <b>IDictionary</b>. Once stored 
        /// successfully, an entry is removed from the <b>IDictionary</b>
        /// (if possible).</p>
        /// <p>
        /// <b>Note:</b>
        /// For many types of persistent stores, a single store operation is 
        /// as expensive as a bulk store operation; therefore, subclasses 
        /// should override this method if possible.</p>
        /// </remarks>
        /// <param name="dictionary">
        /// An <see cref="IDictionary"/> of any number of keys and values
        /// to store.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// If this implementation or the underlying store is read-only.
        /// </exception>
        public void StoreAll(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                Store(entry.Key, entry.Value);
            }
            try
            {
                dictionary.Clear();    
            }
            catch (NotSupportedException) {}
        }

        /// <summary>
        /// Remove the specified key from the underlying store if present.
        /// </summary>
        /// <param name="key">
        /// Key whose mapping is being removed from the cache.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// If this implementation or the underlying store is read-only.
        /// </exception>
        public void Erase(object key)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Remove the specified keys from the underlying store if present.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The implementation of this method calls <see cref="Erase"/> for 
        /// each key in the supplied <b>ICollection</b>. Once erased 
        /// successfully, a key is removed from the <b>ICollection</b>
        /// (if possible).</p>
        /// <p>
        /// <b>Note:</b>
        /// For many types of persistent stores, a single erase operation is
        /// as expensive as a bulk erase operation; therefore, subclasses 
        /// should override this method if possible.</p>
        /// </remarks>
        /// <param name="keys">
        /// Keys whose mappings are being removed from the cache.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// If this implementation or the underlying store is read-only.
        /// </exception>
        public void EraseAll(ICollection keys)
        {
            foreach (object key in keys)
            {
                Erase(key);
            }
            try
            {
                CollectionUtils.Clear(keys);
            }
            catch (NotSupportedException) {}
        }
    
        #endregion
    }
}
