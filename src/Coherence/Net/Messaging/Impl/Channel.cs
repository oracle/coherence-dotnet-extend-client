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
using System.Security.Principal;
using System.Text;
using System.Threading;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Impl;
using Tangosol.Util;
using Tangosol.Util.Collections;
using Tangosol.Util.Daemon.QueueProcessor.Service.Peer;

namespace Tangosol.Net.Messaging.Impl
{
    /// <summary>
    /// <see cref="IChannel"/> implementation.
    /// </summary>
    /// <remarks>
    /// A Channel is a communication construct that allows one or more
    /// threads to send and receive <see cref="IMessage"/> objects via a
    /// <see cref="IConnection"/>.
    /// </remarks>
    /// <author>Goran Milosavljevic  2006.08.21</author>
    /// <seealso cref="Peer"/>
    /// <seealso cref="IPofContext"/>
    /// <seealso cref="IPofSerializer"/>
    /// <seealso cref="IChannel"/>
    /// <seealso cref="IMessage"/>
    /// <seealso cref="IConnection"/>
    /// <since>Coherence 3.2</since>
    public class Channel : Extend, IPofSerializer, IChannel, IPofContext
    {
        #region Properties

        /// <summary>
        /// The <b>IConnection</b> that created this Channel.
        /// </summary>
        /// <value>
        /// The <b>IConnection</b> that created this Channel.
        /// </value>
        /// <seealso cref="IChannel.Connection"/>
        public virtual IConnection Connection
        {
            get { return m_connection; }
            set
            {
                Debug.Assert(!IsOpen);
                m_connection = value;
            }
        }

        /// <summary>
        /// The <see cref="Peer"/> that created
        /// this <b>IChannel</b> or <c>null</c> if the <b>IChannel</b> has
        /// been closed.
        /// </summary>
        public virtual Peer ConnectionManager
        {
            get
            {
                IConnection connection = Connection;
                return connection == null ? null : (Peer) connection.ConnectionManager;
            }
        }

        /// <summary>
        /// The unique identifier for this Channel.
        /// </summary>
        /// <remarks>
        /// The returned identifier is only unique among Channel objects
        /// created from the same underlying <b>IConnection</b>. In other
        /// words, Channel objects created by different <b>IConnection</b>
        /// objects may have the same unique identifier, but Channel objects
        /// created by the same <b>IConnection</b> cannot.
        /// </remarks>
        /// <value>
        /// A unique integer identifier for this Channel.
        /// </value>
        /// <seealso cref="IChannel.Id"/>
        public virtual int Id
        {
            get { return m_id; }
            set
            {
                Debug.Assert(!IsOpen);
                m_id = value;
            }
        }

        /// <summary>
        /// Return <b>true</b> if this Channel is open.
        /// </summary>
        /// <value>
        /// <b>true</b> if this Channel is open.
        /// </value>
        /// <seealso cref="IChannel.IsOpen"/>
        public virtual bool IsOpen
        {
            get { return m_isOpen; }
            set { m_isOpen = value; }
        }

        /// <summary>
        /// If <b>true</b>, the <b>Thread</b> that is currently executing
        /// within the Channel should close it immedately upon exiting the
        /// Channel's <b>Gate</b>.
        /// </summary>
        public virtual bool CloseOnExit
        {
            get { return m_closeOnExit; }
            set { m_closeOnExit = value; }
        }

        /// <summary>
        /// Return <b>true</b> if the calling thread is currently executing
        /// within the Channel's Gate.
        /// </summary>
        public virtual bool IsActiveThread
        {
            get { return Gate.IsEnteredByCurrentThread; }
        }

        /// <summary>
        /// Peer notification flag used when the Channel is closed
        /// upon exiting the <b>Gate</b> (<see cref="CloseOnExit"/>).
        /// </summary>
        public virtual bool CloseNotify
        {
            get { return m_closeNotify; }
            set { m_closeNotify = value; }
        }

        /// <summary>
        /// A counter used to generate unique identifiers for <b>Requests</b>
        /// sent through this Channel.
        /// </summary>
        protected virtual long RequestId
        {
            get { return m_requestId; }
            set { m_requestId = value; }
        }

        /// <summary>
        /// The <see cref="IMessageFactory"/> used to create <b>IMessage</b>
        /// objects that may be sent through this Channel over the
        /// underlying <b>IConnection</b>.
        /// </summary>
        /// <value>
        /// The <b>IMessageFactory</b> for this Channel.
        /// </value>
        /// <seealso cref="IChannel.MessageFactory"/>
        public virtual IMessageFactory MessageFactory
        {
            get { return m_messageFactory; }
            set
            {
                Debug.Assert(!IsOpen);
                m_messageFactory = value;
            }
        }

        /// <summary>
        /// The <see cref="ISerializer"/> used to serialize and deserialize
        /// payload objects carried by <b>IMessage</b> objects sent through
        /// this Channel.
        /// </summary>
        /// <value>
        /// The <b>ISerializer</b> for this Channel.
        /// </value>
        /// <seealso cref="IChannel.Serializer"/>
        public virtual ISerializer Serializer
        {
            get { return m_serializer; }
            set
            {
                Debug.Assert(!IsOpen || value == null);
                m_serializer = value;
            }
        }

        /// <summary>
        /// The optional <see cref="IReceiver"/> that processes unsolicited
        /// <b>IMessage</b> objects sent through this Channel over the
        /// underlying <b>IConnection</b>.
        /// </summary>
        /// <value>
        /// The <b>IReceiver</b> for this Channel or <c>null</c> if a
        /// <b>IReceiver</b> has not been associated with this
        /// Channel.
        /// </value>
        /// <seealso cref="IChannel.Receiver"/>
        public virtual IReceiver Receiver
        {
            get { return m_receiver; }
            set
            {
                Debug.Assert(!IsOpen);
                m_receiver = value;
            }
        }

        /// <summary>
        /// Gets or sets the map used to store Channel attributes.
        /// </summary>
        /// <value>
        /// A map with Channel attributes.
        /// </value>
        protected IDictionary Attributes { get; set; }

        /// <summary>
        /// An <see cref="ILongArray"/> with
        /// <see cref="Impl.Request.RequestStatus"/> objects, keyed by
        /// <b>Request</b> identifier.
        /// </summary>
        /// <value>
        /// An <b>ILongArray</b> with <b>RequestStatus</b> objects.
        /// </value>
        protected ILongArray RequestStatusArray { get; set; }

        /// <summary>
        /// The maximum number of milliseconds to wait for a
        /// <see cref="Response"/> before a <b>Request</b> times out.
        /// </summary>
        /// <remarks>
        /// A timeout of 0 is interpreted as an infinite timeout.
        /// </remarks>
        /// <value>
        /// The maximum number of milliseconds to wait for a
        /// <b>Response</b> before a <b>Request</b> times out.
        /// </value>
        /// <seealso cref="IChannel.Request(IRequest,long)"/>
        public virtual long RequestTimeout { get; set; }

        /// <summary>
        /// The optional <b>IPrincipal</b> object associated with this
        /// Channel.
        /// </summary>
        /// <remarks>
        /// If an <b>IPrincipal</b> is associated with this Channel, any
        /// operation performed upon receipt of an <b>IMessage</b> sent
        /// through this Channel will be done on behalf of the
        /// <b>IPrincipal</b>.
        /// </remarks>
        /// <value>
        /// The <b>IPrincipal</b> associated with this Channel.
        /// </value>
        /// <seealso cref="IChannel.Principal"/>
        public virtual IPrincipal Principal
        {
            get { return m_principal; }
            set
            {
                IsSecureContext = (IsSecureContext || value != null);
                m_principal = value;
            }
        }

        /// <summary>
        /// A <see cref="Gate"/> used to prevent concurrent use of this
        /// Channel while it is being opened and closed.
        /// </summary>
        /// <value>
        /// A <b>Gate</b> used to prevent concurrent use of this
        /// Channel while it is being opened and closed.
        /// </value>
        public Gate Gate
        {
            get { return m_gate; }
            set { m_gate = value; }
        }

        /// <summary>
        /// The <b>Exception</b> to pass to the
        /// <see cref="Close(bool,Exception)"/> method when the
        /// <b>IChannel</b> is closed upon exiting the <b>Gate</b>
        /// (see <see cref="CloseOnExit"/> property).
        /// </summary>
        public virtual Exception CloseException
        {
            get { return m_closeException; }
            set { m_closeException = value; }
        }

        /// <summary>
        /// True if either the channel principal or the proxy service principal
        /// exist. When both are null, we can optimize out the MessageAction.
        /// </summary>
        private bool IsSecureContext { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Channel()
        {
            Attributes         = new SynchronizedDictionary();
            RequestStatusArray = new LongSortedList();
            Gate               = GateFactory.NewGate;
            IsSecureContext    = Thread.CurrentPrincipal == null;
        }

        #endregion

        #region IChannel implementation

        /// <summary>
        /// Calculate the default timeout in milliseconds for the given
        /// <see cref="IRequest"/>.
        /// </summary>
        /// <param name="request">
        /// The <b>IRequest</b>.
        /// </param>
        /// <returns>
        /// The default timeout for the given <b>IRequest</b> in
        /// milliseconds.
        /// </returns>
        /// <seealso cref="RegisterRequest"/>
        /// <seealso cref="Request(IRequest)"/>
        protected virtual long CalculateRequestTimeout(IRequest request)
        {
            Peer manager = ConnectionManager;

            long millis = 0L;
            if (manager != null)
            {
                millis = manager.RequestTimeout;
                if (request is IPriorityTask)
                {
                    millis = Peer.AdjustTimeout(millis, ((IPriorityTask) request).RequestTimeoutMillis);
                }

                // when the RequestContext is in place (COH-1026) we will also have:
                /*
                RequestContext ctx = RequestContext.getContext();
                if (ctx != null)
                    {
                    cMillis = manager.adjustTimeout(cMillis, ctx.getRequestTimeout());
                    }
                */
            }

            return millis;
        }

        /// <summary>
        /// Close the Channel and reclaim all resources held by the
        /// Channel.
        /// </summary>
        /// <remarks>
        /// <p>
        /// When this method is invoked, it will not return until
        /// <b>IMessage</b> processing has been shut down in an orderly
        /// fashion. This means that the <see cref="IReceiver"/> object
        /// associated with this Channel (if any) have finished processing
        /// and that all pending requests are completed or canceled. If the
        /// <b>IReceiver</b> is processing an <b>IMessage</b> at the time when
        /// close is invoked, all the facilities of the Channel must remain
        /// available until it finishes.</p>
        /// <p>
        /// If the Channel is not open, calling this method has no effect.
        /// </p>
        /// </remarks>
        /// <seealso cref="IChannel.Close"/>
        public virtual void Close()
        {
            Close(true, null);
        }

        /// <summary>
        /// Close the <b>Channel</b>.
        /// </summary>
        /// <param name="notify">
        /// If <b>true</b>, the peer should be notified when the
        /// Channel is closed.
        /// </param>
        /// <param name="e">
        /// The optional reason why the Channel is being closed.
        /// </param>
        public virtual void Close(bool notify, Exception e)
        {
            if (IsOpen)
            {
                if (Id == 0)
                {
                    throw new InvalidOperationException("cannot close reserved channel: 0");
                }

                Peer manager = ConnectionManager;
                if (Thread.CurrentThread == manager.Thread)
                {
                    CloseInternal(notify, e, 0);
                }
                else
                {
                    Debug.Assert(!IsActiveThread,
                                 "cannot close a channel while executing within the channel");

                    // COH-18404 - not necessary to wait for the close to complete
                    manager.CloseChannel(this, notify, e, false);
                }
            }
        }

        /// <summary>
        /// The <see cref="Close()"/> implementation method. This method is
        /// called on the service thread.
        /// </summary>
        /// <param name="notify">
        /// if <b>true</b>, notify the peer that the <b>IChannel</b> is being
        /// closed.
        /// </param>
        /// <param name="e">
        /// The optional reason why the <b>IChannel</b> is being closed.
        /// </param>
        /// <param name="millis">
        /// The number of milliseconds to wait for the <b>IChannel</b> to
        /// close; pass 0 to perform a non-blocking close or -1 to wait
        /// forever.
        /// </param>
        /// <returns>
        /// <b>true</b> if the invocation of this method closed the
        /// <b>IChannel</b>.
        /// </returns>
        public virtual bool CloseInternal(bool notify, Exception e, int millis)
        {
            if (!IsOpen)
            {
                return false;
            }

            // cancel all pending requests and hold synchronization on the
            // request array while closing to prevent new requests from
            // being registered
            ILongArray requestStatusArray = RequestStatusArray;
            IReceiver  receiver           = Receiver;
            bool       closeReceiver      = false;

            lock (requestStatusArray.SyncRoot)
            {
                Exception eStatus = e ?? new ConnectionException("channel closed", Connection);

                IList requestStatusList = new ArrayList();
                foreach (DictionaryEntry entry in requestStatusArray)
                {
                    requestStatusList.Add(entry.Value);
                }
                foreach (Request.RequestStatus status in requestStatusList)
                {
                    status.Cancel(eStatus);
                }

                requestStatusArray.Clear();

                // close the Channel
                bool isClosed = GateClose(millis);
                try
                {
                    if (!isClosed)
                    {
                        // can't close the gate; signal to the holding
                        // Thread(s) that it must close the Channel
                        // immediately after exiting the gate
                        CloseOnExit    = true;
                        CloseNotify    = notify;
                        CloseException = e;

                        // double check if we can close the gate, as we want
                        // to be sure that the Thread(s) saw the close
                        // notification prior to exiting
                        isClosed = GateClose(0);
                    }

                    if (isClosed && IsOpen)
                    {
                        // notify the receiver that the Channel is closing
                        if (receiver != null)
                        {
                            closeReceiver = true;
                            try
                            {
                                receiver.UnregisterChannel(this);
                            }
                            catch (Exception ex)
                            {
                                CacheFactory.Log("Error unregistering channel from receiver: "
                                                 + receiver, ex, CacheFactory.LogLevel.Error);
                            }
                        }

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
            }

            if (closeReceiver)
            {
                try
                {
                    // needs to be done outside the request array synchronization block
                    receiver.OnChannelClosed(this);
                }
                catch (Exception ex)
                {
                    CacheFactory.Log("Error notifying channel closed to receiver: "
                            + receiver, ex, CacheFactory.LogLevel.Error);
                }
            }

            // notify the peer that the Channel is now closed
            if (notify && !IsOpen && Id != 0)
            {
                // send a NotifyChannelClosed to the peer via "Channel0"
                try
                {
                    IConnection         connection = Connection;
                    IChannel            channel0   = connection.GetChannel(0);
                    IMessageFactory     factory0   = channel0.MessageFactory;
                    NotifyChannelClosed message    = (NotifyChannelClosed) factory0.CreateMessage(NotifyChannelClosed.TYPE_ID);

                    message.Cause     = e;
                    message.ChannelId = Id;

                    channel0.Send(message);
                }
                catch (Exception)
                {}
            }

            // notify the Connection that the Channel is closed
            ((Connection) Connection).UnregisterChannel(this);

            // notify the ConnectionManager that the Channel is closed
            ConnectionManager.OnChannelClosed(this);

            return true;
        }

        /// <summary>
        /// Attempt to close the Channel <b>Gate</b>.
        /// </summary>
        /// <param name="millis">
        /// The number of milliseconds to wait for the <b>Gate</b> to
        /// close; pass 0 to perform a non-blocking close or -1 to wait
        /// forever.
        /// </param>
        /// <returns>
        /// <b>true</b> if the Channel <b>Gate</b> was closed;
        /// <b>false</b> otherwise.
        /// </returns>
        protected virtual bool GateClose(int millis)
        {
            return Gate.Close(millis);
        }

        /// <summary>
        /// Open the <b>IChannel</b> <b>Gate</b>.
        /// </summary>
        protected virtual void GateOpen()
        {
            Gate.Open();
        }

        /// <summary>
        /// Create a new <b>IMessage</b> of the specified type using this
        /// Channel's <b>MessageFactory</b>.
        /// </summary>
        /// <param name="type">
        /// The type identifier of the <b>IMessage</b> to create.
        /// </param>
        /// <returns>
        /// A new <b>IMessage</b> of the specified type.
        /// </returns>
        public virtual IMessage CreateMessage(int type)
        {
            return MessageFactory.CreateMessage(type);
        }

        /// <summary>
        /// Generate and return a new unique request identifier.
        /// </summary>
        /// <returns>
        /// The new unique request identifier.
        /// </returns>
        protected virtual long GenerateRequestId()
        {
            long id = RequestId;
            RequestId = id + 1;

            return id;
        }

        /// <summary>
        /// Return the object bound with the specified name to this
        /// Channel, or <c>null</c> if no object is bound with that
        /// name.
        /// </summary>
        /// <param name="name">
        /// The name with which the object was bound.
        /// </param>
        /// <returns>
        /// The object bound with the given name or <c>null</c> if no such
        /// binding exists.
        /// </returns>
        public virtual object GetAttribute(string name)
        {
            return Attributes[name];
        }

        /// <summary>
        /// Bind an object to the specified name in this Channel.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If an object is already bound to the specified name, it is
        /// replaced with the given object.</p>
        /// <p>
        /// Channel attributes are local to the binding peer's Channel. In
        /// other words, attributes bound to this IChannel object will not be
        /// bound to the peer's Channel object.</p>
        /// </remarks>
        /// <param name="name">
        /// The name with which to bind the object
        /// </param>
        /// <param name="value">
        /// The object to bind.
        /// </param>
        /// <returns>
        /// The object that the newly bound object replaced (if
        /// any).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If argument name passed is <c>null</c>.
        /// </exception>
        public virtual object SetAttribute(string name, object value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            object o = Attributes[name];
            Attributes[name] = value;
            return o;
        }

        /// <summary>
        /// Return the map of Channel attributes.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The keys of the map are the names with which the corresponding
        /// values have been bound to the Channel.</p>
        /// <p>
        /// The client should assume that the returned map is an immutable
        /// snapshot of the actual map of attribute objects maintained by
        /// this Channel.</p>
        /// </remarks>
        /// <returns>
        /// A map of attributes bound to this Channel.
        /// </returns>
        /// <seealso cref="IChannel.GetAttributes"/>
        public virtual IDictionary GetAttributes()
        {
            IDictionary attributes = Attributes;
            lock (attributes.SyncRoot)
            {
                return new HashDictionary(attributes);
            }
        }

        /// <summary>
        /// Unbind the object that was bound with the specified name to this
        /// Channel.
        /// </summary>
        /// <param name="name">
        /// The name with which the object was bound.
        /// </param>
        /// <returns>
        /// The object that was unbound.
        /// </returns>
        public virtual object RemoveAttribute(string name)
        {
            object obj = Attributes[name];
            Attributes.Remove(name);
            return obj;
        }

        /// <summary>
        /// Asynchronous <b>IMessage</b> send implementation.
        /// </summary>
        /// <param name="message">
        /// The <b>IMessage</b> to send asynchronously.
        /// </param>
        protected virtual void Post(IMessage message)
        {
            bool enter;
            if (message is IResponse)
            {
                Debug.Assert(IsActiveThread,
                             "can only send a response while executing within a channel");
                enter = false;
            }
            else
            {
                enter = true;
            }

            if (enter)
            {
                GateEnter();
            }
            try
            {
                message.Channel = this;
                ConnectionManager.Post(message);
            }
            catch (Exception e)
            {
                if (message is IRequest)
                {
                    IStatus status = ((IRequest) message).Status;
                    if (status != null)
                    {
                        status.Cancel(e);
                    }
                }
                throw;
            }
            finally
            {
                if (enter)
                {
                    GateExit();
                }
            }
        }

        /// <summary>
        /// Throws <b>ConnectionException</b> if the Channel is closed or
        /// closing.
        /// </summary>
        /// <exception cref="ConnectionException">
        /// Throws <b>ConnectionException</b> if the Channel is closed or
        /// closing.
        /// </exception>
        protected void AssertOpen()
        {
            if (!IsOpen)
            {
                // REVIEW
                throw new ConnectionException("channel is closed", Connection);
            }
        }

        /// <summary>
        /// Enter the <b>Connection</b> and Channel <b>Gate</b>
        /// (in that order).
        /// </summary>
        /// <exception cref="ConnectionException">
        /// If the <b>Connection</b> or Channel is closing or closed.
        /// </exception>
        public virtual void GateEnter()
        {
            Connection connection = Connection as Connection;
            connection.GateEnter();
            try
            {
                Gate gate = Gate;

                // if the thread is entering for the first time, throw an
                // exception if the Channel has been marked for close; this
                // prevents new threads from entering the Channel and thus
                // keeping it open longer than necessary
                if (CloseOnExit && !gate.IsEnteredByCurrentThread)
                {
                    // REVIEW
                    throw new ConnectionException("channel is closing", connection);
                }

                if (gate.Enter(0)) // see GateClose()
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
                    throw new ConnectionException("connection is closing", connection);
                }
            }
            catch (Exception)
            {
                connection.GateExit();
                throw;
            }
        }

        /// <summary>
        /// Exit the Channel and <b>Connection</b> <b>Gate</b> (in that
        /// order).
        /// </summary>
        public virtual void GateExit()
        {
            Gate gate = Gate;
            gate.Exit();
            ((Connection) Connection).GateExit();

            // see if we've been asked to close the Channel
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
        /// Opens the Channel and process.
        /// </summary>
        public virtual void Open()
        {
            OpenInternal();
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

            Debug.Assert(Connection     != null);
            Debug.Assert(MessageFactory != null);
            Debug.Assert(Serializer     != null);

            IsOpen = true;

            // notify the receiver that the Channel is open
            IReceiver receiver = Receiver;
            if (receiver != null)
            {
                try
                {
                    receiver.RegisterChannel(this);
                }
                catch (Exception e)
                {
                    CacheFactory.Log("Error registering channel with receiver: "
                                     + receiver, e, CacheFactory.LogLevel.Error);
                }
            }

            // notify the ConnectionManager that the Channel is opened
            ConnectionManager.OnChannelOpened(this);
        }

        /// <summary>
        /// Execute the given <b>IMessage</b>.
        /// </summary>
        /// <param name="message">
        /// The <b>IMessage</b> to execute.
        /// </param>
        protected virtual void Execute(IMessage message)
        {
            IReceiver receiver;
            if (message is IResponse)
            {
                // solicited Message
                receiver = null;
            }
            else
            {
                // unsolicited Message
                receiver = Receiver;
            }

            // Execute the Message in the context of the Channel's Principal
            // unless there is no Principal on the Channel or from the Proxy service.

            if (IsSecureContext)
            {
                MessageAction action = new MessageAction
                                           {
                                                   Message = message,
                                                   Receiver = receiver,
                                                   Principal = Principal
                                           };

                action.Run();
            }
            else
            {
                if (receiver == null)
                {
                    message.Run();
                }
                else
                {
                    receiver.OnMessage(message);
                }
            }
        }

        /// <summary>
        /// Called when a <b>IMessage</b> is received via this
        /// Channel. This method is called on the service thread
        /// ("Channel0" Messages) or on a daemon thread.
        /// </summary>
        /// <param name="message">
        /// Received <b>IMessage</b>.
        /// </param>
        /// <seealso cref="Peer.Send"/>
        public virtual void Receive(IMessage message)
        {
            Debug.Assert(message != null);

            try
            {
                GateEnter();
            }
            catch (ConnectionException)
            {
                // ignore: the Channel or Connection is closed or closing
                return;
            }
            try
            {
                if (message is IRequest)
                {
                    IRequest request = message as IRequest;
                    try
                    {
                        Execute(message);
                    }
                    catch (Exception e)
                    {
                        IResponse responseFail = request.EnsureResponse();
                        Debug.Assert(responseFail != null);

                        // see Request#isIncoming and #run
                        if (request.Status == null)
                        {
                            // report the exception and send it back to the peer
                            if (CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Debug))
                            {
                                CacheFactory.Log("An exception occurred while processing a "
                                                 + message.GetType().Name
                                                 + " for Service="
                                                 + ConnectionManager.ServiceName
                                                 + ": " + e.StackTrace,
                                                 CacheFactory.LogLevel.Debug);
                            }
                        }

                        responseFail.IsFailure = true;
                        responseFail.Result    = e;
                    }

                    IResponse response     = request.EnsureResponse();
                    Debug.Assert(response != null);
                    response.RequestId     = request.Id;

                    Send(response);
                }
                else if (message is IResponse)
                {
                    IResponse  response           = message as IResponse;
                    ILongArray requestStatusArray = RequestStatusArray;
                    long       id                 = response.RequestId;

                    Request.RequestStatus status;
                    lock (requestStatusArray.SyncRoot)
                    {
                        status = (Request.RequestStatus) requestStatusArray[id];
                    }

                    if (status == null)
                    {
                        // ignore unsolicited Responses
                    }
                    else
                    {
                        try
                        {
                            Execute(response);
                            if (response.IsFailure)
                            {
                                object result = response.Result;

                                // cancel the Status
                                if (result is Exception)
                                {
                                    status.Cancel((Exception) result);
                                }
                                else
                                {
                                    status.Cancel(new Exception(Convert.ToString(result)));
                                }
                            }
                            else
                            {
                                status.Response = response;
                            }
                        }
                        catch (Exception e)
                        {
                            status.Cancel(e);
                        }
                    }
                }
                else
                {
                    Execute(message);
                }
            }
            catch (Exception e)
            {
                Connection connection = (Connection) Connection;

                bool isInterrupted = false;
                for (Exception eCurrent = e; eCurrent != null; eCurrent = eCurrent.InnerException)
                {
                    if (eCurrent is ThreadInterruptedException)
                    {
                        isInterrupted = true;
                        break;
                    }
                }

                // see Acceptor#OnServiceStopped and Initiator#OnServiceStopped
                if (!connection.CloseOnExit || isInterrupted)
                {
                    CacheFactory.Log("Caught an unhandled exception while processing a "
                                     + message.GetType().Name
                                     + " for Service="
                                     + ConnectionManager.ServiceName,
                                     e,
                                     CacheFactory.LogLevel.Error);
                }

                if (Id == 0)
                {
                    connection.CloseOnExit    = true;
                    connection.CloseNotify    = true;
                    connection.CloseException = e;
                }
                else
                {
                    CloseOnExit    = true;
                    CloseNotify    = true;
                    CloseException = e;
                }
            }
            finally
            {
                GateExit();
            }
        }

        /// <summary>
        /// Called after a <b>Request</b> has completed either successfully
        /// or unsuccessfully.
        /// </summary>
        /// <param name="status">
        /// The <b>IStatus</b> representing the asynchronous <b>Request</b>.
        /// </param>
        public virtual void OnRequestCompleted(IStatus status)
        {
            UnregisterRequest(status);
        }

        /// <summary>
        /// Unregister the given <b>RequestStatus</b> from the
        /// <see cref="RequestStatusArray"/>.
        /// </summary>
        /// <param name="status">
        /// The <b>RequestStatus</b> to unregister; must not be <c>null</c>.
        /// </param>
        protected virtual void UnregisterRequest(IStatus status)
        {
            Debug.Assert(status != null);

            ILongArray requestStatusArray = RequestStatusArray;
            lock (requestStatusArray.SyncRoot) // see CloseInternal
            {
                requestStatusArray.Remove(status.Request.Id);
            }
        }

        /// <summary>
        /// Asynchronously send an <b>IMessage</b> to the peer endpoint
        /// through this IChannel over the underlying <b>IConnection</b>.
        /// </summary>
        /// <param name="message">
        /// The <b>IMessage</b> to send.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <b>IMessage</b> passed is <c>null</c>.
        /// </exception>
        /// <exception cref="ConnectionException">
        /// Thrown if Channel is closed.
        /// </exception>
        /// <seealso cref="IChannel.Send(IMessage)"/>
        public virtual void Send(IMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            Post(message);
        }

        /// <summary>
        /// Asynchronously send an <b>IRequest</b> to the peer endpoint
        /// through this Channel over the underlying <b>IConnection</b>.
        /// </summary>
        /// <param name="request">
        /// The <b>IRequest</b> to send.
        /// </param>
        /// <returns>
        /// An <see cref="IStatus"/> object representing the asynchronous
        /// <b>IRequest</b>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <b>IRequest</b> passed is <c>null</c>.
        /// </exception>
        /// <exception cref="ConnectionException">
        /// Thrown if Channel is closed.
        /// </exception>
        public virtual IStatus Send(IRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            IStatus status = RegisterRequest(request);
            Post(request);

            return status;
        }

        /// <summary>
        /// Create a <b>RequestStatus</b> for the given <b>IRequest</b> and
        /// register the <b>RequestStatus</b> in the
        /// <see cref="RequestStatusArray"/>.
        /// </summary>
        /// <param name="request">
        /// The <b>IRequest</b> to register; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The new <b>RequestStatus</b> that represents the asynchronous
        /// <b>Request</b>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the <b>IRequest</b> has already been registered.
        /// </exception>
        protected virtual IStatus RegisterRequest(IRequest request)
        {
            Debug.Assert(request != null);

            Request.RequestStatus status = new Request.RequestStatus
                                               {
                                                       Channel = this,
                                                       DefaultTimeoutMillis =
                                                               CalculateRequestTimeout
                                                               (request),
                                                       Request = request
                                               };

            request.Status = status;

            ILongArray requestStatusArray = RequestStatusArray;
            lock (requestStatusArray.SyncRoot) // see CloseInternal
            {
                AssertOpen();

                // generate a unique request ID
                long id = GenerateRequestId();
                request.Id = id;

                if (requestStatusArray.Exists(id))
                {
                    Debug.Assert(false, "duplicate request: " + request);
                }
                else
                {
                    requestStatusArray[id] = status;
                }
            }

            return status;
        }

        /// <summary>
        /// Synchronously send an <b>IRequest</b> to the peer endpoint through
        /// this Channel over the underlying <b>IConnection</b> and return
        /// the result of processing the <b>IRequest</b>.
        /// </summary>
        /// <param name="request">
        /// The <b>IRequest</b> to send.
        /// </param>
        /// <returns>
        /// The result sent by the peer.
        /// </returns>
        /// <seealso cref="IChannel.Request(IRequest)"/>
        public virtual object Request(IRequest request)
        {
            return Request(request, -1L);
        }

        /// <summary>
        /// Synchronously send an <b>IRequest</b> to the peer endpoint through
        /// this Channel over the underlying <b>IConnection</b> and return
        /// the result of processing the <b>IRequest</b>.
        /// </summary>
        /// <param name="request">
        /// The <b>IRequest</b> to send.
        /// </param>
        /// <param name="millis">
        /// The number of milliseconds to wait for the result; pass zero to
        /// block the calling thread indefinitely.
        /// </param>
        /// <returns>
        /// The result sent by the peer.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <b>IRequest</b> argument is <c>null</c>.
        /// </exception>
        /// <seealso cref="IChannel.Request(IRequest,long)"/>
        public virtual object Request(IRequest request, long millis)
        {
            Peer manager = ConnectionManager;
            Debug.Assert(manager.Thread != Thread.CurrentThread,
                         "request() is a blocking call and cannot be called on the service thread");

            // block until the service is ready
            manager.WaitAcceptingClients();

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            IStatus status = RegisterRequest(request);
            Post(request);

            IResponse response = status.WaitForResponse(millis);
            if (response.IsFailure)
            {
                object result = response.Result;
                if (result is Exception)
                {
                    throw (Exception) result;
                }
                throw new Exception("received error: " + result);
            }
            return response.Result;
        }

        /// <summary>
        /// Return the outstanding <b>IRequest</b> with the given identifier
        /// or <c>null</c> if no such <b>IRequest</b> exists.
        /// </summary>
        /// <remarks>
        /// This method can be used during <b>IResponse</b> execution to
        /// correlate the <b>IResponse</b> with the <b>IRequest</b> for which
        /// the <b>IResponse</b> was sent.
        /// </remarks>
        /// <param name="id">
        /// The unique identifer of the outstanding <b>IRequest</b>.
        /// </param>
        /// <returns>
        /// The outstanding <b>IRequest</b> with the given identifer or
        /// <c>null</c> if no such <b>IRequest</b> exists.
        /// </returns>
        public virtual IRequest GetRequest(long id)
        {
            ILongArray requestStatusArray = RequestStatusArray;
            lock (requestStatusArray.SyncRoot) // see #CloseInternal
            {
                IStatus status = (IStatus) requestStatusArray[id];
                return status == null ? null : status.Request;
            }
        }

        #endregion

        #region IPofSerializer implementation

        /// <summary>
        /// Serialize a user type instance to a POF stream by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <remarks>
        /// An implementation of <b>IPofSerializer</b> is required to follow
        /// the following steps in sequence for writing out an object of a
        /// user type:
        /// <ol>
        /// <li>If the object is evolvable, the implementation must set the
        /// version by calling <see cref="IPofWriter.VersionId"/>.</li>
        /// <li>The implementation may write any combination of the
        /// properties of the user type by using the "write" methods of the
        /// <b>IPofWriter</b>, but it must do so in the order of the property
        /// indexes.</li>
        /// <li>After all desired properties of the user type have been
        /// written, the implementation must terminate the writing of the
        /// user type by calling <see cref="IPofWriter.WriteRemainder"/>.
        /// </li>
        /// </ol>
        /// </remarks>
        /// <param name="writer">
        /// The <see cref="IPofWriter"/> with which to write the object's
        /// state.
        /// </param>
        /// <param name="o">
        /// The object to serialize.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        /// <seealso cref="IPofSerializer.Serialize"/>
        public virtual void Serialize(IPofWriter writer, object o)
        {
            ISerializer serializer = Serializer;
            if (serializer is IPofSerializer)
            {
                ((IPofSerializer) serializer).Serialize(writer, o);
            }
            else
            {
                BinaryMemoryStream stream = new BinaryMemoryStream(32);

                // use the serializer to write the object out as a binary property
                serializer.Serialize(new DataWriter(stream), o);
                writer.WriteBinary(0, stream.ToBinary());
                writer.WriteRemainder(null);
            }
        }

        /// <summary>
        /// Deserialize a user type instance from a POF stream by reading its
        /// state using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <remarks>
        /// An implementation of <b>IPofSerializer</b> is required to follow
        /// the following steps in sequence for reading in an object of a
        /// user type:
        /// <ol>
        /// <li>If the object is evolvable, the implementation must get the
        /// version by calling <see cref="IPofWriter.VersionId"/>.</li>
        /// <li>The implementation may read any combination of the
        /// properties of the user type by using "read" methods of the
        /// <b>IPofReader</b>, but it must do so in the order of the property
        /// indexes.</li>
        /// <li>After all desired properties of the user type have been read,
        /// the implementation must terminate the reading of the user type by
        /// calling <see cref="IPofReader.ReadRemainder"/>.</li>
        /// </ol>
        /// </remarks>
        /// <param name="reader">
        /// The <see cref="IPofReader"/> with which to read the object's
        /// state.
        /// </param>
        /// <returns>
        /// The deserialized user type instance.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        /// <seealso cref="IPofSerializer.Deserialize"/>
        public virtual object Deserialize(IPofReader reader)
        {
            ISerializer serializer = Serializer;
            if (serializer is IPofSerializer)
            {
                return ((IPofSerializer) serializer).Deserialize(reader);
            }

            Binary bin = reader.ReadBinary(0);
            reader.ReadRemainder();

            // use the serializer to read the object from a binary property
            return serializer.Deserialize(new DataReader(bin.GetStream()));
        }

        #endregion

        #region IPofContext implementation

        /// <summary>
        /// Return an <see cref="IPofSerializer"/> that can be used to
        /// serialize and deserialize an object of the specified user type to
        /// and from a POF stream.
        /// </summary>
        /// <param name="typeId">
        /// The type identifier of the user type that can be serialized and
        /// deserialized using the returned <b>IPofSerializer</b>; must be
        /// non-negative.
        /// </param>
        /// <returns>
        /// An <b>IPofSerializer</b> for the specified user type.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified user type is negative or unknown to this
        /// <b>IPofContext</b>.
        /// </exception>
        public virtual IPofSerializer GetPofSerializer(int typeId)
        {
            ISerializer serializer = Serializer;
            if (serializer is IPofContext)
            {
                return ((IPofContext) serializer).GetPofSerializer(typeId);
            }
            if (typeId == 0)
            {
                return this;
            }

            string target;
            try
            {
                target = Connection.ConnectionManager.ToString();
            }
            catch (Exception)
            {
                target = ToString();
            }

            throw new InvalidOperationException(target +
                    " has not been configured with an IPofContext; " +
                    " this channel cannot decode POF-encoded user types");
        }

        /// <summary>
        /// Determine the user type identifier associated with the given
        /// object.
        /// </summary>
        /// <param name="o">
        /// An instance of a user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The type identifier of the user type associated with the given
        /// object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the user type associated with the given object is unknown to
        /// this <b>IPofContext</b>.
        /// </exception>
        public virtual int GetUserTypeIdentifier(object o)
        {
            ISerializer serializer = Serializer;
            if (serializer is IPofContext)
            {
                return ((IPofContext) serializer).GetUserTypeIdentifier(o);
            }
            Debug.Assert(o != null);
            return 0;
        }

        /// <summary>
        /// Determine the user type identifier associated with the given
        /// type.
        /// </summary>
        /// <param name="type">
        /// A user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The type identifier of the user type associated with the given
        /// type.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the user type associated with the given type is unknown to
        /// this <b>IPofContext</b>.
        /// </exception>
        public virtual int GetUserTypeIdentifier(Type type)
        {
            ISerializer serializer = Serializer;
            if (serializer is IPofContext)
            {
                return ((IPofContext) serializer).GetUserTypeIdentifier(type);
            }
            Debug.Assert(type != null);
            return 0;
        }

        /// <summary>
        /// Determine the user type identifier associated with the given type
        /// name.
        /// </summary>
        /// <param name="typeName">
        /// The name of a user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The type identifier of the user type associated with the given
        /// type name.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the user type associated with the given type name is unknown
        /// to this <b>IPofContext</b>.
        /// </exception>
        public virtual int GetUserTypeIdentifier(string typeName)
        {
            ISerializer serializer = Serializer;
            if (serializer is IPofContext)
            {
                return ((IPofContext) serializer).GetUserTypeIdentifier(typeName);
            }
            Debug.Assert(typeName != null);
            return 0;
        }

        /// <summary>
        /// Determine the name of the type associated with a user type
        /// identifier.
        /// </summary>
        /// <param name="typeId">
        /// The user type identifier; must be non-negative.
        /// </param>
        /// <returns>
        /// The name of the type associated with the specified user type
        /// identifier.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified user type is negative or unknown to this
        /// <b>IPofContext</b>.
        /// </exception>
        public virtual string GetTypeName(int typeId)
        {
            return GetType(typeId).FullName;
        }

        /// <summary>
        /// Determine the type associated with the given user type
        /// identifier.
        /// </summary>
        /// <param name="typeId">
        /// The user type identifier; must be non-negative.
        /// </param>
        /// <returns>
        /// The type associated with the specified user type identifier.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified user type is negative or unknown to this
        /// <b>IPofContext</b>.
        /// </exception>
        public virtual Type GetType(int typeId)
        {
            ISerializer serializer = Serializer;
            if (serializer is IPofContext)
            {
                return ((IPofContext) serializer).GetType(typeId);
            }
            throw new ArgumentException("cannot determine class for user type ID: " + typeId);
        }

        /// <summary>
        /// Determine if the given object is of a user type known to this
        /// <b>IPofContext</b>.
        /// </summary>
        /// <param name="o">
        /// The object to test; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> if the specified object is of a valid user type.
        /// </returns>
        public virtual bool IsUserType(object o)
        {
            ISerializer serializer = Serializer;
            if (serializer is IPofContext)
            {
                return ((IPofContext) serializer).IsUserType(o);
            }
            Debug.Assert(o != null);
            return false;
        }

        /// <summary>
        /// Determine if the given type is a user type known to this
        /// <b>IPofContext</b>.
        /// </summary>
        /// <param name="type">
        /// The type to test; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the specified type is a valid user type.
        /// </returns>
        public virtual bool IsUserType(Type type)
        {
            ISerializer serializer = Serializer;
            if (serializer is IPofContext)
            {
                return ((IPofContext) serializer).IsUserType(type);
            }
            Debug.Assert(type != null);
            return false;
        }

        /// <summary>
        /// Determine if the type with the given name is a user type known to
        /// this <b>IPofContext</b>.
        /// </summary>
        /// <param name="typeName">
        /// The name of the type to test; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the type with the specified name is a valid user
        /// type.
        /// </returns>
        public virtual bool IsUserType(string typeName)
        {
            ISerializer serializer = Serializer;
            if (serializer is IPofContext)
            {
                return ((IPofContext) serializer).IsUserType(typeName);
            }
            Debug.Assert(typeName != null);
            return false;
        }

        /// <summary>
        /// Serialize an object to a stream by writing its state using the
        /// specified <see cref="DataWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>DataWriter</b> with which to write the object's state.
        /// </param>
        /// <param name="o">
        /// The object to serialize.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void Serialize(DataWriter writer, object o)
        {
            PofStreamWriter pofwriter = new PofStreamWriter(writer, this);
            pofwriter.WriteObject(-1, o);
        }

        /// <summary>
        /// Deserialize an object from a stream by reading its state using
        /// the specified <see cref="DataReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>DataReader</b> with which to read the object's state.
        /// </param>
        /// <returns>
        /// The deserialized user type instance.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual object Deserialize(DataReader reader)
        {
            PofStreamReader pofreader = new PofStreamReader(reader, this);
            return pofreader.ReadObject(-1);
        }

        #endregion

        #region Channel implementation

        /// <summary>
        /// Return a human-readable description of this Channel.
        /// </summary>
        /// <returns>
        /// A string representation of this Channel.
        /// </returns>
        /// <since>12.2.1.3</since>
        protected override string GetDescription()
        {
            StringBuilder sb = new StringBuilder(32);

            bool fOpen = IsOpen;

            sb.Append("Id=").Append(Id)
              .Append(", Open=").Append(fOpen);

            if (fOpen)
            {
                sb.Append(", Connection=");
                IConnection connection = Connection;
                if (connection == null)
                {
                    sb.Append("null");
                }
                else
                {
                    sb.Append(connection.Id);
                }
            }

            IMessageFactory factory = MessageFactory;
            if (factory != null)
            {
                IProtocol protocol = factory.Protocol;
                if (protocol != null)
                {
                    sb.Append(", Protocol=").Append(protocol).Append(", ")
                        .Append("NegotiatedProtocolVersion=").Append(factory.Version);
                }
            }

            IReceiver receiver = Receiver;
            if (receiver != null)
            {
                sb.Append(", Receiver=").Append(receiver);
            }

            return sb.ToString();
        }

        #endregion

        #region MessageAction inner class

        /// <summary>
        /// Implementation used to process a received <see cref="IMessage"/>
        /// on behalf of a <see cref="Principal"/>.
        /// </summary>
        public class MessageAction : IRunnable
        {
            #region Properties

            /// <summary>
            /// Gets or sets the Message to process.
            /// </summary>
            public virtual IMessage Message
            {
                get { return m_message; }
                set { m_message = value; }
            }

            /// <summary>
            /// Gets or sets the optional Receiver that will process the
            /// Message.
            /// </summary>
            public virtual IReceiver Receiver
            {
                get { return m_receiver; }
                set { m_receiver = value; }
            }

            /// <summary>
            /// Gets or sets the <b>IPrincipal</b> used to execute this
            /// <b>IMessage</b>.
            /// </summary>
            public virtual IPrincipal Principal
            {
                get { return m_principal; }
                set { m_principal = value; }
            }

            #endregion

            #region Object overriden methods

            /// <summary>
            /// Returns a human-readable description of this object.
            /// </summary>
            /// <returns>
            /// Human-readable description of this object.
            /// </returns>
            public override string ToString()
            {
                return "Message=" + Message + ", Receiver=" + Receiver;
            }

            #endregion

            #region IRunnable implementation

            /// <summary>
            ///  Execute this <b>MessageAction</b>.
            /// </summary>
            public virtual void Run()
            {
                IMessage   message           = Message;
                IReceiver  receiver          = Receiver;
                IPrincipal principal         = Principal;
                IPrincipal originalPrincipal = Thread.CurrentPrincipal;

                Thread.CurrentPrincipal = principal;
                try
                {
                    if (receiver == null)
                    {
                        message.Run();
                    }
                    else
                    {
                        receiver.OnMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("could not execute action as principal: " + principal, ex);
                }
                finally
                {
                    // restore the original principal
                    Thread.CurrentPrincipal = originalPrincipal;
                }

            }

            #endregion

            #region Data members

            /// <summary>
            /// The Message to process.
            /// </summary>
            private IMessage m_message;

            /// <summary>
            /// The optional Receiver that will process the Message.
            /// </summary>
            private IReceiver m_receiver;

            /// <summary>
            /// The <b>IPrincipal</b> used to execute this message.
            /// </summary>
            private IPrincipal m_principal;

            #endregion
        }

        #endregion

        #region Data members

        /// <summary>
        /// Peer notification flag used when the Channel is closed upon
        /// exiting the Gate (see <see cref="CloseOnExit"/> property).
        /// </summary>
        private volatile bool m_closeNotify;

        /// <summary>
        /// If <b>true</b>, the Thread that is currently executing within the
        /// Channel should close it immedately upon exiting the Channel's
        /// Gate.
        /// </summary>
        private volatile bool m_closeOnExit;

        /// <summary>
        /// A counter used to generate unique identifiers for Requests sent
        /// through this Channel.
        /// </summary>
        [NonSerialized]
        private long m_requestId;

        /// <summary>
        /// The Throwable to pass to the <see cref="Close()"/> method when
        /// the Channel is closed upon exiting the Gate (see
        /// <see cref="CloseOnExit"/> property).
        /// </summary>
        private volatile Exception m_closeException;

        /// <summary>
        /// The <b>IConnection</b> that created this Channel.
        /// </summary>
        [NonSerialized]
        private volatile IConnection m_connection;

        /// <summary>
        /// The unique identifier for this Channel.
        /// </summary>
        private int m_id;

        /// <summary>
        /// The <b>IMessageFactory</b> used to create <b>IMessage</b> objects
        /// that may be sent through this Channel.
        /// </summary>
        private IMessageFactory m_messageFactory;

        /// <summary>
        /// <b>true</b> if the Channel is open; <b>false</b> otherwise.
        /// </summary>
        private volatile bool m_isOpen;

        /// <summary>
        /// The optional <b>IReceiver</b> that processes unsolicited
        /// <b>IMessage</b> objects sent through this Channel.
        /// </summary>
        private IReceiver m_receiver;

        /// <summary>
        /// The <b>ISerializer</b> used to serialize and deserialize payload
        /// objects carried by Messages sent through this Channel.
        /// </summary>
        private ISerializer m_serializer;

        /// <summary>
        /// The optional <b>IPrincipal</b> associated with the Channel.
        /// </summary>
        private IPrincipal m_principal;

        /// <summary>
        /// A Gate used to prevent concurrent use of this Channel while
        /// it is being opened and closed.
        /// </summary>
        [NonSerialized]
        private Gate m_gate;

        #endregion
    }
}