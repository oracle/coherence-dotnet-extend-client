/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Net.Cache;

namespace Tangosol.Util.Comparator {

    /// <summary>
    ///  This interface is used by <b>IComparer</b> implementations that can
    ///  use value extraction optimization exposed by the
    /// <see cref="IQueryCacheEntry"/>.
    /// </summary>
    /// <author>Cameron Purdy, Gene Gleyzer  2002.12.13, 2006.06.12</author>
    /// <author>Ana Cikic  2006.09.12</author>
    public interface IQueryCacheComparer : IComparer
    {

        /// <summary>
        /// Compare two entries based on the rules specified by
        /// <b>IComparer</b>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If possible, use the <see cref="IQueryCacheEntry.Extract"/>
        /// method to optimize the value extraction process.</p>
        /// <p>
        /// This method is expected to be implemented by <b>IComparer</b>
        /// wrappers, which simply pass on this invocation to the wrapped
        /// <b>IComparer</b> objects if they too implement this interface, or
        /// to invoke their default compare method passing the actual objects
        /// (not the extracted values) obtained from the extractor using the
        /// passed entries.</p>
        /// <p>
        /// This interface is also expected to be implemented by
        /// <see cref="IValueExtractor"/> implementations that implement the
        /// <b>IComparer</b> interface. It is expected that in most cases,
        /// the <b>IComparer</b> wrappers will eventually terminate at (i.e.
        /// delegate to) <b>IValueExtractors</b> that also implement this
        /// interface.</p>
        /// </remarks>
        /// <param name="entry1">
        /// The first entry to compare values from; read-only.
        /// </param>
        /// <param name="entry2">
        /// The second entry to compare values from; read-only.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as the first
        /// entry denotes a value that is is less than, equal to, or greater
        /// than the value denoted by the second entry.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// If the arguments' types prevent them from being compared by this
        /// <b>IComparer</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the extractor cannot handle the passed objects for any other
        /// reason; an implementor should include a descriptive message.
        /// </exception>
        /// <since>Coherence 3.2</since>
        int CompareEntries(IQueryCacheEntry entry1, IQueryCacheEntry entry2);
    }
}