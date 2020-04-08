/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NUnit.Framework;

namespace Tangosol.IO.Pof
{
    public class NestedType : IPortableObject
    {
        private static bool ArrayEqual(IList a1, IList a2)
        {
            if (a1.Count != a2.Count)
                return false;
            for (int i = 0; i < a1.Count; i++)
            {
                if (a1[i].ToString() != a2[i].ToString())
                    return false;
            }
            return true;
        }

        private const int INTEGER = 100;
        private const String STRING = "Hello World";
        private static String[] STRING_ARRAY = new[] { "one", "two", "three" };
        private static float[] FLOAT_ARRAY = new[] { 1.0f, 2.0f, 3.3f, 4.4f };
        private static IList<String> set = new List<String>
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
            float[] floatArray = nested2.ReadSingleArray(2);
            Assert.AreEqual(ArrayEqual(FLOAT_ARRAY, floatArray), true);

            IPofReader nested3 = nested2.CreateNestedPofReader(3);
            String[] stringArray = (String[])nested3.ReadArray(0,
                    new String[0]);
            Assert.IsTrue(ArrayEqual(stringArray, STRING_ARRAY));
            nested3.ReadRemainder();

            // close nested3 and continue to nested2
            bool boolVal = nested2.ReadBoolean(4);
            Assert.AreEqual(false, boolVal);

            // nested1
            ICollection col = nested1.ReadCollection(1, null);
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
            Assert.AreEqual(2.222, nested1.ReadDouble(10));

            nested1.ReadRemainder();

            Assert.AreEqual(4.444, reader.ReadDouble(2));
            Assert.AreEqual(15, reader.ReadInt32(3));
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32(0, INTEGER);

            IPofWriter nested1 = writer.CreateNestedPofWriter(1);

            IPofWriter nested2 = nested1.CreateNestedPofWriter(0);
            nested2.WriteString(0, STRING);
            nested2.WriteSingleArray(2, FLOAT_ARRAY);

            IPofWriter nested3 = nested2.CreateNestedPofWriter(3);
            nested3.WriteArray(0, STRING_ARRAY, typeof(String));

            nested2.WriteBoolean(4, false);
            nested2.WriteRemainder(null);

            IList list = (IList)set;
            nested1.WriteCollection(1, list);
            nested1.WriteDouble(2, 2.0);
            nested1.WriteInt32(3, 5);
            nested1.WriteCollection(4, set);
            nested1.WriteDouble(10, 2.222);

            writer.WriteDouble(2, 4.444);
            writer.WriteInt32(3, 15);
        }
    }

    public class SimpleType : IPortableObject
    {
        public void ReadExternal(IPofReader reader)
        {
            Assert.AreEqual(0, reader.ReadInt32(0));
            Assert.AreEqual(1, reader.ReadInt32(1));
            Assert.AreEqual(2, reader.ReadInt32(2));
            IPofReader reader2 = reader.CreateNestedPofReader(3);

            Assert.AreEqual(0, reader2.ReadInt32(0));
            Assert.AreEqual(1, reader2.ReadInt32(1));
            Assert.AreEqual(2, reader2.ReadInt32(2));
            Assert.AreEqual(4, reader.ReadInt32(4));
            Assert.AreEqual(5, reader.ReadInt32(5));
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32(0, 0);
            writer.WriteInt32(1, 1);
            writer.WriteInt32(2, 2);
            IPofWriter writer2 = writer.CreateNestedPofWriter(3);
            writer2.WriteInt32(0, 0);
            writer2.WriteInt32(1, 1);
            writer2.WriteInt32(2, 2);
            writer.WriteInt32(4, 4);
            writer.WriteInt32(5, 5);
        }
    }

    public class GenericCollectionsType : IPortableObject
    {
        private static bool ArrayEqual(IList a1, IList a2)
        {
            if (a1.Count != a2.Count)
                return false;
            for (int i = 0; i < a1.Count; i++)
            {
                if (a1[i].ToString() != a2[i].ToString())
                    return false;
            }
            return true;
        }

        private static ICollection<String> set = new List<String>
                                             {
                                                 "four",
                                                 "five",
                                                 "six",
                                                 "seven",
                                                 "eight"
                                             };
        private static IDictionary<int, String> map = new Dictionary<int, String>
                                             {
                                                 {4, "four"},
                                                 {5, "five"},
                                                 {6, "six"},
                                                 {7, "seven"},
                                                 {8, "eight"}
                                             };

        public void ReadExternal(IPofReader reader)
        {
            IList<String> newSet = (IList<String>)reader.ReadCollection(0, (IList<String>)null);
            Assert.IsTrue(ArrayEqual((List<String>)set, (List<String>)newSet));

            IDictionary<int, String> newMap = reader.ReadDictionary(1, (IDictionary<int, String>)null);
            Assert.IsTrue(newMap.ContainsKey(5));

            KeyValuePair<int, String> entry = new KeyValuePair<int, string>(8, "eight");
            Assert.IsTrue(newMap.Contains(entry));
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteCollection(0, set);
            writer.WriteDictionary(1, map);
        }
    }

    public class Balance
    {
        private double m_balance;
        Customer m_customer;

        public void setBalance(double bal)
        {
            m_balance = bal;
        }

        public double getBalance()
        {
            return m_balance;
        }

        public void setCustomer(Customer c)
        {
            m_customer = c;
        }

        public Customer getCustomer()
        {
            return m_customer;
        }
    }

    public class Customer
    {
        String m_name;
        Balance m_balance;
        Product m_product;

        public Customer(String name)
        {
            m_name = name;
        }

        public Customer(String name, Product prod, Balance bal)
        {
            m_name = name;
            m_product = prod;
            m_balance = bal;
        }

        public String getName()
        {
            return m_name;
        }

        public Product getProduct()
        {
            return m_product;
        }

        public void setProduct(Product prod)
        {
            m_product = prod;
        }

        public Balance getBalance()
        {
            return m_balance;
        }

        public void setBalance(Balance bal)
        {
            m_balance = bal;
        }
    }

    public class Product
    {
        private Balance m_balance;

        public Product(Balance bal)
        {
            m_balance = bal;
        }

        public Balance getBalance()
        {
            return m_balance;
        }
    }
}