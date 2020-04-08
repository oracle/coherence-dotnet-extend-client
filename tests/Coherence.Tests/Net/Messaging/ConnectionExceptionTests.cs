/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace Tangosol.Net.Messaging
{
    [TestFixture]
    public class ConnectionExceptionTests
    {
        [Test]
        [ExpectedException(typeof(ConnectionException))]
        public void ConnectionExceptionTest()
        {
            ConnectionException e = new ConnectionException();
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(Exception), e);
            throw e;
        }

        [Test]
        [ExpectedException(typeof(ConnectionException))]
        public void ConnectionExceptionStringTest()
        {
            ConnectionException e = new ConnectionException("test");
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(Exception), e);
            Assert.IsTrue(e.Message.IndexOf("test") >= 0);
            throw e;
        }

        [Test]
        [ExpectedException(typeof(ConnectionException))]
        public void ConnectionExceptionInnerExceptionTest()
        {
            FormatException innerException = new FormatException("inner error");
            ConnectionException e = new ConnectionException(innerException);
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(Exception), e);
            Assert.IsInstanceOf(typeof(FormatException), e.InnerException);
            throw e;
        }

        [Test]
        [ExpectedException(typeof(ConnectionException))]
        public void ConnectionExceptionStringInnerExceptionTest()
        {
            FormatException innerException = new FormatException("inner error");
            ConnectionException e = new ConnectionException("test", innerException);
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(Exception), e);
            Assert.IsInstanceOf(typeof(FormatException), e.InnerException);
            Assert.IsTrue(e.Message.IndexOf("test") >= 0);
            throw e;
        }

        [Test]
        public void ConnectionExceptionSerializationTest()
        {
            ConnectionException ce =
                new ConnectionException("connection exception test",
                                        new ArgumentException("inner_exception", "dummy_param"));
            Assert.IsNotNull(ce);

            IFormatter formatter = new BinaryFormatter();
            byte[] buffer = new byte[1024*16];

            Stream stream = new MemoryStream(buffer);
            formatter.Serialize(stream, ce);
            stream.Close();

            stream = new MemoryStream(buffer);
            ConnectionException deserCE = (ConnectionException) formatter.Deserialize(stream);
            stream.Close();

            Assert.IsNotNull(deserCE);
            Assert.AreEqual(ce.Message, deserCE.Message);
            Assert.AreEqual(ce.StackTrace, deserCE.StackTrace);
            Assert.IsNotNull(deserCE.InnerException);
            Assert.AreEqual(deserCE.InnerException.GetType(), ce.InnerException.GetType());
            Assert.AreEqual(((ArgumentException) deserCE.InnerException).ParamName,
                            ((ArgumentException) ce.InnerException).ParamName);
        }
    }
}
