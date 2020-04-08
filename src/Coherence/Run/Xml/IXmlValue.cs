/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.Util;

namespace Tangosol.Run.Xml
{
    /// <summary>
    /// An interface for XML element content and element attribute values.
    /// </summary>
    /// <author>Cameron Purdy  2000.10.18</author>
    /// <author>Ana Cikic  2008.08.25</author>
    public interface IXmlValue : ICloneable
    {
        #region Accessors and mutators by type

        /// <summary>
        /// Get the value as a <b>boolean</b>.
        /// </summary>
        /// <returns>
        /// The value as a boolean.
        /// </returns>
        bool GetBoolean();

        /// <summary>
        /// Set the <b>boolean</b> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type boolean.
        /// </param>
        void SetBoolean(bool val);

        /// <summary>
        /// Get the value as an <b>int</b>.
        /// </summary>
        /// <returns>
        /// The value as an int.
        /// </returns>
        int GetInt();

        /// <summary>
        /// Set the <b>int</b> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type int.
        /// </param>
        void SetInt(int val);

        /// <summary>
        /// Get the value as a <b>long</b>.
        /// </summary>
        /// <returns>
        /// The value as a long.
        /// </returns>
        long GetLong();

        /// <summary>
        /// Set the <b>long</b> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type long.
        /// </param>
        void SetLong(long val);

        /// <summary>
        /// Get the value as a <b>double</b>.
        /// </summary>
        /// <returns>
        /// The value as a double.
        /// </returns>
        double GetDouble();

        /// <summary>
        /// Set the <b>double</b> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type double.
        /// </param>
        void SetDouble(double val);

        /// <summary>
        /// Get the value as a <b>decimal</b>.
        /// </summary>
        /// <returns>
        /// The value as a decimal.
        /// </returns>
        decimal GetDecimal();

        /// <summary>
        /// Set the <b>decimal</b> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type decimal.
        /// </param>
        void SetDecimal(decimal val);

        /// <summary>
        /// Get the value as a <b>string</b>.
        /// </summary>
        /// <returns>
        /// The value as a string.
        /// </returns>
        string GetString();

        /// <summary>
        /// Set the <b>string</b> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type string.
        /// </param>
        void SetString(string val);

        /// <summary>
        /// Get the value as <see cref="Binary"/>.
        /// </summary>
        /// <remarks>
        /// The XML format is expected to be Base64.
        /// </remarks>
        /// <returns>
        /// The value as a Binary object.
        /// </returns>
        Binary GetBinary();

        /// <summary>
        /// Set the <see cref="Binary"/> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type Binary.
        /// </param>
        void SetBinary(Binary val);

        /// <summary>
        /// Get the value as a <b>DateTime</b>.
        /// </summary>
        /// <returns>
        /// The value as a DateTime.
        /// </returns>
        DateTime GetDateTime();

        /// <summary>
        /// Set the value as a <b>DateTime</b>.
        /// </summary>
        /// <param name="val">
        /// A new value of type DateTime.
        /// </param>
        void SetDateTime(DateTime val);

        #endregion

        #region Accessors with default return values

        /// <summary>
        /// Get the value as a <b>boolean</b>.
        /// </summary>
        /// <param name="defaultValue">
        /// The default return value if the internal value can not be
        /// translated into a legal value of type boolean.
        /// </param>
        /// <returns>
        /// The value as a boolean.
        /// </returns>
        bool GetBoolean(bool defaultValue);

        /// <summary>
        /// Get the value as an <b>int</b>.
        /// </summary>
        /// <param name="defaultValue">
        /// The default return value if the internal value can not be
        /// translated into a legal value of type int.
        /// </param>
        /// <returns>
        /// The value as an int.
        /// </returns>
        int GetInt(int defaultValue);

        /// <summary>
        /// Get the value as a <b>long</b>.
        /// </summary>
        /// <param name="defaultValue">
        /// The default return value if the internal value can not be
        /// translated into a legal value of type long.
        /// </param>
        /// <returns>
        /// The value as a long.
        /// </returns>
        long GetLong(long defaultValue);

        /// <summary>
        /// Get the value as a <b>double</b>.
        /// </summary>
        /// <param name="defaultValue">
        /// The default return value if the internal value can not be
        /// translated into a legal value of type double.
        /// </param>
        /// <returns>
        /// The value as a double.
        /// </returns>
        double GetDouble(double defaultValue);

        /// <summary>
        /// Get the value as a <b>decimal</b>.
        /// </summary>
        /// <param name="defaultValue">
        /// The default return value if the internal value can not be
        /// translated into a legal value of type decimal.
        /// </param>
        /// <returns>
        /// The value as a decimal.
        /// </returns>
        decimal GetDecimal(decimal defaultValue);

        /// <summary>
        /// Get the value as a <b>string</b>.
        /// </summary>
        /// <param name="defaultValue">
        /// The default return value if the internal value can not be
        /// translated into a legal value of type string.
        /// </param>
        /// <returns>
        /// The value as a string.
        /// </returns>
        string GetString(string defaultValue);

        /// <summary>
        /// Get the value as <see cref="Binary"/>.
        /// </summary>
        /// <remarks>
        /// The XML format is expected to be Base64.
        /// </remarks>
        /// <param name="defaultValue">
        /// The default return value if the internal value can not be
        /// translated into a legal value of type Binary.
        /// </param>
        /// <returns>
        /// The value as a Binary object.
        /// </returns>
        Binary GetBinary(Binary defaultValue);

        /// <summary>
        /// Get the value as a <b>DateTime</b>.
        /// </summary>
        /// <param name="defaultValue">
        /// The default return value if the internal value can not be
        /// translated into a legal value of type DateTime.
        /// </param>
        /// <returns>
        /// The value as a DateTime.
        /// </returns>
        DateTime GetDateTime(DateTime defaultValue);

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Get the value as an object.
        /// </summary>
        /// <remarks>
        /// The following types are supported:
        /// <list type="bullet">
        /// <item>Boolean</item>
        /// <item>Integer</item>
        /// <item>Long</item>
        /// <item>Double</item>
        /// <item>Decimal</item>
        /// <item>String</item>
        /// <item>Binary</item>
        /// <item>DateTime</item>
        /// </list>
        /// It is always legal for an implementation to return the value as a
        /// string, for example returning a binary value in a Base64
        /// encoding. This method exists to allow one value to copy from
        /// another value.
        /// </remarks>
        /// <value>
        /// The value as an object or <c>null</c> if the IXmlValue does not
        /// have a value; attributes never have a <c>null</c> value.
        /// </value>
        object Value { get; }

        /// <summary>
        /// Get or set the parent element of this value.
        /// </summary>
        /// <remarks>
        /// The parent can not be modified once set.
        /// </remarks>
        /// <value>
        /// The parent element, or <c>null</c> if this value has no parent.
        /// </value>
        /// <exception cref="ArgumentNullException">
        /// If the specified parent is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If trying to set parent that is already set.
        /// </exception>
        IXmlElement Parent { get; set; }

        /// <summary>
        /// Determine if the value is empty.
        /// </summary>
        /// <value>
        /// <b>true</b> if the value is empty.
        /// </value>
        bool IsEmpty { get; }

        /// <summary>
        /// Determine if this value is an element attribute.
        /// </summary>
        /// <value>
        /// <b>true</b> if this value is an element attribute, otherwise
        /// <b>false</b>.
        /// </value>
        bool IsAttribute { get; }

        /// <summary>
        /// Determine if this value is an element's content.
        /// </summary>
        /// <value>
        /// <b>true</b> if this value is an element's content, otherwise
        /// <b>false</b>.
        /// </value>
        bool IsContent { get; }

        /// <summary>
        /// Determine if this value can be modified.
        /// </summary>
        /// <remarks>
        /// If the value can not be modified, all mutating methods are
        /// required to throw an <b>InvalidOperationException</b>.
        /// </remarks>
        /// <value>
        /// <b>true</b> if this value can be modified, otherwise <b>false</b>
        /// to indicate that this value is read-only.
        /// </value>
        bool IsMutable { get; }

        /// <summary>
        /// Write the value as it will appear in XML.
        /// </summary>
        /// <param name="writer">
        /// A <b>TextWriter</b> object to use to write to.
        /// </param>
        /// <param name="isPretty">
        /// <b>true</b> to specify that the output is intended to be as human
        /// readable as possible.
        /// </param>
        void WriteValue(TextWriter writer, bool isPretty);

        #endregion

        #region Object methods

        /// <summary>
        /// Format the XML value into a string in a display format.
        /// </summary>
        /// <returns>
        /// A string representation of the XML value.
        /// </returns>
        string ToString();

        /// <summary>
        /// Provide a hash value for this XML value.
        /// </summary>
        /// <remarks>
        /// The hash value is defined as one of the following:
        /// <list type="number">
        /// <item>0 if Value is <c>null</c></item>
        /// <item>otherwise the hash value is the GetHashCode() of the string
        /// representation of the value</item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// The hash value for this XML value.
        /// </returns>
        int GetHashCode();

        /// <summary>
        /// Compare this XML value with another XML value for equality.
        /// </summary>
        /// <param name="o">
        /// The XML value object to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if the values are equal, <b>false</b> otherwise.
        /// </returns>
        bool Equals(object o);

        #endregion
    }
}
