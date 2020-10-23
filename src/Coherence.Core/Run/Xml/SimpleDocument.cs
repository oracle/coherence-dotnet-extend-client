/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Util;

namespace Tangosol.Run.Xml
{
    /// <summary>
    /// A simple implementation of the IXmlDocument interface.
    /// </summary>
    /// <remarks>
    /// Protected methods are provided to support inheriting classes.
    /// </remarks>
    /// <author>Cameron Purdy  2000.10.20</author>
    /// <author>Ana Cikic  2009.08.27</author>
    public class SimpleDocument : SimpleElement, IXmlDocument
    {
        #region Constructors

        /// <summary>
        /// Construct an empty SimpleDocument.
        /// </summary>
        public SimpleDocument()
        {}

        /// <summary>
        /// Construct a SimpleDocument.
        /// </summary>
        /// <param name="name">
        /// The name of the root element.
        /// </param>
        public SimpleDocument(string name) : base(name)
        {}

        /// <summary>
        /// Construct a SimpleDocument.
        /// </summary>
        /// <param name="name">
        /// The name of the root element.
        /// </param>
        /// <param name="dtdUri">
        /// The URI of the DTD (system identifier).
        /// </param>
        /// <param name="dtdName">
        /// The name of the DTD (public identifier); may be <c>null</c>.
        /// </param>
        public SimpleDocument(string name, string dtdUri, string dtdName) : base(name)
        {
            DtdUri  = dtdUri;
            DtdName = dtdName;
        }

        #endregion

        #region IXmlDocument members

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
        public virtual string DtdUri
        {
            get { return m_dtdUri; }
            set 
            {
                CheckMutable();
                m_dtdUri = value;
            }
        }

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
        public virtual string DtdName
        {
            get { return m_dtdName; }
            set
            {
                CheckMutable();

                if (value != null && value.Length == 0)
                {
                    value = null;
                }

                if (value != null && !XmlHelper.IsPublicIdentifierValid(value))
                {
                    throw new ArgumentException("illegal xml dtd public id: " + value);
                }

                m_dtdName = value;
            }
        }

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
        public virtual string Encoding
        {
            get { return m_encoding; }
            set
            {
                CheckMutable();

                if (value != null && value.Length == 0)
                {
                    value = null;
                }

                if (value != null && !XmlHelper.IsEncodingValid(value))
                {
                    throw new ArgumentException("illegal xml document encoding: " + value);
                }

                m_encoding = value;
            }
        }

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
        public virtual string DocumentComment
        {
            get { return m_comment; }
            set
            {
                CheckMutable();

                if (value != null && value.Length == 0)
                {
                    value = null;
                }

                if (value != null && !XmlHelper.IsCommentValid(value))
                {
                    throw new ArgumentException("illegal xml comment: " + value);
                }

                m_comment = value;
            }
        }

        /// <summary>
        /// Write the XML document, including an XML header and DOCTYPE if
        /// one exists.
        /// </summary>
        /// <remarks>
        /// This overrides the contract of the IXmlElement super interface.
        /// </remarks>
        /// <param name="writer">
        /// A <b>TextWriter</b> object to use to write to.
        /// </param>
        /// <param name="isPretty">
        /// <b>true</b> to specify that the output is intended to be as human
        /// readable as possible.
        /// </param>
        public override void WriteXml(TextWriter writer, bool isPretty)
        {
            string dtdUri   = DtdUri;
            string dtdName  = DtdName;
            string encoding = Encoding;
            string comment  = DocumentComment;

            writer.Write("<?xml version='1.0'");

            if (!StringUtils.IsNullOrEmpty(encoding))
            {
                writer.Write(" encoding=" + XmlHelper.Quote(encoding));
            }

            writer.Write("?>");

            if (isPretty)
            {
                writer.WriteLine();
            }

            if (!StringUtils.IsNullOrEmpty(dtdUri))
            {
                writer.Write("<!DOCTYPE " + Name + ' ');

                if (!StringUtils.IsNullOrEmpty(dtdName))
                {
                    writer.Write("PUBLIC");

                    if (isPretty)
                    {
                        writer.WriteLine();
                    }

                    writer.Write(' ');
                    writer.Write(XmlHelper.Quote(dtdName));
                }
                else
                {
                    writer.Write("SYSTEM");
                }

                if (isPretty)
                {
                    writer.WriteLine();
                }

                writer.Write(' ');
                writer.Write(XmlHelper.Quote(XmlHelper.EncodeUri(dtdUri)));
                writer.Write('>');

                if (isPretty)
                {
                    writer.WriteLine();
                }
                else
                {
                    writer.Write(' ');
                }
            }

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

                if (isPretty)
                {
                    writer.WriteLine();
                }
                else
                {
                    writer.Write(' ');
                }
            }

            base.WriteXml(writer, isPretty);

            if (isPretty)
            {
                writer.WriteLine();
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
            if (m_dtdUri != null || m_dtdName != null || m_encoding != null || m_comment != null)
            {
                throw new Exception("deserialization not active");
            }

            base.ReadExternal(reader);

            if (reader.ReadBoolean(12))
            {
                m_dtdUri = reader.ReadString(13);
            }

            if (reader.ReadBoolean(14))
            {
                m_dtdName = reader.ReadString(15);
            }

            if (reader.ReadBoolean(16))
            {
                m_encoding = reader.ReadString(17);
            }

            if (reader.ReadBoolean(18))
            {
                m_comment = reader.ReadString(19);
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

            string dtdUri   = m_dtdUri;
            string dtdName  = m_dtdName;
            string encoding = m_encoding;
            string comment  = m_comment;

            bool hasDtdUri   = dtdUri   != null;
            bool hasDtdName  = dtdName  != null;
            bool hasEncoding = encoding != null;
            bool hasComment  = comment  != null;

            writer.WriteBoolean(12, hasDtdUri);
            if (hasDtdUri)
            {
                writer.WriteString(13, m_dtdUri);
            }

            writer.WriteBoolean(14, hasDtdName);
            if (hasDtdName)
            {
                writer.WriteString(15, m_dtdName);
            }

            writer.WriteBoolean(16, hasEncoding);
            if (hasEncoding)
            {
                writer.WriteString(17, m_encoding);
            }

            writer.WriteBoolean(18, hasComment);
            if (hasComment)
            {
                writer.WriteString(19, m_comment);
            }
        }

        #endregion

        #region Object methods

        /// <summary>
        /// Provide a hash value for this XML document and all of its
        /// contained information.
        /// </summary>
        /// <remarks>
        /// Note that this overrides the contract of the GetHashCode() method
        /// in the super interface IXmlElement. The hash value is defined as
        /// a xor of the following:
        /// <list type="number">
        /// <item>the GetHashCode() from the root element</item>
        /// <item>the GetHashCode() from the document type (uri and optional
        /// name)</item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// The hash value for this XML value.
        /// </returns>
        public override int GetHashCode()
        {
            int n = base.GetHashCode();

            string uri = DtdUri;
            if (!StringUtils.IsNullOrEmpty(uri))
            {
                n ^= uri.GetHashCode();

                string name = DtdName;
                if (!StringUtils.IsNullOrEmpty(name))
                {
                    n ^= name.GetHashCode();
                }
            }

            return n;
        }

        /// <summary>
        /// Compare this XML document and all of its contained information
        /// with another XML document for equality.
        /// </summary>
        /// <remarks>
        /// Note that this overrides the contract of the Equals() method in
        /// the super interface IXmlElement.
        /// </remarks>
        /// <param name="o">
        /// The object to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if the documents are equal, <b>false</b> otherwise.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is IXmlDocument)
            {
                IXmlDocument that = (IXmlDocument) o;
                if (!base.Equals(that))
                {
                    return false;
                }

                return Equals(DtdUri         , that.DtdUri         )
                    && Equals(DtdName        , that.DtdName        )
                    && Equals(Encoding       , that.Encoding       )
                    && Equals(DocumentComment, that.DocumentComment);
            }

            return false;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Validates that the element is mutable, otherwise throws an
        /// <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        protected override void CheckMutable()
        {
            if (!IsMutable)
            {
                throw new InvalidOperationException("document \"" + Name + "\" is not mutable");
            }
        }

        #endregion

        #region Data Members

        private string m_dtdUri;
        private string m_dtdName;
        private string m_encoding;
        private string m_comment;

        #endregion
    }
}
