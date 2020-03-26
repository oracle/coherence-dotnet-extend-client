/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.IO;
using System.Runtime.Serialization;

using Tangosol.IO.Pof;
using Tangosol.IO.Pof.Reflection;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// POF-based <see cref="IValueExtractor"/> implementation.
    /// PofExtractor takes advantage of POF's indexed state to extract part of 
    /// an object without needing to deserialize the entire object.
    /// </summary>
    /// <remarks>
    /// POF uses a compact form in the serialized value when possible. For
    /// example, some numeric values are represented as special POF intrinsic
    /// types in which the type implies the value. As a result, POF requires
    /// the receiver of a value to have implicit knowledge of the type.
    /// PofExtractor uses the type supplied in the constructor as the source
    /// of the type information. If the type is <c>null</c>, PofExtractor
    /// will infer the type from the serialized state.
    /// Example where extracted value is Double:
    /// <code>
    ///     IValueExtractor extractor = new PofExtractor(typeof(Double), 2);
    /// </code>
    /// Example where extracted value should be inferred:
    /// <code>
    ///     IValueExtractor extractor = new PofExtractor(null, 2);
    /// </code>
    /// </remarks>
    /// <author>Aleksandar Seovic  2009.02.14</author>
    /// <author>Ivan Cikic  2009.04.01</author>
    /// <since>Coherence 3.5</since>
    [Serializable]
    public class PofExtractor : AbstractExtractor, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Obtain the IPofNavigator for this extractor.
        /// </summary>
        public IPofNavigator Navigator
        {
            get { return m_navigator; }
        }

        /// <summary>
        /// Obtain the type of the extracted value.
        /// </summary>
        /// <value>
        /// The expected type.
        /// </value>
        public Type TypeExtracted
        {
            get { return m_type; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PofExtractor()
        {
            m_typeId = PofConstants.T_UNKNOWN;
        }

        /// <summary>
        /// Constructs a <b>PofExtractor</b> based on a property index.
        /// </summary>
        /// <remarks>
        /// This constructor is equivalent to:
        /// <code>
        /// PofExtractor extractor = 
        ///     new PofExtractor(type, new SimplePofPath(index), VALUE);
        /// </code>
        /// </remarks>
        /// <param name="type">
        /// The required type of the extracted value or <c>null</c> if the
        /// type is to be inferred from the serialized state.
        /// </param>
        /// <param name="index">
        /// Property index.
        /// </param>
        public PofExtractor(Type type, int index) 
            : this(type, new SimplePofPath(index), VALUE)
        {}

        /// <summary>
        /// Constructs a <b>PofExtractor</b> based on a POF navigator.
        /// </summary>
        /// <param name="type">
        /// The required type of the extracted value or <c>null</c> if the
        /// type is to be inferred from the serialized state.
        /// </param>
        /// <param name="navigator">
        /// POF navigator.
        /// </param>
        public PofExtractor(Type type, IPofNavigator navigator) 
            : this(type, navigator, VALUE)
        {}

        /// <summary>
        /// Constructs a <b>PofExtractor</b> based on a POF navigator 
        /// and the entry extraction target.
        /// </summary>
        /// <param name="type">
        /// The required type of the extracted value or <c>null</c> if the
        /// type is to be inferred from the serialized state.
        /// </param>
        /// <param name="navigator">
        /// POF navigator.
        /// </param>
        /// <param name="target">
        /// One of the <see cref="AbstractExtractor.VALUE"/>
        /// or <see cref="AbstractExtractor.KEY"/> values.
        /// </param>
        public PofExtractor(Type type, IPofNavigator navigator, int target)
        {
            if (navigator == null)
            {
                throw new ArgumentNullException("navigator");
            }

            m_type      = type;
            m_navigator = navigator;
            m_target    = target;
            if (type == null)
            {
                m_typeId = PofConstants.T_UNKNOWN;
            }
        }

        #endregion

        #region AbstractExtractor methods

        /// <summary>
        /// Extracts the value from the passed ICacheEntry object.
        /// </summary>
        /// <remarks>
        /// This method will always throw a <see cref="NotSupportedException"/>
        /// if called directly by the .NET client application, as its execution
        /// is only meaningful within the cluster.
        /// <p/>
        /// It is expected that this extractor will only be used against 
        /// POF-encoded binary entries within a remote partitioned cache.
        /// </remarks>
        /// <param name="entry">
        /// An Entry object to extract a value from
        /// </param>
        /// <returns>
        /// The extracted value
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Always, as it is expected that this extractor will only be 
        /// executed within the cluster.
        /// </exception>
        public override Object ExtractFromEntry(ICacheEntry entry)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Object methods

        /// <summary>
        /// Compare the PofExtractor with another object to determine
        /// equality.
        /// </summary>
        /// <remarks>
        /// Two PofExtractor objects are considered equal iff their paths
        /// are equal and they have the same target (key or value).
        /// </remarks>
        /// <param name="o">
        /// Object to compare with
        /// </param>
        /// <returns>
        /// <b>true</b> iff this PofExtractor and the passed object are
        /// equivalent
        /// </returns>
        public override bool Equals(Object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o is PofExtractor)
            {
                PofExtractor that = (PofExtractor) o;
                return m_target == that.m_target
                    && Equals(m_navigator, that.m_navigator);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the PofExtractor object.
        /// </summary>
        /// <returns>
        /// An integer hash value for this PofExtractor object
        /// </returns>
        public override int GetHashCode()
        {
            return m_navigator.GetHashCode() + m_target;
        }

        /// <summary>
        /// Return a human-readable description for this PofExtractor.
        /// </summary>
        /// <returns>
        /// String description of the PofExtractor
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + 
                "(target=" + (m_target == 0 ? "VALUE" : "KEY") +
                ", navigator=" + m_navigator + ')';
        }

        #endregion

        #region IPortableObject implementation

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
            m_target    = reader.ReadInt32(0);
            m_navigator = (IPofNavigator) reader.ReadObject(1);

            // Note: we write out the TypeId offset by T_UNKNOWN to allow for pre
            // 3.5.2 backwards compatibility in the reader, i.e. the lack of this
            // property in the stream will result in T_UNKNOWN, and the old behavior
            // Note 2: this offset fix unfortunately could cause us to push the
            // written TypeId out of the legal int range.  To fix this we write
            // it as a long on the wire.
            m_typeId = (int) (reader.ReadInt64(2) + PofConstants.T_UNKNOWN);
            m_type   = null;
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
            IPofNavigator navigator = m_navigator;
            if (navigator == null)
                {
                throw new SerializationException(
                        "PofExtractor was constructed without a navigator");
                }
            writer.WriteInt32(0, m_target);
            writer.WriteObject(1, navigator);

            // see note in readExternal regarding T_UNKNOWN offset
            writer.WriteInt64(2, (long) GetPofTypeId(writer.PofContext) -
                (long) PofConstants.T_UNKNOWN);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Compute the expected pof type id based on the type.
        /// </summary>
        /// <param name="ctx">
        /// Pof context.
        /// </param>
        /// <returns>
        /// Pof type id or <see cref="PofConstants.T_UNKNOWN"/> if the type
        /// is <c>null</c>.
        /// </returns>
        protected int GetPofTypeId(IPofContext ctx)
        {
            Type type = m_type;

            return m_type == null
                    ? m_typeId
                    : PofHelper.GetPofTypeId(type, ctx);
        }

        #endregion

        #region Data members

        /// <summary>
        /// POF navigator.
        /// </summary>
        private IPofNavigator m_navigator;

        /// <summary>
        /// Type for what is being extracted; or null if this information is
        /// specified in m_typeId.
        /// </summary>
        [NonSerialized]
        private Type m_type;

        /// <summary>
        /// POF type for expected value.
        /// This value is only meaninful when m_type == null.
        /// </summary>
        private int m_typeId;

        #endregion
    }
}