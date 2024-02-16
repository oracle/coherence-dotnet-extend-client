/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which returns the logical "and" of two other
    /// filters.
    /// </summary>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.10.26</author>
    /// <author>Goran Milosavljevic  2006.10.20</author>
    public class AndFilter : AllFilter
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AndFilter()
        {}

        /// <summary>
        /// Construct an "and" filter.
        /// </summary>
        /// <remarks>
        /// The result is defined as:
        /// <p>
        /// <pre>
        /// filterLeft &amp;&amp; filterRight
        /// </pre></p>
        /// </remarks>
        /// <param name="filterLeft">
        /// The "left" filter.
        /// </param>
        /// <param name="filterRight">
        /// The "right" filter.
        /// </param>
        public AndFilter(IFilter filterLeft, IFilter filterRight)
            : base(new[] { filterLeft, filterRight })
        {}

        #endregion
    }
}