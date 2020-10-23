/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections.Specialized;
using System.IO;

using Tangosol.Net.Impl;
using Tangosol.Net.Messaging.Impl.CacheService;
using Tangosol.Run.Xml;
using Tangosol.Util.Aggregator;
using Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    public class BaseNamedCacheTest
    {
        protected NameValueCollection appSettings = TestUtils.AppSettings;
        protected RemoteNamedCache.ConverterValueToBinary convToBinary = new RemoteNamedCache.ConverterValueToBinary();
        protected RemoteNamedCache.ConverterFromBinary convFromBinary = new RemoteNamedCache.ConverterFromBinary();

        protected String CacheName
        {
            get { return appSettings.Get("cacheName"); }
        }

        protected String CacheNameTemp
        {
            get { return appSettings.Get("cacheNameTemp"); }
        }

        protected TcpInitiator GetInitiator()
        {
            var initiator = new TcpInitiator
            {
                OperationalContext = new DefaultOperationalContext()
            };

            Stream stream = GetType().Assembly.GetManifestResourceStream("Tangosol.Resources.s4hc-cache-config.xml");
            IXmlDocument xmlConfig = XmlHelper.LoadXml(stream);
            IXmlElement initConfig =
                    xmlConfig.FindElement("caching-schemes/remote-cache-scheme/initiator-config");

            initiator.Configure(initConfig);
            initiator.RegisterProtocol(CacheServiceProtocol.Instance);
            initiator.RegisterProtocol(NamedCacheProtocol.Instance);
            initiator.Start();

            convToBinary.Serializer = initiator.InternalChannel.Serializer;
            convFromBinary.Serializer = initiator.InternalChannel.Serializer;

            return initiator;
        }

        public IChannel GetNamedCacheChannel(IConnection conn)
        {
            IChannel cacheService = conn.OpenChannel(CacheServiceProtocol.Instance,
                                                                     "CacheServiceProxy", null, null);
            EnsureCacheRequest ensureCache =
                    (EnsureCacheRequest)cacheService.MessageFactory.CreateMessage(EnsureCacheRequest.TYPE_ID);
            ensureCache.CacheName = CacheName;

            string response = (string)cacheService.Request(ensureCache);
            Uri uri = new Uri(response);
            return conn.AcceptChannel(uri, null, null);
        }
    }

    public class PriorityAggregatorStub : PriorityAggregator
    {
        public bool fRunCanceledCalled = false;

        public override void RunCanceled(bool isAbandoned)
        {
            fRunCanceledCalled = isAbandoned;
        }
    }

    public class PriorityProcessorStub : PriorityProcessor
    {
        public bool fRunCanceledCalled = false;

        public override void RunCanceled(bool isAbandoned)
        {
            fRunCanceledCalled = isAbandoned;
        }
    }
}
