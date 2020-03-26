/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;
using NUnit.Framework;
using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Transformer
{
    [TestFixture]
    public class SemiLiteEventTransformerTests
    {
        [Test]
        public void TestTransform()
        {
            SemiLiteEventTransformer transformer = SemiLiteEventTransformer.Instance;
            SemiLiteEventTransformer transformer2 = new SemiLiteEventTransformer();

            Assert.AreEqual(transformer, transformer2);
            Assert.IsNotNull(transformer);

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
            Assert.IsInstanceOf(typeof(ConverterCollections.ConverterCacheEventArgs), evtNew);
            ConverterCollections.ConverterCacheEventArgs convEvtNew = evtNew as ConverterCollections.ConverterCacheEventArgs;
            Assert.IsNotNull(convEvtNew);

            Assert.AreEqual(convEvt.Cache, convEvtNew.Cache);
            Assert.AreEqual(convEvt.EventType, convEvtNew.EventType);
            Assert.AreEqual(convEvt.Key, convEvtNew.Key);
            Assert.AreEqual(convEvt.NewValue, convEvtNew.NewValue);
            Assert.AreEqual(convEvt.IsSynthetic, convEvtNew.IsSynthetic);
            Assert.AreEqual(convEvt.ConverterKeyUp, convEvtNew.ConverterKeyUp);
            Assert.AreEqual(convEvt.ConverterValueUp, convEvtNew.ConverterValueUp);
            Assert.IsNotNull(convEvt.CacheEvent.OldValue);
            Assert.IsNull(convEvtNew.CacheEvent.OldValue);
        }

        [Test]
        public void TestSerialization()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.IsNotNull(ctx);

            SemiLiteEventTransformer semiLiteEventTransformer = new SemiLiteEventTransformer();

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), semiLiteEventTransformer);

            stream.Position = 0;
            Assert.AreEqual(semiLiteEventTransformer, ctx.Deserialize(new DataReader(stream)));

            stream.Close();
        }
    }
}
