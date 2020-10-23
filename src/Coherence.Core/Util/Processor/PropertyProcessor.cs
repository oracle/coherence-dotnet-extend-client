/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// <b>PropertyProcessor</b> is a base class for
    /// <see cref="IEntryProcessor"/> implementations that depend on a
    /// <see cref="PropertyManipulator"/>.
    /// </summary>
    /// <remarks>
    /// A typical concrete subclass would implement the
    /// <see cref="IEntryProcessor.Process"/> method using the following
    /// pattern:
    /// <pre>
    /// public Object Process(IInvocableDictonaryEntry entry)
    /// {
    ///     // retrieve an old property value
    ///     Object oldValue = entry;
    ///
    ///     ... // calculate a new value and the process result
    ///     ... // based on the old value and the processor's attributes
    ///
    ///     if (!newValue.Equals(oldValue))
    ///     {
    ///         // set the new property value
    ///         entry = newValue;
    ///     }
    ///
    ///     // return the process result
    ///     return oResult;
    /// }
    /// </pre>
    /// </remarks>
    /// <author>Gene Gleyzer  2005.10.31</author>
    /// <author>Ivan Cikic  2006.10.21</author>
    public abstract class PropertyProcessor : AbstractProcessor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PropertyProcessor()
        {}

        /// <summary>
        /// Construct a <see cref="PropertyProcessor"/> for the specified
        /// property name.
        /// </summary>
        /// <param name="name">
        /// A property name.
        /// </param>
        public PropertyProcessor(string name) : this(name, false)
        {}

        /// <summary>
        /// Construct a <see cref="PropertyProcessor"/> for the specified
        /// property name.
        /// </summary>
        /// <param name="name">
        /// A property name.
        /// </param>
        /// <param name="useIs">
        /// If <b>true</b>, the getter method will be prefixed with "Is".
        /// </param>
        public PropertyProcessor(string name, bool useIs)
            : this(name == null ? null : new PropertyManipulator(name, useIs))
        {}

        /// <summary>
        /// Construct a <b>PropertyProcessor</b> based for the specified
        /// <see cref="PropertyManipulator"/>.
        /// </summary>
        /// <param name="manipulator">
        /// A <b>PropertyManipulator</b>; could be <c>null</c>.
        /// </param>
        public PropertyProcessor(PropertyManipulator manipulator)
        {
            m_manipulator = manipulator;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Get the property value from the passed entry object.
        /// </summary>
        /// <param name="entry">
        /// The Entry object.
        /// </param>
        /// <returns>
        /// The property value.
        /// </returns>
        /// <seealso cref="IValueExtractor.Extract"/>
        protected virtual object Get(IInvocableCacheEntry entry)
        {
            IValueManipulator manipulator = m_manipulator;
            if (manipulator != null)
            {
                IValueExtractor extractor = manipulator.Extractor;
                if (extractor == null)
                {
                    throw new InvalidOperationException("The IValueManipulator ("
                            + manipulator + ") failed to provide an IValueExtractor");
                }
                else
                {
                    return entry.Extract(extractor);
                }
            }
            return entry.Value;
        }

        /// <summary>
        /// Set the property value into the passed entry object.
        /// </summary>
        /// <param name="entry">
        /// The entry object.
        /// </param>
        /// <param name="value">
        /// A new property value.
        /// </param>
        /// <seealso cref="IValueUpdater.Update"/>
        protected virtual void Set(IInvocableCacheEntry entry, object value)
        {
            IValueManipulator manipulator = m_manipulator;
            if (m_manipulator != null)
            {
                IValueUpdater updater = manipulator.Updater;
                if (updater != null)
                {
                    entry.Update(updater, value);
                    return;
                }
            }
            entry.SetValue(value, false);
        }

        /// <summary>
        ///  Returns this <b>PropertyProcessor</b>'s description.
        /// </summary>
        /// <returns>
        /// This <b>PropertyProcessor</b>'s description.
        /// </returns>
        abstract protected string Description
        {
            get;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>PropertyProcessor</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>PropertyProcessor</b> and the passed object
        /// are equivalent <b>PropertyProcessor</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is PropertyProcessor)
            {
                PropertyProcessor that = (PropertyProcessor) o;
                return Equals(m_manipulator, that.m_manipulator);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>PropertyProcessor</b> object
        /// according to the general <see cref="object.GetHashCode()"/>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>PropertyProcessor</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_manipulator.GetHashCode();
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>PropertyProcessor</b>.
        /// </summary>
        /// <returns>
        /// A <b>String</b> description of the <b>PropertyProcessor</b>.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + '(' + m_manipulator + Description + ')';
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
            m_manipulator = (IValueManipulator) reader.ReadObject(0);
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
            writer.WriteObject(0, m_manipulator);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The property value manipulator.
        /// </summary>
        protected IValueManipulator m_manipulator;

        #endregion
    }
}