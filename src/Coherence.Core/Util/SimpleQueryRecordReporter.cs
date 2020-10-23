/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Text;

using Tangosol.Util.Aggregator;

namespace Tangosol.Util
{
    /// <summary>
    /// Simple query record reporter used to obtain a string representation of 
    /// <see cref="IQueryRecord"/> object.
    /// </summary>
    /// <author>tb 2011.05.26</author>
    /// <since>Coherence 3.7.1</since>
    public class SimpleQueryRecordReporter
    {
        #region Reporter methods

        /// <summary>
        /// Return a report for the given query record.
        /// </summary>
        /// <param name="record">
        /// The record.
        /// </param>
        /// <returns>
        /// A report for the given query record.
        /// </returns>
        public static string Report(IQueryRecord record)
        {
            StringBuilder sb = new StringBuilder();

            IList listIndexLookups = new ArrayList();
            IList listRecords      = record.Results;
            bool  fReportPartition = listRecords.Count > 1;

            foreach (IPartialResult partial in listRecords)
            {
                sb.Append(ReportResult(partial, record.RecordType,
                        listIndexLookups, fReportPartition));
            }

            sb.Append(ReportIndexLookUps(listIndexLookups));

            return sb.ToString();
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Report the given result.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <param name="type">
        /// The record type.
        /// </param>
        /// <param name="listIndexLookups">
        /// The list of lookup ids.
        /// </param>
        /// <param name="fReportPartition">
        /// Indicates whether or not to report partitions.
        /// </param>
        /// <returns>
        /// A report for the given result.
        /// </returns>
        protected static string ReportResult(IPartialResult result,
                QueryRecorder.RecordType type,
                IList listIndexLookups,
                bool fReportPartition)
        {
            StringBuilder sb = new StringBuilder();

            if (type == QueryRecorder.RecordType.Trace)
            {
                sb.Append(String.Format(REPORT_TRACE_HEADER_FORMAT,
                        "Name", "Index", "Effectiveness", "Duration"));
            }
            else
            {
                sb.Append(String.Format(REPORT_EXPLAIN_HEADER_FORMAT,
                        "Name", "Index", "Cost"));
            }

            sb.Append(String.Format(REPORT_DIVIDER));


            foreach (IStep childStep in result.Steps)
            {
                sb.Append(ReportStep(childStep, type, listIndexLookups, 0));
                sb.Append(String.Format("\n"));
            }

            sb.Append(String.Format("\n"));

            if (fReportPartition)
            {
                sb.Append(String.Format(REPORT_PARTITION_FORMAT,
                        result.Partitions.ToString()));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Report the index look ups.
        /// </summary>
        /// <param name="listIndexLookups">
        /// The list of lookup ids.
        /// </param>
        /// <returns>
        /// A report for the index look ups.
        /// </returns>
        protected static string ReportIndexLookUps(
                IList listIndexLookups)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(String.Format(REPORT_INDEX_HEADER_FORMAT, 
                "Index", "Description", "Extractor", "Ordered"));
            sb.Append(String.Format(REPORT_DIVIDER));

            for (int i = 0; i < listIndexLookups.Count; i++)
            {
                sb.Append(ReportIndexLookupRecord(i, 
                    (IIndexLookupRecord)listIndexLookups[i]));
                sb.Append(String.Format("\n"));
            }

            sb.Append(String.Format("\n"));
            return sb.ToString();
        }

        /// <summary>
        /// Report the given step.
        /// </summary>
        /// <param name="step">
        /// The step.
        /// </param>
        /// <param name="type">
        /// The record type.
        /// </param>
        /// <param name="listIndexLookups">
        /// The list of lookup ids.
        /// </param>
        /// <param name="nLevel">
        /// The indent level.
        /// </param>
        /// <returns>
        /// A report line for the given step.
        /// </returns>
        protected static string ReportStep(IStep step,
                QueryRecorder.RecordType type,
                IList listIndexLookups,
                int nLevel)
        {
            StringBuilder sbName = new StringBuilder();
            for (int i = 0; i < nLevel; ++i)
            {
                sbName.Append("  ");
            }
            sbName.Append(step.FilterDescription);

            string sCost     = step.Efficiency >= 0 ? step.Efficiency.ToString() : REPORT_NA;
            string sSizeIn   = step.PreFilterKeySetSize >= 0 ? step.PreFilterKeySetSize.ToString() : REPORT_NA;
            string sSizeOut  = step.PostFilterKeySetSize >= 0 ? step.PostFilterKeySetSize.ToString() : REPORT_NA;
            string sDuration = step.Duration >= 0 ? step.Duration.ToString() : REPORT_NA;

            StringBuilder sbIndex = new StringBuilder();

            foreach (IIndexLookupRecord record in step.IndexLookupRecords)
            {
                int nIndex = listIndexLookups.IndexOf(record);
                if (nIndex == -1)
                {
                    nIndex = listIndexLookups.Count;

                    listIndexLookups.Add(record);
                }
                sbIndex.Append(sbIndex.Length > 0 ? "," : "" + nIndex);
            }

            StringBuilder sbStep = new StringBuilder();
            if (type == QueryRecorder.RecordType.Trace)
            {
                int nEff = step.PreFilterKeySetSize == 0 ? 0 :
                        (step.PreFilterKeySetSize - step.PostFilterKeySetSize) * 100 / step.PreFilterKeySetSize;

                String sEff = sSizeIn + "|" + sSizeOut + "(" + nEff + "%)";

                sbStep.Append(String.Format(REPORT_TRACE_STEP_FORMAT,
                        sbName,
                        sbIndex.Length > 0 ? sbIndex.ToString() : REPORT_NA,
                        sEff, sDuration));
            }
            else
            {
                sbStep.Append(String.Format(REPORT_EXPLAIN_STEP_FORMAT,
                        sbName,
                        sbIndex.Length > 0 ? sbIndex.ToString() : REPORT_NA,
                        sCost));
            }

            foreach (IStep stepChild in step.Steps)
            {
                sbStep.Append(String.Format("\n")).Append(ReportStep(stepChild,
                        type, listIndexLookups, nLevel + 1));
            }

            return sbStep.ToString();
        }

        /// <summary>
        /// Report the given index lookup record with the given id.
        /// </summary>
        /// <param name="nIndexLookupId">
        /// The index lookup id.
        /// </param>
        /// <param name="record">
        /// The index lookup record.
        /// </param>
        /// <returns>
        /// A report line for the given index lookup.
        /// </returns>
        protected static string ReportIndexLookupRecord(int nIndexLookupId,
                IIndexLookupRecord record)
        {
            string sIndexDesc = record.IndexDescription;

            return String.Format(REPORT_INDEX_LOOKUP_FORMAT,
                    nIndexLookupId.ToString(),
                    sIndexDesc == null ? REPORT_NO_INDEX : sIndexDesc,
                    record.ExtractorDescription,
                    record.IsOrdered);
        }
        #endregion


        #region Constants

        /// <summary>
        /// Report divider format string.
        /// </summary>
        private static readonly string REPORT_DIVIDER = "======================================================================================\n";

        /// <summary>
        /// Report no-info available string.
        /// </summary>
        private static readonly string REPORT_NA = "----";

        /// <summary>
        /// Report no-index string.
        /// </summary>
        private static readonly string REPORT_NO_INDEX = "No index found";

        /// <summary>
        /// Report partition group string.
        /// </summary>
        private static readonly string REPORT_PARTITION_FORMAT = "{0}\n\n";

        /// <summary>
        /// Report trace header string.
        /// </summary>
        private static readonly string REPORT_TRACE_HEADER_FORMAT = "\nTrace\n{0, -35}   {1, -10}   {2, -20}   {3, -10}\n";

        /// <summary>
        /// Report explain header string.
        /// </summary>
        private static readonly string REPORT_EXPLAIN_HEADER_FORMAT = "\nExplain Plan\n{0, -35}   {1, -10}   {2, -10}\n";

        /// <summary>
        /// Report index lookup header string.
        /// </summary>
        private static readonly string REPORT_INDEX_HEADER_FORMAT = "\nIndex Lookups\n{0, -5}  {1, -40}  {2, -20}  {3, -10}\n";

        /// <summary>
        /// Report trace step format string.
        /// </summary>
        private static readonly string REPORT_TRACE_STEP_FORMAT = "{0, -35} | {1, -10} | {2, -20} | {3, -10}";

        /// <summary>
        /// Report explain step format string.
        /// </summary>
        private static readonly string REPORT_EXPLAIN_STEP_FORMAT = "{0, -35} | {1, -10} | {2, -10}";

        /// <summary>
        /// Report index lookup format string.
        /// </summary>
        private static readonly string REPORT_INDEX_LOOKUP_FORMAT = "{0, -5} | {1, -40} | {2, -20}  {3}";
        
        #endregion
    }
}
