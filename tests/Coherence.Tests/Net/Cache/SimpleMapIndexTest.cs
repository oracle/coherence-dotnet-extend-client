/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections;
using System.Threading;

using Tangosol.Util;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;
using Tangosol.Util.Collections;

using NUnit.Framework;


namespace Tangosol.Net.Cache
{
    /// <summary>
    /// SimpleMapIndex functional tests
    /// </summary>
    /// <author> bbc 2012.5.26 </author>
    [TestFixture]
    public class SimpleMapIndexTest
    {
        /// <summary>
        /// Test indexed value as collection
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>
        [Test]
        public void testCollection()
        {
            LocalCache cache = new LocalCache();
            cache.Clear();

            cache.AddIndex(IdentityExtractor.Instance, false, null);

            DoPutALL doDoPutAll = new DoPutALL(cache, "collection");
            using (BlockingLock l = BlockingLock.Lock(doDoPutAll.m_myLock))
            {
                Thread thread = new Thread(new ThreadStart(doDoPutAll.Run));
                thread.Start();
                Blocking.Sleep(2222);

                ArrayList qname = new ArrayList();
                qname.Add("test");
                IFilter filter = new ContainsAnyFilter(IdentityExtractor.Instance, (ICollection) qname);

                for (int i = 0; i < 50000; i++)
                {
                    object[] result = cache.GetEntries(filter);
                    Assert.AreEqual(result.Length, cache.Count);
                }

                // signal the PutAll thread to stop
                Blocking.Wait(doDoPutAll.m_myLock);
                thread.Join();
            }
        }

        /// <summary>
        /// Test indexed value as object[]
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>
        [Test]
        public void testArray()
        {
            LocalCache cache = new LocalCache();
            cache.Clear();

            cache.AddIndex(IdentityExtractor.Instance, false, null);
            DoPutALL doDoPutAll = new DoPutALL(cache, "array");

            using (BlockingLock l = BlockingLock.Lock(doDoPutAll.m_myLock))
            {
                Thread thread = new Thread(new ThreadStart(doDoPutAll.Run));
                thread.Start();
                Blocking.Sleep(2222);

                ArrayList qname = new ArrayList();
                qname.Add("test");
                IFilter filter = new ContainsAnyFilter(IdentityExtractor.Instance, (ICollection)qname);

                for (int i = 0; i < 50000; i++)
                {
                    object[] result = cache.GetEntries(filter);
                    Assert.AreEqual(result.Length, cache.Count);
                }

                // signal the PutAll thread to stop
                Blocking.Wait(doDoPutAll.m_myLock);
                thread.Join();
            }
        }

        internal class DoPutALL
        {
            public DoPutALL(LocalCache cache, String name)
            {
                m_cache = cache;
                m_name  = name;
            }

            public void Run()
            {
                LocalCache cache = m_cache;
                String     sname = m_name;

                Random random = new Random();

                while (true)
                {
                    if (sname.Equals("collection"))
                    {
                        doCollection(cache, sname, random);
                    }
                    else
                    {
                        doArray(cache, sname, random);
                    }

                    // check if test is completed
                    if (Monitor.TryEnter(m_myLock))
                    {
                        Monitor.Pulse(m_myLock);
                        Monitor.Exit(m_myLock);
                        break;
                    }
                }
            }

            private static void doCollection(LocalCache cache, string sname, Random random)
            {
                ArrayList values = new ArrayList();
                for (int i = 0; i <= 8; i++)
                {
                    // duplicates will cause Missing inverse index messages
                    values.Add("test");
                    values.Add("test " + random.Next(4));

                    IDictionary dict = new Hashtable();
                    dict.Add(sname, values);

                    cache.InsertAll(dict);
                }
            }

            private static void doArray(LocalCache cache, string sname, Random random)
            {
                string[] values = new string[10] { "null", "null", "null", "null", "null", "null", "null", "null", "null", "null" };
                for (int i = 0; i <= 8; i++)
                {
                    // duplicates will cause Missing inverse index messages
                    values[i]     = "test";
                    values[i + 1] = "test " + random.Next(4);

                    IDictionary dict = new Hashtable();
                    dict.Add(sname, values);

                    cache.InsertAll(dict);
                }
            }

            private readonly LocalCache m_cache;
            private readonly string     m_name;
            public  readonly Object     m_myLock = new Object();
        }
    }
}