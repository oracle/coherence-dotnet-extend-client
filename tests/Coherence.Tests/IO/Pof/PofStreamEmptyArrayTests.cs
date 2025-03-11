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
    public class PofStreamEmptyArrayTests
    {
        
        public void initPOF()
        {
            initPOFWriter();
            initPOFReader();
        }

        private void initPOFReader()
        {
            stream.Position = 0;
            reader = new DataReader(stream);
            pofReader = new PofStreamReader(reader, new SimplePofContext());
        }

        private void initPOFWriter()
        {
            stream = new MemoryStream();
            writer = new DataWriter(stream);
            pofWriter = new PofStreamWriter(writer, new SimplePofContext());
        }

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
        public void TestEmptyArray()
        {
            initPOFWriter();
            pofWriter.WriteArray(0, new object[0]);

            initPOFReader();
            Assert.AreEqual(0, pofReader.ReadArray(0).Length);
        }

        [Test]
        public void TestEmptyUniformArray()
        {
            initPOFWriter();
            pofWriter.WriteByteArray(0, new byte[0]);

            initPOFReader();
            Assert.AreEqual(0, pofReader.ReadByteArray(0).Length);
        }

        [Test]
        public void TestNestedEmptyArray()
        {
            initPOFWriter();
            pofWriter.WriteArray(0, new object[] { new object[0] });

            initPOFReader();
            object[] result = (object[]) pofReader.ReadArray(0);
            Assert.IsTrue(result[0] is object[], "Element was not deserialized as an empty object array.");
            Assert.AreEqual(0, ((object[]) result[0]).Length);
        }

        [Test]
        public void TestNestedEmptyUniformArray()
        {
            initPOFWriter();
            pofWriter.WriteArray(0, new object[] { new byte[0] });

            initPOFReader();
            object[] result = (object[]) pofReader.ReadArray(0);
            Assert.IsTrue(result[0] is byte[], "Element was not deserialized as an empty byte array.");
            Assert.AreEqual(0, ((byte[]) result[0]).Length);
        }

        private DataReader reader;
        private DataWriter writer;
        private PofStreamReader pofReader;
        private PofStreamWriter pofWriter;
        private MemoryStream stream;
    }
}