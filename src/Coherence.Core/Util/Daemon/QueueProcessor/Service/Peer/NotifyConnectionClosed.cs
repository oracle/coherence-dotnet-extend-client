/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer
{
    /// <summary>
    /// This <see cref="IMessage"/> is sent to the peer when a
    /// <see cref="IConnection"/> has been closed.
    /// </summary>
    /// <remarks>
    /// This allows the peer to collect any resources held by the
    /// <b>IConnection</b>.
    /// </remarks>
    public class NotifyConnectionClosed : Message
    {
        #region Properties

        /// <summary>
        /// The optional reason why the <b>IConnection</b> was closed.
        /// </summary>
        /// <value>
        /// The <b>Exception</b> that was the reason why the
        /// <b>IConnection</b> was closed.
        /// </value>
        public virtual Exception Cause
        {
            get { return m_cause; }
            set { m_cause = value; }
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

        #region Message overrides

        /// <summary>
        /// Execute the action specific to the Message implementation.
        /// </summary>
        public override void Run()
        {
            IChannel channel0 = Channel;
            Debug.Assert(channel0.Id == 0);

            Connection connection = (Connection) channel0.Connection;
            Debug.Assert(connection != null);

            connection.Close(false, Cause);
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            Cause = (Exception) reader.ReadObject(0);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteObject(0, Cause);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = 10;

        /// <summary>
        /// The optional reason why the IConnection was closed.
        /// </summary>
        private Exception m_cause;

        #endregion
    }
}