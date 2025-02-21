/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using NUnit.Framework;

using Tangosol.Run.Xml;

namespace Tangosol.Net
{
/**
* Default Serializer Test Suite.
*/
    [TestFixture]
    public class DefaultSerializerTest
    {
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
        public void TestDefaultPofSerializer()
        {
            IXmlElement xmlConfig = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-extend-default-serializer-cache-config.xml");
            CacheFactory.Configure(xmlConfig,null);
            INamedCache cache = CacheFactory.GetCache("dist-default");
            cache.Clear();

            // create a key, and value
            String sKey   = "hello";
            String sValue = "grid";

            // insert the pair into the cache
            cache.Add(sKey, sValue);

            // get the key and value back
            IDictionary entries = cache.GetAll(cache.Keys);
            Assert.AreEqual(1, entries.Count);
            Assert.IsInstanceOf(typeof(IDictionary), entries);
            Assert.AreEqual(sValue, ((String)((IDictionary) entries)[sKey]));

            CacheFactory.ReleaseCache(cache);
        }

        [Test]
        public void TestNamedDefaultSerializer()
        {
            IXmlElement xmlConfig = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-extend-named-default-serializer-cache-config.xml");
            CacheFactory.Configure(xmlConfig,null);
            INamedCache cache = CacheFactory.GetCache("dist-default");
            cache.Clear();

            // create a key, and value
            String sKey   = "hello";
            String sValue = "grid";

            // insert the pair into the cache
            cache.Add(sKey, sValue);

            // get the key and value back
            IDictionary entries = cache.GetAll(cache.Keys);
            Assert.AreEqual(1, entries.Count);
            Assert.IsInstanceOf(typeof(IDictionary), entries);
            Assert.AreEqual(sValue, ((String)((IDictionary) entries)[sKey]));

            CacheFactory.ReleaseCache(cache);
        }

        [Test]
        public void TestNamedDefaultPofSerializer()
        {
            IXmlElement xmlConfig = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-extend-named-pof-default-serializer-cache-config.xml");
            CacheFactory.Configure(xmlConfig,null);
            INamedCache cache = CacheFactory.GetCache("dist-default");
            cache.Clear();

            // create a key, and value
            String sKey   = "hello";
            String sValue = "grid";

            // insert the pair into the cache
            cache.Add(sKey, sValue);

            // get the key and value back
            IDictionary entries = cache.GetAll(cache.Keys);
            Assert.AreEqual(1, entries.Count);
            Assert.IsInstanceOf(typeof(IDictionary), entries);
            Assert.AreEqual(sValue, ((String)((IDictionary) entries)[sKey]));

            CacheFactory.ReleaseCache(cache);
        }
    }
}