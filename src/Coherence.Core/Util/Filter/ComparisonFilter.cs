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

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which compares the result of a member
    /// invocation with a value.
    /// </summary>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.10.27</author>
    /// <author>Goran Milosavljevic  2006.10.20</author>
    /// <author>Tom Beerbower  2009.03.09</author>
    public abstract class ComparisonFilter : ExtractorFilter
    {
        #region Properties

        /// <summary>
        /// Gets the object to compare the extraction result with.
        /// </summary>
        /// <value>
        /// The object to compare the extraction result with.
        /// </value>
        public virtual object Value
        {
            get { return m_value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected ComparisonFilter()
        {}

        /// <summary>
        /// Construct a ComparisonFilter.
        /// </summary>
        /// <param name="extractor">
        /// The <b>IValueExtractor</b> to use by this filter.
        /// </param>
        /// <param name="value">
        /// The object to compare the result with.
        /// </param>
        protected ComparisonFilter(IValueExtractor extractor, object value)
            : base(extractor)
        {
            m_value = value;
        }

        /// <summary>
        /// Construct a ComparisonFilter.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="value">
        /// The object to compare the result with.
        /// </param>
        protected ComparisonFilter(string member, object value)
            : base(member)
        {
            m_value = value;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>ComparisonFilter</b> with another object to
        /// determine equality.
        /// </summary>
        /// <remarks>
        /// Two <b>ComparisonFilter</b> objects are considered equal if they
        /// are of the same type and their Extractor and Value are equal.
        /// </remarks>
        /// <param name="o">
        /// The <b>ComparisonFilter</b> to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if this <b>ComparisonFilter</b> and the passed object
        /// are equivalent <b>ComparisonFilter</b> objects.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is ComparisonFilter)
            {
                var that = (ComparisonFilter) o;
                return GetType() == that.GetType() 
                    && Equals(m_extractor, that.m_extractor) 
                    && Equals(m_value, that.m_value);
            }

            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>ComparisonFilter</b> object
        /// according to the general <b>object.GetHashCode()</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>ComparisonFilter</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            object value = m_value;
            return m_extractor.GetHashCode() + (value == null ? 0 : value.GetHashCode());
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>ComparisonFilter</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>ComparisonFilter</b>.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + '(' + ValueExtractor + ", " + Value + ')';
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
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            m_value = reader.ReadObject(1);
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
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteObject(1, m_value);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper method to calculate effectiveness for ComparisonFilters that 
        /// need no more than a single index match in order to retrieve all 
        /// necessary keys to perform the applyIndex() operation.
        /// Such filters are: Contains, Equals, NotEquals.
        /// </summary>
        /// <param name="indexes">
        /// The available MapIndex objects keyed by the related IValueExtractor; 
        /// read-only
        /// </param>
        /// <param name="keys">
        /// The set of keys that will be filtered; read-only
        /// </param>
        /// <returns>
        /// An effectiveness estimate of how well this filter can use the
        /// specified indexes to filter the specified keys
        /// </returns>
        protected int CalculateMatchEffectiveness(IDictionary indexes, ICollection keys)
        {
            var index = (ICacheIndex) indexes[ValueExtractor];
            return index == null ? CalculateIteratorEffectiveness(keys.Count) : 1;
        }

        /// <summary>
        /// Helper method to calculate effectiveness for ComparisonFilters that 
        /// need a range of values from an index in order to retrieve all 
        /// necessary keys to perform the applyIndex() operation.
        /// Such filters are: Less, LessEquals, Greater, GreaterEquals.
        /// </summary>
        /// <param name="indexes">
        /// The available MapIndex objects keyed by the related IValueExtractor; 
        /// read-only
        /// </param>
        /// <param name="keys">
        /// The set of keys that will be filtered; read-only
        /// </param>
        /// <returns>
        /// An effectiveness estimate of how well this filter can use the
        /// specified indexes to filter the specified keys
        /// </returns>
        protected int CalculateRangeEffectiveness(IDictionary indexes, ICollection keys)
        {
            var index = (ICacheIndex) indexes[ValueExtractor];
            if (index == null)
            {
                return CalculateIteratorEffectiveness(keys.Count);
            }
            if (index.IsOrdered)
            {
                // TODO we could be more precise if the position of the value
                // in the SortedMap could be quickly calculated
                return Math.Max(index.IndexContents.Count / 4, 1);
            }
            return index.IndexContents.Count;
        }

        /// <summary>
        /// Helper method to calculate effectiveness (or rather ineffectiveness) of
        /// a simple iteration against a key set that has to be performed due to an
        /// absence of corresponding index.
        /// </summary>
        /// <param name="cKeys">
        /// The number of keys to iterate through.
        /// </param>
        /// <returns>
        /// The cost of the iteration.
        /// </returns>
        public static int CalculateIteratorEffectiveness(int cKeys)
        {
            // convert int to long to prevent integer overflow
            long lCost = ((long) EVAL_COST) * cKeys;
            return lCost <= Int32.MaxValue ? (int) lCost : Int32.MaxValue;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The value to compare to.
        /// </summary>
        protected internal object m_value;

        #endregion
    }
}