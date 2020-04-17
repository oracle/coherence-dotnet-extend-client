/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.IO;

using NUnit.Framework;

using Tangosol.Run.Xml;

namespace Tangosol.Net
{
    [TestFixture]
    public class MessageSizeTests
    {
        INamedCache namedCache;

        [SetUp]
        public void SetUp()
        {
            var ccf    = CacheFactory.ConfigurableCacheFactory;
            var config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-cache-config-msg-size.xml");
            ccf.Config = config;

            namedCache = CacheFactory.GetCache("dist-extend-direct");
        }

        [TearDown]
        public void TearDown()
        {
            namedCache.CacheService.Shutdown();
            CacheFactory.Shutdown();
        }

        [Test]
        /// <summary>
        /// Test CacheData.
        /// Put and get a set of data from cache.
        /// </summary>
        public void TestCacheData()
        {
            var map  = new Hashtable();
            var keys = new ArrayList();
            for (int i = 0; i < 100; i++)
            {
                string key = "key" + i;
                keys.Add(key);
                map.Add(key, i);
            }

            namedCache.Clear();
            namedCache.InsertAll(map);
            ICollection entrySet = namedCache.GetAll(keys);
            Assert.AreEqual(100, entrySet.Count);
            namedCache.Release();
        }

        /// <summary>
        /// Test GetLargeData.
        /// Retrieve a large set of data from cache. The result message exceeds
        /// the max-message-size of the initiator's incoming message handler.
        /// </summary>
        [Test]
        [Ignore("Ignore Docker Test")]
        [ExpectedException(typeof(IOException))]
        public void TestGetLargeData()
        {
            var map = new Hashtable();
            var keys = new ArrayList();
            for (int i = 0; i < 1000; i++)
            {
                string key = "key" + i;
                keys.Add(key);
                map.Add(key, i);
            }

            namedCache.Clear();
            namedCache.InsertAll(map);
            namedCache.GetAll(keys);
        }

        /// <summary>
        /// Test PutLargeData.
        /// Put a large set of data to cache. The request message exceeds the
        /// max-message-size of the initiator's outgoing message handler.
        /// </summary>
        [Test]
        [Ignore("Ignore Docker Test")]
        [ExpectedException(typeof(IOException))]
        public void TestPutLargeData()
        {
            var map  = new Hashtable();
            for (int i = 0; i < 1500; i++)
            {
                map.Add("key"+i, i);
            }

            namedCache.Clear();
            namedCache.InsertAll(map);
        }
    }
}
