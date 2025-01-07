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

using Tangosol;
using Tangosol.Net.Impl;
using Tangosol.Net.Internal;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Aggregator;
using Tangosol.Util.Comparator;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;
using Tangosol.Util.Processor;
using Tangosol.Util.Transformer;

namespace Tangosol.Net.Cache
{
    [TestFixture]
    public class ContinuousQueryCacheTests
    {
        private INamedCache GetCache(String cacheName)
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            return CacheFactory.GetCache(cacheName);
        }

        [Test]
        public void TestInitialization()
        {
            INamedCache namedCache = GetCache("local-default");

            Hashtable ht = new Hashtable();
            ht.Add("Key1", 435);
            ht.Add("Key2", 253);
            ht.Add("Key3", 3);
            ht.Add("Key4", 200);
            ht.Add("Key5", 333);
            namedCache.InsertAll(ht);

            SyncListener listener = new SyncListener();
            IFilter greaterThan300 = new GreaterFilter(IdentityExtractor.Instance, 300);

            ContinuousQueryCache queryCache = new ContinuousQueryCache(namedCache, greaterThan300);
            Assert.AreEqual(2, queryCache.Count);

            queryCache = new ContinuousQueryCache(namedCache, greaterThan300, true);
            Assert.AreEqual(2, queryCache.Count);

            queryCache = new ContinuousQueryCache(namedCache, greaterThan300, listener);
            Assert.AreEqual(2, queryCache.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestINamedCache()
        {
            INamedCache namedCache = GetCache("local-default");

            Hashtable ht = new Hashtable();
            ht.Add("Key1", 435);
            ht.Add("Key2", 253);
            ht.Add("Key3", 3);
            ht.Add("Key4", 200);
            ht.Add("Key5", 333);
            namedCache.InsertAll(ht);

            IFilter greaterThan300 = new GreaterFilter(IdentityExtractor.Instance, 300);
            ContinuousQueryCache queryCache = new ContinuousQueryCache(namedCache, greaterThan300);

            // testing CacheName property
            Assert.IsNotNull(queryCache.CacheName);
            Assert.IsTrue(queryCache.CacheName.StartsWith("ContinuousQueryCache"));

            // testing CacheService property
            Assert.AreEqual(namedCache.CacheService, queryCache.CacheService);

            // testing IsActive property
            Assert.IsTrue(queryCache.IsActive);

            // testing Destroy and Release (implicitly) methods
            queryCache.Destroy();

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestIDictionary()
        {
            INamedCache namedCache = GetCache("local-default");

            Hashtable ht = new Hashtable();
            ht.Add("Key1", 435);
            ht.Add("Key2", 253);
            ht.Add("Key3", 3);
            ht.Add("Key4", 200);
            ht.Add("Key5", 333);
            namedCache.InsertAll(ht);

            IFilter greaterThan300 = new GreaterFilter(IdentityExtractor.Instance, 300);
            ContinuousQueryCache queryCache = new ContinuousQueryCache(namedCache, greaterThan300);

            // testing count property
            Assert.AreEqual(2, queryCache.Count);
            queryCache.Clear();
            Assert.AreEqual(0, queryCache.Count);

            namedCache.InsertAll(ht);
            Assert.AreEqual(2, queryCache.Count);

            // testing remove method
            queryCache.Remove("Key5");
            Assert.AreEqual(1, queryCache.Count);
            Assert.IsNotNull(queryCache["Key1"]);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestExtractorEventTransformer()
        {
            //testing on local cache
            INamedCache cache = GetCache("local-default");
            cache.Clear();

            Hashtable ht = new Hashtable();
            Address address1 = new Address("Street1", "City1", "State1", "Zip1");
            Address address2 = new Address("Street2", "City2", "State2", "Zip2");
            ht.Add("key1", address1);
            ht.Add("key2", address2);
            cache.InsertAll(ht);

            SyncListener listener = new SyncListener();
            IFilter filter = new ValueChangeEventFilter("Street");
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
        public void TestIObservableCache()
        {
            INamedCache namedCache = GetCache("local-default");

            Hashtable ht = new Hashtable();
            ht.Add("Key1", 435);
            ht.Add("Key2", 253);
            ht.Add("Key3", 3);
            ht.Add("Key4", 200);
            ht.Add("Key5", 333);
            namedCache.InsertAll(ht);

            IFilter greaterThan300 = new GreaterFilter(IdentityExtractor.Instance, 300);
            IFilter listenerFilter = new CacheEventFilter(new GreaterFilter(IdentityExtractor.Instance, 350));
            ContinuousQueryCache queryCache = new ContinuousQueryCache(namedCache, greaterThan300);
            SyncListener listener = new SyncListener();

            // listener
            queryCache.AddCacheListener(listener);

            listener.CacheEvent = null;
            queryCache.Insert("Key7", 400);
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Inserted, listener.CacheEvent.EventType);

            listener.CacheEvent = null;
            queryCache.Insert("Key7", 350);
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Updated, listener.CacheEvent.EventType);

            listener.CacheEvent = null;
            queryCache.Remove("Key5");
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Deleted, listener.CacheEvent.EventType);

            queryCache.RemoveCacheListener(listener);

            // listener, key, lite
            namedCache.Clear();
            namedCache.InsertAll(ht);
            queryCache.AddCacheListener(listener, "Key5", false);

            listener.CacheEvent = null;
            queryCache.Insert("Key6", 400);
            Assert.IsNull(listener.CacheEvent);

            queryCache.Insert("Key5", 400);
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Updated, listener.CacheEvent.EventType);

            listener.CacheEvent = null;
            queryCache.Remove("Key1");
            Assert.IsNull(listener.CacheEvent);

            queryCache.Remove("Key5");
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Deleted, listener.CacheEvent.EventType);

            queryCache.RemoveCacheListener(listener, "Key5");

            // listener, filter, lite
            namedCache.Clear();
            namedCache.InsertAll(ht);
            queryCache.AddCacheListener(listener, listenerFilter, false);

            listener.CacheEvent = null;
            queryCache.Insert("Key6", 320);
            Assert.IsNull(listener.CacheEvent);

            queryCache.Insert("Key5", 350);
            Assert.IsNull(listener.CacheEvent);

            queryCache.Insert("Key6", 400);
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Updated, listener.CacheEvent.EventType);

            listener.CacheEvent = null;
            queryCache.Insert("Key7", 340);
            Assert.IsNull(listener.CacheEvent);

            queryCache.Remove("Key7");
            Assert.IsNull(listener.CacheEvent);

            queryCache.Remove("Key6");
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Deleted, listener.CacheEvent.EventType);

            queryCache.RemoveCacheListener(listener, listenerFilter);

            CacheFactory.Shutdown();
        }

        [Test]
        public void ContinuousQueryCacheInsertTest()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 500);
            ContinuousQueryCache cache = new ContinuousQueryCache(remotecache, filter);
            Assert.IsNotNull(cache);
            Assert.IsInstanceOf(typeof(SafeNamedCache), cache.Cache);
            Assert.IsInstanceOf(typeof(LessFilter), cache.Filter);

            Hashtable ht = new Hashtable();
            ht.Add("key1", 111);
            ht.Add("key2", 222);
            ht.Add("key3", 333);
            ht.Add("key4", 444);

            // local cache gets updated automaticly
            cache.InsertAll(ht);
            Assert.AreEqual(4, cache.Count);
            Assert.IsTrue(cache.Contains("key2"));

            // underlaying cache gets updated with the entry
            // that does not satisfies filter
            // event is not raised and local cache is not updated
            cache.Cache.Insert("key100", 10001);
            Assert.AreEqual(4, cache.Count);
            Assert.IsFalse(cache.Contains("key100"));


            cache.Insert("key5", 1);
            Assert.AreEqual(1, cache["key5"]);

            cache.Insert("key4", 499);
            Assert.AreEqual(499, cache["key4"]);

            cache.Cache.Remove("key5");
            Assert.IsFalse(cache.Contains("key5"));

            // With the latest coherence.jar we receive the delete
            // event due to expiry as soon as it happened instead of
            // when the next cache operation occurrs.  So, change
            // the test accordingly.
            cache.Insert("key6", 173, 400);
            Blocking.Sleep(100);
            Assert.AreNotEqual(null, cache["key6"]);
            Thread.Sleep(600);
            Assert.AreEqual(null, cache.Cache["key6"]);

            // updating a cache with the value that does not satisfies
            // the filter criteria is not permited
            bool permited = true;
            try
            {
                cache.Insert("key100", 1001);
            }
            catch (Exception)
            {
                permited = false;
            }
            Assert.IsFalse(permited);

            Assert.IsTrue(cache.CacheValues);
            Assert.IsFalse(cache.IsReadOnly);

            CacheFactory.Shutdown();
        }

        [Test]
        public void ContinuousQueryCacheGetTest()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 500);
            ContinuousQueryCache cache = new ContinuousQueryCache(remotecache, filter);
            Assert.IsNotNull(cache);
            Assert.IsInstanceOf(typeof(SafeNamedCache), cache.Cache);
            Assert.IsInstanceOf(typeof(LessFilter), cache.Filter);

            Hashtable ht = new Hashtable();
            ht.Add("key1", 111);
            ht.Add("key2", 222);
            ht.Add("key3", 333);
            ht.Add("key4", 444);

            // local cache gets updated automaticly
            cache.InsertAll(ht);
            Assert.AreEqual(111, cache["key1"]);
            Assert.AreEqual(null, cache["dummy"]);

            ICollection entries = cache.GetAll(cache.Keys);
            foreach (DictionaryEntry entry in (IDictionary) entries)
            {
                Assert.IsTrue(ht.Contains(entry.Key));
            }

            entries = cache.Entries;
            Assert.AreEqual(4, entries.Count);

            ArrayList values = new ArrayList(cache.Values);
            foreach (object value in ht.Values)
            {
                values.Remove(value);
            }
            Assert.AreEqual(0, values.Count);

            ArrayList keys = new ArrayList(cache.Keys);
            foreach (object key in ht.Keys)
            {
                keys.Remove(key);
            }
            Assert.AreEqual(0, keys.Count);

            cache.Clear();
            cache.CacheValues = false;
            cache.Insert("key5", 1);
            entries = cache.GetAll(cache.Keys);
            Assert.AreEqual(1, entries.Count);
            Assert.IsInstanceOf(typeof(IDictionary), entries);
            Assert.AreEqual(1, ((IDictionary) entries)["key5"]);

            cache.InsertAll(ht);
            entries = cache.GetAll(cache.Keys);
            Assert.AreEqual(5, entries.Count);
            Assert.IsInstanceOf(typeof(IDictionary), entries);
            IDictionary entriesDict = entries as IDictionary;
            Assert.IsNotNull(entriesDict);
            Assert.AreEqual(111, entriesDict["key1"]);

            CacheFactory.Shutdown();
        }

        [Test]
        public void ContinuousQueryCacheNotCachingValuesTest()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 500);
            ContinuousQueryCache cache = new ContinuousQueryCache(remotecache, filter, false);
            Assert.IsNotNull(cache);
            Assert.IsInstanceOf(typeof(SafeNamedCache), cache.Cache);
            Assert.IsInstanceOf(typeof(LessFilter), cache.Filter);

            Hashtable ht = new Hashtable();
            ht.Add("key1", 111);
            ht.Add("key2", 222);
            ht.Add("key3", 333);
            ht.Add("key4", 444);

            // local cache gets updated automaticly
            cache.InsertAll(ht);
            Assert.AreEqual(ht["key1"], cache["key1"]);
            Assert.AreEqual(ht["key2"], cache["key2"]);
            Assert.AreEqual(ht["key3"], cache["key3"]);
            Assert.AreEqual(ht["key4"], cache["key4"]);

            CacheFactory.Shutdown();
        }

        [Test]
        public void ContinuousQueryCacheIQueryTest()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 500);
            ContinuousQueryCache cache = new ContinuousQueryCache(remotecache, filter);
            Assert.IsNotNull(cache);
            Assert.IsInstanceOf(typeof(SafeNamedCache), cache.Cache);
            Assert.IsInstanceOf(typeof(LessFilter), cache.Filter);

            Hashtable ht = new Hashtable();
            ht.Add("key1", 111);
            ht.Add("key2", 222);
            ht.Add("key3", 333);
            ht.Add("key4", 444);
            cache.InsertAll(ht);

            // Do twice, once without an index and once with.
            for (int i = 0; i < 2; ++i )
            {
                ICollection keys = cache.GetKeys(new GreaterFilter(IdentityExtractor.Instance, 200));
                Assert.AreEqual(3, keys.Count);
                ArrayList keysList = new ArrayList(keys);
                Assert.IsFalse(keysList.Contains("key1"));

                ICollection values = cache.GetValues(new GreaterFilter(IdentityExtractor.Instance, 200));
                Assert.AreEqual(3, values.Count);
                ArrayList valuesList = new ArrayList(values);
                Assert.IsFalse(valuesList.Contains("key1"));

                values =
                        cache.GetValues(new GreaterFilter(IdentityExtractor.Instance, 200),
                                        new SafeComparer());
                Assert.AreEqual(3, values.Count);
                valuesList = new ArrayList(values);
                Assert.IsFalse(valuesList.Contains("key1"));

                values =
                        cache.GetValues(new GreaterFilter(IdentityExtractor.Instance, 200),
                                        new InverseComparer(new SafeComparer()));
                Assert.AreEqual(3, values.Count);

                ICollection entries = cache.GetEntries(new GreaterFilter(IdentityExtractor.Instance, 200));
                Assert.AreEqual(3, entries.Count);
                Assert.IsInstanceOf(typeof(ICacheEntry[]), entries);

                entries = cache.GetEntries(new GreaterFilter(IdentityExtractor.Instance, 200), new SafeComparer());
                Assert.AreEqual(3, entries.Count);

                // add index here for the second pass
                cache.AddIndex(IdentityExtractor.Instance, true, SafeComparer.Instance);
            }
            cache.RemoveIndex(IdentityExtractor.Instance);

            CacheFactory.Shutdown();
        }

        [Test]
        public void ContinuousQueryCacheIQueryNoValuesTest()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 500);
            ContinuousQueryCache cache = new ContinuousQueryCache(remotecache, filter, false);
            Assert.IsNotNull(cache);
            Assert.IsInstanceOf(typeof(SafeNamedCache), cache.Cache);
            Assert.IsInstanceOf(typeof(LessFilter), cache.Filter);

            Hashtable ht = new Hashtable();
            ht.Add("key1", 111);
            ht.Add("key2", 222);
            ht.Add("key3", 333);
            ht.Add("key4", 444);
            cache.InsertAll(ht);

            ICollection keys = cache.GetKeys(new GreaterFilter(IdentityExtractor.Instance, 200));
            Assert.AreEqual(3, keys.Count);
            ArrayList keysList = new ArrayList(keys);
            Assert.IsFalse(keysList.Contains("key1"));

            ICollection values = cache.GetValues(new GreaterFilter(IdentityExtractor.Instance, 200));
            Assert.AreEqual(3, values.Count);
            ArrayList valuesList = new ArrayList(values);
            Assert.IsFalse(valuesList.Contains("key1"));

            ICollection entries = cache.GetEntries(new GreaterFilter(IdentityExtractor.Instance, 200));
            Assert.AreEqual(3, entries.Count);
            Assert.IsInstanceOf(typeof(ICacheEntry[]), entries);

            CacheFactory.Shutdown();
        }

        [Test]
        public void CQCICacheMethodsWithValuesTest()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 50);

            for (int i = 0; i < 100; i++)
            {
                remotecache.Insert("key" + i, i);
            }

            ContinuousQueryCache cache = new ContinuousQueryCache(remotecache, filter);
            Assert.IsNotNull(cache);
            Assert.IsInstanceOf(typeof(SafeNamedCache), cache.Cache);
            Assert.IsInstanceOf(typeof(LessFilter), cache.Filter);

            Assert.AreEqual(50, cache.Count);
            cache.Cache.Insert("key101", 101);
            Assert.AreEqual(50, cache.Count);
            Assert.IsFalse(cache.Contains("key101"));

            object o = cache.Insert("key5", 1);
            Assert.AreEqual(5, o);
            Assert.AreEqual(1, cache["key5"]);

            cache.Remove("key5");
            Assert.IsFalse(cache.Contains("key5"));

            IFilter removefilter = new LessFilter(IdentityExtractor.Instance, 30);
            ICollection removecoll = cache.GetKeys(removefilter);
            cache.InvokeAll(removecoll, new ConditionalRemove(removefilter, false));

            Assert.IsNull(cache["key1"]);
            Hashtable entries = new Hashtable(20);
            for (int i = 0; i < 20; i++)
            {
                entries["key" + i] = i;
            }
            cache.InsertAll(entries);

            ICollection coll = cache.GetAll(entries.Keys);
            Assert.AreEqual(20, coll.Count);

            cache.State = ContinuousQueryCache.CacheState.Disconnected;
            Assert.AreEqual(40, cache.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void CQCICacheMethodsWithoutValuesTest()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 50);

            for (int i = 0; i < 100; i++)
            {
                remotecache.Insert("key" + i, i);
            }

            ContinuousQueryCache cache = new ContinuousQueryCache(remotecache, filter, false);
            Assert.IsNotNull(cache);
            Assert.IsInstanceOf(typeof(SafeNamedCache), cache.Cache);
            Assert.IsInstanceOf(typeof(LessFilter), cache.Filter);

            object o = cache.Insert("key5", 1);
            Assert.AreEqual(5, o);
            Assert.AreEqual(1, cache["key5"]);

            cache.Remove("key5");
            Assert.IsFalse(cache.Contains("key5"));

            IFilter removefilter = new LessFilter(IdentityExtractor.Instance, 30);
            ICollection removecoll = cache.GetKeys(removefilter);
            cache.InvokeAll(removecoll, new ConditionalRemove(removefilter, false));

            Assert.IsNull(cache["key1"]);
            Hashtable entries = new Hashtable(20);
            for (int i = 0; i < 20; i++)
            {
                entries["key" + i] = i;
            }
            cache.InsertAll(entries);

            ICollection coll = cache.GetAll(entries.Keys);
            Assert.AreEqual(20, coll.Count);

            Assert.IsFalse(cache.CacheValues);
            cache.CacheValues = true;
            Assert.AreEqual(0, cache["key0"]);
            Assert.IsTrue(cache.CacheValues);

            CacheFactory.Shutdown();
        }

        [Test]
        public void ContinuousQueryCacheIsReadOnlyTest()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 500);
            ContinuousQueryCache cache = new ContinuousQueryCache(remotecache, filter, false);

            try
            {
                Assert.IsFalse(cache.IsReadOnly);
                cache.IsReadOnly = true;
                Assert.IsTrue(cache.IsReadOnly);
                cache.IsReadOnly = true;
                Assert.That(() => cache.IsReadOnly = false, Throws.InvalidOperationException);
            }
            finally
            {
                CacheFactory.Shutdown();
            }

        }

        [Test]
        public void ContinuousQueryCacheIsCacheValuesTest()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 500);
            ContinuousQueryCache cache = new ContinuousQueryCache(remotecache, filter);

            Hashtable ht = new Hashtable();
            ht.Add("key1", 111);
            ht.Add("key2", 222);
            ht.Add("key3", 333);
            ht.Add("key4", 444);
            cache.InsertAll(ht);

            Assert.IsTrue(cache.CacheValues);
            cache.CacheValues = false;
            Assert.IsFalse(cache.CacheValues);
            cache.CacheValues = false;
            CacheFactory.Shutdown();
        }

        [Test]
        public void ContinuousQueryCacheInvocationTest()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 500);
            ContinuousQueryCache cache = new ContinuousQueryCache(remotecache, filter);
            Assert.IsNotNull(cache);
            Assert.IsInstanceOf(typeof(SafeNamedCache), cache.Cache);
            Assert.IsInstanceOf(typeof(LessFilter), cache.Filter);

            Hashtable ht = new Hashtable();
            ht.Add("key1", 111);
            ht.Add("key2", 222);
            ht.Add("key3", 333);
            ht.Add("key4", 444);
            cache.InsertAll(ht);

            cache.Cache.Insert("key5", 555);
            cache.Cache.Insert("key6", 666);
            cache.Cache.Insert("key7", 777);

            object result = cache["key5"];
            Assert.IsNull(result);

            IEntryProcessor agent =
                    new ConditionalRemove(new GreaterFilter(IdentityExtractor.Instance, 200));
            cache.Invoke("key4", agent);
            Assert.IsNull(cache["key4"]);

            cache.Invoke("key1", agent);
            Assert.IsNotNull(cache["key1"]);

            cache.InvokeAll(ht.Keys, agent);
            Assert.AreEqual(111,  cache["key1"]);
            Assert.AreEqual(null, cache["key2"]);
            Assert.AreEqual(null, cache["key3"]);
            Assert.AreEqual(null, cache["key4"]);

            cache.InsertAll(ht);

            cache.InvokeAll(AlwaysFilter.Instance, agent);
            Assert.AreEqual(111, cache["key1"]);
            Assert.AreEqual(null, cache["key2"]);
            Assert.AreEqual(null, cache["key3"]);
            Assert.AreEqual(null, cache["key4"]);

            Assert.AreEqual(1, cache.Count);

            IDictionary dict = cache.InvokeAll(new ArrayList(), agent);
            Assert.IsTrue(dict.Count == 0);

            cache.InsertAll(ht);

            IEntryAggregator aggregator = new Count();
            result = cache.Aggregate(ht.Keys, aggregator);
            Assert.AreEqual(result, cache.Count);

            result = cache.Aggregate(new ArrayList(), aggregator);
            Assert.IsInstanceOf(typeof(IDictionary), result);
            dict = result as IDictionary;
            Assert.IsNotNull(dict);
            Assert.IsTrue(dict.Count == 0);

            result = cache.Aggregate(new GreaterFilter(IdentityExtractor.Instance, 200), aggregator);
            Assert.AreEqual(3, result);

            cache.AddIndex(IdentityExtractor.Instance, true, SafeComparer.Instance);
            cache.RemoveIndex(IdentityExtractor.Instance);

            CacheFactory.Shutdown();
        }

        [Test]
        public void ContinuousQueryCacheBadConstructorsTest1()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 500);
            try
            {
                Assert.That(() => new ContinuousQueryCache((INamedCache) null, filter), Throws.ArgumentNullException);
            }
            finally
            {
                CacheFactory.Shutdown();
            }
        }

        [Test]
        public void ContinuousQueryCacheBadConstructorsTest2()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            try
            {
                Assert.That(() => new ContinuousQueryCache(remotecache, null), Throws.ArgumentNullException);
            }
            finally
            {
                CacheFactory.Shutdown();
            }
        }

        [Test]
        public void TestCQCDispose()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();
            IFilter filter = new LessFilter(IdentityExtractor.Instance, 50);

            ContinuousQueryCache cache;
            using(cache = new ContinuousQueryCache(remotecache, filter, false))
            {
                cache.Clear();
                Assert.IsTrue(cache.Count == 0);
            }
            //after disposal CQC is not active, but
            //underlaying cache is not destroyed
            Assert.IsFalse(cache.IsActive);
            Assert.IsTrue(remotecache.IsActive);
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestCQCGetAllWithSameKeys()
        {
            INamedCache remotecache = CacheFactory.GetCache("dist-extend-direct");
            remotecache.Clear();

            IFilter filter = new LessFilter(IdentityExtractor.Instance, 100);
            ContinuousQueryCache cache = new ContinuousQueryCache(remotecache, filter, true);
            cache.Clear();

            IDictionary dict = new Hashtable();
            for (int i = 0; i < 10; i++)
            {
                dict.Add("key" + i, i);
            }
            cache.InsertAll(dict);
            string[] keys = { "key1", "key1", "key1", "key3" };
            IDictionary result = cache.GetAll(keys);
            Assert.AreEqual(2, result.Count);

            cache = new ContinuousQueryCache(remotecache, filter, false);
            string[] keys2 = {"key1", "key1", "key1", "key2", "key3", "key20"};
            result = cache.GetAll(keys2);
            Assert.AreEqual(3, result.Count);

            cache.Release();
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestCoh2532()
        {
            INamedCache cache = CacheFactory.GetCache("dist-extend-direct");
            cache.Clear();

            cache.Add(1, new Address("111 Main St", "Burlington", "MA", "01803"));
            cache.Add(2, new Address("222 Main St", "Lutz", "FL", "33549"));

            ContinuousQueryCache cqc = 
                new ContinuousQueryCache(cache, AlwaysFilter.Instance, 
                    new ReflectionExtractor("getCity"));
            Assert.IsTrue(cqc.IsReadOnly);

            Assert.AreEqual("Burlington", cqc[1]);
            Assert.AreEqual("Lutz", cqc[2]);

            cache.Add(3, new Address("333 Main St", "Belgrade", "Serbia", "11000"));
            Assert.AreEqual("Belgrade", cqc[3]);
            cache.Add(3, new Address("333 Main St", "Beograd", "Srbija", "11000"));
            Assert.AreEqual("Beograd", cqc[3]);

            Console.Out.WriteLine(cqc);
        }

        [Test]
        public void TestLiteListener()
        {
            INamedCache remoteCache = GetAndPopulateNamedCache("dist-extend-direct");
            
            ValidateLiteListener listener = new ValidateLiteListener(NUM_INSERTS); 
            ContinuousQueryCache theCQC   = 
                new ContinuousQueryCache(() => remoteCache, AlwaysFilter.Instance, 
                    false, listener, null);
            
            Assert.IsFalse(theCQC.CacheValues);
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(() => listener.GetActualTotal(), Is.EqualTo(NUM_INSERTS).After(500, 50));    
            Assert.That(theCQC.IsActive, Is.True);
        }
        
        [Test]
        public void TestLiteListenerToObservable()
        {
            INamedCache remoteCache = GetAndPopulateNamedCache("dist-extend-direct");
            
            ValidateLiteListener listener = new ValidateLiteListener(NUM_INSERTS); 
            ContinuousQueryCache theCQC   = 
                new ContinuousQueryCache(() => remoteCache, AlwaysFilter.Instance, false, 
                    listener, null);
            
            Assert.IsFalse(theCQC.CacheValues);
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(() => listener.GetActualTotal(), Is.EqualTo(NUM_INSERTS).After(500, 50));    
            Assert.That(theCQC.IsActive, Is.True);
            
            // add standard (non-lite) listener and validate that CacheValues is overriden to true.
            bool         isLite           = false;
            SyncListener listenerStandard = new SyncListener();

            theCQC.AddCacheListener(listenerStandard, AlwaysFilter.Instance, isLite);
            Assert.IsTrue(theCQC.CacheValues); 
        }        
        
        [Test]
        public void TestTransformerNoCacheValues()
        {
            INamedCache cache = CacheFactory.GetCache("dist-extend-direct");
            cache.Clear();

            cache.Add(1, new Address("111 Main St", "Burlington", "MA", "01803"));
            cache.Add(2, new Address("222 Main St", "Lutz", "FL", "33549"));
           
            TestCacheListener listener  = new TestCacheListener();
            IValueExtractor transformer = new UniversalExtractor("City");
            ContinuousQueryCache cqc      = 
                new ContinuousQueryCache(() => cache, AlwaysFilter.Instance, false, 
                    listener, transformer);
            
            // assert that cacheValues of false is overridden by non-null Transformer.
            Assert.IsTrue(cqc.IsReadOnly);
            Assert.IsTrue(cqc.CacheValues);

            Assert.AreEqual("Burlington", cqc[1]);
            Assert.AreEqual("Lutz", cqc[2]);

            cache.Add(3, new Address("333 Main St", "Belgrade", "Serbia", "11000"));
            Assert.AreEqual("Belgrade", cqc[3]);
            cache.Add(3, new Address("333 Main St", "Beograd", "Srbija", "11000"));
            Assert.AreEqual("Beograd", cqc[3]);

            Console.Out.WriteLine(cqc);
        }

        [Test]
        public void TestCoh10013()
        {
            INamedCache cache = CacheFactory.GetCache("dist-extend-direct");
            cache.Clear();

            TestContact t1 = new TestContact("John", "Loehe", 
                 new ExampleAddress("675 Beacon St.", "", "Dthaba", "SC", "91666", "USA"));
            cache.Add(1, t1);

            TestContact t2 = new TestContact("John", "Sydqtiinz", 
                 new ExampleAddress("336 Beacon St.", "", "Wltowuixs", "MA", "00595", "USA"));
            cache.Add(2, t2);

            FilterFactory ff = new FilterFactory("RemoteInvocationService");

            // Find all contacts who live in Massachusetts - direct cache access, no filters
            ICollection results = cache.GetEntries(ff.CreateFilter("homeAddress.state = 'MA'"));

            // Assert we got one result
            Assert.AreEqual(1, results.Count);

            foreach (object result in results)
               {
               TestContact val = (TestContact)((ICacheEntry)result).Value;
               Assert.AreEqual(0, val.Compare(val, t2));
               }

            // Query with an InFilter created on the client
            IList values = new ArrayList();
            values.Add("Loehe");
            IFilter inFil = new InFilter("getLastName", values);
            ContinuousQueryCache cqc1 = new ContinuousQueryCache(cache, inFil, false); 
            results = cqc1.Entries; 

            // Assert we got one result
            Assert.AreEqual(1, results.Count);
            foreach (object result in results)
               {
               TestContact val = (TestContact)((ICacheEntry)result).Value;
               Assert.AreEqual(0, val.Compare(val, t1));
               }

            // Query with an InFilter created on the cache
            IFilter filter = ff.CreateFilter("lastName in ('Loehe')");
            ContinuousQueryCache cqc2 = new ContinuousQueryCache(cache, filter, false);

            results = cqc2.Entries;

            // Assert we got one result
            Assert.AreEqual(1, results.Count);
            foreach (object result in results)
               {
               TestContact val = (TestContact)((ICacheEntry)result).Value;
               Assert.AreEqual(0, val.Compare(val, t1));
               }
        }

        [Test]
        public void TestCoh11230()
        {
            INamedCache namedCache = GetCache("local-coh11230");
            namedCache.Clear();

            ContinuousQueryCache cqc = new ContinuousQueryCache(namedCache, AlwaysFilter.Instance, false); 

            cqc.CacheValues = true;

            LocalCacheAccessor lcAccessor = new LocalCacheAccessor(namedCache, 1000, 100);
            Thread             thread1    = new Thread(new ThreadStart(lcAccessor.Run));
            thread1.Name                  = "LocalCacheAccessor";
            thread1.Start();

            CQCAccessor cqcAccessor = new CQCAccessor(cqc, 1000, 0);
            Thread      thread2     = new Thread(new ThreadStart(cqcAccessor.Run));
            thread2.Name            = "CQCAccessor";
            thread2.Start();

            DateTime endTime = DateTime.Now.AddSeconds(30);
            while (lcAccessor.Error == null
                    && cqcAccessor.Error == null
                    && !cqcAccessor.Finished
                    && !lcAccessor.Finished
                    && (DateTime.Now < endTime))
                {
                Blocking.Sleep(250);
                }
            bool IsEntriesSuccess = (cqcAccessor.Error == null);

			thread1.Interrupt();
			thread2.Interrupt();
			thread1.Join();
			thread2.Join();

            namedCache.Clear();
            lcAccessor   = new LocalCacheAccessor(namedCache, 1000, 100);
            thread1      = new Thread(new ThreadStart(lcAccessor.Run));
            thread1.Name = "LocalCacheAccessor";
            thread1.Start();

            cqcAccessor  = new CQCAccessor(cqc, 1000, 1);
            thread2      = new Thread(new ThreadStart(cqcAccessor.Run));
            thread2.Name = "CQCAccessor";
            thread2.Start();

            endTime = DateTime.Now.AddSeconds(30);
            while (lcAccessor.Error == null
                    && cqcAccessor.Error == null
                    && !cqcAccessor.Finished
                    && !lcAccessor.Finished
                    && (DateTime.Now < endTime))
                {
                Blocking.Sleep(250);
                }
            bool IsKeysSuccess = (cqcAccessor.Error == null);

			thread1.Interrupt();
			thread2.Interrupt();
			thread1.Join();
			thread2.Join();

            namedCache.Clear();
            lcAccessor   = new LocalCacheAccessor(namedCache, 1000, 100);
            thread1      = new Thread(new ThreadStart(lcAccessor.Run));
            thread1.Name = "LocalCacheAccessor";
            thread1.Start();

            cqcAccessor  = new CQCAccessor(cqc, 1000, 2);
            thread2      = new Thread(new ThreadStart(cqcAccessor.Run));
            thread2.Name = "CQCAccessor";
            thread2.Start();

            endTime = DateTime.Now.AddSeconds(30);
            while (lcAccessor.Error == null
                    && cqcAccessor.Error == null
                    && !cqcAccessor.Finished
                    && !lcAccessor.Finished
                    && (DateTime.Now < endTime))
                {
                Blocking.Sleep(250);
                }
            bool IsValuesSuccess = (cqcAccessor.Error == null);

			thread1.Interrupt();
			thread2.Interrupt();
			thread1.Join();
			thread2.Join();

            CacheFactory.Shutdown();

            Assert.IsTrue(IsEntriesSuccess);
            Assert.IsTrue(IsKeysSuccess);
            Assert.IsTrue(IsValuesSuccess);
        }

        [Test]
        public void TestTruncate()
        {
            INamedCache remoteCache = GetAndPopulateNamedCache("dist-extend-direct");
            
            TestCacheListener    listener = new TestCacheListener(); 
            ContinuousQueryCache   theCQC = new ContinuousQueryCache(remoteCache, AlwaysFilter.Instance, listener);
            
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(() => listener.GetActualTotal(), Is.EqualTo(NUM_INSERTS).After(500, 50));
            
            listener.ResetActualTotal();
            
            theCQC.Truncate();
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(() => listener.GetActualTotal(), Is.EqualTo(0).After(500, 50));
            Assert.That(() => theCQC.Count, Is.EqualTo(0).After(500, 50));
            Assert.That(theCQC.IsActive, Is.True);
        }
        
        [Test]
        public void TestTruncateReadOnly()
        {
            INamedCache remoteCache = GetAndPopulateNamedCache("dist-extend-direct");
            
            TestCacheListener    listener = new TestCacheListener(); 
            ContinuousQueryCache   theCQC = new ContinuousQueryCache(remoteCache, AlwaysFilter.Instance, listener);
            
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(() => listener.GetActualTotal(), Is.EqualTo(NUM_INSERTS).After(500, 50));
            
            listener.ResetActualTotal();

            theCQC.IsReadOnly = true;
            Assert.Throws<InvalidOperationException>(theCQC.Truncate);
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(() => listener.GetActualTotal(), Is.EqualTo(0).After(500, 50));
            Assert.That(() => theCQC.Count, Is.EqualTo(NUM_INSERTS).After(500, 50));
            Assert.That(theCQC.IsActive, Is.True);
        }
        
        [Test]
        public void TestTruncateReadOnlyBackCacheTruncation()
        {
            INamedCache remoteCache = GetAndPopulateNamedCache("dist-extend-direct");
            
            TestCacheListener    listener = new TestCacheListener(); 
            ContinuousQueryCache   theCQC = new ContinuousQueryCache(remoteCache, AlwaysFilter.Instance, listener);
            
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(() => listener.GetActualTotal(), Is.EqualTo(NUM_INSERTS).After(500, 50));
            
            listener.ResetActualTotal();

            theCQC.IsReadOnly = true;
            theCQC.Cache.Truncate();
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(() => listener.GetActualTotal(), Is.EqualTo(0).After(500, 50));
            Assert.That(() => theCQC.Count, Is.EqualTo(0).After(500, 50));
            Assert.That(theCQC.IsActive, Is.True);
        }
        
        public void TestTruncateWithListenerRegistered()
        {
            INamedCache remoteCache = GetAndPopulateNamedCache("dist-extend-direct");
            
            TestCacheListener    listener             = new TestCacheListener();
            TestNcdListener      deactivationListener = new TestNcdListener();
            ContinuousQueryCache theCQC               = new ContinuousQueryCache(remoteCache);
            
            theCQC.AddCacheListener(deactivationListener);
            
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(() => listener.GetActualTotal(), Is.EqualTo(NUM_INSERTS).After(500, 50));
            
            listener.ResetActualTotal();
            
            theCQC.AddCacheListener(deactivationListener);    
            theCQC.Truncate();
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(() => listener.GetActualTotal(), Is.EqualTo(0).After(500, 50));
            Assert.That(() => theCQC.Count, Is.EqualTo(0).After(500, 50));
            Assert.That(theCQC.IsActive, Is.True);
            
            Assert.That(() => deactivationListener.GetActualTotal(), Is.EqualTo(1).After(500, 50));
            Assert.That(deactivationListener.m_evt, Is.Not.Null);
            Assert.That(deactivationListener.m_evt.EventType, Is.EqualTo(CacheEventType.Updated));
            Assert.That(deactivationListener.m_evt.Key, Is.Null);
            Assert.That(deactivationListener.m_evt.OldValue, Is.Null);
            Assert.That(deactivationListener.m_evt.NewValue, Is.Null);
        }
        
        [Test]
        public void TestRelease()
        {
            INamedCache          remoteCache = GetAndPopulateNamedCache("dist-extend-direct");
            ContinuousQueryCache theCQC      = new ContinuousQueryCache(remoteCache);
            
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(theCQC.IsActive, Is.True);
            
            theCQC.Release();
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Disconnected).After(500, 50));
            Assert.That(theCQC.IsActive, Is.False);

            var result = theCQC["someKey"]; // trigger resync
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(theCQC.IsActive, Is.True);
        }
        
        [Test]
        public void TestDestroy()
        {
            INamedCache          remoteCache = GetAndPopulateNamedCache("dist-extend-direct");
            ContinuousQueryCache theCQC      = new ContinuousQueryCache(remoteCache);
            
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(theCQC.IsActive, Is.True);
            
            theCQC.Destroy();
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Disconnected).After(500, 50));
            Assert.That(theCQC.IsActive, Is.False);

            var result = theCQC["someKey"]; // trigger resync
            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized).After(500, 50));
            Assert.That(theCQC.IsActive, Is.True);
        }

        [Test]
        public void TestReconnectInterval()
        {
            ContinuousQueryCache theCQC = new ContinuousQueryCache(() => GetAndPopulateNamedCache("dist-extend-direct"));

            Assert.That(theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized));

            theCQC.ReconnectInterval = 3000;
            theCQC.Cache.Destroy();

            Assert.That(() => theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Disconnected).After(500, 50));
            Assert.That(theCQC[1], Is.EqualTo(1));
            Assert.That(theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Disconnected));

            theCQC[-1] = -1; // remote operation

            Assert.That(theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Disconnected));
            Assert.That(theCQC[-1], Is.EqualTo(null));
            Assert.That(theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Disconnected));

            Thread.Sleep(4000);

            Assert.That(theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Disconnected));
            Assert.That(theCQC[-1], Is.EqualTo(-1));
            Assert.That(theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized));
        }

        [Test]
        public void TestReconnectIntervalSetAtZero()
        {
            TestContinuousQueryCache theCQC = new TestContinuousQueryCache(() => GetAndPopulateNamedCache("dist-extend-direct"));

            Assert.That(theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized));

            theCQC.State = ContinuousQueryCache.CacheState.Disconnected;
            theCQC.LockState();

            Assert.That(theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Disconnected));

            try
            {
                Object val = theCQC[1];
            }
            catch (SystemException expected)
            {
                CacheFactory.Log("This error is expected: " + expected.ToString(), CacheFactory.LogLevel.Info);
            }

            theCQC.UnlockState();

            Assert.That(theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Disconnected));
            Assert.That(theCQC[1], Is.EqualTo(1));
            Assert.That(theCQC.State, Is.EqualTo(ContinuousQueryCache.CacheState.Synchronized));
        }
        
        #region helper methods

        private INamedCache GetAndPopulateNamedCache(String sCacheName)
        {
            INamedCache remoteCache = CacheFactory.GetCache(sCacheName);
            remoteCache.Clear();

            for (int i = 0; i < NUM_INSERTS; i++)
            {
                remoteCache.Insert(i, i);
            }

            return remoteCache;
        }

        #endregion

        #region Inner class: LocalCacheAccessor

        /// <summary>
        /// Class that adds and removes items from the internal cache.
        /// </summary>
        internal class LocalCacheAccessor
        {
            #region Constructors

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="cache">
            /// The internal cache of the CQC.
            /// </param>
            /// <param name="entries">
            /// Number of entries to add to the internal cache.
            /// </param>
            /// <param name="iterations">
            /// Number of times to update the cache.
            /// </param>
            public LocalCacheAccessor(INamedCache cache, int entries, int iterations)
            {
                m_cache = cache;
                m_exception = null;
                m_finished = false;
                m_entries = entries;
                m_iterations = iterations;
            }

            #endregion

            #region Thread implementation

            public void Run()
            {
                INamedCache cache = m_cache;
                cache.Clear();

                int iterations = m_iterations;
                int entries = m_entries;

                try
                {
                    for (var k = 0; k < iterations; k++)
                    {
                        for (int i = 0; i < entries; i++)
                        {
                            cache.Insert(i, i);
                        }

                        cache.Clear();
                    }
                }
                catch (Exception e)
                {
                    m_exception = e;
                }
                finally
                {
                    m_finished = true;
                }
            }

            #endregion

            #region Helper methods

            public bool Finished
            {
                get { return m_finished; }
            }

            public Exception Error
            {
                get { return m_exception; }
            }

            #endregion

            #region Data members

            private readonly INamedCache m_cache;

            private volatile bool m_finished;
            private volatile Exception m_exception;
            private readonly int m_entries;
            private readonly int m_iterations;

            #endregion
        }

        #endregion

        #region Inner class: TestContinuousQueryCache

        /// <summary>
        /// Custom CQC that can prevent state transitions.
        ///
        internal class TestContinuousQueryCache : ContinuousQueryCache
        {
            #region Constructors

            public TestContinuousQueryCache(Func<INamedCache> supplierCache) : base(supplierCache)
            {
            }

            #endregion

            #region Properties

            public override CacheState State
            {
                get
                {
                    return base.State;
                }
                set
                {
                    if (m_fLockState)
                    {
                        return;
                    }
                    base.State = value;
                }
            }

            #endregion

            #region Methods

            public void LockState()
            {
                m_fLockState = true;
            }

            public void UnlockState()
            {
                m_fLockState = false;
            }

            #endregion

            #region Data Members

            private bool m_fLockState;

            #endregion
        }
        #endregion
        
        #region Inner class: CQCAccessor

        /// <summary>
        /// Class that gets entries from the CQC.
        /// </summary>
        internal class CQCAccessor
        {
            #region Constructors

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="cache">
            /// The CQC.
            /// </param>
            /// <param name="iterations">
            /// Number of times to get entries of the cache.
            /// </param>
            /// <param name="method">
            /// Which CQC method to test.
            /// 0 (default): GetEntries, 1: GetKeys, 2: GetValues
            /// </param>
            public CQCAccessor(INamedCache cache, int iterations, int method)
            {
                m_cache = cache;
                m_exception = null;
                m_finished = false;
                m_iterations = iterations;
                m_method = method;
            }

            #endregion

            #region Thread implementation

            public void Run()
            {
                INamedCache cache = m_cache;

                int iterations = m_iterations;
                int batchSize = 1000;
                int method = m_method;

                try
                {
                    for (var k = 0; k < iterations; k++)
                    {
                        var start = 0;
                        var end = batchSize;
                        var rangeFilter = new BetweenFilter(IdentityExtractor.Instance, start, end);
                        ICollection results;
                        switch (method)
                        {
                            case 1:
                                results = cache.GetKeys(rangeFilter);
                                break;

                            case 2:
                                results = cache.GetValues(rangeFilter);
                                break;

                            default:
                                results = cache.GetEntries(rangeFilter);
                                break;
                        }

                        if (results.Count <= 0)
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    m_exception = e;
                }
                finally
                {
                    m_finished = true;
                }
            }

            #endregion

            #region Helper methods

            public bool Finished
            {
                get { return m_finished; }
            }

            public Exception Error
            {
                get { return m_exception; }
            }

            #endregion

            #region Data members

            private readonly INamedCache m_cache;

            private volatile bool m_finished;
            private volatile Exception m_exception;
            private readonly int m_iterations;
            private readonly int m_method;

            #endregion
        }

        #endregion
        
        #region Inner class: TestNCDListener

        internal class TestNcdListener : TestCacheListener, INamedCacheDeactivationListener
        {
        }
        
        #endregion

        #region Data members

        public const int NUM_INSERTS = 100;

        #endregion
        
    // ----- inner class: ValidateLiteListener --------------------------------------

    /**
    * MapListener that continuously receives events from the cache.
    */
    #region Helper class

    class ValidateLiteListener : ICacheListener
    { 
        public ValidateLiteListener(int count)
        {
            m_cCount         = count;
            m_cActualInserts = 0;
            m_cActualUpdates = 0;
            m_cActualDeletes = 0;
        }

        public int Count
        {
            get { return m_cCount; } 
            set { m_cCount = value; }
        }

        /**
        * Number of insert events listener actually received.
        *
        * @return  number of event received
        */
        public int ActualInserts
        { 
            get { return m_cActualInserts; }
            set { m_cActualInserts = value; }
        }

        /**
        * Number of update events listener actually received.
        *
        * @return  number of event received
        */
        public int ActualUpdates
        {
            get { return m_cActualUpdates; } 
            set { m_cActualUpdates = value; }
        }

        /**
        * Number of delete events listener actually received.
        *
        * @return  number of event received
        */
        public int ActualDeletes
        { 
            get { return m_cActualDeletes; } 
            set { m_cActualDeletes = value; }
        }

        public void EntryUpdated(CacheEventArgs evt)
        {
            m_cActualUpdates++;
            Assert.AreEqual(evt.NewValue, null);
            Assert.AreEqual(evt.OldValue, null);
        }

        public void EntryInserted(CacheEventArgs evt)
        {
            m_cActualInserts++;
            Assert.AreEqual(evt.NewValue, null);
            Assert.AreEqual(evt.OldValue, null);
        }

        public void EntryDeleted(CacheEventArgs evt)
        {
            m_cActualDeletes++;
            Assert.AreEqual(evt.OldValue, null);
        }

        /**
        * Total number of events listener actually received.
        *
        * @return  number of event received
        */
        public int GetActualTotal()
        {
            return m_cActualInserts+m_cActualUpdates+m_cActualDeletes;
        }

        /**
        * Reset the number of events received.
        *
        */
        public void ResetActualTotal()
        {
            m_cActualUpdates = 0;
            m_cActualInserts = 0;
            m_cActualDeletes = 0;
        }


        // ----- data members -----------------------------------------------

        /**
        * Number of insert events actually received
        */
        int m_cActualInserts;

        /**
        * Number of update events actually received
        */
        int m_cActualUpdates;

        /**
        * Number of delete events actually received
        */
        int m_cActualDeletes;

        /**
        * Number of events listener should receive
        */
        int m_cCount;
        }
        #endregion    
    }
}
