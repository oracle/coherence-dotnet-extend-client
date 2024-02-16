/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading;

using Tangosol.IO;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Daemon.QueueProcessor.Service;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// Base class for remote services safe-wrapping.
    /// </summary>
    /// <remarks>
    /// These "Safe" wrappers are responsible for ensuring that the wrapped
    /// component is always usable. For example, if the connection between
    /// the Coherence*Extend client and proxy service is ever severed, the
    /// messaging framework alerts the "Remote" service (via a
    /// ConnectionListener), and the "Remote" service transitions itself to
    /// a stopped state. During the next use of the "Remote" service (via the
    /// "Safe" wrapper), the "Safe" wrapper detects that the wrapped service
    /// has stopped and attempts to restart it.
    /// </remarks>
    public class SafeService : IService
    {
        #region Properties

        /// <summary>
        /// The configuration data.
        /// </summary>
        /// <value>
        /// The <see cref="IXmlElement"/> holding configuration data.
        /// </value>
        public virtual IXmlElement Config
        {
            get { return m_config; }
            set { m_config = value; }
        }

        /// <summary>
        /// The <see cref="IOperationalContext"/> for this IService
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
        /// Calculated property that returns the running wrapped
        /// <b>IService</b>.
        /// </summary>
        /// <value>
        /// The wrapped <b>IService</b>.
        /// </value>
        public virtual Util.IService RunningService
        {
            get { return EnsureRunningService(true); }
        }

        /// <summary>
        /// The state of the SafeService.
        /// </summary>
        /// <value>
        /// One of the <see cref="ServiceState"/> enum values.
        /// </value>
        public virtual ServiceState SafeServiceState
        {
            get { return m_safeServiceState; }
            set { m_safeServiceState = value; }
        }

        /// <summary>
        /// The actual (wrapped) <see cref="Util.IService"/>.
        /// </summary>
        /// <value>
        /// Wrapped <b>IService</b>.
        /// </value>
        public virtual Util.IService Service
        {
            get { return m_service; }
            set { m_service = value; }
        }

        /// <summary>
        /// Service name.
        /// </summary>
        /// <value>
        /// Service name.
        /// </value>
        public virtual string ServiceName
        {
            get { return m_serviceName; }
            set { m_serviceName = value; }
        }

        /// <summary>
        /// Service type.
        /// </summary>
        /// <value>
        /// Service type.
        /// </value>
        public virtual ServiceType ServiceType
        {
            get { return m_serviceType; }
            set { m_serviceType = value; }
        }

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
                Util.IService service = Service;
                return service != null && Service.IsRunning;
            }
        }

        /// <summary>
        /// The optional <b>IPrincipal</b> object associated with this
        /// service.
        /// </summary>
        /// <remarks>
        /// If an <b>IPrincipal</b> is associated with this service,
        /// RestartService will be done on behalf of this <b>IPrincipal</b>.
        /// </remarks>
        /// <value>
        /// The <b>IPrincipal</b> associated with this service.
        /// </value>
        public virtual IPrincipal Principal
        {
            get { return m_principal; }
            set { m_principal = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SafeService()
        {
            SafeServiceState = ServiceState.Initial;
        }

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
            get { return ((IService) RunningService).Info; }
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
        public virtual object UserContext
        {
            get { return m_userContext; }
            set
            {
                m_userContext = value;

                IService service = (IService) Service;
                if (service != null)
                {
                    service.UserContext = value;
                }
            }
        }

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
                return ((IService) RunningService).Serializer;
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
        public virtual event MemberEventHandler MemberJoined
        {
            add
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    bool wasEmpty = m_memberJoined == null;
                    m_memberJoined += value;

                    if (wasEmpty && m_memberJoined != null)
                    {
                        IService service = (IService) Service;
                        if (service != null && service.IsRunning)
                        {
                            service.MemberJoined += new MemberEventHandler(OnMemberJoined);
                        }
                    }
                }
            }

            remove
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    if (m_memberJoined != null)
                    {
                        m_memberJoined -= value;

                        if (m_memberJoined == null)
                        {
                            IService service = (IService) Service;
                            if (service != null && service.IsRunning)
                            {
                                service.MemberJoined -= new MemberEventHandler(OnMemberJoined);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when an <see cref="IMember"/> is leaving the service.
        /// </summary>
        public virtual event MemberEventHandler MemberLeaving
        {
            add
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    bool wasEmpty = m_memberLeaving == null;
                    m_memberLeaving += value;

                    if (wasEmpty && m_memberLeaving != null)
                    {
                        IService service = (IService) Service;
                        if (service != null && service.IsRunning)
                        {
                            service.MemberLeaving += new MemberEventHandler(OnMemberLeaving);
                        }
                    }
                }
            }

            remove
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    if (m_memberLeaving != null)
                    {
                        m_memberLeaving -= value;

                        if (m_memberLeaving == null)
                        {
                            IService service = (IService) Service;
                            if (service != null && service.IsRunning)
                            {
                                service.MemberLeaving -= new MemberEventHandler(OnMemberLeaving);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when an <see cref="IMember"/> has left the service.
        /// </summary>
        /// <remarks>
        /// Note: this event could be called during the service restart on
        /// the local node in which case the listener's code should not
        /// attempt to use any clustered cache or service functionality.
        /// </remarks>
        public virtual event MemberEventHandler MemberLeft
        {
            add
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    bool wasEmpty = m_memberLeft == null;
                    m_memberLeft += value;

                    if (wasEmpty && m_memberLeft != null)
                    {
                        IService service = (IService) Service;
                        if (service != null && service.IsRunning)
                        {
                            service.MemberLeft += new MemberEventHandler(OnMemberLeft);
                        }
                    }
                }
            }

            remove
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    if (m_memberLeft != null)
                    {
                        m_memberLeft -= value;

                        if (m_memberLeft == null)
                        {
                            IService service = (IService) Service;
                            if (service != null && service.IsRunning)
                            {
                                service.MemberLeft -= new MemberEventHandler(OnMemberLeft);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when <see cref="IService"/> is starting.
        /// </summary>
        public virtual event ServiceEventHandler ServiceStarting
        {
            add
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    bool wasEmpty = m_serviceStarting == null;
                    m_serviceStarting += value;

                    if (wasEmpty && m_serviceStarting != null)
                    {
                        Util.IService service = Service;
                        if (service != null && service.IsRunning)
                        {
                            service.ServiceStarting += new ServiceEventHandler(OnServiceStarting);
                        }
                    }
                }
            }

            remove
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    if (m_serviceStarting != null)
                    {
                        m_serviceStarting -= value;

                        if (m_serviceStarting == null)
                        {
                            Util.IService service = Service;
                            if (service != null && service.IsRunning)
                            {
                                service.ServiceStarting -= new ServiceEventHandler(OnServiceStarting);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when <see cref="IService"/> has started.
        /// </summary>
        public virtual event ServiceEventHandler ServiceStarted
        {
            add
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    bool wasEmpty = m_serviceStarted == null;
                    m_serviceStarted += value;

                    if (wasEmpty && m_serviceStarted != null)
                    {
                        Util.IService service = Service;
                        if (service != null && service.IsRunning)
                        {
                            service.ServiceStarted += new ServiceEventHandler(OnServiceStarted);
                        }
                    }
                }
            }

            remove
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    if (m_serviceStarted != null)
                    {
                        m_serviceStarted -= value;

                        if (m_serviceStarted == null)
                        {
                            Util.IService service = Service;
                            if (service != null && service.IsRunning)
                            {
                                service.ServiceStarted -= new ServiceEventHandler(OnServiceStarted);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when <see cref="IService"/> is stopping.
        /// </summary>
        public virtual event ServiceEventHandler ServiceStopping
        {
            add
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    bool wasEmpty = m_serviceStopping == null;
                    m_serviceStopping += value;

                    if (wasEmpty && m_serviceStopping != null)
                    {
                        Util.IService service = Service;
                        if (service != null && service.IsRunning)
                        {
                            service.ServiceStopping += new ServiceEventHandler(OnServiceStopping);
                        }
                    }
                }
            }

            remove
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    if (m_serviceStopping != null)
                    {
                        m_serviceStopping -= value;

                        if (m_serviceStopping == null)
                        {
                            Util.IService service = Service;
                            if (service != null && service.IsRunning)
                            {
                                service.ServiceStopping -= new ServiceEventHandler(OnServiceStopping);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when <see cref="IService"/> has stopped.
        /// </summary>
        public virtual event ServiceEventHandler ServiceStopped
        {
            add
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    bool wasEmpty = m_serviceStopped == null;
                    m_serviceStopped += value;

                    if (wasEmpty && m_serviceStopped != null)
                    {
                        Util.IService service = Service;
                        if (service != null && service.IsRunning)
                        {
                            service.ServiceStopped += new ServiceEventHandler(OnServiceStopped);
                        }
                    }
                }
            }

            remove
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    if (m_serviceStopped != null)
                    {
                        m_serviceStopped -= value;

                        if (m_serviceStopped == null)
                        {
                            Util.IService service = Service;
                            if (service != null && service.IsRunning)
                            {
                                service.ServiceStopped -= new ServiceEventHandler(OnServiceStopped);
                            }
                        }
                    }
                }
            }
        }

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
            Config = xml;
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
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                if (SafeServiceState == ServiceState.Stopped)
                {
                    // allow restart after explicit stop
                    SafeServiceState = ServiceState.Initial;
                }

                try
                {
                    EnsureRunningService(false);
                }
                finally
                {
                    // ServiceState.Started indicates that "start" was called
                    SafeServiceState = ServiceState.Started;
                }
            }

            DrainEvents();
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
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                if (SafeServiceState != ServiceState.Stopped)
                {
                    Util.IService service = Service;
                    if (service != null)
                    {
                        service.Shutdown();
                    }
                    Cleanup();
                    SafeServiceState = ServiceState.Stopped;
                }
            }
        }

        /// <summary>
        /// Hard-stop the controllable service.
        /// </summary>
        /// <remarks>
        /// Use <see cref="IControllable.Shutdown"/> for normal service
        /// termination. Calling this method for a service that has already
        /// stopped has no effect.
        /// </remarks>
        public virtual void Stop()
        {
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                if (SafeServiceState != ServiceState.Stopped)
                {
                    Util.IService service = Service;
                    if (service != null)
                    {
                        service.Stop();
                    }
                    Cleanup();
                    SafeServiceState = ServiceState.Stopped;
                }
            }
        }

        #endregion

        #region Member event handlers

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
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// An event which indicates that membership has changed.
        /// </param>
        public virtual void OnMemberJoined(object sender, MemberEventArgs evt)
        {
            TranslateEvent(evt);
        }

        /// <summary>
        /// Invoked when an <see cref="IMember"/> is leaving the service.
        /// </summary>
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// An event which indicates that membership has changed.
        /// </param>
        public virtual void OnMemberLeaving(object sender, MemberEventArgs evt)
        {
            TranslateEvent(evt);
        }

        /// <summary>
        /// Invoked when an <see cref="IMember"/> has left the service.
        /// </summary>
        /// <remarks>
        /// Note: this event could be called during the service restart on
        /// the local node in which case the listener's code should not
        /// attempt to use any clustered cache or service functionality.
        /// </remarks>
        /// <param name="sender">
        /// <see cref="IService"/> that raised an event.
        /// </param>
        /// <param name="evt">
        /// An event which indicates that membership has changed.
        /// </param>
        public virtual void OnMemberLeft(object sender, MemberEventArgs evt)
        {
            TranslateEvent(evt);
        }

        #endregion

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
            TranslateEvent(evt);
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
            TranslateEvent(evt);
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
            TranslateEvent(evt);
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
            TranslateEvent(evt);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Return the wrapped <b>IService</b>.
        /// </summary>
        /// <remarks>
        /// This method ensures that the returned <b>IService</b> is running
        /// before returning it. If the <b>IService</b> is not running and has
        /// not been explicitly stopped, the <b>IService</b> is restarted.
        /// </remarks>
        /// <param name="drain">
        /// If true and the wrapped <b>IService</b> is restarted, the calling
        /// thread will be blocked until the wrapped <b>IService</b> event
        /// dispatcher queue is empty and all outstanding tasks have been
        /// executed.
        /// </param>
        /// <returns>
        /// The running wrapped <b>IService</b>.
        /// </returns>
        public virtual Util.IService EnsureRunningService(bool drain)
        {
            Util.IService service = Service;
            if (service == null || !service.IsRunning)
            {
                // to prevent a deadlock during restart we need to obtain the lock
                // before restarting the service (see problem COH-77)
                using (BlockingLock l = BlockingLock.Lock(typeof(CacheFactory)))
                {
                    using (BlockingLock l2 = BlockingLock.Lock(this))
                    {
                        service = Service;
                        switch (SafeServiceState)
                        {
                            case ServiceState.Initial:
                                if (service == null)
                                {
                                    Service = service = RestartService();
                                }
                                else
                                {
                                    StartService(service);
                                }
                                break;

                            case ServiceState.Started:
                                if (service == null || !service.IsRunning)
                                {
                                    Service = null; // release memory before restarting

                                    // restart the actual service
                                    CacheFactory.Log("Restarting Service: " + ServiceName,
                                                     CacheFactory.LogLevel.Info);

                                    Service = service = RestartService();
                                }
                                break;

                            case ServiceState.Stopped:
                                throw new InvalidOperationException("SafeService was explicitly stopped");
                        }
                    }
                }

                if (drain)
                {
                    DrainEvents();
                }
            }

            return service;
        }

        /// <summary>
        /// Block the calling thread until the wrapped <b>IService</b> event
        /// dispatcher queue is empty and all outstanding tasks have been
        /// executed.
        /// </summary>
        public virtual void DrainEvents()
        {
            Util.IService service = Service;
            if (service is Service)
            {
                ((Service) service).DrainEvents();
            }
            else if (service is RemoteService)
            {
                ((RemoteService) service).DrainEvents();
            }
        }

        /// <summary>
        /// Cleanup used resources.
        /// </summary>
        protected virtual void Cleanup()
        {
            Service = null;

            m_memberJoined  = null;
            m_memberLeaving = null;
            m_memberLeft    = null;

            m_serviceStarting = null;
            m_serviceStarted  = null;
            m_serviceStopping = null;
            m_serviceStopped  = null;
        }

        /// <summary>
        /// Restart <see cref="IService"/>.
        /// </summary>
        /// <returns>
        /// Running <b>IService</b> object.
        /// </returns>
        protected virtual Util.IService RestartService()
        {
            IService   service;
            IPrincipal currentPrincipal = Thread.CurrentPrincipal;

            try
            {
                switch (ServiceType)
                {
                    case ServiceType.RemoteCache:
                        service = new RemoteCacheService()
                                      {
                                          ServiceName        = ServiceName,
                                          OperationalContext = OperationalContext
                                      };
                        break;

                    case ServiceType.RemoteInvocation:
                        service = new RemoteInvocationService()
                                      {
                                          ServiceName        = ServiceName,
                                          OperationalContext = OperationalContext
                                      };
                        break;

                    default:
                        throw new InvalidOperationException("invalid service type");
                }
                // In case the service is scoped by Principal, use the
                // original Principal
                Thread.CurrentPrincipal = Principal;
                StartService(service);
            }
            finally
            {
                Thread.CurrentPrincipal = currentPrincipal;
            }
            return service;
        }

        /// <summary>
        /// Start the <see cref="IService"/>.
        /// </summary>
        /// <param name="service">
        /// The <b>IService</b> object to start.
        /// </param>
        protected virtual void StartService(Util.IService service)
        {
            service.Configure(Config);

            if (service is IService)
            {
                IService _service = (IService) service;
                _service.UserContext = UserContext;

                if (m_memberJoined != null)
                {
                    _service.MemberJoined += new MemberEventHandler(OnMemberJoined);
                }
                if (m_memberLeaving != null)
                {
                    _service.MemberLeaving += new MemberEventHandler(OnMemberLeaving);
                }
                if (m_memberLeft != null)
                {
                    _service.MemberLeft += new MemberEventHandler(OnMemberLeft);
                }
            }

            if (m_serviceStarting != null)
            {
                service.ServiceStarting += new ServiceEventHandler(OnServiceStarting);
            }
            if (m_serviceStarted != null)
            {
                service.ServiceStarted += new ServiceEventHandler(OnServiceStarted);
            }
            if (m_serviceStopping != null)
            {
                service.ServiceStopping += new ServiceEventHandler(OnServiceStopping);
            }
            if (m_serviceStopped != null)
            {
                service.ServiceStopped += new ServiceEventHandler(OnServiceStopped);
            }

            try
            {
                service.Start();
            }
            catch (Exception e)
            {
                CacheFactory.Log("Error while starting service \""
                                 + ServiceName + "\": " + e,
                                 CacheFactory.LogLevel.Error);
                try
                {
                    service.Stop();
                }
                catch (Exception e2)
                {
                    CacheFactory.Log("Failed to stop service \""
                                     + ServiceName + "\": " + e2,
                                     CacheFactory.LogLevel.Warn);
                    // eat the exception
                }

                throw;
            }
        }

        /// <summary>
        /// Translate the specified member event into another
        /// <see cref="MemberEventArgs"/> with a source set to this
        /// service.
        /// </summary>
        /// <param name="evt">
        /// The <b>MemberEventArgs</b>.
        /// </param>
        protected virtual void TranslateEvent(MemberEventArgs evt)
        {
            IService service = (IService) Service;

            if (service == null)
            {
                // for the JOIN events, the Service property may not be set
                // until after the "real" service is started
                // (see synchronized block at the EnsureRunningService method)
                // just wait till we are out of there
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    service = (IService)Service;
                }
            }

            IService serviceSource = evt.Service;

            // allow for post-mortem events
            MemberEventArgs evtSafe = new MemberEventArgs(
                    service == serviceSource ? this : serviceSource,
                    evt.EventType,
                    evt.Member);
            DispatchMemberEvent(evtSafe);
        }

        /// <summary>
        /// Translate the specified service event into another
        /// <see cref="ServiceEventArgs"/> with a source set to this
        /// service.
        /// </summary>
        /// <param name="evt">
        /// The <b>ServiceEventArgs</b>.
        /// </param>
        protected virtual void TranslateEvent(ServiceEventArgs evt)
        {
            Util.IService service = Service;

            if (service == null)
            {
                // for the JOIN events, the Service property may not be set
                // until after the "real" service is started
                // (see synchronized block at the EnsureRunningService method)
                // just wait till we are out of there
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    service = Service;
                }
            }

            Util.IService serviceSource = evt.Service;

            // allow for post-mortem events
            ServiceEventArgs evtSafe = new ServiceEventArgs(
                    service == serviceSource? this : serviceSource,
                    evt.EventType);
            DispatchServiceEvent(evtSafe);
        }

        /// <summary>
        /// Dispatch a <see cref="MemberEventArgs"/> to the registered
        /// event handlers.
        /// </summary>
        /// <param name="evt">
        /// <b>MemberEventArgs</b> to dispatch.
        /// </param>
        protected virtual void DispatchMemberEvent(MemberEventArgs evt)
        {
            switch (evt.EventType)
            {
                case MemberEventType.Joined:
                    InvokeMemberEvent(m_memberJoined, evt);
                    break;

                case MemberEventType.Leaving:
                    InvokeMemberEvent(m_memberLeaving, evt);
                    break;

                case MemberEventType.Left:
                    InvokeMemberEvent(m_memberLeft, evt);
                    break;
            }
        }

        /// <summary>
        /// Dispatch a <see cref="ServiceEventArgs"/> to the registered
        /// event handlers.
        /// </summary>
        /// <param name="evt">
        /// <b>ServiceEventArgs</b> to dispatch.
        /// </param>
        protected virtual void DispatchServiceEvent(ServiceEventArgs evt)
        {
            switch (evt.EventType)
            {
                case ServiceEventType.Starting:
                    InvokeServiceEvent(m_serviceStarting, evt);
                    break;

                case ServiceEventType.Started:
                    InvokeServiceEvent(m_serviceStarted, evt);
                    break;

                case ServiceEventType.Stopping:
                    InvokeServiceEvent(m_serviceStopping, evt);
                    break;

                case ServiceEventType.Stopped:
                    InvokeServiceEvent(m_serviceStopped, evt);
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

        #region Object override methods

        /// <summary>
        /// Provide a human-readable representation of this SafeService.
        /// </summary>
        /// <returns>
        /// A human-readable representation of this SafeService.
        /// </returns>
        public override string ToString()
        {
            Util.IService service = Service;
            return GetType().Name + ": " + (service == null ? "STOPPED" : service.ToString());
        }

        #endregion

        #region Data members

        /// <summary>
        /// The configuration data.
        /// </summary>
        [NonSerialized]
        private IXmlElement m_config;

        /// <summary>
        /// The IOperationalContext for this IService.
        /// </summary>
        private IOperationalContext m_operationalContext;

        /// <summary>
        /// The state of the SafeService; one of the ServiceState enum
        /// values.
        /// </summary>
        private ServiceState m_safeServiceState;

        /// <summary>
        /// The actual (wrapped) IService.
        /// </summary>
        [NonSerialized]
        private Util.IService m_service;

        /// <summary>
        /// Service name.
        /// </summary>
        private string m_serviceName;

        /// <summary>
        /// Service type.
        /// </summary>
        private ServiceType m_serviceType;

        /// <summary>
        /// User context object associated with this IService.
        /// </summary>
        private object m_userContext;

        ///
        /// MemberEvent handlers.
        ///
        private MemberEventHandler m_memberJoined;
        private MemberEventHandler m_memberLeaving;
        private MemberEventHandler m_memberLeft;

        /// <summary>
        /// ServiceEvent handlers.
        /// </summary>
        private ServiceEventHandler m_serviceStarting;
        private ServiceEventHandler m_serviceStarted;
        private ServiceEventHandler m_serviceStopping;
        private ServiceEventHandler m_serviceStopped;

        /// <summary>
        /// The <b>IPrincipal</b> associated with the service.
        /// </summary>
        private IPrincipal m_principal;

        #endregion
    }

    #region Enum: ServiceState

    /// <summary>
    /// Service state enum.
    /// </summary>
    public enum ServiceState
    {
        /// <summary>
        /// The SafeService has been created but has not been started yet.
        /// </summary>
        Initial = 0,

        /// <summary>
        /// The SafeService has been started.
        /// </summary>
        Started = 1,

        /// <summary>
        /// The SafeService has beed explicitely stopped.
        /// </summary>
        Stopped = 2
    }

    #endregion
}
