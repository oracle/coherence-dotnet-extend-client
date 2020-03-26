/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which returns the logical "or" of two other
    /// filters.
    /// </summary>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.10.27</author>
    /// <author>Goran Milosavljevic  2006.10.24</author>
    public class OrFilter : AnyFilter
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public OrFilter()
        {}

        /// <summary>
        /// Construct an "or" filter.
        /// </summary>
        /// <remarks>
        /// The result is defined as:
        /// <code>
        /// filterLeft || filterRight
        /// </code>
        /// </remarks>
        /// <param name="filterLeft">
        /// The "left" filter.
        /// </param>
        /// <param name="filterRight">
        /// The "right" filter.
        /// </param>
        public OrFilter(IFilter filterLeft, IFilter filterRight)
            : base(new[] { filterLeft, filterRight })
        {}

        #endregion
    }
}