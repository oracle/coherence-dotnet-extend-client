/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Net;

namespace Tangosol.Net
{
    /// <summary>
    /// The IAddressProvider is an interface that serves as a means to
    /// provide addresses to a consumer.
    /// </summary>
    /// <remarks>
    /// Simple implementations could be backed by a static list; more
    /// complex ones could use dynamic discovery protocols.
    /// </remarks>
    /// <author>Gene Gleyzer, Jason Howes  2008.08.14</author>
    /// <author>Ana Cikic  2008.08.22</author>
    /// <since>Coherence 3.4</since>
    public interface IAddressProvider
    {
        /// <summary>
        /// Next available address to use.
        /// </summary>
        /// <remarks>
        /// If the caller can successfully use the returned address (e.g. a
        /// connection was established), it should call the
        /// IAddressProvider's <see cref="Accept"/> method.
        /// </remarks>
        /// <value>
        /// The next available address or <c>null</c> if the list of
        /// available addresses was exhausted.
        /// </value>
        IPEndPoint NextAddress { get; }

        /// <summary>
        /// This method should be called by the client immediately after it
        /// determines that it can successfully use an address returned by
        /// the <see cref="NextAddress"/>.
        /// </summary>
        void Accept();

        /// <summary>
        /// This method should be called by the client immediately after it
        /// determines that an attempt to use an address returned by the
        /// <see cref="NextAddress"/> has failed.
        /// </summary>
        /// <param name="eCause">
        /// (Optional) an exception that carries the reason why the caller
        /// rejected the previously returned address.
        /// </param>
        void Reject(Exception eCause);
    }
}