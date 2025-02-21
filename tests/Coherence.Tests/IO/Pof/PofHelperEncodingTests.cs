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
    public class PofHelperEncodingTests
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
        public void TestEncodeTinyInt()
        {   //PASSED
            Assert.AreEqual(PofConstants.V_INT_1, PofHelper.EncodeTinyInt(1));
            Assert.AreEqual(PofConstants.V_INT_NEG_1, PofHelper.EncodeTinyInt(-1));
            Assert.AreEqual(PofConstants.V_INT_13, PofHelper.EncodeTinyInt(13));
        }

        [Test]
        public void TestWriteDate()
        {   //PASSED
            initPOFWriter();
            PofHelper.WriteDate(writer, 2006, 8, 10);
            PofHelper.WriteDate(writer, 2006, 8, 11);

            initPOFReader();
            Assert.AreEqual(new DateTime(2006, 8, 10), PofHelper.ReadDate(reader));
            Assert.AreEqual(new DateTime(2006, 8, 11), PofHelper.ReadDate(reader));
        }

        [Test]
        public void TestWriteTime()
        {   //PASSED
            initPOFWriter();
            PofHelper.WriteTime(writer, 12, 59, 58, 0, 1, TimeSpan.Zero);
            PofHelper.WriteTime(writer, 12, 59, 59, 0, 0, TimeSpan.Zero);
            PofHelper.WriteTime(writer, 12, 59, 59, 0, 2, new TimeSpan(1, 0, 0));

            initPOFReader();
            Assert.AreEqual(new RawTime(12,59,58,0, true), PofHelper.ReadRawTime(reader));
            Assert.AreEqual(new RawTime(12,59,59,0, false), PofHelper.ReadRawTime(reader));
            Assert.AreEqual(new RawTime(12,59,59,0,1,0), PofHelper.ReadRawTime(reader));
        }
    }
}