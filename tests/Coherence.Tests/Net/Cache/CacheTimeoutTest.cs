/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Threading;

using NUnit.Framework;

using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// Functional tests for using cache with ThreadTimeout.
    /// </summary>
    /// <author>lh 2.23.22</author>
    /// <since>14.1.2.0</since>
    [TestFixture]
    public class CacheTimeoutTest
    {
        private static INamedCache GetCache(String cacheName)
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config =
                XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            ccf.Config = config;

            return CacheFactory.GetCache(cacheName);
        }

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
         * Test GetCache() should timeout when a short timeout value is specified.
         */
        [Test]
        public void TestShouldInterruptWithGetCache()
        {
            try
            {
                using (ThreadTimeout t = ThreadTimeout.After(100))
                {
                    INamedCache cache = GetCache("dist-timeout");
                    for (int i = 0; i < 1000; i++)
                    {
                        cache.Add(i, "value" + i);
                    }

                    Assert.AreEqual(cache[5], "value5");
                    Assert.Fail("CacheFactory.GetCache should be interrupted!");
                }
            }
            catch (ThreadInterruptedException)
            {
                CacheFactory.Shutdown();
            }

            Assert.AreEqual(ThreadTimeout.RemainingTimeoutMillis, Int32.MaxValue);

            try
            {
                IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;
                using (ThreadTimeout t = ThreadTimeout.After(40000))
                {
                    INamedCache cache = GetCache("dist-timeout");
                    for (int i = 0; i < 1000; i++)
                    {
                        cache.Add(i, "value" + i);
                    }

                    Assert.AreEqual(cache[5], "value5");
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Cache operation got exception:" + e);
            }
        }
    }
}