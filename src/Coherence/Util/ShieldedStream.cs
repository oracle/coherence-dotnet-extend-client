/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.IO;

namespace Tangosol.Util
{
    /// <summary>
    /// Proxy stream that prevents the underlying stream from being
    /// closed.
    /// </summary>
    /// <author>Goran Milosavljevic  2008.08.13</author>
    public class ShieldedStream : Stream
    {
        #region Constructors

        /// <summary>
        /// Constructs a wrapper around underlying stream that should
        /// be shielded.
        /// </summary>
        /// <param name="stream">
        /// Underlying stream.
        /// </param>
        public ShieldedStream(Stream stream)
        {
            m_stream = stream;
        }

        #endregion

        #region Stream methods

        /// <summary>
        /// Overriden method that prevents underlying stream from being
        /// closed.
        /// </summary>
        public override void Close()
        { }

        /// <summary>
        /// Waits for the pending asynchronous read to complete.
        /// </summary>
        /// <param name="asyncResult">
        /// The reference to the pending asynchronous request to finish.
        /// </param>
        /// <value>
        /// The number of bytes read from the stream, between zero (0) and
        /// the number of bytes you requested. Streams return zero (0) only
        /// at the end of the stream, otherwise, they should block until at
        /// least one byte is available.
        /// </value>
        /// <returns>
        /// The number of bytes read from the stream or 0 if end of the stream
        /// is reached.
        /// </returns>
        public override int EndRead(IAsyncResult asyncResult)
        {
            return m_stream.EndRead(asyncResult);
        }

        /// <summary>
        /// Ends an asynchronous write operation.
        /// </summary>
        /// <param name="asyncResult">
        /// A reference to the outstanding asynchronous I/O request.
        /// </param>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            m_stream.EndWrite(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous read operation.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read the data into.
        /// </param>
        /// <param name="offset">
        /// The byte offset in buffer at which to begin writing data read
        /// from the stream.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to read.
        /// </param>
        /// <param name="callback">
        /// An optional asynchronous callback, to be called when the read is
        /// complete.
        /// </param>
        /// <param name="state">
        /// A user-provided object that distinguishes this particular
        /// asynchronous read request from other requests.
        /// </param>
        /// <value>
        /// An <see cref="System.IAsyncResult"/> that represents the
        /// asynchronous read, which could still be pending.
        /// </value>
        /// <returns>
        /// An <b>IAsyncResult</b>.
        /// </returns>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return m_stream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Begins an asynchronous write operation.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to write data from.
        /// </param>
        /// <param name="offset">
        /// The byte offset in buffer from which to begin writing.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to write.
        /// </param>
        /// <param name="callback">
        /// An optional asynchronous callback, to be called when the write
        /// is complete.
        /// </param>
        /// <param name="state">
        /// A user-provided object that distinguishes this particular
        /// asynchronous write request from other requests.
        /// </param>
        /// <value>
        /// An <b>IAsyncResult</b> that represents the asynchronous write,
        /// which could still be pending.
        /// </value>
        /// <returns>
        /// An <b>IAsyncResult</b>.
        /// </returns>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return m_stream.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this
        /// stream and causes any buffered data to be written to the
        /// underlying device.
        /// </summary>
        public override void Flush()
        {
            m_stream.Flush();
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <value>
        /// A long value representing the length of the stream in bytes.
        /// </value>
        public override long Length
        {
            get { return m_stream.Length; }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <value>
        /// The current position within the stream.
        /// </value>
        public override long Position
        {
            get { return m_stream.Position; }
            set { m_stream.Position = value; }
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances
        /// the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. When this method returns, the buffer contains
        /// the specified byte array with the values between offset and
        /// (offset + count - 1) replaced by the bytes read from the current
        /// source.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin storing
        /// the data read from the current stream.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to be read from the current stream.
        /// </param>
        /// <value>
        /// The total number of bytes read into the buffer. This can be less
        /// than the number of bytes requested if that many bytes are not
        /// currently available, or zero (0) if the end of the stream has
        /// been reached.
        /// </value>
        /// <returns>
        /// The number of bytes read from the stream or 0 if end of the stream
        /// is reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_stream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">
        /// A byte offset relative to the origin parameter.
        /// </param>
        /// <param name="origin">
        /// A value of type <see cref="System.IO.SeekOrigin"/> indicating the
        /// reference point used to obtain the new position.
        /// </param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return m_stream.Seek(offset, origin);
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the
        /// stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>
        /// The unsigned byte cast to an Int32, or -1 if at the end of the
        /// stream.
        /// </returns>
        public override int ReadByte()
        {
            return m_stream.ReadByte();
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">
        /// The desired length of the current stream in bytes.
        /// </param>
        public override void SetLength(long value)
        {
            m_stream.SetLength(value);
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the
        /// current position within this stream by the number of bytes
        /// written.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. This method copies count bytes from buffer to
        /// the current stream.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin copying
        /// bytes to the current stream.
        /// </param>
        /// <param name="count">
        /// The number of bytes to be written to the current stream.
        /// </param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            m_stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances
        /// the position within the stream by one byte.
        /// </summary>
        /// <param name="value">
        /// The byte to write to the stream.
        /// </param>
        public override void WriteByte(byte value)
        {
            m_stream.WriteByte(value);
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports
        /// reading.
        /// </summary>
        /// <value>
        /// <b>true</b> if the stream supports reading; otherwise,
        /// <b>false</b>.
        /// </value>
        public override bool CanRead
        {
            get { return m_stream.CanRead; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports
        /// seeking.
        /// </summary>
        /// <value>
        /// <b>true</b> if the stream supports seeking; otherwise,
        /// <b>false</b>.
        /// </value>
        public override bool CanSeek
        {
            get { return m_stream.CanSeek; }
        }

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
            get { return m_stream.CanWrite; }
        }

        #endregion

        #region Data members

        /// <summary>
        /// Underlying stream.
        /// </summary>
        private Stream m_stream;

        #endregion
    }
}