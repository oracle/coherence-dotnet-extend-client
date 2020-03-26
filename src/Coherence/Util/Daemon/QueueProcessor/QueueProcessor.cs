/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Util.Daemon.QueueProcessor
{
    /// <summary>
    /// This is a <see cref="Daemon"/> component that waits for items to
    /// process from a <see cref="Queue"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Whenever the <b>Queue</b> contains an item, the
    /// <see cref="Daemon.OnNotify"/> event occurs. It is expected that
    /// sub-classes will process OnNotify as follows:
    /// <pre>
    /// Object o;
    /// while ((o = Queue.RemoveNoWait()) != null)
    /// {
    /// // process the item
    /// // ...
    /// }
    /// </pre></p>
    /// <p>
    /// The <b>Queue</b> is used as the synchronization point for the daemon.
    /// </p>
    /// </remarks>
    /// <author>Goran Milosavljevic  2006.08.24</author>
    public class QueueProcessor : Daemon
    {
        #region Properties

        /// <summary>
        /// This is the <b>Queue</b> to which items that need to be processed
        /// are added, and from which the daemon pulls items to process.
        /// </summary>
        /// <value>
        /// A <see cref="Queue"/> object.
        /// </value>
        public virtual Queue Queue
        {
            get
            {
                Queue queue = m_queue;
                if (queue == null)
                {
                    queue = m_queue = new Queue();
                }
                return queue;
            }
            set { m_queue = value; }
        }

        /// <summary>
        /// Specifes whether there is work for the daemon to do; if there is
        /// work, IsNotification must evaluate to <b>true</b>, and if there
        /// is no work (implying that the daemon should wait for work) then
        /// IsNotification must evaluate to <b>false</b>.
        /// </summary>
        /// <remarks>
        /// To verify that a wait is necessary, the monitor on the Lock
        /// property is first obtained and then IsNotification is evaluated;
        /// only if IsNotification evaluates to <b>false</b> will the daemon
        /// go into a wait state on the Lock property.
        /// <p/>
        /// To unblock (notify) the daemon, another thread should set
        /// IsNotification to true.
        /// </remarks>
        public override bool IsNotification
        {
            get { return !Queue.IsEmpty(); }
        }

        #endregion

        #region Contructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public QueueProcessor()
        {
            Lock = Queue;
        }

        #endregion

        #region Data members

        /// <summary>
        /// This is the Queue to which items that need to be processed are
        /// added, and from which the daemon pulls items to process.
        /// </summary>
        private Queue m_queue;

        #endregion
    }
}