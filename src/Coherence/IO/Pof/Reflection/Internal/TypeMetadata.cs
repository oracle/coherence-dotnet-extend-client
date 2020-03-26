/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Tangosol.Util;
using Tangosol.Util.Collections;

namespace Tangosol.IO.Pof.Reflection.Internal
{
    /// <summary>
    /// A <see cref="ITypeMetadata{T}"/> implementation coupled to the .NET 
    /// type metadata definition language: <see cref="Type"/>,
    /// <see cref="PropertyInfo"/>, <see cref="FieldInfo"/>, 
    /// and <see cref="MethodInfo"/>.
    /// </summary>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <typeparam name="T">The Type being visited.</typeparam>
    /// <since>Coherence 3.7.1</since>
    public class TypeMetadata<T> : ITypeMetadata<T>, IEquatable<ITypeMetadata<T>> 
        where T : class, new()
    {
        #region ITypeMetadata implementation

        /// <summary>
        /// Return a unique key for this ITypeMetaData.
        /// </summary>
        /// <value>
        /// ITypeKey uniquely identifying an instance of ITypeMetadata.
        /// </value> 
        public virtual ITypeKey Key
        {
            get; set;
        }

        /// <summary>
        /// Create a new instance of the object represented by this type.
        /// </summary>
        /// <returns>
        /// New object instance represented by this metadata.
        /// </returns>
        public virtual T NewInstance()
        {
            ConstructorInfo ci = TyepInfo.GetConstructor(new Type[0]);
            if (ci == null)
            {
                throw new ArgumentException("Type " + TyepInfo + " must have a no-arg constructor");
            }
            return (T) ci.Invoke(null);
        }

        /// <summary>
        /// Provides a predictable <see cref="IEnumerator"/> over 
        /// <see cref="IAttributeMetadata{T}"/>
        /// for the attributes of the type represented by this TypeMetadata.
        /// </summary>
        /// <returns>
        /// <see cref="IEnumerator"/> of <see cref="IAttributeMetadata{T}"/> 
        /// instances.
        /// </returns>
        public virtual IEnumerator GetAttributes()
        {
            return m_attributes.GetEnumerator();
        }

        /// <summary>
        /// Provides a <see cref="IAttributeMetadata{T}"/> encapsulating 
        /// either the property, field or method requested.
        /// </summary>
        /// <param name="name">
        /// Name of the attribute.
        /// </param>
        /// <returns>
        /// <see cref="IAttributeMetadata{T}"/> representing the annotated 
        /// method or field.
        /// </returns>
        public virtual IAttributeMetadata<T> GetAttribute(string name)
        {
            return m_attributesByName[name];
        }
        #endregion 

        #region Accessors

        /// <summary>
        /// Specify the <see cref="Type"/> that uniquely identifies this metadata.
        /// </summary>
        /// <value>
        /// Type that defines this type.
        /// </value>
        public virtual Type TyepInfo
        { 
            get; set;
        }

        /// <summary>
        /// Specify all <see cref="IAttributeMetadata{T}"/> instances that 
        /// represent this type.
        ///</summary>
        /// <param name="attributes">
        /// Attribute metadata information to enrich this metadata.
        /// </param>
        public virtual void SetAttributes(ICollection<IAttributeMetadata<T>> attributes)
        {
            var typeAttributes   = m_attributes;
            var attributesByName = m_attributesByName;
            typeAttributes.Clear();   
            attributesByName.Clear();

            foreach (var attribute in attributes)
            {
                typeAttributes.Add(attribute);
                attributesByName[attribute.Name] = attribute;
            }
        }

        /// <summary>
        /// Add an attribute to this TypeMetadata.
        /// </summary>
        /// <param name="attribute">
        /// Attribute metadata definition to add.
        /// </param>
        /// <returns>
        /// Whether the attribute metadata was added.
        /// </returns>
        public virtual bool AddAttribute(IAttributeMetadata<T> attribute)
        {
            var typeAttributes = m_attributes;
            bool add           = !typeAttributes.Contains(attribute);

            if (add)
            {
                typeAttributes.Add(attribute);
                m_attributesByName[attribute.Name] = attribute;
            }
            return add;
        }
        #endregion

        #region IEquatable members

        /// <summary>
        /// Compare the TypeMetadata with another TypeMetadata object to 
        /// determine equality.
        /// </summary>
        /// <remarks>
        /// Two TypeMetadata objects are considered equal iff their 
        /// <see cref="ITypeKey"/> values and 
        /// <see cref="IAttributeMetadata{T}"/>s are equal.
        /// </remarks>
        /// <param name="that">
        /// ITypeMetadata instance to compare this instance to.
        /// </param>
        /// <returns>
        /// <c>true</c> iff this TypeMetadata and the passed object are 
        /// equivalent.
        /// </returns>
        public virtual bool Equals(ITypeMetadata<T> that)
        {
            if (this == that)
            {
                return true;
            }

            // check key and version
            if (!Equals(Key, that.Key))
            {
                return false;
            }

            for (IEnumerator enmrThis = GetAttributes(), enmrThat = that.GetAttributes(); ; )
            {
                var attributeThis = enmrThis.MoveNext() ? enmrThis.Current : null;
                var attributeThat = enmrThat.MoveNext() ? enmrThat.Current : null;
                if (!Equals(attributeThis, attributeThat))
                {
                    return false;
                }
                // we assume an attribute returned by the enmr will never be null
                if (attributeThis == null && attributeThat == null)
                {
                    break;
                }
            }
            return true;
        }

        #endregion

        #region Object methods

        /// <summary>
        /// Determine a hash value for the TypeMetadata object according to
        /// the general <see cref="object.GetHashCode"/> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this TypeMetadata instance.
        /// </returns>
        public override int GetHashCode()
        {
            ITypeKey key  = Key;
            int      hash = HashHelper.Hash(key.TypeId, 31);
                     hash = HashHelper.Hash(key.VersionId, hash);
                     hash = HashHelper.Hash(m_attributes, hash);
            return hash;
        }

        /// <summary>
        /// Compare the TypeMetadata with another object to determine 
        /// equality.
        /// </summary>
        /// <remarks>
        /// Two TypeMetadata objects are considered equal iff their 
        /// <see cref="ITypeKey"/> values and 
        /// <see cref="IAttributeMetadata{T}"/>s are equal.
        /// </remarks>
        /// <param name="that">
        /// Object to compare this TypeMetadata to.
        /// </param>
        /// <returns>
        /// <c>true</c> iff this TypeMetadata and the passed object are equivalent.
        /// </returns>
        public override bool Equals(object that)
        {
            return that is ITypeMetadata<T> ? Equals((ITypeMetadata<T>)that) : false;
        }

        /// <summary>
        /// Return a human-readable description for this TypeMetadata.
        /// </summary>
        /// <returns>
        /// A string description of the TypeMetadata.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} (key={1}, attributes={2})",
                GetType().Name, Key, m_attributes);
        }

        #endregion

        #region Inner class: TypeKey

        /// <summary>
        /// A TypeKey contains information to uniquely identify this  
        /// type instance. Specifically unique identification is a product of
        /// <code>typeId + version + type-hash</code>.
        /// </summary>
        /// <author>Harvey Raja  2011.07.25</author>
        /// <since>Coherence 3.7.1</since>
        public class TypeKey : ITypeKey
        {

            #region ITypeKey

            /// <summary>
            /// An integer identifying a unique pof user type providing the
            /// ability to distinguish between types using a compact form.
            /// </summary>
            /// <value>
            /// POF user type identifier.
            /// </value>
            public virtual int TypeId
            {
                get; set;
            }

            /// <summary>
            /// The version specified by the serializer when this object was
            /// serialized.
            /// </summary>
            /// <value>
            /// Integer representing the version of this POF type.
            /// </value>
            public virtual int VersionId
            {
                get; set;
            }

            /// <summary>
            /// A unique hash representing the TypeMetadata structure.
            /// </summary>
            /// <value>
            /// Hash of TypeMetadata.
            /// </value>
            public virtual int Hash
            {
                get; set;
            }

            #endregion

            #region IEquatable memebers

            /// <summary>
            /// Compare this TypeKey with another object to determine 
            /// equality.
            /// </summary>
            /// <remarks>
            /// Two TypeKey objects are considered equal iff their 
            /// <see cref="TypeId"/> and <see cref="VersionId"/> are equal.
            /// </remarks>
            /// <param name="that">
            /// ITypeKey instance to compare this instance to.
            /// </param>
            /// <returns>
            /// <c>true</c> iff this TypeKey and the passed object are 
            /// equivalent.
            /// </returns>
            public virtual bool Equals(ITypeKey that)
            {
                return TypeId == that.TypeId && VersionId == that.VersionId;
            }

            #endregion 

            #region Object members

            /// <summary>
            /// Compare this TypeKey with another object to determine 
            /// equality.
            /// </summary>
            /// <remarks>
            /// Two TypeKey objects are considered equal iff their 
            /// <see cref="TypeId"/>
            /// and <see cref="VersionId"/> are equal.
            /// </remarks>
            /// <param name="that">
            /// Object instance to compare this instance to.
            /// </param>
            /// <returns>
            /// <c>true</c> iff this TypeKey and the passed object are 
            /// equivalent.
            /// </returns>
            public override bool Equals(object that)
            {
                return that is ITypeKey ? Equals((ITypeKey) that) : false;
            }

            /// <summary>
            /// Serves as a hash function for TypeKey.
            /// </summary>
            /// <returns>
            /// A hash code for this instance of TypeKey.
            /// </returns>
            public override int GetHashCode()
            {
                // the class key will use the hash to determine uniqueness
                // whereas the ClassMetadata will not call this hashCode to
                // determine its hash but instead uses the appropriate elements
                int hash =  HashHelper.Hash(TypeId, 31);
                    hash =  HashHelper.Hash(VersionId, hash);
                    hash =  HashHelper.Hash(Hash, hash);
                return hash;
            }

            #endregion
        }

        #endregion

        #region Inner class: TypeAttribute

        /// <summary>
        /// An <see cref="IAttributeMetadata{T}"/> implementation acting as 
        /// a container for attribute inspection and invocation.
        /// </summary>
        /// <author>Harvey Raja  2011.07.25</author>
        /// <since>Coherence 3.7.1</since>
        /// <seealso cref="IAttributeMetadata{T}"/>
        public class TypeAttribute : IAttributeMetadata<T>,
                                     IEquatable<IAttributeMetadata<T>>,
                                     IComparable
                                             
        {
            #region Properties

            /// <summary>
            /// Name of the attribute this metadata describes.
            /// </summary>
            /// <value>
            /// The normalized name of the attribute.
            /// </value>
            public string Name
            {
                get; set;
            }

            /// <summary>
            /// Returns the versionId assigned to this attributes metadata
            /// instance. This versionId is not required however is used as 
            /// an indicator to determine the version this attribute was 
            /// introduced in.
            /// </summary>
            /// <value>
            /// Integer representing the version of this attribute metadata.
            /// </value>
            public int VersionId
            {
                get; set;
            }

            /// <summary>
            /// The index used to order the attributes when iterated by the
            /// containing <see cref="ITypeMetadata{T}"/> class.
            /// </summary>
            /// <value>
            /// Index to identify this attribute's position in a sequence.
            /// </value>
            public int Index
            {
                get; set;
            }

            /// <summary>
            /// The codec assigned to this attribute which will perform type
            /// safe (de)serialization.
            /// </summary>
            /// <value>
            /// The <see cref="ICodec"/> used to (de)serialize this attribute.
            /// </value>
            public ICodec Codec
            {
                get; set;
            }

            /// <summary>
            /// Specify an <see cref="IInvocationStrategy{T}"/>.
            /// </summary>
            /// <value>
            /// The invocation strategy to use to get and set values.
            /// </value>
            public IInvocationStrategy<T> InvocationStrategy
            {
                get; set;
            }

            #endregion

            #region IAttributeMetadata implementation

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
            public virtual object Get(T container)
            {
                return InvocationStrategy.Get(container);
            }

            /// <summary>
            /// Sets the <c>value</c> of this attribute within the given object.
            ///</summary>
            /// <param name="container">
            /// The containing object.
            /// </param>
            /// <param name="o">
            /// The value to set this attribute to.
            /// </param>
            public virtual void Set(T container, object o)
            {
                InvocationStrategy.Set(container, o);
            }

            #endregion

            #region IComparable members

            /// <summary>
            /// Compares the current instance with another object of the same
            /// type.
            /// </summary>
            /// <returns>
            /// A 32-bit signed integer that indicates the relative order of 
            /// the objects being compared. The return value has these 
            /// meanings: Value Meaning Less than zero This instance is less 
            /// than <paramref name="o"/>. Zero This instance is equal to 
            /// <paramref name="o"/>. Greater than zero This instance is 
            /// greater than <paramref name="o"/>. 
            /// </returns>
            /// <param name="o">
            /// An object to compare with this instance. 
            /// </param>
            /// <exception cref="T:System.ArgumentException">
            /// <paramref name="o"/> is not the same type as this 
            /// instance.
            /// </exception>
            /// <filterpriority>
            /// 2
            /// </filterpriority>
            public virtual int CompareTo(object o)
            {
                return o is IAttributeMetadata<T> ? CompareTo((IAttributeMetadata<T>) o) : 1;
            }

            /// <summary>
            /// Compares the current object with another object of the same type.
            /// </summary>
            /// <remarks>
            /// Sorting of attributes is determined by:
            /// <list type="number">
            ///     <item>version</item>
            ///     <item>index</item>
            ///     <item>name</item>
            /// </list>
            /// </remarks>
            /// <returns>
            /// A 32-bit signed integer that indicates the relative order of 
            /// the objects being compared. The return value has the 
            /// following meanings: Value Meaning Less than zero This object 
            /// is less than the <paramref name="that"/> parameter.Zero This 
            /// object is equal to <paramref name="that"/>. Greater than 
            /// zero This object is greater than <paramref name="that"/>. 
            /// </returns>
            /// <param name="that">
            /// An object to compare with this object.
            /// </param>
            public virtual int CompareTo(IAttributeMetadata<T> that)
            {
                if (that == null)
                {
                    return 1;
                }

                if (that == this)
                {
                    return 0;
                }

                int n = VersionId - that.VersionId;
                if (n == 0)
                {
                    n = Index - that.Index;
                    if (n == 0)
                    {
                        string thisName = Name;
                        string thatName = that.Name;

                        n = thisName == null
                            ? (thatName == null ? 0 : -1)
                            : (thatName == null ? 1 : thisName.CompareTo(thatName));
                    }
                }
                return n;
            }

            #endregion

            #region IEquatable members

            /// <summary>
            /// Compare this TypeAttribute with another object to determine 
            /// equality.
            /// </summary>
            /// <remarks>
            /// Two TypeAttribute objects are considered equal iff their 
            /// <see cref="Name"/>, <see cref="Index"/> and 
            /// <see cref="VersionId"/> are equal.
            /// </remarks>
            /// <param name="that">
            /// IAttributeMetadata instance to compare this instance to.
            /// </param>
            /// <returns>
            /// <c>true</c> iff this TypeAttribute and the passed object are equivalent.
            /// </returns>
            public virtual bool Equals(IAttributeMetadata<T> that)
            {
                if (this == that)
                {
                    return true;
                }
                return Equals(VersionId, that.VersionId) && Equals(Index, that.Index)
                       && Equals(Name, that.Name);
            }

            #endregion

            #region Object members

            /// <summary>
            /// Compare this TypeAttribute with another object to determine 
            /// equality.
            /// </summary>
            /// <remarks>
            /// Two TypeAttribute objects are considered equal iff their 
            /// <see cref="Name"/>, <see cref="Index"/> and 
            /// <see cref="VersionId"/> are equal.
            /// </remarks>
            /// <param name="o">
            /// IAttributeMetadata instance to compare this instance to.
            /// </param>
            /// <returns>
            /// <c>true</c> iff this TypeAttribute and the passed object are equivalent.
            /// </returns>
            public override bool Equals(object o)
            {
                return o is IAttributeMetadata<T> ? Equals((IAttributeMetadata<T>) o) : false;
            }

            /// <summary>
            /// Serves as a hash function for TypeAttribute.
            /// </summary>
            /// <returns>
            /// A hash code for this instance of TypeAttribute.
            /// </returns>
            public override int GetHashCode()
            {
                int hash = HashHelper.Hash(VersionId, 31);
                    hash = HashHelper.Hash(Index, hash);
                    hash = HashHelper.Hash(Name, hash);
                return hash;
            }

            /// <summary>
            /// Return a human-readable description for this TypeAttribute.
            /// </summary>
            /// <returns>
            /// A string description of the TypeAttribute.
            /// </returns>
            public override string ToString()
            {
                return string.Format("TypeAttribute (name={0}, version={1}, index={2})",
                                     Name, VersionId, Index);
            }

            #endregion
        }

        #endregion

        #region Data members

        /**
         * All attributes within this typeIterator.
         */
        private readonly SortedHashSet m_attributes = new SortedHashSet();

        /**
         * A reference store for efficient lookup from attribute name to metadata.
         */
        private readonly IDictionary<string, IAttributeMetadata<T>> m_attributesByName
            = new Dictionary<string, IAttributeMetadata<T>>();

        #endregion
    }
}
