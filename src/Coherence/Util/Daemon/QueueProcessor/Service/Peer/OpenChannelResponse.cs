/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Diagnostics;
using System.Security.Principal;

using Tangosol.IO;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    /// <summary>
    /// <see cref="IResponse"/> to an
    /// <see cref="OpenChannelRequest"/>.
    /// </summary>
    public class OpenChannelResponse : Response
    {
        #region Properties

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

        #endregion

        #region Message overrides

        /// <summary>
        /// Execute the action specific to the Message implementation.
        /// </summary>
        public override void Run()
        {
            Channel channel0 = (Channel) Channel;
            Debug.Assert(channel0.Id == 0);

            if (IsFailure)
            {
                return;
            }

            // extract the new Channel configuration from the OpenChannelRequest
            OpenChannelRequest request = (OpenChannelRequest) channel0.GetRequest(RequestId);
            Debug.Assert(request != null);

            Connection      connection = (Connection) channel0.Connection;
            IMessageFactory factory    = request.MessageFactory;
            IReceiver       receiver   = request.Receiver;
            ISerializer     serializer = request.Serializer;
            IPrincipal      principal  = request.Principal;
            int             id         = (int) Result;

            Result = connection.OpenChannelResponse(id, factory, serializer, receiver, principal);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = 12;

        #endregion
    }
}