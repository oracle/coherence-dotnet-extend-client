/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace Tangosol.Util.Collections
{
    /// <summary>
    /// <see cref="IDictionary"/>-based <see cref="ICollection"/> implementation 
    /// that contains no duplicate elements.
    /// </summary>
    /// <author>Jason Howes  2010.09.30</author>
    /// <author>Luk Ho  2012.08.27</author>
    public class DictionarySet : ICollection, ISerializable
    {
        #region Constructors

        /// <summary>
        /// Default constructor. By default, HashDictionary is the underlying
        /// Dictionary. If that is not the desired behavior, then pass an
        /// explicit Dictionary to the <c>DictionarySet</c> constructor. To
        /// change the default Dictionary implementation, sub-class the
        /// DictionarySet and override the
        /// <see cref="M:Tangosol.Util.Collections.DictionarySet.InstantiateDictionary"/>
        /// method.
        /// </summary>
        public DictionarySet() 
        {
            m_dict = InstantiateDictionary();
        }

        /// <summary>
        /// Create and populate a new <c>DictionarySet</c> with the given
        /// collection of elements.
        /// </summary>
        /// <param name="items">The collection of elements to populate the set
        /// with.</param>
        public DictionarySet(ICollection items)
            : this()
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Create a new <c>DictionarySet</c> that uses the specified IDictionary to
        /// store its elements.
        /// </summary>
        /// <param name="dict">The storage dictionary.</param>
        protected internal DictionarySet(IDictionary dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException("dict");
            }
            m_dict = dict;
        }

        /// <summary>
        /// Initializes a new instance of the <c>DictionarySet</c> class using the
        /// specified 
        /// <see cref="T:System.Runtime.Serialization.SerializationInfo"/> 
        /// and <see cref="T:System.Runtime.Serialization.StreamingContext"/>.
        /// </summary>
        /// <param name="info">
        /// A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> 
        /// object containing the information required to initialize this 
        /// <c>DictionarySet</c> instance.
        /// </param>
        /// <param name="context">
        /// A <see cref="T:System.Runtime.Serialization.StreamingContext"/> 
        /// object containing the source and destination of the serialized 
        /// stream associated with this dictionary. 
        /// </param>
        protected DictionarySet(SerializationInfo info, StreamingContext context)
            : this((IDictionary) info.GetValue("dict", typeof(IDictionary)))
        {}

        #endregion

        #region DictionarySet methods

        /// <summary>
        /// Adds an element to the set and returns a value to indicate if the 
        /// element was successfully added.
        /// </summary>
        /// <param name="item">The element to add to the set.</param>
        /// <returns><b>true</b> if the element is added to the set; 
        /// <b>false</b> if the element is already in the set.</returns>
        public virtual bool Add(object item)
        {
            var dict    = m_dict;
            var entered = false;

            if (IsSynchronized)
            {
                Blocking.Enter(dict.SyncRoot);
                entered = true;
            }
            try
            {
                if (dict.Contains(item))
                {
                    return false;
                }
                dict[item] = item == null ? "NULL" : ObjectUtils.NO_VALUE;
                return true;
            }
            finally
            {
                if (entered)
                {
                    Monitor.Exit(dict.SyncRoot);
                }
            }
        }

        /// <summary>
        /// Determine whether the set contains a specific element.
        /// </summary>
        /// <param name="item">The element to locate in the set.</param>
        /// <returns><b>true</b> if the element is found in the set; 
        /// <b>false</b> otherwise</returns>.
        public virtual bool Contains(object item)
        {
            return m_dict[item] != null;
        }

        /// <summary>
        /// Remove the specified element from the set.
        /// </summary>
        /// <param name="item">The element to remove from the set.</param>
        /// <returns><b>true</b> if the element was successfully removed from 
        /// the set; otherwise, <b>false</b>. This method also returns 
        /// <b>false</b> if the specified element is not found in the set.
        /// </returns>
        public virtual bool Remove(object item)
        {
            var dict    = m_dict;
            var entered = false;

            if (IsSynchronized)
            {
                Blocking.Enter(dict.SyncRoot);
                entered = true;
            }
            try
            {
                var contains = dict.Contains(item);
                if (contains)
                {
                    dict.Remove(item);    
                }
                return contains;
            }
            finally
            {
                if (entered)
                {
                    Monitor.Exit(dict.SyncRoot);
                }
            }
        }

        /// <summary>
        /// Remove all elements from the set.
        /// </summary>
        public virtual void Clear()
        {
            m_dict.Clear();
        }

        #endregion

        #region ICollection implementation

        /// <summary>
        /// Return an <b>IEnumerator</b> that iterates through this collection.
        /// </summary>
        /// <returns>
        /// An <b>IEnumerator</b> that iterates through this collection.
        /// </returns>
        public virtual IEnumerator GetEnumerator()
        {
            return m_dict.Keys.GetEnumerator();
        }

        /// <summary>
        /// Copy elements from this collection into the one-dimensional array.
        /// </summary>
        /// <param name="array">
        /// An array to which elements should be copied.
        /// </param>
        /// <param name="arrayIndex">
        /// Index in <paramref name="array"/> at which copying should start.
        /// </param>
        public virtual void CopyTo(Array array, int arrayIndex)
        {
            m_dict.Keys.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements in this collection.
        /// </summary>
        /// <value>
        /// The number of elements in this collection.
        /// </value>
        public virtual int Count
        {
            get { return m_dict.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether access to this collection is
        /// thread-safe.
        /// </summary>
        /// <value>
        /// <b>true</b> if this collection is thread-safe; <b>false</b>
        /// otherwise.
        /// </value>
        public virtual bool IsSynchronized
        {
            get { return m_dict.IsSynchronized; }
        }

        /// <summary>
        /// Get an object that can be used to synchronize access to this 
        /// collection.
        /// </summary>
        /// <value>
        /// An object that is used to synchronize access to this collection.
        /// </value>
        public virtual object SyncRoot
        {
            get { return m_dict.SyncRoot; }
        }

        #endregion

        #region ISerializable implementation

        /// <summary>
        /// Populates SerializationInfo with the data needed to serialize this 
        /// object.
        /// </summary>
        /// <param name="info">
        /// The SerializationInfo to populate with data. 
        /// </param>
        /// <param name="context">
        /// The serialization context. 
        /// </param>
        /// <exception cref="T:System.Security.SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("dict", m_dict);
        }

        #endregion

        #region Object method overrides

        /// <summary>
        /// Determines whether the specified object is equal to this object.
        /// </summary>
        /// <returns>
        /// true if the specified object is equal to this object; 
        /// otherwise, false.
        /// </returns>
        /// <param name="obj">
        /// The object to compare with this object. 
        /// </param>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is DictionarySet)
            {
                return Equals((DictionarySet) obj);
            }
            return false;
        }

        /// <summary>
        /// Compares this set with another set for equality.
        /// </summary>
        /// <remarks>
        /// This method returns true if this set and the specified set have 
        /// exactly the same contents.
        /// </remarks>
        /// <param name="set">
        /// Set to compare this set with.
        /// </param>
        /// <returns>
        /// <c>true</c> if the two sets are equal; <c>false</c> otherwise.
        /// </returns>
        public virtual bool Equals(DictionarySet set)
        {
            if (Count != set.Count)
            {
                return false;
            }
            foreach (var o in set)
            {
                if (!Contains(o))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a hash code for this object. 
        /// </summary>
        /// <returns>
        /// A hash code for this object.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 0;
                foreach (var o in this)
                {
                    hashCode += ((o == null ? 0 : o.GetHashCode()) * 397);
                }
                return hashCode;
            }
        }

        /// <summary>
        /// Returns string representation of this instance.
        /// </summary>
        /// <returns>
        /// String representation of this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(GetType().Name).Append("{");

            var separator = "";
            foreach (var o in this)
            {
                sb.Append(separator).Append(o);
                separator = ", ";
            }
            return sb.Append("}").ToString();
        }

        #endregion

        #region Internal

        /// <summary>
        /// Factory pattern: Provide an underlying dictionary for this Set
        /// implementation.
        /// </summary>
        /// <returns>
        /// A new dictionary instance.
        /// </returns>
        protected virtual IDictionary InstantiateDictionary()
        {
            IDictionary dict = m_dict;
            if (dict == null)
            {
                return new HashDictionary();
            }
            try
            {
                return (IDictionary) ObjectUtils.CreateInstance(dict.GetType());
            }
            catch (Exception e)
            {
                throw new Exception("Unable to instantiate IDictionary class: "
                    + dict.GetType().Name, e);
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// IDictionary that stores the elements of this DictionarySet as its keys.
        /// </summary>
        private readonly IDictionary m_dict;

        #endregion
    }
}
