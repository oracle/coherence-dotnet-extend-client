/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    /// CompositeAggregator provides an ability to execute a collection of
    /// aggregators against the same subset of the entries in an
    /// <see cref="IInvocableCache"/>, resulting in a list of
    /// corresponding aggregation results.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The size of the returned list will always be equal to the length of
    /// the aggregators' array.</p>
    /// <p>
    /// Note: Unlike many other concrete <see cref="IEntryAggregator"/>
    /// implementations that are constructed directly, instances of
    /// CompositeAggregator should only be created using the factory method
    /// <see cref="CreateInstance"/>.</p>
    /// </remarks>
    /// <author>Gene Gleyzer  2006.02.08</author>
    /// <author>Ana Cikic  2006.10.23</author>
    /// <since>Coherence 3.2</since>
    public class CompositeAggregator : IEntryAggregator, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CompositeAggregator()
        {}

        /// <summary>
        /// Construct a CompositeAggregator based on a specified
        /// <see cref="IEntryAggregator"/> array.
        /// </summary>
        /// <param name="aggregators">
        /// An array of <b>IEntryAggregator</b> objects; may not be
        /// <c>null</c>.
        /// </param>
        protected CompositeAggregator(IEntryAggregator[] aggregators)
        {
            Debug.Assert(aggregators != null);
            m_aggregators = aggregators;
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
            m_aggregators = (IParallelAwareAggregator[]) reader.ReadArray(0, EMPTY_AGGREGATOR_ARRAY);
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
            writer.WriteArray(0, m_aggregators);
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
        public virtual object Aggregate(ICollection entries)
        {
            IEntryAggregator[] aggregators = m_aggregators;

            int      aggregatorsCount = aggregators.Length;
            object[] results          = new object[aggregatorsCount];
            for (int i = 0; i < aggregatorsCount; i++)
            {
                results[i] = aggregators[i].Aggregate(entries);
            }
            return results;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the CompositeAggregator with another object to determine
        /// equality.
        /// </summary>
        /// <param name="o">
        /// The <b>CompositeAggregator</b> to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if this CompositeAggregator and the passed object are
        /// equivalent.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is CompositeAggregator)
            {
                CompositeAggregator that = (CompositeAggregator) o;
                return CollectionUtils.EqualsDeep(m_aggregators, that.m_aggregators);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the CompositeAggregator object
        /// according to the general <b>object.GetHashCode()</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this object.
        /// </returns>
        public override int GetHashCode()
        {
            IEntryAggregator[] aggregators = m_aggregators;

            int hash = 0;
            for (int i = 0, c = aggregators.Length; i < c; i++)
            {
                hash += aggregators[i].GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Return a human-readable description for this CompositeAggregator.
        /// </summary>
        /// <returns>
        /// A string description of the CompositeAggregator.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetType().Name).Append('(');

            IEntryAggregator[] aggregators = m_aggregators;
            for (int i = 0, c = aggregators.Length; i < c; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(aggregators[i]);
            }
            sb.Append(')');

            return sb.ToString();
        }

        #endregion

        #region Factory methods

        /// <summary>
        /// Create an instance of CompositeAggregator based on a specified
        /// <see cref="IEntryAggregator"/> array.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If all the aggregators in the specified array are instances of
        /// <see cref="IParallelAwareAggregator"/>, then a parallel-aware
        /// instance of the CompositeAggregator will be created.</p>
        /// <p>
        /// If at least one of the specified aggregator is not
        /// parallel-aware, then the resulting CompositeAggregator will not
        /// be parallel-aware and could be ill-suited for aggregations run
        /// against large partitioned caches.</p>
        /// </remarks>
        /// <param name="aggregators">
        /// An array of <b>IEntryAggregator</b> objects; must contain not
        /// less than two aggregators.
        /// </param>
        /// <returns>
        /// CompositeAggregator instance.
        /// </returns>
        public static CompositeAggregator CreateInstance(IEntryAggregator[] aggregators)
        {
            int aggregatorsCount = aggregators == null ? 0 : aggregators.Length;
            if (aggregatorsCount < 2)
            {
                throw new ArgumentException("Invalid array size: "
                    + aggregatorsCount);
            }

            return new CompositeAggregator(aggregators);
        }

        #endregion

        #region Inner class: Parallel

        /// <summary>
        /// Parallel implementation of the CompositeAggregator.
        /// </summary>
        [Obsolete("Obsolete as of Coherence 12.2.1")]
        public class Parallel : CompositeAggregator, IParallelAwareAggregator
        {
            #region Constructors

            /// <summary>
            /// Construct a parallel CompositeAggregator based on a specified
            /// <see cref="IEntryAggregator"/> array.
            /// </summary>
            /// <param name="aggregators">
            /// An array of <see cref="IParallelAwareAggregator"/> objects;
            /// may not be <c>null</c>.
            /// </param>
            protected internal Parallel(IParallelAwareAggregator[] aggregators) : base(aggregators)
            {}

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
                get
                {
                    IParallelAwareAggregator[] parallels = (IParallelAwareAggregator[]) m_aggregators;

                    int                aggregatorsCount = parallels.Length;
                    IEntryAggregator[] aggregators      = new IEntryAggregator[aggregatorsCount];

                    for (int i = 0; i < aggregatorsCount; i++)
                    {
                        aggregators[i] = parallels[i].ParallelAggregator;
                    }

                    return new CompositeAggregator(aggregators);
                }
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
                IParallelAwareAggregator[] parallels = (IParallelAwareAggregator[]) m_aggregators;

                int aggregatorsCount = parallels.Length;
                int resultsCount     = results.Count;

                // the collection of partial results must be a collection of lists
                // (each list of a size of the aggregator array);
                // first we need to transpose it into an array of collections
                // and then run the final aggregation pass
                object[][] resultsArray = new object[aggregatorsCount][];
                for (int i = 0; i < aggregatorsCount; i++)
                {
                    resultsArray[i] = new object[resultsCount];
                }

                int resCount = 0;
                foreach (object resultPart in results)
                {
                    if (!(resultPart is IList))
                    {
                        throw new InvalidOperationException("Expected result type: System.Collections.IList; actual type: " +
                            resultPart.GetType().FullName);
                    }

                    IList listResultPart = (IList) resultPart;
                    if (listResultPart.Count != aggregatorsCount)
                    {
                        throw new InvalidOperationException("Expected result list size: " + aggregatorsCount +
                            "; actual size: " + listResultPart.Count);
                    }

                    for (int i = 0; i < aggregatorsCount; i++)
                    {
                        resultsArray[i][resCount] = listResultPart[i];
                    }

                    resCount++;
                }

                object[] result = new object[aggregatorsCount];
                for (int i = 0; i < aggregatorsCount; i++)
                {
                    result[i] = parallels[i].AggregateResults(resultsArray[i]);
                }
                return result;
            }

            #endregion
        }

        #endregion

        #region Constants

        /// <summary>
        /// An empty array of <see cref="IEntryAggregator"/>s.
        /// </summary>
        private static readonly IParallelAwareAggregator[] EMPTY_AGGREGATOR_ARRAY = new IParallelAwareAggregator[0];

        #endregion

        #region Data members

        /// <summary>
        /// The underlyig IEntryAggregator array.
        /// </summary>
        protected IEntryAggregator[] m_aggregators;

        #endregion
    }
}