/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.IO;
using Tangosol.Net;
using Tangosol.Util;
using Tangosol.Util.Extractor;

namespace Tangosol.Web.Model
{
    /// <summary>
    /// Implementation of <see cref="ISessionModelManager"/> that
    /// stores large session attributes as separate cache entries. 
    /// </summary>
    /// <author>Aleksandar Seovic  2009.10.07</author>
    public class SplitSessionModelManager 
        : AbstractSessionModelManager
    {
        #region Constructors

        /// <summary>
        /// Construct instance of SplitSessionModelManager.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        /// <param name="minExtAttributeSize">Minimum external attribute size.</param>
        public SplitSessionModelManager(ISerializer serializer, int minExtAttributeSize)
            : this(serializer, null, minExtAttributeSize)
        {}

        /// <summary>
        /// Construct instance of SplitSessionModelManager.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        /// <param name="cacheName">The cache name.</param>
        /// <param name="minExtAttributeSize">Minimum external attribute size.</param>
        public SplitSessionModelManager(ISerializer serializer,
            string cacheName, int minExtAttributeSize) 
            : base(serializer, cacheName)
        {
            m_externalAttributeCache = CacheFactory.GetCache(EXTERNAL_ATTRIBUTES_CACHE_NAME);
            m_externalAttributeCache.AddIndex(new ReflectionExtractor(
                "getSessionKey", null, ReflectionExtractor.KEY), false, null);

            m_minExtAttributeSize    = minExtAttributeSize == 0 
                ? DEFAULT_MIN_EXT_ATTRIBUTE_SIZE 
                : minExtAttributeSize;
        }

        #endregion

        #region Properties

        /// <summary>
        /// External attribute cache.
        /// </summary>
        public INamedCache ExternalAttributeCache
        {
            get { return m_externalAttributeCache; }
        }

        /// <summary>
        /// Minimum external attribute size.
        /// </summary>
        public int MinExtAttributeSize
        {
            get { return m_minExtAttributeSize; }
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
            return new SplitSessionModel(this);
        }

        /// <summary>
        /// Deserialize specified Binary into a <b>ISessionModel</b>.
        /// </summary>
        /// <param name="binModel">
        /// A Binary containing the serialized form of a <b>ISessionModel</b>.
        /// </param>
        /// <returns>
        /// The deserialized <b>ISessionModel</b>.
        /// </returns>
        public override ISessionModel Deserialize(Binary binModel)
        {
            using (DataReader reader = binModel.GetReader())
            {
                ISessionModel model = new SplitSessionModel(this, binModel);
                model.ReadExternal(reader);
                return model;
            }
        }

        /// <summary>
        /// Get external attributes.
        /// </summary>
        /// <param name="model">
        /// Model to get the external attributes from.
        /// </param>
        /// <returns>External attributes dictionary.</returns>
        protected override IDictionary GetExternalAttributes(ISessionModel model)
        {
            IDictionary extAttrs = ((SplitSessionModel) model).GetExternalAttributes();
            return extAttrs.Count > 0 ? extAttrs : null;
        }

        /// <summary>
        /// Get obsolete external attributes.
        /// </summary>
        /// <param name="model">
        /// Model to get the obsolete external attributes from.
        /// </param>
        /// <returns>External attributes dictionary.</returns>
        protected override IList GetObsoleteExternalAttributes(ISessionModel model)
        {
            IList obsoleteExtAttrs = ((SplitSessionModel) model).GetObsoleteExternalAttributes();
            return obsoleteExtAttrs.Count > 0 ? obsoleteExtAttrs : null;
        }

        #endregion

        #region Data members

        /// <summary>
        /// External attributes cache name.
        /// </summary>
        public const string EXTERNAL_ATTRIBUTES_CACHE_NAME = "aspnet-session-overflow";

        /// <summary>
        /// Default minimum external attribute size.
        /// </summary>
        private const int DEFAULT_MIN_EXT_ATTRIBUTE_SIZE = 1024;

        /// <summary>
        /// External attribute cache.
        /// </summary>
        private readonly INamedCache m_externalAttributeCache;

        /// <summary>
        /// Minimum external attribute size.
        /// </summary>
        private readonly int m_minExtAttributeSize;

        #endregion
    }
}