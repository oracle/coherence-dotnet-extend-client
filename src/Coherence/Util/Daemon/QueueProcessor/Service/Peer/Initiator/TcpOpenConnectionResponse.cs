/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.Collections;
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator
{
    /// <summary>
    /// Implementation of
    /// <see cref="Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator.OpenConnectionResponse"/>
    /// specific for <see cref="TcpInitiator"/>.
    /// </summary>
    class TcpOpenConnectionResponse : OpenConnectionResponse
    {
        #region Properties

        /// <summary>
        /// True if the TcpConnection should be redirected.
        /// </summary>
        public bool IsRedirect { get; set; }

        /// <summary>
        /// A list of TCP/IP addresses that the TcpConnection should be 
        /// redirected to. Each element of the list is a two element 
        /// array, with the first element being the IP address in string 
        /// format and the second being the port number.
        /// </summary>
        public IList RedirectList { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TcpOpenConnectionResponse()
        {
            ImplVersion = 1;
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading
        /// its state using the specified <see cref="IPofReader"/>
        /// object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's
        /// state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            IsRedirect = reader.ReadBoolean(10);
            if (IsRedirect)
            {
                RedirectList = (IList) reader.ReadCollection(11, new ArrayList());
            }
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing
        /// its state using the specified <see cref="IPofWriter"/>
        /// object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's
        /// state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteBoolean(10, IsRedirect);
            if (IsRedirect)
            {
                writer.WriteCollection(11, RedirectList);
            }
        }

        #endregion

        #region IRunnable implementation

        /// <summary>
        /// Execute the action specific to the Message implementation.
        /// </summary>
        public override void Run()
        {
            // update the connection with redirection information
            var connection = (TcpInitiator.TcpConnection) Channel.Connection;
            connection.IsRedirect   = IsRedirect;
            connection.RedirectList = RedirectList;

            base.Run();
        }

        #endregion
    }
}
