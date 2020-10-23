/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;

using NUnit.Framework;

using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.Net
{
    [TestFixture]
    public class ConfigurableAddressProviderTests
    {
        [Test]
        public void TestWithInvalidConfiguration()
        {
            string xml = "<remote-addresses xmlns=\"http://schemas.tangosol.com/cache\"><socket-address><address>neka</address><port>-80</port></socket-address></remote-addresses>";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            IXmlDocument config = XmlHelper.ConvertDocument(xmlDoc);
            Assert.IsNotNull(config);

            Assert.That(() => new ConfigurableAddressProvider(config), Throws.Exception);
        }

        [Test]
        public void TestWithEmptyConfiguration()
        {
            IXmlDocument config = new SimpleDocument();
            Assert.IsNotNull(config);

            ConfigurableAddressProvider cap = new ConfigurableAddressProvider(config);
            Assert.IsNotNull(cap);
            Assert.AreEqual(cap.ToString(), "[]");
            Assert.IsNull(cap.NextAddress);
        }

        [Test]
        public void TestWithValidConfiguration()
        {
            string xml = "<remote-addresses xmlns=\"http://schemas.tangosol.com/cache\"><socket-address><address>10.0.0.120</address><port>80</port></socket-address><socket-address><address>10.0.0.121</address><port>8080</port></socket-address></remote-addresses>";
            IXmlDocument config = XmlHelper.LoadXml(new StringReader(xml));
            Assert.IsNotNull(config);

            ConfigurableAddressProvider cap = new ConfigurableAddressProvider(config);
            Assert.IsNotNull(cap);
            string capString = cap.ToString();
            Assert.IsTrue(capString.Equals("[10.0.0.121:8080,10.0.0.120:80]") ||
                capString.Equals("[10.0.0.120:80,10.0.0.121:8080]"));

            // without accepting, addresses will be exhausted
            IPEndPoint addr = cap.NextAddress;
            Assert.IsNotNull(addr);
            addr = cap.NextAddress;
            Assert.IsNotNull(addr);
            addr = cap.NextAddress;
            Assert.IsNull(addr);

            addr = cap.NextAddress;
            Assert.IsNotNull(addr);
            cap.Accept();
            if (addr.Port == 80)
            {
                Assert.AreEqual(addr.Address.ToString(), "10.0.0.120");
            }
            else
            {
                Assert.AreEqual(addr.Address.ToString(), "10.0.0.121");
            }

            addr = cap.NextAddress;
            Assert.IsNotNull(addr);
            cap.Reject(null);
        }

        [Test]
        public void TestWithValidHostname()
        {
            string hostname = "time.apple.com";  // time.apple.com returns multiple addresses (hopefully)
            string xml      = "<remote-addresses xmlns=\"http://schemas.tangosol.com/cache\"><socket-address><address>"
                + hostname + "</address><port>80</port></socket-address></remote-addresses>";
            IXmlDocument config = XmlHelper.LoadXml(new StringReader(xml));
            Assert.IsNotNull(config);

            ConfigurableAddressProvider cap = new ConfigurableAddressProvider(config);
            Assert.IsNotNull(cap);
            Assert.AreEqual(cap.ToString(), "[time.apple.com:80]");

            int addressCount = 0;
            for (IPEndPoint endpoint = cap.NextAddress; endpoint != null; endpoint = cap.NextAddress)
            {
                ++addressCount;
                Assert.NotNull(endpoint);
            }
            System.Console.WriteLine("\nConfigurableAddressProvider returned " + addressCount
                + " address(es) for host \"" + hostname + "\".");
            Assert.Greater(addressCount, 0);
        }

        [Test]
        public void TestWithInvalidHostname()
        {
            string xml = "<remote-addresses xmlns=\"http://schemas.tangosol.com/cache\"><socket-address><address>nonexistenthost.never.ever</address><port>80</port></socket-address></remote-addresses>";
            IXmlDocument config = XmlHelper.LoadXml(new StringReader(xml));
            Assert.IsNotNull(config);

            // safe
            ConfigurableAddressProvider cap = new ConfigurableAddressProvider(config);
            Assert.IsNotNull(cap);
            Assert.AreEqual(cap.ToString(), "[nonexistenthost.never.ever:80]");

            IPEndPoint addr = cap.NextAddress;
            Assert.IsNull(addr);

            // not safe - throw exception on non-resolvable addresses
            cap = new ConfigurableAddressProvider(config, false);
            Assert.IsNotNull(cap);
            Assert.AreEqual(cap.ToString(), "[nonexistenthost.never.ever:80]");

            try
            {
                addr = cap.NextAddress;
                Assert.Fail();
            }
            catch (Exception)
            {
            }
        }

        [Test]
        public void TestCreateAddressProvider1()
        {
            string xml = "<remote-addresses xmlns=\"http://schemas.tangosol.com/cache\"><socket-address><address>10.0.0.120</address><port>80</port></socket-address><socket-address><address>10.0.0.121</address><port>8080</port></socket-address></remote-addresses>";
            IXmlDocument config = XmlHelper.LoadXml(new StringReader(xml));
            Assert.IsNotNull(config);

            ConfigurableAddressProviderFactory factory = new ConfigurableAddressProviderFactory();
            factory.Config = config;
            IAddressProvider addrProvider = factory.CreateAddressProvider();
            Assert.IsNotNull(addrProvider);

            string addrProviderString = addrProvider.ToString();
            Assert.IsTrue(addrProviderString.Equals("[10.0.0.121:8080,10.0.0.120:80]") ||
                    addrProviderString.Equals("[10.0.0.120:80,10.0.0.121:8080]"));

            Assert.IsInstanceOf(typeof (ConfigurableAddressProvider), addrProvider);
        }

        [Test]
        public void TestCreateAddressProvider2()
        {
            string xml = "<remote-addresses xmlns=\"http://schemas.tangosol.com/cache\"><address-provider><class-name>Tangosol.Net.ConfigurableAddressProviderTests+LoopbackAddressProvider, Coherence.Core.Tests</class-name></address-provider></remote-addresses>";
            IXmlDocument config = XmlHelper.LoadXml(new StringReader(xml));
            Assert.IsNotNull(config);

            ConfigurableAddressProviderFactory factory = new ConfigurableAddressProviderFactory();
            factory.Config = config.GetElement("address-provider");
            IAddressProvider addrProvider = factory.CreateAddressProvider();
            Assert.IsNotNull(addrProvider);
            Assert.IsInstanceOf(typeof(LoopbackAddressProvider), addrProvider);
        }

        [Test]
        public void TestAcceptLast()
        {
            string xml = "<remote-addresses xmlns=\"http://schemas.tangosol.com/cache\"><socket-address><address>127.0.0.1</address><port>80</port></socket-address><socket-address><address>127.0.0.1</address><port>81</port></socket-address></remote-addresses>";
            IXmlDocument config = XmlHelper.LoadXml(new StringReader(xml));
            Assert.IsNotNull(config);

            ConfigurableAddressProviderFactory factory = new ConfigurableAddressProviderFactory();
            factory.Config = config;
            IAddressProvider addrProvider = factory.CreateAddressProvider();
            Assert.IsNotNull(addrProvider);
            Assert.IsInstanceOf(typeof(ConfigurableAddressProvider), addrProvider);

            string addrProviderString = addrProvider.ToString();
            Assert.IsTrue(addrProviderString.Equals("[127.0.0.1:81,127.0.0.1:80]") ||
                    addrProviderString.Equals("[127.0.0.1:80,127.0.0.1:81]"));

            IPEndPoint addr = addrProvider.NextAddress;
            Assert.IsNotNull(addr);
            addrProvider.Reject(null);
            addr = addrProvider.NextAddress;
            Assert.IsNotNull(addr);
            addrProvider.Accept();
            addr = addrProvider.NextAddress;
            Assert.IsNotNull(addr);
        }

        public class LoopbackAddressProvider : IAddressProvider
        {
            public IPEndPoint NextAddress
            {
                get { return new IPEndPoint(IPAddress.Loopback, 9099); }
            }

            public void Accept()
            {}

            public void Reject(Exception eCause)
            {}
        }
    }
}
