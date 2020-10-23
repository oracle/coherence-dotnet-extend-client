/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;

using NUnit.Framework;

using Tangosol.Net.Cache;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;

namespace Tangosol.Net
{
    /// <summary>
    /// Unit tests for ViewBuilder.
    /// </summary>
    /// <author>rl 6.4.19</author>
    /// <since>12.2.1.4</since>
    [TestFixture]
    public class ViewBuilderTests
    {
        #region Test methods

        [Test]
        public void TestViewBuilderDefaults()
        {
            INamedCache cache = new ViewBuilder(GetCache("v-defaults")).Build();
            Assert.IsInstanceOf(typeof(ContinuousQueryCache), cache);

            ContinuousQueryCache queryCache = (ContinuousQueryCache) cache;

            Assert.That(queryCache.Filter,            Is.EqualTo(AlwaysFilter.Instance));
            Assert.That(queryCache.IsReadOnly,        Is.False);
            Assert.That(queryCache.CacheListener,     Is.Null);
            Assert.That(queryCache.Transformer,       Is.Null);
            Assert.That(queryCache.ReconnectInterval, Is.EqualTo(0L));
            Assert.That(queryCache.CacheNameSupplier, Is.Null);
            Assert.That(queryCache.CacheValues,       Is.False);
        }

        [Test]
        public void TestViewBuilderFromNamedCache()
        {
            INamedCache backCache = GetCache("v-fromNamedCache");
            INamedCache cache = backCache.View().Build();
            Assert.IsInstanceOf(typeof(ContinuousQueryCache), cache);

            ContinuousQueryCache queryCache = (ContinuousQueryCache)cache;

            Assert.That(queryCache.Filter,            Is.EqualTo(AlwaysFilter.Instance));
            Assert.That(queryCache.IsReadOnly,        Is.False);
            Assert.That(queryCache.CacheListener,     Is.Null);
            Assert.That(queryCache.Transformer,       Is.Null);
            Assert.That(queryCache.ReconnectInterval, Is.EqualTo(0L));
            Assert.That(queryCache.CacheNameSupplier, Is.Null);
            Assert.That(queryCache.CacheValues,       Is.False);
        }

        [Test]
        public void TestViewBuilderWithFilter()
        {
            IFilter     filter = new EqualsFilter("foo", "bar");
            INamedCache cache  = new ViewBuilder(GetCache("v-filter")).Filter(filter).Build();
            Assert.IsInstanceOf(typeof(ContinuousQueryCache), cache);

            ContinuousQueryCache queryCache = (ContinuousQueryCache)cache;

            Assert.That(queryCache.Filter, Is.EqualTo(filter));
        }

        [Test]
        public void TestViewBuilderWithListener()
        {
            ICacheListener listener = new TestCacheListener();
            INamedCache    cache    = new ViewBuilder(GetCache("v-listener")).Listener(listener).Build();
            Assert.IsInstanceOf(typeof(ContinuousQueryCache), cache);

            ContinuousQueryCache queryCache = (ContinuousQueryCache)cache;

            Assert.That(queryCache.CacheListener, Is.EqualTo(listener));
        }

        [Test]
        public void TestViewBuilderWithTransformer()
        {
            IValueExtractor transformer = new IdentityExtractor();
            INamedCache     cache       = new ViewBuilder(GetCache("v-transformer")).Map(transformer).Build();
            Assert.IsInstanceOf(typeof(ContinuousQueryCache), cache);

            ContinuousQueryCache queryCache = (ContinuousQueryCache)cache;

            Assert.That(queryCache.Transformer, Is.EqualTo(transformer));
        }

        [Test]
        public void TestViewBuilderCacheValues()
        {
            INamedCache cache = new ViewBuilder(GetCache("v-cachingValues")).Values().Build();
            Assert.IsInstanceOf(typeof(ContinuousQueryCache), cache);

            ContinuousQueryCache queryCache = (ContinuousQueryCache)cache;

            Assert.That(queryCache.CacheValues, Is.True);
        }

        [Test]
        public void TestViewBuilderKeysOnly()
        {
            IValueExtractor transformer = new IdentityExtractor();
            INamedCache cache = new ViewBuilder(GetCache("v-keysOnly")).Keys().Build();
            Assert.IsInstanceOf(typeof(ContinuousQueryCache), cache);

            ContinuousQueryCache queryCache = (ContinuousQueryCache)cache;

            Assert.That(queryCache.CacheValues, Is.False);
        }

        #endregion

        #region Helper methods

        private INamedCache GetCache(String cacheName)
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            ccf.Config = config;

            return CacheFactory.GetCache(cacheName);
        }

        #endregion
    }
}