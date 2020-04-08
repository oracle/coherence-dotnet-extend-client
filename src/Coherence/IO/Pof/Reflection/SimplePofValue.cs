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
    /// SimplePofValue represents POF values which do not contain children 
    /// (e.g. numeric values, strings, etc.)
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public class SimplePofValue : AbstractPofValue
    {
        #region Constructors

        /// <summary>
        /// Construct a SimplePofValue instance wrapping the supplied binary.
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
        public SimplePofValue(IPofValue valueParent, Binary binValue,
                              IPofContext ctx, int of, int nType)
            : base(valueParent, binValue, ctx, of, nType)
        {}
        
        #endregion

        #region Implementation of AbstractPofValue abstract methods

        /// <summary>
        /// Locate a child IPofValue contained within this IPofValue.
        /// </summary>
        /// <remarks>
        /// The returned IPofValue could represent a non-existent (null) value.
        /// </remarks>
        /// <param name="nIndex">
        /// Index of the child value.
        /// </param>
        /// <returns>
        /// The the child IPofValue.
        /// </returns>
        /// <exception cref="PofNavigationException">
        /// If this value is a "terminal" or the child value cannot be located 
        /// for any other reason.
        /// </exception>
        public override IPofValue GetChild(int nIndex)
        {
            throw new PofNavigationException("GetChild() method cannot be invoked"
                + " on the SimplePofValue instance.");
        }

        #endregion
    }
}