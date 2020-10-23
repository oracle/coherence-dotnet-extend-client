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
    /// Parses POF-encoded binary and returns an instance of a 
    /// <see cref="IPofValue"/> wrapper for it.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public abstract class PofValueParser
    {
        #region Public API

        /// <summary>
        /// Parses POF-encoded binary and returns an instance of a 
        /// <see cref="IPofValue"/> wrapper for it.
        /// </summary>
        /// <param name="binValue">
        /// POF-encoded binary value.
        /// </param>
        /// <param name="ctx">
        /// POF context to use.
        /// </param>
        /// <returns>
        /// An IPofValue instance.
        /// </returns>
        public static IPofValue Parse(Binary binValue, IPofContext ctx)
        {
            return ParseValue(null, binValue, ctx, 0);
        }

        #endregion

        #region Internal API

        /// <summary>
        /// Parses POF-encoded binary and returns an instance of a 
        /// <see cref="IPofValue"/> wrapper for it.
        /// </summary>
        /// <param name="valueParent">
        /// Parent POF value.
        /// </param>
        /// <param name="binValue">
        /// POF-encoded binary value.
        /// </param>
        /// <param name="ctx">
        /// POF context to use.
        /// </param>
        /// <param name="of">
        /// Offset of the parsed value from the beginning of the POF stream.
        /// </param>
        /// <returns>
        /// An IPofValue instance.
        /// </returns>
        internal static IPofValue ParseValue(IPofValue valueParent, Binary binValue, 
            IPofContext ctx, int of)
        {
            DataReader reader = binValue.GetReader();
            int        nType  = reader.ReadPackedInt32();

            return InstantiatePofValue(valueParent, nType, binValue, ctx, of, reader);
        }

        /// <summary>
        /// Parses POF-encoded binary and returns an instance of a 
        /// <see cref="IPofValue"/> wrapper for it.
        /// </summary>
        /// <param name="valueParent">
        /// Parent POF value.
        /// </param>
        /// <param name="nType">
        /// Type identifier of this POF value.
        /// </param>
        /// <param name="binValue">
        /// POF-encoded binary value.
        /// </param>
        /// <param name="ctx">
        /// POF context to use.
        /// </param>
        /// <param name="of">
        /// Offset of the parsed value from the beginning of the POF stream.
        /// </param>
        /// <returns>
        /// An IPofValue instance.
        /// </returns>
        internal static IPofValue ParseUniformValue(IPofValue valueParent, 
            int nType, Binary binValue, IPofContext ctx, int of)
        {
            AbstractPofValue val = InstantiatePofValue(valueParent, nType, 
                binValue, ctx, of, binValue.GetReader());
            val.SetUniformEncoded();
            return val;
        }

		/// <summary>
		/// Creates a PofValue instance.
		/// </summary>
		/// <param name="valueParent">
		/// Parent POF value.
		/// </param>
		/// <param name="nType">
		/// Type identifier of this POF value.
		/// </param>
		/// <param name="binValue">
		/// POF-encoded binary value without the type identifier.
		/// </param>
		/// <param name="ctx">
		/// POF context to use.
		/// </param>
		/// <param name="of">
		/// Offset of the parsed value from the beginning of the POF stream.
		/// </param>
		/// <param name="reader">
		/// <see cref="DataReader"/> to read the value from.
		/// </param>
		/// <returns>
		/// A <see cref="IPofValue"/> instance.
		/// </returns>
        protected static AbstractPofValue InstantiatePofValue(
                IPofValue valueParent, int nType, Binary binValue, 
                IPofContext ctx, int of, DataReader reader)
        {
            int         cSize;
            int         nElementType;
            int         nId;
            int         ofChildren;
            PofUserType value;
            IPofValue   valueRef;

            switch (nType)
                {
                case PofConstants.T_ARRAY:
                    cSize      = reader.ReadPackedInt32();
                    ofChildren = (int) reader.BaseStream.Position;
                    return new PofArray(valueParent, binValue, ctx, of, nType,
                            ofChildren, cSize);

                case PofConstants.T_UNIFORM_ARRAY:
                    nElementType = reader.ReadPackedInt32();
                    cSize        = reader.ReadPackedInt32();
                    ofChildren   = (int) reader.BaseStream.Position;
                    return new PofUniformArray(valueParent, binValue, ctx, of,
                            nType, ofChildren, cSize, nElementType);

                case PofConstants.T_COLLECTION:
                    cSize = reader.ReadPackedInt32();
                    ofChildren = (int) reader.BaseStream.Position;
                    return new PofCollection(valueParent, binValue, ctx, of,
                            nType, ofChildren, cSize);

                case PofConstants.T_UNIFORM_COLLECTION:
                    nElementType = reader.ReadPackedInt32();
                    cSize        = reader.ReadPackedInt32();
                    ofChildren   = (int) reader.BaseStream.Position;
                    return new PofUniformCollection(valueParent, binValue, ctx,
                            of, nType, ofChildren, cSize, nElementType);

                case PofConstants.T_SPARSE_ARRAY:
                                   reader.ReadPackedInt32(); // skip size
                    ofChildren = (int) reader.BaseStream.Position;
                    return new PofSparseArray(valueParent, binValue, ctx, of,
                            nType, ofChildren);

                case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                    nElementType = reader.ReadPackedInt32();
                                   reader.ReadPackedInt32(); // skip size
                    ofChildren   = (int) reader.BaseStream.Position;
                    return new PofUniformSparseArray(valueParent, binValue, ctx,
                            of, nType, ofChildren, nElementType);

                case PofConstants.T_REFERENCE:
                    nId        = reader.ReadPackedInt32();
                    ofChildren = (int) reader.BaseStream.Position;
                    value      = new PofUserType(valueParent, binValue, ctx, of,
                            nType, ofChildren, 0);
                    valueRef = value.LookupIdentity(nId);
                    if (valueRef != null)
                        {
                        value.SetValue(valueRef.GetValue());
                        }
                    return value;

                default:
                    nId = -1;
                    if (nType == PofConstants.T_IDENTITY)
                    {
                        nId   = reader.ReadPackedInt32();
                        nType = reader.ReadPackedInt32();
                        if (valueParent == null)
                        {
                            of = (int) reader.BaseStream.Position;
                        }
                    }

                    if (nType >= 0)
                    {
                        int nVersionId = reader.ReadPackedInt32();
                        ofChildren     = (int) reader.BaseStream.Position;

                        value = new PofUserType(valueParent, binValue, ctx, of,
                                nType, ofChildren, nVersionId);
                        if (nId > -1)
                        {
                            value.RegisterIdentity(nId, value);
                        }
                        return value;
                    }  
                    return new SimplePofValue(valueParent, binValue, ctx, of, nType);
                }
        }

        #endregion
    }
}