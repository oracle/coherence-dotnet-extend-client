/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

using Tangosol.IO.Pof;

namespace Tangosol.Net
{
    /// <summary>
    /// Signals that a request execution in a distributed environment failed to
    /// complete successfully.
    /// </summary>
    /// <remarks>
    /// For some specific requests this exception could carry a partial
    /// execution result or failure information.
    /// </remarks>
    /// <author>Bin Chen  2013.05.16</author>
    /// <since>Coherence 12.1.3</since>
    /// <seealso cref="IPriorityTask"/>
    [Serializable]
    public class RequestIncompleteException : PortableException, ISerializable
    {
        #region Properties

        /// <summary>
        /// A partial execution result that may have been assembled prior
        /// to an exception.
        /// </summary>
        /// <value>
        /// A partial execution result (optional).
        /// </value>
        public virtual object PartialResult
        {
            get { return m_partialResult; }
            set { m_partialResult = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a RequestIncompleteException with no detail message.
        /// </summary>
        public RequestIncompleteException()
        { }

        /// <summary>
        /// Constructs a RequestIncompleteException with the specified detail
        /// message.
        /// </summary>
        /// <param name="s">
        /// The string that contains a detailed message.
        /// </param>
        public RequestIncompleteException(string s)
            : base(s)
        { }

        /// <summary>
        /// Construct a RequestIncompleteException from an <b>Exception</b>
        /// object.
        /// </summary>
        /// <param name="e">
        /// The <b>Exception</b> object.
        /// </param>
        public RequestIncompleteException(Exception e)
            : base(null, e)
        { }

        /// <summary>
        /// Construct a RequestIncompleteException from a <b>Exception</b>
        /// object and an additional description.
        /// </summary>
        /// <param name="s">
        /// The additional description.
        /// </param>
        /// <param name="e">
        /// The <b>Exception</b> object.
        /// </param>
        public RequestIncompleteException(string s, Exception e)
            : base(s, e)
        { }

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
        public RequestIncompleteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            m_partialResult = info.GetValue("PartialResult", typeof(object));
        }

        #endregion

        /// <summary>
        /// Sets the <b>SerializationInfo</b> with information about the
        /// exception.
        /// </summary>
        /// <param name="info">
        /// The <b>SerializationInfo</b> that holds the serialized object
        /// data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <b>StreamingContext</b> that contains contextual information
        /// about the source or destination.
        /// </param>
#if NET8_0_OR_GREATER
        [Obsolete("Obsolete as of Coherence 14.1.2.0. This API uses obsolete formatter-based serialization. It should not be called or extended by application code. (https://aka.ms/dotnet-warnings/SYSLIB0051)")]
#endif
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("PartialResult", m_partialResult);
        }

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);
            m_partialResult = reader.ReadObject(4);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);
            writer.WriteObject(4, m_partialResult);
        }

        #endregion

        #region Data members

        /// <summary>
        /// Partial execution result (optional).
        /// </summary>
        private object m_partialResult;

        #endregion
    }
}