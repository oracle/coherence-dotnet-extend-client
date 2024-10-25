/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;

namespace Tangosol.Util.Collections
{
    [TestFixture]
    public class SortedDictionaryTests : AbstractBaseDictionaryTests
    {
        #region Overrides of AbstractBaseDictionaryTests

        protected override IDictionary InstantiateDictionary()
        {
            return new SortedDictionary();
        }

        #endregion

        [Test]
        public void TestContainsKey()
        {
            SortedDictionary dict = new SortedDictionary();
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
            SortedDictionary dict = new SortedDictionary();
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

        [Test]
        public void TestSortedEnumerator()
        {
            IDictionary dict = InstantiateDictionary();
            dict.Add("A", "A");
            dict.Add("B", "B");
            dict.Add("C", "C");

            String[] expected = new String[] {"A", "B", "C"};

            int n = 0;
            foreach (DictionaryEntry entry in dict)
            {
                Assert.AreEqual(expected[n++], entry.Key);
            }
            Assert.AreEqual(3, n);

            dict.Add(null, null);
            expected = new String[] { null, "A", "B", "C" };

            n = 0;
            foreach (DictionaryEntry entry in dict)
            {
                Assert.AreEqual(expected[n++], entry.Key);
            }
            Assert.AreEqual(4, n);
        }

        [Test]
        public void TesSortedHashSet()
        {
            var setSorted = new SortedHashSet();

            setSorted.Add("C");
            setSorted.Add("A");
            setSorted.Add("B");

            // assert we have ascending characters
            EnsureAlphabetic(setSorted);
        }

        [Test]
        public void TestSortedHashSetConstruction()
        {
            var listValues = new ArrayList(3);
            listValues.Add("C");
            listValues.Add("A");
            listValues.Add("B");

            var setSorted = new SortedHashSet(listValues);

            EnsureAlphabetic(setSorted);

            // assert the underlying dictionary created is of the sorted variety
            AssertInstanceOf(typeof(SortedHashSet), "m_dict", setSorted, typeof(SortedDictionary));
        }

        private static void EnsureAlphabetic(ICollection colSorted)
        {
            // assert we have ascending characters
            int start = 'A';
            foreach (string alpha in colSorted)
            {
                Assert.AreEqual(start++, alpha[0]);
            }
        }

        private static void AssertInstanceOf(Type container, string fieldName, object value, Type expected)
        {
            // assert the underlying dictionary created is of the sorted variety
            FieldInfo field = container.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (field == null)
            {
                return;
            }
            var dict = field.GetValue(value);

            Assert.IsInstanceOf(expected, dict);
        }
    }
}