/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Net;

using NUnit.Framework;

namespace Tangosol.Util
{
    [TestFixture]
    public class NetworkUtilsTests
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

        [Test]
        public void GetLocalHostTest()
        {
            IPAddress address = NetworkUtils.GetLocalHostAddress();
            Assert.IsFalse(IPAddress.IsLoopback(address));
        }

        [Test]
        public void IsAnyLocalAddressTest()
        {
            IPAddress address = NetworkUtils.GetLocalHostAddress();
            Assert.IsFalse(NetworkUtils.IsAnyLocalAddress(address));
            Assert.IsTrue(NetworkUtils.IsAnyLocalAddress(IPAddress.Any));
        }

        [Test]
        public void IsLoopbackAddressTest()
        {
            Assert.IsTrue(NetworkUtils.IsLoopbackAddress(IPAddress.Loopback));
            IPAddress address = NetworkUtils.GetLocalHostAddress();
            Assert.IsFalse(NetworkUtils.IsLoopbackAddress(address));
        }

        [Test]
        public void GetHostAddressTest()
        {
            IPAddress address = NetworkUtils.GetHostAddress("www.oracle.com", 10000);
            Assert.IsFalse(NetworkUtils.IsLoopbackAddress(address));

            try
            {
                NetworkUtils.GetHostAddress("nonexistenthost.never.ever");
                Assert.Fail();
            }
            catch (Exception)
            {
            }
        }

        [Test]
        public void GetAllAddressesTest()
        {
            IPAddress[] arrAddress = NetworkUtils.GetAllAddresses("www.oracle.com", 10000);
            Assert.IsTrue(arrAddress.Length > 0);
            Assert.IsFalse(NetworkUtils.IsLoopbackAddress(arrAddress[0]));

            try
            {
                NetworkUtils.GetAllAddresses("nonexistenthost.never.ever");
                Assert.Fail();
            }
            catch (Exception)
            {
            }
        }

    }
}