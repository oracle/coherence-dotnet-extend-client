/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Tangosol.Net.Ssl
{
    public sealed class SslClient
    {
        public SslClient(IPEndPoint server)
        {
            TcpClient = new TcpClient();
            ServerAddress = server;
            Certificates = new X509CertificateCollection();
        }

        private IPEndPoint ServerAddress { get; set; }

        public X509CertificateCollection Certificates { get; set; }

        public void AppendCertificate(X509Certificate certificate)
        {
            Certificates.Add(certificate);
        }

        public void ClearCertificate()
        {
            Certificates.Clear();
        }

        public void Connect()
        {
            TcpClient.Connect(ServerAddress);
            Stream = new SslStreamProvider
            {
                ServerName = ServerName,
                Protocols = Protocol,
                ClientCertificates = Certificates
            }.GetStream(TcpClient);
        }

        public string Echo(string message)
        {
            return Echo(Stream, message);
        }

        public void Close()
        {
            TcpClient.Close();
        }

        public static string Echo(Stream stream, string message)
        {
            byte[] ab = Encoding.UTF8.GetBytes(message + "<EOF>");
            // Send hello message to the server. 
            stream.Write(ab, 0, ab.Length);
            stream.Flush();
            // Read message from the server.
            string serverMessage = ReadMessage(stream);
            Console.WriteLine("Server says: {0}", serverMessage);
            return serverMessage;
        }

        private static string ReadMessage(Stream sslStream)
        {
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            var buffer = new byte[2048];
            var messageData = new StringBuilder();
            int bytes;
            do
            {
                bytes = sslStream.Read(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                var chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF.
                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {
                    break;
                }
            } while (bytes != 0);

            if (messageData.ToString().IndexOf("<EOF>") != -1)
            {
                messageData.Remove(messageData.Length - "<EOF>".Length,
                               "<EOF>".Length);
            }  
            return messageData.ToString();
        }

        public SslProtocols Protocol { get; set; }
        public string ServerName { get; set; }

        private Stream Stream { get; set; }

        private TcpClient TcpClient { get; set; }
    }
}
