/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System;
using NUnit.Framework;

namespace Tangosol.Run.Xml
{
    [TestFixture]
    public class SimpleParserTests
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
        public void TestSimpleParser()
        {
            SimpleParser parser = new SimpleParser();
            IXmlDocument xmlDoc = parser.ParseXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            Assert.IsNotNull(xmlDoc);
            Assert.AreEqual(xmlDoc.Name, "cache-config");
        }
    }
}
