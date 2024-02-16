/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.Collections.Specialized;

using Tangosol.Run.Xml;

namespace Tangosol
{
    /// <summary>
    /// Summary description for TestUtils.
    /// </summary>
    public class TestUtils
    {
        private static NameValueCollection appSettings;

        public static NameValueCollection AppSettings
        {
            get
            {
                if (appSettings == null)
                {
                    appSettings = new NameValueCollection();
                    appSettings["cacheName"] = "dist-extend-direct";
                    appSettings["cacheNameTemp"] = "dist-extend-direct-temp";
                    appSettings["sslOneWayCacheName"] = "dist-extend-oneway-ssl";
                    appSettings["sslTwoWayCacheName"] = "dist-extend-twoway-ssl";
                    appSettings["tls12CacheName"] = "dist-extend-tls12";
                }
                return appSettings;
            }
        }

        public static IDictionary CreateFilterConfigMap(IXmlElement filterConfig)
        {
            IDictionary mapConfigByName = new Hashtable();
            IXmlElement config = filterConfig;
            if (config != null)
            {
                for (IEnumerator enumerator = config.GetElements("filter"); enumerator.MoveNext(); )
                {
                    IXmlElement xmlFilter = (IXmlElement) enumerator.Current;
                    string name = xmlFilter.GetSafeElement("filter-name").GetString();
                    mapConfigByName.Add(name, xmlFilter);
                }
            }
            return mapConfigByName;
        }
    }
}