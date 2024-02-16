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
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// PropertyManipulator is a reflection based
    /// <see cref="IValueManipulator"/> implementation.
    /// </summary>
    /// <author>Gene Gleyzer  2005.10.31</author>
    /// <author>Ivan Cikic  2006.10.25</author>
    public class PropertyManipulator : IValueManipulator, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PropertyManipulator()
        {}

        /// <summary>
        /// Construct a <b>PropertyManipulator</b> for the specified property
        /// name.
        /// </summary>
        /// <param name="name">
        /// A property name.
        /// </param>
        public PropertyManipulator(string name) : this(name, false)
        {}

        /// <summary>
        /// Construct a <b>PropertyManipulator</b> for the specified property
        /// name.
        /// </summary>
        /// <param name="name">
        /// A property name.
        /// </param>
        /// <param name="useIs">
        /// If <b>true</b>, the getter method will be prefixed with "Is".
        /// </param>
        public PropertyManipulator(string name, bool useIs)
        {
            Debug.Assert(name != null && name.Length > 0);

            // composite ('.'-delimited) names are supported, but not documented
            m_name  = name;
            m_useIs = useIs;
        }

        #endregion

        #region IValueManipulator implementation

        /// <summary>
        /// Retreive the underlying <see cref="IValueExtractor"/> reference.
        /// </summary>
        /// <value>
        /// The <b>IValueExtractor</b>.
        /// </value>
        public virtual IValueExtractor Extractor
        {
            get
            {
                IValueExtractor extractor = m_extractor;
                if (extractor == null)
                {
                    Init();
                    extractor = m_extractor;
                }
                return extractor;
            }
        }

        /// <summary>
        /// Retreive the underlying <see cref="IValueUpdater"/> reference.
        /// </summary>
        /// <value>
        /// The <b>IValueUpdater</b>.
        /// </value>
        public virtual IValueUpdater Updater
        {
            get
            {
                IValueUpdater updater = m_updater;
                if (updater == null)
                {
                    Init();
                    updater = m_updater;
                }
                return updater;
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Parse the property name and initialize necessary extractor and
        /// updater.
        /// </summary>
        protected internal void Init()
        {
            // allow composite property name (not documented)
            string name   = m_name;
            int    ofLast = name.LastIndexOf('.');
            string prop   = name.Substring(ofLast + 1);

            IValueExtractor extractor = new ReflectionExtractor(prop); //(m_useIs ? "is" : "get") +
            IValueUpdater   updater   = new ReflectionUpdater(prop); //"set" +

            if (ofLast > 0)
            {
                string[] array = name.Substring(0, ofLast).Split(new char[] {'.'});
                int      count = array.Length;

                IValueExtractor[] veGet = new IValueExtractor[count + 1];
                IValueExtractor[] vePut = new IValueExtractor[count];
                for (int i = 0; i < count; i++)
                {
                    veGet[i] = vePut[i] = new ReflectionExtractor(array[i]); //"get" +
                }
                veGet[count] = extractor;

                extractor = new ChainedExtractor(veGet);
                updater   = new CompositeUpdater(count == 1 ? vePut[0] : new ChainedExtractor(vePut),
                                                 updater);

            }
            m_extractor = extractor;
            m_updater   = updater;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>PropertyManipulator</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>PropertyManipulator</b> and the passed
        /// object are equivalent <b>PropertyManipulator</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is PropertyManipulator)
            {
                PropertyManipulator that = (PropertyManipulator)o;
                return m_name.Equals(that.m_name) && m_useIs == that.m_useIs;
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>PropertyManipulator</b> object
        /// according to the general <see cref="object.GetHashCode()"/>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>PropertyManipulator</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_name.GetHashCode();
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>PropertyManipulator</b>.
        /// </summary>
        /// <returns>
        /// A <b>String</b> description of the <b>PropertyManipulator</b>.
        /// </returns>
        public override string ToString()
        {
            return "PropertyManipulator(" + m_name + ')';
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
            m_name  = reader.ReadString(0);
            m_useIs = reader.ReadBoolean(1);
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
            writer.WriteString(0, m_name);
            writer.WriteBoolean(1, m_useIs);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The property name, never <c>null</c>.
        /// </summary>
        protected string m_name;

        /// <summary>
        /// The getter prefix flag.
        /// </summary>
        protected bool m_useIs;

        /// <summary>
        /// A partial <see cref="IValueExtractor"/> used for composite
        /// properties.
        /// </summary>
        [NonSerialized]
        protected IValueExtractor m_extractorPart;

        /// <summary>
        /// The underlying <see cref="IValueExtractor"/>.
        /// </summary>
        [NonSerialized]
        protected IValueExtractor m_extractor;

        /// <summary>
        /// The underlying <see cref="IValueUpdater"/>.
        /// </summary>
        [NonSerialized]
        protected IValueUpdater m_updater;

        #endregion
    }
}