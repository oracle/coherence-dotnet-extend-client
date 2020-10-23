/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.Diagnostics;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util.Collections;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which checks whether the result of a member
    /// invocation belongs to a predefined collection of values.
    /// </summary>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.11.08</author>
    /// <author>Goran Milosavljevic  2006.10.23</author>
    /// <author>Tom Beerbower  2009.03.09</author>
    public class InFilter : ComparisonFilter, IIndexAwareFilter
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InFilter()
        {}

        /// <summary>
        /// Construct an InFilter for testing "In" condition.
        /// </summary>
        /// <param name="extractor">
        /// The <see cref="IValueExtractor"/> to use by this filter.
        /// </param>
        /// <param name="values">
        /// The collection of values to compare the result with.
        /// </param>
        public InFilter(IValueExtractor extractor, ICollection values)
            : base(extractor, new ImmutableArrayList(CollectionUtils.ToArray(values)))
        {
            Debug.Assert(values != null);
        }

        /// <summary>
        /// Construct an <b>InFilter</b> for testing "In" condition.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="values">
        /// The collection of values to compare the result with.
        /// </param>
        public InFilter(string member, ICollection values)
            : base(member, new ImmutableArrayList(CollectionUtils.ToArray(values)))
        {
            Debug.Assert(values != null, "Null collection");
        }

        #endregion

        #region ExtractorFilter override methods

        /// <summary>
        /// Evaluate the specified extracted value.
        /// </summary>
        /// <param name="extracted">
        /// An extracted value to evaluate.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        protected internal override bool EvaluateExtracted(object extracted)
        {
            return CollectionUtils.Contains((ICollection) Value, extracted);
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
            m_extractor = (IValueExtractor) reader.ReadObject(0);
            m_value     = reader.ReadCollection(1, (ICollection) m_value);
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
            writer.WriteObject    (0, m_extractor);
            writer.WriteCollection(1, (ICollection) m_value);
        }

        #endregion

        #region IIndexAwareFilter implementation

        /// <summary>
        /// Given an IDictionary of available indexes, determine if this 
        /// IIndexAwareFilter can use any of the indexes to assist in its 
        /// processing, and if so, determine how effective the use of that 
        /// index would be.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The returned value is an effectiveness estimate of how well this 
        /// filter can use the specified indexes to filter the specified 
        /// keys. An operation that requires no more than a single access to 
        /// the index content (i.e. Equals, NotEquals) has an effectiveness of 
        /// <b>one</b>. Evaluation of a single entry is assumed to have an 
        /// effectiveness that depends on the index implementation and is 
        /// usually measured as a constant number of the single operations.  
        /// This number is referred to as <i>evaluation cost</i>.
        /// </p>
        /// <p>
        /// If the effectiveness of a filter evaluates to a number larger 
        /// than the keys.size() then a user could avoid using the index and 
        /// iterate through the keys calling <tt>Evaluate</tt> rather than 
        /// <tt>ApplyIndex</tt>.
        /// </p>
        /// </remarks>
        /// <param name="indexes">
        /// The available <see cref="ICacheIndex"/> objects keyed by the 
        /// related IValueExtractor; read-only.
        /// </param>
        /// <param name="keys">
        /// The set of keys that will be filtered; read-only.
        /// </param>
        /// <returns>
        /// An effectiveness estimate of how well this filter can use the 
        /// specified indexes to filter the specified keys.
        /// </returns>
        public int CalculateEffectiveness(IDictionary indexes, ICollection keys)
        {
            var index = (ICacheIndex) indexes[ValueExtractor];
            return index == null ? CalculateIteratorEffectiveness(keys.Count)
                                 : ((ICollection) Value).Count;
        }

        /// <summary>
        /// Filter remaining keys using an IDictionary of available indexes.
        /// </summary>
        /// <remarks>
        /// The filter is responsible for removing all keys from the passed 
        /// set of keys that the applicable indexes can prove should be 
        /// filtered. If the filter does not fully evaluate the remaining 
        /// keys using just the index information, it must return a filter
        /// (which may be an <see cref="IEntryFilter"/>) that can complete the 
        /// task using an iterating implementation. If, on the other hand, the
        /// filter does fully evaluate the remaining keys using just the index
        /// information, then it should return <c>null</c> to indicate that no 
        /// further filtering is necessary.
        /// </remarks>
        /// <param name="indexes">
        /// The available <see cref="ICacheIndex"/> objects keyed by the 
        /// related IValueExtractor; read-only.
        /// </param>
        /// <param name="keys">
        /// The mutable set of keys that remain to be filtered.
        /// </param>
        /// <returns>
        /// An <see cref="IFilter"/> object that can be used to process the 
        /// remaining keys, or <c>null</c> if no additional filter processing 
        /// is necessary.
        /// </returns>
        public IFilter ApplyIndex(IDictionary indexes, ICollection keys)
        {
            var index = (ICacheIndex) indexes[ValueExtractor];
            if (index == null)
            {
                // there is no relevant index
                return this;
            }

            var values = (ICollection) Value;
            var setIn  = new HashSet();

            foreach (object oValue in values)
            {
                var colEQ = (ICollection) index.IndexContents[oValue];
                if (colEQ != null)
                {
                    CollectionUtils.AddAll(setIn, colEQ);
                }
            }

            if (setIn.Count == 0)
            {
                CollectionUtils.Clear(keys);
            }
            else
            {
                CollectionUtils.RetainAll(keys, setIn);
            }
            return null;
        }

        #endregion
    }
}