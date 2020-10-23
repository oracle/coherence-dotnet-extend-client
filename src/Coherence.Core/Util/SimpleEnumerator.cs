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
    /// Provide a generic implementation of an array enumerator.
    /// </summary>
    /// <author>Cameron Purdy  1998.08.07</author>
    /// <author>Ivan Cikic  2006.11.27</author>
    public class SimpleEnumerator : IEnumerator
    {
        #region Constructors

        /// <summary>
        /// Construct the simple enumerator based on an array of objects,
        /// making a copy of the array if specified.
        /// </summary>
        /// <param name="items">
        /// Array of objects to enumerate.
        /// </param>
        /// <param name="start">
        /// The first object position.
        /// </param>
        /// <param name="countItems">
        /// The number of objects to enumerate.
        /// </param>
        /// <param name="forward">
        /// <b>true</b> to iterate forwards, <b>false</b> to iterate from the
        /// end backwards to the beginning.
        /// </param>
        /// <param name="copy">
        /// Pass <b>true</b> to make a copy of the array or <b>false</b> if
        /// the array's contents will not change.
        /// </param>
        public SimpleEnumerator(object[] items, int start, int countItems,
                                bool forward, bool copy)
        {
            if (countItems < 0)
            {
                throw new ArgumentOutOfRangeException("Negative count");
            }
            if (forward
                    ? start < 0 || start + countItems > items.Length
                    : start >= items.Length || start - countItems < 0)
            {
                throw new ArgumentOutOfRangeException("Off limits");
            }

            // only copy if there are at least two items in the iterator
            if (copy && countItems > 1)
            {
                items = (object[])items.Clone();
            }

            m_items   = items;
            m_forward = forward;
            m_index   = start;
            m_limit   = forward ? start + countItems : start - countItems;
        }

        /// <summary>
        /// Construct the simple enumerator based on an array of objects.
        /// </summary>
        /// <param name="items">
        /// Array of objects to enumerate.
        /// </param>
        /// <param name="start">
        /// The first object position.
        /// </param>
        /// <param name="countItems">
        /// The number of objects to enumerate.
        /// </param>
        public SimpleEnumerator(object[] items, int start, int countItems)
            : this(items, start, countItems, true, false)
        {}

        /// <summary>
        /// Construct a simple enumerator based on a collection.
        /// </summary>
        /// <param name="col">
        /// The <b>ICollection</b> to enumerate.
        /// </param>
        public SimpleEnumerator(ICollection col)
            : this(CollectionUtils.ToArray(col))
        {}

        /// <summary>
        /// Construct the simple enumerator based on an array of objects.
        /// </summary>
        /// <param name="items">
        /// Array of objects to enumerate.
        /// </param>
        public SimpleEnumerator(object[] items)
            : this(items, 0, items.Length, true, false)
        {}

        #endregion

        #region Data members

        /// <summary>
        /// Array of items to enumerate.
        /// </summary>
        protected object[] m_items;

        /// <summary>
        /// Iterator position:  next item to return.
        /// </summary>
        protected int m_index;

        /// <summary>
        /// Iterator end position (beyond last).
        /// </summary>
        protected int m_limit;

        /// <summary>
        /// Iterator direction.
        /// </summary>
        protected bool m_forward;

        /// <summary>
        /// Iterator start position.
        /// </summary>
        protected int m_startindex;

        #endregion

        #region IEnumerator implementation

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the enumerator was successfully advanced to the
        /// next element; <b>false</b> if the enumerator has passed the end
        /// of the collection.
        /// </returns>
        public virtual bool MoveNext()
        {
            bool hasNext = m_forward ? m_index < m_limit : m_index > m_limit;
            if (!hasNext)
            {
                return false;
            }
            if (m_forward)
            {
                m_index++;
            }
            else
            {
                m_index--;
            }
            return true;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the
        /// first element in the collection.
        /// </summary>
        public virtual void Reset()
        {
            m_index = m_startindex;
        }

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        /// <returns>
        /// The current element in the collection.
        /// </returns>
        public virtual object Current
        {
            get
            {
                if (m_forward && m_index <= m_limit)
                {
                    return m_items[m_index - 1];
                }
                else if (!m_forward && m_index >= m_limit)
                {
                    return m_items[m_index + 1];
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        #endregion
    }
}