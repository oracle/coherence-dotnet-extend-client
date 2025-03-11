/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.Threading;

using NUnit.Framework;

using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// Functional tests for using cache with ThreadTimeout.
    /// </summary>
    /// <author>lh 2.23.22</author>
    /// <since>14.1.2.0</since>
    [TestFixture]
    public class CachesTimeoutTest
    {
        private static INamedCache GetCache(String cacheName)
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

        /**
         * Test concurrent GetCache() behavior. If a winner thread took the
         * lock and ensured the cache, other threads should only block on 
         * lock for the specified timeout.
         */
        [Test]
        public void testShouldTimeoutWithGetCache()
        {
            CountDownLatch latch1   = new CountDownLatch(1);
            CountDownLatch latch2   = new CountDownLatch(1);
            Exception      e1       = null;
            Exception      e2       = new ThreadInterruptedException();
            long           duration = 0;

            Thread thread1 = new Thread(() =>
            {
                latch1.Wait();

                try
                {
                    using (ThreadTimeout t = ThreadTimeout.After(10000))
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        CacheFactory.GetCache("dist-timeout-cache1");
                        stopwatch.Stop();
                        duration = stopwatch.ElapsedMilliseconds;
                        Console.Out.WriteLine("#### CachesTimeoutTest.testShouldTimeoutWithGetCache(), GetCache() took: " + duration + "ms.");
                    }
                }
                catch (Exception e)
                {
                    e1 = e;
                }
            });

            Thread thread2 = new Thread(() =>
            {
                latch2.Wait();

                try
                {
                    using (ThreadTimeout t = ThreadTimeout.After(1000))
                    {
                        CacheFactory.GetCache("dist-timeout-cache2");
                    }
                }
                catch (Exception e)
                {
                    e2 = e;
                }
            });

            thread1.Start();
            thread2.Start();
            Blocking.Sleep(100);

            // ensure task1 is the lock winner
            latch1.CountDown();
            Blocking.Sleep(50);
            latch2.CountDown();

            thread1.Join();
            thread2.Join();

            if (e1 != null)
            {
                Assert.Fail("CacheFactory.getCache() failed to get the cache!");
            }

            if (e2 == null)
            {
                Assert.Fail("CacheFactory.GetCache() should failed with RequestTimeoutException!");
            }
            else
            {
                Assert.IsTrue(e2 is ThreadInterruptedException);
            }
        }
    }

    #region Helper classes
    internal class CountDownLatch
    {
        private int m_remain;
        private EventWaitHandle m_event;

        public CountDownLatch(int count)
        {
            m_remain = count;
            m_event = new ManualResetEvent(false);
        }

        public void CountDown()
        {
            // The last thread to signal also sets the event.
            if (Interlocked.Decrement(ref m_remain) == 0)
                m_event.Set();
        }

        public void Wait()
        {
            m_event.WaitOne();
        }
    }
    #endregion
}

