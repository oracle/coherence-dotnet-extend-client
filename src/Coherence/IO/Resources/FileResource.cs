/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

namespace Tangosol.IO.Resources
{
    /// <summary>
    /// File resource implementation of <see cref="IResource"/>.
    /// </summary>
    /// <remarks>
    /// <p/>
    /// Supports both a <b>System.IO.FileInfo</b> and a <b>System.Uri</b>.
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class FileResource : AbstractResource
    {
        #region Properties

        /// <summary>
        /// Gets a stream for this <see cref="IResource"/>.
        /// </summary>
        /// <returns>
        /// A <b>System.IO.Stream</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// If the stream can not be opened.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// If the file can not be found.
        /// </exception>
        /// <seealso cref="IResource"/>
        public override Stream GetStream()
        {
            return m_fileHandle.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a <b>FileResource</b> class using the resource name
        /// supplied.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the file system resource.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the supplied <paramref name="resourceName"/> is <c>null</c> or
        /// contains only whitespace character(s).
        /// </exception>
        public FileResource(string resourceName) : base(resourceName)
        {
            m_fileHandle = GetFileHandle(resourceName);
            AbsolutePath = m_fileHandle.FullName;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Gets the <b>System.IO.FileInfo</b> for the specified
        /// <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the file system resource.
        /// </param>
        /// <returns>
        /// The <b>System.IO.FileInfo</b> for this <b>FileResource</b>.
        /// </returns>
        protected virtual FileInfo GetFileHandle(string resourceName)
        {
            string fileName = GetResourceNameWithoutProtocol(resourceName);
            if (fileName.StartsWith("~"))
            {
                // resolve it against application's base path
                fileName = AppDomain.CurrentDomain.BaseDirectory + fileName.Substring(2);
            }
            return new FileInfo(fileName);
        }

        #endregion

        #region Data members

        private readonly FileInfo m_fileHandle;

        #endregion
    }
}