/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Util
{
    /// <summary>
    /// Implementation of the IList interface in a read-only fashion based on an
    /// array.
    /// </summary>
    /// <author>Mark Falco  2009.09.20</author>
    public class ImmutableArrayList : ImmutableMultiList
    {
        #region Constructors

        /// <summary>
        /// Construct a List containing the elements of the specified array.
        /// </summary>
        /// <param name="ao">
        /// the array backing the list
        /// </param>
        public ImmutableArrayList(Object[] ao)
            : base(new Object[][] {ao})
        {
        }

        #endregion
    }
}