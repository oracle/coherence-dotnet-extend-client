/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using NUnit.Framework;
using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;

namespace Tangosol.Util.Transformer
{
    [TestFixture]
    public class ExtractorEventTransformerTests
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
        public void TestTransform()
        {
            ExtractorEventTransformer transformer = new ExtractorEventTransformer(null, IdentityExtractor.Instance);

            LocalCache cache = new LocalCache();
            CacheEventArgs evt = new CacheEventArgs(cache, CacheEventType.Inserted, "inserted", "old value", "new value", false);
            CacheEventArgs evtNew = transformer.Transform(evt);
            
            Assert.IsNotNull(evtNew);
            Assert.AreEqual(evt.Cache, evtNew.Cache);
            Assert.AreEqual(evt.EventType, evtNew.EventType);
            Assert.AreEqual(evt.Key, evtNew.Key);
            Assert.AreNotEqual(evt.OldValue, evtNew.OldValue);
            Assert.IsNotNull(evt.OldValue);
            Assert.IsNull(evtNew.OldValue);
            Assert.AreEqual(evt.NewValue, evtNew.NewValue);
            Assert.AreEqual(evt.IsSynthetic, evtNew.IsSynthetic);
            
            evt = ConverterCollections.GetCacheEventArgs(cache, evt,
                NullImplementation.GetConverter(), NullImplementation.GetConverter());
            Assert.IsNotNull(evt);
            Assert.IsInstanceOf(typeof (ConverterCollections.ConverterCacheEventArgs), evt);
            ConverterCollections.ConverterCacheEventArgs convEvt = evt as ConverterCollections.ConverterCacheEventArgs;
            Assert.IsNotNull(convEvt);
            evtNew = transformer.Transform(convEvt);
            Assert.IsNotNull(evtNew);
        }

        [Test]
        public void TestSerialization()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.IsNotNull(ctx);

            ExtractorEventTransformer extractorEventTransformer = new ExtractorEventTransformer("methodName1");

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), extractorEventTransformer);

            stream.Position = 0;
            Assert.AreEqual(extractorEventTransformer, ctx.Deserialize(new DataReader(stream)));

            stream.Close();
        }

        [Test]
        public void TestCustomEventTransformer()
        {
            INamedCache cache = CacheFactory.GetCache("dist-cache");
            cache.Clear();

            // add data
            var obj1 = new EventTransformerTestObject();
            obj1.ID = 1;
            obj1.Name = "A";
            cache.Add(1, obj1);

            // add two listeners 
            ICacheListener listener1 = AddListener(cache);
            ICacheListener listener2 = AddListener(cache);
            Blocking.Sleep(100);

            // add more data
            var obj2 = new EventTransformerTestObject();
            var obj3 = new EventTransformerTestObject();
            obj2.ID = 2;
            obj2.Name = "B";
            obj3.ID = 3;
            obj3.Name = "C";
            cache.Add(2, obj2);
            cache.Add(3, obj3);

            // remove an entry 
            cache.Remove(1);

            Blocking.Sleep(1000);
            Assert.AreEqual(4, cInsertCalled);
            Assert.AreEqual(2, cDeleteCalled);
        }

        private ICacheListener AddListener(INamedCache cache)
        {
            ICacheListener            mapListener = new CoherenceMapListner();
            ExtractorEventTransformer transformer = new CustomExtractorEventTransformer("getID");
            IFilter                   filter      = new CacheEventTransformerFilter(
                new CacheEventFilter(CacheEventFilter.CacheEventMask.All, new AlwaysFilter()), transformer);

            cache.AddCacheListener(mapListener, filter, false);
            return mapListener;
        }

        public static int cDeleteCalled = 0;
        public static int cInsertCalled = 0;
        public static int cUpdateCalled = 0;
    }

    // ICacheListerner implementation 
    internal class CoherenceMapListner : ICacheListener
    {
        public void EntryDeleted(CacheEventArgs evt)
        {
            lock (syncObj)
            {
                ExtractorEventTransformerTests.cDeleteCalled++;
            }
        }

        public void EntryInserted(CacheEventArgs evt)
        {
            lock (syncObj)
            {
                ExtractorEventTransformerTests.cInsertCalled++;
            }
        }

        public void EntryUpdated(CacheEventArgs evt)
        {
            lock (syncObj)
            {
                ExtractorEventTransformerTests.cUpdateCalled++;
            }
        }

        private readonly Object syncObj = new Object();
    }
    
    [Serializable]
    public class CustomExtractorEventTransformer : ExtractorEventTransformer
    {
        public CustomExtractorEventTransformer()
        {}

        public CustomExtractorEventTransformer(String method)
            : base(method)
        {}
    }
}
