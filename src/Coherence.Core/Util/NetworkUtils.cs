/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using Tangosol.Net;

namespace Tangosol.Util
{
    /// <summary>
    /// Miscellaneuos utility methods for network related objects.
    /// </summary>
    /// <author>Ana Cikic  2006.08.29</author>
    public static class NetworkUtils
    {
        /// <summary>
        /// -1 is infinite for timeout.
        /// </summary>
        public static readonly long NO_TIMEOUT = -1;

        /// <summary>
        /// Static initializer, specify the default address family.
        /// </summary>
        static NetworkUtils()
        {
            PreferredAddressFamily = AddressFamily.InterNetwork;
        }
        
        /// <summary>
        /// Gets or sets the preferred AddressFamily. 
        /// </summary>
        public static AddressFamily PreferredAddressFamily { get; set;}

        /// <summary>
        /// Returns the local host IP address.
        /// </summary>
        /// <returns>
        /// The local host IP address.
        /// </returns>
        public static IPAddress GetLocalHostAddress()
        {
            return GetLocalHostAddress(NO_TIMEOUT);
        }

        /// <summary>
        /// Returns the local host IP address.
        /// </summary>
        /// <param name="cMillis">The maximum amount of time to wait in getting the address</param>
        /// <returns>
        /// The local host IP address.
        /// </returns>
        public static IPAddress GetLocalHostAddress(long cMillis)
        {
            foreach (IPAddress localAddress in GetAllAddresses(Dns.GetHostName(), cMillis))
            {
                if (VerifyIpAddress(localAddress))
                {
                    return localAddress;
                }
            }

            IPAddress bestAddress = null;

            foreach (IPAddress ipAddress in GetAllLocalAddresses())
            {
                if (VerifyIpAddress(ipAddress))
                {
                    if (IsSiteLocalAddress(ipAddress))
                    {
                        if (bestAddress == null)
                        {
                            bestAddress = ipAddress;
                        }
                    }
                    else
                    {
                        return ipAddress;
                    }
                }
            }
            return bestAddress;
        }

        /// <summary>
        /// Check if the specified IPAddress is usable.
        /// </summary>
        /// <param name="ipAddress">The IPAddress to check</param>
        /// <returns>True if the IPAddress is usable</returns>
        private static bool VerifyIpAddress(IPAddress ipAddress)
        {
            return !IPAddress.IsLoopback(ipAddress) 
                    && !IsAnyLocalAddress(ipAddress) 
                    && !IsLinkLocalAddress(ipAddress) 
                    && ipAddress.AddressFamily == PreferredAddressFamily;
        }

        /// <summary>
        /// Returns first available IP address for the specified
        /// host name.
        /// </summary>
        /// <param name="hostName">
        /// The name of the host.
        /// </param>
        /// <returns>
        /// A first available IP address for the specified
        /// host name.
        /// </returns>
        public static IPAddress GetHostAddress(string hostName)
        {
            return GetHostAddress(hostName, NO_TIMEOUT);
        }

        /// <summary>
        /// Returns first available IP address for the specified
        /// host name.
        /// </summary>
        /// <param name="hostName">
        /// The name of the host.
        /// </param>
        /// <param name="cMillis">
        /// The maximum amount of time to wait in getting the address
        /// </param>
        /// <returns>
        /// A first available IP address for the specified
        /// host name.
        /// </returns>
        public static IPAddress GetHostAddress(string hostName, long cMillis)
        {
            return GetAllAddresses(hostName, cMillis)[0];
        }

        /// <summary>
        /// Returns all available IP addresses for the specified
        /// host name.
        /// </summary>
        /// <param name="hostName">
        /// The name of the host.
        /// </param>
        /// <returns>
        /// An array of type IPAddress.
        /// </returns>
        public static IPAddress[] GetAllAddresses(string hostName)
        {
            return GetAllAddresses(hostName, NO_TIMEOUT);
        }

        /// <summary>
        /// Returns all available IP addresses for the specified
        /// host name.
        /// </summary>
        /// <param name="hostName">
        /// The name of the host.
        /// </param>
        /// <param name="cMillis">
        /// The maximum amount of time to wait in getting the address
        /// </param>
        /// <returns>
        /// An array of type IPAddress.
        /// </returns>
        public static IPAddress[] GetAllAddresses(string hostName, long cMillis)
        {
            Task<IPAddress[]> task = Task<IPAddress[]>.Factory.StartNew(() =>
            {
                var result = Dns.GetHostAddresses(hostName);
                return result;
            });

            bool success = task.Wait(Convert.ToInt32(cMillis));
            if (success)
            {
                return task.Result;
            }

            throw new RequestTimeoutException("Dns.GetHostAddresses() timed out while trying to resolve hostname " + hostName);
        }

        /// <summary>
        /// Determines if <b>IPAddress</b> is loopback address.
        /// </summary>
        /// <param name="address">
        /// An <b>IPAddress.</b>
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="address"/> is the loopback
        /// address; otherwise, <b>false</b>.
        /// </returns>
        public static bool IsLoopbackAddress(IPAddress address)
        {
            return IPAddress.IsLoopback(address);
        }

        /// <summary>
        /// Determines if <b>IPAddress</b> is "Any" address.
        /// </summary>
        /// <remarks>
        /// The "Any" field is equivalent to 0.0.0.0 in dotted-quad notation.
        /// </remarks>
        /// <param name="address">
        /// An <b>IPAddress.</b>
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="address"/> is equal to "Any"
        /// address; otherwise, <b>false</b>.
        /// </returns>
        public static bool IsAnyLocalAddress(IPAddress address)
        {
            return address.Equals(IPAddress.Any);
        }

        /// <summary>
        /// Determines if <b>IPAddress</b> is link local address.
        /// </summary>
        /// <param name="address">
        /// An <b>IPAddress.</b>
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="address"/> is link local address;
        /// otherwise, <b>false</b>.
        /// </returns>
        //TODO: resolve IsLinkLocalAddress
        public static bool IsLinkLocalAddress(IPAddress address)
        {
            return false;
        }

        /// <summary>
        /// Determines if <b>IPAddress</b> is site local address.
        /// </summary>
        /// <param name="address">
        /// An <b>IPAddress.</b>
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="address"/> is site local address;
        /// otherwise, <b>false</b>.
        /// </returns>
        //TODO: resolve IsSiteLocalAddress
        public static bool IsSiteLocalAddress(IPAddress address)
        {
            return false;
        }

        /// <summary>
        /// Returns the list of all local addresses ofor all network interfaces 
        /// on the local computer.
        /// </summary>
        /// <returns>
        /// The list of all addresses for all network interfaces on the local computer.
        /// </returns>
        public static IEnumerable<IPAddress> GetAllLocalAddresses()
        {
            foreach (NetworkInterface nt in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation ipAddress 
                    in nt.GetIPProperties().UnicastAddresses)
                {
                    yield return ipAddress.Address;
                }
            }
        }

        /// <summary>
        /// Utility method that prints out an <b>IPEndPoint</b> in a user-
        /// friendly manner.
        /// </summary>
        /// <param name="addr">
        /// <b>IPEndPoint</b> to print out.
        /// </param>
        /// <returns>
        /// The value in format ADDRESS:PORT.
        /// </returns>
        public static string ToString(IPEndPoint addr)
        {
            if (addr == null)
            {
                return "null";
            }
            else
            {
                return (addr.Address + ":" + addr.Port);
            }
        }

        /// <summary>
        /// Utility method that prints out an <b>IPEndPoint</b> with 
        /// optional subport in a user-friendly manner.
        /// </summary>
        /// <param name="addr">
        /// <b>IPEndPoint</b> to print out.
        /// </param>
        /// <param name="subport">
        /// Subport, -1 if there is none.
        /// </param>
        /// <returns>
        /// The value in format ADDRESS:BASEPORT.SUBPORT.
        /// </returns>
        /// <since>Coherence 12.1.2</since>
        public static string ToString(IPEndPoint addr, Int32 subport)
        {
            if (subport == -1)
            {
                return ToString(addr);
            }

            return ToString(addr) + "." + subport;
        }

        /// <summary>
        /// Write out the 8 byte multiplexed protocol header which consists 
        /// of two 4-byte words: first is the protocol ID 0x05AC1E000,
        /// second is the subport number.
        /// </summary>
        /// <param name="stream">
        /// The output stream to write to.
        /// </param>
        /// <param name="subport">
        /// The subport number.
        /// </param>
        /// <since>Coherence 12.1.2</since>
        public static void WriteSubport(Stream stream, Int32 subport)
        {
            const Int32 protocolId = 0x05AC1E000;

            byte[] intBytes1 = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(protocolId));
            byte[] intBytes2 = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(subport));
            byte[] result    = new byte[8];
            intBytes1.CopyTo(result, 0);
            intBytes2.CopyTo(result, 4);
            stream.Write(result, 0, result.Length);
        }

        #region Socket related methods

        /// <summary>
        /// Configure the linger time of the given <b>Socket</b>.
        /// </summary>
        /// <param name="client">
        /// Target <b>Socket</b> object.
        /// </param>
        /// <param name="secs">
        /// The linger time in seconds (-1 to disable linger).
        /// </param>
        public static void SetLingerTime(TcpClient client, int secs)
        {
            secs = Math.Max(secs, -1);

            bool         isEnabled = secs >= 0 ? true : false;
            LingerOption linger    = (LingerOption) client.Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);

            if (linger.Enabled != isEnabled || linger.LingerTime != secs)
            {
                if (isEnabled)
                {
                    LingerOption lingerOption = new LingerOption(true, secs);
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                }
                else
                {
                    LingerOption lingerOption = new LingerOption(false, 0);
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                }

                linger = (LingerOption) client.Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);
                if (isEnabled)
                {
                    if (linger.LingerTime != secs)
                    {
                        CacheFactory.Log("Failed to set Socket linger time to " + secs
                                            + " seconds; actual value is " + linger.LingerTime
                                            + " seconds.",
                                            CacheFactory.LogLevel.Warn);
                    }
                }
                else
                {
                    if (linger.Enabled)
                    {
                        CacheFactory.Log("Failed to disable Socket linger time",
                                         CacheFactory.LogLevel.Warn);
                    }
                }
            }
        }

        /// <summary>
        /// Configure the <b>KeepAlive</b> option of the given <b>Socket</b> object.
        /// </summary>
        /// <param name="client">
        /// Target <b>Socket</b> object.
        /// </param>
        /// <param name="keepAlive">
        /// Boolean parameter that represents the new <b>KeepAlive</b> value.
        /// </param>
        public static void SetKeepAlive(TcpClient client, bool keepAlive)
        {
            if (SetSocketOption(client, SocketOptionLevel.Socket, SocketOptionName.KeepAlive, keepAlive) != keepAlive)
            {
                if (keepAlive)
                {
                    CacheFactory.Log("Failed to enable Socket keep alive",
                            CacheFactory.LogLevel.Warn);
                }
                else
                {
                    CacheFactory.Log("Failed to disable Socket keep alive",
                            CacheFactory.LogLevel.Warn);
                }
            }
        }

        /// <summary>
        /// Configure the <b>ReuseAddress</b> option of the given <b>Socket</b> object.
        /// </summary>
        /// <param name="client">
        /// Target <b>Socket</b> object.
        /// </param>
        /// <param name="reuseAddress">
        /// Boolean parameter that represents the new <b>ReuseAddress</b> value.
        /// </param>
        public static void SetReuseAddress(TcpClient client, bool reuseAddress)
        {
            if (SetSocketOption(client, SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, reuseAddress) != reuseAddress)
            {
                if (reuseAddress)
                {
                    CacheFactory.Log("Failed to enable Socket reuse",
                            CacheFactory.LogLevel.Warn);
                }
                else
                {
                    CacheFactory.Log("Failed to disable Socket reuse",
                            CacheFactory.LogLevel.Warn);
                }
            }
        }

        /// <summary>
        /// Configure the <b>ReceiveBuffer</b> option of the given <b>Socket</b> object.
        /// </summary>
        /// <param name="client">
        /// Target <b>Socket</b> object.
        /// </param>
        /// <param name="bufferSize">
        /// Integer parameter that represents the new <b>ReceiveBuffer</b> value.
        /// </param>
        public static void SetReceiveBufferSize(TcpClient client, int bufferSize)
        {
            ValidateBufferSize(true, SetSocketOption(client, SocketOptionLevel.Socket,
                    SocketOptionName.ReceiveBuffer, bufferSize), bufferSize);
        }

        /// <summary>
        /// Configure the <b>SendBuffer</b> option of the given <b>Socket</b> object.
        /// </summary>
        /// <param name="client">
        /// Target <b>Socket</b> object.
        /// </param>
        /// <param name="bufferSize">
        /// Integer parameter that represents the new <b>SendBuffer</b> value.
        /// </param>
        public static void SetSendBufferSize(TcpClient client, int bufferSize)
        {
            ValidateBufferSize(false, SetSocketOption(client, SocketOptionLevel.Socket,
                    SocketOptionName.SendBuffer, bufferSize), bufferSize);
        }

        /// <summary>
        /// Configure the <b>NoDelay</b> option of the given <b>Socket</b> object.
        /// </summary>
        /// <param name="client">
        /// Target <b>Socket</b> object.
        /// </param>
        /// <param name="noDelay">
        /// Boolean parameter that represents the new <b>NoDelay</b> value.
        /// </param>
        public static void SetTcpNoDelay(TcpClient client, bool noDelay)
        {
            if (SetSocketOption(client, SocketOptionLevel.Tcp, SocketOptionName.NoDelay, noDelay) != noDelay)
            {
                if (noDelay)
                {
                    CacheFactory.Log("Failed to disable Socket TCP delay", CacheFactory.LogLevel.Warn);
                }
                else
                {
                    CacheFactory.Log("Failed to enable Socket TCP delay", CacheFactory.LogLevel.Warn);
                }
            }
        }

        /// <summary>
        /// Closes the given <b>TcpClient</b> object.
        /// </summary>
        /// <param name="client">
        /// The <b>Socket</b> object to close.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the operation was successful; <b>false</b>
        /// otherwise.
        /// </returns>
        public static bool Close(TcpClient client)
        {
            if (client != null)
            {
                try
                {
                    client.Close();
                    return true;
                }
                catch
                { }
            }
            return false;
        }

        /// <summary>
        /// Configure the value of the specified boolean <b>Socket</b> option.
        /// </summary>
        /// <param name="client">
        /// Target <b>TcpClient</b> object.
        /// </param>
        /// <param name="level">
        /// The <b>SocketOptionLevel</b> of the target Socket option.
        /// </param>
        /// <param name="name">
        /// The <b>SocketOptionName</b> of the target Socket option.
        /// </param>
        /// <param name="value">
        /// The desired value of the target <b>TcpClient</b> option.
        /// </param>
        /// <returns>
        /// Returns the new value of the target <b>TcpClient</b> option.
        /// </returns>
        internal static bool SetSocketOption(TcpClient client, SocketOptionLevel level, SocketOptionName name, bool value)
        {
            if ((int) client.Client.GetSocketOption(level, name) == (value ? 1 : 0))
            {
                return value;
            }
            client.Client.SetSocketOption(level, name, value);
            return (int) client.Client.GetSocketOption(level, name) == 1 ? true : false;
        }

        /// <summary>
        /// Configure the value of the specified integer <b>Socket</b> option.
        /// </summary>
        /// <param name="client">
        /// Target <b>TcpClient</b> object.
        /// </param>
        /// <param name="level">
        /// The <b>SocketOptionLevel</b> of the target Socket option.
        /// </param>
        /// <param name="name">
        /// The <b>SocketOptionName</b> of the target Socket option.
        /// </param>
        /// <param name="value">
        /// The desired value of the target <b>Socket</b> option.
        /// </param>
        /// <returns>
        /// Returns the new value of the target <b>Socket</b> option.
        /// </returns>
        internal static int SetSocketOption(TcpClient client, SocketOptionLevel level, SocketOptionName name, int value)
        {
            if ((int) client.Client.GetSocketOption(level, name) == value)
            {
                return value;
            }
            client.Client.SetSocketOption(level, name, value);
            return (int) client.Client.GetSocketOption(level, name);
        }

        /// <summary>
        /// BufferSize validation method.
        /// </summary>
        /// <param name="isReceiveBuffer">
        /// Boolean flag that represents whether a receive or send buffer
        /// was configured; <b>true</b> represents a receive buffer;
        /// <b>false</b> represents a send buffer.
        /// </param>
        /// <param name="actual">
        /// Actual buffer size.
        /// </param>
        /// <param name="requested">
        /// Requested buffer size.
        /// </param>
        internal static void ValidateBufferSize(bool isReceiveBuffer, int actual, int requested)
        {
            string name = isReceiveBuffer ? "receive" : "send";
            if (actual < requested)
            {
                CacheFactory.Log("Failed to set a TCP Socket " + name
                                 + " buffer size to " + requested
                                 + " bytes; actual size is " + actual + " bytes. "
                                 + "Consult your OS documentation regarding increasing the maximum "
                                 + "TCP Socket " + name + " buffer size. Proceeding with the "
                                 + "actual value may cause sub-optimal performance.",
                                 CacheFactory.LogLevel.Warn);
            }
        }

        #endregion
    }

    ///<summary>
    /// Utility struct to compute baseport and subport from the 32 bit port 
    /// number.
    ///</summary>
    /// <since>Coherence 12.1.2</since>
    public struct Port32
    {
        /// <summary>
        /// The baseport number.
        /// </summary>
        public Int32 Baseport;

        /// <summary>
        /// The subport number.
        /// </summary>
        public Int32 Subport;

        /// <summary>
        /// Construct a Port32 struct with the 32 bit port number.
        /// </summary>
        /// <param name="port">
        /// The 32 bit port number.
        /// </param>
        public Port32(Int32 port)
        {
            int uport = ~port;

            Baseport = uport >> 16;
            Subport = uport & 0xffff;
            if (Baseport == -1)
            {
                Baseport = port;
                Subport = -1;
            }
            else if (Baseport < -1)
            {
                Baseport = (int)((uint) uport >> 16);
            }
        }
    }

    /// <summary>
    /// Well known subport.
    /// </summary>
    /// <since>Coherence 12.1.2</since>
    public enum WellKnownSubPorts
    {
        /// <summary>
        /// The name service subport.
        /// </summary>
        NameService = 3
    }
}