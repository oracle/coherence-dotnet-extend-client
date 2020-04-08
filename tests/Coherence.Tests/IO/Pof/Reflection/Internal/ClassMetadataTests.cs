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
    public class ClassMetadataTests
    {
        [Test]
        public void Builder()
        {
            var builder = new ClassMetadataBuilder<object>();
            ITypeMetadata<object> tmd = builder.SetClass(typeof (object))
                .SetHash("TestDomain".GetHashCode())
                .SetTypeId(1234)
                .AddAttribute( builder.NewAttribute()
                    .SetIndex(0)
                    .SetName("normal")
                    .SetVersion(0).Build()).Build();

            Assert.AreEqual(1234, tmd.GetKey().GetTypeId());
            Assert.IsTrue(tmd.GetKey().GetHash()!=0);
            Assert.IsNotNull(tmd.GetAttribute("normal"));
            Assert.AreEqual("normal", tmd.GetAttribute("normal").GetName());
            Assert.AreEqual(0, tmd.GetAttribute("normal").GetIndex());
            Assert.AreEqual(0, tmd.GetAttribute("normal").GetVersion());
        }

        [Test]
        public void TestClassMetadata()
        {
            MethodInfo method = typeof (ClassMetadataDescribable).GetMethod("GetName");

            var builder = new ClassMetadataBuilder<ClassMetadataDescribable>();
            ITypeMetadata<ClassMetadataDescribable> tmd = builder.SetClass(typeof(ClassMetadataDescribable))
                .SetHash("TestDomain".GetHashCode())
                .SetTypeId(1234)
                .AddAttribute(builder.NewAttribute()
                    .SetIndex(0)
                    .SetName("name")
                    .SetCodec(Codecs.DEFAULT_CODEC)
                    .SetInvocationStrategy(new InvocationStrategies.MethodInvcationStrategy<ClassMetadataDescribable>(method))
                    .SetVersion(0).Build()).Build();

            ITypeMetadata<ClassMetadataDescribable> tmd2 = builder.SetClass(typeof(ClassMetadataDescribable))
                .SetHash("TestDomain".GetHashCode())
                .SetTypeId(1234)
                .AddAttribute(builder.NewAttribute()
                    .SetIndex(0)
                    .SetName("name")
                    .SetCodec(Codecs.DEFAULT_CODEC)
                    .SetInvocationStrategy(new InvocationStrategies.MethodInvcationStrategy<ClassMetadataDescribable>(method))
                    .SetVersion(0).Build()).Build();

            Assert.AreEqual(1234, tmd.GetKey().GetTypeId());
            Assert.IsTrue(tmd.GetKey().GetHash() != 0);
            Assert.IsNotNull(tmd.GetAttribute("name"));
            Assert.AreEqual("name", tmd.GetAttribute("name").GetName());
            Assert.AreEqual(0, tmd.GetAttribute("name").GetIndex());
            Assert.AreEqual(0, tmd.GetAttribute("name").GetVersion());
            Assert.AreEqual(tmd, tmd2);
            Assert.AreEqual(tmd.GetHashCode(), tmd2.GetHashCode());

            ClassMetadataDescribable cmdd = new ClassMetadataDescribable("augusta");
            Assert.AreEqual("augusta", tmd.GetAttribute("name").Get(cmdd));
            tmd.GetAttribute("name").Set(cmdd,"ada");
            Assert.AreEqual("ada", tmd.GetAttribute("name").Get(cmdd));
            Assert.IsInstanceOf(typeof(Codecs.DefaultCodec), tmd.GetAttribute("name").GetCodec());
            Assert.IsTrue(tmd.GetAttributes().MoveNext());
            Assert.IsInstanceOf(typeof (ClassMetadataDescribable), tmd.NewInstance());

            object a = (object)"dasd";
        }

        [Test]
        public void TestClassAttributeSort()
        {
            var builder = new ClassMetadataBuilder<object>();
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

            ClassMetadata<object> cmd = builder.Build();

            Assert.AreEqual(0, cmd.GetAttribute("c").GetIndex());
            Assert.AreEqual(1, cmd.GetAttribute("a").GetIndex());
            Assert.AreEqual(2, cmd.GetAttribute("b").GetIndex());
            Assert.AreEqual(3, cmd.GetAttribute("e").GetIndex());
            Assert.AreEqual(4, cmd.GetAttribute("d").GetIndex());
            Assert.AreEqual(5, cmd.GetAttribute("f").GetIndex());
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
