/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

using Tangosol.IO.Resources;

namespace Tangosol.Config
{
    /// <summary>
    /// Value object containing information from the parsed Coherence for
    /// .NET configuration section (&lt;coherence>).
    /// </summary>
    /// <author>Aleksandar Seovic  2006.10.06</author>
    public class CoherenceConfig
    {
        #region Properties

        /// <summary>
        /// The value of the <b>coherence-config</b> element within the
        /// Coherence for .NET configuration section.
        /// </summary>
        /// <value>
        /// The value of the <b>coherence-config</b> element within the
        /// Coherence for .NET configuration section.
        /// </value>
        public IResource OperationalConfig { get; set; }

        /// <summary>
        /// The value of the <b>cache-config</b> element within the Coherence
        /// for .NET configuration section.
        /// </summary>
        /// <value>
        /// The value of the <b>cache-config</b> element within the Coherence
        /// for .NET configuration section.
        /// </value>
        public IResource CacheConfig { get; set; }

        /// <summary>
        /// The value of the <b>pof-config</b> element within the Coherence
        /// for .NET configuration section.
        /// </summary>
        /// <value>
        /// The value of the <b>pof-config</b> element within the Coherence
        /// for .NET configuration section.
        /// </value>
        public IResource PofConfig { get; set; }

        /// <summary>
        /// A map of miscellaneous configuraion properties within
        /// the Coherence for .NET configuration section.
        /// </summary>
        /// <value>
        /// A map of name, value pair of configuration properties within the
        /// Coherence for .NET configuration section.
        /// </value>
        public Hashtable ConfigProperties { get; set; }

        #endregion
    }
}