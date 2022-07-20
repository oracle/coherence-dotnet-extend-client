/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;

using NUnit.Framework;

using Tangosol.Net.Cache.Support;
using Tangosol.Net.Impl;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Aggregator;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Cache
{
    [TestFixture]
    public class NearCacheTests
    {
        NameValueCollection appSettings = TestUtils.AppSettings;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
        }

        [Test]
        public void NearCacheListenNoneTest()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);
            nearcache.Clear();

            nearcache.Add(1, "Ivan");
            Assert.AreEqual(1, nearcache.Count);
            Assert.AreEqual(1, nearcache.FrontCache.Count);
            Assert.AreEqual(1, nearcache.BackCache.Count);
            nearcache.Insert(2, "Goran");
            Assert.AreEqual(2, nearcache.Count);
            Assert.AreEqual(2, nearcache.FrontCache.Count);
            Assert.AreEqual(2, nearcache.BackCache.Count);
            Assert.IsTrue(nearcache.FrontCache.Contains(1));
            Assert.IsTrue(nearcache.FrontCache.Contains(2));
            Assert.IsTrue(nearcache.BackCache.Contains(1));
            Assert.IsTrue(nearcache.BackCache.Contains(2));

            object obj = nearcache[1];
            Assert.AreEqual("Ivan", obj);
            obj = nearcache[2];
            Assert.AreEqual("Goran", obj);

            nearcache.Clear();
            Assert.AreEqual(0, nearcache.Count);
            Assert.IsTrue(nearcache.IsActive);
            localcache.LocalCache = new LocalCache(Int32.MaxValue, 500);
            nearcache.Insert(1, "Ana");
            nearcache.Add(2, "Goran");
            Assert.IsTrue(nearcache.FrontCache.Contains(1));
            Assert.IsTrue(nearcache.FrontCache.Contains(2));
            Assert.IsTrue(nearcache.BackCache.Contains(1));
            Assert.IsTrue(nearcache.BackCache.Contains(2));
            Blocking.Sleep(1000);
            Assert.IsNull(nearcache.FrontCache[1]);
            Assert.IsNull(nearcache.FrontCache[2]);

            nearcache.Insert(3, "Ivan");
            IDictionary dict = nearcache.GetAll(new object[] {1, 2, 3, 4});
            Assert.AreEqual("Ana", dict[1]);
            Assert.AreEqual("Goran", dict[2]);
            Assert.AreEqual("Ivan", dict[3]);
            Assert.AreEqual(null, dict[4]);

            localcache.LocalCache = new LocalCache();
            obj = nearcache[1];
            Assert.AreEqual(obj, "Ana");
            Assert.IsTrue(nearcache.FrontCache.Contains(1));
            Assert.IsNull(nearcache.FrontCache[2]);

            Hashtable ht = new Hashtable();
            ht.Add(2, "Goran");
            ht.Add(3, "Ivan");
            ht.Add(4, "Aleks");
            nearcache.InsertAll(ht);
            nearcache.Remove(1);
            Assert.IsNull(nearcache.FrontCache[1]);
            Assert.IsNull(nearcache[1]);

            nearcache.Clear();
            nearcache.Release();
            Assert.IsFalse(nearcache.IsActive);

            CacheFactory.Shutdown();
        }

        [Test]
        public void NearCacheListenNoneCacheStatisticsTest()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);
            nearcache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add(1, "Aleks");
            ht.Add(2, "Ana");
            ht.Add(3, "Goran");
            ht.Add(4, "Ivan");
            nearcache.InsertAll(ht);
            foreach (DictionaryEntry entry in ht)
            {
                Assert.IsTrue(nearcache.Contains(entry.Key));
            }
            Assert.AreEqual(ht.Count, nearcache.CacheHits);

            localcache.LocalCache = new LocalCache(Int32.MaxValue, 1);
            Blocking.Sleep(1);
            foreach (DictionaryEntry entry in ht)
            {
                Assert.IsTrue(nearcache.Contains(entry.Key));
            }
            Assert.AreEqual(ht.Count, nearcache.CacheMisses);

            nearcache.Clear();
            nearcache.Destroy();
            Assert.IsFalse(nearcache.IsActive);
        }

        [Test]
        public void NearCacheListenNoneUnitsBeforePruningTest()
        {
            LocalNamedCache localcache = new LocalNamedCache(3);
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);
            nearcache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add(1, "Aleks");
            ht.Add(2, "Ana");
            ht.Add(3, "Goran");
            ht.Add(4, "Ivan");
            nearcache.InsertAll(ht);
            int miss = 0;
            int hit = 0;
            foreach (ICacheEntry entry in nearcache)
            {
                if (nearcache.FrontCache.Contains(entry.Key))
                {
                    hit++;
                }
                else
                {
                    miss++;
                }
            }
            for (int i = 0; i < nearcache.Count; i++)
            {
                Assert.AreEqual(ht[i], nearcache[i]);

            }

            Assert.AreEqual(hit, nearcache.CacheHits);
            Assert.AreEqual(miss, nearcache.CacheMisses);

            nearcache.Clear();
            nearcache.Release();
            Assert.IsFalse(nearcache.IsActive);

            CacheFactory.Shutdown();
        }

        [Test]
        public void NearCacheListenNoneWithNullsTest()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);
            nearcache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add(1, "Aleks");
            ht.Add(2, "Ana");
            ht.Add(3, "Goran");
            ht.Add(4, "Ivan");
            nearcache.InsertAll(ht);
            nearcache.Insert(5, null);
            Assert.AreEqual(ht.Count + 1, nearcache.Count);
            Assert.AreEqual(ht.Count, nearcache.FrontCache.Count);

            nearcache.Clear();
            nearcache.Release();
            Assert.IsFalse(nearcache.IsActive);

            CacheFactory.Shutdown();
        }

        #region IObservableCache method tests

        [Test]
        public void TestListeners()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config =
                XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-near-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("dist-extend-direct");

            cache.Clear();

            SyncListener listen = new SyncListener();

            cache.AddCacheListener(listen);
            Assert.IsNull(listen.CacheEvent);
            cache.Insert("global", "yes");
            Assert.IsNotNull(listen.CacheEvent);
            Assert.AreEqual(listen.CacheEvent.EventType, CacheEventType.Inserted);
            listen.CacheEvent = null;
            cache.RemoveCacheListener(listen);

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
            var newValue = (String) cache["test"];
            Assert.AreEqual("d", newValue);
            Assert.IsNull(listen.CacheEvent);

            cache.Remove("test");
            Assert.IsNotNull(listen.CacheEvent);
            Assert.AreEqual(listen.CacheEvent.EventType, CacheEventType.Deleted);

            cache.RemoveCacheListener(listen, "test");
            CacheEventFilter likeFilter =
                new CacheEventFilter(new LikeFilter(IdentityExtractor.Instance, "%ic", '\\', false));

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

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestListenerConfiguration()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config =
                XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-near-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("dist-extend-direct-listener");
            cache.Clear();
            SyncListenerStatic.Reset();

            Assert.IsNull(SyncListenerStatic.CacheEvent);
            cache.Insert("test", "yes");
            Assert.AreEqual(SyncListenerStatic.CacheEvent.EventType, CacheEventType.Inserted);
            cache["test"] = "d";
            Assert.AreEqual(SyncListenerStatic.CacheEvent.EventType, CacheEventType.Updated);
            cache.Remove("test");
            Assert.AreEqual(SyncListenerStatic.CacheEvent.EventType, CacheEventType.Deleted);

            CacheFactory.Shutdown();
        }

        /// <summary>
        /// Test events that generated by NearCache priming from
        /// INamedCache.get() should be ignored.
        /// 
        /// See COH-18376.
        /// </summary>
        [Test]
        public void TestListenerEvents()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config =
                XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-near-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("dist-extend-direct");

            cache.Clear();

            ListenerWithWait listen = new ListenerWithWait();

            cache.AddCacheListener(listen, "test", true);

            cache.Insert("test", "c");
            CacheEventArgs localEvent = listen.WaitForEvent(2000);
            Assert.IsNotNull(localEvent);

            String value = (String) cache["test"];
            localEvent = listen.WaitForEvent(4000);
            Assert.AreEqual("c", value);
            Assert.IsNull(localEvent);

            CacheFactory.Shutdown();
        }

        #endregion

        #region IInvocableCache method tests

        [Test]
        public void NearCacheListenNoneInvokeTest()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);
            nearcache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("Aleks", 1);
            ht.Add("Ana", 2);
            ht.Add("Goran", 3);
            ht.Add("Ivan", 4);
            nearcache.InsertAll(ht);

            IFilter filter = new GreaterFilter(IdentityExtractor.Instance, 199);
            IEntryProcessor processor = new ConditionalPut(filter, 204);
            nearcache.Invoke("Ivan", processor);
            Assert.AreEqual(4, nearcache["Ivan"]);
            Assert.AreEqual(4, nearcache.BackCache["Ivan"]);

            nearcache["Ivan"] = 200;
            nearcache.Invoke("Ivan", processor);
            Assert.AreEqual(200, nearcache["Ivan"]);
            Assert.AreEqual(204, nearcache.BackCache["Ivan"]);

            nearcache.Clear();
            nearcache.Release();
            Assert.IsFalse(nearcache.IsActive);

            CacheFactory.Shutdown();
        }

        [Test]
        public void NearCacheListenNoneInvokeAllTest()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);
            nearcache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("Aleks", 1);
            ht.Add("Ana", 2);
            ht.Add("Goran", 3);
            ht.Add("Ivan", 4);
            nearcache.InsertAll(ht);

            Hashtable htput = new Hashtable();
            htput.Add("Aleks", 195);
            htput.Add("Ana", 205);

            IFilter filter = new LessFilter(IdentityExtractor.Instance, 2);
            IEntryProcessor processor = new ConditionalPutAll(AlwaysFilter.Instance, htput);
            nearcache.InvokeAll(filter, processor);
            Assert.AreEqual(195, nearcache.BackCache["Aleks"]);
            Assert.AreEqual(2, nearcache.BackCache["Ana"]);

            nearcache.InvokeAll(htput.Keys, processor);
            Assert.AreEqual(205, nearcache.BackCache["Ana"]);

            nearcache.Clear();
            nearcache.Release();
            Assert.IsFalse(nearcache.IsActive);

            CacheFactory.Shutdown();
        }

        [Test]
        public void NearCacheListenNoneAggregateTest()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);
            nearcache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("Aleks", 1);
            ht.Add("Ana", 2);
            ht.Add("Goran", 3);
            ht.Add("Ivan", 4);
            nearcache.InsertAll(ht);

            IEntryAggregator aggregator = new Count();
            object count = nearcache.Aggregate(nearcache.Keys, aggregator);
            Assert.AreEqual(ht.Count, count);

            IFilter filter = new LessFilter(IdentityExtractor.Instance, 3);
            count = nearcache.Aggregate(filter, aggregator);
            Assert.AreEqual(2, count);

            nearcache.Clear();
            nearcache.Release();
            Assert.IsFalse(nearcache.IsActive);

            CacheFactory.Shutdown();
        }

        #endregion

        #region IQueryCache method tests

        [Test]
        public void NearCacheListenNoneGetKeysTest()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);
            nearcache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("Aleks", 1);
            ht.Add("Ana", 2);
            ht.Add("Goran", 3);
            ht.Add("Ivan", 4);
            nearcache.InsertAll(ht);

            IFilter filter = new BetweenFilter(IdentityExtractor.Instance, 2, 10);
            object[] result = nearcache.GetKeys(filter);
            Assert.AreEqual(3, result.Length);

            nearcache.Clear();
            nearcache.Release();
            Assert.IsFalse(nearcache.IsActive);

            CacheFactory.Shutdown();
        }

        [Test]
        public void NearCacheListenNoneGetValuesTest()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);
            nearcache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("Aleks", 1);
            ht.Add("Ana", 2);
            ht.Add("Goran", 3);
            ht.Add("Ivan", 4);
            nearcache.InsertAll(ht);

            IFilter filter = new BetweenFilter(IdentityExtractor.Instance, 2, 10);
            object[] result = nearcache.GetValues(filter);
            Assert.AreEqual(3, result.Length);

            result = nearcache.GetValues(filter, IdentityExtractor.Instance);
            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(i + 2, result[i]);
            }
            CacheFactory.Shutdown();
        }

        [Test]
        public void NearCacheListenNoneGetEntriesTest()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);
            nearcache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("Aleks", 1);
            ht.Add("Ana", 4);
            ht.Add("Goran", 3);
            ht.Add("Ivan", 2);
            nearcache.InsertAll(ht);

            IFilter filter = new BetweenFilter(IdentityExtractor.Instance, 2, 10);
            object[] result = nearcache.GetEntries(filter);
            Assert.AreEqual(3, result.Length);

            result = nearcache.GetEntries(filter, IdentityExtractor.Instance);
            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(i + 2, ((ICacheEntry) result[i]).Value);
            }

            CacheFactory.Shutdown();
        }

        #endregion

        #region IConcurrentCache method tests

        /*[Test]
        public void NearCacheLockTest()
        {
            Thread updater = new Thread(new ThreadStart(update));
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);

            nearcache.Clear();
            nearcache.Insert("Ivan", 1);
            Assert.AreEqual(1, nearcache.FrontCache["Ivan"]);

            if(nearcache.Lock("Ivan"))
            {
                Assert.IsNull(nearcache.FrontCache["Ivan"]);
                updater.Start();
                Blocking.Wait(this);
            }
            Assert.AreEqual(1, nearcache["Ivan"]);

        }

        public static void update()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache("dist-extend-direct");
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);
            nearcache.Insert("Ivan", 0);
            Monitor.PulseAll();
        }*/

        #endregion

        #region Regression tests

        [Test]
        public void Coh8796()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache("dist-extend-direct");
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenLogical);

            safecache.Clear();

            ICache cacheFront = nearcache.FrontCache;
            ICache cacheBack  = nearcache.BackCache;
            int    cPuts      = 1000;

            for (int i = 0; i < cPuts; i++)
            {
                cacheBack.Insert(i, i, 10000);
                if (i % 2 == 0)
                {
                    Object o = nearcache[i];
                    Assert.AreEqual(i, o);
                }
            }

            Assert.AreEqual(cPuts / 2, cacheFront.Count);
            Assert.AreEqual(cPuts, cacheBack.Count);

            // expire the back map
            Thread.Sleep(15000);

            // calling Count expires the entries in the back and sends out synthetic deletes
            Assert.AreEqual(0, cacheBack.Count);

            // ensure that synthetic deletes are filtered out;
            // front map values for evens are still there
            for (int i = 0; i < cPuts; i++)
            {
                if (i % 2 == 0)
                {
                    Assert.AreEqual(i, cacheFront[i]);
                }
                else
                {
                    Assert.IsNull(cacheFront[i]);
                }
            }

            Assert.AreEqual(cPuts / 2, cacheFront.Count); // 0, 2, 4, ...
            Assert.AreEqual(0, cacheBack.Count);

            // ensure that Insert works, and that a value update is properly
            // raised to both the front and back maps
            for (int i = 0; i < cPuts; i++)
            {
                int nKey = i * 4;

                nearcache.Insert(nKey, nKey);

                Assert.AreEqual(nKey, cacheFront[nKey]);
                Assert.AreEqual(nKey, cacheBack[nKey]);

                cacheBack.Insert(nKey, nKey + 1);

                Assert.IsNull(cacheFront[nKey]);
                Assert.AreEqual(nKey + 1, cacheBack[nKey]);

                nearcache.Insert(nKey, nKey);

                Assert.AreEqual(nKey, cacheFront[nKey]);
                Assert.AreEqual(nKey, cacheBack[nKey]);

                cacheBack.Remove(nKey);

                Assert.IsFalse(cacheBack.Contains(nKey));
                Assert.IsFalse(cacheFront.Contains(nKey));
                Assert.IsNull(cacheBack[nKey]);
                Assert.IsNull(cacheFront[nKey]);
            }
            nearcache.Release();
            // fresh reference to the cache
            safecache = CacheFactory.GetCache(CacheName);
            safecache.Destroy();
            CacheFactory.Shutdown();
        }

        #endregion

        [Test]
        public void TestNearCacheDispose()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache;

            string[] keys = {"key1", "key2", "key3", "key4"};
            string[] values = {"value1", "value2", "value3", "value4"};
            using (nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone))
            {
                nearcache.Clear();
                IDictionary h = new Hashtable();
                h.Add(keys[0], values[0]);
                h.Add(keys[1], values[1]);
                h.Add(keys[2], values[2]);
                h.Add(keys[3], values[3]);
                nearcache.InsertAll(h);

                foreach (object key in nearcache.Keys)
                {
                    Assert.IsTrue(nearcache.Contains(key));
                }
            }
            //after disposal
            Assert.IsFalse(nearcache.IsActive);
            // fresh reference to the cache
            safecache = CacheFactory.GetCache(CacheName);
            Assert.IsTrue(safecache.IsActive);
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestNearCacheTruncate()
        {
            LocalNamedCache localcache = new LocalNamedCache();
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            NearCache nearcache = new NearCache(localcache, safecache, CompositeCacheStrategyType.ListenNone);
            nearcache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("Aleks", 1);
            ht.Add("Ana", 2);
            ht.Add("Goran", 3);
            ht.Add("Ivan", 4);
            nearcache.InsertAll(ht);

            IFilter filter = new BetweenFilter(IdentityExtractor.Instance, 2, 10);
            object[] result = nearcache.GetKeys(filter);
            Assert.AreEqual(3, result.Length);

            nearcache.Truncate();
            Assert.IsTrue(nearcache.IsActive);

            Thread.Sleep(2000);
            result = nearcache.GetKeys(filter);
            Assert.AreEqual(0, result.Length);

            nearcache.Release();
            Assert.IsFalse(nearcache.IsActive);

            CacheFactory.Shutdown();
        }

        #region Helper class

        class SyncListener : CacheListenerSupport.ISynchronousListener
        {
            private CacheEventArgs m_cacheEvent;

            public SyncListener()
            {
                m_cacheEvent = null;
            }

            public CacheEventArgs CacheEvent
            {
                get { return m_cacheEvent; }
                set { m_cacheEvent = value; }
            }

            public void EntryInserted(CacheEventArgs evt)
            {
                CacheEvent = evt;
            }

            public void EntryUpdated(CacheEventArgs evt)
            {
                CacheEvent = evt;
            }

            public void EntryDeleted(CacheEventArgs evt)
            {
                CacheEvent = evt;
            }
        }

        public class SyncListenerStatic : CacheListenerSupport.ISynchronousListener
        {
            private static CacheEventArgs m_cacheEvent;

            public SyncListenerStatic()
            {
                m_cacheEvent = null;
            }

            public static CacheEventArgs CacheEvent
            {
                get { return m_cacheEvent; }
                set { m_cacheEvent = value; }
            }

            public void EntryInserted(CacheEventArgs evt)
            {
                CacheEvent = evt;
            }

            public void EntryUpdated(CacheEventArgs evt)
            {
                CacheEvent = evt;
            }

            public void EntryDeleted(CacheEventArgs evt)
            {
                CacheEvent = evt;
            }

            public static void Reset()
            {
                CacheEvent = null;
            }
        }

        public class ListenerWithWait : ICacheListener
        {
            private CacheEventArgs m_cacheEvent;

            public ListenerWithWait()
            {
                m_cacheEvent = null;
            }

            public CacheEventArgs CacheEvent
            {
                get { return m_cacheEvent; }
                set { m_cacheEvent = value; }
            }

            public void EntryInserted(CacheEventArgs evt)
            {
                CacheEvent = evt;
            }

            public void EntryUpdated(CacheEventArgs evt)
            {
                CacheEvent = evt;
            }

            public void EntryDeleted(CacheEventArgs evt)
            {
                CacheEvent = evt;
            }

            /// <summary>
            /// Return the MapEvent received by this MapListener, blocking for the
            /// specified number of milliseconds in the case that an event hasn't been
            /// received yet.
            /// </summary>
            /// <param name="cMillis">
            ///  the number of milliseconds to wait for an event
            /// </param>
            /// <returns>
            /// the MapEvent received by this MapListener.
            /// </returns>
            public CacheEventArgs WaitForEvent(int cMillis)
            {
                var localEvent = m_cacheEvent;
                if (localEvent == null)
                {
                    try
                    {
                        Thread.Sleep(cMillis);
                        localEvent = m_cacheEvent;
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Thread.CurrentThread.Interrupt();
                        throw new SystemException(e.Message);
                    }
                }

                clearEvent();
                return localEvent;            
            }

            /// <summary>
            /// Reset the MapEvent property.
            /// </summary>
            public void clearEvent()
            {
                m_cacheEvent = null;
            }
        }

        #endregion
    }
}
