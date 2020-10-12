/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

using Tangosol.Util;

namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// An IConnectionAcceptor represents an <see cref="IConnectionManager"/>
    /// running on a server, and as a server, it is responsible for accepting
    /// a connection request initiated by a
    /// <see cref="IConnectionInitiator"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Before a connection can be accepted, the IConnectionAcceptor must be
    /// started using the <see cref="IControllable.Start()"/> method. Calling
    /// this method allocates any necessary resources and transitions the
    /// IConnectionAcceptor to the running state. The IConnectionAcceptor
    /// will then accept new connections, which are represented by
    /// <see cref="IConnection"/> object. The IConnectionAcceptor maintains
    /// references to accepted <b>IConnection</b> objects until they are
    /// closed or the IConnectionAcceptor is
    /// <see cref="IControllable.Shutdown()"/> or
    /// <see cref="IControllable.Stop()"/> terminated. Terminating a
    /// IConnectionAcceptor also closes all accepted <b>IConnection</b>
    /// objects.</p>
    /// <p>
    /// All IConnectionAcceptor implementations must be fully thread-safe.
    /// </p>
    /// </remarks>
    /// <author>Jason Howes  2006.03.23</author>
    /// <author>Goran Milosavljevic  2006.08.15</author>
    /// <seealso cref="IConnection"/>
    /// <seealso cref="IConnectionInitiator"/>
    /// <seealso cref="IConnectionManager"/>
    /// <since>Coherence 3.2</since>
    public interface IConnectionAcceptor : IConnectionManager
    {
        /// <summary>
        /// The collection of open <b>IConnection</b> objects accepted by
        /// this IConnectionAcceptor.
        /// </summary>
        /// <remarks>
        /// The client should assume that the returned collection is an
        /// immutable snapshot of the actual set of <b>IConnection</b>
        /// objects maintained by this IConnectionAcceptor.
        /// </remarks>
        /// <value>
        /// The collection of open <b>IConnection</b> objects.
        /// </value>
        ICollection Connections { get; }
    }
}