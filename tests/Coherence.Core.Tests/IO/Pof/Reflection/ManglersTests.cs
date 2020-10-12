/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using NUnit.Framework;
using Tangosol.IO.Pof.Reflection.Internal;

namespace Tangosol.IO.Pof.Reflection
{
    [TestFixture]
    public class ManglersTest
    {
        [Test]
        public void TestFieldMangler()
        {
            Assert.AreEqual("foo", NameManglers.FIELD_MANGLER.Mangle("m_fFoo"));
        }

        [Test]
        public void TestMethodMangler()
        {
            var mangler = NameManglers.METHOD_MANGLER;

            string sGetter = mangler.Mangle("GetFoo");
            string sSetter = mangler.Mangle("SetFoo");
            string sIs     = mangler.Mangle("IsFoo");

            Assert.AreEqual("foo", sGetter);
            Assert.AreEqual("foo", sSetter);
            Assert.AreEqual("foo", sIs    );
        }

        [Test]
        public void TestPropertyMangler()
        {
            var mangler = NameManglers.PROPERTY_MANGLER;

            Assert.AreEqual("foo", mangler.Mangle("Foo"));
        }
    }
}
