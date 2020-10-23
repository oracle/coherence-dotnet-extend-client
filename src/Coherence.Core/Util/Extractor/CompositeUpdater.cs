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
using Tangosol.Util;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// An <see cref="IValueUpdater"/> implementation based on an
    /// extractor-updater pair.
    /// </summary>
    /// <author>Gene Gleyzer  2005.10.31</author>
    /// <author>Ivan Cikic  2006.10.20</author>
    public class CompositeUpdater : IValueUpdater, IPortableObject
    {
        #region Properties

        /// <summary>
        /// The <see cref="IValueExtractor"/> part.
        /// </summary>
        /// <value>
        /// The <b>IValueExtractor</b> part.
        /// </value>
        public virtual IValueExtractor Extractor
        {
            get { return m_extractor; }
        }

        /// <summary>
        /// The <see cref="IValueUpdater"/> part.
        /// </summary>
        /// <value>
        /// The <b>IValueUpdater</b> part.
        /// </value>
        public virtual IValueUpdater Updater
        {
            get { return m_updater; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CompositeUpdater()
        {}

        /// <summary>
        /// Construct a <b>CompositeUpdater</b> based on the specified
        /// extractor and updater.
        /// </summary>
        /// <remarks>
        /// <b>Note:</b> the extractor and updater here are not symmetrical
        /// in nature: the extractor is used to "drill-down" to the target
        /// object, while the updater will operate on that extracted object.
        /// </remarks>
        /// <param name="extractor">
        /// The <see cref="IValueExtractor"/>.
        /// </param>
        /// <param name="updater">
        /// The <see cref="IValueUpdater"/>.
        /// </param>
        public CompositeUpdater(IValueExtractor extractor, IValueUpdater updater)
        {
            Debug.Assert(extractor != null && updater != null);
            m_extractor = extractor;
            m_updater   = updater;
        }

        /// <summary>
        /// Construct a <b>CompositeUpdater</b> for a specified method name
        /// sequence.
        /// </summary>
        /// <remarks>
        /// For example: "Address.Zip" property will indicate that
        /// the "Address" property should be used to extract an Address
        /// object, which will then be used by the "Zip" call.
        /// </remarks>
        /// <param name="name">
        /// A dot-delimited sequence of N method names which results in a
        /// <b>CompositeUpdater</b> that is based on an chain of (N-1)
        /// <see cref="ReflectionExtractor"/> objects and a single
        /// <see cref="ReflectionUpdater"/>.
        /// </param>
        public CompositeUpdater(string name)
        {
            Debug.Assert(name != null && name.Length > 0);

            int ofLast = name.LastIndexOf('.');

            m_extractor = ofLast == -1
                          ? (IValueExtractor) IdentityExtractor.Instance
                          : new ChainedExtractor(name.Substring(0, ofLast));
            m_updater   = new ReflectionUpdater(name.Substring(ofLast + 1));
        }

        #endregion

        #region IValueUpdater implementation

        /// <summary>
        /// Update the state of the passed target object using the passed
        /// value.
        /// </summary>
        /// <param name="target">
        /// The object to update the state of.
        /// </param>
        /// <param name="value">
        /// The new value to update the state with.
        /// </param>
        /// <exception cref="InvalidCastException">
        /// If this IValueUpdater is incompatible with the passed target
        /// object or the value and the implementation <b>requires</b> the
        /// passed object or the value to be of a certain type.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this IValueUpdater cannot handle the passed target object or
        /// value for any other reason; an implementor should include a
        /// descriptive message.
        /// </exception>
        public virtual void Update(object target, object value)
        {
            Updater.Update(Extractor.Extract(target), value);
        }

        #endregion

        #region Object override methods

        /// <summary>
        ///  Return a human-readable description for this
        /// <b>CompositeUpdater</b>.
        /// </summary>
        /// <returns>
        /// A String description of the <b>CompositeUpdater</b>.
        /// </returns>
        public override string ToString()
        {
            return "CompositeUpdater(" + m_extractor + ", " + m_updater + ')';
        }

        /// <summary>
        /// Compare the <see cref="IValueUpdater"/> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <see cref="IValueUpdater"/> and the passed
        /// object are quivalent <b>IValueUpdater</b>s.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is CompositeUpdater)
            {
                CompositeUpdater that = (CompositeUpdater) o;
                return m_extractor.Equals(that.m_extractor) && m_updater.Equals(that.m_updater);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <see cref="IValueUpdater"/>
        /// object according to the general <b>object.GetHashCode</b>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>IValueUpdater</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_extractor.GetHashCode() + m_updater.GetHashCode();
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
            m_extractor = (IValueExtractor) reader.ReadObject(0);
            m_updater   = (IValueUpdater) reader.ReadObject(1);
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
            writer.WriteObject(0, m_extractor);
            writer.WriteObject(1, m_updater);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The <see cref="IValueExtractor"/> part.
        /// </summary>
        protected IValueExtractor m_extractor;

        /// <summary>
        /// The <see cref="IValueUpdater"/> part.
        /// </summary>
        protected IValueUpdater m_updater;

        #endregion
    }
}