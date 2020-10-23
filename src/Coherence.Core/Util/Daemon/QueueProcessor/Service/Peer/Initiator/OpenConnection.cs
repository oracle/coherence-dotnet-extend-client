/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;

using Tangosol.Net.Impl;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator
{
    /// <summary>
    /// Implementation of
    /// <see cref="Util.Daemon.QueueProcessor.Service.Peer.OpenConnection"/>
    /// specific for <see cref="Initiator"/>.
    /// </summary>
    public class OpenConnection : Util.Daemon.QueueProcessor.Service.Peer.OpenConnection
    {
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
            Connection connection = ConnectionOpen;
            Debug.Assert(!connection.IsOpen);

            Initiator module = (Initiator) Channel.Receiver;

            connection.OpenInternal();

            try
            {
                IChannel        channel0 = connection.GetChannel(0);
                IMessageFactory factory0 = channel0.MessageFactory;

                // sent a OpenConnectionRequest to the peer via "Channel0"
                OpenConnectionRequest request = (OpenConnectionRequest) factory0.CreateMessage(
                        Util.Daemon.QueueProcessor.Service.Peer.OpenConnectionRequest.TYPE_ID);

                request.ClientId           = Initiator.ProcessId;
                request.ConnectionOpen     = connection;
                request.Edition            = module.OperationalContext.Edition;
                request.IdentityToken      = IdentityToken;
                request.Member             = module.OperationalContext.LocalMember;
                request.Principal          = Principal;
                request.ProtocolVersionMap = module.ProtocolVersionMap;

                IService svcParent = module.ParentService;
                if (svcParent is RemoteService)
                {
                    RemoteService svcRemote = (RemoteService) svcParent;
                    request.ClusterName = svcRemote.RemoteClusterName;
                    request.ServiceName = svcRemote.RemoteServiceName;
                }

                response.Result = channel0.Send(request);
            }
            catch (Exception e)
            {
                connection.CloseInternal(false, e, -1);
                throw;
            }
        }
    }
}