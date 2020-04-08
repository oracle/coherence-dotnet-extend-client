/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using NUnit.Framework;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class SimplePofContextTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestTypeRegistrationWithNullType()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, null, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestTypeRegistrationWithNullSerializer()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, this.GetType(), null);
        }

        [Test]
        public void TestTypeRegistration()
        {
            SimplePofContext ctx = new SimplePofContext();
            PortableObjectSerializer serializer = new PortableObjectSerializer(1);
            ctx.RegisterUserType(1, this.GetType(), serializer);
            ctx.RegisterUserType(2, typeof(IPortableObject), new PortableObjectSerializer(2));

            Assert.AreEqual(1, ctx.GetUserTypeIdentifier(this));
            Assert.AreEqual(1, ctx.GetUserTypeIdentifier(this.GetType()));
            Assert.AreEqual(1, ctx.GetUserTypeIdentifier(this.GetType().AssemblyQualifiedName));

            Assert.AreEqual(this.GetType(), ctx.GetType(1));
            Assert.AreEqual(this.GetType().FullName, ctx.GetTypeName(1));
            Assert.AreEqual(serializer, ctx.GetPofSerializer(1));

            ctx.UnregisterUserType(1);
            ctx.UnregisterUserType(2);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetPofSerializerWithNegativeTypeId()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.GetPofSerializer(-1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetPofSerializerWithUnknownTypeId()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.GetPofSerializer(5);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetTypeWithNegativeTypeId()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.GetType(-1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetTypeWithUnknownTypeId()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.GetType(5);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetUserTypeIdentifierWithNullObject()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.GetUserTypeIdentifier((object) null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetUserTypeIdentifierWithNullType()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.GetUserTypeIdentifier((Type) null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetUserTypeIdentifierWithUnknownType()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.GetUserTypeIdentifier(this.GetType());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetUserTypeIdentifierWithNullTypeName()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.GetUserTypeIdentifier((String) null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetUserTypeIdentifierWithUnknownTypeName()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.GetUserTypeIdentifier(this.GetType().AssemblyQualifiedName);
        }

        [Test]
        public void TestSerialization()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, typeof(PortablePerson), new PortableObjectSerializer(1));
            ctx.RegisterUserType(2, typeof(Address), new PortableObjectSerializer(2));

            PortablePerson aleks = new PortablePerson("Aleksandar Seovic", new DateTime(1974, 8, 24));
            PortablePerson marija = new PortablePerson("Marija Seovic", new DateTime(1978, 2, 20));
            PortablePerson ana = new PortablePerson("Ana Maria Seovic", new DateTime(2004, 8, 14, 7, 43, 0));
            aleks.Spouse = marija;
            aleks.Address = marija.Address = ana.Address = new Address("208 Myrtle Ridge Rd", "Lutz", "FL", "33549");
            aleks.Children = marija.Children = new PortablePerson[] { ana };

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), aleks);

            stream.Position = 0;
            PortablePerson aleks2 = (PortablePerson) ctx.Deserialize(new DataReader(stream));

            Assert.AreEqual(aleks.Name, aleks2.Name);
            Assert.AreEqual(aleks.DOB, aleks2.DOB);
            Assert.AreEqual(aleks.Spouse.Name, aleks2.Spouse.Name);
            Assert.AreEqual(aleks.Address.City, aleks2.Address.City);
            Assert.AreEqual(1, aleks2.Children.Length);
            Assert.AreEqual(ana.Name, aleks2.Children[0].Name);
            Assert.AreEqual(ana.Address.Street, aleks2.Children[0].Address.Street);

            SimplePofContext liteCtx = new SimplePofContext();
            liteCtx.RegisterUserType(1, typeof(PortablePersonLite), new PortableObjectSerializer(1));
            liteCtx.RegisterUserType(2, typeof(Address), new PortableObjectSerializer(2));

            stream.Position = 0;
            PortablePersonLite aleks3 = (PortablePersonLite) liteCtx.Deserialize(new DataReader(stream));
            Assert.AreEqual(aleks.Name, aleks3.Name);
            Assert.AreEqual(aleks.DOB, aleks3.DOB);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestDeserializationException()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, typeof(PortablePerson), new PortableObjectSerializer(1));
            ctx.RegisterUserType(2, typeof(Address), new PortableObjectSerializer(2));

            PortablePerson aleks = new PortablePerson("Aleksandar Seovic", new DateTime(1974, 8, 24));
            PortablePerson marija = new PortablePerson("Marija Seovic", new DateTime(1978, 2, 20));
            PortablePerson ana = new PortablePerson("Ana Maria Seovic", new DateTime(2004, 8, 14, 7, 43, 0));
            aleks.Spouse = marija;
            aleks.Address = marija.Address = ana.Address = new Address("208 Myrtle Ridge Rd", "Lutz", "FL", "33549");
            aleks.Children = marija.Children = new PortablePerson[] { ana };

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), aleks);

            stream.Position = 0;
            stream.Close();
            PortablePerson aleks2 = (PortablePerson)ctx.Deserialize(new DataReader(stream));
        }

        [Test]
        [ExpectedException(typeof(IOException))]
        public void TestSerializationWithNonPortableType()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, typeof(PersonLite), new PortableObjectSerializer(1));

            PersonLite ana = new PersonLite("Ana Maria Seovic", new DateTime(2006, 8, 14));

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), ana);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSerializationWithBackwardsReadingType()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, typeof(BadPersonLite), new PortableObjectSerializer(1));

            BadPersonLite ana = new BadPersonLite("Ana Maria Seovic", new DateTime(2006, 8, 14));

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), ana);

            stream.Position = 0;
            BadPersonLite ana2 = (BadPersonLite) ctx.Deserialize(new DataReader(stream));
        }

        [Test]
        [ExpectedException(typeof(EndOfStreamException))]
        public void TestSerializationWithSkippingReadingType()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, typeof(SkippingPersonLite), new PortableObjectSerializer(1));

            SkippingPersonLite ana = new SkippingPersonLite("Ana Maria Seovic", new DateTime(2006, 8, 14));

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), ana);

            stream.Position = 0;
            SkippingPersonLite ana2 = (SkippingPersonLite) ctx.Deserialize(new DataReader(stream));
        }

        [Test]
        public void TestEvolvableObjectSerialization()
        {
            SimplePofContext ctx_v1 = new SimplePofContext();
            ctx_v1.RegisterUserType(1, typeof(EvolvablePortablePerson), new PortableObjectSerializer(1));
            ctx_v1.RegisterUserType(2, typeof(Address), new PortableObjectSerializer(2));

            SimplePofContext ctx_v2 = new SimplePofContext();
            ctx_v2.RegisterUserType(1, typeof(EvolvablePortablePerson2), new PortableObjectSerializer(1));
            ctx_v2.RegisterUserType(2, typeof(Address), new PortableObjectSerializer(2));

            EvolvablePortablePerson2 aleks = new EvolvablePortablePerson2("Aleksandar Seovic", new DateTime(1974, 8, 24));
            EvolvablePortablePerson2 ana = new EvolvablePortablePerson2("Ana Maria Seovic", new DateTime(2004, 8, 14, 7, 43, 0));
            aleks.Address = ana.Address = new Address("208 Myrtle Ridge Rd", "Lutz", "FL", "33549");
            aleks.Nationality = ana.Nationality = "Serbian";
            aleks.PlaceOfBirth = new Address(null, "Belgrade", "Serbia", "11000");
            ana.PlaceOfBirth = new Address("128 Asbury Ave, #401", "Evanston", "IL", "60202");
            aleks.Children = new EvolvablePortablePerson2[] { ana };

            Stream stream_v2 = new MemoryStream();
            ctx_v2.Serialize(new DataWriter(stream_v2), aleks);

            stream_v2.Position = 0;
            EvolvablePortablePerson aleks_v1 = (EvolvablePortablePerson) ctx_v1.Deserialize(new DataReader(stream_v2));

            EvolvablePortablePerson marija_v1 = new EvolvablePortablePerson("Marija Seovic", new DateTime(1978, 2, 20));
            marija_v1.Address = aleks_v1.Address;
            marija_v1.Children = aleks_v1.Children;
            aleks_v1.Spouse = marija_v1;

            Stream stream_v1 = new MemoryStream();
            ctx_v1.Serialize(new DataWriter(stream_v1), aleks_v1);

            stream_v1.Position = 0;
            EvolvablePortablePerson2 aleks_v2 = (EvolvablePortablePerson2) ctx_v2.Deserialize(new DataReader(stream_v1));

            Assert.AreEqual(aleks.Name, aleks_v2.Name);
            Assert.AreEqual(aleks.Nationality, aleks_v2.Nationality);
            Assert.AreEqual(aleks.DOB, aleks_v2.DOB);
            Assert.AreEqual(aleks_v1.Spouse.Name, aleks_v2.Spouse.Name);
            Assert.AreEqual(aleks.Address.City, aleks_v2.Address.City);
            Assert.AreEqual(aleks.PlaceOfBirth.City, aleks_v2.PlaceOfBirth.City);
        }
    }
}