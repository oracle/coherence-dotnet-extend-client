/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;

using Tangosol.Util;

namespace Tangosol.IO
{
    /// <summary>
    /// An abstract base class for implementing <see cref="IEvolvable"/>
    /// objects.
    /// </summary>
    /// <author>Cameron Purdy  2006.07.20</author>
    /// <author>Jason Howes  2006.07.20</author>
    /// <author>Marc Falco  2006.07.20</author>
    /// <author>Ivan Cikic  2007.05.17</author>
    public abstract class AbstractEvolvable : IEvolvable
    {
        #region IEvolvable interface implementation

        /// <summary>
        /// Determine the serialization version supported by the implementing
        /// type.
        /// </summary>
        /// <value>
        /// The serialization version supported by this object.
        /// </value>
        public abstract int ImplVersion
        {
            get;
        }

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
        public int DataVersion
        {
            get { return dataVersion; }
            set
            {
                dataVersion = value;
            }
        }

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
        public Binary FutureData
        {
            get { return futureData; }
            set
            {
                futureData = value;
            }
        }

        #endregion
        
        #region Data members

        /// <summary>
        /// The "unknown future data" from the data stream that this object 
        /// was deserialized from.
        /// </summary>
        private Binary futureData;
        
        /// <summary>
        /// The version of the data stream that this object was deserialized 
        /// from.
        /// </summary>
        private int dataVersion;
        

        #endregion
    }
}
