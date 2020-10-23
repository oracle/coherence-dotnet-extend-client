/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Threading;
using System.Xml;

using NUnit.Framework;

using Tangosol.Net;
using Tangosol.Run.Xml;

namespace Tangosol.Util.Logging
{
    [TestFixture]
    public class LoggingTests
    {
        [Test]
        public void TestInitialization()
        {
            Stream stream           = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-test-logging.xml");
            IXmlDocument config     = XmlHelper.LoadXml(stream);
            IOperationalContext ctx = new DefaultOperationalContext(config);
            Logger logger = new Logger();
            logger.Configure(ctx);

            Assert.AreEqual(logger.Destination, "stdout");
            Assert.AreEqual(logger.Level, 5);
            Assert.AreEqual(logger.Limit, 8192);
            Assert.AreEqual(logger.Format, "{date} {product} {version} <{level}> (thread={thread}, member={member}): {text}");
            Assert.AreEqual(logger.IsEnabled(3), true);
            Assert.AreEqual(logger.IsEnabled(8), false);
        }

        [Test]
        public void TestDefaultInitialization()
        {
            string xmlString   = "<coherence><logging-config/></coherence>";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            IXmlDocument config     = XmlHelper.ConvertDocument(xmlDoc);
            IOperationalContext ctx = new DefaultOperationalContext(config);
            Logger logger = new Logger();
            logger.Configure(ctx);

            Assert.AreEqual(logger.Destination, Logger.DefaultDestination);
            Assert.AreEqual(logger.Level, Logger.DefaultLevel);
            Assert.AreEqual(logger.Limit, Int32.MaxValue);
            Assert.AreEqual(logger.Format, Logger.DefaultFormat);
            Assert.AreEqual(logger.Parameters, logger.DefaultParameters);
        }

        [Test]
        public void TestStdOut()
        {
            Stream stream           = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-test-logging-stdout.xml");
            IXmlDocument config     = XmlHelper.LoadXml(stream);
            IOperationalContext ctx = new DefaultOperationalContext(config);
            Logger logger = new Logger();
            logger.Configure(ctx);

            Assert.AreEqual(logger.Destination, "stdout");
            logger.Start();

            logger.Log(1, "Error message", null);

            Exception e1 = new Exception("Exception without stack trace");
            logger.Log(2, e1, e1.Message, null);

            try
            {
                throw new Exception("Exception with stack trace");
            }
            catch (Exception e2)
            {
                logger.Log(3, e2, e2.Message, null);
                logger.Log(1, e2, null, null);
                logger.Log(1, null, "Exception message without exception", null);
            }

            logger.Shutdown();
        }

        [Test]
        public void TestStdErr()
        {
            Stream stream = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-test-logging-stderr.xml");
            IXmlDocument config = XmlHelper.LoadXml(stream);
            IOperationalContext ctx = new DefaultOperationalContext(config);
            Logger logger = new Logger();
            logger.Configure(ctx);

            Assert.AreEqual(logger.Destination, "stderr");
            logger.Start();
            logger.Log(1, "Error message", null);
            logger.Shutdown();

            logger.Limit = 10;
            logger.Start();
            logger.Log(2, "Message that will be logged", null);
            logger.Log(2, "Message that will not be logged", null);

            lock (this)
            {
                //so that daemon does not shut down before the last message is written
                Monitor.Wait(this, 1000);
            }
            logger.Shutdown();
        }

        [Test]
        public void TestFile()
        {
            Stream stream = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-test-logging-file.xml");
            IXmlDocument config = XmlHelper.LoadXml(stream);
            IOperationalContext ctx = new DefaultOperationalContext(config);
            Logger logger = new Logger();
            logger.Configure(ctx);

            Assert.AreEqual(logger.Destination, "Coherence.Core.Tests.log");
            logger.Start();

            Exception e = new Exception("TestFileLoggingException");
            object[] param = { 1, 2, 3 };
            string msg = "Error message";

            logger.Log(2, msg, null);
            logger.Log(2, msg, param);
            logger.Log(2, e, null);
            logger.Log(2, e, param);
            logger.Log(2, e, msg, null);
            logger.Log(2, e, msg, param);
            logger.Log(Logger.LEVEL_INTERNAL, msg, null);
            logger.Log(Logger.LEVEL_INTERNAL, e, null, null);
            logger.Log(Logger.LEVEL_INTERNAL, msg, param);
            logger.Log(Logger.LEVEL_INTERNAL, e, null, param);

            logger.Shutdown();
        }

        public void TestCommonLogging(String configFile)
        {
            Stream stream = GetType().Assembly.GetManifestResourceStream(configFile);
            IXmlDocument config = XmlHelper.LoadXml(stream);
            IOperationalContext ctx = new DefaultOperationalContext(config);
            Logger logger = new Logger();
            logger.Configure(ctx);

            logger.Start();

            Exception e = new Exception("TestCommonLoggerLoggingException");
            object[] param = { 1, 2, 3 };
            string msg = "Error message";

            logger.Log(0, e, null, null); //debug
            logger.Log(1, e, msg, null); //error
            logger.Log(2, e, msg, null); //warn
            logger.Log(3, e, msg, param); //info
            logger.Log(4, e, msg, null); //debug
            logger.Log(5, null, msg, null); //debug
            logger.Log(6, e, msg, null);
            logger.Log(7, e, msg, null);
            logger.Log(8, e, msg, param);
            logger.Log(9, e, null, null);

            try
            {
                CacheFactory.GetCache("testing-logging-exceptions");
            }
            catch (Exception e1)
            {
                logger.Log(1, e1, "Unable to connect to cache", null);
                logger.Log(1, e1, null, null);
            }

            logger.Shutdown();
        }

        [Test]
        public void TestCommonLoggingLogger()
        {
            TestCommonLogging("Tangosol.Resources.s4hc-test-logging-commonlogger.xml");
        }

        [Test]
        public void TestCommonLoggingMyLogger()
        {
            TestCommonLogging("Tangosol.Resources.s4hc-test-logging-commonlogger-mylogger.xml");
        }

        [Test]
        public void TestFileShare()
        {
            Stream stream = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-test-logging-file.xml");
            IXmlDocument config = XmlHelper.LoadXml(stream);
            IOperationalContext ctx = new DefaultOperationalContext(config);
            Logger logger = new Logger();
            logger.Configure(ctx);

            logger.Start();

            FileInfo info = new FileInfo(logger.Destination);
            // exception will be thrown if this thread can't access log file

            // I had tried just FileShare.Read since I only wanted read access and
            // IOException was thrown, so I guess you need to specify write as well
            // since the other process is writing to the file.
            FileStream fs = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fs.Close();

            logger.Shutdown();
        }

        [Test]
        public void TestToOracleVersion()
        {
            string oracleVersion = StringUtils.ToOracleVersion(new Version("12.1.2.1001"));
            Assert.AreEqual(oracleVersion, "12.1.2.1.1");
            oracleVersion = StringUtils.ToOracleVersion(new Version("12.1.2.1"));
            Assert.AreEqual(oracleVersion, "12.1.2.0.1");
            oracleVersion = StringUtils.ToOracleVersion(new Version("12.1.2.0"));
            Assert.AreEqual(oracleVersion, "12.1.2.0.0");
            oracleVersion = StringUtils.ToOracleVersion(new Version("12.1.2"));
            Assert.AreEqual(oracleVersion, "12.1.2.0.0");
        }
    }
}
