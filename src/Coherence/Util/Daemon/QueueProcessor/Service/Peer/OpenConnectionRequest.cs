/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using System.Security.Principal;

using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;
using Tangosol.Util.Collections;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    /// <summary>
    /// This <see cref="IRequest"/> is used to open a new
    /// <see cref="IConnection"/>.
    /// </summary>
    public class OpenConnectionRequest : Request
    {
        #region Properties

        /// <summary>
        /// The unique identifier <see cref="UUID"/> of the client that sent
        /// this <b>Request</b>.
        /// </summary>
        /// <value>
        /// The <b>UUID</b> of the client that sent this <b>Request</b>.
        /// </value>
        public virtual UUID ClientId
        {
            get { return m_clientId; }
            set { m_clientId = value; }
        }

        /// <summary>
        /// The name of the cluster the peer wishes to connect to.
        /// </summary>
        /// <since>12.2.1</since>
        public virtual string ClusterName { get; set; }

        /// <summary>
        /// The product edition used by the client.
        /// </summary>
        /// <value>
        /// The product edition used by the client.
        /// </value>
        public virtual int Edition
        {
            get { return m_edition; }
            set { m_edition = value; }
        }

        /// <summary>
        /// An optional token representing a user identity to associate
        /// with "Channel0".
        /// </summary>
        /// <remarks>
        /// Operations performed on receipt of <b>IMessages</b> sent via 
        /// "Channel0" will be performed on behalf of this identity.
        /// </remarks>
        /// <value>
        /// Token representing a user identity.
        /// </value>
        public virtual byte[] IdentityToken
        {
            get { return m_identityToken; }
            set { m_identityToken = value; }
        }

        /// <summary>
        /// The IMember object <see cref="IMember"/> of the client that sent
        /// this <b>Request</b>.
        /// </summary>
        /// <value>
        /// The <b>IMember</b> object of the client that sent this <b>Request</b>.
        /// </value>
        public virtual IMember Member
        {
            get { return m_member; }
            set { m_member = value; }
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
        /// A map of required <see cref="IProtocol"/>s.
        /// </summary>
        /// <remarks>
        /// The keys are the names of the required <b>IProtocol</b>s and the
        /// values are two element Int32 arrays, the first element being the
        /// current version and the second being the supported version of the
        /// corresponding Protocol.
        /// </remarks>
        public virtual IDictionary ProtocolVersionMap
        {
            get { return m_protocolVersionMap; }
            set { m_protocolVersionMap = value; }
        }

        /// <summary>
        /// The name of the service the peer wishes to connect to.
        /// </summary>
        /// <since>12.2.1</since>
        public virtual string ServiceName { get; set; }

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
            return (Response) factory.CreateMessage(OpenConnectionResponse.TYPE_ID);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public OpenConnectionRequest()
        {
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

            ClientId           = (UUID) reader.ReadObject(1);
            Edition            = reader.ReadInt32(2);
            ProtocolVersionMap = reader.ReadDictionary(3, new HashDictionary());
            IdentityToken      = reader.ReadByteArray(4);
            Member             = (IMember) reader.ReadObject(5);
            ClusterName        = reader.ReadString(6);
            ServiceName        = reader.ReadString(7);
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

            writer.WriteObject(1, ClientId);
            writer.WriteInt32(2, Edition);
            writer.WriteDictionary(3, ProtocolVersionMap, typeof(string), typeof(Int32[]));
            writer.WriteByteArray(4, IdentityToken);
            writer.WriteObject(5, Member);
            writer.WriteString(6, ClusterName);
            writer.WriteString(7, ServiceName);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The unique identifier (UUID) of the client that sent this
        /// Request.
        /// </summary>
        private UUID m_clientId;

        /// <summary>
        /// The product edition used by the client.
        /// </summary>
        private int m_edition;

        /// <summary>
        /// An optional token representing a user identity to associate with
        /// "Channel0".
        /// </summary>
        private byte[] m_identityToken;

        /// <summary>
        /// The IMember object of the client that sent this request.
        /// </summary>
        private IMember m_member;

        /// <summary>
        /// The identity under which IMessages received by the new IChannel
        /// will be executed.
        /// </summary>
        [NonSerialized]
        private IPrincipal m_principal;

        /// <summary>
        /// A map of required Protocols. The keys are the names of the
        /// required Protocols and the values are two element Int32 arrays,
        /// the first element being the current version and the second being
        /// the supported version of the corresponding Protocol.
        /// </summary>
        private IDictionary m_protocolVersionMap;

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = 1;

        #endregion
    }
}