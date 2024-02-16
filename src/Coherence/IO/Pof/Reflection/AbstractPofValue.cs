/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Tangosol.Util;

namespace Tangosol.IO.Pof.Reflection
{
    /// <summary>
    /// An abstract base class that implements common functionality for all
    /// IPofValue types.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public abstract class AbstractPofValue : IPofValue
    {
        #region Constructors

        /// <summary>
        /// Construct a PofValue instance wrapping the supplied binary.
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
        protected AbstractPofValue(IPofValue valueParent, Binary binValue,
                                   IPofContext ctx, int of, int nType)
        {
            m_valueParent = valueParent;
            m_binValue    = binValue;
            m_ctx         = ctx;
            m_nType       = nType;
            m_of          = of;
        }

        #endregion

        #region Abstract members

        /// <summary>
        /// Locate a child IPofValue contained within this IPofValue.
        /// </summary>
        /// <remarks>
        /// The returned IPofValue could represent a non-existent (null)
        /// value.
        /// </remarks>
        /// <param name="nIndex">
        /// The index of the child value.
        /// </param>
        /// <returns>
        /// The child IPofValue.
        /// </returns>
        /// <exception cref="PofNavigationException">
        /// If this value is a "terminal" or the child value cannot be
        /// located 
        /// for any other reason.
        /// </exception>
        public abstract IPofValue GetChild(int nIndex);
        
        #endregion

        #region Implementation of IPofValue interface

        /// <summary>
        /// Obtain the POF type identifier for this value.
        /// </summary>
        /// <value>
        /// POF type identifier for this value.
        /// </value>
        public virtual int TypeId
        {
            get { return m_nType; }
        }

        /// <summary>
        /// Return the root of the hierarchy this value belongs to.
        /// </summary>
        /// <value>
        /// The root value.
        /// </value>
        public virtual IPofValue Root
        {
            get
            {
                IPofValue value = this;
                while (true)
                {
                    IPofValue valueParent = value.Parent;
                    if (valueParent == null)
                    {
                        return value;
                    }
                    value = valueParent;
                }
            }
        }

        /// <summary>
        /// Return the parent of this value.
        /// </summary>
        /// <value>
        /// The parent value, or <c>null</c> if this is the root value.
        /// </value>
        public virtual IPofValue Parent
        {
            get { return m_valueParent; }
        }

        /// <summary>
        /// Return the deserialized value which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        public virtual object GetValue()
        {
            return GetValue(PofConstants.T_UNKNOWN);
        }

        /// <summary>
        /// Return the deserialized value which this IPofValue represents.
        /// </summary>
        /// <param name="type">
        /// The required type of the returned value or <c>null</c> if the
        /// type is to be inferred from the serialized state.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// If the value is incompatible with the specified type.
        /// </exception>
        public virtual object GetValue(Type type)
        {
            return GetValue(type == null
                ? PofConstants.T_UNKNOWN
                : PofHelper.GetPofTypeId(type, m_ctx));
        }

        /// <summary>
        /// Return the deserialized value which this IPofValue represents.
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
        public virtual object GetValue(int typeId)
        {
            object oValue = m_oValue;
            int valueType = m_nType;

            if (typeId == PofConstants.T_UNKNOWN)
            {
                typeId = valueType;
            }

            if (oValue == ObjectUtils.NO_VALUE || typeId != valueType)
            {
                oValue = new PofValueReader(this).ReadValue(typeId);

                if (typeId == valueType)
                {
                    // cache the retrieved value for the "default" type
                    m_oValue = oValue;
                }
            }

            return oValue;
        }

        /// <summary>
        /// Update this PofValue.
        /// </summary>
        /// <remarks>
        /// The changes made using this method will be immediately reflected
        /// in the result of <see cref="IPofValue.GetValue()"/> method, but
        /// will not be applied to the underlying POF stream until the
        /// <see cref="IPofValue.ApplyChanges"/> method is invoked on the
        /// root IPofValue.
        /// </remarks>
        /// <param name="oValue">
        /// New deserialized value for this IPofValue.
        /// </param>
        public virtual void SetValue(object oValue)
        {
            m_oValue = oValue;
            SetDirty();
        }

        /// <summary>
        /// Apply all the changes that were made to this value and return a 
        /// binary representation of the new value.
        /// </summary>
        /// <remarks>
        /// Any format prefixes and/or decorations that were present in the original
        /// buffer this value orginated from will be preserved.
        /// <p/>
        /// <b>Note:</b> This method can only be called on the root PofValue.
        /// </remarks>
        /// <returns>
        /// New Binary object that contains modified PofValue.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// If called on a non-root PofValue.
        /// </exception>
        public virtual Binary ApplyChanges()
        {
            if (!IsRoot)
            {
                throw new NotSupportedException("ApplyChanges() method can"
                        + " only be invoked on the root IPofValue instance.");
            }

            if (m_arrayRefs != null)
            {
                // TODO: see COH-11347
                throw new NotSupportedException("ApplyChanges() method could not"
                        + " be invoked when Object Identity/Reference is enabled.");
            }

            Binary binOriginal = m_binValue;
            Binary binDelta    = GetChanges();

            return binDelta == null
                   ? binOriginal
                   : new BinaryDeltaCompressor().ApplyDelta(binOriginal, binDelta);
        }

        /// <summary>
        /// Return a binary containing changes made to this IPofValue in the
        /// format defined by the <see cref="BinaryDeltaCompressor"/>.
        /// </summary>
        /// <remarks>
        /// <b>Note:</b> This method can only be called on the root
        /// IPofValue.
        /// </remarks>
        /// <returns>
        /// A binary containing changes made to this IPofValue.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// If called on a non-root IPofValue.
        /// </exception>
        public virtual Binary GetChanges()
        {
            if (!IsRoot)
            {
                throw new NotSupportedException("GetChanges() method can"
                        + " only be invoked on the root IPofValue instance.");
            }

            if (m_cDirty == 0)
            {
                // no changes need to be applied
                return null;
            }
            if (DirtyBytesCount * 100L / Size > REPLACE_THRESHOLD)
            {
                // encode delta in FMT_REPLACE format
                return new ReplacementEncoder(this).Encode();
            }
            // encode delta in FMT_BINDIFF format
            return new BinaryDiffEncoder(this).Encode();
        }

        /// <summary>
        /// Return the <b>Boolean</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Boolean</b> value.
        /// </returns>
        public virtual Boolean GetBoolean()
        {
            return (Boolean) GetValue(PofConstants.T_BOOLEAN);
        }

        /// <summary>
        /// Return the <b>Byte</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Byte</b> value.
        /// </returns>
        public virtual Byte GetByte()
        {
             return (Byte) GetValue(PofConstants.T_OCTET);
        }

        /// <summary>
        /// Return the <b>Char</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Char</b> value.
        /// </returns>
        public virtual Char GetChar()
        {
            return (Char) GetValue(PofConstants.T_CHAR);
        }

        /// <summary>
        /// Return the <b>Int16</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Int16</b> value.
        /// </returns>
        public virtual Int16 GetInt16()
        {
            return (Int16) GetValue(PofConstants.T_INT16);
        }

        /// <summary>
        /// Return the <b>Int32</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Int32</b> value.
        /// </returns>
        public virtual Int32 GetInt32()
        {
            return (Int32) GetValue(PofConstants.T_INT32);
        }

        /// <summary>
        /// Return the <b>Int64</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Int64</b> value.
        /// </returns>
        public virtual Int64 GetInt64()
        {
            return (Int64) GetValue(PofConstants.T_INT64);
        }

        /// <summary>
        /// Return the <b>Single</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Single</b> value.
        /// </returns>
        public virtual Single GetSingle()
        {
            return (Single) GetValue(PofConstants.T_FLOAT32);
        }

        /// <summary>
        /// Return the <b>Double</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Double</b> value.
        /// </returns>
        public virtual Double GetDouble()
        {
            return (Double) GetValue(PofConstants.T_FLOAT64);
        }

        /// <summary>
        /// Return the <b>Boolean[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Boolean[]</b> value.
        /// </returns>
        public virtual Boolean[] GetBooleanArray()
        {
            return (Boolean[]) GetValue(PofConstants.T_ARRAY);
        }

        /// <summary>
        /// Return the <b>Byte[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Byte[]</b> value.
        /// </returns>
        public virtual Byte[] GetByteArray()
        {
            return (Byte[]) GetValue(PofConstants.T_ARRAY);
        }

        /// <summary>
        /// Return the <b>Char[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Char[]</b> value.
        /// </returns>
        public virtual Char[] GetCharArray()
        {
            return (Char[]) GetValue(PofConstants.T_ARRAY);
        }

        /// <summary>
        /// Return the <b>Int16[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Int16[]</b> value.
        /// </returns>
        public virtual Int16[] GetInt16Array()
        {
            return (Int16[]) GetValue(PofConstants.T_ARRAY);
        }

        /// <summary>
        /// Return the <b>Int32[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Int32[]</b> value.
        /// </returns>
        public virtual Int32[] GetInt32Array()
        {
            return (Int32[]) GetValue(PofConstants.T_ARRAY);
        }

        /// <summary>
        /// Return the <b>Int64[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Int64[]</b> value.
        /// </returns>
        public virtual Int64[] GetInt64Array()
        {
            return (Int64[]) GetValue(PofConstants.T_ARRAY);
        }

        /// <summary>
        /// Return the <b>Single[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Single[]</b> value.
        /// </returns>
        public virtual Single[] GetSingleArray()
        {
            return (Single[]) GetValue(PofConstants.T_ARRAY);
        }

        /// <summary>
        /// Return the <b>Double[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Double[]</b> value.
        /// </returns>
        public virtual Double[] GetDoubleArray()
        {
            return (Double[]) GetValue(PofConstants.T_ARRAY);
        }

        /// <summary>
        /// Return the <b>Decimal</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Decimal</b> value.
        /// </returns>
        public virtual Decimal GetDecimal()
        {
            return (Decimal) GetValue(PofConstants.T_DECIMAL32);
        }

        /// <summary>
        /// Return the <b>String</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>String</b> value.
        /// </returns>
        public virtual string GetString()
        {
            return (string) GetValue(PofConstants.T_CHAR_STRING);
        }

        /// <summary>
        /// Return the <b>DateTime</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>DateTime</b> value.
        /// </returns>
        public virtual DateTime GetDateTime()
        {
            return (DateTime) GetValue(PofConstants.T_DATETIME);
        }

        /// <summary>
        /// Return the <b>DateTime</b> which this IPofValue represents.
        /// </summary>
        /// <remarks>
        /// This method will return only the date component. It will ignore the 
        /// time component if present and initialize the time-related fields of
        /// the return value to their default values.
        /// </remarks>
        /// <returns>
        /// The <b>DateTime</b> value.
        /// </returns>
        public virtual DateTime GetDate()
        {
            return (DateTime) GetValue(PofConstants.T_DATE);
        }

        /// <summary>
        /// Return the <b>TimeSpan</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>TimeSpan</b> value.
        /// </returns>
        public virtual TimeSpan GetDayTimeInterval()
        {
            return (TimeSpan) GetValue(PofConstants.T_DAY_TIME_INTERVAL);
        }

        /// <summary>
        /// Return an <b>ICollection</b> of object values which this IPofValue
        /// represents.
        /// </summary>
        /// <param name="coll">
        /// The optional <b>ICollection</b> to use to store the values.
        /// </param>
        /// <returns>
        /// An <b>ICollection</b> of object values.
        /// </returns>
        public virtual ICollection GetCollection(ICollection coll)
        {
            if(coll == null)
            {
                coll = (ICollection) GetValue(PofConstants.T_COLLECTION);    
            }
            else
            {
                CollectionUtils.AddAll(coll, (ICollection) GetValue(PofConstants.T_COLLECTION));    
            }
            return coll;
        }

        /// <summary>
        /// Return an <b>ICollection&lt;T&gt;</b> of object values which this
        /// IPofValue represents.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements in the collection.
        /// </typeparam>
        /// <param name="coll">
        /// The optional <b>ICollection&lt;T&gt;</b> to use to store the values.
        /// </param>
        /// <returns>
        /// An <b>ICollection&lt;T&gt;</b> of object values.
        /// </returns>
        public virtual ICollection<T> GetCollection<T>(ICollection<T> coll)
        {
            var value = (object[]) GetValue(PofConstants.T_COLLECTION);
            if (coll == null)
            {
                coll = new List<T>(value.Length);
            }
            foreach (T entry in value)
            {
                coll.Add(entry);
            }
            return coll;
        }

        /// <summary>
        /// Return an <b>IDictionary</b> of key/value pairs which this
        /// IPofValue represents.
        /// </summary>
        /// <param name="dict">
        /// The optional <b>IDictionary</b> to use to store the key/value pairs.
        /// </param>
        /// <returns>
        /// An <b>IDictionary</b> of key/value pairs.
        /// </returns>
        public virtual IDictionary GetDictionary(IDictionary dict)
        {
            if (dict == null)
            {
                dict = (IDictionary) GetValue(PofConstants.T_MAP);
            }
            else
            {
                foreach (DictionaryEntry entry in (IDictionary) GetValue(PofConstants.T_MAP))
                {
                    dict.Add(entry.Key, entry.Value);
                }
            }
            return dict;
        }

        /// <summary>
        /// Read an <b>IDictionar&lt;TKey, TValue&gt;y</b> of key/value pairs
        /// which this IPofValue represents.
        /// </summary>
        /// <typeparam name="TKey">
        /// The key type of the <b>IDictionary&lt;TKey, TValue&gt;</b>.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The value type of the <b>IDictionary&lt;TKey, TValue&gt;</b>.
        /// </typeparam>
        /// <param name="dict">
        /// The optional <b>IDictionary&lt;TKey, TValue&gt;</b> use to store
        /// the key/value pairs.
        /// </param>
        /// <returns>
        /// An <b>IDictionary&lt;TKey, TValue&gt;</b> of key/value pairs.
        /// </returns>
        public virtual IDictionary<TKey, TValue> GetDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            var value = (IDictionary) GetValue(PofConstants.T_MAP);
            if (dict == null)
            {
                dict = new Dictionary<TKey, TValue>(value.Count);
            }
            foreach (DictionaryEntry entry in value)
            {
                dict.Add((TKey)entry.Key, (TValue)entry.Value);
            }
            return dict;
        }
        #endregion

        #region Public API

        /// <summary>
        /// Return the POF context to use for serialization and
        /// deserialization.
        /// </summary>
        /// <value>
        /// The POF context.
        /// </value>
        public virtual IPofContext PofContext
        {
            get { return m_ctx; }
        }

        /// <summary>
        /// Return the offset of this value from the beginning of POF stream.
        /// </summary>
        /// <value>
        /// The offset of this value from the beginning of POF stream.
        /// </value>
        public virtual int Offset
        {
            get { return m_of; }
        }

        /// <summary>
        /// Return the size of the encoded value in bytes.
        /// </summary>
        /// <value>
        /// The size of the encoded value.
        /// </value>
        public virtual int Size
        {
            get { return m_binValue.Length; }
        }

        /// <summary>
        /// Return <c>true</c> if this value has been modified,
        /// <c>false</c> otherwise.
        /// </summary>
        /// <value>
        /// <c>true</c> if this value has been modified,
        /// <c>false</c> otherwise.
        /// </value>
        public virtual bool IsDirty
        {
            get { return m_fDirty; }
        }

        /// <summary>
        /// Set the dirty flag for this value.
        /// </summary>
        protected virtual void SetDirty()
        {
            if (!IsDirty)
            {
                ((AbstractPofValue) Root).IncrementDirtyValuesCount();
                ((AbstractPofValue) Root).IncrementDirtyBytesCount(Size);
                m_fDirty = true;
            }
        }

        /// <summary>
        /// Return this value's serialized form.
        /// </summary>
        /// <returns>
        /// This value's serialized form.
        /// </returns>
        public virtual Binary GetSerializedValue()
        {
            if (IsDirty)
            {
                var buf    = new BinaryMemoryStream(Size);
                var writer = new PofStreamWriter(new DataWriter(buf), PofContext);
                writer.WriteObject(0, m_oValue);
                buf.Position = 0;

                if (IsUniformEncoded)
                {
                    var reader = new DataReader(buf);

                    // skip type id
                    reader.ReadPackedInt32();
                    var of = (int) buf.Position;
                    return new Binary(buf.GetInternalByteArray(), of, 
                        (int) (buf.Length - of));
                }
                return buf.ToBinary();
            }
            return m_binValue;
        }

        #endregion

        #region Internal API

        /// <summary>
        /// Obtain the registry for identity-reference pairs, creating it if
        /// necessary.
        /// </summary>
        /// <returns>
        /// The identity-reference registry, never <c>null</c>.
        /// </returns>
        protected virtual ILongArray EnsureReferenceRegistry()
        {
            ILongArray array = m_arrayRefs;
            if (array == null)
            {
                var root = (AbstractPofValue) Root;
                m_arrayRefs = array = root == this ? new LongSortedList() : root.EnsureReferenceRegistry();
            }

            return array;
        }

        /// <summary>
        /// Register the passed value with the passed identity.
        /// </summary>
        /// <param name="id">
        /// The identity.
        /// </param>
        /// <param name="value">
        /// The object registerd under the passed identity.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the specified identity is already registered with a different object. 
        /// </exception>
        protected internal void RegisterIdentity(int id, object value)
        {
            if (id >= 0)
            {
                ILongArray map = EnsureReferenceRegistry();
                object     o   = map[id];
                if (o != null && o != value)
                {
                    throw new ArgumentException("duplicate identity: " + id);
                }

                map[id] = value;
            }
        }

        /// <summary>
        /// Look up the specified identity and return the object to which it
        /// refers.
        /// </summary>
        /// <param name="id">
        /// The identity.
        /// </param>
        /// <returns>
        /// The object registered under that identity.
        /// </returns>
        /// <exception cref="IOException">
        /// If the requested identity is not registered.
        /// </exception>
        protected internal IPofValue LookupIdentity(int id)
        {
            ILongArray map = EnsureReferenceRegistry();
            if (!map.Exists(id))
            {
                throw new IOException("missing identity: " + id);
            }

            return (IPofValue) map[id];
        }

        /// <summary>
        /// Return binary representation of this value.
        /// </summary>
        /// <value>
        /// Binary representation of this value.
        /// </value>
        protected virtual Binary BinaryValue
        {
            get { return m_binValue; }
        }

        /// <summary>
        /// Return <c>true</c> if this instance is the root of the IPofValue
        /// hierarchy.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is the root value.
        /// </value>
        protected virtual bool IsRoot
        {
            get { return Parent == null; }
        }

        /// <summary>
        /// Return <c>true</c> if the buffer contains only the value, without
        /// the type identifier.
        /// </summary>
        /// <value>
        /// <c>true</c> if the buffer contains only the value.
        /// </value>
        protected virtual bool IsUniformEncoded
        {
            get { return m_fUniformEncoded; }
        }

        /// <summary>
        /// Specifies that the buffer contains only a value, without a type
        /// identifier.
        /// </summary>
        protected internal virtual void SetUniformEncoded()
        {
            m_fUniformEncoded = true;
        }

        /// <summary>
        /// Get the estimated number of dirty bytes in this POF value
        /// hierarchy.
        /// </summary>
        /// <value>
        /// The number of dirty bytes.
        /// </value>
        protected virtual int DirtyBytesCount
        {
            get { return m_cbDirty; }
        }

        /// <summary>
        /// Increment the counter representing the number of values within
        /// this POF hierarchy that have been modified.
        /// </summary>
        protected virtual void IncrementDirtyValuesCount()
        {
            m_cDirty++;
        }

        /// <summary>
        /// Increment the counter representing the estimated number of bytes
        /// in the original buffer that have been modified.
        /// </summary>
        /// <param name="cb">
        /// The number of bytes to increment counter for.
        /// </param>
        protected virtual void IncrementDirtyBytesCount(int cb)
        {
            m_cbDirty += cb;
        }

        #endregion

        #region PofValueReader inner class

        /// <summary>
        /// PofStreamReader that allows reading of both complete and uniform
        /// encoded values.
        /// </summary>
        class PofValueReader : PofStreamReader
        {
            // ----- constructors -------------------------------------------

            /// <summary>
            /// Construct a PofValueReader instance.
            /// </summary>
            public PofValueReader(AbstractPofValue value)
                : base(new DataReader(value.BinaryValue.GetStream()), value.PofContext)
            {
                m_value = value;
            }


            // ----- properties ---------------------------------------------

            /// <summary>
            /// Containing IPofValue
            /// </summary>
            public AbstractPofValue PofValue
            {
                get { return m_value; }
            }


            // ----- methods ------------------------------------------------

            /// <summary>
            /// Return the deserialized value of this POF value.
            /// </summary>
            /// <returns>
            /// The deserialized value of this POF value.
            /// </returns>
            public virtual object ReadValue()
            {
                return PofValue.IsUniformEncoded
                        ? base.ReadAsObject(PofValue.TypeId)
                        : base.ReadObject(0);
            }

            /// <summary>
            /// Return the deserialized value which this IPofValue
            /// represents.
            /// </summary>
            /// <param name="typeId">
            /// PofType expected as a result.
            /// </param>
            /// <returns>
            /// The deserialized value.
            /// </returns>
            public virtual object ReadValue(int typeId)
            {
                // Prevent promotion of null to an intrinsic default value.
                if (PofValue.TypeId == PofConstants.V_REFERENCE_NULL)
                {
                    return ReadValue();
                }

                if (PofValue.IsUniformEncoded)
                {
                    return base.ReadAsObject(typeId);
                }

                switch (typeId)
                {
                    // Return pof "small" values as the specified type
                    // because the serialized form has lost knowledge of
                    // the original type.
                    case PofConstants.T_INT16:
                        return ReadInt16(0);

                    case PofConstants.T_INT32:
                        return ReadInt32(0);

                    case PofConstants.T_INT64:
                        return ReadInt64(0);

                    case PofConstants.T_FLOAT32:
                        return ReadSingle(0);

                    case PofConstants.T_FLOAT64:
                        return ReadDouble(0);

                    case PofConstants.T_BOOLEAN:
                        return ReadBoolean(0);

                    case PofConstants.T_OCTET:
                        return ReadByte(0);

                    case PofConstants.T_CHAR:
                        return ReadChar(0);

                    case PofConstants.T_UNKNOWN:
                        return ReadValue();

                    case PofConstants.T_DATE:
                        return ReadDate(0);

                    case PofConstants.T_ARRAY:
                    case PofConstants.T_UNIFORM_ARRAY:
                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        object o = ReadValue();
                        if (!o.GetType().IsArray && !(o is LongSortedList))
                        {
                            throw new InvalidCastException(o.GetType().FullName + "is not an array");
                        }
                        return o;

                    default:
                        return PofReflectionHelper.EnsureType(ReadValue(), typeId, PofContext);
                }
            }

            /// <summary>
            /// Look up the specified identity and return the object to which it
            /// refers.
            /// </summary>
            /// <param name="id">
            /// The identity.
            /// </param>
            /// <returns>
            /// The object registered under that identity.
            /// </returns>
            /// <exception cref="IOException">
            /// If the requested identity is not registered.
            /// </exception>
            protected internal override object LookupIdentity(int id)
            {
                object     o     = null;
                ILongArray array = m_referenceMap;
                if (array != null)
                {
                    o = array[id];
                }

                return o == null ? m_value.LookupIdentity(id).GetValue() : o;
            }

            // ----- data members -------------------------------------------

            /// <summary>
            /// Containing IPofValue
            /// </summary>
            private readonly AbstractPofValue m_value;
        }
    
        #endregion

        #region BinaryDiffEncoder inner class 

        class BinaryDiffEncoder : BinaryDeltaCompressor
        {
            // ----- constructors -------------------------------------------

            /// <summary>
            /// Construct a BinaryDiffEncoder instance.
            /// </summary>
            public BinaryDiffEncoder(AbstractPofValue value)
            {
                m_value = value;
            }


            // ----- properties ---------------------------------------------

            /// <summary>
            /// Containing IPofValue
            /// </summary>
            public AbstractPofValue PofValue
            {
                get { return m_value; }
            }


            // ----- methods ------------------------------------------------

            /// <summary>
            /// Encode changes made to this POF value in FMT_BINDIFF delta format, 
            /// as defined by the <see cref="BinaryDeltaCompressor"/> class.
            /// </summary>
            /// <returns>
            /// A binary delta containing the changes that can be applied to
            /// the original buffer to reflect the current state of this 
            /// POF value.
            /// </returns>
            public virtual Binary Encode()
            {
                AbstractPofValue value  = PofValue;
                DataWriter       writer = EnsureWriter(null, 
                                                       value.DirtyBytesCount * 2);
                int              pos    = EncodeValue(writer, value, 0);
                int              cbOld  = value.BinaryValue.Length;
                
                if (pos < cbOld)
                {
                    WriteExtract(writer, 0, pos, cbOld - pos);
                }
                FinalizeDelta(writer);

                return ((BinaryMemoryStream) writer.BaseStream).ToBinary();
            }

            /// <summary>
            /// Encode the changes in the IPofValue hierarchy recursively.
            /// </summary>
            /// <param name="writer">
            /// DataWriter to write changes into.
            /// </param>
            /// <param name="value">
            /// POF value to encode.
            /// </param>
            /// <param name="pos">
            /// Current position in the original POF stream.
            /// </param>
            /// <returns>
            /// Current position in the original POF stream.
            /// </returns>
            protected virtual int EncodeValue(DataWriter writer,
                                              AbstractPofValue value, int pos)
            {
                if (value.IsDirty)
                {
                    int of = value.Offset;
                    if (pos < of)
                    {
                        WriteExtract(writer, 0, pos, of - pos);
                    }
                    byte[] ab = value.GetSerializedValue().ToByteArray();
                    WriteAppend(writer, 0, ab, 0, ab.Length);
                    pos = of + value.Size;
                }
                else if (value is ComplexPofValue)
                {
                    IEnumerator en = ((ComplexPofValue) value).GetChildrenEnumerator();
                    while (en.MoveNext())
                    {
                        var entry = (DictionaryEntry) en.Current;
                        pos = EncodeValue(writer, (AbstractPofValue) entry.Value, pos);
                    }
                }
                // else if SimplePofValue: handled by isDirty block
                return pos;
            }


            // ----- data members -------------------------------------------

            /// <summary>
            /// Containing IPofValue
            /// </summary>
            private readonly AbstractPofValue m_value;
        }

        #endregion

        #region ReplacementEncoder inner class

        class ReplacementEncoder : BinaryDeltaCompressor
        {
            // ----- constructors -------------------------------------------

            /// <summary>
            /// Construct a ReplacementEncoder instance.
            /// </summary>
            public ReplacementEncoder(AbstractPofValue value)
            {
                m_value = value;
            }


            // ----- properties ---------------------------------------------

            /// <summary>
            /// Containing IPofValue
            /// </summary>
            public AbstractPofValue PofValue
            {
                get { return m_value; }
            }


            // ----- methods ------------------------------------------------

            /// <summary>
            /// Encode changes made to this POF value in FMT_REPLACE delta
            /// format, as defined by the <see cref="BinaryDeltaCompressor"/>
            /// class.
            /// </summary>
            /// <returns>
            /// A binary delta containing the changes that can be applied to
            /// the original buffer to reflect the current state of this 
            /// POF value.
            /// </returns>
            public virtual Binary Encode()
            {
                AbstractPofValue value = PofValue;

                var buf    = new BinaryMemoryStream(value.DirtyBytesCount * 2);
                var writer = new DataWriter(buf);
                                                       
                writer.Write(FMT_REPLACE);
                int pos = EncodeValue(writer, value, 0);
                int cbOld = value.BinaryValue.Length;

                if (pos < cbOld)
                {
                    CopyFromOriginal(writer, pos, cbOld - pos);
                }

                return buf.ToBinary();
            }

            /// <summary>
            /// Encode the changes in the IPofValue hierarchy recursively.
            /// </summary>
            /// <param name="writer">
            /// DataWriter to write changes into.
            /// </param>
            /// <param name="value">
            /// POF value to encode.
            /// </param>
            /// <param name="pos">
            /// Current position in the original POF stream.
            /// </param>
            /// <returns>
            /// Current position in the original POF stream.
            /// </returns>
            protected virtual int EncodeValue(DataWriter writer,
                                              AbstractPofValue value, int pos)
            {
                if (value.IsDirty)
                {
                    int of = value.Offset;
                    if (pos < of)
                    {
                        CopyFromOriginal(writer, pos, of - pos);
                    }
                    value.GetSerializedValue().WriteTo(writer);
                    pos = of + value.Size;
                }
                else if (value is ComplexPofValue)
                {
                    IEnumerator en = ((ComplexPofValue) value).GetChildrenEnumerator();
                    while (en.MoveNext())
                    {
                        var entry = (DictionaryEntry) en.Current;
                        pos = EncodeValue(writer, (AbstractPofValue) entry.Value, pos);
                    }
                }
                // else if SimplePofValue: handled by IsDirty block
                return pos;
            }

            /// <summary>
            /// Copy region from the original value into the delta.
            /// </summary>
            /// <param name="writer">
            /// DataWriter to write bytes into.
            /// </param>
            /// <param name="of">
            /// Offset of the region to copy within the original value.
            /// </param>
            /// <param name="cb">
            /// Number of bytes to copy.
            /// </param>
            protected virtual void CopyFromOriginal(DataWriter writer, int of, int cb)
            {
                PofValue.BinaryValue.WriteTo(writer.BaseStream, of, cb);
            }

            // ----- data members -------------------------------------------

            /// <summary>
            /// Containing IPofValue
            /// </summary>
            private readonly AbstractPofValue m_value;
        }

        #endregion

        #region Data members

        /// <summary>
        /// Threshold that determines if the delta generated when applying
        /// changes should be in FMT_REPLACE or FMT_BINDIFF format. If more
        /// than a specified percentage of bytes are "dirty", the FMT_REPLACE
        /// will be used. Otherwise, FMT_BINDIFF format will be used to
        /// capture the changes.
        /// </summary>
        private const int REPLACE_THRESHOLD = 67;

        /// <summary>
        /// Parent value.
        /// </summary>
        private readonly IPofValue m_valueParent;

        /// <summary>
        /// POF context to use for serialization and deserialization.
        /// </summary>
        private readonly IPofContext m_ctx;

        /// <summary>
        /// Lazily-constructed mapping of identities to references.
        /// </summary>
        private ILongArray m_arrayRefs;

        /// <summary>
        /// Binary representation of this value.
        /// </summary>
        private readonly Binary m_binValue;

        /// <summary>
        /// Offset of this value from the beginning of POF stream.
        /// </summary>
        private readonly int m_of;

        /// <summary>
        /// POF type identifer of this value.
        /// </summary>
        protected int m_nType;

        /// <summary>
        /// Deserialized representation of this value.
        /// </summary>
        protected object m_oValue = ObjectUtils.NO_VALUE;

        /// <summary>
        /// True if the this IPofValue represents a uniform value without the
        /// type id; false for a complete POF value that includes the type
        /// id.
        /// </summary>
        private bool m_fUniformEncoded;

        /// <summary>
        /// True iff this value has been changed.
        /// </summary>
        private bool m_fDirty;

        /// <summary>
        /// The number of "dirty" values within this POF hierarchy.
        /// </summary>
        private int m_cDirty;

        /// <summary>
        /// The number of "dirty" bytes within this POF hierarchy.
        /// </summary>
        private int m_cbDirty;

        #endregion
    }
}
