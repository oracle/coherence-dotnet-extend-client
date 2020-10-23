/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using NUnit.Framework;

namespace Tangosol.Util
{
    [TestFixture]
    public class BinaryMemoryStreamTests
    {
        [Test]
        public void TestDefaultConstructor()
        {
            BinaryMemoryStream stream = new BinaryMemoryStream();
            
            Assert.IsNotNull(stream);
            Assert.AreEqual(stream.Length, 0);
            Assert.IsTrue(stream.CanWrite);
            stream.WriteByte(1);
            Assert.AreEqual(stream.Length, 1);
            stream.ToBinary(); //this invokes streamGetInternalByteArray and makes stream immutable
            //byte[] arr = stream.GetInternalByteArray();
            //Assert.IsNotNull(arr);
            //Assert.AreEqual(arr.Length, 1);
            Assert.IsFalse(stream.CanWrite);
            Exception ex = TryWrite(stream);
            Assert.IsNotNull(ex);
            Assert.IsInstanceOf(typeof (NotSupportedException), ex);
        }

        [Test]
        public void TestConstructorWithCapacity()
        {
            BinaryMemoryStream stream = new BinaryMemoryStream(4);

            Assert.IsNotNull(stream);
            Assert.AreEqual(stream.Length, 0);
            Assert.AreEqual(stream.Capacity, 4);
            Assert.IsTrue(stream.CanWrite);
            Exception ex = TryWrite(stream);
            Assert.IsNull(ex);
            Assert.AreEqual(stream.Length, 1);
            stream.ToBinary();
            //byte[] arr = stream.GetInternalByteArray();
            //Assert.IsNotNull(arr);
            //Assert.AreEqual(arr.Length, 1);
            Assert.IsFalse(stream.CanWrite);
            ex = TryWrite(stream);
            Assert.IsNotNull(ex);
            Assert.IsInstanceOf(typeof(NotSupportedException), ex);
        }

        [Test]
        public void TestGetBuffer1()
        {
            BinaryMemoryStream stream = new BinaryMemoryStream();
            Assert.IsNotNull(stream);
            Assert.AreEqual(stream.Length, 0);
            Assert.That(() => stream.GetBuffer(), Throws.TypeOf<UnauthorizedAccessException>());
        }

        [Test]
        public void TestGetBuffer2()
        {
            BinaryMemoryStream stream = new BinaryMemoryStream(4);
            Assert.IsNotNull(stream);
            Assert.AreEqual(stream.Length, 0);
            Assert.AreEqual(stream.Capacity, 4);
            Assert.That(() => stream.GetBuffer(), Throws.TypeOf<UnauthorizedAccessException>());
        }

        private Exception TryWrite(MemoryStream stream)
        {
            try
            {
                stream.WriteByte(10);
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }
    }
}
