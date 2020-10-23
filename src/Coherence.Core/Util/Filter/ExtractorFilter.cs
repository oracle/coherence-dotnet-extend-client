/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Diagnostics;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// Base <see cref="IFilter"/> implementation for doing extractor-based
    /// processing.
    /// </summary>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.11.01</author>
    /// <author>Goran Milosavljevic  2006.10.20</author>
    public abstract class ExtractorFilter : IEntryFilter, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Obtain the <see cref="IValueExtractor"/> used by this filter.
        /// </summary>
        /// <value>
        /// The <b>IValueExtractor</b> used by this filter.
        /// </value>
        public virtual IValueExtractor ValueExtractor
        {
            get { return m_extractor; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected ExtractorFilter()
        {}

        /// <summary>
        /// Construct a ExtractorFilter for a given
        /// <see cref="IValueExtractor"/>.
        /// </summary>
        /// <param name="extractor">
        /// The <b>IValueExtractor</b> to use by this filter.
        /// </param>
        protected ExtractorFilter(IValueExtractor extractor)
        {
            Debug.Assert(extractor != null);
            m_extractor = extractor;
        }

        /// <summary>
        /// Construct an ExtractorFilter for a given member name.
        /// </summary>
        /// <param name="member">
        /// A member name to make a <see cref="ReflectionExtractor"/> for;
        /// this parameter can also be a dot-delimited sequence of member
        /// names which would result in an ExtractorFilter based on the
        /// <see cref="ChainedExtractor"/> that is based on an array of
        /// corresponding <b>ReflectionExtractor</b> objects.
        /// </param>
        protected ExtractorFilter(string member)
        {
            m_extractor = member.IndexOf('.') < 0
                              ? new ReflectionExtractor(member)
                              : (IValueExtractor) new ChainedExtractor(member);
        }

        #endregion

        #region IFilter implementation

        /// <summary>
        /// Apply the test to the object.
        /// </summary>
        /// <param name="o">
        /// An object to which the test is applied.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        public virtual bool Evaluate(object o)
        {
            return EvaluateExtracted(Extract(o));
        }

        #endregion

        #region IEntryFilter implementation

        /// <summary>
        /// Apply the test to an <see cref="ICacheEntry"/>.
        /// </summary>
        /// <param name="entry">
        /// The <b>ICacheEntry</b> to evaluate; never <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        public virtual bool EvaluateEntry(ICacheEntry entry)
        {
            IValueExtractor extractor = ValueExtractor;
            return EvaluateExtracted(entry is IQueryCacheEntry
                    ? ((IQueryCacheEntry) entry).Extract(extractor)
                    : InvocableCacheHelper.ExtractFromEntry(extractor, entry));
        }

        #endregion

        #region ExtractorFilter methods

        /// <summary>
        /// Evaluate the specified extracted value.
        /// </summary>
        /// <param name="extracted">
        /// An extracted value to evaluate.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        protected internal abstract bool EvaluateExtracted(object extracted);

        /// <summary>
        /// Gets the result of <see cref="IValueExtractor"/> invocation.
        /// </summary>
        /// <param name="o">
        /// The object on which to invoke the <b>IValueExtractor</b>;
        /// must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The result of the method invocation.
        /// </returns>
        protected internal virtual object Extract(object o)
        {
            return ValueExtractor.Extract(o);
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
            m_extractor = (IValueExtractor) reader.ReadObject(0);
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
        }

        #endregion

        #region Constants

        /// <summary>
        /// The evaluation cost as a factor to the single index access operation.
        /// </summary>
        public static readonly int EVAL_COST = 1000;
        
        #endregion
        
        #region Data members

        /// <summary>
        /// The ValueExtractor used by this filter.
        /// </summary>
        protected internal IValueExtractor m_extractor;

        #endregion
    }
}