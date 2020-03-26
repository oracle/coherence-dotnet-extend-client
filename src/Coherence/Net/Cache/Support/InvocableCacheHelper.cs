/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Util;
using Tangosol.Util.Collections;
using Tangosol.Util.Comparator;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;

namespace Tangosol.Net.Cache.Support
{
    /// <summary>
    /// Helper methods for <see cref="IInvocableCache"/> implementations
    /// and <see cref="IFilter"/> related evaluations.
    /// </summary>
    /// <author>Gene Gleyzer  2005.10.24</author>
    /// <author>Goran Milosavljevic  2006.10.25</author>
    /// <author>Jason Howes  2010.10.04</author>
    /// <since>Coherence 3.1</since>
    public abstract class InvocableCacheHelper
    {
        #region Invoke/InvokeAll methods

        /// <summary>
        /// Invoke the passed <see cref="IEntryProcessor"/> against the
        /// specified <see cref="IInvocableCacheEntry"/>.
        /// </summary>
        /// <remarks>
        /// The invocation is made thread safe by locking the corresponding key
        /// on the cache.
        /// </remarks>
        /// <param name="cache">
        /// The <see cref="IConcurrentCache"/> that the
        /// <b>IEntryProcessor</b> works against.
        /// </param>
        /// <param name="entry">
        /// The <b>IInvocableCacheEntry</b> to process; it is not required to
        /// exist within the cache.
        /// </param>
        /// <param name="agent">
        /// The <b>IEntryProcessor</b> to use to process the specified key.
        /// </param>
        /// <returns>
        /// The result of the invocation as returned from the
        /// <b>IEntryProcessor</b>.
        /// </returns>
        public static object InvokeLocked(IConcurrentCache cache, 
            IInvocableCacheEntry entry, IEntryProcessor agent)
        {
            var key = entry.Key;
            cache.Lock(key, -1);
            try
            {
                return agent.Process(entry);
            }
            finally
            {
                cache.Unlock(key);
            }
        }

        /// <summary>
        /// Invoke the passed <see cref="IEntryProcessor"/> against the
        /// entries specified by the passed cache and entries.
        /// </summary>
        /// <remarks>
        /// The invocation is made thread safe by locking the corresponding
        /// keys on the cache. If an attempt to lock all the entries at once
        /// fails, they will be processed individually one-by-one.
        /// </remarks>
        /// <param name="cache">
        /// The <see cref="IConcurrentCache"/> that the
        /// <b>IEntryProcessor</b> works against.
        /// </param>
        /// <param name="entries">
        /// A collection of <see cref="IInvocableCacheEntry"/> objects to
        /// process.
        /// </param>
        /// <param name="agent">
        /// The <b>IEntryProcessor</b> to use to process the specified keys.
        /// </param>
        /// <returns>
        /// An <b>IDictionary</b> containing the results of invoking the
        /// <b>IEntryProcessor</b> against each of the specified entry.
        /// </returns>
        public static IDictionary InvokeAllLocked(IConcurrentCache cache, 
            ICollection entries, IEntryProcessor agent)
        {
            ICollection keys = ConverterCollections.GetCollection(entries, 
                ENTRY_TO_KEY_CONVERTER, NullImplementation.GetConverter());

            // try to lock them all at once
            var listLocked = LockAll(cache, keys, 0);
            if (listLocked == null)
            {
                // the attempt failed; do it one-by-one
                var result = new HashDictionary(entries.Count);
                foreach (IInvocableCacheEntry entry in entries)
                {
                    result[entry.Key] = InvokeLocked(cache, entry, agent);
                }
                return result;
            }
            try
            {
                return agent.ProcessAll(entries);
            }
            finally
            {
                UnlockAll(cache, listLocked);
            }
        }

        #endregion

        #region LockAll/UnlockAll methods

        /// <summary>
        /// Attempt to lock all the specified keys within a specified period
        /// of time.
        /// </summary>
        /// <param name="cache">
        /// The <see cref="IConcurrentCache"/> to use.
        /// </param>
        /// <param name="keys">
        /// A collection of keys to lock.
        /// </param>
        /// <param name="waitMillis">
        /// The number of milliseconds to continue trying to obtain locks;
        /// pass zero to return immediately; pass -1 to block the calling
        /// thread until the lock could be obtained.
        /// </param>
        /// <returns>
        /// An <b>IList</b> containing all the locked keys in the order
        /// opposite to the locking order (LIFO); <c>null</c> if timeout has
        /// occurred.
        /// </returns>
        public static IList LockAll(IConcurrentCache cache, ICollection keys, 
            int waitMillis)
        {
            // remove the duplicates
            HashSet setKeys = keys is HashSet ? (HashSet) keys : new HashSet(keys);

            // copy the keys into a list to fully control the iteration order
            var  listKeys   = new ArrayList(setKeys);
            var  listLocked = new ArrayList();
            int  keysCount  = listKeys.Count;
            bool isSuccess  = true;

            do
            {
                int waitNextMillis = waitMillis; // allow blocking wait for the very first key
                for (int i = 0; i < keysCount; i++)
                {
                    var key = listKeys[i];

                    isSuccess = cache.Lock(key, waitNextMillis);
                    if (isSuccess)
                    {
                        // add the last locked item into the front of the locked
                        // list so it behaves as a stack (FILO strategy)
                        listLocked.Insert(0, key);

                        // to prevent a deadlock don't wait afterwards
                        waitNextMillis = 0;
                    }
                    else
                    {
                        if (i == 0)
                        {
                            // the very first key cannot be locked -- timeout
                            return null;
                        }

                        // unlock all we hold and try again
                        foreach (var o in listLocked)
                        {
                            cache.Unlock(o);
                        }
                        listLocked.Clear();

                        // move the "offending" key to the top of the list
                        // so next iteration we will attempt to lock it first
                        listKeys.RemoveAt(i);
                        listKeys.Insert(0, key);
                    }
                }
            }
            while (!isSuccess);

            return listLocked;
        }

        /// <summary>
        /// Unlock all the specified keys.
        /// </summary>
        /// <param name="cache">
        /// The <see cref="IConcurrentCache"/> to use.
        /// </param>
        /// <param name="keys">
        /// A collection of keys to unlock.
        /// </param>
        public static void UnlockAll(IConcurrentCache cache, ICollection keys)
        {
            foreach (var key in keys)
            {
                cache.Unlock(key);
            }
        }

        #endregion

        #region Entry evaluation

        /// <summary>
        /// Check if the entry passes the filter evaulation.
        /// </summary>
        /// <param name="filter">
        /// The filter to evaluate against.
        /// </param>
        /// <param name="entry">
        /// An <see cref="ICacheEntry"/> to filter.
        /// </param>
        /// <returns>
        /// <b>true</b> if the entry passes the filter, <b>false</b>
        /// otherwise.
        /// </returns>
        public static bool EvaluateEntry(IFilter filter, ICacheEntry entry)
        {
            return filter is IEntryFilter
                ? ((IEntryFilter) filter).EvaluateEntry(entry)
                : filter.Evaluate(entry.Value);
        }

        /// <summary>
        /// Check if an entry, expressed as a key and value, passes the
        /// filter evaulation.
        /// </summary>
        /// <param name="filter">
        /// The filter to evaluate against.
        /// </param>
        /// <param name="key">
        /// The key for the entry.
        /// </param>
        /// <param name="value">
        /// The value for the entry.
        /// </param>
        /// <returns>
        /// <b>true</b> if the entry passes the filter, <b>false</b>
        /// otherwise.
        /// </returns>
        public static bool EvaluateEntry(IFilter filter, object key, object value)
        {
            return filter is IEntryFilter
                    ? ((IEntryFilter) filter).EvaluateEntry(new CacheEntry(key, value))
                    : filter.Evaluate(value);
        }

        /// <summary>
        /// Check if the entry, in its "original" form, passes the filter 
        /// evaulation.
        /// </summary>
        /// <param name="filter">
        /// The filter to evaluate against.
        /// </param>
        /// <param name="entry">
        /// A <see cref="CacheEntry"/> whose "original" value to evaluate.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the entry has an original value and passes the 
        /// filter, <b>false</b> otherwise.
        /// </returns>
        /// <since>Coherence 3.7.1</since>
        public static bool EvaluateOriginalEntry(IFilter filter, CacheEntry entry)
        {
            if (entry.OriginalValue != null)
            {
                object valueOrig = entry.OriginalValue;
                return filter is IEntryFilter
                        ? ((IEntryFilter) filter).EvaluateEntry(
                                new CacheEntry(entry.Key, valueOrig))
                        : filter.Evaluate(valueOrig);
            }

            return false;
        }

        #endregion

        #region Entry extraction

        /// <summary>
        /// Extract a value from the specified entry using the specified
        /// extractor.
        /// </summary>
        /// <param name="extractor">
        /// The extractor to use.
        /// </param>
        /// <param name="entry">
        /// The entry to extract from.
        /// </param>
        /// <returns>
        /// The extracted value.
        /// </returns>
        public static object ExtractFromEntry(IValueExtractor extractor, ICacheEntry entry)
        {
            return extractor is AbstractExtractor
                    ? ((AbstractExtractor) extractor).ExtractFromEntry(entry)
                    : extractor.Extract(entry.Value);
        }

        /// <summary>
        /// Extract a value from the "original value" of the specified entry
        /// using the specified extractor.
        /// </summary>
        /// <param name="extractor">
        /// The extractor to use.
        /// </param>
        /// <param name="entry">
        /// The entry to extract from.
        /// </param>
        /// <returns>
        /// The extracted original value.
        /// </returns>
        /// <since>Coherence 3.7</since>
        public static object ExtractOriginalFromEntry(IValueExtractor extractor, CacheEntry entry)
        {
            return extractor is AbstractExtractor
                    ? ((AbstractExtractor) extractor).ExtractOriginalFromEntry(entry)
                    : extractor.Extract(entry.OriginalValue);
        }

        #endregion

        #region Query methods

        /// <summary>
        /// Generic implementation of the get methods for the particular
        /// <see cref="IFilter"/> provided.
        /// </summary>
        /// <param name="cache">
        /// The <see cref="ICache"/> to be queried.
        /// </param>
        /// <param name="filter">
        /// The <see cref="IFilter"/> object representing the criteria that
        /// the entries of this cache should satisfy.
        /// </param>
        /// <param name="queryType">
        /// An enum value that defines whether return array should be values,
        /// keys or entries.
        /// </param>
        /// <param name="sort">
        /// If <b>true</b>, sort the result-set before returning.
        /// </param>
        /// <param name="comparer">
        /// The <see cref="IComparer"/> to use for sorting (optional).
        /// </param>
        /// <returns>
        /// A collection of the keys/values for entries that satisfy the
        /// specified criteria.
        /// </returns>
        public static object[] Query(ICache cache, IFilter filter, 
            QueryType queryType, bool sort, IComparer comparer)
        {
            return Query(cache, null, filter, queryType, sort, comparer);
        }

        /// <summary>
        /// Generic implementation of the get methods for the particular
        /// <see cref="IFilter"/> provided.
        /// </summary>
        /// <param name="cache">
        /// The <see cref="ICache"/> to be queried.
        /// </param>
        /// <param name="dictIndex">
        /// The <see cref="IDictionary"/> of indexes.
        /// </param>
        /// <param name="filter">
        /// The <see cref="IFilter"/> object representing the criteria that
        /// the entries of this cache should satisfy.
        /// </param>
        /// <param name="queryType">
        /// An enum value that defines whether return array should be values,
        /// keys or entries.
        /// </param>
        /// <param name="sort">
        /// If <b>true</b>, sort the result-set before returning.
        /// </param>
        /// <param name="comparer">
        /// The <see cref="IComparer"/> to use for sorting (optional).
        /// </param>
        /// <returns>
        /// A collection of the keys/values for entries that satisfy the
        /// specified criteria.
        /// </returns>
        public static object[] Query(ICache cache, IDictionary dictIndex, 
            IFilter filter, QueryType queryType, bool sort, IComparer comparer)
        {
            IFilter filterOrig = filter;
            if (AlwaysFilter.Instance.Equals(filter))
            {
                filter = null;
            }

            object[] results; // may contain keys, values, or entries

            // try to apply an index (if available)
            if (dictIndex != null && filter is IIndexAwareFilter)
            {
                // take a thread-safe snapshot of the cache key set; this
                // differs a from the Java version in which the Set interface,
                // SubSet class, and thread-safe Collection.toArray() methods
                // are at our disposal
                ICollection keys;
                CollectionUtils.AcquireReadLock(cache);
                try
                {
                    keys = new HashSet(cache.Keys);
                }
                finally
                {
                    CollectionUtils.ReleaseReadLock(cache);
                }

                filter = ((IIndexAwareFilter) filter).ApplyIndex(dictIndex, keys);
                results = CollectionUtils.ToArray(keys);
            }
            else
            {
                // perform a thread-safe conversion of the cache key set into
                // an object array; again, this differs a bit from the Java
                // version
                CollectionUtils.AcquireReadLock(cache);
                try
                {
                    results = CollectionUtils.ToArray(cache.Keys);
                }
                finally
                {
                    CollectionUtils.ReleaseReadLock(cache);
                }
            }

            int resultCount = 0;
            if (filter == null && queryType == QueryType.Keys)
            {
                resultCount = results.Length;
            }
            else
            {
                // we still have a filter to evaluate or we need an entry set
                // or values collection
                for (int i = 0, c = results.Length; i < c; i++)
                {
                    var key   = results[i];
                    var value = cache[key];

                    if (value != null || cache.Contains(key))
                    {
                        ICacheEntry entry = new CacheEntry(key, value);
                        if (filter == null || EvaluateEntry(filter, entry))
                        {
                            object result;
                            switch (queryType)
                            {
                                case QueryType.Entries:
                                    result = entry;
                                    break;
                                case QueryType.Keys:
                                    result = key;
                                    break;
                                default:
                                    result = value;
                                    break;
                            }
                            results[resultCount++] = result;
                        }
                    }
                }
            }

            // convert the result array into an array of the appropriate 
            // type and length; this differs from the Java version in which
            // this method returns a Set and not an array
            if (queryType == QueryType.Entries)
            {
                var entries = new ICacheEntry[resultCount];
                Array.Copy(results, 0, entries, 0, resultCount);
                results = entries;
            }
            else if (resultCount < results.Length)
            {
                var newResults = new object[resultCount];
                Array.Copy(results, 0, newResults, 0, resultCount);
                results = newResults;
            }

            // sort the results; this differs from the Java version in which
            // this method only can return keys or entries (and not values)
            if (sort)
            {
                if (comparer == null)
                {
                    comparer = SafeComparer.Instance;
                }
                if (queryType == QueryType.Entries)
                {
                    comparer = new EntryComparer(comparer);
                }
                Array.Sort(results, 0, resultCount, comparer);
            }

            // if the original filter is a LimitFilter then we can only return
            // a page at a time
            if (filterOrig is LimitFilter)
            {
                var filterLimit = filterOrig as LimitFilter;
                filterLimit.Comparer = null;

                results     = filterLimit.ExtractPage(results);
                resultCount = results.Length;
                filterLimit.Comparer = comparer; // for debug output only
            }

            // convert the result array into an array of the appropriate 
            // type and length; this differs from the Java version in which
            // this method returns a Set and not an array
            if (queryType == QueryType.Entries)
            {
                var entries = new ICacheEntry[resultCount];
                Array.Copy(results, 0, entries, 0, resultCount);
                results = entries;
            }
            else if (resultCount < results.Length)
            {
                var newResults = new object[resultCount];
                Array.Copy(results, 0, newResults, 0, resultCount);
                results = newResults;
            }

            return results;
        }

        /// <summary>
        /// Add an index to the given dictionary of indexes, keyed by the given 
        /// extractor. Also add the index as a listener to the given cache.
        /// </summary>
        /// <param name="extractor">
        /// The IValueExtractor object that is used to extract an indexable 
        /// property value from a cache entry.
        /// </param>
        /// <param name="ordered"> 
        /// True if the contents of the indexed information should be ordered; 
        /// false otherwise
        /// </param>
        /// <param name="comparator">
        /// The IComparer object which imposes an ordering on entries in the 
        /// indexed cache or <c>null</c> if the entries' values natural 
        /// ordering should be used.
        /// </param>
        /// <param name="cache">
        /// The cache that the newly created ICacheIndex will use for 
        /// initialization and listen to for changes.
        /// </param>
        /// <param name="dictIndex">
        /// The dictionary of indexes that the newly created ICacheIndex will 
        /// be added to.
        /// </param>
        public static void AddIndex(IValueExtractor extractor, bool ordered,
                IComparer comparator, IObservableCache cache, IDictionary dictIndex)
        {
            var index = (ICacheIndex) dictIndex[extractor];

            if (index == null)
            {
                for (int cAttempts = 4; ; )
                {
                    if (extractor is IIndexAwareExtractor)
                    {
                        index = ((IIndexAwareExtractor) extractor).
                                CreateIndex(ordered, comparator, dictIndex);
                        if (index == null)
                        {
                            return;
                        }
                    }
                    else
                    {
                        index = new SimpleCacheIndex(extractor, ordered, comparator);
                        dictIndex[extractor] = index;
                    } 

                    ICacheListener listener = EnsureListener(index);
                    cache.AddCacheListener(listener, null, false);

                    try
                    {
                        // build the index
                        foreach (ICacheEntry entry in cache)
                        {
                            index.Insert(entry);
                        }
                        break;
                    }
                    catch (InvalidOperationException ioe) //collection was modified
                    {
                        cache.RemoveCacheListener(listener);
                        if (--cAttempts == 0)
                        {
                            RemoveIndex(extractor, cache, dictIndex);
                            CacheFactory.Log("Exception occured during index rebuild: " +
                                    ioe, CacheFactory.LogLevel.Error);
                            throw;
                        }
                    }
                }
            }
            else if (!(ordered == index.IsOrdered &&
                    Equals(comparator, index.Comparer)))
            {
                throw new InvalidOperationException("Index for " + extractor +
                        " already exists;" +
                        " remove the index and add it with the new settings");
            }
        }

        /// <summary>
        /// Remove the index keyed by the given extractor from the given 
        /// dictionary of indexes. Also, remove the index as a listener from 
        /// the given cache.
        /// </summary>
        /// <param name="extractor">
        /// The IValueExtractor object that is used to extract an indexable 
        /// object from a value stored in the cache.
        /// </param>
        /// <param name="cache">
        /// The resource map associated with the index.
        /// </param>
        /// <param name="dictIndex">
        /// The dictionary of indexes to remove the ICacheIndex from.
        /// </param>
        public static void RemoveIndex(IValueExtractor extractor, 
            IObservableCache cache, IDictionary dictIndex)
        {
            ICacheIndex index;
            if (extractor is IIndexAwareExtractor)
            {
                index = (extractor as IIndexAwareExtractor).DestroyIndex(dictIndex);
            }
            else
            {
                index = (ICacheIndex) dictIndex[extractor];
                dictIndex.Remove(extractor);
            }

            if (index != null)
            {
                cache.RemoveCacheListener(EnsureListener(index));
            }
        }

        #endregion

        #region helpers

        /// <summary>
        /// Ensure an ICacheListener for the given index. The listener will 
        /// route the cache events into the corresponding ICacheIndex calls.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>A listener for given index.</returns>
        protected static ICacheListener EnsureListener(ICacheIndex index)
        {
            return index is CacheListenerSupport.ISynchronousListener ?
                (ICacheListener) index : new IndexAdapter(index);
        }

        #endregion

        #region constants

        /// <summary>
        /// Trivial Entry-to-Key converter.
        /// </summary>
        public static readonly IConverter ENTRY_TO_KEY_CONVERTER = new EntryToKeyConverter();

        #endregion

        #region Inner class: EntryToKeyConverter

        /// <summary>
        /// Trivial Entry-to-Key <see cref="IConverter"/>.
        /// </summary>
        public class EntryToKeyConverter : IConverter
        {
            #region IConverter Members

            /// <summary>
            /// Convert the passed object to another object.
            /// </summary>
            /// <param name="o">
            /// Object to be converted.
            /// </param>
            /// <returns>
            /// The new, converted object.
            /// </returns>
            public object Convert(object o)
            {
                return ((ICacheEntry) o).Key;
            }

            #endregion
        }

        #endregion

        #region Enum: QueryType

        /// <summary>
        /// The enum type used to pass to the method that will query cache
        /// against one of the <b>QueryType</b> modes.
        /// </summary>
        public enum QueryType
        {
            /// <summary>
            /// The cache will be queried for values.
            /// </summary>
            Values = 0,

            /// <summary>
            /// The cache will be queried for keys.
            /// </summary>
            Keys = 1,

            /// <summary>
            /// The cache will be queried for <see cref="ICacheEntry"/>s.
            /// </summary>
            Entries = 2
        }

        #endregion
    }

    #region Inner class: IndexAdapter

    /// <summary>
    /// ICacheListener implementation that routes the cache events into the
    /// corresponding ICacheIndex calls.
    /// </summary>
    public class IndexAdapter : CacheListenerSupport.ISynchronousListener
    {
        /// <summary>
        /// Construct an IndexAdapter.
        /// </summary>
        /// <param name="index">The ICacheIndex being wrapped.</param>
        public IndexAdapter(ICacheIndex index)
        {
            m_index = index;
        }

        #region MapListener interface

        /// <summary>
        /// Invoked when a cache entry has been inserted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the insert
        /// information.
        /// </param>
        public void EntryInserted(CacheEventArgs evt)
        {
            m_index.Insert(new CacheEntry(evt.Key, evt.NewValue));
        }

        /// <summary>
        /// Invoked when a cache entry has been updated.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the update
        /// information.
        /// </param>
        public void EntryUpdated(CacheEventArgs evt)
        {
            m_index.Update(new CacheEntry(evt.Key, evt.NewValue, evt.OldValue));
        }

        /// <summary>
        /// Invoked when a cache entry has been deleted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the remove
        /// information.
        /// </param>
        public void EntryDeleted(CacheEventArgs evt)
        {
            m_index.Delete(new CacheEntry(evt.Key, null, evt.OldValue));
        }

        #endregion

        #region Object methods

        /// <summary>
        /// Compare this IndexMapListener with another object for equality.
        /// </summary>
        /// <param name="o">An object reference or null.</param>
        /// <returns><c>true</c> iff the passed object reference is a 
        /// IndexMapListener object with the same index.</returns>
        public override bool Equals(Object o)
        {
            return this == o || o is IndexAdapter &&
                    Equals(m_index, ((IndexAdapter) o).m_index);
        }

        /// <summary>
        /// Return a hash code value for the IndexMapListener object.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>IndexAdapter</b>.
        /// </returns>
        public override int GetHashCode()
        {
            return m_index.GetHashCode();
        }

        #endregion

        #region Data members

        /// <summary>
        /// The wrapped index.
        /// </summary>
        private readonly ICacheIndex m_index;

        #endregion
    }

    #endregion
}