/*
 * Copyright (c) 2000, 2022, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using NUnit.Framework;

using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// Functional test for using cache with ThreadTimeout.
    /// </summary>
    /// <author>lh 2.23.22</author>
    /// <since>14.1.2.0</since>
    [TestFixture]
    public class CacheWithTimeoutTest
    {
        private static INamedCache GetCache(String cacheName)
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config =
                XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            ccf.Config = config;

            return CacheFactory.GetCache(cacheName);
        }

        /**
         * Test a simple GetCache() with ThreadTimeout.
         */
        [Test]
        public void TestShouldGetCacheWithTimeout()
        {
            INamedCache cache;
            using (ThreadTimeout t = ThreadTimeout.After(10000))
            {
                cache = GetCache("dist-default");
            }

            cache.Add("A", 1);
            Assert.AreEqual(cache["A"], 1);
        }

        /**
         * Test a simple GetCache()
         */
        [Test]
        public void TestShouldGetCache()
        {
            INamedCache cache;
            cache = GetCache("dist-default");

            cache.Add("A", 1);
            Assert.AreEqual(cache["A"], 1);
        }
    }
}

