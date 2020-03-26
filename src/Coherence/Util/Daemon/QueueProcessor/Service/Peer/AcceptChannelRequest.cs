/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    /// <summary>
    /// This <see cref="IRequest"/> is used to accept a
    /// <see cref="IChannel"/> that was spawned by a peer.
    /// </summary>
    public class AcceptChannelRequest : Request
    {
        #region Properties

        /// <summary>
        /// The unique identifier of the <see cref="IChannel"/> to
        /// accept.
        /// </summary>
        /// <remarks>
        /// This channel identifier must have been previously
        /// returned in a <see cref="IResponse"/> sent by the peer.
        /// </remarks>
        /// <value>
        /// The unique identifier of the <b>IChannel</b> to accept.
        /// </value>
        public virtual int ChannelId
        {
            get { return m_channelId; }
            set { m_channelId = value; }
        }

        /// <summary>
        /// An optional token representing a user identity to
        /// associate with the <b>Channel</b>.
        /// </summary>
        /// <remarks>
        /// Operations performed on receipt of messages sent via the
        /// newly established <b>IChannel</b> will be performed on
        /// behalf of this identity.
        /// </remarks>
        /// <value>
        /// Token representing a user identity.
        /// </value>
        public virtual byte[] IdentityToken
        {
            get { return m_identityToken; }
            set { m_identityToken = value; }
        }

        /// <summary>
        /// The <see cref="IMessageFactory"/> used by the new
        /// <see cref="IChannel"/>.
        /// </summary>
        /// <value>
        /// The <b>IMessageFactory</b> used by the new
        /// <b>IChannel</b>.
        /// </value>
        public virtual IMessageFactory MessageFactory
        {
            get { return m_messageFactory; }
            set { m_messageFactory = value; }
        }

        /// <summary>
        /// The name of the <see cref="IProtocol"/> used by the new
        /// <see cref="IChannel"/>.
        /// </summary>
        /// <remarks>
        /// This protocol name must have been previously returned in
        /// a <see cref="IResponse"/> sent by the peer.
        /// </remarks>
        /// <value>
        /// The name of the <b>IProtocol"</b> used by the new
        /// <b>IChannel</b>.
        /// </value>
        public virtual string ProtocolName
        {
            get { return m_protocolName; }
            set { m_protocolName = value; }
        }

        /// <summary>
        /// The optional <see cref="IReceiver"/> that the
        /// <see cref="IChannel"/> will register with.
        /// </summary>
        /// <value>
        /// The optional <b>IReceiver</b> that the <b>IChannel"</b>
        /// will register with.
        /// </value>
        public virtual IReceiver Receiver
        {
            get { return m_receiver; }
            set { m_receiver = value; }
        }

        /// <summary>
        /// The <see cref="ISerializer"/> used by the new
        /// <see cref="IChannel"/>.
        /// </summary>
        /// <value>
        /// The <b>ISerializer</b> used by the new <b>IChannel</b>.
        /// </value>
        public virtual ISerializer Serializer
        {
            get { return m_serializer; }
            set { m_serializer = value; }
        }

        /// <summary>
        /// The identity under which messages received by the new
        /// <see cref="IChannel"/> will be executed.
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

        #endregion

        #region Request overrides

        /// <summary>
        /// Create a new <see cref="Response"/> for this IRequest.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="IMessageFactory"/> that must be used to create the
        /// returned <b>IResponse</b>; never <c>null</c>.
        /// </param>
        /// <returns>
        /// A new <b>Response</b>.
        /// </returns>
        protected override Response InstantiateResponse(IMessageFactory factory)
        {
            return (Response) factory.CreateMessage(AcceptChannelResponse.TYPE_ID);
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
            IChannel channel0 = Channel;
            Debug.Assert(channel0.Id == 0);

            Connection connection = (Connection) channel0.Connection;
            Debug.Assert(connection != null);

            Peer module = (Peer) Channel.Receiver;
            int  id     = ChannelId;

            connection.AcceptChannelRequest
                (
                id,
                module.AssertIdentityToken(module.DeserializeIdentityToken(IdentityToken))
                );

            // Cannot use ImplVersion here as channel0 is established prior to protocol
            // version negotiation.  Instead check the negotiated version directly on the connection's
            // MessageFactoryMap.
            MessageFactory clientFactory = (MessageFactory) connection.MessageFactoryMap
                [channel0.MessageFactory.Protocol.Name];
            if (clientFactory != null && clientFactory.Version >= 3)
            {
                response.Result = id;
            }
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            ChannelId     = reader.ReadInt32(1);
            IdentityToken = reader.ReadByteArray(2);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteInt32(1, ChannelId);
            writer.WriteByteArray(2, IdentityToken);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = 13;

        /// <summary>
        /// The unique identifier of the IChannel to accept.
        /// </summary>
        private int m_channelId;

        /// <summary>
        /// An optional token representing a user identity to associate
        /// with the IChannel.
        /// </summary>
        private byte[] m_identityToken;

        /// <summary>
        /// The IMessageFactory used by the new IChannel.
        /// </summary>
        [NonSerialized]
        private IMessageFactory m_messageFactory;

        /// <summary>
        /// The name of the IProtocol used by the new IChannel.
        /// </summary>
        [NonSerialized]
        private string m_protocolName;

        /// <summary>
        /// The optional IReceiver that the IChannel will register
        /// with.
        /// </summary>
        [NonSerialized]
        private IReceiver m_receiver;

        /// <summary>
        /// The ISerializer used by the new IChannel.
        /// </summary>
        [NonSerialized]
        private ISerializer m_serializer;

        /// <summary>
        /// The identity under which IMessages received by the new
        /// IChannel will be executed.
        /// </summary>
        [NonSerialized]
        private IPrincipal m_principal;

        #endregion
    }
}