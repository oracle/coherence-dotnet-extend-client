/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.Configuration;
using System.Xml;

using Tangosol.IO.Resources;

namespace Tangosol.Config
{
    /// <summary>
    /// Configuration section handler for the Coherence for .NET
    /// configuration section (&lt;coherence>).
    /// </summary>
    /// <seealso cref="CoherenceConfig"/>
    /// <author>Aleksandar Seovic  2006.10.06</author>
    /// <author>Goran Milosavljevic  2008.10.06</author>
    public class CoherenceConfigHandler : IConfigurationSectionHandler
    {
        /// <summary>
        /// Creates a <see cref="CoherenceConfig"/> with information parsed
        /// from the Coherence for .NET configuration section.
        /// </summary>
        /// <returns>
        /// The <b>CoherenceConfig</b> object with information from the
        /// configuration section.
        /// </returns>
        /// <param name="parent">Parent object.</param>
        /// <param name="configContext">Configuration context object.</param>
        /// <param name="section">Section XML node.</param>
        public object Create(object parent, object configContext, XmlNode section)
        {
            var config             = new CoherenceConfig();
            var cacheFactoryConfig = section.SelectSingleNode("cache-factory-config");
            var coherenceConfig    = section.SelectSingleNode("coherence-config");
            var cacheConfig        = section.SelectSingleNode("cache-config");
            var pofConfig          = section.SelectSingleNode("pof-config");
            var messagingDebug     = section.SelectSingleNode("COHERENCE_MESSAGING_DEBUG");

            config.ConfigProperties = new Hashtable();

            // TODO: The cache-factory-config element is deprecated as of
            // Coherence 3.7. We should remove support altogether in 4.0.
            if (cacheFactoryConfig != null)
            {
                config.OperationalConfig = ResourceLoader.GetResource(cacheFactoryConfig.InnerText);
            }
            if (coherenceConfig != null)
            {
                config.OperationalConfig = ResourceLoader.GetResource(coherenceConfig.InnerText);
            }
            if (cacheConfig != null)
            {
                config.CacheConfig = ResourceLoader.GetResource(cacheConfig.InnerText);
            }
            if (pofConfig != null)
            {
                config.PofConfig = ResourceLoader.GetResource(pofConfig.InnerText);
            }
            if (messagingDebug != null)
            {
                config.ConfigProperties.Add("COHERENCE_MESSAGING_DEBUG", messagingDebug.InnerText);
            }

            foreach (XmlNode node in section.ChildNodes)
            {
                if (node.Name.Equals("property"))
                {
                    XmlAttributeCollection attributes = node.Attributes;
                    if (attributes != null)
                    {
                        string key   = attributes["name"].Value;
                        string value = attributes["value"].Value;
                        if (key != null && value != null)
                        {
                            config.ConfigProperties.Add(key, value);
                        }
                    }
                }
            }

            return config;
        }
    }
}