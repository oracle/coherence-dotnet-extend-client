/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Specialized;

using NUnit.Framework;
using Tangosol.Net.Cache;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl.NamedCache;
using Tangosol.Util;
using Tangosol.Util.Comparator;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;

namespace Tangosol.Net.Impl {

    [TestFixture]
    public class RemoteNamedCacheTests
    {

        NameValueCollection appSettings = TestUtils.AppSettings;

        protected virtual String TestCacheName
        {
            get { return "cacheName"; }
        }

        protected String CacheName
        {
            get { return m_cacheName == null ? appSettings.Get(TestCacheName) : m_cacheName; }
            set { m_cacheName = value; }
        }

        /// <summary>
        /// Convert the passed in key to a (potentially) different object. This
        /// allows classes which extend RemoteNamedCacheTests to use different
        /// key classes.
        /// </summary>
        /// <param name="o">The initial key object.</param>
        /// <returns>The key object.</returns>
        protected virtual Object GetKeyObject(Object o)
        {
            return o;
        }

        /// <summary>
        /// Get a named cache given the cache name.
        /// </summary>
        /// <param name="CacheName">The name of the cache.</param>
        /// <returns>The named cache.</returns>
        protected virtual INamedCache GetCache(String CacheName)
        {
            INamedCache cache = null;

            try
            {
                cache = CacheFactory.GetCache(CacheName);
            }
            catch (ConnectionException e)
            {
                // occasionally, we may get exception from .NET SSPI; wait for a few seconds and try again
                Blocking.Sleep(3000);
                cache = CacheFactory.GetCache(CacheName);
            }

        return cache;
        }

        [Test]
        public virtual void TestInitialize()
        {
            INamedCache cache = GetCache(CacheName);
            Assert.IsNotNull(cache);
            Assert.IsInstanceOf(typeof(SafeNamedCache), cache);
            Assert.IsInstanceOf(typeof(SafeCacheService), cache.CacheService);
            Assert.AreEqual(cache.CacheName, CacheName);
            Assert.IsTrue(cache.IsActive);
            Assert.IsTrue(cache.CacheService.IsRunning);

            SafeNamedCache safeCache = (SafeNamedCache) cache;
            Assert.IsInstanceOf(typeof(RemoteNamedCache), safeCache.NamedCache);
            Assert.IsInstanceOf(typeof(RemoteCacheService), safeCache.SafeCacheService.CacheService);

            CacheFactory.ReleaseCache(cache);
            Assert.IsFalse(cache.IsActive);

            CacheFactory.Shutdown();
            Assert.AreEqual(cache.CacheName, CacheName);
            Assert.IsFalse(cache.CacheService.IsRunning);
        }

        [Test]
        public virtual void TestNamedCacheProperties()
        {
            INamedCache cache = GetCache(CacheName);
            string key = "testNamedCachePropertiesKey";
            string value = "testNamedCachePropertiesValue";

            // INamedCache
            Assert.IsTrue(cache.IsActive);

            cache.Clear();
            Assert.AreEqual(cache.Count, 0);

            cache.Insert(GetKeyObject(key), value);
            Assert.AreEqual(cache.Count, 1);
            Assert.AreEqual(cache[GetKeyObject(key)], value);

            // SafeNamedCache
            SafeNamedCache safeCache = (SafeNamedCache) cache;
            Assert.IsFalse(safeCache.IsReleased);
            Assert.IsFalse(safeCache.IsFixedSize);
            Assert.IsFalse(safeCache.IsReadOnly);
            Assert.IsTrue(safeCache.IsSynchronized);

            // RemoteNamedCache
            RemoteNamedCache remoteCache = (RemoteNamedCache) safeCache.NamedCache;
            Assert.IsTrue(remoteCache.Channel.IsOpen);
            Assert.IsInstanceOf(typeof(NamedCacheProtocol), remoteCache.Protocol);

            CacheFactory.ReleaseCache(cache);
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestNamedCacheMethods()
        {
            INamedCache cache = GetCache(CacheName);
            string key = "testNamedCacheInterfaceMethodsKey";
            string value = "testNamedCacheInterfaceMethodsValue";

            object[] keys = {GetKeyObject("key1"), GetKeyObject("key2"), GetKeyObject("key3")};
            string[] values = {"value1", "value2", "value3"};

            cache.Clear();
            Assert.IsTrue(cache.Count == 0);

            cache.Add(GetKeyObject(key), value);
            Assert.AreEqual(cache.Count, 1);
            Assert.AreEqual(cache[GetKeyObject(key)], value);
            Assert.IsTrue(cache.Contains(GetKeyObject(key)));

            object old = cache.Insert(keys[0], values[0]);
            Assert.IsNull(old);
            Assert.AreEqual(cache[keys[0]], values[0]);

            IDictionary h = new Hashtable();
            h.Add(keys[0], values[0]);
            h.Add(keys[1], values[1]);
            h.Add(keys[2], values[2]);

            cache.InsertAll(h);

            IList list = new ArrayList(keys);
            IDictionary map = cache.GetAll(list);
            Assert.AreEqual(map.Count, list.Count);
            Assert.AreEqual(cache[keys[1]], map[keys[1]]);

            cache.Remove(GetKeyObject(key));
            Assert.IsNull(cache[GetKeyObject(key)]);

            Binary bin = new Binary(new byte[] {1, 2, 3});
            cache.Insert(GetKeyObject("key4"), bin);
            object o = cache[GetKeyObject("key4")];
            Assert.IsNotNull(o);
            Assert.IsInstanceOf(typeof (Binary), o);
            Assert.AreEqual(bin.Length, ((Binary) o).Length);

            CacheFactory.ReleaseCache(cache);
            CacheFactory.Shutdown();
        }

        [Test]
        public virtual void TestNamedCacheLock()
        {
            INamedCache cache = GetCache(CacheName);
            string key = "testNamedCacheInterfaceMethodsKey";
            string value = "testNamedCacheInterfaceMethodsValue";

            object[] keys = {GetKeyObject("key1"), GetKeyObject("key2"), GetKeyObject("key3")};
            string[] values = {"value1", "value2", "value3"};

            cache.Clear();
            Assert.IsTrue(cache.Count == 0);

            cache.Add(GetKeyObject(key), value);
            Assert.AreEqual(cache.Count, 1);
            Assert.AreEqual(cache[GetKeyObject(key)], value);
            Assert.IsTrue(cache.Contains(GetKeyObject(key)));

            cache.Lock(key);
            Assert.AreEqual(cache[GetKeyObject(key)], value);
            cache.Unlock(key);

            cache.Lock(key);
            Assert.AreEqual(cache[GetKeyObject(key)], value);
            cache.Unlock(key);

            CacheFactory.ReleaseCache(cache);
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestNamedCacheKeysCollection()
        {
            INamedCache cache = GetCache(CacheName);
            object[] keys = { GetKeyObject("key1"), GetKeyObject("key2"), GetKeyObject("key3"), GetKeyObject("key4") };
            string[] values = { "value1", "value2", "value3", "value4" };
            cache.Clear();
            IDictionary h = new Hashtable();
            h.Add(keys[0], values[0]);
            h.Add(keys[1], values[1]);
            h.Add(keys[2], values[2]);
            h.Add(keys[3], values[3]);
            cache.InsertAll(h);

            ICollection collKeys = cache.Keys;
            Assert.AreEqual(4, collKeys.Count);

            IEnumerator e = collKeys.GetEnumerator();
            for (; e.MoveNext();)
            {
                Assert.IsTrue(h.Contains(e.Current));
            }

            Object[] oa = new object[8];
            collKeys.CopyTo(oa, 3);
            Assert.IsNull(oa[0]);
            Assert.IsNull(oa[1]);
            Assert.IsNull(oa[2]);
            Assert.IsTrue(oa[3].Equals(GetKeyObject("key1")) || oa[3].Equals(GetKeyObject("key2"))
                    || oa[3].Equals(GetKeyObject("key3")) || oa[3].Equals(GetKeyObject("key4")));
            Assert.IsTrue(oa[4].Equals(GetKeyObject("key1")) || oa[4].Equals(GetKeyObject("key2"))
                    || oa[4].Equals(GetKeyObject("key3")) || oa[4].Equals(GetKeyObject("key4")));

            CacheFactory.ReleaseCache(cache);
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestNamedCacheValuesCollection()
        {
            INamedCache cache = GetCache(CacheName);
            object[] keys = { GetKeyObject("key1"), GetKeyObject("key2"), GetKeyObject("key3"), GetKeyObject("key4") };
            string[] values = { "value1", "value2", "value3", "value4" };
            cache.Clear();
            IDictionary h = new Hashtable();
            h.Add(keys[0], values[0]);
            h.Add(keys[1], values[1]);
            h.Add(keys[2], values[2]);
            h.Add(keys[3], values[3]);
            cache.InsertAll(h);

            ICollection collValues = cache.Values;

            IEnumerator e = collValues.GetEnumerator();
            ArrayList al = new ArrayList(h.Values);
            for (; e.MoveNext();)
            {
                Assert.IsTrue(al.Contains(e.Current));
            }

            Object[] oa = new object[5];
            collValues.CopyTo(oa, 1);
            Assert.IsNull(oa[0]);
            Assert.IsTrue(oa[1].Equals("value1") || oa[1].Equals("value2") || oa[1].Equals("value3") || oa[1].Equals("value4"));
            Assert.IsTrue(oa[2].Equals("value1") || oa[2].Equals("value2") || oa[2].Equals("value3") || oa[2].Equals("value4"));
            Assert.IsTrue(oa[3].Equals("value1") || oa[3].Equals("value2") || oa[3].Equals("value3") || oa[3].Equals("value4"));
            Assert.IsTrue(oa[4].Equals("value1") || oa[4].Equals("value2") || oa[4].Equals("value3") || oa[4].Equals("value4"));

            CacheFactory.ReleaseCache(cache);
            CacheFactory.Shutdown();
        }

        [Test]
        public virtual void TestNamedCacheEntryCollection()
        {
            INamedCache cache = GetCache(CacheName);
            object[] keys = { GetKeyObject("key1"), GetKeyObject("key2"), GetKeyObject("key3"), GetKeyObject("key4") };
            string[] values = { "value1", "value2", "value3", "value4" };
            cache.Clear();
            IDictionary h = new Hashtable();
            h.Add(keys[0], values[0]);
            h.Add(keys[1], values[1]);
            h.Add(keys[2], values[2]);
            h.Add(keys[3], values[3]);
            cache.InsertAll(h);

            SafeNamedCache safeCache = (SafeNamedCache) cache;
            IDictionaryEnumerator de = safeCache.NamedCache.GetEnumerator();
            ArrayList al = new ArrayList(h.Values);
            for(; de.MoveNext();)
            {
                Assert.IsTrue(al.Contains(de.Value));
                Assert.IsTrue(h.Contains(de.Key));
            }

            CacheFactory.ReleaseCache(cache);
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestNamedCacheIndex()
        {
            INamedCache cache = GetCache(CacheName);
            IValueExtractor extractor = IdentityExtractor.Instance;
            cache.Clear();

            cache.AddIndex(extractor, false, null);

            cache.RemoveIndex(extractor);

            IComparer comparer = new SafeComparer();
            extractor = new KeyExtractor(IdentityExtractor.Instance);
            cache.AddIndex(extractor, true, comparer);

            cache.RemoveIndex(extractor);

            CacheFactory.ReleaseCache(cache);
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestRemoteNamedCacheDispose()
        {
            INamedCache cache;
            object[] keys = { GetKeyObject("key1"), GetKeyObject("key2"), GetKeyObject("key3"), GetKeyObject("key4") };
            string[] values = { "value1", "value2", "value3", "value4" };
            using(cache = GetCache(CacheName))
            {
                cache.Clear();
                IDictionary h = new Hashtable();
                h.Add(keys[0], values[0]);
                h.Add(keys[1], values[1]);
                h.Add(keys[2], values[2]);
                h.Add(keys[3], values[3]);
                cache.InsertAll(h);

                foreach(object key in cache.Keys)
                {
                    Assert.IsTrue(cache.Contains(key));
                }
            }
            //after disposal
            Assert.IsFalse(cache.IsActive);
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestRemoteNamedCacheGetAllWithSameKeys()
        {
            INamedCache cache = GetCache(CacheName);
            cache.Clear();
            IDictionary dict = new Hashtable();
            for (int i = 0; i < 10; i++)
            {
                dict.Add(GetKeyObject("key"+i), "value"+i);
            }
            cache.InsertAll(dict);
            object[] keys = {GetKeyObject("key1"), GetKeyObject("key2"), GetKeyObject("key1"), GetKeyObject("key1")};
            IDictionary result = cache.GetAll(keys);
            Assert.AreEqual(2, result.Count);
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestCacheTriggerListener()
        {
            INamedCache cache = GetCache(CacheName);
            cache.Clear();

            FilterTrigger ftRollback = new FilterTrigger(NeverFilter.Instance, FilterTrigger.ActionCode.Rollback);
            FilterTrigger ftIgnore = new FilterTrigger(NeverFilter.Instance, FilterTrigger.ActionCode.Ignore);
            FilterTrigger ftRemove = new FilterTrigger(NeverFilter.Instance, FilterTrigger.ActionCode.Remove);

            IDictionary dict = new Hashtable();
            for (int i = 0; i < 10; i++)
            {
                dict.Add(GetKeyObject("key" + i), "value" + i);
            }
            cache.InsertAll(dict);

            CacheTriggerListener listener = new CacheTriggerListener(ftIgnore);
            cache.AddCacheListener(listener);
            Assert.AreEqual(cache[GetKeyObject("key1")], "value1");
            cache[GetKeyObject("key1")] = "newvalue";
            Assert.AreEqual(cache[GetKeyObject("key1")], "value1");
            cache.RemoveCacheListener(listener);

            listener = new CacheTriggerListener(ftRemove);
            cache.AddCacheListener(listener);
            Assert.IsTrue(cache.Contains(GetKeyObject("key1")));
            cache[GetKeyObject("key1")] = "newvalue";
            Assert.IsFalse(cache.Contains(GetKeyObject("key1")));
            cache.RemoveCacheListener(listener);

            listener = new CacheTriggerListener(ftRollback);
            cache.AddCacheListener(listener);
            Assert.AreEqual(cache[GetKeyObject("key3")], "value3");
            Exception e = null;
            try
            {
                cache[GetKeyObject("key3")] = "newvalue";
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.AreEqual(cache[GetKeyObject("key3")], "value3");
            cache.RemoveCacheListener(listener);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestGetKeys()
        {
            INamedCache cache = GetCache(CacheName);
            cache.Clear();

            IDictionary dict = new Hashtable();
            for (int i = 1; i <= 10; i++)
            {
                dict.Add(GetKeyObject("key" + i), i);
            }
            cache.InsertAll(dict);

            IFilter filter = new GreaterFilter(IdentityExtractor.Instance, 5);
            object[] keys = cache.GetKeys(filter);
            Assert.AreEqual(keys.Length, 5);
            for (int i = 6; i <= 10; i++)
            {
                Assert.Contains(GetKeyObject("key" + i), keys);
            }

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestGetValues()
        {
            INamedCache cache = GetCache(CacheName);
            cache.Clear();

            IDictionary dict = new Hashtable();
            for (int i = 1; i <= 10; i++)
            {
                dict.Add(GetKeyObject("key" + i), i);
            }
            cache.InsertAll(dict);

            IFilter filter = new GreaterFilter(IdentityExtractor.Instance, 5);
            object[] values = cache.GetValues(filter);
            Assert.AreEqual(values.Length, 5);
            for (int i = 6; i <= 10; i++)
            {
                Assert.Contains(i, values);
            }

            values = cache.GetValues(filter, SafeComparer.Instance);
            Assert.AreEqual(values.Length, 5);
            for (int i = 6; i <= 10; i++)
            {
                Assert.Contains(i, values);
            }
            Assert.AreEqual(values[0], 6);
            Assert.AreEqual(values[1], 7);
            Assert.AreEqual(values[2], 8);
            Assert.AreEqual(values[3], 9);
            Assert.AreEqual(values[4], 10);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestGetEntries()
        {
            INamedCache cache = GetCache(CacheName);
            cache.Clear();

            IDictionary dict = new Hashtable();
            for (int i = 1; i <= 10; i++)
            {
                dict.Add(GetKeyObject("key" + i), i);
            }
            cache.InsertAll(dict);

            IFilter filter = new GreaterFilter(IdentityExtractor.Instance, 5);
            ICacheEntry[] entries = cache.GetEntries(filter);
            Assert.AreEqual(entries.Length, 5);
            object[] keys = new object[5];
            object[] values = new object[5];
            for (int i = 0; i < 5; i++)
            {
                ICacheEntry entry = entries[i];
                keys[i] = entry.Key;
                values[i] = entry.Value;
            }
            for (int i = 6; i <= 10; i++)
            {
                Assert.Contains(GetKeyObject("key" + i), keys);
                Assert.Contains(i, values);
            }

            entries = cache.GetEntries(filter, SafeComparer.Instance);
            Assert.AreEqual(entries.Length, 5);
            keys = new object[5];
            values = new object[5];
            for (int i = 0; i < 5; i++)
            {
                ICacheEntry entry = entries[i];
                keys[i] = entry.Key;
                values[i] = entry.Value;
            }
            for (int i = 6; i <= 10; i++)
            {
                Assert.Contains(GetKeyObject("key" + i), keys);
                Assert.Contains(i, values);
            }
            Assert.AreEqual(values[0], 6);
            Assert.AreEqual(values[1], 7);
            Assert.AreEqual(values[2], 8);
            Assert.AreEqual(values[3], 9);
            Assert.AreEqual(values[4], 10);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestListeners()
        {
            INamedCache namedCache = GetCache(CacheName);

            Hashtable ht = new Hashtable();
            ht.Add(GetKeyObject("Key1"), 435);
            ht.Add(GetKeyObject("Key2"), 253);
            ht.Add(GetKeyObject("Key3"), 3);
            ht.Add(GetKeyObject("Key4"), 200);
            ht.Add(GetKeyObject("Key5"), 333);
            namedCache.InsertAll(ht);

            IFilter greaterThan300 = new GreaterFilter(IdentityExtractor.Instance, 300);
            IFilter listenerFilter = new CacheEventFilter(new GreaterFilter(IdentityExtractor.Instance, 350));
            ContinuousQueryCache queryCache = new ContinuousQueryCache(namedCache, greaterThan300);
            Listener listener = new SyncListener();

            // listener
            queryCache.AddCacheListener(listener);

            listener.CacheEvent = null;
            queryCache.Insert(GetKeyObject("Key7"), 400);
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Inserted, listener.CacheEvent.EventType);

            listener.CacheEvent = null;
            queryCache.Insert(GetKeyObject("Key7"), 350);
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Updated, listener.CacheEvent.EventType);

            listener.CacheEvent = null;
            queryCache.Remove(GetKeyObject("Key5"));
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Deleted, listener.CacheEvent.EventType);

            queryCache.RemoveCacheListener(listener);

            // listener, key, lite
            namedCache.Clear();
            namedCache.InsertAll(ht);
            queryCache.AddCacheListener(listener, GetKeyObject("Key5"), false);

            listener.CacheEvent = null;
            queryCache.Insert(GetKeyObject("Key6"), 400);
            Assert.IsNull(listener.CacheEvent);

            queryCache.Insert(GetKeyObject("Key5"), 400);
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Updated, listener.CacheEvent.EventType);

            listener.CacheEvent = null;
            queryCache.Remove(GetKeyObject("Key1"));
            Assert.IsNull(listener.CacheEvent);

            queryCache.Remove(GetKeyObject("Key5"));
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Deleted, listener.CacheEvent.EventType);

            queryCache.RemoveCacheListener(listener, GetKeyObject("Key5"));

            // listener, filter, lite
            namedCache.Clear();
            namedCache.InsertAll(ht);
            queryCache.AddCacheListener(listener, listenerFilter, false);

            listener.CacheEvent = null;
            queryCache.Insert(GetKeyObject("Key6"), 320);
            Assert.IsNull(listener.CacheEvent);

            queryCache.Insert(GetKeyObject("Key5"), 350);
            Assert.IsNull(listener.CacheEvent);

            queryCache.Insert(GetKeyObject("Key6"), 400);
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Updated, listener.CacheEvent.EventType);

            listener.CacheEvent = null;
            queryCache.Insert(GetKeyObject("Key7"), 340);
            Assert.IsNull(listener.CacheEvent);

            queryCache.Remove(GetKeyObject("Key7"));
            Assert.IsNull(listener.CacheEvent);

            queryCache.Remove(GetKeyObject("Key6"));
            Assert.IsNotNull(listener.CacheEvent);
            Assert.AreEqual(CacheEventType.Deleted, listener.CacheEvent.EventType);

            queryCache.RemoveCacheListener(listener, listenerFilter);

            // non-sync listener, filter, heavy
            // COH-2529: Filter-based cache events are reevaluated on the client unncessarily
            listener       = new Listener();
            listenerFilter = new CacheEventFilter(new EqualsFilter("getZip", "02144"));
            namedCache.Clear();
            namedCache.AddCacheListener(listener, listenerFilter, false);

            listener.CacheEvent = null;
            namedCache[GetKeyObject("Jason")] = new Address("3 TBR #8", "Somerville", "MA", "02144");
            listener.waitForEvent(5000);
            Assert.IsNotNull(listener.CacheEvent);
            Assert.IsNotNull(listener.CacheEvent.NewValue);
            Assert.AreEqual(CacheEventType.Inserted, listener.CacheEvent.EventType);

            listener.CacheEvent = null;
            listener.waitForEvent(5000);
            namedCache[GetKeyObject("Oracle")] = new Address("8 NEEP", "Burlington", "MA", "01803");
            Assert.IsNull(listener.CacheEvent);

            namedCache.RemoveCacheListener(listener, listenerFilter);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestGetOrDefault()
        {
            INamedCache cache = GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key4", 0);
            ht.Add("key3", -10);
            ht.Add("key2", 45);
            ht.Add("key1", 398);
            cache.InsertAll(ht);

            object result = cache.GetOrDefault("key1", 400);
            Assert.AreEqual(398, result);

            result = cache.GetOrDefault("key5", 400);
            Assert.AreEqual(400, result);
        }

        [Test]
        public void TestPutIfAbsent()
        {
            INamedCache cache = GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key4", 0);
            ht.Add("key3", -10);
            ht.Add("key2", 45);
            ht.Add("key1", 398);
            cache.InsertAll(ht);

            object result = cache.InsertIfAbsent("key1", 400);
            Assert.AreEqual(398, result);

            result = cache.InsertIfAbsent("key5", 400);
            Assert.AreEqual(null, result);
            Assert.IsTrue(cache.Contains("key5"));
        }

        [Test]
        public void TestRemove()
        {
            INamedCache cache = GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key4", 0);
            ht.Add("key3", -10);
            ht.Add("key2", 45);
            ht.Add("key1", 398);
            cache.InsertAll(ht);

            object result = cache.Remove("key1", 398);
            Assert.AreEqual(true, result);
            Assert.AreEqual(3, cache.Count);
        }

        [Test]
        public void TestReplace()
        {
            INamedCache cache = GetCache(CacheName);
            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("key4", 0);
            ht.Add("key3", -10);
            ht.Add("key2", 45);
            ht.Add("key1", 398);
            cache.InsertAll(ht);

            object result = cache.Replace("key1", 400);
            Assert.AreEqual(398, result);
            Assert.AreEqual(400, cache["key1"]);

            result = cache.Replace("key1", 300, 450);
            Assert.AreEqual(false, result);

            result = cache.Replace("key1", 400, 450);
            Assert.AreEqual(true, result);
            Assert.AreEqual(450, cache["key1"]);
        }

        [Test]
        public void TestExpiry()
        {
            INamedCache cache = GetCache(CacheName);

            TestCacheListener listener = new TestCacheListener();
            cache.AddCacheListener(listener, 1, false);

            cache.Add(1, 1);
            listener.WaitForEvent();

            // synthetic event
            cache.Invoke(1, new TestEntryProcessor(true));
            CacheEventArgs evt = listener.WaitForEvent();

            Assert.AreEqual(true, evt.IsSynthetic);
            Assert.AreEqual(false, evt.IsExpired);
            listener.ClearEvent();

            cache.Insert(1, 2, 2000);
            listener.WaitForEvent();
            listener.ClearEvent();
            
            // wait for synthetic delete due to expiry
            evt = listener.WaitForEvent(3000L);

            Assert.AreEqual(true, evt.IsSynthetic);
            Assert.AreEqual(true, evt.IsExpired);

            cache.Add(1, 1);
            listener.WaitForEvent();
            listener.ClearEvent();

            cache.Remove(1);

            // regular event
            evt = listener.WaitForEvent();

            Assert.AreEqual(false, evt.IsSynthetic);
            Assert.AreEqual(false, evt.IsExpired);
        }

        /// <summary>
        /// The cache name.
        /// </summary>
        protected string m_cacheName;
    }
}