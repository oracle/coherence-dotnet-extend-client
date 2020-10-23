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
    /// Null-safe delegating comparator.
    /// </summary>
    /// <remarks>
    /// <p>
    /// <c>null</c> values are evaluated as "less then" any non-null value.
    /// If the wrapped comparator is not specified then all non-null values
    /// must implement the <b>IComparable</b> interface.</p>
    /// <p>
    /// Use SafeComparer.Instance to obtain an instance of non-delegating
    /// SafeComparer.</p>
    /// </remarks>
    /// <author>Gene Gleyzer  2002.12.10</author>
    /// <author>Ana Cikic  2006.09.12</author>
    public class SafeComparer : IComparer, IQueryCacheComparer, IEntryAwareComparer, IPortableObject
    {
        #region Properties

        /// <summary>
        /// The wrapped <b>IComparer</b>.
        /// </summary>
        /// <value>
        /// The wrapped <b>IComparer</b>.
        /// </value>
        public virtual IComparer Comparer
        {
            get { return m_comparer; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SafeComparer()
        {}

        /// <summary>
        /// Construct a SafeComparer delegating to the specified (wrapped)
        /// comparer.
        /// </summary>
        /// <param name="comparer">
        /// <b>IComparer</b> object to delegate comparison of non-null values
        /// (optional).
        /// </param>
        public SafeComparer(IComparer comparer)
        {
            m_comparer = comparer;
        }

        #endregion

        #region IComparer implementation

        /// <summary>
        /// Compares its two arguments for order.
        /// </summary>
        /// <remarks>
        /// Returns a negative integer, zero, or a positive integer as the
        /// first argument is less than, equal to, or greater than the
        /// second. <c>null</c> values are evaluated as "less then" any
        /// non-null value. If the wrapped comparer is not specified, all
        /// non-null values must implement the <b>IComparable</b> interface.
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
        /// <exception cref="InvalidCastException">
        /// If the arguments' types prevent them from being compared by this
        /// <b>IComparer</b>.
        /// </exception>
        public virtual int Compare(object o1, object o2)
        {
            return CompareSafe(Comparer, o1, o2);
        }

        #endregion

        #region IQueryDictionaryComparer implementation

        /// <summary>
        /// Compare two entries based on the rules specified by <b>IComparer</b>.
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
        /// than the value denoted by the second entry .
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
        public virtual int CompareEntries(IQueryCacheEntry entry1, IQueryCacheEntry entry2)
        {
            IComparer comparer = Comparer;
            return comparer is IQueryCacheComparer ?
                   ((IQueryCacheComparer) comparer).CompareEntries(entry1, entry2)
                   : CompareSafe(comparer, entry1.Value, entry2.Value);
        }

        #endregion

        #region IEntryAwareComparer implementation

        /// <summary>
        /// Specifies whether this comparer expects to compare keys or 
        /// values.
        /// </summary>
        /// <returns>
        /// <b>true</b> if entry keys are expected; <b>false</b> otherwise.
        /// </returns>
        public bool IsKeyComparer()
        {
            return IsKeyComparer(Comparer);
        }

        /// <summary>
        /// Check whether the specified comparer expects to compare keys or
        /// values.
        /// </summary>
        /// <param name="comparer">
        /// A <b>IComparer</b> to check.
        /// </param>
        /// <returns>
        /// <b>true</b> if the comparer expects keys; <b>false</b> otherwise.
        /// </returns>
        public static bool IsKeyComparer(IComparer comparer)
        {
            return comparer is KeyExtractor ||
                   comparer is IEntryAwareComparer &&
                   ((IEntryAwareComparer) comparer).IsKeyComparer();
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
            m_comparer = (IComparer) reader.ReadObject(0);
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
            writer.WriteObject(0, m_comparer);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Returns a human-readable description for this <b>IComparer</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>IComparer</b>.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + " (" + m_comparer + ')';
        }

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
            return o is SafeComparer && Equals(m_comparer, ((SafeComparer) o).m_comparer);
        }

        /// <summary>
        /// Return the hash code for this comparator.
        /// </summary>
        /// <returns>
        /// The hash code value for this comparator.
        /// </returns>
        public override int GetHashCode()
        {
            IComparer comparer = m_comparer;
            return comparer == null ? 17 : comparer.GetHashCode();
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Compares its two arguments for order.
        /// </summary>
        /// <remarks>
        /// Returns a negative integer, zero, or a positive integer as the
        /// first argument is less than, equal to, or greater than the
        /// second. Null values are evaluated as "less then" any non-null
        /// value. Non-null values that do not implement <b>IComparable</b>
        /// interface will be evaluated as equal.
        /// </remarks>
        /// <param name="comparer">
        /// A comparer to use for the comparison (optional).
        /// </param>
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
        /// <exception cref="ArgumentException">
        /// If the arguments are not of the same type or do not implement
        /// <b>IComparable</b>.
        /// </exception>
        public static int CompareSafe(IComparer comparer, object o1, object o2)
        {
            if (comparer != null)
            {
                try
                {
                    return comparer.Compare(o1, o2);
                }
                catch (NullReferenceException) {}
            }

            if (o1 == null)
            {
                return o2 == null ? 0 : -1;
            }

            if (o2 == null)
            {
                return +1;
            }

            if (o1 is IComparable)
            {
                return ((IComparable) o1).CompareTo(o2);
            }
            else
            {
                throw new ArgumentException("Object does not implement IComparable.");
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// The trivial SafeComparer.
        /// </summary>
        public static readonly SafeComparer Instance = new SafeComparer();

        #endregion

        #region Data members

        /// <summary>
        /// The wrapped IComparer.
        /// </summary>
        protected IComparer m_comparer;

        #endregion
    }
}