/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using NUnit.Framework;

using Tangosol.Net.Cache;

namespace Tangosol.Util
{
    /// <summary>
    /// Unit tests for NullImplementation classes.
    /// </summary>
    [TestFixture]
    public class NullImplementationTests
    {
        #region NullCollection tests

        [Test]
        public void TestNullCollection()
        {
            ICollection nullCollection = NullImplementation.GetCollection();
            Assert.IsNotNull(nullCollection);
            Assert.IsInstanceOf(typeof(NullImplementation.NullCollection), nullCollection);
            Assert.AreEqual(nullCollection.Count, 0);
            IList list = new ArrayList();
            Assert.IsTrue(nullCollection.Equals(list));
            list.Add(1);
            Assert.IsFalse(nullCollection.Equals(list));
            Assert.AreEqual(nullCollection.GetHashCode(), 0);
            object[] array = new object[0];
            Assert.AreEqual(array.Length, 0);
            nullCollection.CopyTo(array, 0);
            Assert.AreEqual(array.Length, 0);
            array = new object[] {1, 2, 3};
            Assert.AreEqual(array.Length, 3);
            nullCollection.CopyTo(array, 0);
            Assert.AreEqual(array.Length, 3);

            IEnumerator nullEnumerator = nullCollection.GetEnumerator();
            Assert.IsNotNull(nullEnumerator);
            Assert.IsInstanceOf(typeof (NullImplementation.NullEnumerator), nullEnumerator);
            Assert.IsFalse(nullEnumerator.MoveNext());
            nullEnumerator.Reset();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNullCollectionSyncRoot()
        {
            object o = NullImplementation.NullCollection.Instance.SyncRoot;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNullCollectionEnumeratorCurrent()
        {
            IEnumerator enumerator = NullImplementation.NullCollection.Instance.GetEnumerator();
            object o = enumerator.Current;
        }

        #endregion

        #region NullDictionary tests

        [Test]
        public void TestNullDictionary()
        {
            IDictionary nullDictionary = NullImplementation.GetDictionary();
            Assert.IsNotNull(nullDictionary);
            Assert.IsInstanceOf(typeof (NullImplementation.NullDictionary), nullDictionary);
            Assert.AreEqual(nullDictionary.Count, 0);
            nullDictionary.Add(1, 1);
            Assert.AreEqual(nullDictionary.Count, 0);
            nullDictionary.Clear();
            Assert.AreEqual(nullDictionary.Count, 0);
            Assert.AreEqual(nullDictionary.GetHashCode(), 0);
            IDictionary hashtable = new Hashtable();
            Assert.IsTrue(nullDictionary.Equals(hashtable));
            hashtable.Add(1, 1);
            Assert.IsFalse(nullDictionary.Equals(hashtable));
            Assert.IsFalse(nullDictionary.Contains(2));
            Assert.IsTrue(nullDictionary.IsFixedSize);
            Assert.IsTrue(nullDictionary.IsReadOnly);
            nullDictionary.Remove(1);
            Assert.AreEqual(nullDictionary.Count, 0);

            ICollection keys = nullDictionary.Keys;
            Assert.IsNotNull(keys);
            Assert.IsInstanceOf(typeof (NullImplementation.NullCollection), keys);
            Assert.AreEqual(keys.Count, 0);
            ICollection values = nullDictionary.Values;
            Assert.IsNotNull(values);
            Assert.IsInstanceOf(typeof(NullImplementation.NullCollection), values);
            Assert.AreEqual(values.Count, 0);

            IDictionaryEnumerator nullEnumerator = nullDictionary.GetEnumerator();
            Assert.IsNotNull(nullEnumerator);
            Assert.IsInstanceOf(typeof(NullImplementation.NullEnumerator), nullEnumerator);
            Assert.IsFalse(nullEnumerator.MoveNext());

            int count = 0;
            foreach (DictionaryEntry entry in nullDictionary)
            {
                count++;
            }
            Assert.AreEqual(count, 0);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNullDictionaryEnumeratorKey()
        {
            IDictionaryEnumerator nullEnumerator = NullImplementation.NullDictionary.Instance.GetEnumerator();
            object o = nullEnumerator.Key;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNullDictionaryEnumeratorValue()
        {
            IDictionaryEnumerator nullEnumerator = NullImplementation.NullDictionary.Instance.GetEnumerator();
            object o = nullEnumerator.Value;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNullDictionaryEnumeratorEntry()
        {
            IDictionaryEnumerator nullEnumerator = NullImplementation.NullDictionary.Instance.GetEnumerator();
            object o = nullEnumerator.Entry;
        }

        #endregion

        #region NullCache tests

        [Test]
        public void TestNullCache()
        {
            ICache nullCache = NullImplementation.GetCache();
            Assert.IsNotNull(nullCache);
            Assert.IsInstanceOf(typeof(NullImplementation.NullCache), nullCache);
            Assert.AreEqual(nullCache.Count, 0);
            IList list = new ArrayList();
            list.Add(1);
            IDictionary ht = new Hashtable();
            ht.Add(1, 1);
            ht.Add(2, 2);
            nullCache.InsertAll(ht);
            Assert.AreEqual(nullCache.Count, 0);
            IDictionary res = nullCache.GetAll(list);
            Assert.IsNotNull(res);
            Assert.IsInstanceOf(typeof (NullImplementation.NullDictionary), res);
            object o = nullCache.Insert(1, 1);
            Assert.IsFalse(nullCache.Contains(1));
            Assert.IsNull(o);
            o = nullCache.Insert(1, 1, 100);
            Assert.IsFalse(nullCache.Contains(1));
            Assert.IsNull(o);

            ICollection entries = nullCache.Entries;
            Assert.IsNotNull(entries);
            Assert.IsInstanceOf(typeof (NullImplementation.NullCollection), entries);
            Assert.AreEqual(entries.Count, 0);

            ICacheEnumerator nullEnumerator = nullCache.GetEnumerator();
            Assert.IsNotNull(nullEnumerator);
            Assert.IsInstanceOf(typeof(NullImplementation.NullEnumerator), nullEnumerator);
            Assert.IsFalse(nullEnumerator.MoveNext());

            int count = 0;
            foreach (DictionaryEntry entry in nullCache)
            {
                count++;
            }
            Assert.AreEqual(count, 0);
        }

        #endregion

        #region NullValueExtractor tests

        [Test]
        public void TestNullValueExtractor()
        {
            IValueExtractor nullValueExtractor = NullImplementation.GetValueExtractor();
            Assert.IsNotNull(nullValueExtractor);
            Assert.IsInstanceOf(typeof (NullImplementation.NullValueExtractor), nullValueExtractor);
            object o = new object();
            object o1 = nullValueExtractor.Extract(o);
            Assert.AreEqual(o, o1);
        }

        #endregion
    }
}