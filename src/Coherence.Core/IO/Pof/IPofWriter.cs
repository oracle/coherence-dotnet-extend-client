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

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// The <b>IPofWriter</b> interface provides the capability of writing a set of
    /// non-primitive .NET types ("user types") to a POF stream as an ordered
    /// sequence of indexed properties.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The serialized format of a POF user type is as follows:
    /// <list type="bullet">
    /// <item><description>Type Identifier</description></item>
    /// <item><description>Version Identifier</description></item>
    /// <item><description>[Property Index, Property Value]*</description></item>
    /// <item><description>-1</description></item>
    /// </list></p>
    /// <p>
    /// The type identifier is an integer value greater than or equal to zero
    /// that identifies the non-primitive .NET type. The type identifier has
    /// no explicit or self-describing meaning within the POF stream itself;
    /// in other words, the type identifier does not contain the actual class
    /// definition. Instead, the <b>IPofWriter</b> and corresponding
    /// <see cref="IPofReader"/> share an <see cref="IPofContext"/> which
    /// contains the necessary meta-data, including type identifier to .NET
    /// type mappings.</p>
    /// <p>
    /// The version identifier is used to support both backwards and forwards
    /// compatibility of serialized POF user types. Versioning of user types
    /// allows the addition of new properties to a user type, but not the
    /// replacement or removal of properties that existed in a previous
    /// version of the user type.</p>
    /// <p>
    /// When a version <i>v1</i> of a user type written by a
    /// <b>IPofWriter</b> is read by an <b>IPofReader</b> that supports
    /// version <i>v2</i> of the same user type, the <b>IPofReader</b>
    /// returns default values for the additional properties of the User Type
    /// that exist in <i>v2</i> but do not exist in <i>v1</i>. Conversely,
    /// when a version <i>v2</i> of a user type written by a
    /// <b>IPofWriter</b> is read by an <b>IPofReader</b> that supports
    /// version <i>v1</i> of the same user type, the instance of user type
    /// <i>v1</i> must store those additional opaque properties for later
    /// encoding. The <b>IPofReader</b> enables the user type to store off
    /// the opaque properties in binary form (see
    /// <see cref="IPofReader.ReadRemainder"/>).</p>
    /// <p>
    /// When the user type is re-encoded, it must be done so using the
    /// version identifier <i>v2</i>, since it is including the unaltered
    /// <i>v2</i> properties. The opaque properties are subsequently
    /// included in the POF stream using the <see cref="WriteRemainder"/>
    /// method.</p>
    /// <p>
    /// Following the version identifier is an ordered sequence of
    /// index/value pairs, each of which is composed of a property index
    /// encoded as non-negative integer value whose value is greater than the
    /// previous property index, and a property value encoded as a POF value.
    /// The user type is finally terminated with an illegal property index
    /// of -1.</p>
    /// <p>
    /// Note: To read a property that was written using a <b>IPofWriter</b>
    /// method, the corresponding read method on <see cref="IPofReader"/>
    /// must be used. For example, if a property was written using
    /// <see cref="WriteByteArray"/>, <see cref="IPofReader.ReadByteArray"/>
    /// must be used to read the property.</p>
    /// </remarks>
    /// <author>Cameron Purdy, Jason Howes  2006.07.13</author>
    /// <author>Aleksandar Seovic  2006.08.08</author>
    /// <seealso cref="IPofContext"/>
    /// <seealso cref="IPofReader"/>
    /// <since>Coherence 3.2</since>
    public interface IPofWriter
    {
        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IPofContext"/> object used by this
        /// <b>IPofWriter</b> to serialize user types into a POF stream.
        /// </summary>
        /// <remarks>
        /// This is an advanced method that should be used with care.
        /// For example, if this method is being used to switch to another
        /// <b>IPofContext</b> mid-POF stream, it is important to eventually
        /// restore the original <b>IPofContext</b>. For example:
        /// <pre>
        /// IPofContext ctxOrig = writer.PofContext;
        /// try
        /// {
        ///     // switch to another IPofContext
        ///     writer.PofContext = ...;
        ///
        ///     // write POF data using the writer
        /// }
        /// finally
        /// {
        ///     // restore the original PofContext
        ///     writer.PofContext = ctxOrig;
        /// }
        /// </pre>
        /// </remarks>
        /// <value>
        /// The <b>IPofContext</b> object that contains user type meta-data.
        /// </value>
        IPofContext PofContext { get; set; }

        /// <summary>
        /// Gets the user type that is currently being written.
        /// </summary>
        /// <value>
        /// The user type information, or -1 if the <b>IPofWriter</b> is not
        /// currently writing a user type.
        /// </value>
        int UserTypeId { get; }

        /// <summary>
        /// Gets or sets the version identifier of the user type that is
        /// currently being written.
        /// </summary>
        /// <value>
        /// The integer version ID of the user type; always non-negative.
        /// </value>
        /// <exception cref="ArgumentException">
        /// If the given version ID is negative.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If no user type is being written.
        /// </exception>
        int VersionId { get; set; }

        #endregion

        #region Primitive value support

        /// <summary>
        /// Write a <b>Boolean</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>Boolean</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteBoolean(int index, Boolean value);

        /// <summary>
        /// Write a <b>Byte</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>Byte</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteByte(int index, Byte value);

        /// <summary>
        /// Write a <b>Char</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>Char</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteChar(int index, Char value);

        /// <summary>
        /// Write an <b>Int16</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>Int16</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteInt16(int index, Int16 value);

        /// <summary>
        /// Write an <b>Int32</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>Int32</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteInt32(int index, Int32 value);

        /// <summary>
        /// Write an <b>Int64</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>Int64</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteInt64(int index, Int64 value);

        /// <summary>
        /// Write an <b>RawInt128</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>RawInt128</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteRawInt128(int index, RawInt128 value);

        /// <summary>
        /// Write a <b>Single</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>Single</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteSingle(int index, Single value);

        /// <summary>
        /// Write a <b>Double</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>Double</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteDouble(int index, Double value);

        #endregion

        #region Primitive array support

        /// <summary>
        /// Write a <b>Boolean[]</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="array">
        /// The <b>Boolean[]</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteBooleanArray(int index, Boolean[] array);

        /// <summary>
        /// Write a <b>Byte[]</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="array">
        /// The <b>Byte[]</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteByteArray(int index, Byte[] array);

        /// <summary>
        /// Write a <b>Char[]</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="array">
        /// The <b>Char[]</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteCharArray(int index, Char[] array);

        /// <summary>
        /// Write an <b>Int16[]</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="array">
        /// The <b>Int16[]</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteInt16Array(int index, Int16[] array);

        /// <summary>
        /// Write an <b>Int32[]</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="array">
        /// The <b>Int32[]</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteInt32Array(int index, Int32[] array);

        /// <summary>
        /// Write an <b>Int64[]</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="array">
        /// The <b>Int64[]</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteInt64Array(int index, Int64[] array);

        /// <summary>
        /// Write a <b>Single[]</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="array">
        /// The <b>Single[]</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteSingleArray(int index, Single[] array);

        /// <summary>
        /// Write a <b>Double[]</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="array">
        /// The <b>Double[]</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteDoubleArray(int index, Double[] array);

        #endregion

        #region Object value support

        // TODO: add support for RawQuad

        /// <summary>
        /// Write a <b>Decimal</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>Decimal</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property index is invalid, or is less than or equal to the
        /// index of the previous property written to the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteDecimal(int index, Decimal value);

        /// <summary>
        /// Write a <b>String</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>String</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteString(int index, string value);

        /// <summary>
        /// Write a <b>DateTime</b> property to the POF stream in ISO8601
        /// format.
        /// </summary>
        /// <remarks>
        /// This method encodes only year, month and day information of the
        /// specified <b>DateTime</b> object. No time information is encoded.
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>DateTime</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteDate(int index, DateTime value);

        /// <summary>
        /// Write a <b>DateTime</b> property to the POF stream in ISO8601
        /// format.
        /// </summary>
        /// <remarks>
        /// This method encodes the year, month, day, hour, minute, second
        /// and millisecond information of the specified <b>DateTime</b>
        /// object. No timezone information is encoded.
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>DateTime</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteDateTime(int index, DateTime value);

        /// <summary>
        /// Write a <b>DateTime</b> property to the POF stream in ISO8601
        /// format.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method encodes the year, month, day, hour, minute, second,
        /// millisecond and timezone information of the specified
        /// <b>DateTime</b> object.</p>
        /// <p>
        /// Specified <paramref name="value"/> is converted to the local time
        /// before it is written to the POF stream.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>DateTime</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index
        /// of the previous property written to the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteLocalDateTime(int index, DateTime value);

        /// <summary>
        /// Write a <b>DateTime</b> property to the POF stream in ISO8601
        /// format.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method encodes the year, month, day, hour, minute, second,
        /// millisecond and timezone information of the specified
        /// <b>DateTime</b> object.</p>
        /// <p>
        /// Specified <paramref name="value"/> is converted to UTC time
        /// before it is written to the POF stream.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>DateTime</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteUniversalDateTime(int index, DateTime value);

        /// <summary>
        /// Write a <b>RawTime</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>RawTime</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteRawTime(int index, RawTime value);

        /// <summary>
        /// Write a <b>RawDateTime</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>RawDateTime</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property index is invalid, or is less than or equal to the
        /// index of the previous property written to the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteRawDateTime(int index, RawDateTime value);

        /// <summary>
        /// Write a <b>DateTime</b> property to the POF stream in
        /// ISO8601 format.
        /// </summary>
        /// <remarks>
        /// This method encodes the hour, minute, second and millisecond
        /// information of the specified <b>DateTime</b> object. No year,
        /// month, day or timezone information is encoded.
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>DateTime</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteTime(int index, DateTime value);

        /// <summary>
        /// Write a <b>DateTime</b> property to the POF stream in
        /// ISO8601 format.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method encodes the hour, minute, second, millisecond and
        /// timezone information of the specified <b>DateTime</b> object. No
        /// year, month or day information is encoded.</p>
        /// <p>
        /// Specified <paramref name="value"/> is converted to the local
        /// time before it is written to the POF stream.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>DateTime</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteLocalTime(int index, DateTime value);

        /// <summary>
        /// Write a <b>DateTime</b> property to the POF stream in
        /// ISO8601 format.
        /// </summary>
        /// <remarks>
        /// This method encodes the hour, minute, second, millisecond and
        /// timezone information of the specified <b>DateTime</b> object.
        /// No year, month or day information is encoded.
        /// <p/>
        /// Specified <paramref name="value"/> is converted to the UTC time
        /// before it is written to the POF stream.
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="value">
        /// The <b>DateTime</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteUniversalTime(int index, DateTime value);

        /// <summary>
        /// Write a <b>RawYearMonthInterval</b> property to the POF
        /// stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="interval">
        /// The <b>RawYearMonthInterval</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteRawYearMonthInterval(int index, RawYearMonthInterval interval);

        /// <summary>
        /// Write a <b>TimeSpan</b> property to the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method encodes the hour, minute, second, and millisecond
        /// information of the specified <b>TimeSpan</b> object.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="interval">
        /// The <b>TimeSpan</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteTimeInterval(int index, TimeSpan interval);

        /// <summary>
        /// Write a <b>TimeSpan</b> property to the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method encodes the day, hour, minute, second, and millisecond
        /// information of the specified <b>TimeSpan</b> object.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="interval">
        /// The <b>TimeSpan</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteDayTimeInterval(int index, TimeSpan interval);

        /// <summary>
        /// Write an <b>Object</b> property to the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The given object must be an instance (or an array of instances) of
        /// one of the following:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Byte</item>
        /// <item>Char</item>
        /// <item>Int16</item>
        /// <item>Int32</item>
        /// <item>Int64</item>
        /// <item>Single</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item><see cref="Binary"/></item>
        /// <item>String</item>
        /// <item>DateTime</item>
        /// <item>TimeSpan</item>
        /// <item>ICollection</item>
        /// <item><see cref="ILongArray"/></item>
        /// <item><see cref="RawTime"/></item>
        /// <item><see cref="RawDateTime"/></item>
        /// <item><see cref="RawYearMonthInterval"/></item>
        /// <item><see cref="IPortableObject"/></item>
        /// </list></p>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for the object must be
        /// obtainable from the <see cref="IPofContext"/> associated with
        /// this <b>IPofWriter</b>.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="o">
        /// The <b>Object</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream, or if the given property cannot be encoded into a
        /// POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteObject(int index, object o);

        /// <summary>
        /// Write a <see cref="Binary"/> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="bin">
        /// The <b>Binary</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream, or if the given property cannot be encoded into a
        /// POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteBinary(int index, Binary bin);

        #endregion

        #region Collection support

        /// <summary>
        /// Write an <b>Array</b> property to the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Each element of the given array must be an instance (or an array
        /// of instances) of one of the following:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Byte</item>
        /// <item>Char</item>
        /// <item>Int16</item>
        /// <item>Int32</item>
        /// <item>Int64</item>
        /// <item>Single</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item><see cref="Binary"/></item>
        /// <item>String</item>
        /// <item>DateTime</item>
        /// <item>TimeSpan</item>
        /// <item>ICollection</item>
        /// <item><see cref="ILongArray"/></item>
        /// <item><see cref="RawTime"/></item>
        /// <item><see cref="RawDateTime"/></item>
        /// <item><see cref="RawYearMonthInterval"/></item>
        /// <item><see cref="IPortableObject"/></item>
        /// </list></p>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for each element of the
        /// array must be obtainable from the <see cref="IPofContext"/>
        /// associated with this <b>IPofWriter</b>.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="array">
        /// The <b>Object[]</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream, or if the given property cannot be encoded into a
        /// POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteArray(int index, Array array);

        /// <summary>
        /// Write a uniform <b>Object[]</b> property to the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Each element of the given array must be an instance (or an array
        /// of instances) of one of the following:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Byte</item>
        /// <item>Char</item>
        /// <item>Int16</item>
        /// <item>Int32</item>
        /// <item>Int64</item>
        /// <item>Single</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item><see cref="Binary"/></item>
        /// <item>String</item>
        /// <item>DateTime</item>
        /// <item>TimeSpan</item>
        /// <item>ICollection</item>
        /// <item><see cref="ILongArray"/></item>
        /// <item><see cref="RawTime"/></item>
        /// <item><see cref="RawDateTime"/></item>
        /// <item><see cref="RawYearMonthInterval"/></item>
        /// <item><see cref="IPortableObject"/></item>
        /// </list></p>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for each element of
        /// the array must be obtainable from the <see cref="IPofContext"/>
        /// associated with this <b>IPofWriter</b>.</p>
        /// <p>
        /// Additionally, the type of each element must be equal to the
        /// specified type.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="array">
        /// The <b>Object[]</b> property value to write.
        /// </param>
        /// <param name="type">
        /// The element type.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream; if the given property cannot be encoded into a
        /// POF stream; or if the type of one or more elements of the array
        /// is not equal to the specified type.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteArray(int index, Array array, Type type);

        /// <summary>
        /// Write an <b>ICollection</b> property to the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Each element of the given array must be an instance (or an array
        /// of instances) of one of the following:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Byte</item>
        /// <item>Char</item>
        /// <item>Int16</item>
        /// <item>Int32</item>
        /// <item>Int64</item>
        /// <item>Single</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item><see cref="Binary"/></item>
        /// <item>String</item>
        /// <item>DateTime</item>
        /// <item>TimeSpan</item>
        /// <item>ICollection</item>
        /// <item><see cref="ILongArray"/></item>
        /// <item><see cref="RawTime"/></item>
        /// <item><see cref="RawDateTime"/></item>
        /// <item><see cref="RawYearMonthInterval"/></item>
        /// <item><see cref="IPortableObject"/></item>
        /// </list></p>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for each element of the
        /// array must be obtainable from the <see cref="IPofContext"/>
        /// associated with this <b>IPofWriter</b>.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="coll">
        /// The <b>ICollection</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream, or if the given property cannot be encoded into a
        /// POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteCollection(int index, ICollection coll);

        /// <summary>
        /// Write a uniform <b>ICollection</b> property to the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Each element of the given collection must be an instance (or an
        /// array of instances) of one of the following:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Byte</item>
        /// <item>Char</item>
        /// <item>Int16</item>
        /// <item>Int32</item>
        /// <item>Int64</item>
        /// <item>Single</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item><see cref="Binary"/></item>
        /// <item>String</item>
        /// <item>DateTime</item>
        /// <item>TimeSpan</item>
        /// <item>ICollection</item>
        /// <item><see cref="ILongArray"/></item>
        /// <item><see cref="RawTime"/></item>
        /// <item><see cref="RawDateTime"/></item>
        /// <item><see cref="RawYearMonthInterval"/></item>
        /// <item><see cref="IPortableObject"/></item>
        /// </list></p>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for each element of the
        /// collection must be obtainable from the <see cref="IPofContext"/>
        /// associated with this <b>IPofWriter</b>.</p>
        /// <p>
        /// Additionally, the type of each element must be equal to the
        /// specified type.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="coll">
        /// The <b>ICollection</b> property value to write.
        /// </param>
        /// <param name="type">
        /// The element type.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream; if the given property cannot be encoded into a
        /// POF stream; or if the type of one or more elements of the
        /// collection is not equal to the specified type.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteCollection(int index, ICollection coll, Type type);

        /// <summary>
        /// Write an <b>ILongArray</b> property to the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Each element of the given <b>ILongArray</b> must be an instance
        /// (or an array of instances) of one of the following:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Byte</item>
        /// <item>Char</item>
        /// <item>Int16</item>
        /// <item>Int32</item>
        /// <item>Int64</item>
        /// <item>Single</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item><see cref="Binary"/></item>
        /// <item>String</item>
        /// <item>DateTime</item>
        /// <item>TimeSpan</item>
        /// <item>ICollection</item>
        /// <item><see cref="ILongArray"/></item>
        /// <item><see cref="RawTime"/></item>
        /// <item><see cref="RawDateTime"/></item>
        /// <item><see cref="RawYearMonthInterval"/></item>
        /// <item><see cref="IPortableObject"/></item>
        /// </list></p>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for each element of the
        /// <b>ILongArray</b> must be obtainable from the
        /// <see cref="IPofContext"/> associated with this PofWriter.</p>
        /// </remarks>
        /// <param name="index">
        /// The propertie index.
        /// </param>
        /// <param name="la">
        /// The <b>ILongArray</b> property to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property index is invalid, or is less than or equal to the
        /// index of the previous property written to the POF stream.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the given property cannot be encoded into a POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error ocurs.
        /// </exception>
        void WriteLongArray(int index, ILongArray la);

        /// <summary>
        /// Write a uniform <b>ILongArray</b> property to the POF
        /// stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Each element of the given <b>ILongArray</b> must be an instance
        /// (or an array of instances) of one of the following:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Byte</item>
        /// <item>Char</item>
        /// <item>Int16</item>
        /// <item>Int32</item>
        /// <item>Int64</item>
        /// <item>Single</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item><see cref="Binary"/></item>
        /// <item>String</item>
        /// <item>DateTime</item>
        /// <item>TimeSpan</item>
        /// <item>ICollection</item>
        /// <item><see cref="ILongArray"/></item>
        /// <item><see cref="RawTime"/></item>
        /// <item><see cref="RawDateTime"/></item>
        /// <item><see cref="RawYearMonthInterval"/></item>
        /// <item><see cref="IPortableObject"/></item>
        /// </list></p>
        /// <p/>
        /// Otherwise, an <see cref="IPofSerializer"/> for each element of
        /// the <b>ILongArray</b> must be obtainable from the
        /// <see cref="IPofContext"/> associated with this
        /// <b>PofStreamWriter</b>.
        /// <p/>
        /// Additionally, the type of each element must be equal to the
        /// specified class.
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="la">
        /// The <b>ILongArray</b> property to write.
        /// </param>
        /// <param name="type">
        /// The class of all elements; must not be null.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property index is invalid, or is less than or equal to the
        /// index of the previous property written to the POF stream.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the given property cannot be encoded into a POF stream.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the type of one or more elements of the <b>ILongArray</b> is
        /// not equal to the specified class.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteLongArray(int index, ILongArray la, Type type);

        /// <summary>
        /// Write an <b>IDictionary</b> property to the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Each key and value of the given dictionary must be an instance
        /// (or an array of instances) of one of the following:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Byte</item>
        /// <item>Char</item>
        /// <item>Int16</item>
        /// <item>Int32</item>
        /// <item>Int64</item>
        /// <item>Single</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item><see cref="Binary"/></item>
        /// <item>String</item>
        /// <item>DateTime</item>
        /// <item>TimeSpan</item>
        /// <item>ICollection</item>
        /// <item><see cref="ILongArray"/></item>
        /// <item><see cref="RawTime"/></item>
        /// <item><see cref="RawDateTime"/></item>
        /// <item><see cref="RawYearMonthInterval"/></item>
        /// <item><see cref="IPortableObject"/></item>
        /// </list></p>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for each key and value
        /// of the dictionary must be obtainable from the
        /// <see cref="IPofContext"/> associated with this <b>IPofWriter</b>.
        /// </p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="dict">
        /// The <b>IDictionary</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream, or if the given property cannot be encoded into
        /// a POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteDictionary(int index, IDictionary dict);

        /// <summary>
        /// Write a uniform <b>IDictionary</b> property to the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Each key and value of the given dictionary must be an instance
        /// (or an array of instances) of one of the following:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Byte</item>
        /// <item>Char</item>
        /// <item>Int16</item>
        /// <item>Int32</item>
        /// <item>Int64</item>
        /// <item>Single</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item><see cref="Binary"/></item>
        /// <item>String</item>
        /// <item>DateTime</item>
        /// <item>TimeSpan</item>
        /// <item>ICollection</item>
        /// <item><see cref="ILongArray"/></item>
        /// <item><see cref="RawTime"/></item>
        /// <item><see cref="RawDateTime"/></item>
        /// <item><see cref="RawYearMonthInterval"/></item>
        /// <item><see cref="IPortableObject"/></item>
        /// </list></p>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for each key and value
        /// of the dictionary must be obtainable from the <see cref="IPofContext"/>
        /// associated with this <b>IPofWriter</b>.</p>
        /// <p>
        /// Additionally, the type of each key must be equal to the specified
        /// type.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="dict">
        /// The <b>IDictionary</b> property value to write.
        /// </param>
        /// <param name="keyType">
        /// The type of all keys; must not be <c>null</c>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream; if the given property cannot be encoded into a
        /// POF stream; or if the type of one or more keys of the dictionary
        /// is not equal to the specified type.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteDictionary(int index, IDictionary dict, Type keyType);

        /// <summary>
        /// Write a uniform <b>IDictionary</b> property to the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Each key and value of the given dictionary must be an instance
        /// (or an array of instances) of one of the following:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Byte</item>
        /// <item>Char</item>
        /// <item>Int16</item>
        /// <item>Int32</item>
        /// <item>Int64</item>
        /// <item>Single</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item><see cref="Binary"/></item>
        /// <item>String</item>
        /// <item>DateTime</item>
        /// <item>TimeSpan</item>
        /// <item>ICollection</item>
        /// <item><see cref="ILongArray"/></item>
        /// <item><see cref="RawTime"/></item>
        /// <item><see cref="RawDateTime"/></item>
        /// <item><see cref="RawYearMonthInterval"/></item>
        /// <item><see cref="IPortableObject"/></item>
        /// </list></p>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for each key and value
        /// of the dictionary must be obtainable from the <see cref="IPofContext"/>
        /// associated with this <b>IPofWriter</b>.</p>
        /// <p>
        /// Additionally, the type of each key and value must be equal to the
        /// specified types.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="dict">
        /// The <b>IDictionary</b> property value to write.
        /// </param>
        /// <param name="keyType">
        /// The type of all keys; must not be <c>null</c>.
        /// </param>
        /// <param name="valueType">
        /// The type of all values; must not be <c>null</c>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream; if the given property cannot be encoded into a
        /// POF stream; or if the type of one or more keys or values of the
        /// dictionary is not equal to the specified types.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteDictionary(int index, IDictionary dict, Type keyType, Type valueType);

        /// <summary>
        /// Write a generic <b>ICollection&lt;T&gt;</b> property to the POF
        /// stream.
        /// </summary>
        /// <remarks>
        /// Each element of the given array must be an instance (or an array
        /// of instances) of one of the following:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Byte</item>
        /// <item>Char</item>
        /// <item>Int16</item>
        /// <item>Int32</item>
        /// <item>Int64</item>
        /// <item>Single</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item><see cref="Binary"/></item>
        /// <item>String</item>
        /// <item>DateTime</item>
        /// <item>TimeSpan</item>
        /// <item>ICollection</item>
        /// <item><see cref="ILongArray"/></item>
        /// <item><see cref="RawTime"/></item>
        /// <item><see cref="RawDateTime"/></item>
        /// <item><see cref="RawYearMonthInterval"/></item>
        /// <item><see cref="IPortableObject"/></item>
        /// </list>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for each key and value
        /// of the array must be obtainable from the <see cref="IPofContext"/>
        /// associated with this <b>PofStreamWriter</b>.</p>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the elements in the collection.
        /// </typeparam>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="coll">
        /// The <b>ICollection&lt;T&gt;</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream, or if the given property cannot be encoded into
        /// a POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteCollection<T>(int index, ICollection<T> coll);

        /// <summary>
        /// Write a generic <b>IDictionary&lt;TKey, TValue&gt;</b> property
        /// to the POF stream.
        /// </summary>
        /// <remarks>
        /// Each key and value of the given dictionary must be an instance
        /// (or an array of instances) of one of the following:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Byte</item>
        /// <item>Char</item>
        /// <item>Int16</item>
        /// <item>Int32</item>
        /// <item>Int64</item>
        /// <item>Single</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item><see cref="Binary"/></item>
        /// <item>String</item>
        /// <item>DateTime</item>
        /// <item>TimeSpan</item>
        /// <item>ICollection</item>
        /// <item><see cref="ILongArray"/></item>
        /// <item><see cref="RawTime"/></item>
        /// <item><see cref="RawDateTime"/></item>
        /// <item><see cref="RawYearMonthInterval"/></item>
        /// <item><see cref="IPortableObject"/></item>
        /// </list>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for each key and value
        /// of the dictionary must be obtainable from the <see cref="IPofContext"/>
        /// associated with this <b>PofStreamWriter</b>.</p>
        /// </remarks>
        /// <typeparam name="TKey">
        /// The type of the keys in the dictionary.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The type of the values in the dictionary.
        /// </typeparam>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="dict">
        /// The <b>IDictionary&lt;TKey, TValue&gt;</b> property value to
        /// write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid, or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream, or if the given property cannot be encoded into
        /// a POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteDictionary<TKey, TValue>(int index, IDictionary<TKey, TValue> dict);

        #endregion

        #region POF user type support

        /// <summary>
        /// Obtain a PofWriter that can be used to write a set of properties into
        /// a single property of the current user type. The returned PofWriter is
        /// only valid from the time that it is returned until the next call is
        /// made to this PofWriter.
        /// </summary>
        /// <param name="iProp">
        /// the property index
        /// </param>
        /// <returns>
        /// a PofWriter whose contents are nested into a single property
        /// of this PofWriter
        /// </returns>
        /// <exception cref="ArgumentException">
        /// if the property index is invalid, or is less than or equal to the index
        /// of the previous property written to the POF stream
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// if no user type is being written
        /// </exception>
        /// <exception cref="IOException">
        /// if an I/O error occurs
        /// </exception>
        /// <since> Coherence 3.6 </since>
        IPofWriter CreateNestedPofWriter(int iProp);

        /// <summary>
        /// Obtain a PofWriter that can be used to write a set of properties into
        /// a single property of the current user type. The returned PofWriter is
        /// only valid from the time that it is returned until the next call is
        /// made to this PofWriter.
        /// </summary>
        /// <param name="iProp">
        /// the property index
        /// </param>
        /// <param name="nTypeId">
        /// the type identifier of the nested property
        /// </param>
        /// <returns>
        /// a PofWriter whose contents are nested into a single property
        /// of this PofWriter
        /// </returns>
        /// <exception cref="ArgumentException">
        /// if the property index is invalid, or is less than or equal to the index
        /// of the previous property written to the POF stream
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// if no user type is being written
        /// </exception>
        /// <exception cref="IOException">
        /// if an I/O error occurs
        /// </exception>
        /// <since> Coherence 12.2.1</since>
        IPofWriter CreateNestedPofWriter(int iProp, int nTypeId);


        /// <summary>
        /// Write the remaining properties to the POF stream, terminating the
        /// writing of the currrent user type.
        /// </summary>
        /// <remarks>
        /// <p>
        /// As part of writing out a user type, this method must be called by
        /// the <see cref="IPofSerializer"/> that is writing out the user
        /// type, or the POF stream will be corrupted.</p>
        /// <p>
        /// Calling this method terminates the current user type by writing a
        /// -1 to the POF stream after the last indexed property. Subsequent
        /// calls to the various <b>WriteXYZ</b> methods of this interface
        /// will fail after this method is called.</p>
        /// </remarks>
        /// <param name="properties">
        /// A <b>Binary</b> object containing zero or more indexed
        /// properties in binary POF encoded form; may be <c>null</c>.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If no user type is being written.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        void WriteRemainder(Binary properties);

        #endregion
    }
}