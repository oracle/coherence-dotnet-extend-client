/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿namespace Tangosol.Util
{
    /// <summary>
    /// Use a Gate in cases that large numbers of threads can operate
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
    /// <see cref="Open"/>.
    /// <p>
    /// Each call to <b>Enter</b> requires a corresponding call to
    /// <b>Exit</b>. For example, the following would ensure proper clean-up
    /// using a Gate:</p>
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
    /// <b>Open</b>:</p>
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
    public interface Gate
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
        bool Close(long millis);

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
        void Open();

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
        bool Enter(long millis);

        /// <summary>
        /// Releases the non-exclusive lock. If the non-exclusive lock has 
        /// no more matching Enter(millis) from any thread the exclusive 
        /// lock is released so that threads wanting to Close the Gate
        /// can proceed.
        /// </summary>
        void Exit();

        /// <summary>
        /// Determines if the current thread has Entered, but not yet Exited 
        /// the Gate.
        /// </summary>
        bool IsEnteredByCurrentThread { get; }

        /// <summary>
        /// Determines if the current thread has Closed the Gate but not yet
        /// Opened the Gate.
        /// </summary>
        bool IsClosedByCurrentThread { get; }
    }
}
