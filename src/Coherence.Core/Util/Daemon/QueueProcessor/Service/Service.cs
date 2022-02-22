/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Net.Messaging;
using Tangosol.Run.Xml;
using Tangosol.Util.Logging;

namespace Tangosol.Util.Daemon.QueueProcessor.Service
{
    /// <summary>
    /// Base implementation of <see cref="IService"/>.
    /// </summary>
    /// <remarks>
    /// A Service has a service thread, an optional execute thread pool, and
    /// an event dispatcher thread.
    /// </remarks>
    /// <author>Ana Cikic  2007.12.21</author>
    public abstract class Service : QueueProcessor, IService
    {
        /// <summary>
        /// Represents the method that will raise event.
        /// </summary>
        /// <param name="evt">
        /// <see cref="EventArgs"/> object.
        /// </param>
        public delegate void EventCallback(EventArgs evt);

        #region Properties

        /// <summary>
        /// (Calculated) Name of the service thread decoraded with any
        /// additional information that could be useful for thread dump
        /// analysis.
        /// </summary>
        /// <remarks>
        /// The decorated part is always trailing the full name delimited by
        /// the '|' character and is truncated by the <see cref="Logger"/>.
        /// </remarks>
        /// <value>
        /// Name of the service thread decorated with additional information.
        /// </value>
        public virtual string DecoratedThreadName
        {
            get
            {
                ServiceState state  = ServiceState;
                string       sState = state == ServiceState.Started ? "" :
                                      Logger.THREAD_NAME_DELIM + state.ToString();
                return ThreadName + sState;
            }
        }

        /// <summary>
        /// Human-readable description of additional Service properties.
        /// </summary>
        /// <remarks>
        /// Used by <see cref="ToString"/>.
        /// </remarks>
        public virtual string Description
        {
            get { return null; }
        }

        /// <summary>
        /// The <see cref="EventDispatcher"/>.
        /// </summary>
        /// <remarks>
        /// Called on the service thread only.
        /// </remarks>
        /// <value>
        /// The <b>EventDispatcher</b>.
        /// </value>
        public virtual EventDispatcher Dispatcher
        {
            get { return m_eventDispatcher; }
            //TODO: private
            set { m_eventDispatcher = value; }
        }

        /// <summary>
        /// Configured <see cref="ISerializer"/> instance.
        /// </summary>
        /// <value>
        /// Configured <b>ISerializer</b> instance.
        /// </value>
        public virtual ISerializer Serializer
        {
            get { return m_serializer; }
            //TODO: protected
            set { m_serializer = value; }
        }

        /// <summary>
        /// The <see cref="ISerializerFactory"/> used by this Service.
        /// </summary>
        /// <value>
        /// The <b>ISerializerFactory</b> used by this Service.
        /// </value>
        public virtual ISerializerFactory SerializerFactory
        {
            get { return m_serializerFactory; }
            //TODO: protected
            set { m_serializerFactory = value; }
        }

        /// <summary>
        /// Original XML configuration that was supplied to the Service; may
        /// be <c>null</c>.
        /// </summary>
        /// <value>
        /// <see cref="IXmlElement"/> containing configuration.
        /// </value>
        public virtual IXmlElement ServiceConfig
        {
            get { return m_serviceConfig; }
            set
            {
                if (ServiceState != ServiceState.Initial)
                {
                    throw new InvalidOperationException(
                        "Configuration cannot be specified once the service has been started: " + this);
                }

                m_serviceConfig = value;
            }
        }

        /// <summary>
        /// The name of this Service.
        /// </summary>
        /// <value>
        /// The name of the Service.
        /// </value>
        public virtual string ServiceName
        {
            get
            {
                string name = m_serviceName;
                return name == null ? GetType().Name : name;
            }
            set
            {
                Debug.Assert(!IsStarted);
                m_serviceName = value;
            }
        }

        /// <summary>
        /// The state of the Service.
        /// </summary>
        /// <value>
        /// One of the <see cref="ServiceState"/> enum values.
        /// </value>
        public virtual ServiceState ServiceState
        {
            get { return m_serviceState; }
            set
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    ServiceState prevState = ServiceState;
                    if (value > prevState)
                    {
                        m_serviceState = value;

                        OnServiceState(value);
                        switch (value)
                        {
                            case ServiceState.Starting:
                                DispatchServiceEvent(ServiceEventType.Starting);
                                break;

                            case ServiceState.Started:
                                DispatchServiceEvent(ServiceEventType.Started);
                                break;

                            case ServiceState.Stopping:
                                DispatchServiceEvent(ServiceEventType.Stopping);
                                break;

                            case ServiceState.Stopped:
                                DispatchServiceEvent(ServiceEventType.Stopped);
                                break;

                            default:
                                Debug.Assert(false);
                                break;
                        }

                        UpdateServiceThreadName();
                    }
                    else
                    {
                        Debug.Assert(value == prevState);
                    }

                    Monitor.PulseAll(this);
                }
            }
        }

        /// <summary>
        /// Calculated helper property; returns a human-readible description
        /// of the <see cref="ServiceState"/> property.
        /// </summary>
        /// <value>
        /// String representation of <b>ServiceState</b> value.
        /// </value>
        public virtual string ServiceStateName
        {
            get { return ServiceState.ToString(); }
        }

        /// <summary>
        /// Statistics: total time spent while processing messages.
        /// </summary>
        /// <value>
        /// Total time spent while processing messages.
        /// </value>
        public virtual long StatsCpu
        {
            get { return m_statsCpu; }
            //TODO: protected
            set { m_statsCpu = value; }
        }

        /// <summary>
        /// Statistics: total number of received messages.
        /// </summary>
        /// <value>
        /// Total number of received messages.
        /// </value>
        public virtual long StatsReceived
        {
            get { return m_statsReceived; }
            //TODO: protected
            set { m_statsReceived = value; }
        }

        /// <summary>
        /// Statistics: Date/time value that the stats have been reset.
        /// </summary>
        /// <value>
        /// Date/time value that the stats have been reset.
        /// </value>
        public virtual long StatsReset
        {
            get { return m_statsReset; }
            //TODO: protected
            set { m_statsReset = value; }
        }

        /// <summary>
        /// Set to <b>true</b> when the Service has advanced to the state at
        /// which it can accept requests from client threads.
        /// </summary>
        /// <value>
        /// <b>true</b> when the Service has advanced to the state at which
        /// it can accept requests from client threads.
        /// </value>
        public virtual bool IsAcceptingClients
        {
            get { return m_isAcceptingClients; }
            set
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    m_isAcceptingClients = value;

                    // free any blocked client threads
                    Monitor.PulseAll(this);
                }
            }
        }

        /// <summary>
        /// The <see cref="IOperationalContext"/> for this Service.
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

        #endregion

        #region IService implementation

        /// <summary>
        /// Invoked when <see cref="IService"/> is starting.
        /// </summary>
        public virtual event ServiceEventHandler ServiceStarting
        {
            add
            {
                EnsureEventDispatcher();
                m_serviceStarting += value;
            }

            remove
            {
                m_serviceStarting -= value;
            }
        }

        /// <summary>
        /// Invoked when <see cref="IService"/> has started.
        /// </summary>
        public virtual event ServiceEventHandler ServiceStarted
        {
            add
            {
                EnsureEventDispatcher();
                m_serviceStarted += value;
            }

            remove
            {
                m_serviceStarted -= value;
            }
        }

        /// <summary>
        /// Invoked when <see cref="IService"/> is stopping.
        /// </summary>
        public virtual event ServiceEventHandler ServiceStopping
        {
            add
            {
                EnsureEventDispatcher();
                m_serviceStopping += value;
            }

            remove
            {
                m_serviceStopping -= value;
            }
        }

        /// <summary>
        /// Invoked when <see cref="IService"/> has stopped.
        /// </summary>
        public virtual event ServiceEventHandler ServiceStopped
        {
            add
            {
                EnsureEventDispatcher();
                m_serviceStopped += value;
            }

            remove
            {
                m_serviceStopped -= value;
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
        public virtual void Configure(IXmlElement xml)
        {
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                if (ServiceState > ServiceState.Initial)
                {
                    throw new InvalidOperationException();
                }

                ServiceConfig = xml;

                if (xml != null)
                {
                    //TODO: DaemonPool config
                    /*
                    // <thread-count>
                    // <task-hung-threshold>
                    // <task-timeout>
                    int cThreads = xml.GetSafeElement("thread-count").Int;
                    if (cThreads > 0)
                    {
                        DaemonPool pool = getDaemonPool();
                        pool.setDaemonCount(cThreads);

                        long cHungMillis = ParseTime(xml, "task-hung-threshold", 0L);
                        if (cHungMillis > 0L)
                        {
                            pool.setHungThreshold(cHungMillis);
                        }

                        long cTimeoutMillis = ParseTime(xml, "task-timeout", 0L);
                        if (cTimeoutMillis > 0L)
                        {
                            pool.setTaskTimeout(cTimeoutMillis);
                        }
                    }*/

                    // <serializer>
                    IXmlElement xmlCat = xml.GetElement("serializer");
                    if (xmlCat != null && !XmlHelper.IsEmpty(xmlCat))
                    {
                        if (xmlCat.IsEmpty)
                        {
                            ConfigurableSerializerFactory factory = 
                                    new ConfigurableSerializerFactory();
                            factory.Config = xmlCat;
                            SerializerFactory = factory;
                        }
                        else
                        {
                            // The <serializer> element is _not_ empty; it 
                            // contains a string value (e.g. 
                            // <serializer>pof</serializer>). 
                            // The configured OperationalContext should 
                            // contain a corresponding serializer factory 
                            // in it's serializer map. Resolve the serializer 
                            // against the map.
                            String             sName      = xmlCat.GetString();
                            ISerializerFactory serializer = (ISerializerFactory)
                                    OperationalContext.SerializerMap[sName];
                            if (serializer == null)
                            {
                                throw new ArgumentException("Serializer "
                                    + sName + " not found.");
                            }
                            SerializerFactory = serializer;
                        }
                    }
                }
            }
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
            get { return ServiceState == ServiceState.Started && !IsExiting; }
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
                Stop();

                // as of Coherence 3.5, this method will not return until the 
                // service has actually stopped
                if (Thread != Thread.CurrentThread)
                {
                    while (DaemonState != DaemonState.Exited)
                    {
                        Blocking.Wait(this, 1000);
                    }
                }
            }
        }

        #endregion

        #region QueueProcessor overrides

        /// <summary>
        /// Specifies the name of the daemon thread.
        /// </summary>
        /// <remarks>
        /// If not specified, the type name will be used. This property can
        /// be set at design time or runtime. If set at runtime, it must be
        /// configured before Start() is invoked to cause the daemon thread
        /// to have the specified name.
        /// </remarks>
        /// <value>
        /// The name of the daemon thread.
        /// </value>
        public override string ThreadName
        {
            get { return ServiceName; }
        }

        /// <summary>
        /// The number of milliseconds that the daemon will wait for
        /// notification.
        /// </summary>
        /// <remarks>
        /// Zero means to wait indefinitely. Negative value means to skip
        /// waiting altogether.
        /// </remarks>
        /// <value>
        /// The number of milliseconds that the daemon will wait for
        /// notification.
        /// </value>
        public override long WaitMillis
        {
            get
            {
                //TODO: DaemonPool
                //DaemonPool pool = getDaemonPool();

                long wait1 = base.WaitMillis;
                long wait2 = 0L; //TODO: pool.isStarted() ? Math.max(1L, pool.getNextCheckMillis() - Base.getSafeTimeMillis()) : 0L;
                return wait1 <= 0L ? wait2 : Math.Min(wait1, wait2);
            }
            set
            {
                base.WaitMillis = value;
            }
        }

        /// <summary>
        /// Event notification called once the daemon's thread starts and
        /// before the daemon thread goes into the "wait - perform" loop.
        /// </summary>
        /// <remarks>
        /// Unlike the <c>OnInit()</c> event, this method executes on the
        /// daemon's thread.
        /// <p>
        /// This method is called while the caller's thread is still waiting
        /// for a notification to  "unblock" itself.</p>
        /// <p>
        /// Any exception thrown by this method will terminate the thread
        /// immediately.</p>
        /// </remarks>
        protected override void OnEnter()
        {
            base.OnEnter();

            ResetStats();
            ServiceState = ServiceState.Started;
        }

        /// <summary>
        /// This event occurs when an exception is thrown from
        /// <b>OnEnter</b>, <b>OnWait</b>, <b>OnNotify</b> and <b>OnExit</b>.
        /// </summary>
        /// <param name="e">
        /// Exception that has occured.
        /// </param>
        protected override void OnException(Exception e)
        {
            if (ServiceState < ServiceState.Started || ServiceState == ServiceState.Started && !IsAcceptingClients)
            {
                StartException = e;
            }
            base.OnException(e);
        }

        /// <summary>
        /// Event notification called right before the daemon thread
        /// terminates.
        /// </summary>
        /// <remarks>
        /// This method is guaranteed to be called only once and on the
        /// daemon's thread.
        /// </remarks>
        protected override void OnExit()
        {
            try
            {
                // if Coherence classes are deployed along with an application
                // (as opposed to an application server classpath)
                // it's possible that by this time the corresponding class loader
                // has been invalidated and attempt to load anything that has not been
                // yet loaded will fail (a known issue with WL 8.1 and WAS 5.0);
                // since we are exiting anyway, just log the exception...
                ServiceState = ServiceState.Stopped;
            }
            catch (Exception e)
            {
                CacheFactory.Log("Exception occured during exiting:\n " +  e, CacheFactory.LogLevel.Debug);
            }

            base.OnExit();

            EventDispatcher daemon = Dispatcher;
            if (daemon != null)
            {
                daemon.Stop();

                // give the chance for the daemon to drain it's queue
                try
                {
                    daemon.Thread.Join(1000);
                }
                catch (ThreadInterruptedException)
                {
                    Thread.CurrentThread.Interrupt();
                }
            }

            //TODO: DaemonPool
            /*
            DaemonPool pool = getDaemonPool();
            if (pool.isStarted())
            {
                pool.stop();
            }*/

            //TODO: Security
            /*
            Security security = Security.getInstance();
            if (security != null)
            {
                security.releaseSecureContext(getServiceName());
            }*/
        }

        /// <summary>
        /// Event notification to perform a regular daemon activity.
        /// </summary>
        /// <remarks>
        /// To get it called, another thread has to set IsNotification to
        /// <b>true</b>:
        /// <c>daemon.IsNotification = true;</c>
        /// </remarks>
        protected override void OnNotify()
        {
            base.OnNotify();

            //TODO: DaemonPool
            /*
            DaemonPool pool = getDaemonPool();
            if (pool.isStarted() && Base.getSafeTimeMillis() >= pool.getNextCheckMillis())
            {
                pool.checkTimeouts();
            }*/
        }

        /// <summary>
        /// Starts the daemon thread.
        /// </summary>
        /// <remarks>
        /// If the thread is already starting or has started, invoking
        /// this method has no effect. Synchronization is used here to
        /// verify that the start of the thread occurs; the lock is obtained
        /// before the thread is started, and the daemon thread notifies
        /// back that it has started from the Run() method.
        /// </remarks>
        public override void Start()
        {
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                Debug.Assert(ServiceState <= ServiceState.Started,
                             "Service restart is illegal (ServiceName=" + ServiceName + ')');

                base.Start();

                while (IsStarted && ServiceState <= ServiceState.Started && !IsAcceptingClients)
                {
                    Blocking.Wait(this, 1000);
                }

                if (ServiceState != ServiceState.Started)
                {
                    Exception e = StartException;
                    string    s = "Failed to start Service \"" + ServiceName
                                  + "\" (ServiceState=" + ServiceStateName + ')';
                    throw e == null ? new Exception(s) : new Exception(s, e);
                }
            }
        }

        /// <summary>
        /// Stops the daemon thread associated with this object.
        /// </summary>
        public override void Stop()
        {
            if (!IsStarted)
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    if (!IsStarted)
                    {
                        // service is not running ... don't worry about whether
                        // it has never been running vs. whether it started and
                        // stopped ... just set the state to register that stop
                        // was called so no one can later start it
                        ServiceState = ServiceState.Stopped;
                        return;
                    }
                }
            }

            base.Stop();

            if (Thread == Thread.CurrentThread)
            {
                ServiceState = ServiceState.Stopped;
            }
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Provide a human-readable representation of this object.
        /// </summary>
        /// <returns>
        /// A string whose contents represent the value of this object.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(GetType().Name)
                .Append("{Name=")
                .Append(ServiceName)
                .Append(", State=")
                .Append("(")
                .Append(ServiceStateName)
                .Append(')');

            string desc = Description;
            if (!StringUtils.IsNullOrEmpty(desc))
            {
                sb.Append(desc);
            }

            sb.Append('}');

            return sb.ToString();
        }

        /// <summary>
        /// Update the service thread name with any additional information
        /// that could be useful for thread dump analysis.
        /// </summary>
        /// <remarks>
        /// This method is usually called every time when a service state
        /// changes.
        /// </remarks>
        /// <seealso cref="ServiceState"/>
        protected virtual void UpdateServiceThreadName()
        {
            Thread thread = Thread;
            if (thread != null && thread.Name == null)
            {
                thread.Name = DecoratedThreadName;
            }
        }

        /// <summary>
        /// Block the calling thread until the Service has advanced to the
        /// state at which it can accept requests from client threads.
        /// </summary>
        public virtual void WaitAcceptingClients()
        {
            while (!IsAcceptingClients)
            {
                using (BlockingLock l = BlockingLock.Lock(this))
                {
                    if (ServiceState > ServiceState.Started)
                    {
                        Exception e = StartException;
                        string    s = "Failed to start Service \"" + ServiceName
                                      + "\" (ServiceState=" + ServiceStateName + ')';
                        throw e == null ? new Exception(s) : new Exception(s, e);
                    }

                    if (!IsAcceptingClients)
                    {
                        Blocking.Wait(this);
                    }
                }
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Adjust the default timeout value using the passed
        /// context-specific timeout value.
        /// </summary>
        /// <param name="defaultTimeout">
        /// The default timeout value (must be non-negative).
        /// </param>
        /// <param name="contextTimeout">
        /// Positive, or zero for "default timeout", or -1 for "no timeout".
        /// </param>
        /// <returns>
        /// The adjusted timeout value.
        /// </returns>
        public static long AdjustTimeout(long defaultTimeout, long contextTimeout)
        {
            const long TIMEOUT_DEFAULT = 0L;
            const long TIMEOUT_NONE    = -1L;

            return contextTimeout == TIMEOUT_DEFAULT ? defaultTimeout :
                   contextTimeout == TIMEOUT_NONE    ? 0L 
                                                     : contextTimeout;
        }

        /// <summary>
        /// Dispatch the given event to the <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="evt">
        /// The event to dispatch.
        /// </param>
        /// <param name="callback">
        /// The <see cref="EventCallback"/> object.
        /// </param>
        protected virtual void DispatchEvent(EventArgs evt, EventCallback callback)
        {
            DispatchEventTask task = InstantiateDispatchEvent();
            task.Event         = evt;
            task.EventCallback = callback;
            EnsureEventDispatcher().Queue.Add(task);
        }

        /// <summary>
        /// Dispatch a <see cref="ServiceEventArgs"/> to the
        /// <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="eventType">
        /// Service event type.
        /// </param>
        public virtual void DispatchServiceEvent(ServiceEventType eventType)
        {
            ServiceEventArgs evt      = new ServiceEventArgs(this, eventType);
            EventCallback    callback = new EventCallback(FireServiceEvent);
            DispatchEvent(evt, callback);
        }

        /// <summary>
        /// Return an instance of the configured <b>ISerializer</b>.
        /// </summary>
        /// <returns>
        /// A new <b>ISerializer</b>.
        /// </returns>
        public virtual ISerializer EnsureSerializer()
        {
            ISerializer serializer = Serializer;
            if (serializer == null)
            {
                serializer = InstantiateSerializer();
                Serializer = serializer;
            }

            return serializer;
        }

        /// <summary>
        /// Return the string value of a named child element of the given XML
        /// configuration.
        /// </summary>
        /// <param name="xmlConfig">
        /// The parent XML configuration element.
        /// </param>
        /// <param name="name">
        /// The name of the the child XML configuration element.
        /// </param>
        /// <param name="defaultValue">
        /// The default value to return if the child element is missing or
        /// empty.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the child element and default value is missing or empty.
        /// </exception>
        public static string EnsureStringValue(IXmlElement xmlConfig, string name, string defaultValue)
        {
            string value = xmlConfig.GetSafeElement(name).GetString(defaultValue);
            if (StringUtils.IsNullOrEmpty(value))
            {
                throw new ArgumentException("the required \"" + name
                        + "\" configuration element is missing or empty");
            }

            return value;
        }

        /// <summary>
        /// Raises service event.
        /// </summary>
        /// <param name="evt">
        /// <b>EventArgs</b> object.
        /// </param>
        public virtual void FireServiceEvent(EventArgs evt)
        {
            if (evt is ServiceEventArgs)
            {
                ServiceEventArgs serviceEvt = (ServiceEventArgs) evt;
                switch (serviceEvt.EventType)
                {
                    case ServiceEventType.Starting:
                        InvokeServiceEvent(m_serviceStarting, serviceEvt);
                        break;

                    case ServiceEventType.Started:
                        InvokeServiceEvent(m_serviceStarted, serviceEvt);
                        break;

                    case ServiceEventType.Stopping:
                        InvokeServiceEvent(m_serviceStopping, serviceEvt);
                        break;

                    case ServiceEventType.Stopped:
                        InvokeServiceEvent(m_serviceStopped, serviceEvt);
                        break;
                }
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

        /// <summary>
        /// Block the calling thread until the EventDispatcher Queue is empty
        /// and all outstanding tasks have been executed.
        /// </summary>
        /// <remarks>
        /// This method is mostly called on client threads.
        /// </remarks>
        public virtual void DrainEvents()
        {
            EventDispatcher daemon = Dispatcher;
            if (daemon != null)
            {
                daemon.DrainQueue();
            }
        }

        /// <summary>
        /// Return a running <see cref="Dispatcher"/>.
        /// </summary>
        public virtual EventDispatcher EnsureEventDispatcher()
        {
            EventDispatcher dispatcher = Dispatcher;

            if (dispatcher == null)
            {
                dispatcher = new EventDispatcher();
                dispatcher.Service = this;
                if (!IsExiting)
                {
                    //TODO: dispatcher.ThreadGroup = ThreadGroup;
                    dispatcher.Start();
                }
                Dispatcher = dispatcher;
            }

            return dispatcher;
        }

        /// <summary>
        /// Return a human-readible description of the Service statistics.
        /// </summary>
        public virtual string FormatStats()
        {
            long   cpu   = StatsCpu;
            long   total = DateTimeUtils.GetSafeTimeMillis() - StatsReset;
            long   msgs  = StatsReceived;
            double dCpu  = total == 0L ? 0.0 : ((double) cpu) / ((double) total);
            double dThru = cpu == 0L ? 0.0 : ((double) msgs * 1000) / ((double) cpu);

            // round rates
            dCpu = ((int) (dCpu * 1000)) / 10D; // percentage

            StringBuilder sb = new StringBuilder();
            sb.Append("Cpu=")
                .Append(cpu)
                .Append("ms (")
                .Append(dCpu)
                .Append("%), Messages=")
                .Append(msgs)
                .Append(", Throughput=")
                .Append((float) dThru)
                .Append("msg/sec");

            //TODO: DaemonPool
            // thread pool stats
            /*
            DaemonPool pool = getDaemonPool();
            if (pool.isStarted())
            {
                long cPoolTotal = pool.getStatsActiveMillis();
                long cTasks = pool.getStatsTaskCount();
                long cHung = pool.getStatsHungCount();
                float flAvgThread = cTotal == 0L ? 0.0f : (float)(((double)cPoolTotal) / ((double)cTotal));
                float flAvgTask = cTasks == 0L ? 0.0f : (float)(((double)cPoolTotal) / ((double)cTasks));

                sb.append(", AverageActiveThreadCount=")
                  .append(flAvgThread)
                  .append(", Tasks=")
                  .append(cTasks)
                  .append(", AverageTaskDuration=")
                  .append(flAvgTask)
                  .append("ms, MaximumBacklog=")
                  .append(pool.getStatsMaxBacklog());

                if (cHung > 0)
                {
                    sb.append(", HungTaskCount=")
                      .append(cHung)
                      .append(", HungMaxDuration=")
                      .append(pool.getStatsHungDuration())
                      .append(", HungMaxId=")
                      .append(pool.getStatsHungTaskId());
                }
            }*/

            return sb.ToString();
        }

        /// <summary>
        /// Factory pattern: create a new <see cref="DispatchEventTask"/>
        /// instance.
        /// </summary>
        /// <returns>
        /// A new <b>DispatchEventTask</b> instance.
        /// </returns>
        protected virtual DispatchEventTask InstantiateDispatchEvent()
        {
            return new DispatchEventTask();
        }

        /// <summary>
        /// Instantiate a <see cref="ISerializer"/>
        /// </summary>
        /// <returns>
        /// A new <see cref="ISerializer"/> instance.
        /// </returns>
        protected virtual ISerializer InstantiateSerializer()
        {
            ISerializerFactory factory = SerializerFactory;
            return factory == null
                ? new ConfigurablePofContext()
                : factory.CreateSerializer();
        }

        /// <summary>
        /// Return <b>true</b> if the current thread is one of the Service
        /// threads.
        /// </summary>
        /// <param name="isStrict">
        /// If <b>true</b> then only the service thread and event dispatcher
        /// thread are considered to be service threads, if <b>false</b>,
        /// then DaemonPool threads are also considered to be service
        /// threads.
        /// </param>
        /// <returns>
        /// <b>true</b> if the current thread is one of the Service threads.
        /// </returns>
        public virtual bool IsServiceThread(bool isStrict)
        {
            Thread          thread     = Thread.CurrentThread;
            EventDispatcher dispatcher = Dispatcher;

            if (thread == Thread || dispatcher != null && thread == dispatcher.Thread)
            {
                return true;
            }
            else if (!isStrict)
            {
                //TODO: DaemonPool
                /*DaemonPool pool = getDaemonPool();
                return pool != null && pool.getThreadGroup() == thread.getThreadGroup();*/
            }

            return false;
        }

        /// <summary>
        /// Reset the Service statistics.
        /// </summary>
        public virtual void ResetStats()
        {
            //TODO: getDaemonPool().resetStats();
            StatsCpu      = 0L;
            StatsReceived = 0L;
            StatsReset    = DateTimeUtils.GetSafeTimeMillis();
        }

        /// <summary>
        /// Called when the Service has transitioned to the specified state.
        /// </summary>
        /// <param name="state">
        /// The new Service state.
        /// </param>
        protected virtual void OnServiceState(ServiceState state)
        {
            switch (state)
            {
                case ServiceState.Starting:
                    OnServiceStarting();
                    break;

                case ServiceState.Started:
                    OnServiceStarted();
                    break;

                case ServiceState.Stopping:
                    OnServiceStopping();
                    break;

                case ServiceState.Stopped:
                    OnServiceStopped();
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        /// <summary>
        /// The default implementation of this method does nothing.
        /// </summary>
        protected virtual void OnServiceStarting()
        {}

        /// <summary>
        /// The default implementation of this method sets
        /// <see cref="IsAcceptingClients"/> to <b>true</b>.
        /// </summary>
        /// <remarks>
        /// If the Service has not completed preparing at this point, then
        /// the Service must override this implementation and only set
        /// <b>IsAcceptingClients</b> to <b>true</b> when the Service has
        /// actually "finished starting".
        /// </remarks>
        public virtual void OnServiceStarted()
        {
            IsAcceptingClients = true;
        }

        /// <summary>
        /// The default implementation of this method sets
        /// <see cref="IsAcceptingClients"/> to <b>false</b>.
        /// </summary>
        protected virtual void OnServiceStopping()
        {
            IsAcceptingClients = false;
        }

        /// <summary>
        /// The default implementation of this method sets
        /// <see cref="IsAcceptingClients"/> to <b>false</b>.
        /// </summary>
        protected virtual void OnServiceStopped()
        {
            IsAcceptingClients = false;
        }

        /// <summary>
        /// Parse the <b>String</b> value of the child <b>IXmlElement</b>
        /// with the given name as a time in milliseconds.
        /// </summary>
        /// <remarks>
        /// If the specified child <b>IXmlElement</b> does not exist or is
        /// empty, the specified default value is returned.
        /// </remarks>
        /// <param name="xml">
        /// The parent <b>IXmlElement</b>.
        /// </param>
        /// <param name="name">
        /// The name of the child <b>IXmlElement</b>
        /// </param>
        /// <param name="defaultValue">
        /// The default value.
        /// </param>
        /// <returns>
        /// The time (in milliseconds) represented by the specified child
        /// <b>XmlNode</b>.
        /// </returns>
        protected static long ParseTime(IXmlElement xml, string name, long defaultValue)
        {
            if (xml == null)
            {
                return defaultValue;
            }
            string time = xml.GetSafeElement(name).GetString();
            if (time.Length == 0)
            {
                return defaultValue;
            }
            try
            {
                return XmlHelper.ParseTime(time);
            }
            catch (Exception e)
            {
                throw new Exception("illegal \"" + name + "\" value: " + time, e);
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// Set to true when the Service has advanced to the state at which
        /// it can accept requests from client threads.
        /// </summary>
        private bool m_isAcceptingClients;

        //TODO: private DaemonPool m_daemonPool;

        /// <summary>
        /// The event dispatcher daemon.
        /// </summary>
        [NonSerialized]
        private EventDispatcher m_eventDispatcher;

        /// <summary>
        /// Configured ISerializer instance.
        /// </summary>
        private ISerializer m_serializer;

        /// <summary>
        /// The serializer factory.
        /// </summary>
        [NonSerialized]
        private ISerializerFactory m_serializerFactory;

        /// <summary>
        /// Original XML configuration that was supplied to the Service; may
        /// be null.
        /// </summary>
        private IXmlElement m_serviceConfig;

        /// <summary>
        /// The name of this Service.
        /// </summary>
        private string m_serviceName;

        /// <summary>
        /// The state of the Service; one of the ServiceState enumeration
        /// values.
        /// </summary>
        private ServiceState m_serviceState;

        /// <summary>
        /// Statistics: total time spent while processing messages.
        /// </summary>
        [NonSerialized]
        private long m_statsCpu;

        /// <summary>
        /// Statistics: total number of received messages.
        /// </summary>
        [NonSerialized]
        private long m_statsReceived;

        /// <summary>
        /// Statistics: Date/time value that the stats have been reset.
        /// </summary>
        [NonSerialized]
        private long m_statsReset;

        /// <summary>
        /// Service event handlers.
        /// </summary>
        private ServiceEventHandler m_serviceStarting;
        private ServiceEventHandler m_serviceStarted;
        private ServiceEventHandler m_serviceStopping;
        private ServiceEventHandler m_serviceStopped;

        /// <summary>
        /// The IOperationalContext for this Service.
        /// </summary>
        private IOperationalContext m_operationalContext;

        #endregion

        #region Inner class: EventDispatcher

        /// <summary>
        /// <see cref="QueueProcessor"/> used to dispatch asynchronous
        /// <see cref="ServiceEventArgs"/>s.
        /// </summary>
        public class EventDispatcher : QueueProcessor
        {
            #region Properties

            /// <summary>
            /// Parent <see cref="Service"/>.
            /// </summary>
            /// <value>
            /// Parent Service.
            /// </value>
            public virtual Service Service
            {
                get { return m_parentService; }
                set { m_parentService = value; }
            }

            /// <summary>
            /// This is the <b>Queue</b> to which items that need to be
            /// processed are added, and from which the daemon pulls items to
            /// process.
            /// </summary>
            public override Queue Queue
            {
                get
                {
                    Queue queue = m_queue;
                    if (queue == null)
                    {
                        queue = m_queue = new EventDispatcherQueue();
                        ((EventDispatcherQueue) queue).EventDispatcher = this;
                    }
                    return queue;
                }
                set { m_queue = value; }
            }

            /// <summary>
            /// The maximum number of events in the queue before determining
            /// that the dispatcher is clogged.
            /// </summary>
            /// <remarks>
            /// Zero means no limit.
            /// </remarks>
            public virtual int CloggedCount
            {
                get { return m_cloggedCount; }
                set { m_cloggedCount = value; }
            }

            /// <summary>
            /// The number of milliseconds to pause client threads when a
            /// clog occurs, to wait for the clog to dissipate.
            /// </summary>
            /// <remarks>
            /// The pause is repeated until the clog is gone. Anything less
            /// than one (e.g. zero) is treated as one.
            /// </remarks>
            public virtual int CloggedDelay
            {
                get { return m_cloggedDelay; }
                set { m_cloggedDelay = Math.Max(1, value); }
            }

            /// <summary>
            /// Set to <b>true</b> while the EventDispatcher daemon thread
            /// is in the process of dispatching events.
            /// </summary>
            /// <value>
            /// <b>true</b> while the EventDispatcher daemon thread is in the
            /// process of dispatching events.
            /// </value>
            public virtual bool IsDispatching
            {
                get { return m_isDispatching; }
                //TODO: protected
                set { m_isDispatching = value; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Create new instance of EventDispatcher.
            /// </summary>
            public EventDispatcher()
            {
                CloggedCount = 1024;
                CloggedDelay = 32;
                DaemonState  = DaemonState.Initial;
            }

            #endregion

            #region QueueProcessor overrides

            /// <summary>
            /// Specifies the name of the daemon thread.
            /// </summary>
            /// <remarks>
            /// If not specified, the type name will be used. This property can
            /// be set at design time or runtime. If set at runtime, it must be
            /// configured before Start() is invoked to cause the daemon thread
            /// to have the specified name.
            /// </remarks>
            /// <value>
            /// The name of the daemon thread.
            /// </value>
            public override string ThreadName
            {
                get { return Service.ThreadName + ':' + base.ThreadName; }
            }

            /// <summary>
            /// This event occurs when an exception is thrown from
            /// <b>OnEnter</b>, <b>OnWait</b>, <b>OnNotify</b> and <b>OnExit</b>.
            /// </summary>
            /// <param name="e">
            /// Exception that has occured.
            /// </param>
            protected override void OnException(Exception e)
            {
                if (!IsExiting)
                {
                    CacheFactory.Log("The following exception was caught by the event dispatcher:",
                        CacheFactory.LogLevel.Error);
                    CacheFactory.Log(e, CacheFactory.LogLevel.Error);
                    CacheFactory.Log("(The service event thread has logged the exception and is continuing.)",
                        CacheFactory.LogLevel.Error);
                }
            }

            /// <summary>
            /// Event notification called right before the daemon thread
            /// terminates.
            /// </summary>
            /// <remarks>
            /// This method is guaranteed to be called only once and on the
            /// daemon's thread.
            /// </remarks>
            protected override void OnExit()
            {
                OnNotify();

                base.OnExit();
            }

            /// <summary>
            /// Event notification to perform a regular daemon activity.
            /// </summary>
            /// <remarks>
            /// To get it called, another thread has to set IsNotification to
            /// <b>true</b>:
            /// <c>daemon.IsNotification = true;</c>
            /// </remarks>
            protected override void OnNotify()
            {
                base.OnNotify();

                Queue     queue = Queue;
                IRunnable task  = null;

                IsDispatching = true;
                try
                {
                    while (true)
                    {
                        task = queue.RemoveNoWait() as IRunnable;
                        if (task == null)
                        {
                            break;
                        }

                        task.Run();
                    }
                }
                catch (Exception e)
                {
                    if (Service.IsRunning)
                    {
                        CacheFactory.Log("An exception occurred while dispatching the following event:\n"
                                         + task, CacheFactory.LogLevel.Error);
                        OnException(e);
                    }
                }
                finally
                {
                    IsDispatching = false;
                }
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// This method is mostly called on client threads.
            /// </summary>
            public virtual void DrainOverflow()
            {
                Service service = Service;
                if (!(service.IsServiceThread(false)))
                {
                    // slow down too agressive clients to prevent memory overflow
                    int maxEvents   = CloggedCount;
                    int pauseMillis = CloggedDelay;
                    while (IsStarted)
                    {
                        int eventsCount = Queue.Count;
                        if (eventsCount < maxEvents || maxEvents <= 0 || !Sleep(pauseMillis))
                        {
                            break;
                        }
                    }
                }
            }

            /// <summary>
            /// Block the calling thread until the Queue is empty and all
            /// outstanding tasks have been executed.
            /// </summary>
            /// <remarks>
            /// This method is mostly called on client threads.
            /// </remarks>
            public virtual void DrainQueue()
            {
                // the queue cannot be drained by service threads
                if (!(Service.IsServiceThread(false)))
                {
                    Queue queue = Queue;

                    // wait for all outstanding tasks to complete
                    while (!queue.IsEmpty() || IsDispatching)
                    {
                        Blocking.Sleep(1);
                    }
                }
            }

            #endregion

            #region Data members

            /// <summary>
            /// The maximum number of events in the queue before determining
            /// that the dispatcher is clogged.
            /// </summary>
            private int m_cloggedCount;

            /// <summary>
            /// The number of milliseconds to pause client threads when a
            /// clog occurs, to wait for the clog to dissipate.
            /// </summary>
            private int m_cloggedDelay;

            /// <summary>
            /// Set to true while the EventDispatcher daemon thread is in
            /// the process of dispatching events.
            /// </summary>
            private volatile bool m_isDispatching;

            /// <summary>
            /// Parent Service.
            /// </summary>
            private Service m_parentService;

            /// <summary>
            /// This is the Queue to which items that need to be processed
            /// are added, and from which the daemon pulls items to process.
            /// </summary>
            private Queue m_queue;

            #endregion

            #region Inner class: EventDispatcherQueue

            /// <summary>
            /// Provides a means to efficiently (and in a thread-safe manner)
            /// queue received messages and messages to be sent.
            /// </summary>
            internal class EventDispatcherQueue : Queue
            {
                /// <summary>
                /// Parent <see cref="EventDispatcher"/>.
                /// </summary>
                /// <value>
                /// Parent <b>EventDispatcher</b>.
                /// </value>
                public EventDispatcher EventDispatcher
                {
                    get { return m_dispatcher; }
                    set { m_dispatcher = value; }
                }

                /// <summary>
                /// Appends the specified element to the end of this queue.
                /// </summary>
                /// <remarks>
                /// Queues may place limitations on what elements may be added
                /// to this Queue. In particular, some Queues will impose restrictions
                /// on the type of elements that may be added. Queue implementations
                /// should clearly specify in their documentation any restrictions on
                /// what elements may be added.
                /// </remarks>
                /// <param name="obj">
                /// Element to be appended to this Queue.
                /// </param>
                /// <returns>
                /// <b>true</b> (as per the general contract of the IList.Add method)
                /// </returns>
                /// <exception cref="InvalidCastException">
                /// If the class of the specified element prevents it from being added
                /// to this Queue.
                /// </exception>
                public override bool Add(object obj)
                {
                    using (BlockingLock l = BlockingLock.Lock(this))
                    {
                        switch (EventDispatcher.Service.ServiceState)
                        {
                            case ServiceState.Starting:
                            case ServiceState.Started:
                            case ServiceState.Stopping:
                            case ServiceState.Stopped:
                                return base.Add(obj);

                            default:
                                throw new InvalidOperationException();
                        }
                    }
                }

                /// <summary>
                /// Parent EventDispatcher.
                /// </summary>
                private EventDispatcher m_dispatcher;
            }

            #endregion
        }

        #endregion

        #region Inner class: DispatchEventTask

        /// <summary>
        /// Runnable event.
        /// </summary>
        public class DispatchEventTask : IRunnable
        {
            #region Properties

            /// <summary>
            /// The <b>EventArgs</b> to dispatch.
            /// </summary>
            /// <value>
            /// The <b>EventArgs</b> to dispatch.
            /// </value>
            public virtual EventArgs Event
            {
                get { return m_event; }
                set { m_event = value; }
            }

            /// <summary>
            /// The <see cref="EventCallback"/> that raises the
            /// <see cref="Event"/>.
            /// </summary>
            /// <value>
            /// <b>EventCallback</b> object.
            /// </value>
            public virtual EventCallback EventCallback
            {
                get { return m_eventCallback; }
                set { m_eventCallback = value; }
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Invokes the event callback, with special remark towards
            /// multithreading (using local copy of delegate and no inline
            /// attribute for method).
            /// </summary>
            /// <param name="handler">
            /// The EventCallback that is being invoked.
            /// </param>
            /// <param name="evt">
            /// Event arguments.
            /// </param>
            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void InvokeEventCallback(EventCallback handler, EventArgs evt)
            {
                if (handler != null)
                {
                    handler(evt);
                }
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Provide a human-readable representation of this object.
            /// </summary>
            /// <returns>
            /// A string whose contents represent the value of this object.
            /// </returns>
            public override string ToString()
            {
                return GetType().Name + ": " + Event;
            }

            #endregion

            #region IRunnable implementation

            /// <summary>
            /// Execute the action specific to the object implementation.
            /// </summary>
            public virtual void Run()
            {
                EventArgs evt = Event;
                Debug.Assert(evt != null);
                InvokeEventCallback(EventCallback, evt);
            }

            #endregion

            #region Data members

            /// <summary>
            /// The event to dispatch.
            /// </summary>
            private EventArgs m_event;

            /// <summary>
            /// The event callback.
            /// </summary>
            private EventCallback m_eventCallback;

            #endregion
        }

        #endregion
    }

    #region Enum: ServiceState

    /// <summary>
    /// Service state enum.
    /// </summary>
    public enum ServiceState
    {
        /// <summary>
        /// The Service has been created but has not been started yet.
        /// </summary>
        Initial = 0,

        /// <summary>
        /// The Service has been asked to start but has not yet finished
        /// starting.
        /// </summary>
        Starting = 1,

        /// <summary>
        /// The Service is running.
        /// </summary>
        Started = 2,

        /// <summary>
        /// The Service has shut down gracefully (Shutdown method) or has
        /// been stopped hard (Stop method).
        /// </summary>
        Stopped = 4,

        /// <summary>
        /// The Service has been asked to shut down gracefully but has
        /// not yet finished shutting down gracefully.
        /// </summary>
        Stopping =3
    }

    #endregion
}