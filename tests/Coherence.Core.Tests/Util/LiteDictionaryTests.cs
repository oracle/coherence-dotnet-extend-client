/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

using NUnit.Framework;

namespace Tangosol.Util
{
    [TestFixture]
    public class LiteDictionaryTests
    {
        [Test]
        public void DefaultConstructorTest()
        {
            LiteDictionary ld = new LiteDictionary();
            Assert.IsNotNull(ld);
            Assert.AreEqual(ld.Count, 0);
            Assert.IsTrue(ld.IsEmpty);
            Assert.IsFalse(ld.IsFixedSize);
            Assert.IsFalse(ld.IsReadOnly);
            Assert.IsFalse(ld.IsSynchronized);
            Assert.AreEqual(ld.Keys.Count, 0);
            Assert.AreEqual(ld.Values.Count, 0);
        }

        [Test]
        public void ConstructorWithDictionaryTest()
        {
            Hashtable ht = new Hashtable();
            ht.Add("key1", "value1");
            ht.Add("key2", "value2");
            ht.Add("key3", "value3");

            LiteDictionary ld = new LiteDictionary(ht);
            Assert.IsNotNull(ld);
            Assert.AreEqual(ld.Count, 3);
            Assert.IsFalse(ld.IsEmpty);

            Assert.AreEqual(ld.Keys.Count, 3);
            Assert.AreEqual(ld.Values.Count, 3);
            Assert.AreEqual(ld["key1"], ht["key1"]);
            Assert.IsTrue(ld.Contains("key2"));

            ld.Clear();
            Assert.IsTrue(ld.IsEmpty);
        }

        [Test]
        public void SingleLiteDictionaryTest()
        {
            LiteDictionary ld = new LiteDictionary();
            ld.Add("key", "value");

            Assert.AreEqual(ld.Count, 1);
            Assert.IsFalse(ld.IsEmpty);
            Assert.IsTrue(ld.Contains("key"));
            Assert.IsFalse(ld.Contains("dummy"));
            Assert.AreEqual(ld.Keys.Count, 1);
            Assert.AreEqual(ld.Values.Count, 1);
            Assert.AreEqual(ld["key"], "value");
            ld["key"] = "new value";
            Assert.AreEqual(ld["key"], "new value");
            foreach (object o in ld)
            {
                Assert.IsInstanceOf(typeof(DictionaryEntry), o);
            }

            object[] array = new object[1];
            ld.CopyTo(array, 0);
            Assert.AreEqual(array.Length, ld.Count);
            Assert.IsInstanceOf(typeof(DictionaryEntry), array[0]);
            Assert.AreEqual(((DictionaryEntry) array[0]).Value, ld["key"]);
            ld.Remove("key");
            Assert.AreEqual(ld.Count, 0);
        }

        [Test]
        public void Array3LiteDictionaryTest()
        {
            LiteDictionary ld = new LiteDictionary();
            ld.Add("key1", "value1");
            ld.Add("key2", "value2");
            ld.Add("key3", "value3");

            Assert.AreEqual(ld.Count, 3);
            Assert.IsFalse(ld.IsEmpty);
            Assert.IsTrue(ld.Contains("key1"));
            Assert.IsFalse(ld.Contains("dummy"));
            Assert.AreEqual(ld.Keys.Count, 3);
            Assert.AreEqual(ld.Values.Count, 3);
            Assert.AreEqual(ld["key1"], "value1");
            ld["key1"] = "new value1";
            Assert.AreEqual(ld["key1"], "new value1");
            foreach (object o in ld)
            {
                Assert.IsInstanceOf(typeof(DictionaryEntry), o);
            }

            object[] array = new object[3];
            ld.CopyTo(array, 0);
            Assert.AreEqual(array.Length, ld.Count);
            Assert.IsInstanceOf(typeof(DictionaryEntry), array[0]);
            ld.Remove("key1");
            Assert.AreEqual(ld.Count, 2);
        }

        [Test]
        public void OtherLiteDictionaryTest()
        {
            Hashtable ht = new Hashtable();
            for (int i = 0; i < 9; i++)
            {
                ht.Add("key" + (i+1), "value" + (i+1));
            }
            LiteDictionary ld = new LiteDictionary(ht);

            Assert.AreEqual(ld.Count, ht.Count);
            Assert.IsFalse(ld.IsEmpty);
            Assert.IsTrue(ld.Contains("key1"));
            Assert.IsFalse(ld.Contains("dummy"));
            Assert.AreEqual(ld.Keys.Count, ht.Keys.Count);
            Assert.AreEqual(ld.Values.Count, ht.Values.Count);
            Assert.AreEqual(ld["key1"], ht["key1"]);
            ld["key1"] = "new value1";
            Assert.AreEqual(ld["key1"], "new value1");
            foreach (object o in ld)
            {
                Assert.IsInstanceOf(typeof(DictionaryEntry), o);
            }

            object[] array = new object[9];
            ld.CopyTo(array, 0);
            Assert.AreEqual(array.Length, ld.Count);
            Assert.IsInstanceOf(typeof(DictionaryEntry), array[0]);
            ld.Remove("key1");
            Assert.AreEqual(ld.Count, 8);
        }

        // Test for COHNET-99
        [Test]
        public void GrowingAndShrinkingLiteDictionaryTest()
        {
            LiteDictionary ld = new LiteDictionary();
            ld.Add(1, 1);
            ld.Add(2, 2);
            ld.Add(3, 3);
            ld.Add(4, 4);
            ld.Add(5, 5);
            ld.Add(6, 6);
            ld.Add(7, 7);
            ld.Remove(7);
            ld.Add(8, 8);
            ld.Add(9, 9);
            ld.Add(10, 10);
        }

        [Test]
        public void TestForCOH2353()
        {
            LiteDictionary ld = new LiteDictionary();
            ld.Add(1, 1);
            ld.Add(2, 2);
            ld.Add(3, 3);
            ld.Add(null, 4);
            ld.Add(5, 5);
            ld.Add(6, 6);
            ld.Add(7, 7);
            ld.Remove(7);
            ld.Add(8, 8);
            ld.Add(9, 9);
            ld.Add(10, 10);

            Assert.IsTrue(ld.Contains(null));
        }
    }
}
