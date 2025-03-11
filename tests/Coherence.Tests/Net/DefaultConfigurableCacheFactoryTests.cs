/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using NUnit.Framework;

using Tangosol.Config;
using Tangosol.Net.Cache;
using Tangosol.Net.Impl;
using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.Net
{
    [TestFixture]
    public class DefaultConfigurableCacheFactoryTests
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
        public void TestInitialization()
        {
            DefaultConfigurableCacheFactory ccf = (DefaultConfigurableCacheFactory) CacheFactory.ConfigurableCacheFactory;
            Assert.IsNotNull(ccf.OperationalContext.LocalMember);
            Assert.IsInstanceOf(typeof(LocalMember), ccf.OperationalContext.LocalMember);
            Assert.AreEqual(ccf.OperationalContext.Edition, 1);

            Assert.IsInstanceOf(typeof(DefaultConfigurableCacheFactory), ccf);
            DefaultConfigurableCacheFactory dccf = (DefaultConfigurableCacheFactory) ccf;
            dccf.Shutdown();
        }

        [Test]
        public void TestFindSchemeMapping()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;
            DefaultConfigurableCacheFactory dccf = (DefaultConfigurableCacheFactory) ccf;
            DefaultConfigurableCacheFactory.CacheInfo ci = dccf.FindSchemeMapping("nr-2343535");
            Assert.IsNotNull(ci);
            Assert.AreEqual(ci.CacheName, "nr-2343535");
            Assert.AreEqual(ci.SchemeName, "example-near");

            ci = dccf.FindSchemeMapping("loc-test");
            Assert.IsNotNull(ci);
            Assert.AreEqual(ci.CacheName, "loc-test");
            Assert.AreEqual(ci.SchemeName, "example-local");

            dccf.Shutdown();
        }

        [Test]
        public void TestLocalNamedCacheInstancing()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            ccf.Config = config;

            INamedCache cache1 = ccf.EnsureCache("local-default");
            Assert.IsNotNull(cache1);
            Assert.IsInstanceOf(typeof(LocalNamedCache), cache1);
            LocalNamedCache lnc = cache1 as LocalNamedCache;
            Assert.IsNotNull(lnc);
            LocalCache lc1 = lnc.LocalCache;
            Assert.AreEqual(lc1.ExpiryDelay, LocalCache.DEFAULT_EXPIRE);
            Assert.AreEqual(lc1.FlushDelay, 0);
            Assert.AreEqual(lc1.HighUnits, LocalCache.DEFAULT_UNITS);
            Assert.AreEqual(lc1.CalculatorType, LocalCache.UnitCalculatorType.Fixed);
            Assert.AreEqual(lc1.EvictionType, LocalCache.EvictionPolicyType.Hybrid);

            INamedCache cache2 = ccf.EnsureCache("local-with-init");
            Assert.IsNotNull(cache2);
            Assert.IsInstanceOf(typeof(LocalNamedCache), cache1);
            lnc = cache2 as LocalNamedCache;
            Assert.IsNotNull(lnc);
            LocalCache lc2 = lnc.LocalCache;
            Assert.AreEqual(lc2.ExpiryDelay, 10);
            Assert.AreEqual(lc2.FlushDelay, 1000);
            Assert.AreEqual(lc2.HighUnits, 32000);
            Assert.AreEqual(lc2.LowUnits, 10);
            Assert.AreEqual(lc2.CalculatorType, LocalCache.UnitCalculatorType.Fixed);
            Assert.AreEqual(lc2.EvictionType, LocalCache.EvictionPolicyType.LRU);
            Assert.IsNotNull(lc2.CacheLoader);
            Assert.IsInstanceOf(typeof(TestLocalNamedCache), lc2.CacheLoader);

            INamedCache cache3 = ccf.EnsureCache("local-custom-impl");
            Assert.IsNotNull(cache3);
            Assert.IsInstanceOf(typeof(TestLocalNamedCache), cache3);

            INamedCache cache4 = ccf.EnsureCache("local-custom-impl-with-init");
            Assert.IsNotNull(cache4);
            Assert.IsInstanceOf(typeof(TestLocalNamedCache), cache4);
            TestLocalNamedCache tlnc = cache4 as TestLocalNamedCache;
            Assert.IsNotNull(tlnc);
            LocalCache lc4 = tlnc.LocalCache;
            Assert.AreEqual(lc4.ExpiryDelay, 60000);
            Assert.AreEqual(lc4.FlushDelay, 1000);
            Assert.AreEqual(lc4.HighUnits, 32000);
            Assert.AreEqual(lc4.LowUnits, 10);
            Assert.AreEqual(lc4.CalculatorType, LocalCache.UnitCalculatorType.Fixed);
            Assert.AreEqual(lc4.EvictionType, LocalCache.EvictionPolicyType.LFU);

            INamedCache cache5 = ccf.EnsureCache("local-ref");
            Assert.IsNotNull(cache5);
            Assert.IsInstanceOf(typeof(LocalNamedCache), cache5);
            lnc = cache5 as LocalNamedCache;
            Assert.IsNotNull(lnc);
            LocalCache lc5 = lnc.LocalCache;
            Assert.AreEqual(lc2.ExpiryDelay, lc5.ExpiryDelay);
            Assert.AreEqual(lc2.FlushDelay, lc5.FlushDelay);
            Assert.AreEqual(lc2.HighUnits, lc5.HighUnits);
            Assert.AreEqual(lc2.CalculatorType, lc5.CalculatorType);
            Assert.AreEqual(lc2.EvictionType, lc5.EvictionType);

            Assert.IsInstanceOf(typeof (DefaultConfigurableCacheFactory), ccf);
            DefaultConfigurableCacheFactory dccf = ccf as DefaultConfigurableCacheFactory;
            Assert.IsNotNull(dccf);
            DefaultConfigurableCacheFactory.CacheInfo ci = dccf.FindSchemeMapping("local-override-params");
            Assert.IsNotNull(ci);
            Assert.AreEqual(ci.CacheName, "local-override-params");
            Assert.AreEqual(ci.SchemeName, "example-local-7");
            IDictionary attrs = ci.Attributes;
            Assert.IsNotNull(attrs);
            Assert.AreEqual(attrs.Count, 1);
            Assert.IsTrue(attrs.Contains("LowUnits10"));

            INamedCache cache6 = ccf.EnsureCache("local-override-params");
            Assert.IsNotNull(cache6);
            Assert.IsInstanceOf(typeof(LocalNamedCache), cache6);
            lnc = cache6 as LocalNamedCache;
            Assert.IsNotNull(lnc);
            LocalCache lc6 = lnc.LocalCache;
            Assert.AreEqual(lc6.ExpiryDelay, LocalCache.DEFAULT_EXPIRE);
            Assert.AreEqual(lc6.FlushDelay, 0);
            Assert.AreEqual(lc6.HighUnits, 100);
            Assert.AreEqual(lc6.LowUnits, 10);
            Assert.AreEqual(lc6.CalculatorType, LocalCache.UnitCalculatorType.Fixed);
            Assert.AreEqual(lc6.EvictionType, LocalCache.EvictionPolicyType.LFU);

            Assert.IsNotNull(lc6.CacheLoader);
            ICacheLoader cl = lc6.CacheLoader;
            TestLoader testLoader = cl as TestLoader;
            Assert.IsNotNull(testLoader);
            Assert.AreEqual(testLoader.StringProperty, ci.CacheName);
            Assert.AreEqual(testLoader.IntProperty, 10);
            Assert.IsTrue(testLoader.BoolProperty);
            INamedCache c = testLoader.CacheProperty;
            Assert.IsNotNull(c);
            Assert.IsInstanceOf(typeof (LocalNamedCache), c);

            Assert.IsTrue(cache1.IsActive);
            Assert.IsTrue(cache2.IsActive);
            Assert.IsTrue(cache3.IsActive);
            Assert.IsTrue(cache4.IsActive);
            Assert.IsTrue(cache5.IsActive);
            Assert.IsTrue(cache6.IsActive);

            CacheFactory.Shutdown();

            Assert.IsFalse(cache1.IsActive);
            Assert.IsFalse(cache2.IsActive);
            Assert.IsFalse(cache3.IsActive);
            Assert.IsFalse(cache4.IsActive);
            Assert.IsFalse(cache5.IsActive);
            Assert.IsFalse(cache6.IsActive);
        }

        [Test]
        public void TestRemoteNamedCacheInstancing()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            INamedCache cache = ccf.EnsureCache("dist-extend-direct");
            Assert.IsNotNull(cache);
            Assert.IsInstanceOf(typeof(SafeNamedCache), cache);

            SafeNamedCache sc = cache as SafeNamedCache;
            Assert.IsNotNull(sc);
            Assert.AreEqual(sc.CacheName, "dist-extend-direct");
            Assert.IsInstanceOf(typeof(RemoteNamedCache), sc.NamedCache);
            RemoteNamedCache rc = sc.NamedCache as RemoteNamedCache;
            Assert.IsNotNull(rc);
            Assert.AreEqual(rc.CacheName, sc.CacheName);

            Assert.IsNotNull(sc.CacheService);
            Assert.IsInstanceOf(typeof(SafeCacheService), sc.CacheService);
            SafeCacheService scs = (SafeCacheService) sc.CacheService;
            Assert.AreEqual(scs.CacheNames.Count, 1);
            Assert.Contains(sc.CacheName, new ArrayList(scs.CacheNames));
            List<INamedCache> listCache = new List<INamedCache>(scs.StoreSafeNamedCache.GetAllCaches());
            Assert.AreEqual(listCache.Count, 1);
            Assert.Contains(sc.CacheName, new List<string>(scs.StoreSafeNamedCache.GetNames()));
            Assert.AreEqual(scs.ServiceType, ServiceType.RemoteCache);

            Assert.IsInstanceOf(typeof(RemoteCacheService), scs.CacheService);
            Assert.AreEqual(rc.CacheService, scs.CacheService);
            RemoteCacheService rcs = scs.CacheService as RemoteCacheService;
            Assert.IsNotNull(rcs);
            Assert.AreEqual(rcs.ServiceName, "ExtendTcpCacheService");

            Assert.IsTrue(cache.IsActive);
            Assert.IsTrue(cache.CacheService.IsRunning);

            CacheFactory.Shutdown();

            Assert.IsFalse(cache.IsActive);
            Assert.IsFalse(cache.CacheService.IsRunning);
        }

        [Test]
        public void TestNearNamedCacheInstancing()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-near-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = ccf.EnsureCache("dist-extend-direct");
            Assert.IsNotNull(cache);
            Assert.IsInstanceOf(typeof(NearCache), cache);

            NearCache nc = cache as NearCache;
            Assert.IsNotNull(nc);
            ICache fc = nc.FrontCache;
            INamedCache bc = nc.BackCache;
            Assert.IsNotNull(fc);
            Assert.IsNotNull(bc);

            Assert.IsInstanceOf(typeof(SafeNamedCache), nc.BackCache);
            Assert.IsInstanceOf(typeof(LocalNamedCache), nc.FrontCache);

            SafeNamedCache sc = nc.BackCache as SafeNamedCache;
            Assert.IsNotNull(sc);
            Assert.AreEqual(sc.CacheName, "dist-extend-direct");
            Assert.IsInstanceOf(typeof(RemoteNamedCache), sc.NamedCache);
            RemoteNamedCache rc = sc.NamedCache as RemoteNamedCache;
            Assert.IsNotNull(rc);
            Assert.AreEqual(rc.CacheName, sc.CacheName);

            LocalNamedCache lnc = nc.FrontCache as LocalNamedCache;
            Assert.IsNotNull(lnc);
            LocalCache lc = lnc.LocalCache;
            Assert.AreEqual(lc.ExpiryDelay, LocalCache.DEFAULT_EXPIRE);
            Assert.AreEqual(lc.FlushDelay, 0);
            Assert.AreEqual(lc.HighUnits, LocalCache.DEFAULT_UNITS);
            Assert.AreEqual(lc.CalculatorType, LocalCache.UnitCalculatorType.Fixed);
            Assert.AreEqual(lc.EvictionType, LocalCache.EvictionPolicyType.Hybrid);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestInvalidConfig()
        {
            DefaultConfigurableCacheFactory ccf = null;
            try
            {
                ccf = new DefaultConfigurableCacheFactory("assembly://Coherence.Tests/Tangosol.Resources/s4hc-invalid-cache-config.xml");
                ccf.OperationalContext = ((DefaultConfigurableCacheFactory) CacheFactory
                        .ConfigurableCacheFactory).OperationalContext;
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
            }
            finally
            {
                Assert.IsNull(ccf);
            }
        }

        [Test]
        public void TestInvalidConfig2()
        {
            DefaultConfigurableCacheFactory ccf = null;
            try
            {
                ccf = new DefaultConfigurableCacheFactory("assembly://Coherence.Tests/Tangosol.Resources/s4hc-invalid-cache-config-ns.xml");
                ccf.OperationalContext = ((DefaultConfigurableCacheFactory) CacheFactory
                        .ConfigurableCacheFactory).OperationalContext;
            }
            catch
            {}
            finally
            {
                Assert.IsNull(ccf);
            }
        }

        [Test]
        public void TestWhiteSpaceParsing()
        {
            DefaultConfigurableCacheFactory ccf                 = null;
            DefaultConfigurableCacheFactory.CacheInfo cacheInfo = null;
            try
            {
                ccf       = new DefaultConfigurableCacheFactory("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config-with-spaces.xml");
                cacheInfo = ccf.FindSchemeMapping("test-GetsPutsCache");
            }
            catch
            {}
            finally
            {
               Assert.IsTrue(cacheInfo != null);
               StringAssert.AreEqualIgnoringCase(cacheInfo.CacheName, "test-GetsPutsCache");
               StringAssert.AreEqualIgnoringCase(cacheInfo.SchemeName, "test-local");
            }
        }

        /// <summary>
        /// This test uses a cache config file that does not exist.
        /// </summary>
        [Test]
        public void TestCacheConfigNonExistent()
        {
            IXmlDocument config = null;
            try
            {
                config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/bogus-cache-config.xml");
            }
            catch (IOException e)
            {
                Assert.IsTrue(e.Message.Contains("Could not load"));
            }
            Assert.IsNull(config);
        }

        /// <summary>
        /// This test uses a cache config file with typos: 
        /// <cache-name>*<cache-name> instead of <cache-name>*</cache-name>.
        /// </summary>
        [Test]
        public void TestCacheConfigIllegal()
        {
            IXmlDocument config = null;
            try
            {
                config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config-illegal.xml");
            }
            catch (XmlException e)
            {
                Console.Out.WriteLine(e);
                Assert.IsTrue(e.Message.Contains("cache:cache-name"));
            }
            Assert.IsNull(config);
        }

        /// <summary>
        /// Test the default config contained in coherence.dll
        /// </summary>
        [Test]
        [Ignore("Ignore Docker Test")]
        public void TestEmbeddedCacheConfig()
        {
            var                       coherence   = (CoherenceConfig) ConfigurationUtils.GetCoherenceConfiguration();
            var                       oldResource = coherence.CacheConfig;
            IConfigurableCacheFactory ccf         = null;

            try
            {
                coherence.CacheConfig = null; // null out configured cache config to force loading the default
                ccf                   = new DefaultConfigurableCacheFactory();

                INamedCache cache = ccf.EnsureCache("foo");
                Assert.AreEqual("RemoteCache", ((SafeCacheService) cache.CacheService).ServiceName);
                ccf.DestroyCache(cache);
            }
            finally
            {
                coherence.CacheConfig = oldResource; // restore
                if (ccf != null)
                {
                    ccf.Shutdown();
                }
            }
        }
    }
}