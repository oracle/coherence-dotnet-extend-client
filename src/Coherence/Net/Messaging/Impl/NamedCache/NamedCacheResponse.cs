/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// Generic <see cref="Response"/> used for basic NamedCache Protocol
    /// Responses.
    /// </summary>
    /// <author>Ana Cikic  2006.08.25</author>
    /// <seealso cref="Response"/>
    /// <seealso cref="NamedCacheProtocol"/>
    public class NamedCacheResponse : Response
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

        //CLOVER:OFF
        #region IRunnable implementation

        /// <summary>
        /// Execute the action specific to the <see cref="Message"/>
        /// implementation.
        /// </summary>
        /// <seealso cref="Message.Run"/>
        public override void Run()
        {}

        #endregion
        //CLOVER:ON

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = 0;

        #endregion
    }
}