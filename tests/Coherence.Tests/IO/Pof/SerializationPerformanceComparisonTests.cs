/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
#pragma warning disable 618,612

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Xml.Serialization;
using NUnit.Framework;
using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    [Explicit]
    [TestFixture]
    public class SerializationPerformanceComparisonTests
    {
        private readonly SimplePofContext           pofSerializer;
        private readonly XmlSerializer              xmlSerializer;
        private readonly DataContractSerializer     dcSerializer;
        private readonly DataContractJsonSerializer dcJsonSerializer;

        private readonly object emptyObject;
        private readonly object populatedObject;
        private readonly string typeName;

        private const int ITERATION_COUNT = 10000;

        public SerializationPerformanceComparisonTests()
        {
            pofSerializer    = new SimplePofContext();
            
            ConfigurePofContext();
            emptyObject      = CreateEmptyObjectInstance();
            populatedObject  = CreatePopulatedObjectInstance();
            typeName         = emptyObject.GetType().FullName;

            xmlSerializer    = new XmlSerializer(emptyObject.GetType());
            dcSerializer     = new DataContractSerializer(emptyObject.GetType());
            dcJsonSerializer = new DataContractJsonSerializer(emptyObject.GetType());
        }

        #region Overrideable configuration methods

        protected virtual void ConfigurePofContext()
        {
            pofSerializer.RegisterUserType(1,
                                           typeof(PortablePerson),
                                           new PortableObjectSerializer(1));
            pofSerializer.RegisterUserType(2,
                                           typeof(Address),
                                           new PortableObjectSerializer(2));
        }

        protected virtual object CreateEmptyObjectInstance()
        {
            return new PortablePerson();
        }

        protected virtual object CreatePopulatedObjectInstance()
        {
            var address = new Address("555 Main St", "Tampa", "FL", "33555");

            var children = new Person[]
                                  {
                                      new PortablePerson("Ana Maria Seovic", new DateTime(2004, 8, 14)),
                                      new PortablePerson("Novak Seovic", new DateTime(2007, 12, 28)),
                                      new PortablePerson("Kristina Seovic", new DateTime(2013, 2, 13))
                                  };

            var spouse = new PortablePerson()
            {
                Name = "Marija Seovic",
                DOB = new DateTime(1978, 2, 20),
                Address = address,
                Children = children
            };

            var person = new PortablePerson()
            {
                Name = "Aleksandar Seovic",
                DOB = new DateTime(1974, 8, 24),
                Address = address,
                Spouse = spouse,
                Children = children

            };

            return person;
        }

        #endregion

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
        public void TestSerializationSize()
        {
            Console.Out.WriteLine();
            Console.Out.WriteLine("Empty " + typeName + " Size:");
            Console.Out.WriteLine("----------------------------------------------------------------");
            Console.Out.WriteLine("POF".PadRight(25) + SerializePof(emptyObject).Length.ToString("N0") + " bytes");
            Console.Out.WriteLine("JSON".PadRight(25) + SerializeJson(emptyObject).Length.ToString("N0") + " bytes\n" + SerializeJson(emptyObject));
            //Console.Out.WriteLine("XML".PadRight(25) + SerializeXml(emptyObject).Length.ToString("N0") + " bytes");
            Console.Out.WriteLine("DataContract".PadRight(25) + SerializeDataContract(emptyObject).Length.ToString("N0") + " bytes");
            Console.Out.WriteLine("DataContract JSON".PadRight(25) + SerializeDataContractJson(emptyObject).Length.ToString("N0") + " bytes");

            Console.Out.WriteLine();
            Console.Out.WriteLine("Populated " + typeName + " Size:");
            Console.Out.WriteLine("----------------------------------------------------------------");
            Console.Out.WriteLine("POF".PadRight(25) + SerializePof(populatedObject).Length.ToString("N0") + " bytes");
            Console.Out.WriteLine("JSON".PadRight(25) + SerializeJson(populatedObject).Length.ToString("N0") + " bytes\n" + SerializeJson(populatedObject));
            //Console.Out.WriteLine("XML".PadRight(25) + SerializeXml(populatedObject).Length.ToString("N0") + " bytes");
            Console.Out.WriteLine("DataContract".PadRight(25) + SerializeDataContract(populatedObject).Length.ToString("N0") + " bytes");
            Console.Out.WriteLine("DataContract JSON".PadRight(25) + SerializeDataContractJson(populatedObject).Length.ToString("N0") + " bytes");
        }

        [Test]
        public void TestSerializationSpeed()
        {
            long pof    = TimePofSerialization(emptyObject);
            long json   = TimeJsonSerialization(emptyObject);
            //long xml    = TimeXmlSerialization(emptyObject);
            long dc     = TimeDataContractSerialization(emptyObject);
            long dcJson = TimeDataContractJsonSerialization(emptyObject);

            Console.Out.WriteLine();
            Console.Out.WriteLine("Empty " + typeName + " Speed (" + ITERATION_COUNT.ToString("N0") + " iterations):");
            Console.Out.WriteLine("----------------------------------------------------------------");
            Console.Out.WriteLine("POF".PadRight(25) + pof.ToString("N0") + " ms");
            Console.Out.WriteLine("JSON".PadRight(25) + json.ToString("N0") + " ms");
            //Console.Out.WriteLine("XML".PadRight(25) + xml.ToString("N0") + " ms");
            Console.Out.WriteLine("DataContract".PadRight(25) + dc.ToString("N0") + " ms");
            Console.Out.WriteLine("DataContract JSON".PadRight(25) + dcJson.ToString("N0") + " ms");

            pof    = TimePofSerialization(populatedObject);
            json   = TimeJsonSerialization(populatedObject);
            //xml    = TimeXmlSerialization(populatedObject);
            dc     = TimeDataContractSerialization(populatedObject);
            dcJson = TimeDataContractJsonSerialization(populatedObject);

            Console.Out.WriteLine();
            Console.Out.WriteLine("Populated " + typeName + " Speed (" + ITERATION_COUNT.ToString("N0") + " iterations):");
            Console.Out.WriteLine("----------------------------------------------------------------");
            Console.Out.WriteLine("POF".PadRight(25) + pof.ToString("N0") + " ms");
            Console.Out.WriteLine("JSON".PadRight(25) + json.ToString("N0") + " ms");
            //Console.Out.WriteLine("XML".PadRight(25) + xml.ToString("N0") + " ms");
            Console.Out.WriteLine("DataContract".PadRight(25) + dc.ToString("N0") + " ms");
            Console.Out.WriteLine("DataContract JSON".PadRight(25) + dcJson.ToString("N0") + " ms");
        }

        #region Timer helpers

        private long TimePofSerialization(object o)
        {
            long start = DateTimeUtils.GetSafeTimeMillis();
            for (int i = 0; i < ITERATION_COUNT; i++)
            {
                DeserializePof(SerializePof(o));
            }
            return DateTimeUtils.GetSafeTimeMillis() - start;
        }

        private long TimeJsonSerialization(object o)
        {
            long start = DateTimeUtils.GetSafeTimeMillis();
            for (int i = 0; i < ITERATION_COUNT; i++)
            {
                DeserializeJson(SerializeJson(o));
            }
            return DateTimeUtils.GetSafeTimeMillis() - start;
        }

        private long TimeXmlSerialization(object o)
        {
            long start = DateTimeUtils.GetSafeTimeMillis();
            for (int i = 0; i < ITERATION_COUNT; i++)
            {
                DeserializeXml(SerializeXml(o));
            }
            return DateTimeUtils.GetSafeTimeMillis() - start;
        }

        private long TimeDataContractSerialization(object o)
        {
            long start = DateTimeUtils.GetSafeTimeMillis();
            for (int i = 0; i < ITERATION_COUNT; i++)
            {
                DeserializeDataContract(SerializeDataContract(o));
            }
            return DateTimeUtils.GetSafeTimeMillis() - start;
        }

        private long TimeDataContractJsonSerialization(object o)
        {
            long start = DateTimeUtils.GetSafeTimeMillis();
            for (int i = 0; i < ITERATION_COUNT; i++)
            {
                DeserializeDataContractJson(SerializeDataContractJson(o));
            }
            return DateTimeUtils.GetSafeTimeMillis() - start;
        }

        #endregion

        #region Serialization helpers

        private byte[] SerializePof(object o)
        {
            MemoryStream buffer = new MemoryStream();
            DataWriter writer = new DataWriter(buffer);
            pofSerializer.Serialize(writer, o);
            return buffer.ToArray();
        }

        private object DeserializePof(byte[] data)
        {
            MemoryStream buffer = new MemoryStream(data);
            DataReader reader = new DataReader(buffer);
            return pofSerializer.Deserialize(reader);
        }

        private string SerializeJson(object o)
        {
            return JsonSerializer.Serialize(o);
        }

        private object DeserializeJson(string data)
        {
            return JsonSerializer.Deserialize<PortablePerson>(data);
        }

        private byte[] SerializeXml(object o)
        {
            MemoryStream buffer = new MemoryStream();
            xmlSerializer.Serialize(buffer, o);
            return buffer.ToArray();
        }

        private object DeserializeXml(byte[] data)
        {
            MemoryStream buffer = new MemoryStream(data);
            return xmlSerializer.Deserialize(buffer);
        }

        private byte[] SerializeDataContract(object o)
        {
            MemoryStream buffer = new MemoryStream();
            dcSerializer.WriteObject(buffer, o);
            return buffer.ToArray();
        }

        private object DeserializeDataContract(byte[] data)
        {
            MemoryStream buffer = new MemoryStream(data);
            return dcSerializer.ReadObject(buffer);
        }

        private byte[] SerializeDataContractJson(object o)
        {
            MemoryStream buffer = new MemoryStream();
            dcJsonSerializer.WriteObject(buffer, o);
            return buffer.ToArray();
        }

        private object DeserializeDataContractJson(byte[] data)
        {
            MemoryStream buffer = new MemoryStream(data);
            return dcJsonSerializer.ReadObject(buffer);
        }

        #endregion

    }
}