/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;

using NUnit.Framework;

using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Run.Xml;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;


namespace Tangosol.Util
{
    ///
    /// A functional tests for a Coherence*Extend client that uses
    /// configured trigger listeners.
    ///
    /// @author par  2013.09.23
    ///
    /// @since @BUILDVERSION@
    ///
    [TestFixture]
    public class TriggerListenerTests
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

        //
        // Test configured MapTrigger operation.
        //
        [Test]
        public void TestConfiguredTriggerListener()
        {
            IConfigurableCacheFactory ccf = CacheFactory.ConfigurableCacheFactory;

            IXmlDocument config = XmlHelper.LoadXml("assembly://Coherence.Tests/Tangosol.Resources/s4hc-extend-maptrigger-cache-config.xml");
            ccf.Config = config;

            INamedCache cache = CacheFactory.GetCache(CACHE_NAME);

            cache.Truncate();

            SimplePerson pIn = new SimplePerson("123-45-6789", "Eddie", "Van Halen", 1955, 
                    "987-65-4321", new String[] {"456-78-9123"});

            try
            {
                cache.Insert(1, pIn);
                Object[] keys = cache.GetKeys(AlwaysFilter.Instance);
                Assert.AreEqual(keys.Length, 1);

                SimplePerson pOut = (SimplePerson) cache[1];

                Assert.AreEqual(pIn.LastName.ToUpper(), pOut.LastName);
            }
            finally
            {
                CacheFactory.Shutdown();
            }
        }

        ///
        /// Factory method to instantiate configured MapTrigger
        ///
        public static CacheTriggerListener CreateTriggerListener(String sCacheName)
        {
            if (CACHE_NAME.Equals(sCacheName))
            {
                return new CacheTriggerListener(new PersonMapTrigger());
            }
            throw new ArgumentException("Unknown cache name " + sCacheName);
        }
 
        #region innerclass

        public class PersonMapTrigger : ICacheTrigger, IPortableObject
        {

            ///
            /// Default constructor.
            ///
            public PersonMapTrigger()
            {
            }

            public void Process(ICacheTriggerEntry entry)
            {
                SimplePerson person  = (SimplePerson) entry.Extract(IdentityExtractor.Instance);
                String sName         = person.LastName;
                String sNameUC       = sName.ToUpper();

                if (!sNameUC.Equals(sName))
                {
                    person.LastName = sNameUC;
                   entry.SetValue(person, false);
                }
            }

            public void ReadExternal(IPofReader reader)
            {
            }

            public void WriteExternal(IPofWriter writer)
            {
            }

        #endregion

        }
    #region constants

    public static String CACHE_NAME = "dist-extend-trigger-listener";

    #endregion
    }
}
