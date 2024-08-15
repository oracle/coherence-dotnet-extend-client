/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Web;

/// <summary>
/// Overflow attribute  key.
/// </summary>
/// <remarks>
/// Overflow attribute key is a combination of session key and attribute name
/// </remarks>
/// <author>Vaso Putica  2024.06.06</author>
public class OverflowAttributeKey
    : IPortableObject, IKeyAssociation
{
    #region Constructors

    /// <summary>
    /// Default constructor.
    /// </summary>
    public OverflowAttributeKey()
    {
    }

    /// <summary>
    /// Construct OverflowAttributeKey instance.
    /// </summary>
    /// <param name="sessionKey">
    /// The session key.
    /// </param>
    /// <param name="attributeName">
    /// The attribute name.
    /// </param>
    public OverflowAttributeKey(SessionKey sessionKey, string attributeName)
    {
        m_sessionKey    = sessionKey;
        m_attributeName = attributeName;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Session key.
    /// </summary>
    public SessionKey SessionKey => m_sessionKey;

    /// <summary>
    /// Attribute name.
    /// </summary>
    public string AttributeName => m_attributeName;

    #endregion

    #region Implementation of IKeyAssociation

    /// <summary>
    /// Determine the key object to which this key object is associated.
    /// </summary>
    /// <remarks>
    /// The key object returned by this method is often referred to as a
    ///  <i>host key</i>.
    /// </remarks>
    /// <value>
    /// The host key that for this key object, or <c>null</c> if this key
    /// has no association.
    /// </value>
    public object AssociatedKey => m_sessionKey;

    #endregion

    #region Implementation of IPortableObject

    /// <summary>
    /// Restore the contents of a user type instance by reading its state
    /// using the specified <see cref="IPofReader"/> object.
    /// </summary>
    /// <param name="reader">
    /// The <b>IPofReader</b> from which to read the object's state.
    /// </param>
    /// <exception cref="IOException">
    /// If an I/O error occurs.
    /// </exception>
    public void ReadExternal(IPofReader reader)
    {
        m_sessionKey    = (SessionKey)reader.ReadObject(0);
        m_attributeName = reader.ReadString(1);
    }

    /// <summary>
    /// Save the contents of a POF user type instance by writing its
    /// state using the specified <see cref="IPofWriter"/> object.
    /// </summary>
    /// <param name="writer">
    /// The <b>IPofWriter</b> to which to write the object's state.
    /// </param>
    /// <exception cref="IOException">
    /// If an I/O error occurs.
    /// </exception>
    public void WriteExternal(IPofWriter writer)
    {
        writer.WriteObject(0, m_sessionKey);
        writer.WriteString(1, m_attributeName);
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

        OverflowAttributeKey that = obj as OverflowAttributeKey;
        if (obj == null)
        {
            return false;
        }

        return Equals(that.m_sessionKey, m_sessionKey)
               && Equals(that.m_attributeName, m_attributeName);
    }

    /// <summary>
    /// Return hash code for this object.
    /// </summary>
    /// <returns>This object's hash code.</returns>
    public override int GetHashCode()
    {
        return m_sessionKey.GetHashCode() ^ m_attributeName.GetHashCode();
    }

    /// <summary>
    /// Equality operator implementation.
    /// </summary>
    /// <param name="left">Left argument.</param>
    /// <param name="right">Right argument.</param>
    /// <returns>
    /// True if arguments are equal, false otherwise.
    /// </returns>
    public static bool operator ==(OverflowAttributeKey left, OverflowAttributeKey right)
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
    public static bool operator !=(OverflowAttributeKey left, OverflowAttributeKey right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Return string representation of this object.
    /// </summary>
    /// <returns>
    /// String representation of this object.
    /// </returns>
    public override string ToString()
    {
        return m_sessionKey + ":" + m_attributeName;
    }

    #endregion

    #region Data members

    /// <summary>
    /// Session key.
    /// </summary>
    private SessionKey m_sessionKey;

    /// <summary>
    /// Attribute name.
    /// </summary>
    private string m_attributeName;

    #endregion
}