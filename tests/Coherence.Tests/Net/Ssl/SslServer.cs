/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Tangosol.Net.Ssl
{
    public sealed class SslServer
    {
        public Thread Thread { get; set; }

        public TcpListener Listener { get;  set; }

        public SslProtocols Protocol { get; set; }

        public bool AuthenticateClient { get; set; }

        public bool CheckClientCertRevocation { get; set; }

        public X509Certificate ServerCertificate { get; set; }

        public int WriteTimeout { get; set; }

        public int ReadTimeout { get; set; }

        private bool Running { get; set; }

        public static X509Certificate LoadCertificate(string path)
        {
            return X509Certificate.CreateFromCertFile(path);
        }

        public SslServer(IPEndPoint localEp)
        {
            Listener = new TcpListener(IPAddress.Any, localEp.Port); 
            ReadTimeout = 5000;
            WriteTimeout = 5000;
            CheckClientCertRevocation = false;
            AuthenticateClient = false;
            Protocol = SslProtocols.Default;
        }
    
        public void Start()
        {
            Running = true;
            Thread = new Thread(AcceptClients);
            Thread.Start(); 
        }

        public void Stop()
        {
            Listener.Stop();
            Running = false;
            Thread.Join();
            Console.WriteLine("Stopped SslServer.");
        }

        private void AcceptClients()
        {
            try
            {
                Listener.Start();
                while (Running)
                {
                    ProcessClient(Listener.AcceptTcpClient());
                }
            }
            catch
            {
                //nothing
            }
        }

        /// <summary>
        /// Used by this server to verify the remote Secure Sockets Layer (SSL) certificate used for authentication.
        /// This callback ignores the content of the Common Name for the certificate during the validation.
        /// It also ignores the RemoteCertificateChainErrors because our self-signed client certificate used 
        /// for testing is not trusted.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="certificate">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
        /// <returns>A Boolean value that determines whether the specified certificate is accepted for authentication.</returns>
        public static bool DefaultCertificateValidation(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Allow RemoteCertificateChainErrors because self signed certificate may not be trusted.
            if ((sslPolicyErrors & ~(SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors)) == SslPolicyErrors.None) return true;

            StringBuilder sException = new StringBuilder();

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) ==
                SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                if (sException.Length > 0)
                {
                    sException.Append("\n");
                }
                sException.Append("The certificate was not available.");
            }
            throw new AuthenticationException(sslPolicyErrors.ToString());
        }

        private void ProcessClient(TcpClient client)
        {
            Console.WriteLine("Client Connected: {0}", client.Client.RemoteEndPoint);
            // A client has connected. Create the 
            // SslStream using the client's network stream.
            var sslStream = AuthenticateClient ? new SslStream(client.GetStream(), false, DefaultCertificateValidation)
                : new SslStream(client.GetStream(), false);

            // Authenticate the server but don't require the client to authenticate.
            try
            {
                sslStream.AuthenticateAsServer(ServerCertificate,
                                               AuthenticateClient, Protocol, CheckClientCertRevocation);
              
                // Set timeouts
                sslStream.ReadTimeout = ReadTimeout;
                sslStream.WriteTimeout = WriteTimeout;
                // Read a message from the client.   
                Console.WriteLine("Waiting for client message...");
                string messageData = ReadMessage(sslStream);
                Console.WriteLine("Received: {0}", messageData);

                // Write a message to the client.
                byte[] message =
                        Encoding.UTF8.GetBytes(messageData);
                Console.WriteLine("Sending hello message '{0}'.", messageData);
                sslStream.Write(message);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}",
                                      e.InnerException.Message);
                }
                Console.WriteLine(
                        "Authentication failed - closing the connection.");
                sslStream.Close();
                client.Close();
                throw;
            }
            finally
            {
                // The client stream will be closed with the sslStream
                // because we specified this behavior when creating
                // the sslStream.
                sslStream.Close();
                client.Close();
            }
        }

        private static string ReadMessage(Stream stream)
        {
            // Read the  message sent by the client.
            // The client signals the end of the message using the
            // "<EOF>" marker.
            var buffer = new byte[2048];
            var messageData = new StringBuilder();
            int bytes;
            do
            {
                // Read the client's test message.
                bytes = stream.Read(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                var chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF or an empty message.
                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }
    }
}
