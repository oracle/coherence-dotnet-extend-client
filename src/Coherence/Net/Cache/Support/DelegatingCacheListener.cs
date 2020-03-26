/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Runtime.CompilerServices;

using Tangosol.Net;
using Tangosol.Net.Cache;

namespace Tangosol.Net.Cache.Support
{
    /// <summary>
    /// Represents the method that will handle cache events.
    /// </summary>
    /// <param name="sender">
    /// <see cref="INamedCache"/> that raised an event.
    /// </param>
    /// <param name="args">
    /// Event arguments.
    /// </param>
    public delegate void CacheEventHandler(object sender, CacheEventArgs args);

    /// <summary>
    /// Basic .NET style <see cref="ICacheListener"/> implementation.
    /// </summary>
    /// <remarks>
    /// This class allows users to register for cache events using
    /// .NET-style event/delegate mechanism. When it recieves cache
    /// event, it will raise the corresponding .NET event.
    /// </remarks>
    public class DelegatingCacheListener : ICacheListener
    {
        /// <summary>
        /// Occurs when a new entry is added to the cache.
        /// </summary>
        public virtual event CacheEventHandler EntryInserted;

        /// <summary>
        /// Occurs when a cache entry is updated.
        /// </summary>
        public virtual event CacheEventHandler EntryUpdated;

        /// <summary>
        /// Occurs when an entry is deleted from the cache.
        /// </summary>
        public virtual event CacheEventHandler EntryDeleted;

        /// <summary>
        /// Invoked when a dictionary entry has been inserted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the insert
        /// information.
        /// </param>
        void ICacheListener.EntryInserted(CacheEventArgs evt)
        {
            OnInserted(evt);
        }

        /// <summary>
        /// Invoked when a dictionary entry has been updated.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the update
        /// information.
        /// </param>
        void ICacheListener.EntryUpdated(CacheEventArgs evt)
        {
            OnUpdated(evt);
        }

        /// <summary>
        /// Invoked when a dictionary entry has been deleted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the remove
        /// information.
        /// </param>
        void ICacheListener.EntryDeleted(CacheEventArgs evt)
        {
            OnDeleted(evt);
        }

        /// <summary>
        /// Raises the EntryInserted event.
        /// </summary>
        /// <param name="args">
        /// Cache event arguments containing event data.
        /// </param>
        protected virtual void OnInserted(CacheEventArgs args)
        {
            InvokeCacheEvent(EntryInserted, args);
        }

        /// <summary>
        /// Raises the EntryUpdated event.
        /// </summary>
        /// <param name="args">
        /// Cache event arguments containing event data.
        /// </param>
        protected virtual void OnUpdated(CacheEventArgs args)
        {
            InvokeCacheEvent(EntryUpdated, args);
        }

        /// <summary>
        /// Raises the EntryDeleted event.
        /// </summary>
        /// <param name="args">
        /// Cache event arguments containing event data.
        /// </param>
        protected virtual void OnDeleted(CacheEventArgs args)
        {
            InvokeCacheEvent(EntryDeleted, args);
        }

        /// <summary>
        /// Invokes the event, with special remark towards multithreading 
        /// (using local copy of delegate and no inline attribute for method). 
        /// </summary>
        /// <param name="handler">
        /// The CacheEventHandler event that's being invoked. 
        /// </param>
        /// <param name="args">
        /// Event arguments.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InvokeCacheEvent(CacheEventHandler handler, CacheEventArgs args)
        {
            if (handler != null)
            {
                handler(args.Cache, args);
            }
        }
    }
}