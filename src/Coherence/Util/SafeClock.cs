/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;

namespace Tangosol.Util
{
    /// <summary>
    /// SafeClock maintains a "safe" time in milliseconds.
    /// </summary>
    /// <remarks>
    /// This clock guarantees that the time never "goes back". More
    /// specifically, when queried twice on the same thread, the second query
    /// will never return a value that is less then the value returned by the
    /// first.
    /// 
    /// If we detect the system clock moving backward, an attempt will be made
    /// to gradually compensate the safe clock (by slowing it down), so in the
    /// long run the safe time is the same as the system time.
    ///
    /// The SafeClock supports the concept of "clock jitter", which is a small
    /// time interval that the system clock could fluctuate by without a
    /// corresponding passage of wall time.
    /// </remarks>
    /// <author>mf  2009.12.09</author>
    public class SafeClock
    {
        #region constructors

        /// <summary>
        /// Create a new SafeClock with the default maximum expected jitter.
        /// </summary>
        /// <param name="ldtUnsafe">
        /// The current unsafe time
        /// </param>
        public SafeClock(long ldtUnsafe)
        {
            m_ldtLastSafe = m_ldtLastUnsafe = ldtUnsafe;
            m_lJitter     = DEFAULT_JITTER_THRESHOLD;
        }

        /// <summary>
        /// Create a new SafeClock with the specified jitter threshold.
        /// </summary>
        /// <param name="ldtUnsafe">
        /// The current unsafe time
        /// </param>
        /// <param name="lJitter">
        /// The maximum expected jitter in the underlying system clock
        /// </param>
        public SafeClock(long ldtUnsafe, long lJitter)
        {
            m_ldtLastSafe = m_ldtLastUnsafe = ldtUnsafe;
            m_lJitter     = lJitter;
        }

        #endregion

        #region SafeClock interface

        /// <summary>
        /// Returns a "safe" current time in milliseconds.
        /// </summary>
        /// <param name="ldtUnsafe">
        /// The current unsafe time
        /// </param>
        /// <returns>
        /// The difference, measured in milliseconds, between the corrected
        /// current time and midnight, January 1, 0001.
        /// </returns>
        public long GetSafeTimeMillis(long ldtUnsafe)
        {
            // optimization for heavy concurrent load: if no time has passed,
            // or time jumped back within the expected jitter just return the
            // last time and avoid synchronization; keep short to encourage
            // compiler optimizations
            long lDelta = ldtUnsafe - m_ldtLastUnsafe;

            return lDelta == 0 || (lDelta < 0 && lDelta >= -m_lJitter)
                    ? m_ldtLastSafe // common case during heavy load
                    : UpdateSafeTimeMillis(ldtUnsafe);
        }

        /// <summary>
        /// Returns the last "safe" time as computed by a previous call to the
        /// GetSafeTimeMillis method.
        /// Note: Since the underlying field is non-volatile, the returned
        /// value is only guaranteed to be no less than the last value returned
        /// by GetSafeTimeMillis() call on the same thread.
        /// </summary>
        /// <returns>
        /// The last "safe" time in milliseconds.
        /// </returns>
        public long GetLastSafeTimeMillis()
        {
            return m_ldtLastSafe;
        }

        #endregion

        #region helper methods
        
        /// <summary>
        /// Updates and returns a "safe" current time in milliseconds based on
        /// the "unsafe" time.
        /// </summary>
        /// <param name="ldtUnsafe">
        /// The unsafe current time in milliseconds.
        /// </param>
        /// <returns>
        /// The corrected safe time.
        /// </returns>
        protected long UpdateSafeTimeMillis(long ldtUnsafe)
        {
            // cannot use BlockingLock here as will end up in a cycle between
            // ThreadTimeout.RemainingTimeMillis and DateTimeUtils.GetSafeTimeMillis()
            lock (f_thisLock)
            {
                long lJitter     = m_lJitter;
                long ldtLastSafe = m_ldtLastSafe;
                long lDelta      = ldtUnsafe - m_ldtLastUnsafe;
                long ldtNewSafe  = ldtLastSafe;

                if (lDelta > 0)
                {
                    // unsafe progressed
                    if (ldtUnsafe >= ldtLastSafe)
                    {
                        // common case; unsafe is at or ahead of safe; sync clocks
                        ldtNewSafe = ldtUnsafe;
                    }
                    else if (lDelta > lJitter && ldtLastSafe - ldtUnsafe <= lJitter)
                    {
                        // unsafe is behind safe and jumped; the jump brought
                        // it very close (within jitter) to where it was before
                        // the corresponding regression; this appears to be
                        // jitter, hold safe and avoid recording anything about
                        // this bogus jump as that could artificially push safe
                        // into the future
                        return ldtLastSafe;
                    }
                    else
                    {
                        // unsafe is behind safe and progressed; progress safe
                        // slowly at half the measured delta or every other ms
                        // if delta is 1ms allowing unsafe to eventually catch
                        // up
                        ldtNewSafe += lDelta == 1 ? ldtUnsafe%2 : lDelta/2;
                    }
                }
                else if (lDelta >= -lJitter)
                {
                    // unsafe made an insignificant (within jitter) regression;
                    // or didn't move at all; hold safe and avoid recording
                    // anything about this bogus jump as that could artificially
                    // push safe into the future
                    // Note: the same cases are handled in GetSafeTimeMillis()
                    // but based on synchronization ordering it may not be
                    // detected until here
                    return ldtLastSafe;
                }

                // except in the case of jitter we update our clocks
                m_ldtLastUnsafe = ldtUnsafe;
                return m_ldtLastSafe = ldtNewSafe;
            }
        }

        #endregion

        #region data members

        /// <summary>
        /// The last known safe time value.
        /// </summary>
        protected long m_ldtLastSafe;

        /// <summary>
        /// The last recorded unsafe time value.
        /// </summary>
        protected long m_ldtLastUnsafe;

        /// <summary>
        /// The maximum expected jitter exposed by the underlying unsafe clock.
        /// </summary>
        protected readonly long m_lJitter;

        /// <summary>
        /// Private lock for synchronizing UpdateSafeTimeMillis().
        /// </summary>
        private readonly Object f_thisLock = new Object();

        #endregion

        #region constants

        /// <summary>
        /// The default jitter threshold.
        /// </summary>
        public static readonly long DEFAULT_JITTER_THRESHOLD = 16;

        #endregion
    }
}

