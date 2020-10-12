/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.IO.Compression;

using Tangosol.IO;

namespace Tangosol.Net
{
    /// <summary>
    /// Provides a compression wrapper for an <see cref="Stream"/>.
    /// </summary>
    /// <author>Cameron Purdy  2002.08.20</author>
    /// <author>Goran Milosavljevic  2008.08.07</author>
    [Obsolete ("Obsolete as of Coherence 3.7.")]
    public class CompressionFilter : IWrapperStreamFactory
    {
        #region IWrapperStreamFactory implementation

        /// <summary>
        /// Requests an output <b>Stream</b> that wraps the passed
        /// <b>Stream</b>.
        /// </summary>
        /// <param name="stream">
        /// The <b>Stream</b> to be wrapped.
        /// </param>
        /// <returns>
        /// A <b>Stream</b> that delegates to ("wraps") the passed
        /// <b>Stream</b>.
        /// </returns>
        public Stream GetOutputStream(Stream stream)
        {
            return new GZipOutputStream(stream);
        }

        /// <summary>
        /// Requests an input <b>Stream</b> that wraps the passed
        /// <b>Stream</b>.
        /// </summary>
        /// <param name="stream">
        /// The <b>Stream</b> to be wrapped.
        /// </param>
        /// <returns>
        /// A <b>Stream</b> that delegates to ("wraps") the passed
        /// <b>Stream</b>.
        /// </returns>
        public Stream GetInputStream(Stream stream)
        {
            return new GZipInputStream(stream);
        }

        #endregion

        #region GZipInputStream inner class

        /// <summary>
        /// Provides a wrapper around <see cref="GZipStream"/> that supports
        /// getting a current position within the stream.
        /// </summary>
        public class GZipInputStream : GZipStream
        {
            #region Properties

            /// <summary>
            /// Gets or sets the current position within the stream.
            /// </summary>
            public override long Position
            {
                get { return m_position; }
                set { throw new NotSupportedException(); }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructs new <b>GZipInputStream</b> with decompress option
            /// set.
            /// </summary>
            /// <param name="stream">
            /// The input stream.
            /// </param>
            public GZipInputStream(Stream stream)
                : base(stream, CompressionMode.Decompress)
            {
            }

            #endregion

            #region GZipStream methods

            /// <summary>
            /// Reads a number of decompressed bytes into the specified byte
            /// array.
            /// </summary>
            /// <param name="array">
            /// The array used to store decompressed bytes.
            /// </param>
            /// <param name="offset">
            /// The location in the array to begin reading.
            /// </param>
            /// <param name="count">
            /// The number of bytes decompressed.
            /// </param>
            /// <returns>
            /// The number of bytes that were decompressed into the byte
            /// array. If the end of the stream has been reached, zero or the
            /// number of bytes read is returned.
            /// </returns>
            public override int Read(byte[] array, int offset, int count)
            {
                int bytesRead = base.Read(array, offset, count);
                m_position += bytesRead;

                return bytesRead;
            }

            #endregion

            #region Data members

            /// <summary>
            /// Current position within the stream.
            /// </summary>
            private int m_position;

            #endregion
        }

        #endregion

        #region GZipOutputStream inner class

        /// <summary>
        /// Provides a wrapper around <see cref="GZipStream"/>.
        /// </summary>
        public class GZipOutputStream : GZipStream
        {
            #region Constructors

            /// <summary>
            /// Constructs new <b>GZipOutputStream</b> with compress option
            /// set.
            /// </summary>
            /// <param name="stream">
            /// The output stream.
            /// </param>
            public GZipOutputStream(Stream stream)
                : base(stream, CompressionMode.Compress)
            {
            }

            #endregion
        }

        #endregion
    }
}