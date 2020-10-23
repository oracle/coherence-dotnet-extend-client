/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Util
{
    /// <summary>
    /// IValueExtractor is used to both extract values (for example, for
    /// sorting or filtering) from an object, and to provide an identity for
    /// that extraction.
    /// </summary>
    /// <remarks>
    /// <b>Important Note:</b> all classes that implement IValueExtractor
    /// interface must explicitly implement the <see cref="GetHashCode"/> and
    /// <see cref="Equals"/> methods in a way that is based solely on the
    /// object's serializable state.
    /// </remarks>
    /// <author>Cameron Purdy, Gene Gleyzer  2002.10.31</author>
    /// <author>Aleksandar Seovic  2006.07.12</author>
    public interface IValueExtractor
    {
        /// <summary>
        /// Extract the value from the passed object.
        /// </summary>
        /// <remarks>
        /// The returned value may be <c>null</c>.
        /// </remarks>
        /// <param name="target">
        /// An object to retrieve the value from.
        /// </param>
        /// <returns>
        /// The extracted value as an object; <c>null</c> is an acceptable
        /// value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// If this IValueExtractor is incompatible with the passed object to
        /// extract a value from and the implementation <b>requires</b> the
        /// passed object to be of a certain type.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this IValueExtractor cannot handle the passed object for any
        /// other reason; an implementor should include a descriptive
        /// message.
        /// </exception>
        object Extract(object target);

        /// <summary>
        /// Compare the IValueExtractor with another object to determine
        /// equality.
        /// </summary>
        /// <remarks>
        /// Two IValueExtractor objects, <i>ve1</i> and <i>ve2</i> are
        /// considered equal if <b>ve1.Extract(o)</b> equals
        /// <b>ve2.Extract(o)</b> for all values of <b>o</b>.
        /// </remarks>
        /// <param name="o">
        /// The reference object with which to compare.
        /// </param>
        /// <returns>
        /// <b>true</b> if this IValueExtractor and the passed object are
        /// equivalent.
        /// </returns>
        bool Equals(object o);

        /// <summary>
        /// Determine a hash value for the IValueExtractor object according
        /// to the general <b>object.GetHashCode</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this IValueExtractor object.
        /// </returns>
        int GetHashCode();

        /// <summary>
        /// Provide a human-readable description of this IValueExtractor
        /// object.
        /// </summary>
        /// <returns>
        /// A human-readable description of this IValueExtractor object.
        /// </returns>
        string ToString();
    }
}