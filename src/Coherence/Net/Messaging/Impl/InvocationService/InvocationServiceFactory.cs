/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Net.Messaging.Impl.InvocationService
{
    /// <summary>
    /// <see cref="MessageFactory"/> for version 1 of the InvocationService
    /// Protocol.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The type identifiers of the <see cref="Message"/> types instantiated
    /// by this <b>MessageFactory</b> are organized as follows:</p>
    /// (0) <see cref="InvocationResponse"/>
    /// (1) <see cref="InvocationRequest"/>
    /// </remarks>
    /// <author>Goran Milosavljevic  2006.09.01</author>
    /// <seealso cref="MessageFactory"/>
    /// <seealso cref="InvocationServiceProtocol"/>
    public class InvocationServiceFactory : MessageFactory
    {
        /// <summary>
        /// Initialize an array of <see cref="Message"/> types that can be
        /// created by this factory.
        /// </summary>
        public InvocationServiceFactory()
        {
            Version = 1;
            InitializeMessageTypes(messagingTypes);
        }

        /// <summary>
        /// An array of <b>Message</b> types that can be created by this
        /// factory.
        /// </summary>
        private Type[] messagingTypes = { typeof(InvocationRequest),
                                          typeof(InvocationResponse) };
    }
}