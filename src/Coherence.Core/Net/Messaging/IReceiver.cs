/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// An IReceiver processes unsolicited <see cref="IMessage"/> objects sent
    /// via any number of <see cref="IChannel"/> objects.
    /// </summary>
    /// <remarks>
    /// <p>
    /// An <b>IReceiver</b> acts as a server-side proxy, in that it can be
    /// registered with an <see cref="IConnectionAcceptor"/>, it can be looked
    /// up, and <b>IChannels</b> from multiple clients can be established to
    /// it. In this sense, the IReceiver represents server-side state shared
    /// across any number of client <b>IChannels</b>, and thus provides an
    /// efficient mechanism for demultiplexing multi-client communication
    /// into a shared service proxy, and locating state that is shared across
    /// all of those client <b>IChannels</b>. Conversely, the <b>IChannel</b>
    /// object represents client-specific state, allowing per-client
    /// information to be efficiently managed on the server side.</p>
    /// <p>
    /// While the IReceiver is particularly useful as a server-side proxy, it
    /// is also useful on the client, allowing a client to publish named
    /// services to a server, and in the case of both named services and any
    /// other <b>IChannels</b> created by a client, it allows a client to
    /// efficiently manage stateful communication and process unsolicited
    /// <b>IMessage</b> objects.</p>
    /// </remarks>
    /// <author>Cameron Purdy/Jason Howes  2005.04.17</author>
    /// <author>Ana Cikic  2006.08.15</author>
    /// <since>Coherence 3.2</since>
    public interface IReceiver
    {
        /// <summary>
        /// The name of this IReceiver.
        /// </summary>
        /// <remarks>
        /// If the IReceiver is registered with a
        /// <see cref="IConnectionManager"/>, the registration and any
        /// subsequent accesses are by the IReceiver's name, meaning that the
        /// name must be unique within the domain of the
        /// <b>IConnectionManager</b>.
        /// </remarks>
        /// <value>
        /// The IReceiver name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// The <see cref="IProtocol"/> understood by the IReceiver.
        /// </summary>
        /// <remarks>
        /// Only <b>IChannel</b> objects with the specified <b>IProtocol</b>
        /// can be registered with this IReceiver.
        /// </remarks>
        /// <value>
        /// The <b>IProtocol</b> used by this IReceiver.
        /// </value>
        IProtocol Protocol { get; }

        /// <summary>
        /// Notify this IReceiver that it has been associated with a
        /// <b>IChannel</b>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method is invoked by the <b>IChannel</b> when an IReceiver is
        /// associated with the <b>IChannel</b>.</p>
        /// <p>
        /// Once registered, the IReceiver will receive all unsolicited
        /// <b>IMessage</b> objects sent through the <b>IChannel</b> until
        /// the <b>IChannel</b> is unregistered or closed. Without a
        /// IReceiver, the unsolicited <b>IMessage</b> objects are executed
        /// with only an <b>IChannel</b> as context; with an IReceiver, the
        /// IReceiver is given the <b>IMessage</b> to process, and may
        /// execute the <b>IMessage</b> in turn.</p>
        /// </remarks>
        /// <param name="channel">
        /// An <b>IChannel</b> that has been associated with this IReceiver.
        /// </param>
        void RegisterChannel(IChannel channel);

        /// <summary>
        /// Called when an unsolicited (non-Response) <b>IMessage</b> is
        /// received by an <b>IChannel</b> that had been previously
        /// registered with this IReceiver.
        /// </summary>
        /// <param name="message">
        /// An unsolicited <b>IMessage</b> received by a registered
        /// <b>IChannel</b>.
        /// </param>
        void OnMessage(IMessage message);

        /// <summary>
        /// Unregister the given <b>IChannel</b> with this IReceiver.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method is invoked by the <b>IChannel</b> when an IReceiver is
        /// disassociated with the <b>IChannel</b>.</p>
        /// <p>
        /// Once unregistered, the IReceiver will no longer receive
        /// unsolicited <b>IMessage</b> objects sent through the
        /// <b>IChannel</b>.</p>
        /// </remarks>
        /// <param name="channel">
        /// An <b>IChannel</b> that was disassociated with this IReceiver.
        /// </param>
        void UnregisterChannel(IChannel channel);

        /// <summary>
        /// Notify this IReceiver that the <b>IChannel</b> it was associated with has
        /// been closed.
        /// </summary>
        /// <param name="channel">
        /// An <b>IChannel</b> that was associated with this IReceiver.
        /// </param>
        /// <since>12.2.1.2.0</since>
        void OnChannelClosed(IChannel channel);
    }
}