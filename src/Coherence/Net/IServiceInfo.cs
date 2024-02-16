/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net
{
    /// <summary>
    /// The IServiceInfo represents information about an <see cref="IService"/>.
    /// </summary>
    /// <author>Gene Gleyzer  2002.02.08</author>
    /// <author>Goran Milosavljevic  2006.09.01</author>
    /// <since>Coherence 1.1</since>
    public interface IServiceInfo
    {
        /// <summary>
        /// Gets the name of the <see cref="IService"/>.
        /// </summary>
        /// <value>
        /// The name of the <b>IService</b>.
        /// </value>
        string ServiceName
        {
            get;
        }

        /// <summary>
        /// Gets the type of the <see cref="IService"/>.
        /// </summary>
        /// <value>
        /// The type of the <b>IService</b>.
        /// </value>
        /// <since>Coherence 2.0</since>
        ServiceType ServiceType
        {
            get;
        }
    }

    /// <summary>
    /// Service type constants.
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// The local cache service type constant.
        /// </summary>
        /// <remarks>
        /// A local cache service provides the means for handling a collection
        /// of resources limited to a single client with concurrent access
        /// control.
        /// </remarks>
        LocalCache = 0,

        /// <summary>
        /// The remote cache service type constant.
        /// </summary>
        /// <remarks>
        /// A remote cache service provides the means for handling a
        /// collection of resources managed by a remote cluster with
        /// concurrent access control.
        /// </remarks>
        RemoteCache = 1,

        /// <summary>
        /// The remote invocation service type constant.
        /// </summary>
        /// <remarks>
        /// A remote invocation service enables the execution of invocable
        /// objects by a remote cluster member.
        /// </remarks>
        RemoteInvocation = 2,

        /// <summary>
        /// The remote name service type constant.
        /// </summary>
        /// <remarks>
        /// A remote name service allows a JVM to use a remote NameService 
        /// without having to join the Cluster.
        /// </remarks>
        RemoteNameService = 3
    }
}
