/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

using NUnit.Framework;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util.Collections;
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Filter
{
    [TestFixture]
    public class BetweenFilterTests
    {
        [Test]
        public void ShouldSerializeBetweenFilter()
        {
            BetweenFilter filter    = new BetweenFilter(m_extractor, 10, 20, true, true);
            Stream      stream    = new MemoryStream();
            m_serializer.Serialize(new DataWriter(stream), filter);
            stream.Position = 0;
            var result = m_serializer.Deserialize(new DataReader(stream));

            Assert.IsInstanceOf(typeof(BetweenFilter), result);
            BetweenFilter resultFilter = (BetweenFilter) result;
            Assert.AreEqual(resultFilter.ValueExtractor, filter.ValueExtractor);
            Assert.AreEqual(resultFilter.LowerBound, filter.LowerBound);
            Assert.AreEqual(resultFilter.UpperBound, filter.UpperBound);
            Assert.AreEqual(resultFilter.IsLowerBoundInclusive, filter.IsLowerBoundInclusive);
            Assert.AreEqual(resultFilter.IsUpperBoundInclusive, filter.IsUpperBoundInclusive);
        }

        [Test]
        public void ShouldForceLowerBoundExcludedIfLowerBoundIsNull()
        {
            BetweenFilter filter = new BetweenFilter(m_extractor, null, 20, true, true);
            Assert.IsFalse(filter.IsLowerBoundInclusive);
        }

        [Test]
        public void ShouldEvaluateValueLowerThanRangeToFalse()
        {
            BetweenFilter filter = new BetweenFilter(m_extractor, 10, 20);
            Assert.IsFalse(filter.Evaluate(9));
        }

        [Test]
        public void ShouldEvaluateValueEqualToLowerBoundToTrue()
        {
            BetweenFilter filter = new BetweenFilter(m_extractor, 10, 20);
            Assert.IsTrue(filter.Evaluate(10));
        }

        [Test]
        public void ShouldEvaluateValueEqualToLowerBoundToFalseIfLowerBoundExclusive()
        {
            BetweenFilter filter = new BetweenFilter(m_extractor, 10, 20, false, true);
            Assert.IsFalse(filter.Evaluate(10));
        }

        [Test]
        public void ShouldEvaluateValueHigherThanRangeToFalse()
        {
            BetweenFilter filter = new BetweenFilter(m_extractor, 10, 20);
            Assert.IsFalse(filter.Evaluate(21));
        }

        [Test]
        public void ShouldEvaluateValueEqualToUpperBoundToTrue()
        {
            BetweenFilter filter = new BetweenFilter(m_extractor, 10, 20);
            Assert.IsTrue(filter.Evaluate(20));
        }

        [Test]
        public void ShouldEvaluateValueEqualToUpperBoundToFalseIfUpperBoundExclusive()
        {
            BetweenFilter filter = new BetweenFilter(m_extractor, 10, 20, true, false);
            Assert.IsFalse(filter.Evaluate(20));
        }

        [Test]
        public void ShouldEvaluateNullAsFalse()
        {
            BetweenFilter filter = new BetweenFilter(m_extractor, 10, 20, true, false);
            Assert.IsFalse(filter.Evaluate(null));
        }

        [Test]
        public void ShouldEvaluateAsFalseIfLowerBoundIsNull()
        {
            BetweenFilter filter = new BetweenFilter(m_extractor, null, 20, true, false);
            Assert.IsFalse(filter.Evaluate(12));
        }

        [Test]
        public void ShouldEvaluateAsFalseIfUpperBoundIsNull()
        {
            BetweenFilter filter = new BetweenFilter(m_extractor, 10, null, true, false);
            Assert.IsFalse(filter.Evaluate(12));
        }

        [Test]
        public void ShouldCalculateEffectivenessWithNoInvertedIndex()
        {
            IDictionary indexes       = new HashDictionary();
            BetweenFilter filter        = new BetweenFilter(m_extractor, 10, 20);
            var         effectiveness = filter.CalculateEffectiveness(indexes, s_setKeys);

            Assert.AreEqual(ExtractorFilter.EVAL_COST * s_setKeys.Count, effectiveness);
        }

        [Test]
        public void ShouldCalculateEffectivenessWhenInvertedIndexIsSortedSet()
        {
            IDictionary indexes  = new HashDictionary();
            indexes[m_extractor] = s_sortedIndex;

            BetweenFilter filter        = new BetweenFilter(m_extractor, 10, 20);
            var         effectiveness = filter.CalculateEffectiveness(indexes, s_setKeys);
            Assert.AreEqual(31, effectiveness);
        }

        [Test]
        public void ShouldCalculateEffectivenessWhenInvertedIndexIsPlainSet()
        {
            IDictionary indexes  = new HashDictionary();
            indexes[m_extractor] = s_sortedIndex;

            BetweenFilter filter        = new BetweenFilter(m_extractor, 10, 20);
            var           effectiveness = filter.CalculateEffectiveness(indexes, s_setKeys);

            Assert.AreEqual(s_sortedIndex.IndexContents.Count, effectiveness);
        }

        [Test]
        public void ShouldApplyIndexWhenNoMatchingIndexPresent()
        {
            IDictionary indexes = new HashDictionary();
            ICollection keys    = new HashSet(s_setKeys);
            BetweenFilter filter  = new BetweenFilter(m_extractor, 10, 20);
            var         result  = filter.ApplyIndex(indexes, keys);

            Assert.AreSame(filter, result);
            Assert.AreEqual(s_setKeys, keys);
        }

        [Test]
        public void ShouldApplyIndexWhenLowerBoundIsNull()
        {
            IDictionary indexes = new HashDictionary();
            ICollection keys = new HashSet(s_setKeys);
            BetweenFilter filter = new BetweenFilter(m_extractor, null, 20);
            var result = filter.ApplyIndex(indexes, keys);

            Assert.IsNull(result);
            Assert.AreEqual(keys.Count, 0);
        }

        [Test]
        public void ShouldApplyIndexWhenUpperBoundIsNull()
        {
            IDictionary indexes = new HashDictionary();
            ICollection keys = new HashSet(s_setKeys);
            BetweenFilter filter = new BetweenFilter(m_extractor, 10, null);
            var result = filter.ApplyIndex(indexes, keys);

            Assert.IsNull(result);
            Assert.AreEqual(keys.Count, 0);
        }
        
        [Test]
        public void ShouldApplyIndexWhenSortedIndexPresent()
        {
            IDictionary indexes = new HashDictionary();
            ICollection keys    = new HashSet(s_setKeys);
            BetweenFilter filter  = new BetweenFilter(m_extractor, 10, 20);

            indexes[m_extractor] = s_sortedIndex;

            var result = filter.ApplyIndex(indexes, keys);

            Assert.IsNull(result);
            Assert.IsTrue(s_setKeysTenToTwenty.Equals(keys));
        }

        [Test]
        public void ShouldApplyIndexWhenUnsortedIndexPresent()
        {
            IDictionary indexes = new HashDictionary();
            ICollection keys    = new HashSet(s_setKeys);
            BetweenFilter filter  = new BetweenFilter(m_extractor, 10, 20);

            indexes[m_extractor] = s_unsortedIndex;

            var result = filter.ApplyIndex(indexes, keys);

            Assert.IsNull(result);
            Assert.IsTrue(s_setKeysTenToTwenty.Equals(keys));
        }


        [OneTimeSetUp]
        public static void Setup()
        {
            s_sortedIndex        = new SimpleCacheIndex(IdentityExtractor.Instance, true, null, true);
            s_unsortedIndex      = new SimpleCacheIndex(IdentityExtractor.Instance, false, null, true);
            s_setKeys            = new HashSet();
            s_setKeysTenToTwenty = new HashSet();

            ICacheEntry entry;

            for (var key = 50; key < 250; key++)
            {
                var value = key % 30;

                s_setKeys.Add(key);
                
                if (value >= 10 && value <= 20)
                {
                    s_setKeysTenToTwenty.Add(key);
                }
                
                entry = new CacheEntry(key, value);
                s_sortedIndex.Insert(entry);
                s_unsortedIndex.Insert(entry);
            }

            // Add null value to indexes
            entry = new CacheEntry(-1000, null);
            s_sortedIndex.Insert(entry);
            s_unsortedIndex.Insert(entry);
        }

        protected static ICacheIndex s_sortedIndex;
        protected static ICacheIndex s_unsortedIndex;
        protected static HashSet     s_setKeys;
        protected static HashSet     s_setKeysTenToTwenty;

        protected ConfigurablePofContext m_serializer = new ConfigurablePofContext("assembly://Coherence/Tangosol.Config/coherence-pof-config.xml");
        protected IValueExtractor        m_extractor = IdentityExtractor.Instance;
    }
}
