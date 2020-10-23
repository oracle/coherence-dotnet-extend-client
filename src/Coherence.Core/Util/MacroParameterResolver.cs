/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using Tangosol.Net;
using Tangosol.Run.Xml;
using Tangosol.Util.Collections;

namespace Tangosol.Util
{
    /// <summary>
    /// An <b>XmlHelper.IParameterResolver</b> that parses and evaluates configuration
    /// macros in the format of <code>{user-defined-name [default-value]}</code>.
    /// </summary>
    /// <remarks>
    /// If the <code>user-defined-name</code> is a key within the <b>IDictionary</b>
    /// provided at construction, then the resolved value will be the value from the
    /// dictionary.  If there is no value, then the default, if any, will be returned.
    /// </remarks>
    /// <since>12.2.1.4</since>
    public class MacroParameterResolver : XmlHelper.IParameterResolver
    {
        // ----- constructors -----------------------------------------------

        /// <summary>
        /// Create a new <b>MacroParameterResolver</b> with a set of values to used
        /// in parameter replacement.
        /// </summary>
        /// <param name="attributes">key/values for macro parameter replacement</param>
        public MacroParameterResolver(IDictionary attributes)
        {
            m_attributes = attributes ?? new HashDictionary();
        }
        // ----- IParameterResolver methods ---------------------------------

        /// <summary>
        /// Attempt to resolve the provided macro in the format of <code>{user-defined-name [default-value]}</code>
        /// against the attributes provided at construction time.
        /// </summary>
        /// <param name="type">parameter type</param>
        /// <param name="value">the raw marco</param>
        /// <returns>the resolved value converted to the appropriate type</returns>
        /// <exception cref="ArgumentException">
        /// If the provided macro value is in the incorrect format.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The parameter cannot be resolved against the attributes provided at construction time and the
        /// macro does not include a default
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <code>type</code> or <code>value</code> is <code>null</code>
        /// </exception>
        public virtual object ResolveParameter(string type, string value)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type cannot be null");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value cannot be null");
            }
            
            IDictionary attributes = m_attributes;

            value = value.Trim();
            type  = type.Trim();
            
            if (value.IndexOf('{') != 0 || value.IndexOf('}') == value.Length - 2)
            {
                throw new ArgumentException(String.Format("The specified macro parameter '{0}' is invalid", value));
            }
            
            string rawParameter = value.Substring(1, value.Length - 2).Trim();
            int defaultStart = rawParameter.IndexOf(' ');
            string name = defaultStart == -1
                ? rawParameter
                : rawParameter.Substring(0, defaultStart).Trim();
            string defaultValue = defaultStart == -1
                ? null
                : rawParameter.Substring(defaultStart + 1).Trim();

            object resolvedValue = attributes[name];
            if (resolvedValue == null)
            {
                if (defaultValue == null)
                {
                    throw new ArgumentException(String.Format("The specified parameter name '{0}' in the macro " +
                                                              "parameter '{1}' is unknown and not resolvable",
                        name,
                        rawParameter));
                }

                resolvedValue = defaultValue;
                defaultValue = null;
            }

            if (resolvedValue != null)
            {
                XmlValueType expectedType = XmlHelper.LookupXmlValueType(type);
                if (expectedType == XmlValueType.Unknown)
                {
                    throw new ArgumentException("Unknown type: " + type);
                }

                object converted = XmlHelper.Convert(resolvedValue, expectedType);
                if (converted == null && defaultValue != null && expectedType != XmlValueType.Xml)
                {
                    converted = XmlHelper.Convert(defaultValue, expectedType);
                }

                if (converted != null)
                {
                    return converted;
                }
            }

            return null;
        }

        // ----- data members -----------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        protected readonly IDictionary m_attributes;
    }
}