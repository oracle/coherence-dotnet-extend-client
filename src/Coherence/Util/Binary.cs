/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using Tangosol.IO;

namespace Tangosol.Util
{
    /// <summary>
    /// A thread-safe immutable binary object.
    /// </summary>
    /// <author>Cameron Purdy  2002.01.25</author>
    /// <author>Ana Cikic  2008.06.01</author>
    /// <author>Aleksandar Seovic  2009.04.03</author>
    [Serializable]
    public sealed class Binary
    {
        #region Properties

        /// <summary>
        /// Determine the length of the Binary.
        /// </summary>
        /// <value>
        /// The number of bytes of data represented by this Binary.
        /// </value>
        public int Length
        {
            get { return m_cb; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for a Binary object.
        /// </summary>
        public Binary()
            : this(NO_BYTES, 0, 0)
        {}

        /// <summary>
        /// Construct a Binary object from a byte array.
        /// </summary>
        /// <param name="ab">
        /// An array of bytes.
        /// </param>
        public Binary(byte[] ab)
            : this(ab, 0, ab.Length, true)
        {}

        /// <summary>
        /// Construct a Binary on a portion of a byte array.
        /// </summary>
        /// <param name="ab">
        /// A byte array.
        /// </param>
        /// <param name="of">
        /// An offset into the byte array.
        /// </param>
        /// <param name="cb">
        /// The number of bytes to utilize.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="of"/> or <paramref name="cb"/> is negative,
        /// or <code>of + cb</code> is larger than <code>ab.Length</code>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If byte array is <c>null</c>.
        /// </exception>
        public Binary(byte[] ab, int of, int cb)
            : this(ab, of, cb, true)
        {}

        /// <summary>
        /// Construct a Binary on a portion of a byte array. This
        /// constructor allows internal methods to efficiently create
        /// Binary objects without forcing a copy.
        /// </summary>
        /// <param name="ab">
        /// A byte array.
        /// </param>
        /// <param name="of">
        /// An offset into the byte array.
        /// </param>
        /// <param name="cb">
        /// The number of bytes to utilize.
        /// </param>
        /// <param name="fCopy">
        /// <c>true</c> to force a copy of any mutable data.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="of"/> or <paramref name="cb"/> is negative,
        /// or <code>of + cb</code> is larger than <code>ab.Length</code>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If byte array is <c>null</c>.
        /// </exception>
        private Binary(byte[] ab, int of, int cb, bool fCopy)
        {
            if (cb == 0)
            {
                m_ab = NO_BYTES;
            }
            else if (fCopy)
            {
                byte[] abNew = new byte[cb];
                Array.Copy(ab, of, abNew, 0, cb);

                m_ab = abNew;
                m_cb = cb;
            }
            else
            {
                m_ab = ab;
                m_of = of;
                m_cb = cb;
            }
        }

        /// <summary>
        /// Construct a Binary from a Stream.
        /// </summary>
        /// <param name="stream">
        /// The Stream from which the Binary will load its data.
        /// </param>
        /// <param name="cb">
        /// The number of bytes to read from the Stream.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="cb"/> is negative.
        /// </exception>
        public Binary(Stream stream, int cb)
        {
            if (cb < 0)
            {
                throw new ArgumentOutOfRangeException("cb=" + cb);
            }
            byte[] ab = new byte[cb];
            stream.Read(ab, 0, cb);
            m_ab = ab;
            m_cb = cb;
        }

        /// <summary>
        /// Initializes a new Binary instance from
        /// <see cref="BinaryMemoryStream"/>.
        /// </summary>
        /// <param name="stream">
        /// A <b>BinaryMemoryStream</b> instance.
        /// </param>
        internal Binary(BinaryMemoryStream stream)
        {
            byte[] ab      = stream.GetInternalByteArray();
            int    cbData  = (int) stream.Length;
            int    cbTotal = ab.Length;
            int    cbWaste = cbTotal - cbData;

            // tolerate up to 12.5% waste
            if (cbWaste <= Math.Max(16, NumberUtils.URShift(cbTotal, 3)))
            {
                m_ab = ab;
            }
            else
            {
                byte[] abNew = new byte[cbData];
                Array.Copy(ab, 0, abNew, 0, cbData);
                m_ab = abNew;
            }
            m_cb = cbData;
        }

        #endregion

        #region Binary methods

        /// <summary>
        /// Returns a byte at the specified position.
        /// </summary>
        /// <param name="of">
        /// Offset of the byte to return.
        /// </param>
        /// <returns>
        /// Byte at the specified position.
        /// </returns>
        public byte ByteAt(int of)
        {
            if (of >= 0 && of < m_cb)
            {
                return m_ab[m_of + of];
            }
            else
            {
                throw new IndexOutOfRangeException("of=" + of
                    + ", length=" + m_cb);
            }
        }

        /// <summary>
        /// Write the contents of the Binary object to a Stream.
        /// </summary>
        /// <param name="stream">
        /// A Stream to write to.
        /// </param>
        public void WriteTo(Stream stream)
        {
            stream.Write(m_ab, m_of, m_cb);
        }

        /// <summary>
        /// Write the contents of the Binary object to a Stream.
        /// </summary>
        /// <param name="stream">
        /// A Stream to write to.
        /// </param>
        /// <param name="of">
        /// Offset of the first byte to write.
        /// </param>
        /// <param name="cb">
        /// The number of bytes to write.
        /// </param>
        public void WriteTo(Stream stream, int of, int cb)
        {
            stream.Write(m_ab, m_of + of, cb);
        }

        /// <summary>
        /// Write the contents of the Binary object to a Stream.
        /// </summary>
        /// <param name="writer">
        /// A Stream to write to.
        /// </param>
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(m_ab, m_of, m_cb);
        }

        /// <summary>
        /// Write the contents of the Binary object to a BinaryWriter.
        /// </summary>
        /// <param name="writer">
        /// A BinaryWriter to write to.
        /// </param>
        /// <param name="of">
        /// Offset of the first byte to write.
        /// </param>
        /// <param name="cb">
        /// The number of bytes to write.
        /// </param>
        public void WriteTo(BinaryWriter writer, int of, int cb)
        {
            writer.Write(m_ab, m_of + of, cb);
        }

        /// <summary>
        /// Get a Stream to read the Binary object's contents from.
        /// </summary>
        /// <returns>
        /// A Stream backed by this Binary object.
        /// </returns>
        public Stream GetStream()
        {
            return new BinaryMemoryStream(m_ab, m_of, m_cb);
        }

        /// <summary>
        /// Get a Stream to read the Binary object's contents from.
        /// </summary>
        /// <param name="of">
        /// Offset of the first byte.
        /// </param>
        /// <param name="cb">
        /// The number of bytes.
        /// </param>
        /// <returns>
        /// A Stream backed by this Binary object.
        /// </returns>
        public Stream GetStream(int of, int cb)
        {
            return new BinaryMemoryStream(m_ab, m_of + of, cb);
        }

        /// <summary>
        /// Get a Binary representing a subset of this Binary.
        /// </summary>
        /// <remarks>
        /// This method simply returns a limited "view" into the
        /// underlying Binary it is invoked on, without allocating
        /// new buffer to hold the data.
        /// </remarks>
        /// <param name="of">
        /// Offset of the first byte.
        /// </param>
        /// <param name="cb">
        /// The number of bytes.
        /// </param>
        /// <returns>
        /// A new Binary view, backed by this Binary object.
        /// </returns>
        public Binary GetBinary(int of, int cb)
        {
            return new Binary(m_ab, m_of + of, cb, false);
        }

        /// <summary>
        /// Return a DataReader to read this Binary's contents from.
        /// </summary>
        /// <returns>
        /// A DataReader backed by this Binary.
        /// </returns>
        public DataReader GetReader()
        {
            return new DataReader(GetStream());
        }

        /// <summary>
        /// Get the contents of the Binary as a byte array.
        /// </summary>
        /// <remarks>
        /// This is the equivalent of
        /// <code>ToByteArray(0, Length)</code>.
        /// </remarks>
        /// <returns>
        /// A byte array with the contents of this Binary.
        /// </returns>
        public byte[] ToByteArray()
        {
            return ToByteArray(m_of, m_cb);
        }

        /// <summary>
        /// Get a portion of the contents of the Binary as a byte array.
        /// </summary>
        /// <param name="of">
        /// The beginning index, inclusive.
        /// </param>
        /// <param name="cb">
        /// The number of bytes to include in the resulting byte array.
        /// </param>
        /// <returns>
        /// A byte array containing the specified portion of this Binary.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="of"/> or <paramref name="cb"/> is negative,
        /// or <code>of + cb</code> is larger than the length of this Binary.
        /// </exception>
        public byte[] ToByteArray(int of, int cb)
        {
            // validate parameters
            int cbBuf = m_cb;
            if (of < 0 || cb < 0 || of + cb > cbBuf)
            {
                throw new ArgumentOutOfRangeException("of=" + of + ", cb=" + cb + ", length()=" + cbBuf);
            }

            if (cb == 0)
            {
                return NO_BYTES;
            }

            byte[] abNew;
            byte[] ab = m_ab;

            // adjust offset based on what part of the underlying byte[] this
            // Binary is "over"
            of += m_of;

            if (of == 0 && cb == ab.Length)
            {
                // just clone our byte array and hand back the clone
                abNew = (byte[]) ab.Clone();
            }
            else
            {
                abNew = new byte[cb];
                Array.Copy(ab, of, abNew, 0, cb);
            }

            return abNew;
        }

        /// <summary>
        /// Calculate the partition ID to which the specified <b>Binary</b> should be
        /// naturally assigned. This calculation should not be applied to <b>Binary</b>
        /// objects <i>decorated</i> with artificially assigned partitions.
        /// </summary>
        /// <remarks>
        /// The resulting partition ID will be in the range <tt>[0..cPartitions)</tt>.
        /// </remarks>
        /// <param name="cPartitions">
        /// The partition count.
        /// </param>
        /// <returns>
        /// The partition that the this <b>Binary</b> is naturally assigned to.
        /// </returns>
        public int CalculateNaturalPartition(int cPartitions)
        {
            long lHash = ((long) GetHashCode()) & 0xFFFFFFFFL;
            return cPartitions == 0 ? (int) lHash : (int) (lHash % (long) cPartitions);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Determine a hash value for the <b>Binary</b> object.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>Binary</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            int hash = m_hash;

            if (hash == 0)
            {
                // cache the CRC32 result
                uint crc = NumberUtils.ToCrc(m_ab, m_of, m_cb);

                // crc is uint value
                // Convert.ToInt32 will not work, using BitConverter instead
                // to get bytes representing uint, then convert bytes to int
                byte[] bytes = BitConverter.GetBytes(crc);

                hash = BitConverter.ToInt32(bytes, 0);
                if (hash == 0)
                {
                    // to allow for caching of the hashcode
                    hash = 17;
                }

                m_hash = hash;
            }

            return hash;
        }

        /// <summary>
        /// Compare this object with another object to determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with current object.
        /// </param>
        /// <returns>
        /// <b>true</b> if this object and the passed object are equivalent
        /// <b>Binary</b> objects.
        /// </returns>
        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }

            if (o is Binary)
            {
                Binary that = (Binary) o;

                // compare length (a quick way to disprove equality)
                int cbThis = this.m_cb;
                int cbThat = that.m_cb;
                if (cbThis == cbThat)
                {
                    // 0-length binary values are identical
                    if (cbThis == 0)
                    {
                        return true;
                    }

                    // compare hash-code (another quick way to disprove equality)
                    int nThisHash = this.m_hash;
                    int nThatHash = that.m_hash;
                    if (nThisHash == 0 || nThatHash == 0 || nThisHash == nThatHash)
                    {
                        // brute force byte-by-byte comparison
                        return Equals(this.m_ab, this.m_of,
                                      that.m_ab, that.m_of, cbThat);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Return a human-readable description for this <b>Binary</b>.
        /// </summary>
        /// <returns>
        /// A string description of the object.
        /// </returns>
        public override string ToString()
        {
            return "Binary(length=" + m_cb + ", value="
                + NumberUtils.ToHexEscape(m_ab, m_of, m_cb) + ')';
        }

        #endregion

        #region Static helper methods

        /// <summary>
        /// Compare two binary regions, testing for equality.
        /// </summary>
        /// <param name="ab1">
        /// The byte array containing the first binary region to compare.
        /// </param>
        /// <param name="of1">
        /// The offset of the binary region within <paramref name="ab1"/>.
        /// </param>
        /// <param name="ab2">
        /// The byte array containing the second binary region to compare.
        /// </param>
        /// <param name="of2">
        /// The offset of the binary region within <paramref name="ab2"/>.
        /// </param>
        /// <param name="cb">
        /// The size of the binary regions, which is the number of bytes to
        /// compare.
        /// </param>
        /// <returns>
        /// <c>true</c> iff the two specified binary regions are identical.
        /// </returns>
        public static bool Equals(byte[] ab1, int of1, byte[] ab2, int of2, int cb)
        {
            try
            {
                while (--cb >= 0)
                {
                    if (ab1[of1++] != ab2[of2++])
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("ab1=" + ToString(ab1)
                                    + ", of1=" + of1 + ", ab2=" + ToString(ab2)
                                    + ", of2=" + of2 + ", cb=" + cb, e);
            }
        }

        /// <summary>
        /// Create a clone of the specified binary region.
        /// </summary>
        /// <param name="ab">
        /// The byte array containing the binary region to copy from.
        /// </param>
        /// <param name="of">
        /// The offset of the binary region within <tt>ab</tt>.
        /// </param>
        /// <param name="cb">
        /// The size in bytes of the binary region to copy.
        /// </param>
        /// <returns>
        /// The specified binary region cloned.
        /// </returns>
        public static byte[] Clone(byte[] ab, int of, int cb)
        {
            try
            {
                byte[] abNew = new byte[cb];
                Array.Copy(ab, of, abNew, 0, cb);
                return abNew;
            }
            catch (Exception e)
            {
                throw new Exception("ab=" + ToString(ab)
                                    + ", of=" + of + ", cb=" + cb, e);
            }
        }

        /// <summary>
        /// For debugging purposes, convert the passed byte array into a string
        /// that contains the information regarding whether the reference is null,
        /// and if it is not null, what the length of the byte array is.
        /// </summary>
        /// <param name="ab">
        /// a byte array; may be null
        /// </param>
        /// <returns>
        /// a String; never null
        /// </returns>
        public static String ToString(byte[] ab)
        {
            return ab == null ? "null" : ("byte[" + ab.Length + "]");
        }

        #endregion


        #region Constants

        /// <summary>
        /// An empty byte array (by definition immutable).
        /// </summary>
        public static readonly byte[] NO_BYTES = new byte[0];

        /// <summary>
        /// An empty Binary object.
        /// </summary>
        public static readonly Binary NO_BINARY = new Binary(NO_BYTES);

        #endregion

        #region Data members

        /// <summary>
        /// The byte array that holds the binary data.
        /// </summary>
        /// <remarks>
        /// This value should not be changed.
        /// </remarks>
        private readonly byte[] m_ab;

        /// <summary>
        /// Offset of the first byte of this Binary within the backing
        /// byte array.
        /// </summary>
        private readonly int m_of;

        /// <summary>
        /// The number of bytes of data represented by this Binary object.
        /// </summary>
        private readonly int m_cb;

        /// <summary>
        /// Cached hash code.
        /// </summary>
        [NonSerialized]
        private int m_hash;

        #endregion
    }
}
