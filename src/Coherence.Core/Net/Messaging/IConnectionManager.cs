/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// Represents the method that will handle connection event.
    /// </summary>
    /// <param name="sender">
    /// <see cref="IConnectionManager"/> that raised an event.
    /// </param>
    /// <param name="args">
    /// Event arguments.
    /// </param>
    public delegate void ConnectionEventHandler(object sender, ConnectionEventArgs args);

    /// <summary>
    /// The IConnectionManager is the base SPI (Service Provider Interface)
    /// for both <see cref="IConnectionAcceptor"/> and
    /// <see cref="IConnectionInitiator"/> implementations.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Implementations of this interface use a provider-specific mechanism
    /// to establish a bi-directional communication channel between two
    /// endpoints, represented by an <see cref="IConnection"/>. Some
    /// implementations restrict data transfer between endpoints within a
    /// single JVM, whereas others enable two processes to exchange data.
    /// Advanced implementations allow communication between processes on
    /// different machines, for example using TCP sockets or JMS.</p>
    /// <p>
    /// Before an <b>IConnection</b> can be established between a
    /// <b>IConnectionInitiator</b> (client) and <b>IConnectionAcceptor</b>
    /// (server), one or more <see cref="IProtocol"/> instances must be
    /// registered with the IConnectionManager on each. During
    /// <b>IConnection</b> establishment, the <b>IConnectionInitiator</b>
    /// sends information about each registered <b>IProtocol</b>. A
    /// compatable set of <b>IProtocol</b> instances (or superset) must
    /// be registered with the acceptor's IConnectionManager in order for the
    /// <b>IConnection</b> to be established.</p>
    /// <p>
    /// Establishing an <b>IConnection</b> is assumed to be a heavyweight
    /// operation that may allocate significant resources within and outside
    /// the JVM. For example, a TCP-based implementation of this interface
    /// may implement an <b>IConnection</b> using a persistent Socket
    /// connection with a remote server. However, once established,
    /// successive uses of the same <b>IConnection</b> should be relatively
    /// lightweight. In other words, an <b>IConnection</b> object, once
    /// opened, should appear to be persistent from the perspective of the
    /// user until closed. Additionally, underlying transports used by
    /// implementations must be both reliable and ordered.</p>
    /// <p>
    /// Once an <b>IConnection</b> is established, either client or server may
    /// open an <see cref="IChannel"/> to an <see cref="IReceiver"/> registered
    /// by its peer and use it to send and receive <see cref="IMessage"/>
    /// objects to/from the peer.</p>
    /// </remarks>
    /// <author>Jason Howes  2006.03.30</author>
    /// <author>Goran Milosavljevic  2006.08.15</author>
    /// <seealso cref="IChannel"/>
    /// <seealso cref="IReceiver"/>
    /// <seealso cref="IConnection"/>
    /// <seealso cref="IConnectionAcceptor"/>
    /// <seealso cref="IConnectionInitiator"/>
    /// <seealso cref="IProtocol"/>
    /// <since>Coherence 3.2</since>
    public interface IConnectionManager : Util.IService
    {
        /// <summary>
        /// The <see cref="IOperationalContext"/> used by this
        /// <b>IConnectionManager</b>.
        /// </summary>
        /// <value>
        /// The <see cref="IOperationalContext"/> used by this
        /// <b>IConnectionManager</b>.
        /// </value>
        IOperationalContext OperationalContext { get; set; }

        /// <summary>
        /// Gets a map of <b>IProtocol</b> names to <b>IProtocol</b> objects.
        /// </summary>
        /// <remarks>
        /// The client should assume that the returned map is an immutable
        /// snapshot of the actual map of <b>IProtocol</b> objects maintained
        /// by this IConnectionManager.
        /// </remarks>
        /// <value>
        /// A map of all registered <b>IProtocol</b> objects, keyed by the
        /// <b>IProtocol</b> name.
        /// </value>
        IDictionary Protocols { get; }

        /// <summary>
        /// Gets a map of <b>IReceiver</b> names to <b>IReceiver</b> objects.
        /// </summary>
        /// <remarks>
        /// The client should assume that the returned map is an immutable
        /// snapshot of the actual map of <b>IReceiver</b> objects maintained
        /// by this IConnectionManager.
        /// </remarks>
        /// <value>
        /// A map of all registered <b>IReceiver</b> objects, keyed by the
        /// <b>IReceiver</b> name.
        /// </value>
        IDictionary Receivers { get; }

        /// <summary>
        /// The <see cref="ICodec"/> that will be used to encode and decode
        /// <b>IMessages</b> sent through <b>IConnections</b> managed by this
        /// IConnectionManager.
        /// </summary>
        /// <value>
        /// The <see cref="ICodec"/> object.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If the IConnectionManager is running.
        /// </exception>
        ICodec Codec { get; set; }

        /// <summary>
        /// Gets an <b>IProtocol</b> that was registered with this
        /// IConnectionManager.
        /// </summary>
        /// <param name="name">
        /// The name of the registered <b>IProtocol</b>.
        /// </param>
        /// <returns>
        /// The registered <b>IProtocol</b> or <c>null</c> if a
        /// <b>IProtocol</b> with the given name is not registered with this
        /// IConnectionManager.
        /// </returns>
        IProtocol GetProtocol(string name);

        /// <summary>
        /// Register an <b>IProtocol</b> with this IConnectionManager.
        /// </summary>
        /// <remarks>
        /// This method may only be called before the IConnectionManager
        /// is started.
        /// </remarks>
        /// <param name="protocol">
        /// The new <b>IProtocol</b> to register; if the <b>IProtocol</b> has
        /// already been registered, this method has no effect.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If the IConnectionManager is running.
        /// </exception>
        void RegisterProtocol(IProtocol protocol);

        /// <summary>
        /// Return an <b>IReceiver</b> that was registered with this
        /// IConnectionManager.
        /// </summary>
        /// <remarks>
        /// The client should assume that the returned map is an immutable
        /// snapshot of the actual map of <b>IReceiver</b> objects maintained
        /// by this IConnectionManager.
        /// </remarks>
        /// <param name="name">
        /// The name of the registered <b>IReceiver</b>.
        /// </param>
        /// <returns>
        /// The registered <b>IReceiver</b> or <c>null</c> if a
        /// <b>IReceiver</b> with the given name is not registered with this
        /// IConnectionManager.
        /// </returns>
        IReceiver GetReceiver(string name);

        /// <summary>
        /// Register an <b>IReceiver</b> that will received unsolicited
        /// <b>IMessage</b> objects sent through <b>IChannel</b> objects
        /// associated with the <b>IReceiver</b> name and <b>IProtocol</b>.
        /// </summary>
        /// <remarks>
        /// This method may only be called before the IConnectionManager is
        /// started.
        /// </remarks>
        /// <param name="receiver">
        /// The new <b>IReceiver</b> to register; if the <b>IReceiver</b> has
        /// already been registered, this method has no effect.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If the IConnectionManager is running.
        /// </exception>
        void RegisterReceiver(IReceiver receiver);
        
        /// <summary>
        /// Invoked after an <see cref="IConnection"/> has been successfully
        /// established.
        /// </summary>
        event ConnectionEventHandler ConnectionOpened;
        
        /// <summary>
        /// Invoked after an <see cref="IConnection"/> is closed.
        /// </summary>
        /// <remarks>s
        /// After this event is raised, any attempt to use the
        /// <b>IConnection</b> (or any <b>IChannel</b> created by the
        /// <b>IConnection</b>) may result in an exception.
        /// </remarks>
        event ConnectionEventHandler ConnectionClosed;

        /// <summary>
        /// Invoked when the <b>IConnectionManager</b> detects that the
        /// underlying communication channel has been closed by the peer,
        /// severed, or become unusable.
        /// </summary>
        /// <remarks>
        /// After this event is raised, any attempt to use the
        /// <b>IConnection</b> (or any <b>IChannel</b> created by the
        /// <b>IConnection</b>) may result in an exception.
        /// </remarks>
        event ConnectionEventHandler ConnectionError;
    }
}