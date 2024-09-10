/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tangosol.IO.Pof
{
    /// <summary><see cref="IPofSerializer"/> implementation that supports
    /// the serialization and deserialization of any serializable .NET type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// As of 14.1.2.0, this class is deprecated as it relies on a
    /// deprecated <see cref="BinaryFormatter"/>
    /// </para>
    /// </remarks>
    /// <author>Goran Milosavljevic  2007.08.23</author>
    /// <since>Coherence 3.4</since>
    [Obsolete("since Coherence 14.1.2.0")]
    public class BinaryPofSerializer : IPofSerializer
    {
        #region Constructors

        /// <summary>
        /// Create a new <b>BinaryPofSerializer</b> for the user type with
        /// the given type identifier.
        /// </summary>
        /// <param name="typeId">
        /// The user type identifier.
        /// </param>
        public BinaryPofSerializer(int typeId)
        {
            Debug.Assert(typeId >= 0, "user type identifier cannot be negative");
            m_typeId = typeId;
        }

        #endregion

        #region IPofSerializer Members

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
            MemoryStream stream = new MemoryStream();
            try
            {
                new BinaryFormatter().Serialize(stream, o);

                stream.Position = 0;
                writer.WriteObject(0, stream.GetBuffer());
                writer.WriteRemainder(null);
            }
            catch (SerializationException e)
            {
                string typeName = null;
                try
                {
                    typeName = writer.PofContext.GetTypeName(m_typeId);
                }
                catch (Exception)
                { }

                string actual = o.GetType().FullName;
                throw new IOException(
                        "An exception occurred writing an object"
                        + " user type to a POF stream: type-id=" + m_typeId
                        + (typeName == null ? "" : ", class-name=" + typeName)
                        + (actual == null ? "" : ", actual class-name=" + actual)
                        + ", exception=\n" + e);
            }
            finally
            {
                stream.Close();
            }
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
            MemoryStream stream = new MemoryStream((byte[]) reader.ReadObject(0));
            try
            {
                reader.ReadRemainder();
                return new BinaryFormatter().Deserialize(stream);
            }
            catch (Exception e)
            {
                string typeName = null;
                try
                {
                    typeName = reader.PofContext.GetTypeName(m_typeId);
                }
                catch (Exception)
                { }

                throw new IOException(
                        "An exception occurred instantiating an object"
                        + " user type from a POF stream: type-id=" + m_typeId
                        + (typeName == null ? "" : ", class-name=" + typeName)
                        + ", exception=\n" + e);
            }
            finally
            {
                stream.Close();
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier of the user type to serialize and
        /// deserialize.
        /// </summary>
        protected internal int m_typeId;

        #endregion
    }
}