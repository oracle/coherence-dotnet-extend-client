/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Runtime.Serialization;

using Tangosol.IO.Pof;

namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// Signals that an underlying communication channel used by a
    /// <see cref="IConnection"/> may have been closed, severed, or become
    /// unusable.
    /// </summary>
    /// <remarks>
    /// After this exception is thrown, any attempt to use the
    /// <b>IConnection</b> (or any <see cref="IChannel"/> created by the
    /// <b>IConnection</b>) may result in an exception.
    /// </remarks>
    /// <author>Jason Howes  2006.06.08</author>
    /// <author>Goran Milosavljevic  2006.08.15</author>
    /// <since>Coherence 3.2</since>
    [Serializable]
    public class ConnectionException : PortableException, ISerializable
    {
        /// <summary>
        /// Construct a ConnectionException with no detail message.
        /// </summary>
        public ConnectionException()
        {}

        /// <summary>
        /// Construct a ConnectionException with the specified detail
        /// message.
        /// </summary>
        /// <param name="s">
        /// The string that contains a detailed message.
        /// </param>
        public ConnectionException(string s) : base(s)
        {}

        /// <summary>
        /// Construct a ConnectionException with the specified detail
        /// message.
        /// </summary>
        /// <param name="s">
        /// The string that contains a detailed message.
        /// </param>
        /// <param name="connection">
        /// The connection where the error occured
        /// </param>        
        public ConnectionException(string s, IConnection connection)
            : base(connection == null ? s : s == null ? connection.ToString()
                                                      : connection + ": " + s)
        {}

        /// <summary>
        /// Construct a ConnectionException from an <b>Exception</b> object.
        /// </summary>
        /// <param name="e">
        /// The Exception object.
        /// </param>
        public ConnectionException(Exception e) : base(e)
        {}

        /// <summary>
        /// Construct a ConnectionException from an <b>Exception</b> object.
        /// </summary>
        /// <param name="e">
        /// The Exception object.
        /// </param>
        /// <param name="connection">
        /// The connection where the error occured
        /// </param> 
        public ConnectionException(Exception e, IConnection connection)
            : base(connection == null ? null : connection.ToString(), e)
        {}

        /// <summary>
        /// Construct a ConnectionException from an <b>Exception</b> object
        /// and an additional description.
        /// </summary>
        /// <param name="s">
        /// The additional description.
        /// </param>
        /// <param name="e">
        /// The <b>Exception</b> object.
        /// </param>
        public ConnectionException(string s, Exception e) : base(s, e)
        {}

        /// <summary>
        /// Construct a ConnectionException from an <b>Exception</b> object
        /// and an additional description.
        /// </summary>
        /// <param name="s">
        /// The additional description.
        /// </param>
        /// <param name="e">
        /// The <b>Exception</b> object.
        /// </param>
        /// <param name="connection">
        /// The connection where the error occured
        /// </param> 
        public ConnectionException(string s, Exception e, IConnection connection)
            : base(connection == null ? s : s == null ? connection.ToString()
                                                      : connection + ": " + s, e)
        {}

        /// <summary>
        /// Construct a ConnectionException class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <b>SerializationInfo</b> that holds the serialized object
        /// data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <b>StreamingContext</b> that contains contextual information
        /// about the source or destination.
        /// </param>
        public ConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}