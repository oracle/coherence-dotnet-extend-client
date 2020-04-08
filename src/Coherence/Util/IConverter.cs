/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Util
{
    /// <summary>
    /// Provide for "pluggable" object conversions.
    /// </summary>
    /// <author>Pat McNerthney  2000.04.25</author>
    /// <author>Ana Cikic  2008.05.28</author>
    public interface IConverter
    {
        /// <summary>
        /// Convert the passed object to another object.
        /// </summary>
        /// <param name="o">
        /// Object to be converted.
        /// </param>
        /// <returns>
        /// The new, converted object.
        /// </returns>
        object Convert(object o);
    }
}