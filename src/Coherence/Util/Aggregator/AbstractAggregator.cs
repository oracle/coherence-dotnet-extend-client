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
using Tangosol.Util;
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    /// Abstract base class implementation of <see cref="IEntryAggregator"/>
    /// that supports parallel aggregation.
    /// </summary>
    /// <remarks>
    /// For aggregators which only run within the Coherence cluster
    /// (most common case), the .NET Init, Process, FinalizeResult, Aggregate,
    /// and AggregateResults methods can be left unimplemented.
    /// </remarks>
    /// <author>Cameron Purdy, Gene Gleyzer, Jason Howes  2005.07.19</author>
    /// <author>Ana Cikic  2006.10.23</author>
    /// <since>Coherence 3.1</since>
    public abstract class AbstractAggregator : IParallelAwareAggregator, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Determine the <see cref="IValueExtractor"/> whose values this
        /// aggregator is aggregating.
        /// </summary>
        /// <value>
        /// The <b>IValueExtractor</b> used by this aggregator.
        /// </value>
        public virtual IValueExtractor Extractor
        {
            get { return m_extractor; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AbstractAggregator()
        {}

        /// <summary>
        /// Construct an AbstractAggregator that will aggregate values
        /// extracted from a collection of
        /// <see cref="IInvocableCacheEntry"/> objects.
        /// </summary>
        /// <param name="extractor">
        /// The <see cref="IValueExtractor"/> that provides values to
        /// aggregate.
        /// </param>
        public AbstractAggregator(IValueExtractor extractor)
        {
            Debug.Assert(extractor != null);
            m_extractor = extractor;
        }

        /// <summary>
        /// Construct an AbstractAggregator that will aggregate values
        /// extracted from a collection of
        /// <see cref="IInvocableCacheEntry"/> objects.
        /// </summary>
        /// <param name="member">
        /// The name of the member that could be invoked via reflection and
        /// that returns values to aggregate; this parameter can also be a
        /// dot-delimited sequence of member names which would result in an
        /// aggregator based on the <see cref="ChainedExtractor"/>} that is
        /// based on an array of corresponding
        /// <see cref="ReflectionExtractor"/> objects.
        /// </param>
        public AbstractAggregator(string member)
        {
            m_extractor = member.IndexOf('.') < 0
                              ? new ReflectionExtractor(member)
                              : (IValueExtractor) new ChainedExtractor(member);
        }

        #endregion

        #region AbstractAggregrator interface

        /// <summary>
        /// Initialize the aggregation result.
        /// </summary>
        /// <remarks>
        /// This implementation throws a NotSupportedException.
        /// </remarks>
        /// <param name="isFinal">
        /// <b>true</b> is passed if the aggregation process that is being
        /// initialized must produce a final aggregation result; this will
        /// only be <b>false</b> if a parallel approach is being used and the
        /// initial (partial) aggregation process is being initialized.
        /// </param>
        protected virtual void Init(bool isFinal)
        {
            throw new NotSupportedException(
                "This aggregator cannot be invoked on the client.");
        }

        /// <summary>
        /// Incorporate one aggregatable value into the result.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="isFinal"/> parameter is <b>true</b>, the
        /// given object is a partial result (returned by an individual
        /// parallel aggregator) that should be incorporated into the final
        /// result; otherwise, the object is a value extracted from an
        /// <see cref="IInvocableCacheEntry"/>.
        /// <p>
        /// This implementation throws a NotSupportedException.
        /// </p>
        /// </remarks>
        /// <param name="o">
        /// The value to incorporate into the aggregated result.
        /// </param>
        /// <param name="isFinal">
        /// <b>true</b> to indicate that the given object is a partial
        /// result returned by a parallel aggregator.
        /// </param>
        protected virtual void Process(object o, bool isFinal)
        {
            throw new NotSupportedException(
                "This aggregator cannot be invoked on the client.");
        }

        /// <summary>
        /// Obtain the result of the aggregation.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="isFinal"/> parameter is <b>true</b>, the
        /// returned object must be the final result of the aggregation;
        /// otherwise, the returned object will be treated as a partial
        /// result that should be incorporated into the final result.
        /// <p>
        /// This implementation throws a NotSupportedException.
        /// </p>
        /// </remarks>
        /// <param name="isFinal">
        /// <b>true</b> to indicate that the final result of the aggregation
        /// process should be returned; this will only be <b>false</b> if a
        /// parallel approach is being used.
        /// </param>
        /// <returns>
        /// The result of the aggregation process.
        /// </returns>
        protected virtual object FinalizeResult(bool isFinal)
        {
            throw new NotSupportedException(
                "This aggregator cannot be invoked on the client.");
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
                m_isParallel = true;
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
            Init(true);

            foreach (object o in results)
            {
                Process(o, true);
            }

            try
            {
                return FinalizeResult(true);
            }
            finally
            {
                // reset the state to allow the aggregator re-use
                m_isParallel = false;
            }
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
            bool isFinal = !m_isParallel;
            Init(isFinal);

            IValueExtractor extractor = Extractor;
            foreach (IInvocableCacheEntry invEntry in entries)
            {
                Process(invEntry.Extract(extractor), false);
            }

            return FinalizeResult(isFinal);
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
            m_isParallel = reader.ReadBoolean(0);
            m_extractor  = (IValueExtractor) reader.ReadObject(1);
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
            writer.WriteBoolean(0, m_isParallel);
            writer.WriteObject(1, m_extractor);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Provide a human-readable representation of this object.
        /// </summary>
        /// <returns>
        /// A string whose contents represent the value of this object.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + '(' + Extractor.ToString() + ')';
        }

        /// <summary>
        /// Returns a hash code value for this object.
        /// </summary>
        /// <returns>
        /// A hash code value for this object.
        /// </returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ Extractor.GetHashCode();
        }

        /// <summary>
        /// Compares this object with another object for equality.
        /// </summary>
        /// <param name="o">
        /// An object reference or <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> if the passed object reference is of the same class
        /// and has the same state as this object.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is AbstractAggregator)
            {
                AbstractAggregator that = (AbstractAggregator) o;
                return this == that
                       || GetType()  == that.GetType()
                       && m_isParallel == that.m_isParallel
                       && Equals(m_extractor, that.m_extractor);
            }
            return false;
        }

        #endregion

        #region Data members

        /// <summary>
        /// Set to true if this aggregator realizes that it is going to be
        /// used in parallel.
        /// </summary>
        protected bool m_isParallel;

        /// <summary>
        /// The IValueExtractor that obtains the value to aggregate from the
        /// value that is stored in the dictionary.
        /// </summary>
        private IValueExtractor m_extractor;

        #endregion
    }
}