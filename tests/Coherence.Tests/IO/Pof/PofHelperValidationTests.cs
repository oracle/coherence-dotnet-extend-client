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
    public class PofHelperValidationTests
    {
        [Test]
        public void TestCheckTypeWithException()
        {
            Assert.That(() => PofHelper.CheckType(-100), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckType()
        {
            PofHelper.CheckType(PofConstants.T_TIME);
            PofHelper.CheckType(10);
        }

        [Test]
        public void TestCheckElementCountWithException()
        {
            Assert.That(() => PofHelper.CheckElementCount(-5), Throws.ArgumentException);

        }

        [Test]
        public void TestCheckElementCount()
        {
            PofHelper.CheckElementCount(5);

        }

        [Test]
        public void TestCheckReferenceRange()
        {
            PofHelper.CheckReferenceRange(5);
            Assert.That(() => PofHelper.CheckReferenceRange(-5), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckYear()
        {
           PofHelper.CheckDate(11,11,11);
        }

        [Test]
        public void TestCheckMonth()
        {
            Assert.That(() => PofHelper.CheckDate(2000, 13, 11), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckDay()
        {
            Assert.That(() => PofHelper.CheckDate(2000, 11, 32), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckLeapYear()
        {
            Assert.That(() => PofHelper.CheckDate(2001, 2, 29), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckDate()
        {
            PofHelper.CheckDate(2000,11,11);
        }

        [Test]
        public void TestCheckHour()
        {
            Assert.That(() => PofHelper.CheckTime(25, 34, 10, 10), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckHour24()
        {
            Assert.That(() => PofHelper.CheckTime(24, 0, 0, 0), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckMin()
        {
            Assert.That(() => PofHelper.CheckTime(5, 98, 10, 10), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckSec()
        {
            Assert.That(() => PofHelper.CheckTime(5, 5, 100, 10), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckSec1()
        {
            Assert.That(() => PofHelper.CheckTime(5, 5, 60, 10), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckSec2()
        {
            Assert.That(() => PofHelper.CheckTime(5, 5, 60, 02), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckNSec()
        {
            Assert.That(() => PofHelper.CheckTime(5, 5, 5, -5), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckTime()
        {
            PofHelper.CheckTime(5, 5, 5, 5);
        }

        [Test]
        public void TestCheckTimeZoneWithHourException()
        {
            PofHelper.CheckTimeZone(7, 0);
            Assert.That(() => PofHelper.CheckTimeZone(25,0), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckTimeZoneWithMinuteException()
        {
            Assert.That(() => PofHelper.CheckTimeZone(7, 60), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckYearMonthInterval()
        {
            PofHelper.CheckYearMonthInterval(1,0);
            PofHelper.CheckYearMonthInterval(0,5);
            Assert.That(() => PofHelper.CheckYearMonthInterval(0,15), Throws.ArgumentException);
        }

        [Test]
        public void TestCheckTimeInterval()
        {
            PofHelper.CheckTimeInterval(9,9,9,10);
            PofHelper.CheckTimeInterval(0,9,9,10);
            PofHelper.CheckTimeInterval(0,0,9,10);
            PofHelper.CheckTimeInterval(0,0,0,10);
        }

        [Test]
        public void TestCheckDayTimeInterval()
        {
            PofHelper.CheckDayTimeInterval(0,9,9,9,10);
            PofHelper.CheckDayTimeInterval(1,9,9,9,10);

        }

        [Test]
        public void TestFormatDate()
        {
            PofHelper.FormatDate(2006, 11, 11);
        }
    }
}
