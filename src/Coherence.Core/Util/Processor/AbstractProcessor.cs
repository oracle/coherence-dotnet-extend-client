/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Net.Cache;
using Tangosol.Util.Collections;

namespace Tangosol.Util.Processor
{
    /// <summary>
    /// An <b>AbstractProcessor</b> is a partial
    /// <see cref="IEntryProcessor"/> implementation that provides the
    /// default implementation of the
    /// <see cref="IEntryProcessor.ProcessAll"/> method.
    /// </summary>
    /// <remarks>
    /// For EntryProcessors which only run within the Coherence cluster
    /// (most common case), the .NET Process and ProcessAll methods can be left
    /// unimplemented.
    /// </remarks>
    /// <author>Cameron Purdy  2005.07.19</author>
    /// <author>Jason Howes  2005.07.19</author>
    /// <author>Ivan Cikic  2005.07.19</author>
    /// <since>Coherence 3.1</since>
    public abstract class AbstractProcessor : IEntryProcessor
    {
        /// <summary>
        /// Process an <see cref="IInvocableCacheEntry"/>.
        /// </summary>
        /// <remarks>
        /// This implementation throws a NotSupportedException.
        /// </remarks>
        /// <param name="entry">
        /// The <b>IInvocableCacheEntry</b> to process.
        /// </param>
        /// <returns>
        /// The result of the processing, if any.
        /// </returns>
        /// <since>12.2.1.3</since>
        public virtual object Process(IInvocableCacheEntry entry)
        {
            throw new NotSupportedException(
                "This entry processor cannot be invoked on the client.");
        }

        /// <summary>
        /// Process a collection of <see cref="IInvocableCacheEntry"/>
        /// objects.
        /// </summary>
        /// <remarks>
        /// This method is semantically equivalent to:
        /// <pre>
        /// IDictionary results = new Hashtable();
        /// foreach (IInvocableCacheEntry entry in entries)
        /// {
        ///     results[entry.Key] = Process(entry);
        /// }
        /// return results;
        /// </pre>
        /// </remarks>
        /// <param name="entries">
        /// A read-only collection of <b>IInvocableCacheEntry</b>
        /// objects to process.
        /// </param>
        /// <returns>
        /// A dictionary containing the results of the processing, up to one
        /// entry for each <b>IInvocableCacheEntry</b> that was
        /// processed, keyed by the keys of the dictionary that were
        /// processed, with a corresponding value being the result of the
        /// processing for each key.
        /// </returns>
        public virtual IDictionary ProcessAll(ICollection entries)
        {
            IDictionary results = new HashDictionary();
            foreach (IInvocableCacheEntry entry in entries)
            {
                results[entry.Key] = Process(entry);
            }
            return results;
        }
    }
}