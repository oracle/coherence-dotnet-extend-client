/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Configuration;

using Tangosol.Config;

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
        private const string CONFIG_SECTION_NAME = "coherence";

        // TODO: remove this constant in a future release
        private const string _CONFIG_SECTION_NAME = "tangosol-coherence";

        /// <summary>
        /// Parses the Coherence configuration section within the standard
        /// .NET configuration file (App.config or Web.config).
        /// </summary>
        /// <returns>
        /// An instance of <see cref="CoherenceConfig"/> created by
        /// <see cref="CoherenceConfigHandler"/>.
        /// </returns>
        public static object GetCoherenceConfiguration()
        {
            // TODO: we still check for the legacy "tangosol-coherence" config
            // section for backwards compatibility; this check should be
            // removed in a future release
            object config = ConfigurationManager.GetSection(_CONFIG_SECTION_NAME);
            return config == null
                    ? ConfigurationManager.GetSection(CONFIG_SECTION_NAME)
                    : config;
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