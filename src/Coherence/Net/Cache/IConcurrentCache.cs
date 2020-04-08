/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Cache
{
    /// <summary>
    /// <see cref="ICache"/> with additional concurrency features.
    /// </summary>
    /// <author>Gene Gleyzer  2001.12.16</author>
    /// <author>Aleksandar Seovic  2006.07.12</author>
    public interface IConcurrentCache : ICache
    {
        /// <summary>
        /// Attempt to lock the specified item within the specified period of
        /// time.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The item doesn't have to exist to be <i>locked</i>. While the
        /// item is locked there is known to be a <i>lock holder</i> which
        /// has an exclusive right to modify (calling put and remove methods)
        /// that item.</p>
        /// <p>
        /// Lock holder is an abstract concept that depends on the
        /// IConcurrentCache implementation. For example, holder could
        /// be a cluster member or a thread (or both).</p>
        /// <p>
        /// Locking strategy may vary for concrete implementations as well.
        /// Lock could have an expiration time (this lock is sometimes called
        /// a "lease") or be held indefinitely (until the lock holder
        /// terminates).</p>
        /// <p>
        /// Some implementations may allow the entire cache to be locked. If
        /// the cache is locked in such a way, then only a lock holder is
        /// allowed to perform any of the "put" or "remove" operations.</p>
        /// <p>
        /// Pass the special constant
        /// <see cref="LockScope.LOCK_ALL"/> as the <i>key</i>
        /// parameter to indicate the cache lock.</p>
        /// </remarks>
        /// <param name="key">
        /// Key being locked.
        /// </param>
        /// <param name="waitTimeMillis">
        /// The number of milliseconds to continue trying to obtain a lock;
        /// pass zero to return immediately; pass -1 to block the calling
        /// thread until the lock could be obtained.
        /// </param>
        /// <returns>
        /// <b>true</b> if the item was successfully locked within the
        /// specified time; <b>false</b> otherwise.
        /// </returns>
        bool Lock(object key, long waitTimeMillis);

        /// <summary>
        /// Attempt to lock the specified item and return immediately.
        /// </summary>
        /// <remarks>
        /// This method behaves exactly as if it simply performs the call
        /// <b>Lock(key, 0)</b>.
        /// </remarks>
        /// <param name="key">
        /// Key being locked.
        /// </param>
        /// <returns>
        /// <b>true</b> if the item was successfully locked; <b>false</b>
        /// otherwise.
        /// </returns>
        bool Lock(object key);

        /// <summary>
        /// Unlock the specified item.
        /// </summary>
        /// <remarks>
        /// The item doesn't have to exist to be <i>unlocked</i>.
        /// If the item is currently locked, only the <i>holder</i> of the
        /// lock could successfully unlock it.
        /// </remarks>
        /// <param name="key">
        /// Key being unlocked.
        /// </param>
        /// <returns>
        /// <b>true</b> if the item was successfully unlocked; <b>false</b>
        /// otherwise.
        /// </returns>
        bool Unlock(object key);
    }
}