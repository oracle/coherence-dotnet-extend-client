/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.IO;
using System.Security.Principal;

using NUnit.Framework;

using Tangosol.IO;
using Tangosol.Net.Security;
using Tangosol.Net.Security.Impl;
using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.Net
{
    public class TestIdentityAsserter : IIdentityAsserter
    {
        public IPrincipal AssertIdentity(object oToken, IService service)
        {
            throw new System.NotImplementedException();
        }
    }

    public class TestIdentityTransformer : IIdentityTransformer
    {
        public object TransformIdentity(IPrincipal principal, IService service)
        {
            throw new System.NotImplementedException();
        }
    }

    public class TestNetworkFilter : IWrapperStreamFactory, IXmlConfigurable
    {
        public Stream GetInputStream(Stream stream)
        {
            throw new System.NotImplementedException();
        }

        public Stream GetOutputStream(Stream stream)
        {
            throw new System.NotImplementedException();
        }

        public IXmlElement Config
        {
            get { return null; }
            set {}
        }
    }

    [TestFixture]
    public class DefaultOperationalContextTest
    {
        private static void VerifyDefaultConfig(IOperationalContext opCtx)
        {
            Assert.IsTrue(opCtx.EditionName == DefaultOperationalContext.DEFAULT_EDITION_NAME);
            Assert.IsTrue(opCtx.FilterMap.Count == 2);
            Assert.IsInstanceOf(typeof(CompressionFilter), opCtx.FilterMap["gzip"]);
            Assert.IsInstanceOf(typeof(DefaultIdentityAsserter), opCtx.IdentityAsserter);
            Assert.IsInstanceOf(typeof(DefaultIdentityTransformer), opCtx.IdentityTransformer);
            Assert.IsNotNull(opCtx.LocalMember, "opCtx.LocalMember");
            Assert.IsNotNull(opCtx.LocalMember.MachineName, "opCtx.LocalMember.MachineName");
            Assert.IsTrue(StringUtils.IsNullOrEmpty(opCtx.LocalMember.MemberName), "StringUtils.IsNullOrEmpty(opCtx.LocalMember.MemberName");
            Assert.IsNotNull(opCtx.LocalMember.ProcessName, "opCtx.LocalMember.ProcessName");
            Assert.IsTrue(StringUtils.IsNullOrEmpty(opCtx.LocalMember.RackName), "StringUtils.IsNullOrEmpty(opCtx.LocalMember.RackName)");
            Assert.IsNotNull(opCtx.LocalMember.RoleName, "opCtx.LocalMember.RoleName");
            Assert.IsTrue(opCtx.IsPrincipalScopingEnabled, "opCtx.IsPrincipalScopingEnabled");
        }

        private static void VerifyCustomConfig(IOperationalContext opCtx)
        {
            Assert.IsTrue(opCtx.FilterMap.Count == 5);
            Assert.IsNotNull(opCtx.FilterMap["foo"]);
            Assert.IsNotNull(opCtx.FilterMap["bar"]);
            Assert.IsNotNull(opCtx.FilterMap["baz"]);
            Assert.IsInstanceOf(typeof(CompressionFilter), opCtx.FilterMap["gzip"]);
            Assert.IsInstanceOf(typeof(ConfigurableSerializerFactory), 
                    opCtx.SerializerMap["pof"]);
            Assert.IsInstanceOf(typeof(ConfigurableAddressProviderFactory), 
                    opCtx.AddressProviderMap["ap1"]);
            Assert.IsInstanceOf(typeof(TestIdentityAsserter), opCtx.IdentityAsserter);
            Assert.IsInstanceOf(typeof(TestIdentityTransformer), opCtx.IdentityTransformer);
            Assert.IsNotNull(opCtx.LocalMember);
            Assert.AreEqual(opCtx.LocalMember.MachineName, "test-machine");
            Assert.AreEqual(opCtx.LocalMember.MemberName, "test-member");
            Assert.AreEqual(opCtx.LocalMember.ProcessName, "test-process");
            Assert.AreEqual(opCtx.LocalMember.RackName, "test-rack");
            Assert.AreEqual(opCtx.LocalMember.RoleName, "test-role");
            Assert.AreEqual(opCtx.LocalMember.SiteName, "test-site");
            Assert.IsTrue(opCtx.IsPrincipalScopingEnabled);
            Assert.AreEqual(opCtx.LogLevel, 6);
        }

        [Test]
        public void TestDefaultConfigurationThroughCacheFactory()
        {
            VerifyDefaultConfig(((DefaultConfigurableCacheFactory) CacheFactory
                    .ConfigurableCacheFactory).OperationalContext);
            CacheFactory.Shutdown();
        }

        [Test]
        public void TestCustomConfiguration()
        {
            VerifyCustomConfig(new DefaultOperationalContext(LoadCustomConfig()));
            CacheFactory.Shutdown();
        }

        private static IXmlElement LoadCustomConfig()
        {
            string ss = "<?xml version=\"1.0\"?><coherence xmlns=\"http://schemas.tangosol.com/coherence\">" +
               "  <cluster-config>" +
               "    <member-identity>" +
               "      <site-name>test-site</site-name>" +
               "      <rack-name>test-rack</rack-name>" +
               "      <machine-name>test-machine</machine-name>" +
               "      <process-name>test-process</process-name>" +
               "      <member-name>test-member</member-name>" +
               "      <role-name>test-role</role-name>" +
               "    </member-identity>" +
               "    <filters>" +
               "      <filter>" +
               "        <filter-name>foo</filter-name>" +
               "        <filter-class>Tangosol.Net.CompressionFilter, Coherence</filter-class>" +
               "      </filter>" +
               "      <filter>" +
               "        <filter-name>bar</filter-name>" +
               "        <filter-class>Tangosol.Net.CompressionFilter, Coherence</filter-class>" +
               "      </filter>" +
               "      <filter>" +
               "        <filter-name>baz</filter-name>" +
               "        <filter-class>Tangosol.Net.CompressionFilter, Coherence</filter-class>" +
               "      </filter>" +
               "      <filter>" +
 	 	 	   "        <filter-name>TestNetworkFilter</filter-name>" +
               "        <filter-class>Tangosol.Net.TestNetworkFilter, Coherence.Tests</filter-class>" +
               "        <init-params>" +
               "          <init-param>" +
               "            <param-name>dummy1</param-name>" +
               "            <param-value>value1</param-value>" +
               "          </init-param>" +
               "          <init-param>" +
               "            <param-name>dummy2</param-name>" +
               "            <param-value>value2</param-value>" +
               "          </init-param>" +
               "        </init-params>" +
               "      </filter>" +
               "    </filters>" +
               "    <serializers>" +
               "      <serializer id=\"pof\">" +
               "        <instance>" +
               "          <class-name>Tangosol.IO.Pof.ConfigurablePofContext, Coherence</class-name>" +
               "          <init-params>" +
               "            <init-param>" +
               "              <param-type>string</param-type>" +
               "              <param-value>custom-types-pof-config.xml</param-value>" +
               "            </init-param>" +
               "          </init-params>" +
               "        </instance>" +
               "      </serializer>" +
               "    </serializers>" +
               "    <address-providers>" +
               "      <address-provider id=\"ap1\">" +
               "        <instance>" +
               "          <class-name>Tangosol.Net.ConfigurableAddressProviderTests+LoopbackAddressProvider, Coherence.Tests</class-name>" +
               "        </instance>" +
               "      </address-provider>" +
               "    </address-providers>" +
               "  </cluster-config>" +
               "  <logging-config>" +
               "    <destination>stderr</destination>" +
               "    <severity-level>6</severity-level>" +
               "    <message-format/>" +
               "    <character-limit>1024</character-limit>" +
               "  </logging-config>" +
               "  <license-config>" +
               "    <edition-name>DC</edition-name>" +
               "  </license-config>" +
               "  <security-config>" +
               "    <identity-asserter>" +
               "      <class-name>Tangosol.Net.TestIdentityAsserter, Coherence.Tests</class-name>" +
               "    </identity-asserter>" +
               "    <identity-transformer>" +
               "      <class-name>Tangosol.Net.TestIdentityTransformer, Coherence.Tests</class-name>" +
               "    </identity-transformer>" +
               "    <principal-scope>true</principal-scope>" +
               "  </security-config>" +
               "</coherence>";
            return XmlHelper.LoadXml(new StringReader(ss));
        }
    }
}


