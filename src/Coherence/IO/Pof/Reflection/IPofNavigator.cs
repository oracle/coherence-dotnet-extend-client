/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.IO.Pof.Reflection
{
    /// <summary>
    /// The IPofNavigator interface represents an algorithm for navigating a 
    /// IPofValue hierarchy in order to locate a contained IPofValue for 
    /// extraction, modification and/or removal purposes.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public interface IPofNavigator
    {
        /// <summary>
        /// Locate the <see cref="IPofValue"/> designated by this IPofNavigator 
        /// within the passed IPofValue.
        /// </summary>
        /// <param name="valueOrigin">
        /// The origin from which navigation starts.
        /// </param>
        /// <returns>
        /// The resulting IPofValue.
        /// </returns>
        /// <exception cref="PofNavigationException">
        /// If the navigation fails; for example one of the intermediate nodes 
        /// in this path is a "terminal" IPofValue.
        /// </exception>
        IPofValue Navigate(IPofValue valueOrigin);
    }

}