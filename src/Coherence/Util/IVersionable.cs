/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Util
{
    /// <summary>
    /// An interface for versionable data.
    /// </summary>
    /// <author>Cameron Purdy  2000.10.20</author>
    /// <author>Ivan Cikic  2006.10.24</author>
    public interface IVersionable
    {
        /// <summary>
        ///  Get the version indicator for this object.
        /// </summary>
        /// <remarks>
        /// The version indicator should be an immutable object or one
        /// treated as an immutable, which is to say that after the version
        /// is incremented, the previous version's indicator reference will
        /// never be returned again.
        /// </remarks>
        /// <value>
        /// A non-<c>null</c> version value that implements the
        /// <see cref="IComparable"/> interface.
        /// </value>
        IComparable VersionIndicator
        {
            get;
        }

        /// <summary>
        /// Update the version to the next logical version indicator.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the object is immutable or if the object does not know how to
        /// increment its own version indicator.
        /// </exception>
        void IncrementVersion();
    }
}