/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using NUnit.Framework;

namespace Tangosol.Run.Xml
{
    [TestFixture]
    public class SimpleParserTests
    {
        [Test]
        public void TestSimpleParser()
        {
            SimpleParser parser = new SimpleParser();
            IXmlDocument xmlDoc = parser.ParseXml("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            Assert.IsNotNull(xmlDoc);
            Assert.AreEqual(xmlDoc.Name, "cache-config");
        }
    }
}
