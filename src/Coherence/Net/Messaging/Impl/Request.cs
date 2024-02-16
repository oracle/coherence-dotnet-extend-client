/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using Tangosol.IO.Pof;
using Tangosol.Net.Impl;
using Tangosol.Util;
using Tangosol.Util.Daemon.QueueProcessor.Service.Peer;

namespace Tangosol.Net.Messaging.Impl
{
    /// <summary>
    /// Base implementation of <see cref="IRequest"/>.
    /// </summary>
    /// <author>Ana Cikic  2006.08.18</author>
    /// <seealso cref="IRequest"/>
    /// <seealso cref="Message"/>
    public abstract class Request : Message, IRequest
    {
        #region Properties

        /// <summary>
        /// <b>IResponse</b> for this IRequest.
        /// </summary>
        /// <value>
        /// The <b>IResponse</b>.
        /// </value>
        protected virtual IResponse Response
        {
            get { return m_response; }
            set
            {
                Debug.Assert(Response == null);
                m_response = value;
            }
        }

        /// <summary>
        /// Determines if an incoming IRequest was sent by a peer.
        /// </summary>
        /// <value>
        /// <b>true</b> if an incoming IRequest was sent by a peer.
        /// </value>
        public virtual bool IsIncoming
        {
            get { return Status == null; }
        }

        #endregion

        #region IRequest implementation

        /// <summary>
        /// The unique identifier for this IRequest.
        /// </summary>
        /// <value>
        /// An identifier that uniquely identifies this IRequest object.
        /// </value>
        public virtual long Id
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// The <see cref="IStatus"/> for this IRequest that can be used to
        /// wait for and retrieve the <see cref="IResponse"/>.
        /// </summary>
        /// <value>
        /// The <b>IStatus</b> object or <c>null</c> if object hasn't been
        /// initialized.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If the status has already been configured.
        /// </exception>
        public virtual IStatus Status
        {
            get { return m_status; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", "Status cannot be null");
                }
                if (m_status == null)
                {
                    m_status = value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// Return the <see cref="IResponse"/> for this IRequest.
        /// </summary>
        /// <remarks>
        /// If not already available, the <b>IResponse</b> must be created
        /// using the <see cref="IMessageFactory"/> associated with the
        /// <see cref="IChannel"/> that this IRequest was sent through.
        /// </remarks>
        /// <returns>
        /// The <b>IResponse</b>; must not be <c>null</c>.
        /// </returns>
        public virtual IResponse EnsureResponse()
        {
            IResponse response = m_response;
            if (response == null)
            {
                IChannel channel = Channel;
                if (channel == null)
                {
                    throw new InvalidOperationException("null channel");
                }

                IMessageFactory factory = channel.MessageFactory;
                if (factory == null)
                {
                    throw new InvalidOperationException("null factory");
                }

                response = m_response = InstantiateResponse(factory);
            }

            return response;
        }

        #endregion

        #region Internal methods

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
        protected virtual Response InstantiateResponse(IMessageFactory factory)
        {
            return (Response) factory.CreateMessage(0);
        }

        /// <summary>
        /// Called when an exception is caught while executing the IRequest.
        /// </summary>
        /// <param name="e">
        /// The unhandled exception.
        /// </param>
        protected virtual void OnException(Exception e)
        {
            if (IsIncoming)
            {
                if (CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Debug))
                {
                    Peer manager = (Peer) Channel.Connection.ConnectionManager;
                    CacheFactory.Log("An exception occurred while processing a "
                        + GetType().Name + " for Service="
                        + manager.ServiceName
                        + ": " + e, CacheFactory.LogLevel.Debug);
                }
            }
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
        protected virtual void OnRun(Response response)
        {}

        #endregion

        #region Message overrides

        /// <summary>
        /// Execute the action specific to the <see cref="Message"/>
        /// implementation.
        /// </summary>
        public sealed override void Run()
        {
            Response response = (Response) EnsureResponse();
            try
            {
                OnRun(response);
            }
            catch (Exception e)
            {
                OnException(e);

                response.IsFailure = true;
                response.Result    = e;
            }
        }
        
        #endregion
        
        #region Extend Overrides

        /// <inheritdoc />
        protected override string GetDescription()
        {
            return  base.GetDescription() + ", Id=" + Id + ", Status=" + Status;
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

            Id = reader.ReadInt64(0);
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

            writer.WriteInt64(0, Id);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The unique identifier of this IRequest.
        /// </summary>
        private long m_id;

        /// <summary>
        /// The IResponse.
        /// </summary>
        [NonSerialized]
        private IResponse m_response;

        /// <summary>
        /// The IStatus of this IRequest.
        /// </summary>
        [NonSerialized]
        private IStatus m_status;

        #endregion

        #region Inner class: RequestStatus

        /// <summary>
        /// Implementation of the <see cref="IStatus"/> interface.
        /// </summary>
        /// <author>Ana Cikic  2006.08.21</author>
        public class RequestStatus : Extend, IStatus
        {
            #region Properties

            /// <summary>
            /// The <see cref="Channel"/> associated with this Status.
            /// </summary>
            /// <value>
            /// The <b>Channel</b> associated with this Status.
            /// </value>
            public virtual Channel Channel
            {
                get { return m_channel; }
                set
                {
                    Debug.Assert(!IsClosed && value != null && Channel == null);
                    m_channel = value;
                }
            }

            /// <summary>
            /// The default request timeout in milliseconds.
            /// </summary>
            /// <value>
            /// The default request timeout in milliseconds.
            /// </value>
            /// <seealso cref="WaitForResponse()"/>
            public virtual long DefaultTimeoutMillis
            {
                get { return m_defaultTimeoutMillis; }
                set { m_defaultTimeoutMillis = value; }
            }

            /// <summary>
            /// The time (in milliseconds) that this Status object was
            /// initialized.
            /// </summary>
            /// <value>
            /// The time (in milliseconds) that this Status object was
            /// initialized.
            /// </value>
            public virtual long InitTimeMillis
            {
                get { return m_initTimeMillis; }
                set { m_initTimeMillis = value; }
            }

            /// <summary>
            /// Return the <b>IRequest</b> represented by this IStatus.
            /// </summary>
            /// <value>
            /// The <b>IRequest</b> represented by this IStatus.
            /// </value>
            public virtual IRequest Request
            {
                get { return m_request; }
                set
                {
                    Debug.Assert(!IsClosed && value != null && Request == null);
                    m_request = value;
                }
            }

            /// <summary>
            /// Return the <b>IResponse</b> sent by the peer.
            /// </summary>
            /// <remarks>
            /// This property has a non-null value if <see cref="IsClosed"/> is
            /// <b>true</b>.
            /// </remarks>
            /// <value>
            /// The <b>IResponse</b> sent by the peer.
            /// </value>
            /// <exception cref="Exception">
            /// If the <b>IRequest</b> is cancelled.
            /// </exception>
            public virtual IResponse Response
            {
                get
                {
                    Exception e = Exception;
                    if (e == null)
                    {
                        return m_response;
                    }
                    throw e;
                }
                set
                {
                    Debug.Assert(!IsClosed && value != null && Response == null);

                    Channel channel;
                    using (BlockingLock l = BlockingLock.Lock(this))
                    {
                        if (IsClosed)
                        {
                            channel = null;
                        }
                        else
                        {
                            channel = Channel;

                            m_response = value;
                            OnCompletion();
                        }
                    }

                    if (channel != null)
                    {
                        channel.OnRequestCompleted(this);
                    }
                }
            }

            /// <summary>
            /// Determine if the <b>IRequest</b> represented by this Status has
            /// been completed successfully, completed unsuccessfully, or
            /// canceled.
            /// </summary>
            /// <value>
            /// <b>true</b> if the <b>IRequest</b> has been completed
            /// successfully, completed unsuccessfully, or canceled; <b>false</b>
            /// if the <b>IRequest</b> is still pending.
            /// </value>
            public virtual bool IsClosed
            {
                get { return m_isClosed; }
                set { m_isClosed = value; }
            }

            /// <summary>
            /// The <b>Exception</b> associated with a failed or canceled
            /// request.
            /// </summary>
            /// <value>
            /// An <b>Exception</b>.
            /// </value>
            public virtual Exception Exception
            {
                get { return m_exception; }
                set { m_exception = value; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Default constructor.
            /// </summary>
            public RequestStatus()
            {
                InitTimeMillis = DateTime.UtcNow.Ticks / 10000;
            }

            #endregion

            #region IStatus implementation

            /// <summary>
            /// Cancel the <b>IRequest</b> represented by this IStatus.
            /// </summary>
            /// <remarks>
            /// The requestor can call this method when it is no longer
            /// interested in an <b>IResponse</b> or outcome of the
            /// <b>IRequest</b>.
            /// </remarks>
            public virtual void Cancel()
            {
                Cancel(null);
            }

            /// <summary>
            /// Cancel the <b>IRequest</b> represented by this <b>IStatus</b>
            /// due to an error condition.
            /// </summary>
            /// <remarks>
            /// After this method is called, the <see cref="Response"/> will
            /// throw this exception (wrapping it if necessary).
            /// </remarks>
            /// <param name="e">
            /// The reason that the <b>IRequest</b> is being cancelled.
            /// </param>
            public virtual void Cancel(Exception e)
            {
                if (e == null)
                {
                    e = new Exception("request was canceled");
                }

                Channel channel;
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    if (IsClosed)
                    {
                        channel = null;
                    }
                    else
                    {
                        channel = Channel;

                        Exception = e;
                        OnCompletion();
                    }
                }

                if (channel != null)
                {
                    channel.OnRequestCompleted(this);
                }
            }

            /// <summary>
            /// Block the calling thread until the <b>IRequest</b> is completed
            /// successfully, completed unsuccessfully, canceled, or a timeout
            /// occurs.
            /// </summary>
            /// <returns>
            /// The <b>IResponse</b>.
            /// </returns>
            /// <exception cref="Exception">
            /// If the <b>IRequest</b> is cancelled, a timeout occurs, or the
            /// waiting thread is interrupted.
            /// </exception>
            public virtual IResponse WaitForResponse()
            {
                return WaitForResponse(-1L);
            }

            /// <summary>
            /// Block the calling thread until the <b>IRequest</b> is completed
            /// successfully, completed unsuccessfully, canceled, or a timeout
            /// occurs.
            /// </summary>
            /// <param name="millis">
            /// The number of milliseconds to wait for the result of the
            /// <b>IRequest</b>; pass zero to block the calling thread
            /// indefinitely.
            /// </param>
            /// <returns>
            /// The <b>IResponse</b>.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// If the <b>IRequest</b> is cancelled, a timeout occurs, or the
            /// waiting thread is interrupted.
            /// </exception>
            public virtual IResponse WaitForResponse(long millis)
            {
                Exception e = null;

                if (millis == -1L)
                {
                    millis = DefaultTimeoutMillis;
                }

                if (millis <= 0L || millis > int.MaxValue)
                {
                    using (BlockingLock l = BlockingLock.Lock(this))
                    {
                        while (!IsClosed)
                        {
                            try
                            {
                                Blocking.Wait(this);
                            }
                            catch (Exception caught) // only ThreadInterruptedException can be caught here
                            {
                                e = caught;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    using (BlockingLock l = BlockingLock.Lock(this))
                    {
                        long start  = -1L;
                        long remain = millis;

                        while (!IsClosed)
                        {
                            if (start < 0L)
                            {
                                start = DateTime.UtcNow.Ticks / 10000;
                            }

                            try
                            {
                                Blocking.Wait(this, (int) remain);
                            }
                            catch (Exception caught) // only ThreadInterruptedException can be caught here
                            {
                                e = caught;
                                break;
                            }

                            if (IsClosed)
                            {
                                break;
                            }
                            else if ((remain -= Math.Max(DateTime.UtcNow.Ticks / 10000 - start, 0L)) <= 0L)
                            {
                                e = new RequestTimeoutException("request timed out after " + millis + " millis");
                                break;
                            }
                        }
                    }
                }

                // COH-6105 - Process exceptions outside of the synchronized blocks
                if (e != null)
                {
                    Cancel(e);
                    if (!(e is RequestTimeoutException))
                    {
                        Thread.CurrentThread.Interrupt();
                        throw e;
                    }
                }
                return Response;
            }

            #endregion
            
            #region Extend Overrides
        
            /// <inheritdoc />
            protected override string GetDescription()
            {
                StringBuilder sb = new StringBuilder();

                bool fClosed = IsClosed;
                sb.Append("InitTimeMillis=")
                    .Append(InitTimeMillis)
                    .Append(", Closed=")
                    .Append(fClosed);

                if (fClosed)
                {
                    Exception t = Exception;
                    if (t == null)
                    {
                        sb.Append(", Response=")
                            .Append(Response);
                    }
                    else
                    {
                        sb.Append(", Error=")
                            .Append(t);
                    }
                }

                return sb.ToString();
            }
        
            #endregion

            #region Internal methods

            /// <summary>
            /// Called after the <b>IRequest</b> represented by this
            /// RequestStatus has completed (successfully or unsuccessfully) or
            /// been canceled.
            /// </summary>
            protected virtual void OnCompletion()
            {
                IsClosed = true;
                Monitor.PulseAll(this);
            }

            #endregion

            #region Data members

            /// <summary>
            /// The Channel associated with this Status.
            /// </summary>
            private volatile Channel m_channel;

            /// <summary>
            /// Flag that indicates whether or not the Request represented by
            /// this Status has completed successfully, completed unsucessfully,
            /// or been canceled.
            /// </summary>
            private volatile bool m_isClosed;

            /// <summary>
            /// The default request timeout in milliseconds.
            /// </summary>
            private long m_defaultTimeoutMillis;

            /// <summary>
            /// The exception associated with a failed or canceled request.
            /// </summary>
            private Exception m_exception;

            /// <summary>
            /// The time (in millseconds) that this Status object was
            /// initialized.
            /// </summary>
            private long m_initTimeMillis;

            /// <summary>
            /// The Request represented by this Status.
            /// </summary>
            private volatile IRequest m_request;

            /// <summary>
            /// The Response sent by the peer.
            /// </summary>
            private volatile IResponse m_response;

            #endregion
        }

        #endregion
    }
}