/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using Tangosol.Util.Collections;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// An abstract base class for <see cref="ICacheLoader"/>.
    /// </summary>
    /// <author>Cameron Purdy  2003.05.29</author>
    /// <author>Ivan Cikic  2007.05.17</author>
    public abstract class AbstractCacheLoader : ICacheLoader
    {
        #region ICacheLoader interface implementation

        /// <summary>
        /// Return the value associated with the specified key, or
        /// <c>null</c> if the key does not have an associated value in the
        /// underlying store.
        /// </summary>
        /// <param name="key">
        /// Key whose associated value is to be returned.
        /// </param>
        /// <returns>
        /// The value associated with the specified key, or
        /// <c>null</c> if no value is available for that key.
        /// </returns>
        public abstract object Load(object key);

        /// <summary>
        /// Return the values associated with each the specified keys in
        /// the passed collection.
        /// </summary>
        /// <remarks>
        /// If a key does not have an associated value in the underlying
        /// store, then the return dictionary will not have an entry for
        /// that key.
        /// </remarks>
        /// <param name="keys">
        /// A collection of keys to load.
        /// </param>
        /// <returns>
        /// A dictionary of keys to associated values for the specified
        /// keys.
        /// </returns>
        public IDictionary LoadAll(ICollection keys)
        {
            IDictionary dictionary = new HashDictionary();
            foreach (object key in keys)
            {
                object value = Load(key);
                if (value != null)
                {
                    dictionary[key] = value;
                }
            }
            return dictionary;
        }

        #endregion
    }
}
