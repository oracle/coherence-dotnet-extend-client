/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Util;

namespace Tangosol.IO
{
    /// <summary>
    /// The IDeltaCompressor interface provides the capability of comparing two
    /// in-memory buffers containing an old and a new value, and producing a result
    /// (called a "delta") that can be applied to the old value to create the new
    /// value.
    /// </summary>
    /// <author>Cameron Purdy  2009.01.06</author>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public interface IDeltaCompressor
    {
        /// <summary>
        /// <p>
        /// Compare an old value to a new value and generate a delta that
        /// represents the changes that must be made to the old value in order to
        /// transform it into the new value.  The generated delta must be a Binary
        /// of non-zero length.</p>
        /// <p>
        /// If the old value is null, the generated delta must be a "replace",
        /// meaning that applying it to any value must produce the specified new
        /// value.</p>
        /// </summary>
        /// <param name="binOld">
        /// The old value.
        /// </param>
        /// <param name="binNew">
        /// The new value; must not be null.
        /// </param>
        /// <returns>
        /// The changes that must be made to the old value in order to
        /// transform it into the new value, or null to indicate no change.
        /// </returns>
        Binary ExtractDelta(Binary binOld, Binary binNew);

        /// <summary>
        /// Apply a delta to an old value in order to create a new value.
        /// </summary>
        /// <param name="binOld">
        /// The old value.
        /// </param>
        /// <param name="binDelta">
        /// The delta information returned from <see cref="ExtractDelta"/>
        /// to apply to the old value.
        /// </param>
        /// <returns>
        /// The new value.
        /// </returns>
        Binary ApplyDelta(Binary binOld, Binary binDelta);
    }
}