/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator
{
    /// <summary>
    /// Implementation of
    /// <see cref="Util.Daemon.QueueProcessor.Service.Peer.OpenConnectionRequest"/>
    /// specific for <see cref="TcpInitiator"/>.
    /// </summary>
    class TcpOpenConnectionRequest : OpenConnectionRequest
    {
        #region Properties

        /// <summary>
        /// The Connection to open.
        /// </summary>
        public override Connection ConnectionOpen
        {
            set
            {
                IsRedirect = ((TcpInitiator.TcpConnection)value).IsRedirect;
                base.ConnectionOpen = value;
            }
        }

        /// <summary>
        /// True if the TcpInitiator supports redirection.
        /// </summary>
        public bool IsRedirectSupported
        {
            get { return true; }
        }

        /// <summary>
        /// True if the TcpConnection is being opened in response to a
        /// redirection.
        /// </summary>
        public bool IsRedirect { get; set; }

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

            if (reader.ReadBoolean(10)) /* redirect supported? */
            {
                IsRedirect = reader.ReadBoolean(11);
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

            writer.WriteBoolean(10, IsRedirectSupported);
            writer.WriteBoolean(11, IsRedirect);
        }

        #endregion
    }
}
