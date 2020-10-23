/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

using Tangosol.Net.Cache;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// IIndexAwareExtractor is an extension to the <see cref="IValueExtractor"/>
    /// interface that supports the creation and destruction of an 
    /// <see cref="ICacheIndex"/> index. Instances of this interface are 
    /// intended to be used with the <see cref="IQueryCache.AddIndex"/> and
    /// <see cref="IQueryCache.RemoveIndex"/> API to support the creation of 
    /// custom indexes.
    /// </summary>
    /// <author>Tom Beerbower  2010.02.08</author>
    /// <author>Jason Howes  2010.10.01</author>
    public interface IIndexAwareExtractor : IValueExtractor
    {
        /// <summary>
        /// Create an index and associate it with the corresponding extractor.
        /// <para>
        /// Important: it is a responsibility of this method's implementations
        /// to place the necessary &lt;IValueExtractor, ICacheEntry&gt; entry 
        /// into the given map of indexes.
        /// </para>
        /// </summary>
        /// <param name="ordered">
        /// <c>true</c> iff the contents of the indexed information should be 
        /// ordered; <c>false</c> otherwise.
        /// </param>
        /// <param name="comparer">
        /// The IComparator object which imposes an ordering of entries in the 
        /// index contents; or <c>null</c> if the entries' values natural 
        /// ordering should be used.
        /// </param>
        /// <param name="dict">
        /// IDictionary to be updated with the created index.
        /// </param>
        /// <returns>
        /// The created index; <c>null</c> if the index has not been created.
        /// </returns>
        ICacheIndex CreateIndex(bool ordered, IComparer comparer, IDictionary dict);

        /// <summary>
        /// Destroy an existing index and remove it from the given dictionary
        /// of indexes. 
        /// </summary>
        /// <param name="dict">
        /// IDictionary to be updated by removing the index being destroyed.
        /// </param>
        /// <returns>
        /// The destroyed index; <c>null</c> if the index does not exist.
        /// </returns>
        ICacheIndex DestroyIndex(IDictionary dict);
    }
}