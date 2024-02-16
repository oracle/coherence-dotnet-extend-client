/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;

namespace Tangosol.IO.Pof.Reflection.Internal
{
    /// <summary>
    /// NameManglers contain singleton access to both a
    /// <see cref="FieldMangler"/> and <see cref="MethodMangler"/>. 
    /// NameManglers provide the ability to derive the same name of a 
    /// property regardless of their access or inspection methodology.
    /// </summary>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <since>Coherence 3.7.1</since>
    public class NameManglers
    {
        /// <summary>
        /// Static initialization of <see cref="FieldMangler"/> and 
        /// <see cref="MethodMangler"/>.
        /// </summary>
        static NameManglers()
        {
            FIELD_MANGLER    = new FieldMangler();
            METHOD_MANGLER   = new MethodMangler();
            PROPERTY_MANGLER = new PropertyMangler();
        }

        #region Inner class: PropertyMangler

        /// <summary>
        /// A <see cref="INameMangler"/> implementation that is aware of 
        /// property naming conventions and is able to convert from a method 
        /// name to a generic name.
        /// </summary>
        /// <remarks>
        /// The convention this mangler is aware of is property names being
        /// equivalent to the required name except with the first character 
        /// being in upper case, e.g. <c>Foo</c> will be converted to 
        /// <c>foo</c>.
        /// </remarks>
        /// <author>Harvey Raja  2011.07.25</author>
        /// <since>Coherence 3.7.1</since>
        public class PropertyMangler : INameMangler
        {
            #region INameMangler methods

            /// <summary>
            /// Convert the given string to a new string using a convention 
            /// determined by the implementer.
            /// </summary>
            /// <param name="name">
            /// Original string.
            /// </param>
            /// <returns>
            /// Mangled string.
            /// </returns>
            public string Mangle(string name)
            {
                if (name == null)
                {
                    return null;
                }

                string sMangledName = name;
                if (Char.IsUpper(sMangledName[0]))
                {
                    sMangledName = Char.ToLower(sMangledName[0]) + sMangledName.Substring(1);
                }
                return sMangledName;
            }

            #endregion
        }

        #endregion

        #region Inner class: FieldMangler

        /// <summary>
        /// A <see cref="INameMangler"/> implementation that is aware of 
        /// field naming conventions and is able to convert from a field name
        /// to a generic name.
        /// </summary>
        /// <remarks>
        /// The conventions this mangler is aware of are prefixing variables 
        /// with <c>m_</c>. For example <c>m_bar</c> and 
        /// would be converted to a mangled name of <c>bar</c>.
        /// </remarks>
        /// <author>Harvey Raja  2011.07.25</author>
        /// <since>Coherence 3.7.1</since>
        public class FieldMangler : INameMangler
        {
            #region INameMangler methods

            /// <summary>
            /// Convert the given string to a new string using a convention 
            /// determined by the implementer.
            /// </summary>
            /// <param name="name">
            /// Original string.
            /// </param>
            /// <returns>
            /// Mangled string.
            /// </returns>
            public virtual string Mangle(string name)
            {
                string sMangledName = name;
                if (name.StartsWith("m_"))
                {
                    sMangledName = Char.IsLower(name[2]) && Char.IsUpper(name[3])
                                       ? Char.ToLower(name[3]) + name.Substring(4)
                                       : Char.ToLower(name[2]) + name.Substring(3);
                }
                return sMangledName;
            }

            #endregion
        }

        #endregion

        #region Inner class: MethodMangler

        /// <summary>
        /// A <see cref="INameMangler"/> implementation that is aware of 
        /// method naming conventions and is able to convert from a method 
        /// name to a generic name.
        /// </summary>
        /// <remarks>
        /// The conventions this mangler is aware of are the getter and 
        /// setter style methods, e.g. <c>getBar</c> or <c>setBar</c> which 
        /// are both converted to a mangled name of <c>bar</c>.
        /// </remarks>
        /// <author>Harvey Raja  2011.07.25</author>
        /// <since>Coherence 3.7.1</since>
        public class MethodMangler : INameMangler
        {
            #region INameMangler methods

            /// <summary>
            /// Convert the given string to a new string using a convention 
            /// determined by the implementer.
            /// </summary>
            /// <param name="name">
            /// Original string.
            /// </param>
            /// <returns>
            /// Mangled string.
            /// </returns>
            public string Mangle(string name)
            {
                if (name == null)
                {
                    return null;
                }

                string sMangledName = name;
                if (sMangledName.StartsWith("Get") || sMangledName.StartsWith("Set"))
                {
                    sMangledName = Char.ToLower(sMangledName[3]) + sMangledName.Substring(4);
                }
                else if (sMangledName.StartsWith("Is"))
                {
                    sMangledName = Char.ToLower(sMangledName[2]) + sMangledName.Substring(3);
                }
                return sMangledName;
            }

            #endregion
        }

        #endregion

        /// <summary>
        /// Singleton PropertyMangler reference.
        /// </summary>
        public static readonly INameMangler PROPERTY_MANGLER;

        /// <summary>
        /// Singleton FieldMangler reference.
        /// </summary>
        public static readonly INameMangler FIELD_MANGLER;

        /// <summary>
        /// Singleton MethodMangler reference.
        /// </summary>
        public static readonly INameMangler METHOD_MANGLER;
    }
}
