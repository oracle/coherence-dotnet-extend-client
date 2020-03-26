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
    public class PofHelperTypeConversionTests
    {
        [Test]
        public void TestDotNetTypeID()
        {
            SimplePofContext ctx = new SimplePofContext();

            object o = new byte[0];
            Assert.AreEqual(PofConstants.N_BYTE_ARRAY, PofHelper.GetDotNetTypeId(o, ctx));

            o = new object[0];
            Assert.AreEqual(PofConstants.N_OBJECT_ARRAY, PofHelper.GetDotNetTypeId(o, ctx));

            o = new string[0];
            Assert.AreEqual(PofConstants.N_OBJECT_ARRAY, PofHelper.GetDotNetTypeId(o, ctx));

            ctx.RegisterUserType(1000, o.GetType(), new PortableObjectSerializer(1000));
            o = new object[0];
            Assert.AreEqual(PofConstants.N_OBJECT_ARRAY, PofHelper.GetDotNetTypeId(o, ctx));

            o = new string[0];
            Assert.AreEqual(PofConstants.N_USER_TYPE, PofHelper.GetDotNetTypeId(o, ctx));
        }

        [Test]
        public void TestPofTypeID()
        {
            Assert.AreEqual(PofConstants.T_BOOLEAN, PofHelper.GetPofTypeId(true.GetType(), new SimplePofContext()));
            Assert.AreEqual(PofConstants.T_CHAR, PofHelper.GetPofTypeId('t'.GetType(), new SimplePofContext()));
            Assert.AreEqual(PofConstants.T_INT16,
                            PofHelper.GetPofTypeId(Int16.MinValue.GetType(), new SimplePofContext()));
            Assert.AreEqual(PofConstants.T_INT32, PofHelper.GetPofTypeId((-1).GetType(), new SimplePofContext()));
            Assert.AreEqual(PofConstants.T_INT64,
                            PofHelper.GetPofTypeId(Int64.MaxValue.GetType(), new SimplePofContext()));
            Assert.AreEqual(PofConstants.T_DATETIME,
                            PofHelper.GetPofTypeId(new DateTime(11, 11, 11, 11, 11, 11).GetType(),
                                                   new SimplePofContext()));
            Assert.AreEqual(PofConstants.T_CHAR_STRING, PofHelper.GetPofTypeId("test".GetType(), new SimplePofContext()));

            double[] uniformArray = new double[] {Double.MaxValue, 0, -1, Double.NegativeInfinity};
            Assert.AreEqual(PofConstants.T_UNIFORM_ARRAY,
                            PofHelper.GetPofTypeId(uniformArray.GetType(), new SimplePofContext()));

            Object[] objArray = new object[] {new DateTime(11, 11, 11), 13, Double.NaN};
            Assert.AreEqual(PofConstants.T_ARRAY, PofHelper.GetPofTypeId(objArray.GetType(), new SimplePofContext()));

            ArrayList al = new ArrayList();
            al.Add(new DateTime(11, 11, 11));
            al.Add(5.55);
            Assert.AreEqual(PofConstants.T_COLLECTION, PofHelper.GetPofTypeId(al.GetType(), new SimplePofContext()));

            Hashtable ht = new Hashtable();
            ht.Add("now", new DateTime(2006, 8, 11, 12, 49, 0));
            Assert.AreEqual(PofConstants.T_MAP, PofHelper.GetPofTypeId(ht.GetType(), new SimplePofContext()));

            ICollection ll = new LinkedList<double>();
            Assert.AreEqual(PofConstants.T_COLLECTION, PofHelper.GetPofTypeId(ll.GetType(), new SimplePofContext()));

            Binary bin = new Binary(new byte[] { 1, 2, 3 });
            Assert.AreEqual(PofConstants.T_OCTET_STRING, PofHelper.GetPofTypeId(bin.GetType(), new SimplePofContext()));

            SimplePofContext ctx = new SimplePofContext();

            Assert.AreEqual(PofConstants.T_OCTET_STRING, PofHelper.GetPofTypeId(typeof(byte[]), ctx));
            Assert.AreEqual(PofConstants.T_ARRAY, PofHelper.GetPofTypeId(typeof(object[]), ctx));
            Assert.AreEqual(PofConstants.T_ARRAY, PofHelper.GetPofTypeId(typeof(string[]), ctx));

            ctx.RegisterUserType(1000, typeof(string[]), new PortableObjectSerializer(1000));

            Assert.AreEqual(PofConstants.T_OCTET_STRING, PofHelper.GetPofTypeId(typeof(byte[]), ctx));
            Assert.AreEqual(PofConstants.T_ARRAY, PofHelper.GetPofTypeId(typeof(object[]), ctx));
            Assert.AreEqual(1000, PofHelper.GetPofTypeId(typeof(string[]), ctx));
        }

        [Test]
        public void TestConvertNumber()
        {
            Assert.AreEqual(null, PofHelper.ConvertNumber(null, PofConstants.N_BYTE));
            Assert.AreEqual(111, PofHelper.ConvertNumber(111, PofConstants.N_BYTE));
            Assert.AreEqual(6, PofHelper.ConvertNumber(6.0001, PofConstants.N_INT16));
            Assert.AreEqual(11, PofHelper.ConvertNumber(11, PofConstants.N_INT32));
            Assert.AreEqual(Int64.MaxValue, PofHelper.ConvertNumber(Int64.MaxValue, PofConstants.N_INT64));
            Assert.AreEqual((Double) Int64.MaxValue, PofHelper.ConvertNumber(Int64.MaxValue, PofConstants.N_DOUBLE));
            Assert.AreEqual(-0.1F, PofHelper.ConvertNumber(-0.1, PofConstants.N_SINGLE));
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestConvertNumberWithException()
        {
            PofHelper.ConvertNumber(1, 100);
        }
        [Test]
        public void TestResizeArray()
        {
            Object[] objArray = new object[] {new DateTime(1999, 1, 1, 12, 0, 0, 100), Int64.MinValue, -0.1F};
            Assert.AreEqual(objArray.Length + 10, PofHelper.ResizeArray(objArray, objArray.Length + 10).Length);
            Assert.AreEqual(objArray.Length, PofHelper.ResizeArray(objArray, objArray.Length).Length);
//            Assert.AreEqual(objArray.Length - 2, PofHelper.ResizeArray(objArray, objArray.Length - 2).Length);
            Assert.AreEqual(5, PofHelper.ResizeArray(null, 5).Length);
        }

        [Test]
        public void TestGetDotNetTypeId()
        {
            MemoryStream stream = new MemoryStream();
            DataWriter writer = new DataWriter(stream);
            PofStreamWriter pofWriter = new PofStreamWriter(writer, new SimplePofContext());

            Int32[][] multiarray = new Int32[][] {new int[]{1, 2, 3, 4, 5, 6},
                                                  new int[]{100, 101, 102}};

            object[][][] objarray = new object[][][]
                {
                    new object[][]
                        {
                            new object[] {1, 2, 3}, new object[] {"one", "two", "three"}
                        },
                    new object[][]
                        {
                            new object[] {11.11, 22.22}, new object[] {true, false, DateTime.UtcNow}
                        }
                };

            byte[] arr = new byte[] {1, 2, 3};
            Binary bin = new Binary(arr);

            pofWriter.WriteObject(0, multiarray);
            pofWriter.WriteObject(0, objarray);
            pofWriter.WriteObject(0, bin);

            stream.Position = 0;
            DataReader reader = new DataReader(stream);
            PofStreamReader pofReader = new PofStreamReader(reader, new SimplePofContext());

            object[] result = (object[])pofReader.ReadObject(0);

            Assert.IsInstanceOf(typeof(int[]), result[0]);
            Assert.IsInstanceOf(typeof(int[]), result[1]);

            int[][] arrayresult = new int[][]{(int[])result[0], (int[]) result[1]};

            Assert.AreEqual(multiarray[0].Length, arrayresult[0].Length);
            Assert.AreEqual(multiarray[1].Length, arrayresult[1].Length);

            for(int i = 0; i<multiarray.Length; i++)
            {
                for(int j=0; j<multiarray[i].Length; j++)
                {
                    Assert.AreEqual(multiarray[i][j], arrayresult[i][j]);
                }
            }

            result =(object[]) pofReader.ReadObject(0);
            Assert.IsInstanceOf(typeof(object[]), result[0]);
            Assert.IsInstanceOf(typeof(object[]), result[1]);
            object[] result0 = (object[]) result[0];
            object[] result1 = (object[]) result[1];
            Assert.IsInstanceOf(typeof(object[]), result0[0]);
            Assert.IsInstanceOf(typeof(object[]), result1[0]);
            Assert.IsInstanceOf(typeof(object[]), result0[1]);
            Assert.IsInstanceOf(typeof(object[]), result1[1]);

            object o = pofReader.ReadObject(0);
            Assert.IsInstanceOf(typeof (Binary), o);
        }
    }
}