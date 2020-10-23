/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Aggregator
{
    /// <summary>
    /// Calculates an average for values of any numberic type extracted from
    /// a set of entries in a <b>IDictionary</b> in a form of a
    /// <see cref="Decimal"/> value.
    /// </summary>
    /// <remarks>
    /// All the extracted objects will be treated as <b>Decimal</b> values.
    /// If the set of entries is empty, a <c>null</c> result is returned..
    /// </remarks>
    /// <author>Gene Gleyzer  2006.07.18</author>
    /// <author>Goran Milosavljevic  2008.01.30</author>
    /// <since>Coherence 3.1</since>
    public class DecimalAverage : AbstractDecimalAggregator
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DecimalAverage()
        {}

        /// <summary>
        /// Construct a <b>DecimalAverage</b> aggregator.
        /// </summary>
        /// <param name="extractor">
        /// The extractor that provides a value in the form of any .NET
        /// object that is a <b>Decimal</b>.
        /// </param>
        public DecimalAverage(IValueExtractor extractor) : base(extractor)
        {}

        /// <summary>
        /// Construct a <b>DoubleAverage</b> aggregator.
        /// </summary>
        /// <param name="member">
        /// The name of the member that returns a value in the form of any
        /// .NET object that is a <b>Decimal</b>.
        /// </param>
        public DecimalAverage(string member) : base(member)
        {}

        #endregion

        #region AbstractAggregator override methods

        /// <summary>
        /// Incorporate one aggregatable value into the result.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="isFinal"/> parameter is <b>true</b>, the
        /// given object is a partial result (returned by an individual
        /// parallel aggregator) that should be incorporated into the final
        /// result; otherwise, the object is a value extracted from an
        /// <see cref="IInvocableCacheEntry"/>.
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
                Decimal result = m_result;

                if (isFinal)
                {
                    // aggregate partial results packed into a byte array
                    byte[]     buff   = (byte[]) o;
                    DataReader reader = new DataReader(new MemoryStream(buff));

                    int c = reader.ReadInt32();
                    if (c > 0)
                    {
                        Decimal dec = PofHelper.ReadDecimal(reader);
                        m_count    += c;
                        m_result    = Decimal.Add(result, dec);
                    }
                }
                else
                {
                    Decimal dec = EnsureDecimal(o);

                    // collect partial results
                    m_count++;
                    m_result = Decimal.Add(result, dec);
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
            int     c      = m_count;
            Decimal result = m_result;

            if (isFinal)
            {
                return c == 0 ? Decimal.Zero : Decimal.Divide(result, Convert.ToDecimal(c));
            }
            else
            {
                // return partial aggregation data packed into a byte array
                byte[]     buff   = new byte[32];
                DataWriter writer = new DataWriter(new MemoryStream(buff));

                writer.Write(c);
                if (c > 0)
                {
                    PofHelper.WriteDecimal(writer, result);
                }

                return buff;
            }
        }

        #endregion
    }
}