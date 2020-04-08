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

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Net.Impl;
using Tangosol.Util.Collections;
using Tangosol.Util.Extractor;
using Tangosol.Util.Transformer;

using NUnit.Framework;

namespace Tangosol.Util.Filter
{
    [TestFixture]
    public class FilterTests
    {
        readonly NameValueCollection appSettings = TestUtils.AppSettings;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
        }

        [Test]
        public void TestAlwaysFilter()
        {
            IFilter filter = AlwaysFilter.Instance;
            IFilter filter1 = new AlwaysFilter();
            Assert.IsNotNull(filter);
            Assert.AreEqual(filter, filter1);
            Assert.AreEqual(filter.ToString(), filter1.ToString());
            Assert.AreEqual(filter.GetHashCode(), filter1.GetHashCode());
            Assert.IsTrue(filter.Evaluate(new object()));

            var entry = new CacheEntry("key", "value");
            var af = filter as AlwaysFilter;
            Assert.IsNotNull(af);
            Assert.IsTrue(af.EvaluateEntry(entry));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert("1", 1);
            cache.Insert("2", 2);
            cache.Insert("3", 3);

            ICollection results = cache.GetKeys(filter);
            Assert.AreEqual(results.Count, cache.Count);

            ICacheEntry[] entries = cache.GetEntries(filter);
            Assert.AreEqual(cache.Count, entries.Length);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestAlwaysFilterApplyIndex()
        {
            var extract = new IdentityExtractor();
            AlwaysFilter filter = AlwaysFilter.Instance;
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i % 10));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(100, setResults.Count);
            for (int i = 0; i < 100; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }
        
        [Test]
        public void TestContainsAllFilter()
        {
            //this test covers also ComparisonFilter and ExtractorFilter - super types
            int[] array = {1, 2, 3};
            var list = new ArrayList(array);
            var ext = new ReflectionExtractor("Property");
            IFilter filter = new ContainsAllFilter("Property", array);
            IFilter filter1 = new ContainsAllFilter(ext, list);
            Assert.IsNotNull(filter);
            Assert.AreEqual(filter.ToString(), filter1.ToString());
            var caf = filter as ContainsAllFilter;
            var caf1 = filter as ContainsAllFilter;
            Assert.IsNotNull(caf);
            Assert.IsNotNull(caf1);
            Assert.AreEqual(caf.ValueExtractor, caf1.ValueExtractor);
                        
            Assert.IsInstanceOf(typeof(ImmutableArrayList), caf.Value);
            var al = caf.Value as ImmutableArrayList;
            Assert.IsNotNull(al);
            Assert.AreEqual(al.Count, list.Count);
            foreach (int i in al)
            {
                Assert.Contains(i, list);
            }
            Assert.AreEqual(caf.ValueExtractor, ext);

            filter = new ContainsAllFilter(IdentityExtractor.Instance, list);
            var list1 = new ArrayList();
            Assert.IsFalse(filter.Evaluate(list1));
            Assert.IsFalse(filter.Evaluate(new int[] {}));
            list1 = new ArrayList(array);
            Assert.IsTrue(filter.Evaluate(list1));
            Assert.IsTrue(filter.Evaluate(array));

            caf = filter as ContainsAllFilter;
            Assert.IsNotNull(caf);
            var entry = new TestQueryCacheEntry("key", new[] {1,2});
            Assert.IsFalse(caf.EvaluateEntry(entry));
            entry.Value = array;
            Assert.IsTrue(caf.EvaluateEntry(entry));
            entry.Value = new ArrayList();
            Assert.IsFalse(caf.EvaluateEntry(entry));
            entry.Value = list;
            Assert.IsTrue(caf.EvaluateEntry(entry));
            entry.Value = "bla";
            Assert.IsFalse(caf.EvaluateEntry(entry));

            var centry = new CacheEntry("key", new[] { 1, 2 });
            Assert.IsFalse(caf.EvaluateEntry(centry));
            centry.Value = array;
            Assert.IsTrue(caf.EvaluateEntry(centry));
            centry.Value = new ArrayList();
            Assert.IsFalse(caf.EvaluateEntry(centry));
            centry.Value = list;
            Assert.IsTrue(caf.EvaluateEntry(centry));
            centry.Value = "bla";
            Assert.IsFalse(caf.EvaluateEntry(centry));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(1, 5);
            cache.Insert(7, Int32.MaxValue);
            cache.Insert(8, Int32.MinValue);
            var values = new object[] { "pera", "mika", "banana", "zika" };
            cache.Insert(10, new ArrayList(values));

            values = new object[] { "jovanka", "miladinka" };
            cache.Insert(11, new ArrayList(values));

            values = new object[] { "ananas", "banana", 5 };
            cache.Insert(12, new ArrayList(values));

            values = new object[] { "banana", "zika" };
            var containsAll = new ContainsAllFilter(IdentityExtractor.Instance, values);

            ICollection results = cache.GetKeys(containsAll);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 10));

            ICollection entries = cache.GetEntries(containsAll);
            Assert.AreEqual(1, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestContainsAllFilterApplyIndex()
        {
            var extract = new IdentityExtractor();

            var set = new ArrayList {"Monkey", "Runner"};

            var filter = new ContainsAllFilter(extract, set);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            var value1 = new ArrayList {"Monkey", "Star"};
            index.Insert(new CacheEntry(1, value1));
            setResults.Add(1);

            var value2 = new ArrayList {"Runner", "Pancake", "Monkey"};
            index.Insert(new CacheEntry(2, value2));
            setResults.Add(2);

            var value3 = new ArrayList {"Picture", "Mouse"};
            index.Insert(new CacheEntry(3, value3));
            setResults.Add(3);

            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(1, setResults.Count);

            Assert.IsTrue(setResults.Contains(2));
        }

        [Test]
        public void TestContainsAnyFilter()
        {
            int[] array = { 1, 2, 3 };
            var list = new ArrayList(array);
            IFilter filter = new ContainsAnyFilter("Property", array);
            Assert.IsNotNull(filter);
            filter = new ContainsAnyFilter(IdentityExtractor.Instance, list);
            Assert.IsNotNull(filter);

            array = new[] {3, 4, 5};
            list = new ArrayList();
            Assert.IsFalse(filter.Evaluate(list));
            Assert.IsFalse(filter.Evaluate(new int[] { }));
            list = new ArrayList(array);
            Assert.IsTrue(filter.Evaluate(list));
            Assert.IsTrue(filter.Evaluate(array));
            array = new[] {100, 200};
            list = new ArrayList(array);
            Assert.IsFalse(filter.Evaluate(list));
            Assert.IsFalse(filter.Evaluate(array));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(1, 5);
            cache.Insert(7, Int32.MaxValue);
            cache.Insert(8, Int32.MinValue);
            var values = new object[] { "pera", "mika", "banana", "zika" };
            cache.Insert(10, new ArrayList(values));

            values = new object[] { "jovanka", "miladinka" };
            cache.Insert(11, new ArrayList(values));

            values = new object[] { "ananas", "banana", 5 };
            cache.Insert(12, new ArrayList(values));

            values = new object[] { "banana", "zika" };
            var containsAny = new ContainsAnyFilter(IdentityExtractor.Instance, new ArrayList(values));

            ICollection results = cache.GetKeys(containsAny);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 10));
            Assert.IsTrue(CollectionUtils.Contains(results, 12));

            ICollection entries = cache.GetEntries(containsAny);
            Assert.AreEqual(2, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestContainsAnyFilterApplyIndex()
        {
            var extract = new IdentityExtractor();

            var set = new ArrayList {"Monkey", "Runner"};

            var filter = new ContainsAnyFilter(extract, set);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            var value1 = new ArrayList {"Monkey", "Star"};
            index.Insert(new CacheEntry(1, value1));
            setResults.Add(1);

            var value2 = new ArrayList {"Runner", "Pancake", "Monkey"};
            index.Insert(new CacheEntry(2, value2));
            setResults.Add(2);

            var value3 = new ArrayList {"Picture", "Mouse"};
            index.Insert(new CacheEntry(3, value3));
            setResults.Add(3);

            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(2, setResults.Count);

            Assert.IsTrue(setResults.Contains(1));
            Assert.IsTrue(setResults.Contains(2));
        }
        
        [Test]
        public void TestContainsFilter()
        {
            const int value = 100;
            IFilter filter = new ContainsFilter("Property", value);
            Assert.IsNotNull(filter);
            filter = new ContainsFilter(IdentityExtractor.Instance, value);
            Assert.IsNotNull(filter);

            Assert.IsFalse(filter.Evaluate(new object()));
            var array = new[] { 3, 4, 5 };
            var list = new ArrayList();
            Assert.IsFalse(filter.Evaluate(list));
            Assert.IsFalse(filter.Evaluate(new int[] { }));
            list = new ArrayList(array);
            Assert.IsFalse(filter.Evaluate(list));
            Assert.IsFalse(filter.Evaluate(array));
            array = new[] { 100, 200 };
            list = new ArrayList(array);
            Assert.IsTrue(filter.Evaluate(list));
            Assert.IsTrue(filter.Evaluate(array));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            var values = new object[] { "pera", "mika", "banana", "zika" };
            cache.Insert(10, new ArrayList(values));

            values = new object[] { "jovanka", "miladinka" };
            cache.Insert(11, new ArrayList(values));

            values = new object[] { "ananas", "banana", 5 };
            cache.Insert(12, new ArrayList(values));

            var contains = new ContainsFilter(IdentityExtractor.Instance, "banana");

            ICollection results = cache.GetKeys(contains);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 10));
            Assert.IsTrue(CollectionUtils.Contains(results, 12));

            contains = new ContainsFilter(IdentityExtractor.Instance, "banana");
            ICollection entries = cache.GetEntries(contains);
            Assert.AreEqual(2, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestContainsFilterApplyIndex()
        {
            var extract = new IdentityExtractor();

            var filter = new ContainsFilter(extract, "Monkey");
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            var value1 = new ArrayList {"Monkey", "Star"};
            index.Insert(new CacheEntry(1, value1));
            setResults.Add(1);

            var value2 = new ArrayList {"Runner", "Pancake", "Monkey"};
            index.Insert(new CacheEntry(2, value2));
            setResults.Add(2);

            var value3 = new ArrayList {"Picture", "Mouse"};
            index.Insert(new CacheEntry(3, value3));
            setResults.Add(3);

            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(2, setResults.Count);

            Assert.IsTrue(setResults.Contains(1));
            Assert.IsTrue(setResults.Contains(2));
        }

        [Test]
        public void TestEqualsFilter()
        {
            const int intValue = 100;
            const long longValue = 100L;
            double doubleValue = Convert.ToDouble(intValue);
            float floatValue = Convert.ToSingle(intValue);
            IFilter filter = new EqualsFilter("Property", intValue);
            Assert.IsNotNull(filter);
            filter = new EqualsFilter("Property", longValue);
            Assert.IsNotNull(filter);
            filter = new EqualsFilter("Property", doubleValue);
            Assert.IsNotNull(filter);
            filter = new EqualsFilter("Property", floatValue);
            Assert.IsNotNull(filter);
            filter = new EqualsFilter("Property", new object());
            Assert.IsNotNull(filter);
            filter = new EqualsFilter(IdentityExtractor.Instance, intValue);
            Assert.IsNotNull(filter);

            Assert.IsTrue(filter.Evaluate(intValue));
            Assert.IsFalse(filter.Evaluate(new object()));
            Assert.IsFalse(filter.Evaluate(longValue));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(1, 5);
            cache.Insert(7, Int32.MaxValue);
            cache.Insert(8, Int32.MinValue);

            var equalsFilter = new EqualsFilter(IdentityExtractor.Instance, 5);

            ICollection results = cache.GetKeys(equalsFilter);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 1));

            ICollection entries = cache.GetEntries(equalsFilter);
            Assert.AreEqual(1, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestEqualsFilterApplyIndex()
        {
            var extract = new IdentityExtractor();
            var filter = new EqualsFilter(extract, 5);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i % 10));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(10, setResults.Count);
            for (int i = 5; i < 100; i += 10)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }

        [Test]
        public void TestIsNullFilter()
        {
            IFilter filter = new IsNullFilter("InnerMember");
            Assert.IsNotNull(filter);

            var o = new ReflectionTestType();
            Assert.IsFalse(filter.Evaluate(o));
            o.InnerMember = null;
            Assert.IsTrue(filter.Evaluate(o));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            var address = new Address("XI krajiske", "Belgrade", "Serbia", "11000");
            cache.Insert(1, address);

            address = new Address("Champs-Elysees", "Paris", null, "1000");
            cache.Insert(2, address);


            var isNullFilter = new IsNullFilter("getState");

            ICollection results = cache.GetKeys(isNullFilter);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 2));

            ICollection entries = cache.GetEntries(isNullFilter);
            Assert.AreEqual(1, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestGreaterEqualsFilter()
        {
            const int intValue = 100;
            const long longValue = 100L;
            double doubleValue = Convert.ToDouble(intValue);
            float floatValue = Convert.ToSingle(intValue);
            DateTime date = DateTime.Now;
            IFilter filter = new GreaterEqualsFilter("Property", intValue);
            Assert.IsNotNull(filter);
            filter = new GreaterEqualsFilter("Property", longValue);
            Assert.IsNotNull(filter);
            filter = new GreaterEqualsFilter("Property", doubleValue);
            Assert.IsNotNull(filter);
            filter = new GreaterEqualsFilter("Property", floatValue);
            Assert.IsNotNull(filter);
            filter = new GreaterEqualsFilter("Property", date);
            Assert.IsNotNull(filter);
            filter = new GreaterEqualsFilter(IdentityExtractor.Instance, intValue);
            Assert.IsNotNull(filter);

            Assert.IsTrue(filter.Evaluate(intValue));
            Assert.IsTrue(filter.Evaluate(1000));
            Assert.IsFalse(filter.Evaluate(5));
            try
            {
                filter.Evaluate(new object());
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidCastException), e);
            }

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(1, "n5");
            cache.Insert(2, 10);
            cache.Insert(3, 105);
            cache.Insert(4, Int32.MinValue);
            cache.Insert(5, 55);
            var greaterEquals = new GreaterEqualsFilter(IdentityExtractor.Instance, 55);

            try
            {
                cache.GetKeys(greaterEquals);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(PortableException), e);
            }
            cache.Remove(1);
            ICollection results = cache.GetKeys(greaterEquals);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 3));
            Assert.IsTrue(CollectionUtils.Contains(results, 5));

            ICollection entries = cache.GetEntries(greaterEquals);
            Assert.AreEqual(2, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestGreaterEqualsFilterApplyIndex()
        {
            var extract = new IdentityExtractor();
            var filter = new GreaterEqualsFilter(extract, 50);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(50, setResults.Count);
            for (int i = 50; i < 100; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }

        [Test]
        public void TestGreaterFilter()
        {
            const int intValue = 100;
            const long longValue = 100L;
            double doubleValue = Convert.ToDouble(intValue);
            float floatValue = Convert.ToSingle(intValue);
            DateTime date = DateTime.Now;
            IFilter filter = new GreaterFilter("Property", intValue);
            Assert.IsNotNull(filter);
            filter = new GreaterFilter("Property", longValue);
            Assert.IsNotNull(filter);
            filter = new GreaterFilter("Property", doubleValue);
            Assert.IsNotNull(filter);
            filter = new GreaterFilter("Property", floatValue);
            Assert.IsNotNull(filter);
            filter = new GreaterFilter("Property", date);
            Assert.IsNotNull(filter);
            filter = new GreaterFilter(IdentityExtractor.Instance, intValue);
            Assert.IsNotNull(filter);

            Assert.IsFalse(filter.Evaluate(intValue));
            Assert.IsTrue(filter.Evaluate(1000));
            Assert.IsFalse(filter.Evaluate(5));
            try
            {
                filter.Evaluate(new object());
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidCastException), e);
            }

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(1, 65.35);
            cache.Insert(2, 10);
            cache.Insert(3, 105);
            cache.Insert(4, Int32.MinValue);
            cache.Insert(5, 55);
            cache.Insert(6, Int32.MaxValue);
            var greaterEquals = new GreaterFilter(IdentityExtractor.Instance, 55);

            try
            {
                cache.GetKeys(greaterEquals);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(PortableException), e);
            }
            cache.Remove(1);
            ICollection results = cache.GetKeys(greaterEquals);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 3));
            Assert.IsTrue(CollectionUtils.Contains(results, 6));

            ICollection entries = cache.GetEntries(greaterEquals);
            Assert.AreEqual(2, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestGreaterFilterApplyIndex()
        {
            var extract = new IdentityExtractor();
            var filter = new GreaterFilter(extract, 50);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(49, setResults.Count);
            for (int i = 51; i < 100; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }

        [Test]
        public void TestInFilter()
        {
            int[] array = {1, 2, 3};
            IFilter filter = new InFilter("Property", array);
            Assert.IsNotNull(filter);
            filter = new InFilter(IdentityExtractor.Instance, array);
            Assert.IsNotNull(filter);

            Assert.IsTrue(filter.Evaluate(3));
            Assert.IsFalse(filter.Evaluate(100));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(1, 5);
            cache.Insert(2, 40);
            cache.Insert(3, 75);
            cache.Insert(4, 80);
            cache.Insert(5, 95);
            cache.Insert(6, 105);
            cache.Insert(7, Int32.MaxValue);
            cache.Insert(8, Int32.MinValue);

            var inFilter = new InFilter(IdentityExtractor.Instance, new Object[] { 5, 75, 105 });

            ICollection results = cache.GetKeys(inFilter);

            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 1));
            Assert.IsTrue(CollectionUtils.Contains(results, 3));
            Assert.IsTrue(CollectionUtils.Contains(results, 6));

            ICollection entries = cache.GetEntries(inFilter);
            Assert.AreEqual(3, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestInFilterApplyIndex()
        {
            var extract = new IdentityExtractor();

            var set = new ArrayList {"Monkey", "Runner"};

            var filter = new InFilter(extract, set);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            var value1 = new ArrayList {"Monkey", "Star"};
            index.Insert(new CacheEntry(1, value1));
            setResults.Add(1);

            var value2 = new ArrayList {"Runner", "Pancake", "Monkey"};
            index.Insert(new CacheEntry(2, value2));
            setResults.Add(2);

            var value3 = new ArrayList {"Picture", "Mouse"};
            index.Insert(new CacheEntry(3, value3));
            setResults.Add(3);

            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(2, setResults.Count);

            Assert.IsTrue(setResults.Contains(1));
            Assert.IsTrue(setResults.Contains(2));
        }
        
        [Test]
        public void TestLessEqualsFilter()
        {
            const int intValue = 100;
            const long longValue = 100L;
            double doubleValue = Convert.ToDouble(intValue);
            float floatValue = Convert.ToSingle(intValue);
            DateTime date = DateTime.Now;
            IFilter filter = new LessEqualsFilter("Property", intValue);
            Assert.IsNotNull(filter);
            filter = new LessEqualsFilter("Property", longValue);
            Assert.IsNotNull(filter);
            filter = new LessEqualsFilter("Property", doubleValue);
            Assert.IsNotNull(filter);
            filter = new LessEqualsFilter("Property", floatValue);
            Assert.IsNotNull(filter);
            filter = new LessEqualsFilter("Property", date);
            Assert.IsNotNull(filter);
            filter = new LessEqualsFilter(IdentityExtractor.Instance, intValue);
            Assert.IsNotNull(filter);

            Assert.IsTrue(filter.Evaluate(intValue));
            Assert.IsTrue(filter.Evaluate(10));
            Assert.IsFalse(filter.Evaluate(500));
            try
            {
                filter.Evaluate(new object());
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidCastException), e);
            }

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(2, 10);
            cache.Insert(3, 105);
            cache.Insert(4, Int32.MinValue);
            cache.Insert(5, 55);
            var lessEquals = new LessEqualsFilter(IdentityExtractor.Instance, 55);
            ICollection results = cache.GetKeys(lessEquals);

            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 2));
            Assert.IsTrue(CollectionUtils.Contains(results, 4));
            Assert.IsTrue(CollectionUtils.Contains(results, 5));

            ICollection entries = cache.GetEntries(lessEquals);
            Assert.AreEqual(3, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestLessEqualsFilterApplyIndex()
        {
            var extract = new IdentityExtractor();
            var filter = new LessEqualsFilter(extract, 50);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(51, setResults.Count);
            for (int i = 0; i <= 50; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }

        [Test]
        public void TestLessFilter()
        {
            const int intValue = 100;
            const long longValue = 100L;
            double doubleValue = Convert.ToDouble(intValue);
            float floatValue = Convert.ToSingle(intValue);
            DateTime date = DateTime.Now;
            IFilter filter = new LessFilter("Property", intValue);
            Assert.IsNotNull(filter);
            filter = new LessFilter("Property", longValue);
            Assert.IsNotNull(filter);
            filter = new LessFilter("Property", doubleValue);
            Assert.IsNotNull(filter);
            filter = new LessFilter("Property", floatValue);
            Assert.IsNotNull(filter);
            filter = new LessFilter("Property", date);
            Assert.IsNotNull(filter);
            filter = new LessFilter(IdentityExtractor.Instance, intValue);
            Assert.IsNotNull(filter);

            Assert.IsFalse(filter.Evaluate(intValue));
            Assert.IsTrue(filter.Evaluate(1));
            Assert.IsFalse(filter.Evaluate(500));
            try
            {
                filter.Evaluate(new object());
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidCastException), e);
            }

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(2, 10);
            cache.Insert(3, 105);
            cache.Insert(4, Int32.MinValue);
            cache.Insert(5, 55);
            cache.Insert(6, Int32.MaxValue);
            var lessFilter = new LessFilter(IdentityExtractor.Instance, 55);
            ICollection results = cache.GetKeys(lessFilter);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 2));
            Assert.IsTrue(CollectionUtils.Contains(results, 4));

            ICollection entries = cache.GetEntries(lessFilter);
            Assert.AreEqual(2, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestLessFilterApplyIndex()
        {
            var extract = new IdentityExtractor();
            var filter = new LessFilter(extract, 50);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(50, setResults.Count);
            for (int i = 0; i < 50; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }

        [Test]
        public void TestLikeFilter()
        {
            var filter = new LikeFilter("field", "ana", '\\', false);
            Assert.IsNotNull(filter);
            filter = new LikeFilter("field", "ana", true);
            Assert.IsNotNull(filter);
            filter = new LikeFilter("field", "ana");
            Assert.IsNotNull(filter);

            filter = new LikeFilter(IdentityExtractor.Instance, "ana", '\\', true);
            Assert.IsNotNull(filter);
            Assert.AreEqual(filter.EscapeChar, '\\');
            Assert.IsTrue(filter.IgnoreCase);
            Assert.AreEqual(filter.Pattern, "ana");
            Assert.AreEqual(filter.Value, filter.Pattern);
            Assert.AreEqual(filter.ValueExtractor, IdentityExtractor.Instance);

            string pattern = null;
            filter = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', true);
            var filterCaseSens = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', false);
            Assert.IsNotNull(filter);
            Assert.IsFalse(filter.Evaluate(null));
            Assert.IsFalse(filter.Evaluate("string"));
            Assert.IsFalse(filterCaseSens.Evaluate("string"));

            pattern = "";
            filter = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', true);
            filterCaseSens = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', false);
            Assert.IsFalse(filter.Evaluate(null));
            Assert.IsFalse(filter.Evaluate("sTring"));
            Assert.IsTrue(filter.Evaluate(""));
            Assert.IsFalse(filterCaseSens.Evaluate("sTring"));
            Assert.IsTrue(filterCaseSens.Evaluate(""));

            pattern = "aNa";
            filter = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', true);
            filterCaseSens = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', false);
            Assert.IsTrue(filter.Evaluate("ana"));
            Assert.IsTrue(filter.Evaluate("Ana"));
            Assert.IsTrue(filter.Evaluate("aNa"));
            Assert.IsFalse(filter.Evaluate("sTring"));
            Assert.IsTrue(filterCaseSens.Evaluate("aNa"));
            Assert.IsFalse(filterCaseSens.Evaluate("ANa"));
            Assert.IsFalse(filterCaseSens.Evaluate("ana"));
            Assert.IsFalse(filterCaseSens.Evaluate("sTring"));

            pattern = "%";
            filter = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', true);
            filterCaseSens = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', false);
            Assert.IsTrue(filter.Evaluate("something"));
            Assert.IsTrue(filterCaseSens.Evaluate("something"));

            pattern = "x%";
            filter = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', true);
            filterCaseSens = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', false);
            Assert.IsTrue(filter.Evaluate("xa"));
            Assert.IsTrue(filter.Evaluate("Xa"));
            Assert.IsFalse(filter.Evaluate("axa"));
            Assert.IsTrue(filterCaseSens.Evaluate("xa"));
            Assert.IsFalse(filterCaseSens.Evaluate("Xa"));
            Assert.IsFalse(filterCaseSens.Evaluate("axa"));

            pattern = "ba%";
            filter = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', true);
            filterCaseSens = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', false);
            Assert.IsTrue(filter.Evaluate("ba"));
            Assert.IsTrue(filter.Evaluate("Baaab"));
            Assert.IsFalse(filter.Evaluate("axa"));
            Assert.IsTrue(filterCaseSens.Evaluate("ba"));
            Assert.IsFalse(filterCaseSens.Evaluate("Bad"));
            Assert.IsFalse(filterCaseSens.Evaluate("axa"));

            pattern = "%S";
            filter = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', true);
            filterCaseSens = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', false);
            Assert.IsTrue(filter.Evaluate("baS"));
            Assert.IsTrue(filter.Evaluate("Baaabs"));
            Assert.IsFalse(filter.Evaluate("axa"));
            Assert.IsTrue(filterCaseSens.Evaluate("xaS"));
            Assert.IsFalse(filterCaseSens.Evaluate("Xas"));
            Assert.IsFalse(filterCaseSens.Evaluate("axa"));

            pattern = "%SS";
            filter = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', true);
            filterCaseSens = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', false);
            Assert.IsTrue(filter.Evaluate("baSs"));
            Assert.IsTrue(filter.Evaluate("BaaasS"));
            Assert.IsFalse(filter.Evaluate("axa"));
            Assert.IsTrue(filterCaseSens.Evaluate("xaSS"));
            Assert.IsFalse(filterCaseSens.Evaluate("XasS"));
            Assert.IsFalse(filterCaseSens.Evaluate("axa"));

            pattern = "%ir%";
            filter = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', true);
            filterCaseSens = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', false);
            Assert.IsTrue(filter.Evaluate("irene"));
            Assert.IsTrue(filter.Evaluate("giRos"));
            Assert.IsFalse(filter.Evaluate("axa"));
            Assert.IsTrue(filterCaseSens.Evaluate("xair"));
            Assert.IsFalse(filterCaseSens.Evaluate("XaIrs"));
            Assert.IsFalse(filterCaseSens.Evaluate("axa"));

            pattern = "%m%";
            filter = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', true);
            filterCaseSens = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', false);
            Assert.IsTrue(filter.Evaluate("mom"));
            Assert.IsTrue(filter.Evaluate("soMe"));
            Assert.IsFalse(filter.Evaluate("axa"));
            Assert.IsTrue(filterCaseSens.Evaluate("mom"));
            Assert.IsFalse(filterCaseSens.Evaluate("MoM"));
            Assert.IsFalse(filterCaseSens.Evaluate("axa"));

            pattern = "_na%";
            filter = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', true);
            filterCaseSens = new LikeFilter(IdentityExtractor.Instance, pattern, '\\', false);
            Assert.IsTrue(filter.Evaluate("ANA Cikic"));
            Assert.IsTrue(filter.Evaluate("inauguration"));
            Assert.IsFalse(filter.Evaluate("nam"));
            Assert.IsTrue(filterCaseSens.Evaluate("ana"));
            Assert.IsFalse(filterCaseSens.Evaluate("ANA"));
            Assert.IsFalse(filterCaseSens.Evaluate("axa"));

            //test case for COH-4020
            pattern = "%GetLogonData_getAccountIndicativeInfoInput_7137B2C9070C4951AE00EE5C4F01435A%";
            filter = new LikeFilter(IdentityExtractor.Instance, pattern, ' ', true);
            Assert.IsTrue(filter.Evaluate("xGetLogonDataxgetAccountIndicativeInfoInputx7137B2C9070C4951AE00EE5C4F01435Axxx"));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(1, "n5");
            cache.Insert(2, "6a");
            cache.Insert(3, "ananas");
            cache.Insert(4, "banana");
            var likeFilter = new LikeFilter(IdentityExtractor.Instance, "%an%", '\\', true);
            ICollection results = cache.GetKeys(likeFilter);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 3));
            Assert.IsTrue(CollectionUtils.Contains(results, 4));

            ICollection entries = cache.GetEntries(likeFilter);
            Assert.AreEqual(2, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestLikeFilterApplyIndex()
        {
            var extract = new IdentityExtractor();
            var filter = new LikeFilter(extract, "Value1%", '\\', true);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, "Value" + i));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(11, setResults.Count);

            Assert.IsTrue(setResults.Contains(1));
            for (int i = 10; i < 19; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }

        [Test]
        public void TestNotEqualsFilter()
        {
            const int intValue = 100;
            const long longValue = 100L;
            double doubleValue = Convert.ToDouble(intValue);
            float floatValue = Convert.ToSingle(intValue);
            IFilter filter = new NotEqualsFilter("Property", intValue);
            Assert.IsNotNull(filter);
            filter = new NotEqualsFilter("Property", longValue);
            Assert.IsNotNull(filter);
            filter = new NotEqualsFilter("Property", doubleValue);
            Assert.IsNotNull(filter);
            filter = new NotEqualsFilter("Property", floatValue);
            Assert.IsNotNull(filter);
            filter = new NotEqualsFilter("Property", new object());
            Assert.IsNotNull(filter);
            filter = new NotEqualsFilter(IdentityExtractor.Instance, intValue);
            Assert.IsNotNull(filter);

            Assert.IsFalse(filter.Evaluate(intValue));
            Assert.IsTrue(filter.Evaluate(new object()));
            Assert.IsTrue(filter.Evaluate(longValue));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(1, 5);
            cache.Insert(7, Int32.MaxValue);
            cache.Insert(8, Int32.MinValue);

            var notEqualsFilter = new NotEqualsFilter(IdentityExtractor.Instance, 5);

            ICollection results = cache.GetKeys(notEqualsFilter);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 7));
            Assert.IsTrue(CollectionUtils.Contains(results, 8));

            ICollection entries = cache.GetEntries(notEqualsFilter);
            Assert.AreEqual(2, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestNotEqualsFilterApplyIndex()
        {
            var extract = new IdentityExtractor();
            var filter = new NotEqualsFilter(extract, 50);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(99, setResults.Count);
            for (int i = 0; i < 50; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
            for (int i = 51; i < 100; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }
        
        [Test]
        public void TestValueChangeEventFilter()
        {
            IFilter filter = new ValueChangeEventFilter("field");
            IFilter filter1 = new ValueChangeEventFilter("field");
            Assert.IsNotNull(filter);
            Assert.AreEqual(filter, filter1);
            Assert.AreNotEqual(filter, AlwaysFilter.Instance);
            Assert.AreEqual(filter.ToString(), filter1.ToString());
            Assert.AreEqual(filter.GetHashCode(), filter1.GetHashCode());
            var f = filter as ValueChangeEventFilter;
            var f1 = filter1 as ValueChangeEventFilter;
            Assert.IsNotNull(f);
            Assert.IsNotNull(f1);
            Assert.AreEqual(f.ValueExtractor, f1.ValueExtractor);

            filter = new ValueChangeEventFilter(IdentityExtractor.Instance);
            var ev = new CacheEventArgs(new LocalCache(), CacheEventType.Deleted, "key", "valueOld", "valueNew", false);
            Assert.IsFalse(filter.Evaluate(ev));
            ev = new CacheEventArgs(new LocalCache(), CacheEventType.Updated, "key", "valueOld", "valueNew", false);
            Assert.IsTrue(filter.Evaluate(ev));
            ev = new CacheEventArgs(new LocalCache(), CacheEventType.Updated, "key", "value", "value", false);
            Assert.IsFalse(filter.Evaluate(ev));
        }

        [Test]
        public void TestIsNotNullFilter()
        {
            IFilter filter = new IsNotNullFilter("InnerMember");
            Assert.IsNotNull(filter);

            var o = new ReflectionTestType();
            Assert.IsTrue(filter.Evaluate(o));
            o.InnerMember = null;
            Assert.IsFalse(filter.Evaluate(o));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            var address = new Address("XI krajiske", "Belgrade", "Serbia", "11000");
            cache.Insert(1, address);

            address = new Address("Champs-Elysees", "Paris", "!", "1000");
            cache.Insert(2, address);


            var isNotNullFilter = new IsNotNullFilter("getStreet");

            ICollection results = cache.GetKeys(isNotNullFilter);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 1));
            Assert.IsTrue(CollectionUtils.Contains(results, 2));

            ICollection entries = cache.GetEntries(isNotNullFilter);
            Assert.AreEqual(2, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestAllFilter()
        {
            IFilter filter1 = new GreaterFilter(IdentityExtractor.Instance, 100);
            IFilter filter2 = new LessFilter(IdentityExtractor.Instance, 1000);
            var filters = new[] {filter1, filter2};
            IFilter filter = new AllFilter(filters);
            IFilter filter3 = new AllFilter(filters);
            Assert.IsNotNull(filter);
            Assert.AreEqual(filter, filter3);
            Assert.AreEqual(filter.ToString(), filter3.ToString());
            Assert.AreEqual(filter.GetHashCode(), filter3.GetHashCode());
            Assert.AreNotEqual(filter, filter1);
            var f = filter as AllFilter;
            var f3 = filter3 as AllFilter;
            Assert.IsNotNull(f);
            Assert.IsNotNull(f3);
            Assert.AreEqual(f.Filters, f3.Filters);

            Assert.IsTrue(filter.Evaluate(500));
            Assert.IsFalse(filter.Evaluate(10000));

            var entry1 = new TestQueryCacheEntry(1, 500);
            var entry2 = new TestQueryCacheEntry(2, 10000);
            Assert.IsTrue(f.EvaluateEntry(entry1));
            Assert.IsFalse(f.EvaluateEntry(entry2));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert("5", "5c");
            cache.Insert(1, "5b");
            cache.Insert(3.54, "5a");
            cache.Insert("g", "goran");
            cache.Insert("b", "bojana");
            var inFilter = new InFilter(IdentityExtractor.Instance, new Object[] { "goran" });
            var likeFilter = new LikeFilter(IdentityExtractor.Instance, "%an%", '\\', false);

            filters = new IFilter[] { inFilter, likeFilter };
            var af = new AllFilter(filters);
            ICollection results = cache.GetKeys(af);

            Assert.AreEqual(results.Count, 1);
            Assert.IsTrue(CollectionUtils.Contains(results, "g"));

            ICacheEntry[] entries = cache.GetEntries(af);
            Assert.AreEqual(1, entries.Length);
            ICacheEntry entry = entries[0];
            Assert.AreEqual("g", entry.Key);
            Assert.AreEqual("goran", entry.Value);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestAllFilterApplyIndex()
        {
            var extract = new IdentityExtractor();

            IFilter filter1 = new GreaterFilter(extract, 50);
            IFilter filter2 = new LessFilter(extract, 60);
            var filters = new[] { filter1, filter2 };

            var filter = new AllFilter(filters);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(9, setResults.Count);
            for (int i = 51; i < 60; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }

        [Test]
        public void TestAndFilter()
        {
            IFilter filter1 = new GreaterFilter(IdentityExtractor.Instance, 100);
            IFilter filter2 = new LessFilter(IdentityExtractor.Instance, 1000);
            IFilter filter = new AndFilter(filter1, filter2);
            Assert.IsNotNull(filter);

            Assert.IsTrue(filter.Evaluate(500));
            Assert.IsFalse(filter.Evaluate(10000));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert("5", "Ana");
            cache.Insert(1, "Aleksandar");
            cache.Insert("g", "Ivan");
            cache.Insert("b", "Goran");
            var andFilter = new AndFilter(new LikeFilter(IdentityExtractor.Instance, "%an%", '\\', false), new LikeFilter(IdentityExtractor.Instance, "%r", '\\', true));

            ICollection results = cache.GetKeys(andFilter);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 1));

            ICacheEntry[] entries = cache.GetEntries(andFilter);
            Assert.AreEqual(1, entries.Length);
            ICacheEntry entry = entries[0];
            Assert.AreEqual(1, entry.Key);
            Assert.AreEqual("Aleksandar", entry.Value);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestAndFilterApplyIndex()
        {
            var extract = new IdentityExtractor();

            IFilter filter1 = new GreaterFilter(extract, 50);
            IFilter filter2 = new LessFilter(extract, 60);

            var filter = new AndFilter(filter1, filter2);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(9, setResults.Count);
            for (int i = 51; i < 60; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }

        [Test]
        public void TestBetweenFilter()
        {
            const int intValue = 100;
            const long longValue = 100L;
            double doubleValue = Convert.ToDouble(intValue);
            float floatValue = Convert.ToSingle(intValue);

            IFilter filter = new BetweenFilter("field", intValue, intValue);
            Assert.IsNotNull(filter);
            filter = new BetweenFilter("field", longValue, longValue);
            Assert.IsNotNull(filter);
            filter = new BetweenFilter("field", doubleValue, doubleValue);
            Assert.IsNotNull(filter);
            filter = new BetweenFilter("field", floatValue, floatValue);
            Assert.IsNotNull(filter);
            filter = new BetweenFilter("field", DateTime.Now, DateTime.Now.AddDays(10));
            Assert.IsNotNull(filter);
            filter = new BetweenFilter(IdentityExtractor.Instance, 100, 1000);
            Assert.IsNotNull(filter);

            Assert.IsTrue(filter.Evaluate(500));
            Assert.IsFalse(filter.Evaluate(10000));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            var ht = new Hashtable
                         {
                             {1, 5},
                             {2, 40},
                             {3, 75},
                             {4, 80},
                             {5, 95},
                             {6, 105},
                             {7, Int32.MaxValue},
                             {8, Int32.MinValue}
                         };
            cache.InsertAll(ht);

            var beetweenFilter = new BetweenFilter(IdentityExtractor.Instance, 10, 100);

            ICollection results = cache.GetKeys(beetweenFilter);

            Assert.AreEqual(4, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 2));
            Assert.IsTrue(CollectionUtils.Contains(results, 3));
            Assert.IsTrue(CollectionUtils.Contains(results, 4));
            Assert.IsTrue(CollectionUtils.Contains(results, 5));

            beetweenFilter = new BetweenFilter(IdentityExtractor.Instance, 90, 110);
            ICacheEntry[] entries = cache.GetEntries(beetweenFilter);
            Assert.AreEqual(2, entries.Length);

            foreach (ICacheEntry entry in entries) {
                Assert.IsTrue(((int) entry.Key == 5) || ((int) entry.Key == 6));
                Assert.IsTrue(((int) entry.Value == 95) || ((int) entry.Value == 105));
            }

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestBetweenFilterApplyIndex()
        {
            var extract = new IdentityExtractor();

            var filter = new BetweenFilter(extract, 50, 60);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(11, setResults.Count);
            for (int i = 50; i <= 60; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }

        [Test]
        public void TestAnyFilter()
        {
            IFilter filter1 = new GreaterFilter(IdentityExtractor.Instance, 100);
            IFilter filter2 = new LessFilter(IdentityExtractor.Instance, 0);
            var filters = new[] { filter1, filter2 };
            IFilter filter = new AnyFilter(filters);

            Assert.IsTrue(filter.Evaluate(500));
            Assert.IsTrue(filter.Evaluate(-10));
            Assert.IsFalse(filter.Evaluate(50));

            var entry1 = new TestQueryCacheEntry(1, 500);
            var entry2 = new TestQueryCacheEntry(2, 20);
            var f = filter as AnyFilter;
            Assert.IsNotNull(f);
            Assert.IsTrue(f.EvaluateEntry(entry1));
            Assert.IsFalse(f.EvaluateEntry(entry2));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert("5", "5");
            cache.Insert(1, "10");
            cache.Insert("g", "15");
            cache.Insert("b", "20");
            cache.Insert("1", "105");

            filters = new IFilter[] { new EqualsFilter(IdentityExtractor.Instance, "20"), new LikeFilter(IdentityExtractor.Instance, "1%", '\\', true)};
            var anyFilter = new AnyFilter(filters);

            ICollection results = cache.GetKeys(anyFilter);

            Assert.AreEqual(4, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 1));
            Assert.IsTrue(CollectionUtils.Contains(results, "g"));
            Assert.IsTrue(CollectionUtils.Contains(results, "b"));
            Assert.IsTrue(CollectionUtils.Contains(results, "1"));


            filters = new IFilter[] { new EqualsFilter(IdentityExtractor.Instance, "20"), new LikeFilter(IdentityExtractor.Instance, "5%", '\\', true) };
            anyFilter = new AnyFilter(filters);
            ICacheEntry[] entries = cache.GetEntries(anyFilter);
            Assert.AreEqual(2, entries.Length);

            foreach (ICacheEntry entry in entries)
            {
                Assert.IsTrue((entry.Value.Equals("20")) || (entry.Value.Equals("5")));
                Assert.IsTrue((entry.Key.Equals("b")) || (entry.Key.Equals("5")));
            }

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestAnyFilterApplyIndex()
        {
            var extract = new IdentityExtractor();

            IFilter filter1 = new GreaterFilter(extract, 60);
            IFilter filter2 = new LessFilter(extract, 50);
            var filters = new[] { filter1, filter2 };

            var filter = new AnyFilter(filters);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(89, setResults.Count);
            for (int i = 0; i < 50; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
            for (int i = 61; i < 100; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }

        [Test]
        public void TestOrFilter()
        {
            IFilter filter1 = new GreaterFilter(IdentityExtractor.Instance, 100);
            IFilter filter2 = new LessFilter(IdentityExtractor.Instance, 0);
            IFilter filter = new OrFilter(filter1, filter2);

            Assert.IsTrue(filter.Evaluate(500));
            Assert.IsTrue(filter.Evaluate(-10));
            Assert.IsFalse(filter.Evaluate(50));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert("5", "Ana");
            cache.Insert(1, "Aleksandar");
            cache.Insert("g", "Ivan");
            cache.Insert("b", "Goran");
            var orFilter = new OrFilter(new LikeFilter(IdentityExtractor.Instance, "%an", '\\', false), new LikeFilter(IdentityExtractor.Instance, "An%", '\\', true));

            ICollection results = cache.GetKeys(orFilter);

            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, "5"));
            Assert.IsTrue(CollectionUtils.Contains(results, "g"));
            Assert.IsTrue(CollectionUtils.Contains(results, "b"));

            ICollection entries = cache.GetEntries(orFilter);
            Assert.AreEqual(3, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestOrFilterApplyIndex()
        {
            var extract = new IdentityExtractor();

            IFilter filter1 = new GreaterFilter(extract, 60);
            IFilter filter2 = new LessFilter(extract, 50);

            var filter = new OrFilter(filter1, filter2);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(89, setResults.Count);
            for (int i = 0; i < 50; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
            for (int i = 61; i < 100; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }

        [Test]
        public void TestXorFilter()
        {
            IFilter filter1 = new GreaterFilter(IdentityExtractor.Instance, 100);
            IFilter filter2 = new GreaterFilter(IdentityExtractor.Instance, 0);
            IFilter filter = new XorFilter(filter1, filter2);

            Assert.IsTrue(filter.Evaluate(50));
            Assert.IsFalse(filter.Evaluate(-10));
            Assert.IsFalse(filter.Evaluate(500));

            var entry1 = new TestQueryCacheEntry(1, 500);
            var entry2 = new TestQueryCacheEntry(2, 20);
            var f = filter as XorFilter;
            Assert.IsNotNull(f);
            Assert.IsFalse(f.EvaluateEntry(entry1));
            Assert.IsTrue(f.EvaluateEntry(entry2));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert("5", "Ana");
            cache.Insert(1, "Aleksandar");
            cache.Insert("g", "Ivan");
            cache.Insert("b", "Goran");
            var xorFilter = new XorFilter(new LikeFilter(IdentityExtractor.Instance, "%an", '\\', false), new LikeFilter(IdentityExtractor.Instance, "I%", '\\', true));

            ICollection results = cache.GetKeys(xorFilter);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, "b"));

            ICollection entries = cache.GetEntries(xorFilter);
            Assert.AreEqual(1, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestXorFilterApplyIndex()
        {
            var extract = new IdentityExtractor();

            IFilter filter1 = new GreaterFilter(extract, 60);
            IFilter filter2 = new BetweenFilter(extract, 50, 75);

            var filter = new XorFilter(filter1, filter2);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new ArrayList();

            for (int i = 0; i < 100; ++i)
            {
                index.Insert(new CacheEntry(i, i));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.AreEqual(filter, filterReturn);
        }

        [Test]
        public void TestInKeySetFilter()
        {
            IFilter innerFilter = new GreaterFilter(IdentityExtractor.Instance, 100);
            var array = new[] {100, 200, 300};
            IFilter filter = new InKeySetFilter(innerFilter, array);
            IFilter filter1 = new InKeySetFilter(innerFilter, array);
            Assert.AreEqual(filter.ToString(), filter1.ToString());
            var f = filter as InKeySetFilter;
            var f1 = filter1 as InKeySetFilter;
            Assert.IsNotNull(f);
            Assert.IsNotNull(f1);
            Assert.AreEqual(f.Filter, f1.Filter);

            try
            {
                filter.Evaluate(null);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(NotSupportedException), e);
            }

            var entry1 = new TestQueryCacheEntry(100, 500);
            var entry2 = new TestQueryCacheEntry(200, 20);
            var entry3 = new TestQueryCacheEntry(500, 200);
            Assert.IsTrue(f.EvaluateEntry(entry1));
            Assert.IsFalse(f.EvaluateEntry(entry2));
            Assert.IsFalse(f.EvaluateEntry(entry3));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(1, 5);
            cache.Insert(2, 40);
            cache.Insert(3, 75);
            cache.Insert(4, 80);
            cache.Insert(5, 95);
            cache.Insert(6, 105);
            cache.Insert(7, Int32.MaxValue);
            cache.Insert(8, Int32.MinValue);

            var inKeySetFilter =
                new InKeySetFilter(new GreaterFilter(IdentityExtractor.Instance, 50), 
                                   new[] { 1, 2, 3, 6, 7, 8 });

            ICollection results = cache.GetKeys(inKeySetFilter);

//            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 3));
            Assert.IsTrue(CollectionUtils.Contains(results, 6));
            Assert.IsTrue(CollectionUtils.Contains(results, 7));

            ICollection entries = cache.GetEntries(inKeySetFilter);
            Assert.AreEqual(3, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestKeyAssociatedFilter()
        {
            try
            {
                new KeyAssociatedFilter(AlwaysFilter.Instance, null);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentNullException), e);
            }
            try
            {
                new KeyAssociatedFilter(null, new object());
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
            }
            try
            {
                new KeyAssociatedFilter(new KeyAssociatedFilter(AlwaysFilter.Instance, "key"), new object());
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
            }

            IFilter filter = new KeyAssociatedFilter(AlwaysFilter.Instance, "key");
            IFilter filter1 = new KeyAssociatedFilter(AlwaysFilter.Instance, "key");
            Assert.IsNotNull(filter);
            Assert.AreEqual(filter, filter1);
            Assert.AreEqual(filter.ToString(), filter1.ToString());
            Assert.AreEqual(filter.GetHashCode(), filter1.GetHashCode());
            var f = filter as KeyAssociatedFilter;
            Assert.IsNotNull(f);
            Assert.AreEqual(f.Filter, AlwaysFilter.Instance);
            Assert.AreEqual(f.HostKey, "key");
            Assert.AreEqual(filter.Evaluate(new object()), AlwaysFilter.Instance.Evaluate(new object()));
            Assert.IsTrue(filter.Evaluate(new object()));

            var order1 = new Order(123, "CD");
            var orderKey1 = new OrderKey(123);
            var item1 = new Item(1, 500.15);
            var itemKey1 = new ItemKey(1, 123);

            var order2 = new Order(124, "DVD");
            var orderKey2 = new OrderKey(124);
            var item2 = new Item(1, 675.10);
            var itemKey2 = new ItemKey(1, 124);

            var order3 = new Order(125, "USB");
            var orderKey3 = new OrderKey(125);
            var order4 = new Order(126, "Monitor");
            var orderKey4 = new OrderKey(126);

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(orderKey1, order1);
            cache.Insert(itemKey1, item1);
            cache.Insert(orderKey2, order2);
            cache.Insert(itemKey2, item2);
            cache.Insert(orderKey3, order3);
            cache.Insert(orderKey4, order4);

            IService service = CacheFactory.GetService("RemoteInvocationService");
            Assert.IsNotNull(service);
            var sis = service as SafeInvocationService;
            Assert.IsNotNull(sis);
            var task = new KAFValidationInvocable
                           {
                               Keys = new object[]
                                          {
                                              orderKey1,
                                              itemKey1,
                                              orderKey2,
                                              itemKey2,
                                              orderKey3,
                                              orderKey4
                                          }
                           };

            // validating that all keyassociated keys have the same partitionID

            IDictionary results = sis.Query(task, null);

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count == 1);
            ICollection colResults = results.Values;
            Assert.IsTrue(colResults.Count == 1);
            IEnumerator enumerator = colResults.GetEnumerator();
            enumerator.MoveNext();
            var oResults = (int[]) enumerator.Current;
            Assert.AreEqual(6, oResults.Length);

            // orderKey1 and itemKey1 should have the same partitionID
            Assert.AreEqual(oResults[0], oResults[1]);
            
            // orderKey2 and itemKey2 should have the same partitionID
            Assert.AreEqual(oResults[2], oResults[3]);

            // all the rest should NOT have the same partitionID
            Assert.AreNotEqual(oResults[0], oResults[2]);
            Assert.AreNotEqual(oResults[0], oResults[3]);
            Assert.AreNotEqual(oResults[1], oResults[2]);
            Assert.AreNotEqual(oResults[1], oResults[3]);
            Assert.AreNotEqual(oResults[4], oResults[5]);
            Assert.AreNotEqual(oResults[0], oResults[4]);
            Assert.AreNotEqual(oResults[0], oResults[5]);
            Assert.AreNotEqual(oResults[1], oResults[4]);
            Assert.AreNotEqual(oResults[1], oResults[5]);
            Assert.AreNotEqual(oResults[2], oResults[4]);
            Assert.AreNotEqual(oResults[2], oResults[5]);
            Assert.AreNotEqual(oResults[3], oResults[4]);
            Assert.AreNotEqual(oResults[3], oResults[5]);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestNeverFilter()
        {
            IFilter filter = NeverFilter.Instance;
            IFilter filter1 = new NeverFilter();
            Assert.IsNotNull(filter);
            Assert.AreEqual(filter, filter1);
            Assert.AreEqual(filter.ToString(), filter1.ToString());
            Assert.AreEqual(filter.GetHashCode(), filter1.GetHashCode());
            Assert.IsFalse(filter.Evaluate(new object()));

            var entry = new CacheEntry("key", "value");
            var nf = filter as NeverFilter;
            Assert.IsNotNull(nf);
            Assert.IsFalse(nf.EvaluateEntry(entry));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert("1", 1);
            cache.Insert("2", 2);
            cache.Insert("3", 3);

            ICollection results = cache.GetKeys(filter);
            Assert.AreEqual(results.Count, 0);

            ICacheEntry[] entries = cache.GetEntries(filter);
            Assert.AreEqual(entries.Length, 0);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestNotFilter()
        {
            IFilter filter = new NotFilter(NeverFilter.Instance);
            IFilter filter1 = new NotFilter(new NeverFilter());
            Assert.IsNotNull(filter);
            Assert.AreEqual(filter, filter1);
            Assert.AreEqual(filter.ToString(), filter1.ToString());
            Assert.AreEqual(filter.GetHashCode(), filter1.GetHashCode());
            Assert.AreNotEqual(filter, NeverFilter.Instance);
            var nf = filter as NotFilter;
            Assert.IsNotNull(nf);
            Assert.AreEqual(nf.Filter, NeverFilter.Instance);

            Assert.IsTrue(filter.Evaluate(new object()));

            var entry = new CacheEntry("key", "value");
            Assert.IsTrue(nf.EvaluateEntry(entry));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(1, 5);
            cache.Insert(7, Int32.MaxValue);
            cache.Insert(8, Int32.MinValue);

            var notEqualsFilter = new NotFilter(new NotEqualsFilter(IdentityExtractor.Instance, 5));

            ICollection results = cache.GetKeys(notEqualsFilter);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 1));

            ICollection entries = cache.GetEntries(notEqualsFilter);
            Assert.AreEqual(1, entries.Count);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestNotFilterApplyIndex()
        {
            var extract = new IdentityExtractor();
            var LEfilter = new LessFilter(extract, 50);

            var filter = new NotFilter(LEfilter);
            IDictionary mapIndexes = new Hashtable();
            var index = new SimpleCacheIndex(extract, false, null);
            var setResults = new HashSet();

            for (int i = 0; i < 100; ++i)
            {
                object oldValue = i == 0 ? null : (object) (i - 1);
                index.Insert(new CacheEntry(i, i, oldValue));
                setResults.Add(i);
            }
            mapIndexes[extract] = index;

            IFilter filterReturn = filter.ApplyIndex(mapIndexes, setResults);

            Assert.IsNull(filterReturn);
            Assert.AreEqual(50, setResults.Count);
            for (int i = 50; i < 100; ++i)
            {
                Assert.IsTrue(setResults.Contains(i));
            }
        }

        [Test]
        public void TestPresentFilter()
        {
            IFilter filter = PresentFilter.Instance;
            IFilter filter1 = new PresentFilter();
            Assert.IsNotNull(filter);
            Assert.AreEqual(filter, filter1);
            Assert.AreNotEqual(filter, AlwaysFilter.Instance);
            Assert.AreEqual(filter.ToString(), filter1.ToString());
            Assert.AreEqual(filter.GetHashCode(), filter1.GetHashCode());

            Assert.IsTrue(filter.Evaluate(new object()));

            var entry = new CacheEntry("key", "value");
            var pf = filter as PresentFilter;
            Assert.IsNotNull(pf);
            Assert.IsTrue(pf.EvaluateEntry(entry));

            var entry1 = new TestInvocableCacheEntry("key", "value");
            Assert.IsTrue(pf.EvaluateEntry(entry1));
        }

        [Test]
        public void TestLimitFilter()
        {
            const int pagesize = 3;
            IFilter filter = new GreaterEqualsFilter(IdentityExtractor.Instance, 18);
            var limitFilter = new LimitFilter(filter, pagesize);
            Assert.IsInstanceOf(typeof(GreaterEqualsFilter), limitFilter.Filter);
            Assert.AreEqual(pagesize, limitFilter.PageSize);
            var al = new ArrayList {25, 14, 35, 19, 32};
            limitFilter.Page = 0;
            ICollection one = limitFilter.ExtractPage(al);
            limitFilter.NextPage();
            ICollection two = limitFilter.ExtractPage(al);
            Assert.AreEqual(pagesize, one.Count);
            Assert.AreEqual(2, two.Count);

            limitFilter.PageSize = 6;
            limitFilter.Page = 0;
            limitFilter.Comparer = IdentityExtractor.Instance;
            one = limitFilter.ExtractPage(al.ToArray());
            Assert.AreEqual(al.Count, one.Count);

            limitFilter.PageSize = 3;
            limitFilter.Page = 0;
            var list1 = new ArrayList(limitFilter.ExtractPage(al.ToArray()));
            Assert.AreEqual(pagesize, list1.Count);

            limitFilter.NextPage();
            var list2 = new ArrayList(limitFilter.ExtractPage(al.ToArray()));
            Assert.AreEqual(2, list2.Count);

            //testing on remote cache
            // JH 2011.01.06
            // I'm commenting this test out for now, since LimitFilter paging
            // is pretty much broken with Coherence*Extend (see COH-2717)
//            INamedCache cache = CacheFactory.GetCache(CacheName);
//            cache.Clear();
//            cache.Insert(2, 10);
//            cache.Insert(3, 105);
//            cache.Insert(4, Int32.MinValue);
//            cache.Insert(5, 45);
//            cache.Insert(6, 46);
//            cache.Insert(7, 50);
//            cache.Insert(8, Int32.MaxValue);
//            limitFilter = new LimitFilter(new LessFilter(IdentityExtractor.Instance, 55), 2) {Page = 0};
//            ICollection results = cache.GetKeys(limitFilter);
//
//            Assert.AreEqual(2, results.Count);
//            Assert.IsTrue(CollectionUtils.ContainsAll(new ArrayList(cache.Keys), results));
//
//            limitFilter.Page = 1;
//            results = cache.GetKeys(limitFilter);
//            Assert.AreEqual(2, results.Count);
//            Assert.IsTrue(CollectionUtils.ContainsAll(new ArrayList(cache.Keys), results));
//
//            limitFilter.Page = 2;
//            results = cache.GetKeys(limitFilter);
//            Assert.AreEqual(1, results.Count);
//            Assert.IsTrue(CollectionUtils.ContainsAll(new ArrayList(cache.Keys), results));
//
//            limitFilter.Page = 1;
//            ICollection entries = cache.GetEntries(limitFilter);
//            Assert.AreEqual(2, entries.Count);
//            limitFilter.Page = 2;
//            entries = cache.GetEntries(limitFilter);
//            Assert.AreEqual(1, entries.Count);
//
//            CacheFactory.Shutdown();
        }

        [Test]
        public void TestGetEntriesLimitFilterComparer()
        {
            const int pagesize = 10;
            var limitFilter    = new LimitFilter(new AlwaysFilter(), pagesize);
            var map            = new Hashtable();

            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            for(int i=0; i < 1000; i++)
                {
                map.Add(i,i);
                }
  
            cache.InsertAll(map);

            var comparer = new IntegerComparer();

            ICacheEntry[] entries = cache.GetEntries(limitFilter, comparer);
            Assert.IsNotNull(entries, "Result set is null");
            Assert.AreEqual(entries.Length, pagesize);

            int k = 1000;
            for (var j = 0; j < entries.Length; j++)
                {
                k--;
                Assert.AreEqual(entries[j].Value, k, "Expected Value=" + k + ", Returned Value=" + (entries[j]));
                }

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestCacheEventFilter()
        {
            var filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.Keys, AlwaysFilter.Instance);
            var filter1 = new CacheEventFilter(AlwaysFilter.Instance);
            Assert.IsNotNull(filter);
            Assert.AreEqual(filter, filter1);
            Assert.AreEqual(filter.ToString(), filter1.ToString());
            Assert.AreEqual(filter.GetHashCode(), filter1.GetHashCode());
            Assert.AreEqual(filter.EventMask, filter1.EventMask);
            Assert.AreEqual(filter.Filter, filter1.Filter);

            filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.Inserted);
            Assert.IsNull(filter.Filter);
            Assert.AreNotEqual(filter, filter1);

            var evt = new CacheEventArgs(null, CacheEventType.Deleted, "key", "old value", "new value", false);
            Assert.IsFalse(filter.Evaluate(evt));

            filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.Inserted, AlwaysFilter.Instance);
            Assert.IsFalse(filter.Evaluate(evt));

            IFilter greater = new GreaterFilter(IdentityExtractor.Instance, 100);
            filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.Inserted, greater);
            evt = new CacheEventArgs(null, CacheEventType.Inserted, "key", 10, 101, false);
            Assert.IsTrue(filter.Evaluate(evt));

            evt = new CacheEventArgs(null, CacheEventType.Deleted, "key", 10, 101, false);
            filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.Deleted, greater);
            Assert.IsFalse(filter.Evaluate(evt));

            evt = new CacheEventArgs(null, CacheEventType.Updated, "key", 10, 101, false);
            filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.UpdatedEntered, greater);
            Assert.IsTrue(filter.Evaluate(evt));

            filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.UpdatedLeft, greater);
            Assert.IsFalse(filter.Evaluate(evt));

            filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.UpdatedWithin, greater);
            Assert.IsFalse(filter.Evaluate(evt));

            filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.Updated, greater);
            Assert.IsTrue(filter.Evaluate(evt));

            filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.UpdatedEntered | CacheEventFilter.CacheEventMask.UpdatedLeft, greater);
            Assert.IsTrue(filter.Evaluate(evt));

            filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.UpdatedWithin | CacheEventFilter.CacheEventMask.UpdatedEntered, greater);
            Assert.IsTrue(filter.Evaluate(evt));

            filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.UpdatedWithin | CacheEventFilter.CacheEventMask.UpdatedLeft, greater);
            Assert.IsFalse(filter.Evaluate(evt));
        }

        [Test]
        public void TestNullFilter()
        {
            var nullFilter = new NullFilter();
            Assert.IsNotNull(nullFilter);
            object o = null;
            Assert.IsFalse(nullFilter.Evaluate(o));
            o = new object();
            Assert.IsTrue(nullFilter.Evaluate(o));
            Assert.AreEqual(nullFilter.GetHashCode(), 0x0F);
            Assert.AreEqual(nullFilter.ToString(), "NullFilter");
            Assert.IsFalse(nullFilter.Equals(o));
            var nullFilter2 = new NullFilter();
            Assert.IsTrue(nullFilter.Equals(nullFilter2));

            //testing on remote cache
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();
            cache.Insert(1, 1);
            cache.Insert(2, null);
            cache.Insert(3, Int32.MinValue);
            cache.Insert(4, null);
            cache.Insert(5, null);
            ICollection results = cache.GetKeys(nullFilter);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(CollectionUtils.Contains(results, 1));
            Assert.IsTrue(CollectionUtils.Contains(results, 3));
            Assert.IsFalse(CollectionUtils.Contains(results, 5));
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestCacheEventTransformerFilter()
        {
            SemiLiteEventTransformer transformer = SemiLiteEventTransformer.Instance;
            var transFilter = new CacheEventTransformerFilter(null, transformer);
            var transFilterWithWrappedFilter =
                new CacheEventTransformerFilter(
                    new CacheEventFilter(CacheEventFilter.CacheEventMask.Inserted, AlwaysFilter.Instance), transformer);

            var localCache = new LocalCache();
            var evt = new CacheEventArgs(localCache, CacheEventType.Deleted, "key", "old value", "new value", false);
            Exception ex = null;
            var o = new object();

            Assert.IsNotNull(transFilter);
            Assert.IsNotNull(transFilterWithWrappedFilter);

            // test Evaluate
            // this filter cannot be used as general purpose filter
            try
            {
                transFilter.Evaluate(o);
            }
            catch (Exception e)
            {
                ex = e;
            }
            Assert.IsNotNull(ex);
            Assert.IsInstanceOf(typeof (InvalidOperationException), ex);

            // wrapped filter is null, evaluates always true
            Assert.IsTrue(transFilter.Evaluate(evt));

            // wrapped filter is not null
            Assert.IsFalse(transFilterWithWrappedFilter.Evaluate(evt));

            // test Transform
            CacheEventArgs evtNew = transFilter.Transform(evt);
            Assert.IsNotNull(evtNew);
            Assert.IsNotNull(evt.OldValue);
            Assert.IsNull(evtNew.OldValue);

            // test on remote cache
            INamedCache remoteCache = CacheFactory.GetCache(CacheName);

            remoteCache.Clear();

            var listener = new TestCacheListener();
            remoteCache.AddCacheListener(listener);
            remoteCache.Insert("1", 1);
            Assert.IsNotNull(listener.evt);
            Assert.IsNull(listener.evt.OldValue);
            remoteCache.Insert("1", 100);
            Assert.IsNotNull(listener.evt.OldValue);
            remoteCache.RemoveCacheListener(listener);

            remoteCache.Clear();

            remoteCache.AddCacheListener(listener, transFilter, false);
            remoteCache.Insert("1", 1);
            Assert.IsNotNull(listener.evt);
            Assert.IsNull(listener.evt.OldValue);
            remoteCache.Insert("1", 100);
            Assert.IsNull(listener.evt.OldValue);
            remoteCache.RemoveCacheListener(listener, transFilter);
            
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestFilterSerialization()
        {
            var ctx = new ConfigurablePofContext("assembly://Coherence.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.IsNotNull(ctx);

            var array = new ArrayList(new[] { 1, 3, 7 });

            IFilter alwaysFilter = AlwaysFilter.Instance;
            IFilter neverFilter = NeverFilter.Instance;
            IFilter presentFilter = PresentFilter.Instance;
            IFilter allFilter = new AllFilter(new[] { alwaysFilter, presentFilter });
            IFilter andFilter = new AndFilter(alwaysFilter, presentFilter);
            IFilter anyFilter = new AnyFilter(new[] { alwaysFilter, presentFilter });
            IFilter betweenFilter = new BetweenFilter("memberName1", 10, 15);
            IFilter cacheEventFilter = new CacheEventFilter(neverFilter);
            IFilter cacheEventTransformerFilter = new CacheEventTransformerFilter(allFilter, new SemiLiteEventTransformer());
            IFilter containsAllFilter = new ContainsAllFilter("memberName2", array);
            IFilter containsAnyFilter = new ContainsAnyFilter("memberName3", array);
            IFilter containsFilter = new ContainsFilter("memberName4", "testValue");
            IFilter equalsFilter = new EqualsFilter("memberName4", "testValue1");
            ICacheTrigger filterTrigger = new FilterTrigger(anyFilter, FilterTrigger.ActionCode.Rollback);
            IFilter greaterEqualsFilter = new GreaterEqualsFilter("memberName5", "testValue2");
            IFilter greaterFilter = new GreaterFilter("memberName6", "testValue3");
            IFilter inFilter = new InFilter("memberName6", array);
            IFilter inKeySetFilter = new InKeySetFilter(anyFilter, null);
            IFilter isNotNullFilter = new IsNotNullFilter("memberName7");
            IFilter isNullFilter = new IsNullFilter("memberName8");
            IFilter keyAssociatedFilter = new KeyAssociatedFilter(presentFilter, "hostKey1");
            IFilter lessEqualsFilter = new LessEqualsFilter("memberName9", "testValue4");
            IFilter lessFilter = new LessFilter("memberName10", "testValue5");
            IFilter likeFilter = new LikeFilter("memberName11", "testValue6");
            IFilter limitFilter = new LimitFilter(allFilter, 10);
            IFilter notEqualsFilter = new NotEqualsFilter("memberName12", "testValue7");
            IFilter notFilter = new NotFilter(allFilter);
            IFilter orFilter = new OrFilter(allFilter, betweenFilter);
            IFilter priorityFilter = new PriorityFilter((NeverFilter)neverFilter);
            IFilter valueChangeEventFilter = new ValueChangeEventFilter("memberName13");
            IFilter xorFilter = new XorFilter(allFilter, betweenFilter);

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), alwaysFilter);
            ctx.Serialize(new DataWriter(stream), neverFilter);
            ctx.Serialize(new DataWriter(stream), presentFilter);
            ctx.Serialize(new DataWriter(stream), allFilter);
            ctx.Serialize(new DataWriter(stream), andFilter);
            ctx.Serialize(new DataWriter(stream), anyFilter);
            ctx.Serialize(new DataWriter(stream), betweenFilter);
            ctx.Serialize(new DataWriter(stream), cacheEventFilter);
            ctx.Serialize(new DataWriter(stream), cacheEventTransformerFilter);
            ctx.Serialize(new DataWriter(stream), containsAllFilter);
            ctx.Serialize(new DataWriter(stream), containsAnyFilter);
            ctx.Serialize(new DataWriter(stream), containsFilter);
            ctx.Serialize(new DataWriter(stream), equalsFilter);
            ctx.Serialize(new DataWriter(stream), filterTrigger);
            ctx.Serialize(new DataWriter(stream), greaterEqualsFilter);
            ctx.Serialize(new DataWriter(stream), greaterFilter);
            ctx.Serialize(new DataWriter(stream), inFilter);
            ctx.Serialize(new DataWriter(stream), inKeySetFilter);
            ctx.Serialize(new DataWriter(stream), isNotNullFilter);
            ctx.Serialize(new DataWriter(stream), isNullFilter);
            ctx.Serialize(new DataWriter(stream), keyAssociatedFilter);
            ctx.Serialize(new DataWriter(stream), lessEqualsFilter);
            ctx.Serialize(new DataWriter(stream), lessFilter);
            ctx.Serialize(new DataWriter(stream), likeFilter);
            ctx.Serialize(new DataWriter(stream), limitFilter);
            ctx.Serialize(new DataWriter(stream), notEqualsFilter);
            ctx.Serialize(new DataWriter(stream), notFilter);
            ctx.Serialize(new DataWriter(stream), orFilter);
            ctx.Serialize(new DataWriter(stream), priorityFilter);
            ctx.Serialize(new DataWriter(stream), valueChangeEventFilter);
            ctx.Serialize(new DataWriter(stream), xorFilter);

            stream.Position = 0;
            var filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(alwaysFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(neverFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(presentFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(allFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(andFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(anyFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(betweenFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(cacheEventFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(cacheEventTransformerFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(((ContainsAllFilter)containsAllFilter).Value, ((ContainsAllFilter)filterResult).Value);
            Assert.AreEqual(((ContainsAllFilter)containsAllFilter).ValueExtractor, ((ContainsAllFilter)filterResult).ValueExtractor);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(((ContainsAnyFilter)containsAnyFilter).Value, ((ContainsAnyFilter)filterResult).Value);
            Assert.AreEqual(((ContainsAnyFilter)containsAnyFilter).ValueExtractor, ((ContainsAnyFilter)filterResult).ValueExtractor);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(((ContainsFilter)containsFilter).Value, ((ContainsFilter)filterResult).Value);
            Assert.AreEqual(((ContainsFilter)containsFilter).ValueExtractor, ((ContainsFilter)filterResult).ValueExtractor);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(equalsFilter, filterResult);

            var filterTriggerResult = (ICacheTrigger)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(filterTrigger, filterTriggerResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(greaterEqualsFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(greaterFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(((InFilter)inFilter).Value, ((InFilter)filterResult).Value);
            Assert.AreEqual(((InFilter)inFilter).ValueExtractor, ((InFilter)filterResult).ValueExtractor);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(((InKeySetFilter)inKeySetFilter).Filter, ((InKeySetFilter)filterResult).Filter);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(isNotNullFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(isNullFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(keyAssociatedFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(lessEqualsFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(lessFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(likeFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(((LimitFilter)limitFilter).Filter, ((LimitFilter)filterResult).Filter);
            Assert.AreEqual(((LimitFilter)limitFilter).BottomAnchor, ((LimitFilter)filterResult).BottomAnchor);
            Assert.AreEqual(((LimitFilter)limitFilter).Comparer, ((LimitFilter)filterResult).Comparer);
            Assert.AreEqual(((LimitFilter)limitFilter).Cookie, ((LimitFilter)filterResult).Cookie);
            Assert.AreEqual(((LimitFilter)limitFilter).Page, ((LimitFilter)filterResult).Page);
            Assert.AreEqual(((LimitFilter)limitFilter).PageSize, ((LimitFilter)filterResult).PageSize);
            Assert.AreEqual(((LimitFilter)limitFilter).TopAnchor, ((LimitFilter)filterResult).TopAnchor);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(notEqualsFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(notFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(orFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(((PriorityFilter)priorityFilter).Filter, ((PriorityFilter)filterResult).Filter);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(valueChangeEventFilter, filterResult);

            filterResult = (IFilter)ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(xorFilter, filterResult);

            stream.Close();
        }
    }
}
