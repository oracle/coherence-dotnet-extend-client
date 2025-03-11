/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Specialized;
using NUnit.Framework;

namespace Tangosol.Net.Impl
{
    [TestFixture]
    [Ignore("Ignore Docker Test")]
    public class RemoteNameServiceTests
    {
        NameValueCollection appSettings = TestUtils.AppSettings;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
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

        [Test]
        public void TestRemoteNameService()
        {
            IConfigurableCacheFactory ccf =
                            new DefaultConfigurableCacheFactory(
                                "assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config-nameservice.xml");

            try
            {
                INamedCache cache = ccf.EnsureCache(CacheName);
                cache.Clear();
            }
            finally
            {
                ccf.Shutdown();
            } 
        }
    }
}
