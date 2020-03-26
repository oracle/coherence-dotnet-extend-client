/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Tangosol.Util;
using Tangosol.Util.Collections;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// <see cref="IPofReader"/> implementation that reads POF-encoded data
    /// from a <see cref="DataReader"/>.
    /// </summary>
    /// <author>Cameron Purdy  2006.07.14</author>
    /// <author>Aleksandar Seovic  2006.08.08</author>
    /// <author>Ivan Cikic  2006.08.09</author>
    /// <since>Coherence 3.2</since>
    public class PofStreamReader : IPofReader
    {
        #region Constructors

        /// <summary>
        /// Construct a POF parser that will pull values from the specified
        /// stream.
        /// </summary>
        /// <param name="reader">
        /// A <see cref="DataReader"/> object.
        /// </param>
        /// <param name="ctx">
        /// The <see cref="IPofContext"/>.
        /// </param>
        public PofStreamReader(DataReader reader, IPofContext ctx)
        {
            m_reader = reader;
            m_ctx    = ctx;
        }

        /// <summary>
        /// Construct a POF parser.
        /// </summary>
        protected internal PofStreamReader()
        {}

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IPofContext"/> object used by this
        /// <b>PofStreamReader</b> to deserialize user types from a POF
        /// stream.
        /// </summary>
        /// <remarks>
        /// This is an advanced propertie that should be used with
        /// care.
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
        public virtual IPofContext PofContext
        {
            get { return m_ctx; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                m_ctx = value;
            }
        }

        /// <summary>
        /// Gets the user type that is currently being parsed.
        /// </summary>
        /// <value>
        /// The user type information, or -1 if the <b>PofStreamReader</b> is
        /// not currently parsing a user type.
        /// </value>
        public virtual int UserTypeId
        {
            get { return -1; }
        }

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
        public virtual int VersionId
        {
            get { throw new InvalidOperationException("Not in a user type."); }
        }

        /// <summary>
        /// If this parser is contextually within a user type, obtain the
        /// parser which created this parser in order to parse the user type.
        /// </summary>
        /// <value>
        /// The parser for the context within which this parser is operating.
        /// </value>
        protected internal virtual PofStreamReader ParentParser
        {
            get { return null; }
        }

        #endregion

        #region IPofReader interface implementation

        #region Primitive values

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
        public virtual Boolean ReadBoolean(int index)
        {
            return ReadInt32(index) != 0;
        }

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
        public virtual Byte ReadByte(int index)
        {
            return (Byte) ReadInt32(index);
        }

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
        public virtual char ReadChar(int index)
        {
            return (Char) ReadInt32(index);
        }

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
        public virtual Int16 ReadInt16(int index)
        {
            return (Int16) ReadInt32(index);
        }

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
        public virtual Int32 ReadInt32(int index)
        {
            int n = 0;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            n = ReadInt32(index);
                            RegisterIdentity(id, n);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        object number = LookupIdentity(reader.ReadPackedInt32());
                        if (number != null)
                        {
                            n = Convert.ToInt32(number);
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                    case PofConstants.V_INT_0:
                        break;

                    default:
                        n = PofHelper.ReadAsInt32(reader, typeId);
                        break;
                }
            }
            Complete(index);

            return n;
        }

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
        public virtual Int64 ReadInt64(int index)
        {
            long n = 0L;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            n = ReadInt64(index);
                            RegisterIdentity(id, n);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        object number = LookupIdentity(reader.ReadPackedInt32());
                        if (number != null)
                        {
                            n = Convert.ToInt64(number);
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                    case PofConstants.V_INT_0:
                        break;

                    default:
                        n = PofHelper.ReadAsInt64(reader, typeId);
                        break;
                }
            }
            Complete(index);

            return n;
        }

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
        public virtual RawInt128 ReadRawInt128(int index)
        {
            RawInt128 n = new RawInt128(new byte[16]);

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            n = ReadRawInt128(index);
                            RegisterIdentity(id, n);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        {
                            //object number = LookupIdentity(reader.ReadPackedInt32());
                            //if (number != null)
                            //{
                            //    n = PofHelper.ConvertNumber(number, PofConstants.T_INT128);
                            //}
                            //TODO: what is the result of LookupIdentity?
                            throw new NotSupportedException("reference is not supported for T_INT128");
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        n = new RawInt128((byte[])null);
                        break;

                    case PofConstants.V_INT_0:
                        break;

                    default:
                        n = PofHelper.ReadAsRawInt128(reader, typeId);
                        break;
                }
            }
            Complete(index);

            return n;
        }

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
        public virtual Single ReadSingle(int index)
        {
            float fl = 0.0F;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            fl = ReadSingle(index);
                            RegisterIdentity(id, fl);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        object number = LookupIdentity(reader.ReadPackedInt32());
                        if (number != null)
                        {
                            fl = Convert.ToSingle(number);
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                    case PofConstants.V_INT_0:
                        break;

                    default:
                        fl = PofHelper.ReadAsSingle(reader, typeId);
                        break;
                }
            }
            Complete(index);

            return fl;
        }

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
        public virtual Double ReadDouble(int index)
        {
            double dfl = 0.0;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            dfl = ReadDouble(index);
                            RegisterIdentity(id, dfl);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        object number = LookupIdentity(reader.ReadPackedInt32());
                        if (number != null)
                        {
                            dfl = Convert.ToDouble(number);
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                    case PofConstants.V_INT_0:
                        break;

                    default:
                        dfl = PofHelper.ReadAsDouble(reader, typeId);
                        break;
                }
            }
            Complete(index);

            return dfl;
        }

        #endregion

        #region Primitive arrays

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
        public virtual bool[] ReadBooleanArray(int index)
        {
            bool[] af = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            af = ReadBooleanArray(index);
                            RegisterIdentity(id, af);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        af = (bool[]) LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.V_STRING_ZERO_LENGTH:
                    case PofConstants.V_COLLECTION_EMPTY:
                        af = PofHelper.BOOLEAN_ARRAY_EMPTY;
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            af = new bool[elements];
                            for (int i = 0; i < elements; ++i)
                            {
                                af[i] = PofHelper.ReadAsInt32(reader, reader.ReadPackedInt32()) != 0;
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            af = new bool[elements];
                            switch (elementType)
                            {
                                case PofConstants.T_BOOLEAN:
                                case PofConstants.T_INT16:
                                case PofConstants.T_INT32:
                                case PofConstants.T_INT64:
                                case PofConstants.T_INT128:
                                    for (int i = 0; i < elements; ++i)
                                    {
                                        af[i] = reader.ReadPackedInt32() != 0;
                                    }
                                    break;


                                default:
                                    for (int i = 0; i < elements; ++i)
                                    {
                                        af[i] = PofHelper.ReadAsInt32(reader, elementType) != 0;
                                    }
                                    break;
                            }
                        }
                        break;


                    case PofConstants.T_SPARSE_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            af = new bool[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                af[element] = PofHelper.ReadAsInt32(reader, reader.ReadPackedInt32()) != 0;
                            } while (--elements >= 0);
                        }
                        break;


                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            af = new bool[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                af[element] = PofHelper.ReadAsInt32(reader, elementType) != 0;
                            } while (--elements >= 0);
                        }
                        break;


                    default:
                        throw new IOException("unable to convert type " + typeId + " to an array type");
                }
            }
            Complete(index);

            return af;
        }

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
        public virtual byte[] ReadByteArray(int index)
        {
            byte[] ab = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            ab = ReadByteArray(index);
                            RegisterIdentity(id, ab);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        {
                            object o = LookupIdentity(reader.ReadPackedInt32());
                            ab = o is Binary ? ((Binary) o).ToByteArray() : (byte[]) o;
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.V_STRING_ZERO_LENGTH:
                    case PofConstants.V_COLLECTION_EMPTY:
                        ab = PofHelper.BYTE_ARRAY_EMPTY;
                        break;

                    case PofConstants.T_OCTET_STRING:
                        ab = new byte[reader.ReadPackedInt32()];
                        reader.Read(ab, 0, ab.Length);
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            ab = new byte[elements];
                            for (int i = 0; i < elements; ++i)
                            {
                                ab[i] = (byte) PofHelper.ReadAsInt32(reader, reader.ReadPackedInt32());
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            ab = new byte[elements];
                            for (int i = 0; i < elements; ++i)
                            {
                                ab[i] = elementType == PofConstants.T_OCTET
                                            ? reader.ReadByte()
                                            : (byte) PofHelper.ReadAsInt32(reader, elementType);
                            }
                        }
                        break;

                    case PofConstants.T_SPARSE_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            ab = new byte[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                ab[element] = (byte) PofHelper.ReadAsInt32(reader, reader.ReadPackedInt32());
                            } while (--elements >= 0);
                        }
                        break;

                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            ab = new byte[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                ab[element] = elementType == PofConstants.T_OCTET
                                                   ? reader.ReadByte()
                                                   : (byte) PofHelper.ReadAsInt32(reader, elementType);
                            } while (--elements >= 0);
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to an array type");
                }
            }
            Complete(index);

            return ab;
        }

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
        public virtual char[] ReadCharArray(int index)
        {
            char[] ach = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            ach = ReadCharArray(index);
                            RegisterIdentity(id, ach);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        {
                            object o = LookupIdentity(reader.ReadPackedInt32());
                            ach = o is string ? ((string) o).ToCharArray() : (char[]) o;
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.V_STRING_ZERO_LENGTH:
                    case PofConstants.V_COLLECTION_EMPTY:
                        ach = PofHelper.CHAR_ARRAY_EMPTY;
                        break;

                    case PofConstants.T_OCTET_STRING:
                        {
                            int cb = reader.ReadPackedInt32();
                            byte[] ab = new byte[cb];
                            reader.Read(ab, 0, cb);

                            ach = new char[cb];
                            for (int of = 0; of < cb; ++cb)
                            {
                                ach[of] = (char) (ab[of] & 0xFF);
                            }
                        }
                        break;

                    case PofConstants.T_CHAR_STRING:
                        ach = reader.ReadString().ToCharArray();
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            ach = new char[elements];
                            for (int i = 0; i < elements; ++i)
                            {
                                ach[i] = (char) PofHelper.ReadAsInt32(reader, reader.ReadPackedInt32());
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            ach = new char[elements];
                            for (int i = 0; i < elements; ++i)
                            {
                                ach[i] = elementType == PofConstants.T_CHAR
                                             ? PofHelper.ReadChar(reader)
                                             : (char) PofHelper.ReadAsInt32(reader, elementType);
                            }
                        }
                        break;

                    case PofConstants.T_SPARSE_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            ach = new char[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                ach[element] = (char) PofHelper.ReadAsInt32(reader, reader.ReadPackedInt32());
                            } while (--elements >= 0);
                        }
                        break;

                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            ach = new char[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                ach[element] = elementType == PofConstants.T_CHAR
                                                    ? PofHelper.ReadChar(reader)
                                                    : (char) PofHelper.ReadAsInt32(reader, elementType);
                            } while (--elements >= 0);
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to an array type");
                }
            }
            Complete(index);

            return ach;
        }

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
        public virtual Int16[] ReadInt16Array(int index)
        {
            short[] an = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            an = ReadInt16Array(index);
                            RegisterIdentity(id, an);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        an = (short[]) LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.V_COLLECTION_EMPTY:
                        an = PofHelper.INT16_ARRAY_EMPTY;
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            an = new short[elements];
                            for (int i = 0; i < elements; ++i)
                            {
                                an[i] = (short) PofHelper.ReadAsInt32(reader, reader.ReadPackedInt32());
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            an = new short[elements];
                            switch (elementType)
                            {
                                case PofConstants.T_INT16:
                                case PofConstants.T_INT32:
                                case PofConstants.T_INT64:
                                case PofConstants.T_INT128:
                                    for (int i = 0; i < elements; ++i)
                                    {
                                        an[i] = (short) reader.ReadPackedInt32();
                                    }
                                    break;

                                default:
                                    for (int i = 0; i < elements; ++i)
                                    {
                                        an[i] = (short) PofHelper.ReadAsInt32(reader, elementType);
                                    }
                                    break;
                            }
                        }
                        break;

                    case PofConstants.T_SPARSE_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            an = new short[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                an[element] = (short) PofHelper.ReadAsInt32(reader, reader.ReadPackedInt32());
                            } while (--elements >= 0);
                        }
                        break;

                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            an = new short[elements];
                            switch (elementType)
                            {
                                case PofConstants.T_INT16:
                                case PofConstants.T_INT32:
                                case PofConstants.T_INT64:
                                case PofConstants.T_INT128:
                                    do
                                    {
                                        int element = reader.ReadPackedInt32();
                                        if (element < 0)
                                        {
                                            break;
                                        }
                                        an[element] = (short) reader.ReadPackedInt32();
                                    } while (--elements >= 0);
                                    break;
                                default:
                                    do
                                    {
                                        int element = reader.ReadPackedInt32();
                                        if (element < 0)
                                        {
                                            break;
                                        }
                                        an[element] = (short) PofHelper.ReadAsInt32(reader, elementType);
                                    } while (--elements >= 0);
                                    break;
                            }
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to an array type");
                }
            }
            Complete(index);

            return an;
        }

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
        public virtual Int32[] ReadInt32Array(int index)
        {
            int[] an = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            an = ReadInt32Array(index);
                            RegisterIdentity(id, an);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        an = (int[]) LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.V_COLLECTION_EMPTY:
                        an = PofHelper.INT32_ARRAY_EMPTY;
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            an = new int[elements];
                            for (int i = 0; i < elements; ++i)
                            {
                                an[i] = PofHelper.ReadAsInt32(reader, reader.ReadPackedInt32());
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            an = new int[elements];
                            switch (elementType)
                            {
                                case PofConstants.T_INT16:
                                case PofConstants.T_INT32:
                                case PofConstants.T_INT64:
                                case PofConstants.T_INT128:
                                    for (int i = 0; i < elements; ++i)
                                    {
                                        an[i] = reader.ReadPackedInt32();
                                    }
                                    break;

                                default:
                                    for (int i = 0; i < elements; ++i)
                                    {
                                        an[i] = PofHelper.ReadAsInt32(reader, elementType);
                                    }
                                    break;
                            }
                        }
                        break;

                    case PofConstants.T_SPARSE_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            an = new int[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                an[element] = PofHelper.ReadAsInt32(reader, reader.ReadPackedInt32());
                            } while (--elements >= 0);
                        }
                        break;

                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            an = new int[elements];
                            switch (elementType)
                            {
                                case PofConstants.T_INT16:
                                case PofConstants.T_INT32:
                                case PofConstants.T_INT64:
                                case PofConstants.T_INT128:
                                    do
                                    {
                                        int element = reader.ReadPackedInt32();
                                        if (element < 0)
                                        {
                                            break;
                                        }
                                        an[element] = reader.ReadPackedInt32();
                                    } while (--elements >= 0);
                                    break;

                                default:
                                    do
                                    {
                                        int element = reader.ReadPackedInt32();
                                        if (element < 0)
                                        {
                                            break;
                                        }
                                        an[element] = PofHelper.ReadAsInt32(reader, elementType);
                                    } while (--elements >= 0);
                                    break;
                            }
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to an array type");
                }
            }
            Complete(index);

            return an;
        }

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
        public virtual Int64[] ReadInt64Array(int index)
        {
            long[] an = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            an = ReadInt64Array(index);
                            RegisterIdentity(id, an);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        an = (long[]) LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.V_COLLECTION_EMPTY:
                        an = PofHelper.INT64_ARRAY_EMPTY;
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            an = new long[elements];
                            for (int i = 0; i < elements; ++i)
                            {
                                an[i] = PofHelper.ReadAsInt64(reader, reader.ReadPackedInt32());
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            an = new long[elements];
                            switch (elementType)
                            {
                                case PofConstants.T_INT16:
                                case PofConstants.T_INT32:
                                case PofConstants.T_INT64:
                                case PofConstants.T_INT128:
                                    for (int i = 0; i < elements; ++i)
                                    {
                                        an[i] = reader.ReadPackedInt64();
                                    }
                                    break;

                                default:
                                    for (int i = 0; i < elements; ++i)
                                    {
                                        an[i] = PofHelper.ReadAsInt64(reader, elementType);
                                    }
                                    break;
                            }
                        }
                        break;

                    case PofConstants.T_SPARSE_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            an = new long[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                an[element] = PofHelper.ReadAsInt64(reader, reader.ReadPackedInt32());
                            } while (--elements >= 0);
                        }
                        break;

                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            an = new long[elements];
                            switch (elementType)
                            {
                                case PofConstants.T_INT16:
                                case PofConstants.T_INT32:
                                case PofConstants.T_INT64:
                                case PofConstants.T_INT128:
                                    do
                                    {
                                        int element = reader.ReadPackedInt32();
                                        if (element < 0)
                                        {
                                            break;
                                        }
                                        an[element] = reader.ReadPackedInt64();
                                    } while (--elements >= 0);
                                    break;

                                default:
                                    do
                                    {
                                        int element = reader.ReadPackedInt32();
                                        if (element < 0)
                                        {
                                            break;
                                        }
                                        an[element] = PofHelper.ReadAsInt64(reader, elementType);
                                    } while (--elements >= 0);
                                    break;
                            }
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to an array type");
                }
            }
            Complete(index);

            return an;
        }

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
        public virtual Single[] ReadSingleArray(int index)
        {
            float[] afl = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            afl = ReadSingleArray(index);
                            RegisterIdentity(id, afl);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        afl = (float[]) LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.V_COLLECTION_EMPTY:
                        afl = PofHelper.SINGLE_ARRAY_EMPTY;
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            afl = new float[elements];
                            for (int i = 0; i < elements; ++i)
                            {
                                afl[i] = PofHelper.ReadAsSingle(reader, reader.ReadPackedInt32());
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            afl = new float[elements];
                            for (int i = 0; i < elements; ++i)
                            {
                                afl[i] = elementType == PofConstants.T_FLOAT32
                                             ? reader.ReadSingle()
                                             : PofHelper.ReadAsSingle(reader, elementType);
                            }
                        }
                        break;

                    case PofConstants.T_SPARSE_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            afl = new float[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                afl[element] = PofHelper.ReadAsSingle(reader, reader.ReadPackedInt32());
                            } while (--elements >= 0);
                        }
                        break;

                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            afl = new float[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                afl[element] = elementType == PofConstants.T_FLOAT32
                                             ? reader.ReadSingle()
                                             : PofHelper.ReadAsSingle(reader, elementType);
                            } while (--elements >= 0);
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to an array type");
                }
            }
            Complete(index);

            return afl;
        }

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
        public virtual Double[] ReadDoubleArray(int index)
        {
            double[] adfl = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            adfl = ReadDoubleArray(index);
                            RegisterIdentity(id, adfl);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        adfl = (double[]) LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.V_COLLECTION_EMPTY:
                        adfl = PofHelper.DOUBLE_ARRAY_EMPTY;
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            adfl = new double[elements];
                            for (int i = 0; i < elements; ++i)
                            {
                                adfl[i] = PofHelper.ReadAsDouble(reader, reader.ReadPackedInt32());
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            adfl = new double[elements];
                            for (int i = 0; i < elements; ++i)
                            {
                                adfl[i] = elementType == PofConstants.T_FLOAT64
                                              ? reader.ReadDouble()
                                              : PofHelper.ReadAsDouble(reader, elementType);
                            }
                        }
                        break;

                    case PofConstants.T_SPARSE_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            adfl = new double[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                adfl[element] = PofHelper.ReadAsDouble(reader, reader.ReadPackedInt32());
                            } while (--elements >= 0);
                        }
                        break;

                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            adfl = new double[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                adfl[element] = elementType == PofConstants.T_FLOAT64
                                              ? reader.ReadDouble()
                                              : PofHelper.ReadAsDouble(reader, elementType);
                            } while (--elements >= 0);
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to an array type");
                }
            }
            Complete(index);

            return adfl;
        }

        #endregion

        #region Object values

        // TODO: add support for RawQuad
        // TODO: add support for Binary

        /// <summary>
        /// Read a <b>Decimal</b> from the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <returns>
        /// The <b>Decimal</b> property value, or null if no value was
        /// available in the POF stream
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual Decimal ReadDecimal(int index)
        {
            Decimal dec = Decimal.Zero;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            dec    = ReadDecimal(index);
                            RegisterIdentity(id, dec);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        {
                            object number = LookupIdentity(reader.ReadPackedInt32());
                            dec           = (Decimal) PofHelper.ConvertNumber(number, PofConstants.T_DECIMAL128);
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                    case PofConstants.V_INT_0:
                        break;

                    default:
                        dec = PofHelper.ReadAsDecimal(reader, typeId);
                        break;
                }
            }
            Complete(index);

            return dec;
        }

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
        public virtual string ReadString(int index)
        {
            string s = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            s = ReadString(index);
                            RegisterIdentity(id, s);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        {
                            object o = LookupIdentity(reader.ReadPackedInt32());
                            if (o is char[])
                            {
                                s = new string((char[]) o);
                            }
                            else if (o is Binary)
                            {
                                byte[] bytes = ((Binary) o).ToByteArray();
                                s = SerializationHelper.ConvertUTF(bytes, 0, bytes.Length);
                            }
                            else
                            {
                                s = ((string) o);
                            }
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.V_STRING_ZERO_LENGTH:
                    case PofConstants.V_COLLECTION_EMPTY:
                        s = "";
                        break;

                    case PofConstants.T_OCTET_STRING:
                        {
                            int    cb    = reader.ReadPackedInt32();
                            byte[] bytes = new byte[cb];
                            reader.Read(bytes, 0, cb);
                            s = SerializationHelper.ConvertUTF(bytes, 0, cb);
                        }
                        break;

                    case PofConstants.T_CHAR_STRING:
                        s = reader.ReadString();
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            int    cch = reader.ReadPackedInt32();
                            char[] ach = new char[cch];
                            for (int i = 0; i < cch; ++i)
                            {
                                ach[i] = PofHelper.ReadAsChar(reader, reader.ReadPackedInt32());
                            }
                            s = new string(ach);
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            int    elementType = reader.ReadPackedInt32();
                            int    cch         = reader.ReadPackedInt32();
                            char[] ach         = new char[cch];
                            for (int i = 0; i < cch; ++i)
                            {
                                ach[i] = PofHelper.ReadAsChar(reader, elementType);
                            }
                            s = new string(ach);
                        }
                        break;

                    case PofConstants.T_SPARSE_ARRAY:
                        {
                            int    elements = reader.ReadPackedInt32();
                            char[] ach      = new char[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                ach[element] = PofHelper.ReadAsChar(reader, reader.ReadPackedInt32());
                            } while (--elements >= 0);
                            s = new string(ach);
                        }
                        break;

                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        {
                            int    elementType = reader.ReadPackedInt32();
                            int    elements    = reader.ReadPackedInt32();
                            char[] ach         = new char[elements];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                ach[element] = PofHelper.ReadAsChar(reader, elementType);
                            } while (--elements >= 0);
                            s = new string(ach);
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to a Binary type");
                }
            }
            Complete(index);

            return s;
        }

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
        public virtual DateTime ReadDateTime(int index)
        {
            DateTime date = DateTime.MinValue;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            date = ReadDateTime(index);
                            RegisterIdentity(id, date);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        date = (DateTime) LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.T_DATE:
                        date = PofHelper.ReadDate(reader);
                        break;

                    case PofConstants.T_TIME:
                        date = PofHelper.ReadRawTime(reader).ToDateTime();
                        break;

                    case PofConstants.T_DATETIME:
                        date = PofHelper.ReadDateTime(reader);
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to a .NET DateTime type");
                }
            }
            Complete(index);

            return date;
        }

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
        public DateTime ReadLocalDateTime(int index)
        {
            DateTime utcValue = ReadUniversalDateTime(index);
            return utcValue == DateTime.MinValue 
                ? DateTime.MinValue 
                : utcValue.ToLocalTime();
        }

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
        public DateTime ReadUniversalDateTime(int index)
        {
            DateTime date = DateTime.MinValue;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            date = ReadUniversalDateTime(index);
                            RegisterIdentity(id, date);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        date = (DateTime) LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.T_DATE:
                        date = PofHelper.ReadDate(reader);
                        break;

                    case PofConstants.T_TIME:
                        date = PofHelper.ReadRawTime(reader).ToUniversalTime();
                        break;

                    case PofConstants.T_DATETIME:
                        date = PofHelper.ReadUniversalDateTime(reader);
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to a .NET DateTime type");
                }
            }
            Complete(index);

            return date;
        }

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
        public virtual RawDateTime ReadRawDateTime(int index)
        {
            RawDateTime datetime = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            datetime = ReadRawDateTime(index);
                            RegisterIdentity(id, datetime);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        {
                            object o = LookupIdentity(reader.ReadPackedInt32());
                            if (o is DateTime)
                            {
                                DateTime dt = (DateTime) o;
                                datetime = new RawDateTime(
                                    new DateTime(dt.Year, dt.Month, dt.Day),
                                    new RawTime(dt.Hour, dt.Minute, dt.Second, dt.Millisecond * 1000000,
                                    false));
                            }
                            else if (o is RawDateTime)
                            {
                                datetime = (RawDateTime) o;
                            }
                            else
                            {
                                throw new NotSupportedException("only DateTime and RawDateTime types are supported at the moment.");
                            }
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.T_DATE:
                        datetime = new RawDateTime(PofHelper.ReadDate(reader), new RawTime(0, 0, 0, 0, false));
                        break;

                    case PofConstants.T_DATETIME:
                        datetime = new RawDateTime(PofHelper.ReadDate(reader), PofHelper.ReadRawTime(reader));
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to a RawDateTime type");
                }
            }
            Complete(index);

            return datetime;
        }

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
        public virtual DateTime ReadDate(int index)
        {
            DateTime date = DateTime.MinValue;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            date = ReadDate(index);
                            RegisterIdentity(id, date);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        {
                            object o = LookupIdentity(reader.ReadPackedInt32());
                            if (o is DateTime)
                            {
                                date = (DateTime) o;
                            }
                            else if (o is RawTime)
                            {
                                date = ((RawTime) o).ToDateTime();
                            }
                            else if (o is RawDateTime)
                            {
                                date = ((RawDateTime) o).Date;
                            }
                            else
                            {
                                throw new NotSupportedException("only DateTime, RawTime and RawDateTime types are supported at the moment.");
                            }
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.T_DATE:
                        date = PofHelper.ReadDate(reader);
                        break;

                    case PofConstants.T_DATETIME:
                        {
                            // read the date portion
                            date = PofHelper.ReadDate(reader);

                            // skip the time portion
                            PofHelper.SkipPackedInts(reader, 4);
                            int zoneType = reader.ReadPackedInt32();
                            if (zoneType == 2)
                            {
                                PofHelper.SkipPackedInts(reader, 2);
                            }
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to a RawDate type");
                }
            }
            Complete(index);

            return date;
        }

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
        public virtual RawTime ReadRawTime(int index)
        {
            RawTime rawTime = null;
            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            rawTime = ReadRawTime(index);
                            RegisterIdentity(id, rawTime);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        {
                            object o = LookupIdentity(reader.ReadPackedInt32());
                            if (o is DateTime)
                            {
                                DateTime dt = (DateTime) o;
                                return new RawTime(dt.Hour, dt.Minute, dt.Second, dt.Millisecond * 1000000, false);
                            }
                            if (o is RawTime)
                            {
                                return (RawTime) o;
                            }
                            if (o is RawDateTime)
                            {
                                return ((RawDateTime) o).Time;
                            }
                            
                            throw new NotSupportedException("only DateTime, RawTime and RawDateTime types are supported at the moment.");
                        }

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.T_DATETIME:
                        // skip the date portion
                        PofHelper.SkipPackedInts(reader, 3);
                        // fall through
                        goto case PofConstants.T_TIME;

                    case PofConstants.T_TIME:
                        rawTime = PofHelper.ReadRawTime(reader);
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to a Time type");
                }
            }
            Complete(index);

            return rawTime;
        }

        /// <summary>
        /// Read a year-month interval from the POF stream.
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
        /// The <b>YearMonthInterval</b> property value, or <c>null</c> if no
        /// value was available in the POF stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual RawYearMonthInterval ReadRawYearMonthInterval(int index)
        {
            RawYearMonthInterval interval = new RawYearMonthInterval();

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            interval = ReadRawYearMonthInterval(index);
                            RegisterIdentity(id, interval);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        interval = (RawYearMonthInterval) LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.T_YEAR_MONTH_INTERVAL:
                        {
                            int years  = reader.ReadPackedInt32();
                            int months = reader.ReadPackedInt32();
                            interval = new RawYearMonthInterval(years, months);
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to a YearMonthInterval type");
                }
            }
            Complete(index);

            return interval;
        }

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
        public virtual TimeSpan ReadTimeInterval(int index)
        {
            TimeSpan interval = new TimeSpan();

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {

                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            interval = ReadTimeInterval(index);
                            RegisterIdentity(id, interval);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        interval = (TimeSpan) LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.T_TIME_INTERVAL:
                        {
                            int hours   = reader.ReadPackedInt32();
                            int minutes = reader.ReadPackedInt32();
                            int seconds = reader.ReadPackedInt32();
                            int nanos   = reader.ReadPackedInt32();
                            interval = new TimeSpan(0, hours, minutes, seconds, nanos / 1000000);
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to a TimeSpan type");
                }
            }
            Complete(index);

            return interval;
        }

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
        public virtual TimeSpan ReadDayTimeInterval(int index)
        {
            TimeSpan interval = new TimeSpan();

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            interval = ReadTimeInterval(index);
                            RegisterIdentity(id, interval);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        interval = (TimeSpan)LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.T_DAY_TIME_INTERVAL:
                        {
                            int days    = reader.ReadPackedInt32();
                            int hours   = reader.ReadPackedInt32();
                            int minutes = reader.ReadPackedInt32();
                            int seconds = reader.ReadPackedInt32();
                            int nanos   = reader.ReadPackedInt32();
                            interval = new TimeSpan(days, hours, minutes, seconds, nanos / 1000000);
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to a TimeSpan type");
                }
            }
            Complete(index);

            return interval;
        }

        /// <summary>
        /// Read a property of any type, including a user type, from the POF
        /// stream.
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
        public virtual object ReadObject(int index)
        {
            object o = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            IdentityHolder.Set(this, id);
                            o = ReadObject(index);
                            IdentityHolder.Reset(this, id, o);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        o = LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    default:
                        o = ReadAsObject(typeId);
                        break;
                }
            }
            Complete(index);

            return o;
        }

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
        public virtual Binary ReadBinary(int index)
        {
           Binary bin = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            bin = ReadBinary(index);
                            RegisterIdentity(id, bin);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        {
                            object o = LookupIdentity(reader.ReadPackedInt32());
                            bin = o is byte[] ? new Binary((byte[]) o) : (Binary) o;
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.V_STRING_ZERO_LENGTH:
                    case PofConstants.V_COLLECTION_EMPTY:
                        bin = PofHelper.BINARY_EMPTY;
                        break;

                    case PofConstants.T_OCTET_STRING:
                        bin = ReadBinary(reader);
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            int    cb = reader.ReadPackedInt32();
                            byte[] ab = new byte[cb];
                            for (int i = 0; i < cb; ++i)
                            {
                                ab[i] = (byte) PofHelper.ReadAsInt32(reader, reader.ReadPackedInt32());
                            }
                            bin = new Binary(ab);
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            int    elementType = reader.ReadPackedInt32();
                            int    cb          = reader.ReadPackedInt32();
                            byte[] ab          = new byte[cb];

                            if (elementType == PofConstants.T_OCTET)
                            {
                                reader.Read(ab, 0, ab.Length);
                            }
                            else
                            {
                                for (int i = 0; i < cb; ++i)
                                {
                                    ab[i] = (byte) PofHelper.ReadAsInt32(reader, elementType);
                                }
                            }
                            bin = new Binary(ab);
                        }
                        break;

                    case PofConstants.T_SPARSE_ARRAY:
                        {
                            int    cb = reader.ReadPackedInt32();
                            byte[] ab = new byte[cb];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                ab[element] = (byte) PofHelper.ReadAsInt32(reader, reader.ReadPackedInt32());
                            }
                            while (--cb >= 0);
                            bin = new Binary(ab);
                        }
                        break;

                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        {
                            int    elementType = reader.ReadPackedInt32();
                            int    cb          = reader.ReadPackedInt32();
                            byte[] ab          = new byte[cb];
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                ab[element] = elementType == PofConstants.T_OCTET
                                            ? reader.ReadByte()
                                            : (byte) PofHelper.ReadAsInt32(reader, elementType);
                            }
                            while (--cb >= 0);
                            bin = new Binary(ab);
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId
                                              + " to a Binary type");
                }
            }

            Complete(index);

            return bin;
        }

        #endregion

        #region Collections

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
        public virtual Array ReadArray(int index)
        {
            return ReadArray(index, null);
        }

        /// <summary>
        /// Read an array of object values.
        /// </summary>
        /// <param name="index">
        /// The property index to read.
        /// </param>
        /// <param name="array">
        /// The optional array to use to store the values, or to use as a
        /// typed template for creating an array to store the values,
        /// following the documentation for <b>ArrayList.ToArray</b>.
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
        public virtual Array ReadArray(int index, Array array)
        {
            Array result = null;

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();

                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            result = ReadArray(index, array);
                            RegisterIdentity(id, result);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        {
                            object o = LookupIdentity(reader.ReadPackedInt32());
                            if (o is Array)
                            {
                                result = (Array) o;
                            }
                            else if (o is ICollection)
                            {
                                ICollection col = o as ICollection;
                                result          = Array.CreateInstance(typeof (object), col.Count);
                                col.CopyTo(result, 0);
                            }
                            else
                            {
                                result = (object[]) o;
                            }
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                        break;

                    case PofConstants.V_STRING_ZERO_LENGTH:
                    case PofConstants.V_COLLECTION_EMPTY:
                        result = PofHelper.OBJECT_ARRAY_EMPTY;
                        break;

                    default:
                        result = ReadAsArray(typeId, array);
                        break;
                }
            }
            Complete(index);

            return result;
        }

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
        public ILongArray ReadLongArray(int index, ILongArray array)
        {
            // do not default to null, since the caller is passing in a mutable
            // ILongArray

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int nType = reader.ReadPackedInt32();
                switch (nType)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int nId = reader.ReadPackedInt32();
                            array = ReadLongArray(index, array);
                            RegisterIdentity(nId, array);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        {
                            Object o = LookupIdentity(reader.ReadPackedInt32());
                            if (o is ICollection)
                            {
                                if (array == null)
                                {
                                    array = new LongSortedList();
                                }
                                int i = 0;
                                ICollection col = o as ICollection;
                                foreach (object entry in col)
                                {
                                    array[++i] = entry;
                                }
                            }
                            else
                            {
                                array = (ILongArray)o;
                            }
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                    case PofConstants.V_STRING_ZERO_LENGTH:
                    case PofConstants.V_COLLECTION_EMPTY:
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            if (array == null)
                            {
                                array = new LongSortedList();
                            }

                            int co = reader.ReadPackedInt32();
                            for (int i = 0; i < co; ++i)
                            {
                                array[i] = ReadAsObject(reader.ReadPackedInt32());
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            if (array == null)
                            {
                                array = new LongSortedList();
                            }

                            int nElementType = reader.ReadPackedInt32();
                            int co = reader.ReadPackedInt32();
                            for (int i = 0; i < co; ++i)
                            {
                                array[i] = ReadAsUniformObject(nElementType);
                            }
                        }
                        break;

                    case PofConstants.T_SPARSE_ARRAY:
                        {
                            if (array == null)
                            {
                                array = new LongSortedList();
                            }

                            int elements = reader.ReadPackedInt32();
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                array[element] = ReadAsObject(reader.ReadPackedInt32());
                            } while (--elements >= 0);
                        }
                        break;

                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        {
                            if (array == null)
                            {
                                array = new LongSortedList();
                            }

                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                array[element] = ReadAsUniformObject(elementType);
                            } while (--elements >= 0);
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + nType
                                              + " to an array type");
                }
            }
            Complete(index);

            return array;
        }

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
        public virtual ICollection ReadCollection(int index, ICollection coll)
        {
            // do not default to null, since the caller is passing in a mutable
            // Collection

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            coll = ReadCollection(index, coll);
                            RegisterIdentity(id, coll);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        {
                            ICollection collData = (ICollection) LookupIdentity(reader.ReadPackedInt32());
                            if (coll == null)
                            {
                                coll = collData;
                            }
                            else
                            {
                                CollectionUtils.AddAll(coll, collData);
                            }
                        }
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                    case PofConstants.V_STRING_ZERO_LENGTH:
                    case PofConstants.V_COLLECTION_EMPTY:
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            if (coll == null)
                            {
                                coll = ReadAsArray(typeId, null);
                            }
                            else
                            {
                                int co = reader.ReadPackedInt32();
                                for (int i = 0; i < co; ++i)
                                {
                                    CollectionUtils.Add(coll, ReadAsObject(reader.ReadPackedInt32()));
                                }
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            if (coll == null)
                            {
                                coll = ReadAsArray(typeId, null);
                            }
                            else
                            {
                                int elementType = reader.ReadPackedInt32();
                                int co          = reader.ReadPackedInt32();
                                for (int i = 0; i < co; ++i)
                                {
                                    CollectionUtils.Add(coll, ReadAsUniformObject(elementType));
                                }
                            }
                        }
                        break;

                    case PofConstants.T_SPARSE_ARRAY:
                        {
                            int elements = reader.ReadPackedInt32();
                            if (coll == null)
                            {
                                coll = new ArrayList(elements);
                            }

                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }

                                CollectionUtils.Add(coll, ReadAsObject(reader.ReadPackedInt32()));
                            } while (--elements >= 0);
                        }
                        break;

                    case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                        {
                            int elementType = reader.ReadPackedInt32();
                            int elements    = reader.ReadPackedInt32();
                            if (coll == null)
                            {
                                coll = new ArrayList(elements);
                            }

                            do
                            {
                                int element = reader.ReadPackedInt32();
                                if (element < 0)
                                {
                                    break;
                                }
                                CollectionUtils.Add(coll, ReadAsUniformObject(elementType));
                            } while (--elements >= 0);
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to an array type");
                }
            }
            Complete(index);

            return coll;
        }

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
        public virtual IDictionary ReadDictionary(int index, IDictionary dict)
        {
            // do not default to null, since the caller is passing in a mutable
            // Map

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int        typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            dict = ReadDictionary(index, dict);
                            RegisterIdentity(id, dict);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        dict = (IDictionary) LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                    case PofConstants.V_STRING_ZERO_LENGTH:
                    case PofConstants.V_COLLECTION_EMPTY:
                        break;

                    case PofConstants.T_MAP:
                        {
                            int entries = reader.ReadPackedInt32();
                            if (dict == null)
                            {
                                dict = new HashDictionary(entries);
                            }

                            for (int i = 0; i < entries; ++i)
                            {
                                object key = ReadAsObject(reader.ReadPackedInt32());
                                object val = ReadAsObject(reader.ReadPackedInt32());
                                dict[key] = val;
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_KEYS_MAP:
                        {
                            int keyType = reader.ReadPackedInt32();
                            int entries = reader.ReadPackedInt32();
                            if (dict == null)
                            {
                                dict = new HashDictionary(entries);
                            }

                            for (int i = 0; i < entries; ++i)
                            {
                                object key = ReadAsUniformObject(keyType);
                                object val = ReadAsObject(reader.ReadPackedInt32());
                                dict[key] = val;
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_MAP:
                        {
                            int keyType = reader.ReadPackedInt32();
                            int valType = reader.ReadPackedInt32();
                            int entries = reader.ReadPackedInt32();
                            if (dict == null)
                            {
                                dict = new HashDictionary(entries);
                            }

                            for (int i = 0; i < entries; ++i)
                            {
                                object key = ReadAsUniformObject(keyType);
                                object val = ReadAsUniformObject(valType);
                                dict[key] = val;
                            }
                        }
                        break;

                    default:
                        throw new IOException("unable to convert type " + typeId + " to an array type");
                }
            }
            Complete(index);

            return dict;
        }

        /// <summary>
        /// Read a generic <b>ICollection&lt;T&gt;</b> of object values from
        /// the POF stream.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements in the collection.
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
        public virtual ICollection<T> ReadCollection<T>(int index, ICollection<T> coll)
        {
            // do not default to null, since the caller is passing in a mutable
            // Collection

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                    {
                        int id = reader.ReadPackedInt32();
                        coll = ReadCollection(index, coll);
                        RegisterIdentity(id, coll);
                    }
                    break;

                    case PofConstants.T_REFERENCE:
                    {
                        ICollection<T> collData = LookupIdentity(reader.ReadPackedInt32()) as ICollection<T>;
                        if (coll == null)
                        {
                            coll = collData;
                        }
                        else
                        {
                            foreach(T item in collData)
                            {
                                coll.Add(item);
                            }
                        }
                    }
                    break;

                    case PofConstants.V_REFERENCE_NULL:
                    case PofConstants.V_STRING_ZERO_LENGTH:
                    case PofConstants.V_COLLECTION_EMPTY:
                        break;

                    case PofConstants.T_COLLECTION:
                    case PofConstants.T_ARRAY:
                        {
                            int co = reader.ReadPackedInt32();
                            if (coll == null)
                            {
                                coll = new List<T>(co);
                            }
                            for (int i = 0; i < co; ++i)
                            {
                                int elementTypeId = reader.ReadPackedInt32();
                                object o          = ReadAsObject(elementTypeId);
                                if (o!= null && !(o is T))
                                {
                                    Debug.Assert(false, "can not assign type " 
                                        + o.GetType().Name + " to " + typeof(T).Name);
                                }
                                coll.Add((T) o);
                            }
                        }
                        break;

                    case PofConstants.T_UNIFORM_COLLECTION:
                    case PofConstants.T_UNIFORM_ARRAY:
                        {
                            int elementType  = reader.ReadPackedInt32();
                            int entries      = reader.ReadPackedInt32();
                            System.Type type = typeof(T);

                            if (type.IsSealed)
                            {
                                Debug.Assert(
                                    type.Equals(PofHelper.GetDotNetType(elementType)));
                            }
                            if (coll == null)
                            {
                                coll = new List<T>(entries);
                            }
                            for (int i = 0; i < entries; ++i)
                            {
                                coll.Add((T) ReadAsUniformObject(elementType));
                            }
                        }
                        break;

                    default:
                        throw new IOException("Unable to convert type " + 
                                typeId + " to a generic collection type");
                }
            }
            Complete(index);

            return coll;
        }

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
        public virtual IDictionary<TKey, TValue> ReadDictionary<TKey, TValue>(int index, IDictionary<TKey, TValue> dictionary)
        {
            // do not default to null, since the caller is passing in a mutable
            // Map

            if (AdvanceTo(index))
            {
                DataReader reader = m_reader;
                int typeId = reader.ReadPackedInt32();
                switch (typeId)
                {
                    case PofConstants.T_IDENTITY:
                        {
                            int id = reader.ReadPackedInt32();
                            dictionary = ReadDictionary(index, dictionary);
                            RegisterIdentity(id, dictionary);
                        }
                        break;

                    case PofConstants.T_REFERENCE:
                        dictionary = (IDictionary<TKey, TValue>)LookupIdentity(reader.ReadPackedInt32());
                        break;

                    case PofConstants.V_REFERENCE_NULL:
                    case PofConstants.V_STRING_ZERO_LENGTH:
                    case PofConstants.V_COLLECTION_EMPTY:
                        break;
                        
                    case PofConstants.T_MAP:
                        {
                            int entries = reader.ReadPackedInt32();
                            if (dictionary == null)
                            {
                                dictionary = new Dictionary<TKey, TValue>(entries);
                            }

                            for (int i = 0; i < entries; ++i)
                            {
                                TKey   key = (TKey)ReadAsObject(reader.ReadPackedInt32());
                                TValue val = (TValue)ReadAsObject(reader.ReadPackedInt32());
                                dictionary[key] = val;
                            }
  
                        }
                        break;

                    case PofConstants.T_UNIFORM_KEYS_MAP:
                    {
                        int keyType = reader.ReadPackedInt32();
                        int entries = reader.ReadPackedInt32();
                        if (dictionary == null)
                        {
                            dictionary = new Dictionary<TKey, TValue>(entries);
                        }

                        for (int i = 0; i < entries; ++i)
                        {
                            TKey   key = (TKey) ReadAsUniformObject(keyType);
                            object val = ReadAsObject(reader.ReadPackedInt32());

                            dictionary[key] = (TValue) val;
                        }
                    }
                    break;

                    case PofConstants.T_UNIFORM_MAP:
                    {
                        int keyType   = reader.ReadPackedInt32(); // read key type
                        int valueType = reader.ReadPackedInt32(); // read value type
                        int entries   = reader.ReadPackedInt32(); // read the number of entries

                        if (dictionary == null)
                        {
                            dictionary = new Dictionary<TKey, TValue>(entries);
                        }

                        for (int i = 0; i < entries; ++i)
                        {
                            TKey   key = (TKey) ReadAsUniformObject(keyType); 
                            TValue val = (TValue) ReadAsUniformObject(valueType); 
                            dictionary[key] = val;
                        }
                    }
                    break;

                    default:
                        throw new IOException("Unable to convert type " + typeId + " to an generic collection type");
                }
            }
            Complete(index);

            return dictionary;
        }

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
        /// <exception cref="InvalidOperationException">
        /// If no user type is being parsed.
        /// </exception>
        /// <see>IPofSerializer#Deserialize(IPofReader)</see>
        /// <since>Coherence 3.7.1</since>
        public virtual void RegisterIdentity(object o)
        {
            throw new InvalidOperationException("not in a user type");
        }

        /// <summary>
        /// Obtain a PofReader that can be used to read a set of properties from a
        /// single property of the current user type. The returned PofReader is
        /// only valid from the time that it is returned until the next call is
        /// made to this PofReader.
        /// </summary>
        /// <param name="iProp">
        /// the property index to read from </param>
        /// <returns>
        /// a PofReader that reads its contents from  a single property of
        /// this PofReader
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the POF stream has already advanced past the
        /// desired property, or if no user type is being parsed.
        /// </exception>
        /// <exception cref="IOException">
        /// if an I/O error occurs
        /// </exception>
        /// <since> Coherence 3.6 </since>
        public virtual IPofReader CreateNestedPofReader(int iProp)
        {
            throw new InvalidOperationException("not in a user type");
        }

        /// <summary>
        /// Read all remaining indexed properties of the current user type
        /// from the POF stream.
        /// </summary>
        /// <remarks>
        /// As part of reading in a user type, this method must be called by
        /// the <see cref="IPofSerializer"/> that is reading the user type,
        /// or the read position within the POF stream will be corrupted.
        /// Subsequent calls to the various <b>ReadXYZ</b> methods of this
        /// interface will fail after this method is called.
        /// </remarks>
        /// <returns>
        /// A <b>Byte[]</b> containing zero or more indexed properties in
        /// binary POF encoded form.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If no user type is being parsed.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual Binary ReadRemainder()
        {
            throw new InvalidOperationException("not in a user type");
        }

        #endregion

        #endregion

        #region Internal methods

        /// <summary>
        /// Advance through the POF stream until the specified property is
        /// found.
        /// </summary>
        /// <remarks>
        /// If the property is found, return <b>true</b>, otherwise return
        /// <b>false</b> and advance to the first property that follows the
        /// specified property.
        /// </remarks>
        /// <param name="index">
        /// The index of the property to advance to.
        /// </param>
        /// <returns>
        /// <b>true</b> if the property is found.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// if the POF stream has already advanced past the desired property.
        /// </exception>
        protected internal virtual bool AdvanceTo(int index)
        {
            if (index > 0)
            {
                throw new InvalidOperationException("not in a user type");
            }
            return true;
        }

        /// <summary>
        /// Register the completion of the parsing of a value.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        protected internal virtual void Complete(int index)
        {}

        /// <summary>
        /// Obtain the registry for identity-reference pairs, creating it if
        /// necessary.
        /// </summary>
        /// <returns>
        /// The identity-reference registry, never <c>null</c>.
        /// </returns>
        protected internal virtual ILongArray EnsureReferenceRegistry()
        {
            ILongArray array = m_referenceMap;

            if (array == null)
            {
                PofStreamReader parent = ParentParser;
                m_referenceMap = array = parent == null ? new LongSortedList() : parent.EnsureReferenceRegistry();
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
        protected virtual void RegisterIdentity(int id, object value)
        {
            if (id >= 0)
            {
                ILongArray map = EnsureReferenceRegistry();
                object      o  = map[id];
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
        protected internal virtual object LookupIdentity(int id)
        {
            ILongArray map = EnsureReferenceRegistry();
            if (!map.Exists(id))
            {
                throw new IOException("missing identity: " + id);
            }

            return map[id];
        }

        /// <summary>
        /// Read a POF value as an Object.
        /// </summary>
        /// <param name="typeId">
        /// The type identifier of the value.
        /// </param>
        /// <returns>
        /// An Object value.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        protected internal virtual object ReadAsObject(int typeId)
        {
            object o = null;

            DataReader reader = m_reader;
            switch (typeId)
            {
                case PofConstants.T_INT16:
                    o = (short) reader.ReadPackedInt32();
                    break;

                case PofConstants.T_INT32:
                case PofConstants.V_INT_NEG_1:
                case PofConstants.V_INT_0:
                case PofConstants.V_INT_1:
                case PofConstants.V_INT_2:
                case PofConstants.V_INT_3:
                case PofConstants.V_INT_4:
                case PofConstants.V_INT_5:
                case PofConstants.V_INT_6:
                case PofConstants.V_INT_7:
                case PofConstants.V_INT_8:
                case PofConstants.V_INT_9:
                case PofConstants.V_INT_10:
                case PofConstants.V_INT_11:
                case PofConstants.V_INT_12:
                case PofConstants.V_INT_13:
                case PofConstants.V_INT_14:
                case PofConstants.V_INT_15:
                case PofConstants.V_INT_16:
                case PofConstants.V_INT_17:
                case PofConstants.V_INT_18:
                case PofConstants.V_INT_19:
                case PofConstants.V_INT_20:
                case PofConstants.V_INT_21:
                case PofConstants.V_INT_22:
                    o = PofHelper.ReadAsInt32(reader, typeId);
                    break;

                case PofConstants.T_INT64:
                    o = reader.ReadPackedInt64();
                    break;

                case PofConstants.T_INT128:
                    throw new NotSupportedException("T_INT128 is not supported.");

                case PofConstants.T_FLOAT32:
                    o = reader.ReadSingle();
                    break;

                case PofConstants.T_FLOAT64:
                    o = reader.ReadDouble();
                    break;

                case PofConstants.T_FLOAT128:
                    throw new NotSupportedException("T_FLOAT128 is not supported.");

                case PofConstants.V_FP_POS_INFINITY:
                    o = Double.PositiveInfinity;
                    break;

                case PofConstants.V_FP_NEG_INFINITY:
                    o = Double.NegativeInfinity;
                    break;

                case PofConstants.V_FP_NAN:
                    o = Double.NaN;
                    break;

                case PofConstants.T_DECIMAL32:
                case PofConstants.T_DECIMAL64:
                case PofConstants.T_DECIMAL128:
                    o = PofHelper.ReadAsDecimal(reader, typeId);
                    break;

                case PofConstants.T_BOOLEAN:
                    o = reader.ReadPackedInt32() == 0 ? false : true;
                    break;

                case PofConstants.T_OCTET:
                    o = reader.ReadByte();
                    break;

                case PofConstants.T_OCTET_STRING:
                    o = ReadBinary(reader);
                    break;

                case PofConstants.T_CHAR:
                    o = PofHelper.ReadChar(reader);
                    break;

                case PofConstants.T_CHAR_STRING:
                    o = reader.ReadString();
                    break;

                case PofConstants.T_DATE:
                    o = PofHelper.ReadDate(reader);
                    break;

                case PofConstants.T_TIME:
                    o = PofHelper.ReadRawTime(reader);
                    break;

                case PofConstants.T_DATETIME:
                    o = PofHelper.ReadDateTime(reader);
                    break;

                case PofConstants.T_YEAR_MONTH_INTERVAL:
                    {
                        int years  = reader.ReadPackedInt32();
                        int months = reader.ReadPackedInt32();
                        o = new RawYearMonthInterval(years, months);
                    }
                    break;

                case PofConstants.T_TIME_INTERVAL:
                    {
                        int hours   = reader.ReadPackedInt32();
                        int minutes = reader.ReadPackedInt32();
                        int seconds = reader.ReadPackedInt32();
                        int nanos   = reader.ReadPackedInt32();
                        o = new TimeSpan(0, hours, minutes, seconds, nanos / 1000000);
                    }
                    break;

                case PofConstants.T_DAY_TIME_INTERVAL:
                    {
                        int days    = reader.ReadPackedInt32();
                        int hours   = reader.ReadPackedInt32();
                        int minutes = reader.ReadPackedInt32();
                        int seconds = reader.ReadPackedInt32();
                        int nanos   = reader.ReadPackedInt32();
                        o = new TimeSpan(days, hours, minutes, seconds, nanos / 1000000);
                    }
                    break;

                case PofConstants.T_COLLECTION:
                case PofConstants.T_UNIFORM_COLLECTION:
                    o = ReadAsArray(typeId, null);
                    break;

                case PofConstants.T_ARRAY:
                    o = ReadAsArray(typeId, null);
                    break;

                case PofConstants.T_UNIFORM_ARRAY:
                    int nElementType = reader.ReadPackedInt32();
                    int cElements    = reader.ReadPackedInt32();

                    switch (nElementType)
                    {
                        case PofConstants.T_BOOLEAN:
                            {
                                bool[] ab = new bool[cElements];
                                for (int i = 0; i < cElements; ++i)
                                {
                                    ab[i] = reader.ReadPackedInt32() != 0;
                                }
                                o = ab;
                            }
                            break;

                        case PofConstants.T_OCTET:
                            o = reader.ReadBytes(cElements);
                            break;

                        case PofConstants.T_CHAR:
                            {
                                char[] ach = new char[cElements];
                                for (int i = 0; i < cElements; ++i)
                                {
                                    ach[i] = PofHelper.ReadChar(reader);
                                }
                                o = ach;
                            }
                            break;

                        case PofConstants.T_INT16:
                            {
                                Int16[] an = new short[cElements];
                                for (int i = 0; i < cElements; ++i)
                                {
                                    an[i] = (short) reader.ReadPackedInt32();
                                }
                                o = an;
                            }
                            break;

                        case PofConstants.T_INT32:
                            {
                                Int32[] an = new Int32[cElements];
                                for (int i = 0; i < cElements; ++i)
                                {
                                    an[i] = reader.ReadPackedInt32();
                                }
                                o = an;
                            }
                            break;

                        case PofConstants.T_INT64:
                            {
                                Int64[] an = new Int64[cElements];
                                for (int i = 0; i < cElements; ++i)
                                {
                                    an[i] = reader.ReadPackedInt64();
                                }
                                o = an;
                            }
                            break;

                        case PofConstants.T_FLOAT32:
                            {
                                Single[] afl = new Single[cElements];
                                for (int i = 0; i < cElements; ++i)
                                {
                                    afl[i] = reader.ReadSingle();
                                }
                                o = afl;
                            }
                            break;

                        case PofConstants.T_FLOAT64:
                            {
                                Double[] adfl = new Double[cElements];
                                for (int i = 0; i < cElements; ++i)
                                {
                                    adfl[i] = reader.ReadDouble();
                                }
                                o = adfl;
                            }
                            break;

                        default:
                            {
                                Object[] ao = new Object[cElements];
                                for (int i = 0; i < cElements; ++i)
                                {
                                    ao[i] = ReadAsUniformObject(nElementType);
                                }
                                o = ao;
                            }
                            break;
                    }
                    break;

                case PofConstants.T_SPARSE_ARRAY:
                    {
                        ILongArray array    = new LongSortedList();
                        int        elements = reader.ReadPackedInt32();
                        do
                        {
                            int element = reader.ReadPackedInt32();
                            if (element < 0)
                            {
                                break;
                            }
                            array[element] = ReadAsObject(reader.ReadPackedInt32());
                        } while (--elements >= 0);
                        o = array;
                        break;
                    }

                case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                    {
                        ILongArray array       = new LongSortedList();
                        int        elementType = reader.ReadPackedInt32();
                        int        elements    = reader.ReadPackedInt32();
                        do
                        {
                            int element = reader.ReadPackedInt32();
                            if (element < 0)
                            {
                                break;
                            }
                            array[element] = ReadAsUniformObject(elementType);
                        } while (--elements >= 0);
                        o = array;
                        break;
                    }

                case PofConstants.T_MAP:
                    {
                        IDictionary map     = new HashDictionary();
                        int         entries = reader.ReadPackedInt32();
                        for (int i = 0; i < entries; ++i)
                        {
                            object key = ReadAsObject(reader.ReadPackedInt32());
                            object val = ReadAsObject(reader.ReadPackedInt32());
                            map[key] = val;
                        }
                        o = map;
                    }
                    break;

                case PofConstants.T_UNIFORM_KEYS_MAP:
                    {
                        IDictionary map     = new HashDictionary();
                        int         keyType = reader.ReadPackedInt32();
                        int         entries = reader.ReadPackedInt32();
                        for (int i = 0; i < entries; ++i)
                        {
                            object key = ReadAsUniformObject(keyType);
                            object val = ReadAsObject(reader.ReadPackedInt32());
                            map[key] = val;
                        }
                        o = map;
                    }
                    break;

                case PofConstants.T_UNIFORM_MAP:
                    {
                        IDictionary map     = new HashDictionary();
                        int         keyType = reader.ReadPackedInt32();
                        int         valType = reader.ReadPackedInt32();
                        int         entries = reader.ReadPackedInt32();
                        for (int i = 0; i < entries; ++i)
                        {
                            object key = ReadAsUniformObject(keyType);
                            object val = ReadAsUniformObject(valType);
                            map[key] = val;
                        }
                        o = map;
                    }
                    break;

                case PofConstants.T_IDENTITY:
                    {
                        int id = reader.ReadPackedInt32();
                        typeId = reader.ReadPackedInt32();
                        IdentityHolder.Set(this, id);
                        o = ReadAsObject(typeId);
                        IdentityHolder.Reset(this, id, o);
                    }
                    break;

                case PofConstants.T_REFERENCE:
                    o = LookupIdentity(reader.ReadPackedInt32());
                    break;

                case PofConstants.V_BOOLEAN_FALSE:
                    o = false;
                    break;

                case PofConstants.V_BOOLEAN_TRUE:
                    o = true;
                    break;

                case PofConstants.V_STRING_ZERO_LENGTH:
                    o = "";
                    break;

                case PofConstants.V_COLLECTION_EMPTY:
                    o = PofHelper.COLLECTION_EMPTY;
                    break;

                case PofConstants.V_REFERENCE_NULL:
                    break;

                default:
                    {
                        if (typeId < 0)
                        {
                            throw new IOException("illegal type " + typeId);
                        }

                        IPofContext    ctx            = PofContext;
                        IPofSerializer ser            = ctx.GetPofSerializer(typeId);
                        UserTypeReader userTypeReader = new UserTypeReader(
                                this, reader, ctx, typeId, reader.ReadPackedInt32());
                        o = ser.Deserialize(userTypeReader);
                    }
                    break;
            }

            return o;
        }

        /// <summary>
        /// Read a POF value in a uniform array/map as an Object.
        /// </summary>
        /// <param name="typeId">
        /// The type identifier of the value.
        /// </param>
        /// <returns>
        /// An Object value.
        /// </returns>
        /// <since>Coherence 3.7.1</since>
        protected Object ReadAsUniformObject(int typeId)
        {
            if (typeId < 0)
            {
                return ReadAsObject(typeId);
            }

            DataReader reader = m_reader;            
            long       offset = reader.BaseStream.Position;
            if (offset == 0)
            {
                return ReadAsObject(typeId); 
            }

            int    id    = -1;
            int    value = reader.ReadPackedInt32();
            Object o;
            if (value == PofConstants.T_IDENTITY)
            {
                id = reader.ReadPackedInt32(); 
                IdentityHolder.Set(this, id);
            }
            else
            {
                // it can only be reference if its data type supports
                // reference (a user defined object)
                if (value > 0 && typeId >= 0)
                {
                    o = EnsureReferenceRegistry()[value];
                    if (o != null)
                    {
                        // double check the object type.
                        int type = PofHelper.GetPofTypeId(o.GetType(), PofContext);
                        if (type == typeId)
                        {
                            return o;
                        }
                    }
                }
                reader.BaseStream.Position = offset;
            }

            o = ReadAsObject(typeId);
            if (value == PofConstants.T_IDENTITY)
            {
                IdentityHolder.Reset(this, id, o);
            }
            return o;
        }

        /// <summary>
        /// Read a POF value as a typed object array.
        /// </summary>
        /// <param name="typeId">
        /// The type identifier of the value.
        /// </param>
        /// <param name="array">
        /// The optional array to use to store the values, or to use as a
        /// typed template for creating an array to store the values.
        /// </param>
        /// <returns>
        /// A typed object array.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        protected internal virtual Array ReadAsArray(int typeId, Array array)
        {
            Array result = null;

            DataReader reader = m_reader;
            switch (typeId)
            {
                case PofConstants.V_REFERENCE_NULL:
                    break;

                case PofConstants.V_STRING_ZERO_LENGTH:
                case PofConstants.V_COLLECTION_EMPTY:
                    result = PofHelper.OBJECT_ARRAY_EMPTY;
                    break;

                case PofConstants.T_COLLECTION:
                case PofConstants.T_ARRAY:
                    {
                        int co = reader.ReadPackedInt32();
                        result = PofHelper.ResizeArray(array, co);
                        for (int i = 0; i < co; ++i)
                        {
                            result.SetValue(ReadAsObject(reader.ReadPackedInt32()), i);
                        }
                    }
                    break;

                case PofConstants.T_UNIFORM_COLLECTION:
                case PofConstants.T_UNIFORM_ARRAY:
                    {
                        int elementType = reader.ReadPackedInt32();
                        int co          = reader.ReadPackedInt32();
                        result = PofHelper.ResizeArray(array, co, PofHelper.GetDotNetType(elementType));
                        for (int i = 0; i < co; ++i)
                        {
                            result.SetValue(ReadAsUniformObject(elementType), i);
                        }
                    }
                    break;

                case PofConstants.T_SPARSE_ARRAY:
                    {
                        int elements = reader.ReadPackedInt32();
                        result = PofHelper.ResizeArray(array, elements);
                        do
                        {
                            int element = reader.ReadPackedInt32();
                            if (element < 0)
                            {
                                break;
                            }
                            result.SetValue(ReadAsObject(reader.ReadPackedInt32()), element);
                        } while (--elements >= 0);
                    }
                    break;

                case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                    {
                        int elementType = reader.ReadPackedInt32();
                        int elements    = reader.ReadPackedInt32();
                        result = PofHelper.ResizeArray(array, elements, PofHelper.GetDotNetType(elementType));
                        do
                        {
                            int element = reader.ReadPackedInt32();
                            if (element < 0)
                            {
                                break;
                            }
                            result.SetValue(ReadAsUniformObject(elementType), element);
                        } while (--elements >= 0);
                    }
                    break;

                default:
                    throw new IOException("unable to convert type " + typeId + " to an array type");
            }

            return result;
        }

        /// <summary>
        /// Read a <see cref="Binary"/> object from the specified
        /// <see cref="DataReader"/> in an optimal way.
        /// </summary>
        /// <param name="reader">
        /// A <b>DataReader</b> to read from.
        /// </param>
        /// <returns>
        /// The Binary data.
        /// </returns>
        protected static Binary ReadBinary(DataReader reader)
        {
            return new Binary(reader.BaseStream, reader.ReadPackedInt32());
        }

        #endregion

        #region Inner class: UserTypeReader

        /// <summary>
        /// The <b>UserTypeReader</b> implementation is a contextually-aware
        /// <see cref="IPofReader"/> whose purpose is to advance through the
        /// properties of a value of a specified user type.
        /// </summary>
        /// <remarks>
        /// The "contextual awareness" refers to the fact that the
        /// <b>UserTypeReader</b> maintains state about the type identifier
        /// and version of the user type, the parser's property index
        /// position within the user type value, and a
        /// <see cref="IPofContext"/> that may differ from the
        /// <b>IPofContext</b> that provided the <see cref="IPofSerializer"/>
        /// which is using this <b>UserTypeReader</b> to parse a user type.
        /// </remarks>
        public class UserTypeReader : PofStreamReader
        {
            // CLOVER:OFF

            #region Properties

            /// <summary>
            /// Gets the user type that is currently being parsed.
            /// </summary>
            /// <value>
            /// The user type information, or -1 if the
            /// <see cref="PofStreamReader"/> is not currently parsing a user
            /// type.
            /// </value>
            public override int UserTypeId
            {
                get { return m_typeId; }
            }

            /// <summary>
            /// Gets the version identifier of the user type that is
            /// currently being parsed.
            /// </summary>
            /// <value>
            /// The integer version ID read from the POF stream; always
            /// non-negative.
            /// </value>
            /// <exception cref="InvalidOperationException">
            /// If no user type is being parsed.
            /// </exception>
            public override int VersionId
            {
                get { return m_versionId; }
            }

            /// <summary>
            /// If this parser is contextually within a user type, obtain the
            /// parser which created this parser in order to parse the user type.
            /// </summary>
            /// <value>
            /// The parser for the context within which this parser is operating.
            /// </value>
            protected override internal PofStreamReader ParentParser
            {
                get { return m_parent; }
            }

            #endregion

            // CLOVER:ON

            #region Constructors

            /// <summary>
            /// Construct a parser for parsing the property values of a user
            /// type.
            /// </summary>
            /// <param name="reader">
            /// The <see cref="DataReader"/> that contains the user type
            /// data, except for the user type id itself (which is passed as
            /// a constructor argument).
            /// </param>
            /// <param name="ctx">
            /// The <see cref="IPofContext"/> to use for parsing the user
            /// type property values within the user type that this parser
            /// will be parsing.
            /// </param>
            /// <param name="typeId">
            /// The type id of the user type.
            /// </param>
            /// <param name="versionId">
            /// The version id of the user type.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public UserTypeReader(DataReader reader, IPofContext ctx, int typeId, int versionId)
                    : this(null, reader, ctx, typeId, versionId)
            {
            }

            /// <summary>
            /// Construct a parser for parsing the property values of a user
            /// type.
            /// </summary>
            /// <param name="parent">
            /// The parent <see cref="PofStreamReader"/> (ie the containing) PofBufferReader
            /// </param>
            /// <param name="reader">
            /// The <see cref="DataReader"/> that contains the user type
            /// data, except for the user type id itself (which is passed as
            /// a constructor argument).
            /// </param>
            /// <param name="ctx">
            /// The <see cref="IPofContext"/> to use for parsing the user
            /// type property values within the user type that this parser
            /// will be parsing.
            /// </param>
            /// <param name="typeId">
            /// The type id of the user type.
            /// </param>
            /// <param name="versionId">
            /// The version id of the user type.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public UserTypeReader(PofStreamReader parent, DataReader reader, IPofContext ctx, int typeId, int versionId)
                    : base(reader, ctx)
            {
                Debug.Assert(typeId >= 0);
                Debug.Assert(versionId >= 0);

                m_parent    = parent;
                m_typeId    = typeId;
                m_versionId = versionId;

                // prime the property reader by knowing the offset of index of
                // the next property to read
                m_ofNextProp = reader.BaseStream.Position;
                int index    = reader.ReadPackedInt32();
                m_nextProp   = index < 0 ? EOPS : index;
            }

            /// <summary>
            /// Construct a parser for parsing the property values of a user
            /// type.
            /// </summary>
            /// <param name="parent">
            /// The parent <see cref="PofStreamReader"/> (ie the containing) PofBufferReader
            /// </param>
            /// <param name="reader">
            /// The <see cref="DataReader"/> that contains the user type
            /// data, except for the user type id itself (which is passed as
            /// a constructor argument).
            /// </param>
            /// <param name="context">
            /// The <see cref="IPofContext"/> to use for parsing the user
            /// type property values within the user type that this parser
            /// will be parsing.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            private UserTypeReader(PofStreamReader parent, DataReader reader, 
                IPofContext context) : base(reader, context)
            {
                m_parent     = parent;
             
                // read the type and version directly from the stream
                m_typeId     = reader.ReadPackedInt32();
                m_versionId  = reader.ReadPackedInt32();

                // prime the property reader by knowing the offset of index of 
                // the next property to read
                m_ofNextProp = reader.BaseStream.Position;

                int prop     = reader.ReadPackedInt32();
                m_nextProp   = prop < 0 ? EOPS : prop;
            }

            #endregion

            #region IPofReader interface

            /// <summary>
            /// <p>
            /// Register an identity for a newly created user type instance.</p>
            /// If identity/reference types are enabled, an identity is used to
            /// uniquely identify a user type instance within a POF stream. The
            /// identity immediately proceeds the instance value in the POF stream
            /// and can be used later in the stream to reference the instance.<p/>
            /// IPofSerializer implementations must call this method with each
            /// user type instance instantiated during deserialization prior to 
            /// reading any properties of the instance which are user type
            /// instances themselves.
            /// </summary>
            /// <param name="o">
            /// The object to register the identity for.
            /// </param>
            /// <see> IPofSerializer#Deserialize(IPofReader) </see>
            /// <since> Coherence 3.7.1 </since>
            public override void RegisterIdentity(object o)
            {
                IdentityHolder.Reset(this, -1, o);
            }

            /// <summary>
            /// Obtain a PofReader that can be used to read a set of properties from a
            /// single property of the current user type. The returned PofReader is
            /// only valid from the time that it is returned until the next call is
            /// made to this PofReader.
            /// </summary>
            /// <param name="iProp">
            /// the property index to read from </param>
            /// <returns>
            /// a PofReader that reads its contents from a single property of
            /// this PofReader
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// If the POF stream has already advanced past the
            /// desired property, or if no user type is being parsed.
            /// </exception>
            /// <exception cref="IOException">
            /// if an I/O error occurs
            /// </exception>
            /// <since> Coherence 3.6 </since>
            public override IPofReader CreateNestedPofReader(int iProp)
            {
                UserTypeReader reader;
                if (AdvanceTo(iProp))
                {
                    reader = new UserTypeReader(this, m_reader, PofContext); 
                 
                    // note: there is no complete() call at this point, since the
                    //       property has yet to be read
                }
                else
                {   
                    // nothing to read for that property
                    Complete(iProp);
                
                    // return a "fake" reader that contains no data
                    reader = new UserTypeReader(this, m_reader,
                            PofContext, UserTypeId, VersionId); 
                }

                m_readerNested = reader;
                m_iNestedProp  = iProp;
            
                return reader;
            }

            /// <summary>
            /// Read all remaining indexed properties of the current user
            /// type from the POF stream.
            /// </summary>
            /// <remarks>
            /// As part of reading in a user type, this method must be called
            /// by the <see cref="IPofSerializer"/> that is reading the user
            /// type, or the read position within the POF stream will be
            /// corrupted. Subsequent calls to the various <b>ReadXYZ</b>
            /// methods of this interface will fail after this method is
            /// called.
            /// </remarks>
            /// <returns>
            /// A <b>Binary</b> containing zero or more indexed properties in
            /// binary POF encoded form.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// If no user type is being parsed.
            /// </exception>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override Binary ReadRemainder()
            {
                CloseNested();
                // check if the property stream is aready exhausted
                int nextProp = m_nextProp;

                if (nextProp == EOPS)
                {
                    return null;
                }

                // skip over all the remaining properties
                DataReader reader  = m_reader;
                int        ofBegin = (int) m_ofNextProp;
                int        ofEnd;
                do
                {
                    PofHelper.SkipValue(reader);
                    ofEnd    = (int) reader.BaseStream.Position;
                    nextProp = reader.ReadPackedInt32();
                } while (nextProp != - 1);

                m_nextProp   = EOPS;
                m_ofNextProp = ofEnd;

                // return all the properties that were skipped
                reader.BaseStream.Position = ofBegin;
                Binary bin                 = new Binary(reader.BaseStream, ofEnd - ofBegin);
                reader.ReadPackedInt32();
                
                return bin;
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Return the index of the most recent property read or 
            /// (if it were missing) requested.
            /// </summary>
            public int PreviousPropertyIndex
            {
                get { return m_prevProp; }
            }

            /// <summary>
            /// Return the index of the next property in the POF stream.
            /// </summary>
            public int NextPropertyIndex
            {
                get
                {
                    CloseNested();
                    return m_nextProp == EOPS ? -1 : m_nextProp;
                }
            }

            /// <summary>
            /// Advance through the POF stream until the specified property
            /// is found.
            /// </summary>
            /// <remarks>
            /// If the property is found, return <b>true</b>, otherwise
            /// return <b>false</b> and advance to the first property that
            /// follows the specified property.
            /// </remarks>
            /// <param name="index">
            /// The index of the property to advance to.
            /// </param>
            /// <returns>
            /// <b>true</b> if the property is found.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// If the POF stream has already advanced past the desired
            /// property.
            /// </exception>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            protected internal override bool AdvanceTo(int index)
            {
                // if a nested writer is still open, then "end" that property
                CloseNested();

                // the terminating index is -1; if searching for -1, re-order the
                // goal to come after all other properties (which assumes that
                // there is no valid property index Int32.MAX_VALUE)
                if (index == -1)
                {
                    index = EOPS;
                }

                // check for backwards movement
                if (index <= m_prevProp)
                {
                    throw new InvalidOperationException("previous property index=" + m_prevProp + ", requested property index=" +
                                                        index + " while reading user type " + UserTypeId);
                }

                // check if the stream is already in the correct location
                // (common case)
                int nextProp = m_nextProp;
                if (index == nextProp)
                {
                    return true;
                }

                DataReader reader     = m_reader;
                int        ofNextProp = (int) m_ofNextProp;

                while (nextProp != EOPS && nextProp < index)
                {
                    PofHelper.SkipValue(reader);

                    ofNextProp = (int) reader.BaseStream.Position;
                    nextProp   = reader.ReadPackedInt32();
                    if (nextProp < 0)
                    {
                        nextProp = EOPS;
                    }
                }
                m_ofNextProp = ofNextProp;
                m_nextProp   = nextProp;

                return index == nextProp;
            }

            /// <summary>
            /// Register the completion of the parsing of a value.
            /// </summary>
            /// <param name="index">
            /// The property index.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            protected internal override void Complete(int index)
            {
                if (m_nextProp == index)
                {
                    DataReader reader = m_reader;
                    m_ofNextProp      = (int) reader.BaseStream.Position;
                    int iNextProp     = reader.ReadPackedInt32();
                    m_nextProp        = iNextProp < 0 ? EOPS : iNextProp;
                }

                m_prevProp = index;
            }

            /// <summary>
            /// Notify the UserTypeReader that it is being "closed".
            /// </summary>
            /// <exception cref="NotSupportedException">    
            /// throws IOException  if an I/O error occurs
            /// </exception>
            protected void CloseNested()
            {
                // check if a nested PofReader is open
                UserTypeReader readerNested = m_readerNested;
                if (readerNested != null)
                {
                    // close it
                    readerNested.CloseNested();

                    // finish reading the property that the nested PofReader was
                    // reading from; this is the "complete()" call that was
                    // deferred when the nested stream was opened
                    Complete(m_iNestedProp);

                    m_readerNested = null;
                    m_iNestedProp  = -1;
                }
            }

            #endregion

            #region Constants

            /// <summary>
            /// Fake End-Of-Property-Stream indicator.
            /// </summary>
            private const int EOPS = Int32.MaxValue;

            #endregion

            #region Data members

            /// <summary>
            ///  The parent (ie containing) PofBufferReader.
            /// </summary>
            private readonly PofStreamReader m_parent;

            /// <summary>
            /// The type identifier of the user type that is being parsed.
            /// </summary>
            private int m_typeId;

            /// <summary>
            /// The version identifier of the user type that is being parsed.
            /// </summary>
            private int m_versionId;

            /// <summary>
            /// Most recent property read or (if it were missing) requested.
            /// </summary>
            /// <remarks>
            /// This is used to determine if the client is attempting to read
            /// properties in the wrong order.
            /// </remarks>
            private int m_prevProp = -1;

            /// <summary>
            /// The index of the next property in the POF stream.
            /// </summary>
            private int m_nextProp;

            /// <summary>
            /// The offset of the index of the next property to read.
            /// </summary>
            private long m_ofNextProp;

            /// <summary>
            /// The currently open nested reader, if any.
            /// </summary>
            private UserTypeReader m_readerNested;

            /// <summary>
            /// The property index of the property from which the currently open
            /// nested reader is reading from.
            /// </summary>
            private int m_iNestedProp;

            #endregion
        }

        #endregion

        #region Inner class: IdentityHolder
        
        /// <summary>
        /// Store the identity of an object read by a POF reader.
        /// </summary>
        public static class IdentityHolder
        {
            private static readonly LocalDataStoreSlot s_mapId = Thread.GetNamedDataSlot("MapId");

            /// <summary>
            /// store the identity info
            /// </summary>
            /// <param name="reader">stream reader</param>
            /// <param name="id">type Id</param>
            public static void Set(PofStreamReader reader, int id)
            {
                var mapId = (IDictionary) Thread.GetData(s_mapId);
                if (mapId == null)
                {
                    Thread.SetData(s_mapId, mapId = new HashDictionary());
                }
                mapId[reader] = id;
            }

            /// <summary>
            /// reset the identity info
            /// </summary>
            /// <param name="reader">stream reader</param>
            /// <param name="id">object Id</param>
            /// <param name="o">object to reset</param>
            public static void Reset(PofStreamReader reader, int id, object o)
            {
                var mapId = (IDictionary) Thread.GetData(s_mapId);
                if (mapId != null && mapId.Count > 0)
                {
                    while (reader != null)
                    {
                        object oValue = mapId[reader];
                        if (oValue != null)
                        {
                            var value = (int) oValue;
                            if (id == -1 || value == id)
                            {
                                reader.RegisterIdentity(value, o);
                                mapId.Remove(reader);
                            }
                            break;
                        }
                        reader = reader.ParentParser;
                    }
                }
            }
        }

        #endregion

        #region Data Members

        /// <summary>
        /// The <see cref="DataReader"/> containing the POF stream.
        /// </summary>
        protected internal DataReader m_reader;

        /// <summary>
        ///  The <see cref="IPofContext"/> to use to realize user data types
        /// as .NET objects.
        /// </summary>
        protected internal IPofContext m_ctx;

        /// <summary>
        /// Lazily-constructed mapping of identities to references.
        /// </summary>
        protected internal ILongArray m_referenceMap;

        #endregion
    }
}