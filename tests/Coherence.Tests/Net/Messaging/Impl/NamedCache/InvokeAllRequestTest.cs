/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using NUnit.Framework;

using Tangosol.Internal.Util.Processor;
using Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator;
using Tangosol.Util.Processor;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    [TestFixture]
    public class InvokeAllRequestTest : BaseNamedCacheTest
    {
        public InvokeAllRequest CreateRequest()
        {
            TcpInitiator initiator = GetInitiator();
            IConnection conn = initiator.EnsureConnection();
            IChannel namedCache = GetNamedCacheChannel(conn);

            try
            {
                return (InvokeAllRequest)namedCache.MessageFactory.CreateMessage(InvokeAllRequest.TYPE_ID);
            }
            finally
            {
                conn.Close();
                initiator.Stop();
            }
        }

        [Test]
        public void ShouldReturnDefaultRequestTimeoutWhenProcessorIsNotPriorityTask()
        {
            var request = CreateRequest();

            request.Processor = new CacheProcessors.NullProcessor();

            Assert.That(request.RequestTimeoutMillis, Is.EqualTo((long)PriorityTaskTimeout.Default));
        }

        [Test]
        public void ShouldReturnProcessorRequestTimeoutWhenProcessorIsIPriorityTask()
        {
            var request = CreateRequest();

            request.Processor = new PriorityProcessor()
            {
                RequestTimeoutMillis = 1234
            };

            Assert.That(request.RequestTimeoutMillis, Is.EqualTo(1234));
        }

        [Test]
        public void ShouldReturnDefaultExecutionTimeoutWhenProcessorIsNotPriorityTask()
        {
            var request = CreateRequest();

            request.Processor = new CacheProcessors.NullProcessor();

            Assert.That(request.ExecutionTimeoutMillis, Is.EqualTo((long)PriorityTaskTimeout.Default));
        }

        [Test]
        public void ShouldReturnProcessorExecutionTimeoutWhenProcessorIsIPriorityTask()
        {
            var request = CreateRequest();

            request.Processor = new PriorityProcessor()
            {
                ExecutionTimeoutMillis = 1234
            };

            Assert.That(request.ExecutionTimeoutMillis, Is.EqualTo(1234));
        }

        [Test]
        public void ShouldReturnDefaultSchedulingPriorityWhenProcessorIsNotPriorityTask()
        {
            var request = CreateRequest();

            request.Processor = new CacheProcessors.NullProcessor();

            Assert.That(request.SchedulingPriority, Is.EqualTo(PriorityTaskScheduling.Standard));
        }

        [Test]
        public void ShouldReturnProcessorSchedulingPriorityWhenProcessorIsIPriorityTask()
        {
            var request = CreateRequest();

            request.Processor = new PriorityProcessor()
            {
                SchedulingPriority = PriorityTaskScheduling.First
            };

            Assert.That(request.SchedulingPriority, Is.EqualTo(PriorityTaskScheduling.First));
        }

        [Test]
        public void ShouldNotCallProcessorRunCanceledWhenProcessorIsNotPriorityTask()
        {
            var request = CreateRequest();

            request.Processor = new CacheProcessors.NullProcessor();

            request.RunCanceled(false);
        }

        [Test]
        public void ShouldCallProcessorRunCanceledWhenProcessorIsIPriorityTask()
        {
            var request = CreateRequest();

            PriorityProcessorStub priorityProcessor = new PriorityProcessorStub();

            request.Processor = priorityProcessor;

            request.RunCanceled(true);
            Assert.That(priorityProcessor.fRunCanceledCalled, Is.EqualTo(true));
        }
    }
}
