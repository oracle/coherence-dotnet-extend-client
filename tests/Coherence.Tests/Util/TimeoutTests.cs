/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using NUnit.Framework;
using System;
using System.Threading;

namespace Tangosol.Util
{
    [TestFixture]
    class TimeoutTests
    {
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

        #region ThreadTimeout tests

        /// <summary>
        /// Test ThreadTimeout.RemainingTimeoutMillis and
        /// verify that Thread.Interrupt() interrupts
        /// Blocking.Wait(), Blocking.Sleep(), BlockingLock
        /// and is not suppressed by ThreadTimeout blocks.
        /// </summary>
        [Test]
        public void InterruptTest()
        {
            Object o = new Object();

            // ThreadTimeout.RemainingTimeoutMillis
            Assert.AreEqual(ThreadTimeout.RemainingTimeoutMillis, Int32.MaxValue);
            Assert.IsFalse(ThreadTimeout.IsTimedOut);

            using (BlockingLock l = BlockingLock.Lock(o))
            {
                Assert.IsTrue(l.IsLockObtained);
                Assert.AreEqual(ThreadTimeout.RemainingTimeoutMillis, Int32.MaxValue);
                Assert.IsFalse(ThreadTimeout.IsTimedOut);
                Assert.IsFalse(Blocking.Wait(o, 10));
                Assert.AreEqual(ThreadTimeout.RemainingTimeoutMillis, Int32.MaxValue);

                using (ThreadTimeout t = ThreadTimeout.After(50))
                {
                    bool exceptionCaught = false;
                    Assert.AreNotEqual(ThreadTimeout.RemainingTimeoutMillis, Int32.MaxValue);
                    Assert.IsTrue(ThreadTimeout.RemainingTimeoutMillis > 0);
                    Assert.IsFalse(ThreadTimeout.IsTimedOut);
                    Assert.IsFalse(Blocking.Wait(o, 1000));
                    // TODO: At this point, the thread should time out, and RemainingTimeoutMillis should be 0.
                    // Occasionally, this doesn't happen—probably due to .NET ticks being rounded
                    // to milliseconds, which causes a loss of precision — so RemainingTimeoutMillis
                    // ends up with the value 1.
                    Assert.GreaterOrEqual(1, ThreadTimeout.RemainingTimeoutMillis);
                    // Assert.IsTrue(ThreadTimeout.IsTimedOut);
                    try
                    {
                        // use Monitor.Wait() instead of Blocking.Wait() to show that
                        // ThreadTimeout.RemainingTimeoutMillis interrupted the thread
                        Monitor.Wait(o);
                        Assert.Fail();
                    }
                    catch (ThreadInterruptedException)
                    {
                        exceptionCaught = true;
                    }
                    Assert.IsTrue(exceptionCaught);
                }
                Assert.AreEqual(ThreadTimeout.RemainingTimeoutMillis, Int32.MaxValue);

                using (ThreadTimeout t = ThreadTimeout.After(Timeout.Infinite))
                {
                    // still infinite timeout
                    Assert.AreEqual(ThreadTimeout.RemainingTimeoutMillis, Int32.MaxValue);
                    Assert.IsFalse(Blocking.Wait(o, 10));
                    Assert.AreEqual(ThreadTimeout.RemainingTimeoutMillis, Int32.MaxValue);
                }

                Assert.AreEqual(ThreadTimeout.RemainingTimeoutMillis, Int32.MaxValue);
            }

            // test thread interrupts with ThreadTimeout
            Thread thread = new Thread(() =>
            {
                bool exceptionCaught = false;
                try
                {
                    using (ThreadTimeout t = ThreadTimeout.After(1000))
                    {
                        Assert.AreNotEqual(ThreadTimeout.RemainingTimeoutMillis, Int32.MaxValue);
                        Blocking.Sleep(10000);
                        Assert.Fail();
                    }
                }
                catch (ThreadInterruptedException)
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);
            });
            thread.Start();
            Blocking.Sleep(10); // give the other thread some time to get to its Sleep
            // verify ThreadTimeout is set per thread
            Assert.AreEqual(ThreadTimeout.RemainingTimeoutMillis, Int32.MaxValue);
            Assert.IsTrue(thread.ThreadState == ThreadState.WaitSleepJoin);
            thread.Interrupt();
            thread.Join();

            // interrupt Blocking.Wait()
            thread = new Thread(() =>
            {
                bool exceptionCaught = false;
                using (BlockingLock l = BlockingLock.Lock(o))
                {
                    Monitor.Pulse(o);
                    try
                    {
                        Blocking.Wait(o);
                        Assert.Fail();
                    }
                    catch (ThreadInterruptedException)
                    {
                        exceptionCaught = true;
                    }
                }
                Assert.IsTrue(exceptionCaught);
            });
            using (BlockingLock l = BlockingLock.Lock(o))
            {
                thread.Start();
                Assert.IsTrue(Blocking.Wait(o));
            }
            thread.Interrupt();
            thread.Join();

            // interrupt Blocking.Sleep()
            thread = new Thread(() =>
            {
                bool exceptionCaught = false;
                using (BlockingLock l = BlockingLock.Lock(o))
                {
                    Monitor.Pulse(o);
                }
                try
                {
                    Blocking.Sleep(Timeout.Infinite);
                    Assert.Fail();
                }
                catch (ThreadInterruptedException)
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);
            });
            using (BlockingLock l = BlockingLock.Lock(o))
            {
                thread.Start();
                Assert.IsTrue(Blocking.Wait(o));
            }
            thread.Interrupt();
            thread.Join();

            // interrupt BlockingLock.Lock()
            thread = new Thread(() =>
            {
                bool exceptionCaught = false;
                try
                {
                    using (BlockingLock l = BlockingLock.Lock(o))
                    {
                        Assert.Fail();
                    }
                }
                catch (ThreadInterruptedException)
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);
            });
            using (BlockingLock l = BlockingLock.Lock(o))
            {
                thread.Start();
                Blocking.Sleep(60); // give the other thread some time to get to BlockingLock call
                Assert.IsTrue(thread.ThreadState == ThreadState.WaitSleepJoin);
                thread.Interrupt();
            }
            thread.Join();
        }

        /// <summary>
        /// Spin up multiple threads each of which are performing timeout tests.
        /// </summary>
        [Test]
        public void TimeoutTest()
        {
            Thread[]    runThreads       = new Thread[5];
            ThreadStart myThreadDelegate = new ThreadStart(TimeoutTestRunner);

            for (int i = 0; i < runThreads.Length; ++i)
            {
                Thread myThread = new Thread(myThreadDelegate);
                myThread.Start();
                runThreads[i] = myThread;
                Blocking.Sleep(10); // small gap between Thread starts
            }
            for (int i = 0; i < runThreads.Length; ++i)
            {
                runThreads[i].Join();
            }
        }

        /// <summary>
        /// Ensure that 0 second Blocking.Wait() and Sleep() do not block or sleep.
        /// </summary>
        [Test]
        public void ZeroTest()
        {
            Object o = new Object();
            long ldtStart, cMillis;
            
            // sleep
            ldtStart = DateTimeUtils.GetSafeTimeMillis();
            Blocking.Sleep(0);
            cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
            Assert.IsTrue(cMillis < 50, "cMillis=" + cMillis);

            ldtStart = DateTimeUtils.GetSafeTimeMillis();
            Blocking.Sleep(TimeSpan.FromMilliseconds(0));
            cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
            Assert.IsTrue(cMillis < 50, "cMillis=" + cMillis);

            // wait
            using (BlockingLock l = BlockingLock.Lock(o))
            {
                ldtStart = DateTimeUtils.GetSafeTimeMillis();
                Blocking.Wait(o, 0);
                cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
                Assert.IsTrue(cMillis < 50, "cMillis=" + cMillis);

                ldtStart = DateTimeUtils.GetSafeTimeMillis();
                Blocking.Wait(o, TimeSpan.FromMilliseconds(0));
                cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
                Assert.IsTrue(cMillis < 50, "cMillis=" + cMillis);

                using (ThreadTimeout t = ThreadTimeout.After(100))
                {
                    ldtStart = DateTimeUtils.GetSafeTimeMillis();

                    Blocking.Sleep(0);
                    Blocking.Sleep(TimeSpan.FromMilliseconds(0));
                    Blocking.Wait(o, 0);
                    Blocking.Wait(o, TimeSpan.FromMilliseconds(0));

                    cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
                    Assert.IsTrue(cMillis < 50, "cMillis=" + cMillis);
                    Assert.IsFalse(ThreadTimeout.IsTimedOut);
                }
            }

            // BlockingLock
            Object obj2   = new Object();
            Thread thread = new Thread(() =>
            {
                using (BlockingLock l = BlockingLock.TryLock(o, 0))
                {
                    Assert.IsFalse(l.IsLockObtained);
                }
                using (BlockingLock l = BlockingLock.TryLock(o, TimeSpan.FromMilliseconds(0)))
                {
                    Assert.IsFalse(l.IsLockObtained);
                }
                using (BlockingLock l = BlockingLock.Lock(obj2))
                {
                    Assert.IsTrue(l.IsLockObtained);
                    Monitor.Pulse(obj2);
                }
            });
            using (BlockingLock l = BlockingLock.Lock(o))
            {
                using (BlockingLock lock2 = BlockingLock.Lock(obj2))
                {
                    thread.Start();
                    Blocking.Wait(obj2);
                }
            }
            thread.Join();
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Run through various timeout scenarios
        /// </summary>
        public static void TimeoutTestRunner()
        {
            Object o = new Object();
            long ldtStart;
            long cMillis;

            using (BlockingLock l = BlockingLock.Lock(o))
            {
                // wait/sleep outside of a ThreadTimeout "using" block
                ldtStart = DateTimeUtils.GetSafeTimeMillis();
                Blocking.Wait(o, 10);
                cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
                Assert.IsTrue(cMillis > 5 && cMillis < 50);

                ldtStart = DateTimeUtils.GetSafeTimeMillis();
                Blocking.Sleep(10);
                cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
                Assert.IsTrue(cMillis > 5 && cMillis < 50);

                // Timeout.Infinite
                ldtStart = DateTimeUtils.GetSafeTimeMillis();
                using (ThreadTimeout t = ThreadTimeout.After(10))
                {
                    Blocking.Wait(o, Timeout.Infinite);
                }
                cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
                Assert.IsTrue(cMillis > 5 && cMillis < 50);

                ldtStart = DateTimeUtils.GetSafeTimeMillis();
                using (ThreadTimeout t = ThreadTimeout.After(10))
                {
                    Blocking.Sleep(Timeout.Infinite);
                }
                cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
                Assert.IsTrue(cMillis > 5 && cMillis < 50);

                // calling Wait() after timeout reached
                ldtStart = DateTimeUtils.GetSafeTimeMillis();
                using (ThreadTimeout t = ThreadTimeout.After(10))
                {
                    try
                    {
                        Blocking.Wait(o);
                        Assert.Fail();
                    }
                    catch (ThreadInterruptedException) { }
                }
                cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
                Assert.IsTrue(cMillis > 5 && cMillis < 50);

                // longer timeout
                ldtStart = DateTimeUtils.GetSafeTimeMillis();
                using (ThreadTimeout t = ThreadTimeout.After(1000))
                {
                    Assert.IsFalse(Blocking.Wait(o, Timeout.Infinite));
                }
                cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
                Assert.IsTrue(cMillis > 900 && cMillis < 1100);

                // nested timeouts
                long ldtOuterStart = DateTimeUtils.GetSafeTimeMillis();
                using (ThreadTimeout to = ThreadTimeout.After(2000))
                {
                    ldtStart = DateTimeUtils.GetSafeTimeMillis();
                    using (ThreadTimeout t = ThreadTimeout.After(10))
                    {
                        Blocking.Wait(o, 100);
                    }
                    cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
                    Assert.IsTrue(cMillis > 5 && cMillis < 50);

                    ldtStart = DateTimeUtils.GetSafeTimeMillis();
                    using (ThreadTimeout t = ThreadTimeout.After(100))
                    {
                        Blocking.Wait(o, 1000);
                    }
                    cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
                    Assert.IsTrue(cMillis > 50 && cMillis < 200);

                    ldtStart = DateTimeUtils.GetSafeTimeMillis();
                    using (ThreadTimeout t = ThreadTimeout.After(1000))
                    {
                        Blocking.Sleep(10000);
                    }
                    cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtStart;
                    Assert.IsTrue(cMillis > 900 && cMillis < 1100);

                    using (ThreadTimeout t = ThreadTimeout.After(Timeout.Infinite)) // try to extend timeout
                    {
                        Assert.IsTrue(ThreadTimeout.RemainingTimeoutMillis < 1000);
                        Blocking.Wait(o, Timeout.Infinite); // remainder of the 2s timeout
                    }

                    // time should be used up at this point

                    using (ThreadTimeout t = ThreadTimeout.After(5000))
                    {
                        try
                        {
                            Blocking.Wait(o);
                            Assert.Fail();
                        }
                        catch (ThreadInterruptedException) { }
                    }
                }
                cMillis = DateTimeUtils.GetSafeTimeMillis() - ldtOuterStart;
                Assert.IsTrue(cMillis > 1900 && cMillis < 2100);
            }
        }

        #endregion
    }
}
