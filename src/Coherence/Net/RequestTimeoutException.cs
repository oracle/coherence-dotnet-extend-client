/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

using Tangosol.IO.Pof;

namespace Tangosol.Net
{
    /// <summary>
    /// Signals that a request execution in a clustered environment did not
    /// complete in a pre-determined amount of time.
    /// </summary>
    /// <remarks>
    /// For some specific requests this exception could carry a partial
    /// execution result.
    /// </remarks>
    /// <author>Gene Gleyzer  2006.11.02</author>
    /// <author>Ana Cikic  2007.12.13</author>
    /// <since>Coherence 3.3</since>
    /// <seealso cref="IPriorityTask"/>
    [Serializable]
    public class RequestTimeoutException : RequestIncompleteException
    {
        #region Constructors

        /// <summary>
        /// Constructs a RequestTimeoutException with no detail message.
        /// </summary>
        public RequestTimeoutException()
        {}

        /// <summary>
        /// Constructs a RequestTimeoutException with the specified detail
        /// message.
        /// </summary>
        /// <param name="s">
        /// The string that contains a detailed message.
        /// </param>
        public RequestTimeoutException(string s) : base(s)
        {}

        /// <summary>
        /// Construct a RequestTimeoutException from an <b>Exception</b>
        /// object.
        /// </summary>
        /// <param name="e">
        /// The <b>Exception</b> object.
        /// </param>
        public RequestTimeoutException(Exception e) : base(null, e)
        {}

        /// <summary>
        /// Construct a RequestTimeoutException from a <b>Exception</b>
        /// object and an additional description.
        /// </summary>
        /// <param name="s">
        /// The additional description.
        /// </param>
        /// <param name="e">
        /// The <b>Exception</b> object.
        /// </param>
        public RequestTimeoutException(string s, Exception e) : base(s, e)
        {}

        /// <summary>
        /// Construct a RequestTimeoutException class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <b>SerializationInfo</b> that holds the serialized object
        /// data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <b>StreamingContext</b> that contains contextual information
        /// about the source or destination.
        /// </param>
        public RequestTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}

        #endregion
    }
}