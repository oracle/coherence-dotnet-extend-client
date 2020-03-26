/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;

namespace Tangosol.IO.Resources
{
    /// <summary>
    /// An <see cref="IResource"/> implementation for resources stored within
    /// assemblies.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This implementation expects any resource name passed to the
    /// constructor to adhere to the following format:</p>
    /// <p>
    /// assembly://<i>assemblyName</i>/<i>namespace</i>/<i>resourceName</i>
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class EmbeddedResource : AbstractResource
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the <b>EmbeddedResource</b> class.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the assembly resource.
        /// </param>
        /// <exception cref="UriFormatException">
        /// If the supplied <paramref name="resourceName"/> did not conform
        /// to the expected format.
        /// </exception>
        /// <exception cref="FileLoadException">
        /// If the assembly specified in the supplied
        /// <paramref name="resourceName"/> was loaded twice with two
        /// different evidences.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// If the assembly specified in the supplied
        /// <paramref name="resourceName"/> could not be found.
        /// </exception>
        /// <exception cref="SecurityException">
        /// If the caller does not have the required permission to load
        /// the assembly specified in the supplied
        /// <paramref name="resourceName"/>.
        /// </exception>
        public EmbeddedResource(string resourceName) : base(resourceName)
        {
            string[] info = GetResourceNameWithoutProtocol(resourceName).Split('/');
            if (info.Length != 3)
            {
                throw new UriFormatException(
                    "Invalid resource name. The resource name has to be in the " +
                        "'assembly://<assemblyName>/<namespace>/<resourceName>' format.");
            }

            string assemblyName = info[0] == Assembly.GetExecutingAssembly().GetName().Name
                    ? typeof (EmbeddedResource).Assembly.FullName 
                    : info[0];
            AssemblyName an = assemblyName.Contains(",") 
                    ? new AssemblyName(assemblyName) 
                    : new AssemblyName { Name = assemblyName };

            m_assembly = Assembly.Load(an);
            if (m_assembly == null)
            {
                throw new FileNotFoundException("Unable to load assembly [" + an + "]");
            }

            m_resourceName = String.Format("{0}.{1}", info[1], info[2]);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Opens a stream for this <see cref="IResource"/>.
        /// </summary>
        /// <returns>
        /// A <b>System.IO.Stream</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// If the stream could not be opened.
        /// </exception>
        /// <exception cref="SecurityException">
        /// If the caller does not have the required permission to load the
        /// underlying assembly's manifest.
        /// </exception>
        public override Stream GetStream()
        {
            Stream stream = m_assembly.GetManifestResourceStream(m_resourceName);
            if (stream == null)
            {
                throw new IOException("Could not load resource with name = [" +
                                      m_resourceName + "] from assembly + " + m_assembly);
            }
            return stream;
        }

        #endregion

        #region Data members

        private readonly Assembly m_assembly;
        private readonly string   m_resourceName;

        #endregion
    }
}