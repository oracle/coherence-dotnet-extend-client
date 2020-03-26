/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Threading;
using NUnit.Framework;
using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    [TestFixture]
    public class SimpleCacheStatisticsTests
    {
        [Test]
        public void TestPuts()
        {
            SimpleCacheStatistics stats = new SimpleCacheStatistics();
            int count = 0;
            Assert.AreEqual(stats.TotalPuts, count);
            Assert.AreEqual(stats.TotalPutsMillis, 0);
            Assert.AreEqual(stats.AveragePutMillis, 0);

            long start = DateTimeUtils.GetSafeTimeMillis();
            Thread.Sleep(10); //so that TotalPutsMillis wouldn't be 0
            stats.RegisterPut(start);
            count++;
            Assert.AreEqual(stats.TotalPuts, count);
            Assert.AreEqual(stats.AveragePutMillis, (double) stats.TotalPutsMillis);

            stats.RegisterPut(0);
            count++;
            Assert.AreEqual(stats.TotalPuts, count);
            Assert.AreEqual(stats.AveragePutMillis, (double) stats.TotalPutsMillis/(double) count);

            int n = 5;
            start = DateTimeUtils.GetSafeTimeMillis();
            Thread.Sleep(10);
            stats.RegisterPuts(n, start);
            count += n;
            Assert.AreEqual(stats.TotalPuts, count);
            Assert.AreEqual(stats.AveragePutMillis, (double) stats.TotalPutsMillis/(double) count);

            stats.ResetHitStatistics();
            Assert.AreEqual(stats.TotalPuts, 0);
            Assert.AreEqual(stats.TotalPutsMillis, 0);
            Assert.AreEqual(stats.AveragePutMillis, 0);
        }

        [Test]
        public void TestHits() {
            SimpleCacheStatistics stats = new SimpleCacheStatistics();
            int count = 0;
            Assert.AreEqual(stats.CacheHits, count);
            Assert.AreEqual(stats.CacheHitsMillis, 0);
            Assert.AreEqual(stats.AverageHitMillis, 0);
            Assert.AreEqual(stats.HitProbability, 0);

            long start = DateTimeUtils.GetSafeTimeMillis();
            Thread.Sleep(10); //so that CacheHitsMillis wouldn't be 0
            stats.RegisterHit(start);
            count++;
            Assert.AreEqual(stats.CacheHits, count);
            Assert.AreEqual(stats.AverageHitMillis, (double) stats.CacheHitsMillis);

            stats.RegisterHit(0);
            count++;
            Assert.AreEqual(stats.CacheHits, count);
            Assert.AreEqual(stats.AverageHitMillis, (double) stats.CacheHitsMillis / (double) count);

            int n = 5;
            start = DateTimeUtils.GetSafeTimeMillis();
            Thread.Sleep(10);
            stats.RegisterHits(n, start);
            count += n;
            Assert.AreEqual(stats.CacheHits, count);
            Assert.AreEqual(stats.AverageHitMillis, (double) stats.CacheHitsMillis / (double) count);

            Assert.AreEqual(stats.HitProbability, 1);

            stats.ResetHitStatistics();
            Assert.AreEqual(stats.CacheHits, 0);
            Assert.AreEqual(stats.CacheHitsMillis, 0);
            Assert.AreEqual(stats.AverageHitMillis, 0);
        }

        [Test]
        public void TestMisses() {
            SimpleCacheStatistics stats = new SimpleCacheStatistics();
            int count = 0;
            Assert.AreEqual(stats.CacheMisses, count);
            Assert.AreEqual(stats.CacheMissesMillis, 0);
            Assert.AreEqual(stats.AverageMissMillis, 0);
            Assert.AreEqual(stats.HitProbability, 0);

            long start = DateTimeUtils.GetSafeTimeMillis();
            Thread.Sleep(10); //so that CacheMissesMillis wouldn't be 0
            stats.RegisterMiss(start);
            count++;
            Assert.AreEqual(stats.CacheMisses, count);
            Assert.AreEqual(stats.AverageMissMillis, (double) stats.CacheMissesMillis);

            stats.RegisterMiss(0);
            count++;
            Assert.AreEqual(stats.CacheMisses, count);
            Assert.AreEqual(stats.AverageMissMillis, (double) stats.CacheMissesMillis / (double) count);

            int n = 5;
            start = DateTimeUtils.GetSafeTimeMillis();
            Thread.Sleep(10);
            stats.RegisterMisses(n, start);
            count += n;
            Assert.AreEqual(stats.CacheMisses, count);
            Assert.AreEqual(stats.AverageMissMillis, (double) stats.CacheMissesMillis / (double) count);

            Assert.AreEqual(stats.HitProbability, 0);

            stats.ResetHitStatistics();
            Assert.AreEqual(stats.CacheMisses, 0);
            Assert.AreEqual(stats.CacheMissesMillis, 0);
            Assert.AreEqual(stats.AverageMissMillis, 0);
        }

        [Test]
        public void TestGets() {
            SimpleCacheStatistics stats = new SimpleCacheStatistics();
            int countHits = 0;
            int countMisses = 0;
            Assert.AreEqual(stats.TotalGets, 0);
            Assert.AreEqual(stats.TotalGetsMillis, 0);
            Assert.AreEqual(stats.AverageGetMillis, 0);
            Assert.AreEqual(stats.HitProbability, 0);

            long start = DateTimeUtils.GetSafeTimeMillis();
            Thread.Sleep(10); //so that TotalGetsMillis wouldn't be 0
            stats.RegisterHit(start);
            countHits++;
            Assert.AreEqual(stats.TotalGets, countHits);
            Assert.AreEqual(stats.AverageGetMillis, (double) stats.TotalGetsMillis);
            Assert.AreEqual(stats.HitProbability, 1);

            start = DateTimeUtils.GetSafeTimeMillis();
            Thread.Sleep(10);
            stats.RegisterMiss(start);
            countMisses++;
            Assert.AreEqual(stats.TotalGets, countHits + countMisses);
            Assert.AreEqual(stats.AverageGetMillis, (double) stats.TotalGetsMillis/(double) (countHits + countMisses));
            Assert.AreEqual(stats.HitProbability, 0.5);

            int n = 5;
            stats.RegisterHits(n, 0);
            countHits += n;
            Assert.AreEqual(stats.TotalGets, countHits + countMisses);
            Assert.AreEqual(stats.AverageGetMillis, (double) stats.TotalGetsMillis / (double) (countHits + countMisses));

            stats.ResetHitStatistics();
            Assert.AreEqual(stats.TotalGets, 0);
            Assert.AreEqual(stats.TotalGetsMillis, 0);
            Assert.AreEqual(stats.AverageGetMillis, 0);
        }
    }
}