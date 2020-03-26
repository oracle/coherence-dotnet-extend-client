/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;

namespace Tangosol.Net
{
    /// <summary>
    /// This <see cref="INameService"/> interface represents a service that 
    /// accepts connections from external clients(e.g. Coherence*Extend) and
    /// provides a name lookup service.
    /// </summary>
    /// <author>Wei Lin  2012.05.22</author>
    /// <since>Coherence 12.1.2</since>
    public interface INameService : IService
    {
        /// <summary>
        /// Binds a name to an object.
        /// </summary>
        /// <param name="name">
        /// The name to bind; may not be empty.
        /// </param>
        /// <param name="o">
        /// The object to bind; possibly null.
        /// </param>
        void Bind(string name, Object o);

        /// <summary>
        /// Retrieves the named object.
        /// </summary>
        /// <param name="name">
        /// The name of the object to look up.
        /// </param>
        /// <returns>
        /// The object bound to name.
        /// </returns>
        Object Lookup(string name);

        /// <summary>
        /// Unbinds the named object.
        /// </summary>
        /// <param name="name">
        /// The name of the object to unbind.
        /// </param>
        void Unbind(string name);
    }
}
