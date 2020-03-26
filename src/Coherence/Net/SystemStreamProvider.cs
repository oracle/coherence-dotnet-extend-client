/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.IO;
using System.Net.Sockets;
using Tangosol.Run.Xml;

namespace Tangosol.Net
{
    /// <summary>
    /// Will retrun the default (unsecure) network stream.
    /// </summary>
    public class SystemStreamProvider : IStreamProvider
    {
        #region IStreamProvider implementation
        /// <summary>
        /// Get a default unsecure stream (NetworkStream) from an established connection (TcpClient).
        /// </summary>
        /// <param name="client">A connected TcpClient, used to establish a unsecure connection.</param>
        /// <returns>A NetworkStream connected to the remote host.</returns>
        public Stream GetStream(TcpClient client)
        {
            return client.GetStream();
        }
        #endregion

        #region IXmlConfigurable implementation

        /// <summary>
        /// The current configuration of the object.
        /// </summary>
        /// <value>
        /// The XML configuration or <c>null</c>.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// When setting, if the object is not in a state that allows the
        /// configuration to be set; for example, if the object has already
        /// been configured and cannot be reconfigured.
        /// </exception>
        public IXmlElement Config
        {
            get;
            set;
        }

        #endregion
    }
}
