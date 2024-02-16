/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;

using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    /// <summary>
    /// Internal <see cref="IRequest"/> used to create an
    /// <see cref="IChannel"/>.
    /// </summary>
    public class CreateChannel : Request
    {
        #region Properties

        /// <summary>
        /// The <see cref="Connection"/> used to create the
        /// <see cref="IChannel"/>.
        /// </summary>
        /// <value>
        /// The <b>Connection</b> used to create the <b>IChannel</b>.
        /// </value>
        public virtual Connection Connection
        {
            get { return m_connection; }
            set { m_connection = value; }
        }

        /// <summary>
        /// The optional <see cref="IReceiver"/> that the
        /// <b>IChannel</b> will register with.
        /// </summary>
        /// <value>
        /// The optional <b>IReceiver</b> that the <b>IChannel</b>
        /// will register with.
        /// </value>
        public virtual IReceiver Receiver
        {
            get { return m_receiver; }
            set { m_receiver = value; }
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
            Peer     peer     = (Peer) channel0.Receiver;

            response.Result = connection.CreateChannelInternal(Protocol, peer.EnsureSerializer(), Receiver);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = -4;

        /// <summary>
        /// The Connection used to create the IChannel.
        /// </summary>
        [NonSerialized]
        private Connection m_connection;

        /// <summary>
        /// The optional IReceiver that the IChannel will register
        /// with.
        /// </summary>
        [NonSerialized]
        private IReceiver m_receiver;

        /// <summary>
        /// The IProtocol used by the new IChannel.
        /// </summary>
        [NonSerialized]
        private IProtocol m_protocol;

        #endregion
    }
}