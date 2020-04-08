/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.Security;
using System.Security.Principal;

using Tangosol.Net;

namespace Tangosol.Net.Security
{
    /// <summary>
    /// IdentityAsserter validates a token in order to establish a user's identity. 
    /// The token is an identity assertion, a statement that asserts an identity.
    /// </summary>
    /// <remarks>
    /// A token is opaque to Coherence. It could be a standard type such as a 
    /// SAML Assertion or a proprietary type.
    /// </remarks>
    /// <author>David Guy  2009.10.30</author>
    /// <since>Coherence 3.6</since>
    public interface IIdentityAsserter
    {
        /// <summary>
        /// Asserts an identity based on a token-based identity assertion.
        /// </summary>
        /// <param name="oToken">
        /// The actual token that asserts identity.
        /// </param>
        /// <param name="service">
        /// The IService asserting the identity token.
        /// </param>
        /// <returns>
        /// An IPrincipal representing the identity. 
        /// </returns>
        /// <exception cref="SecurityException">
        /// If the identity assertion fails.
        /// </exception>
        /// <since>
        /// Coherence 3.7 added service param which intentionally breaks 
        /// compatibility with Coherence 3.6
        /// </since>
        IPrincipal AssertIdentity(object oToken, IService service);
    }
}
