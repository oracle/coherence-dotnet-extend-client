/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Text;
using Tangosol.IO.Pof;
using Tangosol.Run.Xml;
using Tangosol.Util;

namespace Tangosol.IO
{
    /// <summary>
    /// A <see cref="ISerializerFactory"/> implementation that creates instances of a
    /// Serializer class configured using an XmlElement of the following structure:
    /// <pre>
    ///   &lt;!ELEMENT instance ((class-name | (class-factory-name, method-name), init-params?)&gt;
    ///   &lt;!ELEMENT init-params (init-param*)&gt;
    ///   &lt;!ELEMENT init-param ((param-name | param-type), param-value, description?)&gt;
    /// </pre>
    /// </summary>
    /// <author>Wei Lin  2011.10.25</author>
    /// <since>Coherence 12.1.2</since>
    public class ConfigurableSerializerFactory : ISerializerFactory, IXmlConfigurable
    {
        #region ISerializerFactory implementation

        /// <summary>
        /// Create a new <see cref="ISerializer"/>.
        /// </summary>
        /// <returns>
        /// The new <see cref="ISerializer"/>.
        /// </returns>
        public ISerializer CreateSerializer()
        {
            return (ISerializer) XmlHelper.CreateInstance(Config, 
                    /*resolver*/null, typeof(ISerializer));
        }

        #endregion

        #region IXmlConfigurable implementation

        /// <summary>
        /// <see cref="IXmlElement"/> holding configuration information.
        /// </summary>
        /// <remarks>
        /// Note that the configuration will not be available unless the
        /// <see cref="ConfigurablePofContext"/> was constructed with the
        /// configuration, the configuration was specified using the
        /// <see cref="IXmlConfigurable"/> interface, or the
        /// <see cref="ConfigurablePofContext"/> has fully initialized itself
        /// <p>
        /// Also, note that the configuration cannot be set after the
        /// <see cref="ConfigurablePofContext"/> is fully initialized.
        /// </p>
        /// </remarks>
        /// <value>
        /// <see cref="IXmlElement"/> holding configuration information.
        /// </value>
        public virtual IXmlElement Config { get; set; }

        #endregion

        #region Object override methods

        /// <summary>
        /// Provide a human-readable representation of this object.
        /// </summary>
        /// <returns>
        /// A string whose contents represent the value of this object.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(GetType().Name)
                .Append("ConfigurableSerializerFactory{Xml=")
                .Append(Config)
                .Append("}");

            return sb.ToString();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Serializer type.
        /// </summary>
        /// <value>
        /// Serializer type.
        /// </value>
        public Type SerializerType { get; set; }

        #endregion
    }
}
