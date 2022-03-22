using System;
using System.Threading;

namespace Tangosol.Util
{
    /// <summary>
    /// ThreadTimeout provides a mechanism for allowing a thread to interrupt itself if it doesn't return
    /// to a specific call site within a given timeout. ThreadTimeout instances are intended to be
    /// used with a <tt>using</tt> Statement. Once constructed a ThreadTimeout attempts to ensure that
    /// the corresponding <tt>using</tt> block completes within the specified timeout and if it
    /// does not the thread will self-interrupt. Exiting the timeout block will automatically clear
    /// any interrupt present on the thread and in such a case a ThreadInterruptedException will be thrown.
    /// </summary>
    /// <remarks>
    /// <example>
    /// try
    /// {
    ///     using (ThreadTimeout t = ThreadTimeout.After(5000))
    ///     {
    ///         DoSomething();
    ///     } // this thread will self-interrupt if it doesn't reach this line within 5 seconds
    /// }
    /// catch (ThreadInterruptedException e)
    /// {
    ///    // thread timed out or was otherwise interrupted
    /// }
    /// </example>
    ///
    /// In order for this to work any blocking code executed from within the context of the Timeout must use the
    /// <see cref="Blocking"/> static helper methods for blocking. An example of a compatible blocking call would be:
    ///
    /// <example>
    /// void DoSomething()
    /// {
    ///    Object oField = m_oField;
    ///    using (BlockingLock l = BlockingLock.Lock(oField)) // rather than lock (oField)
    ///        {
    ///        Blocking.Wait(oField); // rather than Monitor.Wait(oField);
    ///        }
    /// }
    /// </example>
    ///
    /// Note that ThreadTimeout can only self-interrupt at interruptible points, and does not defend against
    /// CPU bound loops for example.
    /// </remarks>
    /// <author>Mark Falco 2015.02.23</author>
    /// <author>Patrick Fry 2015.03.25</author>
    /// <since>12.2.1.4.13</since>
    public class ThreadTimeout : IDisposable
    {
        #region inner class: LongHolder

        /// <summary>
        /// A LongHolder object provides get/set access
        /// to a stored <tt>long</tt>.
        /// </summary>
        protected class LongHolder
        {
            #region Properties

            /// <summary>
            /// The long Value.
            /// </summary>
            public long Value
            { get; set; }

            #endregion

            #region Constructors

            /// <summary>
            /// Construct a LongHolder storing the passed in value.
            /// </summary>
            /// <param name="value">The <tt>long</tt> value to store.</param>
            public LongHolder(long value)
            {
                Value = value;
            }

            #endregion

            #region Object overrides

            /// <summary>
            /// Convert a LongHolder to a printable string.
            /// </summary>
            /// <returns>The encapsulated long value.</returns>
            public override string ToString()
            {
                return Value.ToString();
            }

            #endregion
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// Whether the calling thread is timed out.
        /// </summary>
        /// <remarks>
        /// Note if the current thread is timed out then accessing this property will
        /// also interrupt the thread. This property can be used to externally
        /// add ThreadTimeout support for other blocking APIs not covered by the existing
        /// ThreadTimeout helpers.
        /// </remarks>
        public static bool IsTimedOut
        {
            get
            {
                return RemainingTimeoutMillis == 0;
            }
        }
        
        /// <summary>
        /// The number of remaining milliseconds before this thread will time out,
        /// 0 if timed out, or <see cref="Int32.MaxValue"/> if disabled.
        /// </summary>
        /// <remarks>
        /// Note if the current thread is timed out then accessing this property will
        /// also interrupt the thread. This property can be used to externally
        /// add ThreadTimeout support for other blocking APIs not covered by the existing
        /// ThreadTimeout helpers.
        /// </remarks>
        public static int RemainingTimeoutMillis
        {
            get
            {
                LongHolder mlTimeout = null;
                try
                {
                    mlTimeout = s_tloTimeout.Value;
                }
                catch (ObjectDisposedException)
                {
                }

                if (mlTimeout == null)
                {
                    // no timeout configured; avoid pulling local time
                    return Int32.MaxValue;
                }

                long lTimeout = mlTimeout.Value;
                if (lTimeout == Int64.MaxValue)
                {
                    // no timeout configured; avoid pulling local time
                    return Int32.MaxValue;
                }

                long ldtNow = DateTimeUtils.GetSafeTimeMillis();
                if (lTimeout < 0)
                {
                    // timeout is still relative; actualize and store it
                    mlTimeout.Value = ldtNow - lTimeout; // sets timeout as now + -lTimeout
                    return (int) -lTimeout; // no need to compute relative
                }

                // else; timeout was already realized, compute remainder
                int millis = (int) (lTimeout - ldtNow);
                if (millis <= 0)
                {
                    Thread.CurrentThread.Interrupt();
                    return 0;
                }
                return millis;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Specify a new timeout.
        /// </summary>
        /// <remarks>
        /// This constructor variant allows the caller to override a parent timeout.  This is
        /// rarely needed, and is roughly the equivalent of silently consuming a thread interrupt
        /// without rethrowing the ThreadInterruptedException.
        /// </remarks>
        /// <param name="millis">The new timeout.</param>
        /// <param name="forceOverride">True if this timeout is allowed to extend a parent timeout.</param>
        protected ThreadTimeout(int millis, bool forceOverride)
        {
            LongHolder lhTimeout = s_tloTimeout.Value;
            if (lhTimeout == null)
            {
                s_tloTimeout.Value = lhTimeout = new LongHolder(Int64.MaxValue);
                f_tloCreator = true;
            }
            else
            {
                f_tloCreator = false;
            }

            // convert Timeout.Infinite to Int64.MaxValue
            long lMillis   = millis == Timeout.Infinite ? Int64.MaxValue : millis;
            f_lhTimeout    = lhTimeout;
            f_lTimeoutOrig = f_lhTimeout.Value;

            if (f_lTimeoutOrig == Int64.MaxValue) // orig is disabled (common)
            {
                f_cMillisTimeout  = lMillis;
                if (lMillis < Int64.MaxValue)
                {
                    f_lhTimeout.Value = -lMillis;
                }
            }
            else if (f_lTimeoutOrig < 0) // orig is relative (common)
            {
                if (forceOverride || lMillis < -f_lTimeoutOrig)
                {
                    f_cMillisTimeout  = lMillis;
                    f_lhTimeout.Value = -lMillis;
                }
                else // we are not allowed to extend an existing timeout
                {
                    f_cMillisTimeout = f_lTimeoutOrig;
                }
            }
            else // orig is timestamp
            {
                // TODO: we could avoid pulling the time here if we retained a ref to the prior ThreadTimeout object
                // rather than just its timeout value.  In this case we'd have the absolute timeout and its
                // relative value and could then compute our updated absolute from those.
                long ldtTimeout = lMillis == Int64.MaxValue ? Int64.MaxValue : DateTimeUtils.GetSafeTimeMillis() + lMillis;
                if (forceOverride || ldtTimeout < f_lTimeoutOrig)
                {
                    f_cMillisTimeout  = lMillis;
                    f_lhTimeout.Value = ldtTimeout;
                }
                else // we are not allowed to extend an existing timeout
                {
                    f_cMillisTimeout = f_lTimeoutOrig;
                }
            }
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// As part of closing the ThreadTimeout resource any former timeout will be restored.
        /// </summary>
        /// <exception cref="ThreadInterruptedException">if the calling thread is interrupted</exception>
        public void Dispose()
        {
            // we must always restore the former timeout, even if it is expired
            if (f_tloCreator)
            {
                s_tloTimeout.Value = null;
            }
            else
            if (f_lTimeoutOrig < 0) // orig was never realized
            {
                long lTimeoutCurr = f_lhTimeout.Value;
                if (lTimeoutCurr < 0 ||             // we've yet to block
                    lTimeoutCurr == Int64.MaxValue) // timeout was disabled (note restore is suspect, but override has already violated orig)
                {
                    // simply restore the orig value
                    f_lhTimeout.Value = f_lTimeoutOrig;
                }
                else
                {
                    // curr was realized, orig was not, adjust orig accordingly
                    // and set it as new timeout
                    f_lhTimeout.Value = lTimeoutCurr + (-f_lTimeoutOrig - f_cMillisTimeout);
                }
            }
            else // orig is realized, simply restore it
            {
                f_lhTimeout.Value = f_lTimeoutOrig;
            }

            // checking to see if the thread is interrupted here ensures that if the nested code within the
            // interrupt block were to suppress the ThreadInterruptedException (possibly from a timeout) that it
            // gets recreated here so the application is forced to deal with it
            // Note we don't just throw because of a timeout, as the general contract of a method which
            // throws ThreadInterruptedException is that it throws if the thread is interrupted, period.
            // Note: we don't try to throw some derived exception such as InterruptedTimeoutException as
            // we can't ensure that all timeout points would actually result in that exception.  For instance
            // a timeout in LockSupport.park() will interrupt the thread by throw nothing, and some other code
            // could then detect the interrupt and throw a normal ThreadInterruptedException.  Overall the intent
            // here is to just make the timeout feature be indistinugisable from another thread interrupting
            // this thread.
            Thread.Sleep(0); // 0s sleep throws exception if the thread was interrupted
        }

        #endregion

        #region Static Helpers

        // Note: the use of static factory methods in addition to being more expressive
        // then public constructors allows for the potential to pool ThreadTimeout objects
        // in the future to further reduce the cost of creating a timeout block.
        // It would seem likely that ThreadTimeout objects may live for a decent enough duration
        // that they could become tenured, and thus pooling would be worth consideration.
        // The pool could also be stored in a ThreadLocal and could simply be an array of
        // ThreadTimeouts and an index into the next free slot.  Considering that they are
        // effectively bound to a callsite and the stack depth the expectation is that
        // there would be a relatively small number of them per thread.  If implemented
        // this ThreadLocal could also hold the LongHolder timeout thus avoiding the
        // need for multiple ThreadLocal lookups.

        /// <summary>
        /// Specify a new timeout.  Note that the calling thread's timeout will only be
        /// changed if the specified timeout is less than any existing timeout already
        /// active on the thread.
        /// </summary>
        /// <remarks>
        /// The sub-millisecond portion of the TimeSpan (if any) will be discarded.
        /// </remarks>
        /// <param name="timeSpan">The new timeout.</param>
        /// <returns>A ThreadTimeout instance to be used within a using block.</returns>
        public static ThreadTimeout After(TimeSpan timeSpan)
        {
            int millis = (int) timeSpan.TotalMilliseconds;
            // ensure at least 1ms in case duration was expressed as sub-millisecond
            return After(Math.Max(millis, 1));
        }

        /// <summary>
        /// Specify a new timeout.  Note that the calling thread's timeout will only be
        /// changed if the specified timeout is less then any existing timeout already
        /// active on the thread.
        /// </summary>
        /// <param name="millis">The new timeout in milliseconds.</param>
        /// <returns>A ThreadTimeout instance to be used within a using block.</returns>
        public static ThreadTimeout After(int millis)
        {
            return new ThreadTimeout(millis, /*forceOverride*/ false);
        }

        /// <summary>
        /// Specify a new timeout, potentially extending an already active timeout.
        /// </summary>
        /// <remarks>
        /// This variant allows the caller to extend a parent timeout.  This is rarely
        /// needed, and is roughly the equivalent of silently consuming a thread interrupt
        /// without rethrowing the ThreadInterruptedException.  Use of this method should
        /// be extremely limited.
        /// </remarks>
        /// <param name="millis">The new timeout in milliseconds</param>
        /// <returns>A ThreadTimeout instance to be used within a using block.</returns>
        public static ThreadTimeout Override(int millis)
        {
            return new ThreadTimeout(millis, /*forceOverride*/ true);
        }

        #endregion

        #region Data members

        /// <summary>
        /// True iff this Timeout created (and thus must ultimately destroy) the TLO.
        /// </summary>
        protected readonly bool f_tloCreator;

        /// <summary>
        /// Cached reference to the thread's <see cref="LongHolder"/> holding it's current timeout.
        /// </summary>
        protected readonly LongHolder f_lhTimeout;

        /// <summary>
        /// This ThreadTimeout's timeout.
        /// </summary>
        protected readonly long f_cMillisTimeout;

        /// <summary>
        /// The original timeout before this instance changed it.
        /// </summary>
        protected readonly long f_lTimeoutOrig;

        /// <summary>
        /// A thread-local containing the calling thread's timeout value. Values which are greater or equal to zero
        /// are used to indicate timeout timestamps.  Negative values are relative timeouts which haven't yet been
        /// realized into a timestamp.  This allows for an optimization where we can avoid obtaining
        /// the current time when "setting" the timeout, and defer it until we are about to block.
        /// </summary>
        protected static ThreadLocal<LongHolder> s_tloTimeout = new ThreadLocal<LongHolder>();

        #endregion
    }
}
