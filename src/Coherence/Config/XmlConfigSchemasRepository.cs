/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;
using System.Xml;
using System.Xml.Schema;

using Tangosol.IO.Resources;

namespace Tangosol.Config
{
    /// <summary>
    /// Repository of parsed XML schemas that are used for Coherence for .NET
    /// configuration files validation.
    /// </summary>
    /// <author>Aleksandar Seovic  2006.10.13</author>
    public class XmlConfigSchemasRepository
    {
        #region Properties

        /// <summary>
        /// Returns a schema collection containing validation schemas for all
        /// registered parsers.
        /// </summary>
        /// <value>
        /// A schema collection containing validation schemas for all
        /// registered parsers.
        /// </value>
        public static XmlSchemaSet Schemas
        {
            get { return instance.schemas; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a singleton instance of the XmlConfigSchemasRepository.
        /// </summary>
        private XmlConfigSchemasRepository()
        {
            string fullAssembyName = GetType().Assembly.FullName;

            RegisterSchema("http://schemas.tangosol.com/coherence",
                           string.Format("assembly://{0}/Tangosol.Config/coherence.xsd", fullAssembyName));
            RegisterSchema("http://schemas.tangosol.com/cache",
                           string.Format("assembly://{0}/Tangosol.Config/cache-config.xsd", fullAssembyName));
            RegisterSchema("http://schemas.tangosol.com/pof",
                           string.Format("assembly://{0}/Tangosol.Config/pof-config.xsd", fullAssembyName));
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Parses specified schema file and associates it with a given
        /// namespace.
        /// </summary>
        /// <param name="namespaceUri">
        /// Namespace to associate schema with.
        /// </param>
        /// <param name="schemaLocation">
        /// Location of the physical schema file.
        /// </param>
        private void RegisterSchema(string namespaceUri, string schemaLocation)
        {
            IResource schema = ResourceLoader.GetResource(schemaLocation);
            using (Stream stream = schema.GetStream())
            {
                schemas.Add(namespaceUri, new XmlTextReader(stream));
            }
        }

        #endregion

        #region Data members

        // singleton field
        private static readonly XmlConfigSchemasRepository instance =
                new XmlConfigSchemasRepository();

        private readonly XmlSchemaSet schemas = new XmlSchemaSet();

        #endregion
    }
}