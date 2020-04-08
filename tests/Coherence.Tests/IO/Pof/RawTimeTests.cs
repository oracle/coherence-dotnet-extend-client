/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using NUnit.Framework;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class RawTimeTests
    {
        [Test]
        public void TestToString()
        {
            RawTime timeUtc = new RawTime(13, 2, 30, 0, true);
            RawTime time = new RawTime(13, 2, 30, 99, false);
            RawTime timeOffset = new RawTime(13, 2, 30, 0, -1, 30);
            Assert.IsTrue(timeUtc.ToString().StartsWith("13:02:30Z"));
            Assert.IsTrue(time.ToString().StartsWith("13:02:30.000000099"));
            Assert.IsTrue(timeOffset.ToString().StartsWith("13:02:30-01"));
        }

        [Test]
        public void TestEquals()
        {
            RawTime timeUtc = new RawTime(13, 2, 30, 0, true);
            RawTime time = new RawTime(13, 2, 30, 99, false);
            RawTime timeOffset = new RawTime(13, 2, 30, 0, -1, 30);
            Assert.IsTrue(time.Equals(time));
            Assert.IsTrue(timeUtc.Equals(new RawTime(13, 2, 30, 0, true)));
            Assert.IsFalse(time.Equals(new RawTime(13, 2, 30, 99, true)));
            Assert.IsTrue(timeOffset.Equals(new RawTime(13, 2, 30, 0, -1, 30)));
        }

        [Test]
        public void TestToUniversalTime()
        {
            RawTime timeUtc = new RawTime(15, 55, 0, 0, true);
            RawTime timeNoZone = new RawTime(15, 55, 0, 0, false);
            RawTime timeWithOffset = new RawTime(10, 55, 0, 0, -5, 0);

            DateTime dtUtc = timeUtc.ToUniversalTime();
            Assert.AreEqual(timeUtc.Hour, dtUtc.Hour);
            Assert.AreEqual(timeUtc.Minute, dtUtc.Minute);
            Assert.AreEqual(timeUtc.Second, dtUtc.Second);
            Assert.AreEqual(timeUtc.Nanosecond, dtUtc.Millisecond*1000000);
            Assert.AreEqual(DateTimeKind.Utc, dtUtc.Kind);

            dtUtc = timeNoZone.ToUniversalTime();
            DateTime dt = new DateTime(1, 1, 1,
                                       timeNoZone.Hour,
                                       timeNoZone.Minute,
                                       timeNoZone.Second,
                                       timeNoZone.Nanosecond/1000000);
            Assert.AreEqual(dt, dtUtc);

            Assert.AreEqual(DateTimeKind.Utc, dtUtc.Kind);
            dtUtc = timeWithOffset.ToUniversalTime();
            dt = new DateTime(1, 1, 1,
                                       timeWithOffset.Hour,
                                       timeWithOffset.Minute,
                                       timeWithOffset.Second,
                                       timeWithOffset.Nanosecond/1000000);
            dt = dt.Subtract(new TimeSpan(timeWithOffset.HourOffset, timeWithOffset.MinuteOffset, 0));
            Assert.AreEqual(dt, dtUtc);
            Assert.AreEqual(DateTimeKind.Utc, dtUtc.Kind);
        }

        [Test]
        public void TestToLocalTime()
        {
            RawTime timeUtc = new RawTime(15, 55, 0, 0, true);
            RawTime timeNoZone = new RawTime(15, 55, 0, 0, false);
            RawTime timeWithOffset = new RawTime(10, 55, 0, 0, -5, 0);

            DateTime dtLocal = timeUtc.ToLocalTime();
            DateTime dt = new DateTime(1, 1, 1,
                                       timeUtc.Hour,
                                       timeUtc.Minute,
                                       timeUtc.Second,
                                       timeUtc.Nanosecond / 1000000);
            Assert.AreEqual(dt.ToLocalTime(), dtLocal);

            Assert.AreEqual(DateTimeKind.Local, dtLocal.Kind);
            dtLocal = timeNoZone.ToLocalTime();
            dt = new DateTime(1, 1, 1, 15, 55, 0, 0);
            Assert.AreEqual(dt.ToLocalTime(), dtLocal);
            Assert.AreEqual(DateTimeKind.Local, dtLocal.Kind);
            dtLocal = timeWithOffset.ToLocalTime();
            dt = new DateTime(1, 1, 1,
                              timeWithOffset.Hour,
                              timeWithOffset.Minute,
                              timeWithOffset.Second,
                              timeWithOffset.Nanosecond / 1000000);
            dt = dt.Subtract(new TimeSpan(timeWithOffset.HourOffset, timeWithOffset.MinuteOffset, 0));
            Assert.AreEqual(dt.ToLocalTime(), dtLocal);
            Assert.AreEqual(DateTimeKind.Local, dtLocal.Kind);
        }

        [Test]
        public void TestToDateTime()
        {
            RawTime timeUtc = new RawTime(15, 55, 0, 0, true);
            RawTime timeNoZone = new RawTime(15, 55, 0, 0, false);
            RawTime timeWithOffset = new RawTime(10, 55, 0, 0, -5, 0);

            DateTime dtUnspecified = timeUtc.ToDateTime();
            DateTime dt = new DateTime(1, 1, 1,
                                       timeUtc.Hour,
                                       timeUtc.Minute,
                                       timeUtc.Second,
                                       timeUtc.Nanosecond / 1000000);
            Assert.AreEqual(dt, dtUnspecified);

            Assert.AreEqual(DateTimeKind.Unspecified, dtUnspecified.Kind);
            dtUnspecified = timeNoZone.ToDateTime();
            dt = new DateTime(1, 1, 1, 15, 55, 0, 0);
            Assert.AreEqual(dt, dtUnspecified);
            Assert.AreEqual(DateTimeKind.Unspecified, dtUnspecified.Kind);
            dtUnspecified = timeWithOffset.ToDateTime();
            dt = new DateTime(1, 1, 1,
                              timeWithOffset.Hour,
                              timeWithOffset.Minute,
                              timeWithOffset.Second,
                              timeWithOffset.Nanosecond / 1000000);

            Assert.AreEqual(dt, dtUnspecified);
            Assert.AreEqual(DateTimeKind.Unspecified, dtUnspecified.Kind);
        }
    }
}
