/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;

using Tangosol.IO;
using Tangosol.Net;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;
using Tangosol.Run.Xml;
using Tangosol.Util.Collections;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    /// <summary>
    /// Base implementation of a <see cref="IConnectionManager"/>.
    /// </summary>
    /// <remarks>
    /// A <b>IConnectionManager</b> has a service thread, an optional execute
    /// thread pool, and a <see cref="ConnectionEventArgs"/> dispatcher
    /// thread.<br/>
    /// Concrete implementations must implement the abstract
    /// <see cref="Send"/> method using the underlying transport.
    /// Additionally, the underlying transport must call the
    /// <see cref="Receive"/> or <see cref="Post"/> method when a
    /// <see cref="IMessage"/> is received over the underlying transport.
    /// </remarks>
    /// <author>Ana Cikic  2007.12.25</author>
    public abstract class Peer : Service, IReceiver, IConnectionManager
    {
        #region Properties

        /// <summary>
        /// <see cref="Channel"/> used for all internal communication.
        /// </summary>
        /// <value>
        /// <b>Channel</b> used for all internal communication.
        /// </value>
        public virtual Channel InternalChannel
        {
            get { return m_channel; }
            //TODO: protected
            set { m_channel = value; }
        }

        /// <summary>
        /// <see cref="Connection"/> used for all internal communication.
        /// </summary>
        /// <value>
        /// <b>Connection</b> used for all internal communication.
        /// </value>
        public virtual Connection InternalConnection
        {
            get { return m_connection; }
            //TODO: protected
            set { m_connection = value; }
        }

        /// <summary>
        /// The <see cref="IMessageFactory"/> used to create
        /// <see cref="IMessage"/>s processed by this <see cref="IService"/>.
        /// </summary>
        /// <value>
        /// The MessageFactory used to create Messages processed by this Service.
        /// </value>
        protected virtual IMessageFactory MessageFactory
        {
            get
            {
                IProtocol protocol = Protocol;
                return protocol.GetMessageFactory(protocol.CurrentVersion);
            }
        }

        /// <summary>
        /// The number of milliseconds between successive <b>IConnection</b>
        /// "pings" or 0 if heartbeats are disabled.
        /// </summary>
        /// <value>
        /// The number of milliseconds between successive <b>IConnection</b>
        /// "pings" or 0 if heartbeats are disabled.
        /// </value>
        public virtual long PingInterval
        {
            get { return m_pingInterval; }
            //TODO: protected
            set { m_pingInterval = value; }
        }

        /// <summary>
        /// The last time the <see cref="IConnection"/>(s) managed by this
        /// <b>IConnectionManager</b> were checked for a "ping" timeout.
        /// </summary>
        /// <value>
        /// The last time the <b>IConnection</b>(s) managed by this
        /// <b>IConnectionManager</b> were checked for a "ping" timeout.
        /// </value>
        public virtual long PingLastCheckMillis
        {
            get { return m_pingLastCheckMillis; }
            //TODO: protected
            set { m_pingLastCheckMillis = value; }
        }

        /// <summary>
        /// The last time the <see cref="IConnection"/>(s) managed by this
        /// <b>IConnectionManager</b> were "pinged".
        /// </summary>
        /// <value>
        /// The last time the <b>IConnection</b>(s) managed by this
        /// <b>IConnectionManager</b> were "pinged".
        /// </value>
        public virtual long PingLastMillis
        {
            get { return m_pingLastMillis; }
            //TODO: protected
            set { m_pingLastMillis = value; }
        }

        /// <summary>
        /// The next time the <see cref="IConnection"/>(s) managed by this
        /// <b>IConnectionManager</b> should be checked for a "ping" timeout.
        /// </summary>
        public virtual long PingNextCheckMillis
        {
            get
            {
                long lastMillis = PingLastMillis;
                long millis     = PingTimeout;

                return millis == 0L || lastMillis == 0L || PingLastCheckMillis > 0L
                           ? Int64.MaxValue : lastMillis + millis;
            }
        }

        /// <summary>
        /// The next time the <see cref="IConnection"/>(s) managed by this
        /// <b>IConnectionManager</b> should be "pinged".
        /// </summary>
        /// <value>
        /// The next time the <b>IConnection</b>(s) managed by this
        /// <b>IConnectionManager</b> should be "pinged".
        /// </value>
        public virtual long PingNextMillis
        {
            get
            {
                long lastMillis = PingLastMillis;
                long millis     = PingInterval;

                return millis     == 0L ? Int64.MaxValue :
                       lastMillis == 0L ? DateTimeUtils.GetSafeTimeMillis() : lastMillis + millis;
            }
        }

        /// <summary>
        /// The default request timeout for a <see cref="PingRequest"/>.
        /// </summary>
        /// <remarks>
        /// A timeout of 0 is interpreted as an infinite timeout. This
        /// property defaults to the value of the
        /// <see cref="RequestTimeout"/> property.
        /// </remarks>
        public virtual long PingTimeout
        {
            get { return m_pingTimeout; }
            //TODO: protected
            set { m_pingTimeout = value; }
        }

        /// <summary>
        /// The unique identifier (UUID) of the process using this
        /// <b>IConnectionManager</b>.
        /// </summary>
        /// <value>
        /// The unique identifier (UUID) of the process using this
        /// <b>IConnectionManager</b>.
        /// </value>
        public static UUID ProcessId
        {
            get { return m_processId; }
            //TODO: protected
            set
            {
                Debug.Assert(value != null);
                m_processId = value;
            }
        }

        /// <summary>
        /// The map of registered <see cref="IProtocol"/> objects.
        /// </summary>
        /// <value>
        /// The <b>IDictionary</b> of registered <b>IProtocol</b> objects.
        /// </value>
        public virtual IDictionary ProtocolMap
        {
            get { return m_protocolMap; }
            //TODO: protected
            set { m_protocolMap = value; }
        }

        /// <summary>
        /// A <b>IDictionary</b> map of version ranges for registered Protocols.
        /// It can be used to create an <b>IDictionary</b> of <b>IMessageFactory</b>
        /// objects that may be used by <see cref="IConnection"/> object created by
        /// this <b>IConnectionManager</b>, keyed by <see cref="IProtocol"/>
        /// name.
        /// </summary>
        /// <value>
        /// An <b>IDictionary</b> of version ranges for registered Protocols.
        /// The keys are the names of the Protocols and the values are two 
        /// element Int32 arrays, the first element being the current version
        /// and the second being the supported version of the corresponding
        /// Protocol.
        /// </value>
        public virtual IDictionary ProtocolVersionMap
        {
            get { return m_protocolVersionMap; }
            //TODO: protected
            set { m_protocolVersionMap = value; }
        }

        /// <summary>
        /// The map of registered <see cref="IReceiver"/> objects.
        /// </summary>
        /// <value>
        /// The <b>IDictionary</b> of registered <b>IReceiver</b> objects.
        /// </value>
        public virtual IDictionary ReceiverMap
        {
            get { return m_receiverMap; }
            //TODO: protected
            set { m_receiverMap = value; }
        }

        /// <summary>
        /// The default request timeout for all <see cref="IChannel"/>
        /// objects created by <see cref="IConnection"/> objects managed by
        /// this <b>IConnectionManager</b>.
        /// </summary>
        /// <remarks>
        /// A timeout of 0 is interpreted as an infinite timeout.
        /// </remarks>
        public virtual long RequestTimeout
        {
            get { return m_requestTimeout; }
            //TODO: protected
            set { m_requestTimeout = value; }
        }

        /// <summary>
        /// The Maximum incoming message size.
        /// </summary>
        /// <remarks>
        /// A value of 0 is interpreted as unlimited size.
        /// </remarks>
        public virtual long MaxIncomingMessageSize
        {
            get { return m_maxIncomingMessageSize; }
            //TODO: protected
            set { m_maxIncomingMessageSize = value; }
        }

        /// <summary>
        /// The Maximum outgoing message size.
        /// </summary>
        /// <remarks>
        /// A value of 0 is interpreted as unlimited size.
        /// </remarks>
        public virtual long MaxOutgoingMessageSize
        {
            get { return m_maxOutgoingMessageSize; }
            //TODO: protected
            set { m_maxOutgoingMessageSize = value; }
        }

        /// <summary>
        /// Statistics: total number of bytes received.
        /// </summary>
        /// <value>
        /// Total number of bytes received.
        /// </value>
        public virtual long StatsBytesReceived
        {
            get { return m_statsBytesReceived; }
            //TODO: protected
            set { m_statsBytesReceived = value; }
        }

        /// <summary>
        /// Statistics: total number of bytes sent.
        /// </summary>
        /// <value>
        /// Total number of bytes sent.
        /// </value>
        public virtual long StatsBytesSent
        {
            get { return m_statsBytesSent; }
            //TODO: protected
            set { m_statsBytesSent = value; }
        }

        /// <summary>
        /// Statistics: total number of messages sent.
        /// </summary>
        /// <value>
        /// Total number of messages sent.
        /// </value>
        public virtual long StatsSent
        {
            get { return m_statsSent; }
            //TODO: protected
            set { m_statsSent = value; }
        }

        /// <summary>
        /// The total number of timed-out requests since the last time the
        /// statistics were reset.
        /// </summary>
        /// <value>
        /// The total number of timed-out requests since the last time the
        /// statistics were reset.
        /// </value>
        public virtual long StatsTimeoutCount
        {
            get { return m_statsTimeoutCount; }
            //TODO: protected
            set { m_statsTimeoutCount = value; }
        }

        /// <summary>
        /// A list of <see cref="IWrapperStreamFactory"/> objects that affect
        /// how <see cref="IMessage"/>s are written and read.
        /// </summary>
        /// <value>
        /// A list of <b>IWrapperStreamFactory</b> objects that affect
        /// how <b>IMessage</b>s are written and read.
        /// </value>
        public virtual IList WrapperStreamFactoryList
        {
            get { return m_wrapperStreamFactoryList; }
            //TODO: protected
            set { m_wrapperStreamFactoryList = value; }
        }

        /// <summary>
        /// The parent IService.
        /// </summary>
        /// <value>
        /// The parent IService.
        /// </value>
        public virtual Net.IService ParentService { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes <see cref="ProcessId"/>.
        /// </summary>
        static Peer()
        {
            ProcessId = new UUID();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Accept a new <see cref="IChannel"/>.
        /// </summary>
        /// <remarks>
        /// This method is called by <see cref="IConnection.AcceptChannel"/>
        /// and is always run on client threads.
        /// </remarks>
        public virtual IChannel AcceptChannel(Connection connection, Uri uri, IReceiver receiver, IPrincipal principal)
        {
            Debug.Assert(connection != null);

            IChannel        channel0 = InternalChannel;
            IMessageFactory factory0 = channel0.MessageFactory;

            AcceptChannel request = (AcceptChannel)
                factory0.CreateMessage(Tangosol.Util.Daemon.QueueProcessor.Service.Peer.AcceptChannel.TYPE_ID);

            request.ChannelUri    = uri;
            request.Connection    = connection;
            request.IdentityToken = SerializeIdentityToken(GenerateIdentityToken(principal));
            request.Principal     = principal;
            request.Receiver      = receiver;

            IStatus status = (IStatus) channel0.Request(request);

            AcceptChannelResponse response = (AcceptChannelResponse) status.WaitForResponse(RequestTimeout);

            return (IChannel) response.Result;
        }

        /// <summary>
        /// Validate a token in order to establish a user's identity.
        /// </summary>
        /// <param name="token">
        /// An identity assertion, a statement that asserts an identity.
        /// </param>
        /// <returns>
        /// An IPrincipal reprsenting the identity.
        /// </returns>
        public virtual IPrincipal AssertIdentityToken(object token)
        {
            return OperationalContext.IdentityAsserter.AssertIdentity(token,
                ParentService);
        }

        /// <summary>
        /// Transform an IPrincipal to a token that asserts identity.
        /// </summary>
        /// <param name="principal">
        /// The IPrincipal to transform.
        /// </param>
        /// <returns>
        /// A token that asserts identity.
        /// </returns>
        public virtual object GenerateIdentityToken(IPrincipal principal)
        {
            return OperationalContext.IdentityTransformer.
                TransformIdentity(principal, ParentService);
        }

        /// <summary>
        /// Deserialize the identity token object.
        /// </summary>
        /// <param name="token">
        /// The identity token.
        /// </param>
        /// <returns>
        /// The token.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">
        /// On deserialization error.
        /// </exception>
        public virtual object DeserializeIdentityToken(byte[] token)
        {
            if (token != null)
            {
                try
                {
                    var dataReader = new DataReader(new MemoryStream(token));
                    return EnsureSerializer().Deserialize(dataReader);
                }
                catch (Exception ex)
                {
                    CacheFactory.Log("An exception occurred while deserializing an identity token",
                        ex, CacheFactory.LogLevel.Error);
                    throw new SecurityException("invalid identity token");
                }
            }

            return null;
        }

        /// <summary>
        /// Serialize an identity token.
        /// </summary>
        /// <param name="token">
        /// The identity token object to serialize.
        /// </param>
        /// <returns>
        /// The serialized token.
        /// </returns>
        public virtual byte[] SerializeIdentityToken(object token)
        {
            if (token != null)
            {
                try
                {
                    var stream = new MemoryStream(1024);
                    var writer = new DataWriter(stream);
                    EnsureSerializer().Serialize(writer, token);

                    writer.Flush();
                    return stream.ToArray();
                }
                catch (Exception ex)
                {
                    CacheFactory.Log("An exception occurred while serializing an identity token",
                        ex, CacheFactory.LogLevel.Error);
                    throw new SecurityException("unable to produce identity token");
                }
            }

            return null;
        }

        /// <summary>
        /// Check the given <see cref="Connection"/> for a ping timeout.
        /// </summary>
        /// <remarks>
        /// A <b>Connection</b> that has not received a
        /// <see cref="PingResponse"/> for an oustanding
        /// <see cref="PingRequest"/> within the configured
        /// <see cref="PingTimeout"/> will be closed.
        /// </remarks>
        /// <param name="connection">
        /// The <b>Connection</b> to check.
        /// </param>
        protected virtual void CheckPingTimeout(Connection connection)
        {
            long millis = PingTimeout;
            if (millis > 0L)
            {
                long lastPingMillis = connection.PingLastMillis;
                if (lastPingMillis > 0L)
                {
                    if (DateTimeUtils.GetSafeTimeMillis() >= lastPingMillis + millis)
                    {
                        connection.Close(false, new ConnectionException(
                                "did not receive a response to a ping within "
                                + millis + " millis", connection));
                    }
                }
            }
        }

        /// <summary>
        /// Check the <see cref="Connection"/>(s) managed by this
        /// <see cref="IConnectionManager"/> for a ping timeout.
        /// </summary>
        /// <remarks>
        /// A <b>Connection</b> that has not received a
        /// <see cref="PingResponse"/> for an oustanding
        /// <see cref="PingRequest"/> within the configured
        /// <see cref="PingTimeout"/> will be closed.
        /// </remarks>
        protected virtual void CheckPingTimeouts()
        {}

        /// <summary>
        /// Close the given <see cref="Channel"/>.
        /// </summary>
        /// <remarks>
        /// This method is called by <see cref="Channel.Close()"/> and is
        /// always run on client threads.
        /// </remarks>
        /// <param name="channel">
        /// The <b>Channel</b> to close.
        /// </param>
        /// <param name="notify">
        /// If <b>true</b>, notify the peer that the <b>Channel</b> is being
        /// closed.
        /// </param>
        /// <param name="e">
        /// The optional reason why the <b>Channel</b> is being closed.
        /// </param>
        public virtual void CloseChannel(Channel channel, bool notify, Exception e)
        {
            CloseChannel(channel, notify, e, /*wait*/ true);
        }

        /// <summary>
        /// Close the given <see cref="Channel"/>.
        /// </summary>
        /// <remarks>
        /// This method is always run on client threads.
        /// </remarks>
        /// <param name="channel">
        /// The <b>Channel</b> to close.
        /// </param>
        /// <param name="notify">
        /// If <b>true</b>, notify the peer that the <b>Channel</b> is being
        /// closed.
        /// </param>
        /// <param name="e">
        /// The optional reason why the <b>Channel</b> is being closed.
        /// </param>
        /// <param name="wait">
        /// If <b>true</b>, wait for the <b>Channel</b> to close before
        /// returning.
        /// </param>
        public virtual void CloseChannel(Channel channel, bool notify, Exception e, bool wait)
        {
            Debug.Assert(channel != null);

            IChannel        channel0 = InternalChannel;
            IMessageFactory factory0 = channel0.MessageFactory;

            CloseChannel request = (CloseChannel)
                factory0.CreateMessage(Tangosol.Util.Daemon.QueueProcessor.Service.Peer.CloseChannel.TYPE_ID);

            request.Cause        = e;
            request.ChannelClose = channel;
            request.Notify       = notify;

            if (wait)
            {
                channel0.Request(request);
            }
            else
            {
                channel0.Send(request);
            }
        }

        /// <summary>
        /// Close the given <see cref="Connection"/>.
        /// </summary>
        /// <remarks>
        /// This method is called by
        /// <see cref="Net.Messaging.Impl.Connection.Close()"/> and is always
        /// run on client threads.
        /// </remarks>
        /// <param name="connection">
        /// The <b>Connection</b> to close.
        /// </param>
        /// <param name="notify">
        /// If <b>true</b>, notify the peer that the <b>Connection</b> is
        /// being closed.
        /// </param>
        /// <param name="e">
        /// The optional reason why the <b>Connection</b> is being closed.
        /// </param>
        /// <param name="wait">
        /// If <b>true</b>, wait for the <b>Connection</b> to close before
        /// returning.
        /// </param>
        public virtual void CloseConnection(Connection connection, bool notify, Exception e, bool wait)
        {
            Debug.Assert(connection != null);

            IChannel        channel0 = InternalChannel;
            IMessageFactory factory0 = channel0.MessageFactory;

            CloseConnection request = (CloseConnection)
                factory0.CreateMessage(Tangosol.Util.Daemon.QueueProcessor.Service.Peer.CloseConnection.TYPE_ID);

            request.Cause           = e;
            request.ConnectionClose = connection;
            request.IsNotify        = notify;

            if (wait)
            {
                channel0.Request(request);
            }
            else
            {
                channel0.Send(request);
            }
        }

        /// <summary>
        /// Create a new <see cref="Channel"/>.
        /// </summary>
        /// <remarks>
        /// This method is called by <see cref="Connection.CreateChannel"/>
        /// and is always run on client threads.
        /// </remarks>
        public virtual Uri CreateChannel(Connection connection, IProtocol protocol, IReceiver receiver)
        {
            Debug.Assert(connection != null);

            IChannel        channel0 = InternalChannel;
            IMessageFactory factory0 = channel0.MessageFactory;

            CreateChannel request = (CreateChannel)
                factory0.CreateMessage(Tangosol.Util.Daemon.QueueProcessor.Service.Peer.CreateChannel.TYPE_ID);

            request.Connection = connection;
            request.Protocol   = protocol;
            request.Receiver   = receiver;

            return (Uri) channel0.Request(request);
        }

        /// <summary>
        /// Decode the <b>IMessage</b> from the given
        /// <see cref="DataReader"/> object with the configured
        /// <see cref="ICodec"/> and return a new decoded
        /// <see cref="IMessage"/>.
        /// </summary>
        /// <remarks>
        /// This method is called on either the service thread (see
        /// <see cref="EncodedMessage"/>) or a client (I/O) thread.
        /// </remarks>
        /// <param name="reader">
        /// The <b>DataReader</b> containing the binary-encoded
        /// <b>IMessage</b>.
        /// </param>
        /// <param name="connection">
        /// The <see cref="Connection"/> that received the <b>IMessage</b>.
        /// </param>
        /// <param name="filter">
        /// If <b>true</b>, the <b>DataReader</b> will be filtered using the
        /// list of configured <see cref="IWrapperStreamFactory"/> objects.
        /// </param>
        /// <returns>
        /// The decoded <b>IMessage</b> or <c>null</c> if the <b>IMessage</b>
        /// was sent via an unknown <b>IChannel</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// On I/O error decoding the <b>IMessage</b>.
        /// </exception>
        protected virtual IMessage DecodeMessage(DataReader reader, Connection connection, bool filter)
        {
            Debug.Assert(reader != null);
            Debug.Assert(connection != null);

            ICodec codec = Codec;
            Debug.Assert(codec != null);

            // filter the input, if necessary
            if (filter)
            {
                reader = FilterReader(reader);
            }

            // resolve the Channel
            Channel channel = (Channel) connection.GetChannel(reader.ReadPackedInt32());
            if (channel == null || !channel.IsOpen)
            {
                return null;
            }

            // attempt to decode the Message
            IMessage message = codec.Decode(channel, reader);
            message.Channel = channel;
            return message;
        }

        /// <summary>
        /// Dispatch a <see cref="ConnectionEventArgs"/> to the
        /// <see cref="Service.Dispatcher"/>.
        /// </summary>
        /// <param name="connection">
        /// The <see cref="Connection"/> associated with the Connection
        /// event.
        /// </param>
        /// <param name="eventType">
        /// Connection event type.
        /// </param>
        /// <param name="e">
        /// The optional <b>Exception</b> associated with the Connection
        /// event.
        /// </param>
        public virtual void DispatchConnectionEvent(Connection connection, ConnectionEventType eventType, Exception e)
        {
            ConnectionEventArgs evt      = new ConnectionEventArgs(connection, eventType, e);
            EventCallback       callback = new EventCallback(FireConnectionEvent);
            DispatchEvent(evt, callback);
        }

        /// <summary>
        /// Raises connection event.
        /// </summary>
        /// <param name="evt">
        /// <b>EventArgs</b> object.
        /// </param>
        public virtual void FireConnectionEvent(EventArgs evt)
        {
            if (evt is ConnectionEventArgs)
            {
                ConnectionEventArgs connectionEvt = (ConnectionEventArgs) evt;
                switch (connectionEvt.EventType)
                {
                    case ConnectionEventType.Opened:
                        InvokeConnectionEvent(m_connectionOpened, connectionEvt);
                        break;

                    case ConnectionEventType.Closed:
                        InvokeConnectionEvent(m_connectionClosed, connectionEvt);
                        break;

                    case ConnectionEventType.Error:
                        InvokeConnectionEvent(m_connectionError, connectionEvt);
                        break;
                }
            }
        }

        /// <summary>
        /// Invokes the event, with special remark towards multithreading
        /// (using local copy of delegate and no inline attribute for method).
        /// </summary>
        /// <param name="handler">
        /// The ConnectionEventHandler event that's being invoked.
        /// </param>
        /// <param name="evt">
        /// Event arguments.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InvokeConnectionEvent(ConnectionEventHandler handler, ConnectionEventArgs evt)
        {
            if (handler != null)
            {
                handler(this, evt);
            }
        }

        /// <summary>
        /// Encode the given <b>IMessage</b> into the the given
        /// <see cref="DataWriter"/> object with the configured
        /// <see cref="ICodec"/>.
        /// </summary>
        /// <param name="message">
        /// The <b>IMessage</b> to encode.
        /// </param>
        /// <param name="writer">
        /// The <b>DataWriter</b> that will be used to write out the encoded
        /// <b>IMessage</b>.
        /// </param>
        /// <param name="filter">
        /// If <b>true</b>, the <b>DataWriter</b> will be filtered using the
        /// list of configured <see cref="IWrapperStreamFactory"/> objects.
        /// </param>
        /// <exception cref="IOException">
        /// On encoding error.
        /// </exception>
        protected virtual void EncodeMessage(IMessage message, DataWriter writer, bool filter)
        {
            Debug.Assert(message != null);
            Debug.Assert(writer != null);

            IChannel channel = message.Channel;
            Debug.Assert(channel != null);

            ICodec codec = Codec;
            Debug.Assert(codec != null);

            // filter the output, if necessary
            if (filter)
            {
                writer = FilterWriter(writer);
            }

            // write the Channel ID
            writer.WritePackedInt32(channel.Id);

            // encode the Message
            codec.Encode(channel, message, writer);

            // flush the Message contents
            writer.Flush();
            if (filter)
            {
                // in the case of fitered output, some implementations must be
                // closed in order to be fully flushed (e.g. CompressionFilter);
                // this is safe because FilterWriter() shields the base stream
                writer.Close();
            }
        }

        /// <summary>
        /// Enforce the message size limit of an incoming message
        /// </summary>
        /// <param name="size">
        /// The message size.
        /// </param>
        /// <exception cref="IOException">
        /// When message size exceeds the maximum.
        /// </exception>
        protected virtual void EnforceMaxIncomingMessageSize(long size)
        {
            long max = MaxIncomingMessageSize;
            if (max > 0 && size > max)
            {
                throw new IOException("Message length: " + size + " exceeds the maximum incoming message size.");
            }
        }

        /// <summary>
        /// Enforce the message size limit of an outgoing message
        /// </summary>
        /// <param name="size">
        /// The message size.
        /// </param>
        /// <exception cref="IOException">
        /// When message size exceeds the maximum.
        /// </exception>
        protected virtual void EnforceMaxOutgoingMessageSize(long size)
        {
            long max = MaxOutgoingMessageSize;
            if (max > 0 && size > max)
            {
                throw new IOException("Message length: " + size + " exceeds the maximum outgoing message size.");
            }
        }

        /// <summary>
        /// Filter the given <see cref="DataReader"/> using the list of
        /// <see cref="IWrapperStreamFactory"/> objects.
        /// </summary>
        /// <remarks>
        /// If the list of <b>IWrapperStreamFactory</b> objects is
        /// <c>null</c> or empty, the given <b>DataReader</b> is
        /// returned.
        /// </remarks>
        /// <param name="reader">
        /// The <b>DataReader</b> to filter.
        /// </param>
        /// <returns>
        /// A filtered <b>DataReader</b>.
        /// </returns>
        protected virtual DataReader FilterReader(DataReader reader)
        {
            Stream input = new ShieldedStream(reader.BaseStream);
            IList  list  = WrapperStreamFactoryList;
            if (list != null)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    input = ((IWrapperStreamFactory)list[i]).GetInputStream(input);
                }
            }

            return new DataReader(input);
        }

        /// <summary>
        /// Filter the given <see cref="DataWriter"/> using the list of
        /// <see cref="IWrapperStreamFactory"/> objects.
        /// </summary>
        /// <remarks>
        /// If the list of <b>IWrapperStreamFactory</b> objects is
        /// <c>null</c> or empty, the given <b>DataWriter</b> is
        /// returned.
        /// </remarks>
        /// <param name="writer">
        /// The <b>DataWriter</b> to filter.
        /// </param>
        /// <returns>
        /// A filtered <b>DataWriter</b>.
        /// </returns>
        protected virtual DataWriter FilterWriter(DataWriter writer)
        {
            Stream output = new ShieldedStream(writer.BaseStream);
            IList  list   = WrapperStreamFactoryList;
            if (list != null)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    output = ((IWrapperStreamFactory)list[i]).GetOutputStream(output);
                }
            }

            return new DataWriter(output);
        }

        /// <summary>
        /// Factory method: create a new <see cref="Connection"/>.
        /// </summary>
        /// <remarks>
        /// Implementations must configure the <b>Connection</b> with a
        /// reference to this <b>IConnectionManager</b>.
        /// </remarks>
        /// <returns>
        /// A new <b>Connection</b> object that has yet to be opened.
        /// </returns>
        protected virtual Connection InstantiateConnection()
        {
            return null;
        }

        /// <summary>
        /// Called after a <see cref="Channel"/> has been closed.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        /// <param name="channel">
        /// The <b>Channel</b> that has been closed.
        /// </param>
        public virtual void OnChannelClosed(IChannel channel)
        {
            if (channel.Id != 0 && CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Quiet))
            {
                CacheFactory.Log("Closed: " + channel, CacheFactory.LogLevel.Quiet);
            }
        }

        /// <summary>
        /// Called after a <see cref="Channel"/> has been opened.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        /// <param name="channel">
        /// The <b>Channel</b> that has been opened.
        /// </param>
        public virtual void OnChannelOpened(Channel channel)
        {
            if (channel.Id != 0 && CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Quiet))
            {
                CacheFactory.Log("Opened: " + channel, CacheFactory.LogLevel.Quiet);
            }
        }

        /// <summary>
        /// Called after a <see cref="Connection"/> has closed.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        /// <param name="connection">
        /// The <b>Connection</b> that was closed.
        /// </param>
        public virtual void OnConnectionClosed(Connection connection)
        {
            if (InternalConnection == connection)
            {
                return;
            }

            DispatchConnectionEvent(connection, ConnectionEventType.Closed, null);

            if (CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Quiet))
            {
                CacheFactory.Log("Closed: " + connection, CacheFactory.LogLevel.Quiet);
            }
        }

        /// <summary>
        /// Called after a <see cref="Connection"/> is closed due to an error
        /// or exception.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        /// <param name="connection">
        /// The <b>Connection</b> that was closed.
        /// </param>
        /// <param name="e">
        /// The reason the <b>Connection</b> was closed.
        /// </param>
        public virtual void OnConnectionError(Connection connection, Exception e)
        {
            if (InternalConnection == connection)
            {
                return;
            }

            DispatchConnectionEvent(connection, ConnectionEventType.Error, e);

            if (CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Quiet))
            {
                CacheFactory.Log("Closed: " + connection + ": due to: " + e,
                        CacheFactory.LogLevel.Quiet);
            }
        }

        /// <summary>
        /// Called after a <see cref="Connection"/> has been successfully
        /// established.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        /// <param name="connection">
        /// The <b>Connection</b> that was opened.
        /// </param>
        public virtual void OnConnectionOpened(Connection connection)
        {
            if (InternalConnection == connection)
            {
                return;
            }

            DispatchConnectionEvent(connection, ConnectionEventType.Opened, null);

            if (CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Quiet))
            {
                CacheFactory.Log("Opened: " + connection, CacheFactory.LogLevel.Quiet);
            }
        }

        /// <summary>
        /// Called when an exception occurs during <see cref="IMessage"/>
        /// decoding.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        /// <param name="e">
        /// The <b>Exception</b> thrown during decoding.
        /// </param>
        /// <param name="reader">
        /// The <see cref="DataReader"/> that contains the encoded
        /// <b>IMessage</b>.
        /// </param>
        /// <param name="connection">
        /// The <see cref="Connection"/> that received the encoded
        /// <b>IMessage</b>.
        /// </param>
        /// <param name="filter">
        /// <b>true</b> iff the <b>DataReader</b> was filtered using the list
        /// of configured <see cref="IWrapperStreamFactory"/> objects.
        /// </param>
        /// <seealso cref="OnNotify"/>
        protected virtual void OnMessageDecodeException(Exception e, DataReader reader, Connection connection, bool filter)
        {
            CacheFactory.Log("An exception occurred while decoding a Message for Service="
                             + ServiceName + " received from: " + connection, e, CacheFactory.LogLevel.Error);

            // resolve the Channel
            Channel channel;
            try
            {
                // filter the input, if necessary
                if (filter)
                {
                    reader = FilterReader(reader);
                }
                channel = (Channel) connection.GetChannel(reader.ReadPackedInt32());
            }
            catch (IOException)
            {
                channel = null;
            }

            // close the Channel or Connection
            if (channel == null || !channel.IsOpen || channel.Id == 0)
            {
                connection.Close(true, e, false);
            }
            else
            {
                channel.Close(true, e);
            }
        }

        /// <summary>
        /// Called when an exception occurs during <see cref="IMessage"/>
        /// encoding.
        /// </summary>
        /// <remarks>
        /// This method is called on both client and service threads.
        /// </remarks>
        /// <param name="e">
        /// The <b>Exception</b> thrown during encoding.
        /// </param>
        /// <param name="message">
        /// The <b>IMessage</b> being encoded.
        /// </param>
        /// <seealso cref="Send"/>
        protected virtual void OnMessageEncodeException(Exception e, IMessage message)
        {
            CacheFactory.Log("An exception occurred while encoding a " + message.GetType().Name
                             + " for Service=" + ServiceName, e, CacheFactory.LogLevel.Error);

            // close the Channel or Connection
            Channel channel = (Channel) message.Channel;
            if (!channel.IsOpen || channel.Id == 0)
            {
                Connection connection = (Connection) channel.Connection;

                connection.CloseOnExit    = true;
                connection.CloseNotify    = true;
                connection.CloseException = e;
            }
            else
            {
                channel.CloseOnExit    = true;
                channel.CloseNotify    = true;
                channel.CloseException = e;
            }
        }

        /// <summary>
        /// Open a new <see cref="IChannel"/>.
        /// </summary>
        /// <remarks>
        /// This method is called by
        /// <see cref="Connection.OpenChannel"/> and is always run on client
        /// threads.
        /// </remarks>
        public virtual IChannel OpenChannel(Connection connection, IProtocol protocol, string name, IReceiver receiver, IPrincipal principal)
        {
            Debug.Assert(connection != null);

            IChannel        channel0 = InternalChannel;
            IMessageFactory factory0 = channel0.MessageFactory;

            OpenChannel request = (OpenChannel)
                factory0.CreateMessage(Tangosol.Util.Daemon.QueueProcessor.Service.Peer.OpenChannel.TYPE_ID);

            request.Connection    = connection;
            request.IdentityToken = SerializeIdentityToken(GenerateIdentityToken(principal));
            request.Principal     = principal;
            request.Protocol      = protocol;
            request.Receiver      = receiver;
            request.ReceiverName  = name;

            IStatus status = (IStatus) channel0.Request(request);

            OpenChannelResponse response = (OpenChannelResponse) status.WaitForResponse(RequestTimeout);

            return (IChannel) response.Result;
        }

        /// <summary>
        /// Open the given <see cref="Connection"/>.
        /// </summary>
        /// <remarks>
        /// This method is called by <see cref="Connection.Open"/> and is
        /// always run on client threads.
        /// </remarks>
        /// <param name="connection">
        /// The <b>Connection</b> to open.
        /// </param>
        public virtual void OpenConnection(Connection connection)
        {
            Debug.Assert(connection != null);

            IChannel        channel0 = InternalChannel;
            IMessageFactory factory0 = channel0.MessageFactory;

            var request = (OpenConnection)
                factory0.CreateMessage(Tangosol.Util.Daemon.QueueProcessor.Service.Peer.OpenConnection.TYPE_ID);

            request.ConnectionOpen = connection;

            var status = (IStatus) channel0.Request(request);
            if (status != null)
            {
                try
                {
                    status.WaitForResponse(RequestTimeout);
                }
                catch (RequestTimeoutException e)
                {
                    connection.Close(false, e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Parse the string value of the child <b>IXmlElement</b> with the
        /// given name as a memory size in bytes.
        /// </summary>
        /// <remarks>
        /// If the specified child <b>IXmlElement</b> does not exist or is
        /// empty, the specified default value is returned.
        /// </remarks>
        /// <param name="xml">
        /// The parent <b>IXmlElement</b>.
        /// </param>
        /// <param name="name">
        /// The name of the child <b>IXmlElement</b>.
        /// </param>
        /// <param name="defaultValue">
        /// The default value.
        /// </param>
        /// <returns>
        /// The memory size (in bytes) represented by the specified child
        /// <b>IXmlElement</b>.
        /// </returns>
        protected static long ParseMemorySize(IXmlElement xml, string name, long defaultValue)
        {
            if (xml == null)
            {
                return defaultValue;
            }

            string  bytes = xml.GetSafeElement(name).GetString();
            if (bytes.Length == 0)
            {
                return defaultValue;
            }

            try
            {
                return XmlHelper.ParseMemorySize(bytes);
            }
            catch (Exception e)
            {
                throw new Exception("illegal \"" + name + "\" value: " + bytes, e);
            }
        }

        /// <summary>
        /// Ping the <see cref="Connection"/>(s) managed by this
        /// <b>IConnectionManager</b>.
        /// </summary>
        protected virtual void Ping()
        {}

        /// <summary>
        /// Handle the given <see cref="IMessage"/> by either adding it to
        /// the service thread queue (internal messages) or sending
        /// asynchronously (external messages).
        /// </summary>
        /// <remarks>
        /// This method is called on both client and service threads.
        /// </remarks>
        /// <param name="message">
        /// The <b>IMessage</b> to post.
        /// </param>
        public virtual void Post(IMessage message)
        {
            Debug.Assert(message != null);

            // monitor the event dispatcher queue and slow down if it gets too long
            EventDispatcher dispatcher = Dispatcher;
            if (dispatcher != null)
            {
                dispatcher.DrainOverflow();
            }

            IChannel channel = message.Channel;
            Debug.Assert(channel != null);

            if (this == channel.Receiver && message.TypeId < 0)
            {
                // internal message
                Queue.Add(message);
            }
            else
            {
                // external message
                Send(message);
            }
        }

        /// <summary>
        /// Called by the underlying transport when an encoded
        /// <see cref="IMessage"/> is received.
        /// </summary>
        /// <remarks>
        /// Called on client threads.
        /// </remarks>
        /// <param name="reader">
        /// The <see cref="DataReader"/> that contains the encoded
        /// <b>IMessage</b>.
        /// </param>
        /// <param name="connection">
        /// The <see cref="Connection"/> that received the encoded
        /// <b>IMessage</b>.
        /// </param>
        public virtual void Receive(DataReader reader, Connection connection)
        {
            Debug.Assert(reader != null);
            Debug.Assert(connection != null);

            IChannel channel0 = InternalChannel;
            if (channel0 == null)
            {
                return;
            }

            IMessageFactory factory0 = channel0.MessageFactory;
            if (factory0 == null)
            {
                return;
            }

            EncodedMessage message = (EncodedMessage) factory0.CreateMessage(EncodedMessage.TYPE_ID);

            message.Channel    = channel0;
            message.Connection = connection;
            message.Reader     = reader;

            Post(message);
        }

        /// <summary>
        /// Perform an asynchronous send of the given <see cref="IMessage"/>
        /// using the underlying transport.
        /// </summary>
        /// <remarks>
        /// This method is called on both client and service threads.
        /// </remarks>
        /// <param name="message">
        /// The <b>IMessage</b> to send.
        /// </param>
        protected virtual void Send(IMessage message)
        {
            Channel channel = (Channel) message.Channel;
            Debug.Assert(channel != null);
            Debug.Assert(channel.IsActiveThread);
            
            // cache the connection earlier than necessary as it's needed
            // to query the message debug configuration
            Connection connection = (Connection) channel.Connection;
            bool       fMsgDebug  = DEBUG && CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Quiet);
            string     sDebugMsg  = null;

            // if messaging debug enabled, cache the result of toString() on message *before* serialization
            // to ensure all state present when creating the message
            if (fMsgDebug)
            {
                sDebugMsg = message.ToString();
            }

            // create a new DataWriter on top of a MemoryStream that has been
            // padded with 5 extra bytes at the beginning; this allows
            // Connection subclasses to length encode the Message without
            // having to recopy the entire Message into a new byte array
            MemoryStream stream = new MemoryStream();
            for (int i = 0; i < 5; ++i)
            {
                stream.WriteByte(0);
            }
            DataWriter writer = new DataWriter(stream);

            // encode the Message
            try
            {
                EncodeMessage(message, writer, true);
                EnforceMaxOutgoingMessageSize(writer.BaseStream.Length);
            }
            catch (Exception e)
            {
                OnMessageEncodeException(e, message);
                throw;
            }

            // send the Message
            try
            {
                connection.Send(writer);
            }
            catch (Exception e)
            {
                connection.CloseOnExit    = true;
                connection.CloseNotify    = false;
                connection.CloseException = e;

                throw;
            }

            // update stats
            StatsSent      += 1;
            StatsBytesSent += writer.BaseStream.Length;

            if (fMsgDebug)
            {
                CacheFactory.Log("Sent: " + sDebugMsg, CacheFactory.LogLevel.Quiet);
            }
        }

        #endregion

        #region IReceiver implementation

        /// <summary>
        /// The name of this IReceiver.
        /// </summary>
        /// <remarks>
        /// If the IReceiver is registered with a
        /// <see cref="IConnectionManager"/>, the registration and any
        /// subsequent accesses are by the IReceiver's name, meaning that the
        /// name must be unique within the domain of the
        /// <b>IConnectionManager</b>.
        /// </remarks>
        /// <value>
        /// The IReceiver name.
        /// </value>
        public virtual string Name
        {
            get { return ServiceName; }
        }

        /// <summary>
        /// The <see cref="IProtocol"/> understood by the IReceiver.
        /// </summary>
        /// <remarks>
        /// Only <b>IChannel</b> objects with the specified <b>IProtocol</b>
        /// can be registered with this IReceiver.
        /// </remarks>
        /// <value>
        /// The <b>IProtocol</b> used by this IReceiver.
        /// </value>
        public virtual IProtocol Protocol
        {
            get
            {
                IProtocol protocol = m_protocol;
                if (protocol == null)
                {
                    Protocol = (protocol = MessagingProtocol.Instance);
                }
                return protocol;
            }
            //TODO: protected
            set { m_protocol = value; }
        }

        /// <summary>
        /// Notify this IReceiver that it has been associated with a
        /// <b>IChannel</b>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method is invoked by the <b>IChannel</b> when an IReceiver is
        /// associated with the <b>IChannel</b>.</p>
        /// <p>
        /// Once registered, the IReceiver will receive all unsolicited
        /// <b>IMessage</b> objects sent through the <b>IChannel</b> until
        /// the <b>IChannel</b> is unregistered or closed. Without a
        /// IReceiver, the unsolicited <b>IMessage</b> objects are executed
        /// with only an <b>IChannel</b> as context; with an IReceiver, the
        /// IReceiver is given the <b>IMessage</b> to process, and may
        /// execute the <b>IMessage</b> in turn.</p>
        /// </remarks>
        /// <param name="channel">
        /// An <b>IChannel</b> that has been associated with this IReceiver.
        /// </param>
        public virtual void RegisterChannel(IChannel channel)
        {}

        /// <summary>
        /// Called when an unsolicited (non-Response) <b>IMessage</b> is
        /// received by an <b>IChannel</b> that had been previously
        /// registered with this IReceiver.
        /// </summary>
        /// <param name="message">
        /// An unsolicited <b>IMessage</b> received by a registered
        /// <b>IChannel</b>.
        /// </param>
        public virtual void OnMessage(IMessage message)
        {
            message.Run();
        }

        /// <summary>
        /// Unregister the given <b>IChannel</b> with this IReceiver.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method is invoked by the <b>IChannel</b> when an IReceiver is
        /// disassociated with the <b>IChannel</b>.</p>
        /// <p>
        /// Once unregistered, the IReceiver will no longer receive
        /// unsolicited <b>IMessage</b> objects sent through the
        /// <b>IChannel</b>.</p>
        /// </remarks>
        /// <param name="channel">
        /// An <b>IChannel</b> that was disassociated with this IReceiver.
        /// </param>
        public virtual void UnregisterChannel(IChannel channel)
        {}

        #endregion

        #region IConnectionManager implementation

        /// <summary>
        /// Gets a map of <b>IProtocol</b> names to <b>IProtocol</b> objects.
        /// </summary>
        /// <remarks>
        /// The client should assume that the returned map is an immutable
        /// snapshot of the actual map of <b>IProtocol</b> objects maintained
        /// by this IConnectionManager.
        /// </remarks>
        /// <value>
        /// A map of all registered <b>IProtocol</b> objects, keyed by the
        /// <b>IProtocol</b> name.
        /// </value>
        public virtual IDictionary Protocols
        {
            get
            {
                IDictionary map = ProtocolMap;
                lock (map.SyncRoot)
                {
                    return new HashDictionary(map);
                }
            }
        }

        /// <summary>
        /// Gets a map of <b>IReceiver</b> names to <b>IReceiver</b> objects.
        /// </summary>
        /// <remarks>
        /// The client should assume that the returned map is an immutable
        /// snapshot of the actual map of <b>IReceiver</b> objects maintained
        /// by this IConnectionManager.
        /// </remarks>
        /// <value>
        /// A map of all registered <b>IReceiver</b> objects, keyed by the
        /// <b>IReceiver</b> name.
        /// </value>
        public virtual IDictionary Receivers
        {
            get
            {
                IDictionary map = ReceiverMap;
                lock (map.SyncRoot)
                {
                    return new HashDictionary(map);
                }
            }
        }

        /// <summary>
        /// The <see cref="ICodec"/> that will be used to encode and decode
        /// <b>IMessages</b> sent through <b>IConnections</b> managed by this
        /// IConnectionManager.
        /// </summary>
        /// <value>
        /// The <see cref="ICodec"/> object.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If the IConnectionManager is running.
        /// </exception>
        public virtual ICodec Codec
        {
            get { return m_codec; }
            set { m_codec = value; }
        }

        /// <summary>
        /// Gets an <b>IProtocol</b> that was registered with this
        /// IConnectionManager.
        /// </summary>
        /// <param name="name">
        /// The name of the registered <b>IProtocol</b>.
        /// </param>
        /// <returns>
        /// The registered <b>IProtocol</b> or <c>null</c> if a
        /// <b>IProtocol</b> with the given name is not registered with this
        /// IConnectionManager.
        /// </returns>
        public virtual IProtocol GetProtocol(string name)
        {
            return (IProtocol) ProtocolMap[name];
        }

        /// <summary>
        /// Register an <b>IProtocol</b> with this IConnectionManager.
        /// </summary>
        /// <remarks>
        /// This method may only be called before the IConnectionManager
        /// is started.
        /// </remarks>
        /// <param name="protocol">
        /// The new <b>IProtocol</b> to register; if the <b>IProtocol</b> has
        /// already been registered, this method has no effect.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If the IConnectionManager is running.
        /// </exception>
        public virtual void RegisterProtocol(IProtocol protocol)
        {
            if (ServiceState > ServiceState.Initial)
            {
                throw new InvalidOperationException();
            }

            if (protocol == null)
            {
                throw new ArgumentNullException("protocol cannot be null");
            }

            string name = protocol.Name;
            if (name == null)
            {
                throw new ArgumentException("missing protocol name: " + protocol);
            }

            ProtocolMap[name] = protocol;
        }

        /// <summary>
        /// Return an <b>IReceiver</b> that was registered with this
        /// IConnectionManager.
        /// </summary>
        /// <remarks>
        /// The client should assume that the returned map is an immutable
        /// snapshot of the actual map of <b>IReceiver</b> objects maintained
        /// by this IConnectionManager.
        /// </remarks>
        /// <param name="name">
        /// The name of the registered <b>IReceiver</b>.
        /// </param>
        /// <returns>
        /// The registered <b>IReceiver</b> or <c>null</c> if a
        /// <b>IReceiver</b> with the given name is not registered with this
        /// IConnectionManager.
        /// </returns>
        public virtual IReceiver GetReceiver(string name)
        {
            return (IReceiver) ReceiverMap[name];
        }

        /// <summary>
        /// Register an <b>IReceiver</b> that will received unsolicited
        /// <b>IMessage</b> objects sent through <b>IChannel</b> objects
        /// associated with the <b>IReceiver</b> name and <b>IProtocol</b>.
        /// </summary>
        /// <remarks>
        /// This method may only be called before the IConnectionManager is
        /// started.
        /// </remarks>
        /// <param name="receiver">
        /// The new <b>IReceiver</b> to register; if the <b>IReceiver</b> has
        /// already been registered, this method has no effect.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If the IConnectionManager is running.
        /// </exception>
        public virtual void RegisterReceiver(IReceiver receiver)
        {
            if (ServiceState > ServiceState.Initial)
            {
                throw new InvalidOperationException();
            }

            if (receiver == null)
            {
                throw new ArgumentNullException("receiver cannot be null");
            }

            string name = receiver.Name;
            if (name == null)
            {
                throw new ArgumentException("missing receiver name: " + receiver);
            }

            ReceiverMap[name] = receiver;
        }

        /// <summary>
        /// Invoked after an <see cref="IConnection"/> has been successfully
        /// established.
        /// </summary>
        public virtual event ConnectionEventHandler ConnectionOpened
        {
            add
            {
                EnsureEventDispatcher();
                m_connectionOpened += value;
            }
            remove
            {
                m_connectionOpened -= value;
            }
        }

        /// <summary>
        /// Invoked after an <see cref="IConnection"/> is closed.
        /// </summary>
        /// <remarks>
        /// After this event is raised, any attempt to use the
        /// <b>IConnection</b> (or any <b>IChannel</b> created by the
        /// <b>IConnection</b>) may result in an exception.
        /// </remarks>
        public virtual event ConnectionEventHandler ConnectionClosed
        {
            add
            {
                EnsureEventDispatcher();
                m_connectionClosed += value;
            }
            remove
            {
                m_connectionClosed -= value;
            }
        }

        /// <summary>
        /// Invoked when the <b>IConnectionManager</b> detects that the
        /// underlying communication channel has been closed by the peer,
        /// severed, or become unusable.
        /// </summary>
        /// <remarks>
        /// After this event is raised, any attempt to use the
        /// <b>IConnection</b> (or any <b>IChannel</b> created by the
        /// <b>IConnection</b>) may result in an exception.
        /// </remarks>
        public virtual event ConnectionEventHandler ConnectionError
        {
            add
            {
                EnsureEventDispatcher();
                m_connectionError += value;
            }
            remove
            {
                m_connectionError -= value;
            }
        }

        #endregion

        #region Service overrides

        /// <summary>
        /// Configure the controllable service.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// This method can only be called before the controllable service
        /// is started.
        /// </remarks>
        /// <param name="xml">
        /// An <see cref="IXmlElement"/> carrying configuration information
        /// specific to the IControllable object.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service is already running.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the configuration information is invalid.
        /// </exception>
        public override void Configure(IXmlElement xml)
        {
            lock (this)
            {
                base.Configure(xml);
                if (xml == null)
                {
                    return;
                }

                IXmlElement xmlCat;

                // <outgoing-message-handler>
                xmlCat = xml.GetSafeElement("outgoing-message-handler");

                // <request-timeout>
                RequestTimeout = ParseTime(xmlCat, "request-timeout", RequestTimeout);

                // <heartbeat-timeout>
                PingTimeout = ParseTime(xmlCat, "heartbeat-timeout", RequestTimeout);

                // <heartbeat-interval>
                PingInterval = ParseTime(xmlCat, "heartbeat-interval", PingInterval);

                // make sure the heartbeat timeout <= interval
                if (PingInterval > 0L)
                {
                    if (PingTimeout == 0L)
                    {
                        PingTimeout = PingInterval;
                    }
                    else
                    {
                        PingTimeout = Math.Min(PingInterval, PingTimeout);
                    }
                }

                // <max-message-size>
                MaxOutgoingMessageSize = ParseMemorySize(xmlCat, "max-message-size", MaxOutgoingMessageSize);

                // <incoming-message-handler>
                xmlCat = xml.GetSafeElement("incoming-message-handler");
                base.Configure(xmlCat);

                // <max-message-size>
                MaxIncomingMessageSize = ParseMemorySize(xmlCat, "max-message-size", MaxIncomingMessageSize);

                // <use-filters>
                xmlCat = xml.GetSafeElement("use-filters");

                // <filter-name>
                IList filterList = new ArrayList();
                for (IEnumerator enumerator = xmlCat.GetElements("filter-name"); enumerator.MoveNext(); )
                {
                    var xmlSub = (IXmlElement) enumerator.Current;
                    var filter = OperationalContext.FilterMap[xmlSub.GetString()];
                    if (filter == null)
                    {
                        throw new ArgumentException("Filter " + xmlSub.GetString() + " not found.");
                    }
                    filterList.Add(filter);
                }
                if (filterList.Count != 0)
                {
                    WrapperStreamFactoryList = ArrayList.ReadOnly(filterList);
                }

                // <message-codec>
                xmlCat = xml.GetElement("message-codec");
                if (xmlCat != null)
                {
                    Codec = (ICodec) XmlHelper.CreateInstance(xmlCat, null, typeof(ICodec));
                }

                ServiceConfig = xml;
            }
        }

        /// <summary>
        /// Return a human-readible description of the Service statistics.
        /// </summary>
        public override string FormatStats()
        {
            long total    = Math.Max(DateTime.UtcNow.Ticks / 10000 - StatsReset, 0L);
            long received = StatsBytesReceived;
            long sent     = StatsBytesSent;
            long bpsIn    = total == 0L ? 0L : (received / total) * 1000L;
            long bpsOut   = total == 0L ? 0L : (sent / total) * 1000L;

            StringBuilder sb = new StringBuilder(base.FormatStats());
            sb.Append(", BytesReceived=")
                .Append(StringUtils.ToMemorySizeString(received, false))
                .Append(", BytesSent=")
                .Append(StringUtils.ToMemorySizeString(sent, false))
                .Append(", ThroughputInbound=")
                .Append(StringUtils.ToBandwidthString(bpsIn, false))
                .Append(", ThroughputOutbound=")
                .Append(StringUtils.ToBandwidthString(bpsOut, false));

            return sb.ToString();
        }

        /// <summary>
        /// Human-readable description of additional Service properties.
        /// </summary>
        /// <remarks>
        /// Used by <see cref="Service.ToString"/>.
        /// </remarks>
        public override string Description
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                //TODO: DaemonPool
                /*DaemonPool pool = getDaemonPool();
                if (pool.isStarted())
                {
                    sb.append(", ThreadCount=")
                      .append(pool.getDaemonCount())
                      .append(", HungThreshold=")
                      .append(pool.getHungThreshold())
                      .append(", TaskTimeout=")
                      .append(pool.getTaskTimeout());
                }
                else
                {
                    sb.append(", ThreadCount=0");
                }*/

                IList list = WrapperStreamFactoryList;
                if (list != null && list.Count > 0)
                {
                    sb.Append(", Filters=[");

                    foreach (IWrapperStreamFactory factory in list)
                    {
                        sb.Append(factory.GetType().Name);
                        sb.Append(',');
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(']');
                }

                ICodec codec = Codec;
                if (codec != null)
                {
                    sb.Append(", Codec=")
                        .Append(codec);
                }

                sb.Append(", PingInterval=")
                    .Append(PingInterval)
                    .Append(", PingTimeout=")
                    .Append(PingTimeout)
                    .Append(", RequestTimeout=")
                    .Append(RequestTimeout)
                    .Append(", MaxIncomingMessageSize=")
                    .Append(MaxIncomingMessageSize)
                    .Append(", MaxOutgoingMessageSize=")
                    .Append(MaxOutgoingMessageSize);

                return sb.ToString();
            }
        }

        /// <summary>
        /// The number of milliseconds that the daemon will wait for
        /// notification.
        /// </summary>
        /// <remarks>
        /// Zero means to wait indefinitely. Negative value means to skip
        /// waiting altogether.
        /// </remarks>
        /// <value>
        /// The number of milliseconds that the daemon will wait for
        /// notification.
        /// </value>
        public override long WaitMillis
        {
            get
            {
                long millis = base.WaitMillis;
                if (PingInterval > 0L)
                {
                    long nowMillis  = DateTimeUtils.GetSafeTimeMillis();
                    long nextMillis = Math.Min(PingNextMillis, PingNextCheckMillis);
                    long next       = nextMillis > nowMillis ? nextMillis - nowMillis : -1L;

                    return millis == 0L ? next : Math.Min(next, millis);
                }
                else
                {
                    return millis;
                }
            }
            set
            {
                base.WaitMillis = value;
            }
        }

        /// <summary>
        /// Event notification called once the daemon's thread starts and
        /// before the daemon thread goes into the "wait - perform" loop.
        /// </summary>
        /// <remarks>
        /// Unlike the <c>OnInit()</c> event, this method executes on the
        /// daemon's thread.
        /// <p>
        /// This method is called while the caller's thread is still waiting
        /// for a notification to  "unblock" itself.</p>
        /// <p>
        /// Any exception thrown by this method will terminate the thread
        /// immediately.</p>
        /// </remarks>
        protected override void OnEnter()
        {
            // open the internal Connection and Channel
            InternalConnection.OpenInternal();

            StartTimestamp = DateTimeUtils.GetSafeTimeMillis();

            ResetStats();

            ServiceState = ServiceState.Starting;

            Channel channel = InternalChannel;
            channel.Send(channel.CreateMessage(NotifyStartup.TYPE_ID));
        }

        /// <summary>
        /// Event notification called right before the daemon thread
        /// terminates.
        /// </summary>
        /// <remarks>
        /// This method is guaranteed to be called only once and on the
        /// daemon's thread.
        /// </remarks>
        protected override void OnExit()
        {
            base.OnExit();

            InternalConnection.CloseInternal(false, null, -1);
        }

        /// <summary>
        /// Initialization method.
        /// </summary>
        public override void OnInit()
        {
            // add the MessagingProtocol
            IProtocol protocol = Protocol;
            RegisterProtocol(protocol);

            // initialize the internal Connection and Channel
            Connection  connection        = new Connection();
            IDictionary messageFactoryMap = new HashDictionary();
            messageFactoryMap.Add(protocol.Name, protocol.GetMessageFactory(protocol.CurrentVersion));

            connection.ConnectionManager = this;
            connection.Id                = ProcessId;
            connection.MessageFactoryMap = messageFactoryMap;

            Channel channel = (Channel) connection.GetChannel(0);
            channel.Receiver = this;

            InternalChannel    = channel;
            InternalConnection = connection;

            base.OnInit();
        }

        /// <summary>
        /// Event notification to perform a regular daemon activity.
        /// </summary>
        /// <remarks>
        /// To get it called, another thread has to set IsNotification to
        /// <b>true</b>:
        /// <c>daemon.IsNotification = true;</c>
        /// </remarks>
        protected override void OnNotify()
        {
            long start         = DateTimeUtils.GetSafeTimeMillis();
            long received      = StatsReceived;
            long bytesReceived = StatsBytesReceived;
            //TODO: DaemonPool pool      = getDaemonPool();

            while (!IsExiting)
            {
                IMessage message = Queue.RemoveNoWait() as IMessage;
                if (message == null)
                {
                    break;
                }
                else
                {
                    received++;
                }

                // decode the Message if necessary
                if (message is EncodedMessage)
                {
                    EncodedMessage messageImpl = (EncodedMessage) message;

                    DataReader reader = messageImpl.Reader;
                    if (reader == null || reader.BaseStream.Length == 0)
                    {
                        continue;
                    }
                    long bytes = reader.BaseStream.Length;

                    // update stats
                    Connection connection = messageImpl.Connection;
                    connection.StatsBytesReceived += bytes;
                    bytesReceived                 += bytes;

                    try
                    {
                        message = DecodeMessage(reader, connection, true);
                    }
                    catch (Exception e)
                    {
                        OnMessageDecodeException(e, reader, connection, true);
                        continue;
                    }

                    if (message == null)
                    {
                        continue;
                    }

                    if (DEBUG && CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Quiet))
                    {
                        CacheFactory.Log("Received: " + message, CacheFactory.LogLevel.Quiet);
                    }
                }

                // make sure the target Channel is still open
                Channel channel = (Channel) message.Channel;
                if (channel == null || !channel.IsOpen)
                {
                    continue;
                }

                // make sure the target Connection is still open
                Connection conn = (Connection) channel.Connection;
                if (conn == null || !conn.IsOpen)
                {
                    continue;
                }

                // update stats
                conn.StatsReceived += 1;

                // execute the Message
                //if (this == channel.Receiver || message.ExecuteInOrder) //TODO: || !pool.isStarted()
                //{
                    // (1) the Message is an internal Message; or
                    // (2) the Message is a "Channel0" Message; or
                    // (3) the daemon pool has not been started; or
                    // (4) the Message calls for in-order execution
                    channel.Receive(message);
                //}
                //else
                //{
                //  //TODO: pool.add(message);
                //}
            }

            // heartbeat
            long now = DateTimeUtils.GetSafeTimeMillis();
            if (now >= PingNextCheckMillis)
            {
                CheckPingTimeouts();
                PingLastCheckMillis = now;
            }
            if (now >= PingNextMillis)
            {
                Ping();
                PingLastCheckMillis = 0L;
                PingLastMillis      = now;
            }

            StatsReceived      = received;
            StatsBytesReceived = bytesReceived;
            StatsCpu          += (now - start);

            base.OnNotify();
        }

        /// <summary>
        /// The default implementation of this method sets
        /// <see cref="Service.IsAcceptingClients"/> to <b>true</b>.
        /// </summary>
        /// <remarks>
        /// If the Service has not completed preparing at this point, then
        /// the Service must override this implementation and only set
        /// <b>IsAcceptingClients</b> to <b>true</b> when the Service has
        /// actually "finished starting".
        /// </remarks>
        public override void OnServiceStarted()
        {
            base.OnServiceStarted();

            // start the daemon pool if necessary
            //TODO: DaemonPool
            /*DaemonPool pool = getDaemonPool();
            if (pool.getDaemonCount() > 0)
            {
                pool.setThreadGroup(new ThreadGroup(getServiceName()));
                pool.start();
            }*/

            CacheFactory.Log("Started: " + this, CacheFactory.LogLevel.Debug);
        }

        /// <summary>
        /// The default implementation of this method does nothing.
        /// </summary>
        protected override void OnServiceStarting()
        {
            base.OnServiceStarting();

            // make sure a Codec is set up
            if (Codec == null)
            {
                Codec = new Codec();
            }

            // set up the Protocol version map
            IDictionary map = new HashDictionary();
            foreach (IProtocol protocol in ProtocolMap.Values)
            {
                string name             = protocol.Name;
                int    currentVersion   = protocol.CurrentVersion;
                int    supportedVersion = protocol.SupportedVersion;

                if (name == null)
                {
                    throw new ArgumentException("protocol has no name: " + protocol);
                }

                map[name] = new Int32[] {currentVersion, supportedVersion};
            }
            ProtocolVersionMap = map;
        }

        /// <summary>
        /// The default implementation of this method sets
        /// <see cref="Service.IsAcceptingClients"/> to <b>false</b>.
        /// </summary>
        protected override void OnServiceStopped()
        {
            base.OnServiceStopped();

            CacheFactory.Log("Stopped: " + this, CacheFactory.LogLevel.Debug);
        }

        /// <summary>
        /// Reset the Service statistics.
        /// </summary>
        public override void ResetStats()
        {
            StatsBytesReceived = 0L;
            StatsBytesSent     = 0L;
            StatsSent          = 0L;
            StatsTimeoutCount  = 0L;

            base.ResetStats();
        }

        /// <summary>
        /// Stop the controllable service.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// This is a controlled shut-down, and is preferred to the
        /// <see cref="IControllable.Stop"/> method.
        /// <p/>
        /// This method should only be called once per the life cycle
        /// of the controllable service. Calling this method for a service
        /// that has already stopped has no effect.
        /// </remarks>
        public override void Shutdown()
        {
            lock (this)
            {
                if (IsStarted)
                {
                    if (ServiceState < ServiceState.Stopping)
                    {
                        // send the request to shut down
                        Channel channel = InternalChannel;
                        channel.Send(channel.CreateMessage(NotifyShutdown.TYPE_ID));
                    }
                }

                Thread thread = Thread;
                if (thread != Thread.CurrentThread)
                {
                    // wait for the service to stop or the thread to die
                    while (IsStarted && ServiceState < ServiceState.Stopped)
                    {
                        Monitor.Wait(this);
                    }

                    if (ServiceState != ServiceState.Stopped)
                    {
                        Stop();
                    }
                }
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// Debug flag.
        /// </summary>
        public static readonly bool DEBUG = Boolean.Parse(ConfigurationUtils.GetProperty("COHERENCE_MESSAGING_DEBUG", "false"));

        /// <summary>
        /// The Channel used for all internal communication.
        /// </summary>
        [NonSerialized]
        private Channel m_channel;

        /// <summary>
        /// The Connection used for all internal communication.
        /// </summary>
        [NonSerialized]
        private Connection m_connection;

        /// <summary>
        /// The ICodec used to encode and decode all messages sent by this
        /// IConnectionManager.
        /// </summary>
        private ICodec m_codec;

        /// <summary>
        /// The number of milliseconds between successive connection "pings"
        /// or 0 if heartbeats are disabled.
        /// </summary>
        private long m_pingInterval;

        /// <summary>
        /// The last time the connection(s) managed by this
        /// IConnectionManager were checked for a "ping" timeout.
        /// </summary>
        [NonSerialized]
        private long m_pingLastCheckMillis;

        /// <summary>
        /// The last time the connection(s) managed by this
        /// IConnectionManager were "pinged".
        /// </summary>
        [NonSerialized]
        private long m_pingLastMillis;

        /// <summary>
        /// The default request timeout for a PingRequest.
        /// </summary>
        private long m_pingTimeout;

        /// <summary>
        /// The unique identifier (UUID) of the process using this
        /// IConnectionManager.
        /// </summary>
        [NonSerialized]
        private static UUID m_processId;

        /// <summary>
        /// Protocol.
        /// </summary>
        private IProtocol m_protocol;

        /// <summary>
        /// The map of registered IProtocol objects.
        /// </summary>
        private IDictionary m_protocolMap = new HashDictionary();

        /// <summary>
        /// A map of version ranges for registered Protocols. The keys are
        /// the names of the Protocols and the values are two element Int32
        /// arrays, the first element being the current version and the 
        /// second being the supported version of the corresponding Protocol.
        /// </summary>
        [NonSerialized]
        private IDictionary m_protocolVersionMap;

        /// <summary>
        /// The map of registered IReceiver objects.
        /// </summary>
        [NonSerialized]
        private IDictionary m_receiverMap = new HashDictionary();

        /// <summary>
        /// The default request timeout for all IChannel objects created by
        /// IConnection objects managed by this IConnectionManager.
        /// </summary>
        private long m_requestTimeout = 30000;

        /// <summary>
        /// The maximum incoming message size.
        /// </summary>
        private long m_maxIncomingMessageSize;

        /// <summary>
        /// The maximum outgoing message size.
        /// </summary>
        private long m_maxOutgoingMessageSize;

        /// <summary>
        /// Statistics: total number of bytes received.
        /// </summary>
        [NonSerialized]
        private long m_statsBytesReceived;

        /// <summary>
        /// Statistics: total number of bytes sent.
        /// </summary>
        [NonSerialized]
        private long m_statsBytesSent;

        /// <summary>
        /// Statistics: total number of messages sent.
        /// </summary>
        [NonSerialized]
        private long m_statsSent;

        /// <summary>
        /// The total number of timed-out requests since the last time the
        /// statistics were reset.
        /// </summary>
        [NonSerialized]
        private long m_statsTimeoutCount;

        /// <summary>
        /// A list of IWrapperStreamFactory objects that affect how messages
        /// are written and read.
        /// </summary>
        [NonSerialized]
        private IList m_wrapperStreamFactoryList;

        /// <summary>
        /// Connection event handlers.
        /// </summary>
        private ConnectionEventHandler m_connectionOpened;
        private ConnectionEventHandler m_connectionClosed;
        private ConnectionEventHandler m_connectionError;

        #endregion
    }
}
