/*
 * Copyright (c) 2000, 2021, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Tangosol.Run.Xml;

namespace Tangosol.Net
{
    ///<summary>
    /// Provide a secured network stream (SslStream) for a given connected TcpClient. 
    ///</summary>
    public class SslStreamProvider : IStreamProvider
    {
        #region Properties

        /// <summary>
        /// Address of remote server which the client is connected to.
        /// </summary>
        public virtual string RemoteAddress { get; set; }

        /// <summary>
        /// Gets or sets the host server specified by the client.
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the security protocol used to 
        /// authenticate this connection.
        /// </summary>
        public SslProtocols Protocols { get; set; }

        /// <summary>
        /// Gets or sets a X509CertificateCollection containing local 
        /// certificates.
        /// </summary>
        public X509CertificateCollection ClientCertificates { get; set; }

        /// <summary>
        /// Gets or sets the delegate used to select the local Secure Sockets 
        /// Layer (SSL) certificate used for authentication.
        /// </summary>
        public LocalCertificateSelectionCallback LocalCertificateSelector { get; set; }

        /// <summary>
        /// Get or sets the delegate used to verify the remote Secure Sockets 
        /// Layer (SSL) certificate used for authentication.
        /// </summary>
        public RemoteCertificateValidationCallback RemoteCertificateValidator { get; set; }

        #endregion

        #region Static methods

        /// <summary>
        /// Selects the local Secure Sockets Layer (SSL) certificate used for authentication.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="targetHost">The host server specified by the client.</param>
        /// <param name="localCertificates">An X509CertificateCollection containing local certificates.</param>
        /// <param name="remoteCertificate">The certificate used to authenticate the remote party.</param>
        /// <param name="acceptableIssuers">A String array of certificate issuers acceptable to the remote party.</param>
        /// <returns>An X509Certificate used for establishing an SSL connection.</returns>
        public static X509Certificate LocalCertificatePicker(
                object sender,
                string targetHost,
                X509CertificateCollection localCertificates,
                X509Certificate remoteCertificate,
                string[] acceptableIssuers)
        {
            if (localCertificates == null || localCertificates.Count == 0)
            {
                return null;
            }

            if (acceptableIssuers != null && acceptableIssuers.Length > 0)
            {
                // use the first certificate that is from an acceptable issuer
                foreach (X509Certificate certificate in localCertificates)
                {
                    if (((IList<string>)acceptableIssuers).Contains(certificate.Issuer))
                    {
                        return certificate;
                    }
                }
            }

            // As a last resort, return the first certificate in the localCertificates.
            return localCertificates[0];
        }

        private static bool CheckRemoteValidationErrors(SslPolicyErrors errors, SslPolicyErrors acceptedErrors)
        {
            if ((errors & ~acceptedErrors) == SslPolicyErrors.None) return true;

            StringBuilder sException = new StringBuilder();
            if ((errors & SslPolicyErrors.RemoteCertificateChainErrors) ==
                SslPolicyErrors.RemoteCertificateChainErrors)
            {
                sException.Append(
                        "The certificate chain was issued by an authority that is not trusted.");
            }

            if ((errors & SslPolicyErrors.RemoteCertificateNameMismatch) ==
                SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                if (sException.Length > 0)
                {
                    sException.Append("\n");
                }
                sException.Append(
                        "The certificate chain was issued to a diffent name.");
            }

            if ((errors & SslPolicyErrors.RemoteCertificateNotAvailable) ==
                SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                if (sException.Length > 0)
                {
                    sException.Append("\n");
                }
                sException.Append("The certificate was not available.");
            }
            throw new AuthenticationException(errors.ToString());
        }

        /// <summary>
        /// Verifies the remote Secure Sockets Layer (SSL) certificate used for authentication.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="certificate">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
        /// <returns>A Boolean value that determines whether the specified certificate is accepted for authentication.</returns>
        public static bool StrictCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return CheckRemoteValidationErrors(sslPolicyErrors, SslPolicyErrors.None);
        }

        /// <summary>
        /// Verifies the remote Secure Sockets Layer (SSL) certificate used for authentication. This callback
        /// ignores the content of the Common Name for the certificate during the validation.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="certificate">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
        /// <returns>A Boolean value that determines whether the specified certificate is accepted for authentication.</returns>
        public static bool IgnoreCommonNameCertificateValidation(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return CheckRemoteValidationErrors(sslPolicyErrors, SslPolicyErrors.RemoteCertificateNameMismatch);
        }

        /// <summary>
        /// Verifies the remote Secure Sockets Layer (SSL) certificate used for authentication.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="certificate">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
        /// <returns>A Boolean value that determines whether the specified certificate is accepted for authentication.</returns>
        public static bool DefaultCertificateValidation(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return CheckRemoteValidationErrors(sslPolicyErrors, SslPolicyErrors.None);
        }
        #endregion

        #region IStreamProvider implementation

        /// <summary>
        /// Get a secured stream (SSLStream) from an established connection (TcpClient).
        /// </summary>
        /// <param name="client">A connected TcpClient, used to establish a secured connection.</param>
        /// <returns>A SSLStream connected to the remote host.</returns>
        public Stream GetStream(TcpClient client)
        {
            try
            {
                string serverName = string.IsNullOrEmpty(ServerName)
                        ? RemoteAddress
                        : ServerName;
                if (serverName == null)
                {
                    serverName = ((IPEndPoint) client.Client.RemoteEndPoint).Address.ToString();
                }

                if (LocalCertificateSelector == null)
                {
                    LocalCertificateSelector = LocalCertificatePicker;
                }
                SslStream stream = new SslStream(client.GetStream(), false,
                                 RemoteCertificateValidator, LocalCertificateSelector);
                stream.AuthenticateAsClient(serverName, ClientCertificates, Protocols, false);
                return stream;
            }
            catch (AuthenticationException)
            {
                client.Close();
                throw;
            }
        }

        #endregion

        #region IXmlConfigurable implementation

        /// <summary>
        /// The current configuration of the object.
        /// </summary>
        /// <value>
        /// The XML configuration or <c>null</c>.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// When setting, if the object is not in a state that allows the
        /// configuration to be set; for example, if the object has already
        /// been configured and cannot be reconfigured.
        /// </exception>
        public IXmlElement Config
        {
            set
            {
                if (m_xmlConfiguration != null)
                {
                    throw new ArgumentException();
                }

                IXmlElement xml = m_xmlConfiguration = value;
                IXmlElement xmlSub = xml.GetElement("server");
                // configure the target server
                if (xmlSub != null)
                {
                    ServerName = xmlSub.GetString();
                }

                // configure the ssl protocol
                xmlSub = xml.GetElement("protocol");
                Protocols = xmlSub == null
                                    ? SslProtocols.Default
                                    : (SslProtocols)
                                      Enum.Parse(typeof(SslProtocols),
                                                 xmlSub.GetString());

                // configure the local X509Certificates
                xmlSub = xml.GetElement("local-certificates");
                if (xmlSub != null)
                {
                    var certificateCollection = new X509CertificateCollection();

                    foreach (IXmlElement element in xmlSub.ElementList)
                    {
                        if (element.Name == "certificate")
                        {
                            X509KeyStorageFlags storeFlags =
                                    X509KeyStorageFlags.DefaultKeySet;

                            IXmlElement xmlStore = element.GetElement("flags");
                            if (xmlStore != null)
                            {
                                storeFlags = (X509KeyStorageFlags)
                                    Enum.Parse(typeof(X509KeyStorageFlags), xmlStore.GetString(), true);
                            }

                            certificateCollection.Add(
                                    new X509Certificate2(
                                            element.GetElement("url").GetString(),
                                            element.GetElement("password").GetString(),
                                            storeFlags));

                            element.GetElement("password").SetString(null);
                        }
                    }

                    ClientCertificates = certificateCollection;
                    // Configure the local certificate selector
                    xmlSub = xmlSub.GetElement("selector");
                    LocalCertificateSelector = xmlSub == null
                            ? LocalCertificatePicker
                            : XmlHelper.CreateDelegate<LocalCertificateSelectionCallback>(xmlSub.GetElement("delegate"));
                }

                // configure the remote certificate validator
                xmlSub = xml.GetElement("remote-certificate-validator");
                RemoteCertificateValidator = xmlSub == null
                        ? DefaultCertificateValidation
                        : XmlHelper.CreateDelegate<RemoteCertificateValidationCallback>(xmlSub.GetElement("delegate"));
            }
            get
            {
                return m_xmlConfiguration;
            }

        }
        #endregion

        #region Data members

        /// <summary>
        /// Contains the configuration for this stream provider
        /// </summary>
        private IXmlElement m_xmlConfiguration;
        #endregion
    }
}
