/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System.Collections;

using Tangosol.IO.Pof;
using Tangosol.Util.Collections;

namespace Tangosol.Web;

/// <summary>
/// A holder object that stores session attributes for a single session.
/// </summary>
/// <author>Vaso Putica  2024.06.06</author>
public class SessionValue : IPortableObject
{
    #region Constructors

    /// <summary>
    /// Deserialization constructor (for internal use only).
    /// </summary>
    public SessionValue()
    {
    }

    /// <summary>
    /// Initializes a new instance of the SessionValue.
    /// </summary>
    /// <param name="attributes">Session attributes except for overflow attributes.</param>
    /// <param name="overflowAttrs">Overflow session attributes</param>
    /// <exception cref="ArgumentNullException"></exception>
    public SessionValue(IDictionary attributes, ICollection<string> overflowAttrs)
    {
        m_attributes    = attributes ?? throw new ArgumentNullException(nameof(attributes));
        m_overflowAttrs = overflowAttrs ?? throw new ArgumentNullException(nameof(overflowAttrs));
    }

    #endregion

    #region Properties

    /// <summary>
    /// Session attributes except for overflow attributes.
    /// </summary>
    public IDictionary Attributes => m_attributes;

    #endregion

    #region Implementation of IPortableObject

    /// <inheritdoc />
    public void ReadExternal(IPofReader reader)
    {
        m_attributes    = reader.ReadDictionary(0, new HashDictionary());
        m_overflowAttrs = reader.ReadCollection(1, new HashSet<string>());
    }

    /// <inheritdoc />
    public void WriteExternal(IPofWriter writer)
    {
        writer.WriteDictionary(0, m_attributes);
        writer.WriteCollection(1, m_overflowAttrs);
    }

    #endregion

    #region Object methods

    /// <summary>
    /// Test objects for equality.
    /// </summary>
    /// <param name="obj">Object to compare this object with.</param>
    /// <returns>
    /// True if this object and the specified object are equal,
    /// false otherwise.
    /// </returns>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        SessionValue val = obj as SessionValue;
        if (val == null)
        {
            return false;
        }

        return Equals(val.m_attributes, m_attributes)
               && Equals(val.m_overflowAttrs, m_overflowAttrs);
    }

    /// <summary>
    /// Return hash code for this object.
    /// </summary>
    /// <returns>This object's hash code.</returns>
    public override int GetHashCode()
    {
        return m_attributes.GetHashCode() ^ m_overflowAttrs.GetHashCode();
    }

    /// <summary>
    /// Equality operator implementation.
    /// </summary>
    /// <param name="left">Left argument.</param>
    /// <param name="right">Right argument.</param>
    /// <returns>
    /// True if arguments are equal, false otherwise.
    /// </returns>
    public static bool operator ==(SessionValue left, SessionValue right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Inequality operator implementation.
    /// </summary>
    /// <param name="left">Left argument.</param>
    /// <param name="right">Right argument.</param>
    /// <returns>
    /// True if arguments are not equal, false otherwise.
    /// </returns>
    public static bool operator !=(SessionValue left, SessionValue right)
    {
        return !Equals(left, right);
    }

    #endregion

    #region Data members

    /// <summary>
    /// Session attributes except for overflow attributes.
    /// </summary>
    private IDictionary m_attributes;

    /// <summary>
    /// Overflow session attributes.
    /// </summary>
    private ICollection<string> m_overflowAttrs;

    #endregion
}