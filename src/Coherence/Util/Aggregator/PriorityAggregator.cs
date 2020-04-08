/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    /// PriorityAggregator is used to explicitly control the scheduling
    /// priority and timeouts for execution of <see cref="IEntryAggregator"/>
    /// -based methods.
    /// </summary>
    /// <remarks>
    /// For example, let's assume that there is an <i>Orders</i> cache that
    /// belongs to a partitioned cache service configured with a
    /// <i>request-timeout</i> and <i>task-timeout</i> of 5 seconds. Also
    /// assume that we are willing to wait longer for a particular
    /// aggregation request that scans the entire cache. Then we could
    /// override the default timeout values by using the PriorityAggregator
    /// as follows:
    /// <code>
    /// DoubleAverage      aggrStandard = new DoubleAverage("Price");
    /// PriorityAggregator aggrPriority = new PriorityAggregator(aggrStandard);
    /// aggrPriority.ExecutionTimeoutMillis = PriorityTaskTimeout.None;
    /// aggrPriority.RequestTimeoutMillis   = PriorityTaskTimeout.None;
    /// cacheOrders.Aggregate(null, aggrPriority);
    /// </code>
    /// This is an advanced feature which should be used judiciously.
    /// </remarks>
    /// <author>Gene Gleyzer  2007.03.20</author>
    /// <since>Coherence 3.3</since>
    public class PriorityAggregator : AbstractPriorityTask, IParallelAwareAggregator, IPortableObject {

        #region Properties

        /// <summary>
        /// Obtain the underlying aggregator.
        /// </summary>
        /// <value>
        /// The aggregator wrapped by this PriorityAggregator.
        /// </value>
        public IParallelAwareAggregator Aggregator
        {
            get { return m_aggregator; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PriorityAggregator()
        {}

        /// <summary>
        /// Construct a PriorityAggregator.
        /// </summary>
        /// <param name="aggregator">
        /// The <see cref="IParallelAwareAggregator"/> wrapped by this
        /// PriorityAggregator.
        /// </param>
        public PriorityAggregator(IParallelAwareAggregator aggregator)
        {
            m_aggregator = aggregator;
        }

        #endregion

        #region IParallelAwareAggregator implementation

        /// <summary>
        /// Get an aggregator that can take the place of this aggregator in
        /// situations in which the <see cref="IInvocableCache"/> can
        /// aggregate in parallel.
        /// </summary>
        /// <value>
        /// The aggregator that will be run in parallel.
        /// </value>
        public IEntryAggregator ParallelAggregator
        {
            get { return m_aggregator.ParallelAggregator; }
        }

        /// <summary>
        /// Aggregate the results of the parallel aggregations.
        /// </summary>
        /// <param name="results">
        /// Results to aggregate.
        /// </param>
        /// <returns>
        /// The aggregation of the parallel aggregation results.
        /// </returns>
        public object AggregateResults(ICollection results)
        {
            return m_aggregator.AggregateResults(results);
        }

        #endregion

        #region IEntryAggregator implementation

        /// <summary>
        /// Process a set of <see cref="IInvocableCacheEntry"/> objects
        /// in order to produce an aggregated result.
        /// </summary>
        /// <param name="entries">
        /// A collection of read-only <b>IInvocableCacheEntry</b>
        /// objects to aggregate.
        /// </param>
        /// <returns>
        /// The aggregated result from processing the entries.
        /// </returns>
        public object Aggregate(ICollection entries)
        {
            return m_aggregator.Aggregate(entries);
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <remarks>
        /// This implementation reserves property index 10.
        /// </remarks>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            m_aggregator = (IParallelAwareAggregator) reader.ReadObject(10);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <remarks>
        /// This implementation reserves property index 10.
        /// </remarks>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteObject(10, m_aggregator);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Return a human-readable description for this
        /// <b>PriorityAggregator</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>PriorityAggregator</b>.
        /// </returns>
        public override string ToString()
        {
            return "PriorityAggregator (" + m_aggregator + ")";
        }

        #endregion

        #region Data members

        /// <summary>
        /// The wrapped IParallelAwareAggregator.
        /// </summary>
        private IParallelAwareAggregator m_aggregator;

        #endregion
    }
}