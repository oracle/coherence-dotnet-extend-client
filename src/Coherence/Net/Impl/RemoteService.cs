/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;

using Tangosol.IO;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Daemon.QueueProcessor;
using Tangosol.Util.Daemon.QueueProcessor.Service;
using Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// Service implementation that allows a client to use a remote clustered
    /// Service without having to join the Cluster.
    /// </summary>
    /// <author>Ana Cikic  2006.09.14</author>
    public abstract class RemoteService : Extend, IService, IServiceInfo
    {
        #region Properties

        /// <summary>
        /// The <see cref="IChannel"/> used to exchange Messages with a
        /// remote Service Proxy.
        /// </summary>
        /// <value>
        /// The <see cref="IChannel"/> used to exchange Messages with a
        /// remote ProxyService.
        /// </value>
        public virtual IChannel Channel { get; protected set; }

        /// <summary>
        /// Gets the name of the <see cref="IService"/>.
        /// </summary>
        /// <value>
        /// The name of the <b>IService</b>.
        /// </value>
        public virtual string ServiceName { get; set; }

        /// <summary>
        /// Gets the type of the <see cref="IService"/>.
        /// </summary>
        /// <value>
        /// The type of the <b>IService</b>.
        /// </value>
        /// <since>Coherence 2.0</since>
        public abstract ServiceType ServiceType { get; }

        /// <summary>
        /// Gets the version of the <see cref="IService"/>.
        /// </summary>
        /// <value>
        /// The version of the <b>IService</b>.
        /// </value>
        public virtual string ServiceVersion { get; protected set; }

        /// <summary>
        /// Determine whether or not the controllable service is running.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// Returns <b>false</b> before a service is started, while the
        /// service is starting, while a service is shutting down and after
        /// the service has stopped. It only returns <b>true</b> after
        /// completing its start processing and before beginning its shutdown
        /// processing.
        /// </remarks>
        /// <returns>
        /// <b>true</b> if the service is running; <b>false</b> otherwise.
        /// </returns>
        public virtual bool IsRunning
        {
            get
            {
                IConnectionInitiator initiator = Initiator;
                return initiator == null ? false : initiator.IsRunning;
            }
        }

        /// <summary>
        /// The <see cref="IConnectionInitiator"/> used to connect to a
        /// ProxyService.
        /// </summary>
        /// <value>
        /// The <b>IConnectionInitiator</b> used to connect to a
        /// ProxyService.
        /// </value>
        public virtual IConnectionInitiator Initiator
        {
            get { return m_initiator; }
            //TODO: protected
            set
            {
                Debug.Assert(Initiator == null);
                m_initiator = value;
            }
        }

        /// <summary>
        /// Whether the remote AddressProvider addresses are to be used to 
        /// look up the remote address of the ProxyService.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the remote AddressProvider addresses are to be 
        /// used; <b>false</b> otherwise.
        /// </returns>
        /// <since>12.2.1</since>
        protected virtual bool IsNameServiceAddressProvider { get; set; }

        /// <summary>
        /// The <see cref="IOperationalContext"/> for this IService.
        /// </summary>
        /// <value>
        /// An <b>IOperationalContext</b> instance.
        /// </value>
        public virtual IOperationalContext OperationalContext
        {
            get
            {
                var ctx = m_operationalContext;
                if (ctx == null)
                {
                    m_operationalContext = ctx = new DefaultOperationalContext();
                }
                return ctx;
            }
            set
            {
                if (m_operationalContext != null)
                {
                    throw new InvalidOperationException(
                        "operational context already set");
                }
                m_operationalContext = value;
            }
        }

        /// <summary>
        /// The remote cluster name or null.
        /// </summary>
        /// <since>12.2.1</since>
        public virtual string RemoteClusterName
        {
            get
            {
                string name = m_remoteClusterName;
                if (StringUtils.IsNullOrEmpty(name))
                {
                    // NS lookups and corresponding redirects are always done with a cluster name since multiple
                    // clusters may effectivley share the cluster port we don't know what cluster we'd land in.
                    // remote-address based lookups on the other hand use the cluster name configured in the remote
                    // scheme, which is allowed to be null.  This is because a remote-address based lookup is pointing
                    // at an explict unsharable port and it is presumed the configuration is correct.    
                    return IsNameServiceAddressProvider
                        ? OperationalContext.LocalMember.ClusterName
                        : null;
                }

                return name;
            }

            set
            {
                m_remoteClusterName = value;
            }
        }

        /// <summary>
        /// The remote service name or null.
        /// </summary>
        /// <since>12.2.1</since>
        public virtual string RemoteServiceName
        { 
            get
            {
                string name = m_remoteServiceName;
                if (StringUtils.IsNullOrEmpty(name))
                {
                    return IsNameServiceAddressProvider
                        ? ServiceName // already scoped
                        : null;
                }
                String scopeName = ScopeName;
                return (StringUtils.IsNullOrEmpty(scopeName))
                        ? name
                        : scopeName + ':' + RemoteServiceName;
            }

            set
            {
                m_remoteServiceName = value;
            }
        }

        /// <summary>
        /// The scope name of the remote service, or null.
        /// </summary>
        /// <since>12.2.1</since>
        public virtual string ScopeName { get; set; }

        /// <summary>
        /// The XML element used to configure the initiator.
        /// </summary>
        public virtual IXmlElement Xml { get; set; }

        #endregion

        #region IService implementation

        /// <summary>
        /// Gets the <see cref="IServiceInfo"/> object for this
        /// <see cref="IService"/>.
        /// </summary>
        /// <value>
        /// The <b>IServiceInfo</b> object.
        /// </value>
        public virtual IServiceInfo Info
        {
            get { return this; }
        }

        /// <summary>
        /// Gets or sets the user context object associated with this
        /// <see cref="IService"/>.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// The data type and semantics of this context object are entirely
        /// application specific and are opaque to the <b>IService</b>
        /// itself.
        /// </remarks>
        /// <value>
        /// User context object associated with this <b>IService</b>.
        /// </value>
        /// <since>Coherence 3.0</since>
        public virtual object UserContext { get; set; }

        /// <summary>
        /// The <see cref="ISerializer"/> used to serialize and deserialize
        /// objects by this <b>IService</b>.
        /// </summary>
        /// <value>
        /// The <b>ISerializer</b> for this <b>IService</b>.
        /// </value>
        public virtual ISerializer Serializer
        {
            get
            {
                IConnection connection = EnsureChannel().Connection;
                if (connection != null)
                {
                    IConnectionManager manager = connection.ConnectionManager;
                    if (manager is Service)
                    {
                        return ((Service) manager).Serializer;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Invoked when an <see cref="IMember"/> has joined the service.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Note: this event could be called during the service restart on
        /// the local node in which case the listener's code should not
        /// attempt to use any clustered cache or service functionality.</p>
        /// <p>
        /// The most critical situation arises when a number of threads are
        /// waiting for a local service restart, being blocked by a
        /// <b>IService</b> object synchronization monitor. Since the Joined
        /// event should be fired only once, it is called on a client thread
        /// <b>while holding a synchronization monitor</b>. An attempt to use
        /// other clustered service functionality during this local event
        /// notification may result in a deadlock.</p>
        /// </remarks>
        public virtual event MemberEventHandler MemberJoined;

        /// <summary>
        /// Invoked when an <see cref="IMember"/> is leaving the service.
        /// </summary>
        public virtual event MemberEventHandler MemberLeaving;

        /// <summary>
        /// Invoked when an <see cref="IMember"/> has left the service.
        /// </summary>
        /// <remarks>
        /// Note: this event could be called during the service restart on
        /// the local node in which case the listener's code should not
        /// attempt to use any clustered cache or service functionality.
        /// </remarks>
        public virtual event MemberEventHandler MemberLeft;

        /// <summary>
        /// Invoked when <see cref="IService"/> is starting.
        /// </summary>
        public virtual event ServiceEventHandler ServiceStarting;

        /// <summary>
        /// Invoked when <see cref="IService"/> has started.
        /// </summary>
        public virtual event ServiceEventHandler ServiceStarted;

        /// <summary>
        /// Invoked when <see cref="IService"/> is stopping.
        /// </summary>
        public virtual event ServiceEventHandler ServiceStopping;

        /// <summary>
        /// Invoked when <see cref="IService"/> has stopped.
        /// </summary>
        public virtual event ServiceEventHandler ServiceStopped;

        #endregion

        #region IControllable implementation

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
        public virtual void Configure(IXmlElement xml)
        {
            lock (this)
            {
                Debug.Assert(!IsRunning);
                DoConfigure(xml);
            }
        }

        /// <summary>
        /// Start the controllable service.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// This method should only be called once per the life cycle
        /// of the IControllable service. This method has no affect if the
        /// service is already running.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a service does not support being re-started, and the
        /// service was already started and subsequently stopped and then
        /// an attempt is made to start the service again; also thrown if
        /// the IControllable service has not been configured.
        /// </exception>
        public virtual void Start()
        {
            lock (this)
            {
                if (!IsRunning)
                {
                    try
                    {
                        DoStart();
                    }
                    catch (Exception)
                    {
                        DoStop();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Stop the controllable service.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// This is a controlled shut-down, and is preferred to the
        /// <see cref="IControllable.Stop"/> method.
        /// <p/>
        /// This method should only be called once per the life cycle
        /// of the controllable service. Calling this method for a service
        /// that has already stopped has no effect.
        /// </remarks>
        public virtual void Shutdown()
        {
            DoShutdown();
        }

        /// <summary>
        /// Hard-stop the controllable service.
        /// </summary>
        /// <remarks>
        /// Use <see cref="IControllable.Shutdown"/> for normal service termination.
        /// Calling this method for a service that has already stopped has no
        /// effect.
        /// </remarks>
        public virtual void Stop()
        {
            DoStop();
        }

        #endregion

        #region Extend override methods

        /// <summary>
        /// Return a human-readable description of this class.
        /// </summary>
        /// <returns>
        /// A string representation of this class.
        /// </returns>
        /// <since>12.2.1.3</since>
        protected override string GetDescription()
        {
            return "Name=" + ServiceName;
        }

        #endregion

        #region Connection event handlers

        /// <summary>
        /// Invoked after an <see cref="IConnection"/> has been successfully
        /// established.
        /// </summary>
        /// <param name="sender">
        /// <see cref="IConnectionManager"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// The <see cref="ConnectionEventType.Opened"/> event.
        /// </param>
        public virtual void OnConnectionOpened(object sender, ConnectionEventArgs evt)
        {
            DispatchMemberEvent(MemberEventType.Joined);
        }

        /// <summary>
        /// Invoked after an <see cref="IConnection"/> is closed.
        /// </summary>
        /// <param name="sender">
        /// <see cref="IConnectionManager"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// The <see cref="ConnectionEventType.Closed"/> event.
        /// </param>
        public virtual void OnConnectionClosed(object sender, ConnectionEventArgs evt)
        {
            Channel = null;

            DispatchMemberEvent(MemberEventType.Leaving);
            DispatchMemberEvent(MemberEventType.Left);
        }

        /// <summary>
        /// Invoked when the <see cref="IConnection"/> detects that the
        /// underlying communication channel has been severed or become
        /// unusable.
        /// </summary>
        /// <remarks>
        /// After this event is raised, any attempt to use the
        /// <b>IConnection</b> (or any <see cref="IChannel"/> created by the
        /// <b>IConnection</b>) may result in an exception.
        /// </remarks>
        /// <param name="sender">
        /// <see cref="IConnectionManager"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// The <see cref="ConnectionEventType.Error"/> event.
        /// </param>
        public virtual void OnConnectionError(object sender, ConnectionEventArgs evt)
        {
            Channel = null;

            DispatchMemberEvent(MemberEventType.Leaving);
            DispatchMemberEvent(MemberEventType.Left);
        }

        #endregion.

        #region Service event handlers

        /// <summary>
        /// Invoked when an <see cref="IService"/> is starting.
        /// </summary>
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// A <see cref="ServiceEventType.Starting"/> event.
        /// </param>
        public virtual void OnServiceStarting(object sender, ServiceEventArgs evt)
        {
            DispatchServiceEvent(evt.EventType);
        }

        /// <summary>
        /// Invoked when an <see cref="IService"/> is starting.
        /// </summary>
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// A <see cref="ServiceEventType.Started"/> event.
        /// </param>
        public virtual void OnServiceStarted(object sender, ServiceEventArgs evt)
        {
            DispatchServiceEvent(evt.EventType);
        }

        /// <summary>
        /// Invoked when an <see cref="IService"/> is starting.
        /// </summary>
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// A <see cref="ServiceEventType.Stopping"/> event.
        /// </param>
        public virtual void OnServiceStopping(object sender, ServiceEventArgs evt)
        {
            DispatchServiceEvent(evt.EventType);
        }

        /// <summary>
        /// Invoked when an <see cref="IService"/> is starting.
        /// </summary>
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// A <see cref="ServiceEventType.Stopped"/> event.
        /// </param>
        public virtual void OnServiceStopped(object sender, ServiceEventArgs evt)
        {
            DispatchServiceEvent(evt.EventType);
            Channel = null;
        }

        #endregion

        #region Abstract methods

        /// <summary>
        /// Open an <b>IChannel</b> to the remote ProxyService.
        /// </summary>
        protected abstract IChannel OpenChannel();

        #endregion

        #region Internal methods

        /// <summary>
        /// The <see cref="Configure"/> implementation method.
        /// </summary>
        /// <remarks>
        /// This method must only be called by a thread that has synchronized
        /// on this RemoteService.
        /// </remarks>
        /// <param name="xml">
        /// The <see cref="IXmlElement"/> containing the new configuration
        /// for this RemoteService.
        /// </param>
        protected virtual void DoConfigure(IXmlElement xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml", "xml configuration must not be null");
            }

            Xml = xml;

            // find the configuration for the Initiator
            IXmlElement xmlInitiator = Tangosol.Util.Daemon.QueueProcessor.
                    Service.Peer.Initiator.Initiator.FindInitiatorConfig(xml);

            // inject service configuration
            IXmlElement xmlHandler = XmlHelper.EnsureElement(xmlInitiator, "incoming-message-handler");

            IXmlElement xmlSub = XmlHelper.EnsureElement(xmlHandler, "request-timeout");
            if (xmlSub.Value == null)
            {
                xmlSub.SetString(xml.GetSafeElement("request-timeout").GetString());
            }

            // create the Initiator
            IConnectionInitiator initiator = Tangosol.Util.Daemon.QueueProcessor.
                    Service.Peer.Initiator.Initiator.CreateInitiator(xmlInitiator, OperationalContext);

            if (initiator is Initiator)
            {
                var initiatorImpl = (Initiator) initiator;
                initiatorImpl.ServiceName = ServiceName + ':' + initiatorImpl.ServiceName;
                initiatorImpl.ParentService = this;
            }
            Initiator = initiator;

            RemoteClusterName = xml.GetSafeElement("cluster-name").GetString();
            RemoteServiceName = xml.GetSafeElement("proxy-service-name").GetString();
            ScopeName         = xml.GetSafeElement("scope-name").GetString();

            if (initiator is TcpInitiator)
            {
                IsNameServiceAddressProvider = ((TcpInitiator) initiator).IsNameService;
            }
        }

        /// <summary>
        /// The <see cref="Shutdown"/> implementation method.
        /// </summary>
        /// <remarks>
        /// This method must only be called by a thread that has synchronized
        /// on this RemoteService.
        /// </remarks>
        protected virtual void DoShutdown()
        {
            Initiator.Shutdown();
        }

        /// <summary>
        /// The <see cref="Start"/> implementation method.
        /// </summary>
        /// <remarks>
        /// This method must only be called by a thread that has synchronized
        /// on this RemoteService.
        /// </remarks>
        protected virtual void DoStart()
        {
            IConnectionInitiator initiator = Initiator;
            Debug.Assert(initiator != null);

            initiator.ConnectionOpened += new ConnectionEventHandler(OnConnectionOpened);
            initiator.ConnectionClosed += new ConnectionEventHandler(OnConnectionClosed);
            initiator.ConnectionError  += new ConnectionEventHandler(OnConnectionError);

            initiator.ServiceStarting += new ServiceEventHandler(OnServiceStarting);
            initiator.ServiceStarted  += new ServiceEventHandler(OnServiceStarted);
            initiator.ServiceStopping += new ServiceEventHandler(OnServiceStopping);
            initiator.ServiceStopped  += new ServiceEventHandler(OnServiceStopped);

            initiator.Start();

            EnsureChannel();
        }

        /// <summary>
        /// The <see cref="Stop"/> implementation method.
        /// </summary>
        /// <remarks>
        /// This method must only be called by a thread that has synchronized
        /// on this RemoteService.
        /// </remarks>
        protected virtual void DoStop()
        {
            Initiator.Stop();
        }

        /// <summary>
        /// Return <b>true</b> if the current thread is one of the service
        /// threads.
        /// </summary>
        /// <param name="isStrict">
        /// If <b>true</b> then only the service thread and event dispatcher
        /// thread are considered to be service threads, if <b>false</b>,
        /// then <b>DaemonPool</b> threads are also considered to be service
        /// threads.
        /// </param>
        public virtual bool IsServiceThread(bool isStrict)
        {
            IConnectionInitiator initiator = Initiator;
            if (initiator is Initiator)
            {
                return ((Initiator) initiator).IsServiceThread(isStrict);
            }
            return false;
        }

        /// <summary>
        /// Obtains the connect address of the ProxyService from a remote NameService.
        /// </summary>
        protected void LookupProxyServiceAddress()
        {
            if (IsNameServiceAddressProvider)
            {
                IConnectionInitiator initiator = Initiator;
                if (initiator is TcpInitiator)
                {
                    // attempt to lookup the ProxyService address from a NameService

                    TcpInitiator      tcpInitiator = (TcpInitiator) initiator;
                    RemoteNameService serviceNS    = new RemoteNameService();

                    serviceNS.OperationalContext = OperationalContext;
                    serviceNS.ServiceName        = ServiceName + ':' + ServiceType.RemoteNameService;
                    serviceNS.DoConfigure(Xml);

                    string clusterRemote = RemoteClusterName;
                    string serviceRemote = RemoteServiceName;

                    serviceNS.RemoteClusterName = clusterRemote;
                    serviceNS.RemoteServiceName = "NameService";

                    Exception e = null;
                    try
                    {
                        tcpInitiator.CloseOnExit.Add(serviceNS);
                        serviceNS.Start();

                        Object[] ao = (Object[]) serviceNS.Lookup(serviceRemote);
                        if (ao == null)
                        {
                            // we got an answer, which means we found the cluster, but not the service
                            e = new ConnectionException("Unable to locate ProxyService '" + serviceRemote
                                + "' within cluster '" + clusterRemote + "'");
                        }
                        else
                        {
                            Port32 port32 = new Port32((Int32) ao[1]);

                            tcpInitiator.RemoteAddressProvider = new SingleAddressProvider(
                                    new IPEndPoint(NetworkUtils.GetHostAddress((String) ao[0], tcpInitiator.ConnectTimeout), port32.Baseport));
                            tcpInitiator.Subport       = port32.Subport;
                            tcpInitiator.IsNameService = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        // we failed to connect, thus the cluster was not reachable
                        e = new ConnectionException("Unable to locate cluster '" + clusterRemote + "' while looking for its ProxyService '"
                                + serviceRemote + "'", ex);
                    }
                    finally
                    {
                        tcpInitiator.CloseOnExit.Remove(serviceNS);
                        serviceNS.Stop();
                    }
                    if (e != null)
                    {
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// Return the <see cref="IChannel"/> used by this RemoteService.
        /// </summary>
        /// <remarks>
        /// If the <b>IChannel</b> is <c>null</c> or is not open, a new
        /// <b>Channel</b> is opened.
        /// </remarks>
        /// <returns>
        /// An <b>IChannel</b> that can be used to exchange Messages with a
        /// remote ProxyService.
        /// </returns>
        protected virtual IChannel EnsureChannel()
        {
            lock (this)
            {
                IChannel channel = Channel;
                if (channel == null || !channel.IsOpen)
                {
                    Channel = (channel = OpenChannel());
                }

                return channel;
            }
        }

        /// <summary>
        /// Return a running <see cref="QueueProcessor"/> used to dispatch
        /// events to registered listeners.
        /// </summary>
        /// <returns>
        /// A running <b>QueueProcessor</b>.
        /// </returns>
        protected virtual QueueProcessor EnsureEventDispatcher()
        {
            Channel channel = (Channel) EnsureChannel();
            return channel.ConnectionManager.EnsureEventDispatcher();
        }

        /// <summary>
        /// Block the calling thread until the EventDispatcher Queue is empty
        /// and all outstanding tasks have been executed.
        /// </summary>
        public virtual void DrainEvents()
        {
            IConnectionInitiator initiator = Initiator;
            if (initiator is Initiator)
            {
                ((Initiator) initiator).DrainEvents();
            }
        }

        /// <summary>
        /// Create and dispatch a new local <see cref="MemberEventArgs"/>
        /// with the given type to the registered event handlers.
        /// </summary>
        /// <param name="eventType">
        /// The type of <b>MemberEventArgs</b> to create and dispatch.
        /// </param>
        protected virtual void DispatchMemberEvent(MemberEventType eventType)
        {
            MemberEventArgs evt = new MemberEventArgs(this, eventType, OperationalContext.LocalMember);
            switch (evt.EventType)
            {
                case MemberEventType.Joined:
                    InvokeMemberEvent(MemberJoined, evt);
                    break;

                case MemberEventType.Leaving:
                    InvokeMemberEvent(MemberLeaving, evt);
                    break;

                case MemberEventType.Left:
                    InvokeMemberEvent(MemberLeft, evt);
                    break;
            }
        }

        /// <summary>
        /// Create and dispatch a new local <see cref="ServiceEventArgs"/>
        /// with the given type to the registered event handlers.
        /// </summary>
        /// <param name="eventType">
        /// The type of <b>ServiceEventArgs</b> to create and dispatch.
        /// </param>
        protected virtual void DispatchServiceEvent(ServiceEventType eventType)
        {
            ServiceEventArgs evt = new ServiceEventArgs(this, eventType);
            switch (evt.EventType)
            {
                case ServiceEventType.Starting:
                    InvokeServiceEvent(ServiceStarting, evt);
                    break;

                case ServiceEventType.Started:
                    InvokeServiceEvent(ServiceStarted, evt);
                    break;

                case ServiceEventType.Stopping:
                    InvokeServiceEvent(ServiceStopping, evt);
                    break;

                case ServiceEventType.Stopped:
                    InvokeServiceEvent(ServiceStopped, evt);
                    break;
            }
        }

        /// <summary>
        /// Invokes the event, with special remark towards multithreading
        /// (using local copy of delegate and no inline attribute for method).
        /// </summary>
        /// <param name="handler">
        /// The MemberEventHandler event that's being invoked.
        /// </param>
        /// <param name="evt">
        /// Event arguments.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InvokeMemberEvent(MemberEventHandler handler, MemberEventArgs evt)
        {
            if (handler != null)
            {
                handler(this, evt);
            }
        }

        /// <summary>
        /// Invokes the event, with special remark towards multithreading
        /// (using local copy of delegate and no inline attribute for method).
        /// </summary>
        /// <param name="handler">
        /// The ServiceEventHandler event that's being invoked.
        /// </param>
        /// <param name="evt">
        /// Event arguments.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InvokeServiceEvent(ServiceEventHandler handler, ServiceEventArgs evt)
        {
            if (handler != null)
            {
                handler(this, evt);
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The IConnectionInitiator used to connect to a ProxyService.
        /// </summary>
        private volatile IConnectionInitiator m_initiator;

        /// <summary>
        /// The remote cluster name or null.
        /// </summary>
        /// <since>12.2.1</since>
        private string m_remoteClusterName;

        /// <summary>
        /// The remote service name or null.
        /// </summary>
        /// <since>12.2.1</since>
        private string m_remoteServiceName;

        /// <summary>
        /// The IOperationalContext for this IService.
        /// </summary>
        private IOperationalContext m_operationalContext;

        #endregion
    }
}