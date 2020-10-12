/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Security.Principal;

using Tangosol.Util;

namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// An IConnection represents a bi-directional communication channel for
    /// exchanging <see cref="IMessage"/> objects between two endpoints.
    /// </summary>
    /// <remarks>
    /// <p>
    /// An IConnection is created by an <see cref="IConnectionInitiator"/>
    /// running on a client. Before an IConnection can be established, the
    /// server must initialize and start an <see cref="IConnectionAcceptor"/>.
    /// Creating the IConnection results in a handshake between the client
    /// and the server (including <see cref="IProtocol"/> negotiation); if
    /// the handshake is successful, the IConnection will be transitioned to
    /// the open state.</p>
    /// <p>
    /// An IConnection itself is not used to send and receive <b>IMessage</b>
    /// objects. Rather, one or more <see cref="IChannel"/> objects may be
    /// created from the IConnection and used to exchange <b>IMessage</b>
    /// objects of a known <b>IProtocol</b> with a peer. An <b>IChannel</b>
    /// may be <see cref="OpenChannel"/> opened to a named
    /// <see cref="IReceiver"/> registered by the peer.
    /// Alternatively, an "anonymous" <b>IChannel</b> (a back-Channel) may be
    /// <see cref="CreateChannel"/> created by one peer and returned to the
    /// other, where it must be <see cref="AcceptChannel"/> accepted
    /// before it can be used.</p>
    /// <p>
    /// Once finished with the IConnection, an application should release all
    /// resources held by the IConnection by calling the
    /// <see cref="Close"/> method. Closing an IConnection also closes all
    /// <b>IChannel</b> objects created by the IConnection and renders the
    /// IConnection unusable. Attempting to use a closed IConnection or any
    /// <b>IChannel</b> created by a closed IConnection may result in an
    /// exception.</p>
    /// <p>
    /// All IConnection implementations must be fully thread-safe.</p>
    /// </remarks>
    /// <author>Jason Howes  2006.03.22</author>
    /// <author>Ana Cikic  2006.08.16</author>
    /// <seealso cref="IChannel"/>
    /// <seealso cref="IReceiver"/>
    /// <seealso cref="IConnectionAcceptor"/>
    /// <seealso cref="IConnectionInitiator"/>
    /// <seealso cref="IMessage"/>
    /// <seealso cref="IProtocol"/>
    /// <since>Coherence 3.2</since>
    public interface IConnection
    {
        /// <summary>
        /// The <see cref="IConnectionManager"/> that created or accepted
        /// this IConnection.
        /// </summary>
        /// <value>
        /// The <b>IConnectionManager</b>.
        /// </value>
        IConnectionManager ConnectionManager { get; }

        /// <summary>
        /// The unique identifier of this IConnection.
        /// </summary>
        /// <value>
        /// The unique identifier of this IConnection or <c>null</c> if the
        /// IConnection has not been accepted.
        /// </value>
        UUID Id { get; }

        /// <summary>
        /// The unique identifier of the peer to which this IConnection
        /// object is connected.
        /// </summary>
        /// <value>
        /// The unique identifier of the peer or <c>null</c> if the
        /// IConnection has not been accepted.
        /// </value>
        UUID PeerId { get; }

        /// <summary>
        /// Return <b>true</b> if this IConnection is open.
        /// </summary>
        /// <remarks>
        /// An IConnection can only be used to exchange data when it is open.
        /// </remarks>
        /// <value>
        /// <b>true</b> if this IConnection is open.
        /// </value>
        bool IsOpen { get; }

        /// <summary>
        /// Close the IConnection.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Closing an IConnection also reclaims all resources held by the
        /// IConnection, so there is no need to close <b>IChannel</b> objects
        /// of a closed IConnection.</p>
        /// <p>
        /// If the IConnection is already closed, calling this method has no
        /// effect.</p>
        /// </remarks>
        void Close();

        /// <summary>
        /// Create an <b>IChannel</b> using a specific <b>IProtocol</b>
        /// through this IConnection to a named <b>IReceiver</b> on the other
        /// end of the IConnection, optionally providing an <b>IPrincipal</b>
        /// to indicate the identity that will be utilizing the
        /// <b>IChannel</b>, and optionally providing an <b>IReceiver</b> that
        /// will process unsolicited <b>IMessage</b> objects on this end of
        /// the <b>IChannel</b>.
        /// </summary>
        /// <remarks>
        /// Conceptually, this is how an <b>IChannel</b> is established to an
        /// existing named "service" (e.g. an <b>IReceiver</b>) on the peer;
        /// note that either peer can register named services and either peer
        /// can use this method to find a named service on its peer.
        /// </remarks>
        /// <param name="protocol">
        /// The <b>IProtocol</b> that will be used to communicate through the
        /// <b>IChannel</b>; the <b>IProtocol</b> is used to verify that the
        /// <b>IReceiver</b> on the peer with the specified name is capable
        /// of communicating using that <b>IProtocol</b>.
        /// </param>
        /// <param name="name">
        /// The name that the <b>IReceiver</b> was registered with, on the
        /// other end of this IConnection; an <b>IReceiver</b> with the
        /// specified name must have been registered with the peer's
        /// <b>IConnectionManager</b> prior to calling this method (see
        /// <see cref="IConnectionManager.RegisterReceiver"/>).
        /// </param>
        /// <param name="receiver">
        /// An optional <b>IReceiver</b> to associate with this
        /// <b>IChannel</b> that will process any unsolicited <b>IMessage</b>
        /// objects sent back through the <b>IChannel</b> by the peer.
        /// </param>
        /// <param name="principal">
        /// An optional <b>IPrincipal</b> to associate with this
        /// <b>IChannel</b>; if specified, any operation performed upon
        /// receipt of an <b>IMessage</b> sent using the returned
        /// <b>IChannel</b> will be done on behalf of the specified
        /// <b>IPrincipal</b>.
        /// </param>
        /// <returns>
        /// A new <b>IChannel</b> object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IProtocol</b> has not been registered with
        /// the underlying <b>IConnectionManager</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If an <b>IReceiver</b> with the given name has not been registered
        /// with the peer's <b>IConnectionManager</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IReceiver</b> does not use the same
        /// <b>IProtocol</b> as the one registered on the peer.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IReceiver</b> does not use the specified
        /// <b>IProtocol</b>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If the IConnection is not open.
        /// </exception>
        IChannel OpenChannel(IProtocol protocol, string name, IReceiver receiver, IPrincipal principal);

        /// <summary>
        /// Create a back-Channel to expose another service to the peer.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method is particularly useful for building a Response
        /// Message to send back a new <b>IChannel</b> that can be used by
        /// the peer. In practice, this means that when a call to a stub is
        /// made, it can easily return a new stub that has its own
        /// <b>IChannel</b>; for example, a stub representing one service can
        /// return a stub representing a different service.</p>
        /// <p>
        /// The new <b>IChannel</b> cannot be used until the returned
        /// <b>Uri</b> is
        /// <see cref="AcceptChannel"/> accepted by the peer.</p>
        /// </remarks>
        /// <param name="protocol">
        /// The <b>IProtocol</b> that will be used to communicate through the
        /// new <b>IChannel</b>.
        /// </param>
        /// <param name="receiver">
        /// An optional <b>IReceiver</b> to associate with the new
        /// <b>IChannel</b> that will process any unsolicited <b>IMessage</b>
        /// objects sent back through the <b>IChannel</b> by the peer.
        /// </param>
        /// <returns>
        /// A <b>Uri</b> that represents the new <b>IChannel</b> object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IProtocol</b> has not been registered with
        /// the underlying <b>IConnectionManager</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IReceiver</b> does not use the specified
        /// <b>IProtocol</b>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If the IConnection is not open.
        /// </exception>
        /// <seealso cref="AcceptChannel"/>
        Uri CreateChannel(IProtocol protocol, IReceiver receiver);

        /// <summary>
        /// Accept a newly created back-Channel that was spawned by the peer.
        /// </summary>
        /// <remarks>
        /// Before a spawned <b>IChannel</b> can be used to send and receive
        /// <b>IMessage</b> objects, its <b>Uri</b> must be accepted by the
        /// peer.
        /// </remarks>
        /// <param name="uri">
        /// The <b>Uri</b> of an <b>IChannel</b> that was spawned by the peer.
        /// </param>
        /// <param name="receiver">
        /// An optional <b>IReceiver</b> to associate with the new
        /// <b>IChannel</b> that will process any unsolicited <b>IMessage</b>
        /// objects sent back through the <b>IChannel</b> by the peer.
        /// </param>
        /// <param name="principal">
        /// An optional <b>IPrincipal</b> to associate with the new
        /// <b>IChannel</b>; if specified, any operation performed upon
        /// receipt of an <b>IMessage</b> sent using the accepted
        /// <b>IChannel</b> will be done on behalf of the specified
        /// <b>IPrincipal</b>.
        /// </param>
        /// <returns>
        /// The newly accepted <b>IChannel</b>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified <b>IReceiver</b> does not use the same
        /// <b>IProtocol</b> as the spawned <b>IChannel</b> (as described by
        /// its <b>Uri</b>).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If the IConnection is not open.
        /// </exception>
        /// <seealso cref="CreateChannel"/>
        IChannel AcceptChannel(Uri uri, IReceiver receiver, IPrincipal principal);

        /// <summary>
        /// Return the open <b>IChannel</b> object with the given identifier.
        /// </summary>
        /// <remarks>
        /// If an <b>IChannel</b> object with the specified identifier does
        /// not exist or has been closed, <c>null</c> is returned.
        /// </remarks>
        /// <param name="id">
        /// The unique <b>IChannel</b> identifier.
        /// </param>
        /// <returns>
        /// The open <b>IChannel</b> object with the specified identifer or
        /// <c>null</c> if no such open <b>IChannel</b> exists.
        /// </returns>
        IChannel GetChannel(int id);

        /// <summary>
        /// Return the collection of open <b>IChannel</b> objects through
        /// this IConnection.
        /// </summary>
        /// <remarks>
        /// The client should assume that the returned collection is an
        /// immutable snapshot of the actual collection of open
        /// <b>IChannel</b> objects maintained by this IConnection.
        /// </remarks>
        /// <returns>
        /// The collection of open <b>IChannel</b> objects.
        /// </returns>
        ICollection GetChannels();
    }
}