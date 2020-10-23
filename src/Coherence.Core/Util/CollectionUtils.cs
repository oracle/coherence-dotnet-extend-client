/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Tangosol.Util.Collections;

namespace Tangosol.Util
{
    /// <summary>
    /// This class provides the functionality not found in the .NET
    /// Collections classes.
    /// </summary>
    /// <author>Aleksandar Seovic  2006.08.09</author>
    /// <author>Ivan Cikic  2006.08.09</author>
    public abstract class CollectionUtils
    {
        /// <summary>
        /// Adds a new element to the specified collection.
        /// </summary>
        /// <remarks>
        /// This method allows us to add element to any collection type from
        /// the <see cref="System.Collections"/> namespace in a uniform
        /// manner, thus hiding differences in the API between
        /// <see cref="IList"/> implementations and classes such as
        /// <see cref="Queue"/> and <see cref="Stack"/>.
        /// When collection is <see cref="IDictionary"/> object should be
        /// <see cref="DictionaryEntry"/>.
        /// </remarks>
        /// <param name="target">
        /// Collection where the new element will be added.
        /// </param>
        /// <param name="obj">
        /// Object to add.
        /// </param>
        /// <returns>
        /// <b>true</b> if the element was added, <b>false</b> otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// If <paramref name="target"/> is a fixed-size collection, or if
        /// its type is an unknown collection type.
        /// </exception>
        public static bool Add(ICollection target, object obj)
        {
            if (target.GetType().IsArray)
            {
                throw new
                    NotSupportedException("element cannot be added to a fixed-size collection, such as array");
            }
            if (target is IList)
            {
                ((IList) target).Add(obj);
            }
            else if (target is Queue)
            {
                ((Queue) target).Enqueue(obj);
            }
            else if (target is Stack)
            {
                ((Stack) target).Push(obj);
            }
            else if (target is IDictionary && obj is DictionaryEntry)
            {
                var entry = (DictionaryEntry) obj;
                ((IDictionary) target).Add(entry.Key, entry.Value);
            }
            else if (target is HashSet)
            {
                return ((HashSet) target).Add(obj);
            }
            else
            {
                throw new NotSupportedException("unknown collection type");
            }

            return true;
        }

        /// <summary>
        /// Adds a new element to the specified collection.
        /// </summary>
        /// <remarks>
        /// This method allows us to add element to any collection type from
        /// the <see cref="System.Collections"/> namespace in a uniform
        /// manner, thus hiding differences in the API between
        /// <see cref="IList"/> implementations and classes such as
        /// <see cref="Queue"/> and <see cref="Stack"/>.
        /// When collection is <see cref="IDictionary"/> object should be
        /// <see cref="DictionaryEntry"/>.
        /// </remarks>
        /// <param name="target">
        /// Collection where the new element will be added.
        /// </param>
        /// <param name="obj">
        /// Object to add.
        /// </param>
        /// <param name="isUnique">
        /// <b>true</b> if the value should be unique in the
        /// target collection; <b>false</b> otherwise.
        /// </param>
        /// <returns>
        /// <b>true</b> if the element was added, <b>false</b> otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// If <paramref name="target"/> is a fixed-size collection, or if
        /// its type is an unknown collection type.
        /// </exception>
        public static bool Add(ICollection target, object obj, bool isUnique)
        {
            if (isUnique)
            {
                if (!Contains(target, obj))
                {
                    return Add(target, obj);
                }
                return false;
            }
            return Add(target, obj);
        }

        /// <summary>
        /// Adds all of the elements of the "source" collection to the
        /// "target" collection.
        /// </summary>
        /// <remarks>
        /// This method allows us to add elements to any collection type from
        /// the <see cref="System.Collections"/> namespace in a uniform
        /// manner, thus hiding differences in the API between
        /// <see cref="IList"/> implementations and classes such as
        /// <see cref="Queue"/> and <see cref="Stack"/>.
        /// </remarks>
        /// <param name="target">
        /// Collection where the new elements will be added.
        /// </param>
        /// <param name="source">
        /// Collection whose elements will be added.
        /// </param>
        /// <returns>
        /// <b>true</b> if at least one element was added, <b>false</b>
        /// otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// If <paramref name="target"/> is a fixed-size collection, or if
        /// its type is an unknown collection type.
        /// </exception>
        public static bool AddAll(ICollection target, ICollection source)
        {
            bool isAdded = false;

            foreach (object o in source)
            {
                isAdded = Add(target, o) || isAdded;
            }

            return isAdded;
        }

        /// <summary>
        /// Returns an array containing all of the elements from the
        /// specified collection.
        /// </summary>
        /// <param name="col">
        /// Collection whose elements are copied to an array.
        /// </param>
        /// <returns>
        /// An array containing the elements of the collection.
        /// </returns>
        public static object[] ToArray(ICollection col)
        {
            var o = new object[col.Count];
            col.CopyTo(o, 0);
            return o; 
        }

        /// <summary>
        /// Determines whether an element is in the collection.
        /// </summary>
        /// <remarks>
        /// This method allows us to determine if an element is in any
        /// collection type from the <see cref="System.Collections"/>
        /// namespace since <see cref="ICollection"/> does not have method
        /// "Contains" defined in its interface.
        /// When collection is <see cref="IDictionary"/> object should be
        /// <see cref="DictionaryEntry"/>.
        /// </remarks>
        /// <param name="col">
        /// Collection where element is searched for.
        /// </param>
        /// <param name="obj">
        /// The object to locate in the collection.
        /// </param>
        /// <returns>
        /// <b>true</b> if the element is found in the collection,
        /// <b>false</b> otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// If <paramref name="col"/>'s type is an unknown collection type.
        /// </exception>
        public static bool Contains(ICollection col, object obj)
        {
            bool contains = false;
            if (col.GetType().IsArray)
            {
                var arr        = (Array) col;
                var lowerBound = arr.GetLowerBound(0);
                return Array.IndexOf(arr, obj, lowerBound, arr.Length) >= lowerBound;
            }
            if (col is IList)
            {
                contains = ((IList) col).Contains(obj);
            }
            else if (col is IDictionary)
            {
                if (obj is DictionaryEntry)
                {
                    var entry = (DictionaryEntry) obj;
                    contains = ((IDictionary) col).Contains(entry.Key);
                }
                else
                {
                    contains = ((IDictionary) col).Contains(obj);
                }
            }
            else if (col is HashSet)
            {
                contains = ((HashSet) col).Contains(obj);
            }
            else
            {
                foreach (object o in col)
                {
                    contains = (o == null ? obj == null : o == obj || o.Equals(obj));
                    if (contains)
                    {
                        break;
                    }
                }
            }

            return contains;
        }

        /// <summary>
        /// Determines whether all elements from the source collection are
        /// contained in the target collection.
        /// </summary>
        /// <remarks>
        /// When target collection is <see cref="IDictionary"/>, source
        /// collection elements should be <see cref="DictionaryEntry"/>s.
        /// </remarks>
        /// <param name="target">
        /// Collection where elements are searched for.
        /// </param>
        /// <param name="source">
        /// Collection with objects to locate in the collection.
        /// </param>
        /// <returns>
        /// <b>true</b> if all the elements are found in the collection,
        /// <b>false</b> otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// If <paramref name="target"/>'s type is an unknown collection type.
        /// </exception>
        /// <seealso cref="Contains"/>
        public static bool ContainsAll(ICollection target, ICollection source)
        {
            bool containsAll = source.Count > 0;
            foreach (object o in source)
            {
                containsAll = Contains(target, o) && containsAll;
                if (!containsAll)
                {
                    return false;
                }
            }
            return containsAll;
        }

        /// <summary>
        /// Removes an element from the specified collection.
        /// </summary>
        /// <remarks>
        /// This method allows us to remove element from any collection type
        /// from the <see cref="System.Collections"/> namespace in a uniform
        /// manner, thus hiding differences in the API between
        /// <see cref="IList"/> implementations and classes such as
        /// <see cref="Queue"/> and <see cref="Stack"/>.
        /// When collection is <see cref="IDictionary"/> object should be
        /// <see cref="DictionaryEntry"/>.
        /// </remarks>
        /// <param name="col">
        /// Collection from which the element should be removed.
        /// </param>
        /// <param name="obj">
        /// Object to remove.
        /// </param>
        /// <returns>
        /// <b>true</b> if the element was removed, <b>false</b> otherwise.
        /// </returns>
        public static bool Remove(ICollection col, object obj)
        {
            bool isRemoved = false;

            if (col is HashSet)
            {
                isRemoved = ((HashSet) col).Remove(obj);
            }
            else if (Contains(col, obj))
            {
                isRemoved = true;
                if (col is IList)
                {
                    ((IList) col).Remove(obj);
                }
                else if (col is IDictionary && obj is DictionaryEntry)
                {
                    var entry = (DictionaryEntry) obj;
                    ((IDictionary) col).Remove(entry.Key);
                }
            }

            return isRemoved;
        }

        /// <summary>
        /// Clears specified collection.
        /// </summary>
        /// <remarks>
        /// This method allows us to clear any collection type from the
        /// <b>System.Collections</b> namespace in a uniform manner, thus
        /// hiding differences in the API between <b>IList</b>
        /// implementations and classes such as <b>Queue</b> and
        /// <b>Stack</b>.
        /// </remarks>
        /// <param name="col">
        /// Collection to clear.
        /// </param>
        public static void Clear(ICollection col)
        {
            if (col is IList)
            {
                ((IList) col).Clear();
            }
            else if (col is Queue)
            {
                ((Queue) col).Clear();
            }
            else if (col is Stack)
            {
                ((Stack) col).Clear();
            }
            else if (col is IDictionary)
            {
                ((IDictionary) col).Clear();
            }
            else if (col is HashSet)
            {
                ((HashSet) col).Clear();
            }
            else
            {
                throw new NotSupportedException("unknown collection type");
            }
        }

        /// <summary>
        /// Removes all elements contained in the source collection from the
        /// target collection.
        /// </summary>
        /// <remarks>
        /// When collection is <see cref="IDictionary"/> objects in source
        /// collection should be <see cref="DictionaryEntry"/>s.
        /// </remarks>
        /// <param name="target">
        /// Collection from which the elements should be removed.
        /// </param>
        /// <param name="source">
        /// Collection of elements to remove.
        /// </param>
        /// <returns>
        /// <b>true</b> if at least one element was removed, <b>false</b>
        /// otherwise.
        /// </returns>
        public static bool RemoveAll(ICollection target, ICollection source)
        {
            bool isModified = false;

            if (target.Count > source.Count)
            {
                foreach (object o in source)
                {
                    isModified = Remove(target, o) || isModified;
                }
            }
            else
            {
                var listToRemove = new ArrayList();
                foreach (object o in target)
                {
                    if (Contains(source, o))
                    {
                        listToRemove.Add(o);
                    }
                }
                foreach (object o in listToRemove)
                {
                    isModified = Remove(target, o) || isModified;
                }
            }
            return isModified;
        }

        /// <summary>
        /// Removes all elements from the target collection that are not
        /// contained in the source collection.
        /// </summary>
        /// <remarks>
        /// When collection is <see cref="IDictionary"/> objects in source
        /// collection should be <see cref="DictionaryEntry"/>s.
        /// </remarks>
        /// <param name="target">
        /// Collection from which the elements should be removed.
        /// </param>
        /// <param name="source">
        /// Collection of elements not to remove.
        /// </param>
        /// <returns>
        /// <b>true</b> if at least one element was removed, <b>false</b>
        /// otherwise.
        /// </returns>
        public static bool RetainAll(ICollection target, ICollection source)
        {
            bool isModified = false;

            var listToRemove = new ArrayList();
            foreach (object o in target)
            {
                if (!Contains(source, o))
                {
                    listToRemove.Add(o);
                }
            }
            foreach (object o in listToRemove)
            {
                isModified = Remove(target, o) || isModified;
            }

            return isModified;
        }

        /// <summary>
        /// Randomize the order of the elements within the passed list.
        /// </summary>
        /// <param name="list">
        /// The list to randomize; the passed list is not altered.
        /// </param>
        /// <returns>
        /// A new and immutable List whose contents are identical to those
        /// of the passed list except for the order in which they appear.
        /// </returns>
        public static IList Randomize(IList list)
        {
            return (IList) Randomize((IList<object>) list);
        }

        /// <summary>
        /// Randomize the order of the elements within the passed list.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the element.
        /// </typeparam>        
        /// <param name="list">
        /// The list to randomize; the passed list is not altered.
        /// </param>
        /// <returns>
        /// A new and immutable List whose contents are identical to those
        /// of the passed list except for the order in which they appear.
        /// </returns>
        /// <since>12.2.1</since>
        public static IList<T> Randomize<T>(IList<T> list)
        {
            var arr = new T[list.Count];
            list.CopyTo(arr, 0);

            return new List<T>(Randomize(arr)).AsReadOnly();
        }

        /// <summary>
        /// Reorder elements of a type T array in a random way.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the element.
        /// </typeparam>        
        /// <param name="array">
        /// The array of type T objects to randomize.
        /// </param>
        /// <returns>
        /// The array of elements ordered in a random manner.
        /// </returns>
        public static T[] Randomize<T>(T[] array)
        {
            int c;
            if (array == null || (c = array.Length) <= 1)
            {
                return array;
            }

            Random rnd = NumberUtils.GetRandom();
            for (int i1 = 0; i1 < c; i1++)
            {
                int    i2 = rnd.Next(c);
                T      t  = array[i2];
                array[i2] = array[i1];
                array[i1] = t;
            }
            return array;
        }

        /// <summary>
        /// Convert an array of byte values to an array of corresponding
        /// sbyte values.
        /// </summary>
        /// <param name="array">
        /// A byte array.
        /// </param>
        /// <returns>
        /// A sbyte array.
        /// </returns>
        public static sbyte[] ToSByteArray(byte[] array)
        {
            if (array == null)
            {
                return null;
            }

            var result = new sbyte[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = Convert.ToSByte(array[i]);
            }
            return result;
        }

        /// <summary>
        /// Convert an array of byte values to an array of corresponding
        /// sbyte values.
        /// </summary>
        /// <remarks>
        /// Conversion of bytes is being executed as <b>unchecked</b>.
        /// </remarks>
        /// <param name="array">
        /// A byte array.
        /// </param>
        /// <returns>
        /// A sbyte array.
        /// </returns>
        public static sbyte[] ToSByteArrayUnchecked(byte[] array)
        {
            if (array == null)
            {
                return null;
            }

            var result = new sbyte[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = unchecked((sbyte) array[i]);
            }
            return result;
        }

        /// <summary>
        /// Convert an array of sbyte values to an array of corresponding
        /// byte values.
        /// </summary>
        /// <remarks>
        /// Conversion of bytes is being executed as <b>unchecked</b>.
        /// </remarks>
        /// <param name="array">
        /// A sbyte array.
        /// </param>
        /// <returns>
        /// A byte array.
        /// </returns>
        public static byte[] ToByteArrayUnchecked(sbyte[] array)
        {
            if (array == null)
            {
                return null;
            }

            var result = new byte[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = unchecked((byte) array[i]);
            }
            return result;
        }

        /// <summary>
        /// Convert an array of sbyte values to an array of corresponding
        /// byte values.
        /// </summary>
        /// <param name="array">
        /// A sbyte array.
        /// </param>
        /// <returns>
        /// A byte array.
        /// </returns>
        public static byte[] ToByteArray(sbyte[] array)
        {
            if (array == null)
            {
                return null;
            }

            var result = new byte[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = Convert.ToByte(array[i]);
            }
            return result;
        }

        /// <summary>
        /// Trim zero bytes from the beggining of the array.
        /// </summary>
        /// <param name="array">
        /// Array to trim.
        /// </param>
        /// <returns>
        /// Left trimmed array.
        /// </returns>
        public static byte[] TrimLeftZeroBytes(byte[] array)
        {
            if (array == null)
            {
                return null;
            }

            int zeroBytes = -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != 0)
                {
                    zeroBytes = i;
                    break;
                }
            }

            if (zeroBytes > -1)
            {
                var tmpbytes = new byte[array.Length - zeroBytes];
                Buffer.BlockCopy(array, zeroBytes, tmpbytes, 0, tmpbytes.Length);
                return tmpbytes;
            }
            return array;
        }

        /// <summary>
        /// Examines whether two parameters are two equal array objects.
        /// </summary>
        /// <param name="obj1">
        /// Object to compare.
        /// </param>
        /// <param name="obj2">
        /// Object to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if two objects are arrays and are of the same
        /// array type; otherwise <b>false</b>.
        /// </returns>
        public static bool EqualsDeep(object obj1, object obj2)
        {
            if (obj1 == obj2)
            {
                return true;
            }
            if (obj1 == null || obj2 == null)
            {
                return false;
            }
            if (obj1.GetType().IsArray)
            {
                if (obj1 is byte[])
                {
                    return (obj2 is byte[]) 
                            && EqualsArray<byte>((byte[]) obj1, (byte[]) obj2, EqualityComparer<byte>.Default);
                }
                if (obj1 is object[])
                {
                    if (obj2 is object[])
                    {
                        var ao1 = (object[])obj1;
                        var ao2 = (object[])obj2;
                        int c   = ao1.Length;
                        if (c == ao2.Length)
                        {
                            for (int i = 0; i < c; i++)
                            {
                                if (!EqualsDeep(ao1[i], ao2[i]))
                                {
                                    return false;
                                }
                            }
                            return true;
                        }
                    }
                    return false;
                }
                if (obj1 is int[])
                {
                    return (obj2 is int[]) 
                            && EqualsArray<int>((int[]) obj1, (int[]) obj2, EqualityComparer<int>.Default);
                }
                if (obj1 is char[])
                {
                    return (obj2 is char[]) 
                            && EqualsArray<char>((char[]) obj1, (char[]) obj2, EqualityComparer<char>.Default);
                }
                if (obj1 is long[])
                {
                    return (obj2 is long[]) 
                            && EqualsArray<long>((long[]) obj1, (long[]) obj2, EqualityComparer<long>.Default);
                }
                if (obj1 is double[])
                {
                    return (obj2 is double[]) 
                            && EqualsArray<double>((double[]) obj1, (double[]) obj2, EqualityComparer<double>.Default);
                }
                if (obj1 is bool[])
                {
                    return (obj2 is bool[]) 
                            && EqualsArray<bool>((bool[]) obj1, (bool[]) obj2, EqualityComparer<bool>.Default);
                }
                if (obj1 is short[])
                {
                    return (obj2 is short[]) 
                            && EqualsArray<short>((short[]) obj1, (short[]) obj2, EqualityComparer<short>.Default);
                }
                if (obj1 is float[])
                {
                   return (obj2 is float[]) 
                            && EqualsArray<float>((float[]) obj1, (float[]) obj2, EqualityComparer<float>.Default);
                }
            }
            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Returns true if the two specified arrays are equal to one another. 
        /// Two arrays are considered equal if both arrays contain the same number 
        /// of elements, and all corresponding pairs of elements in the two arrays 
        /// are equal. In other words, two arrays are equal if they contain the same 
        /// elements in the same order. Also, two array references are considered 
        /// equal if both are null
        /// </summary>
        /// <typeparam name="T">
        /// The type of the array element.
        /// </typeparam>        
        /// <param name="array1">
        /// One array to be tested for equality.
        /// </param>
        /// <param name="array2">
        /// The other array to be tested for equality.
        /// </param>
        /// <param name="comparer">
        /// Comparer to use when comparing elements.
        /// </param>
        /// <returns>
        /// <b>true</b> if the two arrays are equal; otherwise <b>false</b>.
        /// </returns>
        public static bool EqualsArray<T>(T[] array1, T[] array2, IEqualityComparer comparer)
        {
            if (array1 == array2)
            {
                return true;
            }
            if (array1 == null || array2 == null)
            {
                return false;
            }
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (!comparer.Equals(array1[i], array2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a portion of the list whose keys are less than the limit
        /// object parameter.
        /// </summary>
        /// <param name="dict">
        /// The list where the portion will be extracted.
        /// </param>
        /// <param name="limit">
        /// The end element of the portion to extract.
        /// </param>
        /// <returns>
        /// The portion of the collection whose elements are less than the
        /// limit object parameter.
        /// </returns>
        public static SortedList HeadList(IDictionary dict, object limit)
        {
            var syncDict = dict as SynchronizedDictionary;
            if (syncDict != null)
            {
                dict = syncDict.Delegate;
            }

            SortedList list;
            IComparer  comparer;
            if (dict is SortedDictionary)
            {
                list     = (SortedDictionary) dict;
                comparer = ((SortedDictionary) dict).Comparer;
            }
            else if (dict is SortedList)
            {
                list     = dict as SortedList;
                comparer = null;
            }
            else
            {
                throw new NotSupportedException("Dictionary is not sorted: " + dict);
            }
            if (comparer == null)
            {
                comparer = Comparer.Default;
            }

            var newList = new SortedList();

            if (syncDict != null)
            {
                syncDict.AcquireReadLock();
            }
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (comparer.Compare(list.GetKey(i), limit) >= 0)
                    {
                        break;
                    }
                    newList.Add(list.GetKey(i), list[list.GetKey(i)]);
                }
            }
            finally
            {
                if (syncDict != null)
                {
                    syncDict.ReleaseReadLock();
                }
            }

            return newList;
        }

        /// <summary>
        /// Returns a portion of the list whose keys are greater than the
        /// limit object parameter.
        /// </summary>
        /// <param name="dict">
        /// The list where the portion will be extracted.
        /// </param>
        /// <param name="limit">
        /// The start element of the portion to extract.
        /// </param>
        /// <returns>
        /// The portion of the collection whose elements are greater than
        /// the limit object parameter.
        /// </returns>
        public static SortedList TailList(IDictionary dict, object limit)
        {
            var syncDict = dict as SynchronizedDictionary;
            if (syncDict != null)
            {
                dict = syncDict.Delegate;
            }

            SortedList list;
            IComparer  comparer;
            if (dict is SortedDictionary)
            {
                list     = (SortedDictionary)dict;
                comparer = ((SortedDictionary)dict).Comparer;
            }
            else if (dict is SortedList)
            {
                list     = dict as SortedList;
                comparer = null;
            }
            else
            {
                throw new NotSupportedException("Dictionary is not sorted: " + dict);
            }
            if (comparer == null)
            {
                comparer = Comparer.Default;
            }

            var newList  = new SortedList();

            if (syncDict != null)
            {
                syncDict.AcquireReadLock();
            }
            try
            {
                if (list.Count > 0)
                {
                    int index = 0;
                    while (comparer.Compare(list.GetKey(index), limit) < 0)
                    {
                        index++;
                    }

                    for (; index < list.Count; index++)
                    {
                        newList.Add(list.GetKey(index), list[list.GetKey(index)]);
                    }
                }
            }
            finally
            {
                if (syncDict != null)
                {
                    syncDict.ReleaseReadLock();
                }
            }

            return newList;
        }

        /// <summary>
        /// Format the content of the passed Object array as a delimited string.
        /// </summary>
        /// <param name="col">
        /// The array.
        /// </param>
        /// <param name="delim">
        /// The delimiter.
        /// </param>
        /// <returns>
        /// The formated string.
        /// </returns>
        public static string ToDelimitedString(ICollection col, string delim)
        {
            if (col != null &&  col.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (object obj in col)
                {
                    sb.Append(delim).Append(obj);
                }
                return sb.ToString().Substring(delim.Length);
            }
            return "";
        }

        /// <summary>
        /// Get the value of the specified bit.
        /// </summary>
        /// <param name="bits">
        /// The BitArray to get the bit from.
        /// </param>
        /// <param name="index">
        /// The bit index to get.
        /// </param>
        /// <returns>
        /// The value of the bit or false if the index exceeds the length
        /// of the bits.
        /// </returns>
        public static bool GetBit(BitArray bits, Int32 index)
        {
            return (index < bits.Length) && bits.Get(index);
        }

        /// <summary>
        /// Set the specified bit to the specified value.
        /// </summary>
        /// <param name="bits">
        /// The BitArray to modify.
        /// </param>
        /// <param name="index">
        /// The bit index to set.
        /// </param>
        /// <param name="value">
        /// The value to set the bit to.
        /// </param>
        public static void SetBit(BitArray bits, Int32 index, bool value)
        {
            for (int increment = 0; index >= bits.Length; increment = +64)
            {
                bits.Length += increment;
            }

            bits.Set(index, value);
        }

        /// <summary>
        /// Acquire a read-lock on the supplied collection, if possible. While 
        /// held, A read-lock prevents the given collection from being modified.
        /// </summary>
        /// <param name="col">The collection.</param>
        public static void AcquireReadLock(ICollection col)
        {
            if (col is SynchronizedDictionary)
            {
                var localCache = col as SynchronizedDictionary;
                localCache.AcquireReadLock();
            }
            else
            {
                Monitor.Enter(col.SyncRoot);
            }
        }

        /// <summary>
        /// Release a read-lock on the supplied collection.
        /// </summary>
        /// <param name="col">The collection.</param>
        public static void ReleaseReadLock(ICollection col)
        {
            if (col is SynchronizedDictionary)
            {
                var localCache = col as SynchronizedDictionary;
                localCache.ReleaseReadLock();
            }
            else
            {
                Monitor.Exit(col.SyncRoot);
            }
        }
    }
}