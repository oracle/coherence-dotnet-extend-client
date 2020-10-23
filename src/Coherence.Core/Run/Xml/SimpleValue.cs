/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Util;

namespace Tangosol.Run.Xml
{
    /// <summary>
    /// A simple implementation of the <see cref="IXmlValue"/> interface.
    /// </summary>
    /// <remarks>
    /// Protected methods are provided to support inheriting classes.
    /// </remarks>
    /// <author>Cameron Purdy  2000.10.18</author>
    /// <author>Ana Cikic  2009.08.25</author>
    public class SimpleValue : IXmlValue, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Construct an empty SimpleValue.
        /// </summary>
        public SimpleValue() : this(null, false, false)
        {}

        /// <summary>
        /// Construct a SimpleValue.
        /// </summary>
        /// <remarks>
        /// Constructs an element's content value from the passed object
        /// value. If the object is a string, then the string should be
        /// un-escaped by this point; it must not still be in the form of
        /// the CDATA construct.
        /// </remarks>
        /// <param name="value">
        /// The initial value for this SimpleValue.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the string value is illegal.
        /// </exception>
        public SimpleValue(object value) : this(value, false, false)
        {}

        /// <summary>
        /// Construct a SimpleValue.
        /// </summary>
        /// <remarks>
        /// Constructs an element's content or attribute value from the
        /// passed string value. The string should be un-escaped by this
        /// point; it must not still be in the form of the CDATA construct.
        /// </remarks>
        /// <param name="value">
        /// The initial value for this SimpleValue.
        /// </param>
        /// <param name="isAttribute">
        /// <b>true</b> if this SimpleValue is an element attribute value;
        /// <b>false</b> if an element's content's value.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the string value is illegal.
        /// </exception>
        public SimpleValue(object value, bool isAttribute) : this(value, isAttribute, false)
        {}

        /// <summary>
        /// Construct a SimpleValue.
        /// </summary>
        /// <remarks>
        /// Constructs an element's content or attribute value from the
        /// passed string value, and also allows the caller to specify that
        /// the value is immutable. The string should be un-escaped by this
        /// point; it must not still be in the form of the CDATA construct.
        /// </remarks>
        /// <param name="value">
        /// The initial value for this SimpleValue.
        /// </param>
        /// <param name="isAttribute">
        /// <b>true</b> if this SimpleValue is an element attribute value;
        /// <b>false</b> if an element's content's value.
        /// </param>
        /// <param name="isReadOnly">
        /// <b>true</b> if this SimpleValue is intended to be read-only once
        /// the constructor has finished.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the string value is illegal.
        /// </exception>
        public SimpleValue(object value, bool isAttribute, bool isReadOnly)
        {
            if (value != null &&
                !( value is bool
                || value is int
                || value is long
                || value is double
                || value is decimal
                || value is string
                || value is Binary
                || value is DateTime))
            {
                throw new ArgumentException("Unsupported type: " + value.GetType().FullName);
            }

            // attribute values must not be null
            if (isAttribute && value == null)
            {
                value = "";
            }
            
            IsAttribute   = isAttribute;
            InternalValue = value;
            IsMutable     = !isReadOnly; // must be the last call in case isReadOnly is true
        }

        #endregion

        #region IXmlValue members

        /// <summary>
        /// Get the value as a <b>boolean</b>.
        /// </summary>
        /// <returns>
        /// The value as a boolean.
        /// </returns>
        public virtual bool GetBoolean()
        {
            return GetBoolean(false);
        }

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
        public virtual bool GetBoolean(bool defaultValue)
        {
            object value = EnsureType(XmlValueType.Boolean);
            return value == null ? defaultValue : (bool) value;
        }

        /// <summary>
        /// Set the <b>boolean</b> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type boolean.
        /// </param>
        public virtual void SetBoolean(bool val)
        {
            InternalValue = val;
        }

        /// <summary>
        /// Get the value as an <b>int</b>.
        /// </summary>
        /// <returns>
        /// The value as an int.
        /// </returns>
        public virtual int GetInt()
        {
            return GetInt(0);
        }

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
        public virtual int GetInt(int defaultValue)
        {
            object i = EnsureType(XmlValueType.Integer);
            return i == null ? defaultValue : (int) i;
        }

        /// <summary>
        /// Set the <b>int</b> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type int.
        /// </param>
        public virtual void SetInt(int val)
        {
            InternalValue = val;
        }

        /// <summary>
        /// Get the value as a <b>long</b>.
        /// </summary>
        /// <returns>
        /// The value as a long.
        /// </returns>
        public virtual long GetLong()
        {
            return GetLong(0L);
        }

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
        public virtual long GetLong(long defaultValue)
        {
            object l = EnsureType(XmlValueType.Long);
            return l == null ? defaultValue : (long) l;
        }

        /// <summary>
        /// Set the <b>long</b> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type long.
        /// </param>
        public virtual void SetLong(long val)
        {
            InternalValue = val;
        }

        /// <summary>
        /// Get the value as a <b>double</b>.
        /// </summary>
        /// <returns>
        /// The value as a double.
        /// </returns>
        public virtual double GetDouble()
        {
            return GetDouble(0.0);
        }

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
        public virtual double GetDouble(double defaultValue)
        {
            object d = EnsureType(XmlValueType.Double);
            return d == null ? defaultValue : (double) d;
        }

        /// <summary>
        /// Set the <b>double</b> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type double.
        /// </param>
        public virtual void SetDouble(double val)
        {
            InternalValue = val;
        }

        /// <summary>
        /// Get the value as a <b>decimal</b>.
        /// </summary>
        /// <returns>
        /// The value as a decimal.
        /// </returns>
        public virtual decimal GetDecimal()
        {
            return GetDecimal(DEC_ZERO);
        }

        /// <summary>
        /// Get the value as a <b>Decimal</b>.
        /// </summary>
        /// <param name="defaultValue">
        /// The default return value if the internal value can not be
        /// translated into a legal value of type decimal.
        /// </param>
        /// <returns>
        /// The value as a Decimal.
        /// </returns>
        public virtual decimal GetDecimal(decimal defaultValue)
        {
            object dec = EnsureType(XmlValueType.Decimal);
            return dec == null ? defaultValue : (decimal) dec;
        }

        /// <summary>
        /// Set the <b>decimal</b> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type decimal.
        /// </param>
        public virtual void SetDecimal(decimal val)
        {
            InternalValue = val;
        }

        /// <summary>
        /// Get the value as a <b>string</b>.
        /// </summary>
        /// <returns>
        /// The value as a string.
        /// </returns>
        public virtual string GetString()
        {
            return GetString("");
        }

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
        public virtual string GetString(string defaultValue)
        {
            object s = EnsureType(XmlValueType.String);
            return s == null ? defaultValue : (string) s;
        }

        /// <summary>
        /// Set the <b>string</b> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type string.
        /// </param>
        public virtual void SetString(string val)
        {
            InternalValue = val;
        }

        /// <summary>
        /// Get the value as <see cref="Binary"/>.
        /// </summary>
        /// <remarks>
        /// The XML format is expected to be Base64.
        /// </remarks>
        /// <returns>
        /// The value as a Binary object.
        /// </returns>
        public virtual Binary GetBinary()
        {
            return GetBinary(NO_BYTES);
        }

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
        public virtual Binary GetBinary(Binary defaultValue)
        {
            object bin = EnsureType(XmlValueType.Binary);
            return bin == null ? defaultValue : (Binary) bin;
        }

        /// <summary>
        /// Set the <see cref="Util.Binary"/> value.
        /// </summary>
        /// <param name="val">
        /// A new value of type Binary.
        /// </param>
        public virtual void SetBinary(Binary val)
        {
            InternalValue = val;
        }

        /// <summary>
        /// Get the value as a <b>DateTime</b>.
        /// </summary>
        /// <returns>
        /// The value as a DateTime.
        /// </returns>
        public virtual DateTime GetDateTime()
        {
            return GetDateTime(DFT_DATE);
        }

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
        public virtual DateTime GetDateTime(DateTime defaultValue)
        {
            object dt = EnsureType(XmlValueType.DateTime);
            return dt == null ? defaultValue : (DateTime) dt;
        }

        /// <summary>
        /// Set the value as a <b>DateTime</b>.
        /// </summary>
        /// <param name="val">
        /// A new value of type DateTime.
        /// </param>
        public virtual void SetDateTime(DateTime val)
        {
            InternalValue = val;
        }

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
        public virtual object Value
        {
            get { return InternalValue; }
        }

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
        public virtual IXmlElement Parent
        {
            get { return m_parent; }
            set
            {
                if (!IsMutable)
                {
                    throw new InvalidOperationException("value \"" + this + "\" is not mutable");
                }

                if (value == null)
                {
                    throw new ArgumentNullException("parent");
                }

                IXmlElement xmlParent = Parent;
                if (xmlParent != null && xmlParent != value)
                {
                    throw new InvalidOperationException("parent already set");
                }

                m_parent = value;
            }
        }

        /// <summary>
        /// Determine if the value is empty.
        /// </summary>
        /// <value>
        /// <b>true</b> if the value is empty.
        /// </value>
        public virtual bool IsEmpty
        {
            get
            {
                object o = InternalValue;

                if (o == null)
                {
                    return true;
                }

               if (o is string && ((string) o).Length == 0)
                {
                    return true;
                }

                if (o is Binary && ((Binary) o).Length == 0)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Determine if this value is an element attribute.
        /// </summary>
        /// <value>
        /// <b>true</b> if this value is an element attribute, otherwise
        /// <b>false</b>.
        /// </value>
        public virtual bool IsAttribute
        {
            get { return m_isAttribute; }
            set { m_isAttribute = value; }
        }

        /// <summary>
        /// Determine if this value is an element's content.
        /// </summary>
        /// <value>
        /// <b>true</b> if this value is an element's content, otherwise
        /// <b>false</b>.
        /// </value>
        public virtual bool IsContent
        {
            get { return !IsAttribute; }
        }

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
        public virtual bool IsMutable
        {
            get
            {
                if (!m_isMutable)
                {
                    return false;
                }

                IXmlElement parent = Parent;
                if (parent != null && !parent.IsMutable)
                {
                    return false;
                }

                return true;
            }
            set { m_isMutable = value; }
        }

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
        public virtual void WriteValue(TextWriter writer, bool isPretty)
        {
            object o = InternalValue;
            if (IsAttribute)
            {
                if (o is Binary)
                {
                    Base64FormattingOptions options = isPretty
                                                    ? Base64FormattingOptions.InsertLineBreaks
                                                    : Base64FormattingOptions.None;
                    writer.Write(System.Convert.ToBase64String(((Binary) o).ToByteArray(), options));
                }
                else
                {
                    writer.Write(XmlHelper.Quote(o.ToString()));
                }
            }
            else
            {
                if (o is string)
                {
                    writer.Write(XmlHelper.EncodeContent((string) o, true));
                }
                else if (o is Binary)
                {
                    Base64FormattingOptions options = isPretty
                                                     ? Base64FormattingOptions.InsertLineBreaks
                                                     : Base64FormattingOptions.None;
                    writer.Write(System.Convert.ToBase64String(((Binary) o).ToByteArray(), options));
                }
                else if (o != null)
                {
                    writer.Write(o.ToString());
                }
            }
        }

        #endregion

        #region IPortableObject members

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void ReadExternal(IPofReader reader)
        {
            if (m_value != null || m_isAttribute || !m_isMutable)
            {
                throw new Exception("deserialization not active");
            }

            switch (reader.ReadByte(0))
            {
                case 0:
                    break;

                case (byte) XmlValueType.Boolean:
                    m_value = reader.ReadBoolean(1);
                    break;

                case (byte) XmlValueType.Integer:
                    m_value = reader.ReadInt32(1);
                    break;

                case (byte) XmlValueType.Long:
                    m_value = reader.ReadInt64(1);
                    break;

                case (byte) XmlValueType.Double:
                    m_value = reader.ReadDouble(1);
                    break;

                case (byte) XmlValueType.Decimal:
                    m_value = reader.ReadDecimal(1);
                    break;

                case (byte) XmlValueType.String:
                    m_value = reader.ReadString(1);
                    break;

                case (byte) XmlValueType.Binary:
                    m_value = reader.ReadBinary(1);
                    break;

                case (byte) XmlValueType.DateTime:
                    m_value = reader.ReadDateTime(1);
                    break;

                default:
                    throw new IOException();
            }

            m_isAttribute = reader.ReadBoolean(2);
            m_isMutable   = reader.ReadBoolean(3);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteExternal(IPofWriter writer)
        {
            // note: this object is not responsible for writing out its parent

            object o = m_value;
            if (o == null)
            {
                writer.WriteByte(0, 0);
            }
            else if (o is string)
            {
                writer.WriteByte(0, (byte) XmlValueType.String);
                writer.WriteString(1, (string) o);
            }
            else if (o is bool)
            {
                writer.WriteByte(0, (byte) XmlValueType.Boolean);
                writer.WriteBoolean(1, (bool) o);
            }
            else if (o is int)
            {
                writer.WriteByte(0, (byte) XmlValueType.Integer);
                writer.WriteInt32(1, (int) o);
            }
            else if (o is long)
            {
                writer.WriteByte(0, (byte) XmlValueType.Long);
                writer.WriteInt64(1, (long) o);
            }
            else if (o is double)
            {
                writer.WriteByte(0, (byte) XmlValueType.Double);
                writer.WriteDouble(1, (Double) o);
            }
            else if (o is decimal)
            {
                writer.WriteByte(0, (byte )XmlValueType.Decimal);
                writer.WriteDecimal(1, (decimal) o);
            }
            else if (o is Binary)
            {
                writer.WriteByte(0, (byte) XmlValueType.Binary);
                writer.WriteBinary(1, (Binary) o);
            }
            else if (o is DateTime)
            {
                writer.WriteByte(0, (byte) XmlValueType.DateTime);
                writer.WriteDateTime(1, (DateTime) o);
            }
            else
            {
                throw new IOException("unsupported type to write: " + o.GetType().FullName);
            }

            writer.WriteBoolean(2, m_isAttribute);
            writer.WriteBoolean(3, m_isMutable);
        }

        #endregion

        #region Support for inheriting classes

        /// <summary>
        /// Get or set the internal value of this IXmlValue.
        /// </summary>
        /// <remarks>
        /// This property acts as a single point to which all accessor calls
        /// route. As such, it is intended to be extended by inheriting
        /// implementations.
        /// </remarks>
        /// <value>
        /// The current value of this SimpleValue object or <c>null</c>.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If trying to set value when this IXmlValue is not mutable.
        /// </exception>
        protected virtual object InternalValue
        {
            get { return m_value; }
            set
            {
                if (!IsMutable)
                {
                    throw new InvalidOperationException("value \"" + this + "\" is not mutable");
                }

                // attribute values must not be null
                if (IsAttribute && value == null)
                {
                    throw new ArgumentNullException("attribute value");
                }

                m_value = value;
            }
        }

        /// <summary>
        /// Change the type of the internal representation of the IXmlValue.
        /// </summary>
        /// <remarks>
        /// A failed conversion will leave the value as <c>null</c>.
        /// </remarks>
        /// <param name="type">
        /// The enumerated type to convert to.
        /// </param>
        /// <returns>
        /// The current value of this SimpleValue object as the specified
        /// type or <c>null</c>.
        /// </returns>
        protected virtual object EnsureType(XmlValueType type)
        {
            object oldValue = InternalValue;
            object newValue = Convert(oldValue, type);
            if (oldValue != newValue && IsMutable)
            {
                InternalValue = newValue;
            }
            return newValue;
        }

        /// <summary>
        /// Convert the passed object to the specified type.
        /// </summary>
        /// <param name="o">
        /// The object value.
        /// </param>
        /// <param name="type">
        /// The enumerated type to convert to.
        /// </param>
        /// <returns>
        /// An object of the specified type.
        /// </returns>
        protected virtual object Convert(object o, XmlValueType type)
        {
            return XmlHelper.Convert(o, type);
        }

        #endregion
        
        #region Object methods

        /// <summary>
        /// Format the XML value into a string in a display format.
        /// </summary>
        /// <returns>
        /// A string representation of the XML value.
        /// </returns>
        public override string ToString()
        {
            return GetString();
        }

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
        public override int GetHashCode()
        {
            return XmlHelper.HashValue(this);
        }

        /// <summary>
        /// Compare this XML value with another XML value for equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if the values are equal, <b>false</b> otherwise.
        /// </returns>
        public override bool Equals(object o)
        {
            if (!(o is IXmlValue))
            {
                return false;
            }

            return XmlHelper.EqualsValue(this, (IXmlValue) o);
        }

        #endregion

        #region ICloneable members

        /// <summary>
        /// Creates and returns a copy of this SimpleValue.
        /// </summary>
        /// <remarks>
        /// The returned copy is "unlinked" from the parent and mutable.
        /// </remarks>
        /// <returns>
        /// A clone of this instance.
        /// </returns>
        public virtual object Clone()
        {
            SimpleValue that = (SimpleValue) MemberwiseClone();
            that.m_parent    = null;
            that.m_isMutable = true;

            return that;
        }

        #endregion

        #region Constants

        private static readonly Binary   NO_BYTES = Binary.NO_BINARY;
        private static readonly decimal  DEC_ZERO = decimal.Zero;
        private static readonly DateTime DFT_DATE = DateTime.MinValue;

        #endregion

        #region Data members

        /// <summary>
        /// The <see cref="IXmlElement"/> object that contains this value.
        /// </summary>
        private IXmlElement m_parent;

        /// <summary>
        /// The value of this SimpleValue object.
        /// </summary>
        /// <remarks>
        /// The SimpleValue implementation supports the following types for
        /// this value:
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
        /// </remarks>
        /// All values can convert through string, meaning if necessary, any
        /// of the above types can be converted to string then to any other
        /// of the above types.
        private object m_value;

        /// <summary>
        /// <b>True</b> if an element attribute value, otherwise assumed to
        /// be element content.
        /// </summary>
        private bool m_isAttribute;

        /// <summary>
        /// <b>True</b> if this value is mutable.
        /// </summary>
        private bool m_isMutable = true;

        #endregion
    }
}
