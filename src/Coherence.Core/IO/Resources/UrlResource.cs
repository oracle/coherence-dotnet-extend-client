/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Net;

namespace Tangosol.IO.Resources
{
    /// <summary>
    /// URL resource implementation.
    /// </summary>
    /// <remarks>
    /// This <see cref="IResource"/> implementation can be used to access
    /// resources on remote servers over HTTP or FTP.
    /// </remarks>
    /// <author>Aleksandar Seovic  2006.10.07</author>
    /// <seealso cref="IResource"/>
    /// <seealso cref="ResourceLoader"/>
    public class UrlResource : AbstractResource
    {
        #region Constructors

        /// <summary>
        /// Constructor that takes a resource name and creates resource.
        /// </summary>
        /// <remarks>
        /// You can use http://www.mycompany.com/services.txt, 
        /// ftp://user:pass@ftp.myserver.com/dir/file.xml or similar URL 
        /// as a resource name.
        /// </remarks>
        /// <param name="resourceName">
        /// Resource name of the URL resource.
        /// </param>
        public UrlResource(string resourceName) : base(resourceName)
        {
            m_url        = new Uri(resourceName);
            AbsolutePath = m_url.AbsolutePath;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a stream for this <see cref="IResource"/>.
        /// </summary>
        /// <returns>
        /// A <b>System.IO.Stream</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// If the stream could not be opened.
        /// </exception>
        /// <seealso cref="IResource"/>
        public override Stream GetStream()
        {
            return WebRequest.Create(m_url).GetResponse().GetResponseStream();
        }

        #endregion

        #region Data members

        private readonly Uri m_url;

        #endregion
    }
}