/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections;

using Tangosol.Net.Cache.Support;
using Tangosol.Util;
using Tangosol.Util.Collections;
using Tangosol.Util.Comparator;
using Tangosol.Util.Extractor;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// SimpleCacheIndex is an <see cref="ICacheIndex"/> implementation used to 
    /// correlate property values extracted from resource cache entries with 
    /// corresponding keys using what is commonly known as an <i>Inverted Index 
    /// algorithm.</i>.    
    /// </summary>
    /// <author>Tom Beerbower  2009.03.09</author>
    /// <author>Jason Howes  2010.09.28</author>
    public class SimpleCacheIndex : ICacheIndex
    {
        // NOTE: jh 2010.09.29
        // The implementation of SimpleCacheIndex diverges a bit from its
        // Java version. The Java implementation is very careful to reuse the 
        // exact same key object for a given entry when updating the index in 
        // response to an update operation on a cache (see #updateInternal()). 
        // This is done in order to reduce the memory footprint of an index 
        // and is especially effective for binary caches. Unfortunately, the 
        // IDictionary interface does not give us direct access to its 
        // DictionaryEntry objects, so we're going to trade efficiency of 
        // memory utilization here for simiplicity sakes. Since this 
        // optimization is fairly ineffective for objet caches, the impact of
        // doing so is quite minimal.

        #region Properties

        /// <summary>
        /// IValueExtractor object that this ICacheIndex uses to extract an 
        /// indexable Object from a [converted] value stored in the Storage.
        /// This property is never <c>null</c>.
        /// </summary>
        public virtual IValueExtractor ValueExtractor { get; protected set; }

        /// <summary>
        /// Specifies whether or not this ICacheIndex orders the contents of
        /// the indexed information.
        /// </summary>
        public virtual bool IsOrdered { get; protected set; }

        /// <summary>
        /// IComparer used to sort the index. Used iff Ordered is <b>true</b>.
        /// Could be <c>null</c>, which implicates a natural order.
        /// </summary>
        public virtual IComparer Comparer { get; protected set; }

        /// <summary>
        /// Determine if indexed information for any entry in the indexed 
        /// ICache has been excluded from this index. This information is used 
        /// by <see cref="Tangosol.Util.Filter.IIndexAwareFilter"/> 
        /// implementations to determine the most optimal way to apply the 
        /// index.
        /// </summary>
        /// <returns>
        /// <b>true</b> if any entry of the indexed ICache has been excluded 
        /// from the index, <b>false</b> otherwise
        /// </returns>
        // an index is partial if there are (corrupted) entries
        // which are in the cache but not in the index
        public virtual bool IsPartial { get { return !(m_keysExcluded.Count == 0); } }
        /// <summary>
        /// The IDictionary that contains the <i>index contents</i>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The keys of the IDictionary are the return values from the 
        /// IValueExtractor operating against the indexed ICache's values, and 
        /// for each key, the corresponding value stored in the 
        /// IDictionary is an ICollection of keys to the indexed value.</p>
        /// <p>
        /// If the ICacheIndex is known to be ordered, then the returned 
        /// IDictionary object will be an instance of SortedList (or wrapper
        /// thereof). The SortedList may or may not have an IComparer object 
        /// associated with it.</p>
        /// <p>
        /// A client should assume that the returned IDictionary object is 
        /// read-only and must not attempt to modify it.</p>
        /// </remarks>
        public IDictionary IndexContents { get; protected set; }

        /// <summary>
        /// IDictionary that contains the index values (forward index). The 
        /// keys of the IDictionary are the keys to the indexed cache and the 
        /// values are the extracted values. This map is used by 
        /// <see cref="Tangosol.Util.Filter.IIndexAwareFilter"/> implementations 
        /// to avoid conversion and value extraction steps.
        /// </summary>
        protected IDictionary IndexForward { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct an index.
        /// </summary>
        /// <param name="extractor">
        /// The IValueExtractor that is used to extract an indexed value from a 
        /// resource cache entry.
        /// </param>
        /// <param name="ordered">
        /// <b>true</b> iff the contents of the indexed information should be 
        /// ordered; <b>false</b> otherwise.
        /// </param>
        /// <param name="comparer">
        /// The IComparator object which imposes an ordering on entries in the 
        /// index map; or <tt>null</tt> if the entries' values natural ordering 
        /// should be used.
        /// </param>
        public SimpleCacheIndex(IValueExtractor extractor,
                         bool ordered,
                         IComparer comparer)
            : this(extractor, ordered, comparer, true)
        { }

        /// <summary>
        /// Construct an index.
        /// </summary>
        /// <param name="extractor">
        /// The IValueExtractor that is used to extract an indexed value from a 
        /// resource cache entry.
        /// </param>
        /// <param name="ordered">
        /// <b>true</b> iff the contents of the indexed information should be 
        /// ordered; <b>false</b> otherwise.
        /// </param>
        /// <param name="comparer">
        /// The IComparator object which imposes an ordering on entries in the 
        /// index map; or <tt>null</tt> if the entries' values natural ordering 
        /// should be used.
        /// </param>
        /// <param name="init">
        /// Initialize the index if <b>true</b>.
        /// </param>
        public SimpleCacheIndex(IValueExtractor extractor,
                         bool ordered,
                         IComparer comparer,
                         bool init)
        {
            ValueExtractor     = extractor;
            IsOrdered          = ordered;
            Comparer           = comparer;
            m_splitCollection  = !(extractor is MultiExtractor);
            m_fImmutableValues = extractor is KeyExtractor ||
                    extractor is AbstractExtractor &&
                    ((AbstractExtractor) extractor).Target == AbstractExtractor.KEY;

            if (init)
            {
                Initialize(true);
            }
        }

        #endregion

        #region ICacheIndex implementation

        /// <summary>
        /// Using the index information if possible, get the value associated
        /// with the specified key. This is expected to be more efficient than 
        /// using the IValueExtractor against an object containing the value, 
        /// because the index should already have the necessary information at 
        /// hand.
        /// </summary>
        /// <param name="key">
        /// The key that specifies the object to extract the value from.
        /// </param>
        /// <returns>
        /// The value that would be extracted by this ICacheIndex's 
        /// IValueExtractor from the object specified by the passed key;
        /// <see cref="ObjectUtils.NO_VALUE"/> if the index does not have the
        /// necessary information.
        /// </returns>
        public virtual object Get(object key)
        {
            return IndexForward == null ? ObjectUtils.NO_VALUE : IndexForward[key];
        }

        /// <summary>
        /// Update this index in response to a insert operation on a cache.
        /// </summary>
        /// <param name="entry">
        /// The entry representing the object being inserted.
        /// </param>    
        public virtual void Insert(ICacheEntry entry)
        {
            InsertInternal(entry);
        }

        /// <summary>
        /// Update this index in response to a update operation on a cache.
        /// </summary>
        /// <param name="entry">
        /// The entry representing the object being updated.
        /// </param>
        public virtual void Update(ICacheEntry entry)
        {
            if (!m_fImmutableValues)
            {
                UpdateInternal(entry);
            }
        }

        /// <summary>
        /// Update this index in response to a remove operation on a cache.
        /// </summary>
        /// <param name="entry">
        /// The entry representing the object being removed.
        /// </param>        
        public virtual void Delete(ICacheEntry entry)
        {
            DeleteInternal(entry);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Initialize the index's data structures.
        /// </summary>
        /// <param name="forwardIndex">
        /// <b>true</b> If forward index is supported; <b>false</b> otherwise.
        /// </param>
        protected virtual void Initialize(bool forwardIndex)
        {
            IndexContents  = InstatiateInverseIndex(IsOrdered, Comparer);
            IndexForward   = forwardIndex ? InstantiateForwardIndex() : null;
            m_keysExcluded = new SafeHashSet();
        }

        /// <summary>
        /// Get the forward index value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value associated with the key.</returns>
        protected virtual object GetForwardValue(Object key)
        {
            var index = IndexForward;
            return index == null ? ObjectUtils.NO_VALUE 
                : index.Contains(key) ? index[key] : ObjectUtils.NO_VALUE;
        }

        /// <summary>
        /// Remove the forward index entry for the specified key.
        /// </summary>
        ///<param name="key">
        /// The key to remove the forward index entry for.
        ///</param>
        protected virtual void RemoveForwardEntry(Object key)
        {
            if (IndexForward != null)
            {
                IndexForward.Remove(key);
            }
        }

        /// <summary>
        /// Extract the "new" value from the specified entry.
        /// </summary>
        /// <param name="entry">
        /// The entry to extract the "new" value from.
        /// </param>
        /// <returns>
        /// The extracted "new" value, or NO_VALUE if the extraction failed
        /// </returns>
        protected Object ExtractNewValue(ICacheEntry entry)
        {
            try
            {
                return InvocableCacheHelper.ExtractFromEntry(ValueExtractor, entry);
            }
            catch (Exception e)
            {
                CacheFactory.Log("An Exception occurred during index update for key " + entry.Key
                               + ". The entry will be excluded from the index.\n",
                                 CacheFactory.LogLevel.Warn);
                CacheFactory.Log(e + ":\n" + e.StackTrace, CacheFactory.LogLevel.Warn);

                return NO_VALUE;
            }
        }

        /// <summary>
        /// Extract the "old" value from the specified entry.
        /// </summary>
        /// <param name="entry">
        /// The entry to extract the "old" value from.
        /// </param>
        /// <returns>
        /// The extracted "old" value, or NO_VALUE if the extraction failed.
        /// </returns>
        protected Object ExtractOldValue(CacheEntry entry)
        {
            try
            {
                return InvocableCacheHelper.ExtractOriginalFromEntry(ValueExtractor, entry);
            }
            catch (Exception)
            {
                return NO_VALUE;
            }
        }

        /// <summary>
        /// Return a Collection representation of the specified value, which could be
        /// a Collection, Object[], scalar, or NO_VALUE.
        /// </summary>
        /// <param name="value"> 
        /// The value.
        /// </param>
        /// <returns>
        /// A Collection representation of the specified value, or an empty
        /// Collection if NO_VALUE.
        /// </returns>
        protected static ICollection EnsureCollection(Object value)
        {
            if (value == NO_VALUE)
            {
                return new ArrayList();
            }
            if (value is ICollection)
            {
                return (ICollection) value;
            }
            if (value is Object[])
            {
                return new ImmutableArrayList((Object[]) value);
            }

            ArrayList list = new ArrayList();
            list.Add(value);
            return list;
        }

        /// <summary>
        /// Instantiate the forward index.
        /// </summary>
        /// <returns>
        /// The forward index.
        /// </returns>
        protected virtual IDictionary InstantiateForwardIndex()
        {
            return new SynchronizedDictionary();
        }

        /// <summary>
        /// Instantiate the inverse index.
        /// </summary>
        /// <param name="ordered">
        /// <b>true</b> iff the contents of the indexed information should be 
        /// ordered; false otherwise.
        /// </param>
        /// <param name="comparer">
        /// The IComparator object which imposes an ordering on entries in the 
        /// index; or <tt>null</tt> if the entries' values natural ordering 
        /// should be used.
        /// </param>
        /// <returns>The inverse index.</returns>
        protected virtual IDictionary InstatiateInverseIndex(bool ordered, IComparer comparer)
        {
            if (ordered)
            {
                if (!(comparer is SafeComparer))
                {
                    comparer = new SafeComparer(comparer);
                }
                return new SynchronizedDictionary(new SortedDictionary(comparer));
            }
            return new SynchronizedDictionary();
        }

        /// <summary>
        /// Update this index in response to a insert operation on a cache.
        /// </summary>
        /// <param name="entry">
        /// The entry representing the object being inserted.
        /// </param>        
        protected virtual void InsertInternal(ICacheEntry entry)
        {
            var key   = entry.Key;
            var value = ExtractNewValue(entry);
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                if (value == NO_VALUE)
                {
                    // COH-6447: exclude corrupted entries from index and keep track of them
                    UpdateExcludedKeys(entry, true);
                }
                else
                {
                    // add a new mapping(s) to the inverse index
                    AddInverseMapping(value, key);

                    // add a new mapping to the forward index
                    var index = IndexForward;
                    if (index != null)
                    {
                        index[key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Update this index in response to a update operation on a cache.
        /// </summary>
        /// <param name="entry">
        /// The entry representing the object being updated.
        /// </param>
        protected virtual void UpdateInternal(ICacheEntry entry)
        {
            var key      = entry.Key;
            var valueNew = ExtractNewValue(entry);

            using (BlockingLock l = BlockingLock.Lock(this))
            {
                var valueOld = GetForwardValue(key);
                if (ObjectUtils.NO_VALUE == valueOld)
                {
                    if (entry is CacheEntry)
                    {
                        valueOld = ExtractOldValue((CacheEntry) entry);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Cannot extract the old value");
                    }
                }

                // replace the mapping(s) in the inverse index (if necessary)
                if (!Equals(valueOld, valueNew))
                {
                    // remove the old mapping(s) from the inverse index
                    if (valueOld == NO_VALUE)
                    {
                        // extraction of old value failed; must do a full-scan, ensuring that
                        // any mappings to collection element values in the "new" value will remain
                        // (COH-7206)
                        RemoveInverseMapping(NO_VALUE, key, EnsureCollection(valueNew));
                    }
                    else if (m_splitCollection &&
                        valueOld is ICollection || valueOld.GetType().IsArray)
                    {
                        // Note: it's important to only remove the elements that are no longer 
                        //       present in the new value (see COH-7206)
                        RemoveInverseMapping(CollectRemoved(valueOld, valueNew), key);
                    }
                    else
                    {
                        RemoveInverseMapping(valueOld, key);
                    }

                    if (valueNew == NO_VALUE)
                    {
                        // COH-6447: exclude corrupted entries from index and keep track of them
                        if (GetForwardValue(key) != null)
                        {
                            RemoveForwardEntry(key);
                        }

                        UpdateExcludedKeys(entry, true);
                    }
                    else
                    {
                        // add a new mapping(s) to the inverse index
                        AddInverseMapping(valueNew, key);

                        // replace the mapping in the forward index
                        IDictionary indexForward = IndexForward;
                        if (indexForward != null)
                        {
                            indexForward[key] = valueNew;
                        }

                        // entry was successfully updated, ensure that the key is not excluded
                        UpdateExcludedKeys(entry, false);
                    }
                }
            }
        }

        /// <summary>
        /// Update this index in response to a remove operation on a cache.
        /// </summary>
        /// <param name="entry">
        /// The entry representing the object being removed.
        /// </param>
        protected virtual void DeleteInternal(ICacheEntry entry)
        {
            var key = entry.Key;
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                object valueOld;

                var index = IndexForward;
                if (index == null)
                {
                    if (entry is CacheEntry)
                    {
                        valueOld = ExtractOldValue((CacheEntry) entry);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Cannot extract the old value");
                    }
                }
                else
                {
                    // remove the mapping from the forward index
                    valueOld = index[key];
                    index.Remove(key);
                }

                // remove the mapping(s) from the inverse index
                RemoveInverseMapping(valueOld, key);
                // make sure the entry is no longer tracked as excluded
                UpdateExcludedKeys(entry, false);
            }
        }

        /// <summary>
        /// Add a new mapping from the given indexed value to the given key in
        /// the inverse index.
        /// </summary>
        /// <param name="value">The index value (serves as a key in the inverse
        /// index).</param>
        /// <param name="key">the key to insert into the inverse index</param>
        protected virtual void AddInverseMapping(object value, object key)
        {
            var index = IndexContents;

            // add a new mapping(s) to the inverse index
            if (value is object[] || m_splitCollection && value is ICollection)
            {
                AddInverseCollectionMapping(index, (ICollection) value, key);
            }
            else
            {
                AddInverseMapping(index, value, key);
            }
        }

        /// <summary>
        /// Add a new mapping from the given indexed value to the given key in
        /// the supplied index.
        /// </summary>
        /// <param name="index">the index to which to add the mapping</param>
        /// <param name="value">the indexed value</param>
        /// <param name="key">the key to insert into the inverse index</param>
        protected virtual void AddInverseMapping(IDictionary index, 
            object value, object key)
        {
            var keys = (HashSet) index[value];
            if (keys == null)
            {
                keys = new HashSet();
                index[value] = keys;
            }
            keys.Add(key);
        }

        /// <summary>
        /// Add new mappings from the elements of the given value to the given 
        /// key in the supplied index.
        /// </summary>
        /// <param name="index">the index to which to add the mapping</param>
        /// <param name="value">the indexed ICollection value (each element 
        /// serves as a key in the inverse index)</param>
        /// <param name="key">the key to insert into the inverse index</param>
        protected virtual void AddInverseCollectionMapping(IDictionary index,
            ICollection value, object key)
        {
            foreach (var o in value)
            {
                AddInverseMapping(index, o, key);
            }
        }

        /// <summary>
        /// Remove the mapping from the given indexed value to the given key 
        /// from the inverse index.
        /// </summary>
        /// <param name="value">The indexed value.</param>
        /// <param name="key">The key.</param>
        /// <param name="colIgnore">
        /// The Collection of values to ignore (exclude from removal), or null.
        /// </param>
        protected virtual void RemoveInverseMapping(object value, object key, ICollection colIgnore)
        {
            var index = IndexContents;
            if (value == NO_VALUE && !IsKeyExcluded(key))
            {
                // the old value could not be obtained; must resort to a full-scan
                foreach (var o in index.Keys)
                {
                    RemoveInverseMapping(o, key, colIgnore);
                }
            }
            else if (value is object[] || m_splitCollection && value is ICollection)
            {
                foreach (var o in (ICollection) value)
                {
                    RemoveInverseMapping(index, o, key);
                }
            }
            else
            {
                RemoveInverseMapping(index, value, key);
            }
        }

        /// <summary>
        /// Remove the mapping from the given indexed value to the given key 
        /// from the inverse index.
        /// </summary>
        /// <param name="value">The indexed value, or NO_VALUE if unknown.</param>
        /// <param name="key">The key</param>
        protected virtual void RemoveInverseMapping(object value, object key)
        {
            RemoveInverseMapping(value, key, null);
        }

        /// <summary>
        /// Remove the mapping from the given indexed value to the given key 
        /// from the supplied index.
        /// </summary>
        /// <param name="index">the index from which to remove the 
        /// mapping</param>
        /// <param name="value">the indexed value</param>
        /// <param name="key">the key</param>
        protected virtual void RemoveInverseMapping(IDictionary index, 
            object value, object key)
        {
            var keys = (HashSet) index[value];
            if (keys == null)
            {
                if (!IsPartial)
                {
                    LogMissingIdx(value, key);
                }
            }
            else
            {
                keys.Remove(key);
                if (keys.Count == 0)
                {
                    index.Remove(value);
                }
            }
        }

        /// <summary>
        /// Given that the old value is known to be a Collection or an array,
        /// collect all the enclosed elements that are not part of the new value.
        /// </summary> 
        /// <param name="valueOld">The old value, must be a collection or an array.</param>
        /// <param name="valueNew">The new value.</param>
        /// <returns>The set of values that are contained in the old collection
        /// or array, but not part of the new value.
        /// </returns>
        protected HashSet CollectRemoved(Object valueOld, Object valueNew)
        {
            HashSet setRemove;
            if (valueOld is ICollection)
            {
                // clone the original collection
                setRemove = new HashSet((ICollection) valueOld);
            }
            else // oIxValueOld instanceof Object[]
            {
                setRemove = new HashSet((IList) valueOld);
            }

            foreach (var o in EnsureCollection(valueNew))
            {
                if (setRemove.Contains(o))
                {
                    setRemove.Remove(o);
                }
            }

            return setRemove;
        }

        /// <summary>
        /// Log messages for missing inverse index. 
        /// </summary>
        /// <param name="value">the indexed value</param>
        /// <param name="key">the key</param>
        protected void LogMissingIdx(Object value, Object key)
        {
            // COH-5939: limit logging frequency to 10 messages for every 5 minutes interval
            long ldtNow       = DateTimeUtils.GetSafeTimeMillis();
            long ldtLogResume = m_ldtLogMissingIdx + 300000L;

            if (ldtNow > ldtLogResume)
            {
                m_ldtLogMissingIdx = ldtNow;
                m_cLogMissingIdx   = 0;
            }

            int cLog = ++m_cLogMissingIdx;
            if (cLog < 10)
            {
                CacheFactory.Log("Missing inverse index: value=" +
                    value + ", key=" + key, CacheFactory.LogLevel.Always); 
            }
            else if (cLog == 10)
            {
                CacheFactory.Log("Suppressing missing inverse index messages for " + 
                    (ldtLogResume - ldtNow) / 1000 + " seconds", CacheFactory.LogLevel.Always);
            }
        }

        // NOTE: jh 2010.10.01
        // When we move to .NET 4.0 we'll have a proper ISet interface. Until
        // then, this factory method doesn't really make much sense.
        /*
        protected virtual ISet instantiateSet()
        {
            return new HashSet();
        }
        */

        /// <summary>
        /// Check the entry against the set of entries not included in the
        /// index and update the set if necessary.
        /// </summary>
        /// <param name="entry">
        /// The entry to be checked.
        /// </param>
        /// <param name="excluded">
        /// True if the insert or update of the entry into the index caused
        /// an exception.
        /// </param>
        protected void UpdateExcludedKeys(ICacheEntry entry, bool excluded)
        {
            SafeHashSet setExcluded = m_keysExcluded;
            if (excluded || !(setExcluded.Count == 0))
            {
                Object key = entry.Key;
                if (excluded)
                {
                    setExcluded.Add(key);
                }
                else
                {
                    setExcluded.Remove(key);
                }
            }
        }

        /// <summary>
        /// Check if the entry with the given key is excluded from the index.
        /// </summary>
        /// <param name="key"> The key to test </param>
        /// <returns>
        /// True if the key is in the list of keys currently excluded from
        ///  the index, false if the entry with the key is in the index.
        /// </returns>
        protected bool IsKeyExcluded(Object key)
        {
            return m_keysExcluded.Contains(key);
        }

        #endregion

        #region Object method overrides

        /// <summary>
        /// Determines whether the specified object is equal to this object.
        /// </summary>
        /// <returns>
        /// true if the specified object is equal to this object; 
        /// otherwise, false.
        /// </returns>
        /// <param name="obj">
        /// The object to compare with this object. 
        /// </param>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is SimpleCacheIndex)
            {
                return Equals((SimpleCacheIndex) obj);
            }
            return false;
        }

        /// <summary>
        /// Compares this index with another index for equality.
        /// </summary>
        /// <remarks>
        /// This method returns true if this index and the specified index have 
        /// exactly the same contents.
        /// </remarks>
        /// <param name="index">
        /// index to compare this index with.
        /// </param>
        /// <returns>
        /// <c>true</c> if the two indexes are equal; <c>false</c> otherwise.
        /// </returns>
        public virtual bool Equals(SimpleCacheIndex index)
        {
            return Equals(Comparer, index.Comparer)             &&
                   Equals(ValueExtractor, index.ValueExtractor) &&
                   IsOrdered == index.IsOrdered;
        }

        /// <summary>
        /// Returns a hash code for this object. 
        /// </summary>
        /// <returns>
        /// A hash code for this object.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var comparer  = Comparer;
                var extractor = ValueExtractor;
                return comparer == null ? 0 : comparer.GetHashCode() +
                       extractor.GetHashCode() + (IsOrdered ? 1 : 0);
            }
        }

        /// <summary>
        /// Returns string representation of this instance.
        /// </summary>
        /// <returns>
        /// String representation of this instance.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name +
                   ": Extractor=" + ValueExtractor +
                   ", Ordered=" + IsOrdered +
                   ", Content=" + IndexContents.Keys;
        }

        #endregion

        #region Constants and Data Members

        /// <summary>
        /// Marker object used to represent extractor failure.
        /// </summary>
        protected static readonly Object NO_VALUE = new Object();

        /// <summary>
        /// If a value extracted by the IValueExtractor is an ICollection, this
        /// property specifies whether or not it should be treated as a 
        /// collection of contained attributes or indexed as a single composite
        /// attribute.
        /// </summary>
        protected bool m_splitCollection;

        /// <summary>
        /// The time at which the most recent logging of "missing inverse index"
        /// messages started.
        /// </summary>
        protected long m_ldtLogMissingIdx;

        /// <summary>
        /// The number of "missing inverse index" messages that have been logged.
        /// </summary>
        protected int m_cLogMissingIdx;

        /// <summary>
        /// A set of keys for the entries, which could not be included in the index.
        /// </summary>
        protected SafeHashSet m_keysExcluded;

        /// <summary>
        /// Specifies whether or not the index is based on the immutable values (e.g. keys).
        /// </summary>
        /// <since>12.2.1.</since>
        protected bool m_fImmutableValues;

        #endregion
    }
}
