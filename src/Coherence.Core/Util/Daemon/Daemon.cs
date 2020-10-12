/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.Threading;

using Tangosol.Net;
using Tangosol.Net.Messaging;

namespace Tangosol.Util.Daemon
{
    /// <summary>
    /// This class is used to create and manage a daemon thread.
    /// </summary>
    /// <remarks>
    /// <p>
    /// A caller may use the following methods to control the Daemon
    /// object:
    /// <list type="number">
    /// <item>
    /// <term><see cref="Start"/></term>
    /// <description>creates and starts the daemon thread</description>
    /// </item>
    /// <item>
    /// <term><see cref="IsStarted"/></term>
    /// <description>determines whether the daemon is running</description>
    /// </item>
    /// <item>
    /// <term><see cref="Stop"/></term>
    /// <description>stops the daemon thread and releases the related
    /// resources </description>
    /// </item>
    /// </list></p>
    /// <p>
    /// Advanced options available to a designer or caller include:
    /// <list type="number">
    /// <item>
    /// <term><see cref="Priority"/></term>
    /// <description>before starting the daemon, a Thread priority can be
    /// provided</description>
    /// </item>
    /// <item>
    /// <term><see cref="ThreadName"/></term>
    /// <description>before starting the daemon, a Thread name can be
    /// provided</description>
    /// </item>
    /// <item>
    /// <term><see cref="Thread"/></term>
    /// <description>the actual Thread object can be accessed via this
    /// property</description>
    /// </item>
    /// <item>
    /// <term><see cref="StartException"/></term>
    /// <description>if the start method fails to start the daemon, the
    /// StartException property provides the failure information
    /// </description>
    /// </item>
    /// </list></p>
    /// The daemon thread itself executes the following events while it is
    /// running:
    /// <list type="number">
    /// <item>
    /// <term><see cref="OnEnter"/></term>
    /// <description>invoked when the daemon starts</description>
    /// </item>
    /// <item>
    /// <term><see cref="OnWait"/></term>
    /// <description>invoked to wait for notification</description>
    /// </item>
    /// <item>
    /// <term><see cref="OnNotify"/></term>
    /// <description>invoked when a notification occurs</description>
    /// </item>
    /// <item>
    /// <term><see cref="OnInterrupt"/></term>
    /// <description>invoked when the thread is interrupted when waiting for
    /// a notification</description>
    /// </item>
    /// <item>
    /// <term><see cref="OnException"/></term>
    /// <description>invoked when an exception occurs while invoking one of
    /// the daemon events</description>
    /// </item>
    /// <item>
    /// <term><see cref="OnExit"/></term>
    /// <description>invoked before the daemon exits</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <author>Goran Milosavljevic  2006.08.23</author>
    public class Daemon : IRunnable
    {
        #region Properties

        /// <summary>
        /// The resolution of the system clock in milliseconds.
        /// </summary>
        /// <value>
        /// The resolution of the system clock in milliseconds.
        /// </value>
        public static long ClockResolutionMillis
        {
            set { m_clockResolutionMillis = Math.Max(1, value); }
            get { return m_clockResolutionMillis; }
        }

        /// <summary>
        /// Specifies the state of the daemon
        /// (Initial, Starting, Running, Exiting, Exited).
        /// </summary>
        /// <remarks>
        /// Change the daemon's state to the specified state iff the new state is greater then the current state.
        /// 
        /// Despite this property being volatile, the setter is synchronized to ensure forward only state transitions.
        /// Additionally this allows for queries of the state to be held stable by synchronizing before the get and the corresponding usage.
        /// State transitions also trigger a notifyAll on the daemon's monitor
        /// </remarks>
        /// <value>
        /// One of the <see cref="DaemonState"/> values.
        /// </value>
        public virtual DaemonState DaemonState
        {
            get { return m_daemonState; }
            set
            {
                lock(this)
                {
                    if (value > m_daemonState)
                    {
                        m_daemonState = value;
                        Monitor.PulseAll(this);
                    }
                }
            }
        }

        /// <summary>
        /// Monitor object to coordinate clearing the thread interrupt set by
        /// <see cref="Stop"/> prior to running <see cref="OnExit"/>.
        /// </summary>
        /// <since>12.2.1.0.7</since>
        protected virtual object ExitMonitor
        {
            get
            {
                return f_exitMonitor;
            }
        }

        /// <summary>
        /// An object that serves as a mutex for this Daemon synchronization.
        /// </summary>
        /// <remarks>
        /// When idle, the Daemon is waiting for a notification on the Lock
        /// object.
        /// </remarks>
        /// <value>
        /// An object that serves as a mutex for this Deamon synchronization.
        /// </value>
        /// <seealso cref="OnNotify"/>
        /// <seealso cref="OnWait"/>
        public virtual object Lock
        {
            get
            {
                object lockObject = m_lock;
                return lockObject == null ? this : lockObject;
            }
            set { m_lock = value; }
        }

        /// <summary>
        /// A non-zero value specifies the priority of the daemon's thread.
        /// </summary>
        /// <remarks>
        /// A zero value implies the Thread default priority. Priority must
        /// be set before the Daemon is started (by the Start() method) in
        /// order to have effect.
        /// </remarks>
        /// <value>
        /// <b>ThreadPriority</b> value.
        /// </value>
        public virtual ThreadPriority Priority
        {
            get { return m_priority; }
            set { m_priority = value; }
        }

        /// <summary>
        /// The exception (if any) that prevented the daemon from starting
        /// successfully.
        /// </summary>
        /// <value>
        /// The exception that prevented the daemon from starting
        /// successfully.
        /// </value>
        public virtual Exception StartException
        {
            get { return m_startException; }
            set { m_startException = value; }
        }

        /// <summary>
        /// Date/time value that this Daemon's thread has started.
        /// </summary>
        /// <value>
        /// Date/time value that this Daemon's thread has started.
        /// </value>
        public virtual long StartTimestamp
        {
            get { return m_startTimestamp; }
            set { m_startTimestamp = value; }
        }

        /// <summary>
        /// The daemon thread if it is running, or <c>null</c> before the
        /// daemon starts and after the daemon stops.
        /// </summary>
        /// <value>
        /// The daemon thread if it is running, or <c>null</c>.
        /// </value>
        public virtual Thread Thread
        {
            get { return m_thread; }
            set { m_thread = value; }
        }

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
        public virtual string ThreadName
        {
            get
            {
                string name = m_threadName;
                return name == null ? GetType().FullName : name;
            }
            set { m_threadName = value; }
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
        /// <seealso cref="OnWait"/>
        public virtual long WaitMillis
        {
            get { return m_waitMillis; }
            set { m_waitMillis = value; }
        }

        /// <summary>
        /// Specifes whether there is work for the daemon to do; if there is
        /// work, IsNotification must evaluate to <b>true</b>, and if there
        /// is no work (implying that the daemon should wait for work) then
        /// IsNotification must evaluate to <b>false</b>.
        /// </summary>
        /// <remarks>
        /// To verify that a wait is necessary, the monitor on the Lock
        /// property is first obtained and then IsNotification is evaluated;
        /// only if IsNotification evaluates to <b>false</b> will the daemon
        /// go into a wait state on the Lock property.
        /// <p/>
        /// To unblock (notify) the daemon, another thread should set
        /// IsNotification to <b>true</b>.
        /// </remarks>
        /// <value>
        /// <b>true</b> if there is work for the daemon to do, <b>false</b>
        /// otherwise.
        /// </value>
        /// <seealso cref="OnWait"/>
        public virtual bool IsNotification
        {
            get { return m_isNotification; }
            set
            {
                object o = Lock;
                lock (o)
                {
                    m_isNotification = value;

                    if (value)
                    {
                        Thread thread = Thread;
                        if (thread != null && thread != Thread.CurrentThread)
                        {
                            Monitor.PulseAll(o);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determine whether this Daemon should start automatically at the
        /// initialization time.
        /// </summary>
        /// <value>
        /// <b>true</b> if Daemon should start automatically at the
        /// initialization time.
        /// </value>
        /// <seealso cref="OnInit"/>
        public virtual bool IsAutoStart
        {
            get { return m_isAutoStart; }
            set { m_isAutoStart = value; }
        }

        /// <summary>
        /// Specifies whether the daemon is instructed to stop.
        /// </summary>
        /// <value>
        /// <b>true</b> if Deamon is in the state Exiting; otherwise
        /// it <b>false</b>.
        /// </value>
        public virtual bool IsExiting
        {
            get { return DaemonState == DaemonState.Exiting; }
            set
            {
                if (value)
                {
                    DaemonState = DaemonState.Exiting;
                }
            }
        }

        /// <summary>
        /// Specifies whether the daemon has been started.
        /// </summary>
        /// <value>
        /// <b>true</b> if Daemon is started; <b>false</b> if it is in the
        /// state of Initial or Exited.
        /// </value>
        public virtual bool IsStarted
        {
            get
            {
                DaemonState state = DaemonState;
                return state == DaemonState.Starting || state == DaemonState.Running || state == DaemonState.Exiting;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for Daemon object.
        /// </summary>
        /// <remarks>
        /// It sets <see cref="DaemonState"/> to
        /// <see cref="Util.Daemon.DaemonState.Initial"/>.
        /// </remarks>
        public Daemon() : this(true, false)
        {}

        /// <summary>
        /// Parametrized constructor.
        /// </summary>
        /// <param name="init">
        /// If <b>true</b>, <see cref="DaemonState"/> would be set to
        /// <see cref="Util.Daemon.DaemonState.Initial"/>.
        /// </param>
        /// <param name="autoStart">
        /// Determines whether Daemon should be automatically
        /// started or not.
        /// </param>
        public Daemon(bool init, bool autoStart)
        {
            if (init)
            {
                ClockResolutionMillis = 1L;
                DaemonState           = DaemonState.Initial;
            }
            IsAutoStart = autoStart;
            OnInit();
        }

        /// <summary>
        /// Perform clean up before object is reclaimed by GC.
        /// </summary>
        ~Daemon()
        {
            Stop();
        }

        #endregion

        #region Event handlers

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
        protected virtual void OnEnter()
        {
            StartTimestamp = DateTimeUtils.GetSafeTimeMillis();
        }

        /// <summary>
        /// This event occurs when an exception is thrown from
        /// <b>OnEnter</b>, <b>OnWait</b>, <b>OnNotify</b> and <b>OnExit</b>.
        /// </summary>
        /// <param name="e">
        /// Exception that has occured.
        /// </param>
        protected virtual void OnException(Exception e)
        {
            if (IsExiting)
            {
                CacheFactory.Log(GetType().Name + " caught an unhandled exception ("
                                 + e.GetType().Name + ": " + e.Message + ") while exiting.",
                                 CacheFactory.LogLevel.Debug);
            }
            else
            {
                CacheFactory.Log("Terminating " + GetType().Name + " due to unhandled exception: "
                                 + e.GetType().Name,
                                 CacheFactory.LogLevel.Error);
                CacheFactory.Log(e, CacheFactory.LogLevel.Error);
                Stop();
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
        protected virtual void OnExit()
        {}

        /// <summary>
        /// Initialization method.
        /// </summary>
        public virtual void OnInit()
        {
            if (IsAutoStart)
            {
                Start();
            }
        }

        /// <summary>
        /// Event notification called if the daemon's thread get interrupted.
        /// </summary>
        /// <param name="e">
        /// <b>ThreadInterruptedException</b>.
        /// </param>
        /// <seealso cref="Stop"/>
        protected virtual void OnInterrupt(ThreadInterruptedException e)
        {
            if (!IsExiting)
            {
                CacheFactory.Log("Interrupted " + GetType().Name + ", " + Thread.CurrentThread,
                                 CacheFactory.LogLevel.Always);
            }
        }

        /// <summary>
        /// Event notification to perform a regular daemon activity.
        /// </summary>
        /// <remarks>
        /// To get it called, another thread has to set IsNotification to
        /// <b>true</b>:
        /// <c>daemon.IsNotification = true;</c>
        /// </remarks>
        /// <seealso cref="OnWait"/>
        protected virtual void OnNotify()
        {}

        /// <summary>
        /// Event notification called when the daemon's Thread is waiting
        /// for work.
        /// </summary>
        /// <seealso cref="Run"/>
        protected virtual void OnWait()
        {
            object o = Lock;
            lock (o)
            {
                if (!IsNotification)
                {
                    long timeout = WaitMillis;
                    if (timeout >= 0)
                    {
                        if (timeout == 0 || timeout > int.MaxValue)
                        {
                            Monitor.Wait(o);
                        }
                        else
                        {
                            Monitor.Wait(o, (int) timeout);
                        }    
                    }
                }
                IsNotification = false;
            }
        }

        #endregion

        #region Controlling the Daemon

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
        public virtual void Start()
        {
            lock (this)
            {
                if (IsStarted)
                {
                    return;
                }

                Thread thread       = new Thread(new ThreadStart(Run));
                thread.Name         = ThreadName;
                thread.IsBackground = true;
                thread.Priority     = Priority;
                Thread              = thread;

                // start the thread
                StartException = null;
                DaemonState    = DaemonState.Starting;
                thread.Start();

                // wait for the thread to enter its "wait for notification" section
                while (DaemonState == DaemonState.Initial || DaemonState == DaemonState.Starting)
                {
                    Monitor.Wait(this);
                }

                Exception ex = StartException;
                if (ex != null)
                {
                    StartException = null;
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Stops the daemon thread associated with this object.
        /// </summary>
        public virtual void Stop()
        {
            // Once IsExiting is set the daemon's thread will attempt to clear any interrupts and then proceed to OnExit.
            // In order to ensure that this doesn't occur before we actually get to interrupt the thread we synchronize this method
            // as well as Run's call to clear the interrupt.
            lock (ExitMonitor)
            {
                // only go through Stop() once to prevent spurious interrupts during OnExit()
                if (!IsExiting)
                {
                    IsExiting = true;

                    Thread thread = Thread;
                    if (thread != null && thread != Thread.CurrentThread)
                    {
                        try
                        {
                            thread.Interrupt();
                        }
                        catch (Exception)
                        {}
                    }
                }
            }
        }

        /// <summary>
        /// This method is called right after this daemon's thread starts.
        /// </summary>
        public virtual void Run()
        {
            // run() must only be invoked on the daemon thread
            Debug.Assert(Thread == Thread.CurrentThread,
                         "run() invoked on a different thread: " +
                         Thread.CurrentThread);

            try
            {
                // any exception OnEnter kills the thread
                try
                {
                    OnEnter();
                }
                catch (Exception e)
                {
                    // If an exception is thrown from OnEnter, we want to kill
                    // the thread.  Returning from here will cause the finally block
                    // run onExit() and notify waiters. 
                    StartException = e;
                    IsExiting = true;
                    OnException(e);
                    return;
                }

                DaemonState = DaemonState.Running;

                while (!IsExiting)
                {
                    try
                    {
                        OnWait();

                        if (!IsExiting)
                        {
                            OnNotify();
                        }
                    }
                    catch (ThreadInterruptedException e)
                    {
                        OnInterrupt(e);
                    }
                    catch (Exception e)
                    {
                        OnException(e);
                    }
                }
            }
            finally
            {
                try
                {
                    try
                    {
                        // see comment in Stop()
                        lock (ExitMonitor)
                        {
                            try
                            {
                                Thread.Sleep(0); // catch and clear the interrupt flag
                            }
                            catch (ThreadInterruptedException)
                            {
                            }
                        }
                        OnExit();
                    }
                    finally
                    {
                        Thread      = null;
                        DaemonState = DaemonState.Exited;
                    }
                }
                catch (Exception e)
                {
                    OnException(e);
                }
            }
            // the thread terminates right here
        }

        /// <summary>
        /// Causes the current thread to sleep for the specified interval.
        /// </summary>
        /// <remarks>
        /// If interrupted while sleeping the interrupt flag will be set
        /// and sleep will return <b>false</b>.
        /// </remarks>
        /// <returns>
        /// <b>true</b> if the thread slept, or <b>false</b> if its sleep
        /// was interrupted.
        /// </returns>
        public static bool Sleep(long millis)
        {
            try
            {
                if (millis > Int32.MaxValue)
                {
                    millis = Int32.MaxValue;
                }
                Thread.Sleep((int) millis);
                return true;
            }
            catch (ThreadInterruptedException)
            {
                Thread.CurrentThread.Interrupt();
                return false;
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// pecifies whether this Daemon should start automatically at the
        /// initialization time.
        /// </summary>
        private bool m_isAutoStart;

        /// <summary>
        /// The resolution of the system clock in milliseconds.
        /// </summary>
        [NonSerialized]
        private static long m_clockResolutionMillis;

        /// <summary>
        /// Specifies the state of the daemon.
        /// </summary>
        [NonSerialized]
        private volatile DaemonState m_daemonState;

        /// <summary>
        /// Monitor object to coordinate clearing the thread interrupt set by
        /// <see cref="Stop"/> prior to running <see cref="OnExit"/>.
        /// </summary>
        [NonSerialized]
        private readonly object f_exitMonitor = new object();

        /// <summary>
        /// An object that serves as a mutex for this Daemon synchronization.
        /// </summary>
        [NonSerialized]
        private object m_lock;

        /// <summary>
        /// Specifes whether there is work for the daemon to do.
        /// </summary>
        [NonSerialized]
        private bool m_isNotification;

        /// <summary>
        /// Specifies the priority of the daemon's thread.
        /// </summary>
        private ThreadPriority m_priority = ThreadPriority.Normal;

        /// <summary>
        /// The exception (if any) that prevented the daemon from starting
        /// successfully.
        /// </summary>
        [NonSerialized]
        private Exception m_startException;

        /// <summary>
        /// Date/time value that this Daemon's thread has started.
        /// </summary>
        [NonSerialized]
        private long m_startTimestamp;

        /// <summary>
        /// The daemon thread if it is running, or null before the daemon
        /// starts and after the daemon stops.
        /// </summary>
        [NonSerialized]
        private Thread m_thread;

        /// <summary>
        /// Specifies the name of the daemon thread.
        /// </summary>
        private string m_threadName;

        /// <summary>
        /// The number of milliseconds that the daemon will wait for notification.
        /// </summary>
        private long m_waitMillis;

        #endregion
    }

    #region Enum: DaemonState

    /// <summary>
    /// Daemon state enum.
    /// </summary>
    public enum DaemonState
    {
        /// <summary>
        /// State indicating that the daemon has yet to be started.
        /// </summary>
        Initial = 0,

        /// <summary>
        /// State indicating that the daemon is currently starting.
        /// </summary>
        Starting = 1,

        /// <summary>
        /// State indicating that the daemon is ready for operation.
        /// </summary>
        Running = 2,

        /// <summary>
        /// State indicating that the daemon is currently exiting.
        /// </summary>
        Exiting = 3,

        /// <summary>
        /// State indicating that the daemon has exited.
        /// </summary>
        Exited = 4
    }

    #endregion
}