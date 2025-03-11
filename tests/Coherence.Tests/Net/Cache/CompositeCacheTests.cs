/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;

using Tangosol.Net.Impl;
using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    [TestFixture]
    public class CompositeCacheTests
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
        public void CompositeCacheListenAllTest()
        {
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            CompositeCache ccache = new CompositeCache(new LocalNamedCache(),
                                                       safecache,
                                                       CompositeCacheStrategyType.ListenAll);
            ccache.Clear();
            Hashtable ht = new Hashtable();
            ht.Add(1, "Aleks");
            ht.Add(2, "Ana");
            ht.Add(3, "Goran");
            ht.Add(4, "Ivan");
            ccache.InsertAll(ht);

            Assert.AreEqual(4, ccache.Keys.Count);
            Assert.AreEqual(4, ccache.Values.Count);
            Assert.AreEqual(4, ccache.Entries.Count);

            //first entry gets inserted into back cache
            ccache.Insert(100, "Jason");
            Assert.IsNull(ccache.FrontCache[100]);

            // when we reference the entry it gets inserted into front cache
            // and listener is added (global listener)
            Assert.AreEqual("Jason", ccache[100]);
            Assert.AreEqual("Jason", ccache.FrontCache[100]);

            // event is raised when entry is updated and front cache
            // gets updated also
            ccache[100] = "Cameron";
            Assert.AreEqual("Cameron", ccache.FrontCache[100]);
            Assert.AreEqual("Cameron", ccache[100]);

            ccache[100] = null;
            Assert.AreEqual(null, ccache[100]);

            ccache.Remove(100);
            Assert.IsNull(ccache.FrontCache[100]);

            // gets all the entries from the hashtable ht,
            // events are handled and entries are put into the front cache
            IDictionary dict = ccache.GetAll(ht.Keys);
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual("Aleks", ccache.FrontCache[1]);
            Assert.AreEqual("Ana", ccache.FrontCache[2]);
            Assert.AreEqual("Goran", ccache.FrontCache[3]);
            Assert.AreEqual("Ivan", ccache.FrontCache[4]);

            Hashtable htnew = new Hashtable();
            htnew.Add(1, "Cameron");
            htnew.Add(2, "Gene");
            htnew.Add(10, "Jason");
            ccache.InsertAll(htnew);

            // <deprecated>
            // listeners are added for the first two entries, so front cache is updated
            // thrird entry is not updated, it is inserted, so there is no entry in front cache
            // first entry needs to be referenced so that event would be raised and entry copied
            // into front cache.
            // Assert.IsNull(ccache.FrontCache[10]);
            // Assert.AreEqual("Jason", ccache[10]);
            // </deprecated>

            // after COHNET-94 fix, if invalidation strategy is ListenAll
            // in the time of inserting new values into cache events are handled and
            // front cache gets refreshed.
            Assert.AreEqual("Cameron", ccache.FrontCache[1]);
            Assert.AreEqual("Gene", ccache.FrontCache[2]);
            Assert.AreEqual("Jason", ccache.FrontCache[10]);

            // checking if inserting new entry is refreshing the front cache,
            // a feature gained after COHNET-94 fix
            ccache.Insert(11, "Ivan");
            Assert.AreEqual("Ivan", ccache.FrontCache[11]);

            CompositeCache ccache2 = new CompositeCache(new LocalNamedCache(),
                                                        safecache,
                                                        CompositeCacheStrategyType.ListenAll);

            // another client is updating back cache
            // front cache entry of current client gets invalidated
            long invhits = ccache.InvalidationHits;
            long invmiss = ccache.InvalidationMisses;

            // invalidation is called for existing entry
            ccache2.Insert(10, "unknown");
            Assert.AreEqual("unknown", ccache[10]);
            Assert.AreEqual(invhits+1, ccache.InvalidationHits);
            Assert.AreEqual(invmiss, ccache.InvalidationMisses);

            // invalidation is called for non-existig entry
            invhits = ccache.InvalidationHits;
            invmiss = ccache.InvalidationMisses;
            ccache2.Insert(101, "Milica");
            Assert.AreEqual("Milica", ccache[101]);
            Assert.AreEqual(invhits, ccache.InvalidationHits);
            Assert.AreEqual(invmiss+1, ccache.InvalidationMisses);

            //releasing the front and back cache, including listeners
            ccache2.Release();

            ccache.Clear();
            Assert.AreEqual(0, ccache.Count);
            ccache.Release();
            CacheFactory.Shutdown();
        }

        protected void PresentTest(INamedCache safecache, CompositeCache ccache)
        {
            Hashtable ht = new Hashtable();
            ht.Add(1, "Aleks");
            ht.Add(2, "Ana");
            ht.Add(3, "Goran");
            ht.Add(4, "Ivan");
            ccache.InsertAll(ht);

            //first entry gets inserted into back cache
            ccache.Insert(100, "Jason");
            Assert.IsNull(ccache.FrontCache[100]);

            // when we reference the entry it gets inserted into front cache
            // and listener is added
            Assert.AreEqual("Jason", ccache[100]);
            Assert.AreEqual("Jason", ccache.FrontCache[100]);

            Assert.AreEqual(1, ccache.TotalRegisterListener);

            // event is raised when entry is updated and front cache
            // gets updated also
            ccache[100] = "Cameron";
            Assert.AreEqual("Cameron", ccache.FrontCache[100]);
            Assert.AreEqual("Cameron", ccache[100]);

            ccache.Remove(100);
            Assert.IsNull(ccache.FrontCache[100]);

            // gets all the entries from the "hashtable ht",
            // puts them into the front cache
            // and adds listener for them
            IDictionary dict = ccache.GetAll(ht.Keys);
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual("Aleks", ccache.FrontCache[1]);
            Assert.AreEqual("Ana", ccache.FrontCache[2]);
            Assert.AreEqual("Goran", ccache.FrontCache[3]);
            Assert.AreEqual("Ivan", ccache.FrontCache[4]);

            Assert.AreEqual(5, ccache.TotalRegisterListener);

            Hashtable htnew = new Hashtable();
            htnew.Add(1, "Cameron");
            htnew.Add(2, "Gene");
            htnew.Add(10, "Jason");
            ccache.InsertAll(htnew);

            // listeners are added for the first two entries, so front cache is updated
            // thrird entry is not updated, it is inserted, so there is no entry in front cache
            Assert.AreEqual("Cameron", ccache.FrontCache[1]);
            Assert.AreEqual("Gene", ccache.FrontCache[2]);
            Assert.IsNull(ccache.FrontCache[10]);
            Assert.AreEqual("Jason", ccache[10]);

            CompositeCache ccache2 = new CompositeCache(new LocalNamedCache(),
                                             safecache,
                                             CompositeCacheStrategyType.ListenPresent);

            // another client is updating back cache
            // front cache entry of current client gets invalidated
            long invhits = ccache.InvalidationHits;
            long invmiss = ccache.InvalidationMisses;

            // invalidation is called for existing entry
            // therefore event listener is present
            ccache2.Insert(10, "unknown");
            Assert.AreEqual("unknown", ccache[10]);

            // adding an listener when getting an entry for the first time
            object obj = ccache2[10];
            Assert.AreEqual(1, ccache2.TotalRegisterListener);

            Assert.AreEqual(invhits + 1, ccache.InvalidationHits);
            Assert.AreEqual(invmiss, ccache.InvalidationMisses);

            // invalidation is called for non-existig entry
            // therefore event listener is not present
            // and event is not picked up and ccache.InvalidationHits or
            // ccache.InvalidationMisses has not been changed
            invhits = ccache.InvalidationHits;
            invmiss = ccache.InvalidationMisses;
            ccache2.Insert(101, "Milica");
            Assert.AreEqual("Milica", ccache[101]);
            Assert.AreEqual(invhits, ccache.InvalidationHits);
            Assert.AreEqual(invmiss, ccache.InvalidationMisses);

            //releasing the front and back cache, including listeners
            ccache2.Release();

            ccache.Clear();
            Assert.AreEqual(0, ccache.Count);
            ccache.Release();
            CacheFactory.Shutdown();
        }

        [Test]
        public void CompositeCacheListenPresentTest()
        {
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            CompositeCache ccache = new CompositeCache(new LocalNamedCache(),
                                                       safecache,
                                                       CompositeCacheStrategyType.ListenPresent);
            PresentTest(safecache, ccache);
        }

        [Test]
        public void CompositeCacheListenAutoTest()
        {
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            CompositeCache ccache = new CompositeCache(new LocalNamedCache(),
                                                       safecache,
                                                       CompositeCacheStrategyType.ListenAuto);
            PresentTest(safecache, ccache);
        }

        [Test]
        public void CompositeCacheListenDefaultTest()
        {
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            CompositeCache ccache = new CompositeCache(new LocalNamedCache(),
                                                       safecache);
            Assert.AreEqual(CompositeCacheStrategyType.ListenAuto, ccache.InvalidationStrategy);
            PresentTest(safecache, ccache);
        }

        [Test]
        public void CompositeCacheListenNoneTest()
        {
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            CompositeCache ccache = new CompositeCache(new LocalNamedCache(),
                                                       safecache,
                                                       CompositeCacheStrategyType.ListenNone);
            ccache.Clear();
            Hashtable ht = new Hashtable();
            ht.Add(1, "Aleks");
            ht.Add(2, "Ana");
            ht.Add(3, "Goran");
            ht.Add(4, "Ivan");
            ccache.InsertAll(ht);

            Assert.AreEqual(4, ccache.Keys.Count);
            Assert.AreEqual(4, ccache.Values.Count);
            Assert.AreEqual(4, ccache.Entries.Count);

            // there is no listeners, so all entries are stored in
            // front cache as well as back cache
            Assert.AreEqual("Aleks", ccache.FrontCache[1]);
            Assert.AreEqual("Ana", ccache.FrontCache[2]);
            Assert.AreEqual("Goran", ccache.FrontCache[3]);
            Assert.AreEqual("Ivan", ccache.FrontCache[4]);

            ccache[100] = "Cameron";
            Assert.AreEqual("Cameron", ccache.FrontCache[100]);
            Assert.AreEqual("Cameron", ccache[100]);

            ccache[100] = null;
            Assert.AreEqual(null, ccache.FrontCache[100]);

            ccache.Remove(100);
            Assert.IsNull(ccache.FrontCache[100]);

            // gets all the entries from the hashtable ht,
            // puts them into the front cache
            // and adds listener for them
            IDictionary dict = ccache.GetAll(ht.Keys);
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual("Aleks", ccache.FrontCache[1]);
            Assert.AreEqual("Ana",   ccache.FrontCache[2]);
            Assert.AreEqual("Goran", ccache.FrontCache[3]);
            Assert.AreEqual("Ivan",  ccache.FrontCache[4]);


            CompositeCache ccache2 = new CompositeCache(new LocalNamedCache(),
                                                        safecache,
                                                        CompositeCacheStrategyType.ListenNone);

            // we're updating safecache from a different client
            ccache2[4] = "Jason";
            ccache2[5] = "Cameron";
            Assert.AreEqual("Jason", ccache2[4]);

            // miss
            Assert.AreEqual("Ivan", ccache[4]);
            Assert.AreNotEqual("Jason", ccache[4]);

            ccache2.Clear();
            ccache.Clear();
            Assert.AreEqual(0, ccache.Count);
            ccache.Release();
            CacheFactory.Shutdown();
        }


        [Test]
        public void CompositeCacheObjectMethodsTest()
        {
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            CompositeCache ccache = new CompositeCache(new LocalNamedCache(),
                                                       safecache,
                                                       CompositeCacheStrategyType.ListenPresent);
            string s = ccache.ToString();
            Assert.IsTrue(s.IndexOf("PRESENT") >= 0);

            ccache.Clear();
            Assert.AreEqual(0, ccache.Count);
            ccache.Release();
            CacheFactory.Shutdown();
        }

        [Test]
        public void CompositeCacheGetAllWithSameKeys()
        {
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            // ListenAll
            CompositeCache ccache = new CompositeCache(new LocalNamedCache(),
                                                       safecache,
                                                       CompositeCacheStrategyType.ListenAll);
            ccache.Clear();
            Hashtable ht = new Hashtable();
            ht.Add(1, "Aleks");
            ht.Add(2, "Ana");
            ht.Add(3, "Goran");
            ht.Add(4, "Ivan");
            ccache.InsertAll(ht);

            int[] keys1 = {1, 1, 1, 2, 3, 10, 10};
            IDictionary result = ccache.GetAll(keys1);
            Assert.AreEqual(3, result.Count);

            safecache.Insert(5, "Milos");
            int[] keys2 = {1, 1, 1, 2, 3, 5, 10, 10};
            result = ccache.GetAll(keys2);
            Assert.AreEqual(4, result.Count);
            ccache.Release();

            // ListenNone
            ccache = new CompositeCache(new LocalNamedCache(),
                                        safecache,
                                        CompositeCacheStrategyType.ListenNone);
            ccache.Clear();
            ccache.InsertAll(ht);
            result = ccache.GetAll(keys1);
            Assert.AreEqual(3, result.Count);
            safecache.Insert(5, "Milos");
            result = ccache.GetAll(keys2);
            Assert.AreEqual(4, result.Count);
            ccache.Release();

            // ListenPresent
            ccache = new CompositeCache(new LocalNamedCache(),
                                        safecache,
                                        CompositeCacheStrategyType.ListenPresent);
            ccache.Clear();
            ccache.InsertAll(ht);
            result = ccache.GetAll(keys1);
            Assert.AreEqual(3, result.Count);
            safecache.Insert(5, "Milos");
            result = ccache.GetAll(keys2);
            Assert.AreEqual(4, result.Count);
            safecache.Remove(1);
            result = ccache.GetAll(keys2);
            Assert.AreEqual(3, result.Count);
        }

        /// <summary>
        /// Testing CompositeCache expiry functionality.
        /// </summary>
        [Test]
        public void TestCompositeCacheExpiryDelay()
        {
            INamedCache safecache = CacheFactory.GetCache(CacheName);
            // ListenAll
            CompositeCache compositeCache = new CompositeCache(new LocalCache(),
                                                               safecache,
                                                               CompositeCacheStrategyType.ListenNone);
            compositeCache.Insert("key1", "value1", 50);
            Assert.AreEqual("value1", compositeCache["key1"]);

            Blocking.Sleep(20);
            // entry still didn't expire
            Assert.AreEqual("value1", compositeCache["key1"]);

            Blocking.Sleep(100);
            // should expire by now
            Assert.IsNull(compositeCache["key1"]);
        }
    }
}