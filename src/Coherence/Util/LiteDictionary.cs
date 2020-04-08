/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using Tangosol.Util.Collections;

namespace Tangosol.Util
{
    /// <summary>
    /// An implementation of <b>IDictionary</b> that is optimal (in terms of
    /// both size and speed) for very small sets of data but still works
    /// excellently with large sets of data.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This implementation is not thread-safe.</p>
    /// <p>
    /// The LiteDictionary implementation switches at runtime between several
    /// different sub-implementations for storing the <b>IDictionary</b> of
    /// objects, described here:</p>
    /// <p>
    /// <list type="number">
    /// <item>
    /// "empty dictionary" - a dictionary that contains no data;
    /// </item>
    /// <item>
    /// "single entry" - a reference directly to a single dictionary entry;
    /// </item>
    /// <item>
    /// "object[]" - a reference to an array of entries; the item limit for
    /// this implementation is determined by the <see cref="THRESHOLD"/>
    /// constant;
    /// </item>
    /// <item>
    /// "delegation" - for more than <see cref="THRESHOLD"/> items, a
    /// dictionary is created to delegate the dictionary management to;
    /// sub-classes can override the default delegation class
    /// <b>Hashtable</b> by overriding the factory method
    /// <see cref="InstantiateDictionary"/>.
    /// </item>
    /// </list></p>
    /// <p>
    /// The LiteDictionary implementation supports the <c>null</c> key value.
    /// </p>
    /// </remarks>
    /// <author>Cameron Purdy  1999.06.29</author>
    /// <author>Goran Milosavljevic  2006.09.11</author>
    public class LiteDictionary : IDictionary
    {
        #region Properties

        /// <summary>
        /// Gets the number of elements contained in this dictionary.
        /// </summary>
        /// <value>
        /// The number of elements contained in this dictionary.
        /// </value>
        public virtual int Count
        {
            get
            {
                switch (m_implType)
                {
                    case LiteDictionaryType.Empty:
                        return 0;

                    case LiteDictionaryType.Single:
                        return 1;

                    case LiteDictionaryType.Array1:
                    case LiteDictionaryType.Array2:
                    case LiteDictionaryType.Array3:
                    case LiteDictionaryType.Array4:
                    case LiteDictionaryType.Array5:
                    case LiteDictionaryType.Array6:
                    case LiteDictionaryType.Array7:
                    case LiteDictionaryType.Array8:
                        return m_implType - LiteDictionaryType.Array1 + 1;

                    case LiteDictionaryType.Other:
                        return ((IDictionary) m_contents).Count;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// Gets <b>true</b> if this dictionary contains no key-value
        /// mappings.
        /// </summary>
        /// <value>
        /// <b>true</b> if this dictionary contains no key-value
        /// mappings.
        /// </value>
        public virtual bool IsEmpty
        {
            get { return m_implType == LiteDictionaryType.Empty; }
        }

        /// <summary>
        /// Returns the value to which this dictionary maps the specified
        /// key.
        /// </summary>
        /// <returns>
        /// The value to which this dictionary maps the specified key, or
        /// <c>null</c> if the dictionary contains no mapping for this key.
        /// </returns>
        /// <param name="key">
        /// The key of the element to get or set.
        /// </param>
        public virtual object this[object key]
        {
            get
            {
                switch (m_implType)
                {
                    case LiteDictionaryType.Empty:
                        return null;

                    case LiteDictionaryType.Single:
                        DictionaryEntry entry    = (DictionaryEntry) m_contents;
                        object          keyEntry = entry.Key;
                        return Equals(key, keyEntry) ? entry.Value : null;

                    case LiteDictionaryType.Array1:
                    case LiteDictionaryType.Array2:
                    case LiteDictionaryType.Array3:
                    case LiteDictionaryType.Array4:
                    case LiteDictionaryType.Array5:
                    case LiteDictionaryType.Array6:
                    case LiteDictionaryType.Array7:
                    case LiteDictionaryType.Array8:
                        DictionaryEntry[] entries = (DictionaryEntry[]) m_contents;
                        int               c       = m_implType - LiteDictionaryType.Array1 + 1;
                        int               i       = IndexOf(entries, c, key);
                        return i < 0 ? null : entries[i].Value;

                    case LiteDictionaryType.Other:
                        return ((IDictionary) m_contents)[key];

                    default:
                        throw new InvalidOperationException();
                }
            }
            set { Add(key, value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a LiteDictionary.
        /// </summary>
        public LiteDictionary()
        {}

        /// <summary>
        /// Construct a LiteDictionary with the same mappings as the given
        /// dictionary.
        /// </summary>
        /// <param name="dictionary">
        /// The dictionary whose mappings are to be placed in this
        /// dictionary.
        /// </param>
        public LiteDictionary(IDictionary dictionary)
        {
            CollectionUtils.AddAll(this, dictionary);
        }

        #endregion

        #region IDictionary implementation

        /// <summary>
        /// Removes all elements from the dictionary.
        /// </summary>
        public virtual void Clear()
        {
            m_implType = LiteDictionaryType.Empty;
            m_contents = null;
        }

        /// <summary>
        /// Returns <b>true</b> if this dictionary contains a mapping for the
        /// specified key.
        /// </summary>
        /// <returns>
        /// <b>true</b> if this dictionary contains a mapping for the
        /// specified key, <b>false</b> otherwise.
        /// </returns>
        /// <param name="key">
        /// The key to locate in the dictionary.
        /// </param>
        public virtual bool Contains(object key)
        {
            switch (m_implType)
            {
                case LiteDictionaryType.Empty:
                    return false;

                case LiteDictionaryType.Single:
                    object keyEntry = ((DictionaryEntry) m_contents).Key;
                    return Equals(key, keyEntry);

                case LiteDictionaryType.Array1:
                case LiteDictionaryType.Array2:
                case LiteDictionaryType.Array3:
                case LiteDictionaryType.Array4:
                case LiteDictionaryType.Array5:
                case LiteDictionaryType.Array6:
                case LiteDictionaryType.Array7:
                case LiteDictionaryType.Array8:
                    DictionaryEntry[] entries = (DictionaryEntry[]) m_contents;
                    int               c       = m_implType - LiteDictionaryType.Array1 + 1;
                    return IndexOf(entries, c, key) >= 0;

                case LiteDictionaryType.Other:
                    return ((IDictionary) m_contents).Contains(key);

                default:
                    throw new InvalidOperationException();
            }
        }

        #endregion

        #region ICollection implementation

        /// <summary>
        /// Adds an element with the provided key and value to the
        /// <b>IDictionary</b> object.
        /// </summary>
        /// <param name="value">
        /// The object to use as the value of the element to add.
        /// </param>
        /// <param name="key">
        /// The object to use as the key of the element to add.
        /// </param>
        public virtual void Add(object key, object value)
        {
            switch (m_implType)
            {
                case LiteDictionaryType.Empty:
                    m_implType = LiteDictionaryType.Single;
                    m_contents = InstantiateEntry(key, value);
                    break;

                case LiteDictionaryType.Single:
                    DictionaryEntry entry    = (DictionaryEntry) m_contents;
                    object          keyEntry = entry.Key;
                    if (Equals(key, keyEntry))
                    {
                        entry.Value = value;
                        m_contents  = entry;
                    }
                    else
                    {
                        // grow to array implementation
                        DictionaryEntry[] entries = new DictionaryEntry[THRESHOLD];
                        entries[0] = entry;
                        entries[1] = InstantiateEntry(key, value);

                        m_implType = LiteDictionaryType.Array2;
                        m_contents = entries;
                    }
                    break;

                case LiteDictionaryType.Array1:
                case LiteDictionaryType.Array2:
                case LiteDictionaryType.Array3:
                case LiteDictionaryType.Array4:
                case LiteDictionaryType.Array5:
                case LiteDictionaryType.Array6:
                case LiteDictionaryType.Array7:
                case LiteDictionaryType.Array8:
                    LiteDictionaryType impl         = m_implType;
                    DictionaryEntry[]  entriesArray = (DictionaryEntry[]) m_contents;

                    int c = impl - LiteDictionaryType.Array1 + 1;
                    int i = IndexOf(entriesArray, c, key);

                    if (i >= 0)
                    {
                        entriesArray[i].Value = value;
                    }
                    else
                    {
                        // check if adding the object exceeds the "lite" threshold
                        if (c >= THRESHOLD)
                        {
                            // time to switch to a different map implementation
                            IDictionary map = InstantiateDictionary();
                            for (i = 0; i < c; ++i)
                            {
                                DictionaryEntry entr = entriesArray[i];
                                map.Add(entr.Key, entr.Value);
                            }
                            map.Add(key, value);

                            m_implType = LiteDictionaryType.Other;
                            m_contents = map;
                        }
                        else
                        {
                            // use the next available element in the array
                            entriesArray[c] = InstantiateEntry(key, value);
                            m_implType      = impl + 1;
                            m_contents      = entriesArray;
                        }
                    }
                    break;

                case LiteDictionaryType.Other:
                    ((IDictionary) m_contents)[key] = value;
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Removes the mapping for this key from this dictionary if present.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        public virtual void Remove(object key)
        {
            RemoveEx(key);
        }

        /// <summary>
        /// Returns an <b>IEnumerator</b> object for this LiteDictionary.
        /// </summary>
        /// <returns>
        /// An <b>IEnumerator</b> object for this LiteDictionary.
        /// </returns>
        public virtual IEnumerator GetEnumerator()
        {
            DictionaryEntry[] entries = new DictionaryEntry[0];

            switch (m_implType)
            {
                case LiteDictionaryType.Empty:
                    return entries.GetEnumerator();

                case LiteDictionaryType.Single:
                    DictionaryEntry entry = (DictionaryEntry) m_contents;
                    entries    = new DictionaryEntry[1];
                    entries[0] = entry;
                    return entries.GetEnumerator();

                case LiteDictionaryType.Array1:
                case LiteDictionaryType.Array2:
                case LiteDictionaryType.Array3:
                case LiteDictionaryType.Array4:
                case LiteDictionaryType.Array5:
                case LiteDictionaryType.Array6:
                case LiteDictionaryType.Array7:
                case LiteDictionaryType.Array8:
                    entries = (DictionaryEntry[]) m_contents;
                    int               c      = m_implType - LiteDictionaryType.Array1 + 1;
                    DictionaryEntry[] result = new DictionaryEntry[c];
                    Array.Copy(entries, 0, result, 0, c);
                    return result.GetEnumerator();

                case LiteDictionaryType.Other:
                    return ((IDictionary) m_contents).GetEnumerator();

                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Returns an <b>IDictionaryEnumerator</b> object for this
        /// LiteDictionary.
        /// </summary>
        /// <returns>
        /// An <b>IDictionaryEnumerator</b> object for this LiteDictionary.
        /// </returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a collection containing the keys of the LiteDictionary.
        /// </summary>
        /// <returns>
        /// A collection containing the keys of the LiteDictionary.
        /// </returns>
        public virtual ICollection Keys
        {
            get
            {
                ArrayList keys = new ArrayList();
                foreach (DictionaryEntry entry in this)
                {
                    keys.Add(entry.Key);
                }
                return keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in the LiteDictionary.
        /// </summary>
        /// <returns>
        /// A collection containing the values in the LiteDictionary.
        /// </returns>
        public virtual ICollection Values
        {
            get
            {
                ArrayList values = new ArrayList();
                foreach (DictionaryEntry entry in this)
                {
                    values.Add(entry.Value);
                }
                return values;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <b>IDictionary</b> object is
        /// read-only.
        /// </summary>
        /// <value>
        /// Always <b>false</b> for LiteDictionary.
        /// </value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the <b>IDictionary</b> object has
        /// a fixed size.
        /// </summary>
        /// <value>
        /// Always <b>false</b> for LiteDictionary.
        /// </value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the
        /// <b>ICollection</b>.
        /// </summary>
        /// <value>
        /// An object that can be used to synchronize access to the
        /// <b>ICollection"></b>.
        /// </value>
        public virtual object SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <b>ICollection</b>
        /// is synchronized (thread safe).
        /// </summary>
        /// <value>
        /// Always <b>false</b> for LiteDictionary.
        /// </value>
        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Copies the elements of the <b>ICollection</b> to an <b>Array</b>,
        /// starting at a particular index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <b>Array</b> that is the destination of the
        /// elements copied from <b>ICollection</b>.
        /// </param>
        /// <param name="index">
        /// The zero-based index in array at which copying begins.
        /// </param>
        public virtual void CopyTo(Array array, int index)
        {
            object[] list = new object[Count];
            int      i    = 0;
            foreach (object o in this)
            {
                list[i++] = o;
            }

            Array.Copy(list, 0, array, index, Count);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Scan up to the first <b>searchCount</b> elements of the passed
        /// Entry array looking for the specified key.
        /// </summary>
        /// If it is found, return its position <i>i</i> in the array
        /// such that <b>(0 &lt;= i &lt; searchCount)</b>. If it is not found,
        /// return <b>-1</b>.
        /// <param name="entries">
        /// The array of objects to search.
        /// </param>
        /// <param name="searchCount">
        /// The number of Entry objects in the array to search.
        /// </param>
        /// <param name="key">
        /// The key to look for.
        /// </param>
        /// <returns>
        /// The index of the object, if found; otherwise -1.
        /// </returns>
        private int IndexOf(DictionaryEntry[] entries, int searchCount, object key)
        {
            // first quick-scan by reference
            for (int i = 0; i < searchCount; ++i)
            {
                if (key == entries[i].Key)
                {
                    return i;
                }
            }

            // slow scan by Equals()
            if (key != null)
            {
                for (int i = 0; i < searchCount; ++i)
                {
                    if (key.Equals(entries[i].Key))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// After a mutation operation has reduced the size of an underlying
        /// dictionary, check if the delegation model should be replaced with
        /// a more size-efficient storage approach, and switch accordingly.
        /// </summary>
        protected virtual void CheckShrinkFromOther()
        {
            Debug.Assert(m_implType == LiteDictionaryType.Other);

            // check if the dictionary is now significantly below the "lite"
            // threshold
            IDictionary map = (IDictionary) m_contents;
            int         c   = map.Count;
            switch (c)
            {
                case 0:
                    m_implType = LiteDictionaryType.Empty;
                    m_contents = null;
                    break;

                case 1:
                    DictionaryEntry entry = (DictionaryEntry) CollectionUtils.ToArray(map)[0];
                    m_implType = LiteDictionaryType.Single;
                    m_contents = InstantiateEntry(entry.Key, entry.Value);
                    break;

                case 2:
                case 3:
                case 4:
                    DictionaryEntry[] entries = new DictionaryEntry[THRESHOLD];
                    int               i       = 0;
                    foreach (DictionaryEntry entr in map)
                    {
                        entries[i++] = InstantiateEntry(entr.Key, entr.Value);
                    }

                    Debug.Assert(i == c);

                    m_implType = LiteDictionaryType.Array1 + i - 1;
                    m_contents = entries;
                    break;
            }
        }

        /// <summary>
        /// Instantiate a <b>DictionaryEntry</b>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// An instance of <b>DictionaryEntry</b>.
        /// </returns>
        protected virtual DictionaryEntry InstantiateEntry(object key, object value)
        {
            return new DictionaryEntry(key, value);
        }

        /// <summary>
        /// Instantiate an <b>IDictionary</b> object to store entries in once
        /// the "lite" threshold has been exceeded.
        /// </summary>
        /// <returns>
        /// An instance of <b>IDictionary</b>.
        /// </returns>
        protected virtual IDictionary InstantiateDictionary()
        {
            return new HashDictionary();
        }

        /// <summary>
        /// Removes the mapping for this key from this map if present.
        /// </summary>
        /// <remarks>
        /// Expensive: updates both the underlying cache and the local cache.
        /// </remarks>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        /// <returns>
        /// Previous value associated with specified key, or <c>null</c> if
        /// there was no mapping for key. A <c>null</c> return can also
        /// indicate that the dictionary previously associated <c>null</c>
        /// with the specified key, if the implementation supports
        /// <c>null</c> values.
        /// </returns>
        public virtual object RemoveEx(object key)
        {
            switch (m_implType)
            {
                case LiteDictionaryType.Empty:
                    return null;

                case LiteDictionaryType.Single:
                    DictionaryEntry entry    = (DictionaryEntry) m_contents;
                    object          keyEntry = entry.Key;
                    object          prev     = null;
                    if (Equals(key, keyEntry))
                    {
                        prev       = entry.Value;
                        m_implType = LiteDictionaryType.Empty;
                        m_contents = null;
                    }
                    return prev;

                case LiteDictionaryType.Array1:
                case LiteDictionaryType.Array2:
                case LiteDictionaryType.Array3:
                case LiteDictionaryType.Array4:
                case LiteDictionaryType.Array5:
                case LiteDictionaryType.Array6:
                case LiteDictionaryType.Array7:
                case LiteDictionaryType.Array8:
                    LiteDictionaryType impl    = m_implType;
                    DictionaryEntry[]  entries = (DictionaryEntry[]) m_contents;
                    int                c       = impl - LiteDictionaryType.Array1 + 1;
                    int                i       = IndexOf(entries, c, key);
                    if (i < 0)
                    {
                        return null;
                    }

                    object prevValue = entries[i].Value;
                    if (c == 1)
                    {
                        m_implType = LiteDictionaryType.Empty;
                        m_contents = null;
                    }
                    else
                    {
                        Array.Copy(entries, i + 1, entries, i, c - i - 1);
                        // cannot set entries[c - 1] to null as in Java code
                        // initialize to empty DictionaryEntry instead
                        entries[c - 1] = new DictionaryEntry();
                        m_implType     = --impl;
                    }
                    return prevValue;

                case LiteDictionaryType.Other:
                    IDictionary map     = (IDictionary) m_contents;
                    object      prevVal = map[key];
                    map.Remove(key);
                    CheckShrinkFromOther();
                    return prevVal;

                default:
                    throw new InvalidOperationException();
            }
        }

        #endregion

        #region Enum: LiteDictionaryType

        /// <summary>
        /// LiteDictionary type enum values.
        /// </summary>
        protected enum LiteDictionaryType
        {
            /// <summary>
            /// Implementation: Empty dictionary.
            /// </summary>
            Empty = 0,

            /// <summary>
            /// Implementation: Single-item dictionary.
            /// </summary>
            Single = 1,

            /// <summary>
            /// Implementation: Array dictionary of 1 item.
            /// </summary>
            Array1 = 2,

            /// <summary>
            /// Implementation: Array dictionary of 2 items.
            /// </summary>
            Array2 = 3,

            /// <summary>
            /// Implementation: Array dictionary of 3 items.
            /// </summary>
            Array3 = 4,

            /// <summary>
            /// Implementation: Array dictionary of 4 items.
            /// </summary>
            Array4 = 5,

            /// <summary>
            /// Implementation: Array dictionary of 5 items.
            /// </summary>
            Array5 = 6,

            /// <summary>
            /// Implementation: Array dictionary of 6 items.
            /// </summary>
            Array6 = 7,

            /// <summary>
            /// Implementation: Array dictionary of 7 items.
            /// </summary>
            Array7 = 8,

            /// <summary>
            /// Implementation: Array dictionary of 8 items.
            /// </summary>
            Array8 = 9,

            /// <summary>
            /// Implementation: Delegation.
            /// </summary>
            Other = 10
        }

        #endregion

        #region Constants

        /// <summary>
        /// A constant array of zero size.
        /// </summary>
        private static readonly object[] NO_OBJECTS = new object[0];

        /// <summary>
        /// The default point above which the LiteDictionary delegates to
        /// another dictionary implementation.
        /// </summary>
        private const int THRESHOLD = 8;

        #endregion

        #region Data members

        /// <summary>
        /// The dictionary contents, based on the implementation being used.
        /// </summary>
        private object m_contents;

        /// <summary>
        /// Implementation, one of LiteDictionaryType enum values.
        /// </summary>
        private LiteDictionaryType m_implType;

        #endregion
    }
}