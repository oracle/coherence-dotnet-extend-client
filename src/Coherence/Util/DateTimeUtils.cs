/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Util
{
    /// <summary>
    /// Miscellaneuos utility methods for DateTime manipulation.
    /// </summary>
    /// <author>Ana Cikic  2006.08.22</author>
    public abstract class DateTimeUtils
    {
        /// <summary>
        /// Returns a "safe" current time in milliseconds.
        /// </summary>
        /// <remarks>
        /// This method guarantees that the time never "goes back".
        /// More specifically, when called twice on the same thread, the
        /// second call will never return a value that is less then the value
        /// returned by the first call. If a system time correction becomes
        /// necessary, an attempt will be made to gradually compensate the
        /// returned value, so in the long run the value returned by this
        /// method is the same as the system time.
        /// </remarks>
        /// <returns>
        /// The difference, measured in milliseconds, between the corrected
        /// current time and midnight, January 1, 0001.
        /// </returns>
        public static long GetSafeTimeMillis()
        {
            return s_safeClock.GetSafeTimeMillis(DateTime.UtcNow.Ticks / 10000);
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
            return s_safeClock.GetLastSafeTimeMillis();
        }

        /// <summary>
        /// Return whether the passed in time in milliseconds is before the
        /// epoch (Jan 1, 1970).
        /// </summary>
        /// <param name="timeMillis">
        /// A timestamp in milliseconds since 1/1/0001.
        /// </param>
        /// <returns>
        /// True if the passed in time in milliseconds is before the
        /// epoch (Jan 1, 1970).
        /// </returns>
        /// <since>Coherence 3.7.1.8</since>
        public static Boolean IsBeforeTheEpoch(long timeMillis)
        {
            return timeMillis < s_theEpoch;
        }

        /// <summary>
        /// Convert the passed in time in milliseconds from time since 1/1/0001
        /// to time since the epoch (Jan 1, 1970).
        /// </summary>
        /// <param name="timeMillis">
        /// A timestamp in milliseconds since 1/1/0001.
        /// </param>
        /// <returns>
        /// The passed in time converted to milliseconds since the
        /// epoch (Jan 1, 1970).
        /// </returns>
        /// <since>Coherence 3.7.1.8</since>
        public static long GetTimeMillisSinceTheEpoch(long timeMillis)
        {
            if (IsBeforeTheEpoch(timeMillis))
            {
                throw new ArgumentException("The passed in time predates the epoch (January 1, 1970): " + GetDateTime(timeMillis));
            }
            return timeMillis - s_theEpoch;
        }

        /// <summary>
        /// Convert the passed in time in milliseconds from time since the
        /// epoch (Jan 1, 1970) to time since 1/1/0001.
        /// </summary>
        /// <param name="timeMillis">
        /// A timestamp in milliseconds since the epoch (Jan 1, 1970).
        /// </param>
        /// <returns>
        /// The passed in time converted to milliseconds since the
        /// 1/1/0001.
        /// </returns>
        /// <since>Coherence 3.7.1.8</since>
        public static long GetTimeMillisFromEpochBasedTime(long timeMillis)
        {
            return timeMillis + s_theEpoch;
        }

        /// <summary>
        /// Convert the passed in time in milliseconds to a DateTime object.
        /// </summary>
        /// <param name="timeMillis">
        /// A timestamp in milliseconds since the 1/1/0001.
        /// </param>
        /// <returns>
        /// A corresponding DateTime object for the passed in time in milliseconds.
        /// </returns>
        /// <since>Coherence 3.7.1.8</since>
        public static DateTime GetDateTime(long timeMillis)
        {
            return new DateTime(timeMillis * 10000);
        }

        /// <summary>
        /// The SafeClock
        /// </summary>
        private static SafeClock s_safeClock = new SafeClock(DateTime.UtcNow.Ticks / 10000);


        /// <summary>
        /// Timestamp in milliseconds of the epoch (Jan 1, 1970)
        /// </summary>
        private static long s_theEpoch = (new DateTime(1970, 1, 1)).Ticks / 10000;
    }
}