/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Tangosol.Net;

namespace Tangosol.Web.Model;

/// <summary>
/// Abstract base implementation of <see cref="ISession"/>
/// </summary>
/// <author>Vaso Putica  2024.06.06</author>
public abstract class AbstractSession : ISession
{
    #region Constructors

    /// <summary>
    /// Construct SplitSession
    /// </summary>
    /// <param name="sessionCache">The session cache</param>
    /// <param name="applicationId">The application identity</param>
    /// <param name="sessionKey">A unique key used to lookup the session.</param>
    /// <param name="idleTimeout">How long the session can be inactive (e.g. not accessed) before it will expire.</param>
    /// <param name="ioTimeout">
    /// The maximum amount of time <see cref="ISession.LoadAsync(System.Threading.CancellationToken)"/> and
    /// <see cref="ISession.CommitAsync(System.Threading.CancellationToken)"/> are allowed take.
    /// </param>
    /// <param name="tryEstablishSession">
    /// A callback invoked during <see cref="ISession.Set(string, byte[])"/> to verify that modifying the session is currently valid.
    /// If the callback returns <see langword="false"/>, <see cref="ISession.Set(string, byte[])"/> should throw an <see cref="InvalidOperationException"/>.
    /// <see cref="SessionMiddleware"/> provides a callback that returns <see langword="false"/> if the session was not established
    /// prior to sending the response.
    /// </param>
    /// <param name="isNewSessionKey"><see langword="true"/> if establishing a new session; <see langword="false"/> if resuming a session.</param>
    protected AbstractSession(
        INamedCache sessionCache,
        string applicationId,
        string sessionKey,
        TimeSpan idleTimeout,
        TimeSpan ioTimeout,
        Func<bool> tryEstablishSession,
        bool isNewSessionKey)
    {
        ArgumentNullException.ThrowIfNull(sessionCache);
        if (string.IsNullOrEmpty(sessionKey))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(sessionKey));
        }

        ArgumentNullException.ThrowIfNull(tryEstablishSession);

        SessionCache        = sessionCache;
        ApplicationId       = applicationId;
        IdleTimeout         = idleTimeout;
        IoTimeout           = ioTimeout;
        TryEstablishSession = tryEstablishSession;
        IsNewSessionKey     = isNewSessionKey;
        SessionKey          = new SessionKey(applicationId, sessionKey);
    }

    #endregion

    #region Implementation of ISession

    /// <inheritdoc cref="ISession.LoadAsync"/>
    public abstract Task LoadAsync(CancellationToken cancellationToken = new CancellationToken());

    /// <inheritdoc cref="ISession.CommitAsync"/>
    public abstract Task CommitAsync(CancellationToken cancellationToken = new CancellationToken());

    /// <inheritdoc cref="ISession.TryGetValue"/>
    public abstract bool TryGetValue(string key, out byte[] value);

    /// <inheritdoc cref="ISession.Set"/>
    public abstract void Set(string key, byte[] value);

    /// <inheritdoc cref="ISession.Remove"/>
    public abstract void Remove(string key);

    /// <inheritdoc cref="ISession.Clear"/>
    public abstract void Clear();

    /// <inheritdoc cref="ISession.IsAvailable"/>
    public abstract bool IsAvailable { get; }

    /// <inheritdoc cref="ISession.Id"/>
    public string Id => SessionKey.SessionId;

    /// <inheritdoc cref="ISession.Keys"/>
    public abstract IEnumerable<string> Keys { get; }

    #endregion

    #region Helper methods

    /// <summary>
    /// Reset session timeout.
    /// </summary>
    protected virtual void ResetSessionTimeout()
    {
        SessionCache.Invoke(SessionKey,
            new ResetSessionTimeoutProcessor(Convert.ToInt64(IdleTimeout.TotalMilliseconds)));
    }

    #endregion

    #region Properties

    /// <summary>
    /// Session cache.
    /// </summary>
    protected INamedCache SessionCache { get; }

    /// <summary>
    /// The application identity
    /// </summary>
    protected string ApplicationId { get; }

    /// <summary>
    ///  a unique key used to look up the session.
    /// </summary>
    protected SessionKey SessionKey { get; }

    /// <summary>
    /// How long the session can be inactive (e.g. not accessed) before it will expire.
    /// </summary>
    protected TimeSpan IdleTimeout { get; }

    /// <summary>
    ///    A callback invoked during <see cref="Set(string, byte[])"/> to verify that modifying the session is currently valid.
    /// If the callback returns <see langword="false"/>, <see cref="Set(string, byte[])"/> throws an <see cref="InvalidOperationException"/>.
    /// <see cref="SessionMiddleware"/> provides a callback that returns <see langword="false"/> if the session was not established
    /// prior to sending the response.
    /// </summary>
    /// <returns></returns>
    protected Func<bool> TryEstablishSession { get; }

    /// <summary>
    /// The maximum amount of time <see cref="LoadAsync(CancellationToken)"/> and <see cref="CommitAsync(CancellationToken)"/> are allowed take.
    /// </summary>
    protected TimeSpan IoTimeout { get; }

    /// <summary>
    /// <see langword="true"/> if establishing a new session; <see langword="false"/> if resuming a session.
    /// </summary>
    protected bool IsNewSessionKey { get; }

    #endregion
}