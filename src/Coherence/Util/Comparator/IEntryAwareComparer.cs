/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

namespace Tangosol.Util.Comparator
{
    /// <summary>
    /// <b>IEntryAwareComparator</b> is an extension to the 
    /// <b>IComparer</b> interface that allows the 
    /// <see cref="EntryComparer"/> to know whether the underlying comparer
    /// expects to compare the corresponding entries' keys or values.
    /// </summary>
    /// <author>Gene Gleyzer  2007.05.05</author>
    /// <author>Ivan Cikic  2007.05.16</author>
    public interface IEntryAwareComparer : IComparer
    {
        /// <summary>
        /// Specifies whether this comparer expects to compare keys or 
        /// values.
        /// </summary>
        /// <returns>
        /// <b>true</b> if entry keys are expected; <b>false</b> otherwise.
        /// </returns>>
        bool IsKeyComparer();
    }
}
