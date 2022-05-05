/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using NUnit.Framework;
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Net.Impl;
using Tangosol.Util.Aggregator;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;
using Tangosol.Util.Processor;

namespace Tangosol.Util
{
    [TestFixture]
    public class ConverterCollectionsTests
    {
        [Test]
        public void TestConvertArray()
        {
            object[] a1 = new object[] {1, 2, 3};
            IConverter c1 = new ConvertDown();
            ConverterCollections.ConvertArray(a1, c1);
            Assert.AreEqual(a1.Length, 3);
            for (int i = 0; i < a1.Length; i++)
            {
                Assert.AreEqual(a1[i], c1.Convert(i + 1));
            }

            a1 = new object[] { 1, 2, 3 };
            object[] a2 = new object[0];
            //destination array is not large enough
            Assert.Greater(a1.Length, a2.Length);
            object[] a3 = ConverterCollections.ConvertArray(a1, c1, a2);
            Assert.AreEqual(a2.Length, 0);
            Assert.AreEqual(a1.Length, a3.Length);
            for (int i = 0; i < a1.Length; i++)
            {
                Assert.AreEqual(a3[i], c1.Convert(a1[i]));
            }

            a1 = new object[] { 1, 2, 3 };
            a2 = new object[3];
            //destination array is large enough
            Assert.AreEqual(a1.Length, a2.Length);
            a3 = ConverterCollections.ConvertArray(a1, c1, a2);
            Assert.AreEqual(a1.Length, a2.Length);
            Assert.AreEqual(a1.Length, a3.Length);
            for (int i = 0; i < a1.Length; i++)
            {
                Assert.AreEqual(a2[i], c1.Convert(a1[i]));
                Assert.AreEqual(a3[i], c1.Convert(a1[i]));
            }

            a1 = new object[] { 1, 2, 3 };
            a2 = new object[5];
            //destination array is larger than source
            Assert.Less(a1.Length, a2.Length);
            a3 = ConverterCollections.ConvertArray(a1, c1, a2);
            Assert.Less(a1.Length, a2.Length);
            Assert.AreEqual(a2.Length, a3.Length);
            for (int i = 0; i < a1.Length; i++)
            {
                Assert.AreEqual(a2[i], c1.Convert(a1[i]));
                Assert.AreEqual(a3[i], c1.Convert(a1[i]));
            }
            Assert.IsNull(a2[3]);
        }

        [Test]
        public void ConverterCollectionTests()
        {
            ArrayList list = new ArrayList();
            IConverter cDown = new ConvertDown();
            IConverter cUp = new ConvertUp();
            list.Add(cDown.Convert(1));
            list.Add(cDown.Convert(2));
            list.Add(cDown.Convert(3));

            ICollection convCol = ConverterCollections.GetCollection(list, cUp, cDown);

            Assert.IsNotNull(convCol);
            Assert.AreEqual(convCol.Count, list.Count);
            Assert.AreEqual(convCol.IsSynchronized, list.IsSynchronized);
            Assert.AreEqual(convCol.SyncRoot, list.SyncRoot);

            object[] a = new object[convCol.Count];
            convCol.CopyTo(a, 0);
            Assert.AreEqual(a.Length, convCol.Count);
            for (int i = 0; i < convCol.Count; i++)
            {
                Assert.AreEqual(a[i], cUp.Convert(list[i]));
            }

            foreach (object o in convCol)
            {
                Assert.IsTrue(list.Contains(cDown.Convert(o)));
            }

            Assert.IsInstanceOf(typeof(ConverterCollections.ConverterCollection), convCol);
            ConverterCollections.ConverterCollection cc = convCol as ConverterCollections.ConverterCollection;
            Assert.IsNotNull(cc);
            Assert.AreEqual(cc.Collection, list);
            Assert.AreEqual(cc.ConverterDown, cDown);
            Assert.AreEqual(cc.ConverterUp, cUp);

            cc.Invalidate();
            Assert.IsNull(cc.Collection);
            Assert.IsNull(cc.ConverterDown);
            Assert.IsNull(cc.ConverterUp);
        }

        [Test]
        public void ConverterDictionaryEnumeratorTests()
        {
            IDictionary dict = new Hashtable();
            IConverter conv = new ConvertDown();
            for (int i = 0; i < 3; i++)
            {
                dict.Add(i, i + 1);
            }
            IDictionaryEnumerator enmr = dict.GetEnumerator();
                        
            IDictionaryEnumerator convEnum = ConverterCollections.GetDictionaryEnumerator(enmr, conv, conv);

            Assert.IsNotNull(convEnum);
            Assert.IsTrue(convEnum.MoveNext());
            convEnum.MoveNext();
            convEnum.MoveNext();
            Assert.IsFalse(convEnum.MoveNext());
            convEnum.Reset();
            Assert.IsTrue(convEnum.MoveNext());

            object o = convEnum.Current;
            DictionaryEntry entry = convEnum.Entry;
            Assert.AreEqual(o, entry);

            Assert.AreEqual(entry.Key, convEnum.Key);
            Assert.AreEqual(entry.Value, convEnum.Value);
            Assert.AreEqual(entry.Key, conv.Convert(enmr.Key));
            Assert.AreEqual(entry.Value, conv.Convert(enmr.Value));

            Assert.IsInstanceOf(typeof (ConverterCollections.ConverterDictionaryEnumerator), convEnum);
            ConverterCollections.ConverterDictionaryEnumerator cde =
                convEnum as ConverterCollections.ConverterDictionaryEnumerator;
            Assert.IsNotNull(cde);
            Assert.AreEqual(cde.ConverterKeyUp, conv);
            Assert.AreEqual(cde.ConverterValueUp, conv);
        }

        [Test]
        public void ConverterDictionaryTests()
        {
            IDictionary dict = new Hashtable();
            IConverter cDown = new ConvertDown();
            IConverter cUp = new ConvertUp();
            dict.Add(cDown.Convert(0), cDown.Convert(1));

            IDictionary convDict = ConverterCollections.GetDictionary(dict, cUp, cDown, cUp, cDown);

            Assert.IsNotNull(convDict);
            Assert.AreEqual(convDict.Count, dict.Count);
            Assert.AreEqual(convDict.IsFixedSize, dict.IsFixedSize);
            Assert.AreEqual(convDict.IsReadOnly, dict.IsReadOnly);
            Assert.AreEqual(convDict.IsSynchronized, dict.IsSynchronized);
            Assert.AreEqual(convDict.SyncRoot, dict.SyncRoot);

            Assert.IsTrue(convDict.Contains(0));
            Assert.IsTrue(dict.Contains(cDown.Convert(0)));
            Assert.IsFalse(convDict.Contains(1));

            Assert.AreEqual(cUp.Convert(dict[cDown.Convert(0)]), convDict[0]);
            convDict[0] = "2";
            Assert.AreEqual(cUp.Convert(dict[cDown.Convert(0)]), convDict[0]);

            convDict.Add("ana", "cikic");
            Assert.AreEqual(dict.Count, 2);
            Assert.IsTrue(dict.Contains(cDown.Convert("ana")));
            Assert.AreEqual(cUp.Convert(dict[cDown.Convert("ana")]), "cikic");

            convDict.Remove("ana");
            Assert.AreEqual(dict.Count, 1);

            Assert.IsInstanceOf(typeof(ConverterCollections.ConverterDictionary), convDict);
            ConverterCollections.ConverterDictionary cd = convDict as ConverterCollections.ConverterDictionary;
            Assert.IsNotNull(cd);
            Assert.AreEqual(cd.ConverterKeyDown, cDown);
            Assert.AreEqual(cd.ConverterKeyUp, cUp);
            Assert.AreEqual(cd.ConverterValueDown, cDown);
            Assert.AreEqual(cd.ConverterValueUp, cUp);
            Assert.AreEqual(cd.Dictionary, dict);

            convDict.Clear();
            Assert.AreEqual(convDict.Count, 0);
            Assert.AreEqual(convDict.Count, dict.Count);

            for (int i = 0; i < 5; i++)
            {
                convDict.Add(i, i + 1);
            }
            Assert.AreEqual(convDict.Count, 5);

            ArrayList keys = new ArrayList(convDict.Keys);
            Assert.AreEqual(keys.Count, 5);
            ArrayList values = new ArrayList(convDict.Values);
            Assert.AreEqual(values.Count, 5);

            object[] entries = new object[convDict.Count];
            int c = 0;
            foreach (object o in convDict)
            {
                Assert.IsInstanceOf(typeof (DictionaryEntry), o);
                DictionaryEntry entry = (DictionaryEntry) o;
                Assert.IsNotNull(entry);
                entries[c++] = entry;
                Assert.IsTrue(keys.Contains(entry.Key));
                Assert.IsTrue(values.Contains(entry.Value));
            }

            object[] a = new object[convDict.Count];
            convDict.CopyTo(a, 0);
            Assert.IsTrue(CollectionUtils.EqualsDeep(a, entries));
        }

        [Test]
        public void ConverterCacheEnumeratorTests()
        {
            ICache cache = InstantiateCache();
            IConverter conv = new ConvertDown();
            for (int i = 0; i < 3; i++)
            {
                cache.Add(i, i + 1);
            }
            ICacheEnumerator cacheEnumerator = cache.GetEnumerator();

            ICacheEnumerator convEnum = ConverterCollections.GetCacheEnumerator(cacheEnumerator, conv, conv, conv);

            Assert.IsNotNull(convEnum);
            Assert.IsTrue(convEnum.MoveNext());
            convEnum.MoveNext();
            convEnum.MoveNext();
            Assert.IsFalse(convEnum.MoveNext());
            convEnum.Reset();
            Assert.IsTrue(convEnum.MoveNext());

            object o = convEnum.Current;
            ICacheEntry entry = convEnum.Entry;
            Assert.AreEqual(o, entry);

            Assert.AreEqual(entry.Key, convEnum.Key);
            Assert.AreEqual(entry.Value, convEnum.Value);
            Assert.AreEqual(entry.Key, conv.Convert(cacheEnumerator.Key));
            Assert.AreEqual(entry.Value, conv.Convert(cacheEnumerator.Value));

            Assert.IsInstanceOf(typeof(ConverterCollections.ConverterCacheEnumerator), convEnum);
            ConverterCollections.ConverterCacheEnumerator cce =
                convEnum as ConverterCollections.ConverterCacheEnumerator;
            Assert.IsNotNull(cce);
            Assert.AreEqual(cce.ConverterKeyUp, conv);
            Assert.AreEqual(cce.ConverterValueUp, conv);
        }

        [Test]
        public void ConverterCacheEntriesTests()
        {
            ICache cache = InstantiateCache();
            IConverter cDown = new ConvertDown();
            IConverter cUp = new ConvertUp();
            for (int i = 0; i < 3; i++)
            {
                cache.Add(cDown.Convert(i), cDown.Convert(i + 1));
            }
            
            ICollection convEntries = ConverterCollections.GetCacheEntries(cache.Entries, cUp, cDown, cUp, cDown);

            Assert.IsNotNull(convEntries);
            Assert.AreEqual(convEntries.Count, 3);
            ArrayList list = new ArrayList(convEntries);
            Assert.AreEqual(list.Count, 3);
            object o = list[0];
            Assert.IsNotNull(o);
            Assert.IsInstanceOf(typeof (ICacheEntry), o);
            ICacheEntry entry = o as ICacheEntry;
            Assert.IsNotNull(entry);
            Assert.AreEqual(Convert.ToInt32(entry.Key) + 1, Convert.ToInt32(entry.Value));

            IEnumerator enumerator = convEntries.GetEnumerator();
            Assert.IsNotNull(enumerator);
            enumerator.Reset();
            Assert.IsTrue(enumerator.MoveNext());
            o = enumerator.Current;
            Assert.IsNotNull(o);
            Assert.IsInstanceOf(typeof(ICacheEntry), o);
            entry = o as ICacheEntry;
            Assert.IsNotNull(entry);
            Assert.AreEqual(Convert.ToInt32(entry.Key) + 1, Convert.ToInt32(entry.Value));
        }

        [Test]
        public void ConverterCacheTests()
        {
            ICache cache = InstantiateCache();
            IConverter cDown = new ConvertDown();
            IConverter cUp = new ConvertUp();
            for (int i = 1; i <= 3; i++)
            {
                cache.Add(cDown.Convert(i), cDown.Convert(i+1));
            }

            ICache convCache = ConverterCollections.GetCache(cache, cUp, cDown, cUp, cDown);

            Assert.IsNotNull(convCache);
            Assert.AreEqual(convCache.Count, 3);
            convCache.Insert(4, 5);
            Assert.AreEqual(convCache.Count, 4);
            Assert.AreEqual(convCache["4"], "5");
            Assert.AreEqual(cache[cDown.Convert(4)], cDown.Convert(5));

            ICollection entries = convCache.Entries;
            Assert.IsNotNull(entries);
            Assert.AreEqual(entries.Count, 4);
            ArrayList list = new ArrayList(entries);
            ICacheEnumerator cacheEnumerator = convCache.GetEnumerator();
            Assert.IsNotNull(cacheEnumerator);
            Assert.IsTrue(cacheEnumerator.MoveNext());
            cacheEnumerator.Reset();
            for (int i = 0; i < list.Count && cacheEnumerator.MoveNext(); i++)
            {
                object o1 = list[i];
                object o2 = cacheEnumerator.Current;
                Assert.IsNotNull(o1);
                Assert.IsNotNull(o2);
                Assert.IsInstanceOf(typeof(ICacheEntry), o1);
                Assert.IsInstanceOf(typeof(ICacheEntry), o2);
                ICacheEntry e1 = o1 as ICacheEntry;
                ICacheEntry e2 = o2 as ICacheEntry;
                Assert.IsNotNull(e1);
                Assert.IsNotNull(e2);
                Assert.AreEqual(e1.Key, e2.Key);
                Assert.AreEqual(e1.Value, e2.Value);
            }

            ArrayList keys = new ArrayList();
            keys.Add(1);
            keys.Add(3);
            IDictionary result = convCache.GetAll(keys);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 2);
            Assert.AreEqual(result["1"], "2");
            Assert.AreEqual(result["3"], "4");

            IDictionary d = new Hashtable();
            for (int i = 5; i < 7; i++)
            {
                d.Add(i, i + 1);
            }
            convCache.InsertAll(d);
            Assert.AreEqual(convCache.Count, 6);
            Assert.AreEqual(convCache["6"], "7");

            Assert.IsInstanceOf(typeof(ConverterCollections.ConverterCache), convCache);
            ConverterCollections.ConverterCache cc = convCache as ConverterCollections.ConverterCache;
            Assert.IsNotNull(cc);
            Assert.AreEqual(cc.Cache, cache);
        }

        [Test]
        public void ConverterConcurrentCacheTests()
        {
            IConcurrentCache cache = InstantiateCache();
            IConverter cDown = new ConvertDown();
            IConverter cUp = new ConvertUp();

            IConcurrentCache convCache = ConverterCollections.GetConcurrentCache(cache, cUp, cDown, cUp, cDown);

            Assert.IsNotNull(convCache);
            Assert.IsInstanceOf(typeof (ConverterCollections.ConverterConcurrentCache), convCache);
            ConverterCollections.ConverterConcurrentCache cc =
                convCache as ConverterCollections.ConverterConcurrentCache;
            Assert.IsNotNull(cc);
            Assert.AreEqual(cc.ConcurrentCache, cache);
        }

        [Test]
        public void ConverterInvocableCacheTests()
        {
            IInvocableCache cache = InstantiateCache();
            IConverter cDown = new ConvertDown();
            IConverter cUp = new ConvertUp();
            IEntryProcessor processor = new ConditionalPut(AlwaysFilter.Instance, "value_converted", false);

            IInvocableCache convCache = ConverterCollections.GetInvocableCache(cache, cUp, cDown, cUp, cDown);

            Assert.IsNotNull(convCache);
            Assert.AreEqual(cache.Count, 0);
            convCache.Invoke("key", processor);
            Assert.AreEqual(cache.Count, 1);
            Assert.AreEqual(convCache["key"], cUp.Convert("value_converted"));

            ArrayList keys = new ArrayList();
            keys.Add("key");
            keys.Add("anotherkey");
            processor = new ConditionalPut(AlwaysFilter.Instance, "newvalue_converted", false);
            IDictionary result = convCache.InvokeAll(keys, processor);
            //results should be empty since return is set to false
            Assert.AreEqual(result.Count, 0);
            Assert.AreEqual(convCache.Count, 2);
            foreach (object key in result.Keys)
            {
                Assert.IsTrue(convCache.Contains(key));
                Assert.AreEqual(convCache[key], cUp.Convert("newvalue_converted"));
            }

            processor = new ConditionalPut(AlwaysFilter.Instance, "value_converted", false);
            result = convCache.InvokeAll(AlwaysFilter.Instance, processor);
            //results should be empty since return is set to false
            Assert.AreEqual(result.Count, 0);
            Assert.AreEqual(convCache.Count, 2);
            foreach (object key in result.Keys)
            {
                Assert.IsTrue(convCache.Contains(key));
                Assert.AreEqual(convCache[key], cUp.Convert("value_converted"));
            }

            convCache.Clear();
            for (int i = 0; i < 5; i++)
            {
                convCache.Add(i, i + 1);
            }

            IEntryAggregator aggregator = new ComparableMax(IdentityExtractor.Instance);
            keys.Clear();
            keys.Add("2");
            keys.Add("3");
            object o = convCache.Aggregate(keys, aggregator);
            Assert.IsNotNull(o);
            Assert.AreEqual(o, "4");

            o = convCache.Aggregate(AlwaysFilter.Instance, aggregator);
            Assert.IsNotNull(o);
            Assert.AreEqual(o, "5");

            Assert.IsInstanceOf(typeof(ConverterCollections.ConverterInvocableCache), convCache);
            ConverterCollections.ConverterInvocableCache cc =
                convCache as ConverterCollections.ConverterInvocableCache;
            Assert.IsNotNull(cc);
            Assert.AreEqual(cc.InvocableCache, cache);
        }

        [Test]
        public void ConverterNamedCacheTests()
        {
            INamedCache cache = InstantiateNamedCache();
            IConverter cDown = new ConvertDown();
            IConverter cUp = new ConvertUp();

            INamedCache convCache = ConverterCollections.GetNamedCache(cache, cUp, cDown, cUp, cDown);

            Assert.IsNotNull(convCache);
            Assert.IsInstanceOf(typeof(ConverterCollections.ConverterNamedCache), convCache);
            ConverterCollections.ConverterNamedCache cc =
                convCache as ConverterCollections.ConverterNamedCache;
            Assert.IsNotNull(cc);
            Assert.AreEqual(cc.NamedCache, cache);
            Assert.AreEqual(cc.CacheName, cache.CacheName);
            Assert.AreEqual(cc.CacheService, cache.CacheService);
            Assert.AreEqual(cc.IsActive, cache.IsActive);
        }

        [Test]
        public void ConverterObservableCacheTests()
        {
            IObservableCache cache = InstantiateCache();
            IConverter cDown = new ConvertDown();
            IConverter cUp = new ConvertUp();
            TestCacheListener listener = new TestCacheListener();

            IObservableCache convCache = ConverterCollections.GetObservableCache(cache, cUp, cDown, cUp, cDown);

            Assert.IsNotNull(convCache);
            //AddCacheListener(listener)
            convCache.AddCacheListener(listener);
            Assert.AreEqual(listener.m_inserted, 0);
            convCache.Add(1, 1);
            Assert.AreEqual(listener.m_inserted, 1);
            Assert.AreEqual(listener.m_updated, 0);
            convCache[1] = 2;
            Assert.AreEqual(listener.m_updated, 1);
            Assert.AreEqual(listener.m_deleted, 0);
            convCache.Remove(1);
            Assert.AreEqual(listener.m_deleted, 1);
            //RemoveCacheListener(listener)
            convCache.RemoveCacheListener(listener);
            Assert.AreEqual(listener.m_inserted, 1);
            convCache.Add(1, 1);
            Assert.AreEqual(listener.m_inserted, 1);

            listener.m_inserted = listener.m_updated = listener.m_deleted = 0;
            convCache.Clear();
            //AddCacheListener(listener, key, isLite);
            convCache.AddCacheListener(listener, "1", true);
            for (int i = 0; i < 3; i++ )
            {
                convCache.Add(i, i);
            }
            Assert.AreEqual(listener.m_inserted, 1);
            Assert.AreEqual(listener.m_updated, 0);
            Assert.AreEqual(listener.m_deleted, 0);
            for (int i = 0; i < 3; i++ )
            {
                convCache[i] = i + 1;
            }
            Assert.AreEqual(listener.m_inserted, 1);
            Assert.AreEqual(listener.m_updated, 1);
            Assert.AreEqual(listener.m_deleted, 0);
            convCache.Clear();
            Assert.AreEqual(listener.m_inserted, 1);
            Assert.AreEqual(listener.m_updated, 1);
            Assert.AreEqual(listener.m_deleted, 1);
            //RemoveCacheListener(listener, key)
            convCache.RemoveCacheListener(listener, "1");
            convCache.Add(1, 1);
            Assert.AreEqual(listener.m_inserted, 1);

            listener.m_inserted = listener.m_updated = listener.m_deleted = 0;
            convCache.Clear();
            IFilter filter = AlwaysFilter.Instance;
            //AddCacheListener(listener, filter, isLite)
            convCache.AddCacheListener(listener, filter, true);
            for (int i = 0; i < 3; i++)
            {
                convCache.Add(i, i);
            }
            Assert.AreEqual(listener.m_inserted, 3);
            Assert.AreEqual(listener.m_updated, 0);
            Assert.AreEqual(listener.m_deleted, 0);
            for (int i = 0; i < 3; i++)
            {
                convCache[i] = i + 1;
            }
            Assert.AreEqual(listener.m_inserted, 3);
            Assert.AreEqual(listener.m_updated, 3);
            Assert.AreEqual(listener.m_deleted, 0);
            convCache.Clear();
            Assert.AreEqual(listener.m_inserted, 3);
            Assert.AreEqual(listener.m_updated, 3);
            Assert.AreEqual(listener.m_deleted, 3);
            //RemoveCacheListener(listener, filter)
            convCache.RemoveCacheListener(listener, filter);
            convCache.Add(1, 1);
            Assert.AreEqual(listener.m_inserted, 3);

            Assert.IsInstanceOf(typeof(ConverterCollections.ConverterObservableCache), convCache);
            ConverterCollections.ConverterObservableCache coc =
                convCache as ConverterCollections.ConverterObservableCache;
            Assert.IsNotNull(coc);
            Assert.AreEqual(coc.ObservableCache, cache);
        }

        [Test]
        public void ConverterQueryCacheTests()
        {
            IQueryCache cache = InstantiateCache();
            IConverter cDown = new ConvertDown();
            IConverter cUp = new ConvertUp();

            IQueryCache convCache = ConverterCollections.GetQueryCache(cache, cUp, cDown, cUp, cDown);

            Assert.IsNotNull(convCache);
            for (int i = 0; i < 5; i++)
            {
                convCache.Add(i, i + 1);
            }
            Assert.AreEqual(convCache.Count, 5);

            IFilter filter = new EqualsFilter(IdentityExtractor.Instance, cDown.Convert("4"));
            object[] keys = convCache.GetKeys(filter);
            Assert.IsNotNull(keys);
            Assert.AreEqual(keys.Length, 1);
            Assert.AreEqual(keys[0], "3");

            filter = new GreaterEqualsFilter(IdentityExtractor.Instance, "2");
            object[] values = convCache.GetValues(filter);
            Assert.IsNotNull(values);
            Assert.AreEqual(values.Length, 4);
            values = convCache.GetValues(filter, Comparer.DefaultInvariant);
            Assert.IsNotNull(values);
            Assert.AreEqual(values.Length, 4);
            Assert.AreEqual(values[0], "2");

            object[] entries = convCache.GetEntries(filter);
            Assert.IsNotNull(entries);
            Assert.AreEqual(entries.Length, 4);
            entries = convCache.GetEntries(filter, new KeyExtractor(IdentityExtractor.Instance));
            Assert.IsNotNull(entries);
            Assert.AreEqual(entries.Length, 4);
            Assert.IsInstanceOf(typeof(ICacheEntry), entries[0]);
            ICacheEntry e = entries[0] as ICacheEntry;
            Assert.IsNotNull(e);
            Assert.AreEqual(e.Key, "1");
            Assert.AreEqual(e.Value, "2");

            Assert.IsInstanceOf(typeof(ConverterCollections.ConverterQueryCache), convCache);
            ConverterCollections.ConverterQueryCache cqc = convCache as ConverterCollections.ConverterQueryCache;
            Assert.IsNotNull(cqc);
            Assert.AreEqual(cqc.QueryCache, cache);
        }

        [Test]
        public void ConverterCacheEntryTests()
        {
            IConverter cDown = new ConvertDown();
            IConverter cUp = new ConvertUp();
            ICacheEntry entry = new CacheEntry(cDown.Convert("key"), cDown.Convert("value"));

            ICacheEntry convEntry = ConverterCollections.GetCacheEntry(entry, cUp, cUp, cDown);

            Assert.IsNotNull(convEntry);
            Assert.AreEqual(convEntry.Key, cUp.Convert(entry.Key));
            Assert.AreEqual(convEntry.Value, cUp.Convert(entry.Value));

            convEntry.Value = "newvalue";
            Assert.AreEqual(entry.Value, cDown.Convert("newvalue"));

            Assert.IsInstanceOf(typeof (ConverterCollections.ConverterCacheEntry), convEntry);
            ConverterCollections.ConverterCacheEntry ce = convEntry as ConverterCollections.ConverterCacheEntry;
            Assert.IsNotNull(ce);
            Assert.AreEqual(ce.Entry, entry);
        }

        [Test]
        public void ConverterCacheEventArgsTests()
        {
            IObservableCache cache = InstantiateCache();
            IObservableCache newCache = InstantiateCache();
            IConverter cDown = new ConvertDown();
            IConverter cUp = new ConvertUp();
            CacheEventArgs evt =
                new CacheEventArgs(cache, CacheEventType.Inserted, cDown.Convert("key"), cDown.Convert("valueOld"), cDown.Convert("valueNew"), false);

            CacheEventArgs convEvt = ConverterCollections.GetCacheEventArgs(newCache, evt, cUp, cUp);

            Assert.IsNotNull(convEvt);
            Assert.AreEqual(convEvt.Cache, newCache);
            Assert.IsFalse(convEvt.Cache.Equals(evt.Cache));
            Assert.AreEqual(convEvt.EventType, evt.EventType);
            Assert.AreEqual(convEvt.IsSynthetic, evt.IsSynthetic);
            Assert.AreEqual(convEvt.OldValue, cUp.Convert(evt.OldValue));
            Assert.AreEqual(convEvt.NewValue, cUp.Convert(evt.NewValue));
            Assert.AreEqual(convEvt.Key, cUp.Convert(evt.Key));

            Assert.IsInstanceOf(typeof (ConverterCollections.ConverterCacheEventArgs), convEvt);
            ConverterCollections.ConverterCacheEventArgs ccea = convEvt as ConverterCollections.ConverterCacheEventArgs;
            Assert.IsNotNull(ccea);
            Assert.AreEqual(ccea.CacheEvent, evt);
            Assert.AreEqual(ccea.ConverterKeyUp, cUp);
            Assert.AreEqual(ccea.ConverterValueUp, cUp);
        }

        [Test]
        public void ConverterCacheListenerTests()
        {
            TestCacheListener listener = new TestCacheListener();
            IObservableCache cache = InstantiateCache();
            IConverter cUp = new ConvertUp();
            CacheEventArgs evt =
                new CacheEventArgs(cache, CacheEventType.Inserted, "key", "oldvalue", "newvalue", false);
            ConverterCollections.ConverterCacheEventArgs convEvt =
                (ConverterCollections.ConverterCacheEventArgs) ConverterCollections.GetCacheEventArgs(cache, evt, cUp, cUp);
            
            ICacheListener convListener = new ConverterCollections.ConverterCacheListener(cache, listener, cUp, cUp);

            Assert.IsNotNull(convListener);
            Assert.AreEqual(listener.m_inserted, 0);
            convListener.EntryInserted(evt);
            Assert.AreEqual(listener.m_inserted, 1);
            Assert.IsNotNull(listener.m_evt);
            CacheEventArgs listenerEvt = listener.m_evt;
            Assert.AreEqual(listenerEvt.Cache, convEvt.Cache);
            Assert.AreEqual(listenerEvt.EventType, convEvt.EventType);
            Assert.AreEqual(listenerEvt.IsSynthetic, convEvt.IsSynthetic);
            Assert.AreEqual(listenerEvt.Key, convEvt.Key);
            Assert.AreEqual(listenerEvt.NewValue, convEvt.NewValue);
            Assert.AreEqual(listenerEvt.OldValue, convEvt.OldValue);

            Assert.AreEqual(listener.m_updated, 0);
            convListener.EntryUpdated(evt);
            Assert.AreEqual(listener.m_updated, 1);

            Assert.AreEqual(listener.m_deleted, 0);
            convListener.EntryDeleted(evt);
            Assert.AreEqual(listener.m_deleted, 1);

            Assert.IsInstanceOf(typeof (ConverterCollections.ConverterCacheListener), convListener);
            ConverterCollections.ConverterCacheListener ccl =
                convListener as ConverterCollections.ConverterCacheListener;
            Assert.IsNotNull(ccl);
            Assert.AreEqual(ccl.CacheListener, listener);
            Assert.AreEqual(ccl.ConverterKeyUp, cUp);
            Assert.AreEqual(ccl.ConverterValueUp, cUp);
            Assert.AreEqual(ccl.ObservableCache, cache);
        }

        #region Helper methods

        private LocalCache InstantiateCache()
        {
            return new LocalCache();
        }

        private LocalNamedCache InstantiateNamedCache()
        {
            return new LocalNamedCache();
        }

        #endregion

        #region Test classes

        /// <summary>
        /// Very simple converter, appends "_converted" to the end of string
        /// representation of passed object.
        /// </summary>
        /// <remarks>
        /// Can work with any object, but in order to be used in pair with
        /// <see cref="ConvertUp"/>, use strings (or integers).
        /// </remarks>
        public class ConvertDown : IConverter
        {
            /// <summary>
            /// Convert the passed object to another object.
            /// </summary>
            /// <param name="o">
            /// Object to be converted.
            /// </param>
            /// <returns>
            /// The new, converted object.
            /// </returns>
            public object Convert(object o)
            {
                if (o == null)
                {
                    return "_converted";
                }
                return o + "_converted";
            }
        }

        /// <summary>
        /// Very simple converter, removes "_converted" from the end of
        /// string representation of passed object.
        /// </summary>
        /// <remarks>
        /// Can work with any object, but in order to be used in pair with
        /// <see cref="ConvertDown"/>, use strings (or integers).
        /// </remarks>
        public class ConvertUp : IConverter
        {
            /// <summary>
            /// Convert the passed object to another object.
            /// </summary>
            /// <param name="o">
            /// Object to be converted.
            /// </param>
            /// <returns>
            /// The new, converted object.
            /// </returns>
            public object Convert(object o)
            {
                if (o == null)
                {
                    return "";
                }
                string s = o.ToString();
                int i = s.IndexOf("_converted");
                if (i > -1)
                {
                    return s.Substring(0, i);
                }
                else
                {
                    return s;
                }
            }
        }

        #endregion
    }
}