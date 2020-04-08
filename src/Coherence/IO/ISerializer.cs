/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

namespace Tangosol.IO
{
    /// <summary>
    /// Provides the capability of reading and writing a .NET object from and
    /// to an in-memory buffer.
    /// </summary>
    /// <author>Cameron Purdy/Jason Howes  2007.07.21</author>
    /// <author>Goran Milosavljevic  2006.08.10</author>
    /// <seealso cref="DataReader"/>
    /// <seealso cref="DataWriter"/>
    /// <since>Coherence 3.2</since>
    public interface ISerializer
    {
        /// <summary>
        /// Serialize an object to a stream by writing its state using the
        /// specified <see cref="DataWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>DataWriter</b> with which to write the object's state.
        /// </param>
        /// <param name="o">
        /// The object to serialize.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void Serialize(DataWriter writer, object o);

        /// <summary>
        /// Deserialize an object from a stream by reading its state using
        /// the specified <see cref="DataReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>DataReader</b> with which to read the object's state.
        /// </param>
        /// <returns>
        /// The deserialized user type instance.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        object Deserialize(DataReader reader);
    }
}