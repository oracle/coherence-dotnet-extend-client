/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Util.Comparator;
using Tangosol.Util.Extractor;

using NUnit.Framework;

namespace Tangosol.Util.Filter
{
    /**
    * A collection of functional tests for a Coherence*Extend client that
    * uses LimitFilter.
    *
    * @author lh  2012.02.23
    */
    [TestFixture]
    public class LimitFilterTests
    {
        readonly NameValueCollection appSettings = TestUtils.AppSettings;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
        }

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

        // ----- LimitFilter tests ----------------------------------------------

        [Test]
        public void TestLimitFilter()
        {
            INamedCache                cache          = CacheFactory.GetCache(CacheName);
            LimitFilter                limitFilter    = new LimitFilter(new AlwaysFilter(), 10);
            IDictionary<Int32, String> mapReturn      = new Dictionary<Int32, String>();
            bool                       entryReturned  = true;
            int                        totalCount     = 0, 
                                       uniqueCount    = 0;

            cache.Clear();
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                cache.Insert(i, "value" + i);
            }

            while (entryReturned)
            {
                entryReturned = false;
                foreach (ICacheEntry entry in cache.GetEntries(limitFilter))
                {
                    ++totalCount;
                    entryReturned = true;
                    if (!mapReturn.ContainsKey((int) entry.Key))
                    {
                        mapReturn.Add((Int32)entry.Key,
                                (String)entry.Value);
                        ++uniqueCount;
                    }
                }
                limitFilter.NextPage();
            };

            Assert.AreEqual(CACHE_SIZE, totalCount);
            Assert.AreEqual(totalCount, uniqueCount);
        }

        [Test]
        public void TestComparer()
        {
            INamedCache cache = CacheFactory.GetCache("dist-comparator-cache");
            Random      r     = new Random();
            for (int i = 0; i < 10000; i++)
            {
                AirDealComparer.AirDeal deal = new AirDealComparer.AirDeal(i, "SFO", "JFK", r.NextDouble());
                cache.Add(deal.Oid, deal);
            }
            IValueExtractor ve = new ReflectionExtractor("getOrigAirport");
            cache.AddIndex(ve, true, null);

            IFilter     primaryFilter = new EqualsFilter(ve, "SFO");
            IFilter     filterLimit   = new LimitFilter(primaryFilter, 40);
            ICollection setReturn     = cache.GetEntries(filterLimit, new AirDealComparer());
            Assert.AreEqual(setReturn.Count, 40);
        }

        [Test]
        public void TestToString()
        {
            // minimal LimitFilter
            LimitFilter filter = new LimitFilter(AlwaysFilter.Instance, 5);

            Assert.AreEqual("LimitFilter: (AlwaysFilter [pageSize=5, pageNum=0])", filter.ToString());

            // set the page
            filter.Page = 1;

            Assert.AreEqual("LimitFilter: (AlwaysFilter [pageSize=5, pageNum=1])", filter.ToString());

            // add a Comparer
            filter.Comparer = Comparer.Default;

            Assert.AreEqual("LimitFilter: (AlwaysFilter [pageSize=5, pageNum=1, top=, bottom=, comparer=System.Collections.Comparer])", filter.ToString());
        }

        private static int CACHE_SIZE = 84;
    }
}
