/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Tangosol.Net;
using Tangosol.Web.Model;

namespace Tangosol.Web;

/// <summary>
/// Coherence based impelentation of <see cref="ISessionStore"/>.
/// </summary>
/// <author>Vaso Putica  2024.06.06</author>
public class CoherenceSessionStore : ISessionStore
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the CoherenceSessionStore.
    /// </summary>
    /// <param name="optionsAccessor"></param>
    /// <param name="hostEnvironment"></param>
    public CoherenceSessionStore(IOptions<CoherenceSessionOptions> optionsAccessor, IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(optionsAccessor);
        m_options     = optionsAccessor.Value;
        ApplicationId = m_options.ApplicationId ?? hostEnvironment.ApplicationName;

        ConfigureCacheFactory(m_options);
    }

    #endregion

    #region Implementation of ISessionStore

    /// <inheritdoc />
    public ISession Create(
        string sessionKey,
        TimeSpan idleTimeout,
        TimeSpan ioTimeout,
        Func<bool> tryEstablishSession,
        bool isNewSessionKey)
    {
        return m_options.SessionType switch
        {
            CoherenceSessionOptions.HttpSessionType.Monolithic =>
                new MonolithicSession(
                    m_ccf.EnsureCache(m_options.CacheName),
                    ApplicationId,
                    sessionKey,
                    idleTimeout,
                    ioTimeout,
                    tryEstablishSession,
                    isNewSessionKey),
            CoherenceSessionOptions.HttpSessionType.Split =>
                new SplitSession(
                    m_ccf.EnsureCache(m_options.CacheName),
                    m_ccf.EnsureCache(m_options.OverflowCacheName),
                    ApplicationId,
                    sessionKey,
                    idleTimeout,
                    ioTimeout,
                    tryEstablishSession,
                    isNewSessionKey,
                    m_options.MinOverflowAttributeSize),
            _ => throw new InvalidOperationException($"Unknown Coherence session type: {m_options.SessionType}")
        };
    }

    #endregion

    #region Helper methods

    /// <summary>
    /// Configures CacheFactory based on the supplied options or,
    /// if they are missing, on the defaults.
    /// </summary>
    /// <param name="options"></param>
    protected void ConfigureCacheFactory(CoherenceSessionOptions options)
    {
        var ctx = new DefaultOperationalContext(options.CoherenceConfig);
        var ccf = new DefaultConfigurableCacheFactory(options.CacheConfig);
        ccf.OperationalContext = ctx;
        m_ccf = ccf;
    }

    #endregion

    #region Data members

    /// <summary>
    /// Coherence session options.
    /// </summary>
    protected readonly CoherenceSessionOptions m_options;

    /// <summary>
    /// Configurable Cache Factory
    /// </summary>
    protected IConfigurableCacheFactory m_ccf;

    /// <summary>
    /// Application identifier.
    /// </summary>
    protected string ApplicationId { get; }

    #endregion
}