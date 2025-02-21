/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using NUnit.Framework;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class PofStreamUserTypeTests
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
        public void TestBeginPropertyWithExcepton1()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, typeof(PortablePerson), new PortableObjectSerializer(1));
            PortablePerson zoja = new PortablePerson("Zoja", new DateTime(1982, 11, 11, 7, 15, 30));
            Stream stream = new MemoryStream();
            IPofWriter writer = new PofStreamWriter.UserTypeWriter(new DataWriter(stream), ctx, 1, -1);
            Assert.That(() => writer.WriteObject(-1, zoja), Throws.ArgumentException);

        }

        [Test]
        public void TestWriteUserTypeInfoWithExcepton1()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, typeof(PortablePerson), new PortableObjectSerializer(1));
            PortablePerson zoja = new PortablePerson("Zoja", new DateTime(1982, 11, 11, 7, 15, 30));
            Stream stream = new MemoryStream();
            IPofWriter writer = new PofStreamWriter.UserTypeWriter(new DataWriter(stream), ctx, 1, -1);
            writer.WriteRemainder(null);
            Assert.That(() => writer.WriteObject(0, zoja), Throws.TypeOf<EndOfStreamException>());
        }

        [Test]
        public void TestWriteUserTypeInfoWithExcepton2()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(1, typeof(PortablePerson), new PortableObjectSerializer(1));
            PortablePerson zoja = new PortablePerson("Zoja", new DateTime(1982, 11, 11, 7, 15, 30));
            Stream stream = new MemoryStream();
            IPofWriter writer = new PofStreamWriter.UserTypeWriter(new DataWriter(stream), ctx, 1, -1);
            stream.Close();
            Assert.That(() => writer.WriteObject(0, zoja), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestVersionIdNegativeVelueException()
        {
            SimplePofContext ctx = new SimplePofContext();
            Stream stream = new MemoryStream();
            IPofWriter writer = new PofStreamWriter.UserTypeWriter(new DataWriter(stream), ctx, 1, -1);
            Assert.That(() => writer.VersionId = -1, Throws.ArgumentException);
        }

    }
}