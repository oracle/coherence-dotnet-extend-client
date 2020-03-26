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
    public abstract class AbstractCoherenceSessionStoreTests : WebTestUtil
    {
        #region Abstract methods

        protected abstract ISessionModelManager CreateModelManager(ISerializer serializer);

        protected abstract ISerializer CreateSerializer();

        #endregion

        #region Setup and Teardown

        [TestFixtureSetUp]
        public void InitializeSessionStore()
        {
            m_sessionCache = CacheFactory.GetCache(AbstractSessionModelManager.SESSION_CACHE_NAME);
            m_sessionCache.Clear();
            m_store = CreateSessionStore();
        }

        [SetUp]
        public void CreateSession()
        {
            SessionStateStoreData data = m_store.CreateNewStoreData(null, 20);

            ISessionStateItemCollection session = data.Items;
            session["int"]    = 1;
            session["string"] = "test string";
            session["date"]   = DateTime.Today;
            session["bool"]   = false;
            session["blob"]   = BLOB;
            session["person"] = PERSON;

            m_store.SetAndReleaseItemExclusive(null, SESSION_ID, data, null, true);
            Assert.AreEqual(1, m_sessionCache.Count);
        }

        [TearDown]
        public void RemoveSession()
        {
            m_store.RemoveItem(null, SESSION_ID, null, null);
            Assert.AreEqual(0, m_sessionCache.Count);
        }

        #endregion

        #region Tests

        [Test]
        public void TestNonExclusiveReadFromSession()
        {
            bool     locked;
            TimeSpan lockAge;
            object   lockId;
            SessionStateActions actions;

            SessionStateStoreData data = 
                m_store.GetItem(null, SESSION_ID, out locked, out lockAge, out lockId, out actions);    

            Assert.IsFalse(locked);
            Assert.AreEqual(TimeSpan.Zero, lockAge);
            Assert.IsNull(lockId);
            Assert.AreEqual(SessionStateActions.None, actions);

            ISessionStateItemCollection session = data.Items;
            AssertSessionDefaults(session);
        }

        [Test]
        public void TestExclusiveReadFromSession()
        {
            bool     locked;
            TimeSpan lockAge;
            object   lockId;
            SessionStateActions actions;

            SessionStateStoreData data =
                m_store.GetItemExclusive(null, SESSION_ID, out locked, out lockAge, out lockId, out actions);

            Assert.IsTrue(locked);
            Assert.IsTrue(lockAge >= TimeSpan.Zero);
            Assert.IsNotNull(lockId);
            Assert.AreEqual(SessionStateActions.None, actions);

            ISessionStateItemCollection session = data.Items;
            AssertSessionDefaults(session);

            m_store.ReleaseItemExclusive(null, SESSION_ID, lockId);
        }

        [Test]
        public void TestExclusiveReadFromLockedSession()
        {
            bool     locked;
            TimeSpan lockAge;
            object   lockId;
            SessionStateActions actions;

            SessionStateStoreData data =
                m_store.GetItemExclusive(null, SESSION_ID, out locked, out lockAge, out lockId, out actions);

            Assert.IsNotNull(data);
            Assert.IsTrue(locked);
            Assert.IsTrue(lockAge >= TimeSpan.Zero);
            Assert.IsNotNull(lockId);
            Assert.AreEqual(SessionStateActions.None, actions);

            Thread.Sleep(50);

            TimeSpan lockAge2;
            object lockId2;
            data = m_store.GetItemExclusive(null, SESSION_ID, out locked, out lockAge2, out lockId2, out actions);
            
            Assert.IsNull(data);
            Assert.IsTrue(locked);
            Assert.IsTrue(lockAge2 > lockAge);
            Assert.AreEqual(lockId, lockId2);
            
            m_store.ReleaseItemExclusive(null, SESSION_ID, lockId);
        }

        [Test]
        public void TestSessionModification()
        {
            bool     locked;
            TimeSpan lockAge;
            object   lockId;
            SessionStateActions actions;

            SessionStateStoreData data =
                m_store.GetItemExclusive(null, SESSION_ID, out locked, out lockAge, out lockId, out actions);

            Assert.IsTrue(locked);
            Assert.IsTrue(lockAge >= TimeSpan.Zero);
            Assert.IsNotNull(lockId);
            Assert.AreEqual(SessionStateActions.None, actions);

            ISessionStateItemCollection session = data.Items;
            AssertSessionDefaults(session);

            session["int"]    = 2;
            session["string"] = "modified string";
            session["date"]   = DateTime.Now;
            session["bool"]   = true;
            session["blob"]   = CreateBlob(1024);
            session["person"] = new PortablePerson("Novak Seovic", new DateTime(2007, 12, 28));

            m_store.SetAndReleaseItemExclusive(null, SESSION_ID, data, lockId, false);

            data = m_store.GetItem(null, SESSION_ID, out locked, out lockAge, out lockId, out actions);

            Assert.IsFalse(locked);
            Assert.AreEqual(TimeSpan.Zero, lockAge);
            Assert.IsNull(lockId);
            Assert.AreEqual(SessionStateActions.None, actions);

            session = data.Items;
            Assert.AreEqual(2, session["int"]);
            Assert.AreEqual("modified string", session["string"]);
            Assert.AreNotEqual(DateTime.Today, session["date"]);
            Assert.AreEqual(true, session["bool"]);
            Assert.AreNotEqual(BLOB, session["blob"]);
            Assert.AreNotEqual(PERSON, session["person"]);
        }

        [Test]
        public void TestSessionAttributeRemoval()
        {
            bool     locked;
            TimeSpan lockAge;
            object   lockId;
            SessionStateActions actions;

            SessionStateStoreData data =
                m_store.GetItemExclusive(null, SESSION_ID, out locked, out lockAge, out lockId, out actions);

            Assert.IsTrue(locked);
            Assert.IsTrue(lockAge >= TimeSpan.Zero);
            Assert.IsNotNull(lockId);
            Assert.AreEqual(SessionStateActions.None, actions);

            ISessionStateItemCollection session = data.Items;
            AssertSessionDefaults(session);

            session.Remove("INT");
            session.Remove("BLOB");

            m_store.SetAndReleaseItemExclusive(null, SESSION_ID, data, lockId, false);

            data = m_store.GetItem(null, SESSION_ID, out locked, out lockAge, out lockId, out actions);

            Assert.IsFalse(locked);
            Assert.AreEqual(TimeSpan.Zero, lockAge);
            Assert.IsNull(lockId);
            Assert.AreEqual(SessionStateActions.None, actions);

            session = data.Items;
            Assert.IsNull(session["int"]);
            Assert.IsNull(session["blob"]);
        }

        #endregion

        #region Helper methods

        protected virtual CoherenceSessionStore CreateSessionStore()
        {
            CoherenceSessionStore store = new CoherenceSessionStore();
            store.ApplicationId = "UnitTests";
            store.ModelManager  = CreateModelManager(CreateSerializer());
            store.Timeout       = TimeSpan.FromMinutes(1);
            return store;
        }

        protected void AssertSessionDefaults(ISessionStateItemCollection session)
        {
            Assert.AreEqual(1, session["int"]);
            Assert.AreEqual("test string", session["string"]);
            Assert.AreEqual(DateTime.Today, session["date"]);
            Assert.AreEqual(false, session["bool"]);
            try 
                {
                Assert.AreEqual(BLOB, session["blob"]);
                }
            catch (Exception /* e */)
                {
                // sleep a bit to make sure blob got into the external cache...
                Thread.Sleep(100);
                Assert.AreEqual(BLOB, session["blob"]);
                }
            Assert.AreEqual(PERSON.Name, ((PortablePerson) session["person"]).Name);
        }

        #endregion

        #region Data members

        protected const string SESSION_ID = "test.session";
        
        protected static readonly byte[] BLOB   = CreateBlob(512);
        protected static readonly Person PERSON = CreatePerson();

        protected CoherenceSessionStore m_store;
        protected INamedCache m_sessionCache;

        #endregion
    }
}