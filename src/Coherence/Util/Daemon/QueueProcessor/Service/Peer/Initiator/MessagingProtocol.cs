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
    /// <see cref="Util.Daemon.QueueProcessor.Service.Peer.MessagingProtocol"/>
    /// specific for <see cref="Initiator"/>.
    /// </summary>
    public class MessagingProtocol : Util.Daemon.QueueProcessor.Service.Peer.MessagingProtocol
    {
        #region Static initializer

        /// <summary>
        /// Static initializer.
        /// </summary>
        static MessagingProtocol()
        {
            m_instance = new MessagingProtocol();
        }

        #endregion

        /// <summary>
        /// The singleton Initiator MessagingProtocol instance.
        /// </summary>
        /// <value>
        /// The singleton Initiator MessagingProtocol instance.
        /// </value>
        public new static MessagingProtocol Instance
        {
            get { return m_instance; }
            set { m_instance = value; }
        }

        /// <summary>
        /// Instantiate a new <b>MessageFactory</b> for the given version of
        /// this Protocol.
        /// </summary>
        /// <param name="version">
        /// The version of the Protocol that the returned
        /// <b>MessageFactory</b> will use.
        /// </param>
        /// <returns>
        /// A new <b>MessageFactory</b> for the given version of this
        /// Protocol.
        /// </returns>
        protected override MessageFactory InstantiateMessageFactory(int version)
        {
            return new MessagingFactory();
        }

        /// <summary>
        /// The singleton Protocol instance.
        /// </summary>
        private static MessagingProtocol m_instance;
    }
}
