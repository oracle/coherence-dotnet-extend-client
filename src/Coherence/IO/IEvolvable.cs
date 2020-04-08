/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.Util;

namespace Tangosol.IO
{
    /// <summary>
    /// The <b>IEvolvable</b> interface is implemented by types that require
    /// forwards- and backwards-compatibility of their serialized form.
    /// </summary>
    /// <remarks>
    /// An <b>IEvolvable</b> class has an integer version identifier <b>n</b>,
    /// where <b>n >= 0</b>. When the contents and/or semantics of the
    /// serialized form of the <b>IEvolvable</b> class changes, the version
    /// identifier is increased. Two versions identifiers, <b>n1</b>
    /// and <b>n2</b>, indicate the same version if <b>n1 == n2</b>;
    /// the version indicated by <tt>n2</tt> is newer than the version
    /// indicated by <b>n1</b> if <b>n2 > n1</b>.
    /// <p/>
    /// The <b>IEvolvable</b> interface is designed to support the evolution of
    /// types by the addition of data. Removal of data cannot be safely
    /// accomplished as long as a previous version of the type exists that
    /// relies on that data. Modifications to the structure or semantics of
    /// data from previous versions likewise cannot be safely accomplished
    /// as long as a previous version of the type exists that relies on the
    /// previous structure or semantics of the data.
    /// <p/>
    /// When an <b>IEvolvable</b> object is deserialized, it retains any unknown
    /// data that has been added to newer versions of the type, and the
    /// version identifier for that data format. When the <b>IEvolvable</b> object
    /// is subsequently serialized, it includes both that version identifier
    /// and the unknown future data.
    /// <p/>
    /// When an <b>IEvolvable</b> object is deserialized from a data stream whose
    /// version identifier indicates an older version, it must default and/or
    /// calculate the values for any data fields and properties that have
    /// been added since that older version. When the <b>IEvolvable</b> object is
    /// subsequently serialized, it includes its own version identifier and
    /// all of its data. Note that there will be no unknown future data in
    /// this case; future data can only exist when the version of the data
    /// stream is newer than the version of the <b>IEvolvable</b> type.
    /// </remarks>
    /// <author>Cameron Purdy, Jason Howes  2006.07.14</author>
    /// <author>Aleksandar Seovic  2006.08.12</author>
    /// <since>Coherence 3.2</since>
    public interface IEvolvable
    {
        /// <summary>
        /// Determine the serialization version supported by the implementing
        /// type.
        /// </summary>
        /// <value>
        /// The serialization version supported by this object.
        /// </value>
        int ImplVersion { get; }

        /// <summary>
        /// Gets or sets the version associated with the data stream from
        /// which this object was deserialized.
        /// </summary>
        /// <remarks>
        /// If the object was constructed (not deserialized), the data
        /// version is the same as the implementation version.
        /// </remarks>
        /// <value>
        /// The version of the data used to initialize this object, greater
        /// than or equal to zero.
        /// </value>
        /// <exception cref="ArgumentException">
        /// If the specified version is negative.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If the object is not in a state in which the version can be set,
        /// for example outside of deserialization.
        /// </exception>
        int DataVersion { get; set; }

        /// <summary>
        /// Gets or sets all the unknown remainder of the data stream from
        /// which this object was deserialized.
        /// </summary>
        /// <remarks>
        /// The remainder is unknown because it is data that was originally
        /// written by a future version of this object's type.
        /// </remarks>
        /// <value>
        /// Future data in binary form.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If the object is not in a state in which the version can be set,
        /// for example outside of deserialization.
        /// </exception>
        Binary FutureData { get; set; }
    }
}