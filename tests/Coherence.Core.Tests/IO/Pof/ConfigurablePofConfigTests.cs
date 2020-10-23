/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using NUnit.Framework;
using Tangosol.Net;
using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class ConfigurablePofConfigTests
    {
        [Test]
        public void TestLocalMemberPof()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core/Tangosol.Config/coherence-pof-config.xml");

            Type type = ctx.GetType(160);
            Assert.AreEqual(typeof(LocalMember), type);

            LocalMember member = new LocalMember
            {
                MachineName = "machine1",
                MemberName = "member1",
                ProcessName = "process1",
                RackName = "rack1",
                RoleName = "role1",
                SiteName = "site1"
            };

            MemoryStream memstream  = new MemoryStream();

            ctx.Serialize(new DataWriter(memstream), member);
            memstream.Position = 0;
            LocalMember copy = (LocalMember)ctx.Deserialize(new DataReader(memstream));
            Assert.AreEqual(member.MachineName, copy.MachineName);
            Assert.AreEqual(member.MemberName, copy.MemberName);
            Assert.AreEqual(member.ProcessName, copy.ProcessName);
            Assert.AreEqual(member.RackName, copy.RackName);
            Assert.AreEqual(member.RoleName, copy.RoleName);
            Assert.AreEqual(member.SiteName, copy.SiteName);
        }

        [Test]
        public void TestConfigPofContextWithPortableObject()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.AreEqual(typeof(PortableObjectSerializer), ctx.GetPofSerializer(1201).GetType());

            Type type = ctx.GetType(1201);
            Assert.AreEqual(typeof(Address), type);
            type = ctx.GetType(1005);
            Assert.AreEqual(typeof(PortablePerson), type);

            // adding tests for null implementations that are included in POF config
            type = ctx.GetType(10);
            Assert.AreEqual(typeof(NullFilter), type);
            type = ctx.GetType(11);
            Assert.AreEqual(typeof(NullImplementation.NullCollection), type);
            type = ctx.GetType(12);
            Assert.AreEqual(typeof(NullImplementation.NullObservableCache), type);
            type = ctx.GetType(13);
            Assert.AreEqual(typeof(NullImplementation.NullValueExtractor), type);

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            ctx = new ConfigurablePofContext(config);
            IXmlElement res = ctx.Config;
            Assert.AreEqual(res, config);

            Assert.AreEqual("Tangosol.Address", ctx.GetTypeName(1201));
            Assert.AreEqual("Tangosol.PortablePerson", ctx.GetTypeName(1005));

            Assert.AreEqual(1002, ctx.GetUserTypeIdentifier(typeof(PortablePersonLite)));
            Assert.AreEqual(1005, ctx.GetUserTypeIdentifier("Tangosol.PortablePerson, Coherence.Core.Tests"));

            Address home = new Address("Palmira Toljatija 50", "Belgrade",
                                        "Serbia", "11070");
            Assert.AreEqual(1201, ctx.GetUserTypeIdentifier(home));

            ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config-include.xml");

            Assert.AreEqual("Tangosol.Address", ctx.GetTypeName(1201));
            Assert.AreEqual("Tangosol.PortablePerson", ctx.GetTypeName(1005));
            Assert.AreEqual("Tangosol.SkippingPersonLite", ctx.GetTypeName(2002));

            Assert.AreEqual(1002, ctx.GetUserTypeIdentifier(typeof(PortablePersonLite)));
            Assert.AreEqual(1005, ctx.GetUserTypeIdentifier("Tangosol.PortablePerson, Coherence.Core.Tests"));
            Assert.AreEqual(2001, ctx.GetUserTypeIdentifier(typeof(BadPersonLite)));
            Assert.IsTrue(ctx.IsUserType("Tangosol.PortablePersonLite, Coherence.Core.Tests"));
        }

        [Test]
        public void TestDefaultConstructor()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext();
            IPofSerializer ser = ctx.GetPofSerializer(14);
            Assert.AreEqual(typeof(PortableObjectSerializer), ser.GetType());

            ser = ctx.GetPofSerializer(0);
            Assert.AreEqual(typeof(ExceptionPofSerializer), ser.GetType());

            Type type = ctx.GetType(0);
            Assert.AreEqual(typeof(Exception), type);
            type = ctx.GetType(14);
            Assert.AreEqual(typeof(UUID), type);
            type = ctx.GetType(1201);
            Assert.AreEqual(typeof(Address), type);
        }

        [Test]
        public void TestConfigPofContextException()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.That(() => ctx.GetType(333), Throws.ArgumentException);
        }

        [Test]
        public void TestGetPofSerializerWithNull()
        {
            Int32 nonExistingId = 333;
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.That(() => ctx.GetPofSerializer(nonExistingId), Throws.ArgumentException);
        }

        [Test]
        public void TestGetUserTypeIdentifierWithNullObject()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.That(() => ctx.GetUserTypeIdentifier((object) null), Throws.ArgumentNullException);
        }

        [Test]
        public void TestGetUserTypeIdWithUnknownType1()
        {
            String nonExistingType = "Dummy";
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.That(() => ctx.GetUserTypeIdentifier(nonExistingType), Throws.ArgumentException);
        }

        [Test]
        public void TestGetUserTypeIdWithNullType1()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.That(() => ctx.GetUserTypeIdentifier((String)null), Throws.ArgumentException);
        }

        [Test]
        public void TestGetUserTypeIdWithUnknownType2()
        {
            Type nonExistingType = typeof(PersonLite);
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.That(() => ctx.GetUserTypeIdentifier(nonExistingType), Throws.ArgumentException);
        }

        [Test]
        public void TestGetUserTypeIdWithUnknownType3()
        {
            Type nonExistingType = typeof(PersonLite);
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.That(() => ctx.GetUserTypeIdentifier(nonExistingType), Throws.ArgumentException);
        }

        [Test]
        public void TestGetUserTypeIdWithNullType2()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.That(() => ctx.GetUserTypeIdentifier((Type)null), Throws.ArgumentNullException);
        }

        [Test]
        public void TestInitialize()
        {
            ConfigurablePofContext ctx1 = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            IXmlElement config1 = ctx1.Config;
            ConfigurablePofContext ctx2 = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            IXmlElement config2 = ctx2.Config;
            Assert.AreEqual(config1, config2);
        }

        [Test]
        public void TestCheckNotInit()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            IXmlElement config = ctx.Config;
            Assert.That(() => ctx.Config = config, Throws.InvalidOperationException);
        }

        [Test]
        public void TestSerialization()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config-include.xml");
            Address home = new Address("Palmira Toljatija 50", "Belgrade","Serbia", "11070");
            MemoryStream memstream = new MemoryStream();
            ctx.Serialize(new DataWriter(memstream), home);
            memstream.Position = 0;
            Address res = (Address) ctx.Deserialize(new DataReader(memstream));
            Assert.AreEqual(home.Street, res.Street);
            Assert.AreEqual(home.State, res.State);
            Assert.AreEqual(home.City, res.City);
            Assert.AreEqual(home.ZIP, res.ZIP);
        }

        [Test]
        public void TestConfigPofContextAllowSubclasses()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config-allowsubclass.xml");
            int typeId = ctx.GetUserTypeIdentifier(typeof(EvolvablePortablePerson));
            Assert.AreEqual(ctx.GetUserTypeIdentifier(typeof(PortablePerson)), typeId);
            Assert.AreEqual(ctx.GetUserTypeIdentifier(typeof(PortablePerson)),
                            ctx.GetUserTypeIdentifier(typeof(EvolvablePortablePerson)));
        }

        [Test]
        public void TestConfigPofContextAllowSubclasses2()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config-allowsubclass2.xml");
            Assert.That(() => ctx.GetUserTypeIdentifier(typeof(EvolvablePortablePerson)), Throws.ArgumentException);
        }

        [Test]
        public void TestConfigPofContextAllowInterfaces()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config-allowinterfaces.xml");
            Assert.That(() => ctx.GetUserTypeIdentifier(typeof(EvolvablePortablePerson)), Throws.ArgumentException);
        }

        [Test]
        public void TestConfigPofContextBad1()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config-bad1.xml"); ;
            Assert.That(() => ctx.GetUserTypeIdentifier(typeof(PortablePerson)), Throws.InvalidOperationException);
        }

        [Test]
        public void TestConfigPofContextBad2()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config-bad2.xml");
            Assert.That(() => ctx.GetUserTypeIdentifier(typeof(PortablePerson)), Throws.InvalidOperationException);
        }

        [Test]
        public void TestConfigPofContextBad3()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config-bad3.xml");
            Assert.That(() => ctx.IsUserType(typeof(PortablePerson)), Throws.Exception);
        }

        [Test]
        public void TestConfigWithDefaultSerializer()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config-defaultserializer.xml");
            Assert.IsNotNull(ctx);

            IXmlElement config = ctx.Config;
            Assert.IsNotNull(config);

            // uses default serializer
            Address address = new Address();
            string addressTypeName = "Tangosol.Address";
            Type addressType = typeof(Address);

            int typeId = ctx.GetUserTypeIdentifier(addressType);
            Assert.AreEqual(typeId, 1201);
            string typeName = ctx.GetTypeName(typeId);
            Assert.AreEqual(typeName, addressTypeName);
            Type type = ctx.GetType(typeId);
            Assert.AreEqual(type, addressType);
            IPofSerializer serializer = ctx.GetPofSerializer(typeId);
            Assert.IsInstanceOf(typeof(DummyDefaultPofSerializer), serializer);
            Assert.IsTrue(ctx.IsUserType(address));

            // has its own serializer so default won't be used
            PortablePersonLite portablePersonLite = new PortablePersonLite();
            string portablePersonLiteTypeName = "Tangosol.PortablePersonLite";
            Type portablePersonLiteType = typeof(PortablePersonLite);

            typeId = ctx.GetUserTypeIdentifier(portablePersonLiteType);
            Assert.AreEqual(typeId, 1002);
            typeName = ctx.GetTypeName(typeId);
            Assert.AreEqual(typeName, portablePersonLiteTypeName);
            type = ctx.GetType(typeId);
            Assert.AreEqual(type, portablePersonLiteType);
            serializer = ctx.GetPofSerializer(typeId);
            Assert.IsInstanceOf(typeof(XmlPofSerializer), serializer);
            Assert.IsTrue(ctx.IsUserType(portablePersonLite));

            // is defined in included file with it's own default serializer
            // different from previous
            PortablePerson portablePerson = new PortablePerson();
            string portablePersonTypeName = "Tangosol.PortablePerson";
            Type portablePersonType = typeof(PortablePerson);

            typeId = ctx.GetUserTypeIdentifier(portablePersonType);
            Assert.AreEqual(typeId, 1005);
            typeName = ctx.GetTypeName(typeId);
            Assert.AreEqual(typeName, portablePersonTypeName);
            type = ctx.GetType(typeId);
            Assert.AreEqual(type, portablePersonType);
            serializer = ctx.GetPofSerializer(typeId);
            Assert.IsInstanceOf(typeof(BinaryPofSerializer), serializer);
            Assert.IsTrue(ctx.IsUserType(portablePerson));

            // does not have serializer defined and default is not specified
            // in the config, therefore will use PortableObjectSerializer
            serializer = ctx.GetPofSerializer(14); //UUID
            Assert.IsInstanceOf(typeof (PortableObjectSerializer), serializer);
        }

        [Test]
        public void TestMultipleIncludes()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("Config/multiple-include-pof-config.xml");
            ctx.IsUserType(typeof(PortablePerson));
        }

        [Test]
        public void TestPofAnnotationSerializer()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("Config/multiple-include-pof-config.xml");

            Assert.IsInstanceOf(typeof(PortableObjectSerializer), ctx.GetPofSerializer(2000));
            Assert.IsInstanceOf(typeof(PofAnnotationSerializer), ctx.GetPofSerializer(2003));
            Assert.IsInstanceOf(typeof(PofAnnotationSerializer), ctx.GetPofSerializer(2004));
        }
    }

    public class DummyDefaultPofSerializer : IPofSerializer
    {
        public void Serialize(IPofWriter writer, object o)
        {}

        public object Deserialize(IPofReader reader)
        {
            return null;
        }
    }
}