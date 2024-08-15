/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using Tangosol.IO.Pof;
using Tangosol.Util.Processor;

namespace Tangosol.Web;

/// <summary>
/// Entry processor that resets session timeout.
/// </summary>
/// <author>Vaso Putica  2024.06.06</author>
public class ResetSessionTimeoutProcessor : AbstractProcessor, IPortableObject
{
    #region Constructors

    /// <summary>
    /// Construct new ResetSessionTimeoutProcessor
    /// </summary>
    /// <param name="timeout">How long the session can be inactive before it will expire.</param>
    public ResetSessionTimeoutProcessor(long timeout)
    {
        m_timeout = timeout;
    }

    #endregion

    #region Implementation of IPortableObject

    /// <inheritdoc />
    public void ReadExternal(IPofReader reader)
    {
        m_timeout = reader.ReadInt64(0);
    }

    /// <inheritdoc />
    public void WriteExternal(IPofWriter writer)
    {
        writer.WriteInt64(0, m_timeout);
    }

    #endregion

    #region Data members

    /// <summary>
    /// How long the session can be inactive before it will expire.
    /// </summary>
    private long m_timeout;

    #endregion
}