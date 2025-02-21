/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Threading;

using NUnit.Framework;

using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Aggregator;
using Tangosol.Util.Comparator;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Impl
{
    [TestFixture]
    public class LocalNamedCacheTests
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
        public void TestPropertiesAndMethods()
        {
            LocalNamedCache lnc = new LocalNamedCache(10, 10, null);
            Assert.IsNotNull(lnc);
            lnc = new LocalNamedCache(10, 10, 15.5);
            Assert.IsNotNull(lnc);

            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            Assert.IsTrue(cache is LocalNamedCache);

            LocalNamedCache localNamedCache = cache as LocalNamedCache;

            Assert.IsNotNull(localNamedCache);
            Assert.AreEqual("local-default", localNamedCache.CacheName);
            Assert.IsNull(localNamedCache.CacheService);

            Assert.IsTrue(localNamedCache.IsActive);
            Assert.IsFalse(localNamedCache.IsReleased);

            Assert.IsFalse(localNamedCache.IsReadOnly);
            Assert.IsFalse(localNamedCache.IsFixedSize);

            Assert.IsNotNull(localNamedCache.SyncRoot);

            Assert.IsTrue(localNamedCache.IsSynchronized);

            localNamedCache.Release();
            localNamedCache.Destroy();
        }

        [Test]
        public void TestInvoke()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("conditionalPutKey1", 435);
            ht.Add("conditionalPutKey2", 253);
            ht.Add("conditionalPutKey3", 3);
            ht.Add("conditionalPutKey4", 200);
            ht.Add("conditionalPutKey5", 333);
            cache.InsertAll(ht);

            IFilter greaterThen600 = new GreaterFilter(IdentityExtractor.Instance, 600);
            IFilter greaterThen200 = new GreaterFilter(IdentityExtractor.Instance, 200);

            // no entries with value>600
            ICollection keys = cache.GetKeys(greaterThen600);
            Assert.IsTrue(keys.Count == 0);

            // invoke processor for one entry with filter that will evaluate to false
            // again, no entries with value>600
            IEntryProcessor processor = new ConditionalPut(greaterThen600, 666);
            cache.Invoke("conditionalPutKey1", processor);
            keys = cache.GetKeys(greaterThen600);
            Assert.IsTrue(keys.Count == 0);

            // invoke processor for one entry with filter that will evaluate to true
            // this will change one entry
            processor = new ConditionalPut(AlwaysFilter.Instance, 666);
            cache.Invoke("conditionalPutKey1", processor);
            keys = cache.GetKeys(greaterThen600);
            Assert.AreEqual(keys.Count, 1);
            Assert.AreEqual(cache["conditionalPutKey1"], 666);

            // 3 entries with value>200
            keys = cache.GetKeys(greaterThen200);
            Assert.AreEqual(keys.Count, 3);

            // invoke processor for these three entries
            processor = new ConditionalPut(greaterThen200, 666);
            cache.InvokeAll(cache.Keys, processor);
            keys = cache.GetKeys(greaterThen600);
            Assert.AreEqual(keys.Count, 3);

            cache.Clear();

            ht = new Hashtable();
            ht.Add("conditionalPutAllKey1", 435);
            ht.Add("conditionalPutAllKey2", 253);
            ht.Add("conditionalPutAllKey3", 200);
            ht.Add("conditionalPutAllKey4", 333);
            cache.InsertAll(ht);

            Hashtable htPut = new Hashtable();
            htPut.Add("conditionalPutAllKey1", 100);
            htPut.Add("conditionalPutAllKey6", 80);
            htPut.Add("conditionalPutAllKey3", 10);

            // put key1 and compare cache value with the put one
            processor = new ConditionalPutAll(AlwaysFilter.Instance, htPut);
            cache.Invoke("conditionalPutAllKey1", processor);
            Assert.IsNotNull(cache["conditionalPutAllKey1"]);
            Assert.AreEqual(cache["conditionalPutAllKey1"], htPut["conditionalPutAllKey1"]);

            // TODO: Decide wheter putall should insert new entries or not
            // put all keys from htPut and compare cache values with put ones
            //cache.InvokeAll(htPut.Keys, processor);
            //Assert.IsTrue(cache.Count == 5);
            //Assert.AreEqual(cache["conditionalPutAllKey1"], htPut["conditionalPutAllKey1"]);
            //Assert.AreEqual(cache["conditionalPutAllKey6"], htPut["conditionalPutAllKey6"]);
            //Assert.AreEqual(cache["conditionalPutAllKey3"], htPut["conditionalPutAllKey3"]);

            //htPut.Clear();
            //htPut.Add("conditionalPutAllKey4", 355);
            //processor = new ConditionalPutAll(AlwaysFilter.Instance, htPut);

            //cache.InvokeAll(new GreaterFilter(IdentityExtractor.Instance, 300), processor);
            //Assert.AreEqual(cache["conditionalPutAllKey4"], htPut["conditionalPutAllKey4"]);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestRemove()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("conditionalPutAllKey1", 435);
            ht.Add("conditionalPutAllKey2", 253);
            ht.Add("conditionalPutAllKey3", 200);
            ht.Add("conditionalPutAllKey4", 333);
            cache.InsertAll(ht);

            IFilter greaterThen300 = new GreaterFilter(IdentityExtractor.Instance, 300);
            IFilter lessThen300 = new LessFilter(IdentityExtractor.Instance, 300);
            IFilter key3 = new LikeFilter(new KeyExtractor(IdentityExtractor.Instance), "%Key3", '\\', false);

            Assert.IsTrue(cache.Count == 4);

            // remove key1 with greaterThen300 filter applied
            ConditionalRemove processor = new ConditionalRemove(greaterThen300, false);
            cache.Invoke("conditionalPutAllKey1", processor);
            Assert.IsTrue(cache.Count == 3);

            // remove all entries that satisfy filter criteria
            processor = new ConditionalRemove(greaterThen300, false);
            cache.InvokeAll(ht.Keys, processor);
            Assert.IsTrue(cache.Count == 2);
            Assert.IsNotNull(cache["conditionalPutAllKey2"]);
            Assert.IsNotNull(cache["conditionalPutAllKey3"]);

            processor = new ConditionalRemove(lessThen300, false);
            cache.InvokeAll(new GreaterFilter(IdentityExtractor.Instance, 200), processor);
            Assert.IsTrue(cache.Count == 1);
            Assert.IsNotNull(cache["conditionalPutAllKey3"]);

            processor = new ConditionalRemove(key3, false);
            cache.InvokeAll(new GreaterFilter(IdentityExtractor.Instance, 100), processor);
            Assert.IsTrue(cache.Count == 0);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestConditional()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("conditionalKey1", 200);
            ht.Add("conditionalKey2", 250);
            ht.Add("conditionalKey3", 300);
            ht.Add("conditionalKey4", 400);
            cache.InsertAll(ht);


            IFilter lessThen300 = new LessFilter(IdentityExtractor.Instance, 300);
            ConditionalProcessor processor = new ConditionalProcessor(
                new GreaterFilter(IdentityExtractor.Instance, 200),
                new ConditionalRemove(lessThen300, false));

            Assert.IsTrue(cache.Count == 4);
            cache.Invoke("conditionalKey4", processor);
            Assert.IsTrue(cache.Count == 4);

            cache.Invoke("conditionalKey3", processor);
            Assert.IsTrue(cache.Count == 4);

            cache.Invoke("conditionalKey2", processor);
            Assert.IsTrue(cache.Count == 3);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestComposite()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            cache.Clear();

            Address addr1 = new Address("XI krajiske divizije", "Belgrade", "Serbia", "11000");
            Address addr2 = new Address("Pere Velimirovica", "Belgrade", "Serbia", "11000");
            Address addr3 = new Address("Rige od Fere", "Belgrade", "Serbia", "11000");
            cache.Insert("addr1", addr1);
            cache.Insert("addr2", addr2);

            Assert.IsTrue(cache.Count == 2);

            LikeFilter likeXI = new LikeFilter(new ReflectionExtractor("Street"), "XI%", '\\', true);
            ExtractorProcessor extractStreet = new ExtractorProcessor(new ReflectionExtractor("Street"));
            IEntryProcessor putAddr3 = new ConditionalPut(AlwaysFilter.Instance, addr3);
            IEntryProcessor removeLikeXI = new ConditionalRemove(likeXI, false);
            IEntryProcessor[] processors = new IEntryProcessor[] { extractStreet, removeLikeXI, putAddr3 };
            CompositeProcessor processor = new CompositeProcessor(processors);
            
            Object objResult = cache.Invoke("addr1", processor);

            Assert.IsTrue(cache.Count == 2);
            object[] objResultArr = objResult as object[];
            Assert.IsNotNull(objResultArr);
            Assert.AreEqual(addr1.Street, objResultArr[0]);

            Address res = cache["addr1"] as Address;
            Assert.IsNotNull(res);
            Assert.AreEqual(addr3.City, res.City);
            Assert.AreEqual(addr3.State, res.State);
            Assert.AreEqual(addr3.Street, res.Street);
            Assert.AreEqual(addr3.ZIP, res.ZIP);

            res = cache["addr2"] as Address;
            Assert.IsNotNull(res);
            Assert.AreEqual(addr2.City, res.City);
            Assert.AreEqual(addr2.State, res.State);
            Assert.AreEqual(addr2.Street, res.Street);
            Assert.AreEqual(addr2.ZIP, res.ZIP);

            IDictionary dictResult = cache.InvokeAll(new ArrayList(new object[] { "addr1", "addr2" }), processor);

            Assert.IsTrue(cache.Count == 2);
            Address address = cache["addr1"] as Address;
            Assert.IsNotNull(address);
            Assert.AreEqual(addr3.Street, address.Street);
            address = cache["addr2"] as Address;
            Assert.IsNotNull(address);
            Assert.AreEqual(addr3.Street, address.Street);
            object[] objectArr = dictResult["addr1"] as object[];
            Assert.IsNotNull(objectArr);
            Assert.AreEqual(objectArr[0], addr3.Street);
            objectArr = dictResult["addr2"] as object[];
            Assert.IsNotNull(objectArr);
            Assert.AreEqual(objectArr[0], addr2.Street);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestExtractor()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            cache.Clear();
            ExtractorProcessor processor = new ExtractorProcessor(new ReflectionExtractor("Street"));
            Address addr1 = new Address("XI krajiske divizije", "Belgrade", "Serbia", "11000");
            Address addr2 = new Address("Pere Velimirovica", "Uzice", "Serbia", "11000");
            Address addr3 = new Address("Rige od Fere", "Novi Sad", "Serbia", "11000");
            cache.Insert("addr1", addr1);
            cache.Insert("addr2", addr2);
            cache.Insert("addr3", addr3);

            Assert.IsTrue(cache.Count == 3);

            Object result = cache.Invoke("addr1", processor);

            Assert.IsNotNull(result);
            Assert.AreEqual(addr1.Street, result as String);

            processor = new ExtractorProcessor(new ReflectionExtractor("City"));
            IDictionary dictResult = cache.InvokeAll(cache.Keys, processor);

            Assert.AreEqual(addr1.City, dictResult["addr1"]);
            Assert.AreEqual(addr2.City, dictResult["addr2"]);
            Assert.AreEqual(addr3.City, dictResult["addr3"]);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestFilters()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            cache.Clear();
            cache.Insert("5", "5");
            cache.Insert(1, "10");
            cache.Insert("g", "15");
            cache.Insert("b", "20");
            cache.Insert("1", "105");

            IFilter[] filters = new IFilter[] {
                new EqualsFilter(IdentityExtractor.Instance, "20"),
                new LikeFilter(IdentityExtractor.Instance, "1%", '\\', true) };
            AnyFilter anyFilter = new AnyFilter(filters);

            ICollection results = cache.GetKeys(anyFilter);

            Assert.AreEqual(4, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 1));
            Assert.IsTrue(CollectionUtils.Contains(results, "g"));
            Assert.IsTrue(CollectionUtils.Contains(results, "b"));
            Assert.IsTrue(CollectionUtils.Contains(results, "1"));


            filters = new IFilter[] {
                    new EqualsFilter(IdentityExtractor.Instance, "20"),
                    new LikeFilter(IdentityExtractor.Instance, "5%", '\\', true) };
            anyFilter = new AnyFilter(filters);
            ICacheEntry[] entries = cache.GetEntries(anyFilter);
            Assert.AreEqual(2, entries.Length);

            Assert.Contains("b", new object[] {entries[0].Key, entries[1].Key});
            Assert.Contains("5", new object[] {entries[0].Key, entries[1].Key});

            Assert.Contains("20", new object[] {entries[0].Value, entries[1].Value});
            Assert.Contains("5", new object[] {entries[0].Value, entries[1].Value});

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestListeners()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            cache.Clear();

            SyncListener listen = new SyncListener();

            cache.AddCacheListener(listen, "test", false);
            Assert.IsNull(listen.CacheEvent);

            cache.Insert("t", "a");
            Assert.IsNull(listen.CacheEvent);

            cache.Insert("tes", "b");
            Assert.IsNull(listen.CacheEvent);

            cache.Insert("test", "c");
            Assert.IsNotNull(listen.CacheEvent);
            Assert.AreEqual(listen.CacheEvent.EventType, CacheEventType.Inserted);

            listen.CacheEvent = null;
            Assert.IsNull(listen.CacheEvent);
            cache["test"] = "d";
            Assert.IsNotNull(listen.CacheEvent);
            Assert.AreEqual(listen.CacheEvent.EventType, CacheEventType.Updated);

            listen.CacheEvent = null;
            Assert.IsNull(listen.CacheEvent);
            cache.Remove("test");
            Assert.IsNotNull(listen.CacheEvent);
            Assert.AreEqual(listen.CacheEvent.EventType, CacheEventType.Deleted);

            cache.RemoveCacheListener(listen, "test");
            CacheEventFilter likeFilter = new CacheEventFilter(
                new LikeFilter(IdentityExtractor.Instance, "%ic", '\\', false));

            cache.AddCacheListener(listen, likeFilter, false);

            listen.CacheEvent = null;
            Assert.IsNull(listen.CacheEvent);

            cache.Insert("key1", "Ratko");
            Assert.IsNull(listen.CacheEvent);

            cache.Insert("key2", "PerIc");
            Assert.IsNull(listen.CacheEvent);

            cache.Insert("key3", "RatkoviC");
            Assert.IsNull(listen.CacheEvent);

            cache.Insert("key4", "Perovic");
            Assert.IsNotNull(listen.CacheEvent);
            Assert.AreEqual(listen.CacheEvent.EventType, CacheEventType.Inserted);

            cache.RemoveCacheListener(listen);

            cache.Clear();
            cache.AddCacheListener(listen);

            listen.CacheEvent = null;
            Assert.IsNull(listen.CacheEvent);

            cache.Insert("key1", "Ratko");
            Assert.IsNotNull(listen.CacheEvent);
            Assert.AreEqual(listen.CacheEvent.EventType, CacheEventType.Inserted);
            cache.Insert("key1", "Ratko NEW");
            Assert.AreEqual(listen.CacheEvent.EventType, CacheEventType.Updated);

            cache.Insert("key2", "Pera");
            Assert.IsNotNull(listen.CacheEvent);
            Assert.AreEqual(listen.CacheEvent.EventType, CacheEventType.Inserted);

            cache.RemoveCacheListener(listen);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestAggregate()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            cache.Clear();


            Hashtable ht = new Hashtable();
            ht.Add("comparableMaxKey1", 100);
            ht.Add("comparableMaxKey2", 80.5);
            ht.Add("comparableMaxKey3", 19.5);
            ht.Add("comparableMaxKey4", 2);
            cache.InsertAll(ht);

            IEntryAggregator aggregator = new DoubleAverage(IdentityExtractor.Instance);
            object result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(50.5, result);

            cache.Insert("comparableKey5", null);
            result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(50.5, result);

            IFilter alwaysFilter = new AlwaysFilter();
            result = cache.Aggregate(alwaysFilter, aggregator);
            Assert.AreEqual(50.5, result);

            cache.Clear();

            ht = new Hashtable();
            ht.Add("comparableMaxKey1", 435);
            ht.Add("comparableMaxKey2", 253);
            ht.Add("comparableMaxKey3", 3);
            ht.Add("comparableMaxKey4", null);
            ht.Add("comparableMaxKey5", -3);
            cache.InsertAll(ht);

            aggregator = new ComparableMax(IdentityExtractor.Instance);
            object max = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(max, 435);

            max = cache.Aggregate(alwaysFilter, aggregator);
            Assert.AreEqual(max, 435);

            cache.Clear();

            ht = new Hashtable();
            ht.Add("comparableMaxKey1", 435);
            ht.Add("comparableMaxKey2", 253);
            ht.Add("comparableMaxKey3", 3);
            ht.Add("comparableMaxKey4", 3);
            ht.Add("comparableMaxKey5", 3);
            ht.Add("comparableMaxKey6", null);
            ht.Add("comparableMaxKey7", null);
            cache.InsertAll(ht);

            aggregator = new DistinctValues(IdentityExtractor.Instance);
            result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(3, ((ICollection)result).Count);
            foreach (object o in ht.Values)
            {
                Assert.IsTrue(((IList)result).Contains(o) || o == null);
            }

            IFilter lessFilter = new LessFilter(IdentityExtractor.Instance, 100);
            result = cache.Aggregate(lessFilter, aggregator);
            Assert.AreEqual(1, ((ICollection)result).Count);

            cache.Clear();

            ht = new Hashtable();
            ht.Add("key1", 100);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2);

            cache.InsertAll(ht);

            aggregator = new DoubleSum(IdentityExtractor.Instance);
            result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(202, result);

            IFilter filter = new AlwaysFilter();
            result = cache.Aggregate(filter, aggregator);
            Assert.AreEqual(202, result);


            cache.Clear();

            ht = new Hashtable();
            ht.Add("key1", 100);
            ht.Add("key2", 80);
            ht.Add("key3", 19);
            ht.Add("key4", 2);
            ht.Add("key5", null);

            cache.InsertAll(ht);

            aggregator = new LongSum(IdentityExtractor.Instance);
            result = cache.Aggregate(cache.Keys, aggregator);
            Assert.AreEqual(201, result);

            IFilter greaterFilter = new GreaterFilter(IdentityExtractor.Instance, 1);
            result = cache.Aggregate(greaterFilter, aggregator);
            Assert.AreEqual(201, result);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestClearAndCount()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            cache.Clear();
            Assert.AreEqual(0, cache.Count);

            cache.Insert("key1", "value1");
            Assert.AreEqual(1, cache.Count);

            cache.Insert("key2", "value2");
            Assert.AreEqual(2, cache.Count);

            cache.Insert("key3", "value3");
            cache.Insert("key4", "value4");
            Assert.AreEqual(4, cache.Count);

            cache.Clear();
            Assert.AreEqual(0, cache.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestGetValues()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key1", 100.0);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2.0);

            cache.InsertAll(ht);
            Assert.AreEqual(4, cache.Count);

            IFilter lessFilter = new LessFilter(IdentityExtractor.Instance, 80.5);
            object[] result = cache.GetValues(lessFilter);
            Assert.AreEqual(2, result.Length);
            foreach (object o in result)
            {
                Assert.IsTrue((Convert.ToDouble(o) == 19.5) || (Convert.ToDouble(o) == 2.0));
            }

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestGetEnumerator()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key1", 100.0);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2.0);

            cache.InsertAll(ht);

            foreach (ICacheEntry entry in cache)
            {
                Assert.IsTrue(CollectionUtils.Contains(new ArrayList(ht.Values), entry.Value));
            }

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestPrunization()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-custom-highunits");

            cache.Clear();
            Hashtable ht = new Hashtable();
            ht.Add("key1", 100.0);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2.0);
            ht.Add("key5", 2.0);
            ht.Add("key6", 2.0);
            ht.Add("key7", 2.0);
            ht.Add("key8", 2.0);
            ht.Add("key9", 2.0);

            cache.InsertAll(ht);

            Assert.AreEqual(9, cache.Count);

            object result = cache["key8"];
            result = cache["key7"];
            result = cache["key8"];
            result = cache["key8"];

            ht.Clear();
            ht.Add("key10", 2.0);
            ht.Add("key11", 2.0);
            cache.InsertAll(ht);

            Assert.AreEqual(3, cache.Count);

            cache.Clear();
            ht.Clear();
            Assert.AreEqual(0, cache.Count);

            LocalCache localCache = new LocalCache(10);
            localCache.EvictionType = LocalCache.EvictionPolicyType.Hybrid;
            localCache.LowUnits = 4;

            ht.Add("key1", 100.0);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2.0);
            ht.Add("key5", 2.0);
            ht.Add("key6", 2.0);
            ht.Add("key7", 2.0);
            ht.Add("key8", 2.0);
            ht.Add("key9", 2.0);

            localCache.InsertAll(ht);

            Assert.AreEqual(ht.Count, localCache.Count);

            result = cache["key8"];
            result = cache["key7"];
            result = cache["key8"];
            result = cache["key8"];

            ht.Clear();
            ht.Add("key10", 2.0);
            ht.Add("key11", 2.0);
            localCache.InsertAll(ht);

            Assert.AreEqual(localCache.LowUnits, localCache.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestExpiry()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-with-init");

            cache.Clear();
            Hashtable ht = new Hashtable();
            ht.Add("key1", 100.0);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2.0);

            cache.InsertAll(ht);
            lock(this)
            {
                Blocking.Wait(this, 50);
            }

            foreach (ICacheEntry entry in cache)
            {
                Assert.IsTrue(entry is LocalCache.Entry);
                Assert.IsTrue(((LocalCache.Entry) entry).IsExpired);
            }


            cache = CacheFactory.GetCache("local-custom-impl-with-init");

            cache.Clear();
            ht = new Hashtable();
            ht.Add("key1", 100.0);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2.0);

            cache.InsertAll(ht);
            lock (this)
            {
                Blocking.Wait(this, 50);
            }

            foreach (ICacheEntry entry in cache)
            {
                Assert.IsTrue(entry is LocalCache.Entry);
                Assert.IsFalse(((LocalCache.Entry)entry).IsExpired);
            }

            LocalNamedCache localCache = new LocalNamedCache();
            localCache.LocalCache.ExpiryDelay = 50;

            localCache.Clear();

            localCache.Insert("key1", 100.0, 2100);
            localCache.Insert("key2", 801.5, 2100);
            localCache.Insert("key3", 40.1, 2100);
            
            lock (this)
            {
                Blocking.Wait(this, 2300);
            }

            foreach (ICacheEntry entry in localCache)
            {
                Assert.IsTrue(entry is LocalCache.Entry);
                Assert.IsTrue(((LocalCache.Entry)entry).IsExpired);
            }
            
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestEntryTouch()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");

            cache.Clear();
            Hashtable ht = new Hashtable();
            ht.Add("key1", 100.0);
            ht.Add("key2", 80.5);
            ht.Add("key3", 19.5);
            ht.Add("key4", 2.0);

            cache.InsertAll(ht);

            object result = cache["key2"];
            result = cache["key2"];
            result = cache["key2"];
            result = cache["key1"];

            ICacheEntry[] results = cache.GetEntries(new EqualsFilter(new KeyExtractor(IdentityExtractor.Instance), "key2"));
// TODO : we don't get back the actual entry!!!
//            Assert.AreEqual(3, (results[0] as LocalCache.Entry).TouchCount);

            results = cache.GetEntries(new EqualsFilter(new KeyExtractor(IdentityExtractor.Instance), "key1"));
//            Assert.AreEqual(1, (results[0] as LocalCache.Entry).TouchCount);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestInsertWithMillis()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            Assert.IsTrue(cache is LocalNamedCache);

            LocalNamedCache localNamedCache = cache as LocalNamedCache;
            Assert.IsNotNull(localNamedCache);
            localNamedCache.Insert("key1", new LocalCache.Entry(localNamedCache.LocalCache, "key1", "value1"), 300);

            Assert.IsNotNull(localNamedCache["key1"]);

            lock(this)
            {
                Blocking.Wait(this, 400);
            }

            Assert.IsNull(localNamedCache["key1"]);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestEntries()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            Assert.IsTrue(cache is LocalNamedCache);

            LocalNamedCache localNamedCache = cache as LocalNamedCache;
            Assert.IsNotNull(localNamedCache);
            Hashtable ht = new Hashtable();
            ht.Add("key1", 435);
            ht.Add("key2", 253);
            ht.Add("key3", 3);
            ht.Add("key4", 200);
            localNamedCache.InsertAll(ht);

            ICollection result = localNamedCache.Entries;

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);

            result = localNamedCache.Values;
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);

            int[] results = new int[localNamedCache.Count];
            localNamedCache.CopyTo(results, 0);
            Assert.AreEqual(localNamedCache.Count, result.Count);


            CacheFactory.Shutdown();
        }

        [Test]
        public void TestLocking()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            Assert.IsTrue(cache is LocalNamedCache);

            LocalNamedCache localNamedCache = cache as LocalNamedCache;
            Assert.IsNotNull(localNamedCache);
            Hashtable ht = new Hashtable();
            ht.Add("key1", 435);
            ht.Add("key2", 253);
            ht.Add("key3", 3);
            ht.Add("key4", 200);
            localNamedCache.InsertAll(ht);

            localNamedCache.Lock("key2");
            localNamedCache.Lock("key2", 1000);

            localNamedCache.Unlock("key2");

            GreaterFilter filter = new GreaterFilter(IdentityExtractor.Instance, 280);
            object[] results = localNamedCache.GetValues(filter, null);

            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(435, results[0]);

            results = localNamedCache.GetEntries(filter, null);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(435, ((ICacheEntry)results[0]).Value);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestIndexes()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");
            Assert.IsTrue(cache is LocalNamedCache);

            LocalNamedCache localNamedCache = cache as LocalNamedCache;
            Assert.IsNotNull(localNamedCache);
            localNamedCache.AddIndex(IdentityExtractor.Instance, true, null);
            localNamedCache.RemoveIndex(IdentityExtractor.Instance);

            localNamedCache.AddIndex(IdentityExtractor.Instance, true, SafeComparer.Instance);
            localNamedCache.RemoveIndex(IdentityExtractor.Instance);

            localNamedCache.AddIndex(IdentityExtractor.Instance, false, null);
            localNamedCache.RemoveIndex(IdentityExtractor.Instance);

            localNamedCache.AddIndex(IdentityExtractor.Instance, false, SafeComparer.Instance);
            localNamedCache.RemoveIndex(IdentityExtractor.Instance);
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestLocalNamedCacheDispose()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;
            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;
            INamedCache cache;
            string[] keys = { "key1", "key2", "key3", "key4" };
            string[] values = { "value1", "value2", "value3", "value4" };
            using (cache = CacheFactory.GetCache("local-default"))
            {
                cache.Clear();
                IDictionary h = new Hashtable();
                h.Add(keys[0], values[0]);
                h.Add(keys[1], values[1]);
                h.Add(keys[2], values[2]);
                h.Add(keys[3], values[3]);
                cache.InsertAll(h);

                foreach (object key in cache.Keys)
                {
                    Assert.IsTrue(cache.Contains(key));
                }
            }
            //after disposal
            Assert.IsFalse(cache.IsActive);
            LocalNamedCache lnc = cache as LocalNamedCache;
            Assert.IsNotNull(lnc);
            Assert.IsTrue(lnc.IsReleased);
            Assert.IsNull(lnc.CacheService);
            
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestCustomEvictionPolicy()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-custom-eviction");
            Assert.IsTrue(cache is LocalNamedCache);

            LocalNamedCache localNamedCache = cache as LocalNamedCache;
            Assert.IsNotNull(localNamedCache);

            LocalCache localCache = localNamedCache.LocalCache;
            Assert.IsNotNull(localCache);

            Assert.AreEqual(localCache.EvictionType, LocalCache.EvictionPolicyType.External);
            Assert.IsInstanceOf(typeof(TestEvictionPolicy),localCache.EvictionPolicy);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestCustomUnitCalculator()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-custom-unit-calculator");
            Assert.IsTrue(cache is LocalNamedCache);

            LocalNamedCache localNamedCache = cache as LocalNamedCache;
            Assert.IsNotNull(localNamedCache);

            LocalCache localCache = localNamedCache.LocalCache;

            Assert.IsNotNull(localCache);
            Assert.IsNotNull(localCache.UnitCalculator);

            localCache.Insert("key1", "value1");
            localCache.Insert("key2", "value2");
            LocalCache.Entry entry = localCache.GetEntry("key1");

            //evaluates total units in the cache and each entry's units
            Assert.AreEqual(localCache.CalculatorType, LocalCache.UnitCalculatorType.External);
            Assert.IsInstanceOf(typeof(TestUnitCalculator), localCache.UnitCalculator);
            Assert.AreEqual(localCache.Units, 4);
            Assert.AreEqual(entry.Units, 2);          
            
            CacheFactory.Shutdown();
        }

    }

    class Listener : ICacheListener
    {
        private CacheEventArgs m_cacheEvent;

        public Listener()
        {
            m_cacheEvent = null;
        }

        public CacheEventArgs CacheEvent
        {
            get
            {
                lock (this)
                {
                    return m_cacheEvent;
                }
            }
            set
            {
                lock (this)
                {
                    m_cacheEvent = value;
                }
            }
        }

        public void EntryInserted(CacheEventArgs evt)
        {
            lock (this)
            {
                CacheEvent = evt; 
                Monitor.Pulse(this);
            }
        }

        public void EntryUpdated(CacheEventArgs evt)
        {
            lock (this)
            {
                CacheEvent = evt;
                Monitor.Pulse(this);
            }
        }

        public void EntryDeleted(CacheEventArgs evt)
        {
            lock (this)
            {
                CacheEvent = evt;
                Monitor.Pulse(this);
            }
        }

        public void waitForEvent(int millis)
        {
            lock (this)
            {
                if (CacheEvent == null)
                {
                    Blocking.Wait(this, millis);
                }
            }
        }
    }

    class SyncListener : Listener, CacheListenerSupport.ISynchronousListener
    {
    }
}