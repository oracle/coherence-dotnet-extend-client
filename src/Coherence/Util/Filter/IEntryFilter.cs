/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Net.Cache;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// IEntryFilter provides an extension to <see cref="IFilter"/> for those
    /// cases in which both a key and a value may be necessary to evaluate
    /// the conditional inclusion of a particular object.
    /// </summary>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.11.01</author>
    /// <author>Goran Milosavljevic  2006.10.20</author>
    public interface IEntryFilter : IFilter
    {
        /// <summary>
        /// Apply the test to an <see cref="ICacheEntry"/>.
        /// </summary>
        /// <param name="entry">
        /// The <b>ICacheEntry</b> to evaluate; never <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        bool EvaluateEntry(ICacheEntry entry);
    }
}