/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using NUnit.Framework;
using Tangosol.Net.Cache;
using Tangosol.Net.Messaging.Impl.NamedCache;
using Tangosol.Util;

namespace Tangosol.Net.Impl
{
    [TestFixture]
    public class TLS12OneWayRemoteNamedCacheTests : RemoteNamedCacheTests
    {
        protected override String TestCacheName
        {
            get { return "tls12CacheName"; }
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