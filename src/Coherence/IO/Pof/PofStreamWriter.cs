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
using System.Runtime.CompilerServices;

using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// <see cref="IPofWriter"/> implementation that writes POF-encoded data
    /// to a POF stream.
    /// </summary>
    /// <author>Jason Howes  2006.07.11</author>
    /// <author>Goran Milosavljevic  2006.08.09</author>
    /// <since>Coherence 3.2</since>
    public class PofStreamWriter : IPofWriter
    {
        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IPofContext"/> object used by this
        /// <b>PofStreamWriter</b> to serialize user types into a POF stream.
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
        public virtual IPofContext PofContext
        {
            get { return m_ctx; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("PofContext cannot be null");
                }
                m_ctx = value;
            }
        }

        /// <summary>
        /// Gets the user type that is currently being written.
        /// </summary>
        /// <value>
        /// The user type information, or -1 if the <b>PofStreamWriter</b> is
        /// not currently writing a user type.
        /// </value>
        public virtual int UserTypeId
        {
            get { return -1; }
        }

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
        public virtual int VersionId
        {
            get { throw new InvalidOperationException("Not in a user type"); }
            set { throw new InvalidOperationException("Not in a user type"); }
        }

        /// <summary>
        /// Gets the <see cref="DataWriter"/> that this
        /// <b>PofStreamWriter</b> writes to.
        /// </summary>
        /// <value>
        /// The <b>DataWriter.</b>
        /// </value>
        internal virtual DataWriter Writer
        {
            get { return m_writer; }
        }

        /// <summary>
        /// Gets the <see cref="WritingPofHandler"/> used internally by this
        /// <b>PofStreamWriter</b> to write the POF stream.
        /// </summary>
        /// <value>
        /// The <b>IPofHandler.</b>
        /// </value>
        protected internal virtual WritingPofHandler PofHandler
        {
            get { return m_handler; }
        }

        /// <summary>
        /// Ensure that reference support (necessary for cyclic dependencies) is
        /// enabled.
        /// </summary>
        public virtual void EnableReference()
        {
            if (m_refs == null)
            {
                m_refs = new ReferenceLibrary();
            }
        }

        /// <summary>
        /// Determine if reference support is enabled.
        /// </summary>
        /// <value> <b>true</b> iff reference support is enabled
        /// </value>
        /// <returns>
        /// <b>true</b> if reference support is enabled; <b>false</b>, otherwise.
        /// </returns>
        /// <since>Coherence 3.7.1</since>
        public bool IsReferenceEnabled()
        {
            return m_refs != null;
        }

        /// <summary>
        /// Gets or sets the flag that indicate if the object to be written
        /// is either evolvable or part of an evolvable object.
        /// </summary>
        /// <value>
        /// True iff the object to be written is Evolvable.
        /// </value>
        /// <since>Coherence 3.7.1</since>
        protected virtual bool IsEvolvable
        {
            get { return m_evolvable; }
            set { m_evolvable = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a new <b>PofStreamWriter</b> that will write a POF
        /// stream to the passed <see cref="DataWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>DataWriter</b> object to write to; must not be
        /// <c>null</c>.
        /// </param>
        /// <param name="ctx">
        /// The <see cref="IPofContext"/> used by the new
        /// <b>PofStreamWriter</b> to serialize user types; must not be
        /// <c>null</c>.
        /// </param>
        public PofStreamWriter(DataWriter writer, IPofContext ctx)
        {
            Debug.Assert(writer != null, "DataWriter cannot be null");
            Debug.Assert(ctx != null, "IPofContext cannot be null");

            m_writer  = writer;
            m_ctx     = ctx;
            m_handler = new WritingPofHandler(writer);
        }

        /// <summary>
        /// Construct a new <b>PofStreamWriter</b> that will write a POF
        /// stream using the passed <see cref="WritingPofHandler"/>.
        /// </summary>
        /// <param name="handler">
        /// The <b>WritingPofHandler</b> used for writing; must not be
        /// <c>null</c>.
        /// </param>
        /// <param name="ctx">
        /// The <see cref="IPofContext"/> used by the new
        /// <b>PofStreamWriter</b> to serialize user types; must not be
        /// <c>null</c>.
        /// </param>
        public PofStreamWriter(WritingPofHandler handler, IPofContext ctx)
        {
            Debug.Assert(handler != null, "WritingPofHandler cannot be null");
            Debug.Assert(ctx != null, "PofContext cannot be null");

            m_writer  = handler.Writer;
            m_ctx     = ctx;
            m_handler = handler;
        }

        #endregion

        #region IPofWriter interface implementation

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
        public virtual void WriteBoolean(int index, bool value)
        {
            BeginProperty(index);
            try
            {
                PofHandler.OnBoolean(index, value);
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteByte(int index, byte value)
        {
            BeginProperty(index);
            try
            {
                PofHandler.OnOctet(index, value);
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteChar(int index, char value)
        {
            BeginProperty(index);
            try
            {
                PofHandler.OnChar(index, value);
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteInt16(int index, Int16 value)
        {
            BeginProperty(index);
            try
            {
                PofHandler.OnInt16(index, value);
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteInt32(int index, int value)
        {
            BeginProperty(index);
            try
            {
                PofHandler.OnInt32(index, value);
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteInt64(int index, long value)
        {
            BeginProperty(index);
            try
            {
                PofHandler.OnInt64(index, value);
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public void WriteRawInt128(int index, RawInt128 value)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (value.Value == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);
                    handler.OnInt128(index, value);
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }

            EndProperty(index);
        }

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
        public virtual void WriteSingle(int index, float value)
        {
            BeginProperty(index);
            try
            {
                PofHandler.OnFloat32(index, value);
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteDouble(int index, double value)
        {
            BeginProperty(index);
            try
            {
                PofHandler.OnFloat64(index, value);
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

        #endregion

        #region Primitive array support



        /// <summary>
        /// Write a <b>Boolean[]</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index
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
        public virtual void WriteBooleanArray(int index, bool[] array)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (array == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int elements = array.Length;

                    handler.RegisterIdentity(-1);
                    handler.BeginUniformArray(index, elements, PofConstants.T_BOOLEAN);
                    for (int i = 0; i < elements; ++i)
                    {
                        handler.OnBoolean(i, array[i]);
                    }
                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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

        public virtual void WriteByteArray(int index, byte[] array)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (array == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);
                    handler.BeginUniformArray(index, array.Length, PofConstants.T_OCTET);

                    m_writer.Write(array);

                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteCharArray(int index, char[] array)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (array == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int elements = array.Length;

                    handler.RegisterIdentity(-1);
                    handler.BeginUniformArray(index, elements, PofConstants.T_CHAR);
                    for (int i = 0; i < elements; ++i)
                    {
                        handler.OnChar(i, array[i]);
                    }
                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteInt16Array(int index, short[] array)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (array == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int elements = array.Length;

                    handler.RegisterIdentity(-1);
                    handler.BeginUniformArray(index, elements, PofConstants.T_INT16);
                    for (int i = 0; i < elements; ++i)
                    {
                        handler.OnInt16(i, array[i]);
                    }
                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteInt32Array(int index, int[] array)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (array == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int elements = array.Length;

                    handler.RegisterIdentity(-1);
                    handler.BeginUniformArray(index, elements, PofConstants.T_INT32);
                    for (int i = 0; i < elements; ++i)
                    {
                        handler.OnInt32(i, array[i]);
                    }
                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteInt64Array(int index, long[] array)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (array == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int elements = array.Length;

                    handler.RegisterIdentity(-1);
                    handler.BeginUniformArray(index, elements, PofConstants.T_INT64);
                    for (int i = 0; i < elements; ++i)
                    {
                        handler.OnInt64(i, array[i]);
                    }
                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteSingleArray(int index, Single[] array)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (array == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int elements = array.Length;

                    handler.RegisterIdentity(-1);
                    handler.BeginUniformArray(index, elements, PofConstants.T_FLOAT32);
                    for (int i = 0; i < elements; ++i)
                    {
                        handler.OnFloat32(i, array[i]);
                    }
                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteDoubleArray(int index, double[] array)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (array == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int elements = array.Length;

                    handler.RegisterIdentity(-1);
                    handler.BeginUniformArray(index, elements, PofConstants.T_FLOAT64);
                    for (int i = 0; i < elements; ++i)
                    {
                        handler.OnFloat64(i, array[i]);
                    }
                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteDecimal(int index, Decimal value)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                handler.RegisterIdentity(-1);

                switch (PofHelper.CalcDecimalSize(value))
                {
                    case 4:
                        handler.OnDecimal32(index, value);
                        break;

                    case 8:
                        handler.OnDecimal64(index, value);
                        break;

                    case 16:
                    default:
                        handler.OnDecimal128(index, value);
                        break;
                 }
            }
            catch (Exception e)
            {
                OnException(e);
            }

            EndProperty(index);
        }

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
        public virtual void WriteString(int index, string value)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (value == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);
                    handler.OnCharString(index, value);
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        /// If an I/O error occurs.</exception>
        public virtual void WriteDate(int index, DateTime value)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (value == DateTime.MinValue)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);
                    handler.OnDate(index, value.Year, value.Month, value.Day);
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteDateTime(int index, DateTime value)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (value == DateTime.MinValue)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);
                    handler.OnDateTime(index, value.Year, value.Month, value.Day,
                                       value.Hour, value.Minute, value.Second,
                                       value.Millisecond * 1000000, false); // UTC?
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

        /// <summary>
        /// Write a <b>DateTime</b> property to the POF stream in ISO8601
        /// format.
        /// </summary>
        /// <remarks>
        /// This method encodes the year, month, day, hour, minute, second,
        /// millisecond and timezone information of the specified
        /// <b>DateTime</b> object.
        /// <p/>
        /// Specified <paramref name="value"/> is converted to the local
        /// time before it is written to the POF stream.
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
        public virtual void WriteLocalDateTime(int index, DateTime value)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (value == DateTime.MinValue)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);
                    DateTime localValue =
                            (value.Kind == DateTimeKind.Unspecified
                                     ? DateTime.SpecifyKind(value, DateTimeKind.Local)
                                     : value.ToLocalTime());

                    // Due to a bug in TimeZone.CurrentTimeZone.GetUtcOffset() in .NET 4.5, GetUtcOffset() returns
                    // incorrect value for certain values of DateTime around daylight saving change dates.  Instead
                    // of calling TimeZone.CurrentTimeZone.GetUtcOffset(localValue) directly, we use the following to 
                    // workaround the problem.  See COH-8478 for more information.
                    TimeSpan daylightSavingZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime(localValue.Year, 6, 1, 12, 33, 40, DateTimeKind.Local));
                    TimeSpan standardZoneOffset       = TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime(localValue.Year, 12, 1, 12, 33, 40, DateTimeKind.Local));
                    TimeSpan zoneOffset               = localValue.IsDaylightSavingTime() ? daylightSavingZoneOffset : standardZoneOffset;
 
                    handler.OnDateTime(index, localValue.Year, localValue.Month, localValue.Day,
                                       localValue.Hour, localValue.Minute, localValue.Second,
                                       localValue.Millisecond * 1000000, zoneOffset);
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteUniversalDateTime(int index, DateTime value)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (value == DateTime.MinValue)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);
                    DateTime utcValue =
                            (value.Kind == DateTimeKind.Unspecified
                                     ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                                     : value.ToUniversalTime());
                    handler.OnDateTime(index, utcValue.Year, utcValue.Month, utcValue.Day,
                                       utcValue.Hour, utcValue.Minute, utcValue.Second,
                                       utcValue.Millisecond * 1000000, true);
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteRawDateTime(int index, RawDateTime value)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (value == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);

                    DateTime date = value.Date;
                    RawTime  time = value.Time;

                    if (time.HasTimezone)
                    {
                        if (time.IsUtc)
                        {
                            handler.OnDateTime(index,
                                    date.Year,
                                    date.Month,
                                    date.Day,
                                    time.Hour,
                                    time.Minute,
                                    time.Second,
                                    time.Nanosecond,
                                    true);
                        }
                        else
                        {
                            handler.OnDateTime(index,
                                    date.Year,
                                    date.Month,
                                    date.Day,
                                    time.Hour,
                                    time.Minute,
                                    time.Second,
                                    time.Nanosecond,
                                    new TimeSpan(time.HourOffset, time.MinuteOffset, 0));
                        }
                    }
                    else
                    {
                        handler.OnDateTime(index,
                                date.Year,
                                date.Month,
                                date.Day,
                                time.Hour,
                                time.Minute,
                                time.Second,
                                time.Nanosecond,
                                false);
                    }
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

        /// <summary>
        /// Write a <b>RawTime</b> property to the POF stream.
        /// </summary>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="time">
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
        public void WriteRawTime(int index, RawTime time)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (time == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);

                    if (time.HasTimezone)
                    {
                        if (time.IsUtc)
                        {
                            handler.OnTime(index,
                                    time.Hour,
                                    time.Minute,
                                    time.Second,
                                    time.Nanosecond,
                                    true);
                        }
                        else
                        {
                            handler.OnTime(index,
                                    time.Hour,
                                    time.Minute,
                                    time.Second,
                                    time.Nanosecond,
                                    new TimeSpan(time.HourOffset, time.MinuteOffset, 0));
                        }
                    }
                    else
                    {
                        handler.OnTime(index,
                                time.Hour,
                                time.Minute,
                                time.Second,
                                time.Nanosecond,
                                false);
                    }
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteTime(int index, DateTime value)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (value == DateTime.MinValue)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);
                    handler.OnTime(index, value.Hour, value.Minute, value.Second,
                                   value.Millisecond * 1000000, false); // UTC?
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

        /// <summary>
        /// Write a <see cref="DateTime"/> property to the POF stream
        /// in ISO8601 format.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method encodes the hour, minute, second, millisecond and
        /// timezone information of the specified <b>DateTime</b> object.
        /// No year, month or day information is encoded.</p>
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
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public void WriteLocalTime(int index, DateTime value)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (value == DateTime.MinValue)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);
                    DateTime localTime =
                            (value.Kind == DateTimeKind.Unspecified
                                     ? DateTime.SpecifyKind(value, DateTimeKind.Local)
                                     : value.ToLocalTime());
                    TimeSpan zoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(localTime);

                    handler.OnTime(index, localTime.Hour, localTime.Minute, localTime.Second,
                                           localTime.Millisecond * 1000000, zoneOffset);
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public void WriteUniversalTime(int index, DateTime value)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (value == DateTime.MinValue)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);
                    DateTime utcTime =
                            (value.Kind == DateTimeKind.Unspecified
                                     ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                                     : value.ToUniversalTime());
                    handler.OnTime(index, utcTime.Hour, utcTime.Minute, utcTime.Second,
                                           utcTime.Millisecond * 1000000, true); // UTC?
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteRawYearMonthInterval(int index, RawYearMonthInterval interval)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                handler.RegisterIdentity(-1);
                handler.OnYearMonthInterval(index,
                                            interval.Years,
                                            interval.Months);
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteTimeInterval(int index, TimeSpan interval)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                handler.RegisterIdentity(-1);
                handler.OnTimeInterval(index,
                                       interval.Hours,
                                       interval.Minutes,
                                       interval.Seconds,
                                       interval.Milliseconds * 1000000);
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteDayTimeInterval(int index, TimeSpan interval)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                handler.RegisterIdentity(-1);
                handler.OnDayTimeInterval(index,
                                          interval.Days,
                                          interval.Hours,
                                          interval.Minutes,
                                          interval.Seconds,
                                          interval.Milliseconds * 1000000);
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

        /// <summary>
        /// Write an <b>Object</b> property to the POF stream.
        /// </summary>
        /// <remarks>
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
        /// </list>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for the object must be
        /// obtainable from the <see cref="IPofContext"/> associated with this
        /// <b>PofStreamWriter</b>.</p>
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
        /// the POF stream, or if the given property cannot be encoded into
        /// a POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteObject(int index, object o)
        {
            // indicate to the handler that the subsequent value is of a
            // identifiable reference type
            PofHandler.RegisterIdentity(-1);

            switch (PofHelper.GetDotNetTypeId(o, PofContext))
            {
                case PofConstants.N_NULL:
                    BeginProperty(index);
                    try
                    {
                        PofHandler.OnNullReference(index);
                    }
                    catch (Exception e)
                    {
                        OnException(e);
                    }
                    EndProperty(index);
                    break;

                case PofConstants.N_BOOLEAN:
                    WriteBoolean(index, ((Boolean) o));
                    break;

                case PofConstants.N_BYTE:
                    WriteByte(index, (Byte) o);
                    break;

                case PofConstants.N_CHARACTER:
                    WriteChar(index, (Char) o);
                    break;

                case PofConstants.N_INT16:
                    WriteInt16(index, (Int16) o);
                    break;

                case PofConstants.N_INT32:
                    WriteInt32(index, (Int32) o);
                    break;

                case PofConstants.N_INT64:
                    WriteInt64(index, (Int64) o);
                    break;

                case PofConstants.N_SINGLE:
                    WriteSingle(index, (Single) o);
                    break;

                case PofConstants.N_DOUBLE:
                    WriteDouble(index, (Double) o);
                    break;

                case PofConstants.N_DECIMAL:
                    WriteDecimal(index, (Decimal) o);
                    break;

                case PofConstants.N_BINARY:
                    WriteBinary(index, (Binary) o);
                    break;

                case PofConstants.N_INT128:
                    WriteRawInt128(index, (RawInt128) o);
                    break;

                case PofConstants.N_STRING:
                    WriteString(index, (string) o);
                    break;

                case PofConstants.N_DATETIME:                    
                    DateTime dt = (DateTime) o;
                    switch (dt.Kind)
                    {
                        case DateTimeKind.Utc:
                            WriteUniversalDateTime(index, dt);
                            break;
                        case DateTimeKind.Local:
                            WriteLocalDateTime(index, dt);
                            break;
                        default:
                            WriteDateTime(index, dt);
                            break;
                    }
                    break;                    

                case PofConstants.N_DATE:
                    WriteDate(index, (DateTime) o);
                    break;

                case PofConstants.N_TIME:
                    WriteRawTime(index, (RawTime) o);
                    break;

                case PofConstants.N_YEAR_MONTH_INTERVAL:
                    WriteRawYearMonthInterval(index, (RawYearMonthInterval) o);
                    break;

                case PofConstants.N_TIME_INTERVAL:
                    WriteTimeInterval(index, (TimeSpan) o);
                    break;

                case PofConstants.N_DAY_TIME_INTERVAL:
                    WriteDayTimeInterval(index, (TimeSpan) o);
                    break;

                case PofConstants.N_BOOLEAN_ARRAY:
                    WriteBooleanArray(index, (bool[]) o);
                    break;

                case PofConstants.N_BYTE_ARRAY:
                    WriteByteArray(index, (byte[]) o);
                    break;

                case PofConstants.N_CHAR_ARRAY:
                    WriteCharArray(index, (char[]) o);
                    break;

                case PofConstants.N_INT16_ARRAY:
                    WriteInt16Array(index, (short[]) o);
                    break;

                case PofConstants.N_INT32_ARRAY:
                    WriteInt32Array(index, (int[]) o);
                    break;

                case PofConstants.N_INT64_ARRAY:
                    WriteInt64Array(index, (long[]) o);
                    break;

                case PofConstants.N_SINGLE_ARRAY:
                    WriteSingleArray(index, (float[]) o);
                    break;

                case PofConstants.N_DOUBLE_ARRAY:
                    WriteDoubleArray(index, (double[]) o);
                    break;

                case PofConstants.N_OBJECT_ARRAY:
                    WriteArray(index, (object[]) o);
                    break;

                case PofConstants.N_SPARSE_ARRAY:
                    WriteLongArray(index, (ILongArray) o);
                    break;

                case PofConstants.N_COLLECTION:
                    WriteCollection(index, (ICollection) o);
                    break;

                case PofConstants.N_DICTIONARY:
                    WriteDictionary(index, (IDictionary) o);
                    break;

                default:
                    WriteUserType(index, o);
                    break;
            }
        }

        /// <summary>
        /// Write an instance of a user type to the POF stream at the specified
        /// index.
        /// </summary>
        /// <param name="iProp">The property index.</param>
        /// <param name="o">The user type instance.</param>
        protected void WriteUserType(int iProp, Object o)
        {
            bool evolvableOld = IsEvolvable;
            bool evolvable    = evolvableOld || o is IEvolvable;

            IsEvolvable = evolvable;
            BeginProperty(iProp);

            try
            {
                WritingPofHandler handler = PofHandler;
                int               iRef    = -1;
                bool              fRef    = false;
                ReferenceLibrary  refs    = m_refs;

                // COH-5065: due to the complexity of maintaining references
                // in future data, we won't support them for evolvable objects
                if (refs != null && !IsEvolvable)
                {
                    iRef = refs.getIdentity(o);
                    if (iRef < 0)
                    {
                        iRef = refs.registerReference(o);
                    }
                    else
                    {
                        fRef = true;
                    }
                }

                if (fRef)
                {
                    handler.OnIdentityReference(iProp, iRef);
                }
                else
                {
                    IPofContext ctx = PofContext;

                    // resolve the user type identifier
                    int typeId = ctx.GetUserTypeIdentifier(o);

                    // create a new PofWriter for the user type
                    UserTypeWriter writer =
                            new UserTypeWriter(this, handler, ctx,
                                               typeId, iProp, iRef);
                    if (refs != null && !evolvable)
                    {
                        writer.EnableReference();
                    }

                    // serialize the object using a PofSerializer
                    ctx.GetPofSerializer(typeId).Serialize(writer, o);

                    // notify the nested PofWriter that it is closing
                    writer.CloseNested();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(iProp);
            IsEvolvable = evolvableOld;
        }

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
        public virtual void WriteBinary(int index, Binary bin)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (bin == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    handler.RegisterIdentity(-1);
                    handler.OnOctetString(index, bin);
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

        #endregion

        #region Collection support

        /// <summary>
        /// Write an <tt>Object[]</tt> property to the POF stream.
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
        /// Otherwise, an <see cref="IPofSerializer"/> for the object must be
        /// obtainable from the <see cref="IPofContext"/> associated with
        /// this <b>PofStreamWriter</b>.
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="array">
        /// The <b>Object</b> property value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid; or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream; or if the given property cannot be encoded into
        /// a POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteArray(int index, Array array)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (array == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int elements = array.Length;
                    handler.RegisterIdentity(-1);
                    handler.BeginArray(index, elements);
                    for (int i = 0; i < elements; ++i)
                    {
                        WriteObject(i, array.GetValue(i));
                    }
                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

        /// <summary>
        /// Write an uniform <tt>Object[]</tt> property to the POF stream.
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
        /// Otherwise, an <see cref="IPofSerializer"/> for the object must be
        /// obtainable from the <see cref="IPofContext"/> associated with
        /// this <b>PofStreamWriter</b>.</p>
        /// <p>
        /// Additionally, the type of each element must be equal to the
        /// specified class.</p>
        /// </remarks>
        /// <param name="index">
        /// The property index.
        /// </param>
        /// <param name="array">
        /// The <b>Object</b> property value to write.
        /// </param>
        /// <param name="type">
        /// The type of all elements; must not be <c>null</c>
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid; or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream; or if the given property cannot be encoded into
        /// a POF stream; or if the type of one or more elements of the array
        /// is not equal to the specified class.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteArray(int index, Array array, Type type)
        {
            // COH-3370: uniform arrays cannot contain null values
            for (int i = 0, c = array == null ? 0 : array.Length; i < c; ++i)
            {
                if (array.GetValue(i) == null)
                {
                    WriteArray(index, array);
                    return;
                }
            }

            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (array == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int typeId   = PofHelper.GetPofTypeId(type, PofContext);
                    int elements = array.Length;

                    handler.RegisterIdentity(-1);
                    handler.BeginUniformArray(index, elements, typeId);
                    for (int i = 0; i < elements; ++i)
                    {
                        object o = array.GetValue(i);
                        AssertEqual(type, o.GetType());
                        WriteObject(i, o);
                    }
                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

        // TODO: add support for LongArray

        /// <summary>
        /// Write an <b>ICollection</b> property to the POF stream.
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
        /// Otherwise, an <see cref="IPofSerializer"/> for each element of the
        /// array must be obtainable from the <see cref="IPofContext"/>
        /// associated with this <b>PofStreamWriter</b>.</p>
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
        /// the POF stream, or if the given property cannot be encoded into
        /// a POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteCollection(int index, ICollection coll)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (coll == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int elements = coll.Count;
                    int written  = 0;

                    handler.RegisterIdentity(-1);
                    handler.BeginCollection(index, elements);
                    foreach (object o in coll)
                    {
                        WriteObject(written++, o);
                    }

                    // check for under/overflow
                    if (written != elements)
                    {
                        throw new IOException("expected to write " + elements + " objects but actually wrote " +
                                              written);
                    }

                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

        /// <summary>
        /// Write a uniform <b>ICollection</b> property to the POF stream.
        /// </summary>
        /// <remarks>
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
        /// </list>
        /// <p>
        /// Otherwise, an <see cref="IPofSerializer"/> for each element of the
        /// collection must be obtainable from the <see cref="IPofContext"/>
        /// associated with this <b>PofStreamWriter</b>.</p>
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
        /// the POF stream; if the given property cannot be encoded into
        /// a POF stream; or if the type of one or more elements of the
        /// collection is not equal to the specified type.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteCollection(int index, ICollection coll, Type type)
        {
            // COH-3370: uniform collections cannot contain null values
            if (coll != null)
            {
                foreach (object o in coll)
                {
                    if (o == null)
                    {
                        WriteCollection(index, coll);
                        return;
                    }
                }
            }

            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (coll == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int typeId   = PofHelper.GetPofTypeId(type, PofContext);
                    int elements = coll.Count;
                    int written  = 0;

                    handler.RegisterIdentity(-1);
                    handler.BeginUniformCollection(index, elements, typeId);
                    foreach (object o in coll)
                    {
                        AssertEqual(type, o.GetType());
                        WriteObject(written++, o);
                    }

                    // check for under/overflow
                    if (written != elements)
                    {
                        throw new IOException("expected to write " + elements + " objects but actually wrote " +
                                              written);
                    }

                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteLongArray(int index, ILongArray la)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (la == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    long size    = la.LastIndex + 1;
                    int elements = la.Count;
                    int written  = 0;

                    if (elements > 0 && (la.FirstIndex < 0L || size > Int32.MaxValue))
                    {
                        throw new IndexOutOfRangeException("cannot encode LongArray["
                                + la.FirstIndex + ", " + la.LastIndex
                                + "] as a POF sparse array");
                    }

                    handler.RegisterIdentity(-1);
                    handler.BeginSparseArray(index, (int)size);
                    foreach (DictionaryEntry entry in la)
                    {
                        int n = Convert.ToInt32(entry.Key);
                        WriteObject(n, entry.Value);
                        written++;
                    }

                    // check for under/overflow
                    if (written != elements)
                    {
                        throw new IOException("expected to write " + elements
                            + " objects but actually wrote " + written);
                    }

                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteLongArray(int index, ILongArray la, Type type)
        {
            // COH-3370: uniform arrays cannot contain null values
            if (la != null && la.Contains(null))
            {
                WriteLongArray(index, la);
                return;
            }

            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (la == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    long size    = la.LastIndex + 1;
                    int typeId   = PofHelper.GetPofTypeId(type, PofContext);
                    int elements = la.Count;
                    int written  = 0;

                    if (elements > 0 && (la.FirstIndex < 0L || size > Int32.MaxValue))
                    {
                        throw new IndexOutOfRangeException("cannot encode LongArray["
                                + la.FirstIndex + ", " + la.LastIndex
                                + "] as a POF sparse array");
                    }

                    handler.RegisterIdentity(-1);
                    handler.BeginUniformSparseArray(index, (int)size, typeId);
                    foreach (DictionaryEntry entry in la)
                    {
                        int    pos   = Convert.ToInt32(entry.Key);
                        object value = entry.Value;

                        AssertEqual(type, value.GetType());
                        WriteObject(pos, value);
                        ++written;
                    }

                    // check for under/overflow
                    if (written != elements)
                    {
                        throw new IOException("expected to write " + elements
                            + " objects but actually wrote " + written);
                    }

                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteDictionary(int index, IDictionary dict)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (dict == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int elements = dict.Count;
                    int written  = 0;

                    handler.RegisterIdentity(-1);
                    handler.BeginMap(index, elements);
                    foreach (DictionaryEntry entry in dict)
                    {
                        object key   = entry.Key;
                        object value = entry.Value;

                        WriteObject(written, key); // index is ignored
                        WriteObject(written, value); // index is ignored
                        written++;
                    }
                    // check for under/overflow
                    if (written != elements)
                    {
                        throw new IOException("expected to write " + elements + " objects but actually wrote " +
                                              written);
                    }

                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteDictionary(int index, IDictionary dict, Type keyType)
        {
            // COH-3370: uniform maps cannot contain null keys
            if (dict != null)
            {
                foreach (DictionaryEntry entry in dict)
                {
                    if (entry.Key == null)
                    {
                        WriteDictionary(index, dict);
                        return;
                    }
                }
            }

            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (dict == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int typeId   = PofHelper.GetPofTypeId(keyType, PofContext);
                    int elements = dict.Count;
                    int written  = 0;

                    handler.RegisterIdentity(-1);
                    handler.BeginUniformKeysMap(index, elements, typeId);
                    foreach (DictionaryEntry entry in dict)
                    {
                        object key   = entry.Key;
                        object value = entry.Value;

                        AssertEqual(keyType, key.GetType());
                        WriteObject(written, key);   // index is ignored
                        WriteObject(written, value); // index is ignored
                        written++;
                    }

                    // check for under/overflow
                    if (written != elements)
                    {
                        throw new IOException("expected to write " + elements + " objects but actually wrote " +
                                              written);
                    }

                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteDictionary(int index, IDictionary dict, Type keyType, Type valueType)
        {
            // COH-3370: uniform maps cannot contain null keys or values
            if (dict != null)
            {
                foreach (DictionaryEntry entry in dict)
                {
                    if (entry.Key == null || entry.Value == null)
                    {
                        WriteDictionary(index, dict);
                        return;
                    }
                }
            }

            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (dict == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    IPofContext ctx = PofContext;

                    int keyTypeId   = PofHelper.GetPofTypeId(keyType, ctx);
                    int valueTypeId = PofHelper.GetPofTypeId(valueType, ctx);
                    int elements    = dict.Count;
                    int written     = 0;

                    handler.RegisterIdentity(-1);
                    handler.BeginUniformMap(index, elements, keyTypeId, valueTypeId);
                    foreach (DictionaryEntry entry in dict)
                    {
                        object key   = entry.Key;
                        object value = entry.Value;

                        AssertEqual(keyType, key.GetType());
                        AssertEqual(valueType, value.GetType());
                        WriteObject(written, key);      // index is ignored
                        WriteObject(written, value);    // index is ignored
                        written++;
                    }

                    // check for under/overflow
                    if (written != elements)
                    {
                        throw new IOException("expected to write " + elements + " objects but actually wrote " +
                                              written);
                    }

                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual void WriteCollection<T>(int index, ICollection<T> coll)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (coll == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int elements = coll.Count;
                    int written  = 0;

                    handler.RegisterIdentity(-1);

                    System.Type type = typeof(T);
                    if (type.IsSealed)
                    {
                        int typeId = PofHelper.GetPofTypeId(type, PofContext);
                        handler.BeginUniformCollection(index, elements, typeId);
                    }
                    else
                    {
                        handler.BeginCollection(index, elements);
                    }
                    foreach (T o in coll)
                    {
                        WriteObject(written++, o);
                    }

                    // check for under/overflow
                    if (written != elements)
                    {
                        throw new IOException("expected to write " + elements + " objects but actually wrote " +
                                              written);
                    }

                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);

        }

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
        public virtual void WriteDictionary<TKey, TValue>(int index, IDictionary<TKey, TValue> dict)
        {
            BeginProperty(index);
            try
            {
                IPofHandler handler = PofHandler;
                if (dict == null)
                {
                    handler.OnNullReference(index);
                }
                else
                {
                    int elements    = dict.Count;
                    int written     = 0;

                    handler.RegisterIdentity(-1);

                    System.Type typeKey   = typeof(TKey);
                    System.Type typeValue = typeof(TValue);
                    if (typeKey.IsSealed && typeValue.IsSealed)
                    {
                        int keyTypeId   = PofHelper.GetPofTypeId(typeKey, PofContext);
                        int valueTypeId = PofHelper.GetPofTypeId(typeValue, PofContext);
                        handler.BeginUniformMap(index, elements, keyTypeId, valueTypeId);
                    }
                    else
                    {
                        handler.BeginMap(index, elements);    
                    }
                    foreach (KeyValuePair<TKey, TValue> entry in dict)
                    {
                        TKey key     = entry.Key;
                        TValue value = entry.Value;
                        WriteObject(written, key); // index is ignored
                        WriteObject(written, value); // index is ignored
                        written++;
                    }

                    // check for under/overflow
                    if (written != elements)
                    {
                        throw new IOException("expected to write " + elements + " objects but actually wrote " +
                                              written);
                    }

                    handler.EndComplexValue();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }
            EndProperty(index);
        }

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
        public virtual IPofWriter CreateNestedPofWriter(int iProp)
        {
            throw new InvalidOperationException("not in a user type");
        }

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
        public virtual IPofWriter CreateNestedPofWriter(int iProp, int nTypeId)
        {
            throw new InvalidOperationException("not in a user type");
        }

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
        public virtual void WriteRemainder(Binary properties)
        {
            throw new InvalidOperationException("not in a user type");
        }

        #endregion

        #endregion

        #region Internal methods

        /// <summary>
        /// Report that a POF property is about to be written to the POF
        /// stream.
        /// </summary>
        /// <remarks>
        /// This method call will be followed by one or more separate calls
        /// to a "write" method and the property extent will then be
        /// terminated by a call to <see cref="EndProperty"/>.
        /// </remarks>
        /// <param name="index">
        /// The index of the property being written.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the property <paramref name="index"/> is invalid; or is less
        /// than or equal to the index of the previous property written to
        /// the POF stream.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        protected internal virtual void BeginProperty(int index)
        {
            if (index > 0 && PofHandler.GetComplex() == null)
            {
                throw new ArgumentException("not in a complex type");
            }
        }

        /// <summary>
        /// Signifies the termination of the current POF property.
        /// </summary>
        /// <param name="index">
        /// The index of the current property.
        /// </param>
        protected internal virtual void EndProperty(int index)
        { }

        /// <summary>
        /// Called when an unexpected exception is caught while writing to
        /// the POF stream.
        /// </summary>
        /// <remarks>
        /// If the given exception wraps an <b>IOException</b>, the
        /// <b>IOException</b> is unwrapped and rethrown; otherwise the given
        /// exception is rethrown.
        /// </remarks>
        /// <param name="e">
        /// The exception.
        /// </param>
        /// <exception cref="IOException">
        /// The wrapped <b>IOException</b>, if the given exception is a
        /// wrapped <b>IOException</b>.
        /// </exception>
        protected internal virtual void OnException(Exception e)
        {
            if (e.InnerException != null)
            {
                Exception eOrig = e.InnerException;
                if (eOrig is IOException)
                {
                    throw eOrig;
                }
            }

            throw e;
        }

        /// <summary>
        /// Assert that a class is equal to another class.
        /// </summary>
        /// <param name="type">
        /// The expected class; must not be <c>null</c>.
        /// </param>
        /// <param name="testType">
        /// The class to test for equality; must not be <c>null</c>
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the second class is not equal to the first.
        /// </exception>
        protected internal static void AssertEqual(Type type, Type testType)
        {
            if (!type.Equals(testType))
            {
                throw new ArgumentException("illegal type \"" + testType.FullName + "\"; expected \"" + type.FullName + '"');
            }
        }

        #endregion

        #region Inner class: UserTypeWriter

        /// <summary>
        /// The <b>UserTypeWriter</b> implementation is a contextually-aware
        /// <see cref="IPofWriter"/> whose purpose is to write the properties
        /// of a value of a specified user type.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The "contextual awareness" refers to the fact that the
        /// <b>UserTypeWriter</b> maintains state about the type identifier,
        /// the PofWriter's property index position within the user type
        /// value, and an <see cref="IPofContext"/> that may differ from the
        /// <b>IPofContext</b> that provided the <see cref="IPofSerializer"/>
        /// which is using this <b>UserTypeWriter</b> to serialize a user
        /// type.</p>
        /// </remarks>
        public class UserTypeWriter : PofStreamWriter
        {
            #region Properties

            /// <summary>
            /// Gets the user type that is currently being written.
            /// </summary>
            /// <value>
            /// The user type information, or -1 if the
            /// <b>PofStreamWriter</b> is not currently writing a user type.
            /// </value>
            public override int UserTypeId
            {
                get { return m_typeId; }
            }

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
            public override int VersionId
            {
                get { return m_versionId; }
                set
                {
                    if (value < 0)
                    {
                        throw new ArgumentException("negative version identifier: " + value);
                    }
                    m_versionId = value;
                }
            }

            /// <summary>
            /// If this writer is contextually within a user type, obtain the writer
            /// which created this writer in order to write the user type.
            /// </summary>
            public PofStreamWriter WriterParent { get; private set; }

            /// <summary>
            /// Gets the flag that indicate if the object to be written is
            /// either evolvable or part of an evolvable object.
            /// </summary>
            /// <value>
            /// True iff the object to be written is IEvolvable.
            /// </value>
            /// <since>Coherence 3.7.1</since>
            protected override bool IsEvolvable
            {
                get
                {
                    if (!m_evolvable)
                    {
                        PofStreamWriter parent = WriterParent;
                        if (parent != null)
                        {
                            m_evolvable = parent.IsEvolvable;
                        }
                    }
                    return m_evolvable;
                }

            }

            #endregion

            #region Constructors

            /// <summary>
            /// Construct a <b>UserTypeWriter</b> for writing the property
            /// values of a user type.
            /// </summary>
            /// <param name="writer">
            /// The <see cref="DataWriter"/> object to write to; must not be
            /// <c>null</c>.
            /// </param>
            /// <param name="ctx">
            /// The <see cref="IPofContext"/> to use for writing the user
            /// type property values within the user type that this writer
            /// will be writing.
            /// </param>
            /// <param name="typeId">
            /// The type identifier of the user type; must be non-negative.
            /// </param>
            /// <param name="index">
            /// The index of the user type being written.
            /// </param>
            public UserTypeWriter(DataWriter writer, IPofContext ctx, 
                int typeId, int index)
                : this(null, writer, ctx, typeId, index)
            {}

            /// <summary>
            /// Construct a <b>UserTypeWriter</b> for writing the property
            /// values of a user type.
            /// </summary>
            /// <param name="parent">
            /// the containing PofBufferWriter
            /// </param>
            /// <param name="writer">
            /// The <see cref="DataWriter"/> object to write to; must not be
            /// <c>null</c>.
            /// </param>
            /// <param name="ctx">
            /// The <see cref="IPofContext"/> to use for writing the user
            /// type property values within the user type that this writer
            /// will be writing.
            /// </param>
            /// <param name="typeId">
            /// The type identifier of the user type; must be non-negative.
            /// </param>
            /// <param name="index">
            /// The index of the user type being written.
            /// </param>
            public UserTypeWriter(PofStreamWriter parent, DataWriter writer,
                IPofContext ctx, int typeId, int index)
                : base(writer, ctx)
            {
                Debug.Assert(typeId >= 0);

                WriterParent = parent;
                m_typeId     = typeId;
                m_prop       = index;
                m_refs       = parent == null ? null : parent.m_refs;
            }

            /// <summary>
            /// Construct a <b>UserTypeWriter</b> for writing the property
            /// values of a user type.
            /// </summary>
            /// <param name="handler">
            /// The <see cref="WritingPofHandler"/> used to write user type
            /// data (except for the user type id itself, which is passed as
            /// a constructor argument).
            /// </param>
            /// <param name="ctx">
            /// The <see cref="IPofContext"/> to use for writing the user
            /// type property values within the user type that this writer
            /// will be writing.
            /// </param>
            /// <param name="typeId">
            /// The type identifier of the user type; must be non-negative.
            /// </param>
            /// <param name="index">
            /// The index of the user type being written.
            /// </param>
            public UserTypeWriter(WritingPofHandler handler, 
                IPofContext ctx, int typeId, int index)
                : this(null, handler, ctx, typeId, index)
            {}

            /// <summary>
            /// Construct a <b>UserTypeWriter</b> for writing the property
            /// values of a user type.
            /// </summary>
            /// <param name="parent">
            /// the containing PofBufferWriter
            /// </param>
            /// <param name="handler">
            /// The <see cref="WritingPofHandler"/> used to write user type
            /// data (except for the user type id itself, which is passed as
            /// a constructor argument).
            /// </param>
            /// <param name="ctx">
            /// The <see cref="IPofContext"/> to use for writing the user
            /// type property values within the user type that this writer
            /// will be writing.
            /// </param>
            /// <param name="typeId">
            /// The type identifier of the user type; must be non-negative.
            /// </param>
            /// <param name="index">
            /// The index of the user type being written.
            /// </param>
            public UserTypeWriter(PofStreamWriter parent, WritingPofHandler handler, 
                IPofContext ctx, int typeId, int index)
                : this(parent, handler, ctx, typeId, index, -1)
            {}

            /// <summary>
            /// Construct a <b>UserTypeWriter</b> for writing the property
            /// values of a user type.
            /// </summary>
            /// <param name="parent">
            /// the containing PofBufferWriter
            /// </param>
            /// <param name="handler">
            /// The <see cref="WritingPofHandler"/> used to write user type
            /// data (except for the user type id itself, which is passed as
            /// a constructor argument).
            /// </param>
            /// <param name="ctx">
            /// The <see cref="IPofContext"/> to use for writing the user
            /// type property values within the user type that this writer
            /// will be writing.
            /// </param>
            /// <param name="typeId">
            /// The type identifier of the user type; must be non-negative.
            /// </param>
            /// <param name="index">
            /// The index of the user type being written.
            /// </param>
            /// <param name="id">
            /// the identity of the object to encode, or -1 if the identity
            /// shouldn't be encoded in the POF stream
            /// </param>
            public UserTypeWriter(PofStreamWriter parent, WritingPofHandler handler, 
                IPofContext ctx, int typeId, int index, int id)
                : base(handler, ctx)
            {
                Debug.Assert(typeId >= 0);

                WriterParent = parent;
                m_typeId     = typeId;
                m_prop       = index;
                m_refs       = parent == null ? null : parent.m_refs;
                m_id         = id;
            }

            #endregion

            #region IPofWriter interface

            /// <summary>
            /// Write an <b>Object</b> property to the POF stream.
            /// </summary>
            /// <remarks>
            /// <p>
            /// The given object must be an instance (or an array of instances)
            /// of one of the following:
            /// <list type="bullet">
            /// <item>Boolean</item>
            /// <item>Byte</item>
            /// <item>Char</item>
            /// <item>Int16</item>
            /// <item>Int32</item>
            /// <item>Int64</item>
            /// <item>Single</item>
            /// <item>Double</item>
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
            /// Otherwise, an <see cref="IPofSerializer"/> for the object must
            /// be obtainable from the <see cref="IPofContext"/> associated
            /// with this <b>IPofWriter</b>.</p>
            /// </remarks>
            /// <param name="index">
            /// The property index.
            /// </param>
            /// <param name="o">
            /// The <b>Object</b> property value to write.
            /// </param>
            /// <exception cref="ArgumentException">
            /// If the property <paramref name="index"/> is invalid, or is
            /// less than or equal to the index of the previous property
            /// written to the POF stream, or if the given property cannot be
            /// encoded into a POF stream.
            /// </exception>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void WriteObject(int index, object o)
            {
                // force the user type information to be written, if necessary,
                // because otherwise the effect of a call to RegisterIdentity()
                // will be lost
                WriteUserTypeInfo();
                if (WriterParent != null)
                {
                    base.IsEvolvable = WriterParent.IsEvolvable;
                }
                base.WriteObject(index, o);
            }

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
            public override IPofWriter CreateNestedPofWriter(int iProp)
            {
                return CreateNestedPofWriter(iProp, UserTypeId);
            }

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
            /// <since> Coherence 3.6 </since>
            public override IPofWriter CreateNestedPofWriter(int iProp, int nTypeId)
            {
                BeginProperty(iProp);
                PofHandler.RegisterIdentity(-1);

                // create a new PofWriter for the user type
                IPofContext ctx = PofContext;
                UserTypeWriter writer = new UserTypeWriter(this,
                        PofHandler, ctx, nTypeId, iProp);
                if (IsReferenceEnabled())
                {
                    writer.EnableReference();
                }
                m_writerNested = writer;
                return writer;

                // note: there is no endProperty() call at this point, since the
                //       property has yet to be written
            }

            /// <summary>
            /// Write the remaining properties to the POF stream, terminating
            /// the writing of the currrent user type.
            /// </summary>
            /// <remarks>
            /// <p>
            /// As part of writing out a user type, this method must be
            /// called by the <see cref="IPofSerializer"/> that is writing
            /// out the user type, or the POF stream will be corrupted.</p>
            /// <p>
            /// Calling this method terminates the current user type by
            /// writing a -1 to the POF stream after the last indexed
            /// property. Subsequent calls to the various <b>WriteXYZ</b>
            /// methods of this interface will fail after this method is
            /// called.</p>
            /// </remarks>
            /// <param name="properties">
            /// A <b>Byte[]</b> object containing zero or more indexed
            /// properties in binary POF encoded form; may be <c>null</c>.
            /// </param>
            /// <exception cref="InvalidOperationException">
            /// If no user type is being written.
            /// </exception>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void WriteRemainder(Binary properties)
            {
                CloseNested();

                               // write out the type and version identifiers, if necessary
                WriteUserTypeInfo();

                try
                {
                    if (properties != null)
                    {
                        properties.WriteTo(Writer.BaseStream);
                    }
                    PofHandler.EndComplexValue();
                }
                catch (Exception e)
                {
                    OnException(e);
                }

                m_isUserTypeEnd = true; // EOF
                m_complex       = null;
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Report that a POF property is about to be written to the POF
            /// stream.
            /// </summary>
            /// <remarks>
            /// <p>
            /// This method call will be followed by one or more separate
            /// calls to a "write" method and the property extent will then
            /// be terminated by a call to <see cref="EndProperty"/>.</p>
            /// </remarks>
            /// <param name="index">
            /// The index of the property being written.
            /// </param>
            /// <exception cref="ArgumentException">
            /// If the property <paramref name="index"/> is invalid; or is
            /// less than or equal to the index of the previous property
            /// written to the POF stream.
            /// </exception>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            protected internal override void BeginProperty(int index)
            {
                // if a nested writer is still open, then "end" that property
                CloseNested();

                // check for negative index
                if (index < 0)
                {
                    throw new ArgumentException("negative property index: " + index);
                }

                // write out the type and version identifiers, if necessary
                WriteUserTypeInfo();

                // check for backwards movement
                if (PofHandler.GetComplex() == m_complex && index <= m_prevProp)
                {
                    throw new ArgumentException("previous property index=" + m_prevProp + ", requested property index=" +
                                                index + " while writing user type " + UserTypeId);
                }
            }

            /// <summary>
            /// Signifies the termination of the current POF property.
            /// </summary>
            /// <param name="index">
            /// The index of the current property.
            /// </param>
            protected internal override void EndProperty(int index)
            {
                if (PofHandler.GetComplex() == m_complex)
                {
                    m_prevProp = index;
                }
            }

            /// <summary>
            /// Notify the UserTypeWriter that it is being "closed". This
            /// notification allows the UserTypeWriter to write any remaining data
            /// that it has pending to write.
            /// </summary>
            public void CloseNested()
            {
                // check if a nested PofWriter is open
                UserTypeWriter writerNested = m_writerNested;
                if (writerNested != null)
                {
                    if (!writerNested.m_isUserTypeEnd)
                    {
                        PofHandler.EndComplexValue();
                    }
                    // close it
                    writerNested.CloseNested();

                    // finish writing the property that the nested PofWriter was
                    // writing into
                    EndProperty(writerNested.m_prop);

                    m_writerNested = null;
                }
            }

            /// <summary>
            /// Called when an unexpected exception is caught while writing
            /// to the POF stream.
            /// </summary>
            /// <remarks>
            /// If the given exception wraps an IOException, the IOException
            /// is unwrapped and rethrown; otherwise the given exception is
            /// rethrown.
            /// </remarks>
            /// <param name="e">
            /// The exception.
            /// </param>
            /// <exception cref="IOException">
            /// The wrapped <b>IOException</b>, if the given exception is a
            /// wrapped <b>IOException.</b></exception>
            protected internal override void OnException(Exception e)
            {
                m_isUserTypeEnd = true; // EOF
                m_complex       = null;

                base.OnException(e);
            }

            /// <summary>
            /// Write out the type and version identifiers of the user type
            /// to the POF stream, if they haven't already been written.
            /// </summary>
            /// <exception cref="IOException">
            /// On I/O error.
            /// </exception>
            protected internal virtual void WriteUserTypeInfo()
            {
                // check for EOF
                if (m_isUserTypeEnd)
                {
                    throw new EndOfStreamException("user type POF stream terminated");
                }

                if (!m_isUserTypeBegin)
                {
                    WritingPofHandler handler = PofHandler;
                    try
                    {
                        handler.BeginUserType(m_prop, m_id, UserTypeId, VersionId);
                    }
                    catch (Exception e)
                    {
                        OnException(e);
                    }
                    m_isUserTypeBegin = true;
                    m_complex         = handler.GetComplex();
                }
            }

            /// <summary>
            /// Ensure that reference support (necessary for cyclic dependencies) is
            /// enabled.
            /// </summary>
            public override void EnableReference()
            {
                if (m_refs == null)
                {
                    PofStreamWriter parent = WriterParent;
                    if (parent == null)
                    {
                        m_refs = new ReferenceLibrary();
                    }
                    else
                    {
                        parent.EnableReference();
                        m_refs = parent.m_refs;
                    }

                    UserTypeWriter child = m_writerNested;
                    if (child != null)
                    {
                        child.EnableReference();
                    }
                }
            }

            #endregion

            #region Data members

            /// <summary>
            /// The type identifier of the user type that is being written.
            /// </summary>
            protected internal int m_typeId;

            /// <summary>
            /// The version identifier of the user type that is being
            /// written.
            /// </summary>
            protected internal int m_versionId;

            /// <summary>
            /// The index of the user type being written.
            /// </summary>
            protected internal int m_prop;

            /// <summary>
            /// The identity of the object to encode, or -1 if the identity
            /// shouldn't be encoded in the POF stream
            /// </summary>
            protected int m_id = -1;

            /// <summary>
            /// The index of the last property written to the POF stream or
            /// -1 if the first property has yet to be written.
            /// </summary>
            protected internal int m_prevProp = -1;

            /// <summary>
            /// <b>true</b> if the type and version identifier of the user
            /// type was written to the POF stream.
            /// </summary>
            protected internal bool m_isUserTypeBegin;

            /// <summary>
            /// <b>true</b> if the user type was written to the POF stream.
            /// </summary>
            protected internal bool m_isUserTypeEnd;

            /// <summary>
            /// The <b>Complex</b> value that corresponds to the user type
            /// that is being written.
            /// </summary>
            protected internal WritingPofHandler.Complex m_complex;

            /// <summary>
            /// The currently open nested writer, if any.
            /// </summary>
            protected UserTypeWriter m_writerNested;

            #endregion
        }

        #endregion

        #region Inner class: ReferenceLibrary

        /// <summary>
        /// A "library" of object references and their corresponding identities in
        /// the POF stream.
        /// </summary>
        public class ReferenceLibrary
        {
            /// <summary>
            /// Look up an identity for an object.
            /// </summary>
            /// <param name="o">the object</param>
            /// <returns> the identity, or -1 if the object is not registered
            /// </returns>
            public int getIdentity(Object o)
            {
                int key = RuntimeHelpers.GetHashCode(o);
                return m_mapIdentities.ContainsKey(key)
                               ? (int) m_mapIdentities[key]
                               : -1;
            }

            /// <summary>
            /// Register an object.
            /// </summary>
            /// <param name="o"> the object </param>
            /// <returns>
            /// the assigned identity for the object
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// if the object is already registered
            /// </exception>
            public int registerReference(Object o)
            {
                Hashtable mapIdentities = m_mapIdentities;
                int       iRef          = ++m_cRefs;

                try
                {
                    mapIdentities.Add(RuntimeHelpers.GetHashCode(o), iRef);
                }
                catch
                {
                    throw new InvalidOperationException("object already registered");
                    --m_cRefs;
                }

                return iRef;
            }

            /// <summary>
            /// The reference counter.
            /// </summary>
            private int m_cRefs;

            /// <summary>
            /// A map from objects that can be referenced to their integer
            /// identities.
            /// </summary>
            public Hashtable m_mapIdentities = new Hashtable();
        }
        #endregion

        #region Data members

        /// <summary>
        /// The <b>Stream</b> object that the <b>PofStreamWriter</b> writes
        /// to.
        /// </summary>
        protected internal DataWriter m_writer;

        /// <summary>
        /// The <b>IPofContext</b> used by this <b>PofStreamWriter</b> to
        /// serialize user types.
        /// </summary>
        protected internal IPofContext m_ctx;

        /// <summary>
        /// A flag to indicate if the object to be written is either
        /// evolvable or part of an evolvable object.
        /// </summary>
        protected bool m_evolvable;

        /// <summary>
        /// The <b>WritingPofHandler</b> used to write a POF stream.
        /// </summary>
        protected internal WritingPofHandler m_handler;

        /// <summary>If references are used, then this is the ReferenceLibrary
        /// </summary>
        protected ReferenceLibrary m_refs;

        #endregion
    }
}