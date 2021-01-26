/*
 * Copyright (c) 2000, 2021, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

using NUnit.Framework;
using Tangosol.Net;
using Tangosol.Util;

namespace Tangosol.Net.Impl
{

    [TestFixture]
    public class SSLRemoteInvocationTests
    {
        [Test]
        public void TestRemoteInvocationOnCache()
        {
            Console.WriteLine(Directory.GetCurrentDirectory());

            IInvocationService service = (IInvocationService) CacheFactory.GetService("RemoteInvocationServiceSSL");
            Assert.IsNotNull(service);

            IDictionary result = service.Query(new EmptyInvocable(), null);
            Assert.IsNotNull(result);
        }
    }
}
