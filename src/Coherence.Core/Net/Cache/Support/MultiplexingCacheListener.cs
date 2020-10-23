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
    /// <see cref="ICacheListener"/> by multiplexing all events into a single
    /// listener method.
    /// </summary>
    /// <author>Cameron Purdy  2006.01.19</author>
    /// <author>Ivan Cikic  2006.11.09</author>
    /// <since>Coherence 3.1</since>
    public abstract class MultiplexingCacheListener : ICacheListener
    {
        /// <summary>
        /// Invoked when a cache entry has been inserted, updated or
        /// deleted.
        /// </summary>
        /// <remarks>
        ///  To determine what action has occurred, use
        /// <see cref="CacheEventArgs.EventType"/> property.
        /// </remarks>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the insert, update or
        /// delete information.
        /// </param>
        protected abstract void OnCacheEvent(CacheEventArgs evt);

        #region ICacheListener implementation

        /// <summary>
        /// Invoked when a cache entry has been inserted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the insert
        /// information.
        /// </param>
        public void EntryInserted(CacheEventArgs evt)
        {
            OnCacheEvent(evt);
        }

        /// <summary>
        /// Invoked when a cache entry has been updated.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the update
        /// information.
        /// </param>
        public void EntryUpdated(CacheEventArgs evt)
        {
            OnCacheEvent(evt);
        }

        /// <summary>
        /// Invoked when a cache entry has been deleted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the remove
        /// information.
        /// </param>
        public void EntryDeleted(CacheEventArgs evt)
        {
            OnCacheEvent(evt);
        }

        #endregion

    }
}
