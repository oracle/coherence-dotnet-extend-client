/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections;
using System.Collections.Generic;

using Tangosol.Net;

namespace Tangosol.IO.Pof.Reflection.Internal
{
    /// <summary>
    /// TypeMetadataBuilder provides a simple mechanism to
    /// instantiate and inject state into a <see cref="ITypeMetadata{T}"/> 
    /// instance. Parsers that read a source will use this builder to derive
    /// a <see cref="ITypeMetadata{T}"/> destination.
    /// </summary>
    /// <remarks>
    /// The general usage of this class is to perform multiple chained set
    /// calls with a final build call which will realize a 
    /// <see cref="ITypeMetadata{T}"/> instance.
    /// </remarks>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <typeparam name="T">The type the TypeMetadataBuilder will be 
    /// enriched using.</typeparam>
    /// <since>Coherence 3.7.1</since>
    public class TypeMetadataBuilder<T> : IRecipient<TypeMetadataBuilder<T>> 
        where T : class, new()
    {
        #region Properties

        /// <summary>
        /// Returns the <see cref="ITypeMetadata{PT}"/> in its current form, 
        /// i.e. prior to <see cref="Build"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="ITypeMetadata{PT}"/> instance being enriched.
        /// </returns>
        public virtual ITypeMetadata<T> TypeMetadata
        {
            get { return m_cmd; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new TypeMetadataBuilder.
        /// </summary>
        public TypeMetadataBuilder()
        {
            m_cmd = new TypeMetadata<T>();
            m_cmd.Key = m_key = new TypeMetadata<T>.TypeKey();
        }

        #endregion

        #region TypeMetadataBuilder methods

        /// <summary>
        /// Specify the class type this <see cref="ITypeMetadata{T}"/> is 
        /// assigned to.
        /// </summary>
        /// <param name="type">
        /// Type that the resulting <see cref="ITypeMetadata{T}"/> instance 
        /// describes.
        /// </param>
        /// <returns>
        /// A reference to this for chained set calls.
        /// </returns>
        /// <seealso cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TyepInfo"/>
        public virtual TypeMetadataBuilder<T> SetType(Type type)
        {
            m_cmd.TyepInfo = type;
            return this;
        }

        /// <summary>
        /// Add an <see cref="IAttributeMetadata{PT}"/> instance that is a 
        /// child of the <see cref="ITypeMetadata{T}"/> instance.
        /// </summary>
        /// <param name="attribute">
        /// <see cref="IAttributeMetadata{PT}"/> implementation to add to the
        /// enclosing <see cref="ITypeMetadata{T}"/> instance.
        /// </param>
        /// <returns>
        /// A reference to this for chained set calls.
        /// </returns>
        /// <seealso cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.AddAttribute"/>
        public virtual TypeMetadataBuilder<T> AddAttribute(IAttributeMetadata<T> attribute)
        {
            m_cmd.AddAttribute(attribute);
            return this;
        }

        /// <summary>
        /// Creates a new attribute builder for populating an
        /// <see cref="IAttributeMetadata{PT}"/> instance. 
        /// </summary>
        /// <returns>
        /// A ClassAttributeBuilder that builds an attribute.
        /// </returns>
        public virtual ClassAttributeBuilder NewAttribute()
        {
            return new ClassAttributeBuilder();
        }

        /// <summary>
        /// Specify the unique type id for the <see cref="ITypeKey"/>.
        /// </summary>
        /// <param name="typeId">
        /// Type id used in uniquely identifying a 
        /// <see cref="ITypeMetadata{PT}"/> instance.
        /// </param>
        /// <returns>
        /// A reference to this for chained set calls.
        /// </returns>
        /// <seealso cref="ITypeKey.TypeId"/>
        public virtual TypeMetadataBuilder<T> SetTypeId(int typeId)
        {
            m_key.TypeId = typeId;
            return this;
        }

        /// <summary>
        /// Specify the version for this <see cref="ITypeMetadata{PT}"/> 
        /// instance.
        /// </summary>
        /// <param name="versionId">
        /// The version of this <see cref="ITypeMetadata{PT}"/> instance.
        /// </param>
        /// <returns>
        /// A reference to this for chained set calls.
        /// </returns>
        /// <seealso cref="ITypeKey.VersionId"/>
        public virtual TypeMetadataBuilder<T> SetVersionId(int versionId)
        {
            m_key.VersionId = versionId;
            return this;
        }

        /// <summary>
        /// Specify the hash for this <see cref="ITypeMetadata{PT}"/> 
        /// instance.
        /// </summary>
        /// <param name="hash">
        /// A hash value of the <see cref="ITypeMetadata{PT}"/> instance.
        /// </param>
        /// <returns>
        /// A reference to this for chained set calls.
        /// </returns>
        /// <seealso cref="ITypeKey.Hash"/>
        public virtual TypeMetadataBuilder<T> SetHash(int hash)
        {
            m_key.Hash = hash;
            return this;
        }

        /// <summary>
        /// Based on the state that the builder has been informed of create 
        /// and return a <see cref="ITypeMetadata{T}"/> instance.
        /// </summary>
        /// <returns>
        /// The built <see cref="ITypeMetadata{T}"/> instance.
        /// </returns>
        public virtual TypeMetadata<T> Build()
        {
            var cmd = m_cmd;
            var key = (TypeMetadata<T>.TypeKey) cmd.Key;

            // now that we are aware of entirety of this TypeMetadata instance
            // determine the appropriate indexes or ensure they are explicitly
            // defined
            IList<IAttributeMetadata<T>> nonSortedAttributes = new List<IAttributeMetadata<T>>();

            // create an exclusion list of indexes that are explicitly defined, i.e.
            // we must be aware of the obstacles on the road to determining POF binary
            // structure
            ICollection<int> reservedIndexes = new HashSet<int>();

            for (IEnumerator enmrAttr = cmd.GetAttributes(); enmrAttr.MoveNext(); )
            {
                var attr = (TypeMetadata<T>.TypeAttribute) enmrAttr.Current;
                if (attr.Index >= 0)
                {
                    int proposedIndex, attributeIndex = proposedIndex = attr.Index;
                    while (reservedIndexes.Contains(proposedIndex))
                    {
                        ++proposedIndex;
                    }

                    if (proposedIndex != attributeIndex)
                    {
                        if (CacheFactory.IsLogEnabled(CacheFactory.LogLevel.Debug))
                        {
                            CacheFactory.Log("The requested index " + attributeIndex
                                    + " on a PortableProperty annotation " 
                                    + "for [typeId=" + key.TypeId
                                    + ", version=" + key.VersionId 
                                    + ", property-name=" + attr.Name
                                    + "] is already allocated to an existing PortableProperty. "
                                    + "Allocated index " + proposedIndex + " instead.",
                                    CacheFactory.LogLevel.Debug);
                        }
                        attr.Index = proposedIndex;
                    }
                    reservedIndexes.Add(proposedIndex);
                }
            }

            int i = 0;
            for (IEnumerator attributes = cmd.GetAttributes(); attributes.MoveNext(); ++i)
            {
                var attr = (TypeMetadata<T>.TypeAttribute) attributes.Current;
                if (attr.Index < 0)
                {
                    for (; reservedIndexes.Contains(i); ++i) { }
                    attr.Index = i;
                }
                nonSortedAttributes.Add(attr);
            }
            cmd.SetAttributes(nonSortedAttributes);

            // inform key of the hash of the class structure now that we are primed
            key.Hash = cmd.GetHashCode();

            m_cmd = new TypeMetadata<T>();
            m_cmd.Key = m_key = new TypeMetadata<T>.TypeKey();

            return cmd;
        }

        #endregion

        #region IRecipient members

        /// <summary>
        /// Accept the given visitor.
        /// </summary>
        /// <param name="visitor">
        /// IVisitor that is requesting to visit this recipient.
        /// </param>
        /// <param name="type">
        /// The Type that can be used by the visitor.
        /// </param>
        public virtual void Accept(IVisitor<TypeMetadataBuilder<T>> visitor, Type type)
        {
            Type        typeRecipient = type;
            IList<Type> hierarchy     = new List<Type>();
            while (typeRecipient != null && !typeof(object).Equals(typeRecipient))
            {
                hierarchy.Add(typeRecipient);
                typeRecipient = typeRecipient.BaseType;
            }

            // walk the hierarchy from the root
            for (int i = hierarchy.Count - 1; i >= 0; --i)
            {
                visitor.Visit(this, hierarchy[i]);
            }
        }

        #endregion

        #region Inner class: ClassAttributeBuilder

        /// <summary>
        /// The ClassAttributeBuilder provide the ability to build a
        /// <see cref="IAttributeMetadata{PT}"/> implementation.
        /// </summary>
        /// <author>Harvey Raja  2011.07.25</author>
        /// <since>Coherence 3.7.1</since>
        public class ClassAttributeBuilder
        {
            #region Constructors

            /// <summary>
            /// Construct a ClassAttributeBuilder instance.
            /// </summary>
            public ClassAttributeBuilder()
            {
                m_attribute = new TypeMetadata<T>.TypeAttribute();
            }

            #endregion

            #region ClassAttributeBuilder methods

            /// <summary>
            /// Specify the normalized name of the 
            /// <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute"/>
            /// instance.
            /// </summary>
            /// <param name="name">
            /// The normalized name of the 
            /// <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute"/>
            /// instance.
            /// </param>
            /// <returns>
            /// A reference to this for chained set calls.
            /// </returns>
            /// <seealso cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute.Name"/>
            public virtual ClassAttributeBuilder SetName(String name)
            {
                m_attribute.Name = name;
                return this;
            }

            /// <summary>
            /// Specify the versionId of this 
            /// <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute"/> 
            /// instance.
            /// </summary>
            /// <param name="version">
            /// Version of the 
            /// <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute"/>
            /// instance.
            /// </param>
            /// <returns>
            /// A reference to this for chained set calls.
            /// </returns>
            /// <seealso cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute.VersionId"/>
            public virtual ClassAttributeBuilder SetVersion(int version)
            {
                m_attribute.VersionId = version;
                return this;
            }

            /// <summary>
            /// Specify the index of this 
            /// <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute"/>
            /// instance used to sequence many 
            /// <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute"/>
            /// instances.
            /// </summary>
            /// <param name="index">
            /// Index to specify this attributes sequence number.
            /// </param>
            /// <returns>
            /// A reference to this for chained set calls.
            /// </returns>
            /// <seealso cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute.Index"/>
            public virtual ClassAttributeBuilder SetIndex(int index)
            {
                m_attribute.Index = index;
                return this;
            }

            /// <summary>
            /// Specify the <see cref="ICodec"/> to use for this 
            /// <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute"/>
            /// instance.
            /// </summary>
            /// <param name="codec">
            /// The codec to use for this 
            /// <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute"/>
            /// instance.
            /// </param>
            /// <returns>
            /// A reference to this for chained set calls.
            /// </returns>
            /// <seealso cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute.Codec"/>
            public virtual ClassAttributeBuilder SetCodec(ICodec codec)
            {
                m_attribute.Codec = codec;
                return this;
            }

            /// <summary>
            /// Specify the <see cref="IInvocationStrategy{T}"/> 
            /// implementation that allows values to be written and received 
            /// to the attribute.
            /// </summary>
            /// <param name="strategy">
            /// The strategy provides an implementation to write and receive 
            /// values.
            /// </param>
            /// <returns>
            /// A reference to this for chained set calls.
            /// </returns>
            /// <seealso cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute.InvocationStrategy"/>
            public virtual ClassAttributeBuilder SetInvocationStrategy(IInvocationStrategy<T> strategy)
            {
                m_attribute.InvocationStrategy = strategy;
                return this;
            }

            /// <summary>
            /// Create a 
            /// <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute"/> 
            /// instance based on the values set during the lifetime of this 
            /// builder.
            /// </summary>
            /// <returns>
            /// An enriched 
            /// <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute"/> 
            /// instance.
            /// </returns>
            public virtual TypeMetadata<T>.TypeAttribute Build()
            {
                TypeMetadata<T>.TypeAttribute attribute = m_attribute;
                m_attribute = new TypeMetadata<T>.TypeAttribute();
                
                return attribute;
            }

            #endregion

            #region Data members

            /**
             * <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeAttribute"/> 
             * that is built 
             * across the duration of  ClassAttributeBuilder calls until it 
             * is returned via the <see cref="Build"/> method.
             */
            private TypeMetadata<T>.TypeAttribute m_attribute;
            
            #endregion
        }

        # endregion

        #region Data members

        /**
         * <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}"/>
         * that is built across the duration 
         * of TypeMetadataBuilder calls until it is returned via the
         * <see cref="Build"/> method.
         */
        private TypeMetadata<T> m_cmd;

        /**
         * <see cref="Tangosol.IO.Pof.Reflection.Internal.TypeMetadata{T}.TypeKey"/> 
         * that is built across the 
         * duration of TypeMetadataBuilder calls until it is returned via 
         * the <see cref="Build"/> method.
         */
        private TypeMetadata<T>.TypeKey m_key;
        
        #endregion
    }
}
