/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Reflection;
using NUnit.Framework;

namespace Tangosol.IO.Pof.Reflection.Internal
{
    [TestFixture]
    public class TypeMetadataTests
    {
        [Test]
        public void Builder()
        { 
            var builder = new TypeMetadataBuilder<object>();
            ITypeMetadata<object> tmd = builder.SetType(typeof (object))
                .SetHash("TestDomain".GetHashCode())
                .SetTypeId(1234)
                .AddAttribute( builder.NewAttribute()
                    .SetIndex(0)
                    .SetName("normal")
                    .SetVersion(0).Build()).Build();

            Assert.AreEqual(1234, tmd.Key.TypeId);
            Assert.IsTrue(tmd.Key.Hash!=0);
            Assert.IsNotNull(tmd.GetAttribute("normal"));
            Assert.AreEqual("normal", tmd.GetAttribute("normal").Name);
            Assert.AreEqual(0, tmd.GetAttribute("normal").Index);
            Assert.AreEqual(0, tmd.GetAttribute("normal").VersionId);
        }

        [Test]
        public void TestClassMetadata()
        {
            MethodInfo method = typeof (ClassMetadataDescribable).GetMethod("GetName");

            var builder = new TypeMetadataBuilder<ClassMetadataDescribable>();
            ITypeMetadata<ClassMetadataDescribable> tmd = builder.SetType(typeof(ClassMetadataDescribable))
                .SetHash("TestDomain".GetHashCode())
                .SetTypeId(1234)
                .AddAttribute(builder.NewAttribute()
                    .SetIndex(0)
                    .SetName("name")
                    .SetCodec(Codecs.DEFAULT_CODEC)
                    .SetInvocationStrategy(new InvocationStrategies.MethodInvocationStrategy<ClassMetadataDescribable>(method))
                    .SetVersion(0).Build()).Build();

            ITypeMetadata<ClassMetadataDescribable> tmd2 = builder.SetType(typeof(ClassMetadataDescribable))
                .SetHash("TestDomain".GetHashCode())
                .SetTypeId(1234)
                .AddAttribute(builder.NewAttribute()
                    .SetIndex(0)
                    .SetName("name")
                    .SetCodec(Codecs.DEFAULT_CODEC)
                    .SetInvocationStrategy(new InvocationStrategies.MethodInvocationStrategy<ClassMetadataDescribable>(method))
                    .SetVersion(0).Build()).Build();

            Assert.AreEqual(1234, tmd.Key.TypeId);
            Assert.IsTrue(tmd.Key.Hash != 0);
            Assert.IsNotNull(tmd.GetAttribute("name"));
            Assert.AreEqual("name", tmd.GetAttribute("name").Name);
            Assert.AreEqual(0, tmd.GetAttribute("name").Index);
            Assert.AreEqual(0, tmd.GetAttribute("name").VersionId);
            Assert.AreEqual(tmd, tmd2);
            Assert.AreEqual(tmd.GetHashCode(), tmd2.GetHashCode());

            ClassMetadataDescribable cmdd = new ClassMetadataDescribable("augusta");
            Assert.AreEqual("augusta", tmd.GetAttribute("name").Get(cmdd));
            tmd.GetAttribute("name").Set(cmdd,"ada");
            Assert.AreEqual("ada", tmd.GetAttribute("name").Get(cmdd));
            Assert.IsInstanceOf(typeof(Codecs.DefaultCodec), tmd.GetAttribute("name").Codec);
            Assert.IsTrue(tmd.GetAttributes().MoveNext());
            Assert.IsInstanceOf(typeof (ClassMetadataDescribable), tmd.NewInstance());
        }

        [Test]
        public void TestClassAttributeSort()
        {
            var builder = new TypeMetadataBuilder<object>();
            var attrBuilder = builder.NewAttribute();

            IAttributeMetadata<object>[] expected = new IAttributeMetadata<object>[]
            {
                attrBuilder.SetVersion(0).SetIndex( 0).SetName("c").Build(),
                attrBuilder.SetVersion(0).SetIndex( 1).SetName("a").Build(),
                attrBuilder.SetVersion(0).SetIndex( 2).SetName("b").Build(),
                attrBuilder.SetVersion(1).SetIndex( 3).SetName("e").Build(),
                attrBuilder.SetVersion(1).SetIndex(-1).SetName("d").Build(),
                attrBuilder.SetVersion(1).SetIndex(-1).SetName("f").Build()
            };

            builder.AddAttribute(expected[3]);
            builder.AddAttribute(expected[1]);
            builder.AddAttribute(expected[5]);
            builder.AddAttribute(expected[0]);
            builder.AddAttribute(expected[4]);
            builder.AddAttribute(expected[2]);

            TypeMetadata<object> cmd = builder.Build();

            Assert.AreEqual(0, cmd.GetAttribute("c").Index);
            Assert.AreEqual(1, cmd.GetAttribute("a").Index);
            Assert.AreEqual(2, cmd.GetAttribute("b").Index);
            Assert.AreEqual(3, cmd.GetAttribute("e").Index);
            Assert.AreEqual(4, cmd.GetAttribute("d").Index);
            Assert.AreEqual(5, cmd.GetAttribute("f").Index);
        }

        public class ClassMetadataDescribable
        {
            public ClassMetadataDescribable()
            {
            }

            public ClassMetadataDescribable(String sName)
            {
                m_sName = sName;
            }

            public String GetName()
            {
                return m_sName;
            }

            public void SetName(string sName)
            {
                m_sName = sName;
            }

            private String m_sName;
        }
    }
}
