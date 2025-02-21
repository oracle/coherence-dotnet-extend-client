/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using NUnit.Framework;
using System.Collections;
using System;

namespace Tangosol.Util
{
    [TestFixture]
    public class ImmutableMultiListTests
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
        public void SmallArray()
        {
            Object[] aL = _MakeArray(10);
            _Test(_MakeList(aL), aL);
        }

        [Test]
        public void MediumArray()
        {
            Object[] aL = _MakeArray(1000);
            _Test(_MakeList(aL), aL);
        }

        [Test]
        public void LargeArray()
        {
            Object[] aL = _MakeArray(10000);
            _Test(_MakeList(aL), aL);
        }


        // ----- internal test methods ------------------------------------------

        public IList _MakeList(Object[] aL)
        {
            return new ImmutableMultiList(new object[][] { aL });
        }

        public static Object[] _MakeArray(int c)
        {
            Assert.IsTrue(c > 0);
            Object[] aL = new Object[c];
            for (long i = 0; i < c; ++i)
            {
                aL[i] = i;
            }

            return aL;
        }

        /// <summary>
        /// Run a variety of tests of the following methods on a new
        /// ImmutableArrayList of the specified size:
        /// </summary>
        /// <param name="list">list to test</param>
        /// <param name="aL">expected values</param>
        static void _Test(IList list, Object[] aL)
        {
            int c = aL.Length;

            // size
            Assert.IsTrue(c == list.Count);

            // iterator
            {
                int i = 0;
                IEnumerator iter = list.GetEnumerator();
                for (; iter.MoveNext(); )
                {
                    Object l = iter.Current;
                    Assert.IsTrue(l == aL[i++]);
                }
                try
                {
                    Assert.IsFalse(iter.MoveNext());
                    object current = iter.Current;
                    Assert.IsTrue(false);
                }
                catch (InvalidOperationException)
                {
                    // expected
                }
            }

            // get
            for (int i = 0; i < c; ++i)
            {
                Assert.IsTrue(list[i].Equals(aL[i]));
            }

            // contains
            Assert.IsTrue(!list.Contains(null));                   // test null
            Assert.IsTrue(!list.Contains(-1L));           // test !contains
            Assert.IsTrue(list.Contains(0L) == (c != 0)); // test .equals()
            for (int i = 0; i < c; ++i)
            {
                Assert.IsTrue(list.Contains(aL[i]));
            }

            // indexOf
            Assert.IsTrue(list.IndexOf(null) == -1);                       // test null
            Assert.IsTrue(list.IndexOf(-1L) == -1);               // test !contains
            Assert.IsTrue(list.IndexOf(0L) == (c == 0 ? -1 : 0)); // test .equals()
            for (int i = 0; i < c; ++i)
            {
                Assert.IsTrue(list.IndexOf(aL[i]) == i);
            }

            // toArray
            {

                Object[] ao1 = new object[c];
                list.CopyTo(ao1, 0);
                
                for (int i = 0; i < c; ++i)
                {
                    Assert.IsTrue(ao1[i].Equals(aL[i]));
                }
            }

            if (c < 2)
            {
                return;
            }

            // test dups (changes "value index" logic)
            aL[c - 1] = aL[0];
            list = new ImmutableMultiList(new object[][] { aL });

            // contains
            Assert.IsTrue(!list.Contains(null));                   // test null
            Assert.IsTrue(!list.Contains(-1L));           // test !contains
            Assert.IsTrue(list.Contains(0L) == (c != 0)); // test .equals()
            for (int i = 0; i < c; ++i)
            {
                Assert.IsTrue(list.Contains(aL[i]));
            }

            // indexOf
            Assert.IsTrue(list.IndexOf(null) == -1);               // test null
            Assert.IsTrue(list.IndexOf(-1L) == -1);       // test !contains
            Assert.IsTrue(list.IndexOf(0L) == 0);         // test .equals()
            for (int i = 0; i < c - 1; ++i)
            {
                Assert.IsTrue(list.IndexOf(aL[i]) == i);
            }
        }
    }
}