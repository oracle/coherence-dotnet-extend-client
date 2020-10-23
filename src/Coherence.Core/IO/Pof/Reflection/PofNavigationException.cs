/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Runtime.Serialization;

namespace Tangosol.IO.Pof.Reflection
{
    /// <summary>
    /// PofNavigationException indicates a failure to navigate a 
    /// <see cref="IPofValue"/> hierarchy.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    [Serializable]
    public class PofNavigationException : PortableException, ISerializable
    {
        /// <summary>
        /// Construct a PofNavigationException.
        /// </summary>
        public PofNavigationException()
        {}

        /// <summary>
        /// Construct a PofNavigationException with a specified detail message.
        /// </summary>
        /// <param name="message">
        /// A detailed message.
        /// </param>
        public PofNavigationException(string message) 
            : base(message)
        {}

        /// <summary>
        /// Construct a PofNavigationException with a specified cause.
        /// </summary>
        /// <param name="cause">
        /// The underlying cause for this exception.
        /// </param>
        public PofNavigationException(Exception cause) 
            : base(cause)
        {}

        /// <summary>
        /// Construct a PofNavigationException with a specified detail message
        /// and a cause.
        /// </summary>
        /// <param name="message">
        /// A detailed message.
        /// </param>
        /// <param name="cause">
        /// The underlying cause for this exception.
        /// </param>
        public PofNavigationException(string message, Exception cause)
            : base(message, cause)
        {}

        /// <summary>
        /// Construct a PofNavigationException class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <b>SerializationInfo</b> that holds the serialized object
        /// data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <b>StreamingContext</b> that contains contextual information
        /// about the source or destination.
        /// </param>
        public PofNavigationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}
