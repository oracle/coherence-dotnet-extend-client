/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;

using NUnit.Framework;

namespace Tangosol.Util
{
    [TestFixture]
    public class UriUtilsTests
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
        public void GetSchemeSpecificPartTest()
        {
            Uri uri = new Uri("http://www.s4hc.com/services#1");
            Assert.AreEqual("//www.s4hc.com/services", UriUtils.GetSchemeSpecificPart(uri));
            uri = new Uri("http://www.s4hc.com/");
            Assert.AreEqual("//www.s4hc.com/", UriUtils.GetSchemeSpecificPart(uri));
            uri = new Uri("https://www.s4hc.com/services");
            Assert.AreEqual("//www.s4hc.com/services", UriUtils.GetSchemeSpecificPart(uri));

        }
    }
}
