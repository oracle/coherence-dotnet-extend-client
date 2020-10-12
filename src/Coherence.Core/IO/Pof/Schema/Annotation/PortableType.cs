/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.IO.Pof.Schema.Annotation
{
    /// <summary>
    /// Class-level annotation that marks class as portable and defines type
    /// identifier (and optionally implementation version) for it.
    /// </summary>
    /// <author>Aleksandar Seovic  2013.11.01</author>
    /// <since>Coherence 12.2.1</since>
    [AttributeUsage(AttributeTargets.Class)]
    public class PortableType : Attribute
    {
        #region Constructors

        /// <summary>
        /// Construct PortableType instance with a default implementation version of 0.
        /// </summary>
        /// <param name="id">Type identifier</param>
        public PortableType(int id)
            : this(id, 0)
        {
        }

        /// <summary>
        /// Construct PortableType instance with a specified type id and implementation version.
        /// </summary>
        /// <param name="id">Type identifier</param>
        /// <param name="version">Implementation version</param>
        public PortableType(int id, int version)
        {
            m_id = id;
            m_version = version;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Return type identifier.
        /// </summary>
        public int Id
        {
            get { return m_id; }
        }

        /// <summary>
        /// Return implementation version.
        /// </summary>
        public int Version
        {
            get { return m_version; }
        }

        #endregion

        #region Data members

        /// <summary>
        /// Type identifier
        /// </summary>
        private readonly int m_id;

        /// <summary>
        /// Implementation version
        /// </summary>
        private readonly int m_version;

        #endregion
    }
}