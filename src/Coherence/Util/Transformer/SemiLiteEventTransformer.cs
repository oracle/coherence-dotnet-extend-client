/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Transformer
{
    /// <summary>
    /// SemiLiteEventTransformer is a special purpose
    /// <see cref="ICacheEventTransformer"/> implementation that removes an
    /// <see cref="CacheEventArgs.OldValue"/> from the
    /// <see cref="CacheEventArgs"/> object for the purpose of reducing the
    /// amount of data that has to be sent over the network to event
    /// consumers.
    /// </summary>
    /// <remarks>
    /// Usage example:
    /// <code>
    /// cache.AddCacheListener(listener,
    ///     new CacheEventTransformerFilter(null,
    ///     SemiLiteEventTransformer.Instance), false);
    /// </code>
    /// </remarks>
    /// <author>Gene Gleyzer/Jason Howes  2008.05.01</author>
    /// <author>Ana Cikic  2008.06.17</author>
    /// <since>Coherence 3.4</since>
    public class SemiLiteEventTransformer : ICacheEventTransformer, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SemiLiteEventTransformer()
        {}

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
            if (evt is ConverterCollections.ConverterCacheEventArgs)
            {
                ConverterCollections.ConverterCacheEventArgs evtConv = (ConverterCollections.ConverterCacheEventArgs) evt;
                CacheEventArgs                               evtOrig = evtConv.CacheEvent;

                return ConverterCollections.GetCacheEventArgs(evtConv.Cache,
                                                              new CacheEventArgs(evtOrig.Cache, evtOrig.EventType, evtOrig.Key, null, evtOrig.NewValue, evt.IsSynthetic, evt.IsPriming),
                                                              evtConv.ConverterKeyUp, evtConv.ConverterValueUp);
            }
            else
            {
                return new CacheEventArgs(evt.Cache, evt.EventType, evt.Key, null, evt.NewValue, evt.IsSynthetic, evt.IsPriming);
            }
        }

        #endregion

        #region IPortableObject implementation

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
        {}

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
        {}

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
            return 79;
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
            return o is SemiLiteEventTransformer;
        }

        /// <summary>
        /// Return a human-readable description for this object.
        /// </summary>
        /// <returns>
        /// A string description of the object.
        /// </returns>
        public override string ToString()
        {
            return GetType().FullName + "@" + GetHashCode();
        }

        #endregion

        #region Constants

        /// <summary>
        /// The SemiLiteEventTransformer singleton.
        /// </summary>
        public static readonly SemiLiteEventTransformer Instance = new SemiLiteEventTransformer();

        #endregion
    }
}