/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections.Generic;
using System.Reflection;

using Tangosol.IO.Pof.Annotation;

using IS = Tangosol.IO.Pof.Reflection.Internal.InvocationStrategies;

namespace Tangosol.IO.Pof.Reflection.Internal
{
    /// <summary>
    /// An AnnotationVisitor is a <see cref="IVisitor{B}"/> implementation 
    /// that injects information obtained by inspecting a provided type into
    /// a <see cref="TypeMetadataBuilder{T}"/>. The modified builder will 
    /// then realize a <see cref="ITypeMetadata{T}"/> instance with this 
    /// injected information.
    /// </summary>
    /// <remarks>
    /// This implementation is responsible purely for injecting explicitly 
    /// defined information in the form of annotations. It depends upon, and
    /// hence is aware of, only the following annotations:
    /// <list type="bullet">
    ///      <item><see cref="Portable"/></item> 
    ///      <item><see cref="PortableProperty"/></item> 
    /// </list>
    /// <p/>
    /// This class has three strategies of metadata discovery - property, 
    /// field and accessor - inspected respectively. Duplication is deemed by
    /// the same name derived by <see cref="INameMangler"/> implementations.
    /// </remarks>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <typeparam name="T">A <see cref="TypeMetadataBuilder{T}"/> type with
    ///  type <c>TB</c>.</typeparam>
    /// <typeparam name="TB">The Class type being visited.</typeparam>
    /// <since>Coherence 3.7.1</since>
    public class AnnotationVisitor<T, TB> : IVisitor<T>
        where TB : class, new()
        where T : TypeMetadataBuilder<TB>
    {
        #region Constructors

        /// <summary>
        /// Construct an AnnotationVisitor instance with the auto index 
        /// feature off.
        /// </summary>
        public AnnotationVisitor() 
            : this(false)
        {
        }

        /// <summary>
        /// Construct an AnnotationVisitor instance with auto indexing enable
        /// or disabled based on <c>autoIndex</c>.
        /// </summary>
        /// <param name="autoIndex">
        /// Whether to enable auto-indexing.
        /// </param>
        public AnnotationVisitor(bool autoIndex)
        {
            m_autoIndex = autoIndex;
        }
        #endregion

        #region IVisitor interface

        /// <summary>
        /// Visit the given builder <c>T</c> and optionally mutate it using
        /// information contained within the given Type.
        /// </summary>
        /// <param name="builder">
        /// The builder being visited.
        /// </param>
        /// <param name="type">
        /// The Type used to enrich the builder.
        /// </param>
        public virtual void Visit(T builder, Type type)
        {
            Portable portable = Attribute.GetCustomAttribute(type, typeof (Portable)) as Portable;

            // fast escape switch
            if (portable == null)
            {
                return;
            }

            // get header level information
            bool autoIndex = m_autoIndex;
            builder.SetType(type);

            // BindingFlags for member access
            const BindingFlags bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            // get property level information
            PropertyInfo[] props          = type.GetProperties(bindings);
            IList<string>  fieldsExcluded = new List<string>(props.Length);

            for (int i = 0; i < props.Length; ++i)
            {
                PropertyInfo     prop      = props[i];
                PortableProperty attribute = Attribute.GetCustomAttribute(prop, typeof(PortableProperty)) as PortableProperty;
                if (attribute == null)
                {
                    continue;
                }

                string propName  = prop.Name;
                string mangled   = NameManglers.PROPERTY_MANGLER.Mangle(propName);
                ICodec codec     = Codecs.GetCodec(attribute.Codec);

                if (!autoIndex && attribute.Index < 0)
                {
                    throw new ArgumentException("A POF Index must be specified for the property "
                         + type.Name + "#" + propName + " by specifying "
                         + "within the annotation or enabling auto indexing");
                }

                builder.AddAttribute(
                    builder.NewAttribute()
                        .SetName(mangled)
                        .SetCodec(codec)
                        .SetInvocationStrategy(new IS.PropertyInvcationStrategy<TB>(prop))
                        .SetIndex(attribute.Index).Build());
                
                // field level annotations take precedence over accessor annotations
                fieldsExcluded.Add(mangled);
            }

            // get field level information
            FieldInfo[] fields = type.GetFields(bindings);

            for (int i = 0; i < fields.Length; ++i)
            {
                FieldInfo        field     = fields[i];
                PortableProperty attribute = Attribute.GetCustomAttribute(field, typeof(PortableProperty)) as PortableProperty;
                if (attribute == null)
                {
                    continue;
                }

                string fieldName = field.Name;
                string mangled   = NameManglers.FIELD_MANGLER.Mangle(fieldName);
                ICodec codec     = Codecs.GetCodec(attribute.Codec);

                if (!autoIndex && attribute.Index < 0)
                {
                    throw new ArgumentException("A POF Index must be specified for the property "
                        + type.Name + "#" + fieldName + " by specifying "
                        + "within the annotation or enabling auto indexing");
                }

                builder.AddAttribute(
                    builder.NewAttribute()
                        .SetName(mangled)
                        .SetCodec(codec)
                        .SetInvocationStrategy(new IS.FieldInvcationStrategy<TB>(field))
                        .SetIndex(attribute.Index).Build());
                
                // field level annotations take precedence over accessor annotations
                fieldsExcluded.Add(mangled);
            }

            // get method level information
            MethodInfo[] methods = type.GetMethods(bindings);
            for (int i = 0; i < methods.Length; ++i)
            {
                MethodInfo       method    = methods[i];
                PortableProperty attribute = Attribute.GetCustomAttribute(method, typeof(PortableProperty)) as PortableProperty;
                if (attribute == null)
                {
                    continue;
                }

                string methodName = method.Name;
                if (methodName.StartsWith("Get") || methodName.StartsWith("Set")
                    || methodName.StartsWith("Is"))
                {
                    string mangled = NameManglers.METHOD_MANGLER.Mangle(methodName);
                    if (fieldsExcluded.Contains(mangled))
                    {
                        continue;
                    }

                    ICodec codec = Codecs.GetCodec(attribute.Codec);
                    if (!autoIndex && attribute.Index < 0)
                    {
                        throw new ArgumentException("A POF Index must be specified for the method "
                            + type.Name + "#" + methodName + " by specifying "
                            + "within the annotation or enabling auto indexing");
                    }

                    builder.AddAttribute(
                        builder.NewAttribute()
                            .SetName(mangled)
                            .SetCodec(codec)
                            .SetInvocationStrategy(new IS.MethodInvocationStrategy<TB>(method))
                            .SetIndex(attribute.Index).Build());

                    // in the case where both accessors (getters and setters) are
                    // annotated we only use the values in the first annotation we
                    // come stumble across
                    fieldsExcluded.Add(mangled);
                }
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// Whether to use the auto-indexing feature to derive indexes.
        /// </summary>
        private readonly bool m_autoIndex;

        #endregion
    }
}
