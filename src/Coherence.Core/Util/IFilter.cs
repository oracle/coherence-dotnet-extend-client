/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Util
{
    /// <summary>
    /// Provide for "pluggable" conditional behavior.
    /// </summary>
    /// <author>Cameron Purdy</author>
    /// <author>Aleksandar Seovic</author>
    public interface IFilter
    {
        /// <summary>
        /// Apply the test to the object.
        /// </summary>
        /// <param name="o">
        /// An object to which the test is applied.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        bool Evaluate(object o);
    }
}