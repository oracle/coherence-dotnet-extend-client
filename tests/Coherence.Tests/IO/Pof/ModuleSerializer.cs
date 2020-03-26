/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// Custom serializer for SerializerTest.Balance, SerializerTest.Customer,
    /// and SerializerTest.Product used by SerializerTest.
    /// </summary>
    /// <author>lh  2011.06.10</author>
    public class ModuleSerializer
    {
        ModuleSerializer()
        {}

        #region Inner class: BalanceSerializer

        public class BalanceSerializer : IPofSerializer
        {
            public void Serialize(IPofWriter pofWriter, object o)
            {
                var bal = (Balance) o;
                pofWriter.WriteDouble(0, bal.getBalance());
                pofWriter.WriteObject(1, bal.getCustomer());
                pofWriter.WriteRemainder(null);
            }

            public object Deserialize(IPofReader pofReader)
            {
                var bal = new Balance();
                pofReader.RegisterIdentity(bal);
                bal.setBalance(pofReader.ReadDouble(0));
                bal.setCustomer((Customer) pofReader.ReadObject(1));
                pofReader.ReadRemainder();
                return bal;
            }
        }

        #endregion 

        #region Inner class: ProductSerializer

        public class ProductSerializer : IPofSerializer
        {
            public void Serialize(IPofWriter pofWriter, object o)
            {
                var p = (Product) o;
                pofWriter.WriteObject(0, p.getBalance());
                pofWriter.WriteRemainder(null);
            }

            public object Deserialize(IPofReader pofReader)
            {
                var bal = (Balance) pofReader.ReadObject(0);
                pofReader.ReadRemainder();
                return new Product(bal);
            }
        }

        #endregion

        #region Inner class: CustomerSerializer

        public class CustomerSerializer : IPofSerializer
        {
            public void Serialize(IPofWriter pofWriter, object o)
            {
                var c = (Customer) o;
                pofWriter.WriteString(0, c.getName());
                pofWriter.WriteObject(1, c.getProduct());
                pofWriter.WriteObject(2, c.getBalance());
                pofWriter.WriteRemainder(null);
            }

            public object Deserialize(IPofReader pofReader)
            {
                String name = pofReader.ReadString(0);
                var    c    = new Customer(name);

                pofReader.RegisterIdentity(c);
                c.setProduct((Product) pofReader.ReadObject(1));
                c.setBalance((Balance) pofReader.ReadObject(2));
                pofReader.ReadRemainder();
                return c;
            }
        }

        #endregion
    }
}