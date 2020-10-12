/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

using NUnit.Framework;
using Tangosol.Net.Cache.Support;
using Tangosol.Net.Impl;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Aggregator;
using Tangosol.Util.Collections;
using Tangosol.Util.Comparator;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Cache
{
    [TestFixture]
    public class LocalCacheTests : LocalCache
    {
        private static INamedCache GetCache(String cacheName)
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            return CacheFactory.GetCache(cacheName);
        }

        /// <summary>
        /// Testing properties that are set by constructor, specifically
        /// checking that the ones that are not set explicitely are set to
        /// the default values.
        /// </summary>
        [Test]
        public void TestLocalCacheConstructor()
        {
            const int units = 20;
            const int expiry = 600;
            const double prune = 0.5;

            LocalCache localCacheWithAllDefaults = new LocalCache();
            Assert.IsNotNull(localCacheWithAllDefaults);
            Assert.AreEqual(localCacheWithAllDefaults.HighUnits, DEFAULT_UNITS);
            Assert.AreEqual(localCacheWithAllDefaults.LowUnits, (int) (DEFAULT_UNITS * DEFAULT_PRUNE));
            Assert.AreEqual(localCacheWithAllDefaults.ExpiryDelay, DEFAULT_EXPIRE);
            Assert.IsNull(localCacheWithAllDefaults.CacheLoader);

            LocalCache localCacheWithUnits = new LocalCache(units);
            Assert.IsNotNull(localCacheWithUnits);
            Assert.AreEqual(localCacheWithUnits.HighUnits, units);
            Assert.AreEqual(localCacheWithUnits.LowUnits, (int) (units * DEFAULT_PRUNE));
            Assert.AreEqual(localCacheWithUnits.ExpiryDelay, DEFAULT_EXPIRE);
            Assert.IsNull(localCacheWithUnits.CacheLoader);

            LocalCache localCacheWithUnitsAndExpiry = new LocalCache(units, expiry);
            Assert.IsNotNull(localCacheWithUnitsAndExpiry);
            Assert.AreEqual(localCacheWithUnitsAndExpiry.HighUnits, units);
            Assert.AreEqual(localCacheWithUnitsAndExpiry.LowUnits, (int) (units * DEFAULT_PRUNE));
            Assert.AreEqual(localCacheWithUnitsAndExpiry.ExpiryDelay, expiry);
            Assert.IsNull(localCacheWithUnitsAndExpiry.CacheLoader);

            LocalCache localCacheWithUnitsAndExpiryAndPrune = new LocalCache(units, expiry, prune);
            Assert.IsNotNull(localCacheWithUnitsAndExpiryAndPrune);
            Assert.AreEqual(localCacheWithUnitsAndExpiryAndPrune.HighUnits, units);
            Assert.AreEqual(localCacheWithUnitsAndExpiryAndPrune.LowUnits, (int) (units * prune));
            Assert.AreEqual(localCacheWithUnitsAndExpiryAndPrune.ExpiryDelay, expiry);
            Assert.IsNull(localCacheWithUnitsAndExpiryAndPrune.CacheLoader);

            LocalCache localCacheWithUnitsAndExpiryAndLoader = new LocalCache(units, expiry, new LocalCacheLoader());
            Assert.IsNotNull(localCacheWithUnitsAndExpiryAndLoader);
            Assert.AreEqual(localCacheWithUnitsAndExpiryAndLoader.HighUnits, units);
            Assert.AreEqual(localCacheWithUnitsAndExpiryAndLoader.LowUnits, (int) (units * DEFAULT_PRUNE));
            Assert.AreEqual(localCacheWithUnitsAndExpiryAndLoader.ExpiryDelay, expiry);
            Assert.IsNotNull(localCacheWithUnitsAndExpiryAndLoader.CacheLoader);

            ICacheStore store = new LocalCacheStore();
            LocalCache localCacheWithUnitsAndExpiryAndStore = new LocalCache(units, expiry, store);
            Assert.IsNotNull(localCacheWithUnitsAndExpiryAndStore);
            Assert.AreEqual(localCacheWithUnitsAndExpiryAndStore.HighUnits, units);
            Assert.AreEqual(localCacheWithUnitsAndExpiryAndStore.LowUnits, (int) (units * DEFAULT_PRUNE));
            Assert.AreEqual(localCacheWithUnitsAndExpiryAndStore.ExpiryDelay, expiry);
            Assert.IsNotNull(localCacheWithUnitsAndExpiryAndStore.CacheLoader);
            Assert.AreEqual(localCacheWithUnitsAndExpiryAndStore.CacheLoader, store);
        }

        /// <summary>
        /// Testing HighUnits property.
        /// </summary>
        [Test]
        public void TestHighUnits()
        {
            LocalCache localCache = new LocalCache(20);
            Assert.IsNotNull(localCache);
            Assert.AreEqual(localCache.HighUnits, 20);

            //exception when setting HighUnits to value less than 0
            try
            {
                localCache.HighUnits = -10;
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof (ArgumentException), e);
            }

            localCache.Insert("key1", "value1");
            localCache.Insert("key2", "value2");
            localCache.Insert("key3", "value3");
            Assert.AreEqual(localCache.Units, 3);

            //when setting units to greater value, nothing's changed
            localCache.HighUnits = 45;

            //when setting units to lesser value, cache might have to shrink
            //in this case, not, because number of units is lesser than new max
            localCache.HighUnits = 24;
            Assert.AreEqual(localCache.Units, 3);

            //but in this case, yes
            //number of units is greater than new max, so prune will be executed
            localCache.HighUnits = 2;
            Assert.AreEqual(localCache.Units, localCache.LowUnits);
        }

        /// <summary>
        /// Testing LowUnits property.
        /// </summary>
        [Test]
        public void TestLowUnits()
        {
            LocalCache localCache = new LocalCache(20);
            Assert.IsNotNull(localCache);
            //low units are evaluated in constructor using default prune level
            Assert.AreEqual(localCache.LowUnits, (int) (DEFAULT_PRUNE * localCache.HighUnits));

            //using custom prune level
            localCache = new LocalCache(20, DEFAULT_EXPIRE, 0.5);
            Assert.IsNotNull(localCache);
            Assert.AreEqual(localCache.LowUnits, 10);
            //if prune level is not valid value i.e. less than 0, 0 is used
            localCache = new LocalCache(20, DEFAULT_EXPIRE, -0.5);
            Assert.IsNotNull(localCache);
            Assert.AreEqual(localCache.LowUnits, 0);
            //if prune level is not valid value i.e. greater than 0.99, 0.99 is used
            localCache = new LocalCache(20, DEFAULT_EXPIRE, 1.5);
            Assert.IsNotNull(localCache);
            Assert.AreEqual(localCache.LowUnits, 19);

            //when setting low units to value less than 0 exception is raised
            try
            {
                localCache.LowUnits = -10;
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof (ArgumentException), e);
            }

            //COHNET-95
            localCache = new LocalCache(20, DEFAULT_EXPIRE, 0.5);
            Assert.IsNotNull(localCache);
            Assert.AreEqual(localCache.PruneLevel, 0.5);
            Assert.AreEqual(localCache.LowUnits, 10);
            Assert.AreEqual(localCache.HighUnits, 20);

            localCache.LowUnits = 25;
            Assert.AreEqual(localCache.LowUnits, 10);
            localCache.PruneLevel = 0.75;
            Assert.AreEqual(localCache.LowUnits, 15);
        }

        /// <summary>
        /// Testing ExpiryDelay property.
        /// </summary>
        [Test]
        public void TestExpiryDelay()
        {
            LocalCache localCache = new LocalCache(20);
            Assert.IsNotNull(localCache);
            //expiry delay is set to default
            Assert.AreEqual(localCache.ExpiryDelay, DEFAULT_EXPIRE);

            localCache = new LocalCache(20, 600);
            Assert.IsNotNull(localCache);
            //expiry delay is set to custom value
            Assert.AreEqual(localCache.ExpiryDelay, 600);

            //cannot be set to value less than 0
            localCache.ExpiryDelay = -10;
            Assert.AreEqual(localCache.ExpiryDelay, 0);
        }

        /// <summary>
        /// Testing FlushDelay and FlushTime property.
        /// </summary>
        [Test]
        public void TestFlushDelayAndTime()
        {
            LocalCache localCache = new LocalCache(20);
            Assert.IsNotNull(localCache);
            Assert.AreEqual(localCache.FlushDelay, 0);

            //setting flush delay to value greater than 0
            long start = DateTimeUtils.GetSafeTimeMillis();
            localCache.FlushDelay = 600;
            Assert.AreEqual(localCache.FlushDelay, 600);
            Assert.IsTrue(localCache.FlushTime >= start + 600);

            //cannot be set to value less than 0
            localCache.FlushDelay = -10;
            Assert.AreEqual(localCache.FlushDelay, 0);
            //setting it to 0 sets FlushTime to long.MaxValue
            Assert.AreEqual(localCache.FlushTime, long.MaxValue);

            //flush time can be set explicitly
            long newFlushTime = DateTimeUtils.GetSafeTimeMillis() + 250;
            localCache.FlushTime = newFlushTime;
            Assert.AreEqual(localCache.FlushTime, newFlushTime);
        }

        /// <summary>
        /// Testing CalculatorType and UnitCalculator properties.
        /// </summary>
        [Test]
        public void TestUnitCalculator()
        {
            LocalCache localCache = new LocalCache();
            Assert.IsNotNull(localCache);
            //default is fixed unit calculator
            Assert.IsNull(localCache.UnitCalculator);
            Assert.AreEqual(localCache.CalculatorType, UnitCalculatorType.Fixed);

            localCache.Insert("key1", "value1");
            localCache.Insert("key2", "value2");
            Assert.AreEqual(localCache.Units, 2);
            Entry entry = localCache.GetEntry("key1");
            Assert.AreEqual(entry.Units, 1);

            //setting calculator type to 'unknown' raises an exception
            try
            {
                localCache.CalculatorType = UnitCalculatorType.Unknown;
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
            }
            //the state is unchanged
            Assert.IsNull(localCache.UnitCalculator);
            Assert.AreEqual(localCache.CalculatorType, UnitCalculatorType.Fixed);

            //setting calculator type to 'external' raises an exception
            try
            {
                localCache.CalculatorType = UnitCalculatorType.External;
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
            }
            //the state is unchanged
            Assert.IsNull(localCache.UnitCalculator);
            Assert.AreEqual(localCache.CalculatorType, UnitCalculatorType.Fixed);

            //setting unit calculator to external implementation modifies
            //both calculator type and unit calculator
            //also reevaluates total units in the cache and each entry's units
            IUnitCalculator externalUnitCalculator = new Fixed2UnitCalculator();
            localCache.UnitCalculator = externalUnitCalculator;
            Assert.AreEqual(localCache.CalculatorType, UnitCalculatorType.External);
            Assert.AreEqual(localCache.UnitCalculator, externalUnitCalculator);
            Assert.AreEqual(localCache.Units, 4);
            Assert.AreEqual(entry.Units, 2);

            //setting calculator type to 'fixed' is equivalent to setting unit calculator to 'null'
            localCache.CalculatorType = UnitCalculatorType.Fixed;
            Assert.IsNull(localCache.UnitCalculator);
            Assert.AreEqual(localCache.CalculatorType, UnitCalculatorType.Fixed);
            localCache.UnitCalculator = null;
            Assert.IsNull(localCache.UnitCalculator);
            Assert.AreEqual(localCache.CalculatorType, UnitCalculatorType.Fixed);
            //units are re-evaluated as well
            Assert.AreEqual(localCache.Units, 2);
            Assert.AreEqual(entry.Units, 1);
        }

        /// <summary>
        /// Testing EvictionPolicy and EvictionType properties.
        /// </summary>
        [Test]
        public void TestEvictionPolicy()
        {
            LocalCache localCache = new LocalCache();
            Assert.IsNotNull(localCache);
            //default is hybrid eviction policy type
            Assert.IsNull(localCache.EvictionPolicy);
            Assert.AreEqual(localCache.EvictionType, EvictionPolicyType.Hybrid);

            //setting eviction type to 'unknown' raises an exception
            try
            {
                localCache.EvictionType = EvictionPolicyType.Unknown;
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
            }
            //the state is unchanged
            Assert.IsNull(localCache.EvictionPolicy);
            Assert.AreEqual(localCache.EvictionType, EvictionPolicyType.Hybrid);

            //setting eviction type to 'external' raises an exception
            try
            {
                localCache.EvictionType = EvictionPolicyType.External;
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
            }
            //the state is unchanged
            Assert.IsNull(localCache.EvictionPolicy);
            Assert.AreEqual(localCache.EvictionType, EvictionPolicyType.Hybrid);

            //setting eviction type to any of hybrid, lru or lfu modifies
            //eviction type and sets eviction policy to null
            localCache.EvictionType = EvictionPolicyType.LFU;
            Assert.IsNull(localCache.EvictionPolicy);
            Assert.AreEqual(localCache.EvictionType, EvictionPolicyType.LFU);
            localCache.EvictionType = EvictionPolicyType.LRU;
            Assert.IsNull(localCache.EvictionPolicy);
            Assert.AreEqual(localCache.EvictionType, EvictionPolicyType.LRU);
            localCache.EvictionType = EvictionPolicyType.Hybrid;
            Assert.IsNull(localCache.EvictionPolicy);
            Assert.AreEqual(localCache.EvictionType, EvictionPolicyType.Hybrid);

            //setting eviction policy to external implementation modifies
            //both eviction type and eviction policy
            IEvictionPolicy externalEvictionPolicy = new DummyEvictionPolicy();
            localCache.EvictionPolicy = externalEvictionPolicy;
            Assert.AreEqual(localCache.EvictionType, EvictionPolicyType.External);
            Assert.AreEqual(localCache.EvictionPolicy, externalEvictionPolicy);

            //setting eviction policy to null is equivalent to setting
            //eviction type to hybrid
            localCache.EvictionPolicy = null;
            Assert.IsNull(localCache.EvictionPolicy);
            Assert.AreEqual(localCache.EvictionType, EvictionPolicyType.Hybrid);
        }

        [Test]
        public void TestEntry()
        {
            LocalCache localCache = new LocalCache();
            Assert.IsNotNull(localCache);
            LocalCacheListener listener = new LocalCacheListener();
            localCache.AddCacheListener(listener);
            Assert.AreEqual(listener.Deleted, 0);

            localCache.Add("key1", "value1");

            Entry entry = (Entry) localCache.GetCacheEntry("key1");
            Assert.IsNotNull(entry);
            Assert.AreEqual(entry.Cache, localCache);
            Assert.AreEqual(entry.Key, "key1");
            Assert.AreEqual(entry.Value, "value1");
            Assert.AreEqual(entry.Units, 1);
            Assert.AreEqual(entry.ExpiryMillis, 0);
            Assert.IsFalse(entry.IsExpired);
            Assert.IsTrue(entry.IsPresent);
            Assert.AreEqual(entry.TouchCount, 1);

            Thread.Sleep(100);
            entry.Touch();
            Assert.AreEqual(entry.TouchCount, 2);
            Assert.IsTrue(entry.LastTouchMillis > entry.CreatedMillis);

            object value = entry.Extract(IdentityExtractor.Instance);
            Assert.AreEqual(value, "value1");

            entry.ExpiryMillis = DateTimeUtils.GetSafeTimeMillis() + 25;
            Thread.Sleep(50);
            Assert.IsTrue(entry.IsExpired);

            entry.Remove(true);
            Assert.AreEqual(entry.Units, -1);
            Assert.AreEqual(listener.Deleted, 1);
        }

        /// <summary>
        /// Testing CacheLoader property, Load and LoadAll methods.
        /// </summary>
        [Test]
        public void TestLoader()
        {
            LocalCache localCache = new LocalCache();
            Assert.IsNotNull(localCache);
            Assert.AreEqual(localCache.ExpiryDelay, 0);

            localCache.Insert("key1", "key1");
            localCache.Insert("key2", "key2");
            localCache.Insert("key3", "key3");
            Assert.AreEqual(localCache.Count, 3);

            Assert.IsNull(localCache.CacheLoader);
            Assert.IsNull(localCache.GetCacheEntry("key4"));
            localCache.Load("key4");
            Assert.IsNull(localCache.GetCacheEntry("key4"));

            ArrayList keys = new ArrayList();
            keys.Add("key4");
            keys.Add("key5");

            Assert.IsNull(localCache.GetCacheEntry("key4"));
            Assert.IsNull(localCache.GetCacheEntry("key5"));
            localCache.LoadAll(keys);
            Assert.IsNull(localCache.GetCacheEntry("key4"));
            Assert.IsNull(localCache.GetCacheEntry("key5"));

            LocalCacheLoader loader = new LocalCacheLoader();
            localCache.CacheLoader = loader;
            Assert.AreEqual(localCache.CacheLoader, loader);
            Assert.IsNotNull(localCache.GetCacheEntry("key4"));
            Assert.AreEqual(localCache.Count, 4);

            localCache.GetAll(keys);
            Assert.IsNotNull(localCache.GetCacheEntry("key5"));
            Assert.AreEqual(localCache.Count, 5);
        }

        /// <summary>
        /// Testing CacheStore property, InternalListener that causes store
        /// to be updated when entries are added or modified, and removal
        /// of entries when cache is cleared or entry is removed.
        /// </summary>
        [Test]
        public void TestStore()
        {
            LocalCacheStore store = new LocalCacheStore();
            LocalCache localCache = new LocalCache(DEFAULT_UNITS, DEFAULT_EXPIRE, store);
            Assert.IsNotNull(localCache);
            Assert.AreEqual(localCache.Count, 0);
            Assert.AreEqual(localCache.CacheLoader, store);

            //testing InternalListener
            Assert.AreEqual(store.StoreDictionary.Count, 0);
            Assert.IsNull(store.StoreDictionary["key3"]);
            localCache.Insert("key1", "key1");
            localCache.Insert("key2", "key2");
            localCache.Insert("key3", "key3");
            Assert.AreEqual(localCache.Count, 3);
            Assert.AreEqual(store.StoreDictionary.Count, 3);

            Assert.AreEqual(store.StoreDictionary["key1"], "key1");
            localCache.Insert("key1", "newkey1");
            Assert.AreEqual(store.StoreDictionary["key1"], "newkey1");

            localCache.Remove("key1");
            Assert.IsNull(store.StoreDictionary["key1"]);

            localCache.Clear();
            Assert.AreEqual(localCache.Count, 0);
            Assert.AreEqual(store.StoreDictionary.Count, 0);
        }

        /// <summary>
        /// Testing different types of 'get' operations including
        /// GetCacheEntry, GetEntry, this[], GetAll, Peek, PeekAll,
        /// Contains, ContainsKey and ContainsValue.
        /// Also statistics are checked.
        /// </summary>
        [Test]
        public void TestGets()
        {
            LocalCache localCache = new LocalCache();
            Assert.IsNotNull(localCache);
            Assert.IsNotNull(localCache.CacheStatistics);
            Assert.AreEqual(localCache.CacheHits, 0);
            Assert.AreEqual(localCache.CacheMisses, 0);

            localCache.Insert("key1", "value1");
            localCache.Insert("key2", "value2");
            localCache.Insert("key3", "value3");
            Assert.AreEqual(localCache.Count, 3);

            //GetCacheEntry
            Entry entry = (Entry) localCache.GetCacheEntry("key1");
            //not expired and statistics updated
            Assert.IsNotNull(entry);
            Assert.AreEqual(entry.TouchCount, 1);
            Assert.AreEqual(localCache.CacheHits, 1);
            Assert.AreEqual(localCache.CacheMisses, 0);

            Assert.AreEqual(entry.ExpiryMillis, 0);
            Assert.IsFalse(entry.IsExpired);
            entry.ExpiryMillis = DateTimeUtils.GetSafeTimeMillis() + 25;
            Thread.Sleep(50);
            Assert.IsTrue(entry.IsExpired);
            entry = (Entry) localCache.GetCacheEntry("key1");
            Assert.IsNull(entry);
            Assert.AreEqual(localCache.CacheHits, 1);
            Assert.AreEqual(localCache.CacheMisses, 1);
            Assert.AreEqual(localCache.Count, 2);

            entry = (Entry) localCache.GetCacheEntry("key2");
            //not expired and statistics updated
            Assert.IsNotNull(entry);
            Assert.AreEqual(entry.TouchCount, 1);
            Assert.AreEqual(localCache.CacheHits, 2);
            Assert.AreEqual(localCache.CacheMisses, 1);
            Assert.AreEqual(localCache.HitProbability, 2.0/3.0);

            //GetEntry
            entry = localCache.GetEntry("key2");
            //not expired and statistics updated
            Assert.IsNotNull(entry);
            Assert.AreEqual(entry.TouchCount, 2);
            Assert.AreEqual(localCache.CacheHits, 3);
            Assert.AreEqual(localCache.CacheMisses, 1);
            Assert.AreEqual(localCache.HitProbability, 3.0/4.0);

            entry = localCache.GetEntry("key1");
            Assert.IsNull(entry);
            Assert.AreEqual(localCache.CacheHits, 3);
            Assert.AreEqual(localCache.CacheMisses, 2);
            Assert.AreEqual(localCache.HitProbability, 3.0/5.0);

            //this[]
            object value = localCache["key2"];
            //not expired and statistics updated
            Assert.IsNotNull(value);
            Assert.AreEqual(value, "value2");
            entry = (Entry) localCache.GetCacheEntry("key2");
            Assert.AreEqual(entry.TouchCount, 4);
            Assert.AreEqual(localCache.CacheHits, 5);
            Assert.AreEqual(localCache.CacheMisses, 2);
            Assert.AreEqual(localCache.HitProbability, 5.0/7.0);

            value = localCache["key1"];
            Assert.IsNull(value);
            Assert.AreEqual(localCache.CacheHits, 5);
            Assert.AreEqual(localCache.CacheMisses, 3);

            //GetAll
            ArrayList keys = new ArrayList();
            keys.Add("key1");
            keys.Add("key2");
            keys.Add("key3");

            IDictionary ht = localCache.GetAll(keys);
            Assert.AreEqual(ht.Count, 2);
            Assert.AreEqual(localCache.CacheHits, 7);
            Assert.AreEqual(localCache.CacheMisses, 4);

            //Peek
            value = localCache.Peek("key2");
            //not expired and statistics not updated
            Assert.IsNotNull(value);
            Assert.AreEqual(value, "value2");
            entry = (Entry) localCache.GetCacheEntry("key2");
            Assert.AreEqual(entry.TouchCount, 5);
            Assert.AreEqual(localCache.CacheHits, 8);
            Assert.AreEqual(localCache.CacheMisses, 4);

            //PeekAll
            ht = localCache.PeekAll(keys);
            Assert.AreEqual(ht.Count, 2);
            Assert.AreEqual(localCache.CacheHits, 8);
            Assert.AreEqual(localCache.CacheMisses, 4);
            Assert.IsFalse(ht.Contains("key1"));

            //Contains
            Assert.IsTrue(localCache.Contains("key2"));
            Assert.AreEqual(localCache.CacheHits, 8);
            Assert.AreEqual(localCache.CacheMisses, 4);
            Assert.IsFalse(localCache.Contains("key1"));
            Assert.AreEqual(localCache.CacheHits, 8);
            Assert.AreEqual(localCache.CacheMisses, 4);

            //ContainsKey
            Assert.IsTrue(localCache.ContainsKey("key2"));
            Assert.AreEqual(localCache.CacheHits, 8);
            Assert.AreEqual(localCache.CacheMisses, 4);
            Assert.IsFalse(localCache.ContainsKey("key1"));
            Assert.AreEqual(localCache.CacheHits, 8);
            Assert.AreEqual(localCache.CacheMisses, 4);

            //ContainsValue, does not update statistics
            Assert.IsTrue(localCache.ContainsValue("value2"));
            Assert.AreEqual(localCache.CacheHits, 8);
            Assert.AreEqual(localCache.CacheMisses, 4);
            Assert.IsFalse(localCache.ContainsValue(null));
            Assert.AreEqual(localCache.CacheHits, 8);
            Assert.AreEqual(localCache.CacheMisses, 4);
        }

        /// <summary>
        /// Testing different types of 'get' operations including
        /// GetCacheEntry, GetEntry, this[], GetAll, Peek and PeekAll
        /// when CacheLoader is not null.
        /// Also statistics are checked.
        /// </summary>
        [Test]
        public void TestGetsWithLoader()
        {
            LocalCacheLoader loader = new LocalCacheLoader();
            LocalCache localCache = new LocalCache(DEFAULT_UNITS, DEFAULT_EXPIRE, loader);
            Assert.IsNotNull(localCache);
            Assert.IsNotNull(localCache.CacheLoader);

            localCache.Insert("key1", "value1");
            localCache.Insert("key2", "value2");
            localCache.Insert("key3", "value3");
            Assert.AreEqual(localCache.Count, 3);

            //GetCacheEntry is not dependant on loader
            //GetEntry
            Entry entry = localCache.GetEntry("key2");
            Assert.IsNotNull(entry);
            Assert.AreEqual(localCache.CacheHits, 1);
            Assert.AreEqual(localCache.CacheMisses, 0);

            //the entry does not exist, but will be loaded
            entry = localCache.GetEntry("key4");
            Assert.IsNotNull(entry);
            Assert.AreEqual(localCache.Count, 4);
            Assert.AreEqual(entry.Key, entry.Value);
            Assert.AreEqual(localCache.CacheHits, 1);
            Assert.AreEqual(localCache.CacheMisses, 1);

            //this[] behaves the same
            object value = localCache["key5"];
            Assert.IsNotNull(value);
            Assert.AreEqual(value, "key5");
            Assert.AreEqual(localCache.Count, 5);
            Assert.AreEqual(localCache.CacheHits, 1);
            Assert.AreEqual(localCache.CacheMisses, 2);

            //GetAll
            ArrayList keys = new ArrayList();
            keys.Add("key3");
            keys.Add("key6");
            keys.Add("key7");

            IDictionary ht = localCache.GetAll(keys);
            Assert.AreEqual(ht.Count, 3);
            Assert.AreEqual(localCache.Count, 7);
            Assert.AreEqual(localCache.CacheHits, 2);
            Assert.AreEqual(localCache.CacheMisses, 4);

            //Peek and PeekAll are not dependant on loader

            //Contains
            Assert.IsTrue(localCache.Contains("key2"));
            Assert.AreEqual(localCache.CacheHits, 2);
            Assert.AreEqual(localCache.CacheMisses, 4);
            Assert.IsFalse(localCache.Contains("key8"));
            Assert.AreEqual(localCache.CacheHits, 2);
            Assert.AreEqual(localCache.CacheMisses, 4);

            //ContainsValue not dependant on loader
        }

        /// <summary>
        /// Testing different types of put methods including:
        /// Add, Insert, Insert with milliseconds, and this[] setter.
        /// Also statistics and listeners are checked.
        /// </summary>
        [Test]
        public void TestPuts()
        {
            LocalCacheListener listener = new LocalCacheListener();
            LocalCache localCache = new LocalCache(20);
            localCache.AddCacheListener(listener);

            Assert.AreEqual(listener.Inserted, 0);
            Assert.AreEqual(listener.Updated, 0);
            Assert.AreEqual(listener.Deleted, 0);

            //Add
            localCache.Add("key1", "value1");
            Assert.AreEqual(localCache.Count, 1);
            Assert.AreEqual(localCache.CacheStatistics.TotalPuts, 1);
            Assert.AreEqual(listener.Inserted, 1);

            try
            {
                localCache.Add("key1", "newvalue1");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof (ArgumentException), e);
            }
            Assert.AreEqual(localCache.GetCacheEntry("key1").Value, "value1");

            //Insert without millis
            localCache.Insert("key2", "value2");
            Assert.AreEqual(localCache.Count, 2);
            Assert.AreEqual(localCache.CacheStatistics.TotalPuts, 2);
            Assert.AreEqual(listener.Inserted, 2);

            Entry entry = (Entry) localCache.GetCacheEntry("key2");
            Assert.IsNotNull(entry);
            Assert.AreEqual(entry.TouchCount, 1);

            localCache.Insert("key2", "newvalue2");
            Assert.AreEqual(localCache.Count, 2);
            Assert.AreEqual(localCache.CacheStatistics.TotalPuts, 3);
            Assert.AreEqual(listener.Inserted, 2);
            Assert.AreEqual(listener.Updated, 1);

            entry = (Entry) localCache.GetCacheEntry("key2");
            Assert.IsNotNull(entry);
            Assert.AreEqual(entry.Value, "newvalue2");
            Assert.AreEqual(entry.TouchCount, 3);

            Assert.AreEqual(localCache.LowUnits, 15);
            //insert enough entries to fill the cache and invoke Prune
            for (int i=3; i<=21; i++)
            {
                localCache.Insert("key" + i, "value" + i);
            }
            Assert.AreEqual(localCache.Count, 15);

            localCache.Clear();
            Assert.AreEqual(localCache.HighUnits, 20);
            Assert.AreEqual(localCache.LowUnits, 15);
            Assert.AreEqual(localCache.ExpiryDelay, 0);
            Assert.AreEqual(localCache.FlushDelay, 0);
            Assert.AreEqual(localCache.FlushTime, long.MaxValue);

            //Insert with millis
            localCache.Insert("key1", "value1", 25);
            entry = (Entry) localCache.GetCacheEntry("key1");
            Assert.IsNotNull(entry);
            Assert.IsTrue(entry.ExpiryMillis > 0L);
            Assert.IsFalse(entry.IsExpired);
            Assert.AreEqual(localCache.ExpiryDelay, 0);
            Assert.AreEqual(localCache.FlushDelay, DEFAULT_FLUSH);
            Assert.IsTrue(localCache.FlushTime != long.MaxValue);

            Thread.Sleep(50);
            Assert.IsTrue(entry.IsExpired);
        }
        /// <summary>
        /// Testing Remove and Clear.
        /// </summary>
        [Test]
        public void TestRemoves()
        {
            LocalCacheListener listener = new LocalCacheListener();
            LocalCache localCache = new LocalCache(20);
            localCache.AddCacheListener(listener);
            Assert.AreEqual(listener.Deleted, 0);

            localCache.Add("key1", "value1");
            Assert.AreEqual(localCache.Count, 1);

            localCache.Remove("key2");
            Assert.AreEqual(localCache.Count, 1);
            localCache.Remove("key1");
            Assert.AreEqual(localCache.Count, 0);
            Assert.AreEqual(listener.Deleted, 1);

            localCache.Insert("key1", "value1");
            localCache.Insert("key2", "value2");
            localCache.Insert("key3", "value3");
            Assert.AreEqual(localCache.Count, 3);

            localCache.Clear();
            Assert.AreEqual(localCache.Count, 0);
            Assert.AreEqual(listener.Deleted, 4);
        }

        [Test]
        public void TestCheckFlush()
        {
            INamedCache cache = GetCache("local-default");
            Assert.IsTrue(cache is LocalNamedCache);

            LocalNamedCache localNamedCache = (LocalNamedCache) cache;
            LocalCache localCache = localNamedCache.LocalCache;

            localCache.Clear();

            localCache.ExpiryDelay = 100;
            localCache.FlushTime = DateTimeUtils.GetSafeTimeMillis() + 200;
            localCache.FlushDelay = 200;
            Hashtable ht = new Hashtable();
            ht.Add("Key1", 435);
            ht.Add("Key2", 253);
            ht.Add("Key3", 3);
            ht.Add("Key4", 200);
            ht.Add("Key5", 333);
            localCache.InsertAll(ht);

            Assert.AreEqual(5, localCache.Count);

            Thread.Sleep(400);
            Assert.AreEqual(0, localCache.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestEvicting()
        {
            INamedCache cache = GetCache("local-default");
            Assert.IsTrue(cache is LocalNamedCache);

            LocalNamedCache localNamedCache = (LocalNamedCache) cache;
            LocalCache localCache = localNamedCache.LocalCache;

            localCache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("Key1", 435);
            ht.Add("Key2", 253);
            ht.Add("Key3", 3);
            ht.Add("Key4", 200);
            ht.Add("Key5", 333);
            cache.InsertAll(ht);

            Assert.IsNotNull(localCache["Key2"]);

            localCache.Evict("Key2");

            Assert.IsNull(localCache["Key2"]);

            ArrayList evictEntries = new ArrayList();
            evictEntries.Add("Key1");
            evictEntries.Add("Key4");
            evictEntries.Add("Key5");

            localCache.EvictAll(evictEntries);
            Assert.IsNotNull(localCache["Key3"]);
            Assert.IsNull(localCache["Key1"]);
            Assert.IsNull(localCache["Key4"]);
            Assert.IsNull(localCache["Key5"]);

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

            LocalNamedCache localNamedCache = (LocalNamedCache) cache;
            LocalCache localCache = localNamedCache.LocalCache;

            localCache.Clear();

            localCache.AddIndex(IdentityExtractor.Instance, true, null);
            localCache.RemoveIndex(IdentityExtractor.Instance);

            localCache.AddIndex(IdentityExtractor.Instance, true, SafeComparer.Instance);
            localCache.RemoveIndex(IdentityExtractor.Instance);

            localCache.AddIndex(IdentityExtractor.Instance, false, null);
            localCache.RemoveIndex(IdentityExtractor.Instance);

            localCache.AddIndex(IdentityExtractor.Instance, false, SafeComparer.Instance);
            localCache.RemoveIndex(IdentityExtractor.Instance);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestGetKeys()
        {
            LocalCache localCache = new LocalCache();
            Assert.IsNotNull(localCache);
            Assert.IsTrue(localCache.Count == 0);

            localCache.Add("key1", 100);
            Hashtable ht = new Hashtable();
            ht.Add("key2", -10);
            ht.Add("key3", 45);
            ht.Add("key4", 398);
            localCache.InsertAll(ht);

            ICollection entries = localCache.Entries;
            ICollection keys = localCache.Keys;
            ICollection values = localCache.Values;

            Assert.AreEqual(entries.Count, ht.Count + 1);
            Assert.AreEqual(keys.Count, values.Count);

            Assert.Contains("key1", new ArrayList(keys));
            Assert.Contains(-10, new ArrayList(values));

            // Do twice, once without an index and once with.
            for (int i = 0; i < 2; ++i )
            {
                //without filter
                object[] result = localCache.GetKeys(null);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, keys.Count);

                //with filter
                IFilter filter = new GreaterFilter(IdentityExtractor.Instance, 55);
                result = localCache.GetKeys(filter);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, 2);
                Assert.Contains("key1", result);
                Assert.Contains("key4", result);

                //with filter and key extractor
                filter = new EqualsFilter(new KeyExtractor(IdentityExtractor.Instance), "key3");
                result = localCache.GetKeys(filter);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, 1);
                Assert.Contains("key3", result);

                //with never filter
                filter = NeverFilter.Instance;
                result = localCache.GetKeys(filter);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, 0);

                // add index here for the second pass
                localCache.AddIndex(IdentityExtractor.Instance, true, SafeComparer.Instance);
            }
            localCache.RemoveIndex(IdentityExtractor.Instance);
            localCache.Clear();
        }

        [Test]
        public void TestGetValues()
        {
            LocalCache localCache = new LocalCache();

            Hashtable ht = new Hashtable();
            ht.Add("key4", 0);
            ht.Add("key3", -10);
            ht.Add("key2", 45);
            ht.Add("key1", 398);
            localCache.InsertAll(ht);

            // Do twice, once without an index and once with.
            for (int i = 0; i < 2; ++i )
            {
                //without filter
                object[] result = localCache.GetValues(null);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, localCache.Values.Count);

                //without filter, with comparer
                result = localCache.GetValues(null, null);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, localCache.Values.Count);
                Assert.AreEqual(result[0], -10);
                Assert.AreEqual(result[result.Length - 1], 398);

                //with filter, without comparer
                IFilter filter = new LessFilter(IdentityExtractor.Instance, 1);
                result = localCache.GetValues(filter);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, 2);

                //with filter, with comparer
                filter =
                    new AndFilter(new LessEqualsFilter(new KeyExtractor(IdentityExtractor.Instance), "key3"),
                                  new GreaterFilter(IdentityExtractor.Instance, 0));
                result = localCache.GetValues(filter, SafeComparer.Instance);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, 2);
                Assert.AreEqual(result[0], 45);

                // add index here for the second pass
                localCache.AddIndex(IdentityExtractor.Instance, true, SafeComparer.Instance);
            }
            localCache.RemoveIndex(IdentityExtractor.Instance);
        }

        [Test]
        public void TestGetEntries()
        {
            LocalCache localCache = new LocalCache();

            Hashtable ht = new Hashtable();
            ht.Add("key4", 0);
            ht.Add("key3", -10);
            ht.Add("key2", 45);
            ht.Add("key1", 398);
            localCache.InsertAll(ht);

            // Do twice, once without an index and once with.
            for (int i = 0; i < 2; ++i )
            {
                //without filter
                object[] result = localCache.GetEntries(null);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, localCache.Entries.Count);

                //without filter, with value comparer
                result = localCache.GetEntries(null, null);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, localCache.Entries.Count);
                Assert.AreEqual(((ICacheEntry)result[0]).Value, -10);
                Assert.AreEqual(((ICacheEntry)result[result.Length - 1]).Value, 398);

                //without filter, with key comparer
                result = localCache.GetEntries(null, new KeyExtractor(IdentityExtractor.Instance));
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, localCache.Entries.Count);
                Assert.AreEqual(((ICacheEntry)result[0]).Key, "key1");
                Assert.AreEqual(((ICacheEntry)result[result.Length - 1]).Key, "key4");

                //with filter, without comparer
                IFilter filter = new LessFilter(IdentityExtractor.Instance, 1);
                result = localCache.GetEntries(filter);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, 2);

                //with filter, with comparer
                filter =
                    new AndFilter(new LessEqualsFilter(new KeyExtractor(IdentityExtractor.Instance), "key3"),
                                  new GreaterFilter(IdentityExtractor.Instance, 0));
                result = localCache.GetEntries(filter, SafeComparer.Instance);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, 2);
                Assert.AreEqual(((ICacheEntry)result[0]).Value, 45);

                //with limit filter, returns 2 entries
                filter = new LimitFilter(new GreaterEqualsFilter(IdentityExtractor.Instance, 0), 2);
                result = localCache.GetEntries(filter);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, 2);

                //without limit filter, returns 3 entries
                filter = new GreaterEqualsFilter(IdentityExtractor.Instance, 0);
                result = localCache.GetEntries(filter);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Length, 3);

                // add index here for the second pass
                localCache.AddIndex(IdentityExtractor.Instance, true, SafeComparer.Instance);
            }
            localCache.RemoveIndex(IdentityExtractor.Instance);
        }

       [Test]
        public void TestInvoke()
        {
            LocalCache localCache = new LocalCache();
            for (int i = 0; i < 4; i++)
            {
                ReflectionTestType o = new ReflectionTestType();
                o.field = i;
                localCache.Insert("key" + i, o);
            }
            Assert.AreEqual(4, localCache.CacheStatistics.TotalPuts);
            Assert.AreEqual(0, localCache.CacheMisses);
            Assert.AreEqual(0, localCache.CacheHits);

            //invoke on key
            IEntryProcessor processor = new ExtractorProcessor("field");
            object result = localCache.Invoke("key2", processor);
            Assert.IsNotNull(result);
            Assert.AreEqual(result, 2);
            Assert.AreEqual(4, localCache.CacheStatistics.TotalPuts);
            Assert.AreEqual(0, localCache.CacheMisses);
            Assert.AreEqual(1, localCache.CacheHits);


            //invoke on non-existent key, should return null
            result = localCache.Invoke("key", processor);
            Assert.IsNull(result);
            Assert.AreEqual(4, localCache.CacheStatistics.TotalPuts);
            Assert.AreEqual(1, localCache.CacheMisses);
            Assert.AreEqual(1, localCache.CacheHits);


            //invoke on non-existent key, but this one should insert new entry
            Assert.IsTrue(localCache.Count == 4);
            processor = new ConditionalPut(AlwaysFilter.Instance, new ReflectionTestType());
            localCache.Invoke("key", processor);
            Assert.IsTrue(localCache.Count == 5);
            Assert.AreEqual(5, localCache.CacheStatistics.TotalPuts);
            Assert.AreEqual(2, localCache.CacheMisses);
            Assert.AreEqual(1, localCache.CacheHits);

            //invoke on non-existent key, with PresentFilter, should not insert new entry
            processor = new ConditionalPut(PresentFilter.Instance, new ReflectionTestType());
            localCache.Invoke("missingKey", processor);
            Assert.IsTrue(localCache.Count == 5);
            Assert.AreEqual(5, localCache.CacheStatistics.TotalPuts);
            // <deprecated>
            // in previous invoke entry is missed 2 times
            // first in EnsureEntry and after that in IsPresent property !!!
            // </deprecated>
            // COHNET-101 fixed this
            Assert.AreEqual(3, localCache.CacheMisses);
            Assert.AreEqual(1, localCache.CacheHits);

            localCache.Clear();
            for (int i = 0; i < 4; i++)
            {
                ReflectionTestType o = new ReflectionTestType();
                o.field = i;
                localCache.Insert("key" + i, o);
            }
            Assert.AreEqual(localCache.Count, 4);

            //invoke on collection determined by filter
            processor = new NumberIncrementor("field", 2, false);
            IFilter filter = new GreaterEqualsFilter("field", 3);
            Assert.AreEqual(((ReflectionTestType) localCache["key3"]).field, 3);
            result = localCache.InvokeAll(filter, processor);
            IDictionary dictResult = result as IDictionary;
            Assert.IsNotNull(dictResult);
            Assert.IsTrue(dictResult.Count == 1);
            Assert.AreEqual(dictResult["key3"], 5);

            ArrayList keys = new ArrayList();
            keys.Add("key0");
            keys.Add("key3");
            keys.Add("missingKey");
            Assert.AreEqual(localCache.GetKeys(filter).Length, 1);

            //invoke on collection of keys, non-existent keys are ignored by this processor
            result = localCache.InvokeAll(keys, processor);
            dictResult = result as IDictionary;
            Assert.IsNotNull(dictResult);
            Assert.AreEqual(dictResult.Count, keys.Count);
            Assert.AreEqual(dictResult["key0"], 2);
            Assert.AreEqual(dictResult["key3"], 7);
            Assert.IsNull(dictResult["missingKey"]);

            //invoke on collection of keys, non-existent keys are NOT ignored by this processor
            //ConditionalPut
            processor = new ConditionalPut(AlwaysFilter.Instance, 100, true);
            result = localCache.InvokeAll(keys, processor);
            dictResult = result as IDictionary;
            Assert.IsNotNull(dictResult);
            //results should be empty since all values should be put
            Assert.AreEqual(dictResult.Count, 0);

            Assert.AreEqual(localCache["key0"], 100);
            Assert.AreEqual(localCache["key3"], 100);
            Assert.AreEqual(localCache["missingKey"], 100);

            //invoke on collection of keys
            processor = new ConditionalPut(NeverFilter.Instance, 99, true);
            result = localCache.InvokeAll(keys, processor);
            dictResult = result as IDictionary;
            Assert.IsNotNull(dictResult);
            //results should be the keys passed into InvokeAll and the current value
            Assert.AreEqual(dictResult.Count, keys.Count);

            Assert.AreEqual(dictResult["key0"], 100);
            Assert.AreEqual(dictResult["key3"], 100);
            Assert.AreEqual(dictResult["missingKey"], 100);

            //ConditionalPutAll
            Hashtable ht = new Hashtable();
            ht.Add("key1", 100);
            ht.Add("key2", 200);
            ht.Add("key3", 300);
            processor = new ConditionalPutAll(AlwaysFilter.Instance, ht);
            result = localCache.InvokeAll(keys, processor);
            dictResult = result as IDictionary;
            Assert.IsNotNull(dictResult);
            //results should always be empty
            Assert.AreEqual(dictResult.Count, 0);

            //ConditionalRemove
            processor = new ConditionalRemove(NeverFilter.Instance, true);
            result = localCache.InvokeAll(keys, processor);
            dictResult = result as IDictionary;
            Assert.IsNotNull(dictResult);
            //results should be the keys passed into InvokeAll and the current value
            Assert.AreEqual(dictResult.Count, keys.Count);

            Assert.AreEqual(dictResult["key0"], 100);
            Assert.AreEqual(dictResult["key3"], 300);
            Assert.AreEqual(dictResult["missingKey"], 100);

            processor = new ConditionalRemove(AlwaysFilter.Instance, true);
            result = localCache.InvokeAll(keys, processor);
            dictResult = result as IDictionary;
            Assert.IsNotNull(dictResult);
            //results should be the keys passed into InvokeAll and the current value
            Assert.AreEqual(dictResult.Count, 0);

            Assert.AreEqual(localCache["key0"], null);
            Assert.AreEqual(localCache["key3"], null);
            Assert.AreEqual(localCache["missingKey"], null);
        
        }

        [Test]
        public void TestAggregate()
        {
            LocalCache localCache = new LocalCache();

            Hashtable ht = new Hashtable();
            ht.Add("key4", 0);
            ht.Add("key3", -10);
            ht.Add("key2", 45);
            ht.Add("key1", 398);
            localCache.InsertAll(ht);

            IEntryAggregator aggregator = new LongSum(IdentityExtractor.Instance);
            object result = localCache.Aggregate(localCache.Keys, aggregator);
            Assert.IsNotNull(result);
            Assert.AreEqual(result, 433);

            aggregator = new ComparableMax(IdentityExtractor.Instance);
            result = localCache.Aggregate(localCache.Keys, aggregator);
            Assert.IsNotNull(result);
            Assert.AreEqual(result, 398);

            IFilter filter = new LessEqualsFilter(IdentityExtractor.Instance, 0);
            result = localCache.Aggregate(filter, aggregator);
            Assert.IsNotNull(result);
            Assert.AreEqual(result, 0);

            localCache.Add("key5", 0);
            localCache.Add("key6", 45);
            localCache.Add("key7", 500);

            aggregator = new DistinctValues(IdentityExtractor.Instance);
            result = localCache.Aggregate(localCache.Keys, aggregator);
            Assert.IsNotNull(result);
            IList list = (IList) result;
            Assert.AreEqual(list.Count, 5);

            aggregator = new Count();
            result = localCache.Aggregate(localCache.Keys, aggregator);
            Assert.IsNotNull(result);
            Assert.AreEqual(result, localCache.Count);
        }

        [Test]
        public void TestDisposeDefaultLocalCache()
        {
            INamedCache cache = GetCache("local-default");
            cache.AddCacheListener(new DisposableCacheListener());
            
            LocalNamedCache localNamedCache = (LocalNamedCache) cache;
            LocalCache localCache = localNamedCache.LocalCache;
            localCache.Clear();

            TestDisposableObject value1 = new TestDisposableObject();
            localCache.Insert("key1", value1);

            TestDisposableObject result = (TestDisposableObject) localCache.GetEntry("key1").Value;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Stream);
            Assert.IsTrue(result.Stream.CanRead);

            localCache.Remove("key1");
            // all object  resources should be  released
            Assert.IsFalse(result.Stream.CanRead);
            Assert.IsNotNull(result.Stream);

            Hashtable ht = new Hashtable();
            ht.Add("key1", new TestDisposableObject());
            ht.Add("key2", new TestDisposableObject());
            ht.Add("key3", new TestDisposableObject());
            localCache.InsertAll(ht);

            IDictionary results = localCache.GetAll(ht.Keys);
            foreach(TestDisposableObject tdo in results.Values)
            {
                Assert.IsTrue(tdo.Stream.CanRead);
            }
            localCache.Clear();
            foreach(TestDisposableObject tdo in results.Values)
            {
                Assert.IsFalse(tdo.Stream.CanRead);
            }

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestDisposeLocalCacheWithTimeExpiry()
        {
            LocalNamedCache cache = new LocalNamedCache(2, 10);
            cache.AddCacheListener(new DisposableCacheListener());

            TestDisposableObject value1 = new TestDisposableObject();
            Assert.IsNotNull(value1.Stream);
            Assert.IsTrue(value1.Stream.CanRead);
            cache.Insert("key1", value1);

            // wait for expiration
            Thread.Sleep(50);
            Assert.IsNull(cache["key1"]);
            Assert.IsFalse(value1.Stream.CanRead);
        }

        [Test]
        public void TestDisposeLocalCacheWithPrune()
        {
            LocalCache localCache = new LocalCache(2);
            localCache.AddCacheListener(new DisposableCacheListener());
            localCache.Clear();

            localCache.EvictionType = EvictionPolicyType.LFU;

            TestDisposableObject value1 = new TestDisposableObject();
            TestDisposableObject value2 = new TestDisposableObject();
            TestDisposableObject value3 = new TestDisposableObject();
            localCache.Insert("key1", value1);
            localCache.Insert("key2", value2);

            Assert.IsNotNull(value1.Stream);
            Assert.IsNotNull(value2.Stream);
            Assert.IsTrue(value1.Stream.CanRead);
            Assert.IsTrue(value2.Stream.CanRead);

            Assert.IsNotNull(localCache.GetEntry("key1").Value);
            localCache.Insert("key3", value3);

            Assert.IsTrue(value1.Stream.CanRead);
            Assert.IsFalse(value2.Stream.CanRead);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestKeys()
        {
            LocalCache localCache = new LocalCache(4, 50);
            localCache.FlushDelay = 30;
            localCache.Clear();

            localCache.Add("one", 1);
            localCache.Add("two", 2);
            localCache.Add("three", 3);
            localCache.Add("four", 4);

            ArrayList keys = new ArrayList(localCache.Keys);
            Assert.AreEqual(4, localCache.Keys.Count);
            Assert.Contains("one", keys);
            Assert.Contains("two", keys);
            Assert.Contains("three", keys);
            Assert.Contains("four", keys);

            Thread.Sleep(100);
            Assert.AreEqual(0, localCache.Keys.Count);
        }

        [Test]
        public void TestValues()
        {

            LocalCache localCache = new LocalCache(4, 50);
            localCache.FlushDelay = 30;
            localCache.Clear();

            localCache.Add("one", 1);
            localCache.Add("two", 2);
            localCache.Add("three", 3);
            localCache.Add("four", 4);

            ArrayList values = new ArrayList(localCache.Values);
            Assert.AreEqual(4, values.Count);
            Assert.Contains(1, values);
            Assert.Contains(2, values);
            Assert.Contains(3, values);
            Assert.Contains(4, values);

            Thread.Sleep(100);

            values = new ArrayList(localCache.Values);
            Assert.AreEqual(0, values.Count);
        }

        [Test]
        public void TestEntries()
        {
            LocalCache localCache = new LocalCache(4, 50);
            localCache.FlushDelay = 30;
            localCache.Clear();

            Entry entry1 = new Entry(localCache, "one", 1);
            Entry entry2 = new Entry(localCache, "two", 2);
            Entry entry3 = new Entry(localCache, "three", 3);
            Entry entry4 = new Entry(localCache, "four", 4);

            localCache.Add(entry1.Key, entry1);
            localCache.Add(entry2.Key, entry2);
            localCache.Add(entry3.Key, entry3);
            localCache.Add(entry4.Key, entry4);

            ArrayList entries = new ArrayList(localCache.Entries);
            Assert.AreEqual(4, entries.Count);
            Assert.Contains(entry1, entries);
            Assert.Contains(entry2, entries);
            Assert.Contains(entry3, entries);
            Assert.Contains(entry4, entries);

            Thread.Sleep(100);

            entries = new ArrayList(localCache.Keys);
            Assert.AreEqual(0, entries.Count);
        }

        [Test]
        public void TestCopyTo()
        {
            LocalCache localCache = new LocalCache(4, 50);
            localCache.Clear();

            localCache.Add("one", 1);
            localCache.Add("two", 2);
            localCache.Add("three", 3);
            localCache.Add("four", 4);

            ArrayList values = new ArrayList(localCache.Values);
            object[] values2 = new object[values.Count];
            localCache.CopyTo(values2, 0);
            for (int i = 0; i < values.Count; i++)
            {
                Assert.AreEqual(values[i], values2[i]);
                values2[i] = null;
            }

            Thread.Sleep(100);

            localCache.CopyTo(values2, 0);
            for (int i = 0; i < values.Count; i++)
            {
                Assert.IsNull(values2[i]);
            }
        }

        /// <summary>
        /// Testing different events dispatching after
        /// Add, Insert, Entry.Value and entry.Discard methods.
        /// </summary>
        [Test]
        public void TestEventsDispatching()
        {
            LocalCacheListener listener = new LocalCacheListener();
            LocalCache localCache = new LocalCache(20);
            localCache.AddCacheListener(listener);

            Assert.AreEqual(listener.Inserted, 0);
            Assert.AreEqual(listener.Updated, 0);
            Assert.AreEqual(listener.Deleted, 0);

            // Inserted with Add
            localCache.Add("key1", "value1");
            Assert.AreEqual(listener.Inserted, 1);
            Assert.AreEqual(listener.Updated, 0);
            Assert.AreEqual(listener.Deleted, 0);

            // Inserted with Insert
            localCache.Insert("key2", "value2");
            Assert.AreEqual(listener.Inserted, 2);
            Assert.AreEqual(listener.Updated, 0);
            Assert.AreEqual(listener.Deleted, 0);

            Entry entry = (Entry) localCache.GetCacheEntry("key2");
            Assert.IsNotNull(entry);
            Assert.AreEqual(entry.TouchCount, 1);

            // Inserted with Insert
            localCache.Insert("key3", "value3");
            Assert.AreEqual(listener.Inserted, 3);
            Assert.AreEqual(listener.Updated, 0);
            Assert.AreEqual(listener.Deleted, 0);
            Assert.AreEqual(localCache.Count, 3);

            // Updated with Insert
            localCache.Insert("key2", "newvalue2");
            Assert.AreEqual(localCache.Count, 3);
            Assert.AreEqual(listener.Inserted, 3);
            Assert.AreEqual(listener.Updated, 1);
            Assert.AreEqual(listener.Deleted, 0);

            entry = (Entry) localCache.GetCacheEntry("key2");
            Assert.IsNotNull(entry);
            Assert.AreEqual(entry.Value, "newvalue2");
            Assert.AreEqual(entry.TouchCount, 3);

            // Updated directly with Value, just checking events
            entry.Value = "newestvalue2";
            Assert.AreEqual(localCache.Count, 3);
            Assert.AreEqual(listener.Inserted, 3);
            Assert.AreEqual(listener.Updated, 2);
            Assert.AreEqual(listener.Deleted, 0);
            Assert.AreEqual(entry.Value, "newestvalue2");

            localCache.Remove("key2");
            Assert.AreEqual(localCache.Count, 2);
            Assert.AreEqual(listener.Inserted, 3);
            Assert.AreEqual(listener.Updated, 2);
            Assert.AreEqual(listener.Deleted, 1);

            entry = (Entry) localCache.GetCacheEntry("key2");
            Assert.IsNull(entry);
        }

        [Test]
        public void TestGetAllWithSameKeys()
        {
            LocalCache cache = new LocalCache();
            cache.Clear();
            IDictionary dict = new Hashtable();
            for (int i = 0; i < 10; i++)
            {
                dict.Add("key" + i, "value" + i);
            }
            cache.InsertAll(dict);
            string[] keys = { "key1", "key2", "key1", "key1" };
            IDictionary result = cache.GetAll(keys);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void TestForCOH2353()
        {
            IQueryCache cache = new LocalCache();
            cache.AddIndex(new IdentityExtractor(), false, null);
            cache.Insert("baz", null);
        }

        /// <summary>
        /// Test of accessing various LocalCache methods concurrently.
        /// </summary>
        [Test]
        public void TestConcurrentAccess()
        {
            LocalCache         cache     = new LocalCache(100, 1000);
            LocalCacheAccessor accessor1 = new LocalCacheAccessor("accessor1", cache);
            LocalCacheAccessor accessor2 = new LocalCacheAccessor("accessor2", cache);

            // run the two tests concurrently
            Thread thread1 = new Thread(new ThreadStart(accessor1.Run));
            Thread thread2 = new Thread(new ThreadStart(accessor2.Run));
            thread1.Start();
            thread2.Start();

            // wait for the threads to finish
            thread1.Join(60000);
            thread2.Join(60000);
            if (thread1.IsAlive || thread2.IsAlive)
            {
                string message = "deadlock detected:";
                if (thread1.IsAlive)
                {
                    thread1.Suspend();
                    message += "\nthread1: ALIVE";
                }
                if (thread2.IsAlive)
                {
                    thread2.Suspend();
                    message += "\nthread2: ALIVE";
                }

                try
                {
                    if (thread1.IsAlive)
                    {
                        thread1.Interrupt();
                        thread1.Abort();
                    }
                }
                catch (Exception)
                { }
                try
                {
                    if (thread2.IsAlive)
                    {
                        thread2.Interrupt();
                        thread2.Abort();
                    }
                }
                catch (Exception)
                { }
                Assert.Fail(message);
            }

            Assert.IsTrue(accessor1.IsSuccess, "accessor1 failure: " + accessor1.Error);
            Assert.IsTrue(accessor2.IsSuccess, "accessor2 failure: " + accessor2.Error);
        }

        /// <summary>
        /// Testing listener configured in config file.
        /// </summary>
        [Test]
        public void TestListenerConfig()
        {
            INamedCache cache = GetCache("local-listener");
            Assert.AreEqual(LocalCacheListenerStatic.Inserted, 0);
            Assert.AreEqual(LocalCacheListenerStatic.Updated, 0);
            Assert.AreEqual(LocalCacheListenerStatic.Deleted, 0);
            cache.Insert("key1", "value1");
            Assert.AreEqual(LocalCacheListenerStatic.Inserted, 1);
            Assert.AreEqual(LocalCacheListenerStatic.Updated, 0);
            Assert.AreEqual(LocalCacheListenerStatic.Deleted, 0);
            cache.Insert("key1", "value2");
            Assert.AreEqual(LocalCacheListenerStatic.Inserted, 1);
            Assert.AreEqual(LocalCacheListenerStatic.Updated, 1);
            Assert.AreEqual(LocalCacheListenerStatic.Deleted, 0);
            cache.Remove("key1");
            Assert.AreEqual(LocalCacheListenerStatic.Inserted, 1);
            Assert.AreEqual(LocalCacheListenerStatic.Updated, 1);
            Assert.AreEqual(LocalCacheListenerStatic.Deleted, 1);

            CacheFactory.Shutdown();
        }

        /// <summary>
        /// Test case for customer issue, read/write lock recursion
        [Test]
        public void TestBug21113841()
        {
            String testString = "This is selva local cache issue problem";

            // init local test cache
            INamedCache testCache = GetCache("local-Bug21113841-cache");
            testCache.Clear();

            try
            {
                for (int i = 0; i < 10; i++)
                {
                    Object[] array = testCache.GetValues(null);
                    if (array == null || array.Length == 0)
                    {
                        // Delay so cache flush will occur.
                        Thread.Sleep(250);
                        testCache.Add("1", testString);
                    }
                }

            }
            catch(Exception e)
            {
                Assert.Fail("Exception occurred getting local cache values: " + e.ToString());
            }
            finally
            {
                CacheFactory.Shutdown();
            }
        }

        [Test]
        public void TestGetOrDefault()
        {
            LocalCache localCache = new LocalCache();

            Hashtable ht = new Hashtable();
            ht.Add("key4", 0);
            ht.Add("key3", -10);
            ht.Add("key2", 45);
            ht.Add("key1", 398);
            localCache.InsertAll(ht);

            object result = localCache.GetOrDefault("key1", 400);
            Assert.AreEqual(398, result);

            result = localCache.GetOrDefault("key5", 400);
            Assert.AreEqual(400, result);
        }

        [Test]
        public void TestLocale()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            ccf.Config = config;

            var testMap = CacheFactory.GetCache("dist-locale");

            //test for number group separator for US locale
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string s1 = "5,555.55";
            decimal d1;
            Decimal.TryParse(s1, NumberStyles.Any, CultureInfo.CurrentCulture, out d1);
            testMap.Add("key1", d1);
            Assert.AreEqual(d1, testMap["key1"]);

            //test for number group separator for Turkish locale
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");
            string s2 = "5.555,55";
            decimal d2;
            Decimal.TryParse(s2, NumberStyles.Any, CultureInfo.CurrentCulture, out d2);
            testMap.Add("key2", d2);
            Assert.AreEqual(d2, testMap["key2"]);

            //test for number decimal separator for Turkish locale 
            string s3 = "5,55";
            decimal d3;
            Decimal.TryParse(s3, NumberStyles.Any, CultureInfo.CurrentCulture, out d3);
            testMap.Add("key3", d3);
            Assert.AreEqual(d3, testMap["key3"]);
        }

        [Test]
        public void TestPutIfAbsent()
        {
            LocalCache localCache = new LocalCache();

            Hashtable ht = new Hashtable();
            ht.Add("key4", 0);
            ht.Add("key3", -10);
            ht.Add("key2", 45);
            ht.Add("key1", 398);
            localCache.InsertAll(ht);

            object result = localCache.InsertIfAbsent("key1", 400);
            Assert.AreEqual(398, result);

            result = localCache.InsertIfAbsent("key5", 400);
            Assert.AreEqual(null, result);
            Assert.IsTrue(localCache.Contains("key5"));
        }

        [Test]
        public void TestRemove()
        {
            LocalCache localCache = new LocalCache();

            Hashtable ht = new Hashtable();
            ht.Add("key4", 0);
            ht.Add("key3", -10);
            ht.Add("key2", 45);
            ht.Add("key1", 398);
            localCache.InsertAll(ht);

            object result = localCache.Remove("key1", 398);
            Assert.AreEqual(true, result);
            Assert.AreEqual(3, localCache.Count);
        }

        [Test]
        public void TestReplace()
        {
            LocalCache localCache = new LocalCache();

            Hashtable ht = new Hashtable();
            ht.Add("key4", 0);
            ht.Add("key3", -10);
            ht.Add("key2", 45);
            ht.Add("key1", 398);
            localCache.InsertAll(ht);

            object result = localCache.Replace("key1", 400);
            Assert.AreEqual(398, result);
            Assert.AreEqual(400, localCache.GetEntry("key1").Value);

            result = localCache.Replace("key1", 300, 450);
            Assert.AreEqual(false, result);

            result = localCache.Replace("key1", 400, 450);
            Assert.AreEqual(true, result);
            Assert.AreEqual(450, localCache.GetEntry("key1").Value);
        }
    }

    #region Helper classes

    internal class LocalCacheLoader : ICacheLoader
    {
        public object Load(object key)
        {
            return key;
        }

        public IDictionary LoadAll(ICollection keys)
        {
            IDictionary ht = new Hashtable();
            foreach (object key in keys)
            {
                ht.Add(key, key);
            }
            return ht;
        }
    }

    internal class LocalCacheStore : ICacheStore
    {
        private readonly Hashtable ht = new Hashtable();

        public Hashtable StoreDictionary
        {
            get { return ht; }
        }

        public void Store(object key, object value)
        {
            ht[key] = value;
        }

        public void StoreAll(IDictionary dictionary)
        {
            foreach(DictionaryEntry entry in dictionary)
            {
                ht[entry.Key] = entry.Value;
            }
        }

        public void Erase(object key)
        {
            ht.Remove(key);
        }

        public void EraseAll(ICollection keys)
        {
            foreach(object key in keys)
            {
                ht.Remove(key);
            }
        }

        public object Load(object key)
        {
            return ht[key];
        }

        public IDictionary LoadAll(ICollection keys)
        {
            Hashtable newht = new Hashtable();
            foreach (object key in keys)
            {
                newht[key] = ht[key];
            }
            return newht;
        }
    }

    internal class Fixed2UnitCalculator : IUnitCalculator
    {
        public int CalculateUnits(object key, object value)
        {
            return 2;
        }

        public string Name
        {
            get { return GetType().Name; }
        }
    }

    internal class DummyEvictionPolicy : IEvictionPolicy
    {
        public void EntryTouched(IConfigurableCacheEntry entry)
        {}

        public void RequestEviction(long maximum)
        {}

        public string Name
        {
            get { return GetType().Name; }
        }
    }

    internal class LocalCacheListener : ICacheListener
    {
        private int inserted;
        private int updated;
        private int deleted;

        public int Inserted
        {
            get { return inserted; }
        }

        public int Updated
        {
            get { return updated; }
        }

        public int Deleted
        {
            get { return deleted; }
        }

        public void EntryInserted(CacheEventArgs evt)
        {
            inserted = Inserted + 1;
        }

        public void EntryUpdated(CacheEventArgs evt)
        {
            updated = Updated + 1;
        }

        public void EntryDeleted(CacheEventArgs evt)
        {
            deleted = Deleted + 1;
        }
    }

    internal class LocalCacheListenerStatic : ICacheListener
    {
        private static int inserted;
        private static int updated;
        private static int deleted;

        public static int Inserted
        {
            get { return inserted; }
        }

        public static int Updated
        {
            get { return updated; }
        }

        public static int Deleted
        {
            get { return deleted; }
        }

        public void EntryInserted(CacheEventArgs evt)
        {
            inserted = Inserted + 1;
        }

        public void EntryUpdated(CacheEventArgs evt)
        {
            updated = Updated + 1;
        }

        public void EntryDeleted(CacheEventArgs evt)
        {
            deleted = Deleted + 1;
        }
    }

    internal class LocalCacheAccessor
    {
        public LocalCacheAccessor(string name, LocalCache cache)
        {
            m_name  = name;
            m_cache = cache;
        }

        public void Run()
        {
            LocalCache cache = m_cache;
            Random     rnd   = new Random();

            try
            {
                for (LocalCacheOperation op = LocalCacheOperation.CopyTo;
                     op <= LocalCacheOperation.Aggregate;
                     ++op)
                {
                    if (cache.Count < 25)
                    {
                        for (int i = 0; i < 100; ++i)
                        {
                            cache[i] = i;
                        }
                    }
                    for (int i = 0, c = rnd.Next(100); i < c; ++i)
                    {
                        switch (op)
                        {
                            case LocalCacheOperation.CopyTo:
                            {
                                cache.CopyTo(new Object[200], 0);
                                break;
                            }
                            case LocalCacheOperation.Count:
                            {
                                if (cache.Count < 0)
                                {
                                    Assert.Fail("Count");
                                }
                                break;
                            }
                            case LocalCacheOperation.IsSynchronized:
                            {
                                if (!cache.IsSynchronized)
                                {
                                    Assert.Fail("IsSynchronized");
                                }
                                break;
                            }
                            case LocalCacheOperation.SyncRoot:
                            {
                                if (cache.SyncRoot == null)
                                {
                                    Assert.Fail("SyncRoot");
                                }
                                break;
                            }
                            case LocalCacheOperation.GetEnumerator:
                            {
                                if (cache.GetEnumerator() == null)
                                {
                                    Assert.Fail("GetEnumerator");
                                }
                                break;
                            }
                            case LocalCacheOperation.Add:
                            {
                                cache.Add(m_name + op + i, m_name + op + i);
                                break;
                            }
                            case LocalCacheOperation.Clear:
                            {
                                cache.Clear();
                                break;
                            }
                            case LocalCacheOperation.Contains:
                            {
                                if (cache.Contains("this is an invalid key"))
                                {
                                    Assert.Fail("Contains");
                                }
                                break;
                            }
                            case LocalCacheOperation.Remove:
                            {
                                cache.Remove(rnd.Next(100));
                                break;
                            }
                            case LocalCacheOperation.Indexer:
                            {
                                cache[rnd.Next(100)] = rnd.Next(100);
                                break;
                            }
                            case LocalCacheOperation.Keys:
                            {
                                if (cache.Keys == null)
                                {
                                    Assert.Fail("Keys");
                                }
                                break;
                            }
                            case LocalCacheOperation.Values:
                            {
                                if (cache.Values == null)
                                {
                                    Assert.Fail("Values");
                                }
                                break;
                            }
                            case LocalCacheOperation.IsReadOnly:
                            {
                                if (cache.IsReadOnly)
                                {
                                    Assert.Fail("IsReadOnly");    
                                }
                                break;
                            }
                            case LocalCacheOperation.IsFixedSize:
                            {
                                if (cache.IsFixedSize)
                                {
                                    Assert.Fail("IsReadOnly");
                                }
                                break;
                            }
                            case LocalCacheOperation.Entries:
                            {
                                if (cache.Entries == null)
                                {
                                    Assert.Fail("Entries");
                                }
                                break;
                            }
                            case LocalCacheOperation.GetAll:
                            {
                                int[] an = new int[100];
                                for (int j = 0; j < 100; ++j)
                                {
                                    an[j] = j;
                                }
                                if (cache.GetAll(an) == null)
                                {
                                    Assert.Fail("GetAll");
                                }
                                break;
                            }
                            case LocalCacheOperation.Insert:
                            {
                                cache.Insert(m_name + op + i, m_name + op + i);
                                break;
                            }
                            case LocalCacheOperation.InsertAll:
                            {
                                IDictionary dictionary = new HashDictionary();
                                for (int j = 0; j < 100; ++j)
                                {
                                    string s = m_name + op + i + "_" + j;
                                    dictionary[s] = s;
                                }
                                cache.InsertAll(dictionary);
                                break;
                            }
                            case LocalCacheOperation.Units:
                            {
                                if (cache.Units < 0)
                                {
                                    Assert.Fail("Units");
                                }
                                break;
                            }
                            case LocalCacheOperation.LowUnits:
                            {
                                if (cache.LowUnits < 0)
                                {
                                    Assert.Fail("LowUnits");
                                }
                                cache.LowUnits = cache.LowUnits;
                                break;
                            }
                            case LocalCacheOperation.HighUnits:
                            {
                                if (cache.HighUnits < 0)
                                {
                                    Assert.Fail("HighUnits");
                                }
                                cache.HighUnits = cache.HighUnits;
                                break;
                            }
                            case LocalCacheOperation.ExpiryDelay:
                            {
                                if (cache.ExpiryDelay < 0)
                                {
                                    Assert.Fail("ExpiryDelay");
                                }
                                cache.ExpiryDelay = cache.ExpiryDelay;
                                break;
                            }
                            case LocalCacheOperation.FlushDelay:
                            {
                                if (cache.FlushDelay < 0)
                                {
                                    Assert.Fail("FlushDelay");
                                }
                                cache.FlushDelay = cache.FlushDelay;
                                break;
                            }
                            case LocalCacheOperation.UnitCalculator:
                            {
                                cache.UnitCalculator = cache.UnitCalculator;
                                break;
                            }
                            case LocalCacheOperation.EvictionPolicy:
                            {
                                cache.EvictionPolicy = cache.EvictionPolicy;
                                break;
                            }
                            case LocalCacheOperation.GetCacheEntry:
                            {
                                cache.GetCacheEntry(rnd.Next(100));
                                break;
                            }
                            case LocalCacheOperation.Evict:
                            {
                                cache.Evict(rnd.Next(100));
                                break;
                            }
                            case LocalCacheOperation.EvictAll:
                            {
                                int[] an = new int[100];
                                for (int j = 0; j < 100; ++j)
                                {
                                    an[j] = j;
                                }
                                cache.EvictAll(an);
                                break;
                            }
                            case LocalCacheOperation.Lock:
                            {
                                int n = rnd.Next(100);
                                if (!cache.Lock(n, -1))
                                {
                                    Assert.Fail("Lock");
                                }
                                if (!cache.Unlock(n))
                                {
                                    Assert.Fail("Unlock");
                                }
                                break;
                            }
                            case LocalCacheOperation.GetKeys:
                            {
                                if (cache.GetKeys(AlwaysFilter.Instance) == null)
                                {
                                    Assert.Fail("GetKeys");
                                }
                                break;
                            }
                            case LocalCacheOperation.GetValues:
                            {
                                if (cache.GetValues(AlwaysFilter.Instance) == null)
                                {
                                    Assert.Fail("GetValues");
                                }
                                break;
                            }
                            case LocalCacheOperation.GetEntries:
                            {
                                if (cache.GetEntries(AlwaysFilter.Instance) == null)
                                {
                                    Assert.Fail("GetEntries");
                                }
                                break;
                            }
                            case LocalCacheOperation.Invoke:
                            {
                                cache.Invoke(rnd.Next(100), 
                                    new ConditionalRemove(AlwaysFilter.Instance));
                                break;
                            }
                            case LocalCacheOperation.InvokeAll:
                            {
                                int[] an = new int[100];
                                for (int j = 0; j < 100; ++j)
                                {
                                    an[j] = j;
                                }
                                cache.InvokeAll(an, 
                                    new ConditionalRemove(AlwaysFilter.Instance));
                                cache.InvokeAll(AlwaysFilter.Instance, 
                                    new ConditionalRemove(AlwaysFilter.Instance));
                                break;
                            }
                            case LocalCacheOperation.Aggregate:
                            {
                                int[] an = new int[100];
                                for (int j = 0; j < 100; ++j)
                                {
                                    an[j] = j;
                                }
                                cache.Aggregate(an, new Count());
                                cache.Aggregate(AlwaysFilter.Instance, new Count());
                                break;
                            }
                            default:
                            {
                                throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }

                m_success = true;
            }
            catch (Exception e)
            {
                m_exception = e;
            }
        }

        public bool IsSuccess
        {
            get { return m_success; }
        }

        public Exception Error
        {
            get { return m_exception; }
        }

        private readonly string m_name;
        private readonly LocalCache m_cache;
        private bool m_success;
        private Exception m_exception;
    }

    #endregion

    #region Helper enumerations

    /// <summary>
    /// LocalCache operations enum.
    /// </summary>
    public enum LocalCacheOperation
    {
        CopyTo = 0,
        Count = 1,
        IsSynchronized = 2,
        SyncRoot = 3,
        GetEnumerator = 4,
        Add = 5,
        Clear = 6,
        Contains = 7,
        Remove = 8,
        Indexer = 9,
        Keys = 10,
        Values = 11,
        IsReadOnly = 12,
        IsFixedSize = 13,
        Entries = 14,
        GetAll = 15,
        Insert = 16,
        InsertAll = 17,
        Units = 18,
        LowUnits = 19,
        HighUnits = 20,
        ExpiryDelay = 21,
        FlushDelay = 22,
        UnitCalculator = 23,
        EvictionPolicy = 24,
        GetCacheEntry = 25,
        Evict = 26,
        EvictAll = 27,
        Lock = 28,
        GetKeys = 29,
        GetValues = 30,
        GetEntries = 31,
        Invoke = 32,
        InvokeAll = 33,
        Aggregate = 34
    }

    #endregion
}
