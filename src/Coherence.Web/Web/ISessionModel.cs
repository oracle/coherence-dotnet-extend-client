/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Web.SessionState;

using Tangosol.IO;

namespace Tangosol.Web
{
    /// <summary>
    /// Session model interface.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.11.16</author>
    public interface ISessionModel : ISessionStateItemCollection
    {
        /// <summary>
        /// Gets or sets session key.
        /// </summary>
        /// <value>
        /// Session key.
        /// </value>
        SessionKey SessionId { get; set; }

        /// <summary>
        /// Deserialize model using specified reader.
        /// </summary>
        /// <param name="reader">Reader to use.</param>
        void ReadExternal(DataReader reader);

        /// <summary>
        /// Serialize model using specified writer.
        /// </summary>
        /// <param name="writer">Writer to use.</param>
        void WriteExternal(DataWriter writer);
    }
}