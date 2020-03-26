/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;

using Tangosol.IO.Pof;

namespace Tangosol.Net.Internal
{
    /// <summary>
    /// The NameServicePofContext is a basic <see cref="IPofContext"/> implementation which
    /// supports the types used to manage Coherence*Extend connections.
    /// </summary>
    /// <author>Patrick Fry  2012.04.27</author>
    /// <since>12.2.1</since>
    public class NameServicePofContext : ConfigurablePofContext
    {
        #region Properties

        /// <summary>
        /// The NameServicePofContext singleton.
        /// </summary>
        public static NameServicePofContext INSTANCE
        {
            get { return s_instance.Value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new NameServicePofContext.
        /// </summary>
        public NameServicePofContext() : base("assembly://Coherence/Tangosol.Config/coherence-pof-config.xml")
        {
        }

        #endregion

        #region data members

        /// <summary>
        /// The lazily instantiated NameServicePofContext singleton.
        /// </summary>
        static private readonly Lazy<NameServicePofContext> s_instance =
            new Lazy<NameServicePofContext>(() => new NameServicePofContext());

        #endregion
    }
}
