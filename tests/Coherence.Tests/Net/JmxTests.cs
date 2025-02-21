/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Specialized;

using NUnit.Framework;

using Tangosol.Run.Xml;

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
            TestContext.Error.WriteLine($"[START] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
            namedCache = CacheFactory.GetCache(CacheName);
        }

        [TearDown]
        public void TearDown()
        {
            namedCache.CacheService.Shutdown();
            CacheFactory.Shutdown();
            TestContext.Error.WriteLine($"[END]   {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
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
