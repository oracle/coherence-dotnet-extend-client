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
    public class PofStreamPrimitiveValueTests
    {
        private MemoryStream stream;
        private DataWriter writer;
        private PofStreamWriter pofwriter;
        private DataReader reader;
        private PofStreamReader pofreader;

        public void initPOF()
        {
            initPOFWriter();
            initPOFReader();
        }

        private void initPOFReader()
        {
            stream.Position = 0;
            reader = new DataReader(stream);
            pofreader = new PofStreamReader(reader, new SimplePofContext());
        }

        private void initPOFWriter()
        {
            stream = new MemoryStream();
            writer = new DataWriter(stream);
            pofwriter = new PofStreamWriter(writer, new SimplePofContext());
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
        public void TestPofStreamWriteBoolean()
        {
            initPOFWriter();
            pofwriter.WriteBoolean(0, true);
            pofwriter.WriteBoolean(0, false);
            pofwriter.WriteBoolean(0, true);
            pofwriter.WriteBoolean(0, true);

            initPOFReader();
            Assert.IsTrue(pofreader.ReadBoolean(0));
            Assert.IsFalse(pofreader.ReadBoolean(0));
            Assert.IsTrue(pofreader.ReadBoolean(0));

            object obool = pofreader.ReadObject(0);
            Assert.IsTrue(obool is bool);
            bool rbool = (bool)obool;
            Assert.IsTrue(rbool);
        }

        [Test]
        public void TestPofStreamWriteBooleanException()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofwriter.WriteBoolean(0, true), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestPofStreamWriteByte()
        {
            initPOFWriter();
            pofwriter.WriteByte(0, 1);
            pofwriter.WriteByte(0, 2);
            pofwriter.WriteByte(0, 255);
            pofwriter.WriteByte(0, 0);

            initPOFReader();
            Assert.AreEqual(1, pofreader.ReadByte(0));
            Assert.AreEqual(2, pofreader.ReadByte(0));
            Assert.AreEqual(255, pofreader.ReadByte(0));
            Assert.AreEqual(0, pofreader.ReadByte(0));
        }

        [Test]
        public void TestPofStreamWriteByteException()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofwriter.WriteByte(0, 1), Throws.TypeOf<ObjectDisposedException>());
        }


        [Test]
        public void TestPofStreamWriteChar()
        {
            initPOFWriter();
            pofwriter.WriteChar(0, 'g');
            pofwriter.WriteChar(0, Char.MaxValue);
            pofwriter.WriteChar(0, 'A');
            pofwriter.WriteChar(0, Char.MinValue);
            pofwriter.WriteChar(0, '%');

            initPOFReader();
            Assert.AreEqual('g', pofreader.ReadChar(0));
            Assert.AreEqual(Char.MaxValue, pofreader.ReadChar(0));
            Assert.AreEqual('A', pofreader.ReadChar(0));
            Assert.AreEqual(Char.MinValue, pofreader.ReadChar(0));
            Assert.AreEqual('%', pofreader.ReadChar(0));
        }

        [Test]
        public void TestPofStreamWriteCharException()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofwriter.WriteChar(0, 'c'), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestPofStreamWriteInt16()
        {
            initPOFWriter();
            pofwriter.WriteInt16(0, 0);
            pofwriter.WriteInt16(0, -1);
            pofwriter.WriteInt16(0, Int16.MaxValue);
            pofwriter.WriteInt16(0, 101);
            pofwriter.WriteInt16(0, Int16.MinValue);

            initPOFReader();
            Assert.AreEqual(0, pofreader.ReadInt16(0));
            Assert.AreEqual(-1, pofreader.ReadInt16(0));
            Assert.AreEqual(Int16.MaxValue, pofreader.ReadInt16(0));
            Assert.AreEqual(101, pofreader.ReadInt16(0));
            Assert.AreEqual(Int16.MinValue, pofreader.ReadInt16(0));
        }

        [Test]
        public void TestPofStreamWriteInt16Exception()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofwriter.WriteInt16(0, Int16.MinValue), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestPofStreamWriteInt32()
        {
            initPOFWriter();
            pofwriter.WriteInt32(0, 0);
            pofwriter.WriteInt32(0, -1);
            pofwriter.WriteInt32(0, Int32.MaxValue);
            pofwriter.WriteInt32(0, 101);
            pofwriter.WriteInt32(0, Int32.MinValue);

            initPOFReader();
            Assert.AreEqual(0, pofreader.ReadInt32(0));
            Assert.AreEqual(-1, pofreader.ReadInt32(0));
            Assert.AreEqual(Int32.MaxValue, pofreader.ReadInt32(0));
            Assert.AreEqual(101, pofreader.ReadInt32(0));
            Assert.AreEqual(Int32.MinValue, pofreader.ReadInt32(0));
        }

        [Test]
        public void TestPofStreamWriteInt32Exception()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofwriter.WriteInt32(0, Int32.MinValue), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestPofStreamWriteInt64()
        {
            initPOFWriter();
            pofwriter.WriteInt64(0, 0);
            pofwriter.WriteInt64(0, -1);
            pofwriter.WriteInt64(0, Int64.MaxValue);
            pofwriter.WriteInt64(0, 101);
            pofwriter.WriteInt64(0, Int64.MinValue);

            initPOFReader();
            Assert.AreEqual(0, pofreader.ReadInt64(0));
            Assert.AreEqual(-1, pofreader.ReadInt64(0));
            Assert.AreEqual(Int64.MaxValue, pofreader.ReadInt64(0));
            Assert.AreEqual(101, pofreader.ReadInt64(0));
            Assert.AreEqual(Int64.MinValue, pofreader.ReadInt64(0));
        }

        [Test]
        public void TestPofStreamWriteInt64Exception()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofwriter.WriteInt64(0, Int64.MinValue), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestPofStreamWriteSingle()
        {
            initPOFWriter();
            pofwriter.WriteSingle(0, 0);
            pofwriter.WriteSingle(0, -1.0F);
            pofwriter.WriteSingle(0, Single.MaxValue);
            pofwriter.WriteSingle(0, 100000F);
            pofwriter.WriteSingle(0, Single.MinValue);
            pofwriter.WriteSingle(0, Single.NegativeInfinity);
            pofwriter.WriteSingle(0, Single.PositiveInfinity);
            pofwriter.WriteSingle(0, Single.NaN);

            initPOFReader();
            Assert.AreEqual(0, pofreader.ReadSingle(0));
            Assert.AreEqual(-1.0F, pofreader.ReadSingle(0));
            Assert.AreEqual(Single.MaxValue, pofreader.ReadSingle(0));
            Assert.AreEqual(100000F, pofreader.ReadSingle(0));
            Assert.AreEqual(Single.MinValue, pofreader.ReadSingle(0));
            Assert.AreEqual(Single.NegativeInfinity, pofreader.ReadSingle(0));
            Assert.AreEqual(Single.PositiveInfinity, pofreader.ReadSingle(0));
            Assert.AreEqual(Single.NaN, pofreader.ReadSingle(0));
        }

        [Test]
        public void TestPofStreamWriteSingleException()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofwriter.WriteSingle(0, Single.MinValue), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestPofStreamWriteDouble()
        {
            initPOFWriter();
            pofwriter.WriteDouble(0, 0);
            pofwriter.WriteDouble(0, -1.0);
            pofwriter.WriteDouble(0, 1.0);
            pofwriter.WriteDouble(0, Double.MaxValue);
            pofwriter.WriteDouble(0, 100.0);
            pofwriter.WriteDouble(0, Double.MinValue);
            pofwriter.WriteDouble(0, Double.NegativeInfinity);
            pofwriter.WriteDouble(0, Double.PositiveInfinity);
            pofwriter.WriteDouble(0, Double.NaN);

            initPOFReader();
            Assert.AreEqual(0, pofreader.ReadDouble(0));
            Assert.AreEqual(-1.0, pofreader.ReadDouble(0));
            Assert.AreEqual(1.0, pofreader.ReadDouble(0));
            Assert.AreEqual(Double.MaxValue, pofreader.ReadDouble(0));
            Assert.AreEqual(100.0, pofreader.ReadDouble(0));
            Assert.AreEqual(Double.MinValue, pofreader.ReadDouble(0));
            Assert.AreEqual(Double.NegativeInfinity, pofreader.ReadDouble(0));
            Assert.AreEqual(Double.PositiveInfinity, pofreader.ReadDouble(0));
            Assert.AreEqual(Double.NaN, pofreader.ReadDouble(0));
        }

        [Test]
        public void TestPofStreamWriteDoubleException()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofwriter.WriteDouble(0, Double.MinValue), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestPofStreamBadAdvanceTo()
        {
            initPOFWriter();
            pofwriter.WriteSingle(0, 100.0F);

            initPOFReader();
            Assert.That(() => pofreader.ReadSingle(1), Throws.InvalidOperationException);
        }

        [Test]
        public void TestPofStreamReaderBadReadObjectFloat128()
        {
            initPOFWriter();
            writer.WritePackedInt32(PofConstants.T_FLOAT128);
            initPOFReader();
            Assert.That(() => pofreader.ReadObject(0), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void TestPofStreamReaderBadReadObjectINT128()
        {
            initPOFWriter();
            writer.WritePackedInt32(PofConstants.T_INT128);
            writer.WritePackedInt32(int.MaxValue);

            initPOFReader();
            Assert.That(() => pofreader.ReadObject(0), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void TestPofStreamReaderReadObjectDate()
        {
            initPOFWriter();
            writer.WritePackedInt32(PofConstants.T_DATE);
            writer.WritePackedInt32(2006);
            writer.WritePackedInt32(8);
            writer.WritePackedInt32(8);

            initPOFReader();
            Assert.AreEqual(new DateTime(2006,8,8), pofreader.ReadObject(0));
        }

        [Test]
        public void TestPofStreamReaderReadObjectTime()
        {
            initPOFWriter();
            writer.WritePackedInt32(PofConstants.T_TIME);
            writer.WritePackedInt32(12);
            writer.WritePackedInt32(8);
            writer.WritePackedInt32(8);
            writer.WritePackedInt32(100);
            writer.WritePackedInt32(1);

            initPOFReader();
            Assert.AreEqual(new RawTime(12, 8, 8, 100*1000000, true), pofreader.ReadObject(0));
        }

        [Test]
        public void TestPofStreamReaderReadObjectYearMonthInterval()
        {
            initPOFWriter();
            pofwriter.WriteRawYearMonthInterval(0, new RawYearMonthInterval(1,1));

            initPOFReader();
            Assert.AreEqual(new RawYearMonthInterval(1,1), pofreader.ReadObject(0));
        }
    }
}