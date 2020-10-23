/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Security.Principal;
using System.Threading;
using NUnit.Framework;

using Tangosol.Net;
using Tangosol.Net.Impl;
using Tangosol.Net.Messaging;
using Tangosol.Net.Security.Impl;

namespace Tangosol.Security
{
    [TestFixture]
    public class ExtendSecurityTest
    {
        private static String CacheName
        {
            get { return "secure-cache"; }
        }

        [Test]
        [Category("Security")]
        [Ignore("Ignore Docker Test")]
        public void TestIdentityPassing()
        {
            string       name         = "CN=Manager,OU=MyUnit";
            IIdentity    identity     = new GenericIdentity(name, "POF");
            IPrincipal   principalNew = new GenericPrincipal(identity, null);
            IPrincipal   principalOld = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                IService service = CacheFactory.GetService("ExtendTcpCacheService");
                service = (IService) ((SafeService) service).Service;

                IChannel channel = ((RemoteService) service).Channel;
                Assert.AreEqual(principalNew, channel.Principal);

                channel = channel.Connection.GetChannel(0);
                Assert.AreEqual(principalNew, channel.Principal);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }

            Thread.CurrentPrincipal = principalNew;
            try
            {
                IService service = CacheFactory.GetService("RemoteInvocationService");
                service = (IService)((SafeService) service).Service;

                IChannel channel = ((RemoteService) service).Channel;
                Assert.AreEqual(principalNew, channel.Principal);

                channel = channel.Connection.GetChannel(0);
                Assert.AreEqual(principalNew, channel.Principal);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }

            Thread.CurrentPrincipal = principalNew;
            try
            {
                INamedCache cache = CacheFactory.GetCache(CacheName);
                Assert.IsNotNull(cache);
                cache["user"] = name;
                cache["key"] = "value";
                CacheFactory.ReleaseCache(cache);
            }
            catch (Exception)
            {
                Assert.IsTrue(false, "exception accessing cache");
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }
            
            // Negative test using unauthorized user
            Exception ex = null;

            name         = "CN=Admin,OU=MyUnit";
            identity     = new GenericIdentity(name, "POF");
            principalNew = new GenericPrincipal(identity, null);
            principalOld = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                INamedCache cache = CacheFactory.GetCache(CacheName);
                Assert.IsNotNull(cache);
                cache["user"] = name;
                cache["key"] = "value";
            }
            catch (Exception ee)
            {
                // should get exception for unauthorized user
                ex = ee;
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }
            Assert.IsNotNull(ex, "expected security exception");
        }

        [Test]
        [Ignore("Ignore Docker Test")]
        [Category("Security")]
        public void TestIdentityCacheScoping()
        {
            string      name         = "CN=Manager,OU=MyUnit";
            IIdentity   identity     = new GenericIdentity(name, "POF");
            IPrincipal  principalNew = new GenericPrincipal(identity, null);
            IPrincipal  principalOld = Thread.CurrentPrincipal;
            INamedCache cache00;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                cache00 = CacheFactory.GetCache(CacheName);
                Assert.IsNotNull(cache00);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }
            
            INamedCache cache01;
            
            name         = "CN=CEO,OU=MyUnit";
            identity     = new GenericIdentity(name, "POF");
            principalNew = new GenericPrincipal(identity, null);
            principalOld = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                cache01 = CacheFactory.GetCache(CacheName);
                Assert.IsNotNull(cache01);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }
            Assert.IsFalse(cache00.Equals(cache01));
        }

        [Test]
        [Category("Security")]
        public void TestIdentityServiceScoping()
        {
            string name = "CN=Manager,OU=MyUnit";
            IIdentity identity = new GenericIdentity(name, "POF");
            IPrincipal principalNew = new GenericPrincipal(identity, null);
            IPrincipal principalOld = Thread.CurrentPrincipal;
            IService service00;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                service00 = CacheFactory.GetService("RemoteInvocationService");
                Assert.IsNotNull(service00);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }

            IService service01;

            name = "CN=Admin,OU=MyUnit";
            identity = new GenericIdentity(name, "POF");
            principalNew = new GenericPrincipal(identity, null);
            principalOld = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                service01 = CacheFactory.GetService("RemoteInvocationService");
                Assert.IsNotNull(service01);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }
            Assert.AreNotEqual(service00, service01);
        }

        [Test]
        [Category("Security")]
        public void TestSimplePrincipal()
        {
            string     name         = "CN=Manager,OU=MyUnit";
            IIdentity  identity     = new GenericIdentity(name, "POF");
            IPrincipal principalOne = new GenericPrincipal(identity, null);
            IPrincipal principalTwo = new GenericPrincipal(identity, null);

            Assert.IsFalse(principalOne.Equals(principalTwo));
            Assert.IsFalse(Object.Equals(principalOne, principalTwo));

            principalOne = new SimplePrincipal(identity, null);
            principalTwo = new SimplePrincipal(identity, null);

            Assert.IsTrue(principalOne.Equals(principalTwo));
            Assert.IsTrue(Object.Equals(principalOne, principalTwo));
        }

        [Test]
        [Ignore("Ignore Docker Test")]
        [Category("Security")]
        public void TestSimpleIdentityPassing()
        {
            string name             = "CN=Manager,OU=MyUnit";
            IIdentity identity      = new GenericIdentity(name, "POF");
            IPrincipal principalNew = new SimplePrincipal(identity, null);
            IPrincipal principalOld = Thread.CurrentPrincipal;

            INamedCache cache00;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                cache00 = CacheFactory.GetCache(CacheName);
                Assert.IsNotNull(cache00);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }
            
            INamedCache cache01;
            
            name         = "CN=CEO,OU=MyUnit";
            identity     = new GenericIdentity(name, "POF");
            principalNew = new SimplePrincipal(identity, null);
            principalOld = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                cache01 = CacheFactory.GetCache(CacheName);
                Assert.IsNotNull(cache01);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }
            Assert.IsFalse(cache00.Equals(cache01));
        }

        [Test]
        [Ignore("Ignore Docker Test")]
        [Category("Security")]
        public void TestSimpleIdentityCacheScoping()
        {
            // Test two caches are not equal when different principals
            // have different identity
            string      name         = "CN=Manager,OU=MyUnit";
            IIdentity   identity     = new GenericIdentity(name, "POF");
            IPrincipal  principalNew = new SimplePrincipal(identity, null);
            IPrincipal  principalOld = Thread.CurrentPrincipal;
            INamedCache cache00;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                cache00 = CacheFactory.GetCache(CacheName);
                Assert.IsNotNull(cache00);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }
            
            INamedCache cache01;
            
            name         = "CN=CEO,OU=MyUnit";
            identity     = new GenericIdentity(name, "POF");
            principalNew = new SimplePrincipal(identity, null);
            principalOld = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                cache01 = CacheFactory.GetCache(CacheName);
                Assert.IsNotNull(cache01);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }
            Assert.IsFalse(cache00.Equals(cache01));


            // Test two caches are equal when different principals
            // have same identity
            name         = "CN=Manager,OU=MyUnit";
            identity     = new GenericIdentity(name, "POF");
            principalNew = new SimplePrincipal(identity, null);
            principalOld = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                cache00 = CacheFactory.GetCache(CacheName);
                Assert.IsNotNull(cache00);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }
            
            identity     = new GenericIdentity(name, "POF");
            principalNew = new SimplePrincipal(identity, null);
            principalOld = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                cache01 = CacheFactory.GetCache(CacheName);
                Assert.IsNotNull(cache01);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }
            Assert.AreEqual(cache00, cache01);
        }

        [Test]
        [Category("Security")]
        public void TestSimpleIdentityServiceScoping()
        {
            string name             = "CN=Manager,OU=MyUnit";
            IIdentity identity      = new GenericIdentity(name, "POF");
            IPrincipal principalNew = new SimplePrincipal(identity, null);
            IPrincipal principalOld = Thread.CurrentPrincipal;
            IService service00;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                service00 = CacheFactory.GetService("RemoteInvocationService");
                Assert.IsNotNull(service00);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }

            IService service01;

            name         = "CN=Admin,OU=MyUnit";
            identity     = new GenericIdentity(name, "POF");
            principalNew = new SimplePrincipal(identity, null);
            principalOld = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = principalNew;
            try
            {
                service01 = CacheFactory.GetService("RemoteInvocationService");
                Assert.IsNotNull(service01);
            }
            finally
            {
                Thread.CurrentPrincipal = principalOld;
            }
            Assert.AreNotEqual(service00, service01);
        }

    }
}