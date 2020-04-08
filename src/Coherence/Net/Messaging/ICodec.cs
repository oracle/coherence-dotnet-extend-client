/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO;

namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// An ICodec converts an <see cref="IMessage"/> object to and from a
    /// binary representation.
    /// </summary>
    /// <author>Cameron Purdy/Jason Howes  2006.04.18</author>
    /// <author>Ana Cikic  2006.08.15</author>
    /// <since>Coherence 3.2</since>
    /// <seealso cref="IMessage"/>
    /// <seealso cref="IChannel"/>
    public interface ICodec
    {
        /// <summary>
        /// Encode and write a binary representation of the given
        /// <b>IMessage</b> to the given <see cref="DataWriter"/>.
        /// </summary>
        /// <remarks>
        /// Using the passed <see cref="IChannel"/>, the ICodec has access to
        /// both the <see cref="IMessageFactory"/> for the <b>IChannel</b>
        /// and the underlying <see cref="IConnection"/>.
        /// </remarks>
        /// <param name="channel">
        /// The <b>IChannel</b> object through which the binary-encoded
        /// <b>IMessage</b> was passed.
        /// </param>
        /// <param name="message">
        /// The <b>IMessage</b> to encode.
        /// </param>
        /// <param name="writer">
        /// The <b>DataWriter</b> to write the binary representation of the
        /// <b>IMessage</b> to.
        /// </param>
        /// <exception cref="IOException">
        /// If an error occurs encoding or writing the <b>IMessage</b>.
        /// </exception>
        void Encode(IChannel channel, IMessage message, DataWriter writer);

        /// <summary>
        /// Reads a binary-encoded <b>IMessage</b> from the passed
        /// <see cref="DataReader"/> object.
        /// </summary>
        /// <remarks>
        /// Using the passed <see cref="IChannel"/>, the ICodec has access to
        /// both the <see cref="IMessageFactory"/> for the <b>IChannel</b>
        /// and the underlying <see cref="IConnection"/>.
        /// </remarks>
        /// <param name="channel">
        /// The <b>IChannel</b> object through which the binary-encoded
        /// <b>IMessage</b> was passed.
        /// </param>
        /// <param name="reader">
        /// The <b>DataReader</b> containing the binary-encoded
        /// <b>IMessage</b>.
        /// </param>
        /// <returns>
        /// The <b>IMessage</b> object encoded in the given
        /// <b>DataReader</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// If an error occurs reading or decoding the <b>IMessage</b>.
        /// </exception>
        IMessage Decode(IChannel channel, DataReader reader);
    }
}