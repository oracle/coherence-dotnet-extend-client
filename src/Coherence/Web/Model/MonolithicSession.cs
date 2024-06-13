/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;

using Tangosol.Net;

namespace Tangosol.Web.Model;

/// <summary>
/// Implementation of <see cref="ISession"/> that stores
/// all session attributes in a single cache entry.
/// </summary>
/// <author>Vaso Putica  2024.06.06</author>
public class MonolithicSession
    : AbstractSession, ISession
{
    #region Constructors

    /// <summary>
    /// Construct MonolithicSession.
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
    public MonolithicSession(
        INamedCache sessionCache,
        string applicationId,
        string sessionKey,
        TimeSpan idleTimeout,
        TimeSpan ioTimeout,
        Func<bool> tryEstablishSession,
        bool isNewSessionKey)
        : base(sessionCache, applicationId, sessionKey, idleTimeout, ioTimeout, tryEstablishSession, isNewSessionKey)
    {
    }

    #endregion

    #region Implementation of ISession

    /// <inheritdoc />
    public override Task LoadAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.Run(Load, cancellationToken);
    }

    /// <inheritdoc />
    public override Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.Run(() =>
        {
            if (m_isModified)
            {
                SessionCache.Insert(SessionKey, m_attrStore, Convert.ToInt64(IdleTimeout.TotalMilliseconds));
                m_isModified = false;
            }
            else
            {
                ResetSessionTimeout();
            }
        }, cancellationToken);
    }


    /// <inheritdoc />
    public override bool TryGetValue(string key, out byte[] value)
    {
        Load();
        return m_attrStore.TryGetValue(key, out value);
    }

    /// <inheritdoc />
    public override void Set(string key, byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (IsAvailable)
        {
            if (!TryEstablishSession())
            {
                throw new InvalidOperationException("The session cannot be established after the response has started.");
            }

            var copy = new byte[value.Length];
            Buffer.BlockCopy(src: value, srcOffset: 0, dst: copy, dstOffset: 0, count: value.Length);

            m_attrStore[key] = value;
            m_isModified = true;
        }
    }

    /// <inheritdoc />
    public override void Remove(string key)
    {
        Load();
        m_isModified |= m_attrStore.Remove(key);
    }

    /// <inheritdoc />
    public override void Clear()
    {
        Load();
        m_isModified |= m_attrStore.Count > 0;
        m_attrStore.Clear();
    }

    #endregion

    #region Helper methods

    /// <summary>
    /// Loads all attributes from the session cache.
    /// </summary>
    private void Load()
    {
        if (!m_isLoaded)
        {
            try
            {
                IEnumerable data = (IEnumerable)SessionCache[SessionKey];
                if (data is not null)
                {
                    m_attrStore.Clear();
                    foreach (DictionaryEntry entry in data)
                    {
                        m_attrStore[(string)entry.Key] = (byte[])entry.Value;
                    }
                }
                else if (!IsNewSessionKey)
                {
                    // accessing expired session
                }

                m_isAvailable = true;
            }
            catch (Exception)
            {
                m_isAvailable = false;
                m_attrStore.Clear();

                throw;
            }
            finally
            {
                m_isLoaded = true;
            }
        }
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public override bool IsAvailable
    {
        get
        {
            Load();
            return m_isAvailable;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<string> Keys
    {
        get
        {
            Load();
            return m_attrStore.Keys.ToImmutableList();
        }
    }

    #endregion

    #region Data members

    private readonly Dictionary<string, byte[]> m_attrStore = new();

    /// <summary>
    /// Indicates whether the current session loaded successfully.
    /// </summary>
    private bool m_isAvailable;

    /// <summary>
    /// Flag specifying whether this session was modified.
    /// </summary>
    private bool m_isModified;

    /// <summary>
    /// Flag specifying whether this session was successfully loaded from the cache.
    /// </summary>
    private bool m_isLoaded;

    #endregion
}