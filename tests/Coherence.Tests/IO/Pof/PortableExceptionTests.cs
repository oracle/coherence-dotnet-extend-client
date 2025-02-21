/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System;
using System.IO;

using NUnit.Framework;

using Tangosol.Net;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;
using Tangosol.Net.Messaging.Impl.CacheService;
using Tangosol.Net.Messaging.Impl.NamedCache;
using Tangosol.Run.Xml;
using Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class PortableExceptionTests
    {
        TcpInitiator initiator;

        [SetUp]
        public void SetUp()
        {
            TestContext.Error.WriteLine($"[START] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
            Stream stream          = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-cache-config.xml");
            IXmlDocument xmlConfig = XmlHelper.LoadXml(stream);
            IXmlElement initConfig =
                    xmlConfig.FindElement("caching-schemes/remote-cache-scheme/initiator-config");

            initiator = new TcpInitiator
                {
                    OperationalContext = new DefaultOperationalContext()
                };
            initiator.Configure(initConfig);
            initiator.RegisterProtocol(CacheServiceProtocol.Instance);
            initiator.RegisterProtocol(NamedCacheProtocol.Instance);
            initiator.Start();
        }

        [TearDown]
        public void TearDown()
        {
            initiator.Connection.Close();
            initiator.Stop();
            TestContext.Error.WriteLine($"[END]   {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
        }

        [Test]
        public void PortableExceptionConstructorTest()
        {
            IConnection conn = initiator.EnsureConnection();
            Channel cacheServiceChannel = (Channel) conn.OpenChannel(CacheServiceProtocol.Instance,
                                                                     "CacheServiceProxy", null, null);

            EnsureCacheRequest ensureCacheRequest =
                    (EnsureCacheRequest)cacheServiceChannel.MessageFactory.CreateMessage(EnsureCacheRequest.TYPE_ID);

            ensureCacheRequest.CacheName = "nonexisting";
            Assert.AreEqual(EnsureCacheRequest.TYPE_ID, ensureCacheRequest.TypeId);

            IStatus ensureCacheStatus = cacheServiceChannel.Send(ensureCacheRequest);
            Assert.IsInstanceOf(typeof(EnsureCacheRequest), ensureCacheRequest);
            Assert.AreEqual(EnsureCacheRequest.TYPE_ID, ensureCacheRequest.TypeId);
            try
            {
                ensureCacheStatus.WaitForResponse(-1);
            }
            catch (PortableException pe)
            {
                Assert.IsNotNull(pe);
                Assert.IsTrue(pe.Name.StartsWith("Portable("));
                Assert.IsNull(pe.InnerException);
                Assert.IsTrue(pe.Message.ToLower().IndexOf("no scheme") >= 0);

                string[] fullStackTrace = pe.FullStackTrace;
                string stackTrace = pe.StackTrace;
                Assert.IsTrue(fullStackTrace.Length > 0);
                foreach (string s in fullStackTrace)
                {
                    Assert.IsTrue(stackTrace.IndexOf(s) > 0);
                }
                Assert.IsTrue(stackTrace.IndexOf("process boundary") > 0);
                Assert.IsTrue(
                    pe.ToString().Equals(pe.Name + ": " + pe.Message));

                using (Stream stream = new MemoryStream())
                {
                    DataWriter writer = new DataWriter(stream);
                    IPofContext ctx = (IPofContext) cacheServiceChannel.Serializer;
                    ctx.Serialize(writer, pe);
                    stream.Position = 0;
                    DataReader reader = new DataReader(stream);
                    PortableException result = (PortableException)ctx.Deserialize(reader);

                    Assert.AreEqual(pe.Name, result.Name);
                    Assert.AreEqual(pe.Message, result.Message);
                }
            }
        }

        [Test]
        public void PortableExceptionSerializationTest()
        {
            IConnection conn = initiator.EnsureConnection();
            Channel cacheServiceChannel = (Channel)conn.OpenChannel(CacheServiceProtocol.Instance,
                                                                     "CacheServiceProxy", null, null);

            EnsureCacheRequest ensureCacheRequest =
                    (EnsureCacheRequest) cacheServiceChannel.MessageFactory.CreateMessage(EnsureCacheRequest.TYPE_ID);

            ensureCacheRequest.CacheName = "nonexisting";

            IStatus ensureCacheStatus = cacheServiceChannel.Send(ensureCacheRequest);

            try
            {
                ensureCacheStatus.WaitForResponse(-1);
            }
            catch (PortableException pe)
            {
                IPofContext ctx = (IPofContext) cacheServiceChannel.Serializer;
                byte[] buffer = new byte[1024 * 16];
                Stream stream = new MemoryStream(buffer);
                DataWriter writer = new DataWriter(stream);
                ctx.Serialize(writer, pe);
                stream.Close();

                stream = new MemoryStream(buffer);
                DataReader reader = new DataReader(stream);
                PortableException desPE = (PortableException)ctx.Deserialize(reader);
                Assert.IsNotNull(desPE);
                stream.Close();

                // these values should be initialized before serialization
                Assert.AreEqual(pe.Name, desPE.Name);
                Assert.AreEqual(pe.Message, desPE.Message);
                Assert.AreEqual(pe.InnerException, desPE.InnerException);
                Assert.IsNull(pe.InnerException);
                Assert.AreEqual(pe.FullStackTrace, desPE.FullStackTrace);
            }
       }
    }
}
