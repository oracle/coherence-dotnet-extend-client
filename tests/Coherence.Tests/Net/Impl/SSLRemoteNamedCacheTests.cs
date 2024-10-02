/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Collections.Specialized;

using NUnit.Framework;
using Tangosol.Run.Xml;

namespace Tangosol.Net.Impl {

    [TestFixture]
    [Platform(Exclude="Unix,Linux,MacOsX")]
    public class SSLRemoteNamedCacheTests
    {

        NameValueCollection appSettings = TestUtils.AppSettings;

        protected String OneWayCacheName
        {
            get { return appSettings.Get("sslOneWayCacheName"); }
            set { m_cacheName = value; }
        }

        protected String TwoWayCacheName
        {
            get { return appSettings.Get("sslTwoWayCacheName"); }
            set { m_cacheName = value; }
        }

        [SetUp]
        public void SetUp()
        {
            var ccf = new DefaultConfigurableCacheFactory(
                "assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config-ssl.xml");
            CacheFactory.ConfigurableCacheFactory = ccf;
        }

        [TearDown]
        public void TearDown()
        {
            CacheFactory.Shutdown();
        }

        [Test]
        public virtual void TestOneWayNamedCache()
        {
            Console.WriteLine(Directory.GetCurrentDirectory());

            INamedCache cache = CacheFactory.GetCache(OneWayCacheName);

            // INamedCache
            Assert.IsTrue(cache.IsActive);

            cache.Clear();
            Assert.AreEqual(cache.Count, 0);

            CacheFactory.ReleaseCache(cache);
            CacheFactory.Shutdown();
        }

        [Test]
        public virtual void TestTwoWayNamedCache()
        {

            Console.WriteLine(Directory.GetCurrentDirectory());
            INamedCache cache = CacheFactory.GetCache(TwoWayCacheName);

            // INamedCache
            Assert.IsTrue(cache.IsActive);

            cache.Clear();
            Assert.AreEqual(cache.Count, 0);

            CacheFactory.ReleaseCache(cache);
            CacheFactory.Shutdown();
        }

        /// <summary>
        /// The cache name.
        /// </summary>
        private string m_cacheName;
    }
}