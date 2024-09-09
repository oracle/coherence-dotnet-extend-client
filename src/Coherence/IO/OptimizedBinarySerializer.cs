/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tangosol.IO
{
    /// <summary>
    /// <see cref="ISerializer"/> implementation that optimizes serialization 
    /// of primitive types and falls back to .NET BinaryFormatter for
    /// custom types.
    /// </summary>
    /// <remarks>
    /// <para>
    /// As of 14.1.2.0, this class is deprecated as it relies on a
    /// deprecated <see cref="BinarySerializer"/>
    /// </para>
    /// </remarks>
    /// <author>Aleksandar Seovic  2010.03.17</author>
    /// <since>Coherence 3.6</since>
    [Obsolete("since Coherence 14.1.2.0")]
    public class OptimizedBinarySerializer : ISerializer
    {
        #region Implementation of ISerializer

        /// <summary>
        /// Serialize an object to a stream by writing its state using the
        /// specified <see cref="DataWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>DataWriter</b> with which to write the object's state.
        /// </param>
        /// <param name="o">
        /// The object to serialize.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public void Serialize(DataWriter writer, Object o)
        {
            TypeId typeId = GetTypeId(o);
            writer.Write((Byte) typeId);
            
            switch (typeId)
            {
                case TypeId.NULL:
                    break;

                case TypeId.BYTE:
                    writer.Write((Byte) o);
                    break;

                case TypeId.CHAR:
                    writer.Write((Char) o);
                    break;
                
                case TypeId.STRING:
                    writer.Write((String) o);
                    break;
                
                case TypeId.BOOL:
                    writer.Write((Boolean) o);
                    break;
                
                case TypeId.INT16:
                    writer.Write((Int16) o);
                    break;
                
                case TypeId.INT32:
                    writer.WritePackedInt32((Int32) o);
                    break;
                
                case TypeId.INT64:
                    writer.WritePackedInt64((Int64) o);
                    break;
                
                case TypeId.SINGLE:
                    writer.Write((Single) o);
                    break;
                
                case TypeId.DOUBLE:
                    writer.Write((Double) o);
                    break;
                
                case TypeId.DECIMAL:
                    int[] bits = Decimal.GetBits((Decimal) o);
                    for (int i = 0; i < 4; i++)
                    {
                        writer.WritePackedInt32(bits[i]);
                    }
                    break;
                
                case TypeId.DATETIME:
                    DateTime dt = (DateTime) o;
                    writer.WritePackedInt64(dt.Ticks);
                    break;
                
                case TypeId.TIMESPAN:
                    TimeSpan ts = (TimeSpan) o;
                    writer.WritePackedInt64(ts.Ticks);
                    break;
                
                case TypeId.GUID:
                    byte[] bytes = ((Guid) o).ToByteArray();
                    writer.Write(bytes);
                    break;
                
                case TypeId.BYTE_ARRAY:
                    byte[] arrBytes = (Byte[]) o;
                    writer.WritePackedInt32(arrBytes.Length);
                    writer.Write(arrBytes);
                    break;
                
                default:
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(writer.BaseStream, o);
                    break;
            }
        }

        /// <summary>
        /// Deserialize an object from a stream by reading its state using
        /// the specified <see cref="DataReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>DataReader</b> with which to read the object's state.
        /// </param>
        /// <returns>
        /// The deserialized user type instance.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public object Deserialize(DataReader reader)
        {
            TypeId typeId = (TypeId) Enum.ToObject(typeof(TypeId), reader.ReadByte());

            switch (typeId)
            {
                case TypeId.NULL:
                    return null;
                
                case TypeId.BYTE:
                    return reader.ReadByte();
                
                case TypeId.CHAR:
                    return reader.ReadChar();
                
                case TypeId.STRING:
                    return reader.ReadString();
                
                case TypeId.BOOL:
                    return reader.ReadBoolean();

                case TypeId.INT16:
                    return reader.ReadInt16();

                case TypeId.INT32:
                    return reader.ReadPackedInt32();

                case TypeId.INT64:
                    return reader.ReadPackedInt64();

                case TypeId.SINGLE:
                    return reader.ReadSingle();

                case TypeId.DOUBLE:
                    return reader.ReadDouble();
                
                case TypeId.DECIMAL:
                    int[] bits = new int[4];
                    for (int i = 0; i < 4; i++)
                    {
                        bits[i] = reader.ReadPackedInt32();
                    }
                    return new Decimal(bits);
                
                case TypeId.DATETIME:
                    return new DateTime(reader.ReadPackedInt64());

                case TypeId.TIMESPAN:
                    return new TimeSpan(reader.ReadPackedInt64());

                case TypeId.GUID:
                    return new Guid(reader.ReadBytes(0x10));

                case TypeId.BYTE_ARRAY:
                    int cb = reader.ReadPackedInt32();
                    return reader.ReadBytes(cb);

                default:
                    BinaryFormatter formatter = new BinaryFormatter();
                    return formatter.Deserialize(reader.BaseStream);
            }
        }

        #endregion

        #region Helper methods

        private static TypeId GetTypeId(object o)
        {
            return o == null        ? TypeId.NULL
                    : o is byte     ? TypeId.BYTE
                    : o is char     ? TypeId.CHAR
                    : o is string   ? TypeId.STRING
                    : o is bool     ? TypeId.BOOL
                    : o is Int16    ? TypeId.INT16
                    : o is Int32    ? TypeId.INT32
                    : o is Int64    ? TypeId.INT64
                    : o is Single   ? TypeId.SINGLE
                    : o is Double   ? TypeId.DOUBLE
                    : o is Decimal  ? TypeId.DECIMAL
                    : o is DateTime ? TypeId.DATETIME
                    : o is TimeSpan ? TypeId.TIMESPAN
                    : o is Guid     ? TypeId.GUID
                    : o is Byte[]   ? TypeId.BYTE_ARRAY
                    : TypeId.USER_TYPE;
        }

        #endregion

        #region TypeId enumeration

        private enum TypeId : byte
        {
            NULL, 
            BYTE, 
            CHAR, 
            STRING, 
            BOOL, 
            INT16, 
            INT32, 
            INT64, 
            SINGLE, 
            DOUBLE, 
            DECIMAL, 
            DATETIME, 
            TIMESPAN, 
            GUID,
            BYTE_ARRAY, 
            USER_TYPE
        }

        #endregion
    }
}