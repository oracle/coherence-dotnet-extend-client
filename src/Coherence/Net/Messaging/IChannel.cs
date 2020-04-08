/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.Security.Principal;

using Tangosol.IO;

namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// An IChannel is a communication construct that allows one or more
    /// threads to send and receive <see cref="IMessage"/> objects via a
    /// <see cref="IConnection"/>.
    /// </summary>
    /// <remarks>
    /// IChannel objects are created from an <b>IConnection</b>. Once created,
    /// an IChannel can be used to:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// asynchronously <see cref="Send(IMessage)"/> send an <b>IMessage</b>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// asynchronously <see cref="Send(IRequest)"/> send an <b>IRequest</b>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// synchronously <see cref="Request(IRequest)"/> send an <b>IRequest</b>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// asynchronously <see cref="Receiver"/> receive an <b>IMessage</b>
    /// </description>
    /// </item>
    /// </list>
    /// <p>
    /// Once an IChannel has been closed, any attempt to send a
    /// <b>IMessage</b> using the IChannel may result in an exception.</p>
    /// <p>
    /// All IChannel implementations must be fully thread-safe.</p>
    /// </remarks>
    /// <author>Jason Howes  2006.03.23</author>
    /// <author>Ana Cikic  2006.08.15</author>
    /// <seealso cref="IConnection"/>
    /// <seealso cref="IMessage"/>
    /// <seealso cref="IRequest"/>
    /// <seealso cref="IResponse"/>
    /// <seealso cref="IReceiver"/>
    /// <since>Coherence 3.2</since>
    public interface IChannel
    {
        /// <summary>
        /// The <b>IConnection</b> that created this IChannel.
        /// </summary>
        /// <value>
        /// The <b>IConnection</b> that created this IChannel.
        /// </value>
        IConnection Connection { get; }

        /// <summary>
        /// The unique identifier for this IChannel.
        /// </summary>
        /// <remarks>
        /// The returned identifier is only unique among IChannel objects
        /// created from the same underlying <b>IConnection</b>. In other
        /// words, IChannel objects created by different <b>IConnection</b>
        /// objects may have the same unique identifier, but IChannel objects
        /// created by the same <b>IConnection</b> cannot.
        /// </remarks>
        /// <value>
        /// A unique integer identifier for this IChannel.
        /// </value>
        int Id { get; }

        /// <summary>
        /// Return <b>true</b> if this IChannel is open.
        /// </summary>
        /// <value>
        /// <b>true</b> if this IChannel is open.
        /// </value>
        bool IsOpen { get; }

        /// <summary>
        /// Close the IChannel and reclaim all resources held by the
        /// IChannel.
        /// </summary>
        /// <remarks>
        /// <p>
        /// When this method is invoked, it will not return until
        /// <b>IMessage</b> processing has been shut down in an orderly
        /// fashion. This means that the <see cref="IReceiver"/> object
        /// associated with this IChannel (if any) have finished processing
        /// and that all pending requests are completed or canceled. If the
        /// <b>IReceiver</b> is processing an <b>IMessage</b> at the time when
        /// close is invoked, all the facilities of the <b>IChannel</b> must
        /// remain available until it finishes.</p>
        /// <p>
        /// If the IChannel is not open, calling this method has no effect.
        /// </p>
        /// </remarks>
        void Close();

        /// <summary>
        /// The <see cref="IMessageFactory"/> used to create <b>IMessage</b>
        /// objects that may be sent through this IChannel over the
        /// underlying <b>IConnection</b>.
        /// </summary>
        /// <value>
        /// The <b>IMessageFactory</b> for this IChannel.
        /// </value>
        IMessageFactory MessageFactory { get; }

        /// <summary>
        /// The <see cref="ISerializer"/> used to serialize and deserialize
        /// payload objects carried by <b>IMessage</b> objects sent through
        /// this IChannel.
        /// </summary>
        /// <value>
        /// The <b>ISerializer</b> for this IChannel.
        /// </value>
        ISerializer Serializer { get; }

        /// <summary>
        /// The optional <see cref="IReceiver"/> that processes unsolicited
        /// <b>IMessage</b> objects sent through this IChannel over the
        /// underlying <b>IConnection</b>.
        /// </summary>
        /// <value>
        /// The <b>IReceiver</b> for this IChannel or <c>null</c> if a
        /// <b>IReceiver</b> has not been associated with this IChannel.
        /// </value>
        IReceiver Receiver { get; }

        /// <summary>
        /// The optional <b>IPrincipal</b> object associated with this
        /// IChannel.
        /// </summary>
        /// <remarks>
        /// If an <b>IPrincipal</b> is associated with this IChannel, any
        /// operation performed upon receipt of an <b>IMessage</b> sent
        /// through this IChannel will be done on behalf of the
        /// <b>IPrincipal</b>.
        /// </remarks>
        /// <value>
        /// The <b>IPrincipal</b> associated with this IChannel.
        /// </value>
        IPrincipal Principal { get; }

        /// <summary>
        /// Return the object bound with the specified name to this IChannel,
        /// or <c>null</c> if no object is bound with that name.
        /// </summary>
        /// <param name="name">
        /// The name with which the object was bound.
        /// </param>
        /// <returns>
        /// The object bound with the given name or <c>null</c> if no such
        /// binding exists.
        /// </returns>
        object GetAttribute(string name);

        /// <summary>
        /// Return the map of IChannel attributes.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The keys of the map are the names with which the corresponding
        /// values have been bound to the IChannel.</p>
        /// <p>
        /// The client should assume that the returned map is an immutable
        /// snapshot of the actual map of attribute objects maintained by
        /// this IChannel.</p>
        /// </remarks>
        /// <returns>
        /// A map of attributes bound to this IChannel.
        /// </returns>
        IDictionary GetAttributes();

        /// <summary>
        /// Bind an object to the specified name in this IChannel.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If an object is already bound to the specified name, it is
        /// replaced with the given object.</p>
        /// <p>
        /// IChannel attributes are local to the binding peer's IChannel. In
        /// other words, attributes bound to this IChannel object will not be
        /// bound to the peer's IChannel object.</p>
        /// </remarks>
        /// <param name="name">
        /// The name with which to bind the object
        /// </param>
        /// <param name="value">
        /// The object to bind.
        /// </param>
        /// <returns>
        /// The object that the newly bound object replaced (if
        /// any).
        /// </returns>
        object SetAttribute(string name, object value);

        /// <summary>
        /// Unbind the object that was bound with the specified name to this
        /// IChannel.
        /// </summary>
        /// <param name="name">
        /// The name with which the object was bound.
        /// </param>
        /// <returns>
        /// The object that was unbound.
        /// </returns>
        object RemoveAttribute(string name);

        /// <summary>
        /// Asynchronously send an <b>IMessage</b> to the peer endpoint
        /// through this IChannel over the underlying <b>IConnection</b>.
        /// </summary>
        /// <param name="message">
        /// The <b>IMessage</b> to send.
        /// </param>
        void Send(IMessage message);

        /// <summary>
        /// Asynchronously send an <b>IRequest</b> to the peer endpoint
        /// through this IChannel over the underlying <b>IConnection</b>.
        /// </summary>
        /// <param name="request">
        /// The <b>IRequest</b> to send.
        /// </param>
        /// <returns>
        /// An <see cref="IStatus"/> object representing the asynchronous
        /// <b>IRequest</b>.
        /// </returns>
        IStatus Send(IRequest request);

        /// <summary>
        /// Synchronously send an <b>IRequest</b> to the peer endpoint through
        /// this IChannel over the underlying <b>IConnection</b> and return
        /// the result of processing the <b>IRequest</b>.
        /// </summary>
        /// <param name="request">
        /// The <b>IRequest</b> to send.
        /// </param>
        /// <returns>
        /// The result sent by the peer.
        /// </returns>
        object Request(IRequest request);

        /// <summary>
        /// Synchronously send an <b>IRequest</b> to the peer endpoint through
        /// this IChannel over the underlying <b>IConnection</b> and return
        /// the result of processing the <b>IRequest</b>.
        /// </summary>
        /// <param name="request">
        /// The <b>IRequest</b> to send.
        /// </param>
        /// <param name="millis">
        /// The number of milliseconds to wait for the result; pass zero to
        /// block the calling thread indefinitely.
        /// </param>
        /// <returns>
        /// The result sent by the peer.
        /// </returns>
        object Request(IRequest request, long millis);

        /// <summary>
        /// Return the outstanding <b>IRequest</b> with the given identifier
        /// or <c>null</c> if no such <b>IRequest</b> exists.
        /// </summary>
        /// <remarks>
        /// This method can be used during <b>IResponse</b> execution to
        /// correlate the <b>IResponse</b> with the <b>IRequest</b> for which
        /// the <b>IResponse</b> was sent.
        /// </remarks>
        /// <param name="id">
        /// The unique identifer of the outstanding <b>IRequest</b>.
        /// </param>
        /// <returns>
        /// The outstanding <b>IRequest</b> with the given identifer or
        /// <c>null</c> if no such <b>IRequest</b> exists.
        /// </returns>
        IRequest GetRequest(long id);
    }
}