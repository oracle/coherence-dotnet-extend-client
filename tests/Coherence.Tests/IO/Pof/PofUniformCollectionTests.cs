/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NUnit.Framework;
using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class PofUniformCollectionTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(101,
                    typeof(DictionaryTestClass<Int32, String>),
                    new PortableObjectSerializer(101));

            ctx.RegisterUserType(102,
                    typeof(DictionaryTestClass<String, Int32>),
                    new PortableObjectSerializer(102));

            ctx.RegisterUserType(103,
                    typeof(CollectionTestClass<Int32>),
                    new PortableObjectSerializer(103));

            ctx.RegisterUserType(104,
                    typeof(DictionaryTestClass<int, object>),
                    new PortableObjectSerializer(104));
            m_serializer = ctx;
        }

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
        public void TestIntegerStringMap()
        {
            Dictionary<Int32, String> map = new Dictionary<Int32, String>
                                                {
                                                    { PofConstants.T_IDENTITY, "A string" }
                                                };
            VerifyPofSerializer(new DictionaryTestClass<Int32, String>(map, true));
        }

        [Test]
        public void TestIntegerStringKeyOnly()
        {
            Dictionary<Int32, String> map = new Dictionary<Int32, String>
                                                {
                                                    { PofConstants.T_IDENTITY, "A string" }
                                                };
            VerifyPofSerializer(new DictionaryTestClass<Int32, String>(map, false));
        }

        [Test]
        public void TestStringIntegerKeyOnly()
        {
            Dictionary<String, Int32> map = new Dictionary<String, Int32>
                                                {
                                                    { "A string", PofConstants.T_IDENTITY }
                                                };
            VerifyPofSerializer(new DictionaryTestClass<String, Int32>(map, false));
        }

        [Test]
        public void TestStringIntegerMap()
        {
            Dictionary<String, Int32> map = new Dictionary<String, Int32>
                                                {
                                                    { "A string", PofConstants.T_IDENTITY }
                                                };
            VerifyPofSerializer(new DictionaryTestClass<String, Int32>(map, true));
        }

        [Test]
        public void TestIntegerCollection()
        {
            var coll = new Collection<Int32> { PofConstants.T_IDENTITY };
            VerifyPofSerializer(new CollectionTestClass<Int32>(coll));
        }

        [Test]
        public void TestReadUniformKeyMap()
        {
            Dictionary<int, object> map = new Dictionary<int, object>();
            map[1] = "5";
            VerifyPofSerializer(new DictionaryTestClass<int, object>(map, false));
        }

        [Test]
        public void TestReadEmptyUniformKeyMap()
        {
            Dictionary<int, object> map = new Dictionary<int, object>();
            VerifyPofSerializer(new DictionaryTestClass<int, object>(map, false));
        }

        private void VerifyPofSerializer(Object testObject)
        {
            ISerializer serializer = m_serializer;
            Binary      binary     = SerializationHelper.ToBinary(testObject, serializer);
            Object      result     = SerializationHelper.FromBinary(binary, serializer);
            Assert.IsTrue(testObject.Equals(result));
        }

        public class DictionaryTestClass<K, V> : IPortableObject
        {
            public DictionaryTestClass()
            { }

            public DictionaryTestClass(Dictionary<K, V> dict, bool isUniformValue)
            {
                m_dict = dict;
                m_isUniformValue = isUniformValue;
            }

            public void ReadExternal(IPofReader reader)
            {
                m_dict = (Dictionary<K, V>) reader.ReadDictionary<K, V>(0, null);
            }

            public void WriteExternal(IPofWriter writer)
            {
                if (m_isUniformValue)
                {
                    writer.WriteDictionary(0, m_dict, typeof(K), typeof(V));
                }
                else
                {
                    writer.WriteDictionary(0, m_dict, typeof(K));
                }
            }

            public override int GetHashCode()
            {
                int              prime = 31;
                Dictionary<K, V> dict  = m_dict;
                return prime + ((dict == null) ? 0 : dict.GetHashCode());
            }

            public override bool Equals(object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null)
                {
                    return false;
                }

                IDictionary<K, V> thatDict = ((DictionaryTestClass<K, V>) o).m_dict;
                IDictionary<K, V> thisDict = m_dict;
                if (thisDict == thatDict)
                {
                    return true;
                }
                if (thisDict == null || thatDict == null)
                {
                    return false;
                }
                if (thisDict.Count == thatDict.Count)
                {
                    // System.Collections.Generic.Dictionary<int, string>.Equals 
                    // only does reference equality.  So we have to go through
                    // the map to compare each entry.
                    foreach (var key in thatDict.Keys)
                    {
                        if (!thisDict[key].Equals(thatDict[key]))
                        {
                            return false;
                        }
                    }
                    return true;
                }

                return false;
            }

            private Dictionary<K, V> m_dict;
            private bool             m_isUniformValue;
        }

        public class CollectionTestClass<T> : IPortableObject
        {
            public CollectionTestClass()
            { }

            public CollectionTestClass(Collection<T> coll)
            {
                m_coll = coll;
            }

            public void ReadExternal(IPofReader reader)
            {
                m_coll = (Collection<T>) reader.ReadCollection<T>(0, new Collection<T>());
            }

            public void WriteExternal(IPofWriter writer)
            {
                writer.WriteCollection(0, m_coll, typeof(T));
            }

            public override int GetHashCode()
            {
                int           prime = 31;
                Collection<T> coll  = m_coll;
                return prime + ((coll == null) ? 0 : coll.GetHashCode());
            }

            public override bool Equals(object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null)
                {
                    return false;
                }

                ICollection<T> thatColl = ((CollectionTestClass<T>) o).m_coll;
                ICollection<T> thisColl = this.m_coll;
                if (thisColl == thatColl)
                {
                    return true;
                }
                if (thisColl == null || thatColl == null)
                {
                    return false;
                }
                if (thisColl.Count == thatColl.Count)
                {
                    // System.Collections.ObjectModel.Collection<T>.Equals could do
                    // reference equality.  So to be safe, we have to go through
                    // the collection to compare each item.
                    foreach (var item in thatColl)
                    {
                        if (!thisColl.Contains(item))
                        {
                            return false;
                        }
                    }
                    return true;
                }

                return false;
            }

            private Collection<T> m_coll;
        }

        private ISerializer m_serializer;
    }
}
