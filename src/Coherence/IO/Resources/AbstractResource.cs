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
    /// Asbtract base class for other <see cref="IResource"/>
    /// implementations.
    /// </summary>
    /// <author>Aleksandar Seovic  2006.10.07</author>
    /// <seealso cref="IResource"/>
    /// <seealso cref="ResourceLoader"/>
    public abstract class AbstractResource : IResource
    {
        #region Properties

        /// <summary>
        /// Gets the fully qualified URI for this <see cref="IResource"/>.
        /// </summary>
        /// <remarks>
        /// Fuly qualified URI for a resource is the name of the resource
        /// as specified, with a protocol name prepended if necessary.
        /// </remarks>
        /// <returns>
        /// The fully qualified resource URI.
        /// </returns>
        /// <seealso cref="IResource.Uri"/>
        public virtual string Uri
        {
            get { return m_uri; }
        }

        /// <summary>
        /// Gets an absolute path of this resource.
        /// </summary>
        /// <remarks>
        /// The exact value returned will depend on the specific
        /// resource implementation.
        /// <p/>
        /// For example, file system-based resources that support
        /// relative path resolution, such as <see cref="FileResource"/>,
        /// should return an absolute
        /// path of the resource on the file system. Other resource
        /// types should return fully qualified URI for the resource,
        /// which is basically the value of the <see cref="Uri"/>
        /// property.
        /// </remarks>
        public string AbsolutePath
        {
            get { return m_absolutePath; }
            set { m_absolutePath = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that takes resource name and creates a new instance 
        /// of the <b>AbstractResource</b> class.
        /// </summary>
        /// <remarks>
        /// Since this is an <b>abstract</b> class it does not expose
        /// any public constructors.
        /// </remarks>
        /// <param name="resourceName">
        /// A String representing the resource.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="resourceName"/> is <c>null</c> or
        /// it contains only whitespace character(s).
        /// </exception>
        protected AbstractResource(string resourceName)
        {
            m_uri = m_absolutePath = resourceName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Opens an input stream for this resource.
        /// </summary>
        /// <returns>
        /// A <b>System.IO.Stream</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// If the stream could not be opened.
        /// </exception>
        /// <seealso cref="IResource"/>
        public abstract Stream GetStream();


        /// <summary>
        /// Strips protocol name (if present) from the 
        /// <paramref name="resourceName"/>.
        /// </summary>        
        /// <param name="resourceName">
        /// Resource name.
        /// </param>
        /// <returns>
        /// Resource name without the protocol name.
        /// </returns>
        protected static string GetResourceNameWithoutProtocol(string resourceName)
        {
            int pos = resourceName.IndexOf(ResourceLoader.PROTOCOL_SEPARATOR);
            return pos == -1 
                    ? resourceName 
                    : resourceName.Substring(pos + ResourceLoader.PROTOCOL_SEPARATOR.Length);
        }

        /// <summary>
        /// Returns the textual information about this resource.
        /// </summary>
        /// <returns>
        /// Human readable description of this resource.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + "(Uri = " + Uri + 
                                    ", AbsolutePath = " + AbsolutePath + ")";
        }

        /// <summary>
        /// Compares specified with the current object.
        /// </summary>
        /// <remarks>
        /// This implementation compares <see cref="AbsolutePath"/> values.
        /// </remarks>
        /// <seealso cref="IResource.Uri"/>
        /// <param name="obj">
        /// The <b>IResource</b> object to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if this resource and the passed object are
        /// equivalent.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is IResource
                   && ((IResource)obj).AbsolutePath.Equals(AbsolutePath);
        }

        /// <summary>
        /// A hash function that returns the hashcode for this type, that is
        /// suitable for use in hashing algorithms and data structures like a
        /// hash table.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return AbsolutePath.GetHashCode();
        }

        #endregion

        #region Data members

        private readonly string m_uri;
        private string m_absolutePath;

        #endregion
    }
}