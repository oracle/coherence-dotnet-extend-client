/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Reflection;
using Tangosol.Util.Collections;

namespace Tangosol.IO.Resources
{
    /// <summary>
    /// Registry class that contains mappings between various protocols
    /// and their associated resource handlers.
    /// </summary>
    /// <remarks>
    /// <p>The following protocols and resource handlers are 
    /// registered by default:</p>
    /// <list>
    /// <item>file - <see cref="FileResource"/></item>
    /// <item>http/https/ftp - <see cref="UrlResource"/></item>
    /// <item>assembly/asm - <see cref="EmbeddedResource"/></item>
    /// <item>web - <see cref="WebResource"/></item>
    /// </list>
    /// </remarks>
    /// <author>Aleksandar Seovic  2006.10.7</author>
    public class ResourceRegistry
    {
        #region Constructors

        /// <summary>
        /// Singleton constructor.
        /// </summary>
        private ResourceRegistry()
        {
            m_resourceHandlers = new HashDictionary();

            RegisterHandlerInternal("file",     typeof(FileResource));
            RegisterHandlerInternal("http",     typeof(UrlResource));
            RegisterHandlerInternal("https",    typeof(UrlResource));
            RegisterHandlerInternal("ftp",      typeof(UrlResource));
            RegisterHandlerInternal("assembly", typeof(EmbeddedResource));
            RegisterHandlerInternal("asm",      typeof(EmbeddedResource));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers resource handler for a specified protocol.
        /// </summary>
        /// <param name="protocolName">
        /// The name of the protocol to register handler for.
        /// </param>
        /// <param name="handlerType">
        /// Resource handler type to use for the specified protocol.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the specified type does not have a constructor that
        /// accepts a single argument of <see cref="string"/> type.
        /// </exception> 
        public static void RegisterHandler(string protocolName, 
                                           Type handlerType)
        {
            instance.RegisterHandlerInternal(protocolName, handlerType);
        }

        /// <summary>
        /// Returns resource handler for the protocol name specified.
        /// </summary>
        /// <param name="protocolName">
        /// Protocol name of the resource.
        /// </param>
        /// <returns>
        /// <b>ConstructorInfo</b> object that is handler constructor for the
        /// specified protocol name.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="protocolName"/> is <c>null</c>.
        /// </exception>
        public static ConstructorInfo GetHandler(string protocolName)
        {
            return (ConstructorInfo) instance.m_resourceHandlers[protocolName];
        }

        /// <summary>
        /// Determines whether a handler is registered for the specified
        /// protocol.
        /// </summary>
        /// <param name="protocolName">
        /// Protocol name.
        /// </param>
        /// <returns>
        /// <b>true</b> if a handler is registered for the specified
        /// protocol, otherwise <b>false</b>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="protocolName"/> is <c>null</c>.
        /// </exception>
        public static bool IsHandlerRegistered(string protocolName)
        {
            return instance.m_resourceHandlers.Contains(protocolName);
        }

        /// <summary>
        /// Registers resource handler for a specified protocol.
        /// </summary>
        /// <param name="protocolName">
        /// The name of the protocol to register handler for.
        /// </param>
        /// <param name="handlerType">
        /// Resource handler type to use for the specified protocol.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the specified type does not have a constructor that
        /// accepts a single argument of <see cref="string"/> type.
        /// </exception> 
        private void RegisterHandlerInternal(string protocolName,
                                             Type handlerType)
        {
            m_resourceHandlers[protocolName]
                            = GetHandlerConstructor(handlerType);
        }
        /// <summary>
        /// Finds a constructor that should be used to create 
        /// instances of this resource handler type.
        /// </summary>
        /// <param name="handlerType">The resource handler type.</param>
        /// <returns></returns>
        private static ConstructorInfo GetHandlerConstructor(Type handlerType)
        {
            ConstructorInfo ctor = 
                handlerType.GetConstructor(new Type[] {typeof(string)});
            
            if (ctor == null)
            {
                throw new ArgumentException(
                    string.Format("[{0}] does not have a constructor that " +
                                  "takes a single string as an argument.",
                                  handlerType.FullName));
            }
            return ctor;
        }

        #endregion

        #region Data members

        // singleton instance
        private static readonly ResourceRegistry instance = new ResourceRegistry();

        private readonly IDictionary m_resourceHandlers;

        #endregion
    }
}