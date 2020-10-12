/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Net.Messaging.Impl.CacheService
{
    /// <summary>
    /// <see cref="MessageFactory"/> implementation for version 1 of the
    /// CacheService Protocol.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The type identifiers of the <see cref="Message"/> types instantiated
    /// by this <b>MessageFactory</b> are organized as follows:</p>
    /// (0) <see cref="CacheServiceResponse"/>
    /// (1) <see cref="EnsureCacheRequest"/>
    /// (2) <see cref="DestroyCacheRequest"/>
    /// </remarks>
    /// <author>Ana Cikic  2006.08.25</author>
    /// <seealso cref="MessageFactory"/>
    /// <seealso cref="CacheServiceProtocol"/>
    public class CacheServiceFactory : MessageFactory
    {
        /// <summary>
        /// Initialize an array of <see cref="Message"/> types that can be
        /// created by this factory.
        /// </summary>
        public CacheServiceFactory()
        {
            InitializeMessageTypes(messagingTypes);
        }

        /// <summary>
        /// An array of Message types that can be created by this factory.
        /// </summary>
        private Type[] messagingTypes = { typeof(DestroyCacheRequest),
                                          typeof(EnsureCacheRequest),
                                          typeof(CacheServiceResponse) };
    }
}