/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using NUnit.Framework;
using Tangosol.Run.Xml;

namespace Tangosol.Net.Ssl
{
    [TestFixture]
    [Ignore("disable for now")]
    public class SslTest 
    {
        private const string SERVER_CERT = "./Net/Ssl/Server.pfx";
        private const string CLIENT_CERT = "./Net/Ssl/Client.pfx";
        private const string CLIENT_CERT_PASSWORD = "password";

        [Test]
        public void TestSslServerAuthentication()
        {
            var server = new SslServer()
            {
                ServerCertificate = SslServer.LoadCertificate(SERVER_CERT)
            };
            server.Start();

            var client =
                new SslClient(server.EndPoint)
                    {
                        ServerName = "MyServerName",
                        Protocol   = SslProtocols.Default
                    };
            try
            {
                client.Connect();

                var echo = client.Echo("Hello World");
                Assert.AreEqual(echo, "Hello World");
            }
            finally
            {
                client.Close();
                server.Stop();
            }
        }

        [Test]
        [Ignore("fails intermittently due to some threading issue")]
        public void TestSslClientAuthenticationException()
        {
            var server = new SslServer()
                    {
                        ServerCertificate  = SslServer.LoadCertificate(SERVER_CERT),
                        AuthenticateClient = true
                    };
            server.Start();

            var client =
                    new SslClient(server.EndPoint)
                        {
                            ServerName   = "MyServerName",
                            Protocol     = SslProtocols.Default,
                            Certificates = null
                        };
            try
            {
                client.Connect();
                
                // the server should've closed the connection because the client certificate is missing,
                // so the following should fail
                Thread.Sleep(500);
                Assert.That(() => client.Echo("Hello World"), Throws.TypeOf<IOException>());
            }
            finally
            {
                client.Close();
                server.Stop();
            }
        }

        [Test]
        public void TestSslClientAuthentication()
        {
            var server = new SslServer()
            {
                ServerCertificate = SslServer.LoadCertificate(SERVER_CERT),
                AuthenticateClient = true
            };
            server.Start();

            var client =
                new SslClient(server.EndPoint)
                        {
                            ServerName = "MyServerName",
                            Protocol   = SslProtocols.Default
                        };
            try
            {
                client.AppendCertificate(new X509Certificate2(CLIENT_CERT, CLIENT_CERT_PASSWORD));
                client.Connect();

                var echo = client.Echo("Hello World");
                Assert.AreEqual("Hello World", echo);
            }
            finally
            {
                client.ClearCertificate();
                client.Close();
                server.Stop();
            }
        }

        [Test]
        public void TestSslClientConfiguration()
        {
            var server = new SslServer()
            {
                ServerCertificate = SslServer.LoadCertificate(SERVER_CERT),
                AuthenticateClient = true
            };
            server.Start();
            var client = new TcpClient();
            try
            {
                var xmlDoc = XmlHelper.LoadXml("./Net/Ssl/Configs/config.xml");
                
                var streamProvider = StreamProviderFactory.CreateProvider(xmlDoc);

                client.Connect(server.EndPoint);
                var stream = streamProvider.GetStream(client);
                
                var echo = SslClient.Echo(stream, "Hello World");
                Assert.AreEqual(echo, "Hello World");
            }
            finally
            {
                client.Close();
                server.Stop();
            }
        }

        [Test]
        public void TestSslClientConfiguration2()
        {
            var xmlDoc = XmlHelper.LoadXml("./Net/Ssl/Configs/config2.xml");

            Assert.That(() => StreamProviderFactory.CreateProvider(xmlDoc), Throws.TypeOf<TypeLoadException>());
        }

        [Test]
        public void TestSslClientConfiguration3()
        {
            var server = new SslServer()
            {
                ServerCertificate = SslServer.LoadCertificate(SERVER_CERT),
                AuthenticateClient = true
            };
            server.Start();
            var client = new TcpClient();
            try
            {
                var xmlDoc = XmlHelper.LoadXml("./Net/Ssl/Configs/config3.xml");

                var streamProvider = StreamProviderFactory.CreateProvider(xmlDoc);

                client.Connect(server.EndPoint);
                var stream = streamProvider.GetStream(client);

                var echo = SslClient.Echo(stream, "Hello World");
                Assert.AreEqual(echo, "Hello World");
            }
            finally
            {
                client.Close();
                server.Stop();
            }
        }

        [Test]
        public void TestSslClientConfiguration4()
        {
            var server = new SslServer()
            {
                ServerCertificate = SslServer.LoadCertificate(SERVER_CERT),
                AuthenticateClient = true
            };
            server.Start();
            
            var client = new TcpClient();
            try
            {
                var xmlDoc = XmlHelper.LoadXml("./Net/Ssl/Configs/config4.xml");
                var streamProvider = StreamProviderFactory.CreateProvider(xmlDoc);

                client.Connect(server.EndPoint);
                Assert.That(() => streamProvider.GetStream(client), Throws.TypeOf<AuthenticationException>());
            }
            finally
            {
                client.Close();
                server.Stop();
            }
        }

        [Test]
        public void TestSslClientConfiguration5()
        {
            var xmlDoc = XmlHelper.LoadXml("./Net/Ssl/Configs/config5.xml");
            Assert.That(() => StreamProviderFactory.CreateProvider(xmlDoc), Throws.InstanceOf<CryptographicException>());
        }

        [Test]
        public void TestSslConfigurationWithSelector()
        {
            var xmlDoc = XmlHelper.LoadXml("./Net/Ssl/Configs/config6.xml");
            Assert.NotNull(xmlDoc);

            var streamProvider = StreamProviderFactory.CreateProvider(xmlDoc);
            Assert.NotNull(streamProvider);
            Assert.IsTrue(streamProvider is SslStreamProvider);

            var sslStreamProvider = (streamProvider as SslStreamProvider);
            Assert.IsTrue(sslStreamProvider.LocalCertificateSelector is LocalCertificateSelectionCallback);

            var server = new SslServer()
            {
                ServerCertificate = SslServer.LoadCertificate(SERVER_CERT),
                AuthenticateClient = true
            };
            server.Start();

            var client = new TcpClient();
            try
            {
                client.Connect(server.EndPoint);
                var stream = streamProvider.GetStream(client);

                var echo = SslClient.Echo(stream, "Hello World");
                Assert.Fail("Expected Exception, but got none");
            }
            catch (Exception e)
            {
                Console.WriteLine("SslTests.TestSslConfigurationWithSelector(), exception: " + e.ToString());
                Assert.IsTrue(e is AuthenticationException);
                Assert.AreEqual("The remote certificate is invalid according to the validation procedure.", e.Message);
            }
            finally
            {
                client.Close();
                server.Stop();
            }
        }

        [Test]
        public void TestSslRemoteCertValidation()
        {
            var xmlDoc = XmlHelper.LoadXml("./Net/Ssl/Configs/config7.xml");
            Assert.NotNull(xmlDoc);

            var streamProvider = StreamProviderFactory.CreateProvider(xmlDoc);
            Assert.NotNull(streamProvider);
            Assert.IsTrue(streamProvider is SslStreamProvider);

            var sslStreamProvider = (streamProvider as SslStreamProvider);
            Assert.IsTrue(sslStreamProvider.RemoteCertificateValidator is RemoteCertificateValidationCallback);

            var server = new SslServer()
            {
                ServerCertificate = SslServer.LoadCertificate(SERVER_CERT),
                AuthenticateClient = true
            };
            server.Start();

            var client = new TcpClient();
            try
            {
                client.Connect(server.EndPoint);
                var stream = streamProvider.GetStream(client);
                Assert.NotNull(stream);
                Assert.AreEqual(sslStreamProvider.ServerName, "MyServerName");

                var echo = SslClient.Echo(stream, "Hello World");
                Assert.AreEqual(echo, "Hello World");
            }
            finally
            {
                client.Close();
                server.Stop();
            }
        }
    }
}
