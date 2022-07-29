/*
 * Copyright (c) 2000, 2022, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Diagnostics;

using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// An extension of the <see cref="CacheEventArgs"/> which may
    /// carry no values (old or new), but instead holds on an array of
    /// <see cref="IFilter"/> objects being the "cause" of the event.
    /// </summary>
    public class FilterEventArgs : CacheEventArgs
    {
        #region Properties

        /// <summary>
        /// Return an array of filters that are the cause of this event.
        /// </summary>
        /// <value>
        /// An array of filters.
        /// </value>
        public virtual IFilter[] Filters
        {
            get { return m_filters; }
        }

        /// <summary>
        /// Return the FilterEventArgs.
        /// </summary>
        /// <value>
        /// A FilterEventArgs.
        /// </value>
        /// <since>Coherence 3.7.1.8</since>
        public virtual CacheEventArgs Event
        {
            get { return m_event; }
            set { m_event = value; }
        }

        /// <summary>
        /// Returns the filter event key.
        /// </summary>
        /// <value>
        /// The event key.
        /// </value>
        /// <since>Coherence 3.7.1.8</since>
        public override object Key
        {
            get { return m_event == null ? base.Key : m_event.m_key; }
        }

        /// <summary>
        /// Returns the filter event old value.
        /// </summary>
        /// <value>
        /// The old value.
        /// </value>
        /// <since>Coherence 3.7.1.8</since>
        public override object OldValue
        {
            get { return m_event == null ? base.OldValue : m_event.m_valueOld; }
        }

        /// <summary>
        /// Returns the filter event new value.
        /// </summary>
        /// <value>
        /// The old value.
        /// </value>
        /// <since>Coherence 3.7.1.8</since>
        public override object NewValue
        {
            get { return m_event == null ? base.NewValue : m_event.m_valueNew; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new lite (no values are speicifed) FilterEventArgs.
        /// </summary>
        /// <param name="map">
        /// The <see cref="IObservableCache"/> object that fired the
        /// event.
        /// </param>
        /// <param name="id">
        /// This event's id.
        /// </param>
        /// <param name="key">
        /// The key into the cache.
        /// </param>
        /// <param name="isSynthetic">
        /// <b>true</b> if the event is caused by the cache internal
        /// processing such as eviction or loading.
        /// </param>
        /// <param name="filters">
        /// An array of filters that caused this event.
        /// </param>
        public FilterEventArgs(IObservableCache map, CacheEventType id, object key, bool isSynthetic, IFilter[] filters)
            : this(map, id, key, null, null, isSynthetic, TransformationState.TRANSFORMABLE, false, filters)
        {}

        /// <summary>
        /// Constructs a new FilterEventArgs.
        /// </summary>
        /// <param name="cache">
        /// The <see cref="IObservableCache"/> object that fired the
        /// event.
        /// </param>
        /// <param name="type">
        /// This event's type.
        /// </param>
        /// <param name="key">
        /// The key into the cache.
        /// </param>
        /// <param name="valueOld">
        /// The old value.
        /// </param>
        /// <param name="valueNew">
        /// The new value.
        /// </param>
        /// <param name="isSynthetic">
        /// <b>true</b> if the event is caused by the cache internal
        /// processing such as eviction or loading.
        /// </param>
        /// <param name="filters">
        /// An array of filters caused this event.
        /// </param>
        public FilterEventArgs(IObservableCache cache, CacheEventType type,
                               object key, object valueOld, object valueNew,
                               bool isSynthetic, IFilter[] filters)
                : base(cache, type, key, valueOld, valueNew, isSynthetic,
                       TransformationState.TRANSFORMABLE, false)
        {
            Debug.Assert(filters != null);
            m_filters = filters;
        }

        /// <summary>
        /// Constructs a new FilterEventArgs.
        /// </summary>
        /// <param name="cache">
        /// The IObservableCache object that fired the event
        /// </param>
        /// <param name="type">
        /// This event's type.
        /// </param>
        /// <param name="key">
        /// The key into the cache.
        /// </param>
        /// <param name="valueOld">
        /// The old value.
        /// </param>
        /// <param name="valueNew">
        /// The new value.
        /// </param>
        /// <param name="isSynthetic">
        /// <b>true</b> if the event is caused by the cache internal
        /// processing such as eviction or loading.
        /// </param>
        /// <param name="transformState">
        /// The TransformationState state describing
        /// how this event has been or should be transformed.
        /// </param>
        /// <param name="filters">
        /// An array of filters that caused this event.
        /// </param>
        /// <since>Coherence 3.7.1.9</since>
        public FilterEventArgs(IObservableCache cache, CacheEventType type, object key,
                               object valueOld, object valueNew,
                               bool isSynthetic, TransformationState transformState,
                               IFilter[] filters)
            : base(cache, type, key, valueOld, valueNew, isSynthetic)
        {
            Debug.Assert(filters != null);
            m_filters        = filters;
            m_event          = null;
            m_transformState = transformState;
        }

        /// <summary>
        /// Constructs a new FilterEventArgs.
        /// </summary>
        /// <param name="cache">
        /// The IObservableCache object that fired the event
        /// </param>
        /// <param name="type">
        /// This event's type.
        /// </param>
        /// <param name="key">
        /// The key into the cache.
        /// </param>
        /// <param name="valueOld">
        /// The old value.
        /// </param>
        /// <param name="valueNew">
        /// The new value.
        /// </param>
        /// <param name="isSynthetic">
        /// <b>true</b> if the event is caused by the cache internal
        /// processing such as eviction or loading.
        /// </param>
        /// <param name="transformState">
        /// The TransformationState state describing
        /// how this event has been or should be transformed.
        /// </param>
        /// <param name="filters">
        /// An array of filters that caused this event.
        /// </param>
        /// <param name="isPriming">
        /// <b>true</b> if the event is a priming event.
        /// </param>
        /// <since>Coherence 12.2.1.3.2</since>
        public FilterEventArgs(IObservableCache cache, CacheEventType type, object key,
                               object valueOld, object valueNew,
                               bool isSynthetic, TransformationState transformState,
                               bool isPriming, IFilter[] filters)
            : base(cache, type, key, valueOld, valueNew, isSynthetic, transformState, isPriming)
        {
            Debug.Assert(filters != null);
            m_filters = filters;
            m_event = null;
        }

        /// <summary>
        /// Constructs a new FilterEventArgs.
        /// </summary>
        /// <param name="cache">
        /// The IObservableCache object that fired the event
        /// </param>
        /// <param name="type">
        /// This event's type.
        /// </param>
        /// <param name="key">
        /// The key into the cache.
        /// </param>
        /// <param name="valueOld">
        /// The old value.
        /// </param>
        /// <param name="valueNew">
        /// The new value.
        /// </param>
        /// <param name="isSynthetic">
        /// <b>true</b> if the event is caused by the cache internal
        /// processing such as eviction or loading.
        /// </param>
        /// <param name="transformState">
        /// The TransformationState state describing
        /// how this event has been or should be transformed.
        /// </param>
        /// <param name="isPriming">
        /// <b>true</b> if the event is a priming event.
        /// </param>
        /// <param name="isExpired">
        /// <b>true</b> if the event is an expired event.
        /// </param>
        /// <param name="filters">
        /// An array of filters that caused this event.
        /// </param>
        /// <since>14.1.1.10</since>
        public FilterEventArgs(IObservableCache cache, CacheEventType type, object key,
            object valueOld, object valueNew,
            bool isSynthetic, TransformationState transformState,
            bool isPriming, bool isExpired, IFilter[] filters)
            : base(cache, type, key, valueOld, valueNew, isSynthetic, transformState, isPriming, isExpired)
        {
            Debug.Assert(filters != null);
            m_filters = filters;
            m_event = null;
        }

        /// <summary>
        /// Constructs a new FilterEventArgs from another FilterEventArgs.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="FilterEventArgs"/> object that fired the
        /// event.
        /// </param>
        /// <param name="filters">
        /// An array of filters that caused this event.
        /// </param>
        /// <since>Coherence 3.7.1.8</since>
        public FilterEventArgs(CacheEventArgs evt, IFilter[] filters)
                : this(evt.m_source, evt.m_eventType, evt.Key, evt.OldValue, evt.NewValue,
                       evt.IsSynthetic, evt.TransformState, evt.IsPriming, evt.IsExpired, filters)
        {
            m_event = evt;
        }

        #endregion

        #region Data members

        /// <summary>
        /// Filters that caused the event.
        /// </summary>
        protected IFilter[] m_filters;

        /// <summary>
        /// The Filter event.
        /// </summary>
        /// <since>Coherence 3.7.1.8</since>
        protected CacheEventArgs m_event;

        #endregion
    }
}