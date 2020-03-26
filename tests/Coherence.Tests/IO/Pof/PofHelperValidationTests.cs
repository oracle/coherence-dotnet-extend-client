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
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckTypeWithException()
        {
            PofHelper.CheckType(-100);

        }
        [Test]
        public void TestCheckType()
        {
            PofHelper.CheckType(PofConstants.T_TIME);
            PofHelper.CheckType(10);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckElementCountWithException()
        {
            PofHelper.CheckElementCount(-5);

        }
        [Test]
        public void TestCheckElementCount()
        {
            PofHelper.CheckElementCount(5);

        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckReferenceRange()
        {
            PofHelper.CheckReferenceRange(5);
            PofHelper.CheckReferenceRange(-5);
        }

        [Test]
        public void TestCheckYear()
        {
           PofHelper.CheckDate(11,11,11);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckMonth()
        {
            PofHelper.CheckDate(2000, 13, 11);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckDay()
        {
            PofHelper.CheckDate(2000, 11, 32);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckLeapYear()
        {
            PofHelper.CheckDate(2001, 2, 29);
        }
        [Test]
        public void TestCheckDate()
        {
            PofHelper.CheckDate(2000,11,11);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckHour()
        {
            PofHelper.CheckTime(25, 34, 10, 10);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckHour24()
        {
            PofHelper.CheckTime(24, 0, 0, 0);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckMin()
        {
            PofHelper.CheckTime(5, 98, 10, 10);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckSec()
        {
            PofHelper.CheckTime(5, 5, 100, 10);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckSec1()
        {
            PofHelper.CheckTime(5, 5, 60, 10);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckSec2()
        {
            PofHelper.CheckTime(5, 5, 60, 02);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckNSec()
        {
            PofHelper.CheckTime(5, 5, 5, -5);
        }
        [Test]
        public void TestCheckTime()
        {
            PofHelper.CheckTime(5, 5, 5, 5);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckTimeZoneWithHourException()
        {
            PofHelper.CheckTimeZone(7, 0);
            PofHelper.CheckTimeZone(25,0);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckTimeZoneWithMinuteException()
        {
            PofHelper.CheckTimeZone(7, 60);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCheckYearMonthInterval()
        {
            PofHelper.CheckYearMonthInterval(1,0);
            PofHelper.CheckYearMonthInterval(0,5);
            PofHelper.CheckYearMonthInterval(0,15);
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
