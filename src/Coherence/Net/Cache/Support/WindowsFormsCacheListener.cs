/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Windows.Forms;

using Tangosol.Net.Cache;

namespace Tangosol.Net.Cache.Support
{
    /// <summary>
    /// Ensures that any event handling code that needs to run
    /// as a response to a cache event is executed on the UI thread.
    /// </summary>
    /// <remarks>
    /// The crucial limiting factor when updating UI elements from a
    /// background thread is the thread affinity Windows Forms controls
    /// and forms have to the underlying thread that created them.
    /// <p/>
    /// All Windows messages are actually messages between threads,
    /// and each thread has its own message queue. Each and every thread
    /// message can only be processed on the thread it belongs to.
    /// When a thread creates a window, that window's messages are actually
    /// messages destined for the message queue of the creating thread.
    /// Consequently, all windows (such as forms and controls) can only
    /// process messages on the thread that created them. Method calls on
    /// forms and controls often result internally with posting of at least
    /// one such message.
    /// <p/>
    /// This class allows end users to ignore this fact and handle Coherence
    /// cache events, which are always raised from a background thread, as
    /// if they were raised within the UI thread. This class will ensure that
    /// the call is properly marshalled and executed on the UI thread.
    /// </remarks>
    /// <author>Aleksandar Seovic  2006.11.01</author>
    public class WindowsFormsCacheListener : DelegatingCacheListener, ICacheListener
    {
        private delegate void CacheEventCallback(CacheEventArgs args);
        private Control control;

        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        /// <param name="control">
        /// Control that should be used to determine UI thread
        /// and marshal events appropriately.
        /// </param>
        public WindowsFormsCacheListener(Control control)
        {
            this.control = control;
        }

        /// <summary>
        /// Invoked when a dictionary entry has been inserted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the insert
        /// information.
        /// </param>
        void ICacheListener.EntryInserted(CacheEventArgs evt)
        {
            CacheEventCallback callback = new CacheEventCallback(OnInserted);
            control.BeginInvoke(callback, new object[] {evt});
        }

        /// <summary>
        /// Invoked when a dictionary entry has been updated.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the update
        /// information.
        /// </param>
        void ICacheListener.EntryUpdated(CacheEventArgs evt)
        {
            CacheEventCallback callback = new CacheEventCallback(OnUpdated);
            control.BeginInvoke(callback, new object[] {evt});
        }

        /// <summary>
        /// Invoked when a dictionary entry has been deleted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the remove
        /// information.
        /// </param>
        void ICacheListener.EntryDeleted(CacheEventArgs evt)
        {
            CacheEventCallback callback = new CacheEventCallback(OnDeleted);
            control.BeginInvoke(callback, new object[] {evt});
        }
    }
}