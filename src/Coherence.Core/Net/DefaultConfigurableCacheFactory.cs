/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;

using Tangosol.Config;
using Tangosol.IO.Resources;
using Tangosol.Net.Cache;
using Tangosol.Net.Impl;
using Tangosol.Net.Internal;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Collections;
using Tangosol.Util.Filter;

namespace Tangosol.Net
{
    /// <summary>
    /// The <b>DefaultConfigurableCacheFactory</b> provides a facility to
    /// access caches declared in a "cache-config.xsd" compliant configuration
    /// file.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class is designed to be easily extendable with a collection of
    /// factory methods allowing subclasses to customize it by overriding any
    /// subset of cache instantiation routines or even allowing the addition of
    /// custom schemes.</p>
    /// <p>
    /// There are various ways of using this factory:</p>
    /// <pre>
    /// IConfigurableCacheFactory factory =
    ///     new DefaultConfigurableCacheFactory(path);
    /// INamedCache cacheOne = factory.EnsureCache("one");
    /// INamedCache cacheTwo = factory.EnsureCache("two");
    /// </pre>
    /// <p>
    /// Using this approach allows an easy customization by extending the
    /// DefaultConfigurableCacheFactory and changing the instantiation line:
    /// </p>
    /// <pre>
    /// IConfigurableCacheFactory factory =
    ///     new CustomConfigurableCacheFactory();
    /// ...
    /// </pre>
    /// <p>
    /// Another option is using the static version of the "EnsureCache" call:
    /// </p>
    /// <pre>
    /// INamedCache cacheOne = CacheFactory.GetCache("one");
    /// INamedCache cacheTwo = CacheFactory.GetCache("two");
    /// </pre>
    /// which uses an instance of <see cref="IConfigurableCacheFactory"/>
    /// obtained by the <see cref="CacheFactory.ConfigurableCacheFactory"/>.
    /// </remarks>
    /// <author>Gene Gleyzer  2003.05.26</author>
    /// <author>Ana Cikic  2006.09.22</author>
    /// <since>Coherence 3.2</since>
    /// <seealso cref="CacheFactory.GetCache"/>
    public class DefaultConfigurableCacheFactory : IConfigurableCacheFactory
    {
        #region Properties

        /// <summary>
        /// The <see cref="IResource"/> for the default XML configuration used
        /// when one isn't explicitly passed in the constructor for this class.
        /// </summary>
        /// <value>
        /// The <see cref="IResource"/> for the default XML configuration.
        /// </value>
        public static IResource DefaultCacheConfigResource
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
        public static IXmlDocument DefaultCacheConfig
        {
            get;
            set;
        }

        /// <summary>
        /// The current configuration of the object.
        /// </summary>
        /// <value>
        /// The XML configuration or <c>null</c>.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// When setting, if the object is not in a state that allows the
        /// configuration to be set; for example, if the object has already
        /// been configured and cannot be reconfigured.
        /// </exception>
        public virtual IXmlElement Config
        {
            get { return (IXmlElement) m_xmlConfig.Clone(); }
            set
            {
                ValidateConfig(value);

                lock (this)
                {
                    value       = (IXmlElement) value.Clone();
                    m_xmlConfig = value;

                    if (m_storeCache != null)
                    {
                        m_storeCache.Clear();
                    }
                    if (m_storeService != null)
                    {
                        m_storeService.Clear();    
                    }
                }
            }
        }

        /// <summary>
        /// The <see cref="IOperationalContext"/> for this
        /// DefaultConfigurableCacheFactory.
        /// </summary>
        /// <value>
        /// An <b>IOperationalContext</b> instance.
        /// </value>
        public virtual IOperationalContext OperationalContext
        {
            get
            {
                var ctx = m_operationalContext;
                if (ctx == null)
                {
                    m_operationalContext = ctx = new DefaultOperationalContext();
                }
                return ctx;
            }
            set
            {
                if (m_operationalContext != null)
                {
                    throw new InvalidOperationException(
                        "operational context already set");
                }
                m_operationalContext = value;
            }
        }

        /// <summary>
        /// Store that holds cache references by name and optionally,
        /// if configured, IPrincipal.
        /// </summary>
        protected virtual ScopedReferenceStore StoreCache
        {
            get
            {
                var store = m_storeCache;
                if (store == null)
                {
                    m_storeCache = store = new ScopedReferenceStore(
                            OperationalContext);
                }
                return store;
            }
            set
            {
                m_storeCache = value;
            }
        }

        /// <summary>
        /// Store that holds cache references by name and optionally,
        /// if configured, IPrincipal.
        /// </summary>
        protected virtual ScopedReferenceStore StoreService
        {
            get
            {
                var store = m_storeService;
                if (store == null)
                {
                    m_storeService = store = new ScopedReferenceStore(
                            OperationalContext);
                }
                return store;
            }
            set
            {
                m_storeService = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a default DefaultConfigurableCacheFactory using the
        /// default configuration file name.
        /// </summary>
        public DefaultConfigurableCacheFactory()
            : this((IXmlElement) null)
        {}

        /// <summary>
        /// Construct a DefaultConfigurableCacheFactory using the specified
        /// path to a "cache-config.xsd" compliant configuration file.
        /// </summary>
        /// <param name="path">
        /// The configuration file path.
        /// </param>
        public DefaultConfigurableCacheFactory(string path)
            : this(XmlHelper.LoadResource(ResourceLoader.GetResource(path),
                    "cache configuration"))
        {}

        /// <summary>
        /// Construct a DefaultConfigurableCacheFactory using the specified
        /// configuration XML.
        /// </summary>
        /// <param name="xmlConfig">
        /// The configuration <see cref="IXmlElement"/>.
        /// </param>
        public DefaultConfigurableCacheFactory(IXmlElement xmlConfig)
        {
            if (xmlConfig == null)
            {
                xmlConfig = LoadDefaultCacheConfig();
            }

            IXmlDocument doc;
            if (xmlConfig is IXmlDocument)
            {
                doc = (IXmlDocument) xmlConfig;
                doc.IterateThroughAllNodes(CacheFactory.PreprocessProp);
            }
            else
            {
                XmlDocument tempDoc = new XmlDocument();
                tempDoc.LoadXml(xmlConfig.GetString());
                doc = XmlHelper.ConvertDocument(tempDoc);
                doc.IterateThroughAllNodes(CacheFactory.PreprocessProp);
            }
            Config = doc;
        }

        #endregion

        #region IConfigurableCacheFactory implementation

        /// <summary>
        /// Ensure a cache for the given name using the corresponding XML
        /// configuration.
        /// </summary>
        /// <param name="cacheName">
        /// The cache name.
        /// </param>
        /// <returns>
        /// An <see cref="INamedCache"/> created according to the
        /// configuration XML.
        /// </returns>
        public virtual INamedCache EnsureCache(string cacheName)
        {
            if (StringUtils.IsNullOrEmpty(cacheName))
            {
                throw new InvalidOperationException("Cache name cannot be null");
            }

            ScopedReferenceStore storeCache = StoreCache;
            INamedCache          cache      = storeCache.GetCache(cacheName);

            if (cache != null && cache.IsActive)
            {
                return cache;
            }

            lock(storeCache)
            {
                // since we first checked, someone could create it
                cache = storeCache.GetCache(cacheName);
                if (cache != null && cache.IsActive)
                {
                    return cache;
                }

                CacheInfo   infoCache = FindSchemeMapping(cacheName);
                IXmlElement xmlScheme = ResolveScheme(infoCache);
                xmlScheme.AddAttribute("tier").SetString("front"); // mark the "entry point"

                cache = ConfigureCache(infoCache, xmlScheme);

                storeCache.PutCache(cache);

                return cache;
            }
        }

        /// <summary>
        /// Release local resources associated with the specified cache
        /// instance.
        /// </summary>
        /// <remarks>
        /// This invalidates a reference obtained from the factory.
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
        /// <since>Coherence 3.5.1</since>
        /// <seealso cref="IConfigurableCacheFactory.DestroyCache"/>
        public virtual void ReleaseCache(INamedCache cache)
        {
            ReleaseCache(cache, /*destroy*/ false);
        }

        /// <summary>
        /// Release and destroy the specified cache.
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
        /// <since>Coherence 3.5.1</since>
        /// <seealso cref="IConfigurableCacheFactory.ReleaseCache"/>
        public virtual void DestroyCache(INamedCache cache)
        {
            ReleaseCache(cache, /*destroy*/ true);
        }

        /// <summary>
        /// Ensure a service for the given name using the corresponding XML
        /// configuration.
        /// </summary>
        /// <param name="serviceName">
        /// The service name.
        /// </param>
        /// <returns>
        /// An <see cref="IService"/> created according to the configuration
        /// XML.
        /// </returns>
        public virtual IService EnsureService(string serviceName)
        {
            return EnsureService(FindServiceScheme(serviceName));
        }

        /// <summary>
        /// Release all resources allocated by this cache factory.
        /// </summary>
        public virtual void Shutdown()
        {
            lock (this)
            {
                ScopedReferenceStore storeCache = StoreCache;
                // release all caches
                foreach (INamedCache cache in storeCache.GetAllCaches())
                {
                    cache.Release();
                }
                storeCache.Clear();
                ScopedReferenceStore storeService = StoreService;
                // shutdown all services
                foreach (IService service in storeService.GetAllServices())
                {
                    service.Shutdown();
                }
                storeService.Clear();
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Load and return the default XML cache configuration.
        /// </summary>
        /// <returns>
        /// The default XML cache configuration.
        /// </returns>
        protected static IXmlDocument LoadDefaultCacheConfig()
        {
            var config = DefaultCacheConfig;
            if (config == null)
            {
                var coherence = (CoherenceConfig)
                        ConfigurationUtils.GetCoherenceConfiguration();
                var resource = coherence == null ? null : coherence.CacheConfig;
                if (resource == null)
                {
                    resource = DefaultCacheConfigResource;
                    try
                    {
                        // test that the DefaultCacheConfigResource exists...
                        using (var stream = resource.GetStream()) { }
                    }
                    catch (IOException)
                    {
                        // the default cache config resource was not found. Use a subset of the default java cache config,
                        // i.e. extend applicable part
                        resource = ResourceLoader.GetResource(
                            "assembly://Coherence.Core/Tangosol.Config/coherence-cache-config.xml");
                    }
                }
                config = XmlHelper.LoadResource(resource,
                        "cache configuration");
            }
            return config;
        }

        /// <summary>
        /// In the configuration XML find a "cache-mapping" element
        /// associated with a given cache name.
        /// </summary>
        /// <param name="cacheName">
        /// The value of the "cache-name" element to look for.
        /// </param>
        /// <returns>
        /// A <see cref="CacheInfo"/> object associated with a given cache
        /// name.
        /// </returns>
        public virtual CacheInfo FindSchemeMapping(string cacheName)
        {
            IXmlElement xmlDefaultMatch = null;
            IXmlElement xmlPrefixMatch  = null;
            IXmlElement xmlExactMatch   = null;
            string      suffix          = null;

            for (IEnumerator enumerator = m_xmlConfig.GetSafeElement("caching-scheme-mapping").
                GetElements("cache-mapping"); enumerator.MoveNext(); )
            {
                IXmlElement xmlMapping = (IXmlElement) enumerator.Current;

                string name = xmlMapping.GetSafeElement("cache-name").GetString().Trim();
                if (name.Equals(cacheName))
                {
                    xmlExactMatch = xmlMapping;
                    break;
                }
                else if (name.Equals("*"))
                {
                    xmlDefaultMatch = xmlMapping;
                }
                else
                {
                    int cchPrefix = name.IndexOf('*');
                    if (cchPrefix >= 0)
                    {
                        string prefix = name.Substring(0, cchPrefix);
                        if (cacheName.StartsWith(prefix))
                        {
                            if (cchPrefix != name.Length - 1)
                            {
                                throw new ArgumentException("Invalid wildcard pattern:\n" + xmlMapping);
                            }
                            xmlPrefixMatch = xmlMapping;
                            suffix         = cacheName.Substring(cchPrefix);
                        }
                    }
                }
            }

            IXmlElement xmlMatch;
            if (xmlExactMatch != null)
            {
                xmlMatch = xmlExactMatch;
                suffix   = "";
            }
            else if (xmlPrefixMatch != null)
            {
                xmlMatch = xmlPrefixMatch;
            }
            else
            {
                xmlMatch = xmlDefaultMatch;
                suffix   = cacheName;
            }

            if (xmlMatch == null)
            {
                throw new ArgumentException("No scheme for cache: \"" + cacheName + '"');
            }

            string      scheme  = xmlMatch.GetSafeElement("scheme-name").GetString();
            IDictionary mapAttr = new HashDictionary();
            for (IEnumerator enumerator = xmlMatch.GetSafeElement("init-params").
                    GetElements("init-param"); enumerator.MoveNext(); )
            {
                IXmlElement xmlParam = (IXmlElement) enumerator.Current;
                string      name     = xmlParam.GetSafeElement("param-name").GetString();
                string      value    = xmlParam.GetSafeElement("param-value").GetString();

                if (name.Length > 0)
                {
                    int ofReplace = value.IndexOf('*');
                    if (ofReplace >= 0 && suffix != null)
                    {
                        value = value.Substring(0, ofReplace) + suffix +
                                 value.Substring(ofReplace + 1);
                    }
                    mapAttr.Add(name, value);
                }
            }

            return new CacheInfo(cacheName, scheme, mapAttr);
        }

        /// <summary>
        /// In the configuration XML find a "scheme" element associated with
        /// a given cache and resolve it (recursively) using the "scheme-ref"
        /// elements.
        /// </summary>
        /// <remarks>
        /// The returned XML is always a clone of the actual configuration
        /// and could be safely modified.
        /// </remarks>
        /// <param name="info">
        /// The cache info.
        /// </param>
        /// <returns>
        /// A resolved "scheme" element associated with a given cache.
        /// </returns>
        public virtual IXmlElement ResolveScheme(CacheInfo info)
        {
            IXmlElement xmlScheme = FindScheme(info.SchemeName);
            info.ReplaceAttributes(xmlScheme);

            return ResolveScheme(xmlScheme, info, false, true);
        }

        /// <summary>
        /// In the configuration XML find a "scheme" element associated with
        /// a given cache name.
        /// </summary>
        /// <param name="schemeName">
        /// The value of the "scheme-name" element to look for.
        /// </param>
        /// <returns>
        /// A "scheme" element associated with a given cache name.
        /// </returns>
        protected virtual IXmlElement FindScheme(string schemeName)
        {
            IXmlElement xmlScheme = FindScheme(m_xmlConfig, schemeName);
            if (xmlScheme != null)
            {
                return (IXmlElement) xmlScheme.Clone();
            }

            throw new ArgumentException("Missing scheme: \"" + schemeName + '"');
        }

        /// <summary>
        /// In the specified configuration XML, find a "scheme" element
        /// associated with the specified scheme name.
        /// </summary>
        /// <param name="xmlConfig">
        /// The xml configuration.
        /// </param>
        /// <param name="schemeName">
        /// The value of the "scheme-name" element to look for.
        /// </param>
        /// <returns>
        /// A "scheme" element associated with a given scheme name, or
        /// <c>null</c> if none is found.
        /// </returns>
        protected static IXmlElement FindScheme(IXmlElement xmlConfig, string schemeName)
        {
            if (schemeName != null)
            {
                foreach (IXmlElement xml in xmlConfig.GetSafeElement("caching-schemes").ElementList)
                {
                    if (xml.GetSafeElement("scheme-name").GetString().Equals(schemeName))
                    {
                        return xml;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// In the configuration XML find a "scheme" element associated with
        /// a given service name.
        /// </summary>
        /// <param name="serviceName">
        /// The value of the "service-name" element to look for.
        /// </param>
        /// <returns>
        /// A "scheme" element associated with a given service name.
        /// </returns>
        protected virtual IXmlElement FindServiceScheme(string serviceName)
        {
            if (serviceName != null)
            {
                foreach (IXmlElement xml in m_xmlConfig.GetSafeElement("caching-schemes").ElementList)
                {
                    if (xml.GetSafeElement("service-name").GetString().Equals(serviceName))
                    {
                        return (IXmlElement) xml.Clone();
                    }
                }
            }

            throw new ArgumentException("Missing scheme for service: \"" + serviceName + '"');
        }

        /// <summary>
        /// Resolve the specified "XYZ-scheme" by retrieving the base element
        /// refered to by the "scheme-ref" element, resolving it recursively,
        /// and combining it with the specified overrides and cache specific
        /// attributes.
        /// </summary>
        /// <param name="xmlConfig">
        /// The cache configuration xml.
        /// </param>
        /// <param name="xmlScheme">
        /// A scheme element to resolve.
        /// </param>
        /// <param name="info">
        /// The cache info (optional).
        /// </param>
        /// <param name="isChild">
        /// If <b>true</b>, the actual cache scheme is the only "xyz-scheme"
        /// child of the specified xmlScheme element; otherwise it's the
        /// xmlScheme element itself.</param>
        /// <param name="isRequired">
        /// If <b>true</b>, the child scheme must be present; <b>false</b>
        /// otherwise.
        /// </param>
        /// <param name="apply">
        /// If <b>true</b>, apply the specified overrides and cache-specific
        /// attributes to the base scheme element; otherwise return a
        /// reference to the base scheme element.
        /// </param>
        /// <returns>
        /// A "scheme" element associated with a given cache name;
        /// <c>null</c> if the child is missing and is not required.
        /// </returns>
        protected static IXmlElement ResolveScheme(IXmlElement xmlConfig, IXmlElement xmlScheme,
            CacheInfo info, bool isChild, bool isRequired, bool apply)
        {
            if (isChild)
            {
                IXmlElement xmlChild = null;
                foreach (IXmlElement xml in xmlScheme.ElementList)
                {
                    if (xml.Name.EndsWith("-scheme"))
                    {
                        if (xmlChild == null)
                        {
                            xmlChild = xml;
                        }
                        else
                        {
                            throw new ArgumentException(
                                "Scheme contains more then one child scheme:\n" + xmlScheme);
                        }
                    }
                }

                if (xmlChild == null)
                {
                    if (isRequired)
                    {
                        string name = xmlScheme.Name;
                        if (xmlScheme == xmlScheme.Parent.GetElement(name))
                        {
                            throw new ArgumentException("Child scheme is missing at:\n" + xmlScheme);
                        }
                        else
                        {
                            throw new ArgumentException("Element \"" + name + "\" is missing at:\n" + xmlScheme.Parent);
                        }
                    }
                    return null;
                }
                xmlScheme = xmlChild;
            }

            string refName = xmlScheme.GetSafeElement("scheme-ref").GetString();
            if (refName.Length == 0)
            {
                return xmlScheme;
            }

            IXmlElement xmlBase = FindScheme(xmlConfig, refName);
            xmlBase = apply ? (IXmlElement) xmlBase.Clone() : xmlBase;

            if (!xmlScheme.Name.Equals(xmlBase.Name))
            {
                throw new ArgumentException("Reference does not match the scheme type: scheme=\n" +
                    xmlScheme + "\nbase=" + xmlBase);
            }
            if (xmlScheme.Equals(xmlBase))
            {
                throw new ArgumentException("Circular reference in scheme:\n" + xmlScheme);
            }

            if (info != null)
            {
                info.ReplaceAttributes(xmlBase);
            }

            IXmlElement xmlResolve = ResolveScheme(xmlConfig, xmlBase, info, false, false, apply);

            if (apply)
            {
                foreach (IXmlElement xml in xmlScheme.ElementList)
                {
                    XmlHelper.ReplaceElement(xmlResolve, xml);
                }
            }
            return xmlResolve;
        }

        /// <summary>
        /// Resolve the specified "XYZ-scheme" by retrieving the base element
        /// refered to by the "scheme-ref" element, resolving it recursively,
        /// and combining it with the specified overrides and cache specific
        /// attributes.
        /// </summary>
        /// <param name="xmlScheme">
        /// A scheme element to resolve.
        /// </param>
        /// <param name="info">
        /// The cache info (optional).
        /// </param>
        /// <param name="isChild">
        /// If <b>true</b>, the actual cache scheme is the only "xyz-scheme"
        /// child of the specified xmlScheme element; otherwise it's the
        /// xmlScheme element itself.
        /// </param>
        /// <param name="isRequired">
        /// If <b>true</b>, the child scheme must be present; <b>false</b>
        /// otherwise.
        /// </param>
        /// <returns>
        /// A "scheme" element associated with a given cache name;
        /// <c>null</c> if the child is missing and is not required.
        /// </returns>
        protected virtual IXmlElement ResolveScheme(IXmlElement xmlScheme, CacheInfo info, bool isChild, bool isRequired)
        {
            return ResolveScheme(m_xmlConfig, xmlScheme, info, isChild, isRequired, true);
        }

        /// <summary>
        /// Obtain the <see cref="INamedCache"/> reference for the cache
        /// service defined by the specified scheme.
        /// </summary>
        /// <param name="info">
        /// The cache info.
        /// </param>
        /// <param name="xmlScheme">
        /// The scheme element for the cache.
        /// </param>
        /// <returns>
        /// <see cref="INamedCache"/> instance.
        /// </returns>
        protected virtual INamedCache EnsureCache(CacheInfo info, IXmlElement xmlScheme)
        {
            try
            {
                ICacheService service = (ICacheService) EnsureService(xmlScheme);
                return service.EnsureCache(info.CacheName);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Invalid scheme:\n" + xmlScheme);
            }
        }

        /// <summary>
        /// Ensure the service for the specified scheme.
        /// </summary>
        /// <param name="xmlScheme">
        /// The scheme.
        /// </param>
        /// <returns>
        /// Running <see cref="IService"/> corresponding to the scheme.
        /// </returns>
        public virtual IService EnsureService(IXmlElement xmlScheme)
        {
            xmlScheme = ResolveScheme(xmlScheme, null, false, false);

            string      schemeType  = xmlScheme.Name;
            string      serviceName = xmlScheme.GetSafeElement("service-name").GetString();
            ServiceType serviceType;
            SchemeType  nSchemeType = TranslateSchemeType(schemeType);

            switch (nSchemeType)
            {
                case SchemeType.Near:
                case SchemeType.View:    
                    return EnsureService(ResolveScheme(xmlScheme.GetSafeElement("back-scheme"), null, true, true));

                case SchemeType.RemoteInvocation:
                    serviceType = ServiceType.RemoteInvocation;
                    break;

                case SchemeType.RemoteCache:
                    serviceType = ServiceType.RemoteCache;
                    break;

                default:
                    throw new InvalidOperationException("EnsureService: " + schemeType);
            }

            if (serviceName.Length == 0)
            {
                serviceName = serviceType.ToString();
            }

            lock (typeof(CacheFactory))
            {
                IService service = EnsureService(serviceName, serviceType);

                if (!service.IsRunning)
                {
                    // merge the standard service config parameters
                    IXmlElement xmlConfig    = xmlScheme;
                    IList       listStandard = xmlConfig.ElementList;
                    for (int i = 0, c = listStandard.Count; i < c; i++)
                    {
                        IXmlElement xmlParamStandard = (IXmlElement) listStandard[i];
                        string      paramName        = xmlParamStandard.Name;
                        IXmlElement xmlParam         = xmlScheme.GetElement(paramName);

                        if (xmlParam != null && !XmlHelper.IsEmpty(xmlParam))
                        {
                            listStandard[i] = xmlParam.Clone();
                        }
                    }

                    // resolve nested serializers for remote services
                    switch (nSchemeType)
                    {
                        case SchemeType.RemoteCache:
                        case SchemeType.RemoteInvocation:
                            ResolveSerializer(xmlConfig.EnsureElement("initiator-config"));
                            break;
                    }

                    service.Configure(xmlConfig);
                    service.Start();
                }
                return service;
            }
        }

        /// <summary>
        /// Ensures a cache for given scheme.
        /// </summary>
        /// <param name="info">
        /// The cache info.
        /// </param>
        /// <param name="xmlScheme">
        /// The corresponding scheme.
        /// </param>
        /// <returns>
        /// A named cache created according to the description in the
        /// configuration.
        /// </returns>
        protected INamedCache ConfigureCache(CacheInfo info, IXmlElement xmlScheme)
        {
            string      schemeType = xmlScheme.Name;
            INamedCache cache;

            switch (TranslateSchemeType(schemeType))
            {
                case SchemeType.Local:
                    cache = InstantiateLocalNamedCache(info, xmlScheme);
                    break;

                case SchemeType.Class:
                    cache = InstantiateCache(info, xmlScheme) as INamedCache;
                    break;

                case SchemeType.RemoteCache:
                    cache = EnsureCache(info, xmlScheme);

                    IXmlElement xmlBundling = xmlScheme.GetElement("operation-bundling");
                    if (xmlBundling != null)
                    {
                        cache = InstantiateBundlingNamedCache(cache, xmlBundling);
                    }
                    break;

                case SchemeType.Near:
                    IXmlElement xmlFront   = ResolveScheme(xmlScheme.GetSafeElement("front-scheme"), info, true, true);
                    IXmlElement xmlBack    = ResolveScheme(xmlScheme.GetSafeElement("back-scheme"), info, true, true);
                    ICache      cacheFront = ConfigureBackingCache(info, xmlFront);
                    INamedCache cacheBack  = EnsureCache(info, xmlBack);
                    string      strategy   = xmlScheme.GetSafeElement("invalidation-strategy").GetString("auto");

                    CompositeCacheStrategyType strategyType = strategy.Equals("none")    ? CompositeCacheStrategyType.ListenNone
                                                            : strategy.Equals("present") ? CompositeCacheStrategyType.ListenPresent
                                                            : strategy.Equals("all")     ? CompositeCacheStrategyType.ListenAll
                                                            : strategy.Equals("logical") ? CompositeCacheStrategyType.ListenLogical
                                                            : CompositeCacheStrategyType.ListenAuto;

                    string subtype = xmlScheme.GetSafeElement("class-name").GetString();

                    if (subtype.Length == 0)
                    {
                        cache = InstantiateNearCache(cacheFront, cacheBack, strategyType);
                    }
                    else
                    {
                        object[] initParams = new object[] {cacheFront, cacheBack, strategyType};
                        cache = (INamedCache) InstantiateSubtype(
                            subtype, typeof(NearCache), initParams, xmlScheme.GetElement("init-params"));
                    }
                    break;
                
                case SchemeType.View:
                    IXmlElement     xmlViewFilter      = xmlScheme.GetElement("view-filter");
                    IXmlElement     xmlTransformer     = xmlScheme.GetElement("transformer");
                    IXmlElement     xmlListener        = xmlScheme.GetElement("listener");
                                    xmlBack            = ResolveScheme(xmlScheme.GetSafeElement("back-scheme"), info, true, true);
                    bool            fReadOnly          = xmlScheme.GetSafeElement("read-only").GetBoolean();
                    long            cReconnectInterval = XmlHelper.ParseTime(xmlScheme.GetSafeElement("reconnect-interval").GetString("0"), XmlHelper.UNIT_MS);
                    IFilter         filter             = AlwaysFilter.Instance;
                    IValueExtractor transformer        = null;
                    ICacheListener  listener           = null;
                    
                    if (xmlViewFilter != null)
                    {
                        filter = (IFilter) InstantiateAny(info, xmlViewFilter.GetSafeElement("class-scheme"));
                    }

                    if (xmlTransformer != null)
                    {
                        transformer = (IValueExtractor) InstantiateAny(info, xmlTransformer.GetSafeElement("class-scheme"));
                    }

                    if (xmlListener != null)
                    {
                        listener = InstantiateCacheListener(info, xmlListener.GetSafeElement("class-scheme"));
                    }
                    
                    ContinuousQueryCache queryCache = new ContinuousQueryCache(() => EnsureCache(info, xmlBack),
                                                                               filter,
                                                                               true,
                                                                               listener,
                                                                               transformer);
                    queryCache.CacheNameSupplier = () => queryCache.Cache.CacheName;
                    queryCache.ReconnectInterval = cReconnectInterval;
                    queryCache.IsReadOnly        = transformer != null || fReadOnly;
                    return queryCache;
                    
                    break;

                default:
                    throw new NotSupportedException("configureCache: " + schemeType);
            }

            VerifyCacheListener(info, cache, xmlScheme);

            return cache;
        }

        /// <summary>
        /// Check whether or not an <see cref="ICacheListener"/> has to be
        /// instantiated and added to an <b>ICache</b> according to a scheme
        /// definition.
        /// </summary>
        /// <param name="info">
        /// The cache info.
        /// </param>
        /// <param name="cache">
        /// The <b>ICache</b> to add a listener to.
        /// </param>
        /// <param name="xmlScheme">
        /// The corresponding scheme.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the listener is required, but the cache does not implement the
        /// <see cref="IObservableCache"/> interface or if the listener cannot
        /// be instantiated.
        /// </exception>
        protected virtual void VerifyCacheListener(CacheInfo info, ICache cache,
                IXmlElement xmlScheme)
        {
            IXmlElement xmlListener = xmlScheme.GetSafeElement("listener");
            IXmlElement xmlClass    = ResolveScheme(xmlListener, info, true, false);
            if (xmlClass != null)
            {
                ICacheListener listener = InstantiateCacheListener(info, xmlClass);
                try
                {
                    ((IObservableCache) cache).AddCacheListener(listener);
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("Cache is not observable: " + cache.GetType());
                }
            }
        }

        /// <summary>
        /// Construct an <see cref="NearCache"/> using the specified
        /// parameters.
        /// </summary>
        /// <remarks>
        /// This method exposes a corresponding <b>NearCache</b> constructor
        /// and is provided for the express purpose of allowing its override.
        /// </remarks>
        /// <param name="cacheFront">
        /// <b>ICache</b> to put in front of the back cache.
        /// </param>
        /// <param name="cacheBack">
        /// <b>INamedCache</b> to put behind the front cache.
        /// </param>
        /// <param name="strategy">
        /// Specifies the strategy used for the front cache
        /// invalidation; valid values are:
        /// <see cref="CompositeCacheStrategyType.ListenNone"/>
        /// <see cref="CompositeCacheStrategyType.ListenPresent"/>
        /// <see cref="CompositeCacheStrategyType.ListenAll"/>
        /// <see cref="CompositeCacheStrategyType.ListenAuto"/>
        /// </param>
        /// <returns>
        /// A newly instantiated <see cref="NearCache"/>.
        /// </returns>
        protected virtual NearCache InstantiateNearCache(ICache cacheFront, INamedCache cacheBack, CompositeCacheStrategyType strategy)
        {
            return new NearCache(cacheFront, cacheBack, strategy);
        }

        /// <summary>
        /// Configures a backing cache according to the scheme.
        /// </summary>
        /// <param name="info">
        /// The cache info.
        /// </param>
        /// <param name="xmlScheme">
        /// The scheme element for cache configuration.
        /// </param>
        /// <returns>
        /// A backing cache configured according to the scheme.
        /// </returns>
        public virtual ICache ConfigureBackingCache(CacheInfo info, IXmlElement xmlScheme)
        {
            string schemeType = xmlScheme.Name;
            ICache cache;

            switch (TranslateSchemeType(schemeType))
            {
                case SchemeType.Local:
                    cache = InstantiateLocalNamedCache(info, xmlScheme);
                    break;

                case SchemeType.RemoteCache:
                    cache = EnsureCache(info, xmlScheme);
                    break;

                case SchemeType.Class:
                    cache = InstantiateCache(info, xmlScheme);
                    break;

                default:
                    throw new InvalidOperationException("configureBackingCache: " + schemeType);
            }

            return cache;
        }

        /// <summary>
        /// Create a backing cache using the "local-scheme" element.
        /// </summary>
        /// <param name="info">
        /// The cache info.
        /// </param>
        /// <param name="xmlLocal">
        /// The "local-scheme" element.
        /// </param>
        /// <returns>
        /// A newly instantiated cache.
        /// </returns>
        protected virtual INamedCache InstantiateLocalNamedCache(CacheInfo info, IXmlElement xmlLocal)
        {
            int highUnits         = (int) XmlHelper.ParseMemorySize(xmlLocal.GetSafeElement("high-units").GetString("0"));
            int lowUnits          = (int) XmlHelper.ParseMemorySize(xmlLocal.GetSafeElement("low-units").GetString("0"));
            int expiryDelayMillis = (int) XmlHelper.ParseTime(xmlLocal.GetSafeElement("expiry-delay").GetString("0"), XmlHelper.UNIT_S);
            int flushDelayMillis  = (int) XmlHelper.ParseTime(xmlLocal.GetSafeElement("flush-delay").GetString("0"), XmlHelper.UNIT_S);

            // check and default all of the Cache options
            if (highUnits <= 0)
            {
                highUnits = LocalCache.DEFAULT_UNITS;
            }
            if (lowUnits <= 0)
            {
                lowUnits = (int) (highUnits * LocalCache.DEFAULT_PRUNE);
            }
            if (expiryDelayMillis < 0)
            {
                expiryDelayMillis = 0;
            }
            if (flushDelayMillis < 0)
            {
                flushDelayMillis = 0;
            }

            // As of Coherence 3.0, if the local cache has been configured with a
            // positive expiry-delay, but does not have a flush-delay,
            // the value of the flush-delay will be set to the default value
            if (expiryDelayMillis > 0 && flushDelayMillis == 0)
            {
                flushDelayMillis = LocalCache.DEFAULT_FLUSH;
            }

            // configure and return the LocalCache
            LocalNamedCache cache;
            string subtype = xmlLocal.GetSafeElement("class-name").GetString();
            if (subtype.Length == 0)
            {
                cache = InstantiateLocalNamedCache(highUnits, expiryDelayMillis);
            }
            else
            {
                object[] initParams = new object[] { highUnits, expiryDelayMillis };
                cache = (LocalNamedCache) InstantiateSubtype(
                    subtype, typeof(LocalNamedCache), initParams, xmlLocal.GetElement("init-params"));
            }
            cache.LocalCache.LowUnits   = lowUnits;
            cache.LocalCache.FlushDelay = flushDelayMillis;
            cache.CacheName             = info.CacheName;

            IXmlElement xmlEviction = xmlLocal.GetElement("eviction-policy");
            if (xmlEviction != null)
            {
                string eviction = xmlEviction.GetString();
                LocalCache.EvictionPolicyType evictionType;

                if (eviction.ToUpper().Equals("HYBRID"))
                {
                    evictionType = LocalCache.EvictionPolicyType.Hybrid;
                }
                else if (eviction.ToUpper().Equals("LRU"))
                {
                    evictionType = LocalCache.EvictionPolicyType.LRU;
                }
                else if (eviction.ToUpper().Equals("LFU"))
                {
                    evictionType = LocalCache.EvictionPolicyType.LFU;
                }
                else
                {
                    evictionType = LocalCache.EvictionPolicyType.Unknown;
                }

                if (evictionType != LocalCache.EvictionPolicyType.Unknown)
                {
                    cache.LocalCache.EvictionType = evictionType;
                }
                else
                {
                    IXmlElement xmlClass = xmlEviction.GetElement("class-scheme");
                    if (xmlClass != null)
                    {
                        try
                        {
                            cache.LocalCache.EvictionPolicy = (IEvictionPolicy) InstantiateAny(info, xmlClass);
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException("Unknown eviction policy:\n" + xmlClass);
                        }
                    }
                }
            }

            IXmlElement xmlCalculator = xmlLocal.GetElement("unit-calculator");
            if (xmlCalculator != null)
            {
                string calculator = xmlCalculator.GetString();
                LocalCache.UnitCalculatorType calculatorType;

                if (calculator.ToUpper().Equals("FIXED"))
                {
                    calculatorType = LocalCache.UnitCalculatorType.Fixed;
                }
                else
                {
                    calculatorType = LocalCache.UnitCalculatorType.Unknown;
                }

                if (calculatorType != LocalCache.UnitCalculatorType.Unknown)
                {
                    cache.LocalCache.CalculatorType = calculatorType;
                }
                else
                {
                    IXmlElement xmlClass = xmlCalculator.GetElement("class-scheme");
                    if (xmlClass != null)
                    {
                        try
                        {
                            cache.LocalCache.UnitCalculator = (IUnitCalculator) InstantiateAny(info, xmlClass);
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException("Unknown unit calculator:\n" + xmlClass);
                        }
                    }
                }
            }

            IXmlElement  xmlStore = ResolveScheme(xmlLocal.GetSafeElement("cachestore-scheme"), info, false, false);
            ICacheLoader store    = InstantiateCacheStore(info, xmlStore);
            if (store != null)
            {
                cache.LocalCache.CacheLoader = store;
            }

            if (xmlLocal.GetSafeElement("pre-load").GetBoolean())
            {
                try
                {
                    cache.LocalCache.LoadAll();
                }
                catch (Exception e)
                {
                    string text = "An exception occurred while pre-loading the \"" + info.CacheName + "\" cache:"
                                  + '\n' + e
                                  + "\nThe following configuration was used for the \"" + info.CacheName + "\" cache:"
                                  + '\n' + xmlLocal.ToString()
                                  + "\n(The exception has been logged and will be ignored.)";

                    CacheFactory.Log(text, CacheFactory.LogLevel.Warn);
                }
            }

            return cache;
        }

        /// <summary>
        /// Construct a <see cref="LocalNamedCache"/> using the specified
        /// parameters.
        /// </summary>
        /// <remarks>
        /// This method exposes a corresponding <b>LocalNamedCache</b>
        /// constructor and is provided for the express purpose of allowing
        /// its override.
        /// </remarks>
        /// <param name="units">
        /// The number of units that the cache manager will cache before
        /// pruning the cache.
        /// </param>
        /// <param name="expiryMillis">
        /// The number of milliseconds that each cache entry lives before
        /// being automatically expired.
        /// </param>
        /// <returns>
        /// A newly instantiated cache.
        /// </returns>
        protected virtual LocalNamedCache InstantiateLocalNamedCache(int units, int expiryMillis)
        {
            return new LocalNamedCache(units, expiryMillis);
        }

        /// <summary>
        /// Create a backing cache using the "class-scheme" element.
        /// </summary>
        /// <remarks>
        /// This method is a thin wrapper around
        /// <see cref="InstantiateAny"/>.
        /// </remarks>
        /// <param name="info">
        /// The cache info.
        /// </param>
        /// <param name="xmlClass">
        /// The "class-scheme" element.
        /// </param>
        /// <returns>
        /// A newly instantiated cache.
        /// </returns>
        protected virtual ICache InstantiateCache(CacheInfo info, IXmlElement xmlClass)
        {
            try
            {
                return (ICache) InstantiateAny(info, xmlClass);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Not a cache:\n" + xmlClass);
            }
        }

        /// <summary>
        /// Create an <see cref="ICacheLoader"/> or <see cref="ICacheStore"/>
        /// using the "class-scheme" element.
        /// </summary>
        /// <param name="info">
        /// The cache info.
        /// </param>
        /// <param name="xmlStore">
        /// The "class-scheme" or "extend-cache-scheme" element for the
        /// store.
        /// </param>
        /// <returns>
        /// A newly instantiated <b>ICacheStore</b>.
        /// </returns>
        protected virtual ICacheLoader InstantiateCacheStore(CacheInfo info, IXmlElement xmlStore)
        {
            xmlStore = ResolveScheme(xmlStore, info, true, false);
            if (xmlStore == null || XmlHelper.IsEmpty(xmlStore))
            {
                return null;
            }

            string schemeType = xmlStore.Name;
            try
            {
                switch (TranslateSchemeType(schemeType))
                {
                    case SchemeType.Class:
                        return (ICacheLoader) InstantiateAny(info, xmlStore);

                    case SchemeType.RemoteCache:
                        return (ICacheLoader) EnsureCache(info, xmlStore);

                    default:
                        throw new InvalidOperationException("InstantiateCacheStore: " + schemeType);
                }
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Not an ICacheLoader:\n" + xmlStore);
            }
        }

        /// <summary>
        /// Initialize the specified bundler using the "bundle-config" element.
        /// </summary>
        /// <param name="bundler">
        /// The bundler.
        /// </param>
        /// <param name="xmlBundle">
        /// A "bundle-config" element.
        /// </param>
        protected void initializeBundler(AbstractBundler bundler, IXmlElement xmlBundle)
        {
            if (bundler != null)
            {
                bundler.ThreadThreshold =
                    ConvertInt(xmlBundle.GetSafeElement("thread-threshold"), 4);
                bundler.DelayMillis =
                    ConvertInt(xmlBundle.GetSafeElement("delay-millis"), 1);
                bundler.AllowAutoAdjust =
                    xmlBundle.GetSafeElement("auto-adjust").GetBoolean(false);
            }
        }

        /// <summary>
        /// Create a BundlingNamedCache using the "operation-bundling" element.
        /// </summary>
        /// <param name="cache">
        /// The wrapped cache.
        /// </param>
        /// <param name="xmlBundling">
        /// The "operation-bundling" element.
        /// </param>
        /// <returns>
        /// A newly instantiated BundlingNamedCache.
        /// </returns>
        public BundlingNamedCache InstantiateBundlingNamedCache(INamedCache cache,
                IXmlElement xmlBundling)
        {
            BundlingNamedCache cacheBundle = new BundlingNamedCache(cache);
            for (IEnumerator enumerator = xmlBundling.GetElements("bundle-config");
                enumerator.MoveNext(); )
            {
                IXmlElement xmlBundleConfig = (IXmlElement) enumerator.Current;

                string sOperation = xmlBundleConfig.GetSafeElement("operation-name").GetString("all").Trim();
                int    cBundle    = ConvertInt(xmlBundleConfig.GetSafeElement("preferred-size"));

                if (sOperation.Equals("all"))
                {
                    initializeBundler(cacheBundle.EnsureGetBundler(cBundle), xmlBundleConfig);
                    initializeBundler(cacheBundle.EnsureInsertBundler(cBundle), xmlBundleConfig);
                    initializeBundler(cacheBundle.EnsureRemoveBundler(cBundle), xmlBundleConfig);
                }
                else if (sOperation.Equals("get"))
                {
                    initializeBundler(cacheBundle.EnsureGetBundler(cBundle), xmlBundleConfig);
                }
                else if (sOperation.Equals("put"))
                {
                    initializeBundler(cacheBundle.EnsureInsertBundler(cBundle), xmlBundleConfig);
                }
                else if (sOperation.Equals("remove"))
                {
                    initializeBundler(cacheBundle.EnsureRemoveBundler(cBundle), xmlBundleConfig);
                }
                else
                {
                    throw new ArgumentException(
                        "Invalid \"operation-name\" element:\n" + xmlBundleConfig);
                }
            }

            return cacheBundle;
        }

        /// <summary>
        /// Create an <see cref="ICacheListener"/> using the "class-scheme"
        /// element.
        /// </summary>
        /// <param name="info">
        /// The cache info.
        /// </param>
        /// <param name="xmlClass">
        /// The "class-scheme" element.
        /// </param>
        /// <returns>
        /// A newly instantiated <b>ICacheListener</b>.
        /// </returns>
        protected virtual ICacheListener InstantiateCacheListener(CacheInfo info, 
                IXmlElement xmlClass)
        {
            try
            {
                return (ICacheListener) InstantiateAny(info, xmlClass);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Not a listener:\n" + xmlClass);
            }    
        }

        /// <summary>
        /// Create an object using "class-scheme" element.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the value of any "param-value" element contains the literal
        /// "{cache-name}", replace it with the actual cache name.</p>
        /// <p>
        /// Finally, if the value of "param-type" is "{scheme-ref}" then the
        /// "param-value" should be a name of the scheme that will be used in
        /// place of the value.</p>
        /// </remarks>
        /// <param name="info">
        /// The cache info.
        /// </param>
        /// <param name="xmlClass">
        /// The "class-scheme" element.
        /// </param>
        /// <returns>
        /// A newly instantiated object.
        /// </returns>
        public virtual object InstantiateAny(CacheInfo info, IXmlElement xmlClass)
        {
            if (TranslateSchemeType(xmlClass.Name) != SchemeType.Class)
            {
                throw new ArgumentException("Invalid class definition: " + xmlClass);
            }

            return XmlHelper.CreateInstance(xmlClass, new ClassSchemeParameterResolver(this, info));
        }

        /// <summary>
        /// Construct an instance of the specified type using the specified
        /// parameters.
        /// </summary>
        /// <param name="typeName">
        /// The type name.
        /// </param>
        /// <param name="supType">
        /// The super type of the newly instantiated type.
        /// </param>
        /// <param name="initParams">
        /// The constructor paramters.
        /// </param>
        /// <param name="xmlParams">
        /// Optional <b>IXmlElement</b> ("init-params").
        /// </param>
        /// <returns>
        /// A newly instantiated object.
        /// </returns>
        protected virtual object InstantiateSubtype(string typeName, Type supType, object[] initParams, IXmlElement xmlParams)
        {
            if (typeName == null || typeName == String.Empty || supType == null)
            {
                throw new ArgumentException("Type name and super type must be specified");
            }

            try
            {
                Type type = TypeResolver.Resolve(typeName);
                if (!supType.IsAssignableFrom(type))
                {
                    throw new ArgumentException(supType + " is not a super-type of " + type);
                }

                object target;
                if (initParams == null)
                {
                    target = Activator.CreateInstance(type);
                }
                else
                {
                    target = ObjectUtils.CreateInstance(type, initParams);
                }

                if (xmlParams != null && target is IXmlConfigurable)
                {
                    IXmlElement xmlConfig = new SimpleElement("config");
                    XmlHelper.TransformInitParams(xmlConfig, xmlParams);

                    ((IXmlConfigurable) target).Config = xmlConfig;
                }

                return target;
            }
            catch (Exception e)
            {
                throw new Exception("Fail to instantiate subtype: " + typeName + " of " + supType, e);
            }
        }

        /// <summary>
        /// Release a cache managed by this factory, optionally destroying it.
        /// </summary>
        /// <remarks>
        /// This invalidates a reference obtained from the factory.
        /// </remarks>
        /// <param name="cache">
        /// The <b>INamedCache</b> object to be released.
        /// </param>
        /// <param name="destroy">
        /// True if the cache should also be destroyed.
        /// </param>
        /// <since>Coherence 3.5.1</since>
        protected virtual void ReleaseCache(INamedCache cache, bool destroy)
        {
            ScopedReferenceStore storeCache = StoreCache;
            String               cacheName  = cache.CacheName;

            lock(storeCache)
            {
                bool fFound = storeCache.ReleaseCache(cache);

                if (fFound)
                {
                    if (destroy)
                    {
                        cache.Destroy();
                    }
                    else if (cache.IsActive && cache is NearCache)
                    {
                        // release both the NearCache and its back
                        INamedCache cacheBack = (cache as NearCache).BackCache;

                        // the back must be handled last as the NearCache
                        // will reference it during its own cleanup
                        cache.Release();
                        cacheBack.Release();
                    }
                    else
                    {
                        cache.Release();
                    }
                }
                else if (cache.IsActive)
                {
                    // active, but not managed by this factory
                    throw new ArgumentException("The cache " + cacheName +
                            " was created using a different factory; that same" +
                            " factory should be used to release the cache.");
                }
            }
        }

        /// <summary>
        /// Translate the scheme name into the scheme type.
        /// </summary>
        /// <remarks>
        /// Valid scheme types are any of the <see cref="SchemeType"/>
        /// enumeration values.
        /// </remarks>
        /// <param name="scheme">
        /// The scheme name.
        /// </param>
        /// <returns>
        /// The scheme type.
        /// </returns>
        public virtual SchemeType TranslateSchemeType(string scheme)
        {
            return scheme.Equals("local-scheme")                  ? SchemeType.Local
                 : scheme.Equals("class-scheme")                  ? SchemeType.Class
                 : scheme.Equals("near-scheme")                   ? SchemeType.Near
                 : scheme.Equals("remote-cache-scheme")           ? SchemeType.RemoteCache
                 : scheme.Equals("remote-invocation-scheme")      ? SchemeType.RemoteInvocation
                 : scheme.Equals("view-scheme")                   ? SchemeType.View
                 : SchemeType.Unknown;
        }

        /// <summary>
        /// Ensure the service for the specified service name and type.
        /// </summary>
        /// <param name="serviceName">
        /// Service name.
        /// </param>
        /// <param name="serviceType">
        /// Service type.
        /// </param>
        /// <returns>
        /// An <b>IService</b> object.
        /// </returns>
        protected virtual IService EnsureService(string serviceName, ServiceType serviceType)
        {
            Debug.Assert(serviceName != null);

            ScopedReferenceStore storeService = StoreService;
            IService             service      = storeService.GetService(serviceName);

            if (service != null && service.IsRunning)
            {
                return service;
            }

            lock(storeService)
            {
                service = storeService.GetService(serviceName);
                if (service != null && service.IsRunning)
                {
                    return service;
                }

                switch (serviceType)
                {
                    case ServiceType.RemoteCache:
                        service =
                            new SafeCacheService()
                                {
                                    ServiceName        = serviceName,
                                    ServiceType        = serviceType,
                                    Principal          = Thread.CurrentPrincipal,
                                    OperationalContext = OperationalContext
                                };
                        break;

                    case ServiceType.RemoteInvocation:
                        service =
                            new SafeInvocationService()
                                {
                                    ServiceName        = serviceName,
                                    ServiceType        = serviceType,
                                    Principal          = Thread.CurrentPrincipal,
                                    OperationalContext = OperationalContext
                                };
                        break;

                    default:
                        throw new InvalidOperationException("invalid service type");
                }

                storeService.PutService(service, serviceName, serviceType);
            }

            return service;
        }

        /// <summary>
        /// Check if configuration is valid:
        /// <list type="bullet">
        /// <item>service definition must not be duplicated</item>
        /// </list>
        /// </summary>
        /// <param name="config">
        /// <b>IXmlElement</b> with factory configuration.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If configuration is not valid.
        /// </exception>
        protected virtual void ValidateConfig(IXmlElement config)
        {
            ArrayList serviceNameList = new ArrayList();

            foreach (IXmlElement xml in config.GetSafeElement("caching-schemes").ElementList)
            {
                IXmlElement xmlService = xml.GetElement("service-name");
                if (xmlService != null)
                {
                    serviceNameList.Add(xmlService.GetString());
                }
            }
            serviceNameList.Sort();

            int duplicateServiceIndex = -1;
            for (int i = 0, j = serviceNameList.Count - 1; i < j && duplicateServiceIndex == -1; i++)
            {
                if (serviceNameList[i].Equals(serviceNameList[i + 1]))
                {
                    duplicateServiceIndex = i;
                }
            }
            if (duplicateServiceIndex > -1)
            {
                throw new ArgumentException("Duplicate service definition in configuration: " + serviceNameList[duplicateServiceIndex]);
            }
        }

        /// <summary>
        /// Convert the value in the specified <see cref="IXmlValue"/> to an int.
        /// If the conversion fails, a warning will be logged.
        /// </summary>
        /// <param name="xmlValue">
        /// The element expected to contain an int value.
        /// </param>
        /// <returns>
        /// The int value in the provided element, or 0 upon a conversion
        /// failure.
        /// </returns>
        protected static int ConvertInt(IXmlValue xmlValue)
        {
            return ConvertInt(xmlValue, 0);
        }

        /// <summary>
        /// Convert the value in the specified <see cref="IXmlValue"/> to an int.
        /// If the conversion fails, a warning will be logged.
        /// </summary>
        /// <param name="xmlValue">
        /// The element expected to contain an int value.
        /// </param>
        /// <param name="defaultValue">
        /// The value that will be returned if the element does not contain
        /// a value that can be converted to int.
        /// </param>
        /// <returns>
        /// The int value in the provided element, or defaultValue upon a 
        /// conversion failure.
        /// </returns>
        protected static int ConvertInt(IXmlValue xmlValue, int defaultValue)
        {
            try
            {
                String value = xmlValue.GetString();
                object I     = XmlHelper.Convert(value, XmlValueType.Integer);

                return I == null ? defaultValue : (int) I;
            }
            catch (SystemException e)
            {
                ReportConversionError(xmlValue, "int", defaultValue.ToString(), e);
                return defaultValue;
            }
        }

        /// <summary>
        /// Log a failed type conversion.
        /// </summary>
        /// <param name="xmlValue">
        /// Element that contains the value that failed conversion.
        /// </param>
        /// <param name="type">
        /// Type that conversion was attempted to.
        /// </param>
        /// <param name="defaultValue">
        /// Default value that will be substituted.
        /// </param>
        /// <param name="e">
        /// Root cause of failed type conversion.
        /// </param>
        protected static void ReportConversionError(IXmlValue xmlValue, String type,
                String defaultValue, SystemException e)
        {
            CacheFactory.Log("Error converting " + xmlValue +
                    " to " + type + "; proceeding with default value of "
                    + defaultValue + "\n" + e.StackTrace,
                    CacheFactory.LogLevel.Warn);
        }

        /// <summary>
        /// Resolve and inject service serializer elements based on defaults
        /// defined in the cache configuration.
        /// </summary>
        /// <param name="xmlConfig">
        /// The configuration element to examine and modify.
        /// </param>
        protected void ResolveSerializer(IXmlElement xmlConfig)
        {
            IXmlElement xmlSerializer = xmlConfig.GetElement("serializer");
            if (xmlSerializer == null || XmlHelper.IsEmpty(xmlSerializer))
            {
                // remove an empty serializer element from the service config
                if (xmlSerializer != null)
                {
                    XmlHelper.RemoveElement(xmlConfig, "serializer");
                }

                // apply the default serializer (if specified)
                xmlSerializer = Config.FindElement("defaults/serializer");
                if (xmlSerializer != null)
                {
                    xmlConfig.ElementList.Add(xmlSerializer);
                }
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The default location of the cache configuration file.
        /// </summary>
        private static IResource s_configResource = ResourceLoader.GetResource(
                "cache-config.xml", true);

        /// <summary>
        /// The configuration XML.
        /// </summary>
        private IXmlElement m_xmlConfig;

        /// <summary>
        /// Store that holds cache references by name and optionally,
        /// if configured, IPrincipal.
        /// </summary>
        protected ScopedReferenceStore m_storeCache;

        /// <summary>
        /// Store that holds service references by name and optionally,
        /// if configured, IPrincipal.
        /// </summary>
        protected ScopedReferenceStore m_storeService;

        /// <summary>
        /// The IOperationalContext for this DefaultConfigurableCacheFactory.
        /// </summary>
        private IOperationalContext m_operationalContext;

        #endregion

        #region Constants

        /// <summary>
        /// The name of the replaceable parameter representing the cache
        /// name.
        /// </summary>
        public const string CACHE_NAME = "{cache-name}";

        /// <summary>
        /// The name of the replaceable parameter representing the scheme
        /// reference.
        /// </summary>
        public const string SCHEME_REF = "{scheme-ref}";

        #endregion

        #region Enum: SchemeType

        /// <summary>
        /// Scheme type enumeration.
        /// </summary>
        public enum SchemeType
        {
            /// <summary>
            /// The unknwown scheme type.
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// The near cache scheme.
            /// </summary>
            Near = 4,

            /// <summary>
            /// The local cache scheme.
            /// </summary>
            Local = 6,

            /// <summary>
            /// The custom class scheme.
            /// </summary>
            Class = 11,

            /// <summary>
            /// The remote cache scheme.
            /// </summary>
            RemoteCache = 16,

            /// <summary>
            /// The remote invocation scheme.
            /// </summary>
            RemoteInvocation = 17,
            
            /// <summary>
            /// The view cache scheme.
            /// </summary>
            View = 18
        }

        #endregion

        #region Inner class: CacheInfo

        /// <summary>
        /// A <b>CacheInfo</b> is a placeholder for cache attributes retrieved
        /// during parsing the corresponding cache mapping element.
        /// </summary>
        public class CacheInfo
        {
            #region Properties

            /// <summary>
            /// Obtain the cache name.
            /// </summary>
            /// <value>
            /// The cache name.
            /// </value>
            public virtual string CacheName
            {
                get { return m_cacheName; }
            }

            /// <summary>
            /// Obtain the scheme name.
            /// </summary>
            /// <value>
            /// The scheme name.
            /// </value>
            public virtual string SchemeName
            {
                get { return m_schemeName; }
            }

            /// <summary>
            /// Obtain the attributes dictionary.
            /// </summary>
            /// <value>
            /// The attributes dictionary.
            /// </value>
            public virtual IDictionary Attributes
            {
                get { return m_attributes; }
            }

            #endregion

            #region Constuctors

            /// <summary>
            /// Construct a <b>CacheInfo</b> object.
            /// </summary>
            /// <param name="cacheName">
            /// The cache name.
            /// </param>
            /// <param name="schemeName">
            /// The corresponding scheme name.
            /// </param>
            /// <param name="attributes">
            /// The corresponding dictionary of attributes.
            /// </param>
            public CacheInfo(string cacheName, string schemeName, IDictionary attributes)
            {
                m_cacheName  = cacheName;
                m_schemeName = schemeName;
                m_attributes = attributes;
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Find and replace the attributes names in "{}" format with the
            /// corresponding values for this cache info.
            /// </summary>
            /// <remarks>
            /// Note: the content of the specified <b>IXmlElement</b> could
            /// be modified, so the caller is supposed to clone the passed in
            /// XML if necessary.
            /// </remarks>
            /// <param name="xml">
            /// The <b>IXmlElement</b> to replace "{}" attributes at.
            /// </param>
            public virtual void ReplaceAttributes(IXmlElement xml)
            {
                foreach (IXmlElement xmlChild in xml.ElementList)
                {
                    if (xmlChild.Name.Equals("init-param"))
                    {
                        IXmlElement paramNameNode  = xmlChild.GetElement("param-name");
                        IXmlElement paramTypeNode  = xmlChild.GetElement("param-type");
                        IXmlElement paramValueNode = xmlChild.GetElement("param-value");

                        // if <init-param> contains <param-name> check if a
                        // parameter with the same name is predefined, and
                        // if so, replace the <param-value> inner text with
                        // the predefined parameter value
                        if (paramNameNode != null)
                        {
                            IDictionary attrs     = Attributes;
                            string      paramName = paramNameNode.GetString();
                            string      value     = attrs.Contains(paramName) ? (string) attrs[paramName] : null;

                            if (paramValueNode != null && value != null)
                            {
                                paramValueNode.SetString(value);
                            }
                        }

                        if (paramValueNode != null)
                        {
                            // parameter macro:
                            // <param-type>string</param-type>
                            // <param-value>{cache-name}</param-value>
                            if (CACHE_NAME.Equals(paramValueNode.GetString()))
                            {
                                paramValueNode.SetString(CacheName);
                            }

                            // parameter macro:
                            // <param-type>{scheme-ref}</param-type>
                            // <param-value>some_name</param-value>
                            else if (paramTypeNode != null
                                    && SCHEME_REF.Equals(paramTypeNode.GetString())
                                    && StringUtils.IsNullOrEmpty(paramValueNode.GetString()))
                            {
                                CacheFactory.Log("Missing parameter definition: "
                                        + SCHEME_REF + " for cache \""
                                        + CacheName + '"', CacheFactory.LogLevel.Warn);
                            }
                        }
                    }
                    // for all other elements, if the element contains a
                    // "param-name" attribute and a parameter with the
                    // same name is predefined, replace the element's
                    // inner text with the predefined parameter value
                    else
                    {
                        IXmlValue attribute = xmlChild.GetAttribute("param-name");
                        if (attribute != null)
                        {
                            IDictionary attrs     = Attributes;
                            string      paramName = attribute.GetString();
                            string      value     = attrs.Contains(paramName) ? (string) attrs[paramName] : null;

                            if (value != null)
                            {
                                xmlChild.SetString(value);
                            }

                            xmlChild.Attributes.Remove("param-name");
                        }
                    }
                    ReplaceAttributes(xmlChild);
                }
            }

            #endregion

            #region Data members

            /// <summary>
            /// The cache name.
            /// </summary>
            protected string m_cacheName;

            /// <summary>
            /// The corresponding scheme name.
            /// </summary>
            protected string m_schemeName;

            /// <summary>
            /// Map of scheme attributes.
            /// </summary>
            protected IDictionary m_attributes;

            #endregion
        }

        #endregion

        #region Inner class: ClassSchemeParameterResolver

        /// <summary>
        /// An <see cref="XmlHelper.IParameterResolver"/> implementation used
        /// by DefaultConfigurableCacheFactory when resolving class scheme
        /// configuration.
        /// </summary>
        protected class ClassSchemeParameterResolver : MacroParameterResolver
        {
            /// <summary>
            /// Create ClassSchemeParameterResolver with specified parent
            /// DefaultConfigurableCacheFactory and cache info.
            /// </summary>
            /// <param name="parent">
            /// Parent <see cref="DefaultConfigurableCacheFactory"/>.
            /// </param>
            /// <param name="info">
            /// <see cref="CacheInfo"/> instance.
            /// </param>
            public ClassSchemeParameterResolver(DefaultConfigurableCacheFactory parent, CacheInfo info) 
                : base(info.Attributes)
            {
                m_dccf      = parent;
                m_cacheInfo = info;
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
            override public object ResolveParameter(string type, string value)
            {
                DefaultConfigurableCacheFactory dccf = m_dccf;
                CacheInfo                       info = m_cacheInfo;

                if (type.Equals(SCHEME_REF))
                {
                    IXmlElement xmlScheme  = dccf.ResolveScheme(new CacheInfo(info.CacheName, value, info.Attributes));
                    string      schemeType = xmlScheme.Name;

                    switch (dccf.TranslateSchemeType(schemeType))
                    {
                        case SchemeType.Near:
                        case SchemeType.RemoteCache:
                        case SchemeType.View:    
                            return dccf.ConfigureCache(info, xmlScheme);

                        case SchemeType.Local:
                            return dccf.ConfigureBackingCache(info, xmlScheme);

                        case SchemeType.Class:
                            return dccf.InstantiateAny(info, xmlScheme);

                        case SchemeType.RemoteInvocation:
                            return dccf.EnsureService(xmlScheme);

                        default:
                            throw new InvalidOperationException("InstantiateAny: " + schemeType);
                    }
                }

                object result = base.ResolveParameter(type, value);
                return result ?? XmlHelper.UNRESOLVED;
            }

            private DefaultConfigurableCacheFactory m_dccf;
            private CacheInfo m_cacheInfo;
        }

        #endregion
    }
}