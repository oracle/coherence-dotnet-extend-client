/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using NUnit.Framework;

using Tangosol.IO;
using Tangosol.IO.Pof;

namespace Tangosol.Data
{
    [TestFixture]
    public class PofStreamTests
    {
        private PofStreamReader initPofReader(String typeName)
        {
            Stream stream = GetType().Assembly.GetManifestResourceStream("Tangosol.Data.Java." + typeName + ".data");
            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofreader = new PofStreamReader(reader, new SimplePofContext());
            return pofreader;
        }

        private DataWriter initPofWriter(String sFileName)
        {
            const String fileDir = "../../../tests/Coherence.Tests/Data/dotnet/";
            FileStream fs = new FileStream(fileDir+sFileName+".data", FileMode.OpenOrCreate);
            DataWriter dw = new DataWriter(fs);
            return dw;
        }

/*
* Not currently supported

        [Test]
        public void TestBoolean()
        {
            IPofReader pofReader = initPofReader("Boolean");
            Assert.AreEqual(1, pofReader.ReadByte(0));
        }
*/
        [Test]
        public void TestByte()
        {
            IPofReader pofReader = initPofReader("Byte");
            Assert.AreEqual(1,   pofReader.ReadByte(0));
            Assert.AreEqual(0,   pofReader.ReadByte(0));
            Assert.AreEqual(200, pofReader.ReadByte(0));
            Assert.AreEqual(255, pofReader.ReadByte(0));
        }

        [Test]
        public void TestChar()
        {
            IPofReader pofReader = initPofReader("Char");
            Assert.AreEqual('f', pofReader.ReadChar(0));
            Assert.AreEqual('0', pofReader.ReadChar(0));
        }

        [Test]
        public void TestInt16()
        {
            IPofReader pofReader = initPofReader("Int16");
            Assert.AreEqual((Int16) (-1),   pofReader.ReadInt16(0));
            Assert.AreEqual((Int16) 0,      pofReader.ReadInt16(0));
            Assert.AreEqual(Int16.MaxValue, pofReader.ReadInt16(0));
        }

        [Test]
        public void TestInt32()
        {
            IPofReader pofReader = initPofReader("Int32");
            Assert.AreEqual(255,            pofReader.ReadInt32(0));
            Assert.AreEqual(-12345,         pofReader.ReadInt32(0));
            Assert.AreEqual(Int32.MaxValue, pofReader.ReadInt32(0));
        }

        [Test]
        public void TestInt64()
        {
            IPofReader pofReader = initPofReader("Int64");
            Assert.AreEqual(-1L,            pofReader.ReadInt64(0));
            Assert.AreEqual(Int64.MaxValue, pofReader.ReadInt64(0));
        }

        [Test]
        public void TestInt128()
        {
            // Test data:
            //   First byte array is equivalent to BigInteger.TEN
            byte[] b1 = { 0x0a };
            //   Second byte array is equivalent to BigInteger("55 5555 5555 5555 5555")
            //   (spaces are there for human parsing).
            //   Equivalent to hex: 0x 07 b5 ba d5 95 e2 38 e3
            byte[] b2 = { 0x07, 0xb5, 0xba, 0xd5, 0x95, 0xe2, 0x38, 0xe3 };

            IPofReader pofReader = initPofReader("Int128");
            Assert.AreEqual(new RawInt128(b1), pofReader.ReadRawInt128(0));
            Assert.AreEqual(new RawInt128(b2), pofReader.ReadRawInt128(0));
        }

        [Test]
        public void TestDec32()
        {
            IPofReader pofReader = initPofReader("Dec32");

            Assert.AreEqual(new Decimal((Int32) 99999),                    pofReader.ReadDecimal(0));
            Assert.AreEqual(new Decimal((Int32) 9999999, 0, 0, false, 0),  pofReader.ReadDecimal(0));
            Assert.AreEqual(new Decimal((Int32) 9999999, 0, 0, false, 28), pofReader.ReadDecimal(0));
        }

        [Test]
        public void TestDec64()
        {
            IPofReader pofReader = initPofReader("Dec64");

            Decimal d2  = new Decimal(9999999999999999);
            int[] value = Decimal.GetBits(d2);
            Decimal d3  = new Decimal(value[0], value[1], value[3], false, 28);

            Assert.AreEqual(new Decimal(9999999999), pofReader.ReadDecimal(0));
            Assert.AreEqual(d2,                      pofReader.ReadDecimal(0));
            Assert.AreEqual(d3,                      pofReader.ReadDecimal(0));
        }

        [Test]
        public void TestDec128()
        {
            IPofReader pofReader = initPofReader("Dec128");

            Decimal d1  = Decimal.Add(new Decimal(Int64.MaxValue), Decimal.One);
            int[] value = Decimal.GetBits(d1);
            Decimal d2  = new Decimal(value[0], value[1], value[2], false, 28);

            Assert.AreEqual(Decimal.MaxValue, pofReader.ReadDecimal(0));
            Assert.AreEqual(d1, pofReader.ReadDecimal(0));
            Assert.AreEqual(d2, pofReader.ReadDecimal(0));
        }

        [Test]
        public void TestSingle()
        {
            IPofReader pofReader = initPofReader("Single");
            Assert.AreEqual(100F, pofReader.ReadSingle(0));
            Assert.AreEqual(10000.1978F, pofReader.ReadSingle(0));
            Assert.AreEqual(-1.0F, pofReader.ReadSingle(0));
            Assert.AreEqual(Single.NaN, pofReader.ReadSingle(0));
            Assert.AreEqual(Single.NegativeInfinity, pofReader.ReadSingle(0));
            Assert.AreEqual(1.0F, pofReader.ReadSingle(0));
            Assert.AreEqual(0.9999F, pofReader.ReadSingle(0));
            Assert.AreEqual(Single.MaxValue, pofReader.ReadSingle(0));
        }

        [Test]
        public void TestDouble()
        {
            IPofReader pofReader = initPofReader("Double");
            Assert.AreEqual(100.0, pofReader.ReadDouble(0));
            Assert.AreEqual(100000.999, pofReader.ReadDouble(0));
            Assert.AreEqual(0.00001, pofReader.ReadDouble(0));
            Assert.AreEqual(Double.PositiveInfinity, pofReader.ReadDouble(0));
            Assert.AreEqual(Double.MaxValue, pofReader.ReadDouble(0));
        }

        [Test]
        public void TestBooleanArray()
        {
            IPofReader pofReader = initPofReader("BooleanArray");
            Boolean[] res = pofReader.ReadBooleanArray(0);
            Assert.AreEqual(true, res[0]);
            Assert.AreEqual(false, res[1]);
            Assert.AreEqual(false, res[2]);
        }

        [Test]
        public void TestByteArray()
        {
            IPofReader pofReader = initPofReader("ByteArray");
            Byte[] res = pofReader.ReadByteArray(0);
            Assert.AreEqual(1, res[0]);
            Assert.AreEqual(101, res[1]);
            Assert.AreEqual(255, res[2]);
            //Assert.AreEqual(-100, res[3]);
        }

        [Test]
        public void TestCharacterArray()
        {
            IPofReader pofReader = initPofReader("CharacterArray");
            Char[] res = pofReader.ReadCharArray(0);
            Assert.AreEqual('0', res[0]);
            Assert.AreEqual('1', res[1]);
            Assert.AreEqual(Char.MaxValue, res[2]);
            Assert.AreEqual('%', res[3]);
        }

        [Test]
        public void TestInt16Array()
        {
            IPofReader pofReader = initPofReader("Int16Array");
            Int16[] res = pofReader.ReadInt16Array(0);
            Assert.AreEqual(0, res[0]);
            Assert.AreEqual(-1, res[1]);
            Assert.AreEqual(1, res[2]);
            Assert.AreEqual(57, res[3]);
            Assert.AreEqual(-100, res[4]);
            Assert.AreEqual(Int16.MinValue, res[5]);
        }

        [Test]
        public void TestInt32Array()
        {
            IPofReader pofReader = initPofReader("Int32Array");
            Int32[] res = pofReader.ReadInt32Array(0);
            Assert.AreEqual(0, res[0]);
            Assert.AreEqual(-1, res[1]);
            Assert.AreEqual(1, res[2]);
            Assert.AreEqual(100, res[3]);
            Assert.AreEqual(-8, res[4]);
            Assert.AreEqual(Int32.MinValue, res[5]);
        }

        [Test]
        public void TestInt64Array()
        {
            IPofReader pofReader = initPofReader("Int64Array");
            Int64[] res = pofReader.ReadInt64Array(0);
            Assert.AreEqual(0, res[0]);
            Assert.AreEqual(-1, res[1]);
            Assert.AreEqual(1, res[2]);
            Assert.AreEqual(12345678, res[3]);
            Assert.AreEqual(Int64.MinValue, res[4]);
        }

        [Test]
        public void TestSingleArray()
        {
            IPofReader pofReader = initPofReader("SingleArray");
            Single[] res = pofReader.ReadSingleArray(0);
            Assert.AreEqual(-1, res[0]);
            Assert.AreEqual(0, res[1]);
            Assert.AreEqual(1, res[2]);
            Assert.AreEqual(-0.999F, res[3]);
            Assert.AreEqual(Single.NaN, res[4]);
            Assert.AreEqual(Single.NegativeInfinity, res[5]);
            Assert.AreEqual(Single.MaxValue, res[6]);
        }

        [Test]
        public void TestDoubleArray()
        {
            IPofReader pofReader = initPofReader("DoubleArray");
            Double[] res = pofReader.ReadDoubleArray(0);
            Assert.AreEqual(-1, res[0]);
            Assert.AreEqual(0, res[1]);
            Assert.AreEqual(1, res[2]);
            Assert.AreEqual(11.123, res[3]);
            Assert.AreEqual(-0.999, res[4]);
            Assert.AreEqual(Double.NaN, res[5]);
            Assert.AreEqual(Double.NegativeInfinity, res[6]);
            Assert.AreEqual(Double.MaxValue, res[7]);
        }

        [Test]
        public void TestString()
        {
            IPofReader pofReader = initPofReader("String");
            Assert.AreEqual("coherence", pofReader.ReadString(0));

        }

        [Test]
        public void TestDateTime()
        {
            IPofReader pofReader = initPofReader("DateTime");
            Assert.AreEqual(new DateTime(2006, 9, 3), pofReader.ReadDateTime(0));
            Assert.AreEqual(new DateTime(2006, 9, 4, 14, 6, 11), pofReader.ReadDateTime(0));
            Assert.AreEqual(new DateTime(2006, 9, 20, 14, 6, 11), pofReader.ReadDateTime(0));
            Assert.AreEqual(new DateTime(2006, 9, 30, 14, 6, 11), pofReader.ReadDateTime(0));

        }

        [Test]
        public void TestDate()
        {
            IPofReader pofReader = initPofReader("Date");
            Assert.AreEqual(new DateTime(2006, 9, 4), pofReader.ReadDate(0));
            Assert.AreEqual(new DateTime(2006, 9, 8), pofReader.ReadDate(0));
            Assert.AreEqual(new DateTime(2006, 9, 20), pofReader.ReadDate(0));
            Assert.AreEqual(new DateTime(2006, 9, 30), pofReader.ReadDate(0));
        }

        [Test]
        public void TestTime()
        {
            IPofReader pofReader = initPofReader("Time");
            Assert.AreEqual(new RawTime(14, 49, 10, 0, false), pofReader.ReadRawTime(0));
        }

        [Test]
        public void TestDateTimeWithZone()
        {
            IPofReader pofReader = initPofReader("DateTimeWithZone");
            DateTime dt = new DateTime(2006, 9, 4, 12, 6, 30, DateTimeKind.Utc);
            Assert.AreEqual(dt, pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(new DateTime(2006, 9, 25, 16, 44, 30, DateTimeKind.Utc), pofReader.ReadUniversalDateTime(0));
        }

        [Test]
        public void TestTimeWithZone()
        {
            IPofReader pofReader = initPofReader("TimeWithZone");
            RawTime t = new RawTime(14, 11, 34, 0, 2, 0);
            Assert.AreEqual(t, pofReader.ReadRawTime(0));
        }

        [Test]
        public void TestYearMonthInterval()
        {
            IPofReader pofReader = initPofReader("YearMonthInterval");
            RawYearMonthInterval ymi = new RawYearMonthInterval(4, 7);
            RawYearMonthInterval res = pofReader.ReadRawYearMonthInterval(0);
            Assert.AreEqual(ymi.Years, res.Years);
            Assert.AreEqual(ymi.Months, res.Months);

        }

        [Test]
        public void TestDayTimeInterval()
        {
            IPofReader pofReader = initPofReader("DayTimeInterval");
            TimeSpan ts = new TimeSpan(4, 3, 2, 1, 10);
            TimeSpan res = pofReader.ReadDayTimeInterval(0);
            Assert.AreEqual(ts.Hours, res.Hours);
            Assert.AreEqual(ts.Minutes, res.Minutes);
            Assert.AreEqual(ts.Seconds, res.Seconds);
            Assert.AreEqual(ts.Milliseconds, res.Milliseconds);

        }

        [Test]
        public void TestTimeInterval()
        {
            IPofReader pofReader = initPofReader("TimeInterval");
            TimeSpan ts = new TimeSpan(2, 15, 55);
            TimeSpan res = pofReader.ReadTimeInterval(0);
            Assert.AreEqual(ts.Hours, res.Hours);
            Assert.AreEqual(ts.Minutes, res.Minutes);
            Assert.AreEqual(ts.Seconds, res.Seconds);

            ts = new TimeSpan(0, 3, 15, 10, 11);
            res = pofReader.ReadTimeInterval(0);
            Assert.AreEqual(ts.Hours, res.Hours);
            Assert.AreEqual(ts.Minutes, res.Minutes);
            Assert.AreEqual(ts.Seconds, res.Seconds);

        }

        [Test]
        public void TestMaxDecimal128()
        {
            // test that tries to deserialize a value that is too
            // big for the native Decimal. Expecting an exception.
            IPofReader pofReader = initPofReader("MaxDec128");
            try {
                Assert.AreEqual(Decimal.MaxValue, pofReader.ReadDecimal(0));
                }
            catch (System.OverflowException)
                {
                // good. return.
                return;
                }
            // ooh. Bad.  Throw exception
            Assert.Fail("Expected System.OverflowException for value greater than Decimal maximum.");

        }

        /*---------Utility methods for writing POF data----------------------*/

        private void writeByte()
        {
            DataWriter dw         = initPofWriter("Byte");
            WritingPofHandler wfh = new WritingPofHandler(dw);
            PofStreamWriter psw   = new PofStreamWriter(wfh, new SimplePofContext());

            psw.WriteByte(0, 1);
            psw.WriteByte(0, 0);
            psw.WriteByte(0, 200);
            psw.WriteByte(0, 255);

            dw.Flush();
            dw.Close();
        }

        private void writeChar()
        {
            DataWriter dw         = initPofWriter("Char");
            WritingPofHandler wfh = new WritingPofHandler(dw);
            PofStreamWriter psw   = new PofStreamWriter(wfh, new SimplePofContext());

            psw.WriteChar(0, 'f');
            psw.WriteChar(0, '0');
         
            dw.Flush();
            dw.Close();
        }

        private void writeInt16()
        {
            DataWriter dw         = initPofWriter("Int16");
            WritingPofHandler wfh = new WritingPofHandler(dw);
            PofStreamWriter psw   = new PofStreamWriter(wfh, new SimplePofContext());

            psw.WriteInt16(0, (Int16) (-1));
            psw.WriteInt16(0, (Int16) 0);            
            psw.WriteInt16(0, Int16.MaxValue);
            
            dw.Flush();
            dw.Close();
        }

        private void writeInt32()
        {
            DataWriter dw         = initPofWriter("Int32");
            WritingPofHandler wfh = new WritingPofHandler(dw);
            PofStreamWriter psw   = new PofStreamWriter(wfh, new SimplePofContext());

            psw.WriteInt32(0, 255);
            psw.WriteInt32(0, -12345);            
            psw.WriteInt32(0, Int32.MaxValue);
            
            dw.Flush();
            dw.Close();
        }

        private void writeInt64()
        {
            DataWriter dw         = initPofWriter("Int64");
            WritingPofHandler wfh = new WritingPofHandler(dw);
            PofStreamWriter psw   = new PofStreamWriter(wfh, new SimplePofContext());

            psw.WriteInt64(0, -1L);          
            psw.WriteInt64(0, Int64.MaxValue);
            
            dw.Flush();
            dw.Close();
        }

        private void writeInt128()
        {
            DataWriter dw         = initPofWriter("Int128");
            WritingPofHandler wfh = new WritingPofHandler(dw);
            PofStreamWriter psw   = new PofStreamWriter(wfh, new SimplePofContext());

            // Test data:
            //   First byte array is equivalent to BigInteger.TEN
            byte[] b1 = { 0x0a };

            //   Second byte array is equivalent to BigInteger("55 5555 5555 5555 5555")
            //   (spaces are there for human parsing).
            //   Equivalent to hex: 0x 07 b5 ba d5 95 e2 38 e3
            byte[] b2 = { 0x07, 0xb5, 0xba, 0xd5, 0x95, 0xe2, 0x38, 0xe3 };

            RawInt128 rawInt1 = new RawInt128(b1);
            RawInt128 rawInt2 = new RawInt128(b2);

            psw.WriteRawInt128(0, rawInt1);          
            psw.WriteRawInt128(0, rawInt2);
            
            dw.Flush();
            dw.Close();
        }

        private void writeDecimal32()
        {
            DataWriter dw         = initPofWriter("Dec32");
            WritingPofHandler wfh = new WritingPofHandler(dw);
            PofStreamWriter psw   = new PofStreamWriter(wfh, new SimplePofContext());

            psw.WriteDecimal(0, new Decimal(99999,   0, 0, false, 0));
            psw.WriteDecimal(0, new Decimal(9999999, 0, 0, false, 0));
            psw.WriteDecimal(0, new Decimal(9999999, 0, 0, false, 28));

            dw.Flush();
            dw.Close();
        }

        private void writeDecimal64()
        {     
            DataWriter dw         = initPofWriter("Dec64");
            WritingPofHandler wfh = new WritingPofHandler(dw);
            PofStreamWriter psw   = new PofStreamWriter(wfh, new SimplePofContext());

            psw.WriteDecimal(0, new Decimal(9999999999));        

            Decimal d2  = new Decimal(9999999999999999);
            psw.WriteDecimal(0, d2);        

            int[] value = Decimal.GetBits(d2);
            psw.WriteDecimal(0, new Decimal(value[0], value[1], value[3], false, 28)); 
       
            dw.Flush();
            dw.Close();
        }

        private void writeDecimal128()
        {
            DataWriter dw         = initPofWriter("Dec128");
            WritingPofHandler wfh = new WritingPofHandler(dw);
            PofStreamWriter psw   = new PofStreamWriter(wfh, new SimplePofContext());

            psw.WriteDecimal(0, Decimal.MaxValue);
        
            Decimal d1  = Decimal.Add(new Decimal(Int64.MaxValue), Decimal.One);
            psw.WriteDecimal(0, d1);        

            // Scale the maximum integer plus one value
            int[] value = Decimal.GetBits(d1);
            psw.WriteDecimal(0, new Decimal(value[0], value[1], value[3], false, 28));  
    
            dw.Flush();
            dw.Close();
        }


        /*-------------main method for writing data files-----------------------------*/
        /*
        *  writes the POF data files generated by .net.  This class is build into
        *  the test dll, and as a standalone exe. To run this so the data files are
        *  generated:
        *
        *  - p4 edit the data files in .\main.net\tests\Coherence.Tests\Data\dotnet
        *  - cd .\main.net\build\Coherence.Tests.2008\{Release|Debug}
        *  - PofStreamTests.exe
        *  
        */
        static void Main() 
        {
            PofStreamTests pofTest = new PofStreamTests();
            pofTest.writeByte();
            pofTest.writeChar();
            pofTest.writeInt16();
            pofTest.writeInt32();
            pofTest.writeInt64();
            pofTest.writeInt128();
            pofTest.writeDecimal32();
            pofTest.writeDecimal64();
            pofTest.writeDecimal128();
        }

    }
}