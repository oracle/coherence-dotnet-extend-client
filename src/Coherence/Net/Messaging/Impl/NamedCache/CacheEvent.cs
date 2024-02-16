/*
 * Copyright (c) 2000, 2022, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Net.Impl;
using Tangosol.Util;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// <see cref="Message"/> that encapsulates the information in a
    /// <see cref="CacheEventArgs"/>.
    /// </summary>
    /// <remarks>
    /// This message is sent by an <see cref="ICacheListener"/>
    /// registered on the remote <see cref="INamedCache"/> by the peer
    /// NamedCacheProxy.
    /// </remarks>
    /// <author>Goran Milosavljevic  2006.09.04</author>
    /// <seealso cref="Message"/>
    public class CacheEvent : Message
    {
        #region Properties

        /// <summary>
        /// Return the identifier for <b>Message</b> object's class.
        /// </summary>
        /// <value>
        /// An identifier that uniquely identifies <b>Message</b> object's
        /// class.
        /// </value>
        public override int TypeId
        {
            get { return TYPE_ID; }
        }

        /// <summary>
        /// If positive, identifies the <see cref="IFilter"/> that caused the
        /// <see cref="CacheEventArgs"/> to be raised.
        /// </summary>
        /// <value>
        /// The identifier of the <b>IFilter</b> that caused the event.
        /// </value>
        public virtual long FilterId
        {
            get { return m_filterId; }
            set { m_filterId = value; }
        }

        /// <summary>
        /// If positive, identifies the array of <see cref="IFilter"/> that caused the
        /// <see cref="CacheEventArgs"/> to be raised.
        /// </summary>
        /// <value>
        /// The array of identifiers of the <b>IFilter</b> that caused the event.
        /// </value>
        /// <since>Coherence 3.7.1.8</since>
        public virtual long[] FilterIds
        {
            get { return m_filterIds; }
            set { m_filterIds = value; }
        }

        /// <summary>
        /// The cache event type, one of <see cref="CacheEventType"/> values.
        /// </summary>
        /// <value>
        /// <b>CacheEventType</b> of the event.
        /// </value>
        public virtual CacheEventType EventType
        {
            get { return m_type; }
            set { m_type = value; }
        }

        /// <summary>
        /// The key associated with the <b>CacheEvent</b>.
        /// </summary>
        /// <value>
        /// The key associated with the <b>CacheEvent</b>.
        /// </value>
        public virtual object Key
        {
            get { return m_key; }
            set { m_key = value; }
        }

        /// <summary>
        /// <b>true</b> if the <b>CacheEvent</b> was caused by the cache
        /// internal processing such as eviction or loading.
        /// </summary>
        /// <value>
        /// <b>true</b> if the <b>CacheEvent</b> was caused by the cache
        /// internal processing such as eviction or loading.
        /// </value>
        public virtual bool IsSynthetic
        {
            get { return (m_Flags & SYNTHETIC) != 0 ; }
            set { m_Flags |= value ? SYNTHETIC : 0; }
        }

        /// <summary>
        /// Getter for property TransformState.
        /// The TransformationState value of the event.
        /// </summary>
        /// <value>
        /// The <see cref="CacheEventArgs.TransformationState" /> value of the event.
        /// </value>
        public virtual CacheEventArgs.TransformationState TransformState
        {
            get { return m_transformationState; }
            set { m_transformationState = value; }
        }

        /// <summary>
        /// <b>true</b> if this is a cache truncate request.
        /// <b>false</b> if this is a cache clear request.
        /// </summary>
        /// <value>
        /// <b>true</b> if is is a truncate request.
        /// <b>false</b> if it is clear request.
        /// </value>
        /// <since>12.2.1</since>
        public virtual bool IsTruncate
        {
            get { return (m_Flags & TRUNCATE) != 0; }
            set { m_Flags |= value ? TRUNCATE : 0; }
        }

        /// <summary>
        /// <b>true</b> if this is a priming event.
        /// <b>false</b> if this is a priming event.
        /// </summary>
        /// <value>
        /// <b>true</b> if is is a priming event.
        /// <b>false</b> if it is priming event.
        /// </value>
        /// <since>12.2.1.2</since>
        public virtual bool IsPriming
        {
            get { return (m_Flags & PRIMING) != 0; }
            set { m_Flags |= value ? PRIMING : 0; }
        }

        /// <summary>
        /// <b>true</b> if the <b>CacheEvent</b> was caused cache entry expired.
        /// </summary>
        /// <value>
        /// <b>true</b> if the <b>CacheEvent</b> was caused by cache entry expired
        /// </value>
        /// <since>14.1.1.0.10</since>
        public virtual bool IsExpired
        {
            get { return (m_Flags & EXPIRED) != 0; }
            set { m_Flags |= value ? EXPIRED : 0; }
        }

        /// <summary>
        /// The new value (for insert and update events).
        /// </summary>
        /// <value>
        /// The new value.
        /// </value>
        public virtual object ValueNew
        {
            get { return m_valueNew; }
            set { m_valueNew = value; }
        }

        /// <summary>
        /// The old value (for update and delete events).
        /// </summary>
        /// <value>
        /// The old value.
        /// </value>
        public virtual object ValueOld
        {
            get { return m_valueOld; }
            set { m_valueOld = value; }
        }

        /// <summary>
        /// Determine if this IMessage should be executed in the same order
        /// as it was received relative to other messages sent through the
        /// same <b>IChannel</b>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Consider two messages: M1 and M2. Say M1 is received before M2
        /// but executed on a different execute thread (for example, when the
        /// <see cref="IConnectionManager"/> is configured with an execute
        /// thread pool of size greater than 1). In this case, there is no
        /// way to guarantee that M1 will finish executing before M2.
        /// However, if M1 returns <b>true</b> from this method, the
        /// <b>IConnectionManager</b> will execute M1 on its service thread,
        /// thus guaranteeing that M1 will execute before M2.</p>
        /// <p>
        /// In-order execution should be considered as a very advanced
        /// feature and implementations that return <b>true</b> from this
        /// method must exercise extreme caution during execution, since any
        /// delay or unhandled exceptions will cause a delay or complete
        /// shutdown of the underlying <b>IConnectionManager</b>.</p>
        /// </remarks>
        /// <returns>
        /// <b>true</b> if the IMessage should be executed in the same order
        /// as it was received relative to other messages.
        /// </returns>
        public override bool ExecuteInOrder
        {
            get { return true; }
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            EventType   = (CacheEventType) reader.ReadInt32(0);
            int implVersion = ImplVersion;
            if (implVersion > 3)
            {
                FilterIds = reader.ReadInt64Array(1);
            }
            else
            {
                FilterId  = reader.ReadInt64(1);
            }
            Key         = reader.ReadObject(2);
            ValueNew    = reader.ReadObject(3);
            ValueOld    = reader.ReadObject(4);
            m_Flags    |= reader.ReadBoolean(5) ? SYNTHETIC : 0;

            // COH-9355
            if (implVersion > 4)
            {
                TransformState = (CacheEventArgs.TransformationState) reader.ReadInt32(6);
            }

            // COH-13916
            if (implVersion > 5)
            {
                m_Flags |= reader.ReadBoolean(7) ? TRUNCATE : 0;
            }

            // COH-18376
            if (implVersion > 6)
            {
                m_Flags |= reader.ReadBoolean(8) ? PRIMING : 0;
            }

            // COH-24927
            if (implVersion > 8)
            {
                m_Flags |= reader.ReadBoolean(9) ? EXPIRED : 0;
            }
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteInt32(0, (int) EventType);
            int implVersion = ImplVersion;
            if (implVersion > 3)
            {
                writer.WriteInt64Array(1, FilterIds);
            }
            else
            {
                writer.WriteInt64(1, FilterId);
            }
            writer.WriteObject(2, Key);
            writer.WriteObject(3, ValueNew);
            writer.WriteObject(4, ValueOld);
            writer.WriteBoolean(5, IsSynthetic);

            // COH-9355
            if (implVersion > 4)
            {
                writer.WriteInt32(6, (int) TransformState);
            }

            // COH-13916
            if (implVersion > 5)
            {
                writer.WriteBoolean(7, IsTruncate);
            }

            // COH-18376
            if (implVersion > 6)
            {
                writer.WriteBoolean(8, IsPriming);
            }

            // COH-24927
            if (implVersion > 8)
            {
                writer.WriteBoolean(9, IsExpired);
            }
        }

        #endregion

        #region IRunnable implementation

        /// <summary>
        /// Execute the action specific to the Message implementation.
        /// </summary>
        public override void Run()
        {
            IChannel channel = Channel;
            Debug.Assert(channel != null);

            RemoteNamedCache cache = (RemoteNamedCache) channel.Receiver;
            Debug.Assert(cache != null);

            if (IsTruncate)
            {
                Listeners listeners = cache.DeactivationListeners;
                if (!listeners.IsEmpty)
                {
                    CacheEventArgs evt = new CacheEventArgs(cache, CacheEventType.Updated, null, null, null, false); 
                    CacheListenerSupport.Dispatch(evt, listeners, false);
                }
            }
            else
            {
                cache.BinaryCache.Dispatch(EventType, FilterIds, Key, ValueOld, ValueNew, 
                    IsSynthetic, (int) TransformState, IsPriming, IsExpired);   
            }
        }

        #endregion
        
         
        #region Extend Overrides
        
        /// <inheritdoc />
        protected override string GetDescription()
        {
            object oOldValue = ValueOld;
            object oNewValue = ValueNew;
            
            return base.GetDescription()
                   + ", Action="    + CacheEventArgs.GetDescription(EventType)
                   + ", FilterId="  + FilterId
                   + ", FilterIds=" + string.Join(", ", FilterIds)
                   + ", Key="       + Key
                   + ", OldValue="  + (oOldValue == null ? "null" : oOldValue.GetType().Name + "(HashCode=" + oOldValue.GetHashCode() + ')')
                   + ", NewValue="  + (oNewValue == null ? "null" : oNewValue.GetType().Name + "(HashCode=" + oNewValue.GetHashCode() + ')')
                   + ", Synthetic=" + IsSynthetic
                   + ", Priming="   + IsPriming
                   + ", Expired="   + IsExpired;
        }

        #endregion

        #region Constants

        /// <summary>
        /// true if the CacheEvent was caused by the cache internal
        /// processing such as eviction or loading.
        /// </summary>
        private const int SYNTHETIC = 0x00000001;

        /// <summary>
        /// The value of true indicates that this is a priming event.
        /// </summary>
        /// <since>12.2.1.2</since>
        private const int PRIMING = 0x00000002;

        /// <summary>
        /// The value of true indicates that this is an expired event.
        /// </summary>
        /// <since>14.1.1.0.10</since>
        private const int EXPIRED = 0x00000004;

        /// <summary>
        /// The value of true indicates that this is a cache truncate request.
        /// </summary>
        /// <since>12.2.1</since>
        private const int TRUNCATE = 0x00000008;

        #endregion

        #region Data members

        /// <summary>
        /// If positive, identifies the IFilter that caused the
        /// CacheEvent to be raised.
        /// </summary>
        private long m_filterId;

        /// <summary>
        /// If positive, identifies the array of IFilters that caused the
        /// CacheEvent to be raised.
        /// </summary>
        private long[] m_filterIds;

        /// <summary>
        /// The CacheEventType identifier.
        /// </summary>
        private CacheEventType m_type;

        /// <summary>
        /// The key associated with the CacheEvent.
        /// </summary>
        private object m_key;

        /// <summary>
        /// The transformation value of the event.
        /// See CacheEvent$TransformatioState enum.
        /// </summary>
        private CacheEventArgs.TransformationState m_transformationState;

        /// <summary>
        ///  Flags holder for event details such as whether the event is synthetic
        /// </summary>
        /// <since>14.1.1.0.10</since>
        protected int m_Flags;

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 13;

        /// <summary>
        /// The new value (for insert and update events).
        /// </summary>
        private object m_valueNew;

        /// <summary>
        /// The old value (for update and delete events).
        /// </summary>
        private object m_valueOld;

        #endregion
    }
}
