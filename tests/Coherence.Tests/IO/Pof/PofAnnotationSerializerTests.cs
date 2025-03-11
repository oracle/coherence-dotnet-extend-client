/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
﻿using System;
using NUnit.Framework;
using Tangosol.IO.Pof.Annotation;
using Tangosol.Util;
using EH = Tangosol.Util.SerializationHelper;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class PofAnnotationSerializerTests
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
        public void TestSerialization()
        {
            SimplePofContext cxt = new SimplePofContext();
            cxt.RegisterUserType(1001, typeof(PersonV1), new PofAnnotationSerializer(1001, typeof(PersonV1)));
            cxt.RegisterUserType(1002, typeof(Child), new PofAnnotationSerializer(1002, typeof(Child), true));

            PersonV1 value = new PersonV1("Frank", "Spencer", 57);
            Child child = new Child("Betty", "Spencer", 55);

            Binary binValue = EH.ToBinary(child, cxt);
            var reader = new DataReader(binValue.GetStream());
            int typeId = reader.ReadByte() == 21 ? reader.ReadPackedInt32() : -1;

            PersonV1 teleported = EH.FromBinary(SerializationHelper.ToBinary(value, cxt), cxt) as PersonV1;
            Child teleported2 = EH.FromBinary(binValue, cxt) as Child;

            Assert.AreEqual(1002, typeId);
            Assert.IsNotNull(teleported);
            Assert.IsNotNull(teleported2);
            Assert.AreEqual("Frank", teleported.m_firstName);
            Assert.AreEqual("Spencer", teleported.m_lastName);
            Assert.AreEqual(57, teleported.m_age);
            Assert.AreEqual("Betty", teleported2.m_firstName);
            Assert.AreEqual("Spencer", teleported2.m_lastName);
            Assert.AreEqual(55, teleported2.m_age);
        }

        [Test]
        public void TestAncestry()
        {
            SimplePofContext cxt = new SimplePofContext();
            cxt.RegisterUserType(1002, typeof(GrandFather), new PofAnnotationSerializer(1002, typeof(GrandFather), true));
            cxt.RegisterUserType(1003, typeof(Father), new PofAnnotationSerializer(1003, typeof(Father), true));
            cxt.RegisterUserType(1004, typeof(Child), new PofAnnotationSerializer(1004, typeof(Child), true));

            var son = new Child("Bart", "Simpson", 10);
            var dad = new Father("Homer", "Simpson", 50, son);
            var gf = new GrandFather("Abe", "Simpson", 100, dad);

            var teleported = (GrandFather)EH.FromBinary(EH.ToBinary(gf, cxt), cxt);

            Assert.AreEqual("Abe", teleported.m_firstName);
            Assert.AreEqual("Simpson", teleported.m_lastName);
            Assert.AreEqual(100, teleported.m_age);
            Assert.AreEqual("Homer", teleported.m_father.m_firstName);
            Assert.AreEqual("Simpson", teleported.m_father.m_lastName);
            Assert.AreEqual(50, teleported.m_father.m_age);
            Assert.AreEqual("Bart", teleported.m_father.m_child.m_firstName);
            Assert.AreEqual("Simpson", teleported.m_father.m_child.m_lastName);
            Assert.AreEqual(10, teleported.m_father.m_child.m_age);
        }


        [Test]
        public void TestInheritance()
        {
            var cxt = new SimplePofContext();
            cxt.RegisterUserType(1001, typeof(PersonV1), new PofAnnotationSerializer(1001, typeof(PersonV1)));
            cxt.RegisterUserType(1005, typeof(BewilderedPerson), new PofAnnotationSerializer(1005, typeof(BewilderedPerson), true));

            var value = new BewilderedPerson("Frank", "Spencer", 57, "dizzy");
            var teleported = EH.FromBinary(EH.ToBinary(value, cxt), cxt) as BewilderedPerson;

            Assert.IsNotNull(teleported);
            Assert.AreEqual("Frank", teleported.m_firstName);
            Assert.AreEqual("Spencer", teleported.m_lastName);
            Assert.AreEqual(57, teleported.m_age);
            Assert.AreEqual("dizzy", teleported.m_state);
        }

        [Test]
        public void TestEvolvable()
        {
            var cxt1 = new SimplePofContext();
            var cxt2 = new SimplePofContext();
            cxt1.RegisterUserType(1001, typeof(PersonV1), new PofAnnotationSerializer(1001, typeof(PersonV1)));
            cxt2.RegisterUserType(1001, typeof(PersonV2), new PofAnnotationSerializer(1001, typeof(PersonV2)));

            var personV1 = new PersonV1("Frank", "Spencer", 57);
            // verify we can go forward 1 => 2
            var teleportedV2 = EH.FromBinary(EH.ToBinary(personV1, cxt1), cxt2) as PersonV2;

            // verify we can go back 2 => 1
            teleportedV2.m_fMale = true;
            var teleportedV1 = EH.FromBinary(EH.ToBinary(teleportedV2, cxt2), cxt1) as PersonV1;
            var teleportedV2FromV1 = EH.FromBinary(EH.ToBinary(teleportedV1, cxt1), cxt2) as PersonV2;

            // v1 => v2
            Assert.IsNotNull(teleportedV2);
            Assert.AreEqual("Frank", teleportedV2.m_firstName);
            Assert.AreEqual("Spencer", teleportedV2.m_lastName);
            Assert.AreEqual(57, teleportedV2.m_age);
            // v2 => v1
            Assert.IsNotNull(teleportedV1);
            Assert.IsNotNull(teleportedV1.FutureData);
            Assert.AreEqual("Frank", teleportedV1.m_firstName);
            Assert.AreEqual("Spencer", teleportedV1.m_lastName);
            Assert.AreEqual(57, teleportedV1.m_age);
            Assert.IsNotNull(teleportedV2FromV1);
            Assert.AreEqual("Frank", teleportedV2FromV1.m_firstName);
            Assert.AreEqual("Spencer", teleportedV2FromV1.m_lastName);
            Assert.AreEqual(57, teleportedV2FromV1.m_age);
            Assert.IsTrue(teleportedV2FromV1.m_fMale);
        }

        [Test]
        public void TestGenerics()
        {
            SimplePofContext cxt = new SimplePofContext();
            cxt.RegisterUserType(1001, typeof(PersonV1), new PofAnnotationSerializer(1001, typeof(PersonV1)));

            PersonV1 value = new PersonV1("Frank", "Spencer", 57);

            PersonV1 teleported = EH.FromBinary(EH.ToBinary(value, cxt), cxt) as PersonV1;

            Assert.IsNotNull(teleported);
            Assert.AreEqual("Frank", teleported.m_firstName);
            Assert.AreEqual("Spencer", teleported.m_lastName);
            Assert.AreEqual(57, teleported.m_age);
        }
    }

    [Portable]
    public class PersonV1 : AbstractEvolvable
    {
        public PersonV1()
        {
        }

        public PersonV1(string firstName, string lastName, int age)
        {
            m_firstName = firstName;
            m_lastName = lastName;
            m_age = age;
        }   

        [PortableProperty(2)]
        public string m_firstName;
        [PortableProperty(0)]
        public string m_lastName;
        [PortableProperty(1)]
        public int m_age;

        public override int ImplVersion
        {
            get { return 1; }
        }
    }

    [Portable]
    public class PersonV2 : AbstractEvolvable
    {
        public PersonV2()
        {
        }

        public PersonV2(string firstName, string lastName, int age)
            : this(firstName, lastName, age, default(Boolean))
        {
        }

        public PersonV2(string firstName, string lastName, int age, Boolean fMale)
        {
            m_firstName = firstName;
            m_lastName  = lastName;
            m_age       = age;
            m_fMale     = fMale;
        }

        [PortableProperty(2)]
        public string m_firstName;
        [PortableProperty(0)]
        public string m_lastName;
        [PortableProperty(1)]
        public int m_age;
        [PortableProperty(3)]
        public Boolean m_fMale;

        public override int ImplVersion
        {
            get { return 1; }
        }
    }

    [Portable]
    public class BewilderedPerson : PersonV1
    {

        public BewilderedPerson()
        {
        }

        public BewilderedPerson(string firstName, string lastName, int age) 
            : base(firstName, lastName, age)
        {
        }

        public BewilderedPerson(string firstName, string lastName, int age, string state)
            : base(firstName, lastName, age)
        {
            m_state = state;
        }

        [PortableProperty]
        public string m_state;
    }


    [Portable]
    public class GrandFather
    {
        public GrandFather()
        {
        }

        public GrandFather(string firstName, string lastName, int age, Father father)
        {
            m_firstName = firstName;
            m_lastName = lastName;
            m_age = age;
            m_father = father;
        }

        [PortableProperty(2)]
        public string m_firstName;
        [PortableProperty(0)]
        public string m_lastName;
        [PortableProperty(1)]
        public int m_age;
        [PortableProperty]
        public Father m_father;
    }

    [Portable]
    public class Father
    {
        public Father()
        {
        }

        public Father(string firstName, string lastName, int age, Child child)
        {
            m_firstName = firstName;
            m_lastName = lastName;
            m_age = age;
            m_child = child;
        }

        [PortableProperty]
        public string m_firstName;
        [PortableProperty]
        public string m_lastName;
        [PortableProperty]
        public int m_age;
        [PortableProperty]
        public Child m_child;
    }

    [Portable]
    public class Child
    {
        public Child()
        {
        }

        public Child(string firstName, string lastName, int age)
        {
            m_firstName = firstName;
            m_lastName = lastName;
            m_age = age;
        }

        [PortableProperty]
        public string m_firstName;
        [PortableProperty]
        public string m_lastName;
        [PortableProperty]
        public int m_age;
    }
}
