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
    /// An abstract base class for custom cache eviction policies.
    /// </summary>
    /// <author>Jason Howes  2005.12.14</author>
    /// <author>Ivan Cikic  2007.05.17</author>
    /// <author>Aleksandar Seovic  2009.07.28</author>
    public abstract class AbstractEvictionPolicy : IEvictionPolicy, ICacheListener
    {
        #region Properties
        
        /// <summary>
        /// Return the <see cref="IConfigurableCache"/> that uses this 
        /// eviction policy.
        /// </summary>
        /// <remarks>
        /// The <b>IConfigurableCache</b> is set the first time a cache event 
        /// is processed by the eviction policy.
        /// </remarks>
        /// <value>
        /// The <b>IConfigurableCache</b> or <c>null</c> if a cache event has 
        /// not yet been processed by this eviction policy.
        /// </value>
        protected IConfigurableCache Cache
        {
            get { return m_cache; }
        }

        #endregion
                
        #region IEvictionPolicy interface implementation

        /// <summary>
        /// This method is called by the cache to indicate that an entry
        /// has been touched.
        /// </summary>
        /// <param name="entry">
        /// The <see cref="IConfigurableCacheEntry"/> that has been touched.
        /// </param>
        public abstract void EntryTouched(IConfigurableCacheEntry entry);

        /// <summary>
        /// This method is called by the cache when the cache requires
        /// the eviction policy to evict entries.
        /// </summary>
        /// <param name="maximum">
        /// The maximum number of units that should remain in the cache
        /// when the eviction is complete.
        /// </param>
        public abstract void RequestEviction(long maximum);

        /// <summary>
        /// Obtain the name of the eviction policy. 
        /// </summary>
        /// <value>
        /// The name of the eviction policy.
        /// </value>
        public virtual String Name
        {
            get { return GetType().Name; }
        }

        #endregion

        #region ICacheListener interface implementation

        /// <summary>
        /// Invoked when a cache entry has been inserted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the insert
        /// information.
        /// </param>
        public virtual void EntryInserted(CacheEventArgs evt)
        {
            EntryUpdated(GetEntry(evt));
        }

        /// <summary>
        /// Invoked when a cache entry has been updated.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the update
        /// information.
        /// </param>
        public virtual void EntryUpdated(CacheEventArgs evt)
        {
            EntryUpdated(GetEntry(evt));
        }

        /// <summary>
        /// Invoked when a cache entry has been deleted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the remove
        /// information.
        /// </param>
        public virtual void EntryDeleted(CacheEventArgs evt)
        {
            EnsureCache(evt);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// This method is called to indicate that an entry has been either 
        /// inserted or updated.
        /// </summary>
        /// <param name="entry">
        /// The cache entry that has been updated.
        /// </param>
        public abstract void EntryUpdated(IConfigurableCacheEntry entry);

        /// <summary>
        /// Return the <see cref="IConfigurableCache"/> that uses this eviction
        /// policy. 
        /// </summary>
        /// <remarks>
        /// If the <b>LocalCache</b> property has not been intialized, it is
        /// set to the <b>LocalCache</b> that raised the given event.
        /// </remarks>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> raised by the <b>LocalCache</b>
        /// that uses this eviction policy.
        /// </param>
        /// <returns>
        /// The <b>LocalCache</b> that uses this eviction policy.
        /// </returns>
        protected virtual IConfigurableCache EnsureCache(CacheEventArgs evt)
        {
            IConfigurableCache cache = m_cache;
            if (cache == null)
            {
                try
                {
                    cache = m_cache = (IConfigurableCache) evt.Cache;
                }
                catch (InvalidCastException)
                {
                    throw new InvalidCastException("Illegal cache type: " +
                                                   evt.Cache.GetType().Name);
                }
            }
            return cache;
        }

        /// <summary>
        /// Return the cache entry associated with the given cache event.
        /// </summary>
        /// <param name="evt">
        /// A cache event raised by the <see cref="IConfigurableCache"/> 
        /// that uses this eviction policy.
        /// </param>
        /// <returns>
        /// The cache entry associated with the given event.
        /// </returns>
        protected virtual IConfigurableCacheEntry GetEntry(CacheEventArgs evt)
        {
            IConfigurableCache cache = EnsureCache(evt);

            // if the IConfigurableCache.GetCacheEntry() implementation
            // causes an eviction or notifies the eviction policy that the
            // entry has been touched, we'll go into an infinite recursion
            try
            {
                return cache.GetCacheEntry(evt.Key);
            }
            catch (StackOverflowException)
            {
                throw new StackOverflowException(cache.GetType().FullName
                    + "#GetCacheEntry() implementation causes an infinite"
                    + " recursion when used with " + GetType().FullName
                    + "#getEntry() implementation (inherited from "
                    + typeof(AbstractEvictionPolicy).FullName + ")");
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The <see cref="IConfigurableCache"/> that is using this eviction 
        /// policy.
        /// </summary>
        private volatile IConfigurableCache m_cache;

        #endregion
    }
}
