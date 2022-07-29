/*
 * Copyright (c) 2000, 2022, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Threading;

using NUnit.Framework;
using Tangosol.Net.Messaging;
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
    public class CacheInvalidAddressTimeoutTest
    {
        private static INamedCache GetCache(String cacheName)
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config =
                XmlHelper.LoadXml("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            ccf.Config = config;

            return CacheFactory.GetCache(cacheName);
        }

        /**
         * Test GetCache() times out with a cache name of a cache service that has
         * a bad address; then succeed with a good cache name.
         */
        [Test]
        public void TestShouldInterruptWithGetCache()
        {
            Exception e1 = null;
            Thread thread = new Thread(() =>
            {
                try
                {
                    using (ThreadTimeout t = ThreadTimeout.After(200))
                    {
                        // a cache with bad address
                        INamedCache cache = GetCache("bad-timeout");
                        Assert.Fail("CacheFactory.GetCache should be interrupted!");
                    }
                }
                catch (Exception e)
                {
                    e1 = e;
                }
            });
            thread.Start();
            thread.Join();

            if (e1 == null)
            {
                Assert.Fail("CacheFactory.getCache() should failed to get the cache!");
            }
            else
            {
                Assert.IsTrue(e1 is ThreadInterruptedException || e1 is ConnectionException);
            }

            using (ThreadTimeout t = ThreadTimeout.After(20000))
            {
                try
                {
                    INamedCache cache = GetCache("dist-cache");
                    cache.Add("A", 1);
                    Assert.AreEqual(cache["A"], 1);
                }
                catch (Exception e)
                {
                    Assert.Fail("CacheFactory.GetCache failed with exception: " + e);
                }
            }
        }
    }
}