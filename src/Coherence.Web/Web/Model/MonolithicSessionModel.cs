/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.IO;

namespace Tangosol.Web.Model
{
    /// <summary>
    /// Implementation of a <see cref="ISessionModel"/>
    /// that serializes and deserializes all session state items
    /// on each request.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.10.06</author>
    public class MonolithicSessionModel 
        : AbstractSessionModel
    {
        #region Constructors

        /// <summary>
        /// Construct MonolithicSessionModel.
        /// </summary>
        /// <param name="manager">Manager for this model.</param>
        public MonolithicSessionModel(AbstractSessionModelManager manager) 
                : base(manager)
        {}

        #endregion

        #region Implementation of ISessionModel

        /// <summary>
        /// Deserializes model using specified reader.
        /// </summary>
        /// <param name="reader">Reader to use.</param>
        public override void ReadExternal(DataReader reader)
        {
            Object[] keys   = (Object[]) Serializer.Deserialize(reader);
            Object[] values = (Object[]) Serializer.Deserialize(reader);

            for (int i = 0; i < keys.Length; i++)
            {
                BaseAdd((String) keys[i], values[i]);
            }
        }

        /// <summary>
        /// Serializes model using specified writer.
        /// </summary>
        /// <param name="writer">Writer to use.</param>
        public override void WriteExternal(DataWriter writer)
        {
            Serializer.Serialize(writer, BaseGetAllKeys());
            Serializer.Serialize(writer, BaseGetAllValues());
        }

        #endregion
    }
}