/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;

using NUnit.Framework;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class RawDateTimeTests
    {
        [SetUp]
        public void SetUp()
        {
            TestContext.Error.WriteLine($"[START] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
        }

        [TearDown]
        public void TearDown()
        {
            TestContext.Error.WriteLine($"[END]   {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
        }

        [Test]
        public void TestToString()
        {
            DateTime date = new DateTime(1976, 4, 19);
            RawTime timeUtc = new RawTime(13, 2, 30, 0, true);
            RawTime time = new RawTime(13, 2, 30, 99, false);
            RawTime timeOffset = new RawTime(13, 2, 30, 0, -1, 30);
            RawDateTime datetimeUtc = new RawDateTime(date, timeUtc);
            RawDateTime datetime = new RawDateTime(date, time);
            RawDateTime datetimeOffset = new RawDateTime(date, timeOffset);

            Assert.IsTrue(datetime.ToString().StartsWith(PofHelper.FormatDate(1976, 4, 19)));
            Assert.IsTrue(datetimeUtc.ToString().EndsWith("13:02:30Z"));
            Assert.IsTrue(datetime.ToString().EndsWith("13:02:30.000000099"));
            Assert.IsTrue(datetimeOffset.ToString().EndsWith("13:02:30-01:30"));
        }

        [Test]
        public void TestEquals()
        {
            DateTime date = new DateTime(1976, 4, 19);
            RawTime timeUtc = new RawTime(13, 2, 30, 0, true);
            RawTime time = new RawTime(13, 2, 30, 99, false);
            RawDateTime datetimeUtc = new RawDateTime(date, timeUtc);
            RawDateTime datetime = new RawDateTime(date, time);

            Assert.IsTrue(datetime.Equals(datetime));
            Assert.IsFalse(datetime.Equals(datetimeUtc));
            Assert.IsTrue(datetimeUtc.Equals(new RawDateTime(new DateTime(1976, 4, 19), new RawTime(13, 2, 30, 0, true))));
        }

        [Test]
        public void TestToUniversalTime()
        {
            DateTime date = new DateTime(1976, 4, 19);
            RawTime timeUtc = new RawTime(15, 55, 0, 0, true);
            RawTime timeNoZone = new RawTime(15, 55, 0, 0, false);
            RawTime timeWithOffset = new RawTime(10, 55, 0, 0, -5, 0);
            RawDateTime datetimeUtc = new RawDateTime(date, timeUtc);
            RawDateTime datetimeNoZone = new RawDateTime(date, timeNoZone);
            RawDateTime datetimeWithOffset = new RawDateTime(date, timeWithOffset);

            DateTime dtUtc = datetimeUtc.ToUniversalTime();
            DateTime dt = new DateTime(
                datetimeUtc.Date.Year,
                datetimeUtc.Date.Month,
                datetimeUtc.Date.Day,
                datetimeUtc.Time.Hour,
                datetimeUtc.Time.Minute,
                datetimeUtc.Time.Second,
                datetimeUtc.Time.Nanosecond/1000000);
            Assert.AreEqual(dt, dtUtc);

            Assert.AreEqual(DateTimeKind.Utc, dtUtc.Kind);

            dtUtc = datetimeNoZone.ToUniversalTime();
            dt = new DateTime(
                datetimeNoZone.Date.Year,
                datetimeNoZone.Date.Month,
                datetimeNoZone.Date.Day,
                datetimeNoZone.Time.Hour,
                datetimeNoZone.Time.Minute,
                datetimeNoZone.Time.Second,
                datetimeNoZone.Time.Nanosecond / 1000000);
            Assert.AreEqual(dt, dtUtc);

            Assert.AreEqual(DateTimeKind.Utc, dtUtc.Kind);

            dtUtc = datetimeWithOffset.ToUniversalTime();
            dt = new DateTime(
                datetimeWithOffset.Date.Year,
                datetimeWithOffset.Date.Month,
                datetimeWithOffset.Date.Day,
                datetimeWithOffset.Time.Hour,
                datetimeWithOffset.Time.Minute,
                datetimeWithOffset.Time.Second,
                datetimeWithOffset.Time.Nanosecond / 1000000);
            dt = dt.Subtract(new TimeSpan(datetimeWithOffset.Time.HourOffset, datetimeWithOffset.Time.MinuteOffset, 0));
            Assert.AreEqual(dt, dtUtc);

            Assert.AreEqual(DateTimeKind.Utc, dtUtc.Kind);
        }

        [Test]
        public void TestToLocalTime()
        {
            DateTime date = new DateTime(1976, 4, 19);
            RawTime timeUtc = new RawTime(15, 55, 0, 0, true);
            RawTime timeNoZone = new RawTime(15, 55, 0, 0, false);
            RawTime timeWithOffset = new RawTime(10, 55, 0, 0, -5, 0);
            RawDateTime datetimeUtc = new RawDateTime(date, timeUtc);
            RawDateTime datetimeNoZone = new RawDateTime(date, timeNoZone);
            RawDateTime datetimeWithOffset = new RawDateTime(date, timeWithOffset);

            DateTime dtLocal = datetimeUtc.ToLocalTime();
            DateTime dt = new DateTime(
                datetimeUtc.Date.Year,
                datetimeUtc.Date.Month,
                datetimeUtc.Date.Day,
                datetimeUtc.Time.Hour,
                datetimeUtc.Time.Minute,
                datetimeUtc.Time.Second,
                datetimeUtc.Time.Nanosecond / 1000000);
            Assert.AreEqual(dt.ToLocalTime(), dtLocal);

            Assert.AreEqual(DateTimeKind.Local, dtLocal.Kind);

            dtLocal = datetimeNoZone.ToLocalTime();
            dt = new DateTime(
                datetimeNoZone.Date.Year,
                datetimeNoZone.Date.Month,
                datetimeNoZone.Date.Day,
                datetimeNoZone.Time.Hour,
                datetimeNoZone.Time.Minute,
                datetimeNoZone.Time.Second,
                datetimeNoZone.Time.Nanosecond / 1000000);
            Assert.AreEqual(dt.ToLocalTime(), dtLocal);

            Assert.AreEqual(DateTimeKind.Local, dtLocal.Kind);

            dtLocal = datetimeWithOffset.ToLocalTime();
            dt = new DateTime(
                datetimeWithOffset.Date.Year,
                datetimeWithOffset.Date.Month,
                datetimeWithOffset.Date.Day,
                datetimeWithOffset.Time.Hour,
                datetimeWithOffset.Time.Minute,
                datetimeWithOffset.Time.Second,
                datetimeWithOffset.Time.Nanosecond / 1000000);
            dt = dt.Subtract(new TimeSpan(datetimeWithOffset.Time.HourOffset, datetimeWithOffset.Time.MinuteOffset, 0));
            Assert.AreEqual(dt.ToLocalTime(), dtLocal);

            Assert.AreEqual(DateTimeKind.Local, dtLocal.Kind);
        }

        [Test]
        public void TestToDateTime()
        {
            DateTime date = new DateTime(1976, 4, 19);
            RawTime timeUtc = new RawTime(15, 55, 0, 0, true);
            RawTime timeNoZone = new RawTime(15, 55, 0, 0, false);
            RawTime timeWithOffset = new RawTime(10, 55, 0, 0, -5, 0);
            RawDateTime datetimeUtc = new RawDateTime(date, timeUtc);
            RawDateTime datetimeNoZone = new RawDateTime(date, timeNoZone);
            RawDateTime datetimeWithOffset = new RawDateTime(date, timeWithOffset);

            DateTime dtUnspecified = datetimeUtc.ToDateTime();
            DateTime dt = new DateTime(
                datetimeUtc.Date.Year,
                datetimeUtc.Date.Month,
                datetimeUtc.Date.Day,
                datetimeUtc.Time.Hour,
                datetimeUtc.Time.Minute,
                datetimeUtc.Time.Second,
                datetimeUtc.Time.Nanosecond / 1000000);
            Assert.AreEqual(dt, dtUnspecified);

            Assert.AreEqual(DateTimeKind.Unspecified, dtUnspecified.Kind);

            dtUnspecified = datetimeNoZone.ToDateTime();
            dt = new DateTime(
                datetimeNoZone.Date.Year,
                datetimeNoZone.Date.Month,
                datetimeNoZone.Date.Day,
                datetimeNoZone.Time.Hour,
                datetimeNoZone.Time.Minute,
                datetimeNoZone.Time.Second,
                datetimeNoZone.Time.Nanosecond / 1000000);
            Assert.AreEqual(dt, dtUnspecified);

            Assert.AreEqual(DateTimeKind.Unspecified, dtUnspecified.Kind);

            dtUnspecified = datetimeWithOffset.ToDateTime();
            dt = new DateTime(
                datetimeWithOffset.Date.Year,
                datetimeWithOffset.Date.Month,
                datetimeWithOffset.Date.Day,
                datetimeWithOffset.Time.Hour,
                datetimeWithOffset.Time.Minute,
                datetimeWithOffset.Time.Second,
                datetimeWithOffset.Time.Nanosecond / 1000000);
            Assert.AreEqual(dt, dtUnspecified);

            Assert.AreEqual(DateTimeKind.Unspecified, dtUnspecified.Kind);
        }
    }
}
