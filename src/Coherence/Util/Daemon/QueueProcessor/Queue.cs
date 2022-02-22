/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Threading;

namespace Tangosol.Util.Daemon.QueueProcessor
{
    /// <summary>
    /// The Queue provides a means to efficiently (and in a thread-safe
    /// manner) queue received messages and messages to be sent.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.08.23</author>
    public class Queue
    {
        #region Properties

        /// <summary>
        /// Gets or sets the List that backs the Queue.
        /// </summary>
        /// <remarks>
        /// Subclasses are allowed to change the value of ElementList over
        /// time, and this property is accessed in unsynchronized methods,
        /// thus it is volatile.
        /// </remarks>
        protected virtual IList ElementList
        {
            get { return m_elementList; }
            set { m_elementList = value; }
        }

        /// <summary>
        /// Returns the number of objects in the Queue.
        /// </summary>
        /// <value>
        /// Number of objects in the Queue.
        /// </value>
        public virtual int Count
        {
            get { return ElementList.Count; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for Queue object.
        /// </summary>
        public Queue()
        {
            ElementList = ArrayList.Synchronized(new ArrayList());
        }

        #endregion

        #region Queue implementation

        /// <summary>
        /// Appends the specified element to the end of this queue.
        /// </summary>
        /// <remarks>
        /// Queues may place limitations on what elements may be added
        /// to this Queue. In particular, some Queues will impose restrictions
        /// on the type of elements that may be added. Queue implementations
        /// should clearly specify in their documentation any restrictions on
        /// what elements may be added.
        /// </remarks>
        /// <param name="obj">
        /// Element to be appended to this Queue.
        /// </param>
        /// <returns>
        /// <b>true</b> (as per the general contract of the IList.Add method)
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// If the class of the specified element prevents it from being added
        /// to this Queue.
        /// </exception>
        public virtual bool Add(object obj)
        {
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                ElementList.Add(obj);

                // this Queue reference is waited on by the blocking remove
                // method; use notify to wake up a thread waiting
                // to remove
                Monitor.Pulse(this);

                return true;
            }
        }

        /// <summary>
        /// Inserts  the specified element to the front of this queue.
        /// </summary>
        /// <seealso cref="Add"/>
        public virtual bool AddHead(object obj)
        {
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                ElementList.Insert(0, obj);

                Monitor.Pulse(this);

                return true;
            }
        }

        /// <summary>
        /// Determine the number of elements in this Queue.
        /// </summary>
        /// <remarks>
        /// The size of the Queue may change after the size is returned from
        /// this method, unless the Queue is synchronized on before reading
        /// Count and the monitor is held until the operation based on this
        /// size result is complete.
        /// </remarks>
        /// <returns>
        /// <b>true</b> if Queue has no elements in it; otherwise it
        /// returns <b>false</b>.
        /// </returns>
        public virtual bool IsEmpty()
        {
            return ElementList.Count == 0;
        }

        /// <summary>
        /// Provides an Enumerator over the elements in this Queue.
        /// </summary>
        /// The enumerator is a point-in-time snapshot, and the contents
        /// of the Queue may change after the enumerator is returned, unless
        /// the Queue is synchronized on before calling this method and until
        /// the enumerator is exhausted.
        /// <returns>
        /// An enumerator of the elements in this Queue.
        /// </returns>
        public virtual IEnumerator GetEnumerator()
        {
            return ElementList.GetEnumerator();
        }

        /// <summary>
        /// Returns the first element from the front of this Queue.
        /// </summary>
        /// <remarks>
        /// If the Queue is empty, no element is returned. There is no
        /// blocking equivalent of this method as it would require
        /// notification to wake up from an empty Queue, and this would
        /// mean that the "Add" and "AddHead" methods would need to
        /// perform PulseAll over Pulse which has performance implications.
        /// </remarks>
        /// <returns>
        /// The first element in the front of this Queue or <c>null</c> if
        /// the Queue is empty.
        /// </returns>
        /// <seealso cref="Remove"/>
        public virtual object PeekNoWait()
        {
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                IList list = ElementList;
                return list.Count == 0 ? null : list[0];
            }
        }

        /// <summary>
        /// Waits for and removes the first element from the front of this
        /// Queue.
        /// </summary>
        /// <remarks>
        /// If the Queue is empty, this method will block until an element
        /// is in the Queue. The unblocking equivalent of this method is
        /// "RemoveNoWait".
        /// </remarks>
        /// <returns>
        /// The first element in the front of this Queue.
        /// </returns>
        /// <seealso cref="RemoveNoWait"/>
        public virtual object Remove()
        {
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                IList list = ElementList;
                while (list.Count == 0)
                {
                    Blocking.Wait(this);
                }
                object obj = list[0];
                list.RemoveAt(0);
                return obj;
            }
        }

        /// <summary>
        /// Removes and returns the first element from the front of this Queue.
        /// </summary>
        /// <remarks>
        /// If the Queue is empty, no element is returned.
        /// <p/>
        /// The blocking equivalent of this method is "Remove".
        /// </remarks>
        /// <returns>
        /// The first element in the front of this Queue or null if
        /// the Queue is empty.
        /// </returns>
        /// <seealso cref="Remove"/>
        public virtual object RemoveNoWait()
        {
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                IList list = ElementList;
                if (list.Count == 0)
                {
                    return null;
                }
                else
                {
                    object obj = list[0];
                    list.RemoveAt(0);
                    return obj;
                }
            }
        }

        /// <summary>
        /// Flush the queue.
        /// </summary>
        public virtual void Flush()
        {
            using (BlockingLock l = BlockingLock.Lock(this))
            {
                Monitor.Pulse(this);
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The List that backs the Queue.
        /// </summary>
        private volatile IList m_elementList;

        #endregion
    }
}