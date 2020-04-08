/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Util;

namespace Tangosol.Run.Xml
{
    /// <summary>
    /// A simple implementation of the IXmlElement interface.
    /// </summary>
    /// <remarks>
    /// Protected methods are provided to support inheriting classes.
    /// </remarks>
    /// <author>Cameron Purdy  2000.10.20</author>
    /// <author>Ana Cikic  2009.08.27</author>
    public class SimpleElement : SimpleValue, IXmlElement
    {
        #region Constructors

        /// <summary>
        /// Construct an empty SimpleElement.
        /// </summary>
        public SimpleElement()
        {}

        /// <summary>
        /// Construct a SimpleElement.
        /// </summary>
        /// <param name="name">
        /// The name of the element.
        /// </param>
        public SimpleElement(string name) : this(name, null)
        {}

        /// <summary>
        /// Construct a SimpleElement.
        /// </summary>
        /// <param name="name">
        /// The name of the element.
        /// </param>
        /// <param name="value">
        /// An initial value for this element.
        /// </param>
        public SimpleElement(string name, object value) : base(value)
        {
            Name = name;
        }
        
        #endregion

        #region IXmlElement members

        /// <summary>
        /// Get or set the name of the element.
        /// </summary>
        /// <remarks>
        /// Setter is intended primarily to be utilized to configure a newly
        /// instantiated element before adding it as a child element to
        /// another element.<br/>
        /// Implementations of this interface that support read-only
        /// documents are expected to throw <b>InvalidOperationException</b>
        /// from this method if the document (or this element) is in a
        /// read-only state.<br/>
        /// If this IXmlElement has a parent IXmlElement, then the
        /// implementation of this interface is permitted to throw
        /// <b>InvalidOperationException</b> from this method. This results
        /// from typical document implementations in which the name of an
        /// element that is a child of another element is immutable; the W3C
        /// DOM interfaces are one example.
        /// </remarks>
        /// <value>
        /// The element name.
        /// </value>
        public virtual string Name
        {
            get { return m_name; }
            set
            {
                if (!IsNameMutable)
                {
                    throw new InvalidOperationException("the element cannot be renamed");
                }

                if (!XmlHelper.IsNameValid(value))
                {
                    throw new ArgumentException("illegal name \"" + value + "\"; see XML 1.0 2ed section 2.3 [5]");
                }

                m_name = value;
            }
        }

        /// <summary>
        /// Get the root element.
        /// </summary>
        /// <remarks>
        /// This is a convenience property. Parent element is retrived using
        /// <see cref="IXmlValue.Parent"/>.
        /// </remarks>
        /// <value>
        /// The root element for this element.
        /// </value>
        public virtual IXmlElement Root
        {
            get
            {
                IXmlElement parent = Parent;
                return parent == null ? this : parent.Root;
            }
        }

        /// <summary>
        /// Get the '/'-delimited path of the element starting from the root
        /// element.
        /// </summary>
        /// <remarks>
        /// This is a convenience property. Elements are retrieved by simple
        /// name using <see cref="Name"/>.
        /// </remarks>
        /// <returns>
        /// The element path.
        /// </returns>
        public virtual string AbsolutePath
        {
            get { return XmlHelper.GetAbsolutePath(this); }
        }

        /// <summary>
        /// Get the list of all child elements.
        /// </summary>
        /// <remarks>
        /// The contents of the list implement the <see cref="IXmlValue"/>
        /// interface. If this IXmlElement is mutable, then the list returned
        /// from this method is expected to be mutable as well.<br/>
        /// An element should be fully configured before it is added to the
        /// list:
        /// <list type="bullet">
        /// <item>The IList implementation is permitted (and most
        /// implementations are expected) to instantiate its own copy of any
        /// IXmlElement objects added to it.</item>
        /// <item>Certain properties of an element (such as
        /// <see cref="Name"/>) may not be settable once the element has been
        /// added.</item>
        /// </list>
        /// </remarks>
        /// <value>
        /// An <b>IList</b> containing all elements of this IXmlElement.
        /// </value>
        public virtual IList ElementList
        {
            get
            {
                IList list = m_listChildren;
                if (list == null)
                {
                    m_listChildren = list = InstantiateElementList();
                }
                return list;
            }
        }

        /// <summary>
        /// Get a child element.
        /// </summary>
        /// <remarks>
        /// This is a convenience method. Elements are accessed and
        /// manipulated via the list returned from
        /// <see cref="ElementList"/>.<br/>
        /// If multiple child elements exist that have the specified name,
        /// then the behavior of this method is undefined, and it is
        /// permitted to return any one of the matching elements, to return
        /// <c>null</c>, or to throw an arbitrary runtime exception.
        /// </remarks>
        /// <param name="name">
        /// The name of child element.
        /// </param>
        /// <returns>
        /// The specified element as an object implementing IXmlElement, or
        /// <c>null</c> if the specified child element does not exist.
        /// </returns>
        public virtual IXmlElement GetElement(string name)
        {
            return XmlHelper.GetElement(this, name);
        }

        /// <summary>
        /// Get an enumerator of child elements that have a specific name.
        /// </summary>
        /// <remarks>
        /// This is a convenience method. Elements are accessed and
        /// manipulated via the list returned from
        /// <see cref="ElementList"/>.
        /// </remarks>
        /// <param name="name">
        /// The name of child elements.
        /// </param>
        /// <returns>
        /// An enumerator containing all child elements of the specified
        /// name.
        /// </returns>
        public virtual IEnumerator GetElements(string name)
        {
            return new SimpleElementEnumerator(this, name);
        }

        /// <summary>
        /// Create a new element and add it as a child element to this
        /// element.
        /// </summary>
        /// <remarks>
        /// This is a convenience method. Elements are accessed and
        /// manipulated via the list returned from
        /// <see cref="ElementList"/>.
        /// </remarks>
        /// <param name="name">
        /// The name for the new element.
        /// </param>
        /// <returns>
        /// The new IXmlElement object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the name is <c>null</c> or if the name is not a legal XML tag
        /// name.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If this element is immutable or otherwise can not add a child
        /// element.
        /// </exception>
        public virtual IXmlElement AddElement(string name)
        {
            IXmlElement element = InstantiateElement(name, null);
            ElementList.Add(element);
            return element;
        }

        /// <summary>
        /// Find a child element with the specified '/'-delimited path.
        /// </summary>
        /// <remarks>
        /// This is based on a subset of the XPath specification, supporting:
        /// <list type="bullet">
        /// <item>Leading '/' to specify root</item>
        /// <item>Use of '/' as a path delimiter</item>
        /// <item>Use of '..' to specify parent</item>
        /// </list>
        /// This is a convenience method. Elements are accessed and
        /// manipulated via the list returned from
        /// <see cref="ElementList"/>.<br/>
        /// If multiple child elements exist that have the specified name,
        /// then the behavior of this method is undefined, and it is
        /// permitted to return any one of the matching elements, to return
        /// <c>null</c>, or to throw an arbitrary runtime exception.
        /// </remarks>
        /// <param name="path">
        /// Element path.
        /// </param>
        /// <returns>
        /// The specified element as an object implementing IXmlElement, or
        /// <c>null</c> if the specified child element does not exist.
        /// </returns>
        public virtual IXmlElement FindElement(string path)
        {
            return XmlHelper.FindElement(this, path);
        }

        /// <summary>
        /// Return the specified child element using the same path notation
        /// as supported by <see cref="FindElement(String)"/>, but return a
        /// read-only element if the specified element does not exist.
        /// </summary>
        /// <remarks>
        /// <b>This method never returns <c>null</c>.</b><br/>
        /// This is a convenience method. Elements are accessed and
        /// manipulated via the list returned from
        /// <see cref="ElementList"/>.<br/>
        /// If multiple child elements exist that have the specified name,
        /// then the behavior of this method is undefined, and it is
        /// permitted to return any one of the matching elements, to return
        /// <c>null</c>, or to throw an arbitrary runtime exception.
        /// </remarks>
        /// <param name="path">
        /// Element path.
        /// </param>
        /// <returns>
        /// The specified element (never <c>null</c>) as an object
        /// implementing IXmlElement for read-only use.
        /// </returns>
        public virtual IXmlElement GetSafeElement(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (path.Length == 0)
            {
                return this;
            }

            if (path.StartsWith("/"))
            {
                return Root.GetSafeElement(path.Substring(1));
            }

            // get the first name from the path
            int of = path.IndexOf('/');
            string name;
            string remain;
            if (of < 0)
            {
                name   = path;
                remain = null;
            }
            else
            {
                name   = path.Substring(0, of);
                remain = path.Substring(of + 1);
            }

            // check if going "up" (..) or "down" (child name)
            IXmlElement element;
            if (name.Equals(".."))
            {
                element = Parent;
                if (element == null)
                {
                    throw new ArgumentException("Invalid path " + path);
                }
            }
            else
            {
                element = GetElement(name);
                if (element == null)    
                {
                    // create a temporary "safe" element (read-only)
                    element = InstantiateElement(name, null);

                    // parent (this) does not know its new safe child (element)
                    // because this is a "read-only" operation; however, the child
                    // does know its parent so it can answer pathed questions etc.
                    element.Parent = this;

                    // child is marked read-only if it supports it
                    if (element is SimpleElement)
                    {
                        ((SimpleElement) element).IsMutable = false;
                    }
                }
            }

            return remain == null ? element : element.GetSafeElement(remain);
        }

        /// <summary>
        /// Ensure that a child element exists.
        /// </summary>
        /// <remarks>
        /// This is a convenience method. It combines the functionality of
        /// <see cref="FindElement(String)"/> and
        /// <see cref="AddElement(String)"/>. If any part of the path does
        /// not exist create new child elements to match the path.
        /// </remarks>
        /// <param name="path">
        /// Element path.
        /// </param>
        /// <returns>
        /// The existing or new IXmlElement object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the name is <c>null</c> or if any part of the path is not a
        /// legal XML tag name.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If any element in the path is immutable or otherwise can not add
        /// a child element.
        /// </exception>
        public virtual IXmlElement EnsureElement(string path)
        {
            return XmlHelper.EnsureElement(this, path);
        }

        /// <summary>
        /// Get the dictionary of all attributes.
        /// </summary>
        /// <remarks>
        /// The dictionary is keyed by attribute names. The corresponding
        /// values are non-null objects that implement the <b>IXmlValue</b>
        /// interface.
        /// </remarks>
        /// <value>
        /// A <b>IDictionary</b> containing all attributes of this
        /// IXmlElement; the return value will never be <c>null</c>, although
        /// it may be an empty dictionary.
        /// </value>
        public virtual IDictionary Attributes 
        { 
            get
            {
                IDictionary attrs = m_attributes;
                if (attrs == null)
                {
                    m_attributes = attrs = InstantiateAttributes();
                }
                return attrs;
            } 
        }

        /// <summary>
        /// Get an attribute value.
        /// </summary>
        /// <remarks>
        /// This is a convenience method. Attributes are accessed and
        /// manipulated via the dictionary returned from
        /// <see cref="Attributes"/>.
        /// </remarks>
        /// <param name="name">
        /// The name of the attribute.
        /// </param>
        /// <returns>
        /// The value of the specified attribute, or <c>null</c> if the
        /// attribute does not exist.
        /// </returns>
        public virtual IXmlValue GetAttribute(string name)
        {
            if (name == null || !XmlHelper.IsNameValid(name))
            {
                throw new ArgumentException("Name is null or is not valid");
            }

            return (IXmlValue) Attributes[name];
        }

        /// <summary>
        /// Set an attribute value.
        /// </summary>
        /// <remarks>
        /// If the attribute does not already exist, and the new value is
        /// non-null, then the attribute is added and its value is set to the
        /// passed value. If the attribute does exist, and the new value is
        /// non-null, then the attribute's value is updated to the passed
        /// value. If the attribute does exist, but the new value is
        /// <c>null</c>, then the attribute and its corresponding value are
        /// removed.<br/>
        /// This is a convenience method. Attributes are accessed and
        /// manipulated via the dictionary returned from
        /// <see cref="Attributes"/>.
        /// </remarks>
        /// <param name="name">
        /// The name of the attribute.
        /// </param>
        /// <param name="value">
        /// The new value for the attribute; <c>null</c> indicates that the
        /// attribute should be removed.
        /// </param>
        public virtual void SetAttribute(string name, IXmlValue value)
        {
            IDictionary dictionary = Attributes;
            if (value == null)
            {
                dictionary.Remove(name);
            }
            else
            {
                dictionary[name] = value;
            }
        }

        /// <summary>
        /// Provides a means to add a new attribute value.
        /// </summary>
        /// <remarks>
        /// If the attribute of the same name already exists, it is returned,
        /// otherwise a new value is created and added as an attribute.<br/>
        /// This is a convenience method. Attributes are accessed and
        /// manipulated via the dictionary returned from
        /// <see cref="Attributes"/>.
        /// </remarks>
        /// <param name="name">
        /// The name of the attribute.
        /// </param>
        /// <returns>
        /// The newly added attribute value.
        /// </returns>
        public virtual IXmlValue AddAttribute(string name)
        {
            IXmlValue attr = GetAttribute(name);
            if (attr == null)
            {
                attr = InstantiateAttribute();
                SetAttribute(name, attr);
            }
            return attr;
        }

        /// <summary>
        /// Get an attribute value, and return a temporary value if the
        /// attribute does not exist.
        /// </summary>
        /// This is a convenience method. Attributes are accessed and
        /// manipulated via the dictionary returned from
        /// <see cref="Attributes"/>.
        /// <param name="name">
        /// The name of the attribute.
        /// </param>
        /// <returns>
        /// The value of the specified attribute, or a temporary value if
        /// the attribute does not exist.
        /// </returns>
        public virtual IXmlValue GetSafeAttribute(string name)
        {
            IXmlValue value = GetAttribute(name);
            if (value == null)
            {
                value = InstantiateAttribute();
                value.Parent = this;
                if (value is SimpleValue)
                {
                    ((SimpleValue) value).IsMutable = false;
                }
            }

            return value;
        }

        /// <summary>
        /// Get or set the text of any comments that are in the XML element.
        /// </summary>
        /// <remarks>
        /// <b>The XML specification does not allow a comment to contain the
        /// string "--".</b><br/>
        /// An element can contain many comments interspersed randomly with
        /// textual values and child elements. In reality, comments are
        /// rarely used. The purpose of this method and the corresponding
        /// mutator are to ensure that if comments do exist, that their text
        /// will be accessible through this interface and not lost through a
        /// transfer from one instance of this interface to another.
        /// </remarks>
        /// <value>
        /// The comment text from this element (not including the "<!--" and
        /// "-->") or <c>null</c> if there was no comment.
        /// </value>
        public virtual string Comment
        {
            get { return m_comment; }
            set
            {
                CheckMutable();

                if (value.IndexOf("--") >= 0)
                {
                    throw new ArgumentException("comment contains \"--\"; see XML 1.0 2ed section 2.5 [15]");
                }

                m_comment = value;
            }
        }

        /// <summary>
        /// Write the element as it will appear in XML.
        /// </summary>
        /// <param name="writer">
        /// A <b>TextWriter</b> object to use to write to.
        /// </param>
        /// <param name="isPretty">
        /// <b>true</b> to specify that the output is intended to be as human
        /// readable as possible.
        /// </param>
        public virtual void WriteXml(TextWriter writer, bool isPretty)
        {
            string comment     = Comment;
            bool   hasComment  = !StringUtils.IsNullOrEmpty(comment);
            bool   hasValue    = !IsEmpty;
            bool   hasChildren = ElementList.Count > 0;

            if (!hasComment && !hasValue && !hasChildren)
            {
                WriteEmptyTag(writer, isPretty);
            }
            else if (!hasChildren)
            {
                WriteStartTag(writer, isPretty);
                WriteComment(writer, isPretty);
                WriteValue(writer, isPretty);
                WriteEndTag(writer, isPretty);
            }
            else
            {
                TextWriter writer2 = isPretty ? new IndentingWriter(writer, "  ") : writer;

                WriteStartTag(writer, isPretty);
                if (isPretty)
                {
                    writer.WriteLine();
                }

                if (hasComment)
                {
                    WriteComment(writer2, isPretty);
                    if (isPretty)
                    {
                        writer2.WriteLine();
                    }
                }

                if (hasValue)
                {
                    WriteValue(writer2, isPretty);
                    if (isPretty)
                    {
                        writer2.WriteLine();
                    }
                }

                WriteChildren(writer2, isPretty);
                if (isPretty)
                {
                    writer2.WriteLine();
                }

                writer2.Flush();

                WriteEndTag(writer, isPretty);
            }

            if (Parent == null)
            {
                writer.Flush();
            }
        }

        #endregion

        #region IXmlValue members

        /// <summary>
        /// Write the value as it will appear in XML.
        /// </summary>
        /// <param name="writer">
        /// A <b>TextWriter</b> object to use to write to.
        /// </param>
        /// <param name="isPretty">
        /// <b>true</b> to specify that the output is intended to be as human
        /// readable as possible.
        /// </param>
        public override void WriteValue(TextWriter writer, bool isPretty)
        {
            if (isPretty && writer is IndentingWriter)
            {
                ((IndentingWriter) writer).Suspend();
                base.WriteValue(writer, isPretty);
                ((IndentingWriter) writer).Resume();
            }
            else
            {
                base.WriteValue(writer, isPretty);
            }
        }
        
        #endregion

        #region IPortableObject members

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void ReadExternal(IPofReader reader)
        {
            if (m_name != null)
            {
                throw new Exception("deserialization not active");
            }

            m_isDeserializing = true;
            try
            {
                base.ReadExternal(reader);

                // element name
                if (reader.ReadBoolean(4))
                {
                    m_name = reader.ReadString(5);
                }

                // child elements
                if (reader.ReadBoolean(6))
                {
                    reader.ReadCollection(7, m_listChildren = InstantiateElementList());
                }

                // attributes
                if (reader.ReadBoolean(8))
                {
                    reader.ReadDictionary(9, m_attributes = InstantiateAttributes());
                }

                // element comment
                if (reader.ReadBoolean(10))
                {
                    m_comment = reader.ReadString(11);
                }
            }
            finally
            {
                m_isDeserializing = false;
            }
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            // element name
            string  name    = m_name;
            bool    hasName = name != null;
            writer.WriteBoolean(4, hasName);
            if (hasName)
            {
                writer.WriteString(5, name);
            }

            // child elements
            IList listKids = m_listChildren;
            bool  hasKids  = listKids != null && listKids.Count > 0;
            writer.WriteBoolean(6, hasKids);
            if (hasKids)
            {
                writer.WriteCollection(7, listKids);
            }

            // attributes
            IDictionary mapAttr = m_attributes;
            bool        hasAttr = mapAttr != null && mapAttr.Count > 0;
            writer.WriteBoolean(8, hasAttr);
            if (hasAttr)
            {
                writer.WriteDictionary(9, mapAttr);
            }

            // element comment
            string  comment    = m_comment;
            bool    hasComment = comment != null;
            writer.WriteBoolean(10, hasComment);
            if (hasComment)
            {
                writer.WriteString(11, comment);
            }
        }

        #endregion

        #region Support for inheriting implementations

        /// <summary>
        /// Validates that the element is mutable, otherwise throws an
        /// <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        protected virtual void CheckMutable()
        {
            if (!IsMutable)
            {
                throw new InvalidOperationException("element \"" + AbsolutePath + "\" is not mutable");
            }
        }

        /// <summary>
        /// Determine if the name can be changed.
        /// </summary>
        /// <remarks>
        /// The default implementation allows a name to be changed. This can
        /// be overridden by inheriting implementations.
        /// </remarks>
        /// <returns>
        /// <b>true</b> if the name can be changed.
        /// </returns>
        protected virtual bool IsNameMutable
        {
            get { return m_isDeserializing || IsMutable; }
        }

        /// <summary>
        /// Instantiate an <b>IList</b> implementation that will hold child
        /// elements.
        /// </summary>
        /// <returns>
        /// A IList that supports IXmlElements.
        /// </returns>
        protected virtual IList InstantiateElementList()
        {
            return new SimpleElementList(this);
        }

        /// <summary>
        /// Instantiate an IXmlElement implementation for an element.
        /// </summary>
        /// <param name="name">
        /// Element name.
        /// </param>
        /// <param name="value">
        /// Element value.
        /// </param>
        /// <returns>
        /// A new IXmlElement to be used as an element.
        /// </returns>
        protected virtual IXmlElement InstantiateElement(string name, object value)
        {
            return new SimpleElement(name, value);
        }

        /// <summary>
        /// Instantiate an <b>IDictionary</b> implementation that will
        /// support the name to value dictionary used to hold attributes.
        /// </summary>
        /// <returns>
        /// A IDictionary that supports string keys and <b>IXmlValue</b>
        /// values.
        /// </returns>
        protected virtual IDictionary InstantiateAttributes()
        {
            return new SimpleElementAttributes(this);
        }

        /// <summary>
        /// Instantiate an <see cref="IXmlValue"/> implementation for an
        /// attribute value.
        /// </summary>
        /// <returns>
        /// A new <b>IXmlValue</b> to be used as an attribute value.
        /// </returns>
        protected virtual IXmlValue InstantiateAttribute()
        {
            return new SimpleValue(null, true);
        }

        #endregion

        #region XML generation

        /// <summary>
        /// Write the element as a combined start/end tag.
        /// </summary>
        /// <param name="writer">
        /// A <b>TextWriter</b> object to use to write to.
        /// </param>
        /// <param name="isPretty">
        /// <b>true</b> to specify that the output is intended to be as human
        /// readable as possible.
        /// </param>
        protected virtual void WriteEmptyTag(TextWriter writer, bool isPretty)
        {
            writer.Write('<');
            writer.Write(Name);
            WriteAttributes(writer, isPretty);
            writer.Write("/>");
        }

        /// <summary>
        /// Write the element's start tag.
        /// </summary>
        /// <param name="writer">
        /// A <b>TextWriter</b> object to use to write to.
        /// </param>
        /// <param name="isPretty">
        /// <b>true</b> to specify that the output is intended to be as human
        /// readable as possible.
        /// </param>
        protected virtual void WriteStartTag(TextWriter writer, bool isPretty)
        {
            writer.Write('<');
            writer.Write(Name);
            WriteAttributes(writer, isPretty);
            writer.Write('>');
        }

        /// <summary>
        /// Write the element's end tag.
        /// </summary>
        /// <param name="writer">
        /// A <b>TextWriter</b> object to use to write to.
        /// </param>
        /// <param name="isPretty">
        /// <b>true</b> to specify that the output is intended to be as human
        /// readable as possible.
        /// </param>
        protected virtual void WriteEndTag(TextWriter writer, bool isPretty)
        {
            writer.Write("</");
            writer.Write(Name);
            writer.Write(">");
        }

        /// <summary>
        /// Write the attributes as part of a start tag.
        /// </summary>
        /// <param name="writer">
        /// A <b>TextWriter</b> object to use to write to.
        /// </param>
        /// <param name="isPretty">
        /// <b>true</b> to specify that the output is intended to be as human
        /// readable as possible.
        /// </param>
        protected virtual void WriteAttributes(TextWriter writer, bool isPretty)
        {
            foreach (DictionaryEntry entry in Attributes)
            {
                var name  = (string)    entry.Key;
                var value = (IXmlValue) entry.Value;
                writer.Write(' ');
                writer.Write(name);
                writer.Write('=');
                value.WriteValue(writer, isPretty);
            }
        }

        /// <summary>
        /// Write the comment as it will appear in XML.
        /// </summary>
        /// <param name="writer">
        /// A <b>TextWriter</b> object to use to write to.
        /// </param>
        /// <param name="isPretty">
        /// <b>true</b> to specify that the output is intended to be as human
        /// readable as possible.
        /// </param>
        protected virtual void WriteComment(TextWriter writer, bool isPretty)
        {
            string comment = Comment;
            if (!StringUtils.IsNullOrEmpty(comment))
            {
                writer.Write("<!--");

                if (isPretty)
                {
                    writer.WriteLine();
                    writer.WriteLine(StringUtils.BreakLines(comment, 78, ""));
                }
                else
                {
                    writer.Write(comment);
                }

                writer.Write("-->");
            }
        }

        /// <summary>
        /// Write the element's children.
        /// </summary>
        /// <param name="writer">
        /// A <b>TextWriter</b> object to use to write to.
        /// </param>
        /// <param name="isPretty">
        /// <b>true</b> to specify that the output is intended to be as human
        /// readable as possible.
        /// </param>
        protected virtual void WriteChildren(TextWriter writer, bool isPretty)
        {
            IList list    = ElementList;
            bool  isFirst = true;
            foreach (IXmlElement element in list)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else if (isPretty)
                {
                    writer.WriteLine();
                }

                element.WriteXml(writer, isPretty);
            }
        }
        
        #endregion

        #region Object methods

        /// <summary>
        /// Format the XML element and all its contained information into a
        /// string in a display format.
        /// </summary>
        /// <remarks>
        /// Note that this overrides the contract of the ToString() method in
        /// the super interface IXmlValue.
        /// </remarks>
        /// <returns>
        /// A string representation of the XML element.
        /// </returns>
        public override string ToString()
        {
            return ToString(true);
        }

        /// <summary>
        /// Format the XML element and all its contained information into a
        /// string in a display format.
        /// </summary>
        /// <param name="isPretty">
        /// <b>true</b> to specify that the output is intended to be as human
        /// readable as possible.
        /// </param>
        /// <returns>
        /// A string representation of the XML element.
        /// </returns>
        public virtual string ToString(bool isPretty)
        {
            var writer = new StringWriter();
            WriteXml(writer, isPretty);
            writer.Flush();

            return writer.ToString();
        }

        /// <summary>
        /// Provide a hash value for this XML element and all of its
        /// contained information.
        /// </summary>
        /// <remarks>
        /// Note that this overrides the contract of the GetHashCode() method
        /// in the super interface IXmlValue. The hash value is defined as a
        /// xor of the following:
        /// <list type="number">
        /// <item>the GetHashCode() from the element's value (i.e.
        /// <c>base.GetHashCode()</c>)</item>
        /// <item>the GetHashCode() from each attribute name</item>
        /// <item>the GetHashCode() from each attribute value</item>
        /// <item>the GetHashCode() from each sub-element</item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// The hash value for this XML element.
        /// </returns>
        public override int GetHashCode()
        {
            return XmlHelper.HashElement(this);
        }

        /// <summary>
        /// Compare this XML element and all of its contained information
        /// with another XML element for equality.
        /// </summary>
        /// <remarks>
        /// Note that this overrides the contract of the Equals() method in
        /// the super interface IXmlValue.
        /// </remarks>
        /// <param name="o">
        /// The object to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if the elements are equal, <b>false</b> otherwise.
        /// </returns>
        public override bool Equals(object o)
        {
            if (!(o is IXmlElement))
            {
                return false;
            }

            return XmlHelper.EqualsElement(this, (IXmlElement) o);
        }

        #endregion

        #region ICloneable members

        /// <summary>
        /// Creates and returns a copy of this SimpleElement.
        /// </summary>
        /// <remarks>
        /// The returned copy is a deep clone of this SimpleElement
        /// "unlinked" from the parent and mutable.
        /// </remarks>
        /// <returns>
        /// A clone of this instance.
        /// </returns>
        public override object Clone()
        {
            var that = (SimpleElement) base.Clone();

            IDictionary mapThat = that.InstantiateAttributes();
            foreach (DictionaryEntry entry in Attributes)
            {
                var name  = (string) entry.Key;
                var value = (IXmlValue) entry.Value;

                mapThat[name] = value.Clone();
            }
            that.m_attributes = mapThat;

            IList listThat = that.InstantiateElementList();
            foreach (IXmlElement el in ElementList)
            {
                listThat.Add(el.Clone());
            }
            that.m_listChildren = listThat;

            return that;
        }

        #endregion

        #region Data members

        private string      m_name;
        private IList       m_listChildren;
        private IDictionary m_attributes;
        private string      m_comment;

        [NonSerialized]
        private bool m_isDeserializing;

        #endregion

        #region Inner class: SimpleElementList

        /// <summary>
        /// An implementation of <b>IList</b> that only supports IXmlElements
        /// as the content of the list.
        /// </summary>
        protected class SimpleElementList : ArrayList
        {
            /// <summary>
            /// Construct an empty SimpleElementList with specified parent
            /// <b>SimpleElement</b>.
            /// </summary>
            /// <param name="parent">
            /// Parent <b>SimpleElement</b>.
            /// </param>
            public SimpleElementList(SimpleElement parent)
            {
                m_parent = parent;
            }

            /// <summary>
            /// Parent <b>SimpleElement</b>.
            /// </summary>
            public virtual SimpleElement Parent
            {
                get { return m_parent; }
            }

            /// <summary>
            /// Adds an object to the end of the ArrayList.
            /// </summary>
            /// <param name="value">
            /// The object to be added to the end of the ArrayList. The value
            /// can be <c>null</c>.
            /// </param>
            /// <returns>
            /// The index at which the value has been added.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override int Add(object value)
            {
                CheckMutable();
                return base.Add(CheckElement((IXmlElement) value));
            }

            /// <summary>
            /// Adds the elements of an <b>ICollection</b> to the end of the
            /// ArrayList.
            /// </summary>
            /// <param name="c">
            /// The <b>ICollection</b> whose elements should be added to the
            /// end of the ArrayList. The collection itself cannot be
            /// <c>null</c>, but it can contain elements that are
            /// <c>null</c>.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Collection is <c>null</c>.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void AddRange(ICollection c)
            {
                foreach (object o in c)
                {
                    Add(o);
                }
            }

            /// <summary>
            /// Inserts an element into the ArrayList at the specified index.
            /// </summary>
            /// <param name="index">
            /// The zero-based index at which value should be inserted.
            /// </param>
            /// <param name="value">
            /// The object to insert. The value can be <c>null</c>.
            /// </param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero or greater than
            /// <see cref="ArrayList.Count"/>.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void Insert(int index, object value)
            {
                CheckMutable();
                base.Insert(index, CheckElement((IXmlElement) value));
            }

            /// <summary>
            /// Inserts the elements of a collection into the ArrayList at
            /// the specified index.
            /// </summary>
            /// <param name="index">
            /// The zero-based index at which the new elements should be
            /// inserted.
            /// </param>
            /// <param name="c">
            ///  The <b>ICollection</b> whose elements should be inserted
            /// into the ArrayList. The collection itself cannot be
            /// <c>null</c>, but it can contain elements that are
            /// <c>null</c>.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Collection is <c>null</c>.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero or greater than
            /// <see cref="ArrayList.Count"/>.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void InsertRange(int index, ICollection c)
            {
                CheckMutable();

                IList checkedList = new ArrayList();
                foreach (object o in c)
                {
                    checkedList.Add(CheckElement((IXmlElement) o));
                }
                base.InsertRange(index, checkedList);
            }

            /// <summary>
            /// Removes the first occurrence of a specific object from the
            /// ArrayList.
            /// </summary>
            /// <param name="obj">
            ///  The object to remove from the ArrayList. The value can be
            /// <c>null</c>.
            /// </param>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void Remove(object obj)
            {
                CheckMutable();
                base.Remove(obj);
            }

            /// <summary>
            /// Removes the element at the specified index of the ArrayList.
            /// </summary>
            /// <param name="index">
            /// The zero-based index of the element to remove.
            /// </param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero or greater than
            /// <see cref="ArrayList.Count"/>.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void RemoveAt(int index)
            {
                CheckMutable();
                base.RemoveAt(index);
            }

            /// <summary>
            /// Removes a range of elements from the ArrayList.
            /// </summary>
            /// <param name="index">
            /// The zero-based starting index of the range of elements to
            /// remove.
            /// </param>
            /// <param name="count">
            /// The number of elements to remove.
            /// </param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero or greater than
            /// <see cref="ArrayList.Count"/>.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Index and count do not denote a valid range of elements in
            /// the ArrayList.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void RemoveRange(int index, int count)
            {
                CheckMutable();
                base.RemoveRange(index, count);
            }

            /// <summary>
            /// Removes all elements from the ArrayList.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void Clear()
            {
                CheckMutable();
                base.Clear();
            }

            /// <summary>
            /// Copies the elements of a collection over a range of elements
            /// in the ArrayList.
            /// </summary>
            /// <param name="index">
            /// The zero-based ArrayList index at which to start copying the
            /// elements of c.
            /// </param>
            /// <param name="c">
            /// The <b>ICollection</b> whose elements to copy to the
            /// ArrayList. The collection itself cannot be <c>null</c>, but
            /// it can contain elements that are <c>null</c>.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Collection is <c>null</c>.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero or greater than
            /// <see cref="ArrayList.Count"/>.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void SetRange(int index, ICollection c)
            {
                CheckMutable();

                IList checkedList = new ArrayList();
                foreach (object o in c)
                {
                    checkedList.Add(CheckElement((IXmlElement) o));
                }
                base.SetRange(index, checkedList);
            }

            /// <summary>
            /// Reverses the order of the elements in the entire ArrayList.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void Reverse()
            {
                CheckMutable();
                base.Reverse();
            }

            /// <summary>
            /// Reverses the order of the elements in the specified range.
            /// </summary>
            /// <param name="index">
            /// The zero-based starting index of the range to reverse.
            /// </param>
            /// <param name="count">
            /// The number of elements in the range to reverse.
            /// </param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero or count is less than zero.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Index and count do not denote a valid range of elements in
            /// the ArrayList.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void Reverse(int index, int count)
            {
                CheckMutable();
                base.Reverse(index, count);
            }

            /// <summary>
            /// Gets or sets the element at the specified index.
            /// </summary>
            /// <param name="index">
            /// The zero-based index of the element to get or set.
            /// </param>
            /// <returns>
            /// The element at the specified index.
            /// </returns>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero or greater than
            /// <see cref="ArrayList.Count"/>.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override object this[int index]
            {
                set
                {
                    CheckMutable();
                    base[index] = CheckElement((IXmlElement) value);
                }
            }

            /// <summary>
            /// Sorts the elements in the entire ArrayList using the
            /// <b>IComparable</b> implementation of each element.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void Sort()
            {
                CheckMutable();
                base.Sort();
            }

            /// <summary>
            /// Sorts the elements in a range of elements in ArrayList using
            /// the specified comparer.
            /// </summary>
            /// <param name="index">
            /// The zero-based starting index of the range to sort.
            /// </param>
            /// <param name="count">
            /// The length of the range to sort.
            /// </param>
            /// <param name="comparer">
            /// The <b>IComparer</b> implementation to use when comparing
            /// elements or <c>null</c> to use the <b>IComparable</b>
            /// implementation of each element.
            /// </param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero or count is less than zero.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Index and count do not denote a valid range of elements in
            /// the ArrayList.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void Sort(int index, int count, IComparer comparer)
            {
                CheckMutable();
                base.Sort(index, count, comparer);
            }

            /// <summary>
            /// Sorts the elements in the entire ArrayList using the
            /// specified comparer.
            /// </summary>
            /// <param name="comparer">
            /// The <b>IComparer</b> implementation to use when comparing
            /// elements or <c>null</c> to use the <b>IComparable</b>
            /// implementation of each element.
            /// </param>
            /// <exception cref="InvalidOperationException">
            /// Parent SimpleElement is not mutable.
            /// </exception>
            public override void Sort(IComparer comparer)
            {
                CheckMutable();
                base.Sort(comparer);
            }

            /// <summary>
            /// Provide a hash value for this SimpleElementList. 
            /// </summary>
            /// <returns>
            /// A hash code for this SimpleElementList.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override int GetHashCode()
            {
                int n = 0;
                foreach (IXmlElement element in this)
                {
                    n ^= element.GetHashCode();
                }
                return n;
            }

            /// <summary>
            /// Compare this list with another for equality.
            /// </summary>
            /// <param name="obj">
            /// The list to compare to.
            /// </param>
            /// <returns>
            /// <b>true</b> if the lists are equal, <b>false</b> otherwise.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj is IList)
                {
                    var that = (IList) obj;
                    if (Count != that.Count)
                    {
                        return false;
                    }

                    IEnumerator iterThis = GetEnumerator();
                    IEnumerator iterThat = that.GetEnumerator();
                    try
                    {
                        while (iterThis.MoveNext())
                        {
                            if (!iterThat.MoveNext() || !iterThis.Current.Equals(iterThat.Current))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    catch (InvalidOperationException) { }
                }
                return false;
            }



            /// <summary>
            /// Validates the passed IXmlElement, copying the element into a
            /// new valid element if necessary.
            /// </summary>
            /// <param name="element">
            /// The element to validate.
            /// </param>
            /// <returns>
            /// The new valid element.
            /// </returns>
            protected virtual IXmlElement CheckElement(IXmlElement element)
            {
                // element must not have a parent
                if (element.Parent != null)
                {
                    // copy name and value
                    IXmlElement elementNew = m_parent.InstantiateElement(element.Name, element.Value);

                    // copy comment
                    string comment = element.Comment;
                    if (comment != null)
                    {
                        elementNew.Comment = comment;
                    }

                    // copy attributes
                    IDictionary map  = element.Attributes;
                    if (map.Count > 0)
                    {
                        CollectionUtils.AddAll(elementNew.Attributes, map);
                    }

                    // copy child elements
                    IList list = element.ElementList;
                    if (list.Count > 0)
                    {
                        CollectionUtils.AddAll(elementNew.ElementList, list);
                    }

                    element = elementNew;
                }

                element.Parent = m_parent;
                return element;
            }

            /// <summary>
            /// Validates that the parent element is mutable, otherwise
            /// throws an <see cref="InvalidOperationException"/>.
            /// </summary>
            /// <exception cref="InvalidOperationException"/>
            protected virtual void CheckMutable()
            {
                Parent.CheckMutable();
            }

            private readonly SimpleElement m_parent;
        }

        #endregion

        #region Inner class: SimpleElementEnumerator

         /// <summary>
         /// Provides an <b>IEnumerator</b> implementation that exposes only
         /// those elements from the element list that match a certain name.
         /// </summary>
        protected class SimpleElementEnumerator : IEnumerator
        {
            /// <summary>
            /// Create new SimpleElementEnumerator with specified parent
            /// element and element name.
            /// </summary>
            /// <param name="parent">
            /// Parent <b>SimpleElement</b>.
            /// </param>
            /// <param name="name">
            /// Name of elements that will be enumerated.
            /// </param>
            public SimpleElementEnumerator(SimpleElement parent, string name)
            {
                m_name       = name;
                m_enumerator = parent.ElementList.GetEnumerator();
            }

            /// <summary>
            /// Advances the enumerator to the next element of the
            /// collection.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the enumerator was successfully advanced to
            /// the next element; <b>false</b> if the enumerator has passed
            /// the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public virtual bool MoveNext()
            {
                if (m_state == FOUND)
                {
                    return true;
                }

                IEnumerator enumerator = m_enumerator;
                while (enumerator.MoveNext())
                {
                    var element = (IXmlElement) enumerator.Current;
                    if (element.Name.Equals(m_name))
                    {
                        m_element = element;
                        m_state   = FOUND;
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before
            /// the first element in the collection.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public virtual void Reset()
            {
                m_enumerator.Reset();
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <returns>
            /// The current element in the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first element of the
            /// collection or after the last element.
            /// </exception>
            public virtual object Current
            {
                get
                {
                    switch (m_state)
                    {
                        case RETURNED:
                        case INITIAL:
                            if (!MoveNext())
                            {
                                throw new InvalidOperationException();
                            }
                            goto case FOUND;
                        case FOUND:
                            m_state = RETURNED;
                            return m_element;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            /// <summary>
            /// Name of elements that are enumerated.
            /// </summary>
            protected string        m_name;

            /// <summary>
            /// Elements list enumerator that is wrapped by this enumerator.
            /// </summary>
            protected IEnumerator   m_enumerator;

            /// <summary>
            /// Current element.
            /// </summary>
            protected IXmlElement   m_element;

            /// <summary>
            /// Current state, can be one of values <see cref="FOUND"/>,
            /// <see cref="RETURNED"/> and <see cref="INITIAL"/>.
            /// </summary>
            protected int           m_state = INITIAL;

            /// <summary>
            /// Element with specified name has been found.
            /// </summary>
            protected const int FOUND    = 0;
            /// <summary>
            /// Element with specified name has been returned.
            /// </summary>
            protected const int RETURNED = 1;
            /// <summary>
            /// Initial enumerator state.
            /// </summary>
            protected const int INITIAL  = 2;
        }

        #endregion

        #region Inner class: SimpleElementAttributes

        /// <summary>
        /// An <b>IDictionary</b> implementation using a
        /// <b>ListDictionary</b> that supports only strings for keys and
        /// <see cref="IXmlValue"/> for values.
        /// </summary>
        protected class SimpleElementAttributes : IDictionary, ICloneable
        {
            /// <summary>
            /// Create new SimpleElementAttributes instance with specified
            /// parent.
            /// </summary>
            /// <param name="parent">
            /// Parent <b>SimpleElement</b>.
            /// </param>
            public SimpleElementAttributes(SimpleElement parent)
            {
                m_parent = parent;
            }

            /// <summary>
            /// Determines whether this dictionary contains an element with
            /// the specified key.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the dictionary contains an element with the
            /// key; otherwise, <b>false</b>.
            /// </returns>
            /// <param name="key">
            /// The key to locate in the dictionary.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Key is <c>null</c>.
            /// </exception>
            public virtual bool Contains(object key)
            {
                return m_listDict.Contains(key);
            }

            /// <summary>
            /// Adds an element with the provided key and value to the
            /// dictionary.
            /// </summary>
            /// <param name="key">
            /// The object to use as the key of the element to add.
            /// </param>
            /// <param name="value">
            /// The object to use as the value of the element to add.
            /// </param>
            /// <exception cref="ArgumentException">
            /// An element with the same key already exists in the
            /// dictionary.
            /// </exception>
            /// <exception cref="ArgumentNullException">
            /// Key is <c>null</c>.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Key is not string, or it is not a valid attribute name, or
            /// value is not <see cref="IXmlValue"/>.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Parent <b>SimpleElement</b> is not mutable.
            /// </exception>
            public virtual void Add(object key, object value)
            {
                CheckMutable();
                CheckKey(key);
                IXmlValue xmlvalue = CheckValue(value);

                m_listDict.Add(key, xmlvalue);
            }

            /// <summary>
            /// Removes all elements from the dictionary.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// Parent <b>SimpleElement</b> is not mutable.
            /// </exception>
            public virtual void Clear()
            {
                CheckMutable();
                m_listDict.Clear();
            }

            /// <summary>
            /// Returns an <b>IDictionaryEnumerator</b> object for the
            /// dictionary.
            /// </summary>
            /// <returns>
            /// An <b>IDictionaryEnumerator</b> object for the dictionary.
            /// </returns>
            IDictionaryEnumerator IDictionary.GetEnumerator()
            {
                return m_listDict.GetEnumerator();
            }

            /// <summary>
            /// Removes the element with the specified key from the
            /// dictionary.
            /// </summary>
            /// <param name="key">
            /// The key of the element to remove.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Key is <c>null</c>.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Parent <b>SimpleElement</b> is not mutable.
            /// </exception>
            public virtual void Remove(object key)
            {
                CheckMutable();
                m_listDict.Remove(key);
            }

            /// <summary>
            /// Gets or sets the element with the specified key.
            /// </summary>
            /// <returns>
            /// The element with the specified key.
            /// </returns>
            /// <param name="key">
            /// The key of the element to get or set.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Key is <c>null</c>.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Key is not string, or it is not a valid attribute name, or
            /// value is not <see cref="IXmlValue"/>.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// Parent <b>SimpleElement</b> is not mutable.
            /// </exception>
            public virtual object this[object key]
            {
                get { return m_listDict[key]; }
                set
                {
                    CheckMutable();
                    CheckKey(key);

                    IXmlValue xmlvalue = CheckValue(value);
                    m_listDict[key] = xmlvalue;
                }
            }

            /// <summary>
            /// Gets an <b>ICollection</b> object containing the keys of the
            /// dictionary.
            /// </summary>
            /// <returns>
            /// An <b>ICollection</b> containing the keys of the dictionary.
            /// </returns>
            public virtual ICollection Keys
            {
                get { return m_listDict.Keys; }
            }

            /// <summary>
            /// Gets an <b>ICollection</b> object containing the values of
            /// the dictionary.
            /// </summary>
            /// <returns>
            /// An <b>ICollection</b> containing the values of the
            /// dictionary.
            /// </returns>
            public virtual ICollection Values
            {
                get { return m_listDict.Values; }
            }

            /// <summary>
            /// Gets a value indicating whether the dictionary is read-only.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the dictionary is read-only; otherwise,
            /// <b>false</b>.
            /// </returns>
            public virtual bool IsReadOnly
            {
                get { return m_listDict.IsReadOnly || !m_parent.IsMutable; }
            }

            /// <summary>
            /// Gets a value indicating whether the dictionary object has a
            /// fixed size.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the dictionary object has a fixed size;
            /// otherwise, <b>false</b>.
            /// </returns>
            public virtual bool IsFixedSize
            {
                get { return m_listDict.IsFixedSize; }
            }

            /// <summary>
            /// Copies the elements of the <b>ICollection</b> to an
            /// <b>Array</b>, starting at a particular array index.
            /// </summary>
            /// <param name="array">
            /// The one-dimensional array that is the destination of the
            /// elements copied from <b>ICollection</b>. The array must have
            /// zero-based indexing.
            /// </param>
            /// <param name="index">
            /// The zero-based index in array at which copying begins.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Array is <c>null</c>.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Array is multidimensional or index is equal to or greater
            /// than the length of array or the number of elements in the
            /// source collection is greater than the available space from
            /// index to the end of the destination array.
            /// </exception>
            /// <exception cref="InvalidCastException">
            /// The type of the source collection cannot be cast
            /// automatically to the type of the destination array.
            /// </exception>
            public virtual void CopyTo(Array array, int index)
            {
                m_listDict.CopyTo(array, index);
            }

            /// <summary>
            /// Gets the number of elements contained in the collection.
            /// </summary>
            /// <returns>
            /// The number of elements contained in the collection.
            /// </returns>
            public virtual int Count
            {
                get { return m_listDict.Count; }
            }

            /// <summary>
            /// Gets an object that can be used to synchronize access to the
            /// collection.
            /// </summary>
            /// <returns>
            /// An object that can be used to synchronize access to the
            /// collection.
            /// </returns>
            public virtual object SyncRoot
            {
                get { return m_listDict.SyncRoot; }
            }

            ///<summary>
            /// Gets a value indicating whether access to the collection is
            /// synchronized (thread safe).
            /// </summary>
            /// <returns>
            /// <b>true</b> if access to the collection is synchronized
            /// (thread safe); otherwise, <b>false</b>.
            /// </returns>
            public virtual bool IsSynchronized
            {
                get { return m_listDict.IsSynchronized; }
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <b>IEnumerator</b> object that can be used to iterate
            /// through the collection.
            /// </returns>
            public virtual IEnumerator GetEnumerator()
            {
                return m_listDict.GetEnumerator();
            }

            /// <summary>
            /// Provide a hash value for this SimpleElementAttributes. 
            /// </summary>
            /// <returns>
            /// A hash code for the current <see cref="T:System.Object" />.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override int GetHashCode()
            {
                int n = 0;
                foreach (DictionaryEntry entry in this)
                {
                    // entry.GetHashCode() is a xor of the key and value, which
                    // is the attribute name and value
                    n ^= entry.Key.GetHashCode() ^ entry.Value.GetHashCode();
                }
                return n;
            }

            /// <summary>
            /// Compare this dictionary with another for equality.
            /// </summary>
            /// <param name="obj">
            /// The object to compare to.
            /// </param>
            /// <returns>
            /// <b>true</b> if the dictionaries are equal, <b>false</b>
            /// otherwise.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj is IDictionary)
                {
                    var that = (IDictionary) obj;
                    if (Count != that.Count)
                    {
                        return false;
                    }

                    IEnumerator iterThis = GetEnumerator();
                    IEnumerator iterThat = that.GetEnumerator();
                    try
                    {
                        while (iterThis.MoveNext())
                        {
                            if (!iterThat.MoveNext() || !iterThis.Current.Equals(iterThat.Current))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    catch (InvalidOperationException) {}
                }
                return false;
            }

            /// <summary>
            /// Creates and returns a copy of this dictionary.
            /// </summary>
            /// <returns>
            /// A clone of this instance.
            /// </returns>
            public virtual object Clone()
            {
                var that = new SimpleElementAttributes(m_parent);
                foreach (DictionaryEntry entry in this)
                {
                    that[entry.Key] = entry.Value;
                }
                return that;
            }

            /// <summary>
            /// Validates that the parent element is mutable, otherwise
            /// throws an <see cref="InvalidOperationException"/>.
            /// </summary>
            /// <exception cref="InvalidOperationException"/>
            protected virtual void CheckMutable()
            {
                m_parent.CheckMutable();
            }

            /// <summary>
            /// Validates that specified key is string and is valid
            /// attribute name (<see cref="XmlHelper.IsNameValid(String)"/>.
            /// </summary>
            /// <param name="key">
            /// Key to be validated.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Key is not string or is not valid name.
            /// </exception>
            protected virtual void CheckKey(object key)
            {
                if (!(key is string))
                {
                    throw new ArgumentException("attribute name must be a String");
                }

                if (!XmlHelper.IsNameValid((string) key))
                {
                    throw new ArgumentException("illegal name \"" + key + "\"; see XML 1.0 2ed section 2.3 [5]");
                }
            }

            /// <summary>
            /// Validates that the valus is <see cref="IXmlValue"/>.
            /// </summary>
            /// <param name="value">
            /// Value to be validated.
            /// </param>
            /// <returns>
            /// <b>IXmlValue</b> whose parent is set to this attributes
            /// parent.
            /// </returns>
            protected virtual IXmlValue CheckValue(object value)
            {
                if (!(value is IXmlValue))
                {
                    throw new ArgumentException("attribute value must be an XmlValue");
                }

                var xmlvalue = (IXmlValue) value;
                if (xmlvalue.Parent != null || !xmlvalue.IsAttribute)
                {
                    // clone the value as an attribute
                    xmlvalue = new SimpleValue(xmlvalue.Value, true);
                }

                // set the parent of the value to this element
                xmlvalue.Parent = m_parent;
                return xmlvalue;
            }

            private readonly SimpleElement  m_parent;
            private readonly ListDictionary m_listDict = new ListDictionary();
        }

        #endregion
    }
}
