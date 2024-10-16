/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// Universal <see cref="IValueUpdater"/> implementation.
    /// </summary>
    /// <remarks>
    /// UniversalUpdater can only run within the Coherence cluster.
    /// Refer to the Coherence for Java documentation for more information.
    /// </remarks>
    /// <author>Gene Gleyzer 2005.10.27</author>
    /// <author>Joe Fialli 2017.11.28</author>
    /// <author>Patrick Fry 2024.09.23</author>
    /// <since>14.1.2.0.0</since>
	public class UniversalUpdater : IValueUpdater, IPortableObject
	{
        #region Properties

        /// <summary>
        /// Get the method or property name.
        /// </summary>
        /// <value>
        /// the method or property name.
        /// </value>
        public virtual string Name
        {
            get { return m_name; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor (necessary for the IPortableObject interface).
        /// </summary>
        public UniversalUpdater()
		{
        }

       /// <summary>
       /// Construct a UniversalUpdater for the provided name.
       /// </summary>
       /// <param name="name">
       /// A method or property name.
       /// </param>
        public UniversalUpdater(string name)
        {
            Debug.Assert(name != null);

            m_name = name;
        }

        #endregion

        #region IValueUpdater implementation

        /// <summary>
        /// Update the passed target object using the specified value.
        /// </summary>
        /// <remarks>
        /// This method will always throw a <see cref="NotSupportedException"/>
        /// if called directly by the .NET client application, as its execution
        /// is only meaningful within the cluster.
        /// </remarks>
        /// <param name="target">
        /// The object to update.
        /// </param>
        /// <param name="value">
        /// The new value to update the target's property with.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// Always, as it is expected that this extractor will only be 
        /// executed within the cluster.
        /// </exception>
        public void Update(object target, object value)
        {
            throw new NotSupportedException();
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
        public void ReadExternal(IPofReader reader)
        {
            m_name = reader.ReadString(0);
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
        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, Name);
        }

        #endregion

        #region Data members

        /// <summary>
        /// A method name, or a property name.
        /// </summary>
        protected string m_name;

        #endregion
    }
}
