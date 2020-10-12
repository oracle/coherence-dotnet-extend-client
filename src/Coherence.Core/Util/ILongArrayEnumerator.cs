/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

namespace Tangosol.Util {

    /// <summary>
    /// An IEnumerator for ILongArray.
    /// </summary>
    /// <author>Ana Cikic  2006.09.08</author>
    public interface ILongArrayEnumerator : IEnumerator {

        /// <summary>
        /// Returns the index of the current value, which is the value
        /// returned by the most recent call to the <b>MoveNext</b> method.
        /// </summary>
        /// <returns>
        /// The index of the current value.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the <b>MoveNext</b> method has not yet been called, or the
        /// <b>remove</b> method has already been called after the last call
        /// to the <b>MoveNext</b> method.
        /// </exception>
        long Index { get; }

        /// <summary>
        /// Returns the current value, which is the same value returned by
        /// the most recent call to the <b>MoveNext</b> method, or the most
        /// recent value passed to <b>SetValue</b> if <b>SetValue</b> were
        /// called after the <b>MoveNext</b> method.
        /// </summary>
        /// <returns>
        /// The current value.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the <b>MoveNext</b> method has not yet been called, or the
        /// <b>Remove</b> method has already been called after the last call
        /// to the <b>MoveNext</b> method.
        /// </exception>
        Object GetValue();

        /// <summary>
        /// Stores a new value at the current value index, returning the
        /// value that was replaced. The index of the current value is
        /// obtainable by reading the <b>Index</b> property.
        /// </summary>
        /// <param name="value">
        /// The new value to store.
        /// </param>
        /// <returns>
        /// The replaced value.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the <b>MoveNext</b> method has not yet been called, or the
        /// <b>Remove</b> method has already been called after the last call
        /// to the <b>MoveNext</b> method.
        /// </exception>
        Object SetValue(Object value);

    }
}
