/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util;
using Tangosol.Util.Collections;

namespace Tangosol.Web.Model
{
    /// <summary>
    /// Implementation of a <see cref="ISessionModel"/>
    /// that deserializes individual items on demand and stores
    /// attributes that exceed certain size as separate cache
    /// entries.
    /// </summary>
    /// <author>Aleksandar Seovic  2008.10.18</author>
    public class SplitSessionModel : TraditionalSessionModel
    {
        #region Constructors

        /// <summary>
        /// Construct SplitSessionModel.
        /// </summary>
        /// <param name="manager">Manager for this model.</param>
        public SplitSessionModel(AbstractSessionModelManager manager) 
                : base(manager)
        {}

        /// <summary>
        /// Construct SplitSessionModel.
        /// </summary>
        /// <param name="manager">Manager for this model.</param>
        /// <param name="binModel">Serialized model.</param>
        public SplitSessionModel(AbstractSessionModelManager manager, Binary binModel)
                : base(manager, binModel)
        {}

        #endregion

        #region Helper methods

        /// <summary>
        /// Return a map of external attributes.
        /// </summary>
        /// <returns>External attributes.</returns>
        public IDictionary GetExternalAttributes()
        {
            IDictionary extAttrs = new HashDictionary();
            foreach (ExternalAttributeHolder attr in BaseGetAllValues())
            {
                if (attr.External)
                {
                    extAttrs[attr.Name] = attr.BinaryValue;
                }
            }
            return extAttrs;
        }

        /// <summary>
        /// Return a map of external obsolete attributes.
        /// These atributes once were big so they were taken in external cache and now the size is smaller than 
        /// limit for going to external cache so they need to be evicted from the external cache
        /// </summary>
        /// <returns>Obsolete External attributes.</returns>
        public IList GetObsoleteExternalAttributes()
        {
            IList obsoletedExternalAttrs = new ArrayList();
            foreach (ExternalAttributeHolder attr in BaseGetAllValues())
            {
                if (attr.ExternalObsolete)
                {
                    obsoletedExternalAttrs.Add(attr.Name);
                }
            }
            return obsoletedExternalAttrs;
        }

        #endregion

        #region Inner class: ExternalAttributeHolder

        /// <summary>
        /// Instantiate attribute holder for existing session attribute.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="binValue">Serialized attribute value.</param>
        /// <param name="of">Attribute offset within collection buffer.</param>
        /// <returns>Attribute holder.</returns>
        protected override AttributeHolder InstantiateAttributeHolder(string name, Binary binValue, int of)
        {
            return new ExternalAttributeHolder(this, name, binValue, of);
        }

        /// <summary>
        /// Instantiate attribute holder for new session attribute.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <returns>Attribute holder.</returns>
        protected override AttributeHolder InstantiateAttributeHolder(string name)
        {
            return new ExternalAttributeHolder(this, name);
        }

        /// <summary>
        /// Attribute holder.
        /// </summary>
        public class ExternalAttributeHolder : AttributeHolder
        {
            #region Constructors

            /// <summary>
            /// Construct AttributeHolder instance from a buffer.
            /// </summary>
            /// <param name="parent">
            /// Parent collection.</param>
            /// <param name="name">
            /// Attribute name.</param>
            /// <param name="binValue">
            /// Serialized attribute value.</param>
            /// <param name="of">
            /// Attribute offset within collection buffer.</param>
            public ExternalAttributeHolder(AbstractSessionModel parent, String name, Binary binValue, int of)
                : base(parent, name, binValue, of)
            {}

            /// <summary>
            /// Construct AttributeHolder instance.
            /// </summary>
            /// <param name="parent">
            /// Parent collection.</param>
            /// <param name="name">
            /// Attribute name.</param>
            public ExternalAttributeHolder(AbstractSessionModel parent, String name)
                : this(parent, name, null)
            {}

            /// <summary>
            /// Construct AttributeHolder instance.
            /// </summary>
            /// <param name="parent">
            /// Parent collection.</param>
            /// <param name="name">
            /// Attribute name.</param>
            /// <param name="value">
            /// Attribute value.</param>
            public ExternalAttributeHolder(AbstractSessionModel parent, String name, Object value)
                : base(parent, name, value)
            {}

            #endregion

            #region Properties

            /// <summary>
            /// Get or set attribute value.
            /// </summary>
            /// <value>Attribute value.</value>
            public override Object Value
            {
                get
                {
                    Object value = m_value;
                    if (value == ObjectUtils.NO_VALUE)
                    {
                        Binary binValue = m_binValue;
                        if (binValue == null)
                        {
                            // another thread either marked the attribute as dirty
                            // or read the attribute from the split attribute cache;
                            // re-read the newly deserialized value
                            value = m_value;
                            if (value == ObjectUtils.NO_VALUE)
                            {
                                // attribute was removed from the split attribute cache
                                value = null;
                            }
                        }
                        else
                        {
                            if (Binary.NO_BINARY.Equals(binValue))
                            {
                                LoadExternalAttributeValue();
                                binValue      = m_binValue;
                                m_wasExternal = true;
                            }

                            m_value = value = binValue == null
                                    ? null // attribute was removed from the split attribute cache
                                    : Serializer.Deserialize(new DataReader(binValue.GetStream()));

                            if (!ObjectUtils.IsImmutable(value))
                            {
                                SetDirty();
                            }
                        }
                    }
                    return value;
                }
                set
                {
                    if (m_value == ObjectUtils.NO_VALUE && Binary.NO_BINARY.Equals(m_binValue))
                    {
                        m_wasExternal = true;
                    }
                    base.Value = value;
                }
            }

            /// <summary>
            /// True if this is external attribute.
            /// </summary>
            public bool External
            {
                get { return m_external; }
            }

            /// <summary>
            /// True if this atribute was external and became internal
            /// </summary>
            public bool ExternalObsolete
            {
                get { return m_wasExternal && !m_external; }
            }

            /// <summary>
            /// Model manager for the parent model.
            /// </summary>
            public virtual SplitSessionModelManager ModelManager
            {
                get { return (SplitSessionModelManager) m_parent.ModelManager; }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Load external attribute value.
            /// </summary>
            protected virtual void LoadExternalAttributeValue()
            {
                ExternalAttributeKey key = new ExternalAttributeKey(m_parent.SessionId, Name);
                m_binValue = (Binary) ModelManager.ExternalAttributeCache[key];
            }

            /// <summary>
            /// Serialize this attribute using specified writer.
            /// </summary>
            /// <param name="writer">Data writer to use.</param>
            public override void WriteExternal(DataWriter writer)
            {
                Binary binValue = BinaryValue;
                if (Dirty && binValue.Length >= ModelManager.MinExtAttributeSize)
                {
                    // mark as external attribute and write NO_BINARY instead
                    m_external = true;
                    Binary.NO_BINARY.WriteTo(writer);
                }
                else
                {
                    binValue.WriteTo(writer);
                }
            }

            #endregion

            #region Data members

            /// <summary>
            /// Flag specifying whether this is external attribute.
            /// </summary>
            protected bool m_external;

            /// <summary>
            /// Flag specifying whether this attribute was serialised as external attribute.
            /// </summary>
            protected bool m_wasExternal;

            #endregion
        }

        #endregion

        #region Inner class: ExternalAttributeKey

        /// <summary>
        /// External attribute key.
        /// </summary>
        public class ExternalAttributeKey 
            : IPortableObject, IKeyAssociation
        {
            #region Constructors

            /// <summary>
            /// Default constructor.
            /// </summary>
            public ExternalAttributeKey()
            {
            }

            /// <summary>
            /// Construct ExternalAttributeKey instance.
            /// </summary>
            /// <param name="sessionKey">
            /// The session key.
            /// </param>
            /// <param name="attributeName">
            /// The attribute name.
            /// </param>
            public ExternalAttributeKey(SessionKey sessionKey, String attributeName)
            {
                m_sessionKey    = sessionKey;
                m_attributeName = attributeName;
            }

            #endregion

            #region Implementation of IKeyAssociation

            /// <summary>
            /// Determine the key object to which this key object is associated.
            /// </summary>
            /// <remarks>
            /// The key object returned by this method is often referred to as a
            ///  <i>host key</i>.
            /// </remarks>
            /// <value>
            /// The host key that for this key object, or <c>null</c> if this key
            /// has no association.
            /// </value>
            public object AssociatedKey
            {
                get { return m_sessionKey; }
            }

            #endregion

            #region Implementation of IPortableObject

            /// <summary>
            /// Restore the contents of a user type instance by reading its state
            /// using the specified <see cref="IPofReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>IPofReader</b> from which to read the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public void ReadExternal(IPofReader reader)
            {
                m_sessionKey    = (SessionKey)reader.ReadObject(0);
                m_attributeName = reader.ReadString(1);
            }

            /// <summary>
            /// Save the contents of a POF user type instance by writing its
            /// state using the specified <see cref="IPofWriter"/> object.
            /// </summary>
            /// <param name="writer">
            /// The <b>IPofWriter</b> to which to write the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public void WriteExternal(IPofWriter writer)
            {
                writer.WriteObject(0, m_sessionKey);
                writer.WriteString(1, m_attributeName);
            }

            #endregion

            #region Object methods

            /// <summary>
            /// Test objects for equality.
            /// </summary>
            /// <param name="obj">Object to compare this object with.</param>
            /// <returns>
            /// True if this object and the specified object are equal, 
            /// false otherwise.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                ExternalAttributeKey that = obj as ExternalAttributeKey;
                if (obj == null)
                {
                    return false;
                }

                return Equals(that.m_sessionKey, m_sessionKey)
                       && Equals(that.m_attributeName, m_attributeName);
            }

            /// <summary>
            /// Return hash code for this object.
            /// </summary>
            /// <returns>This object's hash code.</returns>
            public override int GetHashCode()
            {
                return m_sessionKey.GetHashCode() ^ m_attributeName.GetHashCode();
            }

            /// <summary>
            /// Equality operator implementation.
            /// </summary>
            /// <param name="left">Left argument.</param>
            /// <param name="right">Right argument.</param>
            /// <returns>
            /// True if arguments are equal, false otherwise.
            /// </returns>
            public static bool operator ==(ExternalAttributeKey left, ExternalAttributeKey right)
            {
                return Equals(left, right);
            }

            /// <summary>
            /// Inequality operator implementation.
            /// </summary>
            /// <param name="left">Left argument.</param>
            /// <param name="right">Right argument.</param>
            /// <returns>
            /// True if arguments are not equal, false otherwise.
            /// </returns>
            public static bool operator !=(ExternalAttributeKey left, ExternalAttributeKey right)
            {
                return !Equals(left, right);
            }

            /// <summary>
            /// Return string representation of this object.
            /// </summary>
            /// <returns>
            /// String representation of this object.
            /// </returns>
            public override string ToString()
            {
                return m_sessionKey + ":" + m_attributeName;
            }

            #endregion

            #region Data members

            /// <summary>
            /// Session key.
            /// </summary>
            private SessionKey m_sessionKey;

            /// <summary>
            /// Attribute name.
            /// </summary>
            private String m_attributeName;

            #endregion
        }

        #endregion
    }
}