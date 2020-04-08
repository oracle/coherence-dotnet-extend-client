/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.IO.Pof
{
    /// <summary>
    /// An immutable POF year-month interval value.
    /// </summary>
    /// <author>Cameron Purdy  2006.07.17</author>
    /// <author>Aleksandar Seovic  2006.08.14</author>
    /// <since>Coherence 3.2</since>
    public struct RawYearMonthInterval
    {
        #region Constructors

        /// <summary>
        /// Constructs a year-month interval value.
        /// </summary>
        /// <param name="years">
        /// The number of years in the year-month interval.
        /// </param>
        /// <param name="months">
        /// The number of months in the year-month interval.
        /// </param>
        public RawYearMonthInterval(int years, int months)
        {
            PofHelper.CheckYearMonthInterval(years, months);

            m_years  = years;
            m_months = months;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of years in the year-month interval.
        /// </summary>
        /// <value>
        /// The number of years in the year-month interval.
        /// </value>
        public int Years
        {
            get { return m_years; }
        }

        /// <summary>
        /// Gets the number of months in the year-month interval.
        /// </summary>
        /// <returns>
        /// The number of months in the year-month interval.
        /// </returns>
        public int Months
        {
            get { return m_months; }
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare this object with another for equality.
        /// </summary>
        /// <param name="o">
        /// Another object to compare to for equality.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this object is equal to the other object.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is RawYearMonthInterval)
            {
                RawYearMonthInterval that = (RawYearMonthInterval) o;
                return Years == that.Years && Months == that.Months;
            }

            return false;
        }

        /// <summary>
        /// Obtain the hashcode for this object.
        /// </summary>
        /// <returns>
        /// An integer hashcode.
        /// </returns>
        public override int GetHashCode()
        {
            return Years ^ Months;
        }

        /// <summary>
        /// Format this object's data as a human-readable string.
        /// </summary>
        /// <returns>
        /// A string description of this object.
        /// </returns>
        public override string ToString()
        {
            return "Years=" + Years + ", Months=" + Months;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The number of years in the year-month interval.
        /// </summary>
        private int m_years;

        /// <summary>
        /// The number of months in the year-month interval.
        /// </summary>
        private int m_months;

        #endregion
    }
}