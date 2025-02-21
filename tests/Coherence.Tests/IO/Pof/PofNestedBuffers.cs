/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class PofNestedBuffers
    {
        private const string complexValue = "901F000041A40101901F0000901F00004E0B48656C6C6F20576F726C64025844043F8000004000000040533333408CCCCD03901F0000584E03036F6E650374776F05746872656540400155054E04666F75724E04666976654E037369784E05736576656E4E056569676874026B036E04564E0504666F757204666976650373697805736576656E0565696768740A454001C6A7EF9DB22D4002454011C6A7EF9DB22D037840";
        private const string simpleValue  = "911F00004100016A026B03911F00004100016A026B40046D056E40";

        ConfigurablePofContext cpc = new ConfigurablePofContext("Config/include-pof-config.xml");

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
        public void testNestedWriter()
        {
            MemoryStream buffer = new MemoryStream(1000);
            DataWriter writer   = new DataWriter(buffer);
            DataReader reader   = new DataReader(buffer);
            NestedType nt       = new NestedType();

            cpc.Serialize(writer, nt);
            Assert.AreEqual(complexValue, ByteArrayToString(buffer.ToArray()));

            buffer.Position   = 0;
            cpc.Deserialize(reader);
        }

        [Test]
        public void testNestedSimple()
        {
            MemoryStream buffer = new MemoryStream(1000);
            DataWriter writer   = new DataWriter(buffer);
            DataReader reader   = new DataReader(buffer);
            SimpleType nt       = new SimpleType();

            cpc.Serialize(writer, nt);
            Assert.AreEqual(simpleValue, ByteArrayToString(buffer.ToArray()));

            buffer.Position = 0;
            cpc.Deserialize(reader);
        }

        public static string ByteArrayToString(byte[] array)
        {
            StringBuilder builder = new StringBuilder();

            foreach (byte b in array)
            {
                if (b < 0x10)
                {
                    builder.AppendFormat("0{0:X}", b);
                }
                else
                {
                    builder.AppendFormat("{0:X}", b);
                }
            }
            return builder.ToString();
        }
    }
}