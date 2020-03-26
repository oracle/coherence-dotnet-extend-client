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
    /// The <b>IPortableObject</b> interface is implemented by .NET classes
    /// that can self-serialize and deserialize their state to and from a POF
    /// data stream.
    /// </summary>
    /// <remarks>
    /// The <see cref="ReadExternal"/> and <see cref="WriteExternal"/>
    /// methods of the <b>IPortableObject</b> interface are implemented by a
    /// class to give the class complete control its own POF serialization
    /// and deserialization.
    /// </remarks>
    /// <author>Cameron Purdy, Jason Howes  2006.07.13</author>
    /// <author>Aleksandar Seovic  2006.08.08</author>
    /// <seealso cref="IPofReader"/>
    /// <seealso cref="IPofWriter"/>
    /// <since>Coherence 3.2</since>
    public interface IPortableObject
    {
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
        void ReadExternal(IPofReader reader);

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
        void WriteExternal(IPofWriter writer);
    }
}