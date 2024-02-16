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
    /// The <b>IPofReader</b> interface provides the capability of reading a
    /// set of non-primitive .NET types ("user types") from a POF stream as an
    /// ordered sequence of indexed properties.
    /// </summary>
    /// <remarks>
    /// See <see cref="IPofWriter"/> for a complete description of the POF
    /// user type serialization format.
    /// </remarks>
    /// <author>Cameron Purdy, Jason Howes  2006.07.13</author>
    /// <author>Aleksandar Seovic  2006.08.08</author>
    /// <seealso cref="IPofContext"/>
    /// <seealso cref="IPofWriter"/>
    /// <since>Coherence 3.2</since>
    public interface IPofReader
    {
        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IPofContext"/> object used by this
        /// <b>IPofReader</b> to deserialize user types from a POF stream.
        /// </summary>
        /// <remarks>
        /// This is an advanced method that should be used with care.
        /// For example, if this method is being used to switch to another
        /// <b>IPofContext</b> mid-POF stream, it is important to eventually
        /// restore the original <b>IPofContext</b>. For example:
        /// <pre>
        /// IPofContext ctxOrig = reader.PofContext;
        /// try
        /// {
        ///     // switch to another IPofContext
        ///     reader.PofContext = ...;
        ///
        ///     // read POF data using the reader
        /// }
        /// finally
        /// {
        ///     // restore the original PofContext
        ///     reader.PofContext = ctxOrig;
        /// }
        /// </pre>
        /// </remarks>
        /// <value>
        /// The <b>IPofContext</b> object that contains user type meta-data.
        /// </value>
        IPofContext PofContext { get; set; }

        /// <summary>
        /// Gets the user type that is currently being parsed.
        /// </summary>
        /// <value>
        /// The user type information, or -1 if the <b>IPofReader</b> is not
        /// currently parsing a user type.
        /// </value>
        int UserTypeId { get; }

        /// <summary>
        /// Gets the version identifier of the user type that is currently
        /// being parsed.
        /// </summary>
        /// <value>
        /// The integer version ID read from the POF stream; always
        /// non-negative.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If no user type is being parsed.
        /// </exception>
        int VersionId { get; }

        #endregion

        #region Primitive value support

        /// <summary>
        /// Read a <b>Boolean</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Boolean</b> property value, or zero if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Boolean ReadBoolean(int index);

        /// <summary>
        /// Read a <b>Byte</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Byte</b> property value, or zero if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Byte ReadByte(int index);

        /// <summary>
        /// Read a <b>Char</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Char</b> property value, or zero if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Char ReadChar(int index);

        /// <summary>
        /// Read an <b>Int16</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Int16</b> property value, or zero if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Int16 ReadInt16(int index);

        /// <summary>
        /// Read an <b>Int32</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Int32</b> property value, or zero if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Int32 ReadInt32(int index);

        /// <summary>
        /// Read an <b>Int64</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Int64</b> property value, or zero if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Int64 ReadInt64(int index);

        /// <summary>
        /// Read an <b>RawInt128</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>RawInt128</b> property value, or zero if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        RawInt128 ReadRawInt128(int index);

        /// <summary>
        /// Read a <b>Single</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Single</b> property value, or zero if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Single ReadSingle(int index);

        /// <summary>
        /// Read a <b>Double</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Double</b> property value, or zero if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Double ReadDouble(int index);

        #endregion

        #region Primitive array support

        /// <summary>
        /// Read a <b>Boolean[]</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Boolean[]</b> property value, or null if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Boolean[] ReadBooleanArray(int index);

        /// <summary>
        /// Read a <b>Byte[]</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Byte[]</b> property value, or null if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Byte[] ReadByteArray(int index);

        /// <summary>
        /// Read a <b>Char[]</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Char[]</b> property value, or null if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Char[] ReadCharArray(int index);

        /// <summary>
        /// Read an <b>Int16[]</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Int16[]</b> property value, or null if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Int16[] ReadInt16Array(int index);

        /// <summary>
        /// Read an <b>Int32[]</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Int32[]</b> property value, or null if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Int32[] ReadInt32Array(int index);

        /// <summary>
        /// Read an <b>Int64[]</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Int64[]</b> property value, or null if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Int64[] ReadInt64Array(int index);

        /// <summary>
        /// Read a <b>Single[]</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Single[]</b> property value, or null if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Single[] ReadSingleArray(int index);

        /// <summary>
        /// Read a <b>Double[]</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Double[]</b> property value, or null if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Double[] ReadDoubleArray(int index);

        #endregion

        #region Object value support

        // TODO: add support for RawQuad

        /// <summary>
        /// Read a <b>Decimal</b> from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Decimal</b> property value, or zero if no value was
        /// available in the POF stream
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Decimal ReadDecimal(int index);

        /// <summary>
        /// Read a <b>String</b> property from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>String</b> property value, or null if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        string ReadString(int index);

        /// <summary>
        /// Read a <b>DateTime</b> property from the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method will attempt to read both the date and time component
        /// from the POF stream. If the value in the stream does not contain
        /// both components, the corresponding values in the returned
        /// <b>DateTime</b> instance will be set to default values.</p>
        /// <p>
        /// If the encoded value in the POF stream contains time zone
        /// information, this method will ignore time zone information
        /// and return a literal <b>DateTime</b> value, as read from the
        /// stream.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>DateTime</b> property value.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        DateTime ReadDateTime(int index);

        /// <summary>
        /// Read a <b>DateTime</b> property from the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method will attempt to read both the date and time component
        /// from the POF stream. If the value in the stream does not contain
        /// both components, the corresponding values in the returned
        /// <b>DateTime</b> instance will be set to default values.</p>
        /// <p>
        /// If the encoded value in the POF stream contains time zone
        /// information, this method will use it to determine and return
        /// the local time <b>for the reading thread</b>.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>DateTime</b> property value.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        DateTime ReadLocalDateTime(int index);

        /// <summary>
        /// Read a <b>DateTime</b> property from the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method will attempt to read both the date and time components
        /// from the POF stream. If the value in the stream does not contain
        /// both components, the corresponding values in the returned
        /// <b>DateTime</b> instance will be set to default values.</p>
        /// <p>
        /// If the encoded value in the POF stream contains time zone
        /// information, this method will use it to determine and return
        /// a Coordinated Universal Time (UTC) value.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>DateTime</b> property value.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        DateTime ReadUniversalDateTime(int index);

        /// <summary>
        /// Read a <b>DateTime</b> property from the POF stream.
        /// </summary>
        /// <remarks>
        /// This method will read only the date component of a date-time value
        /// from the POF stream. It will ignore the time component if present
        /// and initialize the time-related fields of the return value to their
        /// default values.
        /// </remarks>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>DateTime</b> property value.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        DateTime ReadDate(int index);

        /// <summary>
        /// Read a <b>RawTime</b> property from the POF stream.
        /// </summary>
        /// <remarks>
        /// The <see cref="RawTime"/> class contains the raw time information
        /// that was carried in the POF stream, including raw timezone
        /// information.
        /// </remarks>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>RawTime</b> property value, or null if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        RawTime ReadRawTime(int index);

        /// <summary>
        /// Read a <b>RawDateTime</b> from the POF stream.
        /// </summary>
        /// <remarks>
        /// The <see cref="RawDateTime"/> class contains the raw date and
        /// time information that was carried in the POF stream, including
        /// raw timezone information.
        /// </remarks>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>RawDateTime</b> property value, or <c>null</c> if no value
        /// was available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        RawDateTime ReadRawDateTime(int index);

        /// <summary>
        /// Read a <b>RawYearMonthInterval</b> from the POF stream.
        /// </summary>
        /// <remarks>
        /// The <see cref="RawYearMonthInterval"/> struct contains the raw
        /// year-month interval information that was carried in the POF
        /// stream.
        /// </remarks>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>RawYearMonthInterval</b> property value, or <c>null</c> if no
        /// value was available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        RawYearMonthInterval ReadRawYearMonthInterval(int index);

        /// <summary>
        /// Reads a <b>TimeSpan</b> from the POF stream.
        /// </summary>
        /// <remarks>
        /// This method will read only the time component of a day-time-interval
        /// value from the POF stream. It will ignore the day component if present
        /// and initialize day-related fields of the return value to their default
        /// values.
        /// </remarks>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>TimeSpan</b> property value, or <c>null</c> if no value
        /// was available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        TimeSpan ReadTimeInterval(int index);

        /// <summary>
        /// Reads a <b>TimeSpan</b> from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>TimeSpan</b> property value, or <c>null</c> if no value
        /// was available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        TimeSpan ReadDayTimeInterval(int index);

        /// <summary>
        /// Read a property of any type, including a user type,
        /// from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The object value; may be <c>null</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        object ReadObject(int index);

        /// <summary>
        /// Read a <see cref="Binary"/> from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Binary</b> property value, or <c>null</c> if no value was
        /// available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Binary ReadBinary(int index);

        #endregion

        #region Collections support

        /// <summary>
        /// Read an array of object values.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// An array of object values, or <c>null</c> if
        /// there is no array data in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Array ReadArray(int index);

        /// <summary>
        /// Read an array of object values.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <param name="array">
        /// The optional array to use to store the values, or to use as a
        /// typed template for creating an array to store the values,
        /// following the documentation for <b>ArrayList.ToArray(Type)</b>.
        /// </param>
        /// <returns>
        /// An array of object values, or <c>null</c> if no array is passed
        /// and there is no array data in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Array ReadArray(int index, Array array);

        /// <summary>
        /// Read an <b>ILongArray</b> of object values.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <param name="array">
        /// The optional <b>ILongArray</b> object to use to store the values.
        /// </param>
        /// <returns>
        /// An <b>ILongArray</b> of object values, or <c>null</c> if no
        /// <b>ILongArray</b> is passed and there is no array data in the
        /// POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        ILongArray ReadLongArray(int index, ILongArray array);

        /// <summary>
        /// Read an <b>ICollection</b> of object values from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <param name="coll">
        /// The optional <b>ICollection</b> to use to store the values.
        /// </param>
        /// <returns>
        /// A collection of object values, or <c>null</c> if no collection is
        /// passed and there is no collection data in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        ICollection ReadCollection(int index, ICollection coll);

        /// <summary>
        /// Read an <b>IDictionary</b> of key/value pairs from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <param name="dict">
        /// The optional <b>IDictionary</b> to initialize.
        /// </param>
        /// <returns>
        /// An <b>IDictionary</b> of key/value pairs object values, or
        /// <c>null</c> if no dictionary is passed and there is no key/value
        /// data in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="dict"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        IDictionary ReadDictionary(int index, IDictionary dict);

        /// <summary>
        /// Read a generic <b>ICollection&lt;T&gt;</b> of object values from
        /// the POF stream.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements it the collection.
        /// </typeparam>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <param name="coll">
        /// The optional <b>ICollection&lt;T&gt;</b> to use to store the
        /// values.
        /// </param>
        /// <returns>
        /// A generic collection of object values, or <c>null</c> if no
        /// collection is passed and there is no collection data in the POF
        /// stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        ICollection<T> ReadCollection<T>(int index, ICollection<T> coll);

        /// <summary>
        /// Read a generic <b>IDictionary&lt;TKey, TValue&gt;</b> of
        /// key/value pairs from the POF stream.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the keys in the dictionary.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The type of the values in the dictionary.
        /// </typeparam>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <param name="dictionary">
        /// The optional <b>IDictionary&lt;TKey, TValue&gt;</b> to initialize.
        /// </param>
        /// <returns>
        /// An <b>IDictionary&lt;TKey, TValue&gt;</b> of key/value pairs
        /// object values, or <c>null</c> if no dictionary is passed and
        /// there is no key/value data in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="dictionary"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        IDictionary<TKey, TValue> ReadDictionary<TKey, TValue>(int index, IDictionary<TKey, TValue> dictionary);

        #endregion

        #region POF user type support

        /// <summary>
        /// Register an identity for a newly created user type instance.
        /// </summary>
        /// <remarks>
        /// If identity/reference types are enabled, an identity is used to
        /// uniquely identify a user type instance within a POF stream. The
        /// identity immediately proceeds the instance value in the POF stream
        /// and can be used later in the stream to reference the instance.
        /// <p/>
        /// IPofSerializer implementations must call this method with each
        /// user type instance instantiated during deserialization prior to 
        /// reading any properties of the instance which are user type
        /// instances themselves.
        /// </remarks>
        /// <param name="o">
        /// The object to register the identity for.
        /// </param>
        /// <see>IPofSerializer#Deserialize(IPofReader)</see>
        /// <since>Coherence 3.7.1</since>
        void RegisterIdentity(object o);
        
        /// <summary>
        /// Obtain an IPofReader that can be used to read a set of properties 
        /// from a single property of the current user type. The returned 
        /// IPofReader is only valid from the time that it is returned until
        /// the next call is made to this IPofReader.
        /// </summary>
        /// <param name="iProp">
        /// The property index to read from. </param>
        /// <returns>
        /// An IPofReader that reads its contents from  a single property of
        /// this IPofReader.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property, or if no user type is being parsed.
        /// </exception>
        /// <exception cref="IOException">
        /// if an I/O error occurs
        /// </exception>
        /// <since> Coherence 3.6 </since>
        IPofReader CreateNestedPofReader(int iProp);

        /// <summary>
        /// Read all remaining indexed properties of the current user type
        /// from the POF stream.
        /// </summary>
        /// <remarks>
        /// <p>
        /// As part of reading in a user type, this method must be called by
        /// the <see cref="IPofSerializer"/> that is reading the user type,
        /// or the read position within the POF stream will be corrupted.</p>
        /// <p>
        /// Subsequent calls to the various <b>ReadXYZ</b> methods of this
        /// interface will fail after this method is called.</p>
        /// </remarks>
        /// <returns>
        /// A <b>Binary</b> object containing zero or more indexed properties
        /// in binary POF encoded form, or null if no value was available
        /// in the POF stream
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If no user type is being parsed.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        Binary ReadRemainder();

        #endregion
    }
}