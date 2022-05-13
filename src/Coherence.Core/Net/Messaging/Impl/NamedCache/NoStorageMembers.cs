/*
 * Copyright (c) 2022, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Net.Impl;
using Tangosol.Util;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// Message indicating no storage members are available to
    /// service a request.
    /// </summary>
    /// <author>rl  2022.05.13</author>
    /// <seealso cref="NamedCacheRequest"/>
    /// <seealso cref="NamedCacheProtocol"/>
    public class NoStorageMembers : NamedCacheRequest
    {
        #region Properties

        /// <inheritdoc />
        public override int TypeId
        {
            get { return TYPE_ID; }
        }
        
        #endregion
        
        #region Extend Overrides

        /// <inheritdoc />
        protected override void OnRun(Response response)
        {
            IChannel         channel   = Channel;
            RemoteNamedCache cache     = (RemoteNamedCache) channel.Receiver;
            Listeners        listeners = cache.DeactivationListeners;

            if (!listeners.IsEmpty)
            {
                CacheEventArgs evtDeleted = new CacheEventArgs(cache, CacheEventType.Deleted,
                    null, null, null, false);
                RunnableCacheEvent.DispatchSafe(evtDeleted, listeners, null /*Queue*/);
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 56;

        #endregion
    }
}