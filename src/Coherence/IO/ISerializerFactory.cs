/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿namespace Tangosol.IO
{
    /// <summary>
    /// A factory for <see cref="ISerializer"/> objects.
    /// </summary>
    /// <author>Wei Lin  2011.10.25</author>
    /// <since>Coherence 12.1.2</since>
    public interface ISerializerFactory
    {
        /// <summary>
        /// Create a new <see cref="ISerializer"/>.
        /// </summary>
        /// <returns>
        /// The new <see cref="ISerializer"/>.
        /// </returns>
        ISerializer CreateSerializer();
    }
}
