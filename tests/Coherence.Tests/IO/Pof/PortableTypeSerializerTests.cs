/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
﻿using System;

using NUnit.Framework;

using Tangosol.Test;
using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class PortableTypeSerializerTests
    {
    // ReSharper disable InconsistentNaming
    private SimplePofContext V1;
    private SimplePofContext V2;

    [SetUp]
    public void Setup()
    {
        TestContext.Error.WriteLine($"[START] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
        V1 = new SimplePofContext();
        V1.RegisterUserType(1, typeof(Test.V1.Pet),
                            new PortableTypeSerializer(1, typeof(Test.V1.Pet)));
        V1.RegisterUserType(2, typeof(Test.V1.Dog),
                            new PortableTypeSerializer(2, typeof(Test.V1.Dog)));

        V2 = new SimplePofContext();
        V2.RegisterUserType(1, typeof(Test.V2.Pet),
                            new PortableTypeSerializer(1, typeof(Test.V2.Pet)));
        V2.RegisterUserType(2, typeof(Test.V2.Dog),
                            new PortableTypeSerializer(2, typeof(Test.V2.Dog)));
        V2.RegisterUserType(3, typeof(Test.V2.Animal),
                            new PortableTypeSerializer(3, typeof(Test.V2.Animal)));
        V2.RegisterUserType(5, typeof(Color), new EnumPofSerializer());
    }

    [TearDown]
    public void TearDown()
    {
        TestContext.Error.WriteLine($"[END]   {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
    }

    [Test]
    public void TestRoundTripV1()
        {
        Test.V1.Dog dog = new Test.V1.Dog("Nadia", "Boxer");
        Console.WriteLine(dog);
        Binary binDog = SerializationHelper.ToBinary(dog, V1);
        Assert.AreEqual(dog, SerializationHelper.FromBinary(binDog, V1));
        }

    [Test]
    public void TestRoundTripV2()
        {
        Test.V2.Dog dog = new Test.V2.Dog("Nadia", 10, "Boxer", Color.Brindle);
        Console.WriteLine(dog);
        Binary binDog = SerializationHelper.ToBinary(dog, V2);
        Assert.AreEqual(dog, SerializationHelper.FromBinary(binDog, V2));
        }

    [Test]
    public void TestEvolution()
        {
        Test.V2.Dog dogV2 = new Test.V2.Dog("Nadia", 10, "Boxer", Color.Brindle);

        Console.WriteLine(dogV2);
        Binary binDogV2 = SerializationHelper.ToBinary(dogV2, V2);

        Test.V1.Dog dogV1 = (Test.V1.Dog) SerializationHelper.FromBinary(binDogV2, V1);
        Console.WriteLine(dogV1);
        Binary binDogV1 = SerializationHelper.ToBinary(dogV1, V1);

        Object dog = SerializationHelper.FromBinary(binDogV1, V2);
        Console.WriteLine(dog);

        Assert.AreEqual(dogV2, dog);
        }
    }
}
