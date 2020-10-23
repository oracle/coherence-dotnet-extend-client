/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.IO;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    /// <summary>
    /// A <see cref="IMessage"/> with a <see cref="DataReader"/> that
    /// contains an encoded <b>IMessage</b>.
    /// </summary>
    /// <remarks>
    /// The service thread will decode the message using the configured
    /// <see cref="ICodec"/> before dispatching it for execution.
    /// </remarks>
    public class EncodedMessage : Message
    {
        #region Properties

        ///<summary>
        /// The <see cref="Connection"/> that received the encoded
        /// <see cref="IMessage"/>.
        /// </summary>
        /// <value>
        /// The <b>Connection</b> that received the encoded <b>IMessage</b>.
        /// </value>
        public virtual Connection Connection
        {
            get { return m_connection; }
            set { m_connection = value; }
        }

        /// <summary>
        /// The <see cref="DataReader"/> with encoded <b>IMessage</b>.
        /// </summary>
        /// <value>
        /// The <b>DataReader</b> with encoded <b>IMessage</b>.
        /// </value>
        public virtual DataReader Reader
        {
            get { return m_reader; }
            set { m_reader = value; }
        }

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
        public override void Run()
        {}

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = -10;

        ///<summary>
        /// The Connection that received the encoded Message.
        /// </summary>
        private Connection m_connection;

        /// <summary>
        /// The DataReader with encoded IMessage.
        /// </summary>
        private DataReader m_reader;

        #endregion
    }
}