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
using Tangosol.Util.Comparator;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    ///  TopNAggregator is a ParallelAwareAggregator that aggregates the
    ///  top <i>N</i> extracted values into an array.  The extracted values
    ///   must not be null, but do not need to be unique.
    /// </summary>
    /// <author>Robert Lee  2013.04.24</author>
    /// <since>Coherence 12.1.3</since>
    public class TopNAggregator : IParallelAwareAggregator,
                                  IPartialResultAggregator, IPortableObject
    {
        #region Properties

        /// <summary>
        /// The ValueExtractor used by this aggregator.
        /// </summary>
        protected IValueExtractor Extractor
        {
            get; set;
        }

        /// <summary>
        /// True iff this aggregator is to be used in parallel.
        /// </summary>
        protected bool IsParallel
        {
            get; set;
        }

        /// <summary>
        /// The IComparer used by this aggregator.
        /// </summary>
        protected IComparer Comparer
        {
            get; set;
        }

        /// <summary>
        /// The maximum number of results to include in the aggregation result.
        /// </summary>
        protected int Results
        {
            get; set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TopNAggregator()
        {}

        /// <summary>
        /// Construct a TopNAggregator that will aggregate the top extracted
        /// values, as determined by the specified comparator.
        /// </summary>
        /// <param name="extractor">
        /// The extractor.
        /// </param>
        /// <param name="comparer">
        /// The comparer for extracted values.
        /// </param>
        /// <param name="cResults">
        /// The maximum number of results to return.
        /// </param>
        public TopNAggregator(IValueExtractor extractor, IComparer comparer, int cResults)
        {
            Extractor = extractor;
            Results   = cResults;
            Comparer  = comparer == null ? SafeComparer.Instance : comparer;
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
            get
            {
                IsParallel = true;
                return this;
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
            return FinalizeResult((PartialResult) AggregatePartialResults(results));
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
            PartialResult resultPartial = new PartialResult(Comparer);
            IConverter    converter     = new ExtractingConverter(Extractor);

            AddToResult(ConverterCollections.GetCollection(entries, converter, converter), resultPartial);

            return IsParallel
                   ? (object) resultPartial
                   : (object) FinalizeResult(resultPartial);
        }

        #endregion

        #region IPartialResultAggregator implementation

        /// <summary>
        /// Aggregate the results of the parallel aggregations, producing a
        /// partial result logically representing the partial aggregation. The
        /// returned partial result will be further {@link
        /// ParallelAwareAggregator#aggregateResults aggregated} to produce
        /// the final result.
        /// </summary>
        /// <param name="colPartialResults">
        /// The partial results to agregate.
        /// </param>
        /// <returns>
        /// An aggregattion of the collection of partial results.
        /// </returns>
        public virtual object AggregatePartialResults(ICollection colPartialResults)
        {
            PartialResult resultMerged = new PartialResult(Comparer);

            foreach (PartialResult resultPartial in colPartialResults)
            {
                AddToResult(resultPartial.Results, resultMerged);
            }

            return resultMerged;
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Add the specified values to the result if they are within the top-N.
        /// </summary>
        /// <param name="values">The collection of values to add.</param>
        /// <param name="result">The result.</param>
        protected void AddToResult(ICollection values, PartialResult result)
        {
            int maxSize      = Results;
            int curSize      = result.Count;
            object elemFirst = null;

            foreach (object value in values)
            {
                if (value == null)
                {
                    continue;
                }

                if (curSize < maxSize)
                {
                    result.Add(value);
                }
                else
                {
                    if (elemFirst == null)
                    {
                        elemFirst = result.First;
                    }
                    if (Comparer.Compare(value, elemFirst) > 0)
                    {
                        result.Add(value);
                        result.RemoveFirst();
                        elemFirst = null;
                    }
                }

                ++curSize;
            }
        }

        /// <summary>
        /// Finalize the partial aggregation result.
        /// </summary>
        /// <param name="result">
        /// The final aggregation result.
        /// </param>
        /// <returns>
        /// The finalized partial aggregation result.
        /// </returns>
        protected object[] FinalizeResult(PartialResult result)
        {
            object[] results = CollectionUtils.ToArray(result.Results);
            Array.Reverse(results);
            return results;
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
            IsParallel = reader.ReadBoolean(0);
            Extractor  = (IValueExtractor) reader.ReadObject(1);
            Comparer   = (IComparer)       reader.ReadObject(2);
            Results    = reader.ReadInt32(3);
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
            writer.WriteBoolean(0, IsParallel);
            writer.WriteObject (1, Extractor);
            writer.WriteObject (2, Comparer);
            writer.WriteInt32  (3, Results);
        }

        #endregion

        #region Inner class: PartialResult

        /// <summary>
        /// The sorted partial result.
        /// </summary>
        /// Note the implementation difference between Java due to the lack
        /// of SortedMap/NavigableMap interfaces (and hence SortedBag).
        public class PartialResult : IPortableObject
        {
            #region Properties

            /// <summary>
            /// The IComparer used to sort elements in this partial result.
            /// </summary>
            protected IComparer Comparer
            {
                get; private set;
            }

            /// <summary>
            /// The sorted collection of partial results.
            /// </summary>
            public IList Results
            {
                get
                {
                    IList keys = BackingDictionary.GetKeyList();

                    if (keys.Count == Count)
                    {
                        // no duplicates
                        return keys;
                    }

                    // need to iterate through to add duplicates to the returned list
                    IList returnList = new ArrayList(Count);
                    for (int i = 0; i < keys.Count; ++i)
                    {
                        object key     = keys[i];
                        int    numKeys = (int) BackingDictionary[key];

                        for (int j = 0; j < numKeys; ++j)
                        {
                            returnList.Add(key);
                        }
                    }

                    return returnList;
                }
            }

            /// <summary>
            /// The SortedList used to maintain the sorted results (as keys in the SortedList).
            /// The keys are the sorted results, and the values are the number of results for
            /// the particular key (to track duplicates).
            /// </summary>
            protected SortedList BackingDictionary
            {
                get; set;
            }

            /// <summary>
            /// The number of results.
            /// </summary>
            public int Count
            {
                get; protected set;
            }

            /// <summary>
            /// The head of the list of results.
            /// </summary>
            public object First
            {
                get
                {
                    return BackingDictionary.GetKey(0);
                }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Default constructor.
            /// </summary>
            public PartialResult() : this(null)
            {}

            /// <summary>
            /// Construct a PartialResult using the specified comparer.
            /// </summary>
            /// <param name="comparer">The IComparer to use for sorting.</param>
            public PartialResult(IComparer comparer)
            {
                Comparer          = comparer == null ? SafeComparer.Instance : comparer;
                Count             = 0;
                BackingDictionary = new SortedList(Comparer);
            }

            #endregion

            /// <summary>
            /// Add a new result to the list of results.
            /// </summary>
            /// <param name="o"></param>
            public void Add(object o)
            {
                if (BackingDictionary.ContainsKey(o))
                {
                    int currentCount     = (int) BackingDictionary[o];
                    BackingDictionary[o] = currentCount + 1;
                }
                else
                {
                    BackingDictionary.Add(o, 1);
                }
                ++Count;
            }

            /// <summary>
            /// Remove the first element from the list of results.
            /// </summary>
            public void RemoveFirst()
            {
                if (BackingDictionary.Count > 0)
                {
                    object key        = BackingDictionary.GetKey(0);
                    int    firstCount = (int) BackingDictionary[key];

                    if (firstCount == 1)
                    {
                        BackingDictionary.RemoveAt(0);
                    }
                    else
                    {
                        BackingDictionary[key] = firstCount - 1;
                    }
                    --Count;
                }
            }

            #region IPortableObject implementation

            /// <summary>
            /// Restore the contents of a user type instance by reading its
            /// state using the specified <see cref="IPofReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>IPofReader</b> from which to read the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public void ReadExternal(IPofReader reader)
            {
                Comparer        = (IComparer) reader.ReadObject(0);
                ICollection col = reader.ReadCollection(1, new ArrayList());

                BackingDictionary = new SortedList(Comparer, col.Count);
                foreach (object element in col)
                {
                    Add(element);
                }
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
            public void WriteExternal(IPofWriter writer)
            {
                writer.WriteObject(    0, Comparer);
                writer.WriteCollection(1, Results);
            }

            #endregion
        }

        #endregion

        #region Inner class: ExtractingConverter

        /// <summary>
        /// A value-extracting converter.
        /// </summary>
        protected class ExtractingConverter : IConverter
        {
            #region Properties

            /// <summary>
            /// The value extractor.
            /// </summary>
            private IValueExtractor Extractor
            { get; set; }

            #endregion

            #region Constructors

            /// <summary>
            /// Create a new ExtractingConverter.
            /// </summary>
            /// <param name="extractor">The value extractor.</param>
            public ExtractingConverter(IValueExtractor extractor)
            {
                Extractor = extractor;
            }

            #endregion

            #region IConverter implementation

            /// <summary>
            /// Convert the passed object to another object.
            /// </summary>
            /// <param name="o">
            /// Object to be converted.
            /// </param>
            /// <returns>
            /// The new, converted object.
            /// </returns>
            public object Convert(object o)
            {
                return ((IInvocableCacheEntry) o).Extract(Extractor);
            }

            #endregion
        }

        #endregion
    }
}
