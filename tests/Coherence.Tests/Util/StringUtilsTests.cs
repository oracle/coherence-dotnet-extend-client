/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;

using NUnit.Framework;

namespace Tangosol.Util
{
    [TestFixture]
    public class StringUtilsTests
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
        public void TestDupChar()
        {
            Assert.AreEqual("000000", StringUtils.Dup('0', 6));
            Assert.AreEqual("", StringUtils.Dup('0', 0));
        }

        [Test]
        public void TestDupString()
        {
            Assert.AreEqual("AleksAleksAleks", StringUtils.Dup("Aleks", 3));
            Assert.AreEqual("Aleks", StringUtils.Dup("Aleks", 1));
            Assert.AreEqual("", StringUtils.Dup("Aleks", 0));
        }

        [Test]
        public void IsNullOrEmptyTest()
        {
            Assert.IsTrue(StringUtils.IsNullOrEmpty(""));
            Assert.IsTrue(StringUtils.IsNullOrEmpty(null));
            Assert.IsFalse(StringUtils.IsNullOrEmpty(" "));
            Assert.IsFalse(StringUtils.IsNullOrEmpty("test"));
        }

        [Test]
        public void ToMemorySizeStringNegativeValueTest()
        {
            Assert.That(() => StringUtils.ToMemorySizeString(-300, false), Throws.ArgumentException);
        }

        [Test]
        public void ToMemorySizeStringTest()
        {
            long ms = 832;
            string result = StringUtils.ToMemorySizeString(ms, false);
            Assert.AreEqual(ms.ToString(), result);

            // exact values
            ms = 1024;
            result = StringUtils.ToMemorySizeString(ms, true);
            Assert.AreEqual(result, "1KB");

            ms = 1024 * ms;
            result = StringUtils.ToMemorySizeString(ms, true);
            Assert.AreEqual(result, "1MB");

            ms = 1024 * ms;
            result = StringUtils.ToMemorySizeString(ms, true);
            Assert.AreEqual(result, "1GB");

            ms = 1024 * ms;
            result = StringUtils.ToMemorySizeString(ms, true);
            Assert.AreEqual(result, "1TB");

            // not exact values
            ms = 1028;
            result = StringUtils.ToMemorySizeString(ms, false);
            Assert.AreEqual(result, "1.00KB");
        }

        [Test]
        public void ToBandwidthStringTest()
        {
            long ms = 1024;
            string result = StringUtils.ToBandwidthString(ms, true);
            Assert.AreEqual("8Kbps", result);

            ms = 430;
            result = StringUtils.ToBandwidthString(ms, true);
            Assert.AreEqual(3440 + "bps", result);
            result = StringUtils.ToBandwidthString(ms, false);
            Assert.AreEqual("3.35Kbps", result);
        }

        [Test]
        public void ToDecStringTest()
        {
            const int n = 123;
            int digits = 3;

            Assert.AreEqual(n.ToString(), StringUtils.ToDecString(n, digits));

            digits = 2;
            Assert.AreEqual("23", StringUtils.ToDecString(n, digits));

            digits = 0;
            Assert.AreEqual("", StringUtils.ToDecString(n, digits));

            digits = 4;
            Assert.AreEqual("0123", StringUtils.ToDecString(n, digits));
        }

        [Test]
        public void EscapeTest()
        {
            var chars = new char[6];
            
            int len = StringUtils.Escape('a', chars, 0);
            Assert.AreEqual(len, 1);
            Assert.AreEqual(chars[0], 'a');

            len = StringUtils.Escape('\b', chars, 0);
            Assert.AreEqual(len, 2);
            Assert.AreEqual(chars[0], '\\');
            Assert.AreEqual(chars[1], 'b');

            len = StringUtils.Escape('\t', chars, 0);
            Assert.AreEqual(len, 2);
            Assert.AreEqual(chars[0], '\\');
            Assert.AreEqual(chars[1], 't');

            len = StringUtils.Escape('\n', chars, 0);
            Assert.AreEqual(len, 2);
            Assert.AreEqual(chars[0], '\\');
            Assert.AreEqual(chars[1], 'n');

            len = StringUtils.Escape('\f', chars, 0);
            Assert.AreEqual(len, 2);
            Assert.AreEqual(chars[0], '\\');
            Assert.AreEqual(chars[1], 'f');

            len = StringUtils.Escape('\r', chars, 0);
            Assert.AreEqual(len, 2);
            Assert.AreEqual(chars[0], '\\');
            Assert.AreEqual(chars[1], 'r');

            len = StringUtils.Escape('\"', chars, 0);
            Assert.AreEqual(len, 2);
            Assert.AreEqual(chars[0], '\\');
            Assert.AreEqual(chars[1], '"');

            len = StringUtils.Escape('\'', chars, 0);
            Assert.AreEqual(len, 2);
            Assert.AreEqual(chars[0], '\\');
            Assert.AreEqual(chars[1], '\'');

            len = StringUtils.Escape('\\', chars, 0);
            Assert.AreEqual(len, 2);
            Assert.AreEqual(chars[0], '\\');
            Assert.AreEqual(chars[1], '\\');

            len = StringUtils.Escape((char) 26, chars, 0);
            Assert.AreEqual(len, 4);
            Assert.AreEqual(chars[0], '\\');
            Assert.AreEqual(chars[1], '0');
            Assert.AreEqual(chars[2], '3');
            Assert.AreEqual(chars[3], '2');

            len = StringUtils.Escape((char) 0xF8FF, chars, 0);
            Assert.AreEqual(len, 6);
            Assert.AreEqual(chars[0], '\\');
            Assert.AreEqual(chars[1], 'u');
            Assert.AreEqual(chars[2], 'F');
            Assert.AreEqual(chars[3], '8');
            Assert.AreEqual(chars[4], 'F');
            Assert.AreEqual(chars[5], 'F');
        }

        [Test]
        public void ToCharEscapeTest()
        {
            Assert.AreEqual(StringUtils.ToCharEscape('a'), "a");
            Assert.AreEqual(StringUtils.ToCharEscape('\"'), "\\\"");
            Assert.AreEqual(StringUtils.ToCharEscape((char) 26), "\\032");
            Assert.AreEqual(StringUtils.ToCharEscape((char) 0xF8FF), "\\uF8FF");
        }

        [Test]
        public void BreakLinesTest()
        {
            string text = "looong text";
            string result = StringUtils.BreakLines(text, 5, null);
            Assert.AreEqual(result, "looon\ng\ntext");

            text = "looong text";
            const string indent = "\t";
            result = StringUtils.BreakLines(text, 5, indent, true);
            Assert.AreEqual(result, "\tlooo\n\tng\n\ttext");
            result = StringUtils.BreakLines(text, 5, indent, false);
            Assert.AreEqual(result, "looo\n\tng\n\ttext");

            text = "looong\ntext";
            result = StringUtils.BreakLines(text, 5, indent, false);
            Assert.AreEqual(result, "looo\n\tng\n\ttext");

            Exception e = null;
            try
            {
                StringUtils.BreakLines(text, 5, "longer");
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
        }

        [Test]
        public void ByteArrayToHexStringTest()
        {
            var array  = new byte[] { 0x1b, 0x0e, 0x92, 0xe8, 0x07, 0x1f, 0x6b, 0x91 };
            var result = StringUtils.ByteArrayToHexString(array);
            Assert.AreEqual(result, "1B0E92E8071F6B91");
        }

        [Test]
        public void HexStringToByteArrayTest()
        {
            var text   = "1b0E92e8071f6B91";
            var result = StringUtils.HexStringToByteArray(text);
            Assert.AreEqual(result,
                    new byte[] { 0x1b, 0x0e, 0x92, 0xe8, 0x07, 0x1f, 0x6b, 0x91 });
        }
    }
}