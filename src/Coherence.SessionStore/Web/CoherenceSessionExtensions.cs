/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tangosol.Web;

/// <summary>
/// Extension methods for adding the Coherence based ASP.NET sessions support to an application.
/// </summary>
/// <author>Vaso Putica  2024.06.06</author>
public static class CoherenceSessionExtensions
{
    /// <summary>
    /// Initializes Coherence ASP.NET session support.
    /// </summary>
    /// <param name="services">The service collection container</param>
    /// <param name="configureOptions">Action to configure Coherence Session Options</param>
    /// <returns></returns>
    public static IServiceCollection UseCoherenceSession(
        this IServiceCollection services,
        Action<CoherenceSessionOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.Add(ServiceDescriptor.Singleton<ISessionStore, CoherenceSessionStore>());

        return services;
    }

    /// <summary>
    /// Initializes Coherence ASP.NET session support.
    /// </summary>
    /// <param name="services">The service collection container</param>
    /// <param name="configSection">The configuration section that contains the configuration</param>
    /// <returns></returns>
    public static IServiceCollection UseCoherenceSession(
        this IServiceCollection services,
        IConfigurationSection configSection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configSection);

        services.Configure<CoherenceSessionOptions>(configSection);
        services.Add(ServiceDescriptor.Singleton<ISessionStore, CoherenceSessionStore>());

        return services;
    }
}