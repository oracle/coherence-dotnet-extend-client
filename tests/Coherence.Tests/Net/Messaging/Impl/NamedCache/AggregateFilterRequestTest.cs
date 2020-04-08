/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using NUnit.Framework;

using Tangosol.Util.Aggregator;
using Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    [TestFixture]
    public class AggregateFilterRequestTest : BaseNamedCacheTest
    {
        public AggregateFilterRequest CreateRequest()
        {
            TcpInitiator initiator = GetInitiator();
            IConnection conn = initiator.EnsureConnection();
            IChannel namedCache = GetNamedCacheChannel(conn);

            try
            {
                return (AggregateFilterRequest)namedCache.MessageFactory.CreateMessage(AggregateFilterRequest.TYPE_ID);
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

            request.Aggregator = new DoubleMax();

            Assert.That(request.RequestTimeoutMillis, Is.EqualTo((long)PriorityTaskTimeout.Default));
        }

        [Test]
        public void ShouldReturnProcessorRequestTimeoutWhenProcessorIsIPriorityTask()
        {
            var request = CreateRequest();

            request.Aggregator = new PriorityAggregator()
            {
                RequestTimeoutMillis = 1234
            };

            Assert.That(request.RequestTimeoutMillis, Is.EqualTo(1234));
        }

        [Test]
        public void ShouldReturnDefaultExecutionTimeoutWhenProcessorIsNotPriorityTask()
        {
            var request = CreateRequest();

            request.Aggregator = new DoubleMax();

            Assert.That(request.ExecutionTimeoutMillis, Is.EqualTo((long)PriorityTaskTimeout.Default));
        }

        [Test]
        public void ShouldReturnProcessorExecutionTimeoutWhenProcessorIsIPriorityTask()
        {
            var request = CreateRequest();

            request.Aggregator = new PriorityAggregator()
            {
                ExecutionTimeoutMillis = 1234
            };

            Assert.That(request.ExecutionTimeoutMillis, Is.EqualTo(1234));
        }

        [Test]
        public void ShouldReturnDefaultSchedulingPriorityWhenProcessorIsNotPriorityTask()
        {
            var request = CreateRequest();

            request.Aggregator = new DoubleMax();

            Assert.That(request.SchedulingPriority, Is.EqualTo(PriorityTaskScheduling.Standard));
        }

        [Test]
        public void ShouldReturnProcessorSchedulingPriorityWhenProcessorIsIPriorityTask()
        {
            var request = CreateRequest();

            request.Aggregator = new PriorityAggregator()
            {
                SchedulingPriority = PriorityTaskScheduling.First
            };

            Assert.That(request.SchedulingPriority, Is.EqualTo(PriorityTaskScheduling.First));
        }

        [Test]
        public void ShouldNotCallProcessorRunCanceledWhenProcessorIsNotPriorityTask()
        {
            var request = CreateRequest();

            request.Aggregator = new DoubleMax();

            request.RunCanceled(false);
        }

        [Test]
        public void ShouldCallProcessorRunCanceledWhenProcessorIsIPriorityTask()
        {
            var request = CreateRequest();

            PriorityAggregatorStub priorityAggregator = new PriorityAggregatorStub();

            request.Aggregator = priorityAggregator;

            request.RunCanceled(true);
            Assert.That(priorityAggregator.fRunCanceledCalled, Is.EqualTo(true));
        }
    }
}
