/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Security
{
    /// <summary>
    /// Defines the basic functionality of an identity object.
    /// </summary>
    /// <author>Aleksandar Seovic  2007.07.31</author>
    public interface IIdentity
    {
        /// <summary>
        /// Gets the type of authentication used.
        /// </summary>
        /// <value>
        /// The type of authentication used to identify the user.
        /// </value>
        string AuthenticationType { get; }

        /// <summary>
        /// Gets a value that indicates whether the user has been authenticated.
        /// </summary>
        /// <value>
        /// <b>true</b> if the user was authenticated; otherwise, <b>false</b>.
        /// </value>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        /// <value>
        /// The name of the user on whose behalf the code is running.
        /// </value>
        string Name { get; }
    }
}