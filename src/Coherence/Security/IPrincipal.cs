/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Security
{
    /// <summary>
    /// Defines the basic functionality of a principal object.
    /// </summary>
    /// <remarks>
    /// We need to replace standard .NET interface in order to
    /// support .NET Compact Framework, which doesn't contain classes
    /// from <see cref="System.Security.Principal"/> namespace.
    /// </remarks>
    /// <author>Aleksandar Seovic  2007.07.31</author>
    public interface IPrincipal
    {
        /// <summary>
        /// Gets the identity of the current principal.
        /// </summary>
        /// <value>
        /// The <see cref="IIdentity"/> object associated with the
        /// current principal.
        /// </value>
        IIdentity Identity { get; }

        /// <summary>
        /// Determines whether the current principal belongs to the specified role. 
        /// </summary>
        /// <param name="role">
        /// The name of the role for which to check membership.
        /// </param>
        /// <returns>
        /// <b>true</b> if the current principal is a member of the specified role; otherwise, <b>false</b>.
        /// </returns>
        bool IsInRole(string role);
    }
}