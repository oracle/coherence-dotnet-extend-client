/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

using NUnit.Framework;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class PofHelperParsingTests
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

        [Test]
        public void TestDecodeTinyInt()
        {   //PASSED
            Assert.AreEqual(0, PofHelper.DecodeTinyInt(PofConstants.V_INT_0));
            Assert.AreEqual(22, PofHelper.DecodeTinyInt(PofConstants.V_INT_22));
            Assert.AreEqual(-1, PofHelper.DecodeTinyInt(PofConstants.V_INT_NEG_1));
            Assert.AreEqual(17, PofHelper.DecodeTinyInt(PofConstants.V_INT_17));
        }

        [Test]
        public void TestReadChar()
        {   //PASSED
            initPOFWriter();
            writer.Write('a');
            writer.Write(char.MinValue);
            writer.Write(char.MaxValue);
            writer.Write((char) 0x0080);
            writer.Write((char) 0x0800);

            initPOFReader();
            Assert.AreEqual('a', PofHelper.ReadChar(reader));
            Assert.AreEqual(char.MinValue, PofHelper.ReadChar(reader));
            Assert.AreEqual(char.MaxValue, PofHelper.ReadChar(reader));
            Assert.AreEqual((char)0x0080, PofHelper.ReadChar(reader));
            Assert.AreEqual((char)0x0800, PofHelper.ReadChar(reader));
        }

        [Test]
        public void TestReadAsChar()
        {   //PASSED
            initPOFWriter();
            writer.Write('A');
            writer.Write(char.MinValue);
            writer.Write(char.MaxValue);
            writer.WritePackedInt32(0x0080);
            writer.Write((char)0x0800);

            initPOFReader();
            Assert.AreEqual('A', PofHelper.ReadAsChar(reader, PofConstants.T_OCTET));
            Assert.AreEqual(char.MinValue, PofHelper.ReadAsChar(reader, PofConstants.T_CHAR));
            Assert.AreEqual(char.MaxValue, PofHelper.ReadAsChar(reader, PofConstants.T_CHAR));
            Assert.AreEqual((char)0x0080, PofHelper.ReadAsChar(reader, PofConstants.T_INT32));
            Assert.AreEqual((char)0x0800, PofHelper.ReadAsChar(reader, PofConstants.T_CHAR));
        }

        [Test]
        public void TestReadAsInt32()
        {   //PASSED
            initPOFWriter();
            writer.WritePackedInt32((Int32)0);
            writer.WritePackedInt32(Int32.MinValue);
            writer.WritePackedInt32(PofConstants.V_INT_NEG_1);
            writer.WritePackedInt32(Int32.MaxValue);

            initPOFReader();
            Assert.AreEqual((Int32)0, PofHelper.ReadAsInt32(reader, PofConstants.T_INT32));
            Assert.AreEqual(Int32.MinValue, PofHelper.ReadAsInt32(reader, PofConstants.T_INT32));
            Assert.AreEqual(PofConstants.V_INT_NEG_1, PofHelper.ReadAsInt32(reader, PofConstants.T_INT32));
            Assert.AreEqual(Int32.MaxValue, PofHelper.ReadAsInt32(reader, PofConstants.T_INT32));

            initPOFWriter();
            pofwriter.WriteSingle(0, 1000.123f);
            pofwriter.WriteDouble(0, 3000.123456);

            initPOFReader();
            Assert.AreEqual(1000, pofreader.ReadInt32(0));
            Assert.AreEqual(3000, pofreader.ReadInt32(0));
        }

         [Test]
         public void TestReadAsInt32Exception()
         {
             initPOFWriter();
             pofwriter.WriteString(0, "invalid value");

             initPOFReader();
             Assert.That(pofreader.ReadInt32(0), Throws.TypeOf<IOException>());
         }

        [Test]
        public void TestReadAsInt64()
        {   //PASSED
            initPOFWriter();
            writer.WritePackedInt64((Int64)0);
            writer.WritePackedInt64(Int64.MinValue);
            writer.WritePackedInt64((Int64)PofConstants.V_INT_NEG_1);
            writer.WritePackedInt64(Int64.MaxValue);

            initPOFReader();
            Assert.AreEqual((Int64)0, PofHelper.ReadAsInt64(reader, PofConstants.T_INT64));
            Assert.AreEqual(Int64.MinValue, PofHelper.ReadAsInt64(reader, PofConstants.T_INT64));
            Assert.AreEqual((Int64)PofConstants.V_INT_NEG_1, PofHelper.ReadAsInt64(reader, PofConstants.T_INT64));
            Assert.AreEqual(Int64.MaxValue, PofHelper.ReadAsInt64(reader, PofConstants.T_INT64));

            initPOFWriter();
            pofwriter.WriteSingle(0, 1000.123f);
            pofwriter.WriteDouble(0, 3000.123456);

            initPOFReader();
            Assert.AreEqual(1000, pofreader.ReadInt64(0));
            Assert.AreEqual(3000, pofreader.ReadInt64(0));
        }

        [Test]
        public void TestReadAsSingle()
        {   //PASSED
            initPOFWriter();
            writer.Write((float)0.0);
            writer.Write((float)Int64.MinValue);
            writer.Write((float)PofConstants.V_INT_NEG_1);
            writer.Write((float)Int64.MaxValue);

            initPOFReader();
            Assert.AreEqual((float)0.0, PofHelper.ReadAsSingle(reader, PofConstants.T_FLOAT32));
            Assert.AreEqual((float)Int64.MinValue, PofHelper.ReadAsSingle(reader, PofConstants.T_FLOAT32));
            Assert.AreEqual((float)PofConstants.V_INT_NEG_1, PofHelper.ReadAsSingle(reader, PofConstants.T_FLOAT32));
            Assert.AreEqual((float)Int64.MaxValue, PofHelper.ReadAsSingle(reader, PofConstants.T_FLOAT32));

            initPOFWriter();
            pofwriter.WriteDouble(0, 3000.123456);

            initPOFReader();
            Assert.AreEqual(3000.123456f, pofreader.ReadSingle(0));
        }

        [Test]
        public void TestReadAsDouble()
        {   //PASSED
            initPOFWriter();
            writer.Write((double)0.0);
            writer.Write((double)Double.MinValue);
            writer.Write((double)PofConstants.V_INT_NEG_1);
            writer.Write((double)Double.MaxValue);
            pofwriter.WriteSingle(0, 11.234f);

            initPOFReader();
            Assert.AreEqual((double)0.0, PofHelper.ReadAsDouble(reader, PofConstants.T_FLOAT64));
            Assert.AreEqual((double)Double.MinValue, PofHelper.ReadAsDouble(reader, PofConstants.T_FLOAT64));
            Assert.AreEqual((double)PofConstants.V_INT_NEG_1, PofHelper.ReadAsDouble(reader, PofConstants.T_FLOAT64));
            Assert.AreEqual((double)Double.MaxValue, PofHelper.ReadAsDouble(reader, PofConstants.T_FLOAT64));
            Assert.AreEqual((Single)11.234, (Single)pofreader.ReadDouble(0));
        }

        [Test]
        public void TestReadDate()
        {   //PASSED
            initPOFWriter();
            writer.WritePackedInt32(2006); writer.WritePackedInt32(8); writer.WritePackedInt32(11);
            writer.WritePackedInt32(2006); writer.WritePackedInt32(8); writer.WritePackedInt32(12);
            writer.WritePackedInt32(2006); writer.WritePackedInt32(8); writer.WritePackedInt32(13);

            initPOFReader();
            Assert.AreEqual(new DateTime(2006, 8, 11), PofHelper.ReadDate(reader));
            Assert.AreEqual(new DateTime(2006, 8, 12), PofHelper.ReadDate(reader));
            Assert.AreEqual(new DateTime(2006, 8, 13), PofHelper.ReadDate(reader));
        }

        [Test]
        public void TestReadTime()
        {   //PASSED
            initPOFWriter();
            writer.WritePackedInt32(12); writer.WritePackedInt32(59); writer.WritePackedInt32(57); writer.WritePackedInt32(-100); writer.WritePackedInt32(0);
            writer.WritePackedInt32(12); writer.WritePackedInt32(59); writer.WritePackedInt32(58); writer.WritePackedInt32(-100); writer.WritePackedInt32(0);
            writer.WritePackedInt32(12); writer.WritePackedInt32(59); writer.WritePackedInt32(59); writer.WritePackedInt32(-100); writer.WritePackedInt32(0);

            initPOFReader();
            
            Assert.AreEqual(new RawTime(12, 59, 57, 100, false), PofHelper.ReadRawTime(reader));
            Assert.AreEqual(new RawTime(12, 59, 58, 100, false), PofHelper.ReadRawTime(reader));
            Assert.AreEqual(new RawTime(12, 59, 59, 100, false), PofHelper.ReadRawTime(reader));
        }

        [Test]
        public void TestReadDateTime()
        {   //PASSED
            initPOFWriter();

            writer.WritePackedInt32(2006); writer.WritePackedInt32(8); writer.WritePackedInt32(11);
            writer.WritePackedInt32(12); writer.WritePackedInt32(59); writer.WritePackedInt32(57); writer.WritePackedInt32(100); writer.WritePackedInt32(1);

            writer.WritePackedInt32(2006); writer.WritePackedInt32(8); writer.WritePackedInt32(12);
            writer.WritePackedInt32(12); writer.WritePackedInt32(59); writer.WritePackedInt32(58); writer.WritePackedInt32(100); writer.WritePackedInt32(1);

            writer.WritePackedInt32(2006); writer.WritePackedInt32(8); writer.WritePackedInt32(13);
            writer.WritePackedInt32(12); writer.WritePackedInt32(59); writer.WritePackedInt32(59); writer.WritePackedInt32(100); writer.WritePackedInt32(1);

            initPOFReader();
            Assert.AreEqual(new DateTime(2006, 8, 11, 12, 59, 57, 100), PofHelper.ReadDateTime(reader));
            Assert.AreEqual(new DateTime(2006, 8, 12, 12, 59, 58, 100), PofHelper.ReadDateTime(reader));
            Assert.AreEqual(new DateTime(2006, 8, 13, 12, 59, 59, 100), PofHelper.ReadDateTime(reader));
        }

        [Test]
        public void TestSkipValue()
        {
            //PASSED
            initPOFWriter();

            pofwriter.WriteDate(0, new DateTime(2006, 8, 10, 12, 59, 11));
            pofwriter.WriteDateTime(0, new DateTime(2006, 8, 10, 12, 59, 11));
            pofwriter.WriteUniversalDateTime(0, new DateTime(2006, 8, 10, 12, 59, 11, 1));
            pofwriter.WriteCharArray(0, new char[] {'g', 't', 's'});
            pofwriter.WriteArray(0, new object[]{'g', "Gor", 55});
            pofwriter.WriteArray(0, new object[] { new int[] { 1, 2 } , new int[] { 3, 2, 4 } } );

            Hashtable ht = new Hashtable();
            ht.Add(1, "t"); ht.Add(2, "g");
            pofwriter.WriteObject(0, ht);

            pofwriter.WriteByte(0, 0x00F0);

            pofwriter.WriteInt32(0, 300);

            initPOFReader();
            PofHelper.SkipValue(reader);
            PofHelper.SkipValue(reader);
            PofHelper.SkipValue(reader);
            PofHelper.SkipValue(reader);
            PofHelper.SkipValue(reader);
            PofHelper.SkipValue(reader);
            PofHelper.SkipValue(reader);
            PofHelper.SkipValue(reader);
            Assert.AreEqual(PofConstants.T_INT32, PofHelper.ReadAsInt32(reader, PofConstants.T_INT32));
            Assert.AreEqual(300, PofHelper.ReadAsInt32(reader, PofConstants.T_INT32));
        }

        [Test]
        public void TestSkipUniformValue()
        {   //PASSED
            // PLEASE DO NOTE, the method SkipUniformValue should be used only for Uniform_Arrays/Collections
            initPOFWriter();

            pofwriter.WriteInt32(0, 100);
            pofwriter.WriteInt32(0, 200);
            pofwriter.WriteInt32(0, 300);

            initPOFReader();
            PofHelper.SkipUniformValue(reader, PofConstants.T_INT32);
            PofHelper.SkipUniformValue(reader, PofConstants.T_INT32);
            PofHelper.SkipUniformValue(reader, PofConstants.T_INT32);
            PofHelper.SkipUniformValue(reader, PofConstants.T_INT32);
            Assert.AreEqual(PofConstants.T_INT32, PofHelper.ReadAsInt32(reader, PofConstants.T_INT32));
            Assert.AreEqual(300, PofHelper.ReadAsInt32(reader, PofConstants.T_INT32));

            initPOFWriter();
            RawYearMonthInterval ymi = new RawYearMonthInterval(2, 10);
            pofwriter.WriteRawYearMonthInterval(0, ymi);
            pofwriter.WriteString(0, "skipping to string1");
            pofwriter.WriteTimeInterval(0, new TimeSpan(4, 52, 10));
            pofwriter.WriteString(0, "skipping to string2");
            pofwriter.WriteDayTimeInterval(0, new TimeSpan(11, 12, 13, 14, 50) );
            pofwriter.WriteString(0, "skipping to string3");
            pofwriter.WriteSingle(0, 120.34f );
            pofwriter.WriteString(0, "skipping to string4");
            pofwriter.WriteDouble(0, 1222.22);
            pofwriter.WriteString(0, "skipping to string5");
            pofwriter.WriteChar(0, 'A');
            pofwriter.WriteString(0, "skipping to string6");

            initPOFReader();
            PofHelper.SkipUniformValue(reader, PofConstants.T_INT32);
            PofHelper.SkipUniformValue(reader, PofConstants.T_YEAR_MONTH_INTERVAL);
            Assert.AreEqual("skipping to string1", pofreader.ReadString(0));

            PofHelper.SkipUniformValue(reader, PofConstants.T_INT32);
            PofHelper.SkipUniformValue(reader, PofConstants.T_TIME_INTERVAL);
            Assert.AreEqual("skipping to string2", pofreader.ReadString(0));

            PofHelper.SkipUniformValue(reader, PofConstants.T_INT32);
            PofHelper.SkipUniformValue(reader, PofConstants.T_DAY_TIME_INTERVAL);
            Assert.AreEqual("skipping to string3", pofreader.ReadString(0));

            PofHelper.SkipUniformValue(reader, PofConstants.T_INT32);
            PofHelper.SkipUniformValue(reader, PofConstants.T_FLOAT32);
            Assert.AreEqual("skipping to string4", pofreader.ReadString(0));

            PofHelper.SkipUniformValue(reader, PofConstants.T_INT32);
            PofHelper.SkipUniformValue(reader, PofConstants.T_FLOAT64);
            Assert.AreEqual("skipping to string5", pofreader.ReadString(0));

            PofHelper.SkipUniformValue(reader, PofConstants.T_INT32);
            PofHelper.SkipUniformValue(reader, PofConstants.T_CHAR);
            Assert.AreEqual("skipping to string6", pofreader.ReadString(0));


        }

        [Test]
        public void TestSkipPackedInts()
        {   //PASSED
            initPOFWriter();

            writer.WritePackedInt32(100);
            writer.WritePackedInt32(200);
            writer.WritePackedInt32(300);


            initPOFReader();
            PofHelper.SkipPackedInts(reader, 2);
            Assert.AreEqual(300, PofHelper.ReadAsInt32(reader, PofConstants.T_INT32));
        }
    }
}