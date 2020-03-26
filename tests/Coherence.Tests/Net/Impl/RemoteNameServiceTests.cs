/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Specialized;
using NUnit.Framework;

namespace Tangosol.Net.Impl
{
    [TestFixture]
    public class RemoteNameServiceTests
    {
        NameValueCollection appSettings = TestUtils.AppSettings;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
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
