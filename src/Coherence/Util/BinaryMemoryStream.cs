/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

namespace Tangosol.Util
{
    /// <summary>
    /// A <see cref="MemoryStream"/> implementation whose primary purpose is
    /// to be used to create <see cref="Binary"/> objects.
    /// </summary>
    /// <author>Cameron Purdy  2005.06.02</author>
    /// <author>Ana Cikic  2008.06.01</author>
    public sealed class BinaryMemoryStream : MemoryStream
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the BinaryMemoryStream with an
        /// expandable capacity initialized to zero.
        /// </summary>
        public BinaryMemoryStream()
        {}

        /// <summary>
        /// Initializes a new instance of the BinaryMemoryStream with an
        /// expandable capacity initialized as specified.
        /// </summary>
        /// <param name="capacity">
        /// The initial size of the internal array in bytes.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Capacity is negative.
        /// </exception>
        public BinaryMemoryStream(int capacity) : base(capacity)
        {}

        /// <summary>
        /// Initializes a new non-resizable instance of the BinaryMemoryStream
        /// based on the specified region of a byte array.
        /// </summary>
        /// <remarks>
        /// Initialized stream is read-only.
        /// </remarks>
        /// <param name="ab">
        /// The array of bytes from which to create the stream.
        /// </param>
        /// <param name="of">
        /// The offset into the given array at which the stream begins.
        /// </param>
        /// <param name="cb">
        /// The length of the stream in bytes.
        /// </param>
        internal BinaryMemoryStream(byte[] ab, int of, int cb) : base(ab, of, cb)
        {
            m_isReadOnly = true;
        }
        
        #endregion

        #region MemoryStream overrides

        /// <summary>
        /// Gets a value indicating whether the current stream supports
        /// writing.
        /// </summary>
        /// <value>
        /// <b>true</b> if the stream supports writing; otherwise,
        /// <b>false</b>.
        /// </value>
        public override bool CanWrite
        {
            get { return !m_isReadOnly && base.CanWrite; }
        }

        /// <summary>
        /// Writes a block of bytes to the current stream using data read
        /// from buffer.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to write data from.
        /// </param>
        /// <param name="offset">
        /// The byte offset in buffer at which to begin writing from.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to write.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Buffer is <c>null</c>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The stream does not support writing or the current position is
        /// closer than count bytes to the end of the stream, and the
        /// capacity cannot be modified.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Offset subtracted from the buffer length is less than count.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Offset or count are negative.
        /// </exception>
        /// <exception cref="IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// The current stream instance is closed.
        /// </exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (CheckWriteable())
            {
                base.Write(buffer, offset, count);
            }
        }

        /// <summary>
        /// Writes a byte to the current stream at the current position.
        /// </summary>
        /// <param name="value">
        /// The byte to write.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// The stream does not support writing or the current position is at
        /// the end of the stream, and the capacity cannot be modified.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// The current stream is closed.
        /// </exception>
        public override void WriteByte(byte value)
        {
            if (CheckWriteable())
            {
                base.WriteByte(value);
            }
        }

        /// <summary>
        /// Sets the length of the current stream to the specified value.
        /// </summary>
        /// <param name="value">
        /// The value at which to set the length.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// The current stream is not resizable and value is larger than the
        /// current capacity or the current stream does not support writing.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Value is negative or is greater than the maximum length of the
        /// stream, where the maximum length is
        /// <b>(System.Int32.MaxValue - origin)</b>, and origin is the index
        /// into the underlying buffer at which the stream starts.
        /// </exception>
        public override void SetLength(long value)
        {
            if (CheckWriteable())
            {
                base.SetLength(value);
            }
        }

        /// <summary>
        /// Returns the array of unsigned bytes from which this stream was
        /// created.
        /// </summary>
        /// <returns>
        /// The byte array from which this stream was created.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Buffer is not publicly visible.
        /// </exception>
        public override byte[] GetBuffer()
        {
            throw new UnauthorizedAccessException();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Returns a new <see cref="Binary"/> object that holds the complete
        /// contents of this stream.
        /// </summary>
        /// <returns>
        /// The contents of this stream as a <b>Binary</b> object.
        /// </returns>
        public Binary ToBinary()
        {
            return new Binary(this);
        }

        /// <summary>
        /// Obtain the internal byte array that this stream uses.
        /// </summary>
        /// <remarks>
        /// Package private, for use only by <see cref="Binary"/>.
        /// </remarks>
        /// <returns>
        /// The actual byte array that this stream uses.
        /// </returns>
        internal byte[] GetInternalByteArray()
        {
            m_isReadOnly = true;
            return base.GetBuffer();
        }

        /// <summary>
        /// Checks if stream is writeable.
        /// </summary>
        /// <returns>
        /// <b>true</b> if stream is writeable.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// If stream is not writeable.
        /// </exception>
        /// <seealso cref="CanWrite"/>.
        private bool CheckWriteable()
        {
            if (m_isReadOnly)
            {
                throw new NotSupportedException("BinaryMemoryStream is immutable");
            }
            return true;
            //            bool canWrite = CanWrite;
            //            if (!canWrite)
            //            {
            //                throw new NotSupportedException("BinaryMemoryStream is immutable");
            //            }
            //            return canWrite;
        }

        #endregion

        #region Data members

        /// <summary>
        /// Indicator that no more modifications are permitted.
        /// </summary>
        private bool m_isReadOnly;

        #endregion
    }
}