/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using NUnit.Framework;
using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Internal.Util.Processor;
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;

namespace Tangosol.Util.Processor
{
    [TestFixture]
    public class ProcessorTests
    {
        NameValueCollection appSettings = TestUtils.AppSettings;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
        }

        [Test]
        public void TestConditionalPut()
        {
            ConditionalPut conditionalPut = new ConditionalPut(AlwaysFilter.Instance, 1500);
            ConditionalPut conditionalPut1 = new ConditionalPut(AlwaysFilter.Instance, 1500);
            IInvocableCacheEntry entry = new LocalCache.Entry(new LocalCache(), "key1", 1200);
            Object result = conditionalPut.Process(entry);
            Assert.AreEqual(1500, entry.Value);

            ConditionalPut conditionalPut2 = new ConditionalPut(new GreaterFilter(IdentityExtractor.Instance, 100), 100);
            entry = new LocalCache.Entry(new LocalCache(), "key1", 1200);
            result = conditionalPut2.Process(entry);
            Assert.AreEqual(100, entry.Value);

            entry = new LocalCache.Entry(new LocalCache(), "key1", 80);
            result = conditionalPut2.Process(entry);
            Assert.AreEqual(80, entry.Value);

            Assert.AreEqual(conditionalPut, conditionalPut1);
            Assert.AreEqual(conditionalPut.ToString(), conditionalPut1.ToString());
            Assert.AreEqual(conditionalPut.GetHashCode(), conditionalPut1.GetHashCode());

            // testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
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
            IDictionary results = cache.InvokeAll(cache.Keys, processor);
            keys = cache.GetKeys(greaterThen600);
            Assert.AreEqual(keys.Count, 3);
 
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestConditionalPutAll()
        {
            Hashtable ht = new Hashtable();
            ht.Add("key1", 100);
            ht.Add("key2", 200);
            ht.Add("key3", 300);
            ConditionalPutAll conditionalPutAll = new ConditionalPutAll(AlwaysFilter.Instance, ht);
            ConditionalPutAll conditionalPutAll1 = new ConditionalPutAll(AlwaysFilter.Instance, ht);

            Assert.AreEqual(conditionalPutAll.ToString(), conditionalPutAll1.ToString());

            LocalCache lCache = new LocalCache();
            IInvocableCacheEntry entry = new LocalCache.Entry(lCache, "key2", 400);
            Object result = conditionalPutAll.Process(entry);
            Assert.AreEqual(200, entry.Value);

            // testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
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
            ConditionalPutAll processor = new ConditionalPutAll(AlwaysFilter.Instance, htPut);
            cache.Invoke("conditionalPutAllKey1", processor);
            Assert.IsNotNull(cache["conditionalPutAllKey1"]);
            Assert.AreEqual(cache["conditionalPutAllKey1"], htPut["conditionalPutAllKey1"]);

            // put all keys from htPut and compare cache values with put ones
            cache.InvokeAll(htPut.Keys, processor);
            Assert.IsTrue(cache.Count == 5);
            Assert.AreEqual(cache["conditionalPutAllKey1"], htPut["conditionalPutAllKey1"]);
            Assert.AreEqual(cache["conditionalPutAllKey6"], htPut["conditionalPutAllKey6"]);
            Assert.AreEqual(cache["conditionalPutAllKey3"], htPut["conditionalPutAllKey3"]);

            htPut.Clear();
            htPut.Add("conditionalPutAllKey4", 355);
            processor = new ConditionalPutAll(AlwaysFilter.Instance, htPut);

            cache.InvokeAll(new GreaterFilter(IdentityExtractor.Instance, 300), processor);
            Assert.AreEqual(cache["conditionalPutAllKey4"], htPut["conditionalPutAllKey4"]);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestConditionalRemove()
        {
            ConditionalRemove conditionalRemove = new ConditionalRemove(new GreaterFilter(IdentityExtractor.Instance, 100), true);
            ConditionalRemove conditionalRemove1 = new ConditionalRemove(new GreaterFilter(IdentityExtractor.Instance, 100), true);

            Assert.AreEqual(conditionalRemove, conditionalRemove1);
            Assert.AreEqual(conditionalRemove.ToString(), conditionalRemove1.ToString());
            Assert.AreEqual(conditionalRemove.GetHashCode(), conditionalRemove1.GetHashCode());

            IInvocableCacheEntry entry = new LocalCache.Entry(new LocalCache(), "key1", 1200);
            Object result = conditionalRemove.Process(entry);
            Assert.IsNull(entry.Value);

            entry = new LocalCache.Entry(new LocalCache(), "key1", 50);
            result = conditionalRemove.Process(entry);
            Assert.AreEqual(50, entry.Value);

            // testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("conditionalPutAllKey1", 435);
            ht.Add("conditionalPutAllKey2", 253);
            ht.Add("conditionalPutAllKey3", 200);
            ht.Add("conditionalPutAllKey4", 333);
            cache.InsertAll(ht);

            IFilter greaterThen300 = new GreaterFilter(IdentityExtractor.Instance, 300);
            IFilter lessThen300 = new LessFilter(IdentityExtractor.Instance, 300);

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

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestConditional()
        {
            IFilter lessThen250 = new LessFilter(IdentityExtractor.Instance, 250);
            ConditionalProcessor condProcessor = new ConditionalProcessor(new GreaterFilter(IdentityExtractor.Instance, 200), new ConditionalRemove(lessThen250, false));
            ConditionalProcessor condProcessor1 = new ConditionalProcessor(new GreaterFilter(IdentityExtractor.Instance, 200), new ConditionalRemove(lessThen250, false));

            Assert.AreEqual(condProcessor, condProcessor1);
            Assert.AreEqual(condProcessor.GetHashCode(), condProcessor1.GetHashCode());
            Assert.AreEqual(condProcessor.ToString(), condProcessor1.ToString());

            LocalCache.Entry entry = new LocalCache.Entry(new LocalCache(), "key1", 225);
            condProcessor.Process(entry);
            Assert.IsNull(entry.Value);

            entry = new LocalCache.Entry(new LocalCache(), "key1", 150);
            condProcessor.Process(entry);
            Assert.AreEqual(150, entry.Value);

            // testing on Remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("conditionalKey1", 200);
            ht.Add("conditionalKey2", 250);
            ht.Add("conditionalKey3", 300);
            ht.Add("conditionalKey4", 400);
            cache.InsertAll(ht);


            IFilter lessThen300 = new LessFilter(IdentityExtractor.Instance, 300);
            ConditionalProcessor processor = new ConditionalProcessor(new GreaterFilter(IdentityExtractor.Instance, 200), new ConditionalRemove(lessThen300, false));

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
            Address addr1 = new Address("XI krajiske divizije", "Belgrade", "Serbia", "11000");
            Address addr2 = new Address("Pere Velimirovica", "Belgrade", "Serbia", "11000");
            Address addr3 = new Address("Rige od Fere", "Belgrade", "Serbia", "11000");
            Address addrNEW = new Address("NEW Pere", "NEW Belgrade", "Serbia", "11000");

            IEntryProcessor putLikeXI = new ConditionalPut(new LikeFilter(new ReflectionExtractor("Street"), "XI%", '\\', true), addr2);
            IEntryProcessor putLikeRig = new ConditionalPut(new LikeFilter(new ReflectionExtractor("Street"), "Rige%", '\\', true), addrNEW);
            IEntryProcessor[] processors = new IEntryProcessor[] { putLikeXI, putLikeRig };
            CompositeProcessor processor = new CompositeProcessor(processors);
            CompositeProcessor processor1 = new CompositeProcessor(processors);

            Assert.AreEqual(processor, processor1);
            Assert.AreEqual(processor.GetHashCode(), processor1.GetHashCode());
            Assert.AreEqual(processor.ToString(), processor1.ToString());

            LocalCache.Entry entry = new LocalCache.Entry(new LocalCache(), "addr1", addr1);
            processor.Process(entry);
            Assert.AreEqual(addr2, entry.Value);

            entry = new LocalCache.Entry(new LocalCache(), "addr2", addr2);
            processor.Process(entry);
            Assert.AreEqual(addr2, entry.Value);

            entry = new LocalCache.Entry(new LocalCache(), "addr3", addr3);
            processor.Process(entry);
            Assert.AreEqual(addrNEW, entry.Value);

            // testing on Remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            cache.Insert("addr1", addr1);
            cache.Insert("addr2", addr2);

            Assert.IsTrue(cache.Count == 2);

            LikeFilter likeXI = new LikeFilter(new ReflectionExtractor("getStreet"), "XI%", '\\', true);
            ExtractorProcessor extractStreet = new ExtractorProcessor(new ReflectionExtractor("getStreet"));
            IEntryProcessor putAddr3 = new ConditionalPut(AlwaysFilter.Instance, addr3);
            IEntryProcessor removeLikeXI = new ConditionalRemove(likeXI, false);
            processors = new IEntryProcessor[] { extractStreet, removeLikeXI, putAddr3 };
            processor = new CompositeProcessor(processors);

            Object objResult = cache.Invoke("addr1", processor);

            Assert.IsTrue(cache.Count == 2);
            Assert.AreEqual(addr1.Street, (objResult as Object[])[0]);

            Address res = cache["addr1"] as Address;
            Assert.AreEqual(addr3.City, res.City);
            Assert.AreEqual(addr3.State, res.State);
            Assert.AreEqual(addr3.Street, res.Street);
            Assert.AreEqual(addr3.ZIP, res.ZIP);

            res = cache["addr2"] as Address;
            Assert.AreEqual(addr2.City, res.City);
            Assert.AreEqual(addr2.State, res.State);
            Assert.AreEqual(addr2.Street, res.Street);
            Assert.AreEqual(addr2.ZIP, res.ZIP);

            IDictionary dictResult = cache.InvokeAll(new ArrayList(new object[] { "addr1", "addr2" }), processor);

            Assert.IsTrue(cache.Count == 2);
            Assert.AreEqual(addr3.Street, (cache["addr1"] as Address).Street);
            Assert.AreEqual(addr3.Street, (cache["addr2"] as Address).Street);
            Assert.AreEqual((dictResult["addr1"] as object[])[0], addr3.Street);
            Assert.AreEqual((dictResult["addr2"] as object[])[0], addr2.Street);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestExtractor()
        {
            Address addr = new Address("Champs-Elysees", "Paris", "France", "1616");
            ExtractorProcessor extractorProcessor = new ExtractorProcessor(new ReflectionExtractor("Street"));
            ExtractorProcessor extractorProcessor1 = new ExtractorProcessor(new ReflectionExtractor("Street"));

            Assert.AreEqual(extractorProcessor, extractorProcessor1);
            Assert.AreEqual(extractorProcessor.GetHashCode(), extractorProcessor1.GetHashCode());
            Assert.AreEqual(extractorProcessor.ToString(), extractorProcessor1.ToString());

            LocalCache.Entry entry = new LocalCache.Entry(new LocalCache(), "addr1", addr);
            Object result = extractorProcessor.Process(entry);
            Assert.AreEqual("Champs-Elysees", result);

            extractorProcessor = new ExtractorProcessor(new KeyExtractor(IdentityExtractor.Instance));
            result = extractorProcessor.Process(entry);
            Assert.AreEqual("addr1", result);

            // testing on Remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            ExtractorProcessor processor = new ExtractorProcessor(new ReflectionExtractor("getStreet"));
            Address addr1 = new Address("XI krajiske divizije", "Belgrade", "Serbia", "11000");
            Address addr2 = new Address("Pere Velimirovica", "Uzice", "Serbia", "11000");
            Address addr3 = new Address("Rige od Fere", "Novi Sad", "Serbia", "11000");
            cache.Insert("addr1", addr1);
            cache.Insert("addr2", addr2);
            cache.Insert("addr3", addr3);

            Assert.IsTrue(cache.Count == 3);

            result = cache.Invoke("addr1", processor);

            Assert.IsNotNull(result);
            Assert.AreEqual(addr1.Street, result as String);

            processor = new ExtractorProcessor(new ReflectionExtractor("getCity"));
            IDictionary dictResult = cache.InvokeAll(cache.Keys, processor);

            Assert.AreEqual(addr1.City, dictResult["addr1"]);
            Assert.AreEqual(addr2.City, dictResult["addr2"]);
            Assert.AreEqual(addr3.City, dictResult["addr3"]);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestVersionPut()
        {
            Temperature temperature = new Temperature(35, 'f', 11);
            Temperature temperatureNew = new Temperature(15, 'C', 10);
            LocalCache.Entry entry = new LocalCache.Entry(new LocalCache(), "morning", temperature);
            IEntryProcessor processorVersioned = new VersionedPut(temperatureNew, true, true);
            IEntryProcessor processorVersioned1 = new VersionedPut(temperatureNew, true, true);

            Assert.AreEqual(processorVersioned, processorVersioned1);
            Assert.AreEqual(processorVersioned.GetHashCode(), processorVersioned1.GetHashCode());
            Assert.AreEqual(processorVersioned.ToString(), processorVersioned1.ToString());

            Temperature res = (Temperature) processorVersioned.Process(entry);
            Assert.AreEqual(temperature, res);

            temperatureNew = new Temperature(15, 'C', 11);
            processorVersioned = new VersionedPut(temperatureNew, true, true);
            processorVersioned.Process(entry);
            Assert.AreEqual((entry.Value as Temperature).Grade, temperatureNew.Grade);
            Assert.AreEqual((entry.Value as Temperature).Value, temperatureNew.Value);

            // testing on Remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Temperature t1 = new Temperature(3, 'c', 8);
            Temperature t2 = new Temperature(24, 'c', 12);
            Temperature t3 = new Temperature(15, 'c', 18);
            Temperature t4 = new Temperature(0, 'c', 0);

            cache.Insert("morning", t1);
            cache.Insert("noon", t2);
            cache.Insert("afternoon", t3);
            cache.Insert("midnight", t4);


            Temperature t = new Temperature(35, 'f', 11);
            IEntryProcessor processor = new VersionedPut(t, true, true);
            Temperature result = (Temperature) cache.Invoke("morning", processor);
            Assert.AreNotEqual(t.Value, result.Value);
            Assert.AreNotEqual(t.Grade, result.Grade);
            Temperature after = (Temperature) cache["morning"];
            Assert.AreNotEqual(t.Value, after.Value);
            Assert.AreNotEqual(t.Version, after.Version);
            Assert.AreNotEqual(t.Grade, after.Grade);

            t = new Temperature(35, 'f', 8);
            processor = new VersionedPut(t, true, true);
            result = (Temperature) cache.Invoke("morning", processor);
            Assert.IsNull(result);
            after = (Temperature) cache["morning"];
            Assert.AreEqual(t.Version + 1, after.Version);
            Assert.AreEqual(t.Value, after.Value);
            Assert.AreEqual(t.Grade, after.Grade);

            t = new Temperature(38, 'f', 10);
            processor = new VersionedPut(t, true, true);
            result = (Temperature) cache.Invoke("sometime", processor);
            after = (Temperature) cache["sometime"];

            Assert.AreEqual(t.Value, after.Value);
            Assert.AreEqual(t.Grade, after.Grade);
            Assert.AreEqual(t.Version + 1, after.Version);

            cache.Remove("sometime");

            processor = new VersionedPut(t, false, true);
            result = (Temperature) cache.Invoke("sometime", processor);
            after = (Temperature) cache["sometime"];
            Assert.IsNull(after);

            CacheFactory.Shutdown();

        }

        [Test]
        public void TestVersionPutAll()
        {
            Hashtable ht = new Hashtable();
            ht.Add("noon", new Temperature(100, 'f', 12));
            ht.Add("midnight", new Temperature(25, 'f', 0));
            IEntryProcessor processor = new VersionedPutAll(ht, true, false);
            IEntryProcessor processor1 = new VersionedPutAll(ht, true, false);

            Assert.AreEqual(processor.ToString(), processor1.ToString());

            Temperature temperature = new Temperature(500, 'C', 1);
            LocalCache.Entry entry = new LocalCache.Entry(new LocalCache(), "morning", temperature);

            processor.Process(entry);

            Temperature entryTemp = entry.Value as Temperature;
            Assert.AreEqual(entryTemp.Grade, temperature.Grade);
            Assert.AreEqual(entryTemp.Value, temperature.Value);

            temperature = new Temperature(500, 'C', 12);
            entry = new LocalCache.Entry(new LocalCache(), "noon", temperature);

            processor.Process(entry);

            entryTemp = entry.Value as Temperature;
            Assert.AreEqual(entryTemp.Grade, 'F');
            Assert.AreEqual(entryTemp.Value, 100);

            // testing on Remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Temperature t1 = new Temperature(3, 'c', 8);
            Temperature t2 = new Temperature(24, 'c', 12);
            Temperature t3 = new Temperature(15, 'c', 18);
            Temperature t4 = new Temperature(0, 'c', 0);

            cache.Insert("morning", t1);
            cache.Insert("noon", t2);
            cache.Insert("afternoon", t3);
            cache.Insert("midnight", t4);

            ht = new Hashtable();
            ht.Add("noon", new Temperature(100, 'f', 12));
            ht.Add("midnight", new Temperature(25, 'f', 0));

            processor = new VersionedPutAll(ht, true, false);
            cache.InvokeAll(cache.Keys, processor);

            Temperature after = (Temperature)cache["midnight"];
            Assert.AreEqual(25, after.Value);
            Assert.AreEqual('F', after.Grade);
            Assert.AreEqual(1, after.Version);

            after = (Temperature)cache["noon"];
            Assert.AreEqual(100, after.Value);
            Assert.AreEqual('F', after.Grade);
            Assert.AreEqual(13, after.Version);

            CacheFactory.Shutdown();

        }

        [Test]
        public void TestNumberIncrementor()
        {
            Temperature belgrade = new Temperature(25, 'c', 1);
            int bgdBefore = belgrade.Value;
            PropertyManipulator valueManipulator = new PropertyManipulator("Value");
            IEntryProcessor processor = new NumberIncrementor(valueManipulator, 1, true);
            IEntryProcessor processor1 = new NumberIncrementor(valueManipulator, 1, true);

            Assert.AreEqual(processor, processor1);
            Assert.AreEqual(processor.GetHashCode(), processor1.GetHashCode());
            Assert.AreEqual(processor.ToString(), processor1.ToString());

            LocalCache.Entry entry = new LocalCache.Entry(new LocalCache(), "belgrade", belgrade);
            processor.Process(entry);
            Assert.AreEqual(bgdBefore+1, ((Temperature)entry.Value).Value);
            processor.Process(entry);
            Assert.AreEqual(bgdBefore+2, ((Temperature)entry.Value).Value);

            // testing on Remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Temperature bgd = new Temperature(25, 'c', 12);
            Temperature nyc = new Temperature(99, 'f', 12);
            cache.Insert("BGD", bgd);
            cache.Insert("NYC", nyc);

            PropertyManipulator manipulator = new PropertyManipulator("Value");
            processor = new NumberIncrementor(manipulator, 1, true);
            object before = cache.Invoke("BGD", processor);
            Assert.AreEqual(bgd.Value, before);

            Temperature after = (Temperature) cache["BGD"];
            Assert.AreEqual(((int) before) + 1, after.Value);

            processor = new NumberIncrementor(manipulator, -19, false);
            object newNYC = cache.Invoke("NYC", processor);
            Assert.AreEqual(nyc.Value - 19, newNYC);

            Score score = new Score(1, 1, 1, 1, 1, 1, 1, new RawInt128(new byte[] {0}), 1 );
            LocalCache.Entry scoreEntry = new LocalCache.Entry(new LocalCache(), "score", score);
            valueManipulator = new PropertyManipulator("RawInt128Value");
            processor = new NumberIncrementor(valueManipulator, 1, true);
            processor.Process(scoreEntry);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestUnitNumberIncrementor()
        {
            Score score = new Score(1, 1, 1, 1L, 1, 1, new decimal(1), new RawInt128(new byte[] {1}), 1);
            Score scoreOrig = new Score(1, 1, 1, 1L, 1, 1, new decimal(1), new RawInt128(new byte[] {1}), 1);

            PropertyManipulator byteManipulator = new PropertyManipulator("ByteValue");
            PropertyManipulator shortManipulator = new PropertyManipulator("ShortValue");
            PropertyManipulator intManipulator = new PropertyManipulator("IntValue");
            PropertyManipulator longManipulator = new PropertyManipulator("LongValue");
            PropertyManipulator floatManipulator = new PropertyManipulator("FloatValue");
            PropertyManipulator doubleManipulator = new PropertyManipulator("DoubleValue");
            PropertyManipulator decimalManipulator = new PropertyManipulator("DecimalValue");
            PropertyManipulator int128Manipulator = new PropertyManipulator("RawInt128Value");

            NumberIncrementor processorByte = new NumberIncrementor(byteManipulator, 2, true);
            NumberIncrementor processorByte2 = new NumberIncrementor("ByteValue", 2, false);
            Assert.IsTrue(processorByte.Equals(processorByte2));
            NumberIncrementor processorShort = new NumberIncrementor(shortManipulator, 2, true);
            NumberIncrementor processorInt = new NumberIncrementor(intManipulator, 2, true);
            NumberIncrementor processorLong = new NumberIncrementor(longManipulator, 2, true);
            NumberIncrementor processorFloat = new NumberIncrementor(floatManipulator, 2, true);
            NumberIncrementor processorDouble = new NumberIncrementor(doubleManipulator, 2, true);
            NumberIncrementor processorDecimal = new NumberIncrementor(decimalManipulator, new decimal(2), true);

            Exception e = null;
            NumberIncrementor processorInt128 = null;
            try
            {
                processorInt128 = new NumberIncrementor(int128Manipulator, new RawInt128(new byte[] {6}), true);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNull(processorInt128);
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof (ArgumentException), e);

            LocalCache.Entry entry = new LocalCache.Entry(new LocalCache(), "score", score);
            object result1 = processorByte.Process(entry);
            Assert.IsNotNull(result1);
            Assert.IsInstanceOf(typeof (byte), result1);
            Assert.AreEqual((byte) result1, 1);
            result1 = processorByte2.Process(entry);
            Assert.IsNotNull(result1);
            Assert.IsInstanceOf(typeof(byte), result1);
            Assert.AreEqual((byte)result1, 5);
            
            processorShort.Process(entry);
            processorInt.Process(entry);
            processorLong.Process(entry);
            processorFloat.Process(entry);
            processorDouble.Process(entry);
            processorDecimal.Process(entry);

            e = null;
            try
            {
                processorInt128 = new NumberIncrementor("v", null, false);
                Assert.IsNotNull(processorInt128);
                processorInt128.Process(entry);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof (ArgumentNullException), e);

            processorInt128 = new NumberIncrementor(int128Manipulator, 5, false);
            processorInt128.Process(entry);
            
            Assert.AreEqual(scoreOrig.ByteValue + 4, ((Score) entry.Value).ByteValue);
            Assert.AreEqual(scoreOrig.ShortValue + 2, ((Score) entry.Value).ShortValue);
            Assert.AreEqual(scoreOrig.IntValue + 2, ((Score) entry.Value).IntValue);
            Assert.AreEqual(scoreOrig.LongValue + 2, ((Score) entry.Value).LongValue);
            Assert.AreEqual(scoreOrig.FloatValue + 2, ((Score) entry.Value).FloatValue);
            Assert.AreEqual(scoreOrig.DoubleValue + 2, ((Score) entry.Value).DoubleValue);
            Assert.AreEqual(scoreOrig.RawInt128Value.ToDecimal() + 5, ((Score) entry.Value).RawInt128Value.ToDecimal());
            Assert.AreEqual(Decimal.Add(scoreOrig.DecimalValue, new Decimal(2)), ((Score) entry.Value).DecimalValue);
        }

        [Test]
        public void TestNumberMultiplier()
        {
            Temperature belgrade = new Temperature(25, 'c', 1);
            int bgdBefore = belgrade.Value;
            PropertyManipulator valueManipulator = new PropertyManipulator("Value");
            IEntryProcessor processor = new NumberMultiplier(valueManipulator, 2, true);
            IEntryProcessor processor1 = new NumberMultiplier(valueManipulator, 2, true);

            Assert.AreEqual(processor, processor1);
            Assert.AreEqual(processor.GetHashCode(), processor1.GetHashCode());
            Assert.AreEqual(processor.ToString(), processor1.ToString());

            LocalCache.Entry entry = new LocalCache.Entry(new LocalCache(), "belgrade", belgrade);
            processor.Process(entry);
            Assert.AreEqual(bgdBefore * 2, ((Temperature)entry.Value).Value);

            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Temperature bgd = new Temperature(25, 'c', 12);
            Temperature nyc = new Temperature(99, 'f', 12);
            cache.Insert("BGD", bgd);
            cache.Insert("NYC", nyc);

            PropertyManipulator manipulator = new PropertyManipulator("Value");
            processor = new NumberMultiplier(manipulator, 2, false);
            object newTemp = cache.Invoke("BGD", processor);
            Assert.AreEqual(bgd.Value * 2, newTemp);

            Temperature newBGD = (Temperature) cache["BGD"];
            Assert.AreEqual(bgd.Value * 2, newBGD.Value);

            processor = new NumberMultiplier(manipulator, 0.5, false);
            object newNYC = cache.Invoke("NYC", processor);
            Assert.AreEqual(49, newNYC);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestUnitNumberMultiplier()
        {
            Score score = new Score(3, 3, 3, 3L, 3, 3, new decimal(3), new RawInt128(new byte[] {3}), 3);
            Score scoreOrig = new Score(3, 3, 3, 3L, 3, 3, new decimal(3), new RawInt128(new byte[] { 3 }), 3);

            PropertyManipulator byteManipulator = new PropertyManipulator("ByteValue");
            PropertyManipulator shortManipulator = new PropertyManipulator("ShortValue");
            PropertyManipulator intManipulator = new PropertyManipulator("IntValue");
            PropertyManipulator longManipulator = new PropertyManipulator("LongValue");
            PropertyManipulator floatManipulator = new PropertyManipulator("FloatValue");
            PropertyManipulator doubleManipulator = new PropertyManipulator("DoubleValue");
            PropertyManipulator decimalManipulator = new PropertyManipulator("DecimalValue");
            PropertyManipulator int128Manipulator = new PropertyManipulator("RawInt128Value");

            NumberMultiplier processorByte = new NumberMultiplier(byteManipulator, 2, true);
            NumberMultiplier processorByte2 = new NumberMultiplier("ByteValue", 2, false);
            Assert.IsTrue(processorByte.Equals(processorByte2));
            NumberMultiplier processorShort = new NumberMultiplier(shortManipulator, 2, true);
            NumberMultiplier processorInt = new NumberMultiplier(intManipulator, 2, true);
            NumberMultiplier processorLong = new NumberMultiplier(longManipulator, 2, true);
            NumberMultiplier processorFloat = new NumberMultiplier(floatManipulator, 2, true);
            NumberMultiplier processorDouble = new NumberMultiplier(doubleManipulator, 2, true);
            NumberMultiplier processorDecimal = new NumberMultiplier(decimalManipulator, new decimal(2), true);

            Exception e = null;
            NumberMultiplier processorInt128 = null;
            try
            {
                processorInt128 = new NumberMultiplier(int128Manipulator, new RawInt128(new byte[] {6}), true);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNull(processorInt128);
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);

            processorInt128 = new NumberMultiplier(int128Manipulator, 2, true);

            LocalCache.Entry entry = new LocalCache.Entry(new LocalCache(), "score", score);
            object result1 = processorByte.Process(entry);
            Assert.IsNotNull(result1);
            Assert.IsInstanceOf(typeof(byte), result1);
            Assert.AreEqual((byte) result1, 3);
            result1 = processorByte2.Process(entry);
            Assert.IsNotNull(result1);
            Assert.IsInstanceOf(typeof(byte), result1);
            Assert.AreEqual((byte) result1, 12);

            processorShort.Process(entry);
            processorInt.Process(entry);
            processorLong.Process(entry);
            processorFloat.Process(entry);
            processorDouble.Process(entry);
            processorDecimal.Process(entry);
            processorInt128.Process(entry);

            Assert.AreEqual(scoreOrig.ByteValue * 4, ((Score) entry.Value).ByteValue);
            Assert.AreEqual(scoreOrig.ShortValue * 2, ((Score) entry.Value).ShortValue);
            Assert.AreEqual(scoreOrig.IntValue * 2, ((Score) entry.Value).IntValue);
            Assert.AreEqual(scoreOrig.LongValue * 2, ((Score) entry.Value).LongValue);
            Assert.AreEqual(scoreOrig.FloatValue * 2, ((Score) entry.Value).FloatValue);
            Assert.AreEqual(scoreOrig.DoubleValue * 2, ((Score) entry.Value).DoubleValue);
            Assert.AreEqual(scoreOrig.RawInt128Value.ToDecimal() * 2, ((Score) entry.Value).RawInt128Value.ToDecimal());
            Assert.AreEqual(Decimal.Multiply(scoreOrig.DecimalValue, new Decimal(2)), ((Score) entry.Value).DecimalValue);

            processorShort = new NumberMultiplier(shortManipulator, 2.5, true);
            processorLong = new NumberMultiplier(longManipulator, 2.5, true);
            processorByte = new NumberMultiplier(byteManipulator, 2.5, true);
            processorShort.Process(entry);
            processorLong.Process(entry);
            processorByte.Process(entry);
            Assert.AreEqual(scoreOrig.ByteValue * 10, ((Score) entry.Value).ByteValue);
            Assert.AreEqual(scoreOrig.ShortValue * 5, ((Score) entry.Value).ShortValue);
            Assert.AreEqual(scoreOrig.LongValue * 5, ((Score) entry.Value).LongValue);
        }

        [Test]
        public void TestNumberMultiplierWithException()
        {
            Assert.That(() => new NumberMultiplier("Value", "badnumber", true), Throws.ArgumentException);
        }

        [Test]
        public void TestNumberMultiplierWithException1()
        {
            PropertyManipulator valueManipulator = new PropertyManipulator("Value");
            Assert.That(() => new NumberMultiplier(valueManipulator, "badnumber", true), Throws.ArgumentException);
        }

        [Test]
        public void TestUpdaterProcessor()
        {
            Temperature belgrade = new Temperature(25, 'c', 1);
            IValueUpdater updater = new ReflectionUpdater("Value");
            IEntryProcessor processor = new UpdaterProcessor(updater, 0);
            IEntryProcessor processor1 = new UpdaterProcessor(updater, 0);

            Assert.AreEqual(processor, processor1);
            Assert.AreEqual(processor.GetHashCode(), processor1.GetHashCode());
            Assert.AreEqual(processor.ToString(), processor1.ToString());

            LocalCache.Entry entry = new LocalCache.Entry(new LocalCache(), "belgrade", belgrade);
            processor.Process(entry);
            Assert.AreEqual(0, ((Temperature)entry.Value).Value);

            // testing on Remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Temperature bgd = new Temperature(25, 'c', 12);
            Temperature nyc = new Temperature(99, 'f', 12);
            cache.Insert("BGD", bgd);
            cache.Insert("NYC", nyc);

            updater = new ReflectionUpdater("setValue");
            processor = new UpdaterProcessor(updater, 0);
            object newTemp = cache.Invoke("BGD", processor);

            Temperature newBGD = (Temperature) cache["BGD"];
            Assert.AreEqual(0, newBGD.Value);

            updater = new ReflectionUpdater("setValue");
            IValueUpdater compositeupdater = new CompositeUpdater(IdentityExtractor.Instance, updater);
            processor = new UpdaterProcessor(compositeupdater, 5);
            cache.Invoke("NYC", processor);
            Temperature newNYC = (Temperature) cache["NYC"];
            Assert.AreEqual(5, newNYC.Value);

            CacheFactory.Shutdown();

        }

        [Test]
        public void TestPreloadRequest()
        {
            IEntryProcessor processor = PreloadRequest.Instance;
            IEntryProcessor processor1 = PreloadRequest.Instance;

            Assert.AreEqual(processor, processor1);
            Assert.AreEqual(processor.GetHashCode(), processor1.GetHashCode());
            Assert.AreEqual(processor.ToString(), processor1.ToString());

            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Temperature bgd = new Temperature(25, 'c', 12);
            Temperature nyc = new Temperature(99, 'f', 12);
            cache.Insert("BGD", bgd);
            cache.Insert("NYC", nyc);

            object o = cache.Invoke("BGD", processor);
            Assert.IsNull(o);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestProcessorSerialization()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.IsNotNull(ctx);

            CompositeProcessor compositeProcessor = new CompositeProcessor();
            ConditionalProcessor conditionalProcessor = new ConditionalProcessor();
            ConditionalPut conditionalPut = new ConditionalPut(AlwaysFilter.Instance, 1);
            ConditionalPutAll conditionalPutAll = new ConditionalPutAll(AlwaysFilter.Instance, new Hashtable());
            ConditionalRemove conditionalRemove = new ConditionalRemove(AlwaysFilter.Instance, true);
            ExtractorProcessor extractorProcessor = new ExtractorProcessor("member1");
            NumberIncrementor numberIncrementor = new NumberIncrementor("name1", 5, true);
            NumberMultiplier numberMultiplier = new NumberMultiplier("name2", 10, false);
            PreloadRequest preloadRequest = new PreloadRequest();
            PriorityProcessor priorityProcessor = new PriorityProcessor();
            PropertyManipulator propertyManipulator = new PropertyManipulator("name3");
            UpdaterProcessor updaterProcessor = new UpdaterProcessor("member2", 20);
            VersionedPut versionedPut = new VersionedPut();
            VersionedPutAll versionedPutAll = new VersionedPutAll(new Hashtable());

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), compositeProcessor);
            ctx.Serialize(new DataWriter(stream), conditionalProcessor);
            ctx.Serialize(new DataWriter(stream), conditionalPut);
            ctx.Serialize(new DataWriter(stream), conditionalPutAll);
            ctx.Serialize(new DataWriter(stream), conditionalRemove);
            ctx.Serialize(new DataWriter(stream), extractorProcessor);
            ctx.Serialize(new DataWriter(stream), numberIncrementor);
            ctx.Serialize(new DataWriter(stream), numberMultiplier);
            ctx.Serialize(new DataWriter(stream), preloadRequest);
            ctx.Serialize(new DataWriter(stream), priorityProcessor);
            ctx.Serialize(new DataWriter(stream), propertyManipulator);
            ctx.Serialize(new DataWriter(stream), updaterProcessor);
            ctx.Serialize(new DataWriter(stream), versionedPut);
            ctx.Serialize(new DataWriter(stream), versionedPutAll);

            stream.Position = 0;
            Assert.AreEqual(compositeProcessor, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(conditionalProcessor, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(conditionalPut, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(conditionalPutAll.GetType(), ctx.Deserialize(new DataReader(stream)).GetType());
            Assert.AreEqual(conditionalRemove, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(extractorProcessor, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(numberIncrementor, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(numberMultiplier, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(preloadRequest, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(priorityProcessor.GetType(), ctx.Deserialize(new DataReader(stream)).GetType());
            Assert.AreEqual(propertyManipulator, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(updaterProcessor, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(versionedPut, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(versionedPutAll.GetType(), ctx.Deserialize(new DataReader(stream)).GetType());

            stream.Close();
        }

        [Test]
        public void TestGetOrDefault()
        {
            IInvocableCacheEntry entry = new LocalCache.Entry(new LocalCache(), "key1", 1200);
            CacheProcessors.GetOrDefaultProcessor processor = new CacheProcessors.GetOrDefaultProcessor();
            Object result = processor.Process(entry);
            Assert.AreEqual(1200, entry.Value);
        }

        [Test]
        public void TestPutIfAbsent()
        {
            IInvocableCacheEntry entry = new LocalCache.Entry(new LocalCache(), "key1", 1200);
            CacheProcessors.InsertIfAbsentProcessor processor = new CacheProcessors.InsertIfAbsentProcessor(1300);
            Object result = processor.Process(entry);
            Assert.AreEqual(1200, entry.Value);

            entry = new LocalCache.Entry(new LocalCache(), "key1", null);
            processor = new CacheProcessors.InsertIfAbsentProcessor(1300);
            result = processor.Process(entry);
            Assert.AreEqual(1300, entry.Value);
        }

        [Test]
        public void TestRemove()
        {
            IInvocableCacheEntry entry = new LocalCache.Entry(new LocalCache(), "key1", 1200);
            CacheProcessors.RemoveProcessor processor = new CacheProcessors.RemoveProcessor();
            Object result = processor.Process(entry);
            Assert.AreEqual(1200, result);
            Assert.AreEqual(null, entry.Value);

            entry = new LocalCache.Entry(new LocalCache(), "key1", 1200);
            CacheProcessors.RemoveValueProcessor processor2 = new CacheProcessors.RemoveValueProcessor(1300);
            result = processor2.Process(entry);
            Assert.AreEqual(false, result);
            Assert.AreEqual(1200, entry.Value);

            processor2 = new CacheProcessors.RemoveValueProcessor(1200);
            result = processor2.Process(entry);
            Assert.AreEqual(true, result);
            Assert.AreEqual(null, entry.Value);
        }

        [Test]
        public void TestReplace()
        {
            IInvocableCacheEntry entry = new LocalCache.Entry(new LocalCache(), "key1", 1200);
            CacheProcessors.ReplaceProcessor processor = new CacheProcessors.ReplaceProcessor(1300);
            Object result = processor.Process(entry);
            Assert.AreEqual(1300, entry.Value);

            entry = new LocalCache.Entry(new LocalCache(), "key1", 1300);
            CacheProcessors.ReplaceValueProcessor processor2 = new CacheProcessors.ReplaceValueProcessor(1300, 1500);
            result = processor2.Process(entry);
            Assert.AreEqual(true, result);

            processor2 = new CacheProcessors.ReplaceValueProcessor(1300, 1500);
            result = processor2.Process(entry);
            Assert.AreEqual(false, result);
        }
    }
}

