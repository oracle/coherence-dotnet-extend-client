/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

using Tangosol.Util;

namespace Tangosol.IO.Pof.Reflection
{
    /// <summary>
    /// An abstract base class for complex POF types, such as collections, arrays, 
    /// dictionaries and user types.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public abstract class ComplexPofValue : AbstractPofValue
    {
        #region Constructors

        /// <summary>
        /// Construct a ComplexPofValue instance wrapping the supplied binary.
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
        public ComplexPofValue(IPofValue valueParent, Binary binValue,
                              IPofContext ctx, int of, int nType, int ofChildren)
            : base(valueParent, binValue, ctx, of, nType)
        {
            m_aChildren  = new LongSortedList();
            m_ofChildren = ofChildren;
        }
        
        #endregion

        #region Abstract members

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
        protected abstract IPofValue FindChildInternal(int nIndex, int ofStart, int iStart);

        #endregion

        #region Implementation of AbstractPofValue abstract methods

        /// <summary>
        /// Locate a child IPofValue contained within this IPofValue.
        /// </summary>
        /// <remarks>
        /// The returned IPofValue could represent a non-existent (null) value.
        /// </remarks>
        /// <param name="nIndex">
        /// Index of the child value to get.
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
            IPofValue valueChild = (IPofValue) m_aChildren[nIndex];
            if (valueChild == null)
            {
                valueChild = FindChild(nIndex);
                m_aChildren[nIndex] = valueChild;
            }
            return valueChild;
        }

        #endregion

        #region Public members

        /// <summary>
        /// Return an enumerator over all parsed child values.
        /// </summary>
        /// <returns>
        /// A children enumerator.
        /// </returns>
        public IEnumerator GetChildrenEnumerator()
        {
            return m_aChildren.GetEnumerator();    
        }

        #endregion

        #region Internal members

        /// <summary>
        /// Gets or sets element type if this is a uniform collection.
        /// </summary>
        /// <value>
        /// Type of elements if this is a uniform collection; 
        /// <see cref="PofConstants.T_UNKNOWN"/> otherwise.
        /// </value>
        protected int UniformElementType
        {
            get { return m_nElementType; }
            set { m_nElementType = value; }
        }

        /// <summary>
        /// Find the child value with the specified index.
        /// </summary>
        /// <param name="nIndex">
        /// Index of the child value to find.
        /// </param>
        /// <returns>
        /// The child value.
        /// </returns>
        protected IPofValue FindChild(int nIndex)
        {
            int ofStart = m_ofChildren;
            int iStart  = GetLastChildIndex(nIndex);
            if (iStart >= 0)
            {
                AbstractPofValue lastChild = (AbstractPofValue) m_aChildren[iStart];
                ofStart = lastChild.Offset - Offset + lastChild.Size;
                iStart  = iStart + 1;
            }
            else
            {
                iStart  = 0;
            }

            return FindChildInternal(nIndex, ofStart, iStart);
        }

        /// <summary>
        /// Return index of the last parsed child with an index lower than the
        /// specified one.
        /// </summary>
        /// <param name="nIndex">
        /// Index to find the preceding child index for.
        /// </param>
        /// <returns>
        /// Index of the last parsed child, or -1 if one does not exist.
        /// </returns>
        protected int GetLastChildIndex(int nIndex)
        {
            ILongArray aChildren = m_aChildren;
            int        nLast     = (int) aChildren.LastIndex;

            if (nIndex < nLast)
            {
                nLast = nIndex;
                while (nLast >= 0 && !aChildren.Exists(nLast))
                {
                    nLast--;
                }
            }
            return nLast;
        }

        /// <summary>
        /// Return <c>true</c> if this complex value is encoded as one of 
        /// uniform collection types.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is a uniform collection
        /// </value>
        protected bool IsUniformCollection
        {
            get { return m_nElementType != PofConstants.T_UNKNOWN; }
        }

        /// <summary>
        /// Skip a single child value.
        /// </summary>
        /// <param name="reader">
        /// Reader used to read child values.
        /// </param>
        protected void SkipChild(DataReader reader)
        {
            if (IsUniformCollection)
            {
                PofHelper.SkipUniformValue(reader, m_nElementType);
            }
            else
            {
                PofHelper.SkipValue(reader);
            }
        }

        /// <summary>
        /// Extract child IPofValue from this value.
        /// </summary>
        /// <param name="of">
        /// Offset of the child within this value.
        /// </param>
        /// <param name="cb">
        /// Length of the child in bytes.
        /// </param>
        /// <returns>
        /// The child value.
        /// </returns>
        protected IPofValue ExtractChild(int of, int cb)
        {
            return IsUniformCollection
                   ? PofValueParser.ParseUniformValue(this, m_nElementType,
                            BinaryValue.GetBinary(of, cb), PofContext, Offset + of)
                   : PofValueParser.ParseValue(this, 
                            BinaryValue.GetBinary(of, cb), PofContext, Offset + of);
        }

        #endregion

        #region Constants and data members

        /// <summary>
        /// Sparse array of child values.
        /// </summary>
        private readonly ILongArray m_aChildren;

        /// <summary>
        /// Offset of the first child element within this value.
        /// </summary>
        private readonly int m_ofChildren;

        /// <summary>
        /// Type of the child values, if this is a uniform collection.
        /// </summary>
        private int m_nElementType = PofConstants.T_UNKNOWN;

        #endregion
    }
}