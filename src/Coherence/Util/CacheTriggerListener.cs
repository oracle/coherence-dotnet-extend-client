/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;

using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;

namespace Tangosol.Util
{
    /// <summary>
    /// CacheTriggerListener is a special purpose
    /// <see cref="ICacheListener"/> implementation that is used to register
    /// a <see cref="ICacheTrigger"/> on a corresponding
    /// <see cref="IObservableCache"/>.
    /// </summary>
    /// <remarks>
    /// <b>Note:</b> Currently, the CacheTriggerListener can only be
    /// registered with partitioned caches and only "globally" (without
    /// specifying any filter or key), using the
    /// <see cref="IObservableCache.AddCacheListener(ICacheListener)"/>
    /// method.
    /// </remarks>
    /// <author>Cameron Purdy/Gene Gleyzer  2008.03.11</author>
    /// <author>Ana Cikic  2008.07.02</author>
    /// <since>Coherence 3.4</since>
    public class CacheTriggerListener : MultiplexingCacheListener
    {
        #region Constants

        /// <summary>
        /// Obtain the <see cref="ICacheTrigger"/> agent represented by this
        /// CacheTriggerListener.
        /// </summary>
        /// <returns>
        /// The <b>ICacheTrigger</b> agent represented by this
        /// CacheTriggerListener.
        /// </returns>
        public virtual ICacheTrigger Trigger
        {
            get { return m_trigger; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a CacheTriggerListener that can be used to register the
        /// specified <see cref="ICacheTrigger"/>.
        /// </summary>
        /// <param name="trigger">
        /// The <b>ICacheTrigger</b>.
        /// </param>
        public CacheTriggerListener(ICacheTrigger trigger)
        {
            Debug.Assert(trigger != null, "Null trigger");

            m_trigger = trigger;
        }

        #endregion

        #region MultiplexingCacheListener implementation

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
        protected override void OnCacheEvent(CacheEventArgs evt)
        {
            throw new InvalidOperationException(
                "CacheTriggerListener may not be used as a generic ICacheListener");
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying ICacheTrigger.
        /// </summary>
        private ICacheTrigger m_trigger;

        #endregion
    }
}
