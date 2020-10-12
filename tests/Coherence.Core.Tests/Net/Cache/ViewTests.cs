/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using NUnit.Framework;
using Tangosol.Net.Messaging.Impl.CacheService;
using Tangosol.Run.Xml;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;

namespace Tangosol.Net.Cache
{
    public class ViewTests
    {
        #region Test Methods

        [Test]
        public void ShouldUseCustomFilter()
        {
            ContinuousQueryCache cache = ValidateIsCqc(GetCache("view-filter"));
            Assert.That(cache.Filter, Is.EqualTo(NeverFilter.Instance));
            Assert.That(cache.Transformer, Is.Null);
            Assert.That(cache.CacheListener, Is.Null);
            Assert.That(cache.ReconnectInterval, Is.EqualTo(0L));
            Assert.That(cache.IsReadOnly, Is.False);
        }
        
        [Test]
        public void ShouldUseCustomFilterWithInitParams()
        {
            ContinuousQueryCache cache = ValidateIsCqc(GetCache("view-filter-with-params"));
            Assert.That(cache.Filter, Is.EqualTo(new GreaterFilter("foo", 10)));
            Assert.That(cache.Transformer, Is.Null);
            Assert.That(cache.CacheListener, Is.Null);
            Assert.That(cache.ReconnectInterval, Is.EqualTo(0L));
            Assert.That(cache.IsReadOnly, Is.False);
        }
        
        [Test]
        public void ShouldThrowIfConfiguredFilterIsInvalid()
        {
            Assert.That(() => ValidateIsCqc(GetCache("view-filter-invalid")), Throws.Exception);
        }
        
        [Test]
        public void ShouldUseCustomTransformer()
        {
            ContinuousQueryCache cache = ValidateIsCqc(GetCache("view-transformer"));
            Assert.That(cache.Filter, Is.EqualTo(AlwaysFilter.Instance));
            Assert.That(cache.Transformer, Is.EqualTo(IdentityExtractor.Instance));
            Assert.That(cache.CacheListener, Is.Null);
            Assert.That(cache.ReconnectInterval, Is.EqualTo(0L));
            Assert.That(cache.IsReadOnly, Is.True);
        }
        
        [Test]
        public void ShouldUseCustomTransformerWithInitParams()
        {
            ContinuousQueryCache cache = ValidateIsCqc(GetCache("view-transformer-with-params"));
            Assert.That(cache.Filter, Is.EqualTo(AlwaysFilter.Instance));
            Assert.That(cache.Transformer, Is.EqualTo(new KeyExtractor("foo")));
            Assert.That(cache.CacheListener, Is.Null);
            Assert.That(cache.ReconnectInterval, Is.EqualTo(0L));
            Assert.That(cache.IsReadOnly, Is.True);
        }
        
        [Test]
        public void ShouldUseConfiguredReadOnly()
        {
            ContinuousQueryCache cache = ValidateIsCqc(GetCache("view-read-only"));
            Assert.That(cache.Filter, Is.EqualTo(AlwaysFilter.Instance));
            Assert.That(cache.Transformer, Is.Null);
            Assert.That(cache.CacheListener, Is.Null);
            Assert.That(cache.ReconnectInterval, Is.EqualTo(0L));
            Assert.That(cache.IsReadOnly, Is.True);
        }
        
        [Test]
        public void ShouldUseConfiguredReconnectInterval()
        {
            ContinuousQueryCache cache = ValidateIsCqc(GetCache("view-reconnect-interval"));
            Assert.That(cache.Filter, Is.EqualTo(AlwaysFilter.Instance));
            Assert.That(cache.Transformer, Is.Null);
            Assert.That(cache.CacheListener, Is.Null);
            Assert.That(cache.ReconnectInterval, Is.EqualTo(1000L));
            Assert.That(cache.IsReadOnly, Is.False);
        }
        
        [Test]
        public void ShouldUseConfiguredListener()
        {
            ContinuousQueryCache cache = ValidateIsCqc(GetCache("view-with-listener"));
            Assert.That(cache.Filter, Is.EqualTo(AlwaysFilter.Instance));
            Assert.That(cache.Transformer, Is.Null);
            Assert.That(cache.CacheListener, Is.EqualTo(new TestCacheListener()));
            Assert.That(cache.ReconnectInterval, Is.EqualTo(0L));
            Assert.That(cache.IsReadOnly, Is.False);
        }
        
        [Test]
        public void ShouldUseCustomFilterWithInitParamsAndMacro()
        {
            ContinuousQueryCache cache = ValidateIsCqc(GetCache("view-with-macro"));
            Assert.That(cache.Filter, Is.EqualTo(new GreaterFilter("foo", 50)));
            Assert.That(cache.Transformer, Is.Null);
            Assert.That(cache.CacheListener, Is.Null);
            Assert.That(cache.ReconnectInterval, Is.EqualTo(0L));
            Assert.That(cache.IsReadOnly, Is.False);
        }
        
        #endregion
        
        #region Helper Methods

        private INamedCache GetCache(String cacheName)
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-view-cache-config.xml");
            ccf.Config          = config;

            return CacheFactory.GetCache(cacheName);    
        }

        private ContinuousQueryCache ValidateIsCqc(INamedCache cache)
        {
            Assert.That(cache.CacheService.Info.ServiceType, Is.EqualTo(ServiceType.RemoteCache));
            Assert.That(cache, Is.InstanceOf(typeof(ContinuousQueryCache)));
            return (ContinuousQueryCache) cache;
        }
        
        #endregion
    }
}