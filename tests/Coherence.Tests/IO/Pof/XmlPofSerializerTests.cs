/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Data;
using System.IO;

using NUnit.Framework;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class XmlPofSerializerTests
    {
        protected DataSet CreateTestDataSet()
        {
            DataSet hq = new DataSet("HeadQuarter");

            // master table
            DataTable projects = new DataTable("Projects");
            DataColumn column1 = new DataColumn("Id", typeof(int));
            DataColumn column2 = new DataColumn("ProjectName", typeof(string));
            DataColumn column3 = new DataColumn("StartDate", typeof(DateTime));
            DataColumn column4 = new DataColumn("ClientId", typeof(int));
            projects.Columns.Add(column1);
            projects.Columns.Add(column2);
            projects.Columns.Add(column3);
            projects.Columns.Add(column4);
            projects.PrimaryKey = new DataColumn[] { column1 };
            hq.Tables.Add(projects);
            DataRow row;
            for(int i=0; i<10; i++)
            {
                row = projects.NewRow();
                row[0] = i;
                row[1] = "Project" + i;
                row[2] = DateTime.Now;
                row[3] = i;
                projects.Rows.Add(row);
            }


            // detail table
            DataTable clients = new DataTable("Clients");
            column1 = new DataColumn("Id", typeof(int));
            column2 = new DataColumn("ClientName", typeof(string));
            column3 = new DataColumn("SomeInformation", typeof(double));

            clients.Columns.Add(column1);
            clients.Columns.Add(column2);
            clients.Columns.Add(column3);
            hq.Tables.Add(clients);
            for (int i = 0; i < 10; i++)
            {
                row = clients.NewRow();
                row[0] = i;
                row[1] = "Client" + i;
                row[2] = new Random().NextDouble();
                clients.Rows.Add(row);
            }
            // relations
            DataRelation relation = new DataRelation("projectclient", projects.Columns["ClientId"], clients.Columns["Id"]);
            hq.Relations.Add(relation);

            return hq;
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
        public void TestXmlPofSerializer()
        {
            MemoryStream stream = new MemoryStream();
            DataWriter writer = new DataWriter(stream);
            XmlPofSerializer xmlSerializer = new XmlPofSerializer(1);
            SimplePofContext context = new SimplePofContext();
            context.RegisterUserType(1, typeof(DataSet), xmlSerializer);

            // create test DataSet
            DataSet set = CreateTestDataSet();
            // serialize DataSet
            context.Serialize(writer, set);

            // deserialize DataSet
            stream.Seek(0, SeekOrigin.Begin);
            DataReader reader = new DataReader(stream);
            object deserObj = context.Deserialize(reader);
            Assert.IsInstanceOf(typeof(DataSet), deserObj);
            DataSet deserDS = (DataSet) deserObj;

            // Assert tables
            Assert.AreEqual(set.Tables.Count, deserDS.Tables.Count);
            for(int i=0; i<set.Tables.Count; i++)
            {
                DataTable table = set.Tables[i];
                DataTable deserTable = deserDS.Tables[i];
                Assert.AreEqual(table.TableName, deserTable.TableName);
                Assert.AreEqual(table.Columns.Count, deserTable.Columns.Count);

                for(int j=0, c=table.Columns.Count; j<c; j++)
                {
                    DataColumn column1 = table.Columns[j];
                    DataColumn column2 = deserTable.Columns[j];
                    Assert.AreEqual(column1.ColumnName, column2.ColumnName);
                    Assert.AreEqual(column1.DataType, column2.DataType);
                    Assert.AreEqual(column1.Unique, column2.Unique);
                }

                Assert.AreEqual(table.Rows.Count, deserTable.Rows.Count);
                for (int k = 0, r = table.Rows.Count; k < r; k++)
                {
                    DataRow row1 = table.Rows[k];
                    DataRow row2 = deserTable.Rows[k];
                    for (int m = 0, c=table.Columns.Count; m < c; m++)
                    {
                        Assert.AreEqual(row1[m], row2[m]);
                    }
                }
            }

            // Assert relations
            Assert.AreEqual(set.Relations.Count, deserDS.Relations.Count);
            for(int i=0; i<set.Relations.Count; i++)
            {
                DataColumn[] parentColumns1 = set.Relations[i].ParentColumns;
                DataColumn[] parentColumns2 = deserDS.Relations[i].ParentColumns;
                Assert.AreEqual(parentColumns1.Length, parentColumns2.Length);
                for (int j = 0; j < parentColumns1.Length; j++)
                {
                    Assert.AreEqual(parentColumns1[j].ColumnName, parentColumns2[j].ColumnName);
                }

                DataColumn[] childColumns1 = set.Relations[i].ChildColumns;
                DataColumn[] childColumns2 = deserDS.Relations[i].ChildColumns;
                Assert.AreEqual(childColumns1.Length, childColumns2.Length);
                for (int j = 0; j < childColumns1.Length; j++)
                {
                    Assert.AreEqual(childColumns1[j].ColumnName, childColumns2[j].ColumnName);
                }
            }
        }
    }
}
