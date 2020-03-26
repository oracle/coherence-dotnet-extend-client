/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.IO;

namespace Tangosol.Web.Model
{
    /// <summary>
    /// Implementation of <see cref="ISessionModelManager"/> that serializes 
    /// and deserializes all session items on each request. 
    /// </summary>
    /// <author>Aleksandar Seovic  2008.11.13</author>
    public class MonolithicSessionModelManager 
        : AbstractSessionModelManager
    {
        #region Constructors

        /// <summary>
        /// Construct instance of MonolithicSessionModelManager.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        public MonolithicSessionModelManager(ISerializer serializer) 
            : this(serializer, null)
        {}

        /// <summary>
        /// Construct instance of MonolithicSessionModelManager.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        /// <param name="cacheName">The cache name.</param>
        public MonolithicSessionModelManager(ISerializer serializer, string cacheName) 
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
            return new MonolithicSessionModel(this);
        }

        #endregion
    }
}