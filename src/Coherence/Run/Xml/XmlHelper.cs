/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

using Tangosol.IO.Resources;
using Tangosol.Net;
using Tangosol.Util;

namespace Tangosol.Run.Xml
{
    /// <summary>
    /// This abstract class contains XML manipulation methods.
    /// </summary>
    /// <author>Cameron Purdy  2000.10.25</author>
    /// <author>Gene Gleyzer  2000.10.25</author>
    /// <author>Ana Cikic  2009.08.26</author>
    public abstract class XmlHelper
    {
        #region Xml loading helpers

        /// <summary>
        /// Load XML from a resource specified by path.
        /// </summary>
        /// <param name="path">
        /// Location of Xml data; an URL or valid path.
        /// </param>
        /// <returns>
        /// The XML content.
        /// </returns>
        public static IXmlDocument LoadXml(string path)
        {
            return LoadXml(ResourceLoader.GetResource(path));
        }

        /// <summary>
        /// Load XML from a given <see cref="IResource"/>.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <returns>
        /// The XML content.
        /// </returns>
        public static IXmlDocument LoadXml(IResource resource)
        {
            return new SimpleParser().ParseXml(resource);
        }

        /// <summary>
        /// Load XML from a reader.
        /// </summary>
        /// <param name="reader">
        /// The <b>TextReader</b> object.
        /// </param>
        /// <returns>
        /// The XML content.
        /// </returns>
        public static IXmlDocument LoadXml(TextReader reader)
        {
            return new SimpleParser().ParseXml(reader);
        }

        /// <summary>
        /// Load XML from a stream.
        /// </summary>
        /// <param name="stream">
        /// The <b>Stream</b> object.
        /// </param>
        /// <returns>
        /// The XML content.
        /// </returns>
        public static IXmlDocument LoadXml(Stream stream)
        {
            return new SimpleParser().ParseXml(stream);
        }

        /// <summary>
        /// Load XML from a stream using the specified encoding.
        /// </summary>
        /// <param name="stream">
        /// The <b>Stream</b> object.
        /// </param>
        /// <param name="encoding">
        /// Encoding.
        /// </param>
        /// <returns>
        /// The XML content.
        /// </returns>
        public static IXmlDocument LoadXml(Stream stream, Encoding encoding)
        {
            return new SimpleParser().ParseXml(stream, encoding);
        }

        /// <summary>
        /// Load an XML configuration from a resource.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <param name="description">
        /// A description of the resource being loaded (e.g. "cache
        /// configuration"). The description is only used in logging and error
        /// messages related to loading the resource.
        /// </param>
        /// <returns>
        /// The configuration XML.
        /// </returns>
        public static IXmlDocument LoadResource(IResource resource, string description)
        {
            if (description == null)
            {
                // default to something meaningless and generic
                description = "configuration";
            }
            try
            {
                var config = LoadXml(resource);
                CacheFactory.Log("Loaded " + description + " from \""
                        + resource + '"', CacheFactory.LogLevel.Debug);
                return config;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load " + description 
                        + ": " + resource, e);
            }
        }

        #endregion

        #region Convert XmlDocument to IXmlDocument

        /// <summary>
        /// Converts specified <b>System.Xml.XmlDocument</b> into
        /// <see cref="IXmlDocument"/>.
        /// </summary>
        /// <param name="xmlDoc">
        /// Source <b>XmlDocument</b>.
        /// </param>
        /// <returns>
        /// <b>IXmlDocument</b> that is the result of conversion.
        /// </returns>
        public static IXmlDocument ConvertDocument(XmlDocument xmlDoc)
        {
            IXmlDocument resultDoc = null;

            if (xmlDoc != null)
            {
                resultDoc = new SimpleDocument();

                foreach (XmlNode child in xmlDoc.ChildNodes)
                {
                    switch (child.NodeType)
                    {
                        case XmlNodeType.XmlDeclaration:
                            XmlDeclaration xmlDecl = (XmlDeclaration) child;
                            resultDoc.Encoding = xmlDecl.Encoding;
                            break;

                        case XmlNodeType.DocumentType:
                            XmlDocumentType xmlDocType = xmlDoc.DocumentType;
                            resultDoc.DtdName = xmlDocType.PublicId;
                            resultDoc.DtdUri  = xmlDocType.SystemId;
                            break;

                        case XmlNodeType.Comment:
                            XmlComment xmlComment = (XmlComment) child;
                            resultDoc.DocumentComment = xmlComment.Value;
                            break;

                        case XmlNodeType.Element:
                            XmlElement xmlRoot = xmlDoc.DocumentElement;
                            ConvertElement(xmlRoot, resultDoc, null);
                            break;

                        default:
                            // ignore
                            break;
                    }
                }
            }

            return resultDoc;
        }

        /// <summary>
        /// Converts specified <b>System.Xml.XmlElement</b> into
        /// <see cref="IXmlElement"/>.
        /// </summary>
        /// <param name="source">
        /// Source <b>XmlDocument</b>; must not be <c>null</c>.
        /// </param>
        /// <param name="target">
        /// <b>IXmlElement</b> that will be the result of conversion. If it
        /// is <c>null</c>, new instance of <b>IXmlElement</b> will be
        /// created.
        /// </param>
        /// <param name="parent">
        /// Parent of target <b>IXmlElement</b>; may be <c>null</c>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static void ConvertElement(XmlElement source, IXmlElement target, IXmlElement parent)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            string elementName = source.LocalName;
            if (target == null)
            {
                if (parent == null)
                {
                    target = new SimpleElement(elementName);
                }
                else
                {
                    target = parent.AddElement(elementName);
                }
            }
            else
            {
                target.Name = elementName;
            }

            if (target != null)
            {
                foreach (XmlAttribute attr in source.Attributes)
                {
                    target.AddAttribute(attr.LocalName).SetString(attr.Value);
                }

                StringBuilder value       = new StringBuilder(target.GetString());
                StringBuilder comment     = new StringBuilder(target.Comment);
                IList         elementList = new ArrayList();

                foreach (XmlNode child in source.ChildNodes)
                {
                    switch (child.NodeType)
                    {
                        case XmlNodeType.Comment:
                            XmlComment xmlComment = (XmlComment) child;

                            if (StringUtils.IsNullOrEmpty(comment.ToString()))
                            {
                                comment.Append(xmlComment.Value);
                            }
                            else
                            {
                                comment.Append('\n').Append(xmlComment.Value);
                            }
                            break;

                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                            value.Append(child.Value);
                            break;

                        case XmlNodeType.Element:
                            elementList.Add(child as XmlElement);
                            break;

                        default:
                            // ignore
                            break;
                    }
                }

                target.Comment = comment.ToString();
                target.SetString(value.ToString());
                foreach (XmlElement element in elementList)
                {
                    ConvertElement(element, null, target);
                }
            }
        }

        #endregion

        #region Formatting helpers

        /// <summary>
        /// Validate the passed encoding.
        /// </summary>
        /// <remarks>
        /// Encodings are lating strings defined as:
        /// [A-Za-z] ([A-Za-z0-9._] | '-')*
        /// </remarks>
        /// <param name="encoding">
        /// The document encoding.
        /// </param>
        /// <returns>
        /// <b>true</b> if the encoding is valid, <b>false</b> otherwise.
        /// </returns>
        public static bool IsEncodingValid(string encoding)
        {
            if (encoding == null)
            {
                return false;
            }

            char[] ach = encoding.ToCharArray();
            int    cch = ach.Length;

            if (cch == 0)
            {
                return false;
            }

            char ch = ach[0];
            if (!(ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z'))
            {
                return false;
            }

            for (int of = 1; of < cch; ++of)
            {
                ch = ach[of];
                switch (ch)
                {
                    case 'A': case 'B': case 'C': case 'D': case 'E': case 'F':
                    case 'G': case 'H': case 'I': case 'J': case 'K': case 'L':
                    case 'M': case 'N': case 'O': case 'P': case 'Q': case 'R':
                    case 'S': case 'T': case 'U': case 'V': case 'W': case 'X':
                    case 'Y': case 'Z':

                    case 'a': case 'b': case 'c': case 'd': case 'e': case 'f':
                    case 'g': case 'h': case 'i': case 'j': case 'k': case 'l':
                    case 'm': case 'n': case 'o': case 'p': case 'q': case 'r':
                    case 's': case 't': case 'u': case 'v': case 'w': case 'x':
                    case 'y': case 'z':

                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9':

                    // other legal characters
                    case '.':
                    case '_':
                    case '-':
                        break;

                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validate the passed system identifier.
        /// </summary>
        /// <param name="name">
        /// The system identifier of the XML document.
        /// </param>
        /// <returns>
        /// <b>true</b> if the identifier is valid, <b>false</b> otherwise.
        /// </returns>
        public static bool IsSystemIdentifierValid(string name)
        {
            return true;
        }

        /// <summary>
        /// Validate the passed public identifier.
        /// </summary>
        /// <param name="name">
        /// The public identifier of the XML document.
        /// </param>
        /// <returns>
        /// <b>true</b> if the identifier is valid, <b>false</b> otherwise.
        /// </returns>
        public static bool IsPublicIdentifierValid(string name)
        {
            // PubidLiteral ::= '"' PubidChar* '"' | "'" (PubidChar - "'")* "'"
            // PubidChar    ::= #x20 | #xD | #xA | [a-zA-Z0-9] | [-'()+,./:=?;!*#@$_%]
            char[] ach = name.ToCharArray();
            int    cch = ach.Length;
            for (int of = 0; of < cch; ++of)
            {
                switch (ach[of])
                {
                    case ' ' : //0x20
                    case '\r': //0x0D
                    case '\n': //0x0A

                    case 'A': case 'B': case 'C': case 'D': case 'E': case 'F':
                    case 'G': case 'H': case 'I': case 'J': case 'K': case 'L':
                    case 'M': case 'N': case 'O': case 'P': case 'Q': case 'R':
                    case 'S': case 'T': case 'U': case 'V': case 'W': case 'X':
                    case 'Y': case 'Z':

                    case 'a': case 'b': case 'c': case 'd': case 'e': case 'f':
                    case 'g': case 'h': case 'i': case 'j': case 'k': case 'l':
                    case 'm': case 'n': case 'o': case 'p': case 'q': case 'r':
                    case 's': case 't': case 'u': case 'v': case 'w': case 'x':
                    case 'y': case 'z':

                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9':

                    case '-': case '\'':case '(': case ')': case '+':
                    case ',': case '.': case '/': case ':': case '=':
                    case '?': case ';': case '!': case '*': case '#':
                    case '@': case '$': case '_': case '%':
                        break;

                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validate the passed comment.
        /// </summary>
        /// <remarks>
        /// Comments may not contain "--". See the XML specification 1.0 2ed
        /// section 2.5.
        /// </remarks>
        /// <param name="comment">
        /// The XML comment.
        /// </param>
        /// <returns>
        /// <b>true</b> if the comment is valid, <b>false</b> otherwise.
        /// </returns>
        public static bool IsCommentValid(string comment)
        {
            return comment.IndexOf("--") == -1;
        }

        /// <summary>
        /// Validate the passed name.
        /// </summary>
        /// <remarks>
        /// Currently, this does not allow the "CombiningChar" or "Extender"
        /// characters that are allowed by the XML specification 1.0 2ed
        /// section 2.3 [4].
        /// </remarks>
        /// <param name="name">
        /// The XML name to validate.
        /// </param>
        /// <returns>
        /// <b>true</b> if the name is valid, <b>false</b> otherwise.
        /// </returns>
        public static bool IsNameValid(string name)
        {
            if (name == null)
            {
                return false;
            }

            char[] ach = name.ToCharArray();
            int    cch = ach.Length;

            if (cch == 0)
            {
                return false;
            }

            char ch = ach[0];
            if (!(char.IsLetter(ch) || ch == '_' || ch == ':'))
            {
                return false;
            }

            for (int of = 1; of < cch; ++of)
            {
                ch = ach[of];
                switch (ch)
                {
                    // inline latin uppercase/lowercase letters and digits
                    case 'A': case 'B': case 'C': case 'D': case 'E': case 'F':
                    case 'G': case 'H': case 'I': case 'J': case 'K': case 'L':
                    case 'M': case 'N': case 'O': case 'P': case 'Q': case 'R':
                    case 'S': case 'T': case 'U': case 'V': case 'W': case 'X':
                    case 'Y': case 'Z':

                    case 'a': case 'b': case 'c': case 'd': case 'e': case 'f':
                    case 'g': case 'h': case 'i': case 'j': case 'k': case 'l':
                    case 'm': case 'n': case 'o': case 'p': case 'q': case 'r':
                    case 's': case 't': case 'u': case 'v': case 'w': case 'x':
                    case 'y': case 'z':

                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9':

                    // other legal characters
                    case '.':
                    case '-':
                    case '_':
                    case ':':
                        break;

                    default:
                        if (!(char.IsLetter(ch) || char.IsDigit(ch)))
                        {
                            return false;
                        }
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Test if the specified character is XML whitespace.
        /// </summary>
        /// <param name="ch">
        /// A character.
        /// </param>
        /// <returns>
        /// <b>true</b> if the passed character is XML whitespace.
        /// </returns>
        public static bool IsWhitespace(char ch)
        {
            switch (ch)
            {
                case '\t': //0x09
                case '\n': //0x0A
                case '\r': //0x0D
                case ' ' : //0x20
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Trim XML whitespace.
        /// </summary>
        /// <remarks>
        /// See XML 1.0 2ed section 2.3.
        /// </remarks>
        /// <param name="s">
        /// The original string.
        /// </param>
        /// <returns>
        /// The passed string minus any leading or trailing whitespace.
        /// </returns>
        public static string Trim(string s)
        {
            char[] ach     = s.ToCharArray();
            int    cch     = ach.Length;
            int    ofStart = 0;
            int    ofEnd   = cch;

            while (ofStart < cch && IsWhitespace(ach[ofStart]))
            {
                ++ofStart;
            }

            if (ofStart >= cch)
            {
                return "";
            }

            while (IsWhitespace(ach[ofEnd - 1]))
            {
                --ofEnd;
            }

            return ofStart > 0 || ofEnd < cch ? s.Substring(ofStart, ofEnd - ofStart) : s;
        }

        /// <summary>
        /// Trim leading XML whitespace.
        /// </summary>
        /// <remarks>
        /// See XML 1.0 2ed section 2.3.
        /// </remarks>
        /// <param name="s">
        /// The original string.
        /// </param>
        /// <returns>
        /// The passed string minus any leading whitespace.
        /// </returns>
        public static string Trimf(string s)
        {
            char[] ach = s.ToCharArray();
            int    cch = ach.Length;
            int    of  = 0;

            while (of < cch && IsWhitespace(ach[of]))
            {
                ++of;
            }

            if (of >= cch)
            {
                return "";
            }

            if (of == 0)
            {
                return s;
            }

            return s.Substring(of);
        }

        /// <summary>
        /// Trim trailing XML whitespace.
        /// </summary>
        /// <remarks>
        /// See XML 1.0 2ed section 2.3.
        /// </remarks>
        /// <param name="s">
        /// The original string.
        /// </param>
        /// <returns>
        /// The passed string minus any trailing whitespace.
        /// </returns>
        public static string Trimb(string s)
        {
            char[] ach = s.ToCharArray();
            int    cch = ach.Length;
            int    of  = cch - 1;

            while (of >= 0 && IsWhitespace(ach[of]))
            {
                --of;
            }

            if (of < 0)
            {
                return "";
            }

            if (of == cch - 1)
            {
                return s;
            }

            return s.Substring(0, of + 1);
        }

        /// <summary>
        /// Encode an attribute value so that it can be quoted and made part
        /// of a valid and well formed XML document.
        /// </summary>
        /// <param name="value">
        /// The attribute value to encode.
        /// </param>
        /// <param name="chQuote">
        /// The character that will be used to quote the attribute.
        /// </param>
        /// <returns>
        /// The attribute value in its encoded form (but not quoted).
        /// </returns>
        public static string EncodeAttribute(string value, char chQuote)
        {
            if (!(chQuote == '\'' || chQuote == '"'))
            {
                throw new ArgumentException("Invalid quote character");
            }

            char[] ach = value.ToCharArray();
            int    cch = ach.Length;

            // check for an empty attribute value
            if (cch == 0)
            {
                return value;
            }

            StringBuilder sb     = null;
            int           ofPrev = 0;
            for (int of = 0; of < cch; ++of)
            {
                char ch = ach[of];
                switch (ch)
                {
                    // escape only the quote that is planned to be used
                    case '\'':
                    case '"':
                        if (ch != chQuote)
                        {
                            break;
                        }
                        goto case '&';

                    case (char) 0x00: case (char) 0x01: case (char) 0x02: case (char) 0x03:
                    case (char) 0x04: case (char) 0x05: case (char) 0x06: case (char) 0x07:
                    case (char) 0x08: case (char) 0x09: case (char) 0x0A: case (char) 0x0B:
                    case (char) 0x0C: case (char) 0x0D: case (char) 0x0E: case (char) 0x0F:
                    case (char) 0x10: case (char) 0x11: case (char) 0x12: case (char) 0x13:
                    case (char) 0x14: case (char) 0x15: case (char) 0x16: case (char) 0x17:
                    case (char) 0x18: case (char) 0x19: case (char) 0x1A: case (char) 0x1B:
                    case (char) 0x1C: case (char) 0x1D: case (char) 0x1E: case (char) 0x1F:

                    // characters that must be escaped
                    // see XML 1.0 2ed section 2.3[10]
                    case '<':
                    case '>':
                    case '&':
                    {
                        if (sb == null)
                        {
                            // pre-allocate enough for several escapes
                            sb = new StringBuilder(cch + 16);
                        }

                        // transfer up to but not including the current offset
                        if (of > ofPrev)
                        {
                            sb.Append(ach, ofPrev, of - ofPrev);
                        }

                        switch (ch)
                        {
                            case '>':
                                sb.Append("&gt;");
                                break;
                            case '<':
                                sb.Append("&lt;");
                                break;
                            case '&':
                                sb.Append("&amp;");
                                break;
                            case '\'':
                                sb.Append("&apos;");
                                break;
                            case '\"':
                                sb.Append("&quot;");
                                break;
                            default:
                                // encode the current character
                                sb.Append("&#x");
                                int n = ch;
                                if ((n & 0xF000) != 0)
                                {
                                    sb.Append(HEX[NumberUtils.URShift(n, 12)]);
                                }
                                if ((n & 0xFF00) != 0)
                                {
                                    sb.Append(HEX[NumberUtils.URShift(n, 8) & 0xF]);
                                }
                                if ((n & 0xFFF0) != 0)
                                {
                                    sb.Append(HEX[NumberUtils.URShift(n, 4) & 0xF]);
                                }
                                sb.Append(HEX[n & 0xF]);
                                sb.Append(';');
                                break;
                        }

                        // the next character in the string is now the next
                        // character to transfer/encode
                        ofPrev = of + 1;
                        break;
                    }
                }
            }

            // there may be a portion of the string left that does not require
            // encoding
            if (sb != null && ofPrev < cch)
            {
                sb.Append(ach, ofPrev, cch - ofPrev);
            }

            return sb == null ? value : sb.ToString();
        }

        /// <summary>
        /// Decode an attribute value that was quoted.
        /// </summary>
        /// <param name="value">
        /// The attribute value to decode.
        /// </param>
        /// <returns>
        /// The attribute value in its decoded form.
        /// </returns>
        public static string DecodeAttribute(string value)
        {
            if (value.IndexOf('&') == -1)
            {
                return value;
            }

            char[] ach = value.ToCharArray();
            int    cch = ach.Length;

            StringBuilder sb     = new StringBuilder(cch);
            int           ofPrev = 0;
            for (int of = 0; of < cch; ++of)
            {
                if (ach[of] == '&')
                {
                    // scan up to ';'
                    int ofSemi = of + 1;
                    while (ofSemi < cch && ach[ofSemi] != ';')
                    {
                        ++ofSemi;
                    }
                    if (ofSemi >= cch || ofSemi == of + 1)
                    {
                        throw new ArgumentException("The XML attribute ("+ value + ") contains an unescaped '&'");
                    }

                    // transfer up to but not including the current offset
                    if (of > ofPrev)
                    {
                        sb.Append(ach, ofPrev, of - ofPrev);
                        ofPrev = of;
                    }

                    // convert the escaped sequence to a character, ignoring
                    // potential entity refs
                    if (ach[of + 1] == '#')
                    {
                        bool   isHex = (ach[of + 2] == 'x');
                        int    ofEsc = of + (isHex ? 3 : 2);
                        string esc   = value.Substring(ofEsc, ofSemi - ofEsc);
                        try
                        {
                            if (esc.Length < 1)
                            {
                                throw new ArgumentException("not a number");
                            }

                            int n = int.Parse(esc, (isHex ? NumberStyles.HexNumber : NumberStyles.Integer));
                            if (n < 0 || n > 0xFFFF)
                            {
                                throw new ArgumentException("out of range");
                            }

                            sb.Append((char) n);
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException("The XML attribute ("+ value + ") contains an illegal escape ("
                                    + (isHex ? "hex" : "decimal") + ' ' + esc + ')');
                        }
                    }
                    else
                    {
                        int    ofNext = of + 1;
                        string esc    = value.Substring(ofNext, ofSemi - ofNext);
                        if (esc.Equals("amp"))
                        {
                            sb.Append('&');
                        }
                        else if (esc.Equals("apos"))
                        {
                            sb.Append('\'');
                        }
                        else if (esc.Equals("gt"))
                        {
                            sb.Append('>');
                        }
                        else if (esc.Equals("lt"))
                        {
                            sb.Append('<');
                        }
                        else if (esc.Equals("quot"))
                        {
                            sb.Append('\"');
                        }
                        else
                        {
                            // assume it is an entity ref etc.
                            continue;
                        }
                    }

                    of     = ofSemi;
                    ofPrev = of + 1;
                }
            }

            if (ofPrev < cch)
            {
                sb.Append(ach, ofPrev, cch - ofPrev);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Encode an element's content value so that it can be made part of
        /// a valid and well formed XML document.
        /// </summary>
        /// <param name="value">
        /// The content value to encode.
        /// </param>
        /// <param name="preferBlockEscape">
        /// Pass <b>true</b> to use the CDATA escape if two conditions are
        /// met: that escaping is required, and that the value does not
        /// contain the string "]]&gt;".
        /// </param>
        /// <returns>
        /// The attribute value in its encoded form (but not quoted).
        /// </returns>
        public static string EncodeContent(string value, bool preferBlockEscape)
        {
            char[] ach = value.ToCharArray();
            int    cch = ach.Length;

            // check for an empty attribute value
            if (cch == 0)
            {
                return value;
            }

            if (preferBlockEscape)
            {
                // scan to see if any escape is necessary, and if so, use CDATA
                // if possible (content must not contain "]]>")
                bool useCdataEscape = true;
                bool requiresEscape = IsWhitespace(ach[0]) || IsWhitespace(ach[cch-1]);
                for (int of = 0; of < cch; ++of)
                {
                    int nch = ach[of];
                    switch (nch)
                    {
                        case '<':
                        case '&':
                            requiresEscape = true;
                            break;

                        case ']':
                            if (of + 2 < cch && ach[of+1] == ']' && ach[of+2] == '>')
                            {
                                useCdataEscape = false;
                            }
                            break;
                    }
                }

                if (!requiresEscape)
                {
                    return value;
                }

                if (useCdataEscape)
                {
                    return "<![CDATA[" + value + "]]>";
                }
            }

            StringBuilder sb = new StringBuilder(cch + 16);

            // encode leading whitespace
            int  off          = 0;
            bool isWhitespace = true;
            while (off < cch && isWhitespace)
            {
                switch (ach[off])
                {
                    case '\t':
                        sb.Append("&#x09;");
                        break;
                    case '\n':
                        sb.Append("&#x0A;");
                        break;
                    case '\r':
                        sb.Append("&#x0D;");
                        break;
                    case ' ':
                        sb.Append("&#x20;");
                        break;
                    default:
                        isWhitespace = false;
                        break;
                }
                if (isWhitespace)
                {
                    ++off;
                }
            }

            // figure out the extent of trailing whitespace
            int cchNonWhite = cch;
            while (cchNonWhite > off && IsWhitespace(ach[cchNonWhite-1]))
            {
                --cchNonWhite;
            }

            // encode portion between leading and trailing whitespace
            int ofPrev    = off;
            int cBrackets = 0;
            for (; off < cchNonWhite; ++off)
            {
                char ch = ach[off];
                switch (ch)
                {
                    case ']':
                        ++cBrackets;
                        break;

                    case '>':
                        if (cBrackets < 2)
                        {
                            cBrackets = 0;
                            break;
                        }
                        goto case '&';

                    case '<':
                    case '&':
                    {
                        // transfer up to but not including the current offset
                        if (off > ofPrev)
                        {
                            sb.Append(ach, ofPrev, off - ofPrev);
                        }

                        // escape the character
                        switch (ch)
                        {
                            case '>':
                                sb.Append("&gt;");
                                break;
                            case '<':
                                sb.Append("&lt;");
                                break;
                            case '&':
                                sb.Append("&amp;");
                                break;
                            default:
                                throw new Exception();
                        }

                        // the next character in the String is now the next
                        // character to transfer/encode
                        ofPrev = off + 1;

                        cBrackets = 0;
                        break;
                    }

                    default:
                        cBrackets = 0;
                        break;
                }
            }

            // there may be a portion of the string left that does not require
            // encoding
            if (ofPrev < cchNonWhite)
            {
                sb.Append(ach, ofPrev, cchNonWhite - ofPrev);
            }

            // encode trailing whitespace
            for (; off < cch; ++off)
            {
                switch (ach[off])
                {
                    case '\t':
                        sb.Append("&#x09;");
                        break;
                    case '\n':
                        sb.Append("&#x0A;");
                        break;
                    case '\r':
                        sb.Append("&#x0D;");
                        break;
                    case ' ':
                        sb.Append("&#x20;");
                        break;
                    default:
                        throw new Exception();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decode an element's content value.
        /// </summary>
        /// <param name="value">
        /// The content value to decode.
        /// </param>
        /// <returns>
        /// The attribute value in its decoded form.
        /// </returns>
        public static string DecodeContent(string value)
        {
            return DecodeAttribute(value);
        }

        /// <summary>
        /// Encode a System Identifier as per the XML 1.0 Specification
        /// second edition, section 4.2.2.
        /// </summary>
        /// <param name="uri">
        /// The URI to encode.
        /// </param>
        /// <returns>
        /// The encoded URI.
        /// </returns>
        public static string EncodeUri(string uri)
        {
            // From the XML 1.0 specification, section 4.2.2:
            //
            // URI references require encoding and escaping of certain
            // characters. The disallowed characters include all non-ASCII
            // characters, plus the excluded characters listed in Section
            // 2.4 of [IETF RFC 2396], except for the number sign (#) and
            // percent sign (%) characters and the square bracket characters
            // re-allowed in [IETF RFC 2732]. Disallowed characters must be
            // escaped as follows:
            //
            // Each disallowed character is converted to UTF-8 [IETF RFC 2279]
            // as one or more bytes.
            //
            // Any octets corresponding to a disallowed character are escaped
            // with the URI escaping mechanism (that is, converted to %HH,
            // where HH is the hexadecimal notation of the byte value).
            //
            // The original character is replaced by the resulting character
            // sequence.

            // determine if escaping is necessary
            char[] ach = uri.ToCharArray();
            int    cch = ach.Length;

            bool isEsc = false;
            for (int of = 0; of < cch && !isEsc; ++of)
            {
                char ch = ach[of];
                switch (ch)
                {
                    case '<': case '>': case '"': case '{': case '}':
                    case '|': case '\\':case '^': case '`': case '%':
                    case '\'': case ' ':
                        isEsc = true;
                        break;

                    default:
                        if (ch <= 0x1F || ch >= 0x7F)
                        {
                            isEsc = true;
                        }
                        break;
                }
            }

            if (!isEsc)
            {
                return uri;
            }

            // convert the UTF octets from bytes to chars
            byte[] bytes = Encoding.UTF8.GetBytes(ach);
            ach = Encoding.UTF8.GetString(bytes).ToCharArray();
            cch = ach.Length;

            StringBuilder sb = new StringBuilder(cch + 32);

            // scan for characters to escape
            int ofPrev = 0;
            for (int ofCur = ofPrev; ofCur < cch; ++ofCur)
            {
                char ch = ach[ofCur];
                switch (ch)
                {
                    default:
                        if (ch > 0x1F && ch < 0x7F)
                        {
                            break;
                        }
                        goto case '<';
                        // fall through

                    case '<': case '>': case '"': case '{': case '}':
                    case '|': case '\\':case '^': case '`': case '%':
                    case '\'':case ' ':
                    {
                        // copy up to this point
                        if (ofCur > ofPrev)
                        {
                            sb.Append(ach, ofPrev, ofCur - ofPrev);
                        }

                        // encode the character
                        sb.Append('%')
                          .Append(NumberUtils.ToHex(ch));

                        // processed up to the following character
                        ofPrev = ofCur + 1;
                        break;
                    }
                }
            }

            // copy the remainder of the string
            if (ofPrev < cch)
            {
                sb.Append(ach, ofPrev, cch - ofPrev);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decode a System Identifier as per the XML 1.0 Specification 2nd
        /// ed section 4.2.2.
        /// </summary>
        /// <param name="uri">
        /// The URI to decode.
        /// </param>
        /// <returns>
        /// The decoded URI.
        /// </returns>
        public static string DecodeUri(string uri)
        {
            string orig = uri;

            // scan for characters to unescape
            if (uri.IndexOf('%') != -1)
            {
                char[]        ach    = uri.ToCharArray();
                int           cch    = ach.Length;
                StringBuilder sb     = new StringBuilder(cch + 16);
                int           ofPrev = 0;
                for (int of = 0; of < cch; ++of)
                {
                    if (ach[of] == '%')
                    {
                        if (of + 2 >= cch)
                        {
                            throw new ArgumentException("The URI (" + orig + ") contains an unescaped '%'");
                        }

                        // transfer up to but not including the current offset
                        if (of > ofPrev)
                        {
                            sb.Append(ach, ofPrev, of - ofPrev);
                        }

                        // convert the escaped sequence to a character
                        try
                        {
                            int n = int.Parse(uri.Substring(of + 1, 2), NumberStyles.HexNumber);
                            if (n < 0 || n > 0xFF)
                            {
                                throw new ArgumentException("out of range");
                            }

                            sb.Append((char) n);
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException("The URI (" + orig + ") contains an illegal escape");
                        }

                        of += 2;
                        ofPrev = of + 1;
                    }
                }

                if (ofPrev < cch)
                {
                    sb.Append(ach, ofPrev, cch - ofPrev);
                }

                uri = sb.ToString();
            }

            return uri;
        }

        /// <summary>
        /// XML quote the passed string.
        /// </summary>
        /// <param name="s">
        /// The string to quote.
        /// </param>
        /// <returns>
        /// The quoted string.
        /// </returns>
        public static string Quote(string s)
        {
            return '\'' + EncodeAttribute(s, '\'') + '\'';
        }

        #endregion

        #region IXmlElement helpers

        /// <summary>
        /// Get the '/'-delimited path of the passed element starting from
        /// the root element.
        /// </summary>
        /// <param name="xml">
        /// An <see cref="IXmlElement"/>.
        /// </param>
        /// <returns>
        /// The path to the passed element in "absolute" format.
        /// </returns>
        public static string GetAbsolutePath(IXmlElement xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("element");
            }

            StringBuilder sb = new StringBuilder();
            do
            {
                sb.Insert(0, "/" + xml.Name);
                xml = xml.Parent;
            }
            while (xml != null);

            return sb.ToString();
        }

        /// <summary>
        /// Check whether or not this element or any of its children elements
        /// have any content such as values or attributes.
        /// </summary>
        /// <param name="xml">
        /// An <see cref="IXmlElement"/>.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the element itself and all of its children have
        /// neither values nor attributes.
        /// </returns>
        public static bool IsEmpty(IXmlElement xml)
        {
            if (!xml.IsEmpty)
            {
                return false;
            }

            IDictionary attrs = xml.Attributes;
            if (attrs.Count > 0)
            {
                return false;
            }

            IList listEl = xml.ElementList;
            if (listEl.Count > 0)
            {
                return false;
            }

            foreach (IXmlElement xmlEl in listEl)
            {
                if (!IsEmpty(xmlEl))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get a child element for the specified element.
        /// </summary>
        /// <remarks>
        /// If multiple child elements exist that have the specified name,
        /// then the behavior of this method is undefined, and it is
        /// permitted to return any one of the matching elements, to return
        /// <c>null</c>, or to throw an arbitrary runtime exception.
        /// </remarks>
        /// <param name="xml">
        /// An <see cref="IXmlElement"/>.
        /// </param>
        /// <param name="name">
        /// The name of the desired child element.
        /// </param>
        /// <returns>
        /// The specified element as an object implementing
        /// <b>IXmlElement</b>, or <c>null</c> if the specified child element
        /// does not exist.
        /// </returns>
        public static IXmlElement GetElement(IXmlElement xml, string name)
        {
            if (xml == null || name == null || !IsNameValid(name))
            {
                throw new ArgumentException("Null element or invalid name");
            }

            IList list = xml.ElementList;
            if (list.Count == 0)
            {
                return null;
            }

            foreach (IXmlElement xmlEl in list)
            {
                if (xmlEl.Name.Equals(name))
                {
                    return xmlEl;
                }
            }

            return null;
        }

        /// <summary>
        /// Find a child element with the specified '/'-delimited path.
        /// </summary>
        /// <remarks>
        /// The path format is based on a subset of the XPath specification,
        /// supporting:
        /// <list type="bullet">
        /// <item>Leading '/' to specify root</item>
        /// <item>Use of '/' as a path delimiter</item>
        /// <item>Use of '..' to specify parent</item>
        /// </list>
        /// If multiple child elements exist that have the specified name,
        /// then the behavior of this method is undefined, and it is
        /// permitted to return any one of the matching elements, to return
        /// <c>null</c>, or to throw an arbitrary runtime exception.
        /// </remarks>
        /// <param name="xml">
        /// An <see cref="IXmlElement"/>.
        /// </param>
        /// <param name="path">
        /// The path to follow to find the desired XML element.
        /// </param>
        /// <returns>
        /// The child element with the specified path or <c>null</c> if such
        /// a child element does not exist.
        /// </returns>
        public static IXmlElement FindElement(IXmlElement xml, string path)
        {
            if (xml == null || path == null)
            {
                throw new ArgumentNullException("element or path");
            }

            if (path.StartsWith("/"))
            {
                xml = xml.Root;
            }

            string[] tokens = path.Split('/');
            for (int i = 0; i < tokens.Length && xml != null; i++ )
            {
                string name = tokens[i];
                // in case of path that begins with '/', first element of tokens array
                // will be empty string
                if (name.Length > 0)
                {
                    if (name.Equals(".."))
                    {
                        xml = xml.Parent;
                        if (xml == null)
                        {
                            throw new ArgumentException("Invalid path " + path);
                        }
                    }
                    else
                    {
                        xml = xml.GetElement(name);
                    }
                }
            }

            return xml;
        }

        /// <summary>
        /// Find a child element with the specified '/'-delimited path and
        /// the specified value.
        /// </summary>
        /// <remarks>
        /// The path format is based on a subset of the XPath specification,
        /// supporting:
        /// <list type="bullet">
        /// <item>Leading '/' to specify root</item>
        /// <item>Use of '/' as a path delimiter</item>
        /// <item>Use of '..' to specify parent</item>
        /// </list>
        /// If multiple child elements exist that have the specified name and
        /// value, then this method returns any one of the matching elements.
        /// </remarks>
        /// <param name="xml">
        /// An <see cref="IXmlElement"/>.
        /// </param>
        /// <param name="path">
        /// The path to follow to find the desired XML element.
        /// </param>
        /// <param name="value">
        /// The value to match.
        /// </param>
        /// <returns>
        /// The child element with the specified path and value or
        /// <c>null</c> if the such a child element does not exist.
        /// </returns>
        public static IXmlElement FindElement(IXmlElement xml, string path, object value)
        {
            if (xml == null || path == null)
            {
                throw new ArgumentNullException("element or path");
            }

            while (path.StartsWith("/"))
            {
                xml  = xml.Root;
                path = path.Substring(1);
            }

            string name;
            int    ofNext = path.IndexOf("/");
            if (ofNext == -1)
            {
                name = path;
                path = "";
            }
            else
            {
                name = path.Substring(0, ofNext);
                path = path.Substring(ofNext + 1);

                while (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }
            }

            if (path.Length == 0)
            {
                if (name.Equals(".."))
                {
                    xml = xml.Parent;
                    return xml != null && Equals(xml.Value, value) ? xml : null;
                }
                else
                {
                    for (IEnumerator elEnum = xml.GetElements(name); elEnum.MoveNext(); )
                    {
                        xml = (IXmlElement) elEnum.Current;
                        if (Equals(xml.Value, value))
                        {
                            return xml;
                        }
                    }
                    return null;
                }
            }
            else
            {
                if (name.Equals(".."))
                {
                    xml = xml.Parent;
                    return xml == null ? null : FindElement(xml, path, value);
                }
                else
                {
                    for (IEnumerator elEnum = xml.GetElements(name); elEnum.MoveNext(); )
                    {
                        xml = FindElement((IXmlElement) elEnum.Current, path, value);

                        if (xml != null)
                        {
                            return xml;
                        }
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Ensure that a child element exists.
        /// </summary>
        /// <remarks>
        /// If any part of the path does not exist create new child
        /// elements to match the path.
        /// </remarks>
        /// <param name="xml">
        /// An XML element.
        /// </param>
        /// <param name="path">
        /// Element path.
        /// </param>
        /// <returns>
        /// The existing or new <see cref="IXmlElement"/> object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the name is <c>null</c> or if any part of the path is not a
        /// legal XML tag name.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If any element in the path is immutable or otherwise can not add
        /// a child element.
        /// </exception>
        /// <seealso cref="FindElement(IXmlElement, string)"/>
        public static IXmlElement EnsureElement(IXmlElement xml, string path)
        {
            if (xml == null || path == null)
            {
                throw new ArgumentNullException("element or path");
            }

            if (path.StartsWith("/"))
            {
                xml = xml.Root;
            }

            string[] tokens = path.Split('/');
            foreach (string name in tokens)
            {
                // in case of path that begins with '/', first element of tokens array
                // will be empty string
                if (name.Length > 0)
                {
                    if (name.Equals(".."))
                    {
                        xml = xml.Parent;
                        if (xml == null)
                        {
                            throw new ArgumentException("Invalid path " + path);
                        }
                    }
                    else
                    {
                        IXmlElement child = xml.GetElement(name);
                        xml = child == null ? xml.AddElement(name) : child;
                    }
                }
            }

            return xml;
        }

        /// <summary>
        /// Add the elements from the <b>IEnumerator</b> to the passed XML.
        /// </summary>
        /// <param name="xml">
        /// An <see cref="IXmlElement"/> object to add to.
        /// </param>
        /// <param name="enumerator">
        /// An <b>IEnumerator</b> of zero or more <b>IXmlElement</b> objects
        /// to add.
        /// </param>
        public static void AddElements(IXmlElement xml, IEnumerator enumerator)
        {
            IList list = xml.ElementList;
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Current);
            }
        }

        /// <summary>
        /// Remove all immediate child elements with the given name.
        /// </summary>
        /// <param name="xml">
        /// An <see cref="IXmlElement"/>.
        /// </param>
        /// <param name="name">
        /// Child element name.
        /// </param>
        /// <returns>
        /// The number of removed child elements.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the element is immutable or otherwise cannot remove a child
        /// element.
        /// </exception>
        public static int RemoveElement(IXmlElement xml, string name)
        {
            IList elToRemove = new ArrayList();

            foreach (IXmlElement xmlEl in xml.ElementList)
            {
                if (xmlEl.Name.Equals(name))
                {
                    elToRemove.Add(xmlEl);
                }
            }
            CollectionUtils.RemoveAll(xml.ElementList, elToRemove);
            return elToRemove.Count;
        }

        /// <summary>
        /// Replace a child element with the same name as the specified
        /// element.
        /// </summary>
        /// <remarks>
        /// If the child element does not exist the specified element is just
        /// added.
        /// </remarks>
        /// <param name="xmlParent">
        /// Parent <see cref="IXmlElement"/>.
        /// </param>
        /// <param name="xmlReplace">
        /// Element to replace with.
        /// </param>
        /// <returns>
        /// <b>true</b> if matching child element has been found and
        /// replaced; <b>false</b> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the parent element is immutable or otherwise cannot remove a
        /// child element.
        /// </exception>
        public static bool ReplaceElement(IXmlElement xmlParent, IXmlElement xmlReplace)
        {
            IList list = xmlParent.ElementList;

            IXmlElement xmlToReplace = null;
            foreach (IXmlElement xml in list)
            {
                if (xml.Name.Equals(xmlReplace.Name))
                {
                    xmlToReplace = xml;
                    break;
                }
            }

            if (xmlToReplace == null)
            {
                list.Add(xmlReplace.Clone());
                return false;
            }
            else
            {
                list[list.IndexOf(xmlToReplace)] = xmlReplace.Clone();
                return true;
            }
        }

        /// <summary>
        /// Override the values of the specified base element with values
        /// from the specified override element.
        /// </summary>
        /// <remarks>
        /// The values are only overriden if there is an exact match between
        /// the element paths and all attribute values. Empty override values
        /// are ignored. Override elements that do not match any of the base
        /// elements are just copied over. No ambiguity is allowed.<br/>
        /// For example, if the base element has more then one child with the
        /// same name and attributes then the override is not allowed.
        /// </remarks>
        /// <param name="xmlBase">
        /// Base <see cref="IXmlElement"/>.
        /// </param>
        /// <param name="xmlOverride">
        /// Override <b>IXmlElement</b>.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If the base element is immutable or there is ambiguity between
        /// the override and base elements.
        /// </exception>
        public static void OverrideElement(IXmlElement xmlBase, IXmlElement xmlOverride)
        {
            OverrideElement(xmlBase, xmlOverride, null);
        }

        /// <summary>
        /// Override the values of the specified base element with values
        /// from the specified override element.
        /// </summary>
        /// <remarks>
        /// The values are only overriden if there is an exact match between
        /// the element paths and an attribute value for the specified
        /// attribute name. Empty override values are ignored. Override
        /// elements that do not match any of the base elements are just
        /// copied over. No ambiguity is allowed.<br/>
        /// For example, if the base element has more then one child with the
        /// same name and the specified attribute's value then the override
        /// is not allowed.
        /// </remarks>
        /// <param name="xmlBase">
        /// Base <see cref="IXmlElement"/>.
        /// </param>
        /// <param name="xmlOverride">
        /// Override <b>IXmlElement</b>.
        /// </param>
        /// <param name="idAttrName">
        /// Attribute name that serves as an identifier allowing to match
        /// elements with the same name; if not specified all attributes have
        /// to match for an override.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If the base element is immutable or there is ambiguity between
        /// the override and base elements.
        /// </exception>
        public static void OverrideElement(IXmlElement xmlBase, IXmlElement xmlOverride, string idAttrName)
        {
            foreach (IXmlElement xmlOver in xmlOverride.ElementList)
            {
                if (IsEmpty(xmlOver))
                {
                    continue;
                }

                string name   = xmlOver.Name;
                object attrId = idAttrName == null ? (object) xmlOver.Attributes : xmlOver.GetAttribute(idAttrName);

                // ensure uniqueness
                for (IEnumerator enumerator = xmlOverride.GetElements(name); enumerator.MoveNext(); )
                {
                    IXmlElement xmlTest = (IXmlElement) enumerator.Current;
                    if (xmlTest != xmlOver)
                    {
                        object attrTest = idAttrName == null ? (object) xmlTest.Attributes : xmlTest.GetAttribute(idAttrName);
                        if (Equals(attrTest, attrId))
                        {
                            throw new InvalidOperationException("Override element is not unique:\n" + xmlOver);
                        }
                    }
                }

                // find matching base element
                IXmlElement xmlMatch = null;
                for (IEnumerator enumBase = xmlBase.GetElements(name); enumBase.MoveNext(); )
                {
                    IXmlElement xmlTest = (IXmlElement) enumBase.Current;

                    object attrTest = idAttrName == null ? (object) xmlTest.Attributes : xmlTest.GetAttribute(idAttrName);
                    if (Equals(attrTest, attrId))
                    {
                        if (xmlMatch == null)
                        {
                            xmlMatch = xmlTest;
                        }
                        else
                        {
                            throw new InvalidOperationException("Override element is ambiguous:\n" + xmlOver);
                        }
                    }
                }

                if (xmlMatch == null)
                {
                    // no match; append to the base
                    xmlBase.ElementList.Add(xmlOver.Clone());
                }
                else
                {
                    // replace the value, if present
                    if (xmlOver.Value != null)
                    {
                        xmlMatch.SetString(xmlOver.GetString());
                    }

                    // and repeat for all the children
                    OverrideElement(xmlMatch, xmlOver, idAttrName);
                }
            }
        }

        #endregion

        #region Namespace suport helpers

        /// <summary>
        /// Retrieve the Namespace URI for a given prefix in a context of the
        /// specified <see cref="IXmlElement"/>.
        /// </summary>
        /// <param name="xml">
        /// The <b>IXmlElement</b>.
        /// </param>
        /// <param name="prefix">
        /// The Namespace prefix.
        /// </param>
        /// <returns>
        /// The Namespace URI corresponding to the prefix.
        /// </returns>
        public static string GetNamespaceUri(IXmlElement xml, string prefix)
        {
            string attrName = "xmlns:" + prefix;

            while (xml != null)
            {
                IXmlValue attrXmlns = xml.GetAttribute(attrName);
                if (attrXmlns != null)
                {
                    return attrXmlns.GetString();
                }
                xml = xml.Parent;
            }
            return null;
        }

        /// <summary>
        /// Retrieve the Namespace prefix for a given URI in a context of the
        /// specified <see cref="IXmlElement"/>.
        /// </summary>
        /// <param name="xml">
        /// The <b>IXmlElement</b>.
        /// </param>
        /// <param name="uri">
        /// The Namespace URI.
        /// </param>
        /// <returns>
        /// The Namespace prefix corresponding to the URI.
        /// </returns>
        public static string GetNamespacePrefix(IXmlElement xml, string uri)
        {
            while (xml != null)
            {
                foreach (DictionaryEntry entry in xml.Attributes)
                {
                    if (uri.Equals(((IXmlValue) entry.Value).GetString()))
                    {
                        string attr = (string) entry.Key;
                        if (attr.StartsWith("xmlns:"))
                        {
                            return attr.Substring(6); // "xmlns:".Length
                        }
                    }
                }
                xml = xml.Parent;
            }
            return null;
        }

        /// <summary>
        /// Ensure the existence of the Namespace declaration attribute in a
        /// context of the specified <see cref="IXmlElement"/>.
        /// </summary>
        /// <param name="xml">
        /// The <b>IXmlElement</b>.
        /// </param>
        /// <param name="prefix">
        /// The Namespace prefix.
        /// </param>
        /// <param name="uri">
        /// The Namespace URI.
        /// </param>
        public static void EnsureNamespace(IXmlElement xml, string prefix, string uri)
        {
            string nmsUri = GetNamespaceUri(xml, prefix);
            if (nmsUri == null)
            {
                xml.AddAttribute("xmlns:" + prefix).SetString(uri);
            }
            else if (!nmsUri.Equals(uri))
            {
                throw new InvalidOperationException("Namespace conflict: prefix=" + prefix +
                    ", current URI=" + nmsUri + ", new URI=" + uri);
            }
        }

        /// <summary>
        /// Return a universal XML element name.
        /// </summary>
        /// <param name="local">
        /// The local XML element name.
        /// </param>
        /// <param name="prefix">
        /// The Namespace prefix.
        /// </param>
        /// <returns>
        /// The universal XML element name.
        /// </returns>
        /// <seealso href="http://www.jclark.com/xml/xmlns.htm"/>
        public static string GetUniversalName(string local, string prefix)
        {
            return prefix == null || local == null || local.Length == 0 ?
                local : prefix + ':' + local;
        }

        /// <summary>
        /// Check whether or not a universal (composite) name matches to the
        /// specified local name and Namespace URI.
        /// </summary>
        /// <param name="xml">
        /// The (context) <see cref="IXmlElement"/>.
        /// </param>
        /// <param name="name">
        /// The universal name.
        /// </param>
        /// <param name="local">
        /// The local xml name.
        /// </param>
        /// <param name="uri">
        /// The Namespace URI.
        /// </param>
        /// <returns>
        /// <b>true</b> if the specified element matches to the specified
        /// local name and the specified Namespace URI.
        /// </returns>
        public static bool IsNameMatch(IXmlElement xml, string name, string local, string uri)
        {
            if (uri == null)
            {
                return name.Equals(local);
            }
            else
            {
                string suffix = ':' + local;

                if (name.EndsWith(suffix))
                {
                    string prefix = name.Substring(0, name.Length - suffix.Length);
                    return uri.Equals(GetNamespaceUri(xml, prefix));
                }
                else
                {
                    // allow for "default" match
                    return name.Equals(local);
                }
            }
        }

        /// <summary>
        /// Check whether or not an element matches to the specified local
        /// name and Namespace URI.
        /// </summary>
        /// <param name="xml">
        /// The <see cref="IXmlElement"/>
        /// </param>
        /// <param name="local">
        /// The local xml name.
        /// </param>
        /// <param name="uri">
        /// The Namespace URI.
        /// </param>
        /// <returns>
        /// <b>true</b> if the specified element matches to the specified
        /// local name and the specified Namespace URI.
        /// </returns>
        public static bool IsElementMatch(IXmlElement xml, string local, string uri)
        {
            return IsNameMatch(xml, xml.Name, local, uri);
        }

        /// <summary>
        /// Get a child element of the specified <see cref="IXmlElement"/>
        /// that matches to the specified local name and the specified
        /// Namespace URI.
        /// </summary>
        /// <param name="xml">
        /// The parent <b>IXmlElement</b>.
        /// </param>
        /// <param name="local">
        /// The local xml name.
        /// </param>
        /// <param name="uri">
        /// The Namespace URI.
        /// </param>
        /// <returns>
        /// An element that matches to the specified local name and the
        /// specified Namespace URI.
        /// </returns>
        public static IXmlElement GetElement(IXmlElement xml, string local, string uri)
        {
            if (uri == null)
            {
                return xml.GetElement(local);
            }
            else
            {
                foreach (IXmlElement el in xml.ElementList)
                {
                    if (IsElementMatch(el, local, uri))
                    {
                        return el;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Get an attribute of the specified <see cref="IXmlElement"/> that
        /// matches to the specified local name and the specified Namespace
        /// URI.
        /// </summary>
        /// <param name="xml">
        /// The <b>IXmlElement</b>.
        /// </param>
        /// <param name="local">
        /// The local attribute name.
        /// </param>
        /// <param name="uri">
        /// The Namespace URI.
        /// </param>
        /// <returns>
        /// An <see cref="IXmlValue"/> that matches to the specified local
        /// name and the specified Namespace URI.
        /// </returns>
        public static IXmlValue GetAttribute(IXmlElement xml, string local, string uri)
        {
            if (uri == null)
            {
                return xml.GetAttribute(local);
            }
            else
            {
                foreach (DictionaryEntry entry in xml.Attributes)
                {
                    string attr = (string) entry.Key;

                    if (IsNameMatch(xml, attr, local, uri))
                    {
                        return (IXmlValue) entry.Value;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Get an <b>IEnumerator</b> of child elements of the specified
        /// <see cref="IXmlElement"/> that match to the specified local name
        /// and the specified Namespace URI.
        /// </summary>
        /// <param name="xml">
        /// The parent <b>IXmlElement</b>.
        /// </param>
        /// <param name="local">
        /// The local xml name.
        /// </param>
        /// <param name="uri">
        /// The Namespace URI.
        /// </param>
        /// <returns>
        /// An <b>IEnumerator</b> containing all matching child elements.
        /// </returns>
        public static IEnumerator GetElements(IXmlElement xml, string local, string uri)
        {
            if (uri == null)
            {
                return xml.GetElements(local);
            }
            else
            {
                IFilter filter = new ElementMatchFilter(local, uri);
                return new FilterEnumerator(xml.ElementList.GetEnumerator(), filter);
            }
        }

        /// <summary>
        /// For the specified <see cref="IXmlElement"/> purge the Namespace
        /// declarations that are declared somewhere up the xml tree.
        /// </summary>
        /// <param name="xml">
        /// The <b>IXmlElement</b>.
        /// </param>
        public static void PurgeNamespace(IXmlElement xml)
        {
            IXmlElement xmlParent = xml.Parent;
            if (xmlParent == null)
            {
                return;
            }

            IList entriesToRemove = new ArrayList();
            foreach (DictionaryEntry entry in xml.Attributes)
            {
                string attr = (string) entry.Key;
                if (attr.StartsWith("xmlns:"))
                {
                    string prefix = attr.Substring(6); // "xmlns:".Length
                    string uri    = ((IXmlValue) entry.Value).GetString();

                    if (uri.Equals(GetNamespaceUri(xmlParent, prefix)))
                    {
                        entriesToRemove.Add(entry);
                    }
                }
            }

            CollectionUtils.RemoveAll(xml.Attributes, entriesToRemove);
         }

        /// <summary>
        /// For the children elements of the specified
        /// <see cref="IXmlElement"/> purge the repetetive Namespace
        /// declarations.
        /// </summary>
        /// <param name="xml">
        /// The <b>IXmlElement</b>.
        /// </param>
        public static void PurgeChildrenNamespace(IXmlElement xml)
        {
            foreach (IXmlElement xmlEl in xml.ElementList)
            {
                PurgeNamespace(xmlEl);
            }
        }

        #endregion

        #region Misc helpers

        /// <summary>
        /// Parse the specified "init-params" element of the following
        /// structure:
        /// <pre>
        /// &lt;!ELEMENT init-params (init-param*)&gt;
        /// &lt;!ELEMENT init-param ((param-name | param-type), param-value,
        /// description?)&gt;
        /// </pre>
        /// into an object array.
        /// </summary>
        /// <remarks>
        /// For the purpose of this method only the parameters that have the
        /// "param-type" element specified are processed. The following types
        /// are supported:
        /// <list type="bullet">
        /// <item>string   (a.k.a. System.String)</item>
        /// <item>bool     (a.k.a. System.Boolean)</item>
        /// <item>int      (a.k.a. System.Int32)</item>
        /// <item>long     (a.k.a. System.Int64)</item>
        /// <item>float    (a.k.a. System.Single)</item>
        /// <item>double   (a.k.a. System.Double)</item>
        /// <item>decimal  (a.k.a. System.Decimal)</item>
        /// <item>file     (a.k.a. System.IO.File)</item>
        /// <item>date     (a.k.a. System.DateTime)</item>
        /// <item>time     (a.k.a. System.DateTime</item>
        /// <item>xml      (a.k.a. Tangosol.Run.Xml.IXmlElement)</item>
        /// </list>
        /// For any other [explicitly specified] types the corresponding
        /// "init-param" IXmlElement itself is placed into the returned
        /// array.
        /// </remarks>
        /// <param name="xmlParams">
        /// The "init-params" <b>IXmlElement</b> to parse.
        /// </param>
        /// <returns>
        /// An array of parameters.
        /// </returns>
        public static object[] ParseInitParams(IXmlElement xmlParams)
        {
            return ParseInitParams(xmlParams, null);
        }

        /// <summary>
        /// Parse the specified "init-params" element of the following
        /// structure:
        /// <pre>
        /// &lt;!ELEMENT init-params (init-param*)&gt;
        /// &lt;!ELEMENT init-param ((param-name | param-type), param-value,
        /// description?)&gt;
        /// </pre>
        /// into an object array.
        /// </summary>
        /// <remarks>
        /// For the purpose of this method only the parameters that have the
        /// "param-type" element specified are processed. The following types
        /// are supported:
        /// <list type="bullet">
        /// <item>string   (a.k.a. System.String)</item>
        /// <item>bool     (a.k.a. System.Boolean)</item>
        /// <item>int      (a.k.a. System.Int32)</item>
        /// <item>long     (a.k.a. System.Int64)</item>
        /// <item>double   (a.k.a. System.Double)</item>
        /// <item>float    (a.k.a. System.Single)</item>
        /// <item>decimal  (a.k.a. System.Decimal)</item>
        /// <item>file     (a.k.a. System.IO.File)</item>
        /// <item>date     (a.k.a. System.DateTime)</item>
        /// <item>time     (a.k.a. System.DateTime</item>
        /// <item>xml      (a.k.a. Tangosol.Run.Xml.IXmlElement)</item>
        /// </list>
        /// For any other [explicitly specified] types the corresponding
        /// "init-param" IXmlElement itself is placed into the returned
        /// array.
        /// </remarks>
        /// <param name="xmlParams">
        /// The "init-params" <b>IXmlElement</b> to parse.
        /// </param>
        /// <param name="resolver">
        /// An <see cref="IParameterResolver"/> to resolve "{macro}" values
        /// (optional).
        /// </param>
        /// <returns>
        /// An array of parameters.
        /// </returns>
        public static object[] ParseInitParams(IXmlElement xmlParams, IParameterResolver resolver)
        {
            IList listParam = new ArrayList();
            for (IEnumerator elEnum = xmlParams.GetElements("init-param"); elEnum.MoveNext();)
            {
                IXmlElement xmlParam = (IXmlElement) elEnum.Current;
                string      type     = xmlParam.GetSafeElement("param-type" ).GetString();
                IXmlElement xmlValue = xmlParam.GetSafeElement("param-value");
                string      value    = xmlValue.GetString();

                // resolver has a priority if the type is a "{macro}"
                // or the value contains a "{macro}"
                if (resolver != null &&
                  ((type.IndexOf('{') == 0 && type.LastIndexOf('}') == type.Length - 1)
                ||
                  (value.IndexOf('{') >= 0 && value.IndexOf('{') < value.LastIndexOf('}'))))
                {
                    object resolved = resolver.ResolveParameter(type, value);
                    if (resolved != UNRESOLVED)
                    {
                        listParam.Add(resolved);
                        continue;
                    }
                }

                XmlValueType expectedType = LookupXmlValueType(type);
                Object result;
                switch (expectedType)
                {
                    case XmlValueType.Unknown:
                        result = xmlParam.Clone();
                        break;
                    case XmlValueType.Xml:
                        result = xmlValue.Clone();
                        break;
                    default:
                        result = Convert(value, expectedType);
                        break;
                }

                listParam.Add(result);
            }
            return CollectionUtils.ToArray(listParam);
        }

        /// <summary>
        /// Transform the specified "init-params" element of the following
        /// structure:
        /// <pre>
        /// &lt;!ELEMENT init-params (init-param*)&gt;
        /// &lt;!ELEMENT init-param ((param-name | param-type), param-value,
        /// description?)&gt;
        /// </pre>
        /// into an XML element composed of the corrsponding names. For
        /// example, the "init-params" element of the following structure:
        /// <pre>
        /// &lt;init-param&gt;
        ///     &lt;param-name&gt;NameOne&lt;/param-name&gt;
        ///     &lt;param-value&gt;ValueOne&lt;/param-value&gt;
        /// &lt;/init-param&gt;
        /// &lt;init-param&gt;
        ///     &lt;param-name&gt;NameTwo&lt;/param-name&gt;
        ///     &lt;param-value&gt;ValueTwo&lt;/param-value&gt;
        /// &lt;/init-param&gt;
        /// </pre>
        /// will transform into
        /// <pre>
        /// &lt;NameOne&gt;ValueOne&lt;/NameOne&gt;
        /// &lt;NameTwo&gt;ValueTwo&lt;/NameTwo&gt;
        /// </pre>
        /// </summary>
        /// <remarks>
        /// For the purpose of this method only the parameters that have the
        /// "param-name" element specified are processed.
        /// </remarks>
        /// <param name="xmlParent">
        /// The XML element to insert the transformed elements into.
        /// </param>
        /// <param name="xmlParams">
        /// The "init-params" <b>IXmlElement</b> to parse.
        /// </param>
        public static void TransformInitParams(IXmlElement xmlParent, IXmlElement xmlParams)
        {
            for (IEnumerator elEnum = xmlParams.GetElements("init-param"); elEnum.MoveNext();)
            {
                IXmlElement xmlParam = (IXmlElement) elEnum.Current;
                string      name     = xmlParam.GetSafeElement("param-name" ).GetString();
                string      value    = xmlParam.GetSafeElement("param-value").GetString();

                if (name.Length != 0)
                {
                    xmlParent.EnsureElement(name).SetString(value);
                }
            }
        }

        /// <summary>
        /// Check whether or not the specified configuration defines an
        /// instance of a class.
        /// </summary>
        /// <remarks>
        /// The specified <see cref="IXmlElement"/> shoud be of the same
        /// structure as used in the
        /// <see cref="CreateInstance(IXmlElement, XmlHelper.IParameterResolver)"/>.
        /// </remarks>
        /// <param name="xmlClass">
        /// The XML element that contains the instantiation info.
        /// </param>
        /// <returns>
        /// <b>true</b> iff there is no class configuration information
        /// available.
        /// </returns>
        public static bool IsInstanceConfigEmpty(IXmlElement xmlClass)
        {
            return IsEmpty(xmlClass.GetSafeElement("class-name")) &&
               IsEmpty(xmlClass.GetSafeElement("class-factory-name"));
        }

        /// <summary>
        /// Creates a delegate from a given configuration.
        /// <see cref="IXmlElement"/> of the following structure:
        /// <pre>
        /// $lt;delegate&gt;
        ///    &lt;static/&gt; | &lt;instance/&gt;
        ///    &lt;class-name&gt;&lt;/class-name&gt;
        ///    &lt;delegate-type&gt;&lt;/delegate-type&gt;
        ///    &lt;method-name&gt;&lt;/method-name&gt;
        /// &lt;/delegate&gt;
        /// </pre>
        /// </summary>
        /// <typeparam name="TDel">
        /// The type of the delegate.
        /// </typeparam>        
        /// <param name="xmlDelegate">The XML element that contains the instantiation info. </param>
        /// <returns>A delegate obtained by the XML configutation</returns>
        public static TDel CreateDelegate<TDel>(IXmlElement xmlDelegate) where TDel : class
        {
            if (xmlDelegate == null)
            {
                return null;
            }
            // is the method static or not
            bool delegateIsStatic = xmlDelegate.EnsureElement("type").GetString("static") == "static";

            String className = xmlDelegate.GetElement("class-name").GetString();
            Type classType = className.Contains(",") ? TypeResolver.Resolve(className) : Type.GetType(className);

            string methodName = xmlDelegate.GetElement("method-name").GetString();

            if (delegateIsStatic)
            {
                MethodInfo method = classType.GetMethod(methodName,
                                                   BindingFlags.Public |
                                                   BindingFlags.NonPublic |
                                                   BindingFlags.Static);

                return Delegate.CreateDelegate(typeof(TDel), method) as TDel;
            }

            Object instance = Activator.CreateInstance(classType);
            return Delegate.CreateDelegate(typeof(TDel), instance,
                                           classType.GetMethod(methodName,
                                                               BindingFlags.Public |
                                                               BindingFlags.NonPublic |
                                                               BindingFlags.Instance)) as TDel;
        }

        /// <summary>
        /// Create an instance of the class configured using an
        /// <see cref="IXmlElement"/> of the following structure:
        /// <pre>
        /// &lt;!ELEMENT ... (class-name | (class-factory-name, method-name),
        /// init-params?&gt;
        /// &lt;!ELEMENT init-params (init-param*)&gt;
        /// &lt;!ELEMENT init-param ((param-name | param-type), param-value,
        /// description?)&gt;
        /// </pre>
        /// As of Coherence 12.1.2 the supplied element may also be of the 
        /// following format:
        /// <pre>
        /// &lt;!ELEMENT instance&gt;
        /// </pre>
        /// where the "instance" format is the same as above.
        /// </summary>
        /// <param name="xmlClass">
        /// The XML element that contains the instantiation info.
        /// </param>
        /// <param name="resolver">
        /// An <see cref="IParameterResolver"/> to resolve "{macro}" values
        /// (optional).
        /// </param>
        /// <returns>
        /// An object intantiated or obtained based on the class
        /// configuration.
        /// </returns>
        public static object CreateInstance(IXmlElement xmlClass, IParameterResolver resolver)
        {
            return CreateInstance(xmlClass, resolver, /*typeAssignable*/ null);    
        }

        /// <summary>
        /// Create an instance of the class configured using an
        /// <see cref="IXmlElement"/> of the following structure:
        /// <pre>
        /// &lt;!ELEMENT ... (class-name | (class-factory-name, method-name),
        /// init-params?&gt;
        /// &lt;!ELEMENT init-params (init-param*)&gt;
        /// &lt;!ELEMENT init-param ((param-name | param-type), param-value,
        /// description?)&gt;
        /// </pre>
        /// As of Coherence 12.1.2 the supplied element may also be of the 
        /// following format:
        /// <pre>
        /// &lt;!ELEMENT instance&gt;
        /// </pre>
        /// where the "instance" format is the same as above.
        /// </summary>
        /// <param name="xmlClass">
        /// The XML element that contains the instantiation info.
        /// </param>
        /// <param name="resolver">
        /// An <see cref="IParameterResolver"/> to resolve "{macro}" values
        /// (optional).
        /// </param>
        /// <param name="typeAssignable">
        /// if non-null, this method will validate that
        /// the Type is assignable from the loaded Type
        /// </param>
        /// <returns>
        /// An object intantiated or obtained based on the class
        /// configuration.
        /// </returns>
        public static object CreateInstance(IXmlElement xmlClass, IParameterResolver resolver, Type typeAssignable)
        {
            IXmlElement xmlInstance = xmlClass.GetSafeElement("instance");
            if (xmlInstance.ElementList.Count == 0)
            {
                // pre 12.1.2 style, no outer instance element
                xmlInstance = xmlClass;
            }

            string cls    = xmlInstance.GetSafeElement("class-name").GetString();
            string method = null;

            if (cls.Length == 0)
            {
                cls    = xmlInstance.GetSafeElement("class-factory-name").GetString();
                method = xmlInstance.GetSafeElement("method-name").GetString();
                if (cls.Length == 0 || method.Length == 0)
                {
                    throw new ArgumentException("Class name is missing:\n" + xmlClass);
                }
            }

            IXmlElement xmlParams = xmlInstance.GetElement("init-params");
            try
            {
                Type type = TypeResolver.Resolve(cls);

                if (typeAssignable != null && !typeAssignable.IsAssignableFrom(type))
                {
                    throw new ArgumentException("the type \""
                        + type
                        + "\" specified in configuration element \""
                        + cls
                        + "\" is not an instance of \""
                        + typeAssignable.Name
                        + '"');
                }
                if (xmlParams == null)
                {
                    if (method == null)
                    {
                        return Activator.CreateInstance(type);
                    }
                    else
                    {
                        MethodInfo methodInfo = type.GetMethod(method);
                        return methodInfo.Invoke(null, null); //static
                    }
                }

                // try instantiation with the parameters, then set config after
                object   target  = null;
                object[] aoParam = null;
                try 
                {
                   aoParam = ParseInitParams(xmlParams, resolver);
                }
                catch (Exception /* e */)
                {
                   // type missing for a parameter, could just be
                   // using IXmlConfigurable params
                   aoParam = null;
                }

                if (aoParam != null)
                {
                    try
                    {
                        if (method == null)
                        {
                            target = ObjectUtils.CreateInstance(type, aoParam);
                        }
                        else
                        {
                            MethodInfo methodInfo = type.GetMethod(method);
                            target = methodInfo.Invoke(null, aoParam);
                        }
                    }
                    finally
                    {
                        for (int i = 0; i < aoParam.Length; i++)
                        {
                            if (aoParam[i] is Stream)
                            {
                                ((Stream) aoParam[i]).Close();
                            }
                        }
                    }
                }

                // if there was an exception for missing a type, then we must create
                // this instance as an IXmlConfigurable, because the <init-params>
                // could be using <param-name>.
                //
                // If the class was already instantiated with the params, there
                // was no exception, just set the Config on it.
                if (typeof(IXmlConfigurable).IsAssignableFrom(type))
                {
                    IXmlElement xmlConfig = new SimpleElement("config");
                    TransformInitParams(xmlConfig, xmlParams);

                    IXmlConfigurable targetConfigurable = null;
                    if (aoParam != null)
                    {
                        targetConfigurable = (IXmlConfigurable) target;
                    }
                    else
                    {
                        if (method == null)
                        {
                            targetConfigurable = (IXmlConfigurable) Activator.CreateInstance(type);
                        }
                        else
                        {
                            MethodInfo methodInfo = type.GetMethod(method);
                            targetConfigurable = (IXmlConfigurable) methodInfo.Invoke(null, null);
                        }
                    }
                    targetConfigurable.Config = xmlConfig;
                    return targetConfigurable;                     
                }
                return target;
            }
            catch (Exception e)
            {
                string target = method == null
                    ? '\"' + cls + '\"'
                    : "using factory method \"" + cls + '.' + method + "()\"";
                throw new Exception("Failed to instantiate class " + target + '\n' + xmlClass, e);
            }
        }

        /// <summary>
        /// Convert a string type to known <b>XmlValueType</b>s, if any.
        /// </summary>
        /// <param name="type">the type to lookup.</param>
        /// <returns>The corresponding <b>XmlValueType</b> or <b>null</b> if no match exists.</returns>
        /// <since>12.2.1.4</since>
        public static XmlValueType LookupXmlValueType(string type)
        {
            if (StringUtils.IsNullOrEmpty(type))
            {
                throw new ArgumentException("Type to check cannot be null or zero-length");
            }
            switch (type.Trim())
            {
                case "string":
                case "System.String":
                    return XmlValueType.String;
                case "int":
                case "System.Int32":
                    return XmlValueType.Integer;
                case "long":
                case "System.Int64":
                    return XmlValueType.Long;
                case "bool":
                case "System.Boolean":
                    return XmlValueType.Boolean;
                case "double":
                case "System.Double":
                    return XmlValueType.Double;
                case "float":
                case "System.Single":
                    return XmlValueType.Float;
                case "decimal":
                case "System.Decimal":
                    return XmlValueType.Decimal;
                case "file":
                case "System.Io.File":
                    return XmlValueType.File;
                case "date":
                case "time":
                case "datetime":
                case "System.DateTime":
                    return XmlValueType.DateTime;
                case "xml":
                case "Tangosol.Run.Xml.IXmlElement":
                    return XmlValueType.Xml;
                default:
                    return XmlValueType.Unknown;
            }
        }

        #endregion

        #region Formatting support: memory size

        /// <summary>
        /// Parse the given string representation of a number of bytes.
        /// </summary>
        /// <remarks>
        /// The supplied string must be in the format:
        /// <p>
        /// <b>[\d]+[[.][\d]+]?[K|k|M|m|G|g|T|t]?[B|b]?</b></p>
        /// where the first non-digit (from left to right) indicates the
        /// factor with which the preceeding decimal value should be
        /// multiplied:
        /// <p>
        /// <list type="bullet">
        /// <item><b>K</b> or <b>k</b> (kilo, 2<sup>10</sup>)</item>
        /// <item><b>M</b> or <b>m</b> (mega, 2<sup>20</sup>)</item>
        /// <item><b>G</b> or <b>g</b> (giga, 2<sup>30</sup>)</item>
        /// <item><b>T</b> or <b>t</b> (tera, 2<sup>40</sup>)</item>
        /// </list></p>
        /// <p>
        /// If the string value does not contain a factor, a factor of one
        /// is assumed.</p>
        /// <p>
        /// The optional last character <b>B</b> or <b>b</b> indicates
        /// a unit of bytes.</p>
        /// </remarks>
        /// <param name="s">
        /// A string with the format
        /// <b>[\d]+[[.][\d]+]?[K|k|M|m|G|g|T|t]?[B|b]?</b>
        /// </param>
        /// <returns>
        /// The number of bytes represented by the given string.
        /// </returns>
        public static long ParseMemorySize(string s)
        {
            return ParseMemorySize(s, 0);
        }

        /// <summary>
        /// Parse the given string representation of a number of bytes.
        /// </summary>
        /// <remarks>
        /// The supplied string must be in the format:
        /// <p>
        /// <b>[\d]+[[.][\d]+]?[K|k|M|m|G|g|T|t]?[B|b]?</b></p>
        /// where the first non-digit (from left to right) indicates the
        /// factor with which the preceeding decimal value should be
        /// multiplied:
        /// <p>
        /// <list type="bullet">
        /// <item><b>K</b> or <b>k</b> (kilo, 2<sup>10</sup>)</item>
        /// <item><b>M</b> or <b>m</b> (mega, 2<sup>20</sup>)</item>
        /// <item><b>G</b> or <b>g</b> (giga, 2<sup>30</sup>)</item>
        /// <item><b>T</b> or <b>t</b> (tera, 2<sup>40</sup>)</item>
        /// </list></p>
        /// <p>
        /// If the string value does not contain an explict or implicit
        /// factor, a factor calculated by raising 2 to the given default
        /// power is used. The default power can be one of:
        /// <list type="bullet">
        /// <item><see cref="POWER_0"/></item>
        /// <item><see cref="POWER_K"/></item>
        /// <item><see cref="POWER_M"/></item>
        /// <item><see cref="POWER_G"/></item>
        /// <item><see cref="POWER_T"/></item>
        /// </list></p>
        /// <p>
        /// The optional last character <b>B</b> or <b>b</b> indicates
        /// a unit of bytes.</p>
        /// </remarks>
        /// <param name="s">
        /// A string with the format
        /// <b>[\d]+[[.][\d]+]?[K|k|M|m|G|g|T|t]?[B|b]?</b>
        /// </param>
        /// <param name="defaultPower">
        /// The exponent used to calculate the factor used in the
        /// conversion if one is not implied by the given string.
        /// </param>
        /// <returns>
        /// The number of bytes represented by the given string.
        /// </returns>
        public static long ParseMemorySize(string s, int defaultPower)
        {
            if (s == null)
            {
                throw new ArgumentException("Passed String must not be null");
            }

            switch (defaultPower)
            {
                case POWER_0:
                case POWER_K:
                case POWER_M:
                case POWER_G:
                case POWER_T:
                    break;
                default:
                    throw new ArgumentException("Illegal default power: " + defaultPower);
            }

            // remove trailing "[K|k|M|m|G|g|T|t]?[B|b]?" and store it as a factor
            int bitShift = POWER_0;
            int cch      = s.Length;
            if (cch > 0)
            {
                char ch = s[cch - 1];
                bool defaultFlag;
                if (ch == 'B' || ch == 'b')
                {
                    // bytes are implicit
                    --cch;
                    defaultFlag = false;
                }
                else
                {
                    defaultFlag = true;
                }

                if (cch > 0)
                {
                    switch (s[--cch])
                    {
                        case 'K':
                        case 'k':
                            bitShift = POWER_K;
                            break;

                        case 'M':
                        case 'm':
                            bitShift = POWER_M;
                            break;

                        case 'G':
                        case 'g':
                            bitShift = POWER_G;
                            break;

                        case 'T':
                        case 't':
                            bitShift = POWER_T;
                            break;

                        default:
                            if (defaultFlag)
                            {
                                bitShift = defaultPower;
                            }
                            ++cch; // oops: shouldn't have chopped off the last char
                            break;
                    }
                }
            }

            // make sure that the string contains some digits
            if (cch == 0)
            {
                throw new FormatException("Passed String (\"" + s + "\") must contain a number");
            }

            // extract the digits (decimal form) to assemble the base number
            long cb      = 0L;
            bool isDec   = false;
            int  divisor = 1;
            for (int of = 0; of < cch; ++of)
            {
                char ch = s[of];
                switch (ch)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        cb = cb * 10L + (ch - '0');
                        if (isDec)
                        {
                            divisor *= 10;
                        }
                        break;

                    case '.':
                        if (isDec)
                        {
                            throw new FormatException("Invalid memory size: \"" + s
                                                      + "\" (illegal second decimal point)");
                        }
                        isDec = true;
                        break;

                    default:
                        throw new FormatException("Invalid memory size: \"" + s
                                                  + "\" (illegal digit: \"" + ch + "\")");
                }
            }

            cb <<= bitShift;
            if (isDec)
            {
                if (divisor == 1)
                {
                    throw new FormatException("invalid memory size: \"" + s
                                              + "\" (illegal trailing decimal point)");
                }
                else
                {
                    cb /= divisor;
                }
            }
            return cb;
        }

        #endregion

        #region Formatting support: time

        /// <summary>
        /// Parse the given string representation of a time duration and
        /// return its value as a number of milliseconds.
        /// </summary>
        /// <remarks>
        /// The supplied string must be in the format:
        /// <p>
        /// <b>[\d]+[[.][\d]+]?[MS|ms|S|s|M|m|H|h|D|d]?</b></p>
        /// <p>
        /// where the first non-digits (from left to right) indicate the unit
        /// of time duration:
        /// <list type="bullet">
        /// <item><b>MS</b> or <b>ms</b> (milliseconds)</item>
        /// <item><b>S</b>  or <b>s</b>  (seconds)</item>
        /// <item><b>M</b>  or <b>m</b>  (minutes)</item>
        /// <item><b>H</b>  or <b>h</b>  (hours)</item>
        /// <item><b>D</b>  or <b>d</b>  (days)</item>
        /// </list></p>
        /// <p>
        /// If the string value does not contain a unit, a unit of
        /// milliseconds is assumed.</p>
        /// </remarks>
        /// <param name="s">
        /// A string with the format
        /// <b>[\d]+[[.][\d]+]?[MS|ms|S|s|M|m|H|h|D|d]?</b>
        /// </param>
        /// <returns>
        /// The number of milliseconds represented by the given string.
        /// </returns>
        public static long ParseTime(string s)
        {
            return ParseTime(s, UNIT_MS);
        }

        /// <summary>
        /// Parse the given string representation of a time duration and
        /// return its value as a number of milliseconds.
        /// </summary>
        /// <remarks>
        /// The supplied string must be in the format:
        /// <p>
        /// <b>[\d]+[[.][\d]+]?[MS|ms|S|s|M|m|H|h|D|d]?</b></p>
        /// <p>
        /// where the first non-digits (from left to right) indicate the unit
        /// of time duration:
        /// <list type="bullet">
        /// <item><b>MS</b> or <b>ms</b> (milliseconds)</item>
        /// <item><b>S</b>  or <b>s</b>  (seconds)</item>
        /// <item><b>M</b>  or <b>m</b>  (minutes)</item>
        /// <item><b>H</b>  or <b>h</b>  (hours)</item>
        /// <item><b>D</b>  or <b>d</b>  (days)</item>
        /// </list></p>
        /// <p>
        /// If the string value does not contain a unit, the specified
        /// default unit is assumed. The default unit can be one of:
        /// <list type="bullet">
        /// <item><see cref="UNIT_MS"/></item>
        /// <item><see cref="UNIT_S"/></item>
        /// <item><see cref="UNIT_M"/></item>
        /// <item><see cref="UNIT_H"/></item>
        /// <item><see cref="UNIT_D"/></item>
        /// </list></p>
        /// </remarks>
        /// <param name="s">
        /// A string with the format
        /// <b>[\d]+[[.][\d]+]?[MS|ms|S|s|M|m|H|h|D|d]?</b>
        /// </param>
        /// <param name="defaultUnit">
        /// The unit to use in the conversion to milliseconds if one is not
        /// specified in the supplied string.
        /// </param>
        /// <returns>
        /// The number of milliseconds represented by the given string.
        /// </returns>
        public static long ParseTime(string s, int defaultUnit)
        {
            if (s == null)
            {
                throw new ArgumentException("Passed String must not be null");
            }

            switch (defaultUnit)
            {
                case UNIT_MS:
                case UNIT_S:
                case UNIT_M:
                case UNIT_H:
                case UNIT_D:
                    break;
                default:
                    throw new ArgumentException("Illegal default unit: " + defaultUnit);
            }

            // remove trailing "[MS|ms|S|s|M|m|H|h|D|d]?" and store it as a factor
            int multiplier = defaultUnit;
            int cch        = s.Length;
            if (cch > 0)
            {
                switch (s[--cch])
                {
                    case 'S':
                    case 's':
                        multiplier = UNIT_S;
                        if (cch > 1)
                        {
                            char c = s[cch - 1];
                            if (c == 'M' || c == 'm')
                            {
                                --cch;
                                multiplier = UNIT_MS;
                            }
                        }
                        break;

                    case 'M':
                    case 'm':
                        multiplier = UNIT_M;
                        break;

                    case 'H':
                    case 'h':
                        multiplier = UNIT_H;
                        break;

                    case 'D':
                    case 'd':
                        multiplier = UNIT_D;
                        break;

                    default:
                        ++cch; // oops: shouldn't have chopped off the last char
                        break;
                }
            }

            // make sure that the string contains some digits
            if (cch == 0)
            {
                throw new FormatException("Passed String (\"" + s + "\") must contain a number");
            }

            // extract the digits (decimal form) to assemble the base number
            long millis  = 0L;
            bool isDec   = false;
            int  divisor = 1;
            for (int of = 0; of < cch; ++of)
            {
                char ch = s[of];
                switch (ch)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        millis = millis * 10L + (ch - '0');
                        if (isDec)
                        {
                            divisor *= 10;
                        }
                        break;

                    case '.':
                        if (isDec)
                        {
                            throw new FormatException("Invalid time: \"" + s +
                                                      "\" (illegal second decimal point)");
                        }
                        isDec = true;
                        break;

                    default:
                        throw new FormatException("Invalid time: \"" + s +
                                                  "\" (illegal digit: \"" + ch + "\")");
                }
            }

            millis *= multiplier;
            if (isDec)
            {
                if (divisor == 1)
                {
                    throw new FormatException("Invalid time: \"" + s +
                                              "\" (illegal trailing decimal point)");
                }
                else
                {
                    millis /= divisor;
                }
            }
            return millis;
        }

        #endregion

        #region Object method helpers

        /// <summary>
        /// Provide a hash value for the XML element and all of its contained
        /// information.
        /// </summary>
        /// <remarks>
        /// The hash value is defined as a xor of the following:
        /// <list type="bullet">
        /// <item>the GetHashCode() from the element's value (i.e.
        /// base.GetHashCode())</item>
        /// <item>the GetHashCode() from each attribute name</item>
        /// <item>the GetHashCode() from each attribute value</item>
        /// <item>the GetHashCode() from each sub-element</item>
        /// </list>
        /// </remarks>
        /// <param name="xml">
        /// The <see cref="IXmlElement"/>.
        /// </param>
        /// <returns>
        /// The hash value for the XML element.
        /// </returns>
        public static int HashElement(IXmlElement xml)
        {
            // start with the HashValue() from the element's value
            int n = HashValue(xml);

            foreach (DictionaryEntry entry in xml.Attributes)
            {
                // entry.GetHashCode() is a xor of the key and value, which
                // is the attribute name and value
                n ^= entry.Key.GetHashCode() ^ entry.Value.GetHashCode();
            }

            foreach (IXmlValue element in xml.ElementList)
            {
                // xor in the GetHashCode() of each sub-element
                n ^= element.GetHashCode();
            }

            return n;
        }

        /// <summary>
        /// Provide a hash value for the XML value.
        /// </summary>
        /// <remarks>
        /// The hash value is defined as one of the following:
        /// <list type="number">
        /// <item>0 if <see cref="IXmlValue.Value"/> returns <c>null</c>
        /// </item>
        /// <item>otherwise the hash value is the GetHashCode() of the string
        /// representation of the value</item>
        /// </list>
        /// </remarks>
        /// <param name="val">
        /// The <see cref="IXmlValue"/>.
        /// </param>
        /// <returns>
        /// The hash value for the XML value.
        /// </returns>
        public static int HashValue(IXmlValue val)
        {
            object o = val.Value;

            if (o == null)
            {
                return 0;
            }

            string s = o is string ? (string) o : (string) Convert(o, XmlValueType.String);

            return s.GetHashCode();
        }

        /// <summary>
        /// Compare one XML element with another XML element for equality.
        /// </summary>
        /// <param name="xml1">
        /// A non-null <b>IXmlElement</b> object.
        /// </param>
        /// <param name="xml2">
        /// A non-null <b>IXmlElement</b> object.
        /// </param>
        /// <returns>
        /// <b>true</b> if the elements are equal, <b>false</b> otherwise.
        /// </returns>
        public static bool EqualsElement(IXmlElement xml1, IXmlElement xml2)
        {
            if (xml1 == null || xml2 == null)
            {
                throw new ArgumentNullException("element");
            }

            // name
            if (!Equals(xml1.Name, xml2.Name))
            {
                return false;
            }

            // comment
            if (!Equals(xml1.Comment, xml2.Comment))
            {
                return false;
            }

            // value
            if (!EqualsValue(xml1, xml2))
            {
                return false;
            }

            // attributes
            if (!Equals(xml1.Attributes, xml2.Attributes))
            {
                return false;
            }

            // children
            if (!Equals(xml1.ElementList, xml2.ElementList))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Compare one XML value with another XML value for equality.
        /// </summary>
        /// <param name="val1">
        /// A non-null <b>IXmlValue</b> object.
        /// </param>
        /// <param name="val2">
        /// A non-null <b>IXmlValue</b> object.
        /// </param>
        /// <returns>
        /// <b>true</b> if the values are equal, <b>false</b> otherwise.
        /// </returns>
        public static bool EqualsValue(IXmlValue val1, IXmlValue val2)
        {
            if (val1 == null || val2 == null)
            {
                throw new ArgumentNullException("value");
            }

            bool isEmpty1 = val1.IsEmpty;
            bool isEmpty2 = val2.IsEmpty;
            if (isEmpty1 || isEmpty2)
            {
                return isEmpty1 && isEmpty2;
            }

            object o1 = val1.Value;
            object o2 = val2.Value;
            if (o1 == null || o2 == null)
            {
                throw new ArgumentException("Null value");
            }

            if (o1.GetType() == o2.GetType())
            {
                return o1.Equals(o2);
            }

            return Convert(o1, XmlValueType.String).Equals(Convert(o2, XmlValueType.String));
        }

        #endregion

        #region Type conversions

        /// <summary>
        /// Convert the passed object to the specified type.
        /// </summary>
        /// <param name="o">
        /// The object value or <c>null</c>.
        /// </param>
        /// <param name="type">
        /// The enumerated type to convert to.
        /// </param>
        /// <returns>
        /// An object of the specified type.
        /// </returns>
        public static object Convert(object o, XmlValueType type)
        {
            if (o == null)
            {
                return null;
            }

            switch (type)
            {
                case XmlValueType.Boolean:
                {
                    if (o is bool)
                    {
                        return o;
                    }

                    string s = (string) Convert(o, XmlValueType.String);
                    if (!StringUtils.IsNullOrEmpty(s))
                    {
                        switch (Trim(s).ToCharArray()[0])
                        {
                            case 'T': // TRUE or True
                            case 't': // true
                            case 'Y': // YES or Yes
                            case 'y': // yes or y
                                return true;

                            case '1': // integer representation of true
                                if (s.Length == 1)
                                {
                                    return true;
                                }
                                break;

                            case 'F': // FALSE or False
                            case 'f': // false
                            case 'N': // NO or No
                            case 'n': // no
                                return false;

                            case '0': // integer representation of false
                                if (s.Length == 1)
                                {
                                    return false;
                                }
                                break;
                        }
                    }

                    return null;
                }

                case XmlValueType.Integer:
                {
                    if (o is int)
                    {
                        return o;
                    }

                    string s = (string) Convert(o, XmlValueType.String);
                    if (!StringUtils.IsNullOrEmpty(s))
                    {
                        try
                        {
                            return int.Parse(Trim(s));
                        }
                        catch (Exception)
                        {}
                    }

                    return null;
                }

                case XmlValueType.Long:
                {
                    if (o is long)
                    {
                        return o;
                    }

                    string s = (string) Convert(o, XmlValueType.String);
                    if (!StringUtils.IsNullOrEmpty(s))
                    {
                        try
                        {
                            return long.Parse(Trim(s));
                        }
                        catch (Exception)
                        {}
                    }

                    return null;
                }

                case XmlValueType.Double:
                {
                    if (o is Double)
                    {
                        return o;
                    }

                    string s = (string) Convert(o, XmlValueType.String);
                    if (!StringUtils.IsNullOrEmpty(s))
                    {
                        try
                        {
                            return double.Parse(Trim(s));
                        }
                        catch (Exception)
                        {}
                    }

                    return null;
                }

                case XmlValueType.Decimal:
                {
                    if (o is Decimal)
                    {
                        return o;
                    }

                    string s = (string) Convert(o, XmlValueType.String);
                    if (!StringUtils.IsNullOrEmpty(s))
                    {
                        try
                        {
                            return decimal.Parse(Trim(s));
                        }
                        catch (Exception)
                        {}
                    }

                    return null;
                }

                case XmlValueType.Float:
                {
                    if (o is Single)
                    {
                        return o;
                    }

                    string s = (string) Convert(o, XmlValueType.String);
                    if (!StringUtils.IsNullOrEmpty(s))
                    {
                        try
                        {
                            return Single.Parse(Trim(s));
                        }
                        catch (Exception)
                        {}
                    }

                    return null;
                }

                case XmlValueType.String:
                {
                    if (o is string)
                    {
                        return o;
                    }

                    if (o is Binary)
                    {
                        Binary bin = (Binary) o;
                        return System.Convert.ToBase64String(bin.ToByteArray());
                    }

                    return o.ToString();
                }

                case XmlValueType.Binary:
                {
                    if (o is Binary)
                    {
                        return o;
                    }

                    string s = (string) Convert(o, XmlValueType.String);
                    if (s != null)
                    {
                        if (s.Length == 0)
                        {
                            return Binary.NO_BINARY;
                        }

                        try
                        {
                            return new Binary(System.Convert.FromBase64String(s));
                        }
                        catch (Exception)
                        {}
                    }

                    return null;
                }

                case XmlValueType.DateTime:
                {
                    if (o is DateTime)
                    {
                        return o;
                    }

                    string s = (string) Convert(o, XmlValueType.String);
                    if (!StringUtils.IsNullOrEmpty(s))
                    {
                        try
                        {
                            return DateTime.Parse(Trim(s));
                        }
                        catch (Exception)
                        {}
                    }

                    return null;
                }

                case XmlValueType.File:
                {
                    if (o is FileStream)
                    {
                        return o;
                    }

                    string s = (string) Convert(o, XmlValueType.String);
                    if (!StringUtils.IsNullOrEmpty(s))
                    {
                        try
                        {
                            return File.Create(s);
                        }
                        catch (Exception e)
                        {
                           Console.Out.WriteLine(e);
                        }
                    }

                    return null;
                }

                case XmlValueType.Xml:
                {
                    if (o is IXmlElement)
                    {
                        return o;
                    }

                    string s = (string) Convert(o, XmlValueType.String);
                    if (!StringUtils.IsNullOrEmpty(s))
                    {
                        try
                        {
                            return XmlHelper.LoadXml(new StringReader(s));
                        }
                        catch (Exception)
                        {
                        }
                    }

                    return null;
                }

                default:
                    throw new NotSupportedException("unsupported type");
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// A unit of milliseconds.
        /// </summary>
        public const int UNIT_MS = 1;

        /// <summary>
        /// A unit of seconds.
        /// </summary>
        public const int UNIT_S = 1000 * UNIT_MS;

        /// <summary>
        /// A unit of minutes.
        /// </summary>
        public const int UNIT_M = 60 * UNIT_S;

        /// <summary>
        /// A unit of hours.
        /// </summary>
        public const int UNIT_H = 60 * UNIT_M;

        /// <summary>
        /// A unit of days.
        /// </summary>
        public const int UNIT_D = 24 * UNIT_H;

        /// <summary>
        /// An exponent of zero.
        /// </summary>
        public const int POWER_0 = 0;

        /// <summary>
        /// An exponent of 10.
        /// </summary>
        public const int POWER_K = 10;

        /// <summary>
        /// An exponent of 20.
        /// </summary>
        public const int POWER_M = 20;

        /// <summary>
        /// An exponent of 30.
        /// </summary>
        public const int POWER_G = 30;

        /// <summary>
        /// An exponent of 40.
        /// </summary>
        public const int POWER_T = 40;

        /// <summary>
        /// A constant that indicates that the parameter cannot be resolved.
        /// </summary>
        public static readonly object UNRESOLVED = new Object();

        /// <summary>
        /// Hexidecimal digits.
        /// </summary>
        private static readonly char[] HEX = "0123456789ABCDEF".ToCharArray();

        #endregion

        #region Inner interface : IParameterResolver

        /// <summary>
        /// An interface that describes a callback to resolve a substitutable
        /// parameter value.
        /// </summary>
        public interface IParameterResolver
        {
            /// <summary>
            /// Resolve the passed substitutable parameter.
            /// </summary>
            /// <param name="type">
            /// The value of the "param-type" element.
            /// </param>
            /// <param name="value">
            /// The value of the "param-value" element, which is enclosed by
            /// curly braces, indicating its substitutability.
            /// </param>
            /// <returns>
            /// The object value to use or the <see cref="UNRESOLVED"/>
            /// constant.
            /// </returns>
            object ResolveParameter(string type, string value);
        }

        #endregion

        #region Inner class : ElementMatchFilter

        private class ElementMatchFilter : IFilter
        {
            public ElementMatchFilter(string local, string uri)
            {
                m_local = local;
                m_uri   = uri;
            }

            /// <summary>
            /// Apply the test to the object.
            /// </summary>
            /// <param name="o">
            /// An object to which the test is applied.
            /// </param>
            /// <returns>
            /// <b>true</b> if the test passes, <b>false</b> otherwise.
            /// </returns>
            public bool Evaluate(object o)
            {
                return o is IXmlElement && IsElementMatch((IXmlElement) o, m_local, m_uri);
            }

            private string m_local;
            private string m_uri;
        }

        #endregion
    }
}