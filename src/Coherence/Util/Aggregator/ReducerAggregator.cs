/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

using Tangosol.Net.Cache;
using Tangosol.IO.Pof;
using Tangosol.Util;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    /// The ReducerAggregator will return a portion of value attributes based on the
    /// provided ValueExtractor, instead of returning the complete set of values.
    /// </summary>
    /// <remarks>
    /// This aggregator could be used in combination with
    /// Tangosol.Util.Extractor.MultiExtractor allowing one to collect
    /// tuples that are a subset of the attributes of each object stored in the cache.
    /// <see cref="Tangosol.Util.Extractor.MultiExtractor"/>
    /// </remarks>
    /// <author> djl  2009.03.02 </author>
    /// <author> par  2013.04.25 </author>
    /// <since>Coherence 12.1.3</since>
    public class ReducerAggregator : AbstractAggregator
    {
        #region Constructors 

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ReducerAggregator() : base()
        {}

        /// <summary>
        /// Construct a ReducerAggregator based on the specified method name.
        /// </summary>
        /// <param name="sMethod">
        /// The name of the method that is used to extract the
        /// portion of the cached value.
        /// </param>
        public ReducerAggregator(string sMethod) : base(sMethod)
        {}

        /// <summary>
        /// Construct a ReducerAggregator based on the specified extractor.
        /// </summary>
        /// <param name="extractor">
        /// The extractor that is used to extract the portion
        /// of the cached value.
        /// </param>
        public ReducerAggregator(IValueExtractor extractor) : base(extractor)
        {}

        #endregion

        #region AbstractAggregator override methods


        /// <summary>
        /// Initialize the aggregation result.
        /// </summary>
        /// <param name="isFinal">
        /// <b>true</b> is passed if the aggregation process that is being
        /// initialized must produce a final aggregation result; this will
        /// only be <b>false</b> if a parallel approach is being used and the
        /// initial (partial) aggregation process is being initialized.
        /// </param>
        protected override void Init(bool isFinal)
        {
            IDictionary map = m_map;
            if (map != null)
            {
                map.Clear();
            }
        }

        /// <summary>
        /// Incorporate one aggregatable value into the result.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="isFinal"/> parameter is <b>true</b>, the
        /// given object is a partial result (returned by an individual
        /// parallel aggregator) that should be incorporated into the final
        /// result; otherwise, the object is a value extracted from an
        /// <see cref="IInvocableCacheEntry"/>
        /// </remarks>
        /// <param name="o">
        /// The value to incorporate into the aggregated result.
        /// </param>
        /// <param name="isFinal">
        /// <b>true</b> to indicate that the given object is a partial
        /// result returned by a parallel aggregator.
        /// </param>
        protected override void Process(object o, bool isFinal)
        {
            if (o != null)
            {
                if (isFinal)
                {
                    if (o is DictionaryEntry)
                    {
                        DictionaryEntry entry = (DictionaryEntry) o;
                        EnsureMap().Add(entry.Key, Extractor.Extract(entry.Value));
                    }
                }
                else
                {
                    // should not be called with isFinal == false
                    // that would mean multiple aggregators would be running on the client,
                    // but shouldn't happen.
                    throw new InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// Obtain the result of the aggregation.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="isFinal"/> parameter is <b>true</b>, the
        /// returned object must be the final result of the aggregation;
        /// otherwise, the returned object will be treated as a partial
        /// result that should be incorporated into the final result.
        /// </remarks>
        /// <param name="isFinal">
        /// <b>true</b> to indicate that the final result of the aggregation
        /// process should be returned; this will only be <b>false</b> if a
        /// parallel approach is being used.
        /// </param>
        /// <returns>
        /// The result of the aggregation process.
        /// </returns>
        protected override object FinalizeResult(bool isFinal)
        {
            IDictionary map = m_map;
            m_map           = null;

            if (isFinal)
            {
                // return the final aggregated result
                return map == null ? NullImplementation.GetDictionary() : map;
            }
            else
            {
                // return partial aggregation data
                return map;
            }
        }

        #endregion



        #region Helpers

        /// <summary>
        /// Return a map that can be used to store reduced values, creating it if
        /// one has not already been created.
        /// </summary>
        /// <returns>
        /// A set that can be used to store distinct values.
        /// </returns>
        protected IDictionary EnsureMap()
        {
            IDictionary map = m_map;
            if (map == null)
            {
                map = m_map = new LiteDictionary();
            }
            return map;
        }
        #endregion

        #region Data members

        /// <summary>
        /// The resulting map of reduced values.
        /// </summary>
        protected IDictionary m_map;

        #endregion 
   }
}