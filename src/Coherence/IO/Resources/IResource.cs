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
    /// Interface for access to different types of resources that can
    /// provide a <b>System.IO.Stream</b> to read from.
    /// </summary>
    /// <remarks>
    /// This interface encapsulates a resource descriptor that abstracts away
    /// from the underlying type of resource; possible resource types include
    /// files, URLs, web resources and assmbly embedded resources.
    /// </remarks>
    /// <seealso cref="ResourceLoader"/>
    /// <author>Aleksandar Seovic  2006.10.07</author>
    public interface IResource
    {
        /// <summary>
        /// Opens an input stream for this resource.
        /// </summary>
        /// <remarks>
        /// <note type="caution">
        /// Clients of this interface must be aware that every call of this
        /// method will create a <i>fresh</i> <b>System.IO.Stream</b>;
        /// it is the responsibility of the calling code to close any such
        /// <b>System.IO.Stream</b>.
        /// </note>
        /// </remarks>
        /// <returns>
        /// A <b>System.IO.Stream</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// If the stream could not be opened.
        /// </exception>
        Stream GetStream();

        /// <summary>
        /// Gets the fully qualified URI for this <see cref="IResource"/>.
        /// </summary>
        /// <remarks>
        /// Fuly qualified URI for a resource is the name of the resource
        /// as specified, with a protocol name prepended if necessary.
        /// </remarks>
        /// <value>
        /// The fully qualified resource URI.
        /// </value>
        string Uri { get; }

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
        /// <value>The absolute path of this resource.</value>
        string AbsolutePath { get; }
    }
}