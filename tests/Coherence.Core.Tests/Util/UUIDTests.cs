/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;

using NUnit.Framework;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net;

namespace Tangosol.Util
{
    [TestFixture]
    public class UUIDTests
    {
        NameValueCollection appSettings = TestUtils.AppSettings;

        private String CacheName
        {
            get { return appSettings.Get("cacheName"); }
        }

        [Test]
        public void UUIDTest()
        {
            long epochTime = new DateTime(1970, 1, 1).Ticks / 10000;

            UUID uuid1 = new UUID();
            UUID uuid2 = new UUID();
            String id1 = uuid1.ToString();
            String id2 = uuid2.ToString();
            Assert.AreNotEqual(id1, id2);
            Assert.AreEqual(uuid1.Address, uuid2.Address);
            Assert.AreNotEqual(uuid1.Port, uuid2.Port);
            Assert.Less(uuid1.Count, uuid2.Count);
            Assert.IsTrue(uuid1.Timestamp<=uuid2.Timestamp);
            Assert.AreNotEqual(uuid1.GetHashCode(), uuid2.GetHashCode());
            Assert.IsTrue(uuid1.IsGenerated);
            Assert.IsTrue(uuid1.IsAddressIncluded);

            IPAddress addr = NetworkUtils.GetLocalHostAddress();
            uuid1 = new UUID(epochTime, addr, 8080, 1);
            uuid2 = new UUID(epochTime, addr, 8080, 1);
            id1 = uuid1.ToString();
            id2 = uuid2.ToString();
            Assert.AreEqual(id1, id2);
            Assert.AreEqual(uuid1.Address, uuid2.Address);
            Assert.AreEqual(uuid1.GetHashCode(), uuid2.GetHashCode());
            Assert.IsTrue(uuid1.CompareTo(uuid2) == 0);

            addr = new IPAddress(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16});
            UUID uuid3 = new UUID(epochTime, addr, 8080, 2);
            String id3 = uuid3.ToString();
            Assert.AreNotEqual(uuid1.Address, uuid3.Address);
            Assert.IsTrue(uuid1.CompareTo(uuid3) > 0);

            byte[] byteArray = uuid3.ToByteArray();
            uuid1 = new UUID(byteArray);
            uuid2 = new UUID(id3);
            id1 = uuid1.ToString();
            id2 = uuid2.ToString();
            Assert.AreEqual(id1, id2);
            Assert.AreEqual(uuid1.Address, uuid2.Address);
            Assert.AreEqual(uuid1.GetHashCode(), uuid2.GetHashCode());
            Assert.IsTrue(uuid1.CompareTo(uuid2) == 0);
            Assert.IsFalse(uuid1.IsGenerated);
            Assert.IsTrue(uuid1.Equals(uuid2));
            Assert.IsTrue(uuid2.Equals(uuid1));

            UUID uuid4 = new UUID(epochTime, new byte[0], 8080, 1);
            uuid4 = new UUID(epochTime, (byte[])null, 8080, 1);
        }

        [Test]
        public void UUIDTestWithException()
        {
            Assert.That(() => new UUID(DateTimeUtils.GetSafeTimeMillis(), new byte[] {1, 2}, 8080, 1), Throws.ArgumentException);
        }

        [Test]
        public void UUIDTestDateBeforeEpoch()
        {
            IPAddress addr = NetworkUtils.GetLocalHostAddress();
            Assert.That(() => new UUID(1L, addr, 8080, 1), Throws.ArgumentException);
        }

        [Test]
        public void UUIDSerializationTest()
        {
            IPAddress addr = NetworkUtils.GetLocalHostAddress();
            UUID uuid = new UUID(DateTimeUtils.GetSafeTimeMillis(), addr, 8080, 1);
            SimplePofContext ctx = new SimplePofContext();
            ctx.RegisterUserType(20, typeof(UUID), new PortableObjectSerializer(20));
            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), uuid );
            stream.Position = 0;
            UUID result = (UUID) ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(uuid.ToString(), result.ToString());
        }

        [Test]
        public void UUIDFetchFromClusterTest()
        {
            // COH-8647 - verify that the timestamp in the Coherence cluster corresponds
            // to the local timestamp.  It would be nice to verify the UUID in LocalMember
            // transfers to the Cluster and back but the UUID is a private data member and
            // LocalMember.Equals() is not implemented.
            INamedCache cache   = CacheFactory.GetCache(CacheName);
            UUID        uuid    = new UUID(DateTimeUtils.GetSafeTimeMillis(), NetworkUtils.GetLocalHostAddress(), 51717, 1);
            string      uuidKey = "UUID";

            cache[uuidKey]  = uuid;
            UUID cachedUUID = (UUID) cache[uuidKey];
            Assert.AreEqual(uuid, cachedUUID);
            Assert.AreEqual(DateTimeUtils.GetDateTime(uuid.Timestamp).ToString(CultureInfo.InvariantCulture), cache.Invoke(uuidKey, new ProcessorPrintUUIDTimestamp()));
            cache.Clear();
            CacheFactory.Shutdown();
        }

        ///
        /// Test UUID construction by multiple threads.  Creates multiple
        /// threads which contend to initialize a single UUID instance.
        ///
        /// The unfixed issue (COH-11868) can't be reproduced with this unit test
        /// because this test requires more than 10K threads to reproduce.
        /// This unit test fails with a system error when about 7K threads
        /// are attempted.  The unfixed issue can be reproduced with
        /// Java or C++; see those unit tests for more info.
        ////
        [Test]
        public void testUUIDConflict()
        {
            // COH-11868; run parallel threads all of which do lazy initialization of a single
            // UUID instance.  If the String representation of the UUID gets changed by
            // any thread, the UUID instance was not locked sufficiently during init.
            runParallel(DoWork, NUM_THREADS);

            bool result = true;
            for (int i = 0; i < UUIDString.Length - 1; ++i)
            {
                if (!UUIDString[i].Equals(UUIDString[i + 1]))
                {
                    Console.Out.WriteLine("UUIDTest; comparison failed: " + i + " vs "+(i + 1));
                    result = false;
                    break;
                }
            }

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Runs the task in parallel on multiple threads.
        /// </summary>
        /// <param name=task>
        /// The task to run on each thread.
        /// </param>
        /// <param name=cThreads>
        /// The number of threads to run.
        /// </param>
        private static void runParallel(ThreadStart task, int threadCount)
        {
            Thread[] threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(new ThreadStart(task));
                threads[i].Name = PREFIX + i;
                threads[i].Start();
            }

            lock (SEMAPHORE)
            {
                allStarted = true;
                Monitor.PulseAll(SEMAPHORE);
            }

            try
            {
                for (int i = 0; i < threadCount; i++)
                {
                    threads[i].Join();
                }
            }
            catch (ThreadInterruptedException) {/*do nothing*/}
        }

        /// <summary>
        /// Waits for all threads to be started.
        /// </summary>
        private static void WaitForSemaphore()
        {
            lock (SEMAPHORE)
            {
                while (!allStarted)
                {
                    try
                    {
                        Blocking.Wait(SEMAPHORE);
                    }
                    catch (ThreadInterruptedException) {/*do nothing*/}
                }
            }
        }

        /// <summary>
        /// Performs the test, on each thread.  Gets the string
        /// representation of the UUID and stores it in the array of
        /// strings.  Each string is stored by the index of
        /// the thread that executed. The array of string is then
        /// examined to see if any thread overwrote the initialization
        /// data while the previous thread was still initializing.
        ///
        /// This can happen when the UUID is not locked sufficiently
        /// during initialization - COH-11868.
        /// </summary>
        private static void DoWork()
        {
            String name  = Thread.CurrentThread.Name;
            int    ofIx  = name.LastIndexOf(PREFIX);
            int    index = int.Parse(name.Substring(ofIx + PREFIX.Length));

            WaitForSemaphore();
            UUIDString[index] = UUIDInstance.ToString();
        }

        ///
        /// The number of concurrent test threads.
        ///
        const int NUM_THREADS = 5;

        ///
        /// The object being locked.
        ///
        static readonly Object SEMAPHORE = new Object();

        ///
        /// The object that signals that the threads have all started.
        ///
        volatile static bool allStarted;

        ///
        /// The single UUID object that will be contended for.
        ///
        static UUID UUIDInstance = new UUID();

        ///
        /// The string representations of the UUID, initialized by the threads
        /// contending for the UUID.
        ///
        static String[] UUIDString = new String[NUM_THREADS];

        ///
        /// Prefix string for the thread, so it can be used as an index
        /// into the array of UUID strings.
        ///
        const String PREFIX = "Thread-";

    }
}
