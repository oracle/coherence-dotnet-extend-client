/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// ICacheEventTransformer interface is used to allow an event consumer
    /// to change the content of a <see cref="CacheEventArgs"/> destined for
    /// the corresponding <see cref="ICacheListener"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// In general, the <see cref="Transform"/> method is called after the
    /// original <b>CacheEventArgs</b> is evaluated by an
    /// <see cref="IFilter"/> (such as
    /// <see cref="Tangosol.Util.Filter.CacheEventFilter"/>). The values
    /// contained by the returned <b>CacheEventArgs</b> object will be the
    /// ones given (sent) to the corresponding listener. Returning
    /// <c>null</c> will prevent the emission of the event altogether.</p>
    /// <p>
    /// <b>Note:</b> Currently, the ICacheEventTransformer interface is
    /// supported only by partitioned caches.</p>
    /// </remarks>
    /// <author>Gene Gleyzer/Jason Howes  2008.05.01</author>
    /// <author>Ana Cikic  2008.06.17</author>
    /// <since>Coherence 3.4</since>
    public interface ICacheEventTransformer
    {
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
        CacheEventArgs Transform(CacheEventArgs evt);
    }
}