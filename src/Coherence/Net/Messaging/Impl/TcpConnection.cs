/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace Tangosol.Net.Messaging.Impl
{
    /// <summary>
    /// <see cref="Connection"/> implementation that wraps a TCP/IP Socket.
    /// </summary>
    /// <author>Ana Cikic  2006.08.23</author>
    /// <seealso cref="Connection"/>
    public class TcpConnection : Connection
    {
        #region Connection override methods

        /// <summary>
        /// Return a human-readable description of this TcpConnection.
        /// </summary>
        /// <returns>
        /// A string representation of this TcpConnection.
        /// </returns>
        /// <since>Coherence 3.7</since>
        protected override string GetDescription()
        {
            TcpClient client = Client;
            Socket    socket = client == null ? null : client.Client;

            try
            {
                return socket == null
                    ? base.GetDescription()
                    : base.GetDescription()
                        + ", LocalAddress=" + socket.LocalEndPoint
                        + ", RemoteAddress=" + socket.RemoteEndPoint;
            }
            catch (ObjectDisposedException)
            {
                // Bug 24917364 - catch dereference of disposed socket.
                // Occurs after EventDispatcher catches exception thrown in MemberLeft handler
                // and attempts to log the handled exception.  Since in MemberLeft handler,
                // not a surprise that socket has already been disposed.
                // TODO: figure out why TcpClient.Client was not set to null after dispose of Socket.
                return base.GetDescription();
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// 
        /// </summary>
        public TcpClient Client { get; set; }

        #endregion
    }
}