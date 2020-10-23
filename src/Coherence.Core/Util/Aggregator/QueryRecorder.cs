/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    /// This parallel aggregator used to produce a {@link com.tangosol.util.QueryRecord}
    /// object that contains an estimated or actual cost of the query execution
    /// for a given filter.
    ///
    /// For example, the following code will return a QueryPlan, containing the
    /// estimated query cost and corresponding execution steps.
    /// </summary>
    /// <author>tb 2011.05.26</author>
    /// <since>Coherence 3.7.1</since>
    public class QueryRecorder : IParallelAwareAggregator, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor (necessary for IPortableObject interface).
        /// </summary>
        public QueryRecorder()
        { }

        /// <summary>
        /// Construct a QueryRecorder.
        /// </summary>
        /// <param name="type">
        /// The type for this aggregator.
        /// </param>
        public QueryRecorder(RecordType type)
        {
            m_type = type;
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Get the record type for this query recorder.
        /// </summary>
        /// <returns>
        /// The record type enum.
        /// </returns>
        public RecordType Type
        {
            get { return m_type; }
        }
        
        #endregion
        
        #region IEntryAggregator implementation

        /// <summary>
        /// Process a set of <see cref="IInvocableCacheEntry"/> objects
        /// in order to produce an aggregated result.
        /// </summary>
        /// <param name="colEntries">
        /// A collection of read-only <b>IInvocableCacheEntry</b>
        /// objects to aggregate.
        /// </param>
        /// <returns>
        /// The aggregated result from processing the entries.
        /// </returns>
        public virtual object Aggregate(ICollection colEntries)
        {
            throw new NotSupportedException(
                "QueryRecorder cannot be used by this service.");
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
        /// <param name="colResults">
        /// Results to aggregate.
        /// </param>
        /// <returns>
        /// The aggregation of the parallel aggregation results.
        /// </returns>
        public virtual object AggregateResults(ICollection colResults)
        {
            return new SimpleQueryRecord(m_type, colResults);
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
        {
            m_type = (QueryRecorder.RecordType) reader.ReadInt32(0);
        }

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
        {
            writer.WriteInt32(0, (int) m_type);
        }

        #endregion

        #region Constants

        /// <summary>
        /// RecordType enum specifies whether the QueryRecorder should be
        /// used to produce a QueryRecord object that contains an estimated 
        /// or an actual cost of the query execution.
        /// </summary>
        public enum RecordType
        {
            /// <summary>
            /// Produce a QueryRecord object that contains an estimated cost 
            /// of the query execution.
            /// </summary>
            Explain = 0,

            /// <summary>
            /// Produce a QueryRecord object that contains the actual cost of 
            /// the query execution.
            /// </summary>
            Trace = 1,
        }

        #endregion

        #region Data members

        /// <summary>
        /// This aggregator record type.
        /// </summary>
        private RecordType m_type;

        #endregion
    }
}
