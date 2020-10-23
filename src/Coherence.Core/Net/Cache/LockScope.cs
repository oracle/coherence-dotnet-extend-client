/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Net.Cache;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// Constants that define the scope of a cache lock.
    /// </summary>
    public class LockScope
    {
        /// <summary>
        /// Special constant used as the <i>key</i> parameter for
        /// <see cref="IConcurrentCache.Lock(object)"/> or
        /// <see cref="IConcurrentCache.Unlock"/> to indicate that all keys of
        /// the <b>IConcurrentCache</b> should be locked or unlocked.
        /// </summary>
        public static readonly object LOCK_ALL = new object();
    }
}