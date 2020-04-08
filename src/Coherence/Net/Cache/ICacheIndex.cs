/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.Collections;

using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// ICacheIndex is used to correlate values stored in an <i>indexed 
    /// ICache</i> (or attributes of those values) to the corresponding keys in 
    /// the indexed <see cref="ICache"/>.
    /// </summary>
    /// <author>Cameron Purdy, Gene Gleyzer  2002.10.31</author>
    /// <author>Jason Howes  2010.09.28</author>
    public interface ICacheIndex
    {
        /// <summary>
        /// Obtain the IValueExtractor object that the ICacheIndex uses to 
        /// extract an indexable Object from a value stored in the indexed 
        /// ICache. This property is never <c>null</c>.
        /// </summary>
        IValueExtractor ValueExtractor
        {
            get;
        }

        /// <summary>
        /// Determine if the ICacheIndex orders the contents of the indexed 
        /// information.
        /// </summary>
        bool IsOrdered
        {
            get;
        }

        /// <summary>
        /// The IComparer used to sort the index.
        /// </summary>
        IComparer Comparer
        {
            get;
        }

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
        bool IsPartial
        { 
            get;
        }

        /// <summary>
        /// Get the IDictionary that contains the <i>index contents</i>.
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
        IDictionary IndexContents
        {
            get;
        }

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
        object Get(object key);

        /// <summary>
        /// Update this index in response to a insert operation on a cache.
        /// </summary>
        /// <param name="entry">
        /// The entry representing the object being inserted.
        /// </param>
        void Insert(ICacheEntry entry);

        /// <summary>
        /// Update this index in response to a update operation on a cache.
        /// </summary>
        /// <param name="entry">
        /// The entry representing the object being updated.
        /// </param>
        void Update(ICacheEntry entry);

        /// <summary>
        /// Update this index in response to a remove operation on a cache.
        /// </summary>
        /// <param name="entry">
        /// The entry representing the object being removed.
        /// </param>        
        void Delete(ICacheEntry entry);
    }
}