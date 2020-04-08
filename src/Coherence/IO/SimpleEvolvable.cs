/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using Tangosol.Util;

namespace Tangosol.IO
{
    /// <summary>
    /// Simple implementation of <see cref="IEvolvable"/> interface.
    /// </summary>
    /// <author>Aleksandar Seovic  2013.11.04</author>
    /// <since>Coherence 12.2.1</since>
    public class SimpleEvolvable : IEvolvable
    {
        /// <summary>
        /// Construct SimpleEvolvable instance.
        /// </summary>
        /// <param name="implVersion">Implementation version</param>
        public SimpleEvolvable(int implVersion)
        {
            ImplVersion = implVersion;
        }

        /// <summary>
        /// Implementation version.
        /// </summary>
        public int ImplVersion { get; private set; }

        /// <summary>
        /// Data version.
        /// </summary>
        public int DataVersion { get; set; }

        /// <summary>
        /// Future data.
        /// </summary>
        public Binary FutureData { get; set; }
    }
}