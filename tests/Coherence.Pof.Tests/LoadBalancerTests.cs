/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Threading;

using NUnit.Framework;
using Tangosol.Net;

namespace Tangosol.Data
{
    [TestFixture]
    [Ignore("Ignore Docker Test")]
    public class LoadBalancerTests
    {
        [Test]
        public virtual void TestLoadBalancer()
        {
            // test that the two remote services connect to a different
            // proxy service
            var service1 = (ICacheService)CacheFactory
                    .GetService("ExtendTcpCacheService");
            var service2 = (ICacheService)CacheFactory
                    .GetService("ExtendTcpCacheService2");
            INamedCache cache1 = service1.EnsureCache("local-test");
            INamedCache cache2 = service2.EnsureCache("local-test");

            cache1.Clear();
            cache2.Clear();
            Object key = "key";
            cache1[key] = "value";

            Assert.IsFalse(cache2.Contains(key),
                    "connected to the same proxy service");

            CacheFactory.Shutdown();
        }
    }
}