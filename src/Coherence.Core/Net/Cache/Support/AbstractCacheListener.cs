/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Cache.Support
{
    /// <summary>
    /// A base class that simplifies the implementation of a
    /// <see cref="ICacheListener"/>, particularly classes that only implement
    /// one or two of the three event methods.
    /// </summary>
    /// <author>Cameron Purdy  2006.01.18</author>
    /// <author>Ivan Cikic  2006.11.09</author>
    /// <since>Coherence 3.1</since>
    public abstract class AbstractCacheListener : ICacheListener
    {
        /// <summary>
        /// Invoked when a cache entry has been inserted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the insert
        /// information.
        /// </param>
        public virtual void EntryInserted(CacheEventArgs evt)
        {}

        /// <summary>
        /// Invoked when a cache entry has been updated.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the update
        /// information.
        /// </param>
        public virtual void EntryUpdated(CacheEventArgs evt)
        {}

        /// <summary>
        /// Invoked when a cache entry has been deleted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the remove
        /// information.
        /// </param>
        public virtual void EntryDeleted(CacheEventArgs evt)
        {}
    }
}