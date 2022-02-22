/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.Threading;

namespace Tangosol.Util
{
    /// <summary>
    /// ThreadGateSlim exposes a ReaderWriterLockSlim through the 
    /// Gate interface.
    /// </summary>
    /// <author>Charlie Helin 2010.09.10</author>
    public class ThreadGateSlim : Gate
    {
        /// <summary>
        /// Tries to acquire the exclusive lock, the attempt is
        /// willing to wait up the specified millis or -1 to wait
        /// infinite.
        /// </summary>
        /// <param name="millis">If the value is positive the caller
        /// will block for this long until either the lock has been 
        /// acquired. If 0 an immediate attempt is made. If -1 wait an 
        /// infinite time for the lock to be acquired.
        /// </param>
        /// <returns>true if the lock has been acquired.</returns>
        public bool Close(long millis)
        {
            return Blocking.TryEnterWriteLock(m_lock, millis > int.MaxValue ? 0 : (int) millis);
        }

        /// <summary>
        /// Releases the exclusive lock. If the exclusive lock has 
        /// no more matching Close(millis) the exclusive lock is released.
        /// Any Threads waiting to Enter(millis) or Close(millis) will
        /// continue executing.
        /// 
        /// If the thread currently is also holding the non-exclusive
        /// lock, the thread will immediately Enter the lock. The thread
        /// will have to match all calls to Enter(millis) with the 
        /// corresponding count of Exit().
        /// </summary>
        public void Open()
        {
            m_lock.ExitWriteLock();
        }

        /// <summary>
        /// Tries to acquire the non-exclusive lock, the attempt is
        /// willing to wait up the specified millis or -1 to wait
        /// infinite.
        /// </summary>
        /// <param name="millis">If the value is positive the caller
        /// will block for this long until either the lock has been 
        /// acquired. If 0 an immediate attempt is made. If -1 wait an 
        /// infinite time for the lock to be acquired.
        /// </param>
        /// <returns>true if the lock has been acquired.</returns>
        public bool Enter(long millis)
        {
            return Blocking.TryEnterReadLock(m_lock, millis > int.MaxValue ? 0 : (int) millis);
        }

        /// <summary>
        /// Releases the non-exclusive lock. If the non-exclusive lock has 
        /// no more matching Enter(millis) from any thread the exclusive 
        /// lock is released so that threads wanting to Close the Gate
        /// can proceed.
        /// </summary>
        public void Exit()
        {
            m_lock.ExitReadLock();
        }

        /// <summary>
        /// Determines if the current thread has Entered, but not yet Exited 
        /// the Gate.
        /// </summary>
        public bool IsEnteredByCurrentThread
        {
            get { return m_lock.IsReadLockHeld;  }
        }

        /// <summary>
        /// Determines if the current thread has Closed the Gate but not yet
        /// Opened the Gate.
        /// </summary>
        public bool IsClosedByCurrentThread
        {
            get { return m_lock.IsWriteLockHeld; }
        }

        /// <summary>
        /// The actual read lock implementation.
        /// </summary>
        private readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    }
}   
