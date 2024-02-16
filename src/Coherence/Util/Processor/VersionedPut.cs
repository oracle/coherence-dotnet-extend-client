/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// <b>VersionedPut</b> is an <see cref="IEntryProcessor"/> that assumes
    /// that entry values implement <see cref="IVersionable"/> interface and
    /// sets an <see cref="IInvocableCacheEntry.Value"/> property
    /// if and only if the version of the specified value matches to the
    /// version of the current value.
    /// </summary>
    /// <remarks>
    /// In case of the match, the <b>VersionedPut</b> will increment the
    /// version indicator before the value is updated.
    /// </remarks>
    /// <seealso cref="ConditionalPut"/>
    /// <author>Gene Gleyzer  2006.05.07</author>
    /// <author>Ivan Cikic  2006.10.23</author>
    /// <since>Coherence 3.2</since>
    public class VersionedPut : AbstractProcessor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public VersionedPut()
        {}

        /// <summary>
        /// Construct a <b>VersionedPut</b> that updates an entry with a new
        /// value if and only if the version of the new value matches to the
        /// version of the current entry's value (which must exist).
        /// </summary>
        /// <remarks>
        /// The result of the <see cref="Process"/> invocation does not
        /// return any result.
        /// </remarks>
        /// <param name="value">
        /// An <see cref="IVersionable"/> value to update an entry with.
        /// </param>
        public VersionedPut(IVersionable value)
                : this(value, false, false)
        {}

        /// <summary>
        /// Construct a <b>VersionedPut</b> that updates an entry with a new
        /// value if and only if the version of the new value matches to the
        /// version of the current entry's value.
        /// </summary>
        /// <remarks>
        /// This processor optionally returns the current value as a result
        /// of the invocation if it has not been updated (the versions did
        /// not match).
        /// </remarks>
        /// <param name="value">
        /// An <see cref="IVersionable"/> value to update an entry with.
        /// </param>
        /// <param name="allowInsert">
        /// Specifies whether or not an insert should be allowed (no
        /// currently existing value).
        /// </param>
        /// <param name="ret">
        /// Specifies whether or not the processor should return the current
        /// value in case it has not been updated.
        /// </param>
        public VersionedPut(IVersionable value, bool allowInsert, bool ret)
        {
            m_value  = value;
            m_insert = allowInsert;
            m_return = ret;
        }

        #endregion

        #region IEntryProcessor implementation

        /// <summary>
        /// Process an <see cref="IInvocableCacheEntry"/>.
        /// </summary>
        /// <param name="entry">
        /// The <b>IInvocableCacheEntry</b> to process.
        /// </param>
        /// <returns>
        /// The result of the processing, if any.
        /// </returns>
        public override object Process(IInvocableCacheEntry entry)
        {
            object result = processEntry(entry, m_value, m_insert, m_return);
            return result == NO_RESULT ? null : result;
        }

        /// <summary>
        /// Process a collection of <see cref="IInvocableCacheEntry"/>
        /// objects.
        /// </summary>
        /// <param name="entries">
        /// A read-only collection of <b>IInvocableCacheEntry</b>
        /// objects to process.
        /// </param>
        /// <returns>
        /// A dictionary containing the results of the processing, up to one
        /// entry for each <b>IInvocableCacheEntry</b> that was
        /// processed, keyed by the keys of the dictionary that were
        /// processed, with a corresponding value being the result of the
        /// processing for each key.
        /// </returns>
        public override IDictionary ProcessAll(ICollection entries)
        {
            IDictionary  results = new LiteDictionary();
            IVersionable value   = m_value;
            bool         insert  = m_insert;
            bool         ret     = m_return;

            foreach (IInvocableCacheEntry entry in entries)
            {
                object result = processEntry(entry, value, insert, ret);
                if (result != NO_RESULT)
                {
                    results[entry.Key] = result;
                }
            }
            return results;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>VersionedPut</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>VersionedPut</b> and the passed object
        /// are equivalent <b>VersionedPut</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is VersionedPut)
            {
                VersionedPut that = (VersionedPut)o;
                return Equals(m_value, that.m_value)
                       && m_insert == that.m_insert
                       && m_return == that.m_return;
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>VersionedPut</b> object
        /// according to the general <see cref="object.GetHashCode()"/>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>VersionedPut</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_value.GetHashCode() + (m_insert ? 1 : 2) + (m_return ? 3 : 4);
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>VersionedPut</b>.
        /// </summary>
        /// <returns>
        /// A <b>String</b> description of the <b>VersionedPut</b>.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + "{Value=" + m_value +
                    ", InsertAllowed= " + m_insert +
                    ", ReturnRequired= " + m_return + '}';
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
            m_value  = (IVersionable) reader.ReadObject(0);
            m_insert = reader.ReadBoolean(1);
            m_return = reader.ReadBoolean(2);
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
            writer.WriteObject(0, m_value);
            writer.WriteBoolean(1, m_insert);
            writer.WriteBoolean(2, m_return);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Process the given entry.
        /// </summary>
        /// <param name="entry">
        /// The <b>IInvocableCacheEntry</b> to process.
        /// </param>
        /// <param name="valueNew">
        /// The new value to update an entry with.
        /// </param>
        /// <param name="insert">
        /// Specifies whether or not an insert is allowed.
        /// </param>
        /// <param name="ret">
        /// Specifies whether or not a return value is required.
        /// </param>
        /// <returns>
        /// The result of the processing, if any.
        /// </returns>
        protected object processEntry(IInvocableCacheEntry entry, IVersionable valueNew, bool insert, bool ret)
        {
            bool         match;
            IVersionable valueCur = (IVersionable)entry.Value;

            if (valueCur == null)
            {
                match = insert;
            }
            else
            {
                IComparable verCur = valueCur.VersionIndicator;
                IComparable verNew = valueNew.VersionIndicator;

                match = (verCur.CompareTo(verNew) == 0);
            }

            if (match)
            {
                valueNew.IncrementVersion();
                entry.SetValue(valueNew, false);
                return NO_RESULT;
            }

            return ret ? valueCur : NO_RESULT;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Used internally to differentiate between "no result" and null result.
        /// </summary>
        protected readonly object NO_RESULT = new Object();

        #endregion

        #region Data members

        /// <summary>
        /// Specifies the new value to update an entry with.
        /// </summary>
        protected IVersionable m_value;

        /// <summary>
        /// Specifies whether or not an insert is allowed.
        /// </summary>
        protected bool m_insert;

        /// <summary>
        /// Specifies whether or not a return value is required.
        /// </summary>
        protected bool m_return;

        #endregion
    }
}