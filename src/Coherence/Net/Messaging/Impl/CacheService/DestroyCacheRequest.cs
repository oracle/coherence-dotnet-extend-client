/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Messaging.Impl.CacheService
{
    /// <summary>
    /// The DestroyCacheRequest is a <see cref="CacheServiceRequest"/> sent
    /// to destroy a NamedCache running within a Coherence cluster.
    /// </summary>
    /// <author>Ivan Cikic  2006.08.28</author>
    /// <seealso cref="CacheServiceRequest"/>
    public class DestroyCacheRequest : CacheServiceRequest
    {
        #region Properties

        /// <summary>
        /// Return the type identifier for this <b>Message</b>.
        /// </summary>
        /// <value>
        /// An identifier that uniquely identifies <b>Message</b> object's
        /// class.
        /// </value>
        public override int TypeId
        {
            get { return TYPE_ID; }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = 2;

        #endregion
    }
}