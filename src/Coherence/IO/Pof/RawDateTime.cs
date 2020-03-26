/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// An immutable POF date-time value.
    /// </summary>
    /// <author>Cameron Purdy  2006.07.17</author>
    /// <author>Ana Cikic  2009.08.25</author>
    /// <since>Coherence 3.2</since>
    public class RawDateTime
    {
        #region Constructors

        /// <summary>
        /// Construct a date-time value.
        /// </summary>
        /// <param name="date">
        /// The date portion of the raw date-time value.
        /// </param>
        /// <param name="time">
        /// The time portion of the raw date-time value.
        /// </param>
        public RawDateTime(DateTime date, RawTime time)
        {
            if (time == null)
            {
                throw new ArgumentNullException("time");
            }

            m_date = date;
            m_time = time;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The date portion of the raw date-time value.
        /// </summary>
        /// <value>
        /// The date portion of the raw date-time value.
        /// </value>
        public DateTime Date
        {
            get { return m_date; }
        }

        /// <summary>
        /// The time portion of the raw date-time value.
        /// </summary>
        /// <value>
        /// The time portion of the raw date-time value.
        /// </value>
        public RawTime Time
        {
            get { return m_time; }
        }

        #endregion

        #region Conversions

        /// <summary>
        /// Converts this instance into a <b>DateTime</b> representation,
        /// ignoring any time zone information if present.
        /// </summary>
        /// <returns>
        /// A <b>DateTime</b> object based on this instance.
        /// </returns>
        public DateTime ToDateTime()
        {
            DateTime datetime = new DateTime(
                Date.Year,
                Date.Month,
                Date.Day,
                Time.Hour,
                Time.Minute,
                Time.Second,
                Time.Nanosecond / 1000000,
                DateTimeKind.Unspecified);

            return datetime;
        }

        /// <summary>
        /// Converts this instance into a Coordinated Universal Time (UTC)
        /// <b>DateTime</b> representation.
        /// </summary>
        /// <returns>
        /// A UTC <b>DateTime</b> object based on this instance.
        /// </returns>
        public DateTime ToUniversalTime()
        {
            DateTime datetime = new DateTime(
                Date.Year,
                Date.Month,
                Date.Day,
                Time.Hour,
                Time.Minute,
                Time.Second,
                Time.Nanosecond / 1000000,
                DateTimeKind.Utc);

            if (Time.HasTimezone && !Time.IsUtc)
            {
                TimeSpan offset = new TimeSpan(Time.HourOffset, Time.MinuteOffset, 0);

                datetime = DateTime.SpecifyKind(datetime.Subtract(offset), DateTimeKind.Utc);
            }

            return datetime;
        }

        /// <summary>
        /// Converts this instance into a local <b>DateTime</b> 
        /// representation.
        /// </summary>
        /// <returns>
        /// A local <b>DateTime</b> object based on this instance.
        /// </returns>
        public DateTime ToLocalTime()
        {
            return ToUniversalTime().ToLocalTime();
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
            if (o is RawDateTime)
            {
                RawDateTime that = (RawDateTime) o;
                return this == that
                    || Date.Equals(that.Date)
                    && Time.Equals(that.Time);
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
            return Date.GetHashCode() ^ Time.GetHashCode();
        }

        /// <summary>
        /// Format this object's data as a human-readable string.
        /// </summary>
        /// <returns>
        /// A string description of this object.
        /// </returns>
        public override string ToString()
        {
            return PofHelper.FormatDate(Date.Year, Date.Month, Date.Day) + ' ' + Time;
        }

        #endregion

        #region Data Members

        /// <summary>
        /// The date portion of the raw date-time value.
        /// </summary>
        private readonly DateTime m_date;

        /// <summary>
        /// The time portion of the raw date-time value.
        /// </summary>
        private readonly RawTime m_time;

        #endregion
    }
}
