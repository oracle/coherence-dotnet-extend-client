/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using Microsoft.Extensions.Configuration;
using Tangosol.Config;
using Tangosol.IO.Resources;

namespace Tangosol.Util
{
    /// <summary>
    /// Helper class used for .NET configuration files access.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class ConfigurationUtils
    {
        /// <summary>
        /// The name of the configuration element that contains Coherence
        /// configuration settings.
        /// </summary>
        private const string CONFIG_SECTION_NAME = "Coherence";

        /// <summary>
        /// Parses the Coherence configuration section within the standard
        /// .NET configuration file (appsettings.json).
        /// </summary>
        /// <returns>
        /// An instance of <see cref="CoherenceConfig"/>
        /// </returns>
        public static object GetCoherenceConfiguration()
        {
            IConfiguration cohCfg = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .Build()
                .GetSection(CONFIG_SECTION_NAME);

            CoherenceConfig config = new CoherenceConfig();
            string coherenceConfig = cohCfg["CoherenceConfig"];
            string cacheConfig     = cohCfg["CacheConfig"];
            string pofConfig       = cohCfg["PofConfig"];
            string messagingDebug  = cohCfg["COHERENCE_MESSAGING_DEBUG"];

            config.ConfigProperties = new Hashtable();
            if (coherenceConfig != null)
            {
                config.OperationalConfig = ResourceLoader.GetResource(coherenceConfig);
            }
            if (cacheConfig != null)
            {
                config.CacheConfig = ResourceLoader.GetResource(cacheConfig);
            }
            if (pofConfig != null)
            {
                config.PofConfig = ResourceLoader.GetResource(pofConfig);
            }
            if (messagingDebug != null)
            {
                config.ConfigProperties.Add("COHERENCE_MESSAGING_DEBUG", messagingDebug);
            }

            IConfigurationSection properties = cohCfg.GetSection("Properties");
            foreach (var property in properties.GetChildren())
            {
                config.ConfigProperties.Add(property.Key, property.Value);
            }

            return config;
        }

        /// <summary>
        /// Get the value of a given Coherence configuration property.
        /// </summary>
        /// <param name="name">
        /// The name of the property.
        /// </param>
        /// <param name="defaultValue">
        /// The default value for the property.
        /// </param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public static string GetProperty(string name, string defaultValue)
        {
            CoherenceConfig config = (CoherenceConfig) GetCoherenceConfiguration();
            string          value  = config == null ? null : (string) config.ConfigProperties[name];

            // environment variable overrides app.config specified property
            string envVar = Environment.GetEnvironmentVariable(name);
            if (envVar != null)
            {
                value = envVar;
            }

            return value == null ? defaultValue : value;
        }
    }
}