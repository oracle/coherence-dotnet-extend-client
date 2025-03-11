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
    public class DataReaderAndWriterTests
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
        public void TestPackedInt32()
        {
            MemoryStream stream = new MemoryStream();
            DataWriter writer = new DataWriter(stream);
            writer.WritePackedInt32(100);
            writer.WritePackedInt32(100000);
            writer.WritePackedInt32(0);
            writer.WritePackedInt32(-1);
            writer.WritePackedInt32(Int32.MinValue);
            writer.WritePackedInt32(Int32.MaxValue);

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            Assert.AreEqual(100, reader.ReadPackedInt32());
            Assert.AreEqual(100000, reader.ReadPackedInt32());
            Assert.AreEqual(0, reader.ReadPackedInt32());
            Assert.AreEqual(-1, reader.ReadPackedInt32());
            Assert.AreEqual(Int32.MinValue, reader.ReadPackedInt32());
            Assert.AreEqual(Int32.MaxValue, reader.ReadPackedInt32());
        }

        [Test]
        public void TestPackedInt64()
        {
            MemoryStream stream = new MemoryStream();
            DataWriter writer = new DataWriter(stream);
            writer.WritePackedInt64(100);
            writer.WritePackedInt64(100000);
            writer.WritePackedInt64(0);
            writer.WritePackedInt64(-1);
            writer.WritePackedInt64(Int64.MinValue);
            writer.WritePackedInt64(Int64.MaxValue);

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            Assert.AreEqual(100L, reader.ReadPackedInt64());
            Assert.AreEqual(100000L, reader.ReadPackedInt64());
            Assert.AreEqual(0L, reader.ReadPackedInt64());
            Assert.AreEqual(-1L, reader.ReadPackedInt64());
            Assert.AreEqual(Int64.MinValue, reader.ReadPackedInt64());
            Assert.AreEqual(Int64.MaxValue, reader.ReadPackedInt64());
        }
    }
}