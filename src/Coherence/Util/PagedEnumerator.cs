/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;

namespace Tangosol.Util
{
    /// <summary>
    /// PagedEnumerator is an <see cref="IEnumerator"/> implementation based
    /// on a concept of a <i>page Advancer</i> - a pluggable component that
    /// knows how to supply a next page of objects to iterate through.
    /// </summary>
    /// <remarks>
    /// As common to iterators, this implementation is not thread safe.
    /// </remarks>
    /// <author>Gene Gleyzer  2008.01.25</author>
    /// <author>Ana Cikic  2008.08.11</author>
    /// <since>Coherence 3.4</since>
    public class PagedEnumerator : IEnumerator
    {
        #region Constructors

        /// <summary>
        /// Construct a PagedEnumerator based on the specified
        /// <see cref="IAdvancer"/>.
        /// </summary>
        /// <param name="advancer">
        /// The underlying <b>IAdvancer</b>.
        /// </param>
        public PagedEnumerator(IAdvancer advancer)
        {
            Debug.Assert(advancer != null);
            m_advancer = advancer;
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
        /// <exception cref="InvalidOperationException">
        /// The collection was modified after the enumerator was created.
        /// </exception>
        public virtual bool MoveNext()
        {
            IEnumerator enumerator = m_enumPage;
            while (enumerator == null || !enumerator.MoveNext())
            {
                ICollection colPage = m_advancer.NextPage();
                if (colPage == null)
                {
                    return false;
                }
                enumerator = m_enumPage = colPage.GetEnumerator();
            }
            return true;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the
        /// first element in the collection.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified after the enumerator was created.
        /// </exception>
        public virtual void Reset()
        {
            m_advancer.Reset();

            m_enumPage = null;
            m_curr     = null;
        }

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        /// <returns>
        /// The current element in the collection.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The enumerator is positioned before the first element of the
        /// collection or after the last element.
        /// </exception>
        public virtual object Current
        {
            get
            {
                IEnumerator enumerator = m_enumPage;
                if (enumerator != null)
                {
                    return m_curr = enumerator.Current;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        #endregion

        #region Inner interface: IAdvancer

        /// <summary>
        /// Advancer is a pluggable component that knows how to load a new
        /// page (collection) of objects to be used by the enclosing
        /// <see cref="PagedEnumerator"/>.
        /// </summary>
        public interface IAdvancer
        {
            /// <summary>
            /// Obtain a new page of objects to be used by the enclosing
            /// <see cref="PagedEnumerator"/>.
            /// </summary>
            /// <returns>
            /// A collection of objects or <c>null</c> if the advancer is
            /// exhausted.
            /// </returns>
            ICollection NextPage();

            /// <summary>
            /// Sets the advancer to its initial position, which is before
            /// the first page.
            /// </summary>
            void Reset();

            /// <summary>
            /// Remove the specified object from the underlying collection.
            /// </summary>
            /// <remarks>
            /// Naturally, only an object from the very last non-empty page
            /// could be removed.
            /// </remarks>
            /// <param name="curr">
            /// Currently "active" item to be removed from an underlying
            /// collection.
            /// </param>
            void Remove(object curr);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying IAdvancer.
        /// </summary>
        protected IAdvancer m_advancer;

        /// <summary>
        /// An IEnumerator for the current page.
        /// </summary>
        protected IEnumerator m_enumPage;

        /// <summary>
        /// Currently "Active" object.
        /// </summary>
        protected object m_curr;

        #endregion
    }
}