/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Specialized;
using System.IO;

using NUnit.Framework;

using Tangosol.Net;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;
using Tangosol.Net.Messaging.Impl.CacheService;
using Tangosol.Net.Messaging.Impl.NamedCache;
using Tangosol.Run.Xml;
using Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    [TestFixture]
    public class MessagingTests
    {
        private NameValueCollection appSettings = TestUtils.AppSettings;
        private IConnectionInitiator connectionInitiator;
        private IConnection connection;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
        }

        [SetUp]
        public void SetUp()
        {
            connectionInitiator = GetInitiator();
            connection = connectionInitiator.EnsureConnection();
        }

        [TearDown]
        public void TearDown()
        {
            connection.Close();
            connectionInitiator.Stop();
        }

        private TcpInitiator GetInitiator()
        {
            var initiator = new TcpInitiator
                {
                    OperationalContext = new DefaultOperationalContext()
                };

            Stream stream          = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-cache-config.xml");
            IXmlDocument xmlConfig = XmlHelper.LoadXml(stream);
            IXmlElement initConfig =
                xmlConfig.FindElement("caching-schemes/remote-cache-scheme/initiator-config");

            initiator.Configure(initConfig);
            initiator.RegisterProtocol(CacheServiceProtocol.Instance);
            initiator.RegisterProtocol(NamedCacheProtocol.Instance);
            initiator.Start();
            return initiator;
        }

        [Test]
        public void TestMessagingFactory()
        {
            IChannel channel0 = connection.GetChannel(0);

            IMessageFactory factory = channel0.MessageFactory;
            Assert.IsNotNull(factory);
            Assert.IsInstanceOf(typeof(Initiator.MessagingFactory), factory);
            Initiator.MessagingFactory initiatorFactory = (Initiator.MessagingFactory) factory;
            Assert.AreEqual(initiatorFactory.Version, 3);
        }

        [Test]
        public void TestMessagingProtocol()
        {
            IChannel channel0 = connection.GetChannel(0);
            IMessageFactory factory = channel0.MessageFactory;

            IProtocol protocol = factory.Protocol;
            Assert.IsNotNull(protocol);
            Assert.IsInstanceOf(typeof (Initiator.MessagingProtocol), protocol);
            Assert.AreEqual(protocol.CurrentVersion, 3);
            Assert.AreEqual(protocol.SupportedVersion, 2);
            Assert.AreEqual(protocol.Name, Initiator.MessagingProtocol.PROTOCOL_NAME);
            IMessageFactory factory2 = protocol.GetMessageFactory(3);
            Assert.AreEqual(factory, factory2);
        }

        [Test]
        public void TestOpenConnection()
        {
            IChannel channel0 = connection.GetChannel(0);

            IMessage message = channel0.MessageFactory.CreateMessage(Initiator.OpenConnection.TYPE_ID);
            Assert.IsInstanceOf(typeof(Initiator.OpenConnection), message);
            Assert.AreEqual(Initiator.OpenConnection.TYPE_ID, message.TypeId);

            message = channel0.MessageFactory.CreateMessage(Initiator.OpenConnectionRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(Initiator.OpenConnectionRequest), message);
            Assert.AreEqual(Initiator.OpenConnectionRequest.TYPE_ID, message.TypeId);

            message = channel0.MessageFactory.CreateMessage(Initiator.OpenConnectionResponse.TYPE_ID);
            Assert.IsInstanceOf(typeof(Initiator.OpenConnectionResponse), message);
            Assert.AreEqual(Initiator.OpenConnectionResponse.TYPE_ID, message.TypeId);

            Assert.IsNotNull(connection);
            Assert.IsNotEmpty(connection.GetChannels());
            Assert.IsTrue(connection.GetChannel(0).IsOpen);
        }

        [Test]
        public void TestPingRequest()
        {
            IChannel channel0 = connection.GetChannel(0);

            IRequest pingRequest = (IRequest) channel0.MessageFactory.CreateMessage(PingRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(PingRequest), pingRequest);
            Assert.AreEqual(PingRequest.TYPE_ID, pingRequest.TypeId);

            IResponse pingResponse = connection.GetChannel(0).Send(pingRequest).WaitForResponse(-1);

            Assert.IsInstanceOf(typeof(PingRequest), pingRequest);
            Assert.AreEqual(PingRequest.TYPE_ID, pingRequest.TypeId);
            Assert.IsInstanceOf(typeof(PingResponse), pingResponse);
            Assert.IsFalse(pingResponse.IsFailure);
            Assert.IsNotNull(pingResponse.Result);
            Assert.AreEqual(pingResponse.RequestId, pingRequest.Id);
        }

        [Test]
        public void TestOpenChannel()
        {
            IChannel channel0 = connection.GetChannel(0);

            IMessage message = channel0.MessageFactory.CreateMessage(OpenChannelRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(OpenChannelRequest), message);
            Assert.AreEqual(OpenChannelRequest.TYPE_ID, message.TypeId);

            message = channel0.MessageFactory.CreateMessage(OpenChannelResponse.TYPE_ID);
            Assert.IsInstanceOf(typeof(OpenChannelResponse), message);
            Assert.AreEqual(OpenChannelResponse.TYPE_ID, message.TypeId);

            OpenChannel openChannel = (OpenChannel) channel0.MessageFactory.CreateMessage(OpenChannel.TYPE_ID);
            Assert.IsInstanceOf(typeof(OpenChannel), openChannel);
            Assert.AreEqual(OpenChannel.TYPE_ID, openChannel.TypeId);

            openChannel.Connection = (Connection) connection;
            openChannel.Protocol = CacheServiceProtocol.Instance;
            openChannel.ReceiverName = "CacheServiceProxy";

            IResponse openChannelResponse = connection.GetChannel(0).Send(openChannel).WaitForResponse(-1);
            Assert.IsInstanceOf(typeof(InternalResponse), openChannelResponse);

            Assert.IsFalse(openChannelResponse.IsFailure);
            Assert.IsNotNull(openChannelResponse.Result);
            Assert.AreEqual(openChannelResponse.RequestId, openChannel.Id);

            Assert.IsInstanceOf(typeof(Request.RequestStatus), openChannelResponse.Result);
            Request.RequestStatus status = (Request.RequestStatus) openChannelResponse.Result;
            Assert.IsNotNull(status);
            Assert.IsNull(status.Exception);
            Assert.IsInstanceOf(typeof(OpenChannelRequest), status.Request);
        }

        [Test]
        public void TestAcceptChannel()
        {
            IChannel cacheServiceChannel = connection.OpenChannel(CacheServiceProtocol.Instance,
                                                                  "CacheServiceProxy", null, null);
            EnsureCacheRequest ensureCacheRequest =
                (EnsureCacheRequest) cacheServiceChannel.MessageFactory.CreateMessage(
                                         EnsureCacheRequest.TYPE_ID);
            ensureCacheRequest.CacheName = CacheName;

            string response = (string) cacheServiceChannel.Request(ensureCacheRequest);
            Uri uri = new Uri(response);
            Assert.IsNotNull(uri);

            int id;
            id = Int32.Parse(UriUtils.GetSchemeSpecificPart(uri));
            Assert.IsTrue(id > 0);
            Assert.IsTrue(connection.GetChannel(id) == null);

            AcceptChannel acceptChannel =
                (AcceptChannel) connection.GetChannel(0).MessageFactory.CreateMessage(
                                    AcceptChannel.TYPE_ID);
            Assert.IsInstanceOf(typeof(AcceptChannel), acceptChannel);
            Assert.AreEqual(AcceptChannel.TYPE_ID, acceptChannel.TypeId);

            acceptChannel.ChannelUri = uri;
            acceptChannel.Connection = (Connection) connection;

            IResponse acceptChannelResponse = connection.GetChannel(0).Send(acceptChannel).WaitForResponse(-1);

            Assert.IsInstanceOf(typeof(InternalResponse), acceptChannelResponse);
            Request.RequestStatus status = (Request.RequestStatus) acceptChannelResponse.Result;
            Assert.IsNotNull(status);
            Assert.IsNull(status.Exception);
            Assert.IsInstanceOf(typeof(AcceptChannelRequest), status.Request);
        }

        [Test]
        public void TestCloseChannel()
        {
            IChannel cacheServiceChannel = connection.OpenChannel(CacheServiceProtocol.Instance,
                                                                  "CacheServiceProxy", null, null);

            EnsureCacheRequest ensureCacheRequest =
                (EnsureCacheRequest) cacheServiceChannel.MessageFactory.CreateMessage(EnsureCacheRequest.TYPE_ID);
            ensureCacheRequest.CacheName = CacheName;

            string response = (string) cacheServiceChannel.Request(ensureCacheRequest);
            Uri uri = new Uri(response);
            Assert.IsNotNull(uri);

            IChannel namedCacheChannel = connection.AcceptChannel(uri, null, null);
            Assert.IsNotNull(namedCacheChannel);

            string channelUri = "channel:" + namedCacheChannel.Id + "#NamedCacheProtocol";
            Assert.AreEqual(channelUri, uri.ToString());

            CloseChannel channelClose = (CloseChannel) connection.GetChannel(0).MessageFactory.CreateMessage(
                                                           CloseChannel.TYPE_ID);
            channelClose.ChannelClose = (Channel) namedCacheChannel;
            connection.GetChannel(0).Send(channelClose);
        }

        [Test]
        public void TestCloseConnection()
        {
            IChannel cacheServiceChannel = connection.OpenChannel(CacheServiceProtocol.Instance,
                                                                  "CacheServiceProxy", null, null);

            EnsureCacheRequest ensureCacheRequest =
                (EnsureCacheRequest) cacheServiceChannel.MessageFactory.CreateMessage(EnsureCacheRequest.TYPE_ID);
            ensureCacheRequest.CacheName = CacheName;

            string response = (string) cacheServiceChannel.Request(ensureCacheRequest);
            Uri uri = new Uri(response);
            Assert.IsNotNull(uri);

            IChannel namedCacheChannel = connection.AcceptChannel(uri, null, null);
            Assert.IsNotNull(namedCacheChannel);

            string channelUri = "channel:" + namedCacheChannel.Id + "#NamedCacheProtocol";
            Assert.AreEqual(channelUri, uri.ToString());

            CloseConnection closeConnection = (CloseConnection)connection.GetChannel(0).MessageFactory.CreateMessage(
                                                                   CloseConnection.TYPE_ID);
            connection.GetChannel(0).Send(closeConnection);
        }
    }
}