/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util.Collections;
using Tangosol.Util.Filter;

using NUnit.Framework;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// Unit test of the <see cref="ConditionalExtractor"/> implementation.
    /// </summary>
    /// <author> tb 2010.2.10 </author>
    /// <author> lh 2010.6.29 </author>
    [TestFixture]
    public class ConditionalExtractorTest
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
        /// Test createIndex
        /// </summary>
        [Test]
        public void TestCreateIndex()
        {
            IValueExtractor extractor          = new IdentityExtractor();
            IFilter         filter             = new GreaterFilter(extractor, 5);
            IDictionary     map                = new HashDictionary();

            var condExtractor = new ConditionalExtractor(filter, extractor, true);

            var index = condExtractor.CreateIndex(false, null, map);
            Assert.IsTrue(index is ConditionalIndex);
            Assert.AreEqual(filter, ((ConditionalIndex)index).Filter);
            Assert.AreEqual(extractor, index.ValueExtractor);

            // make sure that the index map has been updated with the created
            // index
            var index2 = map[extractor] as ICacheIndex;
            Assert.IsNotNull(index2);
            Assert.AreEqual(index, index2);
        }

        /// <summary>
        /// Test destroyIndex
        /// </summary>
        [Test]
        public void TestDestroyIndex()
        {
            IValueExtractor extractor          = new IdentityExtractor();
            IFilter         filter             = new GreaterFilter(extractor, 5);
            IDictionary     map                = new HashDictionary();

            var condExtractor = new ConditionalExtractor(filter, extractor, true);

            var index = condExtractor.CreateIndex(false, null, map);

            // make sure that the index map has been updated with the created
            // index
            var index2 = map[extractor] as ICacheIndex;
            Assert.IsNotNull(index2);
            Assert.AreEqual(index, index2);

            condExtractor.DestroyIndex(map);

            // make sure that the index has been removed from the index map
            Assert.IsNull(map[extractor]);
        }

        /// <summary>
        /// Test extract
        /// </summary>
        [Test]
        public void TestExtract()
        {
            IValueExtractor extractor = new ReflectionExtractor("getId");
            IFilter         filter    = new GreaterFilter(extractor, 5);
            var             person    = new Person("123456789", DateTime.Now);

            var condExtractor = new ConditionalExtractor(filter, extractor, true);

            Assert.That(() => condExtractor.Extract(person), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Regression test for COH-5516.
        /// </summary>
        [Test]
        public void testCoh5516()
        {
            IFilter         typeFilter    = new EqualsFilter("getType", "bird");
            IValueExtractor intExtractor  = new ReflectionExtractor("getWings");
            var             condExtractor = new ConditionalExtractor(typeFilter, intExtractor, false);
            IDictionary     map           = new HashDictionary();

            var index = condExtractor.CreateIndex(false, null, map);

            var bird   = new Bird();
            var fish   = new Fish();
            var entry1 = new CacheEntry(0, bird, bird);
            var entry2 = new CacheEntry(1, fish, fish);

            // add entries of type Fish and Bird - only the Bird should get indexed
            index.Insert(entry1);
            index.Insert(entry2);

            // remove entries of type Fish and Bird - only the Bird should get indexed
            index.Delete(entry1);
            index.Delete(entry2);
        }

        public class Fish : IPortableObject
        {
            public String getType()
            {
                return "fish";
            }

            #region IPortableObject implementation

            public void ReadExternal(IPofReader reader)
            {
            }

            public void WriteExternal(IPofWriter writer)
            {
            }

            #endregion
        }

        public class Bird : IPortableObject
        {
            public String getType()
            {
                return "bird";
            }

            public int getWings()
            {
                return 2;
            }

            #region IPortableObject implementation

            public void ReadExternal(IPofReader reader)
            {
            }

            public void WriteExternal(IPofWriter writer)
            {
            }

            #endregion
        }
    }
}