/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.Security.Principal;

namespace Tangosol.Net.Security.Impl
{
    /// <summary>
    /// The default implementation of the IIdentityTransformer.
    /// </summary>
    /// <remarks>
    /// The default implementation returns the IPrincipal that it receives.
    /// </remarks>
    public class DefaultIdentityTransformer : IIdentityTransformer
    {
        #region IIdentityTransformer implementation

        /// <summary>
        /// Transforms identity <see cref="IIdentityTransformer.TransformIdentity"/>
        /// </summary>
        /// <param name="principal">
        /// An <b>IPrincipal</b>.
        /// </param>
        /// <param name="service">
        /// The service the principal is for.
        /// </param>
        /// <returns>
        /// The <b>IPrincipal</b> it receives; could be null.
        /// </returns>
        public virtual object TransformIdentity(IPrincipal principal, 
            IService service)
        {
            return principal == null || !principal.Identity.IsAuthenticated ? 
                null : principal;
        }
        
        #endregion

        #region Constants

        /// <summary>
        /// An instance of the DefaultIdentityTransformer.
        /// </summary>
        public static readonly DefaultIdentityTransformer Instance = 
            new DefaultIdentityTransformer();

        #endregion
    }
}
