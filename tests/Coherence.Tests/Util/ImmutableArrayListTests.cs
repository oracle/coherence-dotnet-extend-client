/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.Collections;
using NUnit.Framework;

namespace Tangosol.Util
{

    [TestFixture]
    public class ImmutableArrayListTests
    {
        /// <summary>
        /// Test of equality rules between two instances of ImmutableArrayList.
        /// </summary>
        [Test]
        public void TestEqualityRules()
        {
            ImmutableArrayList first  = new ImmutableArrayList(AsArray("1", "2", "3"));
            ImmutableArrayList second = new ImmutableArrayList(AsArray("1", "2", "3"));

            // x.Equals(null) == false
            Assert.IsFalse(first.Equals(null));
            // x.Equals(x) == true
            Assert.IsTrue(first.Equals(first));

            
            // x.Equals(y) == y.Equals(x)
            Assert.IsTrue(first.Equals(second));
            Assert.AreEqual(first.Equals(second), second.Equals(first));
        }

        /// <summary>
        /// Test of equal method where collection to compare with is some
        /// other ICollection implementation (Queue, for example)
        /// </summary>
        [Test]
        public void TestEqualityBehaviorWithCollection()
        {
            ImmutableArrayList iaList = new ImmutableArrayList(AsArray("1", "2", "3"));
            Queue queue1 = new Queue(AsArray("1", "2", "3"));
            Queue queue2 = new Queue(AsArray("2", "3", "1"));
            Queue queue3 = new Queue(AsArray("1", "5", "3"));
            Queue queue4 = new Queue(AsArray("1", "2"));
            
            // same size, same elements
            Assert.IsTrue(iaList.Equals(queue1));
            // same size, same elements, not ordered
            Assert.IsTrue(iaList.Equals(queue2));
            // same size, different elements
            Assert.IsFalse(iaList.Equals(queue3));
            // different size
            Assert.IsFalse(iaList.Equals(queue4));
        }

        /// <summary>
        /// Test of equal method where collection to compare with is some
        /// other IList implementation (ArrayList, for example)
        /// </summary>
        [Test]
        public void TestEqualityBehaviorWithList()
        {
            ImmutableArrayList iaList = new ImmutableArrayList(AsArray("1", "2", "3"));
            ArrayList arrList1 = new ArrayList(AsArray("1", "2", "3"));
            ArrayList arrList2 = new ArrayList(AsArray("1", "3", "2"));
            ArrayList arrList3 = new ArrayList(AsArray("1", "2", "3", "4"));
            ArrayList arrList4 = new ArrayList(AsArray("1", "2"));

            // same size, same elements, same order
            Assert.IsTrue(iaList.Equals(arrList1));
            // same size, same elements, not ordered the same
            Assert.IsFalse(iaList.Equals(arrList2));
            // same size, different elements
            Assert.IsFalse(iaList.Equals(arrList3));
            // different size
            Assert.IsFalse(iaList.Equals(arrList4));
        }

        private static object[] AsArray(params object[] elements)
        {
            return elements;
        }
    }
}