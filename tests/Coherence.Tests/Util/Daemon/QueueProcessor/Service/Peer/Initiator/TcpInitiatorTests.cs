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

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator
{
    [TestFixture]
    public class TcpInitiatorTests
    {
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
        public void TestStartAndStopInitiator()
        {
            TcpInitiator initiator = GetInitiator();
            initiator.Start();
            Assert.AreEqual(initiator.IsRunning, true);
            initiator.Stop();
            Assert.AreEqual(initiator.IsRunning, false);
        }

        [Test]
        public void TestEnsureAndCloseConnection()
        {
            IConnection conn;
            TcpInitiator initiator = GetInitiator();
            initiator.Start();
            conn = initiator.EnsureConnection();
            Assert.IsNotNull(conn);
            Assert.AreEqual(conn.IsOpen, true);
            conn.Close();
            Assert.AreEqual(conn.IsOpen, false);
            initiator.Stop();
        }

        [Test]
        public void TestEnsureAndCloseConnectionSocket()
        {
            TcpConnection conn;
            TcpInitiator initiator = GetInitiator();
            initiator.Start();
            conn = (TcpConnection) initiator.EnsureConnection();
            Assert.IsNotNull(conn);
            Assert.AreEqual(conn.IsOpen, true);
            conn.Client.Close();
            Blocking.Sleep(2000);
            Assert.AreEqual(conn.IsOpen, false);
            initiator.Stop();
        }

        [Test]
        public void TestWithAddressProvider()
        {
            var initiator = new TcpInitiator
                {
                    OperationalContext = new DefaultOperationalContext()
                };
            Stream stream                 = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-cache-config.xml");
            IXmlDocument xmlConfig        = XmlHelper.LoadXml(stream);
            IXmlElement remoteAddressNode = xmlConfig.FindElement(
                "caching-schemes/remote-cache-scheme/initiator-config/tcp-initiator/remote-addresses");
            if (null != remoteAddressNode)
            {
                IXmlElement socketAddressNode = remoteAddressNode.GetElement("socket-address");
                if (null != socketAddressNode)
                {
                    XmlHelper.RemoveElement(remoteAddressNode, "socket-address");
                }

                remoteAddressNode.AddElement("address-provider").AddElement("class-name").SetString(
                    "Tangosol.Net.ConfigurableAddressProviderTests+LoopbackAddressProvider, Coherence.Tests");
            }

            IXmlElement initConfig = xmlConfig.FindElement("caching-schemes/remote-cache-scheme/initiator-config");

            initiator.Configure(initConfig);
            initiator.RegisterProtocol(CacheServiceProtocol.Instance);
            initiator.RegisterProtocol(NamedCacheProtocol.Instance);
            initiator.Start();
            Assert.AreEqual(initiator.IsRunning, true);

            IConnection conn = initiator.EnsureConnection();
            Assert.IsNotNull(conn);
            Assert.AreEqual(conn.IsOpen, true);
            conn.Close();
            initiator.Stop();
        }

        [Test]
        public void TestConnectionTimeout()
        {
            var initiator = new TcpInitiator
                {
                    OperationalContext = new DefaultOperationalContext()
                };
            Stream       stream    = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-timeout-cache-config.xml");
            IXmlDocument xmlConfig = XmlHelper.LoadXml(stream);

            IXmlElement initConfig = xmlConfig.FindElement("caching-schemes/remote-cache-scheme/initiator-config");

            initiator.Configure(initConfig);
            initiator.RegisterProtocol(CacheServiceProtocol.Instance);
            initiator.RegisterProtocol(NamedCacheProtocol.Instance);
            initiator.Start();
            Assert.AreEqual(initiator.IsRunning, true);

            IConnection conn = null;
            bool        exceptionCaught = false;
            try {
               conn = initiator.EnsureConnection();
            }
            catch (Tangosol.Net.Messaging.ConnectionException)
            {
               // good, should time out
               exceptionCaught = true;
            }
            Assert.IsNull(conn);
            Assert.IsTrue(exceptionCaught);
            initiator.Stop();
        }

        [Test]
        public void TestTcpConnectionRemoteHostAddress()
        {
            TcpInitiator initiator = GetInitiator();
            IAddressProvider addressProvider = initiator.RemoteAddressProvider;
            Assert.IsTrue(addressProvider is ConfigurableAddressProvider);

            TcpConnection conn = (TcpConnection) initiator.EnsureConnection();
            Assert.IsNotNull(conn);
            Assert.AreEqual(conn.IsOpen, true);
            Assert.True(addressProvider is ConfigurableAddressProvider);
            Assert.AreEqual(((ConfigurableAddressProvider) addressProvider).RemoteHostAddress, "127.0.0.1");
            conn.Close();
            Assert.AreEqual(conn.IsOpen, false);

            initiator.Stop();
            Assert.AreEqual(initiator.IsRunning, false);
        }

        [Test]
        public void TestConnectTimeout()
        {
            // case 1 - both Connect and Request timeouts configured, Connect timeout should be used
            TcpInitiator initiator = LoadConfig("Tangosol.Resources.s4hc-timeout-cache-config.xml");
            Assert.AreNotEqual(initiator.ConnectTimeout, initiator.RequestTimeout);

            // case 2 - only Connect timeouts configured, Connect timeout should be used
            initiator = LoadConfig("Tangosol.Resources.s4hc-timeout-cache-config2.xml");
            Assert.AreNotEqual(initiator.ConnectTimeout, initiator.RequestTimeout);

            // case 3 - only Request timeouts configured, Connect timeout should be same as Request timeout
            initiator = LoadConfig("Tangosol.Resources.s4hc-timeout-cache-config3.xml");
            Assert.AreEqual(initiator.ConnectTimeout, initiator.RequestTimeout);

            // case 4 - no Connect or Request timeouts configured, Connect timeout should be same as Request timeout
            initiator = LoadConfig("Tangosol.Resources.s4hc-timeout-cache-config4.xml");
            Assert.AreEqual(initiator.ConnectTimeout, initiator.RequestTimeout);
        }

        private TcpInitiator LoadConfig(String file)
        {
            var initiator = new TcpInitiator
                {
                    OperationalContext = new DefaultOperationalContext()
                };
            Stream stream           = GetType().Assembly.GetManifestResourceStream(file);
            IXmlDocument xmlConfig  = XmlHelper.LoadXml(stream);

            IXmlElement initConfig = xmlConfig.FindElement("caching-schemes/remote-cache-scheme/initiator-config");

            initiator.Configure(initConfig);
            return initiator;
        }
    }
}
