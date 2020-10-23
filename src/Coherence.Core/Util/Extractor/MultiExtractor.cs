/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Util.Aggregator;
using Tangosol.Util.Comparator;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// Composite <see cref="IValueExtractor"/> implementation based on an
    /// array of extractors.
    /// </summary>
    /// <remarks>
    /// <p>
    /// All extractors in the array are applied to the same target object and
    /// the result of the extraction is an <b>IList</b> of extracted values.
    /// </p>
    /// <p>
    /// Common scenarios for using the <b>MultiExtractor</b> involve the
    /// <see cref="DistinctValues"/> or <see cref="GroupAggregator"/>
    /// aggregators, that allow clients to collect all distinct combinations
    /// of a given set of attributes or collect and run additional
    /// aggregation against the corresponding groups of entries.</p>
    /// </remarks>
    /// <author>Gene Gleyzer 2006.02.08</author>
    /// <author>Ivan Cikic 2006.10.20</author>
    /// <since>Coherence 3.2</since>
    public class MultiExtractor : AbstractCompositeExtractor
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MultiExtractor()
        {}

        /// <summary>
        /// Construct a <b>MultiExtractor</b>.
        /// </summary>
        /// <param name="extractors">
        /// The <see cref="IValueExtractor"/> array.
        /// </param>
        public MultiExtractor(IValueExtractor[] extractors) : base(extractors)
        {}

        /// <summary>
        /// Construct a <b>MultiExtractor</b> for a specified member name
        /// list.
        /// </summary>
        /// <param name="memberNames">
        /// A comma-delimited sequence of member names which results in a
        /// <b>MultiExtractor</b> that is based on a corresponding array of
        /// <see cref="IValueExtractor"/> objects; individual array elements
        /// will be either <see cref="ReflectionExtractor"/> or
        /// <see cref="ChainedExtractor"/> objects
        /// </param>
        public MultiExtractor(string memberNames) : base(CreateExtractors(memberNames))
        {}

        #endregion

        #region AbstractExtractor methods

        /// <summary>
        /// Extract a collection of values from the passed object using the
        /// underlying array of <see cref="IValueExtractor"/> objects.
        /// </summary>
        /// <remarks>
        /// Note that each individual value could be an object of a standard
        /// wrapper type (for intrinsic types) or <c>null</c>.
        /// </remarks>
        /// <param name="target">
        /// An <b>Object</b> to retrieve the collection of values from.
        /// </param>
        /// <returns>
        /// A List containing the extracted values or <c>null</c> if the target
        /// object itself is <c>null</c>.
        /// </returns>
        public override object Extract(object target)
        {
            if (target == null)
            {
                return null;
            }

            IValueExtractor[] extractors = Extractors;
            int               count      = extractors.Length;
            object[]          values     = new object[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = extractors[i].Extract(target);
            }
            return values;
        }

        /// <summary>
        /// Extract a collection of values from the passed entry using the
        /// underlying array of <b>IValueExtractor</b> objects.
        /// </summary>
        /// <param name="entry">
        /// An entry to retrieve the collection of values from
        /// </param>
        /// <returns>
        /// An array containing extracted values
        /// </returns>
        public override object ExtractFromEntry(ICacheEntry entry)
        {
            IValueExtractor[] extractors = Extractors;
            int               count     = extractors.Length;
            object[]          values    = new object[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = InvocableCacheHelper.ExtractFromEntry(extractors[i], entry);
            }
            return values;
        }

        #endregion

        #region IQueryDictionaryComparator implementation

        /// <summary>
        /// Compare two entries based on the rules specified by
        /// <b>IComparer</b>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If possible, use the <see cref="IQueryCacheEntry.Extract"/>
        /// method to optimize the value extraction process.</p>
        /// <p>
        /// This method is expected to be implemented by <b>IComparer</b>
        /// wrappers, which simply pass on this invocation to the wrapped
        /// <b>IComparer</b> objects if they too implement this interface, or
        /// to invoke their default compare method passing the actual objects
        /// (not the extracted values) obtained from the extractor using the
        /// passed entries.</p>
        /// <p>
        /// This interface is also expected to be implemented by
        /// <see cref="IValueExtractor"/> implementations that implement the
        /// <b>IComparer</b> interface. It is expected that in most cases,
        /// the <b>IComparer</b> wrappers will eventually terminate at (i.e.
        /// delegate to) <b>IValueExtractors</b> that also implement this
        /// interface.</p>
        /// </remarks>
        /// <param name="entry1">
        /// The first entry to compare values from; read-only.
        /// </param>
        /// <param name="entry2">
        /// The second entry to compare values from; read-only.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as the first
        /// entry denotes a value that is is less than, equal to, or greater
        /// than the value denoted by the second entry.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// If the arguments' types prevent them from being compared by this
        /// <b>IComparer</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the extractor cannot handle the passed objects for any other
        /// reason; an implementor should include a descriptive message.
        /// </exception>
        /// <since>Coherence 3.2</since>
        public override int CompareEntries(IQueryCacheEntry entry1, IQueryCacheEntry entry2)
        {
            IValueExtractor[] extractors = Extractors;

            for (int i = 0, c = extractors.Length; i < c; i++)
            {
                IValueExtractor extractor = extractors[i];

                int result = SafeComparer.CompareSafe(null, entry1.Extract(extractor), entry2.Extract(extractor));
                if (result != 0)
                {
                    return result;
                }
            }
            return 0;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Parse a comma-delimited sequence of method names and instantiate
        /// a corresponding array of <see cref="IValueExtractor"/> objects.
        /// </summary>
        /// <remarks>
        /// Individual array elements will be either
        /// <see cref="ReflectionExtractor"/> or
        /// <see cref="ChainedExtractor"/> objects.
        /// </remarks>
        /// <param name="names">
        /// A comma-delimited sequence of method names
        /// </param>
        /// <returns>
        /// An array of <see cref="IValueExtractor"/> objects.
        /// </returns>
        private static IValueExtractor[] CreateExtractors(string names)
        {
            string[]          methods    = names.Split(new char[] {','});
            int               count      = methods.Length;
            IValueExtractor[] extractors = new IValueExtractor[count];

            for (int i = 0; i < count; i++)
            {
                string method = methods[i];
                extractors[i] = method.IndexOf('.') < 0
                                        ? new ReflectionExtractor(method)
                                        : (IValueExtractor) new ChainedExtractor(method);
            }
            return extractors;
        }

        #endregion
    }
}