/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

using Tangosol.Util;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// An <b>IInvocableCache</b> is an <see cref="ICache"/> against which both
    /// entry-targeted processing and aggregating operations can be invoked.
    /// </summary>
    /// <remarks>
    /// <p>
    /// While a traditional model for working with a dictionary is to have an
    /// operation access and mutate the dictionary directly through its API,
    /// the IInvocableCache allows that model of operation to be inverted
    /// such that the operations against the cache contents are executed by
    /// (and thus within the localized context of) a cache.
    /// This is particularly useful in a distributed environment, because it
    /// enables the processing to be moved to the location at which the
    /// entries-to-be-processed are being managed, thus providing efficiency
    /// by localization of processing.</p>
    /// </remarks>
    /// <author>Cameron Purdy, Gene Gleyzer, Jason Howes  2005.07.19</author>
    /// <author>Aleksandar Seovic  2007.07.12</author>
    public interface IInvocableCache : ICache
    {

        /// <summary>
        /// Invoke the passed <see cref="IEntryProcessor"/> against the entry
        /// specified by the passed key, returning the result of the
        /// invocation.
        /// </summary>
        /// <param name="key">
        /// The key to process; it is not required to exist within the
        /// dictionary.
        /// </param>
        /// <param name="agent">
        /// The <b>IEntryProcessor</b> to use to process the specified key.
        /// </param>
        /// <returns>
        /// The result of the invocation as returned from the
        /// <b>IEntryProcessor</b>.
        /// </returns>
        object Invoke(object key, IEntryProcessor agent);

        /// <summary>
        /// Invoke the passed <see cref="IEntryProcessor"/> against the
        /// entries specified by the passed keys, returning the result of the
        /// invocation for each.
        /// </summary>
        /// <param name="keys">
        /// The keys to process; these keys are not required to exist within
        /// the dictionary.
        /// </param>
        /// <param name="agent">
        /// The <b>IEntryProcessor</b> to use to process the specified keys.
        /// </param>
        /// <returns>
        /// A dictionary containing the results of invoking the
        /// <b>IEntryProcessor</b> against each of the specified keys.
        /// </returns>
        IDictionary InvokeAll(ICollection keys, IEntryProcessor agent);

        /// <summary>
        /// Invoke the passed <see cref="IEntryProcessor"/> against the set
        /// of entries that are selected by the given <see cref="IFilter"/>,
        /// returning the result of the invocation for each.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Unless specified otherwise, IInvocableCache implementations
        /// will perform this operation in two steps: (1) use the filter to
        /// retrieve a matching entry collection; (2) apply the agent to
        /// every filtered entry. This algorithm assumes that the agent's
        /// processing does not affect the result of the specified filter
        /// evaluation, since the filtering and processing could be
        /// performed in parallel on different threads.</p>
        /// <p>
        /// If this assumption does not hold, the processor logic has to be
        /// idempotent, or at least re-evaluate the filter. This could be
        /// easily accomplished by wrapping the processor with the
        /// <see cref="ConditionalProcessor"/>.</p>
        /// </remarks>
        /// <param name="filter">
        /// An <see cref="IFilter"/> that results in the collection of keys to
        /// be processed.
        /// </param>
        /// <param name="agent">
        /// The <see cref="IEntryProcessor"/> to use to process the specified
        /// keys.
        /// </param>
        /// <returns>
        /// A dictionary containing the results of invoking the
        /// <b>IEntryProcessor</b> against the keys that are selected by the
        /// given <b>IFilter</b>.
        /// </returns>
        IDictionary InvokeAll(IFilter filter, IEntryProcessor agent);

        /// <summary>
        /// Perform an aggregating operation against the entries specified by
        /// the passed keys.
        /// </summary>
        /// <param name="keys">
        /// The collection of keys that specify the entries within this cache
        /// to aggregate across.
        /// </param>
        /// <param name="agent">
        /// The <see cref="IEntryAggregator"/> that is used to aggregate
        /// across the specified entries of this dictionary.
        /// </param>
        /// <returns>
        /// The result of the aggregation.
        /// </returns>
        object Aggregate(ICollection keys, IEntryAggregator agent);

        /// <summary>
        /// Perform an aggregating operation against the collection of
        /// entries that are selected by the given <b>IFilter</b>.
        /// </summary>
        /// <param name="filter">
        /// an <see cref="IFilter"/> that is used to select entries within
        /// this cache to aggregate across.
        /// </param>
        /// <param name="agent">
        /// The <see cref="IEntryAggregator"/> that is used to aggregate
        /// across the selected entries of this dictionary.
        /// </param>
        /// <returns>
        /// The result of the aggregation.
        /// </returns>
        object Aggregate(IFilter filter, IEntryAggregator agent);

    }

    /// <summary>
    /// An <b>IInvocableCacheEntry</b> contains additional information and
    /// exposes additional operations that the basic <b>ICacheEntry</b>
    /// does not.
    /// </summary>
    /// <remarks>
    /// It allows non-existent entries to be represented, thus allowing
    /// their optional creation. It allows existent entries to be removed
    /// from the cache. It supports a number of optimizations that can
    /// ultimately be mapped through to indexes and other data structures of
    /// the underlying dictionary.
    /// </remarks>
    public interface IInvocableCacheEntry : ICacheEntry
    {
        /// <summary>
        /// Gets the key corresponding to this entry.
        /// </summary>
        /// <remarks>
        /// The resultant key does not necessarily exist within the
        /// containing cache, which is to say that
        /// <b>IInvocableCache.Contains(Key)</b> could return
        /// <b>false</b>. To test for the presence of this key within the
        /// dictionary, use <see cref="IsPresent"/>, and to create the entry
        /// for the key, set <see cref="Value"/> property.
        /// </remarks>
        /// <value>
        /// The key corresponding to this entry; may be <c>null</c> if the
        /// underlying cache supports <c>null</c> keys.
        /// </value>
        new object Key { get; }

        /// <summary>
        /// Gets or sets the value corresponding to this entry.
        /// </summary>
        /// <remarks>
        /// If the entry does not exist, then the value will be <c>null</c>.
        /// <p>
        /// To differentiate between a <c>null</c> value and a non-existent
        /// entry, use <see cref="IsPresent"/>.</p>
        /// </remarks>
        /// <value>
        /// The value corresponding to this entry; may be <c>null</c> if the
        /// value is <c>null</c> or if the entry does not exist in the cache.
        /// </value>
        new object Value { get; set; }

        /// <summary>
        /// Store the value corresponding to this entry.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the entry does not exist, then the entry will be created by
        /// invoking this method, even with a <c>null</c> value (assuming the
        /// cache supports <c>null</c> values).</p>
        /// <p>
        /// Unlike the property <see cref="Value"/>, this method does not
        /// return the previous value, and as a result may be significantly
        /// less expensive (in terms of cost of execution) for certain
        /// cache implementations.</p>
        /// </remarks>
        /// <param name="value">
        /// The new value for this entry.
        /// </param>
        /// <param name="isSynthetic">
        /// Pass <b>true</b> only if the insertion into or modification of
        /// the cache should be treated as a synthetic event.
        /// </param>
        void SetValue(object value, bool isSynthetic);

        /// <summary>
        /// Determine if this entry exists in the cache.
        /// </summary>
        /// <remarks>
        /// If the entry is not present, it can be created by setting the
        /// <see cref="Value"/> property. If the entry is present,
        /// it can be destroyed by calling <see cref="Remove"/>.
        /// </remarks>
        /// <value>
        /// <b>true</b> if this entry exists in the containing cache.
        /// </value>
        bool IsPresent { get; }

        /// <summary>
        /// Extract a value out of the entry's value.
        /// </summary>
        /// <remarks>
        /// Calling this method is semantically equivalent to
        /// <b>extractor.Extract(entry.Value)</b>, but this method may be
        /// significantly less expensive because the resultant value may be
        /// obtained from a forward index, for example.
        /// </remarks>
        /// <param name="extractor">
        /// An <see cref="IValueExtractor"/> to apply to the entry's value
        /// </param>
        /// <returns>
        /// The extracted value.
        /// </returns>
        object Extract(IValueExtractor extractor);

        /// <summary>
        /// Update the entry's value.
        /// </summary>
        /// <remarks>
        /// Calling this method is semantically equivalent to:
        /// <pre>
        /// object target = entry.Value;
        /// updater.Update(target, value);
        /// entry.Value = target;
        /// </pre>
        /// The benefit of using this method is that it may allow the entry
        /// implementation to significantly optimize the operation, such as
        /// for purposes of delta updates and backup maintenance.
        /// </remarks>
        /// <param name="updater">
        /// An <see cref="IValueUpdater"/> used to modify the entry's value.
        /// </param>
        /// <param name="value">
        /// Value to update target object to.
        /// </param>
        void Update(IValueUpdater updater, object value);

        /// <summary>
        /// Remove this entry from the cache if it is present in the cache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method supports both the operation corresponding to
        /// <b>IDictionary.Remove</b> as well as synthetic operations such as
        /// eviction. If the containing cache does not differentiate between
        /// the two, then this method will always be identical to
        /// <tt>IInvocableCache.Remove(Key)</tt>.</p>
        /// </remarks>
        /// <param name="isSynthetic">
        /// Pass <b>true</b> only if the removal from the dictionary should
        /// be treated as a synthetic event.
        /// </param>
        void Remove(bool isSynthetic);
    }

    /// <summary>
    /// An invocable agent that operates against the entry objects within a
    /// cache.
    /// </summary>
    public interface IEntryProcessor
    {
        /// <summary>
        /// Process an <see cref="IInvocableCacheEntry"/>.
        /// </summary>
        /// <param name="entry">
        /// The <b>IInvocableCacheEntry</b> to process.
        /// </param>
        /// <returns>
        /// The result of the processing, if any.
        /// </returns>
        object Process(IInvocableCacheEntry entry);

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
        /// A collection of <b>IInvocableCacheEntry</b> objects to process.
        /// </param>
        /// <returns>
        /// A dictionary containing the results of the processing, up to one
        /// entry for each <b>IInvocableCacheEntry</b> that was processed,
        /// keyed by the keys of the dictionary that were processed, with a
        /// corresponding value being the result of the processing for each
        /// key.
        /// </returns>
        IDictionary ProcessAll(ICollection entries);
    }

    /// <summary>
    /// An <b>IEntryAggregator</b> represents processing that can be directed to
    /// occur against some subset of the entries in an
    /// <see cref="IInvocableCache"/>, resulting in a aggregated result.
    /// </summary>
    /// <remarks>
    /// Common examples of aggregation include functions such as Min(),
    /// Max(), Sum() and Avg(). However, the concept of aggregation applies
    /// to any process that needs to evaluate a group of entries to come up
    /// with a single answer.
    /// </remarks>
    public interface IEntryAggregator
    {
        /// <summary>
        /// Process a set of <see cref="IInvocableCacheEntry"/> objects
        /// in order to produce an aggregated result.
        /// </summary>
        /// <param name="entries">
        /// A collection of read-only <b>IInvocableCacheEntry</b>
        /// objects to aggregate.
        /// </param>
        /// <returns>
        /// The aggregated result from processing the entries.
        /// </returns>
        object Aggregate(ICollection entries);
    }

    /// <summary>
    /// An <b>IParallelAwareAggregator</b> is an advanced extension to
    /// <see cref="IEntryAggregator"/> that is explicitly capable of being
    /// run in parallel, for example in a distributed environment.
    /// </summary>
    public interface IParallelAwareAggregator : IEntryAggregator
    {
        /// <summary>
        /// Get an aggregator that can take the place of this aggregator in
        /// situations in which the <see cref="IInvocableCache"/> can
        /// aggregate in parallel.
        /// </summary>
        /// <value>
        /// The aggregator that will be run in parallel.
        /// </value>
        IEntryAggregator ParallelAggregator { get; }

        /// <summary>
        /// Aggregate the results of the parallel aggregations.
        /// </summary>
        /// <param name="results">
        /// Results to aggregate.
        /// </param>
        /// <returns>
        /// The aggregation of the parallel aggregation results.
        /// </returns>
        object AggregateResults(ICollection results);
    }

    /// <summary>
    /// PartialResultAggregator allows for the intermediate {@link
    /// #AggregatePartialResults aggregation} of the partial results of a
    /// {@link ParallelAwareAggregator parallel aggregation}.
    /// </summary>
    public interface IPartialResultAggregator
    {
        /// <summary>
        /// Aggregate the results of the parallel aggregations, producing a
        /// partial result logically representing the partial aggregation. The
        /// returned partial result will be further {@link
        /// ParallelAwareAggregator#AggregateResults aggregated} to produce
        /// the final result.
        /// </summary>
        /// <param name="partialResults">
        /// The partial results to agregate.
        /// </param>
        /// <returns>
        /// An aggregattion of the collection of partial results.
        /// </returns>
        object AggregatePartialResults(ICollection partialResults);
    }
}