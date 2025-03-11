/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Xml;

using NUnit.Framework;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net;

namespace Tangosol.Run.Xml
{
    [TestFixture]
    public class SimpleDocumentTests
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
        public void TestConstructors()
        {
            SimpleDocument sd = new SimpleDocument("doc");
            Assert.IsNotNull(sd);
            Assert.AreEqual(sd.Name, "doc");

            sd = new SimpleDocument("doc", "test.dtd", null);
            Assert.IsNotNull(sd);
            Assert.IsNotNull(sd.Name);
            Assert.IsNotNull(sd.DtdUri);
            Assert.IsNull(sd.DtdName);
        }

        [Test]
        public void TestPreprocessProps()
        {
            const string cacheConfig = "<?xml version='1.0'?><cache-config xmlns='http://schemas.tangosol.com/cache'>" +
                    " <caching-scheme-mapping>" +
                    "  <cache-mapping>    <cache-name>local-*</cache-name>    <scheme-name>extend-direct</scheme-name>    </cache-mapping>" +
                    "  <cache-mapping>    <cache-name system-property=\"coherence.cacheName\">dist-*</cache-name>    <scheme-name>extend-direct</scheme-name>    </cache-mapping>" +
                    " </caching-scheme-mapping>" +
                    " <caching-schemes>" +
                    "  <remote-cache-scheme>    <scheme-name>extend-direct</scheme-name>    <service-name>ExtendTcpCacheService</service-name>" +
                    "   <initiator-config>" +
                    "    <tcp-initiator>" +
                    "     <local-address>" +
                    "      <address system-property=\"coherence.address\">127.0.0.1</address>    <port system-property=\"coherence.extend.port\">0</port>" +
                    "     </local-address>" +
                    "     <remote-addresses>    <address-provider>ap1</address-provider>    </remote-addresses>" +
                    "    </tcp-initiator>" +
                    "    <outgoing-message-handler>    <heartbeat-interval>1s</heartbeat-interval>" +
                    "     <heartbeat-timeout>10s</heartbeat-timeout>    <request-timeout>0s</request-timeout>" +
                    "    </outgoing-message-handler>" +
                    "    <use-filters>    <filter-name>gzip</filter-name>    </use-filters>" +
                    "    <connect-timeout>30s</connect-timeout>" +
                    "   </initiator-config>" +
                    "  </remote-cache-scheme>" +
                    "  <remote-invocation-scheme>    <scheme-name>invocation-scheme</scheme-name>    <service-name>RemoteInvocationService</service-name>" +
                    "   <initiator-config>" +
                    "    <tcp-initiator>" +
                    "     <remote-addresses>    <address-provider>ap1</address-provider>    </remote-addresses>" +
                    "    </tcp-initiator>" +
                    "    <outgoing-message-handler>    <heartbeat-interval>1s</heartbeat-interval>" +
                    "     <heartbeat-timeout>10s</heartbeat-timeout>    <request-timeout>30s</request-timeout>" +
                    "    </outgoing-message-handler>" +
                    "    <use-filters>    <filter-name>gzip</filter-name>    </use-filters>" +
                    "    <serializer>pof</serializer>    <connect-timeout>5s</connect-timeout>" +
                    "   </initiator-config>" +
                    "  </remote-invocation-scheme>" +
                    " </caching-schemes>" +
                    "</cache-config>";

            Environment.SetEnvironmentVariable("coherence.address", "localhost");
            Environment.SetEnvironmentVariable("coherence.extend.port", "9090");
            var doc = new XmlDocument();
            doc.LoadXml(cacheConfig);
            IXmlDocument coherenceDoc = XmlHelper.ConvertDocument(doc);
            coherenceDoc.IterateThroughAllNodes(CacheFactory.PreprocessProp);
            String result = coherenceDoc.ToString();
            Assert.IsTrue(result.Contains("localhost"));
            Assert.IsTrue(result.Contains("9090"));
        }

        [Test]
        public void TestPreprocessParams()
        {
            Environment.SetEnvironmentVariable("coherence.profile", "client");

            DefaultConfigurableCacheFactory factory
                = new DefaultConfigurableCacheFactory("assembly://Coherence/Tangosol.Config/coherence-cache-config.xml");
            IXmlDocument config = (IXmlDocument) factory.Config;
            config.IterateThroughAllNodes(CacheFactory.PreprocessProp);
            Assert.IsTrue(config.ToString().Contains("client-remote"));
        }

        [Test]
        public void TestProperties()
        {
            SimpleDocument sd = new SimpleDocument();
            Exception e = null;

            sd.Name = "doc";
            Assert.IsNotNull(sd.Name);

            sd.DtdUri = "test.dtd";
            Assert.IsNotNull(sd.DtdUri);

            sd.DtdName = null;
            Assert.IsNull(sd.DtdName);
            sd.DtdName = "";
            Assert.IsNull(sd.DtdName);
            try
            {
                sd.DtdName = "\"test";
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
            sd.DtdName = "public name";
            Assert.IsNotNull(sd.DtdName);

            sd.Encoding = "";
            Assert.IsNull(sd.Encoding);
            sd.Encoding = "UTF-8";
            Assert.IsNotNull(sd.Encoding);
            e = null;
            try
            {
                sd.Encoding = "?test";
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);

            sd.DocumentComment = "";
            Assert.IsNull(sd.DocumentComment);
            sd.DocumentComment = "comment";
            Assert.IsNotNull(sd.DocumentComment);
            e = null;
            try
            {
                sd.DocumentComment = "c-->";
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);

            sd.IsMutable = false;
            e = null;
            try
            {
                sd.DocumentComment = "comm";
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(InvalidOperationException), e);
        }

        [Test]
        public void TestObjectMethods()
        {
            SimpleDocument sd1 = new SimpleDocument("doc1");
            SimpleDocument sd2 = new SimpleDocument("doc2");
            Assert.IsFalse(sd1.Equals(sd2));
            sd1.Name = "doc2";
            Assert.IsTrue(sd1.Equals(sd2));
            sd1.DocumentComment = "comment";
            Assert.IsFalse(sd1.Equals(sd2));
            sd2.DocumentComment = sd1.DocumentComment;
            Assert.IsTrue(sd1.Equals(sd2));
            sd1.DtdUri = "test.dtd";
            Assert.IsFalse(sd1.Equals(sd2));
            Assert.AreNotEqual(sd1.GetHashCode(), sd2.GetHashCode());
            sd2.DtdUri = sd1.DtdUri;
            Assert.IsTrue(sd1.Equals(sd2));
            sd1.DtdName = "public name";
            Assert.IsFalse(sd1.Equals(sd2));
            sd2.DtdName = sd1.DtdName;
            Assert.IsTrue(sd1.Equals(sd2));
            sd1.Encoding = "UTF-8";
            Assert.IsFalse(sd1.Equals(sd2));
            sd2.Encoding = sd1.Encoding;
            Assert.IsTrue(sd1.Equals(sd2));
            Assert.AreEqual(sd1.GetHashCode(), sd2.GetHashCode());

            SimpleElement se = new SimpleElement();
            Assert.IsFalse(sd1.Equals(se));
        }

        [Test]
        public void TestSerialization()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.IsNotNull(ctx);

            SimpleDocument sd = new SimpleDocument();
            sd.Name = "doc";
            sd.DtdUri = "test.dtd";
            sd.DtdName = "public name";
            sd.Encoding = "UTF-8";
            sd.DocumentComment = "comment";

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), sd);

            stream.Position = 0;
            SimpleDocument sdd = (SimpleDocument) ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(sd, sdd);
        }

        [Test]
        public void TestWriteXml()
        {
            SimpleDocument sd = new SimpleDocument("root");
            sd.Encoding = "UTF-8";
            sd.DocumentComment = "document comment";

            TextWriter writer = new StringWriter();
            sd.WriteXml(writer, true);
            Assert.AreEqual(sd.ToString(), writer.ToString());
            Assert.IsTrue(writer.ToString().IndexOf("\n") >= 0);
            writer = new StringWriter();
            sd.WriteXml(writer, false);
            Assert.IsFalse(writer.ToString().IndexOf("\n") >= 0);

            sd.DtdUri = "dtd.uri";
            writer = new StringWriter();
            sd.WriteXml(writer, false);
            string result = writer.ToString();
            Assert.IsFalse(result.IndexOf("\n") >= 0);
            Assert.IsTrue(result.IndexOf("SYSTEM") >= 0);

            sd.DtdName = "dtdname";
            writer = new StringWriter();
            sd.WriteXml(writer, true);
            result = writer.ToString();
            Assert.IsTrue(result.IndexOf("\n") >= 0);
            Assert.IsTrue(result.IndexOf("PUBLIC") >= 0);
        }
    }
}
