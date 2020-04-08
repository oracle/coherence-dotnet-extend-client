/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using System.Text;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which truncates the results of another filter.
    /// </summary>
    /// <remarks>
    /// This filter is a mutable object that is modified by the query
    /// processor. Clients are supposed to hold a reference to this filter
    /// and repetitively pass it to query methods after setting a desired
    /// page context.
    /// </remarks>
    /// <author>Gene Gleyzer  2002.12.06</author>
    /// <author>Goran Milosavljevic  2006.10.24</author>
    /// <author>Tom Beerbower  2009.03.09</author>
    public class LimitFilter : IIndexAwareFilter, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="IFilter"/> whose results are truncated by
        /// this filter.
        /// </summary>
        /// <value>
        /// The filter whose results are truncated by this filter.
        /// </value>
        public virtual IFilter Filter
        {
            get { return m_filter; }
        }

        /// <summary>
        /// Gets or sets the page size (expressed as a number of entries per
        /// page).
        /// </summary>
        /// <value>
        /// The page size.
        /// </value>
        public virtual int PageSize
        {
            get { return m_pageSize; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Invalid page size");
                }
                m_pageSize = value;
            }

        }

        /// <summary>
        /// Gets or sets current page number (zero-based).
        /// </summary>
        /// <value>
        /// The current page number.
        /// </value>
        public virtual int Page
        {
            get { return m_page; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Negative page: " + value);
                }

                if (value == 0) // "reset"
                {
                    TopAnchor    = null;
                    BottomAnchor = null;
                    Cookie       = null;
                }
                else
                {
                    int pageCurr = m_page;
                    if (value == pageCurr + 1)
                    {
                        TopAnchor    = BottomAnchor;
                        BottomAnchor = null;
                    }
                    else if (value == pageCurr - 1)
                    {
                        BottomAnchor = TopAnchor;
                        TopAnchor    = null;
                    }
                    else if (value != pageCurr)
                    {
                        TopAnchor    = null;
                        BottomAnchor = null;
                    }
                }
                m_page = value;
            }
        }

        /// <summary>
        /// Gets or sets the <b>IComparer</b> used to partition the entry
        /// values into pages.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used only by query processors.
        /// Clients should not modify the content of this property.
        /// </remarks>
        /// <value>
        /// The <b>IComparer</b> object.
        /// </value>
        public virtual IComparer Comparer
        {
            get { return m_comparer; }
            set { m_comparer = value; }
        }

        /// <summary>
        /// Gets or sets the top anchor object, which is the last value
        /// object on a previous page.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used only by query processors.
        /// Clients should not modify the content of this property.
        /// </remarks>
        /// <value>
        /// Top anchor object.
        /// </value>
        public virtual object TopAnchor
        {
            get { return m_anchorTop; }
            set { m_anchorTop = value; }
        }

        /// <summary>
        /// Gets or sets the bottom anchor object, which is the last value
        /// object on the current page.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used only by query processors.
        /// Clients should not modify the content of this property.
        /// </remarks>
        /// <value>
        /// Bottom anchor object.
        /// </value>
        public virtual object BottomAnchor
        {
            get { return m_anchorBottom; }
            set { m_anchorBottom = value; }
        }

        /// <summary>
        /// Gets or sets the cookie object.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used only by query processors.
        /// Clients should not modify the content of this property.
        /// </remarks>
        /// <value>
        /// Cookie object.
        /// </value>
        public virtual object Cookie
        {
            get { return m_cookie; }
            set { m_cookie = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LimitFilter()
        {}

        /// <summary>
        /// Construct a limit filter.
        /// </summary>
        /// <param name="filter">
        /// The filter whose results this filter truncates.
        /// </param>
        /// <param name="pageSize">
        /// Page size.
        /// </param>
        public LimitFilter(IFilter filter, int pageSize)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            if (filter is LimitFilter)
            {
                throw new NotSupportedException("Limit of limit");
            }

            m_filter = filter;

            PageSize = pageSize;
        }

        #endregion

        #region IFilter implementation

        /// <summary>
        /// Apply the test to the object.
        /// </summary>
        /// <param name="o">
        /// An object to which the test is applied.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        public virtual bool Evaluate(object o)
        {
            return m_filter.Evaluate(o);
        }

        #endregion

        #region IEntryFilter implementation

        /// <summary>
        /// Apply the test to an <see cref="ICacheEntry"/>.
        /// </summary>
        /// <param name="entry">
        /// The <b>ICacheEntry</b> to evaluate; never <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        public virtual bool EvaluateEntry(ICacheEntry entry)
        {
            return InvocableCacheHelper.EvaluateEntry(m_filter, entry);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Return a human-readable description for this
        /// <b>LimitFilter</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>LimitFilter</b>.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder("LimitFilter: (");
            sb.Append(m_filter)
              .Append(" [pageSize=")
              .Append(m_pageSize)
              .Append(", pageNum=")
              .Append(m_page);

            if (m_comparer != null)
            {
                sb.Append(", top=")
                  .Append(m_anchorTop)
                  .Append(", bottom=")
                  .Append(m_anchorBottom)
                  .Append(", comparer=")
                  .Append(m_comparer);
            }

            sb.Append("])");
            return sb.ToString();
        }

        #endregion

        #region LimitFilter methods

        /// <summary>
        /// Switch to the next page.
        /// </summary>
        public virtual void NextPage()
        {
            Page = Page + 1;
        }

        /// <summary>
        /// Switch to the previous page.
        /// </summary>
        public virtual void PreviousPage()
        {
            Page = Page - 1;
        }

        /// <summary>
        /// Extract a subset of the specified array to fit the filter's
        /// parameters (i.e. page size and page number).
        /// </summary>
        /// <remarks>
        /// If this filter has a comparator, the specified array is presumed
        /// to be sorted accordingly.
        /// <p />
        /// The returned array is guaranteed to iterate exactly in the same
        /// order as the original array.
        /// </remarks>
        /// <param name="entries">
        /// An original array of entries.
        /// </param>
        /// <returns>
        /// An array of entries extracted accordingly to the filter
        /// parameters.
        /// </returns>
        public virtual object[] ExtractPage(object[] entries)
        {
            int       entriesCount = entries.Length;
            int       pageSize     = PageSize;
            IComparer comparer     = Comparer;

            // no reason to optimize for a small result set
            if (comparer != null && entriesCount > pageSize)
            {
                object anchorTop    = TopAnchor;
                object anchorBottom = BottomAnchor;

                if (anchorTop != null)
                {
                    // if both AnchorTop and AnchorBottom are present;
                    // it's a repetitive request for the same page

                    int offAnchor = Array.BinarySearch(entries,
                        entries.GetLowerBound(0), entries.Length, 
                        new CacheEntry(null, anchorTop), comparer);
                    int shift   = anchorBottom == null ? 1 : 0;
                    int ofFirst = offAnchor >= 0 ? offAnchor + shift : -offAnchor - 1;
                    if (ofFirst < entriesCount)
                    {
                        return ExtractPage(new SimpleEnumerator(entries, 
                            ofFirst, Math.Min(pageSize, entriesCount - ofFirst)));
                    }
                    return new object[0];
                }
                if (anchorBottom != null)
                {
                    int ofAnchor = Array.BinarySearch(entries,
                        entries.GetLowerBound(0), entries.Length,
                        new CacheEntry(null, anchorBottom), comparer);
                    int ofAfterLast = ofAnchor >= 0 ? ofAnchor : -ofAnchor - 1;

                    if (ofAfterLast > 0)
                    {
                        int offFirst = Math.Max(0, ofAfterLast - pageSize);
                        return ExtractPage(new SimpleEnumerator(entries, 
                            offFirst, Math.Min(pageSize, ofAfterLast - offFirst)));
                    }
                    return new object[0];
                }
            }

            return ExtractPage(new SimpleEnumerator(entries));
        }

        /// <summary>
        /// Extract a subset of the specified set to fit the filter's
        /// parameters (i.e. page size and page number).
        /// </summary>
        /// <remarks>
        /// If this filter has a comparator, the specified <b>ICollection</b>
        /// is presumed to be sorted accordingly.
        /// <p />
        /// The returned set is guaranteed to iterate exactly in the same
        /// order as the original set.
        /// </remarks>
        /// <param name="entries">
        /// An original set of entries.
        /// </param>
        /// <returns>
        /// A set of entries extracted accordingly to the filter
        /// parameters.
        /// </returns>
        public virtual ICollection ExtractPage(ICollection entries)
        {
            return ExtractPage(entries.GetEnumerator());
        }

        /// <summary>
        /// Extract a subset of the specified iterator to fit the filter's
        /// parameters (i.e. page size and page number).
        /// </summary>
        /// <remarks>
        /// The returned array is guaranteed to iterate exactly in the same
        /// order as the original iterator.
        /// </remarks>
        /// <param name="iter">
        /// An original entry iterator.
        /// </param>
        /// <returns>
        /// An array of entries extracted accordingly to the filter
        /// parameters
        /// </returns>
        public virtual object[] ExtractPage(IEnumerator iter)
        {
            int       pageSize     = PageSize;
            IComparer comparer     = Comparer;
            object    anchorTop    = TopAnchor;
            object    anchorBottom = BottomAnchor;
            var       entries      = new object[pageSize];
            int       entryIndex   = 0;

            if (comparer == null || anchorTop == null && anchorBottom == null)
            {
                int skip = Page * pageSize;

                // THIS IS A HACK: reconsider
                if (comparer == null && anchorTop is int)
                {
                    skip = ((int) anchorTop);
                }

                while (iter.MoveNext())
                {
                    object entry = iter.Current;

                    if (--skip >= 0)
                    {
                        continue;
                    }

                    entries[entryIndex] = entry;

                    if (++entryIndex == pageSize)
                    {
                        break;
                    }
                }

                if (entryIndex < pageSize)
                {
                    // last page is not full
                    int size  = entryIndex;
                    var array = new object[size];

                    if (size > 0)
                    {
                        Array.Copy(entries, 0, array, 0, size);
                    }
                    entries = array;
                }
            }
            else
            {
                bool isHeading   = anchorTop != null || anchorBottom == null;
                bool isInclusive = anchorTop != null && anchorBottom != null;
                bool skip        = isHeading;
                bool wrap        = false;
                var entryTop     = new CacheEntry(null, anchorTop);
                var entryBottom  = new CacheEntry(null, anchorBottom);

                while (iter.MoveNext())
                {
                    var entry = (CacheEntry) iter.Current;

                    if (skip)
                    {
                        int compare = comparer.Compare(entry, entryTop);

                        skip = isInclusive ? (compare < 0) : (compare <= 0);
                        if (skip)
                        {
                            continue;
                        }
                    }

                    if (isHeading)
                    {
                        entries[entryIndex] = entry;

                        if (++entryIndex == pageSize)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (comparer.Compare(entry, entryBottom) >= 0)
                        {
                            break;
                        }

                        entries[entryIndex] = entry;

                        if (++entryIndex == pageSize)
                        {
                            wrap       = true;
                            entryIndex = 0;
                        }
                    }
                }

                if (wrap)
                {
                    var array = new object[pageSize];

                    Array.Copy(entries, entryIndex, array, 0, pageSize - entryIndex);
                    Array.Copy(entries, 0, array, pageSize - entryIndex, entryIndex);
                    entries = array;
                }
                else if (entryIndex < pageSize)
                {
                    // last page is not full
                    int size  = entryIndex;
                    var array = new object[size];

                    if (size > 0)
                    {
                        Array.Copy(entries, 0, array, 0, size);
                    }
                    entries = array;
                }
            }
            return entries;
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void ReadExternal(IPofReader reader)
        {
            m_filter       = (IFilter) reader.ReadObject(0);
            m_pageSize     = reader.ReadInt32(1);
            m_page         = reader.ReadInt32(2);
            m_comparer     = (IComparer) reader.ReadObject(3);
            m_anchorTop    = reader.ReadObject(4);
            m_anchorBottom = reader.ReadObject(5);

        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteExternal(IPofWriter writer)
        {
            writer.WriteObject(0, m_filter);
            writer.WriteInt32 (1, m_pageSize);
            writer.WriteInt32 (2, m_page);
            writer.WriteObject(3, m_comparer);
            writer.WriteObject(4, m_anchorTop);
            writer.WriteObject(5, m_anchorBottom);
        }

        #endregion

        #region IIndexAwareFilter implementation

        /// <summary>
        /// Given an IDictionary of available indexes, determine if this 
        /// IIndexAwareFilter can use any of the indexes to assist in its 
        /// processing, and if so, determine how effective the use of that 
        /// index would be.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The returned value is an effectiveness estimate of how well this 
        /// filter can use the specified indexes to filter the specified 
        /// keys. An operation that requires no more than a single access to 
        /// the index content (i.e. Equals, NotEquals) has an effectiveness of 
        /// <b>one</b>. Evaluation of a single entry is assumed to have an 
        /// effectiveness that depends on the index implementation and is 
        /// usually measured as a constant number of the single operations.  
        /// This number is referred to as <i>evaluation cost</i>.
        /// </p>
        /// <p>
        /// If the effectiveness of a filter evaluates to a number larger 
        /// than the keys.size() then a user could avoid using the index and 
        /// iterate through the keys calling <tt>Evaluate</tt> rather than 
        /// <tt>ApplyIndex</tt>.
        /// </p>
        /// </remarks>
        /// <param name="indexes">
        /// The available <see cref="ICacheIndex"/> objects keyed by the 
        /// related IValueExtractor; read-only.
        /// </param>
        /// <param name="keys">
        /// The set of keys that will be filtered; read-only.
        /// </param>
        /// <returns>
        /// An effectiveness estimate of how well this filter can use the 
        /// specified indexes to filter the specified keys.
        /// </returns>
        public int CalculateEffectiveness(IDictionary indexes, ICollection keys)
        {
            IFilter filter = m_filter;
            return filter is IIndexAwareFilter
                ? ((IIndexAwareFilter) filter).CalculateEffectiveness(indexes, keys)
                : keys.Count*ExtractorFilter.EVAL_COST;
        }

        /// <summary>
        /// Filter remaining keys using an IDictionary of available indexes.
        /// </summary>
        /// <remarks>
        /// The filter is responsible for removing all keys from the passed 
        /// set of keys that the applicable indexes can prove should be 
        /// filtered. If the filter does not fully evaluate the remaining 
        /// keys using just the index information, it must return a filter
        /// (which may be an <see cref="IEntryFilter"/>) that can complete the 
        /// task using an iterating implementation. If, on the other hand, the
        /// filter does fully evaluate the remaining keys using just the index
        /// information, then it should return <c>null</c> to indicate that no 
        /// further filtering is necessary.
        /// </remarks>
        /// <param name="indexes">
        /// The available <see cref="ICacheIndex"/> objects keyed by the 
        /// related IValueExtractor; read-only.
        /// </param>
        /// <param name="keys">
        /// The mutable set of keys that remain to be filtered.
        /// </param>
        /// <returns>
        /// An <see cref="IFilter"/> object that can be used to process the 
        /// remaining keys, or <c>null</c> if no additional filter processing 
        /// is necessary.
        /// </returns>
        public IFilter ApplyIndex(IDictionary indexes, ICollection keys)
        {
            IFilter filter = m_filter;
            if (filter is IIndexAwareFilter)
            {
                return ((IIndexAwareFilter) filter).ApplyIndex(indexes, keys);
            }
            return filter;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The IFilter whose results are truncated by this filter.
        /// </summary>
        private IFilter m_filter;

        /// <summary>
        /// The number of entries per page.
        /// </summary>
        private int m_pageSize;

        /// <summary>
        /// The page number.
        /// </summary>
        private int m_page;

        /// <summary>
        /// The IComparer used to partition the entry values into pages.
        /// </summary>
        private IComparer m_comparer;

        /// <summary>
        /// The top anchor object (the last object on a previous page).
        /// </summary>
        private object m_anchorTop;

        /// <summary>
        /// The bottom anchor object (the last object on the current page).
        /// </summary>
        private object m_anchorBottom;

        /// <summary>
        /// The cookie object used by the query processors to store a
        /// transient state of the request (on a client side).
        /// </summary>
        [NonSerialized]
        private object m_cookie;

        #endregion
    }
}