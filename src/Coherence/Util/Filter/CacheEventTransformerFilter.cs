/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util.Transformer;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// CacheEventTransformerFilter is a generic multiplexing wrapper that
    /// combines two implementations: an <see cref="IFilter"/> (most commonly
    /// a <see cref="CacheEventFilter"/>) and an
    /// <see cref="ICacheEventTransformer"/> and is used to register event
    /// listeners that allow to change the content of a
    /// <see cref="CacheEventArgs"/>.
    /// </summary>
    /// <author>Gene Gleyzer/Jason Howes  2008.05.01</author>
    /// <author>Ana Cikic  2008.06.17</author>
    /// <since>Coherence 3.4</since>
    /// <seealso cref="SemiLiteEventTransformer"/>
    public class CacheEventTransformerFilter : IFilter, ICacheEventTransformer, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CacheEventTransformerFilter()
        {}

        /// <summary>
        /// Construct a CacheEventTransformerFilter based on the specified
        /// <see cref="IFilter"/> and <see cref="ICacheEventTransformer"/>.
        /// </summary>
        /// <param name="filter">
        /// The underlying <b>IFilter</b> (e.g.
        /// <see cref="CacheEventFilter"/>) used to evaluate original
        /// <see cref="CacheEventArgs"/> objects (optional).
        /// </param>
        /// <param name="transformer">
        /// The underlying <b>ICacheEventTransformer</b> used to transform
        /// original <b>CacheEventArgs</b> objects.
        /// </param>
        public CacheEventTransformerFilter(IFilter filter, ICacheEventTransformer transformer)
        {
            if (transformer == null)
            {
                throw new ArgumentNullException("transformer");
            }

            m_filter      = filter;
            m_transformer = transformer;
        }

        #endregion

        #region IFilter implementation

        /// <summary>
        /// Apply the test to the object.
        /// </summary>
        /// <param name="o">
        /// An object to which the test is applied.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        public virtual bool Evaluate(object o)
        {
            if (!(o is CacheEventArgs))
            {
                throw new InvalidOperationException(GetType().FullName
                    + " should not be used as a general purpose filter");
            }
            IFilter filter = m_filter;
            return filter == null || filter.Evaluate(o);
        }

        #endregion

        #region ICacheEventTransformer implementation

        /// <summary>
        /// Transform the specified <see cref="CacheEventArgs"/>.
        /// </summary>
        /// <remarks>
        /// The values contained by the returned <b>CacheEventArgs</b> object
        /// will be the ones given (sent) to the corresponding listener.
        /// </remarks>
        /// <param name="evt">
        /// The original <b>CacheEventArgs</b> object.
        /// </param>
        /// <returns>
        /// Modified <b>CacheEventArgs</b> object or <c>null</c> to discard
        /// the event.
        /// </returns>
        public virtual CacheEventArgs Transform(CacheEventArgs evt)
        {
            return m_transformer.Transform(evt);
        }

        #endregion

        #region IPortableObject Members

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void ReadExternal(IPofReader reader)
        {
            m_filter      = (IFilter) reader.ReadObject(0);
            m_transformer = (ICacheEventTransformer) reader.ReadObject(1);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteExternal(IPofWriter writer)
        {
            writer.WriteObject(0, m_filter);
            writer.WriteObject(1, m_transformer);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Determine a hash value for this object.
        /// </summary>
        /// <returns>
        /// An integer hash value for this object.
        /// </returns>
        public override int GetHashCode()
        {
            IFilter filter = m_filter;
            return (m_filter == null ? 79 : filter.GetHashCode()) + m_transformer.GetHashCode();
        }

        /// <summary>
        /// Compare this object with another object to determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with current object.
        /// </param>
        /// <returns>
        /// <b>true</b> if this object and the passed object are equivalent
        /// objects.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is CacheEventTransformerFilter)
            {
                var that = (CacheEventTransformerFilter) o;
                return Equals(m_filter, that.m_filter) && Equals(m_transformer, that.m_transformer);
            }
            return false;
        }

        /// <summary>
        /// Return a human-readable description for this object.
        /// </summary>
        /// <returns>
        /// A string description of the object.
        /// </returns>
        public override string ToString()
        {
            return GetType().FullName + '(' + m_filter + ", " + m_transformer + ')';
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying IFilter to evaluate CacheEventArgs with.
        /// </summary>
        private IFilter m_filter;

        /// <summary>
        /// The underlying transformer.
        /// </summary>
        private ICacheEventTransformer m_transformer;

        #endregion
    }
}