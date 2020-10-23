/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;

using Tangosol.Util;

namespace Tangosol.Run.Xml
{
    [TestFixture]
    public class XmlHelperTests
    {
        #region Xml loading

        [Test]
        public void TestLoadXml()
        {
            IXmlDocument xmlDoc;
            IXmlDocument xmlDoc2;

            Stream stream = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-local-cache-config.xml");
            xmlDoc = XmlHelper.LoadXml(stream);
            Assert.IsNotNull(xmlDoc);
            stream = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-local-cache-config.xml");
            xmlDoc2 = XmlHelper.LoadXml(stream, Encoding.UTF8);
            Assert.IsNotNull(xmlDoc2);
            Assert.AreEqual(xmlDoc, xmlDoc2);
            xmlDoc2 = XmlHelper.LoadXml("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-local-cache-config.xml");
            Assert.AreEqual(xmlDoc, xmlDoc2);
        }

        [Test]
        public void TestConvertXmlDocument()
        {
            StringBuilder xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            xml.Append("<!--document comment-->");
            xml.Append("<root>");
            xml.Append("<child1 a1=\"val1\" a2=\"val2\">");
            xml.Append("<!--first comment-->");
            xml.Append("child1 value");
            xml.Append("<!--second comment-->");
            xml.Append("</child1>");
            xml.Append("<child2>");
            xml.Append("<![CDATA[cdata value ]]>");
            xml.Append("child2 value");
            xml.Append("</child2>");
            xml.Append("</root>");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml.ToString());

            Assert.IsNotNull(xmlDoc);
            IXmlDocument convXmlDoc = XmlHelper.ConvertDocument(xmlDoc);
            Assert.IsNotNull(convXmlDoc);

            Assert.AreEqual(convXmlDoc.Encoding, "utf-8");
            Assert.IsNull(convXmlDoc.DtdName);
            Assert.IsNull(convXmlDoc.DtdUri);
            Assert.AreEqual(convXmlDoc.DocumentComment, "document comment");
            Assert.AreEqual(convXmlDoc.Name, "root");
            Assert.AreEqual(convXmlDoc.ElementList.Count, 2);

            IXmlElement child1 = convXmlDoc.GetElement("child1");
            Assert.IsNotNull(child1);
            Assert.AreEqual(child1.Attributes.Count, 2);
            Assert.IsTrue(child1.Attributes.Contains("a1"));
            Assert.AreEqual(child1.GetAttribute("a1").GetString(), "val1");
            Assert.IsTrue(child1.Attributes.Contains("a2"));
            Assert.AreEqual(child1.GetAttribute("a2").GetString(), "val2");
            Assert.AreEqual(child1.GetString(), "child1 value");
            Assert.AreEqual(child1.Comment, "first comment\nsecond comment");

            IXmlElement child2 = convXmlDoc.GetElement("child2");
            Assert.IsNotNull(child2);
            Assert.AreEqual(child2.Attributes.Count, 0);
            Assert.AreEqual(child2.GetString(), "cdata value child2 value");
            Assert.IsTrue(child2.Comment.Length == 0);
        }

        #endregion

        #region Formatting helpers

        [Test]
        public void TestIsValid()
        {
            //IsEncodingValid
            Assert.IsFalse(XmlHelper.IsEncodingValid(null));
            Assert.IsFalse(XmlHelper.IsEncodingValid(""));
            Assert.IsFalse(XmlHelper.IsEncodingValid("  encoding"));
            Assert.IsFalse(XmlHelper.IsEncodingValid("invalid char ?"));
            Assert.IsTrue(XmlHelper.IsEncodingValid("UTF-8"));

            //IsSystemIdentifierValid
            Assert.IsTrue(XmlHelper.IsSystemIdentifierValid("something"));

            //IsPublicIdentifierValid
            Assert.IsTrue(XmlHelper.IsPublicIdentifierValid(""));
            Assert.IsFalse(XmlHelper.IsPublicIdentifierValid("invalid char \""));
            Assert.IsTrue(XmlHelper.IsPublicIdentifierValid("  Some Public 111 Identifier #@/"));

            //IsCommentValid
            Assert.IsFalse(XmlHelper.IsCommentValid("--comment"));
            Assert.IsTrue(XmlHelper.IsCommentValid("this is valid comment!"));

            //IsNameValid
            Assert.IsFalse(XmlHelper.IsNameValid(null));
            Assert.IsFalse(XmlHelper.IsNameValid(""));
            Assert.IsFalse(XmlHelper.IsNameValid("1n"));
            Assert.IsFalse(XmlHelper.IsNameValid("?name"));
            Assert.IsTrue(XmlHelper.IsNameValid("_name"));
            Assert.IsTrue(XmlHelper.IsNameValid(":name"));
            Assert.IsFalse(XmlHelper.IsNameValid("name?"));
            Assert.IsFalse(XmlHelper.IsNameValid("name\n"));
            Assert.IsTrue(XmlHelper.IsNameValid("name_1"));
        }

        [Test]
        public void TestIsWhitespace()
        {
            //IsWhitespace
            Assert.IsTrue(XmlHelper.IsWhitespace(' '));
            Assert.IsTrue(XmlHelper.IsWhitespace('\n'));
            Assert.IsTrue(XmlHelper.IsWhitespace('\t'));
            Assert.IsTrue(XmlHelper.IsWhitespace('\r'));
            Assert.IsFalse(XmlHelper.IsWhitespace('\\'));
        }

        [Test]
        public void TestTrim()
        {
            //Trim
            string s = " \t\r\n ana cikic \t\r\n";
            Assert.AreEqual("ana cikic", XmlHelper.Trim(s));
            s = "nowhitespace";
            Assert.AreEqual(s, XmlHelper.Trim(s));
            s = "\n\t ";
            Assert.AreEqual("", XmlHelper.Trim(s));

            //Trimf
            s = " \t\r\n ana cikic \t\r\n";
            Assert.AreEqual("ana cikic \t\r\n", XmlHelper.Trimf(s));
            s = "nowhitespace";
            Assert.AreEqual(s, XmlHelper.Trimf(s));
            s = "\n\t ";
            Assert.AreEqual("", XmlHelper.Trimf(s));

            //Trimb
            s = " \t\r\n ana cikic \t\r\n";
            Assert.AreEqual(" \t\r\n ana cikic", XmlHelper.Trimb(s));
            s = "nowhitespace";
            Assert.AreEqual(s, XmlHelper.Trimb(s));
            s = "\n\t ";
            Assert.AreEqual("", XmlHelper.Trimb(s));
        }

        [Test]
        public void TestEncodeDecodeAttribute()
        {
            Exception e = null;

            //EncodeAttribute & Quote
            string attr = "";
            Assert.AreEqual(attr, XmlHelper.EncodeAttribute(attr, '\''));
            attr = "Text that does not require encoding";
            Assert.AreEqual(attr, XmlHelper.EncodeAttribute(attr, '\''));
            Assert.AreEqual("'" + attr + "'", XmlHelper.Quote(attr));
            attr = "Text that contains & \" > < and ' etc";
            Assert.AreEqual("Text that contains &amp; \" &gt; &lt; and &apos; etc",
                            XmlHelper.EncodeAttribute(attr, '\''));
            Assert.AreEqual("Text that contains &amp; &quot; &gt; &lt; and ' etc", XmlHelper.EncodeAttribute(attr, '\"'));
            attr = "Text that contains control char " + (char) 0x1F;
            Assert.AreEqual("Text that contains control char &#x1F;", XmlHelper.EncodeAttribute(attr, '\''));
            try
            {
                XmlHelper.EncodeAttribute(attr, 'a');
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof (ArgumentException), e);

            //DecodeAttribute
            string decodedAttr = "Text that does not require decoding";
            Assert.AreEqual(decodedAttr, XmlHelper.DecodeAttribute(decodedAttr));
            decodedAttr = "Text that contains &amp; &gt; &lt; &quot; &ref; and &apos;";
            Assert.AreEqual("Text that contains & > < \" &ref; and '", XmlHelper.DecodeAttribute(decodedAttr));
            decodedAttr = "Text that contains control char &#x1F; and then something";
            Assert.AreEqual("Text that contains control char " + (char) 0x1F + " and then something",
                            XmlHelper.DecodeAttribute(decodedAttr));
            e = null;
            try
            {
                XmlHelper.DecodeAttribute("invalid & attr");
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof (ArgumentException), e);
            e = null;
            try
            {
                XmlHelper.DecodeAttribute("invalid hex &#xPP;");
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof (ArgumentException), e);
            e = null;
            try
            {
                XmlHelper.DecodeAttribute("invalid hex &#x;");
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof (ArgumentException), e);
        }

        [Test]
        public void TestEncodeDecodeContent()
        {
            //EncodeContent
            string content = "";
            Assert.AreEqual(content, XmlHelper.EncodeContent(content, false));
            content = "Text that does not require encoding";
            Assert.AreEqual(content, XmlHelper.EncodeContent(content, false));
            content = "\t\r\n Text with leading and trailing whitespace \r\n\t";
            Assert.AreEqual("&#x09;&#x0D;&#x0A;&#x20;Text with leading and trailing whitespace&#x20;&#x0D;&#x0A;&#x09;", XmlHelper.EncodeContent(content, false));
            content = "\tText with leading and trailing whitespace and some <]]>& in between \r\n";
            Assert.AreEqual("&#x09;Text with leading and trailing whitespace and some &lt;]]&gt;&amp; in between&#x20;&#x0D;&#x0A;", XmlHelper.EncodeContent(content, false));
            content = "Text that does not require encoding";
            Assert.AreEqual(content, XmlHelper.EncodeContent(content, true));
            content = "\tText with leading and trailing whitespace \r\n";
            Assert.AreEqual("<![CDATA[\tText with leading and trailing whitespace \r\n]]>", XmlHelper.EncodeContent(content, true));
            content = "Text with <&]]>";
            Assert.AreEqual("Text with &lt;&amp;]]&gt;", XmlHelper.EncodeContent(content, true));
            content = "Text with <&>";
            Assert.AreEqual("<![CDATA[Text with <&>]]>", XmlHelper.EncodeContent(content, true));

            //DecodeContent
            string decodedContent = "Text that does not require decoding";
            Assert.AreEqual(decodedContent, XmlHelper.DecodeContent(decodedContent));
            decodedContent = "&#x09;Text with leading and trailing whitespace&#x0A;&#x0A;";
            Assert.AreEqual("\tText with leading and trailing whitespace\n\n", XmlHelper.DecodeContent(decodedContent));
            decodedContent = "&#x09;Text with leading and trailing whitespace and some &lt;]]&gt;&amp; in between&#x0A;&#x0A;";
            Assert.AreEqual("\tText with leading and trailing whitespace and some <]]>& in between\n\n", XmlHelper.DecodeContent(decodedContent));
            decodedContent = "<![CDATA[\tText with leading and trailing whitespace\n\n]]>";
            Assert.AreEqual("<![CDATA[\tText with leading and trailing whitespace\n\n]]>", XmlHelper.DecodeContent(decodedContent));
        }

        [Test]
        public void TestEncodeDecodeUri()
        {
            string uri = "uri";
            Assert.AreEqual(uri, XmlHelper.EncodeUri(uri));
            uri = "escape uri " + (char) 0x1F;
            Assert.AreEqual("escape%20uri%20%1F", XmlHelper.EncodeUri(uri));
            Assert.AreEqual(uri, XmlHelper.DecodeUri("escape%20uri%20%1F"));
        }

        #endregion

        #region Element helpers

        [Test]
        public void TestElementHelpers()
        {
            Exception e = null;

            //GetAbsolutePath
            SimpleElement root = new SimpleElement();
            Assert.AreEqual(XmlHelper.GetAbsolutePath(root), "/");
            root.Name = "root";
            Assert.AreEqual(XmlHelper.GetAbsolutePath(root), "/root");
            SimpleElement se = new SimpleElement("child");
            se.Parent = root;
            Assert.AreEqual(XmlHelper.GetAbsolutePath(se), "/root/child");
            try
            {
                XmlHelper.GetAbsolutePath(null);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentNullException), e);

            //IsEmpty
            se = new SimpleElement();
            Assert.IsTrue(XmlHelper.IsEmpty(se));
            se.SetString("value");
            Assert.IsFalse(XmlHelper.IsEmpty(se));
            se.SetString(null);
            Assert.IsTrue(XmlHelper.IsEmpty(se));
            se.AddElement("name");
            Assert.IsFalse(XmlHelper.IsEmpty(se));
            se.ElementList.Clear();
            Assert.IsTrue(XmlHelper.IsEmpty(se));
            se.AddAttribute("attr");
            Assert.IsFalse(XmlHelper.IsEmpty(se));

            //GetElement
            se = new SimpleElement();
            Assert.IsNull(XmlHelper.GetElement(se, "child1"));
            se.AddElement("child1");
            se.AddElement("child2");
            Assert.IsNotNull(XmlHelper.GetElement(se, "child1"));
            e = null;
            try
            {
                XmlHelper.GetElement(se, "&invalid");
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);

            //FindElement
            root = new SimpleElement("root");
            IXmlElement child1 = root.AddElement("child1");
            IXmlElement child2 = child1.AddElement("child2");
            string path = "/child1";
            Assert.AreEqual(child1, XmlHelper.FindElement(root, path));
            Assert.AreEqual(child1, XmlHelper.FindElement(child1, path));
            path = "child2";
            Assert.IsNull(XmlHelper.FindElement(root, path));
            Assert.AreEqual(child2, XmlHelper.FindElement(child1, path));
            path = "../child1";
            Assert.IsNull(XmlHelper.FindElement(child2, path));
            Assert.AreEqual(child1, XmlHelper.FindElement(child1, path));
            e = null;
            try
            {
                XmlHelper.FindElement(root, null);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
            e = null;
            try
            {
                XmlHelper.FindElement(root, path);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);

            //FindElement with value
            root = new SimpleElement();
            child1 = root.AddElement("child1");
            child1.SetString("value1");
            root.AddElement("child1").SetString("value2");
            child2 = child1.AddElement("child2");
            path = "/child1";
            Assert.AreEqual(child1, XmlHelper.FindElement(root, path, "value1"));
            Assert.IsNull(XmlHelper.FindElement(root, path, "valueX"));
            path = "child2";
            Assert.IsNull(XmlHelper.FindElement(root, path, null));
            Assert.AreEqual(child2, XmlHelper.FindElement(child1, path, null));
            path = "../child1";
            Assert.IsNull(XmlHelper.FindElement(child2, path, null));
            Assert.AreNotEqual(child1, XmlHelper.FindElement(child1, path, "value2"));
            path = "child2/../child2";
            Assert.IsNull(XmlHelper.FindElement(child1, path, 5));
            Assert.AreEqual(child2, XmlHelper.FindElement(child1, path, null));
            e = null;
            try
            {
                XmlHelper.FindElement(null, path, null);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);

            //EnsureElement
            root = new SimpleElement("root");
            child1 = root.AddElement("child1");
            child1.AddElement("child2");
            path = "/child1";
            Assert.AreEqual(root.ElementList.Count, 1);
            Assert.AreEqual(child1, XmlHelper.EnsureElement(root, path));
            path = "/child3";
            Assert.IsNotNull(XmlHelper.EnsureElement(root, path));
            Assert.AreEqual(root.ElementList.Count, 2);
            path = "/child3/../child4";
            Assert.IsNotNull(XmlHelper.EnsureElement(root, path));
            Assert.AreEqual(root.ElementList.Count, 3);
            e = null;
            try
            {
                XmlHelper.EnsureElement(null, path);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
            e = null;
            try
            {
                XmlHelper.EnsureElement(root, "../child1");
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);

            //AddElements
            root = new SimpleElement();
            Assert.AreEqual(root.ElementList.Count, 0);
            IList elements = new ArrayList();
            elements.Add(new SimpleElement("el1"));
            elements.Add(new SimpleElement("el2"));
            elements.Add(new SimpleElement("el3"));
            XmlHelper.AddElements(root, elements.GetEnumerator());
            Assert.AreEqual(root.ElementList.Count, 3);

            //RemoveElement
            root = new SimpleElement();
            Assert.AreEqual(root.ElementList.Count, 0);
            int result = XmlHelper.RemoveElement(root, "child");
            Assert.AreEqual(result, 0);
            root.AddElement("child");
            root.AddElement("child");
            root.AddElement("child2");
            Assert.AreEqual(root.ElementList.Count, 3);
            result = XmlHelper.RemoveElement(root, "child");
            Assert.AreEqual(result, 2);
            Assert.AreEqual(root.ElementList.Count, 1);

            //ReplaceElement
            root = new SimpleElement("root");
            root.AddElement("child1");
            root.AddElement("child2");
            Assert.AreEqual(root.ElementList.Count, 2);
            Assert.IsNull(root.GetElement("child1").Value);
            IXmlElement replaceEl = new SimpleElement("child1", "value");
            bool replaced = XmlHelper.ReplaceElement(root, replaceEl);
            Assert.IsTrue(replaced);
            Assert.AreEqual(root.GetElement("child1").GetString(), replaceEl.GetString());
            Assert.AreEqual(root.ElementList.Count, 2);

            replaceEl = new SimpleElement("child3");
            replaced = XmlHelper.ReplaceElement(root, replaceEl);
            Assert.IsFalse(replaced);
            Assert.AreEqual(root.ElementList.Count, 3);
        }

        #endregion

        #region Misc helpers

        [Test]
        public void TestParseInitParams()
        {
            IXmlElement root = new SimpleElement("init-params");
            
            IXmlElement paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("string");
            paramEl.AddElement("param-value").SetString("test");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("bool");
            paramEl.AddElement("param-value").SetString("true");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("int");
            paramEl.AddElement("param-value").SetString("152");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("long");
            paramEl.AddElement("param-value").SetString("90089");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("float");
            paramEl.AddElement("param-value").SetString("3.5");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("double");
            paramEl.AddElement("param-value").SetString("3.521");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("decimal");
            paramEl.AddElement("param-value").SetString("0");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("date");
            paramEl.AddElement("param-value").SetString("2/16/1992 12:15:12");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("xml");
            IXmlElement value = paramEl.AddElement("param-value");
            value.AddElement("child").AddAttribute("attr").SetString("attrValue");

            object[] result = XmlHelper.ParseInitParams(root);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Length,9);
            Assert.IsTrue(result[0] is string);
            Assert.IsTrue(result[1] is bool);
            Assert.IsTrue(result[2] is int);
            Assert.IsTrue(result[3] is long);
            Assert.IsTrue(result[4] is float);
            Assert.IsTrue(result[5] is double);
            Assert.IsTrue(result[6] is decimal);
            Assert.IsTrue(result[7] is DateTime);
            Assert.IsTrue(result[8] is IXmlElement);

            root = new SimpleElement("init-params");
            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("System.String");
            paramEl.AddElement("param-value").SetString("test");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("System.Boolean");
            paramEl.AddElement("param-value").SetString("true");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("System.Int32");
            paramEl.AddElement("param-value").SetString("152");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("System.Int64");
            paramEl.AddElement("param-value").SetString("90089");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("System.Single");
            paramEl.AddElement("param-value").SetString("3.5");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("System.Double");
            paramEl.AddElement("param-value").SetString("3.521");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("System.Decimal");
            paramEl.AddElement("param-value").SetString("0");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("System.DateTime");
            paramEl.AddElement("param-value").SetString("2/16/1992 12:15:12");

            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("Tangosol.Run.Xml.IXmlElement");
            value = paramEl.AddElement("param-value");
            value.AddElement("child").AddAttribute("attr").SetString("attrValue");

            object[] result2 = XmlHelper.ParseInitParams(root);
            Assert.IsNotNull(result2);
            Assert.AreEqual(result2.Length, 9);
            Assert.IsTrue(result2[0] is string);
            Assert.IsTrue(result2[1] is bool);
            Assert.IsTrue(result2[2] is int);
            Assert.IsTrue(result2[3] is long);
            Assert.IsTrue(result2[4] is float);
            Assert.IsTrue(result2[5] is double);
            Assert.IsTrue(result2[6] is decimal);
            Assert.IsTrue(result2[7] is DateTime);
            Assert.IsTrue(result2[8] is IXmlElement);

            root = new SimpleElement("init-params");
            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type").SetString("Tangosol.Temperature");
            paramEl.AddElement("param-value").SetString("test");
            result2 = XmlHelper.ParseInitParams(root);
            Assert.AreEqual(result2.Length, 1);
            Assert.AreEqual(result2[0], paramEl);

            root = new SimpleElement("init-params");
            paramEl = root.AddElement("init-param");
            paramEl.AddElement("param-type");
            paramEl.AddElement("param-value").SetString("test");
            Exception e = null;
            try
            {
                XmlHelper.ParseInitParams(root);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);

            IXmlDocument xml = XmlHelper.LoadXml("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-util.xml");

            IXmlElement el = xml.FindElement("user-type-list/user-type/serializer/init-params");
            object[] o = XmlHelper.ParseInitParams(el);
            Assert.AreEqual(101, o[0]);
            Assert.IsInstanceOf(typeof(DateTime), o[1]);
            Assert.AreEqual(new DateTime(1992, 2, 2, 12, 15, 12), o[1]);
            Assert.AreEqual((float) 0.00000001, o[2]);

            object[] o1 = XmlHelper.ParseInitParams(el, new SimpleResolver());
            Assert.AreEqual(o1.Length, 4);
            Assert.AreEqual("test", o1[3]);

            object[] o2 = XmlHelper.ParseInitParams(el, new UnresolvedResolver());
            Assert.AreEqual(o2.Length, 4);
            Assert.AreEqual("{test}", o2[3]);
        }

        [Test]
        public void TestTransformInitParams()
        {
            IXmlDocument root = new SimpleDocument("root");
            IXmlDocument initParams = new SimpleDocument("init-params");
            for (int i = 1; i < 4; i++)
            {
                IXmlElement paramEl = initParams.AddElement("init-param");
                paramEl.AddElement("param-name").SetString("name" + i);
                paramEl.AddElement("param-value").SetInt(i);
            }
            Assert.AreEqual(initParams.ElementList.Count, 3);
            Assert.AreEqual(root.ElementList.Count, 0);
            XmlHelper.TransformInitParams(root, initParams);
            Assert.AreEqual(root.ElementList.Count, 3);
            for (int i = 1; i < 4; i++)
            {
                Assert.IsNotNull(root.GetElement("name" + i));
                Assert.AreEqual(root.GetElement("name" + i).GetInt(), i);
            }

            IXmlDocument xml = XmlHelper.LoadXml("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-util-transformparams.xml");

            IXmlElement el = xml.FindElement("configurable-cache-factory-config/init-params");

            root = new SimpleDocument("root");
            XmlHelper.TransformInitParams(root, el);
            Assert.IsNotNull(root.GetElement("long"));
            Assert.IsNotNull(root.GetElement("date"));
            Assert.IsNotNull(root.GetElement("float"));
        }

        [Test]
        public void TestCreateInstance()
        {
            // no params
            IXmlElement xmlClass = new SimpleElement();
            xmlClass.AddElement("class-name").SetString("Tangosol.Run.Xml.XmlHelperTests+Dummy, Coherence.Core.Tests");
            Assert.IsFalse(XmlHelper.IsInstanceConfigEmpty(xmlClass));
            object o = XmlHelper.CreateInstance(xmlClass, null);
            Assert.IsNotNull(o);
            Assert.IsInstanceOf(typeof(Dummy), o);
            Dummy d = o as Dummy;
            Assert.IsNotNull(d);
            Assert.AreEqual(d.FloatValue, 0.0);

            IXmlElement xmlClassFactory = new SimpleElement();
            xmlClassFactory.AddElement("class-factory-name").SetString("Tangosol.Run.Xml.XmlHelperTests+Dummy, Coherence.Core.Tests");
            xmlClassFactory.AddElement("method-name").SetString("CreateDummyInstance");
            o = XmlHelper.CreateInstance(xmlClassFactory, null);
            Assert.IsNotNull(o);
            Assert.IsInstanceOf(typeof(Dummy), o);
            d = o as Dummy;
            Assert.IsNotNull(d);
            Assert.AreEqual(d.FloatValue, 0.0);

            xmlClassFactory.GetElement("class-factory-name").SetString(null);
            Exception e = null;
            try
            {
                XmlHelper.CreateInstance(xmlClassFactory, null);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);

            //with params
            IXmlElement initParams = new SimpleElement("init-params");
            IXmlElement initParam = initParams.AddElement("init-param");
            initParam.AddElement("param-type").SetString("long");
            initParam.AddElement("param-value").SetString("101");
            initParam = initParams.AddElement("init-param");
            initParam.AddElement("param-type").SetString("date");
            initParam.AddElement("param-value").SetString("2/16/1992 12:15:12");
            initParam = initParams.AddElement("init-param");
            initParam.AddElement("param-type").SetString("float");
            initParam.AddElement("param-value").SetString("0.00000001");
            initParam = initParams.AddElement("init-param");
            initParam.AddElement("param-type").SetString("string");
            initParam.AddElement("param-value").SetString("test");

            xmlClass.ElementList.Add(initParams);
            o = XmlHelper.CreateInstance(xmlClass, null);
            Assert.IsNotNull(o);
            Assert.IsInstanceOf(typeof(Dummy), o);
            d = o as Dummy;
            Assert.IsNotNull(d);
            Assert.AreEqual(d.StringValue, "test");

            xmlClassFactory.GetElement("class-factory-name").SetString("Tangosol.Run.Xml.XmlHelperTests+Dummy, Coherence.Core.Tests");
            xmlClassFactory.GetElement("method-name").SetString("CreateDummyInstanceWithParams");
            xmlClassFactory.ElementList.Add(initParams);
            o = XmlHelper.CreateInstance(xmlClassFactory, null);
            Assert.IsNotNull(o);
            Assert.IsInstanceOf(typeof(Dummy), o);
            d = o as Dummy;
            Assert.IsNotNull(d);
            Assert.AreEqual(d.StringValue, "test");

            //configurable
            IXmlElement config = new SimpleElement("config");
            XmlHelper.TransformInitParams(config, initParams);

            xmlClass.GetElement("class-name").SetString("Tangosol.Run.Xml.XmlHelperTests+ConfigurableDummy, Coherence.Core.Tests");
            o = XmlHelper.CreateInstance(xmlClass, null);
            Assert.IsNotNull(o);
            Assert.IsInstanceOf(typeof(ConfigurableDummy), o);
            ConfigurableDummy cd = o as ConfigurableDummy;
            Assert.IsNotNull(cd);
            Assert.IsNotNull(cd.StringValue);
            Assert.IsNotNull(cd.Config);
            Assert.IsTrue(cd.Config.Equals(config));

            xmlClassFactory.GetElement("class-factory-name").SetString("Tangosol.Run.Xml.XmlHelperTests+ConfigurableDummy, Coherence.Core.Tests");
            xmlClassFactory.GetElement("method-name").SetString("CreateConfigurableDummyInstance");
            xmlClassFactory.ElementList.Add(initParams);
            o = XmlHelper.CreateInstance(xmlClassFactory, null);
            Assert.IsNotNull(o);
            Assert.IsInstanceOf(typeof(ConfigurableDummy), o);
            cd = o as ConfigurableDummy;
            Assert.IsNotNull(cd);
            Assert.IsNotNull(cd.StringValue);
            Assert.IsNotNull(cd.Config);
            Assert.IsTrue(cd.Config.Equals(config));

            e = null;
            xmlClassFactory.GetElement("class-factory-name").SetString("Tangosol.Run.Xml.XmlHelperTests+ConfigurableDummy1, Coherence.Core.Tests");
            try
            {
                XmlHelper.CreateInstance(xmlClassFactory, null);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);

            IXmlDocument xml = XmlHelper.LoadXml("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-util.xml");
            IXmlElement el = xml.FindElement("user-type-list/user-type/serializer");

            o = XmlHelper.CreateInstance(el, null);
            Assert.IsNotNull(o);
            Assert.IsInstanceOf(typeof(Dummy), o);
            d = o as Dummy;
            Assert.IsNotNull(d);
            Assert.AreEqual(d.FloatValue, (float) 0.00000001);
            Assert.AreEqual(d.DateTimeValue, new DateTime(1992, 2, 2, 12, 15, 12));
            Assert.AreEqual(d.LongValue, 101L);
            Assert.AreEqual(d.StringValue, "{test}");
        }

        #endregion

        #region Formatting support

        [Test]
        public void TestParseMethods()
        {
            //ParseTime
            Assert.AreEqual(300, XmlHelper.ParseTime("300"));
            Assert.AreEqual(1000, XmlHelper.ParseTime("1s", XmlHelper.UNIT_S));
            Assert.AreEqual(1000, XmlHelper.ParseTime("1s", XmlHelper.UNIT_MS));
            Assert.AreEqual(11*60*1000, XmlHelper.ParseTime("11m"));
            Assert.AreEqual(112, XmlHelper.ParseTime("112mS"));
            Assert.AreEqual(0.123 * 60 * 60 * 1000, XmlHelper.ParseTime("0.123", XmlHelper.UNIT_H));
            Assert.AreEqual((long) Math.Round(0.005*24*60*60*1000), XmlHelper.ParseTime("0.005D"));

            //ParseMemorySize
            Assert.AreEqual(300, XmlHelper.ParseMemorySize("300"));
            Assert.AreEqual(1 * 1024, XmlHelper.ParseMemorySize("1k", XmlHelper.POWER_K));
            Assert.AreEqual(1 * 1024, XmlHelper.ParseMemorySize("1K", XmlHelper.POWER_G));
            Assert.AreEqual(64 * 1024 * 1024, XmlHelper.ParseMemorySize("64Mb"));
            Assert.AreEqual(112 * 1024 * 1024, XmlHelper.ParseMemorySize("112mB"));
            Assert.AreEqual(0.5 * 1024 * 1024 * 1024, XmlHelper.ParseMemorySize("0.5", XmlHelper.POWER_G));

            Exception e = null;
            try
            {
                XmlHelper.ParseMemorySize(null, XmlHelper.POWER_G);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
            e = null;
            try
            {
                XmlHelper.ParseMemorySize("300", 1000);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
            e = null;
            try
            {
                XmlHelper.ParseMemorySize("asdf", XmlHelper.POWER_G);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(FormatException), e);

            e = null;
            try
            {
                XmlHelper.ParseTime("1111NS");
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(FormatException), e);
        }

        #endregion

        #region Object helpers

        [Test]
        public void TestObjectHelpers()
        {
            Exception e = null;

            //EqualsValue
            SimpleValue sv1 = new SimpleValue("");
            SimpleValue sv2 = new SimpleValue(Binary.NO_BINARY);
            Assert.IsTrue(XmlHelper.EqualsValue(sv1, sv2)); //both are empty
            sv1.SetString("True");
            Assert.IsFalse(XmlHelper.EqualsValue(sv1, sv2));
            sv2.SetBoolean(true);
            Assert.IsTrue(XmlHelper.EqualsValue(sv1, sv2));
            try
            {
                XmlHelper.EqualsValue(null, sv2);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
            sv2.SetString(null);
            try
            {
                XmlHelper.EqualsValue(sv1, sv2);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
            sv2.SetBoolean(true);

            //EqualsElement
            SimpleElement se1 = new SimpleElement();
            SimpleElement se2 = new SimpleElement("name", "value");
            se2.Comment = "comment";
            Assert.IsFalse(XmlHelper.EqualsElement(se1, se2));
            se1.Name = se2.Name;
            Assert.IsFalse(XmlHelper.EqualsElement(se1, se2));
            se1.SetString(se2.GetString());
            Assert.IsFalse(XmlHelper.EqualsElement(se1, se2));
            se1.Comment = se2.Comment;
            Assert.IsTrue(XmlHelper.EqualsElement(se1, se2));
            se1.AddElement("el1");
            Assert.IsFalse(XmlHelper.EqualsElement(se1, se2));
            se2.AddElement("el1");
            Assert.IsTrue(XmlHelper.EqualsElement(se1, se2));
            se1.AddAttribute("attr1");
            Assert.IsFalse(XmlHelper.EqualsElement(se1, se2));
            se2.AddAttribute("attr1");
            Assert.IsTrue(XmlHelper.EqualsElement(se1, se2));
            try
            {
                XmlHelper.EqualsElement(null, se2);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);

            //HashValue
            SimpleValue sv = new SimpleValue();
            Assert.IsNull(sv.Value);
            Assert.AreEqual(0, XmlHelper.HashValue(sv));
            sv.SetString("test");
            Assert.AreEqual("test".GetHashCode(), XmlHelper.HashValue(sv));
            sv.SetDecimal(decimal.MinusOne);
            Assert.AreEqual(XmlHelper.Convert(decimal.MinusOne, XmlValueType.String).GetHashCode(),
                            XmlHelper.HashValue(sv));
            Assert.AreEqual(XmlHelper.HashValue(sv1), XmlHelper.HashValue(sv2));

            //HashElement
            Assert.AreEqual(XmlHelper.HashElement(se1), XmlHelper.HashElement(se2));
            IXmlElement el1 = se1.GetElement("el1");
            el1.SetString("new value");
            Assert.AreNotEqual(XmlHelper.HashElement(se1), XmlHelper.HashElement(se2));
        }

        #endregion

        #region Conversions

        [Test]
        public void TestConvert()
        {
            object o = null;
            Assert.IsNull(XmlHelper.Convert(o, XmlValueType.Boolean));

            //boolean
            o = true;
            Assert.AreEqual(o, XmlHelper.Convert(o, XmlValueType.Boolean));
            Assert.AreEqual(true, XmlHelper.Convert("true", XmlValueType.Boolean));
            Assert.AreEqual(true, XmlHelper.Convert("TRUE", XmlValueType.Boolean));
            Assert.AreEqual(true, XmlHelper.Convert("yes", XmlValueType.Boolean));
            Assert.AreEqual(true, XmlHelper.Convert("Yes", XmlValueType.Boolean));
            Assert.AreEqual(true, XmlHelper.Convert(1, XmlValueType.Boolean));
            Assert.AreEqual(false, XmlHelper.Convert("false", XmlValueType.Boolean));
            Assert.AreEqual(false, XmlHelper.Convert("False", XmlValueType.Boolean));
            Assert.AreEqual(false, XmlHelper.Convert("No", XmlValueType.Boolean));
            Assert.AreEqual(false, XmlHelper.Convert("no", XmlValueType.Boolean));
            Assert.AreEqual(false, XmlHelper.Convert(0, XmlValueType.Boolean));

            Assert.IsNull(XmlHelper.Convert("", XmlValueType.Boolean));
            Assert.IsNull(XmlHelper.Convert(4, XmlValueType.Boolean));
            Assert.IsNull(XmlHelper.Convert("isnotbool", XmlValueType.Boolean));
            Assert.IsNull(XmlHelper.Convert("12", XmlValueType.Boolean));
            Assert.IsNull(XmlHelper.Convert("002", XmlValueType.Boolean));

            //integer
            o = 1;
            Assert.AreEqual(o, XmlHelper.Convert(o, XmlValueType.Integer));
            Assert.AreEqual(45, XmlHelper.Convert("45", XmlValueType.Integer));

            Assert.IsNull(XmlHelper.Convert(true, XmlValueType.Integer));
            Assert.IsNull(XmlHelper.Convert("isnotint", XmlValueType.Integer));

            //long
            o = long.MaxValue;
            Assert.AreEqual(o, XmlHelper.Convert(o, XmlValueType.Long));
            Assert.AreEqual(45, XmlHelper.Convert("45", XmlValueType.Long));

            Assert.IsNull(XmlHelper.Convert(true, XmlValueType.Long));
            Assert.IsNull(XmlHelper.Convert("isnotlong", XmlValueType.Long));

            //double
            o = double.MaxValue;
            Assert.AreEqual(o, XmlHelper.Convert(o, XmlValueType.Double));
            Assert.AreEqual(45d, XmlHelper.Convert("45", XmlValueType.Double));
            Assert.IsNull(XmlHelper.Convert("isnotlong", XmlValueType.Double));

            //decimal
            o = decimal.MinusOne;
            Assert.AreEqual(o, XmlHelper.Convert(o, XmlValueType.Decimal));
            Assert.AreEqual(new decimal(45), XmlHelper.Convert("45", XmlValueType.Decimal));
            Assert.IsNull(XmlHelper.Convert("isnotlong", XmlValueType.Decimal));

            //string
            o = "testing";
            Assert.AreEqual(o, XmlHelper.Convert(o, XmlValueType.String));
            Assert.AreEqual("4", XmlHelper.Convert(4, XmlValueType.String));
            Binary b = new Binary(new byte[] { 1, 2, 3, 4, 5 });
            Assert.AreEqual(Convert.ToBase64String(b.ToByteArray()), XmlHelper.Convert(b, XmlValueType.String));

            //binary
            o = b;
            Assert.AreEqual(o, XmlHelper.Convert(o, XmlValueType.Binary));
            Assert.AreEqual(Binary.NO_BINARY, XmlHelper.Convert("", XmlValueType.Binary));
            Assert.AreEqual(b, XmlHelper.Convert("AQIDBAU=", XmlValueType.Binary));
            Assert.IsNull(XmlHelper.Convert("notvalidbinary", XmlValueType.Binary));

            //datetime
            o = DateTime.Now;
            Assert.AreEqual(o, XmlHelper.Convert(o, XmlValueType.DateTime));
            DateTime dt = (DateTime) XmlHelper.Convert(o.ToString(), XmlValueType.DateTime);
            Assert.AreEqual(DateTime.Parse(o.ToString()), dt);
            Assert.IsNull(XmlHelper.Convert(b, XmlValueType.DateTime));
        }

        #endregion

        #region Test classes

        private class Dummy
        {
            private long m_l;
            private DateTime m_d;
            private float m_f;
            private string m_s;

            public Dummy()
            {}

            public Dummy(long l, DateTime d, float f, string s)
            {
                m_l = l;
                m_d = d;
                m_f = f;
                m_s = s;
            }

            public static Dummy CreateDummyInstance()
            {
                return new Dummy();
            }

            public static Dummy CreateDummyInstanceWithParams(long l, DateTime d, float f, string s)
            {
                return new Dummy(l, d, f, s);
            }

            public long LongValue
            {
                get { return m_l; }
            }

            public DateTime DateTimeValue
            {
                get { return m_d; }
            }

            public float FloatValue
            {
                get { return m_f; }
            }

            public string StringValue
            {
                get { return m_s; }
            }
        }

        private class ConfigurableDummy : Dummy, IXmlConfigurable
        {
            public ConfigurableDummy()
            {}

            public ConfigurableDummy(long l, DateTime d, float f, string s) : base(l,d,f,s)
            {}

            public static ConfigurableDummy CreateConfigurableDummyInstance(long l, DateTime d, float f, string s)
            {
                return new ConfigurableDummy(l, d, f, s);
            }

            private IXmlElement m_config;

            public IXmlElement Config
            {
                get { return m_config; }
                set { m_config = value; }
            }
        }

        private class SimpleResolver : XmlHelper.IParameterResolver
        {
            public object ResolveParameter(string type, string value)
            {
                return value.Replace("{", "").Replace("}", "");
            }
        }

        private class UnresolvedResolver : XmlHelper.IParameterResolver
        {
            public object ResolveParameter(string type, string value)
            {
                return XmlHelper.UNRESOLVED;
            }
        }

        #endregion
    }
}
