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
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Net.Impl;
using Tangosol.Util.Comparator;
using Tangosol.Util.Filter;
using Tangosol.Util.Transformer;

namespace Tangosol.Util.Extractor
{
    [TestFixture]
    public class ExtractorTests
    {
        NameValueCollection appSettings = TestUtils.AppSettings;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
        }

        [Test]
        public void TestIdentityExtractor()
        {
            IValueExtractor extractor = IdentityExtractor.Instance;
            IValueExtractor extractor1 = IdentityExtractor.Instance;
            Assert.IsNotNull(extractor);
            Assert.AreEqual(extractor, extractor1);
            Assert.AreEqual(extractor.ToString(), extractor1.ToString());
            Assert.AreEqual(extractor.GetHashCode(), extractor1.GetHashCode());

            object o = new DictionaryEntry("key", "value");
            object o1 = extractor.Extract(o);
            Assert.AreEqual(o, o1);

            IdentityExtractor ie = extractor as IdentityExtractor;
            Assert.AreEqual(ie.Compare("ana", "cikic"), -1);

            TestQueryCacheEntry entry1 = new TestQueryCacheEntry("1", 1);
            TestQueryCacheEntry entry2 = new TestQueryCacheEntry("2", 2);
            Assert.AreEqual(ie.CompareEntries(entry2, entry1), 1);

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("identityExtractorKey1", 435);
            ht.Add("identityExtractorKey2", 253);
            ht.Add("identityExtractorKey3", 3);
            ht.Add("identityExtractorKey4", null);
            ht.Add("identityExtractorKey5", -3);
            cache.InsertAll(ht);

            IFilter filter = new EqualsFilter(extractor, 253);
            ICollection keys = cache.GetKeys(filter);
            Assert.Contains("identityExtractorKey2", (IList) keys);
            Assert.AreEqual(keys.Count, 1);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestPofExtractorWithValueChangeEventFilter1()
        {
            // Testing on remote cache using CustomerKeyClass, which is not
            // defined on Java side
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            // CustomerKeyClass is not defined on the Java side
            //
            Hashtable ht = new Hashtable();
            CustomKeyClass key1 = new CustomKeyClass("Customer1");
            CustomKeyClass key2 = new CustomKeyClass("Customer2");
            ht.Add("key1", key1);
            ht.Add("key2", key2);
            cache.InsertAll(ht);

            SyncListener listener = new SyncListener();
            IFilter filter = new ValueChangeEventFilter(new PofExtractor(typeof(String), 0));

            cache.AddCacheListener(listener,
                                   filter,
                                   false);
            Assert.IsNull(listener.CacheEvent);

            cache["key1"] = new CustomKeyClass("Customer1");
            Assert.IsNull(listener.CacheEvent);

            cache["key1"] = new CustomKeyClass("Customer12");
            Assert.IsNotNull(listener.CacheEvent);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestPofExtractorWithValueChangeEventFilter2()
        {
            // Testing on remote cache using Address, which is defined on
            // Java side.
            INamedCache cache = CacheFactory.GetCache(CacheName);

            cache.Clear();

            Hashtable ht = new Hashtable();
            Address address1 = new Address("Street1", "City1", "State1", "Zip1");
            Address address2 = new Address("Street2", "City2", "State2", "Zip2");
            ht.Add("key1", address1);
            ht.Add("key2", address2);
            cache.InsertAll(ht);

            SyncListener listener = new SyncListener();
            IFilter filter = new ValueChangeEventFilter(new PofExtractor(typeof(String), 0));

            cache.AddCacheListener(listener,
                                   filter,
                                   false);
            Assert.IsNull(listener.CacheEvent);

            cache["key1"] = new Address("Street1", "City1a", "State1a", "Zip1a");
            Assert.IsNull(listener.CacheEvent);

            cache["key1"] = new Address("Street1a", "City1", "State1", "Zip1");
            Assert.IsNotNull(listener.CacheEvent);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestExtractorEventTransformer()
        {
            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            Address address1 = new Address("Street1", "City1", "State1", "Zip1");
            Address address2 = new Address("Street2", "City2", "State2", "Zip2");
            ht.Add("key1", address1);
            ht.Add("key2", address2);
            cache.InsertAll(ht);

            SyncListener listener = new SyncListener();
            IFilter filter = new ValueChangeEventFilter("getStreet");
            IValueExtractor extractor = IdentityExtractor.Instance;
            ICacheEventTransformer transformer = new ExtractorEventTransformer(null, extractor);

            cache.AddCacheListener(listener,
                                   new CacheEventTransformerFilter(filter,
                                                                   transformer),
                                   false);
            Assert.IsNull(listener.CacheEvent);
            
            cache["key1"] = new Address("Street1", "City1a", "State1a", "Zip1a");
            Assert.IsNull(listener.CacheEvent);

            cache["key1"] = new Address("Street1a", "City1a", "State1a", "Zip1a");
            Assert.IsNotNull(listener.CacheEvent);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestKeyExtractor()
        {
            IValueExtractor extractor = new KeyExtractor((IValueExtractor) null);
            IValueExtractor extractor1 = new KeyExtractor((IValueExtractor) null);
            IValueExtractor extractor2 = IdentityExtractor.Instance;
            Assert.IsNotNull(extractor);
            Assert.AreEqual(extractor, extractor1);
            Assert.AreNotEqual(extractor, extractor2);
            Assert.AreEqual(extractor.ToString(), extractor1.ToString());
            Assert.AreEqual(extractor.GetHashCode(), extractor1.GetHashCode());

            extractor = new KeyExtractor(IdentityExtractor.Instance);
            Assert.IsNotNull(extractor);
            IValueExtractor innerExtractor = (extractor as KeyExtractor).Extractor;
            Assert.IsNotNull(innerExtractor);
            Assert.IsInstanceOf(typeof(IdentityExtractor), innerExtractor);

            object o = "value";
            Assert.AreEqual(o, extractor.Extract(o));

            extractor = new KeyExtractor("field");
            Assert.IsNotNull(extractor);
            innerExtractor = (extractor as KeyExtractor).Extractor;
            Assert.IsNotNull(innerExtractor);
            Assert.IsInstanceOf(typeof(ReflectionExtractor), innerExtractor);
            ReflectionExtractor re = innerExtractor as ReflectionExtractor;
            Assert.AreEqual(re.MemberName, "field");

            o = new ReflectionTestType();
            Assert.AreEqual(extractor.Extract(o), (o as ReflectionTestType).field);

            extractor = new KeyExtractor("InnerMember.field");
            Assert.IsNotNull(extractor);
            innerExtractor = (extractor as KeyExtractor).Extractor;
            Assert.IsNotNull(innerExtractor);
            Assert.IsInstanceOf(typeof(ChainedExtractor), innerExtractor);
            ChainedExtractor ce = innerExtractor as ChainedExtractor;
            Assert.IsNotNull(ce.Extractors);
            Assert.AreEqual(ce.Extractors.Length, 2);
            Assert.AreEqual(extractor.Extract(o), (o as ReflectionTestType).InnerMember.field);

            // testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("identityExtractorKey1", 435);
            ht.Add("identityExtractorKey2", 253);
            ht.Add("identityExtractorKey3", 3);
            ht.Add("identityExtractorKey4", null);
            ht.Add("identityExtractorKey5", -3);
            cache.InsertAll(ht);

            extractor = new KeyExtractor(IdentityExtractor.Instance);
            IFilter filter = new EqualsFilter(extractor, "identityExtractorKey3");
            ICollection keys = cache.GetKeys(filter);
            Assert.Contains("identityExtractorKey3", (IList) keys);
            Assert.AreEqual(keys.Count, 1);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestReflectionExtractor()
        {
            IValueExtractor extractor = new ReflectionExtractor("field");
            IValueExtractor extractor1 = new ReflectionExtractor("field");
            IValueExtractor extractor2 = IdentityExtractor.Instance;
            Assert.IsNotNull(extractor);
            Assert.AreEqual(extractor, extractor1);
            Assert.AreNotEqual(extractor, extractor2);
            Assert.AreEqual(extractor.ToString(), extractor1.ToString());
            Assert.AreEqual(extractor.GetHashCode(), extractor1.GetHashCode());

            ReflectionTestType o = new ReflectionTestType();
            Assert.AreEqual(extractor.Extract(o), o.field);

            extractor = new ReflectionExtractor("Property");
            Assert.AreEqual(extractor.Extract(o), o.Property);

            extractor = new ReflectionExtractor("GetMethod");
            Assert.AreEqual(extractor.Extract(o), o.GetMethod());
            Assert.IsNull(extractor.Extract(null));

            try
            {
                extractor = new ReflectionExtractor("InvalidMember");
                extractor.Extract(o);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e.InnerException);
            }

            // testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Address a1 = new Address("street1", "city1", "state1", "zip1");
            Address a2 = new Address("street2", "city2", "state2", "zip2");
            Address a3 = new Address("street3", "city1", "state3", "zip3");

            Hashtable ht = new Hashtable();
            ht.Add("reflectionExtractorKey1", a1);
            ht.Add("reflectionExtractorKey2", a2);
            ht.Add("reflectionExtractorKey3", a3);
            cache.InsertAll(ht);

            extractor = new ReflectionExtractor("getCity");
            IFilter filter = new EqualsFilter(extractor, "city1");
            ICollection keys = cache.GetKeys(filter);
            Assert.Contains("reflectionExtractorKey1", (IList) keys);
            Assert.Contains("reflectionExtractorKey3", (IList) keys);
            Assert.AreEqual(keys.Count, 2);

            extractor = new ReflectionExtractor("Sum", new object[] { 10 });
            o = new ReflectionTestType();
            o.field = 10;
            Assert.AreEqual(extractor.Extract(o), 20);

            o.field = 4;
            Assert.AreEqual(extractor.Extract(o), 14);

            CacheFactory.Shutdown();
        }



        [Test]
        public void TestChainedExtractor()
        {
            IValueExtractor extractor = new ChainedExtractor("InnerMember.field");

            IValueExtractor re1 = new ReflectionExtractor("InnerMember");
            IValueExtractor re2 = new ReflectionExtractor("field");
            IValueExtractor[] res = {re1, re2};

            IValueExtractor extractor1 = new ChainedExtractor(re1, re2);
            IValueExtractor extractor2 = new ChainedExtractor(res);

            Assert.IsNotNull(extractor);
            Assert.IsNotNull(extractor1);
            Assert.IsNotNull(extractor2);
            Assert.AreEqual(extractor, extractor1);
            Assert.AreEqual(extractor1, extractor2);
            Assert.AreEqual(extractor.ToString(), extractor1.ToString());
            Assert.AreEqual(extractor.GetHashCode(), extractor1.GetHashCode());

            IValueExtractor[] extractors = (extractor as ChainedExtractor).Extractors;
            Assert.IsNotNull(extractors);
            Assert.AreEqual(extractors.Length, 2);
            Assert.AreEqual(extractors[0], re1);
            Assert.AreEqual(extractors[1], re2);

            ReflectionTestType o = new ReflectionTestType();
            Assert.AreEqual(extractor.Extract(o), o.InnerMember.field);
            Assert.AreEqual(extractor.Extract(o), extractors[1].Extract(extractors[0].Extract(o)));
            Assert.IsNull(extractor.Extract(null));

            // testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Address a1 = new Address("street1", "city1", "state1", "zip1");
            Address a2 = new Address("street2", "city2", "state2", "zip2");
            Address a3 = new Address("street3", "city1", "state3", "zip3");

            Hashtable ht = new Hashtable();
            ht.Add(a1, "chainedExtractorValue1");
            ht.Add(a2, "chainedExtractorValue2");
            ht.Add(a3, "chainedExtractorValue3");
            cache.InsertAll(ht);

            extractor1 = new KeyExtractor(IdentityExtractor.Instance);
            extractor2 = new KeyExtractor(new ReflectionExtractor("getCity"));
            extractor = new KeyExtractor(new ChainedExtractor(extractor1, extractor2));

            IFilter filter = new EqualsFilter(extractor, "city1");
            ICollection keys = cache.GetKeys(filter);
            ArrayList list = new ArrayList(keys);
            Assert.AreEqual(keys.Count, 2);
            Assert.AreEqual(((Address) list[0]).City, "city1");
            Assert.AreEqual(((Address) list[1]).City, "city1");

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestMultiExtractor()
        {
            IValueExtractor extractor = new MultiExtractor("field,Property,GetMethod");

            IValueExtractor re1 = new ReflectionExtractor("field");
            IValueExtractor re2 = new ReflectionExtractor("Property");
            IValueExtractor re3 = new ReflectionExtractor("GetMethod");
            IValueExtractor[] res = {re1, re2, re3};

            IValueExtractor extractor1 = new MultiExtractor(res);

            Assert.IsNotNull(extractor);
            Assert.IsNotNull(extractor1);
            Assert.AreEqual(extractor, extractor1);
            Assert.AreEqual(extractor.ToString(), extractor1.ToString());
            Assert.AreEqual(extractor.GetHashCode(), extractor1.GetHashCode());

            IValueExtractor[] extractors = (extractor as MultiExtractor).Extractors;
            Assert.IsNotNull(extractors);
            Assert.AreEqual(extractors.Length, 3);
            Assert.AreEqual(extractors[0], re1);
            Assert.AreEqual(extractors[1], re2);
            Assert.AreEqual(extractors[2], re3);

            ReflectionTestType o1 = new ReflectionTestType();
            IList list = (IList) extractor.Extract(o1);
            Assert.AreEqual(list.Count, extractors.Length);
            for (int i = 0; i < extractors.Length; i++ )
            {
                Assert.AreEqual(list[i], extractors[i].Extract(o1));
            }
            Assert.IsNull(extractor.Extract(null));

            ReflectionTestType o2 = new ReflectionTestType();
            TestQueryCacheEntry entry1 = new TestQueryCacheEntry("key1", o1);
            TestQueryCacheEntry entry2 = new TestQueryCacheEntry("key2", o2);
            Assert.AreEqual((extractor as MultiExtractor).CompareEntries(entry1, entry2), 0);
            o2.field = 100;
            Assert.AreEqual((extractor as MultiExtractor).CompareEntries(entry1, entry2), -1);

            // testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Address a1 = new Address("street1", "city1", "state1", "zip1");
            Address a2 = new Address("treet2", "city2", "tate2", "zip2");
            Address a3 = new Address("street3", "city3", "state3", "zip3");

            Hashtable ht = new Hashtable();
            ht.Add("multiExtractorKey1", a1);
            ht.Add("multiExtractorKey2", a2);
            ht.Add("multiExtractorKey3", a3);
            cache.InsertAll(ht);

            extractor1 = new ReflectionExtractor("getStreet");
            IValueExtractor extractor2 = new ReflectionExtractor("getState");
            extractor = new MultiExtractor(new IValueExtractor[] {extractor1, extractor2});

            IFilter filter = new ContainsFilter(extractor, "street1");
            ICollection keys = cache.GetKeys(filter);
            Assert.AreEqual(keys.Count, 1);
            Assert.Contains("multiExtractorKey1", (IList) keys);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestUnitComparisonValueExtractor()
        {
            IValueExtractor exByte = new ReflectionExtractor("ByteValue");
            IValueExtractor exShort = new ReflectionExtractor("ShortValue");
            IValueExtractor exInt = new ReflectionExtractor("IntValue");
            IValueExtractor exLong = new ReflectionExtractor("LongValue");
            IValueExtractor exFloat = new ReflectionExtractor("FloatValue");
            IValueExtractor exDouble = new ReflectionExtractor("DoubleValue");
            IValueExtractor exDecimal = new ReflectionExtractor("DecimalValue");
            IValueExtractor exInt128 = new ReflectionExtractor("RawInt128Value");

            IValueExtractor[] extractors = new IValueExtractor[] { exDouble, exInt };
            //different ways to instantiate same extractor
            ComparisonValueExtractor cve1 = new ComparisonValueExtractor(extractors);
            ComparisonValueExtractor cve2 = new ComparisonValueExtractor(exDouble, exInt);
            ComparisonValueExtractor cve3 = new ComparisonValueExtractor(exDouble, exInt, null);
            ComparisonValueExtractor cve4 = new ComparisonValueExtractor("DoubleValue", "IntValue");
            ComparisonValueExtractor cve5 = new ComparisonValueExtractor("DoubleValue", "IntValue", null);
            Assert.IsNotNull(cve1);
            Assert.IsNotNull(cve2);
            Assert.IsNotNull(cve3);
            Assert.IsNotNull(cve4);
            Assert.IsNotNull(cve5);
            Assert.IsTrue(cve1.Equals(cve2));
            Assert.IsTrue(cve1.Equals(cve3));
            Assert.IsTrue(cve1.Equals(cve4));
            Assert.IsTrue(cve1.Equals(cve5));
            Assert.IsNull(cve1.Comparer);
            Assert.AreEqual(cve1.Extractors.Length, 2);

            Score score = new Score(1, 1, 126, 10000L, 1.24f, 1432.55, new decimal(11223344), new RawInt128(new byte[] {1}), 1);
            
            Assert.AreEqual(cve1.Extract(score), score.DoubleValue - Convert.ToDouble(score.IntValue));
            cve1 = new ComparisonValueExtractor(exByte, exShort);
            Assert.AreEqual(cve1.Extract(score), Convert.ToInt32(score.ByteValue) - Convert.ToInt32(score.ShortValue));
            cve1 = new ComparisonValueExtractor(exLong, exInt);
            Assert.AreEqual(cve1.Extract(score), score.LongValue - Convert.ToInt64(score.IntValue));
            cve1 = new ComparisonValueExtractor(exLong, exFloat);
            Assert.AreEqual(cve1.Extract(score), Convert.ToDouble(score.LongValue) - Convert.ToDouble(score.FloatValue));
            cve1 = new ComparisonValueExtractor(exDecimal, exDouble);
            Assert.AreEqual(cve1.Extract(score), Decimal.Subtract(score.DecimalValue, Convert.ToDecimal(score.DoubleValue)));

            ComparisonValueExtractor cve6 = new ComparisonValueExtractor("ShortValue", "ShortValue", Comparer.Default);
            Assert.AreEqual(cve6.Extract(score),
                            SafeComparer.CompareSafe(Comparer.Default, score.ShortValue, score.ShortValue));
        }

        [Test]
        public void TestComparisonValueExtractor()
        {
            // testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            Score score = new Score(1, 1, 126, 10000L, 1.24f, 1432.55, new decimal(11223344), new RawInt128(new byte[] { 1 }), 1);

            cache.Insert("score1", score);

            IValueExtractor exByte = new ReflectionExtractor("getByteValue");
            IValueExtractor exShort = new ReflectionExtractor("getShortValue");
            IValueExtractor exInt = new ReflectionExtractor("getIntValue");
            IValueExtractor exLong = new ReflectionExtractor("getLongValue");
            IValueExtractor exFloat = new ReflectionExtractor("getFloatValue");
            IValueExtractor exDouble = new ReflectionExtractor("getDoubleValue");
            IValueExtractor exDecimal = new ReflectionExtractor("getBigDecimalValue");
            IValueExtractor exInt128 = new ReflectionExtractor("getBigIntegerValue");

            //different ways to instantiate same extractor
            ComparisonValueExtractor cve1 = new ComparisonValueExtractor(exByte, exShort);
            ComparisonValueExtractor cve2 = new ComparisonValueExtractor(exInt, exLong);
            ComparisonValueExtractor cve3 = new ComparisonValueExtractor(exFloat, exDouble);
            ComparisonValueExtractor cve4 = new ComparisonValueExtractor(exDecimal, exDecimal);
            ComparisonValueExtractor cve5 = new ComparisonValueExtractor(exInt128, exInt128);

            IFilter filter = new EqualsFilter(cve1, score.ByteValue - score.ShortValue);
            ICollection keys = cache.GetKeys(filter);
            Assert.IsNotEmpty(keys);
            Assert.IsTrue(keys.Count == 1);

            filter = new EqualsFilter(cve2, score.IntValue - score.LongValue);
            keys = cache.GetKeys(filter);
            Assert.IsNotEmpty(keys);
            Assert.IsTrue(keys.Count == 1);

            filter = new EqualsFilter(cve3, score.FloatValue - score.DoubleValue);
            keys = cache.GetKeys(filter);
            Assert.IsNotEmpty(keys);
            Assert.IsTrue(keys.Count == 1);

            filter = new EqualsFilter(cve4, (decimal) 0);
            keys = cache.GetKeys(filter);
            Assert.IsNotEmpty(keys);
            Assert.IsTrue(keys.Count == 1);

            filter = new EqualsFilter(cve5, NumberUtils.DecimalToRawInt128(score.RawInt128Value.ToDecimal() - score.RawInt128Value.ToDecimal()));
            keys = cache.GetKeys(filter);
            Assert.IsNotEmpty(keys);
            Assert.IsTrue(keys.Count == 1);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestExtractorSerialization()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.IsNotNull(ctx);

            ChainedExtractor chainedExtractor = new ChainedExtractor("member1");
            ComparisonValueExtractor comparisonValueExtractor = new ComparisonValueExtractor("member2", "member3");
            IdentityExtractor identityExtractor = new IdentityExtractor();
            KeyExtractor keyExtractor = new KeyExtractor("member4");
            MultiExtractor multiExtractor = new MultiExtractor("member5,member6,member7");
            ReflectionExtractor reflectionExtractor = new ReflectionExtractor("member8");

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), chainedExtractor);
            ctx.Serialize(new DataWriter(stream), comparisonValueExtractor);
            ctx.Serialize(new DataWriter(stream), identityExtractor);
            ctx.Serialize(new DataWriter(stream), keyExtractor);
            ctx.Serialize(new DataWriter(stream), multiExtractor);
            ctx.Serialize(new DataWriter(stream), reflectionExtractor);

            stream.Position = 0;
            Assert.AreEqual(chainedExtractor, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(comparisonValueExtractor, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(identityExtractor, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(keyExtractor, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(multiExtractor, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(reflectionExtractor, ctx.Deserialize(new DataReader(stream)));

            stream.Close();
        }

        [Test]
        public void TestPofExtractorWithFilter()
        {
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            for (int i = 1901; i <= 2000; i++)
            {
                PortablePersonLite customer = new PortablePersonLite();
                customer.Name = "Name" + i;
                customer.DOB = new DateTime(i, 1, 1);
                cache.Insert(i, customer);
            }
            DateTime criteria = new DateTime(1970, 1, 1);
            IValueExtractor extractor = new PofExtractor(null, 2);
            IFilter filter2 = new LessEqualsFilter(extractor, criteria);
            Assert.AreEqual(cache.GetEntries(filter2).Length, 70,
                "Expected: 70; Result was: " + cache.GetEntries(filter2).Length);
            CacheFactory.Shutdown();
        }
    }
}