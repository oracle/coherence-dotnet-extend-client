/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Threading;

using NUnit.Framework;

using Tangosol.Net.Cache;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;
using Tangosol.Util.Transformer;

namespace Tangosol.Net.Cache
    {
    /// <summary>
    /// Coherence*Extend test for map listeners receiving events.
    /// Tests:
    /// COH-8157 (Bug14778008)
    /// COH-8238 (Bug - none)
    /// COH-8578 (Bug16023459)
    ///
    /// Test the behavior of proxy returning events.
    /// COH-8157 reported multiple events received when there is
    /// both a key and a filter listener that should receive the
    /// single event. Test cases;
    /// - one key listener
    /// - one filter listener
    /// - multiple key listeners
    /// - multiple filter listeners
    /// - one key, one filter listener
    /// - one key, multiple filter
    /// - multiple key, one filter
    /// - multiple key, multiple filter
    /// - regression test for COH-9355 use case
    /// </summary>
    /// <author> par  2012.11.2 </author>
    [TestFixture]
    public class ProxyEventTests
        {
        #region Properties

        /// <summary>
        /// Obtain the number of all update events.
        /// </summary>
        static public int AtomicAllUpdate
            {
            get { return atomicAllUpdate; }
            }

        /// <summary>
        /// Obtain the number of all insert events.
        /// </summary>
        static public int AtomicAllInsert
            {
            get { return atomicAllInsert; }
            }

        /// <summary>
        /// Obtain the number of all delete events.
        /// </summary>
        static public int AtomicAllDelete
            {
            get { return atomicAllDelete; }
            }

        /// <summary>
        /// Obtain the number of all lite update events.
        /// </summary>
        static public int AtomicAllLiteUpdate
            {
            get { return atomicAllLiteUpdate; }
            }

        /// <summary>
        /// Obtain the number of all lite insert events.
        /// </summary>
        static public int AtomicAllLiteInsert
            {
            get { return atomicAllLiteInsert; }
            }

        /// <summary>
        /// Obtain the number of all lite delete events.
        /// </summary>
        static public int AtomicAllLiteDelete
            {
            get { return atomicAllLiteDelete; }
            }

        /// <summary>
        /// Obtain the number of key update events.
        /// </summary>
        static public int AtomicKeyUpdate
            {
            get { return atomicKeyUpdate; }
            }

        /// <summary>
        /// Obtain the number of key insert events.
        /// </summary>
        static public int AtomicKeyInsert
            {
            get { return atomicKeyInsert; }
            }

        /// <summary>
        /// Obtain the number of key delete events.
        /// </summary>
        static public int AtomicKeyDelete
            {
            get { return atomicKeyDelete; }
            }

        /// <summary>
        /// Obtain the number of key lite update events.
        /// </summary>
        static public int AtomicKeyLiteUpdate
            {
            get { return atomicKeyLiteUpdate; }
            }

        /// <summary>
        /// Obtain the number of key lite insert events.
        /// </summary>
        static public int AtomicKeyLiteInsert
            {
            get { return atomicKeyLiteInsert; }
            }

        /// <summary>
        /// Obtain the number of key lite delte events.
        /// </summary>
        static public int AtomicKeyLiteDelete
            {
            get { return atomicKeyLiteDelete; }
            }

        /// <summary>
        /// Obtain the number of filter update events.
        /// </summary>
        static public int AtomicFilterUpdate
            {
            get { return atomicFilterUpdate; }
            }

        /// <summary>
        /// Obtain the number of filter insert events.
        /// </summary>
        static public int AtomicFilterInsert
            {
            get { return atomicFilterInsert; }
            }

        /// <summary>
        /// Obtain the number of filter delete events.
        /// </summary>
        static public int AtomicFilterDelete
            {
            get { return atomicFilterDelete; }
            }

        /// <summary>
        /// Obtain the number of filter lite update events.
        /// </summary>
        static public int AtomicFilterLiteUpdate
            {
            get { return atomicFilterLiteUpdate; }
            }

        /// <summary>
        /// Obtain the number of filter lite insert events.
        /// </summary>
        static public int AtomicFilterLiteInsert
            {
            get { return atomicFilterLiteInsert; }
            }

        /// <summary>
        /// Obtain the number of filter lite delete events.
        /// </summary>
        static public int AtomicFilterLiteDelete
            {
            get { return atomicFilterLiteDelete; }
            }

        /// <summary>
        /// Obtain the number of transformed filter update events.
        /// </summary>
        static public int AtomicTransformFilterUpdate
            {
            get { return atomicTransformFilterUpdate; }
            }

        /// <summary>
        /// Obtain the number of transformed filter insert events.
        /// </summary>
        static public int AtomicTransformFilterInsert
            {
            get { return atomicTransformFilterInsert; }
            }

        /// <summary>
        /// Obtain the number of transformed filter delete events.
        /// </summary>
        static public int AtomicTransformFilterDelete
            {
            get { return atomicTransformFilterDelete; }
            }
        /// <summary>
        /// Obtain the number of transformed filter lite update events.
        /// </summary>
        static public int AtomicTransformFilterLiteUpdate
            {
            get { return atomicTransformFilterLiteUpdate; }
            }

        /// <summary>
        /// Obtain the number of transformed filter lite insert events.
        /// </summary>
        static public int AtomicTransformFilterLiteInsert
            {
            get { return atomicTransformFilterLiteInsert; }
            }

        /// <summary>
        /// Obtain the number of transformed filter lite delete events.
        /// </summary>
        static public int AtomicTransformFilterLiteDelete
            {
            get { return atomicTransformFilterLiteDelete; }
            }

        #endregion

        private INamedCache GetTestCache()
            {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            ccf.Config          = config;

            INamedCache cache = CacheFactory.GetCache("dist-test");
            cache.Clear();
            return cache;
            }

        /// <summary>
        /// Test events returned when one key listener is configured.
        /// </summary>
        [Test]
        public void TestOneKeyListener()
            {
            INamedCache cache = GetTestCache();

            TestListener keyListener = new TestListener(1, "KEY");
            cache.AddCacheListener(keyListener, "TestKey1", false);

            // wait for event that reports listeners have been configured
            WaitForEvents(cache);

            // check how many were received
            Assert.AreEqual(keyListener.InsertCount, keyListener.InsertExpected);
            Assert.AreEqual(keyListener.UpdateCount, keyListener.UpdateExpected);
            Assert.AreEqual(keyListener.DeleteCount, keyListener.DeleteExpected);

            cache.RemoveCacheListener(keyListener);
            }

        /// <summary>
        /// Test events returned when one filter listener is configured.
        /// </summary>
        [Test]
        public void TestOneFilterListener()
            {
            INamedCache cache = GetTestCache();

            TestListener filterListener = new TestListener(SOME_EVENTS, "FILTER");

            IFilter filter = new CacheEventFilter(CacheEventFilter.CacheEventMask.All, 
                    new LessFilter(IdentityExtractor.Instance, SOME_EVENTS));

            cache.AddCacheListener(filterListener, filter, false);

            // wait for event that reports listeners have been configured
            WaitForEvents(cache);

            // check how many were received
            Assert.AreEqual(filterListener.InsertCount, filterListener.InsertExpected);
            Assert.AreEqual(filterListener.UpdateCount, filterListener.UpdateExpected);
            Assert.AreEqual(filterListener.DeleteCount, filterListener.DeleteExpected);

            cache.RemoveCacheListener(filterListener, filter);
            }

        ///
        /// Test events returned when more than one key listener is configured.
        ///
        [Test]
        public void TestMultipleKeyListeners()
            {
            INamedCache cache = GetTestCache();

            TestListener keyListener  = new TestListener(1, "KEY");
            TestListener keyListener2 = new TestListener(1, "KEY2");

            cache.AddCacheListener(keyListener,  "TestKey1", false);
            cache.AddCacheListener(keyListener2, "TestKey2", false);

            // wait for event that reports listeners have been configured
            WaitForEvents(cache);

            // check how many were received
            Assert.AreEqual(keyListener.InsertCount, keyListener.InsertExpected);
            Assert.AreEqual(keyListener.UpdateCount, keyListener.UpdateExpected);
            Assert.AreEqual(keyListener.DeleteCount, keyListener.DeleteExpected);

            Assert.AreEqual(keyListener2.InsertCount, keyListener2.InsertExpected);
            Assert.AreEqual(keyListener2.UpdateCount, keyListener2.UpdateExpected);
            Assert.AreEqual(keyListener2.DeleteCount, keyListener2.DeleteExpected);

            cache.RemoveCacheListener(keyListener);
            cache.RemoveCacheListener(keyListener2);
            }

        ///
        ///Test events returned when more than one filter listener is configured.
        ///
        [Test]
        public void TestMultipleFilterListeners()
            {
            INamedCache cache = GetTestCache();

            TestListener filterListener  = new TestListener(SOME_EVENTS, "FILTER");
            TestListener filterListener2 = new TestListener(1,           "FILTER2");

            IFilter filter  = new CacheEventFilter(CacheEventFilter.CacheEventMask.All, 
                    new LessFilter(IdentityExtractor.Instance, SOME_EVENTS));
            IFilter filter2 = new CacheEventFilter(CacheEventFilter.CacheEventMask.All, 
                    new LessFilter(IdentityExtractor.Instance, 1));

            cache.AddCacheListener(filterListener,  filter,  false);
            cache.AddCacheListener(filterListener2, filter2, false);

            // wait for event that reports listeners have been configured
            WaitForEvents(cache);

            // check how many were received
            Assert.AreEqual(filterListener.InsertCount, filterListener.InsertExpected);
            Assert.AreEqual(filterListener.UpdateCount, filterListener.UpdateExpected);
            Assert.AreEqual(filterListener.DeleteCount, filterListener.DeleteExpected);

            Assert.AreEqual(filterListener2.InsertCount, filterListener2.InsertExpected);
            Assert.AreEqual(filterListener2.UpdateCount, filterListener2.UpdateExpected);
            Assert.AreEqual(filterListener2.DeleteCount, filterListener2.DeleteExpected);

            cache.RemoveCacheListener(filterListener,  filter);
            cache.RemoveCacheListener(filterListener2, filter2);
            }

        ///
        /// Test events returned when one key listener and one filter listener are configured.
        ///
        [Test]
        public void TestOneKeyOneFilterListeners()
            {
            INamedCache cache = GetTestCache();

            TestListener keyListener    = new TestListener(1,           "KEY");
            TestListener filterListener = new TestListener(SOME_EVENTS, "FILTER");

            IFilter filter  = new CacheEventFilter(CacheEventFilter.CacheEventMask.All, 
                    new LessFilter(IdentityExtractor.Instance, SOME_EVENTS));

            cache.AddCacheListener(keyListener,    "TestKey1", false);
            cache.AddCacheListener(filterListener, filter,     false);

            // wait for event that reports listeners have been configured
            WaitForEvents(cache);

            // check how many were received
            Assert.AreEqual(keyListener.InsertCount, keyListener.InsertExpected);
            Assert.AreEqual(keyListener.UpdateCount, keyListener.UpdateExpected);
            Assert.AreEqual(keyListener.DeleteCount, keyListener.DeleteExpected);

            Assert.AreEqual(filterListener.InsertCount, filterListener.InsertExpected);
            Assert.AreEqual(filterListener.UpdateCount, filterListener.UpdateExpected);
            Assert.AreEqual(filterListener.DeleteCount, filterListener.DeleteExpected);

            cache.RemoveCacheListener(keyListener);
            cache.RemoveCacheListener(filterListener, filter);
            }

        ///
        /// Test events returned when one key listener and multiple filter listeners are configured.
        ///
        [Test]
        public void TestOneKeyMultipleFilterListeners()
            {
            INamedCache cache = GetTestCache();

            TestListener keyListener     = new TestListener(1,           "KEY");
            TestListener filterListener  = new TestListener(SOME_EVENTS, "FILTER");
            TestListener filterListener2 = new TestListener(1,           "FILTER2");

            IFilter filter  = new CacheEventFilter(CacheEventFilter.CacheEventMask.All, 
                    new LessFilter(IdentityExtractor.Instance, SOME_EVENTS));
            IFilter filter2 = new CacheEventFilter(CacheEventFilter.CacheEventMask.All, 
                    new LessFilter(IdentityExtractor.Instance, 1));

            cache.AddCacheListener(keyListener,     "TestKey1",   false);
            cache.AddCacheListener(filterListener,  filter,       false);
            cache.AddCacheListener(filterListener2, filter2,      false);

            // wait for event that reports listeners have been configured
            WaitForEvents(cache);

            // check how many were received
            Assert.AreEqual(keyListener.InsertCount, keyListener.InsertExpected);
            Assert.AreEqual(keyListener.UpdateCount, keyListener.UpdateExpected);
            Assert.AreEqual(keyListener.DeleteCount, keyListener.DeleteExpected);

            Assert.AreEqual(filterListener.InsertCount, filterListener.InsertExpected);
            Assert.AreEqual(filterListener.UpdateCount, filterListener.UpdateExpected);
            Assert.AreEqual(filterListener.DeleteCount, filterListener.DeleteExpected);

            Assert.AreEqual(filterListener2.InsertCount, filterListener2.InsertExpected);
            Assert.AreEqual(filterListener2.UpdateCount, filterListener2.UpdateExpected);
            Assert.AreEqual(filterListener2.DeleteCount, filterListener2.DeleteExpected);

            cache.RemoveCacheListener(keyListener);
            cache.RemoveCacheListener(filterListener,  filter);
            cache.RemoveCacheListener(filterListener2, filter2);
            }

        ///
        /// Test events returned when multiple key listeners and one filter listener are configured.
        ///
        [Test]
        public void TestMultipleKeyOneFilterListeners()
            {
            INamedCache cache = GetTestCache();

            TestListener keyListener    = new TestListener(1,           "KEY");
            TestListener keyListener2   = new TestListener(1,           "KEY2");
            TestListener filterListener = new TestListener(SOME_EVENTS, "FILTER");

            IFilter filter  = new CacheEventFilter(CacheEventFilter.CacheEventMask.All, 
                    new LessFilter(IdentityExtractor.Instance, SOME_EVENTS));

            cache.AddCacheListener(keyListener,    "TestKey1",  false);
            cache.AddCacheListener(keyListener2,   "TestKey2",  false);
            cache.AddCacheListener(filterListener, filter,      false);

            // wait for event that reports listeners have been configured
            WaitForEvents(cache);

            // check how many were received
            Assert.AreEqual(keyListener.InsertCount, keyListener.InsertExpected);
            Assert.AreEqual(keyListener.UpdateCount, keyListener.UpdateExpected);
            Assert.AreEqual(keyListener.DeleteCount, keyListener.DeleteExpected);

            Assert.AreEqual(keyListener2.InsertCount, keyListener2.InsertExpected);
            Assert.AreEqual(keyListener2.UpdateCount, keyListener2.UpdateExpected);
            Assert.AreEqual(keyListener2.DeleteCount, keyListener2.DeleteExpected);

            Assert.AreEqual(filterListener.InsertCount, filterListener.InsertExpected);
            Assert.AreEqual(filterListener.UpdateCount, filterListener.UpdateExpected);
            Assert.AreEqual(filterListener.DeleteCount, filterListener.DeleteExpected);

            cache.RemoveCacheListener(keyListener);
            cache.RemoveCacheListener(keyListener2);
            cache.RemoveCacheListener(filterListener, filter);
            }

        ///
        /// Test events returned when multiple key and filter listeners are configured.
        ///
        [Test]
        public void TestMultipleKeyMultipleFilterListeners()
            {
            INamedCache cache = GetTestCache();

            TestListener keyListener     = new TestListener(1,           "KEY");
            TestListener keyListener2    = new TestListener(1,           "KEY2");
            TestListener filterListener  = new TestListener(SOME_EVENTS, "FILTER");
            TestListener filterListener2 = new TestListener(1,           "FILTER2");

            IFilter filter  = new CacheEventFilter(CacheEventFilter.CacheEventMask.All, 
                    new LessFilter(IdentityExtractor.Instance, SOME_EVENTS));
            IFilter filter2 = new CacheEventFilter(CacheEventFilter.CacheEventMask.All, 
                    new LessFilter(IdentityExtractor.Instance, 1));

            cache.AddCacheListener(keyListener,     "TestKey1",   false);
            cache.AddCacheListener(keyListener2,    "TestKey2",   false);
            cache.AddCacheListener(filterListener,  filter,  false);
            cache.AddCacheListener(filterListener2, filter2, false);

            // wait for event that reports listeners have been configured
            WaitForEvents(cache);

            // check how many were received
            Assert.AreEqual(keyListener.InsertCount, keyListener.InsertExpected);
            Assert.AreEqual(keyListener.UpdateCount, keyListener.UpdateExpected);
            Assert.AreEqual(keyListener.DeleteCount, keyListener.DeleteExpected);

            Assert.AreEqual(keyListener2.InsertCount, keyListener2.InsertExpected);
            Assert.AreEqual(keyListener2.UpdateCount, keyListener2.UpdateExpected);
            Assert.AreEqual(keyListener2.DeleteCount, keyListener2.DeleteExpected);

            Assert.AreEqual(filterListener.InsertCount, filterListener.InsertExpected);
            Assert.AreEqual(filterListener.UpdateCount, filterListener.UpdateExpected);
            Assert.AreEqual(filterListener.DeleteCount, filterListener.DeleteExpected);

            Assert.AreEqual(filterListener2.InsertCount, filterListener2.InsertExpected);
            Assert.AreEqual(filterListener2.UpdateCount, filterListener2.UpdateExpected);
            Assert.AreEqual(filterListener2.DeleteCount, filterListener2.DeleteExpected);

            cache.RemoveCacheListener(keyListener);
            cache.RemoveCacheListener(keyListener2);
            cache.RemoveCacheListener(filterListener,  filter);
            cache.RemoveCacheListener(filterListener2, filter2);
            }

        ///
        /// Test that cache truncate does not remove configured listeners. 
        ///
        [Test]
        public void TestListenersAfterTruncate()
        {
            INamedCache cache = GetTestCache();

            TestListener keyListener = new TestListener(10, "KEY");

            cache.AddCacheListener(keyListener, new CacheEventFilter(CacheEventFilter.CacheEventMask.All), false);

            // wait for event that reports listeners have been configured
            GenerateEvents(cache, "Truncate", 10);
            Thread.Sleep(2000);

            Assert.AreEqual(keyListener.InsertCount, keyListener.InsertExpected);
            Assert.AreEqual(keyListener.UpdateCount, keyListener.UpdateExpected);
            Assert.AreEqual(keyListener.DeleteCount, keyListener.DeleteExpected);

            cache.Truncate();

            GenerateEvents(cache, "Truncate1", 10);
            Thread.Sleep(2000);

            Assert.AreEqual(keyListener.InsertCount, keyListener.InsertExpected + 10);
            Assert.AreEqual(keyListener.UpdateCount, keyListener.UpdateExpected + 10);
            Assert.AreEqual(keyListener.DeleteCount, keyListener.DeleteExpected + 10);
        }

        ///
        /// Regression test for COH-9355 event delivery semantics.
        ///
        [Test]
        public void TestCoh9355()
            {
            INamedCache cache = GetTestCache();
            cache.AddCacheListener(new TestAllListener(),     AlwaysFilter.Instance, false);
            cache.AddCacheListener(new TestAllLiteListener(), AlwaysFilter.Instance, true);
            cache.AddCacheListener(new TestKeyListener(),     "Key1",                false);
            cache.AddCacheListener(new TestKeyLiteListener(), "Key1",                true);

            cache.AddCacheListener(new TestFilterListener(),
                new CacheEventFilter(CacheEventFilter.CacheEventMask.All,
                          new LessFilter(IdentityExtractor.Instance, (Int32) 50)), false);

            cache.AddCacheListener(new TestFilterLiteListener(),
                new CacheEventFilter(CacheEventFilter.CacheEventMask.All,
                          new LessFilter(IdentityExtractor.Instance, (Int32) 50)), true);

            cache.AddCacheListener(new TestTransformFilterListener(),
                new CacheEventTransformerFilter(
                    new CacheEventFilter(CacheEventFilter.CacheEventMask.All,
                                   new LessFilter(IdentityExtractor.Instance, (Int32) 50)),
                    SemiLiteEventTransformer.Instance), false);

            cache.AddCacheListener(new TestTransformFilterLiteListener(),
                new CacheEventTransformerFilter(
                    new CacheEventFilter(CacheEventFilter.CacheEventMask.All,
                                   new LessFilter(IdentityExtractor.Instance, (Int32) 50)),
                    SemiLiteEventTransformer.Instance), true);

            GenerateEvents(cache, "Key", 100);
            Thread.Sleep(1000);

            Assert.AreEqual(100, AtomicAllInsert);
            Assert.AreEqual(100, AtomicAllUpdate);
            Assert.AreEqual(100, AtomicAllDelete);

            Assert.AreEqual(100, AtomicAllLiteInsert);
            Assert.AreEqual(100, AtomicAllLiteUpdate);
            Assert.AreEqual(100, AtomicAllLiteDelete);

            Assert.AreEqual(1, AtomicKeyInsert);
            Assert.AreEqual(1, AtomicKeyUpdate);
            Assert.AreEqual(1, AtomicKeyDelete);

            Assert.AreEqual(1, AtomicKeyLiteInsert);
            Assert.AreEqual(1, AtomicKeyLiteUpdate);
            Assert.AreEqual(1, AtomicKeyLiteDelete);

            Assert.AreEqual(50, AtomicFilterInsert);
            Assert.AreEqual(50, AtomicFilterUpdate);
            Assert.AreEqual(50, AtomicFilterDelete);

            Assert.AreEqual(50, AtomicFilterLiteInsert);
            Assert.AreEqual(50, AtomicFilterLiteUpdate);
            Assert.AreEqual(50, AtomicFilterLiteDelete);

            Assert.AreEqual(50, AtomicTransformFilterInsert);
            Assert.AreEqual(50, AtomicTransformFilterUpdate);
            Assert.AreEqual(50, AtomicTransformFilterDelete);

            Assert.AreEqual(50, AtomicTransformFilterLiteInsert);
            Assert.AreEqual(50, AtomicTransformFilterLiteUpdate);
            Assert.AreEqual(50, AtomicTransformFilterLiteDelete);
            }

        ///
        /// Test for COH-11015.
        ///
        [Test]
        public void TestCoh11015()
        {
            INamedCache cache = GetTestCache();

            cache.Add(1, new TestObject(1, "AAAAA"));

            // create Listener 1 and add to cache 
            TestListener listener1 = new TestListener(2, "COH-11015; Listener 1");
            IFilter      filter1   = new EqualsFilter(new PofExtractor(null, 1), "AAAAA");

            cache.AddCacheListener(listener1, new CacheEventFilter(CacheEventFilter.CacheEventMask.All, filter1), false); 

            // update entry in the cache and one listener is active 
            cache.Add(1, new TestObject(1, "AAAAA")); 
            DateTime endTime = DateTime.Now.AddSeconds(30);
            while (listener1.UpdateCount < 1 && (DateTime.Now < endTime))
            {
                Thread.Sleep(250);
            }
            Assert.AreEqual(listener1.UpdateCount, 1);

            // create Listener 2 and add to cache 
            TestListener listener2 = new TestListener(1, "Listener 2"); 
            IFilter      filter2   = new EqualsFilter(new PofExtractor(null, 1), "AAAAA"); 

            cache.AddCacheListener(listener2, new CacheEventFilter(CacheEventFilter.CacheEventMask.All, filter2), false); 

            cache.Add(1, new TestObject(1, "AAAAA")); 
            endTime = DateTime.Now.AddSeconds(30);
            while (listener1.UpdateCount < 2 && (DateTime.Now < endTime))
            {
                Thread.Sleep(250);
            }
            Assert.AreEqual(listener1.UpdateCount, 2);
            Assert.AreEqual(listener2.UpdateCount, 1);
 
            cache.RemoveCacheListener(listener1, filter1);
            cache.RemoveCacheListener(listener2, filter2);
        }

            // ----- helper methods -------------------------------------------------

            #region helper methods

            /// <summary>
            /// Wait to receive a specific event or a timeout, whichever comes first.
            /// </summary>
            /// <param name="cache">
            /// Test cache in which to add data that will cause generation of the event
            /// </param>
            protected void WaitForEvents(INamedCache cache)
                {
                WaitListener waitListener = new WaitListener(10000L, "WAIT");
                cache.AddCacheListener(waitListener, "WaitKey", false);
                waitListener.StartWait();

                cache.Insert("WaitKey", SOME_EVENTS+1);
                while (!waitListener.EventReceived)
                    {
                    Assert.IsFalse(waitListener.GetTimedOut());
                    Thread.Sleep(250);
                    }

                cache.RemoveCacheListener(waitListener);

                // generate events
                GenerateEvents(cache, "TestKey", SOME_DATA);

                waitListener.Reset(10000L);
                cache.AddCacheListener(waitListener, "WaitKey", false);
                waitListener.StartWait();
                cache.Insert("WaitKey", SOME_EVENTS+1);
                while (!waitListener.EventReceived)
                    {
                    Assert.IsFalse(waitListener.GetTimedOut());
                    Thread.Sleep(250);
                    }

                cache.RemoveCacheListener(waitListener);
                }

            /// <summary>
            /// Add data to the cache, so as to generate events.
            /// </summary>
            /// <param name="cache">
            ///  Cache in which to add data that will cause generation of events
            /// </param>
            protected void GenerateEvents(INamedCache cache, String key, int number)
                {
                // insert events
                for (int i = 0; i < number; i++)
                    {
                    cache.Insert(key + i, i);
                    }

                // update events
                for (int i = 0; i < number; i++)
                    {
                    cache.Insert(key + i, i);
                    }

                // delete events
                for (int i = 0; i < number; i++)
                    {
                   cache.Remove(key + i);
                    }
                }

            #endregion

            // ----- constants ------------------------------------------------------

            #region constants

            ///
            /// Number of events 
            ///
            public static int SOME_EVENTS = 5;

            ///
            /// Number of data items to put in cache 
            ///
            public static int SOME_DATA = 10;

            #endregion

            // ----- data members ---------------------------------------------------

            #region data members

            ///
            /// Number of all update events 
            ///
            private static int atomicAllUpdate = 0;

            ///
            /// Number of all insert events 
            ///
            private static  int atomicAllInsert = 0;

            ///
            /// Number of all delete events 
            ///
            private static  int atomicAllDelete = 0;

            ///
            /// Number of all lite update events 
            ///
            private static int atomicAllLiteUpdate = 0;

            ///
            /// Number of all lite insert events 
            ///
            private static  int atomicAllLiteInsert = 0;

            ///
            /// Number of all lite delete events 
            ///
            private static  int atomicAllLiteDelete = 0;

            ///
            /// Number of key update events 
            ///
            private static int atomicKeyUpdate = 0;

            ///
            /// Number of key insert events 
            ///
            private static int atomicKeyInsert = 0;

            ///
            /// Number of key delete events 
            ///
            private static int atomicKeyDelete = 0;

            ///
            /// Number of key lite update events 
            ///
            private static int atomicKeyLiteUpdate = 0;

            ///
            /// Number of key lite insert events 
            ///
            private static int atomicKeyLiteInsert = 0;

            ///
            /// Number of key lite delete events
            ///
            private static int atomicKeyLiteDelete = 0;

            ///
            /// Number of filter update events 
            ///
            private static int atomicFilterUpdate = 0;

            ///
            /// Number of filter insert events 
            ///
            private static int atomicFilterInsert = 0;

            ///
            /// Number of filter delete events 
            ///
            private static int atomicFilterDelete = 0;

            ///
            /// Number of filter lite update events 
            ///
            private static int atomicFilterLiteUpdate = 0;

            ///
            /// Number of filter lite insert events 
            ///
            private static int atomicFilterLiteInsert = 0;

            ///
            /// Number of filter lite delete events 
            ///
            private static int atomicFilterLiteDelete = 0;

            ///
            /// Number of transformed filter update events 
            ///
            private static  int atomicTransformFilterUpdate = 0;

            ///
            /// Number of tranformed filter insert events 
            ///
            private static  int atomicTransformFilterInsert = 0;

            ///
            /// Number of transformed filter delete events 
            ///
            private static  int atomicTransformFilterDelete = 0;

            ///
            /// Number of transformed filter lite update events 
            ///
            private static  int atomicTransformFilterLiteUpdate = 0;

            ///
            /// Number of tranformed filter lite insert events 
            ///
            private static  int atomicTransformFilterLiteInsert = 0;

            ///
            /// Number of transformed filter lite delete events 
            ///
            private static  int atomicTransformFilterLiteDelete = 0;

            #endregion

            // ----- inner class: TestListener --------------------------------------

            /// <summary>
            /// Custom listener that keeps track of how many events are received by
            /// this listener during a test.
            /// </summary>
            #region Inner class: TestListener
            public class TestListener : ICacheListener
                {
                /// <summary>
                /// Constructor for listener.
                /// </summary>
                /// <param name="expected">  
                /// Number of events expected to be received
                /// </param>
                /// <param name="listener">
                /// Name of this listener
                /// </param>
                public TestListener(int expected, String listener)
                    {
                    m_cExpected = expected;
                    m_cUpdates  = 0;
                    m_cInserts  = 0;
                    m_cDeletes  = 0;
                    m_sListener = listener;
                    }

                /// <summary>
                /// Receive update events.
                /// </summary>
                /// <param name="evt">
                /// Received event
                /// <param>
                public void EntryUpdated(CacheEventArgs evt)
                    {
                    m_cUpdates++;
                    }

                /// <summary>
                /// Receive insert events.
                /// </summary>
                /// <param name="evt"
                /// Received event
                /// </param>
                public void EntryInserted(CacheEventArgs evt)
                    {
                    m_cInserts++;
                    }

                /// <summary>
                /// Receive delete events.
                /// </summary>
                /// <param name="evt"
                /// Received event
                /// </param>
                public void EntryDeleted(CacheEventArgs evt)
                    {
                    m_cDeletes++;
                    }

                /// <summary>
                /// Return number of update events received.
                /// </summary>
                /// <returns>
                /// Number of events received
                /// </returns>
                public int UpdateCount
                    {
                    get { return m_cUpdates; }
                    set { m_cUpdates = value; }
                    }

                /// <summary>
                /// Return number of insert events received.
                /// </summary>
                /// <returns>
                /// Number of events received
                /// </returns>
                public int InsertCount
                    {
                    get { return m_cInserts; }
                    set { m_cInserts = value; }
                    }

                /// <summary>
                /// Return number of delete events received.
                /// </summary>
                /// <returns>
                /// Number of events received
                /// </returns>
                public int DeleteCount
                    {
                    get { return m_cDeletes; }
                    set { m_cDeletes = value; }
                    }

                /// <summary>
                /// Return number of update events expected.
                /// </summary>
                /// <returns>  
                /// Number of events expected
                /// </returns>
                public int UpdateExpected
                    {
                    get { return m_cExpected; }
                    set { m_cExpected = value; }
                    }

                /// <summary>
                /// Return number of insert events expected.
                /// </summary>
                /// <returns>
                /// Number of events expected
                /// </returns>
                public int InsertExpected
                    {
                    get { return m_cExpected; }
                    set { m_cExpected = value; }
                    }

                /// <summary>
                /// Return number of delete events expected.
                /// </summary>
                /// <returns>
                /// Number of events expected
                /// </returns>
                public int DeleteExpected
                    {
                    get { return m_cExpected; }
                    set { m_cExpected = value; }
                    }

                // ----- data members -----------------------------------------------

                #region data members

                /// <summary>
                /// Number of insert events received
                /// </summary>
                int m_cInserts;

                /// <summary>
                /// Number of update events received
                /// </summary>
                int m_cUpdates;

                /// <summary>
                /// Number of delete events received
                /// </summary>
                int m_cDeletes;

                /// <summary>
                /// Number of events expected
                /// </summary>
                int m_cExpected;

                /// <summary>
                /// Which listener is this, KEY or FILTER
                /// </summary>
                String m_sListener;

                #endregion
            }
            #endregion

            // ----- inner class: WaitListener --------------------------------------

            #region Inner class: WaitListener

            /// <summary>
            /// Custom listener that waits until the listeners have been added to the
            /// cache.  Stops waiting when it receives its event or it times out,
            /// whichever comes first.
            /// </summary>
            public class WaitListener : ICacheListener
                {
                /// <summary>
                /// Constructor for the listener.
                /// </summary>
                /// <param name="timeout">
                /// Length of time in milliseconds to wait for event
                /// </param>
                /// <param name="listener">
                /// Name of this listener
                /// </param>
                public WaitListener(long timeout, String listener)
                {
                    m_cTimeout       = timeout;
                    m_sListener      = listener;
                    m_fEventReceived = false;
                    m_cEndTime       = DateTime.Now;
                }

                /// <summary>
                /// Has event been received?
                /// </summary>
                /// <returns>
                ///  True if listener has timed out, false if not
                /// </returns>
                public bool EventReceived
                    {
                    get { return m_fEventReceived; }
                    set { m_fEventReceived = value; }
                    }

                /// <summary>
                /// Receive update event.
                /// </summary>
                /// <param name="evt">
                /// Update event received
                /// </param>
                public void EntryUpdated(CacheEventArgs evt)
                {
                    m_fEventReceived = true;
                }

                /// <summary>
                /// Receive insert event.
                /// </summary>
                /// <param name="evt">
                /// Insert event received
                /// </param>
                public void EntryInserted(CacheEventArgs evt)
                {
                    m_fEventReceived = true;
                }

                /// <summary>
                /// Receive delete event.
                /// </summary>
                /// <param name="evt">
                /// Delete event received
                /// </param>
                public void EntryDeleted(CacheEventArgs evt)
                {
                    m_fEventReceived = true;
                }

                /// <summary>
                /// Has listener timed out waiting for event?
                /// </summary>
                /// <returns>
                /// True if listener has timed out, false if not
                /// </returns>
                public bool GetTimedOut()
                    {
                    return (DateTime.Now > m_cEndTime);
                    }

                /// <summary>
                /// Start the listener waiting for the event.
                /// </summary>
                public void StartWait()
                    {
                    m_cEndTime = DateTime.Now.AddSeconds(m_cTimeout);
                    }

                /// <summary>
                /// Reset the listener.
                /// </summary>
                /// <param name="timeout">
                /// How long this listener should wait before timing out on next wait iteration
                /// </param>
                public void Reset(long timeout)
                    {
                    m_cEndTime       = DateTime.Now;
                    m_cTimeout       = timeout;
                    m_fEventReceived = false;
                    }

                // ----- data members -----------------------------------------------

                #region data members

                /// <summary>
                /// Events received
                /// </summary>
                private bool m_fEventReceived;

                /// <summary>
                /// Length of timeout
                /// </summary>
                private long m_cTimeout;

                /// <summary>
                /// Wait end time
                /// </summary>
                private DateTime m_cEndTime;

                /// <summary>
                /// Which listener is this, WAIT
                /// </summary>
                private String m_sListener;

                #endregion
            }
            #endregion

            // ----- inner class: TestAllListener --------------------------------------

            #region Inner class: TestAllListener

            /// <summary>
            /// Custom listener that counts all events.
            /// </summary>
            public class TestAllListener : ICacheListener
                {
                /// <summary>
                /// Constructor for the listener.
                /// </summary>
                public TestAllListener()
                {
                }

                /// <summary>
                /// Receive update event.
                /// </summary>
                /// <param name="evt">
                /// Update event received
                /// </param>
                public void EntryUpdated(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicAllUpdate);
                }

                /// <summary>
                /// Receive insert event.
                /// </summary>
                /// <param name="evt">
                /// Insert event received
                /// </param>
                public void EntryInserted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicAllInsert);
                }

                /// <summary>
                /// Receive delete event.
                /// </summary>
                /// <param name="evt">
                /// Delete event received
                /// </param>
                public void EntryDeleted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicAllDelete);
                }
            }

            #endregion

            // ----- inner class: TestAllLiteListener --------------------------------------

            #region Inner class: TestAllLiteListener

            /// <summary>
            /// Custom listener that counts all lite events.
            /// </summary>
            public class TestAllLiteListener : ICacheListener
                {
                /// <summary>
                /// Constructor for the listener.
                /// </summary>
                public TestAllLiteListener()
                {
                }

                /// <summary>
                /// Receive update event.
                /// </summary>
                /// <param name="evt">
                /// Update event received
                /// </param>
                public void EntryUpdated(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicAllLiteUpdate);
                }

                /// <summary>
                /// Receive insert event.
                /// </summary>
                /// <param name="evt">
                /// Insert event received
                /// </param>
                public void EntryInserted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicAllLiteInsert);
                }

                /// <summary>
                /// Receive delete event.
                /// </summary>
                /// <param name="evt">
                /// Delete event received
                /// </param>
                public void EntryDeleted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicAllLiteDelete);
                }
            }

            #endregion

            // ----- inner class: TestKeyListener --------------------------------------

            #region Inner class: TestKeyListener

            /// <summary>
            /// Custom listener that counts key events.
            /// </summary>
            public class TestKeyListener : ICacheListener
                {
                /// <summary>
                /// Constructor for the listener.
                /// </summary>
                public TestKeyListener()
                {
                }

                /// <summary>
                /// Receive update event.
                /// </summary>
                /// <param name="evt">
                /// Update event received
                /// </param>
                public void EntryUpdated(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicKeyUpdate);
                }

                /// <summary>
                /// Receive insert event.
                /// </summary>
                /// <param name="evt">
                /// Insert event received
                /// </param>
                public void EntryInserted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicKeyInsert);
                }

                /// <summary>
                /// Receive delete event.
                /// </summary>
                /// <param name="evt">
                /// Delete event received
                /// </param>
                public void EntryDeleted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicKeyDelete);
                }
            }

            #endregion

            // ----- inner class: TestKeyLiteListener --------------------------------------

            #region Inner class: TestKeyLiteListener

            /// <summary>
            /// Custom listener that counts key lite events.
            /// </summary>
            public class TestKeyLiteListener : ICacheListener
                {
                /// <summary>
                /// Constructor for the listener.
                /// </summary>
                public TestKeyLiteListener()
                {
                }

                /// <summary>
                /// Receive update event.
                /// </summary>
                /// <param name="evt">
                /// Update event received
                /// </param>
                public void EntryUpdated(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicKeyLiteUpdate);
                }

                /// <summary>
                /// Receive insert event.
                /// </summary>
                /// <param name="evt">
                /// Insert event received
                /// </param>
                public void EntryInserted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicKeyLiteInsert);
                }

                /// <summary>
                /// Receive delete event.
                /// </summary>
                /// <param name="evt">
                /// Delete event received
                /// </param>
                public void EntryDeleted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicKeyLiteDelete);
                }
            }

            #endregion

            // ----- inner class: TestFilterListener --------------------------------------

            #region Inner class: TestFilterListener

            /// <summary>
            /// Custom listener that counts filter events.
            /// </summary>
            public class TestFilterListener : ICacheListener
                {
                /// <summary>
                /// Constructor for the listener.
                /// </summary>
                public TestFilterListener()
                {
                }

                /// <summary>
                /// Receive update event.
                /// </summary>
                /// <param name="evt">
                /// Update event received
                /// </param>
                public void EntryUpdated(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicFilterUpdate);
                }

                /// <summary>
                /// Receive insert event.
                /// </summary>
                /// <param name="evt">
                /// Insert event received
                /// </param>
                public void EntryInserted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicFilterInsert);
                }

                /// <summary>
                /// Receive delete event.
                /// </summary>
                /// <param name="evt">
                /// Delete event received
                /// </param>
                public void EntryDeleted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicFilterDelete);
                }
            }

            #endregion

            // ----- inner class: TestFilterLiteListener --------------------------------------

            #region Inner class: TestFilterLiteListener

            /// <summary>
            /// Custom listener that counts filter lite events.
            /// </summary>
            public class TestFilterLiteListener : ICacheListener
                {
                /// <summary>
                /// Constructor for the listener.
                /// </summary>
                public TestFilterLiteListener()
                {
                }

                /// <summary>
                /// Receive update event.
                /// </summary>
                /// <param name="evt">
                /// Update event received
                /// </param>
                public void EntryUpdated(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicFilterLiteUpdate);
                }

                /// <summary>
                /// Receive insert event.
                /// </summary>
                /// <param name="evt">
                /// Insert event received
                /// </param>
                public void EntryInserted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicFilterLiteInsert);
                }

                /// <summary>
                /// Receive delete event.
                /// </summary>
                /// <param name="evt">
                /// Delete event received
                /// </param>
                public void EntryDeleted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicFilterLiteDelete);
                }
            }

            #endregion

            // ----- inner class: TestTransformFilterListener --------------------------------------

            #region Inner class: TestTransformFilterListener

            /// <summary>
            /// Custom listener that counts transformed filter events.
            /// </summary>
            public class TestTransformFilterListener : ICacheListener
                {
                /// <summary>
                /// Constructor for the listener.
                /// </summary>
                public TestTransformFilterListener()
                {
                }

                /// <summary>
                /// Receive update event.
                /// </summary>
                /// <param name="evt">
                /// Update event received
                /// </param>
                public void EntryUpdated(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicTransformFilterUpdate);
                }

                /// <summary>
                /// Receive insert event.
                /// </summary>
                /// <param name="evt">
                /// Insert event received
                /// </param>
                public void EntryInserted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicTransformFilterInsert);
                }

                /// <summary>
                /// Receive delete event.
                /// </summary>
                /// <param name="evt">
                /// Delete event received
                /// </param>
                public void EntryDeleted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicTransformFilterDelete);
                }
            }

            #endregion

            // ----- inner class: TestTransformFilterLiteListener --------------------------------------

            #region Inner class: TestTransformFilterLiteListener

            /// <summary>
            /// Custom listener that counts transformed filter lite events.
            /// </summary>
            public class TestTransformFilterLiteListener : ICacheListener
                {
                /// <summary>
                /// Constructor for the listener.
                /// </summary>
                public TestTransformFilterLiteListener()
                {
                }

                /// <summary>
                /// Receive update event.
                /// </summary>
                /// <param name="evt">
                /// Update event received
                /// </param>
                public void EntryUpdated(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicTransformFilterLiteUpdate);
                }

                /// <summary>
                /// Receive insert event.
                /// </summary>
                /// <param name="evt">
                /// Insert event received
                /// </param>
                public void EntryInserted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicTransformFilterLiteInsert);
                }

                /// <summary>
                /// Receive delete event.
                /// </summary>
                /// <param name="evt">
                /// Delete event received
                /// </param>
                public void EntryDeleted(CacheEventArgs evt)
                {
                    Interlocked.Increment(ref atomicTransformFilterLiteDelete);
                }
            }

            #endregion
    }
}
