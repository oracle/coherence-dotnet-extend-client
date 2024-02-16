/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Cache
{
    /// <summary>
    /// The listener interface for receiving <see cref="CacheEventArgs"/>.
    /// </summary>
    /// <author>Gene Gleyzer  2002.02.11</author>
    /// <author>Aleksandar Seovic  2006.07.12</author>
    /// <seealso cref="IObservableCache"/>
    /// <seealso cref="CacheEventArgs"/>
    public interface ICacheListener
    {
        /// <summary>
        /// Invoked when a cache entry has been inserted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the insert
        /// information.
        /// </param>
        void EntryInserted(CacheEventArgs evt);

        /// <summary>
        /// Invoked when a cache entry has been updated.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the update
        /// information.
        /// </param>
        void EntryUpdated(CacheEventArgs evt);

        /// <summary>
        /// Invoked when a cache entry has been deleted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the remove
        /// information.
        /// </param>
        void EntryDeleted(CacheEventArgs evt);
    }
}