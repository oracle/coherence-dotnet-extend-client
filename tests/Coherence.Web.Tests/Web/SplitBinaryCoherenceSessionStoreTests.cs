/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Threading;
using System.Web.SessionState;

using NUnit.Framework;

using Tangosol.IO;
using Tangosol.Net;
using Tangosol.Web.Model;

namespace Tangosol.Web
{
    [TestFixture]
    public class SplitBinaryCoherenceSessionStoreTests 
        : AbstractCoherenceSessionStoreTests
    {
        #region Overrides of AbstractCoherenceSessionStoreTests

        protected override ISessionModelManager CreateModelManager(ISerializer serializer)
        {
            m_extAttrCache = CacheFactory.GetCache(SplitSessionModelManager.EXTERNAL_ATTRIBUTES_CACHE_NAME);
            m_extAttrCache.Clear();

            return new SplitSessionModelManager(serializer, 500);
        }

        protected override ISerializer CreateSerializer()
        {
            return new BinarySerializer();
        }

        #endregion

        [Test]
        public void TestExternalAttributeRemovalOnSessionRemoval()
        {
            CheckCacheIsReady();

            m_store.RemoveItem(null, SESSION_ID, null, null);

            // external attributes are removed asynchronously
            // sleep a bit to make sure they are removed...
            Thread.Sleep(100);
            Assert.AreEqual(0, m_extAttrCache.Count);
        }

        [Test]
        public void TestExternalAttributeReplacement()
        {
            CheckCacheIsReady();

            bool locked;
            TimeSpan lockAge;
            object lockId;
            SessionStateActions actions;

            SessionStateStoreData data =
                m_store.GetItemExclusive(null, SESSION_ID, out locked, out lockAge, out lockId, out actions);

            // replace "large" external attribute with a "small" attribute
            ISessionStateItemCollection session = data.Items;
            session["blob"] = CreateBlob(1);

            m_store.SetAndReleaseItemExclusive(null, SESSION_ID, data, lockId, false);

            Assert.AreEqual(0, m_extAttrCache.Count);
        }

        [Test]
        public void TestExternalAttributeReadBeforeWrite()
        {
            CheckCacheIsReady();

            bool locked;
            TimeSpan lockAge;
            object lockId;
            SessionStateActions actions;

            SessionStateStoreData data =
                m_store.GetItemExclusive(null, SESSION_ID, out locked, out lockAge, out lockId, out actions);

            ISessionStateItemCollection session = data.Items;
            var blob        = session["blob"];
            session["blob"] = CreateBlob(1);

            m_store.SetAndReleaseItemExclusive(null, SESSION_ID, data, lockId, false);

            Assert.AreEqual(0, m_extAttrCache.Count);
        }

        [Test]
        public void TestExternalAttributeMultipleWrites()
        {
            CheckCacheIsReady();

            bool locked;
            TimeSpan lockAge;
            object lockId;
            SessionStateActions actions;

            SessionStateStoreData data =
                m_store.GetItemExclusive(null, SESSION_ID, out locked, out lockAge, out lockId, out actions);

            ISessionStateItemCollection session = data.Items;
            session["blob"]  = CreateBlob(1);

            session["test"]  = "test";
            session["test"]  = CreateBlob(1024);
            session["test"]  = "test";

            session["test2"] = CreateBlob(1024);
            session["test2"] = "test2";

            m_store.SetAndReleaseItemExclusive(null, SESSION_ID, data, lockId, false);

            Assert.AreEqual(0, m_extAttrCache.Count);
        }

        [Test]
        public void TestExternalAttributeReplaceSmall()
        {
            CheckCacheIsReady();

            bool locked;
            TimeSpan lockAge;
            object lockId;
            SessionStateActions actions;

            SessionStateStoreData data =
                m_store.GetItemExclusive(null, SESSION_ID, out locked, out lockAge, out lockId, out actions);

            ISessionStateItemCollection session = data.Items;
            session["blob"]   = CreateBlob(1);
            session["person"] = CreateBlob(1024);

            m_store.SetAndReleaseItemExclusive(null, SESSION_ID, data, lockId, false);

            Assert.AreEqual(1, m_extAttrCache.Count);
        }

        private void CheckCacheIsReady()
        {
            try
                {
                Assert.AreEqual(1, m_extAttrCache.Count);
                }
            catch (Exception /* e */)
                {
                // sleep a bit to make sure session is initialized...
                Thread.Sleep(3000);
                Assert.AreEqual(1, m_extAttrCache.Count);
                }
        }

        protected INamedCache m_extAttrCache;
    }
}