/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Util
{
	/// <summary>
	/// Implementation of <see cref="AtomicCounter"/>.
    /// </summary>
    public class SynchronizedCounter : AtomicCounter
    {
        private long counter = 0;
        private object lockObject = new object();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SynchronizedCounter()
        {}

		/// <summary>
		/// Return the current value of the counter.
		/// </summary>
		/// <returns>
		/// The current value.
		/// </returns>
        public override long GetCount()
        {
            lock (lockObject)
            {
                return counter;
            }
        }

		/// <summary>
		/// Update the current value, only if it is equal to the assumed
		/// value.
		/// </summary>
		/// <param name="assumedValue">
		/// The assumed old value.
		/// </param>
		/// <param name="newValue">
		/// The new value.
		/// </param>
		/// <returns>
		/// <b>true</b> if the value was updated, <b>false</b> otherwise.
		/// </returns>
        public override bool SetCount(long assumedValue, long newValue)
        {
            lock (lockObject)
            {
                if (counter == assumedValue)
                {
                    counter = newValue;
                    return true;
                }
            }
            return false;
        }

		/// <summary>
		/// Update the current value, and return the previous value.
		/// </summary>
		/// <param name="newValue">
		/// The new value.
		/// </param>
		/// <returns>
		/// The previous value just before the update went through.
		/// </returns>
        public override long SetCount(long newValue)
        {
            lock (lockObject)
            {
                long prevValue = counter;
                counter = newValue;
                return prevValue;
            }
        }

		/// <summary>
		/// Adjust the value of the counter by the specified amount, and
		/// return the new value.
		/// </summary>
		/// <param name="c">
		/// The amount to adjust the counter by.
		/// </param>
		/// <returns>
		/// The new value, after the adjustment has been made.
		/// </returns>
        protected override long Adjust(long c)
        {
            lock (lockObject)
            {
                counter += c;
                return counter;
            }
        }

		/// <summary>
		/// Adjust the value of the counter by the specified amount, and
		/// return the old value.
		/// </summary>
		/// <param name="c">
		/// The amount to adjust the counter by.
		/// </param>
		/// <returns>
		/// The old value, prior to the adjustment having been made.
		/// </returns>
        protected override long PostAdjust(long c)
        {
            lock (lockObject)
            {
                long prevValue = counter;
                counter += c;
                return prevValue;
            }
        }
    }
}