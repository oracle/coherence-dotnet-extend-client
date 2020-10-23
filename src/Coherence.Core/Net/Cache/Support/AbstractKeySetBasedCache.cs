/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Util;

namespace Tangosol.Net.Cache.Support
{
    /// <summary>
    /// <b>AbstractKeySetBasedCache</b> is an extension to the
    /// <see cref="AbstractKeyBasedCache"/> that has a full awareness of the
    /// set of keys upon which the cache is based.
    /// </summary>
    /// <remarks>
    /// <p>
    /// As a result, it is possible to optimize the implementation of a
    /// number of methods that benefit from a knowledge of the entire set of
    /// keys.</p>
    /// <p>
    /// Read-only implementations must implement
    /// <see cref="GetInternalKeysCollection"/> and
    /// <see cref="AbstractKeyBasedCache.Get"/>.
    /// Read/write implementations must additionally implement
    /// <see cref="AbstractKeyBasedCache.Insert(object, object, long)"/> and
    /// <see cref="AbstractKeyBasedCache.Remove"/>.
    /// If the implementation has any cost of returning an "old value", then
    /// the <see cref="AbstractKeyBasedCache.InsertAll"/> and
    /// <see cref="AbstractKeyBasedCache.RemoveBlind"/>
    /// methods should also be implemented. The only other obvious method for
    /// optimization is <see cref="AbstractKeyBasedCache.Clear"/>, if the
    /// implementation is able to do it in bulk.</p>
    /// </remarks>
    /// <author>Cameron Purdy  2005.09.20</author>
    /// <author>Ana Cikic  2006.11.27</author>
    public abstract class AbstractKeySetBasedCache : AbstractKeyBasedCache
    {
        #region AbstractKeyBasedCache override methods

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
            return CollectionUtils.Contains(GetInternalKeysCollection(), key);
        }

        /// <summary>
        /// Returns the number of key-value mappings in this cache.
        /// </summary>
        /// <value>
        /// The number of key-value mappings in this cache.
        /// </value>
        public override int Count
        {
            get { return GetInternalKeysCollection().Count; }
        }

        /// <summary>
        /// Create an <b>IEnumerator</b> over the keys in this cache.
        /// </summary>
        /// <remarks>
        /// Note that this implementation delegates back to the keys
        /// collection, while the super type delegates from the keys
        /// collection to this method.
        /// </remarks>
        /// <returns>
        /// A new instance of an enumerator over the keys in this cache.
        /// </returns>
        protected override IEnumerator GetKeysEnumerator()
        {
            return InstantiateKeyEnumerator();
        }

        #endregion

        #region Internal methods

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
        protected abstract ICollection GetInternalKeysCollection();

        /// <summary>
        /// Factory pattern: Create an <b>IEnumerator</b> over the keys in
        /// the cache.
        /// </summary>
        /// <returns>
        /// A new instance of <b>IEnumerator</b> that iterates over the keys
        /// in the cache.
        /// </returns>
        protected virtual IEnumerator InstantiateKeyEnumerator()
        {
            return GetInternalKeysCollection().GetEnumerator();
        }

        #endregion

        #region Inner class: KeySetBasedCollection

        /// <summary>
        /// Represents collection of keys, values or entries in the
        /// <b>AbstractKeySetBasedCache</b>.
        /// </summary>
        internal class KeySetBasedCollection : KeyBasedCollection
        {
            /// <summary>
            /// Create new instance of KeySetBasedCollection.
            /// </summary>
            /// <param name="parent">
            /// Parent AbstractKeySetBasedCache.
            /// </param>
            /// <param name="type">
            /// Collection type, one of the
            /// <see cref="AbstractKeyBasedCache.KeyBasedCollectionType"/> values.
            /// </param>
            public KeySetBasedCollection(AbstractKeySetBasedCache parent, KeyBasedCollectionType type) : base(parent, type)
            {}

            /// <summary>
            /// Copies the elements of the collection to an array, starting at a
            /// particular index.
            /// </summary>
            /// <param name="array">
            /// The one-dimensional array that is the destination of the elements
            /// copied from the collection. The array must have zero-based
            /// indexing.
            /// </param>
            /// <param name="index">
            /// The zero-based index in array at which copying begins.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Array is null.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero -or- index is equal to or greater than
            /// the length of array.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Array is multidimensional -or- the number of elements in the source
            /// collection is greater than the available space from index to the
            /// end of the destination array.
            /// </exception>
            public override void CopyTo(Array array, int index)
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

                ICollection keys = m_cache.Keys;

                if (keys.Count > array.Length - index)
                {
                    throw new ArgumentException(
                        "Array is not big enough to accomodate all collection elements", "array");
                }

                if (m_type == KeyBasedCollectionType.Keys)
                {
                    keys.CopyTo(array, index);
                }
                else
                {
                    foreach (object key in keys)
                    {
                        object value = (m_type == KeyBasedCollectionType.Values
                                            ? m_cache[key]
                                            : new CacheEntry(key, m_cache[key]));
                        array.SetValue(value, index++);
                    }
                }
            }
        }

        #endregion
    }
}