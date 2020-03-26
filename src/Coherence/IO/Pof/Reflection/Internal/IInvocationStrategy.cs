/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿namespace Tangosol.IO.Pof.Reflection.Internal
{
    /// <summary>
    /// An IInvocationStrategy provides an abstraction of the underlying
    /// mechanisms used to retrieve and set a property's value.
    /// </summary>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <typeparam name="T">The containing type.</typeparam>
    /// <since>Coherence 3.7.1</since>
    public interface IInvocationStrategy<T> where T : class, new()
    {
        /// <summary>
        /// Returns the value of the property.
        /// </summary>
        /// <param name="container">
        /// Container of this and all other properties.
        /// </param>
        /// <returns>
        /// Property value.
        /// </returns>
        object Get(T container);

        /// <summary>
        /// Sets the parameter value to the property.
        /// </summary>
        /// <param name="container">
        /// Container of this and all other sibling properties.
        /// </param>
        /// <param name="value">
        /// New value to assign to the property.
        /// </param>
        void Set(T container, object value);
    }
}
