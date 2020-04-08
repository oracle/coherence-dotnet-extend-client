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
    /// An implementation of <see cref="IEnumerator"/> which converts each of
    /// the items which it enumerates.
    /// </summary>
    /// <author>Cameron Purdy  2002.02.07</author>
    /// <author>Ana Cikic  2008.05.30</author>
    public class ConverterEnumerator : IEnumerator
    {
        #region Constructor

        /// <summary>
        /// Construct the Converter enumerator based on an
        /// <see cref="IEnumerator"/>.
        /// </summary>
        /// <param name="enumerator">
        /// <b>IEnumerator</b> of objects to convert.
        /// </param>
        /// <param name="conv">
        /// An <see cref="IConverter"/>.
        /// </param>
        public ConverterEnumerator(IEnumerator enumerator, IConverter conv)
        {
            m_enum = enumerator;
            m_conv = conv;
        }

        /// <summary>
        /// Construct the Converter enumerator based on an array of objects.
        /// </summary>
        /// <param name="items">
        /// Array of objects to enumerate.
        /// </param>
        /// <param name="conv">
        /// An <see cref="IConverter"/>.
        /// </param>
        public ConverterEnumerator(object[] items, IConverter conv)
            : this(new SimpleEnumerator(items), conv)
        {}

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
            return m_enum.MoveNext();
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
            m_enum.Reset();
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
            get { return m_conv.Convert(m_enum.Current); }
        }

        #endregion

        #region Data members

        /// <summary>
        /// IEnumerator of objects to convert.
        /// </summary>
        protected IEnumerator m_enum;

        /// <summary>
        /// IConverter to convert each item.
        /// </summary>
        protected IConverter m_conv;

        #endregion

    }
}
