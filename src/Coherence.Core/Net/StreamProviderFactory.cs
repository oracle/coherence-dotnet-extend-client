/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using Tangosol.Run.Xml;

namespace Tangosol.Net
{
    /// <summary>
    /// Provides a mechanism for creating StreamProviders.
    /// </summary>
    public static class StreamProviderFactory
    {
        private const string SSL_NAME = "ssl";

        /// <summary>
        /// Create the configured <b>IStreamProvider</b> used to provide
        /// a network stream.
        /// </summary>
        /// <param name="xml">An <b>IXmlElement</b> containing configuraiton for the StreamProviderFactory.</param>
        /// <returns>An <b>IStreamProvider</b>.</returns>
        public static IStreamProvider CreateProvider(IXmlElement xml)
        {
            IXmlElement xmlElement = xml.GetElement(SSL_NAME);

            if (xmlElement != null)
            {
                IStreamProvider streamProvider =
                        new SslStreamProvider {Config = xmlElement};
                return streamProvider;
            }

            return new SystemStreamProvider();
        }
    }
}
