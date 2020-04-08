/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using System.Text;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util;


namespace Tangosol.Net.Partition
{
    /// <summary>
    /// PartitionSet is a light-weight data structure that represents a set of
    /// partitions that are used in parallel processing. This set quite often
    /// accompanies a result of partial parallel execution and is used to determine
    /// whether or not the entire set of partitions was successfully processed.
    /// 
    /// Note that all PartitionSet operations that take another set as an argument
    /// assume that both sets have the same partition count.
    /// 
    /// <author>tb 2011.05.26</author>
    /// <since>Coherence 3.7.1</since>
    /// </summary>
    public class PartitionSet : IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor (necessary for IPortableObject interface).
        /// </summary>
        public PartitionSet()
        { }

        #endregion

        #region pseudo Set operations

        /// <summary>
        /// Add the specified partition to the set.
        /// </summary>
        /// <param name="nPartition">
        /// The partition to add.
        /// </param>
        /// <returns>
        /// True if the specified partition was actually added as a result
        /// of this call; false otherwise.
        /// </returns>
        public bool Add(int nPartition)
        {
            if (nPartition < 0 || nPartition >= m_cPartitions)
            {
                throw new IndexOutOfRangeException(
                    nPartition + " € [0, " + m_cPartitions + ')');
            }

            long[] alBits = m_alBits;
            int    iLong  = (int)((uint)nPartition >> 6);
            long   lBits  = alBits[iLong];
            long   lMask  = 1L << (nPartition & 63);

            if ((lBits & lMask) == 0L)
            {
                alBits[iLong] = lBits | lMask;

                int cMarked = m_cMarked;
                if (cMarked >= 0)
                {
                    m_cMarked = cMarked + 1;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Add the specified PartitionSet to this set.
        /// </summary>
        /// <param name="partitions">
        /// The PartitionSet to add.
        /// </param>
        /// <returns>
        /// True if all of the partitions were actually added as a result
        /// of this call; false otherwise.
        /// </returns>
        public bool Add(PartitionSet partitions)
        {
            int    cPartitions = m_cPartitions;
            long[] alBitsThis  = this.m_alBits;
            long[] alBitsThat  = partitions.m_alBits;
            bool   fResult     = true;

            for (int i = 0, c = alBitsThis.Length; i < c; ++i)
            {
                long lBitsThis = alBitsThis[i];
                long lBitsThat = alBitsThat[i];

                fResult &= (lBitsThis & lBitsThat) == 0L;
                alBitsThis[i] = lBitsThis | lBitsThat;
            }

            m_cMarked = -1;
            return fResult;
        }

        /// <summary>
        /// Fill the set to contain all the partitions.
        /// </summary>
        public void Fill()
        {
            Int64[] alBits = m_alBits;
            int     iLast  = alBits.Length - 1;

            for (int i = 0; i < iLast; ++i)
            {
                alBits[i] = -1L;
            }
            alBits[iLast] = m_lTailMask;

            m_cMarked = m_cPartitions;
        }

        /// <summary>
        /// Return an index of the first marked partition that is greater than or
        /// equal to the specified partition. If no such partition exists then -1 is
        /// returned.
        /// 
        /// This method could be used to iterate over all marked partitions:
        /// <pre>
        /// for (int i = ps.Next(0); i >= 0; i = ps.Next(i+1))
        ///     {
        ///     // process partition
        ///     }
        /// </pre>
        /// 
        /// </summary>
        /// <param name="nPartition">
        /// The partition to start checking from (inclusive).
        /// </param>
        /// <returns>
        /// The next marked partition, or -1 if no next marked partition
        /// exists in the set
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// If the specified partition is invalid.
        /// </exception>
        public int Next(int nPartition)
        {
            int cPartitions = m_cPartitions;
            if (nPartition < 0 || nPartition > cPartitions)
            {
                throw new IndexOutOfRangeException("invalid partition: " + 
                    nPartition + " € [0, " + cPartitions + ')');
            }

            if (nPartition == cPartitions || m_cMarked == 0)
            {
                return -1;
            }

            long[] alBits = m_alBits;
            int    iLong  = (int)((uint)nPartition >> 6);
            int    ofBit  = nPartition & 63;
            long   lBits  = (long)((ulong)alBits[iLong] >> ofBit);

            if (lBits == 0L)
            {
                ofBit = 0;

                // skip empty parts
                for (int iLast = alBits.Length - 1; lBits == 0L && iLong < iLast; )
                {
                    lBits = alBits[++iLong];
                }

                if (lBits == 0L)
                {
                    return -1;
                }
            }

            return (iLong << 6) + ofBit + getTrailingZeroCount(lBits);
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Determine the number of trailing zero bits in the passed long value.
        /// </summary>
        /// <param name="l">
        /// A long value.
        /// </param>
        /// <returns>
        /// The number of trailing zero bits in the value, from 0
        /// (indicating that the least significant bit is set) to 64
        /// (indicating that no bits are set).
        /// </returns>
        protected static int getTrailingZeroCount(long l)
            {
            if (l == 0)
                {
                return 64;
                }

            l = l & -l; // Long.lowestOneBit(l)

            // Long.numberOfTrailingZeros(l);
            int x;
            int y;
            int n = 63;

            y = (int) l; if (y != 0) { n = n -32; x = y; } else x = (int) (((ulong) l)>>32);

            y = x << 16; if (y != 0) { n = n -16; x = y; }
            y = x <<  8; if (y != 0) { n = n - 8; x = y; }
            y = x <<  4; if (y != 0) { n = n - 4; x = y; }
            y = x <<  2; if (y != 0) { n = n - 2; x = y; }

            return n - (int)(((uint) (x << 1)) >> 31);
            }

        #endregion

        #region IPortableObject implementation

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
            // 0: partition-count
            // 1: format-indicator
            // 2: int array of gaps (for MARKED_FEW format)
            // 3: long array of bit masks (for MARKED_MANY format)
            // 4: reserved

            int    cPartitions = reader.ReadInt32(0);
            Format format      = (Format)reader.ReadInt32(1);
            int    cLongs      = (int)((uint)(cPartitions + 63) >> 6);

            Int64[] alBits = format == Format.MarkedMany
                               ? reader.ReadInt64Array(3)
                               : new Int64[cLongs];

            m_cPartitions = cPartitions;
            m_alBits      = alBits;
            m_lTailMask   = (long) (-1L >> (64 - (cPartitions & 63)));
            m_cMarked     = -1;

            switch (format)
            {
                case Format.MarkedNone:
                    m_cMarked = 0;
                    break;

                case Format.MarkedFew:
                    {
                        Int32[] acSkip = reader.ReadInt32Array(2);
                        int     cSkips = acSkip.Length;
                        for (int i = 0, iLast = 0; i < cSkips; ++i)
                        {
                            iLast += acSkip[i];
                            Add(iLast);
                        }
                        m_cMarked = cSkips;
                    }
                    break;

                case Format.MarkedMany:
                    // handled above
                    break;

                case Format.MarkedAll:
                    Fill();
                    break;

                default:
                    throw new IOException("stream corrupted; format=" + format);
            }
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
            int cPartitions = m_cPartitions;
            writer.WriteInt32(0, cPartitions);
            writer.WriteInt32(1, (int)Format.MarkedMany);
            writer.WriteInt64Array(3, m_alBits);
        }

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns a string representation of the <b>PartitionSet</b>.
        /// </summary>
        /// <returns>
        /// A string representation of the <b>PartitionSet</b>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb      = new StringBuilder("PartitionSet{");
            bool          fAppend = false;
            int           cRange  = 0;
            int           iPrev   = -1;

            for (int iPid = Next(0); iPid >= 0; iPid = Next(iPid + 1))
            {
                if (iPid == (iPrev + 1) && iPrev >= 0)
                {
                    // range continuation
                    cRange++;
                }
                else
                {
                    if (cRange > 0)
                    {
                        // range completion
                        sb.Append((cRange > 1) ? ".." : ", ").Append(iPrev);
                        cRange = 0;
                    }

                    if (fAppend)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        fAppend = true;
                    }
                    sb.Append(iPid);
                }

                iPrev = iPid;
            }

            if (cRange > 0)
            {
                sb.Append((cRange > 1) ? ".." : ", ").Append(iPrev);
            }

            return sb.Append("}").ToString();
        }

        #endregion

        #region Constants

        /// <summary>
        /// Serialization format indicator.
        /// </summary>
        public enum Format
        {
            /// <summary>
            /// Indicates that no partitions are marked; 
            /// MarkedNone requires no additional data.
            /// </summary>
            MarkedNone = 0,

            /// <summary>
            /// Indicates that a small number of partitions are marked; 
            /// followed by stream of packed integers indicating gaps 
            /// between each marked partition, terminated with a -1.
            /// </summary>
            MarkedFew = 1,

            /// <summary>
            /// Indicates that a large number of partitions are marked; 
            /// followed by a sequence of 64-bit values sufficient to 
            /// represent the cardinality of the PartitionSet.
            /// </summary>
            MarkedMany = 2,

            /// <summary>
            /// Indicates that all partitions are marked; 
            /// MarkedAll requires no additional data.
            /// </summary>
            MarkedAll = 3,
        }
        #endregion

        #region Data members

        /// <summary>
        /// Total partition count.
        /// </summary>
        private int m_cPartitions;

        /// <summary>
        /// A bit array representing the partitions, stored as an array of longs.
        /// </summary> 
        private Int64[] m_alBits;

        /// <summary>
        /// A mask for the last long that indicates what bits get used.
        /// </summary> 
        private long m_lTailMask;

        /// <summary>
        /// A cached count of marked partitions; -1 indicates that the value must 
        /// be recalculated.
        /// </summary> 
        private int m_cMarked;

        #endregion
    }
}
