/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using NUnit.Framework;

namespace Tangosol.Util
{
    [TestFixture]
    public class CollectionUtilsTests
    {
        [Test]
        public void TestArray()
        {
            Assert.That(() => CollectionUtils.AddAll(new object[] {1, "2", '3'}, new object[] {4, "5", '6'}), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void TestBitArray()
        {
            Assert.That(() => CollectionUtils.AddAll(new BitArray(10), new bool[] { true, false, false }), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void TestDictionaryException()
        {
            Hashtable target = new Hashtable();
            Assert.That(() => CollectionUtils.AddAll(target, new Object[] { "one", 1, "two", 2, "three", 3 }), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void TestArrayList()
        {
            IList target = new ArrayList();
            CollectionUtils.AddAll(target, new object[] { 4, "5", '6' });
            Assert.AreEqual(4, target[0]);
            Assert.AreEqual("5", target[1]);
            Assert.AreEqual('6', target[2]);
        }

        [Test]
        public void TestQueue()
        {
            System.Collections.Queue target = new System.Collections.Queue();
            CollectionUtils.AddAll(target, new object[] { 4, "5", '6' });
            Assert.AreEqual(4, target.Dequeue());
            Assert.AreEqual("5", target.Dequeue());
            Assert.AreEqual('6', target.Dequeue());
        }

        [Test]
        public void TestStack()
        {
            Stack target = new Stack();
            CollectionUtils.AddAll(target, new object[] { 4, "5", '6' });
            Assert.AreEqual('6', target.Pop());
            Assert.AreEqual("5", target.Pop());
            Assert.AreEqual(4, target.Pop());
        }

        [Test]
        public void TestDictionary()
        {
            Hashtable target = new Hashtable();
            Hashtable source = new Hashtable();
            source.Add("one", 1);
            source.Add("two", 2);
            source.Add("three", 3);
            CollectionUtils.AddAll(target, source);
            for (int i = 0; i < target.Count; i++)
            {
                Assert.AreEqual(source[i], target[i]);
            }
        }

        [Test]
        public void TestToArray()
        {
            Object[] ao;
            ArrayList al = new ArrayList();
            al.Add("one");
            al.Add(1);
            al.Add(true);

            ao = CollectionUtils.ToArray(al);
            Assert.AreEqual(al[0], ao[0]);
            Assert.AreEqual(al[1], ao[1]);
            Assert.AreEqual(al[2], ao[2]);
        }

        [Test]
        public void TestDictionaryToArray()
        {
            Hashtable h = new Hashtable();
            h.Add("1", "one");
            h.Add("2", "two");

            Object[] ao = CollectionUtils.ToArray(h);
            foreach (DictionaryEntry entry in ao)
            {
                Assert.AreEqual(h[entry.Key], entry.Value);
            }
        }

        [Test]
        public void TestContainsAll()
        {
            ArrayList target = new ArrayList();
            CollectionUtils.AddAll(target, new object[] {1, "2", true, 2, 3, 4});
            ArrayList source = new ArrayList();
            CollectionUtils.AddAll(source, new object[] {1, true});
            Assert.IsTrue(CollectionUtils.ContainsAll(target, source));
            source.Add("false");
            Assert.IsFalse(CollectionUtils.ContainsAll(target, source));

            Object[] o = new Object[] {1, "2", true, 2, 3, 4};
            Assert.IsFalse(CollectionUtils.ContainsAll(o, source));
            source.Remove("false");
            Assert.IsTrue(CollectionUtils.ContainsAll(o, source));

            Hashtable targetHt = new Hashtable();
            targetHt.Add("one", 1);
            targetHt.Add("two", 2);
            targetHt.Add("three", 3);
            Hashtable sourceHt = new Hashtable();
            sourceHt.Add("one", 1);
            Assert.IsTrue(CollectionUtils.ContainsAll(targetHt, sourceHt));
        }

        [Test]
        public void TestContainsKey()
        {
            Hashtable target = new Hashtable();
            target.Add("one", 1);
            target.Add("two", 2);
            target.Add("three", 3);
            Assert.IsTrue(CollectionUtils.Contains(target, "one"));
        }

        [Test]
        public void TestRemoveAll()
        {
            ArrayList target = new ArrayList();
            CollectionUtils.AddAll(target, new object[] { 1, "2", true, 2, 3, 4 });
            ArrayList source = new ArrayList();
            CollectionUtils.AddAll(source, new object[] { 1, true });
            Assert.IsTrue(CollectionUtils.RemoveAll(target, source));
            Assert.IsFalse(target.Contains(source[0]));
            Assert.IsFalse(target.Contains(source[1]));

            source.Add("one");
            source.Add("two");
            source.Add(4);
            source.Add("2");
            source.Add(2);


            CollectionUtils.RemoveAll(target, source);
            Assert.IsFalse(target.Contains(source));

            Hashtable targetHt = new Hashtable();
            targetHt.Add("one", 1);
            targetHt.Add("two", 2);
            targetHt.Add("three", 3);
            Hashtable sourceHt = new Hashtable();
            sourceHt.Add("one", 1);
            CollectionUtils.RemoveAll(targetHt, sourceHt);
            Assert.IsFalse(targetHt.Contains(sourceHt));

        }

        [Test]
        public void TestRemove()
        {
            ArrayList target = new ArrayList();
            CollectionUtils.AddAll(target, new object[] { 1, "2", true, 2, 3, 4 });

            Assert.IsFalse(CollectionUtils.Remove(target, 'a'));

        }

        [Test]
        public void TestRetainAll()
        {
            ArrayList target = new ArrayList();
            CollectionUtils.AddAll(target, new object[] { 1, "2", true, 2, 3, 4 });
            ArrayList source = new ArrayList();
            CollectionUtils.AddAll(source, new object[] { 1, true, "two", false, 5 });
            Assert.IsTrue(CollectionUtils.RetainAll(target, source));
            Assert.IsTrue(target.Contains(source[0]));
            Assert.IsTrue(target.Contains(source[1]));

            Hashtable targetHt = new Hashtable();
            targetHt.Add("one", 1);
            targetHt.Add("two", 2);
            targetHt.Add("three", 3);
            Hashtable sourceHt = new Hashtable();
            sourceHt.Add("one", 1);
            CollectionUtils.RetainAll(targetHt, sourceHt);
            Assert.IsTrue(targetHt.ContainsValue(1));
            Assert.IsFalse(targetHt.ContainsValue(2));

        }

        [Test]
        public void TestRandomize()
        {
            object[] ao = new object[] {1};
            object[] a1 = CollectionUtils.Randomize(ao);
            Assert.AreEqual(ao.Length, a1.Length);
            Assert.AreEqual(ao[0], a1[0]);

            ao = new object[] {1, 2, 3, 4};
            a1 = CollectionUtils.Randomize(ao);
            Assert.AreEqual(ao.Length, a1.Length);
        }

        [Test]
        public void ToSByteArray()
        {
            byte[] b = new byte[] {34, 2, 0};
            sbyte[] sb = CollectionUtils.ToSByteArray(b);
            Assert.AreEqual(sb[0], Convert.ToSByte(b[0]));
            Assert.AreEqual(sb[1], Convert.ToSByte(b[1]));
            Assert.AreEqual(sb[2], Convert.ToSByte(b[2]));
        }

        [Test]
        public void ToByteArray()
        {
            sbyte[] sb = new sbyte[] { 34, 2, 0 };
            byte[] b = CollectionUtils.ToByteArray(sb);
            Assert.AreEqual(b[0], Convert.ToByte(sb[0]));
            Assert.AreEqual(b[1], Convert.ToByte(sb[1]));
            Assert.AreEqual(b[2], Convert.ToByte(sb[2]));
        }

        [Test]
        public void TestEqualsDeep()
        {
            /*
             * Tests each type (except Object) that is
             * handled by EqualsDeep. Test cases:
             * 1) one object is null
             * 2) same object for both params
             * 3) different length arrays
             * 4) different values in same length arrays
             * 5) equal arrays (same length, elements are the same)
             * 6) arrays of two different types - only done once
             */

            // byte array
            // 1
            byte[] b1 = new byte[] { 34, 2, 0 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(b1, null));

            // 2
            Assert.IsTrue(CollectionUtils.EqualsDeep(b1, b1));

            // 3
            byte[] b2 = new byte[] { 34, 2, 0, 5 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(b1, b2));

            // 4
            byte[] b3 = new byte[] { 2, 0, 5 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(b1, b3));

            // 5
            byte[] b4 = new byte[] { 34, 2, 0 };
            Assert.IsTrue(CollectionUtils.EqualsDeep(b1, b4));

            // 6
            int[] int1 = new int[] { 15, 16, 17 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(b1, int1));

            // int array
            // 1
            int[] i1 = new int[] { 34, 2, 0 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(i1, null));

            // 2
            Assert.IsTrue(CollectionUtils.EqualsDeep(i1, i1));

            // 3
            int[] i2 = new int[] { 34, 2, 0, 5 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(i1, i2));

            // 4
            int[] i3 = new int[] { 2, 0, 5 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(i1, i3));

            // 5
            int[] i4 = new int[] { 34, 2, 0 };
            Assert.IsTrue(CollectionUtils.EqualsDeep(i1, i4));

            // char array
            // 1
            char[] c1 = new char[] { 'w', 'x', 'y' };
            Assert.IsFalse(CollectionUtils.EqualsDeep(c1, null));

            // 2
            Assert.IsTrue(CollectionUtils.EqualsDeep(c1, c1));

            // 3
            char[] c2 = new char[] { 'w', 'x', 'y', 'z' };
            Assert.IsFalse(CollectionUtils.EqualsDeep(c1, c2));

            // 4
            char[] c3 = new char[] { 'a', 'b', 'c' };
            Assert.IsFalse(CollectionUtils.EqualsDeep(c1, c3));

            // 5
            char[] c4 = new char[] { 'w', 'x', 'y' };
            Assert.IsTrue(CollectionUtils.EqualsDeep(c1, c4));

            // long array
            // 1
            long[] l1 = new long[] { 34, 2, 0 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(l1, null));

            // 2
            Assert.IsTrue(CollectionUtils.EqualsDeep(l1, l1));

            // 3
            long[] l2 = new long[] { 34, 2, 0, 5 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(l1, l2));

            // 4
            long[] l3 = new long[] { 2, 0, 5 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(l1, l3));

            // 5
            long[] l4 = new long[] { 34, 2, 0 };
            Assert.IsTrue(CollectionUtils.EqualsDeep(l1, l4));

            // double array
            // 1
            double[] d1 = new double[] { 34.0, 2.0, 0.0 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(d1, null));

            // 2
            Assert.IsTrue(CollectionUtils.EqualsDeep(d1, d1));

            // 3
            double[] d2 = new double[] { 34.0, 2.0, 0.0, 5.0 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(d1, d2));

            // 4
            double[] d3 = new double[] { 2.0, 0.0, 5.0 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(d1, d3));

            // 5
            double[] d4 = new double[] { 34.0, 2.0, 0.0 };
            Assert.IsTrue(CollectionUtils.EqualsDeep(d1, d4));

            // bool array
            // 1
            bool[] bool1 = new bool[] { true, false, true };
            Assert.IsFalse(CollectionUtils.EqualsDeep(bool1, null));

            // 2
            Assert.IsTrue(CollectionUtils.EqualsDeep(bool1, bool1));

            // 3
            bool[] bool2 = new bool[] { true, false, true, false };
            Assert.IsFalse(CollectionUtils.EqualsDeep(bool1, bool2));

            // 4
            bool[] bool3 = new bool[] { false, true, false };
            Assert.IsFalse(CollectionUtils.EqualsDeep(bool1, bool3));

            // 5
            bool[] bool4 = new bool[] { true, false, true };
            Assert.IsTrue(CollectionUtils.EqualsDeep(bool1, bool4));

            // short array
            // 1
            short[] s1 = new short[] { 34, 2, 0 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(s1, null));

            // 2
            Assert.IsTrue(CollectionUtils.EqualsDeep(s1, s1));

            // 3
            short[] s2 = new short[] { 34, 2, 0, 5 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(s1, s2));

            // 4
            short[] s3 = new short[] { 2, 0, 5 };
            Assert.IsFalse(CollectionUtils.EqualsDeep(s1, s3));

            // 5
            short[] s4 = new short[] { 34, 2, 0 };
            Assert.IsTrue(CollectionUtils.EqualsDeep(s1, s4));

            // float array
            // 1
            float[] f1 = new float[] { 34.0F, 2.0F, 0.0F };
            Assert.IsFalse(CollectionUtils.EqualsDeep(f1, null));

            // 2
            Assert.IsTrue(CollectionUtils.EqualsDeep(f1, f1));

            // 3
            float[] f2 = new float[] { 34.0F, 2.0F, 0.0F, 5.0F };
            Assert.IsFalse(CollectionUtils.EqualsDeep(f1, f2));

            // 4
            float[] f3 = new float[] { 2.0F, 0.0F, 5.0F };
            Assert.IsFalse(CollectionUtils.EqualsDeep(f1, f3));

            // 5
            float[] f4 = new float[] { 34.0F, 2.0F, 0.0F };
            Assert.IsTrue(CollectionUtils.EqualsDeep(f1, f4));
        }
    }
}