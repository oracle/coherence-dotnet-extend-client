using System;
using System.Threading;

namespace Tangosol.Util
{
    /// <summary>
    /// Blocking provides a set of helper methods related to blocking a thread.
    /// </summary>
    /// <author>Mark Falco  2015.02.24</author>
    /// <author>Patrick Fry  2015.03.27</author>
    /// <since>12.2.1.4.13</since>
    public class Blocking
    {
        // Note: the blocking helpers are written to minimize their expense when
        // they complete without timing out.  As such they all take the basic
        // approach of only checking for timeout *before* blocking, and truncating
        // the blocking time such that the blocking operation will complete when
        // timed out.  In such a case the blocking operation will not throw an
        // InterruptedException, but any subsequent blocking helper would immediately
        // detect the timeout and interrupt the thread, which would then cause its
        // blocking operation to throw ThreadInterruptedException (if appropriate). The
        // benefit of this approach is that it avoids unnecessary conditional
        // logic.

        // Note: unlike Java, because .NET does not have spurious completions,
        // methods which block indefinitely, such as Blocking.Wait(Object o),
        // will throw a ThreadInterruptedException on timeout.

        // Note: if Coherence starts using new blocking methods that aren't covered
        // here, then Blocking helpers should be added to cover the new cases.

        #region Static Helpers

        /// <summary>
        /// Acquires an exclusive lock on the specified object while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="o">The object on which to acquire the monitor lock.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted or the timeout is reached.
        /// </exception>
        public static void Enter(Object o)
        {
            while (!TryEnter(o, Timeout.Infinite)) { }
        }

        /// <summary>
        /// Acquires an exclusive lock on the specified object while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>, and atomically sets a value that indicates
        /// whether the lock was taken.
        /// </summary>
        /// <param name="o">The object on which to acquire the monitor lock.</param>
        /// <param name="lockTaken">The result of the attempt to acquire the lock, passed by reference.
        /// The input must be false. The output is true if the lock is acquired; otherwise, the output
        /// is false. The output is set even if an exception occurs during the attempt to acquire the lock.
        /// Note: If no exception occurs, the output of this method is always true.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted or the timeout is reached.
        /// </exception>
        public static void Enter(Object o, ref bool lockTaken)
        {
            do
            {
                TryEnter(o, Timeout.Infinite, ref lockTaken);
            } while (!lockTaken);
        }

        /// <summary>
        /// Attempts, for the specified number of milliseconds, to acquire an exclusive
        /// lock on the specified object while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/> 
        /// </summary>
        /// <param name="obj">The object on which to acquire the lock.</param>
        /// <param name="millis">The maximum number of milliseconds to wait for the lock.</param>
        /// <returns><tt>true</tt> if the current thread acquires the lock; otherwise, <tt>false</tt>.</returns>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted.
        /// </exception>
        public static bool TryEnter(Object obj, int millis)
        {
            int millisBlock = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                millis == Timeout.Infinite ? Int32.MaxValue : millis);
            return Monitor.TryEnter(obj, millisBlock == Int32.MaxValue ? Timeout.Infinite : (int) millisBlock);
        }

        /// <summary>
        /// Attempts, for the specified amount of time, to acquire an exclusive
        /// lock on the specified object while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/> 
        /// </summary>
        /// <param name="obj">The object on which to acquire the lock.</param>
        /// <param name="span">The maximum time to wait for the lock.</param>
        /// <returns><tt>true</tt> if the current thread acquires the lock; otherwise, <tt>false</tt>.</returns>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted.
        /// </exception>
        public static bool TryEnter(Object obj, TimeSpan span)
        {
            double spanMillis = span.TotalMilliseconds;
            double millisBlock = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                spanMillis == Timeout.Infinite ? Int32.MaxValue : spanMillis);

            // avoid creating TimeSpan objects while still supporting a submillisecond wait
            return millisBlock == spanMillis
                ? Monitor.TryEnter(obj, span)
                : Monitor.TryEnter(obj, millisBlock == Int32.MaxValue ? Timeout.Infinite : (int) millisBlock);
        }

        /// <summary>
        /// Attempts, for the specified amount of time, to acquire an exclusive lock
        /// on the specified object, while still respecting the calling thread's
        /// <see cref="ThreadTimeout"/>, and atomically sets a value that indicates whether
        /// the lock was taken.
        /// </summary>
        /// <param name="obj">The object on which to acquire the lock.</param>
        /// <param name="millis">The maximum number of milliseconds to wait for the lock.</param>
        /// <param name="lockTaken">The result of the attempt to acquire the lock, passed by reference.
        /// The input must be false. The output is true if the lock is acquired; otherwise,
        /// the output is false. The output is set even if an exception occurs during the attempt
        /// to acquire the lock.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted.
        /// </exception>
        public static void TryEnter(Object obj, int millis, ref bool lockTaken)
        {
            int millisBlock = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                millis == Timeout.Infinite ? Int32.MaxValue : millis);

            Monitor.TryEnter(obj, millisBlock == Int32.MaxValue ? Timeout.Infinite : (int) millisBlock, ref lockTaken);
        }

        /// <summary>
        /// Attempts, for the specified number of milliseconds, to acquire an exclusive lock
        /// on the specified object, while still respecting the calling thread's
        /// <see cref="ThreadTimeout"/>, and atomically sets a value that indicates whether
        /// the lock was taken.
        /// </summary>
        /// <param name="obj">The object on which to acquire the lock.</param>
        /// <param name="span">The maximum time to wait for the lock.</param>
        /// <param name="lockTaken">The result of the attempt to acquire the lock, passed by reference.
        /// The input must be false. The output is true if the lock is acquired; otherwise,
        /// the output is false. The output is set even if an exception occurs during the attempt
        /// to acquire the lock.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted.
        /// </exception>
        public static void TryEnter(Object obj, TimeSpan span, ref bool lockTaken)
        {
            double spanMillis = span.TotalMilliseconds;
            double millisBlock = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                spanMillis == Timeout.Infinite ? Int32.MaxValue : spanMillis);

            // avoid creating TimeSpan objects while still supporting a submillisecond wait
            if (millisBlock == spanMillis)
            {
                Monitor.TryEnter(obj, span, ref lockTaken);
            }
            else
            {
                Monitor.TryEnter(obj, millisBlock == Int32.MaxValue ? Timeout.Infinite : (int) millisBlock, ref lockTaken);
            }
        }

        /// <summary>
        /// Wait on the the specified monitor while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="monitor">The monitor to wait on</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted or the timeout is reached.
        /// </exception>
        public static bool Wait(Object monitor)
        {
            while (!Wait(monitor, Timeout.Infinite)) { }
            return true;
        }

        /// <summary>
        /// Wait on the the specified monitor while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="monitor">The monitor to wait on.</param>
        /// <param name="millis">The maximum number of milliseconds to wait.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted.
        /// </exception>
        public static bool Wait(Object monitor, int millis)
        {
            int millisBlock = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                millis == Timeout.Infinite ? Int32.MaxValue : millis);
            return millisBlock == Int32.MaxValue ? Monitor.Wait(monitor) : Monitor.Wait(monitor, millisBlock);
        }

        /// <summary>
        /// Wait on the specified monitor while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="monitor">The monitor to wait on.</param>
        /// <param name="span">The maximum <see cref="TimeSpan"/> to wait.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted.
        /// </exception>
        public static bool Wait(Object monitor, TimeSpan span)
        {
            double spanMillis  = span.TotalMilliseconds;
            double millisBlock = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                spanMillis == Timeout.Infinite ? Int32.MaxValue : spanMillis);

            // avoid creating TimeSpan objects while still supporting a submillisecond wait
            return millisBlock == spanMillis
                ? Monitor.Wait(monitor, span)
                : millisBlock == Int32.MaxValue
                    ? Monitor.Wait(monitor)
                    : Monitor.Wait(monitor, (int) millisBlock);
        }

        /// <summary>
        /// Invoke <see cref="Thread.Sleep(int)"/> while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="millis">The maximum number of milliseconds to sleep.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted.
        /// </exception>
        public static void Sleep(int millis)
        {
            int millisSleep = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                millis == Timeout.Infinite ? Int32.MaxValue : millis);
            Thread.Sleep(millisSleep == Int32.MaxValue ? Timeout.Infinite : millisSleep);
        }

        /// <summary>
        /// Invoke <see cref="Thread.Sleep(TimeSpan)"/> while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="span">The maximum <see cref="TimeSpan"/> to sleep.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted.
        /// </exception>
        public static void Sleep(TimeSpan span)
        {
            double spanMillis = span.TotalMilliseconds;
            double millisSleep = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                spanMillis == Timeout.Infinite ? Int32.MaxValue : spanMillis);

            // avoid creating TimeSpan objects while still supporting a submillisecond sleep
            if (millisSleep == spanMillis)
            {
                Thread.Sleep(span);
            }
            else
            {
                Thread.Sleep(millisSleep == Int32.MaxValue ? Timeout.Infinite : (int) millisSleep);
            }
        }

        /// <summary>
        /// Tries to enter the lock in read mode while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="rwLock">The lock.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted or the timeout is reached.
        /// </exception>
        public static void EnterReadLock(ReaderWriterLockSlim rwLock)
        {
            while (!TryEnterReadLock(rwLock, Timeout.Infinite)) { }
        }

        /// <summary>
        /// Tries to enter the lock in upgradeable mode while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="rwLock">The lock.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted or the timeout is reached.
        /// </exception>
        public static void EnterUpgradeableReadLock(ReaderWriterLockSlim rwLock)
        {
            while (!TryEnterUpgradeableReadLock(rwLock, Timeout.Infinite)) { }
        }

        /// <summary>
        /// Tries to enter the lock in write mode while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="rwLock">The lock.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted or the timeout is reached.
        /// </exception>
        public static void EnterWriteLock(ReaderWriterLockSlim rwLock)
        {
            while (!TryEnterWriteLock(rwLock, Timeout.Infinite)) { }
        }

        /// <summary>
        /// Tries to enter the lock in read mode, with an optional time-out while still
        /// respecting the calling thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="rwLock">The lock.</param>
        /// <param name="span">The maximum interval to wait, or -1 milliseconds to wait indefinitely.</param>
        /// <returns><tt>true</tt> if the calling thread entered read mode, otherwise, <tt>false</tt>.</returns>
        public static bool TryEnterReadLock(ReaderWriterLockSlim rwLock, TimeSpan span)
        {
            double spanMillis = span.TotalMilliseconds;
            double millisBlock = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                spanMillis == Timeout.Infinite ? Int32.MaxValue : spanMillis);

            // avoid creating TimeSpan objects while still supporting a submillisecond wait
            return millisBlock == spanMillis
                ? rwLock.TryEnterReadLock(span)
                : rwLock.TryEnterReadLock(millisBlock == Int32.MaxValue ? Timeout.Infinite : (int) millisBlock);
        }

        /// <summary>
        /// Tries to enter the lock in read mode, with an optional time-out while still
        /// respecting the calling thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="rwLock">The lock.</param>
        /// <param name="millis">The maximum interval to wait, or -1 milliseconds to wait indefinitely.</param>
        /// <returns><tt>true</tt> if the calling thread entered read mode, otherwise, <tt>false</tt>.</returns>
        public static bool TryEnterReadLock(ReaderWriterLockSlim rwLock, int millis)
        {
            int millisBlock = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                millis == Timeout.Infinite ? Int32.MaxValue : millis);
            return rwLock.TryEnterReadLock(millisBlock == Int32.MaxValue ? Timeout.Infinite : millisBlock);
        }

        /// <summary>
        /// Tries to enter the lock in upgradeable mode, with an optional time-out while still
        /// respecting the calling thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="rwLock">The lock.</param>
        /// <param name="span">The maximum interval to wait, or -1 milliseconds to wait indefinitely.</param>
        /// <returns><tt>true</tt> if the calling thread entered upgradeable mode, otherwise, <tt>false</tt>.</returns>
        public static bool TryEnterUpgradeableReadLock(ReaderWriterLockSlim rwLock, TimeSpan span)
        {
            double spanMillis = span.TotalMilliseconds;
            double millisBlock = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                spanMillis == Timeout.Infinite ? Int32.MaxValue : spanMillis);

            // avoid creating TimeSpan objects while still supporting a submillisecond wait
            return millisBlock == spanMillis
                ? rwLock.TryEnterUpgradeableReadLock(span)
                : rwLock.TryEnterUpgradeableReadLock(millisBlock == Int32.MaxValue ? Timeout.Infinite : (int) millisBlock);
        }

        /// <summary>
        /// Tries to enter the lock in upgradeable mode, with an optional time-out while still
        /// respecting the calling thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="rwLock">The lock.</param>
        /// <param name="millis">The maximum interval to wait, or -1 milliseconds to wait indefinitely.</param>
        /// <returns><tt>true</tt> if the calling thread entered upgradeable mode, otherwise, <tt>false</tt>.</returns>
        public static bool TryEnterUpgradeableReadLock(ReaderWriterLockSlim rwLock, int millis)
        {
            int millisBlock = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                millis == Timeout.Infinite ? Int32.MaxValue : millis);
            return rwLock.TryEnterUpgradeableReadLock(millisBlock == Int32.MaxValue ? Timeout.Infinite : millisBlock);
        }

        /// <summary>
        /// Tries to enter the lock in write mode, with an optional time-out while still
        /// respecting the calling thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="rwLock">The lock.</param>
        /// <param name="millis">The maximum interval to wait, or -1 milliseconds to wait indefinitely.</param>
        /// <returns><tt>true</tt> if the calling thread entered write mode, otherwise, <tt>false</tt>.</returns>
        public static bool TryEnterWriteLock(ReaderWriterLockSlim rwLock, int millis)
        {
            int millisBlock = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                millis == Timeout.Infinite ? Int32.MaxValue : millis);
            return rwLock.TryEnterWriteLock(millisBlock == Int32.MaxValue ? Timeout.Infinite : millisBlock);
        }

        /// <summary>
        /// Tries to enter the lock in write mode, with an optional time-out while still
        /// respecting the calling thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <param name="rwLock">The lock.</param>
        /// <param name="span">The maximum interval to wait, or -1 milliseconds to wait indefinitely.</param>
        /// <returns><tt>true</tt> if the calling thread entered write mode, otherwise, <tt>false</tt>.</returns>
        public static bool TryEnterWriteLock(ReaderWriterLockSlim rwLock, TimeSpan span)
        {
            double spanMillis = span.TotalMilliseconds;
            double millisBlock = Math.Min(ThreadTimeout.RemainingTimeoutMillis,
                spanMillis == Timeout.Infinite ? Int32.MaxValue : spanMillis);

            // avoid creating TimeSpan objects while still supporting a submillisecond wait
            return millisBlock == spanMillis
                ? rwLock.TryEnterWriteLock(span)
                : rwLock.TryEnterWriteLock(millisBlock == Int32.MaxValue ? Timeout.Infinite : (int) millisBlock);
        }

        #endregion
    }
}
