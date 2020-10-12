/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Specialized;

using NUnit.Framework;

using Tangosol.IO.Pof;
using Tangosol.Net.Impl;
using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.Net
{
    [TestFixture]
    public class JmxTests
    {
        NameValueCollection appSettings = TestUtils.AppSettings;

        INamedCache namedCache;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
        }

        [SetUp]
        public void SetUp()
        {
            namedCache = CacheFactory.GetCache(CacheName);
        }

        [TearDown]
        public void TearDown()
        {
            namedCache.CacheService.Shutdown();
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestJmxConnectionInformation()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;
            IXmlDocument config           = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            ccf.Config                    = config;
            IInvocationService service    = (IInvocationService) CacheFactory.GetService("RemoteInvocationService");
            IMember member                = ((DefaultConfigurableCacheFactory) ccf).OperationalContext.LocalMember;

            MBeanInvocable invocable = new MBeanInvocable();

            IDictionary result = service.Query(invocable, null);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 1);

            Assert.IsNotNull(result[member]);

            service.Shutdown();
        }
    }
}
