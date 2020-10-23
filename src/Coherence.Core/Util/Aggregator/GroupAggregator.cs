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

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util.Collections;
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    /// The GroupAggregator provides an ability to split a subset of entries
    /// in an <see cref="IInvocableCache"/> into a collection of
    /// non-intersecting subsets and then aggregate them separately and
    /// independently.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The splitting (grouping) is performed using the results of the
    /// underlying <see cref="IValueExtractor"/> in such a way that two
    /// entries will belong to the same group if and only if the result of
    /// the corresponding <see cref="IValueExtractor.Extract"/> call produces
    /// the same value or tuple (list of values). After the entries are split
    /// into the groups, the underlying aggregator is applied separately to
    /// each group. The result of the aggregation by the GroupAggregator is a
    /// dictionary that has distinct values (or tuples) as keys and results
    /// of the individual aggregation as values. Additionally, those results
    /// could be further reduced using an optional <see cref="IFilter"/>
    /// object.</p>
    /// <p>
    /// Informally speaking, this aggregator is analogous to the SQL
    /// "group by" and "having" clauses. Note that the "having"
    /// <b>IFilter</b> is applied independently on each server against the
    /// partial aggregation results; this generally implies that data
    /// affinity is required to ensure that all required data used to
    /// generate a given result exists within a single cache partition. In
    /// other words, the "group by" predicate should not span multiple
    /// partitions if the "having" clause is used.</p>
    /// <p>
    /// The GroupAggregator is somewhat similar to the
    /// <see cref="DistinctValues"/> aggregator, which returns back a list of
    /// distinct values (tuples) without performing any additional
    /// aggregation work.</p>
    /// <p>
    /// <b>Unlike many other concrete <see cref="IEntryAggregator"/>
    /// implementations that are constructed directly, instances of
    /// GroupAggregator should only be created using one of the factory
    /// methods:</b></p>
    /// <see cref="CreateInstance(IValueExtractor, IEntryAggregator)"/>
    /// <see cref="CreateInstance(IValueExtractor, IEntryAggregator, IFilter)"/>
    /// <see cref="CreateInstance(string, IEntryAggregator)"/>
    /// <see cref="CreateInstance(string, IEntryAggregator, IFilter)"/>
    /// </remarks>
    /// <author>Gene Gleyzer  2006.02.15</author>
    /// <author>Ana Cikic  2006.10.23</author>
    /// <since>Coherence 3.2</since>
    public class GroupAggregator : IEntryAggregator, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Obtain the underlying <see cref="IValueExtractor"/>.
        /// </summary>
        /// <value>
        /// The underlying <b>IValueExtractor</b>.
        /// </value>
        public virtual IValueExtractor Extractor
        {
            get { return m_extractor; }
        }

        /// <summary>
        /// Obtain the underlying <see cref="IEntryAggregator"/>.
        /// </summary>
        /// <value>
        /// The underlying <b>IEntryAggregator</b>.
        /// </value>
        public virtual IEntryAggregator Aggregator
        {
            get { return m_aggregator; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GroupAggregator()
        {}

        /// <summary>
        /// Construct a GroupAggregator based on a specified
        /// <see cref="IValueExtractor"/> and underlying
        /// <see cref="IEntryAggregator"/>.
        /// </summary>
        /// <param name="extractor">
        /// An <b>IValueExtractor</b> object that is used to split
        /// <see cref="IInvocableCache"/> entries into
        /// non-intersecting subsets; may not be <c>null</c>.
        /// </param>
        /// <param name="aggregator">
        /// An <b>IEntryAggregator</b> object; may not be <c>null</c>.
        /// </param>
        /// <param name="filter">
        /// An optional <see cref="IFilter"/> object used to filter out
        /// results of individual group aggregation results.
        /// </param>
        protected GroupAggregator(IValueExtractor extractor, IEntryAggregator aggregator, IFilter filter)
        {
            Debug.Assert(extractor != null && aggregator != null);

            m_extractor  = extractor;
            m_aggregator = aggregator;
            m_filter     = filter;
        }

        #endregion

        #region IEntryAggregator implementation

        /// <summary>
        /// Process a collection of <see cref="IInvocableCacheEntry"/>
        /// objects using the underlying extractor to split the entries
        /// into non-intersecting (distinct) groups and then apply the
        /// underlying aggregator separately to each group.
        /// </summary>
        /// <param name="entries">
        /// A collection of read-only <b>IInvocableCacheEntry</b>
        /// objects to aggregate.
        /// </param>
        /// <returns>
        /// A dictionary that has the unique tuples as keys and results of
        /// the corresponding subset aggregation as values.
        /// </returns>
        public virtual object Aggregate(ICollection entries)
        {
            IValueExtractor  extractor  = m_extractor;
            IEntryAggregator aggregator = m_aggregator;
            IFilter          filter     = m_filter;

            // create non-intersecting groups of entry sets
            IDictionary result = new HashDictionary();
            foreach (IInvocableCacheEntry entry in entries)
            {
                if (entry.IsPresent)
                {
                    // extract a distinct value (or a tuple)
                    object distinct = entry.Extract(extractor);

                    // add the entry to the corresponding group
                    ICollection group = (ICollection) result[distinct];
                    if (group == null)
                    {
                        result.Add(distinct, group = new ArrayList());
                    }
                    CollectionUtils.Add(group, entry);
                }
            }

            // run the aggregation
            IDictionary newResult = new HashDictionary(result);
            foreach (DictionaryEntry entry in result)
            {
                ICollection group = (ICollection) entry.Value;
                object      res   = aggregator.Aggregate(group);
                if (filter == null || filter.Evaluate(res))
                {
                    newResult[entry.Key] = res;
                }
                else
                {
                    newResult.Remove(entry.Key);
                }
            }

            return newResult;
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
            m_extractor  = (IValueExtractor) reader.ReadObject(0);
            m_aggregator = (IEntryAggregator) reader.ReadObject(1);
            m_filter     = (IFilter) reader.ReadObject(2);
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
            writer.WriteObject(0, m_extractor);
            writer.WriteObject(1, m_aggregator);
            writer.WriteObject(2, m_filter);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the GroupAggregator with another object to determine
        /// equality.
        /// </summary>
        /// <param name="o">
        /// The <b>GroupAggregator</b> to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if this GroupAggregator and the passed object are
        /// equivalent.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is GroupAggregator)
            {
                GroupAggregator that = (GroupAggregator) o;
                return Equals(m_extractor, that.m_extractor) && Equals(m_aggregator, that.m_aggregator);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the GroupAggregator object according
        /// to the general <b>object.GetHashCode()</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this GroupAggregator object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_extractor.GetHashCode() + m_aggregator.GetHashCode();
        }

        /// <summary>
        /// Return a human-readable description for this GroupAggregator.
        /// </summary>
        /// <returns>
        /// A string description of the GroupAggregator.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + '(' + m_extractor + ", " + m_aggregator +
              (m_filter == null ? "" : ", " + m_filter) + ')';
        }

        #endregion

        #region Factory methods

        /// <summary>
        /// Create an instance of GroupAggregator based on a specified member
        /// name(s) and an <see cref="IEntryAggregator"/>.
        /// </summary>
        /// <remarks>
        /// If the specified underlying aggregator is an instance of
        /// <see cref="IParallelAwareAggregator"/>, then a parallel-aware
        /// instance of the GroupAggregator will be created. Otherwise, the
        /// resulting GroupAggregator will not be parallel-aware and could be
        /// ill-suited for aggregations run against large partitioned caches.
        /// </remarks>
        /// <param name="member">
        /// A member name or a comma-delimited sequence of names that results
        /// in a <see cref="ReflectionExtractor"/> or a
        /// <see cref="MultiExtractor"/> that will be used to split
        /// <see cref="IInvocableCache"/> entries into distinct groups.
        /// </param>
        /// <param name="aggregator">
        /// An underlying <b>IEntryAggregator</b>.
        /// </param>
        /// <returns>
        /// An instance of GroupAggregator based on a specified member
        /// name(s) and an <see cref="IEntryAggregator"/>.
        /// </returns>
        public static GroupAggregator CreateInstance(string member, IEntryAggregator aggregator)
        {
            return CreateInstance(member, aggregator, null);
        }

        /// <summary>
        /// Create an instance of GroupAggregator based on a specified member
        /// name(s), an <see cref="IEntryAggregator"/> and a result
        /// evaluation filter.
        /// </summary>
        /// <remarks>
        /// If the specified underlying aggregator is an instance of
        /// <see cref="IParallelAwareAggregator"/>, then a parallel-aware
        /// instance of the GroupAggregator will be created. Otherwise, the
        /// resulting GroupAggregator will not be parallel-aware and could be
        /// ill-suited for aggregations run against large partitioned caches.
        /// </remarks>
        /// <param name="member">
        /// A member name or a comma-delimited sequence of names that results
        /// in a <see cref="ReflectionExtractor"/> or a
        /// <see cref="MultiExtractor"/> that will be used to split
        /// <see cref="IInvocableCache"/> entries into distinct groups.
        /// </param>
        /// <param name="aggregator">
        /// An underlying <b>IEntryAggregator</b>.
        /// </param>
        /// <param name="filter">
        /// An optional <b>IFilter</b> object that will be used to evaluate
        /// results of each individual group aggregation.
        /// </param>
        /// <returns>
        /// An instance of GroupAggregator based on a specified member
        /// name(s), an <see cref="IEntryAggregator"/> and a result
        /// evaluation filter.
        /// </returns>
        public static GroupAggregator CreateInstance(string member, IEntryAggregator aggregator, IFilter filter)
        {
            IValueExtractor extractor = member.IndexOf(',') >= 0
                                            ? new MultiExtractor(member)
                                            : member.IndexOf('.') >= 0
                                                  ? new ChainedExtractor(member)
                                                  : (IValueExtractor) new ReflectionExtractor(member);

            return CreateInstance(extractor, aggregator, filter);
        }

        /// <summary>
        /// Create an instance of GroupAggregator based on a specified
        /// extractor and an <b>IEntryAggregator</b>.
        /// </summary>
        /// <remarks>
        /// If the specified aggregator is an instance of
        /// <b>IParallelAwareAggregator</b>, then a parallel-aware instance
        /// of the GroupAggregator will be created. Otherwise, the resulting
        /// GroupAggregator will not be parallel-aware and could be
        /// ill-suited for aggregations run against large partitioned caches.
        /// </remarks>
        /// <param name="extractor">
        /// An <b>IValueExtractor</b> that will be used to split a set of
        /// <b>IInvocableDictionary</b> entries into distinct groups.
        /// </param>
        /// <param name="aggregator">
        /// An underlying <b>IEntryAggregator</b>.
        /// </param>
        /// <returns>
        /// An instance of GroupAggregator based on a specified
        /// extractor and an <b>IEntryAggregator</b>.
        /// </returns>
        public static GroupAggregator CreateInstance(IValueExtractor extractor, IEntryAggregator aggregator)
        {
            return CreateInstance(extractor, aggregator, null);
        }

        /// <summary>
        /// Create an instance of GroupAggregator based on a specified
        /// extractor and an <b>IEntryAggregator</b> and a result
        /// evaluation filter.
        /// </summary>
        /// <remarks>
        /// If the specified aggregator is an instance of
        /// <b>IParallelAwareAggregator</b>, then a parallel-aware instance
        /// of the GroupAggregator will be created. Otherwise, the resulting
        /// GroupAggregator will not be parallel-aware and could be
        /// ill-suited for aggregations run against large partitioned caches.
        /// </remarks>
        /// <param name="extractor">
        /// An <b>IValueExtractor</b> that will be used to split a set of
        /// <b>IInvocableDictionary</b> entries into distinct groups.
        /// </param>
        /// <param name="aggregator">
        /// An underlying <b>IEntryAggregator</b>.
        /// </param>
        /// <param name="filter">
        /// An optional <b>IFilter</b> object used to filter out results of
        /// individual group aggregation results.
        /// </param>
        /// <returns>
        /// An instance of GroupAggregator based on a specified
        /// extractor and an <b>IEntryAggregator</b> and a result
        /// evaluation filter.
        /// </returns>
        public static GroupAggregator CreateInstance(IValueExtractor extractor, IEntryAggregator aggregator, IFilter filter)
        {
            return new GroupAggregator(extractor, aggregator, filter);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying IValueExtractor.
        /// </summary>
        protected IValueExtractor m_extractor;

        /// <summary>
        /// The underlying IEntryAggregator.
        /// </summary>
        protected IEntryAggregator m_aggregator;

        /// <summary>
        /// The IFilter object representing the "having" clause of this
        /// "group by" aggregator.
        /// </summary>
        protected IFilter m_filter;

        #endregion

        #region Inner class: Parallel

        /// <summary>
        /// Parallel implementation of the GroupAggregator.
        /// </summary>
        [Obsolete("Obsolete as of Coherence 12.2.1")]
        public class Parallel : GroupAggregator, IParallelAwareAggregator
        {
            #region Constructors

            /// <summary>
            /// Construct a Parallel aggregator based on a specified
            /// <b>IValueExtractor</b> and underlying
            /// <b>IParallelAwareAggregator</b>.
            /// </summary>
            /// <param name="extractor">
            /// An <b>IValueExtractor</b> object; may not be <c>null</c>.
            /// </param>
            /// <param name="aggregator">
            /// An <b>IEntryAggregator</b> object; may not be <c>null</c>.
            /// </param>
            /// <param name="filter">
            /// An <b>IFilter</b> object.
            /// </param>
            protected internal Parallel(IValueExtractor extractor, IParallelAwareAggregator aggregator, IFilter filter)
                : base(extractor, aggregator, filter)
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
                    IParallelAwareAggregator aggregator = (IParallelAwareAggregator) m_aggregator;
                    return new GroupAggregator(m_extractor, aggregator.ParallelAggregator, m_filter);
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
                IParallelAwareAggregator aggregator = (IParallelAwareAggregator) m_aggregator;

                IDictionary dictionaryResult = new HashDictionary();
                foreach (IDictionary dictPart in results)
                {
                    // partial aggregation results are maps with distinct values
                    // as keys and partial aggregation results as values
                    foreach (DictionaryEntry entry in dictPart)
                    {
                        object distinct = entry.Key;
                        object result   = entry.Value;

                        // collect all the aggregation results per group
                        ICollection group = (ICollection) dictionaryResult[distinct];
                        if (group == null)
                        {
                            dictionaryResult.Add(distinct, group = new ArrayList());
                        }
                        CollectionUtils.Add(group, result);
                    }
                }

                IDictionary newResult = new HashDictionary(dictionaryResult);
                if (dictionaryResult.Count == 0)
                {
                    // we need to call "AggregateResults" on the underlying
                    // aggregator to fulfill our contract, even though any result
                    // will be discarded
                    aggregator.AggregateResults(NullImplementation.GetCollection());
                }
                else
                {
                    IFilter   filter       = m_filter;
                    foreach (DictionaryEntry entry in dictionaryResult)
                    {
                        ICollection group  = (ICollection) entry.Value;
                        object      result = aggregator.AggregateResults(group);
                        if (filter == null || filter.Evaluate(result))
                        {
                            newResult[entry.Key] = result;
                        }
                        else
                        {
                            newResult.Remove(entry.Key);
                        }
                    }
                }
                return newResult;

            }

            #endregion
        }

        #endregion
    }
}