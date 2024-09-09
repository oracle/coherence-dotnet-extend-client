/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tangosol.IO
{
    /// <summary>
    /// <see cref="ISerializer"/> implementation that uses .NET binary
    /// serializer.
    /// </summary>
    /// <remarks>
    /// As of 14.1.2.0, this class is deprecated as it relies on
    /// <see cref="BinaryFormatter"/>,  which is scheduled for removal
    /// in .NET 9.
    /// </remarks>
    /// <author>Aleksandar Seovic  2009.06.22</author>
    /// <since>Coherence 3.5</since>
    [Obsolete("since Coherence 14.1.2.0")]
    public class BinarySerializer : ISerializer
    {
        #region Implementation of ISerializer

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
        public void Serialize(DataWriter writer, object o)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(writer.BaseStream, o);
        }

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
        public object Deserialize(DataReader reader)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(reader.BaseStream);
        }

        #endregion
    }
}