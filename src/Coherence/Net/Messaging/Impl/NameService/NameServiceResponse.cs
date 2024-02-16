/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿namespace Tangosol.Net.Messaging.Impl.NameService
{
    /// <summary>
    /// Generic <see cref="Response"/> used for basic NameService
    /// Protocol Responses.
    /// </summary>
    /// <author>Wei Lin  2012.05.23</author>
    /// <since>Coherence 12.1.2</since>
    /// <seealso cref="Response"/>
    /// <seealso cref="NameServiceProtocol"/>
    public class NameServiceResponse : Response
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

        #region Request processing

        /// <summary>
        /// Execute the action specific to the <see cref="Message"/>
        /// implementation.
        /// </summary>
        /// <seealso cref="Message.Run"/>
        public override void Run()
        {
            // no-op
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = 0;

        #endregion
    }
}
