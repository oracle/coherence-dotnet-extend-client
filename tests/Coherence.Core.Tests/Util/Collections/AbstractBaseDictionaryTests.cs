/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using NUnit.Framework;
using Tangosol.IO;
using Tangosol.IO.Pof;

namespace Tangosol.Util.Collections
{
    public abstract class AbstractBaseDictionaryTests
    {
        protected abstract IDictionary InstantiateDictionary();

        [Test]
        public void TestAddRemove()
        {
            IDictionary dict = InstantiateDictionary();
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.Contains("B"));
            Assert.IsFalse(dict.Contains(null));

            dict.Add("A", "A");
            dict.Add("B", "B");
            dict.Add("C", "C");
            Assert.AreEqual(3, dict.Count);
            Assert.IsTrue(dict.Contains("B"));
            Assert.IsFalse(dict.Contains(null));

            dict.Add(null, "NULL");
            Assert.AreEqual(4, dict.Count);
            Assert.IsTrue(dict.Contains("B"));
            Assert.IsTrue(dict.Contains(null));

            dict.Remove("B");
            Assert.AreEqual(3, dict.Count);
            Assert.IsFalse(dict.Contains("B"));
            Assert.IsTrue(dict.Contains(null));

            dict.Remove(null);
            Assert.AreEqual(2, dict.Count);
            Assert.IsFalse(dict.Contains("B"));
            Assert.IsFalse(dict.Contains(null));

            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.Contains("B"));
            Assert.IsFalse(dict.Contains(null));
        }

        [Test]
        public void TestIndexer()
        {
            IDictionary dict = InstantiateDictionary();
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.Contains("B"));
            Assert.IsFalse(dict.Contains(null));

            dict["A"] = "A";
            dict["B"] = "B";
            dict["C"] = "C";

            Assert.AreEqual(3, dict.Count);
            Assert.IsTrue(dict.Contains("B"));
            Assert.IsFalse(dict.Contains(null));
            Assert.IsNull(dict[null]);

            dict[null] = "NULL";
            Assert.AreEqual(4, dict.Count);
            Assert.IsTrue(dict.Contains("B"));
            Assert.IsTrue(dict.Contains(null));
            Assert.AreEqual("NULL", dict[null]);

            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.Contains("B"));
            Assert.IsFalse(dict.Contains(null));
        }

        [Test]
        public void TestCopyTo()
        {
            IDictionary dict = InstantiateDictionary();
            dict.Add("A", "A");
            dict.Add("B", "B");
            dict.Add("C", "C");

            DictionaryEntry[] ar = new DictionaryEntry[3];
            dict.CopyTo(ar, 0);

            Assert.AreEqual(3, ar.Length);
            Assert.IsTrue(Array.IndexOf(ar, new DictionaryEntry("B", "B")) >= 0);

            dict.Add(null, "NULL");
            ar = new DictionaryEntry[4];
            dict.CopyTo(ar, 0);

            Assert.AreEqual(4, ar.Length);
            Assert.AreEqual(new DictionaryEntry(null, "NULL"), ar[0]);
            Assert.IsTrue(Array.IndexOf(ar, new DictionaryEntry("B", "B")) >= 0);
        }

        [Test]
        public void TestKeys()
        {
            IDictionary dict = InstantiateDictionary();
            dict.Add("A", "A");
            dict.Add("B", "B");
            dict.Add("C", "C");

            ICollection keys = dict.Keys;
            Assert.AreEqual(3, keys.Count);
            
            ArrayList list = new ArrayList(keys);
            Assert.IsFalse(list.Contains(null));
            Assert.IsTrue(list.Contains("A"));
            Assert.IsTrue(list.Contains("B"));
            Assert.IsTrue(list.Contains("C"));

            dict.Add(null, "NULL");
            keys = dict.Keys;
            Assert.AreEqual(4, keys.Count);

            list = new ArrayList(keys);
            Assert.IsTrue(list.Contains(null));
            Assert.IsTrue(list.Contains("A"));
            Assert.IsTrue(list.Contains("B"));
            Assert.IsTrue(list.Contains("C"));
        }

        [Test]
        public void TestValues()
        {
            IDictionary dict = InstantiateDictionary();
            dict.Add("A", "A");
            dict.Add("B", "B");
            dict.Add("C", "C");

            ICollection values = dict.Values;
            Assert.AreEqual(3, values.Count);

            ArrayList list = new ArrayList(values);
            Assert.IsFalse(list.Contains("NULL"));
            Assert.IsTrue(list.Contains("A"));
            Assert.IsTrue(list.Contains("B"));
            Assert.IsTrue(list.Contains("C"));

            dict.Add(null, "NULL");
            values = dict.Values;
            Assert.AreEqual(4, values.Count);

            list = new ArrayList(values);
            Assert.IsTrue(list.Contains("NULL"));
            Assert.IsTrue(list.Contains("A"));
            Assert.IsTrue(list.Contains("B"));
            Assert.IsTrue(list.Contains("C"));
        }

        [Test]
        public void TestEnumerator()
        {
            IDictionary dict = InstantiateDictionary();
            dict.Add("A", "A");
            dict.Add("B", "B");
            dict.Add("C", "C");

            int n = 0;
            foreach (DictionaryEntry entry in dict)
            {
                n++;
            }
            Assert.AreEqual(3, n);

            dict.Add(null, null);
            n = 0;
            foreach (DictionaryEntry entry in dict)
            {
                n++;
            }
            Assert.AreEqual(4, n);
        }

        [Test]
        public void TestBinarySerialization()
        {
            ISerializer serializer = new BinarySerializer();

            IDictionary original = InstantiateDictionary();
            original.Add("A", "A");
            original.Add("B", "B");
            original.Add("C", "C");

            Binary bin  = SerializationHelper.ToBinary(original, serializer);
            Object copy = SerializationHelper.FromBinary(bin, serializer);

            Assert.AreEqual(original, copy);
            Assert.AreEqual(original.GetHashCode(), copy.GetHashCode());

            original.Add(null, null);
            bin  = SerializationHelper.ToBinary(original, serializer);
            copy = SerializationHelper.FromBinary(bin, serializer);

            Assert.AreEqual(original, copy);
            Assert.AreEqual(original.GetHashCode(), copy.GetHashCode());
            Assert.AreEqual(original.ToString(), copy.ToString());
        }

        [Test]
        public void TestPofSerialization()
        {
            ISerializer serializer = new SimplePofContext();

            IDictionary original = InstantiateDictionary();
            original.Add("A", "A");
            original.Add("B", "B");
            original.Add("C", "C");

            var stream        = new BinaryMemoryStream();
            serializer.Serialize(new DataWriter(stream), original);
            Binary     bin    = stream.ToBinary();
            IPofReader reader = new PofStreamReader(bin.GetReader(), (IPofContext) serializer);
            Object     copy   = reader.ReadDictionary(-1, InstantiateDictionary());

            Assert.AreEqual(original, copy);
            Assert.AreEqual(original.GetHashCode(), copy.GetHashCode());

            original.Add(null, null);
            stream = new BinaryMemoryStream();
            serializer.Serialize(new DataWriter(stream), original);
            bin    = stream.ToBinary();
            reader = new PofStreamReader(bin.GetReader(), (IPofContext)serializer);
            copy   = reader.ReadDictionary(-1, InstantiateDictionary());

            Assert.AreEqual(original, copy);
            Assert.AreEqual(original.GetHashCode(), copy.GetHashCode());
            Assert.AreEqual(original.ToString(), copy.ToString());
        }

        [Test]
        public void TestEquals()
        {
            IDictionary dict1 = InstantiateDictionary();
            dict1.Add("A", "A");
            dict1.Add("B", "B");
            dict1.Add("C", "C");

            IDictionary dict2 = InstantiateDictionary();
            dict2.Add("A", "A");
            dict2.Add("B", "B");
            dict2.Add("C", "C");

            IDictionary dict3 = InstantiateDictionary();
            dict3.Add("A", "A");
            dict3.Add("C", "C");

            IDictionary dict4 = InstantiateDictionary();
            dict4.Add("X", "X");
            dict4.Add("Y", "Y");
            dict4.Add("Z", "Z");

            Assert.IsTrue(dict1.Equals(dict1));
            Assert.IsTrue(dict1.Equals(dict2));
            Assert.IsFalse(dict1.Equals(null));
            Assert.IsFalse(dict1.Equals("invalid class"));
            Assert.IsFalse(dict1.Equals(dict3));
            Assert.IsFalse(dict1.Equals(dict4));

            dict1.Add(null, null);
            dict2.Add(null, null);
            dict3.Add(null, null);
            dict4.Add(null, null);
            Assert.IsTrue(dict1.Equals(dict1));
            Assert.IsTrue(dict1.Equals(dict2));
            Assert.IsFalse(dict1.Equals(null));
            Assert.IsFalse(dict1.Equals("invalid class"));
            Assert.IsFalse(dict1.Equals(dict3));
            Assert.IsFalse(dict1.Equals(dict4));
        }
    }
}