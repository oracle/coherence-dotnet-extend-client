/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// Trivial <see cref="IValueExtractor"/> implementation that does not
    /// actually extract anything from the passed value, but returns the
    /// value itself.
    /// </summary>
    /// <author>Jason Howes  2006.03.26</author>
    /// <author>Gene Gleyzer  2006.03.26</author>
    /// <author>Ivan Cikic  2006.10.20</author>
    /// <since>Coherence 3.2</since>
    public class IdentityExtractor : AbstractExtractor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IdentityExtractor()
        {}

        #endregion

        #region IValueExtractor implementation

        /// <summary>
        /// Simply return the passed object.
        /// </summary>
        /// <param name="target">
        /// An object to retrieve the value from.
        /// </param>
        /// <returns>
        /// The extracted value as an object; <c>null</c> is an acceptable
        /// value.
        /// </returns>
        public override object Extract(object target)
        {
            return target;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>IdentityExtractor</b> with another object to
        /// determine equality.
        ///  </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the passed object is an <b>IdentityExtractor</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            return o is IdentityExtractor;
        }

        /// <summary>
        /// Determine a hash value for the <b>IdentityExtractor</b> object
        /// according to the general <b>Object.GetHashCode()</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>IdentityExtractor</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return 7;
        }

        /// <summary>
        /// Provide a human-readable description of this
        /// <b>IdentityExtractor</b> object.
        /// </summary>
        /// <returns>
        /// A human-readable description of this <b>IdentityExtractor</b>
        /// object.
        /// </returns>
        public override string ToString()
        {
            return "IdentityExtractor";
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
        public virtual void ReadExternal(IPofReader reader)
        {
        }

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
        public virtual void WriteExternal(IPofWriter writer)
        {
        }

        #endregion

        #region Constants

        /// <summary>
        /// An instance of the IdentityExtractor.
        /// </summary>
        public static readonly IdentityExtractor Instance = new IdentityExtractor();

        #endregion
    }
}