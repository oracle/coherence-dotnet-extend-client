/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Cache
{
    /// <summary>
    /// Cache expiration constants.
    /// </summary>
    public class CacheExpiration
    {
        /// <summary>
        /// A special time-to-live value that can be passed to the extended
        /// <see cref="ICache.Insert(object,object,long)"/> method
        /// to indicate that the cache's default expiry should be used.
        /// </summary>
        public const long DEFAULT = 0;

        /// <summary>
        /// A special time-to-live value that can be passed to the extended
        /// <see cref="ICache.Insert(object,object,long)"/> method
        /// to indicate that the cache entry should never expire.
        /// </summary>
        public const long NEVER = -1;
    }
}