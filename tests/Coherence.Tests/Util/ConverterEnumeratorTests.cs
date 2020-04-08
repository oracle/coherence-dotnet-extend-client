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
    public class ConverterEnumeratorTests
    {
        [Test]
        public void TestConverterEnumerator()
        {
            ArrayList list = new ArrayList();
            IConverter cDown = new ConverterCollectionsTests.ConvertDown();
            for (int i = 1; i < 4; i++)
            {
                list.Add(i);
            }

            IEnumerator convEnum1 = new ConverterEnumerator(list.GetEnumerator(), cDown);
            Assert.IsTrue(convEnum1.MoveNext());
            Assert.AreEqual(convEnum1.Current, cDown.Convert(list[0]));
            convEnum1.MoveNext();
            convEnum1.MoveNext();
            convEnum1.MoveNext();
            Assert.IsFalse(convEnum1.MoveNext());
            convEnum1.Reset();
            Assert.IsTrue(convEnum1.MoveNext());

            object[] array = new object[] { "a", 1 };
            IEnumerator convEnum2 = new ConverterEnumerator(array, cDown);
            Assert.IsTrue(convEnum2.MoveNext());
            Assert.AreEqual(convEnum2.Current, cDown.Convert(array[0]));
            convEnum2.MoveNext();
            Assert.IsFalse(convEnum2.MoveNext());
            convEnum2.Reset();
            Assert.IsTrue(convEnum2.MoveNext());
        }
    }
}