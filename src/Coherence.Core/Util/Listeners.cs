/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tangosol.Net.Cache;
using Tangosol.Util;

namespace Tangosol.Util
{
    /// <summary>
    /// Provide a simple, efficient, and thread-safe implementation of a list
    /// of event listeners.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The implementation is optimized based on the assumption that
    /// listeners are added and removed relatively rarely, and that the list
    /// of listeners is requested relatively often.</p>
    /// <p>
    /// Thread safety is implemented by synchronizing on all methods that
    /// modify any data member of the class. Read-only methods are not
    /// synchronized.</p>
    /// </remarks>
    /// <author>Cameron Purdy  1997.16.11</author>
    /// <author>Ivan Cikic  2006.09.11</author>
    [Serializable]
    public class Listeners
    {
        #region Properties

        /// <summary>
        /// Get the list of listeners.
        /// </summary>
        /// <remarks>
        /// The contents of this array are immutable; to add or remove
        /// listeners, a copy of this array is made and the new array
        /// replaces the old.
        /// </remarks>
        /// <value>
        /// The list of listeners.
        /// </value>
        public virtual ICacheListener[] ListenersArray
        {
            get { 
                ICacheListener[] syncList  = SynchronousListeners;
                ICacheListener[] asyncList = AsynchronousListeners;
                int              syncSize  = syncList.Length;
                int              asyncSize = asyncList.Length;

                // check common cases
                if (syncSize == 0)
                {
                    return asyncList;
                }
                if (asyncSize == 0)
                {
                    return syncList;
                }

                // concatenate lists
                ICacheListener[] concatList = new ICacheListener[syncSize + asyncSize];
                Array.Copy(syncList, concatList, syncSize);
                Array.Copy(asyncList, 0, concatList, syncSize, asyncSize);
                return concatList;
            }
        }

        /// <summary>
        /// Get the filters for this listener.
        /// </summary>
        /// <value>
        /// The list of filters.
        /// </value>
        /// <since>Coherence 3.7.1.8</since>
        public virtual IFilter[] FiltersArray
        {
            get { return m_filters; }
            set { m_filters = value; }
        }

        /// <summary>
        /// Check if there are no listeners.
        /// </summary>
        /// <value>
        /// <b>true</b> if there are no listeners.
        /// </value>>
        public virtual bool IsEmpty
        {
            get { return SynchronousListeners.Length == 0 && AsynchronousListeners.Length == 0; }
        }

        /// <summary>
        /// Get the list of asynchronous listeners.
        /// </summary>
        /// <remarks>
        /// The contents of this array are immutable; to add or remove
        /// listeners, a copy of this array is made and the new array
        /// replaces the old.
        /// </remarks>
        /// <value>
        /// The list of asynchronous listeners.
        /// </value>
        private ICacheListener[] AsynchronousListeners
        {
            get { return m_asyncListeners; }
            set { m_asyncListeners = value; }
        }

        /// <summary>
        /// Get the list of synchronous listeners.
        /// </summary>
        /// <remarks>
        /// The contents of this array are immutable; to add or remove
        /// listeners, a copy of this array is made and the new array
        /// replaces the old.
        /// </remarks>
        /// <value>
        /// The list of synchronous listeners.
        /// </value>
        private ICacheListener[] SynchronousListeners
        {
            get { return m_syncListeners; }
            set { m_syncListeners = value; }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Add a listener.
        /// </summary>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to add.
        /// </param>
        public virtual void Add(ICacheListener listener)
        {
            lock (this)
            {
                // Swing (Kestrel) will add/remove null listeners
                if (listener == null)
                {
                    return;
                }

                if (!Contains(listener))
                {
                    ICacheListener[] oldArray = GetListenerListFor(listener);
                    int              count    = oldArray.Length;
                    ICacheListener[] newArray = new ICacheListener[count + 1];

                    Array.Copy(oldArray, newArray, count);
                    newArray[count] = listener;
                    SetListenerListFor(listener, newArray);
                }
            }
        }

        /// <summary>
        /// Remove a listener.
        /// </summary>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to remove.
        /// </param>
        public virtual void Remove(ICacheListener listener)
        {
            lock (this)
            {
                // Swing (Kestrel) will add/remove null listeners
                if (listener == null)
                {
                    return;
                }
                ICacheListener[] oldArray = GetListenerListFor(listener);
                int              count    = oldArray.Length;

                // most common case - exactly one listener
                if (count == 1)
                {
                    if (listener.Equals(oldArray[0]))
                    {
                        SetListenerListFor(listener, BLANKLIST);
                    }
                    return;
                }
                if (count > 0)
                {
                    // locate the listener in the list
                    int index = IndexOf(oldArray, listener);
                    if (index >= 0)
                    {
                        // remove the listener from the list
                        ICacheListener[] newArray = new ICacheListener[count - 1];
                        if (index > 0)
                        {
                            Array.Copy(oldArray, newArray, index);
                        }
                        if (index + 1 < count)
                        {
                            Array.Copy(oldArray, index+1, newArray, index, count-index-1);
                        }
                        SetListenerListFor(listener, newArray);
                    }
                }
            }
        }

        /// <summary>
        /// Add all listeners from another <b>Listeners</b> object.
        /// </summary>
        /// <param name="listeners">
        /// The <b>Listeners</b> to add.
        /// </param>
        public virtual void AddAll(Listeners listeners)
        {
            lock (this)
            {
                if (listeners == null)
                {
                    return;
                }
                AsynchronousListeners = Union(AsynchronousListeners, listeners.AsynchronousListeners);
                SynchronousListeners  = Union(SynchronousListeners,  listeners.SynchronousListeners);
            }
        }

        /// <summary>
        /// Remove all listeners.
        /// </summary>
        public virtual void RemoveAll()
        {
            lock(this)
            {
                AsynchronousListeners = BLANKLIST;
                SynchronousListeners  = BLANKLIST;
            }
        }

        /// <summary>
        /// Check if a listener is in the list of listeners.
        /// </summary>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to search for.
        /// </param>
        /// <returns>
        /// <b>true</b> if the listener is in the list of listeners.
        /// </returns>
        public virtual bool Contains(ICacheListener listener)
        {
            return IndexOf(GetListenerListFor(listener), listener) >= 0;
        }

        /// <summary>
        /// Locate a listener is in the list of listeners.
        /// </summary>
        /// <param name="listenerArray">
        /// The array of listeners to sarch
        /// </param>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to search for.
        /// </param>
        /// <returns>
        /// The index of the listener in the list of listeners.
        /// </returns>
        private int IndexOf(ICacheListener[] listenerArray, ICacheListener listener)
        {
            for (int i = 0, c = listenerArray.Length; i < c; ++i)
            {
                if (listener.Equals(listenerArray[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Return an array containing the union of the specified lists.
        /// </summary>
        /// <param name="listeners1">
        /// Array of listeners
        /// </param>
        /// <param name="listeners2">
        /// Array of listeners
        /// </param>
        /// <returns>
        /// The union of the two arrays of listeners
        /// </returns>
        private static ICacheListener[] Union(ICacheListener[] listeners1, ICacheListener[] listeners2)
        {
            int count1 = listeners1.Length;
            int count2 = listeners2.Length;

            // check the degenerate cases
            if (count1 == 0)
            {
                return listeners2;
            }
            if (count2 == 0)
            {
                return listeners1;
            }

            // remove duplicates
            var setUnion = new HashSet<ICacheListener>();
            for (int i = 0; i < count1; i++)
            {
                setUnion.Add(listeners1[i]);
            }
            for (int i = 0; i < count2; i++)
            {
                setUnion.Add(listeners2[i]);
            }

            // check the cheep cases where one array is a subset of the other
            int count = setUnion.Count;
            if (count == count1)
            {
                return listeners1;
            }
            if (count == count2)
            {
                return listeners2;
            }

            return setUnion.ToArray();
            
        }

        /// <summary>
        /// Return the array of listeners (sync or async) that corresponds to
        /// the specified listener
        /// </summary>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to find an array for
        /// </param>
        /// <returns>
        /// The array of listeners corresponding to the specified listener
        /// </returns>
        private ICacheListener[] GetListenerListFor(ICacheListener listener)
        {
            return (listener is ISynchronousListener) ?
                SynchronousListeners : AsynchronousListeners;
        }

        /// <summary>
        /// Set the array of listeners (sync or async) that corresponds to the
        /// specified listener.  For example, if the specified listener is a
        /// SynchronousListener, set the synchronous listener array to the specified
        /// listener list.
        /// </summary>
        /// <param name="listener">
        /// The <see cref="ICacheListener"/> to ser the array for
        /// </param>
        /// <param name="listenerArray">
        /// The array of listeners
        /// </param>
        private void SetListenerListFor(ICacheListener listener, ICacheListener[] listenerArray)
        {
            if (listener is ISynchronousListener) 
            {
                SynchronousListeners = listenerArray;
            }
            else 
            {
                AsynchronousListeners = listenerArray;
            }
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Return a string representation of the <b>Listeners</b> object.
        /// </summary>
        /// <returns>
        /// A string representation of the <b>Listeners</b> object.
        /// </returns>
        public override string ToString()
        {
            String HEADER    = "Listeners{";
            String DELIMITER = ", ";

            StringBuilder sb = new StringBuilder(HEADER);

            foreach (var listeners in new[] { AsynchronousListeners, SynchronousListeners })
            {
                foreach(var listener in listeners)
                {
                    sb.Append(listener).Append(DELIMITER);
                }
            }
            if (sb.Length > HEADER.Length) sb.Length -= DELIMITER.Length;
            sb.Append('}');

            return sb.ToString();
        }

        #endregion

        #region Constants

        /// <summary>
        /// A blank list of listeners.
        /// </summary>
        private static readonly ICacheListener[] BLANKLIST = new ICacheListener[0];

        #endregion

        #region Data members

        /// <summary>
        /// The registered asynchronous listeners.
        /// </summary>
        private ICacheListener[] m_asyncListeners = BLANKLIST;

        /// <summary>
        /// The registred synchronous listeners.
        /// </summary>
        private ICacheListener[] m_syncListeners  = BLANKLIST;

        /// <summary>
        /// An optional array of filters associated with this Listeners object.
        /// </summary>
        private IFilter[] m_filters;

        #endregion
    }
}