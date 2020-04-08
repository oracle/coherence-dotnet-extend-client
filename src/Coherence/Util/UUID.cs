/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Net;

using Tangosol.IO.Pof;
using Tangosol.Util;

namespace Tangosol.Util
{
    /// <summary>
    /// A UUID is a 256-bit identifier that, if it is generated, is
    /// statistically guaranteed to be unique.
    /// </summary>
    /// <author>Cameron Purdy  2004.06.24</author>
    /// <author>Goran Milosavljevic  2006.08.29</author>
    [Serializable]
    public sealed class UUID : IPortableObject
    {
        #region Properties

        /// <summary>
        /// This is true if the UUID was generated, and false if it was
        /// built.
        /// </summary>
        /// <remarks>
        /// A generated UUID is universally unique. Note that the port
        /// number is random if the UUID is generated.
        /// </remarks>
        /// <value>
        /// <b>true</b> if the UUID was generated.
        /// </value>
        public bool IsGenerated
        {
            get
            {
                EnsureConstructed();
                return (m_port & MASK_GENERATED) != 0;
            }
        }

        /// <summary>
        /// Determine the date/time value that the UUID instance was
        /// generated.
        /// </summary>
        /// <value>
        /// Date/time value in millis that the UUID instance was generated.
        /// </value>
        public long Timestamp
        {
            get
            {
                EnsureConstructed();
                return m_dateTime;
            }
            private set
            {
                // COH-8647 - Java expects epoch based timestamps.  Disallow earlier timestamps
                if (DateTimeUtils.IsBeforeTheEpoch(value))
                {
                    throw new ArgumentException("UUIDs with timestamps before the epoch (January 1, 1970) are not supported: "
                        + DateTimeUtils.GetDateTime(value));
                }

                m_dateTime = value;
            }
        }

        /// <summary>
        /// This is <b>true</b> if the IP address is a real IP address.
        /// </summary>
        /// <remarks>
        /// This is only <b>false</b> if two conditions are met: The UUID
        /// is generated, and it could not get an IP address (or one that is
        /// not a loopback/localhost address).
        /// </remarks>
        /// <value>
        /// <b>true</b> if the UUID has IP address information.
        /// </value>
        public bool IsAddressIncluded
        {
            get
            {
                EnsureConstructed();
                return (m_port & MASK_REALADDR) != 0;
            }
        }

        /// <summary>
        /// Determine the internet address of the host that generated the
        /// UUID instance.
        /// </summary>
        /// <value>
        /// An array of bytes containing the IP address information; the
        /// array can be zero bytes (no address,) four bytes (IPv4) or
        /// 16 bytes (IPv6).
        /// </value>
        public byte[] Address
        {
            get
            {
                EnsureConstructed();

                byte[] byteArray;

                switch (m_port & (MASK_REALADDR | MASK_IPV6ADDR))
                {

                    case MASK_REALADDR | MASK_IPV6ADDR:
                    {
                        int addr1 = m_addr1;
                        int addr2 = m_addr2;
                        int addr3 = m_addr3;
                        int addr4 = m_addr4;

                        byteArray     = new byte[16];
                        byteArray[0]  = (byte) NumberUtils.URShift(addr1, 24);
                        byteArray[1]  = (byte) (NumberUtils.URShift(addr1, 16));
                        byteArray[2]  = (byte) (NumberUtils.URShift(addr1, 8));
                        byteArray[3]  = (byte) (addr1);
                        byteArray[4]  = (byte) (NumberUtils.URShift(addr2, 24));
                        byteArray[5]  = (byte) (NumberUtils.URShift(addr2, 16));
                        byteArray[6]  = (byte) (NumberUtils.URShift(addr2, 8));
                        byteArray[7]  = (byte) (addr2);
                        byteArray[8]  = (byte) (NumberUtils.URShift(addr3, 24));
                        byteArray[9]  = (byte) (NumberUtils.URShift(addr3, 16));
                        byteArray[10] = (byte) (NumberUtils.URShift(addr3, 8));
                        byteArray[11] = (byte) (addr3);
                        byteArray[12] = (byte) (NumberUtils.URShift(addr4, 24));
                        byteArray[13] = (byte) (NumberUtils.URShift(addr4, 16));
                        byteArray[14] = (byte) (NumberUtils.URShift(addr4, 8));
                        byteArray[15] = (byte) (addr4);
                    }
                    break;

                    case MASK_REALADDR:
                    {
                        int addr1 = m_addr1;

                        byteArray    = new byte[4];
                        byteArray[0] = (byte) (NumberUtils.URShift(addr1, 24));
                        byteArray[1] = (byte) (NumberUtils.URShift(addr1, 16));
                        byteArray[2] = (byte) (NumberUtils.URShift(addr1, 8));
                        byteArray[3] = (byte) (addr1);
                    }
                    break;

                    case 0:
                        byteArray = NO_BYTES;
                        break;

                    default:
                        throw new SystemException();

                }

                return byteArray;
            }
        }

        /// <summary>
        /// Determine the port portion of the UUID. Note that the port is
        /// a 28-bit value; the first nibble is always 0x0.
        /// </summary>
        /// <value>
        /// The port portion of the UID.
        /// </value>
        public int Port
        {
            get
            {
                EnsureConstructed();
                return m_port & ~ MASK_ALLFLAGS;
            }
        }

        /// <summary>
        /// Determine the "counter" portion of the UUID that ensures that two
        /// UUIDs generated at the same exact time by the same process are
        /// unique.
        /// </summary>
        /// <returns>
        /// A number that helps to make the UUID unique.
        /// </returns>
        public int Count
        {
            get
            {
                EnsureConstructed();
                return m_count;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize host address information.
        /// </summary>
        static UUID()
        {
            bool isRealAddress = false;
            bool isIPv6        = false;
            int  addr1         = 0x00000000;
            int  addr2         = 0x00000000;
            int  addr3         = 0x00000000;
            int  addr4         = 0x00000000;

            try
            {
                IPAddress addr = NetworkUtils.GetLocalHostAddress();

                byte[] ipBytes = NO_BYTES;
                if (addr != null)
                {
                    ipBytes = addr.GetAddressBytes();
                }

                switch (ipBytes.Length)
                {
                    case 4:
                    {
                        int ip = (ipBytes[0] & 0xFF) << 24 | (ipBytes[1] & 0xFF) << 16 | (ipBytes[2] & 0xFF) << 8 | (ipBytes[3] & 0xFF);
                        if (ip != 0x00000000 && ip != 0x7F000001)
                        {
                            isRealAddress = true;
                            addr1         = ip;
                        }
                    }
                    break;


                    case 16:
                    {
                        int ip1 = (ipBytes[0] & 0xFF) << 24 | (ipBytes[1] & 0xFF) << 16 | (ipBytes[2] & 0xFF) << 8 | (ipBytes[3] & 0xFF);
                        int ip2 = (ipBytes[4] & 0xFF) << 24 | (ipBytes[5] & 0xFF) << 16 | (ipBytes[6] & 0xFF) << 8 | (ipBytes[7] & 0xFF);
                        int ip3 = (ipBytes[8] & 0xFF) << 24 | (ipBytes[9] & 0xFF) << 16 | (ipBytes[10] & 0xFF) << 8 | (ipBytes[11] & 0xFF);
                        int ip4 = (ipBytes[12] & 0xFF) << 24 | (ipBytes[13] & 0xFF) << 16 | (ipBytes[14] & 0xFF) << 8 | (ipBytes[15] & 0xFF);

                        if (!(ip1 == 0x00000000 && ip2 == 0x00000000 && ip3 == 0x00000000 && (ip4 == 0x00000000 || ip4 == 0x00000001)) && !((ipBytes[0] & 0xFF) == 0xFE && (ipBytes[1] & 0xC0) == 0x80))
                        // link-local
                        {
                            isRealAddress = true;
                            isIPv6        = true;
                            addr1         = ip1;
                            addr2         = ip2;
                            addr3         = ip3;
                            addr4         = ip4;
                        }
                    }
                    break;
                }
            }
            catch (Exception)
            {}

            s_isRealAddress = isRealAddress;
            s_isIPv6        = isIPv6;
            s_addr1         = addr1;
            s_addr2         = addr2;
            s_addr3         = addr3;
            s_addr4         = addr4;
        }

        /// <summary>
        /// Generate a UUID.
        /// </summary>
        public UUID()
        {}

        /// <summary>
        /// Build a UUID from its constituent members (advanced
        /// constructor).
        /// </summary>
        /// <remarks>
        /// It is guaranteed that a generated UUID will never equal a built
        /// UUID.
        /// </remarks>
        /// <param name="datetime">
        /// The creation date/time millis portion of the UUID.
        /// </param>
        /// <param name="address">
        /// The IPAddress portion of the UUID.
        /// </param>
        /// <param name="port">
        /// The port number portion of the UUID; a port number
        /// is 16 bits, but up to 28 bits of data from this value
        /// will be maintained by the UUID.
        /// </param>
        /// <param name="counter">
        /// The counter portion of the UUID.
        /// </param>
        public UUID(long datetime, IPAddress address, int port, int counter) :
                this(datetime, address == null ? null : address.GetAddressBytes(), port, counter)
        {}

        /// <summary>
        /// Build a UUID from its constituent members (advanced
        /// constructor).
        /// </summary>
        /// <remarks>
        /// <p/>
        /// It is guaranteed that a generated UUID will never equal a
        /// built UUID.
        /// </remarks>
        /// <param name="datetime">
        /// The creation date/time millis portion of the UUID.
        /// </param>
        /// <param name="ip">
        /// The IPAddress portion of the UUID.
        /// </param>
        /// <param name="port">
        /// The port number portion of the UUID; a port number
        /// is 16 bits, but up to 28 bits of data from this value
        /// will be maintained by the UUID.
        /// </param>
        /// <param name="counter">
        /// The counter portion of the UUID.
        /// </param>
        public UUID(long datetime, byte[] ip, int port, int counter)
        {
            Timestamp = datetime;
            m_count   = counter;

            bool isAddr = false;
            bool isIPv6 = false;
            if (ip != null)
            {
                switch (ip.Length)
                {
                    default:
                        throw new ArgumentException("unsupported IP address length: " + ip.Length);
                    case 16:
                        m_addr4 = (ip[12] & 0xFF) << 24 | (ip[13] & 0xFF) << 16 | (ip[14] & 0xFF) << 8 | (ip[15] & 0xFF);
                        m_addr3 = (ip[8] & 0xFF) << 24 | (ip[9] & 0xFF) << 16 | (ip[10] & 0xFF) << 8 | (ip[11] & 0xFF);
                        m_addr2 = (ip[4] & 0xFF) << 24 | (ip[5] & 0xFF) << 16 | (ip[6] & 0xFF) << 8 | (ip[7] & 0xFF);
                        isIPv6 = true;
                        // fall through
                        goto case 4;
                    case 4:
                        m_addr1 = (ip[0] & 0xFF) << 24 | (ip[1] & 0xFF) << 16 | (ip[2] & 0xFF) << 8 | (ip[3] & 0xFF);
                        isAddr = true;
                        break;
                    case 0:
                        break;
                }
            }

            m_port = (isAddr ? MASK_REALADDR : 0) | (isIPv6 ? MASK_IPV6ADDR : 0) | (port & ~ MASK_ALLFLAGS);

            InitHashcode();
        }

        /// <summary>
        /// Construct a UUID from a string.
        /// </summary>
        /// <param name="s">
        /// A string as would be returned from UUID.ToString()
        /// </param>
        public UUID(string s) : this(NumberUtils.ParseHex(s))
        {}

        /// <summary>
        /// Construct a UUID from a byte array.
        /// </summary>
        /// <param name="array">
        /// A byte array as would be returned from UUID.ToByteArray()
        /// </param>
        public UUID(byte[] array)
        {
            Debug.Assert(array != null && array.Length == 32);

            Timestamp = (long)(array[0] & 0xFF) << 56 | (long)(array[1] & 0xFF) << 48 | (long)(array[2] & 0xFF) << 40
                | (long) (array[3] & 0xFF) << 32 | (long) (array[4] & 0xFF) << 24 | (long) (array[5] & 0xFF) << 16
                | (long) (array[6] & 0xFF) << 8 | (array[7] & 0xFF);
            m_addr1   = (array[8] & 0xFF) << 24 | (array[9] & 0xFF) << 16 | (array[10] & 0xFF) << 8 | (array[11] & 0xFF);
            m_addr2   = (array[12] & 0xFF) << 24 | (array[13] & 0xFF) << 16 | (array[14] & 0xFF) << 8 | (array[15] & 0xFF);
            m_addr3   = (array[16] & 0xFF) << 24 | (array[17] & 0xFF) << 16 | (array[18] & 0xFF) << 8 | (array[19] & 0xFF);
            m_addr4   = (array[20] & 0xFF) << 24 | (array[21] & 0xFF) << 16 | (array[22] & 0xFF) << 8 | (array[23] & 0xFF);
            m_port    = (array[24] & 0xFF) << 24 | (array[25] & 0xFF) << 16 | (array[26] & 0xFF) << 8 | (array[27] & 0xFF);
            m_count   = (array[28] & 0xFF) << 24 | (array[29] & 0xFF) << 16 | (array[30] & 0xFF) << 8 | (array[31] & 0xFF);

            InitHashcode();
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Convert the UUID to a byte array of 32 bytes.
        /// </summary>
        /// <returns>
        /// The UUID data as a byte array of 32 bytes.
        /// </returns>
        public byte[] ToByteArray()
        {
            EnsureConstructed();

            byte[] byteArray = new byte[32];

            long datetime = m_dateTime;
            int  addr1    = m_addr1;
            int  addr2    = m_addr2;
            int  addr3    = m_addr3;
            int  addr4    = m_addr4;
            int  port     = m_port;
            int  count    = m_count;

            byteArray[0]  = (byte) (NumberUtils.URShift(datetime, 56));
            byteArray[1]  = (byte) (NumberUtils.URShift(datetime, 48));
            byteArray[2]  = (byte) (NumberUtils.URShift(datetime, 40));
            byteArray[3]  = (byte) (NumberUtils.URShift(datetime, 32));
            byteArray[4]  = (byte) (NumberUtils.URShift(datetime, 24));
            byteArray[5]  = (byte) (NumberUtils.URShift(datetime, 16));
            byteArray[6]  = (byte) (NumberUtils.URShift(datetime, 8));
            byteArray[7]  = (byte) (datetime);
            byteArray[8]  = (byte) (NumberUtils.URShift(addr1, 24));
            byteArray[9]  = (byte) (NumberUtils.URShift(addr1, 16));
            byteArray[10] = (byte) (NumberUtils.URShift(addr1, 8));
            byteArray[11] = (byte) (addr1);
            byteArray[12] = (byte) (NumberUtils.URShift(addr2, 24));
            byteArray[13] = (byte) (NumberUtils.URShift(addr2, 16));
            byteArray[14] = (byte) (NumberUtils.URShift(addr2, 8));
            byteArray[15] = (byte) (addr2);
            byteArray[16] = (byte) (NumberUtils.URShift(addr3, 24));
            byteArray[17] = (byte) (NumberUtils.URShift(addr3, 16));
            byteArray[18] = (byte) (NumberUtils.URShift(addr3, 8));
            byteArray[19] = (byte) (addr3);
            byteArray[20] = (byte) (NumberUtils.URShift(addr4, 24));
            byteArray[21] = (byte) (NumberUtils.URShift(addr4, 16));
            byteArray[22] = (byte) (NumberUtils.URShift(addr4, 8));
            byteArray[23] = (byte) (addr4);
            byteArray[24] = (byte) (NumberUtils.URShift(port, 24));
            byteArray[25] = (byte) (NumberUtils.URShift(port, 16));
            byteArray[26] = (byte) (NumberUtils.URShift(port, 8));
            byteArray[27] = (byte) (port);
            byteArray[28] = (byte) (NumberUtils.URShift(count, 24));
            byteArray[29] = (byte) (NumberUtils.URShift(count, 16));
            byteArray[30] = (byte) (NumberUtils.URShift(count, 8));
            byteArray[31] = (byte) (count);

            return byteArray;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Convert the UUID to a printable String.
        /// </summary>
        /// <returns>
        /// The UUID data as a 0x-prefixed hex string.
        /// </returns>
        public override string ToString()
        {
            return NumberUtils.ToHexEscape(ToByteArray());
        }

        /// <summary>
        /// Determine if two UUIDs are equal.
        /// </summary>
        /// <param name="o">
        /// The other UUID.
        /// </param>
        /// <returns>
        /// <b>true</b> if the passed object is equal to this.
        /// </returns>
        public override bool Equals(object o)
        {
            EnsureConstructed();

            if (o is UUID)
            {
                UUID that = (UUID) o;
                return this == that || m_hash == that.m_hash && m_dateTime == that.m_dateTime && m_addr1 == that.m_addr1 && m_addr2 == that.m_addr2 && m_addr3 == that.m_addr3 && m_addr4 == that.m_addr4 && m_port == that.m_port && m_count == that.m_count;
            }

            return false;
        }

        /// <summary>
        /// Compares this object with the specified object for order.
        /// </summary>
        /// <remarks>
        /// Returns a negative integer, zero, or a positive integer as
        /// this object is less than, equal to, or greater than the
        /// specified object.
        /// </remarks>
        /// <param name="o">
        /// The Object to be compared.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this object
        /// is less than, equal to, or greater than the specified object.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// If the specified object's type prevents it.
        /// </exception>
        /// <summary>
        /// From being compared to this Object.
        /// </summary>
        public int CompareTo(object o)
        {
            EnsureConstructed();

            int  result = 0;
            UUID that   = (UUID) o;

            if (this != that)
            {
                if (m_dateTime != that.m_dateTime)
                {
                    result = m_dateTime < that.m_dateTime ? -1 : 1;
                }
                else if (m_addr1 != that.m_addr1)
                {
                    uint addr1 = (uint) m_addr1;
                    uint addr2 = (uint) that.m_addr1;
                    result = addr1 < addr2 ? -1 : 1;
                }
                else if (m_addr2 != that.m_addr2)
                {
                    uint addr1 = (uint) m_addr2;
                    uint addr2 = (uint) that.m_addr2;
                    result = addr1 < addr2 ? -1 : 1;
                }
                else if (m_addr3 != that.m_addr3)
                {
                    uint addr1 = (uint) m_addr3;
                    uint addr2 = (uint) that.m_addr3;
                    result = addr1 < addr2 ? -1 : 1;
                }
                else if (m_addr4 != that.m_addr4)
                {
                    uint addr1 = (uint) m_addr4;
                    uint addr2 = (uint) that.m_addr4;
                    result = addr1 < addr2 ? -1 : 1;
                }
                else if (m_port != that.m_port)
                {
                    result = m_port < that.m_port ? -1 : 1;
                }
                else if (m_count != that.m_count)
                {
                    result = m_count < that.m_count ? -1 : 1;
                }
            }

            return result;
        }

        /// <summary>
        /// Determine a hash code for the UUID object.
        /// </summary>
        /// <returns>
        /// A hash code reflecting the UUID's data.
        /// </returns>
        public override int GetHashCode()
        {
            EnsureConstructed();
            return m_hash;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// If this UUID is being used as a generated UUID but its fields
        /// have not yet been initialized, this method ensures that the
        /// initialization occurs.
        /// </summary>
        /// <remarks>
        /// All public methods, except for deserialization methods, must
        /// call this method to ensure that the UUID is properly constructed.
        /// </remarks>
        private void  EnsureConstructed()
        {
            if (m_hash == 0)
            {
                // this UUID will be a "generated" UUID
                lock (this)
                {
                    if (m_hash == 0)
                    {
                        lock (LOCK)
                        {
                            m_dateTime = DateTimeUtils.GetSafeTimeMillis();
                            m_count = ++s_lastCount;
                        }

                        bool   isRealAddress = s_isRealAddress;
                        bool   isIPv6        = s_isIPv6;
                        Random rnd           = s_rnd;

                        // the "address" is either a 128-bit IPv6 address, a 32-bit IPv4
                        // address with the balance random, or a 128-bit random
                        if (isRealAddress)
                        {
                            if (isIPv6)
                            {
                                // 128-bit IPv6 address
                                m_addr1 = s_addr1;
                                m_addr2 = s_addr2;
                                m_addr3 = s_addr3;
                                m_addr4 = s_addr4;
                            }
                            else
                            {
                                // 32-bit IPv4 address; the rest is random
                                m_addr1 = s_addr1;
                                m_addr2 = rnd.Next();
                                m_addr3 = rnd.Next();
                                m_addr4 = rnd.Next();
                            }
                        }
                        else
                        {
                            // 128-bit random value instead of an address
                            m_addr1 = rnd.Next();
                            m_addr2 = rnd.Next();
                            m_addr3 = rnd.Next();
                            m_addr4 = rnd.Next();
                        }

                        // the "port" is mostly random data, except that the flags are
                        // encoded into it
                        int rndNext = rnd.Next();
                        m_port = MASK_GENERATED | (isRealAddress ? MASK_REALADDR : 0) | (isIPv6 ? MASK_IPV6ADDR : 0) | (rndNext & ~ MASK_ALLFLAGS);

                        InitHashcode();
                    }
                }
            }
        }

        /// <summary>
        /// Finish construction or deserialization.
        /// </summary>
        /// <remarks>
        /// The UUID's internally cached hashcode value is zero until
        /// construction is completed, or until deserialization is
        /// completed, and never zero otherwise. Every constructor,
        /// except for the deserialization constructor, must call this
        /// method.
        /// </remarks>
        private void InitHashcode()
        {
            int hash = (int) (NumberUtils.URShift(m_dateTime, 32))
                       ^ (int) m_dateTime
                       ^ m_addr1
                       ^ m_addr2
                       ^ m_addr3
                       ^ m_addr4
                       ^ m_port
                       ^ m_count;

            if (hash == 0)
            {
                hash = 2147483647; // Integer.MAX_VALUE is actually a prime ;-)
            }

            m_hash = hash;
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
        public void ReadExternal(IPofReader reader)
        {
            // note: this public method must not call ensureConstructed()
            if (m_hash != 0)
            {
                // an attempt was made to change a UUID -- which is an immutable
                // object -- by deserializing into it!
                throw new InvalidOperationException();
            }

            m_dateTime = DateTimeUtils.GetTimeMillisFromEpochBasedTime(reader.ReadInt64(0));
            m_addr1    = reader.ReadInt32(1);
            m_addr2    = reader.ReadInt32(2);
            m_addr3    = reader.ReadInt32(3);
            m_addr4    = reader.ReadInt32(4);
            m_port     = reader.ReadInt32(5);
            m_count    = reader.ReadInt32(6);

            InitHashcode();
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
        public void WriteExternal(IPofWriter writer)
        {
            EnsureConstructed();

            // COH-8647 - Java expects epoch based timestamps
            writer.WriteInt64(0, DateTimeUtils.GetTimeMillisSinceTheEpoch(m_dateTime));
            writer.WriteInt32(1, m_addr1);
            writer.WriteInt32(2, m_addr2);
            writer.WriteInt32(3, m_addr3);
            writer.WriteInt32(4, m_addr4);
            writer.WriteInt32(5, m_port);
            writer.WriteInt32(6, m_count);
        }

        #endregion

        #region Constants

        /// <summary>
        /// A bit mask that represents the portion of the "port" value
        /// reserved for bit flags.
        /// </summary>
        private const int MASK_ALLFLAGS = unchecked((int) 0xF0000000);

        /// <summary>
        /// The bit mask for the "is generated UUID" flag.
        /// </summary>
        private const int MASK_GENERATED = 1 << 31;

        /// <summary>
        /// The bit mask for the "is a real IP address" flag.
        /// </summary>
        private const int MASK_REALADDR = 1 << 30;

        /// <summary>
        /// The bit mask for the "is an IPv6 address" flag.
        /// </summary>
        private const int MASK_IPV6ADDR = 1 << 29;

        /// <summary>
        /// The one remaining bit for future use.
        /// </summary>
        private const int MASK_UNUSED = 1 << 28;

        /// <summary>
        /// An empty byte array (by definition immutable).
        /// </summary>
        private static readonly byte[] NO_BYTES = new byte[0];

        #endregion

        #region Shared members

        /// <summary>
        /// Random number generator.
        /// </summary>
        private static readonly Random s_rnd = NumberUtils.GetRandom();

        /// <summary>
        /// The spinning counter (which starts spinning at a random point).
        /// </summary>
        private static int s_lastCount = s_rnd.Next();

        /// <summary>
        /// This is true if the host's IP address is a real IP address.
        /// </summary>
        private static bool s_isRealAddress;

        /// <summary>
        /// This is true if the host's address style is known and is IPv6.
        /// </summary>
        private static bool s_isIPv6;

        /// <summary>
        /// The first four bytes of the IP address of the host.
        /// </summary>
        private static int s_addr1;

        /// <summary>
        /// The second four bytes of the IP address of the host.
        /// </summary>
        private static int s_addr2;

        /// <summary>
        /// The third four bytes of the IP address of the host.
        /// </summary>
        private static int s_addr3;

        /// <summary>
        /// The fourth four bytes of the IP address of the host.
        /// </summary>
        private static int s_addr4;

        /// <summary>
        /// A mutex for accessing/updating s_lastCount.
        /// </summary>
        private static readonly object LOCK = typeof(object);

        #endregion

        #region Data members

        /// <summary>
        /// System date/time value that the UUID instance was generated.
        /// </summary>
        private long m_dateTime;

        /// <summary>
        /// Internet address of host that generated the UUID instance.
        /// </summary>
        private int m_addr1;

        /// <summary>
        /// The second four bytes of the IP address.
        /// </summary>
        private int m_addr2;

        /// <summary>
        /// The third four bytes of the IP address.
        /// </summary>
        private int m_addr3;

        /// <summary>
        /// The fourth four bytes of the IP address.
        /// </summary>
        private int m_addr4;

        /// <summary>
        /// The port number if the UUID is built (not generated), otherwise this
        /// is a random number, with the exception of the most significant
        /// nibble which contains the flags of the UUID.
        /// </summary>
        private int m_port;

        /// <summary>
        /// A rolling counter.
        /// </summary>
        private int m_count;

        /// <summary>
        /// Cache the hash.
        /// </summary>
        [NonSerialized]
        private volatile int m_hash;

        #endregion
    }
}