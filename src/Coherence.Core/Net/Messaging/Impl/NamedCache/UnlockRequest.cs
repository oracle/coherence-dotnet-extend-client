/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// The LockRequest is a <see cref="KeyRequest"/> sent to unlock a
    /// specified key in a remote NamedCache.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.08.31</author>
    /// <seealso cref="KeyRequest"/>
    /// <seealso cref="NamedCacheProtocol"/>
    public class UnlockRequest : KeyRequest
    {
        #region Properties

        /// <summary>
        /// Return the identifier for <b>Message</b> object's class.
        /// </summary>
        /// <value>
        /// An identifier that uniquely identifies <b>Message</b> object's
        /// class.
        /// </value>
        /// <seealso cref="Message.TypeId"/>
        public override int TypeId
        {
            get { return TYPE_ID; }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 32;

        #endregion
    }
}