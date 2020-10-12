/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using Tangosol.IO.Pof;

namespace Tangosol.Util
{
    /// <summary>
    /// .NET port of java.util.Optional, a container object which may or may not contain
    /// a non-null value. If a value is present, IsPresent() will return true and Get()
    /// will return the value.
    /// </summary>
    /// <author>as, lh  2015.06.09</author>
    /// <since>Coherence 12.2.1</since>
    public class Optional : IPortableObject
    {
        #region Properties

        /// <summary>
        /// The value of the Optional object; can be null.
        /// </summary>
        /// <value>
        /// The value of the Optional object.
        /// </value>
        public Object Value
        {
            get { return m_value; }
        }

        /// <summary>
        /// Whether there is a value present.
        /// </summary>
        /// <value>
        /// True if there is a value present, otherwise false.
        /// </value>
        public bool IsPresent
        {
            get { return m_value != null; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Optional()
        {
        }

        /// <summary>
        /// Constructs an instance with the value present.
        /// </summary>
        /// <param name="value"> the non-null value to be present
        /// </param>
        /// <exception>
        /// throws ArgumentNullException if value is null
        /// </exception>
        private Optional(Object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            m_value = value;
        }

        #endregion

        /// <summary>
        /// Returns a Optional instance with null object value.
        /// </summary>
        /// <returns>
        /// An instance of Optional object with null value.
        /// </returns>
        public static Optional Empty() 
        {
            Optional t = EMPTY;
            return t;
        }

        /// <summary>
        /// Returns an Optional instance with the given object value.
        /// </summary>
        /// <param name="value">Object to wrap.</param>
        /// <returns>
        /// An instance of Optional with the given object value.
        /// </returns>
        public static Optional Of(Object value)
        {
            return new Optional(value);
        }

        /// <summary>
        /// Returns an <see cref="Optional"/> describing the specified value, if non-null,
        /// otherwise returns an empty <see cref="Optional"/>.
        /// </summary>
        /// <param name="value">
        /// The possibly-null value to describe.
        /// </param>
        /// <returns>
        /// An <see cref="Optional"/> with a present value if the specified value
        /// is non-null, otherwise an empty <see cref="Optional"/>.
        /// </returns>
        public static Optional OfNullable(Object value) 
        {
            return value == null ? Empty() : Of(value);
        }

        /// <summary>
        /// Returns value if present, otherwise, return the given object.
        /// </summary>
        /// <param name="o">The default object to return.</param>
        /// <returns>
        /// Returns value if present, otherwise, return the given object.
        /// </returns>
        public Object OrElse(Object o)
        {
            return m_value ?? o;
        }

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        public void ReadExternal(IPofReader reader)
        {
            bool isPresent = reader.ReadBoolean(0);
            m_value = isPresent ? reader.ReadObject(1) : null;
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteBoolean(0, IsPresent);
            if (IsPresent)
            {
                writer.WriteObject(1, m_value);                
            }
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Returns a non-empty string representation of this Optional suitable
        /// for debugging. The exact presentation format is unspecified and may
        /// vary between implementations and versions.
        /// </summary>
        /// <remarks>
        /// If a value is present the result must include its string
        /// representation in the result. Empty and present Optionals must be
        /// unambiguously differentiable.
        /// </remarks>
        /// <returns>
        /// The string representation of this instance.
        /// </returns>
        public override string ToString()
        {
            return Value != null
                ? String.Format("Optional[%s]", m_value)
                : "Optional.empty";
        }

        /// <summary>
        /// Returns a hash code value for this object.
        /// </summary>
        /// <returns>
        /// A hash code value for this object.
        /// </returns>
        public override int GetHashCode()
        {
            int valueHash = IsPresent ? Value.GetHashCode() : 0;
            return GetType().GetHashCode() ^ valueHash;
        }

        /// <summary>
        /// Compares this object with another object for equality.
        /// </summary>
        /// <param name="o">
        /// An object reference or <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> if the passed object reference is of the same class
        /// and has the same state as this object.
        /// </returns>
        public override bool Equals(object o)
        {
            Optional optional = o as Optional;
            if (optional != null)
            {
                var that = optional;
                return this == that
                       || GetType() == that.GetType()
                       && IsPresent == that.IsPresent
                       && Equals(m_value, that.m_value);
            }
            return false;
        }

        #endregion

        #region Data members

        private static readonly Optional EMPTY = new Optional();

        private Object m_value;

        #endregion
    }
}