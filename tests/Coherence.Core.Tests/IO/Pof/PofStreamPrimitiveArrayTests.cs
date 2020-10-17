/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Globalization;
using System.IO;

using NUnit.Framework;

using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class PofStreamPrimitiveArrayTests
    {
        public void initPOF()
        {
            initPOFWriter();
            initPOFReader();
        }

        private void initPOFReader()
        {
            stream.Position = 0;
            reader = new DataReader(stream);
            pofReader = new PofStreamReader(reader, new SimplePofContext());
        }

        private void initPOFWriter()
        {
            stream = new MemoryStream();
            writer = new DataWriter(stream);
            pofWriter = new PofStreamWriter(writer, new SimplePofContext());
        }

        [Test]
        public void TestBooleanArray()
        {
            var array1   = new bool[] { false, false, true };
            var array3   = new bool[] { false, false, true };
            var array2   = new bool[] { false, true, false, false, false, false, false, false };
            var objArray = new object[] {false, true, true, true};
            var dArray   = new double[] {0.0,0.0,1.0};
            var al       = new ArrayList(0);

            initPOFWriter();
            pofWriter.WriteBooleanArray(0, array1);
            pofWriter.WriteBooleanArray(0, array2);
            pofWriter.WriteBooleanArray(0, array3);
            pofWriter.WriteBooleanArray(0, array3);
            pofWriter.WriteBooleanArray(0, null);
            pofWriter.WriteArray(0, objArray);
            pofWriter.WriteDoubleArray(0, dArray);
            pofWriter.WriteCollection(0, al);

            initPOFReader();
            Assert.AreEqual(array3, pofReader.ReadBooleanArray(0));
            Assert.AreEqual(array2, pofReader.ReadBooleanArray(0));
            Assert.AreEqual(array1, pofReader.ReadBooleanArray(0));
            Assert.AreEqual(array1, pofReader.ReadBooleanArray(0));
            Assert.AreEqual(null, pofReader.ReadBooleanArray(0));
            Assert.AreEqual(objArray, pofReader.ReadBooleanArray(0));
            Assert.AreEqual(array1, pofReader.ReadBooleanArray(0));
            Assert.AreEqual(al.ToArray(), pofReader.ReadBooleanArray(0));
        }

        [Test]
        public void TestReadBooleanArrayWithException()
        {
            string str = "string_booleanarray";
            initPOFWriter();
            pofWriter.WriteString(0, str);
            initPOFReader();
            Assert.That(() => pofReader.ReadBooleanArray(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteBooleanArrayWithException()
        {
            var array = new bool[] {true, false, false};
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteBooleanArray(0, array), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestByteArray()
        {
            var array1 = new byte[] { 1, 22, 8, 0, Byte.MinValue, Byte.MaxValue };
            var array2 = new byte[] { 1, 22, Byte.MinValue, Byte.MaxValue };
            var array3 = new byte[] { 2, Byte.MaxValue, 212, Byte.MinValue };
            var al = new ArrayList(0);
            var objArray = new object[] { 1, Byte.MinValue, Byte.MaxValue };

            initPOFWriter();
            pofWriter.WriteByteArray(0, array1);
            pofWriter.WriteByteArray(0, array2);
            pofWriter.WriteByteArray(0, array3);
            pofWriter.WriteByteArray(0, null);
            pofWriter.WriteCollection(0, al);
            pofWriter.WriteArray(0, objArray);
            pofWriter.WriteObject(0, array1);

            initPOFReader();
            Assert.AreEqual(array1, pofReader.ReadByteArray(0));
            Assert.AreEqual(array2, pofReader.ReadByteArray(0));
            Assert.AreEqual(array3, pofReader.ReadByteArray(0));
            Assert.AreEqual(null, pofReader.ReadByteArray(0));
            Assert.AreEqual(al.ToArray(), pofReader.ReadByteArray(0));
            Assert.AreEqual(objArray, pofReader.ReadByteArray(0));
            object objArr = pofReader.ReadObject(0);
            Assert.AreEqual(array1, objArr);
        }

        [Test]
        public void TestReadByteArrayWithException()
        {
            string str = "sring_bytearray";
            initPOFWriter();
            pofWriter.WriteString(0, str);
            initPOFReader();
            Assert.That(() => pofReader.ReadByteArray(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteByteArrayWithException()
        {
            var array = new byte[] { 1, 1, Byte.MinValue };
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteByteArray(0, array), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestCharArray()
        {
            var array1   = new char[] { 'a', Char.MaxValue, Char.MinValue, (char)0x007F, (char)0x0080, (char)0x0800 };
            var array2   = new char[] { 'B', Char.MaxValue, Char.MinValue, (char)0x07FF, (char)0x08FF };
            var objArray = new object[] { 'a', Char.MinValue, Char.MaxValue };
            var al       = new ArrayList(0);
            String str   = "string_char";

            // Create a character array with multi-bytes character.
            string gkNumber = Char.ConvertFromUtf32(0x10154);
            char[] chars    = new[] { 'z', 'a', '\u0306', '\u01FD', '\u03B2', gkNumber[0], gkNumber[1] };
            // Create a string with multi-bytes character.
            String multiStr = "abc" + Char.ConvertFromUtf32(Int32.Parse("2A601", NumberStyles.HexNumber)) + "def";

            initPOFWriter();
            pofWriter.WriteCharArray(0, array1);
            pofWriter.WriteCharArray(0, array2);
            pofWriter.WriteCharArray(0, null);
            pofWriter.WriteArray(0, objArray);
            pofWriter.WriteCollection(0, al);
            pofWriter.WriteString(0, str);
            //pofWriter.WriteCharArray(0, chars);
            pofWriter.WriteString(0, multiStr);

            initPOFReader();
            Assert.AreEqual(array1, pofReader.ReadCharArray(0));
            Assert.AreEqual(array2, pofReader.ReadCharArray(0));
            Assert.AreEqual(null, pofReader.ReadCharArray(0));
            Assert.AreEqual(objArray, pofReader.ReadCharArray(0));
            Assert.AreEqual(al.ToArray(), pofReader.ReadCharArray(0));
            Assert.AreEqual(str.ToCharArray(), pofReader.ReadCharArray(0));
            // TODO: re-enable this test
            //Assert.AreEqual(chars, pofReader.ReadCharArray(0));
            Assert.AreEqual(multiStr, pofReader.ReadString(0));
        }

        [Test]
        public void TestReadCharArrayWithException()
        {
            initPOFWriter();
            pofWriter.WriteDouble(0, 1.0);
            initPOFReader();
            Assert.That(() => pofReader.ReadCharArray(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteCharArrayWithException()
        {
            var array = new char[] { 'a', Char.MaxValue };
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteCharArray(0, array), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestInt16Array()
        {
            var array1   = new Int16[] { 0,12222, Int16.MaxValue, Int16.MinValue};
            var array2   = new Int16[] { 1, -1, Int16.MaxValue, Int16.MinValue};
            var objArray = new object[] {1, 2, (Byte) 20, (Int64)100};
            var al       = new ArrayList(0);
            var dArray   = new double[] {1.0, 0.0, -1.0};

            initPOFWriter();
            pofWriter.WriteInt16Array(0, array1);
            pofWriter.WriteInt16Array(0, array2);
            pofWriter.WriteInt16Array(0, null);
            pofWriter.WriteArray(0, objArray );
            pofWriter.WriteCollection(0, al );
            pofWriter.WriteDoubleArray(0, dArray );

            initPOFReader();
            Assert.AreEqual(array1, pofReader.ReadInt16Array(0));
            Assert.AreEqual(array2, pofReader.ReadInt16Array(0));
            Assert.AreEqual(null, pofReader.ReadInt16Array(0));
            Assert.AreEqual(objArray, pofReader.ReadInt16Array(0));
            Assert.AreEqual(al.ToArray(), pofReader.ReadInt16Array(0));
            Assert.AreEqual(dArray, pofReader.ReadInt16Array(0));
        }

        [Test]
        public void TestReadInt16ArrayWithException()
        {
            initPOFWriter();
            pofWriter.WriteString(0, "string_shortarray");
            initPOFReader();
            Assert.That(() => pofReader.ReadInt16Array(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteInt16ArrayWithException()
        {
            var array = new Int16[] { 0, -1, Int16.MaxValue };
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteInt16Array(0, array), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestInt32Array()
        {
            var array1   = new Int32[] { 0, Int32.MaxValue, Int32.MinValue };
            var array2   = new Int32[] { 1, -1, Int32.MaxValue, Int32.MinValue };
            var objArray = new object[] { 1, 2, (Byte)20, (Int64)100 };
            var al       = new ArrayList(0);
            var dArray   = new double[] { 1.0, 0.0, -1.0 };

            initPOFWriter();
            pofWriter.WriteInt32Array(0, array1);
            pofWriter.WriteInt32Array(0, array2);
            pofWriter.WriteInt32Array(0, null);
            pofWriter.WriteArray(0, objArray);
            pofWriter.WriteCollection(0, al);
            pofWriter.WriteDoubleArray(0, dArray);

            initPOFReader();
            Assert.AreEqual(array1, pofReader.ReadInt32Array(0));
            Assert.AreEqual(array2, pofReader.ReadInt32Array(0));
            Assert.AreEqual(null, pofReader.ReadInt32Array(0));
            Assert.AreEqual(objArray, pofReader.ReadInt32Array(0));
            Assert.AreEqual(al.ToArray(), pofReader.ReadInt32Array(0));
            Assert.AreEqual(dArray, pofReader.ReadInt32Array(0));
        }

        [Test]
        public void TestReadInt32ArrayWithException()
        {
            initPOFWriter();
            pofWriter.WriteString(0, "string_intarray");
            initPOFReader();
            Assert.That(() => pofReader.ReadInt32Array(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteInt32ArrayWithException()
        {
            var array = new Int32[] { 0, -1, Int32.MaxValue };
            initPOFWriter();
            stream.Close();

            Assert.That(() => pofWriter.WriteInt32Array(0, array), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestInt64Array()
        {
            var array1       = new Int64[] { 0, Int64.MaxValue, Int64.MinValue, 8888888L };
            var array2       = new Int64[] { -1, 1, Int64.MinValue, Int64.MaxValue, 88888 };
            var objArray     = new object[] { 1, 2, (Byte)20, null, (Int64)100 };
            var al           = new ArrayList(0);
            var dArray       = new double[] { 1.0, 0.0, -1.0 };
            ILongArray aLong = new LongSortedList();

            initPOFWriter();
            pofWriter.WriteInt64Array(0, array1);
            pofWriter.WriteInt64Array(0, array2);
            pofWriter.WriteInt64Array(0, null);
            pofWriter.WriteArray(0, objArray);
            pofWriter.WriteCollection(0, al);
            pofWriter.WriteDoubleArray(0, dArray);

            aLong.Add("A");
            aLong.Add("B");
            aLong.Add(null);
            aLong.Add("1");
            pofWriter.WriteLongArray(0, aLong, typeof(String));

            initPOFReader();
            Assert.AreEqual(array1, pofReader.ReadInt64Array(0));
            Assert.AreEqual(array2, pofReader.ReadInt64Array(0));
            Assert.AreEqual(null, pofReader.ReadInt64Array(0));
            var objArrayR = new object[5];
            pofReader.ReadArray(0, objArrayR);
            Assert.AreEqual(objArray, objArrayR);
            Assert.AreEqual(al.ToArray(), pofReader.ReadInt64Array(0));
            Assert.AreEqual(dArray, pofReader.ReadInt64Array(0));

            ILongArray aLongResult = new LongSortedList();
            pofReader.ReadLongArray(0, aLongResult);
            Assert.AreEqual(aLongResult.Count, aLong.Count);
            for (int i = 0; i < aLongResult.Count; i++ )
            {
                Assert.AreEqual(aLong[i], aLongResult[i]);
            }
        }

        [Test]
        public void TestReadInt64ArrayWithException()
        {
            initPOFWriter();
            pofWriter.WriteString(0, "string_longarray");
            initPOFReader();
            Assert.That(() => pofReader.ReadInt64Array(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteInt64ArrayWithException()
        {
            var array = new Int64[] { 0, -1, Int64.MaxValue };
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteInt64Array(0, array), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void TestSingleArray()
        {
            var array1     = new Single[] { 0.0F, 0.00025F, Single.NaN, Single.NegativeInfinity, -1.0F};
            var array2     = new Single[] { -1, 1, Single.MinValue, Single.MaxValue, -1111F};
            var objArray   = new object[] { 1, 2, (Byte)20, (Int64)100, Single.NaN };
            var emptyArray = new Single[0];
            var al         = new ArrayList(0);

            initPOFWriter();
            pofWriter.WriteSingleArray(0, array1);
            pofWriter.WriteSingleArray(0, array2);
            pofWriter.WriteSingleArray(0, null);
            pofWriter.WriteArray(0, objArray);
            pofWriter.WriteCollection(0, (ICollection)emptyArray);
            pofWriter.WriteCollection(0, al);

            initPOFReader();
            Assert.AreEqual(array1, pofReader.ReadSingleArray(0));
            Assert.AreEqual(array2, pofReader.ReadSingleArray(0));
            Assert.AreEqual(null, pofReader.ReadSingleArray(0));
            Assert.AreEqual(objArray, pofReader.ReadSingleArray(0));
            Assert.AreEqual(emptyArray, pofReader.ReadSingleArray(0));
            Assert.AreEqual(al.ToArray(), pofReader.ReadSingleArray(0));
        }

        [Test]
        public void TestReadSingleArrayWithException()
        {
            initPOFWriter();
            pofWriter.WriteString(0, "string_singlearray");
            initPOFReader();
            Assert.That(() => pofReader.ReadSingleArray(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteSingleArrayWithException()
        {
            var array = new Single[] { 0.0f, -1.0f, Single.MaxValue, Single.NegativeInfinity };
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteSingleArray(0, array), Throws.TypeOf<ObjectDisposedException>());
            ;
        }

        [Test]
        public void TestDoubleArray()
        {
            var array1     = new Double[] { 0.0, 0.00025, Double.NaN, Double.NegativeInfinity, -1.0 };
            var array2     = new Double[] { -1, 1, Double.MinValue, Double.MaxValue, Double.PositiveInfinity };
            var array3     = new Double[] {0, -1.11111, 0.1};
            var objArray   = new object[] { 1, 2, (Byte)20, (Int64)100, Single.NaN };
            var emptyArray = new Double[0];
            var al         = new ArrayList(0);

            initPOFWriter();
            pofWriter.WriteDoubleArray(0, array1);
            pofWriter.WriteDoubleArray(0, array2);
            pofWriter.WriteDoubleArray(0, null);
            pofWriter.WriteDoubleArray(0, array3);
            pofWriter.WriteArray(0, objArray);
            pofWriter.WriteCollection(0, (ICollection)emptyArray);
            pofWriter.WriteCollection(0, al);

            initPOFReader();
            Assert.AreEqual(array1, pofReader.ReadDoubleArray(0));
            Assert.AreEqual(array2, pofReader.ReadDoubleArray(0));
            Assert.AreEqual(null, pofReader.ReadDoubleArray(0));
            Assert.AreEqual(array3, pofReader.ReadDoubleArray(0));
            Assert.AreEqual(objArray, pofReader.ReadDoubleArray(0));
            Assert.AreEqual(emptyArray, pofReader.ReadDoubleArray(0));
            Assert.AreEqual(al.ToArray(), pofReader.ReadDoubleArray(0));
        }

        [Test]
        public void TestReadDoubleArrayWithException()
        {
            initPOFWriter();
            pofWriter.WriteString(0, "string_doublearray");
            initPOFReader();
            Assert.That(() => pofReader.ReadDoubleArray(0), Throws.TypeOf<IOException>());
        }

        [Test]
        public void TestWriteDoubleArrayWithException()
        {
            var array = new Double[] { 0.0, -1.0, Double.MaxValue, Double.NegativeInfinity };
            initPOFWriter();
            stream.Close();
            Assert.That(() => pofWriter.WriteDoubleArray(0, array), Throws.TypeOf<ObjectDisposedException>());
        }

        // test case for COH-3370
        [Test]
        public void testPofWriterWriteUniformObjectArrayWithNull()
        {
            var ao1 = new object[] {(double)32, Double.MaxValue, (double)-1,
                    null, (double)0};
            var ao2 = new object[] {true, null, false};
            var ao3 = new object[] {(byte)65, null, PofConstants.V_REFERENCE_NULL,
                    (byte)0, null};
            var ao4 = new object[] {'A', 'B', null};
            var ao5 = new object[] {(float)32, float.MaxValue, (float)-1,
                    (float)0, null};
            var ao6 = new object[] {32, Int32.MaxValue, -1, 0, 
                    PofConstants.V_REFERENCE_NULL, null};
            var ao7 = new object[] {new DateTime(2006, 12, 2, 8, 51, 15), null};
            var ao8 = new object[] {"test", "test3", "testPOF1", null, null, "test4"};

            initPOFWriter();
            pofWriter.WriteArray(0, ao1, typeof(Double));
            pofWriter.WriteArray(0, ao2, typeof(Boolean));
            pofWriter.WriteArray(0, ao3, typeof(Byte));
            pofWriter.WriteArray(0, ao4, typeof(Char));
            pofWriter.WriteArray(0, ao5, typeof(float));
            pofWriter.WriteArray(0, ao6, typeof(Int32));
            pofWriter.WriteArray(0, ao7, typeof(DateTime));
            pofWriter.WriteArray(0, ao8, typeof(String));
            pofWriter.WriteArray(0, null, typeof(Object));

            initPOFReader();
            Assert.AreEqual(ao1, pofReader.ReadArray(0));
            Assert.AreEqual(ao2, pofReader.ReadArray(0));
            Assert.AreEqual(ao3, pofReader.ReadArray(0));
            Assert.AreEqual(ao4, pofReader.ReadArray(0));
            Assert.AreEqual(ao5, pofReader.ReadArray(0));
            Assert.AreEqual(ao6, pofReader.ReadArray(0));
            Assert.AreEqual(ao7, pofReader.ReadArray(0));
            Assert.AreEqual(ao8, pofReader.ReadArray(0));
            Assert.AreEqual(null, pofReader.ReadArray(0));
        }

        private DataReader reader;
        private DataWriter writer;
        private PofStreamReader pofReader;
        private PofStreamWriter pofWriter;
        private MemoryStream stream;
    }
}