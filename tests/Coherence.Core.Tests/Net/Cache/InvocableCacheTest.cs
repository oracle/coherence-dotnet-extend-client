/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using NUnit.Framework;
using Tangosol.Net.Impl;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Cache
{
    [TestFixture]
    public class InvocableCacheTests
    {
        protected IInvocableCache GetInvocableCache()
        {
            return new WrapperNamedCache(new LocalNamedCache());
        }

        [Test]
        public void InvokeTest()
        {
            IInvocableCache cache = GetInvocableCache();
            cache.Add("1", 1);
            cache.Add("2", 2);
            cache.Add("3", 3);

            Assert.AreEqual(4, (Int32) cache.Invoke("2", new SquareProcessor()));
        }

        [Test]
        public void InvokeAllWithKeySetTest()
        {
            IInvocableCache cache = GetInvocableCache();
            cache.Add("1", 1);
            cache.Add("2", 2);
            cache.Add("3", 3);

            IDictionary expected = new Hashtable();
            expected.Add("1", 1);
            expected.Add("3", 9);

            Assert.AreEqual(expected, cache.InvokeAll(new ArrayList { "1", "3" }, new SquareProcessor()));
        }

        [Test]
        public void InvokeAllWithFilterTest()
        {
            IInvocableCache cache = GetInvocableCache();
            cache.Add("1", 1);
            cache.Add("2", 2);
            cache.Add("3", 3);

            IDictionary expected = new Hashtable();
            expected.Add("2", 4);
            expected.Add("3", 9);

            Assert.AreEqual(expected, cache.InvokeAll(GREATER_THAN_1, new SquareProcessor()));
        }

        #region Default Map methods

        [Test]
        public void GetOrDefaultTest()
        {
            IInvocableCache cache = GetInvocableCache();
            cache.Add("1", 1);

            Assert.AreEqual(1, (int) cache.GetOrDefault("1", 1));
            Assert.AreEqual(2, (int) cache.GetOrDefault("2", 2));
        }

        [Test]
        public void PutIfAbsentTest()
        {
            IInvocableCache cache  = GetInvocableCache();
            object          result = cache.InsertIfAbsent("1", 1);
            Assert.IsNull(result);
            Assert.AreEqual(1, (int) cache.InsertIfAbsent("1", 100));
            cache.Add("2", null);
            Assert.IsNull(cache.InsertIfAbsent("2", 2));
            Assert.AreEqual(2, cache.Count);
            Assert.AreEqual(2, (int) cache["2"]);
        }

        [Test]
        public void RemoveTest()
        {
            IInvocableCache cache = GetInvocableCache();
            cache.Add("1", 1);
            cache.Add("2", 2);

            Assert.IsFalse((bool) cache.Remove("1", 2));
            Assert.IsTrue((bool) cache.Remove("2", 2));
            Assert.AreEqual(1, cache.Count);
            Assert.IsTrue(cache.Contains("1"));
            Assert.IsFalse(cache.Contains("2"));
        }

        [Test]
        public void ReplaceTest()
        {
            IInvocableCache cache = GetInvocableCache();
            cache.Add("1", 1);
            cache.Add("2", null);

            Assert.AreEqual(1, (int) cache.Replace("1", 100));
            Assert.IsNull(cache.Replace("2", 200));
            Assert.IsNull(cache.Replace("3", 300));
            Assert.AreEqual(2, cache.Count);
            Assert.IsFalse(cache.Contains("3"));
        }

        [Test]
        public void ReplaceWithValueCheckTest()
        {
            IInvocableCache cache = GetInvocableCache();
            cache.Add("1", 1);
            cache.Add("2", null);
            cache.Add("3", null);

            Assert.IsTrue((bool) cache.Replace("1", 1, 100));
            Assert.IsFalse((bool) cache.Replace("2", 2, 200));
            Assert.IsTrue((bool) cache.Replace("3", null, 300));
            Assert.IsFalse((bool) cache.Replace("4", 4, 400));
            Assert.AreEqual(100, (int) cache["1"]);
            Assert.IsNull(cache["2"]);
            Assert.AreEqual(300, (int) cache["3"]);
            Assert.IsFalse(cache.Contains("4"));
        }

        #endregion

        #region Data member

        public static readonly GreaterFilter GREATER_THAN_1 = new GreaterFilter(IdentityExtractor.Instance, 1);

        #endregion
    }

    public class SquareProcessor : AbstractProcessor
    {
        /// <summary>
        /// Process an <see cref="IInvocableCacheEntry"/>.
        /// </summary>
        /// <param name="entry">
        /// The <b>IInvocableCacheEntry</b> to process.
        /// </param>
        /// <returns>
        /// The result of the processing, if any.
        /// </returns>
        public override object Process(IInvocableCacheEntry entry)
        {
            return ((Int32) entry.Value) * ((Int32) entry.Value);
        }
    }
}