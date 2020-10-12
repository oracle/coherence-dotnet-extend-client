/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Util
{
    /// <summary>
    /// Miscellaneuos utility methods for <b>Uri</b> manipulation.
    /// </summary>
    /// <author>Ana Cikic  2006.08.23</author>
    public abstract class UriUtils
    {
        /// <summary>
        /// Returns the decoded scheme-specific part of the <b>Uri</b>.
        /// </summary>
        /// <remarks>
        /// At the highest level a URI reference in string form has the
        /// syntax:
        ///
        /// [scheme:]scheme-specific-part[#fragment]
        ///
        /// where square brackets [...] delineate optional components and the
        /// characters : and # stand for themselves.
        /// </remarks>
        /// <param name="uri">
        /// <b>Uri</b> object.
        /// </param>
        /// <returns>
        /// Scheme-specific part of the <b>Uri</b>.
        /// </returns>
        public static string GetSchemeSpecificPart(Uri uri)
        {
            string schema   = uri.Scheme;
            string fragment = uri.Fragment;
            string result   = uri.ToString();

            if (schema != null && schema.Length > 0)
            {
                string fullSchema = schema + ":";
                result = result.Substring(result.IndexOf(fullSchema) + fullSchema.Length);
            }
            if (fragment != null && fragment.Length > 0)
            {
                result = result.Substring(0, result.IndexOf("#"));
            }
            return result;
        }
    }
}