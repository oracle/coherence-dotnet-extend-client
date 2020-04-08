/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Threading;

namespace Tangosol.Util
{
    /// <summary>
    /// Implementation of <see cref="AtomicCounter"/> based on .NET
    /// <b>System.Threading.Interlocked</b> class.
    /// </summary>
    /// <author>Ana Cikic  2006.08.23</author>
    /// <seealso cref="AtomicCounter"/>
    public class InterlockedCounter : AtomicCounter
    {
        /// <summary>
        /// Return the current value of the counter.
        /// </summary>
        /// <returns>
        /// The current value.
        /// </returns>
        public override long GetCount()
        {
            return Interlocked.Read(ref m_counter);
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
            // Interlocked.CompareExchange returns the original value in counter
            long old = Interlocked.CompareExchange(ref m_counter, newValue, assumedValue);
            return (old == assumedValue);
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
            return Interlocked.Exchange(ref m_counter, newValue);
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
            return Interlocked.Add(ref m_counter, c);
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
            long newValue = Interlocked.Add(ref m_counter, c);
            return newValue - c;
        }

        /// <summary>
        /// The actual counter value.
        /// </summary>
        protected long m_counter;
    }
}