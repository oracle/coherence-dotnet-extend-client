/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.Util;

namespace Tangosol.IO.Pof.Reflection
{
    /// <summary>
    /// A static <see cref="IPofNavigator"/> implementation which uses 
    /// an array of integer indices to navigate the IPofValue hierarchy.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public class SimplePofPath : AbstractPofPath
    {
        #region Constructors

        /// <summary>
        /// Default constructor (necessary for the IPortableObject interface).
        /// </summary>
        public SimplePofPath()
        {
        }

        /// <summary>
        /// Construct a SimplePofPath using a single index as a path.
        /// </summary>
        /// <param name="nIndex">
        /// Index of the child value.
        /// </param>
        public SimplePofPath(int nIndex)
        {
            m_aiElements = new int[] {nIndex};
        }

        /// <summary>
        /// Construct a SimplePofPath using an array of indices as a path.
        /// </summary>
        /// <param name="anIndices">
        /// An array of indices.
        /// </param>
        public SimplePofPath(params int[] anIndices)
        {
            m_aiElements = anIndices;
        }

        #endregion

        #region Implementation of AbstractPofPath abstract methods

        /// <summary>
        /// Return a collection of path elements.
        /// </summary>
        /// <returns>
        /// A collection of path elements.
        /// </returns>
        protected override int[] GetPathElements()
        {
            return m_aiElements;
        }

        #endregion

        #region Implementation of IPortableObject interface

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
        public override void ReadExternal(IPofReader reader)
        {
            m_aiElements = reader.ReadInt32Array(0);
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
        public override void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32Array(0, m_aiElements);
        }

        #endregion

        #region Object methods

        /// <summary>
        /// Compare the SimplePofPath with another object to determine equality.
        /// </summary>
        /// <remarks>
        /// Two SimplePofPath objects are considered equal iff their indices are
        /// equal.
        /// </remarks>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <c>true</c> iff this SimplePofPath and the passed object are equivalent.
        /// </returns>
        public override bool Equals(Object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o is SimplePofPath)
            {
                SimplePofPath that = (SimplePofPath) o;
                return CollectionUtils.EqualsDeep(m_aiElements, that.m_aiElements);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the SimplePofPath object according to the
        /// general <see cref="object.GetHashCode"/> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this SimplePofPath object.
        /// </returns>
        public override int GetHashCode()
        {
            int[] ai = m_aiElements;

            if (ai == null)
            {
                return 0;
            }

            int iHash = 1;
            foreach (int i in ai)
            {
                iHash = 31 * iHash + i;
            }
            return iHash;
        }

        /// <summary>
        /// Return a human-readable description for this SimplePofPath.
        /// </summary>
        /// <returns>
        /// A String description of the SimplePofPath.
        /// </returns>
        public override String ToString()
        {
            return GetType().Name +
                   "(indices=" + m_aiElements + ')';
        }

        #endregion

        #region Data members

        /// <summary>
        /// Path elements.
        /// </summary>
        private int[] m_aiElements;

        #endregion
    }
}