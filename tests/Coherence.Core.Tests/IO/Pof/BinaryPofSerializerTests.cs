/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Data;
using System.IO;

using NUnit.Framework;

using Tangosol.Util.Collections;
using Tangosol.Util.Filter;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class BinaryPofSerializerTests
    {
        [Test]
        public void TestSynchronizedDictionarySerialization()
        {
            SynchronizedDictionary ht = new SynchronizedDictionary();

            SimplePofContext ctx = new SimplePofContext();
            BinaryPofSerializer serializer = new BinaryPofSerializer(1);

            ctx.RegisterUserType(1, ht.GetType(), serializer);

            Assert.AreEqual(1, ctx.GetUserTypeIdentifier(ht));
            Assert.AreEqual(1, ctx.GetUserTypeIdentifier(ht.GetType()));

            Assert.AreEqual(ht.GetType(), ctx.GetType(1));
            Assert.AreEqual(ht.GetType().FullName, ctx.GetTypeName(1));
            Assert.AreEqual(serializer, ctx.GetPofSerializer(1));

            ht.Add(1, 1);
            ht.Add(2, 2);
            ht.Add(3, 3);
            ht.Add(4, 4);
            ht.Add(5, 5);
            ht.Add(6, 6);
            ht.Add(7, 7);

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), ht);

            stream.Position = 0;
            SynchronizedDictionary ht2 = (SynchronizedDictionary)ctx.Deserialize(new DataReader(stream));

            Assert.AreEqual(ht[1], ht2[1]);
            Assert.AreEqual(ht[2], ht2[2]);
            Assert.AreEqual(ht[3], ht2[3]);
            Assert.AreEqual(ht[4], ht2[4]);
            Assert.AreEqual(ht[5], ht2[5]);
            Assert.AreEqual(ht[6], ht2[6]);
            Assert.AreEqual(ht[7], ht2[7]);
        }

        [Test]
        public void TestDataSetSerialization()
        {
            DataSet dataSet1 = new DataSet();

            SimplePofContext ctx = new SimplePofContext();
            BinaryPofSerializer serializer = new BinaryPofSerializer(1);

            ctx.RegisterUserType(1, dataSet1.GetType(), serializer);

            Assert.AreEqual(1, ctx.GetUserTypeIdentifier(dataSet1));
            Assert.AreEqual(1, ctx.GetUserTypeIdentifier(dataSet1.GetType()));

            Assert.AreEqual(dataSet1.GetType(), ctx.GetType(1));
            Assert.AreEqual(dataSet1.GetType().FullName, ctx.GetTypeName(1));
            Assert.AreEqual(serializer, ctx.GetPofSerializer(1));

            dataSet1.Tables.Add("Order");
            dataSet1.Tables["Order"].Columns.Add("Id"  , typeof(String));
            dataSet1.Tables["Order"].Columns.Add("Name", typeof(String));

            dataSet1.Tables.Add("OrderDetail");
            dataSet1.Tables["OrderDetail"].Columns.Add("Id", typeof(String));
            dataSet1.Tables["OrderDetail"].Columns.Add("Sum", typeof(Double));
            dataSet1.Tables["OrderDetail"].Columns.Add("OrderId", typeof(String));

            DataRelation relation = new DataRelation("Order_OrderDetail", dataSet1.Tables["Order"].Columns["Id"], dataSet1.Tables["OrderDetail"].Columns["OrderId"]);

            dataSet1.Relations.Add(relation);

            dataSet1.Tables["Order"].Rows.Add(new object[] { "01/2007", "Comedy books" });
            dataSet1.Tables["Order"].Rows.Add(new object[] { "02/2007", "Suite" });

            dataSet1.Tables["OrderDetail"].Rows.Add(new object[] { "01/2007-01", 1500, "01/2007" });
            dataSet1.Tables["OrderDetail"].Rows.Add(new object[] { "01/2007-02", 1350, "01/2007" });
            dataSet1.Tables["OrderDetail"].Rows.Add(new object[] { "02/2007-01", 5500.50, "02/2007" });

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), dataSet1);

            stream.Position = 0;
            DataSet dataSet2 = (DataSet)ctx.Deserialize(new DataReader(stream));

            Assert.AreEqual(dataSet1.Tables["Order"].Columns.Count, dataSet2.Tables["Order"].Columns.Count);
            Assert.AreEqual(dataSet1.Tables["Order"].Columns["Id"].DataType  , dataSet2.Tables["Order"].Columns["Id"].DataType);
            Assert.AreEqual(dataSet1.Tables["Order"].Columns["Name"].DataType, dataSet2.Tables["Order"].Columns["Name"].DataType);

            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Columns.Count, dataSet2.Tables["OrderDetail"].Columns.Count);
            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Columns["Id"].DataType     , dataSet2.Tables["OrderDetail"].Columns["Id"].DataType);
            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Columns["Sum"].DataType    , dataSet2.Tables["OrderDetail"].Columns["Sum"].DataType);
            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Columns["OrderId"].DataType, dataSet2.Tables["OrderDetail"].Columns["OrderId"].DataType);

            Assert.AreEqual(dataSet1.Tables["Order"].Rows.Count, dataSet2.Tables["Order"].Rows.Count);
            Assert.AreEqual(dataSet1.Tables["Order"].Rows[0]["Id"]  , dataSet2.Tables["Order"].Rows[0]["Id"]);
            Assert.AreEqual(dataSet1.Tables["Order"].Rows[0]["Name"], dataSet2.Tables["Order"].Rows[0]["Name"]);
            Assert.AreEqual(dataSet1.Tables["Order"].Rows[1]["Id"]  , dataSet2.Tables["Order"].Rows[1]["Id"]);
            Assert.AreEqual(dataSet1.Tables["Order"].Rows[1]["Name"], dataSet2.Tables["Order"].Rows[1]["Name"]);

            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Rows.Count, dataSet2.Tables["OrderDetail"].Rows.Count);
            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Rows[0]["Id"]     , dataSet2.Tables["OrderDetail"].Rows[0]["Id"]);
            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Rows[0]["Sum"]    , dataSet2.Tables["OrderDetail"].Rows[0]["Sum"]);
            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Rows[0]["OrderId"], dataSet2.Tables["OrderDetail"].Rows[0]["OrderId"]);

            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Rows[1]["Id"]     , dataSet2.Tables["OrderDetail"].Rows[1]["Id"]);
            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Rows[1]["Sum"]    , dataSet2.Tables["OrderDetail"].Rows[1]["Sum"]);
            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Rows[1]["OrderId"], dataSet2.Tables["OrderDetail"].Rows[1]["OrderId"]);

            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Rows[2]["Id"]     , dataSet2.Tables["OrderDetail"].Rows[2]["Id"]);
            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Rows[2]["Sum"]    , dataSet2.Tables["OrderDetail"].Rows[2]["Sum"]);
            Assert.AreEqual(dataSet1.Tables["OrderDetail"].Rows[2]["OrderId"], dataSet2.Tables["OrderDetail"].Rows[2]["OrderId"]);

            ctx.UnregisterUserType(1);
        }

        [Test]
        public void TestConfigPofContextException()
        {
            LikeFilter filter = new LikeFilter("field", "goran", '\\', false);

            SimplePofContext ctx = new SimplePofContext();
            BinaryPofSerializer serializer = new BinaryPofSerializer(1);

            ctx.RegisterUserType(1, filter.GetType(), serializer);

            Stream stream = new MemoryStream();
            Assert.That(() => ctx.Serialize(new DataWriter(stream), filter), Throws.TypeOf<IOException>());
        }
    }
}