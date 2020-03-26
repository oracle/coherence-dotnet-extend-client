/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections;

namespace Tangosol.IO.Pof.Reflection.Internal
{
    /// <summary>
    /// ITypeMetadata represents the definition of a type including 
    /// uniqueness <see cref="ITypeKey"/> and all structural properties. 
    /// This definition is used to uniformly define types and their internal 
    /// structures. Uniformity in this context is in relation to the 
    /// supported languages.
    /// </summary>
    /// <remarks>
    /// This interface defines the contract required by users of 
    /// TypeMetadata. This includes the ability to have a predictable order 
    /// for both getter and setter methods, the ability to retrieve a method,
    /// and to create a new instance of a type this metadata describes. 
    /// </remarks>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <typeparam name="T">The user type this metadata instance describes.</typeparam>
    /// <since>Coherence 3.7.1</since>
    public interface ITypeMetadata<T> where T : class, new()
    {
        /// <summary>
        /// Return a unique key for this TypeMetaData.
        /// </summary>
        /// <returns>
        /// TypeKey uniquely identifying an instance of TypeMetadata.
        /// </returns> 
        ITypeKey Key
        { 
            get;
        }

        /// <summary>
        /// Create a new instance of the object represented by this type.
        /// </summary>
        /// <returns>
        /// New object instance represented by this metadata.
        /// </returns>
        T NewInstance();

        /// <summary>
        /// Provides a predictable <see cref="IEnumerator"/> over 
        /// <see cref="IAttributeMetadata{T}"/> for the attributes of the 
        /// type represented by this TypeMetadata.
        /// </summary>
        /// <returns>
        /// <see cref="IEnumerator"/> of <see cref="IAttributeMetadata{T}"/> instances.
        /// </returns>
        IEnumerator GetAttributes();

        /// <summary>
        /// Provides a <see cref="IAttributeMetadata{T}"/> encapsulating 
        /// either the field or property accessor requested.
        /// </summary>
        /// <param name="name">
        /// Name of the attribute.
        /// </param>
        /// <returns>
        /// <see cref="IAttributeMetadata{T}"/> representing the annotated 
        /// method or field.
        /// </returns>
        IAttributeMetadata<T> GetAttribute(string name);
    }

    #region Inner interface: ITypeKey

    /// <summary>
    /// A type key embodies contributors to the uniqueness representing an
    /// ITypeMetadata instance. This is the sum of typeId, versionId and a
    /// hash.
    /// </summary>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <since>Coherence 3.7.1</since>
    public interface ITypeKey : IEquatable<ITypeKey>
    {
        /// <summary>
        /// An integer identifying a unique pof user type providing the 
        /// ability to distinguish between types using a compact form.
        /// </summary>
        /// <value>
        /// POF user type identifier.
        /// </value>
        int TypeId { get; }

        /// <summary>
        /// The version specified by the serializer when this object was
        /// serialized.
        /// </summary>
        /// <value>
        /// Integer representing the version of this POF type.
        /// </value>
        int VersionId { get; }

        /// <summary>
        /// A unique hash representing the ITypeMetadata structure.
        /// </summary>
        /// <value>
        /// Hash of ITypeMetadata.
        /// </value>
        int Hash { get; }
    }

    #endregion

    #region Inner interface: IAttributeMetadata

    /// <summary>
    /// IAttributeMetadata represents all appropriate information relating to
    /// an attribute within a type. This contract has similar forms in all
    /// supported languages providing a language agnostic mechanism to
    /// describe elements within a structure and an invocation mechanism for
    /// setting or retrieving the value for an attribute.
    /// </summary>
    /// <typeparam name="T">The container type of which this attribute is a 
    /// member.</typeparam>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <since>Coherence 3.7.1</since>
    public interface IAttributeMetadata<T> where T : class, new()
    {
        /// <summary>
        /// Name of the attribute this metadata describes.
        /// </summary>
        /// <value>
        /// Attribute name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// The versionId assigned to this attributes metadata
        /// instance. This versionId is not required however is used as an
        /// indicator to determine the version this attribute was introduced
        /// in.
        /// </summary>
        /// <value>
        /// Integer representing the version of this attribute metadata.
        /// </value>
        int VersionId { get; }

        /// <summary>
        /// The index used to order the attributes when iterated by the 
        /// containing <see cref="ITypeMetadata{T}"/> class.
        /// </summary>
        /// <value>
        /// Index to identify this attribute's position in a sequence.
        /// </value>
        int Index { get; }

        /// <summary>
        /// The codec ass;igned to this attribute which will perform type safe
        /// (de)serialization.
        /// </summary>
        /// <value>
        /// The <see cref="ICodec"/> used to (de)serialize this attribute.
        /// </value>
        ICodec Codec { get; }

        /// <summary>
        /// Returns the value of the attribute contained within the given
        /// object.
        /// </summary>
        /// <param name="container">
        /// The containing object.
        /// </param>
        /// <returns>
        /// The attribute value stored on the object passed in.
        /// </returns>
        object Get(T container);

        /// <summary>
        /// Sets the <c>value</c> of this attribute within the given object.
        ///</summary>
        /// <param name="container">
        /// The containing object.
        /// </param >
        /// <param name="attribute">
        /// The value to set this attribute to.
        /// </param>
        void Set(T container, object attribute);
    }

    #endregion
}
