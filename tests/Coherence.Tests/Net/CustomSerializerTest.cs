/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System;

using NUnit.Framework;

using Tangosol.Run.Xml;

namespace Tangosol.Net
{
/**
* Custom Serializer Test Suite.
*/
    [TestFixture]
    public class CustomSerializerTest
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

        /**
        * Test a custom serializer; matches with custom serializer in Java
        */
        [Test]
        public void TestCustomSerializer()
        {
            String sReturnedValue = "TestSerializer";
            String cacheName      = "custom-serializer";
            IXmlDocument config   = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-extend-custom-serializer-cache-config.xml");

            runTest(config, cacheName, sReturnedValue);
        }

        /**
        * Test custom XML Configurable Serializer.
        */

        [Test]
        public void TestCustomSerializerXML()
        {

            String sReturnedValue = "TestSerializerXmlConfigurable";
            String cacheName      = "custom-serializer-xmlconfigurable";
            IXmlDocument config   = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-extend-custom-serializerxml-cache-config.xml");

            runTest(config, cacheName, sReturnedValue);
        }

        /**
        * Test custom POF serializer() with java-side default pof serializer.
        */

        [Test]
        public void TestCustomPofSerializer()
        {
            String sReturnedValue = "TestPofSerializer";
            String cacheName      = "custom-pof-serializer";
            IXmlDocument config   = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-extend-custom-pof-serializer-cache-config.xml");

            runTest(config, cacheName, sReturnedValue);
        }

        /**
        * Test a custom serializer defined in default element; matches with 
        * custom serializer in Java
        */
        [Test]
        public void TestCustomSerializerAsDefault()
        {
            String sReturnedValue = "TestSerializer";
            String cacheName      = "custom-serializer";
            IXmlDocument config   = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-extend-custom-serializer-as-default-cache-config.xml");

            runTest(config, cacheName, sReturnedValue);
        }

        /**
        * Test a custom serializer defined in default element; matches with 
        * custom serializer in Java
        */
        [Test]
        public void TestCustomSerializerAsNamedDefault()
        {
            String sReturnedValue = "TestSerializerXmlConfigurable";
            String cacheName      = "custom-serializer";
            IXmlDocument config   = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-extend-custom-serializerxml-as-named-default-cache-config.xml");

            runTest(config, cacheName, sReturnedValue);
        }

        /**
        * Test a custom serializer defined in config by name; matches with 
        * custom serializer in Java
        */
        [Test]
        public void TestCustomSerializerByName()
        {
            String sReturnedValue = "TestSerializerXmlConfigurable";
            String cacheName      = "custom-serializer-xmlconfigurable";
            IXmlDocument config   = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-extend-custom-serializerxml-byname-cache-config.xml");

            runTest(config, cacheName, sReturnedValue);
        }

        /*
        * common method for tests of this suite
        */
        private void runTest(IXmlDocument config, string cacheName, string serializerName)
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;
            IXmlElement originalConfig    = ccf.Config;

            ccf.Config = config;

            INamedCache cache = ccf.EnsureCache(cacheName);
            cache.Clear();

            // create a key, and value
            String sKey   = "hello";
            String sValue = "grid";

            // insert the pair into the cache
            cache.Insert(sKey, sValue);

            // read back the value, custom serializer should have converted
            Assert.AreEqual(cache.Count, 1); 
            Assert.AreEqual(cache[sKey], serializerName);

            ccf.DestroyCache(cache);
            ccf.Config = originalConfig;
        }
    }
}
