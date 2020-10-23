/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

using Tangosol.IO.Pof.Annotation;
using Tangosol.IO.Pof.Reflection;
using Tangosol.IO.Pof.Reflection.Internal;
using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// A PofAnnotationSerializer provides annotation based
    /// de/serialization. 
    /// </summary>
    /// <remarks>
    /// This serializer must be instantiated with the intended
    /// class which is eventually scanned for the presence of the following
    /// annotations.
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     <see cref="Portable"/>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     <see cref="PortableProperty"/>
    ///     </description>
    ///   </item>
    /// </list>
    /// This serializer supports classes iff they are annotated with the type level
    /// annotation; <see cref="Portable"/>. This annotation is a marker annotation with
    /// no children.
    /// <p/>
    /// All fields annotated with <see cref="PortableProperty"/> are explicitly
    /// deemed POF serializable with the option of specifying overrides to
    /// provide explicit behaviour such as:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     Explicit POF indexes
    ///   </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     Custom <see cref="ICodec"/> to specify concrete implementations
    ///     / customizations
    ///     </description>
    ///   </item>
    /// </list>
    /// <p/>
    /// The <see cref="PortableProperty.Index"/> (POF index) can be omitted 
    /// iff the auto-indexing feature is enabled. This is enabled by 
    /// instantiating this class with the <c>autoIndex</c> constructor 
    /// argument. This feature determines the index based on any explicit 
    /// indexes specified and the name of the portable properties. Currently 
    /// objects with multiple versions is not supported. The following 
    /// illustrates the auto index algorithm:
    /// <table border="1">
    ///   <tr><td>Name</td><td>Explicit Index</td><td>Determined Index</td></tr>
    ///   <tr><td>c</td><td>1</td><td>1</td></tr>
    ///   <tr><td>a</td><td></td><td>0</td></tr>
    ///   <tr><td>b</td><td></td><td>2</td></tr>
    /// </table>
    /// <b>NOTE:</b> This implementation does support objects that implement
    /// Evolvable
    /// </remarks>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <since>Coherence 3.7.1</since>
    public class PofAnnotationSerializer : IPofSerializer
    {
        // internal notes
        //   - PofAnnotationSerializer can and should support generics however due
        //     to our inability to support the instantiation of generic types 
        //     the generics that were once there have been removed until we build
        //     a TypeResolver that supports generics.
        #region Constructors

        /// <summary>
        /// Constructs a PofAnnotationSerializer.
        /// </summary>
        /// <param name="typeId">
        /// The POF type id.
        /// </param>
        /// <param name="type">
        /// Type this serializer is aware of.
        /// </param>
        public PofAnnotationSerializer(int typeId, Type type)    
            : this(typeId, type, false)
        {
        }

        /// <summary>
        /// Constructs a PofAnnotationSerializer.
        /// </summary>
        /// <param name="typeId">
        /// The POF type id.
        /// </param>
        /// <param name="type">
        /// Type this serializer is aware of.
        /// </param>
        /// <param name="autoIndex">
        /// Turns on the auto index feature.
        /// </param>
        public PofAnnotationSerializer(int typeId, Type type, bool autoIndex)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            // fail-fast
            Initialize(typeId, type, autoIndex);
        }

        #endregion

        #region IPofSerializer interface

        /// <summary>
        /// Serialize a user type instance to a POF stream by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <remarks>
        /// An implementation of <b>IPofSerializer</b> is required to follow
        /// the following steps in sequence for writing out an object of a
        /// user type:
        /// <list type="number">
        /// <item>
        /// <description>
        /// If the object is evolvable, the implementation must set the
        /// version by calling <see cref="IPofWriter.VersionId"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The implementation may write any combination of the properties of
        /// the user type by using the "write" methods of the
        /// <b>IPofWriter</b>, but it must do so in the order of the property
        /// indexes.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// After all desired properties of the user type have been written,
        /// the implementation must terminate the writing of the user type by
        /// calling <see cref="IPofWriter.WriteRemainder"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="writer">
        /// The <b>IPofWriter</b> with which to write the object's state.
        /// </param>
        /// <param name="o">
        /// The object to serialize.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void Serialize(IPofWriter writer, object o)
        {
            // set the version identifier
            bool       isEvolvable = o is IEvolvable;
            IEvolvable evolvable   = null;
            if (isEvolvable)
            {
                evolvable = (IEvolvable)o;
                writer.VersionId =
                        Math.Max(evolvable.DataVersion, evolvable.ImplVersion);
            }

            // POF Annotation processing
            for (IEnumerator enmr = m_tmd.GetAttributes(); enmr.MoveNext(); )
            {
                IAttributeMetadata<object> attr = (IAttributeMetadata<object>)enmr.Current;
                attr.Codec.Encode(writer, attr.Index, attr.Get(o)); 
            }

            // write out any future properties
            Binary remainder = null;
            if (isEvolvable)
            {
                remainder = evolvable.FutureData;
            }
            writer.WriteRemainder(remainder);
        }

        /// <summary>
        /// Deserialize a user type instance from a POF stream by reading its
        /// state using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <remarks>
        /// An implementation of <b>IPofSerializer</b> is required to follow
        /// the following steps in sequence for reading in an object of a
        /// user type:
        /// <list type="number">
        /// <item>
        /// <description>
        /// If the object is evolvable, the implementation must get the
        /// version by calling <see cref="IPofWriter.VersionId"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The implementation may read any combination of the
        /// properties of the user type by using "read" methods of the
        /// <b>IPofReader</b>, but it must do so in the order of the property
        /// indexes.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// After all desired properties of the user type have been read,
        /// the implementation must terminate the reading of the user type by
        /// calling <see cref="IPofReader.ReadRemainder"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="reader">
        /// The <b>IPofReader</b> with which to read the object's state.
        /// </param>
        /// <returns>
        /// The deserialized user type instance.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual object Deserialize(IPofReader reader)
        {
            ITypeMetadata<object> tmd = m_tmd;
            object value = tmd.NewInstance();

            // set the version identifier
            bool       isEvolvable = value is IEvolvable;
            IEvolvable evolvable   = null;
            if (isEvolvable)
            {
                evolvable = (IEvolvable)value;
                evolvable.DataVersion = reader.VersionId;
            }

            // POF Annotation processing
            for (IEnumerator enmr = tmd.GetAttributes(); enmr.MoveNext(); )
            {
                IAttributeMetadata<object> attr = (IAttributeMetadata<object>)enmr.Current;
                attr.Set(value, attr.Codec.Decode(reader, attr.Index));
            }

            // read any future properties
            Binary remainder = reader.ReadRemainder();
            if (isEvolvable)
            {
                evolvable.FutureData = remainder;
            }

            return value;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Initialize this serializer with <see cref="ITypeMetadata{PT}"/> pertaining to the
        /// specified class.
        /// </summary>
        /// <param name="typeId">
        /// POF type id that uniquely identifies this type.
        /// </param>
        /// <param name="type">
        /// Type this serializer is aware of.
        /// </param>
        /// <param name="autoIndex">
        /// Turns on the auto index feature.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If annotation is not present on <c>type</c>.
        /// </exception>
        private void Initialize(int typeId, Type type, bool autoIndex)
        {
            Portable portable = Attribute.GetCustomAttribute(type, typeof(Portable)) as Portable;
            if (portable == null)
            {
                throw new ArgumentException(string.Format(
                    "Attempting to use {0} for a class ({1}) that has no {2} annotation",
                    GetType().Name,
                    type.Name,
                    typeof(Portable).Name));
            }

            // via the builder create the type metadata
            TypeMetadataBuilder<object> builder = new TypeMetadataBuilder<object>()
                .SetTypeId(typeId);
            builder.Accept(new AnnotationVisitor<TypeMetadataBuilder<object>, object>(autoIndex), type);
            m_tmd = builder.Build();
        }

        #endregion

        #region Data members

        /**
         * ITypeMetadata representing type information for this serializer instance.
         */
        private ITypeMetadata<object> m_tmd;
        
        #endregion
    }
}
