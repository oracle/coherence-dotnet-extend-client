/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Security.Principal;

using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    /// <summary>
    /// Internal <see cref="IRequest"/> used to open an
    /// <see cref="IConnection"/>.
    /// </summary>
    public class OpenConnection : Request
    {
        #region Properties

        /// <summary>
        /// The <see cref="Connection"/> to open.
        /// </summary>
        /// <value>
        /// The <b>Connection</b> to open.
        /// </value>
        public virtual Connection ConnectionOpen
        {
            get { return m_connectionOpen; }
            set { m_connectionOpen = value; }
        }

        /// <summary>
        /// The token representing a user's identity.
        /// </summary>
        /// <value>
        /// The token representing a user's identity.
        /// </value>
        public virtual byte[] IdentityToken
        {
            get { return m_identityToken; }
            set { m_identityToken = value; }
        }

        /// <summary>
        /// The identity under which messages received by "Channel0" will be 
        /// executed.
        /// </summary>
        /// <value>
        /// The identity under which messages received by "Channel0" will be 
        /// executed.
        /// </value>
        public virtual IPrincipal Principal
        {
            get { return m_principal; }
            set { m_principal = value; }
        }

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

        #region Request overrides

        /// <summary>
        /// Create a new <see cref="Response"/> for this
        /// IRequest.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="IMessageFactory"/> that must be used to create the
        /// returned <b>IResponse</b>; never <c>null</c>.
        /// </param>
        /// <returns>
        /// A new <b>Response</b>.
        /// </returns>
        protected override Response InstantiateResponse(IMessageFactory factory)
        {
            return (Response) factory.CreateMessage(InternalResponse.TYPE_ID);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = -8;

        /// <summary>
        /// The Connection to open.
        /// </summary>
        [NonSerialized]
        private Connection m_connectionOpen;

        /// <summary>
        /// A token representing a user's identity.
        /// </summary>
        private byte[] m_identityToken;

        /// <summary>
        /// The identity under which IMessages received by "Channel0" will be 
        /// executed.
        /// </summary>
        [NonSerialized]
        private IPrincipal m_principal;

        #endregion
    }
}