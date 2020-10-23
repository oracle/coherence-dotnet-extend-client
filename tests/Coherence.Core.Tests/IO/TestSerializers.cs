/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.IO.Pof;
using Tangosol.Run.Xml;

namespace Tangosol.IO
{
    /// <summary>
    /// Custom pof serializer used with default pof serializer
    /// running in the cache.
    /// </summary>
    public class TestPofSerializer : IPofContext
    {
        public TestPofSerializer()
        {
            ctx = new ConfigurablePofContext();
        }

        public TestPofSerializer(string s)
        {
            ctx = new ConfigurablePofContext(s);

        }

        public void Serialize(DataWriter writer, object o)
        {
            // transform a well known string value to demonstrate that we are being called
            if (o is String && ((String)o).Equals("grid"))
                {
                ctx.Serialize(writer, STRING);
                }
            else
            {
                ctx.Serialize(writer, o);
            }
        }

        public object Deserialize(DataReader reader)
        {
            return ctx.Deserialize(reader);
        }

        public IPofSerializer GetPofSerializer(int typeId)
        {
            return ctx.GetPofSerializer(typeId);
        }

        public int GetUserTypeIdentifier(object o)
        {
            return ctx.GetUserTypeIdentifier(o);
        }

        public int GetUserTypeIdentifier(Type type)
        {
            return ctx.GetUserTypeIdentifier(type);
        }

        public int GetUserTypeIdentifier(string typeName)
        {
            return ctx.GetUserTypeIdentifier(typeName);
        }

        public string GetTypeName(int typeId)
        {
            return ctx.GetTypeName(typeId);
        }

        public Type GetType(int typeId)
        {
            return ctx.GetType(typeId);
        }

        public bool IsUserType(object o)
        {
            return ctx.IsUserType(o);
        }

        public bool IsUserType(Type type)
        {
            return ctx.IsUserType(type);
        }

        public bool IsUserType(string typeName)
        {
            return ctx.IsUserType(typeName);
        }

        #region data members

        /// </summary>
        /// Testname used to verify that this serializer was used in the test.
        /// <summary>
        private const String STRING = "TestPofSerializer";

        /// <summary>
        /// Pof context used by this serializer to serialize.
        /// </summary>
        private ConfigurablePofContext ctx;

        #endregion
    }


    /// <summary>
    /// Custom serializer used to test NET client against cache running
    /// the comparable Java custom serializer
    /// </summary>
    public class TestSerializer : ISerializer
    {
        public TestSerializer(string s)
        {
            ctx = new ConfigurablePofContext(s);
        }

        public void Serialize(DataWriter writer, object o)
        {
            // transform a well known string value to demonstrate that we are being called
            if (o is String && ((String)o).Equals("grid"))
            {
                ctx.Serialize(writer, STRING);
            }
            else
            {
                ctx.Serialize(writer, o);
            }
        }

        public object Deserialize(DataReader reader)
        {
            return ctx.Deserialize(reader);
        }

        #region data members

        /// <summary>
        /// Testname used to verify that this serializer was used in the test.
        /// </summary>
        private const String STRING = "TestSerializer";

        /// <summary>
        /// pof context used by this serializer to serialize
        /// </summary>
        private ConfigurablePofContext ctx;

        #endregion
    }

    /// <summary>
    /// Custom configurable serializer used to test NET client against cache running
    /// the comparable Java custom serializer.
    /// </summary>
    public class TestSerializerXmlConfigurable : IXmlConfigurable, ISerializer
    {
        public TestSerializerXmlConfigurable(IXmlElement xml)
        {
            setPofContext(new ConfigurablePofContext(xml));
        }
        public TestSerializerXmlConfigurable()
        {
            setPofContext(new ConfigurablePofContext());
        }
        public TestSerializerXmlConfigurable(String name, String pofconfig)
        {
            m_testName = name;
            m_pofcfg   = pofconfig;
        }

        public virtual IXmlElement Config
        {
            get
            {
                return getPofContext().Config;
            }

            set
            {
                /*
                * get the pof-config element from the provided config value
                * to instantiate the ConfigurablePofContext,
                * and also the test-name element to use in this test.
                */
                ParseXml(value);

                setPofContext(new ConfigurablePofContext(m_pofcfg));
            }
        }

        public void Serialize(DataWriter writer, object o)
        {
            // transform a well known string value to demonstrate that we are being called
            if (o is String && ((String)o).Equals("grid"))
            {
                getPofContext().Serialize(writer, m_testName);
            }
            else
            {
                getPofContext().Serialize(writer, o);
            }
        }

        public object Deserialize(DataReader reader)
        {
            return getPofContext().Deserialize(reader);
        }

        /// <summary>
        /// Parse test serializer config and set the members.
        /// </summary>
        private void ParseXml(IXmlElement xml)
        {
            m_pofcfg   = xml.GetElement("pof-config").GetString();
            m_testName = xml.GetElement("SerializerName").GetString();
        }

        #region accessors

        private ConfigurablePofContext getPofContext()
        {
            return m_Context;
        }

        private void setPofContext(ConfigurablePofContext Context)
        {
            m_Context = Context;
        }

        #endregion

        #region data members

        /// <summary>
        /// Pof context used by this serializer to serialize.
        /// </summary>
        private ConfigurablePofContext m_Context;

        /// <summary>
        /// Location string of the pof data types used by this serializer.
        /// </summary>
        private string m_pofcfg;

        /// <summary>
        /// Testname used to verify that this serializer was used in the test.
        /// </summary>
        private string m_testName;

        #endregion
    }
}