/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// A unit calculator is an object that can calculate the cost of caching
    /// an object.
    /// </summary>
    /// <author>Cameron Purdy  2009.01.13</author>
    /// <author>Aleksandar Seovic  2009.07.27</author>
    /// <since>Coherence 3.5.1</since>
    public interface IUnitCalculator
    {
        /// <summary>
        /// Calculate a cache cost for the specified cache entry key and value.
        /// </summary>
        /// <param name="oKey">
        /// The cache key to evaluate for unit cost.
        /// </param>
        /// <param name="oValue">
        /// The cache value to evaluate for unit cost.
        /// </param>
        /// <returns>
        /// An integer value 0 or greater, with a larger value signifying 
        /// a higher cost.
        /// </returns>
        int CalculateUnits(Object oKey, Object oValue);

        /// <summary>
        /// Obtain the name of the unit calculator. 
        /// </summary>
        /// <remarks>
        /// This is intended to be human readable for use in a monitoring tool; 
        /// examples include "SimpleMemoryCalculator" and "BinaryMemoryCalculator".
        /// </remarks>
        /// <value>
        /// The name of the unit calculator.
        /// </value>
        String Name { get; }
    }
}