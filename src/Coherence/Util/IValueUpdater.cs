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
    /// IValueUpdater is used to update an object's state.
    /// </summary>
    /// <author>Jason Howes, Gene Gleyzer  2005.10.25</author>
    /// <author>Aleksandar Seovic  2006.07.12</author>
    public interface IValueUpdater
    {
        /// <summary>
        /// Update the state of the passed target object using the passed
        /// value.
        /// </summary>
        /// <param name="target">
        /// The object to update the state of.
        /// </param>
        /// <param name="value">
        /// The new value to update the state with.
        /// </param>
        /// <exception cref="InvalidCastException">
        /// If this IValueUpdater is incompatible with the passed target
        /// object or the value and the implementation <b>requires</b> the
        /// passed object or the value to be of a certain type.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this IValueUpdater cannot handle the passed target object or
        /// value for any other reason; an implementor should include a
        /// descriptive message.
        /// </exception>
        void Update(object target, object value);
    }
}