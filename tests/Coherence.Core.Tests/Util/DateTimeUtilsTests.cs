/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Threading;

using NUnit.Framework;

namespace Tangosol.Util
{
    [TestFixture]
    public class DateTimeUtilsTests
    {
        [Test]
        public void GetSafeTimeMillisecTest()
        {
            lock (this)
            {
                long milsec1 = DateTimeUtils.GetSafeTimeMillis();
                long milsec2 = DateTimeUtils.GetSafeTimeMillis();
                long milsec3 = DateTimeUtils.GetSafeTimeMillis();
                Assert.IsTrue(milsec1 <= milsec2);
                Assert.IsTrue(milsec1 <= milsec3);
                Assert.IsTrue(milsec2 <= milsec3);
                Thread.Sleep(5);
                long milsec4 = DateTimeUtils.GetSafeTimeMillis();
                Assert.IsTrue(milsec3 <= milsec4);
                long milsec5 = DateTimeUtils.GetSafeTimeMillis();
                Assert.IsTrue(milsec4 <= milsec5);
            }
        }

        [Test]
        public void GetSafeTimeMillisecDifference()
        {
            lock (this)
            {
                long milsec1 = DateTimeUtils.GetSafeTimeMillis();
                Monitor.Wait(this, 500);
                long milsec2 = DateTimeUtils.GetSafeTimeMillis();
                Monitor.Wait(this, 550);
                long milsec3 = DateTimeUtils.GetSafeTimeMillis();
                Assert.IsTrue(milsec2 > milsec1);
                Assert.IsTrue(milsec3 > milsec2);
            }
        }

        [Test]
        public void EpochTest()
        {
            long epochTime = new DateTime(1970, 1, 1).Ticks / 10000;
            long currentTime = DateTimeUtils.GetSafeTimeMillis();

            Assert.IsFalse(DateTimeUtils.IsBeforeTheEpoch(DateTimeUtils.GetSafeTimeMillis()));
            Assert.IsFalse(DateTimeUtils.IsBeforeTheEpoch(epochTime));
            Assert.IsTrue(DateTimeUtils.IsBeforeTheEpoch(epochTime - 1));
            Assert.IsTrue(DateTimeUtils.IsBeforeTheEpoch(1L));

            Assert.AreEqual(DateTimeUtils.GetTimeMillisSinceTheEpoch(epochTime), 0);
            Assert.AreEqual(DateTimeUtils.GetTimeMillisFromEpochBasedTime(0), epochTime);
            Assert.AreEqual(currentTime, DateTimeUtils.GetTimeMillisFromEpochBasedTime(DateTimeUtils.GetTimeMillisSinceTheEpoch(currentTime)));
        }

        [Test]
        public void EpochException()
        {
            Assert.That(() => DateTimeUtils.GetTimeMillisSinceTheEpoch(1L), Throws.ArgumentException);
        }

    }
}