/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Security.Principal;

namespace Tangosol.Net.Security.Impl
{
    /// <summary>
    /// Simple <see cref="IPrincipal"/> implementation that extends <see cref="GenericPrincipal"/>,
    /// implementing only <b>Equals</b> and <b>GetHashCode</b>.
    /// For hash code generation and equality comparison, <b>SimplePrincipal</b> uses
    /// only the <b>Identity.Name</b> property.
    /// </summary>
    /// <author>Pat Rollman  2013.12.2</author>
    /// <since>Coherence 3.7.1.11</since>
    public class SimplePrincipal : GenericPrincipal
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">
        /// <b>IIdentity </b> a generic user representation.
        /// </param>
        /// <param name="roles">
        /// A list of roles to which the user represented by IIdentity belongs.
        /// </param>
        public SimplePrincipal(IIdentity id, String[] roles) : base(id, roles)
        {
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">
        /// The object to compare with the current object.
        /// </param>
        /// <returns>
        /// <b>true</b> if obj is a <b>SimplePrincipal</b> and its <b>Identity.Name</b> matches that of the current object.
        /// <b>false</b> otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj)
                || (obj is SimplePrincipal
                    && Identity.Name.Equals(((SimplePrincipal) obj).Identity.Name));
        }

        /// <summary>
        /// Obtain the hashcode for this object.
        /// </summary>
        /// <returns>
        /// The integer hashcode of the <b>Identity.Name</b> property.
        /// </returns>
        public override int GetHashCode()
        {
            return Identity.Name.GetHashCode();
        }
        
        #endregion
    }
}
