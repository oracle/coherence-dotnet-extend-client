/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using NUnit.Framework;

using Tangosol.IO.Resources;
using Tangosol.Net;
using Tangosol.Run.Xml;
using Tangosol.Util.Filter;
using Tangosol.Util.Processor;

namespace Tangosol.Data
{
    [TestFixture]
    public class PofReferenceTests
    {
        /**
        * Test POF object with circular references.
        */
        [Test]
        public void PofCircularReference()
            {
            CacheFactory.DefaultPofConfigPath = "//Coherence.Tests/Tangosol.Resources/s4hc-test-reference-config.xml";
            CacheFactory.DefaultPofConfig     = XmlHelper.LoadResource(ResourceLoader.GetResource(
                "assembly://Coherence.Tests/Tangosol.Resources/s4hc-test-reference-config.xml"), "POF configuration");
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;
            ccf.Config = XmlHelper.LoadXml(
                "assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config-reference.xml");
            ICacheService service = (ICacheService) ccf.EnsureService("ExtendTcpCacheService");
            INamedCache   cache   = service.EnsureCache("dist-extend-reference");

            var joe  = new PortablePerson("Joe Smith", new DateTime(78, 4, 25));
            var jane = new PortablePerson("Jane Smith", new DateTime(80, 5, 22));
            joe.Spouse = jane;
            jane.Spouse = joe;

            cache.Add("Key1", joe);
            cache.Invoke("Key1", new ConditionalPut(new AlwaysFilter(), joe, false));

            CacheFactory.DefaultPofConfigPath = "//Coherence.Tests/Tangosol.Resources/s4hc-test-config.xml";
            CacheFactory.DefaultPofConfig     = XmlHelper.LoadResource(ResourceLoader.GetResource(
                "assembly://Coherence.Tests/Tangosol.Resources/s4hc-test-config.xml"), "POF configuration");
            CacheFactory.Shutdown();
            } 

    }
}
