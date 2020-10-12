/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

using Tangosol.Config;
using Tangosol.IO;
using Tangosol.IO.Resources;
using Tangosol.Net.Security;
using Tangosol.Net.Security.Impl;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Collections;
using Tangosol.Util.Logging;

namespace Tangosol.Net
{
    /// <summary>
    /// The DefaultOperationalContext provides an <see cref="IOperationalContext"/>
    /// with information optained from XML in coherence.xsd format and default
    /// values.
    /// </summary>
    /// <author>Wei Lin  2010.11.3</author>
    /// <since>Coherence 3.7</since>
    public class DefaultOperationalContext : IOperationalContext
    {
        #region Properties

        /// <summary>
        /// The <see cref="IResource"/> for the default XML configuration used
        /// when one isn't explicitly passed in the constructor for this class.
        /// </summary>
        /// <value>
        /// The <see cref="IResource"/> for the default XML configuration.
        /// </value>
        public static IResource DefaultOperationalConfigResource
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
        public static IXmlDocument DefaultOperationalConfig
        {
            get; set;
        }

        /// <summary>
        /// The current configuration of the object.
        /// </summary>
        /// <value>
        /// The XML configuration or <c>null</c>.
        /// </value>
        public virtual IXmlElement Config
        {
            get { return m_config; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor. Initializes the default location of the
        /// operational configuration file.
        /// </summary>
        static DefaultOperationalContext()
        {
            s_configResource = ResourceLoader.GetResource("coherence.xml", true);

            // if the user-specific default operaational configuration file
            // doesn't exist, use the one embedded into Coherence.dll
            if (!File.Exists(s_configResource.AbsolutePath))
            {
                s_configResource = new EmbeddedResource(string.Format(
                        "assembly://{0}/Tangosol.Config/coherence.xml",
                        typeof(CacheFactory).Assembly.FullName));
            }
        }

        /// <summary>
        /// Construct a new DefaultOperationalContext.
        /// </summary>
        public DefaultOperationalContext() : this(null)
        {}

        /// <summary>
        /// Construct a new DefaultOperationalContext.
        /// </summary>
        /// <param name="config">
        /// An XML element corresponding to coherence.xsd.
        /// </param>
        public DefaultOperationalContext(IXmlElement config)
        {
            if (config == null)
            {
                config = LoadDefaultOperationalConfig();
            }
            m_config = config;

            ParseEditionConfig();
            ParseLoggingConfig();
            ParseLocalMemberConfig();
            ParseFilterConfig();
            ParseSerializerConfig();
            ParseAddressProviderConfig();
            ParseSecurityConfig();

            DiscoveryTimeToLive = config.GetSafeElement("multicast-listener/time-to-live").GetInt(4);
        }

        #endregion

        #region IOperationalContext implementation

        /// <summary>
        /// The TTL for multicast based discovery.
        /// </summary>
        /// <since>12.2.1</since>
        public virtual int DiscoveryTimeToLive { get; protected set; }

        /// <summary>
        /// The product edition.
        /// </summary>
        /// <value>
        /// The product edition.
        /// </value>
        public virtual int Edition { get; private set; }

        /// <summary>
        /// The product edition in a formatted string.
        /// </summary>
        /// <value>
        /// The product edition in a formatted string.
        /// </value>
        public virtual string EditionName { get; private set; }

        /// <summary>
        /// An <see cref="IMember"/> object representing this process.
        /// </summary>
        /// <value>
        /// The local <see cref="IMember"/>.
        /// </value>
        public virtual IMember LocalMember { get; private set; }

        /// <summary>
        /// A dictionary of network filter factories.
        /// </summary>
        /// <value>
        /// A dictionary of <see cref="IWrapperStreamFactory"/> objects keyed
        /// by filter name.
        /// </value>
        public virtual IDictionary FilterMap { get; private set; }

        /// <summary>
        /// A dictionary of serializer factories.
        /// </summary>
        /// <value>
        /// A dictionary of <see cref="ISerializerFactory"/> objects keyed
        /// by serializer name.
        /// </value>
        public IDictionary SerializerMap { get; private set; }

        /// <summary>
        /// A dictionary of address provider factories.
        /// </summary>
        /// <value>
        /// A dictionary of <see cref="IAddressProviderFactory"/> objects keyed
        /// by name.
        /// </value>
        public IDictionary AddressProviderMap { get; private set; }

        /// <summary>
        /// An <see cref="IIdentityAsserter"/> that can be used to establish a
        /// user's identity.
        /// </summary>
        /// <value>
        /// The <see cref="IIdentityAsserter"/>.
        /// </value>
        public virtual IIdentityAsserter IdentityAsserter { get; private set; }

        /// <summary>
        /// An <see cref="IIdentityTransformer"/> that can be used to transform
        /// an IPrincipal into an identity assertion.
        /// </summary>
        /// <value>
        /// The <see cref="IIdentityTransformer"/>.
        /// </value>
        public virtual IIdentityTransformer IdentityTransformer { get; private set; }

        /// <summary>
        /// Indicates if principal scoping is enabled.
        /// </summary>
        /// <value>
        /// <b>true</b> if principal scoping is enabled.
        /// </value>
        public virtual bool IsPrincipalScopingEnabled { get; private set; }

        /// <summary>
        /// The logging severity level.
        /// </summary>
        /// <value>
        /// The loggng severity level.
        /// </value>
        public virtual int LogLevel { get; private set; }

        /// <summary>
        /// The maximum number of characters for a logger daemon to queue
        /// before truncating.
        /// </summary>
        /// <value>
        /// The maximum number of characters for a logger daemon to queue
        /// before truncating.
        /// </value>
        public virtual int LogCharacterLimit { get; private set; }

        /// <summary>
        /// The log message format.
        /// </summary>
        /// <value>
        /// The log message format.
        /// </value>
        public virtual string LogMessageFormat { get; private set; }

        /// <summary>
        /// The destination for log messages.
        /// </summary>
        /// <value>
        /// The destination for log messages.
        /// </value>
        public virtual string LogDestination { get; private set; }

        /// <summary>
        /// The name of the logger.
        /// </summary>
        /// <value>
        /// The name of the logger.
        /// </value>
        public virtual string LogName { get; private set; }

        #endregion

        #region Internal methods

        /// <summary>
        /// Load and return the default XML operational configuration.
        /// </summary>
        /// <returns>
        /// The default XML operational configuration.
        /// </returns>
        protected static IXmlDocument LoadDefaultOperationalConfig()
        {
            var config = DefaultOperationalConfig;
            if (config == null)
            {
                var coherence = (CoherenceConfig)
                        ConfigurationUtils.GetCoherenceConfiguration();
                var resource = (coherence == null ? null : coherence.OperationalConfig)
                        ?? DefaultOperationalConfigResource;
                config = XmlHelper.LoadResource(resource,
                        "operational configuration");
            }
            config.IterateThroughAllNodes(CacheFactory.PreprocessProp);
            return config;
        }

        /// <summary>
        /// Parse and configure product edition information.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the configured edition name is invalid.
        /// </exception>
        private void ParseEditionConfig()
        {
            var editionName   = DEFAULT_EDITION_NAME;
            var licenseConfig = Config.GetElement("license-config");
            if (licenseConfig != null)
            {
                editionName = licenseConfig.GetSafeElement("edition-name")
                        .GetString(DEFAULT_EDITION_NAME);
            }
            EditionName = editionName;

            int edition;
            switch (editionName)
            {
                case "DC":
                    edition = 0;
                    break;
                case "RTC":
                    edition = 1;
                    break;
                default:
                    throw new InvalidOperationException(
                            "Specified edition name is not allowed for the client.");
            }
            Edition = edition;
        }

        /// <summary>
        /// Parse and configure logging information.
        /// </summary>
        private void ParseLoggingConfig()
        {
            var config = Config.GetElement("logging-config");

            LogDestination = config.GetSafeElement("destination")
                    .GetString(Logger.DefaultDestination);
            LogMessageFormat = config.GetSafeElement("message-format")
                    .GetString(Logger.DefaultFormat);
            LogLevel = config.GetSafeElement("severity-level")
                    .GetInt(Logger.DefaultLevel);
            LogCharacterLimit = config.GetSafeElement("character-limit")
                    .GetInt(Logger.DefaultLimit);
            LogName = config.GetSafeElement("logger-name")
                    .GetString(Logger.DefaultName);
        }

        /// <summary>
        /// Parse and configure local <see cref="IMember"/> information.
        /// </summary>
        private void ParseLocalMemberConfig()
        {
            LocalMember member = new LocalMember();

            IXmlElement xmlConfig = Config.FindElement("cluster-config/member-identity");
            if (xmlConfig != null)
            {
                member.ClusterName = xmlConfig.GetSafeElement("cluster-name").GetString();
                member.SiteName    = xmlConfig.GetSafeElement("site-name").GetString();
                member.RackName    = xmlConfig.GetSafeElement("rack-name").GetString();
                member.MachineName = xmlConfig.GetSafeElement("machine-name").GetString();
                member.ProcessName = xmlConfig.GetSafeElement("process-name").GetString();
                member.MemberName  = xmlConfig.GetSafeElement("member-name").GetString();
                member.RoleName    = xmlConfig.GetSafeElement("role-name").GetString();
            }

            if (StringUtils.IsNullOrEmpty(member.ClusterName))
            {
                // set default cluster name to the user name
                string name = Environment.UserName;
                if (StringUtils.IsNullOrEmpty(name))
                {
                    // we can't obtain the user name, this could be a transient error for instance in the case of NIS.
                    // while we could generate some random or fixed default that wouldn't defend well against transient errors
                    // and we could end up with multiple clusters.  Given that any production system should actually set the
                    // cluster name rather then using a default we will treat this as a hard error.  Note don't try to obtain
                    // the name by other means such as reading env variables because they may produce a different string then
                    // reading "user.name" and again if the error is transient multiple clusters could be unintentionally produced.

                    throw new NotSupportedException(
                        "unable to generate a default cluster name, user name is not available, explicit cluster name configuration is required");
                }

                // this suffix in addition to be cute and suggesting this is not a production cluster also helps
                // minimize the possibility of a collision with a manually named cluster which would be very unlikely
                // to use such a cute name.
                member.ClusterName = name = name + "'s cluster";
                CacheFactory.Log("The cluster name has not been configured, a value of \"" + name + "\" has been automatically generated", CacheFactory.LogLevel.Info);
            }

            if (StringUtils.IsNullOrEmpty(member.MachineName))
            {
                var host  = System.Net.Dns.GetHostName();
                var delim = host.IndexOf('.');

                if (delim == -1 || !Char.IsLetter(host[0]))
                {
                    member.MachineName = host;
                }
                else
                {
                    member.MachineName = host.Substring(0, delim);
                    member.SiteName    = host.Substring(delim + 1);
                }
            }

            if (StringUtils.IsNullOrEmpty(member.RoleName))
            {
                member.RoleName = ".NET " + EditionName + " client";
            }

            if (StringUtils.IsNullOrEmpty(member.ProcessName))
            {
                member.ProcessName = Process.GetCurrentProcess().ProcessName;
            }

            LocalMember = member;
        }

        /// <summary>
        /// Parse and configure network filter information.
        /// </summary>
        private void ParseFilterConfig()
        {
            IDictionary filterMap = new HashDictionary();

            var config = Config.FindElement("cluster-config/filters");
            if (config != null)
            {
                for (var filters = config.GetElements("filter"); filters.MoveNext(); )
                {
                    var xmlFilter  = (IXmlElement) filters.Current;
                    var name       = xmlFilter.GetSafeElement("filter-name").GetString();
                    var className  = xmlFilter.GetSafeElement("filter-class").GetString();
                    var initParams = xmlFilter.GetElement("init-params");

                    var xmlFilterClass = new SimpleElement("instance");
                    xmlFilterClass.AddElement("class-name").SetString(className);
                    if (initParams != null)
                    {
                        var xmlParams = xmlFilterClass.AddElement("init-params");
                        for (var init = initParams.GetElements("init-param"); init.MoveNext(); )
                        {
                            xmlParams.ElementList.Add(init.Current);
                        }
                    }
                    var factory = (IWrapperStreamFactory) XmlHelper.CreateInstance(
                            xmlFilterClass, null, typeof(IWrapperStreamFactory));

                    filterMap.Add(name, factory);
                }
            }

            // add well known "gzip" filter
            if (filterMap["gzip"] == null)
            {
                filterMap["gzip"] = new CompressionFilter();
            }

            FilterMap = filterMap;
        }

        /// <summary>
        /// Parse and configure serializer information.
        /// </summary>
        private void ParseSerializerConfig()
        {
            IDictionary serializerMap = new HashDictionary();

            var config = Config.FindElement("cluster-config/serializers");
            if (config != null)
            {
                for (var serializers = config.GetElements("serializer"); serializers.MoveNext(); )
                {
                    var xmlSerializer = (IXmlElement) serializers.Current;
                    var name          = xmlSerializer.GetAttribute("id").GetString();

                    ConfigurableSerializerFactory factory = new ConfigurableSerializerFactory();
                    factory.Config = xmlSerializer;

                    serializerMap.Add(name, factory);
                }
            }

            // check that the well-known pof serializer is present
            String serializerName = "pof";
            if (!serializerMap.Contains(serializerName))
            {
                IXmlElement pofSerializer = new SimpleElement("serializer");
                IXmlElement xmlInstance   = pofSerializer.EnsureElement("instance");
                xmlInstance.EnsureElement("class-name").SetString("Tangosol.IO.Pof.ConfigurablePofContext, Coherence");

                ConfigurableSerializerFactory factory = new ConfigurableSerializerFactory();
                factory.Config = pofSerializer;

                serializerMap.Add(serializerName, factory);
            }

            SerializerMap = serializerMap;
        }

        /// <summary>
        /// Parse and configure address provider information.
        /// </summary>
        private void ParseAddressProviderConfig()
        {
            IDictionary addressProviderMap = new HashDictionary();

            var config = Config.FindElement("cluster-config/address-providers");
            if (config != null)
            {
                for (var addressProviders = config.GetElements("address-provider");
                        addressProviders.MoveNext(); )
                {
                    var xmlAddressProvider = (IXmlElement) addressProviders.Current;
                    var name               = xmlAddressProvider.GetAttribute("id").GetString();

                    ConfigurableAddressProviderFactory factory = new ConfigurableAddressProviderFactory();
                    factory.Config = xmlAddressProvider;

                    addressProviderMap.Add(name, factory);
                }
            }

            AddressProviderMap = addressProviderMap;
        }
        
        /// <summary>
        /// Parse and configure security-related information.
        /// </summary>
        private void ParseSecurityConfig()
        {
            IIdentityAsserter    asserter    = DefaultIdentityAsserter.Instance;
            IIdentityTransformer transformer = DefaultIdentityTransformer.Instance;

            var xmlConfig = Config.FindElement("security-config");
            if (xmlConfig != null)
            {
                var xmlIdentityAsserter = xmlConfig.GetElement("identity-asserter");
                if (xmlIdentityAsserter != null)
                {
                    asserter = (IIdentityAsserter) XmlHelper.CreateInstance(
                            xmlIdentityAsserter, null, typeof(IIdentityAsserter));
                }

                var xmlIdentityTransformerer = xmlConfig.GetElement("identity-transformer");
                if (xmlIdentityTransformerer != null)
                {
                    transformer = (IIdentityTransformer) XmlHelper.CreateInstance(
                            xmlIdentityTransformerer, null, typeof(IIdentityTransformer));
                }
                IsPrincipalScopingEnabled = xmlConfig.GetSafeElement(
                        "principal-scope").GetBoolean();
            }

            IdentityAsserter    = asserter;
            IdentityTransformer = transformer;
        }

        #endregion

        #region Constants

        /// <summary>
        /// The default edition name.
        /// </summary>
        public const string DEFAULT_EDITION_NAME = "RTC";

        #endregion

        #region Data members

        /// <summary>
        /// The default location of the operational configuration file.
        /// </summary>
        private static IResource s_configResource;

        /// <summary>
        /// The configuration XML.
        /// </summary>
        private readonly IXmlElement m_config;

        #endregion
    }
}