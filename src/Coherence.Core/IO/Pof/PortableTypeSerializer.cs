/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections.Generic;
using System.IO;

using Tangosol.Net;
using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// A <see cref="IPofSerializer"/> implementation that serializes classes 
    /// that implement <see cref="IPortableObject"/> interface (and optionally 
    /// <see cref="IEvolvableObject"/> interface).
    /// </summary>
    /// <remarks>
    /// Unlike legacy <see cref="PortableObjectSerializer"/>, this class serializes attributes
    /// of each class in the object's hierarchy into a separate nested POF stream,
    /// which allows for independent evolution of each class in the hierarchy, as well
    /// as the evolution of the hierarchy itself (addition of new classes at any level
    /// in the hierarchy).
    /// </remarks>
    /// <author>Aleksandar Seovic  2013.11.04</author>
    /// <since>Coherence 12.2.1</since>
    public class PortableTypeSerializer : IPofSerializer
    {
        #region Constructors

        /// <summary>
        /// Create a new PortableTypeSerializer for the user type with the given type
        /// identifier and class.
        /// </summary>
        /// <param name="nTypeId">
        /// The type identifier of the user type to serialize and deserialize.
        /// </param>
        /// <param name="type">
        /// The type of the user type to serialize and deserialize
        /// </param>
        public PortableTypeSerializer(int nTypeId, Type type)
        {
            if (!typeof (IPortableObject).IsAssignableFrom(type))
            {
                CacheFactory.Log("Class [" + type +
                                 "] does not implement a IPortableObject interface",
                                 CacheFactory.LogLevel.Error);
            }
            m_nTypeId = nTypeId;
        }

        #endregion

        #region IPofSerializer implementation

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
        /// <param name="pofWriter">
        /// The <b>IPofWriter</b> with which to write the object's state.
        /// </param>
        /// <param name="o">
        /// The object to serialize.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public void Serialize(IPofWriter pofWriter, Object o)
        {
            if (!(o is IPortableObject))
            {
                throw new IOException("Class [" + o.GetType() +
                                      "] does not implement a IPortableObject interface");
            }

            IPortableObject po = (IPortableObject) o;
            bool fEvolvable = o is IEvolvableObject;
            IEvolvableObject et = fEvolvable ? (IEvolvableObject) o : null;

            try
            {
                CacheFactory.Log("Serializing " + o.GetType(), CacheFactory.LogLevel.Max);

                IPofContext ctx = pofWriter.PofContext;
                IEnumerable<int> typeIds = GetTypeIds(o, ctx);

                foreach (int typeId in typeIds)
                {
                    IEvolvable e = null;
                    if (fEvolvable)
                    {
                        e = et.GetEvolvable(typeId);
                    }

                    IPofWriter writer = pofWriter.CreateNestedPofWriter(typeId, typeId);
                    if (fEvolvable)
                    {
                        writer.VersionId = Math.Max(e.DataVersion, e.ImplVersion);
                    }

                    Type type = GetTypeForTypeId(ctx, typeId);
                    if (type != null)
                    {
                        po.WriteExternal(writer);
                    }

                    writer.WriteRemainder(fEvolvable ? e.FutureData : null);
                }

                pofWriter.WriteRemainder(null);
            }
            catch (Exception e)
            {
                String sClass = null;
                try
                {
                    sClass = pofWriter.PofContext.GetTypeName(m_nTypeId);
                }
                catch (Exception)
                {
                }

                String sActual = null;
                try
                {
                    sActual = o.GetType().FullName;
                }
                catch (Exception)
                {
                }

                throw new IOException(
                    "An exception occurred writing a IPortableObject"
                    + " user type to a POF stream: type-id=" + m_nTypeId
                    + (sClass == null ? "" : ", class-name=" + sClass)
                    + (sActual == null ? "" : ", actual class-name=" + sActual)
                    + ", exception=" + e, e);
            }
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
        /// version by calling <see cref="IPofReader.VersionId"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The implementation may read any combination of the
        /// properties of the user type by using "read" methods of the
        /// <b>IPofReader</b>, but it must do so in the order of the property
        /// indexes. Additionally, the implementation must call 
        /// {@link IPofReader#RegisterIdentity} with the new instance prior
        /// to reading any properties which are user type instances
        /// themselves.
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
        /// <param name="pofReader">
        /// The <b>IPofReader</b> with which to read the object's state.
        /// </param>
        /// <returns>
        /// The deserialized user type instance.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public Object Deserialize(IPofReader pofReader)
        {
            try
            {
                IPortableObject po = (IPortableObject)
                    Activator.CreateInstance(GetTypeForTypeId(pofReader.PofContext, m_nTypeId));

                CacheFactory.Log("Deserializing " + po.GetType(), CacheFactory.LogLevel.Max);

                bool fEvolvable = po is IEvolvableObject;
                IEvolvableObject et = fEvolvable ? (IEvolvableObject) po : null;

                int typeId = ((PofStreamReader.UserTypeReader) pofReader).NextPropertyIndex;
                while (typeId > 0)
                {
                    IEvolvable e = null;
                    IPofReader reader = pofReader.CreateNestedPofReader(typeId);

                    if (fEvolvable)
                    {
                        e = et.GetEvolvable(typeId);
                        e.DataVersion = reader.VersionId;
                    }

                    po.ReadExternal(reader);

                    Binary binRemainder = reader.ReadRemainder();
                    if (fEvolvable)
                    {
                        e.FutureData = binRemainder;
                    }
                    typeId = ((PofStreamReader.UserTypeReader) pofReader).NextPropertyIndex;
                }

                pofReader.ReadRemainder();
                return po;
            }
            catch (Exception e)
            {
                String sClass = null;
                try
                {
                    sClass = pofReader.PofContext.GetTypeName(m_nTypeId);
                }
                catch (Exception)
                {
                }

                throw new IOException(
                    "An exception occurred instantiating a IPortableObject"
                    + " user type from a POF stream: type-id=" + m_nTypeId
                    + (sClass == null ? "" : ", class-name=" + sClass)
                    + ", exception=\n" + e, e);
            }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Return a sorted set of type identifiers for all user types 
        /// in a class hierarchy.
        /// </summary>
        /// <param name="o">The object to return type identifiers for</param>
        /// <param name="ctx">The POF context</param>
        /// <returns>
        /// A sorted enumeration of type identifiers for all user types 
        /// in a class hierarchy.
        /// </returns>
        private IEnumerable<int> GetTypeIds(Object o, IPofContext ctx)
        {
            List<int> typeIds = new List<int>();

            Type type = o.GetType();
            while (type != null && ctx.IsUserType(type))
            {
                typeIds.Add(ctx.GetUserTypeIdentifier(type));
                type = type.BaseType;
            }

            if (o is IEvolvableObject)
            {
                EvolvableHolder evolvableHolder = ((IEvolvableObject) o).GetEvolvableHolder();
                if (!evolvableHolder.IsEmpty)
                {
                    foreach (int typeId in evolvableHolder.TypeIds)
                    {
                        typeIds.Add(typeId);
                    }
                }
            }

            typeIds.Sort();
            return typeIds;
        }

        /// <summary>
        /// Return the class associated with a specified type identifier, or null
        /// if the identifier is not defined in the current POF context.
        /// </summary>
        /// <param name="ctx">The POF context</param>
        /// <param name="nTypeId">The type identifier to lookup</param>
        /// <returns>
        /// </returns>
        private Type GetTypeForTypeId(IPofContext ctx, int nTypeId)

        {
            try
            {
                return ctx.GetType(nTypeId);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier of the user type to serialize and deserialize.
        /// </summary>
        protected readonly int m_nTypeId;

        #endregion
    }
}