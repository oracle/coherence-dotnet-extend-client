/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

using Tangosol.Net.Partition;
using Tangosol.Util.Aggregator;

namespace Tangosol.Util
{
    /// <summary>
    /// The IQueryRecord object carries a record of the estimated or actual
    /// execution cost for a query operation.
    /// </summary>
    /// <author>tb 2011.05.26</author>
    /// <since>Coherence 3.7.1</since>
    public interface IQueryRecord
    {
        /// <summary>
        /// The <see cref="RecordType"/> that was specified when this query record 
        /// was created.
        /// </summary>
        QueryRecorder.RecordType RecordType
        {
            get;
        }

        /// <summary>
        /// The list of partial results for this query record.
        /// </summary>
        IList Results
        {
            get;
        }
    }

    /// <summary>
    /// An <b>IQueryRecord.PartialResult</b> is a partial query record that contains
    /// recorded costs for a query operation.  Partial results are collected
    /// in a query record by a <see cref="QueryRecorder"/>.
    /// </summary>
    public interface IPartialResult
    {
        /// <summary>
        /// The list of steps for this query record partial result in the
        /// order that they occurred.
        /// </summary>
        IList Steps
        {
            get;
        }

        /// <summary>
        /// The set of partitions associated with this partial result.
        /// </summary>
        PartitionSet Partitions
        {
            get;
        }
    }

    /// <summary>
    /// A <b>IQueryRecord.Step</b> carries the recorded cost of evaluating a filter
    /// as part of a query operation.  This cost may be the estimated or
    /// actual execution cost depending on the <see cref="IQueryRecord.RecordType"/> of the recorder 
    /// in use when the step was created.
    /// </summary>
    public interface IStep
    {
        /// <summary>
        /// A description of the filter that was associated with this
        /// step during its creation.
        /// </summary>                         
        string FilterDescription
        {
            get;
        }

        /// <summary>
        /// The recorded information about the index lookups performed 
        /// during filter evaluation as part of a query record.
        /// </summary>           
        ICollection IndexLookupRecords
        {
            get;
        }

        /// <summary>
        /// The calculated cost of applying the filter as defined by 
        /// IIndexAwareFilter.CalculateEffectiveness(IDictionary, ICollection).
        /// </summary>
        int Efficiency
        {
            get;
        }

        /// <summary>
        /// The size of the key set prior to evaluating the filter or applying an index.  
        /// This value can be used together with <see cref="PostFilterKeySetSize"/> to 
        /// calculate an actual effectiveness (reduction of the key set) for this filter 
        /// step.
        /// </summary>              
        int PreFilterKeySetSize
        {
            get;
        }

        /// <summary>
        /// The size of the key set remaining after evaluating the
        /// filter or applying an index.  This value can be used together
        /// with <see cref="PreFilterKeySetSize"/> to calculate an actual
        /// effectiveness (reduction of the key set) for this filter step.
        /// </summary>
        int PostFilterKeySetSize
        {
            get;
        }

        /// <summary>
        /// The amount of time (in ms) spent evaluating the filter or
        /// applying an index for this query plan step.
        /// </summary>
        long Duration
        {
            get;
        }

        /// <summary>
        /// The inner nested steps, may be null if not nested.
        /// </summary>
        IList Steps
        {
            get;
        }
    }

    /// <summary>
    ///  An <b>IIndexLookupRecord</b> holds the recorded information about an index 
    ///  lookup performed during filter evaluation as part of a query record.
    ///  
    /// An <B>IIndexLookupRecord</B> is created each time that RecordExtractor(ValueExtractor)
    /// is called on a query record step.
    /// </summary>
    public interface IIndexLookupRecord
    {
        /// <summary>
        /// A description of the extractor that was used for the index lookup.
        /// </summary>
        string ExtractorDescription
        {
            get;
        }

        /// <summary>
        /// A description of the associated index.
        /// </summary>
        string IndexDescription
        {
            get;
        }

        /// <summary>
        /// Indicates whether or not the associated index is ordered.
        /// </summary>
        bool IsOrdered
        {
            get;
        }
    }
}
