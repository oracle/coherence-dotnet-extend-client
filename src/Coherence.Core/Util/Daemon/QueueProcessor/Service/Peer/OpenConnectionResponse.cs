/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    /// <summary>
    /// <see cref="IResponse"/> to an
    /// <see cref="OpenConnectionRequest"/>.
    /// </summary>
    public class OpenConnectionResponse : Response
    {
        #region Properties

        /// <summary>
        /// A map of negotiated <see cref="IProtocol"/>s.
        /// </summary>
        /// <remarks>
        /// The keys are the names of the negotiated <b>IProtocol</b>s and
        /// the values are the negotiated version numbers of the corresponding
        /// Protocol.
        /// </remarks>
        public virtual IDictionary ProtocolVersionMap
        {
            get { return m_protocolVersionMap; }
            set { m_protocolVersionMap = value; }
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

            ProtocolVersionMap = reader.ReadDictionary(6, null);
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

            writer.WriteDictionary(6, ProtocolVersionMap, typeof(string), typeof(Int32));
        }

        #endregion

        #region Data members

        /// <summary>
        /// A map of negotiated Protocols. The keys are the names of the
        /// required Protocols and the values are the negotiated version 
        /// numbers of the corresponding Protocol.
        /// </summary>
        private IDictionary m_protocolVersionMap;

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = 0;

        #endregion
    }
}