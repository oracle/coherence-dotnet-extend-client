/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Diagnostics;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util;
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// <b>UpdaterProcessor</b> is an <see cref="IEntryProcessor"/>
    /// implementations that updates an attribute of an object cached in an
    /// <see cref="IInvocableCache"/>.
    /// </summary>
    /// <remarks>
    /// A common usage pattern is:
    /// <pre>
    /// cache.Invoke(Key, new UpdaterProcessor(updater, value));
    /// </pre>
    /// which is functionally equivalent to the following operation:
    /// <pre>
    /// Object target = cache.Get(Key);
    /// updater.update(target, value);
    /// cache.Put(Key, target);
    /// </pre>
    /// The major difference is that for clustered caches using the
    /// <see cref="UpdaterProcessor"/> allows avoiding explicit concurrency
    /// control and could significantly reduce the amount of network traffic.
    /// </remarks>
    /// <author>Gene Gleyzer  2006.07.25</author>
    /// <author>Ivan Cikic  2006.10.25</author>
    public class UpdaterProcessor : AbstractProcessor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UpdaterProcessor()
        {}

        /// <summary>
        /// Construct an <b>UpdaterProcessor</b> based on the specified
        /// <see cref="IValueUpdater"/>.
        /// </summary>
        /// <param name="updater">
        /// An <b>IValueUpdater</b> object; passing <c>null</c> will simpy
        /// replace the entry's value with the specified one instead of
        /// updating it.
        /// </param>
        /// <param name="value">
        /// The value to update the target entry with.
        /// </param>
        public UpdaterProcessor(IValueUpdater updater, object value)
        {
            m_updater = updater;
            m_value   = value;
        }

        /// <summary>
        /// Construct an <b>UpdaterProcessor</b> for a given member name.
        /// </summary>
        /// <remarks>
        /// The member must have a single parameter of a .NET type
        /// compatible with the specified value type.
        /// </remarks>
        /// <param name="member">
        /// A member name to make a <see cref="ReflectionUpdater"/> for;
        /// this parameter can also be a dot-delimited sequence of member
        /// names which would result in using a
        /// <see cref="CompositeUpdater"/>.
        /// </param>
        /// <param name="value">
        /// The value to update the target entry with.
        /// </param>
        public UpdaterProcessor(string member, object value)
        {
            Debug.Assert(member != null && member.Length != 0, "Invalid member name");

            m_updater = member.IndexOf('.') < 0
                            ? new ReflectionUpdater(member)
                            : (IValueUpdater) new CompositeUpdater(member);
            m_value   = value;
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
            IValueUpdater updater = m_updater;
            if (updater == null)
            {
                entry.SetValue(m_value, true);
            }
            else if (entry.IsPresent)
            {
                object target = entry.Value;
                updater.Update(target, m_value);
                entry.SetValue(target, true);
            }
            return true;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>UpdaterProcessor</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>UpdaterProcessor</b> and the passed object
        /// are equivalent <b>UpdaterProcessor</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is UpdaterProcessor)
            {
                UpdaterProcessor that = (UpdaterProcessor) o;
                return  Equals(m_updater, that.m_updater) &&
                        Equals(m_value,  that.m_value);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>UpdaterProcessor</b> object
        /// according to the general <see cref="object.GetHashCode()"/>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>UpdaterProcessor</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_updater.GetHashCode();
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>UpdaterProcessor</b>.
        /// </summary>
        /// <returns>
        /// A <b>String</b> description of the <b>UpdaterProcessor</b>.
        /// </returns>
        public override string ToString()
        {
            return "UpdaterProcessor(" + m_updater + ", " + m_value + ')';
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
            m_updater = (IValueUpdater) reader.ReadObject(0);
            m_value   = reader.ReadObject(1);
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
            writer.WriteObject(0, m_updater);
            writer.WriteObject(1, m_value);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying <see cref="IValueUpdater"/>.
        /// </summary>
        protected IValueUpdater m_updater;


        /// <summary>
        /// A value to update the entry's value with.
        /// </summary>
        protected object m_value;

        #endregion
    }
}