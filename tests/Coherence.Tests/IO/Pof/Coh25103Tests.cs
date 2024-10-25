/*
 * Copyright (c) 2022, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System.IO;

using NUnit.Framework;

namespace Tangosol.IO.Pof
{
    [TestFixture]
    public class Coh25103Tests
    {
        ConfigurablePofContext m_pofContext;

        [SetUp]
        public void Setup()
        {
            m_pofContext = new ConfigurablePofContext("config/include-pof-config.xml");
        }

        [Test]
        public void TestCharString()
        {
            byte[] bytes = {0x01,  // position 1
                            0x5D,  // T_UNIFORM_MAP
                            0x4E,  // key-type   -> T_CHAR_STRING
                            0x4E,  // value-type -> T_CHAR_STRING
                            0x01,  // one key/value pair
                            0x01,  // string length 1
                            0x44,  // letter 'D'
                            0x64,  // V_REFERENCE_NULL
                            0x40}; // EOS

            DoReadRemainder(bytes);
        }

        [Test]
        public void TestOctetString()
        {
            byte[] bytes = {0x01,  // position 1
                            0x5D,  // T_UNIFORM_MAP
                            0x4C,  // key-type   -> T_OCTET_STRING
                            0x4C,  // value-type -> T_OCTET_STRING
                            0x01,  // one key/value pair
                            0x01,  // string length 1
                            0x44,  // letter 'D'
                            0x64,  // V_REFERENCE_NULL
                            0x40}; // EOS

            DoReadRemainder(bytes);
        }

        protected void DoReadRemainder(byte[] bytes)
        {
            MemoryStream buffer = new MemoryStream(bytes);
            IPofReader   reader = new PofStreamReader.UserTypeReader(new DataReader(buffer), m_pofContext, 0, 0);

            // ensure skipping doesn't raise an exception
            reader.ReadRemainder();
        }
    }
}
