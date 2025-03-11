/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System;
using System.Collections;
using NUnit.Framework;

namespace Tangosol.Util.Collections
{
    [TestFixture]
    public class HashSetTests
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
        public void TestHashSet()
        {
            HashSet set = new HashSet();
            Assert.AreEqual(0, set.Count);
            Assert.IsFalse(set.Contains("B"));
            Assert.IsFalse(set.Contains(null));

            set.Add("A");
            set.Add("B");
            set.Add("C");
            Assert.AreEqual(3, set.Count);
            Assert.IsTrue(set.Contains("B"));
            Assert.IsFalse(set.Contains(null));

            set.Add(null);
            Assert.AreEqual(4, set.Count);
            Assert.IsTrue(set.Contains("B"));
            Assert.IsTrue(set.Contains(null));

            set.Remove("B");
            Assert.AreEqual(3, set.Count);
            Assert.IsFalse(set.Contains("B"));
            Assert.IsTrue(set.Contains(null));

            set.Remove(null);
            Assert.AreEqual(2, set.Count);
            Assert.IsFalse(set.Contains("B"));
            Assert.IsFalse(set.Contains(null));

            set.Clear();
            Assert.AreEqual(0, set.Count);
            Assert.IsFalse(set.Contains("A"));
            Assert.IsFalse(set.Contains(null));

            HashDictionary dict = new HashDictionary();
            dict.Add("A", "A");
            dict.Add("B", "B");
            dict.Add("C", "C");
            dict.Add("Null", null);

            ICollection col = dict.Values;
            set = new HashSet(col);
            Assert.AreEqual(4, set.Count);
            Assert.IsTrue(set.Contains("A"));
            Assert.IsTrue(set.Contains(null));
        }

        [Test]
        public void TestSafeHashSet()
        {
            SafeHashSet set = new SafeHashSet();
            Assert.AreEqual(0, set.Count);
            Assert.IsFalse(set.Contains("B"));
            Assert.IsFalse(set.Contains(null));

            set.Add("A");
            set.Add("B");
            set.Add("C");
            Assert.AreEqual(3, set.Count);
            Assert.IsTrue(set.Contains("B"));
            Assert.IsFalse(set.Contains(null));

            set.Add(null);
            Assert.AreEqual(4, set.Count);
            Assert.IsTrue(set.Contains("B"));
            Assert.IsTrue(set.Contains(null));

            set.Remove("B");
            Assert.AreEqual(3, set.Count);
            Assert.IsFalse(set.Contains("B"));
            Assert.IsTrue(set.Contains(null));

            set.Remove(null);
            Assert.AreEqual(2, set.Count);
            Assert.IsFalse(set.Contains("B"));
            Assert.IsFalse(set.Contains(null));

            set.Clear();
            Assert.AreEqual(0, set.Count);
            Assert.IsFalse(set.Contains("A"));
            Assert.IsFalse(set.Contains(null));

            HashDictionary dict = new HashDictionary();
            dict.Add("A", "A");
            dict.Add("B", "B");
            dict.Add("C", "C");
            dict.Add("Null", null);

            ICollection col = dict.Values;
            set = new SafeHashSet(col);
            Assert.AreEqual(4, set.Count);
            Assert.IsTrue(set.Contains("A"));
            Assert.IsTrue(set.Contains(null));
        }
    }
}