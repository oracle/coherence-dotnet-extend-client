/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using NUnit.Framework;

namespace Tangosol.Util
{
    [TestFixture]
    public class LongSortedListTests
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
        public void LongSortedListTest()
        {
            LongSortedList lsl = new LongSortedList();
            lsl[2] = "Ana";
            lsl[3] = "Goran";
            lsl.Add("Ivan");
            Assert.AreEqual("Ivan", lsl[4]);
            Assert.AreEqual(2, lsl.FirstIndex);
            long index = lsl.Add("Milos");
            Assert.AreEqual(5, index);
            Assert.IsTrue(lsl.Exists(5));
            Assert.IsNull(lsl.Remove(8));
            Object removed = lsl.Remove(5);
            Assert.IsNotNull(removed);
            Assert.IsFalse(lsl.Contains("Jason"));
            IDictionaryEnumerator e = (IDictionaryEnumerator) lsl.GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("Ana", e.Value);
            Assert.AreEqual(2, e.Key);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("Goran", e.Value);

            e = (IDictionaryEnumerator) lsl.GetEnumerator(4);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("Ivan", e.Value);
            Assert.AreEqual(4, e.Key);

            lsl[28] = "Sele";
            e = (IDictionaryEnumerator)lsl.GetEnumerator(10);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("Sele", e.Value);
            Assert.AreEqual(28, e.Key);

            e = (IDictionaryEnumerator)lsl.GetEnumerator(1);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("Ana", e.Value);
            Assert.AreEqual(2, e.Key);
        }
    }
}
