/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Text;

using NUnit.Framework;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Util;

namespace Tangosol.Run.Xml
{
    [TestFixture]
    public class SimpleValueTests
    {
        [Test]
        public void TestConstructors()
        {
            SimpleValue v1 = new SimpleValue();
            Assert.IsNull(v1.Value);
            Assert.IsFalse(v1.IsAttribute);
            Assert.IsTrue(v1.IsMutable);

            SimpleValue v2 = new SimpleValue("test");
            Assert.IsNotNull(v2.Value);
            Assert.IsFalse(v2.IsAttribute);
            Assert.IsTrue(v2.IsMutable);

            SimpleValue v3 = new SimpleValue("test", true);
            Assert.IsNotNull(v3.Value);
            Assert.IsTrue(v3.IsAttribute);
            Assert.IsTrue(v3.IsMutable);

            SimpleValue v4 = new SimpleValue("test", true, true);
            Assert.IsNotNull(v4.Value);
            Assert.IsTrue(v4.IsAttribute);
            Assert.IsFalse(v4.IsMutable);
        }

        [Test]
        public void TestConstructorWithException()
        {
            Assert.That(() => new SimpleValue(new StringBuilder()), Throws.ArgumentException);
        }

        [Test]
        public void TestProperties()
        {
            SimpleValue sv = new SimpleValue("value", false, false);
            Assert.IsFalse(sv.IsAttribute);
            Assert.IsTrue(sv.IsContent);
            Assert.IsFalse(sv.IsEmpty);
            Assert.IsNull(sv.Parent);

            sv.SetString(null);
            Assert.IsTrue(sv.IsEmpty);
            sv.SetString("");
            Assert.IsTrue(sv.IsEmpty);
            sv.SetBinary(Binary.NO_BINARY);
            Assert.IsTrue(sv.IsEmpty);

            SimpleElement element = new SimpleElement();
            sv.Parent = element;
            Assert.IsNotNull(sv.Parent);

            Exception e = null;
            try
            {
                sv.Parent = new SimpleElement(); //already set
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(InvalidOperationException), e);
            e = null;
            try
            {
                sv.Parent = null; //cannot be null
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
            e = null;
            IXmlValue immutable = element.GetSafeAttribute("attr");
            Assert.IsNotNull(immutable);
            Assert.IsFalse(immutable.IsMutable);
            try
            {
                immutable.Parent = element;
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(InvalidOperationException), e);

            sv = new SimpleValue("value", false, false);
            Assert.IsTrue(sv.IsMutable);
            sv.Parent = element.GetSafeElement("test");
            Assert.IsFalse(sv.IsMutable);
        }

        [Test]
        public void TestDefaultValueAccessors()
        {
            SimpleValue sv = new SimpleValue();
            Assert.IsNull(sv.Value);
            Assert.AreEqual(true, sv.GetBoolean(true));
            Assert.AreEqual(5, sv.GetInt(5));
            Assert.AreEqual(long.MaxValue, sv.GetLong(long.MaxValue));
            Assert.AreEqual("test", sv.GetString("test"));
            Assert.AreEqual(double.MaxValue, sv.GetDouble(double.MaxValue));
            Assert.AreEqual(decimal.MinValue, sv.GetDecimal(decimal.MinValue));
            Assert.AreEqual(Binary.NO_BINARY, sv.GetBinary(Binary.NO_BINARY));
            DateTime dt = DateTime.Now;
            Assert.AreEqual(dt, sv.GetDateTime(dt));

            sv.SetBoolean(false);
            Assert.AreNotEqual(true, sv.GetBoolean(true));
            sv.SetInt(100);
            Assert.AreNotEqual(5, sv.GetInt(5));
            sv.SetLong(100L);
            Assert.AreNotEqual((object) long.MaxValue, (object) sv.GetLong(long.MaxValue));
            sv.SetString("testing");
            Assert.AreNotEqual("test", sv.GetString("test"));
            sv.SetDouble(100D);
            Assert.AreNotEqual(double.MaxValue, sv.GetDouble(double.MaxValue));
            sv.SetDecimal(decimal.MinusOne);
            Assert.AreNotEqual(decimal.MinValue, sv.GetDecimal(decimal.MinValue));
            sv.SetBinary(new Binary(new byte[] {3, 4, 5}));
            Assert.AreNotEqual(Binary.NO_BINARY, sv.GetBinary(Binary.NO_BINARY));
            sv.SetDateTime(DateTime.Today);
            Assert.AreNotEqual(dt, sv.GetDateTime(dt));
        }

        [Test]
        public void TestValue()
        {
            SimpleValue sv = new SimpleValue("true");
            Assert.AreEqual(true, sv.GetBoolean());
            Assert.AreEqual(true.ToString(), sv.GetString());
            Assert.AreEqual(0, sv.GetInt()); //after this, value is null
            Assert.IsNull(sv.Value);

            sv.SetInt(5);
            Assert.AreEqual(5, sv.GetInt());
            Assert.AreEqual(5L, sv.GetLong());
            Assert.AreEqual(5.0, sv.GetDouble());
            Assert.AreEqual(new decimal(5), sv.GetDecimal());
            Assert.AreEqual(DateTime.MinValue, sv.GetDateTime());
            Assert.IsNull(sv.Value);

            Assert.AreEqual(Binary.NO_BINARY, sv.GetBinary());
        }

        [Test]
        public void TestValueWithException1()
        {
            SimpleValue sv = new SimpleValue(null, false, true);
            Assert.That(() => sv.SetBoolean(true), Throws.InvalidOperationException);
        }

        [Test]
        public void TestValueWithException2()
        {
            SimpleValue sv = new SimpleValue("value", true, false);
            Assert.That(() => sv.SetString(null), Throws.ArgumentNullException);
        }

        [Test]
        public void TestSerialization()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.IsNotNull(ctx);

            bool v1 = true;
            SimpleValue sv1 = new SimpleValue(v1);
            int v2 = 5;
            SimpleValue sv2 = new SimpleValue(v2);
            long v3 = 999L;
            SimpleValue sv3 = new SimpleValue(v3);
            double v4 = 5.257;
            SimpleValue sv4 = new SimpleValue(v4);
            decimal v5 = decimal.MinusOne;
            SimpleValue sv5 = new SimpleValue(v5);
            string v6 = "test";
            SimpleValue sv6 = new SimpleValue(v6);
            Binary v7 = new Binary(new byte[] {99, 50, 27});
            SimpleValue sv7 = new SimpleValue(v7);
            DateTime v8 = new DateTime(1976, 4, 19);
            SimpleValue sv8 = new SimpleValue(v8);

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), sv1);
            ctx.Serialize(new DataWriter(stream), sv2);
            ctx.Serialize(new DataWriter(stream), sv3);
            ctx.Serialize(new DataWriter(stream), sv4);
            ctx.Serialize(new DataWriter(stream), sv5);
            ctx.Serialize(new DataWriter(stream), sv6);
            ctx.Serialize(new DataWriter(stream), sv7);
            ctx.Serialize(new DataWriter(stream), sv8);

            stream.Position = 0;
            SimpleValue sv1d = (SimpleValue) ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(sv1, sv1d);

            SimpleValue sv2d = (SimpleValue) ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(sv2, sv2d);

            SimpleValue sv3d = (SimpleValue) ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(sv3, sv3d);

            SimpleValue sv4d = (SimpleValue) ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(sv4, sv4d);

            SimpleValue sv5d = (SimpleValue) ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(sv5, sv5d);

            SimpleValue sv6d = (SimpleValue) ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(sv6, sv6d);

            SimpleValue sv7d = (SimpleValue) ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(sv7, sv7d);

            SimpleValue sv8d = (SimpleValue) ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(sv8, sv8d);
        }

        [Test]
        public void TestObjectMethods()
        {
            bool v1 = true;
            SimpleValue sv1 = new SimpleValue(v1);
            int v2 = 5;
            SimpleValue sv2 = new SimpleValue(v2);
            long v3 = 999L;
            SimpleValue sv3 = new SimpleValue(v3);
            double v4 = 5.257;
            SimpleValue sv4 = new SimpleValue(v4);
            decimal v5 = decimal.MinusOne;
            SimpleValue sv5 = new SimpleValue(v5);
            string v6 = "test";
            SimpleValue sv6 = new SimpleValue(v6);
            Binary v7 = new Binary(new byte[] { 99, 50, 27 });
            SimpleValue sv7 = new SimpleValue(v7);
            DateTime v8 = new DateTime(1976, 4, 19);
            SimpleValue sv8 = new SimpleValue(v8);

            //ToString
            Assert.AreEqual(v1.ToString(), sv1.ToString());
            Assert.AreEqual(v2.ToString(), sv2.ToString());
            Assert.AreEqual(v3.ToString(), sv3.ToString());
            Assert.AreEqual(v4.ToString(), sv4.ToString());
            Assert.AreEqual(v5.ToString(), sv5.ToString());
            Assert.AreEqual(v6, sv6.ToString());
            Assert.AreEqual(Convert.ToBase64String(v7.ToByteArray()), sv7.ToString());
            Assert.AreEqual(v8.ToString(), sv8.ToString());

            //Equals
            sv1.SetBoolean(false);
            sv6.SetString("False");
            Assert.IsTrue(sv1.Equals(sv6));
            Assert.IsFalse(sv1.Equals(sv7));
            Assert.IsFalse(sv1.Equals("test"));

            //GetHashCode
            Assert.AreEqual(XmlHelper.HashValue(sv6), sv6.GetHashCode());

            //Clone
            SimpleValue sv1c = (SimpleValue) sv1.Clone();
            SimpleValue sv2c = (SimpleValue) sv2.Clone();
            SimpleValue sv3c = (SimpleValue) sv3.Clone();
            SimpleValue sv4c = (SimpleValue) sv4.Clone();
            SimpleValue sv5c = (SimpleValue) sv5.Clone();
            SimpleValue sv6c = (SimpleValue) sv6.Clone();
            SimpleValue sv7c = (SimpleValue) sv7.Clone();
            SimpleValue sv8c = (SimpleValue) sv8.Clone();

            Assert.AreEqual(sv1c, sv1);
            Assert.AreNotSame(sv1c, sv1);
            Assert.AreEqual(sv2c, sv2);
            Assert.AreNotSame(sv2c, sv2);
            Assert.AreEqual(sv3c, sv3);
            Assert.AreNotSame(sv3c, sv3);
            Assert.AreEqual(sv4c, sv4);
            Assert.AreNotSame(sv4c, sv4);
            Assert.AreEqual(sv5c, sv5);
            Assert.AreNotSame(sv5c, sv5);
            Assert.AreEqual(sv6c, sv6);
            Assert.AreNotSame(sv6c, sv6);
            Assert.AreEqual(sv7c, sv7);
            Assert.AreNotSame(sv7c, sv7);
            Assert.AreEqual(sv8c, sv8);
            Assert.AreNotSame(sv8c, sv8);
            Assert.IsTrue(sv1c.IsMutable);
            Assert.IsNull(sv1c.Parent);

            sv6.SetString("newtest");
            Assert.AreNotEqual(sv6c, sv6);
        }

        [Test]
        public void TestWriteValue()
        {
            TextWriter writer = new StringWriter();
            SimpleValue attr = new SimpleValue("test", true);
            attr.WriteValue(writer, true);
            Assert.AreEqual("'test'", writer.ToString());

            Binary bin = new Binary(new byte[] { 0x1, 0xF });
            SimpleValue attrBin = new SimpleValue(bin, true);
            writer = new StringWriter();
            attrBin.WriteValue(writer, false);
            Assert.AreEqual("AQ8=", writer.ToString());

            SimpleValue content = new SimpleValue("some content with & and\n");
            writer = new StringWriter();
            content.WriteValue(writer, false);
            Assert.AreEqual("<![CDATA[" + content.GetString() + "]]>", writer.ToString());

            content = new SimpleValue(0xF);
            writer = new StringWriter();
            content.WriteValue(writer, false);
            Assert.AreEqual(0xF.ToString(), writer.ToString());

            byte[] bytes = new byte[100];
            for (byte i = 0; i < 100; i++)
            {
                bytes[i] = i;
            }
            bin = new Binary(bytes);
            content = new SimpleValue(bin);
            writer = new StringWriter();
            content.WriteValue(writer, true);
            Assert.IsTrue(writer.ToString().IndexOf("\n") >= 0);
        }
    }
}
