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
        private PofStreamReader InitPofReader(String typeName)
        {
            Stream stream = GetType().Assembly.GetManifestResourceStream("Tangosol.Data.Java." + typeName + ".data");
            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofreader = new PofStreamReader(reader, new SimplePofContext());
            return pofreader;
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
            IPofReader pofReader = InitPofReader("Byte");
            Assert.AreEqual(1,   pofReader.ReadByte(0));
            Assert.AreEqual(0,   pofReader.ReadByte(0));
            Assert.AreEqual(200, pofReader.ReadByte(0));
            Assert.AreEqual(255, pofReader.ReadByte(0));
        }

        [Test]
        public void TestChar()
        {
            IPofReader pofReader = InitPofReader("Char");
            Assert.AreEqual('f', pofReader.ReadChar(0));
            Assert.AreEqual('0', pofReader.ReadChar(0));
        }

        [Test]
        public void TestInt16()
        {
            IPofReader pofReader = InitPofReader("Int16");
            Assert.AreEqual((Int16) (-1),   pofReader.ReadInt16(0));
            Assert.AreEqual((Int16) 0,      pofReader.ReadInt16(0));
            Assert.AreEqual(Int16.MaxValue, pofReader.ReadInt16(0));
        }

        [Test]
        public void TestInt32()
        {
            IPofReader pofReader = InitPofReader("Int32");
            Assert.AreEqual(255,            pofReader.ReadInt32(0));
            Assert.AreEqual(-12345,         pofReader.ReadInt32(0));
            Assert.AreEqual(Int32.MaxValue, pofReader.ReadInt32(0));
        }

        [Test]
        public void TestInt64()
        {
            IPofReader pofReader = InitPofReader("Int64");
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

            IPofReader pofReader = InitPofReader("Int128");
            Assert.AreEqual(new RawInt128(b1), pofReader.ReadRawInt128(0));
            Assert.AreEqual(new RawInt128(b2), pofReader.ReadRawInt128(0));
        }

        [Test]
        public void TestDec32()
        {
            IPofReader pofReader = InitPofReader("Dec32");

            Assert.AreEqual(new Decimal((Int32) 99999),                    pofReader.ReadDecimal(0));
            Assert.AreEqual(new Decimal((Int32) 9999999, 0, 0, false, 0),  pofReader.ReadDecimal(0));
            Assert.AreEqual(new Decimal((Int32) 9999999, 0, 0, false, 28), pofReader.ReadDecimal(0));
        }

        [Test]
        public void TestDec64()
        {
            IPofReader pofReader = InitPofReader("Dec64");

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
            IPofReader pofReader = InitPofReader("Dec128");

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
            IPofReader pofReader = InitPofReader("Single");
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
            IPofReader pofReader = InitPofReader("Double");
            Assert.AreEqual(100.0, pofReader.ReadDouble(0));
            Assert.AreEqual(100000.999, pofReader.ReadDouble(0));
            Assert.AreEqual(0.00001, pofReader.ReadDouble(0));
            Assert.AreEqual(Double.PositiveInfinity, pofReader.ReadDouble(0));
            Assert.AreEqual(Double.MaxValue, pofReader.ReadDouble(0));
        }

        [Test]
        public void TestBooleanArray()
        {
            IPofReader pofReader = InitPofReader("BooleanArray");
            Boolean[] res = pofReader.ReadBooleanArray(0);
            Assert.AreEqual(true, res[0]);
            Assert.AreEqual(false, res[1]);
            Assert.AreEqual(false, res[2]);
        }

        [Test]
        public void TestByteArray()
        {
            IPofReader pofReader = InitPofReader("ByteArray");
            Byte[] res = pofReader.ReadByteArray(0);
            Assert.AreEqual(1, res[0]);
            Assert.AreEqual(101, res[1]);
            Assert.AreEqual(255, res[2]);
            //Assert.AreEqual(-100, res[3]);
        }

        [Test]
        public void TestCharacterArray()
        {
            IPofReader pofReader = InitPofReader("CharacterArray");
            Char[] res = pofReader.ReadCharArray(0);
            Assert.AreEqual('0', res[0]);
            Assert.AreEqual('1', res[1]);
            Assert.AreEqual(Char.MaxValue, res[2]);
            Assert.AreEqual('%', res[3]);
        }

        [Test]
        public void TestInt16Array()
        {
            IPofReader pofReader = InitPofReader("Int16Array");
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
            IPofReader pofReader = InitPofReader("Int32Array");
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
            IPofReader pofReader = InitPofReader("Int64Array");
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
            IPofReader pofReader = InitPofReader("SingleArray");
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
            IPofReader pofReader = InitPofReader("DoubleArray");
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
            IPofReader pofReader = InitPofReader("String");
            Assert.AreEqual("coherence", pofReader.ReadString(0));

        }

        [Test]
        public void TestDateTime()
        {
            IPofReader pofReader = InitPofReader("DateTime");
            Assert.AreEqual(new DateTime(2006, 9, 3), pofReader.ReadDateTime(0));
            Assert.AreEqual(new DateTime(2006, 9, 4, 14, 6, 11), pofReader.ReadDateTime(0));
            Assert.AreEqual(new DateTime(2006, 9, 20, 14, 6, 11), pofReader.ReadDateTime(0));
            Assert.AreEqual(new DateTime(2006, 9, 30, 14, 6, 11), pofReader.ReadDateTime(0));

        }

        [Test]
        public void TestDate()
        {
            IPofReader pofReader = InitPofReader("Date");
            Assert.AreEqual(new DateTime(2006, 9, 4), pofReader.ReadDate(0));
            Assert.AreEqual(new DateTime(2006, 9, 8), pofReader.ReadDate(0));
            Assert.AreEqual(new DateTime(2006, 9, 20), pofReader.ReadDate(0));
            Assert.AreEqual(new DateTime(2006, 9, 30), pofReader.ReadDate(0));
        }

        [Test]
        public void TestTime()
        {
            IPofReader pofReader = InitPofReader("Time");
            Assert.AreEqual(new RawTime(14, 49, 10, 0, false), pofReader.ReadRawTime(0));
        }

        [Test]
        public void TestDateTimeWithZone()
        {
            IPofReader pofReader = InitPofReader("DateTimeWithZone");
            DateTime dt = new DateTime(2006, 9, 4, 12, 6, 30, DateTimeKind.Utc);
            Assert.AreEqual(dt, pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(new DateTime(2006, 9, 25, 16, 44, 30, DateTimeKind.Utc), pofReader.ReadUniversalDateTime(0));
        }

        [Test]
        public void TestTimeWithZone()
        {
            IPofReader pofReader = InitPofReader("TimeWithZone");
            RawTime t = new RawTime(14, 11, 34, 0, 2, 0);
            Assert.AreEqual(t, pofReader.ReadRawTime(0));
        }

        [Test]
        public void TestYearMonthInterval()
        {
            IPofReader pofReader = InitPofReader("YearMonthInterval");
            RawYearMonthInterval ymi = new RawYearMonthInterval(4, 7);
            RawYearMonthInterval res = pofReader.ReadRawYearMonthInterval(0);
            Assert.AreEqual(ymi.Years, res.Years);
            Assert.AreEqual(ymi.Months, res.Months);

        }

        [Test]
        public void TestDayTimeInterval()
        {
            IPofReader pofReader = InitPofReader("DayTimeInterval");
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
            IPofReader pofReader = InitPofReader("TimeInterval");
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
            IPofReader pofReader = InitPofReader("MaxDec128");
            try 
            {
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
    }
}