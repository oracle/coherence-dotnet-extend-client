/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util
{
    /// <summary>
    /// Key class that consists of a primary and secondary component. Two instances
    /// of CompositeKey are considered to be equal iff both the primary and
    /// secondary components of the two instances are considered equal.
    /// Additionally, the hash code of a CompositeKey takes into the consideration
    /// the hash codes of its two components. Finally, the CompositeKey class
    /// implements KeyAssociation by returning the primary component.
    /// </summary>
    /// <author>jh  2008.12.11</author>
    /// <author>wl 2010.08.29</author>
    public class CompositeKey : IPortableObject, IKeyAssociation
    {
        #region Properties

        /// <summary>
        /// The primary key component.
        /// </summary>
        public Object PrimaryKey
        {
            get { return m_primary; }
        }

        /// <summary>
        /// The secondary key component.
        /// </summary>
        public Object SecondaryKey
        {
            get { return m_secondary; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CompositeKey()
        {
        }

        /// <summary>
        /// Create a new CompositeKey that consists of the given primary and
        /// secondary components.
        /// </summary>
        /// <param name="primary">
        /// the primary key component; must not be null. This is
        /// also the host key returned by the KeyAssociation implementation
        /// </param>
        /// <param name="secondary">
        /// the secondary key component; must not be null
        /// </param>
        public CompositeKey(Object primary, Object secondary)
        {
            if (primary == null || secondary == null)
            {
                throw new ArgumentException();
            }

            m_primary   = primary;
            m_secondary = secondary;
        }

        #endregion

        #region IKeyAssociation implementation

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
        public Object AssociatedKey
        {
            get { return m_primary; }
        }

        #endregion

        #region PortableObject implementation

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
            m_primary   = reader.ReadObject(0);
            m_secondary = reader.ReadObject(1);
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
            writer.WriteObject(0, m_primary);
            writer.WriteObject(1, m_secondary);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Determine a hash value for the <b>CompositeKey</b> object.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>CompositeKey</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_primary.GetHashCode() ^ m_secondary.GetHashCode();
        }

        /// <summary>
        /// Compare this object with another object to determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with current object.
        /// </param>
        /// <returns>
        /// <b>true</b> if this object and the passed object are equivalent
        /// <b>CompositeKey</b> objects.
        /// </returns>
        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }

            if (o is CompositeKey)
            {
                CompositeKey that = (CompositeKey) o;
                return m_primary.Equals(that.m_primary) &&
                   m_secondary.Equals(that.m_secondary);
            }

            return false;
        }

        /// <summary>
        /// Return a human-readable description for this <b>CompositeKey</b>.
        /// </summary>
        /// <returns>
        /// A string description of the object.
        /// </returns>
        public override string ToString()
        {
            return m_primary + ":" + m_secondary;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The primary key component.
        /// </summary>
        private Object m_primary;

        /// <summary>
        /// The secondary key component.
        /// </summary>
        private Object m_secondary;

        #endregion
    }
}
