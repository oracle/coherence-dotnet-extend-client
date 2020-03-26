/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;

using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator
{
    /// <summary>
    /// Implementation of
    /// <see cref="Util.Daemon.QueueProcessor.Service.Peer.OpenConnectionRequest"/>
    /// specific for <see cref="Initiator"/>.
    /// </summary>
    public class OpenConnectionRequest : Util.Daemon.QueueProcessor.Service.Peer.OpenConnectionRequest
    {
        #region Properties

        /// <summary>
        /// The Connection to open.
        /// </summary>
        public virtual Connection ConnectionOpen
        {
            get { return m_connectionOpen; }
            set { m_connectionOpen = value; }
        }

        #endregion

        #region Request overrides

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
            Debug.Assert(Channel.Id == 0);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The Connection to open.
        /// </summary>
        private Connection m_connectionOpen;

        #endregion
    }
}
