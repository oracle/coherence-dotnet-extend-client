/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace Tangosol.Net.Impl {

    [TestFixture]
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
            TestContext.Error.WriteLine($"[START] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
            var configFileName = "assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config-ssl.xml";
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                configFileName = "assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config-ssl-non-win.xml";
            }

            var ccf = new DefaultConfigurableCacheFactory(configFileName);
            CacheFactory.ConfigurableCacheFactory = ccf;
        }

        [TearDown]
        public void TearDown()
        {
            CacheFactory.Shutdown();
            TestContext.Error.WriteLine($"[END]   {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
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