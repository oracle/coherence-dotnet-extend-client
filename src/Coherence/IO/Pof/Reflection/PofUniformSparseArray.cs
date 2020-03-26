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
    /// PofUniformSparseArray is a <see cref="IPofValue"/> implementation for 
    /// uniform sparse arrays.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public class PofUniformSparseArray : PofSparseArray
    {
        #region Constructors

        /// <summary>
        /// Construct a PofUniformSparseArray instance wrapping the supplied 
        /// binary.
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
        /// <param name="nElementType">
        /// A POF type identifier for this value's elements.
        /// </param>
        public PofUniformSparseArray(IPofValue valueParent, Binary binValue, 
                IPofContext ctx, int of, int nType, int ofChildren, int nElementType)
            : base(valueParent, binValue, ctx, of, nType, ofChildren)
        {
            UniformElementType = nElementType;
        }
        
        #endregion

        #region NilPofValue implementation

        /// <summary>
        /// Instantiate a <see cref="PofSparseArray.NilPofValue"/> (factory method).
        /// </summary>
        /// <param name="of">
        /// Offset this value would be at if it existed.
        /// </param>
        /// <param name="nIndex">
        /// Index of this value within the parent sparse array.
        /// </param>
        /// <returns>
        /// An instance of <see cref="PofSparseArray.NilPofValue"/>.
        /// </returns>
        protected override NilPofValue InstantiateNullValue(int of, int nIndex)
        {
            NilPofValue val = new NilPofValue(this, PofContext, Offset + of,
                                              UniformElementType, nIndex);
            val.SetUniformEncoded();
            return val;
        }

        #endregion
    }
}