/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Globalization;
using System.IO;
using System.Text;

using NUnit.Framework;
using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class PofStreamReaderAndWriterTests
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
        public void TestReaderConstructor()
        {
            PofStreamReader psr = new PofStreamReader(new DataReader(new MemoryStream()), new SimplePofContext());
            psr.PofContext = new SimplePofContext();
            Console.WriteLine(psr.UserTypeId);
        }

        [Test]
        public void TestReaderConstructorWithException()
        {
            PofStreamReader psr = new PofStreamReader(new DataReader(new MemoryStream()), new SimplePofContext());
            Assert.That(() => psr.PofContext = null, Throws.ArgumentNullException);
        }

        [Test]
        public void TestReaderReadReminder()
        {
            PofStreamReader psr = new PofStreamReader(new DataReader(new MemoryStream()), new SimplePofContext());
            Assert.That(() => psr.ReadRemainder(), Throws.InvalidOperationException);
        }

        [Test]
        public void TestReaderVersionIdWithException()
        {
            PofStreamReader psr = new PofStreamReader(new DataReader(new MemoryStream()), new SimplePofContext());
            Assert.That(() => Console.WriteLine(psr.VersionId), Throws.InvalidOperationException);
        }

        [Test]
        public void TestWriterProperties()
        {
            PofStreamWriter psw = new PofStreamWriter(new DataWriter(new MemoryStream()), new SimplePofContext());
            psw.PofContext = new SimplePofContext();
            Console.WriteLine(psw.UserTypeId);
            Assert.That(() => psw.PofContext = null, Throws.ArgumentException);
        }

        [Test]
        public void TestWriterVersionIdWithException1()
        {
            PofStreamWriter psw = new PofStreamWriter(new DataWriter(new MemoryStream()), new SimplePofContext());
            Assert.That(() => Console.WriteLine(psw.VersionId), Throws.InvalidOperationException);
        }

        [Test]
        public void TestWriterVersionIdWithException2()
        {
            PofStreamWriter psw = new PofStreamWriter(new DataWriter(new MemoryStream()), new SimplePofContext());
            Assert.That(() => psw.VersionId = 3, Throws.InvalidOperationException);
        }

        [Test]
        public void TestGenericCollection()
        {
            MemoryStream           buffer = new MemoryStream(1000);
            DataWriter             writer = new DataWriter(buffer);
            DataReader             reader = new DataReader(buffer);
            ConfigurablePofContext cpc    = new ConfigurablePofContext("Config/include-pof-config.xml");
            GenericCollectionsType gct    = new GenericCollectionsType();

            cpc.Serialize(writer, gct);
            buffer.Position = 0;
            cpc.Deserialize(reader);
        }

        [Test]
        public void TestUTF8Serialization()
        {
            // Create a string with multi-bytes character.
            String surrogate = "abc" + Char.ConvertFromUtf32(Int32.Parse("2A601", NumberStyles.HexNumber)) + "def";

            // Safe UTF-8 encoding & decoding of string.
            byte[] bytes = Encoding.UTF8.GetBytes(surrogate);

            Console.WriteLine("Safe UTF-8-encoded code units:");
            foreach (var utf8Byte in bytes)
                Console.Write("{0:X2} ", utf8Byte);
            Console.WriteLine();
            string s = SerializationHelper.ConvertUTF(bytes, 0, bytes.Length);
            Assert.AreEqual(s, surrogate);
        }

        /// <summary>
        /// Additional UTF-8 conversion tests.
        /// </summary>
        /// <since>Coherence 14.1.1.15</since>
        [Test]
        public void TestUtfConversion()
        {
            AssertUtfConversion("Aleksandar");
            AssertUtfConversion("Александар");
            AssertUtfConversion("ⅯⅭⅯⅬⅩⅩⅠⅤ");

            uint[] aInt  = new uint[] { 0xf0938080, 0xf09f8ebf, 0xf09f8f80, 0xf09f8e89, 0xf09f9294 };
            byte[] aByte = ToBytes(aInt);
            AssertUtfConversion(aByte);

            // make sure we can still handle our proprietary (broken) encoding
            String      sUtf       = Encoding.UTF8.GetString(aByte);
            ISerializer serializer = new SimplePofContext();
            Binary      bin        = SerializationHelper.ToBinary(sUtf, serializer);

            Assert.AreEqual(23, bin.Length);
            Assert.AreEqual(sUtf, SerializationHelper.FromBinary(bin, serializer));
        }

        #region helper methods

        private void AssertUtfConversion(String s)
        {
            AssertUtfConversion(Encoding.UTF8.GetBytes(s));
        }

        private void AssertUtfConversion(byte[] abUtf8)
        {
            String sExpected = Encoding.UTF8.GetString(abUtf8);
            String sActual   = SerializationHelper.ConvertUTF(abUtf8, 0, abUtf8.Length);
            Console.Write("\n%12s = %-12s : utf8 bytes = %d; string length = %d", sExpected, sActual, abUtf8.Length, sActual.Length);
            Assert.AreEqual(sExpected, sActual);
        }

        private static byte[] ToBytes(uint[] ai)
        {
            byte[] abResult = new byte[ai.Length * 4];
            int    i        = 0;

            foreach (uint n in ai)
            {
                MemoryStream buf    = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(buf);

                writer.Write(n);            // this writes in little endian (.NET default), expects big endian (Java default)
                byte[] ab = buf.ToArray();
                abResult[i + 3] = ab[0];
                abResult[i + 2] = ab[1];
                abResult[i + 1] = ab[2];
                abResult[i]     = ab[3];
                i += 4;
            }

            return abResult;
        }
        #endregion

    }
}
