/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;

using Tangosol.Net;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Collections;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// SafeConfigurablePofContext is an extension of ConfigurablePofContext 
    /// that can serialize and deserialize any valid POF user type, even those 
    /// that have not been explicitly configured, as well as any .NET 
    /// serializable types.
    /// </summary>
    /// <remarks>
    /// <c>Important note:</c> this class is meant to be used only during
    /// application design time and replaced with the ConfigurablePofContext 
    /// for production deployments as it has the following limitations:
    /// <list type="bullet">
    ///   <item>
    ///   SafeConfigurablePofContext is supported only for .NET clients;
    ///   </item>
    ///   <item>
    ///   Its performance is less optimal than of the ConfigurablePofContext;
    ///   </item>
    ///   <item>
    ///   The serialized form produced by the SafeConfigurablePofContext will
    ///   not be recognized by POF aware ValueExtractors.
    ///   </item>
    /// </list>
    /// <para>
    /// For user types that have been explicitly configured, this IPofContext 
    /// behaves identically to the ConfigurablePofContext.
    /// </para>
    /// <para>
    /// As of 14.1.2.0, this class is deprecated as it relies on a
    /// deprecated <see cref="BinarySerializer"/>
    /// </para>
    /// </remarks>
    /// <author>Jason Howes  2007.05.03</author>
    /// <author>Aleksandar Seovic (.NET)  2009.09.25</author>
    /// <since> Coherence 3.6</since>
    [Obsolete("since Coherence 14.1.2.0")]
    public class SafeConfigurablePofContext : ConfigurablePofContext
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// Create a default <b>ConfigurablePofContext</b> that will load
        /// definitions from the default POF config file.
        /// </remarks>
        public SafeConfigurablePofContext()
        {
        }

        /// <summary>
        /// Create a <b>ConfigurablePofContext</b> that will use the passed
        /// configuration information.
        /// </summary>
        /// <param name="stream">
        /// An <b>Stream</b> containing information in the format of a
        /// configuration file used by <b>ConfigurablePofContext</b>.
        /// </param>
        public SafeConfigurablePofContext(Stream stream) : base(stream)
        {
        }

        /// <summary>
        /// Create a <b>ConfigurablePofContext</b> that will load
        /// configuration information from the specified locator.
        /// </summary>
        /// <param name="locator">
        /// The locator that specifies the location of the
        /// <see cref="IPofContext"/> configuration file; the locator is
        /// either a valid path or a URL.
        /// </param>
        public SafeConfigurablePofContext(string locator) : base(locator)
        {
        }

        /// <summary>
        /// Create a <b>ConfigurablePofContext</b> that will use the passed
        /// configuration information.
        /// </summary>
        /// <param name="xml">
        /// An <b>IXmlElement</b> containing information in the format of a
        /// configuration file used by <b>ConfigurablePofContext</b>.
        /// </param>
        public SafeConfigurablePofContext(IXmlElement xml) : base(xml)
        {
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
        public override IPofSerializer GetPofSerializer(int typeId)
        {
            EnsureInitialized();

            switch (typeId)
            {
                case TYPE_PORTABLE:
                    return m_serializerPof;

                case TYPE_SERIALIZABLE:
                    return m_serializerDotNet;

                default:
                    return base.GetPofSerializer(typeId);
            }
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
        public override Type GetType(int typeId)
        {
            switch (typeId)
            {
                case TYPE_PORTABLE:
                    // should never get here
                    return typeof (IPortableObject);

                case TYPE_SERIALIZABLE:
                    // should never get here
                    return typeof (Object);

                default:
                    return base.GetType(typeId);
            }
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
        public override int GetUserTypeIdentifier(Type type)
        {
            EnsureInitialized();

            int nTypeId = GetUserTypeIdentifierInternal(type);
            if (nTypeId < 0)
            {
                if (IsUserType(type))
                {
                    nTypeId = GetGenericTypeId(type);
                    using (BlockingLock l = BlockingLock.Lock(this))
                    {
                        IDictionary mapTypeIdByType =
                                GetPofConfig().m_mapTypeIdByType;
                        mapTypeIdByType = new Hashtable(mapTypeIdByType);
                        mapTypeIdByType[type] = nTypeId;
                        GetPofConfig().m_mapTypeIdByType = mapTypeIdByType;
                    }
                }
                else
                {
                    throw new ArgumentException("Unknown user type: " + type);
                }
            }

            return nTypeId;
        }

        /// <summary>
        /// Determine the user type identifier associated with the given type
        /// name.
        /// </summary>
        /// <param name="typeName">
        /// The assembly-qualified name of a user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The type identifier of the user type associated with the given
        /// type name.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the user type associated with the given type name is unknown
        /// to this <b>IPofContext</b>.
        /// </exception>
        public override int GetUserTypeIdentifier(String typeName)
        {
            EnsureInitialized();

            int nTypeId = GetUserTypeIdentifierInternal(typeName);
            if (nTypeId < 0)
            {
                if (IsUserType(typeName))
                {
                    nTypeId = GetGenericTypeId(TypeResolver.Resolve(typeName));
                    using (BlockingLock l = BlockingLock.Lock(this))
                    {
                        IDictionary mapTypeIdByTypeName =
                                GetPofConfig().m_mapTypeIdByTypeName;
                        mapTypeIdByTypeName = new Hashtable(mapTypeIdByTypeName);
                        mapTypeIdByTypeName[typeName] = nTypeId;
                        GetPofConfig().m_mapTypeIdByTypeName = mapTypeIdByTypeName;
                    }
                }
                else
                {
                    throw new ArgumentException("Unknown user type: " + typeName);
                }
            }

            return nTypeId;
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
        public override bool IsUserType(Type type)
        {
            bool fUserType = base.IsUserType(type);
            if (!fUserType)
            {
                if (!PofHelper.IsIntrinsicPofType(type))
                {
                    fUserType = typeof(IPortableObject).IsAssignableFrom(type) ||
                            typeof(ISerializable).IsAssignableFrom(type) ||
                            type.IsDefined(typeof(SerializableAttribute), false);
                }
            }

            return fUserType;
        }

        /// <summary>
        /// Determine if the type with the given name is a user type known to
        /// this <b>IPofContext</b>.
        /// </summary>
        /// <param name="typeName">
        /// The assembly-qualified name of the type to test; must not be 
        /// <c>null</c> or empty.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the type with the specified name is a valid user
        /// type.
        /// </returns>
        public override bool IsUserType(String typeName)
        {
            bool fUserType = base.IsUserType(typeName);
            if (!fUserType)
            {
                try
                {
                    var type = TypeResolver.Resolve(typeName);
                    if (!PofHelper.IsIntrinsicPofType(type))
                    {
                        fUserType = typeof(IPortableObject).IsAssignableFrom(type) ||
                                typeof(ISerializable).IsAssignableFrom(type) ||
                                type.IsDefined(typeof(SerializableAttribute), false);
                    }
                }
                catch (Exception)
                {
                }
            }

            return fUserType;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Fully initialize the <b>SafeConfigurablePofContext</b> if it has 
        /// not already been initialized.
        /// </summary>
        protected internal override void EnsureInitialized()
        {
            base.EnsureInitialized();
            if (m_serializerDotNet == null)
            {
                m_serializerDotNet = new DotNetPofSerializer();
                m_serializerPof    = new SafePofSerializer(this);
            }
        }

        /// <summary>
        /// For user types that are not registered in the POF configuration 
        /// used by this PofContext, determine if the user type can be 
        /// serialized using POF, otherwise determine if the user type can be 
        /// serialized using standard .NET BinaryFormatter.
        /// </summary>
        /// <param name="type">
        /// A user type that is not configured in this IPofContext.
        /// </param>
        /// <returns>
        /// A special user type id that indicates that the user type is
        /// supported by "generic" POF serialization or traditional .NET 
        /// serialization embedded in a POF stream.
        /// </returns>
        protected virtual int GetGenericTypeId(Type type)
        {
            if (typeof(IPortableObject).IsAssignableFrom(type))
            {
                return TYPE_PORTABLE;
            }

            if (typeof(ISerializable).IsAssignableFrom(type)
                || type.IsDefined(typeof(SerializableAttribute), false))
            {
                return TYPE_SERIALIZABLE;
            }

            throw new ArgumentException("The \"" + type.FullName
                                        + "\" type is not supported by " 
                                        + GetType().Name);
        }

        #endregion

        #region Inner class: DotNetPofSerializer

        /// <summary>
        /// Serializer used for Serializable objects.
        /// </summary>
        public class DotNetPofSerializer : SerializationHelper, IPofSerializer
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
            public void Serialize(IPofWriter writer, Object o)
            {
                writer.WriteBinary(0, ToBinary(o, m_serializer));
                writer.WriteRemainder(null);
                Register(o);
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
            public Object Deserialize(IPofReader reader)
            {
                Object o = FromBinary(reader.ReadBinary(0), m_serializer);
                reader.RegisterIdentity(o);
                reader.ReadRemainder();
                Register(o);
                return o;
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Register a class as having been encountered by the serializer.
            /// </summary>
            /// <param name="o">
            /// An object that is being serialized or has been deserialized.
            /// </param>
            protected void Register(Object o)
            {
                if (o != null)
                {
                    m_mapRegisteredClasses.AcquireWriteLock();
                    try
                    {
                        String typeName = o.GetType().FullName;
                        if (!m_mapRegisteredClasses.Contains(typeName))
                        {
                            m_mapRegisteredClasses.Add(typeName, null);
                            CacheFactory.Log("TODO: Add POF support for \"" + typeName + "\".", 0);
                        }
                    }
                    finally
                    {
                        m_mapRegisteredClasses.ReleaseWriteLock();    
                    }
                }
            }

            #endregion

            #region Data members

            /// <summary>
            /// Serializer used by this IPofSerializer.
            /// </summary>
            private readonly ISerializer m_serializer = new BinarySerializer();

            /// <summary>
            /// All classes that have been registered.
            /// </summary>
            private readonly SynchronizedDictionary m_mapRegisteredClasses =
                new SynchronizedDictionary();

            #endregion
        }

        #endregion

        #region Inner class: SafePofSerializer

        /// <summary>
        /// Serializer used for objects implementing the IPortableObject 
        /// interface.
        /// </summary>
        public class SafePofSerializer : SerializationHelper, IPofSerializer
        {
            #region Constructors

            /// <summary>
            /// Construct a new instance of SafePofSerializer.
            /// </summary>
            /// <param name="ctx">
            /// The ConfigurablePofContext to use.
            /// </param>
            public SafePofSerializer(ConfigurablePofContext ctx)
            {
                m_pofContext = ctx;
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
            /// The <b>IPofWriter</b> with which to write the object's state.
            /// </param>
            /// <param name="o">
            /// The object to serialize.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public void Serialize(IPofWriter writer, Object o)
            {
                var             buffer         = new BinaryMemoryStream(1024*8);
                PofStreamWriter userTypeWriter = new PofStreamWriter.UserTypeWriter(
                    new DataWriter(buffer), m_pofContext, TYPE_PORTABLE, -1);

                // COH-5065: due to the complexity of maintaining references
                // in future data, we won't support them for IEvolvable objects
                if (m_pofContext.IsReferenceEnabled && !(o is IEvolvable))
                {
                    userTypeWriter.EnableReference();
                }

                m_serializer.Serialize(userTypeWriter, o);

                String typeName = o.GetType().AssemblyQualifiedName;
                writer.WriteString(0, typeName);
                writer.WriteBinary(1, buffer.ToBinary());
                writer.WriteRemainder(null);

                Register(typeName);
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
            public Object Deserialize(IPofReader reader)
            {
                String typeName = reader.ReadString(0);
                Binary bin = reader.ReadBinary(1);
                reader.ReadRemainder();

                ConfigurablePofContext ctx = m_pofContext;
                IPortableObject po;
                try
                {
                    po = (IPortableObject) ObjectUtils.CreateInstance(TypeResolver.Resolve(typeName));
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to instantiate PortableObject class: " + typeName, e);
                }

                DataReader dataReader = bin.GetReader();
                int nType = dataReader.ReadPackedInt32();
                if (nType != TYPE_PORTABLE)
                {
                    throw new IOException("Invalid POF type: " + nType
                                          + " (" + TYPE_PORTABLE + " expected)");
                }

                int iVersion = dataReader.ReadPackedInt32();

                IPofReader pofReader = new PofStreamReader.UserTypeReader(
                    dataReader, ctx, TYPE_PORTABLE, iVersion);

                m_serializer.Initialize(po, pofReader);

                Register(typeName);

                return po;
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Register a class as having been encountered by the serializer.
            /// </summary>
            /// <param name="typeName">
            /// The name of a class that is being serialized or deserialized.
            /// </param>
            protected void Register(String typeName)
            {
                m_mapRegisteredClasses.AcquireWriteLock();
                try
                {
                    if (!m_mapRegisteredClasses.Contains(typeName))
                    {
                        m_mapRegisteredClasses.Add(typeName, null);
                        CacheFactory.Log("TODO: Add the class \"" + typeName
                                         + "\" to the POF configuration file.", 0);
                    }
                }
                finally
                {
                    m_mapRegisteredClasses.ReleaseWriteLock();
                }
            }

            #endregion

            #region Data members

            /// <summary>
            /// Reference to outer SafeConfigurablePofContext.
            /// </summary>
            private readonly ConfigurablePofContext m_pofContext;

            /// <summary>
            /// Serializer used by this IPofSerializer.
            /// </summary>
            private readonly PortableObjectSerializer m_serializer =
                new PortableObjectSerializer(TYPE_PORTABLE);

            /// <summary>
            /// All classes that have been registered.
            /// </summary>
            private readonly SynchronizedDictionary m_mapRegisteredClasses =
                new SynchronizedDictionary();

            #endregion
        }

        #endregion

        #region Constants

        /// <summary>
        /// The type identifier for objects that implement the PortableObject
        /// interface.
        /// </summary>
        public const int TYPE_PORTABLE = Int32.MaxValue - 1;

        /// <summary>
        /// The type identifier for .NET Serializable objects.
        /// </summary>
        public const int TYPE_SERIALIZABLE = Int32.MaxValue;

        #endregion

        #region Data members

        /// <summary>
        /// Serializer used for Serializable objects.
        /// </summary>
        private IPofSerializer m_serializerDotNet;

        /// <summary>
        /// Serializer used for [not registered] objects implementing 
        /// IPortableObject interface.
        /// </summary>
        private IPofSerializer m_serializerPof;

        #endregion
    }
}