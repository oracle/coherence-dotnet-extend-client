/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

using NUnit.Framework;

using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;

namespace Tangosol.Util
{
    [TestFixture]
    public class FilterEnumeratorTests
    {
        [Test]
        public void TestFilterEnumerator()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache("local-default");

            cache.Clear();

            Hashtable ht = new Hashtable();
            ht.Add("Key1", 435);
            ht.Add("Key2", 253);
            ht.Add("Key3", 3);
            ht.Add("Key4", 200);
            ht.Add("Key5", 333);
            cache.InsertAll(ht);

            GreaterFilter filter = new GreaterFilter(IdentityExtractor.Instance, 200);
            FilterEnumerator filterEnumerator = new FilterEnumerator(cache.GetEnumerator(), filter);

            ArrayList results = new ArrayList();
            while(filterEnumerator.MoveNext())
            {
                object o = filterEnumerator.Current;
                Assert.IsNotNull(o);
                results.Add(o);
            }

            Assert.AreEqual(3, results.Count);

            foreach (ICacheEntry value in results)
            {
                Assert.IsTrue((int)value.Value > 200);
            }

            filterEnumerator.Reset();
            results.Clear();
            while (filterEnumerator.MoveNext())
            {
                object o = filterEnumerator.Current;
                Assert.IsNotNull(o);
                results.Add(o);
            }

            Assert.AreEqual(3, results.Count);

            foreach (ICacheEntry value in results)
            {
                Assert.IsTrue((int)value.Value > 200);
            }


            filterEnumerator.Reset();
            results.Clear();
            filter = new GreaterFilter(IdentityExtractor.Instance, 400);
            filterEnumerator = new FilterEnumerator(cache.GetEnumerator(), filter);
            results = new ArrayList();
            while (filterEnumerator.MoveNext())
            {
                object o = filterEnumerator.Current;
                Assert.IsNotNull(o);
                results.Add(o);
            }
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(ht["Key1"], ((ICacheEntry)results[0]).Value);

            filterEnumerator.Reset();
            results.Clear();
            filter = new GreaterFilter(IdentityExtractor.Instance, 600);
            filterEnumerator = new FilterEnumerator(cache.GetEnumerator(), filter);
            results = new ArrayList();
            while (filterEnumerator.MoveNext())
            {
                object o = filterEnumerator.Current;
                Assert.IsNotNull(o);
                results.Add(o);
            }
            Assert.AreEqual(0, results.Count);

            CacheFactory.Shutdown();
        }
    }
}
