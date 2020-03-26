/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

namespace Tangosol.IO.Pof.Reflection
{
    /// <summary>
    /// An ICodec provides an interception point for any specific code that 
    /// needs to be executed pre or post (de)serialization. In the case of 
    /// deserialization this could be to return a concrete implementation 
    /// and with serialization this could be to explicitly call a specific 
    /// method on <see cref="IPofWriter"/> that is not carried out by 
    /// <see cref="IPofWriter.WriteObject"/>.
    /// </summary>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <since>Coherence 3.7.1</since>
    public interface ICodec 
    {
        /// <summary>
        /// Deserialize an object from the provided 
        /// <see cref="IPofReader"/>. Implementing this interface allows 
        /// introducing specific return implementations. 
        /// </summary>
        /// <param name="reader">
        /// The <see cref="IPofReader"/> to read from.
        /// </param>    
        /// <param name="index">
        /// The index of the POF property to deserialize.
        /// </param>
        /// <returns>
        /// A specific implementation of the POF property.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        object Decode(IPofReader reader, int index);

        /// <summary>
        /// Serialize an object using the provided 
        /// <see cref="IPofWriter"/>.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="IPofWriter"/>to read from.
        /// </param>
        /// <param name="index">
        /// The index of the POF property to serialize.
        /// </param>
        /// <param name="value">
        /// The value to serialize.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void Encode(IPofWriter writer, int index, object value);
    }
}
