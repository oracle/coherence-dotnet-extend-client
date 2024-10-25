/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Tangosol.IO;
using Tangosol.IO.Pof;

namespace Tangosol.Net.Messaging
{
    [TestFixture]
    public class ConnectionExceptionTests
    {
        [Test]
        public void ConnectionExceptionTest()
        {
            ConnectionException e = new ConnectionException();
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(Exception), e);
        }

        [Test]
        public void ConnectionExceptionStringTest()
        {
            ConnectionException e = new ConnectionException("test");
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(Exception), e);
            Assert.IsTrue(e.Message.IndexOf("test") >= 0);
        }

        [Test]
        public void ConnectionExceptionInnerExceptionTest()
        {
            FormatException innerException = new FormatException("inner error");
            ConnectionException e = new ConnectionException(innerException);
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(Exception), e);
            Assert.IsInstanceOf(typeof(FormatException), e.InnerException);
        }

        [Test]
        public void ConnectionExceptionStringInnerExceptionTest()
        {
            FormatException innerException = new FormatException("inner error");
            ConnectionException e = new ConnectionException("test", innerException);
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(Exception), e);
            Assert.IsInstanceOf(typeof(FormatException), e.InnerException);
            Assert.IsTrue(e.Message.IndexOf("test") >= 0);
        }

        [Test]
        public void ConnectionExceptionSerializationTest()
        {
            ConnectionException ce =
                new ConnectionException("connection exception test",
                                        new ArgumentException("inner_exception", "dummy_param"));
            Assert.IsNotNull(ce);

            ConfigurablePofContext ctx = new ConfigurablePofContext(
                "assembly://Coherence/Tangosol.Config/coherence-pof-config.xml");
            byte[] buffer = new byte[1024*16];

            Stream stream = new MemoryStream(buffer);
            DataWriter writer = new DataWriter(stream);
            ctx.Serialize(writer, ce);
            stream.Close();

            stream = new MemoryStream(buffer);
            DataReader reader = new DataReader(stream);
            PortableException deserCE = (PortableException)ctx.Deserialize(reader);
            stream.Close();

            Assert.IsNotNull(deserCE);
            Assert.AreEqual(ce.Message, deserCE.Message);
            Assert.AreEqual("\tat <process boundary>\n"
                            + Regex.Replace(ce.StackTrace, "System.ArgumentException", "Portable($&)"), deserCE.StackTrace);
            Assert.IsNotNull(deserCE.InnerException);
            Assert.AreEqual(typeof(PortableException), deserCE.InnerException.GetType());
            Assert.IsTrue(deserCE.InnerException.Message.Contains(((ArgumentException)ce.InnerException).ParamName));
        }
    }
}
