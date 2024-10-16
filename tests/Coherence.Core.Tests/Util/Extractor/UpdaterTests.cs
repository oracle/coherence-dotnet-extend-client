/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Specialized;
using System.IO;

using NUnit.Framework;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.IO.Pof.Reflection;
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Util.Processor;

namespace Tangosol.Util.Extractor
{
    [TestFixture]
    public class UpdaterTests
    {
        NameValueCollection appSettings = TestUtils.AppSettings;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
        }

        [Test]
        public void TestReflectionUpdater()
        {
            IValueUpdater updater = new ReflectionUpdater("field");
            IValueUpdater updater1 = new ReflectionUpdater("field");
            IValueUpdater updater2 = new CompositeUpdater("field");
            Assert.IsNotNull(updater);
            Assert.AreEqual(updater, updater1);
            Assert.AreNotEqual(updater, updater2);
            Assert.AreEqual(updater.ToString(), updater1.ToString());
            Assert.AreEqual(updater.GetHashCode(), updater1.GetHashCode());

            ReflectionTestType o = new ReflectionTestType();
            int value = 100;
            o.field = 0;
            updater.Update(o, value);
            Assert.AreEqual(o.field, value);
            updater.Update(o, value*2);
            Assert.AreEqual(o.field, value*2);

            try
            {
                updater.Update(null, value);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentNullException), e);
            }

            updater = new ReflectionUpdater("Property");
            o.Property = 1;
            updater.Update(o, value);
            Assert.AreEqual(o.Property, value);

            updater = new ReflectionUpdater("SetMethod");
            o = new ReflectionTestType();
            updater.Update(o, value);
            Assert.AreEqual(o.field, value);

            try
            {
                updater = new ReflectionUpdater("InvalidMember");
                updater.Update(o, value);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e.InnerException);
            }
        }

        [Test]
        public void TestCompositeUpdater()
        {
            IValueUpdater updater = new CompositeUpdater("field");
            IValueUpdater updater1 = new CompositeUpdater("field");
            IValueUpdater updater2 = new ReflectionUpdater("field");
            Assert.IsNotNull(updater);
            Assert.AreEqual(updater, updater1);
            Assert.AreNotEqual(updater, updater2);
            Assert.AreEqual(updater.ToString(), updater1.ToString());
            Assert.AreEqual(updater.GetHashCode(), updater1.GetHashCode());

            IValueExtractor extractor = (updater as CompositeUpdater).Extractor;
            Assert.IsNotNull(extractor);
            IValueUpdater updter = (updater as CompositeUpdater).Updater;
            Assert.IsNotNull(updter);
            Assert.IsInstanceOf(typeof(IdentityExtractor), extractor);
            Assert.IsInstanceOf(typeof(ReflectionUpdater), updter);

            ReflectionTestType o = new ReflectionTestType();
            int value = 100;
            o.field = 0;
            updater.Update(o, value);
            Assert.AreEqual(o.field, value);

            updater = new CompositeUpdater("InnerMember.field");

            IValueExtractor ext = new ChainedExtractor("InnerMember");
            IValueUpdater upd = new ReflectionUpdater("field");
            updater1 = new CompositeUpdater(ext, upd);

            Assert.IsNotNull(updater);
            Assert.AreEqual(updater, updater1);
            Assert.AreEqual(updater.ToString(), updater1.ToString());
            Assert.AreEqual(updater.GetHashCode(), updater1.GetHashCode());

            updater.Update(o, value);
            Assert.AreEqual(o.InnerMember.field, value);
        }

        [Test]
        public void TestUpdaterSerialization()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.IsNotNull(ctx);

            CompositeUpdater compositeUpdater = new CompositeUpdater("name");
            ReflectionUpdater reflectionUpdater = new ReflectionUpdater("member");

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), compositeUpdater);
            ctx.Serialize(new DataWriter(stream), reflectionUpdater);

            stream.Position = 0;
            Assert.AreEqual(compositeUpdater, ctx.Deserialize(new DataReader(stream)));
            Assert.AreEqual(reflectionUpdater, ctx.Deserialize(new DataReader(stream)));

            stream.Close();
        }

        [Test]
        public void TestPofUpdater()
        {
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            PortablePerson original = new PortablePerson();
            original.Name = "Aleksandar Seovic";
            original.Address = new Address("street", "Belgrade", "SRB", "11000");
            
            cache.Insert("p1", original);
            
            PofUpdater updName = new PofUpdater(0);
            PofUpdater updCity = new PofUpdater(new SimplePofPath(new int[] {1, 1}));

            cache.Invoke("p1", new UpdaterProcessor(updName, "Novak Seovic"));
            cache.Invoke("p1", new UpdaterProcessor(updCity, "Lutz"));

            PortablePerson modified = (PortablePerson) cache["p1"];
            Assert.AreEqual("Novak Seovic", modified.Name);
            Assert.AreEqual("Lutz", modified.Address.City);

            CacheFactory.Shutdown();
        }

        [Test]
        public void TestUniversalUpdater()
        {
            INamedCache cache = CacheFactory.GetCache(CacheName);
            cache.Clear();

            string           key      = "p1";
            string           lastName = "Van Halen";
            SimplePerson     person   = new SimplePerson("123-45-6789", "Eddie", lastName, 1955,
                "987-65-4321", new String[] { "456-78-9123" });
            UniversalUpdater updater  = new UniversalUpdater("LastName");

            cache[key] = person;
            Assert.AreEqual(lastName, ((SimplePerson) cache[key]).LastName);

            // update the last name
            lastName = "Van Helen";
            cache.Invoke(key, new UpdaterProcessor(updater, lastName));
            Assert.AreEqual(lastName, ((SimplePerson) cache[key]).LastName);

            CacheFactory.Shutdown();
        }
    }
}
