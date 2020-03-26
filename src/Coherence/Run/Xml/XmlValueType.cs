/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Run.Xml
{
    /// <summary>
    /// Types supported by <see cref="IXmlValue"/>.
    /// </summary>
    /// <author>Ana Cikic  2008.08.25</author>
    public enum XmlValueType
    {
        /// <summary>
        /// Boolean type.
        /// </summary>
        Boolean  = 1,
        /// <summary>
        /// Integer type.
        /// </summary>
        Integer  = 2,
        /// <summary>
        /// Long type.
        /// </summary>
        Long     = 3,
        /// <summary>
        /// Double type.
        /// </summary>
        Double   = 4,
        /// <summary>
        /// Decimal type.
        /// </summary>
        Decimal  = 5,
        /// <summary>
        /// String type.
        /// </summary>
        String   = 6,
        /// <summary>
        /// Binary type.
        /// </summary>
        Binary   = 7,
        /// <summary>
        /// DateTime type.
        /// </summary>
        DateTime = 8,
        /// <summary>
        /// Float/Single type.
        /// </summary>
        Float    = 9,
        /// <summary>
        /// System.Io.File
        /// </summary>
        File     = 10,
        /// <summary>
        /// The <b>init-param</b> XmlNode
        /// </summary>
        Xml      = 11,
        /// <summary>
        /// Unknown type.
        /// </summary>
        Unknown  = 12
        
    }
}