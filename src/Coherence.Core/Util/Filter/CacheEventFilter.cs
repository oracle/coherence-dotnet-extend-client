/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using System.Text;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which evaluates the content of a
    /// <see cref="CacheEventArgs"/> object according to the specified
    /// criteria.
    /// </summary>
    /// <remarks>
    /// This filter is intended to be used by various
    /// <see cref="IObservableCache"/> listeners that are interested in
    /// particular subsets of <b>CacheEvent</b> notifications emitted by
    /// the cache.
    /// <p/>
    /// Usage examples:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// a filter that evaluates to <b>true</b> if an Employee object is
    /// inserted into a cache with a value of IsMarried property set to
    /// <b>true</b>.
    /// <code>
    /// new CacheEventFilter(CacheEventMask.Inserted,
    /// new EqualsFilter("IsMarried", true));
    /// </code>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// a filter that evaluates to <b>true</b> if any object is removed from
    /// a cache.
    /// <code>
    /// new CacheEventFilter(CacheEventMask.Deleted);
    /// </code>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// a filter that evaluates to <b>true</b> if there is an update to an
    /// Employee object where either an old or new value of LastName property
    /// equals to "Smith".
    /// <code>
    /// new CacheEventFilter(CacheEventMask.Updated,
    /// new EqualsFilter("LastName", "Smith"));
    /// </code>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// a filter that is used to keep a cached keys collection result based
    /// on some cache filter up-to-date.
    /// <code>
    /// ICollection keys = new ArrayList();
    /// IFilter filterEvt = new CacheEventFilter(filterCache);
    /// ICacheListener listener = new TestListener();
    /// cache.AddCacheListener(listener, filterEvt, true);
    /// keys.AddAll(cache.GetKeys(filterCache));
    /// </code>
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <seealso cref="ValueChangeEventFilter" />
    /// <author>Gene Gleyzer  2003.09.22</author>
    /// <author>Goran Milosavljevic  2006.10.24</author>
    /// <since>Coherence 2.3</since>
    public class CacheEventFilter : IFilter, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Obtain the event mask.
        /// </summary>
        /// <remarks>
        /// The mask value is concatenation of any of the
        /// <see cref="CacheEventMask"/> values.
        /// </remarks>
        /// <value>
        /// The event mask.
        /// </value>
        public virtual CacheEventMask EventMask
        {
            get { return m_mask; }
        }

        /// <summary>
        /// Obtain the <see cref="IFilter"/> object used to evaluate the
        /// event value(s).
        /// </summary>
        /// <value>
        /// The filter used to evaluate the event value(s).
        /// </value>
        public virtual IFilter Filter
        {
            get { return m_filter; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CacheEventFilter()
        {}

        /// <summary>
        /// Construct a <b>CacheEventFilter</b> that evaluates
        /// <see cref="CacheEventArgs"/> objects based on the specified
        /// combination of event types.
        /// </summary>
        /// <remarks>
        /// Using this constructor is equivalent to:
        /// <code>
        /// new CacheEventFilter(mask, null, CacheEventSyntheticMask.All);
        /// </code>
        /// </remarks>
        /// <param name="mask">
        /// Any combination of <see cref="CacheEventMask.Inserted"/>,
        /// <see cref="CacheEventMask.Updated"/> and
        /// <see cref="CacheEventMask.Deleted"/>,
        /// <see cref="CacheEventMask.UpdatedEntered"/>,
        /// <see cref="CacheEventMask.UpdatedWithin"/>,
        /// <see cref="CacheEventMask.UpdatedLeft"/>.
        /// </param>
        /// <since>Coherence 3.1</since>
        public CacheEventFilter(CacheEventMask mask) : this(mask, null, CacheEventSyntheticMask.All)
        {}

        /// <summary>
        /// Construct a <b>CacheEventFilter</b> that evaluates
        /// <see cref="CacheEventArgs"/> objects that would affect the
        /// results of a keys collection filter issued by a previous call to
        /// <see cref="IDictionary.Keys"/>.
        /// </summary>
        /// <remarks>
        /// It is possible to easily implement <i>continuous query</i>
        /// functionality.
        /// <p />
        /// Using this constructor is equivalent to:
        /// <code>
        /// new CacheEventFilter(CacheEventMask.Keys, filter, CacheEventSyntheticMask.All);
        /// </code>
        /// </remarks>
        /// <param name="filter">
        /// The filter passed previously to a Keys query property.
        /// </param>
        /// <since>Coherence 3.1</since>
        public CacheEventFilter(IFilter filter) : this(CacheEventMask.Keys, filter, CacheEventSyntheticMask.All)
        {}

        /// <summary>
        /// Construct a <b>CacheEventFilter</b> that evaluates
        /// <see cref="CacheEventArgs"/> objects based on the specified
        /// combination of event types.
        /// </summary>
        /// <param name="mask">
        /// Combination of any of the <see cref="CacheEventMask"/>
        /// values.
        /// </param>
        /// <param name="filter">
        /// Optional filter used for evaluating event values.
        /// </param>
        public CacheEventFilter(CacheEventMask mask, IFilter filter)
            : this(mask, filter, CacheEventSyntheticMask.All)
        {}

        /// <summary>
        /// Construct a <b>CacheEventFilter</b> that evaluates
        /// <see cref="CacheEventArgs"/> objects based on the specified
        /// combination of event types.
        /// </summary>
        /// <param name="mask">
        /// Combination of any of the <see cref="CacheEventMask"/>
        /// values.
        /// </param>
        /// <param name="maskSynth">
        /// Combination of any of the <see cref="CacheEventSyntheticMask"/>
        /// </param>
        public CacheEventFilter(CacheEventMask mask, CacheEventSyntheticMask maskSynth)
            : this(mask, null, maskSynth)
        {}

        /// <summary>
        /// Construct a <b>CacheEventFilter</b> that evaluates
        /// <see cref="CacheEventArgs"/> objects based on the specified
        /// combination of event types.
        /// </summary>
        /// <param name="mask">
        /// Combination of any of the <see cref="CacheEventMask"/>
        /// values.
        /// </param>
        /// <param name="filter">
        /// Optional filter used for evaluating event values.
        /// </param>
        /// <param name="maskSynth">
        /// Combination of any of the <see cref="CacheEventSyntheticMask"/>
        /// </param>
        public CacheEventFilter(CacheEventMask mask, IFilter filter, CacheEventSyntheticMask maskSynth)
        {
            if (((int) mask & (int) (CacheEventMask.All | CacheEventMask.Keys | CacheEventMask.UpdatedWithin)) == 0)
            {
                throw new ArgumentException("At least one CacheEventMask type must be specified");
            }

            m_mask      = mask;
            m_maskSynth = maskSynth;
            m_filter    = filter;
        }

        /// <summary>
        /// Initialize event type to event mask translation array.
        /// </summary>
        static CacheEventFilter()
        {
            MASK = new CacheEventMask[4];
            MASK[(int) CacheEventType.Inserted] = CacheEventMask.Inserted;
            MASK[(int) CacheEventType.Updated]  = CacheEventMask.Updated | CacheEventMask.UpdatedWithin |
                                                       CacheEventMask.UpdatedEntered |
                                                       CacheEventMask.UpdatedLeft;
            MASK[(int) CacheEventType.Deleted]  = CacheEventMask.Deleted;
        }

        #endregion

        #region IFilter implementation

        /// <summary>
        /// Apply the test to the object.
        /// </summary>
        /// <param name="o">
        /// An object to which the test is applied.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        public virtual bool Evaluate(object o)
        {
            var evt = (CacheEventArgs) o;

            // check if the event is of a type that the client is
            // interested in evaluating
            CacheEventType type = evt.EventType;
            CacheEventMask mask = EventMask;
            try
            {
                if ((MASK[(int) type] & mask) == 0)
                {
                    return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }

            // check for a client-specified event filter
            IFilter filter = Filter;
            if (filter == null)
            {
                return true;
            }

            CacheEventSyntheticMask maskSynth  = m_maskSynth;
            bool                    fSynthetic = evt.IsSynthetic;
            if (((maskSynth & CacheEventSyntheticMask.Synthetic) == 0 &&  fSynthetic) ||
                ((maskSynth & CacheEventSyntheticMask.Natural)   == 0 && !fSynthetic))
                {
                return false;
                }

            // evaluate the filter
            switch (type)
            {
                case CacheEventType.Inserted:
                    return filter.Evaluate(evt.NewValue);

                case CacheEventType.Updated:
                    // note that the old value evaluation is deferred, because
                    // the event itself may be deferring loading the old value,
                    // e.g. if the event is coming from a disk-backed cache
                    bool isNew = filter.Evaluate(evt.NewValue);

                    switch (mask & (CacheEventMask.UpdatedEntered | CacheEventMask.UpdatedLeft |
                                     CacheEventMask.Updated | CacheEventMask.UpdatedWithin))
                    {
                        case CacheEventMask.UpdatedEntered:
                            return isNew && !filter.Evaluate(evt.OldValue);

                        case CacheEventMask.UpdatedLeft:
                            return !isNew && filter.Evaluate(evt.OldValue);

                        case CacheEventMask.UpdatedEntered | CacheEventMask.UpdatedLeft:
                            return isNew != filter.Evaluate(evt.OldValue);

                        case CacheEventMask.UpdatedWithin:
                            return isNew && filter.Evaluate(evt.OldValue);

                        case CacheEventMask.UpdatedWithin | CacheEventMask.UpdatedEntered:
                            return isNew;

                        case CacheEventMask.UpdatedWithin | CacheEventMask.UpdatedLeft:
                            return filter.Evaluate(evt.OldValue);

                        default:
                            // all other combinations evaulate to the same as
                            // CacheEventMask.Updated
                            return isNew || filter.Evaluate(evt.OldValue);
                    }

            case CacheEventType.Deleted:
                return filter.Evaluate(evt.OldValue);

            default:
                return false;
            }
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>CacheEventFilter</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The <b>CacheEventFilter</b> to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if this <b>CacheEventFilter</b> and the passed object
        /// are equivalent <b>CacheEventFilter</b> objects.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o != null && o.GetType() == this.GetType())
            {
                var that = (CacheEventFilter) o;
                return m_mask == that.m_mask && Equals(m_filter, that.m_filter);
            }

            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>CacheEventFilter</b> object
        /// according to the general <b>object.GetHashCode()</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>CacheEventFilter</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            int     hash   = (int) m_mask;
            IFilter filter = m_filter;
            if (filter != null)
            {
                hash += filter.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>CacheEventFilter</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>CacheEventFilter</b>.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder("CacheEventFilter(mask=");

            CacheEventMask mask = EventMask;
            if (mask == CacheEventMask.All)
            {
                sb.Append("ALL");
            }
            else if (mask == CacheEventMask.Keys)
            {
                sb.Append("KEYSET");
            }
            else
            {
                if ((mask & CacheEventMask.Inserted) != 0)
                {
                    sb.Append("INSERTED|");
                }
                if ((mask & CacheEventMask.Updated) != 0)
                {
                    sb.Append("UPDATED|");
                }
                if ((mask & CacheEventMask.Deleted) != 0)
                {
                    sb.Append("DELETED|");
                }
                if ((mask & CacheEventMask.UpdatedEntered) != 0)
                {
                    sb.Append("UPDATED_ENTERED|");
                }
                if ((mask & CacheEventMask.UpdatedLeft) != 0)
                {
                    sb.Append("UPDATED_LEFT|");
                }
                if ((mask & CacheEventMask.UpdatedWithin) != 0)
                {
                    sb.Append("UPDATED_WITHIN|");
                }
                sb.Length -= 1;
            }

            IFilter filter = Filter;
            if (filter != null)
            {
                sb.Append(", filter=").Append(filter);
            }

            sb.Append(", synthetic-mask=");
            CacheEventSyntheticMask maskSynth = m_maskSynth;
            if ((maskSynth & CacheEventSyntheticMask.Natural) != 0)
            {
                sb.Append("NATURAL|");
            }
            if ((maskSynth & CacheEventSyntheticMask.Synthetic) != 0)
            {
                sb.Append("SYNTHETIC|");
            }
            sb.Length -= 1;

            sb.Append(')');

            return sb.ToString();
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
        public virtual void ReadExternal(IPofReader reader)
        {
            m_mask   = (CacheEventMask) reader.ReadInt32(0);
            m_filter = (IFilter) reader.ReadObject(1);

            // space left for MapEventFilter expansion
            m_maskSynth = (CacheEventSyntheticMask) reader.ReadInt32(10);
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
        public virtual void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32(0, (int) m_mask);
            writer.WriteObject(1, m_filter);

            // space left for MapEventFilter expansion
            writer.WriteInt32(10, (int) m_maskSynth);
        }

        #endregion

        #region Enum: CacheEventSyntheticMask

        /// <summary>
        /// Event natural/synthetic mask enum.
        /// </summary>
        [Flags]
        public enum CacheEventSyntheticMask
        {
            /// <summary>
            /// This value indicates that synthetic events should be evaluated.
            /// </summary>
            /// <remarks>
            /// A synthetic event is emitted as a result of internal processing
            /// such as expiration, eviction, or read-through.
            /// </remarks>
            Synthetic = 0x0001,

            /// <summary>
            /// This value indicates that natural events should be evaluated.
            /// </summary>
            /// <remarks>
            /// A natural event is emitted as a result of a cache mutation.
            /// </remarks>
            Natural = 0x0002,

            /// <summary>
            /// This value indicates that both synthetic and natural events
            ///  should be evaluated.
            /// </summary>
            All = Synthetic | Natural
        }

        #endregion

        #region Enum: CacheEventMask

        /// <summary>
        /// Event mask enum.
        /// </summary>
        [Flags]
        public enum CacheEventMask
        {
            /// <summary>
            /// This value indicates that
            /// <see cref="CacheEventType.Inserted"/> events should be
            /// evaluated.
            /// </summary>
            /// <remarks>
            /// The event will be fired if there is no filter specified or
            /// the filter evaluates to <b>true</b> for a new value.
            /// </remarks>
            Inserted = 0x0001,

            /// <summary>
            /// This value indicates that
            /// <see cref="CacheEventType.Updated"/> events should be
            /// evaluated.
            /// </summary>
            /// <remarks>
            /// The event will be fired if there is no filter specified or
            /// the filter evaluates to <b>true</b> when applied to either
            /// old or new value.
            /// </remarks>
            Updated = 0x0002,

            /// <summary>
            /// This value indicates that
            /// <see cref="CacheEventType.Deleted"/> events should be
            /// evaluated.
            /// </summary>
            /// <remarks>
            /// The event will be fired if there is no filter specified or
            /// the filter evaluates evaluates to <b>true</b> for an old
            /// value.
            /// </remarks>
            Deleted = 0x0004,

            /// <summary>
            /// This value indicates that
            /// <see cref="CacheEventType.Updated"/> events should be
            /// evaluated, but only if filter evaluation is <b>false</b> for
            /// the old value and <b>true</b> for the new value.
            /// </summary>
            /// <remarks>
            /// This corresponds to an item that was not in a Keys filter
            /// result changing such that it would now be in that Keys filter
            /// result.
            /// </remarks>
            /// <since>Coherence 3.1</since>
            UpdatedEntered = 0x0008,

            /// <summary>
            /// This value indicates that
            /// <see cref="CacheEventType.Updated"/> events should be
            /// evaluated, but only if filter evaluation is <b>true</b> for
            /// the old value and <b>false</b> for the new value.
            /// </summary>
            /// <remarks>
            /// This corresponds to an item that was in a Keys filter result
            /// changing such that it would no longer be in that Keys filter
            /// result.
            /// </remarks>
            /// <since>Coherence 3.1</since>
            UpdatedLeft = 0x0010,

            /// <summary>
            /// This value indicates that
            /// <see cref="CacheEventType.Updated"/> events should be
            /// evaluated, but only if filter evaluation is <b>true</b> for
            /// both the old and the new value.
            /// </summary>
            /// <remarks>
            /// This corresponds to an item that was in a Keys filter result
            /// changing but not leaving the Keys filter result.
            /// </remarks>
            /// <since>Coherence 3.1</since>
            UpdatedWithin = 0x0020,

            /// <summary>
            /// This value indicates that all events should be evaluated.
            /// </summary>
            All = Inserted | Updated | Deleted,

            /// <summary>
            /// This value indicates that all events that would affect the
            /// result of an <b>ICache.Keys</b> query should be evaluated.
            /// </summary>
            /// <since>Coherence 3.1</since>
            Keys = Inserted | Deleted | UpdatedEntered | UpdatedLeft
        }

        #endregion

        #region Data members

        /// <summary>
        /// The event mask.
        /// </summary>
        protected internal CacheEventMask m_mask;

        /// <summary>
        /// The synthetic mask.
        /// </summary>
        protected internal CacheEventSyntheticMask m_maskSynth;

        /// <summary>
        /// The event value(s) filter.
        /// </summary>
        protected internal IFilter m_filter;

        /// <summary>
        /// Event type to event mask translation array.
        /// </summary>
        private static readonly CacheEventMask[] MASK;

        #endregion
    }
}