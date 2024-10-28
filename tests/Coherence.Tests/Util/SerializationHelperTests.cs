/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO;

using NUnit.Framework;
using Tangosol.IO.Pof;

namespace Tangosol.Util
{
    [TestFixture]
    public class SerializationHelperTests
    {
        [Test]
        public void TestToAndFromBinary()
        {
            ISerializer serializer = new ConfigurablePofContext("Config/include-pof-config.xml");
            string      original   = "hello";
            Binary      bin        = SerializationHelper.ToBinary(original, serializer);
            string      copy       = (string) SerializationHelper.FromBinary(bin, serializer);
            Assert.AreEqual(original, copy);

            var stream = new BinaryMemoryStream();
            serializer.Serialize(new DataWriter(stream), original);
            try
            {
                // throws IOException as type (fmt_ext) information is missing
                SerializationHelper.FromBinary(stream.ToBinary(), serializer);
                Assert.Fail();
            }
            catch (IOException)
            {
            }
        }

        [Test]
        public void TestDecoration()
        {
            ISerializer serializer = new ConfigurablePofContext("Config/include-pof-config.xml");
            string      original   = "hello";
            Binary      bin        = SerializationHelper.ToBinary(original, serializer);

            Assert.IsFalse(SerializationHelper.IsIntDecorated(bin));
            bin = SerializationHelper.DecorateBinary(bin, 6);
            Assert.IsTrue(SerializationHelper.IsIntDecorated(bin));
            Assert.AreEqual(SerializationHelper.ExtractIntDecoration(bin), 6);

            // verify adding a decoration does not impact the deserialized Object
            string copy = (string) SerializationHelper.FromBinary(bin, serializer);
            Assert.AreEqual(original, copy);

            // handle empty Binary appropriately
            bin = new Binary();
            Assert.IsFalse(SerializationHelper.IsIntDecorated(bin));
            try
            {
                SerializationHelper.ExtractIntDecoration(bin);
                Assert.Fail();
            }
            catch (System.ArgumentException)
            {
            }
        }

        private class SerializationStatsHelper : SerializationHelper
        {
            static public void runTest()
            {
                Stats stats = new Stats();

                BinaryMemoryStream buf = null;

                // populate stats to give an average
                stats.Update(10000);
                stats.Update(11000);
                stats.Update(12000);
                stats.Update(13000);
                stats.Update(14000);

                buf = stats.InstantiateBuffer();
                Assert.AreEqual(buf.Capacity, (14000));

                // now make a really big request
                stats.Update(1024*1024*4);
                buf = stats.InstantiateBuffer();
                Assert.AreEqual(buf.Capacity, (1024*1024*4));

                // request an average size buffer and check the size
                stats.Update(1000);
                buf = stats.InstantiateBuffer();
                Assert.AreEqual(buf.Capacity, (1000 + 8));
            }
        }


        [Test]
        public void TestStats()
        {
        SerializationStatsHelper.runTest();
        }
    }
}
