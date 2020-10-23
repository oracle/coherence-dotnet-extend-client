/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// <see cref="MessageFactory"/> for version 2 of the NamedCache
    /// Protocol.
    /// </summary>
    /// <remarks>
    /// The type identifiers of the <see cref="Message"/> classes
    /// instantiated by this <b>MessageFactory</b> are organized as follows:
    /// (1-10):
    ///
    /// (1) <see cref="SizeRequest"/>
    /// (2) <see cref="ContainsKeyRequest"/>
    /// (3) <see cref="ContainsValueRequest"/>
    /// (4) <see cref="GetRequest"/>
    /// (5) <see cref="PutRequest"/>
    /// (6) <see cref="RemoveRequest"/>
    /// (7) <see cref="PutAllRequest"/>
    /// (8) <see cref="ClearRequest"/>
    /// (9) <see cref="ContainsAllRequest"/>
    /// (10) <see cref="RemoveAllRequest"/>
    ///
    /// IObservableCache (11-20):
    ///
    /// (11) <see cref="ListenerKeyRequest"/>
    /// (12) <see cref="ListenerFilterRequest"/>
    /// (13) <see cref="CacheEvent"/>
    ///
    /// ICache (21-30):
    ///
    /// (21) <see cref="GetAllRequest"/>
    ///
    /// IConcurrentCache (31-40):
    ///
    /// (31) <see cref="LockRequest"/>
    ///
    /// IQueryCache (41-50):
    ///
    /// (41) <see cref="QueryRequest"/>
    /// (42) <see cref="IndexRequest"/>
    ///
    /// IInvocableCache (51-60):
    ///
    /// (51) <see cref="AggregateAllRequest"/>
    /// (52) <see cref="AggregateFilterRequest"/>
    /// (53) <see cref="InvokeRequest"/>
    /// (54) <see cref="InvokeAllRequest"/>
    /// (55) <see cref="InvokeFilterRequest"/>
    /// </remarks>
    /// <author>Ivan Cikic  2006.08.25</author>
    /// <seealso cref="MessageFactory"/>
    /// <seealso cref="NamedCacheProtocol"/>
    public class NamedCacheFactory : MessageFactory
    {
        /// <summary>
        /// Initialize an array of <see cref="Message"/> subclasses that can
        /// be created by this factory.
        /// </summary>
        public NamedCacheFactory()
        {
            InitializeMessageTypes(messagingTypes);
        }

        /// <summary>
        /// An array of <b>Message</b> subclasses that can be created by this
        /// factory.
        /// </summary>
        private Type[] messagingTypes = { typeof(AggregateAllRequest),
                                          typeof(AggregateFilterRequest),
                                          typeof(ClearRequest),
                                          typeof(ContainsAllRequest),
                                          typeof(ContainsKeyRequest),
                                          typeof(ContainsValueRequest),
                                          typeof(GetAllRequest),
                                          typeof(GetRequest),
                                          typeof(IndexRequest),
                                          typeof(InvokeAllRequest),
                                          typeof(InvokeFilterRequest),
                                          typeof(InvokeRequest),
                                          typeof(ListenerFilterRequest),
                                          typeof(ListenerKeyRequest),
                                          typeof(LockRequest),
                                          typeof(CacheEvent),
                                          typeof(NamedCachePartialResponse),
                                          typeof(PutAllRequest),
                                          typeof(PutRequest),
                                          typeof(QueryRequest),
                                          typeof(RemoveAllRequest),
                                          typeof(RemoveRequest),
                                          typeof(SizeRequest),
                                          typeof(UnlockRequest),
                                          typeof(NamedCacheResponse) };
    }
}