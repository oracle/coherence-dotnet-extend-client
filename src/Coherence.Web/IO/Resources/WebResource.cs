/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace Tangosol.IO.Resources
{
    /// <summary>
    /// A resource implementation that should be used for accces to 
    /// resources within ASP.NET application.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Uses the <c>System.Web.HttpContext.Current.Server.MapPath</c>
    /// method to resolve the file name for a given resource.</p>
    /// <p>
    /// Note that the <c>WebResource</c> is resolved in the context of 
    /// the HTTP request it is constructed in, which means that the 
    /// relative paths will be resolved relative to the requested web 
    /// page.</p>
    /// <p>
    /// If you want the resource to be resolved relative to the 
    /// web application root, make sure that you prefix resource 
    /// name with a tilde (~) character:</p>
    /// <code>
    /// web://~/my-resource.txt
    /// </code>
    /// </remarks>
    /// <author>Aleksandar Seovic  2006.10.07</author>
    public class WebResource : FileResource
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the <b>WebResource</b> class.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the file system resource (on the server).
        /// </param>
        public WebResource(string resourceName) : base(resourceName)
        {}

        #endregion

        #region FileResource.GetFileHandle override

        /// <summary>
        /// Resolves the <b>System.IO.FileInfo</b> handle for the supplied
        /// <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the file system resource.
        /// </param>
        /// <returns>
        /// The <b>System.IO.FileInfo</b> handle for this resource.
        /// </returns>
        protected override FileInfo GetFileHandle(string resourceName)
        {
            string webPath = GetResourceNameWithoutProtocol(resourceName);

            if (HttpContext.Current != null)
            {
                return new FileInfo(HttpContext.Current.Server.MapPath(webPath));
            }
            else
            {
                return base.GetFileHandle(resourceName);
            }
        }

        #endregion
    }
}