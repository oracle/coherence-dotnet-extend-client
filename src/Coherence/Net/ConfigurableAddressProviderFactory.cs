/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Text;
using Tangosol.Run.Xml;

namespace Tangosol.Net
{
    /// <summary>
    /// A <see cref="IAddressProviderFactory"/> implementation that creates instances of a
    /// AddressProvider class configured using an XmlElement of the following structure:
    /// <pre>
    /// &lt;socket-address&gt;
    ///     &lt;address&gt;...&lt;/address&gt;
    ///     &lt;port&gt;...&lt;/port&gt;
    /// &lt;/socket-address&gt;
    /// </pre>
    /// </summary>
    /// <author>Wei Lin  2012.04.11</author>
    /// <since>Coherence 12.1.2</since>
    public class ConfigurableAddressProviderFactory : IAddressProviderFactory, IXmlConfigurable
    {
        #region IAddressProviderFactory implementation

        /// <summary>
        /// Instantiate an <see cref="IAddressProvider"/> configured according 
        /// to the specified XML. The passed XML has to conform to the 
        /// following format:
        /// <pre>
        ///   &lt;!ELEMENT ... (socket-address+ | address-provider)&gt;
        ///   &lt;!ELEMENT address-provider
        ///     (class-name | (class-factory-name, method-name), init-params?&gt;
        ///   &lt;!ELEMENT socket-address (address, port)&gt;
        /// </pre>
        /// </summary>
        /// <returns>
        /// An instance of the corresponding <b>IAddressProvider</b>
        /// implementation.
        /// </returns>
        public IAddressProvider CreateAddressProvider()
        {
            IXmlElement config      = Config;
            string      elementName = config.Name;

            if (elementName.Equals("address-provider"))
            {
                IXmlElement xmlSocketAddress = config.GetElement("socket-address");
                if (xmlSocketAddress != null)
                {
                    return new ConfigurableAddressProvider(config);
                }

                return (IAddressProvider) XmlHelper.CreateInstance(config, null, typeof(IAddressProvider));
            }
            return new ConfigurableAddressProvider(config);
        }

        #endregion

        #region IXmlConfigurable implementation

        /// <summary>
        /// <see cref="IXmlElement"/> holding configuration information.
        /// </summary>
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
            return "ConfigurableAddressProviderFactory{Xml=" + Config +"}";
        }

        #endregion
    }
}
