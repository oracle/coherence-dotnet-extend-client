/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;

using Tangosol.Net;
using Tangosol.Util;
using Tangosol.Util.Collections;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;

namespace Tangosol.Web.Model;

/// <summary>
/// Implementation of <see cref="ISession"/> that stores large
/// session attributes as separate cache entries.
/// </summary>
/// <author>Vaso Putica  2024.06.06</author>
public class SplitSession
    : AbstractSession, ISession
{
    #region Constructors

    /// <summary>
    /// Construct a new instance of SplitSession.
    /// </summary>
    /// <param name="sessionCache">The session cache</param>
    /// <param name="overflowCache">The overflow attributes cache</param>
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
    /// <param name="minOverflowAttrSize">Minimum overflow attribute size.</param>
    public SplitSession(
        INamedCache sessionCache,
        INamedCache overflowCache,
        string applicationId,
        string sessionKey,
        TimeSpan idleTimeout,
        TimeSpan ioTimeout,
        Func<bool> tryEstablishSession,
        bool isNewSessionKey,
        int minOverflowAttrSize)
        : base(sessionCache, applicationId, sessionKey, idleTimeout, ioTimeout, tryEstablishSession, isNewSessionKey)
    {
        ArgumentNullException.ThrowIfNull(overflowCache);

        m_overflowCache       = overflowCache;
        m_minOverflowAttrSize = minOverflowAttrSize;
    }

    #endregion

    #region Implementation of ISession

    /// <inheritdoc />
    public override Task LoadAsync(CancellationToken cancellationToken = new())
    {
        return Task.Run(Load, cancellationToken);
    }

    /// <inheritdoc />
    public override Task CommitAsync(CancellationToken cancellationToken = new())
    {
        return Task.Run(() =>
        {
            if (m_isModified)
            {
                HashDictionary updatedOverflowAttrs = new();
                HashSet obsoletedOverflowAttrs = new();

                foreach (var entry in m_overflowAttrsStatus)
                {
                    switch (entry.Value)
                    {
                        case OverflowAttrStatus.Updated:
                            updatedOverflowAttrs.Add(entry.Key, m_overflowAttrStore[entry.Key]);
                            break;
                        case OverflowAttrStatus.Obsoleted:
                            obsoletedOverflowAttrs.Add(entry.Key);
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown OverflowAttrStatus: {entry.Value}");
                    }
                }

                SessionValue sessionVal = new SessionValue(m_attrStore, m_overflowAttrStore.Keys);
                SessionCache.Invoke(
                    SessionKey,
                    new SaveSessionProcessor(
                        Convert.ToInt64(IdleTimeout.TotalMilliseconds),
                        sessionVal,
                        updatedOverflowAttrs,
                        obsoletedOverflowAttrs));
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
        if (m_attrStore.ContainsKey(key))
        {
            value = (byte[])m_attrStore[key];
            return true;
        }

        if (m_overflowAttrStore.ContainsKey(key))
        {
            byte[] externalValue = m_overflowAttrStore[key];
            if (externalValue != IN_EXTERNAL_CACHE)
            {
                value = externalValue;
                return true;
            }

            var overflowAttributeKey = new OverflowAttributeKey(SessionKey, key);
            byte[] attrVal = (byte[])m_overflowCache[overflowAttributeKey];
            if (attrVal is null)
            {
                CacheFactory.Log(
                    $"Overflow attribute doesn't exist in overflow cache for key: {overflowAttributeKey}",
                    CacheFactory.LogLevel.Warn);
                m_overflowAttrStore.Remove(key);
                m_overflowAttrsStatus[key] = OverflowAttrStatus.Obsoleted;
                value                      = null;
                return false;
            }
            else
            {
                m_overflowAttrStore[key] = attrVal;
                value                    = attrVal;
                return true;
            }
        }

        value = null;
        return false;
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

            bool isOverflowAttr = value.Length >= m_minOverflowAttrSize;

            var copy = new byte[value.Length];
            Buffer.BlockCopy(src: value, srcOffset: 0, dst: copy, dstOffset: 0, count: value.Length);

            if (isOverflowAttr)
            {
                m_attrStore.Remove(key);
                m_overflowAttrStore[key]   = copy;
                m_overflowAttrsStatus[key] = OverflowAttrStatus.Updated;
            }
            else
            {
                m_attrStore[key] = copy;
                if (m_overflowAttrStore.ContainsKey(key))
                {
                    m_overflowAttrStore.Remove(key);
                    m_overflowAttrsStatus[key] = OverflowAttrStatus.Obsoleted;
                }
            }
            m_isModified = true;
        }
    }

    /// <inheritdoc />
    public override void Remove(string key)
    {
        Load();
        if (m_attrStore.ContainsKey(key))
        {
            m_attrStore.Remove(key);
            m_isModified = true;
        }
        else if (m_overflowAttrStore.ContainsKey(key))
        {
            m_overflowAttrStore.Remove(key);
            m_overflowAttrsStatus[key] = OverflowAttrStatus.Obsoleted;
            m_isModified               = true;
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        Load();
        if (m_attrStore.Count == 0 && m_overflowAttrStore.Count == 0)
        {
            return;
        }

        m_isModified = true;
        foreach (string extStoreKey in m_overflowAttrStore.Keys)
        {
            m_overflowAttrsStatus[extStoreKey] = OverflowAttrStatus.Obsoleted;
        }

        m_attrStore.Clear();
        m_overflowAttrStore.Clear();
    }

    #endregion

    #region Helper methods

    /// <summary>
    /// Loads all attributes from primary session cache and all overflow attribute keys
    /// from overflow attribute cache.
    /// </summary>
    private void Load()
    {
        if (!m_isLoaded)
        {
            try
            {
                SessionValue session = (SessionValue)SessionCache[SessionKey];
                IEnumerable smallAttrs = session?.Attributes;
                if (smallAttrs is not null)
                {
                    m_attrStore.Clear();
                    foreach (DictionaryEntry smallAttr in smallAttrs)
                    {
                        m_attrStore[smallAttr.Key] = (byte[])smallAttr.Value;
                    }
                }
                else if (!IsNewSessionKey)
                {
                    // accessing expired session
                }

                // load overflow attr keys
                IValueExtractor extractor = new ReflectionExtractor("getSessionKey", null, AbstractExtractor.KEY);
                ICollection overflowAttrKeys = m_overflowCache.GetKeys(new EqualsFilter(extractor, SessionKey));

                m_overflowAttrStore.Clear();
                foreach (OverflowAttributeKey eak in overflowAttrKeys)
                {
                    m_overflowAttrStore[eak.AttributeName] = IN_EXTERNAL_CACHE;
                }

                m_isAvailable = true;
                ResetSessionTimeout();
            }
            catch (Exception)
            {
                m_isAvailable = false;
                m_attrStore.Clear();
                m_overflowAttrStore.Clear();

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
            var keys = new List<string>(m_attrStore.Count + m_overflowAttrStore.Count);
            CollectionUtils.AddAll(keys, m_attrStore.Keys);
            keys.AddRange(m_overflowAttrStore.Keys);

            return keys;
        }
    }

    #endregion

    #region Data members

    /// <summary>
    /// Overflow attributes cache.
    /// </summary>
    private readonly INamedCache m_overflowCache;

    /// <summary>
    /// Minimum overflow attribute size.
    /// </summary>
    private readonly int m_minOverflowAttrSize;

    /// <summary>
    /// Store for small attributes.
    /// </summary>
    private readonly HashDictionary m_attrStore = new();

    /// <summary>
    /// Store for overflow attributes.
    /// </summary>
    private readonly IDictionary<string, byte[]> m_overflowAttrStore = new Dictionary<string, byte[]>();

    /// <summary>
    /// Overflow attribute status tracker.
    /// </summary>
    private readonly IDictionary<string, OverflowAttrStatus> m_overflowAttrsStatus = new Dictionary<string, OverflowAttrStatus>();

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

    /// <summary>
    /// Marks that the overflow attribute value has not yet been read from the overflow cache.
    /// </summary>
    private static readonly byte[] IN_EXTERNAL_CACHE = new byte[0];

    #endregion

    /// <summary>
    /// Overflow attribute status constants.
    /// </summary>
    private enum OverflowAttrStatus
    {
        Obsoleted,
        Updated
    }
}