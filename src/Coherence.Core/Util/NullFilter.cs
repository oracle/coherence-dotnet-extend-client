/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Util
{
    /// <summary>
    /// Filter which discards null references.
    /// </summary>
    /// <author>Cameron Purdy  1998.08.17</author>
    /// <author>Ana Cikic  2007.09.13</author>
    public class NullFilter : IFilter, IPortableObject
    {
        #region IFilter implementation

        /// <summary>
        /// Apply the test to the object.
        /// </summary>
        /// <param name="o">
        /// An object to which the test is applied.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        public bool Evaluate(object o)
        {
            return o != null;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compares this object with another object for equality.
        /// </summary>
        /// <param name="o">
        /// An object reference or <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> if the passed object reference is of the same class
        /// and has the same state as this object.
        /// </returns>
        public override bool Equals(object o)
        {
            return o is NullFilter;
        }

        /// <summary>
        /// Returns a hash code value for this object.
        /// </summary>
        /// <returns>
        /// A hash code value for this object.
        /// </returns>
        public override int GetHashCode()
        {
            return 0x0F;
        }

        /// <summary>
        /// Provide a human-readable representation of this object.
        /// </summary>
        /// <returns>
        /// A string whose contents represent the value of this object.
        /// </returns>
        public override string ToString()
        {
            return "NullFilter";
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public void ReadExternal(IPofReader reader)
        {}

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public void WriteExternal(IPofWriter writer)
        {}

        #endregion
    }
}