/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Net;

namespace Tangosol.Net
{
    /// <summary>
    /// An <b>ICacheService</b> is a service providing a collection of
    /// named caches that hold resources.
    /// </summary>
    /// <remarks>
    /// These resources are expected to be managed in memory, and are
    /// typically composed of data that are also stored persistently in a
    /// database, or data that have been assembled or calculated at some
    /// significant cost, thus these resources are referred to as
    /// <i>cached</i>.
    /// </remarks>
    /// <author>Gene Gleyzer  2002.02.08</author>
    /// <author>Ana Cikic  2006.09.15</author>
    /// <since>Coherence 1.1</since>
    public interface ICacheService : IService
    {
        /// <summary>
        /// Obtain an <see cref="INamedCache"/> interface that provides a view
        /// of cached resources.
        /// </summary>
        /// <remarks>
        /// The view is identified by name within this ICacheService.
        /// Typically, repeated calls to this method with the same view name
        /// will result in the same view reference being returned.
        /// </remarks>
        /// <param name="name">
        /// The name, within this ICacheService, that uniquely identifies a
        /// view; <c>null</c> is legal, and may imply a default name.
        /// </param>
        /// <returns>
        /// An <b>INamedCache</b> interface which can be used to access the
        /// resources of the specified view.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the service is not running.
        /// </exception>
        INamedCache EnsureCache(string name);

        /// <summary>
        /// A collection of string objects, one for each cache name that has
        /// been previously registered with this ICacheService.
        /// </summary>
        /// <value>
        /// <b>ICollection</b> of cache names.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If the service is not running or has stopped.
        /// </exception>
        ICollection CacheNames { get; }

        /// <summary>
        /// Release local resources associated with the specified instance of
        /// the cache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This invalidates a reference obtained by using the
        /// <see cref="EnsureCache"/> method.</p>
        /// <p>
        /// Releasing a reference to a cache makes the cache reference no
        /// longer usable, but does not affect the cache itself. In other
        /// words, all other references to the cache will still be valid, and
        /// the cache data is not affected by releasing the reference.</p>
        /// <p>
        /// The reference that is released using this method can no longer be
        /// used; any attempt to use the reference will result in an
        /// exception.</p>
        /// </remarks>
        /// <param name="cache">
        /// The cache object to be released.
        /// </param>
        /// <seealso cref="INamedCache.Release"/>
        void ReleaseCache(INamedCache cache);

        /// <summary>
        /// Release and destroy the specified cache.
        /// </summary>
        /// <remarks>
        /// <b>Warning:</b> This method is used to completely destroy the
        /// specified cache across the cluster. All references in the entire
        /// cluster to this cache will be invalidated, the cached data will
        /// be cleared, and all resources will be released.
        /// </remarks>
        /// <param name="cache">
        /// The cache object to be released.
        /// </param>
        /// <seealso cref="INamedCache.Destroy"/>
        void DestroyCache(INamedCache cache);
    }
}