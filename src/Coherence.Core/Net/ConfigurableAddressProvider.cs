/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;

using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.Net
{
    /// <summary>
    /// ConfigurableAddressProvider is an implementation of the
    /// <see cref="IAddressProvider"/> interface based on a static list of
    /// addresses configured in an XML element that contains one or more
    /// items in the following format:
    /// <pre>
    /// &lt;socket-address&gt;
    ///     &lt;address&gt;...&lt;/address&gt;
    ///     &lt;port&gt;...&lt;/port&gt;
    /// &lt;/socket-address&gt;
    /// </pre>
    /// </summary>
    /// <remarks>
    /// The order of items in the configured list will be randomized to
    /// provide basic load balancing.
    /// This implementation is not thread safe.
    /// </remarks>
    /// <author>Gene Gleyzer  2008.08.18</author>
    /// <author>Ana Cikic  2008.08.22</author>
    /// <since>Coherence 3.4</since>
    public class ConfigurableAddressProvider : IAddressProvider
    {
        #region Constructors

        /// <summary>
        /// Construct an instance of ConfigurableAddressProvider based on the
        /// specified XML element.
        /// </summary>
        /// <remarks>
        /// Unresolvable addresses will be skipped.
        /// </remarks>
        /// <param name="xmlConfig">
        /// The XML element that contains the configuration info.
        /// </param>
        public ConfigurableAddressProvider(IXmlElement xmlConfig)
            : this(xmlConfig, /* isSafe */ true)
        {
        }

        /// <summary>
        /// Construct an instance of ConfigurableAddressProvider based on the
        /// specified XML element.
        /// </summary>
        /// <param name="xmlConfig">
        /// The XML element that contains the configuration info.
        /// </param>
        /// <param name="isSafe">
        /// True if the provider skips unresolved addresses.
        /// </param>
        /// <since>12.2.1</since>
        public ConfigurableAddressProvider(IXmlElement xmlConfig, bool isSafe)
        {
            Configure(xmlConfig);
            m_isSafe = isSafe;
        }

        #endregion

        #region IAddressProvider implementation

        /// <summary>
        /// Next available address to use.
        /// </summary>
        /// <remarks>
        /// If the caller can successfully use the returned address (e.g. a
        /// connection was established), it should call the
        /// IAddressProvider's <see cref="IAddressProvider.Accept"/> method.
        /// </remarks>
        /// <value>
        /// The next available address or <c>null</c> if the list of
        /// available addresses was exhausted.
        /// </value>
        public virtual IPEndPoint NextAddress
        {
            get
            {
                IList<AddressHolder> list       = f_listHolders;
                int                  itemsCount = list.Count;

                if (itemsCount == 0)
                {
                    return null;
                }

                IEnumerator<IPEndPoint> iterAddr = m_iterAddr;
                int                     last     = m_last;
                AddressHolder           holder   = null;
                IPEndPoint              address;

                do
                {
                    while (iterAddr == null || !iterAddr.MoveNext())
                    {
                        // select next configured address
                        last = m_last = (last + 1) % itemsCount;

                        holder = list[last];

                        if (holder.IsPending)
                        {
                            Reset();
                            return null;
                        }

                        holder.IsPending = true;
                        iterAddr = m_iterAddr = ResolveAddress(holder.Host, holder.Port);
                    }

                    address = iterAddr.Current;
                }
                while (address == null);

                return address;
            }
        }

        /// <summary>
        /// This method should be called by the client immediately after it
        /// determines that it can successfully use an address returned by
        /// the <see cref="IAddressProvider.NextAddress"/>.
        /// </summary>
        public virtual void Accept()
        {
            Reset(m_last);
        }

        /// <summary>
        /// This method should be called by the client immediately after it
        /// determines that an attempt to use an address returned by the
        /// <see cref="IAddressProvider.NextAddress"/> has failed.
        /// </summary>
        /// <param name="eCause">
        /// (Optional) an exception that carries the reason why the caller
        /// rejected the previously returned address.
        /// </param>
        public virtual void Reject(Exception eCause)
        {}

        #endregion

        #region Helper methods

        /// <summary>
        /// Make all addresses iterable, starting at the first address.
        /// </summary>
        protected void Reset()
        {
            Reset(-1);
        }

        /// <summary>
        /// Make all addresses iterable, starting at the index after the specified one.
        /// </summary>
        /// <param name="iLast">
        /// The index after which to start.
        /// </param>
        protected void Reset(int iLast)
        {
            // reset all holders
            IList<AddressHolder> list = f_listHolders;
            for (int i = 0, c = list.Count; i < c; i++)
            {
                list[i].IsPending = false;
            }
            m_iterAddr = null;
            m_last     = iLast;
        }

        /// <summary>
        /// Configure this ConfigurableAddressProvider based on the specified XML.
        /// </summary>
        /// <param name="xmlConfig">
        /// The XML element that contains the configuration info.
        /// </param>
        protected void Configure(IXmlElement xmlConfig)
        {
            IList<AddressHolder> list = new List<AddressHolder>();

            foreach (IXmlElement xmlAddr in xmlConfig.ElementList)
            {
                string addr;
                int    port;

                switch (xmlAddr.Name)
                {
                    case "socket-address":
                        addr = xmlAddr.GetSafeElement("address").GetString().Trim();
                        port = xmlAddr.GetSafeElement("port").GetInt();
                        break;

                    case "host-address":
                    case "address":
                        addr = xmlAddr.GetString().Trim();
                        port = 0;
                        break;

                    default:
                        continue;
                }

                if (addr.Length == 0)
                {
                    // ignore empty elements
                    continue;
                }

                try
                {
                    list.Add(new AddressHolder(addr, port).Validate());
                }
                catch (Exception e)
                {
                    throw new Exception("Invalid configuration element: " + xmlAddr, e);
                }
            }

            f_listHolders = SortHolders(list);
        }

        /// <summary>
        /// Sort the holders in the order to be returned by the 
        /// NextAddress property.  This implementation randomizes the holder
        /// list for simple load balancing.
        /// </summary>
        /// <param name="list">
        /// The original list retrieved from the configuration.
        /// </param>
        /// <returns>
        /// The re-ordered list.
        /// </returns>
        protected IList<AddressHolder> SortHolders(IList<AddressHolder> list)
        {
            return CollectionUtils.Randomize(list);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Return a string representation of this
        /// ConfigurableAddressProvider.
        /// </summary>
        /// <returns>
        /// A string representation of the list of configured addresses.
        /// </returns>
        public override string ToString()
        {
            return '[' + String.Join(",", f_listHolders.Select(p => p.Host + ':' + p.Port)) + ']';
        }
        
        #endregion

        #region helper methods

        /// <summary>
        /// Resolve a host and port.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port</param>
        /// <returns>An <see cref="IPEndPoint"/> enumerator.</returns>
        /// <since>12.2.1</since>
        protected IEnumerator<IPEndPoint> ResolveAddress(string host, int port)
        {
            IPAddress[] arrAddr;

            try
            {
                arrAddr = "localhost".Equals(host)
                    ? new IPAddress[] { NetworkUtils.GetLocalHostAddress(RequestTimeout) }
                    : CollectionUtils.Randomize(NetworkUtils.GetAllAddresses(host, RequestTimeout));
            }
            catch (Exception e)
            {
                if (m_isSafe)
                {
                    // Failed to resolve the host.  Return an empty list.
                    arrAddr = new IPAddress[] { };

                    AddressHolder holder = f_listHolders[m_last];
                    if (!holder.IsReported)
                    {
                        holder.IsReported = true;
                        CacheFactory.Log("The ConfigurableAddressProvider got an exception: " +
                                         e + " when resolving address; " +
                            "skipping the unresolveable address \"" + host +
                            "\".", CacheFactory.LogLevel.Info);
                    }
                }
                else
                {
                    throw e;
                }
            }

            foreach (IPAddress addr in arrAddr)
            {
                yield return new IPEndPoint(addr, port);
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// A list of AddressHolder objects.
        /// </summary>
        protected IList<AddressHolder> f_listHolders;

        /// <summary>
        /// An address iterator for the previously resolved address.
        /// </summary>
        /// <since>12.2.1</since>
        protected IEnumerator<IPEndPoint> m_iterAddr;

        /// <summary>
        /// Index of the last returned address.
        /// </summary>
        protected int m_last = -1;

        /// <summary>
        /// Specifies if the provider is only to return resolved addresses.
        /// </summary>
        /// <since>12.2.1</since>
        protected readonly bool m_isSafe;

        /// <summary>
        /// The maximum amount of time to wait. Initializing to NO_TIMEOUT.
        /// </summary>
        private long m_requestTimeout = NetworkUtils.NO_TIMEOUT;

        #endregion

        #region Properties

        /// <summary>
        /// The maximum amount of time to wait.
        /// </summary>
        public long RequestTimeout
        {
            set { m_requestTimeout = value; }
            get { return m_requestTimeout; }
        }

        #endregion

        #region Inner class: AddressHolder

        /// <summary>
        /// A stateful holder for obtaining an <see cref="IPEndPoint"/> object.
        /// </summary>
        protected class AddressHolder
        {
            #region constructors

            /// <summary>
            /// Construct an AddressHolder for the specified
            /// <see cref="IPEndPoint"/>.
            /// </summary>
            /// <param name="host">
            /// The host name or IP.
            /// </param>
            /// <param name="port">
            /// The port number.
            /// </param>
            public AddressHolder(String host, int port)
            {
                f_host = host;
                f_port = port;
            }

            /// <summary>
            /// Throw ArgumentException if any values are invalid.
            /// </summary>
            /// <returns>this</returns>
            public AddressHolder Validate()
            {
                if (f_host == null)
                {
                    throw new ArgumentNullException("host");
                }

                if (f_port < 0 || f_port > 0xFFFF)
                {
                    throw new ArgumentOutOfRangeException(
                        "port " + f_port + " out of range of 0 to " + 0xFFFF);
                }
                return this;
            }

            #endregion

            #region accessors

            /// <summary>
            /// Whether the underlying address has been accepted.
            /// </summary>
            /// <value>
            /// <b>true</b> iff the underlying address has not yet been
            /// accepted.
            /// </value>
            public virtual bool IsPending
            {
                get { return m_isPending; }
                set { m_isPending = value; }
            }

            /// <summary>
            /// Whether this address has already been reported as unresolvable.
            /// </summary>
            /// <value>
            /// <b>true</b> iff the underlying address has been reported.
            /// </value>
            public virtual bool IsReported
            {
                get { return m_isReported; }
                set { m_isReported = value; }
            }

            /// <summary>
            /// The host name.
            /// </summary>
            public virtual String Host
            {
                get { return f_host; }
            }

            /// <summary>
            /// The port.
            /// </summary>
            public virtual int Port
            {
                get { return f_port; }
            }

            #endregion

            #region data fields

            /// <summary>
            /// The configured address, either hostname or IP address.
            /// </summary>
            protected readonly String f_host;

            /// <summary>
            /// The configured port.
            /// </summary>
            protected readonly int f_port;

            /// <summary>
            /// A flag indicating that the underlying address has been
            /// provided to a client, but has not yet been accepted.
            /// </summary>
            private bool m_isPending;

            /// <summary>
            /// A flag indicating if this address has already been reported as
            /// unresolved.
            /// </summary>
            private bool m_isReported;

            #endregion
        }

        #endregion
    }
}
