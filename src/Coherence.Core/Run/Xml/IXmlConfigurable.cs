/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Run.Xml
{
    /// <summary>
    /// An interface for XML configuration.
    /// </summary>
    /// <author>Cameron Purdy  2002.08.20</author>
    /// <author>Ana Cikic  2009.08.28</author>
    public interface IXmlConfigurable
    {
        /// <summary>
        /// The current configuration of the object.
        /// </summary>
        /// <value>
        /// The XML configuration or <c>null</c>.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// When setting, if the object is not in a state that allows the
        /// configuration to be set; for example, if the object has already
        /// been configured and cannot be reconfigured.
        /// </exception>
        IXmlElement Config { get; set; }
    }
}