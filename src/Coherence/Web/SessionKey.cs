/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Web
{
    /// <summary>
    /// Session key.
    /// </summary>
    /// <remarks>
    /// Session key is a combination of user-configurable application 
    /// identifier and system-generated session identifier.
    /// </remarks>
    /// <author>Aleksandar Seovic  2009.09.21</author>
    public class SessionKey : IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Deserialization constructor (for internal use only).
        /// </summary>
        public SessionKey()
        {}

        /// <summary>
        /// Initializes a new instance of the SessionKey class.
        /// </summary>
        /// <param name="applicationId">
        /// The application ID.
        /// </param>
        /// <param name="sessionId">
        /// The session ID.
        /// </param>
        public SessionKey(string applicationId, string sessionId)
        {
            if (applicationId == null)
            {
                throw new ArgumentNullException("applicationId");
            }
            if (sessionId == null)
            {
                throw new ArgumentNullException("sessionId");
            }

            m_applicationId = applicationId;
            m_sessionId     = sessionId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Application identifier.
        /// </summary>
        public string ApplicationId
        {
            get { return m_applicationId; }
        }

        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId
        {
            get { return m_sessionId; }
        }

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
            m_applicationId = reader.ReadString(0);
            m_sessionId     = reader.ReadString(1);
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
            writer.WriteString(0, m_applicationId);
            writer.WriteString(1, m_sessionId);
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
            
            SessionKey key = obj as SessionKey;
            if (key == null)
            {
                return false;
            }
                
            return Equals(key.m_applicationId, m_applicationId)
                   && Equals(key.m_sessionId, m_sessionId);
        }

        /// <summary>
        /// Return hash code for this object.
        /// </summary>
        /// <returns>This object's hash code.</returns>
        public override int GetHashCode()
        {
            return m_applicationId.GetHashCode() ^ m_sessionId.GetHashCode();
        }

        /// <summary>
        /// Equality operator implementation.
        /// </summary>
        /// <param name="left">Left argument.</param>
        /// <param name="right">Right argument.</param>
        /// <returns>
        /// True if arguments are equal, false otherwise.
        /// </returns>
        public static bool operator ==(SessionKey left, SessionKey right)
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
        public static bool operator !=(SessionKey left, SessionKey right)
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
            return m_applicationId + ":" + m_sessionId;
        }

        #endregion

        #region Data members

        /// <summary>
        /// Application identifier.
        /// </summary>
        private String m_applicationId;

        /// <summary>
        /// Session identifier.
        /// </summary>
        private String m_sessionId;

        #endregion
    }
}