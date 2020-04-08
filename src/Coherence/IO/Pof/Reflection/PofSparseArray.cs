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
    /// PofSparseArray is a <see cref="IPofValue"/> implementation for sparse arrays.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public class PofSparseArray : ComplexPofValue
    {
        #region Constructors

        /// <summary>
        /// Construct a PofSparseArray instance wrapping the supplied binary.
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
        public PofSparseArray(IPofValue valueParent, Binary binValue, IPofContext ctx, 
                        int of, int nType, int ofChildren)
            : base(valueParent, binValue, ctx, of, nType, ofChildren)
        {}
        
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

            // skip children until we either find the one we are looking for,
            // or reach the end of the sparse array (index == -1)
            int ofLastIndex = ofStart;
            int iProp       = reader.ReadPackedInt32();
            while (iProp < nIndex && iProp >= 0)
                {
                SkipChild(reader);
                ofLastIndex = (int) buf.Position;
                iProp       = reader.ReadPackedInt32();
                }

            // child found. extract it from the parent buffer
            if (iProp == nIndex)
                {
                int of = (int) buf.Position;
                SkipChild(reader);
                int cb = (int) (buf.Position - of);

                return ExtractChild(of, cb);
                }

            // child not found
            return InstantiateNullValue(ofLastIndex, nIndex);
        }

        #endregion

        #region NilPofValue implementation

        /// <summary>
        /// Instantiate a <see cref="NilPofValue"/> (factory method).
        /// </summary>
        /// <param name="of">
        /// Offset this value would be at if it existed.
        /// </param>
        /// <param name="nIndex">
        /// Index of this value within the parent sparse array.
        /// </param>
        /// <returns>
        /// An instance of <see cref="NilPofValue"/>.
        /// </returns>
        protected virtual NilPofValue InstantiateNullValue(int of, int nIndex)
        {
            return new NilPofValue(this, PofContext, Offset + of,
                                    PofConstants.T_UNKNOWN, nIndex);
        }

        /// <summary>
        /// NilPofValue represents a value that does not exist in the 
        /// original POF stream.
        /// </summary>
        public class NilPofValue : SimplePofValue
        {
            // ----- constructors -------------------------------------------

            /// <summary>
            /// Construct a SimplePofValue instance wrapping the supplied 
            /// binary.
            /// </summary>
            /// <param name="valueParent">
            /// Parent value within the POF stream.
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
            /// <param name="nIndex">
            /// Index of this value within the parent sparse array.
            /// </param>
            public NilPofValue(IPofValue valueParent, IPofContext ctx, 
                                int of, int nType, int nIndex) 
                : base(valueParent, Binary.NO_BINARY, ctx, of, nType)
            {
                m_oValue = null;
                m_nIndex = nIndex;
            }


            // ----- method overrides ------------------------------------

            /// <summary>
            /// Return the deserialized value which this IPofValue
            /// represents.
            /// </summary>
            /// <param name="typeId">
            /// The required Pof type of the returned value or
            /// <see cref="PofConstants.T_UNKNOWN"/> if the type is to be
            /// inferred from the serialized state.
            /// </param>
            /// <returns>
            /// The deserialized value.
            /// </returns>
            /// <exception cref="InvalidCastException">
            /// If the value is incompatible with the specified type.
            /// </exception>
            public override object GetValue(int typeId)
            {
                object value = m_oValue;
                if (value == null)
                {
                    // Return default value for primitives that have been
                    // optimized out of the serialized binary.
                    switch (typeId)
                    {
                        case PofConstants.T_INT16:
                            return ((short) 0);

                        case PofConstants.T_INT32:
                            return 0;

                        case PofConstants.T_INT64:
                            return 0L;

                        case PofConstants.T_FLOAT32:
                            return (float) 0;

                        case PofConstants.T_FLOAT64:
                            return (double) 0;

                        case PofConstants.T_BOOLEAN:
                            return false;

                        case PofConstants.T_OCTET:
                            return (byte) 0;

                        case PofConstants.T_CHAR:
                            return (char) 0;

                        default:
                            return null;
                    }
                }

                return PofReflectionHelper.EnsureType(value, typeId, PofContext);
            }

            /// <summary>
            /// Update this PofValue.
            /// </summary>
            /// <remarks>
            /// The changes made using this method will be immediately reflected 
            /// in the result of <see cref="IPofValue.GetValue()"/> method, but will not be 
            /// applied to the underlying POF stream until the 
            /// <see cref="IPofValue.ApplyChanges"/> method is invoked on the root IPofValue.
            /// </remarks>
            /// <param name="oValue">
            /// New deserialized value for this PofValue.
            /// </param>
            public override void SetValue(object oValue)
            {
                base.SetValue(oValue);
                if (oValue != null)
                {
                    m_nType = PofHelper.GetPofTypeId(oValue.GetType(), PofContext);
                }
            }

            /// <summary>
            /// Return this value's serialized form.
            /// </summary>
            /// <returns>
            /// This value's serialized form.
            /// </returns>
            public override Binary GetSerializedValue()
            {
                Binary bin = base.GetSerializedValue();

                BinaryMemoryStream buf = new BinaryMemoryStream(5 + bin.Length);
                DataWriter writer = new DataWriter(buf);

                writer.WritePackedInt32(m_nIndex);
                bin.WriteTo(writer);

                return buf.ToBinary();
            }


            // ----- data members -------------------------------------------

            /// <summary>
            /// Index of this value within the parent sparse array.
            /// </summary>
            private readonly int m_nIndex;
        }
        #endregion
    }
}