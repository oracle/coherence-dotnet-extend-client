/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.Net.Cache;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which evaluates the content of a
    /// <see cref="CacheEventArgs"/> values based on the specified value
    /// extractor.
    /// </summary>
    /// <remarks>
    /// This filter evaluates to <b>true</b> only for update events that
    /// change the value of an extracted attribute.
    /// <p />
    /// Example: a filter that evaluates to <b>true</b> if there is an update
    /// to an Employee object that changes a value of the LastName property.
    /// <code>
    /// new ValueChangeEventFilter("LastName");
    /// </code>
    /// </remarks>
    /// <seealso cref="CacheEventFilter" />
    /// <author>Gene Gleyzer  2003.09.30</author>
    /// <author>Goran Milosavljevic  2006.10.24</author>
    /// <since>Coherence 2.3</since>
    public class ValueChangeEventFilter : ExtractorFilter
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ValueChangeEventFilter()
        {}

        /// <summary>
        /// Construct a <b>ValueChangeEventFilter</b> that evaluates
        /// <see cref="CacheEventArgs"/> values based on the specified
        /// extractor.
        /// </summary>
        /// <param name="extractor">
        /// <see cref="IValueExtractor"/> to extract <b>CacheEvent</b>
        /// values.
        /// </param>
        public ValueChangeEventFilter(IValueExtractor extractor)
            : base(extractor)
        {}

        /// <summary>
        /// Construct a <b>ValueChangeEventFilter</b> that evaluates
        /// <see cref="CacheEventArgs"/> values based on the specified
        /// member name.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        public ValueChangeEventFilter(string member)
            : base(member)
        {}

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
        public override bool Evaluate(object o)
        {
            var evt = (CacheEventArgs) o;
            if (evt.EventType == CacheEventType.Updated)
            {
                return !Equals(Extract(evt.OldValue), Extract(evt.NewValue));
            }
            return false;
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
            throw new NotSupportedException();
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>ValueChangeEventFilter</b> with another object
        /// to determine equality.
        /// </summary>
        /// <param name="o">
        /// The <b>ValueChangeEventFilter</b> to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if this <b>ValueChangeEventFilter</b> and the passed
        /// object are equivalent filters.
        /// </returns>
        public  override bool Equals(object o)
        {
            if (o is ValueChangeEventFilter)
            {
                var that = (ValueChangeEventFilter) o;
                return Equals(m_extractor, that.m_extractor);
            }

            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>ValueChangeEventFilter</b>
        /// object according to the general <b>object.GetHashCode()</b>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>ValueChangeEventFilter</b>
        /// object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_extractor.GetHashCode();
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>ValueChangeEventFilter</b>.
        /// </summary>
        /// <returns>
        /// A String description of the <b>ValueChangeEventFilter</b>.
        /// </returns>
        public override string ToString()
        {
            return "ValueChangeEventFilter(extractor=" + ValueExtractor + ')';
        }

        #endregion
    }
}