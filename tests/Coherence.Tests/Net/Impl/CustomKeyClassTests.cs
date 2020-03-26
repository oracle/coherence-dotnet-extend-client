/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;

using NUnit.Framework;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// A collection of functional tests for Coherence*Extend that go through the
    /// RemoteNamedCacheTests tests using a custom key class which is defined at the
    /// extend client but not at the PartitionedCache.
    /// </summary>
    /// <author>phf 2001.09.06</author>
    [TestFixture]
    public class CustomKeyClassTests : RemoteNamedCacheTests
    {
        protected override Object GetKeyObject(Object o)
        {
            // use a key class which is only defined in the extend client to
            // verify that the PartitionedCache does not deserialize the key
            return new CustomKeyClass(o);
        }

        [Test]
        public void TestDeferKeyAssociationCheck()
        {
            INamedCache cache = CacheFactory.GetCache("defer-key-association-check");

            cache.Add("key", "value");

            Exception e = null;
            try
            {
                cache.Add(new CustomKeyClass("key"), "value");
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);

            cache.Clear();
            CacheFactory.Shutdown();
        }
    }
}
