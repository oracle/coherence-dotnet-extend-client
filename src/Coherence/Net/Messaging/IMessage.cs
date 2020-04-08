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
    /// IMessage is the root interface for all message objects sent by peer
    /// endpoints through an <see cref="IChannel"/>.
    /// </summary>
    /// <remarks>
    /// IMessage objects are created by an <see cref="IMessageFactory"/>.
    /// An IMessage object has a type identifier that uniquely identifies the
    /// IMessage object class and is scoped to the <b>IMessageFactory</b>
    /// that created the IMessage. In other words, IMessage objects with the
    /// same type identifier that were created by two different
    /// <b>IMessageFactory</b> instances may be of different classes, but
    /// IMessage objects with the same type identifier that were created by
    /// the same <b>IMessageFactory</b> are guaranteed to be of the same
    /// type.
    /// </remarks>
    /// <author>Jason Howes  2006.04.04</author>
    /// <author>Goran Milosavljevic  2006.08.15</author>
    /// <seealso cref="IChannel"/>
    /// <seealso cref="IMessageFactory"/>
    /// <since>Coherence 3.2</since>
    public interface IMessage : IRunnable
    {
        /// <summary>
        /// Return the identifier for this IMessage object's class.
        /// </summary>
        /// <remarks>
        /// The type identifier is scoped to the <b>IMessageFactory</b> that
        /// created this IMessage.
        /// </remarks>
        /// <value>
        /// An identifier that uniquely identifies this IMessage object's
        /// class.
        /// </value>
        int TypeId { get; }

        /// <summary>
        /// Gets or sets the <b>IChannel</b> through which the IMessage will
        /// be sent, was sent, or was received.
        /// </summary>
        /// <value>
        /// The <b>IChannel</b> through which the IMessage will be sent, was
        /// sent, or was received.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If the <b>IChannel</b> has already been set.
        /// </exception>
        IChannel Channel
        {
            get;
            set;
        }

        /// <summary>
        /// Determine if this IMessage should be executed in the same order
        /// as it was received relative to other messages sent through the
        /// same <b>IChannel</b>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Consider two messages: M1 and M2. Say M1 is received before M2
        /// but executed on a different execute thread (for example, when the
        /// <see cref="IConnectionManager"/> is configured with an execute
        /// thread pool of size greater than 1). In this case, there is no
        /// way to guarantee that M1 will finish executing before M2.
        /// However, if M1 returns <b>true</b> from this method, the
        /// <b>IConnectionManager</b> will execute M1 on its service thread,
        /// thus guaranteeing that M1 will execute before M2.</p>
        /// <p>
        /// In-order execution should be considered as a very advanced
        /// feature and implementations that return <b>true</b> from this
        /// method must exercise extreme caution during execution, since any
        /// delay or unhandled exceptions will cause a delay or complete
        /// shutdown of the underlying <b>IConnectionManager</b>.</p>
        /// </remarks>
        /// <returns>
        /// <b>true</b> if the IMessage should be executed in the same order
        /// as it was received relative to other messages.
        /// </returns>
        bool ExecuteInOrder { get; }
    }
}