/*
 * Copyright (c) 2000, 2021, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;
using Tangosol.Run.Xml;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator
{
    /// <summary>
    ///  A <see cref="IConnectionInitiator"/> implementation that initiates
    /// <see cref="Connection"/> over TCP/IP.
    /// </summary>
    public class TcpInitiator : Initiator
    {
        #region Properties

        /// <summary>
        /// Indicates whether or not <b>KeepAlive</b> is enabled on
        /// <b>Socket</b> objects created by this TcpInitiator.
        /// </summary>
        /// <value>
        /// <b>true</b> if <b>KeepAlive</b> is enabled on <b>Socket</b>
        /// objects created by this TcpInitiator.
        /// </value>
        public virtual bool IsKeepAliveEnabled { get; protected set; }

        /// <summary>
        /// Indicates if <b>Socket</b> objects created by this TcpInitiator
        /// that are explicitly bound to a local <b>IPEndPoint</b> will be
        /// bound even if a previously bound <b>Socket</b> is in a timeout
        /// state.
        /// </summary>
        /// <value>
        /// <b>true</b> if <b>Socket</b> objects created by this TcpInitiator
        /// that are explicitly bound to a local <b>IPEndPoint</b> will be
        /// bound even if a previously bound <b>Socket</b> is in a timeout
        /// state, <b>false</b> otherwise.
        /// </value>
        public virtual bool IsLocalAddressReusable { get; protected set; }

        /// <summary>
        /// Indicates whether or not TCP delay (Nagle's algorithm) is enabled
        /// on <b>Socket</b> objects created by this TcpInitiator.
        /// </summary>
        /// <value>
        /// <b>true</b> if TCP delay is enabled on <b>Socket</b> objects
        /// created by this TcpInitiator, <b>false</b> otherwise.
        /// </value>
        public virtual bool IsTcpDelayEnabled { get; protected set; }

        /// <summary>
        /// The linger timeout of a <b>Socket</b> created by this
        /// TcpInitiator.
        /// </summary>
        /// <remarks>
        /// If negative, linger will be disabled. If 0, the default value
        /// will be used.
        /// </remarks>
        /// <value>
        /// Linger timeout value.
        /// </value>
        public virtual long LingerTimeout { get; protected set; }

        /// <summary>
        /// The local <b>IPEndPoint</b> that all <b>Socket</b> objects
        /// created by this TcpInitiator will be bound to.
        /// </summary>
        /// <remarks>
        /// If <c>null</c>, an <b>IPEndPoint</b> created from an ephemeral
        /// port and a valid local address will be used.
        /// </remarks>
        /// <value>
        /// The local <b>IPEndPoint</b>.
        /// </value>
        public virtual IPEndPoint LocalAddress { get; protected set; }

        /// <summary>
        /// The <see cref="IProtocol"/> understood by the IReceiver.
        /// </summary>
        /// <remarks>
        /// Only <b>IChannel</b> objects with the specified <b>IProtocol</b>
        /// can be registered with this IReceiver.
        /// </remarks>
        /// <value>
        /// The <b>IProtocol</b> used by this IReceiver.
        /// </value>
        public override IProtocol Protocol
        {
            get
            {
                IProtocol protocol = m_protocol;
                if (protocol == null)
                {
                    Protocol = (protocol = TcpMessagingProtocol.Instance);
                }
                return protocol;
            }
            set
            {
                m_protocol = value;
            }
        }

        /// <summary>
        /// The size of the receive buffer (in bytes) of all <b>Socket</b>
        /// objects created by this TcpInitiator.
        /// </summary>
        /// <remarks>
        /// If 0 or negative, the default receive buffer size will be used.
        /// </remarks>
        /// <value>
        /// The size of the receive buffer (in bytes).
        /// </value>
        public virtual long ReceiveBufferSize { get; protected set; }

        /// <summary>
        /// The <see cref="IAddressProvider"/> used by the TcpInitiator to
        /// obtain the address(es) of the remote TcpAcceptor(s) that it will
        /// connect to.
        /// </summary>
        /// <value>
        /// An <b>IAddressProvider</b>.
        /// </value>
        public virtual IAddressProvider RemoteAddressProvider { get; set; }

        /// <summary>
        /// The size of the send buffer (in bytes) of all <b>Socket</b>
        /// objects created by this TcpInitiator.
        /// </summary>
        /// <remarks>
        /// If 0 or negative, the default send buffer size will be used.
        /// </remarks>
        /// <value>
        /// The size of the send buffer (in bytes).
        /// </value>
        public virtual long SendBufferSize { get; protected set; }

        /// <summary>
        /// Get or sets the current StreamProvider.
        /// </summary>
        public virtual IStreamProvider StreamProvider { get; protected set; }

        /// <summary>
        /// Whether the remote AddressProvider is for connections to a NameService.
        /// </summary>
        public virtual bool IsNameService { get; set; }

        /// <summary>
        /// The subport to connect to.
        /// </summary>
        public virtual int Subport { get; set; }

       #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TcpInitiator()
        {
            DaemonState = DaemonState.Initial;
            IsKeepAliveEnabled = true;
            LingerTimeout = -1L;
        }

        #endregion

        #region Initiator overrides

        /// <summary>
        /// Human-readable description of additional Service properties.
        /// </summary>
        /// <remarks>
        /// Used by <see cref="Service.ToString"/>.
        /// </remarks>
        public override string Description
        {
            get
            {
                var sb = new StringBuilder(base.Description);

                IPEndPoint addr = LocalAddress;
                if (addr != null)
                {
                    sb.Append(", LocalAddress=")
                            .Append(NetworkUtils.ToString(addr))
                            .Append(", LocalAddressReusable=")
                            .Append(IsLocalAddressReusable);
                }

                sb.Append(", RemoteAddresses=")
                        .Append(RemoteAddressProvider)
                        .Append(", KeepAliveEnabled=")
                        .Append(IsKeepAliveEnabled)
                        .Append(", TcpDelayEnabled=")
                        .Append(IsTcpDelayEnabled)
                        .Append(", ReceiveBufferSize=")
                        .Append(StringUtils.ToMemorySizeString(
                                        ReceiveBufferSize, false))
                        .Append(", SendBufferSize=")
                        .Append(StringUtils.ToMemorySizeString(SendBufferSize,
                                                               false))
                        .Append(", LingerTimeout=")
                        .Append(LingerTimeout);

                return sb.ToString();
            }
        }

        /// <summary>
        /// Configure the controllable service.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// This method can only be called before the controllable service
        /// is started.
        /// </remarks>
        /// <param name="xml">
        /// An <see cref="IXmlElement"/> carrying configuration information
        /// specific to the IControllable object.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service is already running.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the configuration information is invalid.
        /// </exception>
        public override void Configure(IXmlElement xml)
        {
            lock (this)
            {
                base.Configure(xml);
                if (xml == null)
                {
                    return;
                }

                // <tcp-initiator>
                IXmlElement xmlCat = xml.GetSafeElement("tcp-initiator");

                // <stream-provider/>
                IXmlElement xmlSub = xmlCat.GetSafeElement("stream-provider");
                StreamProvider = StreamProviderFactory.CreateProvider(xmlSub);

                // <local-address>
                xmlSub = xmlCat.GetSafeElement("local-address");

                // <address>
                // <port>
                LocalAddress = ParseLocalSocketAddress(xmlSub);

                // <reusable>
                IsLocalAddressReusable = xmlSub.GetSafeElement("reusable").
                        GetBoolean(IsLocalAddressReusable);

                // <remote-addresses>
                bool isNameService = false;
                xmlSub = xmlCat.GetElement("name-service-addresses");
                if (xmlSub == null)
                {
                    xmlSub = xmlCat.GetSafeElement("remote-addresses");
                }
                else
                {
                    isNameService = true;
                }
                IAddressProviderFactory factory;

                IXmlElement xmlProvider = xmlSub.GetElement("address-provider");
                bool        missing     = xmlProvider == null;
                bool        empty       = !missing && xmlProvider.IsEmpty;
                if (empty || missing)
                {
                    ConfigurableAddressProviderFactory factoryImpl = 
                            new ConfigurableAddressProviderFactory();
                    factoryImpl.Config = missing ? xmlSub : xmlProvider;
                    factory            = factoryImpl;
                }
                else
                {
                    String name = xmlProvider.GetString();
                    factory = (IAddressProviderFactory) 
                            OperationalContext.AddressProviderMap[name];
                    if (factory == null)
                    {
                        throw new ArgumentException("Address provider "
                                + name + " not found.");
                    }
                }
                RemoteAddressProvider = factory.CreateAddressProvider();
                if (RemoteAddressProvider is ConfigurableAddressProvider && ConnectTimeout > 0)
                {
                    ((ConfigurableAddressProvider) RemoteAddressProvider).RequestTimeout = ConnectTimeout;
                }

                IsNameService         = isNameService;
                if (isNameService)
                {
                    Subport = (int) WellKnownSubPorts.NameService;
                }
                else
                {
                    Subport = -1;
                }

                // <reuse-address>
                IsLocalAddressReusable = xmlCat.GetSafeElement("reuse-address").
                        GetBoolean(IsLocalAddressReusable);

                // <keep-alive-enabled/>
                IsKeepAliveEnabled = xmlCat.GetSafeElement("keep-alive-enabled")
                        .
                        GetBoolean(IsKeepAliveEnabled);

                // <tcp-delay-enabled>
                IsTcpDelayEnabled = xmlCat.GetSafeElement("tcp-delay-enabled").
                        GetBoolean(IsTcpDelayEnabled);

                // <receive-buffer-size>
                ReceiveBufferSize = ParseMemorySize(
                        xmlCat, "receive-buffer-size", ReceiveBufferSize);

                // <send-buffer-size>
                SendBufferSize = ParseMemorySize(
                        xmlCat, "send-buffer-size", SendBufferSize);

                // <linger-timeout>
                LingerTimeout = ParseTime(
                        xmlCat, "linger-timeout", LingerTimeout);
            }
        }

        /// <summary>
        /// Factory method: create a new <see cref="Connection"/>.
        /// </summary>
        /// <remarks>
        /// Implementations must configure the <b>Connection</b> with a
        /// reference to this <b>IConnectionManager</b>.
        /// </remarks>
        /// <returns>
        /// A new <b>Connection</b> object that has yet to be opened.
        /// </returns>
        protected override Connection InstantiateConnection()
        {
            return new TcpConnection {ConnectionManager = this};
        }

        /// <summary>
        /// Open and return a new Connection.
        /// </summary>
        /// <returns>
        /// A newly opened Connection.
        /// </returns>
        protected override Connection OpenConnection()
        {
            IAddressProvider provider = RemoteAddressProvider;
            Debug.Assert(provider != null);

            // Default value for Coherence is 0, which is an infinite timeout.
            // Default value for .NET IAsyncResult.WaitOne infinite timeout is -1.
            // So, convert the Coherence infinite value to .NET Timeout.Infinite.
            Int32 cMillis = (Int32) ConnectTimeout;
            cMillis       = cMillis <=0 ? Timeout.Infinite : cMillis;

            // open a new connection
            var         addresses    = new StringBuilder().Append('[');
            IEnumerator enmrRedirect = null;
            IEnumerator enmrSubport  = null;
            IPEndPoint  addrNext     = null;
            Int32       subportNext  = -1;
            Exception   cause        = null;
            for ( ; ; )
            {
                var connection = (TcpConnection) InstantiateConnection();

                IPEndPoint addr;
                Int32      subport;
                if (enmrRedirect == null || addrNext == null)
                {
                    addr    = provider.NextAddress;
                    subport = Subport;

                    // reset redirection information
                    enmrRedirect = null;
                    enmrSubport  = null;
                }
                else
                {
                    addr     = addrNext;
                    addrNext = enmrRedirect.MoveNext() ?
                            (IPEndPoint) enmrRedirect.Current : null;

                    subport     = subportNext;
                    subportNext = enmrSubport.MoveNext() ?
                            (Int32) enmrSubport.Current : -1;

                    // update redirection information
                    connection.IsRedirect = true;
                }

                if (addr == null)
                {
                    break;
                }

                string address = NetworkUtils.ToString(addr, subport);
                if (RemoteAddressProvider is ConfigurableAddressProvider)
                {
                    StreamProvider.RemoteAddress = ((ConfigurableAddressProvider) RemoteAddressProvider).RemoteHostAddress;
                }

                if (addresses.Length > 1)
                {
                    addresses.Append(", ");
                }
                addresses.Append(address);

                // create and configure a new client
                TcpClient client = InstantiateClient();
                try
                {
                    if (enmrRedirect == null)
                    {
                        CacheFactory.Log("Connecting Socket to " + address,
                                         CacheFactory.LogLevel.Debug);
                    }
                    else
                    {
                        CacheFactory.Log("Redirecting Socket to " + address,
                                         CacheFactory.LogLevel.Debug);
                    }

                    IAsyncResult result = client.BeginConnect(addr.Address, addr.Port, null, null);
                    result.AsyncWaitHandle.WaitOne(cMillis);
                    if (!client.Connected)
                    {
                        CacheFactory.Log("Error connecting TcpClient to "
                                     + address + ": connection timeout", CacheFactory.LogLevel.Info);
                        NetworkUtils.Close(client);
                        continue;
                    }
 
                    CacheFactory.Log("Connected TcpClient to " + address,
                                     CacheFactory.LogLevel.Info);
                    connection.Client = client;
                }
                catch (Exception e)
                {
                    cause = e;
                    CacheFactory.Log("Error connecting TcpClient to "
                                     + address + ": " + e, CacheFactory.LogLevel.Info);

                    NetworkUtils.Close(client);

                    // if we aren't current redirecting, or we've tried the last redirect
                    // address, reject the last address supplied by the address provider
                    if (enmrRedirect == null || addrNext == null)
                    {
                        provider.Reject(e);
                    }
                    continue;
                }

                // write out subport info
                if (subport != -1)
                {
                    bool secure = connection.IsSecure;
                    Stream stream = connection.Stream = StreamProvider.GetStream(client);
                    if (secure)
                    {
                        Monitor.Enter(stream);
                    }

                    try
                    {
                        NetworkUtils.WriteSubport(stream, subport);
                    }
                    catch (Exception e)
                    {
                        CacheFactory.Log("Error connecting TcpClient to "
                                     + address + ": " + e, CacheFactory.LogLevel.Info);
                        throw new ConnectionException(e);
                    }
                    finally
                    {
                        if (secure)
                        {
                            Monitor.Exit(stream);
                        }
                    }
                }

                try
                {
                    connection.Open();
                }
                catch (Exception e)
                {
                    if (enmrRedirect == null && connection.IsRedirect)
                    {
                        IList list = connection.RedirectList;

                        // create an IPEndPoint list from from the redirect list
                        var listRedirect = new ArrayList(list.Count);
                        var listSubport  = new ArrayList(list.Count);
                        foreach (var o in list)
                        {
                            var ao     = (Object[]) o;
                            var s      = (String) ao[0];
                            var port32 = new Port32((Int32)ao[1]);
                            try
                            {
                                listRedirect.Add(new IPEndPoint(NetworkUtils.GetHostAddress(s, ConnectTimeout),
                                    port32.Baseport));
                                listSubport.Add(port32.Subport);
                            }
                            catch (Exception)
                            {
                                // invalid or unresolvable address
                                CacheFactory.Log("Skipping unresolvable address \"" + s + "\".",
                                        CacheFactory.LogLevel.Info);
                            }
                        }
                        enmrRedirect = listRedirect.GetEnumerator();
                        enmrSubport  = listSubport.GetEnumerator();
                        if (enmrRedirect.MoveNext() && enmrSubport.MoveNext())
                        {
                            addrNext    = (IPEndPoint) enmrRedirect.Current;
                            subportNext = (Int32) enmrSubport.Current;
                        }
                        else
                        {
                            addrNext    = null;
                            subportNext = -1;
                        }
                    }
                    else
                    {
                        CacheFactory.Log("Error establishing a connection with " + address + ": " + e,
                                         CacheFactory.LogLevel.Info);

                        // if we aren't current redirecting, or we've tried the last redirect
                        // address, reject the last address supplied by the address provider
                        if (enmrRedirect == null || addrNext == null)
                        {
                            provider.Reject(e);
                        }
                    }
                    continue;
                }

                provider.Accept();
                return connection;
            }
            addresses.Append(']');

            String message = "could not establish a connection "
                             + "to one of the following addresses: " + addresses
                             + "; make sure the \"remote-addresses\" configuration "
                             + "element contains an address and port of a running "
                             + "TcpAcceptor";
            throw cause == null ? new ConnectionException(message)
                    : new ConnectionException(message, cause);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Configure the given <b>TcpClient</b>.
        /// </summary>
        /// <param name="client">
        /// The <b>Socket</b> to configure.
        /// </param>
        protected void ConfigureSocket(TcpClient client)
        {
            try
            {
                NetworkUtils.SetKeepAlive(client, IsKeepAliveEnabled);
                NetworkUtils.SetReuseAddress(client, IsLocalAddressReusable);
                NetworkUtils.SetTcpNoDelay(client, !IsTcpDelayEnabled);
                NetworkUtils.SetReceiveBufferSize(client,
                                                  (int) ReceiveBufferSize);
                NetworkUtils.SetSendBufferSize(client, (int) SendBufferSize);

                long millis = LingerTimeout;
                int secs = millis >= 0 ? (int) (millis/1000L) : -1; // seconds
                NetworkUtils.SetLingerTime(client, secs);
            }
            catch (Exception e)
            {
                throw new Exception("error configuring Socket", e);
            }

            if (LocalAddress != null && !client.Client.IsBound)
            {
                throw new Exception("could not bind Socket to " +
                                    NetworkUtils.ToString(LocalAddress));
            }
        }

        /// <summary>
        /// Factory method: create and configure a new <b>Socket</b>.
        /// </summary>
        /// <returns>
        /// A new <b>Socket</b>.
        /// </returns>s
        protected virtual TcpClient InstantiateClient()
        {
            IPEndPoint addr   = LocalAddress;
            TcpClient  client = addr == null
                                       ? new TcpClient()
                                       : new TcpClient(addr);

            if (addr != null)
            {
                CacheFactory.Log(
                        "Binding Socket to " + NetworkUtils.ToString(addr),
                        CacheFactory.LogLevel.Quiet);
            }

            ConfigureSocket(client);
            return client;
        }

        /// <summary>
        /// Parse the given <b>IXmlElement</b> as a local <b>IPEndPoint</b>.
        /// </summary>
        /// <remarks>
        /// If the specified <b>IXmlElement</b> contains an empty address,
        /// <c>null</c> is returned.
        /// </remarks>
        /// <param name="xml">
        /// The <b>IXmlElement</b> to parse.
        /// </param>
        /// <returns>
        /// A new <b>IPEndPoint</b> representing the contents of the given
        /// <b>XmlNode</b>.
        /// </returns>
        protected static IPEndPoint ParseLocalSocketAddress(IXmlElement xml)
        {
            IXmlElement xmlAddr = xml.GetElement("address");
            IXmlElement xmlPort = xml.GetElement("port");
            String sAddressFamiliy =
                    xml.GetSafeElement("address-family").GetString(
                            "InterNetwork");

            if (xmlAddr == null && xmlPort == null)
            {
                return null;
            }

            string addr = xmlAddr == null ? "localhost" : xmlAddr.GetString();
            int port = xmlPort == null ? 0 : xmlPort.GetInt();

            NetworkUtils.PreferredAddressFamily =
                    (AddressFamily)
                    Enum.Parse(typeof (AddressFamily), sAddressFamiliy);

            IPAddress ipAddress;
            try
            {
                ipAddress = addr.Equals("localhost")
                                    ? NetworkUtils.GetLocalHostAddress()
                                    : NetworkUtils.GetHostAddress(addr);
            }
            catch (Exception e)
            {
                throw new Exception("The \"" + xml.Name + "\" configuration "
                                    +
                                    "element contains an invalid \"address\" element",
                                    e);
            }

            try
            {
                return new IPEndPoint(ipAddress, port);
            }
            catch (Exception e)
            {
                throw new Exception("The \"" + xml.Name + "\" configuration "
                                    +
                                    "element contains an invalid \"port\" element",
                                    e);
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// Protocol.
        /// </summary>
        private IProtocol m_protocol;

        #endregion

        #region Inner class: TcpConnection

        /// <summary>
        /// Implementation of <see cref="TcpConnection"/>.
        /// </summary>
        internal class TcpConnection : Net.Messaging.Impl.TcpConnection
        {
            #region Properties

            /// <summary>
            /// True if the TcpConnection should be redirected.
            /// </summary>
            public bool IsRedirect { get; set; }

            /// <summary>
            /// True if the TcpConnection is an SSL connection.
            /// </summary>
            public bool IsSecure { get; set; }

            /// <summary>
            /// The Reader daemon that reads encoded <b>Message</b> objects
            /// from the <b>Socket</b> associated with this TcpConnection.
            /// </summary>
            /// <value>
            /// The Reader daemon that reads encoded <b>Messsage</b> objects
            /// from the <b>Socket</b> associated with this TcpConnection.
            /// </value>
            protected virtual TcpReader Reader { get; set; }

            /// <summary>
            /// A list of TCP/IP addresses that the TcpConnection should be
            /// redirected to. Each element of the list is a two element
            /// array, with the first element being the IP address in string
            /// format and the second being the port number.
            /// </summary>
            public IList RedirectList { get; set; }

            /// <summary>
            /// Stream to read/write data.
            /// </summary>
            public Stream Stream
            {
                get { return m_stream; }
                set
                {
                    IsSecure = value is SslStream;
                    m_stream = value;
                }
            }

            #endregion

            #region TcpConnection overrides

            /// <summary>
            /// The <see cref="Connection.Close()"/> implementation method.
            /// </summary>
            /// <remarks>
            /// This method is called on the service thread.
            /// </remarks>
            /// <param name="notify">
            /// if <b>true</b>, notify the peer that the Connection is being
            /// closed.
            /// </param>
            /// <param name="e">
            /// The optional reason why the Connection is being closed.
            /// </param>
            /// <param name="millis">
            /// The number of milliseconds to wait for the Connection to close;
            /// pass 0 to perform a non-blocking close or -1 to wait forever.
            /// </param>
            /// <returns>
            /// <b>true</b> if the invocation of this method closed the
            /// Connection.
            /// </returns>
            public override bool CloseInternal(bool notify, Exception e,
                                               int millis)
            {
                if (base.CloseInternal(notify, e, millis))
                {
                    var reader = Reader;
                    if (reader != null)
                    {
                        reader.Stop();
                        Reader = null;
                    }

                    var stream = Stream;
                    if (stream != null)
                    {
                        stream.Close();
                    }

                    NetworkUtils.Close(Client);

                    return true;
                }

                return false;
            }

            /// <summary>
            /// The <see cref="Connection.Open()"/> implementation method.
            /// </summary>
            /// <remarks>
            /// This method is called on the service thread.
            /// </remarks>
            public override void OpenInternal()
            {
                base.OpenInternal();

                var initiator = (TcpInitiator) ConnectionManager;
                Debug.Assert(initiator != null);

                TcpClient client = Client;
                Debug.Assert(client != null);

                if (Stream == null)
                {
                    Stream = initiator.StreamProvider.GetStream(client);
                }

                var reader = new TcpReader(this);
                reader.Start();
                Reader = reader;
            }

            /// <summary>
            /// Send the given <b>DataWriter</b> through this Connection.
            /// </summary>
            /// <param name="writer">
            /// The <b>DataWriter</b> to send.
            /// </param>
            public override void Send(DataWriter writer)
            {
                base.Send(writer);

                var messageStream = (MemoryStream) writer.BaseStream;
                int messageLength = (int) messageStream.Length - 5;
                        // see Connection.Send()
                int lengthLength = PofHelper.LengthPackedInt32(messageLength);
                int lengthPosition = 5 - lengthLength;

                // encode the length
                writer.Seek(lengthPosition, SeekOrigin.Begin);
                writer.WritePackedInt32(messageLength);

                bool   secure = IsSecure;
                Stream stream = Stream;
                if (secure)
                {
                    Monitor.Enter(stream);
                }

                try
                {
                    stream.Write(messageStream.GetBuffer(),
                            lengthPosition,
                            lengthLength + messageLength);
                }
                catch (Exception e)
                {
                    throw new ConnectionException(e);
                }
                finally
                {
                    if (secure)
                    {
                        Monitor.Exit(stream);
                    }
                }
            }

            #endregion

            #region Inner class: TcpReader

            /// <summary>
            /// The <see cref="Daemon"/> that is responsible for reading
            /// encoded <see cref="Message"/>s off the parent
            /// <b>TcpConnection</b> and dispatching them to the
            /// <b>TcpInitiator</b>.
            /// </summary>
            internal class TcpReader : Daemon
            {
                #region Properties

                /// <summary>
                /// Parent <b>TcpConnection</b> object.
                /// </summary>
                /// <value>
                /// <b>TcpConnection</b> object.
                /// </value>
                public virtual TcpConnection Connection { get; set; }

                #endregion

                #region Constructors

                /// <summary>
                /// Initialization constructor.
                /// </summary>
                /// <param name="connection">
                /// Parent <b>TcpConnection</b> object.
                /// </param>
                public TcpReader(TcpConnection connection)
                {
                    Connection = connection;
                }

                #endregion

                #region Daemon override methods

                /// <summary>
                /// Specifies the name of the daemon thread.
                /// </summary>
                /// <remarks>
                /// If not specified, the type name will be used. This
                /// property can be set at design time or runtime. If set at
                /// runtime, it must be configured before Start() is invoked
                /// to cause the daemon thread to have the specified name.
                /// </remarks>
                /// <value>
                /// The name of the daemon thread.
                /// </value>
                public override string ThreadName
                {
                    get
                    {
                        return
                                ((Initiator) Connection.ConnectionManager).
                                        ServiceName + ":" + base.ThreadName;
                    }
                    set { base.ThreadName = value; }
                }

                /// <summary>
                /// This event occurs when an exception is thrown from
                /// <b>OnEnter</b>, <b>OnWait</b>, <b>OnNotify</b> and
                /// <b>OnExit</b>.
                /// </summary>
                /// <param name="e">
                /// Exception that has occured.
                /// </param>
                protected override void OnException(Exception e)
                {
                    // see TcpConnection.CloseInternal()
                    if (!IsExiting)
                    {
                        try
                        {
                            TcpConnection connection = Connection;
                            connection.Close(/*notify*/ false,
                                    e, /*wait*/ false);
                        }
                        catch (Exception)
                        {
                        }
                    }

                    base.OnException(e);
                }

                /// <summary>
                /// Event notification to perform a regular daemon activity.
                /// </summary>
                /// <remarks>
                /// To get it called, another thread has to set
                /// <see cref="Daemon.IsNotification"/> to <b>true</b>:
                /// <c>daemon.IsNotification = true;</c>
                /// </remarks>
                protected override void OnNotify()
                {
                    const int MAX_RECEIVE_BUFFER_SIZE = 64*1024;

                    TcpConnection connection = Connection;
                    Debug.Assert(connection != null);

                    Stream stream = Connection.Stream;
                    Debug.Assert(stream != null);

                    var manager = (TcpInitiator) connection.ConnectionManager;
                    Debug.Assert(manager != null);

                    var lengthBuffer = new byte[5]; // maximum size of a packed 32-bit int
                    while (!IsExiting)
                    {
                        int bytesTotal = 0; // total bytes received
                        int bytesReceived;  // current bytes received

                        // read the next Message length data
                        do
                        {
                            try
                            {
                                bytesReceived = stream.Read(lengthBuffer,
                                                            bytesTotal,
                                                            lengthBuffer.Length -
                                                            bytesTotal);
                            }
                            catch (Exception e)
                            {
                                // see TcpConnection.CloseInternal()
                                if (!IsExiting)
                                {
                                    // I/O error: close the Connection
                                    try
                                    {
                                        connection.Close(
                                                /*notify*/ false,
                                                new ConnectionException(e, connection),
                                                /*wait*/ false);
                                    }
                                    catch
                                    {
                                    }
                                    Stop();
                                }
                                return;
                            }

                            if (bytesReceived == 0)
                            {
                                // see TcpConnection.CloseInternal()
                                if (!IsExiting)
                                {
                                    // EOF: close the Connection
                                    try
                                    {
                                        connection.Close(
                                                /*notify*/ false,
                                                null,
                                                /*wait*/ false);
                                    }
                                    catch
                                    {
                                    }
                                    Stop();
                                }
                                return;
                            }
                            bytesTotal += bytesReceived;
                        }
                        while (!PofHelper.ContainsPackedInt32(lengthBuffer, bytesTotal));

                        // parse the Message length and allocate a buffer large enough to hold the Message
                        var lengthReader  = new DataReader(new MemoryStream(lengthBuffer));
                        int length        = lengthReader.ReadPackedInt32();

                        manager.EnforceMaxIncomingMessageSize(length);
                        var messageBuffer = new byte[length];

                        // copy any Message data from the length buffer into the Message buffer
                        if ((bytesTotal -= (int) lengthReader.BaseStream.Position) > 0)
                        {
                            lengthReader.Read(messageBuffer, 0, bytesTotal);
                        }

                        // read the next Message
                        while (bytesTotal < length)
                        {
                            try
                            {
                                bytesReceived = stream.Read(messageBuffer,
                                        bytesTotal,
                                        Math.Min(length - bytesTotal, MAX_RECEIVE_BUFFER_SIZE));
                            }
                            catch (Exception e)
                            {
                                // see TcpConnection.CloseInternal()
                                if (!IsExiting)
                                {
                                    // I/O error: close the Connection
                                    try
                                    {
                                        connection.Close(
                                                /*notify*/ false,
                                                new ConnectionException(e, connection),
                                                /*wait*/ false);
                                    }
                                    catch
                                    {
                                    }
                                    Stop();
                                }
                                return;
                            }

                            if (bytesReceived == 0)
                            {
                                // see TcpConnection.CloseInternal()
                                if (!IsExiting)
                                {
                                    // EOF: close the Connection
                                    try
                                    {
                                        connection.Close(
                                                /*notify*/ false,
                                                null,
                                                /*wait*/ false);
                                    }
                                    catch
                                    {
                                    }
                                    Stop();
                                }
                                return;
                            }
                            bytesTotal += bytesReceived;
                        }

                        connection.StatsBytesReceived += bytesTotal;
                        connection.StatsReceived += 1;

                        // process the next Message
                        manager.Receive(
                                new DataReader(new MemoryStream(messageBuffer)),
                                connection);
                    }
                }

                /// <summary>
                /// Event notification called when the daemon's Thread is
                /// waiting for work.
                /// </summary>
                /// <seealso cref="Daemon.Run"/>
                protected override void OnWait()
                {
                    // all work is done in OnNotify()
                    return;
                }

                #endregion

                #region Data members

                #endregion
            }

            #endregion

            #region Data members

            /// <summary>
            /// Stream to read/write data.
            /// </summary>
            private Stream m_stream;

            #endregion
        }

        #endregion
    }
}
