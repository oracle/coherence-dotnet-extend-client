/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.IO;
using Tangosol.IO.Pof;
using Tangosol.IO.Pof.Reflection;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// POF-based <see cref="IValueUpdater"/> implementation.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.02.14</author>
    /// <author>Ivan Cikic  2009.04.01</author>
    [Serializable]
    public class PofUpdater : IValueUpdater, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PofUpdater()
        {}

        /// <summary>
        /// Constructs a <b>PofUpdater</b> based on a property index.
        /// </summary>
        /// <remarks>
        /// This constructor is equivalent to:
        /// <code>
        /// PofExtractor extractor = 
        ///     new PofUpdater(new SimplePofPath(index));
        /// </code>
        /// </remarks>
        /// <param name="index">
        /// Property index.
        /// </param>
        public PofUpdater(int index) 
            : this(new SimplePofPath(index))
        {}

        /// <summary>
        /// Constructs a <b>PofUpdater</b> based on a POF navigator.
        /// </summary>
        /// <param name="navigator">
        /// POF navigator.
        /// </param>
        public PofUpdater(IPofNavigator navigator)
        {
            m_navigator = navigator;
        }

        #endregion

        #region IValueUpdater interface

        /// <summary>
        /// Update the passed target object using the specified value.
        /// </summary>
        /// <remarks>
        /// This method will always throw a <see cref="NotSupportedException"/>
        /// if called directly by the .NET client application, as its execution
        /// is only meaningful within the cluster.
        /// <p/>
        /// It is expected that this extractor will only be used against 
        /// POF-encoded binary entries within a remote partitioned cache.
        /// </remarks>
        /// <param name="oTarget">
        /// The object to update.
        /// </param>
        /// <param name="oValue">
        /// The new value to update the target's property with.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// Always, as it is expected that this extractor will only be 
        /// executed within the cluster.
        /// </exception>
        public void Update(object oTarget, object oValue)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Object methods

        /// <summary>
        /// Compare the PofUpdater with another object to determine
        /// equality.
        /// </summary>
        /// <param name="o">
        /// Object to compare with
        /// </param>
        /// <returns>
        /// <b>true</b> iff this PofUpdater and the passed object are
        /// equivalent
        /// </returns>
        public override bool Equals(Object o)
        {
            if (o is PofUpdater)
            {
                PofUpdater that = (PofUpdater)o;
                return Equals(m_navigator, that.m_navigator);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the PofUpdater object.
        /// </summary>
        /// <returns>
        /// An integer hash value for this PofUpdater object
        /// </returns>
        public override int GetHashCode()
        {
            return m_navigator.GetHashCode();
        }

        /// <summary>
        /// Return a human-readable description for this PofUpdater.
        /// </summary>
        /// <returns>
        /// String description of the PofUpdater
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + "(navigator=" + m_navigator + ")";
        }

        #endregion

        #region IPortableObject implementation

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
            m_navigator = (IPofNavigator)reader.ReadObject(0);
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
            writer.WriteObject(0, m_navigator);
        }

        #endregion

        #region Data members

        /// <summary>
        /// POF navigator.
        /// </summary>
        private IPofNavigator m_navigator;

        #endregion
    }
}