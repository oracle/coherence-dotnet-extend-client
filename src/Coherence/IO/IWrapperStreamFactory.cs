/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

namespace Tangosol.IO
{
    /// <summary>
    /// Provides the means to wrap a <b>Stream</b>, such that functionality
    /// such as compression and encryption can be implemented in a layered,
    /// pluggable fashion.
    /// </summary>
    /// <author>Cameron Purdy  2002.08.19</author>
    /// <author>Ivan Cikic  2007.05.03</author>
    [Obsolete ("Obsolete as of Coherence 3.7.")]
    public interface IWrapperStreamFactory
    {
        /// <summary>
        /// Requests an input <b>Stream</b> that wraps the passed
        /// <b>Stream</b>.
        /// </summary>
        /// <param name="stream">
        /// The <b>Stream</b> to be wrapped.
        /// </param>
        /// <returns>
        /// A <b>Stream</b> that delegates to ("wraps") the passed
        /// <b>Stream</b>.
        /// </returns>
        Stream GetInputStream(Stream stream);

        /// <summary>
        /// Requests an output <b>Stream</b> that wraps the passed
        /// <b>Stream</b>.
        /// </summary>
        /// <param name="stream">
        /// The <b>Stream</b> to be wrapped.
        /// </param>
        /// <returns>
        /// A <b>Stream</b> that delegates to ("wraps") the passed
        /// <b>Stream</b>.
        /// </returns>
        Stream GetOutputStream(Stream stream);
    }
}