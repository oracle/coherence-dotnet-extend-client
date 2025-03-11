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
    public class SimpleEnumeratorTests
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
        public void SimpleEnumeratorTest()
        {
            object[] array = new object[] {8, 13, 21, 34, 55};
            SimpleEnumerator enumerator = new SimpleEnumerator(array);
            int i = 0;
            while(enumerator.MoveNext())
            {
                Assert.AreEqual(array[i++], enumerator.Current);
            }
            Assert.AreEqual(5, i);
            enumerator.Reset();
            i = 0;
            while (enumerator.MoveNext())
            {
                Assert.AreEqual(array[i++], enumerator.Current);
            }

            ArrayList al = new ArrayList(array);
            enumerator = new SimpleEnumerator(al);
            i = 0;
            while (enumerator.MoveNext())
            {
                Assert.AreEqual(array[i++], enumerator.Current);
            }
            Assert.AreEqual(5, i);



            SimpleEnumerator reverseEnumerator = new SimpleEnumerator(array, array.Length-1, 3, false, false);
            i = 4;
            while (reverseEnumerator.MoveNext())
            {
                Assert.AreEqual(array[i--], reverseEnumerator.Current);
            }
            Assert.AreEqual(1, i);
            reverseEnumerator.Reset();
            i = 4;
            while (reverseEnumerator.MoveNext())
            {
                Assert.AreEqual(array[i--], reverseEnumerator.Current);
            }
        }

        [Test]
        public void SimpleEnumeratorArgumentOutOfRangeException()
        {
            object[] array = new object[] { 8, 13, 21, 34, 55 };
            Assert.That(() => new SimpleEnumerator(array, array.Length - 1, 10, false, false),
                        Throws.TypeOf<ArgumentOutOfRangeException>());
        }
     }
}
