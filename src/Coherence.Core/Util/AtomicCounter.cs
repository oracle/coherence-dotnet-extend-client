/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Util
{
    /// <summary>
    /// Abstract base class for AtomicCounters.
    /// </summary>
    /// <remarks>
    /// The AtomicCounter allows for atomic updates to a "long" value where
    /// possible without requiring synchronization. The underlying
    /// AtomicCounter implementation will be choosen at runtime.
    /// <p/>
    /// Default implementation used in .NET 2.0 is <b>InterlockedCounter</b>,
    /// while in .NET 1.1 we have to fall back to a
    /// <b>SynchronizedCounter</b>.
    /// </remarks>
    /// <author>Goran Milosavljevic  2006.08.22</author>
    public abstract class AtomicCounter
    {
        #region AtomicCounter methods

        /// <summary>
        /// Increment the value, and return the new value.
        /// </summary>
        /// <returns>
        /// The new value.
        /// </returns>
        public long Increment()
        {
            return Increment(1L);
        }

        /// <summary>
        /// Incremenet the value by c, and return the new value.
        /// </summary>
        /// <param name="c">
        /// The amount to increment the counter by.
        /// </param>
        /// <returns>
        /// The new value.
        /// </returns>
        public long Increment(long c)
        {
            return Adjust(c);
        }

        /// <summary>
        /// Incremenet the value, and return the original value.
        /// </summary>
        /// <returns>
        /// The original value.
        /// </returns>
        public long PostIncrement()
        {
            return PostIncrement(1L);
        }

        /// <summary>
        /// Incremenet the value by c, and return the original value.
        /// </summary>
        /// <param name="c">
        /// The amount to increment the counter by.
        /// </param>
        /// <returns>
        /// The original value.
        /// </returns>
        public long PostIncrement(long c)
        {
            return PostAdjust(c);
        }

        /// <summary>
        /// Decrement the value, and return the new value.
        /// </summary>
        /// <returns>
        /// The new value.
        /// </returns>
        public long Decrement()
        {
            return Decrement(1L);
        }

        /// <summary>
        /// Decrement the value by c, and return the new value.
        /// </summary>
        /// <param name="c">
        /// The amount to decrement the counter by.
        /// </param>
        /// <returns>
        /// The new value.
        /// </returns>
        public long Decrement(long c)
        {
            return Adjust(-c);
        }

        /// <summary>
        /// Decrement the value, and return the original value.
        /// </summary>
        /// <returns>
        /// The original value.
        /// </returns>
        public long PostDecrement()
        {
            return PostDecrement(1L);
        }

        /// <summary>
        /// Decrement the value by c, and return the original value.
        /// </summary>
        /// <param name="c">
        /// The amount to decrement the counter by.
        /// </param>
        /// <returns>
        /// The original value.
        /// </returns>
        public long PostDecrement(long c)
        {
            return PostAdjust(-c);
        }

        /// <summary>
        /// Return the current value of the counter.
        /// </summary>
        /// <returns>
        /// The current value.
        /// </returns>
        public abstract long GetCount();

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
        public abstract bool SetCount(long assumedValue, long newValue);

        /// <summary>
        /// Update the current value, and return the previous value.
        /// </summary>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        /// <returns>
        /// The previous value just before the update went through.
        /// </returns>
        public abstract long SetCount(long newValue);

        #endregion

        #region Object override methods

        /// <summary>
        /// Return the count as a string.
        /// </summary>
        /// <returns>
        /// A string represenation of the count.
        /// </returns>
        public override string ToString()
        {
            return GetCount().ToString();
        }

        #endregion

        #region Helper methods

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
        protected abstract long Adjust(long c);

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
        protected abstract long PostAdjust(long c);

        #endregion

        #region Factory methods

        /// <summary>
        /// Instantiate and return a new AtomicCounter.
        /// </summary>
        /// <returns>
        /// A new AtomicCounter with a count of zero.
        /// </returns>
        public static AtomicCounter NewAtomicCounter()
        {
            return new InterlockedCounter();
        }

        /// <summary>
        /// Instantiate and return a new AtomicCounter initialized to a
        /// particular value.
        /// </summary>
        /// <param name="count">
        /// The initial counter value.
        /// </param>
        /// <returns>
        /// A new AtomicCounter with the specified counter value.
        /// </returns>
        public static AtomicCounter NewAtomicCounter(long count)
        {
            AtomicCounter counter = NewAtomicCounter();
            counter.SetCount(count);
            return counter;
        }

        #endregion
    }
}