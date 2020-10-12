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
    /// Internal <see cref="IRequest"/> used to close an
    /// <see cref="IConnection"/>.
    /// </summary>
    public class CloseConnection : Request
    {
        #region Properties

        /// <summary>
        /// The optional reason why the <b>IConnection</b> is being closed.
        /// </summary>
        /// <value>
        /// The <b>Exception</b> that was the reason why the
        /// <b>IConnection</b> is being closed.
        /// </value>
        public virtual Exception Cause
        {
            get { return m_cause; }
            set { m_cause = value; }
        }

        /// <summary>
        /// The <see cref="Connection"/> to close.
        /// </summary>
        /// <value>
        /// The <b>Connection</b> to close.
        /// </value>
        public virtual Connection ConnectionClose
        {
            get { return m_connectionClose; }
            set { m_connectionClose = value; }
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
        /// If <b>true</b>, notify the peer that the <b>IConnection</b> is
        /// being closed.
        /// </summary>
        /// <value>
        /// <b>true</b> to notify the peer that the <b>IConnection</b> is
        /// being closed.
        /// </value>
        public virtual bool IsNotify
        {
            get { return m_isNotify; }
            set { m_isNotify = value; }
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
            ConnectionClose.CloseInternal(IsNotify, Cause, 0);
        }
        
        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = -3;

        /// <summary>
        /// The optional reason why the IConnection is being closed.
        /// </summary>
        [NonSerialized]
        private Exception m_cause;

        /// <summary>
        /// The Connection to close.
        /// </summary>
        [NonSerialized]
        private Connection m_connectionClose;

        /// <summary>
        /// If true, notify the peer that the IConnection is being closed.
        /// </summary>
        [NonSerialized]
        private bool m_isNotify;

        #endregion
    }
}