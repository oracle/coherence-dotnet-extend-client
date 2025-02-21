/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using NUnit.Framework;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// Unit tests for getting POF user types ids.
    /// Issue reported in COH-9428
    /// </summary>
    /// <author> par 2013.4.08</author>
    [TestFixture]
    public class PofUserTypeTest
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

        /// <summary>
        /// Test getting user type from context.
        /// </summary>
        [Test]
        public void TestIdFromContext()
        {
            SimplePofContext context = new SimplePofContext();
            context.RegisterUserType(9999, typeof(MyCollection), new MySerializer());
            Assert.AreEqual(9999, context.GetUserTypeIdentifier(MyCollection.Instance.GetType()));
        }

        /// <summary>
        /// Test getting user type from PofHelper.
        /// </summary>
        [Test]
        public void TestIdFromPofHelper()
        {
            SimplePofContext context = new SimplePofContext();
            context.RegisterUserType(9999, typeof(MyCollection), new MySerializer());
            Assert.AreEqual(9999, PofHelper.GetPofTypeId(MyCollection.Instance.GetType(), context));
        }

        /// <summary>
        /// Test class for registering user type.
        /// </summary>
        public class MySerializer : IPofSerializer
        {

            /// <summary>
            /// Method to deserialize object, not called.
            /// <param name="reader">
            /// Reader used to get object.
            /// </param>
            /// <exception cref="NotSupportedException">
            /// Exception always thrown, method is not called
            /// </exception>
            public Object Deserialize(IPofReader reader)
            {
                throw new NotSupportedException("Not implemented");
            }

            /// <summary>
            /// Method to serialize object, not called.
            /// <param name="writer">
            /// Writer used to output object.
            /// </param>
            /// <param name="obj">
            /// The object which is written.
            /// </param>
            /// <exception cref="NotSupportedException">
            /// Exception always thrown, method is not called
            /// </exception>
            public void Serialize(IPofWriter writer, Object obj)
            {
                throw new NotSupportedException("Not implemented");
            }
        }

        /// <summary>
        /// Test class to return user type id rather than ArrayList type id
        /// </summary>
        public class MyCollection : ArrayList
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            public MyCollection() {}

            /// <summary>
            /// Singleton instance of class
            /// </summary>
            public static MyCollection Instance
            {
                get
                {
                    if (instance == null)
                    {
                       instance = new MyCollection();
                    }
                    return instance;
                }
            }

            /// <summary>
            /// Singleton instance of class
            /// </summary>
            private static MyCollection instance;
        }
    }
}
