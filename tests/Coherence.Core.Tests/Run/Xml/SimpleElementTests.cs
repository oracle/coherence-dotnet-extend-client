/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

using NUnit.Framework;

using Tangosol.IO;
using Tangosol.IO.Pof;

namespace Tangosol.Run.Xml
{
    [TestFixture]
    public class SimpleElementTests
    {
        [Test]
        public void TestConstructors()
        {
            SimpleElement se = new SimpleElement("name");
            Assert.IsNotNull(se);
            Assert.IsNull(se.Value);
            Assert.IsNotNull(se.Name);
            Assert.AreEqual("name", se.Name);

            se = new SimpleElement("name", "value");
            Assert.IsNotNull(se);
            Assert.IsNotNull(se.Value);
            Assert.IsNotNull(se.Name);
            Assert.AreEqual("name", se.Name);
            Assert.AreEqual(se.Value, "value");
            Assert.AreEqual(se.GetString(), "value");
        }

        [Test]
        public void TestProperties()
        {
            SimpleElement se = new SimpleElement("name", 5);
            Assert.IsNotNull(se);
            Assert.AreEqual(se.Value, 5);
            Assert.AreEqual(se.GetInt(), 5);
            Assert.IsFalse(se.IsAttribute);
            Assert.IsTrue(se.IsContent);
            Assert.IsFalse(se.IsEmpty);
            Assert.IsTrue(se.IsMutable);
            Assert.AreEqual(se.Name, "name");

            Exception e = null;
            try
            {
                se.Name = "?name";
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof (ArgumentException), e);
            Assert.AreEqual(se.Name, "name");
            se.Name = "newname";
            Assert.AreEqual(se.Name, "newname");
            e = null;
            try
            {
                se.GetSafeElement("el").Name = "elname";
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(InvalidOperationException), e);

            Assert.IsNull(se.Parent);
            Assert.IsNotNull(se.Root);
            Assert.AreEqual(se, se.Root);
            Assert.AreEqual("/newname", se.AbsolutePath);
            SimpleElement root = new SimpleElement("root");
            se.Parent = root;
            Assert.IsNotNull(se.Parent);
            Assert.AreEqual(root, se.Parent);
            Assert.IsNotNull(se.Root);
            Assert.AreEqual(root, se.Root);
            Assert.AreEqual("/root/newname", se.AbsolutePath);

            Assert.IsNull(se.Comment);
            e = null;
            try
            {
                se.Comment = "new comment --";
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof (ArgumentException), e);
            Assert.IsNull(se.Comment);
            se.Comment = "new comment";
            Assert.AreEqual(se.Comment, "new comment");
        }

        [Test]
        public void TestSerialization()
        {
            ConfigurablePofContext ctx = new ConfigurablePofContext("assembly://Coherence.Core.Tests/Tangosol.Resources/s4hc-test-config.xml");
            Assert.IsNotNull(ctx);

            SimpleElement se = new SimpleElement("name", 5);
            se.Comment = "comment";
            se.AddElement("child1");
            se.AddElement("child2");
            se.AddAttribute("attr1");
            se.AddAttribute("attr2");
            se.AddAttribute("attr3");

            Stream stream = new MemoryStream();
            ctx.Serialize(new DataWriter(stream), se);

            stream.Position = 0;
            SimpleElement sed = (SimpleElement) ctx.Deserialize(new DataReader(stream));
            Assert.AreEqual(se, sed);
        }

        [Test]
        public void TestObjectMethods()
        {
            //Equals
            SimpleElement se1 = new SimpleElement("name");
            SimpleElement se2 = new SimpleElement("name", "value");
            SimpleValue sv = new SimpleValue("value");
            Assert.IsFalse(se1.Equals(sv));
            Assert.IsFalse(se1.Equals(se2));
            se2.SetString(se1.GetString());
            Assert.IsTrue(se1.Equals(se2));
            se1.AddElement("child1");
            se1.AddElement("child2");
            Assert.IsFalse(se1.Equals(se2));
            se2.AddElement("child1");
            se2.AddElement("child2");
            Assert.IsTrue(se1.Equals(se2));
            se1.AddAttribute("attr1");
            Assert.IsFalse(se1.Equals(se2));
            se2.AddAttribute("attr1");
            Assert.IsTrue(se1.Equals(se2));

            //GetHashCode
            Assert.AreEqual(XmlHelper.HashElement(se1), se1.GetHashCode());

            //Clone
            SimpleElement se1c = (SimpleElement) se1.Clone();
            Assert.AreEqual(se1c, se1);
            Assert.AreNotSame(se1, se1c);
            se1.Comment = "comment";
            Assert.AreNotEqual(se1c, se1);
        }

        [Test]
        public void TestElementList()
        {
            SimpleElement parent = new SimpleElement("parent", "value");
            Assert.IsTrue(parent.IsMutable); //so, element list is mutable as well

            IList elements = parent.ElementList;
            Assert.IsNotNull(elements);
            Assert.AreEqual(elements.Count, 0);

            //adding something that is not IXmlElement will fail
            Exception e = null;
            try
            {
                elements.Add("something");
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(InvalidCastException), e);
            Assert.AreEqual(elements.Count, 0);

            IXmlElement child0 = parent.AddElement("child0");
            IXmlElement child1 = new SimpleElement("child1");
            IXmlElement child2 = new SimpleElement("child2");
            IXmlElement child3 = new SimpleElement("child3");
            IXmlElement child4 = new SimpleElement("child5");
            IXmlElement child5 = new SimpleElement("child6");

            Assert.IsNotNull(child0);
            Assert.AreEqual(child0.Parent, parent);
            Assert.AreEqual(elements.Count, 1);
            Assert.AreEqual(elements[0], child0);

            child1.Comment = "comment";
            child1.AddAttribute("attr").SetString("attrValue");
            child1.Parent = child0;
            elements.Add(child1);
            Assert.AreEqual(elements.Count, 2);
            Assert.AreNotEqual(child1.Parent, ((SimpleElement) elements[1]).Parent);
            Assert.AreEqual(parent, ((SimpleElement) elements[1]).Parent);
            Assert.AreEqual(elements[1], child1);
            Assert.AreNotSame(elements[1], child1);

            Assert.IsInstanceOf(typeof(ArrayList), elements);
            ArrayList elementList = (ArrayList) elements;

            IList l = new ArrayList();
            l.Add(child2);
            elementList.AddRange(l);
            Assert.AreEqual(elementList.Count, 3);
            Assert.AreEqual(child2.Parent, parent);
            Assert.AreEqual(elementList[2], child2);
            elementList.Insert(3, child3);
            Assert.AreEqual(elementList.Count, 4);
            Assert.AreEqual(child3.Parent, parent);
            Assert.AreEqual(elementList[3], child3);

            l = new ArrayList();
            l.Add(child4);
            l.Add(child5);
            elementList.InsertRange(4, l);
            Assert.AreEqual(elementList.Count, 6);
            Assert.AreEqual(elementList[4], child4);
            Assert.AreEqual(elementList[5], child5);

            Assert.IsTrue(elementList.Contains(child5));
            elementList.Remove(child5);
            Assert.AreEqual(elementList.Count, 5);
            elementList.RemoveRange(3, 2);
            Assert.AreEqual(elementList.Count, 3);

            Assert.IsFalse(elementList.Contains(child4));
            Assert.IsFalse(elementList.Contains(child5));
            elementList.SetRange(1, l);
            Assert.AreEqual(elementList.Count, 3);
            Assert.AreEqual(elementList[0], child0);
            Assert.AreEqual(elementList[1], child4);
            Assert.AreEqual(elementList[2], child5);
            elementList[0] = child3;

            Assert.AreEqual(elementList[0], child3); //3,4,5
            elementList.Reverse();
            Assert.AreEqual(elementList[2], child3); //5,4,3
            Assert.AreEqual(elementList[0], child5); //5,4,3
            elementList.Reverse(0, 2);
            Assert.AreEqual(elementList[2], child3); //4,5,3
            Assert.AreEqual(elementList[0], child4); //4,5,3

            elementList.Sort(new SimpleElementComparer());
            Assert.AreEqual(elementList[0], child3); //3,4,5
            Assert.AreEqual(elementList[2], child5); //3,4,5
            elementList.Reverse();
            elementList.Sort(0, 2, new SimpleElementComparer());
            Assert.AreEqual(elementList[0], child4); //4,5,3
            Assert.AreEqual(elementList[2], child3); //4,5,3
            

            elementList.Clear();
            Assert.AreEqual(elementList.Count, 0);

            e = null;
            try
            {
                IXmlElement immutable = parent.GetSafeElement("immutable");
                immutable.AddElement("child");
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(InvalidOperationException), e);

            SimpleElement parent2 = new SimpleElement();
            parent.AddElement("child1");
            Assert.IsFalse(parent.ElementList.Equals(parent2.ElementList));
            Assert.IsFalse(parent.ElementList.Equals("test"));
            parent2.AddElement("child1");
            Assert.IsTrue(parent.ElementList.Equals(parent2.ElementList));
            parent.AddElement("child2");
            parent2.AddElement("child2");
            Assert.AreEqual(parent.ElementList, parent2.ElementList);
            parent2.GetElement("child2").AddAttribute("attr");
            Assert.AreNotEqual(parent.ElementList, parent2.ElementList);
        }

        [Test]
        public void TestElementMethods()
        {
            //GetElement
            SimpleElement se = new SimpleElement();
            Assert.IsNull(se.GetElement("child1"));
            IXmlElement child1 = se.AddElement("child1");
            Assert.IsNotNull(se.GetElement("child1"));

            //FindElement
            Assert.IsNull(se.FindElement("/child2"));
            Assert.AreEqual(child1, se.FindElement("/child1"));

            //EnsureElement
            Assert.AreEqual(se.ElementList.Count, 1);
            Assert.IsNotNull(se.EnsureElement("/child2"));
            IXmlElement child2 = se.GetElement("child2");
            Assert.IsTrue(child2.IsMutable);
            Assert.AreEqual(se.ElementList.Count, 2); //element is added

            //GetSafeElement
            Assert.AreEqual(se.ElementList.Count, 2);
            Assert.AreEqual(se, se.GetSafeElement(""));
            IXmlElement child3 = se.GetSafeElement("/child3");
            Assert.IsNotNull(child3);
            Assert.IsFalse(child3.IsMutable);
            Assert.AreEqual(se.ElementList.Count, 2); //element is not added
            IXmlElement child4 = se.GetSafeElement("/child3/../child4");
            Assert.IsNotNull(child4);
            Exception e = null;
            try
            {
                se.GetSafeElement(null);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentNullException), e);
            e = null;
            try
            {
                se.GetSafeElement("..");
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
        }

        [Test]
        public void TestElementsEnumerator()
        {
            SimpleElement root = new SimpleElement();
            IEnumerator child1Enum = root.GetElements("child1");
            IXmlElement el1 = null;

            Exception e = null;
            try
            {
                el1 = (IXmlElement) child1Enum.Current; //without MoveNext()
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNull(el1);
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(InvalidOperationException), e);

            el1 = root.AddElement("child1");
            root.AddElement("child2");
            root.AddElement("child3");
            IXmlElement el2 = root.AddElement("child1");
            root.AddElement("child2");

            child1Enum = root.GetElements("child1");

            Assert.IsTrue(child1Enum.MoveNext());
            Assert.AreEqual(child1Enum.Current, el1);
            Assert.IsTrue(child1Enum.MoveNext());
            Assert.IsTrue(child1Enum.MoveNext()); //will do nothing
            Assert.AreEqual(child1Enum.Current, el2);
            Assert.IsFalse(child1Enum.MoveNext());
            child1Enum.Reset();
            Assert.IsTrue(child1Enum.MoveNext());
        }

        [Test]
        public void TestAttributes()
        {
            SimpleElement se = new SimpleElement();
            IDictionary attrs = se.Attributes;
            Assert.IsNotNull(attrs);
            Assert.AreEqual(attrs.Count, 0);
            Assert.IsFalse(attrs.IsReadOnly);
            Assert.IsFalse(attrs.IsFixedSize);
            Assert.IsNotNull(attrs.SyncRoot);
            Assert.IsFalse(attrs.IsSynchronized);

            SimpleValue sv = new SimpleValue("value");

            Exception e = null;
            try
            {
                attrs.Add(3, sv);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
            Assert.AreEqual(attrs.Count, 0);

            e = null;
            try
            {
                attrs.Add("?key1", sv);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
            Assert.AreEqual(attrs.Count, 0);

            e = null;
            try
            {
                attrs.Add("key1", 3);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
            Assert.AreEqual(attrs.Count, 0);

            Assert.IsNull(sv.Parent);
            Assert.IsFalse(sv.IsAttribute);
            attrs.Add("key1", sv);
            Assert.AreEqual(attrs.Count, 1);
            Assert.IsTrue(attrs.Contains("key1"));
            Assert.AreEqual(attrs.Keys.Count, 1);
            Assert.AreEqual(attrs.Values.Count, 1);

            IXmlValue v = (IXmlValue) attrs["key1"];
            Assert.AreEqual(se, v.Parent);
            Assert.IsTrue(v.IsAttribute);

            SimpleElement se2 = (SimpleElement) se.Clone();
            Assert.IsTrue(se.Attributes.Equals(se2.Attributes));
            se.Attributes.Add("key2", v.Clone());
            se2.Attributes.Add("key2", new SimpleValue(1000, true));
            Assert.IsFalse(se.Attributes.Equals(se2.Attributes));
            se.Attributes.Add("key3", v.Clone());
            Assert.IsFalse(se.Attributes.Equals(se2.Attributes));
            Assert.IsFalse(se.Attributes.Equals(se2));

            Assert.AreEqual(attrs.Count, 3);
            Assert.IsInstanceOf(typeof(ICloneable), attrs);
            ICloneable cloneableAttrs = attrs as ICloneable;
            Assert.IsNotNull(cloneableAttrs);
            IDictionary clones = (IDictionary) cloneableAttrs.Clone();
            Assert.AreEqual(clones.Count, 3);
            Assert.AreEqual(clones, attrs);

            attrs.Clear();
            Assert.AreEqual(attrs.Count, 0);
        }

        [Test]
        public void TestAttributeMethods()
        {
            SimpleElement se = new SimpleElement();
            Assert.AreEqual(se.Attributes.Count, 0);

            IXmlValue attr1 = se.AddAttribute("attr1");
            attr1.SetString("value1");
            Assert.AreEqual(se.Attributes.Count, 1);

            Assert.IsNull(se.GetAttribute("attr2"));
            Assert.AreEqual(attr1, se.GetAttribute("attr1"));
            Exception e = null;
            try
            {
                se.GetAttribute(null);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);

            se.AddAttribute("attr2");
            Assert.AreEqual(se.Attributes.Count, 2);
            se.SetAttribute("attr2", null);
            Assert.AreEqual(se.Attributes.Count, 1);
            se.SetAttribute("attr2", new SimpleValue("value2"));
            Assert.AreEqual(se.Attributes.Count, 2);

            Assert.IsNull(se.GetAttribute("attr3"));
            IXmlValue attr3 = se.GetSafeAttribute("attr3");
            Assert.IsNotNull(attr3);
            Assert.IsFalse(attr3.IsMutable);
        }

        [Test]
        public void TestWriteMethods()
        {
            //WriteValue
            TextWriter writer = new StringWriter();
            SimpleElement se = new SimpleElement("root", "some\ntext");
            se.WriteValue(writer, true);
            Assert.AreEqual(se.GetString(), writer.ToString());

            writer = new StringWriter();
            IndentingWriter indentedWriter = new IndentingWriter(writer, 2);
            se.WriteValue(indentedWriter, true);
            Assert.AreEqual(se.GetString(), writer.ToString());

            //WriteXml
            se = new SimpleElement("root");
            writer = new StringWriter();
            se.WriteXml(writer, true);
            Assert.AreEqual("<root/>", writer.ToString());

            se.AddAttribute("attr1").SetString("value1");
            writer = new StringWriter();
            se.WriteXml(writer, true);
            Assert.AreEqual("<root attr1='value1'/>", writer.ToString());

            se.Comment = "comment text";
            se.SetString("content");
            writer = new StringWriter();
            se.WriteXml(writer, true);
            Assert.AreEqual("<root attr1='value1'><!--" + writer.NewLine + "comment text" + writer.NewLine + "-->content</root>", writer.ToString());
            writer = new StringWriter();
            se.WriteXml(writer, false);
            Assert.AreEqual("<root attr1='value1'><!--comment text-->content</root>", writer.ToString());

            se.AddElement("child").SetString("child content");
            writer = new StringWriter();
            se.WriteXml(writer, true);
            string result = "<root attr1='value1'>" + writer.NewLine + "  <!--" + writer.NewLine + "  comment text" +
                writer.NewLine + "  -->" + writer.NewLine + "content" + writer.NewLine + "  <child>child content</child>" +
                writer.NewLine + "</root>";
            Assert.AreEqual(result, writer.ToString());
            Assert.AreEqual(result, se.ToString());

            writer = new StringWriter();
            se.WriteXml(writer, false);
            result = "<root attr1='value1'><!--comment text-->content<child>child content</child></root>";
            Assert.AreEqual(result, writer.ToString());
        }

        private class SimpleElementComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                if (x == null || y == null)
                {
                    throw new ArgumentNullException();
                }
                if (!(x is SimpleElement) || !(y is SimpleElement))
                {
                    throw new ArgumentException();
                }
                SimpleElement se1 = (SimpleElement) x;
                SimpleElement se2 = (SimpleElement) y;
                return se1.Name.CompareTo(se2.Name);
            }
        }
    }
}
