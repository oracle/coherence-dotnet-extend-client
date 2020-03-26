/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

using Tangosol.Config;
using Tangosol.IO.Resources;

namespace Tangosol.Run.Xml
{
    /// <summary>
    /// This class uses the validating <b>System.Xml.XmlReader</b> to load
    /// XML into <b>System.Xml.XmlDocument</b> which is then converted to
    /// <see cref="IXmlDocument"/>.
    /// </summary>
    /// <author>Cameron Purdy  2001.07.16</author>
    /// <author>Ana Cikic  2009.09.04</author>
    public class SimpleParser
    {
        #region Public interface

        /// <summary>
        /// Parse the specified resource's content into an
        /// <b>IXmlDocument</b> object.
        /// </summary>
        /// <param name="resource">
        /// The <see cref="IResource"/> with XML to parse.
        /// </param>
        /// <returns>
        /// An <b>IXmlDocument</b> object.
        /// </returns>
        public virtual IXmlDocument ParseXml(IResource resource)
        {
            XmlDocument xmlDoc = LoadXml(resource);
            return XmlHelper.ConvertDocument(xmlDoc);
        }

        /// <summary>
        /// Parse the resource specified by path into an <b>IXmlDocument</b>
        /// object.
        /// </summary>
        /// <param name="path">
        /// Location of Xml data; an URL or valid path.
        /// </param>
        /// <returns>
        /// An <b>IXmlDocument</b> object.
        /// </returns>
        public virtual IXmlDocument ParseXml(string path)
        {
            return ParseXml(ResourceLoader.GetResource(path));
        }

        /// <summary>
        /// Parse the specified <b>TextReader</b> into an <b>IXmlDocument</b>
        /// object.
        /// </summary>
        /// <param name="reader">
        /// The <b>TextReader</b> object.
        /// </param>
        /// <returns>
        /// An <b>IXmlDocument</b> object.
        /// </returns>
        public virtual IXmlDocument ParseXml(TextReader reader)
        {
            XmlDocument xmlDoc = LoadXml(reader);
            return XmlHelper.ConvertDocument(xmlDoc);
        }

        /// <summary>
        /// Parse the specified <b>Stream</b> into an <b>IXmlDocument</b>
        /// object.
        /// </summary>
        /// <param name="stream">
        /// The <b>Stream</b> object.
        /// </param>
        /// <returns>
        /// An <b>IXmlDocument</b> object.
        /// </returns>
        public virtual IXmlDocument ParseXml(Stream stream)
        {
            return ParseXml(new StreamReader(stream));
        }

        /// <summary>
        /// Parse the specified <b>Stream</b> into an <b>IXmlDocument</b>
        /// object using the specified charset.
        /// </summary>
        /// <param name="stream">
        /// The <b>Stream</b> object.
        /// </param>
        /// <param name="encoding">
        /// The character encoding.
        /// </param>
        /// <returns>
        /// An <b>IXmlDocument</b> object.
        /// </returns>
        /// <exception cref="IOException"/>
        public virtual IXmlDocument ParseXml(Stream stream, Encoding encoding)
        {
            return ParseXml(new StreamReader(stream, encoding));
        }

        #endregion

        #region .NET Xml Parsing

        /// <summary>
        /// Gets the <b>XmlDocument</b> representing data from a given
        /// resource.
        /// </summary>
        /// <param name="resource">
        /// <see cref="IResource"/> with Xml data.
        /// </param>
        /// <returns>
        /// <b>XmlDocument</b> representing Xml data.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="resource"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="XmlException">
        /// If there is a load or parse error in the XML.
        /// </exception>
        protected virtual XmlDocument LoadXml(IResource resource)
        {
            using (Stream stream = resource.GetStream())
            {
                XmlReader reader = CreateValidatingReader(stream,
                                                          XmlConfigSchemasRepository.Schemas,
                                                          new ValidationEventHandler(HandleValidation));

                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                return doc;
            }
        }

        /// <summary>
        /// Gets the <b>XmlDocument</b> representing data from a given
        /// <b>TextReader</b>.
        /// </summary>
        /// <param name="textReader">
        /// <b>TextReader</b> that will provide Xml data.
        /// </param>
        /// <returns>
        /// <b>XmlDocument</b> representing Xml data.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="textReader"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="XmlException">
        /// If there is a load or parse error in the XML.
        /// </exception>
        protected virtual XmlDocument LoadXml(TextReader textReader)
        {
            XmlReader reader = CreateValidatingReader(textReader,
                                                      XmlConfigSchemasRepository.Schemas,
                                                      new ValidationEventHandler(HandleValidation));

            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            return doc;
        }

        /// <summary>
        /// Gets an <see cref="System.Xml.XmlReader"/> instance
        /// for the supplied <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="stream">
        /// The XML <see cref="System.IO.Stream"/>.
        /// </param>
        /// <param name="schemas">
        /// XML schemas that will be used for validation.
        /// </param>
        /// <param name="eventHandler">
        /// Validation event handler.
        /// </param>
        /// <returns>
        /// <see cref="System.Xml.XmlReader"/> implementation.
        /// </returns>
        protected virtual XmlReader CreateValidatingReader(Stream stream, XmlSchemaSet schemas,
                                                           ValidationEventHandler eventHandler)
        {
            XmlReaderSettings settings = CreateReaderSettings(schemas, eventHandler);
            return XmlReader.Create(stream, settings);
        }

        /// <summary>
        /// Gets an <see cref="System.Xml.XmlReader"/> instance
        /// for the supplied <see cref="System.IO.TextReader"/>.
        /// </summary>
        /// <param name="textReader">
        /// The XML <see cref="System.IO.TextReader"/>.
        /// </param>
        /// <param name="schemas">
        /// XML schemas that will be used for validation.
        /// </param>
        /// <param name="eventHandler">
        /// Validation event handler.
        /// </param>
        /// <returns>
        /// <see cref="System.Xml.XmlReader"/> implementation.
        /// </returns>
        protected virtual XmlReader CreateValidatingReader(TextReader textReader, XmlSchemaSet schemas,
                                                           ValidationEventHandler eventHandler)
        {
            XmlReaderSettings settings = CreateReaderSettings(schemas, eventHandler);
            return XmlReader.Create(textReader, settings);
        }

        /// <summary>
        /// Callback for a validating XML reader.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="args">
        /// Any data pertinent to the event.
        /// </param>
        private void HandleValidation(object sender, ValidationEventArgs args)
        {
            throw new XmlException(args.Message, args.Exception);
        }

        /// <summary>
        /// Gets an <b>XmlReaderSettings</b> with specified schemas used for
        /// validation and validation event handler.
        /// </summary>
        /// <param name="schemas">
        /// XML schemas that will be used for validation.
        /// </param>
        /// <param name="eventHandler">
        /// Validation event handler.
        /// </param>
        /// <returns>
        /// <b>XmlReaderSettings</b> instance.
        /// </returns>
        protected virtual XmlReaderSettings CreateReaderSettings(XmlSchemaSet schemas, ValidationEventHandler eventHandler)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            if (schemas != null && schemas.Count > 0)
            {
                settings.Schemas.Add(schemas);
                settings.ValidationType = ValidationType.Schema;
                if (eventHandler != null)
                {
                    settings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings;
                    settings.ValidationEventHandler += eventHandler;
                }
            }

            return settings;
        }

        #endregion
    }
}
