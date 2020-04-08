/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using Tangosol.Internal.Util.Processor;
using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// Contains extension methods for IInvocableCache for the new Java 8 
    /// InvocableMap methods.
    /// </summary>
    /// <author>lh 2015.04.28</author>
    /// <since>Coherence 12.2.1</since>
    public static class InvocableCacheEx
    {
        #region Java 8 default InvocableMap methods

        /// <summary>
        /// Returns the value to which the specified key is mapped, or
        /// the defaultValue if this cache contains no mapping for the key.
        /// </summary>
        /// <param name="invocable">
        /// The interface it extends.
        /// </param>
        /// <param name="key">
        /// The key whose associated value is to be returned.
        /// </param>
        /// <param name="defaultValue">
        /// The default value of the key.
        /// </param>
        /// <returns>
        /// The value to which the specified key is mapped, or defaultValue
        /// if this cache contains no mapping for the key.
        /// </returns>
        public static object GetOrDefault(this IInvocableCache invocable, object key, object defaultValue)
        {
            return ((Optional) invocable.Invoke(key, CacheProcessors.GetOrDefault())).OrElse(defaultValue);
        }

        /// <summary>
        /// If the specified key is not already associated with a value 
        /// (or is mapped to null) associates it with the given value and
        /// returns null, else returns the current value.
        /// </summary>
        /// <param name="invocable">
        /// The interface it extends.
        /// </param>
        /// <param name="key">
        /// Key with which the specified value is to be associated.
        /// </param>
        /// <param name="value">
        /// Value to be associated with the specified key.
        /// </param>
        /// <returns>
        /// The current value associated with the specified key, or
        /// null if there was no mapping for the key.
        /// </returns>
        public static object InsertIfAbsent(this IInvocableCache invocable, object key, object value)
        {
            return invocable.Invoke(key, CacheProcessors.InsertIfAbsent(value));
        }

        /// <summary>
        /// Removes the entry for the specified key only if it is currently
        /// mapped to the specified value.
        /// </summary>
        /// <param name="invocable">
        /// The interface it extends.
        /// </param>
        /// <param name="key">
        /// Key with which the specified value is associated.
        /// </param>
        /// <param name="value">
        /// Value expected to be associated with the specified key.
        /// </param>
        /// <returns>
        /// True if the value was removed.
        /// </returns>
        public static object Remove(this IInvocableCache invocable, object key, object value)
        {
            return invocable.Invoke(key, CacheProcessors.Remove(value));
        }

        /// <summary>
        /// Replaces the entry for the specified key only if currently
        /// mapped to the specified value.
        /// </summary>
        /// <param name="invocable">
        /// The interface it extends.
        /// </param>
        /// <param name="key">
        /// Key with which the specified value is associated.
        /// </param>
        /// <param name="oldValue">
        /// Value expected to be associated with the specified key.
        /// </param>
        /// <param name="newValue">
        /// Value to be associated with the specified key.
        /// </param>
        /// <returns>
        /// True if the value was replaced.
        /// </returns>
        public static object Replace(this IInvocableCache invocable, object key, object oldValue, object newValue)
        {
            return invocable.Invoke(key, CacheProcessors.Replace(oldValue, newValue));
        }

        /// <summary>
        /// Replaces the entry for the specified key only if it is
        /// currently mapped to some value.
        /// </summary>
        /// <param name="invocable">
        /// The interface it extends.
        /// </param>
        /// <param name="key">
        /// Key with which the specified value is associated.
        /// </param>
        /// <param name="value">
        /// Value to be associated with the specified key.
        /// </param>
        /// <returns>
        /// The previous value associated with the specified key, or
        /// null if there was no mapping for the key.
        /// </returns>
        public static object Replace(this IInvocableCache invocable, object key, object value)
        {
            return invocable.Invoke(key, CacheProcessors.Replace(value));
        }

        #endregion
    }
}