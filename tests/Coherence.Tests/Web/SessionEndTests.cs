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
    public class SessionEndTests
    {
        [TestFixtureSetUp]
        public void InitializeSessionStore()
        {
            m_sessionCache = CacheFactory.GetCache(AbstractSessionModelManager.SESSION_CACHE_NAME);
            m_extAttrCache = CacheFactory.GetCache(SplitSessionModelManager.EXTERNAL_ATTRIBUTES_CACHE_NAME);
            m_sessionCache.Clear();
            m_extAttrCache.Clear();
            m_store = CreateSessionStore();
        }

        [SetUp]
        public void CreateSession()
        {
            // create new session with a zero timeout in order for the default 
            // store timeout specified in CreateSessionStore to take effect
            SessionStateStoreData data = m_store.CreateNewStoreData(null, 0);  

            ISessionStateItemCollection session = data.Items;
            session["int"]    = 1;
            session["string"] = "test string";
            session["date"]   = DateTime.Today;
            session["bool"]   = false;
            session["blob"]   = BLOB;
            session["person"] = PERSON;

            m_store.SetAndReleaseItemExclusive(null, SESSION_ID, data, null, true);
            m_store.ResetItemTimeout(null, SESSION_ID);

            Assert.AreEqual(1, m_sessionCache.Count);
            Assert.AreEqual(1, m_extAttrCache.Count);

            m_sessionOnEndCalled = false;
        }

        #region Tests

        [Test]
        public void TestExplicitSessionRemoval()
        {
            m_store.RemoveItem(null, SESSION_ID, null, null);
            // sleep a bit to allow Session_OnEnd event handler to execute
            Thread.Sleep(200);

            Assert.AreEqual(0, m_sessionCache.Count);
            Thread.Sleep(200);
            Assert.AreEqual(0, m_extAttrCache.Count);

            Assert.IsTrue(m_sessionOnEndCalled);
        }

        [Test]
        public void TestSessionTimeout()
        {
            // This test is failing intermittently.  Before the problem is
            // being looked at, comment out the Assert() to avoid failure.

            // sleep a bit to allow session to expire 
            Thread.Sleep(1500);
            Assert.AreEqual(0, m_sessionCache.Count);
            Thread.Sleep(200);
            Assert.AreEqual(0, m_extAttrCache.Count);
            
            // sleep to allow Session_End event handler to execute
            Thread.Sleep(200);
            Assert.IsTrue(m_sessionOnEndCalled);
        }

        #endregion

        #region Helper methods

        protected virtual CoherenceSessionStore CreateSessionStore()
        {
            CoherenceSessionStore store = new CoherenceSessionStore();
            store.ApplicationId     = "SessionEndTests";
            store.ModelManager      = new SplitSessionModelManager(new BinarySerializer(), 512);
            store.Timeout           = TimeSpan.FromMilliseconds(1000);
            store.SessionEndEnabled = true;
            
            store.SetItemExpireCallback(Session_OnEnd);

            return store;
        }

        private void Session_OnEnd(string id, SessionStateStoreData item)
        {
            m_sessionOnEndCalled = true;
        }

        protected static byte[] CreateBlob(int size)
        {
            byte[] blob = new byte[size];
            Random rnd  = new Random();
            
            rnd.NextBytes(blob);
            
            return blob;
        }

        #endregion

        #region Data members

        private const string SESSION_ID = "timout.session";

        private static readonly byte[] BLOB = CreateBlob(512);
        private static readonly Person PERSON =
            new PortablePerson("Aleksandar Seovic", new DateTime(1974, 8, 24));

        private CoherenceSessionStore m_store;
        private INamedCache m_sessionCache;
        private INamedCache m_extAttrCache;
        private bool m_sessionOnEndCalled;

        #endregion
    }
}