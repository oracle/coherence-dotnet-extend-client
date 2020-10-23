/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Globalization;

using NUnit.Framework;

using Tangosol.Net.Cache;
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Comparator
{
    [TestFixture]
    public class ComparerTests
    {
        [Test]
        public void TestSafeComparer()
        {
            SafeComparer sc1 = new SafeComparer();
            SafeComparer sc2 = new SafeComparer();
            Assert.IsNotNull(sc1);
            Assert.AreEqual(sc1, sc2);
            Assert.AreEqual(sc1.ToString(), sc2.ToString());
            Assert.AreEqual(sc1.GetHashCode(), sc2.GetHashCode());
            SafeComparer sc3 = new SafeComparer(IdentityExtractor.Instance);
            Assert.AreNotEqual(sc1, sc3);

            Assert.IsNotNull(sc3.Comparer);
            Assert.AreEqual(sc3.Comparer, IdentityExtractor.Instance);

            Assert.AreEqual(sc1.Compare(null, null), 0);
            Assert.AreEqual(sc1.Compare(new object(), null), 1);
            Assert.AreEqual(sc1.Compare(null, new object()), -1);
            try
            {
                Assert.AreEqual(sc1.Compare(new object(), new object()), 0);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
            }

            Assert.AreEqual(sc1.Compare(10, 100), -1);
            Assert.AreEqual(sc3.Compare(10, 100), -1);


            TestQueryCacheEntry entry1 = new TestQueryCacheEntry("k2", "ana");
            TestQueryCacheEntry entry2 = new TestQueryCacheEntry("k1", "cana");

            sc3 = new SafeComparer(IdentityExtractor.Instance);
            Assert.AreEqual(sc3.CompareEntries(entry2, entry1), 1);
            IComparer c = new Comparer(CultureInfo.CurrentCulture);
            sc3 = new SafeComparer(c);
            Assert.AreEqual(sc3.CompareEntries(entry1, entry2), -1);
        }

        [Test]
        public void TestEntryComparer()
        {
            EntryComparer ec1 = new EntryComparer();
            Assert.IsNotNull(ec1);
            Assert.IsNull(ec1.Comparer);
            Assert.AreEqual(ec1.ComparisonStyle, ComparisonStyle.Auto);

            EntryComparer ec2 = new EntryComparer(IdentityExtractor.Instance);
            EntryComparer ec3 = new EntryComparer(IdentityExtractor.Instance, ComparisonStyle.Auto);
            Assert.IsNotNull(ec2);
            Assert.AreEqual(ec2, ec3);
            Assert.AreEqual(ec2.ToString(), ec3.ToString());
            Assert.AreEqual(ec2.GetHashCode(), ec3.GetHashCode());
            Assert.IsTrue(ec2.CompareValue);
            Assert.IsFalse(ec2.CompareEntry);

            CacheEntry entry1 = new CacheEntry("k1", 100);
            CacheEntry entry2 = new CacheEntry("k2", 1);

            ec2 = new EntryComparer(new KeyExtractor(IdentityExtractor.Instance), ComparisonStyle.Auto);
            Assert.IsTrue(ec2.CompareKey);
            Assert.AreEqual(ec2.Compare(entry1, entry2), -1);

            ec2 = new EntryComparer(IdentityExtractor.Instance, ComparisonStyle.Auto);
            Assert.IsTrue(ec2.CompareValue);
            Assert.AreEqual(ec2.Compare(entry1, entry2), 1);

            ec2 = new EntryComparer(IdentityExtractor.Instance, ComparisonStyle.Entry);
            Assert.IsTrue(ec2.CompareEntry);
            Assert.AreEqual(ec2.Compare(entry1, entry2), 1);

            TestQueryCacheEntry qentry1 = new TestQueryCacheEntry("k1", 100);
            TestQueryCacheEntry qentry2 = new TestQueryCacheEntry("k2", 1);
            Assert.AreEqual(ec2.Compare(qentry1, qentry2), 1);
        }

        [Test]
        public void TestInverseComparer()
        {
            InverseComparer ic1 = new InverseComparer();
            InverseComparer ic2 = new InverseComparer();
            Assert.IsNotNull(ic1);
            Assert.AreEqual(ic1, ic2);
            Assert.AreEqual(ic1.ToString(), ic2.ToString());
            Assert.AreEqual(ic1.GetHashCode(), ic2.GetHashCode());

            InverseComparer ic3 = new InverseComparer(IdentityExtractor.Instance);

            Assert.AreNotEqual(ic1, ic3);
            Assert.IsNotNull(ic3.Comparer);
            Assert.AreEqual(ic3.Comparer, IdentityExtractor.Instance);

            Assert.AreEqual(ic1.Compare(null, null), 0);
            Assert.AreEqual(ic1.Compare(new object(), null), -1);
            Assert.AreEqual(ic1.Compare(null, new object()), 1);
            try
            {
                Assert.AreEqual(ic1.Compare(new object(), new object()), 0);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
            }

            SafeComparer sc = new SafeComparer(IdentityExtractor.Instance);
            Assert.AreEqual(ic1.Compare(10, 100), 1);
            Assert.AreEqual(ic3.Compare(10, 100), -sc.Compare(10, 100));


            TestQueryCacheEntry entry1 = new TestQueryCacheEntry("k2", "aaa");
            TestQueryCacheEntry entry2 = new TestQueryCacheEntry("k1", "zzz");

            ic3 = new InverseComparer(IdentityExtractor.Instance);
            sc = new SafeComparer(IdentityExtractor.Instance);

            Assert.AreEqual(ic3.CompareEntries(entry2, entry1), -1);
            Assert.AreEqual(ic3.CompareEntries(entry2, entry1), -sc.CompareEntries(entry2, entry1));
            
            InverseComparer ic4 = new InverseComparer(new KeyExtractor("Key"));
            Assert.AreEqual(ic4.Compare(entry1, entry2), -1);
        }

        [Test]
        public void TestChainedComparer()
        {
            IComparer[] comparers = new IComparer[]
                {
                    new SafeComparer(IdentityExtractor.Instance),
                    new InverseComparer(IdentityExtractor.Instance)
                };

            ChainedComparer cc1 = new ChainedComparer(comparers);
            ChainedComparer cc2 = new ChainedComparer(comparers);
            Assert.IsNotNull(cc1);
            Assert.AreEqual(cc1, cc2);
            Assert.AreEqual(cc1.ToString(), cc2.ToString());
            Assert.AreEqual(cc1.GetHashCode(), cc2.GetHashCode());

            ChainedComparer cc3 = new ChainedComparer(comparers);

            Assert.IsNotNull(cc3.Comparers);
            Assert.IsInstanceOf(typeof(SafeComparer), cc3.Comparers[0]);
            Assert.IsInstanceOf(typeof(InverseComparer), cc3.Comparers[1]);

            Assert.AreEqual(cc3.Compare(null, null), 0);
            Assert.AreEqual(cc3.Compare(new object(), null), 1);
            Assert.AreEqual(cc3.Compare(null, new object()), -1);

            try
            {
                Assert.AreEqual(cc3.Compare(new object(), new object()), 0);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
            }

            Assert.AreEqual(cc3.Compare(10, 100), -1);

            TestQueryCacheEntry entry1 = new TestQueryCacheEntry("k2", "aaa");
            TestQueryCacheEntry entry2 = new TestQueryCacheEntry("k1", "zzz");

            Assert.AreEqual(cc3.CompareEntries(entry2, entry1), 1);
        }
    }
}