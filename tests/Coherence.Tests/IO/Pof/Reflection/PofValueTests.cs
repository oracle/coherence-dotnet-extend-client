/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Tangosol.Util;

namespace Tangosol.IO.Pof.Reflection
{
    [TestFixture]
    public class PofValueTests
    {
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

        #region Tests

        [Test]
        public void TestPofValueInitialization()
        {
            PortablePerson person = CreatePerson();

            // perform the test twice, once with references disable, once with them enabled
            for (bool isRefEnabled = false; ; )
            {
                Binary    binPerson = Serialize(person, isRefEnabled);
                IPofValue pv        = PofValueParser.Parse(binPerson, GetPofContext(isRefEnabled));
                Assert.AreEqual(101, pv.TypeId);
                Assert.AreEqual(person, pv.GetValue());

                if (isRefEnabled)
                {
                    break;
                }
                isRefEnabled = true;
            }            
        }

        [Test]
        public void TestPofValueAccessor()
        {
            // perform the test twice, once with references disable, once with them enabled
            for (bool isRefEnabled = false; ; )
            {
                PortablePerson person    = isRefEnabled ? CreatePersonNoChildren() : CreatePerson();
                Binary         binPerson = Serialize(person, isRefEnabled);

                IPofValue pv = PofValueParser.Parse(binPerson, GetPofContext(isRefEnabled));
                Assert.AreEqual(person.Address, pv.GetChild(1).GetValue());
                Assert.AreEqual(person.Name, pv.GetChild(0).GetValue());
                Assert.AreEqual(person.DOB, pv.GetChild(2).GetValue());

                // test NilPofValue
                IPofValue nv = pv.GetChild(100);
                Assert.IsTrue(nv is PofSparseArray.NilPofValue);
                Assert.IsNull(nv.GetValue());

                // test PofNavigationException
                try
                {
                    pv.GetChild(0).GetChild(0);
                    Assert.Fail("Should've thrown PofNavigationException");
                }
                catch (PofNavigationException)
                {
                }

                if (isRefEnabled)
                {
                    break;
                }
                isRefEnabled = true;
            }
        }

        [Test]
        public void TestNestedPofValueAccessor()
        {
            PortablePerson person = CreatePerson();

            // perform the test twice, once with references disable, once with them enabled
            for (bool isRefEnabled = false; ; )
            {
                Binary binPerson = Serialize(person, isRefEnabled);

                IPofValue pv = PofValueParser.Parse(binPerson, GetPofContext(isRefEnabled));
                Assert.AreEqual(person.Address.Street, pv.GetChild(1).GetChild(0).GetValue());
                Assert.AreEqual(person.Address.City, pv.GetChild(1).GetChild(1).GetValue());
                Assert.AreEqual(person.Address.State, pv.GetChild(1).GetChild(2).GetValue());
                Assert.AreEqual(person.Address.ZIP, pv.GetChild(1).GetChild(3).GetValue());

                if (isRefEnabled)
                {
                    // test the case where we try to access an object that contains a
                    // uniform collection of user defined objects when object reference is enabled
                    try
                    {
                        pv.GetChild(4);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    {
                    }
                    break;
                }
                isRefEnabled = true;
            }
        }

        [Test]
        public void TestPofValueMutator()
        {
            PortablePerson person    = CreatePerson();
            Binary         binPerson = Serialize(person);

            IPofValue pv = PofValueParser.Parse(binPerson, GetPofContext());

            pv.GetChild(0).SetValue("Seovic Aleksandar");
            Assert.AreEqual(pv.GetChild(0).GetValue(), "Seovic Aleksandar");

            pv.GetChild(0).SetValue("Marija Seovic");
            pv.GetChild(1).GetChild(0).SetValue("456 Main St");
            pv.GetChild(1).GetChild(1).SetValue("Lutz");
            pv.GetChild(1).GetChild(3).SetValue("33549");
            pv.GetChild(2).SetValue(new DateTime(1978, 2, 20));
            pv.GetChild(3).SetValue(new PortablePerson("Aleksandar Seovic", new DateTime(1974, 8, 24)));
            pv.GetChild(4).SetValue(person.Children);
            binPerson = pv.ApplyChanges();

            PortablePerson p2 = (PortablePerson) Deserialize(binPerson);
            Assert.AreEqual("Marija Seovic", p2.Name);
            Assert.AreEqual("456 Main St", p2.Address.Street);
            Assert.AreEqual("Lutz", p2.Address.City);
            Assert.AreEqual("33549", p2.Address.ZIP);
            Assert.AreEqual(new DateTime(1978, 2, 20), p2.DOB);
            Assert.AreEqual("Aleksandar Seovic", p2.Spouse.Name);
            Assert.AreEqual(person.Children, p2.Children);

            pv = PofValueParser.Parse(binPerson, GetPofContext());
            pv.GetChild(0).SetValue("Ana Maria Seovic");
            pv.GetChild(2).SetValue(new DateTime(2004, 8, 14));
            pv.GetChild(3).SetValue(null);
            pv.GetChild(4).SetValue(null);
            binPerson = pv.ApplyChanges();

            PortablePerson p3 = (PortablePerson) Deserialize(binPerson);
            Assert.AreEqual("Ana Maria Seovic", p3.Name);
            Assert.AreEqual(p2.Address, p3.Address);
            Assert.AreEqual(new DateTime(2004, 8, 14), p3.DOB);
            Assert.IsNull(p3.Spouse);
            Assert.IsNull(p3.Children);
        }

        [Test]
        public void TestPofArray()
        {
            // perform the test twice, once with references disable, once with them enabled
            for (bool isRefEnabled = false; ; )
            {
                TestValue tv  = CreateTestValue(isRefEnabled);
                Binary    bin = Serialize(tv);

                IPofValue root = PofValueParser.Parse(bin, GetPofContext(isRefEnabled));

                IPofValue pv = root.GetChild(0);
                Assert.AreEqual(((PofArray) pv).Length, 3);
                Assert.AreEqual(pv.GetChild(0).GetValue(), 1);
                Assert.AreEqual(pv.GetChild(1).GetValue(), "two");
                Assert.AreEqual(pv.GetChild(2).GetValue(),
                        isRefEnabled ? CreatePersonNoChildren() : CreatePerson());

                try
                {
                    pv.GetChild(100);
                    Assert.Fail("Should've thrown IndexOutOfRangeException.");
                }
                catch (IndexOutOfRangeException)
                {
                }

                if (isRefEnabled)
                {
                    break;
                }
                
                pv.GetChild(1).SetValue("dva");
                pv.GetChild(2).GetChild(0).SetValue("Novak");
                Binary binModified = root.ApplyChanges();

                TestValue tvModified = (TestValue) Deserialize(binModified);
                Assert.AreEqual(tvModified.m_oArray[1], "dva");
                Assert.AreEqual(((PortablePerson) tvModified.m_oArray[2]).Name,
                                "Novak");
                isRefEnabled = true;
            }
        }

        [Test]
        public void TestPofUniformArray()
        {
            // perform the test twice, once with references disable, once with them enabled
            for (bool isRefEnabled = false; ; )
            {
                TestValue tv  = CreateTestValue(isRefEnabled);
                Binary    bin = Serialize(tv, isRefEnabled);

                IPofValue root = PofValueParser.Parse(bin, GetPofContext(isRefEnabled));

                IPofValue pv = root.GetChild(1);
                Assert.AreEqual(((PofArray) pv).Length, 4);
                Assert.AreEqual(pv.GetChild(0).GetValue(), "one");
                Assert.AreEqual(pv.GetChild(1).GetValue(), "two");
                Assert.AreEqual(pv.GetChild(2).GetValue(), "three");
                Assert.AreEqual(pv.GetChild(3).GetValue(), "four");

                try
                {
                    pv.GetChild(100);
                    Assert.Fail("Should've thrown IndexOutOfRangeException.");
                }
                catch (IndexOutOfRangeException)
                {
                }

                if (isRefEnabled)
                {
                    break;
                }

                pv.GetChild(0).SetValue("jedan");
                pv.GetChild(3).SetValue("cetiri");
                Binary binModified = root.ApplyChanges();

                TestValue tvModified = (TestValue) Deserialize(binModified);
                Assert.AreEqual(tvModified.m_sArray[0], "jedan");
                Assert.AreEqual(tvModified.m_sArray[3], "cetiri");
                isRefEnabled = true;
            }
        }

        [Test]
        public void TestPofCollection()
        {
            // perform the test twice, once with references disable, once with them enabled
            for (bool isRefEnabled = false; ; )
            {
                TestValue tv  = CreateTestValue(isRefEnabled);
                Binary    bin = Serialize(tv, isRefEnabled);

                IPofValue root = PofValueParser.Parse(bin, GetPofContext());

                IPofValue pv = root.GetChild(2);
                Assert.AreEqual(((PofCollection) pv).Length, 3);
                Assert.AreEqual(pv.GetChild(0).GetValue(), 1);
                Assert.AreEqual(pv.GetChild(1).GetValue(), "two");
                if (isRefEnabled)
                {
                    IPofValue pv2 = root.GetChild(0).GetChild(2);
                    pv2.GetValue();
                }
                Assert.AreEqual(pv.GetChild(2).GetValue(), isRefEnabled ? CreatePersonNoChildren() : CreatePerson());

                try
                {
                    pv.GetChild(100);
                    Assert.Fail("Should've thrown IndexOutOfRangeException.");
                }
                catch (IndexOutOfRangeException)
                {
                }

                if (isRefEnabled)
                {
                    break;
                }

                pv.GetChild(1).SetValue("dva");
                pv.GetChild(2).GetChild(0).SetValue("Novak");
                Binary binModified = root.ApplyChanges();

                TestValue tvModified = (TestValue) Deserialize(binModified);
                Assert.AreEqual(tvModified.m_col[1], "dva");
                Assert.AreEqual(((PortablePerson) tvModified.m_col[2]).Name,
                                "Novak");
                isRefEnabled = true;
            }
        }

        [Test]
        public void TestPofUniformCollection()
        {
            // perform the test twice, once with references disable, once with them enabled
            for (bool isRefEnabled = false; ; )
            {
                TestValue tv = CreateTestValue(isRefEnabled);
                Binary bin = Serialize(tv, isRefEnabled);

                IPofValue root = PofValueParser.Parse(bin, GetPofContext(isRefEnabled));

                IPofValue pv = root.GetChild(3);
                Assert.AreEqual(((PofUniformCollection) pv).Length, 4);
                Assert.AreEqual(pv.GetChild(0).GetValue(), "one");
                Assert.AreEqual(pv.GetChild(1).GetValue(), "two");
                Assert.AreEqual(pv.GetChild(2).GetValue(), "three");
                Assert.AreEqual(pv.GetChild(3).GetValue(), "four");

                try
                {
                    pv.GetChild(100);
                    Assert.Fail("Should've thrown IndexOutOfRangeException.");
                }
                catch (IndexOutOfRangeException)
                {
                }

                if (isRefEnabled)
                {
                    break;
                }
                pv.GetChild(0).SetValue("jedan");
                pv.GetChild(3).SetValue("cetiri");
                Binary binModified = root.ApplyChanges();

                TestValue tvModified = (TestValue) Deserialize(binModified);
                Assert.AreEqual(tvModified.m_colUniform[0], "jedan");
                Assert.AreEqual(tvModified.m_colUniform[3], "cetiri");
                isRefEnabled = true;
            }
        }

        [Test]
        public void TestPofSparseArray()
        {
            // perform the test twice, once with references disable, once with them enabled
            for (bool isRefEnabled = false; ; )
            {
                TestValue tv   = CreateTestValue(isRefEnabled);
                Binary    bin  = Serialize(tv, isRefEnabled);
                IPofValue root = PofValueParser.Parse(bin, GetPofContext(isRefEnabled));
                IPofValue pv   = root.GetChild(4);

                Assert.IsTrue(pv is PofSparseArray);
                Assert.AreEqual(pv.GetChild(4).GetValue(), 4);
                Assert.AreEqual(pv.GetChild(2).GetValue(), "two");

                // Test the case where we try to read a reference id before the object is read.
                // We should get an
                // IOException: missing identity: 2
                // Work around by reading the person object in root.getChild(0) where it first appears
                // so that root.getChild(4).getValue(), which references the person object will have it.
                if (isRefEnabled)
                {
                    try
                    {
                        pv.GetChild(5).GetValue();
                        Assert.Fail("Should've thrown Exception.");
                    }
                    catch (IOException)
                    {
                    }
                    root.GetChild(0).GetChild(2).GetValue();
                } 
               
                Assert.AreEqual(pv.GetChild(5).GetValue(), 
                        isRefEnabled ? CreatePersonNoChildren() : CreatePerson());

                if (isRefEnabled)
                {
                    break;
                }

                pv.GetChild(1).SetValue(1);
                pv.GetChild(2).SetValue("dva");
                pv.GetChild(3).SetValue("tri");
                pv.GetChild(5).GetChild(0).SetValue("Novak");
                Binary binModified = root.ApplyChanges();

                TestValue tvModified = (TestValue) Deserialize(binModified);
                Assert.AreEqual(tvModified.m_sparseArray[1], 1);
                Assert.AreEqual(tvModified.m_sparseArray[2], "dva");
                Assert.AreEqual(tvModified.m_sparseArray[3], "tri");
                Assert.AreEqual(((PortablePerson) tvModified.m_sparseArray[5]).Name,
                                "Novak");
                isRefEnabled = true;
            }
        }

        [Test]
        public void TestPofUniformSparseArray()
        {
            // perform the test twice, once with references disable, once with them enabled
            for (bool isRefEnabled = false; ; )
            {
                TestValue tv   = CreateTestValue(isRefEnabled);
                Binary    bin  = Serialize(tv, isRefEnabled);
                IPofValue root = PofValueParser.Parse(bin, GetPofContext(isRefEnabled));

                IPofValue pv = root.GetChild(5);
                Assert.IsTrue(pv is PofUniformSparseArray);
                Assert.AreEqual(pv.GetChild(2).GetValue(), "two");
                Assert.AreEqual(pv.GetChild(4).GetValue(), "four");

                if (isRefEnabled)
                {
                    break;
                }

                pv.GetChild(1).SetValue("jedan");
                pv.GetChild(3).SetValue("tri");
                pv.GetChild(4).SetValue("cetiri");
                pv.GetChild(5).SetValue("pet");
                Binary binModified = root.ApplyChanges();

                TestValue tvModified = (TestValue) Deserialize(binModified);
                ILongArray arr = tvModified.m_uniformSparseArray;
                Assert.AreEqual(arr[1], "jedan");
                Assert.AreEqual(arr[2], "two");
                Assert.AreEqual(arr[3], "tri");
                Assert.AreEqual(arr[4], "cetiri");
                Assert.AreEqual(arr[5], "pet");
                isRefEnabled = true;
            }
        }

        [Test]
        public void TestGetBoolean()
        {
            Boolean bt = true;
            Boolean bf = false;
            Binary binTrue = Serialize(bt);
            Binary binFalse = Serialize(bf);

            IPofValue rootTrue = PofValueParser.Parse(binTrue, GetPofContext());
            IPofValue rootFalse = PofValueParser.Parse(binFalse, GetPofContext());
            Boolean pvTrue = rootTrue.GetBoolean();
            Boolean pvFalse = rootFalse.GetBoolean();
            
            Assert.IsTrue(pvTrue);
            Assert.IsFalse(pvFalse);

            pvTrue = (Boolean) rootTrue.GetValue();
            pvFalse = (Boolean) rootFalse.GetValue();
            Assert.IsTrue(pvTrue);
            Assert.IsFalse(pvFalse);
        }

        [Test]
        public void TestGetChar()
        {
            Char c = '\x0041';
            Binary bin = Serialize(c);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Char pv = root.GetChar();

            Assert.IsInstanceOf(c.GetType(), pv);
            Assert.AreEqual(c, pv);
        }

        [Test]
        public void TestPofInt16()
        {
            Int16 s = 0x1234;
            Binary bin = Serialize(s);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Int16 pv = root.GetInt16();

            Assert.IsInstanceOf(s.GetType(), pv);
            Assert.AreEqual(s, pv);
        }

        [Test]
        public void TestGetInt32()
        {
            Int32 i = 0x12345678;
            Binary bin = Serialize(i);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Int32 pv = root.GetInt32();

            Assert.IsInstanceOf(i.GetType(), pv);
            Assert.AreEqual(i, pv);
        }

        [Test]
        public void TestGetInt64()
        {
            Int64 l = 0x1234567812345678;
            Binary bin = Serialize(l);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Int64 pv = root.GetInt64();

            Assert.IsInstanceOf(l.GetType(), pv);
            Assert.AreEqual(l, pv);
        }

        [Test]
        public void TestGetSingle()
        {
            Single f = 0.12345678f;
            f = 0.1234567890123456789f;
            Binary bin = Serialize(f);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Single pv = root.GetSingle();

            Assert.IsInstanceOf(f.GetType(), pv);
            Assert.AreEqual(f, pv);
        }

        [Test]
        public void TestGetDouble()
        {
            Double d = 0.1234567890123456789d;
            Binary bin = Serialize(d);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Double pv = root.GetDouble();

            Assert.IsInstanceOf(d.GetType(), pv);
            Assert.AreEqual(d, pv);
        }

        [Test]
        public void TestGetDecimal()
        {
            Decimal d = Decimal.Parse("28162514264337593543950335.1");
            Binary bin = Serialize(d);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Decimal pv = root.GetDecimal();

            Assert.IsInstanceOf(d.GetType(), pv);
            Assert.AreEqual(d, pv);
        }

        [Test]
        public void TestGetString()
        {
            string s = "28162514264337593543950335.1";
            Binary bin = Serialize(s);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            string pv = root.GetString();

            Assert.IsInstanceOf(s.GetType(), pv);
            Assert.AreEqual(s, pv);
        }

        [Test]
        public void TestGetDateTime()
        {
            DateTime dt = new DateTime((DateTime.Now.Ticks / 10000) * 10000);
            Binary bin = Serialize(dt);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            DateTime pv = root.GetDateTime();

            Assert.IsInstanceOf(dt.GetType(), pv);
            Assert.AreEqual(dt, pv);
        }

        [Test]
        public void TestGetDate()
        {
            DateTime dt = DateTime.Now;
            Binary bin = Serialize(dt);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            DateTime pv = root.GetDate();

            Assert.IsInstanceOf(dt.GetType(), pv);
            Assert.AreEqual(dt.Date, pv.Date);
            Assert.AreEqual(pv.Hour, 0);
            Assert.AreEqual(pv.Minute, 0);
            Assert.AreEqual(pv.Second, 0);
            Assert.AreEqual(pv.Millisecond, 0);
        }

        [Test]
        public void TestGetDayTimeInterval()
        {
            TimeSpan ts = new TimeSpan(12, 5, 10, 5);
            Binary bin = Serialize(ts);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            TimeSpan pv = root.GetDayTimeInterval();

            Assert.IsInstanceOf(ts.GetType(), pv);
            Assert.AreEqual(ts, pv);
        }

        [Test]
        public void TestGetBooleanArray()
        {
            Boolean[] tv = new Boolean[] { true, false, true, false };
            Binary bin = Serialize(tv);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Boolean[] pv = root.GetBooleanArray();

            Assert.AreEqual(pv.Length, 4);
            Assert.IsTrue(pv[0]);
            Assert.IsFalse(pv[1]);
            Assert.IsTrue(pv[2]);
            Assert.IsFalse(pv[3]);

            try
            {
                pv[5] = pv[5];
                Assert.Fail("Should've thrown IndexOutOfRangeException.");
            }
            catch (IndexOutOfRangeException) { }
        }

        [Test]
        public void TestGetByteArray()
        {
            Byte[] tv = new byte[] { 1, 2, 3, 4 };
            Binary bin = Serialize(tv);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Byte[] pv = root.GetByteArray();

            Assert.AreEqual(tv, pv);
            try
            {
                pv[5] = pv[5];
                Assert.Fail("Should've thrown IndexOutOfRangeException.");
            }
            catch (IndexOutOfRangeException) { }
        }

        [Test]
        public void TestGetCharArray()
        {
            Char[] tv = new char[] { '\x0041', '\x0042', '\x0043', '\x0044' };
            Binary bin = Serialize(tv);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Char[] pv = root.GetCharArray();

            Assert.AreEqual(tv, pv);
            try
            {
                pv[5] = pv[5];
                Assert.Fail("Should've thrown IndexOutOfRangeException.");
            }
            catch (IndexOutOfRangeException) { }
        }

        [Test]
        public void TestGetInt16Array()
        {
            Int16[] tv = new Int16[] { 0x0000, 0x1010, 0x70a0, 0x7fff };
            Binary bin = Serialize(tv);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Int16[] pv = root.GetInt16Array();

            Assert.AreEqual(tv, pv);
            try
            {
                pv[5] = pv[5];
                Assert.Fail("Should've thrown IndexOutOfRangeException.");
            }
            catch (IndexOutOfRangeException) { }
        }

        [Test]
        public void TestGetInt32Array()
        {
            Int32[] tv = new Int32[] { 0, 0x70f0f0f0, 0x7fffffff };
            Binary bin = Serialize(tv);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Int32[] pv = root.GetInt32Array();

            Assert.AreEqual(tv, pv);
            try
            {
                pv[5] = pv[5];
                Assert.Fail("Should've thrown IndexOutOfRangeException.");
            }
            catch (IndexOutOfRangeException) { }
        }

        [Test]
        public void TestGetInt64Array()
        {
            Int64[] tv = new Int64[] { 0, 0xf0f0f0f0, 0xffffffff, 0x7fffffffffffffff };
            Binary bin = Serialize(tv);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Int64[] pv = root.GetInt64Array();

            Assert.AreEqual(tv, pv);
            try
            {
                pv[5] = pv[5];
                Assert.Fail("Should've thrown IndexOutOfRangeException.");
            }
            catch (IndexOutOfRangeException) { }
        }

        [Test]
        public void TestGetSingleArray()
        {
            Single[] tv = new Single[] { 0, 1.1f, 2.2f, 3.3f, 4.4f };
            Binary bin = Serialize(tv);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Single[] pv = root.GetSingleArray();

            Assert.AreEqual(tv, pv);
            try
            {
                pv[5] = pv[5];
                Assert.Fail("Should've thrown IndexOutOfRangeException.");
            }
            catch (IndexOutOfRangeException) { }
        }

        [Test]
        public void TestGetDoubleArray()
        {
            Double[] tv = new Double[] { 0, 1.1d, 2.2d, 3.3f, 4.4d };
            Binary bin = Serialize(tv);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            Double[] pv = root.GetDoubleArray();

            Assert.AreEqual(tv, pv);
            try
            {
                pv[5] = pv[5];
                Assert.Fail("Should've thrown IndexOutOfRangeException.");
            }
            catch (IndexOutOfRangeException) { }
        }

        [Test]
        public void TestGetCollection()
        {
            // perform the test twice, once with references disable, once with them enabled
            for (bool isRefEnabled = false; ; )
            {
                Stack coll = new Stack();
                coll.Push("One");
                coll.Push("Two");
                coll.Push("Three");
                Binary bin = Serialize(coll, isRefEnabled);

                IPofValue root = PofValueParser.Parse(bin, GetPofContext(isRefEnabled));
                ICollection pv = root.GetCollection(null);

                Assert.AreEqual(coll.ToArray(), pv);
                Queue res = new Queue();
                res.Enqueue("Append");
                pv = root.GetCollection(res);
                Assert.AreSame(res, pv);
                if (isRefEnabled)
                {
                    break;
                }
                isRefEnabled = true;
            }
        }

        [Test]
        public void TestGetCollectionT()
        {
            List<string> coll = new List<string> { "One", "Two", "Three" };
            Binary bin = Serialize(coll);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            System.Collections.Generic.ICollection<string> pv = root.GetCollection<string>(new List<string>());
       
            Assert.AreEqual(coll.Count, pv.Count);
            foreach (var v in pv)
            {
                Assert.IsTrue(coll.Contains(v));
            }

            pv = root.GetCollection<string>(null);
            Assert.AreEqual(coll.Count, pv.Count);
            foreach (var v in pv)
            {
                Assert.IsTrue(coll.Contains(v));
            }
        }

        [Test]
        public void TestGetDictionary()
        {
            IDictionary dict = new Hashtable {{"One", 1}, {"Two", 2}, {"Three", 3}};
            Binary bin = Serialize(dict);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            IDictionary pv = root.GetDictionary(null);

            Assert.AreEqual(dict.Count, pv.Count);
            foreach (DictionaryEntry dentry in pv)
            {
                Assert.AreEqual(dict[dentry.Key], pv[dentry.Key]);
            }

            IDictionary res = new Hashtable {{"Append", 0}};
            pv = root.GetDictionary(res);
            Assert.AreSame(res, pv);
        }

        [Test]
        public void TestGetDictionaryT()
        {
            IDictionary<string, Int32> dict = new Dictionary<string, Int32> { { "One", 1 }, { "Two", 2 }, { "Three", 3 } };
            Binary bin = Serialize(dict);

            IPofValue root = PofValueParser.Parse(bin, GetPofContext());
            IDictionary<string, Int32> pv = root.GetDictionary<string, Int32>(null);

            Assert.AreEqual(dict.Count, pv.Count);
            foreach (KeyValuePair<string, Int32> entry in pv)
            {
                Assert.AreEqual(dict[entry.Key], pv[entry.Key]);
            }

            IDictionary<string, Int32> res = new Dictionary<string, Int32> { { "Append", 0 } };
            pv = root.GetDictionary<string, Int32>(res);
            Assert.AreSame(res, pv);
        }

        [Test]
        public void TestCOH5231()
        {
            var holder = new BooleanHolder {Boolean1 = false, Boolean2 = true};
            Binary bin = Serialize(holder);

            IPofValue root  = PofValueParser.Parse(bin, GetPofContext());
            IPofValue bool1 = new SimplePofPath(0).Navigate(root);
            IPofValue bool2 = new SimplePofPath(1).Navigate(root);

            Assert.IsFalse(bool1.GetBoolean());
            Assert.IsTrue((bool) bool2.GetValue());
        }

        [Test]
        public void TestReferencesWithComplexObject()
        {
            var ivan  = new PortablePersonReference("Ivan", new DateTime(78, 4, 25));
            var goran = new PortablePersonReference("Goran", new DateTime(82, 3, 3));
            var anna  = new PortablePersonReference("Anna", new DateTime(80, 4, 12));
            var tom   = new PortablePerson("Tom", new DateTime(103, 7, 5));
            var ellen = new PortablePerson("Ellen", new DateTime(105, 3, 15));

            ivan.Children = null;
            goran.Children = new PortablePerson[2];
            goran.Children[0] = tom;
            goran.Children[1] = ellen;
            anna.Children = new PortablePerson[2];
            anna.Children[0] = tom;
            anna.Children[1] = ellen;
            ivan.Siblings = new PortablePersonReference[1];
            ivan.Siblings[0] = goran;
            goran.Siblings = new PortablePersonReference[1];
            goran.Siblings[0] = ivan;
            goran.Spouse = anna;
            anna.Spouse = goran;

            IDictionary<CompositeKey, IPortableObject> mapPerson = new Dictionary<CompositeKey, IPortableObject>();
            String                                     lastName  = "Smith";
            CompositeKey                               key1      = new CompositeKey(lastName, "ivan"),
                                                       key2      = new CompositeKey(lastName, "goran");
            mapPerson.Add(key1, ivan);
            mapPerson.Add(key2, goran);

            Binary      bin       = Serialize(mapPerson, true);
            IPofValue   pv        = PofValueParser.Parse(bin, GetPofContext(true));
            IDictionary mapResult = pv.GetDictionary(null);

            Assert.AreEqual(2, mapResult.Count);
            PortablePersonReference ivanR  = (PortablePersonReference) mapResult[key1];
            PortablePersonReference goranR = (PortablePersonReference) mapResult[key2];
            Assert.AreEqual(goran.Name, goranR.Name);
            Assert.IsTrue(ivanR.Siblings[0] == goranR);
            Assert.IsTrue(goranR.Spouse.Children[0] == goranR.Children[0]);

            bin = Serialize(ivan, true);
            pv  = PofValueParser.Parse(bin, GetPofContext(true));

            ivanR = (PortablePersonReference) pv.Root.GetValue();
            goranR = ivanR.Siblings[0];
            Assert.IsTrue(goranR.Siblings[0] == ivanR);

            ivanR = (PortablePersonReference) pv.GetValue(PofConstants.T_UNKNOWN);
            goranR = ivanR.Siblings[0];
            Assert.IsTrue(goranR.Siblings[0] == ivanR);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Creates a SimplePofContext with neccessary test type registrations.
        /// </summary>
        /// <returns>
        /// A configured test POF context.
        /// </returns>
        public static IPofContext GetPofContext()
        {
            return GetPofContext(false);
        }

        /// <summary>
        /// Creates a POF context with neccessary test type registrations.
        /// </summary>
        /// <param name="isRefEnabled">
        /// Flag to indicate if object identity/reference is enabled.
        /// </param>
        /// <returns>
        /// A configured test POF context.
        /// </returns>
        public static IPofContext GetPofContext(bool isRefEnabled)
        {
            if (isRefEnabled)
            {
                return new ConfigurablePofContext("Config/reference-pof-config.xml");
            }
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(3010, typeof(Address), new PortableObjectSerializer(3010));
            ctx.RegisterUserType(101, typeof(PortablePerson), new PortableObjectSerializer(101));
            ctx.RegisterUserType(3011, typeof(BooleanHolder), new PortableObjectSerializer(3011));
            ctx.RegisterUserType(3012, typeof(TestValue), new PortableObjectSerializer(3012));
            return ctx;
        }

        /// <summary>
        /// Creates a populated instance of a PortablePerson class to be used
        /// in tests.
        /// </summary>
        /// <returns>
        /// A populated instance of a PortablePerson class to be used in tests.
        /// </returns>
        public static PortablePerson CreatePerson()
        {
            PortablePerson p = new PortablePerson("Aleksandar Seovic", new DateTime(1974, 8, 24));
            p.Address = new Address("123 Main St", "Tampa", "FL", "12345");
            p.Spouse = new PortablePerson("Marija Seovic", new DateTime(1978, 2, 20));
            p.Children = new PortablePerson[] {
                    new PortablePerson("Ana Maria Seovic", new DateTime(2004, 8, 14)),
                    new PortablePerson("Novak Seovic", new DateTime(2007, 12, 28))
            };
            return p;
        }

        /// <summary>
        /// Creates a populated instance of a PortablePerson class with no
        /// children to be used in tests.
        /// </summary>
        /// <returns>
        /// A populated instance of a PortablePerson class to be used in tests.
        /// </returns>
        public static PortablePerson CreatePersonNoChildren()
        {
            PortablePerson p = new PortablePerson("Aleksandar Seovic", new DateTime(1974, 8, 24));
            p.Address = new Address("123 Main St", "Tampa", "FL", "12345");
            p.Spouse = new PortablePerson("Marija Seovic", new DateTime(1978, 2, 20));
            return p;
        }

        /// <summary>
        /// Creates a populated instance of a TestValue class to be used in tests.
        /// </summary>
        /// <param name="isRefEnabled">
        /// Flag to indicate if object identity/reference is enabled.
        /// </param>
        /// <returns>
        /// A populated instance of a TestValue class to be used in tests.
        /// </returns>
        public static TestValue CreateTestValue(bool isRefEnabled)
        {
            PortablePerson person   = isRefEnabled ? CreatePersonNoChildren() : CreatePerson();
            Object[]       aObj     = new Object[] { 1, "two", person };
            String[]       aStr     = new String[] { "one", "two", "three", "four" };

            IList lstObj = new ArrayList();
            lstObj.Add(1);
            lstObj.Add("two");
            lstObj.Add(person);

            IList lstStr = new ArrayList();
            lstStr.Add("one");
            lstStr.Add("two");
            lstStr.Add("three");
            lstStr.Add("four");

            ILongArray oSparseArray = new LongSortedList();
            oSparseArray[2] = "two";
            oSparseArray[4] = 4;
            oSparseArray[5] = person;

            ILongArray oUniformSparseArray = new LongSortedList();
            oUniformSparseArray[2] = "two";
            oUniformSparseArray[4] = "four";

            return new TestValue(aObj, aStr,
                                 lstObj, lstStr,
                                 oSparseArray, oUniformSparseArray);
        }

        /// <summary>
        /// Serializes object using POF.
        /// </summary>
        /// <param name="o">
        /// Object to be serialized.
        /// </param>
        /// <returns>
        /// POF-encoded serialized binary value.
        /// </returns>
        public static Binary Serialize(Object o)
        {
            return Serialize(o, false);
        }

        /// <summary>
        /// Serializes object using POF.
        /// </summary>
        /// <param name="o">
        /// Object to be serialized.
        /// </param>
        /// <param name="isRefEnabled">
        /// Flag to indicate if object identity/reference is enabled.
        /// </param>
        /// <returns>
        /// POF-encoded serialized binary value.
        /// </returns>
        public static Binary Serialize(Object o, bool isRefEnabled)
        {
            BinaryMemoryStream buf = new BinaryMemoryStream(1000);
            GetPofContext(isRefEnabled).Serialize(new DataWriter(buf), o);
            return buf.ToBinary();
        }

        /// <summary>
        /// Deserializes binary data using POF.
        /// </summary>
        /// <param name="bin">
        /// POF-encoded serialized binary.
        /// </param>
        /// <returns>
        /// Deserialized object instance.
        /// </returns>
        public static Object Deserialize(Binary bin)
        {
            return Deserialize(bin, false);
        }

        /// <summary>
        /// Deserializes binary data using POF.
        /// </summary>
        /// <param name="isRefEnabled">
        /// Flag to indicate if object identity/reference is enabled.
        /// </param>
        /// <returns>
        /// Deserialized object instance.
        /// </returns>
        public static Object Deserialize(Binary bin, bool isRefEnabled)
        {
            return GetPofContext(isRefEnabled).Deserialize(bin.GetReader());
        }


        // ----- TestValue class ------------------------------------------------

        public class TestValue : IPortableObject
        {
            public TestValue()
            {}

            public TestValue(Object[] oArray, String[] sArray,
                         IList col, IList colUniform,
                         ILongArray oSparseArray, ILongArray oUniformSparseArray)
            {
                m_oArray = oArray;
                m_sArray = sArray;
                m_col = col;
                m_colUniform = colUniform;
                m_sparseArray = oSparseArray;
                m_uniformSparseArray = oUniformSparseArray;
            }

            public void ReadExternal(IPofReader reader)
            {
                m_oArray = (object[]) reader.ReadArray(0, new Object[0]);
                m_sArray = (String[]) reader.ReadArray(1, new String[0]);
                m_col = (IList) reader.ReadCollection(2, new ArrayList());
                m_colUniform = (IList) reader.ReadCollection(3, new ArrayList());
                m_sparseArray = reader.ReadLongArray(4, new LongSortedList());
                m_uniformSparseArray = reader.ReadLongArray(5, new LongSortedList());
            }

            public void WriteExternal(IPofWriter writer)
            {
                writer.WriteArray(0, m_oArray);
                writer.WriteArray(1, m_sArray, typeof(String));
                writer.WriteCollection(2, m_col);
                writer.WriteCollection(3, m_colUniform, typeof(String));
                writer.WriteLongArray(4, m_sparseArray);
                writer.WriteLongArray(5, m_uniformSparseArray, typeof(String));
            }


        // ----- data members -----------------------------------------------

        public Object[] m_oArray;
        public String[] m_sArray;
        public IList m_col;
        public IList m_colUniform;
        public ILongArray m_sparseArray;
        public ILongArray m_uniformSparseArray;
        }

        #endregion
    }
}