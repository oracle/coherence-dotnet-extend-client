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
    /// IRequest is the root interface for all request messages sent by peer
    /// endpoints through an <see cref="IChannel"/>.
    /// </summary>
    /// <remarks>
    /// IRequest object is created by an <see cref="IMessageFactory"/> and
    /// has an identifier that uniquely identifies the
    /// <see cref="IMessage"/> instance.
    /// </remarks>
    /// <author>Jason Howes  2006.04.05</author>
    /// <author>Goran Milosavljevic  2006.08.15</author>
    /// <seealso cref="IMessageFactory"/>
    /// <seealso cref="IMessage"/>
    /// <since>Coherence 3.2</since>
    public interface IRequest : IMessage
    {
        /// <summary>
        /// The unique identifier for this IRequest.
        /// </summary>
        /// <value>
        /// An identifier that uniquely identifies this IRequest object.
        /// </value>
        long Id
        {
            get;
            set;
        }

        /// <summary>
        /// The <see cref="IStatus"/> for this IRequest that can be used to
        /// wait for and retrieve the <see cref="IResponse"/>.
        /// </summary>
        /// <value>
        /// The <b>IStatus</b> object or <c>null</c> if object hasn't been
        /// initialized.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If the status has already been configured.
        /// </exception>
        IStatus Status
        {
            get;
            set;
        }

        /// <summary>
        /// Return the <see cref="IResponse"/> for this IRequest.
        /// </summary>
        /// <remarks>
        /// If not already available, the <b>IResponse</b> must be created
        /// using the <see cref="IMessageFactory"/> associated with the
        /// <see cref="IChannel"/> that this IRequest was sent through.
        /// </remarks>
        /// <returns>
        /// The <b>IResponse</b>; must not be <c>null</c>.
        /// </returns>
        IResponse EnsureResponse();
    }
}