/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;
using Tangosol.Util.Collections;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator
{
    /// <summary>
    /// Implementation of
    /// <see cref="Util.Daemon.QueueProcessor.Service.Peer.OpenConnectionResponse"/>
    /// specific for <see cref="Initiator"/>.
    /// </summary>
    public class OpenConnectionResponse : Util.Daemon.QueueProcessor.Service.Peer.OpenConnectionResponse
    {
        /// <summary>
        /// Execute the action specific to the Message implementation.
        /// </summary>
        public override void Run()
        {
            Channel channel0 = (Channel) Channel;
            Debug.Assert(channel0.Id == 0);

            Connection connection = (Connection) channel0.Connection;

            if (IsFailure)
            {
                connection.CloseInternal(false, Result as Exception, -1);
                return;
            }

            Initiator module = (Initiator) channel0.Receiver;
            object[]  ao     = (object[]) Result;

            Debug.Assert(ao != null && ao.Length == 2);

            // extract the "Channel0" configuration from the OpenConnectionRequest
            OpenConnectionRequest request = (OpenConnectionRequest) channel0.GetRequest(RequestId);
            Debug.Assert(request != null);

            connection.Id      = (UUID)ao[0];
            connection.Member  = request.Member;
            connection.PeerId  = (UUID)ao[1];
            channel0.Principal = request.Principal;

            // configure the MessageFactory map for the Connection
            IDictionary mapProtocol = module.ProtocolMap;
            IDictionary mapFactory  = new HashDictionary(mapProtocol.Count);
            IDictionary mapVersion  = ProtocolVersionMap;
            if (mapVersion != null)
            {
                foreach (DictionaryEntry entry in mapVersion)
                {
                    String    name     = (String) entry.Key;
                    Int32     version  = (Int32) entry.Value;
                    IProtocol protocol = (Protocol) mapProtocol[name];

                    mapFactory.Add(name, protocol.GetMessageFactory(version));
                }
            }
            foreach (DictionaryEntry entry in mapProtocol)
            {
                String name = (String) entry.Key;

                if (!mapFactory.Contains(name))
                {
                    IProtocol protocol = (Protocol) entry.Value;
                    mapFactory.Add(name, protocol.GetMessageFactory(protocol.CurrentVersion));
                }
            }
            connection.MessageFactoryMap = mapFactory;

            // the Connection is now ready for use
            module.OnConnectionOpened(connection);
        }
    }
}