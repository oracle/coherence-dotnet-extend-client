/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

namespace Tangosol.Util
{
    /// <summary>
    /// An interface, similar in its methods to <b>IList</b>, and similar in
    /// its purpose to an array, designed for sparse storage and indexed by
    /// long values.
    /// </summary>
    /// <remarks>
    /// Unlike the <b>IList</b> interface, the ILongArray interface assumes
    /// that every valid index (i.e. greater than or equal to zero) can be
    /// accessed and has storage available.
    /// </remarks>
    /// <author>Cameron Purdy</author>
    /// <author>Ana Cikic  2006.09.08</author>
    public interface ILongArray
    {
        /// <summary>
        /// The value stored at the specified index.
        /// </summary>
        /// <param name="index">
        /// A long index value.
        /// </param>
        /// <value>
        /// The object stored at the specified index, or <c>null</c>.
        /// </value>
        object this[long index] { get; set; }

        /// <summary>
        /// Add the passed element value to the ILongArray and return the
        /// index at which the element value was stored.
        /// </summary>
        /// <param name="value">
        /// The object to add to the ILongArray.
        /// </param>
        /// <returns>
        /// The long index value at which the element value was stored.
        /// </returns>
        long Add(object value);

        /// <summary>
        /// Determine if the specified index is in use.
        /// </summary>
        /// <param name="index">
        /// A long index value.
        /// </param>
        /// <returns>
        /// <b>true</b> if a value (including <c>null</c>) is stored at the
        /// specified index, otherwise <b>false</b>.
        /// </returns>
        bool Exists(long index);

        /// <summary>
        /// Remove the specified index from the ILongArray, returning its
        /// associated value.
        /// </summary>
        /// <param name="index">
        /// The index into the ILongArray.
        /// </param>
        /// <returns>
        /// The associated value (which can be <c>null</c>) or <c>null</c> if
        /// the specified index is not in the ILongArray.
        /// </returns>
        object Remove(long index);

        /// <summary>
        /// Determine if the ILongArray contains the specified element.
        /// </summary>
        /// <remarks>
        /// More formally, returns <b>true</b> if and only if this ILongArray
        /// contains at least one element <b>e</b> such that
        /// <b>(o==null ? e==null : o.Equals(e))</b>.
        /// </remarks>
        /// <param name="value">
        /// Element whose presence in this list is to be tested.
        /// </param>
        /// <returns>
        /// <b>true</b> if this list contains the specified element.
        /// </returns>
        bool Contains(object value);

        /// <summary>
        /// Remove all elements from the ILongArray.
        /// </summary>
        void Clear();

        /// <summary>
        /// Determine if ILongArray is empty.
        /// </summary>
        /// <value>
        /// <b>true</b> if ILongArray has no elements.
        /// </value>
        bool IsEmpty { get; }

        /// <summary>
        /// Determine the size of the ILongArray.
        /// </summary>
        /// <value>
        /// The number of elements in the ILongArray.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Determine the first index that exists in the ILongArray.
        /// </summary>
        /// <value>
        /// The lowest long value, 0 &lt;= n &lt;= Int64.MaxValue, that
        /// exists in this ILongArray, or -1 if the ILongArray is empty.
        /// </value>
        long FirstIndex { get; }

        /// <summary>
        /// Determine the last index that exists in the ILongArray.
        /// </summary>
        /// <value>
        /// The highest long value, 0 &lt;= n &lt;= Int64.MaxValue, that
        /// exists in this ILongArray, or -1 if the ILongArray is empty.
        /// </value>
        long LastIndex { get; }

        /// <summary>
        /// Provide a string representation of the ILongArray.
        /// </summary>
        /// <returns>
        /// A human-readable string value describing the ILongArray instance.
        /// </returns>
        string ToString();

        /// <summary>
        /// Test for ILongArray equality.
        /// </summary>
        /// <param name="o">
        /// An Object to compare to this ILongArray for equality.
        /// </param>
        /// <returns>
        /// <b>true</b> if the passed Object is an ILongArray containing the
        /// same indexes and whose elements at those indexes are equal.
        /// </returns>
        bool Equals(object o);

        /// <summary>
        /// Obtain an IEnumerator of the contents of the ILongArray.
        /// </summary>
        /// <returns>
        /// An instance of IEnumerator.
        /// </returns>
        IEnumerator GetEnumerator();

        /// <summary>
        /// Obtain an IEnumerator of the contents of the ILongArray,
        /// starting at a particular index such that the first call to
        /// <b>MoveNext</b> will set the location of the enumerator at the
        /// first existent index that is greater than or equal to the
        /// specified index, or will throw an <b>IndexOutOfRangeException</b>
        /// if there is no such existent index.
        /// </summary>
        /// <param name="index">
        /// The ILongArray index to iterate from.
        /// </param>
        /// <returns>
        /// An instance of IEnumerator.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// If index greater than or equal to the specified index does not
        /// exist.
        /// </exception>
        IEnumerator GetEnumerator(long index);

        /// <summary>
        /// Gets an object that can be used to synchronize access to this
        /// ILongArray.
        /// </summary>
        /// <value>
        /// Object used to synchronize access to this ILongArray.
        /// </value>
        object SyncRoot { get; }
    }
}