/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// An IProtocol is a binding between a unique name, version information,
    /// and a set of <see cref="IMessage"/> types.
    /// </summary>
    /// <remarks>
    /// <p>
    /// It is used to describe the types of <see cref="IMessage"/> objects
    /// (the "dialect", so to speak) that can be exchanged between two
    /// endpoints through an <see cref="IChannel"/> via a <see
    /// cref="IConnection"/>.</p>
    /// <p>
    /// Before an <b>IConnection</b> can be created or accepted, one or more
    /// IProtocol instances must be registered with the client and
    /// server-side <see cref="IConnectionManager"/>.
    /// During <b>IConnection</b> establishment, the client's
    /// <see cref="IConnectionInitiator"/> sends information about each
    /// registered IProtocol. A compatable set of IProtocol objects
    /// (or superset) must be registered with server's
    /// <b>IConnectionManager</b> in order for the the <b>IConnection</b> to
    /// be accepted.</p>
    /// </remarks>
    /// <author>Jason Howes  2006.04.11</author>
    /// <author>Goran Milosavljevic  2006.08.15</author>
    /// <seealso cref="IChannel"/>
    /// <seealso cref="IConnection"/>
    /// <seealso cref="IConnectionAcceptor"/>
    /// <seealso cref="IConnectionInitiator"/>
    /// <seealso cref="IConnectionManager"/>
    /// <since>Coherence 3.2</since>
    public interface IProtocol
    {
        /// <summary>
        /// Gets the unique name of this IProtocol.
        /// </summary>
        /// <remarks>
        /// This name serves as a unique identifier for the IProtocol;
        /// therefore, only a single instance of an IProtocol with a given
        /// name may be registered with an <b>IConnectionManager</b>.
        /// </remarks>
        /// <value>
        /// The IProtocol name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Determine the newest protocol version supported by this
        /// IProtocol.
        /// </summary>
        /// <value>
        /// The version number of this IProtocol.
        /// </value>
        int CurrentVersion { get; }

        /// <summary>
        /// Determine the oldest protocol version supported by this
        /// IProtocol.
        /// </summary>
        /// <value>
        /// The oldest protocol version that this IProtocol object supports.
        /// </value>
        int SupportedVersion { get; }

        /// <summary>
        /// Return an <see cref="IMessageFactory"/> that can be used to create
        /// <b>IMessage</b> objects for the specified version of this
        /// IProtocol.
        /// </summary>
        /// <param name="version">
        /// The desired IProtocol version.
        /// </param>
        /// <returns>
        /// An <b>IMessageFactory</b> that can create <b>IMessage</b> objects
        /// for the specified version of this IProtocol.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified protocol version is not supported by this
        /// IProtocol.
        /// </exception>
        IMessageFactory GetMessageFactory(int version);
    }
}