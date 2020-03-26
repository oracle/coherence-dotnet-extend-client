/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections;

using NUnit.Framework;

using Tangosol.Run.Xml;
using Tangosol.Util.Aggregator;
using Tangosol.Util.Filter;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    [TestFixture]
    public class PriorityTaskTests
    {
        #region Properties

        private INamedCache m_cache;

        private readonly String m_key = "Foo-Key";

        private readonly String m_expectedResult = "Foo-Result";

        private PriorityProcessor m_priorityProcessorDefault;

        private PriorityProcessor m_priorityProcessorNone;

        private PriorityAggregator m_priorityAggregatorDefault;

        private PriorityAggregator m_priorityAggregatorNone;

        #endregion 

        [TestFixtureSetUp]
        public void Init()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-timeout-cache-config5.xml");
            ccf.Config = config;

            m_cache = CacheFactory.GetCache("dist-test");

            m_cache.Clear();

            m_cache[m_key] = 10.0;

            SlowProcessor slowProcessor = new SlowProcessor
                {
                        Time = 10000,
                        ReturnValue = m_expectedResult
                };

            
            m_priorityProcessorDefault = new PriorityProcessor(slowProcessor)
                {
                        RequestTimeoutMillis = (long)PriorityTaskTimeout.Default
                };

            m_priorityProcessorNone = new PriorityProcessor(slowProcessor)
                {
                        RequestTimeoutMillis = (long)PriorityTaskTimeout.None
                };

            SlowAggregator slowAggregator = new SlowAggregator();

            m_priorityAggregatorDefault = new PriorityAggregator(slowAggregator)
                {
                        RequestTimeoutMillis = (long)PriorityTaskTimeout.Default
                };

            m_priorityAggregatorNone = new PriorityAggregator(slowAggregator)
            {
                RequestTimeoutMillis = (long)PriorityTaskTimeout.None
            };
        }


        [TestFixtureTearDown]
        public void Cleanup()
        {
            CacheFactory.Shutdown();    
        }


        [Test]
        public void ShouldRunInvokeAndTimeOut()
        {
            Assert.Throws(Is.TypeOf(typeof(RequestTimeoutException)), () => m_cache.Invoke(m_key, m_priorityProcessorDefault));
        }


        [Test]
        public void ShouldRunInvokeAndNotTimeout()
        {
            String returnValue = (String)m_cache.Invoke(m_key, m_priorityProcessorNone);

            Assert.That(returnValue, Is.EqualTo(m_expectedResult));
        }


        [Test]
        public void ShouldRunInvokeAllAndTimeout()
        {
            ArrayList keys = new ArrayList {"Foo-Key"};

            Assert.Throws(Is.TypeOf(typeof(RequestTimeoutException)), () => m_cache.InvokeAll(keys, m_priorityProcessorDefault));
        }


        [Test]
        public void ShouldRunInvokeAllAndNotTimeout()
        {
            ArrayList keys = new ArrayList {"Foo-Key"};

            IDictionary returnValue = m_cache.InvokeAll(keys, m_priorityProcessorNone);

            Assert.That(returnValue["Foo-Key"], Is.EqualTo(m_expectedResult));
        }


        [Test]
        public void ShouldRunInvokeFilterAndTimeout()
        {
            Assert.Throws(Is.TypeOf(typeof(RequestTimeoutException)), () => m_cache.InvokeAll(AlwaysFilter.Instance, m_priorityProcessorDefault));
        }


        [Test]
        public void ShouldRunInvokeFilterAndNotTimeout()
        {
            IDictionary returnValue = m_cache.InvokeAll(AlwaysFilter.Instance, m_priorityProcessorNone);

            Assert.That(returnValue["Foo-Key"], Is.EqualTo(m_expectedResult));
        }


        [Test]
        public void ShouldRunAggregateKeysAndTimeout()
        {
            ArrayList keys = new ArrayList {"Foo-Key"};

            Assert.Throws(Is.TypeOf(typeof(RequestTimeoutException)), () => m_cache.Aggregate(keys, m_priorityAggregatorDefault));
        }


        [Test]
        public void ShouldRunAggregateKeysAndNotTimeout()
        {
            ArrayList keys = new ArrayList {"Foo-Key"};

            double returnValue = (double) m_cache.Aggregate(keys, m_priorityAggregatorNone);

            Assert.That(returnValue, Is.EqualTo(10.0));
        }


        [Test]
        public void ShouldRunAggregateFilterAndTimeout()
        {
            Assert.Throws(Is.TypeOf(typeof(RequestTimeoutException)), () => m_cache.Aggregate(AlwaysFilter.Instance, m_priorityAggregatorDefault));
        }


        [Test]
        public void ShouldRunAggregateFilterAndNotTimeout()
        {
            double returnValue = (double) m_cache.Aggregate(AlwaysFilter.Instance, m_priorityAggregatorNone);

            Assert.That(returnValue, Is.EqualTo(10.0));
        }
    }
}
