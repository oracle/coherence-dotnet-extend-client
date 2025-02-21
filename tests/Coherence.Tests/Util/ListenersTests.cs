/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System;
using NUnit.Framework;

using Tangosol.Net.Cache;

namespace Tangosol.Util
{
    [TestFixture]
    public class ListenersTests
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
        public void InitializationTest()
        {
            Listeners l = new Listeners();
            Assert.IsTrue(l.IsEmpty);
            ICacheListener[] a = l.ListenersArray;
            Assert.AreEqual(a.Length, 0);
        }

        [Test]
        public void ListenersAddRemoveTest()
        {
            Listeners l = new Listeners();
            Assert.IsTrue(l.IsEmpty);

            DummyEventListener d1 = new DummyEventListener();
            DummyEventListener d2 = new DummyEventListener();
            DummyEventListener d3 = new DummyEventListener();

            l.Add(d1);
            Assert.IsFalse(l.IsEmpty);
            Assert.IsTrue(l.Contains(d1));
            Assert.AreEqual(l.ListenersArray.Length, 1);
            l.Add(null);
            Assert.AreEqual(l.ListenersArray.Length, 1);
            l.Add(d1);
            Assert.AreEqual(l.ListenersArray.Length, 1);
            l.Add(d2);
            Assert.IsTrue(l.Contains(d2));
            Assert.AreEqual(l.ListenersArray.Length, 2);

            l.Remove(null);
            Assert.AreEqual(l.ListenersArray.Length, 2);
            l.Remove(d3);
            Assert.AreEqual(l.ListenersArray.Length, 2);
            l.Remove(d1);
            Assert.AreEqual(l.ListenersArray.Length, 1);
            Assert.IsFalse(l.Contains(d1));
            Assert.IsTrue(l.Contains(d2));
            l.Remove(d1);
            Assert.AreEqual(l.ListenersArray.Length, 1);

            l.RemoveAll();
            Assert.IsTrue(l.IsEmpty);
            Assert.IsFalse(l.Contains(d2));

            DummySyncEventListener s1 = new DummySyncEventListener();

            l.Add(s1);
            Assert.IsTrue(l.Contains(s1));
            Assert.AreEqual(l.ListenersArray.Length, 1);
            l.Add(d1);
            Assert.AreEqual(l.ListenersArray.Length, 2);
            l.Remove(s1);
            Assert.IsTrue(l.Contains(d1));
            Assert.AreEqual(l.ListenersArray.Length, 1);
            l.Remove(d1);
            Assert.IsTrue(l.IsEmpty);
        }

        [Test]
        public void ListenersAddAllTest()
        {
            Listeners l1 = new Listeners();
            Listeners l2 = new Listeners();

            DummyEventListener d1 = new DummyEventListener();
            DummyEventListener d2 = new DummyEventListener();
            DummyEventListener d3 = new DummyEventListener();

            l2.Add(d2);
            l2.Add(d3);

            Assert.AreEqual(l1.ListenersArray.Length, 0);
            l1.AddAll(null);
            Assert.AreEqual(l1.ListenersArray.Length, 0);
            l1.AddAll(l2);
            Assert.AreEqual(l1.ListenersArray.Length, 2);
            Assert.AreEqual(l1.ListenersArray[0], l2.ListenersArray[0]);
            Assert.AreEqual(l1.ListenersArray[1], l2.ListenersArray[1]);

            l1.AddAll(l2);
            Assert.AreEqual(l1.ListenersArray.Length, 2);

            l1.RemoveAll();
            l1.Add(d1);
            l1.AddAll(l2);

            Assert.AreEqual(l1.ListenersArray.Length, 3);
            foreach (ICacheListener listener in l2.ListenersArray)
            {
                Assert.IsTrue(l1.Contains(listener));
            }

            Listeners l3 = new Listeners();
            Listeners l4 = new Listeners();

            DummySyncEventListener s1 = new DummySyncEventListener();
            DummySyncEventListener s2 = new DummySyncEventListener();

            l4.Add(s1);
            l4.Add(s2);
            l3.Add(d1);
            l3.AddAll(l4);

            Assert.AreEqual(l3.ListenersArray.Length, 3);
            foreach (ICacheListener listener in l4.ListenersArray)
            {
                Assert.IsTrue(l3.Contains(listener));
            }

            l1.AddAll(l3);
            Assert.AreEqual(l1.ListenersArray.Length, 5);
        }
    }

    /// <summary>
    /// Dummy implementation of ICacheListener.
    /// </summary>
    public class DummyEventListener : ICacheListener
    {
        public void EntryInserted(CacheEventArgs evt)
        {}

        public void EntryUpdated(CacheEventArgs evt)
        {}

        public void EntryDeleted(CacheEventArgs evt)
        {}
    }

    /// <summary>
    /// Dummy impolementation of ISynchronousListener
    /// </summary>
    public class DummySyncEventListener : ISynchronousListener, ICacheListener
    {
        public void EntryInserted(CacheEventArgs evt)
        {}

        public void EntryUpdated(CacheEventArgs evt)
        {}

        public void EntryDeleted(CacheEventArgs evt)
        {}
    }
}