/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.Net.Cache.Support;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// An event which indicates that the content of a cache has changed.
    /// </summary>
    /// <remarks>
    /// Possible cache changes that can be respresented by this class are:
    /// <list type="bullet">
    /// <item>
    /// <description>an entry has been added</description>
    /// </item>
    /// <item>
    /// <description>an entry has been removed</description>
    /// </item>
    /// <item>
    /// <description>an entry has been changed</description>
    /// </item>
    /// </list>
    /// <p>
    /// A CacheEventArgs object is sent as an argument to the
    /// <see cref="ICacheListener"/> interface methods. <c>null</c>
    /// values may be provided for the old and the new values.</p>
    /// </remarks>
    /// <author>Gene Gleyzer  2002.02.11</author>
    /// <author>Aleksandar Seovic  2006.07.12</author>
    [Serializable]
    public class CacheEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="IObservableCache"/> object on which this
        /// event has actually occured.
        /// </summary>
        /// <value>
        /// An <b>IObservableCache</b> object on which this event has
        /// occured.
        /// </value>
        public virtual IObservableCache Cache
        {
            get { return m_source; }
        }

        /// <summary>
        /// Gets this event's type.
        /// </summary>
        /// <remarks>
        /// The event type is one of the <see cref="CacheEventType"/>
        /// enumerated constants.
        /// </remarks>
        /// <value>
        /// An event type.
        /// </value>
        public virtual CacheEventType EventType
        {
            get { return m_eventType; }
        }

        /// <summary>
        /// Gets a key associated with this event.
        /// </summary>
        /// <value>
        /// A key.
        /// </value>
        public virtual object Key
        {
            get { return m_key; }
        }

        /// <summary>
        /// Gets an old value associated with this event.
        /// </summary>
        /// <remarks>
        /// The old value represents a value deleted from or updated in a
        /// dictionary. It is always <c>null</c> for "insert" notifications.
        /// </remarks>
        /// <value>
        /// An old value.
        /// </value>
        public virtual object OldValue
        {
            get { return m_valueOld; }
        }

        /// <summary>
        /// Gets a new value associated with this event.
        /// </summary>
        /// <remarks>
        /// The new value represents a new value inserted into or updated in
        /// a dictionary. It is always <c>null</c> for "delete" notifications.
        /// </remarks>
        /// <value>
        /// A new value.
        /// </value>
        public virtual object NewValue
        {
            get { return m_valueNew; }
        }

        /// <summary>
        /// Return <b>true</b> if this event is caused by the cache internal
        /// processing such as eviction or loading.
        /// </summary>
        /// <value>
        /// <b>true</b> if this event is caused by the cache internal
        /// processing.
        /// </value>
        public virtual bool IsSynthetic
        {
            get { return m_isSynthetic; }
        }

        /// <summary>
        /// Return TransformationState for this event.
        /// </summary>
        /// <value>
        /// A TransformationState.
        /// </value>
        public virtual TransformationState TransformState
        {
            get { return m_transformState; }
        }

        /// <summary>
        /// Return <b>true</b> if this event is a priming event.
        /// </summary>
        /// <value>
        /// <b>true</b> if this event is a priming event.
        /// </value>
        public virtual bool IsPriming
        {
            get { return m_isPriming; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new CacheEventArgs.
        /// </summary>
        /// <param name="source">
        /// The <see cref="IObservableCache"/> object that fired the
        /// event.
        /// </param>
        /// <param name="eventType">
        /// This event's type, one of <see cref="CacheEventType"/>
        /// values.
        /// </param>
        /// <param name="key">
        /// The key into the cache.
        /// </param>
        /// <param name="valueOld">
        /// The old value (for update and delete events).
        /// </param>
        /// <param name="valueNew">
        /// The new value (for insert and update events).
        /// </param>
        /// <param name="isSynthetic">
        /// <b>true</b> if the event is caused by the cache internal
        /// processing such as eviction or loading.
        /// </param>
        public CacheEventArgs(IObservableCache source, CacheEventType eventType,
                              object key, object valueOld, object valueNew,
                              bool isSynthetic)
                 : this(source, eventType, key, valueOld, valueNew, isSynthetic, TransformationState.TRANSFORMABLE, false)
        {}

        /// <summary>
        /// Constructs a new CacheEventArgs.
        /// </summary>
        /// <param name="source">
        /// The <see cref="IObservableCache"/> object that fired the
        /// event.
        /// </param>
        /// <param name="eventType">
        /// This event's type, one of <see cref="CacheEventType"/>
        /// values.
        /// </param>
        /// <param name="key">
        /// The key into the cache.
        /// </param>
        /// <param name="valueOld">
        /// The old value (for update and delete events).
        /// </param>
        /// <param name="valueNew">
        /// The new value (for insert and update events).
        /// </param>
        /// <param name="isSynthetic">
        /// <b>true</b> if the event is caused by the cache internal
        /// processing such as eviction or loading.
        /// </param>
        /// <param name="isPriming">
        /// <b>true</b> if the event is a priming event.
        /// </param>
        public CacheEventArgs(IObservableCache source, CacheEventType eventType,
                              object key, object valueOld, object valueNew,
                              bool isSynthetic, bool isPriming)
            : this(source, eventType, key, valueOld, valueNew, isSynthetic, TransformationState.TRANSFORMABLE, isPriming)
        { }

        /// <summary>
        /// Constructs a new CacheEventArgs.
        /// </summary>
        /// <param name="source">
        /// The <see cref="IObservableCache"/> object that fired the
        /// event.
        /// </param>
        /// <param name="eventType">
        /// This event's type, one of <see cref="CacheEventType"/>
        /// values.
        /// </param>
        /// <param name="key">
        /// The key into the cache.
        /// </param>
        /// <param name="valueOld">
        /// The old value (for update and delete events).
        /// </param>
        /// <param name="valueNew">
        /// The new value (for insert and update events).
        /// </param>
        /// <param name="isSynthetic">
        /// <b>true</b> if the event is caused by the cache internal
        /// processing such as eviction or loading.
        /// </param>
        /// <param name="transformState">
        /// <b>true</b> if the event is a priming event.
        /// has been or should be transformed.
        /// </param>
        /// <param name="isPriming">
        /// <b>true</b> if the event is a priming event.
        /// </param>
        public CacheEventArgs(IObservableCache source, CacheEventType eventType,
                              object key, object valueOld, object valueNew,
                              bool isSynthetic, TransformationState transformState,
                              bool isPriming)
        {
            m_source         = source;
            m_eventType      = eventType;
            m_key            = key;
            m_valueOld       = valueOld;
            m_valueNew       = valueNew;
            m_isSynthetic    = isSynthetic;
            m_transformState = transformState;
            m_isPriming      = isPriming;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Dispatch this event to the specified MapListener.
        /// </summary>
        /// <param name="listener">
        /// The <b>ICacheListener</b>.
        /// </param>
        /// <since>12.2.1</since>
        public void Dispatch(ICacheListener listener)
        {
            if (ShouldDispatch(listener))
            {
                switch (EventType)
                {
                    case CacheEventType.Inserted:
                        listener.EntryInserted(this);
                        break;

                    case CacheEventType.Updated:
                        listener.EntryUpdated(this);
                        break;

                    case CacheEventType.Deleted:
                        listener.EntryDeleted(this);
                        break;
                }
            }
        }

        /// <summary>
        /// Return true if the provided <b>ICacheListener</b> should receive this event.
        /// </summary>
        /// <param name="listener">
        /// The <b>ICacheListener</b>.
        /// </param>
        /// <since>12.2.1.2</since>
        public bool ShouldDispatch(ICacheListener listener)
        {
            return (!IsPriming || CacheListenerSupport.IsPrimingListener(listener));
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Return a string representation of this
        /// <see cref="CacheEventArgs"/> object.
        /// </summary>
        /// <returns>
        /// A <b>String</b> representation of this <b>CacheEventArgs</b>
        /// object.
        /// </returns>
        public override string ToString()
        {
            string evt = GetType().Name;
            string src = m_source.GetType().Name;

            return evt + '{' + src + GetDescription() + (IsSynthetic ? ", synthetic" : "") + (IsPriming ? ", priming" : "") + '}';
        }

        /// <summary>
        /// Get the event's description.
        /// </summary>
        /// <returns>
        /// This event's description.
        /// </returns>
        protected internal virtual string GetDescription()
        {
            switch (EventType)
            {
                case CacheEventType.Inserted:
                    return " inserted: key=" + Key + ", value=" + NewValue;

                case CacheEventType.Updated:
                    return " updated: key=" + Key + ", old value=" + OldValue + ", new value=" + NewValue;

                case CacheEventType.Deleted:
                    return " deleted: key=" + Key + ", value=" + OldValue;

                default:
                    return " <unknown> key=" + Key + ", value=" + OldValue;     // should never happen
            }
        }

        /// <summary>
        /// Convert an event type into a human-readable string.
        /// </summary>
        /// <param name="eventType">
        /// An event type, one of the <see cref="CacheEventType"/>
        /// enumerated values.
        /// </param>
        /// <returns>
        /// A corresponding human-readable string, for example "inserted".
        /// </returns>
        public static string GetDescription(CacheEventType eventType)
        {
            return eventType.ToString();
        }

        #endregion

        #region Enum: TransformationState

        /// <summary>
        /// TransformationState describes how a CacheEvent has been or should be
        /// transformed.
        /// </summary>
        public enum TransformationState
        {
            /// <summary>
            /// Value used to indicate that an event is non-transformable and should
            /// not be passed to any transformer-based listeners.
            /// </summary>
            NON_TRANSFORMABLE,

            /// <summary>
            /// Value used to indicate that an event is transformable and could be
            /// passed to transformer-based listeners.
            /// </summary>
            TRANSFORMABLE,

            /// <summary>
            /// Value used to indicate that an event has been transformed, and should
            /// only be passed to transformer-based listeners.
            /// </summary>
            TRANSFORMED,
        }

        #endregion

        #region Data members

        /// <summary>
        /// The event's source.
        /// </summary>
        protected internal IObservableCache m_source;

        /// <summary>
        /// The event's type.
        /// </summary>
        protected internal CacheEventType m_eventType;

        /// <summary>
        /// A key.
        /// </summary>
        protected internal object m_key;

        /// <summary>
        /// A previous value.
        /// </summary>
        /// <remarks>
        /// May be <c>null</c> if not known.
        /// </remarks>
        protected internal object m_valueOld;

        /// <summary>
        /// A new value.
        /// </summary>
        /// <remarks>
        /// May be <c>null</c> if not known.
        /// </remarks>
        protected internal object m_valueNew;

        /// <summary>
        /// Event cause flag.
        /// </summary>
        protected bool m_isSynthetic;

        /// <summary>
        /// The transformation state for this event
        /// </summary>
        protected TransformationState m_transformState = TransformationState.TRANSFORMABLE;

        /// <summary>
        /// The priming event flag.
        /// </summary>
        protected bool m_isPriming;

        #endregion
    }

    #region Enum: CacheEventType

    /// <summary>
    /// Cache event type enumeration.
    /// </summary>
    public enum CacheEventType
    {
        /// <summary>
        /// This event indicates that an entry has been added to the cache.
        /// </summary>
        Inserted = 1,

        /// <summary>
        /// This event indicates that an entry has been updated in the cache.
        /// </summary>
        Updated = 2,

        /// <summary>
        /// This event indicates that an entry has been removed from the
        /// cache.
        /// </summary>
        Deleted = 3
    }

    #endregion
}