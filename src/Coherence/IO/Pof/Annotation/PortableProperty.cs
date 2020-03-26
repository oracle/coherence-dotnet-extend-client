/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Generic;

using Tangosol.IO.Pof.Reflection;

namespace Tangosol.IO.Pof.Annotation
{
    /// <summary>
    /// A PortableProperty marks a member variable or method accessor as a
    /// POF serialized attribute. Whilst the <see cref="Index"/> and
    /// <see cref="ICodec"/> can be explicitly specified they can be
    /// determined by classes that use this annotation. Hence these 
    /// attributes serve as hints to the underlying parser. 
    /// </summary>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <since>Coherence 3.7.1</since>
    /// <see>PortableProperty</see>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public class PortableProperty : Attribute
    {
        #region Properties

        /// <summary>
        /// The index of this property.
        /// </summary>
        /// <seealso cref="IPofWriter"/>
        public int Index
        {
            get { return m_index; }
        }

        /// <summary>
        /// A codec to use to short-circuit determining the type via either
        /// method return type or field type.
        /// </summary>
        /// <remarks>
        /// This could be used to determine concrete implementations of
        /// interfaces, i.e. when the method return is a 
        /// <see cref="IList{T}"/> this type definition could instruct the 
        /// code to utilize a <see cref="LinkedList{T}"/>.
        /// </remarks>
        /// <returns>
        /// A <see cref="Type"/> that should be assingable to
        /// <see cref="ICodec"/>.
        /// </returns>
        /// <seealso cref="ICodec"/>
        public Type Codec
        {
            get { return m_codec; }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a PortableProperty attribute with a default index
        /// value of -1.
        /// </summary>
        public PortableProperty()
            : this(-1)
        {
        }

        /// <summary>
        /// Construct a PortableProperty attribute with a 
        /// <see cref="Codecs.DefaultCodec"/>.
        /// </summary>
        /// <param name="index">
        /// The POF index of this portable property.
        /// </param>
        public PortableProperty(int index)
            : this(index, typeof(Codecs.DefaultCodec))
        {
        }

        /// <summary>
        /// Construct a PortableProperty attribute with the specified
        /// <see cref="ICodec"/> Type. Defaults the index to -1.
        /// </summary>
        /// <param name="codec">
        /// Type of the ICodec used to encode/decode the property.
        /// </param>
        public PortableProperty(Type codec)
            : this(-1, codec)
        {
        }

        /// <summary>
        /// Construct a PortableProperty attribute using the specified
        /// index and <see cref="ICodec"/> Type.
        /// </summary>
        /// <param name="index">
        /// The POF index of this portable property.
        /// </param>
        /// <param name="codec">
        /// Type of the ICodec used to encode/decode the property.
        /// </param>
        public PortableProperty(int index, Type codec)
        {
            m_index = index;
            m_codec = codec;
        }

        #endregion

        #region Data Members

        /// <summary>
        /// The POF index of this PortableProperty.
        /// </summary>
        private readonly int m_index;
        
        /// <summary>
        /// Type of the ICodec used to encode/decode the property.
        /// </summary>
        private readonly Type m_codec;

        #endregion
    }
}
