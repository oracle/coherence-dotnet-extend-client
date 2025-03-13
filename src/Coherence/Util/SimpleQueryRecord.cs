/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Partition;
using Tangosol.Run.Xml;
using Tangosol.Util.Aggregator;


namespace Tangosol.Util
{
    /// <summary>
    /// Simple QueryRecord implementation.
    /// </summary>
    /// <author>tb 2011.05.26</author>
    /// <since>Coherence 3.7.1</since>
    public class SimpleQueryRecord : IQueryRecord, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor (necessary for IPortableObject interface).
        /// </summary>
        public SimpleQueryRecord()
        {
        }

        /// <summary>
        /// Construct a <b>SimpleQueryRecord</b> from the given collection of partial
        /// results.
        /// </summary>
        /// <param name="type">
        /// The record type.
        /// </param>
        /// <param name="colResults">
        /// The collection of partial results.
        /// </param>
        public SimpleQueryRecord(QueryRecorder.RecordType type, ICollection colResults)
        {
            m_type = type;
            MergeResults(colResults);
        }

        #endregion

        #region IQueryRecord implementation

        /// <summary>
        /// The RecordType that was specified when this query record 
        /// was created.
        /// </summary>
        public QueryRecorder.RecordType RecordType
        {
            get { return m_type; }
        }

        /// <summary>
        /// The list of partial results for this query record.
        /// </summary>
        public IList Results
        {
            get { return m_listResults; }
        }
        #endregion

        #region Helper methods

        /// <summary>
        /// Merge the partial results from the associated record.  Matching
        /// partial results are merged into a single result for the report.
        /// </summary>
        /// <param name="colResults">
        /// The collection of partial results.
        /// </param>
        protected void MergeResults(ICollection colResults)
        {
            IList listResults = m_listResults;
            foreach (IPartialResult resultThat in colResults)
            {
                bool fMerged = false;
                foreach (SimpleQueryRecord.PartialResult resultThis in listResults)
                {
                    if (resultThis.IsMatching(resultThat))
                    {
                        resultThis.Merge(resultThat);
                        fMerged = true;
                        break;
                    }
                }

                if (!fMerged)
                {
                    // no matching partial result found; create a new one
                    listResults.Add(new SimpleQueryRecord.PartialResult(resultThat));
                }
            }
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
            reader.ReadCollection(1, m_listResults);
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
            writer.WriteCollection(1, m_listResults);
        }

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns a string representation of this <b>SimpleQueryRecord</b>.
        /// </summary>
        /// <returns>
        /// A string representation of this <b>SimpleQueryRecord</b>.
        /// </returns>
        public override string ToString()
        {
            return SimpleQueryRecordReporter.Report(this);
        }

        #endregion

        #region Inner class : PartialResult

        /// <summary>
        /// Simple <see cref="IPartialResult"/> implementation.
        /// </summary>
        public class PartialResult : IPartialResult, IPortableObject
        {
            #region Constructors

            /// <summary>
            /// Default constructor (necessary for IPortableObject interface).
            /// </summary>
            public PartialResult()
            {
            }

            /// <summary>
            /// Copy constructor for a <b>IPartialResult</b>.
            /// </summary>
            /// <param name="colResult">
            /// The <b>IPartialResult</b> to copy.
            /// </param>
            public PartialResult(IPartialResult colResult)
            {
                m_partMask = colResult.Partitions;

                IList listSteps = m_listSteps;

                foreach (IStep step in colResult.Steps)
                {
                    listSteps.Add(new Step(step));
                }
            }
            #endregion

            #region IPartialResult implementation

            /// <summary>
            /// The list of steps for this query record partial result in the
            /// order that they occurred.
            /// </summary>
            public IList Steps
            {
                get { return m_listSteps; }
            }

            /// <summary>
            /// The set of partitions associated with this partial result.
            /// </summary>
            public PartitionSet Partitions
            {
                get { return m_partMask; }
            }

            #endregion

            #region Helper methods

            /// <summary>
            /// Merge the given result with this one.
            /// </summary>
            /// <param name="result">
            /// The result to merge.
            /// </param>
            public void Merge(IPartialResult result)
            {
                Partitions.Add(result.Partitions);

                IList listStepsThis = m_listSteps;
                IList listStepsThat = result.Steps;

                for (int i = 0; i < listStepsThat.Count; i++)
                {
                    IStep step      = (IStep)listStepsThat[i];
                    Step  mergeStep = (Step)listStepsThis[i];

                    mergeStep.Merge(step);
                }
            }

            /// <summary>
            /// Determine whether or not the given result is capable of being
            /// placed in one-to-one correspondence with this result.  Results are
            /// matching if their owned lists of steps have the same size, and
            /// all pairs of steps in the two lists are matching.
            /// </summary>
            /// <param name="result">
            /// The result to be checked.
            /// </param>
            /// <returns>
            /// True iff the given result matches with this result.
            /// </returns>
            public bool IsMatching(IPartialResult result)
            {
                IList listStepsThis = m_listSteps;
                IList listStepsThat = result.Steps;

                if (listStepsThis.Count != listStepsThat.Count)
                {
                    return false;
                }

                for (int i = 0; i < listStepsThis.Count; i++)
                {
                    if (!((Step)listStepsThis[i]).IsMatching((IStep)listStepsThat[i]))
                    {
                        return false;
                    }
                }
                return true;
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
                m_partMask = (PartitionSet) reader.ReadObject(0);
                reader.ReadCollection(1, m_listSteps);
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
                writer.WriteObject(0, m_partMask);
                writer.WriteCollection(1, m_listSteps);
            }

            #endregion
            
            #region Inner class : Step

            /// <summary>
            /// Simple <b>IQueryRecord.IPartialResult.IStep</b> implementation.
            /// </summary>
            public class Step : IStep, IPortableObject
            {
                #region Constructors

                /// <summary>
                /// Default constructor (necessary for IPortableObject interface).
                /// </summary>
                public Step()
                {
                }

                /// <summary>
                /// Copy constructor for an <b>IStep</b>.
                /// </summary>
                /// <param name="step">
                /// The step to copy.
                /// </param>
                public Step(IStep step)
                {
                    m_sFilter     = step.FilterDescription;
                    m_nSizeIn     = step.PreFilterKeySetSize;
                    m_nSizeOut    = step.PostFilterKeySetSize;
                    m_nEfficiency = step.Efficiency;
                    m_cMillis     = step.Duration;

                    foreach (IIndexLookupRecord record in step.IndexLookupRecords)
                    {
                        m_setIndexLookupRecords.Add(new IndexLookupRecord(record));
                    }

                    foreach (IStep stepInner in step.Steps)
                    {
                        m_listSubSteps.Add(new Step(stepInner));
                    }
                }

                #endregion

                #region IStep implementation

                /// <summary>
                /// A description of the filter that was associated with this
                /// step during its creation.
                /// </summary>     
                public string FilterDescription
                {
                    get { return m_sFilter; }
                }

                /// <summary>
                /// The recorded information about the index lookups performed 
                /// during filter evaluation as part of an <b>IQueryRecord</b>.
                /// </summary>  
                public ICollection IndexLookupRecords
                {
                    get { return m_setIndexLookupRecords; }
                }

                /// <summary>
                /// The calculated cost of applying the filter as defined by 
                /// IIndexAwareFilter.CalculateEffectiveness(IDictionary, ICollection).
                /// </summary>
                public int Efficiency
                {
                    get { return m_nEfficiency; }
                }

                /// <summary>
                /// The size of the key set prior to evaluating the filter or applying an index.  
                /// This value can be used together with <see cref="PostFilterKeySetSize"/> to calculate 
                /// an actual effectiveness (reduction of the key set) for this filter step.
                /// </summary> 
                public int PreFilterKeySetSize
                {
                    get { return m_nSizeIn; }
                }

                /// <summary>
                /// The size of the key set remaining after evaluating the
                /// filter or applying an index.  This value can be used together
                /// with <see cref="PreFilterKeySetSize"/> to calculate an actual
                /// effectiveness (reduction of the key set) for this filter step.
                /// </summary>
                public int PostFilterKeySetSize
                {
                    get { return m_nSizeOut; }
                }

                /// <summary>
                /// The amount of time (in ms) spent evaluating the filter or
                /// applying an index for this query plan step.
                /// </summary>
                public long Duration
                {
                    get { return m_cMillis; }
                }

                /// <summary>
                /// The inner nested steps, may be null if not nested.
                /// </summary>
                public IList Steps
                {
                    get { return m_listSubSteps; }
                }

                #endregion

                #region Helper methods

                /// <summary>
                /// Determine whether or not the given step is capable of being
                /// placed in one-to-one correspondence with this step.  Steps are
                /// defined to be matching if both steps have equivalent name,
                /// index lookup records and owned lists of sub-steps.
                /// </summary>
                /// <param name="step">
                /// The <b>IStep</b> to be checked.
                /// </param>
                /// <returns>
                /// True iff the given step matches with this step.
                /// </returns>
                public bool IsMatching(IStep step)
                {
                    if (FilterDescription.Equals(step.FilterDescription) &&
                        IndexLookupRecords.Equals(step.IndexLookupRecords))
                    {
                        IList listSteps = step.Steps;

                        if (m_listSubSteps.Count == listSteps.Count)
                        {
                            int i = 0;
                            foreach (Step subStep in m_listSubSteps)
                            {
                                if (!subStep.IsMatching((IStep)listSteps[i++]))
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    }

                    return false;
                }


                /// <summary>
                /// Merge the given step with this one.  This method assumes that
                /// the given step matches with this one.
                /// </summary>
                /// <param name="step">
                /// The <b>IStep</b> to merge.
                /// </param>
                public void Merge(IStep step)
                {
                    m_nSizeIn     += step.PreFilterKeySetSize;
                    m_nSizeOut    += step.PostFilterKeySetSize;
                    m_nEfficiency += step.Efficiency;
                    m_cMillis     += step.Duration;

                    IDictionary<IndexLookupRecord, IndexLookupRecord> dictIndexRecords = 
                        new Dictionary<IndexLookupRecord, IndexLookupRecord>();
                    foreach (IndexLookupRecord record in step.IndexLookupRecords)
                    {
                        dictIndexRecords.Add(record, record);
                    }

                    foreach (IndexLookupRecord indexLookupRecord in m_setIndexLookupRecords)
                    {
                        IndexLookupRecord record = dictIndexRecords[indexLookupRecord];

                        if (record != null && record.IndexDescription != null)
                        {
                            indexLookupRecord.MemoryUsage     += record.MemoryUsage;
                            indexLookupRecord.Size             = Math.Max(indexLookupRecord.Size, record.Size);
                            indexLookupRecord.IndexDescription = indexLookupRecord.BuildIndexDescription();
                        }
                    }

                    IList listSteps = step.Steps;

                    int i = 0;
                    foreach (Step subStep in m_listSubSteps)
                    {
                        subStep.Merge((IStep)listSteps[i++]);
                    }
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
                    m_sFilter     = (string) reader.ReadObject(0);
                    m_nEfficiency = reader.ReadInt32(1);
                    m_nSizeIn     = reader.ReadInt32(2);
                    m_nSizeOut    = reader.ReadInt32(3);
                    m_cMillis     = reader.ReadInt64(4);

                    reader.ReadCollection(5, m_setIndexLookupRecords);
                    reader.ReadCollection(6, m_listSubSteps);
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
                    writer.WriteObject(0, m_sFilter);
                    writer.WriteInt32(1, m_nEfficiency);
                    writer.WriteInt32(2, m_nSizeIn);
                    writer.WriteInt32(3, m_nSizeOut);
                    writer.WriteInt64(4, m_cMillis);
                    writer.WriteCollection(5, m_setIndexLookupRecords);
                    writer.WriteCollection(6, m_listSubSteps);
                }

                #endregion

                #region Data members

                /// <summary>
                /// The filter description.
                /// </summary> 
                protected string m_sFilter;

                /// <summary>
                /// The estimated cost.
                /// </summary> 
                protected int m_nEfficiency;

                /// <summary>
                /// The pre-execution key set size.
                /// </summary> 
                protected int m_nSizeIn = 0;

                /// <summary>
                /// The post-execution key set size.
                /// </summary> 
                protected int m_nSizeOut = 0;

                /// <summary>
                /// The execution time in milliseconds.
                /// </summary> 
                protected long m_cMillis = 0L;

                /// <summary>
                /// The set of index lookup records.
                /// </summary> 
                protected IList m_setIndexLookupRecords = new ArrayList();

                /// <summary>
                /// The list of child steps.
                /// </summary> 
                protected IList m_listSubSteps = new ArrayList();

                #endregion
            }
            #endregion

            #region Inner class : IndexLookupRecord

            /// <summary>
            /// Simple <b>IQueryRecord.IPartialResult.IIndexLookupRecord</b> implementation.
            /// </summary>
            public class IndexLookupRecord : IIndexLookupRecord, IPortableObject
            {
                #region Contructors

                /// <summary>
                /// Default constructor (necessary for IPortableObject interface).
                /// </summary>
                public IndexLookupRecord()
                {
                }

                /// <summary>
                /// Copy constructor for an IndexLookupRecord.
                /// </summary>
                /// <param name="record">
                /// The record to copy.
                /// </param>
                public IndexLookupRecord(IIndexLookupRecord record)
                {
                    IndexLookupRecord thatRecord = (IndexLookupRecord) record;

                    m_sExtractor      = thatRecord.ExtractorDescription;
                    m_sIndex          = thatRecord.IndexDescription;
                    m_fOrdered        = thatRecord.IsOrdered;
                    m_cBytes          = thatRecord.MemoryUsage;
                    m_cDistinctValues = thatRecord.Size;
                    m_sIndexDef       = thatRecord.IndexDef;
                }
                #endregion

                #region IIndexLookupRecord implementation

                /// <summary>
                /// A description of the extractor that was used for the index lookup.
                /// </summary>
                public string ExtractorDescription
                {
                    get { return m_sExtractor; }
                }

                /// <summary>
                /// A description of the associated index.
                /// </summary>
                public string IndexDescription
                {
                    get { return m_sIndex; }

                    set { m_sIndex = value; }
                }

                /// <summary>
                /// Indicates whether or not the associated index is ordered.
                /// </summary>
                public bool IsOrdered
                {
                    get { return m_fOrdered; }
                }

                /// <summary>
                /// Returns index memory usage in bytes.
                /// </summary>
                /// <return> 
                /// index memory usage in bytes; -1 if there is no index
                /// </return>
                public long MemoryUsage
                {
                    get { return m_cBytes; }

                    set { m_cBytes = value; }
                }

                /// <summary>
                /// Return index content map size.
                /// </summary>
                /// <return> 
                /// index content map size; -1 if there is no index
                /// </return>
                public int Size
                {
                    get { return m_cDistinctValues; }

                    set { m_cDistinctValues = value; }
                }

                /// <summary>
                /// Returns the index definition.
                /// </summary>
                /// <return>
                /// the index definition; null if there is no index
                /// </return>
                public string IndexDef
                {
                    get { return m_sIndexDef; }
                }

                #endregion

                #region Object override methods

                /// <summary>
                /// Generates hash code for this <b>CacheEntry.</b>
                /// </summary>
                /// <returns>
                /// A hash code for this <b>CacheEntry.</b>
                /// </returns>
                public override int GetHashCode()
                {
                    return  (m_sIndexDef == null ? 0 : m_sIndexDef.GetHashCode()) + 
                        m_sExtractor.GetHashCode() + (m_fOrdered ? 1 : 0);
                }

                /// <summary>
                /// Checks two cache entries for equality.
                /// </summary>
                /// <param name="obj">
                /// The <b>IndexLookupRecord</b> to compare to.
                /// </param>
                /// <returns>
                /// <b>true</b> if this <b>IndexLookupRecord</b> and the passed object are
                /// equivalent.
                /// </returns>
                public override bool Equals(object obj)
                {
                    if (this == obj)
                    {
                        return true;
                    }

                    // Note: IndexLookupRecords are considered equivalent based on
                    //       the definition of the indices and not varying factors
                    //       such as index footprint and size.
                    IndexLookupRecord that = obj as IndexLookupRecord;
                    return that != null &&
                           m_fOrdered == that.m_fOrdered &&
                           Equals(m_sIndexDef, that.m_sIndexDef) &&
                           m_sExtractor.Equals(that.m_sExtractor);
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
                    m_sExtractor = (string) reader.ReadObject(0);
                    m_sIndex     = (string) reader.ReadObject(1);
                    m_fOrdered   = reader.ReadBoolean(2);

                    if (m_sIndex != null)
                    {
                        ParseIndexDescription(m_sIndex);
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
                public virtual void WriteExternal(IPofWriter writer)
                {
                    writer.WriteObject(0, m_sExtractor);
                    writer.WriteObject(1, m_sIndex);
                    writer.WriteBoolean(2, m_fOrdered);
                }

                #endregion

                #region helper methods

                /// <summary>
                /// Build an index description for this index.
                /// </summary>
                /// <return>
                /// an index description for this index if there is an index definition;
                /// null otherwise
                /// </return>
                public string BuildIndexDescription()
                {
                    if (m_sIndexDef == null)
                    {
                        return m_sIndexDef;
                    }

                    string sFP = StringUtils.ToMemorySizeString(m_cBytes, false);
                    return m_sIndexDef + FOOTPRINT + (sFP.EndsWith("B") ? sFP : sFP + "B")
                                     + MAP_SIZE + m_cDistinctValues;
                }

                /// <summary>
                /// Parses an index description into it's definition, footprint,
                /// and map size.
                /// </summary>
                /// <param name = "sIndex">
                /// the index description
                /// </param>
                public void ParseIndexDescription(string sIndex)
                {
                    int iStart = sIndex.IndexOf(FOOTPRINT);

                    if (iStart <= 0)
                    {
                        return;
                    }

                    m_sIndexDef = sIndex.Substring(0, iStart);

                    int iLen = sIndex.IndexOf(',', iStart) - (iStart + FOOTPRINT_LEN);
                    m_cBytes = XmlHelper.ParseMemorySize(sIndex.Substring(iStart + FOOTPRINT_LEN, iLen));

                    iStart = sIndex.IndexOf(MAP_SIZE);
                    m_cDistinctValues = Int32.Parse(sIndex.Substring(iStart + MAP_SIZE_LEN));
                }

                #endregion

                #region constants

                /// <summary>
                /// Footprint string in the index description.
                /// </summary>
                private const string FOOTPRINT = "Footprint=";

                /// <summary>
                /// Map size string in the index description.
                /// </summary>
                private const string MAP_SIZE = ", Size=";

                /// <summary>
                /// Footprint string length in the index description.
                /// </summary>
                private readonly int FOOTPRINT_LEN = FOOTPRINT.Length;

                /// <summary>
                /// Map size string length in the index description.
                /// </summary>
                private readonly int MAP_SIZE_LEN = MAP_SIZE.Length;

                #endregion

                #region Data members

                /// <summary>
                /// The extractor description.
                /// </summary> 
                private string m_sExtractor;

                /// <summary>
                /// The index description.
                /// </summary> 
                private string m_sIndex;

                /// <summary>
                /// Indicates whether or not the associated index is ordered.
                /// </summary> 
                private bool m_fOrdered;

                /// <summary>
                /// The index footprint in bytes.
                /// </summary> 
                private long m_cBytes = -1;

                /// <summary>
                /// The index content map size.
                /// </summary> 
                private int m_cDistinctValues = -1;

                /// <summary>
                /// The index definition.
                /// </summary> 
                private string m_sIndexDef;

                #endregion
            }
            #endregion
 
            #region Data members

            /// <summary>
            /// The map of steps.
            /// </summary>
            protected IList m_listSteps = new ArrayList();

            /// <summary>
            /// The partitions.
            /// </summary>
            protected PartitionSet m_partMask;

            #endregion
        }
        #endregion

        #region Data members

        /// <summary>
        /// This record type.
        /// </summary>
        protected QueryRecorder.RecordType m_type;

        /// <summary>
        /// The list of partial results.
        /// </summary>
        protected IList m_listResults = new ArrayList();

        #endregion
    }
}
