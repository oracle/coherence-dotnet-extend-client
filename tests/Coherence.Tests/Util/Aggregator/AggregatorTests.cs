/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

using NUnit.Framework;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Util.Collections;
using Tangosol.Util.Comparator;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;

namespace Tangosol.Util.Aggregator {

    [TestFixture]
    public class AggregatorTests
    {
        NameValueCollection appSettings = TestUtils.AppSettings;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
        }

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
        public void TestComparableMaxAggregator()
        {
            ComparableMax agg1 = new ComparableMax(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            ComparableMax agg2 = new ComparableMax("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor),agg2.Extractor);

            ComparableMax agg3 = new ComparableMax("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173));
            al.Add(new TestInvocableCacheEntry("Goran", 185));
            al.Add(new TestInvocableCacheEntry("Ana", 164));
            Assert.AreEqual(185, agg1.Aggregate(al));

            string[] array = new string[] {"Ana", "Ivan", "Goran"};
            Assert.AreEqual("Ivan", agg1.AggregateResults(array));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("comparableMaxKey1", 435);
            ht.Add("comparableMaxKey2", 253);
            ht.Add("comparableMaxKey3", 3);
            ht.Add("comparableMaxKey4", null);
            ht.Add("comparableMaxKey5", -3);
            cache.InsertAll(ht);

            IEntryAggregator aggregator = new ComparableMax(IdentityExtractor.Instance);
            object max = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(max, 435);

            IFilter filter = new AlwaysFilter();
            max = cache.Aggregate(filter, aggregator);
            Assert.AreEqual(max, 435);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestComparableComparerAggregator()
        {
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("1", new Address("street", "city", "state", "00000"));
            ht.Add("2", new Address("street", "city", "state", "20000"));
            ht.Add("3", new Address("street", "city", "state", "60000"));
            ht.Add("4", new Address("street", "city", "state", "50000"));
            ht.Add("5", new Address("street", "city", "state", "10000"));
            cache.InsertAll(ht);

            ArrayList al = new ArrayList(ht.Values);

            // setup for ComparableMax tests

            ComparableMax maxAgg = new ComparableMax(IdentityExtractor.Instance,
                new SimpleAddressComparer());
            Assert.IsNotNull(maxAgg);
            Assert.AreSame(IdentityExtractor.Instance, maxAgg.Extractor);

            // local aggregation
            Assert.AreEqual("60000", ((Address)maxAgg.AggregateResults(al)).ZIP);

            // remote aggregation
            Address maxAddr = (Address) cache.Aggregate(new AlwaysFilter(), maxAgg);
            Assert.AreEqual("60000", maxAddr.ZIP);

            // setup for ComparableMin tests

            ComparableMin minAgg = new ComparableMin(IdentityExtractor.Instance,
                new SimpleAddressComparer());
            Assert.IsNotNull(minAgg);
            Assert.AreSame(IdentityExtractor.Instance, minAgg.Extractor);

            // local aggregation
            Assert.AreEqual("00000", ((Address)minAgg.AggregateResults(al)).ZIP);

            // remote aggregation
            Address minAddr = (Address)cache.Aggregate(new AlwaysFilter(), minAgg);
            Assert.AreEqual("00000", minAddr.ZIP);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestComparableMinAggregator()
        {
            ComparableMin agg1 = new ComparableMin(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            ComparableMin agg2 = new ComparableMin("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            ComparableMin agg3 = new ComparableMin("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173));
            al.Add(new TestInvocableCacheEntry("Goran", 185));
            al.Add(new TestInvocableCacheEntry("Ana", 164));
            Assert.AreEqual(164, agg1.Aggregate(al));

            string[] array = new string[] { "Ana", "Ivan", "Goran" };
            Assert.AreEqual("Ana", agg1.AggregateResults(array));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("comparableMinKey1", 435);
            ht.Add("comparableMinKey2", 253);
            ht.Add("comparableMinKey3", 3);
            ht.Add("comparableMinKey4", null);
            ht.Add("comparableMinKey5", -3);
            cache.InsertAll(ht);

            IEntryAggregator aggregator = new ComparableMin(IdentityExtractor.Instance);
            object min = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(min, -3);

            IFilter filter = new AlwaysFilter();
            min = cache.Aggregate(filter, aggregator);
            Assert.AreEqual(min, -3);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestCountAggregator()
        {
            Count agg1 = new Count();
            Assert.IsNotNull(agg1);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173));
            al.Add(new TestInvocableCacheEntry("Goran", 185));
            al.Add(new TestInvocableCacheEntry("Ana", 164));
            Assert.AreEqual(3, agg1.Aggregate(al));

            Assert.AreEqual(10, agg1.AggregateResults(new object[] {agg1.Aggregate(al), 7}));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("countKey1", 435);
            ht.Add("countKey2", 253);
            ht.Add("countKey3", 3);
            ht.Add("countKey4", null);
            ht.Add("countKey5", -3);
            cache.InsertAll(ht);

            IEntryAggregator aggregator = new Count();
            object count = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(ht.Count, count);

            IFilter filter = new GreaterFilter(IdentityExtractor.Instance, 100);
            count = cache.Aggregate(filter, aggregator);
            Assert.AreEqual(2, count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestDistinctValuesAggregator()
        {
            DistinctValues agg1 = new DistinctValues(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            DistinctValues agg2 = new DistinctValues("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            DistinctValues agg3 = new DistinctValues("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173));
            al.Add(new TestInvocableCacheEntry("Milica", 173));
            al.Add(new TestInvocableCacheEntry("Goran", 185));
            al.Add(new TestInvocableCacheEntry("Ana", 164));
            ICollection coll = (ICollection) agg1.Aggregate(al);
            Assert.AreEqual(3, coll.Count);

            agg1 = new DistinctValues(IdentityExtractor.Instance);
            al.Clear();
            coll = (ICollection) agg1.Aggregate(al);
            Assert.AreEqual(0, coll.Count);
            Assert.IsInstanceOf(typeof(NullImplementation.NullCollection), coll);

            string[] array1 = new string[] { "Ana", "Ivan", "Goran"};
            string[] array2 = new string[] { "Aleks", "Jason", "Ana"};
            coll = (ICollection) agg1.AggregateResults(new object[]{array1, array2});
            Assert.AreEqual(6, coll.Count);

            // aggragation on remote cache
            Hashtable ht = new Hashtable();
            ht.Add("distinctValuesKey1", 435);
            ht.Add("distinctValuesKey2", 253);
            ht.Add("distinctValuesKey3", 3);
            ht.Add("distinctValuesKey4", 3);
            ht.Add("distinctValuesKey5", 3);
            ht.Add("distinctValuesKey6", null);
            ht.Add("distinctValuesKey7", null);

            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.InsertAll(ht);

            IEntryAggregator aggregator = new DistinctValues(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(3, ((ICollection)result).Count);
            foreach(object o in ht.Values)
            {
                Assert.IsTrue(((IList)result).Contains(o) || o == null);
            }
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 100);
            result = cache.Aggregate(filter, aggregator);
            Assert.AreEqual(1, ((ICollection)result).Count);

            //test case for COHNET-109
            filter = new LessFilter(IdentityExtractor.Instance, 0);
            result = cache.Aggregate(filter, aggregator);
            Assert.AreEqual(0, ((ICollection) result).Count);
            Assert.IsInstanceOf(typeof (NullImplementation.NullCollection), result);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestDecimalAverageAggregator()
        {
            DecimalAverage agg1 = new DecimalAverage(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            DecimalAverage agg2 = new DecimalAverage("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            DecimalAverage agg3 = new DecimalAverage("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173.6M));
            al.Add(new TestInvocableCacheEntry("Goran", 185.1M));
            al.Add(new TestInvocableCacheEntry("Ana", -123456789012345678901234.08M));
            Assert.AreEqual((173.6M + 185.1M - 123456789012345678901234.08M) / 3, agg1.Aggregate(al));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("decimalAvgKey1", 100M);
            ht.Add("decimalAvgKey2", 80.5M);
            ht.Add("decimalAvgKey3", 19.5M);
            ht.Add("decimalAvgKey4", -12345678901234571234.08M);
            cache.InsertAll(ht);

            IEntryAggregator aggregator = new DecimalAverage(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual((100M + 80.5M + 19.5M - 12345678901234571234.08M) / 4, result);

            cache.Insert("comparableKey5", null);
            result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual((100M + 80.5M + 19.5M - 12345678901234571234.08M) / 4, result);

            IFilter filter = new AlwaysFilter();
            result = cache.Aggregate(filter, aggregator);
            Assert.AreEqual((100M + 80.5M + 19.5M - 12345678901234571234.08M) / 4, result);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestDecimalMaxAggregator()
        {
            DecimalMax agg1 = new DecimalMax(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            DecimalMax agg2 = new DecimalMax("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            DecimalMax agg3 = new DecimalMax("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173.6M));
            al.Add(new TestInvocableCacheEntry("Goran", 185.1M));
            al.Add(new TestInvocableCacheEntry("Ana", -123456789012345678901234.08M));
            Assert.AreEqual(185.1, agg1.Aggregate(al));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key1", 100);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 123456789012345678901234.08M);
            ht.Add("key5", 1011);
            cache.InsertAll(ht);

            IEntryAggregator aggregator = new DecimalMax(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(123456789012345678901234.08M, result);

            cache.Insert("key5", Decimal.MaxValue);
            result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(Decimal.MaxValue, result);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestDecimalMinAggregator()
        {
            DecimalMin agg1 = new DecimalMin(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            DecimalMin agg2 = new DecimalMin("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            DecimalMin agg3 = new DecimalMin("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", -173.6M));
            al.Add(new TestInvocableCacheEntry("Goran", 185.1M));
            al.Add(new TestInvocableCacheEntry("Ana", 1643426876432.08M));
            Assert.AreEqual(-173.6M, agg1.Aggregate(al));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key1", 100M);
            ht.Add("key2", 80.523423423423M);
            ht.Add("key3", 4643321321426876432.08M);
            ht.Add("key4", 1643426876432.08M);
            ht.Add("key5", 1011M);
            cache.InsertAll(ht);

            IEntryAggregator aggregator = new DecimalMin(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(80.523423423423M, result);

            cache.Insert("key10", -10.23896128635231234M);
            IFilter filter = new AlwaysFilter();
            result = cache.Aggregate(filter, aggregator);
            Assert.AreEqual(-10.23896128635231234M, result);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestDecimalSumAggregator()
        {
            DecimalSum agg1 = new DecimalSum(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            DecimalSum agg2 = new DecimalSum("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            DecimalSum agg3 = new DecimalSum("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 132131273.643M));
            al.Add(new TestInvocableCacheEntry("Goran", -0.43432M));
            al.Add(new TestInvocableCacheEntry("Ana", 0M));
            Assert.AreEqual((132131273.643M + -0.43432M + 0M), agg1.Aggregate(al));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key1", 100M);
            ht.Add("key2", 80.5M);
            ht.Add("key3", 19.589328917623187963289176M);
            ht.Add("key4", 1M);

            cache.InsertAll(ht);

            IEntryAggregator aggregator = new DecimalSum(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual((100M + 80.5M + 19.589328917623187963289176M + 1M), result);

            IFilter filter = new AlwaysFilter();
            result = cache.Aggregate(filter, aggregator);
            Assert.AreEqual((100M + 80.5M + 19.589328917623187963289176M + 1M), result);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestDoubleAverageAggregator()
        {
            DoubleAverage agg1 = new DoubleAverage(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            DoubleAverage agg2 = new DoubleAverage("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            DoubleAverage agg3 = new DoubleAverage("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173.6));
            al.Add(new TestInvocableCacheEntry("Milica", 173.22));
            al.Add(new TestInvocableCacheEntry("Goran", 185.1));
            al.Add(new TestInvocableCacheEntry("Ana", 164.08));
            Assert.AreEqual((173.6+173.22+185.1+164.08)/4, agg1.Aggregate(al));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("doubleAvgKey1", 100);
            ht.Add("doubleAvgKey2", 80.5);
            ht.Add("doubleAvgKey3", 19.5);
            ht.Add("doubleAvgKey4", 2);
            cache.InsertAll(ht);

            IEntryAggregator aggregator = new DoubleAverage(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(50.5, result);

            cache.Insert("comparableKey5", null);
            result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(50.5, result);

            IFilter filter = new AlwaysFilter();
            result = cache.Aggregate(filter, aggregator);
            Assert.AreEqual(50.5, result);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestDoubleMaxAggregator()
        {
            DoubleMax agg1 = new DoubleMax(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            DoubleMax agg2 = new DoubleMax("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            DoubleMax agg3 = new DoubleMax("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173.6));
            al.Add(new TestInvocableCacheEntry("Milica", 173.22));
            al.Add(new TestInvocableCacheEntry("Goran", 185.1));
            al.Add(new TestInvocableCacheEntry("Ana", 164.08));
            Assert.AreEqual(185.1, agg1.Aggregate(al));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key1", 100);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2);
            ht.Add("key5", 1011);
            cache.InsertAll(ht);

            IEntryAggregator aggregator = new DoubleMax(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(1011, result);

            cache.Insert("key5", Int32.MaxValue);
            result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual((double)Int32.MaxValue, result);

            cache.Insert("key6", Int64.MaxValue);
            result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual((double)Int64.MaxValue, result);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestDoubleMinAggregator()
        {
            DoubleMin agg1 = new DoubleMin(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            DoubleMin agg2 = new DoubleMin("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            DoubleMin agg3 = new DoubleMin("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173.6));
            al.Add(new TestInvocableCacheEntry("Milica", 173.22));
            al.Add(new TestInvocableCacheEntry("Goran", 185.1));
            al.Add(new TestInvocableCacheEntry("Ana", 164.08));
            Assert.AreEqual(164.08, agg1.Aggregate(al));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key1", 100);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2);
            ht.Add("key5", 1011);
            cache.InsertAll(ht);

            IEntryAggregator aggregator = new DoubleMin(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(2, result);

            cache.Insert("key10", -10);
            IFilter filter = new AlwaysFilter();
            result = cache.Aggregate(filter, aggregator);
            Assert.AreEqual(-10, result);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestDoubleSumAggregator()
        {
            DoubleSum agg1 = new DoubleSum(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            DoubleSum agg2 = new DoubleSum("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            DoubleSum agg3 = new DoubleSum("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173.6));
            al.Add(new TestInvocableCacheEntry("Milica", 173.22));
            al.Add(new TestInvocableCacheEntry("Goran", 185.1));
            al.Add(new TestInvocableCacheEntry("Ana", 164.08));
            Assert.AreEqual((173.6 + 173.22 + 185.1 + 164.08), agg1.Aggregate(al));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key1", 100);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2);

            cache.InsertAll(ht);

            IEntryAggregator aggregator = new DoubleSum(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(202, result);

            IFilter filter = new AlwaysFilter();
            result = cache.Aggregate(filter, aggregator);
            Assert.AreEqual(202, result);

            CacheFactory.Shutdown();
        }


        [Test]
        public void TestLongMaxAggregator()
        {
            LongMax agg1 = new LongMax(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            LongMax agg2 = new LongMax("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            LongMax agg3 = new LongMax("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173));
            al.Add(new TestInvocableCacheEntry("Milica", 173));
            al.Add(new TestInvocableCacheEntry("Goran", 185.1));
            al.Add(new TestInvocableCacheEntry("Ana", 164.08));
            Assert.AreEqual(185, agg1.Aggregate(al));

            al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173));
            al.Add(new TestInvocableCacheEntry("Milica", 173));
            al.Add(new TestInvocableCacheEntry("Goran", 185));
            al.Add(new TestInvocableCacheEntry("Ana", 164));
            Assert.AreEqual(185, agg1.Aggregate(al));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key1", 1011.21);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2);
            ht.Add("key5", 1010);
            cache.InsertAll(ht);

            IEntryAggregator aggregator = new LongMax(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(1011, result);

            cache.Insert("key5", Int32.MaxValue);
            result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(Int32.MaxValue, result);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestLongMinAggregator()
        {
            LongMin agg1 = new LongMin(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            LongMin agg2 = new LongMin("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            LongMin agg3 = new LongMin("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", -173.6));
            al.Add(new TestInvocableCacheEntry("Milica", 173.22));
            al.Add(new TestInvocableCacheEntry("Goran", 185.1));
            al.Add(new TestInvocableCacheEntry("Ana", -164.08));
            Assert.AreEqual(-174, agg1.Aggregate(al));

            al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173));
            al.Add(new TestInvocableCacheEntry("Milica", 173));
            al.Add(new TestInvocableCacheEntry("Goran", 185));
            al.Add(new TestInvocableCacheEntry("Ana", 164));
            Assert.AreEqual(164, agg1.Aggregate(al));
            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key1", 100);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2);
            ht.Add("key5", 1011);
            ht.Add("key6", -10.5);

            cache.InsertAll(ht);

            IEntryAggregator aggregator = new LongMin(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(-10, result);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestLongSumAggregator()
        {
            LongSum agg1 = new LongSum(IdentityExtractor.Instance);
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);

            LongSum agg2 = new LongSum("dummy");
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg2.Extractor);

            LongSum agg3 = new LongSum("another.dummy");
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("Ivan", 173.6));
            al.Add(new TestInvocableCacheEntry("Milica", 173.22));
            al.Add(new TestInvocableCacheEntry("Goran", 185.1));
            al.Add(new TestInvocableCacheEntry("Ana", 164.08));
            Assert.AreEqual(174+173+185+164, agg1.Aggregate(al));

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key1", 100);
            ht.Add("key2", 80);
            ht.Add("key3", 19);
            ht.Add("key4", 2);
            ht.Add("key5", null);

            cache.InsertAll(ht);

            IEntryAggregator aggregator = new LongSum(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(201, result);

            IFilter filter = new GreaterFilter(IdentityExtractor.Instance, 1);
            result = cache.Aggregate(filter, aggregator);
            Assert.AreEqual(201, result);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestGroupAggregator()
        {
            GroupAggregator agg1 = GroupAggregator.CreateInstance(IdentityExtractor.Instance,
                                                                  new Count());
            Assert.IsNotNull(agg1);
            Assert.AreSame(IdentityExtractor.Instance, agg1.Extractor);
            Assert.IsInstanceOf(typeof(Count), agg1.Aggregator);

            GroupAggregator agg2 = GroupAggregator.CreateInstance(IdentityExtractor.Instance,
                                                                  new Count(),
                                                                  new LessFilter(
                                                                          IdentityExtractor.Instance, 3));
            Assert.IsNotNull(agg2);
            Assert.IsInstanceOf(typeof(IdentityExtractor), agg2.Extractor);

            GroupAggregator agg3 =
                    GroupAggregator.CreateInstance("dummy", new Count());
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), agg3.Extractor);
            agg3 = GroupAggregator.CreateInstance("dummy.test", new Count());
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(ChainedExtractor), agg3.Extractor);
            agg3 = GroupAggregator.CreateInstance("dummy.test1, dummy.test2", new Count());
            Assert.IsNotNull(agg3);
            Assert.IsInstanceOf(typeof(MultiExtractor), agg3.Extractor);

            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("key1", 173));
            al.Add(new TestInvocableCacheEntry("key2", 173));
            al.Add(new TestInvocableCacheEntry("key3", 185));
            al.Add(new TestInvocableCacheEntry("key4", 164));
            al.Add(new TestInvocableCacheEntry("key5", 164));
            al.Add(new TestInvocableCacheEntry("key6", 164));
            object result = agg2.Aggregate(al);
            if (result is IDictionary)
            {
                Assert.AreEqual(((IDictionary)result)[173], 2);
                Assert.AreEqual(((IDictionary)result)[185], 1);
                Assert.AreEqual(((IDictionary)result)[164], null);
            }

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("Key1", 435);
            ht.Add("Key2", 253);
            ht.Add("Key3", 435);
            ht.Add("Key4", 435);
            ht.Add("Key5", -3);
            cache.InsertAll(ht);

            IEntryAggregator aggregator = GroupAggregator.CreateInstance(IdentityExtractor.Instance,
                                                                         new Count());
            result = cache.Aggregate(cache.Keys, aggregator);
            if (result is IDictionary)
            {
                Assert.AreEqual(((IDictionary)result)[435], 3);
                Assert.AreEqual(((IDictionary)result)[-3], 1);
            }
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestCompositeAggregator()
        {
            IEntryAggregator agg1= CompositeAggregator.CreateInstance(
                    new IEntryAggregator[] {GroupAggregator.CreateInstance(IdentityExtractor.Instance,
                                                                           new Count()),
                                            new LongMax((IdentityExtractor.Instance))
                                           });
            ArrayList al = new ArrayList();
            al.Add(new TestInvocableCacheEntry("key1", 173));
            al.Add(new TestInvocableCacheEntry("key2", 173));
            al.Add(new TestInvocableCacheEntry("key3", 185));
            al.Add(new TestInvocableCacheEntry("key4", 164));
            al.Add(new TestInvocableCacheEntry("key5", 164));
            al.Add(new TestInvocableCacheEntry("key6", 164));
            object result = agg1.Aggregate(al);
            if (result is IList)
            {
                IDictionary results = (IDictionary)((IList)result)[0];

                Assert.AreEqual(results[185], 1);
                Assert.AreEqual(results[164], 3);

                Assert.AreEqual(((IList)result)[1], 185);
            }

            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            HashDictionary hd = new HashDictionary();
            hd.Add("Key1", 435);
            hd.Add("Key2", 253);
            hd.Add("Key3", 435);
            hd.Add("Key4", 435);
            hd.Add(null, -3);
            cache.InsertAll(hd);

            IEntryAggregator aggregator = CompositeAggregator.CreateInstance(
                    new IEntryAggregator[] {GroupAggregator.CreateInstance(IdentityExtractor.Instance,
                                                                           new Count()),
                                            new LongMax((IdentityExtractor.Instance))
                                           });
            result = cache.Aggregate(cache.Keys, aggregator);

            if (result is IList)
            {
                IDictionary results = (IDictionary)((IList)result)[0];

                Assert.AreEqual(results[435], 3);
                Assert.AreEqual(results[-3], 1);

                Assert.AreEqual(((IList)result)[1], 435);
            }

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestAggregatorSerialization()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.IsNotNull(ctx);

            ComparableMax comparableMax = new ComparableMax("member1");
            ComparableMin comparableMin = new ComparableMin("member2");
            CompositeAggregator compositeAggregator = new CompositeAggregator();
            DecimalAverage decimalAverage = new DecimalAverage("member3");
            DecimalMax decimalMax = new DecimalMax("member4");
            DecimalMin decimalMin = new DecimalMin("member5");
            DecimalSum decimalSum = new DecimalSum("member6");
            DistinctValues distinctValues = new DistinctValues("member7");
            DoubleAverage doubleAverage = new DoubleAverage("member8");
            DoubleMax doubleMax = new DoubleMax("member9");
            DoubleMin doubleMin = new DoubleMin("member10");
            DoubleSum doubleSum = new DoubleSum("member11");
            GroupAggregator groupAggregator = new GroupAggregator();
            LongMax longMax = new LongMax("member12");
            LongMin longMin = new LongMin("member13");
            LongSum longSum = new LongSum("member14");
            PriorityAggregator priorityAggregator = new PriorityAggregator();
            Count count = new Count();

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), comparableMax);
            ctx.Serialize(new DataWriter(stream), comparableMin);
            ctx.Serialize(new DataWriter(stream), compositeAggregator);
            ctx.Serialize(new DataWriter(stream), decimalAverage);
            ctx.Serialize(new DataWriter(stream), decimalMax);
            ctx.Serialize(new DataWriter(stream), decimalMin);
            ctx.Serialize(new DataWriter(stream), decimalSum);
            ctx.Serialize(new DataWriter(stream), distinctValues);
            ctx.Serialize(new DataWriter(stream), doubleAverage);
            ctx.Serialize(new DataWriter(stream), doubleMax);
            ctx.Serialize(new DataWriter(stream), doubleMin);
            ctx.Serialize(new DataWriter(stream), doubleSum);
            ctx.Serialize(new DataWriter(stream), groupAggregator);
            ctx.Serialize(new DataWriter(stream), longMax);
            ctx.Serialize(new DataWriter(stream), longMin);
            ctx.Serialize(new DataWriter(stream), longSum);
            ctx.Serialize(new DataWriter(stream), priorityAggregator);
            ctx.Serialize(new DataWriter(stream), count);

            stream.Position = 0;
            Assert.AreEqual(comparableMax, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(comparableMin, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(compositeAggregator, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(decimalAverage, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(decimalMax, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(decimalMin, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(decimalSum, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(distinctValues, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(doubleAverage, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(doubleMax, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(doubleMin, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(doubleSum, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(groupAggregator, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(longMax, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(longMin, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(longSum, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(priorityAggregator.GetType(), ctx.Deserialize(new DataReader(stream)).GetType());
            Assert.AreEqual(count.GetType(), ctx.Deserialize(new DataReader(stream)).GetType());

            stream.Close();
        }

        [Test]
        public void TestQueryRecorder()
        {
            // aggragation on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();

            for (int i = 0; i < 200; ++i)
            {
                ht.Add("countKey" + i, i);
            }

            cache.InsertAll(ht);

            IFilter filter = new OrFilter(
                new GreaterFilter(IdentityExtractor.Instance, 100),
                new LessFilter(IdentityExtractor.Instance, 30));
            //explain
            QueryRecorder aggregator = new QueryRecorder(QueryRecorder.RecordType.Explain);
            IQueryRecord record = (IQueryRecord) cache.Aggregate(filter, aggregator);

            //Console.WriteLine(record.ToString());
            Assert.AreEqual(1, record.Results.Count);
            Assert.AreEqual(1, ((IPartialResult)record.Results[0]).Steps.Count);
            Assert.AreEqual(2, ((IStep)((IPartialResult)record.Results[0]).Steps[0]).Steps.Count);

            //trace
            aggregator = new QueryRecorder(QueryRecorder.RecordType.Trace);

            record = (IQueryRecord)cache.Aggregate(filter, aggregator);

            //Console.WriteLine(record.ToString());
            Assert.AreEqual(1, record.Results.Count);
            Assert.AreEqual(2, ((IPartialResult)record.Results[0]).Steps.Count);
            Assert.AreEqual(2, ((IStep)((IPartialResult)record.Results[0]).Steps[0]).Steps.Count);
            Assert.AreEqual(2, ((IStep)((IPartialResult)record.Results[0]).Steps[1]).Steps.Count);

            CacheFactory.Shutdown();
        }

        /// <summary>
        /// Test of the ReducerAggregator aggregator.
        /// </summary>
        [Test]
        public void TestReducerAggregator()
        {
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("1", new Address("street1", "city", "state", "00000"));
            ht.Add("2", new Address("street2", "city", "state", "20000"));
            ht.Add("3", new Address("street3", "city", "state", "60000"));
            ht.Add("4", new Address("street4", "city", "state", "50000"));
            ht.Add("5", new Address("street5", "city", "state", "10000"));
            cache.InsertAll(ht);

            // Remote execution, java method names
            ReducerAggregator agent = new ReducerAggregator(new MultiExtractor("getStreet,getZip"));
            IDictionary       m     = (IDictionary) cache.Aggregate(AlwaysFilter.Instance, agent);

            Assert.AreEqual(m.Count, cache.Count);

            object[] results = (object[]) m["1"];
            Assert.AreEqual(2,         results.Length);
            Assert.AreEqual("street1", results[0]);
            Assert.AreEqual("00000",   results[1]);

            results = (object[]) m["2"];
            Assert.AreEqual(2,         results.Length);
            Assert.AreEqual("street2", results[0]);
            Assert.AreEqual("20000",   results[1]);

            // local aggregation
            ReducerAggregator localAgent = new ReducerAggregator(new MultiExtractor("Street,ZIP"));
            m = (IDictionary) localAgent.AggregateResults(ht);

            results = (object[]) m["1"];
            Assert.AreEqual(2,         results.Length);
            Assert.AreEqual("street1", results[0]);
            Assert.AreEqual("00000",   results[1]);

            results = (object[]) m["2"];
            Assert.AreEqual(2,         results.Length);
            Assert.AreEqual("street2", results[0]);
            Assert.AreEqual("20000",   results[1]);

            CacheFactory.Shutdown();
        }

        /// <summary>
        /// Test of the TopNAggregator aggregator.
        /// </summary>
        [Test]
        public void TestTopNAggregator()
        {
            DoTestTopN(CacheName);
            DoTestTopN("local-foo");
        }

        protected void DoTestTopN(string sCache)
        {
            INamedCache cache = CacheFactory.GetCache(sCache);
            cache.Clear();

            TopNAggregator agent = new TopNAggregator(
                           IdentityExtractor.Instance, SafeComparer.Instance, 10);

            object[] aoEmpty = new Object[0];
            object[] aoResult;

            aoResult = (object[]) cache.Aggregate(
                       NullImplementation.GetCollection(), agent);
            AssertArrayEquals(aoEmpty, aoResult, "null collection");

            aoResult = (object[]) cache.Aggregate(
                       new ArrayList(new object[] {"1"}), agent);
            AssertArrayEquals(aoEmpty, aoResult, "singleton collection");

            aoResult = (object[]) cache.Aggregate((IFilter) null, agent);
            AssertArrayEquals(aoEmpty, aoResult, "null filter");

            aoResult = (object[]) cache.Aggregate(AlwaysFilter.Instance, agent);
            AssertArrayEquals(aoEmpty, aoResult, "AlwaysFilter");

            Hashtable ht    = new Hashtable();
            int       cKeys = 10000;
            for (int i = 1; i <= cKeys; i++)
            {
                ht.Add(i.ToString(), i);
            }
            cache.InsertAll(ht);

            object[] aoTop10 = new object[10];
            for (int i = 0; i < 10; i++)
                {
                aoTop10[i] = cKeys - i;
                }

            aoResult = (object[]) cache.Aggregate(
                     NullImplementation.GetCollection(), agent);
            AssertArrayEquals(aoEmpty, aoResult);

            aoResult = (object[]) cache.Aggregate(
                       new ArrayList(new object[] {"1"}), agent);
            AssertArrayEquals(new object[] {1}, aoResult);

            aoResult = (object[]) cache.Aggregate(
                       new ArrayList(new object[] {"1"}), agent);
            AssertArrayEquals(new object[] {1}, aoResult);

            aoResult = (object[]) cache.Aggregate((IFilter) null, agent);
            AssertArrayEquals(aoTop10, aoResult);

            aoResult = (object[]) cache.Aggregate(AlwaysFilter.Instance, agent);
            AssertArrayEquals(aoTop10, aoResult);

            // test duplicate values
            cache.Clear();

            cKeys = 100;
            ht    = new Hashtable(cKeys);
            for (int i = 1; i <= cKeys; ++i)
            {
                ht.Add(i.ToString(), i / 2);
            }
            cache.InsertAll(ht);

            aoTop10 = new object[10];
            for (int i = 0; i < 10; ++i)
            {
                aoTop10[i] = (cKeys - i) / 2;
            }

            aoResult = (object[]) cache.Aggregate((IFilter) null, agent);
            AssertArrayEquals(aoTop10, aoResult);

            CacheFactory.Shutdown();
        }

        private void AssertArrayEquals(object[] aoExpect, object[] aoActual)
        {
            AssertArrayEquals(aoExpect, aoActual, null);
        }

        private void AssertArrayEquals(object[] aoExpect, object[] aoActual, string description)
        {
            Assert.AreEqual(aoExpect.Length, aoActual.Length, description +  ": Array length");

            for (int i = 0, c = aoExpect.Length; i < c; i++)
            {
                Assert.AreEqual(aoExpect[i], aoActual[i], description + ": Array entry " + i);
            }
        }

    }
}
