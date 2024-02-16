/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Tangosol.Util
{
    /// <summary>
    /// Test for Tangosol.Util.HashHelper
    /// </summary>
    [TestFixture]
    public class HashHelperTests
    {
        [Test]
        public void TestBoolean()
        {
            Assert.AreEqual(0x04DF, HashHelper.Hash(true, 1));
            Assert.AreEqual(0x0525, HashHelper.Hash(false, 31));

            // array
            bool[] afGeorge = { true };
            Assert.AreEqual(0x000005CF, HashHelper.Hash(afGeorge, 1));
        }

        [Test]
        public void TestByte()
        {
            Assert.AreEqual(0x11, HashHelper.Hash((byte) 0x1, 1));
            Assert.AreEqual(0x01D0, HashHelper.Hash((byte) 0x20, 31));

            // array
            byte[] abOctet = { 0x1 };
            Assert.AreEqual(0x00000101, HashHelper.Hash(abOctet, 1));
        }

        [Test]
        public void TestChar()
        {
            Assert.AreEqual(0x01D0, HashHelper.Hash(' ', 31));

            // array
            char[] achChar = { ' ' };
            Assert.AreEqual(0x00000120, HashHelper.Hash(achChar, 1));
        }

        [Test]
        public void TestDouble()
        {
            Assert.AreEqual(0x3FF00010, HashHelper.Hash(1.0d, 1));
            Assert.AreEqual(-0x73733E0F, HashHelper.Hash(32.1d, 31));

            // array
            double[] adflDouble = { 1.0d };
            Assert.AreEqual(0x3FF00100, HashHelper.Hash(adflDouble, 1));
        }

        [Test]
        public void TestFloat()
        {
            Assert.AreEqual(0x3F800010, HashHelper.Hash(1.0f, 1));
            // float value produces a different hash than the double
            // equivalent due the xor of the long's 2 chunked 4 bytes.
            Assert.AreEqual(0x42006796, HashHelper.Hash(32.1f, 31));

            // array
            float[] aflFloat = { 1.0f };
            Assert.AreEqual(0x3F800100, HashHelper.Hash(aflFloat, 1));
        }

        [Test]
        public void TestInt()
        {
            Assert.AreEqual(0x0011, HashHelper.Hash(1, 1));
            Assert.AreEqual(0x01D0, HashHelper.Hash(32, 31));

            // array
            int[] anInt = { 1 };
            Assert.AreEqual(0x00000101, HashHelper.Hash(anInt, 1));
        }

        [Test]
        public void TestLong()
        {
            Assert.AreEqual(0x0011, HashHelper.Hash(1L, 1));
            Assert.AreEqual(0x01D0, HashHelper.Hash(32L, 31));

            // array
            long[] alLong = { 1L };
            Assert.AreEqual(0x00000101, HashHelper.Hash(alLong, 1));
        }

        [Test]
        public void TestShort()
        {
            Assert.AreEqual(0x0011, HashHelper.Hash((short) 1, 1));
            Assert.AreEqual(0x01D0, HashHelper.Hash((short) 32, 31));

            // array
            short[] ashShort = { 1 };
            Assert.AreEqual(0x00000101, HashHelper.Hash(ashShort, 1));
        }

        [Test]
        public void TestObject()
        {
            IList values = new ArrayList();
            values.Add(1);
            // whilst the below expected values may not mean a lot to the 
            // human eye, they are the expected hashes
            Assert.AreEqual(0x000014DF, HashHelper.Hash((object) new[] {true}, 1));
            Assert.AreEqual(0x00001011, HashHelper.Hash((object) new[] {0x1}, 1));
            Assert.AreEqual(0x00001030, HashHelper.Hash((object) new[] {' '}, 1));
            Assert.AreEqual(0x3FF01010, HashHelper.Hash((object) new[] {1.0d}, 1));
            Assert.AreEqual(0x3F801010, HashHelper.Hash((object) new[] {1.0f}, 1));
            Assert.AreEqual(0x00001011, HashHelper.Hash((object) new[] {(int) 1}, 1));
            Assert.AreEqual(0x00001011, HashHelper.Hash((object) new[] {1L}, 1));
            Assert.AreEqual(0x00001011, HashHelper.Hash((object) new[] {(short) 1}, 1));
            Assert.AreEqual(0x00000011, HashHelper.Hash(values, 1));
        }
    }
}
