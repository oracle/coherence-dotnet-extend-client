/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Threading;

using NUnit.Framework;

using Tangosol.Net.Impl;
using Tangosol.Util.Collections;

namespace Tangosol.Net.Cache
{
    [TestFixture]
    public class BundlingNamedCacheTests : RemoteNamedCacheTests
    {
        [SetUp]
        public void SetUp()
        {
            CacheName = CACHE_NAME;
        }

        [Test]
        public override void TestInitialize()
        {
            INamedCache cache = CacheFactory.GetCache(CacheName);
            Assert.IsNotNull(cache);
            Assert.IsInstanceOf(typeof(BundlingNamedCache), cache);
            Assert.IsInstanceOf(typeof(ICacheService), cache.CacheService);
            Assert.AreEqual(cache.CacheName, CacheName);
            Assert.IsTrue(cache.IsActive);
            Assert.IsTrue(cache.CacheService.IsRunning);

            BundlingNamedCache bundleCache = (BundlingNamedCache) cache;
            Assert.IsInstanceOf(typeof(SafeNamedCache), bundleCache.NamedCache);
            Assert.IsInstanceOf(typeof(SafeCacheService), bundleCache.CacheService);

            CacheFactory.ReleaseCache(cache);
            Assert.IsFalse(cache.IsActive);

            CacheFactory.Shutdown();
            Assert.IsFalse(cache.CacheService.IsRunning);
        }

        [Test]
        public override void TestNamedCacheProperties()
        {
            INamedCache cache = CacheFactory.GetCache(CacheName);
            string key = "testNamedCachePropertiesKey";
            string value = "testNamedCachePropertiesValue";

            // INamedCache
            Assert.IsTrue(cache.IsActive);

            cache.Clear();
            Assert.AreEqual(cache.Count, 0);

            cache.Insert(GetKeyObject(key), value);
            Assert.AreEqual(cache.Count, 1);
            Assert.AreEqual(cache[GetKeyObject(key)], value);

            // BundlingNamedCache
            BundlingNamedCache bundleCache = (BundlingNamedCache) cache;
            Assert.IsFalse(bundleCache.IsFixedSize);
            Assert.IsFalse(bundleCache.IsReadOnly);
            Assert.IsTrue(bundleCache.IsSynchronized);

            // RemoteNamedCache
            SafeNamedCache safeCache = (SafeNamedCache) bundleCache.NamedCache;
            Assert.IsTrue(safeCache.IsActive);
            CacheFactory.ReleaseCache(cache);
            CacheFactory.Shutdown();
        }

        [Test]
        public override void TestNamedCacheEntryCollection()
        {
            INamedCache cache = CacheFactory.GetCache(CacheName);
            object[] keys = { GetKeyObject("key1"), GetKeyObject("key2"), GetKeyObject("key3"), GetKeyObject("key4") };
            string[] values = { "value1", "value2", "value3", "value4" };
            cache.Clear();
            IDictionary h = new Hashtable();
            h.Add(keys[0], values[0]);
            h.Add(keys[1], values[1]);
            h.Add(keys[2], values[2]);
            h.Add(keys[3], values[3]);
            cache.InsertAll(h);

            BundlingNamedCache bundleCache = (BundlingNamedCache) cache;
            IDictionaryEnumerator de = bundleCache.NamedCache.GetEnumerator();
            ArrayList al = new ArrayList(h.Values);
            for (; de.MoveNext(); )
            {
                Assert.IsTrue(al.Contains(de.Value));
                Assert.IsTrue(h.Contains(de.Key));
            }

            CacheFactory.ReleaseCache(cache);
            CacheFactory.Shutdown();
        }

        /// <summary>
        /// Test a bundle configuration with customized values.
        /// </summary>
        [Test]
        public void testDistBundlingConfig()
        {
            var cache = (BundlingNamedCache) CacheFactory.GetCache(CACHE_NAME);
            validateBundler(cache.GetBundlerOp);
            validateBundler(cache.InsertBundlerOp);
            validateBundler(cache.RemoveBundlerOp);
        }

        /// <summary>
        /// Validate the bundler configuration with customized values.
        /// </summary>
        /// <param name="bundler"> The bundler.
        /// </param>
        public static void validateBundler(AbstractBundler bundler)
        {
            Assert.AreEqual(10, bundler.SizeThreshold); // preferred-size
            Assert.AreEqual(3, bundler.DelayMillis);
            Assert.AreEqual(5, bundler.ThreadThreshold);
            Assert.IsTrue(bundler.AllowAutoAdjust);
        }

        /// <summary>
        /// Test a default bundle configuration.  The preferred-size must 
        /// be > 0 so that the default operation-name of "all" takes effect.
        /// </summary>
        [Test]
        public void testDistBundlingDefaultConfig()
        {
            var cache = (BundlingNamedCache)CacheFactory.GetCache("dist-bundling-cache-defaults");
            validateDefaultBundler(cache.GetBundlerOp);
            validateDefaultBundler(cache.InsertBundlerOp);
            validateDefaultBundler(cache.RemoveBundlerOp);
        }

        /// <summary>
        /// Validate the default bundler configuration.
        /// </summary>
        /// <param name="bundler">The bundler.
        /// </param>
        public static void validateDefaultBundler(AbstractBundler bundler)
        {
            Assert.AreEqual(10, bundler.SizeThreshold); // preferred-size
            Assert.AreEqual(1, bundler.DelayMillis);
            Assert.AreEqual(4, bundler.ThreadThreshold);
            Assert.IsFalse(bundler.AllowAutoAdjust);
        }

        /// <summary>
        /// Test concurrent Insert operations.
        /// </summary>
        [Test]
        public void testConcurrentInsert()
        {
            m_cache = CacheFactory.GetCache(CACHE_NAME);
            m_cache.Clear();

            ResetSemaphore();
            runParallel(RunInsert, THREADS);

            TestCacheContent(m_cache, THREADS * COUNT);
            m_cache.Clear();
        }

        /// <summary>
        /// Test concurrent InsertAll operations.
        /// </summary>
        [Test]
        public void testConcurrentInsertAll()
        {
            m_cache = CacheFactory.GetCache(CACHE_NAME);
            m_cache.Clear();

            ResetSemaphore();
            runParallel(RunInsertAll, THREADS);

            TestCacheContent(m_cache, THREADS * COUNT);
            m_cache.Clear();
        }

        /// <summary>
        /// Test concurrent get operations.
        /// </summary>
        [Test]
        public void testConcurrentGet()
        {
            m_cache = CacheFactory.GetCache(CACHE_NAME);
            m_cache.Clear();

            Fill(m_cache, THREADS * COUNT);
            ResetSemaphore();
            runParallel(RunGet, THREADS);

            ResetSemaphore();
            runParallel(RunGetSameKey, THREADS);

            m_cache.Clear();
        }

        /// <summary>
        /// Test concurrent GetAll operations.
        /// </summary>
        [Test]
        public void testConcurrentGetAll()
        {
            m_cache = CacheFactory.GetCache(CACHE_NAME);
            m_cache.Clear();

            Fill(m_cache, THREADS * COUNT);
            ResetSemaphore();
            runParallel(RunGetAll, THREADS);
            m_cache.Clear();
        }

        /// <summary>
        /// Run the Insert operations.
        /// </summary>
        public void RunInsert()
        {
            WaitForSemaphore();
            int ofStart = GetThreadIndex() * COUNT;
            for (int i = 0; i < COUNT; i++)
            {
                m_cache.Insert(ofStart + i, (ofStart + i).ToString());
            }
        }

        /// <summary>
        /// Run the InsertAll operations.
        /// </summary>
        public void RunInsertAll()
        {
            WaitForSemaphore();
            int ofStart = GetThreadIndex() * COUNT;
            var mapTemp = new Hashtable();
            for (int i = 0; i < COUNT; i++)
            {
                mapTemp.Add(ofStart + i, (ofStart + i).ToString());

                if (mapTemp.Count > new Random().Next(5) ||
                    i == COUNT - 1)
                {
                    m_cache.InsertAll(mapTemp);
                    mapTemp.Clear();
                }
            }
        }

        /// <summary>
        /// Run the Get operations.
        /// </summary>
        public void RunGet()
        {
            WaitForSemaphore();
            for (int i = 0; i < COUNT; i++)
            {
                int    key = new Random().Next(m_cache.Count);
                Object val = m_cache[key];
                Assert.AreEqual(key.ToString(), val);
            }
        }

        public void RunGetSameKey()
        {
            WaitForSemaphore();
            for (int i = 0; i < COUNT; i++)
            {
                Object val = m_cache[0];
                Assert.AreEqual("0", val);
            }
        }

        /// <summary>
        /// Run the GetAll operations.
        /// </summary>
        public void RunGetAll()
        {
            WaitForSemaphore();
            var setKeys = new HashSet();
            for (int i = 0; i < COUNT; i++)
            {
                Int32 key = new Random().Next(m_cache.Count);

                setKeys.Add(key);
                if (setKeys.Count > new Random().Next(5))
                {
                    ICollection map = m_cache.GetAll(setKeys);

                    Assert.AreEqual(setKeys.Count, map.Count);
                    setKeys.Clear();

                    foreach (DictionaryEntry entry in (IDictionary) map)
                    {
                        Assert.AreEqual(entry.Key.ToString(), entry.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Run the specified task on multiple cThreads and wait for completion.
        /// </summary>
        /// <param name="task">
        /// The task to run.
        /// </param>
        /// <param name="cThreads">
        /// The number of threads.
        /// </param>
        protected static void runParallel(ThreadStart task, int cThreads)
        {
            Thread[] threads = new Thread[cThreads];
            for (int i = 0; i < cThreads; i++)
            {
                threads[i] = new Thread(new ThreadStart(task));
                threads[i].Name = PREFIX + i;
                threads[i].Start();
            }

            lock (SEMAPHORE)
            {
                s_started = true;
                Monitor.PulseAll(SEMAPHORE);
            }

            try
            {
                for (int i = 0; i < cThreads; i++)
                {
                    threads[i].Join();
                }
            }
            catch (ThreadInterruptedException) {/*do nothing*/}
        }

        /// <summary>
        /// Retrive the thread index from its name.
        /// </summary>
        /// <returns> 
        /// The thread index.
        /// </returns>
        protected static int GetThreadIndex()
        {
            String name = Thread.CurrentThread.Name;
            int ofIx = name.LastIndexOf(PREFIX);
            Assert.IsTrue(ofIx >= 0);
            return int.Parse(name.Substring(ofIx + PREFIX.Length));
        }

        /// <summary>
        /// Thread synchronization support.
        /// </summary>
        protected static void ResetSemaphore()
        {
            s_started = false;
        }

        /// <summary>
        /// Thread synchronization support.
        /// </summary>
        protected static void WaitForSemaphore()
        {
            lock (SEMAPHORE)
            {
                while (!s_started)
                {
                    try
                    {
                        Monitor.Wait(SEMAPHORE);
                    }
                    catch (ThreadInterruptedException) {/*do nothing*/}
                }
            }
        }

        /// <summary>
        /// Test the cache content.
        /// </summary>
        /// <param name="cache">
        /// The cache to test.
        /// </param>
        /// <param name="cnt">
        /// The count.
        /// </param>
        private static void TestCacheContent(IDictionary cache, int cnt)
        {
            TestCacheContent(cache, cnt, null);
        }

        /// <summary>
        /// Test the cache content.
        /// </summary>
        /// <param name="cache">
        /// The cache to test.
        /// </param>
        /// <param name="cnt">
        /// The count.
        /// </param>
        /// <param name="expected">
        /// The expected value.
        ///
        private static void TestCacheContent(IDictionary cache, int cnt, Object expected)
        {
            Assert.AreEqual(cnt, cache.Count);
            for (int i = 0; i < cnt; i++)
            {
                Object value = cache[i];
                Assert.AreEqual(expected == null ? i.ToString() : expected, value);
            }
        }

        /// <summary>
        /// Fill the specified dictionary with <Integer, String> entries.
        /// </summary>
        /// <param name="cache">
        /// The cache to fill.
        /// </param>
        /// <param name="cnt">
        /// The count.
        /// </param>
        private static void Fill(INamedCache cache, int cnt)
        {
            var mapTemp = new Hashtable();
            for (int i = 0; i <= cnt; ++i)
            {
                mapTemp.Add(i, i.ToString());
            }

            cache.InsertAll(mapTemp);
        }

        // ----- fields and constants -------------------------------------------

        const           String      CACHE_NAME = "dist-bundling-cache";
        const           String      PREFIX     = "Thread-";
        static readonly Object      SEMAPHORE  = new Object();
        const           int         THREADS    = 25;
        private const   int         COUNT      = 1000;
        volatile static bool        s_started;
        private         INamedCache m_cache;
    }
}