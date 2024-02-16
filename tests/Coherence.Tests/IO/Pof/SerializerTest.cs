/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using NUnit.Framework;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// Unit tests of custom serializer/deserialiser.
    /// </summary>
    /// <author>lh  2011.06.10</author>
    [TestFixture]
    public class SerializerTest
    {
        private MemoryStream m_stream;
        private DataWriter m_writer;
        private PofStreamWriter m_pofwriter;
        private DataReader m_reader;
        private PofStreamReader m_pofreader;

        public void initPOF()
        {
            initPOFWriter();
            initPOFReader();
        }

        private void initPOFReader()
        {
            m_stream.Position = 0;
            m_reader = new DataReader(m_stream);
            m_pofreader = new PofStreamReader(m_reader, new SimplePofContext());
        }

        private void initPOFWriter()
        {
            m_stream = new MemoryStream();
            m_writer = new DataWriter(m_stream);
            m_pofwriter = new PofStreamWriter(m_writer, new SimplePofContext());
        }

        [Test]
        public void testSerialization()
        {
            String sPath = "Config/reference-pof-config.xml";
            var    ctx   = new ConfigurablePofContext(sPath);
            var    bal   = new Balance();
            var    p     = new Product(bal);
            var    c     = new Customer("Customer", p, bal);
            bal.setBalance(2.0);
            bal.setCustomer(c);

            initPOFWriter();
            m_pofwriter = new PofStreamWriter(m_writer, ctx);
            m_pofwriter.EnableReference();
            m_pofwriter.WriteObject(0, c);
            
            initPOFReader();
            m_pofreader = new PofStreamReader(m_reader, ctx);
            var cResult = (Customer) m_pofreader.ReadObject(0);
            Assert.IsTrue(cResult.getProduct().getBalance() == cResult.getBalance());            
        }
    }
}