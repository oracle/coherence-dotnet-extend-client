/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

namespace Tangosol.Run.Xml
{
    /// <summary>
    /// An interface for XML element access.
    /// </summary>
    /// <remarks>
    /// The IXmlElement interface represents both the element and its content
    /// (through the underlying IXmlValue interface).
    /// </remarks>
    /// <author>Cameron Purdy  2000.10.12</author>
    /// <author>Ana Cikic  2009.08.25</author>
    public interface IXmlElement : IXmlValue
    {
        #region Properties

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
        string Name { get; set; }

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
        IXmlElement Root { get; }

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
        string AbsolutePath { get; }

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
        IList ElementList { get; }

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
        string Comment { get; set; }

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
        IDictionary Attributes { get; }

        #endregion

        #region IXmlElement methods

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
        IXmlElement GetElement(string name);

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
        IEnumerator GetElements(string name);

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
        IXmlElement AddElement(string name);

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
        IXmlElement FindElement(string path);

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
        IXmlElement GetSafeElement(string path);

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
        IXmlElement EnsureElement(string path);

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
        IXmlValue GetAttribute(string name);

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
        void SetAttribute(string name, IXmlValue value);

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
        IXmlValue AddAttribute(string name);

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
        IXmlValue GetSafeAttribute(string name);

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
        void WriteXml(TextWriter writer, bool isPretty);

        #endregion
    }
}
