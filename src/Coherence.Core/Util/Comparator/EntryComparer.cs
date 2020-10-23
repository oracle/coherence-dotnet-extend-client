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
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Comparator
{
    /// <summary>
    /// <b>IComparer</b> implementation used to compare cache entries.
    /// </summary>
    /// <remarks>
    /// Depending on the comparison style this comparator will compare
    /// entries' values, entries' keys or, when the provided comparator is an
    /// instance of <see cref="IQueryCacheComparer"/>, the entries
    /// themselves.
    /// </remarks>
    /// <author>Gene Gleyzer  2002.12.14</author>
    /// <author>Goran Milosavljevic  2006.09.12</author>
    public class EntryComparer : SafeComparer
    {
        #region Properties

        /// <summary>
        /// Obtain the comparison style value utilized by this EntryComparer.
        /// </summary>
        /// <remarks>
        /// The returned value should be one of <see cref="ComparisonStyle"/>
        /// values.
        /// </remarks>
        /// <value>
        /// One of the <see cref="ComparisonStyle"/> values.
        /// </value>
        public virtual ComparisonStyle ComparisonStyle
        {
            get { return m_style; }
        }

        /// <summary>
        /// Check whether or not this EntryComparer uses entries' values to
        /// pass for comparison to the underlying <b>IComparer</b>.
        /// </summary>
        /// <value>
        /// <b>true</b> if entries' values are used for comparison.
        /// </value>
        [Obsolete("As of Coherence 3.4 this property is replaced with CompareValue")]
        public virtual bool IsCompareValue
        {
            get { return CompareValue; }
        }

        /// <summary>
        /// Check whether or not this EntryComparer uses entries' values to
        /// pass for comparison to the underlying <b>IComparer</b>.
        /// </summary>
        /// <value>
        /// <b>true</b> if entries' values are used for comparison.
        /// </value>
        public virtual bool CompareValue
        {
            get { return m_style == ComparisonStyle.Value; }
        }

        /// <summary>
        /// Check whether or not this EntryComparer uses entries' keys to
        /// pass for comparison to the underlying <b>IComparer</b>.
        /// </summary>
        /// <value>
        /// <b>true</b> if entries' keys are used for comparison.
        /// </value>
        [Obsolete("As of Coherence 3.4 this property is replaced with CompareKey")]
        public virtual bool IsCompareKey
        {
            get { return CompareKey; }
        }

        /// <summary>
        /// Check whether or not this EntryComparer uses entries' keys to
        /// pass for comparison to the underlying <b>IComparer</b>.
        /// </summary>
        /// <value>
        /// <b>true</b> if entries' keys are used for comparison.
        /// </value>
        public virtual bool CompareKey
        {
            get { return m_style == ComparisonStyle.Key; }
        }

        /// <summary>
        /// Check whether or not this EntryComparer pass entries themselves
        /// for comparison to the underlying
        /// <see cref="IQueryCacheComparer.CompareEntries"/> method.
        /// </summary>
        /// <value>
        /// <b>true</b> if entries themselves are used for comparison.
        /// </value>
        [Obsolete("As of Coherence 3.4 this property is replaced with CompareEntry")]
        public virtual bool IsCompareEntry
        {
            get { return CompareEntry; }
        }

        /// <summary>
        /// Check whether or not this EntryComparer pass entries themselves
        /// for comparison to the underlying
        /// <see cref="IQueryCacheComparer.CompareEntries"/> method.
        /// </summary>
        /// <value>
        /// <b>true</b> if entries themselves are used for comparison.
        /// </value>
        public virtual bool CompareEntry
        {
            get { return m_style == ComparisonStyle.Entry; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntryComparer()
        {}

        /// <summary>
        /// Construct an EntryComparer to compare entries' values using the
        /// provided <b>IComparer</b> object.
        /// </summary>
        /// <remarks>
        /// The EntryComparer will choose the comparison style based on the
        /// specified comparator type: if the comparer is an instance of the
        /// <see cref="KeyExtractor"/>, the <b>ComparisonStyle.Key</b> style
        /// will be assumed; otherwise, the <b>ComparisonStyle.Value</b>
        /// style is used.
        /// </remarks>
        /// <param name="comparer">
        /// The comparer to use; if not specified the "natural" comparison of
        /// entries' values is used.
        /// </param>
        public EntryComparer(IComparer comparer) : this(comparer, ComparisonStyle.Auto)
        {}

        /// <summary>
        /// Construct an EntryComparer to compare entries using the provided
        /// <b>IComparer</b> object according to the specified comparison
        /// style.
        /// </summary>
        /// <remarks>
        /// If the style is <b>ComparisonStyle.Auto</b> then the comparator
        /// type is checked: if the comparer is an instance of the
        /// <see cref="KeyExtractor"/>, <b>ComparisonStyle.Key</b> style will
        /// be assumed; otherwise, the <b>ComparisonStyle.Value</b> style is used.
        /// </remarks>
        /// <param name="comparer">
        /// The comparer to use; if not specified the "natural" comparison is
        /// used.
        /// </param>
        /// <param name="style">
        /// The comparison style to use; valid values are any of the
        /// <see cref="ComparisonStyle"/> values.
        /// </param>
        public EntryComparer(IComparer comparer, ComparisonStyle style) : base(comparer)
        {
            switch (style)
            {
                case ComparisonStyle.Auto:
                    style = (comparer != null && IsKeyComparer(comparer)) ? ComparisonStyle.Key : ComparisonStyle.Value;
                    break;

                case ComparisonStyle.Value:
                case ComparisonStyle.Key:
                case ComparisonStyle.Entry:
                    break;

                default:
                    throw new ArgumentException("Invalid comparison style: " + style);

            }

            m_style = style;
        }

        #endregion

        #region IComparer implementation

        /// <summary>
        /// Compares two arguments for order.
        /// </summary>
        /// <remarks>
        /// The arguments must be <b>ICacheEntry</b> objects. Depending
        /// on the comparison style, this method will pass either the
        /// entries' values, keys or the entries themselves to the underlying
        /// IComparer.
        /// </remarks>
        /// <param name="o1">
        /// The first object to be compared.
        /// </param>
        /// <param name="o2">
        /// The second object to be compared.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as the first
        /// argument is less than, equal to, or greater than the second.
        /// </returns>
        public override int Compare(object o1, object o2)
        {
            int result;
            ICacheEntry e1 = (ICacheEntry) o1;
            ICacheEntry e2 = (ICacheEntry) o2;

            switch (m_style)
            {
                case ComparisonStyle.Key:
                    result = base.Compare(e1.Key, e2.Key);
                    break;

                case ComparisonStyle.Entry:
                    if ((e1 is IQueryCacheEntry) && (e2 is IQueryCacheEntry))
                    {
                        result = CompareEntries((IQueryCacheEntry) e1, (IQueryCacheEntry) e2);
                    }
                    else
                    {
                        result = base.Compare(e1.Value, e2.Value);
                    }
                    break;

                case ComparisonStyle.Value:
                default:
                    result = base.Compare(e1.Value, e2.Value);
                    break;
            }

            return result;

        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Determine if two comparers are equal.
        /// </summary>
        /// <param name="o">
        /// The other comparer.
        /// </param>
        /// <returns>
        /// <b>true</b> if the passed object is equal to this.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is EntryComparer)
            {
                EntryComparer target = (EntryComparer) o;
                return base.Equals(o) && m_style == target.m_style;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() + 29 * m_style.GetHashCode();
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
        /// <exception cref="IOException">If an I/O error occurs.</exception>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            m_style = (ComparisonStyle) reader.ReadInt32(1);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">If an I/O error occurs.</exception>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteInt32(1, (int) m_style);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The comparison style value utilized by this EntryComparer.
        /// </summary>
        private ComparisonStyle m_style;

        #endregion
    }

    #region Enum: ComparisonStyle

    /// <summary>
    /// Comparison style enum.
    /// </summary>
    public enum ComparisonStyle
    {
        /// <summary>
        /// Indicates that this EntryComparer should choose the comparison
        /// style based on the underying comparer type.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Indicates that this EntryComparer should compare the entries'
        /// values.
        /// </summary>
        Value = 1,

        /// <summary>
        /// Indicates that this EntryComparer should compare the entries'
        /// keys.
        /// </summary>
        Key = 2,

        /// <summary>
        /// Indicates that entries that implement
        /// <see cref="IQueryCacheEntry"/> interface will be compared
        /// using the <see cref="IQueryCacheComparer.CompareEntries"/>
        /// method.
        /// </summary>
        Entry = 3

    }

    #endregion
}