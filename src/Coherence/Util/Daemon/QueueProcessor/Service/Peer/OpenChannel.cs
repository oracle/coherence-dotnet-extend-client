/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.Security.Principal;

using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    /// <summary>
    /// Internal <see cref="IRequest"/> used to open an
    /// <see cref="IChannel"/>.
    /// </summary>
    public class OpenChannel : Request
    {
        #region Properties

        /// <summary>
        /// The <see cref="Connection"/> used to opet the <b>IChannel</b>.
        /// </summary>
        /// <value>
        /// The <b>Connection</b> used to opet the <b>IChannel</b>.
        /// </value>
        public virtual Connection Connection
        {
            get { return m_connection; }
            set { m_connection = value; }
        }

        /// <summary>
        /// The <see cref="IProtocol"/> used by the new <b>IChannel</b>.
        /// </summary>
        /// <value>
        /// The <b>IProtocol</b> used by the new <b>IChannel</b>.
        /// </value>
        public virtual IProtocol Protocol
        {
            get { return m_protocol; }
            set { m_protocol = value; }
        }

        /// <summary>
        /// The optional <see cref="IReceiver"/> that the <b>IChannel</b>
        /// will register with.
        /// </summary>
        /// <value>
        /// The optional <b>IReceiver</b> that the <b>IChannel</b> will
        /// register with.
        /// </value>
        public virtual IReceiver Receiver
        {
            get { return m_receiver; }
            set { m_receiver = value; }
        }

        /// <summary>
        /// The name of the remote <see cref="IReceiver"/>.
        /// </summary>
        /// <value>
        /// The name of the remote <b>IReceiver</b>.
        /// </value>
        public virtual string ReceiverName
        {
            get { return m_receiverName; }
            set { m_receiverName = value; }
        }

        /// <summary>
        /// The identity under which messages received by the new
        /// <b>IChannel</b> will be executed.
        /// </summary>
        /// <value>
        /// The identity under which messages received by the new
        /// <b>IChannel</b> will be executed.
        /// </value>
        public virtual IPrincipal Principal
        {
            get { return m_principal; }
            set { m_principal = value; }
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

        /// <summary>
        /// The token representing a user's identity.
        /// </summary>
        /// <value>
        /// The token representing a user's identity.
        /// </value>
        public virtual byte[] IdentityToken
        {
            get { return m_identityToken; }
            set { m_identityToken = value; }
        }

        #endregion

        #region Request overrides

        /// <summary>
        /// Create a new <see cref="Response"/> for this IRequest.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="IMessageFactory"/> that must be used to create the
        /// returned <b>Response</b>; never <c>null</c>.
        /// </param>
        /// <returns>
        /// A new <b>Response</b>.
        /// </returns>
        protected override Response InstantiateResponse(IMessageFactory factory)
        {
            return (Response) factory.CreateMessage(InternalResponse.TYPE_ID);
        }

        /// <summary>
        /// Process this IRequest and update the given <b>Response</b> with
        /// the result.
        /// </summary>
        /// <remarks>
        /// Implementations of this method are free to throw an exception
        /// while processing the IRequest. An exception will result in the
        /// <b>Response</b> being marked as a failure that the
        /// <b>Response</b> result will be the exception itself.
        /// </remarks>
        /// <param name="response">
        /// The <b>Response</b> that will be sent back to the requestor.
        /// </param>
        /// <exception cref="Exception">
        /// If exception occurs during execution.
        /// </exception>
        protected override void OnRun(Response response)
        {
            Connection connection = Connection;
            Debug.Assert(connection != null);

            IChannel channel0 = Channel;
            Peer     peer     = (Peer) Channel.Receiver;

            response.Result =
                connection.OpenChannelInternal(Protocol, ReceiverName, peer.EnsureSerializer(), Receiver, Principal, 
                IdentityToken);
        }
        
        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = -7;

        /// <summary>
        /// The Connection used to opet the IChannel.
        /// </summary>
        [NonSerialized]
        private Connection m_connection;

        /// <summary>
        /// The IProtocol used by the new IChannel.
        /// </summary>
        [NonSerialized]
        private IProtocol m_protocol;

        /// <summary>
        /// The optional IReceiver that the IChannel will register
        /// with.
        /// </summary>
        [NonSerialized]
        private IReceiver m_receiver;

        /// <summary>
        /// The name of the remote IReceiver.
        /// </summary>
        private string m_receiverName;

        /// <summary>
        /// The identity under which IMessages received by the new IChannel
        /// will be executed.
        /// </summary>
        [NonSerialized]
        private IPrincipal m_principal;

        /// <summary>
        /// A token representing a user's identity.
        /// </summary>
        private byte[] m_identityToken;
        
        #endregion
    }
}