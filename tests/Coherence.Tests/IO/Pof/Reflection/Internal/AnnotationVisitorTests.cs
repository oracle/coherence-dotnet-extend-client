/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Tangosol.IO.Pof.Annotation;

namespace Tangosol.IO.Pof.Reflection.Internal
{
    [TestFixture]
    public class AnnotationVisitorTests
    {
        [Test]
        public void TestVisit()
        {
            TypeMetadataBuilder<Poffed>           builder = new TypeMetadataBuilder<Poffed>();
            IVisitor<TypeMetadataBuilder<Poffed>> visitor = new AnnotationVisitor<TypeMetadataBuilder<Poffed>,Poffed>();

            builder.SetTypeId(1001);
            builder.Accept(visitor, typeof(Poffed));

            ITypeMetadata<Poffed> tmd = builder.Build();

            Assert.AreEqual(1001, tmd.Key.TypeId);
            Assert.AreEqual(0, tmd.GetAttribute("lastName").Index);
            Assert.AreEqual(1, tmd.GetAttribute("age").Index);
            Assert.AreEqual(2, tmd.GetAttribute("firstName").Index);
        }

        [Test]
        public void TestImplyIndicies()
        {
            TypeMetadataBuilder<PoffedImpliedIndicies> builder = new TypeMetadataBuilder<PoffedImpliedIndicies>();
            IVisitor<TypeMetadataBuilder<PoffedImpliedIndicies>> visitor = new AnnotationVisitor<TypeMetadataBuilder<PoffedImpliedIndicies>, PoffedImpliedIndicies>(true);

            builder.SetTypeId(1002);
            builder.Accept(visitor, typeof(PoffedImpliedIndicies));

            ITypeMetadata<PoffedImpliedIndicies> tmd = builder.Build();

            Assert.AreEqual(1002, tmd.Key.TypeId);
            Assert.AreEqual(2, tmd.GetAttribute("lastName").Index);
            Assert.AreEqual(1, tmd.GetAttribute("firstName").Index);
            Assert.AreEqual(0, tmd.GetAttribute("age").Index);
        }

        [Test]
        public void TestClashingIndicies()
        {
            TypeMetadataBuilder<PoffedClashingIndicies> builder = new TypeMetadataBuilder<PoffedClashingIndicies>();
            IVisitor<TypeMetadataBuilder<PoffedClashingIndicies>> visitor = new AnnotationVisitor<TypeMetadataBuilder<PoffedClashingIndicies>, PoffedClashingIndicies>(true);

            builder.SetTypeId(1003);
            builder.Accept(visitor, typeof(PoffedClashingIndicies));

            ITypeMetadata<PoffedClashingIndicies> tmd = builder.Build();

            Assert.AreEqual(1003, tmd.Key.TypeId);
            Assert.AreEqual(3, tmd.GetAttribute("lastName").Index);
            Assert.AreEqual(1, tmd.GetAttribute("age").Index);
            Assert.AreEqual(2, tmd.GetAttribute("firstName").Index);
            Assert.AreEqual(0, tmd.GetAttribute("age2").Index);
        }

        [Test]
        public void TestCustomCodec()
        {
            TypeMetadataBuilder<PoffedCustomCodec> builder = new TypeMetadataBuilder<PoffedCustomCodec>();
            IVisitor<TypeMetadataBuilder<PoffedCustomCodec>> visitor = new AnnotationVisitor<TypeMetadataBuilder<PoffedCustomCodec>, PoffedCustomCodec>(true);

            builder.SetTypeId(1004);
            builder.Accept(visitor, typeof(PoffedCustomCodec));

            ITypeMetadata<PoffedCustomCodec> tmd = builder.Build();

            Assert.AreEqual(1004, tmd.Key.TypeId);
            Assert.AreEqual(0, tmd.GetAttribute("age").Index);
            Assert.AreEqual(1, tmd.GetAttribute("aliases").Index);
            Assert.AreEqual(2, tmd.GetAttribute("firstName").Index);
            Assert.AreEqual(3, tmd.GetAttribute("lastName").Index);
            Assert.IsInstanceOf(typeof(LinkedListCodec<string>), tmd.GetAttribute("aliases").Codec);
        }

        [Test]
        public void TestAccessorAnnotations()
        {
            TypeMetadataBuilder<PoffedMethodInspection> builder = new TypeMetadataBuilder<PoffedMethodInspection>();
            IVisitor<TypeMetadataBuilder<PoffedMethodInspection>> visitor = new AnnotationVisitor<TypeMetadataBuilder<PoffedMethodInspection>, PoffedMethodInspection>(true);

            builder.SetTypeId(1005);
            builder.Accept(visitor, typeof(PoffedMethodInspection));

            ITypeMetadata<PoffedMethodInspection> tmd = builder.Build();

            Assert.AreEqual(1005, tmd.Key.TypeId);
            Assert.AreEqual(0, tmd.GetAttribute("adult").Index);
            Assert.AreEqual(1, tmd.GetAttribute("age").Index);
            Assert.AreEqual(2, tmd.GetAttribute("firstName").Index);
            Assert.AreEqual(3, tmd.GetAttribute("lastName").Index);
        }

        [Test]
        public void TestPropertyAnnotations()
        {
            TypeMetadataBuilder<PoffedPropertyInspection> builder = new TypeMetadataBuilder<PoffedPropertyInspection>();
            IVisitor<TypeMetadataBuilder<PoffedPropertyInspection>> visitor = new AnnotationVisitor<TypeMetadataBuilder<PoffedPropertyInspection>, PoffedPropertyInspection>(true);

            builder.SetTypeId(1007);
            builder.Accept(visitor, typeof(PoffedPropertyInspection));

            ITypeMetadata<PoffedPropertyInspection> tmd = builder.Build();

            Assert.AreEqual(1007, tmd.Key.TypeId);
            Assert.AreEqual(0, tmd.GetAttribute("adult").Index);
            Assert.AreEqual(1, tmd.GetAttribute("age").Index);
            Assert.AreEqual(2, tmd.GetAttribute("firstName").Index);
            Assert.AreEqual(3, tmd.GetAttribute("lastName").Index);

        }

        [Test]
        public void TestHybridAnnotations()
        {
            TypeMetadataBuilder<PoffedHybridInspection> builder = new TypeMetadataBuilder<PoffedHybridInspection>();
            IVisitor<TypeMetadataBuilder<PoffedHybridInspection>> visitor = new AnnotationVisitor<TypeMetadataBuilder<PoffedHybridInspection>, PoffedHybridInspection>(true);

            builder.SetTypeId(1006);
            builder.Accept(visitor, typeof(PoffedHybridInspection));

            ITypeMetadata<PoffedHybridInspection> tmd = builder.Build();

            Assert.AreEqual(1006, tmd.Key.TypeId);
            Assert.AreEqual(0, tmd.GetAttribute("adult").Index);
            Assert.AreEqual(1, tmd.GetAttribute("age").Index);
            Assert.AreEqual(2, tmd.GetAttribute("firstName").Index);
            Assert.AreEqual(3, tmd.GetAttribute("lastName").Index);
        }
    }

    [Portable]
    public class Poffed
    {
        public Poffed()
        {
        }

        public Poffed(string firstName, string lastName, int age, bool fAdult)
        {
            m_firstName = firstName;
            m_lastName = lastName;
            m_age = age;
            m_fAdult = fAdult;
        }

        public bool IsAdult()
        {
            return m_fAdult;
        }

        public void SetAdult(bool fAdult)
        {
            m_fAdult = fAdult;
        }

        [PortableProperty(2)]
        protected string m_firstName;
        [PortableProperty(0)]
        protected string m_lastName;
        [PortableProperty(1)]
        protected int m_age;
        [PortableProperty(3)]
        protected bool m_fAdult;
    }

    [Portable]
    public class PoffedImpliedIndicies
    {
        public PoffedImpliedIndicies()
        {
        }

        [PortableProperty(1)]
        protected string m_firstName;
        [PortableProperty]
        protected string m_lastName;
        [PortableProperty]
        protected int m_age;
    }

    [Portable]
    public class PoffedClashingIndicies
    {
        public PoffedClashingIndicies()
        {
        }

        [PortableProperty(1)]
        protected string m_firstName;
        [PortableProperty]
        protected string m_lastName;
        [PortableProperty(1)]
        protected int m_age;
        [PortableProperty]
        protected int m_age2;
    }

    [Portable]
    public class PoffedCustomCodec
    {
        public PoffedCustomCodec()
        {
        }

        [PortableProperty]
        protected string m_firstName;
        [PortableProperty]
        protected string m_lastName;
        [PortableProperty]
        protected int m_age;
        [PortableProperty(typeof(LinkedListCodec<string>))]
        protected IList<string> m_aliases;
    }

    [Portable]
    public class PoffedMethodInspection
    {
        public PoffedMethodInspection()
        {
        }

        [PortableProperty]
        public string GetFirstName()
        {
            return m_firstName;
        }

        public void SetFirstName(string firstName)
        {
            m_firstName = firstName;
        }

        [PortableProperty]
        public string GetLastName()
        {
            return m_lastName;
        }

        [PortableProperty]
        public void SetLastName(string lastName)
        {
            m_lastName = lastName;
        }

        public int GetAge()
        {
            return m_age;
        }

        [PortableProperty]
        public void SetAge(int age)
        {
            m_age = age;
        }

        [PortableProperty]
        public bool IsAdult()
        {
            return m_fAdult;
        }

        public void SetAdult(bool fAdult)
        {
            m_fAdult = fAdult;
        }

        private string m_firstName;
        private string m_lastName;
        private int m_age;
        private bool m_fAdult;
    }


    [Portable]
    public class PoffedPropertyInspection
    {
        public PoffedPropertyInspection()
        {
        }

        [PortableProperty]
        public string FirstName
        {
            get; set;
        }

        [PortableProperty]
        public string LastName
        {
            get; set;
        }

        [PortableProperty]
        public int Age
        {
            get; set;
        }

        [PortableProperty]
        public bool Adult
        {
            get; set;
        }
    }

    [Portable]
    public class PoffedHybridInspection
    {
        public PoffedHybridInspection()
        {
        }

        [PortableProperty]
        public string GetFirstName()
        {
            return m_firstName;
        }

        public void SetFirstName(string firstName)
        {
            m_firstName = firstName;
        }

        public string GetLastName()
        {
            return m_lastName;
        }

        [PortableProperty]
        public void SetLastName(string lastName)
        {
            m_lastName = lastName;
        }

        public int GetAge()
        {
            return m_age;
        }

        [PortableProperty]
        public void SetAge(int age)
        {
            m_age = age;
        }

        [PortableProperty]
        public bool Adult
        {
            get; set;
        }

        private string m_firstName;
        [PortableProperty]
        private string m_lastName;
        private int m_age;
    }

    public class LinkedListCodec<T> : ICodec
    {
        public static long COUNTER = 0L;

        public object Decode(IPofReader reader, int index)
        {
            COUNTER++;
            return reader.ReadCollection(index, (ICollection)new LinkedList<T>());
        }

        public void Encode(IPofWriter writer, int index, object value)
        {
            COUNTER++;
            writer.WriteCollection(index, (ICollection)value);
        }
    }
}
