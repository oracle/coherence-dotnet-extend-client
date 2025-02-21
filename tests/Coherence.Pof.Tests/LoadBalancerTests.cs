/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;

using NUnit.Framework;
using Tangosol.Net;

namespace Tangosol.Data
{
    [TestFixture]
    public class LoadBalancerTests
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