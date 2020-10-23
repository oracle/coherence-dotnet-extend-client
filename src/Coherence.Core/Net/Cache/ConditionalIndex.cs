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

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// ConditionalIndex is an <see cref="ICacheIndex"/> implementation that 
    /// uses an associated filter to evaluate whether or not an entry should be 
    /// indexed. An entry's extracted value is only added to the index if the
    /// filter evaluates to true.
    /// </summary>
    /// <author>Tom Beerbower  2010.02.08</author>
    /// <author>Jason Howes  2010.10.01</author>
    public class ConditionalIndex : SimpleCacheIndex
    {
        #region Properties

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
        public override bool IsPartial { get { return m_partial || base.IsPartial; } }

        ///<summary>
        /// Get the associated filter.
        /// </summary>
        ///<returns>The filter.</returns>
        public virtual IFilter Filter { get; private set; }

        ///<summary>
        /// Determine whether or not this ConditionalIndex supports a forward 
        /// index.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this ConditionalIndex supports a forward index; 
        /// <c>false</c> otherwise.
        /// </returns>
        public bool IsForwardIndexSupported { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a ConditionalIndex.
        /// </summary>
        /// <param name="filter">the filter that is used to evaluate the 
        /// entries of the resource cache that is being indexed.
        /// </param>
        /// <param name="extractor">
        /// the <see cref="IValueExtractor"/> that is used to extract an
        /// indexed value from a resource cache entry.
        /// </param>
        /// <param name="ordered">
        /// <c>true</c> iff the contents of the indexed information should be 
        /// ordered; <c>false</c> otherwise.
        /// </param>
        /// <param name="comparer">
        /// the IComparer object which imposes an ordering on entries in the
        /// index; or <tt>null</tt> if the entries' values natural ordering 
        /// should be used.
        /// </param>
        /// <param name="forwardIndex">
        /// specifies whether or not this index supports a forward map
        /// </param>
        public ConditionalIndex(IFilter filter, IValueExtractor extractor,
                bool ordered, IComparer comparer, bool forwardIndex)
            : base(extractor, ordered, comparer, false)
        {
            Filter                  = filter;
            IsForwardIndexSupported = forwardIndex;
            m_partial               = false;

            Initialize(forwardIndex);
        }

        #endregion

        #region SimpleCacheIndex methods

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
        public override object Get(object key)
        {
            if (IsForwardIndexSupported)
            {
                var index = IndexForward;
                var value = index[key];

                return value != null || index.Contains(key) ?
                        value : ObjectUtils.NO_VALUE;
            }
            return ObjectUtils.NO_VALUE;
        }

        /// <summary>
        /// Get the forward index value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value associated with the key.</returns>
        protected override object GetForwardValue(Object key)
        {
            return IsForwardIndexSupported ? base.GetForwardValue(key) 
                : ObjectUtils.NO_VALUE;
        }

        /// <summary>
        /// Remove the forward index entry for the specified key.
        /// </summary>
        ///<param name="key">
        /// The key to remove the forward index entry for.
        ///</param>
        protected override void RemoveForwardEntry(Object key)
        {
            if (IsForwardIndexSupported)
            {
                base.RemoveForwardEntry(key);
            }
        }

        /// <summary>
        /// Instantiate the forward index.
        /// </summary>
        /// <returns>
        /// The forward index.
        /// </returns>
        protected override IDictionary InstantiateForwardIndex()
        {
            return IsForwardIndexSupported ? base.InstantiateForwardIndex() : null;
        }

        /// <summary>
        /// Update this index in response to a insert operation on a cache.
        /// </summary>
        /// <param name="entry">
        /// The entry representing the object being inserted.
        /// </param>        
        protected override void InsertInternal(ICacheEntry entry)
        {
            if (EvaluateEntry(entry))
            {
                base.InsertInternal(entry);
            }
        }

        /// <summary>
        /// Update this index in response to a update operation on a cache.
        /// </summary>
        /// <param name="entry">
        /// The entry representing the object being updated.
        /// </param>
        protected override void UpdateInternal(ICacheEntry entry)
        {
            if (EvaluateEntry(entry))
            {
                base.UpdateInternal(entry);
            }
            else
            {
                DeleteInternal(entry);
            }
        }

        /// <summary>
        /// Update this index in response to a delete operation on a cache.
        /// </summary>
        /// <param name="entry">
        /// The entry representing the object being inserted.
        /// </param>        
        protected override void DeleteInternal(ICacheEntry entry)
        {
            try
            {
                if (entry is CacheEntry &&
                    !InvocableCacheHelper.EvaluateOriginalEntry(Filter, (CacheEntry) entry))
                {
                    // the "original" entry would have been excluded; nothing to do
                    return;
                }
            }
            catch (Exception)
            {
                // COH-6447: attempt the delete anyway because the filter may have
                // allowed this value previously and it may be in the index
            }

            base.DeleteInternal(entry);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Evaluate the given entry using this index's filter. If the entry 
        /// does not pass the filter then it should be excluded from this 
        /// index, making this a partial index.
        /// </summary>
        /// <param name="entry">The entry to evaluate.</param>
        /// <returns>
        /// <c>true</c> If the entry passes the filter, <b>false</b> otherwise.
        /// </returns>
        protected virtual bool EvaluateEntry(ICacheEntry entry)
        {
            try
            {
                if (InvocableCacheHelper.EvaluateEntry(Filter, entry))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // COH-6447: don't drop the index upon exception
            }

            m_partial = true;
            return false;
        }

        #endregion

        #region Object interface

        /// <summary>
        /// Returns string representation of this instance.
        /// </summary>
        /// <returns>
        /// String representation of this instance.
        /// </returns>
        public override string ToString()
        {
            return base.ToString() +
                ", Filter="        + Filter +
                ", ForwardIndex="  + IsForwardIndexSupported;
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
        public override bool Equals(SimpleCacheIndex index)
        {
            if (!base.Equals(index) || !(index is ConditionalIndex))
            {
                return false;
            }

            var that = index as ConditionalIndex;
            return Equals(Filter, that.Filter) &&
                   IsForwardIndexSupported == that.IsForwardIndexSupported;
        }

        #endregion

        #region Data members

        /// <summary>
        /// <b>true</b> if any entry of the indexed ICache has been excluded 
        /// from the index, <b>false</b> otherwise
        /// </summary>
        private bool m_partial;

        #endregion
    }
}