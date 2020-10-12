/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;

using NUnit.Framework;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Messaging.Impl.NamedCache;
using Tangosol.Run.Xml;
using Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator;

namespace Tangosol.Net.Messaging.Impl.CacheService {

    [TestFixture]
    public class CacheServiceTests
    {
        readonly NameValueCollection appSettings = TestUtils.AppSettings;

        TcpInitiator initiator;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
        }

        private String CacheNameTemp
        {
            get { return appSettings.Get("cacheNameTemp"); }
        }

        [SetUp]
        public void SetUp()
        {
            Stream stream          = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-cache-config.xml");
            IXmlDocument xmlConfig = XmlHelper.LoadXml(stream);
            IXmlElement initConfig =
                    xmlConfig.FindElement("caching-schemes/remote-cache-scheme/initiator-config");

            stream = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-test-coherence.xml");
            xmlConfig = XmlHelper.LoadXml(stream);
            IXmlElement filterConfig = xmlConfig.FindElement("cluster-config/filters");

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
        }

        [Test]
        public void TestCacheService()
        {
            IConnection conn = initiator.EnsureConnection();

            // this will test the protocol and factory
            IChannel cacheServiceChannel = conn.OpenChannel(CacheServiceProtocol.Instance,
                                                     "CacheServiceProxy", null, null);
            IMessageFactory mf = cacheServiceChannel.MessageFactory;
            Assert.IsNotNull(mf);
            Assert.IsInstanceOf(typeof(CacheServiceFactory), mf);
            Assert.IsInstanceOf(typeof(CacheServiceProtocol), cacheServiceChannel.MessageFactory.Protocol);

            // create all message types, that will test messages code
            IMessage ensureCache = cacheServiceChannel.MessageFactory.CreateMessage(EnsureCacheRequest.TYPE_ID);
            IMessage destroyCache = cacheServiceChannel.MessageFactory.CreateMessage(DestroyCacheRequest.TYPE_ID);
            Assert.IsNotNull(ensureCache);
            Assert.IsNotNull(destroyCache);
            Assert.IsInstanceOf(typeof(EnsureCacheRequest), ensureCache);
            Assert.IsInstanceOf(typeof(DestroyCacheRequest), destroyCache);
        }

        [Test]
        public void TestEnsureCacheRequest()
        {
            IConnection conn = initiator.EnsureConnection();

            IChannel cacheServiceChannel = conn.OpenChannel(CacheServiceProtocol.Instance,
                                                     "CacheServiceProxy", null, null);
            EnsureCacheRequest ensureCacheRequest =
                    (EnsureCacheRequest)cacheServiceChannel.MessageFactory.CreateMessage(EnsureCacheRequest.TYPE_ID);
            ensureCacheRequest.CacheName = CacheName;

            IResponse ensureCacheResponse = cacheServiceChannel.Send(ensureCacheRequest).WaitForResponse(-1);
            Assert.IsInstanceOf(typeof(EnsureCacheRequest), ensureCacheRequest);
            Assert.AreEqual(EnsureCacheRequest.TYPE_ID, ensureCacheRequest.TypeId);
            Assert.IsInstanceOf(typeof(CacheServiceResponse), ensureCacheResponse);
            Assert.IsFalse(ensureCacheResponse.IsFailure);
            Assert.IsNotNull(ensureCacheResponse.Result);
            Assert.AreEqual(ensureCacheResponse.RequestId, ensureCacheRequest.Id);
            Assert.AreEqual(0, ensureCacheResponse.TypeId);

            String response = (String) ensureCacheResponse.Result;
            Uri uri = new Uri(response);
            Assert.IsNotNull(uri);

            IChannel namedCacheChannel = conn.AcceptChannel(uri, null, new GenericPrincipal(new GenericIdentity("test"), null));
            Assert.IsNotNull(namedCacheChannel);

            string channelUri = "channel:" + namedCacheChannel.Id + "#NamedCacheProtocol";
            Assert.AreEqual(channelUri, uri.ToString());
        }

        [Test]
        public void TestEnsureCacheRequestException()
        {
            IConnection conn = initiator.EnsureConnection();

            Channel cacheServiceChannel = (Channel) conn.OpenChannel(CacheServiceProtocol.Instance,
                                                                     "CacheServiceProxy", null, null);

            EnsureCacheRequest ensureCacheRequest =
                    (EnsureCacheRequest)cacheServiceChannel.MessageFactory.CreateMessage(EnsureCacheRequest.TYPE_ID);
            ensureCacheRequest.CacheName = null;
            Assert.AreEqual(EnsureCacheRequest.TYPE_ID, ensureCacheRequest.TypeId);
            Assert.IsInstanceOf(typeof(EnsureCacheRequest), ensureCacheRequest);
            Assert.AreEqual(EnsureCacheRequest.TYPE_ID, ensureCacheRequest.TypeId);

            IStatus ensureCacheStatus = cacheServiceChannel.Send(ensureCacheRequest);
            try
            {
                ensureCacheStatus.WaitForResponse(-1);
            }
            catch (PortableException)
            {
                Assert.IsNotNull(ensureCacheStatus);
            }
        }

        [Test]
        public void TestDestroyCacheRequest()
        {
            IConnection conn = initiator.EnsureConnection();

            Channel cacheServiceChannel = (Channel) conn.OpenChannel(CacheServiceProtocol.Instance,
                                                                     "CacheServiceProxy", null, null);
            EnsureCacheRequest ensureCacheRequest =
                    (EnsureCacheRequest)cacheServiceChannel.MessageFactory.CreateMessage(EnsureCacheRequest.TYPE_ID);
            ensureCacheRequest.CacheName = CacheNameTemp;

            String response = (String) cacheServiceChannel.Request(ensureCacheRequest);
            Uri uri = new Uri(response);
            Channel namedCacheChannel = (Channel) conn.AcceptChannel(uri, null, null);

            Assert.IsTrue(namedCacheChannel.IsOpen);
            DestroyCacheRequest destroyCacheRequest =
                    (DestroyCacheRequest)cacheServiceChannel.MessageFactory.CreateMessage(DestroyCacheRequest.TYPE_ID);

            destroyCacheRequest.CacheName = CacheNameTemp;

            IResponse destroyCacheResponse = cacheServiceChannel.Send(destroyCacheRequest).WaitForResponse(-1);
            Assert.IsInstanceOf(typeof(DestroyCacheRequest), destroyCacheRequest);
            Assert.AreEqual(DestroyCacheRequest.TYPE_ID, destroyCacheRequest.TypeId);
            Assert.IsInstanceOf(typeof(CacheServiceResponse), destroyCacheResponse);
            Assert.IsFalse(destroyCacheResponse.IsFailure);
            Assert.AreEqual(destroyCacheResponse.RequestId, destroyCacheRequest.Id);
            Assert.AreEqual(0, destroyCacheResponse.TypeId);
            Assert.IsTrue((bool) destroyCacheResponse.Result);

            PutRequest putRequest =
                    (PutRequest) namedCacheChannel.MessageFactory.CreateMessage(PutRequest.TYPE_ID);
            putRequest.Key = "ivan";
            putRequest.Value = "3";
            putRequest.IsReturnRequired = true;

            try
            {
                namedCacheChannel.Send(putRequest).WaitForResponse(-1);
            }
            catch (PortableException)
            {
            }
        }

        [Test]
        public void TestEnsureCacheRequestSerDeser()
        {
            IConnection conn = initiator.EnsureConnection();

            IChannel cacheServiceChannel = conn.OpenChannel(CacheServiceProtocol.Instance,
                                                            "CacheServiceProxy", null, null);
            EnsureCacheRequest ensureCacheRequest =
                    (EnsureCacheRequest)cacheServiceChannel.MessageFactory.CreateMessage(EnsureCacheRequest.TYPE_ID);

            ensureCacheRequest.CacheName = CacheName;

            Stream stream = new MemoryStream();
            Codec codec = new Codec();
            codec.Encode(cacheServiceChannel, ensureCacheRequest, new DataWriter(stream));
            stream.Position = 0;
            EnsureCacheRequest result = (EnsureCacheRequest) codec.Decode(cacheServiceChannel, new DataReader(stream));
            stream.Close();
            Assert.AreEqual(ensureCacheRequest.CacheName, result.CacheName);
        }
    }
}
