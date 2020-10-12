/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net
{
    /// <summary>
    /// A factory for <see cref="IAddressProvider"/> objects.
    /// </summary>
    /// <author>Wei Lin  2012.04.11</author>
    /// <since>Coherence 12.1.2</since>
    public interface IAddressProviderFactory
    {
        /// <summary>
        /// Create a new <see cref="IAddressProvider"/>.
        /// </summary>
        /// <returns>
        /// The new <see cref="IAddressProvider"/>.
        /// </returns>
        IAddressProvider CreateAddressProvider();
    }
}