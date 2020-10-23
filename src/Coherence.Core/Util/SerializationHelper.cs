/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Threading;

using Tangosol.IO;

namespace Tangosol.Util
{
    /// <summary>
    /// Miscellaneous serialization utilities.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.06.22</author>
    /// <since>Coherence 3.5</since>
    public class SerializationHelper
    {
        #region Binary serialization methods

        /// <summary>
        /// Serialize an object into its <see cref="Binary"/> form.
        /// </summary>
        /// <param name="o">Object to serialize.</param>
        /// <param name="serializer"><see cref="ISerializer"/> to use.</param>
        /// <returns>
        /// Serialized <see cref="Binary"/> representation of the specified object.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public static Binary ToBinary(Object o, ISerializer serializer)
        {
            Stats stats = FindStats(o);

            var stream = stats == null
                    ? new BinaryMemoryStream()
                    : stats.InstantiateBuffer();
            var writer = new DataWriter(stream);

            writer.Write((byte) SerializationFormat.FMT_EXT);
            serializer.Serialize(writer, o);

            UpdateStats(o, stats, (int) stream.Length);

            return stream.ToBinary();
        }

        /// <summary>
        /// Deserialize an object from its <see cref="Binary"/> form.
        /// </summary>
        /// <param name="bin"><see cref="Binary"/> representation of an object.</param>
        /// <param name="serializer"><see cref="ISerializer"/> to use.</param>
        /// <returns>Deserialized object.</returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public static Object FromBinary(Binary bin, ISerializer serializer)
        {
            Stream stream = bin.GetStream();
            var    reader = new DataReader(stream);
            var    type   = (SerializationFormat) Enum.ToObject(
                    typeof(SerializationFormat), reader.ReadByte());
       
            switch (type)
            {
                case SerializationFormat.FMT_IDO:
                {
                    // read is not symmetrical to the write;
                    // it returns the decorated value itself!
                    reader.ReadPackedInt32();
                    type = (SerializationFormat) Enum.ToObject(
                            typeof (SerializationFormat), reader.ReadByte());
                }
                break;

                case SerializationFormat.FMT_BIN_DECO:
                case SerializationFormat.FMT_BIN_EXT_DECO:
                {
                    long mask = type == SerializationFormat.FMT_BIN_DECO
                            ? reader.ReadByte()
                            : reader.ReadPackedInt64();

                    if ((mask & (1L << DECO_VALUE)) == 0L)
                    {
                        throw new IOException("Decorated value is missing a value");
                    }
                        
                    // read the length of the DECO_VALUE
                    reader.ReadPackedInt32();

                    type = (SerializationFormat) Enum.ToObject(
                            typeof(SerializationFormat), reader.ReadByte());
                }
                break;
            }

            if (type == SerializationFormat.FMT_EXT)
            {
                return serializer.Deserialize(reader);
            }
            throw new IOException("Illegal Binary format: " + type);
        }

        /// <summary>
        /// Convert binary UTF-8 encode data to a String. This method is a helper
        /// to allow various I/O implementations to share a single, efficient
        /// implementation.
        /// </summary>
        /// <param name="ab">
        /// an array of bytes containing UTF-8 encoded characters
        /// </param>
        /// <param name="of">
        /// the offset into the array of the UTF-8 data to decode
        /// </param>
        /// <param name="cb">
        /// the binary length in the array of the UTF-8 data to decode
        /// </param>
        /// <return>
        /// a String value
        /// </return>
        /// <exception cref="IOException">    
        /// throws ArgumentException  if the UTF data is corrupt
        /// </exception>
        public static String ConvertUTF(byte[] ab, int of, int cb)
        {
            // first run through the bytes determining if we have to
            // translate them at all (they might all be in the range 0-127)
            bool fAscii = true;
            int ofch = 0;
            int ofAsc = of;
            int ofEnd = of + cb;
            char[] ach = new char[cb];
            for (; ofAsc < ofEnd; ++ofAsc)
            {
                int n = ab[ofAsc] & 0xFF;
                if (n >= 0x80)
                {
                    // it's not all "ascii" data
                    fAscii = false;
                    break;
                }
                else
                {
                    ach[ofch++] = (char)n;
                }
            }

            if (!fAscii)
            {
                for (; ofAsc < ofEnd; ++ofAsc)
                {
                    int ch = ab[ofAsc] & 0xFF;
                    switch ((ch & 0xF0) >> 4)
                    {
                        case 0x0:
                        case 0x1:
                        case 0x2:
                        case 0x3:
                        case 0x4:
                        case 0x5:
                        case 0x6:
                        case 0x7:
                            // 1-byte format:  0xxx xxxx
                            ach[ofch++] = (char)ch;
                            break;

                        case 0xC:
                        case 0xD:
                            {
                                // 2-byte format:  110x xxxx, 10xx xxxx
                                int ch2 = ab[++ofAsc] & 0xFF;
                                if ((ch2 & 0xC0) != 0x80)
                                {
                                    throw new IOException(
                                        "illegal leading UTF byte: " + ch2);
                                }
                                ach[ofch++] = (char)(((ch & 0x1F) << 6) | ch2 & 0x3F);
                                break;
                            }

                        case 0xE:
                            {
                                // 3-byte format:  1110 xxxx, 10xx xxxx, 10xx xxxx
                                int ch2 = ab[++ofAsc] & 0xFF;
                                int ch3 = ab[++ofAsc] & 0xFF;
                                if ((ch2 & 0xC0) != 0x80 || (ch3 & 0xC0) != 0x80)
                                {
                                    throw new IOException(
                                        "illegal leading UTF bytes: " + ch2 + ", " + ch3);
                                }
                                ach[ofch++] = (char)(((ch & 0x0F) << 12) |
                                                      ((ch2 & 0x3F) << 6) |
                                                      ((ch3 & 0x3F)));
                                break;
                            }

                        default:
                            throw new IOException(
                                "illegal leading UTF byte: " + ch);
                    }
                }
            }

            return new String(ach, 0, ofch);
        }

        #endregion

        #region Integer decoration methods

        /// <summary>
        /// Decorate the specified <see cref="Binary"/> with the specified integer decoration.
        /// </summary>
        /// <param name="binValue">The <see cref="Binary"/> to be decorated.</param>
        /// <param name="decoration">The integer decoration.</param>
        /// <returns>The decorated (with integer decoration) <see cref="Binary"/> object.</returns>
        /// <since>Coherence 3.7.1</since>
        public static Binary DecorateBinary(Binary binValue, Int32 decoration)
        {
            var stream = new BinaryMemoryStream(6 + binValue.Length);
            var writer = new DataWriter(stream);

            writer.Write((byte) SerializationFormat.FMT_IDO);
            writer.WritePackedInt32(decoration);
            binValue.WriteTo(writer);

            return stream.ToBinary();
        }

        /// <summary>
        /// Extract a decoration value from the specified <see cref="Binary"/> that contains an
        /// integer decoration.
        /// </summary>
        /// <param name="bin">The <see cref="Binary"/> object.</param>
        /// <returns>The integer decoration value.</returns>
        /// <exception cref="ArgumentException">
        /// If the <see cref="Binary"/> does not have an int decoration.
        /// </exception>
        /// <since>Coherence 3.7.1</since>
        public static Int32 ExtractIntDecoration(Binary bin)
        {
            try
            {
                DataReader reader = bin.GetReader();
                reader.ReadByte(); // skip the type
                return reader.ReadPackedInt32();
            }
            catch (IOException)
            {
                throw new ArgumentException("invalid binary");
            }
        }

        /// <summary>
        /// Check whether or not the specified <see cref="Binary"/> has an integer
        /// decoration.
        /// </summary>
        /// <param name="bin">The <see cref="Binary"/> object.</param>
        /// <returns>True if the <see cref="Binary"/> contains (starts with) an integer
        /// decoration; false otherwise.</returns>
        /// <since>Coherence 3.7.1</since>
        public static bool IsIntDecorated(Binary bin)
        {
            try
            {
                return bin.ByteAt(0) == (byte) SerializationFormat.FMT_IDO;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        /// <summary>
        /// Remove the integer decoration from the specified <see cref="Binary"/>.
        /// </summary>
        /// <param name="bin">The <see cref="Binary"/> object.</param>
        /// <returns>The undecorated <see cref="Binary"/> value.</returns>
        /// <since>Coherence 3.7.1</since>
        public static Binary RemoveIntDecoration(Binary bin)
        {
            try
            {
                DataReader reader = bin.GetReader();
                Stream     stream = reader.BaseStream;
                long       pos    = stream.Position;

                reader.ReadByte();        // skip the type
                reader.ReadPackedInt32(); // skip the int decoration

                int of = (int) (stream.Position - pos);
                return bin.GetBinary(of, bin.Length - of);
            }
            catch (IOException)
            {
                throw new ArgumentException("invalid binary");
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// Decoration: The original value (before being decorated).
        /// </summary>
        public static readonly int DECO_VALUE = 0;

        #endregion

        #region Enum: SerializationFormat

        /// <summary>
        /// Serialization format.
        /// </summary>
        internal enum SerializationFormat : byte
        {
            /// <summary>
            /// Serialization format: Integer-decorated value.
            /// </summary>
            FMT_IDO = 13,

            /// <summary>
            /// Serialization format: Decorated Binary value.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Structure is:
            /// byte 0    : format identifier (18)
            /// byte 1    : bit mask of decoration identifiers
            /// byte 2    : packed int specifying the length of the first
            /// decorator
            /// byte next : binary data
            /// ...</p>
            /// <p>
            /// For each decorator, there is a packed int for its length,
            /// followed by its binary data. The first decorator is the
            /// decorated value itself, if present.
            /// </p>
            /// <p>
            /// Note: FMT_IDO cannot be combined with FMT_BIN_DECO.
            /// </p>
            /// </remarks>
            FMT_BIN_DECO = 18,

            /// <summary>
            /// Serialization format: Extended Decorated Binary value.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Structure is:
            /// byte 0    : format identifier (19)
            /// byte 1... : a packed long as bit mask 
            /// byte next : packed int specifying the length of the first
            /// decorator
            /// byte next : binary data
            /// ...</p>
            /// <p>
            /// For each decorator, there is a packed int for its length,
            /// followed by its binary data. The first decorator is the
            /// decorated value itself, if present.
            /// </p>
            /// <p>
            /// Note: FMT_IDO cannot be combined with FMT_BIN_EXT_DECO.
            /// </p>
            /// </remarks>
            FMT_BIN_EXT_DECO = 19,

            /// <summary>
            /// Serialization format: A default serializer is NOT used.
            /// </summary>
            FMT_EXT = 21
        }

        #endregion

        #region Stats related methods

        /// <summary>
        /// If statistics are being maintained for the class of the specified
        /// Object value, then find and return those stats.
        /// </summary>
        /// <param name="o">
        /// The value to search for a Stats object for.
        /// </param>
        /// <returns>
        /// The Stats object for the specified Object value, or null.
        /// </returns>
        /// <since>Coherence 3.7.1</since>
        private static Stats FindStats(Object o)
        {
            return s_astats[CalculateStatsId(o)];
        }

        /// <summary>
        /// If statistics are being maintained for the class of the specified
        /// Object value, then find and return those stats.
        /// </summary>
        /// <param name="o"> 
        /// The object that has been written.
        /// </param>
        /// <param name="stats">
        /// The statistics that track the serialized sizes of objects.
        /// </param>
        /// <param name="cb">
        /// The size in bytes of the object as it was written.
        /// </param>
        /// <since>Coherence 3.7.1</since>
        private static void UpdateStats(Object o, Stats stats, int cb)
        {
            if (stats == null)
            {
                s_astats[CalculateStatsId(o)] = stats = new Stats();
            }
            stats.Update(cb);
        }

        /// <summary>
        /// Calculate a somewhat unique ID for the type of the passed Object.
        /// </summary>
        /// <param name="o">
        /// A user type value.
        /// </param>
        /// <returns>
        /// An ID that is hopefully unique across the set of user type
        /// classes in use within this VM at this general point in time.
        /// </returns>
        /// <since>Coherence 3.7.1</since>
        private static int CalculateStatsId(Object o)
        {
            if (o == null)
            {
                return 0;
            }
            int n = o.GetType().GetHashCode();
            return (int) (((((uint) n) >> 1) + (n & 0x01)) % s_astats.Length);
        }

        #endregion

        #region Inner class: Stats

        /// <summary>
        /// Serialization statistics for a given user type.
        /// </summary>
        /// <since>Coherence 3.7.1</since>
        protected class Stats
        {
            #region Stats method 

            /// <summary>
            /// Update the serialization statistics with the size (in bytes) of a
            /// newly serialized object.
            /// </summary>
            /// <param name="cb">  
            /// The number of bytes used to serialize.
            /// </param>
            public void Update(int cb)
            {
                long lStats  = Interlocked.Read(ref m_lStats);
                long lAccum  = Interlocked.Read(ref m_lAccum);
                int  cbMax   = (int) (((ulong) lStats) >> 32);
                int  cItems  = (int) (((ulong) lAccum) >> 48);
                long cbTotal = lAccum & 0xFFFFFFFFFFFFL;

                if (cItems > 0)
                {
                    bool fResetStats = false;
                    int  cbOldAvg    = (int) (cbTotal / cItems);
                    long ldtNow      = 0;

                    if (Math.Abs(cbOldAvg - cb) > (cb / 2))
                    {
                        // reset the stats because the cb differs by
                        // more than 50% from the current average
                        ldtNow = DateTimeUtils.GetSafeTimeMillis();
                        fResetStats = true;
                    }
                    else if ((cItems & 0x3FF) == 0)
                    {
                        long ldtCreated = Interlocked.Read(ref m_ldtCreated);
                        ldtNow = DateTimeUtils.GetSafeTimeMillis();
                        if (ldtNow > ldtCreated + EXPIRY_MILLIS || // stats expiry
                                (cItems & 0xFFFF) == 0)            // cItems overflow
                        {
                        // reset the stats periodically
                        fResetStats = true;
                        }
                    }
                    if (fResetStats)
                    {
                        cbMax = 0;
                        lAccum = 0L;
                        cItems = 0;
                        Interlocked.Exchange(ref m_ldtCreated, ldtNow);
                    }
                }

                // accumulate the total bytes (uses lowest 48 out of 64 bits)
                cbTotal = (lAccum + cb) & 0xFFFFFFFFFFFFL;

                // recalculate the average
                int cbAvg = (int) (cbTotal / ++cItems);

                // check for a new max size
                if (cb > cbMax)
                {
                    cbMax = cb;
                }

                // the item count and total bytes are stored in a "volatile long"
                // so that they are accessed (and modified) atomically
                Interlocked.Exchange(ref m_lAccum, (((long) cItems) << 48) | cbTotal);

                // the average and max are stored in a "volatile long" so that
                // they are subsequently accessed atomically
                Interlocked.Exchange(ref m_lStats, (((long) cbMax) << 32) | cbAvg);
            }

            /// <summary>
            /// Instantiate a WriteBuffer to write a user type for which this
            /// Stats object maintains serialization statistics.
            /// </summary>
            /// <returns>
            /// A WriteBuffer to write to.
            /// </returns>
            public BinaryMemoryStream InstantiateBuffer()
            {
                long lStats = Interlocked.Read(ref m_lStats);
                int  cbMax  = (int) (((ulong) lStats) >> 32);

                return new BinaryMemoryStream((cbMax + 0xF) & ~0xF);
            }

            #endregion

            #region Constants
            
            /// <summary>
            /// The expiry for statistics (in milliseconds).
            /// </summary>
            private static readonly int EXPIRY_MILLIS = 10 * 60 * 1000; // 10 minutes

            #endregion

            #region Data members
            
            /// <summary>
            /// <ul>
            /// <li>high 2 bytes - Number of items that have been submitted for
            /// statistics keeping.</li>
            /// <li>low 6 bytes - Total number of bytes of all the items
            /// submitted.</li>
            /// </ul>
            /// </summary>
            private long m_lAccum;

            /// <summary>
            /// <ul>
            /// <li>highWord - Largest size in bytes of all the items
            /// submitted.</li>
            /// <li>lowWord  - The average size in bytes of all the items
            /// submitted.</li>
            /// </ul>
            /// </summary>
            private long m_lStats;

            /// <summary>
            /// Time at which this Stats object was created.
            /// </summary>
            private long m_ldtCreated;

            #endregion
        }
    
        #endregion

        #region data member

        /// <summary>
        /// An array of Stats objects, indexed by the modulo of a swizzled class
        /// hashcode.
        /// </summary>
        private static Stats[] s_astats = new Stats[6451];

        #endregion
    }
}
