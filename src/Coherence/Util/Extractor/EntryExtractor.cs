/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿
using System.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// The <b>EntryExtractor</b> is a base abstract class for special
    /// purpose custom <see cref="IValueExtractor"/> implementations.
    /// </summary>
    /// <remarks>
    /// It allows them to extract a desired value using all available
    /// information on the corresponding <see cref="ICacheEntry "/> object
    /// and is intended to be used in advanced custom scenarios, when
    /// application code needs to look at both key and value at the same time
    /// or can make some very specific assumptions regarding to the
    /// implementation details of the underlying Entry object.
    /// As of Coherence 3.5, the same behavior can be achieved by subclasses
    /// of the <see cref="AbstractExtractor "/> by overriding the
    /// <see cref="AbstractExtractor.ExtractFromEntry"/>.
    /// </remarks>
    /// <author>Gene Gleyzer 2008.04.14</author>
    /// <author>Ivan Cikic 2009.04.01</author>
    /// <since>Coherence 3.4</since>
    public abstract class EntryExtractor : AbstractExtractor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor (for backward compability)
        /// </summary>
        public EntryExtractor() : this(VALUE)
        {}

        /// <summary>
        /// Construct an <b>EntryExtractor</b> based on the entry
        /// extraction target.
        /// </summary>
        /// <param name="target">
        /// One of the <see cref="AbstractExtractor.VALUE"/> or
        /// <see cref="AbstractExtractor.KEY"/> values.
        /// </param>
        /// <since>Coherence 3.5</since>
        public EntryExtractor(int target)
        {
            m_target = target;
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
            m_target = reader.ReadInt32(0);
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
            writer.WriteInt32(0, m_target);
        }

        #endregion
    }
}