/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Xml;

using Tangosol.IO.Pof;
using Tangosol.IO.Resources;
using Tangosol.Net.Impl;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Logging;

namespace Tangosol.Net
{
    /// <summary>
    /// Factory for the <b>Oracle Coherence&#8482; for .NET</b> product.
    /// </summary>
    /// <remarks>
    /// <p>
    /// One of the most common functions provided by the CacheFactory is
    /// ability to obtain an instance of a cache. There are various cache
    /// services and cache topologies that are supported by Coherence.</p>
    /// <p>
    /// To get a cache reference use the <see cref="GetCache"/> method.</p>
    /// <p>
    /// This approach that has a lot of advantages over service type specific
    /// methods described further below because:</p>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// complex cache topology could be configured declaratively in the cache
    /// configuration XML rather then programmaticaly via API;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// the caller's code could become completely generic and agnostic to the
    /// cache topology;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// the cache topology decision could be deferred and made much later in
    /// the development cycle without changing the application code.
    /// </description>
    /// </item>
    /// </list>
    /// <p>
    /// When a cache is no longer used, it is preferrable to call
    /// <see cref="ReleaseCache"/> to release the associated resources. To
    /// destroy all instances of the cache across the cluster, use
    /// <see cref="DestroyCache"/>.</p>
    /// <p>
    /// Other services:</p>
    /// <list type="bullet">
    /// <item>
    /// <term>Invocation</term>
    /// <description>
    /// Invocation service provides the means for invoking and monitoring
    /// execution of classes on specified nodes across a cluster.<p/>
    /// The following factory method returns an instance of Invocation
    /// service:<p/>
    /// <see cref="GetService(string)"/>
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <author>Cameron Purdy  2001.12.14</author>
    /// <author>Gene Gleyzer</author>
    /// <author>Ana Cikic  2006.09.19</author>
    /// <author>Aleksandar Seovic  2008.10.09</author>
    /// <author>Jason Howes  2010.12.15</author>
    public abstract class CacheFactory
    {
        #region Properties

        /// <summary>
        /// The path to the default XML cache configuration.
        /// </summary>
        /// <value>
        /// The path to the default XML cache configuration.
        /// </value>
        /// <since>Coherence 3.7</since>
        public static string DefaultCacheConfigPath
        {
            get
            {
                return DefaultConfigurableCacheFactory.DefaultCacheConfigResource.AbsolutePath;
            }
            set
            {
                DefaultConfigurableCacheFactory.DefaultCacheConfigResource
                        = ResourceLoader.GetResource(value);
            }
        }

        /// <summary>
        /// The <see cref="IResource"/> for the default XML cache
        /// configuration.
        /// </summary>
        /// <value>
        /// The <see cref="IResource"/> for the default XML cache
        /// configuration.
        /// </value>
        /// <since>Coherence 3.7</since>
        public static IResource DefaultCacheConfigResource
        {
            get
            {
                return DefaultConfigurableCacheFactory.DefaultCacheConfigResource;
            }
            set
            {
                DefaultConfigurableCacheFactory.DefaultCacheConfigResource = value;
            }
        }

        /// <summary>
        /// The default XML cache configuration.
        /// </summary>
        /// <value>
        /// The default XML cache configuration.
        /// </value>
        /// <since>Coherence 3.7</since>
        public static IXmlDocument DefaultCacheConfig
        {
            get
            {
                return DefaultConfigurableCacheFactory.DefaultCacheConfig;
            }
            set
            {
                DefaultConfigurableCacheFactory.DefaultCacheConfig = value;
            }
        }

        /// <summary>
        /// The path to the default XML operational configuration.
        /// </summary>
        /// <value>
        /// The path to the default XML operational configuration.
        /// </value>
        /// <since>Coherence 3.7</since>
        public static string DefaultOperationalConfigPath
        {
            get
            {
                return DefaultOperationalContext.DefaultOperationalConfigResource.AbsolutePath;
            }
            set
            {
                DefaultOperationalContext.DefaultOperationalConfigResource
                        = ResourceLoader.GetResource(value);
            }
        }

        /// <summary>
        /// The <see cref="IResource"/> for the default XML operational
        /// configuration.
        /// </summary>
        /// <value>
        /// The <see cref="IResource"/> for the default XML operational
        /// configuration.
        /// </value>
        /// <since>Coherence 3.7</since>
        public static IResource DefaultOperationalConfigResource
        {
            get
            {
                return DefaultOperationalContext.DefaultOperationalConfigResource;
            }
            set
            {
                DefaultOperationalContext.DefaultOperationalConfigResource = value;
            }
        }

        /// <summary>
        /// The default XML operational configuration.
        /// </summary>
        /// <value>
        /// The default XML operational configuration.
        /// </value>
        /// <since>Coherence 3.7</since>
        public static IXmlDocument DefaultOperationalConfig
        {
            get
            {
                return DefaultOperationalContext.DefaultOperationalConfig;
            }
            set
            {
                DefaultOperationalContext.DefaultOperationalConfig = value;
            }
        }

        /// <summary>
        /// The path to the default XML POF configuration.
        /// </summary>
        /// <value>
        /// The path to the default XML POF configuration.
        /// </value>
        /// <since>Coherence 3.7</since>
        public static string DefaultPofConfigPath
        {
            get
            {
                return ConfigurablePofContext.DefaultPofConfigResource.AbsolutePath;
            }
            set
            {
                ConfigurablePofContext.DefaultPofConfigResource
                        = ResourceLoader.GetResource(value);
            }
        }

        /// <summary>
        /// The <see cref="IResource"/> for the default XML POF
        /// configuration.
        /// </summary>
        /// <value>
        /// The <see cref="IResource"/> for the default XML POF
        /// configuration.
        /// </value>
        /// <since>Coherence 3.7</since>
        public static IResource DefaultPofConfigResource
        {
            get
            {
                return ConfigurablePofContext.DefaultPofConfigResource;
            }
            set
            {
                ConfigurablePofContext.DefaultPofConfigResource = value;
            }
        }

        /// <summary>
        /// The default XML POF configuration.
        /// </summary>
        /// <value>
        /// The default XML POF configuration.
        /// </value>
        /// <since>Coherence 3.7</since>
        public static IXmlDocument DefaultPofConfig
        {
            get
            {
                return ConfigurablePofContext.DefaultPofConfig;
            }
            set
            {
                ConfigurablePofContext.DefaultPofConfig = value;
            }
        }

        /// <summary>
        /// The <see cref="IConfigurableCacheFactory"/> singleton.
        /// </summary>
        /// <value>
        /// An instance of <b>IConfigurableCacheFactory</b>.
        /// </value>
        /// <since>Coherence 2.2</since>
        public static IConfigurableCacheFactory ConfigurableCacheFactory
        {
            get
            {
                IConfigurableCacheFactory factory = s_factory;
                if (factory == null)
                {
                    lock (typeof(CacheFactory))
                    {
                        factory = s_factory;
                        if (factory == null)
                        {
                            Configure((string) null, null);
                        }
                        factory = s_factory;
                    }
                }
                return factory;
            }

            set
            {
                IOperationalContext ctx = null;
                if (value is DefaultConfigurableCacheFactory)
                {
                    ctx = ((DefaultConfigurableCacheFactory) value).OperationalContext;
                }
                Configure(value, ctx);
            }
        }

        #endregion

        #region Lifecycle methods

        /// <summary>
        /// Configure the CacheFactory.
        /// </summary>
        /// <param name="cacheConfig">
        /// An optional path to a file that conforms to <c>cache-config.xsd</c>.
        /// </param>
        /// <param name="coherenceConfig">
        /// An optional path to a file that conforms to <c>coherence.xsd</c>.
        /// </param>
        /// <since>Coherence 3.7</since>
        public static void Configure(String cacheConfig, String coherenceConfig)
        {
            IResource cacheResource     = null;
            IResource coherenceResource = null;

            // load all specified resources
            if (cacheConfig != null)
            {
                cacheResource = ResourceLoader.GetResource(cacheConfig);
            }
            if (coherenceConfig != null)
            {
                coherenceResource = ResourceLoader.GetResource(coherenceConfig);
            }

            Configure(cacheResource, coherenceResource);
        }

        /// <summary>
        /// Configure the CacheFactory.
        /// </summary>
        /// <param name="cacheConfig">
        /// An optional location of a resource that conforms to
        /// <c>cache-config.xsd</c>.
        /// </param>
        /// <param name="coherenceConfig">
        /// An optional location of a resource that conforms to
        /// <c>coherence.xsd</c>.
        /// </param>
        /// <since>Coherence 3.7</since>
        public static void Configure(IResource cacheConfig, IResource coherenceConfig)
        {
            IXmlElement xmlCache     = null;
            IXmlElement xmlCoherence = null;

            // load all specified configuration files
            if (cacheConfig != null)
            {
                xmlCache = XmlHelper.LoadResource(cacheConfig,
                    "cache configuration");
            }
            if (coherenceConfig != null)
            {
                xmlCoherence = XmlHelper.LoadResource(coherenceConfig,
                    "operational configuration");
            }

            Configure(xmlCache, xmlCoherence);
        }

        /// <summary>
        /// Configure the CacheFactory.
        /// </summary>
        /// <param name="xmlCache">
        /// An optional <see cref="IXmlElement"/> that conforms to
        /// <c>cache-config.xsd</c>.
        /// </param>
        /// <param name="xmlCoherence">
        /// An optional <see cref="IXmlElement"/> that conforms to
        /// <c>coherence.xsd</c>.
        /// </param>
        /// <since>Coherence 3.7</since>
        public static void Configure(IXmlElement xmlCache, IXmlElement xmlCoherence)
        {
            // create an IOperationalContext
            var ctx = new DefaultOperationalContext(xmlCoherence);

            // create a IConfigurableCacheFactory
            var xmlFactory = ctx.Config.GetSafeElement("configurable-cache-factory-config");
            var typeName   = xmlFactory.GetSafeElement("class-name").GetString(null);
            IConfigurableCacheFactory factory;
            if (typeName == null)
            {
                factory = new DefaultConfigurableCacheFactory(xmlCache);
            }
            else
            {
                var type  = TypeResolver.Resolve(typeName);
                var param = XmlHelper.ParseInitParams(
                    xmlFactory.GetSafeElement("init-params"));

                factory = (IConfigurableCacheFactory)
                    ObjectUtils.CreateInstance(type, param);
            }
            if (factory is DefaultConfigurableCacheFactory)
            {
                ((DefaultConfigurableCacheFactory)factory).OperationalContext = ctx;
            }

            Configure(factory, ctx);
        }

        /// <summary>
        /// Configure the CacheFactory.
        /// </summary>
        /// <param name="factory">
        /// The rquired singleton <see cref="IConfigurableCacheFactory"/>.
        /// </param>
        /// <param name="ctx">
        /// An optional <see cref="IOperationalContext"/> that contains
        /// operational configuration information.
        /// </param>
        /// <since>Coherence 3.7</since>
        public static void Configure(IConfigurableCacheFactory factory,
            IOperationalContext ctx)
        {
            // validate input parameters
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            using (BlockingLock l = BlockingLock.Lock(typeof(CacheFactory)))
            {
                IConfigurableCacheFactory factoryOld = s_factory;
                if (factoryOld != null)
                {
                    // shutdown the old factory
                    factoryOld.Shutdown();
                }

                Logger logger = s_logger;
                if (ctx != null && logger != null)
                {
                    // switch to a new logger
                    logger.Shutdown();
                    logger = null;
                }

                if (logger == null)
                {
                    // create, configure, and start a new logger
                    if (ctx == null)
                    {
                        ctx = new DefaultOperationalContext();
                    }
                    logger = new Logger();
                    logger.Configure(ctx);
                    logger.Start();

                    // output the product startup banner
                    logger.Log((int)LogLevel.Always,
                        string.Format("\n{0} Version {1}\n {2} Build\n{3}\n",
                           logger.Product,
                           logger.Version + (logger.BuildInfo == "0" ? "" : " Build " + logger.BuildInfo),
                           logger.Edition + " " + logger.BuildType,
                           logger.Copyright), null);

                    IList initLogMessages = s_initLogMessages;
                    using (BlockingLock l2 = BlockingLock.Lock(initLogMessages))
                    {
                        foreach (object[] logMessage in initLogMessages)
                        {
                            var message  = (string)    logMessage[0];
                            var exc      = (Exception) logMessage[1];
                            var severity = (LogLevel)  logMessage[2];
                            logger.Log((int) severity, exc, message, null);
                        }
                        initLogMessages.Clear();
                    }
                }

                if (factory.Config is IXmlDocument)
                {
                    IXmlDocument doc = (IXmlDocument) factory.Config;
                    doc.IterateThroughAllNodes(PreprocessProp);
                    factory.Config = doc;
                }
                else
                {
                    XmlDocument tempDoc = new XmlDocument();
                    tempDoc.LoadXml(factory.Config.GetString());
                    IXmlDocument doc = XmlHelper.ConvertDocument(tempDoc);
                    doc.IterateThroughAllNodes(PreprocessProp);
                    factory.Config = doc;
                }

                // update all singletons
                s_factory = factory;
                s_logger  = logger;
            }
        }

        /// <summary>
        /// Shutdown all services.
        /// </summary>
        /// <since>Coherence 1.0</since>
        public static void Shutdown()
        {
            using (BlockingLock l = BlockingLock.Lock(typeof(CacheFactory)))
            {
                var factory = s_factory;
                if (factory != null)
                {
                    // shutdown the old factory
                    s_factory = null;
                    factory.Shutdown();
                }

                var logger = s_logger;
                if (logger != null)
                {
                    // shutdown the old logger
                    s_logger = null;
                    logger.Shutdown();
                }
            }
        }

        /// <summary>
        /// Preprocess the Coherence properties specified either in the
        /// application configuration or environment variables.
        /// When both are specified, environment varialbe takes the precedence.
        /// </summary>
        /// <param name="xmlElement">The XML element to process.</param>
        /// <since>coherence 12.2.1.0.1</since>
        public static void PreprocessProp(IXmlElement xmlElement)
        {
            IXmlValue attr = xmlElement.GetAttribute("system-property");
            if (attr != null)
            {
                string property = attr.GetString();
                string val      = ConfigurationUtils.GetProperty(property, null);
                if (val != null)
                {
                    if (xmlElement.Value is Int32)
                    {
                        xmlElement.SetInt(Int32.Parse(val));
                    }
                    else
                    {
                        xmlElement.SetString(val);
                    }
                }
                xmlElement.Attributes.Remove("system-property");
            }

            string value          = xmlElement.Value.ToString();
            string newValue       = null;
            int    next           = value.IndexOf("${");
            int    i              = next + 2;
            bool   processedParam = false;
            while (next >= 0)
            {
                processedParam = true;
                string   curParam = value.Substring(i, value.IndexOf('}', i) - i);
                string[] entry    = curParam.Split(' ');
                string   property = entry[0];

                string   val = ConfigurationUtils.GetProperty(property, null);
                if (val == null)
                {
                    newValue += entry[1];
                }
                else
                {
                    newValue += val;
                }

                next = value.IndexOf("${", i);
                int start = value.IndexOf('}', i) + 1;
                if (next > 0)
                {
                    newValue += value.Substring(start, next - start);
                    i = next + 2;
                }
                else
                {
                    i = start;
                }
            }
            if (processedParam)
            {
                if (i < value.Length)
                {
                    newValue += value.Substring(i);                    
                }
                xmlElement.SetString(newValue);                
            }
        }

        #endregion

        #region Cache related methods

        /// <summary>
        /// Return an instance of a cache configured by the current
        /// <see cref="ConfigurableCacheFactory"/>.
        /// </summary>
        /// <remarks>
        /// This helper method is a simple wrapper around
        /// <see cref="IConfigurableCacheFactory.EnsureCache"/> method.
        /// </remarks>
        /// <param name="name">
        /// Cache name (unique for a given configurable cache factory). If
        /// the <see cref="INamedCache"/> with the specified name already
        /// exists, a reference to the same object will be returned.
        /// </param>
        /// <returns>
        /// The <b>INamedCache</b> object.
        /// </returns>
        /// <since>Coherence 2.2</since>
        public static INamedCache GetCache(string name)
        {
            return ConfigurableCacheFactory.EnsureCache(name);
        }

        /// <summary>
        /// Release local resources associated with the specified instance of
        /// the cache.
        /// </summary>
        /// <remarks>
        /// This invalidates a reference obtained by using one of the factory
        /// methods (<pre>GetReplicatedCache, GetOptimisticCache</pre>).
        /// <p>
        /// Releasing an <see cref="INamedCache"/> reference makes it no
        /// longer usable, but does not affect the content of the cache. In
        /// other words, all other references to the cache will still be
        /// valid, and the cache data is not affected by releasing the
        /// reference.</p>
        /// <p>
        /// The reference that is released using this method can no longer be
        /// used; any attempt to use the reference will result in an
        /// exception.</p>
        /// </remarks>
        /// <param name="cache">
        /// The <b>INamedCache</b> object to be released.
        /// </param>
        /// <since>Coherence 1.1</since>
        /// <seealso cref="RemoteCacheService.ReleaseCache(INamedCache)"/>
        /// <seealso cref="DestroyCache"/>
        public static void ReleaseCache(INamedCache cache)
        {
            ConfigurableCacheFactory.ReleaseCache(cache);
        }

        /// <summary>
        /// Releases and destroys the specified <see cref="INamedCache"/>.
        /// </summary>
        /// <remarks>
        /// <b>Warning:</b> This method is used to completely destroy the
        /// specified cache across the cluster. All references in the entire
        /// cluster to this cache will be invalidated, the cached data will
        /// be cleared, and all resources will be released.
        /// </remarks>
        /// <param name="cache">
        /// The <b>INamedCache</b> object to be destroyed.
        /// </param>
        /// <since>Coherence 1.1</since>
        /// <seealso cref="RemoteCacheService.DestroyCache(INamedCache)"/>
        /// <seealso cref="ReleaseCache"/>
        public static void DestroyCache(INamedCache cache)
        {
            ConfigurableCacheFactory.DestroyCache(cache);
        }

        #endregion

        #region Service related methods

        /// <summary>
        /// Return an instance of a service configured by the current
        /// <see cref="ConfigurableCacheFactory"/>.
        /// </summary>
        /// <remarks>
        /// This helper method is a simple wrapper around the
        /// <see cref="DefaultConfigurableCacheFactory.EnsureService(string)"/>
        /// method.
        /// </remarks>
        /// <param name="name">
        /// Service name (unique for a given configurable cache factory).
        /// If the <b>IService</b> with the specified name already exists, a
        /// reference to the same object will be returned.
        /// </param>
        /// <returns>
        /// The <b>IService</b> object.
        /// </returns>
        /// <since>Coherence 3.3</since>
        public static IService GetService(string name)
        {
            return ConfigurableCacheFactory.EnsureService(name);
        }

        #endregion

        #region Log related methods

        /// <summary>
        /// Return <b>true</b> if the Logger would log a message with the
        /// given log level.
        /// </summary>
        /// <param name="level">
        /// <see cref="LogLevel"/> value.
        /// </param>
        /// <returns>
        /// Whether the Logger would log a message with the given log level.
        /// </returns>
        public static bool IsLogEnabled(LogLevel level)
        {
            Logger logger = s_logger;
            return logger == null ? false : logger.IsEnabled((int) level);
        }

        /// <summary>
        /// Log a message using Coherence logging facility which is driven by
        /// the "logging-config" element located in the coherence.xml
        /// configuration file.
        /// </summary>
        /// <param name="message">
        /// A message to log.
        /// </param>
        /// <param name="severity">
        /// The severity of the logged message.
        /// </param>
        /// <since>Coherence 2.0</since>
        public static void Log(string message, LogLevel severity)
        {
            LogInternal(message, null, severity);
        }

        /// <summary>
        /// Log an exception using Coherence logging facility which is driven
        /// by the "logging-config" element located in the coherence.xml
        /// configuration file.
        /// </summary>
        /// <param name="exc">
        /// An exception to log.
        /// </param>
        /// <param name="severity">
        /// The severity of the logged message.
        /// </param>
        /// <since>Coherence 2.0</since>
        public static void Log(Exception exc, LogLevel severity)
        {
            LogInternal(null, exc, severity);
        }

        /// <summary>
        /// Log a message and exception using Coherence logging facility
        /// which is driven by the "logging-config" element located in the
        /// coherence.xml configuration file.
        /// </summary>
        /// <param name="message">
        /// A message to log.
        /// </param>
        /// <param name="exc">
        /// An exception to log.
        /// </param>
        /// <param name="severity">
        /// The severity of the logged message.
        /// </param>
        /// <since>Coherence 2.0</since>
        public static void Log(string message, Exception exc, LogLevel severity)
        {
            LogInternal(message, exc, severity);
        }

        /// <summary>
        /// Log a message and exception using the current Logger. If the
        /// Logger is null, defer the log mesage by placing it on the defered
        /// log message list.
        /// </summary>
        /// <param name="message">
        /// A message to log.
        /// </param>
        /// <param name="exc">
        /// An exception to log.
        /// </param>
        /// <param name="severity">
        /// The severity of the logged message.
        /// </param>
        /// <since>Coherence 3.7</since>
        private static void LogInternal(string message, Exception exc, LogLevel severity)
        {
            Logger logger = s_logger;
            if (logger == null)
            {
                IList list = s_initLogMessages;
                using (BlockingLock l = BlockingLock.Lock(list))
                {
                    logger = s_logger;
                    if (logger == null)
                    {
                        // defer log message
                        list.Add(new object[] {message, exc, severity});
                        return;
                    }
                }
            }
            logger.Log((int) severity, exc, message, null);
        }

        #endregion

        #region Data members

        /// <summary>
        /// IConfigurableCacheFactory singleton.
        /// </summary>
        private static IConfigurableCacheFactory s_factory;

        /// <summary>
        /// Logger singleton.
        /// </summary>
        private static Logger s_logger;

        /// <summary>
        /// Log messages that should be queued up before logger is initialized.
        /// </summary>
        private static readonly IList s_initLogMessages = new ArrayList();

        #endregion

        #region Enum: LogLevel

        /// <summary>
        /// The logging level a message must meet or exceed in order to be
        /// logged.
        /// </summary>
        public enum LogLevel
        {
            /// <summary>
            /// It is expected that items with a log level of 0 will always
            /// be logged.
            /// </summary>
            Always = 0,

            /// <summary>
            /// Log level 1 indicates an error.
            /// </summary>
            Error = 1,

            /// <summary>
            /// Log level 2 indicates a warning.
            /// </summary>
            Warn = 2,

            /// <summary>
            /// Log level 3 indicates information that should likely be
            /// logged.
            /// </summary>
            Info = 3,

            /// <summary>
            /// The default logging level is 5, so using the level of 5 will
            /// show up in the logs by default as a debug message.
            /// </summary>
            Debug = 5,

            /// <summary>
            /// The default logging level is 5, so using a level higher than 5
            /// will be "quiet" by default, meaning that it will not show up in
            /// the logs unless the configured logging level is increased.
            /// </summary>
            Quiet = 6,

            /// <summary>
            /// The maximum logging level indicator.
            /// </summary>
            Max = 9
        }
        #endregion
    }
}