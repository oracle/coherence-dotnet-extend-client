/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// An <see cref="ICacheLoader"/> extension that exposes an <b>IEnumerator</b>
    /// for the collection of keys in the underlying <see cref="ICache"/>.
    /// </summary>
    public interface IIterableCacheLoader : ICacheLoader
    {
        /// <summary>
        /// Gets the <b>IEnumerator</b> for the keys collection.
        /// </summary>
        /// <value>
        /// The <b>IEnumerator</b> for the keys collection.
        /// </value>
        IEnumerator Keys
        {
            get;
        }
    }
}