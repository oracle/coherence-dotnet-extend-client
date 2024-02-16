/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;

namespace Tangosol.Util.Collections
{
    /// <summary>
    /// Synchronized <b>IDictionary</b> wrapper that uses read/write locks 
    /// to synchronize access to the underlying dictionary.
    /// </summary>
    /// <remarks>
    /// This class uses read/write locks to ensure that only a single thread
    /// can modify the underlying dictionary at any given time, while allowing
    /// concurrent reads by multiple threads.
    /// <p/>
    /// While all individual operations exposed by this class are thread-safe,
    /// you may still need to synchronize access to an instance of this class 
    /// if you need to perform multiple operations atomically. 
    /// <p/>
    /// In order to do that, you can do one of the following:
    /// <list type="bullet">
    /// <item>
    /// <b>Lock the <see cref="SyncRoot"/> property.</b> Because the write 
    /// locks used internally also lock <c>SyncRoot</c>, this will prevent 
    /// concurrent modification. However, concurrent read operations will
    /// still be allowed, which means that other threads will be able to see
    /// partial updates. If you need truly atomic multi-operation updates, 
    /// you should use write locks instead.
    /// </item>
    /// <item>
    /// <b>Use read locks.</b> By acquiring a read lock externally, you can 
    /// ensure that no modifications take place while you are reading from the
    /// dictionary. See <see cref="AcquireReadLock()"/> for details.
    /// </item>
    /// <item>
    /// <b>Use write locks.</b> By acquiring a write lock, you can achieve 
    /// complete isolation and fully atomic multi-operation updates, as no 
    /// other thread will be able to either read from or write to the 
    /// dictionary until the write lock is released. See 
    /// <see cref="AcquireWriteLock()"/> for details.
    /// </item>
    /// </list>
    /// <p/>
    /// <b>Note 1:</b> If you attempt to acquire a write lock on a thread that 
    /// holds a read lock, the read lock will be promoted to a write lock as 
    /// soon as all read locks held by other threads are released.
    /// <p/>
    /// <b>Note 2:</b> The enumerator returned by the <see cref="GetEnumerator"/> 
    /// method is  <b>not</b> thread-safe. You should either acquire a read 
    /// lock or lock the <see cref="SyncRoot"/> explicitly if you need to 
    /// enumerate dictionary entries in a thread-safe manner.
    /// <p/>
    /// <b>Note 3:</b> This class has been renamed from SynchronizedHashtable to 
    /// SynchronizedDictionary in Coherence 3.5, to better reflect the fact
    /// that it can be used to wrap any <c>IDictionary</c> implementation.
    /// </remarks>
    /// <author>Aleksandar Seovic  2006.11.13</author>
    /// <author>Aleksandar Seovic  2009.08.03</author>
    [Serializable]
    public class SynchronizedDictionary : IDictionary, ISerializable
    {
        #region Properties

        /// <summary>
        /// Return the delegate IDictionary.
        /// </summary>
        public virtual IDictionary Delegate
        {
            get { return m_dict; }
        }

        /// <summary>
        /// Determines whether or not the current thread holds a read lock.
        /// </summary>
        /// <value>
        /// <b>true</b> if the current thread holds a read lock; <b>false</b>
        /// otherwise.
        /// </value>
        /// <seealso cref="AcquireReadLock()"/>
        /// <seealso cref="AcquireReadLock(int)"/>
        public virtual bool IsReadLockHeld
        {
            get
            {
                return m_gate.IsEnteredByCurrentThread;
            }
        }

        /// <summary>
        /// Determines whether or not the current thread holds the write lock.
        /// </summary>
        /// <value>
        /// <b>true</b> if the current thread holds the write lock; <b>false</b>
        /// otherwise.
        /// </value>
        /// <seealso cref="AcquireWriteLock()"/>
        /// <seealso cref="AcquireWriteLock(int)"/>
        public virtual bool IsWriteLockHeld
        {
            get
            {
                return m_gate.IsClosedByCurrentThread;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create <c>SynchronizedDictionary</c> instance.
        /// </summary>
        /// <remarks>
        /// This constructor will create a wrapper around the internal
        /// <see cref="HashDictionary"/> instance, which means that the 
        /// created dictionary instance will support <c>null</c> keys,
        /// contrary to the general <c>IDictionary</c> contract. 
        /// </remarks>
        /// <seealso cref="HashDictionary"/>
        public SynchronizedDictionary()
            : this(new HashDictionary())
        { }

        /// <summary>
        /// Create <c>SynchronizedDictionary</c> instance.
        /// </summary>
        /// <remarks>
        /// This constructor will create a wrapper around the internal
        /// <see cref="HashDictionary"/> instance, which means that the 
        /// created dictionary instance will support <c>null</c> keys,
        /// contrary to the general <c>IDictionary</c> contract. 
        /// </remarks>
        /// <seealso cref="HashDictionary"/>
        /// <param name="capacity">
        /// The initial capacity of the internal <see cref="HashDictionary"/>.
        /// </param>
        public SynchronizedDictionary(int capacity)
            : this(new HashDictionary(capacity))
        { }

        /// <summary>
        /// specified dictionary.
        /// </summary>
        /// <param name="dict">
        /// Dictionary to wrap.
        /// </param>
        public SynchronizedDictionary(IDictionary dict)
        {
            m_dict = dict;
            m_gate = GateFactory.NewGate;
        }

        /// <summary>
        /// Initializes a new instance of the <c>SynchronizedDictionary</c> 
        /// class using the specified 
        /// <see cref="T:System.Runtime.Serialization.SerializationInfo"/> 
        /// and <see cref="T:System.Runtime.Serialization.StreamingContext"/>.
        /// </summary>
        /// <param name="info">
        /// A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> 
        /// object containing the information required to initialize this 
        /// <c>SynchronizedDictionary</c> instance.
        /// </param>
        /// <param name="context">
        /// A <see cref="T:System.Runtime.Serialization.StreamingContext"/> 
        /// object containing the source and destination of the serialized 
        /// stream associated with this dictionary. 
        /// </param>
        protected SynchronizedDictionary(SerializationInfo info, 
                                         StreamingContext context)
            : this((IDictionary) info.GetValue("dict", typeof(IDictionary)))
        {}

        #endregion

        #region Public methods

        /// <summary>
        /// Acquire a read lock.
        /// </summary>
        /// <remarks>
        /// This method will block until the read lock is acquired.
        /// <p/>
        /// Multiple threads can hold read locks at the same time, but no
        /// thread will be able to acquire a write lock until all read locks
        /// are released.
        /// <p/>
        /// This method should always be used in combination with a
        /// <see cref="ReleaseReadLock"/> method in the following manner:
        /// <code>
        /// dict.AcquireReadLock();
        /// try
        /// {
        ///     // access dictionary
        /// }
        /// finally
        /// {
        ///     dict.ReleaseReadLock();
        /// }
        /// </code>
        /// This will ensure that the lock is released properly even if an
        /// exception is thrown by the code within the <c>try</c> block.
        /// </remarks>
        /// <seealso cref="AcquireReadLock(int)"/>
        /// <seealso cref="ReleaseReadLock"/>
        public void AcquireReadLock()
        {
            AcquireReadLock(-1);
        }

        /// <summary>
        /// Acquire a read lock.
        /// </summary>
        /// <remarks>
        /// This method will attempt to acquire a read lock for up to
        /// <paramref name="timeout"/> milliseconds, and will return a boolean 
        /// value specifying whether or not the lock was acquired successfully.
        /// <p/>
        /// Multiple threads can hold read locks at the same time, but no
        /// thread will be able to acquire a write lock until all read locks
        /// are released.
        /// <p/>
        /// This method should always be used in combination with a
        /// <see cref="ReleaseReadLock"/> method in the following manner:
        /// <code>
        /// if (dict.AcquireReadLock(timeout))
        /// {
        ///     try
        ///     {
        ///         // access dictionary
        ///     }
        ///     finally
        ///     {
        ///         dict.ReleaseReadLock();
        ///     }
        /// }
        /// </code>
        /// This will ensure that the dictionary is not accessed unless the 
        /// lock was acquired successfully, and that the lock is released 
        /// properly even if an exception is thrown by the code within the 
        /// <c>try</c> block. 
        /// <p/>
        /// It is entirely up to you how to handle the case when the 
        /// <c>AcquireReadLock</c> method returns <c>false</c>. For example,
        /// you can ignore the fact, throw an exception, or retry the 
        /// operation by placing the code above within a loop.
        /// </remarks>
        /// <param name="timeout">
        /// Timeout in milliseconds.
        /// </param>
        /// <returns>
        /// <c>true</c> if a lock was acquired within the specified time,
        /// <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="AcquireReadLock()"/>
        /// <seealso cref="ReleaseReadLock"/>
        public bool AcquireReadLock(int timeout)
        {
            return m_gate.Enter(timeout);
        }

        /// <summary>
        /// Release a read lock.
        /// </summary>
        /// <seealso cref="AcquireReadLock()"/>
        /// <seealso cref="AcquireReadLock(int)"/>
        public void ReleaseReadLock()
        {
            m_gate.Exit();
        }

        /// <summary>
        /// Acquire a write lock.
        /// </summary>
        /// <remarks>
        /// This method will block until the write lock is acquired.
        /// <p/>
        /// Only a single thread can hold the write lock at any given time, 
        /// and no other threads will be able to acquire either a read lock
        /// or a write lock until the write lock is released.
        /// <p/>
        /// This method should always be used in combination with a
        /// <see cref="ReleaseWriteLock"/> method in the following manner:
        /// <code>
        /// dict.AcquireWriteLock();
        /// try
        /// {
        ///     // access dictionary
        /// }
        /// finally
        /// {
        ///     dict.ReleaseWriteLock();
        /// }
        /// </code>
        /// This will ensure that the lock is released properly even if an
        /// exception is thrown by the code within the <c>try</c> block.
        /// </remarks>
        /// <seealso cref="AcquireWriteLock(int)"/>
        /// <seealso cref="ReleaseWriteLock"/>
        public void AcquireWriteLock()
        {
            AcquireWriteLock(-1);
        }

        /// <summary>
        /// Acquire a write lock.
        /// </summary>
        /// <remarks>
        /// This method will attempt to acquire a write lock for up to
        /// <paramref name="timeout"/> milliseconds, and will return a boolean 
        /// value specifying whether or not the lock was acquired successfully.
        /// <p/>
        /// Only a single thread can hold the write lock at any given time, 
        /// and no other threads will be able to acquire either a read lock
        /// or a write lock until the write lock is released.
        /// <p/>
        /// This method should always be used in combination with a
        /// <see cref="ReleaseWriteLock"/> method in the following manner:
        /// <code>
        /// if (dict.AcquireWriteLock(timeout))
        /// {
        ///     try
        ///     {
        ///         // access dictionary
        ///     }
        ///     finally
        ///     {
        ///         dict.ReleaseWriteLock();
        ///     }
        /// }
        /// </code>
        /// This will ensure that the dictionary is not accessed unless the 
        /// lock was acquired successfully, and that the lock is released 
        /// properly even if an exception is thrown by the code within the 
        /// <c>try</c> block. 
        /// <p/>
        /// It is entirely up to you how to handle the case when the 
        /// <c>AcquireWriteLock</c> method returns <c>false</c>. For example,
        /// you can ignore the fact, throw an exception, or retry the 
        /// operation by placing the code above within a loop.
        /// </remarks>
        /// <param name="timeout">
        /// Timeout in milliseconds.
        /// </param>
        /// <returns>
        /// <c>true</c> if a lock was acquired within the specified time,
        /// <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="AcquireWriteLock()"/>
        /// <seealso cref="ReleaseWriteLock"/>
        public bool AcquireWriteLock(int timeout)
        {
            Blocking.Enter(SyncRoot);
            bool acquired = false;
            try
            {
                acquired = m_gate.Close(timeout);
            }
            finally
            {
                if (!acquired)
                {
                    Monitor.Exit(SyncRoot);
                }
            }
            return acquired;
        }

        /// <summary>
        /// Release a write lock.
        /// </summary>
        /// <seealso cref="AcquireWriteLock()"/>
        /// <seealso cref="AcquireWriteLock(int)"/>
        public void ReleaseWriteLock()
        {
            m_gate.Open();
            Monitor.Exit(SyncRoot);
        }

        #endregion

        #region Implementation of IDictionary

        /// <summary>
        /// Add an entry with the specified key and value to this dictionary.
        /// </summary>
        /// <param name="key">
        /// Entry key.
        /// </param>
        /// <param name="value">
        /// Entry value.
        /// </param>
        public virtual void Add(object key, object value)
        {
            AcquireWriteLock();
            try
            {
                m_dict.Add(key, value);
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Remove all entries from this dictionary.
        /// </summary>
        public virtual void Clear()
        {
            AcquireWriteLock();
            try
            {
                m_dict.Clear();
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Determine whether this dictionary contains the specified key.
        /// </summary>
        /// <param name="key">
        /// Key to search for.
        /// </param>
        /// <returns>
        /// <b>true</b> if this dictionary contains the specified key.
        /// </returns>
        public virtual bool Contains(object key)
        {
            AcquireReadLock();
            try
            {
                return m_dict.Contains(key);
            }
            finally
            {
                ReleaseReadLock();
            }
        }

        /// <summary>
        /// Return an <b>IDictionaryEnumerator</b> that iterates through this
        /// dictionary.
        /// </summary>
        /// <returns>
        /// An <b>IDictionaryEnumerator</b> that iterates through this
        /// dictionary.
        /// </returns>
        public virtual IDictionaryEnumerator GetEnumerator()
        {
            AcquireReadLock();
            try
            {
                return m_dict.GetEnumerator();
            }
            finally
            {
                ReleaseReadLock();
            }
        }

        /// <summary>
        /// Remove the entrty with the specified key from this dictionary.
        /// </summary>
        /// <param name="key">
        /// Key that determines the entry to remove.
        /// </param>
        public virtual void Remove(object key)
        {
            AcquireWriteLock();
            try
            {
                m_dict.Remove(key);
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Get or set the value associated with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key whose value to get or set.
        /// </param>
        /// <value>
        /// The value associated with the specified key.
        /// </value>
        public virtual object this[object key]
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_dict[key];
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
            set
            {
                AcquireWriteLock();
                try
                {
                    m_dict[key] = value;
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Get a collection containing the keys in this dictionary.
        /// </summary>
        /// <value>
        /// A collection of the keys in this dictionary.
        /// </value>
        public virtual ICollection Keys
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_dict.Keys;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
        }

        /// <summary>
        /// Get a collection containing the values in this dictionary.
        /// </summary>
        /// <value>
        /// A collection of the values in this dictionary.
        /// </value>
        public virtual ICollection Values
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_dict.Values;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
        }

        /// <summary>
        /// Get a value indicating whether this dictionary is read-only.
        /// </summary>
        /// <value>
        /// <b>true</b> if this dictionary is read-only,
        /// <b>false</b> otherwise.
        /// </value>
        public virtual bool IsReadOnly
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_dict.IsReadOnly;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
        }

        /// <summary>
        /// Get a value indicating whether this dictionary has a fixed size.
        /// </summary>
        /// <value>
        /// <b>true</b> if this dictionary has a fixed size,
        /// <b>false</b> otherwise.
        /// </value>
        public virtual bool IsFixedSize
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_dict.IsFixedSize;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        /// Return an <b>IEnumerator</b> that iterates through this dictionary.
        /// </summary>
        /// <returns>
        /// An <b>IEnumerator</b> that iterates through this dictionary.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection

        /// <summary>
        /// Copy entries from this dictionary into the one-dimensional array.
        /// </summary>
        /// <param name="array">
        /// An array to which entries should be copied.
        /// </param>
        /// <param name="arrayIndex">
        /// Index in <paramref name="array"/> at which copying should start.
        /// </param>
        public virtual void CopyTo(Array array, int arrayIndex)
        {
            AcquireReadLock();
            try
            {
                m_dict.CopyTo(array, arrayIndex);
            }
            finally
            {
                ReleaseReadLock();
            }
        }

        /// <summary>
        /// Gets the number of key/value pairs in this dictionary.
        /// </summary>
        /// <value>
        /// The number of key/value pairs in this dictionary.
        /// </value>
        public virtual int Count
        {
            get
            {
                AcquireReadLock();
                try
                {
                    return m_dict.Count;
                }
                finally
                {
                    ReleaseReadLock();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to this dictionary
        /// is thread-safe.
        /// </summary>
        /// <value>
        /// Always <b>true</b>.
        /// </value>
        public virtual bool IsSynchronized
        {
            get { return true; }
        }

        /// <summary>
        /// Get an object that can be used to synchronize access to this 
        /// dictionary.
        /// </summary>
        /// <remarks>
        /// This property is used internally to synchronize mutating 
        /// operations on this dictionary. 
        /// <p/>
        /// You can use it externally to block mutating operations as well, 
        /// but keep in mind that simply locking this property will not 
        /// prevent concurrent read operations. If you need to block both read 
        /// and write operations, use <see cref="AcquireWriteLock()"/> method
        /// instead.
        /// </remarks>
        /// <value>
        /// An object that is used to synchronize access to this dictionary.
        /// </value>
        /// <seealso cref="AcquireReadLock()"/>
        /// <seealso cref="AcquireWriteLock()"/>
        public virtual object SyncRoot
        {
            get { return m_dict.SyncRoot; }
        }

        #endregion

        #region Implementation of ISerializable

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

        #region Data members

        /// <summary>
        /// Wrapped, non-thread safe dictionary.
        /// </summary>
        protected readonly IDictionary m_dict;

        /// <summary>
        /// Gate used to synchronize access to this cache.
        /// </summary>
        [NonSerialized]
        private Gate m_gate;

        #endregion
    }
}