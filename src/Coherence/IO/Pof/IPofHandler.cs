/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// This interface defines the handler for an event-driven approach to
    /// parsing (or assembling) a POF stream.
    /// </summary>
    /// <author>Cameron Purdy  2006.07.10</author>
    /// <author>Aleksandar Seovic  2006.08.08</author>
    /// <author>Ivan Cikic  2006.08.08</author>
    /// <since>Coherence 3.2</since>
    public interface IPofHandler
    {
        /// <summary>
        /// This method is invoked when an identity is encountered in the POF
        /// stream.
        /// </summary>
        /// <remarks>
        /// The identity is used to uniquely identify the next value in the
        /// POF stream, and can be later referenced by the
        /// <see cref="OnIdentityReference"/> method.
        /// </remarks>
        /// <param name="id">
        /// If <b>(id >= 0)</b>, then this is the identity encountered in the
        /// POF stream, otherwise it is an indicator that the following value
        /// <i>could</i> have been assigned an identifier but was not (i.e.
        /// that the subsequent value is of a referenceable data type).
        /// </param>
        void RegisterIdentity(int id);

        /// <summary>
        /// Specifies that a <c>null</c> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        void OnNullReference(int position);

        /// <summary>
        /// Specifies that a reference to a previously-identified value has
        /// been encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="id">
        /// The identity of the previously encountered value, as was
        /// specified in a previous call to <see cref="RegisterIdentity"/>.
        /// </param>
        void OnIdentityReference(int position, int id);

        /// <summary>
        /// Report that an <b>Int16</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="n">
        /// The integer value as an <b>Int16</b>.
        /// </param>
        void OnInt16(int position, short n);

        /// <summary>
        /// Report that an <b>Int32</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="n">
        /// The integer value as an <b>Int32</b>.
        /// </param>
        void OnInt32(int position, int n);

        /// <summary>
        /// Report that an <b>Int64</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="n">
        /// The integer value as an <b>Int64</b>.
        /// </param>
        void OnInt64(int position, long n);

        /// <summary>
        /// Report that an <b>Int128</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="n">
        /// The integer value as an <b>Int128</b>.
        /// </param>
        void OnInt128(int position, RawInt128 n);

        /// <summary>
        /// Report that a base-2 single-precision floating point value has
        /// been encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="fl">
        /// The floating point value as a <b>Single</b>.
        /// </param>
        void OnFloat32(int position, Single fl);

        /// <summary>
        /// Report that a base-2 double-precision floating point value has
        /// been encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="dfl">
        /// The floating point value as a <b>Double</b>.
        /// </param>
        void OnFloat64(int position, double dfl);

        // TODO: add support for RawQuad
        // void OnFloat128(int position, RawQuad qfl);

        /// <summary>
        /// Report that a <b>Decimal</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="dec">
        /// The decimal value as a <b>Decimal</b>.
        /// </param>
        void OnDecimal32(int position, Decimal dec);

        /// <summary>
        /// Report that a <b>Decimal</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="dec">
        /// The decimal value as a <b>Decimal</b>.
        /// </param>
        void OnDecimal64(int position, Decimal dec);

        /// <summary>
        /// Report that a <b>Decimal</b> value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="dec">
        /// The decimal value as a <b>Decimal</b>.
        /// </param>
        void OnDecimal128(int position, Decimal dec);

        /// <summary>
        /// Report that a boolean value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="f">
        /// The boolean value.
        /// </param>
        void OnBoolean(int position, bool f);

        /// <summary>
        /// Report that an octet value (a byte) has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="b">
        /// The octet value as an int whose value is in the range 0 to 255
        /// (0x00-0xFF) inclusive.
        /// </param>
        void OnOctet(int position, int b);

        /// <summary>
        /// Report that a octet string value has been encountered in the POF
        /// stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="bin">
        /// The octect string value as a <b>Binary</b> object.
        /// </param>
        void OnOctetString(int position, Binary bin);

        /// <summary>
        /// Report that a character value has been encountered in the POF
        /// stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="ch">
        /// The character value as a <b>Char</b>.
        /// </param>
        void OnChar(int position, char ch);

        /// <summary>
        /// Report that a character string value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="s">
        /// The character string value as a <b>String</b> object.
        /// </param>
        void OnCharString(int position, string s);

        /// <summary>
        /// Report that a date value has been encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="year">
        /// The year number as defined by ISO8601.
        /// </param>
        /// <param name="month">
        /// The month number between 1 and 12 inclusive as defined by
        /// ISO8601.
        /// </param>
        /// <param name="day">
        /// The day number between 1 and 31 inclusive as defined by ISO8601.
        /// </param>
        void OnDate(int position, int year, int month, int day);

        /// <summary>
        /// Report that a year-month interval value has been encountered in
        /// the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="years">
        /// The number of years in the year-month interval.
        /// </param>
        /// <param name="months">
        /// The number of months in the year-month interval.
        /// </param>
        void OnYearMonthInterval(int position, int years, int months);

        /// <summary>
        /// Report that a time value has been encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="hour">
        /// The hour between 0 and 23 inclusive.
        /// </param>
        /// <param name="minute">
        /// The minute value between 0 and 59 inclusive.
        /// </param>
        /// <param name="second">
        /// The second value between 0 and 59 inclusive (and theoretically 60
        /// for a leap-second).
        /// </param>
        /// <param name="nanosecond">
        /// The nanosecond value between 0 and 999999999 inclusive.
        /// </param>
        /// <param name="isUTC">
        /// <b>true</b> if the time value is UTC or <b>false</b> if the time
        /// value does not have an explicit time zone.
        /// </param>
        void OnTime(int position, int hour, int minute, int second, int nanosecond, bool isUTC);

        /// <summary>
        /// Report that a time value (with a timezone offset) has been
        /// encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="hour">
        /// The hour between 0 and 23 inclusive.
        /// </param>
        /// <param name="minute">
        /// The minute value between 0 and 59 inclusive.
        /// </param>
        /// <param name="second">
        /// The second value between 0 and 59 inclusive (and theoretically 60
        /// for a leap-second).
        /// </param>
        /// <param name="nano">
        /// The nanosecond value between 0 and 999999999 inclusive.
        /// </param>
        /// <param name="zoneOffset">
        /// The timezone offset from UTC, for example 0 for BST, -5 for EST
        /// and +1 for CET.
        /// </param>
        /// <seealso href="http://www.worldtimezone.com/faq.html">
        /// worldtimezone.com
        /// </seealso>
        void OnTime(int position, int hour, int minute, int second, int nano, TimeSpan zoneOffset);

        /// <summary>
        /// Report that a time interval value has been encountered in the POF
        /// stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="hours">
        /// The number of hours in the time interval.
        /// </param>
        /// <param name="minutes">
        /// The number of minutes in the time interval, from 0 to 59
        /// inclusive.
        /// </param>
        /// <param name="seconds">
        /// The number of seconds in the time interval, from 0 to 59
        /// inclusive.
        /// </param>
        /// <param name="nanos">
        /// The number of nanoseconds, from 0 to 999999999 inclusive.
        /// </param>
        void OnTimeInterval(int position, int hours, int minutes, int seconds, int nanos);

        /// <summary>
        /// Report that a date-time value has been encountered in the POF
        /// stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="year">
        /// The year number as defined by ISO8601; note the difference with
        /// the Java Date class, whose year is relative to 1900.
        /// </param>
        /// <param name="month">
        /// The month number between 1 and 12 inclusive as defined by ISO8601;
        /// note the difference from the Java Date class, whose month value
        /// is 0-based (0-11).
        /// </param>
        /// <param name="day">
        /// The day number between 1 and 31 inclusive as defined by ISO8601.
        /// </param>
        /// <param name="hour">
        /// The hour between 0 and 23 inclusive.
        /// </param>
        /// <param name="minute">
        /// The minute value between 0 and 59 inclusive.
        /// </param>
        /// <param name="second">
        /// The second value between 0 and 59 inclusive (and theoretically 60
        /// for a leap-second).
        /// </param>
        /// <param name="nano">
        /// The nanosecond value between 0 and 999999999 inclusive.
        /// </param>
        /// <param name="isUTC">
        /// <b>true</b> if the time value is UTC or <b>false</b> if the time
        /// value does not have an explicit time zone.
        /// </param>
        void OnDateTime(int position, int year, int month, int day, int hour, int minute,
                        int second, int nano, bool isUTC);

        /// <summary>
        /// Report that a date-time value (with a timezone offset) has been
        /// encountered in the POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="year">
        /// The year number as defined by ISO8601; note the difference with
        /// the Java Date class, whose year is relative to 1900.
        /// </param>
        /// <param name="month">
        /// The month number between 1 and 12 inclusive as defined by
        /// ISO8601; note the difference from the Java Date class, whose
        /// month value is 0-based (0-11).</param>
        /// <param name="day">
        /// The day number between 1 and 31 inclusive as defined by ISO8601.
        /// </param>
        /// <param name="hour">
        /// The hour between 0 and 23 inclusive.
        /// </param>
        /// <param name="minute">
        /// The minute value between 0 and 59 inclusive.
        /// </param>
        /// <param name="second">
        /// The second value between 0 and 59 inclusive (and theoretically 60
        /// for a leap-second).
        /// </param>
        /// <param name="nano">
        /// The nanosecond value between 0 and 999999999 inclusive.
        /// </param>
        /// <param name="zoneOffset">
        /// The timezone offset from UTC, for example 0 for BST, -5 for EST
        /// and +1 for CET.
        /// </param>
        void OnDateTime(int position, int year, int month, int day, int hour, int minute,
                        int second, int nano, TimeSpan zoneOffset);

        /// <summary>
        /// Report that a day-time interval value has been encountered in the
        /// POF stream.
        /// </summary>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="days">
        /// The number of days in the day-time interval.
        /// </param>
        /// <param name="hours">
        /// The number of hours in the day-time interval, from 0 to 23
        /// inclusive.
        /// </param>
        /// <param name="minutes">
        /// The number of minutes in the day-time interval, from 0 to 59
        /// inclusive.
        /// </param>
        /// <param name="seconds">
        /// The number of seconds in the day-time interval, from 0 to 59
        /// inclusive.
        /// </param>
        /// <param name="nanos">
        /// The number of nanoseconds in the day-time interval, from 0 to
        /// 999999999 inclusive.
        /// </param>
        void OnDayTimeInterval(int position, int days, int hours, int minutes, int seconds, int nanos);

        /// <summary>
        /// Report that a collection of values has been encountered in the
        /// POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>elementCount</tt> elements
        /// in the collection, and the collection extent will then be
        /// terminated by a call to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of values (elements) in the collection.
        /// </param>
        void BeginCollection(int position, int elementCount);

        /// <summary>
        /// Report that a uniform collection of values has been encountered
        /// in the POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>elementCount</tt> elements
        /// in the collection, and the collection extent will then be
        /// terminated by a call to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of values (elements) in the collection.
        /// </param>
        /// <param name="typeId">
        /// The type identifier for all of the values in the uniform
        /// collection.
        /// </param>
        void BeginUniformCollection(int position, int elementCount, int typeId);

        /// <summary>
        /// Report that an array of values has been encountered in the POF
        /// stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>elementCount</tt> elements
        /// in the array, and the array extent will then be terminated by a
        /// call to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of values (elements) in the array.
        /// </param>
        void BeginArray(int position, int elementCount);

        /// <summary>
        /// Report that a uniform array of values has been encountered in the
        /// POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>cElements</tt> elements in
        /// the array, and the array extent will then be terminated by a call
        /// to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of values (elements) in the array.
        /// </param>
        /// <param name="typeId">
        /// The type identifier for all of the values in the uniform array.
        /// </param>
        void BeginUniformArray(int position, int elementCount, int typeId);

        /// <summary>
        /// Report that a sparse array of values has been encountered in the
        /// POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for present element in the sparse array (up to
        /// <tt>elementCount</tt> elements), and the array extent will then
        /// be terminated by a call to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of elements in the array, which is greater than
        /// or equal to the number of values in the sparse POF stream; in
        /// other words, the number of values that will subsequently be
        /// reported will not exceed this number.
        /// </param>
        void BeginSparseArray(int position, int elementCount);

        /// <summary>
        /// Report that a uniform sparse array of values has been encountered
        /// in the POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for present element in the sparse array (up to
        /// <tt>elementCount</tt> elements), and the array extent will then
        /// be terminated by a call to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of elements in the array, which is greater than
        /// or equal to the number of values in the sparse POF stream; in
        /// other words, the number of  values that will subsequently be
        /// reported will not exceed this number.
        /// </param>
        /// <param name="typeId">
        /// The type identifier for all of the values in the uniform sparse
        /// array.
        /// </param>
        void BeginUniformSparseArray(int position, int elementCount, int typeId);

        /// <summary>
        /// Report that a map of key/value pairs has been encountered in the
        /// POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>elementCount</tt> elements
        /// in the map, and the map extent will then be terminated by a call
        /// to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of key/value pairs (entries) in the map.
        /// </param>
        void BeginMap(int position, int elementCount);

        /// <summary>
        /// Report that a map of key/value pairs (with the keys being of a
        /// uniform type) has been encountered in the POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>elementCount</tt> elements
        /// in the map, and the map extent will then be terminated by a call
        /// to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of key/value pairs (entries) in the map.
        /// </param>
        /// <param name="typeIdKeys">
        /// The type identifier for all of the keys in the uniform-keys map.
        /// </param>
        void BeginUniformKeysMap(int position, int elementCount, int typeIdKeys);

        /// <summary>
        /// Report that a map of key/value pairs (with the keys being of a
        /// uniform type and the values being of a uniform type) has been
        /// encountered in the POF stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the <tt>elementCount</tt> elements
        /// in the map, and the map extent will then be terminated by a call
        /// to <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="elementCount">
        /// The exact number of key/value pairs (entries) in the map.
        /// </param>
        /// <param name="typeIdKeys">
        /// The type identifier for all of the keys in the uniform map.
        /// </param>
        /// <param name="typeIdValues">
        /// The type identifier for all of the values in the uniform map.
        /// </param>
        void BeginUniformMap(int position, int elementCount, int typeIdKeys, int typeIdValues);

        /// <summary>
        /// Report that a value of a "user type" has been encountered in the
        /// POF stream. A user type is analogous to a "type", and a value of
        /// a user type is analogous to an "object".
        /// </summary>
        /// <remarks>
        /// This method call will be followed by a separate call to an "on"
        /// or "begin" method for each of the property values in the user
        /// type, and the user type will then be terminated by a call to
        /// <see cref="EndComplexValue"/>.
        /// </remarks>
        /// <param name="position">
        /// Context-sensitive position information: property index within a
        /// user type, array index within an array, element counter within a
        /// collection, entry counter within a dictionary, -1 otherwise.
        /// </param>
        /// <param name="nId">
        /// Identity of the object to encode, or -1 if identity
        /// shouldn't be encoded in the POF stream.
        /// </param>
        /// <param name="typeId">
        /// The user type identifier, <tt>(typeId &gt;= 0)</tt>.
        /// </param>
        /// <param name="versionId">
        /// The version identifier for the user data type data in the POF
        /// stream, <tt>(versionId &gt;= 0)</tt>.
        /// </param>
        void BeginUserType(int position, int nId, int typeId, int versionId);

        /// <summary>
        /// Signifies the termination of the current complex value.
        /// </summary>
        /// <remarks>
        /// Complex values are any of the collection, array, map and user
        /// types. For each call to one of the "begin" methods, there will be
        /// a corresponding call to this method, even if there were no
        /// contents in the complex value.
        /// </remarks>
        void EndComplexValue();
    }
}