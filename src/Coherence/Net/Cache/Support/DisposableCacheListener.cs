/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */

using System;

namespace Tangosol.Net.Cache.Support
{
    /// <summary>
    /// Utility listener that checks if the value removed from the cache
    /// implements <c>IDisposable</c>, and calls <c>Dispose</c> method
    /// if it does.
    /// </summary>
    /// <author>Aleksandar Seovic  2010.04.06</author>
    public class DisposableCacheListener : AbstractCacheListener
    {
        /// <summary>
        /// Invoked when a cache entry has been deleted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the remove
        /// information.
        /// </param>
        public override void EntryDeleted(CacheEventArgs evt)
        {
            IDisposable disposable = evt.OldValue as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}