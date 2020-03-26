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
    public class PofStreamUserTypeTests
    {

        [Test]
        public void TestUserTypeConstuctor()
        {
            SimplePofContext ctx = new SimplePofContext();
            Stream stream = new MemoryStream();
            IPofWriter writer = new PofStreamWriter.UserTypeWriter(new DataWriter(stream),
                                                                   ctx, 1, -1);
            Assert.IsTrue(writer!=null);
            Assert.AreEqual(1, writer.UserTypeId);
            Assert.AreEqual(0, writer.VersionId);

        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBeginPropertyWithExcepton1()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, typeof(PortablePerson), new PortableObjectSerializer(1));
            PortablePerson zoja = new PortablePerson("Zoja", new DateTime(1982, 11, 11, 7, 15, 30));
            Stream stream = new MemoryStream();
            IPofWriter writer = new PofStreamWriter.UserTypeWriter(new DataWriter(stream), ctx, 1, -1);
            writer.WriteObject(-1,zoja);

        }

        [Test]
        [ExpectedException(typeof(EndOfStreamException))]
        public void TestWriteUserTypeInfoWithExcepton1()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, typeof(PortablePerson), new PortableObjectSerializer(1));
            PortablePerson zoja = new PortablePerson("Zoja", new DateTime(1982, 11, 11, 7, 15, 30));
            Stream stream = new MemoryStream();
            IPofWriter writer = new PofStreamWriter.UserTypeWriter(new DataWriter(stream), ctx, 1, -1);
            writer.WriteRemainder(null);
            writer.WriteObject(0, zoja);
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestWriteUserTypeInfoWithExcepton2()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, typeof(PortablePerson), new PortableObjectSerializer(1));
            PortablePerson zoja = new PortablePerson("Zoja", new DateTime(1982, 11, 11, 7, 15, 30));
            Stream stream = new MemoryStream();
            IPofWriter writer = new PofStreamWriter.UserTypeWriter(new DataWriter(stream), ctx, 1, -1);
            stream.Close();
            writer.WriteObject(0, zoja);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestVersionIdNegativeVelueException()
        {
            SimplePofContext ctx = new SimplePofContext();
            Stream stream = new MemoryStream();
            IPofWriter writer = new PofStreamWriter.UserTypeWriter(new DataWriter(stream), ctx, 1, -1);
            writer.VersionId = -1;
        }

    }
}