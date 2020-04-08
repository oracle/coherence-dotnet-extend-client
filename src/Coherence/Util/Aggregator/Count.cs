/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    ///  Calculates a number of values in an entries collection.
    /// </summary>
    /// <author>Gene Gleyzer  2005.09.05</author>
    /// <author>Ivan Cikic  2006.10.24</author>
    /// <since>Coherence 3.1</since>
    public class Count : IParallelAwareAggregator, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Count()
        {}

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
        public virtual object Aggregate(ICollection entries)
        {
            int count = 0;
            foreach(IInvocableCacheEntry entry in entries)
            {
                if (entry.IsPresent)
                {
                    count++;
                }
            }
            return count;
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
        public virtual IEntryAggregator ParallelAggregator
        {
            get { return this; }
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
        public virtual object AggregateResults(ICollection results)
        {
            int count = 0;
            foreach(int c in results)
            {
                count += c;
            }
            return count;
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void ReadExternal(IPofReader reader)
        {}

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteExternal(IPofWriter writer)
        {}

        #endregion
    }
}