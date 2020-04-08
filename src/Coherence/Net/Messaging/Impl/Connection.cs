/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Threading;

using Tangosol.IO;
using Tangosol.Net.Impl;
using Tangosol.Util;
using Tangosol.Util.Daemon.QueueProcessor.Service.Peer;

namespace Tangosol.Net.Messaging.Impl
{
    /// <summary>
    /// Base definition of an <see cref="IConnection"/>.
    /// </summary>
    /// <author>Ana Cikic  2006.08.18</author>
    /// <author>Goran Milosavljevic  2007.01.14</author>
    /// <seealso cref="IConnection"/>
    public class Connection : Extend, IConnection
    {
        #region Properties

        /// <summary>
        /// An <see cref="ILongArray"/> of open <see cref="IChannel"/>
        /// objects created by this Connection, keyed by <b>IChannel</b>
        /// identifier.
        /// </summary>
        /// <value>
        /// An <b>ILongArray</b> of open <b>IChannel</b> objects.
        /// </value>
        protected ILongArray Channels { get; set; }

        /// <summary>
        /// An <see cref="ILongArray"/> of newly created <b>IChannel</b>
        /// objects that are waiting to be accepted by the peer, keyed by
        /// <b>IChannel</b> identifier.
        /// </summary>
        /// <value>
        /// An <b>ILongArray</b> of newly created <b>IChannel</b> objects
        /// that are waiting to be accepted by the peer.
        /// </value>
        protected ILongArray ChannelsPending { get; set; }

        /// <summary>
        /// The <see cref="ICodec"/> used by the Connection to convert
        /// <see cref="IMessage"/> objects to and from a binary
        /// representation.
        /// </summary>
        /// <value>
        /// The <b>ICodec</b> used to encode/decode <b>IMessage</b> objects.
        /// </value>
        public virtual ICodec Codec
        {
            get { return m_codec; }
            set
            {
                Debug.Assert(!IsOpen);
                m_codec = value;
            }
        }

        /// <summary>
        /// The <see cref="IConnectionManager"/> that created or accepted
        /// this Connection.
        /// </summary>
        /// <value>
        /// The <b>IConnectionManager</b> that manages this Connection.
        /// </value>
        /// <seealso cref="IConnection.ConnectionManager"/>
        public virtual IConnectionManager ConnectionManager
        {
            get { return m_connectionManager; }
            set
            {
                Debug.Assert(!IsOpen);
                m_connectionManager = value;
            }
        }

        /// <summary>
        /// The unique identifier of this IConnection.
        /// </summary>
        /// <value>
        /// The unique identifier of this IConnection or <c>null</c> if the
        /// IConnection has not been accepted.
        /// </value>
        /// <seealso cref="IConnection.Id"/>
        public virtual UUID Id
        {
            get { return m_id; }
            set
            {
                Debug.Assert(Id == null);
                m_id = value;
            }
        }

        /// <summary>
        /// The IMember object for this IConnection.
        /// </summary>
        /// <value>
        /// The IMember object for this IConnection or <c>null</c> if the
        /// IConnection has not been opened.
        /// </value>
        public virtual IMember Member
        {
            get { return m_member; }
            set
            {
                Debug.Assert(m_member == null);
                m_member = value;
            }
        }

        /// <summary>
        /// A map of <b>IMessageFactory</b> objects that may be used by
        /// <b>IChannel</b> objects created by this Connection, keyed by
        /// <b>IProtocol</b> name.
        /// </summary>
        /// <value>
        /// A map of <b>IMessageFactory</b> objects that may be used by
        /// <b>IChannel</b> objects created by this Connection.
        /// </value>
        public virtual IDictionary MessageFactoryMap
        {
            get { return m_messageFactoryMap; }
            set
            {
                m_messageFactoryMap = value;
            }
        }

        /// <summary>
        /// The unique identifier of the peer to which this Connection
        /// object is connected.
        /// </summary>
        /// <value>
        /// The unique identifier of the peer or <c>null</c> if the
        /// Connection is not open.
        /// </value>
        /// <seealso cref="IConnection.PeerId"/>
        public virtual UUID PeerId { get; set; }

        /// <summary>
        /// Returns <b>true</b> if the calling thread is currently executing
        /// within the Connection's <b>Gate</b>.
        /// </summary>
        public virtual bool IsActiveThread
        {
            get { return Gate.IsEnteredByCurrentThread; }
        }

        /// <summary>
        /// Return <b>true</b> if this Connection is open.
        /// </summary>
        /// <remarks>
        /// A Connection can only be used to exchange data when it is open.
        /// </remarks>
        /// <value>
        /// <b>true</b> if this Connection is open.
        /// </value>
        public virtual bool IsOpen
        {
            get { return m_isOpen; }
            set { m_isOpen = value; }
        }

        /// <summary>
        /// Peer notification flag used when the Connection is closed upon
        /// exiting the <b>Gate</b> (see  <see cref="CloseOnExit"/>
        /// property).
        /// </summary>
        public virtual bool CloseNotify
        {
            get { return m_closeNotify; }
            set { m_closeNotify = value; }
        }

        /// <summary>
        /// If <b>true</b>, the Thread that is currently executing within the
        /// Connection should close the Connection immedately upon exiting
        /// the Connection's <b>Gate</b>.
        /// </summary>
        public virtual bool CloseOnExit
        {
            get { return m_closeOnExit; }
            set { m_closeOnExit = value; }
        }

        /// <summary>
        /// The <b>Exception</b> to pass to the <see cref="Close()"/> method
        /// when the Connection is closed upon exiting the Gate
        /// (see <see cref="CloseOnExit"/> property).
        /// </summary>
        public virtual Exception CloseException
        {
            get { return m_closeException; }
            set { m_closeException = value; }
        }

        /// <summary>
        /// The product edition used by the peer.
        /// </summary>
        public virtual int PeerEdition { get; set; }

        /// <summary>
        /// The send time of the last outstanding <b>PingRequest</b> or 0
        /// if a <b>PingRequest</b> is not outstanding.
        /// </summary>
        public virtual long PingLastMillis
        {
            get { return m_pingLastMillis; }
            set { m_pingLastMillis = value; }
        }

        /// <summary>
        /// Statistics: total number of bytes received over this Connection.
        /// </summary>
        public virtual long StatsBytesReceived
        {
            get { return m_statsBytesReceived; }
            set { m_statsBytesReceived = value; }
        }

        /// <summary>
        /// Statistics: total number of bytes sent over this Connection.
        /// </summary>
        public virtual long StatsBytesSent
        {
            get { return m_statsBytesSent; }
            set { m_statsBytesSent = value; }
        }

        /// <summary>
        /// Statistics: total number of Messages received over this
        /// Connection.
        /// </summary>
        public virtual long StatsReceived
        {
            get { return m_statsReceived; }
            set { m_statsReceived = value; }
        }

        /// <summary>
        /// Statistics: date/time value that the stats have been reset.
        /// </summary>
        public virtual long StatsReset
        {
            get { return m_statsReset; }
            set { m_statsReset = value; }
        }

        /// <summary>
        /// Statistics: total number of messages sent over this Connection.
        /// </summary>
        public virtual long StatsSent
        {
            get { return m_statsSent; }
            set { m_statsSent = value; }
        }

        /// <summary>
        /// A <see cref="Gate"/> used to prevent concurrent use of this
        /// Connection while it is being opened and closed.
        /// </summary>
        /// <value>
        /// A <b>Gate</b> used to prevent concurrent use of this
        /// Connection while it is being opened and closed.
        /// </value>
        public Gate Gate
        {
            get; set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize channel maps and create new Connection.
        /// </summary>
        public Connection()
        {
            Channels        = new LongSortedList();
            ChannelsPending = new LongSortedList();
            Gate            = GateFactory.NewGate;

            // OnInit
            // create and register "Channel0"
            Channel channel0    = new Channel {Connection = this};
            RegisterChannel(channel0);
        }

        #endregion

        #region IConnection implementation

        /// <summary>
        /// Close the Connection.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Closing a Connection also reclaims all resources held by the
        /// Connection, so there is no need to close <b>IChannel</b> objects
        /// of a closed Connection.</p>
        /// <p>
        /// If the Connection is already closed, calling this method has no
        /// effect.</p>
        /// </remarks>
        /// <seealso cref="IConnection.Close"/>
        public virtual void Close()
        {
            Close(true, null);
        }

        /// <summary>
        /// Close the Connection.
        /// </summary>
        /// <param name="notify">
        /// If <b>true</b>, notify the peer that the Connection is being
        /// closed.
        /// </param>
        /// <param name="e">
        /// The optional reason why the Connection is being closed.
        /// </param>
        public virtual void Close(bool notify, Exception e)
        {
            Close(notify, e, true);
        }

        /// <summary>
        /// Close the Connection.
        /// </summary>
        /// <param name="notify">
        /// If <b>true</b>, notify the peer that the Connection is being
        /// closed.
        /// </param>
        /// <param name="e">
        /// The optional reason why the Connection is being closed.
        /// </param>
        /// <param name="wait">
        /// if true wait for the ConnectionManager to close the Connection
        /// when called on a client thread. This method will always wait
        /// for the ConnectionManager to close the Connection if called on
        /// the service thread.
        /// </param>
        public virtual void Close(bool notify, Exception e, bool wait)
        {
            if (IsOpen)
            {
                Peer manager = (Peer) ConnectionManager;
                if (Thread.CurrentThread == manager.Thread)
                {
                    CloseInternal(notify, e, 0);
                }
                else
                {
                    Debug.Assert(!IsActiveThread,
                                 "cannot close a connection while executing within the connection");

                    manager.CloseConnection(this, notify, e, wait);
                }
            }
        }

        /// <summary>
        /// The <see cref="Close()"/> implementation method.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        /// <param name="notify">
        /// if <b>true</b>, notify the peer that the Connection is being
        /// closed.
        /// </param>
        /// <param name="e">
        /// The optional reason why the Connection is being closed.
        /// </param>
        /// <param name="millis">
        /// The number of milliseconds to wait for the Connection to close;
        /// pass 0 to perform a non-blocking close or -1 to wait forever.
        /// </param>
        /// <returns>
        /// <b>true</b> if the invocation of this method closed the
        /// Connection.
        /// </returns>
        public virtual bool CloseInternal(bool notify, Exception e, int millis)
        {
            if (!IsOpen)
            {
                return false;
            }

            // close all open Channels, except for "Channel0"
            Channel    channel0;
            ILongArray channels = Channels;
            lock (channels.SyncRoot)
            {
                channel0 = (Channel) channels[0];

                IList channelList = new ArrayList();
                foreach (DictionaryEntry entry in channels)
                {
                    channelList.Add(entry.Value);
                }
                foreach (Channel channel in channelList)
                {
                    if (channel != channel0)
                    {
                        channel.CloseInternal(false, e, 0);
                    }
                }

                channels.Clear();
                channels[0] = channel0;
            }

            bool isClosed = GateClose(millis);
            try
            {
                if (!isClosed)
                {
                    // can't close the gate; signal to the holding Thread(s)
                    // that it must close the Connection immediately after
                    // exiting the gate
                    CloseOnExit    = true;
                    CloseNotify    = notify;
                    CloseException = e;

                    // double check if we can close the gate, as we want to
                    // be sure that the Thread(s) saw the close notification
                    // prior to exiting
                    isClosed = GateClose(0);
                }

                if (isClosed && IsOpen)
                {
                    // notify the peer that the Connection is now closed
                    if (notify)
                    {
                        // send a NotifyConnectionClosed to the peer via
                        // "Channel0"
                        try
                        {
                            IMessageFactory        factory0 = channel0.MessageFactory;
                            NotifyConnectionClosed message  = (NotifyConnectionClosed) factory0.CreateMessage(NotifyConnectionClosed.TYPE_ID);

                            message.Cause = e;
                            channel0.Send(message);
                        }
                        catch
                        {}
                    }

                    // clean up
                    channel0.CloseInternal(false, e, -1);
                    ChannelsPending.Clear();
                    PeerId = null;

                    IsOpen = false;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                if (isClosed)
                {
                    GateOpen();
                }
            }

            // notify the ConnectionManager that the Connection is now closed
            Peer manager = ConnectionManager as Peer;
            if (e == null)
            {
                manager.OnConnectionClosed(this);
            }
            else
            {
                manager.OnConnectionError(this, e);
            }

            return true;
        }

        /// <summary>
        /// Create an <b>IChannel</b> using a specific <b>IProtocol</b>
        /// through this IConnection to a named <b>IReceiver</b> on the other
        /// end of the IConnection, optionally providing an <b>IPrincipal</b>
        /// to indicate the identity that will be utilizing the
        /// <b>IChannel</b>, and optionally providing an <b>IReceiver</b>
        /// that will process unsolicited <b>IMessage</b> objects on this end
        /// of the <b>IChannel</b>.
        /// </summary>
        /// <remarks>
        /// Conceptually, this is how an <b>IChannel</b> is established to an
        /// existing named "service" (e.g. an <b>IReceiver</b>) on the peer;
        /// note that either peer can register named services and either peer
        /// can use this method to find a named service on its peer.
        /// </remarks>
        /// <param name="protocol">
        /// The <b>IProtocol</b> that will be used to communicate through the
        /// <b>IChannel</b>; the <b>IProtocol</b> is used to verify that the
        /// <b>IReceiver</b> on the peer with the specified name is capable
        /// of communicating using that <b>IProtocol</b>.
        /// </param>
        /// <param name="name">
        /// The name that the <b>IReceiver</b> was registered with, on the
        /// other end of this IConnection; an <b>IReceiver</b> with the
        /// specified name must have been registered with the peer's
        /// <b>IConnectionManager</b> prior to calling this method (see
        /// <see cref="IConnectionManager.RegisterReceiver(IReceiver)"/>).
        /// </param>
        /// <param name="receiver">
        /// An optional <b>IReceiver</b> to associate with this
        /// <b>IChannel</b> that will process any unsolicited <b>IMessage</b>
        /// objects sent back through the <b>IChannel</b> by the peer.
        /// </param>
        /// <param name="principal">
        /// An optional <b>IPrincipal</b> to associate with this
        /// <b>IChannel</b>; if specified, any operation performed upon
        /// receipt of an <b>IMessage</b> sent using the returned
        /// <b>IChannel</b> will be done on behalf of the specified
        /// <b>IPrincipal</b>.
        /// </param>
        /// <returns>
        /// A new <b>IChannel</b> object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IProtocol</b> has not been registered with
        /// the underlying <b>IConnectionManager</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If an <b>IReceiver</b> with the given name has not been
        /// registered with the peer's <b>IConnectionManager</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IReceiver</b> does not use the same
        /// <b>IProtocol</b> as the one registered on the peer.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IReceiver</b> does not use the specified
        /// <b>IProtocol</b>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If the IConnection is not open.
        /// </exception>
        /// <seealso cref="IConnection.OpenChannel"/>
        public virtual IChannel OpenChannel(IProtocol protocol, string name, IReceiver receiver, IPrincipal principal)
        {
            return ((Peer) ConnectionManager).OpenChannel(this, protocol, name, receiver, principal);
        }

        /// <summary>
        /// The <see cref="OpenChannel"/> implementation method.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        /// <param name="protocol">
        /// The <b>IProtocol</b> that will be used to communicate through the
        /// <b>IChannel</b>; the <b>IProtocol</b> is used to verify that the
        /// <b>IReceiver</b> on the peer with the specified name is capable
        /// of communicating using that <b>IProtocol</b>.
        /// </param>
        /// <param name="name">
        /// The name that the <b>IReceiver</b> was registered with, on the
        /// other end of this IConnection; an <b>IReceiver</b> with the
        /// specified name must have been registered with the peer's
        /// <b>IConnectionManager</b> prior to calling this method (see
        /// <see cref="IConnectionManager.RegisterReceiver(IReceiver)"/>).
        /// </param>
        /// <param name="serializer">
        /// An optional <b>ISerializer</b> to associate with this
        /// <b>IChannel</b>.
        /// </param>
        /// <param name="receiver">
        /// An optional <b>IReceiver</b> to associate with this
        /// <b>IChannel</b> that will process any unsolicited <b>IMessage</b>
        /// objects sent back through the <b>IChannel</b> by the peer.
        /// </param>
        /// <param name="principal">
        /// An optional <b>IPrincipal</b> to associate with this
        /// <b>IChannel</b>; if specified, any operation performed upon
        /// receipt of an <b>IMessage</b> sent using the returned
        /// <b>IChannel</b> will be done on behalf of the specified
        /// <b>IPrincipal</b>.
        /// </param>
        /// <param name="identityToken">
        /// An optional token representing <b>IPrincipal</b> to associate
        /// with this <b>IChannel</b>.
        /// </param>
        /// <returns></returns>
        public virtual IStatus OpenChannelInternal(IProtocol protocol, string name, ISerializer serializer,
                                                   IReceiver receiver, IPrincipal principal, byte[] identityToken)
        {
            AssertOpen();

            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            string protocolName = protocol.Name;
            Debug.Assert(protocolName != null);

            IMessageFactory factory = MessageFactoryMap[protocolName] as IMessageFactory;
            if (factory == null)
            {
                throw new ArgumentException("unknown protocol: " + protocolName);
            }

            if (receiver != null)
            {
                if (receiver.Protocol != factory.Protocol)
                {
                    throw new ArgumentException("protocol mismatch; expected "
                                                + factory.Protocol + ", retrieved "
                                                + receiver.Protocol + ")");
                }
            }

            // send a ChannelOpenRequest to the peer via "Channel0"
            Channel            channel0 = GetChannel(0) as Channel;
            IMessageFactory    factory0 = channel0.MessageFactory;
            OpenChannelRequest request  = factory0.CreateMessage(
                    Util.Daemon.QueueProcessor.Service.Peer.
                            OpenChannelRequest.TYPE_ID) as OpenChannelRequest;

            request.IdentityToken  = identityToken;
            request.MessageFactory = factory;
            request.ProtocolName   = protocolName;
            request.Receiver       = receiver;
            request.ReceiverName   = name;
            request.Serializer     = serializer;
            request.Principal      = principal;

            return channel0.Send(request);
        }

        /// <summary>
        /// The <see cref="OpenChannel"/> recipient implementation method.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread in response to a
        /// <b>ChannelOpenRequest</b>.
        /// </remarks>
        /// <param name="protocol">
        /// The <b>IProtocol</b> that will be used to communicate through the
        /// <b>IChannel</b>; the <b>IProtocol</b> is used to verify that the
        /// <b>IReceiver</b> on the peer with the specified name is capable
        /// of communicating using that <b>IProtocol</b>.
        /// </param>
        /// <param name="serializer">
        /// An optional <b>ISerializer</b> to associate with this
        /// <b>IChannel</b>.
        /// </param>
        /// <param name="receiver">
        /// An optional <b>IReceiver</b> to associate with this
        /// <b>IChannel</b> that will process any unsolicited <b>IMessage</b>
        /// objects sent back through the <b>IChannel</b> by the peer.
        /// </param>
        /// <param name="principal">
        /// An optional <b>IPrincipal</b> to associate with this
        /// <b>IChannel</b>; if specified, any operation performed upon
        /// receipt of an <b>IMessage</b> sent using the returned
        /// <b>IChannel</b> will be done on behalf of the specified
        /// <b>IPrincipal</b>.
        /// </param>
        /// <returns>
        /// The identifier of the newly opened Channel.
        /// </returns>
        public virtual int OpenChannelRequest(String protocol, ISerializer serializer, IReceiver receiver, IPrincipal principal)
        {
            AssertOpen();

            if (protocol == null)
            {
                throw new ArgumentNullException("protocol name cannot be null");
            }

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer cannot be null");
            }

            IMessageFactory factory = (IMessageFactory) MessageFactoryMap[protocol];
            if (factory == null)
            {
                throw new ArgumentException("unknown protocol: " + protocol);
            }

            if (receiver != null)
            {
                if (receiver.Protocol != factory.Protocol)
                {
                    throw new ArgumentException("protocol mismatch; expected "
                                                + factory.Protocol + ", retrieved "
                                                + receiver.Protocol + ')');
                }
            }

            int     id      = GenerateChannelId();
            Channel channel = new Channel
                             {
                                     Connection = this,
                                     Id = id,
                                     MessageFactory = factory,
                                     Receiver = receiver,
                                     Serializer = serializer,
                                     Principal = principal
                             };
            channel.OpenInternal();

            RegisterChannel(channel);

            return id;
        }

        /// <summary>
        /// The <see cref="OpenChannel"/> initiator implementation method.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread in response to a
        /// <b>ChannelOpenResponse</b>.
        /// </remarks>
        /// <param name="id">
        /// Id of the new Channel.
        /// </param>
        /// <param name="factory">
        /// An optional <b>IMessageFactory</b> to associate with this
        /// <b>IChannel</b>.
        /// </param>
        /// <param name="serializer">
        /// An optional <b>ISerializer</b> to associate with this
        /// <b>IChannel</b>.
        /// </param>
        /// <param name="receiver">
        /// An optional <b>IReceiver</b> to associate with this
        /// <b>IChannel</b> that will process any unsolicited <b>IMessage</b>
        /// objects sent back through the <b>IChannel</b> by the peer.
        /// </param>
        /// <param name="principal">
        /// An optional <b>IPrincipal</b> to associate with this
        /// <b>IChannel</b>; if specified, any operation performed upon
        /// receipt of an <b>IMessage</b> sent using the returned
        /// <b>IChannel</b> will be done on behalf of the specified
        /// <b>IPrincipal</b>.
        /// </param>
        /// <returns></returns>
        public virtual IChannel OpenChannelResponse(int id, IMessageFactory factory, ISerializer serializer, IReceiver receiver, IPrincipal principal)
        {
            AssertOpen();

            if (factory == null)
            {
                throw new ArgumentNullException("factory cannot be null");
            }

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer cannot be null");
            }

            Channel channel        = new Channel();
            channel.Id             = id;
            channel.Connection     = this;
            channel.MessageFactory = factory;
            channel.Receiver       = receiver;
            channel.Serializer     = serializer;
            channel.Principal      = principal;
            channel.OpenInternal();

            RegisterChannel(channel);

            return channel;
        }

        /// <summary>
        /// The <see cref="Open()"/> implementation method.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        public virtual void OpenInternal()
        {
            if (IsOpen)
            {
                return;
            }

            Peer manager = ConnectionManager as Peer;
            Debug.Assert(manager != null);

            // make sure the ConnectionManager has the MessagingProtocol
            MessagingProtocol protocol =  manager.GetProtocol(MessagingProtocol.PROTOCOL_NAME) as MessagingProtocol;
            Debug.Assert(protocol != null);

            // look up the appropriate MessagingProtocol MessageFactory
            IMessageFactory factory = protocol.GetMessageFactory(protocol.CurrentVersion);

            // open "Channel0"
            Channel channel0        = GetChannel(0) as Channel;
            channel0.MessageFactory = factory;
            channel0.Receiver       = manager;
            channel0.Serializer     = manager.EnsureSerializer();
            channel0.OpenInternal();

            IsOpen = true;

            // note that we do not notify the ConnectionManager that the
            // Connection has opened just yet; the Connection still needs
            // to be connected or accepted (See ConnectionOpenRequest and
            // ConnectionOpenResponse)
        }

        /// <summary>
        /// Send a <b>PingRequest</b> via "Channel0" and update the
        /// <b>PingLastMillis</b> property.
        /// </summary>
        /// <remarks>
        /// This method will only send a <b>PingRequest</b> if one is not
        /// already outstanding.
        /// </remarks>
        /// <returns>
        /// <b>true</b> if a <b>PingRequest</b> was sent.
        /// </returns>
        public virtual bool Ping()
        {
            if (PingLastMillis == 0)
            {
                Channel         channel0 = GetChannel(0) as Channel;
                IMessageFactory factory  = channel0.MessageFactory;
                PingRequest     request  = factory.CreateMessage(PingRequest.TYPE_ID) as PingRequest;

                try
                {
                    channel0.Send(request);
                }
                catch (Exception)
                {
                    return false;
                }

                PingLastMillis = DateTimeUtils.GetSafeTimeMillis();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Create a back-Channel to expose another service to the peer.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method is particularly useful for building a Response
        /// Message to send back a new <b>IChannel</b> that can be used by
        /// the peer. In practice, this means that when a call to a stub is
        /// made, it can easily return a new stub that has its own
        /// <b>IChannel</b>; for example, a stub representing one service can
        /// return a stub representing a different service.</p>
        /// <p>
        /// The new <b>IChannel</b> cannot be used until the returned
        /// <b>Uri</b> is
        /// <see cref="AcceptChannel"/> accepted by the peer.</p>
        /// </remarks>
        /// <param name="protocol">
        /// The <b>IProtocol</b> that will be used to communicate through the
        /// new <b>IChannel</b>.
        /// </param>
        /// <param name="receiver">
        /// An optional <b>IReceiver</b> to associate with the new
        /// <b>IChannel</b> that will process any unsolicited <b>IMessage</b>
        /// objects sent back through the <b>IChannel</b> by the peer.
        /// </param>
        /// <returns>
        /// A <b>Uri</b> that represents the new <b>IChannel</b> object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IProtocol</b> has not been registered with
        /// the underlying <b>IConnectionManager</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IReceiver</b> does not use the specified
        /// <b>IProtocol</b>.
        /// </exception>
        /// <exception cref="ConnectionException">
        /// If the Connection is not open.
        /// </exception>
        /// <seealso cref="AcceptChannel"/>
        public virtual Uri CreateChannel(IProtocol protocol, IReceiver receiver)
        {
            Peer manager = ConnectionManager as Peer;
            if (Thread.CurrentThread == manager.Thread)
            {
                return CreateChannelInternal(protocol,
                        manager.EnsureSerializer(),
                        receiver);
            }
            return manager.CreateChannel(this, protocol, receiver);
        }

        /// <summary>
        /// The <see cref="CreateChannel"/> implementation method.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method is called on the service thread.
        /// </p>
        /// </remarks>
        /// <param name="protocol">
        /// The <b>IProtocol</b> that will be used to communicate through the
        /// new <b>IChannel</b>.
        /// </param>
        /// <param name="serializer">
        /// </param>
        /// <param name="receiver">
        /// An optional <b>IReceiver</b> to associate with the new
        /// <b>IChannel</b> that will process any unsolicited <b>IMessage</b>
        /// objects sent back through the <b>IChannel</b> by the peer.
        /// </param>
        /// <returns>
        /// A <b>Uri</b> that represents the new <b>IChannel</b> object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IProtocol</b> has not been registered with
        /// the underlying <b>IConnectionManager</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IReceiver</b> does not use the specified
        /// <b>IProtocol</b>.
        /// </exception>
        /// <exception cref="ConnectionException">
        /// If the Connection is not open.
        /// </exception>
        /// <seealso cref="AcceptChannel"/>
        public virtual Uri CreateChannelInternal(IProtocol protocol, ISerializer serializer, IReceiver receiver)
        {
            AssertOpen();

            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            string protocolName = protocol.Name;
            if (protocolName == null)
            {
                throw new ArgumentException("missing protocol name: " + protocol);
            }

            IMessageFactory factory = MessageFactoryMap[protocol] as IMessageFactory;
            if (factory == null)
            {
                throw new ArgumentException("unsupported protocol: " + protocol);
            }

            int     id             = GenerateChannelId();
            Channel channel        = new Channel();
            channel.Id             = id;
            channel.Connection     = this;
            channel.Receiver       = receiver;
            channel.MessageFactory = factory;
            channel.Serializer     = serializer;

            // add the new Channel to the pending array; log a warning if the 
            // the number of pending channels is high.
            ILongArray channelsPending = ChannelsPending;
            int        size            = channelsPending.Count;

            if (size > MAX_PENDING_CHANNELS)
            {
                CacheFactory.Log("There is a high number of pending open channel requests[" + size
                                 + "] for connection=" + this, CacheFactory.LogLevel.Warn);
            }
            channelsPending[id] = channel;

            try
            {
                string uri = "channel:" + id + "#" + protocolName;
                return new Uri(uri);
            }
            catch (UriFormatException e)
            {
                channelsPending.Remove(channel.Id);
                throw new Exception("error creating Uri", e);
            }
        }

        /// <summary>
        /// Attempt to close the Connection <b>Gate</b>.
        /// </summary>
        /// <param name="millis">
        /// The number of milliseconds to wait for the <b>Gate</b> to
        /// close; pass 0 to perform a non-blocking close or -1 to wait
        /// forever.
        /// </param>
        /// <returns>
        /// <b>true</b> if the Connection Gate was closed;
        /// <b>false</b> otherwise.
        /// </returns>
        protected virtual bool GateClose(int millis)
        {
            bool closed = false;
            try
            {
               closed = Gate.Close(millis);
            }
            catch (System.Threading.LockRecursionException) {}
            return closed;
        }

        /// <summary>
        /// Enter the Connection <b>Gate</b>.
        /// </summary>
        /// <exception cref="ConnectionException">
        /// If the Connection is closing or closed.
        /// </exception>
        public virtual void GateEnter()
        {
            Gate gate = Gate;

            // if the thread is entering for the first time, throw an
            // exception if the Connection has been marked for close;
            // this prevents new threads from entering the Connection
            // and thus keeping it open longer than necessary
            if (CloseOnExit && !gate.IsEnteredByCurrentThread)
            {
                // REVIEW
                throw new ConnectionException("connection is closing", this);
            }

            if (gate.Enter(0)) // see #GateClose
            {
                try
                {
                    AssertOpen();
                }
                catch (Exception)
                {
                    gate.Exit();
                    throw;
                }
            }
            else
            {
                // REVIEW
                throw new ConnectionException("connection is closing", this);
            }
        }

        /// <summary>
        /// Exit the Connection <b>Gate</b>.
        /// </summary>
        public virtual void GateExit()
        {
            Gate gate = Gate;
            gate.Exit();

            // see if we've been asked to close the Connection
            if (CloseOnExit && !gate.IsEnteredByCurrentThread)
            {
                bool isClosed = GateClose(0);
                try
                {
                    if (isClosed && IsOpen)
                    {
                        GateOpen();
                        isClosed = false;
                        Close(CloseNotify, CloseException);
                    }
                }
                finally
                {
                    if (isClosed)
                    {
                        GateOpen();
                    }
                }
            }
        }

        /// <summary>
        /// Open the Connection <b>Gate</b>.
        /// </summary>
        protected void GateOpen()
        {
            Gate.Open();
        }

        /// <summary>
        /// Accept a newly created back-Channel that was spawned by the peer.
        /// </summary>
        /// <remarks>
        /// Before a spawned <b>IChannel</b> can be used to send and receive
        /// <b>IMessage</b> objects, its <b>Uri</b> must be accepted by the
        /// peer.
        /// </remarks>
        /// <param name="uri">
        /// The <b>Uri</b> of an <b>IChannel</b> that was spawned by the
        /// peer.
        /// </param>
        /// <param name="receiver">
        /// An optional <b>IReceiver</b> to associate with the new
        /// <b>IChannel</b> that will process any unsolicited <b>IMessage</b>
        /// objects sent back through the <b>IChannel</b> by the peer.
        /// </param>
        /// <param name="principal">
        /// An optional <b>IPrincipal</b> to associate with the new
        /// <b>IChannel</b>; if specified, any operation performed upon
        /// receipt of an <b>IMessage</b> sent using the accepted
        /// <b>IChannel</b> will be done on behalf of the specified
        /// <b>IPrincipal</b>.
        /// </param>
        /// <returns>
        /// The newly accepted <b>IChannel</b>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IReceiver</b> does not use the same
        /// <b>IProtocol</b> as the spawned <b>IChannel</b> (as described by
        /// its <b>Uri</b>).
        /// </exception>
        /// <exception cref="ConnectionException">
        /// If the Connection is not open.
        /// </exception>
        /// <seealso cref="CreateChannel(IProtocol, IReceiver)"/>
        public virtual IChannel AcceptChannel(Uri uri, IReceiver receiver, IPrincipal principal)
        {
            return ((Peer) ConnectionManager).AcceptChannel(this, uri, receiver, principal);
        }

        /// <summary>
        /// The <see cref="AcceptChannel"/> implementation method.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        /// <param name="uri">
        /// The <b>Uri</b> of an <b>IChannel</b> that was spawned by the
        /// peer.
        /// </param>
        /// <param name="serializer"></param>
        /// <param name="receiver">
        /// An optional <b>IReceiver</b> to associate with the new
        /// <b>IChannel</b> that will process any unsolicited <b>IMessage</b>
        /// objects sent back through the <b>IChannel</b> by the peer.
        /// </param>
        /// <param name="principal">
        /// An optional <b>IPrincipal</b> to associate with the new
        /// <b>IChannel</b>; if specified, any operation performed upon
        /// receipt of an <b>IMessage</b> sent using the accepted
        /// <b>IChannel</b> will be done on behalf of the specified
        /// <b>IPrincipal</b>.
        /// </param>
        /// <param name="identityToken">
        /// An optional token representing an <b>IPrincipal</b> to
        /// associate with the <b>Channel</b>.
        /// </param>
        /// <returns>
        /// An <see cref="IStatus"/> object representing the asynchronous
        /// <b>IRequest</b>.
        /// </returns>
        public virtual IStatus AcceptChannelInternal(Uri uri, ISerializer serializer, IReceiver receiver, IPrincipal principal, byte[] identityToken)
        {
            AssertOpen();

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            int id;
            try
            {
                id = Int32.Parse(UriUtils.GetSchemeSpecificPart(uri));
            }
            catch (Exception)
            {
                throw new ArgumentException("illegal URI: " + uri);
            }

            if (id == 0)
            {
                throw new ArgumentException("channel 0 is reserved");
            }

            if (GetChannel(id) != null)
            {
                throw new InvalidOperationException("duplicate channel: " + id);
            }

            if (uri.Fragment == null)
            {
                throw new ArgumentException("illegal URI: " + uri);
            }

            string protocol = uri.Fragment.Substring(1);
            if (StringUtils.IsNullOrEmpty(protocol))
            {
                throw new ArgumentException("illegal URI: " + uri);
            }

            IMessageFactory factory = MessageFactoryMap[protocol] as IMessageFactory;
            if (factory == null)
            {
                throw new ArgumentException("unknown protocol: " + protocol);
            }

            if (receiver != null)
            {
                if (receiver.Protocol != factory.Protocol)
                {
                    throw new ArgumentException("protocol mismatch; expected "
                                                + factory.Protocol + ", retrieved "
                                                + receiver.Protocol + ")");
                }
            }

            // send a AcceptChannelRequest to the peer via "Channel0"
            Channel              channel0 =  GetChannel(0) as Channel;
            IMessageFactory      factory0 = channel0.MessageFactory;
            AcceptChannelRequest request = factory0.CreateMessage(Util.Daemon.QueueProcessor.Service.Peer.AcceptChannelRequest.TYPE_ID) as AcceptChannelRequest;

            request.ChannelId      = id;
            request.IdentityToken  = identityToken;
            request.MessageFactory = factory;
            request.ProtocolName   = protocol;
            request.Receiver       = receiver;
            request.Serializer     = serializer;
            request.Principal      = principal;

            return channel0.Send(request);
        }

        /// <summary>
        /// The <see cref="AcceptChannel"/> recipient implementation method.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread in response to a 
        /// <b>ChannelAcceptRequest</b>.
        /// </remarks>
        /// <param name="id">
        /// Id of the new <b>IChannel</b>.
        /// </param>
        /// <param name="principal">
        /// An optional <b>IPrincipal</b> to associate with the new
        /// <b>IChannel</b>; if specified, any operation performed upon
        /// receipt of an <b>IMessage</b> sent using the accepted
        /// <b>IChannel</b> will be done on behalf of the specified
        /// <b>IPrincipal</b>.
        /// </param>
        public virtual void AcceptChannelRequest(int id, IPrincipal principal)
        {
            if (id == 0)
            {
                throw new ArgumentException("channel 0 is reserved");
            }

            if (GetChannel(id) != null)
            {
                throw new ArgumentException("channel already exists: " + id);
            }

            Channel channel = (Channel) ChannelsPending.Remove(id);
            if (channel == null)
            {
                throw new ArgumentException("no such channel: " + id);
            }

            channel.Principal = principal;
            channel.OpenInternal();

            RegisterChannel(channel);
        }

        /// <summary>
        /// The <see cref="AcceptChannel"/> initiator implementation method.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread in response to a
        /// <b>ChannelAcceptResponse</b>.
        /// </remarks>
        /// <param name="id">
        /// Id of the new <b>IChannel</b>.
        /// </param>
        /// <param name="factory">
        /// An optional <b>IMessageFactory</b> to associate with the new
        /// Channel.
        /// </param>
        /// <param name="serializer">
        /// An optional <b>ISerializer</b> to associate with the new
        /// Channel.
        /// </param>
        /// <param name="receiver">
        /// An optional <b>IReceiver</b> to associate with the new Channel
        /// that will process any unsolicited <b>IMessage</b> objects sent
        /// back through the <b>Channel</b> by the peer.
        /// </param>
        /// <param name="principal">
        /// An optional <b>IPrincipal</b> to associate with the new Channel;
        /// If specified, any operation performed upon receipt of a
        /// <b>IMessage</b> sent using the accepted Channel will be done
        /// on behalf of the specified <b>IPrincipal</b>.
        /// </param>
        /// <returns>
        /// The newly accepted Channel.
        /// </returns>
        public virtual IChannel AcceptChannelResponse(int id, IMessageFactory factory, ISerializer serializer,
                                                      IReceiver receiver, IPrincipal principal)
        {
            AssertOpen();

            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            Channel channel = new Channel
                             {
                                 Id = id,
                                 Connection = this,
                                 MessageFactory = factory,
                                 Receiver = receiver,
                                 Serializer = serializer,
                                 Principal = principal
                             };
            channel.OpenInternal();

            RegisterChannel(channel);

            return channel;
        }

        /// <summary>
        /// Asserts if the Connection is open.
        /// </summary>
        /// <exception cref="ConnectionException">
        /// If the Connection is closed or closing.
        /// </exception>
        protected virtual void AssertOpen()
        {
            if (!IsOpen)
            {
                // REVIEW
                throw new ConnectionException("connection is closed", this);
            }
        }

        /// <summary>
        /// Return the open <b>IChannel</b> object with the given identifier.
        /// </summary>
        /// <remarks>
        /// If an <b>IChannel</b> object with the specified identifier does
        /// not exist or has been closed, <c>null</c> is returned.
        /// </remarks>
        /// <param name="id">
        /// The unique <b>IChannel</b> identifier.
        /// </param>
        /// <returns>
        /// The open <b>IChannel</b> object with the specified identifer or
        /// <c>null</c> if no such open <b>IChannel</b> exists.
        /// </returns>
        /// <seealso cref="IConnection.GetChannel"/>
        public virtual IChannel GetChannel(int id)
        {
            ILongArray channels = Channels;
            
            // avoid synchronization if possible; see Peer.DecodeMessage
            if (((Peer) ConnectionManager).IsServiceThread(false))
            {
                return (Channel) channels[id];
            }

            lock (channels.SyncRoot)
            {
                return (IChannel) channels[id];
            }
        }

        /// <summary>
        /// Return the collection of open <b>IChannel</b> objects through
        /// this Connection.
        /// </summary>
        /// <remarks>
        /// The client should assume that the returned collection is an
        /// immutable snapshot of the actual collection of open
        /// <b>IChannel</b> objects maintained by this Connection.
        /// </remarks>
        /// <returns>
        /// The collection of open <b>IChannel</b> objects.
        /// </returns>
        /// <seealso cref="IConnection.GetChannels"/>
        public virtual ICollection GetChannels()
        {
            ILongArray channels = Channels;
            ArrayList  list     = new ArrayList();

            lock (channels.SyncRoot)
            {
                foreach(DictionaryEntry entry in channels)
                {
                    list.Add(entry.Value);
                }
            }

            return list;
        }

        #endregion

        #region Extend override methods

        /// <summary>
        /// Return a human-readable description of this Connection.
        /// </summary>
        /// <returns>
        /// A string representation of this Connection.
        /// </returns>
        /// <since>Coherence 3.7</since>
        protected override string GetDescription()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Id="    ).Append(Id)
              .Append(", Open=").Append(IsOpen);

            IMember member = Member;
            if (member != null)
            {
                sb.Append(", ").Append(member);
            }

            return sb.ToString();
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Generate a new unique <b>IChannel</b> identifier.
        /// </summary>
        /// <remarks>
        /// If the <b>IConnectionManager</b> that created this
        /// <b>IChannel</b> is an <b>IConnectionAcceptor</b>, the returned
        /// value will be in the range:
        /// [-Int32.MaxValue, 0).
        /// If the <b>IConnectionManager</b> that created this
        /// <b>IChannel</b> is an <b>IConnectionInitiator</b>, the returned
        /// value will be in the range:
        /// (0, Int32.MaxValue).
        /// The space of identifiers must be partitioned in order to prevent
        /// collisions.
        /// </remarks>
        /// <returns>
        /// A new unique <b>IChannel</b> identifier.
        /// </returns>
        protected virtual int GenerateChannelId()
        {
            ILongArray channelsPending = ChannelsPending;
            int        scale           = ConnectionManager is IConnectionAcceptor ? 1 : -1;
            int        id;

            do
            {
                id = NumberUtils.GetRandom().Next(Int32.MaxValue) * scale;
            }
            while (id == 0 || GetChannel(id) != null || channelsPending[id] != null);

            return id;
        }

        /// <summary>
        /// Open this Connection.
        /// </summary>
        public virtual void Open()
        {
            if (!IsOpen)
            {
                ((Peer) ConnectionManager).OpenConnection(this);
            }
        }

        /// <summary>
        /// Register the given <b>IChannel</b> in the map of channels.
        /// </summary>
        /// <param name="channel">
        /// The <b>IChannel</b> to register; must not be <c>null</c>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the <b>IChannel</b> has already been registered.
        /// </exception>
        protected void RegisterChannel(IChannel channel)
        {
            ILongArray channels = Channels;
            lock (channels.SyncRoot)
            {
                if (channels.Exists(channel.Id))
                {
                    throw new ArgumentException("duplicate channel: " + channel);
                }
                channels[channel.Id] = channel;
            }
        }

        /// <summary>
        /// Reset the Connection statistics.
        /// </summary>
        public virtual void ResetStats()
        {
            StatsBytesReceived = 0L;
            StatsBytesSent     = 0L;
            StatsReceived      = 0L;
            StatsSent          = 0L;
            StatsReset         = DateTimeUtils.GetSafeTimeMillis();
        }

        /// <summary>
        /// Send the given <b>DataWriter</b> through this Connection.
        /// </summary>
        /// <param name="writer">
        /// The <b>DataWriter</b> to send.
        /// </param>
        public virtual void Send(DataWriter writer)
        {
            AssertOpen();

            // update stats
            StatsBytesSent += writer.BaseStream.Length;
            StatsSent      += 1;
        }

        /// <summary>
        /// Unregister the given <b>IChannel</b> from the array of
        /// <b>IChannel</b>s.
        /// </summary>
        /// <param name="channel">
        /// The <b>IChannel</b> to unregister; must not be <c>null</c>.
        /// </param>
        public virtual void UnregisterChannel(IChannel channel)
        {
            if (channel.Id == 0)
            {
                // never unregister "Channel0"
                return;
            }

            ILongArray channels = Channels;
            lock (channels.SyncRoot)
            {
                channels.Remove(channel.Id);
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// The maximum number of pending new IChannel objects. If the limit
        /// is reached, a pending IChannel will be discarded.
        /// </summary>
        private const int MAX_PENDING_CHANNELS = 100;

        #endregion

        #region Data Members

        /// <summary>
        /// Peer notification flag used when the Connection is closed upon
        /// exiting the <b>Gate</b> (see <see cref="CloseOnExit"/>
        /// property).
        /// </summary>
        private volatile bool m_closeNotify;

        /// <summary>
        /// If <b>true</b>, the <b>Thread</b> that is currently executing
        /// within the Connection should close the Connection immedately
        /// upon exiting the Connection's <b>Gate</b>.
        /// </summary>
        private volatile bool m_closeOnExit;

        /// <summary>
        /// The <b>Exception</b> to pass to the <see cref="Close()"/> method
        /// when the Connection is closed upon exiting the Gate
        /// (see <see cref="CloseOnExit"/> property).
        /// </summary>
        private volatile Exception m_closeException;

        /// <summary>
        /// The ICodec used by the Connection to convert IMessage objects to
        /// and from a binary representation.
        /// </summary>
        private ICodec m_codec;

        /// <summary>
        /// The IConnectionManager that created or accepted this Connection.
        /// </summary>
        private volatile IConnectionManager m_connectionManager;

        /// <summary>
        /// The unique identifier of the process using this Connection
        /// object.
        /// </summary>
        private UUID m_id;

        /// <summary>
        /// The IMember object associated with this Connection.
        /// </summary>
        private IMember m_member;

        /// <summary>
        /// A dictionary of <b>IMessageFactory</b> objects that may be used
        /// by <b>IChannel</b> objects created by this Connection, keyed by 
        /// <b>IProtocol</b> name.
        /// </summary>
        [NonSerialized]
        private IDictionary m_messageFactoryMap;

        /// <summary>
        /// Determine if the Connection is open.
        /// </summary>
        private volatile bool m_isOpen;

        /// <summary>
        /// The send time of the last outstanding <b>PingRequest</b> or 0 if
        /// a <b>PingRequest</b> is not outstanding.
        /// </summary>
        [NonSerialized]
        private long m_pingLastMillis;

        /// <summary>
        /// Statistics: total number of bytes received over this Connection.
        /// </summary>
        [NonSerialized]
        private long m_statsBytesReceived;

        /// <summary>
        /// Statistics: total number of bytes sent over this Connection.
        /// </summary>
        [NonSerialized]
        private long m_statsBytesSent;

        /// <summary>
        /// Statistics: total number of <b>IMessages</b> received over this
        /// Connection.
        /// </summary>
        [NonSerialized]
        private long m_statsReceived;

        /// <summary>
        /// Statistics: date/time value that the stats have been reset.
        /// </summary>
        [NonSerialized]
        private long m_statsReset;

        /// <summary>
        /// Statistics: total number of messages sent over this Connection.
        /// </summary>
        [NonSerialized]
        private long m_statsSent;

        #endregion
    }
}