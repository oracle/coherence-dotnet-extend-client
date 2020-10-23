/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

using Tangosol.Net.Cache;

namespace Tangosol.Util.Comparator
{
    /// <summary>
    /// Comparator that reverses the result of another comparator.
    /// </summary>
    /// <author>Cameron Purdy, Gene Gleyzer  2002.11.01</author>
    /// <author>Ivan Cikic  2007.01.29</author>
    public class InverseComparer : SafeComparer, IQueryCacheComparer
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InverseComparer()
        {
        }

        /// <summary>
        /// Construct an <b>InverseComparer</b>.
        /// </summary>
        /// <param name="comparer">
        /// The comparer whose results are inverted by this <b>IComparer</b>.
        /// </param>
        public InverseComparer(IComparer comparer) : base(comparer)
        {
        }

        #endregion

        #region IComparer interface implementation

        /// <summary>
        /// Use the wrapped <b>IComparer</b> to compare the two arguments for
        /// order and negate the result.
        /// </summary>
        /// <param name="o1">
        /// The first object to be compared.
        /// </param>
        /// <param name="o2">
        /// The second object to be compared.
        /// </param>
        /// <returns>
        /// A positive integer, zero, or a negative integer as the first
        /// argument is less than, equal to, or greater than the second.
        /// </returns>
        public override int Compare(object o1, object o2)
        {
            return -base.Compare(o1, o2);
        }

        #endregion

        #region IQueryCacheComparer interface implementation


        /// <summary>
        /// Compare two entries using the underlying comparator and negate
        /// the result.
        /// </summary>
        /// <param name="entry1">
        /// The first entry to compare values from; read-only.
        /// </param>
        /// <param name="entry2">
        /// The second entry to compare values from; read-only.
        /// </param>
        /// <returns>
        /// A positive integer, zero, or a negative integer as the first
        /// entry denotes a value that is is less than, equal to, or greater
        /// than the value denoted by the second entry .
        /// </returns>
        public override int CompareEntries(IQueryCacheEntry entry1, IQueryCacheEntry entry2)
        {
            return -base.CompareEntries(entry1, entry2);
        }

        #endregion

        #region Object methods overrides

        /// <summary>
        /// Compare the <b>InverseComparer</b> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The other comparer.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>InverseComparer</b> and the passed object
        /// are equivalent <b>InverseComparer</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            return o is InverseComparer && base.Equals(o);
        }

        /// <summary>
        /// Return the hash code for this comparator.
        /// </summary>
        /// <returns>
        /// The hash code value for this comparator.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}