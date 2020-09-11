/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Tangosol.Net.Ssl
{
    public class ClientCertificateSelector
    {

        public ClientCertificateSelector()
        {
        }

        public static X509Certificate SelectClientCertificate(
                    object sender,
                    string targetHost,
                    X509CertificateCollection localCertificates,
                    X509Certificate remoteCertificate,
                    string[] acceptableIssuers)
        {
            Console.WriteLine(typeof(ClientCertificateSelector).Name +
                    ": Using custom delegate method for client certificate selector.");

            return localCertificates[0];
        }
    }
}