/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.Net.Cache;
using Tangosol.Util;
using Tangosol.Util.Comparator;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// Abstract base for <see cref="IValueExtractor"/> implementations.
    /// </summary>
    /// <remarks>
    /// It provides common functionality that allows any extending extractor
    /// to be used as a value Comparer.
    /// <p>
    /// Starting with Coherence 3.5, when used to extract information that is
    /// coming from a <see cref="ICache"/>, subclasses have the additional
    /// ability to operate against the <see cref="ICacheEntry "/> instead of
    /// just the value. In other words, like the <see cref="EntryExtractor"/>
    /// class, this allows an extractor implementation to extract a desired
    /// value using all available information on the corresponding
    /// <b>ICacheEntry</b> object and is intended to be used in advanced
    /// custom scenarios, when application code needs to look at both key and
    /// value at the same time or can make some very specific assumptions
    /// regarding to the implementation details of the underlying Entry
    /// object. To maintain full backwards compatibility, the default behavior
    /// remains to extract from the Value property of the <b>ICacheEntry</b>.
    /// </p>
    /// <p>
    /// <b>Note:</b> Subclasses are responsible for initialization and POF and/or
    /// Lite serialization of the <see cref="m_target"/> field.
    /// </p>
    /// </remarks>
    /// <author>Gene Gleyzer  2003.09.22</author>
    /// <author>Ana Cikic  2006.09.12</author>
    public abstract class AbstractExtractor : IValueExtractor, IQueryCacheComparer
    {

        #region Properties

        /// <summary>
        /// The target of the extractor.
        /// </summary>
        /// <since>12.2.1</since>
        public int Target
        {
            get
            {
                return m_target;
            }
        }

        #endregion Properties

        /// <summary>
        /// Extract the value from the passed object.
        /// </summary>
        /// <remarks>
        /// The returned value may be <c>null</c>.
        /// </remarks>
        /// <param name="obj">
        /// An object to retrieve the value from.
        /// </param>
        /// <returns>
        /// The extracted value as an object; <c>null</c> is an acceptable
        /// value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// If this IValueExtractor is incompatible with the passed object to
        /// extract a value from and the implementation <b>requires</b> the
        /// passed object to be of a certain type.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this AbstractExtractor cannot handle the passed object for any
        /// other reason; an implementor should include a descriptive
        /// message.
        /// </exception>
        /// <seealso cref="IValueExtractor.Extract"/>
        public virtual object Extract(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Compares its two arguments for order.
        /// </summary>
        /// <remarks>
        /// Returns a negative integer, zero, or a positive integer as the
        /// first argument is less than, equal to, or greater than the
        /// second. <c>null</c> values are evaluated as "less then" any
        /// non-null value.
        /// </remarks>
        /// <param name="o1">
        /// The first object to be compared.
        /// </param>
        /// <param name="o2">
        /// The second object to be compared.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as the first
        /// argument is less than, equal to, or greater than the second.
        /// </returns>
        public virtual int Compare(object o1, object o2)
        {
            return SafeComparer.CompareSafe(null, Extract(o1), Extract(o2));
        }

        /// <summary>
        /// Compare two entries.
        /// </summary>
        /// <param name="entry1">
        /// The first entry to compare values from; read-only.
        /// </param>
        /// <param name="entry2">
        /// The second entry to compare values from; read-only.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as the first
        /// entry denotes a value that is is less than, equal to, or greater
        /// than the value denoted by the second entry
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// If the arguments' types prevent them from being compared by this
        /// <b>IComparer</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the extractor cannot handle the passed objects for any other
        /// reason; an implementor should include a descriptive message.
        /// </exception>
        public virtual int CompareEntries(IQueryCacheEntry entry1, IQueryCacheEntry entry2)
        {
            return SafeComparer.CompareSafe(null, entry1.Extract(this), entry2.Extract(this));
        }

        /// <summary>
        /// Extract the value from the passed Entry object. The returned 
        /// value should follow the conventions outlined in the 
        /// <see cref="Extract"/> method.
        /// </summary>
        /// <remarks>
        /// By overriding this method, an extractor implementation is able
        /// to extract a desired value using all available information on the
        /// corresponding ICacheEntry object and is intended to be used in 
        /// advanced custom scenarios, when application code needs to look at
        /// both key and value at the same time or can make some very 
        /// specific assumptions regarding to the implementation details of
        /// the underlying Entry object.
        /// </remarks>
        /// <param name="entry">An Entry object to extract a desired value from</param>
        /// <returns>The extracted value</returns>
        /// <since>Coherence 3.5</since>
        public virtual object ExtractFromEntry(ICacheEntry entry)
        {
            return Extract(m_target == VALUE ? entry.Value : entry.Key);
        }

        /// <summary>
        /// Extract the value from the "original value" of the passed 
        /// CacheEntry object or the key (if targeted). This method's conventions are exactly the same
        /// as the <see cref="ExtractFromEntry"/> method.
        /// </summary>
        /// <param name="entry">
        /// A CacheEntry object whose original value should be used to 
        /// extract the desired value from.
        /// </param>
        /// <returns>
        /// The extracted value or null if the original value is not present.
        /// </returns>
        /// <since>Coherence 3.7</since>
        public virtual object ExtractOriginalFromEntry(CacheEntry entry)
        {
            return m_target == KEY
                ? Extract((Object) entry.Key)
                : Extract(entry.OriginalValue);
        }

        /// <summary>
        /// Indicates that the <see cref="ExtractFromEntry"/> operation
        /// should use the Entry's value.
        /// </summary>
        /// <since>Coherence 3.5</since>
        public static readonly int VALUE = 0;

        /// <summary>
        /// Indicates that the <see cref="ExtractFromEntry"/> operation
        /// should use the Entry's value.
        /// </summary>
        /// <since>Coherence 3.5</since>
        public static readonly int KEY = 1;

        
        /// <summary>
        /// Specifies which part of the entry should be used by the
        /// <see cref="ExtractFromEntry"/> operation. Legal values are
        /// <see cref="VALUE" /> (default) or <see cref="KEY"/>.
        /// </summary>
        /// <remarks>
        /// Subclasses are responsible for initialization and POF and/or
        /// Lite serialization of this field.
        /// </remarks>
        /// <since>Coherence 3.5</since>
        protected int m_target;
    }
}