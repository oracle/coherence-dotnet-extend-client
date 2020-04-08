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
    /// An IMessageFactory is a factory for <see cref="IMessage"/> objects.
    /// </summary>
    /// <author>Jason Howes  2006.04.04</author>
    /// <author>Goran Milosavljevic  2006.08.15</author>
    /// <seealso cref="IMessage"/>
    /// <since>Coherence 3.2</since>
    public interface IMessageFactory
    {
        /// <summary>
        /// Gets the <see cref="IProtocol"/> version supported by this
        /// IMessageFactory.
        /// </summary>
        /// <value>
        /// The <b>IProtocol</b> version associated with this
        /// IMessageFactory.
        /// </value>
        int Version { get; }

        /// <summary>
        /// The <b>IProtocol</b> for which this IMessageFactory creates
        /// <b>IMessage</b> objects.
        /// </summary>
        /// <value>
        /// The <b>IProtocol</b> associated with this IMessageFactory.
        /// </value>
        IProtocol Protocol { get; }

        /// <summary>
        /// Create a new <b>IMessage</b> object of the specified type.
        /// </summary>
        /// <param name="type">
        /// The type identifier of the <b>IMessage</b> class to instantiate.
        /// </param>
        /// <returns>
        /// The new <b>IMessage</b> object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified type is unknown to this IMessageFactory.
        /// </exception>
        IMessage CreateMessage(int type);
    }
}