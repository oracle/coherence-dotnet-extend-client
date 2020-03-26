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
    /// An IIdentityTransformer transforms an IPrincipal to a token that asserts 
    /// identity.
    /// </summary>
    /// <author>David Guy  2009.12.04</author>
    /// <since>Coherence 3.6</since>
    public interface IIdentityTransformer
    {
        /// <summary>
        /// Transform a Subject to a token that asserts an identity.
        /// </summary>
        /// <param name="principal">
        /// The IPrincipal representing a user.
        /// </param>
        /// <param name="service">
        /// The IService requesting an identity token.
        /// </param>
        /// <returns>
        /// The token that asserts identity. 
        /// </returns>
        /// <exception cref="SecurityException">
        /// If the identity transformation fails.
        /// </exception>
        /// <since>
        /// Coherence 3.7 added service param which intentionally breaks 
        /// compatibility with Coherence 3.6
        /// </since>
        object TransformIdentity(IPrincipal principal, IService service);
    }
}
