/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

using NUnit.Framework;

using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class SparseArrayTests
    {
        private IPofContext ctx;
        private MemoryStream stream;
        private ILongArray la;
        
        [SetUp]
        public void SetUp()
        {
            TestContext.Error.WriteLine($"[START] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
            ctx    = new SimplePofContext();
            stream = new MemoryStream();
            la = new LongSortedList();
        }
        
        [TearDown]
        public void TearDown()
        {
            stream.Close();
            TestContext.Error.WriteLine($"[END]   {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
        }
        
        [Test]
        public void TestReadCollection()
        {
            la[1L] = "one";
            la[2L] = "two";
            la.Add("five");
            la.Add("three");
            la[200L] = "twohundred";

            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, ctx);

            pofWriter.WriteLongArray(0, la);
            pofWriter.WriteLongArray(0, new LongSortedList());
            pofWriter.WriteLongArray(0, la, typeof(String));

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, ctx);

            ArrayList res = new ArrayList();
            res = (ArrayList) pofReader.ReadCollection(0, res);
            Assert.AreEqual(la.Count, res.Count);

            IEnumerator e = res.GetEnumerator();
            IEnumerator de = la.GetEnumerator();
            for (; e.MoveNext() && de.MoveNext(); )
            {
                Assert.AreEqual(e.Current, ((DictionaryEntry)de.Current).Value);
            }

            res = (ArrayList)pofReader.ReadCollection(0, null);
            Assert.AreEqual(0, res.Count);
            
            // uniform sparse array
            res = new ArrayList();
            res = (ArrayList)pofReader.ReadCollection(0, res);
            Assert.AreEqual(la.Count, res.Count);

            e = res.GetEnumerator();
            de = la.GetEnumerator();
            for (; e.MoveNext() && de.MoveNext(); )
            {
                Assert.AreEqual(e.Current, ((DictionaryEntry)de.Current).Value);
            }
        }
        
        [Test]
        public void TestReadObject()
        {
            la = new LongSortedList();
            la[1L] = "one";
            la[2L] = "two";
            la.Add("five");
            la.Add("three");
            la[200L] = "twohundred";
            

            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, ctx);
            pofWriter.WriteLongArray(0, la);
            pofWriter.WriteLongArray(0, new LongSortedList());
            pofWriter.WriteLongArray(0, la, typeof(String));
            
            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, ctx);
            
            LongSortedList resLA = (LongSortedList)pofReader.ReadObject(0);
            Assert.AreEqual(la.Count, resLA.Count);

            IEnumerator e = resLA.GetEnumerator();
            IEnumerator de = la.GetEnumerator();
            for (; e.MoveNext() && de.MoveNext(); )
            {
                Assert.AreEqual(((DictionaryEntry)e.Current).Key, ((DictionaryEntry)de.Current).Key);
                Assert.AreEqual(((DictionaryEntry)e.Current).Value, ((DictionaryEntry)de.Current).Value);
            }

            resLA = (LongSortedList)pofReader.ReadObject(0);
            Assert.AreEqual(0, resLA.Count);
            
            //uniform sparse array
            resLA = (LongSortedList)pofReader.ReadObject(0);
            Assert.AreEqual(la.Count, resLA.Count);

            e = resLA.GetEnumerator();
            de = la.GetEnumerator();
            for (; e.MoveNext() && de.MoveNext(); )
            {
                Assert.AreEqual(((DictionaryEntry)e.Current).Key, ((DictionaryEntry)de.Current).Key);
                Assert.AreEqual(((DictionaryEntry)e.Current).Value, ((DictionaryEntry)de.Current).Value);
            }
        }
        
        [Test]
        public void TestReadBooleanArray()
        {
            la = new LongSortedList();
            la[1L] = true;
            la[3L] = false;
            la[30L] = true;

            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, ctx);
            pofWriter.WriteLongArray(0, la);
            pofWriter.WriteLongArray(0, new LongSortedList());
            pofWriter.WriteLongArray(0, la, typeof(bool));

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, ctx);

            bool[] resBoolArray = pofReader.ReadBooleanArray(0);
            Assert.AreEqual(la.LastIndex+1, resBoolArray.Length);

            Assert.AreEqual(la[1L], resBoolArray[1]);
            Assert.AreEqual(la[3L], resBoolArray[3]);
            Assert.AreEqual(la[30L], resBoolArray[30]);

            resBoolArray = pofReader.ReadBooleanArray(0);
            Assert.AreEqual(0, resBoolArray.Length);
            
            //uniform sparse array
            resBoolArray = pofReader.ReadBooleanArray(0);
            Assert.AreEqual(la.LastIndex + 1, resBoolArray.Length);

            Assert.AreEqual(la[1L], resBoolArray[1]);
            Assert.AreEqual(la[3L], resBoolArray[3]);
            Assert.AreEqual(la[30L], resBoolArray[30]);
        }

        [Test]
        public void TestReadByteArray()
        {
            la = new LongSortedList();
            la[1L]  = (byte) 1;
            la[3L]  = (byte) 3;
            la[30L] = (byte)30;

            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, ctx);
            pofWriter.WriteLongArray(0, la);
            pofWriter.WriteLongArray(0, new LongSortedList());
            pofWriter.WriteLongArray(0, la, typeof(byte));

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, ctx);

            byte[] resByteArray = pofReader.ReadByteArray(0);
            Assert.AreEqual(la.LastIndex + 1, resByteArray.Length);

            Assert.AreEqual(la[1L], resByteArray[1]);
            Assert.AreEqual(la[3L], resByteArray[3]);
            Assert.AreEqual(la[30L], resByteArray[30]);

            resByteArray = pofReader.ReadByteArray(0);
            Assert.AreEqual(0, resByteArray.Length);

            //uniform sparse array
            resByteArray = pofReader.ReadByteArray(0);
            Assert.AreEqual(la.LastIndex + 1, resByteArray.Length);

            Assert.AreEqual(la[1L], resByteArray[1]);
            Assert.AreEqual(la[3L], resByteArray[3]);
            Assert.AreEqual(la[30L], resByteArray[30]);
            
        }

        [Test]
        public void TestReadCharArray()
        {
            la = new LongSortedList();
            la[1L] = 'a';
            la[3L] = 'b';
            la[20L] = 'c';

            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, ctx);
            pofWriter.WriteLongArray(0, la);
            pofWriter.WriteLongArray(0, new LongSortedList());
            pofWriter.WriteLongArray(0, la, typeof(char));

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, ctx);

            char[] resCharArray = pofReader.ReadCharArray(0);
            Assert.AreEqual(la.LastIndex + 1, resCharArray.Length);

            Assert.AreEqual(la[1L], resCharArray[1]);
            Assert.AreEqual(la[3L], resCharArray[3]);
            Assert.AreEqual(la[20L], resCharArray[20]);

            resCharArray = pofReader.ReadCharArray(0);
            Assert.AreEqual(0, resCharArray.Length);

            //uniform sparse array
            resCharArray = pofReader.ReadCharArray(0);
            Assert.AreEqual(la.LastIndex + 1, resCharArray.Length);

            Assert.AreEqual(la[1L], resCharArray[1]);
            Assert.AreEqual(la[3L], resCharArray[3]);
            Assert.AreEqual(la[20L], resCharArray[20]);
        }

        [Test]
        public void TestReadInt16Array()
        {
            la = new LongSortedList();
            la[1L] = (short)1;
            la[3L] = (short)0;
            la[30L] = Int16.MaxValue;

            LongSortedList la1 = new LongSortedList();
            la1[1L] = (Single)1.0;
            la1[10L] = (Single)10.0;
            la1[11L] = (Single)11.0;

            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, ctx);
            // writting ILongArray
            pofWriter.WriteLongArray(0, la);
            pofWriter.WriteLongArray(0, new LongSortedList());
            // writting uniform ILongArray
            pofWriter.WriteLongArray(0, la, typeof(Int16));
            pofWriter.WriteLongArray(0, la1, typeof(Single));

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, ctx);

            //reading ILongArray
            Int16[] resInt16Array = pofReader.ReadInt16Array(0);
            Assert.AreEqual(la.LastIndex + 1, resInt16Array.Length);

            Assert.AreEqual(la[1L], resInt16Array[1]);
            Assert.AreEqual(la[3L], resInt16Array[3]);
            Assert.AreEqual(la[30L], resInt16Array[30]);

            resInt16Array = pofReader.ReadInt16Array(0);
            Assert.AreEqual(0, resInt16Array.Length);

            //reading  uniform ILongArray
            resInt16Array = pofReader.ReadInt16Array(0);
            Assert.AreEqual(la.LastIndex + 1, resInt16Array.Length);

            Assert.AreEqual(la[1L], resInt16Array[1]);
            Assert.AreEqual(la[3L], resInt16Array[3]);
            Assert.AreEqual(la[30L], resInt16Array[30]);

            resInt16Array = pofReader.ReadInt16Array(0);
            Assert.AreEqual(la1.LastIndex + 1, resInt16Array.Length);

            Assert.AreEqual(la1[1L], resInt16Array[1]);
            Assert.AreEqual(la1[10L], resInt16Array[10]);
            Assert.AreEqual(la1[11L], resInt16Array[11]);
        }

        [Test]
        public void TestReadInt32Array()
        {
            la = new LongSortedList();
            la[1L] = 0;
            la[3L] = 1;
            la[30L] = Int32.MaxValue;
            la[31L] = Int32.MinValue;

            LongSortedList la1 = new LongSortedList();
            la1[1L] = (Single)1.0;
            la1[10L] = (Single)10.0;
            la1[11L] = (Single)11.0;

            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, ctx);
            // writting ILongArray
            pofWriter.WriteLongArray(0, la);
            pofWriter.WriteLongArray(0, new LongSortedList());
            // writting uniform ILongArray
            pofWriter.WriteLongArray(0, la, typeof(Int32));
            pofWriter.WriteLongArray(0, la1, typeof(Single));

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, ctx);

            // reading ILongArray
            Int32[] resInt32Array = pofReader.ReadInt32Array(0);
            Assert.AreEqual(la.LastIndex + 1, resInt32Array.Length);

            Assert.AreEqual(la[1L], resInt32Array[1]);
            Assert.AreEqual(la[3L], resInt32Array[3]);
            Assert.AreEqual(la[30L], resInt32Array[30]);
            Assert.AreEqual(la[31L], resInt32Array[31]);

            resInt32Array = pofReader.ReadInt32Array(0);
            Assert.AreEqual(0, resInt32Array.Length);

            // reading  uniform ILongArray
            resInt32Array = pofReader.ReadInt32Array(0);
            Assert.AreEqual(la.LastIndex + 1, resInt32Array.Length);

            Assert.AreEqual(la[1L], resInt32Array[1]);
            Assert.AreEqual(la[3L], resInt32Array[3]);
            Assert.AreEqual(la[30L], resInt32Array[30]);
            Assert.AreEqual(la[31L], resInt32Array[31]);

            resInt32Array = pofReader.ReadInt32Array(0);
            Assert.AreEqual(la1.LastIndex + 1, resInt32Array.Length);

            Assert.AreEqual(la1[1L], resInt32Array[1]);
            Assert.AreEqual(la1[10L], resInt32Array[10]);
            Assert.AreEqual(la1[11L], resInt32Array[11]);
        }

        [Test]
        public void TestReadInt64Array()
        {
            la = new LongSortedList();
            la[1L] = (Int64)0;
            la[3L] = (Int64)1;
            la[30L] = Int64.MinValue;

            LongSortedList la1 = new LongSortedList();
            la1[1L] = (Single)1.0;
            la1[10L] = (Single)10.0;
            la1[11L] = (Single)11.0;

            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, ctx);
            pofWriter.WriteLongArray(0, la);
            pofWriter.WriteLongArray(0, new LongSortedList());            
            pofWriter.WriteLongArray(0, la, typeof(Int64));
            pofWriter.WriteLongArray(0, la1, typeof(Single));

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, ctx);

            // reading ILongArray
            Int64[] resInt64Array = pofReader.ReadInt64Array(0);
            Assert.AreEqual(la.LastIndex + 1, resInt64Array.Length);
            
            Assert.AreEqual(la[1L], resInt64Array[1]);
            Assert.AreEqual(la[3L], resInt64Array[3]);
            Assert.AreEqual(la[30L], resInt64Array[30]);

            resInt64Array = pofReader.ReadInt64Array(0);
            Assert.AreEqual(0, resInt64Array.Length);
            
            // reading uniform ILongArray
            resInt64Array = pofReader.ReadInt64Array(0);
            Assert.AreEqual(la.LastIndex + 1, resInt64Array.Length);

            Assert.AreEqual(la[1L], resInt64Array[1]);
            Assert.AreEqual(la[3L], resInt64Array[3]);
            Assert.AreEqual(la[30L], resInt64Array[30]);

            resInt64Array = pofReader.ReadInt64Array(0);
            Assert.AreEqual(la1.LastIndex + 1, resInt64Array.Length);

            Assert.AreEqual(la1[1L], resInt64Array[1]);
            Assert.AreEqual(la1[10L], resInt64Array[10]);
            Assert.AreEqual(la1[11L], resInt64Array[11]);
        }

        [Test]
        public void TestReadSingleArray()
        {
            la = new LongSortedList();
            la[1L] = 1F;
            la[3L] = 3F;
            la[20L] = 20F;

            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, ctx);
            pofWriter.WriteLongArray(0, new LongSortedList());
            pofWriter.WriteLongArray(0, la);
            pofWriter.WriteLongArray(0, la, typeof(float));

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, ctx);

            float[] resFloatArray = pofReader.ReadSingleArray(0);
            Assert.AreEqual(0, resFloatArray.Length);

            resFloatArray = pofReader.ReadSingleArray(0);
            Assert.AreEqual(la.LastIndex + 1, resFloatArray.Length);

            Assert.AreEqual(la[1L], resFloatArray[1]);
            Assert.AreEqual(la[3L], resFloatArray[3]);
            Assert.AreEqual(la[20L], resFloatArray[20]);
            
            //uniform sparse array
            resFloatArray = pofReader.ReadSingleArray(0);
            Assert.AreEqual(la.LastIndex + 1, resFloatArray.Length);

            Assert.AreEqual(la[1L], resFloatArray[1]);
            Assert.AreEqual(la[3L], resFloatArray[3]);
            Assert.AreEqual(la[20L], resFloatArray[20]);
        }

        [Test]
        public void TestReadDoubleArray()
        {
            la = new LongSortedList();
            la[1L] = 1.0;
            la[3L] = 3.0;
            la[20L] = 20.0;

            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, ctx);
            pofWriter.WriteLongArray(0, new LongSortedList());
            pofWriter.WriteLongArray(0, la);
            pofWriter.WriteLongArray(0, la, typeof(double));

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, ctx);

            double[] resFloatArray = pofReader.ReadDoubleArray(0);
            Assert.AreEqual(0, resFloatArray.Length);            
            
            resFloatArray = pofReader.ReadDoubleArray(0);
            Assert.AreEqual(la.LastIndex + 1, resFloatArray.Length);

            Assert.AreEqual(la[1L], resFloatArray[1]);
            Assert.AreEqual(la[3L], resFloatArray[3]);
            Assert.AreEqual(la[20L], resFloatArray[20]);

            //uniform sparse array
            resFloatArray = pofReader.ReadDoubleArray(0);
            Assert.AreEqual(la.LastIndex + 1, resFloatArray.Length);

            Assert.AreEqual(la[1L], resFloatArray[1]);
            Assert.AreEqual(la[3L], resFloatArray[3]);
            Assert.AreEqual(la[20L], resFloatArray[20]);
        }
        
        [Test]
        public void TestReadString()
        {
            la = new LongSortedList();
            la[1L] = 'a';
            la[3L] = 'b';
            la[20L] = 'c';

            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, ctx);
            pofWriter.WriteLongArray(0, la);
            pofWriter.WriteLongArray(0, new LongSortedList());
            pofWriter.WriteLongArray(0, la, typeof(char));

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, ctx);

            String resCharArray = pofReader.ReadString(0);
            Assert.AreEqual(la.LastIndex + 1, resCharArray.Length);

            Assert.AreEqual(la[1L], resCharArray[1]);
            Assert.AreEqual(la[3L], resCharArray[3]);
            Assert.AreEqual(la[20L], resCharArray[20]);

            resCharArray = pofReader.ReadString(0);
            Assert.IsTrue(resCharArray.Length == 0);

            //uniform sparse array
            resCharArray = pofReader.ReadString(0);
            Assert.AreEqual(la.LastIndex + 1, resCharArray.Length);

            Assert.AreEqual(la[1L], resCharArray[1]);
            Assert.AreEqual(la[3L], resCharArray[3]);
            Assert.AreEqual(la[20L], resCharArray[20]);
        }

        [Test]
        public void TestReadLongArray()
        {
            la = new LongSortedList();
            la[1L] = 'a';
            la[3L] = 'b';
            la[20L] = 'c';
            byte[] bytes = new byte[] {1, 250, 3};   

            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, ctx);
            pofWriter.WriteLongArray(0, la);
            pofWriter.WriteLongArray(0, la, typeof(char));
            pofWriter.WriteLongArray(0, new LongSortedList());
            pofWriter.WriteLongArray(0, null);
            pofWriter.WriteArray(0, bytes);
            pofWriter.WriteArray(0, bytes, typeof(byte));
            pofWriter.WriteArray(0, new byte[0]);

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, ctx);

            ILongArray resLongArray = pofReader.ReadLongArray(0, null);
            Assert.AreEqual(la.Count, resLongArray.Count);
            Assert.AreEqual(la[1], resLongArray[1]);
            Assert.AreEqual(la[3], resLongArray[3]);
            Assert.AreEqual(la[20], resLongArray[20]);

            //uniform sparse array
            resLongArray = pofReader.ReadLongArray(0, null);
            Assert.AreEqual(la.Count, resLongArray.Count);
            Assert.AreEqual(la[1], resLongArray[1]);
            Assert.AreEqual(la[3], resLongArray[3]);
            Assert.AreEqual(la[20], resLongArray[20]);

            resLongArray = pofReader.ReadLongArray(0, null);
            Assert.AreEqual(0, resLongArray.Count);

            resLongArray = pofReader.ReadLongArray(0, null);
            Assert.IsTrue(resLongArray == null);

            resLongArray = pofReader.ReadLongArray(0, null);
            Assert.AreEqual(3, resLongArray.Count);

            resLongArray = pofReader.ReadLongArray(0, null);
            Assert.AreEqual(3, resLongArray.Count);

            resLongArray = pofReader.ReadLongArray(0, new LongSortedList());
            Assert.AreEqual(0, resLongArray.Count);
        }
    }
}
