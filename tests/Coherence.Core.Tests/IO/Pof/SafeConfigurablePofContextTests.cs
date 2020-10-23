/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using NUnit.Framework;

using Tangosol.Net.Messaging;
using Tangosol.Util;
using Tangosol.Util.Collections;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class SafeConfigurablePofContextTests
    {
        [Test]
        public void testGetPofSerializerWithNegativeTypeId()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.GetPofSerializer(-1), Throws.ArgumentException);
        }

        [Test]
        public void testGetPofSerializerWithKnownTypeId()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsTrue(ctx.GetPofSerializer(1) is PortableObjectSerializer);
        }

        [Test]
        public void testGetPofSerializerWithUnknownTypeId()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.GetPofSerializer(12358), Throws.ArgumentException);
        }

        [Test]
        public void testGetTypeWithNegativeTypeId()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.GetType(-1), Throws.ArgumentException);
        }

        [Test]
        public void testGetTypeWithKnownTypeId()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsTrue(typeof(Exception).Equals(ctx.GetType(0)));
        }

        [Test]
        public void testGetTypeWithUnknownTypeId()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.GetType(12358), Throws.ArgumentException);
        }

        [Test]
        public void testGetUserTypeIdentifierWithNullObject()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.GetUserTypeIdentifier((object) null), Throws.ArgumentNullException);
        }

        [Test]
        public void testGetUserTypeIdentifierWithUnknownObject()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsTrue(ctx.GetUserTypeIdentifier(new NestedType())
                    == SafeConfigurablePofContext.TYPE_PORTABLE);
        }

        [Test]
        public void testGetUserTypeIdentifierWithPofObject()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.GetUserTypeIdentifier(new HashDictionary()), Throws.ArgumentException);
        }

        [Test]
        public void testGetUserTypeIdentifierWithNullType()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.GetUserTypeIdentifier((Type) null), Throws.ArgumentNullException);
        }

        [Test]
        public void testGetUserTypeIdentifierWithUnknownType()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsTrue(ctx.GetUserTypeIdentifier(typeof(NestedType))
                    == SafeConfigurablePofContext.TYPE_PORTABLE);

            // COH-5584: repeat the test to verify that NestedType is now
            // cached in ConfigurablePofContext.
            Assert.IsTrue(ctx.GetUserTypeIdentifier(typeof(NestedType))
                    == SafeConfigurablePofContext.TYPE_PORTABLE);
        }

        [Test]
        public void testGetUserTypeIdentifierWithPofType()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.GetUserTypeIdentifier(typeof(HashDictionary)), Throws.ArgumentException);
        }

        [Test]
        public void testGetUserTypeIdentifierWithKnownType()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsTrue(ctx.GetUserTypeIdentifier(typeof(Exception)) == 0);
        }

        [Test]
        public void testGetUserTypeIdentifierWithNullTypeName()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.GetUserTypeIdentifier((String) null), Throws.ArgumentException);
        }

        [Test]
        public void testGetUserTypeIdentifierWithUnknownTypeName()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsTrue(ctx.GetUserTypeIdentifier(typeof(NestedType).AssemblyQualifiedName) 
                    == SafeConfigurablePofContext.TYPE_PORTABLE);
        }

        [Test]
        public void testGetUserTypeIdentifierWithPofTypeName()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.GetUserTypeIdentifier(ctx.GetUserTypeIdentifier(typeof(HashDictionary).AssemblyQualifiedName)), Throws.ArgumentException);
        }

        [Test]
        public void testGetUserTypeIdentifierWithKnownTypeName()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsTrue(ctx.GetUserTypeIdentifier(typeof(Exception).AssemblyQualifiedName) == 0);
        }

        [Test]
        public void testIsUserTypeWithNullObject()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.IsUserType((Object)null), Throws.ArgumentNullException);
        }

        [Test]
        public void testIsUserTypeWithUnknownObject()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsFalse(ctx.IsUserType(this));
        }

        [Test]
        public void testIsUserTypeWithPofObject()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsFalse(ctx.IsUserType(new HashDictionary()));
        }

        [Test]
        public void testIsUserTypeWithKnownObject()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsTrue(ctx.IsUserType(new Exception()));
        }

        [Test]
        public void testIsUserTypeWithNullType()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.IsUserType((Type) null), Throws.ArgumentNullException);
        }

        [Test]
        public void testIsUserTypeWithUnknownType()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsFalse(ctx.IsUserType(typeof(SafeConfigurablePofContextTests)));
        }

        [Test]
        public void testIsUserTypeWithPofType()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsFalse(ctx.IsUserType(typeof(HashDictionary)));
        }

        [Test]
        public void testIsUserTypeWithKnownType()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsTrue(ctx.IsUserType(typeof(Exception)));
        }

        [Test]
        public void testIsUserTypeWithNullTypeName()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.That(() => ctx.IsUserType((String) null), Throws.ArgumentException);
        }

        [Test]
        public void testIsUserTypeWithUnknownTypeName()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsFalse(ctx.IsUserType(typeof(SafeConfigurablePofContextTests).AssemblyQualifiedName));
        }

        [Test]
        public void testIsUserTypeWithPofTypeName()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsFalse(ctx.IsUserType(typeof(HashDictionary).AssemblyQualifiedName));
        }

        [Test]
        public void testIsUserTypeWithKnownTypeName()
        {
            var ctx = new SafeConfigurablePofContext();
            Assert.IsTrue(ctx.IsUserType(typeof(Exception).AssemblyQualifiedName));
        }

        [Test]
        public void testSerialization()
        {
            var ctx    = new SafeConfigurablePofContext();
            var uuid   = new UUID();
            var buffer = new MemoryStream(1024);
            ctx.Serialize(new DataWriter(buffer), uuid);

            buffer.Position = 0;
            Object o = ctx.Deserialize(new DataReader(buffer));
            Assert.AreEqual(o, uuid);

            var person = new PortablePerson("Aleksandar Seovic",
                    new DateTime(74, 7, 24));
            person.Address = new Address("208 Myrtle Ridge Rd", "Lutz", "FL", "33549");
            person.Children = new Person[]
                {
                new PortablePerson("Aleksandar Seovic JR.", new DateTime(174, 1, 1))
                };

            buffer.Position = 0;
            ctx.Serialize(new DataWriter(buffer), person);

            buffer.Position = 0;
            o = ctx.Deserialize(new DataReader(buffer));
            Assert.AreEqual(o, person);
        }

        [Test]
        public void testEvolvableSerialization()
        {
            var ctx    = new SafeConfigurablePofContext();
            var person = new EvolvablePortablePerson("Aleksandar Seovic",
                    new DateTime(74, 7, 24));
            person.Address = new Address("208 Myrtle Ridge Rd", "Lutz", "FL", "33549");
            person.DataVersion = 2;

            var buffer = new MemoryStream(1024);
            ctx.Serialize(new DataWriter(buffer), person);

            buffer.Position = 0;
            Object o = ctx.Deserialize(new DataReader(buffer));
            Assert.IsTrue((((EvolvablePortablePerson) o).DataVersion == 2));
            Assert.AreEqual(o, person);
        }

        [Test]
        public void testSetSerializer()
        {
            IPofContext ctx    = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            var         set    = new HashSet();
            Assert.IsTrue(ctx.IsUserType(set));
            Assert.IsTrue(ctx.IsUserType(typeof(HashSet)));
            Assert.IsTrue(ctx.IsUserType(typeof(HashSet).AssemblyQualifiedName));

            var buffer = new MemoryStream(1024);
            var writer = new DataWriter(buffer);
            ctx.Serialize(writer, new HashSet());

            var reader = new DataReader(buffer);
            buffer.Position = 0;
            object o = ctx.Deserialize(reader);
            Assert.IsTrue(o is HashSet);
            buffer.Close();

            ctx = new SafeConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.IsTrue(ctx.IsUserType(set));
            Assert.IsTrue(ctx.IsUserType(typeof(HashSet)));
            Assert.IsTrue(ctx.IsUserType(typeof(HashSet).AssemblyQualifiedName));

            buffer = new MemoryStream(1024);
            writer = new DataWriter(buffer);
            ctx.Serialize(writer, new HashSet());

            reader = new DataReader(buffer);
            buffer.Position = 0;
            o = ctx.Deserialize(reader);
            Assert.IsTrue(o is HashSet);
            buffer.Close();
        }

    }
}