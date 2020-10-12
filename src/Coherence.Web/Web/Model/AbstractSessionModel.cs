/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.SessionState;

using Tangosol.IO;
using Tangosol.Util;

namespace Tangosol.Web.Model
{
    /// <summary>
    /// Abstract base class for session item collections.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.10.06</author>
    public abstract class AbstractSessionModel 
        : NameObjectCollectionBase, ISessionModel
    {
        #region Constructors

        /// <summary>
        /// Construct session model.
        /// </summary>
        /// <param name="model">Manager for this model.</param>
        protected AbstractSessionModel(AbstractSessionModelManager model) 
            : base(CASE_INSENSITIVE_COMPARER)
        {
            m_manager = model;
            m_gate    = GateFactory.NewGate;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the manager for this model.
        /// </summary>
        public virtual AbstractSessionModelManager ModelManager
        {
            get { return m_manager; }
        }

        /// <summary>
        /// Get item serializer.
        /// </summary>
        /// <value>
        /// Item serializer.
        /// </value>
        public virtual ISerializer Serializer
        {
            get { return m_manager.Serializer; }
        }

        #endregion

        #region ISessionStateItemCollection implementation

        /// <summary>
        /// Gets or sets a value indicating whether the collection has been 
        /// marked as changed.
        /// </summary>
        /// <returns>
        /// true if the contents have been changed; otherwise, false.
        /// </returns>
        public virtual bool Dirty
        {
            get
            {
                return m_dirty;
            }
            set
            {
                m_dirty = value;
            }
        }

        /// <summary>
        /// Deletes an item from the collection.
        /// </summary>
        /// <param name="name">
        /// The name of the item to delete from the collection.
        /// </param>
        public virtual void Remove(string name)
        {
            AcquireWriteLock();
            try
            {
                SetDirty();
                BaseRemove(name);
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Deletes an item at a specified index from the collection.
        /// </summary>
        /// <param name="index">
        /// The index of the item to remove from the collection.
        /// </param>
        public virtual void RemoveAt(int index)
        {
            AcquireWriteLock();
            try
            {
                SetDirty();
                BaseRemoveAt(index);
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Removes all values and keys from the session-state collection.
        /// </summary>
        public virtual void Clear()
        {
            AcquireWriteLock();
            try
            {
                SetDirty();
                BaseClear();
            }
            finally
            {
                ReleaseWriteLock();
            }
        }

        /// <summary>
        /// Gets or sets a value in the collection by name.
        /// </summary>
        /// <returns>
        /// The value in the collection with the specified name.
        /// </returns>
        /// <param name="name">
        /// The key name of the value in the collection.
        /// </param>
        object ISessionStateItemCollection.this[string name]
        {
            get
            {
                AcquireReadLock();
                try
                {
                    object value = BaseGet(name);
                    if (!ObjectUtils.IsImmutable(value))
                    {
                        SetDirty();
                    }
                    return value;
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
                    SetDirty();
                    BaseSet(name, value);
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value in the collection by numerical index.
        /// </summary>
        /// <returns>
        /// The value in the collection stored at the specified index.
        /// </returns>
        /// <param name="index">
        /// The numerical index of the value in the collection.
        /// </param>
        object ISessionStateItemCollection.this[int index]
        {
            get
            {
                AcquireReadLock();
                try
                {
                    object value = BaseGet(index);
                    if (!ObjectUtils.IsImmutable(value))
                    {
                        SetDirty();
                    }
                    return value;
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
                    SetDirty();
                    BaseSet(index, value);
                }
                finally
                {
                    ReleaseWriteLock();
                }
            }
        }

        #endregion

        #region ISessionModel implementation

        /// <summary>
        /// Gets or sets session key.
        /// </summary>
        /// <value>
        /// Session key.
        /// </value>
        public virtual SessionKey SessionId { get; set; }

        /// <summary>
        /// Deserializes model using specified reader.
        /// </summary>
        /// <param name="reader">Reader to use.</param>
        public abstract void ReadExternal(DataReader reader);

        /// <summary>
        /// Serializes model using specified writer.
        /// </summary>
        /// <param name="writer">Writer to use.</param>
        public abstract void WriteExternal(DataWriter writer);

        #endregion

        #region Synchronization methods

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
        /// model.AcquireReadLock();
        /// try
        /// {
        ///     // access model
        /// }
        /// finally
        /// {
        ///     model.ReleaseReadLock();
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
        /// if (model.AcquireReadLock(timeout))
        /// {
        ///     try
        ///     {
        ///         // access model
        ///     }
        ///     finally
        ///     {
        ///         model.ReleaseReadLock();
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
        /// model.AcquireWriteLock();
        /// try
        /// {
        ///     // access model
        /// }
        /// finally
        /// {
        ///     model.ReleaseWriteLock();
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
        /// if (model.AcquireWriteLock(timeout))
        /// {
        ///     try
        ///     {
        ///         // access model
        ///     }
        ///     finally
        ///     {
        ///         model.ReleaseWriteLock();
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
            return m_gate.Close(timeout);
        }

        /// <summary>
        /// Release a write lock.
        /// </summary>
        /// <seealso cref="AcquireWriteLock()"/>
        /// <seealso cref="AcquireWriteLock(int)"/>
        public void ReleaseWriteLock()
        {
            m_gate.Open();
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Set dirty flag.
        /// </summary>
        protected virtual void SetDirty()
        {
            m_dirty = true;
        }

        #endregion

        #region Data members

        /// <summary>
        /// Case insensitive comparer to use for key comparison.
        /// </summary>
        private static readonly StringComparer CASE_INSENSITIVE_COMPARER =
            StringComparer.Create(CultureInfo.InvariantCulture, true);

        /// <summary>
        /// Manager for this model.
        /// </summary>
        private readonly AbstractSessionModelManager m_manager;

        /// <summary>
        /// Gate used to synchronize access to this object.
        /// </summary>
        private readonly Gate m_gate;

        /// <summary>
        /// Dirty flag.
        /// </summary>
        private volatile bool m_dirty;


        #endregion
    }
}