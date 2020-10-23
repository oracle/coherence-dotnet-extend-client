/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;

using Tangosol.Util;

namespace Tangosol.IO.Pof.Reflection
{
    /// <summary>
    /// IPofValue represents the POF data structure in a POF stream, or any
    /// sub-structure or value thereof.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public interface IPofValue
    {
        /// <summary>
        /// Obtain the POF type identifier for this value.
        /// </summary>
        /// <value>
        /// POF type identifier for this value.
        /// </value>
        int TypeId { get; }

        /// <summary>
        /// Return the root of the hierarchy this value belongs to.
        /// </summary>
        /// <value>
        /// The root value.
        /// </value>
        IPofValue Root { get; }

        /// <summary>
        /// Return the parent of this value.
        /// </summary>
        /// <value>
        /// The parent value, or <c>null</c> if this is the root value.
        /// </value>
        IPofValue Parent { get; }

        /// <summary>
        /// Locate a child IPofValue contained within this IPofValue.
        /// </summary>
        /// <remarks>
        /// The returned IPofValue could represent a non-existent (null)
        /// value.
        /// </remarks>
        /// <param name="nIndex">
        /// Index of the child value to get.
        /// </param>
        /// <returns>
        /// The the child IPofValue.
        /// </returns>
        /// <exception cref="PofNavigationException">
        /// If this value is a "terminal" or the child value cannot be
        /// located for any other reason.
        /// </exception>
        IPofValue GetChild(int nIndex);

        /// <summary>
        /// Return the deserialized value which this IPofValue represents.
        /// </summary>
        /// <remarks>
        /// For primitive types such as int or bool, the POF type is not
        /// stored in the POF stream. Therefore, for primitive types, the POF
        /// type or .NET type must be explicitly specified via
        /// <see cref="GetValue(int)"/> or <see cref="GetValue(Type)"/>.
        /// </remarks>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        object GetValue();

        /// <summary>
        /// Return the deserialized value which this IPofValue represents.
        /// </summary>
        /// <remarks>
        /// For primitive types such as int or bool, the POF type is not
        /// stored in the POF stream. Therefore, for primitive types,
        /// the type parameter must be non-null.
        /// </remarks>
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
        object GetValue(Type type);

        /// <summary>
        /// Return the deserialized value which this IPofValue represents.
        /// </summary>
        /// <remarks>
        /// For primitive types such as int or bool, the POF type is not
        /// stored in the POF stream. Therefore, for primitive types, the type
        /// must be explicitly specified with the typeId parameter.
        /// </remarks>
        /// <param name="typeId">
        /// The required POF type of the returned value or
        /// <see cref="PofConstants.T_UNKNOWN"/> if the type is to be
        /// inferred from the serialized state.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// If the value is incompatible with the specified type.
        /// </exception>
        object GetValue(int typeId);

        /// <summary>
        /// Update this IPofValue.
        /// </summary>
        /// <remarks>
        /// The changes made using this method will be immediately reflected
        /// in the result of <see cref="GetValue()"/> method, but will not be
        /// applied to the underlying POF stream until the
        /// <see cref="ApplyChanges"/> method is invoked on the root
        /// IPofValue.
        /// </remarks>
        /// <param name="oValue">
        /// New deserialized value for this IPofValue.
        /// </param>
        void SetValue(object oValue);

        /// <summary>
        /// Apply all the changes that were made to this value and return a
        /// binary representation of the new value.
        /// </summary>
        /// <remarks>
        /// Any format prefixes and/or decorations that were present in the
        /// original buffer this value orginated from will be preserved.
        /// <p/>
        /// <b>Note:</b> This method can only be called on the root
        /// IPofValue.
        /// </remarks>
        /// <returns>
        /// New Binary object that contains modified IPofValue.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// If called on a non-root IPofValue.
        /// </exception>
        Binary ApplyChanges();

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
        Binary GetChanges();

        /// <summary>
        /// Return the <b>Boolean</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Boolean</b> value.
        /// </returns>
        Boolean GetBoolean();

        /// <summary>
        /// Return the <b>Byte</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Byte</b> value.
        /// </returns>
        Byte GetByte();

        /// <summary>
        /// Return the <b>Char</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Char</b> value.
        /// </returns>
        Char GetChar();

        /// <summary>
        /// Return the <b>Int16</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Int16</b> value.
        /// </returns>
        Int16 GetInt16();

        /// <summary>
        /// Return the <b>Int32</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Int32</b> value.
        /// </returns>
        Int32 GetInt32();

        /// <summary>
        /// Return the <b>Int64</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Int64</b> value.
        /// </returns>
        Int64 GetInt64();

        /// <summary>
        /// Return the <b>Single</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Single</b> value.
        /// </returns>
        Single GetSingle();

        /// <summary>
        /// Return the <b>Double</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Double</b> value.
        /// </returns>
        Double GetDouble();

        /// <summary>
        /// Return the <b>Boolean[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Boolean[]</b> value.
        /// </returns>
        Boolean[] GetBooleanArray();

        /// <summary>
        /// Return the <b>Byte[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Byte[]</b> value.
        /// </returns>
        Byte[] GetByteArray();

        /// <summary>
        /// Return the <b>Char[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Char[]</b> value.
        /// </returns>
        Char[] GetCharArray();

        /// <summary>
        /// Return the <b>Int16[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Int16[]</b> value.
        /// </returns>
        Int16[] GetInt16Array();

        /// <summary>
        /// Return the <b>Int32[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Int32[]</b> value.
        /// </returns>
        Int32[] GetInt32Array();

        /// <summary>
        /// Return the <b>Int64[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Int64[]</b> value.
        /// </returns>
        Int64[] GetInt64Array();

        /// <summary>
        /// Return the <b>Single[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Single[]</b> value.
        /// </returns>
        Single[] GetSingleArray();

        /// <summary>
        /// Return the <b>Double[]</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Double[]</b> value.
        /// </returns>
        Double[] GetDoubleArray();

        /// <summary>
        /// Return the <b>Decimal</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>Decimal</b> value.
        /// </returns>
        Decimal GetDecimal();

        /// <summary>
        /// Return the <b>String</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>String</b> value.
        /// </returns>
        string GetString();

        /// <summary>
        /// Return the <b>DateTime</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>DateTime</b> value.
        /// </returns>
        DateTime GetDateTime();

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
        DateTime GetDate();

        /// <summary>
        /// Return the <b>TimeSpan</b> which this IPofValue represents.
        /// </summary>
        /// <returns>
        /// The <b>TimeSpan</b> value.
        /// </returns>
        TimeSpan GetDayTimeInterval();

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
        ICollection GetCollection(ICollection coll);

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
        ICollection<T> GetCollection<T>(ICollection<T> coll);

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
        IDictionary GetDictionary(IDictionary dict);

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
        IDictionary<TKey, TValue> GetDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict);
    }
}