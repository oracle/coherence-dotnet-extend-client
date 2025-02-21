/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System;
using NUnit.Framework;
using Tangosol.IO.Pof.Reflection.Internal;

namespace Tangosol.IO.Pof.Reflection
{
    [TestFixture]
    public class ManglersTest
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
