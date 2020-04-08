/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// <see cref="IPofSerializer"/> implementation that can serialize and
    /// deserialize a <see cref="Exception"/> to/from a POF stream.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This serializer provides a catch-all mechanism for serializing
    /// exceptions. Any deserialized exception will loose type information,
    /// and simply be represented as a <see cref="PortableException"/>. The
    /// basic detail information of the exception is retained.</p>
    /// <p>
    /// <b>PortableException</b> and this class work asymmetrically to
    /// provide the serialization routines for exceptions.</p>
    /// </remarks>
    /// <author>Mark Falco  2008.08.25</author>
    /// <author>Ana Cikic  2008.08.29</author>
    public class ExceptionPofSerializer : IPofSerializer
    {
        #region IPofSerializer implementation

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
        public void Serialize(IPofWriter writer, object o)
        {
            WriteException(writer, (Exception) o);
            writer.WriteRemainder(null);
        }

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
        /// version by calling <see cref="IPofWriter.VersionId"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The implementation may read any combination of the
        /// properties of the user type by using "read" methods of the
        /// <b>IPofReader</b>, but it must do so in the order of the property
        /// indexes.
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
        public object Deserialize(IPofReader reader)
        {
            PortableException e = new PortableException();
            reader.RegisterIdentity(e);
            e.ReadExternal(reader);
            reader.ReadRemainder();
            return e;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Write the exception to the specified stream.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to write to.
        /// </param>
        /// <param name="e">
        /// The <b>Exception</b> to write.
        /// </param>
        public static void WriteException(IPofWriter writer, Exception e)
        {
            string   name;
            string[] arrTrace;

            if (e is PortableException)
            {
                PortableException pe = (PortableException) e;
                name     = pe.Name;
                arrTrace = pe.FullStackTrace;
            }
            else
            {
                name     = e.GetType().FullName;
                arrTrace = new string[] {e.StackTrace};
            }

            writer.WriteString(0, name);
            writer.WriteString(1, e.Message);
            writer.WriteCollection(2, new ArrayList(arrTrace), typeof(string));
            writer.WriteObject(3, e.InnerException);
        }

        #endregion
    }
}