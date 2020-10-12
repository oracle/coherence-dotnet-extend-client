/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
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

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// A generic Filter-based <see cref="ICacheTrigger"/> implementation.
    /// </summary>
    /// <remarks>
    /// If an evaluation of the <see cref="ICacheTriggerEntry"/> object
    /// representing a pending change fails (returns <b>false</b>), then one
    /// of the following actions is taken:
    /// <list type="bullet">
    /// <item>
    /// <see cref="ActionCode.Rollback"/> - an
    /// <see cref="ArgumentException"/> is thrown by the trigger to reject
    /// the operation that would result in this change (default);
    /// </item>
    /// <item>
    /// <see cref="ActionCode.Ignore"/> - the change is ignored and the
    /// entry's value is reset to the original value returned by the
    /// <see cref="ICacheTriggerEntry.OriginalValue"/>;
    /// </item>
    /// <item>
    /// <see cref="ActionCode.Remove"/> - the entry is removed from the
    /// underlying backing cache using the
    /// <see cref="IInvocableCacheEntry.Remove"/> call.
    /// </item>
    /// </list>
    /// Note: This trigger never prevents entries from being removed.
    /// </remarks>
    /// <author>Gene Gleyzer  2008.03.11</author>
    /// <author>Ana Cikic  2008.07.02</author>
    /// <since>Coherence 3.4</since>
    [Serializable]
    public class FilterTrigger : ICacheTrigger, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Obtain the underlying <see cref="IFilter"/>.
        /// </summary>
        /// <value>
        /// The underlying <b>IFilter</b> object.
        /// </value>
        public virtual IFilter Filter
        {
            get { return m_filter; }
        }

        /// <summary>
        /// The action code for this FilterTrigger.
        /// </summary>
        /// <value>
        /// One of the <see cref="ActionCode"/> values.
        /// </value>
        public virtual ActionCode Action
        {
            get { return m_action; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FilterTrigger()
        {}

        /// <summary>
        /// Construct a FilterTrigger based on the specified
        /// <see cref="IFilter"/> object and
        /// <see cref="ActionCode.Rollback"/>.
        /// </summary>
        /// <param name="filter">
        /// The underlying <b>IFilter</b>.
        /// </param>
        public FilterTrigger(IFilter filter) : this(filter, ActionCode.Rollback)
        {}

        /// <summary>
        /// Construct a FilterTrigger based on the specified
        /// <see cref="IFilter"/> object and the action code.
        /// </summary>
        /// <param name="filter">
        /// The underlying <b>IFilter</b>.
        /// </param>
        /// <param name="action">
        /// One of the <see cref="ActionCode"/> values.
        /// </param>
        public FilterTrigger(IFilter filter, ActionCode action)
        {
            Debug.Assert(filter != null, "Null filter");
            m_filter = filter;
            m_action = action;
        }

        #endregion

        #region ICacheTrigger implementation

        /// <summary>
        /// This method is called before the result of a mutating operation
        /// represented by the specified entry object is committed into the
        /// underlying cache.
        /// </summary>
        /// <remarks>
        /// An implementation of this method can evaluate the change by
        /// analyzing the original and the new value, and can perform any of
        /// the following:
        /// <list type="bullet">
        /// <item>
        /// override the requested change by setting
        /// <see cref="IInvocableCacheEntry.Value"/> to a different value;
        /// </item>
        /// <item>
        /// undo the pending change by resetting the entry value to the
        /// original value obtained from
        /// <see cref="ICacheTriggerEntry.OriginalValue"/>
        /// </item>
        /// <item>
        /// remove the entry from the underlying cache by calling
        /// <see cref="IInvocableCacheEntry.Remove"/>
        /// </item>
        /// <item>
        /// reject the pending change by throwing an <see cref="Exception"/>,
        /// which will prevent any changes from being committed, and will
        /// result in the exception being thrown from the operation that
        /// attempted to modify the cache; or
        /// </item>
        /// <item>
        /// do nothing, thus allowing the pending change to be committed to
        /// the underlying cache.
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="entry">
        /// An <see cref="ICacheTriggerEntry"/> object that represents the
        /// pending change to be committed to the cache, as well as the
        /// original state of the entry.
        /// </param>
        public virtual void Process(ICacheTriggerEntry entry)
        {
            if (entry.IsPresent && !InvocableCacheHelper.EvaluateEntry(m_filter, entry))
            {
                switch (m_action)
                {
                    case ActionCode.Rollback:
                    default:
                        throw new ArgumentException("Rejecting " + entry +
                            " by trigger " + this);

                    case ActionCode.Ignore:
                        object value = entry.OriginalValue;
                        if (value != null || entry.IsOriginalPresent)
                        {
                            entry.SetValue(value, true);
                        }
                        else
                        {
                            entry.Remove(true);
                        }
                        break;

                    case ActionCode.Remove:
                        entry.Remove(true);
                        break;
                }
            }
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
            m_filter  = (IFilter) reader.ReadObject(0);
            m_action  = (ActionCode) reader.ReadInt32(1);
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
            writer.WriteObject(0, m_filter);
            writer.WriteInt32(1, (int) m_action);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the FilterTrigger with another object to determine
        /// equality.
        /// </summary>
        /// <remarks>
        /// Two FilterTrigger objects are considered equal iff the wrapped
        /// filters and action codes are equal.
        /// </remarks>
        /// <param name="o">
        /// The <b>FilterTrigger</b> to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this FilterTrigger and the passed object are
        /// equivalent FilterTrigger objects.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is FilterTrigger)
            {
                var that = (FilterTrigger) o;
                return Equals(m_filter, that.m_filter) && m_action == that.m_action;
            }

            return false;
        }

        /// <summary>
        /// Determine a hash value for the FilterTrigger object according to
        /// the general <see cref="object.GetHashCode()"/> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this FilterTrigger object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_filter.GetHashCode();
        }

        /// <summary>
        /// Return a human-readable description for this FilterTrigger.
        /// </summary>
        /// <returns>
        /// A string description of the FilterTrigger.
        /// </returns>
        public override string ToString()
        {
            string type   = GetType().Name;
            string action = m_action.ToString();
            return type + '(' + m_filter + ", " + action + ')';
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying filter.
        /// </summary>
        protected IFilter m_filter;

        /// <summary>
        /// The action code.
        /// </summary>
        protected ActionCode m_action;

        #endregion

        #region Enum: ActionCode

        /// <summary>
        /// The action taken if an evaluation of the
        /// <see cref="ICacheTriggerEntry"/> object representing a pending
        /// change fails (returns <b>false</b>).
        /// </summary>
        public enum ActionCode
        {
            /// <summary>
            /// Evaluation failure results in an
            /// <see cref="ArgumentException"/> thrown by the trigger.
            /// </summary>
            Rollback = 0,

            /// <summary>
            /// Evaluation failure results in restoring the original entry's
            /// value.
            /// </summary>
            Ignore = 1,

            /// <summary>
            /// Evaluation failure results in a removal of the entry.
            /// </summary>
            Remove = 2
        }

        #endregion
    }
}