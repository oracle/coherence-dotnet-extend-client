/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Runtime.Serialization;

using Tangosol.IO.Pof;

namespace Tangosol.Util
{
    /// <summary>
    /// Signals that an operation has failed due to a licensing error, such
    /// as a missing license or license limit being exceeded.
    /// </summary>
    /// <author>Jason Howes  2006.09.27</author>
    /// <author>Ana Cikic  2008.07.14</author>
    [Serializable]
    public class LicenseException : PortableException, ISerializable
    {
        /// <summary>
        /// Construct a new LicenseException.
        /// </summary>
        public LicenseException()
        {}

        /// <summary>
        /// Construct a LicenseException with the given detail message.
        /// </summary>
        /// <param name="message">
        /// A detail message.
        /// </param>
        public LicenseException(string message) : base(message)
        {}

        /// <summary>
        /// Construct a LicenseException class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <b>SerializationInfo</b> that holds the serialized object
        /// data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <b>StreamingContext</b> that contains contextual information
        /// about the source or destination.
        /// </param>
        public LicenseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}

    }
}
