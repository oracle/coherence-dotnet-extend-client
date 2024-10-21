/*
 * Copyright (c) 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Tangosol.Net.Ssl
{
    public sealed class CertificateValidatorNonWindows
    {
        public CertificateValidatorNonWindows()
        {
        }

        /// <summary>
        /// Verifies the remote Secure Sockets Layer (SSL) certificate used for authentication. This callback
        /// ignores the content of the Common Name for the certificate during the validation.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="certificate">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="errors">One or more errors associated with the remote certificate.</param>
        /// <returns>A Boolean value that determines whether the specified certificate is accepted for authentication.</returns>
        public static bool IgnoreCommonNameCertificateValidation(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors errors)
        {
            return true;

        }
    }
}
