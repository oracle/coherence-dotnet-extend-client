/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.Security;
using System.Security.Principal;


namespace Tangosol.Net.Security.Impl
{
    /// <summary>
    /// The default implementation of the IIdentityAsserter.
    /// </summary>
    /// <remarks>
    /// The default implementation asserts that the token is an IPrincipal.
    /// </remarks>
    public class DefaultIdentityAsserter : IIdentityAsserter
    {
        #region IIdentityAsserter implementation

        /// <summary>
        /// Asserts identity <see cref="IIdentityAsserter.AssertIdentity"/>
        /// </summary>
        /// <param name="oToken">
        /// Identity token.
        /// </param>
        /// <param name="service">
        /// The service the identity is asserted for.
        /// </param>
        /// <returns>
        /// The <b>IPrincipal</b> token; could be null.
        /// </returns>
        public virtual IPrincipal AssertIdentity(object oToken, IService service)
        {
            // support old behavior where a null token is passed if no IPrincipal
            // is in the client context
            if (oToken == null || oToken is IPrincipal)
            {
                return (IPrincipal) oToken;
            }
            CacheFactory.Log("DefaultIdentityAsserter expected IPrincipal but found: " 
                + oToken, CacheFactory.LogLevel.Error);
            throw new SecurityException("identity token is unsupported type");
        }

        #endregion
        
        #region Constants

        /// <summary>
        /// An instance of the DefaultIdentityAsserter.
        /// </summary>
        public static readonly DefaultIdentityAsserter Instance = 
            new DefaultIdentityAsserter();

        #endregion
    }
}
