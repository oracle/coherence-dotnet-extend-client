/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using NUnit.Framework;

namespace Tangosol.Util
{
    [TestFixture]
    public class AtomicCounterTests
    {
        [Test]
        public void CounterTest()
        {
            AtomicCounter counter = AtomicCounter.NewAtomicCounter();
            Assert.AreEqual(0, counter.GetCount());
            counter.Increment();
            Assert.AreEqual(1, counter.GetCount());
            counter.Increment(9);
            Assert.AreEqual(10, counter.GetCount());
            counter.Decrement();
            Assert.AreEqual(9, counter.GetCount());
            counter.Decrement(4);
            Assert.AreEqual(5, counter.GetCount());
            Assert.AreEqual(counter.PostDecrement(), 5);
            Assert.AreEqual(4, counter.GetCount());
            Assert.AreEqual(4, counter.PostIncrement());
            Assert.AreEqual(5, counter.GetCount());

            counter = AtomicCounter.NewAtomicCounter(2006);
            Assert.AreEqual(2006, counter.SetCount(2007));
            Assert.IsTrue(counter.SetCount(2007, 2006));
            Assert.IsFalse(counter.SetCount(2000, 2008));
            Assert.AreEqual(2006.ToString(), counter.ToString());

        }
    }
}
