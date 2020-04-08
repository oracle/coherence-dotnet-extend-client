/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

using Tangosol.Util;
using Tangosol.Util.Collections;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// Basic <see cref="IPofContext"/> implementation.
    /// </summary>
    /// <seealso cref="PortableObjectSerializer"/>
    /// <author>Jason Howes  2006.07.18</author>
    /// <author>Goran Milosavljevic  2006.08.09</author>
    /// <author>Aleksandar Seovic  2006.08.14</author>
    /// <since>Coherence 3.2</since>
    public class SimplePofContext : IPofContext
    {
        #region Properties

        /// <summary>
        /// Determine if Identity/Reference type support is enabled for this
        /// SimplePofContext.
        /// </summary>
        /// <value> <b>true</b> if Identity/Reference type support is enabled
        /// </value>
        /// <since>Coherence 3.7.1</since>
        public bool IsReferenceEnabled
        {
            get
            {
                return m_referenceEnabled;
            }
            set
            {
                m_referenceEnabled = value;
            }
        }

        #endregion

        #region ISerializer implementation

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
        public virtual void Serialize(DataWriter writer, object o)
        {
            PofStreamWriter pofWriter = new PofStreamWriter(writer, this);

            // COH-5065: due to the complexity of maintaining references
            // in future data, we won't support them for IEvolvable objects
            if (IsReferenceEnabled && !(o is IEvolvable))
            {
                pofWriter.EnableReference();
            }

            try
            {
                pofWriter.WriteObject(-1, o);
            }
            catch (ArgumentException e)
            {
                // Guarantee that exceptions from called methods are IOException
                throw new IOException(e.Message, e);
            }
            catch (NotSupportedException e)
            {
                // Guarantee that exceptions from called methods are IOException
                throw new IOException(e.Message, e);
            }
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
        public virtual object Deserialize(DataReader reader)
        {
            PofStreamReader pofReader = new PofStreamReader(reader, this);

            try
            {
                return pofReader.ReadObject(-1);
            }
            catch (ArgumentException e)
            {
                // Guarantee that exceptions from called methods are IOException
                throw new IOException(e.Message, e);
            }
            catch (NotSupportedException e)
            {
                // Guarantee that exceptions from called methods are IOException
                throw new IOException(e.Message, e);
            }
        }

        #endregion

        #region IPofContext implementation

        /// <summary>
        /// Return an <see cref="IPofSerializer"/> that can be used to
        /// serialize and deserialize an object of the specified user type to
        /// and from a POF stream.
        /// </summary>
        /// <param name="typeId">
        /// The type identifier of the user type that can be serialized and
        /// deserialized using the returned <b>IPofSerializer</b>; must be
        /// non-negative.
        /// </param>
        /// <returns>
        /// An <b>IPofSerializer</b> for the specified user type.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified user type is negative or unknown to this
        /// <b>IPofContext</b>.
        /// </exception>
        public virtual IPofSerializer GetPofSerializer(int typeId)
        {
            ValidateTypeId(typeId);

            IDictionary    serializerMap = m_serializerMap;
            IPofSerializer serializer    = serializerMap == null ? null : (IPofSerializer) serializerMap[typeId];

            if (serializer == null)
            {
                throw new ArgumentException("unknown user type: " + typeId);
            }
            return serializer;
        }

        /// <summary>
        /// Determine the user type identifier associated with the given
        /// object.
        /// </summary>
        /// <param name="o">
        /// An instance of a user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The type identifier of the user type associated with the given
        /// object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the user type associated with the given object is unknown to
        /// this <b>IPofContext</b>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="o"/> is <c>null</c>.
        /// </exception>
        public virtual int GetUserTypeIdentifier(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("Object cannot be null");
            }
            return GetUserTypeIdentifier(o.GetType());
        }

        /// <summary>
        /// Determine the user type identifier associated with the given
        /// type.
        /// </summary>
        /// <param name="type">
        /// A user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The type identifier of the user type associated with the given
        /// type.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the user type associated with the given type is unknown to
        /// this <b>IPofContext</b>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="type"/> is <c>null</c>.
        /// </exception>
        public virtual int GetUserTypeIdentifier(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("Type cannot be null");
            }

            IDictionary typeIdMap = m_typeIdMap;
            object      typeId    = typeIdMap == null ? null : typeIdMap[type];

            if (typeId == null)
            {
                throw new ArgumentException("unknown user type: " + type);
            }
            return (int) typeId;
        }

        /// <summary>
        /// Determine the user type identifier associated with the given type
        /// name.
        /// </summary>
        /// <param name="typeName">
        /// The name of a user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The type identifier of the user type associated with the given
        /// type name.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the user type associated with the given type name is unknown
        /// to this <b>IPofContext</b>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="typeName"/> is <c>null</c>.
        /// </exception>
        public virtual int GetUserTypeIdentifier(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("Type name cannot be null");
            }
            return GetUserTypeIdentifier(TypeResolver.Resolve(typeName));
        }

        /// <summary>
        /// Determine the name of the type associated with a user type
        /// identifier.
        /// </summary>
        /// <param name="typeId">
        /// The user type identifier; must be non-negative.
        /// </param>
        /// <returns>
        /// The name of the type associated with the specified user type
        /// identifier.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified user type is negative or unknown to this
        /// <b>IPofContext</b>.
        /// </exception>
        public virtual string GetTypeName(int typeId)
        {
            return GetType(typeId).FullName;
        }

        /// <summary>
        /// Determine the type associated with the given user type
        /// identifier.
        /// </summary>
        /// <param name="typeId">
        /// The user type identifier; must be non-negative.
        /// </param>
        /// <returns>
        /// The type associated with the specified user type identifier.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified user type is negative or unknown to this
        /// <b>IPofContext</b>.
        /// </exception>
        public virtual Type GetType(int typeId)
        {
            ValidateTypeId(typeId);

            IDictionary typeMap = m_typeMap;
            Type        type    = typeMap == null ? null : (Type) typeMap[typeId];

            if (type == null)
            {
                throw new ArgumentException("unknown user type: " + typeId);
            }
            return type;
        }

        /// <summary>
        /// Determine if the given object is of a user type known to this
        /// <b>IPofContext</b>.
        /// </summary>
        /// <param name="o">
        /// The object to test; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the specified object is of a valid user type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="o"/> is <c>null</c>.
        /// </exception>
        public virtual bool IsUserType(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("Object cannot be null");
            }
            return IsUserType(o.GetType());
        }

        /// <summary>
        /// Determine if the given type is a user type known to this
        /// <b>IPofContext</b>.
        /// </summary>
        /// <param name="type">
        /// The type to test; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the specified type is a valid user type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="type"/> is <c>null</c>.
        /// </exception>
        public virtual bool IsUserType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("Type cannot be null");
            }

            IDictionary typeIdMap = m_typeIdMap;
            return typeIdMap != null && typeIdMap[type] != null;
        }

        /// <summary>
        /// Determine if the type with the given name is a user type known to
        /// this <b>IPofContext</b>.
        /// </summary>
        /// <param name="typeName">
        /// The name of the type to test; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the type with the specified name is a valid user
        /// type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="typeName"/> is <c>null</c>.
        /// </exception>
        public virtual bool IsUserType(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("Type name cannot be null");
            }
            return IsUserType(TypeResolver.Resolve(typeName));
        }

        #endregion

        #region User type registration

        /// <summary>
        /// Associate a user type with a type identifier and
        /// <see cref="IPofSerializer"/>.
        /// </summary>
        /// <param name="typeId">
        /// The type identifier of the specified user type; must be greater
        /// or equal to 0.
        /// </param>
        /// <param name="type">
        /// The user type to register with this PofContext; must not be
        /// <c>null</c>.
        /// </param>
        /// <param name="serializer">
        /// The <see cref="IPofSerializer"/> that will be used to serialize
        /// and deserialize objects of the specified type.
        /// </param>
        /// <exception cref="ArgumentException">
        /// On invalid type identifer, type, or <b>IPofSerializer</b>.
        /// </exception>
        public virtual void RegisterUserType(int typeId, Type type,
                                             IPofSerializer serializer)
        {
            ValidateTypeId(typeId);
            if (type == null)
            {
                throw new ArgumentNullException("Type cannot be null");
            }
            if (serializer == null)
            {
                throw new ArgumentNullException("POF serializer cannot be null");
            }

            IDictionary typeIdMap     = m_typeIdMap;
            IDictionary typeMap       = m_typeMap;
            IDictionary serializerMap = m_serializerMap;

            // add class-to-type identifier mapping
            if (typeIdMap == null)
            {
                m_typeIdMap = typeIdMap = new HashDictionary();
            }
            typeIdMap[type] = typeId;

            // add type identifier-to-class mapping
            if (typeMap == null)
            {
                m_typeMap = typeMap = new HashDictionary();
            }
            typeMap[typeId] = type;

            // add type identifier-to-serializer mapping
            if (serializerMap == null)
            {
                m_serializerMap = serializerMap = new HashDictionary();
            }
            serializerMap[typeId] = serializer;
        }

        /// <summary>
        /// Unregister a user type that was previously registered using the
        /// specified type identifier.
        /// </summary>
        /// <param name="typeId">
        /// The type identifier of the user type to unregister.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the specified user type identifier is unknown to this
        /// IPofContext.
        /// </exception>
        public virtual void UnregisterUserType(int typeId)
        {
            Type        type          = GetType(typeId);
            IDictionary typeIdMap     = m_typeIdMap;
            IDictionary typeMap       = m_typeMap;
            IDictionary serializerMap = m_serializerMap;

            // remove class-to-type identifier mapping
            typeIdMap.Remove(type);
            if (typeIdMap.Count == 0)
            {
                typeIdMap = m_typeIdMap = null;
            }

            // remove type identifier-to-class mapping
            typeMap.Remove(typeId);
            if (typeMap.Count == 0)
            {
                typeMap = m_typeMap = null;
            }

            // remove type identifier-to-serializer mapping
            serializerMap.Remove(typeId);
            if (serializerMap.Count == 0)
            {
                serializerMap = m_serializerMap = null;
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Ensure that the given user type identifier is valid.
        /// </summary>
        /// <param name="typeId">
        /// The user type identifier to validate.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the given user type identifier is negative.
        /// </exception>
        protected internal static void ValidateTypeId(int typeId)
        {
            if (typeId < 0)
            {
                throw new ArgumentException("negative user type identifier: "
                                            + typeId);
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// A map that contains mappings from a registered
        /// user type into type identifier.
        /// </summary>
        protected internal IDictionary m_typeIdMap;

        /// <summary>
        /// A map of user types, indexed by type identifier.
        /// </summary>
        protected internal IDictionary m_typeMap;

        /// <summary>
        /// A map of POF serializer objects, indexed by type identifier.
        /// </summary>
        protected internal IDictionary m_serializerMap;

        /// <summary>
        /// <b>true</b> if POF Identity/Reference type support is enabled
        /// </summary>
        protected internal bool m_referenceEnabled;

        #endregion
    }
}