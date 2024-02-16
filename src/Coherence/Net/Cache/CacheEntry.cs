/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// Basic <see cref="ICacheEntry"/> implementation.
    /// </summary>
    /// <author>Aleksandar Seovic  2006.11.11</author>
    public struct CacheEntry : ICacheEntry
    {
        #region Properties

        /// <summary>
        /// Gets the key for this cache entry.
        /// </summary>
        /// <value>
        /// The <b>key</b> for this cache entry.
        /// </value>
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
            get { return m_value; }
            set { m_value = value; }
        }

        /// <summary>
        /// Determine the original value in the cache that this cache entry's 
        /// value is replacing.
        /// </summary>
        /// <value>
        /// The <b>original value</b> for this cache entry.
        /// </value>
        public object OriginalValue
        {
            get { return m_valueOrig; }
        }

        #endregion        

        #region Constructors

        /// <summary>
        /// Creates an instance of a <see cref="CacheEntry"/>.
        /// </summary>
        /// <param name="key">
        /// Cache entry <b>key</b>.
        /// </param>
        /// <param name="value">
        /// Cache entry <b>value</b>.
        /// </param>
        public CacheEntry(object key, object value) 
            : this(key, value, null)
        {}

        /// <summary>
        /// Creates an instance of a <see cref="CacheEntry"/>.
        /// </summary>
        /// <param name="key">
        /// Cache entry <b>key</b>.
        /// </param>
        /// <param name="value">
        /// Cache entry <b>value</b>.
        /// </param>
        /// <param name="origValue">
        /// The original value in the cache that this entry's value is
        /// replacing.
        /// </param>
        public CacheEntry(object key, object value, object origValue)
        {
            m_key       = key;
            m_value     = value;
            m_valueOrig = origValue;
        }

        #endregion

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
        /// <param name="obj">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <c>true</c> iff this <b>CacheEntry.</b> and the passed object are equivalent.
        /// </returns>
        public override bool Equals(object obj)
        {
            if(!(obj is CacheEntry))
            {
                return false;
            }

            var cacheEntry = (CacheEntry) obj;
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
            return "CacheEntry("
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
        public static implicit operator DictionaryEntry(CacheEntry entry)
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
        public static implicit operator CacheEntry(DictionaryEntry entry)
        {
            return new CacheEntry(entry.Key, entry.Value);
        }

        #endregion

        #region Data Members

        /// <summary>
        /// The key for this entry.
        /// </summary>
        private readonly object m_key;

        /// <summary>
        /// The value for this entry. 
        /// </summary>
        private object m_value;

        /// <summary>
        /// The optional original value of this entry.
        /// </summary>
        private readonly object m_valueOrig;

        #endregion
    }
}