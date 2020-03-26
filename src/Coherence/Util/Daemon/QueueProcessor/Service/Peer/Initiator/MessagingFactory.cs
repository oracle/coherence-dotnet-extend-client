/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator
{
    /// <summary>
    /// Implementation of
    /// <see cref="Util.Daemon.QueueProcessor.Service.Peer.MessagingFactory"/>
    /// specific for <see cref="Initiator"/>.
    /// </summary>
    public class MessagingFactory : Util.Daemon.QueueProcessor.Service.Peer.MessagingFactory
    {
        #region Constructors

        /// <summary>
        /// Initialize an array of <see cref="Message"/> subclasses that
        /// can be created by this factory.
        /// </summary>
        public MessagingFactory()
        {
            // override messages with the ones specific to Initiator
            SetMessageType(OpenConnection.TYPE_ID, typeof (OpenConnection));
            SetMessageType(OpenConnectionRequest.TYPE_ID, typeof(OpenConnectionRequest));
            SetMessageType(OpenConnectionResponse.TYPE_ID, typeof(OpenConnectionResponse));
        }

        #endregion
    }
}
