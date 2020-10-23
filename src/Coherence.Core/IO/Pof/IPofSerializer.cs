/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// The <b>IPofSerializer</b> interface provides the capability of
    /// reading and writing a .NET object from and to a POF stream.
    /// </summary>
    /// <author>Cameron Purdy, Jason Howes  2006.07.14</author>
    /// <author>Aleksandar Seovic  2006.08.08</author>
    /// <seealso cref="IPofReader"/>
    /// <seealso cref="IPofWriter"/>
    /// <since>Coherence 3.2</since>
    public interface IPofSerializer
    {
        /// <summary>
        /// Serialize a user type instance to a POF stream by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <remarks>
        /// An implementation of <b>IPofSerializer</b> is required to follow
        /// the following steps in sequence for writing out an object of a
        /// user type:
        /// <list type="number">
        /// <item>
        /// <description>
        /// If the object is evolvable, the implementation must set the
        /// version by calling <see cref="IPofWriter.VersionId"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The implementation may write any combination of the properties of
        /// the user type by using the "write" methods of the
        /// <b>IPofWriter</b>, but it must do so in the order of the property
        /// indexes.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// After all desired properties of the user type have been written,
        /// the implementation must terminate the writing of the user type by
        /// calling <see cref="IPofWriter.WriteRemainder"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="writer">
        /// The <b>IPofWriter</b> with which to write the object's state.
        /// </param>
        /// <param name="o">
        /// The object to serialize.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void Serialize(IPofWriter writer, object o);

        /// <summary>
        /// Deserialize a user type instance from a POF stream by reading its
        /// state using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <remarks>
        /// An implementation of <b>IPofSerializer</b> is required to follow
        /// the following steps in sequence for reading in an object of a
        /// user type:
        /// <list type="number">
        /// <item>
        /// <description>
        /// If the object is evolvable, the implementation must get the
        /// version by calling <see cref="IPofReader.VersionId"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The implementation may read any combination of the
        /// properties of the user type by using "read" methods of the
        /// <b>IPofReader</b>, but it must do so in the order of the property
        /// indexes. Additionally, the implementation must call 
        /// {@link IPofReader#RegisterIdentity} with the new instance prior
        /// to reading any properties which are user type instances
        /// themselves.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// After all desired properties of the user type have been read,
        /// the implementation must terminate the reading of the user type by
        /// calling <see cref="IPofReader.ReadRemainder"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="reader">
        /// The <b>IPofReader</b> with which to read the object's state.
        /// </param>
        /// <returns>
        /// The deserialized user type instance.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        object Deserialize(IPofReader reader);
    }
}