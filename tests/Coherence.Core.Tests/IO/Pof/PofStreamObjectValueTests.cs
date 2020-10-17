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
    public class PofStreamObjectValueTests
    {
        [Test]
        public void TestNullable()
        {
            initPOFWriter();

            int? i = 5;
            int? n = null;

            pofWriter.WriteObject(0, i);
            pofWriter.WriteObject(0, n);

            initPOFReader();
            Assert.AreEqual(i, pofReader.ReadObject(0));
            Assert.AreEqual(n, pofReader.ReadObject(0));
        }

        [Test]
        public void TestString()
        {
            initPOFWriter();
            pofWriter.WriteString(0, String.Empty);
            pofWriter.WriteString(0, "test");
            pofWriter.WriteString(0, "Test");
            pofWriter.WriteString(0, null);
            pofWriter.WriteString(0, " Test@");
            pofWriter.WriteString(0, String.Empty);

            char[] cArray = new char[] { 'a', Char.MinValue };
            pofWriter.WriteCharArray(0, cArray);

            object[] objArray = new object[] { 'a', 'b', (Int16)12 };
            pofWriter.WriteArray(0, objArray);

            initPOFReader();
            Assert.AreEqual(String.Empty, pofReader.ReadString(0));
            Assert.AreEqual("test", pofReader.ReadString(0));
            Assert.AreEqual("Test", pofReader.ReadString(0));
            Assert.AreEqual(null, pofReader.ReadString(0));
            Assert.AreEqual(" Test@", pofReader.ReadString(0));
            Assert.AreEqual(String.Empty, pofReader.ReadString(0));
            Assert.AreEqual(new String(cArray), pofReader.ReadString(0));
            Assert.AreEqual("ab" + (Char)12, pofReader.ReadString(0));

        }

        [Test]
        public void TestReadStringWithException()
        {
            initPOFWriter();
            pofWriter.WriteDouble(0, Double.NegativeInfinity);
            initPOFReader();
            Assert.That(() => pofReader.ReadString(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteStringWithException()
        {
            String str = "test";
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteString(0, str), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestDateTime()
        {
            DateTime dt = new DateTime(1978, 4, 25, 7, 5, 10);
            RawTime rawTime = new RawTime(11, 12, 30, 100, true);
            initPOFWriter();
            pofWriter.WriteDateTime(0, dt);
            pofWriter.WriteDateTime(0, new DateTime(1982, 11, 10, 12, 14, 20));
            pofWriter.WriteDateTime(0, new DateTime(1999, 3, 4, 5, 6, 11, 110));
            pofWriter.WriteDateTime(0, DateTime.MinValue);
            pofWriter.WriteDate(0, dt.Date);
            pofWriter.WriteRawTime(0, rawTime);

            initPOFReader();
            Assert.AreEqual(dt, pofReader.ReadDateTime(0));
            Assert.AreEqual(new DateTime(1982, 11, 10, 12, 14, 20), pofReader.ReadDateTime(0));
            Assert.AreEqual(new DateTime(1999, 3, 4, 5, 6, 11, 110), pofReader.ReadDateTime(0));
            Assert.AreEqual(DateTime.MinValue, pofReader.ReadDateTime(0));

            DateTime result = pofReader.ReadDateTime(0);
            Assert.AreEqual(dt.Date, result.Date);

            Assert.AreEqual(new DateTime(1, 1, 1,
                                         rawTime.Hour,
                                         rawTime.Minute,
                                         rawTime.Second,
                                         rawTime.Nanosecond / 1000000),
                            pofReader.ReadDateTime(0));
        }

        [Test]
        public void TestReadDateTimeWithException()
        {
            initPOFWriter();
            pofWriter.WriteString(0, "string_datetime");
            initPOFReader();
            Assert.That(() => pofReader.ReadDateTime(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteDateTimeWithException()
        {
            DateTime dt = new DateTime(1978, 4, 25, 7, 5, 10, 110);
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteDateTime(0, dt), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestLocalDateTime()
        {
            DateTime dtUtc = new DateTime(2004, 8, 14, 13, 43, 0, DateTimeKind.Utc);
            DateTime dtUnsp = new DateTime(2004, 3, 13, 19, 53, 10, DateTimeKind.Unspecified);
            DateTime dtLoc = new DateTime(2002, 11, 1, 23, 33, 40, DateTimeKind.Local);
            DateTime dtUtc1 = new DateTime(2002, 11, 1, 23, 33, 40, DateTimeKind.Utc);

            initPOFWriter();
            pofWriter.WriteLocalDateTime(0, DateTime.MinValue);
            pofWriter.WriteLocalDateTime(0, DateTime.MinValue);
            pofWriter.WriteLocalDateTime(0, DateTime.MinValue);
            pofWriter.WriteLocalDateTime(0, dtUtc);
            pofWriter.WriteLocalDateTime(0, dtUtc);
            pofWriter.WriteLocalDateTime(0, dtUnsp);
            pofWriter.WriteLocalDateTime(0, dtLoc);
            pofWriter.WriteLocalDateTime(0, dtLoc);
            pofWriter.WriteLocalDateTime(0, dtUtc1);

            initPOFReader();
            Assert.AreEqual(DateTime.MinValue, pofReader.ReadDateTime(0));
            Assert.AreEqual(DateTime.MinValue, pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(DateTime.MinValue, pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(dtUtc, pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(dtUtc.ToLocalTime(), pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(dtUnsp, pofReader.ReadDateTime(0));
            Assert.AreEqual(dtLoc, pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(dtLoc.ToUniversalTime(), pofReader.ReadUniversalDateTime(0));
        }

        [Test]
        public void TestLocalDateTimeWithException()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteLocalDateTime(0, DateTime.MinValue), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestUniversalDateTime()
        {
            DateTime dtUtc = new DateTime(2004, 8, 14, 13, 43, 0, DateTimeKind.Utc);
            DateTime dtUnsp = new DateTime(2004, 3, 13, 19, 53, 10, DateTimeKind.Unspecified);
            DateTime dtLoc = new DateTime(2002, 11, 1, 23, 33, 40, DateTimeKind.Local);
            DateTime dtUtc1 = new DateTime(2002, 11, 1, 23, 33, 40, DateTimeKind.Utc);

            initPOFWriter();
            pofWriter.WriteUniversalDateTime(0, DateTime.MinValue);
            pofWriter.WriteUniversalDateTime(0, DateTime.MinValue);
            pofWriter.WriteUniversalDateTime(0, DateTime.MinValue);
            pofWriter.WriteUniversalDateTime(0, dtUtc);
            pofWriter.WriteUniversalDateTime(0, dtUtc);
            pofWriter.WriteUniversalDateTime(0, dtUnsp);
            pofWriter.WriteUniversalDateTime(0, dtLoc);
            pofWriter.WriteUniversalDateTime(0, dtLoc);
            pofWriter.WriteUniversalDateTime(0, dtUtc1);

            initPOFReader();
            Assert.AreEqual(DateTime.MinValue, pofReader.ReadDateTime(0));
            Assert.AreEqual(DateTime.MinValue, pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(DateTime.MinValue, pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(dtUtc, pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(dtUtc.ToLocalTime(), pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(dtUnsp, pofReader.ReadDateTime(0));
            Assert.AreEqual(dtLoc, pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(dtLoc.ToUniversalTime(), pofReader.ReadUniversalDateTime(0));
        }

        [Test]
        public void TestUniversalDateTimeWithException()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteUniversalDateTime(0, DateTime.MinValue), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        [Ignore("fails due to 3-minute offset that appears during UTC->local->UTC conversion")]
        public void TestLocalTime()
        {
            initPOFWriter();
            pofWriter.WriteLocalTime(0, new DateTime(1, 1, 1, 7, 5, 10));
            pofWriter.WriteLocalTime(0, new DateTime(1, 1, 1, 7, 5, 10, 110));
            pofWriter.WriteLocalTime(0, DateTime.MinValue);
            pofWriter.WriteLocalTime(0, DateTime.MaxValue);

            initPOFReader();
            DateTime result = pofReader.ReadLocalDateTime(0);
            Assert.AreEqual(7, result.Hour);
            Assert.AreEqual(5, result.Minute);
            Assert.AreEqual(10, result.Second);

            result = pofReader.ReadLocalDateTime(0);
            Assert.AreEqual(7, result.Hour);
            Assert.AreEqual(5, result.Minute);
            Assert.AreEqual(10, result.Second);
            Assert.AreEqual(110, result.Millisecond);

            Assert.AreEqual(DateTime.MinValue, pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(new DateTime(1, 1, 1,
                                         DateTime.MaxValue.Hour,
                                         DateTime.MaxValue.Minute,
                                         DateTime.MaxValue.Second,
                                         DateTime.MaxValue.Millisecond),
                            pofReader.ReadLocalDateTime(0));

            initPOFWriter();

            DateTime dtUtc = new DateTime(1, 1, 1, 13, 43, 0, 99, DateTimeKind.Utc);
            DateTime dtLocal = new DateTime(1, 1, 1, 11, 24, 5, 10, DateTimeKind.Local);
            DateTime dtUnspecified = new DateTime(1, 1, 1, 17, 23, 5, 0, DateTimeKind.Unspecified);
            pofWriter.WriteLocalTime(0, dtUnspecified);
            pofWriter.WriteLocalTime(0, dtUnspecified);
            pofWriter.WriteLocalTime(0, dtUnspecified);
            pofWriter.WriteLocalTime(0, dtUtc);
            pofWriter.WriteLocalTime(0, dtUtc);
            pofWriter.WriteLocalTime(0, dtUtc);
            pofWriter.WriteLocalTime(0, dtLocal);
            pofWriter.WriteLocalTime(0, dtLocal);
            pofWriter.WriteLocalTime(0, dtLocal);

            initPOFReader();

            Assert.AreEqual(dtUnspecified, pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(dtUnspecified.ToUniversalTime(), pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(dtUnspecified, pofReader.ReadDateTime(0));

            Assert.AreEqual(dtUtc.ToLocalTime(), pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(dtUtc, pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(dtUtc.ToLocalTime(), pofReader.ReadDateTime(0));

            Assert.AreEqual(dtLocal, pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(dtLocal.ToUniversalTime(), pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(dtLocal, pofReader.ReadDateTime(0));
        }

        [Test]
        public void TestLocalTimeWithException()
        {
            DateTime t = new DateTime(1, 1, 1, 7, 5, 15, 110);
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteLocalTime(0, t), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestUniversalTime()
        {
            initPOFWriter();
            pofWriter.WriteUniversalTime(0, new DateTime(1, 1, 1, 7, 5, 10));
            pofWriter.WriteUniversalTime(0, new DateTime(1, 1, 1, 7, 5, 10, 110));
            pofWriter.WriteUniversalTime(0, DateTime.MinValue);
            pofWriter.WriteUniversalTime(0, DateTime.MaxValue);

            initPOFReader();
            DateTime result = pofReader.ReadUniversalDateTime(0);
            Assert.AreEqual(7, result.Hour);
            Assert.AreEqual(5, result.Minute);
            Assert.AreEqual(10, result.Second);

            result = pofReader.ReadUniversalDateTime(0);
            Assert.AreEqual(7, result.Hour);
            Assert.AreEqual(5, result.Minute);
            Assert.AreEqual(10, result.Second);
            Assert.AreEqual(110, result.Millisecond);

            Assert.AreEqual(DateTime.MinValue, pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(new DateTime(1, 1, 1,
                                         DateTime.MaxValue.Hour,
                                         DateTime.MaxValue.Minute,
                                         DateTime.MaxValue.Second,
                                         DateTime.MaxValue.Millisecond),
                            pofReader.ReadUniversalDateTime(0));

            initPOFWriter();
            DateTime dtUtc = new DateTime(1, 1, 1, 13, 43, 0, 99, DateTimeKind.Utc);
            DateTime dtLocal = new DateTime(1, 1, 1, 11, 24, 5, 10, DateTimeKind.Local);
            DateTime dtUnspecified = new DateTime(1, 1, 1, 17, 23, 5, 0, DateTimeKind.Unspecified);
            pofWriter.WriteUniversalTime(0, dtUnspecified);
            pofWriter.WriteUniversalTime(0, dtUnspecified);
            pofWriter.WriteUniversalTime(0, dtUnspecified);
            pofWriter.WriteUniversalTime(0, dtUtc);
            pofWriter.WriteUniversalTime(0, dtUtc);
            pofWriter.WriteUniversalTime(0, dtUtc);
            pofWriter.WriteUniversalTime(0, dtLocal);
            pofWriter.WriteUniversalTime(0, dtLocal);
            pofWriter.WriteUniversalTime(0, dtLocal);
            initPOFReader();

            Assert.AreEqual(dtUnspecified.ToLocalTime(), pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(dtUnspecified, pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(dtUnspecified, pofReader.ReadDateTime(0));

            Assert.AreEqual(dtUtc.ToLocalTime(), pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(dtUtc, pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(dtUtc, pofReader.ReadDateTime(0));

            Assert.AreEqual(dtLocal, pofReader.ReadLocalDateTime(0));
            Assert.AreEqual(dtLocal.ToUniversalTime(), pofReader.ReadUniversalDateTime(0));
            Assert.AreEqual(dtLocal.ToUniversalTime(), pofReader.ReadDateTime(0));
        }

        [Test]
        public void TestUniversalTimeWithException()
        {
            DateTime t = new DateTime(1, 1, 1, 7, 5, 15, 110);
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteUniversalTime(0, t), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestDate()
        {
            DateTime dt = new DateTime(1978, 4, 25, 7, 5, 10);
            DateTime dtLocal = new DateTime(2006, 8, 18, 11, 35, 30, DateTimeKind.Local);
            DateTime dtUtc = new DateTime(2004, 8, 14, 13, 43, 0, DateTimeKind.Utc);
            initPOFWriter();
            pofWriter.WriteDate(0, dt.Date);
            pofWriter.WriteDate(0, DateTime.MinValue);
            pofWriter.WriteDateTime(0, dt);
            pofWriter.WriteLocalDateTime(0, dtLocal);
            pofWriter.WriteUniversalDateTime(0, dtUtc);

            initPOFReader();
            DateTime result = pofReader.ReadDate(0);
            Assert.AreEqual(dt.Date, result.Date);
            Assert.AreEqual(DateTime.MinValue, pofReader.ReadDate(0));
            result = pofReader.ReadDate(0);
            Assert.AreEqual(dt.Date, result.Date);

            result = pofReader.ReadDate(0);
            Assert.AreEqual(dtLocal.Date, result.Date);

            result = pofReader.ReadDate(0);
            Assert.AreEqual(dtUtc.Date, result.Date);
        }

        [Test]
        public void TestReadDateWithException()
        {
            initPOFWriter();
            pofWriter.WriteString(0, "string_date");
            initPOFReader();
            Assert.That(() => pofReader.ReadDate(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteDateWithException()
        {
            DateTime dt = new DateTime(1978, 4, 25, 7, 5, 10, 110);
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteDate(0, dt.Date), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestTime()
        {
            initPOFWriter();
            pofWriter.WriteTime(0, new DateTime(1, 1, 1, 7, 5, 10));
            pofWriter.WriteTime(0, new DateTime(1, 1, 1, 7, 5, 10, 110));
            pofWriter.WriteTime(0, DateTime.MinValue);
            pofWriter.WriteTime(0, DateTime.MaxValue);

            initPOFReader();
            DateTime result = pofReader.ReadDateTime(0);
            Assert.AreEqual(7, result.Hour);
            Assert.AreEqual(5, result.Minute);
            Assert.AreEqual(10, result.Second);

            result = pofReader.ReadDateTime(0);
            Assert.AreEqual(7, result.Hour);
            Assert.AreEqual(5, result.Minute);
            Assert.AreEqual(10, result.Second);
            Assert.AreEqual(110, result.Millisecond);

            Assert.AreEqual(DateTime.MinValue, pofReader.ReadDateTime(0));
            Assert.AreEqual(new DateTime(1, 1, 1,
                                         DateTime.MaxValue.Hour,
                                         DateTime.MaxValue.Minute,
                                         DateTime.MaxValue.Second,
                                         DateTime.MaxValue.Millisecond),
                            pofReader.ReadDateTime(0));

            initPOFWriter();
            DateTime dtUtc = new DateTime(1, 1, 1, 13, 43, 0, 99, DateTimeKind.Utc);
            DateTime dtLocal = new DateTime(1, 1, 1, 11, 24, 5, 10, DateTimeKind.Local);
            DateTime dtUnspecified = new DateTime(1, 1, 1, 17, 23, 5, 0, DateTimeKind.Unspecified);
            pofWriter.WriteTime(0, dtUnspecified);
            pofWriter.WriteTime(0, dtUtc);
            pofWriter.WriteTime(0, dtLocal);

            initPOFReader();

            Assert.AreEqual(dtUnspecified, pofReader.ReadDateTime(0));
            Assert.AreEqual(dtUtc, pofReader.ReadDateTime(0));
            Assert.AreEqual(dtLocal, pofReader.ReadDateTime(0));
        }

        [Test]
        public void TestRawTime()
        {
            RawTime timeUtc = new RawTime(15, 55, 0, 0, true);
            RawTime timeNoZone = new RawTime(15, 55, 0, 0, false);
            RawTime timeWithOffset = new RawTime(10, 55, 0, 0, -5, 0);

            initPOFWriter();
            pofWriter.WriteRawTime(0, timeUtc);
            pofWriter.WriteRawTime(0, timeNoZone);
            pofWriter.WriteRawTime(0, timeWithOffset);

            initPOFReader();
            Assert.AreEqual(timeUtc, pofReader.ReadRawTime(0));
            Assert.AreEqual(timeNoZone, pofReader.ReadRawTime(0));
            Assert.AreEqual(timeWithOffset, pofReader.ReadRawTime(0));
        }

        [Test]
        public void TestReadTimeWithException()
        {
            initPOFWriter();
            pofWriter.WriteString(0, "string_time");
            initPOFReader();
            Assert.That(() => pofReader.ReadRawTime(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteTimeWithException()
        {
            RawTime t = new RawTime(7, 5, 10, 110, false);
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteRawTime(0, t), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestRawDateTime()
        {
            DateTime date = new DateTime(1976, 4, 19);
            RawTime timeUtc = new RawTime(15, 55, 0, 0, true);
            RawTime timeNoZone = new RawTime(15, 55, 0, 0, false);
            RawTime timeWithOffset = new RawTime(10, 55, 0, 0, -5, 0);
            RawDateTime datetimeUtc = new RawDateTime(date, timeUtc);
            RawDateTime datetimeNoZone = new RawDateTime(date, timeNoZone);
            RawDateTime datetimeWithOffset = new RawDateTime(date, timeWithOffset);

            initPOFWriter();
            pofWriter.WriteDate(0, date);
            pofWriter.WriteRawDateTime(0, datetimeUtc);
            pofWriter.WriteRawDateTime(0, datetimeNoZone);
            pofWriter.WriteRawDateTime(0, datetimeWithOffset);

            initPOFReader();
            Assert.AreEqual(date, pofReader.ReadRawDateTime(0).Date);
            Assert.AreEqual(datetimeUtc, pofReader.ReadRawDateTime(0));
            Assert.AreEqual(datetimeNoZone, pofReader.ReadRawDateTime(0));
            Assert.AreEqual(datetimeWithOffset, pofReader.ReadRawDateTime(0));
        }

        [Test]
        public void TestYearMonthInterval()
        {
            RawYearMonthInterval ymi = new RawYearMonthInterval(30, -1);
            initPOFWriter();
            pofWriter.WriteRawYearMonthInterval(0, ymi);
            pofWriter.WriteObject(0, null);

            initPOFReader();
            RawYearMonthInterval result = pofReader.ReadRawYearMonthInterval(0);
            Assert.AreEqual(ymi, result);

            Assert.AreEqual(new RawYearMonthInterval(), pofReader.ReadRawYearMonthInterval(0));
        }

        [Test]
        public void TestWriteYearMonthIntervalWithException()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteRawYearMonthInterval(0, new RawYearMonthInterval(30, 1)), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestReadYearMonthIntervalWithException()
        {
            initPOFWriter();
            pofWriter.WriteString(0, "invalid value");

            initPOFReader();
            Assert.That(() => pofReader.ReadRawYearMonthInterval(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestTimeInterval()
        {
            TimeSpan ts1 = new TimeSpan();
            TimeSpan ts2 = new TimeSpan(5, 18, 28, 39, 450);
            TimeSpan ts3 = new TimeSpan(0, 12, 0, 20);

            initPOFWriter();
            pofWriter.WriteTimeInterval(0, ts1);
            pofWriter.WriteTimeInterval(0, ts2);
            pofWriter.WriteTimeInterval(0, ts3);
            pofWriter.WriteObject(0, null);

            initPOFReader();
            Assert.AreEqual(ts1, pofReader.ReadTimeInterval(0));
            Assert.AreEqual(new TimeSpan(0, 18, 28, 39, 450), pofReader.ReadTimeInterval(0));
            Assert.AreEqual(ts3, pofReader.ReadTimeInterval(0));
            Assert.AreEqual(new TimeSpan(), pofReader.ReadTimeInterval(0));
        }


        [Test]
        public void TestReadTimeIntervalWithException()
        {
            initPOFWriter();
            pofWriter.WriteString(0, "invalid value");

            initPOFReader();
            Assert.That(() => pofReader.ReadTimeInterval(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteTimeIntervalWithException()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteTimeInterval(0, new TimeSpan(5, 14, 10, 30, 10)), Throws.TypeOf<ObjectDisposedException>());
        }


        [Test]
        public void TestDayTimeInterval()
        {
            TimeSpan ts = new DateTime(2004, 8, 14) - new DateTime(1974, 8, 24);
            TimeSpan ts1 = new TimeSpan();
            TimeSpan ts2 = new TimeSpan(5, 18, 28, 39, 450);
            initPOFWriter();
            pofWriter.WriteDayTimeInterval(0, ts);
            pofWriter.WriteDayTimeInterval(0, ts1);
            pofWriter.WriteDayTimeInterval(0, ts2);
            pofWriter.WriteObject(0, null);

            initPOFReader();
            Assert.AreEqual(10948, pofReader.ReadDayTimeInterval(0).Days);
            Assert.AreEqual(ts1, pofReader.ReadDayTimeInterval(0));
            Assert.AreEqual(ts2, pofReader.ReadDayTimeInterval(0));
            Assert.AreEqual(new TimeSpan(), pofReader.ReadDayTimeInterval(0));
        }

        [Test]
        public void TestReadDayTimeIntervalWithException()
        {
            initPOFWriter();
            pofWriter.WriteString(0, "invalid value");

            initPOFReader();
            Assert.That(() => pofReader.ReadDayTimeInterval(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteDayTimeIntervalWithException()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteDayTimeInterval(0, new TimeSpan(3, 5, 6, 7, 8)), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestDecimal()
        {
            initPOFWriter();
            pofWriter.WriteDecimal(0, 10000000000000000000000000000M);
            pofWriter.WriteDecimal(0, 792281625142643.3759354395033M);
            pofWriter.WriteDecimal(0, Decimal.MinusOne);
            pofWriter.WriteDecimal(0, 345.11M);
            pofWriter.WriteDecimal(0, Decimal.One);
            pofWriter.WriteDecimal(0, -111.56345M);
            pofWriter.WriteDecimal(0, 123456789.123456789M);
            pofWriter.WriteDecimal(0, -1.987654320123456789M);
            pofWriter.WriteDecimal(0, Decimal.Zero);
            pofWriter.WriteDecimal(0, 10000.32M);
            pofWriter.WriteDecimal(0, 9776941210.9865522210M);
            pofWriter.WriteDecimal(0, Decimal.MaxValue);
            pofWriter.WriteDecimal(0, Decimal.MinValue);
            pofWriter.WriteDecimal(0, -692162514264337593543950335M);
            pofWriter.WriteDecimal(0, -1M);
            pofWriter.WriteDecimal(0, -12345678901234571234.08M);


            initPOFReader();
            Assert.AreEqual(10000000000000000000000000000M, pofReader.ReadDecimal(0));
            Assert.AreEqual(792281625142643.3759354395033M, pofReader.ReadDecimal(0));
            Assert.AreEqual(Decimal.MinusOne, pofReader.ReadDecimal(0));
            Assert.AreEqual(345.11M, pofReader.ReadDecimal(0));
            Assert.AreEqual(Decimal.One, pofReader.ReadDecimal(0));
            Assert.AreEqual(-111.56345M, pofReader.ReadDecimal(0));
            Assert.AreEqual(123456789.123456789M, pofReader.ReadDecimal(0));
            Assert.AreEqual(-1.987654320123456789M, pofReader.ReadDecimal(0));
            Assert.AreEqual(Decimal.Zero, pofReader.ReadDecimal(0));
            Assert.AreEqual(10000.32M, pofReader.ReadDecimal(0));
            Assert.AreEqual(9776941210.9865522210M, pofReader.ReadDecimal(0));
            Assert.AreEqual(Decimal.MaxValue, pofReader.ReadDecimal(0));
            Assert.AreEqual(Decimal.MinValue, pofReader.ReadDecimal(0));
            Assert.AreEqual(-692162514264337593543950335M, pofReader.ReadDecimal(0));
            Assert.AreEqual(-1M, pofReader.ReadDecimal(0));
            Assert.AreEqual(-12345678901234571234.08M, pofReader.ReadDecimal(0));
        }

        [Test]
        public void TestDecimalOverflow()
        {
            initPOFWriter();
            pofWriter.WriteByteArray(0, new byte[300]);

            initPOFReader();
            Assert.That(() => pofReader.ReadDecimal(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestRawInt128()
        {
            initPOFWriter();
            RawInt128 input1 = new RawInt128(new byte[] { 6 });
            RawInt128 input2 = new RawInt128(new byte[] { 16, 5, 21, 9 });
            RawInt128 input3 = new RawInt128(new byte[] { 3, 6, 5 });
            RawInt128 input4 = new RawInt128(new byte[] { 1, 3, 0 });
            RawInt128 input5 = new RawInt128(new sbyte[] { 99, -50 });
            RawInt128 input6 = new RawInt128(new byte[] { 5, 7 });
            RawInt128 input7 = new RawInt128(new byte[] { 88, 64, 3 });
            RawInt128 input8 = new RawInt128(new sbyte[] { -1, -5, 127, 0, -126 });
            pofWriter.WriteRawInt128(0, input1);
            pofWriter.WriteRawInt128(0, input2);
            pofWriter.WriteRawInt128(0, input3);
            pofWriter.WriteRawInt128(0, input4);
            pofWriter.WriteRawInt128(0, input5);
            pofWriter.WriteRawInt128(0, input6);
            pofWriter.WriteRawInt128(0, input7);
            pofWriter.WriteRawInt128(0, input8);

            initPOFReader();
            RawInt128 result1 = pofReader.ReadRawInt128(0);
            RawInt128 result2 = pofReader.ReadRawInt128(0);
            RawInt128 result3 = pofReader.ReadRawInt128(0);
            RawInt128 result4 = pofReader.ReadRawInt128(0);
            RawInt128 result5 = pofReader.ReadRawInt128(0);
            RawInt128 result6 = pofReader.ReadRawInt128(0);
            RawInt128 result7 = pofReader.ReadRawInt128(0);
            RawInt128 result8 = pofReader.ReadRawInt128(0);

            Assert.AreEqual(input1.Value, result1.Value);
            Assert.AreEqual(input2.Value, result2.Value);
            Assert.AreEqual(input3.Value, result3.Value);
            Assert.AreEqual(input4.Value, result4.Value);
            Assert.AreEqual(input5.Value, result5.Value);
            Assert.AreEqual(input6.Value, result6.Value);
            Assert.AreEqual(input7.Value, result7.Value);
            Assert.AreEqual(input8.Value, result8.Value);
        }

        [Test]
        public void TestRawInt128Null()
        {
            initPOFWriter();
            RawInt128 input1 = new RawInt128((byte[])null);
            pofWriter.WriteRawInt128(0, input1);

            initPOFReader();
            sbyte[] result1 = pofReader.ReadRawInt128(0).Value;
            Assert.AreEqual(input1.Value, result1);
        }

        [Test]
        public void TestObject()
        {
            initPOFWriter();
            // 1
            pofWriter.WriteObject(0, null);
            pofWriter.WriteObject(0, false);
            pofWriter.WriteBoolean(0, true);
            pofWriter.WriteBoolean(0, false);
            pofWriter.WriteObject(0, 'a');
            pofWriter.WriteObject(0, Byte.MinValue);
            pofWriter.WriteObject(0, Int16.MaxValue);
            pofWriter.WriteObject(0, 0);
            pofWriter.WriteObject(0, -1);
            pofWriter.WriteObject(0, Int32.MaxValue);
            // 11
            pofWriter.WriteObject(0, (Int64)(-1));
            pofWriter.WriteObject(0, Int64.MinValue);
            pofWriter.WriteObject(0, (Single)0);
            pofWriter.WriteObject(0, Single.NaN);
            pofWriter.WriteObject(0, Single.NegativeInfinity);
            pofWriter.WriteObject(0, Double.NaN);
            pofWriter.WriteObject(0, "test ");
            pofWriter.WriteObject(0, String.Empty);
            pofWriter.WriteObject(0, new DateTime(1978, 4, 25, 7, 5, 10, 110));
            
            DateTime dtUtc = new DateTime(2006, 8, 18, 11, 28, 10, 100, DateTimeKind.Utc);
            RawTime rawTime1 = new RawTime(11, 30, 0, 99, true);
            RawTime rawTime2 = new RawTime(11, 30, 0, 99, 2, 30);

            pofWriter.WriteObject(0, dtUtc);
            // 21
            pofWriter.WriteObject(0, rawTime1);
            pofWriter.WriteObject(0, rawTime2);

            bool[] bArray = new bool[] { true, true };
            pofWriter.WriteObject(0, bArray);
            byte[] byteArray = new byte[] { 1, 2, 3 };
            pofWriter.WriteObject(0, byteArray);
            char[] charArray = new char[] { 'a', 'd', Char.MinValue };
            pofWriter.WriteObject(0, charArray);
            Int16[] i16Array = new Int16[] { 100, 200, Int16.MinValue };
            pofWriter.WriteObject(0, i16Array);
            Int32[] i32Array = new Int32[] { 100, 200, Int32.MinValue };
            pofWriter.WriteObject(0, i32Array);
            Int64[] i64Array = new Int64[] { 100, 200, Int64.MinValue };
            pofWriter.WriteObject(0, i64Array);

            ArrayList al = new ArrayList();
            al.Add(true);
            al.Add(5);
            pofWriter.WriteObject(0, al);

            Single[] fArray = new Single[] { Single.MinValue, Single.NegativeInfinity };
            pofWriter.WriteObject(0, fArray);
            Double[] dArray = new Double[] { Double.MinValue, Double.NegativeInfinity };
            pofWriter.WriteObject(0, dArray);
            object[] objArray = new object[] { true, Double.NaN };
            // 31
            pofWriter.WriteObject(0, objArray);
            RawYearMonthInterval ymi = new RawYearMonthInterval(10, 6);
            pofWriter.WriteObject(0, ymi);
            TimeSpan ts = new TimeSpan(1, 16, 30, 25, 400);
            pofWriter.WriteObject(0, ts);
            TimeSpan ts1 = new TimeSpan(5, 10, 30);
            pofWriter.WriteObject(0, ts1);

            initPOFReader();
            // 1
            Assert.AreEqual(null, pofReader.ReadObject(0));
            Assert.AreEqual(false, pofReader.ReadObject(0));
            Assert.AreEqual(true, pofReader.ReadObject(0));
            Assert.AreEqual(false, pofReader.ReadObject(0));
            Assert.AreEqual('a', pofReader.ReadObject(0));
            Assert.AreEqual(Byte.MinValue, pofReader.ReadObject(0));
            Assert.AreEqual(Int16.MaxValue, pofReader.ReadObject(0));
            Assert.AreEqual(0, pofReader.ReadObject(0));
            Assert.AreEqual(-1, pofReader.ReadObject(0));
            Assert.AreEqual(Int32.MaxValue, pofReader.ReadObject(0));
            // 11
            Assert.AreEqual(-1, pofReader.ReadObject(0));
            Assert.AreEqual(Int64.MinValue, pofReader.ReadObject(0));
            Assert.AreEqual(0, pofReader.ReadObject(0));
            Assert.AreEqual(Single.NaN, pofReader.ReadObject(0));
            Assert.AreEqual(Single.NegativeInfinity, pofReader.ReadObject(0));
            Assert.AreEqual(Double.NaN, pofReader.ReadObject(0));
            Assert.AreEqual("test ", pofReader.ReadObject(0));
            Assert.AreEqual(String.Empty, pofReader.ReadObject(0));
            Assert.AreEqual(new DateTime(1978, 4, 25, 7, 5, 10, 110), pofReader.ReadObject(0));
            Assert.AreEqual(dtUtc, pofReader.ReadObject(0));
            // 21
            Assert.AreEqual(rawTime1, pofReader.ReadObject(0));
            Assert.AreEqual(rawTime2, pofReader.ReadObject(0));
            Assert.AreEqual(bArray, pofReader.ReadObject(0));
            Assert.AreEqual(byteArray, pofReader.ReadObject(0));
            Assert.AreEqual(charArray, pofReader.ReadObject(0));
            Assert.AreEqual(i16Array, pofReader.ReadObject(0));
            Assert.AreEqual(i32Array, pofReader.ReadObject(0));
            Assert.AreEqual(i64Array, pofReader.ReadObject(0));

            Object result = pofReader.ReadObject(0);
            Assert.IsTrue(result is ICollection);
            ArrayList ares = new ArrayList((ICollection)result);
            Assert.AreEqual(ares[0], al[0]);
            Assert.AreEqual(ares[1], al[1]);

            Assert.AreEqual(fArray, pofReader.ReadObject(0));
            Assert.AreEqual(dArray, pofReader.ReadObject(0));
            // 31
            Assert.AreEqual(objArray, pofReader.ReadObject(0));
            Assert.AreEqual(ymi, pofReader.ReadObject(0));
            Assert.AreEqual(ts, pofReader.ReadObject(0));
            Assert.AreEqual(ts1, pofReader.ReadObject(0));
        }

        [Test]
        public void TestWriteObjectWithException()
        {
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteObject(0, null), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestReadObjectWithException()
        {
            initPOFWriter();
            writer.WritePackedInt32(-200);
            initPOFReader();
            Assert.That(() => pofReader.ReadObject(0), Throws.TypeOf<IOException>());
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

        private DataReader reader;
        private DataWriter writer;
        private PofStreamReader pofReader;
        private PofStreamWriter pofWriter;
        private MemoryStream stream;
    }
}
