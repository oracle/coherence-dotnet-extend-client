/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;
using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    /// <summary><see cref="IPofSerializer"/> implementation that supports
    /// the serialization and deserialization of any class that implements
    /// <see cref="IPortableObject"/> to and from a POF stream.
    /// </summary>
    /// <author>Jason Howes  2006.07.18</author>
    /// <author>Aleksandar Seovic  2006.08.12</author>
    /// <since>Coherence 3.2</since>
    public class PortableObjectSerializer : IPofSerializer
    {
        #region Constructors

        /// <summary>
        /// Create a new PortableObjectSerializer for the user type with the
        /// given type identifier.
        /// </summary>
        /// <param name="typeId">
        /// The user type identifier.
        /// </param>
        public PortableObjectSerializer(int typeId)
        {
            Debug.Assert(typeId >= 0, "user type identifier cannot be negative");
            m_typeId = typeId;
        }

        #endregion

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
        /// The <see cref="IPofWriter"/> with which to write the object's
        /// state.
        /// </param>
        /// <param name="o">
        /// The object to serialize.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void Serialize(IPofWriter writer, object o)
        {
            IPortableObject portable;
            try
            {
                portable = (IPortableObject) o;
            }
            catch (InvalidCastException e)
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
                        "An exception occurred writing an IPortableObject"
                        + " user type to a POF stream: type-id=" + m_typeId
                        + (typeName == null ? "" : ", class-name=" + typeName)
                        + (actual == null ? "" : ", actual class-name=" + actual)
                        + ", exception=\n" + e);
            }

            // set the version identifier
            bool       isEvolvable = portable is IEvolvable;
            IEvolvable evolvable   = null;
            if (isEvolvable)
            {
                evolvable = (IEvolvable) portable;
                writer.VersionId =
                        Math.Max(evolvable.DataVersion, evolvable.ImplVersion);
            }

            // write out the object's properties
            portable.WriteExternal(writer);

            // write out any future properties
            Binary remainder = null;
            if (isEvolvable)
            {
                remainder = evolvable.FutureData;
            }
            writer.WriteRemainder(remainder);
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
        /// The implementation may read any combination of the properties of
        /// the user type by using "read" methods of the <b>IPofReader</b>,
        /// but it must do so in the order of the property indexes.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// After all desired properties of the user type have been read, the
        /// implementation must terminate the reading of the user type by
        /// calling <see cref="IPofReader.ReadRemainder"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="reader">
        /// The <see cref="IPofReader"/> with which to read the object's
        /// state.
        /// </param>
        /// <returns>
        /// The deserialized user type instance.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual object Deserialize(IPofReader reader)
        {
            // create a new instance of the user type
            IPortableObject portable;
            try
            {
                portable = (IPortableObject)
                           Activator.CreateInstance(reader.PofContext.GetType(m_typeId));
                reader.RegisterIdentity(portable);
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
                        "An exception occurred instantiating an IPortableObject"
                        + " user type from a POF stream: type-id=" + m_typeId
                        + (typeName == null ? "" : ", class-name=" + typeName)
                        + ", exception=\n" + e);
            }

            Initialize(portable, reader);

            return portable;
        }

        /// <summary>
        /// Initialize the specified (newly instantiated) PortableObject instance
        /// using the specified reader.
        /// </summary>
        /// <param name="portable">The object to initialize.</param>
        /// <param name="reader">
        /// The PofReader with which to read the object's state.
        /// </param>
        public void Initialize(IPortableObject portable, IPofReader reader)
        {
            // set the version identifier
            bool       isEvolvable = portable is IEvolvable;
            IEvolvable evolvable   = null;
            if (isEvolvable)
            {
                evolvable = (IEvolvable) portable;
                evolvable.DataVersion = reader.VersionId;
            }

            // read the object's properties
            portable.ReadExternal(reader);

            // read any future properties
            Binary remainder = reader.ReadRemainder();
            if (isEvolvable)
            {
                evolvable.FutureData = remainder;
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