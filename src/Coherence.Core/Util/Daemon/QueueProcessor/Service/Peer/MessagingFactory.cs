/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    /// <summary>
    /// <see cref="IMessageFactory"/> implementation for version 2 of the
    /// MessagingProtocol.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This <b>IMessageFactory</b> contains <see cref="IMessage"/>
    /// classes necessary to manage the lifecycle of
    /// <see cref="IConnection"/>s and <see cref="IChannel"/>s.</p>
    /// <p>
    /// The type identifiers of the <b>IMessage</b> classes instantiated
    /// by this <b>IMessageFactory</b> are organized as follows:</p>
    /// <p>
    /// Internal (&lt;0):<br/>
    /// (-1)  <see cref="AcceptChannel"/><br/>
    /// (-2)  <see cref="CloseChannel"/><br/>
    /// (-3)  <see cref="CloseConnection"/><br/>
    /// (-4)  <see cref="CreateChannel"/><br/>
    /// (-5)  <see cref="NotifyShutdown"/><br/>
    /// (-6)  <see cref="NotifyStartup"/><br/>
    /// (-7)  <see cref="OpenChannel"/><br/>
    /// (-8)  <see cref="OpenConnection"/><br/>
    /// (-9)  <see cref="InternalResponse"/><br/>
    /// (-10) <see cref="EncodedMessage"/></p>
    /// <p>
    /// Connection Lifecycle (0 - 10):<br/>
    /// (0)  <see cref="OpenConnectionResponse"/> (*)<br/>
    /// (1)  <see cref="OpenConnectionRequest"/><br/>
    /// (3)  <see cref="PingRequest"/><br/>
    /// (4)  <see cref="PingResponse"/><br/>
    /// (10) <see cref="NotifyConnectionClosed"/></p>
    /// <p>
    /// The <b>OpenConnectionResponse</b> has type identifier 0 for
    /// historical reasons. Prior to version 2 of the Messaging Protocol,
    /// all Request messages used a common <see cref="IResponse"/> type
    /// with type identifier 0. Since the first <b>IResponse</b> that a
    /// client expects to receive is an <b>OpenConnectionResponse</b>,
    /// this allows version 2 and newer servers to reject connection
    /// attempts from version 1 clients.</p>
    /// <p>
    /// Channel Lifecycle (11-20):<br/>
    /// (11) <see cref="OpenChannelRequest"/><br/>
    /// (12) <see cref="OpenChannelResponse"/><br/>
    /// (13) <see cref="AcceptChannelRequest"/><br/>
    /// (14) <see cref="AcceptChannelResponse"/><br/>
    /// (20) <see cref="NotifyChannelClosed"/></p>p>
    /// </remarks>
    public class MessagingFactory : MessageFactory
    {
        #region Constructors

        /// <summary>
        /// Initialize an array of <see cref="Message"/> subclasses that
        /// can be created by this factory.
        /// </summary>
        public MessagingFactory()
        {
            InitializeMessageTypes(messagingTypes);
        }

        #endregion

        #region MessageFactory overrides

        /// <summary>
        /// Type of the class that is subclass of the <b>Message</b> with
        /// specified type identifier.
        /// </summary>
        /// <param name="typeId">
        /// The type identifier of class that is subclass of the
        /// <b>Message</b>.
        /// </param>
        protected override Type GetMessageType(int typeId)
        {
            return base.GetMessageType(typeId + MESSAGE_OFFSET);
        }

        /// <summary>
        /// Adds class that is subclass of the <b>Message</b> to the
        /// array of subclasses that can be created by this
        /// MessageFactory.
        /// </summary>
        /// <param name="typeId">
        /// Array index at which class should be inserted. It is also
        /// type identifier.
        /// </param>
        /// <param name="cls">
        /// Class to be inserted into array of subclasses.
        /// </param>
        protected override void SetMessageType(int typeId, Type cls)
        {
            base.SetMessageType(typeId + MESSAGE_OFFSET, cls);
        }

        #endregion

        #region Data members

        /// <summary>
        /// Offset to allow for the internal (negative) <b>Message</b>
        /// types.
        /// </summary>
        public const int MESSAGE_OFFSET = 32;

        /// <summary>
        /// An array of <b>Message</b> subclasses that can be created by
        /// this factory.
        /// </summary>
        private Type[] messagingTypes = { typeof(AcceptChannel),
                                          typeof(CloseChannel),
                                          typeof(CloseConnection),
                                          typeof(CreateChannel),
                                          typeof(NotifyShutdown),
                                          typeof(NotifyStartup),
                                          typeof(OpenChannel),
                                          typeof(OpenConnection),
                                          typeof(InternalResponse),
                                          typeof(EncodedMessage),
                                          typeof(OpenConnectionResponse),
                                          typeof(OpenConnectionRequest),
                                          typeof(PingRequest),
                                          typeof(PingResponse),
                                          typeof(NotifyConnectionClosed),
                                          typeof(OpenChannelRequest),
                                          typeof(OpenChannelResponse),
                                          typeof(AcceptChannelRequest),
                                          typeof(AcceptChannelResponse),
                                          typeof(NotifyChannelClosed) };

        #endregion
    }
}