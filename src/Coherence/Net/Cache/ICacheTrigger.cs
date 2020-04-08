/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using Tangosol.Util;
using Tangosol.Util.Filter;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// ICacheTrigger represents a functional agent that allows to validate,
    /// reject or modify mutating operations against an underlying cache.
    /// </summary>
    /// <remarks>
    /// The trigger operates on <see cref="ICacheTriggerEntry"/> object that
    /// represents a pending mutation that is about to be committed to the
    /// underlying cache. An ICacheTrigger could be registered with any
    /// <see cref="IObservableCache"/> using the
    /// <see cref="CacheTriggerListener"/> class:
    /// <code>
    /// INamedCache   cache   = CacheFactory.GetCache(cacheName);
    /// ICacheTrigger trigger = new MyCustomTrigger();
    /// cache.AddCacheListener(new CacheTriggerListener(trigger));
    /// </code>
    /// <b>Note:</b> In a clustered environment, ICacheTrigger registration
    /// process requires triggers to be serializable and providing a
    /// non-default implementation of the GetHashCode() and Equals() methods.
    /// Failure to do so may result in duplicate registration and a redundant
    /// entry processing by equivalent, but "not equal" ICacheTrigger
    /// objects.
    /// </remarks>
    /// <author>Cameron Purdy/Gene Gleyzer  2008.03.11</author>
    /// <author>Ana Cikic  2008.07.02</author>
    /// <since>Coherence 3.4</since>
    /// <seealso cref="FilterTrigger"/>
    public interface ICacheTrigger
    {
        /// <summary>
        /// This method is called before the result of a mutating operation
        /// represented by the specified entry object is committed into the
        /// underlying cache.
        /// </summary>
        /// <remarks>
        /// An implementation of this method can evaluate the change by
        /// analyzing the original and the new value, and can perform any of
        /// the following:
        /// <list type="bullet">
        /// <item>
        /// override the requested change by setting
        /// <see cref="IInvocableCacheEntry.Value"/> to a different value;
        /// </item>
        /// <item>
        /// undo the pending change by resetting the entry value to the
        /// original value obtained from
        /// <see cref="ICacheTriggerEntry.OriginalValue"/>
        /// </item>
        /// <item>
        /// remove the entry from the underlying cache by calling
        /// <see cref="IInvocableCacheEntry.Remove"/>
        /// </item>
        /// <item>
        /// reject the pending change by throwing an <see cref="Exception"/>,
        /// which will prevent any changes from being committed, and will
        /// result in the exception being thrown from the operation that
        /// attempted to modify the cache; or
        /// </item>
        /// <item>
        /// do nothing, thus allowing the pending change to be committed to
        /// the underlying cache.
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="entry">
        /// An <see cref="ICacheTriggerEntry"/> object that represents the
        /// pending change to be committed to the cache, as well as the
        /// original state of the entry.
        /// </param>
        void Process(ICacheTriggerEntry entry);
    }

    /// <summary>
    /// A <see cref="ICacheTrigger"/> entry represents a pending change to an
    /// entry that is about to committed to the underlying cache.
    /// </summary>
    /// <remarks>
    /// The methods inherited from <see cref="IInvocableCacheEntry"/> provide
    /// both the pending state and the ability to alter that state.
    /// The original state of the entry can be obtained using the
    /// <see cref="OriginalValue"/> and <see cref="IsOriginalPresent"/>
    /// properties.
    /// </remarks>
    public interface ICacheTriggerEntry : IInvocableCacheEntry
    {
        /// <summary>
        /// Get the value that existed before the start of the mutating
        /// operation that is being evaluated by the trigger.
        /// </summary>
        /// <value>
        /// The original value corresponding to this entry; may be
        /// <c>null</c> if the value is <c>null</c> or if the entry did not
        /// exist in the cache.
        /// </value>
        object OriginalValue { get; }

        /// <summary>
        /// Determine whether or not the entry existed before the start of
        /// the mutating operation that is being evaluated by the trigger.
        /// </summary>
        /// <value>
        /// <b>true</b> iff this entry was existent in the containing cache.
        /// </value>
        bool IsOriginalPresent { get; }
    }
}