/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;

using NUnit.Framework;

using Tangosol.IO.Pof;
using Tangosol.Net.Impl;
using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.Net
{
    [TestFixture]
    public class CacheFactoryTests
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
            CacheFactory.Shutdown();
            namedCache = CacheFactory.GetCache(CacheName);
        }
        
        [TearDown]
        public void TearDown()
        {
            CacheFactory.Shutdown();
            CacheFactory.GetCache(CacheName).Destroy();
            TestContext.Error.WriteLine($"[END]   {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
        }

        [Test]
        public void TestLogging()
        {
            CacheFactory.Log("Test Err", CacheFactory.LogLevel.Error);
            CacheFactory.Log("Test Warn", CacheFactory.LogLevel.Warn);
            CacheFactory.Log("Test Info", CacheFactory.LogLevel.Info);
            CacheFactory.Log("Test Debug", CacheFactory.LogLevel.Debug);
            CacheFactory.Log("Test Quiet", CacheFactory.LogLevel.Quiet);

            string msg = "Logging test error message";
            Exception e = new Exception("Logging test exception");
            CacheFactory.Log(e, CacheFactory.LogLevel.Warn);
            CacheFactory.Log(msg, e, CacheFactory.LogLevel.Error);
        }

        [Test]
        public void TestIsLogEnabled()
        {
            Assert.IsTrue(CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Error));
            Assert.IsTrue(CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Warn));
            Assert.IsTrue(CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Info));
            Assert.IsTrue(CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Debug));
            Assert.IsTrue(CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Quiet));
        }

        [Test]
        public void TestGetCache()
        {
            Assert.IsNotNull(namedCache);
            Assert.AreEqual(CacheName, namedCache.CacheName);
            Assert.IsTrue(namedCache.IsActive);
            CacheFactory.ReleaseCache(namedCache);
            Assert.IsFalse(namedCache.IsActive);
            Assert.IsTrue(namedCache.CacheService.IsRunning);
            CacheFactory.Shutdown();
            Assert.IsFalse(namedCache.CacheService.IsRunning);
        }

        [Test]
        public void TestGetCacheWithDestroy()
        {
            Assert.IsNotNull(namedCache);
            Assert.AreEqual(CacheName, namedCache.CacheName);
            Assert.IsTrue(namedCache.IsActive);
            CacheFactory.DestroyCache(namedCache);
            Assert.IsFalse(namedCache.IsActive);
            Assert.That(() => namedCache.Count.ToString(), Throws.InvalidOperationException);
        }

        [Test]
        public void TestRemoteInvocation()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            ccf.Config = config;

            IService service = CacheFactory.GetService("RemoteInvocationService");
            Assert.IsNotNull(service);
            Assert.IsInstanceOf(typeof(SafeInvocationService), service);
            SafeInvocationService sis = service as SafeInvocationService;
            Assert.IsNotNull(sis);
            Assert.IsTrue(sis.IsRunning);
            Assert.AreEqual(sis.ServiceName, "RemoteInvocationService");
            Assert.AreEqual(sis.ServiceType, ServiceType.RemoteInvocation);
            Assert.IsNotNull(sis.Service);
            Assert.IsInstanceOf(typeof(RemoteInvocationService), sis.Service);
            Assert.IsNotNull(sis.Serializer);
            Assert.IsInstanceOf(typeof(ConfigurablePofContext), sis.Serializer);
            RemoteInvocationService ris = sis.Service as RemoteInvocationService;
            Assert.IsNotNull(ris);
            Assert.IsTrue(ris.IsRunning);
            Assert.AreEqual(ris.ServiceName, "RemoteInvocationService");
            Assert.AreEqual(ris.ServiceType, ServiceType.RemoteInvocation);
            Assert.AreEqual(ris.OperationalContext.Edition, 1);
            Assert.IsNotNull(ris.OperationalContext.LocalMember);
            Assert.IsNotNull(ris.Serializer);
            Assert.IsInstanceOf(typeof(ConfigurablePofContext), ris.Serializer);

            IInvocable invocable = new EmptyInvocable();
            IDictionary result = sis.Query(invocable, null);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[ris.OperationalContext.LocalMember], 42);

            sis.Shutdown();
            Assert.IsFalse(sis.IsRunning);
        }

            [Test]
        public void TestMemberRemoteInvocation()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;
            IXmlDocument config           = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            ccf.Config                    = config;
            IInvocationService service    = (IInvocationService) CacheFactory.GetService("RemoteInvocationService");

            IMember member     = new LocalMember();
            member.MachineName = "machine1";
            member.MemberName  = "member1";
            member.ProcessName = "process1";
            member.RackName    = "rack1";
            member.RoleName    = "role1";
            member.SiteName    = "site1";

            POFObjectInvocable invocable = new POFObjectInvocable();
            invocable.PofObject          = member;

            IDictionary result = service.Query(invocable, null);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 1);
            IMember copy = (IMember) result[((DefaultConfigurableCacheFactory) CacheFactory
                    .ConfigurableCacheFactory).OperationalContext.LocalMember];
            Assert.AreEqual(member.MachineName, copy.MachineName);
            Assert.AreEqual(member.MemberName, copy.MemberName);
            Assert.AreEqual(member.ProcessName, copy.ProcessName);
            Assert.AreEqual(member.RackName, copy.RackName);
            Assert.AreEqual(member.RoleName, copy.RoleName);
            Assert.AreEqual(member.SiteName, copy.SiteName);

            service.Shutdown();
        }

            [Test]
        public void TestCompositeKeyRemoteInvocation()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;
            IXmlDocument config           = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            ccf.Config                    = config;
            IInvocationService service    = (IInvocationService)CacheFactory.GetService("RemoteInvocationService");

            CompositeKey key             = new CompositeKey("abc", "xyz");
            POFObjectInvocable invocable = new POFObjectInvocable();
            invocable.PofObject          = key;

            IDictionary result = service.Query(invocable, null);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 1);
            CompositeKey copy = (CompositeKey) result[((DefaultConfigurableCacheFactory)
                    CacheFactory.ConfigurableCacheFactory).OperationalContext.LocalMember];
            Assert.AreEqual(key, copy);

            service.Shutdown();
        }

            [Test]
        public void TestSerializerAccessor()
        {
            var ccf = CacheFactory.ConfigurableCacheFactory;
            var config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            ccf.Config = config;

            var cache = CacheFactory.GetCache("dist-extend-direct");
            Assert.IsNotNull(cache.CacheService.Serializer);

            var service = CacheFactory.GetService("RemoteInvocationService");
            Assert.IsNotNull(service.Serializer);
        }

        /// <summary>
        /// Verify concurrency control on initialization of CacheFactory's
        /// singleton CCF.  See COH-14256.
        /// </summary>
        [Test]
        public void TestConcurrentInit()
        {
            // close down the CCF created by SetUp()
            CacheFactory.Shutdown();

            Thread[] threads = new Thread[10];
            Runner[] runners = new Runner[threads.Length];

            for(int i = 0; i < threads.Length; ++i)
            {
                var runner = new Runner(CacheName, i);
                runners[i] = runner;
                threads[i] = new Thread(new ThreadStart(runner.Run));
            }

            foreach (var runner in runners)
            {
                Monitor.Enter(runner);
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            // wait for each runner to signal that it has started
            foreach (var runner in runners)
            {
                System.Console.WriteLine("Waiting for: " + runner);
                Monitor.Wait(runner);
            }

            // signal all runners to "go"
            foreach (var runner in runners)
            {
                Monitor.Pulse(runner);
                Monitor.Exit(runner);
            }

            for(int i = 0; i < threads.Length; ++i)
            {
                threads[i].Join();
                System.Console.WriteLine(runners[i] + " completed");
            }
        }

        [Test]
        [Ignore("Ignore Docker Test")]
        public void TestServiceLevelClusterNameCacheService()
        {
            // this will ultimately establish a connection using the cluster-name configuration either at the global or
            // service levels.
            var service1 = (RemoteCacheService) ((SafeCacheService)CacheFactory.GetService("ExtendTcpCacheService")).RunningService;
            var service2 = (RemoteCacheService) ((SafeCacheService)CacheFactory.GetService("ExtendTcpCacheServiceCN")).RunningService;
            Assert.That(service1.RemoteClusterName, Is.Null);
            Assert.That(service2.RemoteClusterName, Is.EqualTo("DotNetTest"));
        }
        
        [Test]
        [Ignore("Ignore Docker Test")]
        public void TestServiceLevelClusterNameInvocationService()
        {
            // this will ultimately establish a connection using the cluster-name configuration either at the global or
            // service levels.
            var service1 = (RemoteInvocationService) ((SafeInvocationService)CacheFactory.GetService("RemoteInvocationService")).RunningService;
            var service2 = (RemoteInvocationService) ((SafeInvocationService)CacheFactory.GetService("RemoteInvocationServiceCN")).RunningService;
            Assert.That(service1.RemoteClusterName, Is.Null);
            Assert.That(service2.RemoteClusterName, Is.EqualTo("DotNetTest"));
        }
        
        [Test]
        [Ignore("Ignore Docker Test")]
        public void TestServiceLevelUnknownClusterNameCacheService()
        {
            Assert.That(() => (RemoteCacheService) ((SafeCacheService)CacheFactory.GetService("ExtendTcpCacheServiceUnknownCN")).RunningService, Throws.Exception);
        }
        
        [Test]
        [Ignore("Ignore Docker Test")]
        public void TestServiceLevelUnknownClusterNameInvocationService()
        {
            Assert.That(() => (RemoteCacheService) ((SafeCacheService)CacheFactory.GetService("RemoteInvocationServiceUnknownCN")).RunningService, Throws.Exception);
        }

        #region inner class: Runner

        /// <summary>
        /// Worker that calls CacheFactory.GetCache
        /// </summary>
        private class Runner
        {
            #region Properties

            /// <summary>
            /// Cache to access.
            /// </summary>
            private string CacheName
            {
                get;
                set;
            }

            /// <summary>
            /// ID for this Runner - for debugging
            /// </summary>
            private int Id
            {
                get;
                set;
            }

            #endregion

            #region constructors

            /// <summary>
            /// Construct a Runner that will open a given cache.
            /// </summary>
            /// <param name="cacheName">Cache to open.</param>
            /// <param name="id">ID for this Runner - for debugging.</param>
            public Runner(string cacheName, int id)
            {
                CacheName = cacheName;
                Id        = id;
            }

            #endregion

            #region Runner methods

            /// <summary>
            /// Wait for signal, then open the cache.
            /// </summary>
            public void Run()
            {
				Thread.CurrentThread.Name = this.ToString();
                lock (this)
                {
                    Monitor.Pulse(this); // signal main thread that we've started
                    Monitor.Wait(this);  // wait for "go" signal
                }
                CacheFactory.GetCache(CacheName);
            }

            #endregion

            #region object overrides

            override public string ToString()
            {
                return "Runner{Id=" + Id + '}';
            }

            #endregion
        }

        #endregion
    }
}
