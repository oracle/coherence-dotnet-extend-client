/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.IO;
using Tangosol.Util;

namespace Tangosol.Web.Model
{
    /// <summary>
    /// Implementation of <see cref="ISessionModelManager"/> that
    /// transfers whole session state as a single entity, but
    /// only deserializes and serializes individual items on demand. 
    /// </summary>
    /// <author>Aleksandar Seovic  2009.10.06</author>
    public class TraditionalSessionModelManager 
        : AbstractSessionModelManager
    {
        #region Constructors

        /// <summary>
        /// Construct instance of TraditionalSessionModelManager.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        public TraditionalSessionModelManager(ISerializer serializer) 
            : this(serializer, null)
        {}

        /// <summary>
        /// Construct instance of TraditionalSessionModelManager.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        /// <param name="cacheName">The cache name.</param>
        public TraditionalSessionModelManager(ISerializer serializer, string cacheName) 
            : base(serializer, cacheName)
        {
        }

        #endregion

        #region Overrides of AbstractSessionModelManager

        /// <summary>
        /// Create a <see cref="ISessionModel"/> instance.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="ISessionModel"/>.
        /// </returns>
        public override ISessionModel CreateSessionModel()
        {
            return new TraditionalSessionModel(this);
        }

        /// <summary>
        /// Deserialize specified Binary into a <b>ISessionModel</b>.
        /// </summary>
        /// <param name="binModel">
        /// A Binary containing the serialized form of a <b>ISessionModel</b>.
        /// </param>
        /// <returns>
        /// The deserialized ISessionModel.
        /// </returns>
        public override ISessionModel Deserialize(Binary binModel)
        {
            using (DataReader reader = binModel.GetReader())
            {
                ISessionModel model = new TraditionalSessionModel(this, binModel);
                model.ReadExternal(reader);
                return model;
            }
        }

        #endregion
    }
}