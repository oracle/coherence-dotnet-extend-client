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
    /// The <see cref="IProtocol"/> used by the
    /// <see cref="IConnectionManager"/> to manage the lifecycle of
    /// <see cref="IConnection"/> and <see cref="IChannel"/> objects.
    /// </summary>
    /// <remarks>
    /// The name of this Protocol is "MessagingProtocol".
    /// </remarks>
    public class MessagingProtocol : Protocol
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

        #region Properties

        /// <summary>
        /// The singleton Peer MessagingProtocol instance.
        /// </summary>
        /// <value>
        /// The singleton Peer MessagingProtocol instance.
        /// </value>
        public static MessagingProtocol Instance
        {
            get { return m_instance; }
            set { m_instance = value; }
        }

        /// <summary>
        /// Gets the unique name of this Protocol.
        /// </summary>
        /// <value>
        /// The Protocol name.
        /// </value>
        public override string Name
        {
            get { return PROTOCOL_NAME; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected MessagingProtocol()
        {
            CurrentVersion   = 3;
            SupportedVersion = 2;
        }

        #endregion

        #region MessageFactory related methods

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

        #endregion

        #region Data members

        /// <summary>
        /// The name of this Protocol.
        /// </summary>
        public const string PROTOCOL_NAME = "MessagingProtocol";

        /// <summary>
        /// The singleton Protocol instance.
        /// </summary>
        private static MessagingProtocol m_instance;

        #endregion
    }
}