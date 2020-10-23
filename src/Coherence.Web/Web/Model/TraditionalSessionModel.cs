/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.SessionState;

using Tangosol.IO;
using Tangosol.Util;

namespace Tangosol.Web.Model
{
    /// <summary>
    /// Implementation of a <see cref="ISessionModel"/>
    /// that deserializes individual items on demand.
    /// </summary>
    /// <author>Aleksandar Seovic  2008.10.18</author>
    public class TraditionalSessionModel 
        : AbstractSessionModel, ISessionStateItemCollection
    {
        #region Constructors

        /// <summary>
        /// Construct TraditionalSessionModel.
        /// </summary>
        /// <param name="manager">Manager for this model.</param>
        public TraditionalSessionModel(AbstractSessionModelManager manager) 
                : this(manager, null)
        {}

        /// <summary>
        /// Construct TraditionalSessionModel instance.
        /// </summary>
        /// <param name="manager">Manager for this model.</param>
        /// <param name="binItems">Serialized items.</param>
        public TraditionalSessionModel(AbstractSessionModelManager manager, Binary binItems)
                : base(manager)
        {
            m_binItems = binItems;
        }

        #endregion

        #region Implementation of ISessionStateItemCollection

        /// <summary>
        /// Gets or sets a value in the collection by name.
        /// </summary>
        /// <param name="name">
        /// The key name of the value in the collection.
        /// </param>
        /// <returns>
        /// The value in the collection with the specified name.
        /// </returns>
        object ISessionStateItemCollection.this[string name]
        {
            get
            {
                AcquireReadLock();
                try
                {
                    AttributeHolder attr = (AttributeHolder) BaseGet(name);
                    return attr == null ? null : attr.Value;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                AcquireWriteLock();
                try
                {
                    AttributeHolder attr = (AttributeHolder) BaseGet(name);
                    if (attr == null)
                    {
                        attr = AddAttribute(name);
                    }
                    attr.Value = value;
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value in the collection by numerical index.
        /// </summary>
        /// <param name="index">
        /// The numerical index of the value in the collection.
        /// </param>
        /// <returns>
        /// The value in the collection stored at the specified index.
        /// </returns>
        object ISessionStateItemCollection.this[int index]
        {
            get
            {
                AcquireReadLock();
                try
                {
                    AttributeHolder attr = (AttributeHolder) BaseGet(index);
                    return attr.Value;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                AcquireWriteLock();
                try
                {
                    AttributeHolder attr = (AttributeHolder) BaseGet(index);
                    attr.Value = value;
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Deserializes item collection using specified reader.
        /// </summary>
        /// <param name="reader">Reader to use.</param>
        public override void ReadExternal(DataReader reader)
        {
            Stream   buf   = reader.BaseStream;
            int      count = reader.ReadPackedInt32();
            string[] names = new string[count];
            for (int i = 0; i < count; i++)
            {
                names[i] = reader.ReadString();
            }

            int[] offsets = new int[count];
            for (int i = 0; i < count; i++)
            {
                offsets[i] = reader.ReadInt32();
            }

            for (int i = 0; i < count; i++)
            {
                int cb = (i == count - 1)
                             ? (int) buf.Length - offsets[i]
                             : offsets[i + 1] - offsets[i];
                InitializeAttribute(names[i], offsets[i], cb);
            }
        }

        /// <summary>
        /// Serializes item collection using specified writer.
        /// </summary>
        /// <param name="writer">Writer to use.</param>
        public override void WriteExternal(DataWriter writer)
        {
            Stream buf = writer.BaseStream;
            
            int count = Count;
            writer.WritePackedInt32(count);

            String[] keys   = BaseGetAllKeys();
            Object[] values = BaseGetAllValues();
            foreach (String key in keys)
            {
                writer.Write(key);
            }

            IList<int> offsets = new List<int>(count);
            int        ofPos   = (int) buf.Position;
            
            buf.Seek(4 * count, SeekOrigin.Current);
            foreach (AttributeHolder attr in values)
            {
                offsets.Add((int) buf.Position);
                attr.WriteExternal(writer);
            }

            buf.Position = ofPos;
            foreach (int offset in offsets)
            {
                writer.Write(offset);
            }
        }

        /// <summary>
        /// Initialize session attribute. 
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="of">Value offset within serialized items binary.</param>
        /// <param name="cb">The length of the serialized value in bytes.</param>
        protected virtual void InitializeAttribute(string name, int of, int cb)
        {
            AddAttribute(InstantiateAttributeHolder(name, m_binItems.GetBinary(of, cb), of));
        }

        /// <summary>
        /// Add new attribute to underlying name-value collection.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <returns>A holder for the added attribute.</returns>
        protected virtual AttributeHolder AddAttribute(String name)
        {
            return AddAttribute(InstantiateAttributeHolder(name));
        }

        /// <summary>
        /// Add existing attribute to underlying name-value collection.
        /// </summary>
        /// <param name="attr">Attribute holder to add.</param>
        /// <returns>Added attribute holder.</returns>
        protected virtual AttributeHolder AddAttribute(AttributeHolder attr)
        {
            BaseAdd(attr.Name, attr);
            return attr;
        }

        #endregion

        #region Inner class: AttributeHolder

        /// <summary>
        /// Instantiate attribute holder for existing session attribute.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="binValue">Serialized attribute value.</param>
        /// <param name="of">Attribute offset within collection buffer.</param>
        /// <returns>Attribute holder.</returns>
        protected virtual AttributeHolder InstantiateAttributeHolder(string name, Binary binValue, int of)
        {
            return new AttributeHolder(this, name, binValue, of);
        }

        /// <summary>
        /// Instantiate attribute holder for new session attribute.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <returns>Attribute holder.</returns>
        protected virtual AttributeHolder InstantiateAttributeHolder(string name)
        {
            return new AttributeHolder(this, name);
        }

        /// <summary>
        /// Attribute holder.
        /// </summary>
        public class AttributeHolder
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
            public AttributeHolder(AbstractSessionModel parent, String name, Binary binValue, int of)
            {
                if (binValue == null)
                {
                    throw new ArgumentNullException("binValue");
                }
                m_parent   = parent;
                m_name     = name;
                m_binValue = binValue;
                m_of       = of;
                m_value    = ObjectUtils.NO_VALUE;
            }

            /// <summary>
            /// Construct AttributeHolder instance.
            /// </summary>
            /// <param name="parent">
            /// Parent collection.</param>
            /// <param name="name">
            /// Attribute name.</param>
            public AttributeHolder(AbstractSessionModel parent, String name)
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
            public AttributeHolder(AbstractSessionModel parent, String name, Object value)
            {
                if (value == ObjectUtils.NO_VALUE)
                {
                    throw new ArgumentException("value");
                }
                m_parent = parent;
                m_name   = name;
                m_value  = value;
                m_dirty  = true;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Get attribute name.
            /// </summary>
            /// <value>Attribute name.</value>
            public virtual String Name
            {
                get { return m_name; }
            }

            /// <summary>
            /// Get or set attribute value.
            /// </summary>
            /// <value>Attribute value.</value>
            public virtual Object Value
            {
                get
                {
                    Object value = m_value;
                    if (value == ObjectUtils.NO_VALUE)
                    {
                        Binary binValue = m_binValue;
                        if (binValue == null)
                        {
                            // another thread just marked the attribute as dirty;
                            // re-read the newly deserialized value
                            value = m_value;
                        }
                        else
                        {
                            m_value = value = Serializer.Deserialize(new DataReader(binValue.GetStream()));
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
                    if (!Equals(m_value, value))
                    {
                        m_value = value;
                        SetDirty();
                    }
                }
            }

            /// <summary>
            /// Get binary value.
            /// </summary>
            public Binary BinaryValue
            {
                get
                {
                    Binary binValue = m_binValue;
                    if (binValue == null)
                    {
                        var stream = new BinaryMemoryStream();
                        Serializer.Serialize(new DataWriter(stream), m_value);
                        m_binValue = binValue = stream.ToBinary();
                    }
                    return binValue;
                }
            }

            /// <summary>
            /// Get the flag specifying whether this attribute is dirty.
            /// </summary>
            /// <value>
            /// true if this attribute is dirty, false otherwise.
            /// </value>
            protected virtual bool Dirty
            {
                get { return m_dirty; }
            }

            /// <summary>
            /// Attribute serializer.
            /// </summary>
            protected ISerializer Serializer
            {
                get { return m_parent.Serializer; }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Mark this attribute as dirty.
            /// </summary>
            protected void SetDirty()
            {
                m_parent.Dirty = m_dirty = true;
                
                // since the attribute is dirty, there is no need
                // to hold onto its serialized representation anymore
                m_binValue = null;
            }

            /// <summary>
            /// Serialize this attribute using specified writer.
            /// </summary>
            /// <param name="writer">Data writer to use.</param>
            public virtual void WriteExternal(DataWriter writer)
            {
                if (Dirty)
                {
                    Serializer.Serialize(writer, m_value);
                }
                else
                {
                    m_binValue.WriteTo(writer);
                }
            }

            #endregion

            #region Data members

            /// <summary>
            /// Parent model.
            /// </summary>
            protected readonly AbstractSessionModel m_parent;

            /// <summary>
            /// Attribute name.
            /// </summary>
            protected readonly String m_name;

            /// <summary>
            /// Offset of this attribute's value within serialized items buffer.
            /// </summary>
            protected readonly int m_of;

            /// <summary>
            /// Attribute value.
            /// </summary>
            protected volatile Object m_value;

            /// <summary>
            /// Serialized (binary) attribute value.
            /// </summary>
            protected volatile Binary m_binValue;

            /// <summary>
            /// Dirty flag.
            /// </summary>
            protected volatile bool m_dirty;

            #endregion
        }

        #endregion

        #region Data members

        /// <summary>
        /// Serialized items.
        /// </summary>
        private readonly Binary m_binItems;

        #endregion
    }
}