/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Run.Xml;

namespace Tangosol.Net
{
    /// <summary>
    /// An interface for XML-driven cache factory.
    /// </summary>
    /// <author>Gene Gleyzer  2003.05.26</author>
    /// <author>Jason Howes  2006.06.28</author>
    /// <author>Ana Cikic  2006.09.19</author>
    /// <since>Coherence 2.2</since>
    /// <seealso cref="DefaultConfigurableCacheFactory"/>
    public interface IConfigurableCacheFactory : IXmlConfigurable
    {
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
        INamedCache EnsureCache(string cacheName);

        /// <summary>
        /// Release local resources associated with the specified instance of
        /// the cache.
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
        /// <seealso cref="DestroyCache"/>
        void ReleaseCache(INamedCache cache);

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
        /// <since>Coherence 3.5.1</since>
        /// <seealso cref="ReleaseCache"/>
        void DestroyCache(INamedCache cache);

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
        IService EnsureService(string serviceName);

        /// <summary>
        /// Release all resources allocated by this cache factory.
        /// </summary>
        void Shutdown();
    }
}