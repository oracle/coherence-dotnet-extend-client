/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;

namespace Tangosol.Net.Messaging.Impl.NameService
{
    /// <summary>
    /// <see cref="MessageFactory"/> for version 1 of the NameService
    /// Protocol.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The type identifiers of the <see cref="Message"/> types instantiated
    /// by this <b>MessageFactory</b> are organized as follows:</p>
    /// (0) <see cref="NameServiceResponse"/>
    /// (1) <see cref="LookupRequest"/>
    /// </remarks>
    /// <author>Wei Lin  2012.05.23</author>
    /// <since>Coherence 12.1.2</since>
    /// <seealso cref="MessageFactory"/>
    /// <seealso cref="NameServiceProtocol"/>
    public class NameServiceFactory : MessageFactory
    {
        /// <summary>
        /// Initialize an array of <see cref="Message"/> types that can be
        /// created by this factory.
        /// </summary>
        public NameServiceFactory()
        {
            Version = 1;
            InitializeMessageTypes(messagingTypes);
        }

        /// <summary>
        /// An array of <b>Message</b> types that can be created by this
        /// factory.
        /// </summary>
        private Type[] messagingTypes = { typeof(LookupRequest),
                                          typeof(NameServiceResponse) };
    }
}
