/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

using Tangosol.Net.Cache;
using Tangosol.Util.Filter;

namespace Tangosol.Util
{
    /// <summary>
    /// Provide a generic implementation of an enumerator which can enumerate
    /// items based on an inclusion test.
    /// </summary>
    /// <author>Cameron Purdy  1997.09.05</author>
    /// <author>Goran Milosavljevic  2006.11.28</author>
    public class FilterEnumerator : IEnumerator
    {
        #region Constructors

        /// <summary>
        /// Construct the FilterEnumerator based on an <b>IEnumerator</b>.
        /// </summary>
        /// <param name="enumerator">
        /// <b>IEnumerator</b> of objects to filter.
        /// </param>
        /// <param name="test">
        /// An inclusion test.
        /// </param>
        public FilterEnumerator(IEnumerator enumerator, IFilter test)
        {
            m_enum    = enumerator;
            m_test    = test;
            m_hasNext = false;
        }

        /// <summary>
        /// Construct the FilterEnumerator based on an array of objects.
        /// </summary>
        /// <param name="items">
        /// Array of objects to enumerate.
        /// </param>
        /// <param name="test">
        /// An inclusion test.
        /// </param>
        public FilterEnumerator(object[] items, IFilter test)
            : this((IEnumerator) new SimpleEnumerator(items), test)
        {}

        #endregion

        #region Internal methods

        /// <summary>
        /// Tests if this <b>IEnumerator</b> contains more elements.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the <b>IEnumerator</b> contains more elements,
        /// <b>false</b> otherwise.
        /// </returns>
        private bool HasNext()
        {
            bool hasNext = m_hasNext;

            if (hasNext)
            {
                return true;
            }

            // find if there is a "next one"
            IEnumerator iter = m_enum;
            IFilter     test = m_test;

            while (iter.MoveNext())
            {
                object next = iter.Current;
                if (EvaluateEntry(test, next))
                {
                    m_next  = next;
                    hasNext = true;
                    break;
                }
            }

            // can't call remove now because we'd end up potentially
            // removing the wrong one
            m_hasPrev = false;
            m_hasNext = hasNext;

            return hasNext;
        }

        /// <summary>
        /// Evaluates entry provided against the filter parameter.
        /// </summary>
        /// <param name="filter">
        /// An inclusion test.
        /// </param>
        /// <param name="entry">
        /// An object to which the test is applied.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        private bool EvaluateEntry(IFilter filter, object entry)
        {
            return filter is IEntryFilter ? ((IEntryFilter) filter).EvaluateEntry((ICacheEntry) entry) : filter.Evaluate(entry);
        }

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
            if (HasNext())
            {
                m_hasNext = false;
                m_hasPrev = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before
        /// the first element in the collection.
        /// </summary>
        public virtual void Reset()
        {
            m_enum.Reset();
        }

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        /// <returns>
        /// The current element in the collection.
        /// </returns>
        public virtual object Current
        {
            get { return m_next; }
        }

        #endregion

        #region Data members

        /// <summary>
        /// Objects to filter/enumerate.
        /// </summary>
        protected IEnumerator m_enum;

        /// <summary>
        /// Test to perform on each item.
        /// </summary>
        protected IFilter m_test;

        /// <summary>
        /// Is there a next item which passed the test?
        /// </summary>
        protected bool m_hasNext;

        /// <summary>
        /// Is there a previous item which passed the test and can be
        /// removed?
        /// </summary>
        protected bool m_hasPrev;

        /// <summary>
        /// The next item which passed the test.
        /// </summary>
        protected object m_next;

        #endregion
    }
}