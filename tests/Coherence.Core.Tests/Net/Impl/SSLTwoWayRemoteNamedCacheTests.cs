/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Runtime.InteropServices;
using System.IO;

using NUnit.Framework;

namespace Tangosol.Net.Impl
{
    [TestFixture]
    public class SSLTwoWayRemoteNamedCacheTests : RemoteNamedCacheTests
    {
        protected override String TestCacheName
        {
            get { return "sslTwoWayCacheName"; }
        }

        [SetUp]
        public void SetUp()
        {
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
        }

        [Test]
        public virtual void TestNamedCache()
        {
            INamedCache cache = CacheFactory.GetCache(CacheName);

            Assert.IsTrue(cache.IsActive);

            cache.Clear();
            Assert.AreEqual(cache.Count, 0);

            CacheFactory.ReleaseCache(cache);
            CacheFactory.Shutdown();
        }

        [Test]
        public override void TestNamedCacheLock()
        {
            /*
             * Do nothing because lock doesn't work with SSL port.
             * Opened COH-12272 to track this issue.
             */
        }
    }
}