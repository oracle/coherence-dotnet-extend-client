/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using NUnit.Framework;

namespace Tangosol.Util
{
    [TestFixture]
    public class NumberUtilsTests
    {
        [Test]
        public void URShiftTest()
        {
            Assert.AreEqual(8, NumberUtils.URShift(16, 1));
            Assert.AreEqual(1, NumberUtils.URShift(16, 4));
            int i = -100;
            uint ui = (uint) i;
            Assert.AreEqual(ui>>1, NumberUtils.URShift(i, 1));
            Assert.AreEqual(ui>>2, NumberUtils.URShift(i, 2));
            Assert.AreEqual(ui>>8, NumberUtils.URShift(i, 8));
            i = -1;
            ui = (uint) i;
            Assert.AreEqual(ui>>3, NumberUtils.URShift(i, 3));
            long l = Int64.MaxValue;
            ulong ul = (ulong) l;
            Assert.AreEqual(ul >> 1, NumberUtils.URShift(l, 1));
            Assert.AreEqual(ul >> 2, NumberUtils.URShift(l, 2));
            Assert.AreEqual(ul >> 8, NumberUtils.URShift(l, 8));
            l = Int64.MinValue;
            ul = (ulong)l;
            Assert.AreEqual(ul >> 1, NumberUtils.URShift(l, 1));
            Assert.AreEqual(ul >> 2, NumberUtils.URShift(l, 2));
            Assert.AreEqual(ul >> 8, NumberUtils.URShift(l, 8));
            l = -1L;
            ul = (ulong)l;
            Assert.AreEqual(ul >> 1, NumberUtils.URShift(l, 1));
            Assert.AreEqual(ul >> 2, NumberUtils.URShift(l, 2));
            Assert.AreEqual(ul >> 8, NumberUtils.URShift(l, 8));

        }

        [Test]
        public void BitsToSingleTest()
        {
            Single s = 121345.22256f;
            Assert.AreEqual(s, NumberUtils.Int32BitsToSingle(NumberUtils.SingleToInt32Bits(s)));
            Assert.AreEqual(Single.NaN, NumberUtils.Int32BitsToSingle(NumberUtils.SingleToInt32Bits(Single.NaN)));
            Assert.AreEqual(Single.MaxValue,
                            NumberUtils.Int32BitsToSingle(NumberUtils.SingleToInt32Bits(Single.MaxValue)));
            Assert.AreEqual(Single.NegativeInfinity,
                            NumberUtils.Int32BitsToSingle(NumberUtils.SingleToInt32Bits(Single.NegativeInfinity)));

            Double d = 1234533.00009;
            Assert.AreEqual(d, NumberUtils.Int64BitsToDouble(NumberUtils.DoubleToInt64Bits(d)));
            Assert.AreEqual(Double.NaN, NumberUtils.Int64BitsToDouble(NumberUtils.DoubleToInt64Bits(Double.NaN)));
            Assert.AreEqual(Double.MaxValue,
                            NumberUtils.Int64BitsToDouble(NumberUtils.DoubleToInt64Bits(Double.MaxValue)));
            Assert.AreEqual(Double.NegativeInfinity,
                            NumberUtils.Int64BitsToDouble(NumberUtils.DoubleToInt64Bits(Double.NegativeInfinity)));
        }

        [Test]
        public void DecimalTest()
        {
            decimal d = 0.000000M;
            Assert.AreEqual(d, NumberUtils.GetUnscaledValue(d));

            d = 12345.6789M;
            decimal expected = 123456789M;
            Assert.AreEqual(expected, NumberUtils.GetUnscaledValue(d));

            d = 1.0001M;
            expected = 10001M;
            Assert.AreEqual(expected, NumberUtils.GetUnscaledValue(d));
        }

        public void ChangeEndian()
        {
            Int16 i16 = 1234;
            Assert.AreEqual(i16, NumberUtils.ChangeEndian(NumberUtils.ChangeEndian(i16)));
            Int32 i32 = -123487;
            Assert.AreEqual(i32, NumberUtils.ChangeEndian(NumberUtils.ChangeEndian(i32)));
            Int64 i64 = -122234536L;
            Assert.AreEqual(i64, NumberUtils.ChangeEndian(NumberUtils.ChangeEndian(i64)));
            UInt16 ui16 = 11113;
            Assert.AreEqual(ui16, NumberUtils.ChangeEndian(NumberUtils.ChangeEndian(ui16)));
            UInt32 ui32 = 123487;
            Assert.AreEqual(ui32, NumberUtils.ChangeEndian(NumberUtils.ChangeEndian(ui32)));
            UInt64 ui64 = 122234536L;
            Assert.AreEqual(ui64, NumberUtils.ChangeEndian(NumberUtils.ChangeEndian(ui64)));
        }

        public void ParseHexTest()
        {
            Assert.AreEqual(0, NumberUtils.ParseHex("").Length);
            Assert.AreEqual(0, NumberUtils.ParseHex(null).Length);
            String number = "0xafff42";
            byte[] res = NumberUtils.ParseHex(number);
            Assert.AreEqual(0xaf, res[0]);
            Assert.AreEqual(0xff, res[1]);
            Assert.AreEqual(0x42, res[2]);

            Assert.AreEqual("0x0A", NumberUtils.ToHexEscape(10));
            Assert.AreEqual("0xAFFF42", NumberUtils.ToHexEscape(res));

            number = "123E3A";
            res = NumberUtils.ParseHex(number);
            Assert.AreEqual(0x12, res[0]);
            Assert.AreEqual(0x3e, res[1]);
            Assert.AreEqual(0x3a, res[2]);
        }

        [Test]
        public void ToHexTest()
        {
            Assert.AreEqual(NumberUtils.ToHex(0), "00");
            Assert.AreEqual(NumberUtils.ToHex(10), "0A");
            Assert.AreEqual(NumberUtils.ToHex(100), "64");
            Assert.AreEqual(NumberUtils.ToHex(1000), "E8"); //'cuts' off first byte of int
        }
    }
}
