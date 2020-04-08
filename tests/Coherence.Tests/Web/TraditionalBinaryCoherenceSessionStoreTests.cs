/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using NUnit.Framework;
using Tangosol.IO;
using Tangosol.Web.Model;

namespace Tangosol.Web
{
    [TestFixture]
    public class TraditionalBinaryCoherenceSessionStoreTests 
        : AbstractCoherenceSessionStoreTests
    {
        #region Overrides of AbstractCoherenceSessionStoreTests

        protected override ISessionModelManager CreateModelManager(ISerializer serializer)
        {
            return new TraditionalSessionModelManager(serializer);
        }

        protected override ISerializer CreateSerializer()
        {
            return new BinarySerializer();
        }

        #endregion
    }
}