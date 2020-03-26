/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using Tangosol.Util;

namespace Tangosol.IO.Pof.Reflection
{
    /// <summary>
    /// PofArray is a <see cref="IPofValue"/> implementation for arrays.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public class PofArray : ComplexPofValue
    {
        #region Constructors

        /// <summary>
        /// Construct a PofArray instance wrapping the supplied binary.
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
        /// <param name="cElements">
        /// The length of this array.
        /// </param>
        public PofArray(IPofValue valueParent, Binary binValue, IPofContext ctx, 
                        int of, int nType, int ofChildren, int cElements)
            : base(valueParent, binValue, ctx, of, nType, ofChildren)
        {
            m_cElements = cElements;
        }
        
        #endregion

        #region Public methods

        /// <summary>
        /// Return the length of this array.
        /// </summary>
        /// <value>
        /// The length of this array.
        /// </value>
        public int Length
        {
            get { return m_cElements; }
        }

        #endregion

        #region Implementation of ComplexPofValue abstract methods

        /// <summary>
        /// Find the child value with the specified index.
        /// </summary>
        /// <param name="nIndex">
        /// Index of the child value to find.
        /// </param>
        /// <param name="ofStart">
        /// Offset within the parent buffer to start search from.
        /// </param>
        /// <param name="iStart">
        /// Index of the child value to start search from.
        /// </param>
        /// <returns>
        /// The child value.
        /// </returns>
        protected override IPofValue FindChildInternal(int nIndex, int ofStart, int iStart)
        {
            BinaryMemoryStream buf = (BinaryMemoryStream) BinaryValue.GetStream();
            buf.Position = ofStart;
            DataReader reader = new DataReader(buf);

            // check array bounds
            if (nIndex < 0 || nIndex >= Length)
                {
                throw new IndexOutOfRangeException(
                        "Element index " + nIndex + " must be in the range [0 .. "
                        + Length + ").");
                }

            // skip children until we find the one we are looking for
            int iProp = iStart;
            while (iProp < nIndex)
                {
                SkipChild(reader);
                iProp++;
                }

            // child found. parse it and return it
            int of = (int) buf.Position;
            SkipChild(reader);
            int cb = (int) (buf.Position - of);

            return ExtractChild(of, cb);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The length of this array.
        /// </summary>
        private readonly int m_cElements;

        #endregion
    }
}