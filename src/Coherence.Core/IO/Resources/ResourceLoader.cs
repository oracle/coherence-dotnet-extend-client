/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Reflection;
using System.Web;

namespace Tangosol.IO.Resources
{
    /// <summary>
    /// Loads resources from various sources.
    /// </summary>
    /// <remarks>
    /// Protocol name in the resource URI is used to determine the
    /// appropriate resource handler.
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <seealso cref="IResource"/>
    /// <seealso cref="ResourceRegistry"/>
    public class ResourceLoader
    {
        #region Constructors

        /// <summary>
        /// This class cannot be instantiated.
        /// </summary>
        private ResourceLoader()
        {}

        #endregion

        #region Methods

        /// <summary>
        /// Returns an <see cref="IResource"/> that has been mapped to the
        /// protocol of the supplied <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the resource.
        /// </param>
        /// <returns>
        /// A new <see cref="IResource"/> instance for the supplied
        /// <paramref name="resourceName"/>.
        /// </returns>
        /// <exception cref="UriFormatException">
        /// If an <see cref="IResource"/> type mapping does not exist for the
        /// supplied <paramref name="resourceName"/>.
        /// </exception>
        /// <exception cref="Exception">
        /// In the case of any errors arising from the instantiation of the
        /// returned <see cref="IResource"/> instance.
        /// </exception>
        public static IResource GetResource(string resourceName)
        {
            return GetResource(resourceName, false);
        }

        /// <summary>
        /// Returns an <see cref="IResource"/> that has been mapped to the
        /// protocol of the supplied <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the resource.
        /// </param>
        /// <param name="relative">
        /// If <b>true</b> the returned <see cref="IResource"/> will be
        /// relative to the application base directory or web application root.
        /// </param>
        /// <returns>
        /// A new <see cref="IResource"/> instance for the supplied
        /// <paramref name="resourceName"/>.
        /// </returns>
        /// <exception cref="UriFormatException">
        /// If an <see cref="IResource"/> type mapping does not exist for the
        /// supplied <paramref name="resourceName"/>.
        /// </exception>
        /// <exception cref="Exception">
        /// In the case of any errors arising from the instantiation of the
        /// returned <see cref="IResource"/> instance.
        /// </exception>
        public static IResource GetResource(string resourceName, bool relative)
        {
            string protocol = GetProtocol(resourceName);
            if (protocol == null)
            {
                protocol     = "file";
                resourceName = protocol + PROTOCOL_SEPARATOR + resourceName;
            }

            if (relative)
            {
                string rawName = resourceName.Substring(protocol.Length
                        + PROTOCOL_SEPARATOR.Length);
                if (protocol.Equals("file"))
                {
                    // files should be relative to the directory the application
                    // executable is in, not the current working directory
                    rawName = AppDomain.CurrentDomain.BaseDirectory + rawName;
                }
                else if (protocol.Equals("web"))
                {
                    // default config files should be relative to web app root
                    rawName = "~/" + rawName;
                }
                resourceName = protocol + PROTOCOL_SEPARATOR + rawName;
            }

            ConstructorInfo handler = ResourceRegistry.GetHandler(protocol);
            if (handler == null)
            {
                throw new UriFormatException("Resource handler for the '"
                        + protocol + "' protocol is not registered.");
            }

            return (IResource) handler.Invoke(new object[] { resourceName });
        }

        /// <summary>
        /// Checks that the supplied <paramref name="resourceName"/> starts
        /// with one of the protocol names currently mapped by this
        /// <b>ResourceLoader</b> instance.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the resource.
        /// </param>
        /// <returns>
        /// <b>true</b> if the supplied <paramref name="resourceName"/>
        /// starts with one of the known protocols; <b>false</b> if not, or
        /// if the supplied <paramref name="resourceName"/> is itself
        /// <c>null</c>.
        /// </returns>
        public static bool HasProtocol(string resourceName)
        {
            string protocol = GetProtocol(resourceName);
            return protocol != null
                    && ResourceRegistry.IsHandlerRegistered(protocol);
        }

        /// <summary>
        /// Extracts the protocol name from the supplied
        /// <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the resource.
        /// </param>
        /// <returns>
        /// The extracted protocol name or <c>null</c> if the supplied
        /// <paramref name="resourceName"/> is unqualified (or is itself
        /// <c>null</c>).
        /// </returns>
        internal static string GetProtocol(string resourceName)
        {
            if (resourceName == null)
            {
                return null;
            }
            int pos = resourceName.IndexOf(PROTOCOL_SEPARATOR);
            return pos == -1 ? null : resourceName.Substring(0, pos);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The separator between the protocol name and the resource name.
        /// </summary>
        public const string PROTOCOL_SEPARATOR = "://";

        #endregion
    }
}