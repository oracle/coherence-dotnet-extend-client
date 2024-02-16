/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// <see cref="IPofSerializer"/> implementation that supports
    /// .NET XML serialization and deserialization.
    /// </summary>
    /// <author>Ivan Cikic  2007.08.23</author>
    /// <since>Coherence 3.4</since>
    public class XmlPofSerializer : IPofSerializer
    {
        #region Constructors

        /// <summary>
        /// Create a new <b>XmlPofSerializer</b> for the user type with the
        /// given type identifier.
        /// </summary>
        /// <param name="typeId">
        /// The user type identifier.
        /// </param>
        public XmlPofSerializer(int typeId)
        {
            Debug.Assert(typeId >= 0, "user type identifier cannot be negative");
            m_typeId = typeId;
        }

        #endregion

        #region IPofSerializer

        /// <summary>
        /// Serialize a user type instance to a POF stream by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
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
            try
            {
                StringBuilder sb = new StringBuilder();
                using (TextWriter txtWriter = new StringWriter(sb))
                {
                    new System.Xml.Serialization.XmlSerializer(o.GetType()).Serialize(txtWriter, o);
                }
                writer.WriteString(0, sb.ToString());
                writer.WriteRemainder(null);
            }
            catch (InvalidOperationException e)
            {
                string typeName = null;
                try
                {
                    typeName = writer.PofContext.GetTypeName(m_typeId);
                }
                catch (Exception)
                {}

                string actual = o.GetType().FullName;
                throw new IOException(
                        "An exception occurred writing an object"
                        + " user type to a POF stream: type-id=" + m_typeId
                        + (typeName == null ? "" : ", class-name=" + typeName)
                        + (actual == null ? "" : ", actual class-name=" + actual)
                        + ", exception=\n" + e);
            }
        }

        /// <summary>
        /// Deserialize a user type instance from a POF stream by reading its
        /// state using the specified <see cref="IPofReader"/> object.
        /// </summary>
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
            try
            {
                using (TextReader txtReader = new StringReader(reader.ReadString(0)))
                {
                    object o = new System.Xml.Serialization.XmlSerializer(reader.PofContext.GetType(m_typeId)).Deserialize(txtReader);
                    reader.RegisterIdentity(o);
                    reader.ReadRemainder();
                    return o;
                }
            }
            catch (Exception e)
            {
                string typeName = null;
                try
                {
                    typeName = reader.PofContext.GetTypeName(m_typeId);
                }
                catch (Exception)
                {}

                throw new IOException(
                        "An exception occurred instantiating an object"
                        + " user type from a POF stream: type-id=" + m_typeId
                        + (typeName == null ? "" : ", class-name=" + typeName)
                        + ", exception=\n" + e);
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier of the user type to serialize and
        /// deserialize.
        /// </summary>
        protected int m_typeId;

        #endregion
    }
}