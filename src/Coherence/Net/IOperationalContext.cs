/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.Collections;

using Tangosol.IO;
using Tangosol.Net.Security;

namespace Tangosol.Net
{
    /// <summary>
    /// IOperationalConetxt is an interface for providing Oracle Coherence
    /// operational configuration.
    /// </summary>
    /// <author>Wei Lin  2010.11.3</author>
    /// <since>Coherence 3.7</since>
    public interface IOperationalContext
    {
        /// <summary>
        /// The TTL for multicast based discovery.
        /// </summary>
        /// <since>12.2.1</since>
        int DiscoveryTimeToLive { get; }

        /// <summary>
        /// The product edition.
        /// </summary>
        /// <value>
        /// The product edition.
        /// </value>
        int Edition { get; }

        /// <summary>
        /// The product edition in a formatted string.
        /// </summary>
        /// <value>
        /// The product edition in a formatted string.
        /// </value>
        string EditionName { get; }

        /// <summary>
        /// An <see cref="IMember"/> object representing this process.
        /// </summary>
        /// <value>
        /// The local <see cref="IMember"/>.
        /// </value>
        IMember LocalMember { get; }

        /// <summary>
        /// A dictionary of network filter factories.
        /// </summary>
        /// <value>
        /// A dictionary of <see cref="IWrapperStreamFactory"/> objects keyed
        /// by filter name.
        /// </value>
        IDictionary FilterMap { get; }

        /// <summary>
        /// A dictionary of serializer factories.
        /// </summary>
        /// <value>
        /// A dictionary of <see cref="ISerializerFactory"/> objects keyed
        /// by serializer name.
        /// </value>
        IDictionary SerializerMap { get; }

        /// <summary>
        /// A dictionary of address provider factories.
        /// </summary>
        /// <value>
        /// A dictionary of <see cref="IAddressProviderFactory"/> objects keyed
        /// by name.
        /// </value>
        IDictionary AddressProviderMap { get; }

        /// <summary>
        /// An <see cref="IIdentityAsserter"/> that can be used to establish a
        /// user's identity.
        /// </summary>
        /// <value>
        /// The <see cref="IIdentityAsserter"/>.
        /// </value>
        IIdentityAsserter IdentityAsserter { get; }

        /// <summary>
        /// An <see cref="IIdentityTransformer"/> that can be used to transform
        /// an IPrincipal into an identity assertion.
        /// </summary>
        /// <value>
        /// The <see cref="IIdentityTransformer"/>.
        /// </value>
        IIdentityTransformer IdentityTransformer { get; }

        /// <summary>
        /// Indicates if principal scoping is enabled.
        /// </summary>
        /// <value>
        /// <b>true</b> if principal scoping is enabled.
        /// </value>
        bool IsPrincipalScopingEnabled { get; }

        /// <summary>
        /// The logging severity level.
        /// </summary>
        /// <value>
        /// The loggng severity level.
        /// </value>
        int LogLevel { get; }

        /// <summary>
        /// The maximum number of characters for a logger daemon to queue
        /// before truncating.
        /// </summary>
        /// <value>
        /// The maximum number of characters for a logger daemon to queue
        /// before truncating.
        /// </value>
        int LogCharacterLimit { get; }

        /// <summary>
        /// The log message format.
        /// </summary>
        /// <value>
        /// The log message format.
        /// </value>
        string LogMessageFormat { get; }

        /// <summary>
        /// The destination for log messages.
        /// </summary>
        /// <value>
        /// The destination for log messages.
        /// </value>
        string LogDestination { get; }

        /// <summary>
        /// The name of the logger.
        /// </summary>
        /// <value>
        /// The name of the logger.
        /// </value>
        string LogName { get; }
    }
}