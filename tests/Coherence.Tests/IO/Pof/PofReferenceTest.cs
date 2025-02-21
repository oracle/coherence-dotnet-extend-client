/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

using NUnit.Framework;

using Tangosol.IO.Resources;
using Tangosol.Net;
using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class PofReferenceTest
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

        [Test]
        public void TestEnableReferenceConfig()
        {
            const string path   = "Config/reference-pof-config.xml";
            var          ctx    = new ConfigurablePofContext(path);
            IXmlElement  config = ctx.Config;
            Assert.IsTrue(ctx.IsReferenceEnabled);
        }

        [Test]
        public void TestDuplicateObjectReferences()
        {
            // loop twice, one for SimplePofContext, one for 
            // ConfigurablePofContext.
            for (int loop = 0; loop < 2; loop++)
            {
                if (loop == 0)
                {
                    m_ctx = new SimplePofContext();
                    ((SimplePofContext) m_ctx).RegisterUserType(101,
                            typeof(PortablePerson), 
                            new PortableObjectSerializer(101));
                    ((SimplePofContext) m_ctx).RegisterUserType(201,
                            typeof(CompositeKey),
                            new PortableObjectSerializer(201));
                    ((SimplePofContext) m_ctx).IsReferenceEnabled = true;
                }
                else
                {
                    const String sPath = "Config/reference-pof-config.xml";
                    m_ctx = new ConfigurablePofContext(sPath);
                }

                initPOFWriter();
                var joe          = new PortablePerson("Joe Smith", 
                        new DateTime(78, 4, 25));
                var differentJoe = new PortablePerson("Joe Smith",
                        new DateTime(78, 4, 25));
                var key          = new CompositeKey(joe, joe);

                Assert.IsTrue(key.PrimaryKey == key.SecondaryKey);

                // test a collection of duplicate object references
                ICollection<PortablePerson> list = new List<PortablePerson>();
                list.Add(joe);
                list.Add(joe);
                list.Add(differentJoe);
                list.Add(differentJoe);

                m_pofWriter.EnableReference();
                m_pofWriter.WriteObject(0, key);
                m_pofWriter.WriteObject(0, list);

                initPOFReader();
                var result = (CompositeKey) m_pofReader.ReadObject(0);
                Assert.IsTrue(result.PrimaryKey == result.SecondaryKey);

                ICollection<object> result2 = new List<object>();
                m_pofReader.ReadCollection(0, result2);

                PortablePerson person = null;
                int            i      = 0;
                for (IEnumerator iter = result2.GetEnumerator();
                     iter.MoveNext();)
                {
                    var personNext = (PortablePerson) iter.Current;
                    if (person == null)
                    {
                        person = personNext;
                        i++;
                    }
                    else
                    {
                        Assert.IsTrue(person.Equals(personNext));
                        if (i == 1 || i == 3)
                            Assert.IsTrue(person == personNext);
                        else
                            person = personNext;
                        i++;
                    }
                }
            }
        }

        [Test]
        public void TestReferencesInUniformArray()
        {
            var localCtx = new SimplePofContext();
            localCtx.RegisterUserType(101, typeof(PortablePerson), 
                    new PortableObjectSerializer(101));
            localCtx.RegisterUserType(102, typeof(PortablePersonReference), 
                    new PortableObjectSerializer(102));
            localCtx.RegisterUserType(201, typeof(CompositeKey), 
                    new PortableObjectSerializer(201));
            localCtx.IsReferenceEnabled = true;

            var ivan  = new PortablePersonReference("Ivan", new DateTime(78, 4, 25));
            var goran = new PortablePersonReference("Goran", new DateTime(82, 3, 3));
            ivan.Children = null;
            goran.Children = new PortablePerson[2];
            goran.Children[0] = new PortablePerson("Tom", new DateTime(103, 7, 5));
            goran.Children[1] = new PortablePerson("Ellen", new DateTime(105, 3, 15));
            ivan.Siblings = new PortablePersonReference[1];
            ivan.Siblings[0] = goran;
            goran.Siblings = new PortablePersonReference[1];
            goran.Siblings[0] = ivan;

            IDictionary  mapPerson = new Dictionary<CompositeKey, IPortableObject>();
            const String lastName  = "Smith";
            CompositeKey key1      = new CompositeKey(lastName, "ivan"),
                         key2      = new CompositeKey(lastName, "goran");
            mapPerson.Add(key1, ivan);
            mapPerson.Add(key2, goran);

            initPOFWriter();
            m_pofWriter = new PofStreamWriter(m_writer, localCtx);
            if (localCtx.IsReferenceEnabled)
            {
                m_pofWriter.EnableReference();
            }
            m_pofWriter.WriteDictionary(0, mapPerson);

            initPOFReader();
            m_pofReader = new PofStreamReader(m_reader, localCtx);

            IDictionary mapResult = m_pofReader.ReadDictionary(0, null);
            Assert.IsTrue(2 == mapResult.Count);

            var          ivanR  = (PortablePersonReference) mapResult[key1];
            var          goranR = (PortablePersonReference) mapResult[key2];
            ICollection  keySet = mapResult.Keys;
            IEnumerator  iter   = keySet.GetEnumerator();
            iter.MoveNext();
            var key1R = (CompositeKey) iter.Current;
            iter.MoveNext();
            var key2R = (CompositeKey) iter.Current;

            Assert.IsFalse(key1R.PrimaryKey == key2R.PrimaryKey);
            Assert.IsTrue(ivanR.Siblings[0] == goranR);
            Assert.IsTrue(goran.Name.Equals(goranR.Name));
            Assert.IsNull(ivanR.Children);
        }

        [Test]
        public void TestReferencesInUniformMap()
        {
            var localCtx = new SimplePofContext();
            localCtx.RegisterUserType(101, typeof(PortablePerson), 
                    new PortableObjectSerializer(101));
            localCtx.RegisterUserType(102, typeof(PortablePersonReference), 
                    new PortableObjectSerializer(102));
            localCtx.RegisterUserType(201, typeof(CompositeKey), 
                    new PortableObjectSerializer(201));

            IDictionary  mapPerson = new Dictionary<CompositeKey, IPortableObject>();
            const String lastName  = "Smith";
            CompositeKey key1      = new CompositeKey(lastName, "ivan"),
                         key2      = new CompositeKey(lastName, "goran");
            var          ivan      = new PortablePersonReference("Ivan", new DateTime(78, 4, 25));

            ivan.Children = null;
            mapPerson.Add(key1, ivan);
            mapPerson.Add(key2, ivan);

            initPOFWriter();
            m_pofWriter = new PofStreamWriter(m_writer, localCtx);
            m_pofWriter.EnableReference();
            m_pofWriter.WriteDictionary(0, mapPerson, typeof(CompositeKey),
                    typeof(PortablePersonReference));

            var mapPersonR = new Dictionary<CompositeKey, IPortableObject>();
            initPOFReader();
            m_pofReader = new PofStreamReader(m_reader, localCtx);
            m_pofReader.ReadDictionary(0, (IDictionary) mapPersonR);

            // compare mapPerson with result
            Assert.IsTrue(mapPersonR[key1].Equals(mapPerson[key1]));
            Assert.IsTrue(mapPersonR[key2].Equals(mapPerson[key2]));

            ICollection colValR = mapPersonR.Values;
            IEnumerator iter    = colValR.GetEnumerator();
            iter.MoveNext();
            var val1 = (PortablePersonReference) iter.Current;
            iter.MoveNext();
            var val2 = (PortablePersonReference) iter.Current;
            Assert.IsTrue(val1 == val2);
        }

        [Test]
        public void TestReferencesInArray()
        {
            var ctx = new SimplePofContext();
            ctx.RegisterUserType(101, typeof(PortablePerson), 
                    new PortableObjectSerializer(101));
            ctx.RegisterUserType(102, typeof(PortablePersonReference), 
                    new PortableObjectSerializer(102));

            var ivan     = new PortablePersonReference("Ivan", new DateTime(78, 4, 25));
            var goran    = new PortablePersonReference("Goran", new DateTime(82, 3, 3));
            var jack     = new PortablePersonReference("Jack", new DateTime(80, 5, 25));
            var jim      = new PortablePersonReference("Jim", new DateTime(80, 5, 25));
            var siblings = new PortablePersonReference[2];
            siblings[0] = jack;
            siblings[1] = jim;

            ivan.Children = null;
            jack.Children = null;
            jim.Children = null;
            goran.Children = new PortablePerson[2];
            goran.Children[0] = new PortablePerson("Tom", new DateTime(103, 7, 5));
            goran.Children[1] = new PortablePerson("Ellen", new DateTime(105, 3, 15));
            ivan.Siblings = siblings;
            goran.Siblings = siblings;
            Assert.IsTrue(ivan.Siblings == goran.Siblings);

            var col1 = new Collection<IPortableObject>();
            col1.Add(ivan);
            col1.Add(goran);

            initPOFWriter();
            m_pofWriter = new PofStreamWriter(m_writer, ctx);
            m_pofWriter.EnableReference();
            m_pofWriter.WriteCollection(0, (ICollection) col1);
            m_pofWriter.WriteCollection(0, col1, typeof(PortablePersonReference));

            initPOFReader();
            m_pofReader = new PofStreamReader(m_reader, ctx);

            var result  = m_pofReader.ReadCollection(0, null);
            var result2 = m_pofReader.ReadCollection(0, null);
            Assert.IsTrue(2 == result.Count);

            IEnumerator iter = result.GetEnumerator();
            iter.MoveNext();
            var ivanR  = (PortablePersonReference) iter.Current;
            iter.MoveNext();
            var goranR = (PortablePersonReference) iter.Current;

            Assert.IsFalse(ivanR.Siblings == goranR.Siblings);
            Assert.IsTrue(ivanR.Siblings[0] == goranR.Siblings[0]);
            Assert.IsTrue(ivanR.Siblings[1] == goranR.Siblings[1]);
            Assert.IsNull(ivanR.Children);

            iter = result2.GetEnumerator();
            iter.MoveNext();
            var ivanR2  = (PortablePersonReference) iter.Current;
            iter.MoveNext();
            var goranR2 = (PortablePersonReference) iter.Current;

            Assert.IsFalse(ivanR2.Siblings == goranR2.Siblings);
            Assert.IsTrue(ivanR2.Siblings[0] == goranR2.Siblings[0]);
            Assert.IsTrue(ivanR2.Siblings[1] == goranR2.Siblings[1]);
            Assert.IsNull(ivanR2.Children);
        }

        [Test]
        public void TestEvolvableObjectSerialization()
        {
            var ctxV1 = new SimplePofContext();
            ctxV1.RegisterUserType(1, typeof(EvolvablePortablePerson),
                    new PortableObjectSerializer(1));
            ctxV1.RegisterUserType(2, typeof(Address),
                    new PortableObjectSerializer(2));
            ctxV1.IsReferenceEnabled = true;

            var ctxV2 = new SimplePofContext();
            ctxV2.RegisterUserType(1, typeof(EvolvablePortablePerson2),
                    new PortableObjectSerializer(1));
            ctxV2.RegisterUserType(2, typeof(Address),
                    new PortableObjectSerializer(2));
            ctxV2.IsReferenceEnabled = true;

            var person12 = new EvolvablePortablePerson2(
                    "Aleksandar Seovic", new DateTime(74, 7, 24));
            var person22 = new EvolvablePortablePerson2(
                    "Ana Maria Seovic", new DateTime(104, 7, 14, 7, 43, 0));
            var person32 = new EvolvablePortablePerson2(
                    "Art Seovic", new DateTime(107, 8, 12, 5, 20, 0));

            var addr    = new Address("208 Myrtle Ridge Rd", "Lutz", "FL", "33549");
            var addrPOB = new Address("128 Asbury Ave, #401", "Evanston", "IL", "60202");
            person12.Address = addr;
            person22.Address = addr;

            person12.Nationality = person22.Nationality = "Serbian";
            person12.PlaceOfBirth = new Address(null, "Belgrade", "Serbia", "11000");
            person22.PlaceOfBirth = addrPOB;
            person32.PlaceOfBirth = addrPOB;
            person12.Children = new EvolvablePortablePerson2[] { person22, person32 };

            initPOFWriter();
            m_pofWriter = new PofStreamWriter(m_writer, ctxV2);
            m_pofWriter.EnableReference();
            m_pofWriter.WriteObject(0, person12);

            initPOFReader();
            m_pofReader = new PofStreamReader(m_reader, ctxV1);
            var person11 = (EvolvablePortablePerson) m_pofReader.ReadObject(0);
            var person21 = new EvolvablePortablePerson(
                    "Marija Seovic", new DateTime(78, 1, 20));
            person21.Address = person11.Address;
            person21.Children = person11.Children;
            person11.Spouse = person21;

            initPOFWriter();
            m_pofWriter = new PofStreamWriter(m_writer, ctxV1);
            m_pofWriter.EnableReference();
            m_pofWriter.WriteObject(0, person11);

            initPOFReader();
            m_pofReader = new PofStreamReader(m_reader, ctxV2);
            var person = (EvolvablePortablePerson2) m_pofReader.ReadObject(0);

            Assert.IsTrue(person12.Name.Equals(person.Name));
            Assert.IsTrue(person12.Nationality.Equals(person.Nationality));
            Assert.IsTrue(person12.DOB.Equals(person.DOB));
            Assert.IsTrue(person11.Spouse.Name.Equals(person.Spouse.Name));
            Assert.IsTrue(person12.Address.Equals(person.Address));
            Assert.IsTrue(person12.PlaceOfBirth.Equals(person.PlaceOfBirth));
            Assert.IsTrue(person.Address != person.Children[0].Address);
            Assert.IsTrue(person.Address != person.Spouse.Address);
            Assert.IsTrue(person.Children[0] != person.Spouse.Children[0]);
            Assert.IsTrue(person.Children[1] != person.Spouse.Children[1]);
        }

        [Test]
        public void testCircularReferences()
        {
            var ctx = new SimplePofContext();
            ctx.RegisterUserType(101, typeof(PortablePerson), 
                    new PortableObjectSerializer(101));
            ctx.RegisterUserType(102, typeof(PortablePersonReference), 
                    new PortableObjectSerializer(102));

            var ivan = new PortablePersonReference("Ivan", new DateTime(78, 4, 25));
            ivan.Children = new PortablePerson[1];
            ivan.Children[0] = new PortablePerson("Mary Jane", new DateTime(97, 8, 14));
            ivan.Spouse = new PortablePerson("Eda", new DateTime(79, 6, 25));

            var goran = new PortablePersonReference("Goran", new DateTime(82, 3, 3));
            goran.Children = new PortablePerson[2];
            goran.Children[0] = new PortablePerson("Tom", new DateTime(103, 7, 5));
            goran.Children[1] = new PortablePerson("Ellen", new DateTime(105, 3, 15));
            goran.Spouse = new PortablePerson("Tiffany", new DateTime(82, 3, 25));
            goran.Friend = ivan;
            ivan.Friend = goran;

            initPOFWriter();
            m_pofWriter = new PofStreamWriter(m_writer, ctx);
            m_pofWriter.EnableReference();
            m_pofWriter.WriteObject(0, ivan);

            initPOFReader();
            m_pofReader = new PofStreamReader(m_reader, ctx);

            var ivanR = (PortablePersonReference) m_pofReader.ReadObject(0);
            Assert.IsTrue(ivanR.Name.Equals(ivan.Name));
            Assert.IsTrue(ivanR.Children.Length == 1);
            Assert.IsTrue(ivanR.Friend.Equals(goran));
        }

        [Test]
        public void testNestedReferences()
        {
            const String sPath = "Config/reference-pof-config.xml";
            m_ctx = new ConfigurablePofContext(sPath);
            IXmlElement config = ((ConfigurablePofContext) m_ctx).Config;
          
            var          pm    = new PofMaster();
            var          pc1   = new PofChild();
            var          pc2   = new PofChild();
            ArrayList    list1 = null;
            var          list2 = new ArrayList();
            var          list3 = new ArrayList();
            IDictionary  map1  = null;
            IDictionary  map2  = new Hashtable();
            IDictionary  map3  = map2;

            list3.Add(0);
            map3.Add("key1", pc1);
            map3.Add("key2", pc2);
            pc1.setId("child1");
            pc2.setId("child2");

            pm.setList1(list1);
            pm.setList2(list2);
            pm.setList3(list3);
            pm.setMap1(map1);
            pm.setMap2(map2);
            pm.setMap3(map3);
            pm.setNumber(9999);
            pm.setText("cross fingers");
            pm.setChildren(new PofChild[] {pc1, pc2, pc2});

            initPOFWriter();
            m_ctx.Serialize(m_writer, pm);

            initPOFReader();
            var pm2 = (PofMaster) m_ctx.Deserialize(m_reader);

            Assert.IsTrue(pm.Equals(pm2));
            IDictionary map2R = pm2.getMap2();
            IDictionary map3R = pm2.getMap3();

            Assert.IsTrue(map2R != map3R);
            Assert.IsTrue(map2R["key1"] == map3R["key1"]);
            Assert.IsTrue(map2R["key2"] == map3R["key2"]);

            PofChild[] children = pm2.getChildren();
            Assert.IsTrue(children[0] == map2R["key1"]);
            Assert.IsTrue(children[1] == children[2]);
        }

        /// <summary>
        /// Test Nested Type.
        /// </summary>
        [Test]
        public void testNestedType()
        {
            m_ctx = new SimplePofContext();
            ((SimplePofContext) m_ctx).RegisterUserType(101, typeof(NestedTypeWithReference), new PortableObjectSerializer(101));
            ((SimplePofContext) m_ctx).RegisterUserType(102, typeof(PortablePerson), new PortableObjectSerializer(102));

            var tv = new NestedTypeWithReference();

            initPOFWriter();
            m_pofWriter.EnableReference();
            m_pofWriter.WriteObject(0, tv);

            initPOFReader();
            var result = (NestedTypeWithReference) m_pofReader.ReadObject(0);
        }

        [Test]
        public void TestCacheOperations()
        {
            ConfigurablePofContext.DefaultPofConfig = XmlHelper.LoadResource(
                ResourceLoader.GetResource(
                    "assembly://Coherence.Tests/Tangosol.Resources/s4hc-test-reference-config.xml"),
                    "POF configuration");
            INamedCache cache = CacheFactory.GetCache("dist-pof-test");

            PortablePerson ellen = new PortablePerson("Ellen", new DateTime(105, 3, 15));
            PortablePerson tom   = new PortablePerson("Tom", new DateTime(103, 7, 5)){
                    Children = new PortablePerson[]{ellen}};

            cache.Add(tom.Name, tom);

            PortablePerson netObj = (PortablePerson) cache[tom.Name];
            Assert.IsTrue(tom.Equals(netObj));
            CacheFactory.Shutdown();
            ConfigurablePofContext.DefaultPofConfig = null;
        }

        /// <summary>
        /// Regression test for COH-8911.
        /// </summary>
        [Test]
        public void TestCompositeKey()
        {
            ConfigurablePofContext.DefaultPofConfig = XmlHelper.LoadResource(
                ResourceLoader.GetResource(
                    "assembly://Coherence.Tests/Tangosol.Resources/s4hc-test-reference-config.xml"),
                    "POF configuration");
            INamedCache cache = CacheFactory.GetCache("dist-pof-test");

            cache.Clear();
            CompositeKey   key0    = new CompositeKey(new PortablePerson("Joe Smith", new DateTime(78, 4, 25)),
                                       new PortablePerson("Joe Smith", new DateTime(78, 4, 25)));
            PortablePerson person1 = new PortablePerson("Joe Smith", new DateTime(78, 4, 25));
            CompositeKey   key1    = new CompositeKey(person1, person1);

            cache.Add(key0, "value0");
            cache.Add(key1, "value1");
            Assert.AreEqual(1, cache.Count);
            CacheFactory.Shutdown();
            ConfigurablePofContext.DefaultPofConfig = null;
        }

        private void initPOFReader()
        {
            if (m_ctx == null)
            {
                m_ctx = new SimplePofContext();
            }
            m_stream.Position = 0;
            m_reader = new DataReader(m_stream);
            m_pofReader = new PofStreamReader(m_reader, m_ctx);
        }

        private void initPOFWriter()
        {
            if (m_ctx == null)
            {
                m_ctx = new SimplePofContext();
            }
            m_stream = new MemoryStream();
            m_writer = new DataWriter(m_stream);
            m_pofWriter = new PofStreamWriter(m_writer, m_ctx);
        }

        private IPofContext m_ctx;
        private DataReader m_reader;
        private DataWriter m_writer;
        private PofStreamReader m_pofReader;
        private PofStreamWriter m_pofWriter;
        private MemoryStream m_stream;
    }


    public class NestedTypeWithReference : IPortableObject
    {
        public static bool ArrayEqual(IList a1, IList a2)
        {
            if (a1 == a2)
                return true;
            if (a1.Count != a2.Count)
                return false;
            for (int i = 0; i < a1.Count; i++)
            {
                if (a1[i].ToString() != a2[i].ToString())
                    return false;
            }
            return true;
        }

        public static bool MapEqual(IDictionary map1, IDictionary map2)
        {
            if (map1 == map2)
                return true;
            if (map1.Count != map2.Count)
                return false;
            ICollection keys1 = map1.Keys;
            IEnumerator iter  = keys1.GetEnumerator();
            while (iter.MoveNext())
            {
                if (Equals(map1[iter.Current], map2[iter.Current]))
                    return false;
            }
            return true;
        }

        private const int                INTEGER      = 100;
        private const String             STRING       = "Hello World";
        private static readonly String[] STRING_ARRAY = { "one", "two", "three" };
        private static readonly double[] DOUBLE_ARRAY = new double[] { 1.0f,
                2.0f, 3.3f, 4.4f };
        private static readonly PortablePerson CHILD1 = new PortablePerson(
                "Tom", new DateTime(103, 7, 5));
        private static readonly PortablePerson CHILD2 = new PortablePerson(
                "Ellen", new DateTime(105, 3, 15));
        private static readonly PortablePerson PERSON = new PortablePerson(
                "Joe Smith", new DateTime(78, 4, 25)){
                Children = new PortablePerson[]{CHILD1, CHILD2}};

        private static readonly List<String> set      = new List<String>
                {
                    "four",
                    "five",
                    "six",
                    "seven",
                    "eight"
                };

        public void ReadExternal(IPofReader reader)
        {
            Assert.AreEqual(INTEGER, reader.ReadInt32(0));
            IPofReader nested1 = reader.CreateNestedPofReader(1);

            IPofReader nested2 = nested1.CreateNestedPofReader(0);
            Assert.AreEqual(STRING, nested2.ReadString(0));
            var      person2     = (PortablePerson) nested2.ReadObject(1);
            double[] doubleArray = nested2.ReadDoubleArray(2);
            Assert.AreEqual(ArrayEqual(DOUBLE_ARRAY, doubleArray), true);

            IPofReader nested3 = nested2.CreateNestedPofReader(3);
            var stringArray    = (String[]) nested3.ReadArray(0, new String[0]);
            Assert.IsTrue(ArrayEqual(stringArray, STRING_ARRAY));
            nested3.ReadRemainder();

            // close nested3 and continue to nested2
            bool boolVal = nested2.ReadBoolean(4);
            Assert.AreEqual(false, boolVal);

            // nested1
            ICollection col             = nested1.ReadCollection(1, null);
            ICollection<String> results = new Collection<String>();
            foreach (object res in col)
            {
                results.Add((string)res);
            }
            foreach (String val in set)
            {
                Assert.IsTrue(results.Contains(val));
            }

            Assert.AreEqual(2.0, nested1.ReadDouble(2));
            Assert.AreEqual(5, nested1.ReadInt32(3));

            results = nested1.ReadCollection(4, results);
            foreach (String val in set)
            {
                Assert.IsTrue(results.Contains(val));
            }

            var person1 = (PortablePerson) nested1.ReadObject(5);
            Assert.AreEqual(2.222, nested1.ReadDouble(10));

            nested1.ReadRemainder();

            Assert.AreEqual(4.444, reader.ReadDouble(2));
            Assert.AreEqual(15, reader.ReadInt32(3));
            var person = (PortablePerson) reader.ReadObject(4);
            Assert.IsTrue(person == person1);    
            Assert.IsTrue(person1 == person2);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32(0, INTEGER);

            IPofWriter nested1 = writer.CreateNestedPofWriter(1);

            IPofWriter nested2 = nested1.CreateNestedPofWriter(0);
            nested2.WriteString(0, STRING);
            nested2.WriteObject(1, PERSON);
            nested2.WriteDoubleArray(2, DOUBLE_ARRAY);

            IPofWriter nested3 = nested2.CreateNestedPofWriter(3);
            nested3.WriteArray(0, STRING_ARRAY, typeof(String));

            nested2.WriteBoolean(4, false);
            nested2.WriteRemainder(null);

            nested1.WriteCollection(1, (ICollection<String>) set);
            nested1.WriteDouble(2, 2.0);
            nested1.WriteInt32(3, 5);
            nested1.WriteCollection(4, set, typeof(String));
            nested1.WriteObject(5,PERSON);
            nested1.WriteDouble(10, 2.222);

            writer.WriteDouble(2, 4.444);
            writer.WriteInt32(3, 15);
            writer.WriteObject(4, PERSON);
        }
    }

    public class PofMaster : IPortableObject
    {
        public IList getList1()
        {
            return m_list1;
        }

        public void setList1(IList list)
        {
            m_list1 = list;
        }

        public IList getList2()
        {
            return m_list2;
        }

        public void setList2(IList list)
        {
            m_list2 = list;
        }

        public IList getList3()
        {
            return m_list3;
        }

        public void setList3(IList list)
        {
            m_list3 = list;
        }

        public IDictionary getMap1()
        {
            return m_map1;
        }

        public void setMap1(IDictionary map)
        {
            m_map1 = map;
        }

        public IDictionary getMap2()
        {
            return m_map2;
        }

        public void setMap2(IDictionary map)
        {
            m_map2 = map;
        }

        public IDictionary getMap3()
        {
            return m_map3;
        }

        public void setMap3(IDictionary map)
        {
            m_map3 = map;
        }

        public int getNumber()
        {
            return m_n;
        }

        public void setNumber(int n)
        {
            m_n = n;
        }

        public String getText()
        {
            return m_s;
        }

        public void setText(String s)
        {
            m_s = s;
        }

        public PofChild[] getChildren()
        {
            return m_apc;
        }

        public void setChildren(PofChild[] apc)
        {
            m_apc = apc;
        }

        public void ReadExternal(IPofReader reader)
        {
            setList1((IList)reader.ReadCollection(0, null));
            setList2((IList)reader.ReadCollection(1, new ArrayList()));
            setList3((IList)reader.ReadCollection(2, null));
            setMap1(reader.ReadDictionary(3, null));
            setMap2(reader.ReadDictionary(4, new Hashtable()));
            setMap3(reader.ReadDictionary(5, null));
            setNumber(reader.ReadInt32(6));
            setText(reader.ReadString(7));
            setChildren((PofChild[]) reader.ReadArray(8, new PofChild[0]));
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteCollection(0, getList1());
            writer.WriteCollection(1, getList2());
            writer.WriteCollection(2, getList3());
            writer.WriteDictionary(3, getMap1());
            writer.WriteDictionary(4, getMap2());
            writer.WriteDictionary(5, getMap3());
            writer.WriteInt32(6, getNumber());
            writer.WriteString(7, getText());
            writer.WriteArray(8, getChildren(), typeof(PofChild));
        }

        public String toString()
        {
            PofChild[] apc = getChildren();
            var        sb  = new StringBuilder();
            if (apc == null)
            {
                sb.Append("null");
            }
            else
            {
                sb.Append('[');
                for (int i = 0, c = apc.Length; i < c; ++i)
                {
                    sb.Append(apc[i]);
                    if (i + 1 < c)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(']');
            }

            return "PofMaster\n    ("
                   + "\n    List1="    + getList1()
                   + "\n    List2="    + getList2()
                   + "\n    List3="    + getList3()
                   + "\n    Map1="     + getMap1()
                   + "\n    Map2="     + getMap2()
                   + "\n    Map3="     + getMap3()
                   + "\n    Number="   + getNumber()
                   + "\n    Text="     + getText()
                   + "\n    Children=" + sb.ToString()
                   + "\n    )";
        }

        public new bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PofMaster))
            {
                return false;
            }

            PofMaster other = (PofMaster) obj;
            return NestedTypeWithReference.ArrayEqual(getList1(), other.getList1())
                    && NestedTypeWithReference.ArrayEqual(getList2(), other.getList2())
                    && NestedTypeWithReference.ArrayEqual(getList3(), other.getList3())
                    && NestedTypeWithReference.MapEqual(getMap1(), other.getMap1())
                    && NestedTypeWithReference.MapEqual(getMap2(), other.getMap2())
                    && NestedTypeWithReference.MapEqual(getMap3(), other.getMap3())
                    && getNumber() == other.getNumber()
                    && getText().Equals(other.getText())
                    && NestedTypeWithReference.ArrayEqual(getChildren(), other.getChildren());
        }

        private IList m_list1;
        private IList m_list2;
        private IList m_list3;
        private IDictionary m_map1;
        private IDictionary m_map2;
        private IDictionary m_map3;
        private int         m_n;
        private String      m_s;
        private PofChild[]  m_apc;
    }

    public class PofChild : IPortableObject
    {
        public String getId()
        {
            return m_sId;
        }

        public void setId(String sId)
        {
            m_sId = sId;
        }

        public void ReadExternal(IPofReader reader)
        {
            setId(reader.ReadString(0));
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, getId());
        }

        public String toString()
        {
            return "PofChild(Id=" + getId() + ')';
        }

	    public new bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
            	return false;
            }

            if(!(obj is PofChild))
            {
                return false;
            }
                
            var other = (PofChild) obj;
            return m_sId.Equals(other.m_sId);
        }

        private String m_sId;
    }
}
