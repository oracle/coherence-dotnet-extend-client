/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which tests the result of a member invocation
    /// for inequality to <c>null</c>.
    /// </summary>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.10.27</author>
    /// <author>Goran Milosavljevic  2006.10.24</author>
    public class IsNotNullFilter : NotEqualsFilter
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IsNotNullFilter()
        {}

        /// <summary>
        /// Construct an <b>IsNotNullFilter</b> for testing inequality to
        /// <c>null</c>.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        public IsNotNullFilter(string member)
            : base(member, null)
        {}

        #endregion
    }
}