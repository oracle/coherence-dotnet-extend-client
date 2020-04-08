/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;

using Tangosol.Net.Messaging;
using Tangosol.Util;
using Tangosol.Util.Daemon.QueueProcessor;

namespace Tangosol.Net.Cache.Support
{
    /// <summary>
    /// Wrapper for <see cref="CacheEventArgs"/> that implements
    /// <see cref="IRunnable"/> and is aware of cache listeners associated
    /// with wrapped <b>CacheEventArgs</b>.
    /// </summary>
    /// <author>Ana Cikic  2006.10.26</author>
    internal class RunnableCacheEvent : IRunnable
    {
        #region Properties

        /// <summary>
        /// Optional <see cref="Listeners"/> object containing
        /// <see cref="ICacheListener"/> objects.
        /// </summary>
        /// <value>
        /// <b>Listeners</b> object.
        /// </value>
        public virtual Listeners Listeners
        {
            get { return m_listeners; }
            set { m_listeners = value; }
        }

        /// <summary>
        /// Optional <see cref="CacheListenerSupport"/> object
        /// containing <see cref="ICacheListener"/> objects.
        /// </summary>
        /// <value>
        /// <b>CacheListenerSupport</b> object.
        /// </value>
        public virtual CacheListenerSupport CacheListenerSupport
        {
            get { return m_listenerSupport; }
            set { m_listenerSupport = value; }
        }

        /// <summary>
        /// The actual <see cref="CacheEvent"/> to fire.
        /// </summary>
        /// <value>
        /// The actual <b>CacheEventArgs</b> to fire.
        /// </value>
        public virtual CacheEventArgs CacheEvent
        {
            get { return m_cacheEvent; }
            set { m_cacheEvent = value; }
        }

        /// <summary>
        /// Optional <see cref="ICacheListener"/> object.
        /// </summary>
        /// <value>
        /// <b>ICacheListener</b> object.
        /// </value>
        public virtual ICacheListener CacheListener
        {
            get { return m_dictionaryListener; }
            set { m_dictionaryListener = value; }
        }

        #endregion

        #region Factory methods

        private RunnableCacheEvent()
        {}

        /// <summary>
        /// Create RunnableCacheEvent instance with specified <b>CacheEvent</b>
        /// and <b>Listeners</b> objects.
        /// </summary>
        /// <param name="evt">
        /// <b>CacheEvent</b> object.
        /// </param>
        /// <param name="listeners">
        /// <b>Listeners</b> object.
        /// </param>
        /// <returns>
        /// <b>RunnableCacheEvent</b> instance.
        /// </returns>
        public static RunnableCacheEvent Instantiate(CacheEventArgs evt, Listeners listeners)
        {
            Debug.Assert(evt != null && listeners != null);

            RunnableCacheEvent task = new RunnableCacheEvent();
            task.CacheEvent = evt;
            task.Listeners  = listeners;
            return task;
        }

        /// <summary>
        /// Create RunnableCacheEvent instance with specified <b>CacheEvent</b>
        /// and <b>ICacheListener</b> objects.
        /// </summary>
        /// <param name="evt">
        /// <b>CacheEvent</b> object.
        /// </param>
        /// <param name="listener">
        /// <b>ICacheListener</b> object.
        /// </param>
        /// <returns>
        /// <b>RunnableCacheEvent</b> instance.
        /// </returns>
        public static RunnableCacheEvent Instantiate(CacheEventArgs evt, ICacheListener listener)
        {
            Debug.Assert(evt != null && listener != null);

            RunnableCacheEvent task = new RunnableCacheEvent();
            task.CacheEvent    = evt;
            task.CacheListener = listener;
            return task;
        }

        /// <summary>
        /// Create RunnableCacheEvent instance with specified <b>CacheEvent</b>
        /// and <b>CacheListenerSupport</b> objects.
        /// </summary>
        /// <param name="evt">
        /// <b>CacheEvent</b> object.
        /// </param>
        /// <param name="support">
        /// <b>CacheListenerSupport</b> object.
        /// </param>
        /// <returns>
        /// <b>RunnableCacheEvent</b> instance.
        /// </returns>
        public static RunnableCacheEvent Instantiate(CacheEventArgs evt, CacheListenerSupport support)
        {
            Debug.Assert(evt != null && support != null);

            RunnableCacheEvent task = new RunnableCacheEvent();
            task.CacheEvent           = evt;
            task.CacheListenerSupport = support;
            return task;
        }

        #endregion

        #region Dispatch methods

        /// <summary>
        /// Dispatch the specified <see cref="CacheEvent"/> to all
        /// <see cref="Support.CacheListenerSupport.ISynchronousListener"/>
        /// objects and add to the specified <see cref="Queue"/> for deferred
        /// execution for standard ones.
        /// </summary>
        /// <param name="evt">
        /// <b>CacheEvent</b> to dispatch.
        /// </param>
        /// <param name="listeners">
        /// <b>Listeners</b> to which the event is dispatched.
        /// </param>
        /// <param name="queue">
        /// <b>Queue</b> to which event will be added.
        /// </param>
        public static void DispatchSafe(CacheEventArgs evt, Listeners listeners, Queue queue)
        {
            if (listeners != null)
            {
                object[] listenersArray = listeners.ListenersArray;
                for (int i = 0, c = listenersArray.Length; i < c; i++)
                {
                    ICacheListener listener = (ICacheListener) listenersArray[i];
                    if (listener is CacheListenerSupport.ISynchronousListener)
                    {
                        try
                        {
                            CacheListenerSupport.Dispatch(evt, listener);
                        }
                        catch (Exception e)
                        {
                            CacheFactory.Log("An exception occured while dispatching synchronous event:" + evt,
                                             CacheFactory.LogLevel.Error);
                            CacheFactory.Log(e, CacheFactory.LogLevel.Error);
                            CacheFactory.Log("(The exception has been logged and execution is continuing.)",
                                             CacheFactory.LogLevel.Error);
                        }
                    }
                    else
                    {
                        queue.Add(Instantiate(evt, listener));
                    }
                }
            }
        }

        #endregion

        #region IRunnable implementation

        /// <summary>
        /// Dispatch event.
        /// </summary>
        public void Run()
        {
            CacheEventArgs evt   = CacheEvent;
            INamedCache    cache = (INamedCache) evt.Cache;

            if (cache.IsActive)
            {
                CacheListenerSupport support = CacheListenerSupport;
                if (support == null)
                {
                    Listeners listeners = Listeners;
                    if (listeners == null)
                    {
                        CacheListenerSupport.Dispatch(evt, CacheListener);
                    }
                    else
                    {
                        CacheListenerSupport.Dispatch(evt, listeners, true);
                    }
                }
                else
                {
                    support.FireEvent(evt, true);
                }
            }
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Returns sting representation of this object.
        /// </summary>
        /// <returns>
        /// String representation of this object.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + ": " + CacheEvent;
        }

        #endregion

        #region Data members

        /// <summary>
        /// Optional Listeners object containing ICacheListener objects.
        /// </summary>
        private Listeners m_listeners;

        /// <summary>
        /// Optional CacheListenerSupport object containing
        /// ICacheListener objects.
        /// </summary>
        private CacheListenerSupport m_listenerSupport;

        /// <summary>
        /// The actual CacheEventArgs to fire.
        /// </summary>
        private CacheEventArgs m_cacheEvent;

        /// <summary>
        /// Optional ICacheListener object.
        /// </summary>
        private ICacheListener m_dictionaryListener;

        #endregion
    }
}