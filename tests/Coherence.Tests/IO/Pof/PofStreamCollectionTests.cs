/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class PofStreamCollectionTests
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

        [Test]
        public void TestPofStreamWriteArray()
        {
            initPOFWriter();
            Object[] aobj1 = { "test", "test3", "testPOF1" };
            Object[] aobj2 = { "test", "test1", "testPOF2" };
            var aobj3      = new Object[0];
            var objArray   = new Object[] { 11, "test", true };

            pofwriter.WriteArray(0, null);
            pofwriter.WriteArray(0, new object[0]);
            pofwriter.WriteArray(0, aobj1);
            pofwriter.WriteArray(0, aobj2);
            pofwriter.WriteArray(0, aobj3);
            pofwriter.WriteArray(0, objArray);


            initPOFReader();
            Assert.AreEqual(null, pofreader.ReadArray(0, null));
            Assert.AreEqual(new Object[0], pofreader.ReadArray(0, new Object[0]));
            Assert.AreEqual(aobj1, pofreader.ReadArray(0, new object[3]));
            Assert.AreEqual(aobj2, pofreader.ReadArray(0, new object[3]));
            Assert.AreEqual(aobj3, pofreader.ReadArray(0, new object[0]));
            Assert.AreEqual(objArray, pofreader.ReadArray(0, new object[3]));
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestPofStreamWriteArrayException()
        {
            initPOFWriter();
            stream.Close();
            pofwriter.WriteArray(0, new object[] { "test", "test1", "test2" });
        }

        [Test]
        public void TestPofStreamWriteUniformArray()
        {
            initPOFWriter();
            Object[] aobj1 = { "test", "test3", "testPOF1" };
            Object[] aobj2 = { 32, Int32.MaxValue, -1 };
            pofwriter.WriteArray(0, aobj1, typeof(String));
            pofwriter.WriteArray(0, aobj2, typeof(Int32));

            initPOFReader();
            Assert.AreEqual(aobj1, pofreader.ReadArray(0, new object[3]));
            Assert.AreEqual(aobj2, pofreader.ReadArray(0, new object[3]));
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestPofStreamWriteUniformArrayException()
        {
            initPOFWriter();
            stream.Close();
            pofwriter.WriteArray(0, new object[] { "test", "test1", "test2" }, typeof(String));
        }

        [Test]
        [ExpectedException(typeof(IOException))]
        public void TestPofStreamWriteWriteCollection()
        {
            initPOFWriter();
            var col1 = new ArrayList();
            var col2 = new ArrayList();
            col1.Add("A"); col1.Add("Z"); col1.Add("7");
            col2.Add(32); col2.Add(Int32.MinValue); col2.Add(Int32.MaxValue);

            pofwriter.WriteCollection(0, null);
            pofwriter.WriteCollection(0, col1);
            pofwriter.WriteCollection(0, col2);
            pofwriter.WriteCollection(0, col1);
            pofwriter.WriteDate(0, new DateTime(2006, 8, 8));

            initPOFReader();
            Assert.AreEqual(null, pofreader.ReadCollection(0, null));
            Assert.AreEqual(col1.ToArray(), ((ArrayList)pofreader.ReadCollection(0, new ArrayList(3))).ToArray());
            Assert.AreEqual(col2.ToArray(), ((ArrayList)pofreader.ReadCollection(0, new ArrayList(3))).ToArray());
            Assert.AreEqual(col1.ToArray(), pofreader.ReadCollection(0, null));
            Assert.AreEqual(col1.ToArray(), pofreader.ReadCollection(0, null)); // exception
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestPofStreamWriteCollectionException()
        {
            initPOFWriter();
            stream.Close();
            pofwriter.WriteCollection(0, new ArrayList(0));
        }

        [Test]
        public void TestPofStreamWriteWriteCollectionEx1()
        {
            initPOFWriter();
            var col1 = new ArrayList();
            col1.Add("A"); col1.Add("Z"); col1.Add("7");

            pofwriter.WriteCollection(0, col1);
            pofwriter.WriteCollection(0, new ArrayList(0));

            initPOFReader();

            object result1 = pofreader.ReadObject(0);
            Assert.IsTrue(result1 is ICollection);
            var tmp = new ArrayList(result1 as ICollection);
            Assert.IsTrue(tmp.Count == 3);
            Assert.AreEqual(col1[0], tmp[0]);
            Assert.AreEqual(col1[1], tmp[1]);
            Assert.AreEqual(col1[2], tmp[2]);

            result1 = pofreader.ReadObject(0);
            Assert.IsTrue(result1 is ICollection);
            tmp = new ArrayList(result1 as ICollection);
            Assert.IsTrue(tmp.Count == 0);
        }

        [Test]
        public void TestPofStreamWriteWriteUniformCollection()
        {
            initPOFWriter();
            var col1 = new ArrayList();
            var col2 = new ArrayList();
            col1.Add("A");
            col1.Add("Z");
            col1.Add("7");
            col2.Add(32);
            col2.Add(Int32.MinValue);
            col2.Add(Int32.MaxValue);

            pofwriter.WriteCollection(0, col1, typeof(String));
            pofwriter.WriteCollection(0, col2, typeof(Int32));
            pofwriter.WriteCollection(0, new ArrayList(0), typeof(String));
            pofwriter.WriteCollection(0, (ICollection)null, typeof(String));

            initPOFReader();
            ICollection result1 = pofreader.ReadCollection(0, new ArrayList());
            Assert.IsTrue(result1 is ArrayList);
            ArrayList tmp = (ArrayList)result1;
            Assert.IsTrue(result1.Count == 3);
            Assert.AreEqual(col1[0], tmp[0]);
            Assert.AreEqual(col1[1], tmp[1]);
            Assert.AreEqual(col1[2], tmp[2]);

            ICollection result2 = pofreader.ReadCollection(0, null);
            Assert.IsTrue(result2 is Array);
            Assert.AreEqual(col2.ToArray(typeof(int)), result2);
            Assert.AreEqual(new object[0], pofreader.ReadCollection(0, (ICollection)new object[0]));
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestPofStreamWriteUniformCollectionException()
        {
            initPOFWriter();
            stream.Close();
            pofwriter.WriteCollection(0, new ArrayList(0), typeof(String));
        }

        // test case for COH-3370
        [Test]
        public void testPofWriterWriteUniformCollectionWithNulls()
        {
            var list1 = new ArrayList();
            var list2 = new ArrayList();
            var list3 = new ArrayList();
            var list4 = new ArrayList();
            list1.Add("A");
            list1.Add("Z");
            list1.Add("7");
            list1.Add(null);
            list2.Add(32);
            list2.Add(Int32.MinValue);
            list2.Add(Int32.MaxValue);
            list2.Add(-1);
            list2.Add(null);
            list3.Add((long)64);
            list3.Add(Int64.MinValue);
            list3.Add(Int64.MaxValue);
            list3.Add(null);
            list3.Add((long)-1);
            list4.Add((short)16);
            list4.Add(Int16.MinValue);
            list4.Add(Int16.MaxValue);
            list4.Add(null);
            list4.Add((short)-1);

            initPOFWriter();
            pofwriter.WriteCollection(0, list1, typeof(String));
            pofwriter.WriteCollection(0, list2, typeof(Int32));
            pofwriter.WriteCollection(0, list3, typeof(Int64));
            pofwriter.WriteCollection(0, list4, typeof(Int16));
            pofwriter.WriteCollection(0, new ArrayList(0), typeof(String));
            pofwriter.WriteCollection(0, (ICollection)null, typeof(String));

            initPOFReader();
            ICollection col1 = pofreader.ReadCollection(0, new ArrayList());
            Assert.IsTrue(col1 is ArrayList);
            var listTmp = (ArrayList) col1;
            
            Assert.IsTrue(listTmp.Count == list1.Count);
            Assert.AreEqual(list1[0], listTmp[0]);
            Assert.AreEqual(list1[1], listTmp[1]);
            Assert.AreEqual(list1[2], listTmp[2]);
            Assert.AreEqual(list1[3], listTmp[3]);

            ICollection col2 = pofreader.ReadCollection(0, new ArrayList());
            listTmp = (ArrayList)col2;
            Assert.IsTrue(listTmp.Count == list2.Count);
            Assert.AreEqual(list2[0], listTmp[0]);
            Assert.AreEqual(list2[1], listTmp[1]);
            Assert.AreEqual(list2[2], listTmp[2]);
            Assert.AreEqual(list2[3], listTmp[3]);
            Assert.AreEqual(list2[4], listTmp[4]);

            ICollection col3 = pofreader.ReadCollection(0, new ArrayList());
            listTmp = (ArrayList)col3;
            Assert.IsTrue(listTmp.Count == list3.Count);
            Assert.AreEqual(list3[0], listTmp[0]);
            Assert.AreEqual(list3[1], listTmp[1]);
            Assert.AreEqual(list3[2], listTmp[2]);
            Assert.AreEqual(list3[3], listTmp[3]);
            Assert.AreEqual(list3[4], listTmp[4]);

            ICollection col4 = pofreader.ReadCollection(0, new ArrayList());
            listTmp = (ArrayList)col4;
            Assert.IsTrue(listTmp.Count == list4.Count);
            Assert.AreEqual(list4[0], listTmp[0]);
            Assert.AreEqual(list4[1], listTmp[1]);
            Assert.AreEqual(list4[2], listTmp[2]);
            Assert.AreEqual(list4[3], listTmp[3]);
            Assert.AreEqual(list4[4], listTmp[4]);

            Assert.AreEqual(new object[0], pofreader.ReadCollection(0, (ICollection)new object[0]));
            Assert.AreEqual(null, pofreader.ReadCollection(0, null));
        }

        [Test]
        public void TestPofStreamWriteWriteDictionary()
        {
            initPOFWriter();
            var col1 = new Hashtable();
            var col2 = new Hashtable();
            col1.Add(0, "A"); col1.Add(1, "Z"); col1.Add(2, "7");
            col2.Add(5, 32); col2.Add(10, Int32.MinValue); col2.Add(15, Int32.MaxValue);

            pofwriter.WriteDictionary(0, col1);
            pofwriter.WriteDictionary(0, col2);
            pofwriter.WriteDictionary(0, null);
            pofwriter.WriteDictionary(0, col1);

            initPOFReader();
            IDictionary rcol1 = pofreader.ReadDictionary(0, new Hashtable(3));
            IDictionary rcol2 = pofreader.ReadDictionary(0, new Hashtable(3));

            #region Compare entries in rcol1 with col1

            IDictionaryEnumerator denum = col1.GetEnumerator();
            denum.Reset();
            foreach (DictionaryEntry entryr in rcol1)
            {
                denum.MoveNext();
                DictionaryEntry entry = denum.Entry;
                Assert.AreEqual(entryr.Key, entry.Key);
                Assert.AreEqual(entryr.Value, entry.Value);
            }

            #endregion

            #region Compare entries in rcol2 with col2

            denum = col2.GetEnumerator();
            denum.Reset();
            foreach (DictionaryEntry entryr in rcol2)
            {
                denum.MoveNext();
                DictionaryEntry entry = denum.Entry;
                Assert.AreEqual(entryr.Key, entry.Key);
                Assert.AreEqual(entryr.Value, entry.Value);
            }

            #endregion

            Assert.AreEqual(null, pofreader.ReadDictionary(0, null));

            IDictionary rcol3 = pofreader.ReadDictionary(0, null);

            #region Compare entries in rcol3 with col1

            denum = col1.GetEnumerator();
            denum.Reset();
            foreach (DictionaryEntry entryr in rcol3)
            {
                denum.MoveNext();
                DictionaryEntry entry = denum.Entry;
                Assert.AreEqual(entryr.Key, entry.Key);
                Assert.AreEqual(entryr.Value, entry.Value);
            }

            #endregion

        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestPofStreamWriteDictionaryException()
        {
            initPOFWriter();
            stream.Close();
            pofwriter.WriteDictionary(0, new Hashtable(0));
        }

        [Test]
        public void TestPofStreamWriteWriteDictionaryEx1()
        {
            initPOFWriter();
            Hashtable col1 = new Hashtable();
            Hashtable col2 = new Hashtable();
            col1.Add((Int16)0, "A"); col1.Add((Int16)1, "Z"); col1.Add((Int16)2, "7");
            col2.Add(5, 32); col2.Add(10, Int32.MinValue); col2.Add(15, Int32.MaxValue);

            pofwriter.WriteDictionary(0, col1, typeof(Int16));
            pofwriter.WriteDictionary(0, col2, typeof(int));
            pofwriter.WriteDictionary(0, null, typeof(int));

            initPOFReader();
            IDictionary rcol1 = pofreader.ReadDictionary(0, new Hashtable(3));
            IDictionary rcol2 = pofreader.ReadDictionary(0, null);

            #region Compare entries in rcol1 with col1

            IDictionaryEnumerator denum = col1.GetEnumerator();
            denum.Reset();
            foreach (DictionaryEntry entryr in rcol1)
            {
                denum.MoveNext();
                DictionaryEntry entry = denum.Entry;
                Assert.AreEqual(entryr.Key, entry.Key);
                Assert.AreEqual(entryr.Value, entry.Value);
            }

            #endregion

            #region Compare entries in rcol2 with col2

            denum = col2.GetEnumerator();
            denum.Reset();
            foreach (DictionaryEntry entryr in rcol2)
            {
                denum.MoveNext();
                DictionaryEntry entry = denum.Entry;
                Assert.AreEqual(entryr.Key, entry.Key);
                Assert.AreEqual(entryr.Value, entry.Value);
            }

            #endregion

        }

        [Test]
        [ExpectedException(typeof(IOException))]
        public void TestPofStreamWriteWriteUniformDictionary()
        {
            initPOFWriter();
            var col1 = new Hashtable();
            var col2 = new Hashtable();
            col1.Add((Int16)0, "A"); col1.Add((Int16)1, "Z"); col1.Add((Int16)2, "7");
            col2.Add(5, 32); col2.Add(10, Int32.MinValue); col2.Add(15, Int32.MaxValue);

            pofwriter.WriteDictionary(0, col1, typeof(Int16), typeof(String));
            pofwriter.WriteDictionary(0, col2, typeof(int), typeof(Int32));
            pofwriter.WriteDate(0, new DateTime(2006, 8, 8));

            initPOFReader();
            IDictionary rcol1 = pofreader.ReadDictionary(0, new Hashtable(3));
            IDictionary rcol2 = pofreader.ReadDictionary(0, null);

            #region Compare entries in rcol1 with col1

            IDictionaryEnumerator denum = col1.GetEnumerator();
            denum.Reset();
            foreach (DictionaryEntry entryr in rcol1)
            {
                denum.MoveNext();
                DictionaryEntry entry = denum.Entry;
                Assert.AreEqual(entryr.Key, entry.Key);
                Assert.AreEqual(entryr.Value, entry.Value);
            }

            #endregion

            #region Compare entries in rcol2 with col2

            denum = col2.GetEnumerator();
            denum.Reset();
            foreach (DictionaryEntry entryr in rcol2)
            {
                denum.MoveNext();
                DictionaryEntry entry = denum.Entry;
                Assert.AreEqual(entryr.Key, entry.Key);
                Assert.AreEqual(entryr.Value, entry.Value);
            }

            #endregion

            pofreader.ReadDictionary(0, null);
        }

        // test case for COH-3370
        [Test]
        [ExpectedException(typeof(IOException))]
        public void testPofWriterWriteUniformDictionaryWithNulls()
        {
            var col1 = new Hashtable();
            var col2 = new Hashtable();
            col1.Add((Int16)0, "A");
            col1.Add((Int16)1, "Z");
            col1.Add((Int16)2, "7");
            col1.Add((Int16)3, null);

            col2.Add(5, 32);
            col2.Add(10, Int32.MinValue);
            col2.Add(15, Int32.MaxValue);
            col2.Add(20, null);

            initPOFWriter();
            pofwriter.WriteDictionary(0, col1, typeof(Int16), typeof(String));
            pofwriter.WriteDictionary(0, col2, typeof(Int32), typeof(Int32));
            pofwriter.WriteDate(0, new DateTime(2006, 8, 8));

            initPOFReader();
            IDictionary rcol1 = pofreader.ReadDictionary(0, new Hashtable(3));
            IDictionary rcol2 = pofreader.ReadDictionary(0, new Hashtable(3));

            // compare col1 with result 1
            IDictionaryEnumerator denum = col1.GetEnumerator();
            denum.Reset();
            foreach (DictionaryEntry entryr in rcol1)
            {
                denum.MoveNext();
                DictionaryEntry entry = denum.Entry;
                Assert.AreEqual(entryr.Key, entry.Key);
                Assert.AreEqual(entryr.Value, entry.Value);
            }

            // compare col2 with result 2
            denum = col2.GetEnumerator();
            denum.Reset();
            foreach (DictionaryEntry entryr in rcol2)
            {
                denum.MoveNext();
                DictionaryEntry entry = denum.Entry;
                Assert.AreEqual(entryr.Key, entry.Key);
                Assert.AreEqual(entryr.Value, entry.Value);
            }

            pofreader.ReadDictionary(0, null);
        }

        [Test]
        public void TestPofStreamWriteWriteDictionaryEx3()
        {
            initPOFWriter();
            var col1 = new Hashtable();
            var col2 = new Hashtable();
            col1.Add(0, "A");
            col1.Add(1, "G");
            col1.Add(2, "7");


            col2.Add(5, new DateTime(2006, 8, 8));
            col2.Add(10, col1);
            col2.Add(15, Double.PositiveInfinity);
            col2.Add(20, Double.NegativeInfinity);
            col2.Add(25, Double.NaN);
            col2.Add(30, Single.PositiveInfinity);
            col2.Add(35, Single.NegativeInfinity);
            col2.Add(40, Single.NaN);

            pofwriter.WriteDictionary(0, col2);

            initPOFReader();
            IDictionary rcol2 = pofreader.ReadDictionary(0, new Hashtable(3));

            #region Compare entries in rcol2 with col2

            IDictionaryEnumerator denum_col2 = col2.GetEnumerator();
            denum_col2.Reset();
            foreach (DictionaryEntry entry_rcol2 in rcol2)
            {
                denum_col2.MoveNext();
                DictionaryEntry entry_col2 = denum_col2.Entry;
                Assert.AreEqual(entry_rcol2.Key, entry_col2.Key);
                if (entry_rcol2.Value is Hashtable)
                {
                    IDictionaryEnumerator denum_rcol1 = ((Hashtable)entry_rcol2.Value).GetEnumerator();
                    denum_rcol1.Reset();
                    foreach (DictionaryEntry entry_col1 in col1)
                    {
                        denum_rcol1.MoveNext();
                        DictionaryEntry entry_rcol1 = denum_rcol1.Entry;
                        Assert.AreEqual(entry_rcol1.Key, entry_col1.Key);
                        Assert.AreEqual(entry_rcol1.Value, entry_col1.Value);
                    }


                }
                else
                {
                    Assert.AreEqual(entry_rcol2.Value, entry_col2.Value);
                }
            }

            #endregion

        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestPofStreamWriteDictionaryEx3Exception()
        {
            initPOFWriter();
            stream.Close();
            pofwriter.WriteDictionary(0, new Hashtable(0), typeof(int));
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestPofStreamWriteDictionaryEx4Exception()
        {
            initPOFWriter();
            stream.Close();
            pofwriter.WriteDictionary(0, new Hashtable(0), typeof(int), typeof(int));
        }

        [Test]
        public void TestPofStreamWriteWriteDictionaryEx4()
        {
            var col1 = new Hashtable();
            col1.Add(0, "A");
            col1.Add(1, "G");
            col1.Add(2, "7");

            initPOFWriter();
            pofwriter.WriteDictionary(0, col1, typeof(Int32));
            pofwriter.WriteDictionary(0, col1, typeof(Int32), typeof(String));
            pofwriter.WriteDictionary(0, new Hashtable(0), typeof(String));
            pofwriter.WriteDictionary(0, new Hashtable(0), typeof(String), typeof(String));
            pofwriter.WriteDictionary(0, null, typeof(String), typeof(String));

            initPOFReader();
            Object rcol = pofreader.ReadObject(0);
            Assert.IsTrue(rcol is Hashtable);
            Hashtable rcol1 = (Hashtable)rcol;
            Assert.AreEqual(rcol1.Count, col1.Count);
            Assert.AreEqual(rcol1[0], col1[0]);
            Assert.AreEqual(rcol1[1], col1[1]);
            Assert.AreEqual(rcol1[2], col1[2]);

            rcol = pofreader.ReadObject(0);
            Assert.IsTrue(rcol is Hashtable);
            rcol1 = (Hashtable)rcol;
            Assert.AreEqual(rcol1.Count, col1.Count);
            Assert.AreEqual(rcol1[0], col1[0]);
            Assert.AreEqual(rcol1[1], col1[1]);
            Assert.AreEqual(rcol1[2], col1[2]);

        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestReadReminder()
        {
            initPOFWriter();
            initPOFReader();
            pofwriter.WriteRemainder(new Binary(new byte[] { 1, 2, 3 }));
            pofreader.ReadRemainder();
        }

        [Test]
        public void TestWriteGenericList()
        {
            var ctx = new SimplePofContext();
            ctx.RegisterUserType(101, typeof(PortablePersonLite), new PortableObjectSerializer(101));
            ctx.RegisterUserType(102, typeof(PortablePerson), new PortableObjectSerializer(102));
            ctx.RegisterUserType(103, typeof(EvolvablePortablePerson), new PortableObjectSerializer(103));

            //initPOFWriter();
            stream = new MemoryStream();
            writer = new DataWriter(stream);
            pofwriter = new PofStreamWriter(writer, ctx);

            ICollection<string> list1 = new List<string>();
            ICollection<object> list2 = new List<object>();
            list1.Add("A"); list1.Add(null); list1.Add("7");
            list2.Add("A"); list2.Add(null); list2.Add(7);

            ICollection<IPortableObject> persons = new List<IPortableObject>();
            ICollection<PortablePerson> persons2 = new List<PortablePerson>();
            var ivan = new PortablePerson("Ivan", new DateTime(1978, 4, 25));
            ivan.Children = null;
            var goran = new PortablePerson("Goran", new DateTime(1982, 3, 3));
            goran.Children = null;
            var aleks = new EvolvablePortablePerson("Aleks", new DateTime(1974, 8, 24));
            aleks.Children = new EvolvablePortablePerson[1];
            aleks.Children[0] = new EvolvablePortablePerson("Ana Maria", new DateTime(2004, 8, 14));
            aleks.DataVersion = 2;

            persons.Add(ivan);
            persons.Add(aleks);
            persons.Add(goran);
            persons.Add(null);
            persons2.Add(ivan);
            persons2.Add(null);
            persons2.Add(goran);


            pofwriter.WriteCollection(0, list1);
            pofwriter.WriteCollection(0, list2);
            pofwriter.WriteCollection(0, persons);
            pofwriter.WriteCollection(0, persons);
            pofwriter.WriteCollection(0, (ICollection)persons2, typeof(PortablePerson));

            //initPOFReader();
            stream.Position = 0;
            reader = new DataReader(stream);
            pofreader = new PofStreamReader(reader, ctx);

            ICollection<string> result1 = new List<string>();
            pofreader.ReadCollection(0, result1);
            Assert.AreEqual(3, result1.Count);
            for (int i = 0; i < result1.Count; i++)
            {
                Assert.AreEqual(((List<string>)list1)[i], ((List<string>)result1)[i]);
            }

            ICollection<object> result2 = new List<object>();
            pofreader.ReadCollection(0, result2);
            Assert.AreEqual(3, result2.Count);
            for (int i = 0; i < result2.Count; i++)
            {
                Assert.AreEqual(((List<object>)list2)[i], ((List<object>)result2)[i]);
            }

            ICollection<IPortableObject> result3 = new List<IPortableObject>();
            pofreader.ReadCollection(0, result3);
            Assert.AreEqual(4, result3.Count);
            Assert.IsFalse(((List<IPortableObject>)result3)[0] is EvolvablePortablePerson);
            Assert.IsTrue(((List<IPortableObject>)result3)[1] is EvolvablePortablePerson);
            Assert.IsFalse(((List<IPortableObject>)result3)[2] is EvolvablePortablePerson);
            Assert.AreEqual(((List<IPortableObject>)result3)[3], null);
            EvolvablePortablePerson epp = (EvolvablePortablePerson)((List<IPortableObject>)result3)[1];
            Assert.AreEqual(aleks.Name, epp.Name);
            Assert.AreEqual(aleks.Children[0].Name, epp.Children[0].Name);

            PortablePerson pp = (PortablePerson)((List<IPortableObject>)result3)[0];
            Assert.AreEqual(ivan.Name, pp.Name);
            Assert.IsNull(pp.Children);

            List<IPortableObject> result4 = (List<IPortableObject>)pofreader.ReadCollection<IPortableObject>(0, null);
            Assert.AreEqual(4, result4.Count);
            Assert.IsFalse(result4[0] is EvolvablePortablePerson);
            Assert.IsTrue(result4[1] is EvolvablePortablePerson);
            Assert.IsFalse(result4[2] is EvolvablePortablePerson);
            Assert.AreEqual(result4[3], null);
            epp = (EvolvablePortablePerson)result4[1];
            Assert.AreEqual(aleks.Name, epp.Name);
            Assert.AreEqual(aleks.Children[0].Name, epp.Children[0].Name);

            pp = (PortablePerson)result4[0];
            Assert.AreEqual(ivan.Name, pp.Name);
            Assert.IsNull(pp.Children);

            List<PortablePerson> result5 = (List<PortablePerson>)pofreader.ReadCollection<PortablePerson>(0, null);
            for (int i = 0; i < persons2.Count; i++)
            {
                Assert.AreEqual(((List<PortablePerson>)persons2)[i], result5[i]);
            }
        }

        [Test]
        public void TestWriteGenericDictionary()
        {
            var ctx = new SimplePofContext();
            ctx.RegisterUserType(101, typeof(PortablePersonLite), new PortableObjectSerializer(101));
            ctx.RegisterUserType(102, typeof(PortablePerson), new PortableObjectSerializer(102));
            ctx.RegisterUserType(103, typeof(EvolvablePortablePerson), new PortableObjectSerializer(103));

            //initPOFWriter();
            stream = new MemoryStream();
            writer = new DataWriter(stream);
            pofwriter = new PofStreamWriter(writer, ctx);


            IDictionary<string, double> dict = new Dictionary<string, double>();
            dict.Add("A", 11.11); dict.Add("Z", 88.88); dict.Add("7", 100.1);
            IDictionary<string, string> dict2 = new Dictionary<string, string>();
            dict2.Add("ABC", "value"); dict2.Add("N", null);

            IDictionary<string, IPortableObject> persons = new Dictionary<string, IPortableObject>();
            var ivan = new PortablePerson("Ivan", new DateTime(1978, 4, 25));
            ivan.Children = null;
            var goran = new PortablePerson("Goran", new DateTime(1982, 3, 3));
            goran.Children = null;
            var aleks = new EvolvablePortablePerson("Aleks", new DateTime(1974, 8, 24));
            aleks.Children = new EvolvablePortablePerson[1];
            aleks.Children[0] = new EvolvablePortablePerson("Ana Maria", new DateTime(2004, 8, 14));
            aleks.DataVersion = 2;

            persons.Add("key1", ivan);
            persons.Add("key2", aleks);
            persons.Add("key3", goran);

            pofwriter.WriteDictionary(0, dict);
            pofwriter.WriteDictionary(0, dict2);
            pofwriter.WriteDictionary(0, persons);
            pofwriter.WriteDictionary(0, persons);

            //initPOFReader();
            stream.Position = 0;
            reader = new DataReader(stream);
            pofreader = new PofStreamReader(reader, ctx);

            IDictionary<string, double> result = new Dictionary<string, double>();
            pofreader.ReadDictionary(0, result);
            Assert.AreEqual(3, result.Count);
            foreach (string key in dict.Keys)
            {
                Assert.AreEqual(dict[key], result[key]);
            }

            IDictionary<string, string> result2 = new Dictionary<string, string>();
            pofreader.ReadDictionary(0, result2);
            Assert.AreEqual(2, result2.Count);
            foreach (string key in dict.Keys)
            {
                Assert.AreEqual(dict[key], result[key]);
            }

            IDictionary<string, IPortableObject> result3 = new Dictionary<string, IPortableObject>();
            pofreader.ReadDictionary(0, result3);
            Assert.AreEqual(3, result3.Count);
            Assert.IsFalse(result3["key1"] is EvolvablePortablePerson);
            Assert.IsTrue(result3["key2"] is EvolvablePortablePerson);
            Assert.IsFalse(result3["key3"] is EvolvablePortablePerson);
            EvolvablePortablePerson epp = (EvolvablePortablePerson)result3["key2"];
            Assert.AreEqual(aleks.Name, epp.Name);
            Assert.AreEqual(aleks.Children[0].Name, epp.Children[0].Name);

            var pp = (PortablePerson)result3["key3"];
            Assert.AreEqual(goran.Name, pp.Name);
            Assert.IsNull(pp.Children);

            IDictionary<string, IPortableObject> result4 = pofreader.ReadDictionary<string, IPortableObject>(0, null);
            Assert.AreEqual(3, result4.Count);
            Assert.IsFalse(result4["key1"] is EvolvablePortablePerson);
            Assert.IsTrue(result4["key2"] is EvolvablePortablePerson);
            Assert.IsFalse(result4["key3"] is EvolvablePortablePerson);
            epp = (EvolvablePortablePerson)result4["key2"];
            Assert.AreEqual(aleks.Name, epp.Name);
            Assert.AreEqual(aleks.Children[0].Name, epp.Children[0].Name);

            pp = (PortablePerson)result4["key3"];
            Assert.AreEqual(goran.Name, pp.Name);
            Assert.IsNull(pp.Children);
        }

    }
}