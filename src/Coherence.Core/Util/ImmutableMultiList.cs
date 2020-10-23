/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using Tangosol.Util.Collections;

namespace Tangosol.Util
{
    /// <summary>
    /// Implementation of the IList interface in a read-only fashion based on a
    /// collection of arrays.
    /// </summary>
    /// <author>Mark Falco  2009.09.20</author>
    public class ImmutableMultiList : IList
    {
        #region Constructors

        /// <summary>
        /// Construct a List containing the elements of the specified array of
        /// Object arrays.
        /// </summary>
        /// <param name="aao">
        /// the array of arrays backing the MultiList
        /// </param>
        public ImmutableMultiList(Object[][] aao)
        {
            m_total = CalculateTotalLength(aao);
            m_aao    = aao;
        }

        #endregion

        #region Implementation of IEnumerable

        /// <inheritdoc/>
        public IEnumerator GetEnumerator()
        {
            return new MultiEnumerator(m_aao, m_total);
        }

        #endregion

        #region Implementation of ICollection

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }
            Flatten(m_aao, m_total, array, index);
        }

        /// <inheritdoc/>
        public int Count
        {
            get { return m_total; }
        }

        /// <inheritdoc/>
        public object SyncRoot
        {
            get { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public bool IsSynchronized
        {
            get { return false; }
        }

        #endregion

        #region Implementation of IList

        /// <inheritdoc/>
        public int Add(object value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public bool Contains(object value)
        {
            IDictionary set = m_set;
            if (set == null)
            {
                if (m_total < 32)
                {
                    return IndexOf(value) >= 0;
                }
                // We have a decent number of elements and it appears that we're
                // being accessed as a Set. The ImmutableMultiList data-structure
                // is sub-optimal for Set based operations, and thus for large
                // sets we inflate and delegate to a real Set implementation.
                set = new HashDictionary(m_total);
                foreach (Object o in this)
                {
                    set[o] = o;
                }
                m_set = set;
            }
            return set.Contains(value);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public int IndexOf(object value)
        {
            int i = 0;
            Object[][] aao = m_aao;
            for (int iaa = 0, caa = aao.Length; iaa < caa; ++iaa)
            {
                Object[] ao = aao[iaa];
                for (int ia = 0, ca = ao.Length; ia < ca; ++ia, ++i)
                {
                    if (Equals(ao[ia], value))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <inheritdoc/>
        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public void Remove(object value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public object this[int i]
        {
            get
            {
                Object[][] aao = m_aao;
                for (int iaa = 0, caa = aao.Length; iaa < caa; ++iaa)
                {
                    int c = aao[iaa].Length;
                    if (i < c)
                    {
                        return aao[iaa][i];
                    }
                    i -= c;
                }

                throw new ArgumentOutOfRangeException();    
            }
            set { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <inheritdoc/>
        public bool IsFixedSize
        {
            get { return true; }
        }

        #endregion

        #region inner class MultiEnumerator

        /// <summary>
        /// Enumerator implementation based on the ImmutableMultiList.
        /// </summary>
        class MultiEnumerator : IEnumerator
        {
            public MultiEnumerator(Object[][] aao, int cTotal)
            {
                m_aao    = aao;
                m_cTotal = cTotal;
                Reset();
            }

            public bool MoveNext()
            {
                if (++m_i >= m_cTotal)
                    {
                    m_i = m_cTotal;
                    return false;
                    }

                Object[][] aao = m_aao;
                int        ia  = m_ia + 1;
                Object[]   ao  = aao[m_iaa];

                while (ia == ao.Length)
                    {
                    // no more elements in this array; move on to the next
                    // populated array
                    ao = aao[++m_iaa];
                    ia = 0;
                    }
                m_ia = ia;
                return true;
            }

            public void Reset()
            {
                m_iaa = 0;
                m_ia  = m_i = -1;
            }

            public object Current
            {
                get
                {
                    if (m_iaa == -1 || m_ia == -1 || m_i >= m_cTotal)
                    {
                        throw new InvalidOperationException();
                    }

                    return m_aao[m_iaa][m_ia];
                }
            }

            private readonly Object[][] m_aao;    // array of array of elements
            private readonly int        m_cTotal; // total number of elements
            private int                 m_i;      // index of next to be returned
            private int                 m_ia;     // index into current array
            private int                 m_iaa;    // index into array of arrays        
        }
        #endregion

        #region Object implementation

        /// <summary>
        /// Compare this <b>IList</b> implementation with some other Object
        /// and determine if the caller would believe this Object to be equal
        /// to the other Object.
        /// </summary>
        /// <param name="that">
        /// Some other Object that is likely to be an <b>ICollection</b> or
        /// some more specific type (with its related overloaded definition
        /// of what it thinks that equals() means)
        /// </param>
        /// <returns>
        /// <b>true</b> if and only if this Object believes that it can make
        /// a defensible case that this Object is equal to the passed Object.
        /// </returns>
        public override bool Equals(object that)
        {
            if (ReferenceEquals(null, that))
            {
                return false;
            }
            if (ReferenceEquals(this, that))
            {
                return true;
            }
            if (that is IList)
            {
                return Equals((IList)that);
            }
            if (that is ICollection)
            {
                return Equals((ICollection)that);
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified <b>IList</b> is equal to the
        /// current <b>IList</b>.
        /// </summary>
        /// <param name="that"><b>IList</b> to compare to.</param>
        /// <returns>
        /// <b>true</b> if and only if the specified list contains the same
        /// elements in the same order as the current one.
        /// </returns>
        public virtual bool Equals(IList that)
        {
            if (ReferenceEquals(null, that))
            {
                return false;
            }
            if (ReferenceEquals(this, that))
            {
                return true;
            }
            if (Count != that.Count)
            {
                return false;
            }
            IEnumerator e1 = GetEnumerator();
            IEnumerator e2 = that.GetEnumerator();
            while (e1.MoveNext() && e2.MoveNext())
            {
                object o1 = e1.Current;
                object o2 = e2.Current;
                if (!(Equals(o1, o2)))
                {
                    return false;
                }
            }
            return !(e1.MoveNext() || e2.MoveNext());
        }

        /// <summary>
        /// Determines whether the specified <b>ICollection</b> is equal to
        /// the current <b>ICollection</b>.
        /// </summary>
        /// <param name="that"><b>ICollection</b> to compare to.</param>
        /// <returns>
        /// <b>true</b> if the specified collection have the same size and
        /// contains elements 
        /// </returns>
        public virtual bool Equals(ICollection that)
        {
            if (ReferenceEquals(null, that))
            {
                return false;
            }
            if (ReferenceEquals(this, that))
            {
                return true;
            }
            if (Count != that.Count)
            {
                return false;
            }
            foreach (object o in that)
            {
                if (!Contains(o))
                {
                    return false;
                }
            }
            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int nHash = 17;
            foreach (Object o in this)
            {
                if (o != null)
                {
                    nHash += o.GetHashCode();
                }
            }
            return nHash;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Calculate the total number of element in the array of arrays.
        /// </summary>
        /// <param name="aao">
        /// An array of arrays.
        /// </param>
        /// <returns>
        /// The total number of elements.
        /// </returns>
        public static int CalculateTotalLength(Object[][] aao)
        {
            int cnt = 0;
            for (int i = 0, c = aao.Length; i < c; ++i)
            {
                cnt += aao[i].Length;
            }
            return cnt;
        }

        /// <summary>
        /// Create a single dimensional array containing all elements of the
        /// specified array of arrays.
        /// </summary>
        /// <param name="aaoFrom">
        /// An array of arrays to copy from.
        /// </param>
        /// <param name="cTotal">
        /// The total length of the flattened array; pass -1 for it to be
        /// calculated.
        /// </param>
        /// <param name="aoTo">
        /// An array to copy the elements into (optional).
        /// </param>
        /// <param name="iTo">
        /// The position into aoTo at which to start copying.
        /// </param>
        /// <returns>
        /// An array containing all the elements of the array of arrays.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the total length parameter was not sufficient to hold the
        /// flattened array.
        /// </exception>
        public static Array Flatten(Object[][] aaoFrom, int cTotal, Array aoTo, int iTo)
        {
            if (cTotal < 0)
            {
                cTotal = CalculateTotalLength(aaoFrom);
            }
            if (iTo < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (aoTo == null)
            {
                // implied Object[] type
                aoTo = new Object[cTotal + iTo];
            }
            else if (aoTo.Length - iTo < cTotal)
            {
                // if it is not big enough
                throw new ArgumentException();
            }

            for (int i = 0, c = aaoFrom.Length; i < c; ++i)
            {
                Object[] aoNext = aaoFrom[i];
                int cNext = aoNext.Length;
                Array.Copy(aoNext, 0, aoTo, iTo, cNext);
                iTo += cNext;
            }
            return aoTo;
        }
        #endregion

        #region Data members

        /// <summary>
        /// The array of Object arrays.
        /// </summary>
        private readonly Object[][] m_aao;

        /// <summary>
        /// A fully realized Dictionary of this collections contents (as keys).
        /// This is inflated and used for doing set based operations if it is
        /// detected that this collection is large and being accessed as a set.
        /// </summary>
        private IDictionary m_set;

        /// <summary>
        /// The total number of items.
        /// </summary>
        private readonly int m_total;

        #endregion
    }
}
