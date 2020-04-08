/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which compares the result of a member
    /// invocation with <c>null</c>.
    /// </summary>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.10.27</author>
    /// <author>Goran Milosavljevic  2006.10.24</author>
    public class IsNullFilter : EqualsFilter
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IsNullFilter()
        {}

        /// <summary>
        /// Construct an <b>IsNullFilter</b> for testing equality to
        /// <c>null</c>.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        public IsNullFilter(string member)
            : base(member, null)
        {}

        #endregion
    }
}