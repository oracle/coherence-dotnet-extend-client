/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using NUnit.Framework;

using Tangosol.IO;
using Tangosol.IO.Pof;

namespace Tangosol.Web
{
    [TestFixture]
    public class SplitPofCoherenceSessionStoreTests 
        : SplitBinaryCoherenceSessionStoreTests
    {
        #region Overrides of SplitBinaryCoherenceSessionStoreTests

        protected override ISerializer CreateSerializer()
        {
            return new ConfigurablePofContext("assembly://Coherence.Tests/Tangosol.Resources/s4hc-test-config.xml");
        }

        #endregion
    }
}