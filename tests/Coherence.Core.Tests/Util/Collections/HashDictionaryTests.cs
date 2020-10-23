/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using NUnit.Framework;

namespace Tangosol.Util.Collections
{
    [TestFixture]
    public class HashDictionaryTests : AbstractBaseDictionaryTests
    {
        #region Overrides of AbstractBaseDictionaryTests

        protected override IDictionary InstantiateDictionary()
        {
            return new HashDictionary();
        }

        #endregion

        [Test]
        public void TestContainsKey()
        {
            HashDictionary dict = new HashDictionary();
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey("B"));
            Assert.IsFalse(dict.ContainsKey(null));

            dict.Add("A", "A");
            dict.Add("B", "B");
            dict.Add("C", "C");
            Assert.AreEqual(3, dict.Count);
            Assert.IsTrue(dict.ContainsKey("B"));
            Assert.IsFalse(dict.ContainsKey(null));

            dict.Add(null, "NULL");
            Assert.AreEqual(4, dict.Count);
            Assert.IsTrue(dict.ContainsKey("B"));
            Assert.IsTrue(dict.ContainsKey(null));

            dict.Remove("B");
            Assert.AreEqual(3, dict.Count);
            Assert.IsFalse(dict.ContainsKey("B"));
            Assert.IsTrue(dict.ContainsKey(null));

            dict.Remove(null);
            Assert.AreEqual(2, dict.Count);
            Assert.IsFalse(dict.ContainsKey("B"));
            Assert.IsFalse(dict.ContainsKey(null));

            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey("A"));
            Assert.IsFalse(dict.ContainsKey(null));
        }

        [Test]
        public void TestContainsValue()
        {
            HashDictionary dict = new HashDictionary();
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsValue("B"));
            Assert.IsFalse(dict.ContainsValue(null));

            dict.Add("A", "A");
            dict.Add("B", "B");
            dict.Add("C", "C");
            Assert.AreEqual(3, dict.Count);
            Assert.IsTrue(dict.ContainsValue("B"));
            Assert.IsFalse(dict.ContainsValue(null));

            dict.Add(null, null);
            Assert.AreEqual(4, dict.Count);
            Assert.IsTrue(dict.ContainsValue("B"));
            Assert.IsTrue(dict.ContainsValue(null));

            dict.Remove("B");
            Assert.AreEqual(3, dict.Count);
            Assert.IsFalse(dict.ContainsValue("B"));
            Assert.IsTrue(dict.ContainsValue(null));

            dict.Remove(null);
            Assert.AreEqual(2, dict.Count);
            Assert.IsFalse(dict.ContainsValue("B"));
            Assert.IsFalse(dict.ContainsValue(null));

            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsValue("A"));
            Assert.IsFalse(dict.ContainsValue(null));
        }
    }
}