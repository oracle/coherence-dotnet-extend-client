/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

using NUnit.Framework;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Messaging.Impl.CacheService;
using Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    [TestFixture]
    public class NamedCacheTests : BaseNamedCacheTest
    {
        [Test]
        public void TestMessageFactory()
        {
            TcpInitiator initiator = GetInitiator();
            IConnection conn = initiator.EnsureConnection();
            IChannel namedCache = GetNamedCacheChannel(conn);

            IMessage getRequest = namedCache.MessageFactory.CreateMessage(GetRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(GetRequest), getRequest);
            Assert.AreEqual(GetRequest.TYPE_ID, getRequest.TypeId);

            IMessage putRequest = namedCache.MessageFactory.CreateMessage(PutRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(PutRequest), putRequest);
            Assert.AreEqual(PutRequest.TYPE_ID, putRequest.TypeId);

            IMessage sizeRequest = namedCache.MessageFactory.CreateMessage(SizeRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(SizeRequest), sizeRequest);
            Assert.AreEqual(SizeRequest.TYPE_ID, sizeRequest.TypeId);

            IMessage getAllRequest = namedCache.MessageFactory.CreateMessage(GetAllRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(GetAllRequest), getAllRequest);
            Assert.AreEqual(GetAllRequest.TYPE_ID, getAllRequest.TypeId);

            IMessage putAllRequest = namedCache.MessageFactory.CreateMessage(PutAllRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(PutAllRequest), putAllRequest);
            Assert.AreEqual(PutAllRequest.TYPE_ID, putAllRequest.TypeId);

            IMessage clearRequest = namedCache.MessageFactory.CreateMessage(ClearRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(ClearRequest), clearRequest);
            Assert.AreEqual(ClearRequest.TYPE_ID, clearRequest.TypeId);

            IMessage containsKeyRequest = namedCache.MessageFactory.CreateMessage(ContainsKeyRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(ContainsKeyRequest), containsKeyRequest);
            Assert.AreEqual(ContainsKeyRequest.TYPE_ID, containsKeyRequest.TypeId);

            IMessage containsValueRequest = namedCache.MessageFactory.CreateMessage(ContainsValueRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(ContainsValueRequest), containsValueRequest);
            Assert.AreEqual(ContainsValueRequest.TYPE_ID, containsValueRequest.TypeId);

            IMessage containsAllRequest = namedCache.MessageFactory.CreateMessage(ContainsAllRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(ContainsAllRequest), containsAllRequest);
            Assert.AreEqual(ContainsAllRequest.TYPE_ID, containsAllRequest.TypeId);

            IMessage removeRequest = namedCache.MessageFactory.CreateMessage(RemoveRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(RemoveRequest), removeRequest);
            Assert.AreEqual(RemoveRequest.TYPE_ID, removeRequest.TypeId);

            IMessage removeAllRequest = namedCache.MessageFactory.CreateMessage(RemoveAllRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(RemoveAllRequest), removeAllRequest);
            Assert.AreEqual(RemoveAllRequest.TYPE_ID, removeAllRequest.TypeId);

            conn.Close();
            initiator.Stop();
        }

        [Test]
        public void TestPutRequest()
        {
            TcpInitiator initiator = GetInitiator();
            IConnection conn = initiator.EnsureConnection();
            IChannel namedCacheChannel = GetNamedCacheChannel(conn);
            string key = "testPutKey";
            string value1 = "testPutValue1";
            string value2 = "testPutValue2";
            object previousValue = null;

            GetRequest getRequest = (GetRequest) namedCacheChannel.MessageFactory.CreateMessage(GetRequest.TYPE_ID);
            getRequest.Key = convToBinary.Convert(key);
            IResponse getResponse = namedCacheChannel.Send(getRequest).WaitForResponse(-1);
            if (!getResponse.IsFailure)
            {
                previousValue = convFromBinary.Convert(getResponse.Result);
            }

            PutRequest putRequest = (PutRequest) namedCacheChannel.MessageFactory.CreateMessage(PutRequest.TYPE_ID);
            putRequest.Key = convToBinary.Convert(key);
            putRequest.Value = convToBinary.Convert(value1);
            putRequest.IsReturnRequired = true;

            Stream stream = new MemoryStream();
            Codec codec = new Codec();
            codec.Encode(namedCacheChannel, putRequest, new DataWriter(stream));
            stream.Position = 0;
            PutRequest result = (PutRequest) codec.Decode(namedCacheChannel, new DataReader(stream));
            Assert.AreEqual(putRequest.ExpiryDelay, result.ExpiryDelay);
            Assert.AreEqual(putRequest.IsReturnRequired, result.IsReturnRequired);
            Assert.AreEqual(convToBinary.Convert(value1), result.Value);
            Assert.IsNull(putRequest.Key);
            putRequest = result; //necessary, since Key has been null-ed while testing serialization

            IResponse putResponse = namedCacheChannel.Send(putRequest).WaitForResponse(-1);
            Assert.AreEqual(putRequest.TypeId, PutRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), putResponse);
            Assert.AreEqual(namedCacheChannel.MessageFactory.Version, putRequest.ImplVersion);
            Assert.IsFalse(putResponse.IsFailure);
            Assert.AreEqual(putResponse.RequestId, putRequest.Id);
            Assert.AreEqual(0, putResponse.TypeId);
            if (putRequest.IsReturnRequired)
            {
                if (previousValue == null)
                {
                    Assert.IsNull(putResponse.Result);
                }
                else
                {
                    Assert.AreEqual(convToBinary.Convert(previousValue), putResponse.Result);
                }
            }

            putRequest = (PutRequest) namedCacheChannel.MessageFactory.CreateMessage(PutRequest.TYPE_ID);
            putRequest.Key = convToBinary.Convert(key);
            putRequest.Value = convToBinary.Convert(value2);
            putRequest.IsReturnRequired = true;

            putResponse = namedCacheChannel.Send(putRequest).WaitForResponse(-1);
            Assert.IsFalse(putResponse.IsFailure);
            Assert.AreEqual(putResponse.RequestId, putRequest.Id);
            Assert.AreEqual(0, putResponse.TypeId);
            if (putRequest.IsReturnRequired)
            {
                Assert.AreEqual(value1, convFromBinary.Convert(putResponse.Result));
            }

            conn.Close();
            initiator.Stop();
        }

        [Test]
        public void TestGetRequest()
        {
            TcpInitiator initiator = GetInitiator();
            IConnection conn = initiator.EnsureConnection();
            IChannel namedCacheChannel = GetNamedCacheChannel(conn);
            string key = "testGetKey";
            string value = "testGetValue";

            PutRequest putRequest = (PutRequest) namedCacheChannel.MessageFactory.CreateMessage(PutRequest.TYPE_ID);
            putRequest.Key = convToBinary.Convert(key);
            putRequest.Value = convToBinary.Convert(value);
            namedCacheChannel.Request(putRequest);

            GetRequest getRequest = (GetRequest) namedCacheChannel.MessageFactory.CreateMessage(GetRequest.TYPE_ID);
            getRequest.Key = convToBinary.Convert(key);

            IResponse getResponse = namedCacheChannel.Send(getRequest).WaitForResponse(-1);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), getResponse);
            Assert.AreEqual(namedCacheChannel.MessageFactory.Version, getRequest.ImplVersion);
            Assert.IsFalse(getResponse.IsFailure);
            Assert.AreEqual(getResponse.RequestId, getRequest.Id);
            Assert.AreEqual(0, getResponse.TypeId);
            Assert.AreEqual(convFromBinary.Convert(getResponse.Result), value);

            conn.Close();
            initiator.Stop();
        }

        [Test]
        public void TestGetAndPutAllRequest()
        {
            TcpInitiator initiator = GetInitiator();
            IConnection conn = initiator.EnsureConnection();
            IChannel namedCacheChannel = GetNamedCacheChannel(conn);
            string[] keys = {"Ana Cikic", "Goran Milosavljevic", "Ivan Cikic"};
            string[] values = { "10.0.0.120", "10.0.0.180", "10.0.0.125"};

            IDictionary addresses = new Hashtable();
            addresses.Add(convToBinary.Convert(keys[0]), convToBinary.Convert(values[0]));
            addresses.Add(convToBinary.Convert(keys[1]), convToBinary.Convert(values[1]));
            addresses.Add(convToBinary.Convert(keys[2]), convToBinary.Convert(values[2]));

            PutAllRequest putAllRequest =
                    (PutAllRequest) namedCacheChannel.MessageFactory.CreateMessage(PutAllRequest.TYPE_ID);
            putAllRequest.Map = addresses;

            Stream stream = new MemoryStream();
            Codec codec = new Codec();
            codec.Encode(namedCacheChannel, putAllRequest, new DataWriter(stream));
            stream.Position = 0;
            PutAllRequest result = (PutAllRequest) codec.Decode(namedCacheChannel, new DataReader(stream));
            Assert.AreEqual(3, result.Map.Count);
            Assert.AreEqual(addresses[keys[0]], result.Map[keys[0]]);
            Assert.AreEqual(addresses[keys[1]], result.Map[keys[1]]);
            Assert.AreEqual(addresses[keys[2]], result.Map[keys[2]]);

            IResponse putAllResponse = namedCacheChannel.Send(putAllRequest).WaitForResponse(-1);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), putAllResponse);
            Assert.AreEqual(putAllRequest.Id, putAllResponse.RequestId);
            Assert.IsNull(putAllResponse.Result);
            Assert.IsFalse(putAllResponse.IsFailure);

            GetAllRequest getAllRequest =
                    (GetAllRequest) namedCacheChannel.MessageFactory.CreateMessage(GetAllRequest.TYPE_ID);

            ArrayList names = new ArrayList();
            names.Add(convToBinary.Convert(keys[1]));
            names.Add(convToBinary.Convert(keys[2]));
            getAllRequest.Keys = names;

            IResponse getAllResponse = namedCacheChannel.Send(getAllRequest).WaitForResponse(-1);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), getAllResponse);
            Assert.IsFalse(getAllResponse.IsFailure);
            Assert.AreEqual(getAllResponse.RequestId, getAllRequest.Id);
            Assert.AreEqual(0, getAllResponse.TypeId);
            Assert.IsInstanceOf(typeof(IDictionary), getAllResponse.Result);

            IDictionary resultDict = new Hashtable((IDictionary) getAllResponse.Result);
            Assert.AreEqual(addresses[keys[1]], resultDict[keys[1]]);
            Assert.AreEqual(addresses[keys[2]], resultDict[keys[2]]);

            conn.Close();
            initiator.Stop();
        }

        [Test]
        public void TestSizeRequest()
        {
            TcpInitiator initiator = GetInitiator();
            IConnection conn = initiator.EnsureConnection();
            IChannel namedCacheChannel = GetNamedCacheChannel(conn);

            ClearRequest clearRequest =
                    (ClearRequest) namedCacheChannel.MessageFactory.CreateMessage(ClearRequest.TYPE_ID);

            IResponse clearResponse = namedCacheChannel.Send(clearRequest).WaitForResponse(-1);
            Assert.AreEqual(clearRequest.TypeId, ClearRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), clearResponse);
            Assert.AreEqual(clearResponse.TypeId, NamedCacheResponse.TYPE_ID);
            Assert.AreEqual(clearResponse.RequestId, clearRequest.Id);
            Assert.IsFalse(clearResponse.IsFailure);

            SizeRequest sizeRequestBefore =
                    (SizeRequest) namedCacheChannel.MessageFactory.CreateMessage(SizeRequest.TYPE_ID);

            IResponse sizeResponseBefore = namedCacheChannel.Send(sizeRequestBefore).WaitForResponse(-1);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), sizeResponseBefore);
            Assert.IsFalse(sizeResponseBefore.IsFailure);
            Assert.AreEqual(sizeResponseBefore.RequestId, sizeRequestBefore.Id);
            Assert.AreEqual(0, sizeResponseBefore.TypeId);
            Assert.IsInstanceOf(typeof(Int32), sizeResponseBefore.Result);
            int before = (int) sizeResponseBefore.Result;
            Assert.AreEqual(0, before);

            PutRequest putRequest = (PutRequest) namedCacheChannel.MessageFactory.CreateMessage(PutRequest.TYPE_ID);
            putRequest.Key = convToBinary.Convert("newKey");
            putRequest.Value = convToBinary.Convert("newValue");
            namedCacheChannel.Request(putRequest);

            SizeRequest sizeRequest =
                    (SizeRequest) namedCacheChannel.MessageFactory.CreateMessage(SizeRequest.TYPE_ID);
            IResponse sizeResponse = namedCacheChannel.Send(sizeRequest).WaitForResponse(-1);
            Assert.IsFalse(sizeResponse.IsFailure);
            Assert.AreEqual(sizeResponse.RequestId, sizeRequest.Id);
            Assert.AreEqual(0, sizeResponse.TypeId);
            Assert.IsInstanceOf(typeof(Int32), sizeResponse.Result);
            int after = (int) sizeResponse.Result;
            Assert.AreEqual(1, after);

            conn.Close();
            initiator.Stop();
        }

        [Test]
        public void TestNamedCacheException()
        {
            TcpInitiator initiator = GetInitiator();
            IConnection conn = initiator.EnsureConnection();
            IChannel cacheService = conn.OpenChannel(CacheServiceProtocol.Instance,
                                                              "CacheServiceProxy", null, null);

            EnsureCacheRequest ensureCacheRequest =
                    (EnsureCacheRequest) cacheService.MessageFactory.CreateMessage(EnsureCacheRequest.TYPE_ID);
            ensureCacheRequest.CacheName = CacheNameTemp;

            string response = (string) cacheService.Request(ensureCacheRequest);
            Uri uri = new Uri(response);
            IChannel namedCache = conn.AcceptChannel(uri, null, null);

            string[] keys = { "Ana Cikic", "Goran Milosavljevic", "Ivan Cikic"};
            string[] values = { "10.0.0.120", "10.0.0.180", "10.0.0.125" };
            IDictionary addresses = new Hashtable();
            addresses.Add(convToBinary.Convert(keys[0]), convToBinary.Convert(values[0]));
            addresses.Add(convToBinary.Convert(keys[1]), convToBinary.Convert(values[1]));
            addresses.Add(convToBinary.Convert(keys[2]), convToBinary.Convert(values[2]));

            PutAllRequest putAllRequest =
                    (PutAllRequest) namedCache.MessageFactory.CreateMessage(PutAllRequest.TYPE_ID);
            putAllRequest.Map = addresses;
            namedCache.Request(putAllRequest);

            DestroyCacheRequest destroyCacheRequest =
                    (DestroyCacheRequest)cacheService.MessageFactory.CreateMessage(DestroyCacheRequest.TYPE_ID);
            destroyCacheRequest.CacheName = CacheNameTemp;
            cacheService.Request(destroyCacheRequest);

            GetAllRequest getAllRequest =
                    (GetAllRequest) namedCache.MessageFactory.CreateMessage(GetAllRequest.TYPE_ID);
            ArrayList names = new ArrayList();
            names.Add(convToBinary.Convert(keys[1]));
            names.Add(convToBinary.Convert(keys[2]));
            getAllRequest.Keys = names;

            try
            {
                namedCache.Send(getAllRequest).WaitForResponse(-1);
            }
            catch (PortableException)
            {
            }

            conn.Close();
            initiator.Stop();
        }

        [Test]
        public void TestContainsKeyRequest()
        {
            TcpInitiator initiator = GetInitiator();
            IConnection conn = initiator.EnsureConnection();
            IChannel namedCacheChannel = GetNamedCacheChannel(conn);
            string key = "testContainsKeyKey";
            string value = "testContainsKeyValue";

            ClearRequest clearRequest =
                    (ClearRequest) namedCacheChannel.MessageFactory.CreateMessage(ClearRequest.TYPE_ID);
            IResponse clearResponse = namedCacheChannel.Send(clearRequest).WaitForResponse(-1);
            Assert.IsFalse(clearResponse.IsFailure);

            ContainsKeyRequest containsKeyRequest =
                (ContainsKeyRequest) namedCacheChannel.MessageFactory.CreateMessage(ContainsKeyRequest.TYPE_ID);
            containsKeyRequest.Key = convToBinary.Convert(key);
            IResponse containsKeyResponse = namedCacheChannel.Send(containsKeyRequest).WaitForResponse(-1);

            Assert.AreEqual(containsKeyRequest.TypeId, ContainsKeyRequest.TYPE_ID);
            Assert.AreEqual(containsKeyResponse.TypeId, NamedCacheResponse.TYPE_ID);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), containsKeyResponse);
            Assert.IsFalse(containsKeyResponse.IsFailure);
            Assert.AreEqual(containsKeyResponse.RequestId, containsKeyRequest.Id);
            Assert.IsInstanceOf(typeof(bool), containsKeyResponse.Result);
            Assert.IsFalse((bool) containsKeyResponse.Result);

            PutRequest putRequest =
                    (PutRequest) namedCacheChannel.MessageFactory.CreateMessage(PutRequest.TYPE_ID);
            putRequest.Key = convToBinary.Convert(key);
            putRequest.Value = convToBinary.Convert(value);
            namedCacheChannel.Request(putRequest);

            containsKeyRequest = (ContainsKeyRequest) namedCacheChannel.MessageFactory.CreateMessage(ContainsKeyRequest.TYPE_ID);
            containsKeyRequest.Key = convToBinary.Convert(key);
            containsKeyResponse = namedCacheChannel.Send(containsKeyRequest).WaitForResponse(-1);

            Assert.AreEqual(containsKeyRequest.TypeId, ContainsKeyRequest.TYPE_ID);
            Assert.AreEqual(containsKeyResponse.TypeId, NamedCacheResponse.TYPE_ID);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), containsKeyResponse);
            Assert.IsFalse(containsKeyResponse.IsFailure);
            Assert.AreEqual(containsKeyResponse.RequestId, containsKeyRequest.Id);
            Assert.IsInstanceOf(typeof(bool), containsKeyResponse.Result);
            Assert.IsTrue((bool) containsKeyResponse.Result);

            conn.Close();
            initiator.Stop();
        }

        [Test]
        public void TestContainsValueRequest()
        {
            TcpInitiator initiator = GetInitiator();
            IConnection conn = initiator.EnsureConnection();
            IChannel namedCacheChannel = GetNamedCacheChannel(conn);
            string key = "testContainsValueKey";
            string value = "testContainsValueValue";

            ClearRequest clearRequest =
                    (ClearRequest) namedCacheChannel.MessageFactory.CreateMessage(ClearRequest.TYPE_ID);
            IResponse clearResponse = namedCacheChannel.Send(clearRequest).WaitForResponse(-1);
            Assert.IsFalse(clearResponse.IsFailure);

            ContainsValueRequest containsValueRequest =
                (ContainsValueRequest) namedCacheChannel.MessageFactory.CreateMessage(ContainsValueRequest.TYPE_ID);
            containsValueRequest.Value = convToBinary.Convert(value);
            IResponse containsValueResponse = namedCacheChannel.Send(containsValueRequest).WaitForResponse(-1);

            Assert.AreEqual(containsValueRequest.TypeId, ContainsValueRequest.TYPE_ID);
            Assert.AreEqual(containsValueResponse.TypeId, NamedCacheResponse.TYPE_ID);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), containsValueResponse);
            Assert.IsFalse(containsValueResponse.IsFailure);
            Assert.AreEqual(containsValueResponse.RequestId, containsValueRequest.Id);
            Assert.IsInstanceOf(typeof(bool), containsValueResponse.Result);
            Assert.IsFalse((bool) containsValueResponse.Result);

            PutRequest putRequest =
                    (PutRequest) namedCacheChannel.MessageFactory.CreateMessage(PutRequest.TYPE_ID);
            putRequest.Key = convToBinary.Convert(key);
            putRequest.Value = convToBinary.Convert(value);
            namedCacheChannel.Request(putRequest);

            containsValueRequest = (ContainsValueRequest) namedCacheChannel.MessageFactory.CreateMessage(ContainsValueRequest.TYPE_ID);
            containsValueRequest.Value = convToBinary.Convert(value);
            containsValueResponse = namedCacheChannel.Send(containsValueRequest).WaitForResponse(-1);

            Assert.AreEqual(containsValueRequest.TypeId, ContainsValueRequest.TYPE_ID);
            Assert.AreEqual(containsValueResponse.TypeId, NamedCacheResponse.TYPE_ID);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), containsValueResponse);
            Assert.IsFalse(containsValueResponse.IsFailure);
            Assert.AreEqual(containsValueResponse.RequestId, containsValueRequest.Id);
            Assert.IsInstanceOf(typeof(bool), containsValueResponse.Result);
            Assert.IsTrue((bool) containsValueResponse.Result);

            conn.Close();
            initiator.Stop();
        }

        [Test]
        public void TestRemoveRequest()
        {
            TcpInitiator initiator = GetInitiator();
            IConnection conn = initiator.EnsureConnection();
            IChannel namedCacheChannel = GetNamedCacheChannel(conn);
            string key = "testRemoveKey";
            string value = "testRemoveValue";

            ClearRequest clearRequest =
                    (ClearRequest) namedCacheChannel.MessageFactory.CreateMessage(ClearRequest.TYPE_ID);
            IResponse clearResponse = namedCacheChannel.Send(clearRequest).WaitForResponse(-1);
            Assert.IsFalse(clearResponse.IsFailure);

            PutRequest putRequest =
                    (PutRequest) namedCacheChannel.MessageFactory.CreateMessage(PutRequest.TYPE_ID);
            putRequest.Key = convToBinary.Convert(key);
            putRequest.Value = convToBinary.Convert(value);
            putRequest.IsReturnRequired = true;
            IResponse putResponse = namedCacheChannel.Send(putRequest).WaitForResponse(-1);
            Assert.IsFalse(putResponse.IsFailure);
            Assert.IsNull(convFromBinary.Convert(putResponse.Result));

            ContainsKeyRequest containsKeyRequest =
                (ContainsKeyRequest) namedCacheChannel.MessageFactory.CreateMessage(ContainsKeyRequest.TYPE_ID);
            containsKeyRequest.Key = convToBinary.Convert(key);
            IResponse containsKeyResponse = namedCacheChannel.Send(containsKeyRequest).WaitForResponse(-1);
            Assert.IsFalse(containsKeyResponse.IsFailure);
            Assert.IsTrue((bool) containsKeyResponse.Result);

            RemoveRequest removeRequest = (RemoveRequest) namedCacheChannel.MessageFactory.CreateMessage(RemoveRequest.TYPE_ID);
            removeRequest.Key = convToBinary.Convert(key);
            removeRequest.IsReturnRequired = true;
            IResponse removeResponse = namedCacheChannel.Send(removeRequest).WaitForResponse(-1);

            Stream stream = new MemoryStream();
            Codec codec = new Codec();
            codec.Encode(namedCacheChannel, removeRequest, new DataWriter(stream));
            stream.Position = 0;
            RemoveRequest result = (RemoveRequest) codec.Decode(namedCacheChannel, new DataReader(stream));
            Assert.AreEqual(result.IsReturnRequired, removeRequest.IsReturnRequired);
            Assert.AreEqual(result.Key, removeRequest.Key);

            Assert.AreEqual(removeRequest.TypeId, RemoveRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), removeResponse);
            Assert.AreEqual(removeResponse.TypeId, NamedCacheResponse.TYPE_ID);
            Assert.AreEqual(removeRequest.Id, removeResponse.RequestId);
            Assert.IsFalse(removeResponse.IsFailure);
            if (removeRequest.IsReturnRequired)
            {
                Assert.AreEqual(removeResponse.Result, convToBinary.Convert(value));
            }

            containsKeyRequest = (ContainsKeyRequest) namedCacheChannel.MessageFactory.CreateMessage(ContainsKeyRequest.TYPE_ID);
            containsKeyRequest.Key = convToBinary.Convert(key);
            containsKeyResponse = namedCacheChannel.Send(containsKeyRequest).WaitForResponse(-1);
            Assert.IsFalse(containsKeyResponse.IsFailure);
            Assert.IsFalse((bool) containsKeyResponse.Result);

            conn.Close();
            initiator.Stop();
        }

        [Test]
        public void TestContainsAllAndRemoveAllRequest()
        {
            TcpInitiator initiator = GetInitiator();
            IConnection conn = initiator.EnsureConnection();
            IChannel namedCacheChannel = GetNamedCacheChannel(conn);

            string[] keys = { "Ana Cikic", "Goran Milosavljevic", "Ivan Cikic" };
            string[] values = { "10.0.0.120", "10.0.0.180", "10.0.0.125" };
            IDictionary addresses = new Hashtable();
            addresses.Add(convToBinary.Convert(keys[0]), convToBinary.Convert(values[0]));
            addresses.Add(convToBinary.Convert(keys[1]), convToBinary.Convert(values[1]));
            addresses.Add(convToBinary.Convert(keys[2]), convToBinary.Convert(values[2]));
            ArrayList list = new ArrayList();
            list.Add(convToBinary.Convert(keys[0]));
            list.Add(convToBinary.Convert(keys[2]));

            ClearRequest clearRequest =
                    (ClearRequest) namedCacheChannel.MessageFactory.CreateMessage(ClearRequest.TYPE_ID);
            IResponse clearResponse = namedCacheChannel.Send(clearRequest).WaitForResponse(-1);
            Assert.IsFalse(clearResponse.IsFailure);

            PutAllRequest putAllRequest =
                    (PutAllRequest) namedCacheChannel.MessageFactory.CreateMessage(PutAllRequest.TYPE_ID);
            putAllRequest.Map = addresses;
            IResponse putAllResponse = namedCacheChannel.Send(putAllRequest).WaitForResponse(-1);
            Assert.IsFalse(putAllResponse.IsFailure);

            ContainsAllRequest containsAllRequest =
                (ContainsAllRequest) namedCacheChannel.MessageFactory.CreateMessage(ContainsAllRequest.TYPE_ID);
            list.Add(convToBinary.Convert("dummy"));
            containsAllRequest.Keys = list;
            IResponse containsAllResponse = namedCacheChannel.Send(containsAllRequest).WaitForResponse(-1);

            Assert.AreEqual(containsAllRequest.TypeId, ContainsAllRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), containsAllResponse);
            Assert.AreEqual(containsAllResponse.TypeId, NamedCacheResponse.TYPE_ID);
            Assert.AreEqual(containsAllRequest.Id, containsAllResponse.RequestId);
            Assert.IsFalse(containsAllResponse.IsFailure);
            Assert.IsInstanceOf(typeof(bool), containsAllResponse.Result);
            Assert.IsFalse((bool) containsAllResponse.Result);

            containsAllRequest = (ContainsAllRequest) namedCacheChannel.MessageFactory.CreateMessage(ContainsAllRequest.TYPE_ID);
            list.Remove(convToBinary.Convert("dummy"));
            containsAllRequest.Keys = list;
            containsAllResponse = namedCacheChannel.Send(containsAllRequest).WaitForResponse(-1);

            Assert.IsFalse(containsAllResponse.IsFailure);
            Assert.IsTrue((bool) containsAllResponse.Result);

            RemoveAllRequest removeAllRequest = (RemoveAllRequest) namedCacheChannel.MessageFactory.CreateMessage(RemoveAllRequest.TYPE_ID);
            removeAllRequest.Keys = list;
            IResponse removeAllResponse = namedCacheChannel.Send(removeAllRequest).WaitForResponse(-1);

            Assert.AreEqual(removeAllRequest.TypeId, RemoveAllRequest.TYPE_ID);
            Assert.IsInstanceOf(typeof(NamedCacheResponse), removeAllResponse);
            Assert.AreEqual(removeAllResponse.TypeId, NamedCacheResponse.TYPE_ID);
            Assert.AreEqual(removeAllRequest.Id, removeAllResponse.RequestId);
            Assert.IsFalse(removeAllResponse.IsFailure);
            Assert.IsInstanceOf(typeof(bool), removeAllResponse.Result);
            Assert.IsTrue((bool) removeAllResponse.Result);

            conn.Close();
            initiator.Stop();
        }
    }
}
