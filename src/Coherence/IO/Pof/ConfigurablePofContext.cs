/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using System.Text;

using Tangosol.Config;
using Tangosol.IO.Pof.Annotation;
using Tangosol.IO.Resources;
using Tangosol.Net;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Collections;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// This class implements the <see cref="IPofContext"/> interface using
    /// information provided in a configuration file (or in a passed XML
    /// configuration).
    /// </summary>
    /// <remarks>
    /// <p>
    /// For each user type supported by this POF context, it must be provided
    /// with:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// A valid user type ID that is unique within this POF context.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// A .NET type name that identifies a .NET type or interface that all
    /// values of the user type are type-assignable to (and that no values of
    /// other user types are type-assignable to); in other words, all values
    /// of the user type (and no values of other user types) are instances of
    /// the specified class, instances of a sub-class of the specified class,
    /// or (if it is an interface) instances of a class that implements the
    /// specified interface.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// A .NET type name that identifies a non-abstract implementation of
    /// the <see cref="IPofSerializer"/> interface.
    /// </description>
    /// </item>
    /// </list>
    /// </p>
    /// <p>
    /// The format of the configuration XML is as follows:
    /// <tt><pre>
    /// &lt;pof-config&gt;
    ///   &lt;user-type-list&gt;
    ///   ..
    ///     &lt;user-type&gt;
    ///       &lt;type-id&gt;53&lt;/type-id&gt;
    ///       &lt;class-name&gt;My.Example.Data.Trade, MyAssembly&lt;/class-name&gt;
    ///       &lt;serializer&gt;
    ///         &lt;class-name&gt;Tangosol.IO.Pof.PortableObjectSerializer, Coherence&lt;/class-name&gt;
    ///         &lt;init-params&gt;
    ///           &lt;init-param&gt;
    ///             &lt;param-type&gt;System.Int32&lt;/param-type&gt;
    ///             &lt;param-value&gt;{type-id}&lt;/param-value&gt;
    ///           &lt;/init-param&gt;
    ///         &lt;/init-params&gt;
    ///       &lt;/serializer&gt;
    ///     &lt;/user-type&gt;
    ///
    ///     &lt;user-type&gt;
    ///       &lt;type-id&gt;54&lt;/type-id&gt;
    ///       &lt;class-name&gt;My.Example.Data.Position, MyAssembly&lt;/class-name&gt;
    ///     &lt;/user-type&gt;
    ///
    ///   ..
    ///   &lt;include&gt;file:/my-pof-config.xml&lt;/include&gt;
    ///
    ///   ..
    ///   &lt;/user-type-list&gt;
    ///
    ///   &lt;allow-interfaces&gt;false&lt;/allow-interfaces&gt;
    ///   &lt;allow-subclasses&gt;false&lt;/allow-subclasses&gt;
    ///
    ///   &lt;default-serializer&gt;
    ///     &lt;class-name&gt;Tangosol.IO.Pof.XmlPofSerializer, Coherence&lt;/class-name&gt;
    ///     &lt;init-params&gt;
    ///       &lt;init-param&gt;
    ///         &lt;param-type&gt;System.Int32&lt;/param-type&gt;
    ///         &lt;param-value&gt;{type-id}&lt;/param-value&gt;
    ///       &lt;/init-param&gt;
    ///     &lt;/init-params&gt;
    ///   &lt;/default-serializer&gt;
    /// &lt;/pof-config&gt;
    /// </pre></tt></p>
    /// <p>
    /// For each user type, a <tt>user-type</tt> element must exist inside
    /// the <tt>user-type-list</tt> element. The <tt>user-type-list</tt>
    /// element contains up to three elements, in the following order:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// The <tt>user-type</tt> element should contain a <tt>type-id</tt>
    /// element whose value specifies the unique integer type ID; if none of
    /// the <tt>user-type</tt> elements contains a <tt>type-id</tt> element,
    /// then the type IDs for the user types will be based on the order in
    /// which they appear in the configuration, with the first user type
    /// being assigned the type ID 0, the second user type being assigned the
    /// type ID 1, and so on. (It is strongly recommended that user types IDs
    /// always be specified, in order to support schema versioning and
    /// evolution.)
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The <tt>class-name</tt> element is required, and specifies the fully
    /// qualified name of the .NET type or interface that all values of the
    /// user type are type-assignable to.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The <tt>serializer</tt> element is used to specify an implementation
    /// of <b>IPofSerializer</b> to use to serialize and deserialize user
    /// type values to and from a POF stream. Within the <tt>serializer</tt>
    /// element, the <tt>class-name</tt> element is required, and zero or
    /// more constructor parameters can be defined within an
    /// <tt>init-params</tt> block. If no <tt>serializer</tt> is specified,
    /// then the <tt>default-serializer</tt> is used if one is specified,
    /// otherwise the user type is assumed to implement the
    /// <see cref="IPortableObject"/> interface or have a <see cref="Portable"/>
    /// <see cref="Attribute"/>. If the former, a <see cref="PortableObjectSerializer"/> 
    /// will be used. If the later, a <see cref="PofAnnotationSerializer"/> 
    /// will be used.
    /// </description>
    /// </item>
    /// </list></p>
    /// <p>
    /// The optional <tt>include</tt> element allows <tt>user-type</tt>
    /// elements defined in another configuration XML to be added to the user
    /// type list. The value of this element is a locator string (either a
    /// valid path or URL) that specifies the location of the target
    /// <b>IPofContext</b> configuration file. The <tt>user-type</tt>
    /// elements of the target file are imported verbatum; therefore, if the
    /// included elements contain explicit type identifiers, each identifier
    /// must be unique with respect to the the user type identifiers (either
    /// explicit or generated) defined within the including file. If the
    /// included user types do not contain explicit type identifiers, then
    /// the type identifiers will be based on the order in which the user
    /// types appear in the composite configuration file. Multiple
    /// <tt>include</tt> elements may be used within a single
    /// <tt>user-type-list</tt> element.</p>
    /// <p>
    /// In order to be used by the <b>ConfigurablePofContext</b>, a
    /// <b>IPofSerializer</b> implementation must provide a public
    /// constructor that accepts the parameters detailed by the
    /// <tt>init-params</tt> element. The parameter values, as specified by
    /// the <tt>param-value</tt> element, can specify one of the following
    /// substitutable values:
    /// <list type="bullet">
    /// <item>
    /// <term><tt>{type-id}</tt></term>
    /// <description>replaced with the Type ID of the User Type</description>
    /// </item>
    /// <item>
    /// <term><tt>{class-name}</tt></term>
    /// <description>
    /// replaced with the name of the class for the User Type
    /// </description>
    /// </item>
    /// <item>
    /// <term><tt>{class}</tt></term>
    /// <description>replaced with the Type for the User Type</description>
    /// </item>
    /// </list></p>
    /// <p>
    /// If the <tt>init-params</tt> element is not present, then the
    /// <b>ConfigurablePofContext</b> attempts to construct the
    /// <b>IPofSerializer</b> by searching for one of the following
    /// constructors in the same order as they appear here:
    /// <list type="bullet">
    /// <item><description>(int typeId, Type type)</description></item>
    /// <item><description>(int typeId)</description></item>
    /// <item><description>()</description></item>
    /// </list></p>
    /// <p>
    /// Once constructed, if the <b>IPofSerializer</b> implements the
    /// <see cref="IXmlConfigurable"/> interface, the
    /// <see cref="IXmlConfigurable.Config"/> property is set to the passed
    /// XML information, transposed as described by
    /// <see cref="XmlHelper.TransformInitParams"/>, and as described in the
    /// pof-config.xsd file.</p>
    /// </remarks>
    /// <author>Jason Howes/Cameron Purdy  2006.07.24</author>
    /// <author>Ivan Cikic  2006.08.24</author>
    /// <author>Aleksandar Seovic  2008.10.08</author>
    /// <since>Coherence 3.2</since>
    public class ConfigurablePofContext : IPofContext, IXmlConfigurable
    {
        #region Constructors

        /// <summary>
        /// Create a <b>ConfigurablePofContext</b> that will use the passed
        /// configuration information.
        /// </summary>
        /// <param name="stream">
        /// An <b>Stream</b> containing information in the format of a
        /// configuration file used by <b>ConfigurablePofContext</b>.
        /// </param>
        public ConfigurablePofContext(Stream stream)
            : this(XmlHelper.LoadXml(stream))
        {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// Create a default <b>ConfigurablePofContext</b> that will load
        /// definitions from the default POF config file.
        /// </remarks>
        public ConfigurablePofContext()
        {}

        /// <summary>
        /// Create a <b>ConfigurablePofContext</b> that will load
        /// configuration information from the specified locator.
        /// </summary>
        /// <param name="locator">
        /// The locator that specifies the location of the
        /// <see cref="IPofContext"/> configuration file; the locator is
        /// either a valid path or a URL.
        /// </param>
        public ConfigurablePofContext(string locator)
        {
            m_configFile = ResourceLoader.GetResource(locator);
        }

        /// <summary>
        /// Create a <b>ConfigurablePofContext</b> that will use the passed
        /// configuration information.
        /// </summary>
        /// <param name="xml">
        /// An <see cref="IXmlElement"/> containing information in the format
        /// of a configuration file used by <b>ConfigurablePofContext</b>.
        /// </param>
        public ConfigurablePofContext(IXmlElement xml)
        {
            Config = xml;
        }

        /// <summary>
        /// Create a copy of <b>ConfigurablePofContext</b> from the given one.
        /// </summary>
        /// <param name="that">
        /// The <b>ConfigurablePofContext</b> to (shallow) copy from.
        /// </param>
        public ConfigurablePofContext(ConfigurablePofContext that)
        {
            m_cfg                = that.m_cfg;
            m_isReferenceEnabled = that.m_isReferenceEnabled;
            m_xml                = that.m_xml;
            m_configFile         = that.m_configFile;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The <see cref="IResource"/> for the default XML configuration used
        /// when one isn't explicitly passed in the constructor for this class.
        /// </summary>
        /// <value>
        /// The <see cref="IResource"/> for the default XML configuration.
        /// </value>
        public static IResource DefaultPofConfigResource
        {
            get
            {
                return s_configResource;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                s_configResource = value;
            }
        }

        /// <summary>
        /// The default XML configuration used when one isn't explicitly passed
        /// in the constructor for this class.
        /// </summary>
        /// <value>
        /// The default XML configuration.
        /// </value>
        public static IXmlDocument DefaultPofConfig
        {
            get;
            set;
        }

        /// <summary>
        /// Determine if the <b>ConfigurablePofContext</b> has completed its
        /// initialization.
        /// </summary>
        /// <value>
        /// <b>true</b> if the initialization is complete.
        /// </value>
        protected internal virtual bool IsInitialized
        {
            get { return m_cfg != null; }
        }

        /// <summary>
        /// Obtain the location of the configuration that the
        /// <b>ConfigurablePofContext</b> used to configure itself.
        /// </summary>
        /// <value>
        /// The location information for the configuration for the
        /// <b>ConfigurablePofContext</b>, or <c>null</c> if not yet
        /// initialized and no location was specified.
        /// </value>
        protected internal virtual string ConfigLocation
        {
            get { return m_configFile.Uri; }
        }

        /// <summary>
        /// Determine if the <b>ConfigurablePofContext</b> supports the
        /// configuration of user types by specifying an interface (instead
        /// of a class) for the .NET type.
        /// </summary>
        /// <value>
        /// <b>true</b> if an interface name is acceptable in the
        /// configuration as the type of a user type.
        /// </value>
        /// <exception cref="ArgumentNullException">
        /// If the obtained value from the configuration file is <c>null</c>.
        /// </exception>
        /// <exception cref="FormatException">
        /// If the obtained value from the configuration file is not
        /// equivalent to <b>true</b> or <b>false</b>.
        /// </exception>
        protected internal virtual bool IsInterfaceAllowed
        {
            get
            {
                PofConfig cfg = m_cfg;
                return cfg != null && cfg.m_isInterfaceAllowed;
            }
        }

        /// <summary>
        /// Determine if the <b>ConfigurablePofContext</b> supports the
        /// serialization of an object that is an instance of a sub-class of
        /// a configured type, but not actually an instance of a class of a
        /// configured type.
        /// </summary>
        /// <value>
        /// <b>true</b> if serialization of sub-classes is explicitly
        /// enabled.
        /// </value>
        /// <exception cref="ArgumentNullException">
        /// If the obtained value from the configuration file is <c>null</c>.
        /// </exception>
        /// <exception cref="FormatException">
        /// If the obtained value from the configuration file is not
        /// equivalent to <b>true</b> or <b>false</b>.
        /// </exception>
        protected internal virtual bool IsSubclassAllowed
        {
            get
            {
                PofConfig cfg = m_cfg;
                return cfg != null && cfg.m_isSubclassAllowed;
            }
        }

        /// <summary>
        /// Determine if Identity/Reference type support is enabled for this
        /// ConfigurablePofContext.
        /// </summary>
        /// <returns>
        /// <b>true</b> if Identity/Reference type support is enabled
        /// </returns>
        /// <since>Coherence 3.7.1</since>
        public virtual bool IsReferenceEnabled
        {
            get
            {
                return m_isReferenceEnabled;
            }
            set
            {
                m_isReferenceEnabled = value;
            }
        }

        #endregion

        #region IXmlConfigurable implementation

        /// <summary>
        /// <b>IXmlElement</b> holding configuration information.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Note that the configuration will not be available unless the
        /// <b>ConfigurablePofContext</b> was constructed with the
        /// configuration, the configuration was specified using the
        /// <see cref="IXmlConfigurable"/> interface, or the
        /// <b>ConfigurablePofContext</b> has fully initialized itself</p>
        /// <p>
        /// Also, note that the configuration cannot be set after the
        /// <b>ConfigurablePofContext</b> is fully initialized.</p>
        /// </remarks>
        /// <value>
        /// <b>IXmlElement</b> holding configuration information.
        /// </value>
        public virtual IXmlElement Config
        {
            get
            {
                EnsureInitialized();
                return m_cfg.m_xml;
            }
            set
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    if (value != null && !XmlHelper.IsEmpty(value))
                    {
                        CheckNotInitialized();

                        if (m_configFile == null)
                        {
                            // generate a fake locator to use as a unique name
                            // for the configuration, as if it were a URI
                            m_configFile = ResourceLoader.GetResource(
                                StringUtils.ToDecString(
                                    Math.Abs(value.ToString().GetHashCode()), 8));
                        }

                        m_xml = value;
                    }
                }
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
        ///  <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void Serialize(DataWriter writer, object o)
        {
            EnsureInitialized();

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
            EnsureInitialized();

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
            EnsureInitialized();

            IPofSerializer serializer;
            try
            {
                serializer = m_cfg.m_serByTypeId[typeId];
            }
            catch (IndexOutOfRangeException)
            {
                serializer = null;
            }

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
                throw new ArgumentNullException("o", "Argument 'o' cannot be null");
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
        public virtual int GetUserTypeIdentifier(Type type)
        {
            int typeId = GetUserTypeIdentifierInternal(type);
            if (typeId < 0)
            {
                throw new ArgumentException("Unknown user type: " + type.FullName);
            }

            return typeId;
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
        public virtual int GetUserTypeIdentifier(string typeName)
        {
            int typeId = GetUserTypeIdentifierInternal(typeName);
            if (typeId < 0)
            {
                throw new ArgumentException("Unknown user type: " + typeName);
            }

            return typeId;
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
            EnsureInitialized();

            Type type;
            try
            {
                type = m_cfg.m_typeByTypeId[typeId];
            }
            catch (IndexOutOfRangeException)
            {
                type = null;
            }
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
                throw new ArgumentNullException("o", "Argument 'o' cannot be null");
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
        public virtual bool IsUserType(Type type)
        {
            return GetUserTypeIdentifierInternal(type) >= 0;
        }

        /// <summary>
        /// Determine if the type with the given name is a user type known to
        /// this <b>IPofContext</b>.
        /// </summary>
        /// <param name="typeName">
        /// The assembly-qualified name of the type to test; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the type with the specified name is a valid user
        /// type.
        /// </returns>
        public virtual bool IsUserType(string typeName)
        {
            return GetUserTypeIdentifierInternal(typeName) >= 0;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Load and return the default XML POF configuration.
        /// </summary>
        /// <returns>
        /// The default XML POF configuration.
        /// </returns>
        protected static IXmlDocument LoadDefaultPofConfig()
        {
            var config = DefaultPofConfig;
            if (config == null)
            {
                var coherence = (CoherenceConfig)
                        ConfigurationUtils.GetCoherenceConfiguration();
                var resource = coherence?.PofConfig ?? DefaultPofConfigResource;
                config = XmlHelper.LoadResource(resource,
                        "POF configuration");
            }
            return config;
        }

        /// <summary>
        /// Obtain the <see cref="PofConfig"/> that represents the
        /// initialized state of the <b>ConfigurablePofContext</b>.
        /// </summary>
        /// <returns>
        /// The <b>PofConfig</b> for the <b>ConfigurablePofContext</b>, or
        /// <c>null</c> if not yet initialized.
        /// </returns>
        protected internal virtual PofConfig GetPofConfig()
        {
            return m_cfg;
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
        /// type or -1 if the user type is unknown to this
        /// <b>IPofContext</b>.
        /// </returns>
        protected virtual int GetUserTypeIdentifierInternal(Type type)
        {
            EnsureInitialized();

            IDictionary mapTypeIdByType = m_cfg.m_mapTypeIdByType;
            return mapTypeIdByType.Contains(type)
                    ? (int) mapTypeIdByType[type]
                    : GetInheritedUserTypeIdentifier(type);
        }

        /// <summary>
        /// Helper method for determining the user type identifier associated
        /// with a given class that does not have a direct configured
        /// association.
        /// </summary>
        /// <param name="type">
        /// A user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The type identifier of the user type associated with the given
        /// type or -1 if the user type and its superclass(es) and implemented
        /// interface(s) are unknown to this <b>IPofContext</b>.
        /// </returns>
        protected virtual int GetInheritedUserTypeIdentifier(Type type)
        {
            IDictionary mapTypeIdByType = m_cfg.m_mapTypeIdByType;

            if (type == null)
            {
                throw new ArgumentNullException("type", "Argument 'type' is required.");
            }

            if (IsSubclassAllowed)
            {
                Type typeBase = type.BaseType;
                while (typeBase != null)
                {
                    if (mapTypeIdByType.Contains(typeBase))
                    {
                        int typeId = (int) mapTypeIdByType[typeBase];

                        // update the mapping so that we don't have to
                        // brute-force search again
                        using (BlockingLock l = BlockingLock.Lock(this))
                        {
                            mapTypeIdByType = m_cfg.m_mapTypeIdByType;
                            if (!mapTypeIdByType.Contains(type))
                            {
                                mapTypeIdByType = new Hashtable(mapTypeIdByType);
                                mapTypeIdByType[type] = typeId;
                                m_cfg.m_mapTypeIdByType = mapTypeIdByType;
                            }
                        }
                        return typeId;
                    }
                    typeBase = typeBase.BaseType;
                }
            }

            if (IsInterfaceAllowed)
            {
                // check each user type interface to see if the passed class
                // implements it
                foreach (DictionaryEntry entry in mapTypeIdByType)
                {
                    Type currType = (Type) entry.Key;
                    if (currType != null
                            && currType.IsInterface
                            && currType.IsAssignableFrom(type))
                    {
                        int typeId = (int) entry.Value;

                        // update the mapping so that we don't have to
                        // brute-force search again
                        using (BlockingLock l = BlockingLock.Lock(this))
                        {
                            mapTypeIdByType = m_cfg.m_mapTypeIdByType;
                            if (!mapTypeIdByType.Contains(type))
                            {
                                mapTypeIdByType = new Hashtable(mapTypeIdByType);
                                mapTypeIdByType[type] = typeId;
                                m_cfg.m_mapTypeIdByType = mapTypeIdByType;
                            }
                        }
                        return typeId;
                    }
                }
            }

            // update the mapping with the miss so that we don't have to
            // brute-force search again
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                mapTypeIdByType = m_cfg.m_mapTypeIdByType;
                if (!mapTypeIdByType.Contains(type))
                {
                    mapTypeIdByType = new Hashtable(mapTypeIdByType);
                    mapTypeIdByType[type] = -1;
                    m_cfg.m_mapTypeIdByType = mapTypeIdByType;
                }
            }
            return -1;
        }

        /// <summary>
        /// Determine the user type identifier associated with the given type
        /// name.
        /// </summary>
        /// <param name="typeName">
        /// The assembly-qualified name of a user type; must not be <c>null</c>
        /// or empty.
        /// </param>
        /// <returns>
        /// The type identifier of the user type associated with the given
        /// type name or -1 if the user type is unknown to this
        /// <b>IPofContext</b>.
        /// </returns>
        protected virtual int GetUserTypeIdentifierInternal(string typeName)
        {
            EnsureInitialized();

            int         typeId              = -1;
            IDictionary mapTypeIdByTypeName = m_cfg.m_mapTypeIdByTypeName;

            if (StringUtils.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException("Argument 'typeName' must not be null or empty.");
            }
            if (mapTypeIdByTypeName.Contains(typeName))
            {
                typeId = (int) mapTypeIdByTypeName[typeName];
            }
            else
            {
                // special cases: the class name is a sub-class of a user type
                // or a class that implements an interface that is a user type
                if (IsSubclassAllowed || IsInterfaceAllowed)
                {
                    typeId = GetUserTypeIdentifierInternal(TypeResolver.Resolve(typeName));
                    if (typeId >= 0)
                    {
                        using (BlockingLock l = BlockingLock.Lock(this))
                        {
                            mapTypeIdByTypeName = m_cfg.m_mapTypeIdByTypeName;
                            if (!mapTypeIdByTypeName.Contains(typeName))
                            {
                                mapTypeIdByTypeName = new Hashtable(mapTypeIdByTypeName);
                                mapTypeIdByTypeName[typeName] = typeId;
                                m_cfg.m_mapTypeIdByTypeName = mapTypeIdByTypeName;
                            }
                        }
                    }
                }
            }

            return typeId;
        }

        /// <summary>
        /// Verify that the <b>ConfigurablePofContext</b> has not already
        /// been initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the <b>ConfigurablePofContext</b> is already fully
        /// initialized.
        /// </exception>
        protected internal virtual void CheckNotInitialized()
        {
            if (m_cfg != null)
            {
                throw new InvalidOperationException("Already initialized");
            }
        }

        /// <summary>
        /// Fully initialize the <b>ConfigurablePofContext</b> if it has not
        /// already been initialized.
        /// </summary>
        protected internal virtual void EnsureInitialized()
        {
            if (m_cfg == null)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Bind the <b>ConfigurablePofContext</b> to a URI of the
        /// configuration file, resolving all type names, etc.
        /// </summary>
        protected internal virtual void Initialize()
        {
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                if (m_cfg == null)
                {
                    // dereference by URI
                    IDictionary mapConfigByUri = m_mapConfigByUri;
                    PofConfig   cfg;

                    using (BlockingLock l2 = BlockingLock.Lock(mapConfigByUri.SyncRoot))
                    {
                        if (m_configFile == null)
                        {
                            m_configFile = DefaultPofConfigResource;
                            m_xml        = LoadDefaultPofConfig();
                        }
                        IResource configFile = m_configFile;

                        if (mapConfigByUri.Contains(configFile.AbsolutePath))
                        {
                            cfg = (PofConfig) mapConfigByUri[configFile.AbsolutePath];
                        }
                        else
                        {
                            cfg = CreatePofConfig();

                            // now that a PofConfig has been created for the given URI,
                            // store it for future use (assuming that another thread
                            // didn't beat this thread to it)
                            if (mapConfigByUri.Contains(configFile.AbsolutePath))
                            {
                                cfg = (PofConfig) mapConfigByUri[configFile.AbsolutePath];
                            }
                            else
                            {
                                mapConfigByUri[configFile.AbsolutePath] = cfg;
                            }
                        }
                    }

                    // store configuration
                    m_cfg = cfg;
                    m_xml = cfg.m_xml;
                    m_isReferenceEnabled = cfg.m_isReferenceEnabled;
                }
            }
        }

        /// <summary>
        /// Create a <see cref="PofConfig"/> object based on a configuration
        /// that was either provided as XML, or can be loaded from the
        /// specified (or default) URI.
        /// </summary>
        /// <returns>
        /// A <b>PofConfig</b> for this <b>ConfigurablePofContext</b>.
        /// </returns>
        /// <exception cref="SystemException">
        /// If XML configuration file contains bad data.
        /// </exception>
        protected internal virtual PofConfig CreatePofConfig()
        {
            // load the XML configuration if it is not already provided
            string      uri       = m_configFile.AbsolutePath;
            IXmlElement xmlConfig = m_xml;
            if (xmlConfig == null)
            {
                xmlConfig = XmlHelper.LoadResource(m_configFile,
                        "POF configuration");
            }

            // extract options
            bool allowInterfaces  = xmlConfig.GetSafeElement("allow-interfaces").GetBoolean();
            bool allowSubclasses  = xmlConfig.GetSafeElement("allow-subclasses").GetBoolean();
            bool enableReferences = xmlConfig.GetSafeElement("enable-references").GetBoolean();

            // get the type configuration information
            IXmlElement xmlAllTypes = xmlConfig.GetElement("user-type-list");
            if (xmlAllTypes == null)
            {
                ThrowException(uri, -1, null, null, "Missing <user-type-list> element");
            }

            // add default-serializer to each user-type
            AppendDefaultSerializerToUserTypes(xmlConfig);

            // locate and add all included user types
            for (IList listURI = null; xmlAllTypes.GetElement("include") != null; )
            {
                if (listURI == null)
                {
                    listURI = new ArrayList();
                    listURI.Add(uri);
                }

                // load included URIs, checking for duplicates
                IList listInclude = new ArrayList();
                for (IEnumerator enumerator = xmlAllTypes.GetElements("include"); enumerator.MoveNext(); )
                {
                    IResource include    = ResourceLoader.GetResource(((IXmlElement) enumerator.Current).GetString());
                    string    includeUri = include.AbsolutePath;

                    if (!listURI.Contains(includeUri))
                    {
                        listURI.Add(includeUri);
                        listInclude.Add(XmlHelper.LoadResource(include,
                                "included POF configuration"));
                    }
                }
                XmlHelper.RemoveElement(xmlAllTypes, "include");

                // add the user types from all included URIs and adjust options
                foreach (IXmlElement xmlInclude in listInclude)
                {
                    IXmlElement xmlIncludeTypes = xmlInclude.GetSafeElement("user-type-list");

                    AppendDefaultSerializerToUserTypes(xmlInclude);
                    allowInterfaces  |= xmlInclude.GetSafeElement("allow-interfaces").GetBoolean();
                    allowSubclasses  |= xmlInclude.GetSafeElement("allow-subclasses").GetBoolean();
                    enableReferences |= xmlInclude.GetSafeElement("enable-references").GetBoolean();

                    XmlHelper.AddElements(xmlAllTypes, xmlIncludeTypes.GetElements("user-type"));
                    XmlHelper.AddElements(xmlAllTypes, xmlIncludeTypes.GetElements("include"));
                }
            }

            // scan the types for the highest type-id
            IList listTypes     = xmlAllTypes.ElementList;
            int   maxTypeId     = -1;
            bool  isSomeMissing = false;
            bool  isSomePresent = false;

            foreach (IXmlElement xmlType in listTypes)
            {
                if (!xmlType.Name.Equals("user-type"))
                {
                    ThrowException(uri, -1, null, null, "<user-type-list> contains an illegal element: " + xmlType.Name);
                }

                IXmlElement xmlId = xmlType.GetElement("type-id");
                if (xmlId == null)
                {
                    isSomeMissing = true;
                    if (isSomePresent)
                    {
                        ThrowException(uri, -1, null, null,
                            "<user-type-list> contains a" + " <user-type> that is missing a type ID value");
                    }
                }
                else
                {
                    int typeId = xmlId.GetInt(-1);
                    if (typeId < 0)
                    {
                        ThrowException(uri, -1, null, null,
                            "<user-type-list> contains a <user-type> that has a missing or invalid type"
                            + " ID value: " + xmlId.GetString(null));
                    }

                    isSomePresent = true;
                    if (isSomeMissing)
                    {
                        ThrowException(uri, -1, null, null,
                            "<user-type-list> contains a <user-type> that is missing a type ID value");
                    }

                    if (typeId > maxTypeId)
                    {
                        maxTypeId = typeId;
                    }
                }
            }

            bool isAutoNumber = isSomeMissing;
            int  count        = isAutoNumber ? listTypes.Count : maxTypeId + 1;

            // create the relationships between type ids, type names and types
            IDictionary      mapTypeIdByType     = new Hashtable();
            IDictionary      mapTypeIdByTypeName = new Hashtable();
            Type[]           typeByTypeId        = new Type[count];
            IPofSerializer[] serByTypeId         = new IPofSerializer[count];

            int countTypeIds = 0;
            foreach (IXmlElement xmlType in listTypes)
            {
                // determine the user type ID
                int typeId = isAutoNumber ? countTypeIds : xmlType.GetElement("type-id").GetInt();
                if (typeByTypeId[typeId] != null)
                {
                    ThrowException(uri, typeId, null, null, "Duplicate user type id");
                }

                string typeName = xmlType.GetSafeElement("class-name").GetString();
                Type   type     = ResolveType(xmlType, typeId);

                // check if it is an interface or abstract class
                if (type.IsInterface)
                {
                    if (!allowInterfaces)
                    {
                        throw ThrowException(uri, typeId, typeName, null,
                                             "User Type cannot be an interface (allow-interfaces=false)");
                    }
                }
                else if (type.IsAbstract)
                {
                    if (!allowSubclasses)
                    {
                        throw ThrowException(uri, typeId, typeName, null,
                                             "User Type cannot be an abstract class (allow-subclasses=false)");
                    }
                }

                IPofSerializer serializer = GetSerializer(xmlType, type, typeId);

                // store information related to the user type
                mapTypeIdByType.Add(type, typeId);
                mapTypeIdByTypeName.Add(typeName, typeId);
                typeByTypeId[typeId] = type;
                serByTypeId[typeId]  = serializer;

                ++countTypeIds;
            }

            // store off the reusable configuring in a PofConfig object
            PofConfig cfg = new PofConfig();
            cfg.m_xml                 = xmlConfig;
            cfg.m_mapTypeIdByType     = mapTypeIdByType;
            cfg.m_mapTypeIdByTypeName = mapTypeIdByTypeName;
            cfg.m_typeByTypeId        = typeByTypeId;
            cfg.m_serByTypeId         = serByTypeId;
            cfg.m_isInterfaceAllowed  = allowInterfaces;
            cfg.m_isSubclassAllowed   = allowSubclasses;
            cfg.m_isReferenceEnabled  = enableReferences;
            return cfg;
        }

        /// <summary>
        /// Returns <b>Type</b> specified by <paramref name="xmlType"/> with
        /// configuration information.
        /// </summary>
        /// <param name="xmlType">
        /// <b>IXmlElement</b> containing type configuration information.
        /// </param>
        /// <param name="typeId">
        /// Type id to be used for this type.
        /// </param>
        /// <returns>
        /// <b>Type</b> instance specified by configuration xml.
        /// </returns>
        protected virtual Type ResolveType(IXmlElement xmlType, int typeId)
        {
            string uri      = m_configFile.Uri;
            string typeName = xmlType.GetSafeElement("class-name").GetString();
            Type   type;

            if (StringUtils.IsNullOrEmpty(typeName))
            {
                ThrowException(uri, typeId, null, null, "Missing type name");
            }

            try
            {
                type = TypeResolver.Resolve(typeName);
            }
            catch (SystemException e)
            {
                throw ThrowException(uri, typeId, typeName, e, "Unable to load class for user type");
            }

            return type;
        }

        /// <summary>
        /// Returns serializer for user type specified by
        /// <paramref name="xmlType"/>.
        /// </summary>
        /// <param name="xmlType">
        /// <b>IXmlElement</b> with configuration for user type.
        /// </param>
        /// <param name="type">
        /// User type.
        /// </param>
        /// <param name="typeId">
        /// Type id of user type.
        /// </param>
        /// <returns>
        /// <see cref="IPofSerializer"/> instance based on the configuration.
        /// </returns>
        protected virtual IPofSerializer GetSerializer(IXmlElement xmlType, Type type, int typeId)
        {
            string typeName = xmlType.GetSafeElement("class-name").GetString();
            string uri      = m_configFile.Uri;

            // determine the serializer implementation, and register it
            IXmlElement    xmlSer     = xmlType.GetElement("serializer");
            IPofSerializer serializer = null;
            if (xmlSer == null)
            {
                if (typeof(IPortableObject).IsAssignableFrom(type))
                {
                    serializer = new PortableObjectSerializer(typeId);
                }
                else if (Attribute.GetCustomAttribute(type, typeof(Portable)) == null)
                {
                    throw ThrowException(uri, typeId, typeName, null, "Missing IPofSerializer configuration");
                }
                else
                {
                    serializer = new PofAnnotationSerializer(typeId, type);
                }
            }
            else
            {
                string serTypeName = xmlSer.GetElement("class-name").GetString();
                if (StringUtils.IsNullOrEmpty(serTypeName))
                {
                    ThrowException(uri, typeId, typeName, null, "Missing IPofSerializer class name");
                }

                // load the class for the user type, and register it
                Type serType;
                try
                {
                    serType = TypeResolver.Resolve(serTypeName);
                }
                catch (SystemException e)
                {
                    throw ThrowException(uri, typeId, typeName, e,
                                         "Unable to load IPofSerializer class: " + serTypeName);
                }

                if (!typeof(IPofSerializer).IsAssignableFrom(serType))
                {
                    throw ThrowException(uri, typeId, typeName, null,
                                         "Class is not an IPofSerializer: " + serTypeName);
                }

                // only attempt the default IPofSerializer constructors if
                // there are no parameters specified, or if there is at least
                // one parameter specified, but it doesn't have a type (which
                // indicates that the serializer is IXmlConfigurable using a
                // transposed form of the parameters)
                IXmlElement xmlParams = xmlSer.GetElement("init-params");
                if (xmlParams == null)
                {
                    try
                    {
                        serializer = CreateDefaultSerializer(serType, type, typeId);
                    }
                    catch(Exception e)
                    {
                        // all three constructors failed, so use the exception from this
                        // most recent failure as the basis for reporting the failure
                        throw ThrowException(uri, typeId, typeName, e,
                                             "Unable to instantiate IPofSerializer class using"
                                             + " predefined constructors: " + serTypeName);
                    }
                }
                else
                {
                    for (IEnumerator enumerator = xmlParams.GetElements("init-param"); enumerator.MoveNext(); )
                    {
                        if (((IXmlElement) enumerator.Current).GetElement("param-type") == null)
                        {
                            try
                            {
                                serializer = CreateDefaultSerializer(serType, type, typeId);
                            }
                            catch (Exception e)
                            {
                                // all three constructors failed, so use the exception from this
                                // most recent failure as the basis for reporting the failure
                                throw ThrowException(uri, typeId, typeName, e,
                                                     "Unable to instantiate IPofSerializer class using"
                                                     + " predefined constructors: " + serTypeName);
                            }
                        }
                    }
                }

                if (serializer == null)
                {
                    object[] parameters;
                    try
                    {
                        parameters = XmlHelper.ParseInitParams(xmlParams, new PofConfigParameterResolver(typeName, typeId, type));
                    }
                    catch (Exception e)
                    {
                        throw ThrowException(uri, typeId, typeName, e,
                                             "Error parsing constructor parameters for IPofSerializer:"
                                             + serTypeName);
                    }

                    try
                    {
                        serializer = CreateSerializerInstance(serType, parameters);
                    }
                    catch (Exception e)
                    {
                        throw ThrowException(uri, typeId, typeName, e,
                                             "Unable to instantiate IPofSerializer class: "
                                             + serTypeName);
                    }
                }
            }

            // PofSerializer initialization: IXmlConfigurable
            if (serializer is IXmlConfigurable)
            {
                try
                {
                    IXmlElement xmlParams = new SimpleElement("config");
                    XmlHelper.TransformInitParams(xmlParams, xmlSer.GetSafeElement("init-params"));
                    ((IXmlConfigurable) serializer).Config = xmlParams;
                }
                catch (Exception e)
                {
                    ThrowException(uri, typeId, typeName, e, "Unable to configure IPofSerializer");
                }
            }
            return serializer;
        }

        /// <summary>
        /// Process &lt;default-serializer&gt; element from specified xml
        /// configuration and appends information about serializer to each
        /// &lt;user-type&gt; element within &lt;user-type-list&gt; unless
        /// user type already has serializer specified.
        /// </summary>
        /// <param name="xmlConfig">
        /// <b>IXmlElement</b> containing pof configuration.
        /// </param>
        protected virtual void AppendDefaultSerializerToUserTypes(IXmlElement xmlConfig)
        {
            IXmlElement xmlDefaultSerializer = xmlConfig.GetElement("default-serializer");

            if (xmlDefaultSerializer != null)
            {
                IXmlElement xmlAllTypes = xmlConfig.GetElement("user-type-list");

                for (IEnumerator enumerator = xmlAllTypes.GetElements("user-type"); enumerator.MoveNext(); )
                {
                    IXmlElement xmlType = (IXmlElement) enumerator.Current;
                    IXmlElement xmlSer  = xmlType.GetElement("serializer");

                    if (xmlSer == null)
                    {
                        // add the default-serializer to this user-type
                        IXmlElement xmlNewSer = (IXmlElement) xmlDefaultSerializer.Clone();
                        xmlNewSer.Name = "serializer";
                        xmlType.ElementList.Add(xmlNewSer);
                    }
                }
            }
        }

        /// <summary>
        /// Tries to create instance of <see cref="IPofSerializer"/> using
        /// default constructors.
        /// </summary>
        /// <remarks>
        /// <b>ConfigurablePofContext</b> attempts to construct the
        /// <b>IPofSerializer</b> by searching for one of the following
        /// constructors in the same order as they appear here:
        /// <list type="bullet">
        /// <item>
        /// <description>(int userTypeId, Type userType)</description>
        /// </item>
        /// <item><description>(int userTypeId)</description></item>
        /// <item><description>()</description></item>
        /// </list>
        /// </remarks>
        /// <param name="serializerType">
        /// Type of the <b>IPofSerializer</b>.
        /// </param>
        /// <param name="userType">
        /// Type of the object to serialize.
        /// </param>
        /// <param name="userTypeId">
        /// Id of the object to serialize.
        /// </param>
        /// <returns>
        /// An instance of <see cref="IPofSerializer"/> interface.
        /// </returns>
        /// <exception cref="SystemException">
        /// If for any reason making the instance of the serializer was
        /// unsucessful.
        /// </exception>
        private IPofSerializer CreateDefaultSerializer(Type serializerType, Type userType, int userTypeId)
        {
            return (IPofSerializer)
                (ObjectUtils.CreateInstanceSafe(serializerType, userTypeId, userType) ??
                 ObjectUtils.CreateInstanceSafe(serializerType, userTypeId) ??
                 Activator.CreateInstance(serializerType));
        }

        private IPofSerializer CreateSerializerInstance(Type serializerType, params object[] parameters)
        {
            return (IPofSerializer) ObjectUtils.CreateInstance(serializerType, parameters);
        }

        /// <summary>
        /// Assemble and throw an informative exception based on the passed
        /// details.
        /// </summary>
        /// <param name="uri">
        /// The URI of the configuration.
        /// </param>
        /// <param name="typeId">
        /// The type ID (if applicable and if known).
        /// </param>
        /// <param name="typename">
        /// The user type type name (if applicable and if known).
        /// </param>
        /// <param name="e">
        /// The underlying exception, if any.
        /// </param>
        /// <param name="message">
        /// The detailed description of the problem.
        /// </param>
        /// <returns>
        /// This method does not return; it always throws an exception.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Always thrown.
        /// </exception>
        protected virtual Exception ThrowException(string uri, int typeId, string typename,
                                                 Exception e, string message)
        {
            StringBuilder sb = new StringBuilder();

            if (!StringUtils.IsNullOrEmpty(uri))
            {
                sb.Append("Config=").Append(uri);
            }

            if (typeId >= 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append("Type-Id=").Append(typeId);
            }

            if (!StringUtils.IsNullOrEmpty(typename))
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append("Type-Name=").Append(typename);
            }

            if (sb.Length > 0)
            {
                message = message + " (" + sb + ')';
            }

            throw e == null
                  ? new InvalidOperationException(message)
                  : new InvalidOperationException(message, e);
        }

        #endregion

        #region Inner class: PofConfig

        /// <summary>
        /// The information related to the configuration of a particular
        /// <see cref="IPofContext"/> for a specific URI.
        /// </summary>
        protected internal class PofConfig
        {
            /// <summary>
            /// The XML configuration, if supplied by constructor.
            /// </summary>
            public IXmlElement m_xml;

            /// <summary>
            /// Once initialized, this references a non thread-safe map that
            /// contains mappings from .NET types to POF type identifiers.
            /// </summary>
            /// <remarks>
            /// The initial contents of the map reflect the configuration,
            /// but the contents can increase over time as sub-classes of the
            /// contained classes are resolved to type IDs (and those
            /// mappings are added).
            /// </remarks>
            public IDictionary m_mapTypeIdByType;

            /// <summary>
            /// Once initialized, this references a non thread-safe map that
            /// contains mappings from .NET type names to POF type
            /// identifiers.
            /// </summary>
            /// <remarks>
            /// The initial contents of the map reflect the configuration,
            /// but the contents can increase over time as the names of
            /// sub-classes (i.e. of the classes corresponding to the
            /// contained class names) are resolved to type IDs (and those
            /// mappings are added).
            /// </remarks>
            public IDictionary m_mapTypeIdByTypeName;

            /// <summary>
            /// An array of user <b>Type</b>s, indexed by type identifier.
            /// </summary>
            public Type[] m_typeByTypeId;

            /// <summary>
            /// An array of <see cref="IPofSerializer"/> objects, indexed by
            /// type identifier.
            /// </summary>
            public IPofSerializer[] m_serByTypeId;

            /// <summary>
            /// <b>true</b> iff an interface name is acceptable in the
            /// configuration as the class of a user type.
            /// </summary>
            public bool m_isInterfaceAllowed;

            /// <summary>
            /// <b>true</b> iff serialization of sub-classes is explicitly
            /// enabled.
            /// </summary>
            public bool m_isSubclassAllowed;

            /// <summary>
            /// <b>true</b> iff POF Identity/Reference type support is enabled.
            /// </summary>
            public bool m_isReferenceEnabled;
        }

        #endregion

        #region Inner class: PofConfigParameterResolver

        /// <summary>
        /// An <see cref="XmlHelper.IParameterResolver"/> implementation used
        /// by ConfigurablePofContext when resolving serializer
        /// configuration.
        /// </summary>
        /// <seealso cref="ConfigurablePofContext.GetSerializer"/>
        protected class PofConfigParameterResolver : XmlHelper.IParameterResolver
        {
            /// <summary>
            /// Creates PofConfigParameterResolver with parameter values for
            /// {type-id}, {class-name} and {class} macros.
            /// </summary>
            /// <param name="className">
            /// Type name.
            /// </param>
            /// <param name="userTypeId">
            /// User type id.
            /// </param>
            /// <param name="userType">
            /// User type.
            /// </param>
            public PofConfigParameterResolver(string className, int userTypeId, Type userType)
            {
                m_className  = className;
                m_userTypeId = userTypeId;
                m_userType   = userType;
            }

            /// <summary>
            /// Resolve the passed substitutable parameter.
            /// </summary>
            /// <param name="type">
            /// The value of the "param-type" element.
            /// </param>
            /// <param name="value">
            /// The value of the "param-value" element, which is enclosed by
            /// curly braces, indicating its substitutability.
            /// </param>
            /// <returns>
            /// The object value to use or the
            /// <see cref="XmlHelper.UNRESOLVED"/> constant.
            /// </returns>
            public virtual object ResolveParameter(string type, string value)
            {
                if (value.Equals("{type-id}"))
                {
                    return m_userTypeId;
                }
                else if (value.Equals("{class-name}"))
                {
                    return m_className;
                }
                else if (value.Equals("{class}"))
                {
                    return m_userType;
                }
                else
                {
                    return value;
                }
            }

            private string m_className;
            private int m_userTypeId;
            private Type m_userType;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The default location of the POF configuration file.
        /// </summary>
        private static IResource s_configResource = ResourceLoader.GetResource(
                "pof-config.xml", true);

        /// <summary>
        /// Map of configuration information, keyed by URI of the
        /// configuration file.
        /// </summary>
        private static readonly IDictionary m_mapConfigByUri = new HashDictionary();

        /// <summary>
        ///  The resource that specifies the location of the configuration file.
        /// </summary>
        private IResource m_configFile;

        /// <summary>
        /// The XML configuration, if supplied by constructor.
        /// </summary>
        private IXmlElement m_xml;

        /// <summary>
        /// <b>true</b> if POF Identity/Reference type support is enabled.
        /// </summary>
        private bool m_isReferenceEnabled;

        /// <summary>
        /// The <see cref="PofConfig"/> for this <see cref="IPofContext"/> to
        /// use.
        /// </summary>
        private PofConfig m_cfg;

        #endregion
    }
}
