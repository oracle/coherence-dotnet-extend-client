/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator
{
    /// <summary>
    /// Implementation of
    /// <see cref="Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator.MessagingFactory"/>
    /// specific for <see cref="TcpInitiator"/>.
    /// </summary>
    class TcpMessagingFactory : MessagingFactory
    {
        #region Constructors

        /// <summary>
        /// Initialize an array of <see cref="Message"/> subclasses that
        /// can be created by this factory.
        /// </summary>
        public TcpMessagingFactory()
        {
            // override messages with the ones specific to TcpInitiator
            SetMessageType(OpenConnectionRequest.TYPE_ID, typeof(TcpOpenConnectionRequest));
            SetMessageType(OpenConnectionResponse.TYPE_ID, typeof(TcpOpenConnectionResponse));
        }

        #endregion
    }
}
