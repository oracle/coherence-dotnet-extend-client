/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿namespace Tangosol.IO.Pof.Reflection.Internal
{
    /// <summary>
    /// INameMangler implementations provide the ability to transform a
    /// string to the string convention employed by the mangler 
    /// implementation. An example of this would be to convert a non-camel
    /// case string to a camel case string.
    /// </summary> 
    /// <author>Harvey Raja  2011.07.25</author>
    /// <since>Coherence 3.7.1</since>
    public interface INameMangler
    {
        /// <summary>
        /// Convert the given string to a new string using a convention 
        /// determined by the implementer.
        /// </summary>
        /// <param name="name">
        /// Original string.
        /// </param>
        /// <returns>
        /// Mangled string.
        /// </returns>
        string Mangle(string name);
    }
}
