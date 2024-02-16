/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.Util;

namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// An IConnectionInitiator represents an <see cref="IConnectionManager"/>
    /// running on a client, and as a client, it is responsible for
    /// initiating the connection process.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Before a connection can be established, the IConnectionInitiator must
    /// be started using the <see cref="IControllable.Start()"/> method.
    /// Calling this method allocates any necessary resources and transitions
    /// the IConnectionInitiator to the running state. Additionally, the
    /// server endpoint must have initialized and started a
    /// <see cref="IConnectionAcceptor"/>. The IConnectionInitiator can then
    /// be used to establish a connection to the server's
    /// <b>IConnectionAcceptor</b>, which is represented by a single
    /// <see cref="IConnection"/> object, obtained by calling
    /// <see cref="EnsureConnection()"/>. The IConnectionInitiator maintains
    /// a reference to the <b>IConnection</b> object until it is closed or
    /// the IConnectionInitiator is <see cref="IControllable.Shutdown()"/> or
    /// <see cref="IControllable.Stop()"/> terminated.</p>
    /// <p>
    /// All IConnectionInitiator implementations must be fully thread-safe.
    /// </p>
    /// </remarks>
    /// <author>Jason Howes  2006.03.2</author>
    /// <author>Goran Milosavljevic  2006.08.15</author>
    /// <seealso cref="IConnection"/>
    /// <seealso cref="IConnectionAcceptor"/>
    /// <seealso cref="IConnectionManager"/>
    /// <since>Coherence 3.2</since>
    public interface IConnectionInitiator : IConnectionManager
    {
        /// <summary>
        /// Create a new or return the existing <b>IConnection</b> object.
        /// </summary>
        /// <remarks>
        /// <p>
        /// An <b>IConnection</b> object has a one-way state transition from
        /// open to closed; this method will always return an open
        /// <b>IConnection</b> object. If the previously existing
        /// <b>IConnection</b> object has transitioned to a closed state,
        /// this method will return a new <b>IConnectin</b> object in the
        /// open state.</p>
        /// </remarks>
        /// <returns>
        /// An <b>IConnection</b> object representing a client's connection to
        /// a server.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the IConnectionInitiator is not running.
        /// </exception>
        IConnection EnsureConnection();
    }
}