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
    /// A data structure resembling an array keyed by .NET long values.
    /// </summary>
    /// <remarks>
    /// LongSortedList structure is based on
    /// <b>System.Collections.SortedList</b> collection.
    /// </remarks>
    /// <author>Ivan Cikic  2006.09.08</author>
    public class LongSortedList : ILongArray
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LongSortedList()
        {
            m_innerList = new SortedList();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The value stored at the specified index.
        /// </summary>
        /// <param name="index">
        /// A long index value.
        /// </param>
        /// <value>
        /// The object stored at the specified index, or <c>null</c>.
        /// </value>
        public virtual object this[long index]
        {
            get { return m_innerList[index]; }
            set { m_innerList[index] = value; }
        }

        /// <summary>
        /// Determine if ILongArray is empty.
        /// </summary>
        /// <value>
        /// <b>true</b> if ILongArray has no elements.
        /// </value>
        public virtual bool IsEmpty
        {
            get { return Count == 0; }
        }

        /// <summary>
        /// Determine the size of the ILongArray.
        /// </summary>
        /// <value>
        /// The number of elements in the ILongArray.
        /// </value>
        public virtual int Count
        {
            get { return m_innerList.Count; }
        }

        /// <summary>
        /// Determine the first index that exists in the ILongArray.
        /// </summary>
        /// <value>
        /// The lowest long value, 0 &lt;= n &lt;= Int64.MaxValue, that
        /// exists in this ILongArray, or -1 if the ILongArray is empty.
        /// </value>
        public virtual long FirstIndex
        {
            get
            {
                if (m_innerList.Count == 0)
                {
                    return -1L;
                }
                else
                {
                    return (long) m_innerList.GetKey(0);
                }
            }
        }

        /// <summary>
        /// Determine the last index that exists in the ILongArray.
        /// </summary>
        /// <value>
        /// The highest long value, 0 &lt;= n &lt;= Int64.MaxValue, that
        /// exists in this ILongArray, or -1 if the ILongArray is empty.
        /// </value>
        public virtual long LastIndex
        {
            get
            {
                if (m_innerList.Count == 0)
                {
                    return -1L;
                }
                else
                {
                    return (long) m_innerList.GetKey(Count - 1);
                }
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to this
        /// ILongArray.
        /// </summary>
        /// <value>
        /// Object used to synchronize access to this ILongArray.
        /// </value>
        public virtual object SyncRoot
        {
            get { return m_innerList.SyncRoot; }
        }

        #endregion

        #region ILongArray implementation

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
        public virtual long Add(object value)
        {
            long index = LastIndex + 1L;
            m_innerList.Add(index, value);
            return index;
        }

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
        public virtual bool Exists(long index)
        {
            return m_innerList.ContainsKey(index);
        }

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
        public virtual object Remove(long index)
        {
            if (m_innerList.ContainsKey(index))
            {
                object o = m_innerList[index];
                m_innerList.Remove(index);
                return o;
            }
            return null;
        }

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
        public virtual bool Contains(object value)
        {
            return m_innerList.ContainsValue(value);
        }

        /// <summary>
        /// Remove all elements from the ILongArray.
        /// </summary>
        public virtual void Clear()
        {
            m_innerList.Clear();
        }

        /// <summary>
        /// Obtain an IEnumerator of the contents of the ILongArray.
        /// </summary>
        /// <returns>
        /// An instance of IEnumerator.
        /// </returns>
        public virtual IEnumerator GetEnumerator()
        {
            return m_innerList.GetEnumerator();
        }

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
        public virtual IEnumerator GetEnumerator(long index)
        {
            if (index > LastIndex)
            {
                throw new IndexOutOfRangeException("index must be non-negative or smaller then list size");
            }

            IDictionaryEnumerator de    = m_innerList.GetEnumerator();
            IDictionaryEnumerator preDe = m_innerList.GetEnumerator();

            if (!IsEmpty)
            {
                while (de.MoveNext() && (long) de.Key < index)
                {
                    preDe.MoveNext();
                }
            }
            return preDe;
        }

        #endregion

        #region Data members

        /// <summary>
        /// A <b>SortedList</b> used for storing sorted key/value objects.
        /// </summary>
        private SortedList m_innerList;

        #endregion
    }
}