/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Util;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// Composite <see cref="IValueExtractor"/> implementation based on an
    /// array of extractors.
    /// </summary>
    /// <remarks>
    /// The extractors in the array are applied sequentially left-to-right,
    /// so a result of a previous extractor serves as a target object for a
    /// next one.
    /// </remarks>
    /// <author>Gene Gleyzer  2003.09.22</author>
    /// <author>Ivan Cikic  2006.10.20</author>
    public class ChainedExtractor : AbstractCompositeExtractor
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChainedExtractor()
        {}

        /// <summary>
        /// Construct a <b>ChainedExtractor</b> based on a specified
        /// <see cref="IValueExtractor"/> array.
        /// </summary>
        /// <param name="extractors">
        /// The <see cref="IValueExtractor"/> array.
        /// </param>
        public ChainedExtractor(IValueExtractor[] extractors) : base(extractors)
        {}

        /// <summary>
        /// Construct a <b>ChainedExtractor</b> based on two extractors.
        /// </summary>
        /// <param name="extractor1">
        /// The <see cref="IValueExtractor"/>.
        /// </param>
        /// <param name="extractor2">
        /// The <b>IValueExtractor</b>.
        /// </param>
        public ChainedExtractor(IValueExtractor extractor1, IValueExtractor extractor2) :
                base(new IValueExtractor[] {extractor1, extractor2})
        {}

        /// <summary>
        /// Construct a <b>ChainedExtractor</b> for a specified member name
        /// sequence.
        /// </summary>
        /// <param name="member">
        /// A dot-delimited sequence of member names which results in a
        /// <b>ChainedExtractor</b> that is based on an array of
        /// corresponding <see cref="ReflectionExtractor"/> objects.
        /// </param>
        public ChainedExtractor(string member) : base(CreateExtractors(member))
        {}

        #endregion

        #region AbstractExtractor methods

        /// <summary>
        /// Extract the value from the passed object.
        /// </summary>
        /// <remarks>
        /// The underlying extractors are applied sequentially, so a result
        /// of a previous extractor serves as a target object for a next one.
        /// A value of <c>null</c> prevents any further extractions and is
        /// returned immediately.
        /// </remarks>
        /// <param name="target">
        /// An object to retrieve the value from.
        /// </param>
        /// <returns>
        /// The extracted value as an object; <c>null</c> is an acceptable
        /// value.
        /// </returns>
        public override object Extract(object target)
        {
            IValueExtractor[] extractors = Extractors;
            foreach (IValueExtractor ve in extractors)
            {
                if (target == null)
                {
                    break;
                }
                target = ve.Extract(target);
            }
            return target;
        }

        /// <summary>
        /// Extract the value from the passed entry.
        /// </summary>
        /// <remarks>
        /// The underlying extractors are applied sequentially, so a result
        /// of a previous extractor serves as a target object for a next one.
        /// A value of <code>null</code> prevents any further extractions and
        /// is returned immediately.
        /// </remarks>
        /// <param name="entry">
        /// An Entry object to extract a desired value from
        /// </param>
        /// <returns>The extracted value</returns>
        public override object ExtractFromEntry(ICacheEntry entry)
        {
            IValueExtractor[] extractors = Extractors;
            object target = InvocableCacheHelper.ExtractFromEntry(extractors[0], entry);
            for (int i = 1, c = extractors.Length; i < c && target != null; i++)
            {
                target = extractors[i].Extract(target);
            }
            return target;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Parse a dot-delimited sequence of member names and instantiate
        /// a corresponding array of <see cref="ReflectionExtractor"/>
        /// objects.
        /// </summary>
        /// <param name="names">
        /// A dot-delimited sequence of member names.
        /// </param>
        /// <returns>
        /// An array of <see cref="ReflectionExtractor"/> objects.
        /// </returns>
        private static IValueExtractor[] CreateExtractors(string names)
        {
            string[]              methods    = names.Split(new char[] {'.'});
            int                   count      = methods.Length;
            ReflectionExtractor[] extractors = new ReflectionExtractor[count];
            for (int i = 0; i < count; i++)
            {
                extractors[i] = new ReflectionExtractor(methods[i]);
            }
            return extractors;
        }

        #endregion
    }
}