/*
 * Copyright (c) 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using NUnit.Framework;
using Tangosol.Net;
using Tangosol.Run.Xml;
using Tangosol.Util.Aggregator;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;

namespace Tangosol.Util;

[TestFixture]
public class SimpleQueryRecordTests
{
    private INamedCache GetCache(String cacheName)
    {
        IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

        IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config.xml");
        ccf.Config = config;

        return CacheFactory.GetCache(cacheName);
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

    [Test]
    public void TestQueryRecorder()
    {
        INamedCache cache = GetCache("dist-query-recorder");
        cache.Clear();

        var valueExtractor = new UniversalExtractor("City");
        try
        {
            Hashtable ht = new Hashtable();
            Address address1 = new Address("Street1", "City1", "State1", "Zip1");
            Address address2 = new Address("Street2", "City2", "State2", "Zip2");
            ht.Add("key1", address1);
            ht.Add("key2", address2);
            cache.InsertAll(ht);

            cache.AddIndex(valueExtractor, false, null);

            QueryRecorder queryRecorder = new QueryRecorder(QueryRecorder.RecordType.Explain);
            IFilter yourFilter = new ContainsFilter(new UniversalExtractor("City"), "City1");
            Object resultsExplain = cache.Aggregate(yourFilter, queryRecorder);
            Console.WriteLine(resultsExplain);
        }
        finally
        {
            cache.RemoveIndex(valueExtractor);
            CacheFactory.Shutdown();
        }
    }
}