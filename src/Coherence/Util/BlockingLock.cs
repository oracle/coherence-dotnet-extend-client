using System;
using System.Threading;

namespace Tangosol.Util
{
    /// <summary>
    /// An <see cref="IDisposable"/> which can be used as a replacement for the
    /// <tt>lock</tt> keyword which honors the calling thread's
    /// <see cref="ThreadTimeout"/>
    /// </summary>
    /// <remarks>
    /// Sample usage:
    /// <code>
    /// try
    /// {
    ///     using (BlockingLock l = BlockingLock.Lock(o))
    ///     {
    ///         // critical section
    ///     }
    /// }
    /// catch(ThreadInterruptedException)
    /// {
    ///     // handle timeout or thread interrupt
    /// }
    /// </code>
    /// </remarks>
    /// <author>Patrick Fry 2015.05.28</author>
    /// <since>12.2.1.4.13</since>
    public class BlockingLock : IDisposable
    {
        #region Properties

        /// <summary>
        /// Whether the object was successfully locked.
        /// </summary>
        public bool IsLockObtained
        {
            get
            {
                return m_lockObtained;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a BlockingLock.
        /// </summary>
        /// <param name="o">The object to be locked.</param>
        protected BlockingLock(Object o)
        {
            f_lockObject = o;
        }

        #endregion

        #region Lock methods

        /// <summary>
        /// Lock the object while still respecting the calling thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <remarks>
        /// This method will only return when the object is locked.
        /// </remarks>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted or the timeout is reached.
        /// </exception>
        protected void Enter()
        {
            Blocking.Enter(f_lockObject, ref m_lockObtained);
        }

        /// <summary>
        /// Attempt to lock the object, while still respecting the calling thread's
        /// <see cref="ThreadTimeout"/>..
        /// </summary>
        /// <param name="millis">The maximum number of milliseconds to wait for the lock.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted.
        /// </exception>
        protected void TryEnter(int millis)
        {
            Blocking.TryEnter(f_lockObject, millis, ref m_lockObtained);
        }

        /// <summary>
        /// Attempt to lock the object, while still respecting the calling thread's
        /// <see cref="ThreadTimeout"/>..
        /// </summary>
        /// <param name="span">The maximum <see cref="TimeSpan"/> to wait for the lock.</param>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted.
        /// </exception>
        protected void TryEnter(TimeSpan span)
        {
            Blocking.TryEnter(f_lockObject, span, ref m_lockObtained);
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// Release the lock if it was obtained.
        /// </summary>
        public void Dispose()
        {
            if (IsLockObtained)
            {
                Monitor.Exit(f_lockObject);
            }
        }

        #endregion

        #region Static Helpers

        /// <summary>
        /// Construct a new BlockingLock for the specified lockObject and wait
        /// for the lockObject to be locked while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <remarks>
        /// This method will only return when the object is locked.
        /// </remarks>
        /// <param name="lockObject">The object to be locked.</param>
        /// <returns>The BlockingLock</returns>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted or the timeout is reached.
        /// </exception>
        public static BlockingLock Lock(Object lockObject)
        {
            BlockingLock l = new BlockingLock(lockObject);
            l.Enter();
            return l;
        }

        /// <summary>
        /// Construct a new BlockingLock for the specified lockObject and wait
        /// for the lockObject to be locked while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <remarks>
        /// The BlockingLock.IsObjectLocked property can be checked on return
        /// to see if the object has been locked.
        /// </remarks>
        /// <param name="lockObject">The object to be locked.</param>
        /// <param name="millis">The maximum number of milliseconds to wait for the lock.</param>
        /// <returns>The BlockingLock</returns>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted.
        /// </exception>
        public static BlockingLock TryLock(Object lockObject, int millis)
        {
            BlockingLock l = new BlockingLock(lockObject);
            l.TryEnter(millis);
            return l;
        }

        /// <summary>
        /// Construct a new BlockingLock for the specified lockObject and wait
        /// for the lockObject to be locked while still respecting the calling
        /// thread's <see cref="ThreadTimeout"/>.
        /// </summary>
        /// <remarks>
        /// The BlockingLock.IsObjectLocked property can be checked on return
        /// to see if the object has been locked.
        /// </remarks>
        /// <param name="lockObject">The object to be locked.</param>
        /// <param name="span">The maximum <see cref="TimeSpan"/> to wait for the lock..</param>
        /// <returns>The BlockingLock</returns>
        /// <exception cref="ThreadInterruptedException">
        /// If the thread is interrupted.
        /// </exception>
        public static BlockingLock TryLock(Object lockObject, TimeSpan span)
        {
            BlockingLock l = new BlockingLock(lockObject);
            l.TryEnter(span);
            return l;
        }

        #endregion

        #region Data Members

        /// <summary>
        /// The object being locked.
        /// </summary>
        protected readonly Object f_lockObject;

        /// <summary>
        /// Whether the object was successfully locked.
        /// </summary>
        protected bool m_lockObtained = false;

        #endregion
    }
}
