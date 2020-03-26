/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Threading;
using Tangosol.IO;
using Tangosol.Net.Internal;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl.NameService;
using Tangosol.Run.Xml;
using Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// <see cref="INameService"/> implementation that allows a client to
    /// use a remote NameService without having to join the Cluster.
    /// </summary>
    /// <author>Wei Lin  2012.05.23</author>
    /// <since>Coherence 12.1.2</since>
    public class RemoteNameService : RemoteService, INameService, ISerializerFactory, IDisposable
    {
        #region INameService implementation

        /// <summary>
        /// Binds a name to an object.
        /// </summary>
        /// <param name="name">
        /// The name to bind; may not be empty.
        /// </param>
        /// <param name="o">
        /// The object to bind; possibly null.
        /// </param>
        public void Bind(string name, object o)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Retrieves the named object.
        /// </summary>
        /// <param name="name">
        /// The name of the object to look up.
        /// </param>
        /// <returns>
        /// The object bound to name.
        /// </returns>
        public object Lookup(string name)
        {
            IChannel      channel = EnsureChannel();
            LookupRequest request = (LookupRequest) 
                    channel.MessageFactory.CreateMessage(LookupRequest.TYPE_ID);

            request.LookupName = name;

            return channel.Request(request);
        }

        /// <summary>
        /// Unbinds the named object.
        /// </summary>
        /// <param name="name">
        /// The name of the object to unbind.
        /// </param>
        public void Unbind(string name)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region RemoteService override methods

        /// <summary>
        /// Open an <b>IChannel</b> to the remote Service proxy.
        /// </summary>
        /// <seealso cref="RemoteService.OpenChannel"/>
        protected override IChannel OpenChannel()
        {
            IConnection connection = Initiator.EnsureConnection();
            return connection.OpenChannel(NameServiceProtocol.Instance,
                                          "NameService",
                                          null,
                                          Thread.CurrentPrincipal);
        }

        /// <summary>
        /// Gets the type of the <see cref="IService"/>.
        /// </summary>
        /// <value>
        /// The type of the <b>IService</b>.
        /// </value>
        /// <since>Coherence 2.0</since>
        public override ServiceType ServiceType
        {
            get { return Net.ServiceType.RemoteNameService; }
        }

        /// <summary>
        /// The <see cref="RemoteService.Configure"/> implementation method.
        /// </summary>
        /// <remarks>
        /// This method must only be called by a thread that has synchronized
        /// on this RemoteService.
        /// </remarks>
        /// <param name="xml">
        /// The <b>IXmlElement</b> containing the new configuration for this
        /// RemoteService.
        /// </param>
        protected override void DoConfigure(IXmlElement xml)
        {
            base.DoConfigure(xml);

            // register all Protocols
            IConnectionInitiator initiator = Initiator;
            initiator.RegisterProtocol(NameServiceProtocol.Instance);

            if (initiator is Initiator)
            {
                // always use POF for NameService requests
                ((Initiator) initiator).SerializerFactory = this;
            }
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        /// <since>12.2.1</since>
        public void Dispose()
        {
            Shutdown(); // see RemoteService#LookupProxyService
        }

        #endregion

        #region ISerializerFactory implementation

        /// <summary>
        /// Create a new <see cref="ISerializer"/>.
        /// </summary>
        /// <returns>
        /// The new <see cref="ISerializer"/>.
        /// </returns>
        /// <since>12.2.1</since>
        public ISerializer CreateSerializer()
        {
            return NameServicePofContext.INSTANCE;
        }

        #endregion
    }
}
