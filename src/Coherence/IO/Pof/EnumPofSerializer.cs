/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

namespace Tangosol.IO.Pof
{
    /// <summary><see cref="IPofSerializer"/> implementation that supports
    /// the serialization and deserialization of enum values to and from a 
    /// POF stream.
    /// </summary>
    /// <author>Aleksandar Seovic  2008.10.30</author>
    public class EnumPofSerializer : IPofSerializer
    {
        #region IPofSerializer implementation

        /// <summary>
        /// Serialize an enum instance to a POF stream by writing its
        /// value using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> with which to write the object's state.
        /// </param>
        /// <param name="o">
        /// The object to serialize.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public void Serialize(IPofWriter writer, object o)
        {
            if (o == null || !o.GetType().IsEnum)
            {
                throw new ArgumentException(
                        "EnumPofSerializer can only be used to serialize enum types.");
            }

            writer.WriteString(0, o.ToString());
            writer.WriteRemainder(null);
        }

        /// <summary>
        /// Deserialize an enum instance from a POF stream by reading its
        /// value using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> with which to read the object's state.
        /// </param>
        /// <returns>
        /// The deserialized enum instance.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public object Deserialize(IPofReader reader)
        {
            IPofContext pofContext = reader.PofContext;
            Type        enumType   = pofContext.GetType(reader.UserTypeId);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(
                        "EnumPofSerializer can only be used to deserialize enum types.");
            }

            object enumValue = Enum.Parse(enumType, reader.ReadString(0));
            reader.RegisterIdentity(enumValue);
            reader.ReadRemainder();

            return enumValue;
        }

        #endregion
    }
}
