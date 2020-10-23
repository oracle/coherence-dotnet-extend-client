/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Run.Xml
{
    /// <summary>
    /// An interface for XML document access.
    /// </summary>
    /// <remarks>
    /// The IXmlDocumnet interface represents the document as both the root
    /// element (through the underlying IXmlElement interface) and the
    /// properties specific to a document, such as DOCTYPE.
    /// </remarks>
    /// <author>Cameron Purdy  2001.07.11</author>
    /// <author>Ana Cikic  2009.08.25</author>
    public interface IXmlDocument : IXmlElement
    {
        /// <summary>
        /// Get or set the URI of the DTD (DOCTYPE) for the document.
        /// </summary>
        /// <remarks>
        /// This is referred to as the System Identifier by the XML
        /// specification.
        /// </remarks>
        /// <example>
        /// http://java.sun.com/j2ee/dtds/web-app_2_2.dtd
        /// </example>
        /// <value>
        /// The document type URI.
        /// </value>
        string DtdUri { get; set; }

        /// <summary>
        /// Get or set the public identifier of the DTD (DOCTYPE) for the
        /// document.
        /// </summary>
        /// <example>
        /// -//Sun Microsystems, Inc.//DTD Web Application 1.2//EN
        /// </example>
        /// <value>
        /// The DTD public identifier.
        /// </value>
        string DtdName { get; set; }

        /// <summary>
        /// Get or set the encoding string for the XML document.
        /// </summary>
        /// <remarks>
        /// Documents that are parsed may or may not have the encoding string
        /// from the persistent form of the document.
        /// </remarks>
        /// <value>
        /// The encoding set for the document.
        /// </value>
        string Encoding { get; set; }

        /// <summary>
        /// Get or set the XML comment that appears outside of the root
        /// element.
        /// </summary>
        /// <remarks>
        /// This differs from the <see cref="IXmlElement.Comment"/> property
        /// of this object, which refers to the comment within the root
        /// element.
        /// </remarks>
        /// <value>
        /// The document comment.
        /// </value>
        string DocumentComment { get; set; }
    }
}
