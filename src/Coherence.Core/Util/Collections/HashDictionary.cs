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

namespace Tangosol.Util.Collections
{
    /// <summary>
    /// Hashtable-based IDictionary implementation that allows <c>null</c> 
    /// keys.
    /// </summary>
    /// <remarks>
    /// Note that <c>null</c> keys support intentionally breaks the
    /// <see cref="IDictionary"/> contract, which states that an 
    /// <c>ArgumentNullException</c> should be raised if the key is 
    /// <c>null</c>.
    /// <p/>
    /// However, this is necessary in order to match the behavior in Java and
    ///  C++, where some Map implementations support <c>null</c> keys.
    /// <p/>
    /// Note: This implementation is not thread safe. If you need it to be,
    /// you should wrap it with the <see cref="SynchronizedDictionary"/>.
    /// </remarks>
    /// <author>Aleksandar Seovic  2009.05.28</author>
    /// <since>Coherence 3.5</since>
    [Serializable]
    public class HashDictionary : Hashtable, IEnumerable
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HashDictionary"/>
        /// class using the default initial capacity, load factor, hash code 
        /// provider, and comparer.
        /// </summary>
        public HashDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashDictionary"/> 
        /// class using the specified initial capacity, and the default load 
        /// factor, hash code provider, and comparer.
        /// </summary>
        /// <param name="capacity">
        /// The approximate number of elements the <see cref="HashDictionary"/> 
        /// object can initially contain. 
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="capacity" /> is less than zero. 
        /// </exception>
        public HashDictionary(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashDictionary" /> 
        /// class using the specified initial capacity and load factor, 
        /// and the default hash code provider and comparer.
        /// </summary>
        /// <param name="capacity">
        /// The approximate number of elements the <see cref="HashDictionary"/> 
        /// object can initially contain. 
        /// </param>
        /// <param name="loadFactor">
        /// A number in the range from 0.1 through 1.0 that is multiplied by 
        /// the default value which provides the best performance. The result 
        /// is the maximum ratio of elements to buckets. 
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="capacity" /> is less than zero.-or- 
        /// <paramref name="loadFactor" /> is less than 0.1.-or- 
        /// <paramref name="loadFactor" /> is greater than 1.0. 
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="capacity" /> is causing an overflow.
        /// </exception>
        public HashDictionary(int capacity, float loadFactor)
            : base(capacity, loadFactor)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashDictionary"/> 
        /// class using the specified initial capacity, load factor, and 
        /// <see cref="T:System.Collections.IEqualityComparer" /> object.
        /// </summary>
        /// <param name="capacity">
        /// The approximate number of elements the <see cref="HashDictionary"/> 
        /// object can initially contain. 
        /// </param>
        /// <param name="loadFactor">
        /// A number in the range from 0.1 through 1.0 that is multiplied by 
        /// the default value which provides the best performance. The result 
        /// is the maximum ratio of elements to buckets. 
        /// </param>
        /// <param name="equalityComparer">
        /// The <see cref="T:System.Collections.IEqualityComparer" /> object 
        /// that defines the hash code provider and the comparer to use with 
        /// the <see cref="HashDictionary" />. -or- null to use the default 
        /// hash code provider and the default comparer. The default hash code 
        /// provider is each key's implementation of 
        /// <see cref="M:System.Object.GetHashCode" /> and the default 
        /// comparer is each key's implementation of 
        /// <see cref="M:System.Object.Equals(System.Object)" />. 
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="capacity" /> is less than zero.
        /// -or- <paramref name="loadFactor" /> is less than 0.1.-or- 
        /// <paramref name="loadFactor" /> is greater than 1.0. 
        /// </exception>
        public HashDictionary(int capacity, float loadFactor, 
                              IEqualityComparer equalityComparer)
            : base(capacity, loadFactor, equalityComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashDictionary" /> 
        /// class using the default initial capacity and load factor, and
        /// the specified <see cref="T:System.Collections.IEqualityComparer"/> 
        /// object.
        /// </summary>
        /// <param name="equalityComparer">
        /// The <see cref="T:System.Collections.IEqualityComparer" /> object 
        /// that defines the hash code provider and the comparer to use with 
        /// the <see cref="HashDictionary" />. -or- null to use the default 
        /// hash code provider and the default comparer. The default hash code 
        /// provider is each key's implementation of 
        /// <see cref="M:System.Object.GetHashCode" /> and the default 
        /// comparer is each key's implementation of 
        /// <see cref="M:System.Object.Equals(System.Object)" />. 
        /// </param>
        public HashDictionary(IEqualityComparer equalityComparer)
            : base(equalityComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashDictionary" /> 
        /// class using the specified initial capacity and 
        /// <see cref="T:System.Collections.IEqualityComparer" />, and the 
        /// default load factor.
        /// </summary>
        /// <param name="capacity">
        /// The approximate number of elements the <see cref="HashDictionary"/> 
        /// object can initially contain. 
        /// </param>
        /// <param name="equalityComparer">
        /// The <see cref="T:System.Collections.IEqualityComparer" /> object 
        /// that defines the hash code provider and the comparer to use with 
        /// the <see cref="HashDictionary" />. -or- null to use the default 
        /// hash code provider and the default comparer. The default hash code 
        /// provider is each key's implementation of 
        /// <see cref="M:System.Object.GetHashCode" /> and the default 
        /// comparer is each key's implementation of 
        /// <see cref="M:System.Object.Equals(System.Object)" />. 
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="capacity" /> is less than zero. 
        /// </exception>
        public HashDictionary(int capacity, IEqualityComparer equalityComparer)
            : base(capacity, equalityComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashDictionary"/> 
        /// class by copying the elements from the specified dictionary to 
        /// the new <see cref="HashDictionary" /> object. The new <see 
        /// cref="HashDictionary" /> object has an initial capacity equal to 
        /// the number of elements copied, and uses the default load factor, 
        /// hash code provider, and comparer.
        /// </summary>
        /// <param name="d">
        /// The <see cref="T:System.Collections.IDictionary" /> object to copy 
        /// to a new <see cref="HashDictionary" /> object. 
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="d" /> is null. 
        /// </exception>
        public HashDictionary(IDictionary d)
            : base(d)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashDictionary" /> 
        /// class by copying the elements from the specified dictionary to 
        /// the new <see cref="HashDictionary" /> object. The new <see 
        /// cref="HashDictionary" /> object has an initial capacity equal to 
        /// the number of elements copied, and uses the specified load factor, 
        /// and the default hash code provider and comparer.
        /// </summary>
        /// <param name="d">
        /// The <see cref="T:System.Collections.IDictionary" /> object to copy 
        /// to a new <see cref="HashDictionary" /> object.
        /// </param>
        /// <param name="loadFactor">
        /// A number in the range from 0.1 through 1.0 that is multiplied by 
        /// the default value which provides the best performance. The result 
        /// is the maximum ratio of elements to buckets. 
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="d" /> is null. 
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="loadFactor" /> is less than 0.1.-or- 
        /// <paramref name="loadFactor" /> is greater than 1.0. 
        /// </exception>
        public HashDictionary(IDictionary d, float loadFactor)
            : base(d, loadFactor)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashDictionary" /> 
        /// class by copying the elements from the specified dictionary to 
        /// the new <see cref="HashDictionary" /> object. The new <see 
        /// cref="HashDictionary" /> object has an initial capacity equal to 
        /// the number of elements copied, and uses the default load factor and
        /// the specified <see cref="T:System.Collections.IEqualityComparer"/> 
        /// object.
        /// </summary>
        /// <param name="d">
        /// The <see cref="T:System.Collections.IDictionary" /> object to copy 
        /// to a new <see cref="HashDictionary" /> object.
        /// </param>
        /// <param name="equalityComparer">
        /// The <see cref="T:System.Collections.IEqualityComparer" /> object 
        /// that defines the hash code provider and the comparer to use with 
        /// the <see cref="HashDictionary" />. -or- null to use the default 
        /// hash code provider and the default comparer. The default hash code 
        /// provider is each key's implementation of 
        /// <see cref="M:System.Object.GetHashCode" /> and the default 
        /// comparer is each key's implementation of 
        /// <see cref="M:System.Object.Equals(System.Object)" />. 
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="d" /> is null. 
        /// </exception>
        public HashDictionary(IDictionary d, IEqualityComparer equalityComparer)
            : base(d, equalityComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashDictionary" /> 
        /// class by copying the elements from the specified dictionary to 
        /// the new <see cref="HashDictionary" /> object. The new <see 
        /// cref="HashDictionary" /> object has an initial capacity equal to
        /// the number of elements copied, and uses the specified load factor 
        /// and <see cref="T:System.Collections.IEqualityComparer" /> object.
        /// </summary>
        /// <param name="d">
        /// The <see cref="T:System.Collections.IDictionary" /> object to copy 
        /// to a new <see cref="HashDictionary" /> object.
        /// </param>
        /// <param name="loadFactor">
        /// A number in the range from 0.1 through 1.0 that is multiplied by 
        /// the default value which provides the best performance. The result 
        /// is the maximum ratio of elements to buckets. 
        /// </param>
        /// <param name="equalityComparer">
        /// The <see cref="T:System.Collections.IEqualityComparer" /> object 
        /// that defines the hash code provider and the comparer to use with 
        /// the <see cref="HashDictionary" />. -or- null to use the default 
        /// hash code provider and the default comparer. The default hash code 
        /// provider is each key's implementation of 
        /// <see cref="M:System.Object.GetHashCode" /> and the default 
        /// comparer is each key's implementation of 
        /// <see cref="M:System.Object.Equals(System.Object)" />. 
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="d" /> is null. 
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="loadFactor" /> is less than 0.1.-or- 
        /// <paramref name="loadFactor" /> is greater than 1.0. 
        /// </exception>
        public HashDictionary(IDictionary d, float loadFactor, 
                              IEqualityComparer equalityComparer)
            : base(d, loadFactor, equalityComparer)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="HashDictionary"/> 
        /// class that is serializable using the specified 
        /// <see cref="T:System.Runtime.Serialization.SerializationInfo" /> 
        /// and <see cref="T:System.Runtime.Serialization.StreamingContext" /> 
        /// objects.
        /// </summary>
        /// <param name="info">
        /// A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> 
        /// object containing the information required to serialize the 
        /// <see cref="HashDictionary" /> object.
        /// </param>
        /// <param name="context">
        /// A <see cref="T:System.Runtime.Serialization.StreamingContext" /> 
        /// object containing the source and destination of the serialized 
        /// stream associated with the <see cref="HashDictionary" />. 
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="info" /> is null. 
        /// </exception>
        protected HashDictionary(SerializationInfo info, 
                                 StreamingContext context) 
            : base(info, context)
        {
            bool isNullValueSet = info.GetBoolean("isNullValueSet");
            if (isNullValueSet)
            {
                m_nullValue = info.GetValue("nullValue", typeof(Object));
            }
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="nullValue">
        /// Value of an entry with a null key.
        /// </param>
        /// <param name="dict">
        /// The undelying hashtable.
        /// </param>
        private HashDictionary(object nullValue, IDictionary dict)
            : base(dict)
        {
            m_nullValue = nullValue;
        }

        #endregion

        #region Implementation of IDictionary

        /// <summary>
        /// Determines whether this dictionary contains an entry with the 
        /// specified key.
        /// </summary>
        /// <returns>
        /// true if the dictionary contains an element with the key; 
        /// otherwise, false.
        /// </returns>
        /// <param name="key">
        /// The key to locate in this dictionary.
        /// </param>
        public override bool Contains(object key)
        {
            return key == null ? IsNullValueSet : base.Contains(key);
        }

        /// <summary>
        /// Adds an element with the provided key and value to the 
        /// <see cref="T:System.Collections.IDictionary" /> object.
        /// </summary>
        /// <param name="key">
        /// The <see cref="T:System.Object" /> to use as the key of 
        /// the element to add. 
        /// </param>
        /// <param name="value">
        /// The <see cref="T:System.Object" /> to use as the value of 
        /// the element to add. 
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// An element with the same key already exists in the 
        /// <see cref="T:System.Collections.IDictionary" /> object. 
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.IDictionary" /> is read-only.
        /// -or- The <see cref="T:System.Collections.IDictionary" /> has a 
        /// fixed size. 
        /// </exception>
        public override void Add(object key, object value)
        {
            if (key == null)
            {
                AssertIsWriteable();
                AssertIsVariableSize();
                if (IsNullValueSet)
                {
                    throw new ArgumentException(
                        "Entry with a null key already exists.");
                }

                m_nullValue = value;
            }
            else
            {
                base.Add(key, value);
            }
        }

        /// <summary>
        /// Removes all elements from the 
        /// <see cref="T:System.Collections.IDictionary" /> object.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.IDictionary" /> object is 
        /// read-only. 
        /// </exception>
        public override void Clear()
        {
            base.Clear();
            m_nullValue = ObjectUtils.NO_VALUE;
        }

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"/> 
        /// object for this dictionary.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IDictionaryEnumerator" /> 
        /// object for this dictionary.
        /// </returns>
        public override IDictionaryEnumerator GetEnumerator()
        {
            return new HashDictionaryEnumerator(this, EnumeratorMode.Entries);
        }

        /// <summary>
        /// Removes the element with the specified key from the 
        /// <see cref="T:System.Collections.IDictionary" /> object.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove. 
        /// </param>
        /// <exception cref="T:System.NotSupportedException">
        /// The dictionary is read-only.
        /// -or- The dictionary has a fixed size. 
        /// </exception>
        public override void Remove(object key)
        {
            if (key == null && IsNullValueSet)
            {
                AssertIsWriteable();
                AssertIsVariableSize();

                m_nullValue = ObjectUtils.NO_VALUE;
            }
            else
            {
                base.Remove(key);
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">
        /// The key of the element to get or set. 
        /// </param>
        /// <exception cref="T:System.NotSupportedException">
        /// The property is set and the dictionary is read-only. 
        /// -or- The property is set, <paramref name="key" /> does not exist 
        /// in the collection, and the dictionary has a fixed size.
        /// </exception>
        public override object this[object key]
        {
            get
            {
                if (key == null)
                {
                    return IsNullValueSet ? m_nullValue : null;    
                }
                else
                {
                    return base[key];
                }
            }
            set
            {
                if (key == null)
                {
                    AssertIsWriteable();
                    if (IsFixedSize && !IsNullValueSet)
                    {
                        throw new NotSupportedException(
                            "Dictionary has a fixed size");
                    }

                    m_nullValue = value;
                }
                else
                {
                    base[key] = value;
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.ICollection" /> object 
        /// containing the keys of this dictionary.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.ICollection" /> object 
        /// containing the keys of this dictionary object.
        /// </returns>
        public override ICollection Keys
        {
            get
            {
                ICollection keys = m_keys;
                if (keys == null)
                {
                    m_keys = keys = new KeyCollection(this);
                }
                return keys;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.ICollection" /> object 
        /// containing the values in this dictionary.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.ICollection" /> object 
        /// containing the values in this dictionary object.
        /// </returns>
        public override ICollection Values
        {
            get
            {
                ICollection values = m_values;
                if (values == null)
                {
                    m_values = values = new ValueCollection(this);
                }
                return values;
            }
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object 
        /// that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection

        /// <summary>
        /// Copies the elements of the collection to an array, starting at
        /// a particular array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional array that is the destination of the elements 
        /// copied from the collection. The array must have zero-based 
        /// indexing. 
        /// </param>
        /// <param name="index">
        /// The zero-based index in the array at which copying begins. 
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null. 
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than zero. 
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="array" /> is multidimensional.-or- 
        /// <paramref name="index" /> is equal to or greater than the length 
        /// of <paramref name="array" />.-or- The number of elements in the 
        /// source collection is greater than the available space from 
        /// <paramref name="index" /> to the end of the destination 
        /// <paramref name="array" />. 
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The type of the source collection cannot be cast automatically 
        /// to the type of the destination <paramref name="array" />. 
        /// </exception>
        public override void CopyTo(Array array, int index)
        {
            if (IsNullValueSet)
            {
                foreach (DictionaryEntry entry in this)
                {
                    array.SetValue(entry, index++);
                }
            }
            else
            {
                base.CopyTo(array, index);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the 
        /// <see cref="T:System.Collections.ICollection" />.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the 
        /// <see cref="T:System.Collections.ICollection" />.
        /// </returns>
        public override int Count
        {
            get { return IsNullValueSet ? base.Count + 1 : base.Count; }
        }

        #endregion

        #region Implementation of Hashtable methods

        /// <summary>
        /// Determines whether the <see cref="HashDictionary" /> contains 
        /// a specific key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="HashDictionary" /> contains an element 
        /// with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">
        /// The key to locate in the <see cref="HashDictionary" />. 
        /// </param>
        public override bool ContainsKey(object key)
        {
            return key == null ? IsNullValueSet : base.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the <see cref="HashDictionary" /> contains 
        /// a specific value.
        /// </summary>
        /// <returns>
        /// true if the <see cref="HashDictionary" /> contains an element 
        /// with the specified <paramref name="value" />; otherwise, false.
        /// </returns>
        /// <param name="value">
        /// The value to locate in the <see cref="HashDictionary" />. 
        /// The value can be null. 
        /// </param>
        public override bool ContainsValue(object value)
        {
            return IsNullValueSet && Equals(m_nullValue, value) 
                    || base.ContainsValue(value);
        }

        #endregion

        #region Implementation of ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override object Clone()
        {
            return new HashDictionary(m_nullValue, (IDictionary) base.Clone());
        }

        #endregion

        #region Implementation of ISerializable

        /// <summary>
        /// Overrides serialization method to add support for null value
        /// serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Serialization context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("isNullValueSet", IsNullValueSet);
            if (IsNullValueSet)
            {
                info.AddValue("nullValue", m_nullValue);
            }
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
            if (obj is IDictionary)
            {
                return Equals((IDictionary) obj);
            }
            return false;
        }

        /// <summary>
        /// Compares this dictionary with another dictionary for equality.
        /// </summary>
        /// <remarks>
        /// This method returns true if this dictionary and the specified
        /// dictionary have exactly the same entry set.
        /// </remarks>
        /// <param name="dict">
        /// Dictionary to compare this dictionary with.
        /// </param>
        /// <returns>
        /// <c>true</c> if the two dictionaries are equal; <c>false</c>
        /// otherwise.
        /// </returns>
        public virtual bool Equals(IDictionary dict)
        {
            if (Count != dict.Count)
            {
                return false;
            }
            foreach (DictionaryEntry entry in dict)
            {
                Object value = this[entry.Key];
                if (!Equals(value, entry.Value))
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
                int hashCode = 0;
                for (IDictionaryEnumerator en = GetEnumerator(); en.MoveNext(); )
                {
                    DictionaryEntry entry = en.Entry;
                    Object          key   = entry.Key;
                    Object          val   = entry.Value;

                    hashCode += ((key == null ? 0 : key.GetHashCode()) * 397) ^
                                 (val == null ? 0 : val.GetHashCode());
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
            StringBuilder sb = new StringBuilder();
            sb.Append(GetType().Name).Append("{[");

            String separator = "";
            for (IDictionaryEnumerator en = GetEnumerator(); en.MoveNext(); )
            {
                sb.Append(separator).Append(en.Key).Append(":").Append(en.Value);
                separator = ", ";
            }
            return sb.Append("]}").ToString();
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Returns true is the value for the <c>null</c> key is set,
        /// false otherwise.
        /// </summary>
        /// <value>
        /// true is the value for the <c>null</c> key is set,
        /// false otherwise.
        /// </value>
        protected bool IsNullValueSet
        {
            get { return m_nullValue != ObjectUtils.NO_VALUE; }
        }

        /// <summary>
        /// Returns enumerator from the base class.
        /// </summary>
        /// <returns>Base class enumerator.</returns>
        private IDictionaryEnumerator GetBaseEnumerator()
        {
            return base.GetEnumerator();
        }

        /// <summary>
        /// Throw an exception if this dictionary is fixed size.
        /// </summary>
        protected void AssertIsVariableSize()
        {
            if (IsFixedSize)
            {
                throw new NotSupportedException("Dictionary has a fixed size");
            }
        }

        /// <summary>
        /// Throw an exception if this dictionary is read-only.
        /// </summary>
        protected void AssertIsWriteable()
        {
            if (IsReadOnly)
            {
                throw new NotSupportedException("Dictionary is read-only");
            }
        }

        #endregion

        #region Inner class: HashDictionaryEnumerator

        /// <summary>
        /// Enumerator mode.
        /// </summary>
        private enum EnumeratorMode
        {
            Entries,
            Keys,
            Values
        }

        /// <summary>
        /// Internal enumerator implementation.
        /// </summary>
        private class HashDictionaryEnumerator : IDictionaryEnumerator
        {
            #region Constructors

            /// <summary>
            /// Initializes a new instance of the 
            /// <see cref="HashDictionaryEnumerator" /> class.
            /// </summary>
            public HashDictionaryEnumerator(HashDictionary dict, 
                                            EnumeratorMode mode)
            {
                m_dict       = dict;
                m_enumerator = dict.GetBaseEnumerator();
                m_mode       = mode;
            }

            #endregion

            #region Implementation of IEnumerator

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next 
            /// element; false if the enumerator has passed the end of the 
            /// collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created. 
            /// </exception>
            public bool MoveNext()
            {
                bool           fHasNext;
                HashDictionary dict = m_dict;

                if (m_fBeforeFirst && dict.IsNullValueSet)
                {
                    fHasNext = true;
                    m_currentEntry = new DictionaryEntry(null, dict.m_nullValue);
                }
                else
                {
                    fHasNext = m_enumerator.MoveNext();
                    if (fHasNext)
                    {
                        m_currentEntry = m_enumerator.Entry;
                    }
                    else
                    {
                        m_fAfterLast = true;
                    }
                }

                m_fBeforeFirst = false;
                return fHasNext;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before 
            /// the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created. 
            /// </exception>
            public void Reset()
            {
                m_fBeforeFirst = true;
                m_fAfterLast   = false;
                m_enumerator.Reset();
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <returns>
            /// The current element in the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The enumerator is positioned before the first element of the 
            /// collection or after the last element.
            /// -or- The collection was modified after the enumerator was created.
            /// </exception>
            public object Current
            {
                get
                {
                    switch (m_mode)
                    {
                        case EnumeratorMode.Entries : return Entry;
                        case EnumeratorMode.Keys    : return Key;
                        case EnumeratorMode.Values  : return Value;
                        default :
                            throw new ArgumentException("Mode is invalid");
                    }
                }
            }

            #endregion

            #region Implementation of IDictionaryEnumerator

            /// <summary>
            /// Gets the key of the current dictionary entry.
            /// </summary>
            /// <returns>
            /// The key of the current element of the enumeration.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The <see cref="T:System.Collections.IDictionaryEnumerator" /> 
            /// is positioned before the first entry of the dictionary 
            /// or after the last entry. 
            /// </exception>
            public object Key
            {
                get { return Entry.Key; }
            }

            /// <summary>
            /// Gets the value of the current dictionary entry.
            /// </summary>
            /// <returns>
            /// The value of the current element of the enumeration.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The <see cref="T:System.Collections.IDictionaryEnumerator" /> 
            /// is positioned before the first entry of the dictionary 
            /// or after the last entry. 
            /// </exception>
            public object Value
            {
                get { return Entry.Value; }
            }

            /// <summary>
            /// Gets both the key and the value of the current dictionary entry.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.DictionaryEntry" /> containing 
            /// both the key and the value of the current dictionary entry.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The <see cref="T:System.Collections.IDictionaryEnumerator" /> 
            /// is positioned before the first entry of the dictionary 
            /// or after the last entry. 
            /// </exception>
            public DictionaryEntry Entry
            {
                get
                {
                    if (m_fBeforeFirst)
                    {
                        throw new InvalidOperationException("Enumerator is before the first element.");
                    }
                    if (m_fAfterLast)
                    {
                        throw new InvalidOperationException("Enumerator is after the last element.");
                    }

                    return m_currentEntry;
                }
            }

            #endregion

            #region Data members

            /// <summary>
            /// HashDictionary this enumerator enumerates over.
            /// </summary>
            private readonly HashDictionary m_dict;

            /// <summary>
            /// Enumerator the calls should be delegated to.
            /// </summary>
            private readonly IDictionaryEnumerator m_enumerator;

            /// <summary>
            /// Enumerator mode, determines what will be returned by the 
            /// Current property.
            /// </summary>
            private readonly EnumeratorMode m_mode;

            /// <summary>
            /// Flag specifying if this enumerator is before the first 
            /// element.
            /// </summary>
            private bool m_fBeforeFirst = true;

            /// <summary>
            /// Flag specifying if this enumerator is after the last 
            /// element.
            /// </summary>
            private bool m_fAfterLast;

            /// <summary>
            /// Current enumerator entry.
            /// </summary>
            private DictionaryEntry m_currentEntry;

            #endregion
        }

        #endregion

        #region Inner class: KeyCollection

        /// <summary>
        /// Internal key collection.
        /// </summary>
        private class KeyCollection : ICollection
        {
            private readonly HashDictionary m_dict;

            internal KeyCollection(HashDictionary dict)
            {
                m_dict = dict;
            }

            public IEnumerator GetEnumerator()
            {
                return new HashDictionaryEnumerator(m_dict, EnumeratorMode.Keys);
            }

            public void CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (array.Rank != 1)
                {
                    throw new ArgumentException(
                        "Multidimensional array is not supported for this operation");
                }
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "index", "Index cannot be a negative number");
                }
                if ((array.Length - index) < m_dict.Count)
                {
                    throw new ArgumentException("Destination array is too small");
                }
                
                foreach (Object key in this)
                {
                    array.SetValue(key, index++);
                }
            }

            public int Count
            {
                get { return m_dict.Count; }
            }

            public object SyncRoot
            {
                get { return m_dict.SyncRoot; }
            }

            public bool IsSynchronized
            {
                get { return m_dict.IsSynchronized; }
            }
        }

        #endregion

        #region Inner class: ValueCollection

        /// <summary>
        /// Internal value collection.
        /// </summary>
        private class ValueCollection : ICollection
        {
            private readonly HashDictionary m_dict;

            internal ValueCollection(HashDictionary dict)
            {
                m_dict = dict;
            }

            public IEnumerator GetEnumerator()
            {
                return new HashDictionaryEnumerator(m_dict, EnumeratorMode.Values);
            }

            public void CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (array.Rank != 1)
                {
                    throw new ArgumentException(
                        "Multidimensional array is not supported for this operation");
                }
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "index", "Index cannot be a negative number");
                }
                if ((array.Length - index) < m_dict.Count)
                {
                    throw new ArgumentException("Destination array is too small");
                }

                foreach (Object val in this)
                {
                    array.SetValue(val, index++);
                }
            }

            public int Count
            {
                get { return m_dict.Count; }
            }

            public object SyncRoot
            {
                get { return m_dict.SyncRoot; }
            }

            public bool IsSynchronized
            {
                get { return m_dict.IsSynchronized; }
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// Value of a dictionary entry with a <c>null</c> key.
        /// </summary>
        private Object m_nullValue = ObjectUtils.NO_VALUE;

        /// <summary>
        /// Keys collection.
        /// </summary>
        [NonSerialized] 
        private ICollection m_keys;

        /// <summary>
        /// Values collection.
        /// </summary>
        [NonSerialized] 
        private ICollection m_values;

        #endregion
    }
}