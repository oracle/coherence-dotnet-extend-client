/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using Tangosol.IO.Pof;
using Tangosol.Util.Collections;
using Tangosol.Util.Processor;

namespace Tangosol.Web;

/// <summary>
/// Entry processor that updates session items.
/// </summary>
/// <author>Vaso Putica  2024.06.06</author>
public class SaveSessionProcessor : AbstractProcessor, IPortableObject
{
    #region Constructors

    /// <summary>
    /// Construct a new instance of  SaveSessionProcessor.
    /// </summary>
    /// <param name="idleTimeout">How long the session can be inactive (e.g. not accessed) before it will expire.</param>
    /// <param name="sessionValue">Session value to store into cache.</param>
    /// <param name="overflowAttrs">Overflow attributes.</param>
    /// <param name="obsoleteOverflowAttrs">Names of overflow attributes scheduled for removel.</param>
    public SaveSessionProcessor(
        long idleTimeout,
        SessionValue sessionValue,
        HashDictionary overflowAttrs,
        HashSet obsoleteOverflowAttrs
    )
    {
        m_idleTimeout       = idleTimeout;
        m_sessionValue      = sessionValue;
        m_overflowAttrs          = overflowAttrs;
        m_obsoleteOverflowAttrs  = obsoleteOverflowAttrs;
    }

    #endregion

    #region Implementation of IPortableObject

    /// <inheritdoc />
    public void ReadExternal(IPofReader reader)
    {
        m_idleTimeout            = reader.ReadInt64(0);
        m_sessionValue           = (SessionValue)reader.ReadObject(1);
        m_overflowAttrs          = (HashDictionary)reader.ReadDictionary(2, new HashDictionary());
        m_obsoleteOverflowAttrs  = (HashSet)reader.ReadCollection(3, new HashSet());
    }

    /// <inheritdoc />
    public void WriteExternal(IPofWriter writer)
    {
        writer.WriteInt64(0, m_idleTimeout);
        writer.WriteObject(1, m_sessionValue);
        writer.WriteDictionary(2, m_overflowAttrs);
        writer.WriteCollection(3, m_obsoleteOverflowAttrs);
    }

    #endregion

    #region Data members

    /// <summary>
    /// Session idle timeout.
    /// </summary>
    private long m_idleTimeout;

    /// <summary>
    /// Session value.
    /// </summary>
    private SessionValue m_sessionValue;

    /// <summary>
    /// Attributes to be stored in overflow cache.
    /// </summary>
    private HashDictionary m_overflowAttrs;

    /// <summary>
    /// Overflow attributes scheduled for removal.
    /// </summary>
    private HashSet m_obsoleteOverflowAttrs;

    #endregion
}