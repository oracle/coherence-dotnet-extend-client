/*
 * Copyright (c) 2000, 2022, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Util.Processor;

namespace Tangosol.Util
{
    /// <summary>
    /// A collection of <see cref="ICollection"/> implementation classes that
    /// use the <see cref="IConverter"/> interface to convert the items
    /// stored in underlying collection objects.
    /// </summary>
    /// <author>Cameron Purdy  2002.02.08</author>
    /// <author>Jason Howes  2007.09.28</author>
    /// <author>Ana Cikic  2008.05.28</author>
    public abstract class ConverterCollections
    {
        #region Factory methods

        /// <summary>
        /// Returns an instance of <see cref="IEnumerator"/> that uses an
        /// <see cref="IConverter"/> to view an underlying enumerator.
        /// </summary>
        /// <param name="enumerator">
        /// The underlying <b>IEnumerator</b>.
        /// </param>
        /// <param name="conv">
        /// The <b>IConverter</b> to view the underlying <b>IEnumerator</b>
        /// through.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerator"/> that views the passed enumerator
        /// through the specified <b>IConverter</b>.
        /// </returns>
        public static IEnumerator GetEnumerator(IEnumerator enumerator, IConverter conv)
        {
            return new ConverterEnumerator(enumerator, conv);
        }

        /// <summary>
        /// Returns an instance of <see cref="IDictionaryEnumerator"/> that
        /// uses <see cref="IConverter"/>s to view an underlying enumerator.
        /// </summary>
        /// <param name="enumerator">
        /// The underlying <b>IDictionaryEnumerator</b>.
        /// </param>
        /// <param name="convKey">
        /// The <b>IConverter</b> to view the underlying
        /// <b>IDictionaryEnumerator</b> keys through.
        /// </param>
        /// <param name="convVal">
        /// The <b>IConverter</b> to view the underlying
        /// <b>IDictionaryEnumerator</b> values through.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionaryEnumerator"/> that views the passed
        /// enumerator through the specified <b>IConverter</b>s.
        /// </returns>
        public static IDictionaryEnumerator GetDictionaryEnumerator(IDictionaryEnumerator enumerator,
            IConverter convKey, IConverter convVal)
        {
            return new ConverterDictionaryEnumerator(enumerator, convKey,  convVal);
        }

        /// <summary>
        /// Returns an instance of <see cref="ICacheEnumerator"/> that uses
        /// <see cref="IConverter"/>s to view an underlying enumerator.
        /// </summary>
        /// <param name="enumerator">
        /// The underlying <b>ICacheEnumerator</b>.
        /// </param>
        /// <param name="convKeyUp">
        /// The <b>IConverter</b> to view the underlying
        /// <b>ICacheEnumerator</b> keys through.
        /// </param>
        /// <param name="convValUp">
        /// The <b>IConverter</b> to view the underlying
        /// <b>ICacheEnumerator</b> values through.
        /// </param>
        /// <param name="convValDown">
        /// The <b>IConverter</b> to change the underlying
        /// <b>ICacheEnumerator</b> values through.
        /// </param>
        /// <returns>
        /// An <see cref="ICacheEnumerator"/> that views the passed
        /// enumerator through the specified <b>IConverter</b>s.
        /// </returns>
        public static ICacheEnumerator GetCacheEnumerator(ICacheEnumerator enumerator,
            IConverter convKeyUp, IConverter convValUp, IConverter convValDown)
        {
            return new ConverterCacheEnumerator(enumerator, convKeyUp, convValUp, convValDown);
        }

        /// <summary>
        /// Returns an instance of <see cref="ICollection"/> that uses an
        /// <see cref="IConverter"/> to view an underlying collection.
        /// </summary>
        /// <param name="col">
        /// The underlying <b>ICollection</b>.
        /// </param>
        /// <param name="convUp">
        /// The <see cref="IConverter"/> to view the underlying collection
        /// through.
        /// </param>
        /// <param name="convDown">
        /// The <see cref="IConverter"/> to pass items down to the underlying
        /// collection through.
        /// </param>
        /// <returns>
        /// An <see cref="ICollection"/> that views the passed collection
        /// through the specified <b>IConverter</b>.
        /// </returns>
        public static ConverterCollection GetCollection(ICollection col, IConverter convUp,
            IConverter convDown)
        {
            return new ConverterCollection(col, convUp, convDown);
        }

        /// <summary>
        /// Returns a Converter instance of <see cref="IDictionary"/>.
        /// </summary>
        /// <param name="dict">
        /// The underlying <see cref="IDictionary"/>.
        /// </param>
        /// <param name="convKeyUp">
        /// The <see cref="IConverter"/> to view the underlying
        /// dictionary's keys through.
        /// </param>
        /// <param name="convKeyDown">
        /// The <see cref="IConverter"/> to use to pass keys down to the
        /// underlying dictionary.
        /// </param>
        /// <param name="convValUp">
        /// The <see cref="IConverter"/> to view the underlying
        /// dictionary's values through.
        /// </param>
        /// <param name="convValDown">
        /// The <see cref="IConverter"/> to use to pass values down to
        /// the underlying dictionary.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary"/> that views the keys and values of
        /// the passed dictionary through the specified <b>IConverter</b>s.
        /// </returns>
        public static ConverterDictionary GetDictionary(IDictionary dict, IConverter convKeyUp,
            IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
        {
            return new ConverterDictionary(dict, convKeyUp, convKeyDown, convValUp, convValDown);
        }

        /// <summary>
        /// Returns a Converter instance of <see cref="ICache"/>.
        /// </summary>
        /// <param name="cache">
        /// The underlying <see cref="ICache"/>.
        /// </param>
        /// <param name="convKeyUp">
        /// The <see cref="IConverter"/> to view the underlying cache's keys
        /// through.
        /// </param>
        /// <param name="convKeyDown">
        /// The <see cref="IConverter"/> to use to pass keys down to the
        /// underlying cache.
        /// </param>
        /// <param name="convValUp">
        /// The <see cref="IConverter"/> to view the underlying cache's
        /// values through.
        /// </param>
        /// <param name="convValDown">
        /// The <see cref="IConverter"/> to use to pass values down to the
        /// underlying cache.
        /// </param>
        /// <returns>
        /// An <see cref="ICache"/> that views the keys and values of the
        /// passed <b>ICache</b> through the specified <b>IConverter</b>s.
        /// </returns>
        public static ConverterCache GetCache(ICache cache, IConverter convKeyUp,
            IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
        {
            return new ConverterCache(cache, convKeyUp, convKeyDown, convValUp, convValDown);
        }

        /// <summary>
        /// Returns a Converter instance of <see cref="IConcurrentCache"/>.
        /// </summary>
        /// <param name="cache">
        /// The underlying <see cref="IConcurrentCache"/>.
        /// </param>
        /// <param name="convKeyUp">
        /// The <see cref="IConverter"/> to view the underlying cache's keys
        /// through.
        /// </param>
        /// <param name="convKeyDown">
        /// The <see cref="IConverter"/> to use to pass keys down to the
        /// underlying cache.
        /// </param>
        /// <param name="convValUp">
        /// The <see cref="IConverter"/> to view the underlying cache's
        /// values through.
        /// </param>
        /// <param name="convValDown">
        /// The <see cref="IConverter"/> to use to pass values down to the
        /// underlying cache.
        /// </param>
        /// <returns>
        /// An <see cref="IConcurrentCache"/> that views the keys and values
        /// of the passed <b>IConcurrentCache</b> through the specified
        /// <b>IConverter</b>s.
        /// </returns>
        public static ConverterConcurrentCache GetConcurrentCache(IConcurrentCache cache,
            IConverter convKeyUp, IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
        {
            return new ConverterConcurrentCache(cache, convKeyUp, convKeyDown, convValUp, convValDown);
        }

        /// <summary>
        /// Returns a Converter instance of <see cref="IInvocableCache"/>.
        /// </summary>
        /// <param name="cache">
        /// The underlying <see cref="IInvocableCache"/>.
        /// </param>
        /// <param name="convKeyUp">
        /// The <see cref="IConverter"/> to view the underlying cache's keys
        /// through.
        /// </param>
        /// <param name="convKeyDown">
        /// The <see cref="IConverter"/> to use to pass keys down to the
        /// underlying cache.
        /// </param>
        /// <param name="convValUp">
        /// The <see cref="IConverter"/> to view the underlying cache's
        /// values through.
        /// </param>
        /// <param name="convValDown">
        /// The <see cref="IConverter"/> to use to pass values down to the
        /// underlying cache.
        /// </param>
        /// <returns>
        /// An <see cref="IInvocableCache"/> that views the keys and values
        /// of the passed <b>IInvocableCache</b> through the specified
        /// <b>IConverter</b>s.
        /// </returns>
        public static ConverterInvocableCache GetInvocableCache(IInvocableCache cache,
            IConverter convKeyUp, IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
        {
            return new ConverterInvocableCache(cache, convKeyUp, convKeyDown, convValUp, convValDown);
        }

        /// <summary>
        /// Returns a Converter instance of <see cref="IObservableCache"/>.
        /// </summary>
        /// <param name="cache">
        /// The underlying <see cref="IObservableCache"/>.
        /// </param>
        /// <param name="convKeyUp">
        /// The <see cref="IConverter"/> to view the underlying cache's keys
        /// through.
        /// </param>
        /// <param name="convKeyDown">
        /// The <see cref="IConverter"/> to use to pass keys down to the
        /// underlying cache.
        /// </param>
        /// <param name="convValUp">
        /// The <see cref="IConverter"/> to view the underlying cache's
        /// values through.
        /// </param>
        /// <param name="convValDown">
        /// The <see cref="IConverter"/> to use to pass values down to the
        /// underlying cache.
        /// </param>
        /// <returns>
        /// An <see cref="IObservableCache"/> that views the keys and values
        /// of the passed <b>IObservableCache</b> through the specified
        /// <b>IConverter</b>s.
        /// </returns>
        public static ConverterObservableCache GetObservableCache(IObservableCache cache,
            IConverter convKeyUp, IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
        {
            return new ConverterObservableCache(cache, convKeyUp, convKeyDown, convValUp, convValDown);
        }

        /// <summary>
        /// Returns a Converter instance of <see cref="IQueryCache"/>.
        /// </summary>
        /// <param name="cache">
        /// The underlying <see cref="IQueryCache"/>.
        /// </param>
        /// <param name="convKeyUp">
        /// The <see cref="IConverter"/> to view the underlying cache's keys
        /// through.
        /// </param>
        /// <param name="convKeyDown">
        /// The <see cref="IConverter"/> to use to pass keys down to the
        /// underlying cache.
        /// </param>
        /// <param name="convValUp">
        /// The <see cref="IConverter"/> to view the underlying cache's
        /// values through.
        /// </param>
        /// <param name="convValDown">
        /// The <see cref="IConverter"/> to use to pass values down to the
        /// underlying cache.
        /// </param>
        /// <returns>
        /// An <see cref="IQueryCache"/> that views the keys and values of
        /// the passed <b>IQueryCache</b> through the specified
        /// <b>IConverter</b>s.
        /// </returns>
        public static ConverterQueryCache GetQueryCache(IQueryCache cache, IConverter convKeyUp,
            IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
        {
            return new ConverterQueryCache(cache, convKeyUp, convKeyDown, convValUp, convValDown);
        }

        /// <summary>
        /// Returns a Converter instance of <see cref="INamedCache"/>.
        /// </summary>
        /// <param name="cache">
        /// The underlying <see cref="INamedCache"/>.
        /// </param>
        /// <param name="convKeyUp">
        /// The <see cref="IConverter"/> to view the underlying cache's keys
        /// through.
        /// </param>
        /// <param name="convKeyDown">
        /// The <see cref="IConverter"/> to use to pass keys down to the
        /// underlying cache.
        /// </param>
        /// <param name="convValUp">
        /// The <see cref="IConverter"/> to view the underlying cache's
        /// values through.
        /// </param>
        /// <param name="convValDown">
        /// The <see cref="IConverter"/> to use to pass values down to the
        /// underlying cache.
        /// </param>
        /// <returns>
        /// An <see cref="INamedCache"/> that views the keys and values of
        /// the passed <b>INamedCache</b> through the specified
        /// <b>IConverter</b>s.
        /// </returns>
        public static ConverterNamedCache GetNamedCache(INamedCache cache, IConverter convKeyUp,
            IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
        {
            return new ConverterNamedCache(cache, convKeyUp, convKeyDown, convValUp, convValDown);
        }
        
        /// <summary>
        /// Returns a Converter instance of a collection that holds
        /// <see cref="ICacheEntry"/> objects for a
        /// <see cref="ConverterCache"/>.
        /// </summary>
        /// <param name="col">
        /// The underlying collection of <see cref="ICacheEntry"/>
        /// objects.
        /// </param>
        /// <param name="convKeyUp">
        /// The <see cref="IConverter"/> to view the underlying entries'
        /// keys through.
        /// </param>
        /// <param name="convKeyDown">
        /// The <see cref="IConverter"/> to pass keys down to the
        /// underlying entries collection.
        /// </param>
        /// <param name="convValUp">
        /// The <see cref="IConverter"/> to view the underlying entries'
        /// values through.
        /// </param>
        /// <param name="convValDown">
        /// The <see cref="IConverter"/> to pass values down to the
        /// underlying entries collection.
        /// </param>
        /// <returns>
        /// A Converter collection that views the keys and values of the
        /// underlying collection's <b>ICacheEntry</b> objects through
        /// the specified key and value <b>IConverter</b>s.
        /// </returns>
        public static ConverterCacheEntries GetCacheEntries(ICollection col,
            IConverter convKeyUp, IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
        {
            return new ConverterCacheEntries(col, convKeyUp, convKeyDown, convValUp, convValDown);
        }

        /// <summary>
        /// Returns an instance of a <see cref="ICacheEntry"/> that uses
        /// <see cref="IConverter"/>s to retrieve the entry's data.
        /// </summary>
        /// <param name="entry">
        /// The underlying <see cref="ICacheEntry"/>.
        /// </param>
        /// <param name="convKeyUp">
        /// The <see cref="IConverter"/> to view the underlying entry's key.
        /// </param>
        /// <param name="convValUp">
        /// The <see cref="IConverter"/> to view the underlying entry's
        /// value.
        /// </param>
        /// <param name="convValDown">
        /// The <see cref="IConverter"/> to change the underlying entry's
        /// value.
        /// </param>
        /// <returns>
        /// A <see cref="ConverterCacheEntry"/> that converts the passed
        /// entry data using the specified <b>IConverter</b>s.
        /// </returns>
        public static ConverterCacheEntry GetCacheEntry(ICacheEntry entry, IConverter convKeyUp,
            IConverter convValUp, IConverter convValDown)
        {
            return new ConverterCacheEntry(entry, convKeyUp, convValUp, convValDown);
        }

        /// <summary>
        /// Returns an instance of a <see cref="CacheEventArgs"/> that uses
        /// <see cref="IConverter"/>s to retrieve the event data.
        /// </summary>
        /// <param name="cache">
        /// The new event's source.
        /// </param>
        /// <param name="evt">
        /// The underlying <see cref="CacheEventArgs"/>.
        /// </param>
        /// <param name="convKey">
        /// The <see cref="IConverter"/> to view the underlying
        /// <b>CacheEventArgs</b>' key.
        /// </param>
        /// <param name="convVal">
        /// The <see cref="IConverter"/> to view the underlying
        /// <b>CacheEventArgs</b>' values.
        /// </param>
        /// <returns>
        /// A <b>CacheEventArgs</b> that converts the passed event data using
        /// the specified <b>IConverter</b>s.
        /// </returns>
        public static CacheEventArgs GetCacheEventArgs(IObservableCache cache, CacheEventArgs evt,
            IConverter convKey, IConverter convVal)
        {
            return new ConverterCacheEventArgs(cache, evt, convKey, convVal);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Convert the contents of the passed array.
        /// </summary>
        /// <remarks>
        /// The conversion is done "in place" in the passed array.
        /// <p>
        /// This helper method is intended to support the functionality of
        /// <see cref="ICollection.CopyTo"/>.</p>
        /// </remarks>
        /// <param name="ao">
        /// An array of objects to convert.
        /// </param>
        /// <param name="conv">
        /// The <see cref="IConverter"/> to use to convert the objects.
        /// </param>
        /// <returns>
        /// The passed array.
        /// </returns>
        public static object[] ConvertArray(object[] ao, IConverter conv)
        {
            for (int i = 0, c = ao.Length; i < c; ++i)
            {
                ao[i] = conv.Convert(ao[i]);
            }
            return ao;
        }

        /// <summary>
        /// Convert the contents of the passed source array into an array
        /// with the element type of the passed destination array, using the
        /// destination array itself if it is large enough, and placing a
        /// <c>null</c> in the first unused element of the destination array
        /// if it is larger than the source array.
        /// </summary>
        /// <remarks>
        /// This helper method is intended to support the functionality of
        /// <see cref="ICollection.CopyTo"/>.
        /// </remarks>
        /// <param name="aoSrc">
        /// An array of objects to convert.
        /// </param>
        /// <param name="conv">
        /// The <see cref="IConverter"/> to use to convert the objects.
        /// </param>
        /// <param name="aoDest">
        /// The array to use to place the converted objects in if large
        /// enough, otherwise the array from which to obtain the element
        /// type to create a new array that is large enough.
        /// </param>
        /// <returns>
        /// An array whose component type is the same as the passed
        /// destination array and whose contents are the converted objects.
        /// </returns>
        public static object[] ConvertArray(object[] aoSrc, IConverter conv, object[] aoDest)
        {
            int cSrc  = aoSrc.Length;
            int cDest = aoDest.Length;
            if (cSrc > cDest)
            {
                cDest  = cSrc;
                aoDest = (object[]) Array.CreateInstance(aoDest.GetType().GetElementType(), cDest);
            }

            if (cDest > cSrc)
            {
                aoDest[cSrc] = null;
            }

            for (int i = 0; i < cSrc; ++i)
            {
                aoDest[i] = conv.Convert(aoSrc[i]);
            }

            return aoDest;
        }

        #endregion

        #region Inner class: ConverterCollection

        /// <summary>
        /// A Converter Collection views an underlying
        /// <see cref="ICollection"/> through an <see cref="IConverter"/>.
        /// </summary>
        [Serializable]
        public class ConverterCollection : ICollection
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="ICollection"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>ICollection</b>.
            /// </value>
            public virtual ICollection Collection
            {
                get { return m_col; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to view the underlying
            /// collection's values through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> from the underlying collection.
            /// </value>
            public virtual IConverter ConverterUp
            {
                get { return m_convUp; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to pass values down to the
            /// underlying collection.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> to the underlying collection.
            /// </value>
            public virtual IConverter ConverterDown
            {
                get { return m_convDown; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="col">
            /// The underlying <see cref="ICollection"/>.
            /// </param>
            /// <param name="convUp">
            /// The <see cref="IConverter"/> from the underlying collection.
            /// </param>
            /// <param name="convDown">
            /// The <b>IConverter</b> to the underlying collection.
            /// </param>
            public ConverterCollection(ICollection col, IConverter convUp, IConverter convDown)
            {
                Debug.Assert(col != null && convUp != null && convDown != null);

                m_col      = col;
                m_convUp   = convUp;
                m_convDown = convDown;
            }

            #endregion

            #region ICollection implementation

            /// <summary>
            /// Copies the elements of the <see cref="ICollection"/> to an
            /// array, starting at a particular index.
            /// </summary>
            /// <param name="array">
            /// The one-dimensional array that is the destination of the
            /// elements copied from collection. The array must have
            /// zero-based indexing.
            /// </param>
            /// <param name="index">
            /// The zero-based index in array at which copying begins.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Array is null.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Array is multidimensional or index is equal to or greater
            /// than the length of array or the number of elements in the
            /// source collection is greater than the available space from
            /// index to the end of the destination array.
            /// </exception>
            /// <exception cref="InvalidCastException">
            /// The type of the source collection cannot be cast
            /// automatically to the type of the destination array.
            /// </exception>
            public virtual void CopyTo(Array array, int index)
            {
                Collection.CopyTo(array, index);
                ConvertArray((object[]) array, ConverterUp);
            }

            /// <summary>
            /// Gets the number of elements contained in the collection.
            /// </summary>
            /// <returns>
            /// The number of elements contained in the collection.
            /// </returns>
            public virtual int Count
            {
                get { return Collection.Count; }
            }

            /// <summary>
            /// Gets an object that can be used to synchronize access to the
            /// collection.
            /// </summary>
            /// <returns>
            /// An object that can be used to synchronize access to the
            /// collection.
            /// </returns>
            public virtual object SyncRoot
            {
                get { return Collection.SyncRoot; }
            }

            /// <summary>
            /// Gets a value indicating whether access to the collection is
            /// synchronized (thread safe).
            /// </summary>
            /// <returns>
            /// <b>true</b> if access to the collection is synchronized
            /// (thread safe); otherwise, <b>false</b>.
            /// </returns>
            public virtual bool IsSynchronized
            {
                get { return Collection.IsSynchronized; }
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="IEnumerator"/> object that can be used to
            /// iterate through the collection.
            /// </returns>
            public virtual IEnumerator GetEnumerator()
            {
                return InstantiateEnumerator(Collection.GetEnumerator(), ConverterUp);
            }

            #endregion

            #region Object overrides

            /// <summary>
            /// Compares the specified object with this collection for
            /// equality.
            /// </summary>
            /// <param name="o">
            /// Object to be compared for equality with this collection.
            /// </param>
            /// <returns>
            /// <b>true</b> if the specified object is equal to this
            /// collection.
            /// </returns>
            public override bool Equals(object o)
            {
                if (o == this || o == null)
                {
                    return o == this;
                }

                if (o is ConverterCollection)
                {
                    ConverterCollection that = (ConverterCollection) o;
                    return Collection.Equals(that.Collection)
                        && ConverterUp.Equals(that.ConverterUp)
                        && ConverterDown.Equals(that.ConverterDown);
                }

                return false;
            }

            /// <summary>
            /// Returns the hash code value for this collection.
            /// </summary>
            /// <returns>
            /// The hash code value for this collection.
            /// </returns>
            public override int GetHashCode()
            {
                return Collection.GetHashCode()
                    ^ ConverterUp.GetHashCode()
                    ^ ConverterDown.GetHashCode();
            }

            /// <summary>
            /// Return a string description for this collection.
            /// </summary>
            /// <returns>
            /// A string description of the collection.
            /// </returns>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("ConverterCollection{");
                bool isFirst = true;
                foreach (object o in this)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                    sb.Append(o);
                }
                sb.Append('}');
                return sb.ToString();
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Drop references to the underlying collection and the
            /// converters.
            /// </summary>
            public virtual void Invalidate()
            {
                m_col      = null;
                m_convUp   = null;
                m_convDown = null;
            }

            /// <summary>
            /// Create a Converter enumerator.
            /// </summary>
            /// <param name="enumerator">
            /// The underlying <see cref="IEnumerator"/>.
            /// </param>
            /// <param name="conv">
            /// The <see cref="IConverter"/> to view the underlying
            /// <b>IEnumerator</b> through.
            /// </param>
            /// <returns>
            /// A Converter enumerator.
            /// </returns>
            protected virtual IEnumerator InstantiateEnumerator(IEnumerator enumerator, IConverter conv)
            {
                return ConverterCollections.GetEnumerator(enumerator, conv);
            }

            #endregion

            #region Data members

            /// <summary>
            /// The underlying ICollection.
            /// </summary>
            protected ICollection m_col;

            /// <summary>
            /// The IConverter from the underlying ICollection to this
            /// ICollection.
            /// </summary>
            protected IConverter m_convUp;

            /// <summary>
            /// The IConverter from this ICollection to the underlying
            /// ICollection.
            /// </summary>
            protected IConverter m_convDown;

            #endregion
        }

        #endregion

        #region Inner class: ConverterDictionaryEnumerator

        /// <summary>
        /// A Converter DictionaryEnumerator views an underlying
        /// <see cref="IDictionaryEnumerator"/> through key and value
        /// <see cref="IConverter"/>s.
        /// </summary>
        public class ConverterDictionaryEnumerator : IDictionaryEnumerator
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="IDictionaryEnumerator"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>IDictionaryEnumerator</b>.
            /// </value>
            public virtual IDictionaryEnumerator Enumerator
            {
                get { return m_enum; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to convert keys of the
            /// <see cref="DictionaryEntry"/> objects which underlying
            /// enumerator iterates.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> used to convert keys of the
            /// <b>DictionaryEntry</b> objects which underlying enumerator
            /// iterates.
            /// </value>
            public virtual IConverter ConverterKeyUp
            {
                get { return m_convKey; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to convert values of the
            /// <see cref="DictionaryEntry"/> objects which underlying
            /// enumerator iterates.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> used to convert values of the
            /// <b>DictionaryEntry</b> objects which underlying enumerator
            /// iterates.
            /// </value>
            public virtual IConverter ConverterValueUp
            {
                get { return m_convValue; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="enumerator">
            /// The underlying <see cref="IDictionaryEnumerator"/>.
            /// </param>
            /// <param name="convKey">
            /// The <see cref="IConverter"/> used to convert keys.
            /// </param>
            /// <param name="convValue">
            /// The <see cref="IConverter"/> used to convert values.
            /// </param>
            public ConverterDictionaryEnumerator(IDictionaryEnumerator enumerator, IConverter convKey, IConverter convValue)
            {
                m_enum      = enumerator;
                m_convKey   = convKey;
                m_convValue = convValue;
            }

            #endregion

            #region IDictionaryEnumerator implementation

            /// <summary>
            /// Gets the key of the current dictionary entry.
            /// </summary>
            /// <returns>
            /// The key of the current element of the enumeration.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first entry of the
            /// dictionary or after the last entry.
            /// </exception>
            public virtual object Key
            {
                get { return ((DictionaryEntry) Current).Key; }
            }

            /// <summary>
            /// Gets the value of the current dictionary entry.
            /// </summary>
            /// <returns>
            /// The value of the current element of the enumeration.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first entry of the
            /// dictionary or after the last entry.
            /// </exception>
            public virtual object Value
            {
                get { return ((DictionaryEntry) Current).Value; }
            }

            /// <summary>
            /// Gets both the key and the value of the current dictionary entry.
            /// </summary>
            /// <returns>
            /// A <see cref="DictionaryEntry"/> containing both the key and
            /// the value of the current dictionary entry.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first entry of the
            /// dictionary or after the last entry.
            /// </exception>
            public virtual DictionaryEntry Entry
            {
                get { return (DictionaryEntry) Current; }
            }

            #endregion

            #region IEnumerator implementation

            /// <summary>
            /// Advances the enumerator to the next element of the
            /// collection.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the enumerator was successfully advanced to
            /// the next element; <b>false</b> if the enumerator has passed
            /// the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public virtual bool MoveNext()
            {
                return Enumerator.MoveNext();
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before
            /// the first element in the collection.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public virtual void Reset()
            {
                Enumerator.Reset();
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <returns>
            /// The current element in the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first element of the
            /// collection or after the last element.
            /// </exception>
            public virtual object Current
            {
                get
                {
                    return new DictionaryEntry(ConverterKeyUp.Convert(Enumerator.Entry.Key),
                        ConverterValueUp.Convert(Enumerator.Entry.Value));
                }
            }

            #endregion

            #region Data members

            /// <summary>
            /// The underlying IDictionaryEnumerator.
            /// </summary>
            protected IDictionaryEnumerator m_enum;

            /// <summary>
            /// The IConverter used to convert keys.
            /// </summary>
            protected IConverter m_convKey;

            /// <summary>
            /// The IConverter used to convert values.
            /// </summary>
            protected IConverter m_convValue;

            #endregion
        }

        #endregion

        #region Inner class: ConverterDictionary

        /// <summary>
        /// A Converter Dictionary views an underlying
        /// <see cref="IDictionary"/> through a set of key and value
        /// <see cref="IConverter"/>s.
        /// </summary>
        [Serializable]
        public class ConverterDictionary : IDictionary
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="IDictionary"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>IDictionary</b>.
            /// </value>
            public virtual IDictionary Dictionary
            {
                get { return m_dictionary; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to view the underlying
            /// dictionary's keys through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> from the underlying dictionary's keys.
            /// </value>
            public virtual IConverter ConverterKeyUp
            {
                get { return m_convKeyUp; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to pass keys down to the
            /// underlying dictionary.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> to the underlying dictionary's keys.
            /// </value>
            public virtual IConverter ConverterKeyDown
            {
                get { return m_convKeyDown; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to view the underlying
            /// dictionary's values through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> from the underlying dictionary's
            /// values.
            /// </value>
            public virtual IConverter ConverterValueUp
            {
                get { return m_convValUp; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to pass values down to the
            /// underlying dictionary.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> to the underlying dictionary's values.
            /// </value>
            public virtual IConverter ConverterValueDown
            {
                get { return m_convValDown; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="dict">
            /// The underlying <see cref="IDictionary"/>.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> to view the underlying
            /// dictinary's keys through.
            /// </param>
            /// <param name="convKeyDown">
            /// The <see cref="IConverter"/> to use to pass keys down to the
            /// underlying dictionary.
            /// </param>
            /// <param name="convValUp">
            /// The <see cref="IConverter"/> to view the underlying
            /// dictionary's values through.
            /// </param>
            /// <param name="convValDown">
            /// The <see cref="IConverter"/> to use to pass values down to
            /// the underlying dictionary.
            /// </param>
            public ConverterDictionary(IDictionary dict, IConverter convKeyUp, IConverter convKeyDown,
                IConverter convValUp, IConverter convValDown)
            {
                Debug.Assert(dict != null && convKeyUp != null && convKeyDown != null
                    && convValUp != null && convValDown != null);

                m_dictionary  = dict;
                m_convKeyUp   = convKeyUp;
                m_convKeyDown = convKeyDown;
                m_convValUp   = convValUp;
                m_convValDown = convValDown;
            }

            #endregion

            #region IDictionary implementation

            /// <summary>
            /// Determines whether the dictionary contains an element with
            /// the specified key.
            /// </summary>
            /// <param name="key">
            /// The key to locate in the dictionary.
            /// </param>
            /// <returns>
            /// <b>true</b> if the dictionary contains an element with the
            /// key; otherwise, <b>false</b>.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// Key is <c>null</c>.
            /// </exception>
            public virtual bool Contains(object key)
            {
                return Dictionary.Contains(ConverterKeyDown.Convert(key));
            }

            /// <summary>
            /// Adds an element with the provided key and value to the
            /// dictionary.
            /// </summary>
            /// <param name="key">
            /// The object to use as the key of the element to add.
            /// </param>
            /// <param name="value">
            /// The object to use as the value of the element to add.
            /// </param>
            /// <exception cref="ArgumentException">
            /// An element with the same key already exists in the
            /// dictionary.
            /// </exception>
            /// <exception cref="ArgumentNullException">
            /// Key is <c>null</c>.
            /// </exception>
            /// <exception cref="NotSupportedException">
            /// The dictionary is read-only or the dictionary has a fixed
            /// size.
            /// </exception>
            public virtual void Add(object key, object value)
            {
                Dictionary.Add(ConverterKeyDown.Convert(key), ConverterValueDown.Convert(value));
            }

            /// <summary>
            /// Removes all elements from the dictionary.
            /// </summary>
            /// <exception cref="NotSupportedException">
            /// The dictionary is read-only.
            /// </exception>
            public virtual void Clear()
            {
                Dictionary.Clear();
            }

            /// <summary>
            /// Returns an <see cref="IDictionaryEnumerator"/> object for the
            /// dictionary.
            /// </summary>
            /// <returns>
            /// An <b>IDictionaryEnumerator</b> for the dictionary.
            /// </returns>
            public virtual IDictionaryEnumerator GetEnumerator()
            {
                return InstantiateDictionaryEnumerator(Dictionary.GetEnumerator(), ConverterKeyUp, ConverterValueUp);
            }

            /// <summary>
            /// Removes the element with the specified key from the
            /// dictionary.
            /// </summary>
            /// <param name="key">
            /// The key of the element to remove.
            /// </param>
            /// <exception cref="NotSupportedException">
            /// The dictionary is read-only or the dictionary has a fixed size.
            /// </exception>
            /// <exception cref="ArgumentNullException">
            /// Key is <c>null</c>.
            /// </exception>
            public virtual void Remove(object key)
            {
                Dictionary.Remove(ConverterKeyDown.Convert(key));
            }

            /// <summary>
            /// Gets or sets the element with the specified key.
            /// </summary>
            /// <param name="key">
            /// The key of the element to get or set.
            /// </param>
            /// <returns>
            /// The element with the specified key.
            /// </returns>
            /// <exception cref="NotSupportedException">
            /// The property is set and the dictionary object is read-only or
            /// the property is set, key does not exist in the collection,
            /// and the dictionary has a fixed size.
            /// </exception>
            /// <exception cref="ArgumentNullException">
            /// Key is <c>null</c>.
            /// </exception>
            public virtual object this[object key]
            {
                get { return ConverterValueUp.Convert(Dictionary[ConverterKeyDown.Convert(key)]); }
                set { Dictionary[ConverterKeyDown.Convert(key)] = ConverterValueDown.Convert(value); }
            }

            /// <summary>
            /// Gets an <see cref="ICollection"/> containing the keys of the
            /// dictionary.
            /// </summary>
            /// <returns>
            /// An <b>ICollection</b> containing the keys of the dictionary.
            /// </returns>
            public virtual ICollection Keys
            {
                get
                {
                    return InstantiateCollection(Dictionary.Keys, ConverterKeyUp, ConverterKeyDown);
                }
            }

            /// <summary>
            /// Gets an <see cref="ICollection"/> containing the values in
            /// the dictionary.
            /// </summary>
            /// <returns>
            /// An <b>ICollection</b> containing the values in the
            /// dictionary.
            /// </returns>
            public virtual ICollection Values
            {
                get
                {
                    return InstantiateCollection(Dictionary.Values, ConverterValueUp, ConverterValueDown);
                }
            }

            /// <summary>
            /// Gets a value indicating whether the dictionary is read-only.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the dictionary is read-only; otherwise,
            /// <b>false</b>.
            /// </returns>
            public virtual bool IsReadOnly
            {
                get { return Dictionary.IsReadOnly; }
            }

            /// <summary>
            /// Gets a value indicating whether the dictionary has a fixed
            /// size.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the dictionary has a fixed size; otherwise,
            /// <b>false</b>.
            /// </returns>
            public virtual bool IsFixedSize
            {
                get { return Dictionary.IsFixedSize; }
            }

            /// <summary>
            /// Copies the elements of the collection to an array, starting
            /// at a particular index.
            /// </summary>
            /// <param name="array">
            /// The one-dimensional array that is the destination of the
            /// elements copied from collection. The array must have
            /// zero-based indexing.
            /// </param>
            /// <param name="index">
            /// The zero-based index in array at which copying begins.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Array is null.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero.
            /// </exception>
            /// <exception cref="ArgumentException">Array is multidimensional
            /// or index is equal to or greater than the length of array or
            /// the number of elements in the source collection is greater
            /// than the available space from index to the end of the
            /// destination array.
            /// </exception>
            /// <exception cref="InvalidCastException">
            /// The type of the source collection cannot be cast
            /// automatically to the type of the destination array.
            /// </exception>
            public virtual void CopyTo(Array array, int index)
            {
                object[] entries = new object[Count];
                int      c       = 0;
                foreach (DictionaryEntry entry in this)
                {
                    entries[c++] = entry;
                }
                Array.Copy(entries, array, entries.Length);
            }

            /// <summary>
            /// Gets the number of elements contained in the dictionary.
            /// </summary>
            /// <returns>
            /// The number of elements contained in the dictionary.
            /// </returns>
            public virtual int Count
            {
                get { return Dictionary.Count; }
            }

            /// <summary>
            /// Gets an object that can be used to synchronize access to the
            /// dictionary.
            /// </summary>
            /// <returns>
            /// An object that can be used to synchronize access to the
            /// dictionary.
            /// </returns>
            public virtual object SyncRoot
            {
                get { return Dictionary.SyncRoot; }
            }

            /// <summary>
            /// Gets a value indicating whether access to the dictionary is
            /// synchronized (thread safe).
            /// </summary>
            /// <returns>
            /// <b>true</b> if access to the dictionary is synchronized
            /// (thread safe); otherwise, <b>false</b>.
            /// </returns>
            public virtual bool IsSynchronized
            {
                get { return Dictionary.IsSynchronized; }
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="IEnumerator"/> object that can be used to
            /// iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return InstantiateDictionaryEnumerator(Dictionary.GetEnumerator(), ConverterKeyUp, ConverterValueUp);
            }

            #endregion

            #region Factory methods

            /// <summary>
            /// Create a Converter Collection.
            /// </summary>
            /// <param name="col">
            /// The underlying collection.
            /// </param>
            /// <param name="convUp">
            /// The <see cref="IConverter"/> to view the underlying
            /// collection through.
            /// </param>
            /// <param name="convDown">
            /// The <see cref="IConverter"/> to pass items down to the
            /// underlying collection through.
            /// </param>
            /// <returns>
            /// A <see cref="ConverterCollection"/>.
            /// </returns>
            protected virtual ICollection InstantiateCollection(ICollection col, IConverter convUp, IConverter convDown)
            {
                return GetCollection(col, convUp, convDown);
            }

            /// <summary>
            /// Create a Converter Dictionary.
            /// </summary>
            /// <param name="dict">
            /// The underlying <see cref="IDictionary"/>.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> to view the underlying
            /// dictionary's keys through.
            /// </param>
            /// <param name="convKeyDown">
            /// The <see cref="IConverter"/> to use to pass keys down to the
            /// underlying dictionary.
            /// </param>
            /// <param name="convValUp">
            /// The <see cref="IConverter"/> to view the underlying
            /// dictionary's values through.
            /// </param>
            /// <param name="convValDown">
            /// The <see cref="IConverter"/> to use to pass values down to
            /// the underlying dictionary.
            /// </param>
            /// <returns>
            /// A <see cref="ConverterDictionary"/>.
            /// </returns>
            protected virtual IDictionary InstantiateDictionary(IDictionary dict, IConverter convKeyUp,
                IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
            {
                return GetDictionary(dict, convKeyUp, convKeyDown, convValUp, convValDown);
            }

            /// <summary>
            /// Returns an instance of <see cref="IDictionaryEnumerator"/> that
            /// uses an <see cref="IConverter"/>s to view an underlying
            /// enumerator.
            /// </summary>
            /// <param name="enumerator">
            /// The underlying <b>IDictionaryEnumerator</b>.
            /// </param>
            /// <param name="convKey">
            /// The <b>IConverter</b> to view the underlying
            /// <b>IDictionaryEnumerator</b> keys through.
            /// </param>
            /// <param name="convVal">
            /// The <b>IConverter</b> to view the underlying
            /// <b>IDictionaryEnumerator</b> values through.
            /// </param>
            /// <returns>
            /// An <see cref="IDictionaryEnumerator"/> that views the passed
            /// enumerator through the specified <b>IConverter</b>s.
            /// </returns>
            protected virtual IDictionaryEnumerator InstantiateDictionaryEnumerator(IDictionaryEnumerator enumerator,
                IConverter convKey, IConverter convVal)
            {
                return GetDictionaryEnumerator(enumerator, convKey, convVal);
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Return a string description for this dictionary.
            /// </summary>
            /// <returns>
            /// A string description of the dictionary.
            /// </returns>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("ConverterDictionary{");
                bool isFirst = true;
                foreach (object o in this)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                    sb.Append(o);
                }
                sb.Append("}");
                return sb.ToString();
            }

            #endregion

            #region Data members

            /// <summary>
            /// The underlying dictionary.
            /// </summary>
            protected IDictionary m_dictionary;

            /// <summary>
            /// The IConverter used to view keys stored in the dictionary.
            /// </summary>
            protected IConverter m_convKeyUp;

            /// <summary>
            /// The IConverter used to pass keys down to the dictionary.
            /// </summary>
            protected IConverter m_convKeyDown;

            /// <summary>
            /// The IConverter used to view values stored in the dictionary.
            /// </summary>
            protected IConverter m_convValUp;

            /// <summary>
            /// The IConverter used to pass keys down to the dictionary.
            /// </summary>
            protected IConverter m_convValDown;

            #endregion
        }

        #endregion

        #region Inner class: ConverterCacheEnumerator

        /// <summary>
        /// A Converter CacheEnumerator views an underlying
        /// <see cref="ICacheEnumerator"/> through key and value
        /// <see cref="IConverter"/>s.
        /// </summary>
        public class ConverterCacheEnumerator : ICacheEnumerator
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="ICacheEnumerator"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>ICacheEnumerator</b>.
            /// </value>
            public virtual ICacheEnumerator CacheEnumerator
            {
                get { return m_cacheEnum; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to convert keys of the
            /// <see cref="ICacheEntry"/> objects which underlying enumerator
            /// iterates.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> used to convert keys of the
            /// <b>ICacheEntry</b> objects which underlying enumerator
            /// iterates.
            /// </value>
            public virtual IConverter ConverterKeyUp
            {
                get { return m_convKeyUp; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to convert values of the
            /// <see cref="ICacheEntry"/> objects which underlying enumerator
            /// iterates.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> used to convert values of the
            /// <b>ICacheEntry</b> objects which underlying enumerator
            /// iterates.
            /// </value>
            public virtual IConverter ConverterValueUp
            {
                get { return m_convValueUp; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to pass values down to the
            /// <see cref="ICacheEntry"/> objects of the underlying
            /// enumerator.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> used to pass values down to the
            /// <b>ICacheEntry</b> objects of the underlying enumerator.
            /// </value>
            public virtual IConverter ConverterValueDown
            {
                get { return m_convValDown; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="enumerator">
            /// The underlying <see cref="ICacheEnumerator"/>.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> used to view keys of entries
            /// being enumerated.
            /// </param>
            /// <param name="convValueUp">
            /// The <see cref="IConverter"/> used to view values of entries
            /// being enumerated.
            /// </param>
            /// <param name="convValueDown">
            /// The <see cref="IConverter"/> used to change values of entries
            /// being enumerated.
            /// </param>
            public ConverterCacheEnumerator(ICacheEnumerator enumerator, IConverter convKeyUp,
                IConverter convValueUp, IConverter convValueDown)
            {
                m_cacheEnum   = enumerator;
                m_convKeyUp   = convKeyUp;
                m_convValueUp = convValueUp;
                m_convValDown = convValueDown;
            }

            #endregion

            #region ICacheEnumerator implementation

            /// <summary>
            /// Gets both the key and the value of the current cache entry.
            /// </summary>
            /// <value>
            /// An <see cref="ICacheEntry"/> containing both the key and
            /// the value of the current cache entry.
            /// </value>
            public virtual ICacheEntry Entry
            {
                get { return (ICacheEntry) Current; }
            }

            #endregion

            #region IDictionaryEnumerator implementation

            /// <summary>
            /// Gets the key of the current cache entry.
            /// </summary>
            /// <returns>
            /// The key of the current element of the enumeration.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first entry of the
            /// cache or after the last entry.
            /// </exception>
            public virtual object Key
            {
                get { return ((ICacheEntry) Current).Key; }
            }

            /// <summary>
            /// Gets the value of the current cache entry.
            /// </summary>
            /// <returns>
            /// The value of the current element of the enumeration.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first entry of the
            /// cache or after the last entry.
            /// </exception>
            public virtual object Value
            {
                get { return ((ICacheEntry) Current).Value; }
            }

            /// <summary>
            /// Gets both the key and the value of the current cache entry.
            /// </summary>
            /// <returns>
            /// A <see cref="DictionaryEntry"/> containing both the key and
            /// the value of the current cache entry.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first entry of the
            /// cache or after the last entry.
            /// </exception>
            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    ICacheEntry entry = (ICacheEntry) Current;
                    return new DictionaryEntry(entry.Key, entry.Value);
                }
            }

            #endregion

            #region IEnumerator implementation

            /// <summary>
            /// Advances the enumerator to the next element of the
            /// collection.
            /// </summary>
            /// <returns>
            /// <b>true</b> if the enumerator was successfully advanced to
            /// the next element; <b>false</b> if the enumerator has passed
            /// the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public virtual bool MoveNext()
            {
                return CacheEnumerator.MoveNext();
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before
            /// the first element in the collection.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public virtual void Reset()
            {
                CacheEnumerator.Reset();
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <returns>
            /// The current element in the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first element of the
            /// collection or after the last element.
            /// </exception>
            public virtual object Current
            {
                get
                {
                    return InstantiateEntry(CacheEnumerator.Entry, ConverterKeyUp,
                        ConverterValueUp, ConverterValueDown);
                }
            }

            #endregion

            /// <summary>Returns an instance of a <see cref="ICacheEntry"/>
            /// that uses <see cref="IConverter"/>s to retrieve the entry's
            /// data.
            /// </summary>
            /// <param name="entry">
            /// The underlying <see cref="ICacheEntry"/>.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> to view the underlying entry's
            /// key.
            /// </param>
            /// <param name="convValUp">
            /// The <see cref="IConverter"/> to view the underlying entry's
            /// value.
            /// </param>
            /// <param name="convValDown">
            /// The <see cref="IConverter"/> to change the underlying entry's
            /// value.
            /// </param>
            /// <returns>
            /// An <b>ICacheEntry</b> instance.
            /// </returns>
            protected virtual ICacheEntry InstantiateEntry(ICacheEntry entry, IConverter convKeyUp,
                IConverter convValUp, IConverter convValDown)
            {
                return GetCacheEntry(entry, convKeyUp, convValUp, convValDown);
            }

            #region Data members

            /// <summary>
            /// The underlying ICacheEnumerator.
            /// </summary>
            protected ICacheEnumerator m_cacheEnum;

            /// <summary>
            /// The IConverter used to view keys of entries being enumerated.
            /// </summary>
            protected IConverter m_convKeyUp;

            /// <summary>
            /// The IConverter used to view values of entries being
            /// enumerated.
            /// </summary>
            protected IConverter m_convValueUp;

            /// <summary>
            /// The IConverter used to change values of entries being
            /// enumerated.
            /// </summary>
            protected IConverter m_convValDown;

            #endregion
        }

        #endregion

        #region Inner class: ConverterCache

        /// <summary>
        /// A Converter Cache views an underlying <see cref="ICache"/>
        /// through a set of key and value <see cref="IConverter"/>s.
        /// </summary>
        [Serializable]
        public class ConverterCache : ConverterDictionary, ICache
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="ICache"/>.
            /// </summary>
            /// <returns>
            /// The underlying <b>ICache</b>.
            /// </returns>
            public virtual ICache Cache
            {
                get { return (ICache) Dictionary; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="cache">
            /// The underlying <see cref="ICache"/>.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// keys through.
            /// </param>
            /// <param name="convKeyDown">
            /// The <see cref="IConverter"/> to use to pass keys down to the
            /// underlying cache.
            /// </param>
            /// <param name="convValUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// values through.
            /// </param>
            /// <param name="convValDown">
            /// The <see cref="IConverter"/> to use to pass values down to
            /// the underlying cache.
            /// </param>
            public ConverterCache(ICache cache, IConverter convKeyUp, IConverter convKeyDown,
                IConverter convValUp, IConverter convValDown)
                : base(cache, convKeyUp, convKeyDown, convValUp, convValDown)
            {}

            #endregion

            #region ICache implementation

            /// <summary>
            /// Get the values for all the specified keys, if they are in the
            /// cache.
            /// </summary>
            /// <remarks>
            /// <p>
            /// For each key that is in the cache, that key and its
            /// corresponding value will be placed in the dictionary that is
            /// returned by this method. The absence of a key in the returned
            /// dictionary indicates that it was not in the cache, which may
            /// imply (for caches that can load behind the scenes) that the
            /// requested data could not be loaded.</p>
            /// <p>
            /// The result of this method is defined to be semantically the
            /// same as the following implementation, without regards to
            /// threading issues:</p>
            /// <pre>
            /// IDictionary dict = new AnyDictionary();
            /// // could be a Hashtable (but does not have to)
            /// foreach (object key in colKeys)
            /// {
            ///     object value = this[key];
            ///     if (value != null || Contains(key))
            ///     {
            ///         dict[key] = value;
            ///     }
            /// }
            /// return dict;
            /// </pre>
            /// </remarks>
            /// <param name="keys">
            /// A collection of keys that may be in the named cache.
            /// </param>
            /// <returns>
            /// A dictionary of keys to values for the specified keys passed
            /// in <paramref name="keys"/>.
            /// </returns>
            public virtual IDictionary GetAll(ICollection keys)
            {
                IConverter  convKeyDown = ConverterKeyDown;
                IConverter  convKeyUp   = ConverterKeyUp;
                IConverter  convValDown = ConverterValueDown;
                IConverter  convValUp   = ConverterValueUp;
                ICollection colKeysConv = InstantiateCollection(keys, convKeyDown, convKeyUp);

                return InstantiateDictionary(Cache.GetAll(colKeysConv), convKeyUp, convKeyDown, convValUp, convValDown);
            }

            /// <summary>
            /// Associates the specified value with the specified key in this
            /// cache.
            /// </summary>
            /// <remarks>
            /// <p>
            /// If the cache previously contained a mapping for this key, the
            /// old value is replaced.</p>
            /// <p>
            /// Invoking this method is equivalent to the following call:
            /// <pre>
            /// Insert(key, value, CacheExpiration.Default);
            /// </pre></p>
            /// </remarks>
            /// <param name="key">
            /// Key with which the specified value is to be associated.
            /// </param>
            /// <param name="value">
            /// Value to be associated with the specified key.
            /// </param>
            /// <returns>
            /// Previous value associated with specified key, or <c>null</c>
            /// if there was no mapping for key. A <c>null</c> return can
            /// also indicate that the dictionary previously associated
            /// <c>null</c> with the specified key, if the implementation
            /// supports <c>null</c> values.
            /// </returns>
            public virtual object Insert(object key, object value)
            {
                return ConverterValueUp.Convert(
                    Cache.Insert(ConverterKeyDown.Convert(key), ConverterValueDown.Convert(value)));
            }

            /// <summary>
            /// Associates the specified value with the specified key in this
            /// cache.
            /// </summary>
            /// <remarks>
            /// <p>
            /// If the cache previously contained a mapping for this key, the
            /// old value is replaced.</p>
            /// This variation of the <see cref="Insert(object, object)"/>
            /// method allows the caller to specify an expiry (or "time to
            /// live") for the cache entry.
            /// </remarks>
            /// <param name="key">
            /// Key with which the specified value is to be associated.
            /// </param>
            /// <param name="value">
            /// Value to be associated with the specified key.
            /// </param>
            /// <param name="millis">
            /// The number of milliseconds until the cache entry will expire,
            /// also referred to as the entry's "time to live"; pass
            /// <see cref="CacheExpiration.DEFAULT"/> to use the cache's
            /// default time-to-live setting; pass
            /// <see cref="CacheExpiration.NEVER"/> to indicate that the
            /// cache entry should never expire; this milliseconds value is
            /// <b>not</b> a date/time value, but the amount of time object
            /// will be kept in the cache.
            /// </param>
            /// <returns>
            /// Previous value associated with specified key, or <c>null</c>
            /// if there was no mapping for key. A <c>null</c> return can
            /// also indicate that the cache previously associated
            /// <c>null</c> with the specified key, if the implementation
            /// supports <c>null</c> values.
            /// </returns>
            /// <exception cref="NotSupportedException">
            /// If the requested expiry is a positive value and the
            /// implementation does not support expiry of cache entries.
            /// </exception>
            public virtual object Insert(object key, object value, long millis)
            {
                return ConverterValueUp.Convert(
                    Cache.Insert(ConverterKeyDown.Convert(key), ConverterValueDown.Convert(value), millis));
            }

            /// <summary>
            /// Copies all of the mappings from the specified dictionary to this
            /// cache (optional operation).
            /// </summary>
            /// <remarks>
            /// These mappings will replace any mappings that this cache had for
            /// any of the keys currently in the specified dictionary.
            /// </remarks>
            /// <param name="dictionary">
            /// Mappings to be stored in this cache.
            ///  </param>
            /// <exception cref="InvalidCastException">
            /// If the class of a key or value in the specified dictionary
            /// prevents it from being stored in this cache.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// If the lock could not be succesfully obtained for some key.
            /// </exception>
            /// <exception cref="NullReferenceException">
            /// This cache does not permit <c>null</c> keys or values, and the
            /// specified key or value is <c>null</c>.
            /// </exception>
            public virtual void InsertAll(IDictionary dictionary)
            {
                Cache.InsertAll(InstantiateDictionary(dictionary, ConverterKeyDown, ConverterKeyUp, ConverterValueDown, ConverterValueUp));
            }

            /// <summary>
            /// Gets a collection of <see cref="ICacheEntry"/> instances
            /// within the cache.
            /// </summary>
            public virtual ICollection Entries
            {
                get
                {
                    if (m_entries == null)
                    {
                        ICollection entries = Cache.Entries;
                        m_entries = InstantiateEntries(entries, ConverterKeyUp, ConverterKeyDown,
                            ConverterValueUp, ConverterValueDown);
                    }
                    return m_entries;
                }
            }

            /// <summary>
            /// Returns an <see cref="ICacheEnumerator"/> object for the
            /// <b>ICache</b> instance.
            /// </summary>
            /// <returns>An <b>ICacheEnumerator</b> object for the
            /// <b>ICache</b> instance.</returns>
            ICacheEnumerator ICache.GetEnumerator()
            {
                return InstantiateCacheEnumerator(Cache.GetEnumerator(),
                    ConverterKeyUp, ConverterValueUp, ConverterValueDown);
            }

            /// <summary>
            /// Returns an <see cref="IDictionaryEnumerator"/> object for the
            /// dictionary.
            /// </summary>
            /// <returns>
            /// An <b>IDictionaryEnumerator</b> for the dictionary.
            /// </returns>
            public override IDictionaryEnumerator GetEnumerator()
            {
                return ((ICache) this).GetEnumerator();
            }

            #endregion

            #region Factory methods

            /// <summary>
            /// Create a Converter Entry collection.
            /// </summary>
            /// <param name="col">
            /// The underlying collection of entries.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// entry keys through.
            /// </param>
            /// <param name="convKeyDown">
            /// The <see cref="IConverter"/> to use to pass keys down to the
            /// underlying cache's entry collection.
            /// </param>
            /// <param name="convValUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// entry values through.
            /// </param>
            /// <param name="convValDown">
            /// The <see cref="IConverter"/> to use to pass values down to
            /// the underlying cache's entry collection.
            /// </param>
            /// <returns>
            /// A Converter Entry collection.
            /// </returns>
            protected virtual ConverterCacheEntries InstantiateEntries(ICollection col, IConverter convKeyUp,
                IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
            {
                return GetCacheEntries(col, convKeyUp, convKeyDown, convValUp, convValDown);
            }

            /// <summary>
            /// Returns an instance of <see cref="ICacheEnumerator"/> that
            /// uses <see cref="IConverter"/>s to view an underlying
            /// enumerator.
            /// </summary>
            /// <param name="enumerator">
            /// The underlying <b>ICacheEnumerator</b>.
            /// </param>
            /// <param name="convKeyUp">
            /// The <b>IConverter</b> to view the underlying
            /// <b>ICacheEnumerator</b> keys through.
            /// </param>
            /// <param name="convValUp">
            /// The <b>IConverter</b> to view the underlying
            /// <b>ICacheEnumerator</b> values through.
            /// </param>
            /// <param name="convValDown">
            /// The <b>IConverter</b> to change the underlying
            /// <b>ICacheEnumerator</b> values through.
            /// </param>
            /// <returns>
            /// An <see cref="ICacheEnumerator"/> that views the passed
            /// enumerator through the specified <b>IConverter</b>s.
            /// </returns>
            protected virtual ICacheEnumerator InstantiateCacheEnumerator(ICacheEnumerator enumerator,
                IConverter convKeyUp, IConverter convValUp, IConverter convValDown)
            {
                return GetCacheEnumerator(enumerator, convKeyUp, convValUp, convValDown);
            }

            #endregion

            #region Data members

            /// <summary>
            /// The entry collection.
            /// </summary>
            [NonSerialized]
            protected ICollection m_entries;

            #endregion
        }

        #endregion
        
        #region Inner class: ConverterConcurrentCache

        /// <summary>
        /// A Converter ConcurrentCache views an underlying
        /// <see cref="IConcurrentCache"/> through a set of key and value
        /// <see cref="IConverter"/>s.
        /// </summary>
        [Serializable]
        public class ConverterConcurrentCache : ConverterCache, IConcurrentCache
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="IConcurrentCache"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>IConcurrentCache</b>.
            /// </value>
            public virtual IConcurrentCache ConcurrentCache
            {
                get { return (IConcurrentCache) Cache; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="cache">
            /// The underlying <see cref="IConcurrentCache"/>.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// keys through.
            /// </param>
            /// <param name="convKeyDown">
            /// The <see cref="IConverter"/> to use to pass keys down to the
            /// underlying cache.
            /// </param>
            /// <param name="convValUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// values through.
            /// </param>
            /// <param name="convValDown">
            /// The <see cref="IConverter"/> to use to pass values down to
            /// the underlying cache.
            /// </param>
            public ConverterConcurrentCache(IConcurrentCache cache, IConverter convKeyUp,
                IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
                : base(cache, convKeyUp, convKeyDown, convValUp, convValDown)
            {}

            #endregion

            #region IConcurrentCache implementation

            /// <summary>
            /// Attempt to lock the specified item within the specified
            /// period of time.
            /// </summary>
            /// <remarks>
            /// <p>
            /// The item doesn't have to exist to be <i>locked</i>. While the
            /// item is locked there is known to be a <i>lock holder</i>
            /// which has an exclusive right to modify (calling put and
            /// remove methods) that item.</p>
            /// <p>
            /// Lock holder is an abstract concept that depends on the
            /// IConcurrentCache implementation. For example, holder could
            /// be a cluster member or a thread (or both).</p>
            /// <p>
            /// Locking strategy may vary for concrete implementations as
            /// well. Lock could have an expiration time (this lock is
            /// sometimes called a "lease") or be held indefinitely (until
            /// the lock holder terminates).</p>
            /// <p>
            /// Some implementations may allow the entire cache to be locked.
            /// If the cache is locked in such a way, then only a lock holder
            /// is allowed to perform any of the "put" or "remove"
            /// operations.</p>
            /// <p>
            /// Pass the special constant
            /// <see cref="LockScope.LOCK_ALL"/> as the <i>key</i>
            /// parameter to indicate the cache lock.</p>
            /// </remarks>
            /// <param name="key">
            /// Key being locked.
            /// </param>
            /// <param name="waitTimeMillis">
            /// The number of milliseconds to continue trying to obtain a
            /// lock; pass zero to return immediately; pass -1 to block the
            /// calling thread until the lock could be obtained.
            /// </param>
            /// <returns>
            /// <b>true</b> if the item was successfully locked within the
            /// specified time; <b>false</b> otherwise.
            /// </returns>
            public virtual bool Lock(object key, long waitTimeMillis)
            {
                return ConcurrentCache.Lock(ConverterKeyDown.Convert(key), waitTimeMillis);
            }

            /// <summary>
            /// Attempt to lock the specified item and return immediately.
            /// </summary>
            /// <remarks>
            /// This method behaves exactly as if it simply performs the call
            /// <b>Lock(key, 0)</b>.
            /// </remarks>
            /// <param name="key">
            /// Key being locked.
            /// </param>
            /// <returns>
            /// <b>true</b> if the item was successfully locked; <b>false</b>
            /// otherwise.
            /// </returns>
            public virtual bool Lock(object key)
            {
                return ConcurrentCache.Lock(ConverterKeyDown.Convert(key));
            }

            /// <summary>
            /// Unlock the specified item.
            /// </summary>
            /// <remarks>
            /// The item doesn't have to exist to be <i>unlocked</i>.
            /// If the item is currently locked, only the <i>holder</i> of
            /// the lock could successfully unlock it.
            /// </remarks>
            /// <param name="key">
            /// Key being unlocked.
            /// </param>
            /// <returns>
            /// <b>true</b> if the item was successfully unlocked;
            /// <b>false</b> otherwise.
            /// </returns>
            public virtual bool Unlock(object key)
            {
                return ConcurrentCache.Unlock(ConverterKeyDown.Convert(key));
            }

            #endregion
        }

        #endregion

        #region Inner class: ConverterInvocableCache

        /// <summary>
        /// A Converter InvocableCache views an underlying
        /// <see cref="IInvocableCache"/> through a set of key and value
        /// <see cref="IConverter"/>s.
        /// </summary>
        [Serializable]
        public class ConverterInvocableCache : ConverterCache, IInvocableCache
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="IInvocableCache"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>IInvocableCache</b>.
            /// </value>
            public virtual IInvocableCache InvocableCache
            {
                get { return (IInvocableCache) Cache; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="cache">
            /// The underlying <see cref="IInvocableCache"/>.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// keys through.
            /// </param>
            /// <param name="convKeyDown">
            /// The <see cref="IConverter"/> to use to pass keys down to the
            /// underlying cache.
            /// </param>
            /// <param name="convValUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// values through.
            /// </param>
            /// <param name="convValDown">
            /// The <see cref="IConverter"/> to use to pass values down to
            /// the underlying cache.
            /// </param>
            public ConverterInvocableCache(IInvocableCache cache, IConverter convKeyUp,
                IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
                : base(cache, convKeyUp, convKeyDown, convValUp, convValDown)
            {}

            #endregion

            #region IInvocableCache implementation

            /// <summary>
            /// Invoke the passed <see cref="IEntryProcessor"/> against the
            /// entry specified by the passed key, returning the result of
            /// the invocation.
            /// </summary>
            /// <param name="key">
            /// The key to process; it is not required to exist within the
            /// dictionary.
            /// </param>
            /// <param name="agent">
            /// The <b>IEntryProcessor</b> to use to process the specified
            /// key.
            /// </param>
            /// <returns>
            /// The result of the invocation as returned from the
            /// <b>IEntryProcessor</b>.
            /// </returns>
            public virtual object Invoke(object key, IEntryProcessor agent)
            {
                return ConverterValueUp.Convert(InvocableCache.Invoke(ConverterKeyDown.Convert(key), agent));
            }

            /// <summary>
            /// Invoke the passed <see cref="IEntryProcessor"/> against the
            /// entries specified by the passed keys, returning the result of
            /// the invocation for each.
            /// </summary>
            /// <param name="keys">
            /// The keys to process; these keys are not required to exist
            /// within the dictionary.
            /// </param>
            /// <param name="agent">
            /// The <b>IEntryProcessor</b> to use to process the specified
            /// keys.
            /// </param>
            /// <returns>
            /// A dictionary containing the results of invoking the
            /// <b>IEntryProcessor</b> against each of the specified keys.
            /// </returns>
            public virtual IDictionary InvokeAll(ICollection keys, IEntryProcessor agent)
            {
                IConverter  convKeyDown = ConverterKeyDown;
                IConverter  convKeyUp   = ConverterKeyUp;
                ICollection colKeysConv = InstantiateCollection(keys, convKeyDown, convKeyUp);

                return InstantiateDictionary(InvocableCache.InvokeAll(colKeysConv, agent), convKeyUp,
                    convKeyDown, ConverterValueUp, ConverterValueDown);
            }

            /// <summary>
            /// Invoke the passed <see cref="IEntryProcessor"/> against the
            /// set of entries that are selected by the given
            /// <see cref="IFilter"/>, returning the result of the invocation
            /// for each.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Unless specified otherwise, IInvocableCache implementations
            /// will perform this operation in two steps: (1) use the filter
            /// to retrieve a matching entry collection; (2) apply the agent
            /// to every filtered entry. This algorithm assumes that the
            /// agent's processing does not affect the result of the
            /// specified filter evaluation, since the filtering and
            /// processing could be performed in parallel on different
            /// threads.</p>
            /// <p>
            /// If this assumption does not hold, the processor logic has to
            /// be idempotent, or at least re-evaluate the filter. This could
            /// be easily accomplished by wrapping the processor with the
            /// <see cref="ConditionalProcessor"/>.</p>
            /// </remarks>
            /// <param name="filter">
            /// An <see cref="IFilter"/> that results in the collection of
            /// keys to be processed.
            /// </param>
            /// <param name="agent">
            /// The <see cref="IEntryProcessor"/> to use to process the
            /// specified keys.
            /// </param>
            /// <returns>
            /// A dictionary containing the results of invoking the
            /// <b>IEntryProcessor</b> against the keys that are selected by
            /// the given <b>IFilter</b>.
            /// </returns>
            public virtual IDictionary InvokeAll(IFilter filter, IEntryProcessor agent)
            {
                return InstantiateDictionary(InvocableCache.InvokeAll(filter, agent), ConverterKeyUp,
                    ConverterKeyDown, ConverterValueUp, ConverterValueDown);
            }

            /// <summary>
            /// Perform an aggregating operation against the entries
            /// specified by the passed keys.
            /// </summary>
            /// <param name="keys">
            /// The collection of keys that specify the entries within this
            /// cache to aggregate across.
            /// </param>
            /// <param name="agent">
            /// The <see cref="IEntryAggregator"/> that is used to aggregate
            /// across the specified entries of this dictionary.
            /// </param>
            /// <returns>
            /// The result of the aggregation.
            /// </returns>
            public virtual object Aggregate(ICollection keys, IEntryAggregator agent)
            {
                IConverter  convKeyDown = ConverterKeyDown;
                IConverter  convKeyUp   = ConverterKeyUp;
                ICollection colKeysConv = InstantiateCollection(keys, convKeyDown, convKeyUp);

                return ConverterValueUp.Convert(InvocableCache.Aggregate(colKeysConv, agent));
            }

            /// <summary>
            /// Perform an aggregating operation against the collection of
            /// entries that are selected by the given <b>IFilter</b>.
            /// </summary>
            /// <param name="filter">
            /// an <see cref="IFilter"/> that is used to select entries
            /// within this cache to aggregate across.
            /// </param>
            /// <param name="agent">
            /// The <see cref="IEntryAggregator"/> that is used to aggregate
            /// across the selected entries of this dictionary.
            /// </param>
            /// <returns>
            /// The result of the aggregation.
            /// </returns>
            public virtual object Aggregate(IFilter filter, IEntryAggregator agent)
            {
                return ConverterValueUp.Convert(InvocableCache.Aggregate(filter, agent));
            }

            #endregion
        }

        #endregion

        #region Inner class: ConverterObservableCache

        /// <summary>
        /// A Converter ObservableCache views an underlying
        /// <see cref="IObservableCache"/> through a set of key and value
        /// <see cref="IConverter"/>s.
        /// </summary>
        [Serializable]
        public class ConverterObservableCache : ConverterCache, IObservableCache
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="IObservableCache"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>IObservableCache</b>.
            /// </value>
            public virtual IObservableCache ObservableCache
            {
                get { return (IObservableCache) Cache; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="cache">
            /// The underlying <see cref="IObservableCache"/>.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// keys through.
            /// </param>
            /// <param name="convKeyDown">
            /// The <see cref="IConverter"/> to use to pass keys down to the
            /// underlying cache.
            /// </param>
            /// <param name="convValUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// values through.
            /// </param>
            /// <param name="convValDown">
            /// The <see cref="IConverter"/> to use to pass values down to
            /// the underlying cache.
            /// </param>
            public ConverterObservableCache(IObservableCache cache, IConverter convKeyUp,
                IConverter convKeyDown, IConverter convValUp, IConverter convValDown)
                : base(cache, convKeyUp, convKeyDown, convValUp, convValDown)
            {}

            #endregion

            #region IObservableCache implementation

            /// <summary>
            /// Add a standard cache listener that will receive all events
            /// (inserts, updates, deletes) that occur against the cache,
            /// with the key, old-value and new-value included.
            /// </summary>
            /// <remarks>
            /// This has the same result as the following call:
            /// <pre>
            /// AddCacheListener(listener, (IFilter) null, false);
            /// </pre>
            /// </remarks>
            /// <param name="listener">
            /// The <see cref="ICacheListener"/> to add.
            /// </param>
            public virtual void AddCacheListener(ICacheListener listener)
            {
                ObservableCache.AddCacheListener(GetConverterListener(listener));
            }

            /// <summary>
            /// Remove a standard cache listener that previously signed up
            /// for all events.
            /// </summary>
            /// <remarks>
            /// This has the same result as the following call:
            /// <pre>
            /// RemoveCacheListener(listener, (IFilter) null);
            /// </pre>
            /// </remarks>
            /// <param name="listener">
            /// The <see cref="ICacheListener"/> to remove.
            /// </param>
            public virtual void RemoveCacheListener(ICacheListener listener)
            {
                ObservableCache.RemoveCacheListener(GetConverterListener(listener));
            }

            /// <summary>
            /// Add a cache listener for a specific key.
            /// </summary>
            /// <remarks>
            /// <p>
            /// The listeners will receive <see cref="CacheEventArgs"/>
            /// objects, but if <paramref name="isLite"/> is passed as
            /// <b>true</b>, they <i>might</i> not contain the
            /// <see cref="CacheEventArgs.OldValue"/> and
            /// <see cref="CacheEventArgs.NewValue"/> properties.</p>
            /// <p>
            /// To unregister the ICacheListener, use the
            /// <see cref="RemoveCacheListener(ICacheListener, object)"/>
            /// method.</p>
            /// </remarks>
            /// <param name="listener">
            /// The <see cref="ICacheListener"/> to add.
            /// </param>
            /// <param name="key">
            /// The key that identifies the entry for which to raise events.
            /// </param>
            /// <param name="isLite">
            /// <b>true</b> to indicate that the <see cref="CacheEventArgs"/>
            /// objects do not have to include the <b>OldValue</b> and
            /// <b>NewValue</b> property values in order to allow
            /// optimizations.
            /// </param>
            public virtual void AddCacheListener(ICacheListener listener, object key, bool isLite)
            {
                ObservableCache.AddCacheListener(GetConverterListener(listener),
                    ConverterKeyDown.Convert(key), isLite);
            }

            /// <summary>
            /// Remove a cache listener that previously signed up for events
            /// about a specific key.
            /// </summary>
            /// <param name="listener">
            /// The listener to remove.
            /// </param>
            /// <param name="key">
            /// The key that identifies the entry for which to raise events.
            /// </param>
            public virtual void RemoveCacheListener(ICacheListener listener, object key)
            {
                ObservableCache.RemoveCacheListener(GetConverterListener(listener),
                    ConverterKeyDown.Convert(key));
            }

            /// <summary>
            /// Add a cache listener that receives events based on a filter
            /// evaluation.
            /// </summary>
            /// <remarks>
            /// <p>
            /// The listeners will receive <see cref="CacheEventArgs"/>
            /// objects, but if <paramref name="isLite"/> is passed as
            /// <b>true</b>, they <i>might</i> not contain the
            /// <b>OldValue</b> and <b>NewValue</b> properties.</p>
            /// <p>
            /// To unregister the <see cref="ICacheListener"/>, use the
            /// <see cref="RemoveCacheListener(ICacheListener, IFilter)"/>
            /// method.</p>
            /// </remarks>
            /// <param name="listener">
            /// The <see cref="ICacheListener"/> to add.</param>
            /// <param name="filter">
            /// A filter that will be passed <b>CacheEventArgs</b> objects to
            /// select from; a <b>CacheEventArgs</b> will be delivered to the
            /// listener only if the filter evaluates to <b>true</b> for that
            /// <b>CacheEventArgs</b>; <c>null</c> is equivalent to a filter
            /// that alway returns <b>true</b>.
            /// </param>
            /// <param name="isLite">
            /// <b>true</b> to indicate that the <see cref="CacheEventArgs"/>
            /// objects do not have to include the <b>OldValue</b> and
            /// <b>NewValue</b> property values in order to allow
            /// optimizations.
            /// </param>
            public virtual void AddCacheListener(ICacheListener listener, IFilter filter, bool isLite)
            {
                ObservableCache.AddCacheListener(GetConverterListener(listener), filter, isLite);
            }

            /// <summary>
            /// Remove a cache listener that previously signed up for events
            /// based on a filter evaluation.
            /// </summary>
            /// <param name="listener">
            /// The <see cref="ICacheListener"/> to remove.
            /// </param>
            /// <param name="filter">
            /// A filter used to evaluate events; <c>null</c> is equivalent
            /// to a filter that alway returns <b>true</b>.
            /// </param>
            public virtual void RemoveCacheListener(ICacheListener listener, IFilter filter)
            {
                ObservableCache.RemoveCacheListener(GetConverterListener(listener), filter);
            }

            #endregion

            #region Helper methods

            /// <summary>
            /// Create a converter listener for the specified listener.
            /// </summary>
            /// <param name="listener">
            /// The underlying <see cref="ICacheListener"/>.
            /// </param>
            /// <returns>
            /// The converting listener.
            /// </returns>
            protected virtual ICacheListener GetConverterListener(ICacheListener listener)
            {
                // special case CacheTriggerListener, as it's not a "real" listener
                if (listener is CacheTriggerListener)
                {
                    return listener;
                }

                ICacheListener listenerConv =
                    new ConverterCacheListener(this, listener, ConverterKeyUp, ConverterValueUp);

                if (listener is CacheListenerSupport.ISynchronousListener)
                {
                    listenerConv = new CacheListenerSupport.WrapperSynchronousListener(listenerConv);
                }
                return listenerConv;
            }

            #endregion
        }

        #endregion

        #region Inner class: ConverterQueryCache

        /// <summary>
        /// A Converter QueryCache views an underlying
        /// <see cref="IQueryCache"/> through a set of key and value
        /// <see cref="IConverter"/>s.
        /// </summary>
        [Serializable]
        public class ConverterQueryCache : ConverterCache, IQueryCache
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="IQueryCache"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>IQueryCache</b>.
            /// </value>
            public virtual IQueryCache QueryCache
            {
                get { return (IQueryCache) Cache; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="cache">
            /// The underlying <see cref="IQueryCache"/>.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// keys through.
            /// </param>
            /// <param name="convKeyDown">
            /// The <see cref="IConverter"/> to use to pass keys down to the
            /// underlying cache.
            /// </param>
            /// <param name="convValUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// values through.
            /// </param>
            /// <param name="convValDown">
            /// The <see cref="IConverter"/> to use to pass values down to
            /// the underlying cache.
            /// </param>
            public ConverterQueryCache(IQueryCache cache, IConverter convKeyUp, IConverter convKeyDown,
                IConverter convValUp, IConverter convValDown)
                : base(cache, convKeyUp, convKeyDown, convValUp, convValDown)
            {}

            #endregion

            #region IQueryCache implementation

            /// <summary>
            /// Return a collection of the keys contained in this cache for
            /// entries that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <returns>
            /// A collection of keys for entries that satisfy the specified
            /// criteria.
            /// </returns>
            public virtual object[] GetKeys(IFilter filter)
            {
                ICollection result   = InstantiateCollection(QueryCache.GetKeys(filter), ConverterKeyUp, ConverterKeyDown);
                object[]    aoResult = new object[result.Count];
                result.CopyTo(aoResult, 0);
                return aoResult;
            }

            /// <summary>
            /// Return a collection of the values contained in this cache for
            /// entries that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <returns>
            /// A collection of the values for entries that satisfy the
            /// specified criteria.
            /// </returns>
            public virtual object[] GetValues(IFilter filter)
            {
                ICollection result   = InstantiateCollection(QueryCache.GetValues(filter), ConverterValueUp, ConverterValueDown);
                object[]    aoResult = new object[result.Count];
                result.CopyTo(aoResult, 0);
                return aoResult;
            }

            /// <summary>
            /// Return a collection of the values contained in this cache for
            /// entries that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <remarks>
            /// It is guaranteed that enumerator will traverse the array in
            /// such a way that the values come up in ascending order, sorted
            /// by the specified comparer or according to the
            /// <i>natural ordering</i>.
            /// </remarks>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <param name="comparer">
            /// The <b>IComparable</b> object which imposes an ordering on
            /// entries in the resulting collection; or <c>null</c> if the
            /// entries' values natural ordering should be used.
            /// </param>
            /// <returns>
            /// A collection of entries that satisfy the specified criteria.
            /// </returns>
            public virtual object[] GetValues(IFilter filter, IComparer comparer)
            {
                ICollection result   = InstantiateCollection(QueryCache.GetValues(filter, comparer), ConverterValueUp, ConverterValueDown);
                object[]    aoResult = new object[result.Count];
                result.CopyTo(aoResult, 0);
                return aoResult;
            }

            /// <summary>
            /// Return a collection of the entries contained in this cache
            /// that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <returns>
            /// A collection of entries that satisfy the specified criteria.
            /// </returns>
            public virtual ICacheEntry[] GetEntries(IFilter filter)
            {
                ConverterCacheEntries result  = InstantiateEntries(QueryCache.GetEntries(filter),
                    ConverterKeyUp, ConverterKeyDown, ConverterValueUp, ConverterValueDown);
                ICacheEntry[]         aResult = new ICacheEntry[result.Count];

                int c = 0;
                foreach (ICacheEntry entry in result)
                {
                    aResult[c++] = entry;
                }
                return aResult;
            }

            /// <summary>
            /// Return a collection of the entries contained in this cache
            /// that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <remarks>
            /// <p>
            /// It is guaranteed that enumerator will traverse the array in
            /// such a way that the entry values come up in ascending order,
            /// sorted by the specified comparer or according to the
            /// <i>natural ordering</i>.</p>
            /// </remarks>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <param name="comparer">
            /// The <b>IComparable</b> object which imposes an ordering on
            /// entries in the resulting collection; or <c>null</c> if the
            /// entries' values natural ordering should be used.
            /// </param>
            /// <returns>
            /// A collection of entries that satisfy the specified criteria.
            /// </returns>
            public virtual ICacheEntry[] GetEntries(IFilter filter, IComparer comparer)
            {
                ConverterCacheEntries result  = InstantiateEntries(QueryCache.GetEntries(filter, comparer),
                    ConverterKeyUp, ConverterKeyDown, ConverterValueUp, ConverterValueDown);
                ICacheEntry[]         aResult = new ICacheEntry[result.Count];

                int c = 0;
                foreach (ICacheEntry entry in result)
                {
                    aResult[c++] = entry;
                }
                return aResult;
            }

            /// <summary>
            /// Add an index to this IQueryCache.
            /// </summary>
            /// <remarks>
            /// This allows to correlate values stored in this
            /// <i>indexed cache</i> (or attributes of those values) to the
            /// corresponding keys in the indexed dictionary and increase the
            /// performance of <b>GetKeys</b> and <b>GetEntries</b> methods.
            /// </remarks>
            /// <param name="extractor">
            /// The <see cref="IValueExtractor"/> object that is used to
            /// extract an indexable object from a value stored in the
            /// indexed cache. Must not be <c>null</c>.
            /// </param>
            /// <param name="isOrdered">
            /// <b>true</b> if the contents of the indexed information should
            /// be ordered; <b>false</b> otherwise.
            /// </param>
            /// <param name="comparer">
            /// The <b>IComparer</b> object which imposes an ordering on
            /// entries in the indexed cache; or <c>null</c> if the entries'
            /// values natural ordering should be used.
            /// </param>
            public virtual void AddIndex(IValueExtractor extractor, bool isOrdered, IComparer comparer)
            {
                QueryCache.AddIndex(extractor, isOrdered, comparer);
            }

            /// <summary>
            /// Remove an index from this IQueryCache.
            /// </summary>
            /// <param name="extractor">
            /// The <see cref="IValueExtractor"/> object that is used to
            /// extract an indexable object from a value stored in the cache.
            /// </param>
            public virtual void RemoveIndex(IValueExtractor extractor)
            {
                QueryCache.RemoveIndex(extractor);
            }

            #endregion
        }

        #endregion

        #region Inner class: ConverterNamedCache

        /// <summary>
        /// A Converter NamedCache views an underlying
        /// <see cref="INamedCache"/> through a set of key and value
        /// <see cref="IConverter"/>s.
        /// </summary>
        [Serializable]
        public class ConverterNamedCache : ConverterCache, INamedCache
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="INamedCache"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>INamedCache</b>.
            /// </value>
            public virtual INamedCache NamedCache
            {
                get { return (INamedCache) Cache; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="cache">
            /// The underlying <see cref="INamedCache"/>.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// keys through.
            /// </param>
            /// <param name="convKeyDown">
            /// The <see cref="IConverter"/> to use to pass keys down to the
            /// underlying cache.
            /// </param>
            /// <param name="convValUp">
            /// The <see cref="IConverter"/> to view the underlying cache's
            /// values through.
            /// </param>
            /// <param name="convValDown">
            /// The <see cref="IConverter"/> to use to pass values down to
            /// the underlying cache.
            /// </param>
            public ConverterNamedCache(INamedCache cache, IConverter convKeyUp, IConverter convKeyDown,
                IConverter convValUp, IConverter convValDown)
                : base(cache, convKeyUp, convKeyDown, convValUp, convValDown)
            {
                m_cacheConcurrent = GetConcurrentCache(cache, convKeyUp, convKeyDown, convValUp, convValDown);
                m_cacheInvocable  = GetInvocableCache(cache, convKeyUp, convKeyDown, convValUp, convValDown);
                m_cacheQuery      = GetQueryCache(cache, convKeyUp, convKeyDown, convValUp, convValDown);
                m_cacheObservable = GetObservableCache(cache, convKeyUp, convKeyDown, convValUp, convValDown);
            }

            #endregion

            #region INamedCache implementation

            /// <summary>
            /// Gets the cache name.
            /// </summary>
            /// <value>
            /// The cache name.
            /// </value>
            public virtual string CacheName
            {
                get { return NamedCache.CacheName; }
            }

            /// <summary>
            /// Gets the <see cref="ICacheService"/> that this INamedCache is
            /// a part of.
            /// </summary>
            /// <value>
            /// The cache service this INamedCache is a part of.
            /// </value>
            public virtual ICacheService CacheService
            {
                get { return NamedCache.CacheService; }
            }

            /// <summary>
            /// Specifies whether or not the INamedCache is active.
            /// </summary>
            /// <value>
            /// <b>true</b> if the INamedCache is active; <b>false</b>
            /// otherwise.
            /// </value>
            public virtual bool IsActive
            {
                get { return NamedCache.IsActive; }
            }

            /// <summary>
            /// Release local resources associated with this instance of
            /// INamedCache.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Releasing a cache makes it no longer usable, but does not
            /// affect the cache itself. In other words, all other references
            /// to the cache will still be valid, and the cache data is not
            /// affected by releasing the reference.
            /// Any attempt to use this reference afterword will result in an
            /// exception.</p>
            /// </remarks>
            public virtual void Release()
            {
                NamedCache.Release();
            }


            /// <summary>
            /// Removes all mappings from this map.
            /// </summary>
            /// <remarks>
            /// Note: the removal of entries caused by this truncate operation will
            /// not be observable.
            /// </remarks>
            public virtual void Truncate()
            {
                NamedCache.Truncate();
            }

            /// <summary>
            /// Release and destroy this instance of INamedCache.
            /// </summary>
            /// <remarks>
            /// <p>
            /// <b>Warning:</b> This method is used to completely destroy the
            /// specified cache across the cluster. All references in the
            /// entire cluster to this cache will be invalidated, the cached
            /// data will be cleared, and all resources will be released.</p>
            /// </remarks>
            public virtual void Destroy()
            {
                NamedCache.Destroy();
            }

            /// <summary>
            /// Construct a view of this INamedCache.
            /// </summary>
            /// <returns>A local view for this INamedCache</returns>
            /// <see cref="ViewBuilder"/>
            /// <since>12.2.1.4</since>
            public virtual ViewBuilder View()
            {
                return new ViewBuilder(this);
            }

            #endregion

            #region IObservableCache implementation

            /// <summary>
            /// Add a standard cache listener that will receive all events
            /// (inserts, updates, deletes) that occur against the cache,
            /// with the key, old-value and new-value included.
            /// </summary>
            /// <remarks>
            /// This has the same result as the following call:
            /// <pre>
            /// AddCacheListener(listener, (IFilter) null, false);
            /// </pre>
            /// </remarks>
            /// <param name="listener">
            /// The <see cref="ICacheListener"/> to add.
            /// </param>
            public virtual void AddCacheListener(ICacheListener listener)
            {
                m_cacheObservable.AddCacheListener(listener);
            }

            /// <summary>
            /// Remove a standard cache listener that previously signed up
            /// for all events.
            /// </summary>
            /// <remarks>
            /// This has the same result as the following call:
            /// <pre>
            /// RemoveCacheListener(listener, (IFilter) null);
            /// </pre>
            /// </remarks>
            /// <param name="listener">
            /// The <see cref="ICacheListener"/> to remove.
            /// </param>
            public virtual void RemoveCacheListener(ICacheListener listener)
            {
                m_cacheObservable.RemoveCacheListener(listener);
            }

            /// <summary>
            /// Add a cache listener for a specific key.
            /// </summary>
            /// <remarks>
            /// <p>
            /// The listeners will receive <see cref="CacheEventArgs"/>
            /// objects, but if <paramref name="isLite"/> is passed as
            /// <b>true</b>, they <i>might</i> not contain the
            /// <see cref="CacheEventArgs.OldValue"/> and
            /// <see cref="CacheEventArgs.NewValue"/> properties.</p>
            /// <p>
            /// To unregister the ICacheListener, use the
            /// <see cref="RemoveCacheListener(ICacheListener, object)"/>
            /// method.</p>
            /// </remarks>
            /// <param name="listener">
            /// The <see cref="ICacheListener"/> to add.
            /// </param>
            /// <param name="key">
            /// The key that identifies the entry for which to raise events.
            /// </param>
            /// <param name="isLite">
            /// <b>true</b> to indicate that the <see cref="CacheEventArgs"/>
            /// objects do not have to include the <b>OldValue</b> and
            /// <b>NewValue</b> property values in order to allow
            /// optimizations.
            /// </param>
            public virtual void AddCacheListener(ICacheListener listener, object key, bool isLite)
            {
                m_cacheObservable.AddCacheListener(listener, key, isLite);
            }

            /// <summary>
            /// Remove a cache listener that previously signed up for events
            /// about a specific key.
            /// </summary>
            /// <param name="listener">
            /// The listener to remove.
            /// </param>
            /// <param name="key">
            /// The key that identifies the entry for which to raise events.
            /// </param>
            public virtual void RemoveCacheListener(ICacheListener listener, object key)
            {
                m_cacheObservable.RemoveCacheListener(listener, key);
            }

            /// <summary>
            /// Add a cache listener that receives events based on a filter
            /// evaluation.
            /// </summary>
            /// <remarks>
            /// <p>
            /// The listeners will receive <see cref="CacheEventArgs"/>
            /// objects, but if <paramref name="isLite"/> is passed as
            /// <b>true</b>, they <i>might</i> not contain the
            /// <b>OldValue</b> and <b>NewValue</b> properties.</p>
            /// <p>
            /// To unregister the <see cref="ICacheListener"/>, use the
            /// <see cref="RemoveCacheListener(ICacheListener, IFilter)"/>
            /// method.</p>
            /// </remarks>
            /// <param name="listener">
            /// The <see cref="ICacheListener"/> to add.</param>
            /// <param name="filter">
            /// A filter that will be passed <b>CacheEventArgs</b> objects to
            /// select from; a <b>CacheEventArgs</b> will be delivered to the
            /// listener only if the filter evaluates to <b>true</b> for that
            /// <b>CacheEventArgs</b>; <c>null</c> is equivalent to a filter
            /// that alway returns <b>true</b>.
            /// </param>
            /// <param name="isLite">
            /// <b>true</b> to indicate that the <see cref="CacheEventArgs"/>
            /// objects do not have to include the <b>OldValue</b> and
            /// <b>NewValue</b> property values in order to allow
            /// optimizations.
            /// </param>
            public virtual void AddCacheListener(ICacheListener listener, IFilter filter, bool isLite)
            {
                m_cacheObservable.AddCacheListener(listener, filter, isLite);
            }

            /// <summary>
            /// Remove a cache listener that previously signed up for events
            /// based on a filter evaluation.
            /// </summary>
            /// <param name="listener">
            /// The <see cref="ICacheListener"/> to remove.
            /// </param>
            /// <param name="filter">
            /// A filter used to evaluate events; <c>null</c> is equivalent
            /// to a filter that alway returns <b>true</b>.
            /// </param>
            public virtual void RemoveCacheListener(ICacheListener listener, IFilter filter)
            {
                m_cacheObservable.RemoveCacheListener(listener, filter);
            }

            #endregion

            #region IConcurrentCache implementation

            /// <summary>
            /// Attempt to lock the specified item within the specified
            /// period of time.
            /// </summary>
            /// <remarks>
            /// <p>
            /// The item doesn't have to exist to be <i>locked</i>. While the
            /// item is locked there is known to be a <i>lock holder</i>
            /// which has an exclusive right to modify (calling put and
            /// remove methods) that item.</p>
            /// <p>
            /// Lock holder is an abstract concept that depends on the
            /// IConcurrentCache implementation. For example, holder could
            /// be a cluster member or a thread (or both).</p>
            /// <p>
            /// Locking strategy may vary for concrete implementations as
            /// well. Lock could have an expiration time (this lock is
            /// sometimes called a "lease") or be held indefinitely (until
            /// the lock holder terminates).</p>
            /// <p>
            /// Some implementations may allow the entire cache to be locked.
            /// If the cache is locked in such a way, then only a lock holder
            /// is allowed to perform any of the "put" or "remove"
            /// operations.</p>
            /// <p>
            /// Pass the special constant
            /// <see cref="LockScope.LOCK_ALL"/> as the <i>key</i>
            /// parameter to indicate the cache lock.</p>
            /// </remarks>
            /// <param name="key">
            /// Key being locked.
            /// </param>
            /// <param name="waitTimeMillis">
            /// The number of milliseconds to continue trying to obtain a
            /// lock; pass zero to return immediately; pass -1 to block the
            /// calling thread until the lock could be obtained.
            /// </param>
            /// <returns>
            /// <b>true</b> if the item was successfully locked within the
            /// specified time; <b>false</b> otherwise.
            /// </returns>
            public virtual bool Lock(object key, long waitTimeMillis)
            {
                return m_cacheConcurrent.Lock(key, waitTimeMillis);
            }

            /// <summary>
            /// Attempt to lock the specified item and return immediately.
            /// </summary>
            /// <remarks>
            /// This method behaves exactly as if it simply performs the call
            /// <b>Lock(key, 0)</b>.
            /// </remarks>
            /// <param name="key">
            /// Key being locked.
            /// </param>
            /// <returns>
            /// <b>true</b> if the item was successfully locked; <b>false</b>
            /// otherwise.
            /// </returns>
            public virtual bool Lock(object key)
            {
                return m_cacheConcurrent.Lock(key);
            }

            /// <summary>
            /// Unlock the specified item.
            /// </summary>
            /// <remarks>
            /// The item doesn't have to exist to be <i>unlocked</i>.
            /// If the item is currently locked, only the <i>holder</i> of
            /// the lock could successfully unlock it.
            /// </remarks>
            /// <param name="key">
            /// Key being unlocked.
            /// </param>
            /// <returns>
            /// <b>true</b> if the item was successfully unlocked;
            /// <b>false</b> otherwise.
            /// </returns>
            public virtual bool Unlock(object key)
            {
                return m_cacheConcurrent.Unlock(key);
            }

            #endregion

            #region IQueryCache implementation

            /// <summary>
            /// Return a collection of the keys contained in this cache for
            /// entries that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <returns>
            /// A collection of keys for entries that satisfy the specified
            /// criteria.
            /// </returns>
            public virtual object[] GetKeys(IFilter filter)
            {
                return m_cacheQuery.GetKeys(filter);
            }

            /// <summary>
            /// Return a collection of the values contained in this cache for
            /// entries that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <returns>
            /// A collection of the values for entries that satisfy the
            /// specified criteria.
            /// </returns>
            public virtual object[] GetValues(IFilter filter)
            {
                return m_cacheQuery.GetValues(filter);
            }

            /// <summary>
            /// Return a collection of the values contained in this cache for
            /// entries that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <remarks>
            /// It is guaranteed that enumerator will traverse the array in
            /// such a way that the values come up in ascending order, sorted
            /// by the specified comparer or according to the
            /// <i>natural ordering</i>.
            /// </remarks>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <param name="comparer">
            /// The <b>IComparable</b> object which imposes an ordering on
            /// entries in the resulting collection; or <c>null</c> if the
            /// entries' values natural ordering should be used.
            /// </param>
            /// <returns>
            /// A collection of entries that satisfy the specified criteria.
            /// </returns>
            public virtual object[] GetValues(IFilter filter, IComparer comparer)
            {
                return m_cacheQuery.GetValues(filter, comparer);
            }

            /// <summary>
            /// Return a collection of the entries contained in this cache
            /// that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <returns>
            /// A collection of entries that satisfy the specified criteria.
            /// </returns>
            public virtual ICacheEntry[] GetEntries(IFilter filter)
            {
                return m_cacheQuery.GetEntries(filter);
            }

            /// <summary>
            /// Return a collection of the entries contained in this cache
            /// that satisfy the criteria expressed by the filter.
            /// </summary>
            /// <remarks>
            /// <p>
            /// It is guaranteed that enumerator will traverse the array in
            /// such a way that the entry values come up in ascending order,
            /// sorted by the specified comparer or according to the
            /// <i>natural ordering</i>.</p>
            /// </remarks>
            /// <param name="filter">
            /// The <see cref="IFilter"/> object representing the criteria
            /// that the entries of this cache should satisfy.
            /// </param>
            /// <param name="comparer">
            /// The <b>IComparable</b> object which imposes an ordering on
            /// entries in the resulting collection; or <c>null</c> if the
            /// entries' values natural ordering should be used.
            /// </param>
            /// <returns>
            /// A collection of entries that satisfy the specified criteria.
            /// </returns>
            public virtual ICacheEntry[] GetEntries(IFilter filter, IComparer comparer)
            {
                return m_cacheQuery.GetEntries(filter, comparer);
            }

            /// <summary>
            /// Add an index to this IQueryCache.
            /// </summary>
            /// <remarks>
            /// This allows to correlate values stored in this
            /// <i>indexed cache</i> (or attributes of those values) to the
            /// corresponding keys in the indexed dictionary and increase the
            /// performance of <b>GetKeys</b> and <b>GetEntries</b> methods.
            /// </remarks>
            /// <param name="extractor">
            /// The <see cref="IValueExtractor"/> object that is used to
            /// extract an indexable object from a value stored in the
            /// indexed cache. Must not be <c>null</c>.
            /// </param>
            /// <param name="isOrdered">
            /// <b>true</b> if the contents of the indexed information should
            /// be ordered; <b>false</b> otherwise.
            /// </param>
            /// <param name="comparer">
            /// The <b>IComparer</b> object which imposes an ordering on
            /// entries in the indexed cache; or <c>null</c> if the entries'
            /// values natural ordering should be used.
            /// </param>
            public virtual void AddIndex(IValueExtractor extractor, bool isOrdered, IComparer comparer)
            {
                m_cacheQuery.AddIndex(extractor, isOrdered, comparer);
            }

            /// <summary>
            /// Remove an index from this IQueryCache.
            /// </summary>
            /// <param name="extractor">
            /// The <see cref="IValueExtractor"/> object that is used to
            /// extract an indexable object from a value stored in the cache.
            /// </param>
            public virtual void RemoveIndex(IValueExtractor extractor)
            {
                m_cacheQuery.RemoveIndex(extractor);
            }

            #endregion

            #region IInvocableCache implementation

            /// <summary>
            /// Invoke the passed <see cref="IEntryProcessor"/> against the
            /// entry specified by the passed key, returning the result of
            /// the invocation.
            /// </summary>
            /// <param name="key">
            /// The key to process; it is not required to exist within the
            /// dictionary.
            /// </param>
            /// <param name="agent">
            /// The <b>IEntryProcessor</b> to use to process the specified
            /// key.
            /// </param>
            /// <returns>
            /// The result of the invocation as returned from the
            /// <b>IEntryProcessor</b>.
            /// </returns>
            public virtual object Invoke(object key, IEntryProcessor agent)
            {
                return m_cacheInvocable.Invoke(key, agent);
            }

            /// <summary>
            /// Invoke the passed <see cref="IEntryProcessor"/> against the
            /// entries specified by the passed keys, returning the result of
            /// the invocation for each.
            /// </summary>
            /// <param name="keys">
            /// The keys to process; these keys are not required to exist
            /// within the dictionary.
            /// </param>
            /// <param name="agent">
            /// The <b>IEntryProcessor</b> to use to process the specified
            /// keys.
            /// </param>
            /// <returns>
            /// A dictionary containing the results of invoking the
            /// <b>IEntryProcessor</b> against each of the specified keys.
            /// </returns>
            public virtual IDictionary InvokeAll(ICollection keys, IEntryProcessor agent)
            {
                return m_cacheInvocable.InvokeAll(keys, agent);
            }

            /// <summary>
            /// Invoke the passed <see cref="IEntryProcessor"/> against the
            /// set of entries that are selected by the given
            /// <see cref="IFilter"/>, returning the result of the invocation
            /// for each.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Unless specified otherwise, IInvocableCache implementations
            /// will perform this operation in two steps: (1) use the filter
            /// to retrieve a matching entry collection; (2) apply the agent
            /// to every filtered entry. This algorithm assumes that the
            /// agent's processing does not affect the result of the
            /// specified filter evaluation, since the filtering and
            /// processing could be performed in parallel on different
            /// threads.</p>
            /// <p>
            /// If this assumption does not hold, the processor logic has to
            /// be idempotent, or at least re-evaluate the filter. This could
            /// be easily accomplished by wrapping the processor with the
            /// <see cref="ConditionalProcessor"/>.</p>
            /// </remarks>
            /// <param name="filter">
            /// An <see cref="IFilter"/> that results in the collection of
            /// keys to be processed.
            /// </param>
            /// <param name="agent">
            /// The <see cref="IEntryProcessor"/> to use to process the
            /// specified keys.
            /// </param>
            /// <returns>
            /// A dictionary containing the results of invoking the
            /// <b>IEntryProcessor</b> against the keys that are selected by
            /// the given <b>IFilter</b>.
            /// </returns>
            public virtual IDictionary InvokeAll(IFilter filter, IEntryProcessor agent)
            {
                return m_cacheInvocable.InvokeAll(filter, agent);
            }

            /// <summary>
            /// Perform an aggregating operation against the entries
            /// specified by the passed keys.
            /// </summary>
            /// <param name="keys">
            /// The collection of keys that specify the entries within this
            /// cache to aggregate across.
            /// </param>
            /// <param name="agent">
            /// The <see cref="IEntryAggregator"/> that is used to aggregate
            /// across the specified entries of this dictionary.
            /// </param>
            /// <returns>
            /// The result of the aggregation.
            /// </returns>
            public virtual object Aggregate(ICollection keys, IEntryAggregator agent)
            {
                return m_cacheInvocable.Aggregate(keys, agent);
            }

            /// <summary>
            /// Perform an aggregating operation against the collection of
            /// entries that are selected by the given <b>IFilter</b>.
            /// </summary>
            /// <param name="filter">
            /// an <see cref="IFilter"/> that is used to select entries
            /// within this cache to aggregate across.
            /// </param>
            /// <param name="agent">
            /// The <see cref="IEntryAggregator"/> that is used to aggregate
            /// across the selected entries of this dictionary.
            /// </param>
            /// <returns>
            /// The result of the aggregation.
            /// </returns>
            public virtual object Aggregate(IFilter filter, IEntryAggregator agent)
            {
                return m_cacheInvocable.Aggregate(filter, agent);
            }

            #endregion

            #region IDisposable implementation

            /// <summary>
            /// Calls <see cref="Dispose"/> on the underlying cache to release the resources associated with the cache.
            /// </summary>
            public void Dispose()
            {
                NamedCache.Dispose();
            }

            #endregion

            #region Data members

            /// <summary>
            /// A Converter ConcurrentCache around the underlying
            /// INamedCache.
            /// </summary>
            protected ConverterConcurrentCache m_cacheConcurrent;

            /// <summary>
            /// A Converter InvocableCache around the underlying INamedCache.
            /// </summary>
            protected ConverterInvocableCache m_cacheInvocable;

            /// <summary>
            /// A Converter QueryCache around the underlying INamedCache.
            /// </summary>
            protected ConverterQueryCache m_cacheQuery;

            /// <summary>
            /// A Converter ObservableCache aroung the underlying
            /// INamedCache.
            /// </summary>
            protected ConverterObservableCache m_cacheObservable;

            #endregion
        }

        #endregion

        #region Inner class: AbstractConverterCacheEntry

        /// <summary>
        /// An abstract <see cref="ICacheEntry"/> that lazily converts the
        /// key and value.
        /// </summary>
        [Serializable]
        public abstract class AbstractConverterCacheEntry : ICacheEntry
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="ICacheEntry"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>ICacheEntry</b>.
            /// </value>
            public virtual ICacheEntry Entry
            {
                get { return m_entry; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="entry">
            /// The <see cref="ICacheEntry"/> to wrap.
            /// </param>
            protected AbstractConverterCacheEntry(ICacheEntry entry)
            {
                m_entry = entry;
            }

            #endregion

            #region Abstract properties

            /// <summary>
            /// The <see cref="IConverter"/> to view the underlying entry's
            /// key through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> to view the underlying entry's key
            /// through.
            /// </value>
            abstract protected IConverter ConverterKeyUp { get; }

            /// <summary>
            /// The <see cref="IConverter"/> to view the underlying entry's
            /// value through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> to view the underlying entry's value
            /// through.
            /// </value>
            abstract protected IConverter ConverterValueUp { get; }

            /// <summary>
            /// The <see cref="IConverter"/> used to change value in the
            /// underlying entry.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> used to change value in the underlying
            /// entry.
            /// </value>
            abstract protected IConverter ConverterValueDown { get; }

            #endregion

            #region ICacheEntry implementation

            /// <summary>
            /// Gets the key corresponding to this entry.
            /// </summary>
            /// <value>
            /// The key corresponding to this entry; may be <c>null</c> if
            /// the underlying dictionary supports <c>null</c> keys.
            /// </value>
            public virtual object Key
            {
                get
                {
                    object keyUp = m_keyUp;
                    if (keyUp == null)
                    {
                        m_keyUp = keyUp = ConverterKeyUp.Convert(Entry.Key);
                    }
                    return keyUp;
                }
            }

            /// <summary>
            /// Gets or sets the value corresponding to this entry.
            /// </summary>
            /// <value>
            /// The value corresponding to this entry; may be <c>null</c> if
            /// the value is <c>null</c> or if the entry does not exist in
            /// the cache.
            /// </value>
            public virtual object Value
            {
                get
                {
                    object valueUp = m_valueUp;
                    if (valueUp == null)
                    {
                        m_valueUp = valueUp = ConverterValueUp.Convert(Entry.Value);
                    }
                    return valueUp;
                }

                set
                {
                    m_valueUp = null;
                    Entry.Value = ConverterValueDown.Convert(value);
                }
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Compares the specified object with this entry for equality.
            /// </summary>
            /// <param name="o">
            /// Object to be compared for equality with this cache entry.
            /// </param>
            /// <returns>
            /// <b>true</b> if the specified object is equal to this cache
            /// entry.
            /// </returns>
            public override bool Equals(object o)
            {
                ICacheEntry that = o as ICacheEntry;
                if (this == that || that == null)
                {
                    return this == that;
                }

                return Equals(Key, that.Key) && Equals(Value, that.Value);
            }

            /// <summary>
            /// Returns the hash code value for this cache entry.
            /// </summary>
            /// <returns>
            /// The hash code value for this cache entry.
            /// </returns>
            public override int GetHashCode()
            {
                object key   = Key;
                object value = Value;
                return (key == null ? 0 : key.GetHashCode()) ^ (value == null ? 0 : value.GetHashCode());
            }

            /// <summary>
            /// Return a string description for this entry.
            /// </summary>
            /// <returns>
            /// A string description of the entry.
            /// </returns>
            public override string ToString()
            {
                return "ConverterCacheEntry{Key=\"" + Key + "\", Value=\"" + Value + "\"}";
            }

            #endregion

            #region Data members

            /// <summary>
            /// The underlying entry.
            /// </summary>
            protected ICacheEntry m_entry;

            /// <summary>
            /// Cached converted key.
            /// </summary>
            [NonSerialized]
            protected object m_keyUp;

            /// <summary>
            /// Cached converted value.
            /// </summary>
            [NonSerialized]
            protected object m_valueUp;

            #endregion
        }

        #endregion

        #region Inner class: ConverterCacheEntry

        /// <summary>
        /// An <see cref="ICacheEntry"/> that lazily converts the key and
        /// value.
        /// </summary>
        public class ConverterCacheEntry : AbstractConverterCacheEntry
        {
            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="entry">
            /// The <see cref="ICacheEntry"/> to wrap.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> to view the underlying entry's
            /// key through.
            /// </param>
            /// <param name="convValUp">
            /// The <see cref="IConverter"/> to view the underlying entry's
            /// value through.
            /// </param>
            /// <param name="convValDown">
            /// The <see cref="IConverter"/> to use to pass values down to
            /// the underlying entry.
            /// </param>
            public ConverterCacheEntry(ICacheEntry entry, IConverter convKeyUp, IConverter convValUp, IConverter convValDown)
                : base(entry)
            {
                m_convKeyUp   = convKeyUp;
                m_convValUp   = convValUp;
                m_convValDown = convValDown;
            }

            #endregion

            #region AbstractConverterCacheEntry implementation

            /// <summary>
            /// The <see cref="IConverter"/> to view the underlying entry's
            /// key through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> to view the underlying entry's key
            /// through.
            /// </value>
            protected override IConverter ConverterKeyUp
            {
                get { return m_convKeyUp; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> to view the underlying entry's
            /// value through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> to view the underlying entry's value
            /// through.
            /// </value>
            protected override IConverter ConverterValueUp
            {
                get { return m_convValUp; }
            }

            /// <summary>
            /// Return the <see cref="IConverter"/> used to change value in
            /// the underlying entry.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> used to change value in the underlying
            /// entry.
            /// </value>
            protected override IConverter ConverterValueDown
            {
                get { return m_convValDown; }
            }

            #endregion

            #region Data members

            /// <summary>
            /// The IConverter used to view the entry's key.
            /// </summary>
            protected IConverter m_convKeyUp;

            /// <summary>
            /// The IConverter used to view the entry's value.
            /// </summary>
            protected IConverter m_convValUp;

            /// <summary>
            /// The IConverter used to store the entry's value.
            /// </summary>
            protected IConverter m_convValDown;

            #endregion
        }

        #endregion

        #region Inner class: ConverterCacheEventArgs

        /// <summary>
        /// A Converter CacheEventArgs views an underlying
        /// <see cref="CacheEventArgs"/> through a set of key and value
        /// <see cref="IConverter"/>s.
        /// </summary>
        public class ConverterCacheEventArgs : CacheEventArgs
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="CacheEventArgs"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>CacheEventArgs</b>.
            /// </value>
            public virtual CacheEventArgs CacheEvent
            {
                get { return m_event; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to view the underlying
            /// <b>CacheEventArgs</b>' key through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> from the underlying
            /// <b>CacheEventArgs</b>' key.
            /// </value>
            public virtual IConverter ConverterKeyUp
            {
                get { return m_convKey; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to view the underlying
            /// <b>CacheEventArgs</b>' value through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> from the underlying
            /// <b>CacheEventArgs</b>' value.
            /// </value>
            public virtual IConverter ConverterValueUp
            {
                get { return m_convVal; }
            }

            /// <summary>
            /// An old value associated with this event.
            /// </summary>
            /// <remarks>
            /// The old value represents a value deleted from or updated in a
            /// cache. It is always <c>null</c> for "insert" notifications.
            /// </remarks>
            /// <value>
            /// An old value.
            /// </value>
            public override object OldValue
            {
                get
                {
                    object valueOld = m_valueOld;
                    if (valueOld == null)
                    {
                        valueOld = m_valueOld = ConverterValueUp.Convert(CacheEvent.OldValue);
                    }
                    return valueOld;
                }
                //set { m_valueOld = value; }
            }

            /// <summary>
            /// A new value associated with this event.
            /// </summary>
            /// <remarks>
            /// The new value represents a new value inserted into or updated
            /// in a cache. It is always <c>null</c> for "delete"
            /// notifications.
            /// </remarks>
            /// <value>
            /// A new value.
            /// </value>
            public override object NewValue
            {
                get
                {
                    object valueNew = m_valueNew;
                    if (valueNew == null)
                    {
                        valueNew = m_valueNew = ConverterValueUp.Convert(CacheEvent.NewValue);
                    }
                    return valueNew;
                }
                //set { m_valueNew = value; }
            }

            /// <summary>
            /// Gets a key associated with this event.
            /// </summary>
            /// <value>
            /// A key.
            /// </value>
            public override object Key
            {
                get
                {
                    object key = m_key;
                    if (key == null)
                    {
                        key = m_key = ConverterKeyUp.Convert(CacheEvent.Key);
                    }
                    return key;
                }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="source">
            /// The new event's source.
            /// </param>
            /// <param name="evt">
            /// The underlying <see cref="CacheEventArgs"/>.
            /// </param>
            /// <param name="convKey">
            /// The <see cref="IConverter"/> to view the underlying
            /// <b>CacheEventArgs</b>' key.
            /// </param>
            /// <param name="convVal">
            /// The <see cref="IConverter"/> to view the underlying
            /// <b>CacheEventArgs</b>' values.
            /// </param>
            public ConverterCacheEventArgs(IObservableCache source, CacheEventArgs evt, IConverter convKey, IConverter convVal)
                : base(source, evt.EventType, null, null, null, evt.IsSynthetic, evt.TransformState, evt.IsPriming, evt.IsExpired)
            {
                m_event   = evt;
                m_convKey = convKey;
                m_convVal = convVal;
            }

            #endregion

            #region Data members

            /// <summary>
            /// The underlying CacheEvent.
            /// </summary>
            protected CacheEventArgs m_event;

            /// <summary>
            /// The IConverter to view the underlying CacheEventArgs' key.
            /// </summary>
            protected IConverter m_convKey;

            /// <summary>
            /// The IConverter to view the underlying CacheEventArgs' value.
            /// </summary>
            protected IConverter m_convVal;

            #endregion
        }

        #endregion

        #region Inner class: ConverterCacheListener

        /// <summary>
        /// A converter CacheListener that converts events of the underlying
        /// <see cref="ICacheListener"/> for the underlying cache.
        /// </summary>
        public class ConverterCacheListener : ICacheListener
        {
            #region Properties

            /// <summary>
            /// The underlying <see cref="IObservableCache"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>IObservableCache</b>.
            /// </value>
            public virtual IObservableCache ObservableCache
            {
                get { return m_cache; }
            }

            /// <summary>
            /// The underlying <see cref="ICacheListener"/>.
            /// </summary>
            /// <value>
            /// The underlying <b>ICacheListener</b>.
            /// </value>
            public virtual ICacheListener CacheListener
            {
                get { return m_listener; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to view an underlying
            /// <see cref="CacheEventArgs"/>' key through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> from an underlying
            /// <b>CacheEventArgs</b>' key.
            /// </value>
            public virtual IConverter ConverterKeyUp
            {
                get { return m_convKey; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to view an underlying
            /// <see cref="CacheEventArgs"/>' value through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> from an underlying
            /// <b>CacheEventArgs</b>' value.
            /// </value>
            public virtual IConverter ConverterValueUp
            {
                get { return m_convVal; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="cache">
            /// The <see cref="IObservableCache"/> that should be the source
            /// for converted events.
            /// </param>
            /// <param name="listener">
            /// The underlying <see cref="ICacheListener"/>.
            /// </param>
            /// <param name="convKey">
            /// The <see cref="IConverter"/> to view the underlying
            /// <see cref="CacheEventArgs"/>' key.
            /// </param>
            /// <param name="convVal">
            /// The <see cref="IConverter"/> to view the underlying
            /// <see cref="CacheEventArgs"/>' value.
            /// </param>
            public ConverterCacheListener(IObservableCache cache, ICacheListener listener,
                IConverter convKey, IConverter convVal)
            {
                Debug.Assert(listener != null && convKey != null && convVal != null, "Null listener or converter");

                m_cache    = cache;
                m_listener = listener;
                m_convKey  = convKey;
                m_convVal  = convVal;
            }

            #endregion

            #region ICacheListener implementation

            /// <summary>
            /// Invoked when a cache entry has been inserted.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/> carrying the insert
            /// information.
            /// </param>
            public virtual void EntryInserted(CacheEventArgs evt)
            {
                CacheListener.EntryInserted(GetCacheEventArgs(ObservableCache,
                    evt, ConverterKeyUp, ConverterValueUp));
            }

            /// <summary>
            /// Invoked when a cache entry has been updated.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/> carrying the update
            /// information.
            /// </param>
            public virtual void EntryUpdated(CacheEventArgs evt)
            {
                CacheListener.EntryUpdated(GetCacheEventArgs(ObservableCache,
                    evt, ConverterKeyUp, ConverterValueUp));
            }

            /// <summary>
            /// Invoked when a cache entry has been deleted.
            /// </summary>
            /// <param name="evt">
            /// The <see cref="CacheEventArgs"/> carrying the remove
            /// information.
            /// </param>
            public virtual void EntryDeleted(CacheEventArgs evt)
            {
                CacheListener.EntryDeleted(GetCacheEventArgs(ObservableCache,
                    evt, ConverterKeyUp, ConverterValueUp));
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Determine a hash value for the listener.
            /// </summary>
            /// <returns>
            /// An integer hash value for this listener.
            /// </returns>
            public override int GetHashCode()
            {
                return CacheListener.GetHashCode();
            }

            /// <summary>
            /// Compare the ConverterCacheListener with another object to
            /// determine equality.
            /// </summary>
            /// <param name="o">
            /// The ConverterCacheListener object to compare to.
            /// </param>
            /// <returns>
            /// <b>true</b> iff this ConverterCacheListener and the passed
            /// object are equivalent listeners.
            /// </returns>
            public override bool Equals(object o)
            {
                if (o is ConverterCacheListener)
                {
                    ConverterCacheListener that = (ConverterCacheListener) o;
                    return CacheListener.Equals(that.CacheListener)
                        && ConverterKeyUp.Equals(that.ConverterKeyUp)
                        && ConverterValueUp.Equals(that.ConverterValueUp);
                }
                return false;
            }

            #endregion

            #region Data members

            /// <summary>
            /// The converting cache that will be the source of converted
            /// events.
            /// </summary>
            protected IObservableCache m_cache;

            /// <summary>
            /// The underlying ICacheListener.
            /// </summary>
            protected ICacheListener m_listener;

            /// <summary>
            /// The IConverter to view an underlying CacheEventArgs' key.
            /// </summary>
            protected IConverter m_convKey;

            /// <summary>
            /// The IConverter to view an underlying CacheEventArgs' value.
            /// </summary>
            protected IConverter m_convVal;

            #endregion
        }

        #endregion

        #region Inner class: ConverterCacheEntries

        /// <summary>
        /// A Converter Entry Collection views an underlying entry
        /// <see cref="ICollection"/> through a set of key and value
        /// <see cref="IConverter"/>s.
        /// </summary>
        public class ConverterCacheEntries : ICollection
        {
            #region Properties

            /// <summary>
            /// The underlying collection of <see cref="ICacheEntry"/>
            /// objects.
            /// </summary>
            /// <value>
            /// The underlying collection of entries.
            /// </value>
            public virtual ICollection Entries
            {
                get { return m_col; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to view the underlying
            /// entries' keys through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> from the underlying entries' keys.
            /// </value>
            public virtual IConverter ConverterKeyUp
            {
                get { return m_convKeyUp; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to pass keys down to the
            /// underlying entries collection.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> to the underlying entries' keys.
            /// </value>
            public virtual IConverter ConverterKeyDown
            {
                get { return m_convKeyDown; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to view the underlying
            /// entries' values through.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> from the underlying entries' values.
            /// </value>
            public virtual IConverter ConverterValueUp
            {
                get { return m_convValUp; }
            }

            /// <summary>
            /// The <see cref="IConverter"/> used to pass values down to the
            /// underlying entries collection.
            /// </summary>
            /// <value>
            /// The <b>IConverter</b> to the underlying entries' values.
            /// </value>
            public virtual IConverter ConverterValueDown
            {
                get { return m_convValDown; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="col">
            /// The underlying collection of entries.
            /// </param>
            /// <param name="convKeyUp">
            /// The <see cref="IConverter"/> to view the underlying entries'
            /// keys through.
            /// </param>
            /// <param name="convKeyDown">
            /// The <see cref="IConverter"/> to pass keys down to the
            /// underlying entries collection.
            /// </param>
            /// <param name="convValUp">
            /// The <see cref="IConverter"/> to view the underlying entries'
            /// values through.
            /// </param>
            /// <param name="convValDown">
            /// The <see cref="IConverter"/> to pass values down to the
            /// underlying entries collection.
            /// </param>
            public ConverterCacheEntries(ICollection col, IConverter convKeyUp, IConverter convKeyDown,
                IConverter convValUp, IConverter convValDown)
            {
                Debug.Assert(col != null && convKeyUp != null && convKeyDown != null
                    && convValUp != null && convValDown != null);

                m_col         = col;
                m_convKeyUp   = convKeyUp;
                m_convKeyDown = convKeyDown;
                m_convValUp   = convValUp;
                m_convValDown = convValDown;
            }

            #endregion

            #region ICollection implementation

            /// <summary>
            /// Copies the elements of the <see cref="ICollection"/> to an
            /// array, starting at a particular index.
            /// </summary>
            /// <param name="array">
            /// The one-dimensional array that is the destination of the
            /// elements copied from collection. The array must have
            /// zero-based indexing.
            /// </param>
            /// <param name="index">
            /// The zero-based index in array at which copying begins.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Array is null.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Index is less than zero.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Array is multidimensional or index is equal to or greater
            /// than the length of array or the number of elements in the
            /// source collection is greater than the available space from
            /// index to the end of the destination array.
            /// </exception>
            /// <exception cref="InvalidCastException">
            /// The type of the source collection cannot be cast
            /// automatically to the type of the destination array.
            /// </exception>
            public virtual void CopyTo(Array array, int index)
            {
                Entries.CopyTo(array, index);
                int c = array.Length;
                for (int i = 0; i < c; ++i)
                {
                    array.SetValue(WrapEntry((ICacheEntry) array.GetValue(i)), i);
                }
            }

            /// <summary>
            /// Gets the number of elements contained in the collection.
            /// </summary>
            /// <returns>
            /// The number of elements contained in the collection.
            /// </returns>
            public virtual int Count
            {
                get { return Entries.Count; }
            }

            /// <summary>
            /// Gets an object that can be used to synchronize access to the
            /// collection.
            /// </summary>
            /// <returns>
            /// An object that can be used to synchronize access to the
            /// collection.
            /// </returns>
            public virtual object SyncRoot
            {
                get { return Entries.SyncRoot; }
            }

            /// <summary>
            /// Gets a value indicating whether access to the collection is
            /// synchronized (thread safe).
            /// </summary>
            /// <returns>
            /// <b>true</b> if access to the collection is synchronized
            /// (thread safe); otherwise, <b>false</b>.
            /// </returns>
            public virtual bool IsSynchronized
            {
                get { return Entries.IsSynchronized; }
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="IEnumerator"/> object that can be used to
            /// iterate through the collection.
            /// </returns>
            public virtual IEnumerator GetEnumerator()
            {
                return WrapEnumerator(Entries.GetEnumerator());
            }

            #endregion

            #region Object override methods

            /// <summary>
            /// Compares the specified object with this collection for
            /// equality.
            /// </summary>
            /// <param name="o">
            /// Object to be compared for equality with this collection.
            /// </param>
            /// <returns>
            /// <b>true</b> if the specified object is equal to this
            /// collection.
            /// </returns>
            public override bool Equals(object o)
            {
                if (o == this || o == null)
                {
                    return o == this;
                }

                if (o is ConverterCacheEntries)
                {
                    ConverterCacheEntries that = (ConverterCacheEntries) o;
                    return Entries.Equals(that.Entries)
                        && ConverterKeyUp.Equals(that.ConverterKeyUp)
                        && ConverterKeyDown.Equals(that.ConverterKeyDown)
                        && ConverterValueUp.Equals(that.ConverterValueUp)
                        && ConverterValueDown.Equals(that.ConverterValueDown);
                }

                return false;
            }

            /// <summary>
            /// Returns the hash code value for this collection.
            /// </summary>
            /// <returns>
            /// The hash code value for this collection.
            /// </returns>
            public override int GetHashCode()
            {
                return Entries.GetHashCode()
                    ^ ConverterKeyUp.GetHashCode()
                    ^ ConverterKeyDown.GetHashCode()
                    ^ ConverterValueUp.GetHashCode()
                    ^ ConverterValueDown.GetHashCode();
            }

            /// <summary>
            /// Return a string description for this collection.
            /// </summary>
            /// <returns>
            /// A string description of the collection.
            /// </returns>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("ConverterCacheEntries{");
                bool isFirst = true;
                foreach (object o in this)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                    sb.Append(o);
                }
                sb.Append('}');
                return sb.ToString();
            }

            #endregion

            #region Helper methods

            /// <summary>
            /// Wrap an <see cref="ICacheEntry"/> from the entries collection
            /// to make a <see cref="ConverterEntry"/>.
            /// </summary>
            /// <param name="entry">
            /// An <b>ICacheEntry</b> to wrap.
            /// </param>
            /// <returns>
            /// An <b>ICacheEntry</b> that restricts its type.
            /// </returns>
            protected virtual ICacheEntry WrapEntry(ICacheEntry entry)
            {
                return new ConverterEntry(entry, this);
            }

            /// <summary>
            /// Wrap an <see cref="IEnumerator"/> from the entries collection
            /// to make a <see cref="ConverterCacheEnumerator"/>.
            /// </summary>
            /// <param name="enumerator">
            /// An <b>IEnumerator</b> to wrap.
            /// </param>
            /// <returns>
            /// A <b>ConverterCacheEnumerator</b>.
            /// </returns>
            protected IEnumerator WrapEnumerator(IEnumerator enumerator)
            {
                return new ConverterEnumerator(enumerator, this);
            }

            #endregion

            #region Data members

            /// <summary>
            /// The underlying collection of ICacheEntry objects.
            /// </summary>
            protected ICollection m_col;

            /// <summary>
            /// The IConverter used to view keys stored in the entry
            /// collection.
            /// </summary>
            protected IConverter m_convKeyUp;

            /// <summary>
            /// The IConverter used to pass keys down to the entry
            /// collection.
            /// </summary>
            protected IConverter m_convKeyDown;

            /// <summary>
            /// The IConverter used to view values stored in the entry
            /// collection.
            /// </summary>
            protected IConverter m_convValUp;

            /// <summary>
            /// The IConverter used to pass values down to the entry
            /// collection.
            /// </summary>
            protected IConverter m_convValDown;

            #endregion

            #region Inner class: ConverterCacheEntry

            /// <summary>
            /// A <see cref="ICacheEntry"/> that lazily converts the key and
            /// value.
            /// </summary>
            protected class ConverterEntry : AbstractConverterCacheEntry
            {
                #region Constructors

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="entry">
                /// The <see cref="ICacheEntry"/> to wrap.
                /// </param>
                /// <param name="parent">
                /// The parent <see cref="ConverterCacheEntries"/>.
                /// </param>
                public ConverterEntry(ICacheEntry entry, ConverterCacheEntries parent)
                    : base(entry)
                {
                    m_parent = parent;
                }

                #endregion

                #region AbstractConverterCacheEntry implementation

                /// <summary>
                /// Return the <see cref="IConverter"/> to view the underlying
                /// entry's key through.
                /// </summary>
                /// <value>
                /// The <b>IConverter</b> to view the underlying entry's key
                /// through.
                /// </value>
                protected override IConverter ConverterKeyUp
                {
                    get { return m_parent.ConverterKeyUp; }
                }

                /// <summary>
                /// Return the <see cref="IConverter"/> to view the underlying
                /// entry's value through.
                /// </summary>
                /// <value>
                /// The <b>IConverter</b> to view the underlying entry's value
                /// through.
                /// </value>
                protected override IConverter ConverterValueUp
                {
                    get { return m_parent.ConverterValueUp; }
                }

                /// <summary>
                /// Return the <see cref="IConverter"/> used to change value in
                /// the underlying entry.
                /// </summary>
                /// <value>
                /// The <b>IConverter</b> used to change value in the underlying
                /// entry.
                /// </value>
                protected override IConverter ConverterValueDown
                {
                    get { return m_parent.ConverterValueDown; }
                }

                #endregion

                #region Data members

                /// <summary>
                /// Parent entries collection.
                /// </summary>
                protected ConverterCacheEntries m_parent;

                #endregion
            }

            #endregion

            #region Inner class: ConverterEnumerator

            /// <summary>
            /// An <see cref="IEnumerator"/> that converts the key and value
            /// types.
            /// </summary>
            protected class ConverterEnumerator : IEnumerator
            {
                #region Properties

                /// <summary>
                /// The underlying <see cref="IEnumerator"/>.
                /// </summary>
                /// <value>
                /// The underlying <b>IEnumerator</b>.
                /// </value>
                public virtual IEnumerator Enumerator
                {
                    get { return m_enum; }
                }

                #endregion

                #region Constructors

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="enumerator">
                /// The underlying <see cref="IEnumerator"/>.
                /// </param>
                /// <param name="parent">
                /// Parent <see cref="ConverterCacheEntries"/>.
                /// </param>
                public ConverterEnumerator(IEnumerator enumerator, ConverterCacheEntries parent)
                {
                    m_enum   = enumerator;
                    m_parent = parent;
                }

                #endregion

                #region IEnumerator implementation

                /// <summary>
                /// Advances the enumerator to the next element of the
                /// collection.
                /// </summary>
                /// <returns>
                /// <b>true</b> if the enumerator was successfully advanced to
                /// the next element; <b>false</b> if the enumerator has passed
                /// the end of the collection.
                /// </returns>
                /// <exception cref="InvalidOperationException">
                /// The collection was modified after the enumerator was created.
                /// </exception>
                public virtual bool MoveNext()
                {
                    return Enumerator.MoveNext();
                }

                /// <summary>
                /// Sets the enumerator to its initial position, which is before
                /// the first element in the collection.
                /// </summary>
                /// <exception cref="InvalidOperationException">
                /// The collection was modified after the enumerator was created.
                /// </exception>
                public virtual void Reset()
                {
                    Enumerator.Reset();
                }

                /// <summary>
                /// Gets the current element in the collection.
                /// </summary>
                /// <returns>
                /// The current element in the collection.
                /// </returns>
                /// <exception cref="InvalidOperationException">
                /// The enumerator is positioned before the first element of the
                /// collection or after the last element.
                /// </exception>
                public virtual object Current
                {
                    get
                    {
                        return m_parent.WrapEntry((ICacheEntry) Enumerator.Current);
                    }
                }

                #endregion

                #region Data members

                /// <summary>
                /// The underlying IEnumerator.
                /// </summary>
                protected IEnumerator m_enum;

                /// <summary>
                /// Parent ConverterCacheEntries.
                /// </summary>
                protected ConverterCacheEntries m_parent;

                #endregion
            }

            #endregion
        }

        #endregion
    }
}