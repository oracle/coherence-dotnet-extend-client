/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Threading;

namespace Tangosol.Util
{
    /// <summary>
    /// Use this class in cases that large numbers of threads can operate
    /// concurrently with an additional requirement that all threads be
    /// blocked for certain operations.
    /// </summary>
    /// <remarks>
    /// The algorithm is based on a gate concept, allowing threads in
    /// <see cref="Enter"/> and out <see cref="Exit"/>, but occasionally
    /// shutting the gate <see cref="Close"/> such that other threads cannot
    /// enter and exit. However, since threads may "be inside", the gate
    /// cannot fully close until they leave <see cref="Exit"/>. Once all
    /// threads are out, the gate is closed, and can be re-opened
    /// <see cref="Open"/> or permanently closed <see cref="Destroy"/>.
    /// <p>
    /// Each call to <b>Enter</b> requires a corresponding call to
    /// <b>Exit</b>. For example, the following would ensure proper clean-up
    /// using a ThreadGate:</p>
    /// <p>
    /// <pre>
    /// gate.Enter();
    /// try
    /// {
    ///     ...
    /// }
    /// finally
    /// {
    ///     gate.Exit();
    /// }
    /// </pre></p>
    /// <p>
    /// Similarly, each call to <b>Close</b> should be matched with a call to
    /// <b>Open</b>, unless the gate is being destroyed:</p>
    /// <p>
    /// <pre>
    /// gate.Close();
    /// try
    /// {
    ///     ...
    /// }
    /// finally
    /// {
    ///     gate.Open();
    /// }
    /// </pre></p>
    /// <p>
    /// or:</p>
    /// <p>
    /// <pre>
    /// gate.Close();
    /// gate.Destroy();
    /// </pre></p>
    /// <p>
    /// The Enter/Exit calls can be nested; the same thread can invoke Enter
    /// multiple times as long as Exit is invoked a corresponding number of
    /// times. The Close/Open calls work in the same manner. Lastly, the
    /// thread that closes the gate may continue to Enter/Exit the gate even
    /// when it is closed since that thread has exclusive control of the
    /// gate.</p>
    /// </remarks>
    /// <author>Cameron Purdy  2003.05.26</author>
    /// <author>Mark Falco  2007.04.27</author>
    /// <author>Ana Cikic  2006.08.29</author>
    /// <since>Coherence 2.2</since>
    [Obsolete("Use GateFactory.NewGate to obtain a Gate.")]
    public class ThreadGate : Gate
    {
        #region Properties

        /// <summary>
        /// The number of unmatched completed Enter calls.
        /// </summary>
        /// <value>
        /// The number of unmatched completed Enter calls.
        /// </value>
        public virtual int ActiveCount
        {
            get { return (int) (m_atomicState.GetCount() & ACTIVE_COUNT_MASK); }
        }

        /// <summary>
        /// Determine if the current thread has entered and not exited the
        /// thread gate.
        /// </summary>
        /// <remarks>
        /// This is useful for detecting re-entrancy.
        /// </remarks>
        /// <value>
        /// <b>true</b> if the current thread has entered and not exited the
        /// thread gate.
        /// </value>
        public virtual bool IsActiveThread
        {
            get { return GetThreadLocalCount(m_slotThreadEnterCount) > 0; }
        }

        /// <summary>
        /// Determine if the current thread has closed and not opened the
        /// the thread gate.
        /// </summary>
        /// <remarks>
        /// This is useful for detecting re-entrancy.
        /// </remarks>
        /// <value>
        /// <b>true</b> if the current thread has closed and not opened the
        /// thread gate.
        /// </value>
        public virtual bool IsClosingThread
        {
            get { return Status == ThreadGateState.Closed && 
                         Thread.CurrentThread == ClosingThread; }
        }

        /// <summary>
        /// Determine if the current thread has entered and not exited the
        /// thread gate.
        /// </summary>
        /// <remarks>
        /// This is useful for detecting re-entrancy.
        /// </remarks>
        /// <value>
        /// <b>true</b> if the current thread has entered and not exited the
        /// thread gate.
        /// </value>
        public virtual bool IsEnteredByCurrentThread
        {
            get { return IsActiveThread; }
        }

        /// <summary>
        /// Determines if the current thread have Closed the gate but not yet
        /// Opened the Gate.
        /// </summary>
        public virtual bool IsClosedByCurrentThread
        {
            get
            {
                return Status == ThreadGateState.Closed &&
                       Thread.CurrentThread == ClosingThread;
            }
        }

        /// <summary>
        /// The total number of times the gate has been fully opened.
        /// </summary>
        /// <remarks>
        /// When setting the value, the caller must have the gate closed.
        /// </remarks>
        /// <value>
        /// The total number of times the gate has been fully opened.
        /// </value>
        protected virtual long Version
        {
            get { return m_versionCount.GetCount(); }
            set { m_versionCount.SetCount(value); }
        }

        /// <summary>
        /// The number of unmatched completed Close/BarEntry calls.
        /// </summary>
        /// <value>
        /// The number of unmatched completed Close/BarEntry calls.
        /// </value>
        public virtual int CloseCount
        {
            get { return m_closeCount; }
            set { m_closeCount = value; }
        }

        /// <summary>
        /// The thread that is closing the gates.
        /// </summary>
        /// <remarks>
        /// When setting the value, the caller must be synchronized on the
        /// ThreadGate.
        /// </remarks>
        /// <value>
        /// The thread that is closing the gates.
        /// </value>
        protected virtual Thread ClosingThread
        {
            get { return m_threadClosing; }
            set { m_threadClosing = value; }
        }

        /// <summary>
        /// The current thread gate status.
        /// </summary>
        /// <value>
        /// One of the <see cref="ThreadGateState"/> values.
        /// </value>
        public virtual ThreadGateState Status
        {
            get { return (ThreadGateState) NumberUtils.URShift(m_atomicState.GetCount(), STATUS_OFFSET); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Allocates unnamed data slots on threads to store local thread
        /// counter values and returns new instance of ThreadGate.
        /// </summary>
        public ThreadGate()
        {
            m_slotThreadEnterCount   = Thread.AllocateDataSlot();
            m_slotThreadEnterVersion = Thread.AllocateDataSlot();
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Provide a human-readable representation of this ThreadGate.
        /// </summary>
        /// <returns>
        /// A human-readable representation of this ThreadGate.
        /// </returns>
        public override string ToString()
        {
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                string state;
                switch (Status)
                {
                    case ThreadGateState.Open:
                        state = "GATE_OPEN";
                        break;
                    case ThreadGateState.Closing:
                        state = "GATE_CLOSING";
                        break;
                    case ThreadGateState.Closed:
                        state = "GATE_CLOSED";
                        break;
                    case ThreadGateState.Destroyed:
                        state = "GATE_DESTROYED";
                        break;
                    default:
                        state = "INVALID";
                        break;
                }

                return "ThreadGate{State="  + state
                       + ", ActiveCount="   + ActiveCount
                       + ", CloseCount="    + CloseCount
                       + ", ClosingThread=" + ClosingThread
                       + '}';
            }
        }

        #endregion

        #region Gate methods

        /// <summary>
        /// Bar entry of the thread gate by other threads, but do not wait
        /// for the gate to close.
        /// </summary>
        /// <remarks>
        /// When all other threads have exited, the status of the thread gate
        /// will be closeable by the thread which barred entry. Each
        /// sucessful invocation of this method must ultimately have a
        /// corresponding invocation of the Open method (assuming the thread
        /// gate is not destroyed) even if the calling thread does not
        /// subsequently close the gate.
        /// <pre>
        /// gate.BarEntry(-1);
        /// try
        /// {
        ///     // processing that does not require the gate to be closed
        ///     // ...
        /// }
        /// finally
        /// {
        ///     gate.Close(-1);
        ///     try
        ///     {
        ///         // processing that does require the gate to be closed
        ///         // ...
        ///     }
        ///     finally
        ///     {
        ///         gate.Open(); // matches gate.Close()
        ///     }
        ///     gate.Open(); // matches gate.BarEntry()
        /// }
        /// </pre>
        /// </remarks>
        /// <param name="millis">
        /// Maximum number of milliseconds to wait; pass -1 for forever or 0
        /// for no wait.
        /// </param>
        /// <returns>
        /// <b>true</b> iff entry into the thread gate was successfully
        /// barred by the calling thread.
        /// </returns>
        public bool BarEntry(long millis)
        {
            Thread thread = Thread.CurrentThread;

            if (ClosingThread == thread)
            {
                // we've already closed or are closing the gate
                CloseCount = CloseCount + 1;
                return true;
            }

            using (BlockingLock l = BlockingLock.Lock(this))
            {
                while (true)
                {
                    if (ClosingThread == null)
                    {
                        // transition to CLOSING state
                        if (UpdateStatus(ThreadGateState.Closing) == ThreadGateState.Destroyed)
                        {
                            // oops gate was destroyed while we were waiting
                            UpdateStatus(ThreadGateState.Destroyed);
                            throw new InvalidOperationException("ThreadGate.BarEntry: ThreadGate has been destroyed.");
                        }

                        ClosingThread = thread;
                        CloseCount    = 1;
                        return true;
                    }

                    // gate is already closed or closing, wait for notification
                    millis = DoWait(millis);
                    if (millis == 0)
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Close the thread gate.
        /// </summary>
        /// <remarks>
        /// A thread uses this method to obtain exclusive access to the
        /// resource represented by the thread gate. Each invocation of this
        /// method must ultimately have a corresponding invocation of the
        /// Open method.
        /// </remarks>
        /// <param name="millis">
        /// Maximum number of milliseconds to wait; pass -1 for forever or 0
        /// for no wait.
        /// </param>
        /// <returns>
        /// <b>true</b> iff entry into the thread gate was successfully
        /// barred by the calling thread and no other threads remain in the
        /// gate.
        /// </returns>
        public virtual bool Close(long millis)
        {
            Thread thread = Thread.CurrentThread;

            if (ClosingThread == thread && Status == ThreadGateState.Closed)
            {
                // we've already closed the gate
                CloseCount = CloseCount + 1;
                return true;
            }

            AtomicCounter atomicState = m_atomicState;
            long          enterCount  = GetThreadLocalCount(m_slotThreadEnterCount);
            long          statusReq   = EMPTY_GATE_OPEN   | enterCount;
            long          statusEnd   = EMPTY_GATE_CLOSED | enterCount;
            bool          reenter     = false;
            bool          reopen      = false;

            using (BlockingLock l = BlockingLock.Lock(this))
            {
                try
                {
                    if (ClosingThread == thread)
                    {
                        statusReq = EMPTY_GATE_CLOSING;

                        // if we've also "entered" we need to temporarily
                        // decrement the counter so that the last thread to
                        // exit the gate will know to notify us
                        if (enterCount > 0)
                        {
                            reenter = true;
                            atomicState.Decrement(enterCount);
                        }
                    }

                    while (true)
                    {
                        if (atomicState.SetCount(statusReq, statusEnd))
                        {
                            // we've closed the gate
                            CloseCount    = CloseCount + 1;
                            ClosingThread = thread; // in case we bypassed ThreadGateState.Closing
                            reenter     = reopen = false;
                            return true;
                        }
                        else if (ClosingThread == null)
                        {
                            // transition to Closing state
                            if (UpdateStatus(ThreadGateState.Closing) == ThreadGateState.Destroyed)
                            {
                                // oops gate was destroyed while we were waiting
                                UpdateStatus(ThreadGateState.Destroyed);
                                throw new InvalidOperationException("ThreadGate.Close: ThreadGate has been destroyed.");
                            }

                            ClosingThread = thread;
                            statusReq     = EMPTY_GATE_CLOSING;
                            reopen      = true; // reopen if we fail

                            // if we've also "entered" we need to temporarily
                            // decrement the counter so that the last thread to
                            // exit the gate will know to notify us
                            if (enterCount > 0)
                            {
                                reenter = true;
                                atomicState.Decrement(enterCount);
                            }

                            // as we've just transititioned to CLOSING we must
                            // retest the active count since exiting threads only
                            // notify if they when in the state is CLOSING, thus
                            // we can't go to DoWait without retesting
                            continue;
                        }

                        // gate is closed or closing, wait for notification
                        millis = (int) DoWait(millis);
                        if (millis == 0)
                        {
                            return false;
                        }
                    }
                }
                finally
                {
                    // if we transititioned to closing but didn't make it to
                    // closed; re-open the gate
                    if (reenter)
                    {
                        atomicState.Increment(enterCount); // undo temporary decrement
                    }

                    if (reopen)
                    {
                        ClosingThread = null;
                        UpdateStatus(ThreadGateState.Open);
                        Monitor.PulseAll(this);
                    }
                }
            }
        }

        /// <summary>
        /// Destroy the thread gate.
        /// </summary>
        /// <remarks>
        /// This method can only be invoked if the gate is already closed.
        /// </remarks>
        public virtual void Destroy()
        {
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                switch (Status)
                {
                    case ThreadGateState.Closed:
                    {
                        Thread thread = Thread.CurrentThread;
                        if (thread != ClosingThread)
                        {
                            throw new InvalidOperationException("ThreadGate.Destroy: Gate was not closed by this thread! " + this);
                        }

                        UpdateStatus(ThreadGateState.Destroyed);
                        ClosingThread = null;
                        Monitor.PulseAll(this);
                    }
                    break;

                    case ThreadGateState.Destroyed:
                        // the gate has already been destroyed
                        break;

                    default:
                        throw new InvalidOperationException("ThreadGate.Destroy: Gate is not closed! " + this);
                }
            }
        }

        /// <summary>
        /// Enter the thread gate.
        /// </summary>
        /// <remarks>
        /// A thread uses this method to obtain non-exclusive access to the
        /// resource represented by the thread gate. Each invocation of this
        /// method must ultimately have a corresponding invocation of the
        /// Exit method.
        /// </remarks>
        /// <param name="millis">
        /// Maximum number of milliseconds to wait; pass -1 for forever or 0
        /// for no wait.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the calling thread successfully entered the gate.
        /// </returns>
        public virtual bool Enter(long millis)
        {
            AtomicCounter atomicState = m_atomicState;

            if (IncrementThreadLocalCount(m_slotThreadEnterCount) > 1 || ClosingThread == Thread.CurrentThread)
            {
                // we were already in the gate, or are the one which has closed it
                // thus we must get in regardless of the state
                if ((atomicState.Increment() & ACTIVE_COUNT_MASK) > int.MaxValue)
                {
                    // the gate has been entered way too many times, we must
                    // have a cut-off somewhere, it is here as this could only
                    // be possible if a thread keeps reentering the gate
                    atomicState.Decrement();
                    DecrementThreadLocalCount(m_slotThreadEnterCount);
                    throw new InvalidOperationException("The ThreadGate is full.");
                }
                // no need to check m_slotThreadEnterVersion, to get here we must be up to date
                return true;
            }

            bool isSuccess = false;
            try
            {
                while (true)
                {
                    long status = atomicState.GetCount();
                    switch ((ThreadGateState) (NumberUtils.URShift(status, STATUS_OFFSET)))
                    {
                        case ThreadGateState.Open:
                            if (atomicState.SetCount(status, status + 1))
                            {
                                // atomic set succeeded confirming that the gate
                                // remained open and that we made it in
                                long version = Version;
                                if (version > GetThreadLocalCount(m_slotThreadEnterVersion))
                                {
                                    // the gate has been closed/opened since we
                                    // last entered, flush to get up to date
                                    Thread.MemoryBarrier();
                                    SetThreadLocalCount(m_slotThreadEnterVersion, version);
                                }
                                return isSuccess = true;
                            }
                            // we failed to atomically enter an open gate, which
                            // can happen if either the gate closed just as we entered
                            // or if another thread entered at the same time
                            break; // retry

                        case ThreadGateState.Closing:
                        case ThreadGateState.Closed:
                            // we know that we were not already in the gate, and are
                            // not the one closing the gate; wait for it to open
                            using (BlockingLock l = BlockingLock.Lock(this))
                            {
                                ThreadGateState state = Status;
                                if (state == ThreadGateState.Closing || state == ThreadGateState.Closed)
                                {
                                    // wait for the gate to open
                                    millis = (int) DoWait(millis);
                                    if (millis == 0L)
                                    {
                                        return false;
                                    }
                                }
                                // version must be set from within sync since
                                // we have not yet entered the gate.
                                SetThreadLocalCount(m_slotThreadEnterVersion, Version);
                            }
                            break; // retry

                        case ThreadGateState.Destroyed:
                            DecrementThreadLocalCount(m_slotThreadEnterCount);
                            throw new InvalidOperationException("ThreadGate.Enter: ThreadGate has been destroyed.");

                        default:
                            DecrementThreadLocalCount(m_slotThreadEnterCount);
                            throw new InvalidOperationException("ThreadGate.Enter: ThreadGate has an invalid status. " + this);
                    }
                }
            }
            finally
            {
                if (!isSuccess)
                {
                    DecrementThreadLocalCount(m_slotThreadEnterCount);
                }
            }
        }

        /// <summary>
        /// Exit the gate.
        /// </summary>
        /// <remarks>
        /// A thread must invoke this method corresponding to each invocation
        /// of the Enter method.
        /// </remarks>
        public virtual void Exit()
        {
            if (DecrementThreadLocalCount(m_slotThreadEnterCount) < 0)
            {
                IncrementThreadLocalCount(m_slotThreadEnterCount);
                throw new SynchronizationLockException("ThreadGate.Exit: Thread has already exited! " + this);
            }

            if (m_atomicState.Decrement() == EMPTY_GATE_CLOSING)
            {
                // we were the last to exit, and the gate is in the CLOSING state
                // notify everyone, to ensure that we notify the closing thread
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    Monitor.PulseAll(this);
                }
            }
        }

        /// <summary>
        /// After entry into the ThreadGate is restricted by a call to
        /// BarEntry() or Close(), it can be re-opened by calling this
        /// method.
        /// </summary>
        /// <remarks>
        /// Only the thread that called BarEntry() or Close() can call
        /// Open().
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// If the gate is not closed or closing or if Open() is not called
        /// by the thread that called BarEntry() or Close().
        /// </exception>
        public virtual void Open()
        {
            if (Thread.CurrentThread == ClosingThread)
            {
                int closeCount = CloseCount - 1;
                if (closeCount >= 0)
                {
                    CloseCount = closeCount;
                    if (closeCount == 0)
                    {
                        // we've opened the gate
                        long version = Version + 1;

                        Version = version;
                        SetThreadLocalCount(m_slotThreadEnterVersion, version);

                        using (BlockingLock l = BlockingLock.Lock(this))
                        {
                            UpdateStatus(ThreadGateState.Open);
                            ClosingThread = null;
                            Monitor.PulseAll(this);
                        }
                    }
                    return;
                }
            }

            throw new SynchronizationLockException("ThreadGate.Open: Gate was not closed by this thread! " + this);
        }

        #endregion

        #region Thread local storage methods

        /// <summary>
        /// Obtain the long value stored in the <b>LocalDataStoreSlot</b>.
        /// </summary>
        /// <param name="slot">
        /// <b>LocalDataStoreSlot</b> object which provides managed thread
        /// local storage with dynamic data slots that are unique to a thread
        /// and application-domain combination.
        /// </param>
        /// <returns>
        /// Long value stored in the <b>LocalDataStoreSlot</b>.
        /// </returns>
        protected virtual long GetThreadLocalCount(LocalDataStoreSlot slot)
        {
            long   counter = 0;
            object value   = Thread.GetData(slot);

            if (value == null)
            {
                SetThreadLocalCount(slot, counter);
            }
            else
            {
                counter = (long) value;
            }
            return counter;
        }

        /// <summary>
        /// Set the long value to be stored in the <b>LocalDataStoreSlot</b>.
        /// </summary>
        /// <param name="slot">
        /// <b>LocalDataStoreSlot</b> object which provides managed thread
        /// local storage with dynamic data slots that are unique to a thread
        /// and application-domain combination.
        /// </param>
        /// <param name="value">
        /// Long value to be stored in the <b>LocalDataStoreSlot</b>.
        /// </param>
        protected virtual void SetThreadLocalCount(LocalDataStoreSlot slot, long value)
        {
            Thread.SetData(slot, value);
        }

        /// <summary>
        /// Increment the long value from the <b>LocalDataStoreSlot</b> for
        /// the current thread by one.
        /// </summary>
        /// <remarks>
        /// If the value in the <b>LocalDataStoreSlot</b> is <c>null</c>, 1
        /// is returned.
        /// </remarks>
        /// <param name="slot">
        /// <b>LocalDataStoreSlot</b> object which provides managed thread
        /// local storage with dynamic data slots that are unique to a thread
        /// and application-domain combination.
        /// </param>
        /// <returns>
        /// The new long value for the current thread or 1 if previous value
        /// was <c>null</c>.
        /// </returns>
        protected virtual long IncrementThreadLocalCount(LocalDataStoreSlot slot)
        {
            long c = GetThreadLocalCount(slot) + 1;
            SetThreadLocalCount(slot, c);
            return c;
        }

        /// <summary>
        /// Decrement the long value of the <b>LocalDataStoreSlot</b> for the
        /// current thread by one.
        /// </summary>
        /// <remarks>
        /// If the value in the <b>LocalDataStoreSlot</b> is <c>null</c>, -1
        /// is returned.
        /// </remarks>
        /// <param name="slot">
        /// <b>LocalDataStoreSlot</b> object which provides managed thread
        /// local storage with dynamic data slots that are unique to a thread
        /// and application-domain combination.
        /// </param>
        /// <returns>
        /// The new long value for the current thread or -1 if previous value
        /// was <c>null</c>.
        /// </returns>
        protected virtual long DecrementThreadLocalCount(LocalDataStoreSlot slot)
        {
            long c = GetThreadLocalCount(slot) - 1;
            SetThreadLocalCount(slot, c);
            return c;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Wait up to the specified number of milliseconds for notification.
        /// </summary>
        /// <remarks>
        /// Caller must be synchronized.
        /// </remarks>
        /// <param name="millis">
        /// Number of milliseconds to wait for.
        /// </param>
        /// <returns>
        /// The remaining wait time in milliseconds.
        /// </returns>
        protected virtual long DoWait(long millis)
        {
            if (millis == 0)
            {
                return 0;
            }

            long time = DateTimeUtils.GetSafeTimeMillis();
            try
            {
                if (millis < 0 || millis > int.MaxValue)
                {
                    Blocking.Wait(this, Timeout.Infinite);
                }
                else
                {
                    Blocking.Wait(this, (int) millis);
                }
            }
            catch (ThreadInterruptedException e)
            {
                Thread.CurrentThread.Interrupt();
                throw new Exception(ToString(), e);
            }
            return millis < 0 ? millis : Math.Max(0, millis - (DateTimeUtils.GetSafeTimeMillis() - time));
        }

        /// <summary>
        /// Update the current thread gate status, without changing the
        /// active count.
        /// </summary>
        /// <remarks>
        /// The caller must hold synchronization on the ThreadGate.
        /// </remarks>
        /// <param name="status">
        /// The new status.
        /// </param>
        /// <returns>
        /// The old status.
        /// </returns>
        protected virtual ThreadGateState UpdateStatus(ThreadGateState status)
        {
            AtomicCounter atomicState     = m_atomicState;
            long          offsettedStatus = ((long) status) << STATUS_OFFSET;
            while (true)
            {
                long current  = atomicState.GetCount();
                long newValue = offsettedStatus | (current & ACTIVE_COUNT_MASK);
                if (atomicState.SetCount(current, newValue))
                {
                    return (ThreadGateState) (NumberUtils.URShift(current, STATUS_OFFSET));
                }
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// The bit offset at which the ThreadGateStatus is stored within
        /// m_atomicState.
        /// </summary>
        private static readonly int STATUS_OFFSET = 60;

        /// <summary>
        /// The bit mask covering the portion of m_atomicState used to store
        /// the number of unmatched enter calls.
        /// </summary>
        private static readonly long ACTIVE_COUNT_MASK = NumberUtils.URShift(-1L, (64 - STATUS_OFFSET));

        /// <summary>
        /// EMPTY_GATE_OPEN: Threads may Enter, Exit, or Close the gates.
        /// </summary>
        private static readonly long EMPTY_GATE_OPEN = ((long) ThreadGateState.Open << STATUS_OFFSET);

        /// <summary>
        /// EMPTY_GATE_CLOSING: Closing thread may close the gates, all
        /// entered threads have exited.
        /// </summary>
        private static readonly long EMPTY_GATE_CLOSING = ((long) ThreadGateState.Closing << STATUS_OFFSET);

        /// <summary>
        /// EMPTY_GATE_CLOSED: Gates are closed, with no threads inside.
        /// </summary>
        private static readonly long EMPTY_GATE_CLOSED = ((long) ThreadGateState.Closed << STATUS_OFFSET);

        #endregion

        #region Data members

        /// <summary>
        /// The state of the ThreadGate, including:
        /// <code>
        /// bits  0 - 59 store the number of unmatched enter calls
        /// bits 60 - 61 store the ThreadGateState value
        /// bit  62 - 63 always zero
        /// </code>
        /// </summary>
        private AtomicCounter m_atomicState = AtomicCounter.NewAtomicCounter();

        /// <summary>
        /// Number of unmatched completed Close/BarEntry calls.
        /// </summary>
        private int m_closeCount;

        /// <summary>
        /// The thread that is closing the gates.
        /// </summary>
        [NonSerialized]
        private volatile Thread m_threadClosing;

        /// <summary>
        /// An AtomicCounter that tracks the number of times the gate has
        /// transitioned from Closed to Open.
        /// </summary>
        private AtomicCounter m_versionCount = AtomicCounter.NewAtomicCounter();

        /// <summary>
        /// Unnamed LocalDataStoreSlot used to store count of unmatched Enter
        /// calls per thread.
        /// </summary>
        private LocalDataStoreSlot m_slotThreadEnterCount;

        /// <summary>
        /// Unnamed LocalDataStoreLost used to store the version number at
        /// the time the thread last entered/opened the gate.
        /// </summary>
        /// <remarks>
        /// Comparing this version number to the m_versionCount, indicates if
        /// a thread entering the gate needs to flush memory to be up to date
        /// with the last thread which opened the gate.
        /// </remarks>
        private LocalDataStoreSlot m_slotThreadEnterVersion;

        #endregion
    }

    #region Enum: ThreadGateState

    /// <summary>
    /// ThreadGate state enum.
    /// </summary>
    public enum ThreadGateState
    {
        /// <summary>
        /// Threads may enter and exit the gates.
        /// </summary>
        Open = 0,

        /// <summary>
        /// A thread is waiting to be the only thread inside the gates; other
        /// threads can only exit.
        /// </summary>
        Closing = 1,

        /// <summary>
        /// A single thread is inside the gates; other threads cannot enter.
        /// </summary>
        Closed = 2,

        /// <summary>
        /// Life-cycle is complete; the object is no longer usable.
        /// </summary>
        Destroyed = 3
    }

    #endregion
}