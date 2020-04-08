/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Comparator
{
    /// <summary>
    /// Composite comparer implementation based on a collection of
    /// comparers.
    /// </summary>
    /// <remarks>
    /// The comparers in the array assumed to be sorted according to their
    /// priorities; only in a case when the n-th comparer cannot determine
    /// the order of the passed objects:
    /// <code>
    ///     comparers[n].Compare(o1, o2) == 0
    /// </code>
    /// the (n+1)-th comparer will be applied to calculate the value.
    /// </remarks>
    /// <author>Gene Gleyzer  2002.11.14</author>
    /// <author>Ivan Cikic 2007.01.29</author>
    public class ChainedComparer : IComparer, IQueryCacheComparer, IEntryAwareComparer, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Obtain the underlying <b>IComparer</b> array.
        /// </summary>
        /// <value>
        /// The <b>IComparer</b> array.
        /// </value>
        public virtual IComparer[] Comparers
        {
            get { return m_comparers; }
        }


        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChainedComparer()
        {}

        /// <summary>
        /// Construct a <b>ChainedComparer</b>.
        /// </summary>
        /// <param name="comparers">
        /// The comparer array.
        /// </param>
        public ChainedComparer(IComparer[] comparers)
        {
            Debug.Assert(comparers != null);
            m_comparers = comparers;
        }

        #endregion

        #region IComparer interface implementation

        /// <summary>
        /// Compares its two arguments for order.
        /// </summary>
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
        public virtual int Compare(object o1, object o2)
        {
            IComparer[] comparers = Comparers;
            for (int i = 0, c = comparers.Length; i < c; i++)
            {
                int result = comparers[i].Compare(o1, o2);
                if (result != 0)
                {
                    return result;
                }
            }
            return 0;
        }

        #endregion

        #region IQueryCacheComparer interface implementation

        /// <summary>
        /// Compare two entries based on the rules specified by
        /// <b>IComparer</b>.
        /// </summary>
        /// <remarks>
        /// This implementation simply passes on this invocation to the
        /// wrapped <b>IComparer</b> objects if they too implement this
        /// interface, or invokes their default Compare method passing the
        /// values extracted from the passed entries.
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
        public virtual int CompareEntries(IQueryCacheEntry entry1, IQueryCacheEntry entry2)
        {
            IComparer[] comparers = Comparers;
            for (int i = 0, c = comparers.Length; i < c; i++)
            {
                IComparer comparer = comparers[i];
                int       result   = comparer is IQueryCacheComparer
                                         ? ((IQueryCacheComparer) comparer).CompareEntries(entry1, entry2)
                                         : Compare(entry1.Value, entry2.Value);

                if (result != 0)
                {
                    return result;
                }
            }
            return 0;
        }

        #endregion

        #region IEntryAwareComparer implementation

        /// <summary>
        /// Specifies whether this comparer expects to compare keys or 
        /// values.
        /// </summary>
        /// <returns>
        /// <b>true</b> iff all the underlying compares implement the
        /// <see cref="IEntryAwareComparer"/> interface and all 
        /// <code>IsKeyComparator()</code> calls return <b>true</b>.
        /// </returns>
        public bool IsKeyComparer()
        {
            IComparer[] comparers = Comparers;
            for (int i = 0, c = comparers.Length; i < c; i++)
            {
                if (!SafeComparer.IsKeyComparer(comparers[i]))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
        
        #region IPoratebleObject interface implementation

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
            m_comparers = (IComparer[])reader.ReadArray(0);
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
            writer.WriteArray(0, m_comparers);
        }

        #endregion

        #region Object methods overrides

        /// <summary>
        /// Returns a human-readable description for this
        /// <b>ChainedComparer</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>ChainedComparer</b>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("ChainedComparer(");

            IComparer[] comparers = m_comparers;
            for (int i = 0, c = comparers.Length; i < c; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(comparers[i]);
            }
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// Determine if two <b>ChainedComparer</b> objects are equal.
        /// </summary>
        /// <param name="o">
        /// The other comparator.
        /// </param>
        /// <returns>
        /// <b>true</b> if the passed object is equal to this
        /// <b>ChainedComparator</b>.
        /// </returns>
        public override bool Equals(object o)
        {
            return o is ChainedComparer
                   && CollectionUtils.EqualsDeep(m_comparers, ((ChainedComparer) o).m_comparers);
        }


        /// <summary>
        /// Return the hash code for this comparator.
        /// </summary>
        /// <returns>
        /// The hash code value for this comparator.
        /// </returns>
        public override int GetHashCode()
        {
            IComparer[] comparers = m_comparers;
            int         hash      = 0;
            for (int i = 0, c = comparers.Length; i < c; i++)
            {
                hash += comparers[i].GetHashCode();
            }
            return hash;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Empty array of <b>IComparer</b>s.
        /// </summary>
        private static readonly IComparer[] EMPTY_COMPARER_ARRAY = new IComparer[0];

        #endregion

        #region Date members

        /// <summary>
        /// The <b>IComparer</b> array.
        /// </summary>
        protected IComparer[] m_comparers;

        #endregion
    }
}

