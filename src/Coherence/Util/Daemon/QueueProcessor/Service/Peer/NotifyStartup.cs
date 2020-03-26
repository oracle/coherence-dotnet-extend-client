/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    /// <summary>
    /// This internal <see cref="IMessage"/> is sent to a
    /// <see cref="IConnectionManager"/> when it first has been started.
    /// </summary>
    public class NotifyStartup : Message
    {
        #region Properties

        /// <summary>
        /// Return the identifier for <b>Message</b> object's class.
        /// </summary>
        /// <value>
        /// An identifier that uniquely identifies <b>Message</b>
        /// object's class.
        /// </value>
        public override int TypeId
        {
            get { return TYPE_ID; }
        }

        #endregion

        #region Message overrides

        /// <summary>
        /// Execute the action specific to the Message implementation.
        /// </summary>
        public override void  Run()
        {
            Peer peer = (Peer) Channel.Receiver;
            peer.ServiceState = ServiceState.Started;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = -6;

        #endregion
    }
}