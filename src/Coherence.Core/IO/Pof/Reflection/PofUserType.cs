/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Util;

namespace Tangosol.IO.Pof.Reflection
{
    /// <summary>
    /// PofUserType is a <see cref="IPofValue"/> implementation for user types.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public class PofUserType : PofSparseArray
    {
        #region Constructors

        /// <summary>
        /// Construct a PofUserType instance wrapping the supplied binary.
        /// </summary>
        /// <param name="valueParent">
        /// Parent value within the POF stream.
        /// </param>
        /// <param name="binValue">
        /// Binary representation of this value.
        /// </param>
        /// <param name="ctx">
        /// POF context to use when reading or writing properties.
        /// </param>
        /// <param name="of">
        /// Offset of this value from the beginning of POF stream.
        /// </param>
        /// <param name="nType">
        /// POF type identifier for this value.
        /// </param>
        /// <param name="ofChildren">
        /// Offset of the first child element within this value.
        /// </param>
        /// <param name="nVersion">
        /// Data version of this value.
        /// </param>
        public PofUserType(IPofValue valueParent, Binary binValue, 
                IPofContext ctx, int of, int nType, int ofChildren, int nVersion)
            : base(valueParent, binValue, ctx, of, nType, ofChildren)
        {
            m_nVersion = nVersion;
        }
        
        #endregion

        #region Public members

        /// <summary>
        /// Return the data version of this value.
        /// </summary>
        /// <value>
        /// The data version of this value.
        /// </value>
        public virtual int Version
        {
            get { return m_nVersion; }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The data version of this value.
        /// </summary>
        private int m_nVersion;

        #endregion
    }
}