/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using NUnit.Framework;

namespace Tangosol.Util
{
    [TestFixture]
    public class UriUtilsTests
    {
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
