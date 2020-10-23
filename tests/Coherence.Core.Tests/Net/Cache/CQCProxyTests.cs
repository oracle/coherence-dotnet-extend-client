/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Threading;

using NUnit.Framework;

using Tangosol;
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Run.Xml;
using Tangosol.Util.Filter;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// Coherence*Extend test for the ContinuousQueryCache receiving events
    /// after restarting the proxy server.
    /// Tests COH-8145 (Bug14768607)
    /// Tests COH-8470 (Bug15966691)
    /// </summary>
    /// <author> par 2012.11.5 </author>
    [TestFixture]
    [Ignore("Ignore Docker Test")]
    public class CQCProxyTests
    {

        private INamedCache GetCache(String cacheName)
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-cache-config.xml");
            ccf.Config          = config;

            return CacheFactory.GetCache(cacheName);
        }

    /// <summary>
    /// Test the behavior of proxy returning events.
    /// COH-8145 reports the CQC doesn't receive events after
    /// the proxy is restarted.
    /// COH-8470 reports that the CQC resynchronizes multiple
    /// times, giving double or triple events.
    ///
    /// Put a known number of data items into the inner cache,
    /// then count the number of events the listener receives
    /// after restarting the proxy.
    /// </summary>
    [Test]
    public void TestEvents()
    {
        // start the ProxyService on just one cluster node
        IInvocationService invocationService = RestartProxy(null);

        // put data items into inner cache to generate events
        INamedCache testCache = GetCache("proxy-stop-test");
        testCache.Clear();
        for (int i = 0; i < SOME_DATA; i++)
            {
            testCache.Add("TestKey" + i, i);
            }

        // create listener for CQC
        TestCQCListener listener = new TestCQCListener(SOME_DATA);

        // instantiate the CQC, will start the test running.
        theCQC = new ContinuousQueryCache(testCache, AlwaysFilter.Instance, listener);

        // instantiate a service listener to receive memberLeft event
        fMemberLeft = false;
        testCache.CacheService.MemberLeft += new MemberEventHandler(OnMemberLeft);

        // allow test time to complete.
        DateTime endTime = DateTime.Now.AddSeconds(30);
        while (listener.GetActualTotal() < SOME_DATA && (DateTime.Now < endTime))
            {
            Thread.Sleep(250);
            }

        // check listener received the correct number of events.
        Assert.AreEqual(SOME_DATA, listener.GetActualTotal());
        listener.ResetActualTotal();

        // restart proxy
        RestartProxy(invocationService);

        endTime = DateTime.Now.AddSeconds(30);
        while (!fMemberLeft && (DateTime.Now < endTime))
            {
            Thread.Sleep(250);
            }

        // ping the CQC to make it realize the cache needs restart.
        theCQC.Contains("junkstuff");
        
        // allow test time to complete.
        endTime = DateTime.Now.AddSeconds(30);
        while (listener.GetActualTotal() < SOME_DATA && (DateTime.Now < endTime))
            {
            Thread.Sleep(250);
            }

        Assert.AreEqual(SOME_DATA, listener.GetActualTotal());
    }

    /**
    * utility method to stop and restart the proxy.
    */
    private IInvocationService RestartProxy(IInvocationService service)
    {
        if (service == null)
        {
            service = (IInvocationService)CacheFactory.GetService("ExtendTcpProxyControlService");
        }

        ProxyStopInvocable invocable = new ProxyStopInvocable();
        invocable.ProxyServiceName = "ExtendTcpProxyServiceCOH8230";
        try
            {
            Object result = service.Query(invocable, null);
            }
        catch (Exception /* e */)
            {
            // ignore
            }

        return service;
    }

    /**
    * listener to receive memberLeft event
    */
    public void OnMemberLeft(object sender, MemberEventArgs evt)
    {
        fMemberLeft = true;
    }

    // ----- constants ------------------------------------------------------

    /**
    * Number of data items to put in cache; should generate same number of events.
    */
    public static int SOME_DATA = 100;


    // ----- CQCProxyTests data members ----------------------------------------

    /**
    *  The ContinuousQueryCache being tested
    */
    private INamedCache theCQC;

    /**
    *  Whether the memberLeft event has been received
    */
    private Boolean fMemberLeft;


    // ----- inner class: TestCQCListener --------------------------------------

    /**
    * MapListener that continuously receives events from the cache.
    */
    #region Helper class

    class TestCQCListener : ICacheListener
        {

        public TestCQCListener(int count)
            {
            m_cCount         = count;
            m_cActualInserts = 0;
            m_cActualUpdates = 0;
            m_cActualDeletes = 0;
            }

        public int Count
            {
                get { return m_cCount; }
                set { m_cCount = value; }
            }

        /**
        * Number of insert events listener actually received.
        *
        * @return  number of event received
        */
        public int ActualInserts
            {
                get { return m_cActualInserts; }
                set { m_cActualInserts = value; }
            }

        /**
        * Number of update events listener actually received.
        *
        * @return  number of event received
        */
        public int ActualUpdates
            {
                get { return m_cActualUpdates; }
                set { m_cActualUpdates = value; }
            }

        /**
        * Number of delete events listener actually received.
        *
        * @return  number of event received
        */
        public int ActualDeletes
            {
                get { return m_cActualDeletes; }
                set { m_cActualDeletes = value; }
            }

        public void EntryUpdated(CacheEventArgs evt)
            {
            m_cActualUpdates++;
            }

        public void EntryInserted(CacheEventArgs evt)
            {
            m_cActualInserts++;
            }

        public void EntryDeleted(CacheEventArgs evt)
            {
            m_cActualDeletes++;
            }

        /**
        * Total number of events listener actually received.
        *
        * @return  number of event received
        */
        public int GetActualTotal()
            {
            return m_cActualInserts+m_cActualUpdates+m_cActualDeletes;
            }

        /**
        * Reset the number of events received.
        *
        */
        public void ResetActualTotal()
            {
            m_cActualUpdates = 0;
            m_cActualInserts = 0;
            m_cActualDeletes = 0;
            }


        // ----- data members -----------------------------------------------

        /**
        * Number of insert events actually received
        */
        int m_cActualInserts;

        /**
        * Number of update events actually received
        */
        int m_cActualUpdates;

        /**
        * Number of delete events actually received
        */
        int m_cActualDeletes;

        /**
        * Number of events listener should receive
        */
        int m_cCount;
        }
        #endregion
    }
}
