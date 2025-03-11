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
    public class PagedEnumeratorTests
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
        public void TestPagedEnumerator()
        {
            object[] arr = new object[] {1, 2, 3, 4, 5};
            PagedEnumerator.IAdvancer advancer = new TestAdvancer(arr);
            PagedEnumerator enumerator = new PagedEnumerator(advancer);

            Exception e = null;
            object o = null;
            try
            {
                o = enumerator.Current;
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsNull(o);
            Assert.IsInstanceOf(typeof (InvalidOperationException), e);

            Assert.IsTrue(enumerator.MoveNext());
            o = enumerator.Current;
            Assert.IsNotNull(o);
            Assert.AreEqual(o, 1);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsTrue(enumerator.MoveNext());
            o = enumerator.Current;
            Assert.IsNotNull(o);
            Assert.AreEqual(o, 5);

            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Reset();
            Assert.IsTrue(enumerator.MoveNext());
            o = enumerator.Current;
            Assert.AreEqual(o, 1);
        }
    }
}
