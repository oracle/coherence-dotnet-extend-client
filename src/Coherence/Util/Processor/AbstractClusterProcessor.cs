/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// Base class for entry processors that may only be executed 
    /// within the cluster.
    /// </summary>
    /// <remarks>
    /// As of 12.2.1.3 this class is deprecated in favor of AbstractProcessor
    /// which now includes a default Process method implementation.
    /// </remarks>
    /// <author>Aleksandar Seovic  2009.09.21</author>
    /// <since>Coherence 3.6</since>
    [Obsolete("since Coherence 12.2.1.3")]
    public abstract class AbstractClusterProcessor 
        : AbstractProcessor, IPortableObject
    {
        #region Implementation of IPortableObject

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
        }

        #endregion
    }
}