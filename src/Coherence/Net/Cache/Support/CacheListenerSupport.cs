/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

using Tangosol.Util;
using Tangosol.Util.Collections;

namespace Tangosol.Net.Cache.Support
{
    /// <summary>
    /// This class provides support for advanced
    /// <see cref="ICacheListener"/> functionality.
    /// </summary>
    /// <author>Gene Gleyzer  2003.09.16</author>
    /// <author>Goran Milosavljevic  2006.09.11</author>
    /// <since>Coherence 2.3</since>
    public class CacheListenerSupport
    {
        #region Properties

        /// <summary>
        /// Obtain a collection of all <see cref="IFilter"/> objects that
        /// have associated global listeners.
        /// </summary>
        /// <remarks>
        /// <b>Note</b>: The returned value must be treated as an immutable.
        /// </remarks>
        /// <value>
        /// A collection of all filters that have associated global
        /// listeners.
        /// </value>
        public virtual ICollection Filters
        {
            get
            {
                IDictionary cacheListeners = m_cacheListeners;
                return cacheListeners == null ? new ArrayList() : cacheListeners.Keys;
            }
        }

        /// <summary>
        /// Obtain a collection of all keys that have associated key
        /// listeners.
        /// </summary>
        /// <remarks>
        /// <b>Note</b>: The returned value must be treated as an immutable.
        /// </remarks>
        /// <value>
        /// A collection of all keys that have associated key listeners.
        /// </value>
        public virtual ICollection Keys
        {
            get
            {
                IDictionary cacheKeyListeners = m_cacheKeyListeners;
                return cacheKeyListeners == null ? new ArrayList() : cacheKeyListeners.Keys;
            }
        }

        #endregion

        #region Add/Remove Listener

        /// <summary>
        /// Add a cache listener that receives events based on a filter
        /// evaluation.
        /// </summary>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to add.
        /// </param>
        /// <param name="filter">
        /// An <see cref="IFilter"/> that will be passed
        /// <see cref="CacheEventArgs"/> objects to select from; a
        /// <b>CacheEventArgs</b> will be delivered to the listener only if
        /// the filter evaluates to <b>true</b> for that
        /// <b>CacheEventArgs</b>; <c>null</c> is equivalent to a filter
        /// that alway returns <b>true</b>.
        /// </param>
        /// <param name="isLite">
        /// <b>true</b> to indicate that the <b>CacheEventArgs</b> objects
        /// do not have to include the <b>OldValue</b> and <b>NewValue</b>
        /// property values in order to allow optimizations.
        /// </param>
        public virtual void AddListener(ICacheListener listener, IFilter filter, bool isLite)
        {
            lock (this)
            {
                if (listener != null)
                {
                    IDictionary cacheListeners = m_cacheListeners;
                    if (cacheListeners == null)
                    {
                        cacheListeners = m_cacheListeners = new LiteDictionary();
                    }
                    AddSafeListener(cacheListeners, filter, listener);

                    IDictionary cacheStandardListeners = m_cacheStandardListeners;
                    if (cacheStandardListeners == null)
                    {
                        cacheStandardListeners = m_cacheStandardListeners = new LiteDictionary();
                    }
                    AddListenerState(cacheStandardListeners, filter, listener, isLite);

                    m_optimizationPlan = OptimizationPlan.None;
                    m_cachedListeners  = null;
                }
            }
        }

        /// <summary>
        /// Add a cache listener for a specific key.
        /// </summary>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to add.
        /// </param>
        /// <param name="key">
        /// The key that identifies the <see cref="ICacheEntry"/> for
        /// which to raise events.
        /// </param>
        /// <param name="isLite">
        /// <b>true</b> to indicate that the <b>CacheEventArgs</b> objects
        /// do not have to include the <b>OldValue</b> and <b>NewValue</b>
        /// property values in order to allow optimizations.
        /// </param>
        public virtual void AddListener(ICacheListener listener, object key, bool isLite)
        {
            lock (this)
            {
                if (listener != null)
                {
                    IDictionary cacheKeyListeners = m_cacheKeyListeners;
                    if (cacheKeyListeners == null)
                    {
                        cacheKeyListeners = m_cacheKeyListeners = new HashDictionary();
                    }
                    AddSafeListener(cacheKeyListeners, key, listener);

                    IDictionary cacheStandardKeyListeners = m_cacheStandardKeyListeners;
                    if (cacheStandardKeyListeners == null)
                    {
                        cacheStandardKeyListeners = m_cacheStandardKeyListeners = new LiteDictionary();
                    }
                    AddListenerState(cacheStandardKeyListeners, key, listener, isLite);

                    // if the optimization plan was already to optimize for key
                    // listeners, and the key listener that we just added is the
                    // same as was already present, then keep the current plan,
                    // otherwise reset it
                    bool keepPlan = false;
                    if (m_optimizationPlan == OptimizationPlan.KeyListener)
                    {
                        ICacheListener[] listeners = m_cachedListeners.ListenersArray;
                        if (listeners != null && listeners.Length == 1 && listeners[0] == listener)
                        {
                            keepPlan = true;
                        }
                    }

                    if (!keepPlan)
                    {
                        m_optimizationPlan = OptimizationPlan.None;
                        m_cachedListeners  = null;
                    }
                }
            }
        }

        /// <summary>
        /// Remove a cache listener that previously signed up for events
        /// based on a filter evaluation.
        /// </summary>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to remove.
        /// </param>
        /// <param name="filter">
        /// An <see cref="IFilter"/> used to evaluate events.
        /// </param>
        public virtual void RemoveListener(ICacheListener listener, IFilter filter)
        {
            lock (this)
            {
                if (listener != null)
                {
                    IDictionary cacheListeners = m_cacheListeners;
                    if (cacheListeners != null)
                    {
                        RemoveSafeListener(cacheListeners, filter, listener);
                        if (cacheListeners.Count == 0)
                        {
                            m_cacheListeners = null;
                        }

                        IDictionary cacheStandardListeners = m_cacheStandardListeners;
                        if (cacheStandardListeners != null)
                        {
                            RemoveListenerState(cacheStandardListeners, filter, listener);
                            if (cacheStandardListeners.Count == 0)
                            {
                                m_cacheStandardListeners = null;
                            }
                        }
                    }

                    m_optimizationPlan = OptimizationPlan.None;
                    m_cachedListeners  = null;
                }
            }
        }

        /// <summary>
        /// Remove a cache listener that previously signed up for events
        /// about a specific key.
        /// </summary>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to remove.
        /// </param>
        /// <param name="key">
        /// The key that identifies the entry for which to raise events.
        /// </param>
        public virtual void RemoveListener(ICacheListener listener, object key)
        {
            lock (this)
            {
                if (listener != null)
                {
                    IDictionary cacheListeners = m_cacheKeyListeners;
                    if (cacheListeners != null)
                    {
                        RemoveSafeListener(cacheListeners, key, listener);
                        if (cacheListeners.Count == 0)
                        {
                            m_cacheKeyListeners = null;
                        }

                        IDictionary cacheStandardKeyListeners = m_cacheStandardKeyListeners;
                        if (cacheStandardKeyListeners != null)
                        {
                            RemoveListenerState(cacheStandardKeyListeners, key, listener);
                            if (cacheStandardKeyListeners.Count == 0)
                            {
                                m_cacheStandardKeyListeners = null;
                            }
                        }
                    }

                    // if the optimization plan was already to optimize for key
                    // listeners, and the cached set of key listeners is a set of
                    // exactly one listener, and there are still other keys with
                    // that same listener registered, then keep the current plan,
                    // otherwise reset it
                    bool keepPlan = false;
                    if (m_optimizationPlan == OptimizationPlan.KeyListener)
                    {
                        ICacheListener[] listeners = m_cachedListeners.ListenersArray;
                        if (listeners != null && listeners.Length == 1 && listeners[0] == listener)
                        {
                            // keep the plan if there are any keys still being
                            // listened to
                            keepPlan = (m_cacheKeyListeners != null);
                        }
                    }

                    if (!keepPlan)
                    {
                        m_optimizationPlan = OptimizationPlan.None;
                        m_cachedListeners  = null;
                    }
                }
            }
        }

        /// <summary>
        /// Ensure that the specified cache has an
        /// <see cref="Listeners"/> object associated with the specified key
        /// and add the specified listener to it.
        /// </summary>
        /// <param name="cacheListeners">
        /// Dictionary of cache listeners.
        /// </param>
        /// <param name="key">
        /// Key that specified listener should be associated to.
        /// </param>
        /// <param name="listener">
        /// Listener to associate to specified key.
        /// </param>
        protected static void AddSafeListener(IDictionary cacheListeners, object key, ICacheListener listener)
        {
            Listeners listeners = (Listeners) cacheListeners[key];
            if (listeners == null)
            {
                listeners           = new Listeners();
                cacheListeners[key] = listeners;
            }
            listeners.Add(listener);
        }

        /// <summary>
        /// Ensure that the specified cache has an
        /// <see cref="Listeners"/> object associated with the specified filter
        /// and add the specified listener to it.
        /// </summary>
        /// <param name="cacheListeners">
        /// Dictionary of cache listeners.
        /// </param>
        /// <param name="anyFilter">
        /// Filter that specified listener should be associated to.
        /// </param>
        /// <param name="listener">
        /// Listener to associate to specified key.
        /// </param>
        /// <since>Coherence 3.7.1.8</since>
        protected static void AddSafeListener(IDictionary cacheListeners, IFilter anyFilter, ICacheListener listener)
        {
            Listeners listeners = (Listeners) cacheListeners[anyFilter];
            if (listeners == null)
            {
                listeners = new Listeners();
                if (anyFilter != null)
                {
                    listeners.FiltersArray = new IFilter[] {anyFilter};
                }
                cacheListeners[anyFilter] = listeners;
            }
            listeners.Add(listener);
        }

        /// <summary>
        /// Remove the specified listener from the <see cref="Listeners"/>
        /// object associated with the specified key.
        /// </summary>
        /// <param name="cacheListeners">
        /// Dictionary of cache listeners.
        /// </param>
        /// <param name="key">
        /// Key that determines listener to be removed.
        /// </param>
        /// <param name="listener">
        /// Listener to remove.
        /// </param>
        protected static void RemoveSafeListener(IDictionary cacheListeners, object key, ICacheListener listener)
        {
            Listeners listeners = (Listeners) cacheListeners[key];
            if (listeners != null)
            {
                listeners.Remove(listener);
                if (listeners.IsEmpty)
                {
                    cacheListeners.Remove(key);
                }
            }
        }

        /// <summary>
        /// Add a state information (lite or standard) assosiated with
        /// specified key and listener.
        /// </summary>
        /// <param name="cacheStandardListeners">
        /// Dictionary of cache listeners.
        /// </param>
        /// <param name="key">
        /// Key that the specified listener should be associated to.
        /// </param>
        /// <param name="listener">
        /// Cache listener.
        /// </param>
        /// <param name="isLite">
        /// <b>true</b> if listener is "lite", <b>false</b> otherwise.
        /// </param>
        protected static void AddListenerState(IDictionary cacheStandardListeners, object key,
                                               ICacheListener listener, bool isLite)
        {
            ICollection standard = (ICollection) cacheStandardListeners[key];
            if (isLite)
            {
                if (standard != null)
                {
                    CollectionUtils.Remove(standard, listener);
                }
            }
            else
            {
                if (standard == null)
                {
                    standard                    = new ArrayList();
                    cacheStandardListeners[key] = standard;
                }
                CollectionUtils.Add(standard, listener);
            }
        }

        /// <summary>
        /// Remove a state information (lite or standard) associated with
        /// specified key and listener.
        /// </summary>
        /// <param name="cacheStandardListeners">
        /// Dictionary of cache listeners.
        /// </param>
        /// <param name="key">
        /// Key that the specified listener is associated to.
        /// </param>
        /// <param name="listener">
        /// Cache listener.
        /// </param>
        protected static void RemoveListenerState(IDictionary cacheStandardListeners, object key, ICacheListener listener)
        {
            ICollection standard = (ICollection) cacheStandardListeners[key];
            if (standard != null)
            {
                CollectionUtils.Remove(standard, listener);
                if (standard.Count == 0)
                {
                    cacheStandardListeners.Remove(key);
                }
            }
        }

        /// <summary>
        /// Remove all signed up listeners.
        /// </summary>
        public virtual void Clear()
        {
            lock (this)
            {
                m_cacheListeners            = null;
                m_cacheKeyListeners         = null;
                m_cacheStandardListeners    = null;
                m_cacheStandardKeyListeners = null;

                m_optimizationPlan = OptimizationPlan.NoListeners;
                m_cachedListeners  = null;
            }
        }

        #endregion

        #region Get/Contains Listener

        /// <summary>
        /// Checks whether or not this CacheListenerSupport object
        /// contains any listeners.
        /// </summary>
        /// <returns>
        /// <b>true</b> if there are no listeners encapsulated by this
        /// CacheListenerSupport object.
        /// </returns>
        public virtual bool IsEmpty()
        {
            return m_cacheListeners == null && m_cacheKeyListeners == null;
        }

        /// <summary>
        /// Checks whether or not this CacheListenerSupport object
        /// contains any listeners for a given filter.
        /// </summary>
        /// <param name="filter">
        /// The <see cref="IFilter"/>.
        /// </param>
        /// <returns>
        /// <b>true</b> if there are no listeners for the specified filter
        /// encapsulated by this CacheListenerSupport object.
        /// </returns>
        public virtual bool IsEmpty(IFilter filter)
        {
            IDictionary dictionaryListeners = m_cacheListeners;
            return dictionaryListeners == null || !dictionaryListeners.Contains(filter);
        }

        /// <summary>
        /// Checks whether or not this CacheListenerSupport object
        /// contains any listeners for a given key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// <b>true</b> if there are no listeners for the specified filter
        /// encapsulated by this CacheListenerSupport object.
        /// </returns>
        public virtual bool IsEmpty(object key)
        {
            IDictionary dictionaryListeners = m_cacheKeyListeners;
            return dictionaryListeners == null || !dictionaryListeners.Contains(key);
        }

        /// <summary>
        /// Checks whether or not this CacheListenerSupport object
        /// contains any standard (not lite) listeners for a given filter.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// <b>true</b> if there are no standard listeners for the specified
        /// filter encapsulated by this CacheListenerSupport object.
        /// </returns>
        public virtual bool ContainsStandardListeners(IFilter filter)
        {
            IDictionary dictionaryStandardListeners = m_cacheStandardListeners;
            if (dictionaryStandardListeners == null)
            {
                return false;
            }

            ICollection standard = (ICollection) dictionaryStandardListeners[filter];
            return standard != null && standard.Count != 0;
        }

        /// <summary>
        /// Checks whether or not this CacheListenerSupport object
        /// contains any standard (not lite) listeners for a given key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// <b>true</b> if there are no standard listeners for the specified
        /// filter encapsulated by this CacheListenerSupport object.
        /// </returns>
        public virtual bool ContainsStandardListeners(object key)
        {
            IDictionary dictionaryStandardListeners = m_cacheStandardKeyListeners;
            if (dictionaryStandardListeners == null)
            {
                return false;
            }

            ICollection standard = (ICollection) dictionaryStandardListeners[key];
            return standard != null && standard.Count != 0;
        }

        /// <summary>
        /// Obtain the <see cref="Listeners"/> object for a given filter.
        /// </summary>
        /// <remarks>
        /// <b>Note</b>: The returned value must be treated as an immutable.
        /// </remarks>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <b>Listeners</b> object for the filter; <c>null</c> if none
        /// exists.
        /// </returns>
        public virtual Listeners GetListeners(IFilter filter)
        {
            lock (this)
            {
                // this method is synchronized because the underlying map implementation
                // is not thread safe for "get" operations: it could blow up (LiteDictionary)
                // or return null (Hashtable) while there is a valid entry
                IDictionary cacheListeners = m_cacheListeners;
                return cacheListeners == null ? null : (Listeners) cacheListeners[filter];
            }
        }

        /// <summary>
        /// Obtain the <see cref="Listeners"/> object for a given key.
        /// </summary>
        /// <remarks>
        /// <b>Note</b>: The returned value must be treated as an immutable.
        /// </remarks>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <b>Listeners</b> object for the key; <c>null</c> if none
        /// exists.
        /// </returns>
        public virtual Listeners GetListeners(object key)
        {
            lock (this)
            {
                // this method is synchronized because the underlying map implementation
                // is not thread safe for "get" operations: it could blow up (LiteDictionary)
                // or return null (Hashtable) while there is a valid entry
                IDictionary cacheKeyListeners = m_cacheKeyListeners;
                return cacheKeyListeners == null ? null : (Listeners) cacheKeyListeners[key];
            }
        }

        #endregion

        #region CacheEvent dispatch methods

        /// <summary>
        /// Dispatch the <see cref="CacheEventArgs"/> to the specified
        /// <see cref="Listeners"/> collection.
        /// </summary>
        /// <remarks>
        /// This call is equivalent to
        /// <pre>
        /// Dispatch(evt, listeners, true);
        /// </pre>
        /// </remarks>
        /// <param name="evt">
        /// The <b>CacheEventArgs</b>.
        /// </param>
        /// <param name="listeners">
        /// The <see cref="Listeners"/> collection.
        /// </param>
        /// <exception cref="InvalidCastException">
        /// If any of the targets is not an instance of
        /// <see cref="ICacheListener"/> interface.
        /// </exception>
        public static void Dispatch(CacheEventArgs evt, Listeners listeners)
        {
            Dispatch(evt, listeners, true);
        }

        /// <summary>
        /// Dispatch the <see cref="CacheEventArgs"/> to the specified
        /// <see cref="Listeners"/> collection.
        /// </summary>
        /// <param name="evt">
        /// The <b>CacheEventArgs</b>.
        /// </param>
        /// <param name="listeners">
        /// The <see cref="Listeners"/> collection.
        /// </param>
        /// <param name="isStrict">
        /// If <b>true</b> then any <b>Exception</b> thrown by event
        /// handlers stops all further event processing and the exception is
        /// re-thrown; if <b>false</b> then all exceptions are logged and the
        /// process continues.
        /// </param>
        /// <exception cref="InvalidCastException">
        /// If any of the targets is not an instance of
        /// <see cref="ICacheListener"/> interface.
        /// </exception>
        public static void Dispatch(CacheEventArgs evt, Listeners listeners, bool isStrict)
        {
            if (listeners != null)
            {
                foreach (ICacheListener listener in listeners.ListenersArray)
                {
                    try
                    {
                        if (evt.ShouldDispatch(listener))
                        {
                            Dispatch(evt, listener);
                        }
                    }
                    catch (Exception e)
                    {
                        if (isStrict)
                        {
                            throw;
                        }
                        else
                        {
                            CacheFactory.Log(e, CacheFactory.LogLevel.Error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dispatch the <see cref="CacheEventArgs"/> to the specified
        /// <see cref="ICacheListener"/>.
        /// </summary>
        /// <param name="evt">
        /// The <b>CacheEventArgs</b>.
        /// </param>
        /// <param name="listener">
        /// The listener.
        /// </param>
        public static void Dispatch(CacheEventArgs evt, ICacheListener listener)
        {
            if (evt.ShouldDispatch(listener))
            {
                switch (evt.EventType)
                {
                    case CacheEventType.Inserted:
                        listener.EntryInserted(evt);
                        break;

                    case CacheEventType.Updated:
                        listener.EntryUpdated(evt);
                        break;

                    case CacheEventType.Deleted:
                        listener.EntryDeleted(evt);
                        break;
                }
            }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Collect all <see cref="Listeners"/> that should be notified for a
        /// given event.
        /// </summary>
        /// <remarks>
        /// <b>Note</b>: The returned value must be treated as an immutable.
        /// </remarks>
        /// <param name="cacheEvent">
        /// The <see cref="CacheEventArgs"/> object.
        /// </param>
        /// <returns>
        /// The <b>Listeners</b> object containing the relevant listeners.
        /// </returns>
        public virtual Listeners CollectListeners(CacheEventArgs cacheEvent)
        {
            switch (m_optimizationPlan)
            {

                case OptimizationPlan.None:
                default:
                    lock (this)
                    {
                        // put a plan together
                        IDictionary allListeners = m_cacheListeners;
                        IDictionary keyListeners = m_cacheKeyListeners;
                        if (allListeners == null || allListeners.Count == 0)
                        {
                            // no standard listeners; check for key listeners
                            if (keyListeners == null || keyListeners.Count == 0)
                            {
                                m_optimizationPlan = OptimizationPlan.NoListeners;
                                m_cachedListeners  = null;
                            }
                            else
                            {
                                // can only do key optimization if all keys have
                                // the same set of listeners registered
                                ICacheListener[] listenersPrev = null;
                                foreach (Listeners listeners in keyListeners.Values)
                                {
                                    if (listenersPrev == null)
                                    {
                                        // assume that they are all the same
                                        m_optimizationPlan = OptimizationPlan.KeyListener;
                                        m_cachedListeners  = listeners;

                                        listenersPrev = listeners.ListenersArray;
                                    }
                                    else
                                    {
                                        ICacheListener[] listenersCurrent      = listeners.ListenersArray;
                                        int              listenersCurrentCount = listenersCurrent.Length;
                                        int              listenersPrevCount    = listenersPrev.Length;
                                        bool             optimize              = listenersCurrentCount == listenersPrevCount;
                                        if (optimize)
                                        {
                                            for (int i = 0; i < listenersCurrentCount; ++i)
                                            {
                                                if (listenersCurrent[i] != listenersPrev[i])
                                                {
                                                    // assumption was incorrect -- some
                                                    // keys have different listeners
                                                    optimize = false;
                                                    break;
                                                }
                                            }
                                        }
                                        if (!optimize)
                                        {
                                            m_optimizationPlan = OptimizationPlan.NoOptimize;
                                            m_cachedListeners  = null;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        // there are "all" listeners
                        else
                        {
                            // assume no optimizations
                            m_optimizationPlan = OptimizationPlan.NoOptimize;
                            m_cachedListeners  = null;

                            // it is possible to optimize if there are no key
                            // listeners AND no filtered listeners
                            if (keyListeners == null || keyListeners.Count == 0)
                            {
                                // check if there is only one listener and it has
                                // no filter
                                if (allListeners.Count == 1)
                                {
                                    Listeners listeners = (Listeners) allListeners[null];
                                    if (listeners != null)
                                    {
                                        m_optimizationPlan = OptimizationPlan.AllListener;
                                        m_cachedListeners  = listeners;
                                    }
                                }
                            }
                        }

                        Debug.Assert(m_optimizationPlan != OptimizationPlan.None);
                    }
                    return CollectListeners(cacheEvent);

                case OptimizationPlan.NoListeners:
                    return NO_LISTENERS;

                case OptimizationPlan.AllListener:
                    return m_cachedListeners;

                case OptimizationPlan.KeyListener:
                    return m_cacheKeyListeners.Contains(cacheEvent.Key) ? m_cachedListeners : NO_LISTENERS;

                case OptimizationPlan.NoOptimize:
                    // fall through to the full implementation
                    break;
            }

            Listeners listeners2 = new Listeners();

            // add global listeners
            IDictionary cacheListeners = m_cacheListeners;
            if (cacheListeners != null)
            {
                CacheEventArgs evt = UnwrapEvent(cacheEvent);
                if (evt is FilterEventArgs)
                {
                    FilterEventArgs evtFilter = (FilterEventArgs) evt;
                    IFilter[]       aFilter   = evtFilter.Filters;

                    listeners2.FiltersArray = aFilter;
                }
                IFilter[] filters = listeners2.FiltersArray;

                if (filters == null)
                {
                    // the server sent an event without a specified filter list;
                    // attempt to match it to any registered filter-based listeners
                    object[] entries;
                    lock (this)
                    {
                        entries = CollectionUtils.ToArray(cacheListeners);
                    }

                    ArrayList listFilters = null;
                    for (int i = 0, c = entries.Length; i < c; i++)
                    {
                        DictionaryEntry entry = (DictionaryEntry) entries[i];

                        IFilter filter = (IFilter) entry.Key;
                        if (filter == null || EvaluateEvent(filter, cacheEvent))
                        {
                            listeners2.AddAll((Listeners) entry.Value);

                            if (filter != null)
                            {
                                if (listFilters == null)
                                {
                                    listFilters = new ArrayList();
                                }
                                listFilters.Add(filter);
                            }
                        }
                    }
                    if (listFilters != null)
                    {
                        IFilter[] newFilters = new IFilter[listFilters.Count];
                        listFilters.CopyTo(newFilters);
                        listeners2.FiltersArray = newFilters;
                    }
                }
                else
                {
                    lock (this)
                    {
                        foreach (IFilter filter in filters)
                        {
                            listeners2.AddAll((Listeners) cacheListeners[filter]);
                        }
                    }
                }
            }

            // add key listeners, only if the event is not transformed (COH-9355)
            IDictionary cacheKeyListeners2 = m_cacheKeyListeners;
            if (cacheKeyListeners2 != null && !IsTransformedEvent(cacheEvent))
            {
                Listeners listeners = (Listeners) cacheKeyListeners2[cacheEvent.Key];
                if (listeners != null)
                {
                    listeners2.AddAll(listeners);
                }
            }

            return listeners2;
        }

        /// <summary>
        /// Fire the specified cache event.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/>.
        /// </param>
        /// <param name="isStrict">
        /// If <b>true</b> then any <b>Exception</b> thrown by event
        /// handlers stops all further event processing and the exception
        /// is re-thrown; if <b>false</b> then all exceptions are logged and
        /// the process continues.
        /// </param>
        public virtual void FireEvent(CacheEventArgs evt, bool isStrict)
        {
            Listeners listeners = CollectListeners(evt);
            Dispatch(EnrichEvent(evt, listeners), listeners, isStrict);
        }

        /// <summary>
        /// Convert the specified cache event into another
        /// <see cref="CacheEventArgs"/> that ensures the lazy event data
        /// conversion using the specified converters.
        /// </summary>
        /// <param name="evt">
        /// The cache event.
        /// </param>
        /// <param name="cacheConv">
        /// The source for the converted event.
        /// </param>
        /// <param name="convKey">
        /// (Optional) the key <see cref="IConverter"/>.
        /// </param>
        /// <param name="convVal">
        /// (Optional) the value <see cref="IConverter"/>.
        /// </param>
        /// <returns>
        /// The converted <b>CacheEventArgs</b> object.
        /// </returns>
        public static CacheEventArgs ConvertEvent(CacheEventArgs evt, IObservableCache cacheConv,
            IConverter convKey, IConverter convVal)
        {
            if (convKey == null)
            {
                convKey = NullImplementation.GetConverter();
            }
            if (convVal == null)
            {
                convVal = NullImplementation.GetConverter();
            }

            return ConverterCollections.GetCacheEventArgs(cacheConv, evt, convKey, convVal);
        }

        /// <summary>
        /// Enrich the event with the filters associated with the listener.
        /// </summary>
        /// <param name="evt">
        /// The cache event.
        /// </param>
        /// <param name="listners">
        /// The Listeners object that has the associated filters.
        /// </param>
        /// <returns>
        /// The enriched <b>FilterEventArgs</b> object.
        /// </returns>
        /// <since>Coherence 3.7.1.8</since>
        public static CacheEventArgs EnrichEvent(CacheEventArgs evt, Listeners listners)
        {
            if (!(evt is FilterEventArgs))
            {
                IFilter[] filters = listners.FiltersArray;
                if (filters != null)
                {
                   evt = new FilterEventArgs(evt, filters);
                }
            }
            return evt;
        }

        /// <summary>
        /// Unwrap the specified map event and return the underlying source event.
        /// </summary>
        /// <param name="evt">
        /// The event to unwrap.
        /// </param>
        /// <returns>
        /// The unwrapped event.
        /// </returns>
        /// <since>Coherence 3.7.1.9</since>
        public static CacheEventArgs UnwrapEvent(CacheEventArgs evt)
        {
            while (evt is ConverterCollections.ConverterCacheEventArgs)
            {
                evt = ((ConverterCollections.ConverterCacheEventArgs) evt).CacheEvent;
            }

            return evt;
        }

        /// <summary>
        /// Check if the given listener is a PrimingListener or if it wraps one.
        /// </summary>
        /// <param name="listener">
        /// Cache listener to check.
        /// </param>
        /// <returns>
        /// true iff the listener is a PrimingListener or wraps one.
        /// </returns>
        /// <since>12.2.1</since>
        public static bool IsPrimingListener(ICacheListener listener)
        {
           return UnwrapListener(listener) is IPrimingListener;
        }

        /// <summary>
        /// Unwrap the specified cache listener and return the underlying cache listener.
        /// </summary>
        /// <param name="listener">
        /// Cache listener to unwrap.
        /// </param>
        /// <returns>
        /// the unwrapped listener.
        /// </returns>
        /// <since>12.2.1</since>
        protected static ICacheListener UnwrapListener(ICacheListener listener)
        {
            while (true)
            {
                if (listener is ConverterCollections.ConverterCacheListener)
                {
                    listener = ((ConverterCollections.ConverterCacheListener) listener).CacheListener;
                }
                else if (listener is WrapperSynchronousListener &&
                        !(listener is IPrimingListener))
                {
                    listener = ((WrapperSynchronousListener) listener).CacheListener;
                }
                else
                {
                    return listener;
                }
            }
        }

        /// <summary>
        /// Evaluate whether or not the specified event should be delivered to the
        /// listener associated with the specified filter.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <param name="evt">
        /// The event.
        /// </param>
        /// <returns>
        /// true iff the event should be delivered to the corresponding listener
        /// </returns>
        /// <since>Coherence 3.7.1.9</since>
        protected bool EvaluateEvent(IFilter filter, CacheEventArgs evt)
        {
            if (evt.TransformState == CacheEventArgs.TransformationState.NON_TRANSFORMABLE &&
                filter is ICacheEventTransformer)
            {
                // if the event is marked as non-transformable, ensure that it does not
                // get delivered to listeners associated with transformer-filters
                return false;
            }

            return filter.Evaluate(evt);
        }

        /// <summary>
        /// Return true iff the specified event represents a transformed CacheEvent.
        /// </summary>
        /// <param name="evt">
        /// The event to tests
        /// </param>
        /// <returns>
        /// true iff the event has been transformed
        /// </returns>
        protected bool IsTransformedEvent(CacheEventArgs evt)
        {
            return evt.TransformState == CacheEventArgs.TransformationState.TRANSFORMED;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Provide a string representation of the CacheListenerSupport
        /// object.
        /// </summary>
        /// <returns>
        /// A human-readable description of the CacheListenerSupport
        /// instance.
        /// </returns>
        public override string ToString()
        {
            lock (this)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("Global listeners:");
                if (m_cacheListeners == null)
                {
                    sb.Append(" none");
                }
                else
                {
                    foreach (IFilter filter in m_cacheListeners.Keys)
                    {
                        sb.Append("\n  Filter=").Append(filter).Append("; lite=").Append(!ContainsStandardListeners(filter));
                    }
                }

                sb.Append("\nKey listeners:");
                if (m_cacheKeyListeners == null)
                {
                    sb.Append(" none");
                }
                else
                {
                    foreach (object key in m_cacheKeyListeners.Keys)
                    {
                        sb.Append("\n  Key=").Append(key).Append("; lite=").Append(!ContainsStandardListeners(key));
                    }
                }

                return sb.ToString();
            }
        }

        #endregion

        #region Interface: ISynchronousListener

        /// <summary>
        /// A tag interface indicating that tagged
        /// <see cref="ICacheListener"/> implementation has to receive
        /// the <see cref="CacheEventArgs"/> notifications in a synchronous
        /// manner.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Consider an <b>ICacheListener</b> that subscribes to receive
        /// notifications for distributed (partitioned) cache. All events
        /// notifications are received by the service thread and immediately
        /// queued to be processed by the dedicated event dispatcher thread.
        /// This makes it impossible to differentiate between the event
        /// caused by the updates made by this thread and any other thread.
        /// Forcing the events to be processed on the service thread
        /// guarantees that by the time "put" or "remove" requests return to
        /// the caller all relevant cache event notification have been
        /// processed (due to the "in order delivery" rule enforced by the
        /// TCMP).</p>
        /// <p>
        /// This interface should be considered as a very advanced feature,
        /// so an <b>ICacheListener</b> implementation that is tagged as
        /// an ISynchronousListener must exercise extreme caution during event
        /// processing since any delay with return or unhandled exception
        /// will cause a delay or complete shutdown of the corresponding
        /// cache service.</p>
        /// <p>
        /// <b>Note:</b> The contract by the event producer in respect to the
        /// ISynchronousListener is somewhat weaker then the general one.
        /// First, the ISynchronousListener implementaion should make no
        /// assumptions about the event source obtained by
        /// <see cref="CacheEventArgs.Cache"/>.</p>
        /// <p>
        /// Second, in the event of [automatic] service restart, the listener
        /// has to be re-registered manually.</p>
        /// <p>
        /// Third, and the most important, no calls against the
        /// <see cref="INamedCache"/> are allowed during the synchronous
        /// event processing (the only exception being a call to remove the
        /// listener itself).</p>
        /// </remarks>
        public interface ISynchronousListener : Tangosol.Util.ISynchronousListener, ICacheListener
        {}

        #endregion

        #region Interface: IPrimingListener

        /// <summary>
        /// A tag interface indicating that tagged
        /// <see cref="ICacheListener"/> implementation receives "lite"
        /// the <see cref="CacheEventArgs"/> notifications (carrying only a key) 
        /// and generates a "priming" event when registered.
        /// </summary>
        /// <since>12.2.1</since>
        public interface IPrimingListener : ISynchronousListener
        {}

        #endregion

        #region Inner class: WrapperSynchronousListener

        /// <summary>
        /// A wrapper class that turns the specified
        /// <see cref="ICacheListener"/> into a synchronous listener.
        /// </summary>
        /// <since>12.2.1</since>
        public class WrapperSynchronousListener : MultiplexingCacheListener, ISynchronousListener
        {
            /// <summary>
            /// Counstruct WrapperSynchronousListener.
            /// </summary>
            /// <param name="listener">
            /// The wrapped <b>ICacheListener</b>.
            /// </param>
            public WrapperSynchronousListener(ICacheListener listener)
            {
                Debug.Assert(listener != null);
                m_listener = listener;
            }

            /// <summary>
            /// Invoked when a cache entry has been inserted, updated or deleted.
            /// </summary>
            /// <param name="evt">
            /// The cache event.
            /// </param>
            protected override void OnCacheEvent(CacheEventArgs evt)
            {
                evt.Dispatch(m_listener);
            }

            #region Properties

            /// <summary>
            /// Gets the underlying <see cref="ICacheListener"/> object.
            /// </summary>
            /// <value>
            /// An <b>ICacheListener</b> object.
            /// </value>
            public virtual ICacheListener CacheListener
            {
                get { return m_listener; }
            }

            #endregion

            /// <summary>
            /// Determine a hash value for the WrapperSynchronousListener
            /// object.
            /// </summary>
            /// <returns>
            /// An integer hash value for this WrapperSynchronousListener.
            /// </returns>
            public override int GetHashCode()
            {
                return m_listener.GetHashCode();
            }

            /// <summary>
            /// Compare the WrapperSynchronousListener with another object to
            /// determine equality.
            /// </summary>
            /// <param name="o">
            /// The object to compare to.
            /// </param>
            /// <returns>
            /// <b>true</b> if this WrapperSynchronousListener and the passed
            /// object are equivalent listeners.
            /// </returns>
            public override bool Equals(object o)
            {
                if (o != null && o.GetType() == this.GetType())
                {
                    WrapperSynchronousListener that = (WrapperSynchronousListener) o;
                    return m_listener.Equals(that.m_listener);
                }
                return false;
            }

            /// <summary>
            /// Wrapped ICacheListener.
            /// </summary>
            private ICacheListener m_listener;
        }

        #endregion

        #region Inner class: WrapperPrimingListener

        /// <summary>
        /// A wrapper class that turns the specified
        /// <see cref="ICacheListener"/> into a priming listener.
        /// </summary>
        /// <since>12.2.1</since>
        public class WrapperPrimingListener : WrapperSynchronousListener, IPrimingListener
        {
             /// <summary>
            /// Counstruct WrapperPrimingListener.
            /// </summary>
            /// <param name="listener">
            /// The wrapped <b>ICacheListener</b>.
            /// </param>
            public WrapperPrimingListener(ICacheListener listener)
                : base(listener)
            {}
        }
        
        #endregion

        #region Enum: OptimizationPlan

        /// <summary>
        /// The values for CacheListenerSupport optimization plan, which
        /// indicates the fastest way to put together a list of listeners.
        /// </summary>
        internal protected enum OptimizationPlan
        {
            /// <summary>
            /// A plan has not yet been formed.
            /// </summary>
            None = 0,

            /// <summary>
            /// There are no listeners.
            /// </summary>
            NoListeners = 1,

            /// <summary>
            /// There is one all-keys non-filtered listener.
            /// </summary>
            AllListener = 2,

            /// <summary>
            /// There is one key listener (even if for multiple keys).
            /// </summary>
            KeyListener = 3,

            /// <summary>
            /// There is no optimized plan, so just use the default approach.
            /// </summary>
            NoOptimize = 4
        }

        #endregion

        #region Data members

        /// <summary>
        /// The collections of ICacheListener objects that have signed up for
        /// notifications from an IObservableCache implementation keyed by
        /// the corresponding IFilter objects.
        /// </summary>
        protected IDictionary m_cacheListeners;

        /// <summary>
        /// The collections of ICacheListener objects that have signed up for
        /// key based notifications from an IObservableCache implementation
        /// keyed by the corresponding key objects.
        /// </summary>
        protected IDictionary m_cacheKeyListeners;

        /// <summary>
        /// The subset of standard (not lite) global listeners.
        /// </summary>
        /// <remarks>
        /// The keys are the IFilter objects, the values are collections of
        /// corresponding standard listeners.
        /// </remarks>
        protected IDictionary m_cacheStandardListeners;

        /// <summary>
        /// The subset of standard (not lite) key listeners.
        /// </summary>
        /// <remarks>
        /// The keys are the key objects, the values are collections of
        /// corresponding standard listeners.
        /// </remarks>
        protected IDictionary m_cacheStandardKeyListeners;

        /// <summary>
        /// The optimization plan which indicates the fastest way to put
        /// together a list of listeners.
        /// </summary>
        protected OptimizationPlan m_optimizationPlan;

        /// <summary>
        /// A cached list of Listeners.
        /// </summary>
        protected Listeners m_cachedListeners;

        /// <summary>
        /// An empty list of Listeners.
        /// </summary>
        /// <remarks>
        /// Because this is a theoretically mutable object that is used as a
        /// return value, it is purposefully not static.
        /// </remarks>
        protected readonly Listeners NO_LISTENERS = new Listeners();

        #endregion
    }
}