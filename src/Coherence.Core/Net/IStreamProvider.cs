/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.IO;
using System.Net.Sockets;
using Tangosol.Run.Xml;

namespace Tangosol.Net
{
    /// <summary>
    /// IStreamProvider provides an abstraction for configuring an getting
    /// NetworkStreams for the TcpInitiator.
    /// <br/>
    /// INetworkStreamFactories are provided by the StreamProviderFactory.
    /// </summary>
    public interface IStreamProvider : IXmlConfigurable
    {
        /// <summary>
        /// Get a NetworkStream using a <b>TcpClient</b> which is connected.
        /// </summary>
        /// <param name="client">A connected <b>TcpClient</b>.</param>
        /// <returns>A <b>Stream</b></returns>
        Stream GetStream(TcpClient client);

        /// <summary>
        /// address of a remote server this client is connected to.
        /// </summary>
        string RemoteAddress { get; set; }
    }
}
