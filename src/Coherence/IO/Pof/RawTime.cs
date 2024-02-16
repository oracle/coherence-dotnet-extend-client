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
    /// An immutable time value.
    /// </summary>
    /// <author>Ivan Cikic  2007.03.14</author>
    /// <since>Coherence 3.2</since>
    public class RawTime
    {
        #region Constructors

        /// <summary>
        /// Constructs a raw POF time value.
        /// </summary>
        /// <param name="hour">
        /// The hours (0 through 23).
        /// </param>
        /// <param name="minute">
        /// The minutes (0 through 59).
        /// </param>
        /// <param name="second">
        /// The seconds (0 through 59).
        /// </param>
        /// <param name="nanosecond">
        /// The nanoseconds (0 through 999999999).
        /// </param>
        /// <param name="isUTC">
        /// <b>true</b> if the time value is UTC or <b>false</b> if the time
        /// value does not have an explicit time zone.
        /// </param>
        public RawTime(int hour, int minute, int second, int nanosecond, bool isUTC) 
            : this(hour, minute, second, nanosecond)
        {
            m_timeZoneType = isUTC ? TZ_UTC : TZ_NONE;
        }

       
        /// <summary>
        /// Construct a raw POF time value with a timezone.
        /// </summary>
        /// <param name="hour">
        /// The hours (0 through 23).
        /// </param>
        /// <param name="minute">
        /// The minutes (0 through 59).
        /// </param>
        /// <param name="second">
        /// The seconds (0 through 59).
        /// </param>
        /// <param name="nanosecond">
        /// The nanoseconds (0 through 999999999).
        /// </param>
        /// <param name="hourOffset">
        /// The timezone offset in hours from UTC, for example 0 for BST, -5
        /// for EST and 1 for CET.
        /// </param>
        /// <param name="minuteOffset">
        /// The timezone offset in minutes, for example 0 (in most cases) or
        /// 30.
        /// </param>
        public RawTime(int hour, int minute, int second, int nanosecond,
                       int hourOffset, int minuteOffset) 
            : this(hour, minute, second, nanosecond)
        {

            PofHelper.CheckTimeZone(hourOffset, minuteOffset);
            
            m_timeZoneType = TZ_OFFSET;
            m_hourOffset   = hourOffset;
            m_minuteOffset = minuteOffset;
        }


        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="hour">
        /// The hours (0 through 23).
        /// </param>
        /// <param name="minute">
        /// The minutes (0 through 59).
        /// </param>
        /// <param name="second">
        /// The seconds (0 through 59).
        /// </param>
        /// <param name="nanosecond">
        /// The nanoseconds (0 through 999999999).
        /// </param>
        private RawTime(int hour, int minute, int second, int nanosecond)
        {
            PofHelper.CheckTime(hour, minute, second, nanosecond);
            m_hour       = hour;
            m_minute     = minute;
            m_second     = second;
            m_nanosecond = nanosecond;
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Determine the time's hour value.
        /// </summary>
        /// <value>
        /// The hour between 0 and 23 inclusive.
        /// </value>
        public int Hour
        {
            get { return m_hour; }
        }

        /// <summary>
        /// Determine the time's minute value.
        /// </summary>
        /// <value>
        /// The minute between 0 and 23 inclusive.
        /// </value>
        public int Minute
        {
            get { return m_minute; }
        }

        /// <summary>
        /// Determine the time's second value.
        /// </summary>
        /// <value>
        /// The second value between 0 and 59 inclusive.
        /// </value>
        public int Second
        {
            get { return m_second; }
        }

        /// <summary>
        /// Determine the time's nanosecond value.
        /// </summary>
        /// <value>
        /// The nanosecond value between 0 and 999999999 inclusive.
        /// </value>
        public int Nanosecond
        {
            get { return m_nanosecond; }
        }
        
        /// <summary>
        /// Determine if the time value has an explicit timezone. 
        /// </summary>
        /// <remarks>
        /// A time value without an explicit timezone is assumed to be in 
        /// some conventional local timezone, according to ISO8601.
        /// </remarks>
        /// <value>
        /// <b>true</b> iff the time has an explicit timezone.
        /// </value>
        public bool HasTimezone
        {
            get { return m_timeZoneType != TZ_NONE; }
        }
        
        /// <summary>
        /// Determine if the time value uses UTC.
        /// </summary>
        /// <value>
        /// <b>true</b> if the time value is a UTC value.
        /// </value>
        public bool IsUtc
        {
            get { return m_timeZoneType == TZ_UTC; }
        }

        /// <summary>
        /// Determine the timezone's hour offset value.
        /// </summary>
        /// <value>
        /// The hour offset of the timezeone, or zero if there is no
        /// explicit timezone or the time is UTC.
        /// </value>
        public int HourOffset
        {
            get { return m_hourOffset; }
        }

        /// <summary>
        /// Determine the timezone's minute offset value.
        /// </summary>
        /// <value>
        /// The minute offset of the timezeone, or zero if there is no
        /// explicit timezone or the time is UTC.
        /// </value>
        public int MinuteOffset
        {
            get { return m_minuteOffset; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts this instance into a <b>DateTime</b> representation,
        /// ignoring any time zone information if present.
        /// </summary>
        /// <returns>
        /// A <b>DateTime</b> object based on this instance.
        /// </returns>
        public DateTime ToDateTime()
        {
            DateTime datetime = new DateTime(1, 1, 1, Hour, Minute, Second, Nanosecond / 1000000, DateTimeKind.Unspecified);

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

            DateTime datetime = new DateTime(1, 1, 1, Hour, Minute, Second, Nanosecond / 1000000, DateTimeKind.Utc);

            if (HasTimezone && !IsUtc)
            {
                TimeSpan offset = new TimeSpan(HourOffset, MinuteOffset, 0);

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
            if (o is RawTime)
            {
                RawTime that = (RawTime) o;
                return this == that
                          || Hour         == that.Hour
                          && Minute       == that.Minute
                          && Second       == that.Second
                          && Nanosecond   == that.Nanosecond
                          && IsUtc        == that.IsUtc
                          && HourOffset   == that.HourOffset
                          && MinuteOffset == that.MinuteOffset;
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
            return (Hour << 2) ^ (Minute << 1) ^ Second ^ Nanosecond;
        }

        /// <summary>
        /// Format this object's data as a human-readable string.
        /// </summary>
        /// <returns>
        /// A string description of this object.
        /// </returns>
        public override string ToString()
        {
            return HasTimezone && !IsUtc
                       ? PofHelper.FormatTime(Hour, Minute, Second, Nanosecond,HourOffset, MinuteOffset)
                       : PofHelper.FormatTime(Hour, Minute, Second, Nanosecond,IsUtc);
        }

        #endregion

        #region Constants
        
        /// <summary>
        /// Indicates that the time value does not have an explicit time 
        /// zone.
        /// </summary>
        private static readonly int TZ_NONE   = 0;
        
        /// <summary>
        /// Indicates that the time value is in UTC.
        /// </summary>
        private static readonly int TZ_UTC    = 1;
        
        /// <summary>
        /// Indicates that the time value has an explicit time zone.
        /// </summary>
        private static readonly int TZ_OFFSET = 2;
        
        #endregion
        
        #region Data members

        /// <summary>
        /// The hour number.
        /// </summary>
        private int m_hour;

        /// <summary>
        /// The minute number.
        /// </summary>
        private int m_minute;

        /// <summary>
        /// The second number.
        /// </summary>
        private int m_second;

        /// <summary>
        /// The nanosecond number.
        /// </summary>
        private int m_nanosecond;

        /// <summary>
        /// The timezone indicator, one of the TZ_ enumerated constants.
        /// </summary>
        private int m_timeZoneType;

        /// <summary>
        /// The hour offset of the time's timezone.
        /// </summary>
        private int m_hourOffset;

        /// <summary>
        /// The minute offset of the time's timezone.
        /// </summary>
        private int m_minuteOffset;

        #endregion
    }
}


       
 