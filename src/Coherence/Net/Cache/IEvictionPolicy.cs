/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// An eviction policy is an object that the cache provides with access
    /// information, and when requested, the eviction policy selects and
    /// evicts entries from the cache. 
    /// </summary>
    /// <remarks>
    /// If the eviction policy needs to be aware of changes to the cache, 
    /// it must implement the <see cref="ICacheListener"/> interface; 
    /// if it does, it will automatically be registered to receive
    /// cache events.
    /// </remarks>
    /// <seealso cref="AbstractEvictionPolicy"/>
    /// <seealso cref="ICacheListener"/> 
    /// <author>Cameron Purdy  2009.01.13</author>
    /// <author>Aleksandar Seovic  2009.07.27</author>
    /// <since>Coherence 3.5.1</since>
    public interface IEvictionPolicy
    {
        /// <summary>
        /// This method is called by the cache to indicate that an entry has
        /// been touched.
        /// </summary>
        /// <param name="entry">
        /// The cache entry that has been touched.
        /// </param>
        void EntryTouched(IConfigurableCacheEntry entry);

        /// <summary>
        /// This method is called by the cache when the cache requires the
        /// eviction policy to evict entries.
        /// </summary>
        /// <param name="maximum">
        /// The maximum number of units that should remain in the cache when 
        /// the eviction is complete.
        /// </param>
        void RequestEviction(long maximum);

        /// <summary>
        /// Obtain the name of the eviction policy. 
        /// </summary>
        /// <remarks>
        /// This is intended to be human readable for use in a monitoring tool; 
        /// examples include "LRU" and "LFU".
        /// </remarks>
        /// <value>
        /// The name of the eviction policy.
        /// </value>
        String Name { get; }
    }
}